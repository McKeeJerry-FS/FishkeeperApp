using System;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

public class Plant : Livestock
{
    // Basic Information
    public PlantType PlantType { get; set; } // e.g., "Stem", "Carpet", "Floating", "Rosette"

    // Physical Characteristics
    public double MaxHeight { get; set; } // In inches
    public string GrowthRate { get; set; } = string.Empty; // "Slow", "Moderate", "Fast"
    public string Coloration { get; set; } = string.Empty; // "Green", "Red", "Brown", "Variegated"
    public string Placement { get; set; } = string.Empty; // "Foreground", "Midground", "Background", "Floating"

    // Lighting Requirements
    public string LightingNeeds { get; set; } = string.Empty; // Kept for backward compatibility
    public string LightingRequirement { get; set; } = string.Empty; // "Low", "Medium", "High", "Very High"

    // CO2 Requirements
    public string Co2Needs { get; set; } = string.Empty; // Kept for backward compatibility
    public bool RequiresCO2 { get; set; }

    // Care Information
    public string CareLevel { get; set; } = string.Empty; // "Easy", "Moderate", "Difficult", "Expert"

    // Water Parameters
    public double OptimalTemperatureMin { get; set; } // In Fahrenheit
    public double OptimalTemperatureMax { get; set; } // In Fahrenheit
    public string pHRange { get; set; } = string.Empty; // e.g., "6.0-7.5"
    public string HardnessRange { get; set; } = string.Empty; // e.g., "3-10 dGH"

    // Substrate & Fertilization
    public string SubstrateRequirement { get; set; } = string.Empty; // "Sand", "Gravel", "Soil", "Any"
    public bool RequiresRootTabs { get; set; }
    public bool RequiresFertilization { get; set; }

    // Propagation
    public bool CanBePropagated { get; set; }
    public string PropagationMethod { get; set; } = string.Empty; // "Cuttings", "Runners", "Division", "Adventitious plantlets"

    // Special Characteristics
    public string SpecialRequirements { get; set; } = string.Empty; // Any special care notes

    // Tank Compatibility
    public bool SafeForShrimp { get; set; }
    public bool SafeForSnails { get; set; }
    public bool OxygenProducer { get; set; } // Good for oxygenation
    public bool NitrateAbsorber { get; set; } // Good for nutrient export

    // Additional Notes
    public string Benefits { get; set; } = string.Empty; // Benefits to the aquarium
    public string CommonIssues { get; set; } = string.Empty; // Common problems (algae on leaves, melting, etc.)
}
