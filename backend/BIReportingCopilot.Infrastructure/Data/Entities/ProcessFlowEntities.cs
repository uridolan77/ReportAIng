using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BIReportingCopilot.Infrastructure.Data.Entities;

/// <summary>
/// Entity for tracking complete process flow sessions
/// </summary>
[Table("ProcessFlowSessions")]
public class ProcessFlowSessionEntity : BaseEntity
{
    /// <summary>
    /// Primary key for the entity
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Unique session identifier for the process flow
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// User ID who initiated the process
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Original user query that started the process
    /// </summary>
    [Required]
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// Type of query/process (enhanced, simple, etc.)
    /// </summary>
    [MaxLength(100)]
    public string QueryType { get; set; } = "enhanced";

    /// <summary>
    /// Current status of the process flow
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "running"; // running, completed, error, cancelled

    /// <summary>
    /// When the process flow started
    /// </summary>
    public DateTime StartTime { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the process flow ended (if completed)
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Total duration in milliseconds
    /// </summary>
    public long? TotalDurationMs { get; set; }

    /// <summary>
    /// Overall confidence score for the process
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal? OverallConfidence { get; set; }

    /// <summary>
    /// Generated SQL query (if applicable)
    /// </summary>
    public string? GeneratedSQL { get; set; }

    /// <summary>
    /// Execution result data (JSON)
    /// </summary>
    public string? ExecutionResult { get; set; }

    /// <summary>
    /// Error message if process failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional metadata (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Related conversation/chat ID
    /// </summary>
    [MaxLength(450)]
    public string? ConversationId { get; set; }

    /// <summary>
    /// Related message ID
    /// </summary>
    [MaxLength(450)]
    public string? MessageId { get; set; }

    // Navigation properties
    public virtual ICollection<ProcessFlowStepEntity> Steps { get; set; } = new List<ProcessFlowStepEntity>();
    public virtual ICollection<ProcessFlowLogEntity> Logs { get; set; } = new List<ProcessFlowLogEntity>();
    public virtual ProcessFlowTransparencyEntity? Transparency { get; set; }
}

/// <summary>
/// Entity for tracking individual process flow steps
/// </summary>
[Table("ProcessFlowSteps")]
public class ProcessFlowStepEntity : BaseEntity
{
    /// <summary>
    /// Primary key for the entity
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Reference to the parent process flow session
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Unique identifier for this step
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string StepId { get; set; } = string.Empty;

    /// <summary>
    /// Parent step ID (for sub-steps)
    /// </summary>
    [MaxLength(100)]
    public string? ParentStepId { get; set; }

    /// <summary>
    /// Display name of the step
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this step does
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Order of execution within the process
    /// </summary>
    public int StepOrder { get; set; }

    /// <summary>
    /// Current status of the step
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string Status { get; set; } = "pending"; // pending, running, completed, error, skipped

    /// <summary>
    /// When the step started
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// When the step ended
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Duration in milliseconds
    /// </summary>
    public long? DurationMs { get; set; }

    /// <summary>
    /// Confidence score for this step
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal? Confidence { get; set; }

    /// <summary>
    /// Input data for this step (JSON)
    /// </summary>
    public string? InputData { get; set; }

    /// <summary>
    /// Output data from this step (JSON)
    /// </summary>
    public string? OutputData { get; set; }

    /// <summary>
    /// Error message if step failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional metadata for this step (JSON)
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; } = 0;

    // Navigation properties
    public virtual ProcessFlowSessionEntity Session { get; set; } = null!;
    
    public virtual ICollection<ProcessFlowLogEntity> Logs { get; set; } = new List<ProcessFlowLogEntity>();
    public virtual ICollection<ProcessFlowStepEntity> SubSteps { get; set; } = new List<ProcessFlowStepEntity>();
    
    [ForeignKey(nameof(ParentStepId))]
    public virtual ProcessFlowStepEntity? ParentStep { get; set; }
}

/// <summary>
/// Entity for tracking detailed logs within process flow steps
/// </summary>
[Table("ProcessFlowLogs")]
public class ProcessFlowLogEntity : BaseEntity
{
    /// <summary>
    /// Primary key for the entity
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Reference to the parent process flow session
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the specific step (optional)
    /// </summary>
    [MaxLength(100)]
    public string? StepId { get; set; }

    /// <summary>
    /// Log level (Info, Warning, Error, Debug)
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string LogLevel { get; set; } = "Info";

    /// <summary>
    /// Log message
    /// </summary>
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Detailed log data (JSON)
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// Exception details if applicable
    /// </summary>
    public string? Exception { get; set; }

    /// <summary>
    /// Source component that generated the log
    /// </summary>
    [MaxLength(100)]
    public string? Source { get; set; }

    /// <summary>
    /// Timestamp when the log was created
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual ProcessFlowSessionEntity Session { get; set; } = null!;
    
    [ForeignKey(nameof(StepId))]
    public virtual ProcessFlowStepEntity? Step { get; set; }
}

/// <summary>
/// Entity for tracking AI transparency information for process flows
/// </summary>
[Table("ProcessFlowTransparency")]
public class ProcessFlowTransparencyEntity : BaseEntity
{
    /// <summary>
    /// Primary key for the entity
    /// </summary>
    [Key]
    public long Id { get; set; }

    /// <summary>
    /// Reference to the parent process flow session (one-to-one)
    /// </summary>
    [Required]
    [MaxLength(450)]
    public string SessionId { get; set; } = string.Empty;

    /// <summary>
    /// AI model used (e.g., gpt-4, gpt-3.5-turbo)
    /// </summary>
    [MaxLength(100)]
    public string? Model { get; set; }

    /// <summary>
    /// Temperature setting used
    /// </summary>
    [Column(TypeName = "decimal(3,2)")]
    public decimal? Temperature { get; set; }

    /// <summary>
    /// Maximum tokens setting
    /// </summary>
    public int? MaxTokens { get; set; }

    /// <summary>
    /// Tokens used in prompt
    /// </summary>
    public int? PromptTokens { get; set; }

    /// <summary>
    /// Tokens used in completion
    /// </summary>
    public int? CompletionTokens { get; set; }

    /// <summary>
    /// Total tokens used
    /// </summary>
    public int? TotalTokens { get; set; }

    /// <summary>
    /// Estimated cost for this request
    /// </summary>
    [Column(TypeName = "decimal(10,6)")]
    public decimal? EstimatedCost { get; set; }

    /// <summary>
    /// Overall confidence score
    /// </summary>
    [Column(TypeName = "decimal(5,4)")]
    public decimal? Confidence { get; set; }

    /// <summary>
    /// Processing time for AI calls in milliseconds
    /// </summary>
    public long? AIProcessingTimeMs { get; set; }

    /// <summary>
    /// Number of API calls made
    /// </summary>
    public int ApiCallCount { get; set; } = 0;

    /// <summary>
    /// Detailed prompt information (JSON)
    /// </summary>
    public string? PromptDetails { get; set; }

    /// <summary>
    /// Response analysis (JSON)
    /// </summary>
    public string? ResponseAnalysis { get; set; }

    /// <summary>
    /// Quality metrics (JSON)
    /// </summary>
    public string? QualityMetrics { get; set; }

    /// <summary>
    /// Optimization suggestions (JSON)
    /// </summary>
    public string? OptimizationSuggestions { get; set; }

    // Navigation property
    public virtual ProcessFlowSessionEntity Session { get; set; } = null!;
}
