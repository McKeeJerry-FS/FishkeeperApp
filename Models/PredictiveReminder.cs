using System;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

/// <summary>
/// Represents an AI-generated predictive reminder suggestion based on user patterns
/// </summary>
public class PredictiveReminder
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public AppUser? User { get; set; }

    public int? TankId { get; set; }
    public Tank? Tank { get; set; }

    // Suggestion details
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ReminderType SuggestedType { get; set; }
    public DateTime SuggestedDate { get; set; }

    // Confidence and reasoning
    public double ConfidenceScore { get; set; } // 0.0 - 1.0
    public string Reasoning { get; set; } = string.Empty; // Why this suggestion was made
    public PredictionSource Source { get; set; }

    // Pattern analysis data
    public int PatternOccurrences { get; set; } // How many times this pattern was observed
    public double AverageDaysBetween { get; set; } // Average days between occurrences
    public DateTime? LastOccurrence { get; set; }

    // User interaction
    public bool IsAccepted { get; set; } = false;
    public bool IsDismissed { get; set; } = false;
    public DateTime? AcceptedAt { get; set; }
    public DateTime? DismissedAt { get; set; }
    public int? CreatedReminderId { get; set; } // ID of reminder if accepted

    // Metadata
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } // Suggestion expires if not acted upon
}

/// <summary>
/// Source of the predictive reminder
/// </summary>
public enum PredictionSource
{
    MaintenancePattern,      // Based on maintenance log patterns
    WaterTestPattern,        // Based on water test frequency
    ParameterTrend,          // Based on declining water quality
    EquipmentAge,            // Based on equipment installation date
    SeasonalPattern,         // Based on seasonal variations
    TankAge,                 // Based on tank maturity
    HealthScoreTrend,        // Based on declining health score
    CommunityAverage         // Based on similar tank setups (future)
}

