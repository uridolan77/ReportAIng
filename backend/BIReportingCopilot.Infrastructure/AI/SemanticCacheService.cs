using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// Enhanced semantic cache service using bounded contexts for better performance and maintainability
/// Uses QueryDbContext for semantic cache entries and query history
/// </summary>
public class SemanticCacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDbContextFactory _contextFactory;
    private readonly ILogger<SemanticCacheService> _logger;
    private readonly SemanticCacheConfiguration _config;
    private readonly QuerySimilarityAnalyzer _similarityAnalyzer;

    public SemanticCacheService(
        IMemoryCache memoryCache,
        IDbContextFactory contextFactory,
        ILogger<SemanticCacheService> logger)
    {
        _memoryCache = memoryCache;
        _contextFactory = contextFactory;
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

            // Store in database for persistence and similarity search using QueryDbContext
            await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                var queryContext = (QueryDbContext)context;
                queryContext.SemanticCacheEntries.Add(cacheEntry);
                await queryContext.SaveChangesAsync();
            });

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
            await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                var queryContext = (QueryDbContext)context;
                var affectedEntries = await queryContext.SemanticCacheEntries
                    .Where(e => e.GeneratedSql != null && e.GeneratedSql.Contains(tableName, StringComparison.OrdinalIgnoreCase))
                    .ToListAsync();

                foreach (var entry in affectedEntries)
                {
                    entry.ExpiresAt = DateTime.UtcNow; // Mark as expired
                    _memoryCache.Remove($"semantic_{entry.QueryHash}");
                }

                await queryContext.SaveChangesAsync();

                _logger.LogInformation("Invalidated {Count} cache entries for table {TableName} due to {ChangeType}",
                    affectedEntries.Count, tableName, changeType);
            });
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
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                var queryContext = (QueryDbContext)context;
                var stats = await queryContext.SemanticCacheEntries
                    .Where(e => e.ExpiresAt > DateTime.UtcNow)
                    .GroupBy(e => 1)
                    .Select(g => new
                    {
                        TotalEntries = g.Count(),
                        TotalAccesses = g.Sum(e => e.AccessCount),
                        AverageAge = g.Average(e => EF.Functions.DateDiffHour(e.CreatedAt, DateTime.UtcNow))
                    })
                    .FirstOrDefaultAsync();

                var hitRate = await CalculateHitRateAsync(queryContext);

                return new SemanticCacheStatistics
                {
                    TotalEntries = stats?.TotalEntries ?? 0,
                    TotalAccesses = stats?.TotalAccesses ?? 0,
                    AverageAgeHours = stats?.AverageAge ?? 0,
                    HitRate = hitRate,
                    MemoryCacheSize = GetMemoryCacheSize(),
                    LastUpdated = DateTime.UtcNow
                };
            });
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
            await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                var queryContext = (QueryDbContext)context;
                var expiredEntries = await queryContext.SemanticCacheEntries
                    .Where(e => e.ExpiresAt <= DateTime.UtcNow)
                    .ToListAsync();

                queryContext.SemanticCacheEntries.RemoveRange(expiredEntries);
                await queryContext.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} expired cache entries", expiredEntries.Count);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired cache entries");
        }
    }

    private async Task<List<SimilarQueryResult>> FindSimilarQueriesAsync(string naturalLanguageQuery, string sqlQuery)
    {
        var currentFeatures = await _similarityAnalyzer.ExtractSemanticsAsync(naturalLanguageQuery, sqlQuery);

        var candidateEntries = await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
        {
            var queryContext = (QueryDbContext)context;
            return await queryContext.SemanticCacheEntries
                .Where(e => e.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(e => e.LastAccessedAt)
                .Take(100) // Limit for performance
                .ToListAsync();
        });

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
            // Save access count updates using QueryDbContext
            await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                var queryContext = (QueryDbContext)context;
                await queryContext.SaveChangesAsync();
            });
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

    private async Task<double> CalculateHitRateAsync(QueryDbContext queryContext)
    {
        try
        {
            var totalQueries = await queryContext.QueryHistories.CountAsync();
            var cacheHits = await queryContext.SemanticCacheEntries.SumAsync(e => e.AccessCount);

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

// Interface removed - using Core.Interfaces.ISemanticCacheService instead

/// <summary>
/// Semantic cache configuration
/// </summary>
public class SemanticCacheConfiguration
{
    public double MinimumSimilarityThreshold { get; set; } = 0.75;
    public double SimilarityThreshold { get; set; } = 0.75; // Alias for compatibility
    public int MaxSimilarQueries { get; set; } = 5;
    public string EmbeddingModel { get; set; } = "text-embedding-ada-002";
    public TimeSpan DefaultExpiry { get; set; } = TimeSpan.FromHours(24);
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromHours(24); // Alias for compatibility
    public int MaxCacheEntries { get; set; } = 10000;
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromHours(6);
}

/// <summary>
/// Semantic cache result
/// </summary>
public class SemanticCacheResult
{
    public QueryResponse QueryResponse { get; set; } = new();
    public QueryResponse? CachedResponse { get; set; } // Alias for compatibility
    public double SimilarityScore { get; set; }
    public string OriginalQuery { get; set; } = string.Empty;
    public DateTime CacheTimestamp { get; set; }
    public bool IsSemanticMatch { get; set; }
    public bool IsHit { get; set; }
    public List<string> SimilarQueries { get; set; } = new();
    public TimeSpan LookupTime { get; set; }
    public string CacheStrategy { get; set; } = "Semantic";
    public Dictionary<string, object> Metadata { get; set; } = new();
    public object? CacheEntry { get; set; }
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

    // Additional properties expected by Infrastructure layer
    public int HitCount { get; set; }
    public int MissCount { get; set; }
    public long TotalSizeBytes { get; set; }
    public int TotalRequests { get; set; }
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

/// <summary>
/// Query similarity analyzer for semantic understanding
/// </summary>
public class QuerySimilarityAnalyzer
{
    private readonly ILogger _logger;

    public QuerySimilarityAnalyzer(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Extract semantic features from a query
    /// </summary>
    public Task<SemanticFeatures> ExtractSemanticsAsync(string naturalLanguageQuery, string sqlQuery)
    {
        try
        {
            var intent = ClassifyIntent(naturalLanguageQuery);
            var complexity = CalculateComplexity(sqlQuery);

            var features = new SemanticFeatures
            {
                Keywords = ExtractKeywords(naturalLanguageQuery),
                Entities = ExtractEntities(naturalLanguageQuery),
                Intent = intent,
                Intents = new List<string> { intent }, // Populate both for compatibility
                Complexity = complexity,
                ComplexityScore = complexity, // Populate both for compatibility
                TableReferences = ExtractTableReferences(sqlQuery),
                ColumnReferences = ExtractColumnReferences(sqlQuery),
                QueryType = DetermineQueryType(sqlQuery),
                Timestamp = DateTime.UtcNow
            };

            return Task.FromResult(features);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting semantic features");
            return Task.FromResult(new SemanticFeatures());
        }
    }

    /// <summary>
    /// Calculate similarity between two semantic feature sets
    /// </summary>
    public double CalculateSimilarity(SemanticFeatures features1, SemanticFeatures features2)
    {
        try
        {
            var keywordSimilarity = CalculateKeywordSimilarity(features1.Keywords, features2.Keywords);
            var entitySimilarity = CalculateEntitySimilarity(features1.Entities, features2.Entities);
            var intentSimilarity = features1.Intent == features2.Intent ? 1.0 : 0.0;
            var tableSimilarity = CalculateTableSimilarity(features1.TableReferences, features2.TableReferences);

            // Weighted average of different similarity components
            var similarity = (keywordSimilarity * 0.3) +
                           (entitySimilarity * 0.3) +
                           (intentSimilarity * 0.2) +
                           (tableSimilarity * 0.2);

            return Math.Max(0.0, Math.Min(1.0, similarity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating similarity");
            return 0.0;
        }
    }

    private List<string> ExtractKeywords(string query)
    {
        var keywords = new List<string>();
        var words = query.ToLowerInvariant()
            .Split(new[] { ' ', '\t', '\n', '\r', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries);

        var importantWords = words.Where(w => w.Length > 2 && !IsStopWord(w)).ToList();
        keywords.AddRange(importantWords);

        return keywords.Distinct().ToList();
    }

    private List<string> ExtractEntities(string query)
    {
        var entities = new List<string>();
        var lowerQuery = query.ToLowerInvariant();

        // Simple entity extraction - look for common business terms
        var businessTerms = new[] { "revenue", "profit", "sales", "customer", "order", "product", "deposit", "withdrawal", "player", "bet", "win" };
        entities.AddRange(businessTerms.Where(term => lowerQuery.Contains(term)));

        return entities.Distinct().ToList();
    }

    private string ClassifyIntent(string query)
    {
        var lowerQuery = query.ToLowerInvariant();

        if (lowerQuery.Contains("show") || lowerQuery.Contains("list") || lowerQuery.Contains("display"))
            return "display";
        if (lowerQuery.Contains("count") || lowerQuery.Contains("how many"))
            return "count";
        if (lowerQuery.Contains("sum") || lowerQuery.Contains("total"))
            return "aggregate";
        if (lowerQuery.Contains("average") || lowerQuery.Contains("mean"))
            return "average";
        if (lowerQuery.Contains("compare") || lowerQuery.Contains("vs"))
            return "compare";

        return "general";
    }

    private double CalculateComplexity(string sqlQuery)
    {
        var complexity = 0.0;
        var lowerSql = sqlQuery.ToLowerInvariant();

        // Count joins
        complexity += System.Text.RegularExpressions.Regex.Matches(lowerSql, @"\bjoin\b").Count * 0.2;

        // Count subqueries
        complexity += System.Text.RegularExpressions.Regex.Matches(lowerSql, @"\bselect\b").Count * 0.1;

        // Count aggregations
        complexity += System.Text.RegularExpressions.Regex.Matches(lowerSql, @"\b(sum|count|avg|max|min)\b").Count * 0.1;

        return Math.Min(1.0, complexity);
    }

    private List<string> ExtractTableReferences(string sqlQuery)
    {
        var tables = new List<string>();
        var matches = System.Text.RegularExpressions.Regex.Matches(
            sqlQuery, @"\bFROM\s+(\w+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            tables.Add(match.Groups[1].Value.ToLowerInvariant());
        }

        return tables.Distinct().ToList();
    }

    private List<string> ExtractColumnReferences(string sqlQuery)
    {
        var columns = new List<string>();
        // Simple column extraction - this could be enhanced
        var matches = System.Text.RegularExpressions.Regex.Matches(
            sqlQuery, @"\bSELECT\s+(.*?)\s+FROM", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var columnList = match.Groups[1].Value;
            var columnNames = columnList.Split(',').Select(c => c.Trim()).ToList();
            columns.AddRange(columnNames);
        }

        return columns.Distinct().ToList();
    }

    private string DetermineQueryType(string sqlQuery)
    {
        var lowerSql = sqlQuery.ToLowerInvariant();

        if (lowerSql.StartsWith("select"))
            return "select";
        if (lowerSql.StartsWith("insert"))
            return "insert";
        if (lowerSql.StartsWith("update"))
            return "update";
        if (lowerSql.StartsWith("delete"))
            return "delete";

        return "unknown";
    }

    private double CalculateKeywordSimilarity(List<string> keywords1, List<string> keywords2)
    {
        if (!keywords1.Any() && !keywords2.Any()) return 1.0;
        if (!keywords1.Any() || !keywords2.Any()) return 0.0;

        var intersection = keywords1.Intersect(keywords2).Count();
        var union = keywords1.Union(keywords2).Count();

        return (double)intersection / union;
    }

    private double CalculateEntitySimilarity(List<string> entities1, List<string> entities2)
    {
        if (!entities1.Any() && !entities2.Any()) return 1.0;
        if (!entities1.Any() || !entities2.Any()) return 0.0;

        var intersection = entities1.Intersect(entities2).Count();
        var union = entities1.Union(entities2).Count();

        return (double)intersection / union;
    }

    private double CalculateTableSimilarity(List<string> tables1, List<string> tables2)
    {
        if (!tables1.Any() && !tables2.Any()) return 1.0;
        if (!tables1.Any() || !tables2.Any()) return 0.0;

        var intersection = tables1.Intersect(tables2).Count();
        var union = tables1.Union(tables2).Count();

        return (double)intersection / union;
    }

    private bool IsStopWord(string word)
    {
        var stopWords = new[] { "the", "and", "or", "but", "in", "on", "at", "to", "for", "of", "with", "by", "is", "are", "was", "were", "be", "been", "have", "has", "had", "do", "does", "did", "will", "would", "could", "should", "may", "might", "can", "a", "an" };
        return stopWords.Contains(word);
    }
}

/// <summary>
/// Semantic features extracted from queries
/// </summary>
public class SemanticFeatures
{
    public List<string> Keywords { get; set; } = new();
    public List<string> Entities { get; set; } = new();
    public string Intent { get; set; } = string.Empty;
    public List<string> Intents { get; set; } = new();
    public double Complexity { get; set; }
    public double ComplexityScore { get; set; }
    public List<string> TableReferences { get; set; } = new();
    public List<string> ColumnReferences { get; set; } = new();
    public string QueryType { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}


