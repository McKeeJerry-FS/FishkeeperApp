using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

public class Notification
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public AppUser? User { get; set; }

    public int? TankId { get; set; }

    [ForeignKey(nameof(TankId))]
    public Tank? Tank { get; set; }

    public int? ReminderId { get; set; }

    [ForeignKey(nameof(ReminderId))]
    public Reminder? Reminder { get; set; }

    public int? WaterTestId { get; set; }

    [ForeignKey(nameof(WaterTestId))]
    public WaterTest? WaterTest { get; set; }

    [Required]
    public NotificationType Type { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    public bool EmailSent { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }

    [StringLength(500)]
    public string? ActionUrl { get; set; }

    public AlertSeverity? Severity { get; set; }
}
