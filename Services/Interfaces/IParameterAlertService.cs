using System;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Services.Interfaces;

public interface IParameterAlertService
{
    // Alert Configuration Management
    Task<List<ParameterAlert>> GetAlertsByTankAsync(int tankId, string userId);
    Task<List<ParameterAlert>> GetAlertsByUserAsync(string userId);
    Task<ParameterAlert?> GetAlertByIdAsync(int alertId, string userId);
    Task<ParameterAlert> CreateAlertAsync(ParameterAlert alert);
    Task<ParameterAlert?> UpdateAlertAsync(ParameterAlert alert, string userId);
    Task<bool> DeleteAlertAsync(int alertId, string userId);
    Task<bool> ToggleAlertAsync(int alertId, string userId, bool isEnabled);

    // Default Alert Templates
    Task<List<ParameterAlert>> GetDefaultAlertsForTankAsync(int tankId, string userId);
    Task CreateDefaultAlertsForTankAsync(int tankId, string userId);

    // Alert Checking
    Task<List<TriggeredAlert>> CheckWaterTestAsync(int waterTestId);
    Task<List<TriggeredAlert>> CheckAllTanksForUserAsync(string userId);

    // Triggered Alerts
    Task<List<TriggeredAlert>> GetActiveTriggeredAlertsAsync(string userId);
    Task<List<TriggeredAlert>> GetTriggeredAlertsByTankAsync(int tankId, string userId);
    Task<TriggeredAlert?> GetTriggeredAlertByIdAsync(int triggeredAlertId, string userId);
    Task<bool> AcknowledgeAlertAsync(int triggeredAlertId, string userId);
    Task<bool> ResolveAlertAsync(int triggeredAlertId, string userId);
    Task AutoResolveAlertsAsync(int waterTestId);

    // Statistics
    Task<Dictionary<WaterParameter, int>> GetAlertCountsByParameterAsync(string userId, DateTime? since = null);
    Task<int> GetActiveAlertCountAsync(string userId);
}
