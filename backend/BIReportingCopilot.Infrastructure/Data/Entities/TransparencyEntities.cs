using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BIReportingCopilot.Infrastructure.Data.Entities
{
    /// <summary>
    /// Entity Framework entity for PromptConstructionTraces table
    /// </summary>
    [Table("PromptConstructionTraces")]
    public class PromptConstructionTraceEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string TraceId { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string UserQuestion { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string IntentType { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        [Column(TypeName = "decimal(5,4)")]
        public decimal OverallConfidence { get; set; } = 0.0m;

        public int TotalTokens { get; set; } = 0;

        public string? FinalPrompt { get; set; }

        public bool Success { get; set; } = false;

        public string? ErrorMessage { get; set; }

        public string? Metadata { get; set; } // JSON

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ICollection<PromptConstructionStepEntity> Steps { get; set; } = new List<PromptConstructionStepEntity>();
    }

    /// <summary>
    /// Entity Framework entity for PromptConstructionSteps table
    /// </summary>
    [Table("PromptConstructionSteps")]
    public class PromptConstructionStepEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string TraceId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string StepName { get; set; } = string.Empty;

        [Required]
        public int StepOrder { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool Success { get; set; } = false;

        public int TokensAdded { get; set; } = 0;

        [Column(TypeName = "decimal(5,4)")]
        public decimal Confidence { get; set; } = 0.0m;

        public string? Content { get; set; }

        public string? Details { get; set; } // JSON

        public string? ErrorMessage { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Foreign key
        [ForeignKey("TraceId")]
        public virtual PromptConstructionTraceEntity? Trace { get; set; }
    }

    /// <summary>
    /// Entity Framework entity for TokenBudgets table
    /// </summary>
    [Table("TokenBudgets")]
    public class TokenBudgetEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string RequestType { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string IntentType { get; set; } = string.Empty;

        public int MaxTotalTokens { get; set; }

        public int BasePromptTokens { get; set; }

        public int ReservedResponseTokens { get; set; }

        public int AvailableContextTokens { get; set; }

        public int SchemaContextBudget { get; set; }

        public int BusinessContextBudget { get; set; }

        public int ExamplesBudget { get; set; }

        public int RulesBudget { get; set; }

        public int GlossaryBudget { get; set; }

        [Column(TypeName = "decimal(10,6)")]
        public decimal EstimatedCost { get; set; } = 0.0m;

        public int? ActualTokensUsed { get; set; }

        [Column(TypeName = "decimal(10,6)")]
        public decimal? ActualCost { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }
    }

    /// <summary>
    /// Entity Framework entity for BusinessContextProfiles table
    /// </summary>
    [Table("BusinessContextProfiles")]
    public class BusinessContextProfileEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string OriginalQuestion { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string IntentType { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,4)")]
        public decimal IntentConfidence { get; set; } = 0.0m;

        [MaxLength(500)]
        public string? IntentDescription { get; set; }

        [Required]
        [MaxLength(200)]
        public string DomainName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,4)")]
        public decimal DomainConfidence { get; set; } = 0.0m;

        [Column(TypeName = "decimal(5,4)")]
        public decimal OverallConfidence { get; set; } = 0.0m;

        public int ProcessingTimeMs { get; set; } = 0;

        public string? Entities { get; set; } // JSON

        public string? Keywords { get; set; } // JSON

        public string? Metadata { get; set; } // JSON

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Entity Framework entity for BusinessEntities table
    /// </summary>
    [Table("BusinessEntities")]
    public class BusinessEntityEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string ProfileId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string EntityValue { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5,4)")]
        public decimal Confidence { get; set; } = 0.0m;

        public int StartPosition { get; set; }

        public int EndPosition { get; set; }

        public string? Context { get; set; }

        public string? Metadata { get; set; } // JSON

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Entity Framework entity for TransparencyReports table
    /// </summary>
    [Table("TransparencyReports")]
    public class TransparencyReportEntity
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string TraceId { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string UserQuestion { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string IntentType { get; set; } = string.Empty;

        public string? Summary { get; set; }

        public string? DetailedMetrics { get; set; } // JSON

        public string? PerformanceAnalysis { get; set; } // JSON

        public string? OptimizationSuggestions { get; set; } // JSON

        [Required]
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        public string? Metadata { get; set; } // JSON
    }
}
