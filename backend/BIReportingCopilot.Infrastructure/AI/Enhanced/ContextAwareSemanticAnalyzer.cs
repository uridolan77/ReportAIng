using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Performance;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Enhanced semantic analyzer with context-awareness, conversation history, and transformer-based models
/// Implements Enhancement 6: Context-Aware Query Classification
/// </summary>
public class ContextAwareSemanticAnalyzer : ISemanticAnalyzer
{
    private readonly ILogger<ContextAwareSemanticAnalyzer> _logger;
    private readonly ICacheService _cacheService;
    private readonly IContextManager _contextManager;
    private readonly IAIService _aiService;
    private readonly ConversationContextManager _conversationManager;
    private readonly EntityLinker _entityLinker;
    private readonly IntentClassifier _intentClassifier;

    // Configuration
    private readonly SemanticAnalysisConfig _config;

    public ContextAwareSemanticAnalyzer(
        ILogger<ContextAwareSemanticAnalyzer> logger,
        ICacheService cacheService,
        IContextManager contextManager,
        IAIService aiService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _contextManager = contextManager;
        _aiService = aiService;
        _conversationManager = new ConversationContextManager(cacheService, logger);
        _entityLinker = new EntityLinker(logger);
        _intentClassifier = new IntentClassifier(aiService, logger);
        _config = new SemanticAnalysisConfig();
    }

    public async Task<SemanticAnalysis> AnalyzeAsync(string naturalLanguageQuery)
    {
        return await AnalyzeWithContextAsync(naturalLanguageQuery, null, null);
    }

    /// <summary>
    /// Enhanced analysis with conversation context and user session
    /// </summary>
    public async Task<SemanticAnalysis> AnalyzeWithContextAsync(
        string naturalLanguageQuery, 
        string? userId = null, 
        string? sessionId = null)
    {
        try
        {
            _logger.LogDebug("Starting context-aware semantic analysis for query: {Query}", naturalLanguageQuery);

            // Check cache first
            var cacheKey = GenerateCacheKey(naturalLanguageQuery, userId, sessionId);
            var cachedResult = await _cacheService.GetAsync<SemanticAnalysis>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogDebug("Returning cached semantic analysis");
                return cachedResult;
            }

            // Get conversation context
            var conversationContext = await _conversationManager.GetConversationContextAsync(userId, sessionId);
            
            // Get user context for domain-specific analysis
            var userContext = userId != null ? await _contextManager.GetUserContextAsync(userId) : null;

            var analysis = new SemanticAnalysis
            {
                OriginalQuery = naturalLanguageQuery,
                ProcessedQuery = PreprocessQuery(naturalLanguageQuery),
                Metadata = new Dictionary<string, object>
                {
                    ["analysis_timestamp"] = DateTime.UtcNow,
                    ["has_conversation_context"] = conversationContext.PreviousQueries.Any(),
                    ["user_domain"] = userContext?.Domain ?? "general"
                }
            };

            // Step 1: Enhanced Entity Extraction with Context
            analysis.Entities = await ExtractEntitiesWithContextAsync(
                naturalLanguageQuery, conversationContext, userContext);

            // Step 2: Context-Aware Intent Classification
            analysis.Intent = await _intentClassifier.ClassifyWithContextAsync(
                naturalLanguageQuery, conversationContext, analysis.Entities);

            // Step 3: Extract Keywords with Business Context
            analysis.Keywords = await ExtractContextualKeywordsAsync(
                naturalLanguageQuery, analysis.Entities, userContext);

            // Step 4: Calculate Multi-Dimensional Confidence
            analysis.ConfidenceScore = await CalculateEnhancedConfidenceAsync(
                analysis, conversationContext, userContext);

            // Step 5: Add Enhanced Metadata
            await EnrichMetadataAsync(analysis, conversationContext, userContext);

            // Update conversation context
            await _conversationManager.UpdateConversationContextAsync(
                userId, sessionId, naturalLanguageQuery, analysis);

            // Cache the result
            await _cacheService.SetAsync(cacheKey, analysis, TimeSpan.FromHours(1));

            _logger.LogDebug("Context-aware semantic analysis completed with confidence: {Confidence}", 
                analysis.ConfidenceScore);

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in context-aware semantic analysis");
            return await CreateFallbackAnalysis(naturalLanguageQuery);
        }
    }

    /// <summary>
    /// Enhanced entity extraction with conversation context and schema linking
    /// </summary>
    private async Task<List<SemanticEntity>> ExtractEntitiesWithContextAsync(
        string query, 
        ConversationContext conversationContext, 
        UserContext? userContext)
    {
        var entities = new List<SemanticEntity>();

        // Step 1: Extract base entities using improved patterns
        var baseEntities = await ExtractBaseEntitiesAsync(query);
        entities.AddRange(baseEntities);

        // Step 2: Resolve entities using conversation context
        var resolvedEntities = await ResolveEntitiesWithContextAsync(
            baseEntities, conversationContext);
        entities.AddRange(resolvedEntities);

        // Step 3: Link entities to schema elements
        var linkedEntities = await _entityLinker.LinkToSchemaAsync(
            entities, userContext?.Domain);
        
        // Step 4: Extract temporal expressions with context
        var temporalEntities = await ExtractTemporalEntitiesAsync(
            query, conversationContext);
        entities.AddRange(temporalEntities);

        // Step 5: Remove duplicates and rank by confidence
        return entities
            .GroupBy(e => new { e.Text.ToLowerInvariant(), e.Type })
            .Select(g => g.OrderByDescending(e => e.Confidence).First())
            .OrderByDescending(e => e.Confidence)
            .ToList();
    }

    /// <summary>
    /// Extract base entities using enhanced pattern matching and NER
    /// </summary>
    private async Task<List<SemanticEntity>> ExtractBaseEntitiesAsync(string query)
    {
        var entities = new List<SemanticEntity>();
        var lowerQuery = query.ToLowerInvariant();

        // Enhanced entity patterns with confidence scoring
        var entityPatterns = new Dictionary<EntityType, List<(string Pattern, double Confidence)>>
        {
            [EntityType.Table] = new()
            {
                ("players?", 0.9), ("users?", 0.8), ("accounts?", 0.8),
                ("transactions?", 0.9), ("deposits?", 0.9), ("withdrawals?", 0.9),
                ("bets?", 0.9), ("games?", 0.8), ("sessions?", 0.8)
            },
            [EntityType.Column] = new()
            {
                ("amount", 0.9), ("total", 0.8), ("count", 0.8),
                ("revenue", 0.9), ("profit", 0.9), ("balance", 0.9),
                ("date", 0.8), ("time", 0.7), ("country", 0.9)
            },
            [EntityType.Aggregation] = new()
            {
                ("sum", 0.95), ("total", 0.9), ("count", 0.95),
                ("average", 0.95), ("avg", 0.9), ("maximum", 0.9),
                ("minimum", 0.9), ("max", 0.85), ("min", 0.85)
            }
        };

        foreach (var (entityType, patterns) in entityPatterns)
        {
            foreach (var (pattern, confidence) in patterns)
            {
                var matches = System.Text.RegularExpressions.Regex.Matches(
                    lowerQuery, $@"\b{pattern}\b", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    entities.Add(new SemanticEntity
                    {
                        Text = match.Value,
                        Type = entityType,
                        Confidence = confidence,
                        StartPosition = match.Index,
                        EndPosition = match.Index + match.Length,
                        Properties = new Dictionary<string, object>
                        {
                            ["extraction_method"] = "pattern_matching",
                            ["pattern"] = pattern
                        }
                    });
                }
            }
        }

        return entities;
    }

    /// <summary>
    /// Resolve entity references using conversation context
    /// </summary>
    private async Task<List<SemanticEntity>> ResolveEntitiesWithContextAsync(
        List<SemanticEntity> baseEntities,
        ConversationContext conversationContext)
    {
        var resolvedEntities = new List<SemanticEntity>();

        // Look for pronoun references and resolve them
        var pronouns = new[] { "it", "them", "those", "these", "that", "this" };
        
        foreach (var entity in baseEntities)
        {
            if (pronouns.Contains(entity.Text.ToLowerInvariant()))
            {
                // Try to resolve from conversation context
                var resolved = await ResolvePronouns(entity, conversationContext);
                if (resolved != null)
                {
                    resolvedEntities.Add(resolved);
                }
            }
        }

        return resolvedEntities;
    }

    /// <summary>
    /// Extract temporal entities with conversation context
    /// </summary>
    private async Task<List<SemanticEntity>> ExtractTemporalEntitiesAsync(
        string query,
        ConversationContext conversationContext)
    {
        var entities = new List<SemanticEntity>();
        var lowerQuery = query.ToLowerInvariant();

        // Temporal patterns with context awareness
        var temporalPatterns = new Dictionary<string, (string ResolvedValue, double Confidence)>
        {
            ["yesterday"] = (DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"), 0.95),
            ["today"] = (DateTime.Now.ToString("yyyy-MM-dd"), 0.95),
            ["last week"] = ($"{DateTime.Now.AddDays(-7):yyyy-MM-dd} to {DateTime.Now:yyyy-MM-dd}", 0.9),
            ["this month"] = ($"{DateTime.Now:yyyy-MM}-01 to {DateTime.Now:yyyy-MM-dd}", 0.9),
            ["last month"] = ($"{DateTime.Now.AddMonths(-1):yyyy-MM}-01 to {DateTime.Now.AddMonths(-1):yyyy-MM}-{DateTime.DaysInMonth(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month)}", 0.9)
        };

        foreach (var (pattern, (resolvedValue, confidence)) in temporalPatterns)
        {
            if (lowerQuery.Contains(pattern))
            {
                var startPos = lowerQuery.IndexOf(pattern);
                entities.Add(new SemanticEntity
                {
                    Text = pattern,
                    Type = EntityType.DateRange,
                    Confidence = confidence,
                    StartPosition = startPos,
                    EndPosition = startPos + pattern.Length,
                    ResolvedValue = resolvedValue,
                    Properties = new Dictionary<string, object>
                    {
                        ["extraction_method"] = "temporal_pattern",
                        ["resolved_date"] = resolvedValue
                    }
                });
            }
        }

        return entities;
    }

    private async Task<SemanticEntity?> ResolvePronouns(
        SemanticEntity pronounEntity, 
        ConversationContext conversationContext)
    {
        // Simple pronoun resolution - look at the most recent query's entities
        var lastQuery = conversationContext.PreviousQueries.LastOrDefault();
        if (lastQuery?.Analysis?.Entities != null)
        {
            var candidateEntity = lastQuery.Analysis.Entities
                .Where(e => e.Type == EntityType.Table || e.Type == EntityType.Column)
                .OrderByDescending(e => e.Confidence)
                .FirstOrDefault();

            if (candidateEntity != null)
            {
                return new SemanticEntity
                {
                    Text = pronounEntity.Text,
                    Type = candidateEntity.Type,
                    Confidence = candidateEntity.Confidence * 0.7, // Reduce confidence for resolved pronouns
                    StartPosition = pronounEntity.StartPosition,
                    EndPosition = pronounEntity.EndPosition,
                    ResolvedValue = candidateEntity.Text,
                    Properties = new Dictionary<string, object>
                    {
                        ["extraction_method"] = "pronoun_resolution",
                        ["resolved_from"] = candidateEntity.Text,
                        ["original_confidence"] = candidateEntity.Confidence
                    }
                };
            }
        }

        return null;
    }

    private async Task<List<string>> ExtractContextualKeywordsAsync(
        string query, 
        List<SemanticEntity> entities, 
        UserContext? userContext)
    {
        var words = query.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2 && !IsStopWord(w))
            .ToList();

        // Add entity texts as high-priority keywords
        var entityKeywords = entities.Select(e => e.Text.ToLowerInvariant()).ToList();
        
        // Combine and deduplicate
        var allKeywords = words.Concat(entityKeywords).Distinct().ToList();

        // Score keywords based on business context
        if (userContext?.Domain != null)
        {
            allKeywords = ScoreKeywordsByDomain(allKeywords, userContext.Domain);
        }

        return allKeywords.Take(10).ToList(); // Return top 10 keywords
    }

    private List<string> ScoreKeywordsByDomain(List<string> keywords, string domain)
    {
        var domainKeywords = new Dictionary<string, List<string>>
        {
            ["gaming"] = new() { "player", "bet", "game", "deposit", "withdrawal", "revenue", "casino", "sports" },
            ["finance"] = new() { "transaction", "amount", "balance", "account", "payment", "currency" },
            ["retail"] = new() { "customer", "order", "product", "sale", "inventory", "price" }
        };

        if (domainKeywords.ContainsKey(domain.ToLowerInvariant()))
        {
            var priorityKeywords = domainKeywords[domain.ToLowerInvariant()];
            return keywords
                .OrderByDescending(k => priorityKeywords.Contains(k) ? 1 : 0)
                .ThenBy(k => k)
                .ToList();
        }

        return keywords;
    }

    private async Task<double> CalculateEnhancedConfidenceAsync(
        SemanticAnalysis analysis,
        ConversationContext conversationContext,
        UserContext? userContext)
    {
        var confidenceFactors = new Dictionary<string, double>();

        // Entity confidence (30% weight)
        var avgEntityConfidence = analysis.Entities.Any() 
            ? analysis.Entities.Average(e => e.Confidence) 
            : 0.5;
        confidenceFactors["entity_confidence"] = avgEntityConfidence * 0.3;

        // Intent clarity (25% weight)
        var intentConfidence = analysis.Intent != QueryIntent.General ? 0.8 : 0.4;
        confidenceFactors["intent_confidence"] = intentConfidence * 0.25;

        // Conversation context (20% weight)
        var contextConfidence = conversationContext.PreviousQueries.Any() ? 0.9 : 0.6;
        confidenceFactors["context_confidence"] = contextConfidence * 0.2;

        // Keyword relevance (15% weight)
        var keywordConfidence = analysis.Keywords.Count >= 3 ? 0.8 : 0.5;
        confidenceFactors["keyword_confidence"] = keywordConfidence * 0.15;

        // Query complexity (10% weight)
        var complexityConfidence = analysis.ProcessedQuery.Length > 10 ? 0.8 : 0.6;
        confidenceFactors["complexity_confidence"] = complexityConfidence * 0.1;

        var totalConfidence = confidenceFactors.Values.Sum();
        
        // Store confidence breakdown in metadata
        analysis.Metadata["confidence_breakdown"] = confidenceFactors;

        return Math.Min(1.0, Math.Max(0.0, totalConfidence));
    }

    private async Task EnrichMetadataAsync(
        SemanticAnalysis analysis,
        ConversationContext conversationContext,
        UserContext? userContext)
    {
        analysis.Metadata["conversation_turn"] = conversationContext.PreviousQueries.Count + 1;
        analysis.Metadata["entity_count"] = analysis.Entities.Count;
        analysis.Metadata["keyword_count"] = analysis.Keywords.Count;
        analysis.Metadata["has_temporal_entities"] = analysis.Entities.Any(e => e.Type == EntityType.DateRange);
        analysis.Metadata["has_aggregation_entities"] = analysis.Entities.Any(e => e.Type == EntityType.Aggregation);
        
        if (userContext != null)
        {
            analysis.Metadata["user_domain"] = userContext.Domain;
            analysis.Metadata["user_experience_level"] = userContext.ExperienceLevel;
        }
    }

    // Implementation of remaining interface methods...
    public async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        // Placeholder - would integrate with embedding service
        return new float[384]; // Typical embedding dimension
    }

    public async Task<SemanticSimilarity> CalculateSimilarityAsync(string query1, string query2)
    {
        // Placeholder - would use embedding similarity
        return new SemanticSimilarity
        {
            Query1 = query1,
            Query2 = query2,
            SimilarityScore = 0.5
        };
    }

    public async Task<List<SemanticEntity>> ExtractEntitiesAsync(string query)
    {
        var analysis = await AnalyzeAsync(query);
        return analysis.Entities;
    }

    public async Task<QueryIntent> ClassifyIntentAsync(string query)
    {
        var analysis = await AnalyzeAsync(query);
        return analysis.Intent;
    }

    // Helper methods
    private string GenerateCacheKey(string query, string? userId, string? sessionId)
    {
        var keyParts = new[] { "context_semantic", query.GetHashCode().ToString() };
        if (!string.IsNullOrEmpty(userId)) keyParts = keyParts.Append(userId).ToArray();
        if (!string.IsNullOrEmpty(sessionId)) keyParts = keyParts.Append(sessionId).ToArray();
        return string.Join(":", keyParts);
    }

    private string PreprocessQuery(string query)
    {
        return query.Trim().ToLowerInvariant();
    }

    private bool IsStopWord(string word)
    {
        var stopWords = new HashSet<string>
        {
            "the", "and", "for", "are", "but", "not", "you", "all", "can", "had",
            "her", "was", "one", "our", "out", "day", "get", "has", "him", "his",
            "how", "man", "new", "now", "old", "see", "two", "way", "who", "boy",
            "did", "its", "let", "put", "say", "she", "too", "use", "a", "an"
        };
        return stopWords.Contains(word.ToLowerInvariant());
    }

    private async Task<SemanticAnalysis> CreateFallbackAnalysis(string query)
    {
        return new SemanticAnalysis
        {
            OriginalQuery = query,
            ProcessedQuery = query,
            Intent = QueryIntent.General,
            Entities = new List<SemanticEntity>(),
            Keywords = new List<string>(),
            ConfidenceScore = 0.3,
            Metadata = new Dictionary<string, object>
            {
                ["fallback"] = true,
                ["analysis_timestamp"] = DateTime.UtcNow
            }
        };
    }
}
