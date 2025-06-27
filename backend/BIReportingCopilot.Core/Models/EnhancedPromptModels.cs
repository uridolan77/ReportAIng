namespace BIReportingCopilot.Core.Models.Enhanced;

/// <summary>
/// Business profile for enhanced prompt generation
/// </summary>
public class BusinessProfile
{
    public BusinessDomain Domain { get; set; } = new();
    public BusinessIntent Intent { get; set; } = new();
    public List<BusinessEntity> Entities { get; set; } = new();
}

/// <summary>
/// Business domain information
/// </summary>
public class BusinessDomain
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> KeyConcepts { get; set; } = new();
}

/// <summary>
/// Business intent information
/// </summary>
public class BusinessIntent
{
    public string Type { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string? Description { get; set; }
    public List<string> RequiredAggregations { get; set; } = new();
}

/// <summary>
/// Business entity information
/// </summary>
public class BusinessEntity
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Value { get; set; }
    public double? Confidence { get; set; }
}

/// <summary>
/// Contextual schema for enhanced prompt generation
/// </summary>
public class ContextualSchema
{
    public List<RelevantTable> RelevantTables { get; set; } = new();
    public List<GlossaryTerm> GlossaryTerms { get; set; } = new();
}

/// <summary>
/// Relevant table information
/// </summary>
public class RelevantTable
{
    public string TableName { get; set; } = string.Empty;
    public string? BusinessPurpose { get; set; }
    public double RelevanceScore { get; set; }
    public List<ColumnInfo> Columns { get; set; } = new();
}

/// <summary>
/// Column information for enhanced prompts
/// </summary>
public class ColumnInfo
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? BusinessFriendlyName { get; set; }
    public string? NaturalLanguageDescription { get; set; }
    public string? ValueExamples { get; set; }
    public string? ConstraintsAndRules { get; set; }
}

/// <summary>
/// Glossary term information
/// </summary>
public class GlossaryTerm
{
    public string Term { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public string? Category { get; set; }
}
