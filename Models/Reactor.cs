using System;

namespace AquaHub.MVC.Models;

public class Reactor : Equipment
{
    public string ReactorType { get; set; } = string.Empty; // Calcium, Carbon, etc.
    public double FlowRate { get; set; } // Gallons per hour
    public string MediaType { get; set; } = string.Empty;
    public DateTime LastMediaChange { get; set; }
    public double MediaCapacity { get; set; } // Volume in ml or weight in grams
}
