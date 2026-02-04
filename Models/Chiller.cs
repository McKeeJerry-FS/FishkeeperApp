using System;

namespace AquaHub.MVC.Models;

public class Chiller : Equipment
{
    public decimal MinTemperature { get; set; }
    public decimal MaxTemperature { get; set; }
    public decimal TargetTemperature { get; set; }
    public double BTUCapacity { get; set; }
    public double MaxTankSize { get; set; } // Gallons
}
