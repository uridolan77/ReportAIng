using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Comprehensive result of advanced NLU analysis
/// </summary>
public class AdvancedNLUResult
{
    public string AnalysisId { get; set; } = Guid.NewGuid().ToString();
    public string OriginalQuery { get; set; } = string.Empty;
    public string NormalizedQuery { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    
    /// <summary>
    /// Semantic structure analysis
    /// </summary>
    public SemanticStructure SemanticStructure { get; set; } = new();
    
    /// <summary>
    /// Intent classification results
    /// </summary>
    public IntentAnalysis IntentAnalysis { get; set; } = new();
    
    /// <summary>
    /// Entity extraction results
    /// </summary>
    public EntityAnalysis EntityAnalysis { get; set; } = new();
    
    /// <summary>
    /// Contextual analysis results
    /// </summary>
    public ContextualAnalysis ContextualAnalysis { get; set; } = new();
    
    /// <summary>
    /// Domain-specific analysis
    /// </summary>
    public DomainAnalysis DomainAnalysis { get; set; } = new();
    
    /// <summary>
    /// Conversation state and history
    /// </summary>
    public ConversationState ConversationState { get; set; } = new();
    
    /// <summary>
    /// Overall confidence score (0.0 to 1.0)
    /// </summary>
    public double ConfidenceScore { get; set; }
    
    /// <summary>
    /// Processing performance metrics
    /// </summary>
    public NLUProcessingMetrics ProcessingMetrics { get; set; } = new();

    /// <summary>
    /// Recommendations for query improvement
    /// </summary>
    public List<NLURecommendation> Recommendations { get; set; } = new();

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Error { get; set; }

    // Additional compatibility properties
    public double ProcessingTimeMs => ProcessingMetrics?.ProcessingTime.TotalMilliseconds ?? 0;
}

/// <summary>
/// Semantic structure of the query
/// </summary>
public class SemanticStructure
{
    public List<SemanticToken> Tokens { get; set; } = new();
    public List<SemanticPhrase> Phrases { get; set; } = new();
    public List<SemanticRelation> Relations { get; set; } = new();
    public QueryComplexityAnalysis Complexity { get; set; } = new();
    public List<string> KeyConcepts { get; set; } = new();

    // Additional properties for compatibility
    public List<SemanticNode> Nodes { get; set; } = new();
    public double ParseConfidence { get; set; } = 0.85;

    // Additional properties for NLUService compatibility
    public QueryComplexityAnalysis ComplexityAnalysis { get; set; } = new();
}

/// <summary>
/// Unified Intent classification analysis
/// Combines properties from both NLUModels.cs and AdvancedNLU.cs versions
/// </summary>
public class IntentAnalysis
{
    // Primary properties - using string for interface compatibility
    public string PrimaryIntent { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<IntentCandidate> AlternativeIntents { get; set; } = new();

    // Extended properties from both versions
    public IntentCategory Category { get; set; } = IntentCategory.Query;
    public List<string> SubIntents { get; set; } = new();
    public Dictionary<string, object> IntentMetadata { get; set; } = new();

    // Compatibility properties
    public List<IntentClassification> SecondaryIntents { get; set; } = new();
    public double OverallConfidence { get; set; }
    public List<string> IntentHierarchy { get; set; } = new();
    public TimeSpan ProcessingTime { get; set; }

    // Additional properties from NLUModels version
    public List<IntentModifier> Modifiers { get; set; } = new();
    public Dictionary<string, object> IntentParameters { get; set; } = new();
}

/// <summary>
/// Unified Entity extraction and analysis
/// Combines properties from both NLUModels.cs and AdvancedNLU.cs versions
/// </summary>
public class EntityAnalysis
{
    // Core properties
    public List<ExtractedEntity> Entities { get; set; } = new();
    public List<string> MissingEntities { get; set; } = new();
    public double OverallConfidence { get; set; }

    // Relationship properties - supporting both naming conventions
    public List<EntityRelation> Relations { get; set; } = new();
    public List<EntityRelationship> EntityRelations { get; set; } = new();

    // Extended properties
    public Dictionary<string, List<ExtractedEntity>> EntitiesByType { get; set; } = new();
    public EntityResolutionResult ResolutionResult { get; set; } = new();
}

/// <summary>
/// Entity relationship for compatibility
/// </summary>
public class EntityRelationship
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string SourceEntityId { get; set; } = string.Empty;
    public string TargetEntityId { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

/// <summary>
/// Entity resolution result
/// </summary>
public class EntityResolutionResult
{
    public int ResolvedEntities { get; set; }
    public int UnresolvedEntities { get; set; }
    public List<string> AmbiguousEntities { get; set; } = new();
    public double ResolutionConfidence { get; set; }
}

/// <summary>
/// Unified Contextual analysis results
/// Combines properties from both NLUModels.cs and AdvancedNLU.cs versions
/// </summary>
public class ContextualAnalysis
{
    // Core properties
    public double ContextualRelevance { get; set; }
    public ConversationContext ConversationContext { get; set; } = new();
    public TemporalContext TemporalContext { get; set; } = new();
    public UserContext UserContext { get; set; } = new();

    // Clue properties - supporting both naming conventions
    public List<ContextualClue> ContextualClues { get; set; } = new();
    public List<ContextualCue> ContextualCues { get; set; } = new();

    // Extended properties from both versions
    public List<string> ImplicitAssumptions { get; set; } = new();
    public List<ContextualInference> Inferences { get; set; } = new();

    // Compatibility properties
    public List<ContextFactor> ContextFactors { get; set; } = new();
    public ConversationFlow ConversationFlow { get; set; } = new();
    public double OverallConfidence { get; set; }
    public TimeSpan ProcessingTime { get; set; }

    // Additional properties for NLUService compatibility
    public decimal Relevance { get; set; }
}

/// <summary>
/// Domain-specific analysis
/// </summary>
public class DomainAnalysis
{
    public string PrimaryDomain { get; set; } = string.Empty;
    public List<string> SecondaryDomains { get; set; } = new();
    public double DomainConfidence { get; set; }
    public List<DomainConcept> DomainConcepts { get; set; } = new();
    public BusinessContext BusinessContext { get; set; } = new();

    // Additional property for compatibility
    public string Domain { get; set; } = string.Empty;
}

/// <summary>
/// NLU analysis context - Unified version
/// </summary>
public class NLUAnalysisContext
{
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string Language { get; set; } = "en";
    public List<string> ConversationHistory { get; set; } = new();
    public Dictionary<string, object> UserPreferences { get; set; } = new();
    public SchemaMetadata? SchemaContext { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Additional properties for compatibility
    public string Query { get; set; } = string.Empty;
    public ConversationContext? ConversationContext { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Additional properties for NLUService compatibility
    public SchemaMetadata? Schema { get; set; }
}

/// <summary>
/// Query improvement suggestions
/// </summary>
public class QueryImprovement
{
    public string OriginalQuery { get; set; } = string.Empty;
    public string ImprovedQuery { get; set; } = string.Empty;
    public List<ImprovementSuggestion> Suggestions { get; set; } = new();
    public double ImprovementScore { get; set; }
    public string Reasoning { get; set; } = string.Empty;
}

/// <summary>
/// Unified Conversation analysis results
/// Combines properties from both NLUModels.cs and AdvancedNLU.cs versions
/// </summary>
public class ConversationAnalysis
{
    public string UserId { get; set; } = string.Empty;
    public TimeSpan AnalysisWindow { get; set; }
    public int TotalInteractions { get; set; }
    public double AverageQueryLength { get; set; }

    // Intent properties - supporting both types
    public List<string> CommonIntents { get; set; } = new();
    public List<IntentFrequency> CommonIntentFrequencies { get; set; } = new();

    public ConversationFlow ConversationFlow { get; set; } = new();
    public EngagementMetrics EngagementMetrics { get; set; } = new();

    // User preferences - supporting both types
    public UserPreferences UserPreferences { get; set; } = new();
    public NLUUserPreferences NLUUserPreferences { get; set; } = new();

    // Recommendations - supporting both types
    public List<ConversationRecommendation> Recommendations { get; set; } = new();
    public List<string> RecommendationStrings { get; set; } = new();

    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string? Error { get; set; }
}

/// <summary>
/// NLU training data
/// </summary>
public class NLUTrainingData
{
    public string Query { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public List<EntityAnnotation> Entities { get; set; } = new();
    public string? ExpectedSql { get; set; }
    public string? Domain { get; set; }
    public double Quality { get; set; } = 1.0;
}

/// <summary>
/// NLU performance metrics
/// </summary>
public class NLUMetrics
{
    public int TotalAnalyses { get; set; }
    public double AverageConfidence { get; set; }
    public double AverageProcessingTime { get; set; }
    public Dictionary<string, double> IntentAccuracy { get; set; } = new();
    public Dictionary<string, double> EntityAccuracy { get; set; } = new();
    public int CacheHitRate { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Additional properties for NLUService compatibility
    public int TotalQueries { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}



/// <summary>
/// Expected entity for training
/// </summary>
public class ExpectedEntity
{
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
}





/// <summary>
/// Individual improvement suggestion
/// </summary>
public class ImprovementSuggestion
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Example { get; set; } = string.Empty;
    public double Impact { get; set; }
}



/// <summary>
/// Intent frequency analysis
/// </summary>
public class IntentFrequency
{
    public string Intent { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Conversation flow analysis
/// </summary>
public class ConversationFlow
{
    public int AverageConversationLength { get; set; }
    public Dictionary<string, string> CommonTransitions { get; set; } = new();
    public List<string> ConversationPatterns { get; set; } = new();
    public double Coherence { get; set; }
}

/// <summary>
/// User preferences for NLU
/// </summary>
public class NLUUserPreferences
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
    public double SessionDuration { get; set; }
    public int QueriesPerSession { get; set; }
    public double SuccessRate { get; set; }
    public double SatisfactionScore { get; set; }
}

/// <summary>
/// NLU configuration
/// </summary>
public class NLUConfiguration
{
    public double ConfidenceThreshold { get; set; } = 0.7;
    public int MaxAlternativeIntents { get; set; } = 3;
    public bool EnableMultilingualSupport { get; set; } = true;
    public bool EnableContextualAnalysis { get; set; } = true;
    public bool EnableDomainAdaptation { get; set; } = true;
    public TimeSpan ConversationContextWindow { get; set; } = TimeSpan.FromMinutes(30);
    public Dictionary<string, object> ModelParameters { get; set; } = new();

    // Additional properties for NLUService compatibility
    public bool EnableSemanticAnalysis { get; set; } = true;
    public bool EnableContextTracking { get; set; } = true;
    public bool EnableLearning { get; set; } = true;
    public int CacheExpirationMinutes { get; set; } = 30;
    public int MaxContextQueries { get; set; } = 10;
}

// Supporting classes
public class SemanticToken
{
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int Position { get; set; }
}

public class SemanticPhrase
{
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<SemanticToken> Tokens { get; set; } = new();
}

public class SemanticRelation
{
    public string Subject { get; set; } = string.Empty;
    public string Predicate { get; set; } = string.Empty;
    public string Object { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

/// <summary>
/// Semantic node for compatibility
/// </summary>
public class SemanticNode
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> Relations { get; set; } = new();
}

public class QueryComplexityAnalysis
{
    public ComplexityLevel Level { get; set; } = ComplexityLevel.Simple;
    public double Score { get; set; }
    public List<string> ComplexityFactors { get; set; } = new();

    // Additional properties for compatibility
    public List<ComplexityFactor> Factors { get; set; } = new();
    public List<string> SimplificationOpportunities { get; set; } = new();
}

public class IntentCandidate
{
    public string Intent { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Additional properties expected by Infrastructure services
    public string CandidateId { get; set; } = Guid.NewGuid().ToString();
    public double Score { get; set; }
    public List<string> MatchedKeywords { get; set; } = new();
    public Dictionary<string, object> Evidence { get; set; } = new();
    public string Reasoning { get; set; } = string.Empty;
}

/// <summary>
/// Intent classification result
/// </summary>
public class IntentClassification
{
    public string Intent { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Category { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ExtractedEntity
{
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public Dictionary<string, object> Attributes { get; set; } = new();

    // Additional properties for interface compatibility
    public string? SchemaReference { get; set; }
    public string? EntityId { get; set; }

    // Additional properties for NLUService compatibility
    public int Position { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class EntityRelation
{
    public string SourceEntity { get; set; } = string.Empty;
    public string TargetEntity { get; set; } = string.Empty;
    public string RelationType { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

public class ContextualClue
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public decimal Relevance { get; set; }
    public string Source { get; set; } = string.Empty;

    // Additional properties for NLUService compatibility
    public double Confidence { get; set; }
}

/// <summary>
/// Context factor for analysis
/// </summary>
public class ContextFactor
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Source { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// <summary>
/// Unified User context information
/// Combines properties from both NLUModels.cs and AdvancedNLU.cs versions
/// </summary>
public class UserContext
{
    // Core properties
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, object> Preferences { get; set; } = new();

    // Profile properties - supporting both structures
    public string Role { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public UserProfile UserProfile { get; set; } = new();

    // Query/Behavior properties
    public List<string> RecentQueries { get; set; } = new();
    public List<string> UserPreferences { get; set; } = new();
    public UserBehaviorPattern BehaviorPattern { get; set; } = new();

    // Extended properties from AdvancedNLU version
    public List<string> PreferredTables { get; set; } = new();
    public List<string> CommonFilters { get; set; } = new();
    public List<QueryPattern> RecentPatterns { get; set; } = new();
    public string Domain { get; set; } = string.Empty;

    // Timestamps
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Additional properties from NLUModels version
    public Dictionary<string, object> CustomAttributes { get; set; } = new();
    public Dictionary<string, object> SessionData { get; set; } = new();
}

/// <summary>
/// Unified Conversation context
/// Combines properties from both NLUModels.cs and AdvancedNLU.cs versions
/// </summary>
public class ConversationContext
{
    // Core properties
    public string CurrentTopic { get; set; } = string.Empty;

    // Query/Turn properties - supporting both naming conventions
    public List<string> RecentQueries { get; set; } = new();
    public List<ConversationTurn> RecentTurns { get; set; } = new();

    // Entity properties - supporting both naming conventions
    public List<string> MentionedEntities { get; set; } = new();
    public List<string> ActiveEntities { get; set; } = new();

    // State properties
    public Dictionary<string, object> ConversationState { get; set; } = new();
}

/// <summary>
/// Unified Temporal context
/// Combines properties from both NLUModels.cs and AdvancedNLU.cs versions
/// </summary>
public class TemporalContext
{
    // Core properties
    public DateTime QueryTime { get; set; } = DateTime.UtcNow;

    // Reference properties - supporting both types
    public List<string> TemporalReferences { get; set; } = new();
    public List<TemporalReference> TemporalReferenceObjects { get; set; } = new();

    // Extended properties
    public string TimeFrame { get; set; } = string.Empty;
    public TimeZoneInfo UserTimeZone { get; set; } = TimeZoneInfo.Local;
    public BusinessCalendar BusinessCalendar { get; set; } = new();

    // Additional properties for NLUService compatibility
    public List<string> References { get; set; } = new();
    public bool IsRelative { get; set; }
}

public class DomainConcept
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Relevance { get; set; }
    public List<string> RelatedConcepts { get; set; } = new();
}

public class BusinessContext
{
    public string Industry { get; set; } = string.Empty;
    public List<string> BusinessProcesses { get; set; } = new();
    public Dictionary<string, object> Metrics { get; set; } = new();
}

/// <summary>
/// Supporting model classes for unified models
/// </summary>
public class IntentModifier
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

public class ContextualCue
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Strength { get; set; }
    public string Source { get; set; } = string.Empty;
}

public class ContextualInference
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> SupportingEvidence { get; set; } = new();
}

public class ConversationTurn
{
    public string TurnId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public List<string> Entities { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class TemporalReference
{
    public string Expression { get; set; } = string.Empty;
    public DateTime ResolvedDateTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public TemporalType Type { get; set; }
    public double Confidence { get; set; }
}

public class BusinessCalendar
{
    public List<DateTime> Holidays { get; set; } = new();
    public List<DayOfWeek> BusinessDays { get; set; } = new();
    public TimeSpan BusinessHoursStart { get; set; }
    public TimeSpan BusinessHoursEnd { get; set; }
}

public class UserProfile
{
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string ExperienceLevel { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public Dictionary<string, object> Preferences { get; set; } = new();
}

public class UserBehaviorPattern
{
    public List<string> FrequentQueries { get; set; } = new();
    public List<string> PreferredMetrics { get; set; } = new();
    public List<string> CommonTimeRanges { get; set; } = new();
    public string CommunicationStyle { get; set; } = string.Empty;
}

public enum TemporalType
{
    Absolute,
    Relative,
    Duration,
    Recurring
}

/// <summary>
/// Additional supporting model classes for unified models
/// </summary>
public class IntentHierarchy
{
    public string Domain { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subcategory { get; set; } = string.Empty;
    public string SpecificIntent { get; set; } = string.Empty;
}

// IntentFrequency, ConversationFlow, NLUUserPreferences, and EngagementMetrics
// are defined in NLUModels.cs to avoid duplicates









public class ConversationRecommendation
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Priority { get; set; }
}

public class EntityAnnotation
{
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
}

public class NLURecommendation
{
    public RecommendationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecommendationPriority Priority { get; set; }
    public double Confidence { get; set; }

    // Additional properties for NLUService compatibility
    public string Message { get; set; } = string.Empty;
    public string ActionSuggestion { get; set; } = string.Empty;
}

// Enumerations
public enum IntentCategory
{
    Query,
    Command,
    Question,
    Request,
    Clarification,
    Analysis,
    Comparison,
    Other
}

public enum ComplexityLevel
{
    Simple,
    Medium,
    Complex,
    VeryComplex
}

public enum RecommendationType
{
    Clarification,
    Enhancement,
    Context,
    Performance
}

public enum RecommendationPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Processing metrics for NLU analysis
/// </summary>
public class NLUProcessingMetrics
{
    public TimeSpan ProcessingTime { get; set; }
    public List<string> ComponentsUsed { get; set; } = new();
    public int CacheHits { get; set; }
    public Dictionary<string, string> ModelVersions { get; set; } = new();
    public Dictionary<string, object> PerformanceData { get; set; } = new();
}

/// <summary>
/// Conversation state management
/// </summary>
public class ConversationState
{
    public string SessionId { get; set; } = string.Empty;
    public List<string> QueryHistory { get; set; } = new();
    public Dictionary<string, object> SessionVariables { get; set; } = new();
    public string CurrentContext { get; set; } = string.Empty;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;

    // Additional properties for compatibility
    public string CurrentTopic { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Query intelligence result combining NLU and optimization
/// </summary>
public class QueryIntelligenceResult
{
    public string QueryId { get; set; } = Guid.NewGuid().ToString();
    public string OriginalQuery { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public AdvancedNLUResult NLUResult { get; set; } = new();
    public QueryOptimizationResult OptimizationResult { get; set; } = new();
    public List<IntelligentQuerySuggestion> Suggestions { get; set; } = new();
    public QueryAssistance Assistance { get; set; } = new();
    public double OverallScore { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    // Additional properties for IntelligenceService compatibility
    public SchemaOptimizationAnalysis SchemaAnalysis { get; set; } = new();
    public List<SQLSuggestion> SQLSuggestions { get; set; } = new();
    public double IntelligenceScore { get; set; }
    public List<IntelligenceRecommendation> Recommendations { get; set; } = new();
    public QueryIntelligenceMetrics ProcessingMetrics { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Infrastructure compatibility properties
    public string Query { get; set; } = string.Empty;
    public List<QueryInsight> Insights { get; set; } = new();
    public QueryPerformanceAnalysis PerformanceAnalysis { get; set; } = new();
    public QuerySemanticAnalysis SemanticAnalysis { get; set; } = new();
}

/// <summary>
/// Intelligent query suggestion with NLU and performance insights
/// </summary>
public class IntelligentQuerySuggestion
{
    public string SuggestionId { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Relevance { get; set; }
    public double PerformanceScore { get; set; }
    public List<string> Benefits { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Additional properties for IntelligenceService compatibility
    public double Confidence { get; set; }
    public string ExpectedIntent { get; set; } = string.Empty;
    public double SchemaRelevance { get; set; }
    public string ComplexityLevel { get; set; } = string.Empty;
    public string EstimatedPerformance { get; set; } = string.Empty;

    // Additional properties expected by Infrastructure services
    public string Query { get; set; } = string.Empty;
    public List<QueryInsight> Insights { get; set; } = new();
    public QueryPerformanceAnalysis PerformanceAnalysis { get; set; } = new();
    public QuerySemanticAnalysis SemanticAnalysis { get; set; } = new();
}

/// <summary>
/// Query intelligence processing metrics
/// </summary>
public class QueryIntelligenceMetrics
{
    public TimeSpan TotalProcessingTime { get; set; }
    public TimeSpan NLUProcessingTime { get; set; }
    public TimeSpan SchemaAnalysisTime { get; set; }
    public List<string> ComponentsUsed { get; set; } = new();
    public Dictionary<string, double> PerformanceMetrics { get; set; } = new();
}

/// <summary>
/// Query insight for intelligence analysis
/// </summary>
public class QueryInsight
{
    public string InsightId { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; } = 0.8;
    public string Severity { get; set; } = "Info"; // Info, Warning, Error
    public List<string> Recommendations { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Query performance analysis result
/// </summary>
public class QueryPerformanceAnalysis
{
    public string AnalysisId { get; set; } = Guid.NewGuid().ToString();
    public double EstimatedExecutionTime { get; set; }
    public double EstimatedCost { get; set; }
    public string PerformanceCategory { get; set; } = "Medium"; // Fast, Medium, Slow
    public List<string> PerformanceWarnings { get; set; } = new();
    public List<string> OptimizationSuggestions { get; set; } = new();
    public double Confidence { get; set; } = 0.8;
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Query semantic analysis result
/// </summary>
public class QuerySemanticAnalysis
{
    public string AnalysisId { get; set; } = Guid.NewGuid().ToString();
    public string Intent { get; set; } = string.Empty;
    public List<string> Entities { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public double SemanticComplexity { get; set; } = 0.5;
    public List<string> SuggestedTables { get; set; } = new();
    public List<string> SuggestedColumns { get; set; } = new();
    public double Confidence { get; set; } = 0.8;
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Real-time query assistance
/// </summary>
public class QueryAssistance
{
    public List<string> AutocompleteSuggestions { get; set; } = new();
    public List<string> SyntaxSuggestions { get; set; } = new();
    public List<string> PerformanceHints { get; set; } = new();
    public List<string> ContextualHelp { get; set; } = new();
    public List<QueryValidation> Validations { get; set; } = new();

    // Additional compatibility properties
    public List<string> ValidationErrors => Validations
        .Where(v => v.Severity == ValidationSeverity.Error)
        .Select(v => v.Message)
        .ToList();

    // Additional properties expected by Infrastructure services
    public string SuggestionType { get; set; } = "autocomplete";
    public List<string> Suggestions { get; set; } = new();
    public List<string> AutoComplete { get; set; } = new();
    public List<string> Explanations { get; set; } = new();
    public double Confidence { get; set; } = 0.8;
}

/// <summary>
/// Query validation result
/// </summary>
public class QueryValidation
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public ValidationSeverity Severity { get; set; }
    public string? Suggestion { get; set; }
}

// UserFeedback moved to MLModels.cs to avoid duplicates

// Enumerations for new models
public enum ValidationSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

public enum FeedbackType
{
    QueryAccuracy,
    Performance,
    Suggestions,
    UserExperience,
    General
}
