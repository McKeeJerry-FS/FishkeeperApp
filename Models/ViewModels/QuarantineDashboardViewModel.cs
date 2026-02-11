using System;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Models.ViewModels;

public class QuarantineDashboardViewModel
{
    // Tank Information
    public Tank Tank { get; set; } = null!;

    // Quarantine Progress
    public int DaysInQuarantine { get; set; }
    public int RemainingDays { get; set; }
    public double ProgressPercentage { get; set; }
    public bool IsOverdue { get; set; }

    // Current Inhabitants
    public List<Livestock> QuarantinedLivestock { get; set; } = new();

    // AI-Powered Care Recommendations
    public QuarantineCareRecommendations? AIRecommendations { get; set; }

    // Water Chemistry
    public WaterTest? LatestWaterTest { get; set; }
    public List<WaterTest> RecentWaterTests { get; set; } = new();
    public bool HasCriticalParameters { get; set; }
    public List<string> WaterQualityAlerts { get; set; } = new();

    // Chart data for water parameters (last 14 days typical for quarantine)
    public List<string> ChartLabels { get; set; } = new();
    public List<double?> PHData { get; set; } = new();
    public List<double?> TemperatureData { get; set; } = new();
    public List<double?> AmmoniaData { get; set; } = new();
    public List<double?> NitriteData { get; set; } = new();
    public List<double?> NitrateData { get; set; } = new();
    public List<double?> SalinityData { get; set; } = new();

    // Dosing & Treatment
    public List<DosingRecord> ActiveTreatments { get; set; } = new();
    public List<DosingRecord> RecentDosing { get; set; } = new();
    public DosingRecord? NextScheduledDose { get; set; }

    // Feeding Schedule
    public List<FeedingSchedule> FeedingSchedules { get; set; } = new();
    public List<FeedingRecord> RecentFeedings { get; set; } = new();
    public FeedingSchedule? NextFeeding { get; set; }

    // Maintenance & Observations
    public List<MaintenanceLog> RecentMaintenance { get; set; } = new();
    public MaintenanceLog? LastWaterChange { get; set; }
    public int DaysSinceWaterChange { get; set; }

    // Health Monitoring
    public Dictionary<int, string> LivestockHealthStatus { get; set; } = new();
    public bool HasHealthConcerns { get; set; }
    public List<string> HealthAlerts { get; set; } = new();

    // Quick Actions
    public bool NeedsWaterTest { get; set; }
    public bool NeedsWaterChange { get; set; }
    public bool HasMissedDosing { get; set; }
}
