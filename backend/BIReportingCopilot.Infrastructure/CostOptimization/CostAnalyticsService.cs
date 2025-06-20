using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;

namespace BIReportingCopilot.Infrastructure.CostOptimization;

/// <summary>
/// Comprehensive cost analytics and reporting service with ROI analysis and optimization recommendations
/// </summary>
public class CostAnalyticsService : ICostAnalyticsService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<CostAnalyticsService> _logger;
    private readonly ICostManagementService _costManagementService;
    
    private readonly Dictionary<string, Func<DateTime?, DateTime?, CancellationToken, Task<decimal>>> _roiCalculators = new();
    private readonly Dictionary<string, decimal> _featureValueMetrics = new();

    public CostAnalyticsService(
        BICopilotContext context,
        ILogger<CostAnalyticsService> logger,
        ICostManagementService costManagementService)
    {
        _context = context;
        _logger = logger;
        _costManagementService = costManagementService;
        InitializeROICalculators();
        InitializeFeatureValueMetrics();
    }

    #region Cost Analytics

    public async Task<CostAnalyticsSummary> GenerateCostAnalyticsAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use cost management service for basic analytics
            var summary = await _costManagementService.GetCostAnalyticsAsync(userId, startDate, endDate, cancellationToken);
            
            // Enhance with additional analytics
            summary.CostEfficiency = (double)await CalculateCostEfficiencyAsync(userId, startDate, endDate, cancellationToken);
            summary.PredictedMonthlyCost = await PredictMonthlyCostAsync(userId, cancellationToken);
            summary.CostSavingsOpportunities = await GenerateOptimizationRecommendationsAsync(userId, cancellationToken);
            summary.ROIMetrics = await CalculateROIMetricsAsync(userId, startDate, endDate, cancellationToken);

            _logger.LogInformation("Generated cost analytics summary for user {UserId}: Total cost {TotalCost:C}", 
                userId ?? "all", summary.TotalCost);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cost analytics");
            return new CostAnalyticsSummary { GeneratedAt = DateTime.UtcNow };
        }
    }

    public async Task<Dictionary<string, decimal>> GetCostBreakdownByDimensionAsync(string dimension, string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _costManagementService.GetCostBreakdownAsync(dimension, userId, startDate, endDate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost breakdown by dimension {Dimension}", dimension);
            return new Dictionary<string, decimal>();
        }
    }

    public async Task<List<CostTrend>> GetCostTrendsAsync(string? userId = null, string? category = null, string granularity = "daily", int periods = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var trends = await _costManagementService.GetCostTrendsAsync(userId, category, periods, cancellationToken);
            
            // Enhance trends with additional analytics
            foreach (var trend in trends)
            {
                trend.Metadata["growth_rate"] = CalculateGrowthRate(trends, trend);
                trend.Metadata["variance"] = CalculateVariance(trends, trend);
                trend.Metadata["forecast"] = await ForecastNextPeriodAsync(trends, trend, cancellationToken);
            }

            return trends;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost trends");
            return new List<CostTrend>();
        }
    }

    #endregion

    #region ROI Analysis

    public async Task<Dictionary<string, object>> CalculateROIAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var roi = new Dictionary<string, object>();
            var totalCost = await _costManagementService.GetTotalCostAsync(userId ?? "", startDate, endDate, cancellationToken);
            
            // Calculate ROI for different features
            foreach (var calculator in _roiCalculators)
            {
                try
                {
                    var featureValue = await calculator.Value(startDate, endDate, cancellationToken);
                    var featureCost = totalCost * GetFeatureCostRatio(calculator.Key);
                    var roiValue = featureCost > 0 ? (featureValue - featureCost) / featureCost : 0;
                    
                    roi[calculator.Key] = new
                    {
                        value = featureValue,
                        cost = featureCost,
                        roi = roiValue,
                        roi_percentage = roiValue * 100
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error calculating ROI for feature {Feature}", calculator.Key);
                    roi[calculator.Key] = new { error = ex.Message };
                }
            }

            // Overall ROI calculation
            var totalValue = roi.Values
                .Where(v => v.GetType().GetProperty("value") != null)
                .Sum(v => Convert.ToDecimal(v.GetType().GetProperty("value")?.GetValue(v) ?? 0));
            
            roi["overall"] = new
            {
                total_cost = totalCost,
                total_value = totalValue,
                net_value = totalValue - totalCost,
                roi = totalCost > 0 ? (totalValue - totalCost) / totalCost : 0,
                roi_percentage = totalCost > 0 ? ((totalValue - totalCost) / totalCost) * 100 : 0
            };

            return roi;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating ROI");
            return new Dictionary<string, object>();
        }
    }

    public async Task<Dictionary<string, decimal>> GetCostSavingsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var savings = new Dictionary<string, decimal>();
            
            // Calculate savings from optimization recommendations
            var recommendations = await _costManagementService.GetCostOptimizationRecommendationsAsync(null, cancellationToken);
            var implementedRecommendations = recommendations.Where(r => r.IsImplemented);
            
            foreach (var recommendation in implementedRecommendations)
            {
                savings[recommendation.Type] = savings.GetValueOrDefault(recommendation.Type, 0) + recommendation.PotentialSavings;
            }

            // Calculate savings from cache optimization
            savings["cache_optimization"] = await CalculateCacheOptimizationSavingsAsync(startDate, endDate, cancellationToken);
            
            // Calculate savings from model selection optimization
            savings["model_optimization"] = await CalculateModelOptimizationSavingsAsync(startDate, endDate, cancellationToken);

            return savings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost savings");
            return new Dictionary<string, decimal>();
        }
    }

    public async Task<List<Dictionary<string, object>>> GetROIByFeatureAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var featureROI = new List<Dictionary<string, object>>();
            
            foreach (var feature in _featureValueMetrics.Keys)
            {
                var roi = await CalculateFeatureROIAsync(feature, cancellationToken);
                featureROI.Add(roi);
            }

            return featureROI.OrderByDescending(f => Convert.ToDouble(f.GetValueOrDefault("roi", 0))).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ROI by feature");
            return new List<Dictionary<string, object>>();
        }
    }

    #endregion

    #region Cost Forecasting

    public async Task<List<CostTrend>> ForecastCostsAsync(string? userId = null, int forecastDays = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get historical data for forecasting
            var historicalTrends = await GetCostTrendsAsync(userId, null, "daily", 90, cancellationToken);
            
            if (!historicalTrends.Any())
                return new List<CostTrend>();

            var forecast = new List<CostTrend>();
            var lastDate = historicalTrends.Max(t => t.Date);
            
            // Simple linear regression for forecasting
            var (slope, intercept) = CalculateLinearRegression(historicalTrends);
            
            for (int i = 1; i <= forecastDays; i++)
            {
                var forecastDate = lastDate.AddDays(i);
                var daysSinceStart = (forecastDate - historicalTrends.Min(t => t.Date)).Days;
                var forecastAmount = Math.Max(0, (decimal)(slope * daysSinceStart + intercept));
                
                forecast.Add(new CostTrend
                {
                    Date = forecastDate,
                    Amount = forecastAmount,
                    Category = "Forecast",
                    Metadata = new Dictionary<string, object>
                    {
                        ["is_forecast"] = true,
                        ["confidence"] = CalculateForecastConfidence(historicalTrends, i),
                        ["method"] = "linear_regression"
                    }
                });
            }

            _logger.LogInformation("Generated {Days} days cost forecast for user {UserId}", forecastDays, userId ?? "all");

            return forecast;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error forecasting costs");
            return new List<CostTrend>();
        }
    }

    public async Task<decimal> PredictMonthlyCostAsync(string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var forecast = await ForecastCostsAsync(userId, 30, cancellationToken);
            return forecast.Sum(f => f.Amount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting monthly cost");
            return 0;
        }
    }

    public async Task<Dictionary<string, decimal>> GetCostForecastByProviderAsync(int forecastDays = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var providerForecasts = new Dictionary<string, decimal>();
            
            // Get historical cost breakdown by provider
            var historicalBreakdown = await GetCostBreakdownByDimensionAsync("provider", null, DateTime.UtcNow.AddDays(-90), null, cancellationToken);
            
            foreach (var provider in historicalBreakdown.Keys)
            {
                var providerForecast = await ForecastCostsAsync(null, forecastDays, cancellationToken);
                var providerRatio = historicalBreakdown[provider] / historicalBreakdown.Values.Sum();
                providerForecasts[provider] = providerForecast.Sum(f => f.Amount) * providerRatio;
            }

            return providerForecasts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost forecast by provider");
            return new Dictionary<string, decimal>();
        }
    }

    #endregion

    #region Cost Optimization Analytics

    public async Task<List<CostOptimizationRecommendation>> GenerateOptimizationRecommendationsAsync(string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var recommendations = new List<CostOptimizationRecommendation>();

            // Get existing recommendations from cost management service
            var existingRecommendations = await _costManagementService.GetCostOptimizationRecommendationsAsync(userId, cancellationToken);
            recommendations.AddRange(existingRecommendations);

            // Generate additional analytics-based recommendations
            var analyticsRecommendations = await GenerateAnalyticsBasedRecommendationsAsync(userId, cancellationToken);
            recommendations.AddRange(analyticsRecommendations);

            // Prioritize recommendations by impact and feasibility
            return recommendations
                .OrderByDescending(r => r.ImpactScore)
                .ThenBy(r => r.Priority == "High" ? 1 : r.Priority == "Medium" ? 2 : 3)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating optimization recommendations");
            return new List<CostOptimizationRecommendation>();
        }
    }

    public async Task<decimal> CalculatePotentialSavingsAsync(string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var recommendations = await GenerateOptimizationRecommendationsAsync(userId, cancellationToken);
            return recommendations.Where(r => !r.IsImplemented).Sum(r => r.PotentialSavings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating potential savings");
            return 0;
        }
    }

    public async Task<Dictionary<string, object>> AnalyzeCostPatternsAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var patterns = new Dictionary<string, object>();
            var trends = await GetCostTrendsAsync(userId, null, "daily", 90, cancellationToken);

            if (!trends.Any())
                return patterns;

            // Analyze spending patterns
            patterns["peak_spending_day"] = GetPeakSpendingDay(trends);
            patterns["spending_volatility"] = CalculateSpendingVolatility(trends);
            patterns["growth_trend"] = CalculateGrowthTrend(trends);
            patterns["seasonal_patterns"] = AnalyzeSeasonalPatterns(trends);
            patterns["anomalies"] = DetectCostAnomalies(trends);
            patterns["efficiency_score"] = await CalculateEfficiencyScoreAsync(userId, cancellationToken);

            // Provider usage patterns
            var providerBreakdown = await GetCostBreakdownByDimensionAsync("provider", userId, startDate, endDate, cancellationToken);
            patterns["provider_concentration"] = CalculateProviderConcentration(providerBreakdown);
            patterns["provider_efficiency"] = await AnalyzeProviderEfficiencyAsync(providerBreakdown, cancellationToken);

            return patterns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing cost patterns");
            return new Dictionary<string, object>();
        }
    }

    #endregion

    #region Budget Analytics

    public async Task<Dictionary<string, object>> GetBudgetUtilizationAsync(string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var utilization = new Dictionary<string, object>();

            // Get budgets from cost management service
            var budgets = await _costManagementService.GetBudgetsAsync(userId ?? "", "User", cancellationToken);

            if (!budgets.Any())
            {
                utilization["message"] = "No budgets configured";
                return utilization;
            }

            var totalBudget = budgets.Sum(b => b.BudgetAmount);
            var totalSpent = budgets.Sum(b => b.SpentAmount);
            var totalRemaining = budgets.Sum(b => b.RemainingAmount);

            utilization["total_budget"] = totalBudget;
            utilization["total_spent"] = totalSpent;
            utilization["total_remaining"] = totalRemaining;
            utilization["utilization_rate"] = totalBudget > 0 ? totalSpent / totalBudget : 0;
            utilization["days_remaining"] = CalculateDaysRemainingInPeriod(budgets);
            utilization["projected_overspend"] = CalculateProjectedOverspend(budgets);
            utilization["budget_health"] = GetBudgetHealthStatus(budgets);

            // Individual budget details
            utilization["budgets"] = budgets.Select(b => new
            {
                name = b.Name,
                budget_amount = b.BudgetAmount,
                spent_amount = b.SpentAmount,
                remaining_amount = b.RemainingAmount,
                utilization_rate = b.BudgetAmount > 0 ? b.SpentAmount / b.BudgetAmount : 0,
                status = GetBudgetStatus(b),
                days_to_reset = (b.EndDate - DateTime.UtcNow).Days
            }).ToList();

            return utilization;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budget utilization");
            return new Dictionary<string, object>();
        }
    }

    public async Task<List<BudgetManagement>> GetBudgetsAtRiskAsync(decimal riskThreshold = 0.9m, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _costManagementService.GetBudgetsNearLimitAsync(riskThreshold, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budgets at risk");
            return new List<BudgetManagement>();
        }
    }

    public async Task<Dictionary<string, decimal>> GetBudgetVarianceAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var variance = new Dictionary<string, decimal>();

            // This would compare actual vs budgeted spending
            // For now, return mock variance data
            variance["total_variance"] = 0;
            variance["positive_variance"] = 0; // Under budget
            variance["negative_variance"] = 0; // Over budget

            return variance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budget variance");
            return new Dictionary<string, decimal>();
        }
    }

    #endregion

    #region Cost Efficiency Metrics

    public async Task<Dictionary<string, double>> GetCostEfficiencyMetricsAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = new Dictionary<string, double>();

            var totalCost = await _costManagementService.GetTotalCostAsync(userId ?? "", startDate, endDate, cancellationToken);
            var costHistory = await _costManagementService.GetCostHistoryAsync(userId ?? "", startDate, endDate, cancellationToken);

            if (!costHistory.Any())
                return metrics;

            // Calculate efficiency metrics
            metrics["cost_per_request"] = costHistory.Count > 0 ? (double)(totalCost / costHistory.Count) : 0;
            metrics["average_tokens_per_request"] = costHistory.Average(c => c.TotalTokens);
            metrics["cost_per_token"] = costHistory.Average(c => (double)c.CostPerToken);
            metrics["efficiency_score"] = await CalculateEfficiencyScoreAsync(userId, cancellationToken);
            metrics["optimization_potential"] = await CalculateOptimizationPotentialAsync(userId, cancellationToken);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost efficiency metrics");
            return new Dictionary<string, double>();
        }
    }

    public async Task<Dictionary<string, decimal>> GetCostPerQueryAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var costHistory = await _costManagementService.GetCostHistoryAsync(userId ?? "", startDate, endDate, cancellationToken);

            return costHistory
                .Where(c => !string.IsNullOrEmpty(c.QueryId))
                .GroupBy(c => c.QueryId!)
                .ToDictionary(g => g.Key, g => g.Sum(c => c.Cost));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost per query");
            return new Dictionary<string, decimal>();
        }
    }

    public async Task<Dictionary<string, decimal>> GetCostPerUserAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _costManagementService.GetCostByProviderAsync("", startDate, endDate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost per user");
            return new Dictionary<string, decimal>();
        }
    }

    #endregion

    #region Comparative Analytics

    public async Task<Dictionary<string, object>> CompareCostsByPeriodAsync(DateTime period1Start, DateTime period1End, DateTime period2Start, DateTime period2End, CancellationToken cancellationToken = default)
    {
        try
        {
            var period1Cost = await _costManagementService.GetTotalCostAsync("", period1Start, period1End, cancellationToken);
            var period2Cost = await _costManagementService.GetTotalCostAsync("", period2Start, period2End, cancellationToken);

            var comparison = new Dictionary<string, object>
            {
                ["period1"] = new { start = period1Start, end = period1End, cost = period1Cost },
                ["period2"] = new { start = period2Start, end = period2End, cost = period2Cost },
                ["difference"] = period2Cost - period1Cost,
                ["percentage_change"] = period1Cost > 0 ? ((period2Cost - period1Cost) / period1Cost) * 100 : 0,
                ["trend"] = period2Cost > period1Cost ? "increasing" : period2Cost < period1Cost ? "decreasing" : "stable"
            };

            return comparison;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing costs by period");
            return new Dictionary<string, object>();
        }
    }

    public async Task<Dictionary<string, decimal>> CompareProviderCostsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetCostBreakdownByDimensionAsync("provider", null, startDate, endDate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing provider costs");
            return new Dictionary<string, decimal>();
        }
    }

    public async Task<Dictionary<string, object>> GetCostBenchmarksAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var benchmarks = new Dictionary<string, object>
            {
                ["industry_average_cost_per_query"] = 0.05m,
                ["industry_average_cost_per_token"] = 0.0001m,
                ["optimal_cache_hit_rate"] = 0.85,
                ["target_efficiency_score"] = 0.8,
                ["recommended_budget_utilization"] = 0.85
            };

            // Compare with actual metrics
            var actualMetrics = await GetCostEfficiencyMetricsAsync(null, null, null, cancellationToken);

            benchmarks["performance_vs_benchmark"] = new
            {
                cost_per_token_ratio = actualMetrics.GetValueOrDefault("cost_per_token", 0) / 0.0001,
                efficiency_score_ratio = actualMetrics.GetValueOrDefault("efficiency_score", 0) / 0.8
            };

            return benchmarks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost benchmarks");
            return new Dictionary<string, object>();
        }
    }

    #endregion

    #region Real-time Analytics

    public async Task<Dictionary<string, object>> GetRealTimeCostMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            var thisHour = DateTime.UtcNow.AddHours(-1);

            var metrics = new Dictionary<string, object>
            {
                ["current_hour_cost"] = await _costManagementService.GetTotalCostAsync("", thisHour, null, cancellationToken),
                ["today_cost"] = await _costManagementService.GetTotalCostAsync("", today, null, cancellationToken),
                ["current_spending_rate"] = await CalculateCurrentSpendingRateAsync(cancellationToken),
                ["active_users"] = await GetActiveUsersCountAsync(cancellationToken),
                ["requests_per_minute"] = await GetRequestsPerMinuteAsync(cancellationToken),
                ["average_cost_per_request"] = await GetAverageCostPerRequestAsync(thisHour, cancellationToken)
            };

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting real-time cost metrics");
            return new Dictionary<string, object>();
        }
    }

    public async Task<List<CostAlert>> GetCostAlertsAsync(string? userId = null, bool activeOnly = true, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _costManagementService.GetActiveAlertsAsync(userId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost alerts");
            return new List<CostAlert>();
        }
    }

    public async Task<Dictionary<string, decimal>> GetCurrentSpendingRateAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var rates = new Dictionary<string, decimal>();
            var lastHour = DateTime.UtcNow.AddHours(-1);

            var hourlySpending = await _costManagementService.GetTotalCostAsync("", lastHour, null, cancellationToken);

            rates["per_hour"] = hourlySpending;
            rates["per_day"] = hourlySpending * 24;
            rates["per_month"] = hourlySpending * 24 * 30;

            return rates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current spending rate");
            return new Dictionary<string, decimal>();
        }
    }

    #endregion

    #region Export and Reporting

    public async Task<byte[]> ExportCostReportAsync(string format, string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await GenerateCostAnalyticsAsync(userId, startDate, endDate, cancellationToken);

            return format.ToLower() switch
            {
                "csv" => GenerateCSVReport(analytics),
                "json" => GenerateJSONReport(analytics),
                "pdf" => await GeneratePDFReportAsync(analytics, cancellationToken),
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting cost report");
            return Array.Empty<byte>();
        }
    }

    public async Task<string> GenerateCostReportUrlAsync(string reportType, Dictionary<string, object> parameters, CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate a temporary URL for the report
            var reportId = Guid.NewGuid().ToString();
            var expirationTime = DateTime.UtcNow.AddHours(24);

            // Store report parameters temporarily (in a real implementation, this would be in cache or database)
            var reportUrl = $"/api/reports/cost/{reportId}?expires={expirationTime:yyyy-MM-ddTHH:mm:ssZ}";

            _logger.LogInformation("Generated cost report URL: {ReportUrl}", reportUrl);

            return reportUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cost report URL");
            return string.Empty;
        }
    }

    public async Task<List<Dictionary<string, object>>> GetScheduledReportsAsync(string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // This would typically query a database of scheduled reports
            // For now, return mock data
            return new List<Dictionary<string, object>>
            {
                new()
                {
                    ["id"] = "weekly-cost-summary",
                    ["name"] = "Weekly Cost Summary",
                    ["schedule"] = "0 0 * * 1", // Every Monday at midnight
                    ["format"] = "pdf",
                    ["recipients"] = new[] { "admin@company.com" },
                    ["last_run"] = DateTime.UtcNow.AddDays(-7),
                    ["next_run"] = DateTime.UtcNow.AddDays(0)
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scheduled reports");
            return new List<Dictionary<string, object>>();
        }
    }

    #endregion

    #region Helper Methods

    private void InitializeROICalculators()
    {
        _roiCalculators["query_automation"] = async (start, end, ct) =>
        {
            // Calculate value from automated query generation
            var queryCount = await GetQueryCountAsync(start, end, ct);
            return queryCount * 50m; // Assume $50 value per automated query
        };

        _roiCalculators["report_generation"] = async (start, end, ct) =>
        {
            // Calculate value from automated report generation
            var reportCount = await GetReportCountAsync(start, end, ct);
            return reportCount * 100m; // Assume $100 value per automated report
        };

        _roiCalculators["data_insights"] = async (start, end, ct) =>
        {
            // Calculate value from data insights provided
            var insightCount = await GetInsightCountAsync(start, end, ct);
            return insightCount * 25m; // Assume $25 value per insight
        };
    }

    private void InitializeFeatureValueMetrics()
    {
        _featureValueMetrics["query_automation"] = 0.4m; // 40% of total cost
        _featureValueMetrics["report_generation"] = 0.3m; // 30% of total cost
        _featureValueMetrics["data_insights"] = 0.2m; // 20% of total cost
        _featureValueMetrics["other"] = 0.1m; // 10% of total cost
    }

    private decimal GetFeatureCostRatio(string feature)
    {
        return _featureValueMetrics.GetValueOrDefault(feature, 0.1m);
    }

    private async Task<decimal> CalculateCostEfficiencyAsync(string? userId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        try
        {
            var costHistory = await _costManagementService.GetCostHistoryAsync(userId ?? "", startDate, endDate, cancellationToken);
            if (!costHistory.Any()) return 0;

            var avgCostPerToken = costHistory.Average(c => c.CostPerToken);
            var avgResponseTime = costHistory.Average(c => c.DurationMs);

            // Efficiency score based on cost per token and response time
            var costEfficiency = Math.Max(0, 1 - (double)(avgCostPerToken / 0.001m)); // Normalize to $0.001 per token
            var timeEfficiency = Math.Max(0, 1 - (avgResponseTime / 10000.0)); // Normalize to 10 seconds

            return (decimal)((costEfficiency + timeEfficiency) / 2);
        }
        catch
        {
            return 0;
        }
    }

    private async Task<decimal> IdentifyCostSavingsOpportunitiesAsync(string? userId, CancellationToken cancellationToken)
    {
        try
        {
            var recommendations = await _costManagementService.GetCostOptimizationRecommendationsAsync(userId, cancellationToken);
            return recommendations.Where(r => !r.IsImplemented).Sum(r => r.PotentialSavings);
        }
        catch
        {
            return 0;
        }
    }

    private async Task<Dictionary<string, object>> CalculateROIMetricsAsync(string? userId, DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken)
    {
        try
        {
            var roi = await CalculateROIAsync(userId, startDate, endDate, cancellationToken);
            return new Dictionary<string, object>
            {
                ["overall_roi"] = roi.GetValueOrDefault("overall", new { roi_percentage = 0 }),
                ["top_performing_feature"] = roi.Where(kvp => kvp.Key != "overall")
                    .OrderByDescending(kvp => GetROIValue(kvp.Value))
                    .FirstOrDefault().Key ?? "none"
            };
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    private static double GetROIValue(object roiData)
    {
        try
        {
            var roiProperty = roiData.GetType().GetProperty("roi");
            return roiProperty != null ? Convert.ToDouble(roiProperty.GetValue(roiData)) : 0;
        }
        catch
        {
            return 0;
        }
    }

    private static double CalculateGrowthRate(List<CostTrend> trends, CostTrend currentTrend)
    {
        var index = trends.IndexOf(currentTrend);
        if (index <= 0) return 0;

        var previousTrend = trends[index - 1];
        return previousTrend.Amount > 0 ? (double)((currentTrend.Amount - previousTrend.Amount) / previousTrend.Amount) : 0;
    }

    private static double CalculateVariance(List<CostTrend> trends, CostTrend currentTrend)
    {
        if (trends.Count < 2) return 0;

        var average = trends.Average(t => t.Amount);
        return (double)Math.Abs(currentTrend.Amount - average);
    }

    private async Task<decimal> ForecastNextPeriodAsync(List<CostTrend> trends, CostTrend currentTrend, CancellationToken cancellationToken)
    {
        try
        {
            var index = trends.IndexOf(currentTrend);
            if (index < trends.Count - 7) return 0; // Only forecast for recent trends

            var recentTrends = trends.Skip(Math.Max(0, index - 6)).Take(7).ToList();
            var avgGrowthRate = recentTrends.Skip(1).Average(t => CalculateGrowthRate(trends, t));

            return currentTrend.Amount * (1 + (decimal)avgGrowthRate);
        }
        catch
        {
            return currentTrend.Amount;
        }
    }

    private static (double slope, double intercept) CalculateLinearRegression(List<CostTrend> trends)
    {
        if (trends.Count < 2) return (0, 0);

        var n = trends.Count;
        var sumX = trends.Select((t, i) => i).Sum();
        var sumY = trends.Sum(t => (double)t.Amount);
        var sumXY = trends.Select((t, i) => i * (double)t.Amount).Sum();
        var sumX2 = trends.Select((t, i) => i * i).Sum();

        var slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        var intercept = (sumY - slope * sumX) / n;

        return (slope, intercept);
    }

    private static double CalculateForecastConfidence(List<CostTrend> trends, int forecastDay)
    {
        // Confidence decreases with distance and variance
        var variance = CalculateVarianceInTrends(trends);
        var baseConfidence = 0.9;
        var distancePenalty = forecastDay * 0.02; // 2% penalty per day
        var variancePenalty = variance * 0.1;

        return Math.Max(0.1, baseConfidence - distancePenalty - variancePenalty);
    }

    private static double CalculateVarianceInTrends(List<CostTrend> trends)
    {
        if (trends.Count < 2) return 0;

        var amounts = trends.Select(t => (double)t.Amount).ToList();
        var mean = amounts.Average();
        var variance = amounts.Sum(a => Math.Pow(a - mean, 2)) / amounts.Count;

        return Math.Sqrt(variance) / mean; // Coefficient of variation
    }

    private async Task<List<CostOptimizationRecommendation>> GenerateAnalyticsBasedRecommendationsAsync(string? userId, CancellationToken cancellationToken)
    {
        var recommendations = new List<CostOptimizationRecommendation>();

        try
        {
            // Analyze cost patterns and generate recommendations
            var patterns = await AnalyzeCostPatternsAsync(userId, null, null, cancellationToken);

            if (patterns.ContainsKey("spending_volatility") && Convert.ToDouble(patterns["spending_volatility"]) > 0.5)
            {
                recommendations.Add(new CostOptimizationRecommendation
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = "SpendingStabilization",
                    Title = "Stabilize Spending Patterns",
                    Description = "High spending volatility detected. Consider implementing budget controls and usage monitoring.",
                    PotentialSavings = 100m,
                    ImpactScore = 0.7,
                    Priority = "Medium",
                    Implementation = "Implement automated budget alerts and usage caps",
                    Benefits = new List<string> { "More predictable costs", "Better budget control" },
                    Risks = new List<string> { "Potential service limitations during peak usage" },
                    CreatedAt = DateTime.UtcNow,
                    IsImplemented = false
                });
            }

            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating analytics-based recommendations");
            return recommendations;
        }
    }

    // Additional helper methods would be implemented here for various calculations
    private async Task<decimal> CalculateCacheOptimizationSavingsAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken) => 50m;
    private async Task<decimal> CalculateModelOptimizationSavingsAsync(DateTime? startDate, DateTime? endDate, CancellationToken cancellationToken) => 75m;
    private async Task<Dictionary<string, object>> CalculateFeatureROIAsync(string feature, CancellationToken cancellationToken) => new();
    private static string GetPeakSpendingDay(List<CostTrend> trends) => trends.OrderByDescending(t => t.Amount).FirstOrDefault()?.Date.DayOfWeek.ToString() ?? "Unknown";
    private static double CalculateSpendingVolatility(List<CostTrend> trends) => CalculateVarianceInTrends(trends);
    private static string CalculateGrowthTrend(List<CostTrend> trends) => trends.Count > 1 && trends.Last().Amount > trends.First().Amount ? "Increasing" : "Stable";
    private static Dictionary<string, object> AnalyzeSeasonalPatterns(List<CostTrend> trends) => new() { ["pattern"] = "No clear seasonal pattern detected" };
    private static List<object> DetectCostAnomalies(List<CostTrend> trends) => new();
    private async Task<double> CalculateEfficiencyScoreAsync(string? userId, CancellationToken cancellationToken) => 0.75;
    private static double CalculateProviderConcentration(Dictionary<string, decimal> breakdown) => breakdown.Values.Any() ? (double)(breakdown.Values.Max() / breakdown.Values.Sum()) : 0;
    private async Task<Dictionary<string, object>> AnalyzeProviderEfficiencyAsync(Dictionary<string, decimal> breakdown, CancellationToken cancellationToken) => new();
    private static int CalculateDaysRemainingInPeriod(List<BudgetManagement> budgets) => budgets.Any() ? (int)(budgets.First().EndDate - DateTime.UtcNow).TotalDays : 0;
    private static decimal CalculateProjectedOverspend(List<BudgetManagement> budgets) => budgets.Sum(b => Math.Max(0, b.SpentAmount - b.BudgetAmount));
    private static string GetBudgetHealthStatus(List<BudgetManagement> budgets) => budgets.All(b => b.SpentAmount <= b.BudgetAmount * 0.8m) ? "Healthy" : "At Risk";
    private static string GetBudgetStatus(BudgetManagement budget) => budget.SpentAmount >= budget.BudgetAmount ? "Exceeded" : budget.SpentAmount >= budget.BudgetAmount * 0.8m ? "Warning" : "Good";
    private async Task<double> CalculateOptimizationPotentialAsync(string? userId, CancellationToken cancellationToken) => 0.25;
    private async Task<decimal> CalculateCurrentSpendingRateAsync(CancellationToken cancellationToken) => 10m;
    private async Task<int> GetActiveUsersCountAsync(CancellationToken cancellationToken) => 50;
    private async Task<double> GetRequestsPerMinuteAsync(CancellationToken cancellationToken) => 25.5;
    private async Task<decimal> GetAverageCostPerRequestAsync(DateTime since, CancellationToken cancellationToken) => 0.05m;
    private async Task<int> GetQueryCountAsync(DateTime? start, DateTime? end, CancellationToken ct) => 1000;
    private async Task<int> GetReportCountAsync(DateTime? start, DateTime? end, CancellationToken ct) => 100;
    private async Task<int> GetInsightCountAsync(DateTime? start, DateTime? end, CancellationToken ct) => 500;

    private static byte[] GenerateCSVReport(CostAnalyticsSummary analytics)
    {
        var csv = new StringBuilder();
        csv.AppendLine("Metric,Value");
        csv.AppendLine($"Total Cost,{analytics.TotalCost}");
        csv.AppendLine($"Daily Cost,{analytics.DailyCost}");
        csv.AppendLine($"Weekly Cost,{analytics.WeeklyCost}");
        csv.AppendLine($"Monthly Cost,{analytics.MonthlyCost}");
        return Encoding.UTF8.GetBytes(csv.ToString());
    }

    private static byte[] GenerateJSONReport(CostAnalyticsSummary analytics)
    {
        var json = JsonSerializer.Serialize(analytics, new JsonSerializerOptions { WriteIndented = true });
        return Encoding.UTF8.GetBytes(json);
    }

    private async Task<byte[]> GeneratePDFReportAsync(CostAnalyticsSummary analytics, CancellationToken cancellationToken)
    {
        // This would generate a PDF report using a PDF library
        // For now, return the JSON as bytes
        return GenerateJSONReport(analytics);
    }

    #endregion
}
