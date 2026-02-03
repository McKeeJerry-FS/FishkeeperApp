using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

public class Reminder
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

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public ReminderType Type { get; set; }

    [Required]
    public ReminderFrequency Frequency { get; set; }

    [Required]
    public DateTime NextDueDate { get; set; }

    public DateTime? LastCompletedDate { get; set; }

    public bool IsActive { get; set; } = true;

    public bool SendEmailNotification { get; set; } = false;

    // How many hours before the due date to send notification
    public int NotificationHoursBefore { get; set; } = 24;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

