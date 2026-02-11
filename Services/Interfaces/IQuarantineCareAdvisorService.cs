using AquaHub.MVC.Models;

namespace AquaHub.MVC.Services.Interfaces;

public interface IQuarantineCareAdvisorService
{
    /// <summary>
    /// Analyzes quarantine conditions and provides AI-powered care recommendations
    /// </summary>
    Task<QuarantineCareRecommendations> AnalyzeQuarantineConditionsAsync(
        Tank tank,
        List<WaterTest> recentWaterTests,
        List<DosingRecord> recentDosing,
        List<MaintenanceLog> recentMaintenance,
        List<Livestock> quarantinedLivestock);

    /// <summary>
    /// Analyzes water chemistry trends for quarantine tank
    /// </summary>
    List<string> AnalyzeWaterChemistryTrends(List<WaterTest> waterTests);

    /// <summary>
    /// Evaluates dosing effectiveness and suggests adjustments
    /// </summary>
    List<string> EvaluateDosingProtocol(List<DosingRecord> dosingRecords, List<WaterTest> waterTests);

    /// <summary>
    /// Provides recommendations based on quarantine duration
    /// </summary>
    List<string> GetDurationBasedRecommendations(DateTime? startDate, DateTime? endDate, int daysElapsed);

    /// <summary>
    /// Suggests next steps for quarantine care
    /// </summary>
    List<string> SuggestNextSteps(Tank tank, WaterTest? latestTest, int daysSinceLastWaterChange);
}

public class QuarantineCareRecommendations
{
    public string OverallAssessment { get; set; } = string.Empty;
    public string RiskLevel { get; set; } = "Low"; // Low, Medium, High, Critical
    public List<string> WaterChemistryInsights { get; set; } = new();
    public List<string> DosingRecommendations { get; set; } = new();
    public List<string> MaintenanceActions { get; set; } = new();
    public List<string> MonitoringPriorities { get; set; } = new();
    public List<string> NextSteps { get; set; } = new();
    public Dictionary<string, string> TreatmentGuidance { get; set; } = new();
    public bool RequiresImmediateAction { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.Now;
}
