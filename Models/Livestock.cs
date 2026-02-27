using System;

namespace AquaHub.MVC.Models;

public abstract class Livestock
{
    // Environmental and care requirements
    public string? WaterParameters { get; set; } // e.g. "pH 6.5-7.5, Temp 74-78F, Ammonia 0, Nitrite 0, Nitrate <40"
    public string? LightingNeeds { get; set; } // e.g. "Low, Moderate, High" or spectrum
    public string? WaterFlowNeeds { get; set; } // e.g. "Low, Moderate, High, Turbulent"
    public string? FeedingAndNutrition { get; set; } // e.g. "Omnivore, feeds 2x daily, flakes, frozen"
    public string? Placement { get; set; } // e.g. "Foreground, Midground, Background, Rockwork"
    public string? AggressionLevel { get; set; } // e.g. "Peaceful, Semi-aggressive, Aggressive"
    public string? SafetyWarnings { get; set; } // e.g. "Venomous spines, may jump, toxic if eaten"
    public string? CommonPests { get; set; } // e.g. "Ich, flukes, flatworms"
    public string? CommonIllnesses { get; set; } // e.g. "Ich, velvet, dropsy"
    public string? CommonIssues { get; set; } // e.g. "Prone to jumping, fin nipping, sensitive to nitrate"
    public int Id { get; set; }
    public int TankId { get; set; }
    public Tank? Tank { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public DateTime AddedOn { get; set; }
    public string Notes { get; set; } = string.Empty;

    // Path to livestock image
    public string? ImagePath { get; set; }
}
