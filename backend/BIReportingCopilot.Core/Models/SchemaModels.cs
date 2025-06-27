using System.ComponentModel.DataAnnotations;
using BIReportingCopilot.Core.Models.BusinessContext;

namespace BIReportingCopilot.Core.Models;

// SchemaMetadata, TableMetadata, and ViewMetadata already exist in other files - removed duplicates

/// <summary>
/// Procedure metadata model
/// </summary>
public class ProcedureMetadata
{
    public string ProcedureName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string FullName => $"{SchemaName}.{ProcedureName}";
    public List<ParameterMetadata> Parameters { get; set; } = new();
    public string Definition { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Function metadata model
/// </summary>
public class FunctionMetadata
{
    public string FunctionName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string FullName => $"{SchemaName}.{FunctionName}";
    public List<ParameterMetadata> Parameters { get; set; } = new();
    public string ReturnType { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastModified { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
}

// IndexMetadata already exists in other files - removed duplicate

/// <summary>
/// Foreign key metadata model
/// </summary>
public class ForeignKeyMetadata
{
    public string ConstraintName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = new();
    public string ReferencedTableName { get; set; } = string.Empty;
    public List<string> ReferencedColumns { get; set; } = new();
    public string OnDeleteAction { get; set; } = string.Empty;
    public string OnUpdateAction { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Primary key metadata model
/// </summary>
public class PrimaryKeyMetadata
{
    public string ConstraintName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<string> Columns { get; set; } = new();
    public bool IsClustered { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Parameter metadata model
/// </summary>
public class ParameterMetadata
{
    public string ParameterName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public int? MaxLength { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public bool IsNullable { get; set; }
    public string? DefaultValue { get; set; }
    public ParameterDirection Direction { get; set; } = ParameterDirection.Input;
    public Dictionary<string, object> Properties { get; set; } = new();
}

// SchemaSuggestion and DataQualityScore already exist in other files - removed duplicates

/// <summary>
/// Quality issue model
/// </summary>
public class QualityIssue
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public QualityIssueSeverity Severity { get; set; }
    public int AffectedRows { get; set; }
    public double ImpactScore { get; set; }
    public string? Recommendation { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}

/// <summary>
/// Parameter direction enumeration
/// </summary>
public enum ParameterDirection
{
    Input,
    Output,
    InputOutput,
    ReturnValue
}

/// <summary>
/// Suggestion priority enumeration
/// </summary>
public enum SuggestionPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Quality issue severity enumeration
/// </summary>
public enum QualityIssueSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// Represents a foreign key relationship between database tables
/// </summary>
public class ForeignKeyRelationship
{
    public string ConstraintName { get; set; } = string.Empty;
    public string ParentTable { get; set; } = string.Empty;
    public string ParentColumn { get; set; } = string.Empty;
    public string ReferencedTable { get; set; } = string.Empty;
    public string ReferencedColumn { get; set; } = string.Empty;
    public string DeleteAction { get; set; } = string.Empty;
    public string UpdateAction { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public string RelationshipType { get; set; } = string.Empty;
}

/// <summary>
/// Represents information about a table related to another table
/// </summary>
public class RelatedTableInfo
{
    public string TableName { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
    public string RelationshipType { get; set; } = string.Empty;
    public string JoinColumn { get; set; } = string.Empty;
    public string ReferencedColumn { get; set; } = string.Empty;
    public int Distance { get; set; }
    public string RelationshipDirection { get; set; } = string.Empty; // "incoming" or "outgoing"
    public string ConstraintName { get; set; } = string.Empty;
}

/// <summary>
/// Represents an optimal join path between tables
/// </summary>
public class JoinPath
{
    public string FromTable { get; set; } = string.Empty;
    public string ToTable { get; set; } = string.Empty;
    public List<JoinCondition> JoinConditions { get; set; } = new();
    public int PathLength { get; set; }
    public bool IsOptimal { get; set; }
    public double PerformanceScore { get; set; }
}

/// <summary>
/// Represents a join condition between two tables
/// </summary>
public class JoinCondition
{
    public string LeftTable { get; set; } = string.Empty;
    public string LeftColumn { get; set; } = string.Empty;
    public string RightTable { get; set; } = string.Empty;
    public string RightColumn { get; set; } = string.Empty;
    public string Operator { get; set; } = "=";
}

/// <summary>
/// Result of SQL JOIN generation
/// </summary>
public class SqlJoinResult
{
    public bool Success { get; set; }
    public string JoinClause { get; set; } = string.Empty;
    public Dictionary<string, string> TableAliases { get; set; } = new();
    public List<JoinPath> JoinPaths { get; set; } = new();
    public string? PrimaryTable { get; set; }
    public JoinStrategy JoinStrategy { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Result of join validation
/// </summary>
public class JoinValidationResult
{
    public bool IsValid { get; set; }
    public List<string> ConnectedTables { get; set; } = new();
    public List<string> IsolatedTables { get; set; } = new();
    public int AvailableRelationships { get; set; }
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Strategy for generating JOINs
/// </summary>
public enum JoinStrategy
{
    /// <summary>
    /// Optimal strategy based on performance and relationships
    /// </summary>
    Optimal,

    /// <summary>
    /// Use LEFT JOINs to preserve all records from primary table
    /// </summary>
    LeftJoin,

    /// <summary>
    /// Use INNER JOINs for best performance
    /// </summary>
    InnerJoin,

    /// <summary>
    /// Use minimal path with shortest join chains
    /// </summary>
    MinimalPath
}

/// <summary>
/// Result of SQL date filter generation
/// </summary>
public class SqlDateFilterResult
{
    public bool Success { get; set; }
    public string WhereClause { get; set; } = string.Empty;
    public List<string> DateColumns { get; set; } = new();
    public DateFilterStrategy Strategy { get; set; }
    public TimeRange? TimeRange { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Result of date column validation
/// </summary>
public class DateColumnValidationResult
{
    public bool IsValid { get; set; }
    public List<string> ValidColumns { get; set; } = new();
    public List<string> InvalidColumns { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? RecommendedColumn { get; set; }
}

/// <summary>
/// Strategy for generating date filters
/// </summary>
public enum DateFilterStrategy
{
    /// <summary>
    /// Optimal strategy balancing performance and accuracy
    /// </summary>
    Optimal,

    /// <summary>
    /// Inclusive filtering that includes boundary dates
    /// </summary>
    Inclusive,

    /// <summary>
    /// Exclusive filtering that excludes boundary dates
    /// </summary>
    Exclusive,

    /// <summary>
    /// Performance-optimized filtering for large datasets
    /// </summary>
    Performance
}

/// <summary>
/// SQL metric for aggregation
/// </summary>
public class SqlMetric
{
    public string ColumnName { get; set; } = string.Empty;
    public string Function { get; set; } = string.Empty; // SUM, COUNT, AVG, MAX, MIN
    public string Alias { get; set; } = string.Empty;
    public double Priority { get; set; }
}

/// <summary>
/// SQL dimension for grouping
/// </summary>
public class SqlDimension
{
    public string ColumnName { get; set; } = string.Empty;
    public string Alias { get; set; } = string.Empty;
    public double Priority { get; set; }
}

/// <summary>
/// Result of SQL aggregation generation
/// </summary>
public class SqlAggregationResult
{
    public bool Success { get; set; }
    public string SelectClause { get; set; } = string.Empty;
    public string GroupByClause { get; set; } = string.Empty;
    public string OrderByClause { get; set; } = string.Empty;
    public List<SqlMetric> Metrics { get; set; } = new();
    public List<SqlDimension> Dimensions { get; set; } = new();
    public AggregationStrategy Strategy { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Result of aggregation validation
/// </summary>
public class AggregationValidationResult
{
    public bool IsValid { get; set; }
    public List<SqlMetric> ValidMetrics { get; set; } = new();
    public List<SqlMetric> InvalidMetrics { get; set; } = new();
    public List<SqlDimension> ValidDimensions { get; set; } = new();
    public List<SqlDimension> InvalidDimensions { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Strategy for generating aggregations
/// </summary>
public enum AggregationStrategy
{
    /// <summary>
    /// Optimal strategy balancing performance and completeness
    /// </summary>
    Optimal,

    /// <summary>
    /// Performance-focused strategy for large datasets
    /// </summary>
    Performance,

    /// <summary>
    /// Comprehensive strategy with detailed grouping
    /// </summary>
    Comprehensive,

    /// <summary>
    /// Simple strategy with minimal aggregation
    /// </summary>
    Simple
}

/// <summary>
/// Result of enhanced query processing
/// </summary>
public class EnhancedQueryResult
{
    public bool Success { get; set; }
    public string GeneratedSql { get; set; } = string.Empty;
    public BusinessContext.BusinessContextProfile? BusinessProfile { get; set; }
    public SqlJoinResult? JoinResult { get; set; }
    public SqlDateFilterResult? DateFilterResult { get; set; }
    public SqlAggregationResult? AggregationResult { get; set; }
    public double OverallConfidence { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, object> ProcessingMetadata { get; set; } = new();
}
