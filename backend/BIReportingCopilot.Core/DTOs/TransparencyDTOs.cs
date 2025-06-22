using BIReportingCopilot.Core.Models.BusinessContext;

namespace BIReportingCopilot.Core.DTOs;

// ============================================================================
// TRANSPARENCY API REQUEST/RESPONSE DTOS
// ============================================================================

/// <summary>
/// Request for analyzing prompt construction
/// </summary>
public class AnalyzePromptRequest
{
    public string UserQuery { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public Dictionary<string, object>? Context { get; set; }
    public bool IncludeAlternatives { get; set; } = true;
    public bool IncludeOptimizations { get; set; } = true;
    public int MaxTables { get; set; } = 10;
    public string? PreferredDomain { get; set; }
}

/// <summary>
/// Request for prompt optimization suggestions
/// </summary>
public class OptimizePromptRequest
{
    public string UserQuery { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? TraceId { get; set; }
    public string? CurrentPrompt { get; set; }
    public Dictionary<string, object>? Context { get; set; }
    public string[] OptimizationTypes { get; set; } = Array.Empty<string>();
    public int MaxTokens { get; set; } = 4000;
    public string Priority { get; set; } = "Balanced"; // "Performance", "Accuracy", "Balanced"
}

/// <summary>
/// Enhanced transparency report with comprehensive analysis
/// </summary>
public class EnhancedTransparencyReport
{
    public string TraceId { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string UserQuestion { get; set; } = string.Empty;
    public string IntentType { get; set; } = string.Empty;
    public PromptConstructionTraceDto PromptConstruction { get; set; } = new();
    public ConfidenceBreakdown ConfidenceAnalysis { get; set; } = new();
    public List<AlternativeOptionDto> Alternatives { get; set; } = new();
    public List<OptimizationSuggestionDto> Optimizations { get; set; } = new();
    public PerformanceMetrics Performance { get; set; } = new();
    public TokenUsageAnalysis TokenUsage { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Detailed prompt construction trace (API DTO)
/// </summary>
public class PromptConstructionTraceDto
{
    public string TraceId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string UserQuestion { get; set; } = string.Empty;
    public IntentType IntentType { get; set; }
    public List<PromptConstructionStep> Steps { get; set; } = new();
    public double OverallConfidence { get; set; }
    public int TotalTokens { get; set; }
    public string FinalPrompt { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Individual step in prompt construction
/// </summary>
public class PromptConstructionStep
{
    public int StepNumber { get; set; }
    public string StepName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public TimeSpan Duration { get; set; }
    public int TokensAdded { get; set; }
    public List<string> Alternatives { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Confidence analysis breakdown
/// </summary>
public class ConfidenceBreakdown
{
    public string AnalysisId { get; set; } = string.Empty;
    public double OverallConfidence { get; set; }
    public Dictionary<string, double> FactorBreakdown { get; set; } = new();
    public List<ConfidenceFactor> ConfidenceFactors { get; set; } = new();
    public ConfidenceEvolution? Evolution { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Individual confidence factor
/// </summary>
public class ConfidenceFactor
{
    public string FactorName { get; set; } = string.Empty;
    public double Score { get; set; }
    public double Weight { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // "Intent", "Entity", "Schema", "Template"
    public List<string> SupportingEvidence { get; set; } = new();
}

/// <summary>
/// Confidence evolution over time
/// </summary>
public class ConfidenceEvolution
{
    public List<ConfidenceDataPoint> DataPoints { get; set; } = new();
    public double InitialConfidence { get; set; }
    public double FinalConfidence { get; set; }
    public double MaxConfidence { get; set; }
    public double MinConfidence { get; set; }
    public string Trend { get; set; } = string.Empty; // "Increasing", "Decreasing", "Stable"
}

/// <summary>
/// Confidence data point for evolution tracking
/// </summary>
public class ConfidenceDataPoint
{
    public DateTime Timestamp { get; set; }
    public double Confidence { get; set; }
    public string Phase { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Alternative option for prompt construction (API DTO)
/// </summary>
public class AlternativeOptionDto
{
    public string OptionId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Template", "Context", "Schema", "Parameters"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Rationale { get; set; } = string.Empty;
    public double EstimatedImprovement { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public List<string> Pros { get; set; } = new();
    public List<string> Cons { get; set; } = new();
}

/// <summary>
/// Optimization suggestion for prompts (API DTO)
/// </summary>
public class OptimizationSuggestionDto
{
    public string SuggestionId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "Token", "Accuracy", "Performance", "Cost"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty; // "High", "Medium", "Low"
    public int EstimatedTokenSaving { get; set; }
    public double EstimatedPerformanceGain { get; set; }
    public double EstimatedCostSaving { get; set; }
    public string Implementation { get; set; } = string.Empty;
    public List<string> RequiredChanges { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
}

/// <summary>
/// Performance metrics for transparency analysis
/// </summary>
public class PerformanceMetrics
{
    public TimeSpan TotalProcessingTime { get; set; }
    public TimeSpan AnalysisTime { get; set; }
    public TimeSpan PromptBuildTime { get; set; }
    public TimeSpan ValidationTime { get; set; }
    public int TotalApiCalls { get; set; }
    public double CostEstimate { get; set; }
    public Dictionary<string, double> PhaseTimings { get; set; } = new();
    public Dictionary<string, int> ResourceUsage { get; set; } = new();
}

/// <summary>
/// Token usage analysis
/// </summary>
public class TokenUsageAnalysis
{
    public int TotalTokens { get; set; }
    public int PromptTokens { get; set; }
    public int ResponseTokens { get; set; }
    public int ContextTokens { get; set; }
    public int SchemaTokens { get; set; }
    public int ExampleTokens { get; set; }
    public double TokenEfficiency { get; set; }
    public Dictionary<string, int> TokenBreakdown { get; set; } = new();
    public List<TokenOptimizationOpportunity> OptimizationOpportunities { get; set; } = new();
}

/// <summary>
/// Token optimization opportunity
/// </summary>
public class TokenOptimizationOpportunity
{
    public string Area { get; set; } = string.Empty;
    public int CurrentTokens { get; set; }
    public int OptimizedTokens { get; set; }
    public int Savings { get; set; }
    public string Strategy { get; set; } = string.Empty;
    public double Impact { get; set; }
}

/// <summary>
/// Transparency metrics and analytics (API DTO)
/// </summary>
public class TransparencyMetricsDto
{
    public int TotalAnalyses { get; set; }
    public double AverageConfidence { get; set; }
    public Dictionary<string, int> ConfidenceDistribution { get; set; } = new();
    public Dictionary<string, int> TopIntentTypes { get; set; } = new();
    public Dictionary<string, double> OptimizationImpact { get; set; } = new();
    public Dictionary<string, int> AlternativeUsage { get; set; } = new();
    public PerformanceMetrics AveragePerformance { get; set; } = new();
    public object? TimeRange { get; set; }
}

/// <summary>
/// Progressive build result for prompt construction (API DTO)
/// </summary>
public class ProgressiveBuildResultDto
{
    public string FinalPrompt { get; set; } = string.Empty;
    public List<PromptBuildStep> BuildSteps { get; set; } = new();
    public int TotalTokens { get; set; }
    public double ConfidenceScore { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Individual step in progressive prompt building
/// </summary>
public class PromptBuildStep
{
    public string StepName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int TokensAdded { get; set; }
    public double Confidence { get; set; }
    public TimeSpan Duration { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Dashboard-specific transparency metrics
/// </summary>
public class TransparencyDashboardMetricsDto
{
    public int TotalTraces { get; set; }
    public double AverageConfidence { get; set; }
    public List<OptimizationSuggestionDto> TopOptimizations { get; set; } = new();
    public List<ConfidenceTrendDto> ConfidenceTrends { get; set; } = new();
    public List<ModelUsageDto> UsageByModel { get; set; } = new();
}

/// <summary>
/// Transparency settings for a user
/// </summary>
public class TransparencySettingsDto
{
    public bool EnableDetailedLogging { get; set; } = true;
    public double ConfidenceThreshold { get; set; } = 0.7;
    public int RetentionDays { get; set; } = 30;
    public bool EnableOptimizationSuggestions { get; set; } = true;
}

/// <summary>
/// Export transparency data request
/// </summary>
public class ExportTransparencyRequest
{
    public List<string>? TraceIds { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Format { get; set; } = "json"; // json, csv, excel
    public string? UserId { get; set; }
}

/// <summary>
/// Transparency trace summary DTO
/// </summary>
public class TransparencyTraceDto
{
    public string TraceId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserQuestion { get; set; } = string.Empty;
    public string IntentType { get; set; } = string.Empty;
    public double OverallConfidence { get; set; }
    public int TotalTokens { get; set; }
    public bool Success { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Detailed transparency trace DTO
/// </summary>
public class TransparencyTraceDetailDto : TransparencyTraceDto
{
    public List<PromptConstructionStepDto> Steps { get; set; } = new();
    public string? FinalPrompt { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Prompt construction step DTO
/// </summary>
public class PromptConstructionStepDto
{
    public string Id { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public int StepOrder { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool Success { get; set; }
    public int TokensAdded { get; set; }
    public double Confidence { get; set; }
    public string? Content { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}

/// <summary>
/// Confidence trend data point
/// </summary>
public class ConfidenceTrendDto
{
    public DateTime Date { get; set; }
    public double Confidence { get; set; }
    public int TraceCount { get; set; }
}

/// <summary>
/// Model usage statistics
/// </summary>
public class ModelUsageDto
{
    public string Model { get; set; } = string.Empty;
    public int Count { get; set; }
    public double AverageConfidence { get; set; }
    public int TotalTokens { get; set; }
}

/// <summary>
/// Token usage analytics
/// </summary>
public class TokenUsageAnalyticsDto
{
    public int TotalTokensUsed { get; set; }
    public double AverageTokensPerQuery { get; set; }
    public Dictionary<string, int> TokensByIntentType { get; set; } = new();
    public List<TokenTrendDto> TokenTrends { get; set; } = new();
}

/// <summary>
/// Token trend data point
/// </summary>
public class TokenTrendDto
{
    public DateTime Date { get; set; }
    public int TokensUsed { get; set; }
    public int QueryCount { get; set; }
}

/// <summary>
/// Transparency performance metrics
/// </summary>
public class TransparencyPerformanceDto
{
    public double AverageProcessingTime { get; set; }
    public int TotalQueries { get; set; }
    public double SuccessRate { get; set; }
    public Dictionary<string, double> PerformanceByIntentType { get; set; } = new();
}
