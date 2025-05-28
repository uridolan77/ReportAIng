using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BIReportingCopilot.Infrastructure.Data.Entities;

[Table("BusinessTableInfo")]
public class BusinessTableInfoEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string TableName { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string SchemaName { get; set; } = "common";

    [MaxLength(500)]
    public string BusinessPurpose { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string BusinessContext { get; set; } = string.Empty;

    [MaxLength(500)]
    public string PrimaryUseCase { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string CommonQueryPatterns { get; set; } = string.Empty; // JSON

    [MaxLength(2000)]
    public string BusinessRules { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<BusinessColumnInfoEntity> Columns { get; set; } = new List<BusinessColumnInfoEntity>();
}

[Table("BusinessColumnInfo")]
public class BusinessColumnInfoEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    public long TableInfoId { get; set; }

    [Required]
    [MaxLength(128)]
    public string ColumnName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string BusinessMeaning { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string BusinessContext { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string DataExamples { get; set; } = string.Empty; // JSON

    [MaxLength(1000)]
    public string ValidationRules { get; set; } = string.Empty;

    public bool IsKeyColumn { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    [ForeignKey("TableInfoId")]
    public virtual BusinessTableInfoEntity? TableInfo { get; set; }
}

[Table("QueryPatterns")]
public class QueryPatternEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string PatternName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string NaturalLanguagePattern { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string SqlTemplate { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string BusinessContext { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Keywords { get; set; } = string.Empty; // JSON

    [MaxLength(1000)]
    public string RequiredTables { get; set; } = string.Empty; // JSON

    public int Priority { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public int UsageCount { get; set; } = 0;
    public DateTime? LastUsed { get; set; }
}

[Table("BusinessGlossary")]
public class BusinessGlossaryEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Term { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Definition { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string BusinessContext { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Synonyms { get; set; } = string.Empty; // JSON

    [MaxLength(1000)]
    public string RelatedTerms { get; set; } = string.Empty; // JSON

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public int UsageCount { get; set; } = 0;
    public DateTime? LastUsed { get; set; }
}

[Table("AITuningSettings")]
public class AITuningSettingsEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string SettingKey { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string SettingValue { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(50)]
    public string DataType { get; set; } = "string";

    public bool IsActive { get; set; } = true;
}


