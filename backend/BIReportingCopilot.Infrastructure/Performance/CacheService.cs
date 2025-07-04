using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Infrastructure.Interfaces;
using ModelsCacheStatistics = BIReportingCopilot.Core.Models.CacheStatistics;
using System.Text.Json;
using System.Collections.Concurrent;

namespace BIReportingCopilot.Infrastructure.Performance;

/// <summary>
/// Unified cache service implementation supporting both memory and distributed caching
/// Consolidates MemoryOptimizedCacheService, DistributedCacheService, and TestCacheService functionality
/// Provides automatic fallback from distributed to memory cache with advanced features
/// </summary>
public class CacheService : 
    BIReportingCopilot.Core.Interfaces.Cache.ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache? _distributedCache;
    private readonly ILogger<CacheService> _logger;
    private readonly IConfiguration _configuration;
    private readonly BIReportingCopilot.Core.Configuration.CacheConfiguration _config;
    private readonly ConcurrentDictionary<string, DateTime> _keyTimestamps;
    private readonly ModelsCacheStatistics _statistics;
    private readonly object _statsLock = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1); // For thread-safe operations

    public CacheService(
        IMemoryCache memoryCache,
        ILogger<CacheService> logger,
        IConfiguration configuration,
        IDistributedCache? distributedCache = null)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
        _configuration = configuration;
        _keyTimestamps = new ConcurrentDictionary<string, DateTime>();
        _statistics = new ModelsCacheStatistics();

        // Load cache configuration
        _config = new BIReportingCopilot.Core.Configuration.CacheConfiguration();
        configuration.GetSection("Cache").Bind(_config);

        _logger.LogDebug("Cache service initialized with memory cache and {DistributedCache}",
            _distributedCache != null ? "distributed cache" : "no distributed cache");
    }

    #region Basic Cache Operations

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            // Try memory cache first
            if (_memoryCache.TryGetValue(key, out T? cachedValue))
            {
                RecordHit();
                _logger.LogDebug("Cache hit (memory): {Key}", key);
                return cachedValue;
            }

            // Try distributed cache if available
            if (_distributedCache != null)
            {
                var distributedValue = await _distributedCache.GetStringAsync(key);
                if (!string.IsNullOrEmpty(distributedValue))
                {
                    var deserializedValue = JsonSerializer.Deserialize<T>(distributedValue);

                    // Store in memory cache for faster access
                    var memoryExpiry = TimeSpan.FromMinutes(_config.DefaultExpirationMinutes);
                    _memoryCache.Set(key, deserializedValue, memoryExpiry);

                    RecordHit();
                    _logger.LogDebug("Cache hit (distributed): {Key}", key);
                    return deserializedValue;
                }
            }

            RecordMiss();
            _logger.LogDebug("Cache miss: {Key}", key);
            return default(T);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached value for key: {Key}", key);
            RecordMiss();
            return default(T);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var actualExpiry = expiry ?? TimeSpan.FromMinutes(_config.DefaultExpirationMinutes);

            // Set in memory cache
            var memoryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = actualExpiry,
                SlidingExpiration = TimeSpan.FromMinutes(_config.DefaultExpirationMinutes / 2), // Use half of default as sliding
                Priority = CacheItemPriority.Normal
            };

            _memoryCache.Set(key, value, memoryOptions);

            // Set in distributed cache if available
            if (_distributedCache != null)
            {
                var serializedValue = JsonSerializer.Serialize(value);
                var distributedOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = actualExpiry,
                    SlidingExpiration = TimeSpan.FromMinutes(_config.DefaultExpirationMinutes / 2) // Use half of default as sliding
                };

                await _distributedCache.SetStringAsync(key, serializedValue, distributedOptions);
            }

            _keyTimestamps[key] = DateTime.UtcNow;
            _logger.LogDebug("Cache set: {Key} with expiry {Expiry}", key, actualExpiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cached value for key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            _memoryCache.Remove(key);

            if (_distributedCache != null)
            {
                await _distributedCache.RemoveAsync(key);
            }

            _keyTimestamps.TryRemove(key, out _);
            _logger.LogDebug("Cache removed: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached value for key: {Key}", key);
        }
    }

    public async Task RemovePatternAsync(string pattern)
    {
        try
        {
            // For memory cache, we need to track keys manually since IMemoryCache doesn't support pattern removal
            var keysToRemove = _keyTimestamps.Keys
                .Where(key => IsPatternMatch(key, pattern))
                .ToList();

            foreach (var key in keysToRemove)
            {
                await RemoveAsync(key);
            }

            _logger.LogDebug("Cache pattern removed: {Pattern}, {Count} keys removed", pattern, keysToRemove.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached values for pattern: {Pattern}", pattern);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            // Check memory cache first
            if (_memoryCache.TryGetValue(key, out _))
            {
                return true;
            }

            // Check distributed cache if available
            if (_distributedCache != null)
            {
                var value = await _distributedCache.GetStringAsync(key);
                return !string.IsNullOrEmpty(value);
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence for key: {Key}", key);
            return false;
        }
    }

    #endregion

    #region Advanced Cache Operations

    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        try
        {
            var currentValueStr = await GetAsync<string>(key.ToString());
            var currentValue = long.TryParse(currentValueStr, out var parsed) ? parsed : 0;
            var newValue = currentValue + value;
            await SetAsync(key, newValue.ToString());
            return newValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing cached value for key: {Key}", key);
            return 0;
        }
    }

    public Task<TimeSpan?> GetTtlAsync(string key)
    {
        try
        {
            if (_keyTimestamps.TryGetValue(key, out var timestamp))
            {
                var elapsed = DateTime.UtcNow - timestamp;
                var defaultExpiry = TimeSpan.FromMinutes(_config.DefaultExpirationMinutes);
                var remaining = defaultExpiry - elapsed;
                return Task.FromResult<TimeSpan?>(remaining > TimeSpan.Zero ? remaining : null);
            }

            return Task.FromResult<TimeSpan?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TTL for key: {Key}", key);
            return Task.FromResult<TimeSpan?>(null);
        }
    }

    public async Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        try
        {
            if (await ExistsAsync(key))
            {
                return false;
            }

            await SetAsync(key, value, expiry);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cached value if not exists for key: {Key}", key);
            return false;
        }
    }

    public async Task<Dictionary<string, T?>> GetMultipleAsync<T>(string[] keys) where T : class
    {
        var results = new Dictionary<string, T?>();

        foreach (var key in keys)
        {
            results[key] = await GetAsync<T>(key);
        }

        return results;
    }

    public async Task SetMultipleAsync<T>(Dictionary<string, T> keyValuePairs, TimeSpan? expiry = null) where T : class
    {
        var tasks = keyValuePairs.Select(kvp => SetAsync(kvp.Key, kvp.Value, expiry));
        await Task.WhenAll(tasks);
    }

    #endregion

    #region Cache Management

    public Task ClearAllAsync()
    {
        try
        {
            // Clear memory cache by disposing and recreating (if possible)
            // Note: IMemoryCache doesn't have a clear method, so we track keys manually
            var keysToRemove = _keyTimestamps.Keys.ToList();
            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
            }

            _keyTimestamps.Clear();

            // Clear distributed cache if available (implementation depends on the provider)
            // Redis example: await _distributedCache.FlushAllAsync();
            // For now, we'll log that distributed cache clear is not implemented
            if (_distributedCache != null)
            {
                _logger.LogWarning("Distributed cache clear not implemented for this provider");
            }

            _logger.LogInformation("Cache cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
        }

        return Task.CompletedTask;
    }

    public Task<ModelsCacheStatistics> GetStatisticsAsync()
    {
        lock (_statsLock)
        {
            _statistics.TotalEntries = _keyTimestamps.Count;
            _statistics.LastUpdated = DateTime.UtcNow;
            return Task.FromResult(new ModelsCacheStatistics
            {
                TotalEntries = _statistics.TotalEntries,
                HitCount = _statistics.HitCount,
                MissCount = _statistics.MissCount,
                TotalSizeBytes = _statistics.TotalSizeBytes,
                LastUpdated = _statistics.LastUpdated
            });
        }
    }

    public Task<bool> RefreshAsync(string key, TimeSpan? newExpiry = null)
    {
        try
        {
            if (_keyTimestamps.TryGetValue(key, out _))
            {
                _keyTimestamps[key] = DateTime.UtcNow;

                // If we have the value in memory cache, reset its expiry
                if (_memoryCache.TryGetValue(key, out var value))
                {
                    var expiry = newExpiry ?? TimeSpan.FromMinutes(_config.DefaultExpirationMinutes);
                    _memoryCache.Set(key, value, expiry);
                    return Task.FromResult(true);
                }
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing cache for key: {Key}", key);
            return Task.FromResult(false);
        }
    }

    #endregion

    #region Private Helper Methods

    private void RecordHit()
    {
        lock (_statsLock)
        {
            _statistics.HitCount++;
        }
    }

    private void RecordMiss()
    {
        lock (_statsLock)
        {
            _statistics.MissCount++;
        }
    }

    private static bool IsPatternMatch(string key, string pattern)
    {
        // Simple pattern matching - supports * wildcard
        if (pattern.Contains('*'))
        {
            var regexPattern = pattern.Replace("*", ".*");
            return System.Text.RegularExpressions.Regex.IsMatch(key, regexPattern);
        }

        return key.Contains(pattern);
    }

    #endregion

    #region Advanced Caching Strategies

    /// <summary>
    /// Get or set with factory pattern - cache-aside pattern with thread-safe double-check locking
    /// </summary>
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null) where T : class
    {
        // First check without lock
        var cachedValue = await GetAsync<T>(key);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        // Use semaphore for thread-safe operations
        await _semaphore.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            cachedValue = await GetAsync<T>(key);
            if (cachedValue != null)
            {
                return cachedValue;
            }

            // Generate value and cache it
            var value = await factory();
            if (value != null)
            {
                await SetAsync(key, value, expiry);
            }

            return value;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Write-through caching pattern
    /// </summary>
    public async Task<T> WriteThrough<T>(string key, T value, Func<T, Task> persistFunc, TimeSpan? expiry = null) where T : class
    {
        // Write to persistent store first
        await persistFunc(value);

        // Then update cache
        await SetAsync(key, value, expiry);

        return value;
    }

    /// <summary>
    /// Write-behind (write-back) caching pattern
    /// </summary>
    public async Task WriteBehind<T>(string key, T value, Func<T, Task> persistFunc, TimeSpan? expiry = null) where T : class
    {
        // Update cache immediately
        await SetAsync(key, value, expiry);

        // Schedule background write to persistent store
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_config.WriteBehindDelaySeconds));
                await persistFunc(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in write-behind operation for key: {Key}", key);
            }
        });
    }

    /// <summary>
    /// Refresh-ahead caching pattern
    /// </summary>
    public async Task<T?> RefreshAhead<T>(string key, Func<Task<T>> factory, TimeSpan refreshThreshold, TimeSpan? expiry = null) where T : class
    {
        var cachedValue = await GetAsync<T>(key);
        var ttl = await GetTtlAsync(key);

        // If cache is about to expire, refresh in background
        if (cachedValue != null && ttl.HasValue && ttl.Value < refreshThreshold)
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    var newValue = await factory();
                    if (newValue != null)
                    {
                        await SetAsync(key, newValue, expiry);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in refresh-ahead operation for key: {Key}", key);
                }
            });
        }

        return cachedValue;
    }

    /// <summary>
    /// Circuit breaker pattern for cache operations
    /// </summary>
    public async Task<T?> GetWithCircuitBreaker<T>(string key, Func<Task<T>> fallback) where T : class
    {
        try
        {
            return await GetAsync<T>(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache operation failed, using fallback for key: {Key}", key);
            return await fallback();
        }
    }

    #endregion

    #region Missing Interface Methods

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        return await GetAsync<T>(key);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry, CancellationToken cancellationToken)
    {
        await SetAsync(key, value, expiry);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await RemoveAsync(key);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return await ExistsAsync(key);
    }

    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        await ClearAllAsync();
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await ClearAllAsync();
    }

    public async Task<Core.Interfaces.Cache.CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var modelStats = await GetStatisticsAsync();
        return new Core.Interfaces.Cache.CacheStatistics
        {
            TotalKeys = modelStats.TotalEntries,
            HitCount = modelStats.HitCount,
            MissCount = modelStats.MissCount,
            MemoryUsage = modelStats.TotalSizeBytes,
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<CacheHealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await GetStatisticsAsync();
            var isHealthy = true;
            var issues = new List<string>();

            // Check if cache is responding
            var testKey = $"health_check_{Guid.NewGuid()}";
            var testValue = "test";

            try
            {
                await SetAsync(testKey, testValue, TimeSpan.FromSeconds(10));
                var retrieved = await GetAsync<string>(testKey);
                if (retrieved != testValue)
                {
                    isHealthy = false;
                    issues.Add("Cache read/write test failed");
                }
                await RemoveAsync(testKey);
            }
            catch (Exception ex)
            {
                isHealthy = false;
                issues.Add($"Cache operation failed: {ex.Message}");
            }

            // Check hit rate
            var totalRequests = stats.HitCount + stats.MissCount;
            var hitRate = totalRequests > 0 ? (double)stats.HitCount / totalRequests : 0;

            if (hitRate < 0.5 && totalRequests > 100) // Less than 50% hit rate with significant traffic
            {
                issues.Add($"Low cache hit rate: {hitRate:P2}");
            }

            return new CacheHealthStatus
            {
                IsHealthy = isHealthy,
                Issues = issues,
                HitRate = hitRate,
                TotalEntries = stats.TotalEntries,
                LastChecked = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache health");
            return new CacheHealthStatus
            {
                IsHealthy = false,
                Issues = new List<string> { $"Health check failed: {ex.Message}" },
                HitRate = 0,
                TotalEntries = 0,
                LastChecked = DateTime.UtcNow
            };
        }
    }

    // Missing ICacheService interface methods
    public async Task RemovePatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Removing keys by pattern: {Pattern}", pattern);
            
            // For memory cache, we need to track keys to support pattern removal
            // This is a simplified implementation
            var keysToRemove = new List<string>();
            
            // Note: IMemoryCache doesn't provide key enumeration
            // In a production system, you'd need to maintain a separate key registry
            _logger.LogWarning("Pattern removal not fully supported in memory cache: {Pattern}", pattern);
            
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing pattern {Pattern}", pattern);
            throw;
        }
    }

    #endregion

    #region Infrastructure Interface Implementation
    // Note: Infrastructure interface implementation temporarily commented out due to interface consolidation
    // TODO: Re-enable after interface consolidation is complete
    /*
    async Task BIReportingCopilot.Infrastructure.Interfaces.ICacheService.ClearAsync()
    {
        await ClearAsync(CancellationToken.None);
    }
    */
    #endregion
}

// CacheConfiguration moved to Core/Configuration/UnifiedConfigurationModels.cs
// This duplicate class has been removed to eliminate configuration duplication
