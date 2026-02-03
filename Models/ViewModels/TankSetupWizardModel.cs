using System;
using System.ComponentModel.DataAnnotations;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models.ViewModels;

public class TankSetupWizardModel
{
    // Step 1: Basic Information
    [Required(ErrorMessage = "Tank name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tank name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tank type is required")]
    public AquariumType? Type { get; set; }

    // Step 2: Tank Specifications
    [Required(ErrorMessage = "Volume is required")]
    [Range(1, 10000, ErrorMessage = "Volume must be between 1 and 10,000 gallons")]
    public double? VolumeGallons { get; set; }

    [Range(0, 100, ErrorMessage = "Length must be between 0 and 100 inches")]
    public double? LengthInches { get; set; }

    [Range(0, 100, ErrorMessage = "Width must be between 0 and 100 inches")]
    public double? WidthInches { get; set; }

    [Range(0, 100, ErrorMessage = "Height must be between 0 and 100 inches")]
    public double? HeightInches { get; set; }

    // Step 3: Water Parameters (varies by type)
    [Range(0, 120, ErrorMessage = "Temperature must be between 0 and 120°F")]
    public double? TargetTemperature { get; set; }

    [Range(0, 14, ErrorMessage = "pH must be between 0 and 14")]
    public double? TargetPH { get; set; }

    // Saltwater/Reef specific
    [Range(0, 50, ErrorMessage = "Salinity must be between 0 and 50 ppt")]
    public double? TargetSalinity { get; set; }

    [Range(0, 1000, ErrorMessage = "Calcium must be between 0 and 1000 ppm")]
    public double? TargetCalcium { get; set; }

    [Range(0, 500, ErrorMessage = "Alkalinity must be between 0 and 500 dKH")]
    public double? TargetAlkalinity { get; set; }

    // Planted specific
    [Range(0, 100, ErrorMessage = "CO2 must be between 0 and 100 ppm")]
    public double? TargetCO2 { get; set; }

    // Step 4: Equipment Planning
    public bool HasFilter { get; set; }
    public FilterType? FilterType { get; set; }

    public bool HasHeater { get; set; }
    public int? HeaterWattage { get; set; }

    public bool HasLight { get; set; }
    public int? LightWattage { get; set; }

    public bool HasProteinSkimmer { get; set; }
    public bool HasWavemaker { get; set; }
    public bool HasCO2System { get; set; }

    // Step 5: Substrate & Decor
    public AquariumSubstrate? SubstrateType { get; set; }

    [Range(0, 1000, ErrorMessage = "Substrate amount must be between 0 and 1000 lbs")]
    public double? SubstratePounds { get; set; }

    public bool HasLiveRock { get; set; }

    [Range(0, 1000, ErrorMessage = "Live rock amount must be between 0 and 1000 lbs")]
    public double? LiveRockPounds { get; set; }

    // Step 6: Final Details
    [Required(ErrorMessage = "Start date is required")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string Notes { get; set; } = string.Empty;

    public string? ImagePath { get; set; }

    // Helper methods
    public bool IsReefOrSaltwater()
    {
        return Type == AquariumType.Reef || Type == AquariumType.Saltwater;
    }

    public bool IsPlanted()
    {
        return Type == AquariumType.Planted;
    }

    public bool IsFreshwater()
    {
        return Type == AquariumType.Freshwater || Type == AquariumType.Planted;
    }

    // Convert to Tank model
    public Tank ToTank()
    {
        var tank = new Tank
        {
            Name = Name,
            Type = Type!.Value,
            VolumeGallons = VolumeGallons!.Value,
            StartDate = StartDate,
            Notes = Notes,
            ImagePath = ImagePath
        };

        return tank;
    }
}
