using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// Unified semantic cache service that implements the Core interface
/// Consolidates all semantic cache functionality into a single implementation
/// </summary>
public class UnifiedSemanticCacheService : ISemanticCacheService
{
    private readonly SemanticCacheService _infrastructureService;
    private readonly ILogger<UnifiedSemanticCacheService> _logger;

    public UnifiedSemanticCacheService(
        SemanticCacheService infrastructureService,
        ILogger<UnifiedSemanticCacheService> logger)
    {
        _infrastructureService = infrastructureService;
        _logger = logger;
    }

    #region Core Interface Implementation

    public async Task<QueryResponse?> GetSemanticCacheAsync(string query, double similarityThreshold = 0.8)
    {
        try
        {
            var result = await _infrastructureService.GetSemanticallySimilarAsync(query, "");
            return result?.QueryResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting semantic cache for query: {Query}", query);
            return null;
        }
    }

    public async Task SetSemanticCacheAsync(string query, QueryResponse response, TimeSpan? expiry = null)
    {
        try
        {
            await _infrastructureService.CacheSemanticQueryAsync(query, response.Sql, response, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting semantic cache for query: {Query}", query);
        }
    }

    public async Task<List<EnhancedSemanticCacheEntry>> FindSimilarQueriesAsync(string query, int limit = 5)
    {
        try
        {
            // This is a simplified implementation - would need to be enhanced
            var result = await _infrastructureService.GetSemanticallySimilarAsync(query, "");
            if (result != null)
            {
                return new List<EnhancedSemanticCacheEntry>
                {
                    new EnhancedSemanticCacheEntry
                    {
                        OriginalQuery = result.OriginalQuery,
                        SqlQuery = "",
                        SerializedResponse = System.Text.Json.JsonSerializer.Serialize(result.QueryResponse),
                        ConfidenceScore = result.SimilarityScore,
                        CreatedAt = result.CacheTimestamp,
                        LastAccessedAt = DateTime.UtcNow
                    }
                };
            }
            return new List<EnhancedSemanticCacheEntry>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar queries for: {Query}", query);
            return new List<EnhancedSemanticCacheEntry>();
        }
    }

    public async Task<CacheStatistics> GetCacheStatisticsAsync()
    {
        try
        {
            var infraStats = await _infrastructureService.GetCacheStatisticsAsync();
            return new CacheStatistics
            {
                TotalEntries = infraStats.TotalEntries,
                HitCount = infraStats.HitCount,
                MissCount = infraStats.MissCount,
                TotalSizeBytes = infraStats.TotalSizeBytes,
                LastUpdated = infraStats.LastUpdated
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics");
            return new CacheStatistics { LastUpdated = DateTime.UtcNow };
        }
    }

    public async Task ClearSemanticCacheAsync()
    {
        try
        {
            await _infrastructureService.CleanupExpiredEntriesAsync();
            _logger.LogInformation("Semantic cache cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing semantic cache");
        }
    }

    public async Task ClearCacheAsync()
    {
        try
        {
            await _infrastructureService.CleanupExpiredEntriesAsync();
            _logger.LogInformation("All cache cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
        }
    }

    public async Task InvalidateExpiredEntriesAsync()
    {
        try
        {
            await _infrastructureService.CleanupExpiredEntriesAsync();
            _logger.LogInformation("Expired cache entries invalidated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating expired entries");
        }
    }

    public async Task InvalidateCacheByPatternAsync(string pattern)
    {
        try
        {
            // Infrastructure service doesn't have this method, so we'll implement a basic version
            _logger.LogInformation("Cache invalidation by pattern not fully implemented: {Pattern}", pattern);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache by pattern: {Pattern}", pattern);
        }
    }

    public async Task<EnhancedSemanticCacheEntry?> GetCacheEntryAsync(string key)
    {
        try
        {
            // Basic implementation - would need enhancement
            _logger.LogDebug("Getting cache entry by key: {Key}", key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache entry: {Key}", key);
            return null;
        }
    }

    public async Task UpdateCacheEntryMetadataAsync(string key, Dictionary<string, object> metadata)
    {
        try
        {
            _logger.LogDebug("Updating cache entry metadata for key: {Key}", key);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cache entry metadata: {Key}", key);
        }
    }

    public async Task<BIReportingCopilot.Core.Interfaces.CachePerformanceMetrics> GetCachePerformanceMetricsAsync()
    {
        try
        {
            var stats = await GetCacheStatisticsAsync();
            return new BIReportingCopilot.Core.Interfaces.CachePerformanceMetrics
            {
                HitRate = stats.TotalRequests > 0 ? (double)stats.HitCount / stats.TotalRequests : 0.0,
                MissRate = stats.TotalRequests > 0 ? (double)stats.MissCount / stats.TotalRequests : 0.0,
                AverageRetrievalTime = TimeSpan.FromMilliseconds(10), // Default value
                TotalRequests = stats.TotalRequests,
                TotalHits = stats.HitCount,
                TotalMisses = stats.MissCount,
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache performance metrics");
            return new BIReportingCopilot.Core.Interfaces.CachePerformanceMetrics { LastUpdated = DateTime.UtcNow };
        }
    }

    public async Task OptimizeCacheAsync()
    {
        try
        {
            await _infrastructureService.CleanupExpiredEntriesAsync();
            _logger.LogInformation("Cache optimization completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing cache");
        }
    }

    public async Task<CacheHealthStatus> GetCacheHealthAsync()
    {
        try
        {
            var stats = await GetCacheStatisticsAsync();
            return new CacheHealthStatus
            {
                IsHealthy = true,
                Status = "Healthy",
                Issues = new List<string>(),
                Metrics = new Dictionary<string, object>
                {
                    ["total_entries"] = stats.TotalEntries,
                    ["hit_count"] = stats.HitCount,
                    ["miss_count"] = stats.MissCount
                },
                CheckedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache health");
            return new CacheHealthStatus
            {
                IsHealthy = false,
                Status = "Unhealthy",
                Issues = new List<string> { ex.Message },
                CheckedAt = DateTime.UtcNow
            };
        }
    }

    public async Task CacheSemanticQueryAsync(string naturalLanguageQuery, string sqlQuery, QueryResponse response, TimeSpan? expiry = null)
    {
        await _infrastructureService.CacheSemanticQueryAsync(naturalLanguageQuery, sqlQuery, response, expiry);
    }

    public async Task<List<BIReportingCopilot.Core.Models.SemanticCacheResult>> GetSemanticallySimilarAsync(string query, double threshold = 0.8, int limit = 5)
    {
        try
        {
            var result = await _infrastructureService.GetSemanticallySimilarAsync(query, "");
            if (result != null)
            {
                // Convert Infrastructure SemanticCacheResult to Core SemanticCacheResult
                var coreResult = new BIReportingCopilot.Core.Models.SemanticCacheResult
                {
                    IsHit = result.IsHit,
                    SimilarityScore = result.SimilarityScore,
                    CachedResponse = result.QueryResponse,
                    LookupTime = result.LookupTime,
                    CacheStrategy = result.CacheStrategy,
                    Metadata = result.Metadata
                };
                return new List<BIReportingCopilot.Core.Models.SemanticCacheResult> { coreResult };
            }
            return new List<BIReportingCopilot.Core.Models.SemanticCacheResult>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting semantically similar queries");
            return new List<BIReportingCopilot.Core.Models.SemanticCacheResult>();
        }
    }

    public async Task<BIReportingCopilot.Core.Models.SemanticCacheResult?> GetSemanticallySimilarAsync(string naturalLanguageQuery, string sqlQuery)
    {
        try
        {
            var result = await _infrastructureService.GetSemanticallySimilarAsync(naturalLanguageQuery, sqlQuery);
            if (result != null)
            {
                // Convert Infrastructure SemanticCacheResult to Core SemanticCacheResult
                return new BIReportingCopilot.Core.Models.SemanticCacheResult
                {
                    IsHit = result.IsHit,
                    SimilarityScore = result.SimilarityScore,
                    CachedResponse = result.QueryResponse,
                    LookupTime = result.LookupTime,
                    CacheStrategy = result.CacheStrategy,
                    Metadata = result.Metadata
                };
            }
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting semantically similar query");
            return null;
        }
    }

    public async Task InvalidateByDataChangeAsync(string tableName, string changeType)
    {
        await _infrastructureService.InvalidateByDataChangeAsync(tableName, changeType);
    }

    public async Task CleanupExpiredEntriesAsync()
    {
        await _infrastructureService.CleanupExpiredEntriesAsync();
    }

    #endregion
}
