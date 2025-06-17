namespace BIReportingCopilot.Core.DTOs;

/// <summary>
/// Change log entry alias for backward compatibility
/// </summary>
public class ChangeLogEntry : SchemaChangeLogEntry
{
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Item { get; set; } = string.Empty;
    // Inherits all properties from SchemaChangeLogEntry
}

/// <summary>
/// Detailed schema version DTO with all related data
/// </summary>
public class DetailedSchemaVersionDto : BusinessSchemaVersionDto
{
    // Inherits all properties from BusinessSchemaVersionDto and adds detailed loading
}

/// <summary>
/// Schema comparison summary DTO
/// </summary>
public class SchemaComparisonSummary
{
    public string SourceSchemaName { get; set; } = string.Empty;
    public string TargetSchemaName { get; set; } = string.Empty;
    public DateTime ComparisonDate { get; set; }
    public int TotalDifferences { get; set; }
    public int TablesAdded { get; set; }
    public int TablesRemoved { get; set; }
    public int TablesModified { get; set; }
    public int ColumnsAdded { get; set; }
    public int ColumnsRemoved { get; set; }
    public int ColumnsModified { get; set; }
    public int GlossaryTermsAdded { get; set; }
    public int GlossaryTermsRemoved { get; set; }
    public int GlossaryTermsModified { get; set; }
    public int RelationshipsAdded { get; set; }
    public int RelationshipsRemoved { get; set; }
    public int RelationshipsModified { get; set; }
    public List<SchemaDifference> Differences { get; set; } = new();
}

/// <summary>
/// Schema difference DTO
/// </summary>
public class SchemaDifference
{
    public string Type { get; set; } = string.Empty; // "Added", "Removed", "Modified"
    public string Category { get; set; } = string.Empty; // "Table", "Column", "Glossary", "Relationship"
    public string Item { get; set; } = string.Empty; // Object name
    public object? SourceValue { get; set; }
    public object? TargetValue { get; set; }
    public string DifferenceType { get; set; } = string.Empty; // "Added", "Removed", "Modified"
    public string ObjectType { get; set; } = string.Empty; // "Table", "Column", "Index", etc.
    public string ObjectName { get; set; } = string.Empty;
    public string? SchemaName { get; set; }
    public string? TableName { get; set; }
    public string? ColumnName { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> OldValues { get; set; } = new();
    public Dictionary<string, object> NewValues { get; set; } = new();
    public string Severity { get; set; } = "Info"; // "Info", "Warning", "Error"
}

/// <summary>
/// Schema comparison DTO
/// </summary>
public class SchemaComparisonDto
{
    public Guid Id { get; set; }
    public string SourceSchemaName { get; set; } = string.Empty;
    public string TargetSchemaName { get; set; } = string.Empty;
    public BusinessSchemaVersionDto? SourceVersion { get; set; }
    public BusinessSchemaVersionDto? TargetVersion { get; set; }
    public DateTime ComparisonDate { get; set; }
    public string Status { get; set; } = string.Empty; // "InProgress", "Completed", "Failed"
    public SchemaComparisonSummary? Summary { get; set; }
    public List<SchemaDifference> Differences { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Apply to schema request DTO
/// </summary>
public class ApplyToSchemaRequest
{
    public Guid SchemaId { get; set; }
    public string? NewSchemaName { get; set; }
    public string? NewSchemaDescription { get; set; }
    public string? VersionName { get; set; }
    public string? VersionDescription { get; set; }
    public bool CreateNewVersion { get; set; } = true;
    public bool SetAsCurrent { get; set; } = false;
    public List<BIReportingCopilot.Core.DTOs.SchemaChangeLogEntry> ChangeLog { get; set; } = new();
    public List<SchemaTableContextDto> TableContexts { get; set; } = new();
    public List<SchemaGlossaryTermDto> GlossaryTerms { get; set; } = new();
    public List<SchemaRelationshipDto> Relationships { get; set; } = new();
    public Guid TargetSchemaVersionId { get; set; }
    public List<string> DifferenceIds { get; set; } = new();
    public bool CreateBackup { get; set; } = true;
    public string? BackupDescription { get; set; }
    public bool ValidateOnly { get; set; } = false;
    public Dictionary<string, string> ParameterMappings { get; set; } = new();
}
