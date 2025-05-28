namespace BIReportingCopilot.Core.Models;

// Semantic Analysis Models
public class SemanticAnalysis
{
    public string OriginalQuery { get; set; } = string.Empty;
    public QueryIntent Intent { get; set; }
    public List<SemanticEntity> Entities { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public double ConfidenceScore { get; set; }
    public string ProcessedQuery { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class SemanticEntity
{
    public string Text { get; set; } = string.Empty;
    public EntityType Type { get; set; }
    public double Confidence { get; set; }
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public string? ResolvedValue { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class SemanticSimilarity
{
    public string Query1 { get; set; } = string.Empty;
    public string Query2 { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public List<string> CommonEntities { get; set; } = new();
    public List<string> CommonKeywords { get; set; } = new();
}

// Query Classification Models
public class QueryClassification
{
    public QueryCategory Category { get; set; }
    public QueryComplexity Complexity { get; set; }
    public List<string> RequiredJoins { get; set; } = new();
    public List<string> PredictedTables { get; set; } = new();
    public TimeSpan EstimatedExecutionTime { get; set; }
    public VisualizationType RecommendedVisualization { get; set; }
    public double ConfidenceScore { get; set; }
    public List<string> OptimizationSuggestions { get; set; } = new();
}

public class QueryComplexityScore
{
    public QueryComplexity Level { get; set; }
    public int Score { get; set; }
    public List<ComplexityFactor> Factors { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

public class ComplexityFactor
{
    public string Name { get; set; } = string.Empty;
    public int Impact { get; set; }
    public string Description { get; set; } = string.Empty;
}

// Context Management Models
public class UserContext
{
    public string UserId { get; set; } = string.Empty;
    public List<string> PreferredTables { get; set; } = new();
    public List<string> CommonFilters { get; set; } = new();
    public Dictionary<string, object> Preferences { get; set; } = new();
    public List<QueryPattern> RecentPatterns { get; set; } = new();
    public DateTime LastUpdated { get; set; }
    public string Domain { get; set; } = string.Empty; // e.g., "sales", "marketing", "finance"
}

public class QueryPattern
{
    public string Pattern { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public DateTime LastUsed { get; set; }
    public List<string> AssociatedTables { get; set; } = new();
    public QueryIntent Intent { get; set; }
}

public class SchemaContext
{
    public List<TableMetadata> RelevantTables { get; set; } = new();
    public List<RelationshipInfo> Relationships { get; set; } = new();
    public List<string> SuggestedJoins { get; set; } = new();
    public Dictionary<string, string> ColumnMappings { get; set; } = new();
    public List<string> BusinessTerms { get; set; } = new();
}

// Query Optimization Models
public class OptimizedQuery
{
    public string Sql { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public List<SqlCandidate> Alternatives { get; set; } = new();
    public List<string> OptimizationApplied { get; set; } = new();
    public QueryPerformancePrediction PerformancePrediction { get; set; } = new();
}

public class SqlCandidate
{
    public string Sql { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public QueryPerformancePrediction Performance { get; set; } = new();
}

public class QueryPerformancePrediction
{
    public TimeSpan EstimatedExecutionTime { get; set; }
    public int EstimatedRowCount { get; set; }
    public double ComplexityScore { get; set; }
    public List<string> PerformanceWarnings { get; set; } = new();
    public List<string> IndexRecommendations { get; set; } = new();
}

// Enhanced Processing Models
public class ProcessedQuery
{
    public string OriginalQuery { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public QueryContext? Context { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
    public string GeneratedSQL { get; set; } = string.Empty;
    public List<ProcessedQuery> SimilarQueries { get; set; } = new();

    // Legacy properties for backward compatibility
    public string Sql { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<SqlCandidate> AlternativeQueries { get; set; } = new();
    public List<SemanticEntity> SemanticEntities { get; set; } = new();
    public QueryClassification Classification { get; set; } = new();
    public SchemaContext UsedSchema { get; set; } = new();

    // Advanced processing properties
    public string? QueryType { get; set; }
    public string? Complexity { get; set; }
    public List<string> SemanticTokens { get; set; } = new();
    public string? Intent { get; set; }
    public List<string> Entities { get; set; } = new();
}

// Enums
public enum EntityType
{
    Table,
    Column,
    Value,
    DateRange,
    Number,
    Aggregation,
    Condition,
    Sort,
    Limit
}

public enum QueryCategory
{
    Reporting,
    Analytics,
    Lookup,
    Aggregation,
    Trend,
    Comparison,
    Filtering,
    Export
}

public enum QueryIntent
{
    General,
    Aggregation,
    Trend,
    Comparison,
    Filtering
}

public enum QueryComplexity
{
    Low,
    Medium,
    High
}

public enum VisualizationType
{
    Table,
    BarChart,
    LineChart,
    PieChart,
    ScatterPlot,
    Heatmap,
    Gauge,
    KPI
}

// Relationship Models
public class RelationshipInfo
{
    public string FromTable { get; set; } = string.Empty;
    public string ToTable { get; set; } = string.Empty;
    public string FromColumn { get; set; } = string.Empty;
    public string ToColumn { get; set; } = string.Empty;
    public RelationshipType Type { get; set; }
    public double Confidence { get; set; }
}

public enum RelationshipType
{
    OneToOne,
    OneToMany,
    ManyToOne,
    ManyToMany
}

// Query Context Models (moved from IStreamingOpenAIService.cs for better organization)
/// <summary>
/// Query context for enhanced prompt engineering
/// </summary>
public class QueryContext
{
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public List<string> PreviousQueries { get; set; } = new();
    public Dictionary<string, object> UserPreferences { get; set; } = new();
    public string? BusinessDomain { get; set; }
    public StreamingQueryComplexity PreferredComplexity { get; set; } = StreamingQueryComplexity.Medium;
}

/// <summary>
/// Analysis context for insight generation
/// </summary>
public class AnalysisContext
{
    public string? BusinessGoal { get; set; }
    public string? TimeFrame { get; set; }
    public List<string> KeyMetrics { get; set; } = new();
    public string? ComparisonPeriod { get; set; }
    public AnalysisType Type { get; set; } = AnalysisType.Descriptive;
}

/// <summary>
/// Query complexity levels for streaming AI
/// </summary>
public enum StreamingQueryComplexity
{
    Simple,
    Medium,
    Complex,
    Expert
}

/// <summary>
/// Analysis types for insights
/// </summary>
public enum AnalysisType
{
    Descriptive,
    Diagnostic,
    Predictive,
    Prescriptive
}

// Streaming Response Models (moved from IStreamingOpenAIService.cs)
/// <summary>
/// Streaming response from AI service
/// </summary>
public class StreamingResponse
{
    public StreamingResponseType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public int ChunkIndex { get; set; }
    public object? Metadata { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Types of streaming responses
/// </summary>
public enum StreamingResponseType
{
    SQLGeneration,
    Insight,
    Explanation,
    Error,
    Progress
}

// Query Example Models
/// <summary>
/// Example query for training and prompt engineering
/// </summary>
public class QueryExample
{
    public string NaturalLanguage { get; set; } = string.Empty;
    public string SQL { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; } = 1.0;
    public List<string> Tags { get; set; } = new();
}
