using System;
using AquaHub.MVC.Models.Enums;

namespace AquaHub.MVC.Models;

public class Expense
{
    public int Id { get; set; }

    // User and Tank relationship
    public string UserId { get; set; } = string.Empty;
    public AppUser? User { get; set; }

    // Optional tank association (null for general aquarium expenses)
    public int? TankId { get; set; }
    public Tank? Tank { get; set; }

    // Expense details
    public DateTime Date { get; set; }
    public ExpenseCategory Category { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";

    // Item details
    public string ItemName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public int Quantity { get; set; } = 1;
    public decimal? UnitPrice { get; set; }

    // Purchase information
    public string? Vendor { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? ReceiptNumber { get; set; }
    public string? ReceiptPhoto { get; set; }

    // Additional context
    public string Notes { get; set; } = string.Empty;
    public bool IsRecurring { get; set; } = false;
    public string? RecurrenceFrequency { get; set; } // Monthly, Quarterly, Annually

    // Optional links to other entities
    public int? EquipmentId { get; set; }
    public Equipment? Equipment { get; set; }

    public int? LivestockId { get; set; }
    public Livestock? Livestock { get; set; }

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
