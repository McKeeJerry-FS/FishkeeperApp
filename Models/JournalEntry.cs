using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaHub.MVC.Models;

public class JournalEntry
{
public int Id { get; set; }

    [Required]
    public int TankId { get; set; }

    [ForeignKey(nameof(TankId))]
    public Tank? Tank { get; set; }

    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Title must be between 2 and 200 characters")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Entry content is required")]
    [StringLength(5000, ErrorMessage = "Entry content cannot exceed 5000 characters")]
    public string Content { get; set; } = string.Empty;

    [Required]
    public DateTime Timestamp { get; set; }

    // Optional image path for the journal entry
    public string? ImagePath { get; set; }

    // Navigation properties for linked records
    public ICollection<JournalMaintenanceLink> MaintenanceLinks { get; set; } = new List<JournalMaintenanceLink>();
    public ICollection<JournalWaterTestLink> WaterTestLinks { get; set; } = new List<JournalWaterTestLink>();
}

// Junction table for linking journal entries to maintenance logs
public class JournalMaintenanceLink
{
    public int Id { get; set; }

    [Required]
    public int JournalEntryId { get; set; }

    [ForeignKey(nameof(JournalEntryId))]
    public JournalEntry? JournalEntry { get; set; }

    [Required]
    public int MaintenanceLogId { get; set; }

    [ForeignKey(nameof(MaintenanceLogId))]
    public MaintenanceLog? MaintenanceLog { get; set; }
}

// Junction table for linking journal entries to water tests
public class JournalWaterTestLink
{
    public int Id { get; set; }

    [Required]
    public int JournalEntryId { get; set; }

    [ForeignKey(nameof(JournalEntryId))]
    public JournalEntry? JournalEntry { get; set; }

    [Required]
    public int WaterTestId { get; set; }

    [ForeignKey(nameof(WaterTestId))]
    public WaterTest? WaterTest { get; set; }
}

