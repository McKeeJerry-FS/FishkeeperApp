using System.Net;
using System.Text.Json;

namespace AquaHub.MVC.Middleware;

/// <summary>
/// Global exception handling middleware for consistent error responses and logging
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var requestId = context.Items["RequestId"]?.ToString() ?? Guid.NewGuid().ToString();

        // Determine status code based on exception type
        var statusCode = exception switch
        {
            UnauthorizedAccessException => HttpStatusCode.Forbidden,
            ArgumentException => HttpStatusCode.BadRequest,
            KeyNotFoundException => HttpStatusCode.NotFound,
            _ => HttpStatusCode.InternalServerError
        };

        // Log with appropriate level and detailed information
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["RequestId"] = requestId,
            ["ExceptionType"] = exception.GetType().Name,
            ["StatusCode"] = (int)statusCode,
            ["Path"] = context.Request.Path,
            ["Method"] = context.Request.Method
        }))
        {
            _logger.LogError(exception,
                "Exception occurred: {ExceptionType} - {Message} | Path: {Path} | Method: {Method}",
                exception.GetType().Name,
                exception.Message,
                context.Request.Path,
                context.Request.Method);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        // Prepare error response
        var response = new
        {
            RequestId = requestId,
            StatusCode = (int)statusCode,
            Message = _environment.IsDevelopment() ? exception.Message : "An error occurred processing your request.",
            Details = _environment.IsDevelopment() ? exception.ToString() : null
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = _environment.IsDevelopment()
        });

        await context.Response.WriteAsync(json);
    }
}
