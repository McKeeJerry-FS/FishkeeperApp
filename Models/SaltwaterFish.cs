using System;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

public class SaltwaterFish : Livestock
{
    // Basic classification
    public SaltwaterFishType FishType { get; set; }

    // Physical characteristics
    public double AdultSize { get; set; } // in inches
    public string Coloration { get; set; } = string.Empty;
    public string BodyShape { get; set; } = string.Empty;

    // Behavior and temperament
    public string Temperament { get; set; } = string.Empty; // Peaceful, Semi-aggressive, Aggressive
    public string ActivityLevel { get; set; } = string.Empty; // Low, Moderate, High
    public bool IsReefSafe { get; set; }
    public bool IsSchooling { get; set; }
    public int RecommendedSchoolSize { get; set; }

    // Tank requirements
    public double MinTankSize { get; set; } // in gallons
    public string SwimmingRegion { get; set; } = string.Empty; // Top, Middle, Bottom, All
    public bool RequiresLiveRock { get; set; }
    public bool RequiresHidingSpots { get; set; }

    // Water parameters
    public double OptimalTemperatureMin { get; set; } // in Fahrenheit
    public double OptimalTemperatureMax { get; set; }
    public double OptimalSalinityMin { get; set; } // specific gravity
    public double OptimalSalinityMax { get; set; }
    public string pHRange { get; set; } = string.Empty;

    // Diet and feeding
    public string Diet { get; set; } = string.Empty; // Herbivore, Carnivore, Omnivore
    public string FoodTypes { get; set; } = string.Empty; // Flakes, pellets, frozen, live, etc.
    public string FeedingFrequency { get; set; } = string.Empty;
    public string FeedingBehavior { get; set; } = string.Empty;

    // Care requirements
    public string CareLevel { get; set; } = string.Empty; // Beginner, Intermediate, Advanced, Expert
    public bool RequiresQuarantine { get; set; }

    // Compatibility
    public bool AggressiveToSameSpecies { get; set; }
    public bool AggressiveToOtherFish { get; set; }
    public string TankMateCompatibility { get; set; } = string.Empty;
    public bool NipsAtCorals { get; set; }
    public bool NipsAtInvertebrates { get; set; }

    // Special characteristics
    public bool IsJumper { get; set; } // Requires lid
    public bool IsVenomous { get; set; }
    public string SpecialRequirements { get; set; } = string.Empty;

    // Breeding
    public string BreedingDifficulty { get; set; } = string.Empty;
    public string BreedingNotes { get; set; } = string.Empty;

    // Health and lifespan
    public int AverageLifespanYears { get; set; }
    public bool ProneToIch { get; set; }
    public string CommonDiseases { get; set; } = string.Empty;
}
