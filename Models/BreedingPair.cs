using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaHub.MVC.Models;

public class BreedingPair
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Tank")]
    public int TankId { get; set; }
    [ForeignKey(nameof(TankId))]
    public Tank? Tank { get; set; }

    [Required(ErrorMessage = "Pair name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    [Display(Name = "Breeding Pair Name")]
    public string PairName { get; set; } = string.Empty;

    [Display(Name = "Parent 1")]
    public int? Parent1Id { get; set; }
    [ForeignKey(nameof(Parent1Id))]
    public Livestock? Parent1 { get; set; }

    [Display(Name = "Parent 2")]
    public int? Parent2Id { get; set; }
    [ForeignKey(nameof(Parent2Id))]
    public Livestock? Parent2 { get; set; }

    [StringLength(100)]
    [Display(Name = "Species")]
    public string Species { get; set; } = string.Empty;

    [Display(Name = "Breeding Type")]
    [StringLength(50)]
    public string BreedingType { get; set; } = string.Empty; // Egg Layer, Live Bearer, Spawner, etc.

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "Date Paired")]
    [DataType(DataType.Date)]
    public DateTime DatePaired { get; set; } = DateTime.Now;

    [Display(Name = "Breeding Setup")]
    [StringLength(500)]
    public string BreedingSetup { get; set; } = string.Empty; // Breeding tank setup details

    [Display(Name = "Water Conditions Notes")]
    [StringLength(500)]
    public string WaterConditions { get; set; } = string.Empty; // General water parameter notes

    // Ideal Water Parameters for Breeding
    [Display(Name = "Ideal Temperature Min (°F)")]
    [Range(32, 110)]
    public double? IdealTempMin { get; set; }

    [Display(Name = "Ideal Temperature Max (°F)")]
    [Range(32, 110)]
    public double? IdealTempMax { get; set; }

    [Display(Name = "Ideal pH Min")]
    [Range(0, 14)]
    public double? IdealPhMin { get; set; }

    [Display(Name = "Ideal pH Max")]
    [Range(0, 14)]
    public double? IdealPhMax { get; set; }

    [Display(Name = "Ideal GH Min")]
    public int? IdealGhMin { get; set; }

    [Display(Name = "Ideal GH Max")]
    public int? IdealGhMax { get; set; }

    [Display(Name = "Ideal KH Min")]
    public int? IdealKhMin { get; set; }

    [Display(Name = "Ideal KH Max")]
    public int? IdealKhMax { get; set; }

    [Display(Name = "Max Ammonia (ppm)")]
    [Range(0, 10)]
    public double? MaxAmmonia { get; set; }

    [Display(Name = "Max Nitrite (ppm)")]
    [Range(0, 10)]
    public double? MaxNitrite { get; set; }

    [Display(Name = "Max Nitrate (ppm)")]
    [Range(0, 200)]
    public double? MaxNitrate { get; set; }

    [Display(Name = "Diet & Conditioning")]
    [StringLength(500)]
    public string DietConditioning { get; set; } = string.Empty;

    [Display(Name = "Breeding Behavior Notes")]
    [StringLength(1000)]
    public string BehaviorNotes { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Notes { get; set; } = string.Empty;

    // Image path
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

    // Navigation properties
    public ICollection<BreedingAttempt> BreedingAttempts { get; set; } = new List<BreedingAttempt>();
}
