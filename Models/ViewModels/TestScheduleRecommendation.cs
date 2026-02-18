using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models.ViewModels;

public class TestScheduleRecommendation
{
    public int TankId { get; set; }
    public string TankName { get; set; } = string.Empty;
    public AquariumType TankType { get; set; }
    public int TankAgeInDays { get; set; }
    public string TankAgeDescription { get; set; } = string.Empty;
    public TankMaturityLevel MaturityLevel { get; set; }

    // Recommended testing frequencies (in days)
    public List<ParameterTestSchedule> RecommendedSchedule { get; set; } = new();

    // Current testing habits
    public Dictionary<string, int> AverageTestingIntervals { get; set; } = new();
    public int TotalTestsRecorded { get; set; }
    public DateTime? LastTestDate { get; set; }

    // Analysis
    public bool IsTestingOptimal { get; set; }
    public List<string> Suggestions { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    // Quick actions
    public bool CanCreateReminders { get; set; }
    public List<ParameterTestSchedule> ParametersNeedingMoreFrequentTesting { get; set; } = new();
    public List<ParameterTestSchedule> ParametersNeedingLessFrequentTesting { get; set; } = new();
}

public class ParameterTestSchedule
{
    public string ParameterName { get; set; } = string.Empty;
    public string ParameterDisplayName { get; set; } = string.Empty;
    public int RecommendedFrequencyDays { get; set; }
    public string RecommendedFrequencyDescription { get; set; } = string.Empty;
    public int? CurrentFrequencyDays { get; set; }
    public string Importance { get; set; } = string.Empty; // Critical, High, Medium, Low
    public string Reason { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
    public string IconClass { get; set; } = "bi-droplet";
    public string ColorClass { get; set; } = "primary";
}

public enum TankMaturityLevel
{
    New = 1,          // 0-6 weeks
    Cycling = 2,      // 6-12 weeks
    Maturing = 3,     // 3-6 months
    Established = 4,  // 6-12 months
    Mature = 5        // 12+ months
}

public class TestScheduleOptimizerViewModel
{
    public Tank Tank { get; set; } = null!;
    public TestScheduleRecommendation Recommendation { get; set; } = null!;
    public List<WaterTest> RecentTests { get; set; } = new();
    public Dictionary<string, List<WaterTest>> TestHistory { get; set; } = new();
}
