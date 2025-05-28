using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces;

namespace BIReportingCopilot.Infrastructure.Caching;

/// <summary>
/// Redis-based distributed cache service with advanced features
/// </summary>
public class DistributedCacheService : ICacheService, IDisposable
{
    private readonly IDistributedCache _distributedCache;
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<DistributedCacheService> _logger;
    private readonly DistributedCacheConfiguration _config;
    private readonly JsonSerializerOptions _jsonOptions;

    public DistributedCacheService(
        IDistributedCache distributedCache,
        IConnectionMultiplexer redis,
        ILogger<DistributedCacheService> logger,
        IOptions<DistributedCacheConfiguration> config)
    {
        _distributedCache = distributedCache;
        _redis = redis;
        _database = redis.GetDatabase();
        _logger = logger;
        _config = config.Value;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var fullKey = GetFullKey(key);
            var cachedValue = await _distributedCache.GetStringAsync(fullKey);

            if (string.IsNullOrEmpty(cachedValue))
            {
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return null;
            }

            var result = JsonSerializer.Deserialize<T>(cachedValue, _jsonOptions);
            _logger.LogDebug("Cache hit for key: {Key}", key);

            // Update access time for LRU tracking
            await UpdateAccessTimeAsync(fullKey);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached value for key: {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        try
        {
            var fullKey = GetFullKey(key);
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);

            var options = new DistributedCacheEntryOptions();

            if (expiry.HasValue)
            {
                options.SetAbsoluteExpiration(expiry.Value);
            }
            else
            {
                options.SetAbsoluteExpiration(TimeSpan.FromMinutes(_config.DefaultExpiryMinutes));
            }

            await _distributedCache.SetStringAsync(fullKey, serializedValue, options);

            // Store metadata for advanced features
            await StoreMetadataAsync(fullKey, value.GetType().Name, expiry);

            _logger.LogDebug("Cached value for key: {Key} with expiry: {Expiry}", key, expiry);
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
            var fullKey = GetFullKey(key);
            await _distributedCache.RemoveAsync(fullKey);
            await RemoveMetadataAsync(fullKey);

            _logger.LogDebug("Removed cached value for key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached value for key: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var exists = await _database.KeyExistsAsync(fullKey);
            return exists;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if key exists: {Key}", key);
            return false;
        }
    }

    public async Task<TimeSpan?> GetTtlAsync(string key)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var ttl = await _database.KeyTimeToLiveAsync(fullKey);
            return ttl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting TTL for key: {Key}", key);
            return null;
        }
    }

    public async Task ClearAsync()
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{_config.KeyPrefix}:*");

            var keyArray = keys.ToArray();
            if (keyArray.Length > 0)
            {
                await _database.KeyDeleteAsync(keyArray);
                _logger.LogInformation("Cleared {Count} cached items", keyArray.Length);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
        }
    }

    // Advanced distributed cache features

    /// <summary>
    /// Set multiple cache entries in a single operation
    /// </summary>
    public async Task SetManyAsync<T>(Dictionary<string, T> items, TimeSpan? expiry = null) where T : class
    {
        try
        {
            var batch = _database.CreateBatch();
            var tasks = new List<Task>();

            foreach (var item in items)
            {
                var fullKey = GetFullKey(item.Key);
                var serializedValue = JsonSerializer.Serialize(item.Value, _jsonOptions);

                var expiryTime = expiry ?? TimeSpan.FromMinutes(_config.DefaultExpiryMinutes);

                tasks.Add(batch.StringSetAsync(fullKey, serializedValue, expiryTime));
                tasks.Add(StoreMetadataAsync(fullKey, item.Value.GetType().Name, expiry, batch));
            }

            batch.Execute();
            await Task.WhenAll(tasks);

            _logger.LogDebug("Set {Count} cache entries in batch", items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting multiple cache entries");
        }
    }

    /// <summary>
    /// Get multiple cache entries in a single operation
    /// </summary>
    public async Task<Dictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys) where T : class
    {
        var result = new Dictionary<string, T?>();

        try
        {
            var fullKeys = keys.Select(GetFullKey).ToArray();
            var redisKeys = fullKeys.Select(k => (RedisKey)k).ToArray();
            var values = await _database.StringGetAsync(redisKeys);

            for (int i = 0; i < keys.Count(); i++)
            {
                var originalKey = keys.ElementAt(i);
                var value = values[i];

                if (value.HasValue)
                {
                    try
                    {
                        var deserializedValue = JsonSerializer.Deserialize<T>(value!, _jsonOptions);
                        result[originalKey] = deserializedValue;

                        // Update access time
                        _ = UpdateAccessTimeAsync(fullKeys[i]);
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogWarning(ex, "Error deserializing cached value for key: {Key}", originalKey);
                        result[originalKey] = null;
                    }
                }
                else
                {
                    result[originalKey] = null;
                }
            }

            _logger.LogDebug("Retrieved {Count} cache entries in batch", keys.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting multiple cache entries");
        }

        return result;
    }

    /// <summary>
    /// Remove cache entries by pattern
    /// </summary>
    public async Task RemoveByPatternAsync(string pattern)
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var fullPattern = GetFullKey(pattern);
            var keys = server.Keys(pattern: fullPattern);

            var keyArray = keys.ToArray();
            if (keyArray.Length > 0)
            {
                var redisKeys = keyArray.Select(k => (RedisKey)k).ToArray();
                await _database.KeyDeleteAsync(redisKeys);

                // Remove metadata for these keys
                var metadataKeys = keyArray.Select(k => $"{k}:metadata").ToArray();
                var redisMetadataKeys = metadataKeys.Select(k => (RedisKey)k).ToArray();
                await _database.KeyDeleteAsync(redisMetadataKeys);

                _logger.LogInformation("Removed {Count} cache entries matching pattern: {Pattern}", keyArray.Length, pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entries by pattern: {Pattern}", pattern);
        }
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    public async Task<CacheStatistics> GetStatisticsAsync()
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var info = await server.InfoAsync();

            var keyCount = server.Keys(pattern: $"{_config.KeyPrefix}:*").Count();

            var stats = new Core.Interfaces.CacheStatistics
            {
                TotalKeys = keyCount,
                MemoryUsage = GetMemoryUsage(info),
                HitCount = 0, // Would need to track this
                MissCount = 0, // Would need to track this
                LastUpdated = DateTime.UtcNow
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics");
            return new Core.Interfaces.CacheStatistics();
        }
    }

    /// <summary>
    /// Distributed lock implementation
    /// </summary>
    public async Task<IDisposable?> AcquireLockAsync(string lockKey, TimeSpan expiry, TimeSpan? timeout = null)
    {
        try
        {
            var fullLockKey = GetFullKey($"lock:{lockKey}");
            var lockValue = Guid.NewGuid().ToString();
            var timeoutTime = DateTime.UtcNow.Add(timeout ?? TimeSpan.FromSeconds(30));

            while (DateTime.UtcNow < timeoutTime)
            {
                var acquired = await _database.StringSetAsync(fullLockKey, lockValue, expiry, When.NotExists);

                if (acquired)
                {
                    _logger.LogDebug("Acquired distributed lock: {LockKey}", lockKey);
                    return new DistributedLock(_database, fullLockKey, lockValue, _logger);
                }

                await Task.Delay(100); // Wait before retrying
            }

            _logger.LogWarning("Failed to acquire distributed lock: {LockKey}", lockKey);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acquiring distributed lock: {LockKey}", lockKey);
            return null;
        }
    }

    private string GetFullKey(string key)
    {
        return $"{_config.KeyPrefix}:{key}";
    }

    private async Task StoreMetadataAsync(string fullKey, string typeName, TimeSpan? expiry, IBatch? batch = null)
    {
        try
        {
            var metadata = new CacheMetadata
            {
                TypeName = typeName,
                CreatedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow,
                ExpiryTime = expiry.HasValue ? DateTime.UtcNow.Add(expiry.Value) : null
            };

            var metadataKey = $"{fullKey}:metadata";
            var metadataJson = JsonSerializer.Serialize(metadata, _jsonOptions);

            if (batch != null)
            {
                batch.StringSetAsync(metadataKey, metadataJson, expiry);
            }
            else
            {
                await _database.StringSetAsync(metadataKey, metadataJson, expiry);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error storing cache metadata for key: {Key}", fullKey);
        }
    }

    private async Task UpdateAccessTimeAsync(string fullKey)
    {
        try
        {
            var metadataKey = $"{fullKey}:metadata";
            var metadataJson = await _database.StringGetAsync(metadataKey);

            if (metadataJson.HasValue)
            {
                var metadata = JsonSerializer.Deserialize<CacheMetadata>(metadataJson!, _jsonOptions);
                if (metadata != null)
                {
                    metadata.LastAccessedAt = DateTime.UtcNow;
                    var updatedJson = JsonSerializer.Serialize(metadata, _jsonOptions);
                    await _database.StringSetAsync(metadataKey, updatedJson, TimeSpan.FromMinutes(_config.DefaultExpiryMinutes));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error updating access time for key: {Key}", fullKey);
        }
    }

    private async Task RemoveMetadataAsync(string fullKey)
    {
        try
        {
            var metadataKey = $"{fullKey}:metadata";
            await _database.KeyDeleteAsync(metadataKey);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error removing metadata for key: {Key}", fullKey);
        }
    }

    private long GetMemoryUsage(IGrouping<string, KeyValuePair<string, string>>[] info)
    {
        try
        {
            var memorySection = info.FirstOrDefault(g => g.Key == "Memory");
            if (memorySection != null)
            {
                var usedMemory = memorySection.FirstOrDefault(kvp => kvp.Key == "used_memory");
                if (long.TryParse(usedMemory.Value, out var memory))
                {
                    return memory;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error parsing memory usage from Redis info");
        }

        return 0;
    }

    private double GetHitRate(IGrouping<string, KeyValuePair<string, string>>[] info)
    {
        try
        {
            var statsSection = info.FirstOrDefault(g => g.Key == "Stats");
            if (statsSection != null)
            {
                var hits = statsSection.FirstOrDefault(kvp => kvp.Key == "keyspace_hits");
                var misses = statsSection.FirstOrDefault(kvp => kvp.Key == "keyspace_misses");

                if (long.TryParse(hits.Value, out var hitCount) &&
                    long.TryParse(misses.Value, out var missCount))
                {
                    var total = hitCount + missCount;
                    return total > 0 ? (double)hitCount / total : 0.0;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error parsing hit rate from Redis info");
        }

        return 0.0;
    }

    // ICacheService interface implementations that delegate to existing methods
    public async Task RemovePatternAsync(string pattern)
    {
        await RemoveByPatternAsync(pattern);
    }

    public async Task<long> IncrementAsync(string key, long increment = 1)
    {
        try
        {
            var fullKey = GetFullKey(key);
            return await _database.StringIncrementAsync(fullKey, increment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing key: {Key}", key);
            return 0;
        }
    }

    async Task<bool> ICacheService.SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiry)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
            var expiryTime = expiry ?? TimeSpan.FromMinutes(_config.DefaultExpiryMinutes);

            var result = await _database.StringSetAsync(fullKey, serializedValue, expiryTime, When.NotExists);

            if (result)
            {
                await StoreMetadataAsync(fullKey, value?.GetType().Name ?? "Unknown", expiry);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value if not exists for key: {Key}", key);
            return false;
        }
    }

    public async Task<Dictionary<string, T?>> GetMultipleAsync<T>(string[] keys) where T : class
    {
        return await GetManyAsync<T>(keys);
    }

    async Task ICacheService.SetMultipleAsync<T>(Dictionary<string, T> keyValuePairs, TimeSpan? expiry)
    {
        await SetManyAsync(keyValuePairs, expiry);
    }

    public async Task ClearAllAsync()
    {
        await ClearAsync();
    }

    public async Task<bool> RefreshAsync(string key, TimeSpan? newExpiry = null)
    {
        try
        {
            var fullKey = GetFullKey(key);
            var value = await _database.StringGetAsync(fullKey);

            if (value.HasValue)
            {
                var expiryTime = newExpiry ?? TimeSpan.FromMinutes(_config.DefaultExpiryMinutes);
                var result = await _database.KeyExpireAsync(fullKey, expiryTime);

                // Update metadata
                await UpdateAccessTimeAsync(fullKey);

                return result;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing key: {Key}", key);
            return false;
        }
    }

    public void Dispose()
    {
        // Redis connection is managed by DI container
        _logger.LogInformation("Distributed cache service disposed");
    }
}

/// <summary>
/// Distributed cache configuration
/// </summary>
public class DistributedCacheConfiguration
{
    public string KeyPrefix { get; set; } = "bi-reporting";
    public int DefaultExpiryMinutes { get; set; } = 60;
    public string ConnectionString { get; set; } = string.Empty;
    public bool EnableCompression { get; set; } = true;
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromMilliseconds(500);
}

/// <summary>
/// Cache metadata for tracking
/// </summary>
public class CacheMetadata
{
    public string TypeName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastAccessedAt { get; set; }
    public DateTime? ExpiryTime { get; set; }
}

// CacheStatistics class is defined in Core.Interfaces

/// <summary>
/// Distributed lock implementation
/// </summary>
public class DistributedLock : IDisposable
{
    private readonly IDatabase _database;
    private readonly string _lockKey;
    private readonly string _lockValue;
    private readonly ILogger _logger;
    private bool _disposed;

    public DistributedLock(IDatabase database, string lockKey, string lockValue, ILogger logger)
    {
        _database = database;
        _lockKey = lockKey;
        _lockValue = lockValue;
        _logger = logger;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            // Release lock only if we still own it
            const string script = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end";

            _database.ScriptEvaluate(script, new RedisKey[] { _lockKey }, new RedisValue[] { _lockValue });
            _logger.LogDebug("Released distributed lock: {LockKey}", _lockKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error releasing distributed lock: {LockKey}", _lockKey);
        }

        _disposed = true;
    }
}
