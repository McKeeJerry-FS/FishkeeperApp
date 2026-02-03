namespace AquaHub.MVC.Models.Enums;

public enum HealthScoreGrade
{
    Excellent,  // 90-100
    Good,       // 75-89
    Fair,       // 60-74
    Poor,       // 40-59
    Critical    // 0-39
}

public enum HealthCategory
{
    WaterQuality,
    MaintenanceSchedule,
    EquipmentStatus,
    LivestockHealth,
    ParameterStability
}