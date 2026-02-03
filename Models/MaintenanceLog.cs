using System;
using System.ComponentModel.DataAnnotations;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

public class MaintenanceLog
{
    public int Id { get; set; }
    public int TankId { get; set; }
    public Tank? Tank { get; set; }
    public MaintenanceType Type { get; set; }
    public double? WaterChangePercent { get; set; }
    public string Notes { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
