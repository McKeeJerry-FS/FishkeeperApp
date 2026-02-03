using System;

namespace AquaHub.MVC.Models;

public class ProteinSkimmer : Equipment
{
    public double Capacity { get; set; }
    public string Type { get; set; } = string.Empty;
    public int AirIntake { get; set; } 
    public int CupFillLevel { get; set; }
}
