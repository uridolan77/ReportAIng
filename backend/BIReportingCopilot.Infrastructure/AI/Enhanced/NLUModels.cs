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

// =============================================================================
// SEMANTIC MODELS
// =============================================================================

/// <summary>
/// Semantic structure
/// </summary>
public class SemanticStructure
{
    public List<SemanticNode> Nodes { get; set; } = new();
    public List<SemanticRelation> Relations { get; set; } = new();
    public SyntacticStructure SyntacticStructure { get; set; } = new();
    public List<SemanticRole> SemanticRoles { get; set; } = new();
    public double ParseConfidence { get; set; }
}

/// <summary>
/// Semantic node
/// </summary>
public class SemanticNode
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public double Confidence { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Semantic relation
/// </summary>
public class SemanticRelation
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string SourceNodeId { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Syntactic structure
/// </summary>
public class SyntacticStructure
{
    public List<SyntacticToken> Tokens { get; set; } = new();
    public List<SyntacticDependency> Dependencies { get; set; } = new();
    public string ParseTree { get; set; } = string.Empty;
}

/// <summary>
/// Syntactic token
/// </summary>
public class SyntacticToken
{
    public string Text { get; set; } = string.Empty;
    public string PartOfSpeech { get; set; } = string.Empty;
    public string Lemma { get; set; } = string.Empty;
    public int Position { get; set; }
    public List<string> Features { get; set; } = new();
}

/// <summary>
/// Syntactic dependency
/// </summary>
public class SyntacticDependency
{
    public string Relation { get; set; } = string.Empty;
    public int HeadIndex { get; set; }
    public int DependentIndex { get; set; }
}

/// <summary>
/// Semantic role
/// </summary>
public class SemanticRole
{
    public string Role { get; set; } = string.Empty;
    public string Argument { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public double Confidence { get; set; }
}

// =============================================================================
// INTENT MODELS
// =============================================================================

/// <summary>
/// Intent analysis
/// </summary>
public class IntentAnalysis
{
    public string PrimaryIntent { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<IntentCandidate> AlternativeIntents { get; set; } = new();
    public IntentHierarchy IntentHierarchy { get; set; } = new();
    public List<IntentModifier> Modifiers { get; set; } = new();
    public Dictionary<string, object> IntentParameters { get; set; } = new();
}

/// <summary>
/// Intent candidate
/// </summary>
public class IntentCandidate
{
    public string Intent { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> SupportingFeatures { get; set; } = new();
}

/// <summary>
/// Intent hierarchy
/// </summary>
public class IntentHierarchy
{
    public string Domain { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subcategory { get; set; } = string.Empty;
    public string SpecificIntent { get; set; } = string.Empty;
}

/// <summary>
/// Intent modifier
/// </summary>
public class IntentModifier
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

/// <summary>
/// Intent frequency
/// </summary>
public class IntentFrequency
{
    public string Intent { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public double Percentage { get; set; }
}

// =============================================================================
// ENTITY MODELS
// =============================================================================

/// <summary>
/// Entity analysis
/// </summary>
public class EntityAnalysis
{
    public List<ExtractedEntity> Entities { get; set; } = new();
    public List<EntityRelation> EntityRelations { get; set; } = new();
    public List<string> MissingEntities { get; set; } = new();
    public double OverallConfidence { get; set; }
    public EntityResolutionResult ResolutionResult { get; set; } = new();
}

/// <summary>
/// Extracted entity
/// </summary>
public class ExtractedEntity
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string NormalizedValue { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public double Confidence { get; set; }
    public List<EntityAttribute> Attributes { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Entity relation
/// </summary>
public class EntityRelation
{
    public string SourceEntityId { get; set; } = string.Empty;
    public string TargetEntityId { get; set; } = string.Empty;
    public string RelationType { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

/// <summary>
/// Entity attribute
/// </summary>
public class EntityAttribute
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

/// <summary>
/// Entity resolution result
/// </summary>
public class EntityResolutionResult
{
    public List<ResolvedEntity> ResolvedEntities { get; set; } = new();
    public List<string> UnresolvedEntities { get; set; } = new();
    public double ResolutionRate { get; set; }
}

/// <summary>
/// Resolved entity
/// </summary>
public class ResolvedEntity
{
    public string EntityId { get; set; } = string.Empty;
    public string ResolvedValue { get; set; } = string.Empty;
    public string KnowledgeBaseId { get; set; } = string.Empty;
    public double ResolutionConfidence { get; set; }
    public Dictionary<string, object> ResolvedAttributes { get; set; } = new();
}

/// <summary>
/// Entity annotation for training
/// </summary>
public class EntityAnnotation
{
    public string EntityType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
}

// =============================================================================
// CONTEXT MODELS
// =============================================================================

/// <summary>
/// Contextual analysis
/// </summary>
public class ContextualAnalysis
{
    public double ContextualRelevance { get; set; }
    public List<ContextualCue> ContextualCues { get; set; } = new();
    public ConversationContext ConversationContext { get; set; } = new();
    public TemporalContext TemporalContext { get; set; } = new();
    public UserContext UserContext { get; set; } = new();
    public List<ContextualInference> Inferences { get; set; } = new();
}

/// <summary>
/// Contextual cue
/// </summary>
public class ContextualCue
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Strength { get; set; }
    public string Source { get; set; } = string.Empty;
}

/// <summary>
/// Conversation context
/// </summary>
public class ConversationContext
{
    public List<ConversationTurn> RecentTurns { get; set; } = new();
    public string CurrentTopic { get; set; } = string.Empty;
    public List<string> ActiveEntities { get; set; } = new();
    public Dictionary<string, object> ConversationState { get; set; } = new();
}

/// <summary>
/// Conversation turn
/// </summary>
public class ConversationTurn
{
    public string TurnId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public List<string> Entities { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Temporal context
/// </summary>
public class TemporalContext
{
    public DateTime QueryTime { get; set; }
    public List<TemporalReference> TemporalReferences { get; set; } = new();
    public TimeZoneInfo UserTimeZone { get; set; } = TimeZoneInfo.Local;
    public BusinessCalendar BusinessCalendar { get; set; } = new();
}

/// <summary>
/// Temporal reference
/// </summary>
public class TemporalReference
{
    public string Expression { get; set; } = string.Empty;
    public DateTime ResolvedDateTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public TemporalType Type { get; set; }
    public double Confidence { get; set; }
}

/// <summary>
/// Business calendar
/// </summary>
public class BusinessCalendar
{
    public List<DateTime> Holidays { get; set; } = new();
    public List<DayOfWeek> BusinessDays { get; set; } = new();
    public TimeSpan BusinessHoursStart { get; set; }
    public TimeSpan BusinessHoursEnd { get; set; }
}

/// <summary>
/// User context
/// </summary>
public class UserContext
{
    public string UserId { get; set; } = string.Empty;
    public UserProfile UserProfile { get; set; } = new();
    public List<string> UserPreferences { get; set; } = new();
    public UserBehaviorPattern BehaviorPattern { get; set; } = new();
    public Dictionary<string, object> CustomAttributes { get; set; } = new();
}

/// <summary>
/// User profile
/// </summary>
public class UserProfile
{
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string ExperienceLevel { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public Dictionary<string, object> Preferences { get; set; } = new();
}

/// <summary>
/// User behavior pattern
/// </summary>
public class UserBehaviorPattern
{
    public List<string> FrequentQueries { get; set; } = new();
    public List<string> PreferredMetrics { get; set; } = new();
    public List<string> CommonTimeRanges { get; set; } = new();
    public string CommunicationStyle { get; set; } = string.Empty;
}

/// <summary>
/// Contextual inference
/// </summary>
public class ContextualInference
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> SupportingEvidence { get; set; } = new();
}

/// <summary>
/// Conversation state
/// </summary>
public class ConversationState
{
    public string SessionId { get; set; } = string.Empty;
    public string CurrentTopic { get; set; } = string.Empty;
    public List<string> ActiveEntities { get; set; } = new();
    public Dictionary<string, object> StateVariables { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Temporal type enumeration
/// </summary>
public enum TemporalType
{
    Absolute,
    Relative,
    Duration,
    Recurring
}

// =============================================================================
// ANALYTICS MODELS
// =============================================================================

/// <summary>
/// NLU model metadata
/// </summary>
public class NLUModelMetadata
{
    public int TrainingDataCount { get; set; }
    public string? Domain { get; set; }
    public DateTime TrainingDate { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
    public ModelPerformanceMetrics PerformanceMetrics { get; set; } = new();
}

/// <summary>
/// Model performance metrics
/// </summary>
public class ModelPerformanceMetrics
{
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public Dictionary<string, Dictionary<string, int>> ConfusionMatrix { get; set; } = new();
}

/// <summary>
/// NLU analytics
/// </summary>
public class NLUAnalytics
{
    public TimeSpan Period { get; set; }
    public int TotalQueries { get; set; }
    public double AverageConfidence { get; set; }
    public Dictionary<string, int> IntentDistribution { get; set; } = new();
    public Dictionary<string, int> EntityDistribution { get; set; } = new();
    public Dictionary<string, int> LanguageDistribution { get; set; } = new();
    public Dictionary<string, double> PerformanceMetrics { get; set; } = new();
    public ErrorAnalysis ErrorAnalysis { get; set; } = new();
    public List<string> ImprovementSuggestions { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Error analysis
/// </summary>
public class ErrorAnalysis
{
    public int TotalErrors { get; set; }
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    public List<string> CommonErrorPatterns { get; set; } = new();
}

/// <summary>
/// Conversation analysis
/// </summary>
public class ConversationAnalysis
{
    public string UserId { get; set; } = string.Empty;
    public TimeSpan AnalysisWindow { get; set; }
    public int TotalInteractions { get; set; }
    public double AverageQueryLength { get; set; }
    public List<IntentFrequency> CommonIntents { get; set; } = new();
    public ConversationFlow ConversationFlow { get; set; } = new();
    public UserPreferences UserPreferences { get; set; } = new();
    public EngagementMetrics EngagementMetrics { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Conversation flow
/// </summary>
public class ConversationFlow
{
    public int AverageConversationLength { get; set; }
    public Dictionary<string, string> CommonTransitions { get; set; } = new();
    public List<string> ConversationPatterns { get; set; } = new();
}

/// <summary>
/// User preferences
/// </summary>
public class UserPreferences
{
    public List<string> PreferredQueryTypes { get; set; } = new();
    public List<string> PreferredTimeRanges { get; set; } = new();
    public List<string> PreferredMetrics { get; set; } = new();
    public string CommunicationStyle { get; set; } = string.Empty;
    public string TechnicalLevel { get; set; } = string.Empty;
}

/// <summary>
/// Engagement metrics
/// </summary>
public class EngagementMetrics
{
    public TimeSpan AverageSessionDuration { get; set; }
    public double QueriesPerSession { get; set; }
    public double SuccessfulQueryRate { get; set; }
    public double UserSatisfactionScore { get; set; }
    public double ReturnUserRate { get; set; }
}

// =============================================================================
// ENUMERATIONS
// =============================================================================

/// <summary>
/// Recommendation type enumeration
/// </summary>
public enum RecommendationType
{
    QueryOptimization,
    IntentClarification,
    EntityResolution,
    ContextualImprovement,
    PerformanceEnhancement
}

/// <summary>
/// Recommendation priority enumeration
/// </summary>
public enum RecommendationPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Domain analysis
/// </summary>
public class DomainAnalysis
{
    public string Domain { get; set; } = string.Empty;
    public double DomainConfidence { get; set; }
    public List<string> DomainIndicators { get; set; } = new();
    public Dictionary<string, double> DomainScores { get; set; } = new();
    public List<DomainConcept> DomainConcepts { get; set; } = new();
}

/// <summary>
/// Domain concept
/// </summary>
public class DomainConcept
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Relevance { get; set; }
}
