using System;
using AquaHub.MVC.Models;

namespace AquaHub.MVC.Services.Interfaces;

public interface IReminderService
{
    Task<List<Reminder>> GetUserRemindersAsync(string userId);
    Task<List<Reminder>> GetActiveRemindersAsync(string userId);
    Task<List<Reminder>> GetUpcomingRemindersAsync(string userId, int hoursAhead = 168); // Default 7 days
    Task<Reminder?> GetReminderByIdAsync(int id, string userId);
    Task<Reminder> CreateReminderAsync(Reminder reminder);
    Task<Reminder> UpdateReminderAsync(Reminder reminder);
    Task<bool> DeleteReminderAsync(int id, string userId);
    Task<bool> CompleteReminderAsync(int id, string userId);
    Task<List<Reminder>> GetOverdueRemindersAsync(string userId);
    Task<List<Reminder>> CheckAndGenerateNotificationsAsync();
}
