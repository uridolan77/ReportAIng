namespace BIReportingCopilot.Core.DTOs;

/// <summary>
/// Optimized DTO for query patterns with minimal data loading
/// </summary>
public class QueryPatternOptimizedDto
{
    public long Id { get; set; }
    public string PatternName { get; set; } = string.Empty;
    public string NaturalLanguagePattern { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Priority { get; set; }
    public int UsageCount { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
}

/// <summary>
/// Optimized dashboard data with pre-calculated aggregations
/// </summary>
public class OptimizedTuningDashboardData
{
    public int TotalTables { get; set; }
    public int TotalColumns { get; set; }
    public int TotalPatterns { get; set; }
    public int TotalGlossaryTerms { get; set; }
    public int ActivePromptTemplates { get; set; }
    public List<string> RecentlyUpdatedTables { get; set; } = new();
    public List<string> MostUsedPatterns { get; set; } = new();
    public Dictionary<string, int> PatternUsageStats { get; set; } = new();
    public DateTime LastRefreshed { get; set; } = DateTime.UtcNow;

    // Additional performance metrics
    public TimeSpan QueryExecutionTime { get; set; }
    public int CacheHitCount { get; set; }
}

/// <summary>
/// Batch operation result for performance tracking
/// </summary>
public class BatchOperationResult<T>
{
    public List<T> Results { get; set; } = new();
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
    public TimeSpan ExecutionTime { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Streaming data result for large datasets
/// </summary>
public class StreamingDataResult<T>
{
    public IAsyncEnumerable<T> Data { get; set; } = null!;
    public int? TotalCount { get; set; }
    public int BatchSize { get; set; } = 1000;
    public bool HasMore { get; set; }
}

/// <summary>
/// Performance metrics for query operations
/// </summary>
public class QueryPerformanceMetrics
{
    public TimeSpan ExecutionTime { get; set; }
    public int RowsAffected { get; set; }
    public long MemoryUsed { get; set; }
    public bool FromCache { get; set; }
    public string QueryHash { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    // Additional properties from Audit.cs version
    public long LogicalReads { get; set; }
    public long PhysicalReads { get; set; }
    public double CpuTime { get; set; }
    public string PerformanceLevel { get; set; } = "Good"; // Good, Fair, Poor
    public List<string> OptimizationSuggestions { get; set; } = new();

    // Additional properties expected by Infrastructure services
    public double ExecutionTimeMs => ExecutionTime.TotalMilliseconds;
    public int RowCount => RowsAffected;
}

/// <summary>
/// Optimized column info for reduced memory footprint
/// </summary>
public class BusinessColumnInfoOptimizedDto
{
    public long Id { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public string BusinessMeaning { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsKeyColumn { get; set; }
    public bool IsActive { get; set; }
    public long TableId { get; set; }
}

/// <summary>
/// Aggregated statistics for dashboard performance
/// </summary>
public class AggregatedStatistics
{
    public Dictionary<string, int> TablesBySchema { get; set; } = new();
    public Dictionary<string, int> ColumnsByDataType { get; set; } = new();
    public Dictionary<int, int> PatternsByPriority { get; set; } = new();
    public Dictionary<string, int> UsageByTimeframe { get; set; } = new();
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Business column info DTO
/// </summary>
public class BusinessColumnInfoDto
{
    public long Id { get; set; }
    public long TableInfoId { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public string BusinessMeaning { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public string DataExamples { get; set; } = string.Empty;
    public string ValidationRules { get; set; } = string.Empty;
    public string NaturalLanguageAliases { get; set; } = string.Empty;
    public string ValueExamples { get; set; } = string.Empty;
    public string DataLineage { get; set; } = string.Empty;
    public string CalculationRules { get; set; } = string.Empty;
    public string SemanticTags { get; set; } = string.Empty;
    public string BusinessDataType { get; set; } = string.Empty;
    public string ConstraintsAndRules { get; set; } = string.Empty;
    public double DataQualityScore { get; set; } = 5.0;
    public int UsageFrequency { get; set; } = 0;
    public string PreferredAggregation { get; set; } = string.Empty;
    public string RelatedBusinessTerms { get; set; } = string.Empty;
    public bool IsKeyColumn { get; set; }
    public bool IsSensitiveData { get; set; }
    public bool IsCalculatedField { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string UpdatedBy { get; set; } = string.Empty;

    // Additional semantic and AI-related fields
    public string SemanticContext { get; set; } = string.Empty;
    public string ConceptualRelationships { get; set; } = string.Empty;
    public string DomainSpecificTerms { get; set; } = string.Empty;
    public string QueryIntentMapping { get; set; } = string.Empty;
    public string BusinessQuestionTypes { get; set; } = string.Empty;
    public string SemanticSynonyms { get; set; } = string.Empty;
    public string AnalyticalContext { get; set; } = string.Empty;
    public string BusinessMetrics { get; set; } = string.Empty;
    public double SemanticRelevanceScore { get; set; } = 0.5;
    public string LLMPromptHints { get; set; } = string.Empty;
    public string VectorSearchTags { get; set; } = string.Empty;
    public string BusinessPurpose { get; set; } = string.Empty;
    public string BusinessFriendlyName { get; set; } = string.Empty;
    public string NaturalLanguageDescription { get; set; } = string.Empty;
    public string BusinessRules { get; set; } = string.Empty;
    public string RelationshipContext { get; set; } = string.Empty;
    public string DataGovernanceLevel { get; set; } = string.Empty;
    public DateTime? LastBusinessReview { get; set; }
    public double ImportanceScore { get; set; } = 0.5;
}
