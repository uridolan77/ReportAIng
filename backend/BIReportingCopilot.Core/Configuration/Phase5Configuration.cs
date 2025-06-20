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
