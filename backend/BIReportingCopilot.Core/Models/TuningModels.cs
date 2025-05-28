namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Models for AI tuning and business schema management
/// </summary>

public class BusinessTableInfo
{
    public long Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = "common";
    public string BusinessPurpose { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public string PrimaryUseCase { get; set; } = string.Empty;
    public string CommonQueryPatterns { get; set; } = string.Empty; // JSON array
    public string BusinessRules { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}

public class BusinessColumnInfo
{
    public long Id { get; set; }
    public long TableInfoId { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public string BusinessMeaning { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public string DataExamples { get; set; } = string.Empty; // JSON array of sample values
    public string ValidationRules { get; set; } = string.Empty;
    public bool IsKeyColumn { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }

    // Navigation property
    public BusinessTableInfo? TableInfo { get; set; }
}

public class TuningQueryPattern
{
    public long Id { get; set; }
    public string PatternName { get; set; } = string.Empty;
    public string NaturalLanguagePattern { get; set; } = string.Empty;
    public string SqlTemplate { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public string[] Keywords { get; set; } = Array.Empty<string>(); // JSON array
    public string[] RequiredTables { get; set; } = Array.Empty<string>(); // JSON array
    public int Priority { get; set; } = 1; // Higher priority patterns are matched first
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}

public class BusinessGlossary
{
    public long Id { get; set; }
    public string Term { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public string[] Synonyms { get; set; } = Array.Empty<string>(); // JSON array
    public string[] RelatedTerms { get; set; } = Array.Empty<string>(); // JSON array
    public string Category { get; set; } = string.Empty; // e.g., "Financial", "Player", "Gaming"
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}

public class AITuningSettings
{
    public long Id { get; set; }
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // e.g., "Prompts", "Business Rules", "Query Generation"
    public string DataType { get; set; } = "string"; // string, int, bool, json
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? UpdatedBy { get; set; }
}

// DTOs for API
public class BusinessTableInfoDto
{
    public long Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = "common";
    public string BusinessPurpose { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public string PrimaryUseCase { get; set; } = string.Empty;
    public List<string> CommonQueryPatterns { get; set; } = new();
    public string BusinessRules { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<BusinessColumnInfoDto> Columns { get; set; } = new();
}

public class BusinessColumnInfoDto
{
    public long Id { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public string BusinessMeaning { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public List<string> DataExamples { get; set; } = new();
    public string ValidationRules { get; set; } = string.Empty;
    public bool IsKeyColumn { get; set; }
    public bool IsActive { get; set; } = true;
}

public class QueryPatternDto
{
    public long Id { get; set; }
    public string PatternName { get; set; } = string.Empty;
    public string NaturalLanguagePattern { get; set; } = string.Empty;
    public string SqlTemplate { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public List<string> RequiredTables { get; set; } = new();
    public int Priority { get; set; } = 1;
    public bool IsActive { get; set; } = true;
}

public class BusinessGlossaryDto
{
    public long Id { get; set; }
    public string Term { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public List<string> Synonyms { get; set; } = new();
    public List<string> RelatedTerms { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class AITuningSettingsDto
{
    public long Id { get; set; }
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";
    public bool IsActive { get; set; } = true;
}

// Request/Response models
public class CreateTableInfoRequest
{
    public string TableName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = "common";
    public string BusinessPurpose { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public string PrimaryUseCase { get; set; } = string.Empty;
    public List<string> CommonQueryPatterns { get; set; } = new();
    public string BusinessRules { get; set; } = string.Empty;
    public List<CreateColumnInfoRequest> Columns { get; set; } = new();
}

public class CreateColumnInfoRequest
{
    public string ColumnName { get; set; } = string.Empty;
    public string BusinessMeaning { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public List<string> DataExamples { get; set; } = new();
    public string ValidationRules { get; set; } = string.Empty;
    public bool IsKeyColumn { get; set; }
}

public class CreateQueryPatternRequest
{
    public string PatternName { get; set; } = string.Empty;
    public string NaturalLanguagePattern { get; set; } = string.Empty;
    public string SqlTemplate { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
    public List<string> RequiredTables { get; set; } = new();
    public int Priority { get; set; } = 1;
}

public class TuningDashboardData
{
    public int TotalTables { get; set; }
    public int TotalColumns { get; set; }
    public int TotalPatterns { get; set; }
    public int TotalGlossaryTerms { get; set; }
    public int ActivePromptTemplates { get; set; }
    public List<string> RecentlyUpdatedTables { get; set; } = new();
    public List<string> MostUsedPatterns { get; set; } = new();
    public Dictionary<string, int> PatternUsageStats { get; set; } = new();
}
