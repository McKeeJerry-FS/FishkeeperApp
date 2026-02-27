using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

public class NewBaseType
{
    public ICollection<WaterTest> WaterTests { get; set; } = new List<WaterTest>();
}

public class Tank : NewBaseType
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Tank name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tank name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Volume is required")]
    [Range(1, 10000, ErrorMessage = "Volume must be between 1 and 10,000 gallons")]
    [Display(Name = "Volume (Gallons)")]
    public double VolumeGallons { get; set; }

    [Required(ErrorMessage = "Tank type is required")]
    [Display(Name = "Tank Type")]
    public AquariumType Type { get; set; }

    [Required(ErrorMessage = "Start date is required")]
    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string Notes { get; set; } = string.Empty;

    // Indicates if this is a new (cycling) tank or established
    [Display(Name = "Is this a new tank?")]
    public bool IsNewTank { get; set; } = false;

    // Quarantine Tank Properties
    [Display(Name = "Is Quarantine Tank")]
    public bool IsQuarantineTank { get; set; } = false;

    [Display(Name = "Quarantine Start Date")]
    [DataType(DataType.Date)]
    public DateTime? QuarantineStartDate { get; set; }

    [Display(Name = "Expected Quarantine End Date")]
    [DataType(DataType.Date)]
    public DateTime? QuarantineEndDate { get; set; }

    [Display(Name = "Quarantine Purpose")]
    [StringLength(500)]
    public string? QuarantinePurpose { get; set; } // Treatment, Observation, Acclimation

    [Display(Name = "Quarantine Status")]
    [StringLength(50)]
    public string? QuarantineStatus { get; set; } // Active, Completed, Monitoring

    [Display(Name = "Treatment Protocol")]
    [StringLength(1000)]
    public string? TreatmentProtocol { get; set; }

    // Image path stored relative to wwwroot
    public string? ImagePath { get; set; }

    // User ownership - Required at database level but set by service, not by user input
    public string UserId { get; set; } = string.Empty;

    [ForeignKey(nameof(UserId))]
    public AppUser? User { get; set; }
    public ICollection<Livestock> Livestock { get; set; } = new List<Livestock>();
    public ICollection<MaintenanceLog> MaintenanceLogs { get; set; } = new List<MaintenanceLog>();
    public ICollection<ShoppingListItem> ShoppingListItems { get; set; } = new List<ShoppingListItem>();

    // Equipment navigation properties
    public ICollection<Filter> Filters { get; set; } = new List<Filter>();
    public ICollection<Light> Lights { get; set; } = new List<Light>();
    public ICollection<Heater> Heaters { get; set; } = new List<Heater>();
    public ICollection<ProteinSkimmer> ProteinSkimmers { get; set; } = new List<ProteinSkimmer>();

    // Image Properties
    [NotMapped]
    public IFormFile? ImageFile { get; set; }
    public byte[]? ImageData { get; set; }
    public string? ImageType { get; set; }

    // Computed property to get all equipment combined
    [NotMapped]
    public ICollection<Equipment> Equipment =>
        Filters.Cast<Equipment>()
            .Concat(Lights.Cast<Equipment>())
            .Concat(Heaters.Cast<Equipment>())
            .Concat(ProteinSkimmers.Cast<Equipment>())
            .ToList();
}
