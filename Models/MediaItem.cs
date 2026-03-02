using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

/// <summary>
/// Represents a photo or video file in the media archive
/// Can be associated with a Tank, JournalEntry, or stored at account level
/// </summary>
public class MediaItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Required]
    [StringLength(500)]
    [Display(Name = "File Path")]
    public string FilePath { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Media Type")]
    public MediaType MediaType { get; set; }

    [Required]
    [Display(Name = "Category")]
    public MediaCategory Category { get; set; } = MediaCategory.General;

    [Display(Name = "File Size (bytes)")]
    public long FileSize { get; set; }

    [StringLength(100)]
    [Display(Name = "Content Type")]
    public string? ContentType { get; set; }

    [Display(Name = "Width")]
    public int? Width { get; set; }

    [Display(Name = "Height")]
    public int? Height { get; set; }

    [Display(Name = "Duration (seconds)")]
    public int? DurationSeconds { get; set; }

    [Required]
    [Display(Name = "Upload Date")]
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Taken/Recorded Date")]
    public DateTime? TakenAt { get; set; }

    // User association - required for account-level media
    [Required]
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public AppUser? User { get; set; }

    // Optional Tank association - for tank-specific media
    public int? TankId { get; set; }

    [ForeignKey(nameof(TankId))]
    public Tank? Tank { get; set; }

    // Optional JournalEntry association - for journal-specific media
    public int? JournalEntryId { get; set; }

    [ForeignKey(nameof(JournalEntryId))]
    public JournalEntry? JournalEntry { get; set; }

    [StringLength(500)]
    [Display(Name = "Tags")]
    public string? Tags { get; set; } // Comma-separated tags for searchability

    [Display(Name = "Is Favorite")]
    public bool IsFavorite { get; set; } = false;

    [Display(Name = "Is Public")]
    public bool IsPublic { get; set; } = false;

    [Display(Name = "View Count")]
    public int ViewCount { get; set; } = 0;

    // Thumbnail path for videos and large images
    [StringLength(500)]
    public string? ThumbnailPath { get; set; }

    // Sorting order within a collection
    [Display(Name = "Sort Order")]
    public int SortOrder { get; set; } = 0;
}