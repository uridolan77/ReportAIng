namespace BIReportingCopilot.Infrastructure.Data.Entities;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

/// <summary>
/// Cost tracking entity for AI provider usage - extends existing LLMUsageLogs
/// </summary>
public class CostTrackingEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(256)]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string ProviderId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(100)]
    public string ModelId { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string RequestType { get; set; } = string.Empty;
    
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens { get; set; }
    
    [Column(TypeName = "decimal(18,8)")]
    public decimal Cost { get; set; }
    
    [Column(TypeName = "decimal(18,8)")]
    public decimal CostPerToken { get; set; }
    
    public long DurationMs { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [MaxLength(256)]
    public string? RequestId { get; set; }
    
    [MaxLength(256)]
    public string? QueryId { get; set; }
    
    [MaxLength(100)]
    public string? Department { get; set; }
    
    [MaxLength(100)]
    public string? Project { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string? Metadata { get; set; } // JSON
}

/// <summary>
/// Budget management entity for cost control
/// </summary>
public class BudgetManagementEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty; // User, Department, Project, Global
    
    [Required]
    [MaxLength(256)]
    public string EntityId { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal BudgetAmount { get; set; }
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal SpentAmount { get; set; }
    
    [MaxLength(20)]
    public string Period { get; set; } = "Monthly"; // Daily, Weekly, Monthly, Quarterly, Yearly, Custom
    
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    [Column(TypeName = "decimal(3,2)")]
    public decimal AlertThreshold { get; set; } = 0.8m;
    
    [Column(TypeName = "decimal(3,2)")]
    public decimal BlockThreshold { get; set; } = 1.0m;
    
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Resource usage tracking entity
/// </summary>
public class ResourceUsageEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(256)]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string ResourceType { get; set; } = string.Empty;
    
    [MaxLength(256)]
    public string ResourceId { get; set; } = string.Empty;
    
    public int Quantity { get; set; }
    public long DurationMs { get; set; }
    
    [Column(TypeName = "decimal(18,8)")]
    public decimal Cost { get; set; }
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [MaxLength(256)]
    public string? RequestId { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string? Metadata { get; set; } // JSON
}

/// <summary>
/// Performance metrics tracking entity
/// </summary>
public class PerformanceMetricsEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string MetricName { get; set; } = string.Empty;
    
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;
    
    public double Value { get; set; }
    
    [MaxLength(20)]
    public string Unit { get; set; } = string.Empty;
    
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [MaxLength(256)]
    public string? EntityId { get; set; }
    
    [MaxLength(50)]
    public string? EntityType { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string? Tags { get; set; } // JSON
}

/// <summary>
/// Cache statistics tracking entity
/// </summary>
public class CacheStatisticsEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string CacheType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(20)]
    public string Operation { get; set; } = string.Empty; // HIT, MISS, SET, DELETE
    
    [MaxLength(500)]
    public string Key { get; set; } = string.Empty;
    
    public long SizeBytes { get; set; }
    public long DurationMs { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [MaxLength(256)]
    public string? UserId { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string? Metadata { get; set; } // JSON
}

/// <summary>
/// Cache configuration entity
/// </summary>
public class CacheConfigurationEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string CacheType { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
    
    public long DefaultTtlSeconds { get; set; }
    public long MaxSizeBytes { get; set; }
    public int MaxEntries { get; set; }
    
    [MaxLength(20)]
    public string EvictionPolicy { get; set; } = "LRU";
    
    public bool EnableCompression { get; set; } = false;
    public bool EnableEncryption { get; set; } = false;
    
    [Column(TypeName = "nvarchar(max)")]
    public string? Settings { get; set; } // JSON
    
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Resource quota configuration entity
/// </summary>
public class ResourceQuotaEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(256)]
    public string UserId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string ResourceType { get; set; } = string.Empty;
    
    public int MaxQuantity { get; set; }
    public long PeriodSeconds { get; set; }
    public int CurrentUsage { get; set; }
    public DateTime ResetDate { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Cost prediction entity
/// </summary>
public class CostPredictionEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(256)]
    public string QueryId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(256)]
    public string UserId { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,8)")]
    public decimal EstimatedCost { get; set; }
    
    [Column(TypeName = "decimal(3,2)")]
    public decimal ConfidenceScore { get; set; }
    
    [MaxLength(100)]
    public string ModelUsed { get; set; } = string.Empty;
    
    public int EstimatedTokens { get; set; }
    public long EstimatedDurationMs { get; set; }
    
    [Column(TypeName = "nvarchar(max)")]
    public string? Factors { get; set; } // JSON
}

/// <summary>
/// Cost optimization recommendation entity
/// </summary>
public class CostOptimizationRecommendationEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string Description { get; set; } = string.Empty;
    
    [Column(TypeName = "decimal(18,2)")]
    public decimal PotentialSavings { get; set; }
    
    public double ImpactScore { get; set; }
    
    [MaxLength(20)]
    public string Priority { get; set; } = "Medium";
    
    [Column(TypeName = "nvarchar(max)")]
    public string Implementation { get; set; } = string.Empty;
    
    [Column(TypeName = "nvarchar(max)")]
    public string? Benefits { get; set; } // JSON array
    
    [Column(TypeName = "nvarchar(max)")]
    public string? Risks { get; set; } // JSON array
    
    public bool IsImplemented { get; set; } = false;
}
