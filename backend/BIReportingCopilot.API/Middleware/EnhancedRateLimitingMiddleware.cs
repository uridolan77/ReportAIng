using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using BIReportingCopilot.Infrastructure.Security;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Exceptions;

namespace BIReportingCopilot.API.Middleware;

/// <summary>
/// Enhanced rate limiting middleware with distributed Redis support and multiple policies
/// </summary>
public class EnhancedRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimitingService _rateLimitingService;
    private readonly ILogger<EnhancedRateLimitingMiddleware> _logger;
    private readonly RateLimitingConfiguration _config;
    private readonly JsonSerializerOptions _jsonOptions;

    public EnhancedRateLimitingMiddleware(
        RequestDelegate next,
        IRateLimitingService rateLimitingService,
        ILogger<EnhancedRateLimitingMiddleware> logger,
        IOptions<RateLimitingConfiguration> config)
    {
        _next = next;
        _rateLimitingService = rateLimitingService;
        _logger = logger;
        _config = config.Value;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip rate limiting if disabled
        if (!_config.EnableRateLimiting)
        {
            await _next(context);
            return;
        }

        // Skip rate limiting for health checks and internal endpoints
        if (ShouldSkipRateLimiting(context))
        {
            await _next(context);
            return;
        }

        var clientIdentifier = GetClientIdentifier(context);
        var applicablePolicies = GetApplicablePolicies(context);

        if (!applicablePolicies.Any())
        {
            await _next(context);
            return;
        }

        try
        {
            // Check all applicable rate limit policies
            var rateLimitResults = await _rateLimitingService.CheckMultipleRateLimitsAsync(
                clientIdentifier,
                applicablePolicies,
                context.RequestAborted);

            // Find the most restrictive violated policy
            var violatedPolicy = rateLimitResults.Values.FirstOrDefault(r => !r.IsAllowed);

            if (violatedPolicy != null)
            {
                await HandleRateLimitExceededAsync(context, violatedPolicy, clientIdentifier);
                return;
            }

            // Add rate limit headers for successful requests
            AddRateLimitHeaders(context, rateLimitResults.Values);

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in rate limiting middleware for client {ClientId}", clientIdentifier);

            // Fail open if configured to do so
            if (_config.FailOpen)
            {
                await _next(context);
            }
            else
            {
                await HandleRateLimitErrorAsync(context, ex);
            }
        }
    }

    private bool ShouldSkipRateLimiting(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

        // Skip health checks and monitoring endpoints
        var skipPaths = new[]
        {
            "/health",
            "/health/ready",
            "/health/live",
            "/metrics",
            "/swagger",
            "/favicon.ico",
            "/hangfire"
        };

        return skipPaths.Any(skipPath => path.StartsWith(skipPath));
    }

    private string GetClientIdentifier(HttpContext context)
    {
        // Priority: User ID > API Key > IP Address
        var userId = context.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userId))
        {
            return $"user:{userId}";
        }

        var apiKey = context.Request.Headers["X-API-Key"].FirstOrDefault();
        if (!string.IsNullOrEmpty(apiKey))
        {
            return $"apikey:{apiKey}";
        }

        // Get real IP address considering proxies
        var ipAddress = GetRealIpAddress(context);
        return $"ip:{ipAddress}";
    }

    private string GetRealIpAddress(HttpContext context)
    {
        // Check for forwarded IP headers (for load balancers/proxies)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            var ips = forwardedFor.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (ips.Length > 0)
            {
                return ips[0].Trim();
            }
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private List<RateLimitPolicy> GetApplicablePolicies(HttpContext context)
    {
        var applicablePolicies = new List<RateLimitPolicy>();
        var userRoles = context.User?.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? new List<string>();

        var endpoint = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        var method = context.Request.Method.ToUpperInvariant();

        foreach (var policy in _config.Policies)
        {
            // Check if policy applies to this request
            if (PolicyApplies(policy, userRoles, endpoint, method))
            {
                applicablePolicies.Add(policy);
            }
        }

        // If no specific policies apply, use default policies
        if (!applicablePolicies.Any())
        {
            applicablePolicies.AddRange(GetDefaultPolicies());
        }

        return applicablePolicies;
    }

    private bool PolicyApplies(RateLimitPolicy policy, List<string> userRoles, string endpoint, string method)
    {
        if (!policy.AppliesTo.Any())
        {
            return true; // Policy applies to all if no specific criteria
        }

        foreach (var criteria in policy.AppliesTo)
        {
            if (criteria.StartsWith("role:", StringComparison.OrdinalIgnoreCase))
            {
                var requiredRole = criteria.Substring(5);
                if (userRoles.Contains(requiredRole, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            else if (criteria.StartsWith("endpoint:", StringComparison.OrdinalIgnoreCase))
            {
                var requiredEndpoint = criteria.Substring(9);
                if (endpoint.Contains(requiredEndpoint, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            else if (criteria.StartsWith("method:", StringComparison.OrdinalIgnoreCase))
            {
                var requiredMethod = criteria.Substring(7);
                if (method.Equals(requiredMethod, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private List<RateLimitPolicy> GetDefaultPolicies()
    {
        return new List<RateLimitPolicy>
        {
            new RateLimitPolicy
            {
                Name = "default_per_minute",
                RequestLimit = 60,
                WindowSizeSeconds = 60,
                Description = "Default rate limit: 60 requests per minute"
            },
            new RateLimitPolicy
            {
                Name = "default_per_hour",
                RequestLimit = 1000,
                WindowSizeSeconds = 3600,
                Description = "Default rate limit: 1000 requests per hour"
            }
        };
    }

    private async Task HandleRateLimitExceededAsync(HttpContext context, RateLimitResult violatedPolicy, string clientIdentifier)
    {
        _logger.LogWarning("Rate limit exceeded for client {ClientId} on policy {Policy}: {Count}/{Limit} requests",
            clientIdentifier, violatedPolicy.PolicyName, violatedPolicy.RequestCount, violatedPolicy.RequestLimit);

        context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
        context.Response.ContentType = "application/json";

        // Add rate limit headers
        context.Response.Headers.Add("X-RateLimit-Limit", violatedPolicy.RequestLimit.ToString());
        context.Response.Headers.Add("X-RateLimit-Remaining", "0");
        context.Response.Headers.Add("X-RateLimit-Reset", violatedPolicy.ResetTime.ToUnixTimeSeconds().ToString());
        context.Response.Headers.Add("Retry-After", ((int)violatedPolicy.RetryAfter.TotalSeconds).ToString());

        var errorResponse = ApiResponse<object>.CreateError(
            ApiErrorCodes.RATE_LIMITED,
            "Rate limit exceeded. Please try again later.",
            new
            {
                Policy = violatedPolicy.PolicyName,
                Limit = violatedPolicy.RequestLimit,
                WindowSize = violatedPolicy.WindowSizeSeconds,
                ResetTime = violatedPolicy.ResetTime,
                RetryAfterSeconds = (int)violatedPolicy.RetryAfter.TotalSeconds
            });

        var json = JsonSerializer.Serialize(errorResponse, _jsonOptions);
        await context.Response.WriteAsync(json);
    }

    private async Task HandleRateLimitErrorAsync(HttpContext context, Exception ex)
    {
        _logger.LogError(ex, "Rate limiting service error");

        context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
        context.Response.ContentType = "application/json";

        var errorResponse = ApiResponse<object>.CreateError(
            ApiErrorCodes.SERVICE_UNAVAILABLE,
            "Rate limiting service is temporarily unavailable");

        var json = JsonSerializer.Serialize(errorResponse, _jsonOptions);
        await context.Response.WriteAsync(json);
    }

    private void AddRateLimitHeaders(HttpContext context, IEnumerable<RateLimitResult> rateLimitResults)
    {
        // Add headers for the most restrictive policy
        var mostRestrictive = rateLimitResults
            .OrderBy(r => (double)r.RequestCount / r.RequestLimit)
            .LastOrDefault();

        if (mostRestrictive != null)
        {
            var remaining = Math.Max(0, mostRestrictive.RequestLimit - mostRestrictive.RequestCount);

            context.Response.Headers.Add("X-RateLimit-Limit", mostRestrictive.RequestLimit.ToString());
            context.Response.Headers.Add("X-RateLimit-Remaining", remaining.ToString());
            context.Response.Headers.Add("X-RateLimit-Reset", mostRestrictive.ResetTime.ToUnixTimeSeconds().ToString());
            context.Response.Headers.Add("X-RateLimit-Policy", mostRestrictive.PolicyName);
        }
    }
}
