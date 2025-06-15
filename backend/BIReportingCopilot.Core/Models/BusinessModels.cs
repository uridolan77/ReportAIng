using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;
    /// <summary>
    /// Business table model for business context
    /// </summary>
    public class BusinessTable
    {
        public string TableId { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public string SchemaName { get; set; } = string.Empty;
        public string BusinessName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Purpose { get; set; } = string.Empty;
        public List<string> BusinessTerms { get; set; } = new();
        public List<string> RelatedTables { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Business term model for business context
    /// </summary>
    public class BusinessTerm
    {
        public string TermId { get; set; } = string.Empty;
        public string Term { get; set; } = string.Empty;
        public string Definition { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public List<string> Synonyms { get; set; } = new();
        public List<string> RelatedTerms { get; set; } = new();
        public List<string> UsageExamples { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Business relationship model for business context
    /// </summary>
    public class BusinessRelationship
    {
        public string RelationshipId { get; set; } = string.Empty;
        public string SourceTable { get; set; } = string.Empty;
        public string TargetTable { get; set; } = string.Empty;
        public string RelationshipType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> SourceColumns { get; set; } = new();
        public List<string> TargetColumns { get; set; } = new();
        public string BusinessMeaning { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
    }

    /// <summary>
    /// Create column info request model
    /// </summary>
    public class CreateColumnInfoRequest
    {
        [Required]
        public string TableName { get; set; } = string.Empty;

        [Required]
        public string ColumnName { get; set; } = string.Empty;

        public string DataType { get; set; } = string.Empty;
        public string BusinessMeaning { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsForeignKey { get; set; }
        public string? DefaultValue { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// Application database context type
    /// </summary>
    public class ApplicationDbContext
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public Dictionary<string, object> Configuration { get; set; } = new();
    }

    /// <summary>
    /// Context type enumeration
    /// </summary>
    public enum ContextType
    {
        Database,
        Application,
        Business,
        Security,
        Performance
    }

    /// <summary>
    /// Performance analysis result for tuning operations
    /// </summary>
    public class PerformanceAnalysisResult
    {
        public string AnalysisId { get; set; } = string.Empty;
        public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
        public string QueryId { get; set; } = string.Empty;
        public string OriginalQuery { get; set; } = string.Empty;
        public string OptimizedQuery { get; set; } = string.Empty;
        public TimeSpan OriginalExecutionTime { get; set; }
        public TimeSpan OptimizedExecutionTime { get; set; }
        public double PerformanceImprovement { get; set; }
        public List<string> OptimizationSuggestions { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, object> Metrics { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
    }

// All other DTOs and statistics classes already exist in other files - removed duplicates
