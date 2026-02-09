using AquaHub.MVC.Models;
using AquaHub.MVC.Models.ViewModels;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Services;

public class BreedingWaterAnalysisService : IBreedingWaterAnalysisService
{
    public BreedingWaterConditionViewModel AnalyzeWaterConditions(BreedingPair breedingPair, WaterTest? latestWaterTest)
    {
        var viewModel = new BreedingWaterConditionViewModel
        {
            BreedingPair = breedingPair,
            LatestWaterTest = latestWaterTest
        };

        // Check if ideal parameters are set
        viewModel.HasIdealParameters = HasAnyIdealParameters(breedingPair);

        if (!viewModel.HasIdealParameters || latestWaterTest == null)
        {
            viewModel.IsOptimal = false;
            return viewModel;
        }

        // Get parameter statuses
        viewModel.ParameterStatuses = GetParameterStatuses(breedingPair, latestWaterTest);

        // Count parameters out of range
        viewModel.ParametersOutOfRange = viewModel.ParameterStatuses
            .Count(p => p.Status == "Warning" || p.Status == "Critical");

        // Determine if conditions are optimal
        viewModel.IsOptimal = viewModel.ParametersOutOfRange == 0 &&
                             viewModel.ParameterStatuses.Any(p => p.Status == "Optimal");

        // Generate recommendations
        viewModel.Recommendations = GenerateRecommendations(breedingPair, latestWaterTest);

        return viewModel;
    }

    public List<WaterParameterStatus> GetParameterStatuses(BreedingPair breedingPair, WaterTest? waterTest)
    {
        var statuses = new List<WaterParameterStatus>();

        if (waterTest == null) return statuses;

        // Temperature
        if (breedingPair.IdealTempMin.HasValue || breedingPair.IdealTempMax.HasValue)
        {
            statuses.Add(CheckParameter(
                "Temperature",
                waterTest.Temperature,
                breedingPair.IdealTempMin,
                breedingPair.IdealTempMax,
                "°F",
                GetTemperatureRecommendation(waterTest.Temperature, breedingPair.IdealTempMin, breedingPair.IdealTempMax)
            ));
        }

        // pH
        if (breedingPair.IdealPhMin.HasValue || breedingPair.IdealPhMax.HasValue)
        {
            statuses.Add(CheckParameter(
                "pH",
                waterTest.PH,
                breedingPair.IdealPhMin,
                breedingPair.IdealPhMax,
                "",
                GetPhRecommendation(waterTest.PH, breedingPair.IdealPhMin, breedingPair.IdealPhMax)
            ));
        }

        // GH (General Hardness)
        if (breedingPair.IdealGhMin.HasValue || breedingPair.IdealGhMax.HasValue)
        {
            statuses.Add(CheckParameter(
                "GH (General Hardness)",
                waterTest.GH,
                breedingPair.IdealGhMin,
                breedingPair.IdealGhMax,
                "dGH",
                GetGhRecommendation(waterTest.GH, breedingPair.IdealGhMin, breedingPair.IdealGhMax)
            ));
        }

        // KH (Carbonate Hardness)
        if (breedingPair.IdealKhMin.HasValue || breedingPair.IdealKhMax.HasValue)
        {
            statuses.Add(CheckParameter(
                "KH (Carbonate Hardness)",
                waterTest.KH,
                breedingPair.IdealKhMin,
                breedingPair.IdealKhMax,
                "dKH",
                GetKhRecommendation(waterTest.KH, breedingPair.IdealKhMin, breedingPair.IdealKhMax)
            ));
        }

        // Ammonia (should be 0 or below max)
        if (breedingPair.MaxAmmonia.HasValue)
        {
            var ammoniaStatus = new WaterParameterStatus
            {
                ParameterName = "Ammonia",
                CurrentValue = waterTest.Ammonia,
                IdealMax = breedingPair.MaxAmmonia,
                Unit = "ppm"
            };

            if (waterTest.Ammonia.HasValue)
            {
                if (waterTest.Ammonia.Value == 0)
                {
                    ammoniaStatus.Status = "Optimal";
                    ammoniaStatus.Recommendation = "Ammonia levels are ideal for breeding.";
                }
                else if (waterTest.Ammonia.Value <= breedingPair.MaxAmmonia.Value)
                {
                    ammoniaStatus.Status = "Warning";
                    ammoniaStatus.Recommendation = $"Ammonia detected ({waterTest.Ammonia.Value} ppm). Perform water change and check filter.";
                }
                else
                {
                    ammoniaStatus.Status = "Critical";
                    ammoniaStatus.Recommendation = $"Ammonia too high ({waterTest.Ammonia.Value} ppm)! Immediate 50% water change needed. Do not attempt breeding.";
                }
            }
            else
            {
                ammoniaStatus.Status = "Unknown";
            }

            statuses.Add(ammoniaStatus);
        }

        // Nitrite (should be 0 or below max)
        if (breedingPair.MaxNitrite.HasValue)
        {
            var nitriteStatus = new WaterParameterStatus
            {
                ParameterName = "Nitrite",
                CurrentValue = waterTest.Nitrite,
                IdealMax = breedingPair.MaxNitrite,
                Unit = "ppm"
            };

            if (waterTest.Nitrite.HasValue)
            {
                if (waterTest.Nitrite.Value == 0)
                {
                    nitriteStatus.Status = "Optimal";
                    nitriteStatus.Recommendation = "Nitrite levels are ideal for breeding.";
                }
                else if (waterTest.Nitrite.Value <= breedingPair.MaxNitrite.Value)
                {
                    nitriteStatus.Status = "Warning";
                    nitriteStatus.Recommendation = $"Nitrite detected ({waterTest.Nitrite.Value} ppm). Check biological filtration and perform water change.";
                }
                else
                {
                    nitriteStatus.Status = "Critical";
                    nitriteStatus.Recommendation = $"Nitrite too high ({waterTest.Nitrite.Value} ppm)! Tank not cycled properly. Do not breed until resolved.";
                }
            }
            else
            {
                nitriteStatus.Status = "Unknown";
            }

            statuses.Add(nitriteStatus);
        }

        // Nitrate (should be below max)
        if (breedingPair.MaxNitrate.HasValue)
        {
            var nitrateStatus = new WaterParameterStatus
            {
                ParameterName = "Nitrate",
                CurrentValue = waterTest.Nitrate,
                IdealMax = breedingPair.MaxNitrate,
                Unit = "ppm"
            };

            if (waterTest.Nitrate.HasValue)
            {
                if (waterTest.Nitrate.Value <= breedingPair.MaxNitrate.Value * 0.5)
                {
                    nitrateStatus.Status = "Optimal";
                    nitrateStatus.Recommendation = "Nitrate levels are excellent for breeding.";
                }
                else if (waterTest.Nitrate.Value <= breedingPair.MaxNitrate.Value)
                {
                    nitrateStatus.Status = "Warning";
                    nitrateStatus.Recommendation = $"Nitrate slightly elevated ({waterTest.Nitrate.Value} ppm). Consider 25-30% water change before breeding.";
                }
                else
                {
                    nitrateStatus.Status = "Critical";
                    nitrateStatus.Recommendation = $"Nitrate too high ({waterTest.Nitrate.Value} ppm). Perform larger water changes to reduce below {breedingPair.MaxNitrate.Value} ppm.";
                }
            }
            else
            {
                nitrateStatus.Status = "Unknown";
            }

            statuses.Add(nitrateStatus);
        }

        return statuses;
    }

    public List<string> GenerateRecommendations(BreedingPair breedingPair, WaterTest? waterTest)
    {
        var recommendations = new List<string>();

        if (waterTest == null)
        {
            recommendations.Add("⚠️ No water test data available. Test water parameters before attempting to breed.");
            return recommendations;
        }

        var statuses = GetParameterStatuses(breedingPair, waterTest);
        var criticalParams = statuses.Where(s => s.Status == "Critical").ToList();
        var warningParams = statuses.Where(s => s.Status == "Warning").ToList();

        // Critical issues first
        if (criticalParams.Any())
        {
            recommendations.Add("🚨 CRITICAL: Do not attempt breeding until these issues are resolved:");
            foreach (var param in criticalParams)
            {
                if (!string.IsNullOrEmpty(param.Recommendation))
                {
                    recommendations.Add($"   • {param.ParameterName}: {param.Recommendation}");
                }
            }
        }

        // Warning issues
        if (warningParams.Any())
        {
            recommendations.Add("⚠️ WARNING: Address these issues for optimal breeding success:");
            foreach (var param in warningParams)
            {
                if (!string.IsNullOrEmpty(param.Recommendation))
                {
                    recommendations.Add($"   • {param.ParameterName}: {param.Recommendation}");
                }
            }
        }

        // All optimal
        if (!criticalParams.Any() && !warningParams.Any() && statuses.Any(s => s.Status == "Optimal"))
        {
            recommendations.Add("✅ Water conditions are optimal for breeding!");
            recommendations.Add("💡 Continue monitoring parameters daily during conditioning and breeding.");
            recommendations.Add("💡 Increase feeding frequency with high-quality foods for conditioning.");
            recommendations.Add("💡 Maintain stable conditions - avoid sudden changes.");
        }

        // Age of test
        var testAge = (DateTime.Now - waterTest.Timestamp).TotalDays;
        if (testAge > 3)
        {
            recommendations.Add($"📅 Water test is {testAge:F0} days old. Test again before breeding for current conditions.");
        }

        return recommendations;
    }

    private bool HasAnyIdealParameters(BreedingPair breedingPair)
    {
        return breedingPair.IdealTempMin.HasValue || breedingPair.IdealTempMax.HasValue ||
               breedingPair.IdealPhMin.HasValue || breedingPair.IdealPhMax.HasValue ||
               breedingPair.IdealGhMin.HasValue || breedingPair.IdealGhMax.HasValue ||
               breedingPair.IdealKhMin.HasValue || breedingPair.IdealKhMax.HasValue ||
               breedingPair.MaxAmmonia.HasValue || breedingPair.MaxNitrite.HasValue ||
               breedingPair.MaxNitrate.HasValue;
    }

    private WaterParameterStatus CheckParameter(string name, double? current, double? min, double? max, string unit, string? recommendation)
    {
        var status = new WaterParameterStatus
        {
            ParameterName = name,
            CurrentValue = current,
            IdealMin = min,
            IdealMax = max,
            Unit = unit,
            Recommendation = recommendation
        };

        if (!current.HasValue)
        {
            status.Status = "Unknown";
            return status;
        }

        // Check if within range
        bool inRange = true;
        bool nearEdge = false;

        if (min.HasValue && current.Value < min.Value)
        {
            inRange = false;
            var difference = min.Value - current.Value;
            status.Status = difference > (min.Value * 0.1) ? "Critical" : "Warning";
        }
        else if (max.HasValue && current.Value > max.Value)
        {
            inRange = false;
            var difference = current.Value - max.Value;
            status.Status = difference > (max.Value * 0.1) ? "Critical" : "Warning";
        }
        else if (min.HasValue && max.HasValue)
        {
            // Check if near edges (within 10% of range)
            var range = max.Value - min.Value;
            var tolerance = range * 0.1;

            if (current.Value < min.Value + tolerance || current.Value > max.Value - tolerance)
            {
                nearEdge = true;
            }
        }

        if (inRange)
        {
            status.Status = nearEdge ? "Warning" : "Optimal";
        }

        return status;
    }

    private string? GetTemperatureRecommendation(double? current, double? min, double? max)
    {
        if (!current.HasValue) return "Temperature not tested. Measure before breeding.";

        if (min.HasValue && current.Value < min.Value)
        {
            return $"Temperature too low. Increase heater setting to reach {min.Value}°F - {max?.ToString() ?? "target"}°F. Adjust slowly (1-2°F per day).";
        }

        if (max.HasValue && current.Value > max.Value)
        {
            return $"Temperature too high. Reduce heater or add cooling to reach {min?.ToString() ?? "target"}°F - {max.Value}°F. Adjust slowly.";
        }

        return "Temperature is within ideal range for breeding.";
    }

    private string? GetPhRecommendation(double? current, double? min, double? max)
    {
        if (!current.HasValue) return "pH not tested. Measure before breeding.";

        if (min.HasValue && current.Value < min.Value)
        {
            var difference = min.Value - current.Value;
            if (difference > 0.5)
            {
                return $"pH too low ({current.Value}). Add crushed coral, limestone, or baking soda slowly. Target: {min.Value}-{max?.ToString() ?? "higher"}. Adjust gradually!";
            }
            return $"pH slightly low. Small water changes with conditioned tap water may help if tap pH is higher.";
        }

        if (max.HasValue && current.Value > max.Value)
        {
            var difference = current.Value - max.Value;
            if (difference > 0.5)
            {
                return $"pH too high ({current.Value}). Use driftwood, peat, or pH down products. Target: {min?.ToString() ?? "lower"}-{max.Value}. Adjust gradually!";
            }
            return $"pH slightly high. Water changes with RO water or pH-lowering products may help.";
        }

        return "pH is within ideal range for breeding.";
    }

    private string? GetGhRecommendation(double? current, double? min, double? max)
    {
        if (!current.HasValue) return "GH not tested. Measure before breeding.";

        if (min.HasValue && current.Value < min.Value)
        {
            return $"Water too soft. Add GH booster, crushed coral, or wonder shells to increase hardness to {min.Value}-{max?.ToString() ?? "target"} dGH.";
        }

        if (max.HasValue && current.Value > max.Value)
        {
            return $"Water too hard. Use RO water or distilled water in water changes to reduce GH to {min?.ToString() ?? "target"}-{max.Value} dGH.";
        }

        return "General hardness is within ideal range.";
    }

    private string? GetKhRecommendation(double? current, double? min, double? max)
    {
        if (!current.HasValue) return "KH not tested. Measure before breeding.";

        if (min.HasValue && current.Value < min.Value)
        {
            return $"KH too low. Add baking soda or KH buffer to increase stability. Target: {min.Value}-{max?.ToString() ?? "target"} dKH for stable pH.";
        }

        if (max.HasValue && current.Value > max.Value)
        {
            return $"KH too high. Use RO water in changes to reduce carbonate hardness to {min?.ToString() ?? "target"}-{max.Value} dKH.";
        }

        return "Carbonate hardness is within ideal range.";
    }
}
