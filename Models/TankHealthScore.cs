using System;
using System.Collections.Generic;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

public class TankHealthScore
{
    public int TankId { get; set; }
    public string TankName { get; set; } = string.Empty;

    // Overall score (0-100)
    public double OverallScore { get; set; }
    public HealthScoreGrade Grade { get; set; }

    // Category scores
    public double WaterQualityScore { get; set; }
    public double MaintenanceScore { get; set; }
    public double EquipmentScore { get; set; }
    public double LivestockScore { get; set; }
    public double StabilityScore { get; set; }

    // Detailed metrics
    public WaterQualityMetrics WaterQuality { get; set; } = new();
    public MaintenanceMetrics Maintenance { get; set; } = new();
    public EquipmentMetrics Equipment { get; set; } = new();
    public LivestockMetrics Livestock { get; set; } = new();
    public StabilityMetrics Stability { get; set; } = new();

    // Recommendations and issues
    public List<HealthRecommendation> Recommendations { get; set; } = new();
    public List<HealthIssue> Issues { get; set; } = new();

    // Metadata
    public DateTime CalculatedAt { get; set; }
    public DateTime LastWaterTest { get; set; }
    public DateTime LastMaintenance { get; set; }
}

public class WaterQualityMetrics
{
    public bool HasRecentTest { get; set; }
    public int DaysSinceLastTest { get; set; }
    public int ParametersInRange { get; set; }
    public int ParametersOutOfRange { get; set; }
    public int TotalParametersTested { get; set; }
    public List<string> CriticalParameters { get; set; } = new();
    public double ParameterComplianceRate { get; set; }
}

public class MaintenanceMetrics
{
    public int DaysSinceLastMaintenance { get; set; }
    public int MaintenanceLogsLast30Days { get; set; }
    public int MaintenanceLogsLast90Days { get; set; }
    public bool IsOverdue { get; set; }
    public double MaintenanceFrequencyScore { get; set; }
}

public class EquipmentMetrics
{
    public int TotalEquipment { get; set; }
    public int WorkingEquipment { get; set; }
    public int FailedEquipment { get; set; }
    public int EquipmentNeedingMaintenance { get; set; }
    public double EquipmentHealthRate { get; set; }
}

public class LivestockMetrics
{
    public int TotalLivestock { get; set; }
    public int RecentGrowthRecords { get; set; }
    public bool HasHealthConcerns { get; set; }
    public double StockingLevel { get; set; }
    public bool IsOverstocked { get; set; }
}

public class StabilityMetrics
{
    public double TemperatureVariance { get; set; }
    public double PhVariance { get; set; }
    public double AmmoniaConsistency { get; set; }
    public int ConsistentParameters { get; set; }
    public int UnstableParameters { get; set; }
}

public class HealthRecommendation
{
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    public string ActionUrl { get; set; } = string.Empty;
}

public class HealthIssue
{
    public string Category { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "Warning"; // Info, Warning, Error, Critical
    public DateTime DetectedAt { get; set; }
}

