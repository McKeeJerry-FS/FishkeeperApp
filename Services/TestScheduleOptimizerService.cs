using Microsoft.EntityFrameworkCore;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;
using AquaHub.MVC.Models.ViewModels;

namespace AquaHub.MVC.Services;

public interface ITestScheduleOptimizerService
{
    Task<TestScheduleRecommendation> GetRecommendedScheduleAsync(int tankId, string userId);
    Task<bool> CreateRemindersFromScheduleAsync(int tankId, string userId, List<string> selectedParameters);
}

public class TestScheduleOptimizerService : ITestScheduleOptimizerService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TestScheduleOptimizerService> _logger;

    public TestScheduleOptimizerService(
        ApplicationDbContext context,
        ILogger<TestScheduleOptimizerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TestScheduleRecommendation> GetRecommendedScheduleAsync(int tankId, string userId)
    {
        var tank = await _context.Tanks
            .Include(t => t.WaterTests)
            .FirstOrDefaultAsync(t => t.Id == tankId && t.UserId == userId);

        if (tank == null)
        {
            throw new ArgumentException("Tank not found or access denied.");
        }

        var recommendation = new TestScheduleRecommendation
        {
            TankId = tank.Id,
            TankName = tank.Name,
            TankType = tank.Type,
            TankAgeInDays = (DateTime.UtcNow - tank.StartDate).Days
        };

        // Determine tank maturity level
        recommendation.MaturityLevel = DetermineMaturityLevel(recommendation.TankAgeInDays);
        recommendation.TankAgeDescription = GetMaturityDescription(recommendation.MaturityLevel, recommendation.TankAgeInDays);

        // Get testing history
        var allTests = tank.WaterTests.OrderBy(t => t.Timestamp).ToList();
        recommendation.TotalTestsRecorded = allTests.Count;
        recommendation.LastTestDate = allTests.LastOrDefault()?.Timestamp;

        // Calculate current testing intervals
        recommendation.AverageTestingIntervals = CalculateAverageIntervals(allTests);

        // Generate recommended schedule based on tank type and maturity
        recommendation.RecommendedSchedule = GenerateRecommendedSchedule(tank.Type, recommendation.MaturityLevel);

        // Analyze current vs recommended
        AnalyzeTestingBehavior(recommendation);

        return recommendation;
    }

    private TankMaturityLevel DetermineMaturityLevel(int ageInDays)
    {
        if (ageInDays < 42) return TankMaturityLevel.New;           // 0-6 weeks
        if (ageInDays < 84) return TankMaturityLevel.Cycling;       // 6-12 weeks
        if (ageInDays < 180) return TankMaturityLevel.Maturing;     // 3-6 months
        if (ageInDays < 365) return TankMaturityLevel.Established;  // 6-12 months
        return TankMaturityLevel.Mature;                             // 12+ months
    }

    private string GetMaturityDescription(TankMaturityLevel level, int days)
    {
        var weeks = days / 7;
        var months = days / 30;

        return level switch
        {
            TankMaturityLevel.New => $"New Tank ({weeks} weeks old) - Critical cycling period",
            TankMaturityLevel.Cycling => $"Cycling Tank ({weeks} weeks old) - Establishing beneficial bacteria",
            TankMaturityLevel.Maturing => $"Maturing Tank ({months} months old) - Building stability",
            TankMaturityLevel.Established => $"Established Tank ({months} months old) - Stable ecosystem",
            TankMaturityLevel.Mature => $"Mature Tank ({months} months old) - Well-established system",
            _ => "Unknown maturity level"
        };
    }

    private List<ParameterTestSchedule> GenerateRecommendedSchedule(AquariumType tankType, TankMaturityLevel maturity)
    {
        var schedule = new List<ParameterTestSchedule>();

        // Core parameters for all tanks
        AddParameter(schedule, "Ammonia", "Ammonia (NH3/NH4+)", maturity switch
        {
            TankMaturityLevel.New => 2,
            TankMaturityLevel.Cycling => 3,
            TankMaturityLevel.Maturing => 7,
            TankMaturityLevel.Established => 14,
            TankMaturityLevel.Mature => 30,
            _ => 7
        }, "Critical", "Toxic to fish even in small amounts", true, "bi-exclamation-octagon-fill", "danger");

        AddParameter(schedule, "Nitrite", "Nitrite (NO2-)", maturity switch
        {
            TankMaturityLevel.New => 2,
            TankMaturityLevel.Cycling => 3,
            TankMaturityLevel.Maturing => 7,
            TankMaturityLevel.Established => 14,
            TankMaturityLevel.Mature => 30,
            _ => 7
        }, "Critical", "Toxic to fish, indicates incomplete cycling", true, "bi-exclamation-triangle-fill", "danger");

        AddParameter(schedule, "Nitrate", "Nitrate (NO3-)", maturity switch
        {
            TankMaturityLevel.New => 7,
            TankMaturityLevel.Cycling => 7,
            TankMaturityLevel.Maturing => 7,
            TankMaturityLevel.Established => 14,
            TankMaturityLevel.Mature => 14,
            _ => 7
        }, "High", "Accumulates over time, requires water changes", true, "bi-info-circle-fill", "warning");

        AddParameter(schedule, "PH", "pH Level", maturity switch
        {
            TankMaturityLevel.New => 7,
            TankMaturityLevel.Cycling => 7,
            _ => 14
        }, "High", "Affects livestock health and biological processes", true, "bi-droplet-half", "info");

        AddParameter(schedule, "Temperature", "Temperature", 7, "High", "Affects metabolism and oxygen levels", true, "bi-thermometer-half", "primary");

        // Saltwater/Reef specific
        if (tankType == AquariumType.Saltwater || tankType == AquariumType.Reef || tankType == AquariumType.NanoReef)
        {
            AddParameter(schedule, "Salinity", "Salinity (Specific Gravity)", 7, "Critical", "Essential for marine livestock health", true, "bi-droplet-fill", "info");

            AddParameter(schedule, "Alkalinity", "Alkalinity (dKH)", maturity switch
            {
                TankMaturityLevel.New => 7,
                TankMaturityLevel.Cycling => 7,
                _ => 7
            }, "High", "Maintains pH stability and coral health", true, "bi-graph-up", "success");

            if (tankType == AquariumType.Reef || tankType == AquariumType.NanoReef)
            {
                AddParameter(schedule, "Calcium", "Calcium (Ca)", 7, "High", "Essential for coral skeleton growth", true, "bi-gem", "primary");
                AddParameter(schedule, "Magnesium", "Magnesium (Mg)", 14, "Medium", "Supports calcium and alkalinity", true, "bi-hexagon", "secondary");
                AddParameter(schedule, "Phosphate", "Phosphate (PO4)", 7, "High", "Affects coral coloration and algae growth", true, "bi-droplet-half", "warning");
            }
        }

        // Freshwater specific
        if (tankType == AquariumType.Freshwater || tankType == AquariumType.Planted ||
            tankType == AquariumType.Cichlid || tankType == AquariumType.Betta || tankType == AquariumType.Goldfish)
        {
            AddParameter(schedule, "GH", "General Hardness (GH)", 14, "Medium", "Affects fish health and osmoregulation", false, "bi-droplet", "info");
            AddParameter(schedule, "KH", "Carbonate Hardness (KH)", 14, "Medium", "Buffers pH changes", false, "bi-droplet-half", "info");
        }

        // Planted tank specific
        if (tankType == AquariumType.Planted)
        {
            AddParameter(schedule, "Iron", "Iron (Fe)", 7, "Medium", "Essential micronutrient for plant growth", false, "bi-flower1", "success");
            AddParameter(schedule, "CO2", "CO2 Levels", 7, "Medium", "Critical for photosynthesis", false, "bi-cloud", "primary");
            AddParameter(schedule, "TDS", "Total Dissolved Solids", 14, "Low", "Overall water quality indicator", false, "bi-water", "secondary");
        }

        return schedule.OrderByDescending(p => p.IsRequired)
                      .ThenBy(p => p.RecommendedFrequencyDays)
                      .ToList();
    }

    private void AddParameter(List<ParameterTestSchedule> schedule, string name, string displayName,
        int frequencyDays, string importance, string reason, bool isRequired, string icon, string color)
    {
        schedule.Add(new ParameterTestSchedule
        {
            ParameterName = name,
            ParameterDisplayName = displayName,
            RecommendedFrequencyDays = frequencyDays,
            RecommendedFrequencyDescription = GetFrequencyDescription(frequencyDays),
            Importance = importance,
            Reason = reason,
            IsRequired = isRequired,
            IconClass = icon,
            ColorClass = color
        });
    }

    private string GetFrequencyDescription(int days)
    {
        return days switch
        {
            1 => "Daily",
            2 => "Every 2 days",
            3 => "Every 3 days",
            7 => "Weekly",
            14 => "Bi-weekly",
            30 => "Monthly",
            _ => $"Every {days} days"
        };
    }

    private Dictionary<string, int> CalculateAverageIntervals(List<WaterTest> tests)
    {
        var intervals = new Dictionary<string, int>();

        if (tests.Count < 2) return intervals;

        var orderedTests = tests.OrderBy(t => t.Timestamp).ToList();
        var parameterTests = new Dictionary<string, List<DateTime>>();

        // Group tests by parameter
        foreach (var test in orderedTests)
        {
            if (test.Ammonia.HasValue) AddTestDate(parameterTests, "Ammonia", test.Timestamp);
            if (test.Nitrite.HasValue) AddTestDate(parameterTests, "Nitrite", test.Timestamp);
            if (test.Nitrate.HasValue) AddTestDate(parameterTests, "Nitrate", test.Timestamp);
            if (test.PH.HasValue) AddTestDate(parameterTests, "PH", test.Timestamp);
            if (test.Temperature.HasValue) AddTestDate(parameterTests, "Temperature", test.Timestamp);
            if (test.Salinity.HasValue) AddTestDate(parameterTests, "Salinity", test.Timestamp);
            if (test.Alkalinity.HasValue) AddTestDate(parameterTests, "Alkalinity", test.Timestamp);
            if (test.Calcium.HasValue) AddTestDate(parameterTests, "Calcium", test.Timestamp);
            if (test.Magnesium.HasValue) AddTestDate(parameterTests, "Magnesium", test.Timestamp);
            if (test.Phosphate.HasValue) AddTestDate(parameterTests, "Phosphate", test.Timestamp);
            if (test.GH.HasValue) AddTestDate(parameterTests, "GH", test.Timestamp);
            if (test.KH.HasValue) AddTestDate(parameterTests, "KH", test.Timestamp);
            if (test.Iron.HasValue) AddTestDate(parameterTests, "Iron", test.Timestamp);
            if (test.CO2.HasValue) AddTestDate(parameterTests, "CO2", test.Timestamp);
        }

        // Calculate average intervals
        foreach (var kvp in parameterTests)
        {
            if (kvp.Value.Count >= 2)
            {
                var totalDays = 0.0;
                for (int i = 1; i < kvp.Value.Count; i++)
                {
                    totalDays += (kvp.Value[i] - kvp.Value[i - 1]).TotalDays;
                }
                intervals[kvp.Key] = (int)Math.Round(totalDays / (kvp.Value.Count - 1));
            }
        }

        return intervals;
    }

    private void AddTestDate(Dictionary<string, List<DateTime>> dict, string parameter, DateTime date)
    {
        if (!dict.ContainsKey(parameter))
        {
            dict[parameter] = new List<DateTime>();
        }
        dict[parameter].Add(date);
    }

    private void AnalyzeTestingBehavior(TestScheduleRecommendation recommendation)
    {
        recommendation.IsTestingOptimal = true;
        recommendation.Suggestions = new List<string>();
        recommendation.Warnings = new List<string>();
        recommendation.ParametersNeedingMoreFrequentTesting = new List<ParameterTestSchedule>();
        recommendation.ParametersNeedingLessFrequentTesting = new List<ParameterTestSchedule>();

        if (recommendation.TotalTestsRecorded == 0)
        {
            recommendation.IsTestingOptimal = false;
            recommendation.Warnings.Add("No water tests recorded yet. Start testing immediately to monitor your tank's health.");
            recommendation.Suggestions.Add("Begin with daily testing of ammonia and nitrite for new tanks.");
            recommendation.Suggestions.Add("Create reminders for regular testing to establish a routine.");
            return;
        }

        var daysSinceLastTest = recommendation.LastTestDate.HasValue
            ? (DateTime.UtcNow - recommendation.LastTestDate.Value).Days
            : 9999;

        if (daysSinceLastTest > 14)
        {
            recommendation.IsTestingOptimal = false;
            recommendation.Warnings.Add($"It's been {daysSinceLastTest} days since your last water test. Regular testing is crucial for tank health.");
        }

        foreach (var param in recommendation.RecommendedSchedule)
        {
            if (recommendation.AverageTestingIntervals.TryGetValue(param.ParameterName, out var currentInterval))
            {
                param.CurrentFrequencyDays = currentInterval;

                if (param.IsRequired && currentInterval > param.RecommendedFrequencyDays * 1.5)
                {
                    recommendation.IsTestingOptimal = false;
                    recommendation.ParametersNeedingMoreFrequentTesting.Add(param);
                    recommendation.Suggestions.Add($"Test {param.ParameterDisplayName} more frequently. Currently testing every {currentInterval} days, recommended every {param.RecommendedFrequencyDays} days.");
                }
                else if (currentInterval < param.RecommendedFrequencyDays * 0.5 && !param.IsRequired)
                {
                    recommendation.ParametersNeedingLessFrequentTesting.Add(param);
                    recommendation.Suggestions.Add($"You're testing {param.ParameterDisplayName} very frequently. Consider reducing to every {param.RecommendedFrequencyDays} days unless there's a specific concern.");
                }
            }
            else if (param.IsRequired)
            {
                recommendation.IsTestingOptimal = false;
                recommendation.Warnings.Add($"{param.ParameterDisplayName} has never been tested. This is a {param.Importance.ToLower()} priority parameter.");
            }
        }

        // Maturity-specific suggestions
        if (recommendation.MaturityLevel == TankMaturityLevel.New || recommendation.MaturityLevel == TankMaturityLevel.Cycling)
        {
            recommendation.Suggestions.Add("Your tank is still cycling. Monitor ammonia and nitrite closely until they consistently read 0 ppm.");
        }

        if (recommendation.IsTestingOptimal)
        {
            recommendation.Suggestions.Add("Great job! Your testing frequency aligns well with recommendations for your tank's age and type.");
        }

        recommendation.CanCreateReminders = true;
    }

    public async Task<bool> CreateRemindersFromScheduleAsync(int tankId, string userId, List<string> selectedParameters)
    {
        try
        {
            var recommendation = await GetRecommendedScheduleAsync(tankId, userId);
            var tank = await _context.Tanks.FindAsync(tankId);

            if (tank == null) return false;

            foreach (var paramName in selectedParameters)
            {
                var param = recommendation.RecommendedSchedule.FirstOrDefault(p => p.ParameterName == paramName);
                if (param == null) continue;

                // Check if reminder already exists
                var existingReminder = await _context.Reminders
                    .FirstOrDefaultAsync(r => r.UserId == userId &&
                                            r.TankId == tankId &&
                                            r.Title.Contains(param.ParameterDisplayName) &&
                                            r.IsActive);

                if (existingReminder != null) continue;

                var reminder = new Reminder
                {
                    UserId = userId,
                    TankId = tankId,
                    Title = $"Test {param.ParameterDisplayName}",
                    Description = $"Recommended: {param.RecommendedFrequencyDescription}. {param.Reason}",
                    Type = ReminderType.WaterTest,
                    Frequency = ConvertDaysToFrequency(param.RecommendedFrequencyDays),
                    NextDueDate = DateTime.UtcNow.AddDays(param.RecommendedFrequencyDays),
                    IsActive = true,
                    SendEmailNotification = false,
                    NotificationHoursBefore = 24
                };

                _context.Reminders.Add(reminder);
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reminders from schedule for tank {TankId}", tankId);
            return false;
        }
    }

    private ReminderFrequency ConvertDaysToFrequency(int days)
    {
        return days switch
        {
            1 => ReminderFrequency.Daily,
            7 => ReminderFrequency.Weekly,
            14 => ReminderFrequency.BiWeekly,
            30 => ReminderFrequency.Monthly,
            _ => ReminderFrequency.Custom
        };
    }
}
