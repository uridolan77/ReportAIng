using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Advanced NLU analysis result with enhanced features
/// </summary>
public class AdvancedNLUResult : NLUResult
{
    public List<SemanticRelation> SemanticRelations { get; set; } = new();
    public List<ConceptMapping> ConceptMappings { get; set; } = new();
    public AdvancedContextAnalysis AdvancedContext { get; set; } = new();
    public List<QueryVariation> QueryVariations { get; set; } = new();
    public double SemanticComplexity { get; set; }
    public List<string> RequiredCapabilities { get; set; } = new();
}

/// <summary>
/// Comprehensive result of NLU analysis
/// </summary>
public class NLUResult
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
/// Intent classification analysis
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
/// Entity extraction and analysis
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
/// Contextual analysis results
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
/// NLU analysis context
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
/// Conversation analysis results
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

// =============================================================================
// MISSING MODEL CLASSES FOR COMPILATION
// =============================================================================

/// <summary>
/// Query intelligence analysis result
/// </summary>
public class QueryIntelligenceResult
{
    public string AnalysisId { get; set; } = Guid.NewGuid().ToString();
    public string Query { get; set; } = string.Empty;
    public AdvancedNLUResult NLUResult { get; set; } = new();
    public QueryOptimizationResult OptimizationResult { get; set; } = new();
    public List<IntelligentQuerySuggestion> Suggestions { get; set; } = new();
    public double OverallScore { get; set; }
    public double IntelligenceScore { get; set; }
    public List<QueryInsight> Insights { get; set; } = new();
    public List<IntelligenceRecommendation> Recommendations { get; set; } = new();
    public QueryPerformanceAnalysis PerformanceAnalysis { get; set; } = new();
    public QuerySemanticAnalysis SemanticAnalysis { get; set; } = new();
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Query insight for intelligence analysis
/// </summary>
public class QueryInsight
{
    public string InsightId { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; } = 0.8;
    public string Recommendation { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}



/// <summary>
/// Query performance analysis
/// </summary>
public class QueryPerformanceAnalysis
{
    public string AnalysisId { get; set; } = Guid.NewGuid().ToString();
    public double Confidence { get; set; } = 0.8;
    public TimeSpan EstimatedExecutionTime { get; set; }
    public List<string> PerformanceIssues { get; set; } = new();
    public List<string> OptimizationSuggestions { get; set; } = new();
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Query semantic analysis
/// </summary>
public class QuerySemanticAnalysis
{
    public string AnalysisId { get; set; } = Guid.NewGuid().ToString();
    public List<string> ExtractedEntities { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public string Intent { get; set; } = string.Empty;
    public double Confidence { get; set; } = 0.8;
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Intelligent query suggestion with AI insights
/// </summary>
public class IntelligentQuerySuggestion
{
    public string SuggestionId { get; set; } = Guid.NewGuid().ToString();
    public string QueryText { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Relevance { get; set; }
    public double Confidence { get; set; }
    public List<string> Benefits { get; set; } = new();
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Text property (alias for QueryText for compatibility)
    /// </summary>
    public string Text
    {
        get => QueryText;
        set => QueryText = value;
    }
}

/// <summary>
/// Query assistance information
/// </summary>
public class QueryAssistance
{
    public string AssistanceId { get; set; } = Guid.NewGuid().ToString();
    public List<string> Completions { get; set; } = new();
    public List<string> Corrections { get; set; } = new();
    public List<IntelligentQuerySuggestion> Suggestions { get; set; } = new();
    public string Context { get; set; } = string.Empty;

    /// <summary>
    /// Auto complete suggestions (alias for Completions)
    /// </summary>
    public List<string> AutoComplete
    {
        get => Completions;
        set => Completions = value;
    }

    /// <summary>
    /// Explanations for the suggestions
    /// </summary>
    public List<string> Explanations { get; set; } = new();

    /// <summary>
    /// Confidence score for the assistance
    /// </summary>
    public double Confidence { get; set; } = 0.8;

    /// <summary>
    /// Autocomplete suggestions (alias for Completions)
    /// </summary>
    public List<string> AutocompleteSuggestions
    {
        get => Completions;
        set => Completions = value;
    }

    /// <summary>
    /// Performance hints for the query
    /// </summary>
    public List<string> PerformanceHints { get; set; } = new();
}

/// <summary>
/// Query analysis result
/// </summary>
public class QueryAnalysisResult
{
    public string AnalysisId { get; set; } = Guid.NewGuid().ToString();
    public string Query { get; set; } = string.Empty;
    public string OriginalQuery { get; set; } = string.Empty;
    public double Complexity { get; set; }
    public List<string> Issues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    // Additional properties for schema contextualization
    public List<string> BusinessTerms { get; set; } = new();
    public string QueryCategory { get; set; } = string.Empty;
    public List<string> ExtractedEntities { get; set; } = new();
    public string Intent { get; set; } = string.Empty;
}



/// <summary>
/// Concept mapping for domain understanding
/// </summary>
public class ConceptMapping
{
    public string Concept { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public List<string> Synonyms { get; set; } = new();
    public double Confidence { get; set; }
}

/// <summary>
/// Advanced context analysis
/// </summary>
public class AdvancedContextAnalysis
{
    public string ContextId { get; set; } = Guid.NewGuid().ToString();
    public List<string> ContextFactors { get; set; } = new();
    public Dictionary<string, object> ContextData { get; set; } = new();
    public double ContextRelevance { get; set; }
}

/// <summary>
/// Query variation for alternative phrasings
/// </summary>
public class QueryVariation
{
    public string VariationText { get; set; } = string.Empty;
    public double Similarity { get; set; }
    public string VariationType { get; set; } = string.Empty;
}

// =============================================================================
// MISSING ENUMS AND CLASSES FOR COMPILATION
// =============================================================================

/// <summary>
/// Intent category enumeration
/// </summary>
public enum IntentCategory
{
    Query,
    Analysis,
    Reporting,
    Dashboard,
    Configuration,
    Help,
    Unknown,
    Comparison,
    Other
}

/// <summary>
/// Intent modifier enumeration
/// </summary>
public enum IntentModifier
{
    None,
    Urgent,
    Detailed,
    Summary,
    Comparison,
    Trend,
    Forecast
}

/// <summary>
/// Complexity level enumeration
/// </summary>
public enum ComplexityLevel
{
    Simple,
    Medium,
    Complex,
    Advanced
}

/// <summary>
/// Conversation state enumeration
/// </summary>
public enum ConversationStateEnum
{
    Initial,
    InProgress,
    Clarifying,
    Executing,
    Completed,
    Error
}

/// <summary>
/// Conversation state class for tracking conversation context
/// </summary>
public class ConversationState
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public string CurrentTopic { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public ConversationStateEnum State { get; set; } = ConversationStateEnum.Initial;
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// NLU processing metrics
/// </summary>
public class NLUProcessingMetrics
{
    public TimeSpan ProcessingTime { get; set; }
    public int TokenCount { get; set; }
    public double ConfidenceScore { get; set; }
    public string ModelVersion { get; set; } = string.Empty;

    // Properties expected by Infrastructure services
    public List<string> ComponentsUsed { get; set; } = new();
    public int CacheHits { get; set; }
}

/// <summary>
/// NLU recommendation
/// </summary>
public class NLURecommendation
{
    public string RecommendationId { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();

    // Properties expected by Infrastructure services
    public string Message
    {
        get => Description;
        set => Description = value;
    }

    public string Priority { get; set; } = "Medium";
    public string ActionSuggestion { get; set; } = string.Empty;
}

/// <summary>
/// Conversation context
/// </summary>
public class ConversationContext
{
    public string ConversationId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public List<string> MessageHistory { get; set; } = new();
    public Dictionary<string, object> ContextData { get; set; } = new();
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    // Properties expected by Infrastructure services
    public List<string> RecentQueries { get; set; } = new();
    public string CurrentTopic { get; set; } = string.Empty;
    public List<string> MentionedEntities { get; set; } = new();
}

/// <summary>
/// Temporal context
/// </summary>
public class TemporalContext
{
    public DateTime ReferenceTime { get; set; } = DateTime.UtcNow;
    public TimeSpan TimeWindow { get; set; }
    public string TimeZone { get; set; } = "UTC";
    public List<string> TemporalExpressions { get; set; } = new();

    // Properties expected by Infrastructure services
    /// <summary>
    /// References to related temporal entities
    /// </summary>
    public List<string> References { get; set; } = new();

    /// <summary>
    /// Time frame description
    /// </summary>
    public string TimeFrame { get; set; } = string.Empty;

    /// <summary>
    /// Whether this is a relative time reference
    /// </summary>
    public bool IsRelative { get; set; }
}

/// <summary>
/// User context
/// </summary>
public class UserContext
{
    // Original properties
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public Dictionary<string, object> Preferences { get; set; } = new();
    public List<string> RecentQueries { get; set; } = new();

    // Properties expected by Infrastructure services
    public List<string> PreferredTables { get; set; } = new();
    public List<string> CommonFilters { get; set; } = new();
    public string Domain { get; set; } = string.Empty;
    public List<QueryPattern> RecentPatterns { get; set; } = new();
    public Dictionary<string, object> SessionData { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Contextual cue
/// </summary>
public class ContextualCue
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Relevance { get; set; }
    public string Source { get; set; } = string.Empty;
}

/// <summary>
/// Contextual inference
/// </summary>
public class ContextualInference
{
    public string InferenceId { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> SupportingEvidence { get; set; } = new();
}

/// <summary>
/// Context factor
/// </summary>
public class ContextFactor
{
    public string FactorId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public object Value { get; set; } = new();
    public double Weight { get; set; }
    public string Source { get; set; } = string.Empty;
}

/// <summary>
/// Domain concept
/// </summary>
public class DomainConcept
{
    public string ConceptId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public List<string> Synonyms { get; set; } = new();
    public Dictionary<string, object> Properties { get; set; } = new();

    // Properties expected by Infrastructure services
    /// <summary>
    /// Category of the domain concept
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// Relevance score for the concept
    /// </summary>
    public double Relevance { get; set; }
}

/// <summary>
/// Business context
/// </summary>
public class BusinessContext
{
    public string ContextId { get; set; } = Guid.NewGuid().ToString();
    public string Domain { get; set; } = string.Empty;
    public List<DomainConcept> Concepts { get; set; } = new();
    public Dictionary<string, object> BusinessRules { get; set; } = new();
    public List<string> KPIs { get; set; } = new();

    // Properties expected by Infrastructure services
    /// <summary>
    /// Business context description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Business tables in this context
    /// </summary>
    public List<Business.BusinessTable> Tables { get; set; } = new();

    /// <summary>
    /// Business terms in this context
    /// </summary>
    public List<Business.BusinessTerm> Terms { get; set; } = new();

    /// <summary>
    /// Business relationships in this context
    /// </summary>
    public List<Business.BusinessRelationship> Relationships { get; set; } = new();

    /// <summary>
    /// When this context was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // Properties expected by Infrastructure services
    public string Industry { get; set; } = string.Empty;
    public List<string> BusinessProcesses { get; set; } = new();
}

/// <summary>
/// Conversation recommendation
/// </summary>
public class ConversationRecommendation
{
    public string RecommendationId { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string> SuggestedActions { get; set; } = new();
    public double Priority { get; set; }
}

/// <summary>
/// Entity annotation
/// </summary>
public class EntityAnnotation
{
    public string AnnotationId { get; set; } = Guid.NewGuid().ToString();
    public string EntityId { get; set; } = string.Empty;
    public string AnnotationType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

// =============================================================================
// VECTOR SEARCH AND SEMANTIC MODELS
// =============================================================================

/// <summary>
/// Semantic search result
/// </summary>
public class SemanticSearchResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Text { get; set; } = string.Empty;
    public double Similarity { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public float[] Embedding { get; set; } = Array.Empty<float>();

    // Properties expected by Infrastructure services
    public string EmbeddingId
    {
        get => Id;
        set => Id = value;
    }

    public string OriginalQuery
    {
        get => Text;
        set => Text = value;
    }

    public string SqlQuery { get; set; } = string.Empty;
    public object? CachedResponse { get; set; }

    public double SimilarityScore
    {
        get => Similarity;
        set => Similarity = value;
    }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastAccessed { get; set; } = DateTime.UtcNow;
    public int AccessCount { get; set; } = 0;
}

/// <summary>
/// Batch embedding result
/// </summary>
public class BatchEmbeddingResult
{
    public string Text { get; set; } = string.Empty;
    public float[] Embedding { get; set; } = Array.Empty<float>();
    public bool Success { get; set; }
    public string? Error { get; set; }

    /// <summary>
    /// Time taken to process this embedding
    /// </summary>
    public TimeSpan ProcessingTime { get; set; }
}

/// <summary>
/// Conversation turn for NLU processing
/// </summary>
public class ConversationTurn
{
    public string TurnId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Response { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Context { get; set; } = new();
}

// =============================================================================
// MISSING AI RESULT MODELS
// =============================================================================

// SemanticAnalysisResult definition moved to IAIService.cs interface - removed duplicate to avoid ambiguous references

// SemanticEntity already defined in AIModels.cs - removed duplicate

/// <summary>
/// Query classification result
/// </summary>
public class QueryClassificationResult
{
    public string QueryType { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> Categories { get; set; } = new();
    public Dictionary<string, double> Scores { get; set; } = new();
}

// QueryOptimizationResult already defined in QueryOptimization.cs - removed duplicate

/// <summary>
/// Dashboard result
/// </summary>
public class DashboardResult
{
    public string DashboardId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "Success";
    public List<DashboardWidget> Widgets { get; set; } = new();
    public List<WidgetResult> WidgetResults { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Widget result
/// </summary>
public class WidgetResult
{
    public string WidgetId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = "Success";
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

// DashboardWidget already defined in Visualization.cs - removed duplicate

/// <summary>
/// Dashboard request
/// </summary>
public class DashboardRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> DataSources { get; set; } = new();
    public Dictionary<string, object> Preferences { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public bool IsPublic { get; set; } = false;
}

/// <summary>
/// AI request model
/// </summary>
public class AIRequest
{
    public string RequestId { get; set; } = Guid.NewGuid().ToString();
    public string Prompt { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Properties expected by Infrastructure services
    /// <summary>
    /// Maximum number of tokens to generate
    /// </summary>
    public int MaxTokens { get; set; } = 1000;

    /// <summary>
    /// Temperature for response generation
    /// </summary>
    public double Temperature { get; set; } = 0.7;

    /// <summary>
    /// System message for AI context
    /// </summary>
    public string? SystemMessage { get; set; }

    /// <summary>
    /// Timeout in seconds for the request
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}

/// <summary>
/// AI response model
/// </summary>
public class AIResponse
{
    public string ResponseId { get; set; } = Guid.NewGuid().ToString();
    public string RequestId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Properties expected by Infrastructure services
    /// <summary>
    /// Error information (alias for ErrorMessage)
    /// </summary>
    public string? Error
    {
        get => ErrorMessage;
        set => ErrorMessage = value;
    }

    /// <summary>
    /// Number of tokens used in the response
    /// </summary>
    public int TokensUsed { get; set; }

    /// <summary>
    /// Model used for the response
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Provider used for the response
    /// </summary>
    public string? Provider { get; set; }
}

/// <summary>
/// AI provider metrics
/// </summary>
public class AIProviderMetrics
{
    public string ProviderId { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public double AverageResponseTime { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Properties expected by Infrastructure services
    public string ProviderName
    {
        get => ProviderId;
        set => ProviderId = value;
    }

    public bool IsAvailable { get; set; } = true;
    public double ResponseTimeMs
    {
        get => AverageResponseTime;
        set => AverageResponseTime = value;
    }

    public int RequestCount
    {
        get => TotalRequests;
        set => TotalRequests = value;
    }

    public int ErrorCount
    {
        get => FailedRequests;
        set => FailedRequests = value;
    }

    public DateTime LastUsed
    {
        get => LastUpdated;
        set => LastUpdated = value;
    }

    public Dictionary<string, object> Configuration { get; set; } = new();
}
