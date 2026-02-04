# Maintenance Log Supply Tracking Enhancement

## Overview

Enhanced the Maintenance Log feature to integrate with the Supply Inventory system, allowing users to track supply usage when performing maintenance activities like dosing, testing, and other treatments.

## Date Implemented

February 4, 2026

## Changes Made

### 1. Database Schema Updates

**File: [Models/MaintenanceLog.cs](Models/MaintenanceLog.cs)**

- Added `SupplyItemId` (nullable int) - Foreign key to SupplyItem
- Added `SupplyItem` navigation property
- Added `AmountUsed` (nullable double) - Tracks quantity used with validation

**Migration**: `20260204163329_AddSupplyTrackingToMaintenanceLogs`

- Adds `SupplyItemId` column to MaintenanceLogs table
- Adds `AmountUsed` column to MaintenanceLogs table
- Creates foreign key relationship with SupplyItems table

### 2. Service Layer Updates

**File: [Services/Interfaces/IMaintenanceLogService.cs](Services/Interfaces/IMaintenanceLogService.cs)**

- Updated `CreateMaintenanceLogAsync` signature to accept optional `supplyItemId` and `amountUsed` parameters

**File: [Services/MaintenanceLogService.cs](Services/MaintenanceLogService.cs)**

- Added `ISupplyService` dependency injection
- Enhanced `CreateMaintenanceLogAsync` to:
  - Verify supply item ownership
  - Automatically deduct used quantity from inventory via `RemoveQuantityAsync`
  - Record supply usage in maintenance log
- Updated all Get methods to include SupplyItem navigation property
- Added note: Supply usage cannot be modified after creation to maintain inventory accuracy

### 3. Controller Updates

**File: [Controllers/MaintenanceLogController.cs](Controllers/MaintenanceLogController.cs)**

- Added `ISupplyService` dependency
- Updated `Create` GET action to populate supplies dropdown
- Updated `Create` POST action to:
  - Accept `SupplyItemId` and `AmountUsed` in Bind attribute
  - Pass supply tracking data to service layer
  - Display confirmation message when supply inventory is updated
- Added `PopulateSuppliesDropdown` method that:
  - Filters supplies by relevant categories (Medications, WaterTreatment, Supplements, TestKits, Chemicals)
  - Formats display with current quantity and unit
  - Provides category context in dropdown

### 4. View Updates

**File: [Views/MaintenanceLog/Create.cshtml](Views/MaintenanceLog/Create.cshtml)**

- Added new "Supply Section" (initially hidden) containing:
  - Supply Item dropdown (filtered to relevant categories)
  - Amount Used input field with step increment of 0.01
  - Helper text explaining inventory deduction
- Enhanced JavaScript to show/hide supply section based on maintenance type:
  - Shows for: Dosing, Testing, Other
  - Hidden for: Water changes, filter cleaning, glass cleaning, etc.
- Made field ID consistent (`maintenanceType`) for JavaScript targeting

**File: [Views/MaintenanceLog/Index.cshtml](Views/MaintenanceLog/Index.cshtml)**

- Added "Supply Used" column to table header
- Displays supply information when available:
  - Supply item name
  - Amount used with unit of measurement
  - Shows "—" when no supply was used

## Features

### User Workflow

1. User navigates to Log Maintenance
2. Selects a maintenance type (Dosing, Testing, or Other)
3. Supply section automatically appears
4. User can optionally:
   - Select a supply item from filtered dropdown
   - Enter amount used
5. Upon submission:
   - Maintenance log is created
   - Supply quantity is automatically deducted from inventory
   - Low stock alerts will trigger if inventory falls below minimum threshold

### Supply Categories Available for Maintenance

- **Medications** - Treatments for fish diseases
- **Water Treatment** - Conditioners, dechlorinators
- **Supplements** - Additives, nutrients
- **Test Kits** - Water parameter testing supplies
- **Chemicals** - General aquarium chemicals

### Inventory Integration Benefits

- ✅ Accurate inventory tracking
- ✅ Automatic quantity deduction
- ✅ Low stock alerts work correctly
- ✅ Historical usage data captured
- ✅ Prevents manual inventory updates

## Technical Details

### Data Flow

1. User submits maintenance log with supply data
2. Controller validates input and passes to service
3. Service verifies tank and supply ownership
4. Service calls `SupplyService.RemoveQuantityAsync()` to deduct inventory
5. If successful, maintenance log is saved with supply reference
6. Supply service checks thresholds and triggers alerts if needed

### Security & Validation

- User can only select supplies they own
- User can only add logs to tanks they own
- Amount used must be ≥ 0
- Supply deduction uses existing notification system for low stock alerts
- Supply usage cannot be edited after creation (inventory accuracy)

### Database Relationships

```
MaintenanceLog
├── TankId (FK) → Tank
└── SupplyItemId (FK, nullable) → SupplyItem
```

## Testing Recommendations

1. **Basic Supply Usage**
   - Create maintenance log with supply selection
   - Verify inventory decreases correctly
   - Check maintenance log displays supply info

2. **Low Stock Alerts**
   - Use supply until quantity drops below minimum
   - Verify alert is triggered
   - Check notification appears

3. **Optional Usage**
   - Create maintenance log without selecting supply
   - Verify it works normally

4. **Different Maintenance Types**
   - Test with Dosing type
   - Test with Testing type
   - Test with Other type
   - Verify supply section appears/hides correctly

5. **Validation**
   - Try negative amounts
   - Try exceeding available quantity
   - Test with supplies from another user (should fail)

## Future Enhancements (Optional)

1. **Usage History**
   - Add report showing supply usage over time
   - Track which supplies are used most frequently

2. **Automatic Suggestions**
   - Suggest supplies based on maintenance type
   - Remember frequently used combinations

3. **Batch Operations**
   - Allow selecting multiple supplies in one maintenance log
   - Support recipes/protocols with predefined supply combinations

4. **Restore on Delete**
   - Option to restore supply quantity when deleting maintenance logs
   - Requires careful consideration of inventory accuracy

## Notes

- Supply usage is intentionally not editable after creation to prevent inventory manipulation
- Deleting a maintenance log does NOT restore supply quantities (maintains data integrity)
- Only relevant supply categories are shown in the dropdown (not food, filter media, etc.)
- The feature gracefully handles optional usage - supplies are not required for all maintenance types

## Migration Commands

```bash
# Create migration
dotnet ef migrations add AddSupplyTrackingToMaintenanceLogs --project AquaHub.MVC.csproj

# Apply migration
dotnet ef database update --project AquaHub.MVC.csproj
```

## Files Modified

1. Models/MaintenanceLog.cs
2. Services/Interfaces/IMaintenanceLogService.cs
3. Services/MaintenanceLogService.cs
4. Controllers/MaintenanceLogController.cs
5. Views/MaintenanceLog/Create.cshtml
6. Views/MaintenanceLog/Index.cshtml

## Files Created

1. Migrations/20260204163329_AddSupplyTrackingToMaintenanceLogs.cs
2. MAINTENANCE_SUPPLY_TRACKING_UPDATE.md (this file)
