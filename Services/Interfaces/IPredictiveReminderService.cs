using System;
using AquaHub.MVC.Models;

namespace AquaHub.MVC.Services.Interfaces;

public interface IPredictiveReminderService
{
    // Get all active predictions for a user
    Task<List<PredictiveReminder>> GetActivePredictionsAsync(string userId);

    // Get predictions for a specific tank
    Task<List<PredictiveReminder>> GetTankPredictionsAsync(int tankId, string userId);

    // Generate new predictions based on user patterns
    Task<List<PredictiveReminder>> GeneratePredictionsAsync(string userId);

    // Generate predictions for a specific tank
    Task<List<PredictiveReminder>> GenerateTankPredictionsAsync(int tankId, string userId);

    // Accept a prediction and create a real reminder
    Task<Reminder?> AcceptPredictionAsync(int predictionId, string userId);

    // Dismiss a prediction
    Task<bool> DismissPredictionAsync(int predictionId, string userId);

    // Clean up expired or acted-upon predictions
    Task CleanupOldPredictionsAsync();
}
