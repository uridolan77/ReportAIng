using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for query caching operations
/// </summary>
public interface IQueryCacheService
{
    // Cache Operations
    Task<QueryResponse?> GetCachedQueryAsync(string queryHash);
    Task<SemanticCacheResult?> GetSemanticallySimilarQueryAsync(string naturalLanguageQuery, string sqlQuery);
    Task CacheQueryAsync(string queryHash, QueryResponse response, TimeSpan? expiry = null);
    Task CacheSemanticQueryAsync(string naturalLanguageQuery, string sqlQuery, QueryResponse response, TimeSpan? expiry = null);

    // Cache Invalidation
    Task InvalidateCacheAsync(string queryHash);
    Task InvalidateByPatternAsync(string pattern);
    Task InvalidateByDataChangeAsync(string tableName, string changeType);

    // Cache Management
    Task CleanupExpiredEntriesAsync();
    Task WarmupCacheAsync(List<string> commonQueries);

    // Utility Methods
    string GenerateQueryHash(string query);
    string GenerateQueryHash(string naturalLanguageQuery, string sqlQuery);
}
