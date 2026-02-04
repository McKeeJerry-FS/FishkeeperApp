using System;

namespace AquaHub.MVC.Models;

public class UVSterilizer : Equipment
{
    public double Wattage { get; set; }
    public double FlowRate { get; set; } // Gallons per hour
    public DateTime BulbInstalledDate { get; set; }
    public int BulbLifespanHours { get; set; }
    public double MaxTankSize { get; set; } // Gallons
}
