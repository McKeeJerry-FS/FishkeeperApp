using System;

namespace AquaHub.MVC.Models;

public abstract class Livestock
{
    public int Id { get; set; }
    public int TankId { get; set; }
    public Tank? Tank { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
    public DateTime AddedOn { get; set; }
    public string Notes { get; set; } = string.Empty;
}
