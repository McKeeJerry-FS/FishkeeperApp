namespace AquaHub.MVC.Models.Enums;

/// <summary>
/// Severity level for parameter alerts
/// </summary>
public enum AlertSeverity
{
    Info,       // Informational, no immediate action needed
    Warning,    // Parameter is getting close to unsafe range
    Caution,    // Parameter is outside ideal range
    Critical    // Parameter is dangerously out of range
}

