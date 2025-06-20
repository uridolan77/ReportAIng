namespace BIReportingCopilot.Core.Models;

// =============================================================================
// COST CONTROL AND OPTIMIZATION MODELS
// =============================================================================

/// <summary>
/// Cost tracking entry for AI provider usage
/// </summary>
public class CostTrackingEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public string RequestType { get; set; } = string.Empty;
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens { get; set; }
    public decimal Cost { get; set; }
    public decimal CostPerToken { get; set; }
    public long DurationMs { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? RequestId { get; set; }
    public string? QueryId { get; set; }
    public string? Department { get; set; }
    public string? Project { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Budget management for cost control
/// </summary>
public class BudgetManagement
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // User, Department, Project, Global
    public string EntityId { get; set; } = string.Empty; // UserId, DepartmentId, ProjectId, etc.
    public decimal BudgetAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount => BudgetAmount - SpentAmount;
    public BudgetPeriod Period { get; set; } = BudgetPeriod.Monthly;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal AlertThreshold { get; set; } = 0.8m; // Alert at 80%
    public decimal BlockThreshold { get; set; } = 1.0m; // Block at 100%
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Budget period enumeration
/// </summary>
public enum BudgetPeriod
{
    Daily,
    Weekly,
    Monthly,
    Quarterly,
    Yearly,
    Custom
}

/// <summary>
/// Resource usage tracking for monitoring
/// </summary>
public class ResourceUsageEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty; // AI_CALL, DB_QUERY, CACHE_ACCESS, etc.
    public string ResourceId { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public long DurationMs { get; set; }
    public decimal Cost { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? RequestId { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Resource quota for user limits
/// </summary>
public class ResourceQuota
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public int MaxQuantity { get; set; }
    public int CurrentUsage { get; set; }
    public TimeSpan Period { get; set; } = TimeSpan.FromDays(1);
    public DateTime ResetDate { get; set; } = DateTime.UtcNow.AddDays(1);
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Performance metrics tracking
/// </summary>
public class PerformanceMetricsEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? EntityId { get; set; }
    public string? EntityType { get; set; }
    public Dictionary<string, object> Tags { get; set; } = new();
}

/// <summary>
/// Cache statistics tracking
/// </summary>
public class CacheStatisticsEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CacheType { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty; // HIT, MISS, SET, DELETE
    public string Key { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public long DurationMs { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? UserId { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Cache statistics aggregated data
/// </summary>
public class CacheStatistics
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CacheType { get; set; } = string.Empty;
    public long TotalOperations { get; set; }
    public long HitCount { get; set; }
    public long MissCount { get; set; }
    public long SetCount { get; set; }
    public long DeleteCount { get; set; }
    public double HitRate { get; set; }
    public double AverageResponseTime { get; set; }
    public long TotalSizeBytes { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Additional properties expected by CacheService
    public int TotalEntries { get; set; }
    public int TotalRequests => (int)(HitCount + MissCount);
    public TimeSpan AverageRetrievalTime { get; set; }
    public Dictionary<string, object> AdditionalStats { get; set; } = new();
}

/// <summary>
/// Cache configuration for optimization
/// </summary>
public class CacheConfiguration
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CacheType { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public TimeSpan DefaultTtl { get; set; }
    public long MaxSizeBytes { get; set; }
    public int MaxEntries { get; set; }
    public string EvictionPolicy { get; set; } = "LRU";
    public bool EnableCompression { get; set; } = false;
    public bool EnableEncryption { get; set; } = false;
    public Dictionary<string, object> Settings { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

// Note: Duplicate ResourceQuota class removed - using the one defined earlier in this file

/// <summary>
/// Cost prediction result
/// </summary>
public class CostPrediction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string QueryId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
    public decimal ConfidenceScore { get; set; }
    public string ModelUsed { get; set; } = string.Empty;
    public int EstimatedTokens { get; set; }
    public long EstimatedDurationMs { get; set; }
    public Dictionary<string, object> Factors { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Model selection criteria
/// </summary>
public class ModelSelectionCriteria
{
    public string QueryComplexity { get; set; } = string.Empty;
    public decimal MaxCost { get; set; }
    public long MaxDurationMs { get; set; }
    public double MinAccuracy { get; set; }
    public string Priority { get; set; } = "Balanced"; // Cost, Speed, Accuracy, Balanced
    public Dictionary<string, object> CustomCriteria { get; set; } = new();
}

/// <summary>
/// Model selection result
/// </summary>
public class ModelSelectionResult
{
    public string SelectedModel { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
    public long EstimatedDurationMs { get; set; }
    public double ConfidenceScore { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public List<ModelOption> AlternativeOptions { get; set; } = new();
}

/// <summary>
/// Model option for selection
/// </summary>
public class ModelOption
{
    public string ModelId { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public decimal EstimatedCost { get; set; }
    public long EstimatedDurationMs { get; set; }
    public double AccuracyScore { get; set; }
    public bool IsAvailable { get; set; } = true;
    public Dictionary<string, object> Capabilities { get; set; } = new();
}

/// <summary>
/// Cost optimization recommendation
/// </summary>
public class CostOptimizationRecommendation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PotentialSavings { get; set; }
    public double ImpactScore { get; set; }
    public string Priority { get; set; } = "Medium";
    public string Implementation { get; set; } = string.Empty;
    public List<string> Benefits { get; set; } = new();
    public List<string> Risks { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsImplemented { get; set; } = false;
}

/// <summary>
/// Performance benchmark result
/// </summary>
public class PerformanceBenchmark
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double BaselineValue { get; set; }
    public double CurrentValue { get; set; }
    public double ImprovementPercentage => BaselineValue > 0 ? ((CurrentValue - BaselineValue) / BaselineValue) * 100 : 0;
    public string Unit { get; set; } = string.Empty;
    public DateTime BaselineDate { get; set; }
    public DateTime MeasurementDate { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Cost analytics summary
/// </summary>
public class CostAnalyticsSummary
{
    public decimal TotalCost { get; set; }
    public decimal DailyCost { get; set; }
    public decimal WeeklyCost { get; set; }
    public decimal MonthlyCost { get; set; }
    public Dictionary<string, decimal> CostByProvider { get; set; } = new();
    public Dictionary<string, decimal> CostByUser { get; set; } = new();
    public Dictionary<string, decimal> CostByDepartment { get; set; } = new();
    public Dictionary<string, decimal> CostByModel { get; set; } = new();
    public List<CostTrend> Trends { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // Additional properties expected by CostAnalyticsService
    public double CostEfficiency { get; set; }
    public decimal PredictedMonthlyCost { get; set; }
    public List<CostOptimizationRecommendation> CostSavingsOpportunities { get; set; } = new();
    public Dictionary<string, object> ROIMetrics { get; set; } = new();
}

/// <summary>
/// Cost trend data
/// </summary>
public class CostTrend
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

// =============================================================================
// MISSING MODEL CLASSES FOR INTERFACES
// =============================================================================

/// <summary>
/// Cache warming configuration
/// </summary>
public class CacheWarmingConfig
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string CacheType { get; set; } = string.Empty;
    public List<string> WarmupQueries { get; set; } = new();
    public string Schedule { get; set; } = string.Empty; // Cron expression
    public int Priority { get; set; } = 1;
    public bool IsEnabled { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan WarmupInterval { get; set; } = TimeSpan.FromHours(6);
    public int MaxConcurrentWarmups { get; set; } = 5;
    public DateTime? LastWarmup { get; set; }
    public DateTime? NextWarmup { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Performance optimization suggestion
/// </summary>
public class PerformanceOptimizationSuggestion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty; // Query, Cache, Resource, Model
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double ImpactScore { get; set; } // 0.0 - 1.0
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    public string Implementation { get; set; } = string.Empty;
    public List<string> Benefits { get; set; } = new();
    public List<string> Risks { get; set; } = new();
    public decimal EstimatedSavings { get; set; }
    public TimeSpan EstimatedImplementationTime { get; set; }
    public bool IsImplemented { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Additional properties expected by PerformanceOptimizationService
    public double EstimatedImprovement { get; set; }
    public List<string> Requirements { get; set; } = new();
}

/// <summary>
/// Resource monitoring alert
/// </summary>
public class ResourceMonitoringAlert
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string AlertType { get; set; } = string.Empty; // Quota, Performance, Cost, Error
    public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public double Threshold { get; set; }
    public double CurrentValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public bool IsResolved { get; set; } = false;
    public DateTime? ResolvedAt { get; set; }
    public string? Resolution { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<string> Actions { get; set; } = new();
}

// Note: CircuitBreakerState enum moved to CircuitBreakerStateEnum to avoid naming conflicts

/// <summary>
/// Circuit breaker state class (used by ResourceManagementService)
/// </summary>
public class CircuitBreakerState
{
    public string Id { get; set; } = string.Empty;
    public string ServiceName { get; set; } = string.Empty;
    public string State { get; set; } = "Closed"; // Closed, Open, HalfOpen
    public int FailureCount { get; set; }
    public int FailureThreshold { get; set; } = 5;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1);
    public DateTime LastFailure { get; set; } = DateTime.MinValue;
    public DateTime? NextRetry { get; set; }
    public bool IsEnabled { get; set; } = true;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Circuit breaker status (for API responses)
/// </summary>
public class CircuitBreakerStatus
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public CircuitBreakerStateEnum State { get; set; } = CircuitBreakerStateEnum.Closed;
    public int FailureCount { get; set; }
    public int FailureThreshold { get; set; } = 5;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(1);
    public DateTime? LastFailureTime { get; set; }
    public DateTime? LastSuccessTime { get; set; }
    public double SuccessRate { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Circuit breaker state enum
/// </summary>
public enum CircuitBreakerStateEnum
{
    Closed,
    Open,
    HalfOpen
}

/// <summary>
/// Resource allocation result
/// </summary>
public class ResourceAllocationResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ResourceType { get; set; } = string.Empty;
    public string ResourceId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public int AllocatedQuantity { get; set; }
    public int RequestedQuantity { get; set; }
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public TimeSpan AllocationDuration { get; set; }
    public DateTime AllocatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReleasedAt { get; set; }
    public string Priority { get; set; } = "Normal";
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Performance optimization result
/// </summary>
public class PerformanceOptimizationResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OptimizationType { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public double BaselineValue { get; set; }
    public double OptimizedValue { get; set; }
    public double ImprovementPercentage => BaselineValue > 0 ? ((OptimizedValue - BaselineValue) / BaselineValue) * 100 : 0;
    public string Unit { get; set; } = string.Empty;
    public bool IsSuccessful { get; set; }
    public string? FailureReason { get; set; }
    public TimeSpan OptimizationDuration { get; set; }
    public DateTime OptimizedAt { get; set; } = DateTime.UtcNow;
    public List<string> AppliedOptimizations { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Cache optimization recommendation
/// </summary>
public class CacheOptimizationRecommendation
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty; // Configuration, Warming, Invalidation, Performance
    public string CacheType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double ImpactScore { get; set; } // 0.0 - 1.0
    public string Priority { get; set; } = "Medium"; // Low, Medium, High, Critical
    public string Implementation { get; set; } = string.Empty;
    public List<string> Benefits { get; set; } = new();
    public List<string> Risks { get; set; } = new();
    public decimal EstimatedPerformanceGain { get; set; } // Percentage improvement
    public decimal EstimatedCostSavings { get; set; }
    public decimal PotentialSavings { get; set; } // Alias for EstimatedCostSavings
    public TimeSpan EstimatedImplementationTime { get; set; }
    public bool IsImplemented { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Configuration { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Performance alert for real-time monitoring
/// </summary>
public class PerformanceAlert
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string AlertType { get; set; } = string.Empty; // ResponseTime, ErrorRate, Throughput, Resource
    public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty; // Component that triggered the alert
    public string MetricName { get; set; } = string.Empty;
    public double Threshold { get; set; }
    public double CurrentValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime TriggeredAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgedBy { get; set; }
    public bool IsResolved { get; set; } = false;
    public DateTime? ResolvedAt { get; set; }
    public string? Resolution { get; set; }
    public TimeSpan Duration => IsResolved && ResolvedAt.HasValue ? ResolvedAt.Value - TriggeredAt : DateTime.UtcNow - TriggeredAt;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public List<string> RecommendedActions { get; set; } = new();
}

/// <summary>
/// Performance tuning result
/// </summary>
public class TuningResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string EntityId { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public double ImprovementScore { get; set; }
    public List<string> OptimizationSteps { get; set; } = new();
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}
