using System;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

public class Coral : Livestock
{
    // Basic classification
    public CoralType CoralType { get; set; }
    public string CoralFamily { get; set; } = string.Empty;
    public string ScientificName { get; set; } = string.Empty;

    // Physical characteristics
    public double ColonySize { get; set; } // in inches
    public string GrowthRate { get; set; } = string.Empty; // inches per year
    public string Coloration { get; set; } = string.Empty;
    public string PolyExtension { get; set; } = string.Empty; // Day, Night, Both

    // Lighting requirements
    public string LightingNeeds { get; set; } = string.Empty; // Low, Moderate, High
    public string LightIntensityPAR { get; set; } = string.Empty; // PAR range
    public string LightSpectrum { get; set; } = string.Empty; // Blue, Full spectrum, etc.

    // Flow requirements
    public string FlowNeeds { get; set; } = string.Empty; // Low, Moderate, High, Turbulent
    public string FlowType { get; set; } = string.Empty; // Direct, Indirect, Alternating

    // Placement
    public string Placement { get; set; } = string.Empty; // Bottom, Middle, Top, Sand bed
    public double SpacingRequirement { get; set; } // inches from other corals
    public bool IsAggressive { get; set; }
    public string AggressionMethod { get; set; } = string.Empty; // Sweeper tentacles, Chemical warfare, etc.

    // Water parameters
    public double OptimalTemperatureMin { get; set; } // in Fahrenheit
    public double OptimalTemperatureMax { get; set; }
    public double OptimalSalinityMin { get; set; } // specific gravity
    public double OptimalSalinityMax { get; set; }
    public string pHRange { get; set; } = string.Empty;
    public string AlkalinityRange { get; set; } = string.Empty; // dKH
    public string CalciumRange { get; set; } = string.Empty; // ppm
    public string MagnesiumRange { get; set; } = string.Empty; // ppm

    // Feeding
    public bool RequiresFeeding { get; set; }
    public string FoodTypes { get; set; } = string.Empty; // Photosynthetic only, Zooplankton, etc.
    public string FeedingFrequency { get; set; } = string.Empty;

    // Care requirements
    public string CareLevel { get; set; } = string.Empty; // Beginner, Intermediate, Advanced, Expert
    public bool RequiresStableParameters { get; set; }
    public bool RequiresDosing { get; set; }
    public string DosingRequirements { get; set; } = string.Empty; // Calcium, Alkalinity, Magnesium, etc.

    // Special characteristics
    public bool HasZooxanthellae { get; set; } // Symbiotic algae
    public bool IsToxic { get; set; }
    public bool RequiresAcclimation { get; set; }
    public string SpecialRequirements { get; set; } = string.Empty;

    // Propagation
    public bool CanBeFragged { get; set; }
    public string FraggingDifficulty { get; set; } = string.Empty;
    public string FraggingMethod { get; set; } = string.Empty;
    public string FraggingNotes { get; set; } = string.Empty;

    // Health
    public string CommonDiseases { get; set; } = string.Empty; // RTN, STN, Bleaching, etc.
    public string StressSigns { get; set; } = string.Empty;
}
