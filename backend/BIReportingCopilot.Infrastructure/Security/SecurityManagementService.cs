using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using BIReportingCopilot.Infrastructure.Configuration;
using BIReportingCopilot.Core.Configuration;

namespace BIReportingCopilot.Infrastructure.Security;

/// <summary>
/// Unified security management service consolidating rate limiting, SQL validation, and security policies
/// Replaces RateLimitingService, DistributedRateLimitingService, and SQL validation services
/// </summary>
public class SecurityManagementService : IRateLimitingService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<SecurityManagementService> _logger;
    private readonly UnifiedConfigurationService _configurationService;
    private readonly SecurityConfiguration _securityConfig;

    // SQL injection patterns
    private readonly List<string> _sqlInjectionPatterns = new()
    {
        @"(\b(ALTER|CREATE|DELETE|DROP|EXEC(UTE)?|INSERT|MERGE|SELECT|UPDATE|UNION|USE)\b)",
        @"(\b(GRANT|REVOKE|DENY)\b)",
        @"(\b(BACKUP|RESTORE)\b)",
        @"(\b(SHUTDOWN|KILL)\b)",
        @"(--|\#|\/\*|\*\/)",
        @"(\b(xp_|sp_)\w+)",
        @"(\b(OPENROWSET|OPENDATASOURCE|OPENQUERY|OPENXML)\b)",
        @"(\b(BULK\s+INSERT)\b)",
        @"(\b(LOAD_FILE|INTO\s+OUTFILE|INTO\s+DUMPFILE)\b)"
    };

    // Default rate limits
    private readonly Dictionary<string, (int maxRequests, TimeSpan window)> _defaultLimits = new()
    {
        { "query", (100, TimeSpan.FromHours(1)) },
        { "history", (200, TimeSpan.FromHours(1)) },
        { "feedback", (50, TimeSpan.FromHours(1)) },
        { "suggestions", (300, TimeSpan.FromHours(1)) },
        { "login", (10, TimeSpan.FromMinutes(15)) },
        { "api", (1000, TimeSpan.FromHours(1)) }
    };

    public SecurityManagementService(
        IDistributedCache cache,
        ILogger<SecurityManagementService> logger,
        UnifiedConfigurationService configurationService)
    {
        _cache = cache;
        _logger = logger;
        _configurationService = configurationService;
        _securityConfig = configurationService.GetSecuritySettings();
    }

    #region Rate Limiting

    /// <summary>
    /// Check rate limit for user and endpoint
    /// </summary>
    public async Task<RateLimitResult> CheckRateLimitAsync(string userId, string endpoint)
    {
        if (!_securityConfig.EnableRateLimit)
        {
            return new RateLimitResult { IsAllowed = true, PolicyName = "disabled" };
        }

        var (maxRequests, window) = GetRateLimitForEndpoint(endpoint);
        var key = $"rate_limit:{userId}:{endpoint}";
        return await CheckRateLimitInternalAsync(key, maxRequests, window, endpoint);
    }

    /// <summary>
    /// Check rate limit with custom policy
    /// </summary>
    public async Task<RateLimitResult> CheckRateLimitAsync(string identifier, RateLimitPolicy policy, CancellationToken cancellationToken = default)
    {
        if (!_securityConfig.EnableRateLimit)
        {
            return new RateLimitResult { IsAllowed = true, PolicyName = policy.Name };
        }

        var key = $"rate_limit:{identifier}:{policy.Name}";
        var window = TimeSpan.FromSeconds(policy.WindowSizeSeconds);
        return await CheckRateLimitInternalAsync(key, policy.RequestLimit, window, policy.Name);
    }

    /// <summary>
    /// Check multiple rate limits
    /// </summary>
    public async Task<Dictionary<string, RateLimitResult>> CheckMultipleRateLimitsAsync(
        string identifier,
        IEnumerable<RateLimitPolicy> policies,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, RateLimitResult>();

        foreach (var policy in policies)
        {
            var result = await CheckRateLimitAsync(identifier, policy, cancellationToken);
            results[policy.Name] = result;

            // If any policy is violated, stop checking others
            if (!result.IsAllowed)
            {
                break;
            }
        }

        return results;
    }

    /// <summary>
    /// Reset rate limit for identifier and policy
    /// </summary>
    public async Task ResetRateLimitAsync(string identifier, string policyName, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"rate_limit:{identifier}:{policyName}";
            await _cache.RemoveAsync(cacheKey, cancellationToken);
            _logger.LogInformation("Reset rate limit for {Identifier} on policy {Policy}", identifier, policyName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting rate limit for {Identifier} on policy {Policy}", identifier, policyName);
        }
    }

    /// <summary>
    /// Get rate limit statistics
    /// </summary>
    public async Task<RateLimitStatistics?> GetRateLimitStatisticsAsync(string identifier, string policyName, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"rate_limit:{identifier}:{policyName}";
            var rateLimitData = await GetRateLimitDataAsync(cacheKey);

            return new RateLimitStatistics
            {
                Identifier = identifier,
                PolicyName = policyName,
                CurrentWindowRequests = rateLimitData.Requests.Count,
                WindowStartTime = rateLimitData.Requests.Any() ? rateLimitData.Requests.Min() : DateTimeOffset.UtcNow,
                WindowEndTime = DateTimeOffset.UtcNow,
                RequestTimestamps = rateLimitData.Requests.Select(r => (DateTimeOffset)r).ToList(),
                AverageRequestsPerMinute = CalculateAverageRequestsPerMinute(rateLimitData.Requests),
                PeakRequestsPerMinute = rateLimitData.Requests.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rate limit statistics for {Identifier} on policy {Policy}", identifier, policyName);
            return new RateLimitStatistics
            {
                Identifier = identifier,
                PolicyName = policyName,
                CurrentWindowRequests = 0,
                WindowStartTime = DateTimeOffset.UtcNow,
                WindowEndTime = DateTimeOffset.UtcNow,
                RequestTimestamps = new List<DateTimeOffset>(),
                AverageRequestsPerMinute = 0,
                PeakRequestsPerMinute = 0
            };
        }
    }

    #endregion

    #region SQL Validation

    /// <summary>
    /// Validate SQL query for security threats
    /// </summary>
    public SecurityValidationResult ValidateSqlQuery(string query)
    {
        if (!_securityConfig.EnableSqlValidation)
        {
            return new SecurityValidationResult { IsValid = true, ValidationLevel = "disabled" };
        }

        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new SecurityValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Query cannot be empty",
                    ValidationLevel = "basic"
                };
            }

            var normalizedQuery = query.ToUpperInvariant().Trim();

            // Check for SQL injection patterns
            var threats = new List<string>();
            foreach (var pattern in _sqlInjectionPatterns)
            {
                if (Regex.IsMatch(normalizedQuery, pattern, RegexOptions.IgnoreCase))
                {
                    threats.Add($"Potential SQL injection pattern detected: {pattern}");
                }
            }

            // Check for dangerous operations
            if (ContainsDangerousOperations(normalizedQuery))
            {
                threats.Add("Query contains potentially dangerous operations");
            }

            // Check query length
            if (query.Length > _securityConfig.MaxQueryLength)
            {
                threats.Add($"Query exceeds maximum length of {_securityConfig.MaxQueryLength} characters");
            }

            if (threats.Any())
            {
                _logger.LogWarning("SQL validation failed for query. Threats: {Threats}", string.Join(", ", threats));
                return new SecurityValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Query contains potential security threats",
                    SecurityThreats = threats,
                    ValidationLevel = "enhanced"
                };
            }

            return new SecurityValidationResult
            {
                IsValid = true,
                ValidationLevel = "enhanced",
                SecurityThreats = new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating SQL query");
            return new SecurityValidationResult
            {
                IsValid = false,
                ErrorMessage = "Error occurred during SQL validation",
                ValidationLevel = "error"
            };
        }
    }

    /// <summary>
    /// Enhanced SQL validation with additional checks
    /// </summary>
    public SecurityValidationResult ValidateSqlQueryEnhanced(string query, string? userId = null)
    {
        var basicResult = ValidateSqlQuery(query);

        if (!basicResult.IsValid)
        {
            return basicResult;
        }

        try
        {
            var additionalThreats = new List<string>();

            // Check for suspicious patterns
            if (ContainsSuspiciousPatterns(query))
            {
                additionalThreats.Add("Query contains suspicious patterns");
            }

            // Check for excessive complexity
            if (IsQueryTooComplex(query))
            {
                additionalThreats.Add("Query is excessively complex");
            }

            if (additionalThreats.Any())
            {
                _logger.LogWarning("Enhanced SQL validation failed for query by user {UserId}. Additional threats: {Threats}",
                    userId, string.Join(", ", additionalThreats));

                return new SecurityValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Query failed enhanced security validation",
                    SecurityThreats = basicResult.SecurityThreats.Concat(additionalThreats).ToList(),
                    ValidationLevel = "enhanced"
                };
            }

            return new SecurityValidationResult
            {
                IsValid = true,
                ValidationLevel = "enhanced",
                SecurityThreats = new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in enhanced SQL validation");
            return basicResult; // Fall back to basic validation result
        }
    }

    #endregion

    #region Security Monitoring

    /// <summary>
    /// Log security event
    /// </summary>
    public async Task LogSecurityEventAsync(string userId, string eventType, string description, object? details = null)
    {
        try
        {
            var securityEvent = new SecurityEvent
            {
                UserId = userId,
                EventType = eventType,
                Description = description,
                Details = details,
                Timestamp = DateTime.UtcNow,
                IpAddress = GetCurrentIpAddress()
            };

            // Log to application logs
            _logger.LogWarning("Security event: {EventType} for user {UserId}: {Description}",
                eventType, userId, description);

            // Store in cache for monitoring
            var cacheKey = $"security_event:{userId}:{DateTime.UtcNow:yyyyMMddHH}";
            await StoreSecurityEventAsync(cacheKey, securityEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging security event for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Check if user has suspicious activity
    /// </summary>
    public async Task<bool> HasSuspiciousActivityAsync(string userId, TimeSpan timeWindow)
    {
        try
        {
            var events = await GetRecentSecurityEventsAsync(userId, timeWindow);

            // Define suspicious activity thresholds
            var suspiciousThresholds = new Dictionary<string, int>
            {
                { "failed_login", 5 },
                { "rate_limit_exceeded", 10 },
                { "sql_injection_attempt", 1 },
                { "unauthorized_access", 3 }
            };

            foreach (var threshold in suspiciousThresholds)
            {
                var eventCount = events.Count(e => e.EventType == threshold.Key);
                if (eventCount >= threshold.Value)
                {
                    _logger.LogWarning("Suspicious activity detected for user {UserId}: {EventType} occurred {Count} times",
                        userId, threshold.Key, eventCount);
                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking suspicious activity for user {UserId}", userId);
            return false;
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<RateLimitResult> CheckRateLimitInternalAsync(string key, int maxRequests, TimeSpan window, string policyName)
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
                    RequestCount = rateLimitData.Requests.Count,
                    RequestLimit = maxRequests,
                    WindowSizeSeconds = (int)window.TotalSeconds,
                    ResetTime = DateTimeOffset.UtcNow.Add(retryAfter),
                    RetryAfter = retryAfter > TimeSpan.Zero ? retryAfter : TimeSpan.Zero,
                    PolicyName = policyName
                };
            }

            // Add current request
            rateLimitData.Requests.Add(now);
            await SaveRateLimitDataAsync(cacheKey, rateLimitData, window);

            return new RateLimitResult
            {
                IsAllowed = true,
                RequestCount = rateLimitData.Requests.Count,
                RequestLimit = maxRequests,
                WindowSizeSeconds = (int)window.TotalSeconds,
                ResetTime = DateTimeOffset.UtcNow.Add(window),
                RetryAfter = TimeSpan.Zero,
                PolicyName = policyName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for key {Key}", key);
            return new RateLimitResult
            {
                IsAllowed = true,
                RequestCount = 0,
                RequestLimit = maxRequests,
                WindowSizeSeconds = (int)window.TotalSeconds,
                ResetTime = DateTimeOffset.UtcNow.Add(window),
                RetryAfter = TimeSpan.Zero,
                PolicyName = "error"
            };
        }
    }

    private (int maxRequests, TimeSpan window) GetRateLimitForEndpoint(string endpoint)
    {
        // Try to get from configuration first
        var configKey = $"RateLimit:{endpoint}";
        var maxRequests = _securityConfig.RateLimits?.GetValueOrDefault($"{endpoint}:MaxRequests");
        var windowMinutes = _securityConfig.RateLimits?.GetValueOrDefault($"{endpoint}:WindowMinutes");

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
                AbsoluteExpirationRelativeToNow = expiry.Add(TimeSpan.FromMinutes(5))
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save rate limit data for key {Key}", cacheKey);
        }
    }

    private bool ContainsDangerousOperations(string query)
    {
        var dangerousKeywords = new[] { "DROP", "DELETE", "TRUNCATE", "ALTER", "CREATE", "EXEC", "EXECUTE" };
        return dangerousKeywords.Any(keyword => query.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private bool ContainsSuspiciousPatterns(string query)
    {
        var suspiciousPatterns = new[] { "UNION", "OR 1=1", "OR '1'='1'", "WAITFOR", "DELAY" };
        return suspiciousPatterns.Any(pattern => query.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsQueryTooComplex(string query)
    {
        var complexity = 0;
        complexity += Regex.Matches(query, @"\bJOIN\b", RegexOptions.IgnoreCase).Count * 2;
        complexity += Regex.Matches(query, @"\bSUBQUERY\b", RegexOptions.IgnoreCase).Count * 3;
        complexity += Regex.Matches(query, @"\bUNION\b", RegexOptions.IgnoreCase).Count * 2;

        return complexity > _securityConfig.MaxQueryComplexity;
    }

    private double CalculateAverageRequestsPerMinute(List<DateTime> requests)
    {
        if (!requests.Any()) return 0;

        var timeSpan = DateTime.UtcNow - requests.Min();
        var minutes = Math.Max(1, timeSpan.TotalMinutes);
        return requests.Count / minutes;
    }

    private string GetCurrentIpAddress()
    {
        // This would typically be injected via HttpContext
        return "unknown";
    }

    private async Task StoreSecurityEventAsync(string cacheKey, SecurityEvent securityEvent)
    {
        try
        {
            var serialized = JsonSerializer.Serialize(securityEvent);
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store security event");
        }
    }

    private async Task<List<SecurityEvent>> GetRecentSecurityEventsAsync(string userId, TimeSpan timeWindow)
    {
        var events = new List<SecurityEvent>();
        var now = DateTime.UtcNow;

        // Check recent hours for events
        for (int i = 0; i < timeWindow.TotalHours; i++)
        {
            var hour = now.AddHours(-i);
            var cacheKey = $"security_event:{userId}:{hour:yyyyMMddHH}";

            try
            {
                var cachedData = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    var securityEvent = JsonSerializer.Deserialize<SecurityEvent>(cachedData);
                    if (securityEvent != null && securityEvent.Timestamp > now.Subtract(timeWindow))
                    {
                        events.Add(securityEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error retrieving security event from cache");
            }
        }

        return events;
    }

    #endregion
}

/// <summary>
/// Rate limit data for caching
/// </summary>
public class RateLimitData
{
    public List<DateTime> Requests { get; set; } = new();
}

/// <summary>
/// Security event for monitoring
/// </summary>
public class SecurityEvent
{
    public string UserId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public object? Details { get; set; }
    public DateTime Timestamp { get; set; }
    public string IpAddress { get; set; } = string.Empty;
}

/// <summary>
/// Security validation result
/// </summary>
public class SecurityValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> SecurityThreats { get; set; } = new();
    public string ValidationLevel { get; set; } = string.Empty;
}

/// <summary>
/// Rate limit result
/// </summary>
public class RateLimitResult
{
    public bool IsAllowed { get; set; }
    public int RequestCount { get; set; }
    public int RequestLimit { get; set; }
    public int WindowSizeSeconds { get; set; }
    public DateTimeOffset ResetTime { get; set; }
    public TimeSpan RetryAfter { get; set; }
    public string PolicyName { get; set; } = string.Empty;
}

/// <summary>
/// Rate limit policy
/// </summary>
public class RateLimitPolicy
{
    public string Name { get; set; } = string.Empty;
    public int RequestLimit { get; set; }
    public int WindowSizeSeconds { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> AppliesTo { get; set; } = new();
}

/// <summary>
/// Rate limit statistics
/// </summary>
public class RateLimitStatistics
{
    public string Identifier { get; set; } = string.Empty;
    public string PolicyName { get; set; } = string.Empty;
    public int CurrentWindowRequests { get; set; }
    public DateTimeOffset WindowStartTime { get; set; }
    public DateTimeOffset WindowEndTime { get; set; }
    public List<DateTimeOffset> RequestTimestamps { get; set; } = new();
    public double AverageRequestsPerMinute { get; set; }
    public double PeakRequestsPerMinute { get; set; }
}