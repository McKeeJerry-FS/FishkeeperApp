using System;

namespace AquaHub.MVC.Models;

public class DosingRecord
{
    public int Id { get; set; }
    public int TankId { get; set; }
    public Tank? Tank { get; set; }
    
    // Supply item tracking (optional - links to inventory)
    public int? SupplyItemId { get; set; }
    public SupplyItem? SupplyItem { get; set; }
    
    public string Additive { get; set; } = string.Empty; // e.g., "NPK", "Calcium", "Alkalinity"
    public double AmountMl { get; set; }
    public DateTime Timestamp { get; set; }
    
    // User who performed the dosing
    public string UserId { get; set; } = string.Empty;
    public AppUser? User { get; set; }
}
