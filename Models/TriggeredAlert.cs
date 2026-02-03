using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

/// <summary>
/// Represents an instance of a triggered parameter alert.
/// Created when a water test result violates an alert rule.
/// </summary>
public class TriggeredAlert
{
    public int Id { get; set; }

    [Required]
    public int ParameterAlertId { get; set; }

    [ForeignKey(nameof(ParameterAlertId))]
    public ParameterAlert? ParameterAlert { get; set; }

    [Required]
    public int WaterTestId { get; set; }

    [ForeignKey(nameof(WaterTestId))]
    public WaterTest? WaterTest { get; set; }

    [Required]
    public int TankId { get; set; }

    [ForeignKey(nameof(TankId))]
    public Tank? Tank { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public AppUser? User { get; set; }

    /// <summary>
    /// The parameter that triggered the alert
    /// </summary>
    [Required]
    public WaterParameter Parameter { get; set; }

    /// <summary>
    /// The actual value that triggered the alert
    /// </summary>
    public double ActualValue { get; set; }

    /// <summary>
    /// The minimum safe value (from alert config)
    /// </summary>
    public double? MinSafeValue { get; set; }

    /// <summary>
    /// The maximum safe value (from alert config)
    /// </summary>
    public double? MaxSafeValue { get; set; }

    /// <summary>
    /// Severity of this triggered alert
    /// </summary>
    public AlertSeverity Severity { get; set; }

    /// <summary>
    /// Generated message describing the alert
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// When this alert was triggered
    /// </summary>
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether the user has acknowledged this alert
    /// </summary>
    public bool IsAcknowledged { get; set; } = false;

    /// <summary>
    /// When the user acknowledged this alert
    /// </summary>
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>
    /// Whether this alert has been resolved (subsequent test showed values back in range)
    /// </summary>
    public bool IsResolved { get; set; } = false;

    /// <summary>
    /// When this alert was automatically resolved
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// The water test ID that resolved this alert (if resolved)
    /// </summary>
    public int? ResolvedByWaterTestId { get; set; }
}

