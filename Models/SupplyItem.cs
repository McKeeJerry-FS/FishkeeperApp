using System;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

public class SupplyItem
{
    public int Id { get; set; }

    // User relationship
    public string UserId { get; set; } = string.Empty;
    public AppUser? User { get; set; }

    // Optional tank association (null for general supplies)
    public int? TankId { get; set; }
    public Tank? Tank { get; set; }

    // Supply details
    public string Name { get; set; } = string.Empty;
    public SupplyCategory Category { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }

    // Inventory tracking
    public double CurrentQuantity { get; set; }
    public double MinimumQuantity { get; set; } // Threshold for low stock warning
    public double? OptimalQuantity { get; set; } // Ideal quantity to have on hand
    public string Unit { get; set; } = "units"; // units, ml, grams, oz, etc.

    // Stock status (computed from CurrentQuantity vs MinimumQuantity)
    public StockStatus Status
    {
        get
        {
            if (CurrentQuantity <= 0)
                return StockStatus.OutOfStock;
            if (CurrentQuantity <= MinimumQuantity)
                return StockStatus.LowStock;
            return StockStatus.InStock;
        }
    }

    // Purchase information
    public string? PreferredVendor { get; set; }
    public decimal? LastPurchasePrice { get; set; }
    public DateTime? LastPurchaseDate { get; set; }
    public string? ProductUrl { get; set; } // Link to product for easy reordering

    // Usage tracking
    public double? AverageUsagePerWeek { get; set; } // For predictive warnings
    public DateTime? LastUsedDate { get; set; }
    public DateTime? ExpirationDate { get; set; }

    // Location
    public string? StorageLocation { get; set; }

    // Notes and alerts
    public string Notes { get; set; } = string.Empty;
    public bool EnableLowStockAlert { get; set; } = true;
    public bool IsActive { get; set; } = true; // False if discontinued/no longer used

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Computed property for estimated days until out of stock
    public int? EstimatedDaysRemaining
    {
        get
        {
            if (AverageUsagePerWeek.HasValue && AverageUsagePerWeek.Value > 0)
            {
                var weeksRemaining = CurrentQuantity / AverageUsagePerWeek.Value;
                return (int)(weeksRemaining * 7);
            }
            return null;
        }
    }
}
