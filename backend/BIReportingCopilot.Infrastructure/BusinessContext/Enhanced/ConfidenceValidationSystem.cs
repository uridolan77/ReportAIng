using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Extensions;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Advanced confidence validation system that validates AI-generated analysis results
/// </summary>
public class ConfidenceValidationSystem : IConfidenceValidationSystem
{
    private readonly ICacheService _cacheService;
    private readonly IUserFeedbackRepository _feedbackRepository;
    private readonly ILogger<ConfidenceValidationSystem> _logger;

    // Confidence thresholds for different validation levels
    private static readonly Dictionary<ValidationType, double> ConfidenceThresholds = new()
    {
        [ValidationType.Intent] = 0.7,
        [ValidationType.Entity] = 0.6,
        [ValidationType.Domain] = 0.65,
        [ValidationType.BusinessTerm] = 0.75
    };

    // Historical accuracy tracking for different analysis types
    private readonly Dictionary<string, List<ValidationResult>> _validationHistory = new();

    public ConfidenceValidationSystem(
        ICacheService cacheService,
        IUserFeedbackRepository feedbackRepository,
        ILogger<ConfidenceValidationSystem> logger)
    {
        _cacheService = cacheService;
        _feedbackRepository = feedbackRepository;
        _logger = logger;
    }

    public async Task<QueryIntent> ValidateIntentAsync(QueryIntent intent, string userQuestion)
    {
        _logger.LogDebug("Validating intent classification: {Intent} with confidence {Confidence:F3}", 
            intent.Type, intent.ConfidenceScore);

        var validationResult = await PerformValidationAsync(
            ValidationType.Intent, 
            intent.Type.ToString(), 
            intent.ConfidenceScore, 
            userQuestion);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Intent validation failed for {Intent}, applying fallback", intent.Type);
            return await ApplyIntentFallbackAsync(intent, userQuestion, validationResult);
        }

        // Enhance intent with validation metadata
        intent.Metadata["validation_result"] = validationResult;
        intent.Metadata["validation_timestamp"] = DateTime.UtcNow;
        intent.ConfidenceScore = validationResult.AdjustedConfidence;

        return intent;
    }

    public async Task<List<BusinessEntity>> ValidateEntitiesAsync(List<BusinessEntity> entities, string userQuestion)
    {
        _logger.LogDebug("Validating {Count} extracted entities", entities.Count);

        var validatedEntities = new List<BusinessEntity>();

        foreach (var entity in entities)
        {
            var validationResult = await PerformValidationAsync(
                ValidationType.Entity,
                $"{entity.Type}:{entity.Name}",
                entity.ConfidenceScore,
                userQuestion);

            if (validationResult.IsValid)
            {
                entity.ConfidenceScore = validationResult.AdjustedConfidence;
                entity.Metadata["validation_result"] = validationResult;
                validatedEntities.Add(entity);
            }
            else
            {
                _logger.LogDebug("Entity validation failed for {EntityType}:{EntityName}", 
                    entity.Type, entity.Name);
                
                // Try to recover entity with lower confidence
                if (entity.ConfidenceScore > 0.4)
                {
                    entity.ConfidenceScore = Math.Max(0.4, entity.ConfidenceScore * 0.7);
                    entity.Metadata["validation_result"] = validationResult;
                    entity.Metadata["validation_recovery"] = true;
                    validatedEntities.Add(entity);
                }
            }
        }

        // Cross-validate entities for consistency
        var crossValidatedEntities = await CrossValidateEntitiesAsync(validatedEntities, userQuestion);

        _logger.LogInformation("Entity validation: {Original} -> {Validated} entities", 
            entities.Count, crossValidatedEntities.Count);

        return crossValidatedEntities;
    }

    public async Task<BusinessDomain> ValidateDomainAsync(BusinessDomain domain, string userQuestion)
    {
        _logger.LogDebug("Validating domain classification: {Domain} with confidence {Confidence:F3}", 
            domain.Name, domain.RelevanceScore);

        var validationResult = await PerformValidationAsync(
            ValidationType.Domain,
            domain.Name,
            domain.RelevanceScore,
            userQuestion);

        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Domain validation failed for {Domain}, applying fallback", domain.Name);
            return await ApplyDomainFallbackAsync(domain, userQuestion, validationResult);
        }

        domain.RelevanceScore = validationResult.AdjustedConfidence;
        domain.Metadata["validation_result"] = validationResult;
        domain.Metadata["validation_timestamp"] = DateTime.UtcNow;

        return domain;
    }

    private async Task<ValidationResult> PerformValidationAsync(
        ValidationType validationType,
        string value,
        double originalConfidence,
        string userQuestion)
    {
        var threshold = ConfidenceThresholds[validationType];
        var validationKey = $"{validationType}:{value}";

        // Check historical accuracy for this specific classification
        var historicalAccuracy = await GetHistoricalAccuracyAsync(validationKey);
        
        // Perform multiple validation checks
        var checks = new List<ValidationCheck>
        {
            await PerformConfidenceThresholdCheckAsync(originalConfidence, threshold),
            await PerformHistoricalAccuracyCheckAsync(historicalAccuracy, validationType),
            await PerformConsistencyCheckAsync(validationType, value, userQuestion),
            await PerformContextualValidationAsync(validationType, value, userQuestion)
        };

        // Calculate overall validation score
        var validationScore = CalculateValidationScore(checks);
        var isValid = validationScore > 0.6;
        var adjustedConfidence = CalculateAdjustedConfidence(originalConfidence, validationScore, historicalAccuracy);

        var result = new ValidationResult
        {
            ValidationType = validationType,
            Value = value,
            OriginalConfidence = originalConfidence,
            AdjustedConfidence = adjustedConfidence,
            ValidationScore = validationScore,
            IsValid = isValid,
            Checks = checks,
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object>
            {
                ["historical_accuracy"] = historicalAccuracy,
                ["user_question"] = userQuestion.Substring(0, Math.Min(100, userQuestion.Length))
            }
        };

        // Store validation result for future learning
        await StoreValidationResultAsync(validationKey, result);

        return result;
    }

    private async Task<ValidationCheck> PerformConfidenceThresholdCheckAsync(double confidence, double threshold)
    {
        var passed = confidence >= threshold;
        return new ValidationCheck
        {
            CheckType = "ConfidenceThreshold",
            Passed = passed,
            Score = passed ? 1.0 : confidence / threshold,
            Details = $"Confidence {confidence:F3} vs threshold {threshold:F3}"
        };
    }

    private async Task<ValidationCheck> PerformHistoricalAccuracyCheckAsync(double historicalAccuracy, ValidationType validationType)
    {
        var minAccuracy = validationType switch
        {
            ValidationType.Intent => 0.75,
            ValidationType.Entity => 0.65,
            ValidationType.Domain => 0.70,
            ValidationType.BusinessTerm => 0.80,
            _ => 0.70
        };

        var passed = historicalAccuracy >= minAccuracy;
        return new ValidationCheck
        {
            CheckType = "HistoricalAccuracy",
            Passed = passed,
            Score = Math.Min(historicalAccuracy / minAccuracy, 1.0),
            Details = $"Historical accuracy {historicalAccuracy:F3} vs minimum {minAccuracy:F3}"
        };
    }

    private async Task<ValidationCheck> PerformConsistencyCheckAsync(ValidationType validationType, string value, string userQuestion)
    {
        // Check consistency with other extracted information
        var consistencyScore = validationType switch
        {
            ValidationType.Intent => await CheckIntentConsistencyAsync(value, userQuestion),
            ValidationType.Entity => await CheckEntityConsistencyAsync(value, userQuestion),
            ValidationType.Domain => await CheckDomainConsistencyAsync(value, userQuestion),
            _ => 0.8 // Default consistency score
        };

        return new ValidationCheck
        {
            CheckType = "Consistency",
            Passed = consistencyScore > 0.6,
            Score = consistencyScore,
            Details = $"Consistency score: {consistencyScore:F3}"
        };
    }

    private async Task<ValidationCheck> PerformContextualValidationAsync(ValidationType validationType, string value, string userQuestion)
    {
        // Validate based on question context and structure
        var contextualScore = await AnalyzeContextualFitAsync(validationType, value, userQuestion);

        return new ValidationCheck
        {
            CheckType = "Contextual",
            Passed = contextualScore > 0.5,
            Score = contextualScore,
            Details = $"Contextual fit score: {contextualScore:F3}"
        };
    }

    private double CalculateValidationScore(List<ValidationCheck> checks)
    {
        if (!checks.Any()) return 0.0;

        // Weighted scoring based on check importance
        var weights = new Dictionary<string, double>
        {
            ["ConfidenceThreshold"] = 0.3,
            ["HistoricalAccuracy"] = 0.25,
            ["Consistency"] = 0.25,
            ["Contextual"] = 0.2
        };

        var weightedSum = checks.Sum(check => 
            check.Score * weights.GetValueOrDefault(check.CheckType, 0.1));
        
        var totalWeight = checks.Sum(check => 
            weights.GetValueOrDefault(check.CheckType, 0.1));

        return totalWeight > 0 ? weightedSum / totalWeight : 0.0;
    }

    private double CalculateAdjustedConfidence(double originalConfidence, double validationScore, double historicalAccuracy)
    {
        // Adjust confidence based on validation results
        var adjustmentFactor = (validationScore * 0.7) + (historicalAccuracy * 0.3);
        var adjustedConfidence = originalConfidence * adjustmentFactor;
        
        // Apply bounds
        return Math.Max(0.1, Math.Min(0.98, adjustedConfidence));
    }

    private async Task<double> GetHistoricalAccuracyAsync(string validationKey)
    {
        if (_validationHistory.TryGetValue(validationKey, out var history) && history.Any())
        {
            var recentResults = history.TakeLast(20).ToList();
            return recentResults.Average(r => r.ValidationScore);
        }

        // Try to get from cache or database
        var cacheKey = $"validation_history:{validationKey}";
        var (found, cachedAccuracy) = await _cacheService.TryGetValueAsync<double>(cacheKey);
        if (found)
        {
            return cachedAccuracy;
        }

        // Default accuracy for new classifications
        return 0.75;
    }

    private async Task StoreValidationResultAsync(string validationKey, ValidationResult result)
    {
        if (!_validationHistory.ContainsKey(validationKey))
        {
            _validationHistory[validationKey] = new List<ValidationResult>();
        }

        _validationHistory[validationKey].Add(result);

        // Keep only recent results in memory
        if (_validationHistory[validationKey].Count > 50)
        {
            _validationHistory[validationKey] = _validationHistory[validationKey].TakeLast(30).ToList();
        }

        // Cache the updated accuracy
        var accuracy = _validationHistory[validationKey].Average(r => r.ValidationScore);
        var cacheKey = $"validation_history:{validationKey}";
        await _cacheService.SetAsync(cacheKey, accuracy, TimeSpan.FromHours(24));
    }

    private async Task<double> CheckIntentConsistencyAsync(string intentValue, string userQuestion)
    {
        // Check if intent is consistent with question structure and keywords
        var questionWords = userQuestion.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        return intentValue.ToLower() switch
        {
            "aggregation" => questionWords.Any(w => new[] { "total", "sum", "count", "average" }.Contains(w)) ? 0.9 : 0.5,
            "trend" => questionWords.Any(w => new[] { "trend", "over", "time", "growth" }.Contains(w)) ? 0.9 : 0.5,
            "comparison" => questionWords.Any(w => new[] { "compare", "vs", "versus", "against" }.Contains(w)) ? 0.9 : 0.5,
            "detail" => questionWords.Any(w => new[] { "list", "show", "details", "specific" }.Contains(w)) ? 0.9 : 0.5,
            _ => 0.7 // Default consistency for other intents
        };
    }

    private async Task<double> CheckEntityConsistencyAsync(string entityValue, string userQuestion)
    {
        // Check if entity makes sense in the context of the question
        var parts = entityValue.Split(':');
        if (parts.Length != 2) return 0.5;

        var entityType = parts[0];
        var entityName = parts[1].ToLower();
        var questionLower = userQuestion.ToLower();

        // Basic consistency checks
        if (entityType == "Table" && questionLower.Contains(entityName)) return 0.9;
        if (entityType == "Metric" && new[] { "revenue", "profit", "count", "total" }.Any(m => entityName.Contains(m))) return 0.9;
        if (entityType == "Dimension" && new[] { "country", "region", "category", "type" }.Any(d => entityName.Contains(d))) return 0.9;

        return 0.6; // Default consistency
    }

    private async Task<double> CheckDomainConsistencyAsync(string domainValue, string userQuestion)
    {
        // Check if domain is consistent with question content
        var questionLower = userQuestion.ToLower();
        
        return domainValue.ToLower() switch
        {
            "gaming" => questionLower.Contains("player") || questionLower.Contains("game") ? 0.9 : 0.4,
            "financial" => questionLower.Contains("revenue") || questionLower.Contains("profit") ? 0.9 : 0.4,
            "retail" => questionLower.Contains("product") || questionLower.Contains("sales") ? 0.9 : 0.4,
            _ => 0.6 // Default consistency for unknown domains
        };
    }

    private async Task<double> AnalyzeContextualFitAsync(ValidationType validationType, string value, string userQuestion)
    {
        // Analyze how well the classification fits the overall context
        var questionLength = userQuestion.Length;
        var questionComplexity = userQuestion.Split(' ').Length;
        
        // Simple heuristics for contextual fit
        if (questionComplexity < 5 && validationType == ValidationType.Intent && value == "Analytical")
            return 0.4; // Simple questions unlikely to be analytical
        
        if (questionComplexity > 15 && validationType == ValidationType.Intent && value == "Detail")
            return 0.4; // Complex questions unlikely to be simple detail requests
        
        return 0.8; // Default contextual fit
    }

    private async Task<List<BusinessEntity>> CrossValidateEntitiesAsync(List<BusinessEntity> entities, string userQuestion)
    {
        // Check for entity relationships and consistency
        var validatedEntities = new List<BusinessEntity>();

        foreach (var entity in entities)
        {
            var isConsistent = await CheckEntityRelationshipsAsync(entity, entities, userQuestion);
            if (isConsistent)
            {
                validatedEntities.Add(entity);
            }
            else
            {
                // Reduce confidence for inconsistent entities
                entity.ConfidenceScore *= 0.7;
                if (entity.ConfidenceScore > 0.4)
                {
                    entity.Metadata["cross_validation_warning"] = true;
                    validatedEntities.Add(entity);
                }
            }
        }

        return validatedEntities;
    }

    private async Task<bool> CheckEntityRelationshipsAsync(BusinessEntity entity, List<BusinessEntity> allEntities, string userQuestion)
    {
        // Check if entity relationships make sense
        var tables = allEntities.Where(e => e.Type == EntityType.Table).ToList();
        var metrics = allEntities.Where(e => e.Type == EntityType.Metric).ToList();
        var dimensions = allEntities.Where(e => e.Type == EntityType.Dimension).ToList();

        // Basic relationship validation
        if (entity.Type == EntityType.Metric && !tables.Any())
            return false; // Metrics usually need tables

        if (entity.Type == EntityType.Column && !tables.Any())
            return false; // Columns need tables

        return true; // Default to consistent
    }

    private async Task<QueryIntent> ApplyIntentFallbackAsync(QueryIntent intent, string userQuestion, ValidationResult validationResult)
    {
        // Apply fallback logic for failed intent validation
        var fallbackIntent = DetermineIntentFallback(userQuestion);
        
        _logger.LogInformation("Applied intent fallback: {Original} -> {Fallback}", intent.Type, fallbackIntent);
        
        return new QueryIntent
        {
            Type = fallbackIntent,
            ConfidenceScore = 0.6, // Lower confidence for fallback
            Description = GetIntentDescription(fallbackIntent),
            Keywords = intent.Keywords,
            Metadata = new Dictionary<string, object>
            {
                ["original_intent"] = intent.Type,
                ["fallback_applied"] = true,
                ["validation_failure"] = validationResult
            }
        };
    }

    private async Task<BusinessDomain> ApplyDomainFallbackAsync(BusinessDomain domain, string userQuestion, ValidationResult validationResult)
    {
        // Apply fallback logic for failed domain validation
        return new BusinessDomain
        {
            Name = "General",
            Description = "General business domain",
            RelevanceScore = 0.5,
            KeyConcepts = new List<string> { "business", "data", "analysis" },
            Metadata = new Dictionary<string, object>
            {
                ["original_domain"] = domain.Name,
                ["fallback_applied"] = true,
                ["validation_failure"] = validationResult
            }
        };
    }

    private IntentType DetermineIntentFallback(string userQuestion)
    {
        var questionLower = userQuestion.ToLower();
        
        if (questionLower.Contains("total") || questionLower.Contains("sum") || questionLower.Contains("count"))
            return IntentType.Aggregation;
        
        if (questionLower.Contains("list") || questionLower.Contains("show") || questionLower.Contains("display"))
            return IntentType.Detail;
        
        return IntentType.Analytical; // Default fallback
    }

    private string GetIntentDescription(IntentType intentType) => intentType switch
    {
        IntentType.Aggregation => "Aggregate calculations and summaries",
        IntentType.Trend => "Time-based analysis and trends",
        IntentType.Comparison => "Comparative analysis between entities",
        IntentType.Detail => "Detailed record retrieval",
        IntentType.Exploratory => "Data exploration and discovery",
        IntentType.Operational => "Operational and real-time queries",
        IntentType.Analytical => "Complex analytical queries",
        _ => "General business query"
    };
}

public enum ValidationType
{
    Intent,
    Entity,
    Domain,
    BusinessTerm
}

public record ValidationResult
{
    public ValidationType ValidationType { get; init; }
    public string Value { get; init; } = string.Empty;
    public double OriginalConfidence { get; init; }
    public double AdjustedConfidence { get; init; }
    public double ValidationScore { get; init; }
    public bool IsValid { get; init; }
    public List<ValidationCheck> Checks { get; init; } = new();
    public DateTime Timestamp { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

public record ValidationCheck
{
    public string CheckType { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public double Score { get; init; }
    public string Details { get; init; } = string.Empty;
}

public interface IConfidenceValidationSystem
{
    Task<QueryIntent> ValidateIntentAsync(QueryIntent intent, string userQuestion);
    Task<List<BusinessEntity>> ValidateEntitiesAsync(List<BusinessEntity> entities, string userQuestion);
    Task<BusinessDomain> ValidateDomainAsync(BusinessDomain domain, string userQuestion);
}

// IUserFeedbackRepository moved to shared interfaces to avoid duplication
