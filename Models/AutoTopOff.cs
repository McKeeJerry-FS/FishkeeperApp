using System;

namespace AquaHub.MVC.Models;

public class AutoTopOff : Equipment
{
    public double ReservoirCapacity { get; set; } // Gallons
    public double PumpRate { get; set; } // Gallons per minute
    public string SensorType { get; set; } = string.Empty;
    public DateTime LastReservoirRefill { get; set; }
}
