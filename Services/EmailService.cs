using System;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using AquaHub.MVC.Models;



namespace AquaHub.MVC.Services;

public class EmailService : IEmailSender
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }


    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        try
        {
            var emailAddress = _emailSettings.EmailAddress ?? Environment.GetEnvironmentVariable("EmailAddress");
            var emailPassword = _emailSettings.EmailPassword ?? Environment.GetEnvironmentVariable("EmailPassword");
            var emailHost = _emailSettings.EmailHost ?? Environment.GetEnvironmentVariable("EmailHost");
            var emailPortStr = Environment.GetEnvironmentVariable("EmailPort");
            var emailPort = _emailSettings.EmailPort != 0 ? _emailSettings.EmailPort : (!string.IsNullOrEmpty(emailPortStr) ? int.Parse(emailPortStr) : 587);

            // If email settings are not configured, just log and return (don't fail)
            if (string.IsNullOrEmpty(emailAddress) || string.IsNullOrEmpty(emailPassword) || string.IsNullOrEmpty(emailHost))
            {
                Console.WriteLine($"Email not configured. Skipping email to: {email}");
                Console.WriteLine($"Subject: {subject}");
                return;
            }

            MimeMessage newEmail = new();
            newEmail.Sender = MailboxAddress.Parse(emailAddress);
            foreach (string address in email.Split(";"))
            {
                newEmail.To.Add(MailboxAddress.Parse(address));
            }

            // Set the subject
            newEmail.Subject = subject;

            // set the message
            BodyBuilder emailBody = new BodyBuilder();
            emailBody.HtmlBody = htmlMessage;
            newEmail.Body = emailBody.ToMessageBody();

            // send the email

            using SmtpClient smtpClient = new SmtpClient();
            try
            {
                await smtpClient.ConnectAsync(emailHost, emailPort, SecureSocketOptions.StartTls);
                await smtpClient.AuthenticateAsync(emailAddress, emailPassword);
                await smtpClient.SendAsync(newEmail);

                await smtpClient.DisconnectAsync(true);

                // For testing - comment out later
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("****************** SUCCESS *****************");
                Console.WriteLine($"Email Successfully sent!!!!!!");
                Console.WriteLine("****************** SUCCESS *****************");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("****************** ERROR *****************");
                Console.WriteLine($"Failure sending email with Google Provider Error: {ex.Message}");
                Console.WriteLine("****************** ERROR *****************");
                Console.ResetColor();

                // Don't throw - just log the error so registration can continue
            }



        }
        catch (Exception ex)
        {
            // Log but don't throw - allow registration to proceed even if email fails
            Console.WriteLine($"Email service error: {ex.Message}");
        }
    }

}
