using AquaHub.MVC.Models;

namespace AquaHub.MVC.Services.Interfaces;

/// <summary>
/// Service interface for water chemistry predictions using machine learning algorithms
/// </summary>
/// <remarks>
/// MACHINE LEARNING BASICS:
/// 
/// Machine Learning is about finding patterns in historical data to predict future outcomes.
/// Think of it like this:
/// 
/// 1. You record your tank's pH every 3 days: 8.2, 8.1, 8.0, 7.9, 7.8
/// 2. A pattern emerges: pH is dropping by ~0.1 every 3 days
/// 3. ML algorithm: "If this continues, pH will be 7.7 in 3 days"
/// 4. You can now take action BEFORE pH gets too low
/// 
/// KEY CONCEPTS:
/// - Training Data: Historical water test results (your input)
/// - Features: The information we use to make predictions (dates, previous values, trends)
/// - Model: The algorithm that learns from the training data
/// - Prediction: The estimated future value
/// - Confidence: How sure we are about the prediction (more data = higher confidence)
/// </remarks>
public interface IWaterChemistryPredictionService
{
    /// <summary>
    /// Generate predictions for all relevant water parameters for a specific tank
    /// </summary>
    /// <param name="tankId">The tank to analyze</param>
    /// <param name="userId">The user who owns the tank (for security)</param>
    /// <param name="daysAhead">How many days into the future to predict (default: 7)</param>
    /// <returns>A summary of all predictions for this tank</returns>
    Task<TankPredictionSummary> GeneratePredictionsForTankAsync(int tankId, string userId, int daysAhead = 7);

    /// <summary>
    /// Get a single parameter prediction
    /// </summary>
    /// <param name="tankId">The tank to analyze</param>
    /// <param name="parameterName">The specific parameter (e.g., "pH", "Ammonia")</param>
    /// <param name="userId">The user who owns the tank</param>
    /// <param name="daysAhead">How many days ahead to predict</param>
    /// <returns>Prediction for the specified parameter</returns>
    Task<WaterChemistryPredictionDTO?> GenerateParameterPredictionAsync(
        int tankId,
        string parameterName,
        string userId,
        int daysAhead = 7);

    /// <summary>
    /// Get historical predictions to evaluate accuracy
    /// This helps users see how well the ML model has performed
    /// </summary>
    /// <param name="tankId">The tank to check</param>
    /// <param name="userId">The user who owns the tank</param>
    /// <returns>List of past predictions with actual outcomes</returns>
    Task<List<WaterChemistryPrediction>> GetHistoricalPredictionsAsync(int tankId, string userId);

    /// <summary>
    /// Get historical water test records for a tank and user
    /// </summary>
    /// <param name="tankId">The tank to analyze</param>
    /// <param name="userId">The user who owns the tank</param>
    /// <returns>List of WaterTest records</returns>
    Task<List<WaterTest>> GetHistoricalWaterTestsAsync(int tankId, string userId);

    /// <summary>
    /// Extract parameter data points from water tests for charting
    /// </summary>
    /// <param name="tests">List of WaterTest records</param>
    /// <param name="parameterName">Parameter to extract (e.g., "pH")</param>
    /// <returns>List of (Date, Value) tuples</returns>
    List<(DateTime Date, double Value)> ExtractParameterData(List<WaterTest> tests, string parameterName);

    /// <summary>
    /// Save a prediction to the database for future reference
    /// </summary>
    Task SavePredictionAsync(WaterChemistryPrediction prediction);

    /// <summary>
    /// Validate prediction accuracy by comparing past predictions with actual results
    /// This is how we measure if our ML model is working well
    /// </summary>
    /// <param name="tankId">The tank to validate</param>
    /// <param name="userId">The user who owns the tank</param>
    /// <returns>Accuracy metrics (e.g., average error percentage)</returns>
    Task<PredictionAccuracyReport> ValidatePredictionAccuracyAsync(int tankId, string userId);
}

/// <summary>
/// Report showing how accurate our predictions have been
/// This is crucial for ML - we need to know if our model is working!
/// </summary>
public class PredictionAccuracyReport
{
    /// <summary>
    /// Average percentage error across all past predictions
    /// Lower is better! 5% means predictions are typically within 5% of actual values
    /// </summary>
    public double AverageErrorPercentage { get; set; }

    /// <summary>
    /// How many predictions were evaluated
    /// </summary>
    public int PredictionsEvaluated { get; set; }

    /// <summary>
    /// Percentage of predictions that were within 10% of actual value
    /// This is a good measure of "useful" predictions
    /// </summary>
    public double AccuracyWithin10Percent { get; set; }

    /// <summary>
    /// Detailed accuracy for each parameter type
    /// Some parameters may be easier to predict than others
    /// </summary>
    public Dictionary<string, double> AccuracyByParameter { get; set; } = new();

    /// <summary>
    /// Overall quality rating of the prediction model
    /// </summary>
    public string OverallRating { get; set; } = string.Empty;
}
