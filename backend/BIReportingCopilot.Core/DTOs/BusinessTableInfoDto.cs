using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.DTOs;

/// <summary>
/// Business table information DTO
/// </summary>
public class BusinessTableInfoDto
{
    public long Id { get; set; }
    public string TableId { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessPurpose { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public string PrimaryUseCase { get; set; } = string.Empty;
    public List<string> CommonQueryPatterns { get; set; } = new();
    public string BusinessRules { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<BusinessColumnInfoDto> Columns { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }

    // Additional business metadata fields
    public string DomainClassification { get; set; } = string.Empty;
    public List<string> NaturalLanguageAliases { get; set; } = new();
    public List<string> BusinessProcesses { get; set; } = new();
    public List<string> AnalyticalUseCases { get; set; } = new();
    public List<string> ReportingCategories { get; set; } = new();
    public List<string> VectorSearchKeywords { get; set; } = new();
    public List<string> BusinessGlossaryTerms { get; set; } = new();
    public List<string> LLMContextHints { get; set; } = new();
    public List<string> QueryComplexityHints { get; set; } = new();
    public object SemanticRelationships { get; set; } = new();
    public object UsagePatterns { get; set; } = new();
    public object DataQualityIndicators { get; set; } = new();
    public object RelationshipSemantics { get; set; } = new();
    public object DataGovernancePolicies { get; set; } = new();
    public decimal ImportanceScore { get; set; }
    public decimal UsageFrequency { get; set; }
    public decimal SemanticCoverageScore { get; set; }
    public DateTime? LastAnalyzed { get; set; }
    public string BusinessOwner { get; set; } = string.Empty;
}

/// <summary>
/// Business column information
/// </summary>
public class BusinessColumnInfo
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string BusinessMeaning { get; set; } = string.Empty; // Added missing property
    public string BusinessPurpose { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public List<string> SampleValues { get; set; } = new();
    public List<string> DataExamples { get; set; } = new(); // Added missing property
    public bool IsKey { get; set; }
    public bool IsKeyColumn { get; set; } // Added missing property
    public bool IsRequired { get; set; }
    public string? ValidationRules { get; set; }
}

/// <summary>
/// Create table info request DTO
/// </summary>
public class CreateTableInfoRequest
{
    [Required]
    [StringLength(100)]
    public string TableName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string SchemaName { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string BusinessPurpose { get; set; } = string.Empty;

    [StringLength(1000)]
    public string BusinessContext { get; set; } = string.Empty;

    [StringLength(500)]
    public string PrimaryUseCase { get; set; } = string.Empty;

    public List<string> CommonQueryPatterns { get; set; } = new();

    [StringLength(1000)]
    public string BusinessRules { get; set; } = string.Empty;

    public List<BusinessColumnInfo> Columns { get; set; } = new(); // Added missing property
}

/// <summary>
/// Business table statistics DTO (alias for compatibility)
/// </summary>
public class BusinessTableStatistics : BIReportingCopilot.Core.Models.BusinessTableStatistics
{
    // Inherits all properties from the base class
}

/// <summary>
/// Create column info request DTO (alias for compatibility)
/// </summary>
public class CreateColumnInfoRequest : BIReportingCopilot.Core.Models.CreateColumnInfoRequest
{
    // Inherits all properties from the base class
}

/// <summary>
/// Update column request DTO
/// </summary>
public class UpdateColumnRequest
{
    [StringLength(128)]
    public string ColumnName { get; set; } = string.Empty;

    [StringLength(100)]
    public string BusinessFriendlyName { get; set; } = string.Empty;

    [StringLength(50)]
    public string BusinessDataType { get; set; } = string.Empty;

    [StringLength(1000)]
    public string NaturalLanguageDescription { get; set; } = string.Empty;

    [StringLength(500)]
    public string BusinessMeaning { get; set; } = string.Empty;

    [StringLength(1000)]
    public string BusinessContext { get; set; } = string.Empty;

    [StringLength(500)]
    public string BusinessPurpose { get; set; } = string.Empty;

    public List<string> DataExamples { get; set; } = new();

    public List<string> ValueExamples { get; set; } = new();

    [StringLength(1000)]
    public string ValidationRules { get; set; } = string.Empty;

    [StringLength(1000)]
    public string BusinessRules { get; set; } = string.Empty;

    [StringLength(50)]
    public string PreferredAggregation { get; set; } = string.Empty;

    [StringLength(50)]
    public string DataGovernanceLevel { get; set; } = string.Empty;

    public DateTime? LastBusinessReview { get; set; }

    public double DataQualityScore { get; set; } = 5.0;

    public int UsageFrequency { get; set; } = 0;

    public double SemanticRelevanceScore { get; set; } = 0.5;

    public double ImportanceScore { get; set; } = 0.5;

    public bool IsActive { get; set; } = true;

    public bool IsKeyColumn { get; set; } = false;

    public bool IsSensitiveData { get; set; } = false;

    public bool IsCalculatedField { get; set; } = false;
}
