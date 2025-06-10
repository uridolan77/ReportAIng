using System.ComponentModel.DataAnnotations;
using BIReportingCopilot.Core.DTOs;

namespace BIReportingCopilot.Core.Models.DTOs;

/// <summary>
/// AI Tuning dashboard data
/// </summary>
public class TuningDashboardData
{
    // Dashboard statistics properties
    public int TotalTables { get; set; } // Added missing property
    public int TotalColumns { get; set; } // Added missing property
    public int TotalPatterns { get; set; } // Added missing property
    public int TotalGlossaryTerms { get; set; } // Added missing property
    public int ActivePromptTemplates { get; set; } // Added missing property
    public List<string> RecentlyUpdatedTables { get; set; } = new(); // Added missing property
    public List<string> MostUsedPatterns { get; set; } = new(); // Added missing property
    public Dictionary<string, int> PatternUsageStats { get; set; } = new(); // Added missing property

    // Original properties
    public AITuningSettingsDto Settings { get; set; } = new();
    public List<TuningMetric> Metrics { get; set; } = new();
    public List<TuningRecommendation> Recommendations { get; set; } = new();
    public TuningPerformanceData Performance { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

// AITuningSettingsDto moved to BIReportingCopilot.Core.DTOs namespace
// This duplicate definition has been removed to avoid conflicts

/// <summary>
/// Tuning configuration
/// </summary>
public class TuningConfiguration
{
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
/// Tuning metric
/// </summary>
public class TuningMetric
{
    public string MetricId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public double Target { get; set; }
    public string Unit { get; set; } = string.Empty;
    public TuningMetricType Type { get; set; } = TuningMetricType.Performance;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public List<double> History { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Tuning recommendation
/// </summary>
public class TuningRecommendation
{
    public string RecommendationId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TuningRecommendationType Type { get; set; } = TuningRecommendationType.ParameterAdjustment;
    public TuningRecommendationPriority Priority { get; set; } = TuningRecommendationPriority.Medium;
    public double ExpectedImprovement { get; set; }
    public List<string> Actions { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public bool IsImplemented { get; set; } = false;
}

/// <summary>
/// Tuning performance data
/// </summary>
public class TuningPerformanceData
{
    public double OverallScore { get; set; }
    public double AccuracyScore { get; set; }
    public double LatencyScore { get; set; }
    public double ThroughputScore { get; set; }
    public double ResourceUtilizationScore { get; set; }
    public List<PerformanceDataPoint> PerformanceHistory { get; set; } = new();
    public Dictionary<string, double> MetricBreakdown { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

// PerformanceDataPoint already defined in PerformanceModels.cs

/// <summary>
/// Tuning experiment
/// </summary>
public class TuningExperiment
{
    public string ExperimentId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public TuningConfiguration Configuration { get; set; } = new();
    public ExperimentStatus Status { get; set; } = ExperimentStatus.Created;
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public List<TuningMetric> Results { get; set; } = new();
    public string CreatedBy { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Tuning optimization result
/// </summary>
public class TuningOptimizationResult
{
    public string OptimizationId { get; set; } = Guid.NewGuid().ToString();
    public TuningConfiguration OptimalConfiguration { get; set; } = new();
    public double ImprovementScore { get; set; }
    public List<TuningMetric> OptimizedMetrics { get; set; } = new();
    public List<string> OptimizationSteps { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan OptimizationDuration { get; set; }
}

/// <summary>
/// Tuning validation result
/// </summary>
public class TuningValidationResult
{
    public string ValidationId { get; set; } = Guid.NewGuid().ToString();
    public bool IsValid { get; set; }
    public double ValidationScore { get; set; }
    public List<ValidationIssue> Issues { get; set; } = new();
    public List<ValidationRecommendation> Recommendations { get; set; } = new();
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Validation issue
/// </summary>
public class ValidationIssue
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
/// Tuning analytics data
/// </summary>
public class TuningAnalyticsData
{
    public int TotalExperiments { get; set; }
    public int SuccessfulExperiments { get; set; }
    public double AverageImprovementScore { get; set; }
    public List<TuningTrend> Trends { get; set; } = new();
    public Dictionary<string, double> MetricAverages { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Tuning trend
/// </summary>
public class TuningTrend
{
    public string MetricName { get; set; } = string.Empty;
    public TrendDirection Direction { get; set; } = TrendDirection.Stable;
    public double TrendStrength { get; set; }
    public List<TrendDataPoint> DataPoints { get; set; } = new();
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

// Enumerations
public enum TuningMetricType
{
    Performance,
    Accuracy,
    Latency,
    Throughput,
    ResourceUtilization,
    Quality
}

public enum TuningRecommendationType
{
    ParameterAdjustment,
    ConfigurationChange,
    ArchitectureModification,
    DataPreprocessing,
    FeatureEngineering
}

public enum TuningRecommendationPriority
{
    Low,
    Medium,
    High,
    Critical
}

public enum ExperimentStatus
{
    Created,
    Running,
    Completed,
    Failed,
    Cancelled
}

public enum ValidationIssueSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

public enum ValidationRecommendationPriority
{
    Low,
    Medium,
    High,
    Critical
}
