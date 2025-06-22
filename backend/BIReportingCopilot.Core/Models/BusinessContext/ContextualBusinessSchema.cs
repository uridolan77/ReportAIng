using BIReportingCopilot.Core.DTOs;

namespace BIReportingCopilot.Core.Models.BusinessContext;

/// <summary>
/// Represents business schema context relevant to a specific query
/// </summary>
public class ContextualBusinessSchema
{
    public List<BusinessTableInfoDto> RelevantTables { get; set; } = new();
    public Dictionary<long, List<BusinessColumnInfo>> TableColumns { get; set; } = new();
    public List<BusinessGlossaryDto> RelevantGlossaryTerms { get; set; } = new();
    public List<BusinessRule> BusinessRules { get; set; } = new();
    public List<TableRelationship> TableRelationships { get; set; } = new();
    public List<RelevantQueryExample> RelevantExamples { get; set; } = new();
    public Dictionary<string, string> SemanticMappings { get; set; } = new();
    public double RelevanceScore { get; set; }
    public SchemaComplexity Complexity { get; set; }
    
    // Performance hints
    public List<string> SuggestedIndexes { get; set; } = new();
    public List<string> PartitioningHints { get; set; } = new();
}

/// <summary>
/// Represents a business rule that applies to the schema
/// </summary>
public class BusinessRule
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RuleType Type { get; set; }
    public string SqlExpression { get; set; } = string.Empty;
    public List<string> AffectedColumns { get; set; } = new();
    public int Priority { get; set; }
    public double RelevanceScore { get; set; }
}

/// <summary>
/// Types of business rules
/// </summary>
public enum RuleType
{
    Validation,
    Calculation,
    Filter,
    Aggregation,
    Security
}

/// <summary>
/// Represents a relationship between tables
/// </summary>
public class TableRelationship
{
    public string FromTable { get; set; } = string.Empty;
    public string ToTable { get; set; } = string.Empty;
    public string FromColumn { get; set; } = string.Empty;
    public string ToColumn { get; set; } = string.Empty;
    public RelationshipType Type { get; set; }
    public string BusinessMeaning { get; set; } = string.Empty;
}

/// <summary>
/// Types of table relationships
/// </summary>
public enum RelationshipType
{
    OneToOne,
    OneToMany,
    ManyToMany
}

/// <summary>
/// Schema complexity levels
/// </summary>
public enum SchemaComplexity
{
    Simple,      // Single table
    Moderate,    // 2-3 tables with simple joins
    Complex,     // Multiple tables with complex relationships
    VeryComplex  // Many tables with nested relationships
}
