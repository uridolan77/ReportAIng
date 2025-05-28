using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;
using FluentValidation;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Monitoring;

namespace BIReportingCopilot.API.Middleware;

/// <summary>
/// Global exception handler middleware for consistent error responses
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IMetricsCollector _metricsCollector;
    private readonly IWebHostEnvironment _environment;
    private readonly JsonSerializerOptions _jsonOptions;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IMetricsCollector metricsCollector,
        IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _metricsCollector = metricsCollector;
        _environment = environment;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var requestId = context.TraceIdentifier;
        var userId = context.User?.Identity?.Name ?? "Anonymous";
        var endpoint = $"{context.Request.Method} {context.Request.Path}";

        // Log the exception
        _logger.LogError(exception,
            "Unhandled exception occurred. RequestId: {RequestId}, UserId: {UserId}, Endpoint: {Endpoint}",
            requestId, userId, endpoint);

        // Record metrics
        _metricsCollector.IncrementCounter("api_errors_total", new()
        {
            { "exception_type", exception.GetType().Name },
            { "endpoint", endpoint },
            { "user_id", userId }
        });

        // Create error response
        var errorResponse = CreateErrorResponse(exception, requestId);

        // Set response details
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = GetStatusCode(exception);

        // Add correlation headers (only if not already present)
        if (!context.Response.Headers.ContainsKey("X-Request-ID"))
        {
            context.Response.Headers.Add("X-Request-ID", requestId);
        }
        context.Response.Headers.Add("X-Error-ID", errorResponse.Error?.ErrorId ?? Guid.NewGuid().ToString());

        // Serialize and write response
        var jsonResponse = JsonSerializer.Serialize(errorResponse, _jsonOptions);
        await context.Response.WriteAsync(jsonResponse);
    }

    private ApiResponse<object> CreateErrorResponse(Exception exception, string requestId)
    {
        var response = new ApiResponse<object>
        {
            Success = false,
            RequestId = requestId,
            Timestamp = DateTime.UtcNow
        };

        response.Error = exception switch
        {
            ValidationException validationEx => CreateValidationError(validationEx),
            ArgumentException argumentEx => CreateArgumentError(argumentEx),
            UnauthorizedAccessException => CreateUnauthorizedError(),
            InvalidOperationException invalidOpEx => CreateInvalidOperationError(invalidOpEx),
            TimeoutException timeoutEx => CreateTimeoutError(timeoutEx),
            NotSupportedException notSupportedEx => CreateNotSupportedError(notSupportedEx),
            KeyNotFoundException keyNotFoundEx => CreateNotFoundError(keyNotFoundEx),
            HttpRequestException httpEx => CreateHttpError(httpEx),
            TaskCanceledException canceledEx => CreateCanceledError(canceledEx),
            _ => CreateGenericError(exception)
        };

        return response;
    }

    private ApiError CreateValidationError(ValidationException validationException)
    {
        var validationErrors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return new ApiError
        {
            Code = ApiErrorCodes.VALIDATION_ERROR,
            Message = "One or more validation errors occurred",
            Details = validationErrors,
            HelpUrl = "https://docs.bireportingcopilot.com/errors/validation"
        };
    }

    private ApiError CreateArgumentError(ArgumentException argumentException)
    {
        return new ApiError
        {
            Code = ApiErrorCodes.INVALID_INPUT,
            Message = argumentException.Message,
            Details = new { ParameterName = argumentException.ParamName },
            HelpUrl = "https://docs.bireportingcopilot.com/errors/invalid-input"
        };
    }

    private ApiError CreateUnauthorizedError()
    {
        return new ApiError
        {
            Code = ApiErrorCodes.UNAUTHORIZED,
            Message = "Authentication is required to access this resource",
            HelpUrl = "https://docs.bireportingcopilot.com/authentication"
        };
    }

    private ApiError CreateInvalidOperationError(InvalidOperationException invalidOperationException)
    {
        return new ApiError
        {
            Code = ApiErrorCodes.INVALID_STATE,
            Message = invalidOperationException.Message,
            HelpUrl = "https://docs.bireportingcopilot.com/errors/invalid-state"
        };
    }

    private ApiError CreateTimeoutError(TimeoutException timeoutException)
    {
        return new ApiError
        {
            Code = ApiErrorCodes.TIMEOUT,
            Message = "The operation timed out. Please try again or contact support if the problem persists.",
            Details = new { OriginalMessage = timeoutException.Message },
            HelpUrl = "https://docs.bireportingcopilot.com/errors/timeout"
        };
    }

    private ApiError CreateNotSupportedError(NotSupportedException notSupportedException)
    {
        return new ApiError
        {
            Code = ApiErrorCodes.OPERATION_NOT_ALLOWED,
            Message = notSupportedException.Message,
            HelpUrl = "https://docs.bireportingcopilot.com/errors/not-supported"
        };
    }

    private ApiError CreateNotFoundError(KeyNotFoundException keyNotFoundException)
    {
        return new ApiError
        {
            Code = ApiErrorCodes.NOT_FOUND,
            Message = "The requested resource was not found",
            Details = new { OriginalMessage = keyNotFoundException.Message },
            HelpUrl = "https://docs.bireportingcopilot.com/errors/not-found"
        };
    }

    private ApiError CreateHttpError(HttpRequestException httpException)
    {
        return new ApiError
        {
            Code = ApiErrorCodes.SERVICE_UNAVAILABLE,
            Message = "An external service is currently unavailable. Please try again later.",
            Details = _environment.IsDevelopment() ? new { OriginalMessage = httpException.Message } : null,
            HelpUrl = "https://docs.bireportingcopilot.com/errors/service-unavailable"
        };
    }

    private ApiError CreateCanceledError(TaskCanceledException canceledException)
    {
        var isTimeout = canceledException.InnerException is TimeoutException;

        return new ApiError
        {
            Code = isTimeout ? ApiErrorCodes.TIMEOUT : ApiErrorCodes.OPERATION_NOT_ALLOWED,
            Message = isTimeout
                ? "The operation timed out. Please try again or contact support if the problem persists."
                : "The operation was canceled",
            Details = _environment.IsDevelopment() ? new { OriginalMessage = canceledException.Message } : null,
            HelpUrl = isTimeout
                ? "https://docs.bireportingcopilot.com/errors/timeout"
                : "https://docs.bireportingcopilot.com/errors/canceled"
        };
    }

    private ApiError CreateGenericError(Exception exception)
    {
        var includeDetails = _environment.IsDevelopment() || _environment.IsStaging();

        return new ApiError
        {
            Code = ApiErrorCodes.INTERNAL_ERROR,
            Message = "An unexpected error occurred. Please try again or contact support if the problem persists.",
            Details = includeDetails ? new
            {
                ExceptionType = exception.GetType().Name,
                OriginalMessage = exception.Message,
                StackTrace = exception.StackTrace
            } : null,
            HelpUrl = "https://docs.bireportingcopilot.com/errors/internal-error"
        };
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            ValidationException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            NotSupportedException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            TimeoutException => (int)HttpStatusCode.RequestTimeout,
            TaskCanceledException => (int)HttpStatusCode.RequestTimeout,
            HttpRequestException => (int)HttpStatusCode.BadGateway,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }
}

/// <summary>
/// Extension methods for registering the global exception handler
/// </summary>
public static class GlobalExceptionHandlerExtensions
{
    /// <summary>
    /// Add global exception handling to the application pipeline
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}


