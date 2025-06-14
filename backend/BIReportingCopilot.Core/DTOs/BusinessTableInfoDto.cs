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
    public List<BusinessColumnInfo> Columns { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }
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
