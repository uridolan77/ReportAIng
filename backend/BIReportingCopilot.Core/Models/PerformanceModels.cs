namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Query execution metrics for performance tracking
/// </summary>
public class QueryExecutionMetrics
{
    public string QueryId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime EndTime { get; set; } = DateTime.UtcNow;
    public int ExecutionTimeMs { get; set; }
    public int RowCount { get; set; }
    public long MemoryUsageBytes { get; set; }
    public int CpuUsagePercent { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
}

// QueryMetrics moved to MLModels.cs to avoid duplicates

/// <summary>
/// Performance goal for optimization
/// </summary>
public class PerformanceGoal
{
    public string GoalId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PerformanceGoalType Type { get; set; } = PerformanceGoalType.ExecutionTime;
    public double TargetValue { get; set; }
    public string Unit { get; set; } = string.Empty;
    public PerformanceGoalPriority Priority { get; set; } = PerformanceGoalPriority.Medium;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Comprehensive performance metrics for system monitoring (consolidated from multiple duplicates)
/// </summary>
public class PerformanceMetrics
{
    // Core response time metrics
    public double AverageResponseTime { get; set; }
    public double MedianResponseTime { get; set; }
    public double P95ResponseTime { get; set; }

    // System resource metrics
    public double ThroughputPerSecond { get; set; }
    public double ErrorRate { get; set; }
    public double SuccessRate { get; set; } = 1.0;
    public double CpuUsagePercent { get; set; }
    public double MemoryUsagePercent { get; set; }
    public double DiskUsagePercent { get; set; }
    public int ActiveConnections { get; set; }

    // Operation tracking (from PerformanceManagementService)
    public string OperationType { get; set; } = string.Empty;
    public int TotalOperations { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public TimeSpan TotalExecutionTime { get; set; }
    public TimeSpan AverageExecutionTime { get; set; }
    public int TotalResultCount { get; set; }

    // Metadata
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public Dictionary<string, double> CustomMetrics { get; set; } = new();
}

/// <summary>
/// Cache statistics for monitoring
/// </summary>
public class CacheStatistics
{
    public int TotalEntries { get; set; }
    public long TotalSizeBytes { get; set; }
    public int HitCount { get; set; }
    public int MissCount { get; set; }
    public double HitRate => TotalRequests > 0 ? (double)HitCount / TotalRequests : 0.0;
    public int TotalRequests => HitCount + MissCount;
    public TimeSpan AverageRetrievalTime { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> AdditionalStats { get; set; } = new();
}

/// <summary>
/// SQL query result with metadata
/// </summary>
public class SqlQueryResult
{
    public bool Success { get; set; }
    public bool IsSuccessful => Success; // Alias for Success property
    public string? Error { get; set; }
    public List<Dictionary<string, object>> Data { get; set; } = new();
    public int RowCount { get; set; }
    public int ExecutionTimeMs { get; set; }
    public string ExecutedSql { get; set; } = string.Empty;
    public QueryExecutionMetrics Metrics { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// SQL query metadata for tracking
/// </summary>
public class SqlQueryMetadata
{
    public string QueryId { get; set; } = string.Empty;
    public string QueryType { get; set; } = string.Empty;
    public List<string> TablesAccessed { get; set; } = new();
    public int ParameterCount { get; set; }
    public double ComplexityScore { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, object> AdditionalMetadata { get; set; } = new();
    public string? Error { get; set; }
    public int ExecutionTimeMs { get; set; }
}

/// <summary>
/// Trend analysis for performance data
/// </summary>
public class TrendAnalysis
{
    public string AnalysisId { get; set; } = Guid.NewGuid().ToString();
    public string MetricName { get; set; } = string.Empty;
    public TrendDirection Direction { get; set; } = TrendDirection.Stable;
    public double Slope { get; set; }
    public double Confidence { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<TrendDataPoint> DataPoints { get; set; } = new();
    public List<string> Insights { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Data point for trend analysis
/// </summary>
public class TrendDataPoint
{
    public DateTime Timestamp { get; set; }
    public double Value { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Export options for various formats
/// </summary>
public class ExportOptions
{
    public int? Width { get; set; }
    public int? Height { get; set; }
    public string? Theme { get; set; }
    public bool IncludeData { get; set; } = true;
    public bool IncludeMetadata { get; set; } = true;
    public string? Watermark { get; set; }
    public Dictionary<string, object> CustomOptions { get; set; } = new();
}

// NLUMetrics and NLUPerformancePattern moved to AIModels.cs to avoid duplicates

/// <summary>
/// Index suggestion for database optimization
/// </summary>
public class IndexSuggestion
{
    public string SuggestionId { get; set; } = Guid.NewGuid().ToString();
    public string TableName { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = new();
    public IndexType Type { get; set; } = IndexType.BTree;
    public IndexPriority Priority { get; set; } = IndexPriority.Medium;
    public double EstimatedImpact { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public string SqlScript { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public bool IsImplemented { get; set; } = false;

    // Additional properties for compatibility
    public double ImpactScore { get; set; }
    public string CreateStatement { get; set; } = string.Empty;

    // Properties expected by Infrastructure services
    public List<string> ColumnNames { get; set; } = new(); // Alias for Columns
    public string IndexTypeString { get; set; } = string.Empty; // String version of Type
    public string Reason { get; set; } = string.Empty; // Alias for Reasoning
    public string EstimatedSize { get; set; } = string.Empty;
}

/// <summary>
/// Schema health analysis result
/// </summary>
public class SchemaHealthAnalysis
{
    public string AnalysisId { get; set; } = Guid.NewGuid().ToString();
    public double OverallHealthScore { get; set; }
    public List<SchemaIssue> Issues { get; set; } = new();
    public List<SchemaRecommendation> Recommendations { get; set; } = new();
    public SchemaHealthMetrics Metrics { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan AnalysisDuration { get; set; }

    // Additional properties for compatibility
    public TableHealthAnalysis? TableHealth { get; set; }
    public IndexHealthAnalysis? IndexHealth { get; set; }
    public RelationshipAnalysis? RelationshipHealth { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Schema issue identified during health analysis
/// </summary>
public class SchemaIssue
{
    public string IssueId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IssueSeverity Severity { get; set; } = IssueSeverity.Medium;
    public IssueCategory Category { get; set; } = IssueCategory.Performance;
    public string TableName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public List<string> SuggestedActions { get; set; } = new();
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Schema recommendation for improvements
/// </summary>
public class SchemaRecommendation
{
    public string RecommendationId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecommendationPriority Priority { get; set; } = RecommendationPriority.Medium;
    public RecommendationType Type { get; set; } = RecommendationType.Performance;
    public double EstimatedImpact { get; set; }
    public string Implementation { get; set; } = string.Empty;
    public List<string> Benefits { get; set; } = new();
    public List<string> Risks { get; set; } = new();
}

/// <summary>
/// Schema health metrics
/// </summary>
public class SchemaHealthMetrics
{
    public int TotalTables { get; set; }
    public int TotalColumns { get; set; }
    public int TotalIndexes { get; set; }
    public int MissingIndexes { get; set; }
    public int UnusedIndexes { get; set; }
    public double AverageTableSize { get; set; }
    public int NormalizationIssues { get; set; }
    public int PerformanceIssues { get; set; }
    public int SecurityIssues { get; set; }
}

/// <summary>
/// Execution plan analysis result
/// </summary>
public class ExecutionPlanAnalysis
{
    public string AnalysisId { get; set; } = Guid.NewGuid().ToString();
    public string QueryId { get; set; } = string.Empty;
    public double ComplexityScore { get; set; }
    public List<ExecutionPlanBottleneck> Bottlenecks { get; set; } = new();
    public List<OptimizationOpportunity> OptimizationOpportunities { get; set; } = new();
    public ExecutionPlanMetrics Metrics { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // Additional properties for compatibility
    public string Sql { get; set; } = string.Empty;
    public List<ExecutionStep> Steps { get; set; } = new();
    public ResourceUsageAnalysis? ResourceUsage { get; set; }
    public double EstimatedCost { get; set; }
}

/// <summary>
/// Execution plan bottleneck
/// </summary>
public class ExecutionPlanBottleneck
{
    public string BottleneckId { get; set; } = Guid.NewGuid().ToString();
    public string Operation { get; set; } = string.Empty;
    public double CostPercentage { get; set; }
    public string Description { get; set; } = string.Empty;
    public BottleneckSeverity Severity { get; set; } = BottleneckSeverity.Medium;
    public List<string> SuggestedFixes { get; set; } = new();
}

/// <summary>
/// Optimization opportunity in execution plan
/// </summary>
public class OptimizationOpportunity
{
    public string OpportunityId { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = string.Empty;
    public double EstimatedImprovement { get; set; }
    public OptimizationPriority Priority { get; set; } = OptimizationPriority.Medium;
    public string Implementation { get; set; } = string.Empty;
    public List<string> Benefits { get; set; } = new();

    // Additional properties for compatibility
    public string Type { get; set; } = string.Empty;
    public double PotentialImpact { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public List<string> RequiredActions { get; set; } = new();
    public double ImplementationComplexity { get; set; }
}

/// <summary>
/// Execution plan metrics
/// </summary>
public class ExecutionPlanMetrics
{
    public double TotalCost { get; set; }
    public int OperationCount { get; set; }
    public double EstimatedRows { get; set; }
    public double EstimatedExecutionTime { get; set; }
    public int IndexSeeks { get; set; }
    public int IndexScans { get; set; }
    public int TableScans { get; set; }
    public Dictionary<string, double> OperationCosts { get; set; } = new();
}

// Enumerations
public enum PerformanceGoalType
{
    ExecutionTime,
    Throughput,
    MemoryUsage,
    CpuUsage,
    ErrorRate
}

public enum PerformanceGoalPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum IndexType
{
    BTree,
    Hash,
    Bitmap,
    Clustered,
    NonClustered,
    Unique
}

public enum IndexPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum IssueSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum IssueCategory
{
    Performance,
    Security,
    Maintainability,
    Scalability,
    Reliability
}

public enum BottleneckSeverity
{
    Low,
    Medium,
    High,
    Critical
}

public enum OptimizationPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum OptimizationGoal
{
    Performance,
    Readability,
    Maintainability,
    Security,
    Scalability
}

/// <summary>
/// SQL optimization result
/// </summary>
public class SqlOptimizationResult
{
    public string OptimizedSql { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public List<OptimizationImprovement> Improvements { get; set; } = new();
    public TimeSpan EstimatedTimeSaving { get; set; }
    public string Reasoning { get; set; } = string.Empty;

    // Additional properties expected by Infrastructure layer
    public string OriginalSql { get; set; } = string.Empty;
    public List<SqlOptimization> Optimizations { get; set; } = new();
    public PerformanceComparison? PerformanceComparison { get; set; }
    public List<string> Warnings { get; set; } = new();

    // Properties expected by API controllers
    public double ImprovementScore { get; set; }
    public double EstimatedPerformanceGain { get; set; }
    public List<string> Recommendations { get; set; } = new();

    // Properties expected by Infrastructure services
    public double EstimatedSpeedup { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
}

/// <summary>
/// Optimization improvement
/// </summary>
public class OptimizationImprovement
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Impact { get; set; }
}

/// <summary>
/// Optimization goals
/// </summary>
public class OptimizationGoals
{
    public List<OptimizationGoal> Goals { get; set; } = new();
    public Dictionary<string, double> Weights { get; set; } = new();

    // Additional properties expected by Infrastructure layer
    public bool OptimizeForSpeed => Goals.Contains(OptimizationGoal.Performance);
}

/// <summary>
/// Query rewrite suggestion
/// </summary>
public class QueryRewrite
{
    public string OriginalQuery { get; set; } = string.Empty;
    public string RewrittenQuery { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
}

/// <summary>
/// Performance trend analysis
/// </summary>
public class PerformanceTrendAnalysis
{
    public string MetricName { get; set; } = string.Empty;
    public TrendDirection Direction { get; set; }
    public double TrendStrength { get; set; }
    public List<TrendDataPoint> DataPoints { get; set; } = new();
}

/// <summary>
/// Maintenance recommendation
/// </summary>
public class MaintenanceRecommendation
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MaintenancePriority Priority { get; set; }
    public string Action { get; set; } = string.Empty;
}

/// <summary>
/// Schema optimization metrics
/// </summary>
public class SchemaOptimizationMetrics
{
    public int TotalOptimizations { get; set; }
    public double AverageImprovementScore { get; set; }
    public Dictionary<string, int> OptimizationsByType { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // Additional properties for compatibility
    public int IndexSuggestionsGenerated { get; set; }
    public int IndexSuggestionsImplemented { get; set; }
    public double QueryPerformanceImprovement { get; set; }
    public Dictionary<string, int> OptimizationTypes { get; set; } = new();

    // Properties expected by Infrastructure services
    public int TotalTables { get; set; }
    public int OptimizedTables { get; set; }
    public int TotalIndexes { get; set; }
    public int RecommendedIndexes { get; set; }
    public double OverallScore { get; set; }
    public DateTime LastAnalyzed { get; set; } = DateTime.UtcNow;
    public string Details { get; set; } = string.Empty;
}

// DataPoint moved to VisualizationModels.cs to avoid duplicates

public enum MaintenancePriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Recommendation priority enumeration
/// </summary>
public enum RecommendationPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Recommendation type enumeration
/// </summary>
public enum RecommendationType
{
    Performance,
    Security,
    Maintainability,
    Scalability,
    Reliability,
    Index,
    Query,
    Schema,
    Clarification,
    Enhancement,
    Error
}

/// <summary>
/// Trend direction enumeration
/// </summary>
public enum TrendDirection
{
    Improving,
    Stable,
    Degrading,
    Unknown
}

/// <summary>
/// Performance prediction for optimization
/// </summary>
public class PerformancePrediction
{
    public double PredictedImprovement { get; set; }
    public TimeSpan EstimatedExecutionTime { get; set; }
    public double ConfidenceScore { get; set; }
    public List<string> Factors { get; set; } = new();

    // Additional properties for compatibility
    public double EstimatedSpeedup { get; set; }
    public double ConfidenceLevel { get; set; }
    public ResourceUsageEstimate? ResourceUsage { get; set; }
    public List<string> Assumptions { get; set; } = new();
}

/// <summary>
/// SQL optimization recommendation
/// </summary>
public class SqlOptimization
{
    public string OptimizationType { get; set; } = string.Empty;
    public string OriginalSql { get; set; } = string.Empty;
    public string OptimizedSql { get; set; } = string.Empty;
    public double ExpectedImprovement { get; set; }
    public string Reasoning { get; set; } = string.Empty;

    // Additional properties expected by Infrastructure layer
    public string? AfterCode { get; set; } // Alias for OptimizedSql
}

/// <summary>
/// Performance metric for tracking
/// </summary>
public class PerformanceMetric
{
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Performance comparison between optimizations
/// </summary>
public class PerformanceComparison
{
    public string ComparisonId { get; set; } = string.Empty;
    public List<PerformanceMetric> BeforeMetrics { get; set; } = new();
    public List<PerformanceMetric> AfterMetrics { get; set; } = new();
    public double ImprovementPercentage { get; set; }
}

/// <summary>
/// Table health analysis
/// </summary>
public class TableHealth
{
    public string TableName { get; set; } = string.Empty;
    public double HealthScore { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Execution step in query plan
/// </summary>
public class ExecutionStep
{
    public string StepId { get; set; } = string.Empty;
    public string Operation { get; set; } = string.Empty;
    public double Cost { get; set; }
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}

/// <summary>
/// Performance bottleneck identification
/// </summary>
public class PerformanceBottleneck
{
    public string BottleneckId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Impact { get; set; }
    public List<string> Solutions { get; set; } = new();
}

/// <summary>
/// Resource usage analysis
/// </summary>
public class ResourceUsageAnalysis
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double IoUsage { get; set; }
    public Dictionary<string, double> DetailedMetrics { get; set; } = new();
}

/// <summary>
/// Resource usage estimate for performance prediction
/// </summary>
public class ResourceUsageEstimate
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public double NetworkUsage { get; set; }
    public Dictionary<string, double> AdditionalMetrics { get; set; } = new();
}

/// <summary>
/// Estimated impact of an optimization
/// </summary>
public class EstimatedImpact
{
    public double PerformanceImprovement { get; set; }
    public double StorageOverhead { get; set; }
    public double MaintenanceOverhead { get; set; }
    public double ConfidenceLevel { get; set; } = 0.8;
    public List<string> Benefits { get; set; } = new();
    public List<string> Risks { get; set; } = new();
}

/// <summary>
/// Table health analysis result
/// </summary>
public class TableHealthAnalysis
{
    public double OverallScore { get; set; }
    public Dictionary<string, TableHealth> Tables { get; set; } = new();
    public List<string> ProblematicTables { get; set; } = new();
    public List<SchemaIssue> Issues { get; set; } = new();
}

/// <summary>
/// Index health analysis result
/// </summary>
public class IndexHealthAnalysis
{
    public double OverallEfficiency { get; set; }
    public List<string> UnusedIndexes { get; set; } = new();
    public List<string> MissingIndexes { get; set; } = new();
    public List<SchemaIssue> Issues { get; set; } = new();
}

/// <summary>
/// Relationship analysis result
/// </summary>
public class RelationshipAnalysis
{
    public double IntegrityScore { get; set; }
    public List<SchemaIssue> Issues { get; set; } = new();
    public List<string> MissingRelationships { get; set; } = new();
    public List<string> BrokenRelationships { get; set; } = new();
}

/// <summary>
/// Plan optimization result
/// </summary>
public class PlanOptimization
{
    public string OptimizationId { get; set; } = string.Empty;
    public string OriginalPlan { get; set; } = string.Empty;
    public string OptimizedPlan { get; set; } = string.Empty;
    public double ImprovementScore { get; set; }
    public List<string> Changes { get; set; } = new();
}

/// <summary>
/// Comprehensive error analysis data (consolidated from multiple duplicates)
/// </summary>
public class ErrorAnalysis
{
    // Basic error metrics (from MultiModalDashboards)
    public int TotalErrors { get; set; }
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    public List<string> CommonErrorMessages { get; set; } = new();

    // LLM-specific properties (from ILLMManagementService)
    public string ProviderId { get; set; } = string.Empty;
    public double ErrorRate { get; set; }
    public Dictionary<string, int> ErrorsByModel { get; set; } = new();

    // Analysis metadata
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan AnalysisWindow { get; set; }
    public string AnalysisType { get; set; } = "General"; // General, LLM, Query, etc.
}

// TrendDataPoint class already defined above - duplicate removed

// =============================================================================
// MISSING STATISTICS AND BUSINESS MODELS
// =============================================================================

/// <summary>
/// Business table statistics
/// </summary>
public class BusinessTableStatistics
{
    public string TableId { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public int TotalRecords { get; set; }
    public int TotalColumns { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public double AverageQueryTime { get; set; }
    public int QueryCount { get; set; }
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
}

/// <summary>
/// Glossary statistics
/// </summary>
public class GlossaryStatistics
{
    public int TotalTerms { get; set; }
    public int TotalCategories { get; set; }
    public int RecentlyAdded { get; set; }
    public int RecentlyUpdated { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public Dictionary<string, int> TermsByCategory { get; set; } = new();
}

/// <summary>
/// Query pattern statistics
/// </summary>
public class QueryPatternStatistics
{
    public int TotalPatterns { get; set; }
    public int ActivePatterns { get; set; }
    public double AverageConfidence { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public Dictionary<string, int> PatternsByType { get; set; } = new();
}

/// <summary>
/// Cost alert for LLM management
/// </summary>
public class CostAlert
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string AlertId { get; set; } = Guid.NewGuid().ToString();
    public string ProviderId { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty;
    public double Threshold { get; set; }
    public double ThresholdAmount { get; set; }
    public double CurrentValue { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsResolved { get; set; } = false;
    public bool IsEnabled { get; set; } = true;
}

// =============================================================================
// MISSING MODELS FOR INFRASTRUCTURE SERVICES
// =============================================================================

/// <summary>
/// Tuning result
/// </summary>
public class TuningResult
{
    public string TuningId { get; set; } = Guid.NewGuid().ToString();
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Results { get; set; } = new();
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    // Properties expected by Infrastructure services
    public string Status { get; set; } = "Pending";
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public List<TuningImprovement> Improvements { get; set; } = new();
    public TuningMetrics Metrics { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Tuning request
/// </summary>
public class TuningRequest
{
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Tuning status
/// </summary>
public class TuningStatus
{
    public string TuningId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double Progress { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Properties expected by Infrastructure services
    /// <summary>
    /// Current step in the tuning process
    /// </summary>
    public string CurrentStep { get; set; } = string.Empty;

    /// <summary>
    /// Estimated time remaining for completion
    /// </summary>
    public TimeSpan EstimatedTimeRemaining { get; set; }

    /// <summary>
    /// Error message if status is error
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Tuning improvement
/// </summary>
public class TuningImprovement
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Impact { get; set; }
}

/// <summary>
/// Tuning metrics
/// </summary>
public class TuningMetrics
{
    public double PerformanceGain { get; set; }
    public double AccuracyImprovement { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

// =============================================================================
// MISSING AUTHENTICATION MODELS
// =============================================================================

/// <summary>
/// Refresh token request
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// User session information
/// </summary>
public class UserSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Property expected by Infrastructure services
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// MFA challenge
/// </summary>
public class MfaChallenge
{
    public string ChallengeId { get; set; } = Guid.NewGuid().ToString();
    public string Method { get; set; } = string.Empty;
    public string Challenge { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5);

    // Properties expected by Infrastructure services
    /// <summary>
    /// User ID for the challenge
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Whether the challenge has been used
    /// </summary>
    public bool IsUsed { get; set; } = false;
}

/// <summary>
/// MFA validation request
/// </summary>
public class MfaValidationRequest
{
    public string ChallengeId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

/// <summary>
/// MFA validation result
/// </summary>
public class MfaValidationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
}

// =============================================================================
// MISSING INTERFACE SUPPORT MODELS
// =============================================================================

/// <summary>
/// Refresh token information
/// </summary>
public class RefreshTokenInfo
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; } = false;
}

// TableMetadata, SemanticAnalysisResult, QueryClassificationResult, and QueryOptimizationResult
// are already defined in other model files - removed duplicates

/// <summary>
/// AI service metrics (Core interface version)
/// </summary>
public class AIServiceMetrics
{
    public bool IsAvailable { get; set; }
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double AverageResponseTime { get; set; }
    public DateTime LastHealthCheck { get; set; } = DateTime.UtcNow;
    public string ProviderName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// MFA challenge result
/// </summary>
public class MfaChallengeResult
{
    public string ChallengeId { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public string Challenge { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddMinutes(5);
    public bool Success { get; set; }

    // Properties expected by Infrastructure services
    public string? ErrorMessage { get; set; }
    public string Method { get; set; } = string.Empty;
    public string? MaskedDeliveryAddress { get; set; }
}

/// <summary>
/// Create user request
/// </summary>
public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}

// =============================================================================
// MISSING ENTITY MODELS
// =============================================================================

// UserEntity has been consolidated into Infrastructure.Data.Entities.UserEntity
// See: Infrastructure/Data/Entities/BaseEntity.cs

// RefreshTokenEntity has been consolidated into Infrastructure.Data.Entities.RefreshTokenEntity
// See: Infrastructure/Data/Entities/BaseEntity.cs

// MfaChallengeEntity has been consolidated into Infrastructure.Data.Entities.MfaChallengeEntity
// See: Infrastructure/Data/Entities/BaseEntity.cs

// UserSessionEntity has been consolidated into Infrastructure.Data.Entities.UserSessionEntity
// See: Infrastructure/Data/Entities/BaseEntity.cs

/// <summary>
/// User preferences entity
/// </summary>
public class UserPreferencesEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, object> Preferences { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Audit log entity
/// </summary>
public class AuditLogEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}


