using System.ComponentModel.DataAnnotations;

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
