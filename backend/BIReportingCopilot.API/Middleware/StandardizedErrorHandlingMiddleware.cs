using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Constants;
using System.Text.Json;
using System.Security;

namespace BIReportingCopilot.API.Middleware;

/// <summary>
/// Middleware for standardized error handling and response formatting
/// </summary>
public class StandardizedErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<StandardizedErrorHandlingMiddleware> _logger;
    private readonly bool _isDevelopment;

    public StandardizedErrorHandlingMiddleware(
        RequestDelegate next, 
        ILogger<StandardizedErrorHandlingMiddleware> logger,
        bool isDevelopment = false)
    {
        _next = next;
        _logger = logger;
        _isDevelopment = isDevelopment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = ApplicationConstants.ContentTypes.Json;

        var (statusCode, message, errorCode) = GetErrorDetails(exception);

        var response = new
        {
            error = new
            {
                code = errorCode,
                message = message,
                details = GetSafeErrorDetails(exception),
                timestamp = DateTime.UtcNow,
                traceId = context.TraceIdentifier,
                path = context.Request.Path.Value
            }
        };

        context.Response.StatusCode = statusCode;

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static (int statusCode, string message, string errorCode) GetErrorDetails(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException => (StatusCodes.Status400BadRequest, "Required parameter is missing.", "MISSING_PARAMETER"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request parameters.", "INVALID_PARAMETERS"),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Authentication required.", "UNAUTHORIZED"),
            SecurityException => (StatusCodes.Status403Forbidden, "Access denied.", "FORBIDDEN"),
            FileNotFoundException => (StatusCodes.Status404NotFound, "Resource not found.", "NOT_FOUND"),
            NotImplementedException => (StatusCodes.Status501NotImplemented, "Feature not implemented.", "NOT_IMPLEMENTED"),
            TimeoutException => (StatusCodes.Status408RequestTimeout, "Request timeout.", "TIMEOUT"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Operation not allowed in current state.", "INVALID_OPERATION"),
            _ => (StatusCodes.Status500InternalServerError, "An internal server error occurred.", "INTERNAL_ERROR")
        };
    }

    private string GetSafeErrorDetails(Exception exception)
    {
        // In production, don't expose sensitive error details
        if (_isDevelopment)
        {
            return exception.Message;
        }

        return exception switch
        {
            ArgumentException or ArgumentNullException => exception.Message,
            UnauthorizedAccessException or SecurityException => "Access denied",
            FileNotFoundException => "Resource not found",
            _ => "An error occurred while processing your request"
        };
    }
}
