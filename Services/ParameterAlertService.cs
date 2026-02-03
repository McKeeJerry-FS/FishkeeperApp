using System;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;
using AquaHub.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class ParameterAlertService : IParameterAlertService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ParameterAlertService> _logger;

    public ParameterAlertService(
        ApplicationDbContext context,
        INotificationService notificationService,
        ILogger<ParameterAlertService> logger)
    {
        _context = context;
        _notificationService = notificationService;
        _logger = logger;
    }

    #region Alert Configuration Management

    public async Task<List<ParameterAlert>> GetAlertsByTankAsync(int tankId, string userId)
    {
        return await _context.ParameterAlerts
            .Include(a => a.Tank)
            .Where(a => a.TankId == tankId && a.UserId == userId)
            .OrderBy(a => a.Parameter)
            .ToListAsync();
    }

    public async Task<List<ParameterAlert>> GetAlertsByUserAsync(string userId)
    {
        return await _context.ParameterAlerts
            .Include(a => a.Tank)
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.Tank!.Name)
            .ThenBy(a => a.Parameter)
            .ToListAsync();
    }

    public async Task<ParameterAlert?> GetAlertByIdAsync(int alertId, string userId)
    {
        return await _context.ParameterAlerts
            .Include(a => a.Tank)
            .FirstOrDefaultAsync(a => a.Id == alertId && a.UserId == userId);
    }

    public async Task<ParameterAlert> CreateAlertAsync(ParameterAlert alert)
    {
        alert.CreatedAt = DateTime.UtcNow;
        alert.UpdatedAt = DateTime.UtcNow;

        _context.ParameterAlerts.Add(alert);
        await _context.SaveChangesAsync();

        return alert;
    }

    public async Task<ParameterAlert?> UpdateAlertAsync(ParameterAlert alert, string userId)
    {
        var existing = await GetAlertByIdAsync(alert.Id, userId);
        if (existing == null)
            return null;

        existing.MinValue = alert.MinValue;
        existing.MaxValue = alert.MaxValue;
        existing.IsEnabled = alert.IsEnabled;
        existing.Severity = alert.Severity;
        existing.CustomMessage = alert.CustomMessage;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAlertAsync(int alertId, string userId)
    {
        var alert = await GetAlertByIdAsync(alertId, userId);
        if (alert == null)
            return false;

        _context.ParameterAlerts.Remove(alert);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleAlertAsync(int alertId, string userId, bool isEnabled)
    {
        var alert = await GetAlertByIdAsync(alertId, userId);
        if (alert == null)
            return false;

        alert.IsEnabled = isEnabled;
        alert.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Default Alert Templates

    public async Task<List<ParameterAlert>> GetDefaultAlertsForTankAsync(int tankId, string userId)
    {
        var tank = await _context.Tanks
            .FirstOrDefaultAsync(t => t.Id == tankId && t.UserId == userId);

        if (tank == null)
            return new List<ParameterAlert>();

        var alerts = new List<ParameterAlert>();

        // Shared parameters for all tank types
        alerts.Add(CreateDefaultAlert(tankId, userId, WaterParameter.PH, 6.5, 8.5, AlertSeverity.Warning));
        alerts.Add(CreateDefaultAlert(tankId, userId, WaterParameter.Temperature, 72, 82, AlertSeverity.Caution));
        alerts.Add(CreateDefaultAlert(tankId, userId, WaterParameter.Ammonia, null, 0.25, AlertSeverity.Critical));
        alerts.Add(CreateDefaultAlert(tankId, userId, WaterParameter.Nitrite, null, 0.5, AlertSeverity.Critical));
        alerts.Add(CreateDefaultAlert(tankId, userId, WaterParameter.Nitrate, null, 40, AlertSeverity.Warning));

        // Tank type specific parameters
        switch (tank.Type)
        {
            case AquariumType.Freshwater:
            case AquariumType.Planted:
                alerts.Add(CreateDefaultAlert(tankId, userId, WaterParameter.GH, 4, 12, AlertSeverity.Info));
                alerts.Add(CreateDefaultAlert(tankId, userId, WaterParameter.KH, 3, 10, AlertSeverity.Info));
                alerts.Add(CreateDefaultAlert(tankId, userId, WaterParameter.TDS, 100, 400, AlertSeverity.Warning));
                break;

            case AquariumType.Saltwater:
            case AquariumType.Reef:
                alerts.Add(CreateDefaultAlert(tankId, userId, WaterParameter.Salinity, 1.023, 1.026, AlertSeverity.Caution));
                alerts.Add(CreateDefaultAlert(tankId, userId, WaterParameter.Alkalinity, 8, 12, AlertSeverity.Warning));
                alerts.Add(CreateDefaultAlert(tankId, userId, WaterParameter.Calcium, 380, 450, AlertSeverity.Warning));
                alerts.Add(CreateDefaultAlert(tankId, userId, WaterParameter.Magnesium, 1250, 1400, AlertSeverity.Info));
                alerts.Add(CreateDefaultAlert(tankId, userId, WaterParameter.Phosphate, null, 0.1, AlertSeverity.Warning));
                break;

            case AquariumType.Brackish:
                alerts.Add(CreateDefaultAlert(tankId, userId, WaterParameter.Salinity, 1.005, 1.015, AlertSeverity.Warning));
                alerts.Add(CreateDefaultAlert(tankId, userId, WaterParameter.GH, 8, 20, AlertSeverity.Info));
                break;
        }

        return alerts;
    }

    public async Task CreateDefaultAlertsForTankAsync(int tankId, string userId)
    {
        var defaultAlerts = await GetDefaultAlertsForTankAsync(tankId, userId);

        foreach (var alert in defaultAlerts)
        {
            await CreateAlertAsync(alert);
        }

        _logger.LogInformation($"Created {defaultAlerts.Count} default alerts for tank {tankId}");
    }

    private ParameterAlert CreateDefaultAlert(
        int tankId,
        string userId,
        WaterParameter parameter,
        double? minValue,
        double? maxValue,
        AlertSeverity severity)
    {
        return new ParameterAlert
        {
            TankId = tankId,
            UserId = userId,
            Parameter = parameter,
            MinValue = minValue,
            MaxValue = maxValue,
            Severity = severity,
            IsEnabled = true
        };
    }

    #endregion

    #region Alert Checking

    public async Task<List<TriggeredAlert>> CheckWaterTestAsync(int waterTestId)
    {
        var waterTest = await _context.WaterTests
            .Include(wt => wt.Tank)
            .FirstOrDefaultAsync(wt => wt.Id == waterTestId);

        if (waterTest == null || waterTest.Tank == null)
            return new List<TriggeredAlert>();

        var alerts = await _context.ParameterAlerts
            .Where(a => a.TankId == waterTest.TankId && a.IsEnabled)
            .ToListAsync();

        var triggeredAlerts = new List<TriggeredAlert>();

        foreach (var alert in alerts)
        {
            var value = GetParameterValue(waterTest, alert.Parameter);
            if (value.HasValue && IsOutOfRange(value.Value, alert.MinValue, alert.MaxValue))
            {
                var triggeredAlert = await CreateTriggeredAlertAsync(alert, waterTest, value.Value);
                triggeredAlerts.Add(triggeredAlert);

                // Update alert statistics
                alert.LastTriggeredAt = DateTime.UtcNow;
                alert.TriggerCount++;

                // Create notification
                await _notificationService.CreateNotificationAsync(new Notification
                {
                    UserId = alert.UserId,
                    TankId = alert.TankId,
                    Title = $"⚠️ {alert.Parameter} Alert",
                    Message = triggeredAlert.Message,
                    Type = NotificationType.ParameterAlert,
                    Severity = alert.Severity,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow,
                    ActionUrl = $"/tanks/{alert.TankId}/water-tests"
                });
            }
        }

        if (triggeredAlerts.Any())
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Triggered {triggeredAlerts.Count} alerts for water test {waterTestId}");
        }

        // Auto-resolve any previous alerts that are now back in range
        await AutoResolveAlertsAsync(waterTestId);

        return triggeredAlerts;
    }

    public async Task<List<TriggeredAlert>> CheckAllTanksForUserAsync(string userId)
    {
        var tanks = await _context.Tanks
            .Where(t => t.UserId == userId)
            .ToListAsync();

        var allTriggeredAlerts = new List<TriggeredAlert>();

        foreach (var tank in tanks)
        {
            var latestTest = await _context.WaterTests
                .Where(wt => wt.TankId == tank.Id)
                .OrderByDescending(wt => wt.Timestamp)
                .FirstOrDefaultAsync();

            if (latestTest != null)
            {
                var triggeredAlerts = await CheckWaterTestAsync(latestTest.Id);
                allTriggeredAlerts.AddRange(triggeredAlerts);
            }
        }

        return allTriggeredAlerts;
    }

    private async Task<TriggeredAlert> CreateTriggeredAlertAsync(
        ParameterAlert alert,
        WaterTest waterTest,
        double actualValue)
    {
        var message = alert.CustomMessage ?? GenerateAlertMessage(alert.Parameter, actualValue, alert.MinValue, alert.MaxValue);

        var triggeredAlert = new TriggeredAlert
        {
            ParameterAlertId = alert.Id,
            WaterTestId = waterTest.Id,
            TankId = alert.TankId,
            UserId = alert.UserId,
            Parameter = alert.Parameter,
            ActualValue = actualValue,
            MinSafeValue = alert.MinValue,
            MaxSafeValue = alert.MaxValue,
            Severity = alert.Severity,
            Message = message,
            TriggeredAt = DateTime.UtcNow
        };

        _context.TriggeredAlerts.Add(triggeredAlert);
        return triggeredAlert;
    }

    private double? GetParameterValue(WaterTest waterTest, WaterParameter parameter)
    {
        return parameter switch
        {
            WaterParameter.PH => waterTest.PH,
            WaterParameter.Temperature => waterTest.Temperature,
            WaterParameter.Ammonia => waterTest.Ammonia,
            WaterParameter.Nitrite => waterTest.Nitrite,
            WaterParameter.Nitrate => waterTest.Nitrate,
            WaterParameter.GH => waterTest.GH,
            WaterParameter.KH => waterTest.KH,
            WaterParameter.TDS => waterTest.TDS,
            WaterParameter.Salinity => waterTest.Salinity,
            WaterParameter.Alkalinity => waterTest.Alkalinity,
            WaterParameter.Calcium => waterTest.Calcium,
            WaterParameter.Magnesium => waterTest.Magnesium,
            WaterParameter.Phosphate => waterTest.Phosphate,
            _ => null
        };
    }

    private bool IsOutOfRange(double value, double? minValue, double? maxValue)
    {
        if (minValue.HasValue && value < minValue.Value)
            return true;
        if (maxValue.HasValue && value > maxValue.Value)
            return true;
        return false;
    }

    private string GenerateAlertMessage(WaterParameter parameter, double actualValue, double? minValue, double? maxValue)
    {
        var paramName = parameter.ToString();
        var unit = GetParameterUnit(parameter);

        if (minValue.HasValue && actualValue < minValue.Value)
        {
            return $"{paramName} is too low: {actualValue}{unit} (safe range: {minValue}{unit} - {maxValue}{unit})";
        }
        else if (maxValue.HasValue && actualValue > maxValue.Value)
        {
            return $"{paramName} is too high: {actualValue}{unit} (safe range: {minValue}{unit} - {maxValue}{unit})";
        }

        return $"{paramName} is out of safe range: {actualValue}{unit}";
    }

    private string GetParameterUnit(WaterParameter parameter)
    {
        return parameter switch
        {
            WaterParameter.PH => "",
            WaterParameter.Temperature => "°F",
            WaterParameter.Ammonia => " ppm",
            WaterParameter.Nitrite => " ppm",
            WaterParameter.Nitrate => " ppm",
            WaterParameter.GH => " dGH",
            WaterParameter.KH => " dKH",
            WaterParameter.TDS => " ppm",
            WaterParameter.Salinity => " sg",
            WaterParameter.Alkalinity => " dKH",
            WaterParameter.Calcium => " ppm",
            WaterParameter.Magnesium => " ppm",
            WaterParameter.Phosphate => " ppm",
            _ => ""
        };
    }

    #endregion

    #region Triggered Alerts

    public async Task<List<TriggeredAlert>> GetActiveTriggeredAlertsAsync(string userId)
    {
        return await _context.TriggeredAlerts
            .Include(ta => ta.Tank)
            .Include(ta => ta.ParameterAlert)
            .Where(ta => ta.UserId == userId && !ta.IsResolved)
            .OrderByDescending(ta => ta.Severity)
            .ThenByDescending(ta => ta.TriggeredAt)
            .ToListAsync();
    }

    public async Task<List<TriggeredAlert>> GetTriggeredAlertsByTankAsync(int tankId, string userId)
    {
        return await _context.TriggeredAlerts
            .Include(ta => ta.ParameterAlert)
            .Include(ta => ta.WaterTest)
            .Where(ta => ta.TankId == tankId && ta.UserId == userId)
            .OrderByDescending(ta => ta.TriggeredAt)
            .Take(50)
            .ToListAsync();
    }

    public async Task<TriggeredAlert?> GetTriggeredAlertByIdAsync(int triggeredAlertId, string userId)
    {
        return await _context.TriggeredAlerts
            .Include(ta => ta.Tank)
            .Include(ta => ta.ParameterAlert)
            .Include(ta => ta.WaterTest)
            .FirstOrDefaultAsync(ta => ta.Id == triggeredAlertId && ta.UserId == userId);
    }

    public async Task<bool> AcknowledgeAlertAsync(int triggeredAlertId, string userId)
    {
        var alert = await GetTriggeredAlertByIdAsync(triggeredAlertId, userId);
        if (alert == null)
            return false;

        alert.IsAcknowledged = true;
        alert.AcknowledgedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResolveAlertAsync(int triggeredAlertId, string userId)
    {
        var alert = await GetTriggeredAlertByIdAsync(triggeredAlertId, userId);
        if (alert == null)
            return false;

        alert.IsResolved = true;
        alert.ResolvedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task AutoResolveAlertsAsync(int waterTestId)
    {
        var waterTest = await _context.WaterTests
            .FirstOrDefaultAsync(wt => wt.Id == waterTestId);

        if (waterTest == null)
            return;

        var unresolvedAlerts = await _context.TriggeredAlerts
            .Include(ta => ta.ParameterAlert)
            .Where(ta => ta.TankId == waterTest.TankId && !ta.IsResolved)
            .ToListAsync();

        foreach (var triggeredAlert in unresolvedAlerts)
        {
            if (triggeredAlert.ParameterAlert == null)
                continue;

            var currentValue = GetParameterValue(waterTest, triggeredAlert.Parameter);
            if (currentValue.HasValue)
            {
                var isNowInRange = !IsOutOfRange(
                    currentValue.Value,
                    triggeredAlert.ParameterAlert.MinValue,
                    triggeredAlert.ParameterAlert.MaxValue);

                if (isNowInRange)
                {
                    triggeredAlert.IsResolved = true;
                    triggeredAlert.ResolvedAt = DateTime.UtcNow;
                    triggeredAlert.ResolvedByWaterTestId = waterTestId;

                    _logger.LogInformation($"Auto-resolved alert {triggeredAlert.Id} for parameter {triggeredAlert.Parameter}");
                }
            }
        }

        await _context.SaveChangesAsync();
    }

    #endregion

    #region Statistics

    public async Task<Dictionary<WaterParameter, int>> GetAlertCountsByParameterAsync(string userId, DateTime? since = null)
    {
        var query = _context.TriggeredAlerts
            .Where(ta => ta.UserId == userId);

        if (since.HasValue)
        {
            query = query.Where(ta => ta.TriggeredAt >= since.Value);
        }

        var counts = await query
            .GroupBy(ta => ta.Parameter)
            .Select(g => new { Parameter = g.Key, Count = g.Count() })
            .ToListAsync();

        return counts.ToDictionary(x => x.Parameter, x => x.Count);
    }

    public async Task<int> GetActiveAlertCountAsync(string userId)
    {
        return await _context.TriggeredAlerts
            .Where(ta => ta.UserId == userId && !ta.IsResolved)
            .CountAsync();
    }

    #endregion
}
