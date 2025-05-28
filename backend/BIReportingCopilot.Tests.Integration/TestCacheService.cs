using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using System.Text.Json;

namespace BIReportingCopilot.Tests.Integration;

/// <summary>
/// Test-only cache service that uses only memory cache (no Redis dependency)
/// </summary>
public class TestCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<TestCacheService> _logger;

    public TestCacheService(IMemoryCache memoryCache, ILogger<TestCacheService> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        await Task.CompletedTask; // Make it async for interface compatibility

        if (_memoryCache.TryGetValue(key, out T? cached))
        {
            _logger.LogDebug("Cache hit: {Key}", key);
            return cached;
        }

        _logger.LogDebug("Cache miss: {Key}", key);
        return null;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        await Task.CompletedTask; // Make it async for interface compatibility

        var actualExpiry = expiry ?? TimeSpan.FromHours(1);
        _memoryCache.Set(key, value, actualExpiry);
        _logger.LogDebug("Cache set: {Key} with expiry {Expiry}", key, actualExpiry);
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiry = null) where T : class
    {
        var cached = await GetAsync<T>(key);
        if (cached != null)
            return cached;

        var value = await factory();
        await SetAsync(key, value, expiry);
        return value;
    }

    public async Task RemoveAsync(string key)
    {
        await Task.CompletedTask; // Make it async for interface compatibility
        _memoryCache.Remove(key);
        _logger.LogDebug("Cache removed: {Key}", key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        await Task.CompletedTask; // Make it async for interface compatibility
        return _memoryCache.TryGetValue(key, out _);
    }

    public async Task RemovePatternAsync(string pattern)
    {
        await Task.CompletedTask; // Make it async for interface compatibility

        // Simple pattern matching for test purposes
        var keysToRemove = new List<string>();

        // Since IMemoryCache doesn't expose keys, we'll use a simple approach
        // In a real implementation, you'd maintain a separate key registry
        _logger.LogDebug("Pattern removal requested: {Pattern} (simplified implementation)", pattern);
    }

    public async Task<long> IncrementAsync(string key, long value = 1)
    {
        await Task.CompletedTask; // Make it async for interface compatibility

        var currentValue = _memoryCache.TryGetValue(key, out long existing) ? existing : 0;
        var newValue = currentValue + value;

        _memoryCache.Set(key, newValue, TimeSpan.FromHours(1));

        return newValue;
    }

    public async Task<TimeSpan?> GetTtlAsync(string key)
    {
        await Task.CompletedTask; // Make it async for interface compatibility
        // Memory cache doesn't provide TTL information easily
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
}
