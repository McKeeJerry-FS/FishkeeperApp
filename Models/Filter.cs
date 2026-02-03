using System;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

public class Filter : Equipment
{
    public FilterType Type { get; set; }
    public double FlowRate { get; set; } // Gallons per hour
    public string Media { get; set; } = string.Empty;
    public DateTime LastMaintenanceDate { get; set; }
}
