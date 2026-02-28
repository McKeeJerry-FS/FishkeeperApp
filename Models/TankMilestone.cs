using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AquaHub.MVC.Models;

public enum TankMilestoneType
{
    AddPlants,
    AddCorals,
    AddFirstFish,
    AddInverts,
    AddCleanupCrew,
    Custom
}

public class TankMilestone
{
    public int Id { get; set; }
    public int TankId { get; set; }
    public TankMilestoneType Type { get; set; }
    [StringLength(100)]
    public string? Description { get; set; }
    public DateTime? TargetDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsCompleted { get; set; } = false;
    public string? Notes { get; set; }
    public Tank? Tank { get; set; }

    // Manual override fields
    public bool IsManuallyCompleted { get; set; } = false;
    public DateTime? ManualCompletedDate { get; set; }
}
