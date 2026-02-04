using System;

namespace AquaHub.MVC.Models;

public class ReverseOsmosisSystem : Equipment
{
    public double GPDRating { get; set; } // Gallons per day
    public int NumberOfStages { get; set; }
    public DateTime LastMembraneChange { get; set; }
    public DateTime LastFilterChange { get; set; }
    public int MembraneLifespanMonths { get; set; }
    public double WasteWaterRatio { get; set; }
}
