using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.DTOs;

/// <summary>
/// AI Tuning settings DTO
/// </summary>
public class AITuningSettingsDto
{
    public long Id { get; set; }
    public string SettingsId { get; set; } = Guid.NewGuid().ToString();
    public string SettingKey { get; set; } = string.Empty;
    public string SettingValue { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";
    public bool IsActive { get; set; } = true;
    public TuningConfiguration Configuration { get; set; } = new();
    public List<TuningParameter> Parameters { get; set; } = new();
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Tuning configuration
/// </summary>
public class TuningConfiguration
{
    public double ConfidenceThreshold { get; set; } = 0.8;
    public int MaxRetries { get; set; } = 3;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public Dictionary<string, object> Parameters { get; set; } = new();
    public double LearningRate { get; set; } = 0.001;
    public int BatchSize { get; set; } = 32;
    public int MaxIterations { get; set; } = 1000;
    public double ConvergenceThreshold { get; set; } = 0.001;
    public bool EnableEarlyStopping { get; set; } = true;
    public int EarlyStoppingPatience { get; set; } = 10;
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

/// <summary>
/// Tuning parameter
/// </summary>
public class TuningParameter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public object Value { get; set; } = new();
    public object MinValue { get; set; } = new();
    public object MaxValue { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public bool IsOptimizable { get; set; } = true;
}

/// <summary>
/// Tuning dashboard data
/// </summary>
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
    public DateTime LastRefreshed { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Tuning validation result
/// </summary>
public class TuningValidationResult
{
    public string ValidationId { get; set; } = Guid.NewGuid().ToString();
    public bool IsValid { get; set; }
    public double ValidationScore { get; set; }
    public List<TuningValidationIssue> Issues { get; set; } = new();
    public List<ValidationRecommendation> Recommendations { get; set; } = new();
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public double ConfidenceScore { get; set; }
}

/// <summary>
/// Tuning validation issue (specific to AI tuning)
/// </summary>
public class TuningValidationIssue
{
    public string IssueId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ValidationIssueSeverity Severity { get; set; } = ValidationIssueSeverity.Warning;
    public string Category { get; set; } = string.Empty;
    public List<string> SuggestedFixes { get; set; } = new();
}

/// <summary>
/// Validation recommendation
/// </summary>
public class ValidationRecommendation
{
    public string RecommendationId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ValidationRecommendationPriority Priority { get; set; } = ValidationRecommendationPriority.Medium;
    public List<string> Actions { get; set; } = new();
    public double ExpectedImpact { get; set; }
}

/// <summary>
/// Validation issue severity
/// </summary>
public enum ValidationIssueSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// Validation recommendation priority
/// </summary>
public enum ValidationRecommendationPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Tuning optimization result
/// </summary>
public class TuningOptimizationResult
{
    public string OptimizationId { get; set; } = Guid.NewGuid().ToString();
    public TuningConfiguration OptimalConfiguration { get; set; } = new();
    public double ImprovementScore { get; set; }
    public List<string> OptimizationSteps { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan OptimizationDuration { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Optimizations { get; set; } = new();
    public double PerformanceImprovement { get; set; }
}

/// <summary>
/// Tuning analytics data
/// </summary>
public class TuningAnalyticsData
{
    public int TotalExperiments { get; set; }
    public int SuccessfulExperiments { get; set; }
    public double AverageImprovementScore { get; set; }
    public Dictionary<string, int> UsageByCategory { get; set; } = new();
    public Dictionary<string, double> PerformanceMetrics { get; set; } = new();
    public List<string> TopPerformingSettings { get; set; } = new();
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
