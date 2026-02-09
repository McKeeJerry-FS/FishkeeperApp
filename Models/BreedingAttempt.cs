using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaHub.MVC.Models;

public class BreedingAttempt
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Breeding Pair")]
    public int BreedingPairId { get; set; }
    [ForeignKey(nameof(BreedingPairId))]
    public BreedingPair? BreedingPair { get; set; }

    [Required(ErrorMessage = "Attempt date is required")]
    [Display(Name = "Attempt Date")]
    [DataType(DataType.Date)]
    public DateTime AttemptDate { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Status is required")]
    [StringLength(50)]
    public string Status { get; set; } = "In Progress"; // In Progress, Successful, Failed, Eggs Laid, Fry Born, etc.

    [Display(Name = "Spawning Observed")]
    public bool SpawningObserved { get; set; } = false;

    [Display(Name = "Spawning Date/Time")]
    public DateTime? SpawningDateTime { get; set; }

    [Display(Name = "Eggs Laid")]
    public int? EggsLaid { get; set; }

    [Display(Name = "Eggs Fertilized")]
    public int? EggsFertilized { get; set; }

    [Display(Name = "Hatching Date")]
    [DataType(DataType.Date)]
    public DateTime? HatchingDate { get; set; }

    [Display(Name = "Number Hatched")]
    public int? NumberHatched { get; set; }

    [Display(Name = "Fry/Offspring Count")]
    public int? OffspringCount { get; set; }

    [Display(Name = "Survival Rate %")]
    [Range(0, 100)]
    public double? SurvivalRatePercent { get; set; }

    [Display(Name = "Current Survivors")]
    public int? CurrentSurvivors { get; set; }

    [Display(Name = "Water Temperature (°F)")]
    [Range(32, 110)]
    public double? WaterTemperature { get; set; }

    [Display(Name = "pH Level")]
    [Range(0, 14)]
    public double? PhLevel { get; set; }

    [Display(Name = "Hardness (GH)")]
    public int? Hardness { get; set; }

    [Display(Name = "Breeding Trigger")]
    [StringLength(200)]
    public string BreedingTrigger { get; set; } = string.Empty; // Water change, food, temperature, moon phase, etc.

    [Display(Name = "Behavior Observations")]
    [StringLength(1000)]
    public string BehaviorObservations { get; set; } = string.Empty;

    [Display(Name = "Parental Care Notes")]
    [StringLength(500)]
    public string ParentalCareNotes { get; set; } = string.Empty;

    [Display(Name = "Feeding Regimen")]
    [StringLength(500)]
    public string FeedingRegimen { get; set; } = string.Empty;

    [Display(Name = "Challenges/Issues")]
    [StringLength(1000)]
    public string Challenges { get; set; } = string.Empty;

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
}
