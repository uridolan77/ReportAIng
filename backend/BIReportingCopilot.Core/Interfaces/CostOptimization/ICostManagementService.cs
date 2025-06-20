namespace BIReportingCopilot.Core.Interfaces.CostOptimization;

using BIReportingCopilot.Core.Models;

/// <summary>
/// Core cost tracking and management service interface
/// </summary>
public interface ICostManagementService
{
    // Cost Tracking
    Task<CostTrackingEntry> TrackCostAsync(CostTrackingEntry entry, CancellationToken cancellationToken = default);
    Task<List<CostTrackingEntry>> GetCostHistoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalCostAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetCostByProviderAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetCostByModelAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    // Budget Management
    Task<BudgetManagement> CreateBudgetAsync(BudgetManagement budget, CancellationToken cancellationToken = default);
    Task<BudgetManagement> UpdateBudgetAsync(BudgetManagement budget, CancellationToken cancellationToken = default);
    Task<BudgetManagement?> GetBudgetAsync(string budgetId, CancellationToken cancellationToken = default);
    Task<List<BudgetManagement>> GetBudgetsAsync(string entityId, string type, CancellationToken cancellationToken = default);
    Task<bool> DeleteBudgetAsync(string budgetId, CancellationToken cancellationToken = default);
    Task<bool> CheckBudgetLimitAsync(string userId, decimal additionalCost, CancellationToken cancellationToken = default);
    Task<List<BudgetManagement>> GetBudgetsNearLimitAsync(decimal threshold = 0.8m, CancellationToken cancellationToken = default);

    // Cost Prediction
    Task<CostPrediction> PredictCostAsync(string queryId, string userId, ModelSelectionCriteria criteria, CancellationToken cancellationToken = default);
    Task<List<CostPrediction>> GetCostPredictionsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    // Cost Analytics
    Task<CostAnalyticsSummary> GetCostAnalyticsAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<List<CostTrend>> GetCostTrendsAsync(string? userId = null, string? category = null, int days = 30, CancellationToken cancellationToken = default);
    Task<Dictionary<string, decimal>> GetCostBreakdownAsync(string breakdownType, string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    // Cost Optimization
    Task<List<CostOptimizationRecommendation>> GetCostOptimizationRecommendationsAsync(string? userId = null, CancellationToken cancellationToken = default);
    Task<CostOptimizationRecommendation> CreateRecommendationAsync(CostOptimizationRecommendation recommendation, CancellationToken cancellationToken = default);
    Task<bool> ImplementRecommendationAsync(string recommendationId, CancellationToken cancellationToken = default);

    // Provider Cost Management
    Task<Dictionary<string, decimal>> GetProviderCostRatesAsync(CancellationToken cancellationToken = default);
    Task UpdateProviderCostRateAsync(string providerId, string modelId, decimal costPerToken, CancellationToken cancellationToken = default);
    Task<decimal> CalculateEstimatedCostAsync(string providerId, string modelId, int estimatedTokens, CancellationToken cancellationToken = default);

    // Alerts and Notifications
    Task<List<CostAlert>> GetActiveAlertsAsync(string? userId = null, CancellationToken cancellationToken = default);
    Task<CostAlert> CreateAlertAsync(CostAlert alert, CancellationToken cancellationToken = default);
    Task<bool> ResolveAlertAsync(string alertId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Dynamic model selection service interface
/// </summary>
public interface IDynamicModelSelectionService
{
    // Model Selection
    Task<ModelSelectionResult> SelectOptimalModelAsync(ModelSelectionCriteria criteria, CancellationToken cancellationToken = default);
    Task<List<ModelOption>> GetAvailableModelsAsync(CancellationToken cancellationToken = default);
    Task<ModelOption?> GetModelInfoAsync(string providerId, string modelId, CancellationToken cancellationToken = default);

    // Model Performance Tracking
    Task TrackModelPerformanceAsync(string providerId, string modelId, long durationMs, decimal cost, double accuracy, CancellationToken cancellationToken = default);
    Task<Dictionary<string, double>> GetModelPerformanceMetricsAsync(string providerId, string modelId, CancellationToken cancellationToken = default);

    // Provider Failover
    Task<ModelSelectionResult> SelectWithFailoverAsync(ModelSelectionCriteria criteria, List<string> excludeProviders, CancellationToken cancellationToken = default);
    Task<bool> IsProviderAvailableAsync(string providerId, CancellationToken cancellationToken = default);
    Task MarkProviderUnavailableAsync(string providerId, TimeSpan unavailableDuration, CancellationToken cancellationToken = default);

    // Model Capabilities
    Task<Dictionary<string, object>> GetModelCapabilitiesAsync(string providerId, string modelId, CancellationToken cancellationToken = default);
    Task UpdateModelCapabilitiesAsync(string providerId, string modelId, Dictionary<string, object> capabilities, CancellationToken cancellationToken = default);

    // Selection Analytics
    Task<Dictionary<string, int>> GetModelSelectionStatsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<double> GetModelSelectionAccuracyAsync(string providerId, string modelId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Performance optimization service interface
/// </summary>
public interface IPerformanceOptimizationService
{
    // Performance Analysis
    Task<PerformanceMetrics> AnalyzePerformanceAsync(string entityId, string entityType, CancellationToken cancellationToken = default);
    Task<List<PerformanceBottleneck>> IdentifyBottlenecksAsync(string entityId, string entityType, CancellationToken cancellationToken = default);
    Task<List<PerformanceOptimizationSuggestion>> GetOptimizationSuggestionsAsync(string entityId, string entityType, CancellationToken cancellationToken = default);

    // Performance Benchmarking
    Task<PerformanceBenchmark> CreateBenchmarkAsync(string name, string category, double value, string unit, CancellationToken cancellationToken = default);
    Task<PerformanceBenchmark> UpdateBenchmarkAsync(string benchmarkId, double newValue, CancellationToken cancellationToken = default);
    Task<List<PerformanceBenchmark>> GetBenchmarksAsync(string? category = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, double>> GetPerformanceImprovementsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);

    // Performance Monitoring
    Task RecordPerformanceMetricAsync(PerformanceMetricsEntry metric, CancellationToken cancellationToken = default);
    Task<List<PerformanceMetricsEntry>> GetPerformanceMetricsAsync(string metricName, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, double>> GetAggregatedMetricsAsync(string metricName, string aggregationType, TimeSpan period, CancellationToken cancellationToken = default);

    // Automatic Performance Tuning
    Task<TuningResult> AutoTunePerformanceAsync(string entityId, string entityType, CancellationToken cancellationToken = default);
    Task<List<TuningResult>> GetTuningHistoryAsync(string entityId, string entityType, CancellationToken cancellationToken = default);
    Task<bool> ApplyPerformanceOptimizationAsync(string optimizationId, CancellationToken cancellationToken = default);

    // Performance Alerting
    Task<List<ResourceMonitoringAlert>> GetActivePerformanceAlertsAsync(CancellationToken cancellationToken = default);
    Task<ResourceMonitoringAlert> CreatePerformanceAlertAsync(ResourceMonitoringAlert alert, CancellationToken cancellationToken = default);
    Task<bool> ResolvePerformanceAlertAsync(string alertId, string resolutionNotes, CancellationToken cancellationToken = default);
}

/// <summary>
/// Resource management and throttling service interface
/// </summary>
public interface IResourceManagementService
{
    // Resource Quotas
    Task<ResourceQuota> CreateResourceQuotaAsync(ResourceQuota quota, CancellationToken cancellationToken = default);
    Task<ResourceQuota> UpdateResourceQuotaAsync(ResourceQuota quota, CancellationToken cancellationToken = default);
    Task<ResourceQuota?> GetResourceQuotaAsync(string userId, string resourceType, CancellationToken cancellationToken = default);
    Task<List<ResourceQuota>> GetUserResourceQuotasAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteResourceQuotaAsync(string quotaId, CancellationToken cancellationToken = default);

    // Resource Usage Tracking
    Task TrackResourceUsageAsync(ResourceUsageEntry usage, CancellationToken cancellationToken = default);
    Task<bool> CheckResourceQuotaAsync(string userId, string resourceType, int requestedQuantity, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetCurrentResourceUsageAsync(string userId, CancellationToken cancellationToken = default);
    Task ResetResourceUsageAsync(string userId, string resourceType, CancellationToken cancellationToken = default);

    // Priority-Based Processing
    Task<int> GetUserPriorityAsync(string userId, CancellationToken cancellationToken = default);
    Task SetUserPriorityAsync(string userId, int priority, CancellationToken cancellationToken = default);
    Task<List<string>> GetQueuedRequestsAsync(string resourceType, CancellationToken cancellationToken = default);
    Task<bool> QueueRequestAsync(string requestId, string userId, string resourceType, int priority, CancellationToken cancellationToken = default);

    // Circuit Breakers
    Task<CircuitBreakerState> GetCircuitBreakerStateAsync(string serviceName, CancellationToken cancellationToken = default);
    Task UpdateCircuitBreakerStateAsync(string serviceName, string state, CancellationToken cancellationToken = default);
    Task<bool> IsServiceAvailableAsync(string serviceName, CancellationToken cancellationToken = default);
    Task RecordServiceFailureAsync(string serviceName, CancellationToken cancellationToken = default);
    Task RecordServiceSuccessAsync(string serviceName, CancellationToken cancellationToken = default);

    // Load Balancing
    Task<string> SelectOptimalResourceAsync(string resourceType, CancellationToken cancellationToken = default);
    Task UpdateResourceLoadAsync(string resourceId, double loadPercentage, CancellationToken cancellationToken = default);
    Task<Dictionary<string, double>> GetResourceLoadStatsAsync(string resourceType, CancellationToken cancellationToken = default);
}
