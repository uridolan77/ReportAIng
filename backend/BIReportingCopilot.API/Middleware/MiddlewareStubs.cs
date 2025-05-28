using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Constants;
using System.Diagnostics;
using System.Text.Json;
using System.Security;

namespace BIReportingCopilot.API.Middleware;

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

        // Add request ID to response headers using constants
        context.Response.Headers[ApplicationConstants.Headers.RequestId] = requestId;

        _logger.LogInformation("Request {RequestId} started: {Method} {Path}",
            requestId, context.Request.Method, context.Request.Path);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Request {RequestId} completed in {ElapsedMs}ms with status {StatusCode}",
                requestId, stopwatch.ElapsedMilliseconds, context.Response.StatusCode);
        }
    }
}

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
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

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
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

    private static string GetSafeErrorDetails(Exception exception)
    {
        // In production, don't expose sensitive error details
        #if DEBUG
            return exception.Message;
        #else
            return exception switch
            {
                ArgumentException or ArgumentNullException => exception.Message,
                UnauthorizedAccessException or SecurityException => "Access denied",
                FileNotFoundException => "Resource not found",
                _ => "An error occurred while processing your request"
            };
        #endif
    }
}

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ICacheService cacheService, IMemoryCache fallbackCache)
    {
        // Skip rate limiting for health endpoints
        if (ShouldSkipRateLimit(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var clientId = GetClientIdentifier(context);

        if (await IsRateLimitedAsync(clientId, cacheService, fallbackCache))
        {
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["Retry-After"] = "60";
            context.Response.Headers[ApplicationConstants.Headers.RateLimitRemaining] = "0";
            context.Response.Headers[ApplicationConstants.Headers.RateLimitReset] = DateTimeOffset.UtcNow.AddMinutes(1).ToUnixTimeSeconds().ToString();

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = new
                {
                    message = "Rate limit exceeded. Please try again later.",
                    timestamp = DateTime.UtcNow,
                    retryAfter = 60
                }
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));

            return;
        }

        await _next(context);
    }

    private static bool ShouldSkipRateLimit(PathString path)
    {
        var skipPaths = new[] { "/health", "/swagger", "/hangfire" };
        return skipPaths.Any(skip => path.StartsWithSegments(skip));
    }

    private static string GetClientIdentifier(HttpContext context)
    {
        // Try to get user ID from claims first, fallback to IP address
        var userId = context.User?.FindFirst("sub")?.Value ??
                    context.User?.FindFirst("user_id")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        // Use IP address as fallback
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return $"ip:{ipAddress}";
    }

    private async Task<bool> IsRateLimitedAsync(string clientId, ICacheService cacheService, IMemoryCache fallbackCache)
    {
        const int maxRequests = 100;
        const int windowMinutes = 1;

        var cacheKey = ApplicationConstants.CacheKeys.RateLimitKey(clientId);

        try
        {
            // Try to use distributed cache first
            var rateLimitData = await cacheService.GetAsync<RateLimitData>(cacheKey);

            if (rateLimitData == null)
            {
                // First request in the window
                await cacheService.SetAsync(cacheKey, new RateLimitData { Count = 1 }, TimeSpan.FromMinutes(windowMinutes));
                return false;
            }

            if (rateLimitData.Count >= maxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientId}. Count: {Count}", clientId, rateLimitData.Count);
                return true;
            }

            // Increment counter
            rateLimitData.Count++;
            await cacheService.SetAsync(cacheKey, rateLimitData, TimeSpan.FromMinutes(windowMinutes));
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check rate limit using distributed cache, falling back to memory cache");

            // Fallback to memory cache
            return IsRateLimitedMemoryFallback(clientId, maxRequests, windowMinutes, fallbackCache);
        }
    }

    private bool IsRateLimitedMemoryFallback(string clientId, int maxRequests, int windowMinutes, IMemoryCache fallbackCache)
    {
        var cacheKey = $"rate_limit_fallback:{clientId}";
        var now = DateTime.UtcNow;

        if (fallbackCache.TryGetValue(cacheKey, out var cachedData) && cachedData is (DateTime lastRequest, int count))
        {
            // Reset if window expired
            if (now - lastRequest > TimeSpan.FromMinutes(windowMinutes))
            {
                fallbackCache.Set(cacheKey, (now, 1), TimeSpan.FromMinutes(windowMinutes));
                return false;
            }

            if (count >= maxRequests)
            {
                _logger.LogWarning("Rate limit exceeded for client {ClientId} (memory fallback). Count: {Count}", clientId, count);
                return true;
            }

            // Increment counter
            fallbackCache.Set(cacheKey, (lastRequest, count + 1), TimeSpan.FromMinutes(windowMinutes));
            return false;
        }

        // First request
        fallbackCache.Set(cacheKey, (now, 1), TimeSpan.FromMinutes(windowMinutes));
        return false;
    }
}

public class RateLimitData
{
    public int Count { get; set; }
    public DateTime LastRequest { get; set; } = DateTime.UtcNow;
}
