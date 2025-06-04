using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Performance;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Manages conversation context for enhanced query understanding
/// Tracks query history, user patterns, and session state
/// </summary>
public class ConversationContextManager
{
    private readonly ICacheService _cacheService;
    private readonly ILogger _logger;
    private readonly TimeSpan _contextTtl = TimeSpan.FromHours(2);
    private readonly int _maxContextQueries = 10;

    public ConversationContextManager(ICacheService cacheService, ILogger logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Get conversation context for a user session
    /// </summary>
    public async Task<ConversationContext> GetConversationContextAsync(string? userId, string? sessionId)
    {
        try
        {
            var contextKey = GenerateContextKey(userId, sessionId);
            var cachedContext = await _cacheService.GetAsync<ConversationContext>(contextKey);
            
            if (cachedContext != null)
            {
                _logger.LogDebug("Retrieved conversation context with {QueryCount} previous queries", 
                    cachedContext.PreviousQueries.Count);
                return cachedContext;
            }

            // Create new context
            var newContext = new ConversationContext
            {
                UserId = userId,
                SessionId = sessionId,
                StartTime = DateTime.UtcNow,
                PreviousQueries = new List<QueryContextEntry>(),
                UserPatterns = new Dictionary<string, object>(),
                SessionMetadata = new Dictionary<string, object>()
            };

            await _cacheService.SetAsync(contextKey, newContext, _contextTtl);
            _logger.LogDebug("Created new conversation context for user: {UserId}, session: {SessionId}", 
                userId, sessionId);

            return newContext;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversation context");
            return CreateEmptyContext(userId, sessionId);
        }
    }

    /// <summary>
    /// Update conversation context with new query and analysis
    /// </summary>
    public async Task UpdateConversationContextAsync(
        string? userId, 
        string? sessionId, 
        string query, 
        SemanticAnalysis analysis)
    {
        try
        {
            var context = await GetConversationContextAsync(userId, sessionId);
            
            // Add new query to context
            var queryEntry = new QueryContextEntry
            {
                Query = query,
                Analysis = analysis,
                Timestamp = DateTime.UtcNow,
                QueryIndex = context.PreviousQueries.Count
            };

            context.PreviousQueries.Add(queryEntry);

            // Maintain context size limit
            if (context.PreviousQueries.Count > _maxContextQueries)
            {
                context.PreviousQueries.RemoveAt(0);
                // Reindex remaining queries
                for (int i = 0; i < context.PreviousQueries.Count; i++)
                {
                    context.PreviousQueries[i].QueryIndex = i;
                }
            }

            // Update user patterns
            await UpdateUserPatternsAsync(context, analysis);

            // Update session metadata
            UpdateSessionMetadata(context, analysis);

            // Save updated context
            var contextKey = GenerateContextKey(userId, sessionId);
            await _cacheService.SetAsync(contextKey, context, _contextTtl);

            _logger.LogDebug("Updated conversation context. Total queries: {QueryCount}", 
                context.PreviousQueries.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating conversation context");
        }
    }

    /// <summary>
    /// Analyze user patterns from conversation history
    /// </summary>
    private async Task UpdateUserPatternsAsync(ConversationContext context, SemanticAnalysis analysis)
    {
        // Track common intents
        var intentCounts = context.PreviousQueries
            .GroupBy(q => q.Analysis?.Intent ?? QueryIntent.General)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        context.UserPatterns["intent_frequency"] = intentCounts;

        // Track common entities
        var entityCounts = context.PreviousQueries
            .SelectMany(q => q.Analysis?.Entities ?? new List<SemanticEntity>())
            .GroupBy(e => e.Text.ToLowerInvariant())
            .Where(g => g.Count() > 1)
            .ToDictionary(g => g.Key, g => g.Count());

        context.UserPatterns["common_entities"] = entityCounts;

        // Track query complexity trends
        var complexityScores = context.PreviousQueries
            .Select(q => q.Analysis?.ConfidenceScore ?? 0.5)
            .ToList();

        if (complexityScores.Any())
        {
            context.UserPatterns["avg_confidence"] = complexityScores.Average();
            context.UserPatterns["confidence_trend"] = CalculateTrend(complexityScores);
        }

        // Track temporal patterns
        var temporalQueries = context.PreviousQueries
            .Where(q => q.Analysis?.Entities?.Any(e => e.Type == EntityType.DateRange) == true)
            .Count();

        context.UserPatterns["temporal_query_ratio"] = context.PreviousQueries.Count > 0 
            ? (double)temporalQueries / context.PreviousQueries.Count 
            : 0.0;
    }

    /// <summary>
    /// Update session-level metadata
    /// </summary>
    private void UpdateSessionMetadata(ConversationContext context, SemanticAnalysis analysis)
    {
        context.SessionMetadata["last_update"] = DateTime.UtcNow;
        context.SessionMetadata["total_queries"] = context.PreviousQueries.Count;
        context.SessionMetadata["session_duration"] = DateTime.UtcNow - context.StartTime;

        // Track session characteristics
        var avgConfidence = context.PreviousQueries
            .Select(q => q.Analysis?.ConfidenceScore ?? 0.5)
            .DefaultIfEmpty(0.5)
            .Average();

        context.SessionMetadata["avg_session_confidence"] = avgConfidence;

        // Identify session focus
        var dominantIntent = context.PreviousQueries
            .GroupBy(q => q.Analysis?.Intent ?? QueryIntent.General)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? QueryIntent.General;

        context.SessionMetadata["dominant_intent"] = dominantIntent.ToString();

        // Track entity diversity
        var uniqueEntities = context.PreviousQueries
            .SelectMany(q => q.Analysis?.Entities ?? new List<SemanticEntity>())
            .Select(e => e.Text.ToLowerInvariant())
            .Distinct()
            .Count();

        context.SessionMetadata["entity_diversity"] = uniqueEntities;
    }

    /// <summary>
    /// Calculate trend direction for a series of values
    /// </summary>
    private string CalculateTrend(List<double> values)
    {
        if (values.Count < 2) return "stable";

        var firstHalf = values.Take(values.Count / 2).Average();
        var secondHalf = values.Skip(values.Count / 2).Average();

        var difference = secondHalf - firstHalf;
        var threshold = 0.1; // 10% change threshold

        if (Math.Abs(difference) < threshold) return "stable";
        return difference > 0 ? "improving" : "declining";
    }

    /// <summary>
    /// Get similar queries from conversation history
    /// </summary>
    public async Task<List<QueryContextEntry>> GetSimilarQueriesAsync(
        string? userId, 
        string? sessionId, 
        string currentQuery, 
        int maxResults = 3)
    {
        try
        {
            var context = await GetConversationContextAsync(userId, sessionId);
            
            // Simple similarity based on keyword overlap
            var currentKeywords = ExtractKeywords(currentQuery);
            
            var similarQueries = context.PreviousQueries
                .Select(q => new
                {
                    Query = q,
                    Similarity = CalculateKeywordSimilarity(currentKeywords, ExtractKeywords(q.Query))
                })
                .Where(x => x.Similarity > 0.3) // 30% similarity threshold
                .OrderByDescending(x => x.Similarity)
                .Take(maxResults)
                .Select(x => x.Query)
                .ToList();

            _logger.LogDebug("Found {SimilarCount} similar queries for current query", similarQueries.Count);
            return similarQueries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting similar queries");
            return new List<QueryContextEntry>();
        }
    }

    /// <summary>
    /// Clear conversation context for a user session
    /// </summary>
    public async Task ClearConversationContextAsync(string? userId, string? sessionId)
    {
        try
        {
            var contextKey = GenerateContextKey(userId, sessionId);
            await _cacheService.RemoveAsync(contextKey);
            _logger.LogDebug("Cleared conversation context for user: {UserId}, session: {SessionId}", 
                userId, sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing conversation context");
        }
    }

    /// <summary>
    /// Get conversation statistics for analytics
    /// </summary>
    public async Task<ConversationStats> GetConversationStatsAsync(string? userId, string? sessionId)
    {
        try
        {
            var context = await GetConversationContextAsync(userId, sessionId);
            
            return new ConversationStats
            {
                TotalQueries = context.PreviousQueries.Count,
                SessionDuration = DateTime.UtcNow - context.StartTime,
                AverageConfidence = context.PreviousQueries
                    .Select(q => q.Analysis?.ConfidenceScore ?? 0.5)
                    .DefaultIfEmpty(0.5)
                    .Average(),
                DominantIntent = context.PreviousQueries
                    .GroupBy(q => q.Analysis?.Intent ?? QueryIntent.General)
                    .OrderByDescending(g => g.Count())
                    .FirstOrDefault()?.Key ?? QueryIntent.General,
                UniqueEntities = context.PreviousQueries
                    .SelectMany(q => q.Analysis?.Entities ?? new List<SemanticEntity>())
                    .Select(e => e.Text.ToLowerInvariant())
                    .Distinct()
                    .Count(),
                UserPatterns = context.UserPatterns
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting conversation stats");
            return new ConversationStats();
        }
    }

    // Helper methods
    private string GenerateContextKey(string? userId, string? sessionId)
    {
        var keyParts = new List<string> { "conversation_context" };
        if (!string.IsNullOrEmpty(userId)) keyParts.Add(userId);
        if (!string.IsNullOrEmpty(sessionId)) keyParts.Add(sessionId);
        else keyParts.Add("default_session");
        
        return string.Join(":", keyParts);
    }

    private ConversationContext CreateEmptyContext(string? userId, string? sessionId)
    {
        return new ConversationContext
        {
            UserId = userId,
            SessionId = sessionId,
            StartTime = DateTime.UtcNow,
            PreviousQueries = new List<QueryContextEntry>(),
            UserPatterns = new Dictionary<string, object>(),
            SessionMetadata = new Dictionary<string, object>()
        };
    }

    private List<string> ExtractKeywords(string query)
    {
        return query.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2)
            .Distinct()
            .ToList();
    }

    private double CalculateKeywordSimilarity(List<string> keywords1, List<string> keywords2)
    {
        if (!keywords1.Any() || !keywords2.Any()) return 0.0;

        var intersection = keywords1.Intersect(keywords2).Count();
        var union = keywords1.Union(keywords2).Count();

        return union > 0 ? (double)intersection / union : 0.0;
    }
}

/// <summary>
/// Conversation context data structure
/// </summary>
public class ConversationContext
{
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public DateTime StartTime { get; set; }
    public List<QueryContextEntry> PreviousQueries { get; set; } = new();
    public Dictionary<string, object> UserPatterns { get; set; } = new();
    public Dictionary<string, object> SessionMetadata { get; set; } = new();
}

/// <summary>
/// Individual query entry in conversation context
/// </summary>
public class QueryContextEntry
{
    public string Query { get; set; } = string.Empty;
    public SemanticAnalysis? Analysis { get; set; }
    public DateTime Timestamp { get; set; }
    public int QueryIndex { get; set; }
}

/// <summary>
/// Conversation statistics for analytics
/// </summary>
public class ConversationStats
{
    public int TotalQueries { get; set; }
    public TimeSpan SessionDuration { get; set; }
    public double AverageConfidence { get; set; }
    public QueryIntent DominantIntent { get; set; }
    public int UniqueEntities { get; set; }
    public Dictionary<string, object> UserPatterns { get; set; } = new();
}

/// <summary>
/// Configuration for semantic analysis
/// </summary>
public class SemanticAnalysisConfig
{
    public double MinimumEntityConfidence { get; set; } = 0.6;
    public double MinimumIntentConfidence { get; set; } = 0.7;
    public int MaxContextQueries { get; set; } = 10;
    public TimeSpan ContextTtl { get; set; } = TimeSpan.FromHours(2);
    public bool EnableConversationContext { get; set; } = true;
    public bool EnableEntityLinking { get; set; } = true;
}
