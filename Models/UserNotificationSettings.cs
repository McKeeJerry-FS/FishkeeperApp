using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaHub.MVC.Models;

public class UserNotificationSettings
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public AppUser? User { get; set; }

    public bool EmailNotificationsEnabled { get; set; } = true;

    public bool ReminderNotificationsEnabled { get; set; } = true;

    public bool WaterParameterAlertsEnabled { get; set; } = true;

    public bool EquipmentAlertsEnabled { get; set; } = true;

    // Email notification frequency (in hours)
    public int EmailDigestFrequencyHours { get; set; } = 0; // 0 = instant, >0 = digest

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
