# Email Notification Integration Guide

## Overview

AquaHub now supports email notifications for alerts, reminders, and important aquarium events using MailKit SMTP integration.

## Features

### Notification Types

1. **Reminder Emails** - Maintenance tasks and scheduled reminders
2. **Water Parameter Alerts** - Critical water quality warnings
3. **Low Stock Alerts** - Supply inventory warnings
4. **Maintenance Reminders** - Scheduled maintenance notifications
5. **Welcome Emails** - New user onboarding
6. **Notification Digests** - Batched notifications summary

## Configuration

### 1. Email Settings

Add your SMTP credentials to **User Secrets** (recommended for development) or **Environment Variables** (for production):

#### Using User Secrets (Development)

```bash
cd /Users/jerrymckeejr/Desktop/FishkeeperApp
dotnet user-secrets set "EmailSettings:EmailAddress" "your-email@gmail.com"
dotnet user-secrets set "EmailSettings:EmailPassword" "your-app-password"
dotnet user-secrets set "EmailSettings:EmailHost" "smtp.gmail.com"
dotnet user-secrets set "EmailSettings:EmailPort" "587"
```

#### Using Environment Variables (Production/Railway)

Set these environment variables in your Railway dashboard:

```
EmailAddress=your-email@gmail.com
EmailPassword=your-app-password
EmailHost=smtp.gmail.com
EmailPort=587
```

### 2. Gmail App Password Setup

If using Gmail:

1. Enable 2-Factor Authentication on your Google account
2. Go to Google Account → Security → 2-Step Verification → App passwords
3. Generate an app password for "Mail"
4. Use this 16-character password (not your regular Gmail password)

### 3. Alternative SMTP Providers

You can use any SMTP provider by changing the host and port:

- **Gmail**: smtp.gmail.com:587
- **Outlook**: smtp-mail.outlook.com:587
- **SendGrid**: smtp.sendgrid.net:587
- **Mailgun**: smtp.mailgun.org:587

## Services Architecture

### EmailService

Core service implementing `IEmailSender` for sending emails via SMTP.

**Location**: `Services/EmailService.cs`

### EmailNotificationService

High-level service for creating formatted notification emails.

**Location**: `Services/EmailNotificationService.cs`

**Methods**:

- `SendReminderEmailAsync()` - Maintenance and task reminders
- `SendWaterParameterAlertEmailAsync()` - Water quality alerts
- `SendLowStockAlertEmailAsync()` - Supply inventory warnings
- `SendMaintenanceReminderEmailAsync()` - Predictive maintenance alerts
- `SendWelcomeEmailAsync()` - User onboarding
- `SendNotificationDigestEmailAsync()` - Batched notifications

### NotificationService

Manages in-app notifications and triggers email notifications.

**Location**: `Services/NotificationService.cs`

**Integration**: Automatically sends emails when notifications are created if user has email notifications enabled.

### SupplyService

Enhanced with email alerts for low stock warnings.

**Location**: `Services/SupplyService.cs`

## Usage Examples

### 1. Send a Reminder Email

```csharp
await _emailNotificationService.SendReminderEmailAsync(
    userEmail: "user@example.com",
    userName: "John Doe",
    reminderTitle: "Weekly Water Change",
    reminderDescription: "Perform 25% water change and test parameters",
    dueDate: DateTime.Now.AddDays(1)
);
```

### 2. Send Water Parameter Alert

```csharp
await _emailNotificationService.SendWaterParameterAlertEmailAsync(
    userEmail: "user@example.com",
    userName: "John Doe",
    tankName: "Main Reef Tank",
    parameterName: "Nitrate",
    value: 45.0,
    minRange: 0,
    maxRange: 20
);
```

### 3. Send Low Stock Alert

```csharp
await _emailNotificationService.SendLowStockAlertEmailAsync(
    userEmail: "user@example.com",
    userName: "John Doe",
    supplyName: "Marine Salt Mix",
    currentQuantity: 50,
    minimumQuantity: 100,
    unit: "grams",
    tankName: "Main Reef Tank"
);
```

## User Notification Settings

Users can control their email preferences via `UserNotificationSettings`:

```csharp
public class UserNotificationSettings
{
    public bool EmailNotificationsEnabled { get; set; } = true;
    public bool ReminderNotificationsEnabled { get; set; } = true;
    public bool WaterParameterAlertsEnabled { get; set; } = true;
    public bool EquipmentAlertsEnabled { get; set; } = true;
    public int EmailDigestFrequencyHours { get; set; } = 0; // 0 = instant
}
```

### Digest Modes

- **Instant (0 hours)**: Send emails immediately when notifications are created
- **Daily (24 hours)**: Batch notifications and send once per day
- **Weekly (168 hours)**: Batch notifications and send weekly summary

## Email Templates

All emails use responsive HTML templates with:

- Professional branding and styling
- Color-coded by notification type
- Mobile-friendly responsive design
- Clear call-to-action buttons
- Footer with preferences link

### Template Colors

- **Reminders**: Purple gradient (#667eea → #764ba2)
- **Water Alerts**: Red gradient (#e74c3c → #c0392b)
- **Low Stock**: Orange gradient (#f39c12 → #e67e22)
- **Maintenance**: Teal gradient (#16a085 → #1abc9c)
- **Digest**: Blue gradient (#3498db → #2980b9)
- **Welcome**: Purple gradient (#667eea → #764ba2)

## Testing

### Local Testing

1. Configure User Secrets with valid SMTP credentials
2. Run the application: `dotnet run`
3. Create a test notification or trigger an alert
4. Check console output for success/error messages
5. Verify email received in inbox

### Test Notifications

```csharp
// In a controller or service
await _notificationService.CreateNotificationAsync(new Notification
{
    UserId = userId,
    Type = NotificationType.Reminder,
    Title = "Test Reminder",
    Message = "This is a test notification",
    CreatedAt = DateTime.UtcNow
});
```

## Troubleshooting

### Common Issues

**1. Authentication Failed**

- Verify you're using an App Password (not regular password) for Gmail
- Check 2FA is enabled on your Google account
- Ensure credentials are correctly set in User Secrets or Environment Variables

**2. Connection Timeout**

- Check firewall settings allow SMTP port 587
- Verify SMTP host is correct
- Try alternative port (465 for SSL)

**3. Emails Not Sending**

- Check console logs for error messages
- Verify user has EmailNotificationsEnabled = true
- Ensure user has a valid email address in their profile
- Check spam/junk folder

**4. Missing Environment Variables**

- The service falls back to Environment Variables if appsettings values are empty
- Verify variables are set: `echo $EmailAddress` (Linux/Mac) or `echo %EmailAddress%` (Windows)

### Debug Mode

The EmailService includes console output for debugging:

```
****************** SUCCESS *****************
Email Successfully sent!!!!!!
****************** SUCCESS *****************
```

Or on error:

```
****************** ERROR *****************
Failure sending email: [Error Message]
****************** ERROR *****************
```

## Security Best Practices

1. **Never commit credentials** - Use User Secrets or Environment Variables
2. **Use App Passwords** - Don't use your main email account password
3. **Enable 2FA** - Add extra security to your email account
4. **Rotate passwords** - Change app passwords periodically
5. **Monitor usage** - Watch for suspicious email activity

## Railway Deployment

When deploying to Railway:

1. Add environment variables in Railway dashboard:
   - `EmailAddress`
   - `EmailPassword`
   - `EmailHost`
   - `EmailPort`

2. The app automatically uses environment variables when appsettings values are empty

3. Test email functionality after deployment

## Future Enhancements

Potential additions:

- [ ] Email templates customization UI
- [ ] Rich text editor for custom email bodies
- [ ] Email scheduling and queuing
- [ ] Email analytics and tracking
- [ ] SMS notifications integration
- [ ] Push notifications for mobile app
- [ ] Webhook integrations
- [ ] Multi-language email templates

## Related Files

- `Models/EmailSettings.cs` - Email configuration model
- `Services/EmailService.cs` - Core SMTP email sender
- `Services/EmailNotificationService.cs` - Formatted notification emails
- `Services/Interfaces/IEmailNotifiactionService.cs` - Email notification interface
- `Services/NotificationService.cs` - Notification management with email integration
- `Services/SupplyService.cs` - Supply tracking with low stock emails
- `Program.cs` - Service registration and configuration
- `appsettings.json` - Default email settings (empty)
- `appsettings.Development.json` - Development email settings

## Support

For issues or questions:

1. Check the console logs for error details
2. Verify all configuration steps are completed
3. Test with a simple email first
4. Review the EmailService code for debugging output
