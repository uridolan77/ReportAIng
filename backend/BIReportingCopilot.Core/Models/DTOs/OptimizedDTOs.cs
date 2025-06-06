namespace BIReportingCopilot.Core.Models.DTOs;

/// <summary>
/// Optimized DTO for business table info with projection to avoid loading unnecessary data
/// </summary>
public class BusinessTableInfoOptimizedDto
{
    public long Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string BusinessPurpose { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public int ColumnCount { get; set; }
    public DateTime CreatedDate { get; set; }
}

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
