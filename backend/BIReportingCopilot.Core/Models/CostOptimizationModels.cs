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
/// Performance metrics tracking
/// </summary>
public class PerformanceMetricsEntry
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
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

/// <summary>
/// Resource quota configuration
/// </summary>
public class ResourceQuota
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public int MaxQuantity { get; set; }
    public TimeSpan Period { get; set; }
    public int CurrentUsage { get; set; }
    public DateTime ResetDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

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
