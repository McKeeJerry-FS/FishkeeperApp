using System;

namespace AquaHub.MVC.Models;

public class PhotoLog
{
    public int Id { get; set; } 
    public int TankId { get; set; } 
    public Tank? Tank { get; set; } 
    public string ImageUrl { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
