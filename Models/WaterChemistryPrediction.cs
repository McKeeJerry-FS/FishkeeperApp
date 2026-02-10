using System;
using System.ComponentModel.DataAnnotations;

namespace AquaHub.MVC.Models;

/// <summary>
/// Represents a predicted water chemistry reading for a specific parameter.
/// This model stores machine learning predictions about future water parameter values
/// to help aquarists anticipate and prevent water quality issues.
/// </summary>
/// <remarks>
/// Machine Learning Concept: A prediction is an estimated future value based on historical patterns.
/// For example, if pH has been dropping 0.1 every 3 days, we can predict it will be 7.6 in 3 days.
/// </remarks>
public class WaterChemistryPrediction
{
    /// <summary>
    /// Unique identifier for this prediction record
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The tank this prediction belongs to
    /// </summary>
    [Required]
    public int TankId { get; set; }

    /// <summary>
    /// Navigation property to the Tank entity
    /// </summary>
    public Tank? Tank { get; set; }

    /// <summary>
    /// The water parameter being predicted (e.g., "pH", "Ammonia", "Nitrate")
    /// </summary>
    [Required]
    [StringLength(50)]
    public string ParameterName { get; set; } = string.Empty;

    /// <summary>
    /// The predicted value for this parameter
    /// ML Note: This is our model's "output" - what we think the value will be
    /// </summary>
    public double PredictedValue { get; set; }

    /// <summary>
    /// The current (most recent) actual measured value
    /// This provides context for how much change is expected
    /// </summary>
    public double CurrentValue { get; set; }

    /// <summary>
    /// When this prediction was made
    /// </summary>
    public DateTime PredictionDate { get; set; }

    /// <summary>
    /// When we expect this predicted value to occur
    /// For example, if today is Jan 1 and we predict pH 7.5 on Jan 5, this would be Jan 5
    /// </summary>
    public DateTime PredictedDate { get; set; }

    /// <summary>
    /// Confidence score (0-1, where 1 is 100% confident)
    /// ML Note: This represents how "sure" our model is about this prediction
    /// Higher confidence = more historical data and clearer patterns
    /// Lower confidence = less data or more erratic/random historical values
    /// </summary>
    [Range(0, 1)]
    public double ConfidenceScore { get; set; }

    /// <summary>
    /// The trend direction: "Increasing", "Decreasing", "Stable"
    /// This helps users quickly understand if values are getting better or worse
    /// </summary>
    [StringLength(20)]
    public string Trend { get; set; } = string.Empty;

    /// <summary>
    /// How many days into the future this prediction is for
    /// For example, 3 means "3 days from now"
    /// </summary>
    public int DaysAhead { get; set; }

    /// <summary>
    /// Whether this prediction indicates the parameter will be outside safe ranges
    /// This triggers warnings to the user
    /// </summary>
    public bool IsWarning { get; set; }

    /// <summary>
    /// Optional message explaining the prediction or suggesting actions
    /// Example: "pH dropping below safe range - consider adding buffer"
    /// </summary>
    [StringLength(500)]
    public string? Message { get; set; }

    /// <summary>
    /// The algorithm/method used for this prediction
    /// Examples: "Linear Regression", "Weighted Moving Average", "Exponential Smoothing"
    /// ML Note: Different algorithms work better for different patterns
    /// </summary>
    [StringLength(100)]
    public string PredictionMethod { get; set; } = string.Empty;

    /// <summary>
    /// How many historical data points were used to make this prediction
    /// More data points generally means more reliable predictions
    /// </summary>
    public int DataPointsUsed { get; set; }

    /// <summary>
    /// The rate of change per day
    /// Example: -0.05 means the parameter drops by 0.05 units per day
    /// ML Note: This is the "slope" in linear regression - how fast values change
    /// </summary>
    public double RateOfChange { get; set; }
}

/// <summary>
/// Data Transfer Object for displaying prediction results to users
/// This separates our internal data structure from what we show in views
/// </summary>
/// <remarks>
/// Design Pattern: DTOs (Data Transfer Objects) help us control exactly what information
/// gets sent to the user interface, keeping our models clean and secure.
/// </remarks>
public class WaterChemistryPredictionDTO
{
    public string ParameterName { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double PredictedValue { get; set; }
    public double Change { get; set; }
    public double ChangePercentage { get; set; }
    public string Trend { get; set; } = string.Empty;
    public DateTime PredictedDate { get; set; }
    public int DaysAhead { get; set; }
    public double ConfidenceScore { get; set; }
    public bool IsWarning { get; set; }
    public string? Message { get; set; }
    public string PredictionMethod { get; set; } = string.Empty;
    public int DataPointsUsed { get; set; }
    public double RateOfChange { get; set; }

    /// <summary>
    /// Visual indicator for the user: "success", "warning", "danger"
    /// Used for color-coding in the UI
    /// </summary>
    public string AlertLevel { get; set; } = string.Empty;
}

/// <summary>
/// Aggregated prediction results for a tank
/// Provides an overview of all parameter predictions
/// </summary>
public class TankPredictionSummary
{
    public int TankId { get; set; }
    public string TankName { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// All individual parameter predictions
    /// </summary>
    public List<WaterChemistryPredictionDTO> Predictions { get; set; } = new();

    /// <summary>
    /// Overall health forecast: "Excellent", "Good", "Fair", "Concerning"
    /// ML Note: This is an aggregate score based on all predictions
    /// </summary>
    public string OverallForecast { get; set; } = string.Empty;

    /// <summary>
    /// Number of predictions that indicate warnings
    /// </summary>
    public int WarningCount { get; set; }

    /// <summary>
    /// Average confidence across all predictions (0-100%)
    /// Higher average = more reliable overall forecast
    /// </summary>
    public double AverageConfidence { get; set; }

    /// <summary>
    /// Whether the model has enough data to make reliable predictions
    /// ML Note: We need a minimum number of historical readings (typically 10-15)
    /// to start making useful predictions
    /// </summary>
    public bool HasSufficientData { get; set; }

    /// <summary>
    /// Message to display if insufficient data
    /// </summary>
    public string? InsufficientDataMessage { get; set; }
}
