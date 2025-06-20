namespace BIReportingCopilot.Core.Interfaces.CostOptimization;

using BIReportingCopilot.Core.Models;

/// <summary>
/// Cost analytics and reporting service interface
/// </summary>
public interface ICostAnalyticsService
{
    // Cost Analytics
    Task<CostAnalyticsSummary> GenerateCostAnalyticsAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetCostBreakdownByDimensionAsync(string dimension, string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<List<CostTrend>> GetCostTrendsAsync(string? userId = null, string? category = null, string granularity = "daily", int periods = 30, CancellationToken cancellationToken = default);

    // ROI Analysis
    Task<Dictionary<string, object>> CalculateROIAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetCostSavingsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<List<Dictionary<string, object>>> GetROIByFeatureAsync(CancellationToken cancellationToken = default);

    // Cost Forecasting
    Task<List<CostTrend>> ForecastCostsAsync(string? userId = null, int forecastDays = 30, CancellationToken cancellationToken = default);
    Task<decimal> PredictMonthlyCostAsync(string? userId = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetCostForecastByProviderAsync(int forecastDays = 30, CancellationToken cancellationToken = default);

    // Cost Optimization Analytics
    Task<List<CostOptimizationRecommendation>> GenerateOptimizationRecommendationsAsync(string? userId = null, CancellationToken cancellationToken = default);
    Task<decimal> CalculatePotentialSavingsAsync(string? userId = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> AnalyzeCostPatternsAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    // Budget Analytics
    Task<Dictionary<string, object>> GetBudgetUtilizationAsync(string? userId = null, CancellationToken cancellationToken = default);
    Task<List<BudgetManagement>> GetBudgetsAtRiskAsync(decimal riskThreshold = 0.9m, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetBudgetVarianceAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    // Cost Efficiency Metrics
    Task<Dictionary<string, double>> GetCostEfficiencyMetricsAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetCostPerQueryAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetCostPerUserAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    // Comparative Analytics
    Task<Dictionary<string, object>> CompareCostsByPeriodAsync(DateTime period1Start, DateTime period1End, DateTime period2Start, DateTime period2End, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> CompareProviderCostsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetCostBenchmarksAsync(CancellationToken cancellationToken = default);

    // Real-time Analytics
    Task<Dictionary<string, object>> GetRealTimeCostMetricsAsync(CancellationToken cancellationToken = default);
    Task<List<CostAlert>> GetCostAlertsAsync(string? userId = null, bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetCurrentSpendingRateAsync(CancellationToken cancellationToken = default);

    // Export and Reporting
    Task<byte[]> ExportCostReportAsync(string format, string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<string> GenerateCostReportUrlAsync(string reportType, Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
    Task<List<Dictionary<string, object>>> GetScheduledReportsAsync(string? userId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Advanced caching optimization service interface
/// </summary>
public interface ICacheOptimizationService
{
    // Cache Performance Analytics
    Task<CacheStatistics> GetCacheStatisticsAsync(string cacheType, CancellationToken cancellationToken = default);
    Task<Dictionary<string, CacheStatistics>> GetAllCacheStatisticsAsync(CancellationToken cancellationToken = default);
    Task<List<CacheStatisticsEntry>> GetCacheOperationHistoryAsync(string cacheType, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    // Cache Configuration Management
    Task<CacheConfiguration> CreateCacheConfigurationAsync(CacheConfiguration config, CancellationToken cancellationToken = default);
    Task<CacheConfiguration> UpdateCacheConfigurationAsync(CacheConfiguration config, CancellationToken cancellationToken = default);
    Task<CacheConfiguration?> GetCacheConfigurationAsync(string cacheType, CancellationToken cancellationToken = default);
    Task<List<CacheConfiguration>> GetAllCacheConfigurationsAsync(CancellationToken cancellationToken = default);
    Task<bool> DeleteCacheConfigurationAsync(string configId, CancellationToken cancellationToken = default);

    // Intelligent Cache Invalidation
    Task InvalidateCacheByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    Task InvalidateCacheByTagsAsync(List<string> tags, CancellationToken cancellationToken = default);
    Task<List<string>> GetCacheKeysForInvalidationAsync(string entityType, string entityId, CancellationToken cancellationToken = default);
    Task ScheduleCacheInvalidationAsync(string cacheKey, DateTime invalidationTime, CancellationToken cancellationToken = default);

    // Cache Warming
    Task<CacheWarmingConfig> CreateCacheWarmingConfigAsync(CacheWarmingConfig config, CancellationToken cancellationToken = default);
    Task<CacheWarmingConfig> UpdateCacheWarmingConfigAsync(CacheWarmingConfig config, CancellationToken cancellationToken = default);
    Task<List<CacheWarmingConfig>> GetCacheWarmingConfigsAsync(string? cacheType = null, CancellationToken cancellationToken = default);
    Task<bool> ExecuteCacheWarmupAsync(string configId, CancellationToken cancellationToken = default);
    Task<bool> ScheduleCacheWarmupAsync(string configId, DateTime warmupTime, CancellationToken cancellationToken = default);

    // Cache Optimization Recommendations
    Task<List<CacheOptimizationRecommendation>> GetCacheOptimizationRecommendationsAsync(string? cacheType = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> AnalyzeCacheUsagePatternsAsync(string cacheType, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<List<string>> GetUnusedCacheKeysAsync(string cacheType, TimeSpan unusedThreshold, CancellationToken cancellationToken = default);

    // Multi-Layer Cache Management
    Task<bool> PromoteCacheEntryAsync(string cacheKey, string fromLayer, string toLayer, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetCacheLayerDistributionAsync(CancellationToken cancellationToken = default);
    Task OptimizeCacheLayersAsync(CancellationToken cancellationToken = default);

    // Cache Performance Monitoring
    Task RecordCacheOperationAsync(CacheStatisticsEntry operation, CancellationToken cancellationToken = default);
    Task<double> GetCacheHitRateAsync(string cacheType, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<TimeSpan> GetAverageCacheRetrievalTimeAsync(string cacheType, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<long> GetCacheSizeAsync(string cacheType, CancellationToken cancellationToken = default);

    // Cache Health Monitoring
    Task<Dictionary<string, object>> GetCacheHealthStatusAsync(CancellationToken cancellationToken = default);
    Task<List<ResourceMonitoringAlert>> GetCacheAlertsAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<bool> CheckCacheHealthAsync(string cacheType, CancellationToken cancellationToken = default);
}

/// <summary>
/// Real-time resource monitoring service interface
/// </summary>
public interface IResourceMonitoringService
{
    // Real-time Monitoring
    Task<Dictionary<string, object>> GetRealTimeResourceMetricsAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, double>> GetSystemResourceUsageAsync(CancellationToken cancellationToken = default);
    Task<List<ResourceMonitoringAlert>> GetActiveAlertsAsync(CancellationToken cancellationToken = default);

    // Resource Tracking
    Task RecordResourceUsageAsync(ResourceUsageEntry usage, CancellationToken cancellationToken = default);
    Task<List<ResourceUsageEntry>> GetResourceUsageHistoryAsync(string resourceType, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetResourceUsageStatsAsync(string resourceType, CancellationToken cancellationToken = default);

    // Performance Monitoring
    Task RecordPerformanceMetricAsync(PerformanceMetricsEntry metric, CancellationToken cancellationToken = default);
    Task<List<PerformanceMetricsEntry>> GetPerformanceMetricsAsync(string metricName, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, double>> GetAggregatedPerformanceMetricsAsync(string metricName, string aggregationType, TimeSpan period, CancellationToken cancellationToken = default);

    // Alert Management
    Task<ResourceMonitoringAlert> CreateAlertAsync(ResourceMonitoringAlert alert, CancellationToken cancellationToken = default);
    Task<ResourceMonitoringAlert> UpdateAlertAsync(ResourceMonitoringAlert alert, CancellationToken cancellationToken = default);
    Task<bool> ResolveAlertAsync(string alertId, string resolutionNotes, CancellationToken cancellationToken = default);
    Task<List<ResourceMonitoringAlert>> GetAlertHistoryAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    // Threshold Management
    Task SetResourceThresholdAsync(string resourceType, string metricName, double threshold, string severity, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetResourceThresholdsAsync(string resourceType, CancellationToken cancellationToken = default);
    Task<bool> CheckThresholdViolationAsync(string resourceType, string metricName, double currentValue, CancellationToken cancellationToken = default);

    // Health Checks
    Task<Dictionary<string, object>> GetSystemHealthStatusAsync(CancellationToken cancellationToken = default);
    Task<bool> IsResourceHealthyAsync(string resourceType, CancellationToken cancellationToken = default);
    Task<List<Dictionary<string, object>>> GetHealthCheckHistoryAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    // Capacity Planning
    Task<Dictionary<string, object>> GetCapacityForecastAsync(string resourceType, int forecastDays = 30, CancellationToken cancellationToken = default);
    Task<Dictionary<string, double>> GetResourceUtilizationTrendsAsync(string resourceType, int days = 30, CancellationToken cancellationToken = default);
    Task<List<Dictionary<string, object>>> GetCapacityRecommendationsAsync(CancellationToken cancellationToken = default);

    // Monitoring Control (for Hubs)
    Task<string> StartMonitoringAsync(CancellationToken cancellationToken = default);
    Task StopMonitoringAsync(string monitoringId, CancellationToken cancellationToken = default);
    Task<string> StartResourceMonitoringAsync(string resourceType, TimeSpan interval, CancellationToken cancellationToken = default);
    Task StopResourceMonitoringAsync(string monitoringId, CancellationToken cancellationToken = default);
}
