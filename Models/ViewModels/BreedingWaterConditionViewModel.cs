using System;
using AquaHub.MVC.Models;

namespace AquaHub.MVC.Models.ViewModels;

public class BreedingWaterConditionViewModel
{
    public BreedingPair BreedingPair { get; set; } = null!;
    public WaterTest? LatestWaterTest { get; set; }
    public List<WaterParameterStatus> ParameterStatuses { get; set; } = new();
    public bool HasIdealParameters { get; set; }
    public bool IsOptimal { get; set; }
    public int ParametersOutOfRange { get; set; }
    public List<string> Recommendations { get; set; } = new();
}

public class WaterParameterStatus
{
    public string ParameterName { get; set; } = string.Empty;
    public double? CurrentValue { get; set; }
    public double? IdealMin { get; set; }
    public double? IdealMax { get; set; }
    public string Status { get; set; } = "Unknown"; // Optimal, Warning, Critical, Unknown
    public string? Recommendation { get; set; }
    public string Unit { get; set; } = string.Empty;
}
