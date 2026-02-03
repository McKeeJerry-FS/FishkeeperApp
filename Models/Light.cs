using System;

namespace AquaHub.MVC.Models;

public class Light : Equipment
{
    public double Wattage { get; set; }
    public string Spectrum { get; set; } = string.Empty;
    public bool IsDimmable { get; set; }
    public int IntensityPercent { get; set; }
    public string Schedule { get; set; } = string.Empty;
}
