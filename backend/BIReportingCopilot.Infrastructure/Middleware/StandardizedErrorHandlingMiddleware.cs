using BIReportingCopilot.Core.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.Middleware;

/// <summary>
/// Middleware for standardized error handling using RFC 7807 ProblemDetails
/// </summary>
public class StandardizedErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<StandardizedErrorHandlingMiddleware> _logger;
    private readonly bool _includeStackTrace;

    public StandardizedErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<StandardizedErrorHandlingMiddleware> logger,
        bool includeStackTrace = false)
    {
        _next = next;
        _logger = logger;
        _includeStackTrace = includeStackTrace;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred while processing the request");
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var problemDetails = CreateProblemDetails(context, exception);

        context.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = ApplicationConstants.ContentTypes.Json;

        // Add correlation ID if available
        if (context.Request.Headers.ContainsKey(ApplicationConstants.Headers.CorrelationId))
        {
            context.Response.Headers[ApplicationConstants.Headers.CorrelationId] =
                context.Request.Headers[ApplicationConstants.Headers.CorrelationId];
        }

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await context.Response.WriteAsync(json);
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var problemDetails = new ProblemDetails
        {
            Instance = context.Request.Path,
            Detail = GetUserFriendlyMessage(exception),
            Type = GetProblemType(exception)
        };

        switch (exception)
        {
            case UnauthorizedAccessException:
                problemDetails.Status = (int)HttpStatusCode.Unauthorized;
                problemDetails.Title = "Unauthorized";
                problemDetails.Detail = "Authentication is required to access this resource";
                break;

            case SecurityException:
                problemDetails.Status = (int)HttpStatusCode.Forbidden;
                problemDetails.Title = "Forbidden";
                problemDetails.Detail = "You do not have permission to access this resource";
                break;

            case ArgumentException argEx:
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Invalid Request";
                problemDetails.Detail = argEx.Message;
                break;

            case InvalidOperationException:
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Invalid Operation";
                break;

            case TimeoutException:
                problemDetails.Status = (int)HttpStatusCode.RequestTimeout;
                problemDetails.Title = "Request Timeout";
                problemDetails.Detail = "The request took too long to process";
                break;

            case SqlInjectionException:
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Security Violation";
                problemDetails.Detail = "Potential SQL injection detected in query";
                break;

            case QueryValidationException queryEx:
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Query Validation Error";
                problemDetails.Detail = queryEx.Message;
                break;

            case DatabaseConnectionException:
                problemDetails.Status = (int)HttpStatusCode.ServiceUnavailable;
                problemDetails.Title = "Database Unavailable";
                problemDetails.Detail = "Unable to connect to the database";
                break;

            case RateLimitExceededException rateLimitEx:
                problemDetails.Status = (int)HttpStatusCode.TooManyRequests;
                problemDetails.Title = "Rate Limit Exceeded";
                problemDetails.Detail = rateLimitEx.Message;
                // Add rate limit headers
                if (context.Response.Headers.ContainsKey(ApplicationConstants.Headers.RateLimitReset))
                {
                    problemDetails.Extensions["retryAfter"] = context.Response.Headers[ApplicationConstants.Headers.RateLimitReset];
                }
                break;

            case UserNotFoundException:
                problemDetails.Status = (int)HttpStatusCode.NotFound;
                problemDetails.Title = "User Not Found";
                problemDetails.Detail = "The specified user could not be found";
                break;

            case ValidationException validationEx:
                problemDetails.Status = (int)HttpStatusCode.BadRequest;
                problemDetails.Title = "Validation Error";
                problemDetails.Detail = validationEx.Message;
                if (validationEx.Errors?.Any() == true)
                {
                    problemDetails.Extensions["errors"] = validationEx.Errors;
                }
                break;

            default:
                problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                problemDetails.Title = "Internal Server Error";
                problemDetails.Detail = "An unexpected error occurred while processing your request";
                break;
        }

        // Add additional context in development
        if (_includeStackTrace)
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
            if (exception.InnerException != null)
            {
                problemDetails.Extensions["innerException"] = new
                {
                    Type = exception.InnerException.GetType().Name,
                    Message = exception.InnerException.Message
                };
            }
        }

        // Add correlation ID if available
        if (context.Request.Headers.ContainsKey(ApplicationConstants.Headers.CorrelationId))
        {
            problemDetails.Extensions["correlationId"] = context.Request.Headers[ApplicationConstants.Headers.CorrelationId].ToString();
        }

        // Add timestamp
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        return problemDetails;
    }

    private static string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => "You are not authorized to perform this action",
            SecurityException => "Access denied due to security restrictions",
            TimeoutException => "The operation timed out. Please try again",
            ArgumentException => "Invalid input provided",
            SqlInjectionException => "Invalid query detected for security reasons",
            DatabaseConnectionException => "Database is temporarily unavailable",
            RateLimitExceededException => "Too many requests. Please slow down",
            UserNotFoundException => "User not found",
            ValidationException validationEx => validationEx.Message,
            QueryValidationException queryEx => queryEx.Message,
            _ => "An unexpected error occurred"
        };
    }

    private static string GetProblemType(Exception exception)
    {
        var baseUri = "https://tools.ietf.org/html/rfc7231#section-6.";

        return exception switch
        {
            UnauthorizedAccessException => $"{baseUri}5.1",
            SecurityException => $"{baseUri}5.3",
            ArgumentException => $"{baseUri}5.1",
            InvalidOperationException => $"{baseUri}5.1",
            TimeoutException => $"{baseUri}5.8",
            RateLimitExceededException => "https://tools.ietf.org/html/rfc6585#section-4",
            UserNotFoundException => $"{baseUri}5.4",
            ValidationException => $"{baseUri}5.1",
            QueryValidationException => $"{baseUri}5.1",
            SqlInjectionException => $"{baseUri}5.1",
            DatabaseConnectionException => $"{baseUri}5.3",
            _ => $"{baseUri}6.1"
        };
    }
}

// Custom exception classes for better error handling
public class SqlInjectionException : Exception
{
    public SqlInjectionException(string message) : base(message) { }
    public SqlInjectionException(string message, Exception innerException) : base(message, innerException) { }
}

public class QueryValidationException : Exception
{
    public QueryValidationException(string message) : base(message) { }
    public QueryValidationException(string message, Exception innerException) : base(message, innerException) { }
}

public class DatabaseConnectionException : Exception
{
    public DatabaseConnectionException(string message) : base(message) { }
    public DatabaseConnectionException(string message, Exception innerException) : base(message, innerException) { }
}

public class RateLimitExceededException : Exception
{
    public RateLimitExceededException(string message) : base(message) { }
    public RateLimitExceededException(string message, Exception innerException) : base(message, innerException) { }
}

public class UserNotFoundException : Exception
{
    public UserNotFoundException(string message) : base(message) { }
    public UserNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}

public class ValidationException : Exception
{
    public Dictionary<string, string[]>? Errors { get; }

    public ValidationException(string message) : base(message) { }
    public ValidationException(string message, Dictionary<string, string[]> errors) : base(message)
    {
        Errors = errors;
    }
    public ValidationException(string message, Exception innerException) : base(message, innerException) { }
}

public class SecurityException : Exception
{
    public SecurityException(string message) : base(message) { }
    public SecurityException(string message, Exception innerException) : base(message, innerException) { }
}
