using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Configuration;
using BIReportingCopilot.Infrastructure.Performance;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Intelligent AI provider routing with cost optimization and performance monitoring
/// Implements Enhancement 1: Intelligent Provider Routing with Cost Optimization
/// </summary>
public class IntelligentProviderRouter : IAIService
{
    private readonly ILogger<IntelligentProviderRouter> _logger;
    private readonly ICacheService _cacheService;
    private readonly List<IAIProvider> _providers;
    private readonly ProviderPerformanceTracker _performanceTracker;
    private readonly CostOptimizer _costOptimizer;
    private readonly ProviderRoutingConfiguration _config;
    private readonly ProviderHealthMonitor _healthMonitor;

    public IntelligentProviderRouter(
        ILogger<IntelligentProviderRouter> logger,
        ICacheService cacheService,
        IEnumerable<IAIProvider> providers,
        IOptions<ProviderRoutingConfiguration> config)
    {
        _logger = logger;
        _cacheService = cacheService;
        _providers = providers.ToList();
        _config = config.Value;
        _performanceTracker = new ProviderPerformanceTracker(logger, cacheService);
        _costOptimizer = new CostOptimizer(logger, _performanceTracker);
        _healthMonitor = new ProviderHealthMonitor(logger, _providers);
    }

    /// <summary>
    /// Generate SQL using intelligent provider routing
    /// </summary>
    public async Task<string> GenerateSQLAsync(string prompt)
    {
        try
        {
            _logger.LogDebug("Starting intelligent provider routing for SQL generation");

            // Step 1: Analyze query complexity and requirements
            var queryAnalysis = await AnalyzeQueryComplexityAsync(prompt);

            // Step 2: Get provider recommendations based on analysis
            var providerRecommendations = await GetProviderRecommendationsAsync(queryAnalysis);

            // Step 3: Execute with optimal provider selection
            var result = await ExecuteWithOptimalProviderAsync(prompt, providerRecommendations, queryAnalysis);

            _logger.LogDebug("SQL generation completed using provider: {Provider} with cost: ${Cost:F4}", 
                result.ProviderUsed, result.EstimatedCost);

            return result.GeneratedSQL;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in intelligent provider routing");
            return await FallbackGenerationAsync(prompt);
        }
    }

    /// <summary>
    /// Generate response using intelligent routing
    /// </summary>
    public async Task<string> GenerateResponseAsync(string prompt)
    {
        try
        {
            var queryAnalysis = await AnalyzeQueryComplexityAsync(prompt);
            var providerRecommendations = await GetProviderRecommendationsAsync(queryAnalysis);
            var result = await ExecuteWithOptimalProviderAsync(prompt, providerRecommendations, queryAnalysis);

            return result.GeneratedSQL;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in intelligent provider routing for response generation");
            return await FallbackGenerationAsync(prompt);
        }
    }

    /// <summary>
    /// Get provider performance metrics and cost analysis
    /// </summary>
    public async Task<ProviderPerformanceReport> GetPerformanceReportAsync(TimeSpan? period = null)
    {
        try
        {
            var lookbackPeriod = period ?? TimeSpan.FromDays(7);
            var metrics = await _performanceTracker.GetProviderMetricsAsync(lookbackPeriod);
            var costAnalysis = await _costOptimizer.GetCostAnalysisAsync(lookbackPeriod);

            return new ProviderPerformanceReport
            {
                Period = lookbackPeriod,
                ProviderMetrics = metrics,
                CostAnalysis = costAnalysis,
                TotalRequests = metrics.Sum(m => m.RequestCount),
                TotalCost = costAnalysis.TotalCost,
                AverageResponseTime = metrics.Any() ? metrics.Average(m => m.AverageResponseTime) : 0,
                CostSavings = costAnalysis.EstimatedSavings,
                Recommendations = await GenerateOptimizationRecommendationsAsync(metrics, costAnalysis),
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating provider performance report");
            return new ProviderPerformanceReport
            {
                Period = TimeSpan.FromDays(7),
                GeneratedAt = DateTime.UtcNow,
                Recommendations = new List<string> { "Error generating report" }
            };
        }
    }

    /// <summary>
    /// Optimize provider routing based on historical performance
    /// </summary>
    public async Task OptimizeRoutingAsync()
    {
        try
        {
            _logger.LogInformation("Starting provider routing optimization");

            // Get recent performance data
            var metrics = await _performanceTracker.GetProviderMetricsAsync(TimeSpan.FromDays(7));
            
            // Update provider weights based on performance
            await UpdateProviderWeightsAsync(metrics);

            // Update cost thresholds
            await _costOptimizer.UpdateCostThresholdsAsync(metrics);

            // Update health check intervals
            await _healthMonitor.OptimizeHealthCheckIntervalsAsync(metrics);

            _logger.LogInformation("Provider routing optimization completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing provider routing");
        }
    }

    // Private methods

    private async Task<QueryComplexityAnalysis> AnalyzeQueryComplexityAsync(string prompt)
    {
        var analysis = new QueryComplexityAnalysis
        {
            Prompt = prompt,
            TokenCount = EstimateTokenCount(prompt),
            ComplexityScore = CalculateComplexityScore(prompt),
            RequiresSpecializedModel = RequiresSpecializedModel(prompt),
            EstimatedProcessingTime = EstimateProcessingTime(prompt),
            PriorityLevel = DeterminePriorityLevel(prompt)
        };

        // Add semantic analysis if available
        if (prompt.Contains("complex") || prompt.Contains("join") || prompt.Contains("aggregate"))
        {
            analysis.ComplexityScore += 2;
            analysis.RequiresSpecializedModel = true;
        }

        return analysis;
    }

    private async Task<List<ProviderRecommendation>> GetProviderRecommendationsAsync(QueryComplexityAnalysis analysis)
    {
        var recommendations = new List<ProviderRecommendation>();

        foreach (var provider in _providers)
        {
            var healthStatus = await _healthMonitor.GetProviderHealthAsync(provider.Name);
            var performance = await _performanceTracker.GetProviderPerformanceAsync(provider.Name);
            var cost = await _costOptimizer.EstimateCostAsync(provider.Name, analysis);

            var score = CalculateProviderScore(provider, healthStatus, performance, cost, analysis);

            recommendations.Add(new ProviderRecommendation
            {
                ProviderName = provider.Name,
                Score = score,
                EstimatedCost = cost,
                EstimatedResponseTime = performance?.AverageResponseTime ?? 1000,
                HealthStatus = healthStatus,
                Reasoning = GenerateRecommendationReasoning(provider, score, cost, performance, analysis)
            });
        }

        return recommendations.OrderByDescending(r => r.Score).ToList();
    }

    private async Task<ProviderExecutionResult> ExecuteWithOptimalProviderAsync(
        string prompt,
        List<ProviderRecommendation> recommendations,
        QueryComplexityAnalysis analysis)
    {
        var startTime = DateTime.UtcNow;
        Exception? lastException = null;

        // Try providers in order of recommendation score
        foreach (var recommendation in recommendations.Take(3)) // Try top 3 providers
        {
            var provider = _providers.FirstOrDefault(p => p.Name == recommendation.ProviderName);
            if (provider == null) continue;

            try
            {
                _logger.LogDebug("Attempting SQL generation with provider: {Provider}", provider.Name);

                var result = await ExecuteWithProviderAsync(provider, prompt, analysis);
                
                // Record successful execution
                await _performanceTracker.RecordExecutionAsync(new ProviderExecutionRecord
                {
                    ProviderName = provider.Name,
                    Prompt = prompt,
                    Success = true,
                    ResponseTime = DateTime.UtcNow - startTime,
                    TokensUsed = analysis.TokenCount,
                    Cost = recommendation.EstimatedCost,
                    ComplexityScore = analysis.ComplexityScore,
                    Timestamp = DateTime.UtcNow
                });

                return new ProviderExecutionResult
                {
                    GeneratedSQL = result,
                    ProviderUsed = provider.Name,
                    ExecutionTime = DateTime.UtcNow - startTime,
                    EstimatedCost = recommendation.EstimatedCost,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Provider {Provider} failed, trying next provider", provider.Name);
                lastException = ex;

                // Record failed execution
                await _performanceTracker.RecordExecutionAsync(new ProviderExecutionRecord
                {
                    ProviderName = provider.Name,
                    Prompt = prompt,
                    Success = false,
                    ResponseTime = DateTime.UtcNow - startTime,
                    TokensUsed = analysis.TokenCount,
                    Cost = 0,
                    ComplexityScore = analysis.ComplexityScore,
                    Timestamp = DateTime.UtcNow,
                    ErrorMessage = ex.Message
                });
            }
        }

        // All providers failed
        throw new InvalidOperationException(
            $"All providers failed to generate SQL. Last error: {lastException?.Message}", lastException);
    }

    private async Task<string> ExecuteWithProviderAsync(IAIProvider provider, string prompt, QueryComplexityAnalysis analysis)
    {
        // Add provider-specific optimizations
        var optimizedPrompt = await OptimizePromptForProviderAsync(provider, prompt, analysis);
        
        // Execute with timeout based on complexity
        var timeout = TimeSpan.FromSeconds(Math.Min(60, 10 + analysis.ComplexityScore * 2));
        
        using var cts = new CancellationTokenSource(timeout);
        return await provider.GenerateAsync(optimizedPrompt, cts.Token);
    }

    private async Task<string> OptimizePromptForProviderAsync(IAIProvider provider, string prompt, QueryComplexityAnalysis analysis)
    {
        // Provider-specific prompt optimizations
        return provider.Name.ToLowerInvariant() switch
        {
            "openai" => await OptimizeForOpenAIAsync(prompt, analysis),
            "azure" => await OptimizeForAzureAsync(prompt, analysis),
            "anthropic" => await OptimizeForAnthropicAsync(prompt, analysis),
            _ => prompt
        };
    }

    private async Task<string> OptimizeForOpenAIAsync(string prompt, QueryComplexityAnalysis analysis)
    {
        if (analysis.ComplexityScore > 7)
        {
            return $"You are an expert SQL developer. Please generate optimized SQL for this complex query:\n\n{prompt}\n\nFocus on performance and use appropriate indexes.";
        }
        return prompt;
    }

    private async Task<string> OptimizeForAzureAsync(string prompt, QueryComplexityAnalysis analysis)
    {
        return $"Generate SQL Server compatible SQL for:\n{prompt}";
    }

    private async Task<string> OptimizeForAnthropicAsync(string prompt, QueryComplexityAnalysis analysis)
    {
        return $"Please provide a well-structured SQL query for:\n{prompt}\n\nEnsure the query is optimized and follows best practices.";
    }

    private double CalculateProviderScore(
        IAIProvider provider,
        ProviderHealthStatus healthStatus,
        ProviderPerformanceMetrics? performance,
        double estimatedCost,
        QueryComplexityAnalysis analysis)
    {
        var score = 0.0;

        // Health score (30% weight)
        score += (healthStatus.IsHealthy ? 1.0 : 0.0) * 0.3;
        score += (1.0 - healthStatus.ErrorRate) * 0.2;

        // Performance score (40% weight)
        if (performance != null)
        {
            var responseTimeScore = Math.Max(0, 1.0 - (performance.AverageResponseTime / 10000.0)); // Normalize to 10s max
            score += responseTimeScore * 0.25;
            score += performance.SuccessRate * 0.15;
        }

        // Cost score (30% weight) - lower cost = higher score
        var maxCost = 0.10; // $0.10 max expected cost
        var costScore = Math.Max(0, 1.0 - (estimatedCost / maxCost));
        score += costScore * 0.3;

        // Complexity bonus - some providers better for complex queries
        if (analysis.ComplexityScore > 7 && provider.Name.Contains("gpt-4"))
        {
            score += 0.1; // Bonus for complex queries
        }

        return Math.Max(0, Math.Min(1.0, score));
    }

    private string GenerateRecommendationReasoning(
        IAIProvider provider,
        double score,
        double cost,
        ProviderPerformanceMetrics? performance,
        QueryComplexityAnalysis analysis)
    {
        var reasons = new List<string>();

        if (score > 0.8) reasons.Add("High overall score");
        if (cost < 0.01) reasons.Add("Low cost");
        if (performance?.AverageResponseTime < 2000) reasons.Add("Fast response time");
        if (performance?.SuccessRate > 0.95) reasons.Add("High reliability");
        if (analysis.ComplexityScore > 7 && provider.Name.Contains("gpt-4")) reasons.Add("Good for complex queries");

        return reasons.Any() ? string.Join(", ", reasons) : "Standard recommendation";
    }

    private async Task UpdateProviderWeightsAsync(List<ProviderPerformanceMetrics> metrics)
    {
        foreach (var metric in metrics)
        {
            var weight = CalculateProviderWeight(metric);
            await _cacheService.SetAsync($"provider_weight:{metric.ProviderName}", weight, TimeSpan.FromHours(24));
            _logger.LogDebug("Updated weight for provider {Provider}: {Weight:F2}", metric.ProviderName, weight);
        }
    }

    private double CalculateProviderWeight(ProviderPerformanceMetrics metric)
    {
        // Calculate weight based on success rate, response time, and cost efficiency
        var successWeight = metric.SuccessRate * 0.4;
        var speedWeight = Math.Max(0, 1.0 - (metric.AverageResponseTime / 10000.0)) * 0.3;
        var costWeight = Math.Max(0, 1.0 - (metric.AverageCost / 0.10)) * 0.3;

        return successWeight + speedWeight + costWeight;
    }

    private async Task<List<string>> GenerateOptimizationRecommendationsAsync(
        List<ProviderPerformanceMetrics> metrics,
        CostAnalysisResult costAnalysis)
    {
        var recommendations = new List<string>();

        // Performance recommendations
        var slowProviders = metrics.Where(m => m.AverageResponseTime > 5000).ToList();
        if (slowProviders.Any())
        {
            recommendations.Add($"Consider reducing usage of slow providers: {string.Join(", ", slowProviders.Select(p => p.ProviderName))}");
        }

        // Cost recommendations
        if (costAnalysis.TotalCost > costAnalysis.BudgetThreshold)
        {
            recommendations.Add("Current costs exceed budget threshold. Consider using more cost-effective providers for simple queries.");
        }

        // Reliability recommendations
        var unreliableProviders = metrics.Where(m => m.SuccessRate < 0.95).ToList();
        if (unreliableProviders.Any())
        {
            recommendations.Add($"Monitor reliability of providers: {string.Join(", ", unreliableProviders.Select(p => p.ProviderName))}");
        }

        // Usage optimization
        if (costAnalysis.EstimatedSavings > 0)
        {
            recommendations.Add($"Potential savings of ${costAnalysis.EstimatedSavings:F2} available through better provider selection");
        }

        return recommendations;
    }

    private async Task<string> FallbackGenerationAsync(string prompt)
    {
        _logger.LogWarning("Using fallback SQL generation");
        return $"-- Unable to generate SQL for: {prompt}\n-- Please try rephrasing your query";
    }

    // Helper methods for analysis
    private int EstimateTokenCount(string prompt)
    {
        // Simple token estimation (roughly 4 characters per token)
        return prompt.Length / 4;
    }

    private int CalculateComplexityScore(string prompt)
    {
        var score = 1;
        var lowerPrompt = prompt.ToLowerInvariant();

        if (lowerPrompt.Contains("join")) score += 2;
        if (lowerPrompt.Contains("subquery") || lowerPrompt.Contains("nested")) score += 3;
        if (lowerPrompt.Contains("aggregate") || lowerPrompt.Contains("group by")) score += 2;
        if (lowerPrompt.Contains("window function") || lowerPrompt.Contains("over(")) score += 3;
        if (lowerPrompt.Contains("recursive") || lowerPrompt.Contains("cte")) score += 4;
        if (prompt.Length > 500) score += 2;

        return score;
    }

    private bool RequiresSpecializedModel(string prompt)
    {
        var specializedKeywords = new[] { "complex", "advanced", "optimization", "performance", "recursive", "window function" };
        return specializedKeywords.Any(keyword => prompt.ToLowerInvariant().Contains(keyword));
    }

    private TimeSpan EstimateProcessingTime(string prompt)
    {
        var baseTime = TimeSpan.FromSeconds(2);
        var complexityMultiplier = CalculateComplexityScore(prompt) * 0.5;
        return TimeSpan.FromSeconds(baseTime.TotalSeconds * (1 + complexityMultiplier));
    }

    private QueryPriority DeterminePriorityLevel(string prompt)
    {
        if (prompt.ToLowerInvariant().Contains("urgent") || prompt.ToLowerInvariant().Contains("asap"))
            return QueryPriority.High;
        
        if (CalculateComplexityScore(prompt) > 7)
            return QueryPriority.Medium;
        
        return QueryPriority.Normal;
    }
}
