using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using BIReportingCopilot.Core.Exceptions;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.Security;

/// <summary>
/// Distributed rate limiting service using Redis with sliding window algorithm
/// </summary>
public class DistributedRateLimitingService : IRateLimitingService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<DistributedRateLimitingService> _logger;
    private readonly RateLimitingConfiguration _config;

    public DistributedRateLimitingService(
        IConnectionMultiplexer redis,
        ILogger<DistributedRateLimitingService> logger,
        IOptions<RateLimitingConfiguration> config)
    {
        _redis = redis;
        _database = redis.GetDatabase();
        _logger = logger;
        _config = config.Value;
    }

    public async Task<RateLimitResult> CheckRateLimitAsync(string identifier, RateLimitPolicy policy, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetRateLimitKey(identifier, policy.Name);
            var now = DateTimeOffset.UtcNow;
            var windowStart = now.AddSeconds(-policy.WindowSizeSeconds);

            // Use Lua script for atomic operations
            var script = @"
                local key = KEYS[1]
                local window_start = tonumber(ARGV[1])
                local current_time = tonumber(ARGV[2])
                local limit = tonumber(ARGV[3])
                local window_size = tonumber(ARGV[4])
                local ttl = tonumber(ARGV[5])

                -- Remove expired entries
                redis.call('ZREMRANGEBYSCORE', key, 0, window_start)

                -- Count current requests in window
                local current_count = redis.call('ZCARD', key)

                -- Check if limit exceeded
                if current_count >= limit then
                    local oldest_entry = redis.call('ZRANGE', key, 0, 0, 'WITHSCORES')
                    local reset_time = 0
                    if #oldest_entry > 0 then
                        reset_time = oldest_entry[2] + window_size
                    end
                    return {current_count, limit, reset_time, 0}
                end

                -- Add current request
                redis.call('ZADD', key, current_time, current_time)
                redis.call('EXPIRE', key, ttl)

                -- Calculate reset time (when oldest entry expires)
                local oldest_entry = redis.call('ZRANGE', key, 0, 0, 'WITHSCORES')
                local reset_time = current_time + window_size
                if #oldest_entry > 0 then
                    reset_time = oldest_entry[2] + window_size
                end

                return {current_count + 1, limit, reset_time, 1}
            ";

            var result = await _database.ScriptEvaluateAsync(script, new RedisKey[] { key }, new RedisValue[]
            {
                windowStart.ToUnixTimeSeconds(),
                now.ToUnixTimeSeconds(),
                policy.RequestLimit,
                policy.WindowSizeSeconds,
                policy.WindowSizeSeconds * 2 // TTL should be longer than window
            });

            var values = (RedisValue[])result!;
            var currentCount = (int)values[0];
            var limit = (int)values[1];
            var resetTime = DateTimeOffset.FromUnixTimeSeconds((long)values[2]);
            var allowed = (int)values[3] == 1;

            var rateLimitResult = new RateLimitResult
            {
                IsAllowed = allowed,
                RequestCount = currentCount,
                RequestLimit = limit,
                WindowSizeSeconds = policy.WindowSizeSeconds,
                ResetTime = resetTime,
                RetryAfter = allowed ? TimeSpan.Zero : resetTime.Subtract(now),
                PolicyName = policy.Name
            };

            if (!allowed)
            {
                _logger.LogWarning("Rate limit exceeded for {Identifier} on policy {Policy}: {Count}/{Limit} requests", 
                    identifier, policy.Name, currentCount, limit);
            }

            return rateLimitResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for {Identifier} on policy {Policy}", identifier, policy.Name);
            
            // Fail open - allow request if rate limiting service is down
            return new RateLimitResult
            {
                IsAllowed = true,
                RequestCount = 0,
                RequestLimit = policy.RequestLimit,
                WindowSizeSeconds = policy.WindowSizeSeconds,
                ResetTime = DateTimeOffset.UtcNow.AddSeconds(policy.WindowSizeSeconds),
                RetryAfter = TimeSpan.Zero,
                PolicyName = policy.Name
            };
        }
    }

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
            
            // If any policy is violated, we can stop checking others
            if (!result.IsAllowed)
            {
                break;
            }
        }
        
        return results;
    }

    public async Task<RateLimitStatistics> GetRateLimitStatisticsAsync(string identifier, string policyName, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetRateLimitKey(identifier, policyName);
            var now = DateTimeOffset.UtcNow;
            var windowStart = now.AddSeconds(-_config.DefaultWindowSizeSeconds);

            // Get current window data
            var entries = await _database.SortedSetRangeByScoreWithScoresAsync(
                key, 
                windowStart.ToUnixTimeSeconds(), 
                now.ToUnixTimeSeconds());

            var requestTimes = entries.Select(e => DateTimeOffset.FromUnixTimeSeconds((long)e.Score)).ToList();
            
            var statistics = new RateLimitStatistics
            {
                Identifier = identifier,
                PolicyName = policyName,
                CurrentWindowRequests = requestTimes.Count,
                WindowStartTime = windowStart,
                WindowEndTime = now,
                RequestTimestamps = requestTimes,
                AverageRequestsPerMinute = CalculateAverageRequestsPerMinute(requestTimes),
                PeakRequestsPerMinute = CalculatePeakRequestsPerMinute(requestTimes)
            };

            return statistics;
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

    public async Task ResetRateLimitAsync(string identifier, string policyName, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetRateLimitKey(identifier, policyName);
            await _database.KeyDeleteAsync(key);
            
            _logger.LogInformation("Reset rate limit for {Identifier} on policy {Policy}", identifier, policyName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting rate limit for {Identifier} on policy {Policy}", identifier, policyName);
        }
    }

    public async Task<bool> IsRateLimitActiveAsync(string identifier, string policyName, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = GetRateLimitKey(identifier, policyName);
            return await _database.KeyExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if rate limit is active for {Identifier} on policy {Policy}", identifier, policyName);
            return false;
        }
    }

    private string GetRateLimitKey(string identifier, string policyName)
    {
        return $"{_config.KeyPrefix}:ratelimit:{policyName}:{identifier}";
    }

    private double CalculateAverageRequestsPerMinute(List<DateTimeOffset> requestTimes)
    {
        if (!requestTimes.Any())
            return 0;

        var timeSpan = requestTimes.Max().Subtract(requestTimes.Min());
        if (timeSpan.TotalMinutes == 0)
            return requestTimes.Count;

        return requestTimes.Count / timeSpan.TotalMinutes;
    }

    private double CalculatePeakRequestsPerMinute(List<DateTimeOffset> requestTimes)
    {
        if (!requestTimes.Any())
            return 0;

        var maxRequestsInMinute = 0;
        var sortedTimes = requestTimes.OrderBy(t => t).ToList();

        for (int i = 0; i < sortedTimes.Count; i++)
        {
            var windowEnd = sortedTimes[i].AddMinutes(1);
            var requestsInWindow = sortedTimes.Skip(i).TakeWhile(t => t <= windowEnd).Count();
            maxRequestsInMinute = Math.Max(maxRequestsInMinute, requestsInWindow);
        }

        return maxRequestsInMinute;
    }
}

/// <summary>
/// Interface for rate limiting service
/// </summary>
public interface IRateLimitingService
{
    Task<RateLimitResult> CheckRateLimitAsync(string identifier, RateLimitPolicy policy, CancellationToken cancellationToken = default);
    Task<Dictionary<string, RateLimitResult>> CheckMultipleRateLimitsAsync(string identifier, IEnumerable<RateLimitPolicy> policies, CancellationToken cancellationToken = default);
    Task<RateLimitStatistics> GetRateLimitStatisticsAsync(string identifier, string policyName, CancellationToken cancellationToken = default);
    Task ResetRateLimitAsync(string identifier, string policyName, CancellationToken cancellationToken = default);
    Task<bool> IsRateLimitActiveAsync(string identifier, string policyName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Rate limiting configuration
/// </summary>
public class RateLimitingConfiguration
{
    public string KeyPrefix { get; set; } = "bi-reporting";
    public int DefaultWindowSizeSeconds { get; set; } = 3600; // 1 hour
    public bool EnableRateLimiting { get; set; } = true;
    public bool FailOpen { get; set; } = true; // Allow requests if Redis is down
    public List<RateLimitPolicy> Policies { get; set; } = new();
}

/// <summary>
/// Rate limit policy definition
/// </summary>
public class RateLimitPolicy
{
    public string Name { get; set; } = string.Empty;
    public int RequestLimit { get; set; }
    public int WindowSizeSeconds { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> AppliesTo { get; set; } = new(); // User roles, endpoints, etc.
}

/// <summary>
/// Rate limit check result
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
