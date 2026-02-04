using System;

namespace AquaHub.MVC.Models;

public class CO2System : Equipment
{
    public double? TargetPH { get; set; }
    public double? CurrentPH { get; set; }
    public string? RegulatorType { get; set; }
    public string? DiffuserType { get; set; }
}
