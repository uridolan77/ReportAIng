using BIReportingCopilot.Core.Models.BusinessContext;

namespace BIReportingCopilot.Core.DTOs;

// ============================================================================
// TRANSPARENCY API REQUEST/RESPONSE DTOS - MINIMAL SET FOR PROCESSFLOW SYSTEM
// ============================================================================

/// <summary>
/// Request for analyzing prompt construction - USED BY ProcessFlow system
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
/// Request for prompt optimization suggestions - LEGACY COMPATIBILITY
/// NOTE: This DTO is kept for frontend compatibility but optimization is now handled by ProcessFlow
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
/// Legacy DTO for prompt construction steps - COMPATIBILITY ONLY
/// NOTE: Use ProcessFlowStep for new implementations
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
/// Legacy DTO for transparency traces - COMPATIBILITY ONLY
/// NOTE: Use ProcessFlowSession for new implementations
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
/// Legacy DTO for optimization suggestions - COMPATIBILITY ONLY
/// NOTE: Use ProcessFlow analytics for new implementations
/// </summary>
public class OptimizationSuggestionDto
{
    public string SuggestionId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public int EstimatedTokenSaving { get; set; }
    public double EstimatedPerformanceGain { get; set; }
    public double EstimatedCostSaving { get; set; }
    public string Implementation { get; set; } = string.Empty;
    public List<string> RequiredChanges { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
}

/// <summary>
/// Legacy DTO for alternative options - COMPATIBILITY ONLY
/// NOTE: Use ProcessFlow analytics for new implementations
/// </summary>
public class AlternativeOptionDto
{
    public string OptionId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
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
/// Legacy DTO for transparency metrics - COMPATIBILITY ONLY
/// NOTE: Use ProcessFlow metrics for new implementations
/// </summary>
public class TransparencyMetricsDto
{
    public int TotalAnalyses { get; set; }
    public double AverageConfidence { get; set; }
    public Dictionary<string, int> ConfidenceDistribution { get; set; } = new();
    public Dictionary<string, int> TopIntentTypes { get; set; } = new();
    public Dictionary<string, double> OptimizationImpact { get; set; } = new();
    public Dictionary<string, int> AlternativeUsage { get; set; } = new();
    public object? TimeRange { get; set; }
}

// NOTE: All other transparency DTOs have been removed and replaced by ProcessFlow models
// ProcessFlow system uses:
// - ProcessFlowSession for transparency traces
// - ProcessFlowStep for step-by-step analysis  
// - ProcessFlowTransparency for token usage and AI metrics
// - ProcessFlowLog for detailed logging
//
// Legacy DTOs removed:
// - PromptConstructionTraceDto -> ProcessFlowSession
// - PromptConstructionStep -> ProcessFlowStep
// - TransparencyMetricsDto -> ProcessFlow analytics
// - TokenUsageAnalyticsDto -> ProcessFlowTransparency
// - ConfidenceBreakdown -> ProcessFlowStep confidence data
// - AlternativeOptionDto -> ProcessFlow analytics
// - OptimizationSuggestionDto -> ProcessFlow performance insights
// - All other transparency-related DTOs -> ProcessFlow equivalents
