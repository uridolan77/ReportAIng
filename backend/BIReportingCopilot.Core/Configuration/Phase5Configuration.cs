using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Configuration;

/// <summary>
/// Phase 5: Cost Control & Performance Optimization Configuration
/// </summary>
public class Phase5Configuration
{
    public CostOptimizationConfiguration CostOptimization { get; set; } = new();
    public PerformanceOptimizationConfiguration PerformanceOptimization { get; set; } = new();
    public CacheOptimizationConfiguration CacheOptimization { get; set; } = new();
    public ResourceManagementConfiguration ResourceManagement { get; set; } = new();
    public RealTimeMonitoringConfiguration RealTimeMonitoring { get; set; } = new();
}

/// <summary>
/// Cost optimization and management configuration
/// </summary>
public class CostOptimizationConfiguration
{
    /// <summary>
    /// Enable cost tracking and analytics
    /// </summary>
    public bool EnableCostTracking { get; set; } = true;

    /// <summary>
    /// Enable real-time cost monitoring
    /// </summary>
    public bool EnableRealTimeMonitoring { get; set; } = true;

    /// <summary>
    /// Enable budget management and alerts
    /// </summary>
    public bool EnableBudgetManagement { get; set; } = true;

    /// <summary>
    /// Enable cost optimization recommendations
    /// </summary>
    public bool EnableOptimizationRecommendations { get; set; } = true;

    /// <summary>
    /// Cost policies configuration
    /// </summary>
    public CostPoliciesConfiguration Policies { get; set; } = new();

    /// <summary>
    /// Cost analytics configuration
    /// </summary>
    public CostAnalyticsConfiguration Analytics { get; set; } = new();

    /// <summary>
    /// Budget management configuration
    /// </summary>
    public BudgetManagementConfiguration BudgetManagement { get; set; } = new();

    /// <summary>
    /// Cost forecasting configuration
    /// </summary>
    public CostForecastingConfiguration Forecasting { get; set; } = new();
}

/// <summary>
/// Cost policies configuration
/// </summary>
public class CostPoliciesConfiguration
{
    /// <summary>
    /// Default cost limits per user
    /// </summary>
    public CostLimitsConfiguration DefaultLimits { get; set; } = new();

    /// <summary>
    /// Cost alert thresholds
    /// </summary>
    public CostAlertConfiguration Alerts { get; set; } = new();

    /// <summary>
    /// Cost optimization policies
    /// </summary>
    public CostOptimizationPolicies Optimization { get; set; } = new();

    /// <summary>
    /// Provider-specific cost policies
    /// </summary>
    public Dictionary<string, ProviderCostPolicy> ProviderPolicies { get; set; } = new();
}

/// <summary>
/// Cost limits configuration
/// </summary>
public class CostLimitsConfiguration
{
    /// <summary>
    /// Daily cost limit per user (USD)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Daily cost limit must be non-negative")]
    public decimal DailyCostLimit { get; set; } = 100m;

    /// <summary>
    /// Monthly cost limit per user (USD)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Monthly cost limit must be non-negative")]
    public decimal MonthlyCostLimit { get; set; } = 1000m;

    /// <summary>
    /// Cost per query limit (USD)
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "Cost per query limit must be non-negative")]
    public decimal CostPerQueryLimit { get; set; } = 10m;

    /// <summary>
    /// Maximum tokens per request
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Maximum tokens must be positive")]
    public int MaxTokensPerRequest { get; set; } = 4000;

    /// <summary>
    /// Enable automatic cost blocking when limits exceeded
    /// </summary>
    public bool EnableAutomaticBlocking { get; set; } = true;
}

/// <summary>
/// Cost alert configuration
/// </summary>
public class CostAlertConfiguration
{
    /// <summary>
    /// Enable cost alerts
    /// </summary>
    public bool EnableAlerts { get; set; } = true;

    /// <summary>
    /// Budget utilization threshold for warnings (0.0 - 1.0)
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "Warning threshold must be between 0 and 1")]
    public double WarningThreshold { get; set; } = 0.8;

    /// <summary>
    /// Budget utilization threshold for critical alerts (0.0 - 1.0)
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "Critical threshold must be between 0 and 1")]
    public double CriticalThreshold { get; set; } = 0.95;

    /// <summary>
    /// Alert cooldown period to prevent spam
    /// </summary>
    public TimeSpan AlertCooldownPeriod { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Enable email notifications for cost alerts
    /// </summary>
    public bool EnableEmailNotifications { get; set; } = true;

    /// <summary>
    /// Enable real-time notifications for cost alerts
    /// </summary>
    public bool EnableRealTimeNotifications { get; set; } = true;
}

/// <summary>
/// Cost optimization policies
/// </summary>
public class CostOptimizationPolicies
{
    /// <summary>
    /// Enable automatic model selection based on cost
    /// </summary>
    public bool EnableAutomaticModelSelection { get; set; } = true;

    /// <summary>
    /// Prefer cheaper models when quality difference is minimal
    /// </summary>
    public bool PreferCheaperModels { get; set; } = true;

    /// <summary>
    /// Quality threshold for model selection (0.0 - 1.0)
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "Quality threshold must be between 0 and 1")]
    public double QualityThreshold { get; set; } = 0.85;

    /// <summary>
    /// Maximum cost increase allowed for quality improvement
    /// </summary>
    [Range(0.0, double.MaxValue, ErrorMessage = "Max cost increase must be non-negative")]
    public double MaxCostIncreaseForQuality { get; set; } = 0.5; // 50% increase

    /// <summary>
    /// Enable cache-first strategy to reduce costs
    /// </summary>
    public bool EnableCacheFirstStrategy { get; set; } = true;

    /// <summary>
    /// Enable query optimization to reduce token usage
    /// </summary>
    public bool EnableQueryOptimization { get; set; } = true;
}

/// <summary>
/// Provider-specific cost policy
/// </summary>
public class ProviderCostPolicy
{
    /// <summary>
    /// Provider name
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Maximum cost per request for this provider
    /// </summary>
    public decimal MaxCostPerRequest { get; set; } = 5m;

    /// <summary>
    /// Rate limits for this provider
    /// </summary>
    public ProviderRateLimits RateLimits { get; set; } = new();

    /// <summary>
    /// Cost multiplier for this provider (for internal accounting)
    /// </summary>
    public decimal CostMultiplier { get; set; } = 1.0m;

    /// <summary>
    /// Enable this provider for cost optimization
    /// </summary>
    public bool EnableForOptimization { get; set; } = true;
}

/// <summary>
/// Provider rate limits
/// </summary>
public class ProviderRateLimits
{
    /// <summary>
    /// Requests per minute
    /// </summary>
    public int RequestsPerMinute { get; set; } = 60;

    /// <summary>
    /// Requests per hour
    /// </summary>
    public int RequestsPerHour { get; set; } = 1000;

    /// <summary>
    /// Requests per day
    /// </summary>
    public int RequestsPerDay { get; set; } = 10000;

    /// <summary>
    /// Tokens per minute
    /// </summary>
    public int TokensPerMinute { get; set; } = 100000;
}

/// <summary>
/// Cost analytics configuration
/// </summary>
public class CostAnalyticsConfiguration
{
    /// <summary>
    /// Enable detailed cost analytics
    /// </summary>
    public bool EnableDetailedAnalytics { get; set; } = true;

    /// <summary>
    /// Enable ROI analysis
    /// </summary>
    public bool EnableROIAnalysis { get; set; } = true;

    /// <summary>
    /// Analytics data retention period
    /// </summary>
    public TimeSpan DataRetentionPeriod { get; set; } = TimeSpan.FromDays(365);

    /// <summary>
    /// Analytics aggregation interval
    /// </summary>
    public TimeSpan AggregationInterval { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Enable cost trend analysis
    /// </summary>
    public bool EnableTrendAnalysis { get; set; } = true;

    /// <summary>
    /// Enable cost anomaly detection
    /// </summary>
    public bool EnableAnomalyDetection { get; set; } = true;
}

/// <summary>
/// Budget management configuration
/// </summary>
public class BudgetManagementConfiguration
{
    /// <summary>
    /// Enable budget management
    /// </summary>
    public bool EnableBudgetManagement { get; set; } = true;

    /// <summary>
    /// Default budget period
    /// </summary>
    public string DefaultBudgetPeriod { get; set; } = "Monthly";

    /// <summary>
    /// Enable automatic budget rollover
    /// </summary>
    public bool EnableAutomaticRollover { get; set; } = true;

    /// <summary>
    /// Budget rollover percentage (0.0 - 1.0)
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "Rollover percentage must be between 0 and 1")]
    public double RolloverPercentage { get; set; } = 0.1; // 10%

    /// <summary>
    /// Enable budget approval workflow
    /// </summary>
    public bool EnableApprovalWorkflow { get; set; } = false;

    /// <summary>
    /// Budget approval threshold (USD)
    /// </summary>
    public decimal ApprovalThreshold { get; set; } = 10000m;
}

/// <summary>
/// Cost forecasting configuration
/// </summary>
public class CostForecastingConfiguration
{
    /// <summary>
    /// Enable cost forecasting
    /// </summary>
    public bool EnableForecasting { get; set; } = true;

    /// <summary>
    /// Forecasting algorithm
    /// </summary>
    public string ForecastingAlgorithm { get; set; } = "LinearRegression";

    /// <summary>
    /// Forecasting horizon in days
    /// </summary>
    [Range(1, 365, ErrorMessage = "Forecasting horizon must be between 1 and 365 days")]
    public int ForecastingHorizonDays { get; set; } = 30;

    /// <summary>
    /// Historical data window for forecasting (days)
    /// </summary>
    [Range(7, 365, ErrorMessage = "Historical window must be between 7 and 365 days")]
    public int HistoricalWindowDays { get; set; } = 90;

    /// <summary>
    /// Forecasting confidence threshold
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "Confidence threshold must be between 0 and 1")]
    public double ConfidenceThreshold { get; set; } = 0.8;

    /// <summary>
    /// Enable seasonal adjustment
    /// </summary>
    public bool EnableSeasonalAdjustment { get; set; } = true;
}

/// <summary>
/// Performance optimization configuration
/// </summary>
public class PerformanceOptimizationConfiguration
{
    /// <summary>
    /// Enable performance monitoring
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = true;

    /// <summary>
    /// Enable automatic performance optimization
    /// </summary>
    public bool EnableAutomaticOptimization { get; set; } = true;

    /// <summary>
    /// Performance monitoring settings
    /// </summary>
    public PerformanceMonitoringSettings Monitoring { get; set; } = new();

    /// <summary>
    /// Performance optimization settings
    /// </summary>
    public PerformanceOptimizationSettings Optimization { get; set; } = new();

    /// <summary>
    /// Performance benchmarking settings
    /// </summary>
    public PerformanceBenchmarkingSettings Benchmarking { get; set; } = new();

    /// <summary>
    /// Performance alerting settings
    /// </summary>
    public PerformanceAlertingSettings Alerting { get; set; } = new();
}

/// <summary>
/// Performance monitoring settings
/// </summary>
public class PerformanceMonitoringSettings
{
    /// <summary>
    /// Monitoring interval in seconds
    /// </summary>
    [Range(1, 3600, ErrorMessage = "Monitoring interval must be between 1 and 3600 seconds")]
    public int MonitoringIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Enable real-time performance monitoring
    /// </summary>
    public bool EnableRealTimeMonitoring { get; set; } = true;

    /// <summary>
    /// Performance metrics retention period
    /// </summary>
    public TimeSpan MetricsRetentionPeriod { get; set; } = TimeSpan.FromDays(30);

    /// <summary>
    /// Enable performance metrics aggregation
    /// </summary>
    public bool EnableMetricsAggregation { get; set; } = true;

    /// <summary>
    /// Metrics aggregation interval
    /// </summary>
    public TimeSpan AggregationInterval { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Enable bottleneck detection
    /// </summary>
    public bool EnableBottleneckDetection { get; set; } = true;

    /// <summary>
    /// Performance baseline window in days
    /// </summary>
    [Range(1, 90, ErrorMessage = "Baseline window must be between 1 and 90 days")]
    public int BaselineWindowDays { get; set; } = 7;
}

/// <summary>
/// Performance optimization settings
/// </summary>
public class PerformanceOptimizationSettings
{
    /// <summary>
    /// Enable automatic query optimization
    /// </summary>
    public bool EnableQueryOptimization { get; set; } = true;

    /// <summary>
    /// Enable automatic caching optimization
    /// </summary>
    public bool EnableCacheOptimization { get; set; } = true;

    /// <summary>
    /// Enable automatic resource allocation optimization
    /// </summary>
    public bool EnableResourceOptimization { get; set; } = true;

    /// <summary>
    /// Optimization trigger threshold (performance degradation %)
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "Optimization threshold must be between 0 and 1")]
    public double OptimizationThreshold { get; set; } = 0.2; // 20% degradation

    /// <summary>
    /// Maximum optimization attempts per hour
    /// </summary>
    [Range(1, 100, ErrorMessage = "Max optimization attempts must be between 1 and 100")]
    public int MaxOptimizationAttemptsPerHour { get; set; } = 5;

    /// <summary>
    /// Optimization cooldown period
    /// </summary>
    public TimeSpan OptimizationCooldown { get; set; } = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Enable A/B testing for optimizations
    /// </summary>
    public bool EnableABTesting { get; set; } = true;
}

/// <summary>
/// Performance benchmarking settings
/// </summary>
public class PerformanceBenchmarkingSettings
{
    /// <summary>
    /// Enable performance benchmarking
    /// </summary>
    public bool EnableBenchmarking { get; set; } = true;

    /// <summary>
    /// Benchmark execution interval
    /// </summary>
    public TimeSpan BenchmarkInterval { get; set; } = TimeSpan.FromHours(6);

    /// <summary>
    /// Benchmark categories to run
    /// </summary>
    public List<string> BenchmarkCategories { get; set; } = new() { "Query", "Cache", "Database", "AI" };

    /// <summary>
    /// Benchmark timeout in seconds
    /// </summary>
    [Range(1, 3600, ErrorMessage = "Benchmark timeout must be between 1 and 3600 seconds")]
    public int BenchmarkTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Enable benchmark result comparison
    /// </summary>
    public bool EnableResultComparison { get; set; } = true;

    /// <summary>
    /// Benchmark result retention period
    /// </summary>
    public TimeSpan ResultRetentionPeriod { get; set; } = TimeSpan.FromDays(90);
}

/// <summary>
/// Performance alerting settings
/// </summary>
public class PerformanceAlertingSettings
{
    /// <summary>
    /// Enable performance alerts
    /// </summary>
    public bool EnableAlerts { get; set; } = true;

    /// <summary>
    /// Response time threshold in milliseconds
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Response time threshold must be positive")]
    public int ResponseTimeThresholdMs { get; set; } = 5000;

    /// <summary>
    /// Error rate threshold (0.0 - 1.0)
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "Error rate threshold must be between 0 and 1")]
    public double ErrorRateThreshold { get; set; } = 0.05; // 5%

    /// <summary>
    /// CPU usage threshold (0.0 - 1.0)
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "CPU usage threshold must be between 0 and 1")]
    public double CpuUsageThreshold { get; set; } = 0.8; // 80%

    /// <summary>
    /// Memory usage threshold (0.0 - 1.0)
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "Memory usage threshold must be between 0 and 1")]
    public double MemoryUsageThreshold { get; set; } = 0.85; // 85%

    /// <summary>
    /// Alert cooldown period
    /// </summary>
    public TimeSpan AlertCooldown { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Enable escalation for critical alerts
    /// </summary>
    public bool EnableEscalation { get; set; } = true;

    /// <summary>
    /// Escalation threshold in minutes
    /// </summary>
    [Range(1, 1440, ErrorMessage = "Escalation threshold must be between 1 and 1440 minutes")]
    public int EscalationThresholdMinutes { get; set; } = 30;
}

/// <summary>
/// Cache optimization configuration
/// </summary>
public class CacheOptimizationConfiguration
{
    /// <summary>
    /// Enable cache optimization
    /// </summary>
    public bool EnableCacheOptimization { get; set; } = true;

    /// <summary>
    /// Enable multi-layer caching
    /// </summary>
    public bool EnableMultiLayerCaching { get; set; } = true;

    /// <summary>
    /// Enable intelligent cache warming
    /// </summary>
    public bool EnableIntelligentWarming { get; set; } = true;

    /// <summary>
    /// Cache warming configuration
    /// </summary>
    public CacheWarmingConfiguration Warming { get; set; } = new();

    /// <summary>
    /// Cache invalidation configuration
    /// </summary>
    public CacheInvalidationConfiguration Invalidation { get; set; } = new();

    /// <summary>
    /// Cache performance configuration
    /// </summary>
    public CachePerformanceConfiguration Performance { get; set; } = new();

    /// <summary>
    /// Cache analytics configuration
    /// </summary>
    public CacheAnalyticsConfiguration Analytics { get; set; } = new();
}

/// <summary>
/// Cache warming configuration
/// </summary>
public class CacheWarmingConfiguration
{
    /// <summary>
    /// Enable cache warming
    /// </summary>
    public bool EnableWarming { get; set; } = true;

    /// <summary>
    /// Warming schedule (cron expression)
    /// </summary>
    public string WarmingSchedule { get; set; } = "0 */6 * * *"; // Every 6 hours

    /// <summary>
    /// Maximum warming operations per batch
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Max warming operations must be between 1 and 1000")]
    public int MaxWarmingOperationsPerBatch { get; set; } = 100;

    /// <summary>
    /// Warming timeout in seconds
    /// </summary>
    [Range(1, 3600, ErrorMessage = "Warming timeout must be between 1 and 3600 seconds")]
    public int WarmingTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Enable predictive warming based on usage patterns
    /// </summary>
    public bool EnablePredictiveWarming { get; set; } = true;

    /// <summary>
    /// Warming priority levels
    /// </summary>
    public List<string> WarmingPriorities { get; set; } = new() { "High", "Medium", "Low" };
}

/// <summary>
/// Cache invalidation configuration
/// </summary>
public class CacheInvalidationConfiguration
{
    /// <summary>
    /// Enable intelligent invalidation
    /// </summary>
    public bool EnableIntelligentInvalidation { get; set; } = true;

    /// <summary>
    /// Default TTL for cache entries
    /// </summary>
    public TimeSpan DefaultTtl { get; set; } = TimeSpan.FromHours(1);

    /// <summary>
    /// Enable dependency-based invalidation
    /// </summary>
    public bool EnableDependencyInvalidation { get; set; } = true;

    /// <summary>
    /// Enable time-based invalidation
    /// </summary>
    public bool EnableTimeBasedInvalidation { get; set; } = true;

    /// <summary>
    /// Enable event-based invalidation
    /// </summary>
    public bool EnableEventBasedInvalidation { get; set; } = true;

    /// <summary>
    /// Invalidation batch size
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Invalidation batch size must be between 1 and 1000")]
    public int InvalidationBatchSize { get; set; } = 50;
}

/// <summary>
/// Cache performance configuration
/// </summary>
public class CachePerformanceConfiguration
{
    /// <summary>
    /// Enable performance monitoring
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = true;

    /// <summary>
    /// Target cache hit rate (0.0 - 1.0)
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "Target hit rate must be between 0 and 1")]
    public double TargetHitRate { get; set; } = 0.85; // 85%

    /// <summary>
    /// Maximum cache response time in milliseconds
    /// </summary>
    [Range(1, 10000, ErrorMessage = "Max response time must be between 1 and 10000 ms")]
    public int MaxResponseTimeMs { get; set; } = 100;

    /// <summary>
    /// Enable cache compression
    /// </summary>
    public bool EnableCompression { get; set; } = true;

    /// <summary>
    /// Compression threshold in bytes
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Compression threshold must be positive")]
    public int CompressionThresholdBytes { get; set; } = 1024; // 1KB

    /// <summary>
    /// Enable cache encryption for sensitive data
    /// </summary>
    public bool EnableEncryption { get; set; } = false;
}

/// <summary>
/// Cache analytics configuration
/// </summary>
public class CacheAnalyticsConfiguration
{
    /// <summary>
    /// Enable cache analytics
    /// </summary>
    public bool EnableAnalytics { get; set; } = true;

    /// <summary>
    /// Analytics data retention period
    /// </summary>
    public TimeSpan DataRetentionPeriod { get; set; } = TimeSpan.FromDays(30);

    /// <summary>
    /// Enable hit rate analysis
    /// </summary>
    public bool EnableHitRateAnalysis { get; set; } = true;

    /// <summary>
    /// Enable performance trend analysis
    /// </summary>
    public bool EnablePerformanceTrendAnalysis { get; set; } = true;

    /// <summary>
    /// Enable cache usage pattern analysis
    /// </summary>
    public bool EnableUsagePatternAnalysis { get; set; } = true;

    /// <summary>
    /// Analytics aggregation interval
    /// </summary>
    public TimeSpan AggregationInterval { get; set; } = TimeSpan.FromMinutes(5);
}

/// <summary>
/// Resource management configuration
/// </summary>
public class ResourceManagementConfiguration
{
    /// <summary>
    /// Enable resource management
    /// </summary>
    public bool EnableResourceManagement { get; set; } = true;

    /// <summary>
    /// Enable resource quotas
    /// </summary>
    public bool EnableResourceQuotas { get; set; } = true;

    /// <summary>
    /// Enable priority-based processing
    /// </summary>
    public bool EnablePriorityProcessing { get; set; } = true;

    /// <summary>
    /// Resource quota configuration
    /// </summary>
    public ResourceQuotaConfiguration Quotas { get; set; } = new();

    /// <summary>
    /// Resource throttling configuration
    /// </summary>
    public ResourceThrottlingConfiguration Throttling { get; set; } = new();

    /// <summary>
    /// Resource monitoring configuration
    /// </summary>
    public ResourceMonitoringConfiguration Monitoring { get; set; } = new();

    /// <summary>
    /// Resource allocation configuration
    /// </summary>
    public ResourceAllocationConfiguration Allocation { get; set; } = new();
}

/// <summary>
/// Resource quota configuration
/// </summary>
public class ResourceQuotaConfiguration
{
    /// <summary>
    /// Default quotas for new users
    /// </summary>
    public Dictionary<string, int> DefaultQuotas { get; set; } = new()
    {
        { "ai-requests", 1000 },
        { "database-queries", 5000 },
        { "cache-operations", 10000 },
        { "report-generations", 100 },
        { "data-exports", 50 },
        { "api-calls", 2000 }
    };

    /// <summary>
    /// Quota reset period
    /// </summary>
    public TimeSpan QuotaResetPeriod { get; set; } = TimeSpan.FromDays(1);

    /// <summary>
    /// Enable quota warnings
    /// </summary>
    public bool EnableQuotaWarnings { get; set; } = true;

    /// <summary>
    /// Quota warning threshold (0.0 - 1.0)
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "Quota warning threshold must be between 0 and 1")]
    public double QuotaWarningThreshold { get; set; } = 0.8; // 80%

    /// <summary>
    /// Enable quota enforcement
    /// </summary>
    public bool EnableQuotaEnforcement { get; set; } = true;
}

/// <summary>
/// Resource throttling configuration
/// </summary>
public class ResourceThrottlingConfiguration
{
    /// <summary>
    /// Enable resource throttling
    /// </summary>
    public bool EnableThrottling { get; set; } = true;

    /// <summary>
    /// Throttling algorithm
    /// </summary>
    public string ThrottlingAlgorithm { get; set; } = "TokenBucket";

    /// <summary>
    /// Default rate limits
    /// </summary>
    public Dictionary<string, RateLimitConfiguration> DefaultRateLimits { get; set; } = new()
    {
        { "ai-requests", new RateLimitConfiguration { RequestsPerMinute = 60, RequestsPerHour = 1000 } },
        { "database-queries", new RateLimitConfiguration { RequestsPerMinute = 300, RequestsPerHour = 5000 } },
        { "api-calls", new RateLimitConfiguration { RequestsPerMinute = 100, RequestsPerHour = 2000 } }
    };

    /// <summary>
    /// Enable adaptive throttling
    /// </summary>
    public bool EnableAdaptiveThrottling { get; set; } = true;

    /// <summary>
    /// Throttling response strategy
    /// </summary>
    public string ThrottlingResponseStrategy { get; set; } = "Queue"; // Queue, Reject, Delay
}

/// <summary>
/// Rate limit configuration
/// </summary>
public class RateLimitConfiguration
{
    /// <summary>
    /// Requests per minute
    /// </summary>
    public int RequestsPerMinute { get; set; } = 60;

    /// <summary>
    /// Requests per hour
    /// </summary>
    public int RequestsPerHour { get; set; } = 1000;

    /// <summary>
    /// Requests per day
    /// </summary>
    public int RequestsPerDay { get; set; } = 10000;

    /// <summary>
    /// Burst allowance
    /// </summary>
    public int BurstAllowance { get; set; } = 10;
}

/// <summary>
/// Resource monitoring configuration
/// </summary>
public class ResourceMonitoringConfiguration
{
    /// <summary>
    /// Enable resource monitoring
    /// </summary>
    public bool EnableMonitoring { get; set; } = true;

    /// <summary>
    /// Monitoring interval in seconds
    /// </summary>
    [Range(1, 3600, ErrorMessage = "Monitoring interval must be between 1 and 3600 seconds")]
    public int MonitoringIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Enable real-time monitoring
    /// </summary>
    public bool EnableRealTimeMonitoring { get; set; } = true;

    /// <summary>
    /// Resource usage retention period
    /// </summary>
    public TimeSpan UsageRetentionPeriod { get; set; } = TimeSpan.FromDays(30);

    /// <summary>
    /// Enable resource usage alerts
    /// </summary>
    public bool EnableUsageAlerts { get; set; } = true;

    /// <summary>
    /// Resource usage alert threshold (0.0 - 1.0)
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "Usage alert threshold must be between 0 and 1")]
    public double UsageAlertThreshold { get; set; } = 0.9; // 90%
}

/// <summary>
/// Resource allocation configuration
/// </summary>
public class ResourceAllocationConfiguration
{
    /// <summary>
    /// Enable dynamic resource allocation
    /// </summary>
    public bool EnableDynamicAllocation { get; set; } = true;

    /// <summary>
    /// Resource allocation strategy
    /// </summary>
    public string AllocationStrategy { get; set; } = "Priority"; // Priority, RoundRobin, Weighted

    /// <summary>
    /// Enable resource pooling
    /// </summary>
    public bool EnableResourcePooling { get; set; } = true;

    /// <summary>
    /// Resource pool sizes
    /// </summary>
    public Dictionary<string, int> ResourcePoolSizes { get; set; } = new()
    {
        { "ai-processing", 10 },
        { "database-connections", 50 },
        { "cache-connections", 20 }
    };

    /// <summary>
    /// Enable resource preallocation
    /// </summary>
    public bool EnablePreallocation { get; set; } = true;
}

/// <summary>
/// Real-time monitoring configuration
/// </summary>
public class RealTimeMonitoringConfiguration
{
    /// <summary>
    /// Enable real-time monitoring
    /// </summary>
    public bool EnableRealTimeMonitoring { get; set; } = true;

    /// <summary>
    /// Enable SignalR hubs for real-time updates
    /// </summary>
    public bool EnableSignalRHubs { get; set; } = true;

    /// <summary>
    /// Real-time monitoring settings
    /// </summary>
    public RealTimeMonitoringSettings Monitoring { get; set; } = new();

    /// <summary>
    /// Real-time notification settings
    /// </summary>
    public RealTimeNotificationSettings Notifications { get; set; } = new();

    /// <summary>
    /// Real-time dashboard settings
    /// </summary>
    public RealTimeDashboardSettings Dashboard { get; set; } = new();

    /// <summary>
    /// Real-time alerting settings
    /// </summary>
    public RealTimeAlertingSettings Alerting { get; set; } = new();
}

/// <summary>
/// Real-time monitoring settings
/// </summary>
public class RealTimeMonitoringSettings
{
    /// <summary>
    /// Update interval in seconds
    /// </summary>
    [Range(1, 300, ErrorMessage = "Update interval must be between 1 and 300 seconds")]
    public int UpdateIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Enable cost monitoring
    /// </summary>
    public bool EnableCostMonitoring { get; set; } = true;

    /// <summary>
    /// Enable performance monitoring
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = true;

    /// <summary>
    /// Enable resource monitoring
    /// </summary>
    public bool EnableResourceMonitoring { get; set; } = true;

    /// <summary>
    /// Maximum concurrent connections
    /// </summary>
    [Range(1, 10000, ErrorMessage = "Max connections must be between 1 and 10000")]
    public int MaxConcurrentConnections { get; set; } = 1000;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    [Range(1, 3600, ErrorMessage = "Connection timeout must be between 1 and 3600 seconds")]
    public int ConnectionTimeoutSeconds { get; set; } = 300;
}

/// <summary>
/// Real-time notification settings
/// </summary>
public class RealTimeNotificationSettings
{
    /// <summary>
    /// Enable real-time notifications
    /// </summary>
    public bool EnableNotifications { get; set; } = true;

    /// <summary>
    /// Enable cost alerts
    /// </summary>
    public bool EnableCostAlerts { get; set; } = true;

    /// <summary>
    /// Enable performance alerts
    /// </summary>
    public bool EnablePerformanceAlerts { get; set; } = true;

    /// <summary>
    /// Enable resource alerts
    /// </summary>
    public bool EnableResourceAlerts { get; set; } = true;

    /// <summary>
    /// Notification delivery methods
    /// </summary>
    public List<string> DeliveryMethods { get; set; } = new() { "SignalR", "Email", "SMS" };

    /// <summary>
    /// Notification priority levels
    /// </summary>
    public List<string> PriorityLevels { get; set; } = new() { "Low", "Medium", "High", "Critical" };
}

/// <summary>
/// Real-time dashboard settings
/// </summary>
public class RealTimeDashboardSettings
{
    /// <summary>
    /// Enable real-time dashboards
    /// </summary>
    public bool EnableDashboards { get; set; } = true;

    /// <summary>
    /// Dashboard refresh interval in seconds
    /// </summary>
    [Range(1, 300, ErrorMessage = "Dashboard refresh interval must be between 1 and 300 seconds")]
    public int RefreshIntervalSeconds { get; set; } = 10;

    /// <summary>
    /// Enable cost dashboard
    /// </summary>
    public bool EnableCostDashboard { get; set; } = true;

    /// <summary>
    /// Enable performance dashboard
    /// </summary>
    public bool EnablePerformanceDashboard { get; set; } = true;

    /// <summary>
    /// Enable resource dashboard
    /// </summary>
    public bool EnableResourceDashboard { get; set; } = true;

    /// <summary>
    /// Dashboard data retention period
    /// </summary>
    public TimeSpan DataRetentionPeriod { get; set; } = TimeSpan.FromHours(24);

    /// <summary>
    /// Maximum data points per chart
    /// </summary>
    [Range(10, 10000, ErrorMessage = "Max data points must be between 10 and 10000")]
    public int MaxDataPointsPerChart { get; set; } = 1000;
}

/// <summary>
/// Real-time alerting settings
/// </summary>
public class RealTimeAlertingSettings
{
    /// <summary>
    /// Enable real-time alerting
    /// </summary>
    public bool EnableAlerting { get; set; } = true;

    /// <summary>
    /// Alert evaluation interval in seconds
    /// </summary>
    [Range(1, 300, ErrorMessage = "Alert evaluation interval must be between 1 and 300 seconds")]
    public int EvaluationIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Enable alert escalation
    /// </summary>
    public bool EnableEscalation { get; set; } = true;

    /// <summary>
    /// Alert escalation timeout in minutes
    /// </summary>
    [Range(1, 1440, ErrorMessage = "Escalation timeout must be between 1 and 1440 minutes")]
    public int EscalationTimeoutMinutes { get; set; } = 15;

    /// <summary>
    /// Maximum alerts per minute
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Max alerts per minute must be between 1 and 1000")]
    public int MaxAlertsPerMinute { get; set; } = 10;

    /// <summary>
    /// Alert suppression window in minutes
    /// </summary>
    [Range(1, 1440, ErrorMessage = "Suppression window must be between 1 and 1440 minutes")]
    public int SuppressionWindowMinutes { get; set; } = 5;
}
