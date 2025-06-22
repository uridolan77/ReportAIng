using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Extensions;
using BIReportingCopilot.Core.Interfaces.Repository;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Context-aware scoring engine that adapts similarity weights based on query type, domain, and user patterns
/// </summary>
public class ContextualScoringEngine : IContextualScoringEngine
{
    private readonly ICacheService _cacheService;
    private readonly IUserFeedbackRepository _feedbackRepository;
    private readonly ILogger<ContextualScoringEngine> _logger;

    // Intent-specific scoring weights for different factors
    private static readonly Dictionary<IntentType, ScoringWeights> IntentScoringWeights = new()
    {
        [IntentType.Aggregation] = new()
        {
            SemanticSimilarity = 0.35,
            BusinessPurposeMatch = 0.25,
            DomainRelevance = 0.15,
            UsageFrequency = 0.10,
            UserPreference = 0.10,
            TemporalRelevance = 0.05
        },
        [IntentType.Trend] = new()
        {
            SemanticSimilarity = 0.30,
            BusinessPurposeMatch = 0.20,
            DomainRelevance = 0.15,
            UsageFrequency = 0.10,
            UserPreference = 0.10,
            TemporalRelevance = 0.15  // Higher for trend analysis
        },
        [IntentType.Comparison] = new()
        {
            SemanticSimilarity = 0.40,
            BusinessPurposeMatch = 0.25,
            DomainRelevance = 0.15,
            UsageFrequency = 0.08,
            UserPreference = 0.07,
            TemporalRelevance = 0.05
        },
        [IntentType.Detail] = new()
        {
            SemanticSimilarity = 0.45,  // High semantic match for specific details
            BusinessPurposeMatch = 0.20,
            DomainRelevance = 0.15,
            UsageFrequency = 0.12,
            UserPreference = 0.08,
            TemporalRelevance = 0.00
        },
        [IntentType.Exploratory] = new()
        {
            SemanticSimilarity = 0.25,  // Lower semantic, higher exploration factors
            BusinessPurposeMatch = 0.30,
            DomainRelevance = 0.20,
            UsageFrequency = 0.05,
            UserPreference = 0.15,
            TemporalRelevance = 0.05
        },
        [IntentType.Operational] = new()
        {
            SemanticSimilarity = 0.35,
            BusinessPurposeMatch = 0.25,
            DomainRelevance = 0.15,
            UsageFrequency = 0.15,  // Higher for operational queries
            UserPreference = 0.10,
            TemporalRelevance = 0.00
        },
        [IntentType.Analytical] = new()
        {
            SemanticSimilarity = 0.30,
            BusinessPurposeMatch = 0.25,
            DomainRelevance = 0.20,
            UsageFrequency = 0.10,
            UserPreference = 0.10,
            TemporalRelevance = 0.05
        }
    };

    // Domain-specific boost factors
    private static readonly Dictionary<string, DomainBoostFactors> DomainBoosts = new()
    {
        ["Gaming"] = new()
        {
            PlayerMetrics = 1.3,
            RevenueMetrics = 1.2,
            EngagementMetrics = 1.4,
            SessionMetrics = 1.3,
            DefaultBoost = 1.0
        },
        ["Financial"] = new()
        {
            RevenueMetrics = 1.4,
            ProfitMetrics = 1.3,
            CostMetrics = 1.2,
            ComplianceMetrics = 1.3,
            DefaultBoost = 1.0
        },
        ["Retail"] = new()
        {
            SalesMetrics = 1.4,
            InventoryMetrics = 1.3,
            CustomerMetrics = 1.2,
            ProductMetrics = 1.2,
            DefaultBoost = 1.0
        },
        ["General"] = new()
        {
            DefaultBoost = 1.0
        }
    };

    // Performance tracking for scoring optimization
    private readonly Dictionary<string, ScoringPerformanceData> _scoringPerformance = new();
    private readonly object _performanceLock = new();

    public ContextualScoringEngine(
        ICacheService cacheService,
        IUserFeedbackRepository feedbackRepository,
        ILogger<ContextualScoringEngine> logger)
    {
        _cacheService = cacheService;
        _feedbackRepository = feedbackRepository;
        _logger = logger;
    }

    public async Task<double> ApplyContextualAdjustmentsAsync(
        double baseSimilarity, 
        BusinessTableInfoDto table, 
        BusinessContextProfile profile)
    {
        var cacheKey = $"table_contextual_score:{table.Id}:{profile.Intent.Type}:{profile.Domain.Name}:{baseSimilarity:F3}";
        
        var (found, cachedScore) = await _cacheService.TryGetValueAsync<double>(cacheKey);
        if (found)
        {
            return cachedScore;
        }

        try
        {
            var startTime = DateTime.UtcNow;
            
            // Get scoring weights for the intent type
            var weights = GetScoringWeights(profile.Intent.Type);
            
            // Calculate individual scoring factors
            var factors = await CalculateTableScoringFactorsAsync(table, profile);
            
            // Apply weighted scoring
            var contextualScore = CalculateWeightedScore(baseSimilarity, factors, weights);
            
            // Apply domain-specific boosts
            contextualScore = ApplyDomainBoosts(contextualScore, table, profile.Domain.Name);
            
            // Apply user-specific adjustments
            contextualScore = await ApplyUserAdjustmentsAsync(contextualScore, table, profile);
            
            // Apply temporal adjustments if relevant
            if (profile.TimeContext != null)
            {
                contextualScore = ApplyTemporalAdjustments(contextualScore, table, profile.TimeContext);
            }
            
            // Ensure score is within valid bounds
            contextualScore = Math.Max(0.0, Math.Min(1.0, contextualScore));
            
            // Cache the result for 30 minutes
            await _cacheService.SetAsync(cacheKey, contextualScore, TimeSpan.FromMinutes(30));
            
            // Record performance metrics
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            await RecordScoringPerformanceAsync("table_scoring", duration, baseSimilarity, contextualScore);
            
            _logger.LogDebug("Contextual scoring: {Table} - Base: {Base:F3} → Contextual: {Contextual:F3} ({Duration}ms)",
                table.TableName, baseSimilarity, contextualScore, duration);
            
            return contextualScore;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying contextual adjustments for table {TableId}", table.Id);
            return baseSimilarity; // Fallback to base similarity
        }
    }

    public async Task<double> ApplyColumnContextualAdjustmentsAsync(
        double baseSimilarity, 
        BusinessColumnInfo column, 
        BusinessContextProfile profile)
    {
        var cacheKey = $"column_contextual_score:{column.Id}:{profile.Intent.Type}:{baseSimilarity:F3}";
        
        var (found, cachedScore) = await _cacheService.TryGetValueAsync<double>(cacheKey);
        if (found)
        {
            return cachedScore;
        }

        try
        {
            // Get scoring weights for the intent type
            var weights = GetScoringWeights(profile.Intent.Type);
            
            // Calculate column-specific scoring factors
            var factors = await CalculateColumnScoringFactorsAsync(column, profile);
            
            // Apply weighted scoring
            var contextualScore = CalculateWeightedScore(baseSimilarity, factors, weights);
            
            // Apply column-specific boosts
            contextualScore = ApplyColumnTypeBoosts(contextualScore, column, profile);
            
            // Apply entity matching boost
            contextualScore = ApplyEntityMatchingBoost(contextualScore, column, profile.Entities);
            
            // Ensure score is within valid bounds
            contextualScore = Math.Max(0.0, Math.Min(1.0, contextualScore));
            
            // Cache the result for 30 minutes
            await _cacheService.SetAsync(cacheKey, contextualScore, TimeSpan.FromMinutes(30));
            
            _logger.LogDebug("Column contextual scoring: {Column} - Base: {Base:F3} → Contextual: {Contextual:F3}",
                column.ColumnName, baseSimilarity, contextualScore);
            
            return contextualScore;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying contextual adjustments for column {ColumnId}", column.Id);
            return baseSimilarity;
        }
    }

    public async Task<ScoringOptimizationReport> GenerateScoringReportAsync(
        IntentType? intentFilter = null,
        string? domainFilter = null)
    {
        var report = new ScoringOptimizationReport
        {
            GeneratedAt = DateTime.UtcNow,
            IntentFilter = intentFilter,
            DomainFilter = domainFilter
        };

        lock (_performanceLock)
        {
            foreach (var (key, data) in _scoringPerformance)
            {
                var avgDuration = data.TotalOperations > 0 ? data.TotalDuration / data.TotalOperations : 0;
                var avgImprovement = data.TotalOperations > 0 ? data.TotalImprovement / data.TotalOperations : 0;
                
                report.PerformanceMetrics.Add(new ScoringPerformanceMetrics
                {
                    OperationType = key,
                    TotalOperations = data.TotalOperations,
                    AverageDuration = avgDuration,
                    AverageScoreImprovement = avgImprovement,
                    LastUpdated = data.LastUpdated
                });
            }
        }

        return report;
    }

    private ScoringWeights GetScoringWeights(IntentType intentType)
    {
        return IntentScoringWeights.GetValueOrDefault(intentType, IntentScoringWeights[IntentType.Analytical]);
    }

    private async Task<ScoringFactors> CalculateTableScoringFactorsAsync(
        BusinessTableInfoDto table, 
        BusinessContextProfile profile)
    {
        var factors = new ScoringFactors();

        // Business purpose match factor
        factors.BusinessPurposeMatch = CalculateBusinessPurposeMatch(table.BusinessPurpose, profile);

        // Domain relevance factor
        factors.DomainRelevance = CalculateDomainRelevance(table.DomainClassification, profile.Domain.Name);

        // Usage frequency factor (would come from analytics)
        factors.UsageFrequency = await GetTableUsageFrequencyAsync(table.Id);

        // User preference factor
        factors.UserPreference = await GetUserTablePreferenceAsync(table.Id, profile.UserId);

        // Temporal relevance factor
        factors.TemporalRelevance = CalculateTemporalRelevance(table, profile.TimeContext);

        return factors;
    }

    private async Task<ScoringFactors> CalculateColumnScoringFactorsAsync(
        BusinessColumnInfo column, 
        BusinessContextProfile profile)
    {
        var factors = new ScoringFactors();

        // Business meaning match
        factors.BusinessPurposeMatch = CalculateBusinessMeaningMatch(column.BusinessMeaning, profile);

        // Data type relevance
        factors.DomainRelevance = CalculateDataTypeRelevance(column.DataType, profile.Intent.Type);

        // Column usage frequency
        factors.UsageFrequency = await GetColumnUsageFrequencyAsync(column.Id);

        // User preference for this column
        factors.UserPreference = await GetUserColumnPreferenceAsync(column.Id, profile.UserId);

        return factors;
    }

    private double CalculateWeightedScore(double baseSimilarity, ScoringFactors factors, ScoringWeights weights)
    {
        return (baseSimilarity * weights.SemanticSimilarity) +
               (factors.BusinessPurposeMatch * weights.BusinessPurposeMatch) +
               (factors.DomainRelevance * weights.DomainRelevance) +
               (factors.UsageFrequency * weights.UsageFrequency) +
               (factors.UserPreference * weights.UserPreference) +
               (factors.TemporalRelevance * weights.TemporalRelevance);
    }

    private double ApplyDomainBoosts(double score, BusinessTableInfoDto table, string domainName)
    {
        if (!DomainBoosts.TryGetValue(domainName, out var boosts))
        {
            return score;
        }

        var tableName = table.TableName.ToLower();
        var businessPurpose = table.BusinessPurpose?.ToLower() ?? "";

        // Apply specific metric boosts based on table characteristics
        if (tableName.Contains("player") || businessPurpose.Contains("player"))
            return score * boosts.PlayerMetrics;
        
        if (tableName.Contains("revenue") || businessPurpose.Contains("revenue"))
            return score * boosts.RevenueMetrics;
        
        if (tableName.Contains("engagement") || businessPurpose.Contains("engagement"))
            return score * boosts.EngagementMetrics;
        
        if (tableName.Contains("session") || businessPurpose.Contains("session"))
            return score * boosts.SessionMetrics;

        return score * boosts.DefaultBoost;
    }

    private async Task<double> ApplyUserAdjustmentsAsync(
        double score, 
        BusinessTableInfoDto table, 
        BusinessContextProfile profile)
    {
        if (string.IsNullOrEmpty(profile.UserId))
        {
            return score;
        }

        // Get user's historical success rate with this table
        var userSuccessRate = await GetUserTableSuccessRateAsync(table.Id, profile.UserId);
        
        // Boost score for tables the user has had success with
        if (userSuccessRate > 0.7)
        {
            return score * 1.15; // 15% boost
        }
        
        if (userSuccessRate < 0.3)
        {
            return score * 0.9; // 10% penalty
        }

        return score;
    }

    private double ApplyTemporalAdjustments(double score, BusinessTableInfoDto table, TimeRange timeContext)
    {
        var tableName = table.TableName.ToLower();
        var businessPurpose = table.BusinessPurpose?.ToLower() ?? "";

        // Boost tables that are time-relevant when time context is present
        if (tableName.Contains("daily") || tableName.Contains("time") || tableName.Contains("date") ||
            businessPurpose.Contains("temporal") || businessPurpose.Contains("time"))
        {
            return score * 1.2; // 20% boost for time-relevant tables
        }

        return score;
    }

    private double ApplyColumnTypeBoosts(double score, BusinessColumnInfo column, BusinessContextProfile profile)
    {
        var columnName = column.ColumnName.ToLower();
        var dataType = column.DataType.ToLower();

        // Boost key columns for certain intent types
        if (profile.Intent.Type == IntentType.Aggregation)
        {
            if (dataType.Contains("int") || dataType.Contains("decimal") || dataType.Contains("float"))
            {
                return score * 1.2; // Boost numeric columns for aggregation
            }
        }

        if (profile.Intent.Type == IntentType.Detail)
        {
            if (columnName.Contains("id") || columnName.Contains("key"))
            {
                return score * 1.15; // Boost key columns for detail queries
            }
        }

        if (profile.Intent.Type == IntentType.Trend)
        {
            if (dataType.Contains("date") || dataType.Contains("time"))
            {
                return score * 1.3; // Strong boost for date/time columns in trend analysis
            }
        }

        return score;
    }

    private double ApplyEntityMatchingBoost(double score, BusinessColumnInfo column, List<BusinessEntity> entities)
    {
        var columnName = column.ColumnName.ToLower();
        
        foreach (var entity in entities.Where(e => e.Type == EntityType.Column || e.Type == EntityType.Metric))
        {
            if (columnName.Contains(entity.Name.ToLower()) || entity.Name.ToLower().Contains(columnName))
            {
                return score * (1.0 + (entity.ConfidenceScore * 0.3)); // Boost based on entity confidence
            }
        }

        return score;
    }

    // Helper methods for calculating individual factors
    private double CalculateBusinessPurposeMatch(string businessPurpose, BusinessContextProfile profile)
    {
        if (string.IsNullOrEmpty(businessPurpose))
            return 0.5;

        var purposeLower = businessPurpose.ToLower();
        var matchScore = 0.0;

        foreach (var term in profile.BusinessTerms)
        {
            if (purposeLower.Contains(term.ToLower()))
            {
                matchScore += 0.2;
            }
        }

        return Math.Min(matchScore, 1.0);
    }

    private double CalculateDomainRelevance(string tableClassification, string profileDomain)
    {
        if (string.IsNullOrEmpty(tableClassification) || string.IsNullOrEmpty(profileDomain))
            return 0.5;

        return tableClassification.Contains(profileDomain, StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.3;
    }

    private double CalculateBusinessMeaningMatch(string businessMeaning, BusinessContextProfile profile)
    {
        if (string.IsNullOrEmpty(businessMeaning))
            return 0.5;

        var meaningLower = businessMeaning.ToLower();
        var questionLower = profile.OriginalQuestion.ToLower();

        // Simple keyword overlap calculation
        var questionWords = questionLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var meaningWords = meaningLower.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var overlap = questionWords.Intersect(meaningWords).Count();
        var totalWords = Math.Max(questionWords.Length, meaningWords.Length);

        return totalWords > 0 ? (double)overlap / totalWords : 0.5;
    }

    private double CalculateDataTypeRelevance(string dataType, IntentType intentType)
    {
        var dataTypeLower = dataType.ToLower();

        return intentType switch
        {
            IntentType.Aggregation when dataTypeLower.Contains("int") || dataTypeLower.Contains("decimal") => 1.0,
            IntentType.Trend when dataTypeLower.Contains("date") || dataTypeLower.Contains("time") => 1.0,
            IntentType.Detail when dataTypeLower.Contains("varchar") || dataTypeLower.Contains("text") => 0.8,
            _ => 0.6
        };
    }

    private double CalculateTemporalRelevance(BusinessTableInfoDto table, TimeRange? timeContext)
    {
        if (timeContext == null)
            return 0.5;

        var tableName = table.TableName.ToLower();
        var businessPurpose = table.BusinessPurpose?.ToLower() ?? "";

        if (tableName.Contains("daily") || tableName.Contains("monthly") || tableName.Contains("yearly") ||
            businessPurpose.Contains("time") || businessPurpose.Contains("temporal"))
        {
            return 1.0;
        }

        return 0.3;
    }

    // Async helper methods (would integrate with actual analytics/user data)
    private async Task<double> GetTableUsageFrequencyAsync(long tableId)
    {
        // This would query actual usage analytics
        await Task.CompletedTask;
        return 0.5; // Default frequency
    }

    private async Task<double> GetUserTablePreferenceAsync(long tableId, string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return 0.5;

        // This would query user preference data
        await Task.CompletedTask;
        return 0.5; // Default preference
    }

    private async Task<double> GetColumnUsageFrequencyAsync(long columnId)
    {
        await Task.CompletedTask;
        return 0.5;
    }

    private async Task<double> GetUserColumnPreferenceAsync(long columnId, string userId)
    {
        await Task.CompletedTask;
        return 0.5;
    }

    private async Task<double> GetUserTableSuccessRateAsync(long tableId, string userId)
    {
        await Task.CompletedTask;
        return 0.5;
    }

    private async Task RecordScoringPerformanceAsync(string operationType, double duration, double baseScore, double contextualScore)
    {
        lock (_performanceLock)
        {
            if (!_scoringPerformance.ContainsKey(operationType))
            {
                _scoringPerformance[operationType] = new ScoringPerformanceData();
            }

            var data = _scoringPerformance[operationType];
            data.TotalOperations++;
            data.TotalDuration += duration;
            data.TotalImprovement += (contextualScore - baseScore);
            data.LastUpdated = DateTime.UtcNow;
        }
    }
}

// Supporting data structures
public record ScoringWeights
{
    public double SemanticSimilarity { get; init; }
    public double BusinessPurposeMatch { get; init; }
    public double DomainRelevance { get; init; }
    public double UsageFrequency { get; init; }
    public double UserPreference { get; init; }
    public double TemporalRelevance { get; init; }
}

public record ScoringFactors
{
    public double BusinessPurposeMatch { get; set; }
    public double DomainRelevance { get; set; }
    public double UsageFrequency { get; set; }
    public double UserPreference { get; set; }
    public double TemporalRelevance { get; set; }
}

public record DomainBoostFactors
{
    public double PlayerMetrics { get; init; } = 1.0;
    public double RevenueMetrics { get; init; } = 1.0;
    public double EngagementMetrics { get; init; } = 1.0;
    public double SessionMetrics { get; init; } = 1.0;
    public double SalesMetrics { get; init; } = 1.0;
    public double InventoryMetrics { get; init; } = 1.0;
    public double CustomerMetrics { get; init; } = 1.0;
    public double ProductMetrics { get; init; } = 1.0;
    public double ProfitMetrics { get; init; } = 1.0;
    public double CostMetrics { get; init; } = 1.0;
    public double ComplianceMetrics { get; init; } = 1.0;
    public double DefaultBoost { get; init; } = 1.0;
}

public class ScoringPerformanceData
{
    public int TotalOperations { get; set; }
    public double TotalDuration { get; set; }
    public double TotalImprovement { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public record ScoringOptimizationReport
{
    public DateTime GeneratedAt { get; init; }
    public IntentType? IntentFilter { get; init; }
    public string? DomainFilter { get; init; }
    public List<ScoringPerformanceMetrics> PerformanceMetrics { get; init; } = new();
}

public record ScoringPerformanceMetrics
{
    public string OperationType { get; init; } = string.Empty;
    public int TotalOperations { get; init; }
    public double AverageDuration { get; init; }
    public double AverageScoreImprovement { get; init; }
    public DateTime LastUpdated { get; init; }
}
