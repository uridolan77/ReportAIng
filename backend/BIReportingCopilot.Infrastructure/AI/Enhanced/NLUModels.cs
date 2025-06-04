using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.Performance;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// NLU configuration
/// </summary>
public class NLUConfiguration
{
    public bool EnableAdvancedNLU { get; set; } = true;
    public bool EnableMultilingualSupport { get; set; } = true;
    public bool EnableContextualAnalysis { get; set; } = true;
    public bool EnableDomainAdaptation { get; set; } = true;
    public double MinimumConfidenceThreshold { get; set; } = 0.7;
    public int MaxConversationHistory { get; set; } = 10;
    public List<string> SupportedLanguages { get; set; } = new() { "en", "es", "fr", "de" };
    public Dictionary<string, double> ComponentWeights { get; set; } = new()
    {
        ["intent"] = 0.4,
        ["entity"] = 0.3,
        ["context"] = 0.3
    };
}

/// <summary>
/// NLU analysis context
/// </summary>
public class NLUAnalysisContext
{
    public string Query { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public ConversationContext? ConversationContext { get; set; }
    public DateTime Timestamp { get; set; }
    public string Language { get; set; } = "en";
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Advanced NLU result
/// </summary>
public class AdvancedNLUResult
{
    public string AnalysisId { get; set; } = string.Empty;
    public string OriginalQuery { get; set; } = string.Empty;
    public string NormalizedQuery { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public SemanticStructure SemanticStructure { get; set; } = new();
    public IntentAnalysis IntentAnalysis { get; set; } = new();
    public EntityAnalysis EntityAnalysis { get; set; } = new();
    public ContextualAnalysis ContextualAnalysis { get; set; } = new();
    public DomainAnalysis DomainAnalysis { get; set; } = new();
    public ConversationState ConversationState { get; set; } = new();
    public double ConfidenceScore { get; set; }
    public NLUProcessingMetrics ProcessingMetrics { get; set; } = new();
    public List<NLURecommendation> Recommendations { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// NLU processing metrics
/// </summary>
public class NLUProcessingMetrics
{
    public TimeSpan ProcessingTime { get; set; }
    public List<string> ComponentsUsed { get; set; } = new();
    public int CacheHits { get; set; }
    public Dictionary<string, string> ModelVersions { get; set; } = new();
}

/// <summary>
/// NLU recommendation
/// </summary>
public class NLURecommendation
{
    public RecommendationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecommendationPriority Priority { get; set; }
    public double Confidence { get; set; }
}

// All model classes have been moved to separate files for better organization:
// - NLUSemanticModels.cs: Semantic structure, nodes, relations, syntactic analysis
// - NLUIntentModels.cs: Intent analysis, classification, hierarchy
// - NLUEntityModels.cs: Entity extraction, resolution, attributes
// - NLUContextModels.cs: Contextual analysis, conversation, temporal, user context
// - NLUAnalyticsModels.cs: Analytics, performance metrics, error analysis
// - NLUComponents.cs: Component implementations and processors
