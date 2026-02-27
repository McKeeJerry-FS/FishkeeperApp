using System;
using System.Collections.Generic;

namespace AquaHub.MVC.Models.ViewModels;

public class TankDashboardViewModel
{
    public Tank Tank { get; set; } = null!;
    public int SelectedMonth { get; set; } = DateTime.Now.Month;
    public int SelectedYear { get; set; } = DateTime.Now.Year;
    public List<TankMilestone> TankMilestones { get; set; } = new();

    // Water tests
    public WaterTest? MostRecentWaterTest { get; set; }
    public List<WaterTest> RecentWaterTests { get; set; } = new();

    // Chart data
    public List<string> ChartLabels { get; set; } = new();
    public List<double?> PHData { get; set; } = new();
    public List<double?> TemperatureData { get; set; } = new();
    public List<double?> AmmoniaData { get; set; } = new();
    public List<double?> NitriteData { get; set; } = new();
    public List<double?> NitrateData { get; set; } = new();
    public List<double?> SalinityData { get; set; } = new();
    public List<double?> AlkalinityData { get; set; } = new();
    public List<double?> CalciumData { get; set; } = new();
    public List<double?> MagnesiumData { get; set; } = new();
    public List<double?> PhosphateData { get; set; } = new();
    public List<double?> GHData { get; set; } = new();
    public List<double?> KHData { get; set; } = new();
    public List<double?> TDSData { get; set; } = new();

    // Month selection
    public List<MonthYearOption> AvailableMonths { get; set; } = new();

    // Equipment alerts
    public List<Equipment> EquipmentNeedingMaintenance { get; set; } = new();

    // Recent maintenance
    public List<MaintenanceLog> RecentMaintenance { get; set; } = new();

    // Photo logs
    public List<PhotoLog> RecentPhotos { get; set; } = new();

    // Dosing records
    public List<DosingRecord> RecentDosingRecords { get; set; } = new();

    // Reminders and Notifications
    public List<Reminder> UpcomingReminders { get; set; } = new();
    public List<Notification> RecentNotifications { get; set; } = new();
    public int UnreadNotificationCount { get; set; }

    // Latest Journal Entry
    public JournalEntry? LatestJournalEntry { get; set; }
}

public class MonthYearOption
{
    public int Month { get; set; }
    public int Year { get; set; }
    public string DisplayText { get; set; } = string.Empty;
}
