using System;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;
using AquaHub.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailNotifiactionService? _emailService;

    public NotificationService(ApplicationDbContext context, IEmailNotifiactionService? emailService = null)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
    {
        var query = _context.Notifications
            .Where(n => n.UserId == userId);

        if (unreadOnly)
            query = query.Where(n => !n.IsRead);

        return await query
            .Include(n => n.Tank)
            .Include(n => n.Reminder)
            .Include(n => n.WaterTest)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Notification?> GetNotificationByIdAsync(int id, string userId)
    {
        return await _context.Notifications
            .Include(n => n.Tank)
            .Include(n => n.Reminder)
            .Include(n => n.WaterTest)
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);
    }

    public async Task<Notification> CreateNotificationAsync(Notification notification)
    {
        notification.CreatedAt = DateTime.UtcNow;

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        // Send email if user has email notifications enabled
        await TrySendEmailNotificationAsync(notification);

        return notification;
    }

    public async Task<bool> MarkAsReadAsync(int id, string userId)
    {
        var notification = await GetNotificationByIdAsync(id, userId);
        if (notification == null)
            return false;

        notification.IsRead = true;
        notification.ReadAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(string userId)
    {
        var unreadNotifications = await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteNotificationAsync(int id, string userId)
    {
        var notification = await GetNotificationByIdAsync(id, userId);
        if (notification == null)
            return false;

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<int> GetUnreadCountAsync(string userId)
    {
        return await _context.Notifications
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    public async Task CreateReminderNotificationAsync(Reminder reminder)
    {
        var tankName = reminder.Tank?.Name ?? "your aquarium";
        var notification = new Notification
        {
            UserId = reminder.UserId,
            TankId = reminder.TankId,
            ReminderId = reminder.Id,
            Type = NotificationType.Reminder,
            Title = $"Reminder: {reminder.Title}",
            Message = $"{reminder.Description ?? reminder.Title} is due for {tankName} on {reminder.NextDueDate:MMM dd, yyyy 'at' h:mm tt}.",
            CreatedAt = DateTime.UtcNow
        };

        await CreateNotificationAsync(notification);
    }

    public async Task CreateWaterParameterAlertAsync(WaterTest waterTest, string parameterName, double value, double? minRange, double? maxRange)
    {
        var tank = await _context.Tanks.FindAsync(waterTest.TankId);
        var user = await _context.Users.FindAsync(tank?.UserId);

        if (tank == null || user == null)
            return;

        var rangeText = "";
        if (minRange.HasValue && maxRange.HasValue)
            rangeText = $" (Expected range: {minRange.Value} - {maxRange.Value})";
        else if (minRange.HasValue)
            rangeText = $" (Minimum: {minRange.Value})";
        else if (maxRange.HasValue)
            rangeText = $" (Maximum: {maxRange.Value})";

        var notification = new Notification
        {
            UserId = user.Id,
            TankId = waterTest.TankId,
            WaterTestId = waterTest.Id,
            Type = NotificationType.WaterParameterAlert,
            Title = $"Water Parameter Alert: {parameterName}",
            Message = $"The {parameterName} level in {tank.Name} is {value}{rangeText}. Recorded on {waterTest.Timestamp:MMM dd, yyyy}.",
            CreatedAt = DateTime.UtcNow
        };

        await CreateNotificationAsync(notification);
    }

    public async Task<UserNotificationSettings> GetUserSettingsAsync(string userId)
    {
        var settings = await _context.UserNotificationSettings
            .FirstOrDefaultAsync(s => s.UserId == userId);

        if (settings == null)
        {
            // Create default settings for new users
            settings = new UserNotificationSettings
            {
                UserId = userId,
                EmailNotificationsEnabled = true,
                ReminderNotificationsEnabled = true,
                WaterParameterAlertsEnabled = true,
                EquipmentAlertsEnabled = true,
                EmailDigestFrequencyHours = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.UserNotificationSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return settings;
    }

    public async Task<UserNotificationSettings> UpdateUserSettingsAsync(UserNotificationSettings settings)
    {
        settings.UpdatedAt = DateTime.UtcNow;

        _context.Entry(settings).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return settings;
    }

    private async Task TrySendEmailNotificationAsync(Notification notification)
    {
        if (_emailService == null)
            return;

        try
        {
            var settings = await GetUserSettingsAsync(notification.UserId);

            if (!settings.EmailNotificationsEnabled)
                return;

            var user = await _context.Users.FindAsync(notification.UserId);
            if (user == null || string.IsNullOrEmpty(user.Email))
                return;

            // Check notification type settings
            if (notification.Type == NotificationType.Reminder && !settings.ReminderNotificationsEnabled)
                return;
            if (notification.Type == NotificationType.WaterParameterAlert && !settings.WaterParameterAlertsEnabled)
                return;
            if (notification.Type == NotificationType.EquipmentAlert && !settings.EquipmentAlertsEnabled)
                return;

            // For instant notifications (digest frequency = 0)
            if (settings.EmailDigestFrequencyHours == 0)
            {
                if (notification.Type == NotificationType.Reminder && notification.Reminder != null)
                {
                    await _emailService.SendReminderEmailAsync(
                        user.Email,
                        user.FullName,
                        notification.Reminder.Title,
                        notification.Reminder.Description ?? "",
                        notification.Reminder.NextDueDate
                    );
                }
                else if (notification.Type == NotificationType.WaterParameterAlert && notification.WaterTest != null)
                {
                    var tank = await _context.Tanks.FindAsync(notification.TankId);
                    // This is a simplified version - you'd need to extract the parameter details
                    await _emailService.SendWaterParameterAlertEmailAsync(
                        user.Email,
                        user.FullName,
                        tank?.Name ?? "Unknown Tank",
                        "Parameter",
                        0,
                        null,
                        null
                    );
                }

                notification.EmailSent = true;
                await _context.SaveChangesAsync();
            }
        }
        catch
        {
            // Log error but don't fail the notification creation
            // In production, you'd want proper logging here
        }
    }
}
