using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using BIReportingCopilot.Infrastructure.Data;

namespace BIReportingCopilot.Infrastructure.AI.Caching;

/// <summary>
/// Semantic cache service with vector similarity and intelligent caching strategies
/// Consolidates functionality from multiple cache implementations
/// </summary>
public class SemanticCacheService : ISemanticCacheService
{
    private readonly ILogger<SemanticCacheService> _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly IDbContextFactory _contextFactory;
    private readonly IVectorSearchService? _vectorSearchService;
    private readonly SemanticCacheConfiguration _config;

    public SemanticCacheService(
        ILogger<SemanticCacheService> logger,
        IMemoryCache memoryCache,
        IDbContextFactory contextFactory,
        IVectorSearchService? vectorSearchService = null)
    {
        _logger = logger;
        _memoryCache = memoryCache;
        _contextFactory = contextFactory;
        _vectorSearchService = vectorSearchService;
        _config = new SemanticCacheConfiguration
        {
            SimilarityThreshold = 0.85,
            MaxSimilarQueries = 5,
            DefaultExpiration = TimeSpan.FromHours(2),
            EnableVectorSearch = vectorSearchService != null
        };
    }

    public async Task<QueryResponse?> GetSemanticCacheAsync(string query, double similarityThreshold = 0.8)
    {
        try
        {
            _logger.LogDebug("🔍 Semantic cache lookup for query: {Query}", query);

            // Step 1: Quick exact match check
            var exactMatch = await GetExactMatchAsync(query);
            if (exactMatch != null)
            {
                _logger.LogInformation("🎯 Exact cache hit for query");
                return exactMatch;
            }

            // Step 2: Vector similarity search (if available)
            if (_vectorSearchService != null && _config.EnableVectorSearch)
            {
                var similarResult = await GetSimilarCacheAsync(query, similarityThreshold);
                if (similarResult != null)
                {
                    _logger.LogInformation("🎯 Semantic cache hit with similarity: {Similarity:P2}", similarityThreshold);
                    return similarResult;
                }
            }

            // Step 3: Fallback to database search
            var dbResult = await SearchDatabaseCacheAsync(query, similarityThreshold);
            if (dbResult != null)
            {
                _logger.LogInformation("🎯 Database cache hit for query");
                return dbResult;
            }

            _logger.LogDebug("❌ Semantic cache miss for query");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in semantic cache lookup");
            return null;
        }
    }

    public async Task SetSemanticCacheAsync(string query, QueryResponse response, TimeSpan? expiry = null)
    {
        try
        {
            _logger.LogDebug("💾 Caching query response: {Query}", query);

            var cacheExpiry = expiry ?? _config.DefaultExpiration;

            // Step 1: Cache in memory for fast access
            var memoryCacheKey = GenerateMemoryCacheKey(query);
            _memoryCache.Set(memoryCacheKey, response, cacheExpiry);

            // Step 2: Cache with vector search (if available)
            if (_vectorSearchService != null && _config.EnableVectorSearch)
            {
                try
                {
                    var embedding = await _vectorSearchService.GenerateEmbeddingAsync(query);
                    await _vectorSearchService.StoreQueryEmbeddingAsync(query, response.Sql, response, embedding);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to store in vector cache, continuing with other cache methods");
                }
            }

            // Step 3: Store in database for persistence
            await StoreDatabaseCacheAsync(query, response, cacheExpiry);

            _logger.LogDebug("✅ Query cached successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error caching query response");
        }
    }

    public async Task<List<EnhancedSemanticCacheEntry>> FindSimilarQueriesAsync(string query, int limit = 5)
    {
        try
        {
            var similarEntries = new List<EnhancedSemanticCacheEntry>();

            // Use vector search if available
            if (_vectorSearchService != null && _config.EnableVectorSearch)
            {
                var embedding = await _vectorSearchService.GenerateEmbeddingAsync(query);
                var similarQueries = await _vectorSearchService.FindSimilarQueriesAsync(
                    embedding, _config.SimilarityThreshold, limit);

                similarEntries.AddRange(similarQueries.Select(sq => new EnhancedSemanticCacheEntry
                {
                    Id = sq.EmbeddingId,
                    OriginalQuery = sq.OriginalQuery,
                    SqlQuery = sq.SqlQuery,
                    SerializedResponse = JsonSerializer.Serialize(sq.CachedResponse),
                    ConfidenceScore = sq.SimilarityScore,
                    CreatedAt = DateTime.UtcNow,
                    LastAccessedAt = DateTime.UtcNow
                }));
            }

            // Fallback to database search
            if (similarEntries.Count < limit)
            {
                var dbEntries = await FindSimilarInDatabaseAsync(query, limit - similarEntries.Count);
                similarEntries.AddRange(dbEntries);
            }

            return similarEntries.Take(limit).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar queries");
            return new List<EnhancedSemanticCacheEntry>();
        }
    }

    public async Task<SemanticCacheStatistics> GetCacheStatisticsAsync()
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                var stats = new SemanticCacheStatistics
                {
                    TotalEntries = 0,
                    HitCount = 0,
                    MissCount = 0,
                    TotalSizeBytes = 0,
                    LastUpdated = DateTime.UtcNow
                };

                // Get database statistics
                if (context is BICopilotContext dbContext)
                {
                    stats.TotalEntries = await dbContext.SemanticCacheEntries.CountAsync();
                    
                    // Calculate approximate size (simplified)
                    var entries = await dbContext.SemanticCacheEntries
                        .Select(e => e.SerializedResponse.Length)
                        .ToListAsync();
                    stats.TotalSizeBytes = entries.Sum();
                }

                // Add memory cache statistics (simplified)
                if (_memoryCache is MemoryCache mc)
                {
                    // Memory cache statistics would be added here
                    // This is a simplified implementation
                }

                return stats;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics");
            return new SemanticCacheStatistics { LastUpdated = DateTime.UtcNow };
        }
    }

    public async Task ClearSemanticCacheAsync()
    {
        try
        {
            _logger.LogInformation("🗑️ Clearing semantic cache");

            // Clear memory cache
            if (_memoryCache is MemoryCache mc)
            {
                mc.Clear();
            }

            // Clear vector search cache
            if (_vectorSearchService != null)
            {
                await _vectorSearchService.ClearIndexAsync();
            }

            // Clear database cache
            await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                if (context is BICopilotContext dbContext)
                {
                    dbContext.SemanticCacheEntries.RemoveRange(dbContext.SemanticCacheEntries);
                    await dbContext.SaveChangesAsync();
                }
            });

            _logger.LogInformation("✅ Semantic cache cleared successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error clearing semantic cache");
        }
    }

    public async Task InvalidateCacheByPatternAsync(string pattern)
    {
        try
        {
            _logger.LogInformation("🗑️ Invalidating cache entries matching pattern: {Pattern}", pattern);

            // Invalidate in database
            await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                if (context is BICopilotContext dbContext)
                {
                    var entriesToRemove = await dbContext.SemanticCacheEntries
                        .Where(e => EF.Functions.Like(e.OriginalQuery, $"%{pattern}%"))
                        .ToListAsync();

                    dbContext.SemanticCacheEntries.RemoveRange(entriesToRemove);
                    await dbContext.SaveChangesAsync();

                    _logger.LogInformation("🗑️ Invalidated {Count} cache entries", entriesToRemove.Count);
                }
            });

            // Invalidate in vector search
            if (_vectorSearchService != null)
            {
                await _vectorSearchService.InvalidateByPatternAsync(pattern);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error invalidating cache by pattern: {Pattern}", pattern);
        }
    }

    public async Task<EnhancedSemanticCacheEntry?> GetCacheEntryAsync(string key)
    {
        try
        {
            // Try memory cache first
            var memoryCacheKey = GenerateMemoryCacheKey(key);
            if (_memoryCache.TryGetValue(memoryCacheKey, out QueryResponse? response))
            {
                return new EnhancedSemanticCacheEntry
                {
                    Id = GenerateQueryHash(key),
                    OriginalQuery = key,
                    SerializedResponse = JsonSerializer.Serialize(response),
                    CreatedAt = DateTime.UtcNow,
                    LastAccessedAt = DateTime.UtcNow
                };
            }

            // Try database
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                if (context is BICopilotContext dbContext)
                {
                    var entry = await dbContext.SemanticCacheEntries
                        .FirstOrDefaultAsync(e => e.OriginalQuery == key);

                    if (entry != null)
                    {
                        return new EnhancedSemanticCacheEntry
                        {
                            Id = entry.Id,
                            OriginalQuery = entry.OriginalQuery,
                            SqlQuery = entry.SqlQuery,
                            SerializedResponse = entry.SerializedResponse,
                            CreatedAt = entry.CreatedAt,
                            LastAccessedAt = entry.LastAccessedAt ?? DateTime.UtcNow
                        };
                    }
                }

                return null;
            });
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
            await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                if (context is BICopilotContext dbContext)
                {
                    var entry = await dbContext.SemanticCacheEntries
                        .FirstOrDefaultAsync(e => e.OriginalQuery == key);

                    if (entry != null)
                    {
                        entry.Metadata = JsonSerializer.Serialize(metadata);
                        entry.LastAccessedAt = DateTime.UtcNow;
                        await dbContext.SaveChangesAsync();
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cache entry metadata: {Key}", key);
        }
    }

    public async Task InvalidateByDataChangeAsync(string tableName, string changeType)
    {
        try
        {
            _logger.LogInformation("🔄 Invalidating cache due to data change - Table: {Table}, Change: {Change}",
                tableName, changeType);
            var pattern = $"*{tableName}*";
            await InvalidateCacheByPatternAsync(pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache by data change");
        }
    }

    public async Task CleanupExpiredEntriesAsync()
    {
        try
        {
            await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                if (context is BICopilotContext dbContext)
                {
                    var expiredEntries = await dbContext.SemanticCacheEntries
                        .Where(e => e.ExpiresAt < DateTime.UtcNow)
                        .ToListAsync();
                    dbContext.SemanticCacheEntries.RemoveRange(expiredEntries);
                    await dbContext.SaveChangesAsync();
                    _logger.LogInformation("🗑️ Cleaned up {Count} expired cache entries", expiredEntries.Count);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cleaning up expired cache entries");
        }
    }

    public string GenerateQueryHash(string query) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(query)));

    private string GenerateMemoryCacheKey(string query) =>
        $"semantic_cache_{Convert.ToBase64String(Encoding.UTF8.GetBytes(query.Trim().ToLowerInvariant()))[..16]}";
}
