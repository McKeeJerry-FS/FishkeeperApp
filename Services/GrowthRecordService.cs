using System;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class GrowthRecordService : IGrowthRecordService
{
    private readonly ApplicationDbContext _context;

    public GrowthRecordService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<GrowthRecord>> GetGrowthRecordsForLivestockAsync(int livestockId, string userId)
    {
        // Verify the livestock belongs to the user
        var livestock = await _context.Livestock
            .Include(l => l.Tank)
            .FirstOrDefaultAsync(l => l.Id == livestockId && l.Tank!.UserId == userId);

        if (livestock == null)
            return new List<GrowthRecord>();

        return await _context.GrowthRecords
            .Where(gr => gr.LivestockId == livestockId)
            .OrderByDescending(gr => gr.MeasurementDate)
            .ToListAsync();
    }

    public async Task<GrowthRecord?> GetGrowthRecordByIdAsync(int recordId, string userId)
    {
        return await _context.GrowthRecords
            .Include(gr => gr.Livestock)
            .ThenInclude(l => l!.Tank)
            .FirstOrDefaultAsync(gr => gr.Id == recordId && gr.Livestock!.Tank!.UserId == userId);
    }

    public async Task<GrowthRecord> AddGrowthRecordAsync(GrowthRecord record, string userId)
    {
        // Verify the livestock belongs to the user
        var livestock = await _context.Livestock
            .Include(l => l.Tank)
            .FirstOrDefaultAsync(l => l.Id == record.LivestockId && l.Tank!.UserId == userId);

        if (livestock == null)
            throw new UnauthorizedAccessException("Livestock not found or does not belong to user.");

        record.CreatedAt = DateTime.UtcNow;
        _context.GrowthRecords.Add(record);
        await _context.SaveChangesAsync();

        return record;
    }

    public async Task<GrowthRecord?> UpdateGrowthRecordAsync(GrowthRecord record, string userId)
    {
        var existingRecord = await GetGrowthRecordByIdAsync(record.Id, userId);
        if (existingRecord == null)
            return null;

        existingRecord.MeasurementDate = record.MeasurementDate;
        existingRecord.LengthInches = record.LengthInches;
        existingRecord.WeightGrams = record.WeightGrams;
        existingRecord.DiameterInches = record.DiameterInches;
        existingRecord.HeightInches = record.HeightInches;
        existingRecord.HealthCondition = record.HealthCondition;
        existingRecord.ColorVibrancy = record.ColorVibrancy;
        existingRecord.Notes = record.Notes;
        existingRecord.PhotoPath = record.PhotoPath;

        await _context.SaveChangesAsync();
        return existingRecord;
    }

    public async Task<bool> DeleteGrowthRecordAsync(int recordId, string userId)
    {
        var record = await GetGrowthRecordByIdAsync(recordId, userId);
        if (record == null)
            return false;

        _context.GrowthRecords.Remove(record);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<GrowthStatistics?> GetGrowthStatisticsAsync(int livestockId, string userId)
    {
        // Verify the livestock belongs to the user
        var livestock = await _context.Livestock
            .Include(l => l.Tank)
            .FirstOrDefaultAsync(l => l.Id == livestockId && l.Tank!.UserId == userId);

        if (livestock == null)
            return null;

        var records = await GetGrowthRecordsForLivestockAsync(livestockId, userId);
        if (!records.Any())
            return null;

        var orderedRecords = records.OrderBy(r => r.MeasurementDate).ToList();
        var firstRecord = orderedRecords.First();
        var lastRecord = orderedRecords.Last();

        var stats = new GrowthStatistics
        {
            LivestockId = livestockId,
            LivestockName = livestock.Name,
            TotalMeasurements = records.Count,
            FirstMeasurement = firstRecord.MeasurementDate,
            LastMeasurement = lastRecord.MeasurementDate,
            CurrentHealth = lastRecord.HealthCondition
        };

        // Calculate length growth
        if (firstRecord.LengthInches.HasValue && lastRecord.LengthInches.HasValue)
        {
            stats.InitialLength = firstRecord.LengthInches.Value;
            stats.CurrentLength = lastRecord.LengthInches.Value;
            stats.LengthGrowth = stats.CurrentLength.Value - stats.InitialLength.Value;

            if (stats.InitialLength.Value > 0)
            {
                stats.LengthGrowthPercentage = (stats.LengthGrowth.Value / stats.InitialLength.Value) * 100;
            }
        }

        // Calculate weight growth
        if (firstRecord.WeightGrams.HasValue && lastRecord.WeightGrams.HasValue)
        {
            stats.InitialWeight = firstRecord.WeightGrams.Value;
            stats.CurrentWeight = lastRecord.WeightGrams.Value;
            stats.WeightGrowth = stats.CurrentWeight.Value - stats.InitialWeight.Value;

            if (stats.InitialWeight.Value > 0)
            {
                stats.WeightGrowthPercentage = (stats.WeightGrowth.Value / stats.InitialWeight.Value) * 100;
            }
        }

        // Calculate diameter growth (for corals)
        if (firstRecord.DiameterInches.HasValue && lastRecord.DiameterInches.HasValue)
        {
            stats.InitialDiameter = firstRecord.DiameterInches.Value;
            stats.CurrentDiameter = lastRecord.DiameterInches.Value;
            stats.DiameterGrowth = stats.CurrentDiameter.Value - stats.InitialDiameter.Value;
        }

        // Calculate height growth (for corals/plants)
        if (firstRecord.HeightInches.HasValue && lastRecord.HeightInches.HasValue)
        {
            stats.InitialHeight = firstRecord.HeightInches.Value;
            stats.CurrentHeight = lastRecord.HeightInches.Value;
            stats.HeightGrowth = stats.CurrentHeight.Value - stats.InitialHeight.Value;
        }

        // Determine health trend
        if (records.Count >= 2)
        {
            var recentRecords = orderedRecords.TakeLast(3).ToList();
            var healthScores = recentRecords
                .Where(r => !string.IsNullOrEmpty(r.HealthCondition))
                .Select(r => GetHealthScore(r.HealthCondition))
                .ToList();

            if (healthScores.Count >= 2)
            {
                var avgRecentScore = healthScores.TakeLast(2).Average();
                var firstScore = healthScores.First();

                if (avgRecentScore > firstScore + 0.5)
                    stats.HealthTrend = "Improving";
                else if (avgRecentScore < firstScore - 0.5)
                    stats.HealthTrend = "Declining";
                else
                    stats.HealthTrend = "Stable";
            }
        }

        return stats;
    }

    public async Task<List<GrowthRecord>> GetRecentGrowthRecordsAsync(string userId, int count = 10)
    {
        return await _context.GrowthRecords
            .Include(gr => gr.Livestock)
            .ThenInclude(l => l!.Tank)
            .Where(gr => gr.Livestock!.Tank!.UserId == userId)
            .OrderByDescending(gr => gr.MeasurementDate)
            .Take(count)
            .ToListAsync();
    }

    private int GetHealthScore(string? healthCondition)
    {
        return healthCondition?.ToLower() switch
        {
            "excellent" => 4,
            "good" => 3,
            "fair" => 2,
            "poor" => 1,
            _ => 2
        };
    }
}
