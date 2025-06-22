using BIReportingCopilot.Core.Models.Analytics;

namespace BIReportingCopilot.Core.Interfaces.Analytics;

/// <summary>
/// Service for learning and applying user preferences for personalized prompt generation
/// </summary>
public interface IUserPreferenceLearningService
{
    /// <summary>
    /// Learn from user interaction and update preferences
    /// </summary>
    Task LearnFromInteractionAsync(UserInteraction interaction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get personalized prompt recommendations for a user
    /// </summary>
    Task<PersonalizedPromptRecommendations> GetPersonalizedRecommendationsAsync(string userId, string userQuestion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user's preference profile
    /// </summary>
    Task<UserPreferenceProfile> GetUserPreferenceProfileAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update user preferences manually
    /// </summary>
    Task UpdateUserPreferencesAsync(string userId, UserPreferenceUpdate preferences, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get similar users based on behavior patterns
    /// </summary>
    Task<List<SimilarUser>> GetSimilarUsersAsync(string userId, int maxResults = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get personalized template ranking for a user
    /// </summary>
    Task<List<PersonalizedTemplateRanking>> GetPersonalizedTemplateRankingAsync(string userId, string intentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Predict user's likely next query patterns
    /// </summary>
    Task<List<QueryPrediction>> PredictNextQueriesAsync(string userId, int maxPredictions = 5, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user behavior insights for analytics
    /// </summary>
    Task<UserBehaviorInsights> GetUserBehaviorInsightsAsync(string userId, TimeSpan? timeWindow = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get aggregated user preference trends
    /// </summary>
    Task<UserPreferenceTrends> GetUserPreferenceTrendsAsync(TimeSpan timeWindow, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export user preference data for analysis
    /// </summary>
    Task<byte[]> ExportUserPreferenceDataAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null, ExportFormat format = ExportFormat.CSV, CancellationToken cancellationToken = default);

    /// <summary>
    /// Train preference learning models (background service)
    /// </summary>
    Task TrainPreferenceLearningModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get preference learning model performance metrics
    /// </summary>
    Task<PreferenceLearningMetrics> GetPreferenceLearningMetricsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset user preferences (GDPR compliance)
    /// </summary>
    Task ResetUserPreferencesAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user preference learning status
    /// </summary>
    Task<UserLearningStatus> GetUserLearningStatusAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Apply user preferences to prompt generation context
    /// </summary>
    Task<PersonalizedPromptContext> ApplyUserPreferencesToContextAsync(string userId, string userQuestion, object baseContext, CancellationToken cancellationToken = default);
}

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
