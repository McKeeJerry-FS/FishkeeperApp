using System;

namespace AquaHub.MVC.Models;

public class WaveMaker : Equipment
{
    public double FlowRate { get; set; } // Gallons per hour
    public bool IsControllable { get; set; }
    public string WavePattern { get; set; } = string.Empty;
    public int IntensityPercent { get; set; }
    public double PowerConsumption { get; set; } // Watts
}
