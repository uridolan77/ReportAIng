namespace BIReportingCopilot.Core.Models.Analytics;

/// <summary>
/// Enums for user preference learning
/// </summary>
public enum PreferenceConfidence
{
    Low,
    Medium,
    High,
    VeryHigh
}

public enum LearningStatus
{
    New,
    Learning,
    Established,
    Expert
}

public enum PreferenceType
{
    IntentType,
    Domain,
    TemplateStyle,
    ResponseFormat,
    Complexity,
    TimeRange,
    Tables,
    Metrics
}

/// <summary>
/// Model for capturing user interactions for learning
/// </summary>
public class UserInteraction
{
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string UserQuestion { get; set; } = string.Empty;
    public string IntentClassified { get; set; } = string.Empty;
    public string DomainClassified { get; set; } = string.Empty;
    public string TemplateUsed { get; set; } = string.Empty;
    public List<string> TablesAccessed { get; set; } = new();
    public bool WasSuccessful { get; set; }
    public int? UserRating { get; set; }
    public string? UserFeedback { get; set; }
    public int ProcessingTimeMs { get; set; }
    public decimal ConfidenceScore { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> AdditionalContext { get; set; } = new();
}

/// <summary>
/// Personalized prompt recommendations for a user
/// </summary>
public class PersonalizedPromptRecommendations
{
    public string UserId { get; set; } = string.Empty;
    public string UserQuestion { get; set; } = string.Empty;
    public List<RecommendedTemplate> RecommendedTemplates { get; set; } = new();
    public List<RecommendedTable> RecommendedTables { get; set; } = new();
    public List<RecommendedMetric> RecommendedMetrics { get; set; } = new();
    public PersonalizedPromptStyle PreferredStyle { get; set; } = new();
    public List<string> SuggestedImprovements { get; set; } = new();
    public PreferenceConfidence OverallConfidence { get; set; }
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// User's complete preference profile
/// </summary>
public class UserPreferenceProfile
{
    public string UserId { get; set; } = string.Empty;
    public LearningStatus LearningStatus { get; set; }
    public int TotalInteractions { get; set; }
    public DateTime FirstInteraction { get; set; }
    public DateTime LastInteraction { get; set; }
    
    // Intent Preferences
    public List<PreferenceItem<string>> PreferredIntentTypes { get; set; } = new();
    public List<PreferenceItem<string>> PreferredDomains { get; set; } = new();
    
    // Template Preferences
    public List<PreferenceItem<string>> PreferredTemplates { get; set; } = new();
    public PersonalizedPromptStyle PreferredStyle { get; set; } = new();
    
    // Data Preferences
    public List<PreferenceItem<string>> PreferredTables { get; set; } = new();
    public List<PreferenceItem<string>> PreferredMetrics { get; set; } = new();
    public List<PreferenceItem<string>> PreferredTimeRanges { get; set; } = new();
    
    // Behavioral Patterns
    public TimeSpan AverageSessionDuration { get; set; }
    public List<TimeOfDay> PreferredQueryTimes { get; set; } = new();
    public decimal AverageQueryComplexity { get; set; }
    public string PreferredResponseFormat { get; set; } = string.Empty;
    
    // Performance Preferences
    public int PreferredMaxProcessingTime { get; set; }
    public decimal MinimumAcceptableConfidence { get; set; }
    
    // Personalization Settings
    public string LanguagePreference { get; set; } = "en";
    public string TimeZone { get; set; } = "UTC";
    public Dictionary<string, object> CustomPreferences { get; set; } = new();
    
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// User preference update request
/// </summary>
public class UserPreferenceUpdate
{
    public List<string>? PreferredIntentTypes { get; set; }
    public List<string>? PreferredDomains { get; set; }
    public List<string>? PreferredTemplates { get; set; }
    public PersonalizedPromptStyle? PreferredStyle { get; set; }
    public string? PreferredResponseFormat { get; set; }
    public string? LanguagePreference { get; set; }
    public string? TimeZone { get; set; }
    public int? PreferredMaxProcessingTime { get; set; }
    public decimal? MinimumAcceptableConfidence { get; set; }
    public Dictionary<string, object>? CustomPreferences { get; set; }
}

/// <summary>
/// Similar user based on behavior patterns
/// </summary>
public class SimilarUser
{
    public string UserId { get; set; } = string.Empty;
    public decimal SimilarityScore { get; set; }
    public List<string> CommonIntentTypes { get; set; } = new();
    public List<string> CommonDomains { get; set; } = new();
    public List<string> CommonTemplates { get; set; } = new();
    public string SimilarityReason { get; set; } = string.Empty;
    public DateTime LastActivity { get; set; }
}

/// <summary>
/// Personalized template ranking for a user
/// </summary>
public class PersonalizedTemplateRanking
{
    public string TemplateKey { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public decimal PersonalizedScore { get; set; }
    public decimal BaseSuccessRate { get; set; }
    public decimal UserSpecificSuccessRate { get; set; }
    public int UserUsageCount { get; set; }
    public PreferenceConfidence Confidence { get; set; }
    public List<string> ReasoningFactors { get; set; } = new();
    public DateTime LastUsed { get; set; }
}

/// <summary>
/// Query prediction based on user patterns
/// </summary>
public class QueryPrediction
{
    public string PredictedQuery { get; set; } = string.Empty;
    public string IntentType { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public decimal PredictionConfidence { get; set; }
    public List<string> SuggestedTables { get; set; } = new();
    public List<string> SuggestedMetrics { get; set; } = new();
    public string RecommendedTemplate { get; set; } = string.Empty;
    public List<string> PredictionReasons { get; set; } = new();
}

/// <summary>
/// User behavior insights for analytics
/// </summary>
public class UserBehaviorInsights
{
    public string UserId { get; set; } = string.Empty;
    public TimeSpan AnalysisWindow { get; set; }
    
    // Activity Patterns
    public int TotalQueries { get; set; }
    public decimal AverageQueriesPerDay { get; set; }
    public List<HourlyActivity> ActivityByHour { get; set; } = new();
    public List<DailyActivity> ActivityByDay { get; set; } = new();
    
    // Query Patterns
    public List<QueryPattern> TopQueryPatterns { get; set; } = new();
    public decimal QueryComplexityTrend { get; set; }
    public List<string> EvolvingInterests { get; set; } = new();
    
    // Performance Patterns
    public decimal SuccessRateTrend { get; set; }
    public decimal SatisfactionTrend { get; set; }
    public decimal EngagementScore { get; set; }
    
    // Learning Progress
    public LearningStatus CurrentLearningStatus { get; set; }
    public decimal LearningVelocity { get; set; }
    public List<string> MasteredAreas { get; set; } = new();
    public List<string> LearningAreas { get; set; } = new();
    
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Aggregated user preference trends
/// </summary>
public class UserPreferenceTrends
{
    public TimeSpan AnalysisWindow { get; set; }
    public int TotalUsers { get; set; }
    
    // Popular Preferences
    public List<TrendItem> PopularIntentTypes { get; set; } = new();
    public List<TrendItem> PopularDomains { get; set; } = new();
    public List<TrendItem> PopularTemplates { get; set; } = new();
    public List<TrendItem> PopularTables { get; set; } = new();
    
    // Emerging Trends
    public List<TrendItem> EmergingIntentTypes { get; set; } = new();
    public List<TrendItem> EmergingDomains { get; set; } = new();
    public List<TrendItem> EmergingQueryPatterns { get; set; } = new();
    
    // User Segmentation
    public List<UserSegment> UserSegments { get; set; } = new();
    
    // Behavioral Insights
    public decimal AverageSessionDuration { get; set; }
    public decimal AverageQueryComplexity { get; set; }
    public List<TimeOfDay> PeakUsageTimes { get; set; } = new();
    
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Preference learning model performance metrics
/// </summary>
public class PreferenceLearningMetrics
{
    public string ModelVersion { get; set; } = string.Empty;
    public DateTime LastTrainingDate { get; set; }
    public int TrainingDataPoints { get; set; }
    
    // Model Performance
    public decimal PredictionAccuracy { get; set; }
    public decimal RecommendationRelevance { get; set; }
    public decimal UserSatisfactionImprovement { get; set; }
    
    // Coverage Metrics
    public decimal UserCoverage { get; set; }
    public decimal PreferenceCoverage { get; set; }
    public int ActiveLearningUsers { get; set; }
    
    // Quality Metrics
    public decimal ModelConfidence { get; set; }
    public decimal RecommendationDiversity { get; set; }
    public decimal PersonalizationEffectiveness { get; set; }
    
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// User learning status information
/// </summary>
public class UserLearningStatus
{
    public string UserId { get; set; } = string.Empty;
    public LearningStatus Status { get; set; }
    public int InteractionCount { get; set; }
    public decimal LearningProgress { get; set; } // 0-1 scale
    public List<string> LearnedPreferences { get; set; } = new();
    public List<string> LearningAreas { get; set; } = new();
    public PreferenceConfidence OverallConfidence { get; set; }
    public DateTime NextLearningOpportunity { get; set; }
    public bool IsPersonalizationActive { get; set; }
}

/// <summary>
/// Personalized prompt context
/// </summary>
public class PersonalizedPromptContext
{
    public string UserId { get; set; } = string.Empty;
    public object BaseContext { get; set; } = new();
    public List<string> PersonalizedTemplates { get; set; } = new();
    public List<string> PersonalizedTables { get; set; } = new();
    public List<string> PersonalizedMetrics { get; set; } = new();
    public PersonalizedPromptStyle Style { get; set; } = new();
    public Dictionary<string, object> PersonalizationFactors { get; set; } = new();
    public PreferenceConfidence PersonalizationConfidence { get; set; }
}

/// <summary>
/// Supporting models
/// </summary>
public class PreferenceItem<T>
{
    public T Value { get; set; } = default!;
    public decimal Weight { get; set; }
    public PreferenceConfidence Confidence { get; set; }
    public int UsageCount { get; set; }
    public DateTime LastUsed { get; set; }
}

public class PersonalizedPromptStyle
{
    public string Verbosity { get; set; } = "Standard"; // Concise, Standard, Detailed
    public string TechnicalLevel { get; set; } = "Intermediate"; // Beginner, Intermediate, Advanced
    public bool IncludeExplanations { get; set; } = true;
    public bool IncludeExamples { get; set; } = true;
    public string PreferredFormat { get; set; } = "Structured"; // Structured, Conversational, Technical
}

public class RecommendedTemplate
{
    public string TemplateKey { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public decimal RecommendationScore { get; set; }
    public List<string> ReasoningFactors { get; set; } = new();
}

public class RecommendedTable
{
    public string TableName { get; set; } = string.Empty;
    public decimal RelevanceScore { get; set; }
    public List<string> ReasoningFactors { get; set; } = new();
}

public class RecommendedMetric
{
    public string MetricName { get; set; } = string.Empty;
    public decimal RelevanceScore { get; set; }
    public List<string> ReasoningFactors { get; set; } = new();
}

public class TimeOfDay
{
    public int Hour { get; set; }
    public decimal ActivityScore { get; set; }
}

public class HourlyActivity
{
    public int Hour { get; set; }
    public int QueryCount { get; set; }
    public decimal SuccessRate { get; set; }
}

public class DailyActivity
{
    public DayOfWeek Day { get; set; }
    public int QueryCount { get; set; }
    public decimal SuccessRate { get; set; }
}

public class QueryPattern
{
    public string Pattern { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public decimal SuccessRate { get; set; }
    public string IntentType { get; set; } = string.Empty;
}

public class TrendItem
{
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Percentage { get; set; }
    public decimal GrowthRate { get; set; }
}

public class UserSegment
{
    public string SegmentName { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public decimal Percentage { get; set; }
    public List<string> CharacteristicFeatures { get; set; } = new();
}
