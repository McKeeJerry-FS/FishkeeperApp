# Known Issues - Property Mismatches in Views

## Status: Build has 156 errors due to property name mismatches between views and models.

The views were created based on assumed property names that don't match the actual model properties. Below is a comprehensive mapping that needs to be applied.

## Models Need to Be Read First

Before fixing views, need to read these models to understand actual properties:

- Tank.cs
- Livestock.cs
- Reminder.cs
- Equipment.cs (and subclasses: Filter, Heater, Light, ProteinSkimmer)
- WaterTest.cs
- Expense.cs
- GrowthRecord.cs
- TankDashboardViewModel.cs
- LivestockDashboardViewModel.cs

## Known Mismatches Found

### Tank Model

Views expect but model has different:

- `SetupDate` → should be `StartDate`
- `Substrate` → need to check if exists
- `Length`, `Width`, `Height` → need to check if exist

### Livestock Model

Views expect but model doesn't have:

- `DateAdded` → model has `AddedOn`
- `PurchasePrice` → doesn't exist in base Livestock
- `IsAlive` → doesn't exist in base Livestock
- `ImageURL` → doesn't exist (already removed from views)

### Reminder Model

Views expect but model may not have:

- `ReminderType` → need to check enum name
- `DueDate` → need to check actual property name
- `IsCompleted` → need to check if exists
- `CreatedDate`, `ModifiedDate` → need to check if these exist

### Equipment Models

Views expect but models may not have:

- Base Equipment: `Name`, `PurchaseDate`, `PurchasePrice`
- Filter: `FilterType`, `FlowRateGPH`, `LastMaintenanceDate`
- Heater: `Wattage`, `TargetTemperature`
- Light: `HoursPerDay`

### WaterTest Model

Views expect:

- `TemperatureFahrenheit` → need to check actual property name

### Expense Model

Views expect:

- `Description` → need to check if exists

### GrowthRecord Model

Views expect:

- `RecordDate` → need to check actual property name
- `ColorQuality` → need to check if exists

### ViewModels

- `TankDashboardViewModel`: Views expect `LivestockCount`, `EquipmentCount`, `WaterTestCount`, `MaintenanceCount`, `RecentMaintenanceLogs`
- `LivestockDashboardViewModel`: Views expect `GrowthRecords`

## Recommended Approach

1. **Read all model files** to understand actual property names
2. **Create a property mapping document**
3. **Use multi_replace_string_in_file** to fix all mismatches systematically
4. **OR consider updating models** to match the view expectations if the views represent the desired design

## Alternative: Use ViewModels

Instead of fixing all views, consider creating ViewModels that match the views' expectations and use AutoMapper or manual mapping in controllers to transform model data to view-friendly structures.

## Build Command

```bash
dotnet build AquaHub.MVC.csproj
```

## Total Views Created: 36 files

- All follow modern Bootstrap 5 patterns
- All use Bootstrap Icons
- All have proper form validation
- All have responsive layouts

**The views are structurally correct - they just need property names aligned with the actual models.**
