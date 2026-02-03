using System;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;
using AquaHub.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class PredictiveReminderService : IPredictiveReminderService
{
    private readonly ApplicationDbContext _context;
    private readonly IReminderService _reminderService;

    public PredictiveReminderService(ApplicationDbContext context, IReminderService reminderService)
    {
        _context = context;
        _reminderService = reminderService;
    }

    public async Task<List<PredictiveReminder>> GetActivePredictionsAsync(string userId)
    {
        return await _context.PredictiveReminders
            .Where(pr => pr.UserId == userId &&
                        !pr.IsAccepted &&
                        !pr.IsDismissed &&
                        pr.ExpiresAt > DateTime.UtcNow)
            .Include(pr => pr.Tank)
            .OrderByDescending(pr => pr.ConfidenceScore)
            .ThenBy(pr => pr.SuggestedDate)
            .ToListAsync();
    }

    public async Task<List<PredictiveReminder>> GetTankPredictionsAsync(int tankId, string userId)
    {
        return await _context.PredictiveReminders
            .Where(pr => pr.TankId == tankId &&
                        pr.UserId == userId &&
                        !pr.IsAccepted &&
                        !pr.IsDismissed &&
                        pr.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(pr => pr.ConfidenceScore)
            .ToListAsync();
    }

    public async Task<List<PredictiveReminder>> GeneratePredictionsAsync(string userId)
    {
        var tanks = await _context.Tanks
            .Where(t => t.UserId == userId)
            .ToListAsync();

        var allPredictions = new List<PredictiveReminder>();

        foreach (var tank in tanks)
        {
            var tankPredictions = await GenerateTankPredictionsAsync(tank.Id, userId);
            allPredictions.AddRange(tankPredictions);
        }

        return allPredictions;
    }

    public async Task<List<PredictiveReminder>> GenerateTankPredictionsAsync(int tankId, string userId)
    {
        var predictions = new List<PredictiveReminder>();

        // Analyze maintenance patterns
        predictions.AddRange(await AnalyzeMaintenancePatternsAsync(tankId, userId));

        // Analyze water test patterns
        predictions.AddRange(await AnalyzeWaterTestPatternsAsync(tankId, userId));

        // Analyze parameter trends
        predictions.AddRange(await AnalyzeParameterTrendsAsync(tankId, userId));

        // Analyze equipment age
        predictions.AddRange(await AnalyzeEquipmentAgeAsync(tankId, userId));

        // Analyze health score trends
        predictions.AddRange(await AnalyzeHealthScoreTrendsAsync(tankId, userId));

        // Save new predictions
        if (predictions.Any())
        {
            _context.PredictiveReminders.AddRange(predictions);
            await _context.SaveChangesAsync();
        }

        return predictions;
    }

    public async Task<Reminder?> AcceptPredictionAsync(int predictionId, string userId)
    {
        var prediction = await _context.PredictiveReminders
            .FirstOrDefaultAsync(pr => pr.Id == predictionId && pr.UserId == userId);

        if (prediction == null || prediction.IsAccepted || prediction.IsDismissed)
            return null;

        // Create a real reminder from the prediction
        var reminder = new Reminder
        {
            UserId = userId,
            TankId = prediction.TankId,
            Title = prediction.Title,
            Description = prediction.Description + $"\n\n✨ Created from AI suggestion: {prediction.Reasoning}",
            Type = prediction.SuggestedType,
            Frequency = DetermineFrequency(prediction.AverageDaysBetween),
            NextDueDate = prediction.SuggestedDate,
            IsActive = true,
            NotificationHoursBefore = 24
        };

        var createdReminder = await _reminderService.CreateReminderAsync(reminder);

        // Mark prediction as accepted
        prediction.IsAccepted = true;
        prediction.AcceptedAt = DateTime.UtcNow;
        prediction.CreatedReminderId = createdReminder.Id;
        await _context.SaveChangesAsync();

        return createdReminder;
    }

    public async Task<bool> DismissPredictionAsync(int predictionId, string userId)
    {
        var prediction = await _context.PredictiveReminders
            .FirstOrDefaultAsync(pr => pr.Id == predictionId && pr.UserId == userId);

        if (prediction == null)
            return false;

        prediction.IsDismissed = true;
        prediction.DismissedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task CleanupOldPredictionsAsync()
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-30);

        var oldPredictions = await _context.PredictiveReminders
            .Where(pr => pr.ExpiresAt < DateTime.UtcNow ||
                        pr.IsDismissed && pr.DismissedAt < cutoffDate ||
                        pr.IsAccepted && pr.AcceptedAt < cutoffDate)
            .ToListAsync();

        if (oldPredictions.Any())
        {
            _context.PredictiveReminders.RemoveRange(oldPredictions);
            await _context.SaveChangesAsync();
        }
    }

    // ========== Pattern Analysis Methods ==========

    private async Task<List<PredictiveReminder>> AnalyzeMaintenancePatternsAsync(int tankId, string userId)
    {
        var predictions = new List<PredictiveReminder>();

        // Get maintenance logs from last 90 days
        var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);
        var maintenanceLogs = await _context.MaintenanceLogs
            .Where(ml => ml.TankId == tankId && ml.Tank!.UserId == userId && ml.Timestamp >= ninetyDaysAgo)
            .OrderBy(ml => ml.Timestamp)
            .ToListAsync();

        if (maintenanceLogs.Count < 3)
            return predictions;

        // Group by maintenance type
        var typeGroups = maintenanceLogs.GroupBy(ml => ml.Type);

        foreach (var group in typeGroups)
        {
            if (group.Count() < 2)
                continue;

            var orderedLogs = group.OrderBy(ml => ml.Timestamp).ToList();
            var intervals = new List<double>();

            for (int i = 1; i < orderedLogs.Count; i++)
            {
                var daysBetween = (orderedLogs[i].Timestamp - orderedLogs[i - 1].Timestamp).TotalDays;
                intervals.Add(daysBetween);
            }

            var avgInterval = intervals.Average();
            var lastMaintenance = orderedLogs.Last().Timestamp;
            var daysSinceLast = (DateTime.UtcNow - lastMaintenance).TotalDays;
            var suggestedDate = lastMaintenance.AddDays(avgInterval);

            // Only suggest if we're approaching the next expected maintenance
            if (suggestedDate <= DateTime.UtcNow.AddDays(7) && suggestedDate >= DateTime.UtcNow)
            {
                var confidence = CalculateConfidence(intervals);

                predictions.Add(new PredictiveReminder
                {
                    UserId = userId,
                    TankId = tankId,
                    Title = $"{group.Key} Due Soon",
                    Description = $"Based on your pattern, {group.Key.ToString().ToLower()} is typically performed every {avgInterval:F1} days.",
                    SuggestedType = MapMaintenanceTypeToReminder(group.Key),
                    SuggestedDate = suggestedDate,
                    ConfidenceScore = confidence,
                    Reasoning = $"You've performed this maintenance {group.Count()} times in the past 90 days, averaging every {avgInterval:F1} days.",
                    Source = PredictionSource.MaintenancePattern,
                    PatternOccurrences = group.Count(),
                    AverageDaysBetween = avgInterval,
                    LastOccurrence = lastMaintenance,
                    GeneratedAt = DateTime.UtcNow,
                    ExpiresAt = suggestedDate.AddDays(14) // Expires 2 weeks after suggested date
                });
            }
        }

        return predictions;
    }

    private async Task<List<PredictiveReminder>> AnalyzeWaterTestPatternsAsync(int tankId, string userId)
    {
        var predictions = new List<PredictiveReminder>();

        // Get water tests from last 90 days
        var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);
        var waterTests = await _context.WaterTests
            .Where(wt => wt.TankId == tankId && wt.Tank!.UserId == userId && wt.Timestamp >= ninetyDaysAgo)
            .OrderBy(wt => wt.Timestamp)
            .ToListAsync();

        if (waterTests.Count < 3)
            return predictions;

        var intervals = new List<double>();
        for (int i = 1; i < waterTests.Count; i++)
        {
            var daysBetween = (waterTests[i].Timestamp - waterTests[i - 1].Timestamp).TotalDays;
            intervals.Add(daysBetween);
        }

        var avgInterval = intervals.Average();
        var lastTest = waterTests.Last().Timestamp;
        var daysSinceLast = (DateTime.UtcNow - lastTest).TotalDays;
        var suggestedDate = lastTest.AddDays(avgInterval);

        // Suggest if it's time for next test
        if (suggestedDate <= DateTime.UtcNow.AddDays(3) && daysSinceLast >= (avgInterval * 0.8))
        {
            var confidence = CalculateConfidence(intervals);

            predictions.Add(new PredictiveReminder
            {
                UserId = userId,
                TankId = tankId,
                Title = "Water Test Recommended",
                Description = $"You typically test your water every {avgInterval:F1} days. It's been {daysSinceLast:F0} days since your last test.",
                SuggestedType = ReminderType.WaterTest,
                SuggestedDate = suggestedDate,
                ConfidenceScore = confidence,
                Reasoning = $"Based on {waterTests.Count} tests in the past 90 days, averaging every {avgInterval:F1} days.",
                Source = PredictionSource.WaterTestPattern,
                PatternOccurrences = waterTests.Count,
                AverageDaysBetween = avgInterval,
                LastOccurrence = lastTest,
                GeneratedAt = DateTime.UtcNow,
                ExpiresAt = suggestedDate.AddDays(7)
            });
        }

        return predictions;
    }

    private async Task<List<PredictiveReminder>> AnalyzeParameterTrendsAsync(int tankId, string userId)
    {
        var predictions = new List<PredictiveReminder>();

        // Get recent water tests
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var recentTests = await _context.WaterTests
            .Where(wt => wt.TankId == tankId && wt.Tank!.UserId == userId && wt.Timestamp >= thirtyDaysAgo)
            .OrderBy(wt => wt.Timestamp)
            .ToListAsync();

        if (recentTests.Count < 3)
            return predictions;

        // Check for rising ammonia trend
        var ammoniaReadings = recentTests.Where(t => t.Ammonia.HasValue).Select(t => t.Ammonia!.Value).ToList();
        if (ammoniaReadings.Count >= 3 && IsIncreasingTrend(ammoniaReadings) && ammoniaReadings.Last() > 0.1)
        {
            predictions.Add(new PredictiveReminder
            {
                UserId = userId,
                TankId = tankId,
                Title = "Water Change Recommended - Rising Ammonia",
                Description = "Your ammonia levels have been trending upward. A water change may help stabilize parameters.",
                SuggestedType = ReminderType.WaterChange,
                SuggestedDate = DateTime.UtcNow.AddDays(1),
                ConfidenceScore = 0.85,
                Reasoning = $"Ammonia has increased from {ammoniaReadings.First():F2} to {ammoniaReadings.Last():F2} ppm over the past 30 days.",
                Source = PredictionSource.ParameterTrend,
                PatternOccurrences = ammoniaReadings.Count,
                AverageDaysBetween = 0,
                LastOccurrence = recentTests.Last().Timestamp,
                GeneratedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
        }

        // Check for rising nitrate trend
        var nitrateReadings = recentTests.Where(t => t.Nitrate.HasValue).Select(t => t.Nitrate!.Value).ToList();
        if (nitrateReadings.Count >= 3 && IsIncreasingTrend(nitrateReadings) && nitrateReadings.Last() > 20)
        {
            predictions.Add(new PredictiveReminder
            {
                UserId = userId,
                TankId = tankId,
                Title = "Water Change Needed - High Nitrates",
                Description = $"Nitrates have reached {nitrateReadings.Last():F1} ppm. Consider a water change to reduce nutrient levels.",
                SuggestedType = ReminderType.WaterChange,
                SuggestedDate = DateTime.UtcNow.AddDays(1),
                ConfidenceScore = 0.90,
                Reasoning = $"Nitrate levels trending upward from {nitrateReadings.First():F1} to {nitrateReadings.Last():F1} ppm.",
                Source = PredictionSource.ParameterTrend,
                PatternOccurrences = nitrateReadings.Count,
                AverageDaysBetween = 0,
                LastOccurrence = recentTests.Last().Timestamp,
                GeneratedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
        }

        return predictions;
    }

    private async Task<List<PredictiveReminder>> AnalyzeEquipmentAgeAsync(int tankId, string userId)
    {
        var predictions = new List<PredictiveReminder>();
        var now = DateTime.UtcNow;

        // Check filters (typically need maintenance every 30-90 days)
        var filters = await _context.Filters
            .Where(f => f.TankId == tankId)
            .ToListAsync();

        foreach (var filter in filters)
        {
            var daysSinceInstall = (now - filter.InstalledOn).TotalDays;

            if (daysSinceInstall >= 60 && daysSinceInstall <= 90)
            {
                predictions.Add(new PredictiveReminder
                {
                    UserId = userId,
                    TankId = tankId,
                    Title = $"Filter Maintenance Due: {filter.Brand} {filter.Model}",
                    Description = "Filter media typically should be checked or replaced every 60-90 days.",
                    SuggestedType = ReminderType.EquipmentCheck,
                    SuggestedDate = now.AddDays(7),
                    ConfidenceScore = 0.75,
                    Reasoning = $"Filter has been installed for {daysSinceInstall:F0} days. Recommended maintenance interval reached.",
                    Source = PredictionSource.EquipmentAge,
                    PatternOccurrences = 1,
                    AverageDaysBetween = 60,
                    LastOccurrence = filter.InstalledOn,
                    GeneratedAt = now,
                    ExpiresAt = now.AddDays(30)
                });
            }
        }

        return predictions;
    }

    private async Task<List<PredictiveReminder>> AnalyzeHealthScoreTrendsAsync(int tankId, string userId)
    {
        var predictions = new List<PredictiveReminder>();

        // This would require storing historical health scores
        // For now, we'll implement a simple check based on current health score
        // In a future enhancement, we could track health score history

        return predictions;
    }

    // ========== Helper Methods ==========

    private double CalculateConfidence(List<double> intervals)
    {
        if (intervals.Count < 2)
            return 0.5;

        var mean = intervals.Average();
        var variance = intervals.Sum(x => Math.Pow(x - mean, 2)) / intervals.Count;
        var stdDev = Math.Sqrt(variance);
        var coefficientOfVariation = stdDev / mean;

        // Lower variation = higher confidence
        // CV < 0.2 = very consistent (0.9+ confidence)
        // CV < 0.5 = moderately consistent (0.7+ confidence)
        // CV > 0.5 = inconsistent (0.5- confidence)
        if (coefficientOfVariation < 0.2)
            return 0.9;
        else if (coefficientOfVariation < 0.5)
            return 0.7;
        else
            return Math.Max(0.5, 1.0 - coefficientOfVariation);
    }

    private bool IsIncreasingTrend(List<double> values)
    {
        if (values.Count < 3)
            return false;

        // Simple linear regression check
        int increases = 0;
        for (int i = 1; i < values.Count; i++)
        {
            if (values[i] > values[i - 1])
                increases++;
        }

        // Consider it a trend if at least 60% of comparisons show increase
        return (double)increases / (values.Count - 1) >= 0.6;
    }

    private ReminderType MapMaintenanceTypeToReminder(MaintenanceType maintenanceType)
    {
        return maintenanceType switch
        {
            MaintenanceType.WaterChange => ReminderType.WaterChange,
            MaintenanceType.FilterCleaning => ReminderType.EquipmentCheck,
            MaintenanceType.GlassCleaning => ReminderType.Cleaning,
            MaintenanceType.SubstrateVacuum => ReminderType.Cleaning,
            MaintenanceType.EquipmentMaintenance => ReminderType.EquipmentCheck,
            _ => ReminderType.Maintenance
        };
    }

    private ReminderFrequency DetermineFrequency(double avgDays)
    {
        return avgDays switch
        {
            <= 1 => ReminderFrequency.Daily,
            <= 7 => ReminderFrequency.Weekly,
            <= 14 => ReminderFrequency.BiWeekly,
            <= 30 => ReminderFrequency.Monthly,
            <= 90 => ReminderFrequency.Quarterly,
            _ => ReminderFrequency.Yearly
        };
    }
}
