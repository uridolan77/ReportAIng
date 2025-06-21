using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models.BusinessContext;

/// <summary>
/// Represents the analyzed business context of a user's natural language question
/// </summary>
public class BusinessContextProfile
{
    public string OriginalQuestion { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public QueryIntent Intent { get; set; } = new();
    public BusinessDomain Domain { get; set; } = new();
    public List<BusinessEntity> Entities { get; set; } = new();
    public List<string> BusinessTerms { get; set; } = new();
    public Dictionary<string, double> TermRelevanceScores { get; set; } = new();
    public double ConfidenceScore { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    // Additional context for enhanced analysis
    public List<string> IdentifiedMetrics { get; set; } = new();
    public List<string> IdentifiedDimensions { get; set; } = new();
    public TimeRange? TimeContext { get; set; }
    public List<string> ComparisonTerms { get; set; } = new();
}

/// <summary>
/// Represents the classified intent of a business query
/// </summary>
public class QueryIntent
{
    public IntentType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public List<string> SubIntents { get; set; } = new();
}

/// <summary>
/// Types of business query intents
/// </summary>
public enum IntentType
{
    Analytical,      // Complex analysis queries
    Operational,     // Transactional/operational queries
    Exploratory,     // Discovery queries
    Comparison,      // Comparative analysis
    Aggregation,     // Summary/rollup queries
    Trend,          // Time-series analysis
    Detail,         // Drill-down queries
    Unknown
}

/// <summary>
/// Represents a business domain context
/// </summary>
public class BusinessDomain
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> RelatedTables { get; set; } = new();
    public List<string> KeyConcepts { get; set; } = new();
    public double RelevanceScore { get; set; }
}

/// <summary>
/// Represents a business entity extracted from user query
/// </summary>
public class BusinessEntity
{
    public string Name { get; set; } = string.Empty;
    public EntityType Type { get; set; }
    public string OriginalText { get; set; } = string.Empty;
    public string MappedTableName { get; set; } = string.Empty;
    public string MappedColumnName { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Types of business entities that can be extracted
/// </summary>
public enum EntityType
{
    Table,
    Column,
    Metric,
    Dimension,
    GlossaryTerm,
    TimeReference,
    ComparisonValue,
    Unknown
}

/// <summary>
/// Represents a time range context in business queries
/// </summary>
public class TimeRange
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string RelativeExpression { get; set; } = string.Empty; // e.g., "last month", "year to date"
    public TimeGranularity Granularity { get; set; }
}

/// <summary>
/// Time granularity levels for analysis
/// </summary>
public enum TimeGranularity
{
    Hour,
    Day,
    Week,
    Month,
    Quarter,
    Year,
    Unknown
}
