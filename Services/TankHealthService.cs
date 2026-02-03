using System;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;
using AquaHub.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class TankHealthService : ITankHealthService
{
    private readonly ApplicationDbContext _context;

    public TankHealthService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TankHealthScore?> GetTankHealthScoreAsync(int tankId, string userId)
    {
        var tank = await _context.Tanks
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Id == tankId && t.UserId == userId);

        if (tank == null)
            return null;

        return await CalculateHealthScoreAsync(tank, userId);
    }

    public async Task<List<TankHealthScore>> GetAllTankHealthScoresAsync(string userId)
    {
        var tanks = await _context.Tanks
            .Where(t => t.UserId == userId)
            .ToListAsync();

        var healthScores = new List<TankHealthScore>();

        foreach (var tank in tanks)
        {
            var score = await CalculateHealthScoreAsync(tank, userId);
            if (score != null)
            {
                healthScores.Add(score);
            }
        }

        return healthScores.OrderByDescending(h => h.OverallScore).ToList();
    }

    public async Task<List<TankHealthScore>> GetHealthScoreHistoryAsync(int tankId, string userId, DateTime? startDate = null)
    {
        // For now, return current score. In future, store historical scores in database
        var currentScore = await GetTankHealthScoreAsync(tankId, userId);
        return currentScore != null ? new List<TankHealthScore> { currentScore } : new List<TankHealthScore>();
    }

    public async Task<TankHealthScore?> RecalculateHealthScoreAsync(int tankId, string userId)
    {
        return await GetTankHealthScoreAsync(tankId, userId);
    }

    private async Task<TankHealthScore> CalculateHealthScoreAsync(Tank tank, string userId)
    {
        var healthScore = new TankHealthScore
        {
            TankId = tank.Id,
            TankName = tank.Name,
            CalculatedAt = DateTime.UtcNow
        };

        // Calculate each category score
        await CalculateWaterQualityScoreAsync(healthScore, tank.Id, userId);
        await CalculateMaintenanceScoreAsync(healthScore, tank.Id, userId);
        await CalculateEquipmentScoreAsync(healthScore, tank.Id, userId);
        await CalculateLivestockScoreAsync(healthScore, tank.Id, userId);
        await CalculateStabilityScoreAsync(healthScore, tank.Id, userId);

        // Calculate overall score (weighted average)
        healthScore.OverallScore = CalculateWeightedScore(healthScore);
        healthScore.Grade = GetHealthGrade(healthScore.OverallScore);

        // Generate recommendations
        GenerateRecommendations(healthScore);

        return healthScore;
    }

    private async Task CalculateWaterQualityScoreAsync(TankHealthScore healthScore, int tankId, string userId)
    {
        var latestTest = await _context.WaterTests
            .Where(wt => wt.TankId == tankId && wt.Tank!.UserId == userId)
            .OrderByDescending(wt => wt.Timestamp)
            .FirstOrDefaultAsync();

        if (latestTest == null)
        {
            healthScore.WaterQualityScore = 50; // No data
            healthScore.WaterQuality.HasRecentTest = false;
            healthScore.Issues.Add(new HealthIssue
            {
                Category = "Water Quality",
                Title = "No water test data",
                Description = "No water tests have been recorded for this tank.",
                Severity = "Warning",
                DetectedAt = DateTime.UtcNow
            });
            return;
        }

        healthScore.LastWaterTest = latestTest.Timestamp;
        healthScore.WaterQuality.DaysSinceLastTest = (DateTime.UtcNow - latestTest.Timestamp).Days;
        healthScore.WaterQuality.HasRecentTest = healthScore.WaterQuality.DaysSinceLastTest <= 7;

        // Check parameters against ideal ranges
        int parametersInRange = 0;
        int parametersOutOfRange = 0;
        int totalParameters = 0;

        // Check pH (ideal: 6.5-8.5 for general aquariums)
        if (latestTest.PH.HasValue)
        {
            totalParameters++;
            if (latestTest.PH.Value >= 6.5 && latestTest.PH.Value <= 8.5)
            {
                parametersInRange++;
            }
            else
            {
                parametersOutOfRange++;
                healthScore.WaterQuality.CriticalParameters.Add("pH");
            }
        }

        // Check Ammonia (ideal: 0 ppm)
        if (latestTest.Ammonia.HasValue)
        {
            totalParameters++;
            if (latestTest.Ammonia.Value <= 0.25)
            {
                parametersInRange++;
            }
            else
            {
                parametersOutOfRange++;
                healthScore.WaterQuality.CriticalParameters.Add("Ammonia");
                if (latestTest.Ammonia.Value > 0.5)
                {
                    healthScore.Issues.Add(new HealthIssue
                    {
                        Category = "Water Quality",
                        Title = "High Ammonia Detected",
                        Description = $"Ammonia level ({latestTest.Ammonia.Value} ppm) is dangerously high.",
                        Severity = "Critical",
                        DetectedAt = DateTime.UtcNow
                    });
                }
            }
        }

        // Check Nitrite (ideal: 0 ppm)
        if (latestTest.Nitrite.HasValue)
        {
            totalParameters++;
            if (latestTest.Nitrite.Value <= 0.25)
            {
                parametersInRange++;
            }
            else
            {
                parametersOutOfRange++;
                healthScore.WaterQuality.CriticalParameters.Add("Nitrite");
            }
        }

        // Check Nitrate (ideal: < 20 ppm)
        if (latestTest.Nitrate.HasValue)
        {
            totalParameters++;
            if (latestTest.Nitrate.Value < 20)
            {
                parametersInRange++;
            }
            else
            {
                parametersOutOfRange++;
                if (latestTest.Nitrate.Value > 40)
                {
                    healthScore.WaterQuality.CriticalParameters.Add("Nitrate");
                }
            }
        }

        // Check Temperature (general range: 72-82°F)
        if (latestTest.Temperature.HasValue)
        {
            totalParameters++;
            if (latestTest.Temperature.Value >= 72 && latestTest.Temperature.Value <= 82)
            {
                parametersInRange++;
            }
            else
            {
                parametersOutOfRange++;
            }
        }

        healthScore.WaterQuality.ParametersInRange = parametersInRange;
        healthScore.WaterQuality.ParametersOutOfRange = parametersOutOfRange;
        healthScore.WaterQuality.TotalParametersTested = totalParameters;

        if (totalParameters > 0)
        {
            healthScore.WaterQuality.ParameterComplianceRate = (double)parametersInRange / totalParameters * 100;
        }

        // Calculate score
        double baseScore = totalParameters > 0 ? (double)parametersInRange / totalParameters * 100 : 50;

        // Penalize for old tests
        if (healthScore.WaterQuality.DaysSinceLastTest > 14)
        {
            baseScore -= 15;
        }
        else if (healthScore.WaterQuality.DaysSinceLastTest > 7)
        {
            baseScore -= 5;
        }

        healthScore.WaterQualityScore = Math.Max(0, Math.Min(100, baseScore));
    }

    private async Task CalculateMaintenanceScoreAsync(TankHealthScore healthScore, int tankId, string userId)
    {
        var lastMaintenance = await _context.MaintenanceLogs
            .Where(ml => ml.TankId == tankId && ml.Tank!.UserId == userId)
            .OrderByDescending(ml => ml.Timestamp)
            .FirstOrDefaultAsync();

        if (lastMaintenance == null)
        {
            healthScore.MaintenanceScore = 50;
            healthScore.Maintenance.IsOverdue = true;
            return;
        }

        healthScore.LastMaintenance = lastMaintenance.Timestamp;
        healthScore.Maintenance.DaysSinceLastMaintenance = (DateTime.UtcNow - lastMaintenance.Timestamp).Days;

        // Count recent maintenance
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var ninetyDaysAgo = DateTime.UtcNow.AddDays(-90);

        healthScore.Maintenance.MaintenanceLogsLast30Days = await _context.MaintenanceLogs
            .Where(ml => ml.TankId == tankId && ml.Tank!.UserId == userId && ml.Timestamp >= thirtyDaysAgo)
            .CountAsync();

        healthScore.Maintenance.MaintenanceLogsLast90Days = await _context.MaintenanceLogs
            .Where(ml => ml.TankId == tankId && ml.Tank!.UserId == userId && ml.Timestamp >= ninetyDaysAgo)
            .CountAsync();

        // Calculate score based on maintenance frequency
        double score = 100;

        // Recommended: at least 1 maintenance per week
        if (healthScore.Maintenance.DaysSinceLastMaintenance > 14)
        {
            score -= 30;
            healthScore.Maintenance.IsOverdue = true;
            healthScore.Issues.Add(new HealthIssue
            {
                Category = "Maintenance",
                Title = "Maintenance Overdue",
                Description = $"Last maintenance was {healthScore.Maintenance.DaysSinceLastMaintenance} days ago.",
                Severity = "Warning",
                DetectedAt = DateTime.UtcNow
            });
        }
        else if (healthScore.Maintenance.DaysSinceLastMaintenance > 7)
        {
            score -= 10;
        }

        // Check frequency
        if (healthScore.Maintenance.MaintenanceLogsLast30Days < 2)
        {
            score -= 15;
        }

        healthScore.Maintenance.MaintenanceFrequencyScore = score;
        healthScore.MaintenanceScore = Math.Max(0, score);
    }

    private async Task CalculateEquipmentScoreAsync(TankHealthScore healthScore, int tankId, string userId)
    {
        // Query all equipment types
        var filters = await _context.Filters.Where(e => e.TankId == tankId).ToListAsync();
        var lights = await _context.Lights.Where(e => e.TankId == tankId).ToListAsync();
        var heaters = await _context.Heaters.Where(e => e.TankId == tankId).ToListAsync();
        var skimmers = await _context.ProteinSkimmers.Where(e => e.TankId == tankId).ToListAsync();

        var equipment = new List<Equipment>();
        equipment.AddRange(filters);
        equipment.AddRange(lights);
        equipment.AddRange(heaters);
        equipment.AddRange(skimmers);

        if (!equipment.Any())
        {
            healthScore.EquipmentScore = 70; // Neutral if no equipment tracked
            return;
        }

        healthScore.Equipment.TotalEquipment = equipment.Count;
        // Since Equipment model doesn't have Status property yet, assume all are working
        healthScore.Equipment.WorkingEquipment = equipment.Count;
        healthScore.Equipment.FailedEquipment = 0;

        if (healthScore.Equipment.TotalEquipment > 0)
        {
            healthScore.Equipment.EquipmentHealthRate =
                (double)healthScore.Equipment.WorkingEquipment / healthScore.Equipment.TotalEquipment * 100;
        }

        double score = healthScore.Equipment.EquipmentHealthRate;

        if (healthScore.Equipment.FailedEquipment > 0)
        {
            healthScore.Issues.Add(new HealthIssue
            {
                Category = "Equipment",
                Title = $"{healthScore.Equipment.FailedEquipment} Equipment Failure(s)",
                Description = "Some equipment is not functioning properly.",
                Severity = "Error",
                DetectedAt = DateTime.UtcNow
            });
        }

        healthScore.EquipmentScore = score;
    }

    private async Task CalculateLivestockScoreAsync(TankHealthScore healthScore, int tankId, string userId)
    {
        var livestock = await _context.Livestock
            .Where(l => l.TankId == tankId && l.Tank!.UserId == userId)
            .ToListAsync();

        healthScore.Livestock.TotalLivestock = livestock.Count;

        if (!livestock.Any())
        {
            healthScore.LivestockScore = 80; // Neutral
            return;
        }

        // Check for recent growth records (indicates monitoring)
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        healthScore.Livestock.RecentGrowthRecords = await _context.GrowthRecords
            .Where(gr => livestock.Select(l => l.Id).Contains(gr.LivestockId) && gr.MeasurementDate >= thirtyDaysAgo)
            .CountAsync();

        // Basic score based on monitoring
        double score = 75;

        if (healthScore.Livestock.RecentGrowthRecords > 0)
        {
            score += 15; // Bonus for active monitoring
        }

        // Check tank capacity (basic estimation)
        var tank = await _context.Tanks.FindAsync(tankId);
        if (tank != null && tank.VolumeGallons > 0)
        {
            // Very rough rule: 1 inch of fish per gallon
            var totalFishSize = livestock.Count * 3; // Assume avg 3 inches per fish
            healthScore.Livestock.StockingLevel = (double)totalFishSize / tank.VolumeGallons * 100;

            if (healthScore.Livestock.StockingLevel > 100)
            {
                score -= 20;
                healthScore.Livestock.IsOverstocked = true;
                healthScore.Issues.Add(new HealthIssue
                {
                    Category = "Livestock",
                    Title = "Potential Overstocking",
                    Description = "Tank may be overstocked. Consider upgrading or reducing livestock.",
                    Severity = "Warning",
                    DetectedAt = DateTime.UtcNow
                });
            }
        }

        healthScore.LivestockScore = Math.Max(0, Math.Min(100, score));
    }

    private async Task CalculateStabilityScoreAsync(TankHealthScore healthScore, int tankId, string userId)
    {
        // Get last 5 water tests to check stability
        var recentTests = await _context.WaterTests
            .Where(wt => wt.TankId == tankId && wt.Tank!.UserId == userId)
            .OrderByDescending(wt => wt.Timestamp)
            .Take(5)
            .ToListAsync();

        if (recentTests.Count < 3)
        {
            healthScore.StabilityScore = 70; // Not enough data
            return;
        }

        double score = 100;
        int consistentParams = 0;
        int unstableParams = 0;

        // Check pH stability
        var phValues = recentTests.Where(t => t.PH.HasValue).Select(t => t.PH!.Value).ToList();
        if (phValues.Count >= 3)
        {
            var phVariance = CalculateVariance(phValues);
            healthScore.Stability.PhVariance = phVariance;

            if (phVariance < 0.2)
            {
                consistentParams++;
            }
            else if (phVariance > 0.5)
            {
                unstableParams++;
                score -= 15;
            }
        }

        // Check temperature stability
        var tempValues = recentTests.Where(t => t.Temperature.HasValue).Select(t => t.Temperature!.Value).ToList();
        if (tempValues.Count >= 3)
        {
            var tempVariance = CalculateVariance(tempValues);
            healthScore.Stability.TemperatureVariance = tempVariance;

            if (tempVariance < 2)
            {
                consistentParams++;
            }
            else if (tempVariance > 4)
            {
                unstableParams++;
                score -= 10;
            }
        }

        // Check ammonia consistency (should be consistently low)
        var ammoniaValues = recentTests.Where(t => t.Ammonia.HasValue).Select(t => t.Ammonia!.Value).ToList();
        if (ammoniaValues.Count >= 3)
        {
            var avgAmmonia = ammoniaValues.Average();
            healthScore.Stability.AmmoniaConsistency = avgAmmonia;

            if (avgAmmonia <= 0.25)
            {
                consistentParams++;
            }
            else
            {
                unstableParams++;
                score -= 20;
            }
        }

        healthScore.Stability.ConsistentParameters = consistentParams;
        healthScore.Stability.UnstableParameters = unstableParams;
        healthScore.StabilityScore = Math.Max(0, score);
    }

    private double CalculateWeightedScore(TankHealthScore healthScore)
    {
        // Weighted average with water quality being most important
        const double waterWeight = 0.35;
        const double maintenanceWeight = 0.25;
        const double equipmentWeight = 0.15;
        const double livestockWeight = 0.15;
        const double stabilityWeight = 0.10;

        return (healthScore.WaterQualityScore * waterWeight) +
               (healthScore.MaintenanceScore * maintenanceWeight) +
               (healthScore.EquipmentScore * equipmentWeight) +
               (healthScore.LivestockScore * livestockWeight) +
               (healthScore.StabilityScore * stabilityWeight);
    }

    private HealthScoreGrade GetHealthGrade(double score)
    {
        return score switch
        {
            >= 90 => HealthScoreGrade.Excellent,
            >= 75 => HealthScoreGrade.Good,
            >= 60 => HealthScoreGrade.Fair,
            >= 40 => HealthScoreGrade.Poor,
            _ => HealthScoreGrade.Critical
        };
    }

    private void GenerateRecommendations(TankHealthScore healthScore)
    {
        // Water quality recommendations
        if (healthScore.WaterQualityScore < 70)
        {
            healthScore.Recommendations.Add(new HealthRecommendation
            {
                Category = "Water Quality",
                Title = "Improve Water Parameters",
                Description = "Perform a water test and address any parameters outside ideal ranges.",
                Priority = healthScore.WaterQualityScore < 50 ? "High" : "Medium",
                ActionUrl = $"/tanks/{healthScore.TankId}/water-tests"
            });
        }

        if (healthScore.WaterQuality.DaysSinceLastTest > 7)
        {
            healthScore.Recommendations.Add(new HealthRecommendation
            {
                Category = "Water Quality",
                Title = "Schedule Water Test",
                Description = $"It's been {healthScore.WaterQuality.DaysSinceLastTest} days since your last water test.",
                Priority = healthScore.WaterQuality.DaysSinceLastTest > 14 ? "High" : "Medium",
                ActionUrl = $"/tanks/{healthScore.TankId}/water-tests"
            });
        }

        // Maintenance recommendations
        if (healthScore.Maintenance.IsOverdue)
        {
            healthScore.Recommendations.Add(new HealthRecommendation
            {
                Category = "Maintenance",
                Title = "Perform Maintenance",
                Description = "Your tank needs routine maintenance. Consider a water change and cleaning.",
                Priority = "High",
                ActionUrl = $"/tanks/{healthScore.TankId}/maintenance"
            });
        }

        // Equipment recommendations
        if (healthScore.Equipment.FailedEquipment > 0)
        {
            healthScore.Recommendations.Add(new HealthRecommendation
            {
                Category = "Equipment",
                Title = "Repair or Replace Equipment",
                Description = $"You have {healthScore.Equipment.FailedEquipment} piece(s) of equipment that need attention.",
                Priority = "High",
                ActionUrl = "/equipment"
            });
        }

        // Livestock recommendations
        if (healthScore.Livestock.IsOverstocked)
        {
            healthScore.Recommendations.Add(new HealthRecommendation
            {
                Category = "Livestock",
                Title = "Review Stocking Levels",
                Description = "Your tank may be overstocked. Consider upgrading tank size or reducing livestock.",
                Priority = "Medium",
                ActionUrl = "/livestock"
            });
        }

        // Stability recommendations
        if (healthScore.Stability.UnstableParameters > 0)
        {
            healthScore.Recommendations.Add(new HealthRecommendation
            {
                Category = "Stability",
                Title = "Improve Parameter Stability",
                Description = "Some water parameters are fluctuating. Focus on consistent maintenance and feeding.",
                Priority = "Medium",
                ActionUrl = $"/tanks/{healthScore.TankId}/water-tests"
            });
        }
    }

    private double CalculateVariance(List<double> values)
    {
        if (values.Count < 2) return 0;

        var avg = values.Average();
        var sumOfSquares = values.Sum(v => Math.Pow(v - avg, 2));
        return Math.Sqrt(sumOfSquares / values.Count);
    }
}
