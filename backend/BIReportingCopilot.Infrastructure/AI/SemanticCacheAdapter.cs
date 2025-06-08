using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// Adapter that implements ISemanticCacheService using ICacheService
/// Note: This class is not currently used in the unified service architecture
/// </summary>
public class SemanticCacheAdapter
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<SemanticCacheAdapter> _logger;

    public SemanticCacheAdapter(ICacheService cacheService, ILogger<SemanticCacheAdapter> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<SemanticCacheResult?> GetSemanticallySimilarAsync(string naturalLanguageQuery, string sqlQuery)
    {
        try
        {
            // For now, use regular cache key since semantic search is complex
            var cacheKey = $"semantic:{naturalLanguageQuery.GetHashCode()}";
            var cached = await _cacheService.GetAsync<QueryResponse>(cacheKey);

            if (cached != null)
            {
                return new SemanticCacheResult
                {
                    QueryResponse = cached,
                    SimilarityScore = 1.0,
                    OriginalQuery = naturalLanguageQuery,
                    CacheTimestamp = DateTime.UtcNow,
                    IsSemanticMatch = false
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting semantically similar cache for query {Query}", naturalLanguageQuery);
            return null;
        }
    }

    public async Task CacheSemanticQueryAsync(string naturalLanguageQuery, string sqlQuery, QueryResponse response, TimeSpan? expiry = null)
    {
        try
        {
            var cacheKey = $"semantic:{naturalLanguageQuery.GetHashCode()}";
            await _cacheService.SetAsync(cacheKey, response, expiry ?? TimeSpan.FromMinutes(30));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching semantic query {Query}", naturalLanguageQuery);
        }
    }

    public async Task InvalidateByDataChangeAsync(string tableName, string changeType)
    {
        try
        {
            // Basic implementation - remove all semantic cache entries
            _logger.LogInformation("Invalidating semantic cache for table {TableName} due to {ChangeType}", tableName, changeType);
            await _cacheService.RemovePatternAsync("semantic:*");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating semantic cache for table {TableName}", tableName);
        }
    }

    public async Task<SemanticCacheStatistics> GetCacheStatisticsAsync()
    {
        try
        {
            var stats = await _cacheService.GetStatisticsAsync();
            return new SemanticCacheStatistics
            {
                TotalEntries = (int)stats.TotalKeys,
                TotalAccesses = (int)stats.TotalRequests,
                AverageAgeHours = 0,
                HitRate = stats.HitRatio,
                MemoryCacheSize = (int)stats.MemoryUsage,
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting semantic cache statistics");
            return new SemanticCacheStatistics
            {
                TotalEntries = 0,
                TotalAccesses = 0,
                AverageAgeHours = 0,
                HitRate = 0.0,
                MemoryCacheSize = 0,
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    public async Task CleanupExpiredEntriesAsync()
    {
        try
        {
            _logger.LogInformation("Cleaning up expired semantic cache entries");
            // Basic implementation - cache service should handle TTL automatically
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired semantic cache entries");
        }
    }
}
