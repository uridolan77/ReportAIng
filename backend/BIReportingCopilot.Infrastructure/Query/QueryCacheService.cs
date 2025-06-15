using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.Query;

/// <summary>
/// Service responsible for query caching operations
/// </summary>
public class QueryCacheService : IQueryCacheService
{
    private readonly BIReportingCopilot.Infrastructure.Interfaces.ICacheService _cacheService;
    private readonly BIReportingCopilot.Core.Interfaces.AI.ISemanticCacheService _semanticCacheService;
    private readonly ILogger<QueryCacheService> _logger;
    private readonly QueryCacheConfiguration _config;

    public QueryCacheService(
        BIReportingCopilot.Infrastructure.Interfaces.ICacheService cacheService,
        BIReportingCopilot.Core.Interfaces.AI.ISemanticCacheService semanticCacheService,
        ILogger<QueryCacheService> logger)
    {
        _cacheService = cacheService;
        _semanticCacheService = semanticCacheService;
        _logger = logger;
        _config = new QueryCacheConfiguration();
    }

    #region Cache Operations

    public async Task<QueryResponse?> GetCachedQueryAsync(string queryHash)
    {
        try
        {
            // First try exact match cache
            var exactMatch = await _cacheService.GetAsync<QueryResponse>($"query:{queryHash}");
            if (exactMatch != null)
            {
                _logger.LogDebug("Exact cache hit for query hash: {QueryHash}", queryHash);
                return exactMatch;
            }

            _logger.LogDebug("No exact cache hit for query hash: {QueryHash}", queryHash);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached query {QueryHash}", queryHash);
            return null;
        }
    }

    public async Task<SemanticCacheResult?> GetSemanticallySimilarQueryAsync(string naturalLanguageQuery, string sqlQuery)
    {
        try
        {
            return await _semanticCacheService.GetSemanticallySimilarAsync(naturalLanguageQuery, sqlQuery);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving semantically similar query for: {Query}", naturalLanguageQuery);
            return null;
        }
    }

    public async Task CacheQueryAsync(string queryHash, QueryResponse response, TimeSpan? expiry = null)
    {
        try
        {
            var cacheExpiry = expiry ?? _config.DefaultCacheExpiry;
            await _cacheService.SetAsync($"query:{queryHash}", response, cacheExpiry);
            
            _logger.LogDebug("Cached query with hash: {QueryHash} for {Expiry}", queryHash, cacheExpiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching query {QueryHash}", queryHash);
        }
    }

    public async Task CacheSemanticQueryAsync(string naturalLanguageQuery, string sqlQuery, QueryResponse response, TimeSpan? expiry = null)
    {
        try
        {
            await _semanticCacheService.CacheSemanticQueryAsync(naturalLanguageQuery, sqlQuery, response, expiry);
            _logger.LogDebug("Cached semantic query: {Query}", naturalLanguageQuery);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching semantic query: {Query}", naturalLanguageQuery);
        }
    }

    public async Task InvalidateCacheAsync(string queryHash)
    {
        try
        {
            await _cacheService.RemoveAsync($"query:{queryHash}");
            _logger.LogDebug("Invalidated cache for query hash: {QueryHash}", queryHash);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for query {QueryHash}", queryHash);
        }
    }

    public async Task InvalidateByPatternAsync(string pattern)
    {
        try
        {
            await _cacheService.RemovePatternAsync($"query:{pattern}*");
            _logger.LogDebug("Invalidated cache by pattern: {Pattern}", pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache by pattern {Pattern}", pattern);
        }
    }

    public async Task InvalidateByDataChangeAsync(string tableName, string changeType)
    {
        try
        {
            // Invalidate semantic cache
            await _semanticCacheService.InvalidateByDataChangeAsync(tableName, changeType);
            
            // Invalidate regular cache for queries involving this table
            await InvalidateByPatternAsync($"*{tableName}*");
            
            _logger.LogInformation("Invalidated cache for table {TableName} due to {ChangeType}", tableName, changeType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for table {TableName}", tableName);
        }
    }

    #endregion

    #region Cache Statistics

    public async Task<CacheStatistics> GetCacheStatisticsAsync()
    {
        try
        {
            var semanticStats = await _semanticCacheService.GetCacheStatisticsAsync();
            
            return new CacheStatistics
            {
                SemanticCacheHits = semanticStats.HitCount,
                SemanticCacheMisses = semanticStats.MissCount,
                SemanticCacheHitRate = semanticStats.HitRate,
                TotalSemanticEntries = (int)semanticStats.TotalKeys, // Convert TotalKeys to TotalEntries
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics");
            return new CacheStatistics { LastUpdated = DateTime.UtcNow };
        }
    }

    #endregion

    #region Cache Management

    public async Task CleanupExpiredEntriesAsync()
    {
        try
        {
            await _semanticCacheService.CleanupExpiredEntriesAsync();
            _logger.LogInformation("Completed cache cleanup");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache cleanup");
        }
    }

    public async Task WarmupCacheAsync(List<string> commonQueries)
    {
        try
        {
            _logger.LogInformation("Starting cache warmup with {QueryCount} queries", commonQueries.Count);
            
            var warmupTasks = commonQueries.Select(async query =>
            {
                try
                {
                    // Pre-generate cache keys and check if they exist
                    var queryHash = GenerateQueryHash(query);
                    var cached = await GetCachedQueryAsync(queryHash);
                    
                    if (cached == null)
                    {
                        _logger.LogDebug("Cache miss during warmup for query: {Query}", query);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error during cache warmup for query: {Query}", query);
                }
            });

            await Task.WhenAll(warmupTasks);
            _logger.LogInformation("Completed cache warmup");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during cache warmup");
        }
    }

    #endregion

    #region Utility Methods

    public string GenerateQueryHash(string query)
    {
        try
        {
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(query.Trim().ToLowerInvariant()));
            return Convert.ToBase64String(hashBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating query hash");
            return query.GetHashCode().ToString();
        }
    }

    public string GenerateQueryHash(string naturalLanguageQuery, string sqlQuery)
    {
        var combined = $"{naturalLanguageQuery}|{sqlQuery}";
        return GenerateQueryHash(combined);
    }

    #endregion

    #region Missing Interface Method Implementations

    /// <summary>
    /// Get cached value (IQueryCacheService interface)
    /// </summary>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            return await _cacheService.GetAsync<T>(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached value for key: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// Set cached value (IQueryCacheService interface)
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            await _cacheService.SetAsync(key, value, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cached value for key: {Key}", key);
        }
    }

    /// <summary>
    /// Remove cached value (IQueryCacheService interface)
    /// </summary>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cacheService.RemoveAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached value for key: {Key}", key);
        }
    }

    /// <summary>
    /// Remove cached values by pattern (IQueryCacheService interface)
    /// </summary>
    public async Task RemovePatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            await InvalidateByPatternAsync(pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached values by pattern: {Pattern}", pattern);
        }
    }

    /// <summary>
    /// Check if key exists (IQueryCacheService interface)
    /// </summary>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _cacheService.ExistsAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if key exists: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Clear all cached values (IQueryCacheService interface)
    /// </summary>
    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _cacheService.ClearAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all cached values");
        }
    }

    /// <summary>
    /// Get cache statistics (IQueryCacheService interface)
    /// </summary>
    public async Task<BIReportingCopilot.Core.Interfaces.Query.CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await GetCacheStatisticsAsync();
            return new BIReportingCopilot.Core.Interfaces.Query.CacheStatistics
            {
                TotalKeys = stats.TotalSemanticEntries,
                HitCount = stats.SemanticCacheHits,
                MissCount = stats.SemanticCacheMisses,
                MemoryUsage = 0, // Not available in current implementation
                LastUpdated = stats.LastUpdated
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics");
            return new BIReportingCopilot.Core.Interfaces.Query.CacheStatistics
            {
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Invalidate by pattern (IQueryCacheService interface)
    /// </summary>
    public async Task InvalidateByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        await InvalidateByPatternAsync(pattern);
    }

    /// <summary>
    /// Cache query result (IQueryCacheService interface)
    /// </summary>
    public async Task CacheQueryAsync(string key, object result, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cacheService.SetAsync(key, result, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching query result for key: {Key}", key);
        }
    }

    /// <summary>
    /// Get cached query result (IQueryCacheService interface)
    /// </summary>
    public async Task<T?> GetCachedQueryAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _cacheService.GetAsync<T>(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached query result for key: {Key}", key);
            return default;
        }
    }

    #endregion
}

/// <summary>
/// Configuration for query caching
/// </summary>
public class QueryCacheConfiguration
{
    public TimeSpan DefaultCacheExpiry { get; set; } = TimeSpan.FromHours(1);
    public TimeSpan SemanticCacheExpiry { get; set; } = TimeSpan.FromHours(24);
    public int MaxCacheEntries { get; set; } = 10000;
    public double SemanticSimilarityThreshold { get; set; } = 0.85;
}

/// <summary>
/// Cache statistics
/// </summary>
public class CacheStatistics
{
    public long SemanticCacheHits { get; set; }
    public long SemanticCacheMisses { get; set; }
    public double SemanticCacheHitRate { get; set; }
    public int TotalSemanticEntries { get; set; }
    public int TotalEntries => TotalSemanticEntries; // Alias for compatibility
    public DateTime LastUpdated { get; set; }
}
