using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models.DTOs;

/// <summary>
/// Business table information DTO
/// </summary>
public class BusinessTableInfoDto
{
    public long Id { get; set; } // Added missing property
    public string TableId { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty; // Added missing property
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessPurpose { get; set; } = string.Empty; // Added missing property
    public string BusinessContext { get; set; } = string.Empty; // Added missing property
    public string PrimaryUseCase { get; set; } = string.Empty; // Added missing property
    public List<string> CommonQueryPatterns { get; set; } = new(); // Added missing property
    public string BusinessRules { get; set; } = string.Empty; // Added missing property
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public List<BusinessColumnInfo> Columns { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public string Owner { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

// BusinessTableInfoOptimizedDto already defined in OptimizedDTOs.cs

/// <summary>
/// Business column information
/// </summary>
public class BusinessColumnInfo
{
    public string ColumnName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessMeaning { get; set; } = string.Empty; // Added missing property
    public string BusinessContext { get; set; } = string.Empty; // Added missing property
    public string Description { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public List<string> DataExamples { get; set; } = new(); // Added missing property
    public string ValidationRules { get; set; } = string.Empty; // Added missing property
    public bool IsRequired { get; set; } = false;
    public bool IsPrimaryKey { get; set; } = false;
    public bool IsForeignKey { get; set; } = false;
    public bool IsKeyColumn { get; set; } = false; // Added missing property
    public bool IsActive { get; set; } = true; // Added missing property
    public bool IsNullable { get; set; } = true;
    public string? DefaultValue { get; set; }
    public List<string> AllowedValues { get; set; } = new();
    public string Format { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

// BusinessColumnInfoDto moved to BIReportingCopilot.Core.DTOs.OptimizedDTOs
// This duplicate definition has been removed to avoid conflicts

// CreateTableInfoRequest moved to BIReportingCopilot.Core.DTOs.BusinessTableInfoDto
// This duplicate definition has been removed to avoid conflicts

/// <summary>
/// Business glossary DTO
/// </summary>
public class BusinessGlossaryDto
{
    public long Id { get; set; } // Added missing property
    public string GlossaryId { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty; // Added missing property
    public string Category { get; set; } = string.Empty;
    public List<string> Synonyms { get; set; } = new();
    public List<string> RelatedTerms { get; set; } = new();
    public List<string> Examples { get; set; } = new();
    public string Source { get; set; } = string.Empty;
    public string Owner { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsApproved { get; set; } = false;
    public List<string> Tags { get; set; } = new();
    public bool IsActive { get; set; } = true; // Added missing property
    public int UsageCount { get; set; } = 0; // Added missing property
    public DateTime? LastUsedDate { get; set; } // Added missing property
}

/// <summary>
/// Query pattern DTO
/// </summary>
public class QueryPatternDto
{
    public long Id { get; set; } // Added missing property
    public string PatternId { get; set; } = string.Empty;
    public string PatternName { get; set; } = string.Empty; // Added missing property
    public string Name { get; set; } = string.Empty;
    public string NaturalLanguagePattern { get; set; } = string.Empty; // Added missing property
    public string Description { get; set; } = string.Empty;
    public string Pattern { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty; // Added missing property
    public string Category { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public List<string> RequiredTables { get; set; } = new(); // Added missing property
    public string SqlTemplate { get; set; } = string.Empty;
    public List<PatternParameter> Parameters { get; set; } = new();
    public int Priority { get; set; } = 0; // Added missing property
    public int UsageCount { get; set; } = 0;
    public double SuccessRate { get; set; } = 0.0;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastUsedDate { get; set; } // Added missing property
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Pattern parameter
/// </summary>
public class PatternParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsRequired { get; set; } = false;
    public string? DefaultValue { get; set; }
    public List<string> AllowedValues { get; set; } = new();
    public string ValidationPattern { get; set; } = string.Empty;
}

/// <summary>
/// Create query pattern request
/// </summary>
public class CreateQueryPatternRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string PatternName { get; set; } = string.Empty; // Added missing property
    public string NaturalLanguagePattern { get; set; } = string.Empty; // Added missing property
    public string Description { get; set; } = string.Empty;
    [Required]
    public string Pattern { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty; // Added missing property
    public string Category { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public List<string> RequiredTables { get; set; } = new(); // Added missing property
    public string SqlTemplate { get; set; } = string.Empty;
    public int Priority { get; set; } = 1; // Added missing property
    public List<PatternParameter> Parameters { get; set; } = new();
}

/// <summary>
/// Business rule DTO
/// </summary>
public class BusinessRuleDto
{
    public string RuleId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public RuleType Type { get; set; } = RuleType.Validation;
    public string Condition { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public RulePriority Priority { get; set; } = RulePriority.Medium;
    public bool IsActive { get; set; } = true;
    public List<string> ApplicableTables { get; set; } = new();
    public List<string> ApplicableColumns { get; set; } = new();
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Data quality rule DTO
/// </summary>
public class DataQualityRuleDto
{
    public string RuleId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public QualityRuleType Type { get; set; } = QualityRuleType.NotNull;
    public string ValidationExpression { get; set; } = string.Empty;
    public double Threshold { get; set; } = 0.95;
    public QualityRuleSeverity Severity { get; set; } = QualityRuleSeverity.Warning;
    public bool IsActive { get; set; } = true;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Metadata tag DTO
/// </summary>
public class MetadataTagDto
{
    public string TagId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public bool IsSystemTag { get; set; } = false;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int UsageCount { get; set; } = 0;
}

/// <summary>
/// Business context DTO
/// </summary>
public class BusinessContextDto
{
    public string ContextId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public List<string> RelatedTables { get; set; } = new();
    public List<string> RelatedTerms { get; set; } = new();
    public List<BusinessRuleDto> Rules { get; set; } = new();
    public Dictionary<string, object> Properties { get; set; } = new();
    public string Owner { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

// Enumerations
public enum RuleType
{
    Validation,
    Transformation,
    Calculation,
    Security,
    Business
}

public enum RulePriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum QualityRuleType
{
    NotNull,
    Unique,
    Range,
    Pattern,
    Referential,
    Custom
}

public enum QualityRuleSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// Business table statistics
/// </summary>
public class BusinessTableStatistics
{
    public int TotalTables { get; set; }
    public int ActiveTables { get; set; }
    public int InactiveTables { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Glossary statistics
/// </summary>
public class GlossaryStatistics
{
    public int TotalTerms { get; set; }
    public int ActiveTerms { get; set; }
    public int InactiveTerms { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Query pattern statistics
/// </summary>
public class QueryPatternStatistics
{
    public int TotalPatterns { get; set; }
    public int ActivePatterns { get; set; }
    public int InactivePatterns { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// AI tuning settings DTO
/// </summary>
public class AITuningSettingsDto
{
    public long Id { get; set; }
    public string SettingName { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}
