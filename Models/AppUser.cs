using System;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

public class AppUser : IdentityUser
{
    [Required]
    [StringLength(100, ErrorMessage = "First name cannot be longer than 100 characters and must be at least 2 characters long.", MinimumLength = 2)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "Last name cannot be longer than 100 characters and must be at least 2 characters long.", MinimumLength = 2)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [NotMapped]
    [Display(Name = "Full Name")]
    public string FullName => $"{FirstName} {LastName}";

    // Navigation property for tanks
    public ICollection<Tank> Tanks { get; set; } = new List<Tank>();

    // Properties for User Tiering
    public UserTier Tier { get; set; } = UserTier.Hobby;
    public DateTime? ProTierStartDate { get; set; }
    public DateTime? ProTierEndDate { get; set; }
    public SubscriptionStatus SubscriptionStatus { get; set; } = SubscriptionStatus.None;
    
    // Cancellation tracking
    public DateTime? CancellationRequestedDate { get; set; }
    public DateTime? GracePeriodEndDate { get; set; }

    // Trial tracking
    public DateTime? TrialStartDate { get; set; }
    public DateTime? TrialEndDate { get; set; }

    // Payment tracking
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public DateTime? NextBillingDate { get; set; }

    // Navigation property for Payments
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    // Developer mode
    public bool IsDeveloperMode => Tier == UserTier.Developer;

    // Computed properties
    public bool IsProTierActive => Tier == UserTier.Pro && 
                                    SubscriptionStatus == SubscriptionStatus.Active &&
                                    ProTierEndDate.HasValue && 
                                    ProTierEndDate.Value > DateTime.UtcNow;

    public bool IsInGracePeriod => SubscriptionStatus == SubscriptionStatus.Cancelled &&
                                    GracePeriodEndDate.HasValue &&
                                    GracePeriodEndDate.Value > DateTime.UtcNow;

    public bool IsOnTrial => SubscriptionStatus == SubscriptionStatus.Trialing &&
                                TrialEndDate.HasValue &&
                                TrialEndDate.Value > DateTime.UtcNow;
                            

    public bool CanAccessProFeatures => IsDeveloperMode || IsProTierActive || IsInGracePeriod;
    
}
