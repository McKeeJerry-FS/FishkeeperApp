using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaHub.MVC.Models;

public class CoralFrag
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tank is required")]
    [Display(Name = "Tank")]
    public int TankId { get; set; }
    [ForeignKey(nameof(TankId))]
    public Tank? Tank { get; set; }

    [Display(Name = "Parent Coral")]
    public int? ParentCoralId { get; set; }
    [ForeignKey(nameof(ParentCoralId))]
    public Coral? ParentCoral { get; set; }

    [Required(ErrorMessage = "Frag date is required")]
    [Display(Name = "Frag Date")]
    [DataType(DataType.Date)]
    public DateTime FragDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Frag name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Frag name must be between 2 and 100 characters")]
    [Display(Name = "Frag Name/ID")]
    public string FragName { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Coral Species")]
    public string CoralSpecies { get; set; } = string.Empty;

    [Range(0.1, 100, ErrorMessage = "Size must be between 0.1 and 100 inches")]
    [Display(Name = "Initial Size (inches)")]
    public double InitialSize { get; set; }

    [Range(1, 50, ErrorMessage = "Number of polyps must be between 1 and 50")]
    [Display(Name = "Number of Polyps")]
    public int? NumberOfPolyps { get; set; }

    [StringLength(50)]
    [Display(Name = "Coloration")]
    public string Coloration { get; set; } = string.Empty;

    [StringLength(50)]
    [Display(Name = "Fragging Method")]
    public string FraggingMethod { get; set; } = string.Empty; // Saw, Bone cutters, Break, Laser, etc.

    [StringLength(500)]
    [Display(Name = "Fragging Notes")]
    public string FraggingNotes { get; set; } = string.Empty;

    [Display(Name = "Encrusted/Healed")]
    public bool IsEncrusted { get; set; } = false;

    [Display(Name = "Encrusted Date")]
    [DataType(DataType.Date)]
    public DateTime? EncrustmentDate { get; set; }

    [Display(Name = "Ready for Sale/Trade")]
    public bool IsReadyForSale { get; set; } = false;

    [Display(Name = "Sale Date")]
    [DataType(DataType.Date)]
    public DateTime? SaleDate { get; set; }

    [Range(0, 10000, ErrorMessage = "Price must be between 0 and 10,000")]
    [Display(Name = "Sale Price")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal? SalePrice { get; set; }

    [StringLength(100)]
    [Display(Name = "Sold/Traded To")]
    public string SoldTo { get; set; } = string.Empty;

    [Display(Name = "Current Status")]
    [StringLength(50)]
    public string Status { get; set; } = "Growing"; // Growing, Encrusted, Ready, Sold, Lost, Traded

    [Range(0, 100, ErrorMessage = "Current size must be between 0 and 100 inches")]
    [Display(Name = "Current Size (inches)")]
    public double? CurrentSize { get; set; }

    [Display(Name = "Last Measurement Date")]
    [DataType(DataType.Date)]
    public DateTime? LastMeasurementDate { get; set; }

    [Display(Name = "Location in Tank")]
    [StringLength(100)]
    public string LocationInTank { get; set; } = string.Empty;

    [Display(Name = "Mounted On")]
    [StringLength(50)]
    public string MountedOn { get; set; } = string.Empty; // Plug, Rock, Disc, etc.

    [StringLength(1000)]
    public string Notes { get; set; } = string.Empty;

    // Image path for the frag
    public string? ImagePath { get; set; }

    // User ownership
    [Required]
    public string UserId { get; set; } = string.Empty;
    [ForeignKey(nameof(UserId))]
    public AppUser? User { get; set; }

    [Display(Name = "Created Date")]
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    [Display(Name = "Last Updated")]
    public DateTime? LastUpdated { get; set; }
}
