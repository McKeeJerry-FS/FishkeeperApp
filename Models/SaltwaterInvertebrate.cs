using System;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

public class SaltwaterInvertebrate : Livestock
{
    // Basic classification
    public InvertebrateType InvertebrateType { get; set; }

    // Physical characteristics
    public double AdultSize { get; set; } // in inches
    public string Coloration { get; set; } = string.Empty;

    // Habitat and behavior
    public string Habitat { get; set; } = string.Empty;
    public string Behavior { get; set; } = string.Empty; // Peaceful, Aggressive, Nocturnal, etc.
    public bool IsReefSafe { get; set; }

    // Tank requirements
    public double MinTankSize { get; set; } // in gallons
    public string Placement { get; set; } = string.Empty; // Sand bed, Rock work, Open water, etc.

    // Water parameters
    public string WaterParameters { get; set; } = string.Empty;
    public double OptimalTemperatureMin { get; set; } // in Fahrenheit
    public double OptimalTemperatureMax { get; set; }
    public double OptimalSalinityMin { get; set; } // specific gravity
    public double OptimalSalinityMax { get; set; }
    public string pHRange { get; set; } = string.Empty;

    // Care requirements
    public string CareLevel { get; set; } = string.Empty; // Beginner, Intermediate, Advanced, Expert
    public string Diet { get; set; } = string.Empty;
    public string FeedingFrequency { get; set; } = string.Empty;

    // Compatibility
    public bool AggressiveTowardsOwnSpecies { get; set; }
    public string TankMateCompatibility { get; set; } = string.Empty;

    // Special characteristics
    public bool IsCleaner { get; set; } // Cleaner shrimp, etc.
    public bool RequiresAcclimation { get; set; } // Drip acclimation recommended
    public string SpecialRequirements { get; set; } = string.Empty;

    // Health and lifespan
    public int AverageLifespanYears { get; set; }
    public string CommonDiseases { get; set; } = string.Empty;
}
