using System;
using AquaHub.MVC.Models;

namespace AquaHub.MVC.Services.Interfaces;

public interface INotificationService
{
    Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
    Task<Notification?> GetNotificationByIdAsync(int id, string userId);
    Task<Notification> CreateNotificationAsync(Notification notification);
    Task<bool> MarkAsReadAsync(int id, string userId);
    Task<bool> MarkAllAsReadAsync(string userId);
    Task<bool> DeleteNotificationAsync(int id, string userId);
    Task<int> GetUnreadCountAsync(string userId);
    Task CreateReminderNotificationAsync(Reminder reminder);
    Task CreateWaterParameterAlertAsync(WaterTest waterTest, string parameterName, double value, double? minRange, double? maxRange);
    Task<UserNotificationSettings> GetUserSettingsAsync(string userId);
    Task<UserNotificationSettings> UpdateUserSettingsAsync(UserNotificationSettings settings);
}
