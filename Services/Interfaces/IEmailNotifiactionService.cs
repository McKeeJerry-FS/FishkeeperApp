using System;

namespace AquaHub.MVC.Services.Interfaces;

public interface IEmailNotifiactionService
{
    Task SendReminderEmailAsync(string userEmail, string userName, string reminderTitle, string reminderDescription, DateTime dueDate);
    Task SendWaterParameterAlertEmailAsync(string userEmail, string userName, string tankName, string parameterName, double value, double? minRange, double? maxRange);
    Task SendNotificationDigestEmailAsync(string userEmail, string userName, List<string> notifications);
}
