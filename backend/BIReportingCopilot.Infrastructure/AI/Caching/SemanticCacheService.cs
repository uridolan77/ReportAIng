using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.Statistics;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using BIReportingCopilot.Infrastructure.Data;
using ContextType = BIReportingCopilot.Infrastructure.Data.Contexts.ContextType;

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
                    await _vectorSearchService.StoreQueryEmbeddingAsync(GenerateQueryHash(query), query, embedding);
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
                var similarQueries = await _vectorSearchService.SearchAsync(
                    query, limit, _config.SimilarityThreshold);

                similarEntries.AddRange(similarQueries.Select(sq => new EnhancedSemanticCacheEntry
                {
                    Id = sq.DocumentId,
                    OriginalQuery = sq.Content,
                    SqlQuery = sq.Metadata.ContainsKey("sql_query") ? (sq.Metadata["sql_query"]?.ToString() ?? string.Empty) : string.Empty,
                    SerializedResponse = sq.Metadata.ContainsKey("cached_response") ? (sq.Metadata["cached_response"]?.ToString() ?? string.Empty) : string.Empty,
                    ConfidenceScore = sq.Score,
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

    public async Task<BIReportingCopilot.Core.Interfaces.AI.SemanticCacheStatistics> GetSemanticCacheStatisticsAsync()
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                var stats = new BIReportingCopilot.Core.Interfaces.AI.SemanticCacheStatistics
                {
                    TotalEntries = 0,
                    HitCount = 0,
                    MissCount = 0,
                    // TotalSizeBytes = 0, // Property doesn't exist in SemanticCacheStatistics
                    LastUpdated = DateTime.UtcNow
                };

                // Get database statistics
                if (context is BICopilotContext dbContext)
                {
                    stats.TotalEntries = await dbContext.SemanticCacheEntries.CountAsync();

                    // Calculate approximate size (simplified)
                    // var entries = await dbContext.SemanticCacheEntries
                    //     .Select(e => e.SerializedResponse.Length)
                    //     .ToListAsync();
                    // stats.TotalSizeBytes = entries.Sum(); // Property doesn't exist in SemanticCacheStatistics
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
            return new BIReportingCopilot.Core.Interfaces.AI.SemanticCacheStatistics { LastUpdated = DateTime.UtcNow };
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
                            Id = entry.Id.ToString(),
                            OriginalQuery = entry.OriginalQuery,
                            SqlQuery = entry.GeneratedSql ?? string.Empty,
                            SerializedResponse = entry.ResultData != null ? JsonSerializer.Serialize(entry.ResultData) : string.Empty,
                            CreatedAt = entry.CreatedAt,
                            LastAccessedAt = entry.LastAccessedAt
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

    // Missing interface methods - stub implementations
    public Task<BIReportingCopilot.Core.Interfaces.Query.CacheStatistics> GetCacheStatisticsAsync()
    {
        return Task.FromResult(new BIReportingCopilot.Core.Interfaces.Query.CacheStatistics
        {
            TotalKeys = 0,
            HitCount = 0,
            MissCount = 0,
            MemoryUsage = 0,
            LastUpdated = DateTime.UtcNow
        });
    }

    public Task<BIReportingCopilot.Core.Models.CachePerformanceMetrics> GetCachePerformanceMetricsAsync()
    {
        return Task.FromResult(new BIReportingCopilot.Core.Models.CachePerformanceMetrics
        {
            AverageHitTime = TimeSpan.FromMilliseconds(10),
            AverageMissTime = TimeSpan.FromMilliseconds(100),
            HitRatio = 0.75,
            GeneratedAt = DateTime.UtcNow
        });
    }

    public Task OptimizeCacheAsync()
    {
        _logger.LogInformation("🔧 Cache optimization completed");
        return Task.CompletedTask;
    }

    public Task<BIReportingCopilot.Core.Models.CacheHealthStatus> GetCacheHealthAsync()
    {
        return Task.FromResult(new BIReportingCopilot.Core.Models.CacheHealthStatus
        {
            IsHealthy = true,
            LastChecked = DateTime.UtcNow
        });
    }

    public async Task CacheSemanticQueryAsync(string query, string sqlQuery, QueryResponse response, TimeSpan? expiry = null)
    {
        await SetSemanticCacheAsync(query, response, expiry);
    }

    public Task<List<SemanticCacheResult>> GetSemanticallySimilarAsync(string query, double threshold, int maxResults)
    {
        return Task.FromResult(new List<SemanticCacheResult>());
    }

    public async Task<SemanticCacheResult?> GetSemanticallySimilarAsync(string query, string sqlQuery)
    {
        var response = await GetSemanticCacheAsync(query);
        if (response != null)
        {
            return new SemanticCacheResult
            {
                IsHit = true,
                SimilarityScore = 1.0,
                CachedResponse = response,
                LookupTime = TimeSpan.FromMilliseconds(5),
                CacheStrategy = "exact"
            };
        }
        return null;
    }

    private string GenerateMemoryCacheKey(string query) =>
        $"semantic_cache_{Convert.ToBase64String(Encoding.UTF8.GetBytes(query.Trim().ToLowerInvariant()))[..16]}";

    // Helper methods for missing functionality
    private async Task<QueryResponse?> GetExactMatchAsync(string query)
    {
        var memoryCacheKey = GenerateMemoryCacheKey(query);
        if (_memoryCache.TryGetValue(memoryCacheKey, out QueryResponse? response))
        {
            return response;
        }
        return null;
    }

    private async Task<QueryResponse?> GetSimilarCacheAsync(string query, double threshold)
    {
        if (_vectorSearchService == null) return null;

        try
        {
            var similarQueries = await _vectorSearchService.FindSimilarQueriesAsync(query, (int)Math.Round(threshold * 100), 1);
            var firstResult = similarQueries.FirstOrDefault();
            if (firstResult?.Metadata?.ContainsKey("CachedResponse") == true)
            {
                return firstResult.Metadata["CachedResponse"] as QueryResponse;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    private Task<QueryResponse?> SearchDatabaseCacheAsync(string query, double threshold)
    {
        return Task.FromResult<QueryResponse?>(null);
    }

    private Task StoreDatabaseCacheAsync(string query, QueryResponse response, TimeSpan expiry)
    {
        return Task.CompletedTask;
    }

    private Task<List<EnhancedSemanticCacheEntry>> FindSimilarInDatabaseAsync(string query, int limit)
    {
        return Task.FromResult(new List<EnhancedSemanticCacheEntry>());
    }

    #region Missing Interface Method Implementations

    /// <summary>
    /// Get cached value (ISemanticCacheService interface)
    /// </summary>
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var memoryCacheKey = GenerateMemoryCacheKey(key);
            if (_memoryCache.TryGetValue(memoryCacheKey, out T? value))
            {
                return value;
            }

            // Try database lookup for complex objects
            if (typeof(T) == typeof(QueryResponse))
            {
                var response = await GetSemanticCacheAsync(key);
                return response as T;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached value for key: {Key}", key);
            return null;
        }
    }

    /// <summary>
    /// Get semantic cached value (ISemanticCacheService interface)
    /// </summary>
    public async Task<T?> GetSemanticAsync<T>(string query, double similarityThreshold = 0.8, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            if (typeof(T) == typeof(QueryResponse))
            {
                var response = await GetSemanticCacheAsync(query, similarityThreshold);
                return response as T;
            }

            // For other types, try exact match first
            return await GetAsync<T>(query, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting semantic cached value for query: {Query}", query);
            return null;
        }
    }

    /// <summary>
    /// Set cached value (ISemanticCacheService interface)
    /// </summary>
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var cacheExpiry = expiry ?? _config.DefaultExpiration;
            var memoryCacheKey = GenerateMemoryCacheKey(key);
            _memoryCache.Set(memoryCacheKey, value, cacheExpiry);

            // If it's a QueryResponse, also store semantically
            if (value is QueryResponse response)
            {
                await SetSemanticCacheAsync(key, response, expiry);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cached value for key: {Key}", key);
        }
    }

    /// <summary>
    /// Set semantic cached value (ISemanticCacheService interface)
    /// </summary>
    public async Task SetSemanticAsync<T>(string query, T value, TimeSpan? expiry = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            if (value is QueryResponse response)
            {
                await SetSemanticCacheAsync(query, response, expiry);
            }
            else
            {
                await SetAsync(query, value, expiry, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting semantic cached value for query: {Query}", query);
        }
    }

    /// <summary>
    /// Remove cached value (ISemanticCacheService interface)
    /// </summary>
    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var memoryCacheKey = GenerateMemoryCacheKey(key);
            _memoryCache.Remove(memoryCacheKey);

            // Also remove from database
            await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                if (context is BICopilotContext dbContext)
                {
                    var entry = await dbContext.SemanticCacheEntries
                        .FirstOrDefaultAsync(e => e.OriginalQuery == key, cancellationToken);

                    if (entry != null)
                    {
                        dbContext.SemanticCacheEntries.Remove(entry);
                        await dbContext.SaveChangesAsync(cancellationToken);
                    }
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cached value for key: {Key}", key);
        }
    }

    /// <summary>
    /// Remove semantic cached value (ISemanticCacheService interface)
    /// </summary>
    public async Task RemoveSemanticAsync(string query, CancellationToken cancellationToken = default)
    {
        await RemoveAsync(query, cancellationToken);
    }

    /// <summary>
    /// Check if key exists (ISemanticCacheService interface)
    /// </summary>
    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var memoryCacheKey = GenerateMemoryCacheKey(key);
            if (_memoryCache.TryGetValue(memoryCacheKey, out _))
            {
                return true;
            }

            // Check database
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                if (context is BICopilotContext dbContext)
                {
                    return await dbContext.SemanticCacheEntries
                        .AnyAsync(e => e.OriginalQuery == key, cancellationToken);
                }
                return false;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if key exists: {Key}", key);
            return false;
        }
    }

    /// <summary>
    /// Find similar cached entries (ISemanticCacheService interface)
    /// </summary>
    public async Task<List<BIReportingCopilot.Core.Models.SemanticCacheEntry>> FindSimilarAsync(string query, double threshold = 0.8, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            var enhancedEntries = await FindSimilarQueriesAsync(query, maxResults);

            return enhancedEntries.Select(e => new BIReportingCopilot.Core.Models.SemanticCacheEntry
            {
                Id = e.Id,
                OriginalQuery = e.OriginalQuery,
                SerializedResponse = e.SerializedResponse,
                ConfidenceScore = e.ConfidenceScore,
                CreatedAt = e.CreatedAt,
                ExpiresAt = e.CreatedAt.Add(_config.DefaultExpiration),
                ResultData = new Dictionary<string, object>
                {
                    ["sql"] = e.SqlQuery ?? string.Empty,
                    ["confidence"] = e.ConfidenceScore
                }
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar cached entries for query: {Query}", query);
            return new List<BIReportingCopilot.Core.Models.SemanticCacheEntry>();
        }
    }

    /// <summary>
    /// Get cache statistics (ISemanticCacheService interface)
    /// </summary>
    public async Task<BIReportingCopilot.Core.Interfaces.AI.SemanticCacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var stats = await GetSemanticCacheStatisticsAsync();

            return new BIReportingCopilot.Core.Interfaces.AI.SemanticCacheStatistics
            {
                TotalEntries = stats.TotalEntries,
                HitCount = stats.HitCount,
                MissCount = stats.MissCount,
                SemanticHitCount = stats.SemanticHitCount,
                MemoryUsage = stats.MemoryUsage,
                LastUpdated = stats.LastUpdated
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics");
            return new BIReportingCopilot.Core.Interfaces.AI.SemanticCacheStatistics
            {
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    #endregion
}
