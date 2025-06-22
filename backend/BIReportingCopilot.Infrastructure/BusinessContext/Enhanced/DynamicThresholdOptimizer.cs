using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Extensions;
using BIReportingCopilot.Core.Interfaces.Repository;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Dynamic threshold optimizer that adapts similarity thresholds based on performance data and user feedback
/// </summary>
public class DynamicThresholdOptimizer : IDynamicThresholdOptimizer
{
    private readonly ICacheService _cacheService;
    private readonly IUserFeedbackRepository _feedbackRepository;
    private readonly ILogger<DynamicThresholdOptimizer> _logger;

    // Base thresholds for different search types and intents
    private static readonly Dictionary<string, Dictionary<IntentType, double>> BaseThresholds = new()
    {
        ["table_search"] = new()
        {
            [IntentType.Aggregation] = 0.35,
            [IntentType.Trend] = 0.40,
            [IntentType.Comparison] = 0.38,
            [IntentType.Detail] = 0.30,
            [IntentType.Exploratory] = 0.45,
            [IntentType.Operational] = 0.32,
            [IntentType.Analytical] = 0.42
        },
        ["column_search"] = new()
        {
            [IntentType.Aggregation] = 0.40,
            [IntentType.Trend] = 0.45,
            [IntentType.Comparison] = 0.42,
            [IntentType.Detail] = 0.35,
            [IntentType.Exploratory] = 0.50,
            [IntentType.Operational] = 0.37,
            [IntentType.Analytical] = 0.47
        },
        ["business_term_search"] = new()
        {
            [IntentType.Aggregation] = 0.50,
            [IntentType.Trend] = 0.55,
            [IntentType.Comparison] = 0.52,
            [IntentType.Detail] = 0.45,
            [IntentType.Exploratory] = 0.60,
            [IntentType.Operational] = 0.47,
            [IntentType.Analytical] = 0.57
        }
    };

    // Domain-specific adjustments
    private static readonly Dictionary<string, double> DomainAdjustments = new()
    {
        ["Gaming"] = 0.05,      // Gaming domain has rich metadata, can be more selective
        ["Financial"] = 0.03,   // Financial domain has good metadata
        ["Retail"] = 0.02,      // Retail domain has moderate metadata
        ["Healthcare"] = 0.04,  // Healthcare has specific terminology
        ["General"] = -0.05     // General domain needs lower thresholds
    };

    // Performance tracking for threshold optimization
    private readonly Dictionary<string, ThresholdPerformanceData> _performanceData = new();
    private readonly object _performanceLock = new();

    public DynamicThresholdOptimizer(
        ICacheService cacheService,
        IUserFeedbackRepository feedbackRepository,
        ILogger<DynamicThresholdOptimizer> logger)
    {
        _cacheService = cacheService;
        _feedbackRepository = feedbackRepository;
        _logger = logger;
    }

    public async Task<double> GetOptimalThresholdAsync(IntentType intentType, string domainName, string searchType)
    {
        var cacheKey = $"optimal_threshold:{intentType}:{domainName}:{searchType}";
        
        // Check cache first
        var (found, cachedThreshold) = await _cacheService.TryGetValueAsync<double>(cacheKey);
        if (found)
        {
            return cachedThreshold;
        }

        _logger.LogDebug("Calculating optimal threshold for {SearchType} with intent {Intent} in domain {Domain}", 
            searchType, intentType, domainName);

        try
        {
            // Get base threshold
            var baseThreshold = GetBaseThreshold(intentType, searchType);
            
            // Apply domain adjustment
            var domainAdjustment = GetDomainAdjustment(domainName);
            
            // Get historical performance data
            var performanceAdjustment = await GetPerformanceAdjustmentAsync(intentType, domainName, searchType);
            
            // Get user feedback adjustment
            var feedbackAdjustment = await GetFeedbackAdjustmentAsync(intentType, domainName, searchType);
            
            // Calculate final threshold
            var optimalThreshold = baseThreshold + domainAdjustment + performanceAdjustment + feedbackAdjustment;
            
            // Apply bounds (0.1 to 0.9)
            optimalThreshold = Math.Max(0.1, Math.Min(0.9, optimalThreshold));
            
            // Cache for 1 hour
            await _cacheService.SetAsync(cacheKey, optimalThreshold, TimeSpan.FromHours(1));
            
            _logger.LogDebug("Optimal threshold calculated: {Threshold:F3} (base: {Base:F3}, domain: {Domain:+F3;-F3;0}, " +
                           "performance: {Performance:+F3;-F3;0}, feedback: {Feedback:+F3;-F3;0})",
                optimalThreshold, baseThreshold, domainAdjustment, performanceAdjustment, feedbackAdjustment);
            
            return optimalThreshold;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating optimal threshold, using base threshold");
            return GetBaseThreshold(intentType, searchType);
        }
    }

    public async Task RecordSearchPerformanceAsync(
        IntentType intentType, 
        string domainName, 
        string searchType, 
        double threshold, 
        int resultsCount, 
        double userSatisfactionScore)
    {
        var key = $"{intentType}:{domainName}:{searchType}";
        
        lock (_performanceLock)
        {
            if (!_performanceData.ContainsKey(key))
            {
                _performanceData[key] = new ThresholdPerformanceData();
            }

            var data = _performanceData[key];
            data.TotalSearches++;
            data.ThresholdSum += threshold;
            data.ResultsCountSum += resultsCount;
            data.SatisfactionScoreSum += userSatisfactionScore;
            data.LastUpdated = DateTime.UtcNow;

            // Track threshold effectiveness
            if (userSatisfactionScore > 0.7) // Good result
            {
                data.GoodResults++;
                data.GoodThresholdSum += threshold;
            }
            else if (userSatisfactionScore < 0.4) // Poor result
            {
                data.PoorResults++;
                data.PoorThresholdSum += threshold;
            }

            // Keep only recent data (sliding window)
            if (data.TotalSearches > 1000)
            {
                // Reset with weighted averages to maintain trends
                data.TotalSearches = 500;
                data.GoodResults = (int)(data.GoodResults * 0.5);
                data.PoorResults = (int)(data.PoorResults * 0.5);
                data.ThresholdSum *= 0.5;
                data.ResultsCountSum *= 0.5;
                data.SatisfactionScoreSum *= 0.5;
                data.GoodThresholdSum *= 0.5;
                data.PoorThresholdSum *= 0.5;
            }
        }

        // Persist performance data periodically
        if (_performanceData[key].TotalSearches % 10 == 0)
        {
            await PersistPerformanceDataAsync(key, _performanceData[key]);
        }
    }

    public async Task<ThresholdOptimizationReport> GenerateOptimizationReportAsync(
        IntentType? intentType = null, 
        string? domainName = null)
    {
        var report = new ThresholdOptimizationReport
        {
            GeneratedAt = DateTime.UtcNow,
            IntentFilter = intentType,
            DomainFilter = domainName
        };

        lock (_performanceLock)
        {
            var filteredData = _performanceData.Where(kvp =>
            {
                var parts = kvp.Key.Split(':');
                if (parts.Length != 3) return false;

                var dataIntent = Enum.Parse<IntentType>(parts[0]);
                var dataDomain = parts[1];

                return (intentType == null || dataIntent == intentType) &&
                       (domainName == null || dataDomain.Equals(domainName, StringComparison.OrdinalIgnoreCase));
            }).ToList();

            foreach (var (key, data) in filteredData)
            {
                var parts = key.Split(':');
                var dataIntent = Enum.Parse<IntentType>(parts[0]);
                var dataDomain = parts[1];
                var searchType = parts[2];

                var avgThreshold = data.TotalSearches > 0 ? data.ThresholdSum / data.TotalSearches : 0;
                var avgResults = data.TotalSearches > 0 ? data.ResultsCountSum / data.TotalSearches : 0;
                var avgSatisfaction = data.TotalSearches > 0 ? data.SatisfactionScoreSum / data.TotalSearches : 0;
                var goodThresholdAvg = data.GoodResults > 0 ? data.GoodThresholdSum / data.GoodResults : 0;
                var poorThresholdAvg = data.PoorResults > 0 ? data.PoorThresholdSum / data.PoorResults : 0;

                report.ThresholdAnalytics.Add(new ThresholdAnalytics
                {
                    IntentType = dataIntent,
                    DomainName = dataDomain,
                    SearchType = searchType,
                    TotalSearches = data.TotalSearches,
                    AverageThreshold = avgThreshold,
                    AverageResultsCount = avgResults,
                    AverageSatisfactionScore = avgSatisfaction,
                    GoodResultsThreshold = goodThresholdAvg,
                    PoorResultsThreshold = poorThresholdAvg,
                    OptimalThresholdRecommendation = CalculateOptimalRecommendation(data),
                    LastUpdated = data.LastUpdated
                });
            }
        }

        return report;
    }

    private double GetBaseThreshold(IntentType intentType, string searchType)
    {
        if (BaseThresholds.TryGetValue(searchType, out var intentThresholds) &&
            intentThresholds.TryGetValue(intentType, out var threshold))
        {
            return threshold;
        }

        // Default threshold if not found
        return searchType switch
        {
            "table_search" => 0.35,
            "column_search" => 0.40,
            "business_term_search" => 0.50,
            _ => 0.40
        };
    }

    private double GetDomainAdjustment(string domainName)
    {
        return DomainAdjustments.GetValueOrDefault(domainName, 0.0);
    }

    private async Task<double> GetPerformanceAdjustmentAsync(IntentType intentType, string domainName, string searchType)
    {
        var key = $"{intentType}:{domainName}:{searchType}";
        
        lock (_performanceLock)
        {
            if (!_performanceData.TryGetValue(key, out var data) || data.TotalSearches < 10)
            {
                return 0.0; // Not enough data for adjustment
            }

            var avgSatisfaction = data.SatisfactionScoreSum / data.TotalSearches;
            var avgResults = data.ResultsCountSum / data.TotalSearches;

            // If satisfaction is low and results are too many, increase threshold
            if (avgSatisfaction < 0.5 && avgResults > 10)
            {
                return 0.05;
            }

            // If satisfaction is low and results are too few, decrease threshold
            if (avgSatisfaction < 0.5 && avgResults < 3)
            {
                return -0.05;
            }

            // If satisfaction is high, maintain current approach
            if (avgSatisfaction > 0.7)
            {
                return 0.0;
            }

            return 0.0;
        }
    }

    private async Task<double> GetFeedbackAdjustmentAsync(IntentType intentType, string domainName, string searchType)
    {
        try
        {
            var feedbackKey = $"{intentType}:{domainName}:{searchType}";
            var feedbackScore = await _feedbackRepository.GetThresholdFeedbackScoreAsync(feedbackKey);
            
            // Convert feedback score to threshold adjustment
            if (feedbackScore > 0.8) return 0.02;  // Very positive feedback - slightly increase threshold
            if (feedbackScore < 0.4) return -0.03; // Negative feedback - decrease threshold
            
            return 0.0;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting feedback adjustment");
            return 0.0;
        }
    }

    private async Task PersistPerformanceDataAsync(string key, ThresholdPerformanceData data)
    {
        try
        {
            var cacheKey = $"threshold_performance:{key}";
            await _cacheService.SetAsync(cacheKey, data, TimeSpan.FromDays(30));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error persisting performance data for key: {Key}", key);
        }
    }

    private double CalculateOptimalRecommendation(ThresholdPerformanceData data)
    {
        if (data.GoodResults > 0 && data.PoorResults > 0)
        {
            var goodAvg = data.GoodThresholdSum / data.GoodResults;
            var poorAvg = data.PoorThresholdSum / data.PoorResults;
            
            // Recommend threshold closer to good results
            return (goodAvg * 0.7) + (poorAvg * 0.3);
        }
        
        if (data.GoodResults > 0)
        {
            return data.GoodThresholdSum / data.GoodResults;
        }
        
        if (data.TotalSearches > 0)
        {
            return data.ThresholdSum / data.TotalSearches;
        }
        
        return 0.4; // Default recommendation
    }
}

public class ThresholdPerformanceData
{
    public int TotalSearches { get; set; }
    public double ThresholdSum { get; set; }
    public double ResultsCountSum { get; set; }
    public double SatisfactionScoreSum { get; set; }
    public int GoodResults { get; set; }
    public int PoorResults { get; set; }
    public double GoodThresholdSum { get; set; }
    public double PoorThresholdSum { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public record ThresholdOptimizationReport
{
    public DateTime GeneratedAt { get; init; }
    public IntentType? IntentFilter { get; init; }
    public string? DomainFilter { get; init; }
    public List<ThresholdAnalytics> ThresholdAnalytics { get; init; } = new();
}

public record ThresholdAnalytics
{
    public IntentType IntentType { get; init; }
    public string DomainName { get; init; } = string.Empty;
    public string SearchType { get; init; } = string.Empty;
    public int TotalSearches { get; init; }
    public double AverageThreshold { get; init; }
    public double AverageResultsCount { get; init; }
    public double AverageSatisfactionScore { get; init; }
    public double GoodResultsThreshold { get; init; }
    public double PoorResultsThreshold { get; init; }
    public double OptimalThresholdRecommendation { get; init; }
    public DateTime LastUpdated { get; init; }
}

// IUserFeedbackRepository moved to shared interfaces to avoid duplication
