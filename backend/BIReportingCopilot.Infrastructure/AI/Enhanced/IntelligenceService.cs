using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Intelligence Service combining Advanced NLU and Schema Optimization
/// Provides comprehensive query analysis with semantic understanding and performance optimization
/// </summary>
public class IntelligenceService : IQueryIntelligenceService
{
    private readonly ILogger<IntelligenceService> _logger;
    private readonly IAdvancedNLUService _nluService;
    private readonly ISchemaOptimizationService _schemaService;
    private readonly IAIService _aiService;
    private readonly ISchemaService _schemaMetadataService;

    public IntelligenceService(
        ILogger<IntelligenceService> logger,
        IAdvancedNLUService nluService,
        ISchemaOptimizationService schemaService,
        IAIService aiService,
        ISchemaService schemaMetadataService)
    {
        _logger = logger;
        _nluService = nluService;
        _schemaService = schemaService;
        _aiService = aiService;
        _schemaMetadataService = schemaMetadataService;
    }

    /// <summary>
    /// Perform comprehensive query intelligence analysis
    /// </summary>
    public async Task<QueryIntelligenceResult> AnalyzeQueryAsync(string naturalLanguageQuery, string userId, SchemaMetadata schema)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("Starting query intelligence analysis for user: {UserId}", userId);

            // Use provided schema

            // Perform NLU analysis
            var nluContext = new NLUAnalysisContext
            {
                Schema = schema,
                UserId = userId,
                ConversationHistory = new List<string>()
            };

            var nluResult = await _nluService.AnalyzeQueryAsync(naturalLanguageQuery, userId, nluContext);

            // Perform schema optimization analysis
            var schemaAnalysis = await AnalyzeSchemaOptimizationAsync(naturalLanguageQuery, schema, nluResult);

            // Generate SQL suggestions
            var sqlSuggestions = await GenerateSQLSuggestionsAsync(nluResult, schema);

            // Calculate overall intelligence score
            var intelligenceScore = CalculateIntelligenceScore(nluResult, schemaAnalysis);

            // Generate recommendations
            var recommendations = await GenerateIntelligenceRecommendationsAsync(nluResult, schemaAnalysis, sqlSuggestions);

            var result = new QueryIntelligenceResult
            {
                OriginalQuery = naturalLanguageQuery,
                UserId = userId,
                NLUResult = nluResult,
                SchemaAnalysis = schemaAnalysis,
                SQLSuggestions = sqlSuggestions,
                IntelligenceScore = intelligenceScore,
                Recommendations = recommendations,
                ProcessingMetrics = new QueryIntelligenceMetrics
                {
                    TotalProcessingTime = DateTime.UtcNow - startTime,
                    NLUProcessingTime = nluResult.ProcessingMetrics?.ProcessingTime ?? TimeSpan.Zero,
                    SchemaAnalysisTime = TimeSpan.FromMilliseconds(100), // Placeholder
                    ComponentsUsed = new List<string> { "NLU", "SchemaOptimization", "SQLGeneration" }
                },
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Query intelligence analysis completed with score: {Score}", intelligenceScore);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during query intelligence analysis");
            throw;
        }
    }

    /// <summary>
    /// Generate intelligent query suggestions
    /// </summary>
    public async Task<List<IntelligentQuerySuggestion>> GenerateIntelligentSuggestionsAsync(string userId, SchemaMetadata schema, string? context = null)
    {
        try
        {
            var suggestions = new List<IntelligentQuerySuggestion>();

            // Get NLU-based suggestions
            var nluSuggestions = await _nluService.GenerateSmartSuggestionsAsync(context ?? "", userId, schema);

            // Convert to intelligent suggestions with additional context
            foreach (var nluSuggestion in nluSuggestions)
            {
                suggestions.Add(new IntelligentQuerySuggestion
                {
                    Text = nluSuggestion.Text,
                    Category = nluSuggestion.Category,
                    Confidence = nluSuggestion.Relevance,
                    ExpectedIntent = "DataQuery", // Would be determined by analysis
                    SchemaRelevance = 0.8, // Would be calculated based on schema
                    ComplexityLevel = "Medium", // Would be determined by analysis
                    EstimatedPerformance = "Good" // Would be estimated based on schema
                });
            }

            return suggestions.OrderByDescending(s => s.Confidence).Take(10).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating intelligent suggestions");
            return new List<IntelligentQuerySuggestion>();
        }
    }

    /// <summary>
    /// Optimize query using combined intelligence
    /// </summary>
    public async Task<QueryOptimizationResult> OptimizeQueryAsync(string sqlQuery, SchemaMetadata schema, QueryIntelligenceResult? intelligenceResult = null)
    {
        try
        {
            // Use schema optimization service
            var optimizationResult = await _schemaService.AnalyzeQueryPerformanceAsync(sqlQuery, schema);

            // Enhance with intelligence insights if available
            if (intelligenceResult != null)
            {
                // Add intelligence-based optimizations
                optimizationResult = EnhanceWithIntelligence(optimizationResult, intelligenceResult);
            }

            return optimizationResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing query with intelligence");
            throw;
        }
    }

    /// <summary>
    /// Get intelligence metrics and analytics
    /// </summary>
    public async Task<QueryIntelligenceAnalytics> GetAnalyticsAsync(string userId, TimeSpan? timeWindow = null)
    {
        try
        {
            var window = timeWindow ?? TimeSpan.FromDays(7);
            
            // Get NLU metrics
            var nluMetrics = await _nluService.GetMetricsAsync();

            // Get schema optimization metrics (placeholder)
            var schemaMetrics = new SchemaOptimizationMetrics
            {
                TotalOptimizations = 100,
                AverageImprovementScore = 0.25,
                IndexSuggestionsGenerated = 50,
                IndexSuggestionsImplemented = 42,
                QueryPerformanceImprovement = 0.85
            };

            return new QueryIntelligenceAnalytics
            {
                UserId = userId,
                TimeWindow = window,
                NLUMetrics = nluMetrics,
                SchemaMetrics = schemaMetrics,
                OverallIntelligenceScore = 0.82,
                TotalQueries = 150,
                SuccessfulOptimizations = 120,
                AverageProcessingTime = TimeSpan.FromMilliseconds(250)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting intelligence analytics");
            throw;
        }
    }

    // Helper methods
    private async Task<SchemaOptimizationAnalysis> AnalyzeSchemaOptimizationAsync(string query, SchemaMetadata schema, AdvancedNLUResult nluResult)
    {
        // Analyze query for schema optimization opportunities
        var analysis = new SchemaOptimizationAnalysis
        {
            RecommendedIndexes = new List<IndexRecommendation>(),
            QueryComplexity = "Medium",
            EstimatedPerformance = "Good",
            OptimizationOpportunities = new List<string>()
        };

        // Add logic based on NLU results
        if (nluResult.IntentAnalysis.PrimaryIntent == "Aggregation")
        {
            analysis.OptimizationOpportunities.Add("Consider adding indexes for aggregation columns");
        }

        return analysis;
    }

    private async Task<List<SQLSuggestion>> GenerateSQLSuggestionsAsync(AdvancedNLUResult nluResult, SchemaMetadata schema)
    {
        var suggestions = new List<SQLSuggestion>();

        // Generate SQL based on intent
        switch (nluResult.IntentAnalysis.PrimaryIntent)
        {
            case "DataQuery":
                suggestions.Add(new SQLSuggestion
                {
                    SQL = "SELECT * FROM tbl_Daily_actions WHERE ...",
                    Confidence = 0.8,
                    Explanation = "Basic data retrieval query"
                });
                break;
            case "Aggregation":
                suggestions.Add(new SQLSuggestion
                {
                    SQL = "SELECT COUNT(*) FROM tbl_Daily_actions WHERE ...",
                    Confidence = 0.9,
                    Explanation = "Aggregation query based on intent"
                });
                break;
        }

        return suggestions;
    }

    private double CalculateIntelligenceScore(AdvancedNLUResult nluResult, SchemaOptimizationAnalysis schemaAnalysis)
    {
        var nluScore = nluResult.ConfidenceScore;
        var schemaScore = 0.8; // Would be calculated from schema analysis
        
        return (nluScore * 0.6) + (schemaScore * 0.4);
    }

    private async Task<List<IntelligenceRecommendation>> GenerateIntelligenceRecommendationsAsync(
        AdvancedNLUResult nluResult, 
        SchemaOptimizationAnalysis schemaAnalysis, 
        List<SQLSuggestion> sqlSuggestions)
    {
        var recommendations = new List<IntelligenceRecommendation>();

        if (nluResult.ConfidenceScore < 0.7)
        {
            recommendations.Add(new IntelligenceRecommendation
            {
                Type = "NLU",
                Priority = "High",
                Description = "Query intent unclear - consider being more specific",
                ActionItems = new List<string> { "Add specific action words", "Mention table or data type" }
            });
        }

        if (schemaAnalysis.OptimizationOpportunities.Any())
        {
            recommendations.Add(new IntelligenceRecommendation
            {
                Type = "Performance",
                Priority = "Medium",
                Description = "Performance optimization opportunities found",
                ActionItems = schemaAnalysis.OptimizationOpportunities
            });
        }

        return recommendations;
    }

    private QueryOptimizationResult EnhanceWithIntelligence(QueryOptimizationResult optimizationResult, QueryIntelligenceResult intelligenceResult)
    {
        // Add intelligence-based enhancements to optimization result
        if (intelligenceResult.IntelligenceScore > 0.8)
        {
            optimizationResult.ImprovementScore *= 1.1; // Boost confidence for high intelligence scores
        }

        return optimizationResult;
    }

    /// <summary>
    /// Get query assistance for user
    /// </summary>
    public async Task<QueryAssistance> GetQueryAssistanceAsync(string query, string userId, SchemaMetadata schema)
    {
        try
        {
            _logger.LogInformation("Getting query assistance for user: {UserId}", userId);

            // Analyze the query
            var intelligenceResult = await AnalyzeQueryAsync(query, userId, schema);

            // Generate assistance based on analysis
            var assistance = new QueryAssistance
            {
                AutocompleteSuggestions = intelligenceResult.SQLSuggestions.Select(s => s.SQL).ToList(),
                SyntaxSuggestions = intelligenceResult.Recommendations
                    .Where(r => r.Type == "Syntax")
                    .Select(r => r.Description)
                    .ToList(),
                PerformanceHints = intelligenceResult.Recommendations
                    .Where(r => r.Type == "Performance")
                    .Select(r => r.Description)
                    .ToList(),
                ContextualHelp = intelligenceResult.Recommendations
                    .Select(r => r.Description)
                    .ToList(),
                Validations = new List<QueryValidation>()
            };

            return assistance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query assistance for user: {UserId}", userId);
            return new QueryAssistance
            {
                AutocompleteSuggestions = new List<string>(),
                SyntaxSuggestions = new List<string>(),
                PerformanceHints = new List<string>(),
                ContextualHelp = new List<string>(),
                Validations = new List<QueryValidation>()
            };
        }
    }

    /// <summary>
    /// Learn from user interaction and feedback
    /// </summary>
    public async Task LearnFromInteractionAsync(string query, string userId, QueryResponse response, UserFeedback? feedback = null)
    {
        try
        {
            _logger.LogInformation("Learning from interaction for user: {UserId}", userId);

            // Log the interaction for learning
            // In a real implementation, this would update ML models and improve suggestions

            // For now, just log the interaction
            _logger.LogDebug("Query: {Query}, Success: {Success}, Feedback: {Feedback}",
                query, response.Success, feedback?.Comments ?? "None");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error learning from interaction for user: {UserId}", userId);
        }
    }
}
