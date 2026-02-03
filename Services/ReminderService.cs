using System;
using AquaHub.MVC.Data;
using AquaHub.MVC.Models;
using AquaHub.MVC.Models.Enums;
using AquaHub.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AquaHub.MVC.Services;

public class ReminderService : IReminderService
{
    private readonly ApplicationDbContext _context;
    private readonly INotificationService _notificationService;

    public ReminderService(ApplicationDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<List<Reminder>> GetUserRemindersAsync(string userId)
    {
        return await _context.Reminders
            .Where(r => r.UserId == userId)
            .Include(r => r.Tank)
            .OrderBy(r => r.NextDueDate)
            .ToListAsync();
    }

    public async Task<List<Reminder>> GetActiveRemindersAsync(string userId)
    {
        return await _context.Reminders
            .Where(r => r.UserId == userId && r.IsActive)
            .Include(r => r.Tank)
            .OrderBy(r => r.NextDueDate)
            .ToListAsync();
    }

    public async Task<List<Reminder>> GetUpcomingRemindersAsync(string userId, int hoursAhead = 168)
    {
        var cutoffDate = DateTime.UtcNow.AddHours(hoursAhead);
        return await _context.Reminders
            .Where(r => r.UserId == userId &&
                        r.IsActive &&
                        r.NextDueDate <= cutoffDate)
            .Include(r => r.Tank)
            .OrderBy(r => r.NextDueDate)
            .ToListAsync();
    }

    public async Task<Reminder?> GetReminderByIdAsync(int id, string userId)
    {
        return await _context.Reminders
            .Include(r => r.Tank)
            .FirstOrDefaultAsync(r => r.Id == id && r.UserId == userId);
    }

    public async Task<Reminder> CreateReminderAsync(Reminder reminder)
    {
        reminder.CreatedAt = DateTime.UtcNow;
        reminder.UpdatedAt = DateTime.UtcNow;

        _context.Reminders.Add(reminder);
        await _context.SaveChangesAsync();

        return reminder;
    }

    public async Task<Reminder> UpdateReminderAsync(Reminder reminder)
    {
        reminder.UpdatedAt = DateTime.UtcNow;

        _context.Entry(reminder).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return reminder;
    }

    public async Task<bool> DeleteReminderAsync(int id, string userId)
    {
        var reminder = await GetReminderByIdAsync(id, userId);
        if (reminder == null)
            return false;

        _context.Reminders.Remove(reminder);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CompleteReminderAsync(int id, string userId)
    {
        var reminder = await GetReminderByIdAsync(id, userId);
        if (reminder == null)
            return false;

        reminder.LastCompletedDate = DateTime.UtcNow;

        // Calculate next due date based on frequency
        reminder.NextDueDate = CalculateNextDueDate(reminder.NextDueDate, reminder.Frequency);

        // If it's a one-time reminder, deactivate it
        if (reminder.Frequency == ReminderFrequency.Once)
        {
            reminder.IsActive = false;
        }

        reminder.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<Reminder>> GetOverdueRemindersAsync(string userId)
    {
        return await _context.Reminders
            .Where(r => r.UserId == userId &&
                        r.IsActive &&
                        r.NextDueDate < DateTime.UtcNow)
            .Include(r => r.Tank)
            .OrderBy(r => r.NextDueDate)
            .ToListAsync();
    }

    public async Task<List<Reminder>> CheckAndGenerateNotificationsAsync()
    {
        var now = DateTime.UtcNow;
        var remindersNeedingNotification = new List<Reminder>();

        // Get all active reminders
        var activeReminders = await _context.Reminders
            .Where(r => r.IsActive)
            .Include(r => r.Tank)
            .ToListAsync();

        foreach (var reminder in activeReminders)
        {
            var notificationTime = reminder.NextDueDate.AddHours(-reminder.NotificationHoursBefore);

            // Check if it's time to send a notification
            if (now >= notificationTime && now < reminder.NextDueDate)
            {
                // Check if a notification already exists for this reminder
                var existingNotification = await _context.Notifications
                    .Where(n => n.ReminderId == reminder.Id &&
                               n.CreatedAt >= notificationTime)
                    .FirstOrDefaultAsync();

                if (existingNotification == null)
                {
                    await _notificationService.CreateReminderNotificationAsync(reminder);
                    remindersNeedingNotification.Add(reminder);
                }
            }
        }

        return remindersNeedingNotification;
    }

    private DateTime CalculateNextDueDate(DateTime currentDueDate, ReminderFrequency frequency)
    {
        return frequency switch
        {
            ReminderFrequency.Daily => currentDueDate.AddDays(1),
            ReminderFrequency.Weekly => currentDueDate.AddDays(7),
            ReminderFrequency.BiWeekly => currentDueDate.AddDays(14),
            ReminderFrequency.Monthly => currentDueDate.AddMonths(1),
            ReminderFrequency.Quarterly => currentDueDate.AddMonths(3),
            ReminderFrequency.Yearly => currentDueDate.AddYears(1),
            _ => currentDueDate
        };
    }
}
