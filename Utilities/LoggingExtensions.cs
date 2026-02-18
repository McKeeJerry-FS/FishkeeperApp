using System.Diagnostics;

namespace AquaHub.MVC.Utilities;

/// <summary>
/// Extension methods for enhanced logging capabilities
/// Provides consistent patterns for timing operations and structured logging
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Logs the execution time of an operation with structured data
    /// </summary>
    public static async Task<T> LogExecutionTimeAsync<T>(
        this ILogger logger,
        string operationName,
        Func<Task<T>> operation,
        int eventId,
        Dictionary<string, object>? additionalProperties = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var properties = new Dictionary<string, object>
        {
            [LoggingConstants.Properties.OperationName] = operationName
        };

        if (additionalProperties != null)
        {
            foreach (var prop in additionalProperties)
            {
                properties[prop.Key] = prop.Value;
            }
        }

        using (logger.BeginScope(properties))
        {
            logger.LogDebug(eventId, "Starting operation: {OperationName}", operationName);

            try
            {
                var result = await operation();
                stopwatch.Stop();

                logger.LogInformation(eventId + 1,
                    "Completed operation: {OperationName} in {Duration}ms",
                    operationName,
                    stopwatch.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                logger.LogError(eventId + 2, ex,
                    "Failed operation: {OperationName} after {Duration}ms - {ExceptionType}: {Message}",
                    operationName,
                    stopwatch.ElapsedMilliseconds,
                    ex.GetType().Name,
                    ex.Message);

                throw;
            }
        }
    }

    /// <summary>
    /// Logs the execution time of a void operation
    /// </summary>
    public static async Task LogExecutionTimeAsync(
        this ILogger logger,
        string operationName,
        Func<Task> operation,
        int eventId,
        Dictionary<string, object>? additionalProperties = null)
    {
        await LogExecutionTimeAsync<object?>(logger, operationName, async () =>
        {
            await operation();
            return null;
        }, eventId, additionalProperties);
    }

    /// <summary>
    /// Creates a user context scope for logging
    /// </summary>
    public static IDisposable BeginUserScope(this ILogger logger, string userId, string? userName = null)
    {
        var properties = new Dictionary<string, object>
        {
            [LoggingConstants.Properties.UserId] = userId
        };

        if (!string.IsNullOrEmpty(userName))
        {
            properties["UserName"] = userName;
        }

        return logger.BeginScope(properties);
    }

    /// <summary>
    /// Creates a tank context scope for logging
    /// </summary>
    public static IDisposable BeginTankScope(this ILogger logger, int tankId, string? tankName = null)
    {
        var properties = new Dictionary<string, object>
        {
            [LoggingConstants.Properties.TankId] = tankId
        };

        if (!string.IsNullOrEmpty(tankName))
        {
            properties["TankName"] = tankName;
        }

        return logger.BeginScope(properties);
    }

    /// <summary>
    /// Logs database operation metrics
    /// </summary>
    public static void LogDatabaseMetrics(
        this ILogger logger,
        string operation,
        long durationMs,
        int? rowsAffected = null)
    {
        var properties = new Dictionary<string, object>
        {
            [LoggingConstants.Properties.OperationName] = operation,
            [LoggingConstants.Properties.Duration] = durationMs
        };

        if (rowsAffected.HasValue)
        {
            properties["RowsAffected"] = rowsAffected.Value;
        }

        using (logger.BeginScope(properties))
        {
            logger.LogInformation(
                LoggingConstants.EventIds.DatabaseQueryCompleted,
                "Database operation '{Operation}' completed in {Duration}ms affecting {RowsAffected} rows",
                operation,
                durationMs,
                rowsAffected ?? 0);
        }
    }

    /// <summary>
    /// Logs validation failures with structured data
    /// </summary>
    public static void LogValidationFailure(
        this ILogger logger,
        string entityType,
        Dictionary<string, string[]> errors)
    {
        using (logger.BeginScope(new Dictionary<string, object>
        {
            [LoggingConstants.Properties.EntityType] = entityType,
            ["ValidationErrors"] = errors
        }))
        {
            logger.LogWarning(
                LoggingConstants.EventIds.ValidationFailed,
                "Validation failed for {EntityType}: {ErrorCount} errors",
                entityType,
                errors.Count);
        }
    }
}
