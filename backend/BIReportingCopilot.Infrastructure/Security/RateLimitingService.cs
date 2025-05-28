using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.Security;

public interface IRateLimitingService
{
    Task<RateLimitResult> CheckRateLimitAsync(string userId, string endpoint);
    Task<RateLimitResult> CheckRateLimitAsync(string key, int maxRequests, TimeSpan window);
}

public class RateLimitResult
{
    public bool IsAllowed { get; set; }
    public int RequestsRemaining { get; set; }
    public TimeSpan RetryAfter { get; set; }
    public string? Message { get; set; }
}

public class RateLimitingService : IRateLimitingService
{
    private readonly IDistributedCache _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<RateLimitingService> _logger;

    // Default rate limits
    private readonly Dictionary<string, (int maxRequests, TimeSpan window)> _defaultLimits = new()
    {
        { "query", (100, TimeSpan.FromHours(1)) },
        { "history", (200, TimeSpan.FromHours(1)) },
        { "feedback", (50, TimeSpan.FromHours(1)) },
        { "suggestions", (300, TimeSpan.FromHours(1)) }
    };

    public RateLimitingService(
        IDistributedCache cache,
        IConfiguration configuration,
        ILogger<RateLimitingService> logger)
    {
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<RateLimitResult> CheckRateLimitAsync(string userId, string endpoint)
    {
        // Get rate limit configuration for this endpoint
        var (maxRequests, window) = GetRateLimitForEndpoint(endpoint);

        var key = $"rate_limit:{userId}:{endpoint}";
        return await CheckRateLimitAsync(key, maxRequests, window);
    }

    public async Task<RateLimitResult> CheckRateLimitAsync(string key, int maxRequests, TimeSpan window)
    {
        try
        {
            var cacheKey = $"rate_limit:{key}";
            var rateLimitData = await GetRateLimitDataAsync(cacheKey);

            var now = DateTime.UtcNow;
            var windowStart = now.Subtract(window);

            // Clean up old requests
            rateLimitData.Requests = rateLimitData.Requests
                .Where(r => r > windowStart)
                .ToList();

            // Check if limit is exceeded
            if (rateLimitData.Requests.Count >= maxRequests)
            {
                var oldestRequest = rateLimitData.Requests.Min();
                var retryAfter = oldestRequest.Add(window).Subtract(now);

                _logger.LogWarning("Rate limit exceeded for key {Key}: {Count}/{Max} requests",
                    key, rateLimitData.Requests.Count, maxRequests);

                return new RateLimitResult
                {
                    IsAllowed = false,
                    RequestsRemaining = 0,
                    RetryAfter = retryAfter > TimeSpan.Zero ? retryAfter : TimeSpan.Zero,
                    Message = $"Rate limit exceeded. Try again in {retryAfter.TotalSeconds:F0} seconds."
                };
            }

            // Add current request
            rateLimitData.Requests.Add(now);
            await SaveRateLimitDataAsync(cacheKey, rateLimitData, window);

            var remaining = maxRequests - rateLimitData.Requests.Count;

            _logger.LogDebug("Rate limit check passed for key {Key}: {Count}/{Max} requests",
                key, rateLimitData.Requests.Count, maxRequests);

            return new RateLimitResult
            {
                IsAllowed = true,
                RequestsRemaining = remaining,
                RetryAfter = TimeSpan.Zero
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for key {Key}", key);

            // In case of error, allow the request but log the issue
            return new RateLimitResult
            {
                IsAllowed = true,
                RequestsRemaining = maxRequests,
                RetryAfter = TimeSpan.Zero,
                Message = "Rate limiting temporarily unavailable"
            };
        }
    }

    private (int maxRequests, TimeSpan window) GetRateLimitForEndpoint(string endpoint)
    {
        // Try to get from configuration first
        var configKey = $"RateLimit:{endpoint}";
        var maxRequests = _configuration.GetValue<int?>($"{configKey}:MaxRequests");
        var windowMinutes = _configuration.GetValue<int?>($"{configKey}:WindowMinutes");

        if (maxRequests.HasValue && windowMinutes.HasValue)
        {
            return (maxRequests.Value, TimeSpan.FromMinutes(windowMinutes.Value));
        }

        // Fall back to defaults
        if (_defaultLimits.TryGetValue(endpoint.ToLowerInvariant(), out var defaultLimit))
        {
            return defaultLimit;
        }

        // Ultimate fallback
        return (100, TimeSpan.FromHours(1));
    }

    private async Task<RateLimitData> GetRateLimitDataAsync(string cacheKey)
    {
        var cachedData = await _cache.GetStringAsync(cacheKey);

        if (string.IsNullOrEmpty(cachedData))
        {
            return new RateLimitData();
        }

        try
        {
            return JsonSerializer.Deserialize<RateLimitData>(cachedData) ?? new RateLimitData();
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize rate limit data for key {Key}", cacheKey);
            return new RateLimitData();
        }
    }

    private async Task SaveRateLimitDataAsync(string cacheKey, RateLimitData data, TimeSpan expiry)
    {
        try
        {
            var serialized = JsonSerializer.Serialize(data);
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry.Add(TimeSpan.FromMinutes(5)) // Add buffer
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save rate limit data for key {Key}", cacheKey);
        }
    }
}

public class RateLimitData
{
    public List<DateTime> Requests { get; set; } = new();
}

// Middleware for applying rate limiting
public class UserBasedRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimitingService _rateLimitingService;
    private readonly ILogger<UserBasedRateLimitingMiddleware> _logger;

    public UserBasedRateLimitingMiddleware(
        RequestDelegate next,
        IRateLimitingService rateLimitingService,
        ILogger<UserBasedRateLimitingMiddleware> logger)
    {
        _next = next;
        _rateLimitingService = rateLimitingService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip rate limiting for certain paths
        if (ShouldSkipRateLimit(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var userId = GetUserId(context);
        var endpoint = GetEndpointName(context.Request.Path);

        var result = await _rateLimitingService.CheckRateLimitAsync(userId, endpoint);

        if (!result.IsAllowed)
        {
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers["Retry-After"] = result.RetryAfter.TotalSeconds.ToString("F0");

            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                error = "Rate limit exceeded",
                message = result.Message,
                retryAfter = result.RetryAfter.TotalSeconds
            }));

            return;
        }

        // Add rate limit headers
        context.Response.Headers["X-RateLimit-Remaining"] = result.RequestsRemaining.ToString();

        await _next(context);
    }

    private bool ShouldSkipRateLimit(PathString path)
    {
        var skipPaths = new[] { "/health", "/swagger", "/hangfire" };
        return skipPaths.Any(skip => path.StartsWithSegments(skip));
    }

    private string GetUserId(HttpContext context)
    {
        return context.User?.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
    }

    private string GetEndpointName(PathString path)
    {
        // Extract endpoint name from path
        var segments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return segments?.LastOrDefault() ?? "unknown";
    }
}
