using System.ComponentModel.DataAnnotations;
using BIReportingCopilot.Infrastructure.Security;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Enhanced semantic validation result combining security, semantic, and business validation
/// Phase 3: Enhanced SQL Validation Pipeline
/// </summary>
public class EnhancedSemanticValidationResult
{
    public bool IsValid { get; set; } = false;
    public double OverallScore { get; set; } = 0.0;
    public bool CanSelfCorrect { get; set; } = false;
    public bool IsSelfCorrected { get; set; } = false;
    public string? OriginalSql { get; set; }
    public string? CorrectionReason { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ValidationTimestamp { get; set; } = DateTime.UtcNow;

    // Detailed validation results
    public EnhancedSqlValidationResult? SecurityValidation { get; set; }
    public SemanticValidationResult? SemanticValidation { get; set; }
    public SchemaComplianceResult? SchemaCompliance { get; set; }
    public BusinessLogicValidationResult? BusinessLogicValidation { get; set; }

    // Dry-run execution results
    public DryRunExecutionResult? DryRunResult { get; set; }

    // Self-correction history
    public List<SelfCorrectionAttempt> CorrectionHistory { get; set; } = new();
}

/// <summary>
/// Semantic validation result focusing on business intent alignment
/// </summary>
public class SemanticValidationResult
{
    public bool IsValid { get; set; } = false;
    public double AlignmentScore { get; set; } = 0.0;
    public string AlignmentReason { get; set; } = string.Empty;
    public List<string> SemanticInconsistencies { get; set; } = new();
    public BusinessTermValidationResult BusinessTermValidation { get; set; } = new();
    public double ConfidenceScore { get; set; } = 0.0;
}

/// <summary>
/// Schema compliance validation result
/// </summary>
public class SchemaComplianceResult
{
    public bool IsCompliant { get; set; } = false;
    public double ComplianceScore { get; set; } = 0.0;
    public TableValidationResult TableValidation { get; set; } = new();
    public ColumnValidationResult ColumnValidation { get; set; } = new();
    public JoinValidationResult JoinValidation { get; set; } = new();
    public ContextValidationResult ContextValidation { get; set; } = new();
}

/// <summary>
/// Business logic validation result
/// </summary>
public class BusinessLogicValidationResult
{
    public bool IsValid { get; set; } = false;
    public double BusinessLogicScore { get; set; } = 0.0;
    public List<string> RuleViolations { get; set; } = new();
    public AccessValidationResult AccessValidation { get; set; } = new();
    public SensitivityValidationResult SensitivityValidation { get; set; } = new();
    public AggregationValidationResult AggregationValidation { get; set; } = new();
}

/// <summary>
/// Dry-run execution result for SQL validation
/// </summary>
public class DryRunExecutionResult
{
    public bool CanExecute { get; set; } = false;
    public bool ExecutedSuccessfully { get; set; } = false;
    public TimeSpan EstimatedExecutionTime { get; set; } = TimeSpan.Zero;
    public int EstimatedRowCount { get; set; } = 0;
    public List<string> ExecutionWarnings { get; set; } = new();
    public List<string> ExecutionErrors { get; set; } = new();
    public string? ExecutionPlan { get; set; }
    public Dictionary<string, object> PerformanceMetrics { get; set; } = new();
}

/// <summary>
/// Self-correction attempt record
/// </summary>
public class SelfCorrectionAttempt
{
    public DateTime AttemptTimestamp { get; set; } = DateTime.UtcNow;
    public string OriginalSql { get; set; } = string.Empty;
    public string CorrectedSql { get; set; } = string.Empty;
    public string CorrectionReason { get; set; } = string.Empty;
    public double ImprovementScore { get; set; } = 0.0;
    public bool WasSuccessful { get; set; } = false;
    public List<string> IssuesAddressed { get; set; } = new();
}

/// <summary>
/// Business term validation result
/// </summary>
public class BusinessTermValidationResult
{
    public bool IsValid { get; set; } = false;
    public List<string> ValidTerms { get; set; } = new();
    public List<string> InvalidTerms { get; set; } = new();
    public List<string> AmbiguousTerms { get; set; } = new();
    public Dictionary<string, string> TermSuggestions { get; set; } = new();
}

/// <summary>
/// Table validation result
/// </summary>
public class TableValidationResult
{
    public bool IsValid { get; set; } = false;
    public List<string> ValidTables { get; set; } = new();
    public List<string> InvalidTables { get; set; } = new();
    public List<string> InaccessibleTables { get; set; } = new();
    public Dictionary<string, string> TableSuggestions { get; set; } = new();
}

/// <summary>
/// Column validation result
/// </summary>
public class ColumnValidationResult
{
    public bool IsValid { get; set; } = false;
    public List<string> ValidColumns { get; set; } = new();
    public List<string> InvalidColumns { get; set; } = new();
    public List<string> TypeMismatchColumns { get; set; } = new();
    public Dictionary<string, string> ColumnSuggestions { get; set; } = new();
}

/// <summary>
/// Join validation result
/// </summary>
public class JoinValidationResult
{
    public bool IsValid { get; set; } = false;
    public List<string> ValidJoins { get; set; } = new();
    public List<string> InvalidJoins { get; set; } = new();
    public List<string> MissingJoinConditions { get; set; } = new();
    public List<string> JoinSuggestions { get; set; } = new();
}

/// <summary>
/// Context validation result
/// </summary>
public class ContextValidationResult
{
    public bool IsValid { get; set; } = false;
    public List<string> MissingContext { get; set; } = new();
    public List<string> RecommendedFilters { get; set; } = new();
    public List<string> BusinessRuleViolations { get; set; } = new();
}

/// <summary>
/// Access validation result
/// </summary>
public class AccessValidationResult
{
    public bool IsValid { get; set; } = false;
    public List<string> AccessibleTables { get; set; } = new();
    public List<string> RestrictedTables { get; set; } = new();
    public List<string> RequiredPermissions { get; set; } = new();
}

/// <summary>
/// Sensitivity validation result
/// </summary>
public class SensitivityValidationResult
{
    public bool IsValid { get; set; } = false;
    public List<string> SensitiveColumns { get; set; } = new();
    public List<string> ExposureRisks { get; set; } = new();
    public List<string> RecommendedMasking { get; set; } = new();
}

/// <summary>
/// Aggregation validation result
/// </summary>
public class AggregationValidationResult
{
    public bool IsValid { get; set; } = false;
    public List<string> ValidAggregations { get; set; } = new();
    public List<string> InvalidAggregations { get; set; } = new();
    public List<string> MissingGroupBy { get; set; } = new();
    public List<string> AggregationSuggestions { get; set; } = new();
}

/// <summary>
/// SQL elements extraction result
/// </summary>
public class SqlElementsResult
{
    public List<string> Tables { get; set; } = new();
    public List<string> Columns { get; set; } = new();
    public List<string> Joins { get; set; } = new();
    public List<string> WhereConditions { get; set; } = new();
    public List<string> Aggregations { get; set; } = new();
    public List<string> OrderBy { get; set; } = new();
    public List<string> GroupBy { get; set; } = new();
}

/// <summary>
/// Enhanced validation request
/// </summary>
public class EnhancedValidationRequest
{
    [Required]
    public string Sql { get; set; } = string.Empty;
    
    [Required]
    public string OriginalQuery { get; set; } = string.Empty;
    
    public string? Context { get; set; }
    public string? UserId { get; set; }
    public bool EnableSelfCorrection { get; set; } = true;
    public bool EnableDryRun { get; set; } = true;
    public ValidationLevel ValidationLevel { get; set; } = ValidationLevel.Standard;
    public List<string> SkipValidationTypes { get; set; } = new();
}

/// <summary>
/// Enhanced validation response
/// </summary>
public class EnhancedValidationResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public EnhancedSemanticValidationResult? ValidationResult { get; set; }
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Validation level enumeration
/// </summary>
public enum ValidationLevel
{
    Basic,      // Security validation only
    Standard,   // Security + semantic validation
    Comprehensive, // All validations including business logic
    Strict      // All validations with strict thresholds
}

/// <summary>
/// Dry-run execution request
/// </summary>
public class DryRunExecutionRequest
{
    [Required]
    public string Sql { get; set; } = string.Empty;
    
    public int MaxRowsToAnalyze { get; set; } = 1000;
    public TimeSpan MaxExecutionTime { get; set; } = TimeSpan.FromSeconds(30);
    public bool AnalyzePerformance { get; set; } = true;
    public bool GenerateExecutionPlan { get; set; } = false;
}

/// <summary>
/// Self-correction configuration
/// </summary>
public class SelfCorrectionConfiguration
{
    public bool EnableSelfCorrection { get; set; } = true;
    public int MaxCorrectionAttempts { get; set; } = 3;
    public double MinImprovementThreshold { get; set; } = 0.1;
    public TimeSpan CorrectionTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public List<string> CorrectionStrategies { get; set; } = new() { "semantic", "schema", "business_logic" };
}

/// <summary>
/// Validation metrics for monitoring and optimization
/// </summary>
public class ValidationMetrics
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public TimeSpan ValidationDuration { get; set; } = TimeSpan.Zero;
    public int TotalValidations { get; set; } = 0;
    public int SuccessfulValidations { get; set; } = 0;
    public int SelfCorrectionAttempts { get; set; } = 0;
    public int SuccessfulSelfCorrections { get; set; } = 0;
    public double AverageValidationScore { get; set; } = 0.0;
    public Dictionary<string, int> ValidationTypeMetrics { get; set; } = new();
}
