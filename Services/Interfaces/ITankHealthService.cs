using System;
using AquaHub.MVC.Models;

namespace AquaHub.MVC.Services.Interfaces;

public interface ITankHealthService
{
    // Get health score for a specific tank
    Task<TankHealthScore?> GetTankHealthScoreAsync(int tankId, string userId);

    // Get health scores for all user's tanks
    Task<List<TankHealthScore>> GetAllTankHealthScoresAsync(string userId);

    // Get historical health scores
    Task<List<TankHealthScore>> GetHealthScoreHistoryAsync(int tankId, string userId, DateTime? startDate = null);

    // Recalculate and update health score
    Task<TankHealthScore?> RecalculateHealthScoreAsync(int tankId, string userId);
}
