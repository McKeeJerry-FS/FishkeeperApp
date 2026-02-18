# Quick Logging Implementation Template

This template shows how to add comprehensive logging to any service or controller in AquaHub.

## Service Implementation Template

### 1. Add Required Using Statements

```csharp
using AquaHub.MVC.Utilities;
```

### 2. Inject ILogger in Constructor

```csharp
public class YourService : IYourService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<YourService> _logger;  // Add this

    public YourService(
        ApplicationDbContext context,
        ILogger<YourService> logger)  // Add this parameter
    {
        _context = context;
        _logger = logger;  // Add this
    }
}
```

### 3. Wrap Methods with LogExecutionTimeAsync

**For methods returning a value:**

```csharp
public async Task<YourModel> CreateAsync(YourModel model, string userId)
{
    return await _logger.LogExecutionTimeAsync(
        "CreateYourModel",  // Operation name
        async () =>
        {
            using (_logger.BeginUserScope(userId))
            {
                _logger.LogInformation(
                    LoggingConstants.EventIds.YourServiceOperationStarted,
                    "Creating {ModelType}: {ModelName}",
                    nameof(YourModel),
                    model.Name);

                // Your business logic here
                _context.YourModels.Add(model);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    LoggingConstants.EventIds.YourServiceOperationCompleted,
                    "{ModelType} created successfully with ID: {Id}",
                    nameof(YourModel),
                    model.Id);

                return model;
            }
        },
        LoggingConstants.EventIds.YourServiceOperationStarted,
        new Dictionary<string, object>
        {
            [LoggingConstants.Properties.UserId] = userId,
            ["ModelName"] = model.Name
        });
}
```

**For void methods:**

```csharp
public async Task DeleteAsync(int id, string userId)
{
    await _logger.LogExecutionTimeAsync(
        "DeleteYourModel",
        async () =>
        {
            using (_logger.BeginUserScope(userId))
            {
                _logger.LogInformation(
                    LoggingConstants.EventIds.YourServiceOperationStarted,
                    "Deleting {ModelType} with ID: {Id}",
                    nameof(YourModel),
                    id);

                var model = await _context.YourModels.FindAsync(id);
                if (model == null)
                {
                    _logger.LogWarning("Model not found for deletion");
                    return;
                }

                _context.YourModels.Remove(model);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    LoggingConstants.EventIds.YourServiceOperationCompleted,
                    "{ModelType} deleted successfully",
                    nameof(YourModel));
            }
        },
        LoggingConstants.EventIds.YourServiceOperationStarted,
        new Dictionary<string, object>
        {
            [LoggingConstants.Properties.UserId] = userId,
            [LoggingConstants.Properties.EntityId] = id
        });
}
```

## Controller Implementation Template

### 1. Add Required Using Statements

```csharp
using AquaHub.MVC.Utilities;
```

### 2. Inject ILogger in Constructor

```csharp
public class YourController : Controller
{
    private readonly IYourService _yourService;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<YourController> _logger;  // Add this

    public YourController(
        IYourService yourService,
        UserManager<AppUser> userManager,
        ILogger<YourController> logger)  // Add this parameter
    {
        _yourService = yourService;
        _userManager = userManager;
        _logger = logger;  // Add this
    }
}
```

### 3. Add Logging to Actions

**Index/List Action:**

```csharp
public async Task<IActionResult> Index()
{
    try
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning(
                LoggingConstants.EventIds.UnauthorizedAccess,
                "Unauthorized access attempt to {Controller} Index",
                nameof(YourController));
            return Unauthorized();
        }

        using (_logger.BeginUserScope(userId))
        {
            _logger.LogDebug("Loading items for user");

            var items = await _yourService.GetAllAsync(userId);

            _logger.LogInformation(
                "Loaded {ItemCount} items",
                items?.Count() ?? 0);

            return View(items);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex,
            "Error loading {Controller} index",
            nameof(YourController));
        return View("Error");
    }
}
```

**Create Action (GET):**

```csharp
[HttpGet]
public IActionResult Create()
{
    _logger.LogDebug("Displaying create form");
    return View();
}
```

**Create Action (POST):**

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(YourModel model)
{
    try
    {
        if (!ModelState.IsValid)
        {
            _logger.LogValidationFailure(
                nameof(YourModel),
                ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()));
            return View(model);
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning(
                LoggingConstants.EventIds.UnauthorizedAccess,
                "Unauthorized create attempt");
            return Unauthorized();
        }

        using (_logger.BeginUserScope(userId))
        {
            _logger.LogInformation(
                "User initiated create: {ModelName}",
                model.Name);

            var created = await _yourService.CreateAsync(model, userId);

            _logger.LogInformation(
                "Successfully created {ModelType} with ID: {Id}",
                nameof(YourModel),
                created.Id);

            return RedirectToAction(nameof(Index));
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex,
            "Error creating {ModelType}",
            nameof(YourModel));
        ModelState.AddModelError("", "An error occurred while creating the item.");
        return View(model);
    }
}
```

**Edit Action (GET):**

```csharp
[HttpGet]
public async Task<IActionResult> Edit(int id)
{
    try
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        using (_logger.BeginUserScope(userId))
        {
            _logger.LogDebug(
                "Loading {ModelType} for edit: ID {Id}",
                nameof(YourModel),
                id);

            var model = await _yourService.GetByIdAsync(id, userId);
            if (model == null)
            {
                _logger.LogWarning(
                    "{ModelType} not found: ID {Id}",
                    nameof(YourModel),
                    id);
                return NotFound();
            }

            return View(model);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex,
            "Error loading {ModelType} for edit",
            nameof(YourModel));
        return View("Error");
    }
}
```

**Edit Action (POST):**

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, YourModel model)
{
    if (id != model.Id)
    {
        _logger.LogWarning(
            "ID mismatch in edit: URL ID {UrlId} vs Model ID {ModelId}",
            id,
            model.Id);
        return BadRequest();
    }

    try
    {
        if (!ModelState.IsValid)
        {
            _logger.LogValidationFailure(
                nameof(YourModel),
                ModelState.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()));
            return View(model);
        }

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        using (_logger.BeginUserScope(userId))
        {
            _logger.LogInformation(
                "User initiated update of {ModelType} ID {Id}",
                nameof(YourModel),
                id);

            await _yourService.UpdateAsync(model, userId);

            _logger.LogInformation(
                "Successfully updated {ModelType} ID {Id}",
                nameof(YourModel),
                id);

            return RedirectToAction(nameof(Index));
        }
    }
    catch (UnauthorizedAccessException ex)
    {
        _logger.LogWarning(ex,
            LoggingConstants.EventIds.UnauthorizedAccess,
            "Unauthorized update attempt");
        return Forbid();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex,
            "Error updating {ModelType}",
            nameof(YourModel));
        ModelState.AddModelError("", "An error occurred while updating the item.");
        return View(model);
    }
}
```

**Delete Action (POST):**

```csharp
[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    try
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        using (_logger.BeginUserScope(userId))
        {
            _logger.LogInformation(
                "User initiated delete of {ModelType} ID {Id}",
                nameof(YourModel),
                id);

            var success = await _yourService.DeleteAsync(id, userId);

            if (!success)
            {
                _logger.LogWarning(
                    "Delete failed - {ModelType} not found: ID {Id}",
                    nameof(YourModel),
                    id);
                return NotFound();
            }

            _logger.LogInformation(
                "Successfully deleted {ModelType} ID {Id}",
                nameof(YourModel),
                id);

            return RedirectToAction(nameof(Index));
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex,
            "Error deleting {ModelType}",
            nameof(YourModel));
        return View("Error");
    }
}
```

## Common Logging Patterns

### Log a Query

```csharp
_logger.LogDebug("Fetching {EntityType} for user", nameof(YourModel));
var items = await _context.YourModels.Where(x => x.UserId == userId).ToListAsync();
_logger.LogInformation("Retrieved {ItemCount} {EntityType} items", items.Count, nameof(YourModel));
```

### Log with Multiple Scopes

```csharp
using (_logger.BeginUserScope(userId))
using (_logger.BeginTankScope(tankId))
{
    _logger.LogInformation("Processing tank operation");
    // All logs include userId and tankId
}
```

### Log a Warning

```csharp
if (result == null)
{
    _logger.LogWarning(
        "{EntityType} not found for ID {Id}",
        nameof(YourModel),
        id);
}
```

### Log an Error with Exception

```csharp
catch (DbUpdateException ex)
{
    _logger.LogError(ex,
        "Database error while saving {EntityType}",
        nameof(YourModel));
    throw;
}
```

### Log Performance Issue

```csharp
if (stopwatch.ElapsedMilliseconds > 1000)
{
    _logger.LogWarning(
        LoggingConstants.EventIds.SlowOperationDetected,
        "Slow query: {OperationName} took {Duration}ms",
        operationName,
        stopwatch.ElapsedMilliseconds);
}
```

## Event ID Reference (Quick Access)

Use these constants from `LoggingConstants.EventIds`:

```csharp
// Service Operations (adjust range per service)
TankServiceOperationStarted = 3000
TankServiceOperationCompleted = 3001
TankServiceOperationFailed = 3002

// Specific Events
TankCreated = 3010
TankUpdated = 3011
TankDeleted = 3012

// Security
UnauthorizedAccess = 4002

// Validation
ValidationFailed = 6000

// Performance
SlowOperationDetected = 8001

// Errors
UnhandledException = 9000
```

## Best Practices Checklist

✅ Always use structured logging (template syntax, not string interpolation)  
✅ Include relevant context (userId, tankId, entityId)  
✅ Use appropriate log levels (Debug, Info, Warning, Error)  
✅ Use scopes for related operations  
✅ Log operation start, completion, and errors  
✅ Include timing information for performance tracking  
✅ Log validation failures  
✅ Never log sensitive data (passwords, tokens)  
✅ Use event IDs consistently  
✅ Wrap service methods with LogExecutionTimeAsync

## Testing Your Logging

1. Run the application in Development mode
2. Perform operations (create, read, update, delete)
3. Check console output for structured logs
4. Verify logs include:
   - Timestamps
   - Log levels
   - Event IDs
   - Structured properties
   - Scopes (userId, tankId, etc.)
   - Execution times

## Example Console Output

```
[2026-02-18 10:15:23.456 Debug] AquaHub.MVC.Services.TankService: Starting operation: CreateTank | UserId: abc123 | TankName: Main Reef
[2026-02-18 10:15:23.467 Info] AquaHub.MVC.Services.TankService: Creating new tank: Main Reef, Type: Reef, Volume: 75gal | UserId: abc123
[2026-02-18 10:15:23.498 Info] AquaHub.MVC.Services.TankService: Tank created successfully with ID: 5 | UserId: abc123
[2026-02-18 10:15:23.499 Info] AquaHub.MVC.Services.TankService: Completed operation: CreateTank in 43ms | UserId: abc123
[2026-02-18 10:15:23.500 Info] AquaHub.MVC.Middleware.RequestLoggingMiddleware: HTTP Request completed: POST /Tank/Create - Status: 302 - Duration: 156ms
```

---

**For complete documentation, see:** [COMPREHENSIVE_LOGGING_GUIDE.md](COMPREHENSIVE_LOGGING_GUIDE.md)
