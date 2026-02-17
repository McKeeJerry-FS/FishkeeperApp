using System;
using System.ComponentModel.DataAnnotations;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

public class ShoppingListItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Please select a tank")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a valid tank")]
    [Display(Name = "Tank")]
    public int TankId { get; set; }
    public Tank? Tank { get; set; }

    [Required(ErrorMessage = "Item type is required")]
    [Display(Name = "Item Type")]
    public ShoppingListItemType ItemType { get; set; }

    [Required(ErrorMessage = "Item name is required")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    [Display(Name = "Item Name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "Brand cannot exceed 100 characters")]
    [Display(Name = "Brand")]
    public string? Brand { get; set; }

    [StringLength(100, ErrorMessage = "Model cannot exceed 100 characters")]
    [Display(Name = "Model / Species")]
    public string? ModelOrSpecies { get; set; }

    [Display(Name = "Estimated Cost")]
    [DataType(DataType.Currency)]
    [Range(0, double.MaxValue, ErrorMessage = "Cost must be 0 or greater")]
    public decimal? EstimatedCost { get; set; }

    [Display(Name = "Quantity")]
    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; } = 1;

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Display(Name = "Priority")]
    public int Priority { get; set; } = 0; // 0=Low, 1=Medium, 2=High

    [Display(Name = "Added to Inventory")]
    public bool IsPurchased { get; set; } = false;

    [Display(Name = "Date Added")]
    [DataType(DataType.Date)]
    public DateTime DateAdded { get; set; } = DateTime.Now;

    [Display(Name = "Date Purchased")]
    [DataType(DataType.Date)]
    public DateTime? DatePurchased { get; set; }

    // For supply items
    [Display(Name = "Supply Category")]
    public SupplyCategory? SupplyCategory { get; set; }

    // For equipment items
    [Display(Name = "Equipment Type")]
    public string? EquipmentType { get; set; } // "Filter", "Heater", "Light", etc.

    [Display(Name = "Website/Store Link")]
    [Url(ErrorMessage = "Please enter a valid URL")]
    [StringLength(500, ErrorMessage = "URL cannot exceed 500 characters")]
    public string? PurchaseLink { get; set; }
}
