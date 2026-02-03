using System;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

public class FreshwaterFish : Livestock
{
    // Basic classification
    public FreshwaterFishType FishType { get; set; }

    // Physical characteristics
    public double AdultSize { get; set; } // in inches
    public string Coloration { get; set; } = string.Empty;
    public string BodyShape { get; set; } = string.Empty;
    public bool HasLongFins { get; set; }

    // Behavior and temperament
    public string Temperament { get; set; } = string.Empty; // Peaceful, Semi-aggressive, Aggressive
    public string ActivityLevel { get; set; } = string.Empty; // Low, Moderate, High
    public bool IsSchooling { get; set; }
    public int RecommendedSchoolSize { get; set; }
    public bool IsNocturnal { get; set; }

    // Tank requirements
    public double MinTankSize { get; set; } // in gallons
    public string SwimmingRegion { get; set; } = string.Empty; // Top, Middle, Bottom, All
    public bool RequiresPlants { get; set; }
    public bool RequiresHidingSpots { get; set; }
    public bool RequiresDriftwood { get; set; }

    // Water parameters
    public double OptimalTemperatureMin { get; set; } // in Fahrenheit
    public double OptimalTemperatureMax { get; set; }
    public string pHRange { get; set; } = string.Empty;
    public string HardnessRange { get; set; } = string.Empty; // GH and KH
    public string PreferredWaterFlow { get; set; } = string.Empty; // Low, Moderate, High

    // Diet and feeding
    public string Diet { get; set; } = string.Empty; // Herbivore, Carnivore, Omnivore
    public string FoodTypes { get; set; } = string.Empty; // Flakes, pellets, frozen, live, etc.
    public string FeedingFrequency { get; set; } = string.Empty;
    public bool IsBottomFeeder { get; set; }

    // Care requirements
    public string CareLevel { get; set; } = string.Empty; // Beginner, Intermediate, Advanced, Expert
    public bool RequiresQuarantine { get; set; }
    public bool RequiresCycledTank { get; set; }

    // Compatibility
    public bool AggressiveToSameSpecies { get; set; }
    public bool AggressiveToOtherFish { get; set; }
    public string TankMateCompatibility { get; set; } = string.Empty;
    public bool NipsAtFins { get; set; }
    public bool EatsSmallFish { get; set; }
    public bool EatsShrimp { get; set; }
    public bool EatsSnails { get; set; }

    // Special characteristics
    public bool IsJumper { get; set; } // Requires lid
    public bool IsLabyrinthFish { get; set; } // Can breathe air
    public bool RequiresAirstone { get; set; }
    public string SpecialRequirements { get; set; } = string.Empty;

    // Breeding
    public string BreedingDifficulty { get; set; } = string.Empty;
    public string BreedingType { get; set; } = string.Empty; // Egg layer, Live bearer, Mouth brooder, etc.
    public string BreedingNotes { get; set; } = string.Empty;

    // Health and lifespan
    public int AverageLifespanYears { get; set; }
    public bool ProneToIch { get; set; }
    public bool ProneToDropsy { get; set; }
    public string CommonDiseases { get; set; } = string.Empty;
}
