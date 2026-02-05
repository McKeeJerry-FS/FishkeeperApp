using System;
using System.ComponentModel.DataAnnotations;

namespace AquaHub.MVC.Models;

public class WaterTest
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a tank")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid tank")]
    public int TankId { get; set; }
    public Tank? Tank { get; set; }

    // Shared 
    public double? PH { get; set; }
    public double? Temperature { get; set; }
    public double? Ammonia { get; set; }
    public double? Nitrite { get; set; }
    public double? Nitrate { get; set; }

    // Freshwater 
    public double? GH { get; set; }
    public double? KH { get; set; }
    public double? TDS { get; set; }

    // Reef 
    public double? Salinity { get; set; }
    public double? Alkalinity { get; set; }
    public double? Calcium { get; set; }
    public double? Magnesium { get; set; }
    public double? Phosphate { get; set; }
    public DateTime Timestamp { get; set; }
}
