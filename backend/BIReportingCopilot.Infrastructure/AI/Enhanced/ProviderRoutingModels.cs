using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.Performance;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// AI Provider interface for intelligent routing
/// </summary>
public interface IAIProvider
{
    string Name { get; }
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
    Task<bool> IsHealthyAsync();
    Task<ProviderCapabilities> GetCapabilitiesAsync();
}

/// <summary>
/// Provider capabilities definition
/// </summary>
public class ProviderCapabilities
{
    public int MaxTokens { get; set; }
    public bool SupportsStreaming { get; set; }
    public bool SupportsComplexQueries { get; set; }
    public List<string> SupportedLanguages { get; set; } = new();
    public double CostPerToken { get; set; }
    public TimeSpan TypicalResponseTime { get; set; }
}

/// <summary>
/// Query complexity analysis result
/// </summary>
public class QueryComplexityAnalysis
{
    public string Prompt { get; set; } = string.Empty;
    public int TokenCount { get; set; }
    public int ComplexityScore { get; set; }
    public bool RequiresSpecializedModel { get; set; }
    public TimeSpan EstimatedProcessingTime { get; set; }
    public QueryPriority PriorityLevel { get; set; }
}

/// <summary>
/// Provider recommendation with scoring
/// </summary>
public class ProviderRecommendation
{
    public string ProviderName { get; set; } = string.Empty;
    public double Score { get; set; }
    public double EstimatedCost { get; set; }
    public double EstimatedResponseTime { get; set; }
    public ProviderHealthStatus HealthStatus { get; set; } = new();
    public string Reasoning { get; set; } = string.Empty;
}

/// <summary>
/// Provider execution result
/// </summary>
public class ProviderExecutionResult
{
    public string GeneratedSQL { get; set; } = string.Empty;
    public string ProviderUsed { get; set; } = string.Empty;
    public TimeSpan ExecutionTime { get; set; }
    public double EstimatedCost { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Provider performance metrics
/// </summary>
public class ProviderPerformanceMetrics
{
    public string ProviderName { get; set; } = string.Empty;
    public int RequestCount { get; set; }
    public double SuccessRate { get; set; }
    public double AverageResponseTime { get; set; }
    public double AverageCost { get; set; }
    public double ErrorRate { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Provider health status
/// </summary>
public class ProviderHealthStatus
{
    public bool IsHealthy { get; set; }
    public double ErrorRate { get; set; }
    public TimeSpan LastResponseTime { get; set; }
    public DateTime LastHealthCheck { get; set; }
    public string? LastError { get; set; }
}

/// <summary>
/// Provider execution record for tracking
/// </summary>
public class ProviderExecutionRecord
{
    public string ProviderName { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public bool Success { get; set; }
    public TimeSpan ResponseTime { get; set; }
    public int TokensUsed { get; set; }
    public double Cost { get; set; }
    public int ComplexityScore { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Provider performance report
/// </summary>
public class ProviderPerformanceReport
{
    public TimeSpan Period { get; set; }
    public List<ProviderPerformanceMetrics> ProviderMetrics { get; set; } = new();
    public CostAnalysisResult CostAnalysis { get; set; } = new();
    public int TotalRequests { get; set; }
    public double TotalCost { get; set; }
    public double AverageResponseTime { get; set; }
    public double CostSavings { get; set; }
    public List<string> Recommendations { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Cost analysis result
/// </summary>
public class CostAnalysisResult
{
    public double TotalCost { get; set; }
    public double BudgetThreshold { get; set; }
    public double EstimatedSavings { get; set; }
    public Dictionary<string, double> CostByProvider { get; set; } = new();
    public Dictionary<string, double> CostByComplexity { get; set; } = new();
    public List<CostOptimizationOpportunity> Opportunities { get; set; } = new();
}

/// <summary>
/// Cost optimization opportunity
/// </summary>
public class CostOptimizationOpportunity
{
    public string Description { get; set; } = string.Empty;
    public double PotentialSavings { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
    public double ImplementationEffort { get; set; }
}

/// <summary>
/// Query priority levels
/// </summary>
public enum QueryPriority
{
    Low,
    Normal,
    Medium,
    High,
    Critical
}

/// <summary>
/// Provider routing configuration
/// </summary>
public class ProviderRoutingConfiguration
{
    public bool EnableIntelligentRouting { get; set; } = true;
    public bool EnableCostOptimization { get; set; } = true;
    public bool EnablePerformanceTracking { get; set; } = true;
    public double CostThreshold { get; set; } = 0.10; // $0.10 per request
    public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxRetryAttempts { get; set; } = 3;
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public Dictionary<string, double> ProviderWeights { get; set; } = new();
}

/// <summary>
/// Provider performance tracker
/// </summary>
public class ProviderPerformanceTracker
{
    private readonly ILogger _logger;
    private readonly ICacheService _cacheService;

    public ProviderPerformanceTracker(ILogger logger, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task RecordExecutionAsync(ProviderExecutionRecord record)
    {
        try
        {
            var key = $"provider_execution:{DateTime.UtcNow:yyyyMMddHHmmss}:{Guid.NewGuid():N}";
            await _cacheService.SetAsync(key, record, TimeSpan.FromDays(30));
            
            // Update aggregated metrics
            await UpdateAggregatedMetricsAsync(record);
            
            _logger.LogDebug("Recorded execution for provider {Provider}: Success={Success}, Time={Time}ms", 
                record.ProviderName, record.Success, record.ResponseTime.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording provider execution");
        }
    }

    public async Task<List<ProviderPerformanceMetrics>> GetProviderMetricsAsync(TimeSpan period)
    {
        try
        {
            // In a real implementation, this would query stored execution records
            // For now, return sample metrics
            return new List<ProviderPerformanceMetrics>
            {
                new ProviderPerformanceMetrics
                {
                    ProviderName = "OpenAI-GPT4",
                    RequestCount = 150,
                    SuccessRate = 0.96,
                    AverageResponseTime = 2500,
                    AverageCost = 0.025,
                    ErrorRate = 0.04,
                    LastUpdated = DateTime.UtcNow
                },
                new ProviderPerformanceMetrics
                {
                    ProviderName = "Azure-OpenAI",
                    RequestCount = 200,
                    SuccessRate = 0.94,
                    AverageResponseTime = 3200,
                    AverageCost = 0.020,
                    ErrorRate = 0.06,
                    LastUpdated = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider metrics");
            return new List<ProviderPerformanceMetrics>();
        }
    }

    public async Task<ProviderPerformanceMetrics?> GetProviderPerformanceAsync(string providerName)
    {
        var allMetrics = await GetProviderMetricsAsync(TimeSpan.FromDays(7));
        return allMetrics.FirstOrDefault(m => m.ProviderName == providerName);
    }

    private async Task UpdateAggregatedMetricsAsync(ProviderExecutionRecord record)
    {
        try
        {
            var key = $"provider_metrics:{record.ProviderName}";
            var existingMetrics = await _cacheService.GetAsync<ProviderPerformanceMetrics>(key);
            
            if (existingMetrics == null)
            {
                existingMetrics = new ProviderPerformanceMetrics
                {
                    ProviderName = record.ProviderName,
                    RequestCount = 0,
                    SuccessRate = 0,
                    AverageResponseTime = 0,
                    AverageCost = 0,
                    ErrorRate = 0
                };
            }

            // Update metrics with new record
            existingMetrics.RequestCount++;
            existingMetrics.SuccessRate = CalculateSuccessRate(existingMetrics, record.Success);
            existingMetrics.AverageResponseTime = CalculateAverageResponseTime(existingMetrics, record.ResponseTime);
            existingMetrics.AverageCost = CalculateAverageCost(existingMetrics, record.Cost);
            existingMetrics.ErrorRate = 1.0 - existingMetrics.SuccessRate;
            existingMetrics.LastUpdated = DateTime.UtcNow;

            await _cacheService.SetAsync(key, existingMetrics, TimeSpan.FromDays(7));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating aggregated metrics");
        }
    }

    private double CalculateSuccessRate(ProviderPerformanceMetrics metrics, bool success)
    {
        var totalSuccesses = metrics.SuccessRate * (metrics.RequestCount - 1) + (success ? 1 : 0);
        return totalSuccesses / metrics.RequestCount;
    }

    private double CalculateAverageResponseTime(ProviderPerformanceMetrics metrics, TimeSpan responseTime)
    {
        var totalTime = metrics.AverageResponseTime * (metrics.RequestCount - 1) + responseTime.TotalMilliseconds;
        return totalTime / metrics.RequestCount;
    }

    private double CalculateAverageCost(ProviderPerformanceMetrics metrics, double cost)
    {
        var totalCost = metrics.AverageCost * (metrics.RequestCount - 1) + cost;
        return totalCost / metrics.RequestCount;
    }
}

/// <summary>
/// Cost optimizer for provider selection
/// </summary>
public class CostOptimizer
{
    private readonly ILogger _logger;
    private readonly ProviderPerformanceTracker _performanceTracker;

    public CostOptimizer(ILogger logger, ProviderPerformanceTracker performanceTracker)
    {
        _logger = logger;
        _performanceTracker = performanceTracker;
    }

    public async Task<double> EstimateCostAsync(string providerName, QueryComplexityAnalysis analysis)
    {
        try
        {
            // Provider-specific cost models
            var baseCost = providerName.ToLowerInvariant() switch
            {
                "openai-gpt4" => 0.03 * (analysis.TokenCount / 1000.0), // $0.03 per 1K tokens
                "openai-gpt3.5" => 0.002 * (analysis.TokenCount / 1000.0), // $0.002 per 1K tokens
                "azure-openai" => 0.025 * (analysis.TokenCount / 1000.0), // $0.025 per 1K tokens
                "anthropic-claude" => 0.008 * (analysis.TokenCount / 1000.0), // $0.008 per 1K tokens
                _ => 0.01 * (analysis.TokenCount / 1000.0) // Default cost
            };

            // Complexity multiplier
            var complexityMultiplier = 1.0 + (analysis.ComplexityScore * 0.1);
            
            return baseCost * complexityMultiplier;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating cost for provider {Provider}", providerName);
            return 0.01; // Default cost
        }
    }

    public async Task<CostAnalysisResult> GetCostAnalysisAsync(TimeSpan period)
    {
        try
        {
            var metrics = await _performanceTracker.GetProviderMetricsAsync(period);
            
            var totalCost = metrics.Sum(m => m.AverageCost * m.RequestCount);
            var budgetThreshold = 100.0; // $100 budget
            
            var costByProvider = metrics.ToDictionary(
                m => m.ProviderName, 
                m => m.AverageCost * m.RequestCount);

            var opportunities = await IdentifyCostOptimizationOpportunitiesAsync(metrics);
            var estimatedSavings = opportunities.Sum(o => o.PotentialSavings);

            return new CostAnalysisResult
            {
                TotalCost = totalCost,
                BudgetThreshold = budgetThreshold,
                EstimatedSavings = estimatedSavings,
                CostByProvider = costByProvider,
                Opportunities = opportunities
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing cost analysis");
            return new CostAnalysisResult();
        }
    }

    public async Task UpdateCostThresholdsAsync(List<ProviderPerformanceMetrics> metrics)
    {
        try
        {
            // Update cost thresholds based on recent performance
            foreach (var metric in metrics)
            {
                var threshold = CalculateOptimalCostThreshold(metric);
                var key = $"cost_threshold:{metric.ProviderName}";
                await _performanceTracker._cacheService.SetAsync(key, threshold, TimeSpan.FromDays(1));
                
                _logger.LogDebug("Updated cost threshold for {Provider}: ${Threshold:F4}", 
                    metric.ProviderName, threshold);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cost thresholds");
        }
    }

    private async Task<List<CostOptimizationOpportunity>> IdentifyCostOptimizationOpportunitiesAsync(
        List<ProviderPerformanceMetrics> metrics)
    {
        var opportunities = new List<CostOptimizationOpportunity>();

        // Find expensive providers with alternatives
        var expensiveProviders = metrics.Where(m => m.AverageCost > 0.02).ToList();
        var cheapProviders = metrics.Where(m => m.AverageCost <= 0.02 && m.SuccessRate > 0.9).ToList();

        if (expensiveProviders.Any() && cheapProviders.Any())
        {
            var potentialSavings = expensiveProviders.Sum(ep => 
                (ep.AverageCost - cheapProviders.Min(cp => cp.AverageCost)) * ep.RequestCount);

            opportunities.Add(new CostOptimizationOpportunity
            {
                Description = "Route simple queries to lower-cost providers",
                PotentialSavings = potentialSavings,
                RecommendedAction = $"Use {cheapProviders.First().ProviderName} for simple queries",
                ImplementationEffort = 0.3
            });
        }

        return opportunities;
    }

    private double CalculateOptimalCostThreshold(ProviderPerformanceMetrics metric)
    {
        // Calculate threshold based on performance and cost balance
        var performanceScore = metric.SuccessRate * (1.0 - metric.ErrorRate);
        var costEfficiency = 1.0 / (metric.AverageCost + 0.001); // Avoid division by zero
        
        return metric.AverageCost * (1.0 + (performanceScore * costEfficiency * 0.1));
    }
}

/// <summary>
/// Provider health monitor
/// </summary>
public class ProviderHealthMonitor
{
    private readonly ILogger _logger;
    private readonly List<IAIProvider> _providers;

    public ProviderHealthMonitor(ILogger logger, List<IAIProvider> providers)
    {
        _logger = logger;
        _providers = providers;
    }

    public async Task<ProviderHealthStatus> GetProviderHealthAsync(string providerName)
    {
        try
        {
            var provider = _providers.FirstOrDefault(p => p.Name == providerName);
            if (provider == null)
            {
                return new ProviderHealthStatus
                {
                    IsHealthy = false,
                    ErrorRate = 1.0,
                    LastHealthCheck = DateTime.UtcNow,
                    LastError = "Provider not found"
                };
            }

            var isHealthy = await provider.IsHealthyAsync();
            
            return new ProviderHealthStatus
            {
                IsHealthy = isHealthy,
                ErrorRate = isHealthy ? 0.0 : 1.0,
                LastHealthCheck = DateTime.UtcNow,
                LastResponseTime = TimeSpan.FromMilliseconds(100) // Placeholder
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking health for provider {Provider}", providerName);
            return new ProviderHealthStatus
            {
                IsHealthy = false,
                ErrorRate = 1.0,
                LastHealthCheck = DateTime.UtcNow,
                LastError = ex.Message
            };
        }
    }

    public async Task OptimizeHealthCheckIntervalsAsync(List<ProviderPerformanceMetrics> metrics)
    {
        try
        {
            foreach (var metric in metrics)
            {
                // Adjust health check frequency based on reliability
                var interval = metric.SuccessRate > 0.95 
                    ? TimeSpan.FromMinutes(10) // Less frequent for reliable providers
                    : TimeSpan.FromMinutes(2);  // More frequent for unreliable providers

                _logger.LogDebug("Optimized health check interval for {Provider}: {Interval}", 
                    metric.ProviderName, interval);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing health check intervals");
        }
    }
}
