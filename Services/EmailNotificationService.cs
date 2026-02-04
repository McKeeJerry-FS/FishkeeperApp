using System;
using AquaHub.MVC.Models;
using AquaHub.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace AquaHub.MVC.Services;

public class EmailNotificationService : IEmailNotifiactionService
{
    private readonly IEmailSender _emailSender;

    public EmailNotificationService(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task SendReminderEmailAsync(string userEmail, string userName, string reminderTitle, string reminderDescription, DateTime dueDate)
    {
        var subject = $"AquaHub Reminder: {reminderTitle}";

        var htmlMessage = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
                    .reminder-card {{ background: white; padding: 20px; border-left: 4px solid #667eea; margin: 20px 0; border-radius: 4px; }}
                    .due-date {{ color: #e74c3c; font-weight: bold; font-size: 1.1em; }}
                    .footer {{ text-align: center; padding: 20px; color: #666; font-size: 0.9em; }}
                    .btn {{ display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>🐠 AquaHub Reminder</h1>
                    </div>
                    <div class='content'>
                        <p>Hi {userName},</p>
                        
                        <div class='reminder-card'>
                            <h2>{reminderTitle}</h2>
                            {(!string.IsNullOrEmpty(reminderDescription) ? $"<p>{reminderDescription}</p>" : "")}
                            <p class='due-date'>⏰ Due: {dueDate:MMMM dd, yyyy 'at' h:mm tt}</p>
                        </div>
                        
                        <p>Don't forget to complete this task to keep your aquarium healthy and thriving!</p>
                        
                        <a href='#' class='btn'>View in AquaHub</a>
                    </div>
                    <div class='footer'>
                        <p>This is an automated notification from AquaHub.</p>
                        <p>To manage your notification preferences, visit your account settings.</p>
                    </div>
                </div>
            </body>
            </html>";

        await _emailSender.SendEmailAsync(userEmail, subject, htmlMessage);
    }

    public async Task SendWaterParameterAlertEmailAsync(string userEmail, string userName, string tankName, string parameterName, double value, double? minRange, double? maxRange)
    {
        var subject = $"⚠️ AquaHub Alert: {parameterName} out of range in {tankName}";

        var rangeInfo = "";
        if (minRange.HasValue && maxRange.HasValue)
            rangeInfo = $"<p><strong>Expected Range:</strong> {minRange.Value} - {maxRange.Value}</p>";
        else if (minRange.HasValue)
            rangeInfo = $"<p><strong>Minimum:</strong> {minRange.Value}</p>";
        else if (maxRange.HasValue)
            rangeInfo = $"<p><strong>Maximum:</strong> {maxRange.Value}</p>";

        var htmlMessage = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #e74c3c 0%, #c0392b 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
                    .alert-card {{ background: white; padding: 20px; border-left: 4px solid #e74c3c; margin: 20px 0; border-radius: 4px; }}
                    .parameter-value {{ color: #e74c3c; font-weight: bold; font-size: 1.3em; }}
                    .warning-icon {{ font-size: 3em; text-align: center; }}
                    .footer {{ text-align: center; padding: 20px; color: #666; font-size: 0.9em; }}
                    .btn {{ display: inline-block; padding: 12px 30px; background: #e74c3c; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                    .action-steps {{ background: #fff3cd; padding: 15px; border-radius: 5px; margin: 15px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <div class='warning-icon'>⚠️</div>
                        <h1>Water Parameter Alert</h1>
                    </div>
                    <div class='content'>
                        <p>Hi {userName},</p>
                        
                        <div class='alert-card'>
                            <h2>{tankName}</h2>
                            <p><strong>{parameterName}:</strong> <span class='parameter-value'>{value}</span></p>
                            {rangeInfo}
                        </div>
                        
                        <div class='action-steps'>
                            <h3>Recommended Actions:</h3>
                            <ul>
                                <li>Test the water again to confirm the reading</li>
                                <li>Check your filtration system</li>
                                <li>Consider a partial water change</li>
                                <li>Monitor your livestock for signs of stress</li>
                            </ul>
                        </div>
                        
                        <p>Take action promptly to ensure the health of your aquarium inhabitants.</p>
                        
                        <a href='#' class='btn'>View Water Test Details</a>
                    </div>
                    <div class='footer'>
                        <p>This is an automated alert from AquaHub.</p>
                        <p>To manage your notification preferences, visit your account settings.</p>
                    </div>
                </div>
            </body>
            </html>";

        await _emailSender.SendEmailAsync(userEmail, subject, htmlMessage);
    }

    public async Task SendNotificationDigestEmailAsync(string userEmail, string userName, List<string> notifications)
    {
        var subject = $"AquaHub Notification Digest - {notifications.Count} notification(s)";

        var notificationList = string.Join("", notifications.Select(n => $"<li style='padding: 10px; border-bottom: 1px solid #eee;'>{n}</li>"));

        var htmlMessage = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #3498db 0%, #2980b9 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
                    .notification-list {{ background: white; padding: 0; margin: 20px 0; border-radius: 4px; list-style: none; }}
                    .count-badge {{ background: #e74c3c; color: white; padding: 5px 15px; border-radius: 20px; font-size: 0.9em; }}
                    .footer {{ text-align: center; padding: 20px; color: #666; font-size: 0.9em; }}
                    .btn {{ display: inline-block; padding: 12px 30px; background: #3498db; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>🔔 Your AquaHub Digest</h1>
                        <p>You have <span class='count-badge'>{notifications.Count}</span> new notification(s)</p>
                    </div>
                    <div class='content'>
                        <p>Hi {userName},</p>
                        
                        <p>Here's a summary of your recent aquarium notifications:</p>
                        
                        <ul class='notification-list'>
                            {notificationList}
                        </ul>
                        
                        <a href='#' class='btn'>View All Notifications</a>
                    </div>
                    <div class='footer'>
                        <p>This is an automated digest from AquaHub.</p>
                        <p>To adjust your digest frequency or manage notification preferences, visit your account settings.</p>
                    </div>
                </div>
            </body>
            </html>";

        await _emailSender.SendEmailAsync(userEmail, subject, htmlMessage);
    }

    public async Task SendLowStockAlertEmailAsync(string userEmail, string userName, string supplyName, double currentQuantity, double minimumQuantity, string unit, string? tankName = null)
    {
        var subject = $"🔴 AquaHub: Low Stock Alert - {supplyName}";

        var tankInfo = !string.IsNullOrEmpty(tankName) ? $"for {tankName}" : "";

        var htmlMessage = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #f39c12 0%, #e67e22 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
                    .stock-card {{ background: white; padding: 20px; border-left: 4px solid #f39c12; margin: 20px 0; border-radius: 4px; }}
                    .stock-level {{ color: #e67e22; font-weight: bold; font-size: 1.2em; }}
                    .footer {{ text-align: center; padding: 20px; color: #666; font-size: 0.9em; }}
                    .btn {{ display: inline-block; padding: 12px 30px; background: #f39c12; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>📦 Low Stock Alert</h1>
                    </div>
                    <div class='content'>
                        <p>Hi {userName},</p>
                        
                        <div class='stock-card'>
                            <h2>{supplyName}</h2>
                            {(!string.IsNullOrEmpty(tankName) ? $"<p><strong>Tank:</strong> {tankName}</p>" : "")}
                            <p class='stock-level'>Current: {currentQuantity} {unit}</p>
                            <p>Minimum Level: {minimumQuantity} {unit}</p>
                        </div>
                        
                        <p>Your supply is running low. Consider restocking soon to avoid running out.</p>
                        
                        <a href='#' class='btn'>View Supply Details</a>
                    </div>
                    <div class='footer'>
                        <p>This is an automated alert from AquaHub.</p>
                        <p>To manage your supply tracking, visit the Supplies section.</p>
                    </div>
                </div>
            </body>
            </html>";

        await _emailSender.SendEmailAsync(userEmail, subject, htmlMessage);
    }

    public async Task SendMaintenanceReminderEmailAsync(string userEmail, string userName, string tankName, string maintenanceType, DateTime lastPerformed, int daysSinceLastMaintenance)
    {
        var subject = $"🔧 AquaHub: Maintenance Reminder for {tankName}";

        var htmlMessage = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #16a085 0%, #1abc9c 100%); color: white; padding: 30px; text-align: center; border-radius: 8px 8px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
                    .maintenance-card {{ background: white; padding: 20px; border-left: 4px solid #16a085; margin: 20px 0; border-radius: 4px; }}
                    .days-count {{ color: #16a085; font-weight: bold; font-size: 1.3em; }}
                    .footer {{ text-align: center; padding: 20px; color: #666; font-size: 0.9em; }}
                    .btn {{ display: inline-block; padding: 12px 30px; background: #16a085; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>🔧 Maintenance Reminder</h1>
                    </div>
                    <div class='content'>
                        <p>Hi {userName},</p>
                        
                        <div class='maintenance-card'>
                            <h2>{tankName}</h2>
                            <p><strong>Maintenance Type:</strong> {maintenanceType}</p>
                            <p><strong>Last Performed:</strong> {lastPerformed:MMMM dd, yyyy}</p>
                            <p>It's been <span class='days-count'>{daysSinceLastMaintenance} days</span> since your last {maintenanceType.ToLower()}.</p>
                        </div>
                        
                        <p>Regular maintenance is key to a healthy aquarium. Don't forget to log your maintenance when complete!</p>
                        
                        <a href='#' class='btn'>Log Maintenance</a>
                    </div>
                    <div class='footer'>
                        <p>This is an automated reminder from AquaHub.</p>
                    </div>
                </div>
            </body>
            </html>";

        await _emailSender.SendEmailAsync(userEmail, subject, htmlMessage);
    }

    public async Task SendWelcomeEmailAsync(string userEmail, string userName)
    {
        var subject = "Welcome to AquaHub! 🐠";

        var htmlMessage = $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 40px; text-align: center; border-radius: 8px 8px 0 0; }}
                    .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 8px 8px; }}
                    .feature {{ background: white; padding: 15px; margin: 15px 0; border-radius: 5px; }}
                    .feature-icon {{ font-size: 2em; }}
                    .footer {{ text-align: center; padding: 20px; color: #666; font-size: 0.9em; }}
                    .btn {{ display: inline-block; padding: 12px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>🐠 Welcome to AquaHub!</h1>
                        <p>Your complete aquarium management solution</p>
                    </div>
                    <div class='content'>
                        <p>Hi {userName},</p>
                        
                        <p>Welcome to AquaHub! We're excited to help you maintain healthy, thriving aquariums.</p>
                        
                        <h3>Get Started:</h3>
                        
                        <div class='feature'>
                            <span class='feature-icon'>🐟</span>
                            <strong>Add Your Tanks</strong> - Set up your aquarium profiles
                        </div>
                        
                        <div class='feature'>
                            <span class='feature-icon'>💧</span>
                            <strong>Track Water Parameters</strong> - Log and monitor water quality
                        </div>
                        
                        <div class='feature'>
                            <span class='feature-icon'>⏰</span>
                            <strong>Set Reminders</strong> - Never miss maintenance tasks
                        </div>
                        
                        <div class='feature'>
                            <span class='feature-icon'>📊</span>
                            <strong>Monitor Health</strong> - Get insights into your aquarium's health
                        </div>
                        
                        <a href='#' class='btn'>Start Managing Your Aquariums</a>
                    </div>
                    <div class='footer'>
                        <p>Happy fishkeeping!</p>
                        <p>The AquaHub Team</p>
                    </div>
                </div>
            </body>
            </html>";

        await _emailSender.SendEmailAsync(userEmail, subject, htmlMessage);
    }
}
