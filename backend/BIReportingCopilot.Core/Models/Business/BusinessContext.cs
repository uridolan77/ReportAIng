namespace BIReportingCopilot.Core.Models.Business;

/// <summary>
/// Business table information for context generation
/// </summary>
public class BusinessTable
{
    /// <summary>
    /// Table identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Table name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Schema name
    /// </summary>
    public string Schema { get; set; } = string.Empty;

    /// <summary>
    /// Business purpose description
    /// </summary>
    public string BusinessPurpose { get; set; } = string.Empty;

    /// <summary>
    /// Business context
    /// </summary>
    public string BusinessContext { get; set; } = string.Empty;

    /// <summary>
    /// Primary use cases
    /// </summary>
    public List<string> PrimaryUseCases { get; set; } = new();

    /// <summary>
    /// Table columns
    /// </summary>
    public List<BusinessColumn> Columns { get; set; } = new();

    /// <summary>
    /// Related tables
    /// </summary>
    public List<string> RelatedTables { get; set; } = new();
}

/// <summary>
/// Business column information
/// </summary>
public class BusinessColumn
{
    /// <summary>
    /// Column name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Business meaning
    /// </summary>
    public string BusinessMeaning { get; set; } = string.Empty;

    /// <summary>
    /// Data type
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a key column
    /// </summary>
    public bool IsKey { get; set; }

    /// <summary>
    /// Sample values
    /// </summary>
    public List<string> SampleValues { get; set; } = new();
}

/// <summary>
/// Business term for glossary
/// </summary>
public class BusinessTerm
{
    /// <summary>
    /// Term identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Term name
    /// </summary>
    public string Term { get; set; } = string.Empty;

    /// <summary>
    /// Term definition
    /// </summary>
    public string Definition { get; set; } = string.Empty;

    /// <summary>
    /// Business context
    /// </summary>
    public string BusinessContext { get; set; } = string.Empty;

    /// <summary>
    /// Synonyms
    /// </summary>
    public List<string> Synonyms { get; set; } = new();

    /// <summary>
    /// Related terms
    /// </summary>
    public List<string> RelatedTerms { get; set; } = new();

    /// <summary>
    /// Category
    /// </summary>
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// Business relationship between tables
/// </summary>
public class BusinessRelationship
{
    /// <summary>
    /// Relationship identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Source table
    /// </summary>
    public string SourceTable { get; set; } = string.Empty;

    /// <summary>
    /// Target table
    /// </summary>
    public string TargetTable { get; set; } = string.Empty;

    /// <summary>
    /// Relationship type
    /// </summary>
    public string RelationshipType { get; set; } = string.Empty;

    /// <summary>
    /// Source columns
    /// </summary>
    public List<string> SourceColumns { get; set; } = new();

    /// <summary>
    /// Target columns
    /// </summary>
    public List<string> TargetColumns { get; set; } = new();

    /// <summary>
    /// Business description
    /// </summary>
    public string BusinessDescription { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score
    /// </summary>
    public double ConfidenceScore { get; set; }
}
