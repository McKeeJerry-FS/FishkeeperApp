using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

/// <summary>
/// Represents an alert configuration for water parameter monitoring.
/// When water test results fall outside the defined safe ranges, alerts are triggered.
/// </summary>
public class ParameterAlert
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public AppUser? User { get; set; }

    [Required]
    public int TankId { get; set; }

    [ForeignKey(nameof(TankId))]
    public Tank? Tank { get; set; }

    [Required]
    public WaterParameter Parameter { get; set; }

    /// <summary>
    /// Minimum safe value for this parameter (inclusive)
    /// </summary>
    public double? MinValue { get; set; }

    /// <summary>
    /// Maximum safe value for this parameter (inclusive)
    /// </summary>
    public double? MaxValue { get; set; }

    /// <summary>
    /// Whether this alert rule is currently active
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Alert severity level
    /// </summary>
    public AlertSeverity Severity { get; set; } = AlertSeverity.Warning;

    /// <summary>
    /// Custom message to display when alert is triggered (optional)
    /// </summary>
    [StringLength(500)]
    public string? CustomMessage { get; set; }

    /// <summary>
    /// When this alert configuration was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this alert was last modified
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The last time this alert was triggered (if ever)
    /// </summary>
    public DateTime? LastTriggeredAt { get; set; }

    /// <summary>
    /// How many times this alert has been triggered
    /// </summary>
    public int TriggerCount { get; set; } = 0;
}