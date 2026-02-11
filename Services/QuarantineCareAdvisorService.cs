using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Services;

public class QuarantineCareAdvisorService : IQuarantineCareAdvisorService
{
    private readonly ILogger<QuarantineCareAdvisorService> _logger;

    public QuarantineCareAdvisorService(ILogger<QuarantineCareAdvisorService> logger)
    {
        _logger = logger;
    }

    public async Task<QuarantineCareRecommendations> AnalyzeQuarantineConditionsAsync(
        Tank tank,
        List<WaterTest> recentWaterTests,
        List<DosingRecord> recentDosing,
        List<MaintenanceLog> recentMaintenance,
        List<Livestock> quarantinedLivestock)
    {
        var recommendations = new QuarantineCareRecommendations();

        try
        {
            var latestTest = recentWaterTests.OrderByDescending(wt => wt.Timestamp).FirstOrDefault();
            var daysInQuarantine = tank.QuarantineStartDate.HasValue
                ? (DateTime.Now - tank.QuarantineStartDate.Value).Days
                : 0;

            // Analyze water chemistry
            recommendations.WaterChemistryInsights = AnalyzeWaterChemistryTrends(recentWaterTests);

            // Evaluate dosing effectiveness
            recommendations.DosingRecommendations = EvaluateDosingProtocol(recentDosing, recentWaterTests);

            // Duration-based recommendations
            var durationRecs = GetDurationBasedRecommendations(
                tank.QuarantineStartDate,
                tank.QuarantineEndDate,
                daysInQuarantine);
            recommendations.NextSteps.AddRange(durationRecs);

            // Maintenance recommendations
            var lastWaterChange = recentMaintenance
                .Where(m => m.Type == Models.Enums.MaintenanceType.WaterChange)
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefault();
            var daysSinceWaterChange = lastWaterChange != null
                ? (DateTime.Now - lastWaterChange.Timestamp).Days
                : 999;

            recommendations.MaintenanceActions = GetMaintenanceRecommendations(
                daysSinceWaterChange,
                recentWaterTests,
                daysInQuarantine);

            // Next steps suggestions
            recommendations.NextSteps.AddRange(
                SuggestNextSteps(tank, latestTest, daysSinceWaterChange));

            // Monitoring priorities
            recommendations.MonitoringPriorities = DetermineMonitoringPriorities(
                latestTest,
                daysInQuarantine,
                recentWaterTests);

            // Treatment guidance based on purpose
            recommendations.TreatmentGuidance = GetTreatmentGuidance(
                tank.QuarantinePurpose,
                latestTest,
                daysInQuarantine);

            // Overall assessment and risk level
            (recommendations.OverallAssessment, recommendations.RiskLevel, recommendations.RequiresImmediateAction) =
                GenerateOverallAssessment(latestTest, recentWaterTests, daysInQuarantine, recentDosing);

            recommendations.GeneratedAt = DateTime.Now;

            _logger.LogInformation(
                "Generated quarantine care recommendations for tank {TankId}. Risk Level: {RiskLevel}",
                tank.Id,
                recommendations.RiskLevel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating quarantine care recommendations for tank {TankId}", tank.Id);
            recommendations.OverallAssessment = "Unable to generate recommendations due to an error.";
            recommendations.RiskLevel = "Unknown";
        }

        return recommendations;
    }

    public List<string> AnalyzeWaterChemistryTrends(List<WaterTest> waterTests)
    {
        var insights = new List<string>();

        if (!waterTests.Any())
        {
            insights.Add("⚠️ No water test data available. Testing is critical during quarantine.");
            return insights;
        }

        var orderedTests = waterTests.OrderBy(wt => wt.Timestamp).ToList();
        var latestTest = orderedTests.Last();

        // Ammonia trend analysis
        if (orderedTests.Count >= 2)
        {
            var ammoniaValues = orderedTests.Select(t => t.Ammonia ?? 0).ToList();
            var ammoniaTrend = CalculateTrend(ammoniaValues);

            if (latestTest.Ammonia > 0.25)
            {
                insights.Add($"🔴 CRITICAL: Ammonia is {latestTest.Ammonia:F2} ppm. " +
                    (ammoniaTrend > 0 ? "Rising trend detected - increase water changes immediately!" :
                     ammoniaTrend < 0 ? "Decreasing trend - current treatment is working." :
                     "Stable but elevated - maintain current water change schedule."));
            }
            else if (latestTest.Ammonia > 0)
            {
                insights.Add($"⚠️ Ammonia detected ({latestTest.Ammonia:F2} ppm). " +
                    "Continue monitoring closely and maintain frequent water changes.");
            }
            else
            {
                insights.Add("✅ Ammonia levels are optimal (0 ppm). Biological filtration is functioning well.");
            }

            // Nitrite trend analysis
            var nitriteValues = orderedTests.Select(t => t.Nitrite ?? 0).ToList();
            var nitriteTrend = CalculateTrend(nitriteValues);

            if (latestTest.Nitrite > 0.25)
            {
                insights.Add($"🔴 CRITICAL: Nitrite is {latestTest.Nitrite:F2} ppm. " +
                    (nitriteTrend > 0 ? "Rising trend - nitrogen cycle may be stalled!" :
                     "Present but decreasing - continue current treatment."));
            }
            else if (latestTest.Nitrite > 0)
            {
                insights.Add($"⚠️ Nitrite detected ({latestTest.Nitrite:F2} ppm). Tank is cycling - increase aeration.");
            }
            else
            {
                insights.Add("✅ Nitrite levels are safe (0 ppm). Nitrogen cycle is established.");
            }

            // Nitrate trend analysis
            var nitrateValues = orderedTests.Select(t => t.Nitrate ?? 0).ToList();
            var nitrateTrend = CalculateTrend(nitrateValues);

            if (latestTest.Nitrate > 80)
            {
                insights.Add($"🔴 Nitrate is very high ({latestTest.Nitrate:F0} ppm). " +
                    "Large water change (50%+) recommended to reduce stress.");
            }
            else if (latestTest.Nitrate > 40)
            {
                insights.Add($"⚠️ Nitrate is elevated ({latestTest.Nitrate:F0} ppm). " +
                    (nitrateTrend > 0 ? "Increasing - more frequent water changes needed." :
                     "Consider 25-30% water change to bring levels down."));
            }
            else
            {
                insights.Add($"✅ Nitrate is at safe levels ({latestTest.Nitrate:F0} ppm).");
            }
        }

        // Temperature analysis
        if (latestTest.Temperature.HasValue)
        {
            if (latestTest.Temperature < 72)
            {
                insights.Add($"🌡️ Temperature is low ({latestTest.Temperature:F1}°F). " +
                    "Increase heater setting - low temps can suppress immune function.");
            }
            else if (latestTest.Temperature > 82)
            {
                insights.Add($"🌡️ Temperature is high ({latestTest.Temperature:F1}°F). " +
                    "Reduce temperature - high temps accelerate disease progression.");
            }
            else if (latestTest.Temperature >= 76 && latestTest.Temperature <= 78)
            {
                insights.Add($"✅ Temperature is optimal ({latestTest.Temperature:F1}°F) for recovery.");
            }
            else
            {
                insights.Add($"✔️ Temperature is acceptable ({latestTest.Temperature:F1}°F).");
            }
        }

        // pH stability analysis
        if (orderedTests.Count >= 3)
        {
            var phValues = orderedTests.Where(t => t.PH.HasValue).Select(t => t.PH!.Value).ToList();
            if (phValues.Any())
            {
                var phStdDev = CalculateStandardDeviation(phValues);
                if (phStdDev > 0.3)
                {
                    insights.Add("⚠️ pH is fluctuating significantly. Unstable pH causes stress - " +
                        "check KH/alkalinity and avoid rapid water changes.");
                }
                else
                {
                    insights.Add("✅ pH is stable, which reduces stress during treatment.");
                }
            }
        }

        return insights;
    }

    public List<string> EvaluateDosingProtocol(List<DosingRecord> dosingRecords, List<WaterTest> waterTests)
    {
        var recommendations = new List<string>();

        if (!dosingRecords.Any())
        {
            recommendations.Add("ℹ️ No dosing records found. If treating disease, ensure medications are logged.");
            return recommendations;
        }

        var recentDoses = dosingRecords.OrderByDescending(d => d.Timestamp).Take(10).ToList();
        var medicationTypes = recentDoses.Select(d => d.Additive.ToLower()).Distinct().ToList();

        // Check dosing frequency
        var lastDose = recentDoses.First();
        var hoursSinceLastDose = (DateTime.Now - lastDose.Timestamp).TotalHours;

        if (hoursSinceLastDose > 24)
        {
            recommendations.Add($"⏰ Last dose was {hoursSinceLastDose:F0} hours ago. " +
                "Verify if additional dosing is scheduled or treatment is complete.");
        }

        // Medication-specific guidance
        foreach (var med in medicationTypes)
        {
            if (med.Contains("copper") || med.Contains("cupramine"))
            {
                var latestTest = waterTests.OrderByDescending(wt => wt.Timestamp).FirstOrDefault();
                recommendations.Add("🔬 Copper Treatment Detected:");
                recommendations.Add("  • Test copper levels daily (maintain 0.15-0.25 ppm for saltwater)");
                recommendations.Add("  • Remove carbon filtration (absorbs copper)");
                recommendations.Add("  • Monitor fish closely for stress (rapid breathing, hiding)");
                recommendations.Add("  • Typical treatment: 14-21 days minimum");

                if (latestTest != null && latestTest.Ammonia > 0)
                {
                    recommendations.Add("  ⚠️ Ammonia present with copper treatment - very dangerous! " +
                        "Water change needed but will dilute copper. Re-dose after water change.");
                }
            }
            else if (med.Contains("prazi") || med.Contains("praziquantel"))
            {
                recommendations.Add("💊 Praziquantel Treatment Detected:");
                recommendations.Add("  • Single dose or 3-dose protocol depending on parasites");
                recommendations.Add("  • Safe for inverts and most fish");
                recommendations.Add("  • Monitor for improvement in 3-5 days");
                recommendations.Add("  • Consider second dose in 7 days if symptoms persist");
            }
            else if (med.Contains("metroplex") || med.Contains("metronidazole"))
            {
                recommendations.Add("💊 Metronidazole Treatment Detected:");
                recommendations.Add("  • Effective for internal parasites and hole-in-head disease");
                recommendations.Add("  • Dose daily for 3 days, then reassess");
                recommendations.Add("  • Can be combined with focus for better absorption");
                recommendations.Add("  • Watch for improved appetite and behavior");
            }
            else if (med.Contains("kanaplex") || med.Contains("kanamycin"))
            {
                recommendations.Add("💊 Kanamycin Treatment Detected:");
                recommendations.Add("  • Bacterial infection treatment - monitor for improvement in 48-72 hours");
                recommendations.Add("  • May impact biological filtration - test ammonia daily");
                recommendations.Add("  • Continue for full course (usually 5-7 days)");
                recommendations.Add("  • Increase aeration during treatment");
            }
        }

        // Treatment duration analysis
        var firstDose = recentDoses.OrderBy(d => d.Timestamp).FirstOrDefault();
        if (firstDose != null)
        {
            var treatmentDays = (DateTime.Now - firstDose.Timestamp).Days;

            if (treatmentDays >= 14 && treatmentDays < 21)
            {
                recommendations.Add($"📅 Day {treatmentDays} of treatment. Monitor for signs of improvement:");
                recommendations.Add("  • Improved appetite");
                recommendations.Add("  • Normal swimming behavior");
                recommendations.Add("  • Reduced visible symptoms");
                recommendations.Add("  • Active and alert");
            }
            else if (treatmentDays >= 21)
            {
                recommendations.Add($"📅 Day {treatmentDays} of treatment. Consider:");
                recommendations.Add("  • If improved: Begin planning reintroduction protocol");
                recommendations.Add("  • If not improved: Reassess diagnosis and treatment plan");
                recommendations.Add("  • Consult with veterinarian or experienced aquarist");
            }
        }

        return recommendations;
    }

    public List<string> GetDurationBasedRecommendations(DateTime? startDate, DateTime? endDate, int daysElapsed)
    {
        var recommendations = new List<string>();

        if (!startDate.HasValue)
        {
            return recommendations;
        }

        // Early quarantine (Days 1-7)
        if (daysElapsed <= 7)
        {
            recommendations.Add($"📍 Early Quarantine Phase (Day {daysElapsed}):");
            recommendations.Add("  • Test water parameters daily (ammonia, nitrite, nitrate)");
            recommendations.Add("  • Observe closely for signs of disease or stress");
            recommendations.Add("  • Maintain stable temperature and pH");
            recommendations.Add("  • Small frequent water changes (10-15% daily) if needed");
            recommendations.Add("  • Document baseline behavior and appetite");
        }
        // Middle quarantine (Days 8-21)
        else if (daysElapsed <= 21)
        {
            recommendations.Add($"📍 Mid Quarantine Phase (Day {daysElapsed}):");
            recommendations.Add("  • Continue monitoring but can reduce testing to every 2-3 days");
            recommendations.Add("  • Any diseases should be showing symptoms by now");
            recommendations.Add("  • If treating, evaluate treatment effectiveness");
            recommendations.Add("  • Regular water changes (25-30% twice weekly)");
            recommendations.Add("  • Ensure fish is eating well and behaving normally");
        }
        // Late quarantine (Days 22-30)
        else if (daysElapsed <= 30)
        {
            recommendations.Add($"📍 Late Quarantine Phase (Day {daysElapsed}):");
            recommendations.Add("  • Final observation period - watch for any late-emerging issues");
            recommendations.Add("  • If healthy, begin planning reintroduction");
            recommendations.Add("  • Match main tank parameters (temperature, salinity, pH)");
            recommendations.Add("  • Consider prophylactic treatment if not already done");
            recommendations.Add("  • Prepare acclimation plan for transfer");
        }
        // Extended quarantine (30+ days)
        else
        {
            recommendations.Add($"📍 Extended Quarantine (Day {daysElapsed}):");

            if (endDate.HasValue && DateTime.Now > endDate.Value)
            {
                recommendations.Add("  ⏰ Scheduled end date has passed. Evaluate:");
                recommendations.Add("  • Is fish completely healthy with no symptoms?");
                recommendations.Add("  • Are water parameters stable and ideal?");
                recommendations.Add("  • Is fish eating normally and active?");
                recommendations.Add("  • If yes to all: Safe to begin reintroduction process");
                recommendations.Add("  • If no: Extend quarantine until fully recovered");
            }
            else
            {
                recommendations.Add("  • Extended quarantine may indicate ongoing treatment");
                recommendations.Add("  • Reassess: Is extended time necessary or can reintroduction begin?");
                recommendations.Add("  • Ensure treatment goals are being met");
                recommendations.Add("  • Monitor for medication fatigue or stress from isolation");
            }
        }

        return recommendations;
    }

    public List<string> SuggestNextSteps(Tank tank, WaterTest? latestTest, int daysSinceLastWaterChange)
    {
        var steps = new List<string>();

        // Water change recommendations
        if (daysSinceLastWaterChange > 7)
        {
            steps.Add("🚰 IMMEDIATE: Perform a 25-30% water change (overdue)");
        }
        else if (daysSinceLastWaterChange > 3)
        {
            steps.Add("🚰 Schedule a water change within 1-2 days");
        }

        // Testing recommendations
        if (latestTest == null || (DateTime.Now - latestTest.Timestamp).Days > 3)
        {
            steps.Add("🔬 Test water parameters (especially ammonia, nitrite, nitrate)");
        }

        // Parameter-specific actions
        if (latestTest != null)
        {
            if (latestTest.Ammonia > 0.25 || latestTest.Nitrite > 0.25)
            {
                steps.Add("⚠️ Test again in 24 hours to monitor toxic parameter trends");
                steps.Add("🚰 Consider additional water change if levels don't improve");
            }

            if (latestTest.Nitrate > 40)
            {
                steps.Add("📉 Increase water change frequency to reduce nitrate buildup");
            }
        }

        // General quarantine steps
        steps.Add("👁️ Observe livestock behavior, appetite, and physical appearance daily");
        steps.Add("📝 Document any changes in symptoms or behavior");
        steps.Add("🌡️ Verify temperature and salinity remain stable");

        return steps;
    }

    private List<string> GetMaintenanceRecommendations(
        int daysSinceWaterChange,
        List<WaterTest> waterTests,
        int daysInQuarantine)
    {
        var recommendations = new List<string>();

        // Water change frequency
        if (daysInQuarantine <= 7)
        {
            recommendations.Add("💧 Early quarantine: 10-15% daily water changes recommended");
            recommendations.Add("   This helps remove waste and medication metabolites");
        }
        else if (daysInQuarantine <= 21)
        {
            recommendations.Add("💧 Mid quarantine: 25-30% water changes 2-3x per week");
            recommendations.Add("   Maintain consistent schedule for stability");
        }
        else
        {
            recommendations.Add("💧 Late quarantine: 25% weekly water changes sufficient");
            recommendations.Add("   Focus on stability rather than aggressive changes");
        }

        // Filter maintenance
        recommendations.Add("🧽 Clean mechanical filter media weekly (rinse in old tank water)");
        recommendations.Add("🔄 Monitor filter flow rate - reduced flow = reduced oxygenation");

        // Aeration
        var latestTest = waterTests.OrderByDescending(wt => wt.Timestamp).FirstOrDefault();
        if (latestTest != null && (latestTest.Ammonia > 0 || latestTest.Nitrite > 0))
        {
            recommendations.Add("💨 INCREASE AERATION: Toxic parameters detected - add airstone");
        }

        return recommendations;
    }

    private List<string> DetermineMonitoringPriorities(
        WaterTest? latestTest,
        int daysInQuarantine,
        List<WaterTest> recentTests)
    {
        var priorities = new List<string>();

        if (latestTest == null)
        {
            priorities.Add("🔴 HIGH PRIORITY: No water test data - test immediately");
            return priorities;
        }

        // Critical parameters
        if (latestTest.Ammonia > 0.25 || latestTest.Nitrite > 0.25)
        {
            priorities.Add("🔴 CRITICAL: Test ammonia and nitrite daily until 0 ppm");
        }

        // High priority
        if (daysInQuarantine <= 7)
        {
            priorities.Add("🟠 HIGH: Test all parameters daily during first week");
        }

        if (latestTest.Nitrate > 40)
        {
            priorities.Add("🟠 HIGH: Monitor nitrate - reduce through water changes");
        }

        // Medium priority
        if (daysInQuarantine > 7)
        {
            priorities.Add("🟡 MEDIUM: Test parameters every 2-3 days");
        }

        priorities.Add("🟡 MEDIUM: Daily visual health checks of livestock");
        priorities.Add("🟡 MEDIUM: Monitor feeding response and appetite");

        // Low priority
        priorities.Add("🟢 LOW: Weekly full parameter panel test");
        priorities.Add("🟢 LOW: Photo documentation of progress");

        return priorities;
    }

    private Dictionary<string, string> GetTreatmentGuidance(
        string quarantinePurpose,
        WaterTest? latestTest,
        int daysInQuarantine)
    {
        var guidance = new Dictionary<string, string>();

        var purpose = quarantinePurpose?.ToLower() ?? "";

        if (purpose.Contains("treatment") || purpose.Contains("disease") || purpose.Contains("sick"))
        {
            guidance["Primary Goal"] = "Disease Treatment & Recovery";
            guidance["Success Indicators"] = "Visible symptom reduction, improved appetite, normal behavior";
            guidance["Typical Duration"] = "14-30 days depending on condition severity";
            guidance["Water Quality"] = "CRITICAL - pristine water reduces stress and aids recovery";
            guidance["Feeding"] = "Offer small amounts of high-quality food 2-3x daily";
        }
        else if (purpose.Contains("observation") || purpose.Contains("new"))
        {
            guidance["Primary Goal"] = "Observation & Disease Prevention";
            guidance["Success Indicators"] = "No disease symptoms after 28 days";
            guidance["Typical Duration"] = "28-30 days minimum";
            guidance["Water Quality"] = "Maintain stable, ideal parameters";
            guidance["Feeding"] = "Normal feeding schedule to assess appetite and health";
        }
        else if (purpose.Contains("acclimation") || purpose.Contains("stress"))
        {
            guidance["Primary Goal"] = "Stress Reduction & Acclimation";
            guidance["Success Indicators"] = "Eating well, exploring tank, reduced hiding";
            guidance["Typical Duration"] = "14-21 days";
            guidance["Water Quality"] = "Match eventual display tank parameters";
            guidance["Feeding"] = "Start with small amounts, gradually increase";
        }
        else
        {
            guidance["Primary Goal"] = "Health Monitoring";
            guidance["Success Indicators"] = "Consistent good health indicators";
            guidance["Typical Duration"] = "Minimum 14 days";
            guidance["Water Quality"] = "Maintain stable optimal parameters";
        }

        // Add phase-specific guidance
        if (daysInQuarantine <= 7)
        {
            guidance["Current Phase"] = "Initial - High monitoring, frequent testing";
        }
        else if (daysInQuarantine <= 21)
        {
            guidance["Current Phase"] = "Middle - Continue treatment, monitor progress";
        }
        else
        {
            guidance["Current Phase"] = "Final - Prepare for transition or complete treatment";
        }

        return guidance;
    }

    private (string assessment, string riskLevel, bool requiresAction) GenerateOverallAssessment(
        WaterTest? latestTest,
        List<WaterTest> recentTests,
        int daysInQuarantine,
        List<DosingRecord> recentDosing)
    {
        var criticalIssues = new List<string>();
        var warnings = new List<string>();

        // Assess water chemistry
        if (latestTest == null)
        {
            criticalIssues.Add("No water test data");
        }
        else
        {
            if (latestTest.Ammonia > 0.5)
                criticalIssues.Add($"Critical ammonia level ({latestTest.Ammonia:F2} ppm)");
            else if (latestTest.Ammonia > 0.25)
                warnings.Add($"Elevated ammonia ({latestTest.Ammonia:F2} ppm)");

            if (latestTest.Nitrite > 0.5)
                criticalIssues.Add($"Critical nitrite level ({latestTest.Nitrite:F2} ppm)");
            else if (latestTest.Nitrite > 0.25)
                warnings.Add($"Elevated nitrite ({latestTest.Nitrite:F2} ppm)");

            if (latestTest.Nitrate > 100)
                criticalIssues.Add($"Very high nitrate ({latestTest.Nitrate:F0} ppm)");
            else if (latestTest.Nitrate > 60)
                warnings.Add($"High nitrate ({latestTest.Nitrate:F0} ppm)");

            if (latestTest.Temperature < 70 || latestTest.Temperature > 84)
                warnings.Add($"Temperature out of ideal range ({latestTest.Temperature:F1}°F)");
        }

        // Assess quarantine duration
        if (daysInQuarantine >= 35)
        {
            warnings.Add("Extended quarantine - verify if additional time is necessary");
        }

        // Determine risk level and assessment
        string riskLevel;
        string assessment;
        bool requiresAction;

        if (criticalIssues.Any())
        {
            riskLevel = "Critical";
            assessment = $"⚠️ IMMEDIATE ACTION REQUIRED: {string.Join(", ", criticalIssues)}. " +
                "Perform emergency water change and retest within 24 hours.";
            requiresAction = true;
        }
        else if (warnings.Count >= 2)
        {
            riskLevel = "High";
            assessment = $"⚠️ Multiple concerns detected: {string.Join(", ", warnings)}. " +
                "Increase monitoring frequency and address issues promptly.";
            requiresAction = true;
        }
        else if (warnings.Any())
        {
            riskLevel = "Medium";
            assessment = $"⚠️ {warnings.First()}. Monitor closely and take corrective action. " +
                "Continue with current care protocol.";
            requiresAction = false;
        }
        else
        {
            riskLevel = "Low";
            assessment = $"✅ Quarantine conditions are optimal. Day {daysInQuarantine} - " +
                "Continue current care protocol. All parameters within safe ranges.";
            requiresAction = false;
        }

        return (assessment, riskLevel, requiresAction);
    }

    // Helper methods for statistical analysis
    private double CalculateTrend(List<double> values)
    {
        if (values.Count < 2) return 0;

        var x = Enumerable.Range(0, values.Count).Select(i => (double)i).ToList();
        var y = values;

        var xMean = x.Average();
        var yMean = y.Average();

        var numerator = x.Zip(y, (xi, yi) => (xi - xMean) * (yi - yMean)).Sum();
        var denominator = x.Select(xi => Math.Pow(xi - xMean, 2)).Sum();

        return denominator != 0 ? numerator / denominator : 0;
    }

    private double CalculateStandardDeviation(List<double> values)
    {
        if (!values.Any()) return 0;

        var mean = values.Average();
        var sumOfSquares = values.Select(v => Math.Pow(v - mean, 2)).Sum();
        return Math.Sqrt(sumOfSquares / values.Count);
    }
}
