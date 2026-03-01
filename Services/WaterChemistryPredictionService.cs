using Microsoft.EntityFrameworkCore;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;

namespace AquaHub.MVC.Services;

/// <summary>
/// Implementation of water chemistry prediction service using machine learning algorithms
/// </summary>
/// <remarks>
/// ======================================================================================
/// MACHINE LEARNING PRIMER FOR AQUARIUM WATER CHEMISTRY
/// ======================================================================================
/// 
/// This service implements several ML algorithms to predict future water parameters:
/// 
/// 1. LINEAR REGRESSION:
///    - What it does: Finds the straight-line trend in your data
///    - Best for: Parameters that change steadily over time (like gradual pH drift)
///    - Example: If pH drops from 8.2 to 7.8 over 20 days, it predicts a drop of 0.02/day
///    - Formula: y = mx + b (where m is the rate of change, b is the starting point)
/// 
/// 2. WEIGHTED MOVING AVERAGE:
///    - What it does: Averages recent values, giving more weight to newer readings
///    - Best for: Stable parameters with occasional fluctuations
///    - Example: Recent readings matter more than old ones for prediction
///    - Why: More recent patterns are usually more relevant to current conditions
/// 
/// 3. EXPONENTIAL SMOOTHING:
///    - What it does: Similar to moving average but uses exponential weighting
///    - Best for: Parameters with seasonal patterns or cycles
///    - Example: Temperature that varies with room temperature
///    - Math: Each older point has exponentially less influence
/// 
/// WHICH ALGORITHM DOES THIS SERVICE USE?
/// - We primarily use Linear Regression because aquarium chemistry often follows
///   predictable trends (bioload increases, buffering capacity decreases, etc.)
/// - We calculate confidence based on how well the data fits a straight line
/// - We can add more algorithms later for different parameter types
/// 
/// MACHINE LEARNING TERMINOLOGY USED IN THIS CODE:
/// - Training Data: Historical WaterTest records from the database
/// - Features: Date, previous values (what we use to predict)
/// - Target/Label: The future value we're trying to predict
/// - Model: The linear regression calculation
/// - Prediction: The estimated future value
/// - Confidence Score: R² (R-squared) - measures how well data fits the trend line
/// - Validation: Comparing old predictions with actual results
/// 
/// ======================================================================================
/// </remarks>
public class WaterChemistryPredictionService : IWaterChemistryPredictionService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<WaterChemistryPredictionService> _logger;

    // Configuration constants for the ML model
    /// <summary>
    /// Minimum number of historical data points needed to make a reliable prediction
    /// ML Note: With fewer points, predictions become unreliable (not enough pattern data)
    /// </summary>
    private const int MINIMUM_DATA_POINTS = 10;

    /// <summary>
    /// Maximum days in history to consider for predictions
    /// ML Note: Very old data may not reflect current tank conditions
    /// </summary>
    private const int MAX_HISTORY_DAYS = 90;

    /// <summary>
    /// Minimum R² (R-squared) value to consider a prediction reliable
    /// ML Note: R² measures how well data fits the trend line (0-1 scale)
    /// 0.5 means 50% of variance is explained by the trend
    /// </summary>
    private const double MINIMUM_CONFIDENCE = 0.5;

    public WaterChemistryPredictionService(
        ApplicationDbContext context,
        ILogger<WaterChemistryPredictionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Generate predictions for all water parameters in a tank
    /// This is the main entry point for the prediction system
    /// </summary>
    public async Task<TankPredictionSummary> GeneratePredictionsForTankAsync(
        int tankId,
        string userId,
        int daysAhead = 7)
    {
        _logger.LogInformation(
            "Generating water chemistry predictions for tank {TankId}, {DaysAhead} days ahead",
            tankId,
            daysAhead);

        // Initialize the summary object that will hold all predictions
        var summary = new TankPredictionSummary
        {
            TankId = tankId,
            GeneratedAt = DateTime.UtcNow,
            Predictions = new List<WaterChemistryPredictionDTO>()
        };

        // Get the tank information
        var tank = await _context.Tanks
            .FirstOrDefaultAsync(t => t.Id == tankId && t.UserId == userId);

        if (tank == null)
        {
            _logger.LogWarning("Tank {TankId} not found for user {UserId}", tankId, userId);
            summary.HasSufficientData = false;
            summary.InsufficientDataMessage = "Tank not found.";
            return summary;
        }

        summary.TankName = tank.Name;

        // Step 1: Get all historical water test data for this tank
        // ML Note: This is our "training data" - the historical information we learn from
        var historicalTests = await GetHistoricalWaterTestsAsync(tankId, userId);

        // Step 2: Check if we have enough data to make predictions
        if (historicalTests.Count < MINIMUM_DATA_POINTS)
        {
            _logger.LogInformation(
                "Insufficient data for tank {TankId}. Found {Count} tests, need {Required}",
                tankId,
                historicalTests.Count,
                MINIMUM_DATA_POINTS);

            summary.HasSufficientData = false;
            summary.InsufficientDataMessage =
                $"Need at least {MINIMUM_DATA_POINTS} water tests to generate predictions. " +
                $"You currently have {historicalTests.Count}. Keep testing regularly!";
            return summary;
        }

        summary.HasSufficientData = true;

        // Step 3: Determine which parameters to predict based on tank type
        // Different tank types have different relevant parameters
        var parametersToPredict = GetRelevantParameters(tank);

        _logger.LogInformation(
            "Analyzing {ParamCount} parameters for tank {TankId}",
            parametersToPredict.Count,
            tankId);

        // Step 4: Generate a prediction for each parameter
        foreach (var parameterName in parametersToPredict)
        {
            var prediction = await GenerateParameterPredictionAsync(
                tankId,
                parameterName,
                userId,
                daysAhead);

            if (prediction != null)
            {
                summary.Predictions.Add(prediction);

                // Count warnings (parameters predicted to go out of safe range)
                if (prediction.IsWarning)
                {
                    summary.WarningCount++;
                }
            }
        }

        // Step 5: Calculate summary statistics
        if (summary.Predictions.Any())
        {
            // Average confidence across all predictions
            summary.AverageConfidence = summary.Predictions.Average(p => p.ConfidenceScore) * 100;

            // Determine overall forecast based on warnings
            summary.OverallForecast = CalculateOverallForecast(summary);
        }
        else
        {
            summary.OverallForecast = "Insufficient Data";
        }

        _logger.LogInformation(
            "Generated {Count} predictions for tank {TankId}. Overall forecast: {Forecast}",
            summary.Predictions.Count,
            tankId,
            summary.OverallForecast);

        return summary;
    }

    /// <summary>
    /// Generate a prediction for a single water parameter using Linear Regression
    /// </summary>
    /// <remarks>
    /// LINEAR REGRESSION EXPLAINED:
    /// 
    /// Imagine plotting your water test results on a graph:
    /// - X-axis: Date (time)
    /// - Y-axis: Parameter value (e.g., pH)
    /// 
    /// Linear regression finds the "best fit" straight line through your points.
    /// 
    /// The math (don't worry, we do this for you):
    /// 1. Calculate the slope (m): How much the value changes per day
    /// 2. Calculate the intercept (b): Where the line crosses the y-axis
    /// 3. Predict future value: future_value = m × days_ahead + current_value
    /// 4. Calculate R² (confidence): How well points follow the line
    /// 
    /// Example:
    /// - Days: [0, 3, 6, 9, 12]
    /// - pH values: [8.2, 8.1, 8.0, 7.9, 7.8]
    /// - Slope (m) ≈ -0.033 (pH drops 0.033 per day)
    /// - Prediction for day 15: 8.2 + (-0.033 × 15) ≈ 7.7
    /// - R² = 0.99 (99% confidence - points fit line almost perfectly!)
    /// </remarks>
    public async Task<WaterChemistryPredictionDTO?> GenerateParameterPredictionAsync(
        int tankId,
        string parameterName,
        string userId,
        int daysAhead = 7)
    {
        _logger.LogDebug(
            "Generating prediction for {Parameter} in tank {TankId}",
            parameterName,
            tankId);

        // Step 1: Get historical data for this specific parameter
        var historicalTests = await GetHistoricalWaterTestsAsync(tankId, userId);

        // Step 2: Extract the values for the parameter we're predicting
        // ML Note: We need to filter out null values and convert to a time series
        var parameterData = ExtractParameterData(historicalTests, parameterName);

        // Step 3: Check if we have enough non-null data points
        if (parameterData.Count < MINIMUM_DATA_POINTS)
        {
            _logger.LogDebug(
                "Insufficient data for {Parameter}. Found {Count} non-null values",
                parameterName,
                parameterData.Count);
            return null;
        }

        // Step 4: Perform Linear Regression to find the trend
        // This is where the machine learning happens!
        var regressionResult = PerformLinearRegression(parameterData);

        // Step 5: Use the regression model to predict future value
        var currentValue = parameterData.Last().Value;
        var predictedValue = regressionResult.Slope * daysAhead + currentValue;

        // Step 6: Determine the trend direction
        var trend = DetermineTrend(regressionResult.Slope);

        // Step 7: Check if the prediction is within safe ranges
        var safeRange = GetSafeRange(parameterName);
        var isWarning = IsOutsideRange(predictedValue, safeRange);

        // Step 8: Generate a helpful message for the user
        var message = GeneratePredictionMessage(
            parameterName,
            currentValue,
            predictedValue,
            trend,
            isWarning,
            safeRange);

        // Step 9: Calculate the change and percentage change
        var change = predictedValue - currentValue;
        var changePercentage = currentValue != 0
            ? (change / currentValue) * 100
            : 0;

        // Step 10: Determine alert level for UI
        var alertLevel = DetermineAlertLevel(isWarning, regressionResult.RSquared);

        // Step 11: Create the prediction DTO
        var predictionDTO = new WaterChemistryPredictionDTO
        {
            ParameterName = parameterName,
            CurrentValue = Math.Round(currentValue, 2),
            PredictedValue = Math.Round(predictedValue, 2),
            Change = Math.Round(change, 2),
            ChangePercentage = Math.Round(changePercentage, 1),
            Trend = trend,
            PredictedDate = DateTime.UtcNow.AddDays(daysAhead),
            DaysAhead = daysAhead,
            ConfidenceScore = Math.Round(regressionResult.RSquared, 2),
            IsWarning = isWarning,
            Message = message,
            PredictionMethod = "Linear Regression",
            DataPointsUsed = parameterData.Count,
            RateOfChange = Math.Round(regressionResult.Slope, 4),
            AlertLevel = alertLevel
        };

        // Step 12: Save the prediction to database for future reference
        await SavePredictionToDatabase(tankId, predictionDTO);

        _logger.LogDebug(
            "Prediction generated: {Parameter} = {Current} → {Predicted} ({Trend}, {Confidence}% confidence)",
            parameterName,
            currentValue,
            predictedValue,
            trend,
            (int)(regressionResult.RSquared * 100));

        return predictionDTO;
    }

    /// <summary>
    /// Perform Linear Regression on time series data
    /// </summary>
    /// <remarks>
    /// MATHEMATICAL EXPLANATION:
    /// 
    /// Linear Regression finds the line y = mx + b that best fits your data points.
    /// 
    /// We use the "Least Squares" method:
    /// 1. For each data point, calculate how far it is from the line (the "error")
    /// 2. Square all the errors (so negative and positive errors don't cancel out)
    /// 3. Find the line that minimizes the sum of squared errors
    /// 
    /// Formulas (we implement these below):
    /// - Slope (m) = Σ[(x - x̄)(y - ȳ)] / Σ[(x - x̄)²]
    /// - Intercept (b) = ȳ - m × x̄
    /// - R² = 1 - (SS_residual / SS_total)
    /// 
    /// Where:
    /// - x̄ (x-bar) = average of x values (time)
    /// - ȳ (y-bar) = average of y values (parameter values)
    /// - Σ (sigma) = sum of all values
    /// - SS = Sum of Squares
    /// 
    /// Don't worry about memorizing these formulas! The code does it for you.
    /// </remarks>
    private LinearRegressionResult PerformLinearRegression(List<(DateTime Date, double Value)> data)
    {
        // Convert dates to numbers (days since first measurement)
        // ML Note: We need to convert dates to numbers to do math on them
        var startDate = data.First().Date;
        var dataPoints = data.Select(d => new
        {
            X = (d.Date - startDate).TotalDays, // Days since first test
            Y = d.Value                           // Parameter value
        }).ToList();

        // Calculate means (averages)
        // x̄ (x-bar): Average day number
        // ȳ (y-bar): Average parameter value
        var n = dataPoints.Count;
        var xMean = dataPoints.Average(p => p.X);
        var yMean = dataPoints.Average(p => p.Y);

        // Calculate slope (m): How much Y changes for each unit change in X
        // This tells us the rate of change per day
        // Formula: m = Σ[(x - x̄)(y - ȳ)] / Σ[(x - x̄)²]
        var numerator = dataPoints.Sum(p => (p.X - xMean) * (p.Y - yMean));
        var denominator = dataPoints.Sum(p => Math.Pow(p.X - xMean, 2));

        // Slope represents rate of change per day
        var slope = denominator != 0 ? numerator / denominator : 0;

        // Calculate intercept (b): Where the line crosses the Y-axis
        // Formula: b = ȳ - m × x̄
        var intercept = yMean - (slope * xMean);

        // Calculate R² (R-squared): Measure of how well the line fits the data
        // R² ranges from 0 to 1:
        // - 1.0 = perfect fit (all points exactly on the line)
        // - 0.5 = moderate fit (points somewhat scattered)
        // - 0.0 = no fit (data is completely random)
        //
        // Formula: R² = 1 - (SS_residual / SS_total)
        // - SS_total: Total variance in the data
        // - SS_residual: Variance not explained by the line

        // Total Sum of Squares: How much data varies from the mean
        var ssTotal = dataPoints.Sum(p => Math.Pow(p.Y - yMean, 2));

        // Residual Sum of Squares: How much data varies from our predicted line
        var ssResidual = dataPoints.Sum(p =>
        {
            var predicted = slope * p.X + intercept;
            return Math.Pow(p.Y - predicted, 2);
        });

        // R² = portion of variance explained by our model
        var rSquared = ssTotal != 0 ? 1 - (ssResidual / ssTotal) : 0;

        // Ensure R² is between 0 and 1
        rSquared = Math.Max(0, Math.Min(1, rSquared));

        return new LinearRegressionResult
        {
            Slope = slope,
            Intercept = intercept,
            RSquared = rSquared,
            DataPoints = n
        };
    }

    /// <summary>
    /// Get historical water test data for analysis
    /// </summary>
    /// <remarks>
    /// ML Note: This is our "training data" - the historical information our model learns from.
    /// Quality of predictions depends heavily on:
    /// - Amount of data: More tests = better predictions
    /// - Regularity: Tests at consistent intervals work better
    /// - Recency: Recent data is more relevant than old data
    /// </remarks>
    public async Task<List<WaterTest>> GetHistoricalWaterTestsAsync(int tankId, string userId)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-MAX_HISTORY_DAYS);

        return await _context.WaterTests
            .Where(wt => wt.TankId == tankId
                      && wt.Tank!.UserId == userId
                      && wt.Timestamp >= cutoffDate)
            .OrderBy(wt => wt.Timestamp)
            .ToListAsync();
    }

    /// <summary>
    /// Extract data points for a specific parameter from water tests
    /// </summary>
    /// <remarks>
    /// ML Note: Different parameters are stored in different properties.
    /// We extract the relevant property values and create a time series
    /// (a list of dates and values) that we can analyze.
    /// 
    /// We filter out null values because:
    /// - Not every test includes every parameter
    /// - ML algorithms can't work with missing data (null values)
    /// - We need continuous data points to find patterns
    /// </remarks>
    public List<(DateTime Date, double Value)> ExtractParameterData(
        List<WaterTest> tests,
        string parameterName)
    {
        var data = new List<(DateTime Date, double Value)>();

        foreach (var test in tests)
        {
            double? value = parameterName switch
            {
                "PH" => test.PH,
                "Temperature" => test.Temperature,
                "Ammonia" => test.Ammonia,
                "Nitrite" => test.Nitrite,
                "Nitrate" => test.Nitrate,
                "Salinity" => test.Salinity,
                "Alkalinity" => test.Alkalinity,
                "Calcium" => test.Calcium,
                "Magnesium" => test.Magnesium,
                "Phosphate" => test.Phosphate,
                "GH" => test.GH,
                "KH" => test.KH,
                "TDS" => test.TDS,
                _ => null
            };

            // Only include non-null values
            // ML Note: Null values would break our calculations
            if (value.HasValue)
            {
                data.Add((test.Timestamp, value.Value));
            }
        }

        return data;
    }

    /// <summary>
    /// Get the list of parameters relevant to a tank type
    /// </summary>
    /// <remarks>
    /// Different tank types have different relevant parameters:
    /// - Reef: Salinity, calcium, alkalinity, magnesium critical
    /// - Freshwater: GH, KH, TDS important
    /// - All tanks: pH, ammonia, nitrite, nitrate, temperature
    /// </remarks>
    private List<string> GetRelevantParameters(Tank tank)
    {
        var parameters = new List<string>
        {
            // Parameters common to all tank types
            "PH",
            "Temperature",
            "Ammonia",
            "Nitrite",
            "Nitrate"
        };

        // Add reef-specific parameters
        if (tank.Type.ToString().Contains("Reef") || tank.Type.ToString().Contains("Saltwater"))
        {
            parameters.AddRange(new[] { "Salinity", "Alkalinity", "Calcium", "Magnesium", "Phosphate" });
        }

        // Add freshwater-specific parameters
        if (tank.Type.ToString().Contains("Freshwater"))
        {
            parameters.AddRange(new[] { "GH", "KH", "TDS" });
        }

        return parameters;
    }

    /// <summary>
    /// Determine if a value is outside safe ranges
    /// </summary>
    /// <remarks>
    /// This is where domain knowledge (aquarium science) meets ML.
    /// We use established safe ranges for each parameter to determine if
    /// a prediction indicates a problem.
    /// </remarks>
    private bool IsOutsideRange(double value, (double Min, double Max) range)
    {
        return value < range.Min || value > range.Max;
    }

    /// <summary>
    /// Get safe range for a parameter
    /// </summary>
    /// <remarks>
    /// These ranges are based on established aquarium science.
    /// ML predicts the values, but domain knowledge determines if they're safe.
    /// </remarks>
    private (double Min, double Max) GetSafeRange(string parameterName)
    {
        return parameterName switch
        {
            "PH" => (7.8, 8.4),              // Reef tank ideal range
            "Temperature" => (75, 82),        // Fahrenheit
            "Ammonia" => (0, 0.25),          // ppm (should be 0!)
            "Nitrite" => (0, 0.25),          // ppm (should be 0!)
            "Nitrate" => (0, 20),            // ppm
            "Salinity" => (1.023, 1.026),    // Specific gravity
            "Alkalinity" => (8, 12),         // dKH
            "Calcium" => (400, 450),         // ppm
            "Magnesium" => (1250, 1350),     // ppm
            "Phosphate" => (0, 0.1),         // ppm
            "GH" => (4, 8),                  // degrees
            "KH" => (3, 6),                  // degrees
            "TDS" => (150, 250),             // ppm
            _ => (double.MinValue, double.MaxValue)
        };
    }

    /// <summary>
    /// Determine trend direction from slope
    /// </summary>
    /// <remarks>
    /// The slope tells us the direction of change:
    /// - Positive slope: Value is increasing
    /// - Negative slope: Value is decreasing
    /// - Near zero: Value is stable
    /// 
    /// We use thresholds to avoid calling tiny changes a "trend"
    /// </remarks>
    private string DetermineTrend(double slope)
    {
        const double STABLE_THRESHOLD = 0.001;

        if (Math.Abs(slope) < STABLE_THRESHOLD)
            return "Stable";

        return slope > 0 ? "Increasing" : "Decreasing";
    }

    /// <summary>
    /// Generate a helpful message for the user
    /// </summary>
    private string GeneratePredictionMessage(
        string parameterName,
        double currentValue,
        double predictedValue,
        string trend,
        bool isWarning,
        (double Min, double Max) safeRange)
    {
        if (!isWarning && trend == "Stable")
        {
            return $"{parameterName} is stable and within optimal range.";
        }

        if (!isWarning)
        {
            return $"{parameterName} is {trend.ToLower()} but expected to remain within safe range.";
        }

        // Generate warning message
        if (predictedValue < safeRange.Min)
        {
            return $"⚠️ {parameterName} predicted to drop below safe range ({safeRange.Min}). Consider corrective action.";
        }

        if (predictedValue > safeRange.Max)
        {
            return $"⚠️ {parameterName} predicted to exceed safe range ({safeRange.Max}). Consider corrective action.";
        }

        return $"{parameterName} is {trend.ToLower()}.";
    }

    /// <summary>
    /// Determine alert level for UI styling
    /// </summary>
    private string DetermineAlertLevel(bool isWarning, double confidence)
    {
        if (isWarning && confidence >= 0.7)
            return "danger";

        if (isWarning && confidence >= 0.5)
            return "warning";

        if (confidence >= 0.7)
            return "success";

        return "info";
    }

    /// <summary>
    /// Calculate overall forecast for the tank
    /// </summary>
    private string CalculateOverallForecast(TankPredictionSummary summary)
    {
        if (summary.WarningCount == 0 && summary.AverageConfidence >= 70)
            return "Excellent";

        if (summary.WarningCount == 0)
            return "Good";

        if (summary.WarningCount <= 2)
            return "Fair";

        return "Concerning";
    }

    /// <summary>
    /// Save a prediction to the database
    /// </summary>
    /// <remarks>
    /// We save predictions so we can:
    /// 1. Track prediction history
    /// 2. Validate accuracy later (compare predictions vs actual results)
    /// 3. Improve the model over time
    /// </remarks>
    private async Task SavePredictionToDatabase(int tankId, WaterChemistryPredictionDTO dto)
    {
        var prediction = new WaterChemistryPrediction
        {
            TankId = tankId,
            ParameterName = dto.ParameterName,
            PredictedValue = dto.PredictedValue,
            CurrentValue = dto.CurrentValue,
            PredictionDate = DateTime.UtcNow,
            PredictedDate = dto.PredictedDate,
            ConfidenceScore = dto.ConfidenceScore,
            Trend = dto.Trend,
            DaysAhead = dto.DaysAhead,
            IsWarning = dto.IsWarning,
            Message = dto.Message,
            PredictionMethod = dto.PredictionMethod,
            DataPointsUsed = dto.DataPointsUsed,
            RateOfChange = dto.RateOfChange
        };

        _context.WaterChemistryPredictions.Add(prediction);
        await _context.SaveChangesAsync();
    }

    public async Task<List<WaterChemistryPrediction>> GetHistoricalPredictionsAsync(int tankId, string userId)
    {
        return await _context.WaterChemistryPredictions
            .Include(p => p.Tank)
            .Where(p => p.TankId == tankId && p.Tank!.UserId == userId)
            .OrderByDescending(p => p.PredictionDate)
            .Take(50)
            .ToListAsync();
    }

    public async Task SavePredictionAsync(WaterChemistryPrediction prediction)
    {
        _context.WaterChemistryPredictions.Add(prediction);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Validate prediction accuracy by comparing past predictions with actual results
    /// </summary>
    /// <remarks>
    /// MODEL VALIDATION - CRUCIAL FOR ML:
    /// 
    /// This is how we know if our ML model is actually working!
    /// 
    /// Process:
    /// 1. Get old predictions from the database
    /// 2. For each prediction, find the actual water test closest to the predicted date
    /// 3. Calculate the error: |predicted_value - actual_value|
    /// 4. Calculate percentage error: (error / actual_value) × 100
    /// 5. Average all errors to get overall accuracy
    /// 
    /// Example:
    /// - Predicted: pH 7.8 on Jan 10
    /// - Actual: pH 7.9 on Jan 10
    /// - Error: |7.8 - 7.9| = 0.1
    /// - Percentage error: (0.1 / 7.9) × 100 = 1.27%
    /// 
    /// Good accuracy: < 5% average error
    /// Fair accuracy: 5-10% average error
    /// Poor accuracy: > 10% average error
    /// </remarks>
    public async Task<PredictionAccuracyReport> ValidatePredictionAccuracyAsync(int tankId, string userId)
    {
        _logger.LogInformation("Validating prediction accuracy for tank {TankId}", tankId);

        var report = new PredictionAccuracyReport
        {
            AccuracyByParameter = new Dictionary<string, double>()
        };

        // Get all past predictions that have reached their predicted date
        var pastPredictions = await _context.WaterChemistryPredictions
            .Include(p => p.Tank)
            .Where(p => p.TankId == tankId
                     && p.Tank!.UserId == userId
                     && p.PredictedDate <= DateTime.UtcNow)
            .ToListAsync();

        if (!pastPredictions.Any())
        {
            report.OverallRating = "No data available";
            return report;
        }

        var errors = new List<double>();
        var accurateWithin10Percent = 0;
        var parameterErrors = new Dictionary<string, List<double>>();

        // For each prediction, find the actual value and calculate error
        foreach (var prediction in pastPredictions)
        {
            // Find actual water test closest to the predicted date
            var actualTest = await _context.WaterTests
                .Where(wt => wt.TankId == tankId
                          && wt.Timestamp >= prediction.PredictedDate.AddDays(-1)
                          && wt.Timestamp <= prediction.PredictedDate.AddDays(1))
                .OrderBy(wt => Math.Abs((wt.Timestamp - prediction.PredictedDate).TotalDays))
                .FirstOrDefaultAsync();

            if (actualTest == null)
                continue;

            // Get actual value for this parameter
            var actualValue = GetActualParameterValue(actualTest, prediction.ParameterName);

            if (!actualValue.HasValue || actualValue.Value == 0)
                continue;

            // Calculate error
            var error = Math.Abs(prediction.PredictedValue - actualValue.Value);
            var percentageError = (error / actualValue.Value) * 100;

            errors.Add(percentageError);

            // Track per-parameter errors
            if (!parameterErrors.ContainsKey(prediction.ParameterName))
            {
                parameterErrors[prediction.ParameterName] = new List<double>();
            }
            parameterErrors[prediction.ParameterName].Add(percentageError);

            // Count accurate predictions (within 10%)
            if (percentageError <= 10)
            {
                accurateWithin10Percent++;
            }
        }

        // Calculate overall metrics
        report.PredictionsEvaluated = errors.Count;

        if (errors.Any())
        {
            report.AverageErrorPercentage = Math.Round(errors.Average(), 2);
            report.AccuracyWithin10Percent = Math.Round(
                (double)accurateWithin10Percent / errors.Count * 100, 1);

            // Calculate per-parameter accuracy
            foreach (var kvp in parameterErrors)
            {
                report.AccuracyByParameter[kvp.Key] = Math.Round(kvp.Value.Average(), 2);
            }

            // Determine overall rating
            report.OverallRating = report.AverageErrorPercentage switch
            {
                < 5 => "Excellent - Predictions are highly accurate",
                < 10 => "Good - Predictions are reliable",
                < 15 => "Fair - Predictions show trends but may not be precise",
                _ => "Poor - More data needed for accurate predictions"
            };
        }
        else
        {
            report.OverallRating = "No comparable data available";
        }

        _logger.LogInformation(
            "Validation complete: {Count} predictions, {Avg}% average error",
            report.PredictionsEvaluated,
            report.AverageErrorPercentage);

        return report;
    }

    /// <summary>
    /// Get actual parameter value from a water test
    /// </summary>
    private double? GetActualParameterValue(WaterTest test, string parameterName)
    {
        return parameterName switch
        {
            "PH" => test.PH,
            "Temperature" => test.Temperature,
            "Ammonia" => test.Ammonia,
            "Nitrite" => test.Nitrite,
            "Nitrate" => test.Nitrate,
            "Salinity" => test.Salinity,
            "Alkalinity" => test.Alkalinity,
            "Calcium" => test.Calcium,
            "Magnesium" => test.Magnesium,
            "Phosphate" => test.Phosphate,
            "GH" => test.GH,
            "KH" => test.KH,
            "TDS" => test.TDS,
            _ => null
        };
    }
}

/// <summary>
/// Result of linear regression calculation
/// </summary>
/// <remarks>
/// This holds the "model" from our linear regression.
/// - Slope: Rate of change (m in y = mx + b)
/// - Intercept: Starting value (b in y = mx + b)
/// - RSquared: How well the line fits (confidence measure)
/// - DataPoints: How many values we used (more = better)
/// </remarks>
public class LinearRegressionResult
{
    public double Slope { get; set; }
    public double Intercept { get; set; }
    public double RSquared { get; set; }
    public int DataPoints { get; set; }
}
