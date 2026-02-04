using System;

namespace AquaHub.MVC.Models;

public class Pump : Equipment
{
    public double FlowRate { get; set; } // Gallons per hour
    public double MaxHeadHeight { get; set; } // Feet
    public double PowerConsumption { get; set; } // Watts
    public bool IsVariableSpeed { get; set; }
}
