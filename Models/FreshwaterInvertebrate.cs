using System;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

public class FreshwaterInvertebrate : Livestock
{
    // Basic classification
    public FreshwaterInvertebrateType InvertebrateType { get; set; }

    // Physical characteristics
    public double AdultSize { get; set; } // in inches
    public string Coloration { get; set; } = string.Empty;

    // Habitat and behavior
    public string Habitat { get; set; } = string.Empty;
    public string Behavior { get; set; } = string.Empty; // Peaceful, Aggressive, Nocturnal, etc.
    public bool IsPlantSafe { get; set; }

    // Tank requirements
    public double MinTankSize { get; set; } // in gallons
    public string Placement { get; set; } = string.Empty; // Substrate, Driftwood, Rocks, etc.

    // Water parameters
    public string WaterParameters { get; set; } = string.Empty;
    public double OptimalTemperatureMin { get; set; } // in Fahrenheit
    public double OptimalTemperatureMax { get; set; }
    public string pHRange { get; set; } = string.Empty;
    public string HardnessRange { get; set; } = string.Empty; // GH and KH

    // Care requirements
    public string CareLevel { get; set; } = string.Empty; // Beginner, Intermediate, Advanced, Expert
    public string Diet { get; set; } = string.Empty;
    public string FeedingFrequency { get; set; } = string.Empty;

    // Compatibility
    public bool AggressiveTowardsOwnSpecies { get; set; }
    public string TankMateCompatibility { get; set; } = string.Empty;

    // Special characteristics
    public bool IsAlgaeEater { get; set; }
    public bool IsScavenger { get; set; }
    public bool RequiresAcclimation { get; set; }
    public bool SensitiveToCopper { get; set; } // Important for medication compatibility
    public string SpecialRequirements { get; set; } = string.Empty;

    // Breeding
    public string BreedingDifficulty { get; set; } = string.Empty;
    public string BreedingNotes { get; set; } = string.Empty;

    // Health and lifespan
    public int AverageLifespanYears { get; set; }
    public string CommonDiseases { get; set; } = string.Empty;
}
