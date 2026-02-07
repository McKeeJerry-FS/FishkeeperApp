using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

/// <summary>
/// Represents a feeding event that was completed
/// </summary>
public class FeedingRecord
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tank is required")]
    [Display(Name = "Tank")]
    public int TankId { get; set; }

    [ForeignKey(nameof(TankId))]
    public Tank? Tank { get; set; }

    [Display(Name = "Feeding Schedule")]
    public int? FeedingScheduleId { get; set; }

    [ForeignKey(nameof(FeedingScheduleId))]
    public FeedingSchedule? FeedingSchedule { get; set; }

    [Required(ErrorMessage = "Feed type is required")]
    [Display(Name = "Feed Type")]
    public FeedType FeedType { get; set; }

    [Required(ErrorMessage = "Feed name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Feed name must be between 2 and 100 characters")]
    [Display(Name = "Feed Name")]
    public string FeedName { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Brand name cannot exceed 50 characters")]
    [Display(Name = "Brand")]
    public string? Brand { get; set; }

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, 1000, ErrorMessage = "Amount must be between 0.01 and 1000")]
    [Display(Name = "Amount")]
    public double Amount { get; set; }

    [Required(ErrorMessage = "Unit is required")]
    [StringLength(20, ErrorMessage = "Unit cannot exceed 20 characters")]
    [Display(Name = "Unit")]
    public string Unit { get; set; } = "grams";

    [Required(ErrorMessage = "Fed date and time is required")]
    [Display(Name = "Fed Date & Time")]
    public DateTime FedDateTime { get; set; } = DateTime.UtcNow;

    [Display(Name = "Was Scheduled")]
    public bool WasScheduled { get; set; } = false;

    [Display(Name = "Scheduled Time")]
    public TimeSpan? ScheduledTime { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    // User who recorded the feeding - set by system, not user input
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public AppUser? User { get; set; }

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Helper properties
    [NotMapped]
    [Display(Name = "Was Late")]
    public bool WasLate
    {
        get
        {
            if (!WasScheduled || ScheduledTime == null)
                return false;

            var actualTime = FedDateTime.TimeOfDay;
            var scheduledTimeSpan = ScheduledTime.Value;
            var difference = actualTime - scheduledTimeSpan;

            // Consider late if more than 30 minutes after scheduled time
            return difference.TotalMinutes > 30;
        }
    }

    [NotMapped]
    [Display(Name = "Time Difference")]
    public string TimeDifference
    {
        get
        {
            if (!WasScheduled || ScheduledTime == null)
                return "N/A";

            var actualTime = FedDateTime.TimeOfDay;
            var scheduledTimeSpan = ScheduledTime.Value;
            var difference = actualTime - scheduledTimeSpan;

            if (Math.Abs(difference.TotalMinutes) < 5)
                return "On time";

            var sign = difference.TotalMinutes > 0 ? "+" : "";
            return $"{sign}{difference.TotalMinutes:F0} min";
        }
    }
}
