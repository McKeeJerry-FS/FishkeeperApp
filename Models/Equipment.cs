using System;
using System.ComponentModel.DataAnnotations;

namespace AquaHub.MVC.Models;

public abstract class Equipment
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a tank")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid tank")]
    public int TankId { get; set; }
    public Tank? Tank { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public DateTime InstalledOn { get; set; }
}
