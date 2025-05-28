using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// Semantic cache service that understands query similarity and provides intelligent caching
/// </summary>
public class SemanticCacheService : ISemanticCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly BICopilotContext _context;
    private readonly ILogger<SemanticCacheService> _logger;
    private readonly QuerySimilarityAnalyzer _similarityAnalyzer;
    private readonly SemanticCacheConfiguration _config;

    public SemanticCacheService(
        IMemoryCache memoryCache,
        BICopilotContext context,
        ILogger<SemanticCacheService> logger)
    {
        _memoryCache = memoryCache;
        _context = context;
        _logger = logger;
        _similarityAnalyzer = new QuerySimilarityAnalyzer(logger);
        _config = new SemanticCacheConfiguration();
    }

    /// <summary>
    /// Get cached result for a semantically similar query
    /// </summary>
    public async Task<SemanticCacheResult?> GetSemanticallySimilarAsync(string naturalLanguageQuery, string sqlQuery)
    {
        try
        {
            var querySignature = GenerateQuerySignature(naturalLanguageQuery, sqlQuery);

            // First check exact match in memory cache
            if (_memoryCache.TryGetValue($"semantic_{querySignature}", out SemanticCacheResult? exactMatch))
            {
                _logger.LogDebug("Exact semantic cache hit for query signature: {Signature}", querySignature);
                return exactMatch;
            }

            // Check for similar queries in database
            var similarQueries = await FindSimilarQueriesAsync(naturalLanguageQuery, sqlQuery);

            if (similarQueries.Any())
            {
                var bestMatch = similarQueries.OrderByDescending(q => q.SimilarityScore).First();

                if (bestMatch.SimilarityScore >= _config.MinimumSimilarityThreshold)
                {
                    _logger.LogInformation("Semantic cache hit with similarity {Score:P2} for query: {Query}",
                        bestMatch.SimilarityScore, naturalLanguageQuery);

                    var cacheResult = new SemanticCacheResult
                    {
                        QueryResponse = JsonSerializer.Deserialize<QueryResponse>(bestMatch.CachedResponse)!,
                        SimilarityScore = bestMatch.SimilarityScore,
                        OriginalQuery = bestMatch.NaturalLanguageQuery,
                        CacheTimestamp = bestMatch.Timestamp,
                        IsSemanticMatch = true
                    };

                    // Cache in memory for faster subsequent access
                    _memoryCache.Set($"semantic_{querySignature}", cacheResult, TimeSpan.FromMinutes(30));

                    return cacheResult;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving semantic cache for query: {Query}", naturalLanguageQuery);
            return null;
        }
    }

    /// <summary>
    /// Cache a query result with semantic indexing
    /// </summary>
    public async Task CacheSemanticQueryAsync(
        string naturalLanguageQuery,
        string sqlQuery,
        QueryResponse response,
        TimeSpan? expiry = null)
    {
        try
        {
            var querySignature = GenerateQuerySignature(naturalLanguageQuery, sqlQuery);
            var semanticFeatures = await _similarityAnalyzer.ExtractSemanticsAsync(naturalLanguageQuery, sqlQuery);

            var cacheEntry = new Core.Models.SemanticCacheEntry
            {
                QueryHash = querySignature,
                OriginalQuery = naturalLanguageQuery,
                NormalizedQuery = sqlQuery,
                GeneratedSql = sqlQuery,
                ResultData = JsonSerializer.Serialize(response),
                ResultMetadata = JsonSerializer.Serialize(semanticFeatures),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.Add(expiry ?? TimeSpan.FromHours(24)),
                AccessCount = 1,
                LastAccessedAt = DateTime.UtcNow
            };

            // Store in database for persistence and similarity search
            _context.SemanticCacheEntries.Add(cacheEntry);
            await _context.SaveChangesAsync();

            // Store in memory cache for fast access
            var cacheResult = new SemanticCacheResult
            {
                QueryResponse = response,
                SimilarityScore = 1.0,
                OriginalQuery = naturalLanguageQuery,
                CacheTimestamp = DateTime.UtcNow,
                IsSemanticMatch = false
            };

            _memoryCache.Set($"semantic_{querySignature}", cacheResult, expiry ?? TimeSpan.FromHours(1));

            _logger.LogInformation("Cached semantic query with signature: {Signature}", querySignature);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching semantic query: {Query}", naturalLanguageQuery);
        }
    }

    /// <summary>
    /// Invalidate cache entries based on data changes
    /// </summary>
    public async Task InvalidateByDataChangeAsync(string tableName, string changeType)
    {
        try
        {
            var affectedEntries = await _context.SemanticCacheEntries
                .Where(e => e.GeneratedSql != null && e.GeneratedSql.Contains(tableName, StringComparison.OrdinalIgnoreCase))
                .ToListAsync();

            foreach (var entry in affectedEntries)
            {
                entry.ExpiresAt = DateTime.UtcNow; // Mark as expired
                _memoryCache.Remove($"semantic_{entry.QueryHash}");
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Invalidated {Count} cache entries for table {TableName} due to {ChangeType}",
                affectedEntries.Count, tableName, changeType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache for table: {TableName}", tableName);
        }
    }

    /// <summary>
    /// Get cache statistics for monitoring
    /// </summary>
    public async Task<SemanticCacheStatistics> GetCacheStatisticsAsync()
    {
        try
        {
            var stats = await _context.SemanticCacheEntries
                .Where(e => e.ExpiresAt > DateTime.UtcNow)
                .GroupBy(e => 1)
                .Select(g => new
                {
                    TotalEntries = g.Count(),
                    TotalAccesses = g.Sum(e => e.AccessCount),
                    AverageAge = g.Average(e => EF.Functions.DateDiffHour(e.CreatedAt, DateTime.UtcNow))
                })
                .FirstOrDefaultAsync();

            var hitRate = await CalculateHitRateAsync();

            return new SemanticCacheStatistics
            {
                TotalEntries = stats?.TotalEntries ?? 0,
                TotalAccesses = stats?.TotalAccesses ?? 0,
                AverageAgeHours = stats?.AverageAge ?? 0,
                HitRate = hitRate,
                MemoryCacheSize = GetMemoryCacheSize(),
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics");
            return new SemanticCacheStatistics();
        }
    }

    /// <summary>
    /// Clean up expired cache entries
    /// </summary>
    public async Task CleanupExpiredEntriesAsync()
    {
        try
        {
            var expiredEntries = await _context.SemanticCacheEntries
                .Where(e => e.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();

            _context.SemanticCacheEntries.RemoveRange(expiredEntries);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Cleaned up {Count} expired cache entries", expiredEntries.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired cache entries");
        }
    }

    private async Task<List<SimilarQueryResult>> FindSimilarQueriesAsync(string naturalLanguageQuery, string sqlQuery)
    {
        var currentFeatures = await _similarityAnalyzer.ExtractSemanticsAsync(naturalLanguageQuery, sqlQuery);

        var candidateEntries = await _context.SemanticCacheEntries
            .Where(e => e.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(e => e.LastAccessedAt)
            .Take(100) // Limit for performance
            .ToListAsync();

        var similarQueries = new List<SimilarQueryResult>();

        foreach (var entry in candidateEntries)
        {
            try
            {
                var entryFeatures = JsonSerializer.Deserialize<SemanticFeatures>(entry.ResultMetadata ?? "{}");
                var similarity = _similarityAnalyzer.CalculateSimilarity(currentFeatures, entryFeatures!);

                if (similarity >= _config.MinimumSimilarityThreshold)
                {
                    similarQueries.Add(new SimilarQueryResult
                    {
                        NaturalLanguageQuery = entry.OriginalQuery,
                        SqlQuery = entry.GeneratedSql ?? "",
                        CachedResponse = entry.ResultData ?? "",
                        SimilarityScore = similarity,
                        Timestamp = entry.CreatedAt
                    });

                    // Update access statistics
                    entry.AccessCount++;
                    entry.LastAccessedAt = DateTime.UtcNow;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error calculating similarity for cache entry {Id}", entry.Id);
            }
        }

        if (similarQueries.Any())
        {
            await _context.SaveChangesAsync(); // Save access count updates
        }

        return similarQueries;
    }

    private string GenerateQuerySignature(string naturalLanguageQuery, string sqlQuery)
    {
        var combined = $"{naturalLanguageQuery}|{sqlQuery}";
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
        return Convert.ToBase64String(hash)[..16]; // Take first 16 characters
    }

    private async Task<double> CalculateHitRateAsync()
    {
        try
        {
            var totalQueries = await _context.QueryHistories.CountAsync();
            var cacheHits = await _context.SemanticCacheEntries.SumAsync(e => e.AccessCount);

            return totalQueries > 0 ? (double)cacheHits / totalQueries : 0.0;
        }
        catch
        {
            return 0.0;
        }
    }

    private int GetMemoryCacheSize()
    {
        // This is a simplified approach - in production, you might want to use a more sophisticated method
        if (_memoryCache is MemoryCache mc)
        {
            var field = typeof(MemoryCache).GetField("_coherentState",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field?.GetValue(mc) is object coherentState)
            {
                var entriesCollection = coherentState.GetType()
                    .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (entriesCollection?.GetValue(coherentState) is System.Collections.IDictionary entries)
                {
                    return entries.Count;
                }
            }
        }
        return 0;
    }
}

/// <summary>
/// Interface for semantic cache service
/// </summary>
public interface ISemanticCacheService
{
    Task<SemanticCacheResult?> GetSemanticallySimilarAsync(string naturalLanguageQuery, string sqlQuery);
    Task CacheSemanticQueryAsync(string naturalLanguageQuery, string sqlQuery, QueryResponse response, TimeSpan? expiry = null);
    Task InvalidateByDataChangeAsync(string tableName, string changeType);
    Task<SemanticCacheStatistics> GetCacheStatisticsAsync();
    Task CleanupExpiredEntriesAsync();
}

/// <summary>
/// Semantic cache configuration
/// </summary>
public class SemanticCacheConfiguration
{
    public double MinimumSimilarityThreshold { get; set; } = 0.75;
    public TimeSpan DefaultExpiry { get; set; } = TimeSpan.FromHours(24);
    public int MaxCacheEntries { get; set; } = 10000;
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(6);
}

/// <summary>
/// Semantic cache result
/// </summary>
public class SemanticCacheResult
{
    public QueryResponse QueryResponse { get; set; } = new();
    public double SimilarityScore { get; set; }
    public string OriginalQuery { get; set; } = string.Empty;
    public DateTime CacheTimestamp { get; set; }
    public bool IsSemanticMatch { get; set; }
}

/// <summary>
/// Semantic cache statistics
/// </summary>
public class SemanticCacheStatistics
{
    public int TotalEntries { get; set; }
    public int TotalAccesses { get; set; }
    public double AverageAgeHours { get; set; }
    public double HitRate { get; set; }
    public int MemoryCacheSize { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Similar query result for cache lookup
/// </summary>
public class SimilarQueryResult
{
    public string NaturalLanguageQuery { get; set; } = string.Empty;
    public string SqlQuery { get; set; } = string.Empty;
    public string CachedResponse { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public DateTime Timestamp { get; set; }
}


