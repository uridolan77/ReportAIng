using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Extensions;
using BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Enhanced business context analyzer that consolidates and replaces all legacy analyzers
/// </summary>
public class EnhancedBusinessContextAnalyzer : IEnhancedBusinessContextAnalyzer
{
    private readonly IIntentClassificationEnsemble _intentEnsemble;
    private readonly IEntityExtractionPipeline _entityPipeline;
    private readonly IAdvancedDomainDetector _domainDetector;
    private readonly IConfidenceValidationSystem _confidenceValidator;
    private readonly IBusinessTermExtractor _termExtractor;
    private readonly ITimeContextAnalyzer _timeAnalyzer;
    private readonly IUserFeedbackLearner _feedbackLearner;
    private readonly ICacheService _cacheService;
    private readonly ILogger<EnhancedBusinessContextAnalyzer> _logger;

    public EnhancedBusinessContextAnalyzer(
        IIntentClassificationEnsemble intentEnsemble,
        IEntityExtractionPipeline entityPipeline,
        IAdvancedDomainDetector domainDetector,
        IConfidenceValidationSystem confidenceValidator,
        IBusinessTermExtractor termExtractor,
        ITimeContextAnalyzer timeAnalyzer,
        IUserFeedbackLearner feedbackLearner,
        ICacheService cacheService,
        ILogger<EnhancedBusinessContextAnalyzer> logger)
    {
        _intentEnsemble = intentEnsemble;
        _entityPipeline = entityPipeline;
        _domainDetector = domainDetector;
        _confidenceValidator = confidenceValidator;
        _termExtractor = termExtractor;
        _timeAnalyzer = timeAnalyzer;
        _feedbackLearner = feedbackLearner;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<BusinessContextProfile> AnalyzeUserQuestionAsync(string userQuestion, string? userId = null)
    {
        var analysisId = Guid.NewGuid().ToString();
        _logger.LogInformation("Starting enhanced business context analysis {AnalysisId} for question: {Question}", 
            analysisId, userQuestion.Substring(0, Math.Min(100, userQuestion.Length)));

        var startTime = DateTime.UtcNow;

        try
        {
            // Check cache first
            var cacheKey = $"enhanced_context_analysis:{userQuestion.GetHashCode()}:{userId}";
            var (found, cachedProfile) = await _cacheService.TryGetAsync<BusinessContextProfile>(cacheKey);
            if (found && cachedProfile != null)
            {
                _logger.LogDebug("Retrieved cached analysis for question");
                return cachedProfile;
            }

            // Get user patterns for personalization
            var userPatterns = userId != null ? 
                await _feedbackLearner.GetUserPatternsAsync(userId) : 
                new UserAnalysisPatterns();

            // Run all analysis components in parallel for maximum performance
            var intentTask = _intentEnsemble.ClassifyWithConfidenceAsync(userQuestion);
            var entitiesTask = _entityPipeline.ExtractEntitiesAsync(userQuestion);
            var domainTask = _domainDetector.DetectDomainAsync(userQuestion);
            var termsTask = _termExtractor.ExtractBusinessTermsAsync(userQuestion);
            var timeTask = _timeAnalyzer.ExtractTimeContextAsync(userQuestion);

            await Task.WhenAll(intentTask, entitiesTask, domainTask, termsTask, timeTask);

            var rawIntent = await intentTask;
            var rawEntities = await entitiesTask;
            var rawDomain = await domainTask;
            var businessTerms = await termsTask;
            var timeContext = await timeTask;

            // Validate all analysis results with confidence validation
            var intentValidationTask = _confidenceValidator.ValidateIntentAsync(rawIntent, userQuestion);
            var entitiesValidationTask = _confidenceValidator.ValidateEntitiesAsync(rawEntities, userQuestion);
            var domainValidationTask = _confidenceValidator.ValidateDomainAsync(rawDomain, userQuestion);

            await Task.WhenAll(intentValidationTask, entitiesValidationTask, domainValidationTask);

            var validatedIntent = await intentValidationTask;
            var validatedEntities = await entitiesValidationTask;
            var validatedDomain = await domainValidationTask;

            // Build comprehensive business context profile
            var profile = new BusinessContextProfile
            {
                OriginalQuestion = userQuestion,
                UserId = userId ?? string.Empty,
                Intent = validatedIntent,
                Domain = validatedDomain,
                Entities = validatedEntities,
                BusinessTerms = businessTerms,
                TimeContext = timeContext,
                UserPatterns = new Dictionary<string, object>
                {
                    ["intent_patterns"] = userPatterns.IntentPatterns,
                    ["frequent_entities"] = userPatterns.FrequentEntities,
                    ["domain_preferences"] = userPatterns.DomainPreferences,
                    ["term_preferences"] = userPatterns.TermPreferences,
                    ["last_updated"] = userPatterns.LastUpdated
                },
                ConfidenceScore = CalculateOverallConfidence(validatedIntent, validatedDomain, validatedEntities),
                CreatedAt = DateTime.UtcNow,
                AnalysisId = analysisId,
                Metadata = new Dictionary<string, object>
                {
                    ["analysis_duration_ms"] = (DateTime.UtcNow - startTime).TotalMilliseconds,
                    ["validation_applied"] = true,
                    ["user_patterns_applied"] = userId != null,
                    ["cache_miss"] = true,
                    ["analysis_version"] = "enhanced_v1.0"
                }
            };

            // Apply user-specific adjustments based on historical patterns
            if (userId != null)
            {
                profile = await ApplyUserPersonalizationAsync(profile, userPatterns);
            }

            // Calculate term relevance scores for better context matching
            profile.TermRelevanceScores = await CalculateTermRelevanceScoresAsync(profile);

            // Cache the result for future use
            await _cacheService.SetAsync(cacheKey, profile, TimeSpan.FromMinutes(30));

            // Record this analysis for future learning
            await _feedbackLearner.RecordAnalysisAsync(profile, userQuestion, userId);

            var analysisTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("Enhanced analysis {AnalysisId} completed in {Duration}ms with confidence {Confidence:F3}", 
                analysisId, analysisTime, profile.ConfidenceScore);

            return profile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Enhanced business context analysis failed for {AnalysisId}", analysisId);
            
            // Return fallback analysis
            return await CreateFallbackAnalysisAsync(userQuestion, userId, analysisId);
        }
    }

    private double CalculateOverallConfidence(QueryIntent intent, BusinessDomain domain, List<BusinessEntity> entities)
    {
        var intentWeight = 0.4;
        var domainWeight = 0.3;
        var entitiesWeight = 0.3;

        var intentScore = intent.ConfidenceScore * intentWeight;
        var domainScore = domain.RelevanceScore * domainWeight;
        var entitiesScore = entities.Any() ? 
            entities.Average(e => e.ConfidenceScore) * entitiesWeight : 
            0.5 * entitiesWeight; // Default score if no entities

        var overallScore = intentScore + domainScore + entitiesScore;
        
        // Apply bonus for high-confidence components
        if (intent.ConfidenceScore > 0.9) overallScore += 0.05;
        if (domain.RelevanceScore > 0.9) overallScore += 0.05;
        if (entities.Count(e => e.ConfidenceScore > 0.9) >= 2) overallScore += 0.05;

        return Math.Min(overallScore, 0.98); // Cap at 98%
    }

    private async Task<BusinessContextProfile> ApplyUserPersonalizationAsync(
        BusinessContextProfile profile, 
        UserAnalysisPatterns userPatterns)
    {
        _logger.LogDebug("Applying user personalization based on {PatternCount} historical patterns", 
            userPatterns.IntentPatterns.Count);

        // Adjust intent confidence based on user patterns
        if (userPatterns.IntentPatterns.TryGetValue(profile.Intent.Type, out var intentPattern))
        {
            var adjustment = intentPattern.SuccessRate > 0.8 ? 0.1 : -0.05;
            profile.Intent.ConfidenceScore = Math.Min(profile.Intent.ConfidenceScore + adjustment, 0.98);
            
            profile.Metadata["intent_personalization_applied"] = true;
            profile.Metadata["intent_historical_success_rate"] = intentPattern.SuccessRate;
        }

        // Boost confidence for entities the user frequently queries
        foreach (var entity in profile.Entities)
        {
            if (userPatterns.FrequentEntities.ContainsKey(entity.Name.ToLower()))
            {
                var frequency = userPatterns.FrequentEntities[entity.Name.ToLower()];
                var boost = Math.Min(frequency * 0.02, 0.1); // Max 10% boost
                entity.ConfidenceScore = Math.Min(entity.ConfidenceScore + boost, 0.98);
                entity.Metadata["personalization_boost"] = boost;
            }
        }

        // Adjust domain confidence based on user's domain preferences
        if (userPatterns.DomainPreferences.TryGetValue(profile.Domain.Name, out var domainPreference))
        {
            var adjustment = domainPreference > 0.7 ? 0.1 : -0.05;
            profile.Domain.RelevanceScore = Math.Min(profile.Domain.RelevanceScore + adjustment, 0.98);
            
            profile.Metadata["domain_personalization_applied"] = true;
            profile.Metadata["domain_user_preference"] = domainPreference;
        }

        return profile;
    }

    private async Task<Dictionary<string, double>> CalculateTermRelevanceScoresAsync(BusinessContextProfile profile)
    {
        var relevanceScores = new Dictionary<string, double>();

        foreach (var term in profile.BusinessTerms)
        {
            var score = 0.5; // Base relevance

            // Boost score if term appears in high-confidence entities
            if (profile.Entities.Any(e => e.Name.Contains(term, StringComparison.OrdinalIgnoreCase) && e.ConfidenceScore > 0.8))
            {
                score += 0.3;
            }

            // Boost score if term is related to the detected intent
            if (IsTermRelevantToIntent(term, profile.Intent.Type))
            {
                score += 0.2;
            }

            // Boost score if term is domain-specific
            if (profile.Domain.KeyConcepts.Contains(term, StringComparer.OrdinalIgnoreCase))
            {
                score += 0.2;
            }

            // Boost score based on term position in question (earlier = more important)
            var termPosition = profile.OriginalQuestion.IndexOf(term, StringComparison.OrdinalIgnoreCase);
            if (termPosition >= 0)
            {
                var positionScore = 1.0 - ((double)termPosition / profile.OriginalQuestion.Length * 0.2);
                score *= positionScore;
            }

            relevanceScores[term] = Math.Min(score, 1.0);
        }

        return relevanceScores;
    }

    private bool IsTermRelevantToIntent(string term, IntentType intentType)
    {
        var termLower = term.ToLower();
        
        return intentType switch
        {
            IntentType.Aggregation => new[] { "total", "sum", "count", "average", "aggregate" }.Contains(termLower),
            IntentType.Trend => new[] { "trend", "growth", "change", "time", "period" }.Contains(termLower),
            IntentType.Comparison => new[] { "compare", "versus", "difference", "better", "worse" }.Contains(termLower),
            IntentType.Detail => new[] { "detail", "specific", "individual", "list", "show" }.Contains(termLower),
            IntentType.Exploratory => new[] { "find", "discover", "explore", "pattern", "insight" }.Contains(termLower),
            IntentType.Operational => new[] { "current", "status", "real-time", "live", "active" }.Contains(termLower),
            _ => false
        };
    }

    private async Task<BusinessContextProfile> CreateFallbackAnalysisAsync(string userQuestion, string? userId, string analysisId)
    {
        _logger.LogWarning("Creating fallback analysis for {AnalysisId}", analysisId);

        return new BusinessContextProfile
        {
            OriginalQuestion = userQuestion,
            UserId = userId ?? string.Empty,
            Intent = new QueryIntent
            {
                Type = IntentType.Analytical,
                ConfidenceScore = 0.5,
                Description = "Fallback analytical intent",
                Keywords = userQuestion.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(5).ToList()
            },
            Domain = new BusinessDomain
            {
                Name = "General",
                Description = "General business domain",
                RelevanceScore = 0.5,
                KeyConcepts = new List<string> { "business", "data" }
            },
            Entities = new List<BusinessEntity>(),
            BusinessTerms = ExtractBasicBusinessTerms(userQuestion),
            TimeContext = null,
            ConfidenceScore = 0.4, // Low confidence for fallback
            CreatedAt = DateTime.UtcNow,
            AnalysisId = analysisId,
            Metadata = new Dictionary<string, object>
            {
                ["fallback_analysis"] = true,
                ["analysis_version"] = "fallback_v1.0"
            }
        };
    }

    private List<string> ExtractBasicBusinessTerms(string userQuestion)
    {
        var commonBusinessTerms = new[]
        {
            "revenue", "profit", "sales", "customers", "users", "products", "orders",
            "total", "count", "average", "sum", "growth", "trend", "analysis"
        };

        var questionWords = userQuestion.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return commonBusinessTerms.Where(term => questionWords.Contains(term)).ToList();
    }

    // Legacy interface compatibility methods
    public async Task<QueryIntent> ClassifyBusinessIntentAsync(string userQuestion)
    {
        var profile = await AnalyzeUserQuestionAsync(userQuestion);
        return profile.Intent;
    }

    public async Task<List<BusinessEntity>> ExtractBusinessEntitiesAsync(string userQuestion)
    {
        var profile = await AnalyzeUserQuestionAsync(userQuestion);
        return profile.Entities;
    }

    public async Task<BusinessDomain> DetectBusinessDomainAsync(string userQuestion)
    {
        var profile = await AnalyzeUserQuestionAsync(userQuestion);
        return profile.Domain;
    }

    public async Task<List<string>> ExtractBusinessTermsAsync(string userQuestion)
    {
        var profile = await AnalyzeUserQuestionAsync(userQuestion);
        return profile.BusinessTerms;
    }

    public async Task<TimeRange?> ExtractTimeContextAsync(string userQuestion)
    {
        var profile = await AnalyzeUserQuestionAsync(userQuestion);
        return profile.TimeContext;
    }

    /// <summary>
    /// Alias method for compatibility with QueryController
    /// </summary>
    public async Task<BusinessContextProfile> AnalyzeBusinessContextAsync(string userQuestion, string? userId = null)
    {
        return await AnalyzeUserQuestionAsync(userQuestion, userId);
    }
}

public record UserAnalysisPatterns
{
    public Dictionary<IntentType, IntentPattern> IntentPatterns { get; init; } = new();
    public Dictionary<string, int> FrequentEntities { get; init; } = new();
    public Dictionary<string, double> DomainPreferences { get; init; } = new();
    public Dictionary<string, double> TermPreferences { get; init; } = new();
    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;
}

public record IntentPattern
{
    public IntentType IntentType { get; init; }
    public int UsageCount { get; init; }
    public double SuccessRate { get; init; }
    public List<string> CommonKeywords { get; init; } = new();
}

public interface IEnhancedBusinessContextAnalyzer
{
    Task<BusinessContextProfile> AnalyzeUserQuestionAsync(string userQuestion, string? userId = null);
    Task<BusinessContextProfile> AnalyzeBusinessContextAsync(string userQuestion, string? userId = null);

    // Legacy compatibility methods
    Task<QueryIntent> ClassifyBusinessIntentAsync(string userQuestion);
    Task<List<BusinessEntity>> ExtractBusinessEntitiesAsync(string userQuestion);
    Task<BusinessDomain> DetectBusinessDomainAsync(string userQuestion);
    Task<List<string>> ExtractBusinessTermsAsync(string userQuestion);
    Task<TimeRange?> ExtractTimeContextAsync(string userQuestion);
}

public interface IAdvancedDomainDetector
{
    Task<BusinessDomain> DetectDomainAsync(string userQuestion);
}

public interface IBusinessTermExtractor
{
    Task<List<string>> ExtractBusinessTermsAsync(string userQuestion);
}

public interface ITimeContextAnalyzer
{
    Task<TimeRange?> ExtractTimeContextAsync(string userQuestion);
}

public interface IUserFeedbackLearner
{
    Task<UserAnalysisPatterns> GetUserPatternsAsync(string userId);
    Task RecordAnalysisAsync(BusinessContextProfile profile, string userQuestion, string? userId);
}
