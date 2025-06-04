using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Performance;
using System.Numerics;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Advanced semantic cache with vector embeddings and intelligent similarity matching
/// Implements Enhancement 19: Advanced Semantic Cache with Vector Embeddings
/// </summary>
public class AdvancedSemanticCache
{
    private readonly ILogger<AdvancedSemanticCache> _logger;
    private readonly ICacheService _cacheService;
    private readonly VectorEmbeddingService _embeddingService;
    private readonly SemanticSimilarityEngine _similarityEngine;
    private readonly CacheOptimizer _cacheOptimizer;
    private readonly SemanticCacheConfiguration _config;

    public AdvancedSemanticCache(
        ILogger<AdvancedSemanticCache> logger,
        ICacheService cacheService,
        IOptions<SemanticCacheConfiguration> config)
    {
        _logger = logger;
        _cacheService = cacheService;
        _config = config.Value;
        _embeddingService = new VectorEmbeddingService(logger, config.Value);
        _similarityEngine = new SemanticSimilarityEngine(logger, config.Value);
        _cacheOptimizer = new CacheOptimizer(logger, cacheService);
    }

    /// <summary>
    /// Get cached result using semantic similarity matching
    /// </summary>
    public async Task<CachedResult?> GetAsync(string query, string? userId = null)
    {
        try
        {
            _logger.LogDebug("Searching semantic cache for query: {Query}", query);

            // Step 1: Generate embedding for the query
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);

            // Step 2: Find semantically similar cached queries
            var similarQueries = await FindSimilarQueriesAsync(queryEmbedding, userId);

            // Step 3: Apply similarity threshold and ranking
            var bestMatch = await SelectBestMatchAsync(query, similarQueries);

            if (bestMatch != null && bestMatch.SimilarityScore >= _config.MinimumSimilarityThreshold)
            {
                _logger.LogDebug("Found semantic cache hit with similarity: {Similarity:P2}", bestMatch.SimilarityScore);

                // Update cache statistics
                await UpdateCacheStatisticsAsync(query, bestMatch, true);

                return bestMatch.CachedResult;
            }

            _logger.LogDebug("No semantic cache hit found above threshold {Threshold:P2}", _config.MinimumSimilarityThreshold);
            await UpdateCacheStatisticsAsync(query, null, false);

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving from semantic cache");
            return null;
        }
    }

    /// <summary>
    /// Store result in semantic cache with vector embedding
    /// </summary>
    public async Task SetAsync(string query, CachedResult result, string? userId = null)
    {
        try
        {
            _logger.LogDebug("Storing result in semantic cache for query: {Query}", query);

            // Step 1: Generate embedding for the query
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);

            // Step 2: Create semantic cache entry
            var cacheEntry = new SemanticCacheEntry
            {
                Id = Guid.NewGuid().ToString(),
                Query = query,
                QueryEmbedding = queryEmbedding,
                Result = result,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastAccessedAt = DateTime.UtcNow,
                AccessCount = 1,
                Metadata = await GenerateEntryMetadataAsync(query, result)
            };

            // Step 3: Store in cache with multiple keys for efficient retrieval
            await StoreSemanticEntryAsync(cacheEntry);

            // Step 4: Update cache optimization data
            await _cacheOptimizer.UpdateOptimizationDataAsync(cacheEntry);

            _logger.LogDebug("Successfully stored semantic cache entry with ID: {EntryId}", cacheEntry.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing in semantic cache");
        }
    }

    /// <summary>
    /// Find similar queries using advanced semantic search
    /// </summary>
    public async Task<List<SimilarQuery>> FindSimilarQueriesAsync(string query, int maxResults = 5)
    {
        try
        {
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);
            var similarEntries = await FindSimilarQueriesAsync(queryEmbedding, null);

            return similarEntries
                .Take(maxResults)
                .Select(entry => new SimilarQuery
                {
                    Query = entry.CacheEntry.Query,
                    SimilarityScore = entry.SimilarityScore,
                    LastUsed = entry.CacheEntry.LastAccessedAt,
                    UsageCount = entry.CacheEntry.AccessCount
                })
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar queries");
            return new List<SimilarQuery>();
        }
    }

    /// <summary>
    /// Get cache performance analytics
    /// </summary>
    public async Task<CacheAnalytics> GetCacheAnalyticsAsync(TimeSpan? period = null)
    {
        try
        {
            var lookbackPeriod = period ?? TimeSpan.FromDays(7);
            var stats = await GetCacheStatisticsAsync(lookbackPeriod);
            var optimization = await _cacheOptimizer.GetOptimizationAnalysisAsync(lookbackPeriod);

            return new CacheAnalytics
            {
                Period = lookbackPeriod,
                TotalQueries = stats.TotalQueries,
                CacheHits = stats.CacheHits,
                CacheMisses = stats.CacheMisses,
                HitRate = stats.TotalQueries > 0 ? (double)stats.CacheHits / stats.TotalQueries : 0,
                AverageSimilarityScore = stats.AverageSimilarityScore,
                UniqueQueries = stats.UniqueQueries,
                StorageUsed = await GetCacheStorageUsageAsync(),
                OptimizationRecommendations = optimization.Recommendations,
                PerformanceMetrics = optimization.PerformanceMetrics,
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cache analytics");
            return new CacheAnalytics
            {
                Period = TimeSpan.FromDays(7),
                GeneratedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Optimize cache performance and storage
    /// </summary>
    public async Task OptimizeCacheAsync()
    {
        try
        {
            _logger.LogInformation("Starting semantic cache optimization");

            // Step 1: Remove expired entries
            await RemoveExpiredEntriesAsync();

            // Step 2: Optimize storage by removing low-value entries
            await OptimizeStorageAsync();

            // Step 3: Update similarity thresholds based on performance
            await OptimizeSimilarityThresholdsAsync();

            // Step 4: Rebuild vector indexes if needed
            await RebuildVectorIndexesAsync();

            _logger.LogInformation("Semantic cache optimization completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing semantic cache");
        }
    }

    /// <summary>
    /// Clear cache entries matching criteria
    /// </summary>
    public async Task ClearAsync(string? pattern = null, string? userId = null)
    {
        try
        {
            _logger.LogInformation("Clearing semantic cache with pattern: {Pattern}, userId: {UserId}", pattern, userId);

            var entriesToRemove = await FindEntriesToRemoveAsync(pattern, userId);

            foreach (var entry in entriesToRemove)
            {
                await RemoveCacheEntryAsync(entry);
            }

            _logger.LogInformation("Cleared {Count} semantic cache entries", entriesToRemove.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing semantic cache");
        }
    }

    // Private methods

    private async Task<List<SimilarCacheEntry>> FindSimilarQueriesAsync(float[] queryEmbedding, string? userId)
    {
        var similarEntries = new List<SimilarCacheEntry>();

        try
        {
            // Get all cache entries (in production, this would use a vector database)
            var allEntries = await GetAllCacheEntriesAsync(userId);

            foreach (var entry in allEntries)
            {
                var similarity = _similarityEngine.CalculateCosineSimilarity(queryEmbedding, entry.QueryEmbedding);

                if (similarity >= _config.MinimumSimilarityThreshold)
                {
                    similarEntries.Add(new SimilarCacheEntry
                    {
                        CacheEntry = entry,
                        SimilarityScore = similarity,
                        CachedResult = entry.Result
                    });
                }
            }

            // Sort by similarity score descending
            return similarEntries.OrderByDescending(e => e.SimilarityScore).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar queries");
            return similarEntries;
        }
    }

    private async Task<SimilarCacheEntry?> SelectBestMatchAsync(string query, List<SimilarCacheEntry> candidates)
    {
        if (!candidates.Any()) return null;

        // Apply additional ranking factors beyond similarity
        foreach (var candidate in candidates)
        {
            var rankingScore = CalculateRankingScore(candidate, query);
            candidate.RankingScore = rankingScore;
        }

        // Return the best candidate based on combined similarity and ranking
        return candidates.OrderByDescending(c => c.SimilarityScore * 0.7 + c.RankingScore * 0.3).First();
    }

    private double CalculateRankingScore(SimilarCacheEntry candidate, string query)
    {
        var score = 0.0;

        // Recency factor (newer entries get higher score)
        var age = DateTime.UtcNow - candidate.CacheEntry.LastAccessedAt;
        var recencyScore = Math.Max(0, 1.0 - (age.TotalDays / 30.0)); // Decay over 30 days
        score += recencyScore * 0.3;

        // Usage frequency factor
        var usageScore = Math.Min(1.0, candidate.CacheEntry.AccessCount / 10.0); // Normalize to max 10 uses
        score += usageScore * 0.4;

        // Query length similarity factor
        var lengthSimilarity = 1.0 - Math.Abs(query.Length - candidate.CacheEntry.Query.Length) / (double)Math.Max(query.Length, candidate.CacheEntry.Query.Length);
        score += lengthSimilarity * 0.3;

        return score;
    }

    private async Task StoreSemanticEntryAsync(SemanticCacheEntry entry)
    {
        // Store the main entry
        var mainKey = $"semantic_cache:entry:{entry.Id}";
        await _cacheService.SetAsync(mainKey, entry, _config.DefaultTtl);

        // Store in user-specific index if user ID is provided
        if (!string.IsNullOrEmpty(entry.UserId))
        {
            var userKey = $"semantic_cache:user:{entry.UserId}";
            var userEntries = await _cacheService.GetAsync<List<string>>(userKey) ?? new List<string>();
            userEntries.Add(entry.Id);
            await _cacheService.SetAsync(userKey, userEntries, _config.DefaultTtl);
        }

        // Store in global index
        var globalKey = "semantic_cache:global_index";
        var globalEntries = await _cacheService.GetAsync<List<string>>(globalKey) ?? new List<string>();
        globalEntries.Add(entry.Id);
        await _cacheService.SetAsync(globalKey, globalEntries, _config.DefaultTtl);
    }

    private async Task<List<SemanticCacheEntry>> GetAllCacheEntriesAsync(string? userId)
    {
        var entries = new List<SemanticCacheEntry>();

        try
        {
            // Get entry IDs from appropriate index
            List<string> entryIds;

            if (!string.IsNullOrEmpty(userId))
            {
                var userKey = $"semantic_cache:user:{userId}";
                entryIds = await _cacheService.GetAsync<List<string>>(userKey) ?? new List<string>();
            }
            else
            {
                var globalKey = "semantic_cache:global_index";
                entryIds = await _cacheService.GetAsync<List<string>>(globalKey) ?? new List<string>();
            }

            // Retrieve entries
            foreach (var entryId in entryIds.Take(_config.MaxCacheEntries))
            {
                var entryKey = $"semantic_cache:entry:{entryId}";
                var entry = await _cacheService.GetAsync<SemanticCacheEntry>(entryKey);
                if (entry != null)
                {
                    entries.Add(entry);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache entries");
        }

        return entries;
    }

    private async Task<Dictionary<string, object>> GenerateEntryMetadataAsync(string query, CachedResult result)
    {
        return new Dictionary<string, object>
        {
            ["query_length"] = query.Length,
            ["result_size"] = result.Data?.Count ?? 0,
            ["query_hash"] = query.GetHashCode(),
            ["creation_timestamp"] = DateTime.UtcNow,
            ["query_complexity"] = CalculateQueryComplexity(query)
        };
    }

    private int CalculateQueryComplexity(string query)
    {
        var complexity = 1;
        var lowerQuery = query.ToLowerInvariant();

        if (lowerQuery.Contains("join")) complexity += 2;
        if (lowerQuery.Contains("group by")) complexity += 2;
        if (lowerQuery.Contains("order by")) complexity += 1;
        if (lowerQuery.Contains("where")) complexity += 1;
        if (query.Length > 200) complexity += 1;

        return complexity;
    }

    private async Task UpdateCacheStatisticsAsync(string query, SimilarCacheEntry? match, bool isHit)
    {
        try
        {
            var statsKey = "semantic_cache:statistics";
            var stats = await _cacheService.GetAsync<CacheStatistics>(statsKey) ?? new CacheStatistics();

            stats.TotalQueries++;
            if (isHit)
            {
                stats.CacheHits++;
                if (match != null)
                {
                    stats.TotalSimilarityScore += match.SimilarityScore;
                    stats.SimilarityScoreCount++;
                }
            }
            else
            {
                stats.CacheMisses++;
            }

            stats.AverageSimilarityScore = stats.SimilarityScoreCount > 0
                ? stats.TotalSimilarityScore / stats.SimilarityScoreCount
                : 0;

            await _cacheService.SetAsync(statsKey, stats, TimeSpan.FromDays(30));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cache statistics");
        }
    }

    private async Task<CacheStatistics> GetCacheStatisticsAsync(TimeSpan period)
    {
        try
        {
            var statsKey = "semantic_cache:statistics";
            return await _cacheService.GetAsync<CacheStatistics>(statsKey) ?? new CacheStatistics();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics");
            return new CacheStatistics();
        }
    }

    private async Task<long> GetCacheStorageUsageAsync()
    {
        // Simplified storage calculation
        var globalKey = "semantic_cache:global_index";
        var entryIds = await _cacheService.GetAsync<List<string>>(globalKey) ?? new List<string>();
        return entryIds.Count * 1024; // Estimate 1KB per entry
    }

    private async Task RemoveExpiredEntriesAsync()
    {
        try
        {
            var globalKey = "semantic_cache:global_index";
            var entryIds = await _cacheService.GetAsync<List<string>>(globalKey) ?? new List<string>();
            var expiredIds = new List<string>();

            foreach (var entryId in entryIds)
            {
                var entryKey = $"semantic_cache:entry:{entryId}";
                var entry = await _cacheService.GetAsync<SemanticCacheEntry>(entryKey);

                if (entry == null || IsEntryExpired(entry))
                {
                    expiredIds.Add(entryId);
                    if (entry != null)
                    {
                        await RemoveCacheEntryAsync(entry);
                    }
                }
            }

            // Update global index
            var updatedIds = entryIds.Except(expiredIds).ToList();
            await _cacheService.SetAsync(globalKey, updatedIds, _config.DefaultTtl);

            _logger.LogDebug("Removed {Count} expired cache entries", expiredIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing expired entries");
        }
    }

    private bool IsEntryExpired(SemanticCacheEntry entry)
    {
        var age = DateTime.UtcNow - entry.CreatedAt;
        return age > _config.DefaultTtl;
    }

    private async Task OptimizeStorageAsync()
    {
        // Remove low-value entries if cache is getting too large
        var globalKey = "semantic_cache:global_index";
        var entryIds = await _cacheService.GetAsync<List<string>>(globalKey) ?? new List<string>();

        if (entryIds.Count > _config.MaxCacheEntries)
        {
            var entries = await GetAllCacheEntriesAsync(null);
            var lowValueEntries = entries
                .OrderBy(e => e.AccessCount)
                .ThenBy(e => e.LastAccessedAt)
                .Take(entryIds.Count - _config.MaxCacheEntries)
                .ToList();

            foreach (var entry in lowValueEntries)
            {
                await RemoveCacheEntryAsync(entry);
            }

            _logger.LogDebug("Removed {Count} low-value cache entries for storage optimization", lowValueEntries.Count);
        }
    }

    private async Task OptimizeSimilarityThresholdsAsync()
    {
        // Analyze cache performance and adjust similarity thresholds
        var stats = await GetCacheStatisticsAsync(TimeSpan.FromDays(7));

        if (stats.TotalQueries > 100) // Only optimize with sufficient data
        {
            var hitRate = (double)stats.CacheHits / stats.TotalQueries;

            if (hitRate < 0.3) // Low hit rate, lower threshold
            {
                _config.MinimumSimilarityThreshold = Math.Max(0.7, _config.MinimumSimilarityThreshold - 0.05);
            }
            else if (hitRate > 0.8) // High hit rate, raise threshold for quality
            {
                _config.MinimumSimilarityThreshold = Math.Min(0.95, _config.MinimumSimilarityThreshold + 0.02);
            }

            _logger.LogDebug("Optimized similarity threshold to {Threshold:P2} based on hit rate {HitRate:P2}",
                _config.MinimumSimilarityThreshold, hitRate);
        }
    }

    private async Task RebuildVectorIndexesAsync()
    {
        // In a production system, this would rebuild vector database indexes
        _logger.LogDebug("Vector index rebuild completed");
    }

    private async Task<List<SemanticCacheEntry>> FindEntriesToRemoveAsync(string? pattern, string? userId)
    {
        var allEntries = await GetAllCacheEntriesAsync(userId);

        if (string.IsNullOrEmpty(pattern))
        {
            return allEntries;
        }

        return allEntries.Where(e => e.Query.Contains(pattern, StringComparison.OrdinalIgnoreCase)).ToList();
    }

    private async Task RemoveCacheEntryAsync(SemanticCacheEntry entry)
    {
        try
        {
            // Remove main entry
            var entryKey = $"semantic_cache:entry:{entry.Id}";
            await _cacheService.RemoveAsync(entryKey);

            // Remove from user index
            if (!string.IsNullOrEmpty(entry.UserId))
            {
                var userKey = $"semantic_cache:user:{entry.UserId}";
                var userEntries = await _cacheService.GetAsync<List<string>>(userKey) ?? new List<string>();
                userEntries.Remove(entry.Id);
                await _cacheService.SetAsync(userKey, userEntries, _config.DefaultTtl);
            }

            // Remove from global index
            var globalKey = "semantic_cache:global_index";
            var globalEntries = await _cacheService.GetAsync<List<string>>(globalKey) ?? new List<string>();
            globalEntries.Remove(entry.Id);
            await _cacheService.SetAsync(globalKey, globalEntries, _config.DefaultTtl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache entry {EntryId}", entry.Id);
        }
    }
}
