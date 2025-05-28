using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.Performance;

public class MemoryOptimizedCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly ILogger<MemoryOptimizedCacheService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public MemoryOptimizedCacheService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        ILogger<MemoryOptimizedCacheService> logger)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        // Try memory cache first
        if (_memoryCache.TryGetValue(key, out T? cached))
        {
            _logger.LogDebug("Cache hit (memory): {Key}", key);
            return cached;
        }

        // Try distributed cache
        try
        {
            var distributedData = await _distributedCache.GetStringAsync(key);
            if (!string.IsNullOrEmpty(distributedData))
            {
                _logger.LogDebug("Cache hit (distributed): {Key}", key);
                var value = JsonSerializer.Deserialize<T>(distributedData);

                // Add to memory cache for faster subsequent access
                _memoryCache.Set(key, value, TimeSpan.FromMinutes(5));

                return value;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error accessing distributed cache for key: {Key}", key);
        }

        _logger.LogDebug("Cache miss: {Key}", key);
        return null;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var actualExpiry = expiry ?? TimeSpan.FromHours(1);

        // Set in memory cache with shorter expiry
        _memoryCache.Set(key, value, TimeSpan.FromMinutes(Math.Min(5, actualExpiry.TotalMinutes)));

        // Set in distributed cache
        try
        {
            var serialized = JsonSerializer.Serialize(value);
            await _distributedCache.SetStringAsync(key, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = actualExpiry
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting distributed cache for key: {Key}", key);
        }
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiry = null) where T : class
    {
        var cached = await GetAsync<T>(key);
        if (cached != null)
            return cached;

        await _semaphore.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            cached = await GetAsync<T>(key);
            if (cached != null)
                return cached;

            var value = await factory();
            await SetAsync(key, value, expiry);
            return value;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task RemoveAsync(string key)
    {
        _memoryCache.Remove(key);
        await _distributedCache.RemoveAsync(key);
    }

    public async Task RemovePatternAsync(string pattern)
    {
        // This is a simplified implementation
        // In production, you might want to use Redis SCAN with pattern matching
        _logger.LogWarning("RemoveByPatternAsync not fully implemented for pattern: {Pattern}", pattern);
        await Task.CompletedTask;
    }

    public async Task<bool> ExistsAsync(string key)
    {
        if (_memoryCache.TryGetValue(key, out _))
            return true;

        try
        {
            var distributedData = await _distributedCache.GetStringAsync(key);
            return !string.IsNullOrEmpty(distributedData);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking cache existence for key: {Key}", key);
            return false;
        }
    }

    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        // This is a simplified implementation
        // In production, you'd want to use Redis INCR command for atomic operations
        try
        {
            // For increment operations, we'll use a simple approach
            // In production, you'd want to use Redis INCR for atomic operations
            var currentStr = await _distributedCache.GetStringAsync(key);
            var current = string.IsNullOrEmpty(currentStr) ? 0 : long.Parse(currentStr);
            var newValue = current + value;
            await _distributedCache.SetStringAsync(key, newValue.ToString(), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
            });
            return newValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing cache key: {Key}", key);
            return value; // Return the increment value as fallback
        }
    }

    public async Task<TimeSpan?> GetTtlAsync(string key)
    {
        // This is a limitation of the current implementation
        // IDistributedCache doesn't provide TTL information
        // In production, you'd use Redis directly for this
        _logger.LogWarning("GetTtlAsync not fully supported with IDistributedCache");
        await Task.CompletedTask;
        return null;
    }

    public async Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var exists = await ExistsAsync(key);
        if (!exists)
        {
            await SetAsync(key, value, expiry);
            return true;
        }
        return false;
    }

    // Additional methods required by ICacheService interface
    public async Task<Dictionary<string, T?>> GetMultipleAsync<T>(string[] keys) where T : class
    {
        var results = new Dictionary<string, T?>();

        foreach (var key in keys)
        {
            results[key] = await GetAsync<T>(key);
        }

        return results;
    }

    public async Task SetMultipleAsync<T>(Dictionary<string, T> items, TimeSpan? expiry = null) where T : class
    {
        var tasks = items.Select(kvp => SetAsync(kvp.Key, kvp.Value, expiry));
        await Task.WhenAll(tasks);
    }

    public async Task ClearAllAsync()
    {
        // This is a simplified implementation
        // In production, you'd want to use Redis FLUSHDB or similar
        _logger.LogWarning("ClearAllAsync not fully implemented - only clearing memory cache");

        if (_memoryCache is MemoryCache mc)
        {
            mc.Clear();
        }

        await Task.CompletedTask;
    }

    public async Task<CacheStatistics> GetStatisticsAsync()
    {
        // This is a simplified implementation
        // In production, you'd gather real statistics from Redis
        return new CacheStatistics
        {
            TotalKeys = 0, // Not easily available with IDistributedCache
            HitCount = 0,  // Would need to track this
            MissCount = 0, // Would need to track this
            MemoryUsage = 0, // Not easily available
            LastUpdated = DateTime.UtcNow
        };
    }

    public async Task<bool> RefreshAsync(string key, TimeSpan? newExpiry = null)
    {
        // Get the current value
        var value = await GetAsync<object>(key);
        if (value != null)
        {
            // Reset with new expiry
            await SetAsync(key, value, newExpiry);
            return true;
        }
        return false;
    }
}
