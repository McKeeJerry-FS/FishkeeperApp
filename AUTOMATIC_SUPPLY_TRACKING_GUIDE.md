# Automatic Supply Usage Tracking Guide

## Overview

The supply tracking system now automatically deducts inventory when you log dosing activities. This ensures your supply levels stay accurate and you receive timely low stock alerts.

## How It Works

When you add a dosing record and link it to a supply item:
1. The system automatically deducts the dosage amount from the supply inventory
2. Updates the "last used" date
3. Logs the usage in the supply's notes
4. Checks if the supply has reached low stock threshold
5. Sends a notification if low stock alert is enabled

## Setting Up Supply Tracking for Dosing

### Step 1: Create Your Supply Items

1. Navigate to **Supplies** → **Add Supply**
2. Fill in the details:
   - **Name**: e.g., "Seachem Prime"
   - **Category**: Choose "Water Treatment", "Supplements", "Chemicals", or "Medications"
   - **Current Quantity**: e.g., 350 (the amount you currently have)
   - **Minimum Quantity**: e.g., 50 (when to alert you)
   - **Unit**: Choose "ml" for liquids
   - **Average Usage Per Week**: Optional but recommended for "days remaining" calculation

### Step 2: Enable Low Stock Alerts

Make sure **"Enable Low Stock Alerts"** is checked when creating the supply item. This will automatically notify you when stock falls below the minimum quantity.

## Using Supply Tracking with Dosing

### Method 1: Manual Deduction (Current)

When you dose a product:

1. Go to **Supplies**
2. Find the supply item you used
3. Click **"Details"**
4. Click **"Use Supply"** button
5. Enter the amount used (e.g., 5 ml)
6. The system will:
   - Deduct 5ml from inventory (350ml → 345ml)
   - Update last used date
   - Log the usage in notes
   - Check low stock threshold
   - Send alert if needed

### Method 2: Integration with Dosing Records (Recommended)

**Coming in next update:** When creating a dosing record through the tank dashboard:

1. Go to your Tank Dashboard
2. Click "Add Dosing Record"
3. Fill in details:
   - **Select Supply Item**: Choose from dropdown (shows only relevant categories)
   - **Amount**: Enter dosage (e.g., 5ml)
   - **Notes**: Optional details
4. Submit
5. The system automatically:
   - Creates the dosing record
   - Calls `RecordSupplyUsageAsync()` to deduct from inventory
   - Updates supply status
   - Sends low stock alert if threshold reached

## Example Workflow

### Scenario: Dosing Water Conditioner

**Initial Setup:**
- Supply: "Seachem Prime"
- Current Quantity: 350 ml
- Minimum Quantity: 50 ml
- Unit: ml
- Average Usage: 20 ml/week (for predictive tracking)

**Weekly Dosing:**

**Week 1:** Water change, dose 5ml
- Before: 350 ml
- After: 345 ml
- Status: In Stock ✓
- Days Remaining: ~69 days

**Week 2:** Water change, dose 5ml
- Before: 345 ml
- After: 340 ml
- Status: In Stock ✓

... (weeks continue)

**Week 60:** Water change, dose 5ml
- Before: 55 ml
- After: 50 ml
- Status: **Low Stock ⚠️**
- Alert sent: "Your Seachem Prime is running low. Current: 50ml, Minimum: 50ml"

**Week 61:** Water change, dose 5ml
- Before: 50 ml
- After: 45 ml
- Status: **Low Stock ⚠️**
- Days Remaining: ~4 days

## API Methods for Developers

### RecordSupplyUsageAsync

Automatically deduct supply when used:

```csharp
await _supplyService.RecordSupplyUsageAsync(
    supplyId: 123,
    amountUsed: 5.0,
    userId: userId,
    notes: "Water change dosing"
);
```

This method:
- Validates supply exists and belongs to user
- Deducts amount from current quantity
- Updates last used date
- Logs usage in notes with timestamp
- Checks low stock threshold
- Sends notification if needed
- Returns true if successful

### GetSuppliesForDosingAsync

Get supplies suitable for dosing:

```csharp
var dosingSupplies = await _supplyService.GetSuppliesForDosingAsync(userId);
```

Returns supplies in categories:
- Supplements
- Water Treatment
- Chemicals
- Medications

## Best Practices

### 1. Accurate Initial Quantities
When adding a new supply, measure the actual quantity instead of using the bottle size. A "500ml" bottle might only have 480ml after accounting for the cap and tube.

### 2. Track Average Usage
Enter your typical weekly usage to get accurate "days remaining" estimates. Update this periodically as your dosing routine changes.

### 3. Set Appropriate Minimums
Set your minimum quantity to give yourself time to reorder:
- **Fast shipping (2-3 days)**: Minimum = 1 week usage
- **Slow shipping (7-14 days)**: Minimum = 3 weeks usage
- **Hard to find items**: Minimum = 1 month usage

### 4. Use Product URLs
Add product URLs to supply items for one-click reordering when you receive low stock alerts.

### 5. Regular Audits
Periodically verify actual quantities match system quantities. Adjust if needed using the "Set Quantity" feature.

## Notifications

You'll receive notifications when:

1. **Low Stock Alert**: Supply falls below minimum quantity
2. **Out of Stock**: Supply quantity reaches zero
3. **Expiring Soon**: Supply expires within 30 days (if expiration date set)

Notifications appear in:
- Notification bell icon (top navigation)
- Low Stock page (Supplies → Low Stock)
- Home dashboard (if configured)

## Shopping List Feature

The **Low Stock** page auto-generates a shopping list showing:
- Items needing attention (priority sorted)
- Calculated quantities needed (optimal - current)
- Preferred vendors
- Product links for easy reordering
- Printable format for in-store shopping

## Troubleshooting

### Supply Not Deducting
**Problem**: Dosing but inventory not updating

**Solutions**:
1. Verify you're using the "Use Supply" button or integrated dosing
2. Check you have permission to modify the supply
3. Ensure supply is marked as "Active"

### Incorrect Low Stock Alerts
**Problem**: Getting alerts too early or too late

**Solutions**:
1. Adjust the "Minimum Quantity" threshold
2. Verify "Enable Low Stock Alerts" is checked
3. Update "Average Usage Per Week" for better predictions

### Supply Not in Dosing Dropdown
**Problem**: Can't find supply when creating dosing record

**Solutions**:
1. Verify supply category is appropriate (Supplements, Water Treatment, Chemicals, or Medications)
2. Check supply is marked as "Active"
3. Ensure it's associated with the correct tank (or set to "General")

## Future Enhancements

Planned improvements:
- **Barcode scanning**: Quick add/use via barcode
- **Dosing schedules**: Auto-deduct on schedule
- **Batch operations**: Process multiple supplies at once
- **Recipe management**: Save dosing combinations
- **Analytics dashboard**: Usage trends and predictions
- **Integration with equipment**: Auto-track when dosing pumps run

---

**Last Updated**: February 4, 2026
**Version**: 2.0
