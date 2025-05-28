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
    public string Sql { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<SqlCandidate> AlternativeQueries { get; set; } = new();
    public List<SemanticEntity> SemanticEntities { get; set; } = new();
    public QueryClassification Classification { get; set; } = new();
    public SchemaContext UsedSchema { get; set; } = new();
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
