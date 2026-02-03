# AquaHub.MVC Controllers Documentation

## Overview

This document describes the MVC controllers created for the AquaHub application. All controllers follow ASP.NET Core best practices with proper dependency injection, authorization, error handling, and logging.

## Architecture Patterns

### Dependency Injection

All controllers use constructor-based dependency injection following these patterns:

- **Service Interfaces**: Injected as `IServiceName` (e.g., `ITankService`)
- **UserManager**: Injected for user authentication and authorization
- **ILogger**: Injected for structured logging

### Authorization

- All controllers are decorated with `[Authorize]` attribute
- User authentication is checked at the start of each action method
- User ID is retrieved using `_userManager.GetUserId(User)`

### Error Handling

- Try-catch blocks wrap all action methods
- Errors are logged using structured logging with contextual information
- User-friendly error messages are displayed using TempData
- Failed operations gracefully redirect to appropriate views

### Validation

- `[ValidateAntiForgeryToken]` protects all POST actions
- `[Bind]` attribute explicitly defines bindable properties
- ModelState validation ensures data integrity

## Controllers

### 1. TankController

**Purpose**: Manages aquarium tanks

**Key Actions**:

- `Index()` - List all user's tanks
- `Details(id)` - View tank details
- `Dashboard(id, month, year)` - View comprehensive tank dashboard with metrics
- `Create()` / `Create(tank)` - Add new tank
- `Edit(id)` / `Edit(id, tank)` - Modify existing tank
- `Delete(id)` / `DeleteConfirmed(id)` - Remove tank

**Dependencies**:

- `ITankService`
- `UserManager<AppUser>`
- `ILogger<TankController>`

---

### 2. LivestockController

**Purpose**: Manages fish, invertebrates, corals, and plants

**Key Actions**:

- `Index(tankId?)` - List livestock (optionally filtered by tank)
- `Details(id)` - View livestock details
- `Dashboard(id)` - View livestock-specific dashboard with growth and health data
- `Create(tankId?)` / `Create(livestock, tankId)` - Add new livestock
- `Edit(id)` / `Edit(id, livestock)` - Update livestock information
- `Delete(id)` / `DeleteConfirmed(id)` - Remove livestock

**Dependencies**:

- `ILivestockService`
- `ITankService` (for tank dropdown population)
- `UserManager<AppUser>`
- `ILogger<LivestockController>`

**Helper Methods**:

- `PopulateTanksDropdown(userId)` - Populates ViewBag with tank options

---

### 3. WaterTestController

**Purpose**: Records and analyzes water parameter testing

**Key Actions**:

- `Index(tankId?)` - List water tests (optionally filtered by tank)
- `Details(id)` - View test details
- `Trends(tankId, startDate?, endDate?)` - View parameter trends over time
- `Create(tankId?)` / `Create(waterTest, tankId)` - Record new test
- `Edit(id)` / `Edit(id, waterTest)` - Update test data
- `Delete(id)` / `DeleteConfirmed(id)` - Remove test record

**Parameters Tracked**:

- **Shared**: Temperature, pH, Ammonia, Nitrite, Nitrate
- **Freshwater**: GH, KH, TDS
- **Saltwater**: Salinity, Alkalinity, Calcium, Magnesium, Phosphate

**Dependencies**:

- `IWaterTestService`
- `ITankService`
- `UserManager<AppUser>`
- `ILogger<WaterTestController>`

---

### 4. EquipmentController

**Purpose**: Manages aquarium equipment (filters, heaters, lights, etc.)

**Key Actions**:

- `Index(tankId?)` - List equipment (optionally filtered by tank)
- `Details(id)` - View equipment details
- `Dashboard(id)` - View equipment maintenance and warranty information
- `Create(tankId?)` / `Create(equipment, tankId)` - Add new equipment
- `Edit(id)` / `Edit(id, equipment)` - Update equipment details
- `Delete(id)` / `DeleteConfirmed(id)` - Remove equipment

**Dependencies**:

- `IEquipmentService`
- `ITankService`
- `UserManager<AppUser>`
- `ILogger<EquipmentController>`

---

### 5. MaintenanceLogController

**Purpose**: Tracks tank maintenance activities

**Key Actions**:

- `Index(tankId?)` - List maintenance logs (optionally filtered by tank)
- `Details(id)` - View maintenance details
- `Create(tankId?)` / `Create(maintenanceLog, tankId)` - Record maintenance
- `Edit(id)` / `Edit(id, maintenanceLog)` - Update maintenance record
- `Delete(id)` / `DeleteConfirmed(id)` - Remove log entry

**Maintenance Types**:

- Water changes (with percentage tracking)
- Filter cleaning
- Equipment maintenance
- General maintenance tasks

**Dependencies**:

- `IMaintenanceLogService`
- `ITankService`
- `UserManager<AppUser>`
- `ILogger<MaintenanceLogController>`

---

### 6. ExpenseController

**Purpose**: Tracks aquarium-related expenses

**Key Actions**:

- `Index(tankId?, category?)` - List expenses with optional filters
- `Details(id)` - View expense details
- `Summary(startDate?, endDate?)` - View expense analytics and trends
- `Create(tankId?)` / `Create(expense)` - Record new expense
- `Edit(id)` / `Edit(id, expense)` - Update expense
- `Delete(id)` / `DeleteConfirmed(id)` - Remove expense

**Features**:

- Filter by tank or category
- Monthly spending trends
- Category totals
- Date range analysis

**Dependencies**:

- `IExpenseService`
- `ITankService`
- `UserManager<AppUser>`
- `ILogger<ExpenseController>`

---

### 7. ReminderController

**Purpose**: Manages maintenance and care reminders

**Key Actions**:

- `Index(filter)` - List reminders (all/active/upcoming/overdue)
- `Details(id)` - View reminder details
- `Create()` / `Create(reminder)` - Create new reminder
- `Edit(id)` / `Edit(id, reminder)` - Update reminder
- `Complete(id)` - Mark reminder as complete
- `Delete(id)` / `DeleteConfirmed(id)` - Remove reminder

**Filter Options**:

- `all` - All reminders
- `active` - Currently active reminders
- `upcoming` - Reminders due within 7 days
- `overdue` - Past due reminders

**Dependencies**:

- `IReminderService`
- `UserManager<AppUser>`
- `ILogger<ReminderController>`

---

### 8. GrowthRecordController

**Purpose**: Tracks growth measurements for livestock

**Key Actions**:

- `Index(livestockId?)` - List growth records (optionally filtered by livestock)
- `Details(id)` - View record details
- `Statistics(livestockId)` - View growth statistics and trends
- `Create(livestockId)` / `Create(growthRecord)` - Record new measurement
- `Edit(id)` / `Edit(id, growthRecord)` - Update measurement
- `Delete(id)` / `DeleteConfirmed(id)` - Remove record

**Measurements Tracked**:

- **Fish/Invertebrates**: Length (inches), Weight (grams)
- **Corals/Plants**: Diameter (inches), Height (inches)
- **Qualitative**: Health condition, Color vibrancy
- **Notes**: Observation notes and photo references

**Dependencies**:

- `IGrowthRecordService`
- `ILivestockService`
- `UserManager<AppUser>`
- `ILogger<GrowthRecordController>`

---

## Dependency Injection Configuration

All services are registered in [Program.cs](Program.cs) using the Scoped lifetime:

```csharp
// Core Services
builder.Services.AddScoped<ITankService, TankService>();
builder.Services.AddScoped<ILivestockService, LivestockService>();
builder.Services.AddScoped<IEquipmentService, EquipmentService>();

// Monitoring & Testing Services
builder.Services.AddScoped<IWaterTestService, WaterTestService>();
builder.Services.AddScoped<IMaintenanceLogService, MaintenanceLogService>();
builder.Services.AddScoped<IGrowthRecordService, GrowthRecordService>();

// Financial Services
builder.Services.AddScoped<IExpenseService, ExpenseService>();

// Notification & Reminder Services
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IPredictiveReminderService, PredictiveReminderService>();

// Health & Alert Services
builder.Services.AddScoped<ITankHealthService, TankHealthService>();
builder.Services.AddScoped<IParameterAlertService, ParameterAlertService>();
```

## Common Patterns

### User Context Retrieval

```csharp
var userId = _userManager.GetUserId(User);
if (string.IsNullOrEmpty(userId))
{
    return Unauthorized();
}
```

### Success/Error Messages

```csharp
TempData["Success"] = "Operation completed successfully!";
TempData["Error"] = "An error occurred during the operation.";
```

### Structured Logging

```csharp
_logger.LogError(ex, "Error message with context: {ContextId}", id);
```

### Dropdown Population Helper

```csharp
private async Task PopulateTanksDropdown(string userId)
{
    var tanks = await _tankService.GetAllTanksAsync(userId);
    ViewBag.Tanks = new SelectList(tanks, "Id", "Name");
}
```

## Next Steps

1. **Create Views**: Build Razor views for each controller action
2. **Add Navigation**: Update layout with links to all controllers
3. **Implement ViewModels**: Use the existing ViewModels for complex views
4. **Add Validation**: Create client-side validation attributes
5. **Enhance UI**: Add charts and visualizations for dashboards
6. **Testing**: Create unit tests for each controller

## Additional Notes

- All controllers handle both authenticated and unauthenticated scenarios
- Foreign key relationships are maintained through proper service layer calls
- Date/time properties are pre-populated with current values for user convenience
- All DELETE operations use POST with ActionName attribute for safety
- Controllers are designed to be testable with proper separation of concerns
