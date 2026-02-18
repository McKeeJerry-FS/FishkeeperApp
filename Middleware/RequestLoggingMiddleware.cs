using System.Diagnostics;
using System.Text;

namespace AquaHub.MVC.Middleware;

/// <summary>
/// Middleware for detailed HTTP request/response logging
/// Captures request details, timing, and response status for observability
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();

        // Add request ID to context for correlation
        context.Items["RequestId"] = requestId;

        // Log request start
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["Method"] = context.Request.Method,
            ["Path"] = context.Request.Path,
            ["QueryString"] = context.Request.QueryString.ToString(),
            ["UserAgent"] = context.Request.Headers["User-Agent"].ToString()
        }))
        {
            _logger.LogInformation(
                "HTTP Request started: {Method} {Path}{QueryString}",
                context.Request.Method,
                context.Request.Path,
                context.Request.QueryString);

            try
            {
                // Call the next middleware in the pipeline
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unhandled exception during request processing: {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
                throw;
            }
            finally
            {
                stopwatch.Stop();

                // Log request completion
                var logLevel = context.Response.StatusCode >= 500 ? LogLevel.Error :
                              context.Response.StatusCode >= 400 ? LogLevel.Warning :
                              LogLevel.Information;

                _logger.Log(logLevel,
                    "HTTP Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
