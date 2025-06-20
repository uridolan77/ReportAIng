using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using System.Text.RegularExpressions;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.CostOptimization;

/// <summary>
/// Advanced cache optimization service with multi-layer caching, intelligent invalidation, and performance analytics
/// </summary>
public class CacheOptimizationService : ICacheOptimizationService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<CacheOptimizationService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache? _distributedCache;
    
    private readonly Dictionary<string, CacheLayerStats> _layerStats = new();
    private readonly Dictionary<string, List<CacheAccessPattern>> _accessPatterns = new();
    private readonly Dictionary<string, DateTime> _lastInvalidation = new();
    private readonly Dictionary<string, CacheWarmingConfig> _warmingConfigs = new();
    private readonly SemaphoreSlim _operationSemaphore = new(10, 10);

    public CacheOptimizationService(
        BICopilotContext context,
        ILogger<CacheOptimizationService> logger,
        IMemoryCache memoryCache,
        IDistributedCache? distributedCache = null)
    {
        _context = context;
        _logger = logger;
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        InitializeCacheLayerStats();
    }

    #region Cache Performance Analytics

    public async Task<CacheStatistics> GetCacheStatisticsAsync(string cacheType, CancellationToken cancellationToken = default)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddDays(-7);
            var operations = await _context.CacheStatistics
                .Where(c => c.CacheType == cacheType && c.Timestamp >= startDate)
                .ToListAsync(cancellationToken);

            var totalOperations = operations.Count;
            var hits = operations.Count(o => o.Operation == "HIT");
            var misses = operations.Count(o => o.Operation == "MISS");
            var sets = operations.Count(o => o.Operation == "SET");
            var deletes = operations.Count(o => o.Operation == "DELETE");

            var stats = new CacheStatistics
            {
                CacheType = cacheType,
                TotalOperations = totalOperations,
                HitCount = hits,
                MissCount = misses,
                SetCount = sets,
                DeleteCount = deletes,
                HitRate = totalOperations > 0 ? (double)hits / totalOperations : 0,
                AverageResponseTime = operations.Any() ? operations.Average(o => o.DurationMs) : 0,
                TotalSizeBytes = operations.Where(o => o.Operation == "SET").Sum(o => o.SizeBytes),
                LastUpdated = DateTime.UtcNow,
                PeriodStart = startDate,
                PeriodEnd = DateTime.UtcNow
            };

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics for {CacheType}", cacheType);
            return new CacheStatistics { CacheType = cacheType, LastUpdated = DateTime.UtcNow };
        }
    }

    public async Task<Dictionary<string, CacheStatistics>> GetAllCacheStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheTypes = await _context.CacheStatistics
                .Select(c => c.CacheType)
                .Distinct()
                .ToListAsync(cancellationToken);

            var allStats = new Dictionary<string, CacheStatistics>();

            foreach (var cacheType in cacheTypes)
            {
                allStats[cacheType] = await GetCacheStatisticsAsync(cacheType, cancellationToken);
            }

            return allStats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all cache statistics");
            return new Dictionary<string, CacheStatistics>();
        }
    }

    public async Task<List<CacheStatisticsEntry>> GetCacheOperationHistoryAsync(string cacheType, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.CacheStatistics
                .Where(c => c.CacheType == cacheType);

            if (startDate.HasValue)
                query = query.Where(c => c.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.Timestamp <= endDate.Value);

            var entities = await query
                .OrderByDescending(c => c.Timestamp)
                .Take(1000)
                .ToListAsync(cancellationToken);

            return entities.Select(MapToCacheStatisticsEntry).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache operation history");
            return new List<CacheStatisticsEntry>();
        }
    }

    #endregion

    #region Cache Configuration Management

    public async Task<CacheConfiguration> CreateCacheConfigurationAsync(CacheConfiguration config, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new CacheConfigurationEntity
            {
                CacheType = config.CacheType,
                Name = config.Name,
                DefaultTtlSeconds = (long)config.DefaultTtl.TotalSeconds,
                MaxSizeBytes = config.MaxSizeBytes,
                MaxEntries = config.MaxEntries,
                EvictionPolicy = config.EvictionPolicy,
                EnableCompression = config.EnableCompression,
                EnableEncryption = config.EnableEncryption,
                Settings = JsonSerializer.Serialize(config.Settings),
                IsActive = config.IsActive,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "system"
            };

            _context.CacheConfigurations.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            config.Id = entity.Id.ToString();

            _logger.LogInformation("Created cache configuration: {Name} for {CacheType}", config.Name, config.CacheType);

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cache configuration");
            throw;
        }
    }

    public async Task<CacheConfiguration> UpdateCacheConfigurationAsync(CacheConfiguration config, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!long.TryParse(config.Id, out var id))
                throw new ArgumentException("Invalid configuration ID");

            var entity = await _context.CacheConfigurations
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (entity == null)
                throw new ArgumentException($"Configuration not found: {config.Id}");

            entity.Name = config.Name;
            entity.DefaultTtlSeconds = (long)config.DefaultTtl.TotalSeconds;
            entity.MaxSizeBytes = config.MaxSizeBytes;
            entity.MaxEntries = config.MaxEntries;
            entity.EvictionPolicy = config.EvictionPolicy;
            entity.EnableCompression = config.EnableCompression;
            entity.EnableEncryption = config.EnableEncryption;
            entity.Settings = JsonSerializer.Serialize(config.Settings);
            entity.IsActive = config.IsActive;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = "system";

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated cache configuration: {Name}", config.Name);

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cache configuration");
            throw;
        }
    }

    public async Task<CacheConfiguration?> GetCacheConfigurationAsync(string cacheType, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _context.CacheConfigurations
                .FirstOrDefaultAsync(c => c.CacheType == cacheType && c.IsActive, cancellationToken);

            return entity != null ? MapToCacheConfiguration(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache configuration for {CacheType}", cacheType);
            return null;
        }
    }

    public async Task<List<CacheConfiguration>> GetAllCacheConfigurationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _context.CacheConfigurations
                .Where(c => c.IsActive)
                .OrderBy(c => c.CacheType)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);

            return entities.Select(MapToCacheConfiguration).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all cache configurations");
            return new List<CacheConfiguration>();
        }
    }

    public async Task<bool> DeleteCacheConfigurationAsync(string configId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!long.TryParse(configId, out var id))
                return false;

            var entity = await _context.CacheConfigurations
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (entity == null)
                return false;

            entity.IsActive = false;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = "system";

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted cache configuration: {ConfigId}", configId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cache configuration: {ConfigId}", configId);
            return false;
        }
    }

    #endregion

    #region Intelligent Cache Invalidation

    public async Task InvalidateCacheByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            await _operationSemaphore.WaitAsync(cancellationToken);

            var regex = new Regex(pattern.Replace("*", ".*"), RegexOptions.Compiled);
            var invalidatedCount = 0;

            // Invalidate from memory cache
            // Note: IMemoryCache doesn't provide key enumeration, so we track keys manually
            var memoryKeysToInvalidate = GetTrackedKeys()
                .Where(key => regex.IsMatch(key))
                .ToList();

            foreach (var key in memoryKeysToInvalidate)
            {
                _memoryCache.Remove(key);
                invalidatedCount++;
            }

            // Invalidate from distributed cache if available
            if (_distributedCache != null)
            {
                // Implementation depends on the distributed cache provider
                // For Redis, we could use SCAN with pattern matching
                _logger.LogWarning("Pattern-based invalidation for distributed cache not fully implemented");
            }

            _lastInvalidation[pattern] = DateTime.UtcNow;

            await RecordCacheOperationAsync("INVALIDATE_PATTERN", pattern, 0, 0, cancellationToken);

            _logger.LogInformation("Invalidated {Count} cache entries matching pattern: {Pattern}", 
                invalidatedCount, pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache by pattern: {Pattern}", pattern);
        }
        finally
        {
            _operationSemaphore.Release();
        }
    }

    public async Task InvalidateCacheByTagsAsync(List<string> tags, CancellationToken cancellationToken = default)
    {
        try
        {
            await _operationSemaphore.WaitAsync(cancellationToken);

            var invalidatedCount = 0;

            // This would require a tag-based cache implementation
            // For now, we'll simulate by invalidating keys that contain the tag names
            foreach (var tag in tags)
            {
                var keysToInvalidate = GetTrackedKeys()
                    .Where(key => key.Contains(tag, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var key in keysToInvalidate)
                {
                    _memoryCache.Remove(key);
                    if (_distributedCache != null)
                    {
                        await _distributedCache.RemoveAsync(key, cancellationToken);
                    }
                    invalidatedCount++;
                }
            }

            await RecordCacheOperationAsync("INVALIDATE_TAGS", string.Join(",", tags), 0, 0, cancellationToken);

            _logger.LogInformation("Invalidated {Count} cache entries for tags: {Tags}", 
                invalidatedCount, string.Join(", ", tags));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache by tags");
        }
        finally
        {
            _operationSemaphore.Release();
        }
    }

    public async Task<List<string>> GetCacheKeysForInvalidationAsync(string entityType, string entityId, CancellationToken cancellationToken = default)
    {
        try
        {
            var patterns = new List<string>
            {
                $"{entityType}:{entityId}:*",
                $"*:{entityType}:{entityId}",
                $"{entityType.ToLower()}_{entityId}_*",
                $"cache_{entityType}_{entityId}*"
            };

            var keysToInvalidate = new HashSet<string>();

            foreach (var pattern in patterns)
            {
                var regex = new Regex(pattern.Replace("*", ".*"), RegexOptions.Compiled);
                var matchingKeys = GetTrackedKeys()
                    .Where(key => regex.IsMatch(key))
                    .ToList();

                foreach (var key in matchingKeys)
                {
                    keysToInvalidate.Add(key);
                }
            }

            return keysToInvalidate.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache keys for invalidation");
            return new List<string>();
        }
    }

    public async Task ScheduleCacheInvalidationAsync(string cacheKey, DateTime invalidationTime, CancellationToken cancellationToken = default)
    {
        try
        {
            // Schedule background task for cache invalidation
            var delay = invalidationTime - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(delay, cancellationToken);
                        _memoryCache.Remove(cacheKey);
                        if (_distributedCache != null)
                        {
                            await _distributedCache.RemoveAsync(cacheKey, cancellationToken);
                        }

                        await RecordCacheOperationAsync("SCHEDULED_INVALIDATE", cacheKey, 0, 0, cancellationToken);

                        _logger.LogInformation("Scheduled invalidation executed for key: {CacheKey}", cacheKey);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in scheduled cache invalidation for key: {CacheKey}", cacheKey);
                    }
                }, cancellationToken);

                _logger.LogInformation("Scheduled cache invalidation for key: {CacheKey} at {InvalidationTime}",
                    cacheKey, invalidationTime);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling cache invalidation");
        }
    }

    #endregion

    #region Cache Warming

    public async Task<CacheWarmingConfig> CreateCacheWarmingConfigAsync(CacheWarmingConfig config, CancellationToken cancellationToken = default)
    {
        try
        {
            config.Id = Guid.NewGuid().ToString();
            config.NextWarmup = DateTime.UtcNow.Add(config.WarmupInterval);

            _warmingConfigs[config.Id] = config;

            _logger.LogInformation("Created cache warming config: {Name} for {CacheType}", config.Name, config.CacheType);

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cache warming config");
            throw;
        }
    }

    public async Task<CacheWarmingConfig> UpdateCacheWarmingConfigAsync(CacheWarmingConfig config, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_warmingConfigs.ContainsKey(config.Id))
                throw new ArgumentException($"Cache warming config not found: {config.Id}");

            _warmingConfigs[config.Id] = config;

            _logger.LogInformation("Updated cache warming config: {Name}", config.Name);

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cache warming config");
            throw;
        }
    }

    public async Task<List<CacheWarmingConfig>> GetCacheWarmingConfigsAsync(string? cacheType = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var configs = _warmingConfigs.Values.ToList();

            if (!string.IsNullOrEmpty(cacheType))
            {
                configs = configs.Where(c => c.CacheType.Equals(cacheType, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return configs.Where(c => c.IsActive).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache warming configs");
            return new List<CacheWarmingConfig>();
        }
    }

    public async Task<bool> ExecuteCacheWarmupAsync(string configId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_warmingConfigs.TryGetValue(configId, out var config))
            {
                _logger.LogWarning("Cache warming config not found: {ConfigId}", configId);
                return false;
            }

            var warmupTasks = new List<Task>();
            var semaphore = new SemaphoreSlim(config.MaxConcurrentWarmups, config.MaxConcurrentWarmups);

            foreach (var query in config.WarmupQueries)
            {
                warmupTasks.Add(WarmupSingleQueryAsync(query, semaphore, cancellationToken));
            }

            await Task.WhenAll(warmupTasks);

            config.LastWarmup = DateTime.UtcNow;
            config.NextWarmup = DateTime.UtcNow.Add(config.WarmupInterval);

            _logger.LogInformation("Executed cache warmup for config: {Name}, {Count} queries processed",
                config.Name, config.WarmupQueries.Count);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing cache warmup for config: {ConfigId}", configId);
            return false;
        }
    }

    public async Task<bool> ScheduleCacheWarmupAsync(string configId, DateTime warmupTime, CancellationToken cancellationToken = default)
    {
        try
        {
            var delay = warmupTime - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(delay, cancellationToken);
                        await ExecuteCacheWarmupAsync(configId, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in scheduled cache warmup");
                    }
                }, cancellationToken);

                _logger.LogInformation("Scheduled cache warmup for config: {ConfigId} at {WarmupTime}",
                    configId, warmupTime);

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling cache warmup");
            return false;
        }
    }

    #endregion

    #region Cache Optimization Recommendations

    public async Task<List<CacheOptimizationRecommendation>> GetCacheOptimizationRecommendationsAsync(string? cacheType = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var recommendations = new List<CacheOptimizationRecommendation>();
            var allStats = await GetAllCacheStatisticsAsync(cancellationToken);

            foreach (var kvp in allStats)
            {
                if (!string.IsNullOrEmpty(cacheType) && !kvp.Key.Equals(cacheType, StringComparison.OrdinalIgnoreCase))
                    continue;

                var stats = kvp.Value;
                var cacheRecommendations = AnalyzeCachePerformance(stats);
                recommendations.AddRange(cacheRecommendations);
            }

            return recommendations.OrderByDescending(r => r.ImpactScore).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache optimization recommendations");
            return new List<CacheOptimizationRecommendation>();
        }
    }

    public async Task<Dictionary<string, object>> AnalyzeCacheUsagePatternsAsync(string cacheType, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var operations = await GetCacheOperationHistoryAsync(cacheType, startDate, endDate, cancellationToken);

            var analysis = new Dictionary<string, object>
            {
                ["total_operations"] = operations.Count,
                ["unique_keys"] = operations.Select(o => o.Key).Distinct().Count(),
                ["peak_hour"] = GetPeakUsageHour(operations),
                ["average_key_size"] = operations.Where(o => o.Operation == "SET").Any() ?
                    operations.Where(o => o.Operation == "SET").Average(o => o.SizeBytes) : 0,
                ["hot_keys"] = GetHotKeys(operations, 10),
                ["cold_keys"] = GetColdKeys(operations, 10),
                ["operation_distribution"] = GetOperationDistribution(operations),
                ["temporal_patterns"] = GetTemporalPatterns(operations)
            };

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing cache usage patterns");
            return new Dictionary<string, object>();
        }
    }

    public async Task<List<string>> GetUnusedCacheKeysAsync(string cacheType, TimeSpan unusedThreshold, CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffTime = DateTime.UtcNow.Subtract(unusedThreshold);
            var recentOperations = await _context.CacheStatistics
                .Where(c => c.CacheType == cacheType && c.Timestamp >= cutoffTime)
                .Select(c => c.Key)
                .Distinct()
                .ToListAsync(cancellationToken);

            var allKeys = GetTrackedKeys().Where(k => k.StartsWith($"{cacheType}:")).ToList();
            var unusedKeys = allKeys.Except(recentOperations).ToList();

            _logger.LogInformation("Found {Count} unused cache keys for {CacheType} (threshold: {Threshold})",
                unusedKeys.Count, cacheType, unusedThreshold);

            return unusedKeys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unused cache keys");
            return new List<string>();
        }
    }

    #endregion

    #region Multi-Layer Cache Management

    public async Task<bool> PromoteCacheEntryAsync(string cacheKey, string fromLayer, string toLayer, CancellationToken cancellationToken = default)
    {
        try
        {
            // This would implement promotion between cache layers (e.g., L1 -> L2 -> L3)
            // For now, simulate promotion from distributed to memory cache
            if (fromLayer == "distributed" && toLayer == "memory" && _distributedCache != null)
            {
                var value = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);
                if (!string.IsNullOrEmpty(value))
                {
                    _memoryCache.Set(cacheKey, value, TimeSpan.FromMinutes(30));

                    await RecordCacheOperationAsync("PROMOTE", cacheKey, value.Length, 0, cancellationToken);

                    _logger.LogDebug("Promoted cache entry from {FromLayer} to {ToLayer}: {Key}",
                        fromLayer, toLayer, cacheKey);

                    return true;
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error promoting cache entry");
            return false;
        }
    }

    public async Task<Dictionary<string, int>> GetCacheLayerDistributionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var distribution = new Dictionary<string, int>();

            // Count entries in each layer
            foreach (var kvp in _layerStats)
            {
                distribution[kvp.Key] = kvp.Value.EntryCount;
            }

            return distribution;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache layer distribution");
            return new Dictionary<string, int>();
        }
    }

    public async Task OptimizeCacheLayersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Implement cache layer optimization logic
            // This could include promoting frequently accessed items to faster layers
            // and demoting rarely accessed items to slower layers

            var hotKeys = await GetHotKeysFromStats(cancellationToken);
            var coldKeys = await GetColdKeysFromStats(cancellationToken);

            // Promote hot keys to memory cache
            foreach (var key in hotKeys.Take(100)) // Limit to top 100
            {
                await PromoteCacheEntryAsync(key, "distributed", "memory", cancellationToken);
            }

            // Could implement demotion of cold keys here

            _logger.LogInformation("Optimized cache layers: promoted {HotCount} hot keys", hotKeys.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing cache layers");
        }
    }

    #endregion

    #region Cache Performance Monitoring

    public async Task RecordCacheOperationAsync(CacheStatisticsEntry operation, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new CacheStatisticsEntity
            {
                CacheType = operation.CacheType,
                Operation = operation.Operation,
                Key = operation.Key,
                SizeBytes = operation.SizeBytes,
                DurationMs = operation.DurationMs,
                Timestamp = operation.Timestamp,
                UserId = operation.UserId,
                Metadata = JsonSerializer.Serialize(operation.Metadata),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = operation.UserId ?? "system"
            };

            _context.CacheStatistics.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            // Update layer stats
            UpdateLayerStats(operation.CacheType, operation.Operation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording cache operation");
        }
    }

    public async Task<double> GetCacheHitRateAsync(string cacheType, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await GetCacheStatisticsAsync(cacheType, cancellationToken);
            return stats.HitRate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache hit rate");
            return 0.0;
        }
    }

    public async Task<TimeSpan> GetAverageCacheRetrievalTimeAsync(string cacheType, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await GetCacheStatisticsAsync(cacheType, cancellationToken);
            return TimeSpan.FromMilliseconds(stats.AverageResponseTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average cache retrieval time");
            return TimeSpan.Zero;
        }
    }

    public async Task<long> GetCacheSizeAsync(string cacheType, CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await GetCacheStatisticsAsync(cacheType, cancellationToken);
            return stats.TotalSizeBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache size");
            return 0;
        }
    }

    #endregion

    #region Cache Health Monitoring

    public async Task<Dictionary<string, object>> GetCacheHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var allStats = await GetAllCacheStatisticsAsync(cancellationToken);
            var healthStatus = new Dictionary<string, object>();

            foreach (var kvp in allStats)
            {
                var stats = kvp.Value;
                var health = new Dictionary<string, object>
                {
                    ["hit_rate"] = stats.HitRate,
                    ["avg_response_time"] = stats.AverageResponseTime,
                    ["total_operations"] = stats.TotalOperations,
                    ["health_score"] = CalculateHealthScore(stats),
                    ["status"] = GetHealthStatus(stats)
                };

                healthStatus[kvp.Key] = health;
            }

            return healthStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache health status");
            return new Dictionary<string, object>();
        }
    }

    public async Task<List<ResourceMonitoringAlert>> GetCacheAlertsAsync(bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var alerts = new List<ResourceMonitoringAlert>();
            var allStats = await GetAllCacheStatisticsAsync(cancellationToken);

            foreach (var kvp in allStats)
            {
                var stats = kvp.Value;

                // Low hit rate alert
                if (stats.HitRate < 0.5) // Less than 50% hit rate
                {
                    alerts.Add(new ResourceMonitoringAlert
                    {
                        Id = Guid.NewGuid().ToString(),
                        AlertType = "LowCacheHitRate",
                        ResourceType = "Cache",
                        Severity = stats.HitRate < 0.3 ? "High" : "Medium",
                        Message = $"Low cache hit rate for {kvp.Key}: {stats.HitRate:P}",
                        Threshold = 0.5,
                        CurrentValue = stats.HitRate,
                        TriggeredAt = DateTime.UtcNow,
                        IsResolved = false,
                        Metadata = new Dictionary<string, object>
                        {
                            ["cache_type"] = kvp.Key,
                            ["total_operations"] = stats.TotalOperations
                        }
                    });
                }

                // High response time alert
                if (stats.AverageResponseTime > 1000) // More than 1 second
                {
                    alerts.Add(new ResourceMonitoringAlert
                    {
                        Id = Guid.NewGuid().ToString(),
                        AlertType = "HighCacheResponseTime",
                        ResourceType = "Cache",
                        Severity = stats.AverageResponseTime > 5000 ? "High" : "Medium",
                        Message = $"High cache response time for {kvp.Key}: {stats.AverageResponseTime:F0}ms",
                        Threshold = 1000,
                        CurrentValue = stats.AverageResponseTime,
                        TriggeredAt = DateTime.UtcNow,
                        IsResolved = false,
                        Metadata = new Dictionary<string, object>
                        {
                            ["cache_type"] = kvp.Key,
                            ["total_operations"] = stats.TotalOperations
                        }
                    });
                }
            }

            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache alerts");
            return new List<ResourceMonitoringAlert>();
        }
    }

    public async Task<bool> CheckCacheHealthAsync(string cacheType, CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await GetCacheStatisticsAsync(cacheType, cancellationToken);
            var healthScore = CalculateHealthScore(stats);
            return healthScore > 0.7; // Consider healthy if score > 70%
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache health");
            return false;
        }
    }

    #endregion

    #region Helper Methods

    private void InitializeCacheLayerStats()
    {
        _layerStats["memory"] = new CacheLayerStats { LayerName = "memory", EntryCount = 0, HitCount = 0, MissCount = 0 };
        _layerStats["distributed"] = new CacheLayerStats { LayerName = "distributed", EntryCount = 0, HitCount = 0, MissCount = 0 };
    }

    private List<string> GetTrackedKeys()
    {
        // This would typically come from a key tracking mechanism
        // For now, return empty list as IMemoryCache doesn't provide key enumeration
        return new List<string>();
    }

    private async Task RecordCacheOperationAsync(string operation, string key, long sizeBytes, long durationMs, CancellationToken cancellationToken)
    {
        try
        {
            var entry = new CacheStatisticsEntry
            {
                CacheType = "system",
                Operation = operation,
                Key = key,
                SizeBytes = sizeBytes,
                DurationMs = durationMs,
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>()
            };

            await RecordCacheOperationAsync(entry, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording cache operation");
        }
    }

    private async Task WarmupSingleQueryAsync(string query, SemaphoreSlim semaphore, CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            // This would execute the actual query to warm up the cache
            // For now, just simulate the operation
            await Task.Delay(100, cancellationToken); // Simulate query execution

            _logger.LogDebug("Warmed up cache for query: {Query}", query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error warming up cache for query: {Query}", query);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private List<CacheOptimizationRecommendation> AnalyzeCachePerformance(CacheStatistics stats)
    {
        var recommendations = new List<CacheOptimizationRecommendation>();

        // Low hit rate recommendation
        if (stats.HitRate < 0.7)
        {
            recommendations.Add(new CacheOptimizationRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Type = "HitRateOptimization",
                Title = "Improve Cache Hit Rate",
                Description = $"Cache hit rate is {stats.HitRate:P}, consider optimizing cache keys and TTL",
                PotentialSavings = 0,
                ImpactScore = 1.0 - stats.HitRate,
                Priority = stats.HitRate < 0.5 ? "High" : "Medium",
                Implementation = "Review cache key patterns, optimize TTL settings, implement cache warming",
                Benefits = new List<string> { "Improved response times", "Reduced database load", "Better user experience" },
                Risks = new List<string> { "Increased memory usage", "Potential stale data" },
                CreatedAt = DateTime.UtcNow,
                IsImplemented = false
            });
        }

        // High response time recommendation
        if (stats.AverageResponseTime > 500)
        {
            recommendations.Add(new CacheOptimizationRecommendation
            {
                Id = Guid.NewGuid().ToString(),
                Type = "ResponseTimeOptimization",
                Title = "Optimize Cache Response Time",
                Description = $"Average response time is {stats.AverageResponseTime:F0}ms, consider cache layer optimization",
                PotentialSavings = 0,
                ImpactScore = Math.Min(1.0, stats.AverageResponseTime / 5000.0),
                Priority = stats.AverageResponseTime > 2000 ? "High" : "Medium",
                Implementation = "Implement multi-layer caching, optimize serialization, consider local caching",
                Benefits = new List<string> { "Faster response times", "Better scalability", "Improved throughput" },
                Risks = new List<string> { "Increased complexity", "Memory overhead" },
                CreatedAt = DateTime.UtcNow,
                IsImplemented = false
            });
        }

        return recommendations;
    }

    private static double CalculateHealthScore(CacheStatistics stats)
    {
        var hitRateScore = stats.HitRate;
        var responseTimeScore = Math.Max(0, 1.0 - (stats.AverageResponseTime / 5000.0)); // Normalize to 5 seconds
        var operationScore = stats.TotalOperations > 100 ? 1.0 : stats.TotalOperations / 100.0;

        return (hitRateScore * 0.5) + (responseTimeScore * 0.3) + (operationScore * 0.2);
    }

    private static string GetHealthStatus(CacheStatistics stats)
    {
        var score = CalculateHealthScore(stats);
        return score switch
        {
            >= 0.8 => "Excellent",
            >= 0.6 => "Good",
            >= 0.4 => "Fair",
            >= 0.2 => "Poor",
            _ => "Critical"
        };
    }

    private static int GetPeakUsageHour(List<CacheStatisticsEntry> operations)
    {
        if (!operations.Any()) return 0;

        return operations
            .GroupBy(o => o.Timestamp.Hour)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;
    }

    private static List<string> GetHotKeys(List<CacheStatisticsEntry> operations, int count)
    {
        return operations
            .Where(o => o.Operation == "HIT")
            .GroupBy(o => o.Key)
            .OrderByDescending(g => g.Count())
            .Take(count)
            .Select(g => g.Key)
            .ToList();
    }

    private static List<string> GetColdKeys(List<CacheStatisticsEntry> operations, int count)
    {
        return operations
            .Where(o => o.Operation == "MISS")
            .GroupBy(o => o.Key)
            .OrderByDescending(g => g.Count())
            .Take(count)
            .Select(g => g.Key)
            .ToList();
    }

    private static Dictionary<string, int> GetOperationDistribution(List<CacheStatisticsEntry> operations)
    {
        return operations
            .GroupBy(o => o.Operation)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private static Dictionary<string, object> GetTemporalPatterns(List<CacheStatisticsEntry> operations)
    {
        if (!operations.Any()) return new Dictionary<string, object>();

        var hourlyDistribution = operations
            .GroupBy(o => o.Timestamp.Hour)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var dailyDistribution = operations
            .GroupBy(o => o.Timestamp.DayOfWeek)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        return new Dictionary<string, object>
        {
            ["hourly_distribution"] = hourlyDistribution,
            ["daily_distribution"] = dailyDistribution,
            ["peak_hour"] = GetPeakUsageHour(operations),
            ["total_span_hours"] = operations.Any() ?
                (operations.Max(o => o.Timestamp) - operations.Min(o => o.Timestamp)).TotalHours : 0
        };
    }

    private async Task<List<string>> GetHotKeysFromStats(CancellationToken cancellationToken)
    {
        try
        {
            var recentOperations = await _context.CacheStatistics
                .Where(c => c.Operation == "HIT" && c.Timestamp >= DateTime.UtcNow.AddHours(-1))
                .GroupBy(c => c.Key)
                .OrderByDescending(g => g.Count())
                .Take(100)
                .Select(g => g.Key)
                .ToListAsync(cancellationToken);

            return recentOperations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting hot keys from stats");
            return new List<string>();
        }
    }

    private async Task<List<string>> GetColdKeysFromStats(CancellationToken cancellationToken)
    {
        try
        {
            var recentOperations = await _context.CacheStatistics
                .Where(c => c.Operation == "MISS" && c.Timestamp >= DateTime.UtcNow.AddHours(-1))
                .GroupBy(c => c.Key)
                .OrderByDescending(g => g.Count())
                .Take(100)
                .Select(g => g.Key)
                .ToListAsync(cancellationToken);

            return recentOperations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cold keys from stats");
            return new List<string>();
        }
    }

    private void UpdateLayerStats(string cacheType, string operation)
    {
        if (_layerStats.TryGetValue(cacheType, out var stats))
        {
            switch (operation)
            {
                case "HIT":
                    stats.HitCount++;
                    break;
                case "MISS":
                    stats.MissCount++;
                    break;
                case "SET":
                    stats.EntryCount++;
                    break;
                case "DELETE":
                    stats.EntryCount = Math.Max(0, stats.EntryCount - 1);
                    break;
            }
        }
    }

    private static CacheStatisticsEntry MapToCacheStatisticsEntry(CacheStatisticsEntity entity)
    {
        return new CacheStatisticsEntry
        {
            Id = entity.Id.ToString(),
            CacheType = entity.CacheType,
            Operation = entity.Operation,
            Key = entity.Key,
            SizeBytes = entity.SizeBytes,
            DurationMs = entity.DurationMs,
            Timestamp = entity.Timestamp,
            UserId = entity.UserId,
            Metadata = string.IsNullOrEmpty(entity.Metadata) ?
                new Dictionary<string, object>() :
                JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Metadata) ?? new Dictionary<string, object>()
        };
    }

    private static CacheConfiguration MapToCacheConfiguration(CacheConfigurationEntity entity)
    {
        return new CacheConfiguration
        {
            Id = entity.Id.ToString(),
            CacheType = entity.CacheType,
            Name = entity.Name,
            DefaultTtl = TimeSpan.FromSeconds(entity.DefaultTtlSeconds),
            MaxSizeBytes = entity.MaxSizeBytes,
            MaxEntries = entity.MaxEntries,
            EvictionPolicy = entity.EvictionPolicy,
            EnableCompression = entity.EnableCompression,
            EnableEncryption = entity.EnableEncryption,
            Settings = string.IsNullOrEmpty(entity.Settings) ?
                new Dictionary<string, object>() :
                JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Settings) ?? new Dictionary<string, object>(),
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedDate,
            UpdatedAt = entity.UpdatedDate ?? entity.CreatedDate
        };
    }

    #endregion

    private class CacheLayerStats
    {
        public string LayerName { get; set; } = string.Empty;
        public int EntryCount { get; set; }
        public int HitCount { get; set; }
        public int MissCount { get; set; }
    }

    private class CacheAccessPattern
    {
        public string Key { get; set; } = string.Empty;
        public DateTime AccessTime { get; set; }
        public string Operation { get; set; } = string.Empty;
        public long DurationMs { get; set; }
    }
}
