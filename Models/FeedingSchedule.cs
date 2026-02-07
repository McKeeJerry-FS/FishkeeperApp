using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

/// <summary>
/// Represents a feeding schedule for a specific tank
/// </summary>
public class FeedingSchedule
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tank is required")]
    [Display(Name = "Tank")]
    public int TankId { get; set; }

    [ForeignKey(nameof(TankId))]
    public Tank? Tank { get; set; }

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

    [Required(ErrorMessage = "Feeding times are required")]
    [Display(Name = "Feeding Times")]
    public string FeedingTimes { get; set; } = string.Empty; // Stored as JSON array of TimeSpan strings

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [Display(Name = "Last Modified")]
    public DateTime? LastModified { get; set; }

    // Navigation property
    public ICollection<FeedingRecord> FeedingRecords { get; set; } = new List<FeedingRecord>();

    // Helper property to parse feeding times
    [NotMapped]
    public List<TimeSpan> ParsedFeedingTimes
    {
        get
        {
            if (string.IsNullOrEmpty(FeedingTimes))
                return new List<TimeSpan>();

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<string>>(FeedingTimes)?
                    .Select(t => TimeSpan.Parse(t))
                    .OrderBy(t => t)
                    .ToList() ?? new List<TimeSpan>();
            }
            catch
            {
                return new List<TimeSpan>();
            }
        }
        set
        {
            var timeStrings = value.Select(t => t.ToString(@"hh\:mm")).ToList();
            FeedingTimes = System.Text.Json.JsonSerializer.Serialize(timeStrings);
        }
    }
}
