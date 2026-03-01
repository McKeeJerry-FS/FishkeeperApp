using System;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;
using AquaHub.MVC.Models.ViewModels;
using AquaHub.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class WaterTestService : IWaterTestService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService? _notificationService;
    private readonly IParameterAlertService? _parameterAlertService;

    public WaterTestService(
        ApplicationDbContext context,
        INotificationService? notificationService = null,
        IParameterAlertService? parameterAlertService = null)
    {
        _context = context;
        _notificationService = notificationService;
        _parameterAlertService = parameterAlertService;
    }

    public async Task<List<WaterTest>> GetAllWaterTestsAsync(string userId)
    {
        return await _context.WaterTests
            .Include(w => w.Tank)
            .Where(w => w.Tank!.UserId == userId)
            .OrderByDescending(w => w.Timestamp)
            .ToListAsync();
    }

    public async Task<List<WaterTest>> GetWaterTestsByTankAsync(int tankId, string userId)
    {
        return await _context.WaterTests
            .Include(w => w.Tank)
            .Where(w => w.TankId == tankId && w.Tank!.UserId == userId)
            .OrderByDescending(w => w.Timestamp)
            .ToListAsync();
    }

    public async Task<WaterTest?> GetWaterTestByIdAsync(int id, string userId)
    {
        return await _context.WaterTests
            .Include(w => w.Tank)
            .Where(w => w.Tank!.UserId == userId)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<WaterTest> CreateWaterTestAsync(WaterTest waterTest, int tankId, string userId)
    {
        // Verify tank ownership
        var tank = await _context.Tanks
            .FirstOrDefaultAsync(t => t.Id == tankId && t.UserId == userId);

        if (tank == null)
        {
            throw new UnauthorizedAccessException("You don't have permission to add water tests to this tank.");
        }

        waterTest.TankId = tankId;
        waterTest.Timestamp = DateTime.UtcNow;

        _context.WaterTests.Add(waterTest);
        await _context.SaveChangesAsync();

        // Check for out-of-range parameters and create notifications
        await CheckWaterParametersAsync(waterTest, tank);

        return waterTest;
    }

    public async Task<WaterTest> UpdateWaterTestAsync(WaterTest waterTest, string userId)
    {
        // Verify ownership through tank
        var existingWaterTest = await _context.WaterTests
            .Include(w => w.Tank)
            .FirstOrDefaultAsync(w => w.Id == waterTest.Id && w.Tank!.UserId == userId);

        if (existingWaterTest == null)
        {
            throw new UnauthorizedAccessException("You don't have permission to update this water test.");
        }

        // Update properties
        existingWaterTest.PH = waterTest.PH;
        existingWaterTest.Temperature = waterTest.Temperature;
        existingWaterTest.Ammonia = waterTest.Ammonia;
        existingWaterTest.Nitrite = waterTest.Nitrite;
        existingWaterTest.Nitrate = waterTest.Nitrate;
        existingWaterTest.GH = waterTest.GH;
        existingWaterTest.KH = waterTest.KH;
        existingWaterTest.TDS = waterTest.TDS;
        existingWaterTest.Salinity = waterTest.Salinity;
        existingWaterTest.Alkalinity = waterTest.Alkalinity;
        existingWaterTest.Calcium = waterTest.Calcium;
        existingWaterTest.Magnesium = waterTest.Magnesium;

        // Check for out-of-range parameters after update
        await CheckWaterParametersAsync(existingWaterTest, existingWaterTest.Tank!);

        existingWaterTest.Phosphate = waterTest.Phosphate;

        await _context.SaveChangesAsync();
        return existingWaterTest;
    }

    public async Task<bool> DeleteWaterTestAsync(int id, string userId)
    {
        var waterTest = await _context.WaterTests
            .Include(w => w.Tank)
            .FirstOrDefaultAsync(w => w.Id == id && w.Tank!.UserId == userId);

        if (waterTest == null) return false;

        _context.WaterTests.Remove(waterTest);
        await _context.SaveChangesAsync();
        return true;
    }

    private async Task CheckWaterParametersAsync(WaterTest waterTest, Tank tank)
    {
        // Use new ParameterAlertService if available
        if (_parameterAlertService != null)
        {
            try
            {
                await _parameterAlertService.CheckWaterTestAsync(waterTest.Id);
                return; // New system handled it
            }
            catch (Exception)
            {
                // Fall back to legacy system if new system fails
            }
        }

        // Legacy notification system (kept for backwards compatibility)
        if (_notificationService == null)
            return;

        var settings = await _notificationService.GetUserSettingsAsync(tank.UserId);
        if (!settings.WaterParameterAlertsEnabled)
            return;

        // Define safe ranges based on tank type
        var isSaltwater = tank.Type == AquariumType.Saltwater || tank.Type == AquariumType.Reef;

        // Check pH
        if (waterTest.PH.HasValue)
        {
            double minPH = isSaltwater ? 8.0 : 6.5;
            double maxPH = isSaltwater ? 8.4 : 8.0;

            if (waterTest.PH.Value < minPH || waterTest.PH.Value > maxPH)
            {
                await _notificationService.CreateWaterParameterAlertAsync(
                    waterTest, "pH", waterTest.PH.Value, minPH, maxPH);
            }
        }

        // Check Ammonia (should be 0)
        if (waterTest.Ammonia.HasValue && waterTest.Ammonia.Value > 0.25)
        {
            await _notificationService.CreateWaterParameterAlertAsync(
                waterTest, "Ammonia", waterTest.Ammonia.Value, null, 0.25);
        }

        // Check Nitrite (should be 0)
        if (waterTest.Nitrite.HasValue && waterTest.Nitrite.Value > 0.5)
        {
            await _notificationService.CreateWaterParameterAlertAsync(
                waterTest, "Nitrite", waterTest.Nitrite.Value, null, 0.5);
        }

        // Check Nitrate
        if (waterTest.Nitrate.HasValue)
        {
            double maxNitrate = isSaltwater ? 20 : 40;
            if (waterTest.Nitrate.Value > maxNitrate)
            {
                await _notificationService.CreateWaterParameterAlertAsync(
                    waterTest, "Nitrate", waterTest.Nitrate.Value, null, maxNitrate);
            }
        }

        // Check Temperature
        if (waterTest.Temperature.HasValue)
        {
            double minTemp = isSaltwater ? 75 : 72;
            double maxTemp = isSaltwater ? 80 : 82;

            if (waterTest.Temperature.Value < minTemp || waterTest.Temperature.Value > maxTemp)
            {
                await _notificationService.CreateWaterParameterAlertAsync(
                    waterTest, "Temperature", waterTest.Temperature.Value, minTemp, maxTemp);
            }
        }

        // Check Salinity (for saltwater tanks)
        if (isSaltwater && waterTest.Salinity.HasValue)
        {
            if (waterTest.Salinity.Value < 1.023 || waterTest.Salinity.Value > 1.026)
            {
                await _notificationService.CreateWaterParameterAlertAsync(
                    waterTest, "Salinity", waterTest.Salinity.Value, 1.023, 1.026);
            }
        }

        // Check Calcium (for reef tanks)
        if (tank.Type == AquariumType.Reef && waterTest.Calcium.HasValue)
        {
            if (waterTest.Calcium.Value < 380 || waterTest.Calcium.Value > 450)
            {
                await _notificationService.CreateWaterParameterAlertAsync(
                    waterTest, "Calcium", waterTest.Calcium.Value, 380, 450);
            }
        }

        // Check Magnesium (for reef tanks)
        if (tank.Type == AquariumType.Reef && waterTest.Magnesium.HasValue)
        {
            if (waterTest.Magnesium.Value < 1250 || waterTest.Magnesium.Value > 1350)
            {
                await _notificationService.CreateWaterParameterAlertAsync(
                    waterTest, "Magnesium", waterTest.Magnesium.Value, 1250, 1350);
            }
        }

        // Check Phosphate
        if (waterTest.Phosphate.HasValue)
        {
            double maxPhosphate = isSaltwater ? 0.03 : 1.0;
            if (waterTest.Phosphate.Value > maxPhosphate)
            {
                await _notificationService.CreateWaterParameterAlertAsync(
                    waterTest, "Phosphate", waterTest.Phosphate.Value, null, maxPhosphate);
            }
        }
    }

    public async Task<WaterParameterTrendsViewModel> GetParameterTrendsAsync(int tankId, string userId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // Verify tank ownership
        var tank = await _context.Tanks
            .FirstOrDefaultAsync(t => t.Id == tankId && t.UserId == userId);

        if (tank == null)
        {
            throw new UnauthorizedAccessException("You don't have permission to access this tank.");
        }

        // Set default date range if not provided
        var end = endDate ?? DateTime.UtcNow;
        var start = startDate ?? end.AddMonths(-3); // Default to 3 months

        // Get water tests within the date range
        var waterTests = await _context.WaterTests
            .Where(w => w.TankId == tankId && w.Timestamp >= start && w.Timestamp <= end)
            .OrderBy(w => w.Timestamp)
            .ToListAsync();

        var viewModel = new WaterParameterTrendsViewModel
        {
            Tank = tank,
            StartDate = start,
            EndDate = end,
            TotalTests = waterTests.Count,
            ParameterTrends = new List<WaterParameterTrendViewModel>()
        };

        if (waterTests.Any())
        {
            // Calculate average test frequency
            if (waterTests.Count > 1)
            {
                var totalDays = (waterTests.Last().Timestamp - waterTests.First().Timestamp).TotalDays;
                viewModel.AverageTestFrequency = totalDays / (waterTests.Count - 1);
            }

            // Get ideal ranges based on tank type

            var isSaltwater = tank.Type == AquariumType.Saltwater || tank.Type == AquariumType.Reef || tank.Type == AquariumType.NanoReef;
            var isReef = tank.Type == AquariumType.Reef;

            // pH Trend
            double phMin, phMax;
            switch (tank.Type)
            {
                case AquariumType.Saltwater:
                case AquariumType.NanoReef:
                    phMin = 8.1; phMax = 8.6; break;
                case AquariumType.Reef:
                    phMin = 8.1; phMax = 8.6; break;
                case AquariumType.Planted:
                    phMin = 6.5; phMax = 7.5; break;
                case AquariumType.Cichlid:
                    phMin = 7.8; phMax = 8.6; break;
                case AquariumType.Betta:
                    phMin = 6.5; phMax = 7.5; break;
                case AquariumType.Goldfish:
                    phMin = 7.0; phMax = 8.0; break;
                case AquariumType.Shrimp:
                    phMin = 6.2; phMax = 7.4; break;
                case AquariumType.Brackish:
                    phMin = 7.5; phMax = 8.5; break;
                default:
                    phMin = 6.5; phMax = 7.5; break;
            }
            viewModel.ParameterTrends.Add(AnalyzeParameter(
                waterTests, "pH", test => test.PH,
                phMin, phMax));

            // Temperature Trend
            viewModel.ParameterTrends.Add(AnalyzeParameter(
                waterTests, "Temperature", test => test.Temperature,
                isSaltwater ? 75 : 72,
                isSaltwater ? 80 : 82, "°F"));

            // Ammonia Trend
            viewModel.ParameterTrends.Add(AnalyzeParameter(
                waterTests, "Ammonia", test => test.Ammonia,
                null, 0.25, "ppm"));

            // Nitrite Trend
            viewModel.ParameterTrends.Add(AnalyzeParameter(
                waterTests, "Nitrite", test => test.Nitrite,
                null, 0.5, "ppm"));

            // Nitrate Trend
            viewModel.ParameterTrends.Add(AnalyzeParameter(
                waterTests, "Nitrate", test => test.Nitrate,
                null, isSaltwater ? 20 : 40, "ppm"));

            // Saltwater-specific parameters
            if (isSaltwater)
            {
                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "Salinity", test => test.Salinity,
                    1.023, 1.026, "ppt"));

                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "Alkalinity", test => test.Alkalinity,
                    8, 12, "dKH"));

                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "Phosphate", test => test.Phosphate,
                    null, 0.03, "ppm"));
            }

            // Reef-specific parameters
            if (isReef)
            {
                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "Calcium", test => test.Calcium,
                    380, 450, "ppm"));

                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "Magnesium", test => test.Magnesium,
                    1250, 1350, "ppm"));
            }

            // Freshwater-specific parameters
            if (tank.Type == AquariumType.Freshwater || tank.Type == AquariumType.Planted)
            {
                // GH and KH ranges vary by tank type
                double? ghMin = null, ghMax = null, khMin = null, khMax = null;

                switch (tank.Type)
                {
                    case AquariumType.Planted:
                        // Planted tanks prefer softer water
                        ghMin = 50; ghMax = 100;  // ~3-6 °dH
                        khMin = 40; khMax = 80;   // ~2-4 °dH
                        break;
                    case AquariumType.Freshwater:
                    default:
                        // General freshwater community
                        ghMin = 70; ghMax = 215;  // ~4-12 °dH
                        khMin = 54; khMax = 143;  // ~3-8 °dH
                        break;
                }

                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "GH", test => test.GH,
                    ghMin, ghMax, "ppm"));

                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "KH", test => test.KH,
                    khMin, khMax, "ppm"));

                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "TDS", test => test.TDS,
                    150, 250, "ppm"));

                if (tank.Type != AquariumType.Planted)
                {
                    viewModel.ParameterTrends.Add(AnalyzeParameter(
                        waterTests, "Phosphate", test => test.Phosphate,
                        null, 1.0, "ppm"));
                }
            }

            // Planted tank specific parameters
            if (tank.Type == AquariumType.Planted)
            {
                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "Phosphate", test => test.Phosphate,
                    0.5, 2.0, "ppm"));

                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "Iron", test => test.Iron,
                    0.1, 0.5, "ppm"));

                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "CO2", test => test.CO2,
                    20, 30, "ppm"));
            }

            // Cichlid-specific parameters (African cichlids prefer harder water)
            if (tank.Type == AquariumType.Cichlid)
            {
                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "GH", test => test.GH,
                    180, 360, "ppm"));  // ~10-20 °dH

                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "KH", test => test.KH,
                    125, 215, "ppm"));  // ~7-12 °dH

                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "TDS", test => test.TDS,
                    200, 400, "ppm"));

                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "Phosphate", test => test.Phosphate,
                    null, 1.0, "ppm"));
            }

            // Shrimp-specific parameters (prefer very soft water)
            if (tank.Type == AquariumType.Shrimp)
            {
                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "GH", test => test.GH,
                    36, 90, "ppm"));  // ~2-5 °dH

                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "KH", test => test.KH,
                    18, 54, "ppm"));  // ~1-3 °dH

                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "TDS", test => test.TDS,
                    100, 200, "ppm"));

                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "Phosphate", test => test.Phosphate,
                    null, 0.5, "ppm"));
            }

            // Betta/Goldfish parameters (moderate hardness)
            if (tank.Type == AquariumType.Betta || tank.Type == AquariumType.Goldfish)
            {
                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "GH", test => test.GH,
                    90, 180, "ppm"));  // ~5-10 °dH

                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "KH", test => test.KH,
                    54, 125, "ppm"));  // ~3-7 °dH

                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "TDS", test => test.TDS,
                    150, 300, "ppm"));

                viewModel.ParameterTrends.Add(AnalyzeParameter(
                    waterTests, "Phosphate", test => test.Phosphate,
                    null, 1.0, "ppm"));
            }

            // Generate alerts
            viewModel.Alerts = GenerateAlerts(viewModel.ParameterTrends);
        }

        return viewModel;
    }

    private WaterParameterTrendViewModel AnalyzeParameter(
        List<WaterTest> waterTests,
        string parameterName,
        Func<WaterTest, double?> valueSelector,
        double? idealMin,
        double? idealMax,
        string unit = "")
    {
        var dataPoints = waterTests
            .Select(test => new DataPoint
            {
                Date = test.Timestamp,
                Value = valueSelector(test)
            })
            .Where(dp => dp.Value.HasValue)
            .ToList();

        var values = dataPoints.Select(dp => dp.Value!.Value).ToList();

        var trend = new WaterParameterTrendViewModel
        {
            ParameterName = parameterName,
            DataPoints = dataPoints,
            Unit = unit,
            IdealMin = idealMin,
            IdealMax = idealMax,
            Statistics = new ParameterStatistics()
        };

        if (values.Any())
        {
            trend.Statistics.TotalReadings = values.Count;
            trend.Statistics.CurrentValue = values.Last();
            trend.Statistics.Average = values.Average();
            trend.Statistics.Min = values.Min();
            trend.Statistics.Max = values.Max();

            // Calculate median
            var sortedValues = values.OrderBy(v => v).ToList();
            var mid = sortedValues.Count / 2;
            trend.Statistics.Median = sortedValues.Count % 2 == 0
                ? (sortedValues[mid - 1] + sortedValues[mid]) / 2
                : sortedValues[mid];

            // Calculate standard deviation
            var mean = values.Average();
            var sumOfSquares = values.Select(v => Math.Pow(v - mean, 2)).Sum();
            trend.Statistics.StandardDeviation = Math.Sqrt(sumOfSquares / values.Count);

            // Determine trend direction
            if (values.Count >= 3)
            {
                var recentValues = values.TakeLast(Math.Min(10, values.Count)).ToList();
                var firstHalf = recentValues.Take(recentValues.Count / 2).Average();
                var secondHalf = recentValues.Skip(recentValues.Count / 2).Average();
                var change = ((secondHalf - firstHalf) / firstHalf) * 100;

                trend.Statistics.TrendPercentage = change;

                if (Math.Abs(change) < 2)
                    trend.Statistics.Trend = TrendDirection.Stable;
                else if (change > 0)
                    trend.Statistics.Trend = TrendDirection.Rising;
                else
                    trend.Statistics.Trend = TrendDirection.Falling;

                // Check for high fluctuation
                if (trend.Statistics.StandardDeviation > mean * 0.15)
                    trend.Statistics.Trend = TrendDirection.Fluctuating;
            }

            // Check if current value is in ideal range
            if (idealMin.HasValue && idealMax.HasValue)
            {
                trend.Statistics.IsInIdealRange =
                    trend.Statistics.CurrentValue >= idealMin.Value &&
                    trend.Statistics.CurrentValue <= idealMax.Value;

                // Count days outside ideal range
                trend.Statistics.DaysAboveIdeal = dataPoints.Count(dp => dp.Value > idealMax.Value);
                trend.Statistics.DaysBelowIdeal = dataPoints.Count(dp => dp.Value < idealMin.Value);
            }
            else if (idealMax.HasValue)
            {
                trend.Statistics.IsInIdealRange = trend.Statistics.CurrentValue <= idealMax.Value;
                trend.Statistics.DaysAboveIdeal = dataPoints.Count(dp => dp.Value > idealMax.Value);
            }
        }

        return trend;
    }

    private Dictionary<string, List<string>> GenerateAlerts(List<WaterParameterTrendViewModel> trends)
    {
        var alerts = new Dictionary<string, List<string>>
        {
            { "Critical", new List<string>() },
            { "Warning", new List<string>() },
            { "Info", new List<string>() }
        };

        foreach (var trend in trends)
        {
            if (trend.Statistics.TotalReadings == 0) continue;

            // Critical: Current value outside ideal range
            if (!trend.Statistics.IsInIdealRange && trend.Statistics.CurrentValue.HasValue)
            {
                if (trend.IdealMin.HasValue && trend.Statistics.CurrentValue < trend.IdealMin.Value)
                {
                    alerts["Critical"].Add($"{trend.ParameterName} is below ideal range: {trend.Statistics.CurrentValue:F2}{trend.Unit} (ideal: {trend.IdealMin:F2}-{trend.IdealMax:F2}{trend.Unit})");
                }
                else if (trend.IdealMax.HasValue && trend.Statistics.CurrentValue > trend.IdealMax.Value)
                {
                    alerts["Critical"].Add($"{trend.ParameterName} is above ideal range: {trend.Statistics.CurrentValue:F2}{trend.Unit} (ideal max: {trend.IdealMax:F2}{trend.Unit})");
                }
            }

            // Warning: Trend is moving towards dangerous levels
            if (trend.Statistics.Trend == TrendDirection.Rising && trend.IdealMax.HasValue)
            {
                var percentToMax = ((trend.IdealMax.Value - trend.Statistics.CurrentValue!.Value) / trend.IdealMax.Value) * 100;
                if (percentToMax < 20 && percentToMax > 0)
                {
                    alerts["Warning"].Add($"{trend.ParameterName} is rising and approaching maximum safe level");
                }
            }

            if (trend.Statistics.Trend == TrendDirection.Falling && trend.IdealMin.HasValue)
            {
                var percentToMin = ((trend.Statistics.CurrentValue!.Value - trend.IdealMin.Value) / trend.IdealMin.Value) * 100;
                if (percentToMin < 20 && percentToMin > 0)
                {
                    alerts["Warning"].Add($"{trend.ParameterName} is falling and approaching minimum safe level");
                }
            }

            // Info: High fluctuation
            if (trend.Statistics.Trend == TrendDirection.Fluctuating)
            {
                alerts["Info"].Add($"{trend.ParameterName} shows high fluctuation - consider more frequent testing");
            }
        }

        return alerts;
    }
}
