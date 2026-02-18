# Comprehensive Logging Guide for AquaHub

## Overview

AquaHub now implements comprehensive, structured logging throughout the application to support observability and prepare for OpenTelemetry integration. This guide covers the logging infrastructure, best practices, and usage patterns.

## Table of Contents

1. [Logging Architecture](#logging-architecture)
2. [Configuration](#configuration)
3. [Structured Logging](#structured-logging)
4. [Logging Constants & Event IDs](#logging-constants--event-ids)
5. [Logging Extensions](#logging-extensions)
6. [Middleware](#middleware)
7. [Service Layer Logging](#service-layer-logging)
8. [Controller Layer Logging](#controller-layer-logging)
9. [Performance Tracking](#performance-tracking)
10. [OpenTelemetry Preparation](#opentelemetry-preparation)
11. [Best Practices](#best-practices)

---

## Logging Architecture

### Components

The logging infrastructure consists of:

- **Configuration Files**: `appsettings.json`, `appsettings.Development.json`
- **Middleware**: `RequestLoggingMiddleware`, `ExceptionHandlingMiddleware`
- **Utilities**: `LoggingConstants`, `LoggingExtensions`
- **Service Integration**: ILogger<T> injected into all services and controllers

### Log Levels

- **Trace**: Very detailed diagnostic information (rarely used)
- **Debug**: Detailed flow information for development and debugging
- **Information**: General application flow and significant events
- **Warning**: Abnormal or unexpected events that don't stop execution
- **Error**: Errors and exceptions that prevent specific operations
- **Critical**: Catastrophic failures requiring immediate attention

---

## Configuration

### Production Configuration (appsettings.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information",
      "AquaHub.MVC": "Information"
    },
    "Console": {
      "FormatterName": "json",
      "FormatterOptions": {
        "SingleLine": false,
        "IncludeScopes": true,
        "TimestampFormat": "yyyy-MM-dd HH:mm:ss.fff ",
        "UseUtcTimestamp": true
      }
    }
  }
}
```

### Development Configuration (appsettings.Development.json)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Debug",
      "AquaHub.MVC.Controllers": "Debug",
      "AquaHub.MVC.Services": "Debug"
    },
    "Console": {
      "FormatterName": "simple",
      "FormatterOptions": {
        "SingleLine": false,
        "IncludeScopes": true,
        "TimestampFormat": "yyyy-MM-dd HH:mm:ss.fff "
      }
    }
  }
}
```

**Key Differences:**

- **Production**: JSON formatted logs for structured parsing, Information level
- **Development**: Simple formatted logs for readability, Debug level

---

## Structured Logging

### What is Structured Logging?

Structured logging captures log data as key-value pairs instead of plain text. This makes logs:

- **Searchable**: Query by specific fields
- **Filterable**: Filter by user ID, tank ID, operation type, etc.
- **Analyzable**: Aggregate metrics and insights
- **OpenTelemetry-Ready**: Direct mapping to telemetry attributes

### Example: Structured vs Unstructured

**❌ Unstructured:**

```csharp
_logger.LogInformation($"User {userId} created tank {tankName} with volume {volume}");
```

**✅ Structured:**

```csharp
_logger.LogInformation(
    "Tank created: {TankName} with volume {VolumeGallons}",
    tankName,
    volume);
```

### Using Scopes for Context

Scopes add contextual information to all logs within a using block:

```csharp
using (_logger.BeginScope(new Dictionary<string, object>
{
    ["UserId"] = userId,
    ["TankId"] = tankId,
    ["OperationName"] = "UpdateTank"
}))
{
    _logger.LogInformation("Operation started");
    // ... operation logic
    _logger.LogInformation("Operation completed");
}
```

All logs within the scope automatically include UserId, TankId, and OperationName.

---

## Logging Constants & Event IDs

### Event IDs

Event IDs are numeric codes that categorize log events. They enable:

- **Filtering**: Show only specific types of events
- **Alerting**: Trigger alerts on critical event IDs
- **Monitoring**: Track frequency of specific operations

### Event ID Ranges

Located in `Utilities/LoggingConstants.cs`:

| Range     | Category           | Examples                            |
| --------- | ------------------ | ----------------------------------- |
| 1000-1099 | Application Events | Startup, Shutdown, Configuration    |
| 1100-1199 | HTTP Requests      | Request Started, Completed, Failed  |
| 2000-2099 | Database Events    | Query, Save, Connection             |
| 3000-3999 | Service Operations | Tank, Livestock, Water Test, etc.   |
| 4000-4099 | Security & Auth    | Authentication, Authorization       |
| 5000-5099 | File Operations    | Upload, Delete, Read                |
| 6000-6099 | Validation         | Validation Failures, Business Rules |
| 7000-7099 | External APIs      | API Calls, Integrations             |
| 8000-8099 | Performance        | Metrics, Slow Operations            |
| 9000-9099 | Errors             | Exceptions, Unhandled Errors        |

### Usage Example

```csharp
_logger.LogInformation(
    LoggingConstants.EventIds.TankCreated,
    "Tank created successfully with ID: {TankId}",
    tank.Id);
```

---

## Logging Extensions

The `LoggingExtensions` class provides helper methods for common logging patterns.

### LogExecutionTimeAsync

Automatically logs operation start, completion, and timing:

```csharp
public async Task<Tank> CreateTankAsync(Tank tank, string userId)
{
    return await _logger.LogExecutionTimeAsync(
        "CreateTank",
        async () =>
        {
            // Your business logic here
            tank.UserId = userId;
            _context.Tanks.Add(tank);
            await _context.SaveChangesAsync();
            return tank;
        },
        LoggingConstants.EventIds.TankServiceOperationStarted,
        new Dictionary<string, object>
        {
            [LoggingConstants.Properties.UserId] = userId,
            ["TankName"] = tank.Name
        });
}
```

**Output:**

```
[DEBUG] Starting operation: CreateTank | UserId: abc123 | TankName: Main Reef
[INFO] Completed operation: CreateTank in 45ms | UserId: abc123 | TankName: Main Reef
```

### BeginUserScope

Creates a scope with user context:

```csharp
using (_logger.BeginUserScope(userId, userName))
{
    _logger.LogInformation("Fetching user data");
    // All logs here include userId and userName
}
```

### BeginTankScope

Creates a scope with tank context:

```csharp
using (_logger.BeginTankScope(tankId, tankName))
{
    _logger.LogInformation("Processing tank operation");
    // All logs here include tankId and tankName
}
```

### LogDatabaseMetrics

Specialized logging for database operations:

```csharp
_logger.LogDatabaseMetrics("InsertTank", durationMs: 42, rowsAffected: 1);
```

### LogValidationFailure

Logs validation errors with structured data:

```csharp
_logger.LogValidationFailure(
    "Tank",
    new Dictionary<string, string[]>
    {
        ["Name"] = new[] { "Name is required" },
        ["VolumeGallons"] = new[] { "Volume must be greater than 0" }
    });
```

---

## Middleware

### RequestLoggingMiddleware

Logs every HTTP request with:

- Request ID (for correlation)
- HTTP method and path
- Query string
- User agent
- Status code
- Duration

**Example Output:**

```
[INFO] HTTP Request started: GET /Tank/Dashboard/5?month=2&year=2026 | RequestId: f7a3b2c1-...
[INFO] HTTP Request completed: GET /Tank/Dashboard/5 - Status: 200 - Duration: 156ms
```

### ExceptionHandlingMiddleware

Catches unhandled exceptions and:

- Logs with full context
- Returns consistent error responses
- Includes request ID for correlation
- Provides detailed errors in development, sanitized in production

**Example Output:**

```
[ERROR] Exception occurred: ArgumentException - Invalid tank ID |
        Path: /Tank/Edit/999 | Method: POST | RequestId: f7a3b2c1-...
```

---

## Service Layer Logging

### Pattern: Comprehensive Service Logging

Every service method should:

1. Use `LogExecutionTimeAsync` wrapper
2. Create appropriate scopes (user, tank, etc.)
3. Log operation start (Debug level)
4. Log significant milestones (Info level)
5. Log warnings for unexpected conditions
6. Log errors for failures
7. Include structured data

### Example: TankService.CreateTankAsync

```csharp
public async Task<Tank> CreateTankAsync(Tank tank, string userId)
{
    return await _logger.LogExecutionTimeAsync(
        "CreateTank",
        async () =>
        {
            using (_logger.BeginUserScope(userId))
            {
                _logger.LogInformation(
                    LoggingConstants.EventIds.TankServiceOperationStarted,
                    "Creating new tank: {TankName}, Type: {TankType}, Volume: {VolumeGallons}gal",
                    tank.Name,
                    tank.Type,
                    tank.VolumeGallons);

                tank.UserId = userId;
                tank.StartDate = DateTime.SpecifyKind(tank.StartDate.Date, DateTimeKind.Utc);

                if (tank.ShoppingListItems == null)
                {
                    tank.ShoppingListItems = new List<ShoppingListItem>();
                }

                _context.Tanks.Add(tank);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    LoggingConstants.EventIds.TankCreated,
                    "Tank created successfully with ID: {TankId}",
                    tank.Id);

                return tank;
            }
        },
        LoggingConstants.EventIds.TankServiceOperationStarted,
        new Dictionary<string, object>
        {
            [LoggingConstants.Properties.UserId] = userId,
            ["TankName"] = tank.Name,
            ["TankType"] = tank.Type.ToString()
        });
}
```

### What to Log in Services

✅ **DO Log:**

- Operation start/completion
- Data retrieval counts
- Business logic decisions
- Data modifications (create, update, delete)
- Performance metrics
- Warnings (missing data, unexpected states)
- Errors and exceptions

❌ **DON'T Log:**

- Sensitive data (passwords, tokens)
- Personal information (unless properly anonymized)
- Excessive detail at Information level
- Duplicate information

---

## Controller Layer Logging

### Pattern: Controller Logging

Controllers should log:

1. Action entry (Debug level)
2. Authorization checks
3. Model validation failures
4. Service call results
5. View rendering
6. Redirects

### Example: TankController.Index

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
                "Unauthorized access attempt to Tank Index");
            return Unauthorized();
        }

        using (_logger.BeginUserScope(userId))
        {
            _logger.LogDebug("Loading tanks for user");

            var tanks = await _tankService.GetAllTanksAsync(userId);

            _logger.LogInformation(
                "Tank Index loaded with {TankCount} tanks",
                tanks?.Count() ?? 0);

            return View(tanks);
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex,
            "Error loading tank index");
        return View("Error");
    }
}
```

---

## Performance Tracking

### Automatic Timing

`LogExecutionTimeAsync` automatically tracks and logs operation duration:

```csharp
await _logger.LogExecutionTimeAsync("OperationName", async () =>
{
    // Your code
}, eventId, properties);
```

### Slow Operation Detection

Configure alerts for operations exceeding thresholds:

```csharp
if (stopwatch.ElapsedMilliseconds > 1000)
{
    _logger.LogWarning(
        LoggingConstants.EventIds.SlowOperationDetected,
        "Slow operation: {OperationName} took {Duration}ms",
        operationName,
        stopwatch.ElapsedMilliseconds);
}
```

---

## OpenTelemetry Preparation

### Why This Logging Prepares for OpenTelemetry

1. **Structured Data**: All logs use structured properties that map directly to OpenTelemetry attributes
2. **Event IDs**: Categorization ready for spans and metrics
3. **Scopes**: Context propagation aligns with OpenTelemetry trace context
4. **Timing**: Duration tracking maps to span duration
5. **Correlation**: Request IDs enable distributed tracing

### Migration Path to OpenTelemetry

When ready to add OpenTelemetry:

1. **Install Packages:**

```bash
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
dotnet add package OpenTelemetry.Instrumentation.EntityFrameworkCore
dotnet add package OpenTelemetry.Exporter.Console
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
```

2. **Configure in Program.cs:**

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter());
```

3. **Add Activity Sources:**

```csharp
private static readonly ActivitySource ActivitySource = new("AquaHub.MVC");

using var activity = ActivitySource.StartActivity("CreateTank");
activity?.SetTag("tank.name", tank.Name);
activity?.SetTag("tank.type", tank.Type);
```

4. **Existing logs automatically become trace events!**

---

## Best Practices

### 1. Use Structured Logging

✅ **Good:**

```csharp
_logger.LogInformation(
    "User {UserId} created tank {TankId} with volume {Volume}",
    userId, tankId, volume);
```

❌ **Bad:**

```csharp
_logger.LogInformation($"User {userId} created tank {tankId} with volume {volume}");
```

### 2. Choose Appropriate Log Levels

- **Debug**: Development details, not needed in production
- **Information**: Normal flow, significant events
- **Warning**: Unexpected but handled situations
- **Error**: Errors preventing specific operations
- **Critical**: Application-wide failures

### 3. Use Event IDs Consistently

```csharp
_logger.LogInformation(
    LoggingConstants.EventIds.TankCreated,  // Use constants
    "Tank created");
```

### 4. Include Context with Scopes

```csharp
using (_logger.BeginUserScope(userId))
using (_logger.BeginTankScope(tankId))
{
    // All logs include userId and tankId
}
```

### 5. Log Exceptions Properly

✅ **Good:**

```csharp
catch (Exception ex)
{
    _logger.LogError(ex,
        "Failed to create tank for user {UserId}",
        userId);
    throw;
}
```

❌ **Bad:**

```csharp
catch (Exception ex)
{
    _logger.LogError(ex.Message);  // Loses stack trace
}
```

### 6. Don't Log Sensitive Data

❌ **Never log:**

- Passwords
- API keys
- Tokens
- Credit card numbers
- Social security numbers

### 7. Use Semantic Property Names

✅ **Good:**

```csharp
_logger.LogInformation("Tank created: {TankName}", name);  // TankName is semantic
```

❌ **Bad:**

```csharp
_logger.LogInformation("Tank created: {0}", name);  // Positional, not semantic
```

### 8. Log Asynchronously

ASP.NET Core logging is already asynchronous, but avoid blocking:

```csharp
await _logger.LogExecutionTimeAsync(...);  // Prefer async patterns
```

### 9. Balance Detail vs. Noise

- **Development**: Debug level, verbose
- **Production**: Information level, key events only
- Avoid logging in tight loops
- Aggregate similar events

### 10. Include Correlation IDs

Always include RequestId, UserId, TankId, etc. for correlation:

```csharp
var requestId = context.Items["RequestId"]?.ToString();
```

---

## Examples by Scenario

### Scenario 1: User Creates a Tank

**Controller:**

```csharp
_logger.LogInformation(
    "User initiated tank creation: {TankName}",
    tank.Name);
```

**Service:**

```csharp
_logger.LogInformation(
    LoggingConstants.EventIds.TankCreated,
    "Tank created successfully with ID: {TankId}",
    tank.Id);
```

**Output:**

```
[INFO] User initiated tank creation: Main Reef | UserId: abc123 | RequestId: f7a3...
[DEBUG] Starting operation: CreateTank | UserId: abc123
[INFO] Tank created successfully with ID: 5 | UserId: abc123
[INFO] Completed operation: CreateTank in 42ms | UserId: abc123
```

### Scenario 2: Database Query

```csharp
_logger.LogDebug("Fetching tanks for user");
var tanks = await _context.Tanks.Where(...).ToListAsync();
_logger.LogInformation("Retrieved {TankCount} tanks", tanks.Count);
```

### Scenario 3: Validation Failure

```csharp
if (!ModelState.IsValid)
{
    _logger.LogValidationFailure("Tank", ModelState.ToDictionary(...));
    return View(tank);
}
```

### Scenario 4: Exception Handling

```csharp
try
{
    await _tankService.CreateTankAsync(tank, userId);
}
catch (UnauthorizedAccessException ex)
{
    _logger.LogWarning(ex,
        LoggingConstants.EventIds.UnauthorizedAccess,
        "Unauthorized tank creation attempt");
    return Forbid();
}
catch (Exception ex)
{
    _logger.LogError(ex,
        "Unexpected error creating tank");
    throw;
}
```

---

## Monitoring & Observability

### Querying Logs

With structured logging, you can query by properties:

**Find all tank creations:**

```
EventId:3010
```

**Find slow operations:**

```
Duration > 1000 AND OperationName:*
```

**Find errors for specific user:**

```
LogLevel:Error AND UserId:abc123
```

### Metrics to Track

- Request duration (p50, p95, p99)
- Error rate by endpoint
- Database query performance
- Service operation success rate
- User activity patterns

---

## Summary

AquaHub's comprehensive logging provides:

✅ **Structured, searchable logs**  
✅ **Performance tracking**  
✅ **Error correlation**  
✅ **User and tank context**  
✅ **OpenTelemetry-ready**  
✅ **Development and production configurations**  
✅ **Middleware for automatic request logging**  
✅ **Consistent event categorization**  
✅ **Extension methods for common patterns**

This foundation makes it easy to:

- Debug issues in development
- Monitor performance in production
- Transition to OpenTelemetry
- Analyze user behavior
- Detect anomalies
- Optimize database queries

---

## Next Steps

1. **Review Existing Code**: Ensure all controllers and services use the logging patterns
2. **Test Logging**: Run the application and observe logs in different scenarios
3. **Configure Log Storage**: Set up log aggregation (Seq, ELK, Application Insights)
4. **Set Up Alerts**: Configure alerts for errors and slow operations
5. **Plan OpenTelemetry**: When ready, follow the migration path above

---

**Questions or Issues?**  
Refer to the official documentation:

- [ASP.NET Core Logging](https://docs.microsoft.com/aspnet/core/fundamentals/logging/)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
