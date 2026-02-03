using System;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

public class GrowthRecord
{
    public int Id { get; set; }

    // Foreign key to livestock
    public int LivestockId { get; set; }
    public Livestock? Livestock { get; set; }

    // Measurement details
    public DateTime MeasurementDate { get; set; }

    // Size measurements (optional - user can track one or both)
    public double? LengthInches { get; set; }
    public double? WeightGrams { get; set; }

    // For corals/plants
    public double? DiameterInches { get; set; }
    public double? HeightInches { get; set; }

    // Health and appearance notes
    public string? HealthCondition { get; set; } // Excellent, Good, Fair, Poor
    public string? ColorVibrancy { get; set; } // Vibrant, Normal, Faded, Pale
    public string Notes { get; set; } = string.Empty;

    // Optional photo reference
    public string? PhotoPath { get; set; }

    // Timestamp
    public DateTime CreatedAt { get; set; }
}
