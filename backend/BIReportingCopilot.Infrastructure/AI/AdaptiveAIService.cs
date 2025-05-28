using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Runtime.CompilerServices;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// Adaptive AI service that learns from user feedback and improves query generation over time
/// </summary>
public class AdaptiveAIService : IAIService
{
    private readonly IAIService _innerService;
    private readonly BICopilotContext _context;
    private readonly ILogger<AdaptiveAIService> _logger;
    private readonly IMemoryCache _cache;
    private readonly FeedbackLearningEngine _learningEngine;
    private readonly PromptOptimizer _promptOptimizer;

    public AdaptiveAIService(
        IAIService innerService,
        BICopilotContext context,
        ILogger<AdaptiveAIService> logger,
        IMemoryCache cache)
    {
        _innerService = innerService;
        _context = context;
        _logger = logger;
        _cache = cache;
        _learningEngine = new FeedbackLearningEngine(context, logger);
        _promptOptimizer = new PromptOptimizer(context, logger);
    }

    public async Task<string> GenerateSQLAsync(string prompt)
    {
        return await GenerateSQLAsync(prompt, CancellationToken.None);
    }

    public async Task<string> GenerateSQLAsync(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            // Get learning insights for this type of query
            var learningInsights = await _learningEngine.GetLearningInsightsAsync(prompt);

            // Optimize the prompt based on historical feedback
            var optimizedPrompt = await _promptOptimizer.OptimizePromptAsync(prompt, learningInsights);

            _logger.LogInformation("Generating SQL with adaptive learning. Original prompt length: {OriginalLength}, Optimized: {OptimizedLength}",
                prompt.Length, optimizedPrompt.Length);

            // Generate SQL using the optimized prompt
            var generatedSQL = await _innerService.GenerateSQLAsync(optimizedPrompt, cancellationToken);

            // Store the generation attempt for future learning
            await StoreGenerationAttemptAsync(prompt, optimizedPrompt, generatedSQL);

            return generatedSQL;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in adaptive SQL generation");
            // Fallback to original service
            return await _innerService.GenerateSQLAsync(prompt, cancellationToken);
        }
    }

    public async Task<double> CalculateConfidenceScoreAsync(string naturalLanguageQuery, string generatedSQL)
    {
        try
        {
            // Get base confidence from inner service
            var baseConfidence = await _innerService.CalculateConfidenceScoreAsync(naturalLanguageQuery, generatedSQL);

            // Enhance confidence with learning insights
            var learningInsights = await _learningEngine.GetLearningInsightsAsync(naturalLanguageQuery);
            var enhancedConfidence = await _learningEngine.EnhanceConfidenceWithLearningAsync(
                baseConfidence, naturalLanguageQuery, generatedSQL, learningInsights);

            _logger.LogDebug("Confidence enhanced from {BaseConfidence:P2} to {EnhancedConfidence:P2}",
                baseConfidence, enhancedConfidence);

            return enhancedConfidence;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error enhancing confidence score, using base confidence");
            return await _innerService.CalculateConfidenceScoreAsync(naturalLanguageQuery, generatedSQL);
        }
    }

    public async Task<string[]> GenerateQuerySuggestionsAsync(string context, SchemaMetadata schema)
    {
        try
        {
            // Get base suggestions
            var baseSuggestions = await _innerService.GenerateQuerySuggestionsAsync(context, schema);

            // Enhance with personalized suggestions based on user patterns
            var personalizedSuggestions = await _learningEngine.GetPersonalizedSuggestionsAsync(context, schema);

            // Combine and rank suggestions
            var combinedSuggestions = baseSuggestions.Concat(personalizedSuggestions).Distinct().ToArray();

            _logger.LogInformation("Generated {BaseCount} base suggestions, {PersonalizedCount} personalized suggestions",
                baseSuggestions.Length, personalizedSuggestions.Length);

            return combinedSuggestions;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generating adaptive suggestions, using base suggestions");
            return await _innerService.GenerateQuerySuggestionsAsync(context, schema);
        }
    }

    public async Task<string> GenerateInsightAsync(string query, object[] data)
    {
        try
        {
            // Get learning context for insight generation
            var insightContext = await _learningEngine.GetInsightContextAsync(query);

            // Generate enhanced insight
            var baseInsight = await _innerService.GenerateInsightAsync(query, data);
            var enhancedInsight = await _promptOptimizer.EnhanceInsightAsync(baseInsight, insightContext, data);

            return enhancedInsight;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error generating adaptive insight, using base insight");
            return await _innerService.GenerateInsightAsync(query, data);
        }
    }

    public async Task<string> GenerateVisualizationConfigAsync(string query, ColumnMetadata[] columns, object[] data)
    {
        return await _innerService.GenerateVisualizationConfigAsync(query, columns, data);
    }

    public async Task<bool> ValidateQueryIntentAsync(string naturalLanguageQuery)
    {
        return await _innerService.ValidateQueryIntentAsync(naturalLanguageQuery);
    }

    // Streaming methods - delegate to inner service for now
    public async IAsyncEnumerable<StreamingResponse> GenerateSQLStreamAsync(
        string prompt,
        SchemaMetadata? schema = null,
        QueryContext? context = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var response in _innerService.GenerateSQLStreamAsync(prompt, schema, context, cancellationToken))
        {
            yield return response;
        }
    }

    public async IAsyncEnumerable<StreamingResponse> GenerateInsightStreamAsync(
        string query,
        object[] data,
        AnalysisContext? context = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var response in _innerService.GenerateInsightStreamAsync(query, data, context, cancellationToken))
        {
            yield return response;
        }
    }

    public async IAsyncEnumerable<StreamingResponse> GenerateExplanationStreamAsync(
        string sql,
        StreamingQueryComplexity complexity = StreamingQueryComplexity.Medium,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (var response in _innerService.GenerateExplanationStreamAsync(sql, complexity, cancellationToken))
        {
            yield return response;
        }
    }

    /// <summary>
    /// Process user feedback to improve future generations
    /// </summary>
    public async Task ProcessFeedbackAsync(string originalPrompt, string generatedSQL, QueryFeedback feedback, string userId)
    {
        try
        {
            await _learningEngine.ProcessFeedbackAsync(originalPrompt, generatedSQL, feedback, userId);
            _logger.LogInformation("Processed feedback for user {UserId}, feedback: {Feedback}", userId, feedback.Feedback);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing feedback for user {UserId}", userId);
        }
    }

    /// <summary>
    /// Get learning statistics for monitoring
    /// </summary>
    public async Task<LearningStatistics> GetLearningStatisticsAsync()
    {
        return await _learningEngine.GetLearningStatisticsAsync();
    }

    private async Task StoreGenerationAttemptAsync(string originalPrompt, string optimizedPrompt, string generatedSQL)
    {
        try
        {
            var attempt = new Core.Models.AIGenerationAttempt
            {
                UserQuery = originalPrompt,
                GeneratedSql = generatedSQL,
                AttemptedAt = DateTime.UtcNow,
                IsSuccessful = !string.IsNullOrEmpty(generatedSQL),
                ConfidenceScore = 0.8, // Default confidence
                UserId = "system", // Default user for now
                GenerationTimeMs = 0, // Will be calculated elsewhere
                PromptTemplate = optimizedPrompt
            };

            _context.AIGenerationAttempts.Add(attempt);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to store generation attempt");
        }
    }

    private static string ComputeHash(string input)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash)[..16]; // Take first 16 characters
    }
}



/// <summary>
/// Learning statistics for monitoring
/// </summary>
public class LearningStatistics
{
    public int TotalGenerations { get; set; }
    public int TotalFeedbackItems { get; set; }
    public double AverageRating { get; set; }
    public double AverageConfidence { get; set; }
    public int UniqueUsers { get; set; }
    public Dictionary<string, int> PopularPatterns { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
