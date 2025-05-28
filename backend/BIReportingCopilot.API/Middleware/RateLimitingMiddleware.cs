using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Constants;
using System.Text.Json;

namespace BIReportingCopilot.API.Middleware;

/// <summary>
/// Middleware for rate limiting API requests with distributed cache support
/// </summary>
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

/// <summary>
/// Data structure for rate limiting information
/// </summary>
public class RateLimitData
{
    public int Count { get; set; }
    public DateTime LastRequest { get; set; } = DateTime.UtcNow;
}
