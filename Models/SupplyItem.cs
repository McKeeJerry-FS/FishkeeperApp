
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using AquaHub.MVC.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace AquaHub.MVC.Models;

public class SupplyItem
{
    // Image Properties
    [NotMapped]
    public IFormFile? ImageFile { get; set; }
    public byte[]? ImageData { get; set; }
    public string? ImageType { get; set; }
    public int Id { get; set; }

    // User relationship (set by controller, not from form)
    [BindNever]
    public string UserId { get; set; } = string.Empty;
    public AppUser? User { get; set; }

    // Optional tank association (null for general supplies)
    public int? TankId { get; set; }
    public Tank? Tank { get; set; }

    // Supply details
    [Required(ErrorMessage = "Supply name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    [Display(Name = "Supply Name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    [Display(Name = "Category")]
    public SupplyCategory Category { get; set; }

    [StringLength(100, ErrorMessage = "Brand name cannot exceed 100 characters")]
    [Display(Name = "Brand")]
    public string? Brand { get; set; }

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    // Inventory tracking
    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Current quantity must be 0 or greater")]
    [Display(Name = "Current Quantity")]
    public double CurrentQuantity { get; set; }

    [Required]
    [Range(0, double.MaxValue, ErrorMessage = "Minimum quantity must be 0 or greater")]
    [Display(Name = "Minimum Quantity")]
    public double MinimumQuantity { get; set; } // Threshold for low stock warning

    [Range(0, double.MaxValue, ErrorMessage = "Optimal quantity must be 0 or greater")]
    [Display(Name = "Optimal Quantity")]
    public double? OptimalQuantity { get; set; } // Ideal quantity to have on hand

    [Required(ErrorMessage = "Unit is required")]
    [StringLength(20, ErrorMessage = "Unit cannot exceed 20 characters")]
    [Display(Name = "Unit of Measurement")]
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
    [StringLength(100, ErrorMessage = "Vendor name cannot exceed 100 characters")]
    [Display(Name = "Preferred Vendor")]
    public string? PreferredVendor { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Price must be 0 or greater")]
    [Display(Name = "Last Purchase Price")]
    [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
    public decimal? LastPurchasePrice { get; set; }

    [Display(Name = "Last Purchase Date")]
    [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime? LastPurchaseDate { get; set; }

    [Url(ErrorMessage = "Please enter a valid URL")]
    [StringLength(500, ErrorMessage = "Product URL cannot exceed 500 characters")]
    [Display(Name = "Product URL")]
    public string? ProductUrl { get; set; } // Link to product for easy reordering

    // Usage tracking
    [Range(0, double.MaxValue, ErrorMessage = "Average usage must be 0 or greater")]
    [Display(Name = "Average Usage Per Week")]
    public double? AverageUsagePerWeek { get; set; } // For predictive warnings

    [Display(Name = "Last Used Date")]
    [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime? LastUsedDate { get; set; }

    [Display(Name = "Expiration Date")]
    [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime? ExpirationDate { get; set; }

    // Location
    [StringLength(100, ErrorMessage = "Storage location cannot exceed 100 characters")]
    [Display(Name = "Storage Location")]
    public string? StorageLocation { get; set; }

    // Notes and alerts
    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
    [Display(Name = "Notes")]
    public string Notes { get; set; } = string.Empty;

    [Display(Name = "Enable Low Stock Alert")]
    public bool EnableLowStockAlert { get; set; } = true;

    [Display(Name = "Active")]
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
