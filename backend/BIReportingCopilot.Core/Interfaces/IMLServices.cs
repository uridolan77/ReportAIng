using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for ML-based anomaly detection
/// </summary>
public interface IMLAnomalyDetector
{
    /// <summary>
    /// Analyze a query for potential anomalies
    /// </summary>
    Task<AnomalyDetectionResult> AnalyzeQueryAsync(string userId, string naturalLanguageQuery, string generatedSQL);

    /// <summary>
    /// Analyze user behavior patterns
    /// </summary>
    Task<List<BehaviorAnomaly>> AnalyzeBehaviorAsync(string userId, TimeSpan timeWindow);

    /// <summary>
    /// Train the anomaly detection model
    /// </summary>
    Task TrainModelAsync(List<MLTrainingData> trainingData);

    /// <summary>
    /// Get model performance metrics
    /// </summary>
    Task<MLModelMetrics> GetModelMetricsAsync();
}

/// <summary>
/// Interface for feedback-based learning engine
/// </summary>
public interface IFeedbackLearningEngine
{
    /// <summary>
    /// Process user feedback for learning
    /// </summary>
    Task ProcessFeedbackAsync(UserFeedback feedback);

    /// <summary>
    /// Generate learning insights from feedback
    /// </summary>
    Task<List<LearningInsights>> GenerateInsightsAsync(string userId);

    /// <summary>
    /// Get personalized recommendations
    /// </summary>
    Task<List<PersonalizedRecommendation>> GetRecommendationsAsync(string userId);

    /// <summary>
    /// Update learning model based on feedback
    /// </summary>
    Task UpdateModelAsync(List<UserFeedback> feedbackBatch);
}

/// <summary>
/// Interface for prompt optimization
/// </summary>
public interface IPromptOptimizer
{
    /// <summary>
    /// Optimize a prompt based on historical performance
    /// </summary>
    Task<string> OptimizePromptAsync(string originalPrompt, QueryExecutionContext context);

    /// <summary>
    /// Analyze prompt effectiveness
    /// </summary>
    Task<double> AnalyzePromptEffectivenessAsync(string prompt, List<QueryMetrics> results);

    /// <summary>
    /// Generate prompt variations for A/B testing
    /// </summary>
    Task<List<string>> GeneratePromptVariationsAsync(string basePrompt);

    /// <summary>
    /// Get best performing prompts
    /// </summary>
    Task<List<string>> GetBestPromptsAsync(string category);
}

/// <summary>
/// User entity repository interface
/// </summary>
public interface IUserEntityRepository
{
    Task<object?> GetByIdAsync(string id);
    Task<object> CreateAsync(object entity);
    Task UpdateAsync(object entity);
    Task DeleteAsync(string id);
}