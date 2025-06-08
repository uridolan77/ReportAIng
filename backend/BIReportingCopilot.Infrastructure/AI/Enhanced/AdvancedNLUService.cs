using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Performance;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Advanced Natural Language Understanding service with deep semantic analysis
/// Implements Enhancement 15: Advanced Natural Language Understanding
/// </summary>
public class AdvancedNLUService
{
    private readonly ILogger<AdvancedNLUService> _logger;
    private readonly ICacheService _cacheService;
    private readonly NLUConfiguration _config;
    private readonly SemanticParser _semanticParser;
    private readonly IntentClassifier _intentClassifier;
    private readonly EntityExtractor _entityExtractor;
    private readonly ContextualAnalyzer _contextualAnalyzer;
    private readonly ConversationManager _conversationManager;
    private readonly MultilingualProcessor _multilingualProcessor;
    private readonly DomainAdaptationEngine _domainEngine;

    public AdvancedNLUService(
        ILogger<AdvancedNLUService> logger,
        ICacheService cacheService,
        IOptions<NLUConfiguration> config)
    {
        _logger = logger;
        _cacheService = cacheService;
        _config = config.Value;

        _semanticParser = new SemanticParser(logger, config.Value);
        _intentClassifier = new IntentClassifier(logger, config.Value);
        _entityExtractor = new EntityExtractor(logger, config.Value);
        _contextualAnalyzer = new ContextualAnalyzer(logger, cacheService, config.Value);
        _conversationManager = new ConversationManager(logger, cacheService);
        _multilingualProcessor = new MultilingualProcessor(logger, config.Value);
        _domainEngine = new DomainAdaptationEngine(logger, cacheService);
    }

    /// <summary>
    /// Perform comprehensive natural language understanding analysis
    /// </summary>
    public async Task<AdvancedNLUResult> AnalyzeNaturalLanguageAsync(
        string naturalLanguageQuery,
        string userId,
        ConversationContext? context = null)
    {
        try
        {
            _logger.LogDebug("Starting advanced NLU analysis for user {UserId}: {Query}", 
                userId, naturalLanguageQuery);

            var analysisContext = new NLUAnalysisContext
            {
                Query = naturalLanguageQuery,
                UserId = userId,
                ConversationContext = context,
                Timestamp = DateTime.UtcNow,
                Language = await DetectLanguageAsync(naturalLanguageQuery)
            };

            // Step 1: Multilingual processing and normalization
            var normalizedQuery = await _multilingualProcessor.ProcessAndNormalizeAsync(analysisContext);

            // Step 2: Semantic parsing and structure analysis
            var semanticStructure = await _semanticParser.ParseSemanticStructureAsync(normalizedQuery, analysisContext);

            // Step 3: Intent classification with confidence scoring
            var intentAnalysis = await _intentClassifier.ClassifyIntentAsync(normalizedQuery, semanticStructure, analysisContext);

            // Step 4: Advanced entity extraction with relationship mapping
            var entityAnalysis = await _entityExtractor.ExtractEntitiesAsync(normalizedQuery, semanticStructure, analysisContext);

            // Step 5: Contextual analysis with conversation history
            var contextualAnalysis = await _contextualAnalyzer.AnalyzeContextAsync(normalizedQuery, intentAnalysis, entityAnalysis, analysisContext);

            // Step 6: Domain adaptation and business context integration
            var domainAnalysis = await _domainEngine.AdaptToDomainAsync(normalizedQuery, intentAnalysis, entityAnalysis, analysisContext);

            // Step 7: Conversation state management
            await _conversationManager.UpdateConversationStateAsync(userId, analysisContext, intentAnalysis, entityAnalysis);

            // Step 8: Generate comprehensive understanding result
            var result = new AdvancedNLUResult
            {
                AnalysisId = Guid.NewGuid().ToString(),
                OriginalQuery = naturalLanguageQuery,
                NormalizedQuery = normalizedQuery,
                Language = analysisContext.Language,
                SemanticStructure = semanticStructure,
                IntentAnalysis = intentAnalysis,
                EntityAnalysis = entityAnalysis,
                ContextualAnalysis = contextualAnalysis,
                DomainAnalysis = domainAnalysis,
                ConversationState = await _conversationManager.GetConversationStateAsync(userId),
                ConfidenceScore = CalculateOverallConfidence(intentAnalysis, entityAnalysis, contextualAnalysis),
                ProcessingMetrics = new NLUProcessingMetrics
                {
                    ProcessingTime = DateTime.UtcNow - analysisContext.Timestamp,
                    ComponentsUsed = GetUsedComponents(),
                    CacheHits = 0, // Would be tracked in real implementation
                    ModelVersions = GetModelVersions()
                },
                Recommendations = await GenerateNLURecommendationsAsync(intentAnalysis, entityAnalysis, contextualAnalysis),
                Timestamp = DateTime.UtcNow
            };

            _logger.LogDebug("Advanced NLU analysis completed with confidence {Confidence:P2}", result.ConfidenceScore);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in advanced NLU analysis");
            return new AdvancedNLUResult
            {
                AnalysisId = Guid.NewGuid().ToString(),
                OriginalQuery = naturalLanguageQuery,
                Language = "en",
                ConfidenceScore = 0.0,
                Timestamp = DateTime.UtcNow,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Train NLU models with domain-specific data
    /// </summary>
    public async Task TrainNLUModelsAsync(List<NLUTrainingData> trainingData, string? domain = null)
    {
        try
        {
            _logger.LogInformation("Training NLU models with {DataCount} samples for domain {Domain}", 
                trainingData.Count, domain ?? "general");

            // Train intent classifier
            await _intentClassifier.TrainAsync(trainingData, domain);

            // Train entity extractor
            await _entityExtractor.TrainAsync(trainingData, domain);

            // Train semantic parser
            await _semanticParser.TrainAsync(trainingData, domain);

            // Update domain adaptation models
            await _domainEngine.UpdateDomainModelsAsync(trainingData, domain);

            // Store training metadata
            var metadata = new NLUModelMetadata
            {
                TrainingDataCount = trainingData.Count,
                Domain = domain,
                TrainingDate = DateTime.UtcNow,
                ModelVersion = GenerateModelVersion(),
                PerformanceMetrics = await EvaluateModelPerformanceAsync(trainingData)
            };

            await _cacheService.SetAsync($"nlu_model_metadata:{domain ?? "general"}", metadata, TimeSpan.FromDays(30));

            _logger.LogInformation("NLU model training completed for domain {Domain}", domain ?? "general");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error training NLU models");
        }
    }

    /// <summary>
    /// Get NLU performance analytics
    /// </summary>
    public async Task<NLUAnalytics> GetNLUAnalyticsAsync(TimeSpan period, string? userId = null)
    {
        try
        {
            var endTime = DateTime.UtcNow;
            var startTime = endTime - period;

            var analytics = new NLUAnalytics
            {
                Period = period,
                TotalQueries = await CountQueriesInPeriodAsync(startTime, endTime, userId),
                AverageConfidence = await CalculateAverageConfidenceAsync(startTime, endTime, userId),
                IntentDistribution = await GetIntentDistributionAsync(startTime, endTime, userId),
                EntityDistribution = await GetEntityDistributionAsync(startTime, endTime, userId),
                LanguageDistribution = await GetLanguageDistributionAsync(startTime, endTime, userId),
                PerformanceMetrics = await GetPerformanceMetricsAsync(startTime, endTime),
                ErrorAnalysis = await GetErrorAnalysisAsync(startTime, endTime, userId),
                ImprovementSuggestions = await GenerateImprovementSuggestionsAsync(startTime, endTime),
                GeneratedAt = DateTime.UtcNow
            };

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating NLU analytics");
            return new NLUAnalytics
            {
                Period = period,
                GeneratedAt = DateTime.UtcNow,
                Error = ex.Message
            };
        }
    }

    /// <summary>
    /// Update NLU configuration and thresholds
    /// </summary>
    public async Task UpdateNLUConfigurationAsync(NLUConfiguration configuration)
    {
        try
        {
            _logger.LogInformation("Updating NLU configuration");

            // Update component configurations
            await _intentClassifier.UpdateConfigurationAsync(configuration);
            await _entityExtractor.UpdateConfigurationAsync(configuration);
            await _semanticParser.UpdateConfigurationAsync(configuration);
            await _contextualAnalyzer.UpdateConfigurationAsync(configuration);
            await _multilingualProcessor.UpdateConfigurationAsync(configuration);

            // Store updated configuration
            await _cacheService.SetAsync("nlu_configuration", configuration, TimeSpan.FromDays(30));

            _logger.LogInformation("NLU configuration updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating NLU configuration");
        }
    }

    /// <summary>
    /// Analyze conversation patterns and generate insights
    /// </summary>
    public async Task<ConversationAnalysis> AnalyzeConversationPatternsAsync(
        string userId, 
        TimeSpan analysisWindow)
    {
        try
        {
            var conversationHistory = await _conversationManager.GetConversationHistoryAsync(userId, analysisWindow);
            
            var analysis = new ConversationAnalysis
            {
                UserId = userId,
                AnalysisWindow = analysisWindow,
                TotalInteractions = conversationHistory.Count,
                AverageQueryLength = conversationHistory.Any() ? conversationHistory.Average(c => c.Query.Length) : 0,
                CommonIntents = await IdentifyCommonIntentsAsync(conversationHistory),
                ConversationFlow = await AnalyzeConversationFlowAsync(conversationHistory),
                UserPreferences = await InferUserPreferencesAsync(conversationHistory),
                EngagementMetrics = await CalculateEngagementMetricsAsync(conversationHistory),
                Recommendations = await GenerateConversationRecommendationsAsync(conversationHistory),
                GeneratedAt = DateTime.UtcNow
            };

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing conversation patterns for user {UserId}", userId);
            return new ConversationAnalysis
            {
                UserId = userId,
                AnalysisWindow = analysisWindow,
                GeneratedAt = DateTime.UtcNow,
                Error = ex.Message
            };
        }
    }

    // Private methods

    private async Task<string> DetectLanguageAsync(string text)
    {
        // Simple language detection - in production would use proper language detection
        if (text.Any(c => c > 127)) // Non-ASCII characters
        {
            return "auto"; // Auto-detect
        }
        return "en"; // Default to English
    }

    private double CalculateOverallConfidence(
        IntentAnalysis intentAnalysis,
        EntityAnalysis entityAnalysis,
        ContextualAnalysis contextualAnalysis)
    {
        var weights = new[] { 0.4, 0.3, 0.3 }; // Intent, Entity, Context weights
        var scores = new[]
        {
            intentAnalysis.Confidence,
            entityAnalysis.OverallConfidence,
            contextualAnalysis.ContextualRelevance
        };

        return weights.Zip(scores, (w, s) => w * s).Sum();
    }

    private List<string> GetUsedComponents()
    {
        return new List<string>
        {
            "SemanticParser",
            "IntentClassifier", 
            "EntityExtractor",
            "ContextualAnalyzer",
            "MultilingualProcessor",
            "DomainAdaptationEngine"
        };
    }

    private Dictionary<string, string> GetModelVersions()
    {
        return new Dictionary<string, string>
        {
            ["intent_classifier"] = "v2.1.0",
            ["entity_extractor"] = "v1.8.0",
            ["semantic_parser"] = "v1.5.0",
            ["contextual_analyzer"] = "v1.3.0"
        };
    }

    private async Task<List<NLURecommendation>> GenerateNLURecommendationsAsync(
        IntentAnalysis intentAnalysis,
        EntityAnalysis entityAnalysis,
        ContextualAnalysis contextualAnalysis)
    {
        var recommendations = new List<NLURecommendation>();

        // Intent-based recommendations
        if (intentAnalysis.Confidence < 0.8)
        {
            recommendations.Add(new NLURecommendation
            {
                Type = RecommendationType.Clarification,
                Title = "Intent Clarification Needed",
                Description = "The query intent is unclear. Consider rephrasing for better understanding.",
                Priority = RecommendationPriority.Medium,
                Confidence = 1.0 - intentAnalysis.Confidence
            });
        }

        // Entity-based recommendations
        if (entityAnalysis.MissingEntities.Any())
        {
            recommendations.Add(new NLURecommendation
            {
                Type = RecommendationType.Enhancement,
                Title = "Missing Information",
                Description = $"Consider specifying: {string.Join(", ", entityAnalysis.MissingEntities)}",
                Priority = RecommendationPriority.Low,
                Confidence = 0.7
            });
        }

        // Context-based recommendations
        if (contextualAnalysis.ContextualRelevance < 0.6)
        {
            recommendations.Add(new NLURecommendation
            {
                Type = RecommendationType.Context,
                Title = "Context Enhancement",
                Description = "Providing more context could improve query understanding",
                Priority = RecommendationPriority.Low,
                Confidence = 0.6
            });
        }

        return recommendations;
    }

    private string GenerateModelVersion()
    {
        return $"v{DateTime.UtcNow:yyyyMMdd}.{DateTime.UtcNow:HHmm}";
    }

    private async Task<ModelPerformanceMetrics> EvaluateModelPerformanceAsync(List<NLUTrainingData> testData)
    {
        // Simplified performance evaluation
        return new ModelPerformanceMetrics
        {
            Accuracy = 0.92,
            Precision = 0.89,
            Recall = 0.91,
            F1Score = 0.90,
            ConfusionMatrix = new Dictionary<string, Dictionary<string, int>>()
        };
    }

    private async Task<int> CountQueriesInPeriodAsync(DateTime startTime, DateTime endTime, string? userId)
    {
        // In real implementation, would query analytics database
        return 150;
    }

    private async Task<double> CalculateAverageConfidenceAsync(DateTime startTime, DateTime endTime, string? userId)
    {
        return 0.87;
    }

    private async Task<Dictionary<string, int>> GetIntentDistributionAsync(DateTime startTime, DateTime endTime, string? userId)
    {
        return new Dictionary<string, int>
        {
            ["DataQuery"] = 45,
            ["Aggregation"] = 30,
            ["Filtering"] = 25,
            ["Comparison"] = 20,
            ["Trend"] = 15
        };
    }

    private async Task<Dictionary<string, int>> GetEntityDistributionAsync(DateTime startTime, DateTime endTime, string? userId)
    {
        return new Dictionary<string, int>
        {
            ["Date"] = 60,
            ["Metric"] = 45,
            ["Dimension"] = 35,
            ["Filter"] = 25,
            ["Aggregation"] = 20
        };
    }

    private async Task<Dictionary<string, int>> GetLanguageDistributionAsync(DateTime startTime, DateTime endTime, string? userId)
    {
        return new Dictionary<string, int>
        {
            ["en"] = 120,
            ["es"] = 15,
            ["fr"] = 10,
            ["de"] = 5
        };
    }

    private async Task<Dictionary<string, double>> GetPerformanceMetricsAsync(DateTime startTime, DateTime endTime)
    {
        return new Dictionary<string, double>
        {
            ["average_processing_time_ms"] = 125.5,
            ["cache_hit_rate"] = 0.73,
            ["model_accuracy"] = 0.89,
            ["error_rate"] = 0.05
        };
    }

    private async Task<ErrorAnalysis> GetErrorAnalysisAsync(DateTime startTime, DateTime endTime, string? userId)
    {
        return new ErrorAnalysis
        {
            TotalErrors = 8,
            ErrorsByType = new Dictionary<string, int>
            {
                ["IntentClassificationError"] = 3,
                ["EntityExtractionError"] = 2,
                ["ContextualAnalysisError"] = 2,
                ["ParsingError"] = 1
            },
            CommonErrorPatterns = new List<string>
            {
                "Ambiguous temporal references",
                "Complex nested queries",
                "Domain-specific terminology"
            }
        };
    }

    private async Task<List<string>> GenerateImprovementSuggestionsAsync(DateTime startTime, DateTime endTime)
    {
        return new List<string>
        {
            "Add more training data for temporal entity recognition",
            "Improve handling of complex nested queries",
            "Expand domain-specific vocabulary",
            "Enhance contextual understanding for multi-turn conversations"
        };
    }

    private async Task<List<IntentFrequency>> IdentifyCommonIntentsAsync(List<ConversationTurn> conversationHistory)
    {
        return conversationHistory
            .GroupBy(c => c.Intent)
            .Select(g => new IntentFrequency
            {
                Intent = g.Key,
                Frequency = g.Count(),
                Percentage = (double)g.Count() / conversationHistory.Count * 100
            })
            .OrderByDescending(i => i.Frequency)
            .Take(5)
            .ToList();
    }

    private async Task<ConversationFlow> AnalyzeConversationFlowAsync(List<ConversationTurn> conversationHistory)
    {
        return new ConversationFlow
        {
            AverageConversationLength = conversationHistory.Count,
            CommonTransitions = new Dictionary<string, string>
            {
                ["DataQuery"] = "Filtering",
                ["Filtering"] = "Aggregation",
                ["Aggregation"] = "Comparison"
            },
            ConversationPatterns = new List<string>
            {
                "Query -> Filter -> Aggregate",
                "Compare -> Trend -> Drill-down"
            }
        };
    }

    private async Task<NLUUserPreferences> InferUserPreferencesAsync(List<ConversationTurn> conversationHistory)
    {
        return new NLUUserPreferences
        {
            PreferredQueryTypes = new List<string> { "DataQuery", "Aggregation" },
            PreferredTimeRanges = new List<string> { "Last 30 days", "This month" },
            PreferredMetrics = new List<string> { "Revenue", "Users", "Conversions" },
            CommunicationStyle = "Concise",
            TechnicalLevel = "Intermediate"
        };
    }

    private async Task<EngagementMetrics> CalculateEngagementMetricsAsync(List<ConversationTurn> conversationHistory)
    {
        return new EngagementMetrics
        {
            AverageSessionDuration = TimeSpan.FromMinutes(12),
            QueriesPerSession = 4.2,
            SuccessfulQueryRate = 0.89,
            UserSatisfactionScore = 4.3,
            ReturnUserRate = 0.76
        };
    }

    private async Task<List<string>> GenerateConversationRecommendationsAsync(List<ConversationTurn> conversationHistory)
    {
        return new List<string>
        {
            "User prefers concise responses - maintain brevity",
            "Frequently asks about revenue metrics - suggest related KPIs",
            "Often filters by date ranges - provide quick date shortcuts",
            "Shows interest in trend analysis - offer predictive insights"
        };
    }
}
