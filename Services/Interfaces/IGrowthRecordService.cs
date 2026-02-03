using System;
using AquaHub.MVC.Models;

namespace AquaHub.MVC.Services.Interfaces;

public interface IGrowthRecordService
{
    // Get all growth records for a specific livestock
    Task<List<GrowthRecord>> GetGrowthRecordsForLivestockAsync(int livestockId, string userId);

    // Get a single growth record by id
    Task<GrowthRecord?> GetGrowthRecordByIdAsync(int recordId, string userId);

    // Add a new growth record
    Task<GrowthRecord> AddGrowthRecordAsync(GrowthRecord record, string userId);

    // Update an existing growth record
    Task<GrowthRecord?> UpdateGrowthRecordAsync(GrowthRecord record, string userId);

    // Delete a growth record
    Task<bool> DeleteGrowthRecordAsync(int recordId, string userId);

    // Get growth statistics for a livestock
    Task<GrowthStatistics?> GetGrowthStatisticsAsync(int livestockId, string userId);

    // Get recent growth records across all livestock for a user
    Task<List<GrowthRecord>> GetRecentGrowthRecordsAsync(string userId, int count = 10);
}

public class GrowthStatistics
{
    public int LivestockId { get; set; }
    public string LivestockName { get; set; } = string.Empty;
    public int TotalMeasurements { get; set; }
    public DateTime? FirstMeasurement { get; set; }
    public DateTime? LastMeasurement { get; set; }

    // Length growth
    public double? InitialLength { get; set; }
    public double? CurrentLength { get; set; }
    public double? LengthGrowth { get; set; }
    public double? LengthGrowthPercentage { get; set; }

    // Weight growth
    public double? InitialWeight { get; set; }
    public double? CurrentWeight { get; set; }
    public double? WeightGrowth { get; set; }
    public double? WeightGrowthPercentage { get; set; }

    // For corals/plants
    public double? InitialDiameter { get; set; }
    public double? CurrentDiameter { get; set; }
    public double? DiameterGrowth { get; set; }

    public double? InitialHeight { get; set; }
    public double? CurrentHeight { get; set; }
    public double? HeightGrowth { get; set; }

    // Health trends
    public string? CurrentHealth { get; set; }
    public string? HealthTrend { get; set; } // Improving, Stable, Declining
}
