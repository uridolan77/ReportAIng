using BIReportingCopilot.Core.Models.ML;

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
/// Basic implementation of IFeedbackLearningEngine
/// </summary>
public class FeedbackLearningEngineAdapter : IFeedbackLearningEngine
{
    public async Task ProcessFeedbackAsync(UserFeedback feedback)
    {
        // Basic implementation - store feedback
        await Task.CompletedTask;
    }

    public async Task<List<LearningInsights>> GenerateInsightsAsync(string userId)
    {
        // Basic implementation
        await Task.CompletedTask;
        return new List<LearningInsights>();
    }

    public async Task<List<PersonalizedRecommendation>> GetRecommendationsAsync(string userId)
    {
        // Basic implementation
        await Task.CompletedTask;
        return new List<PersonalizedRecommendation>();
    }

    public async Task UpdateModelAsync(List<UserFeedback> feedbackBatch)
    {
        // Basic implementation
        await Task.CompletedTask;
    }
}

/// <summary>
/// Basic implementation of IPromptOptimizer
/// </summary>
public class PromptOptimizerAdapter : IPromptOptimizer
{
    public async Task<string> OptimizePromptAsync(string originalPrompt, QueryExecutionContext context)
    {
        // Basic implementation - return original prompt
        await Task.CompletedTask;
        return originalPrompt;
    }

    public async Task<double> AnalyzePromptEffectivenessAsync(string prompt, List<QueryMetrics> results)
    {
        // Basic implementation - calculate average success rate
        await Task.CompletedTask;
        if (!results.Any()) return 0.0;
        return results.Average(r => r.IsSuccessful ? 1.0 : 0.0);
    }

    public async Task<List<string>> GeneratePromptVariationsAsync(string basePrompt)
    {
        // Basic implementation
        await Task.CompletedTask;
        return new List<string> { basePrompt };
    }

    public async Task<List<string>> GetBestPromptsAsync(string category)
    {
        // Basic implementation
        await Task.CompletedTask;
        return new List<string>();
    }
}

// Type aliases for backward compatibility
public class MLAnomalyDetector
{
    public async Task<AnomalyDetectionResult> AnalyzeQueryAsync(string userId, string naturalLanguageQuery, string generatedSQL)
    {
        // Basic implementation
        await Task.CompletedTask;
        return new AnomalyDetectionResult
        {
            QueryId = Guid.NewGuid().ToString(),
            UserId = userId,
            AnomalyScore = 0.1,
            RiskLevel = RiskLevel.Low,
            DetectedAnomalies = new List<DetectedAnomaly>(),
            AnalyzedAt = DateTime.UtcNow
        };
    }
}

public class FeedbackLearningEngine
{
    public async Task ProcessFeedbackAsync(UserFeedback feedback)
    {
        await Task.CompletedTask;
    }
}

public class PromptOptimizer
{
    public async Task<string> OptimizePromptAsync(string originalPrompt, QueryExecutionContext context)
    {
        await Task.CompletedTask;
        return originalPrompt;
    }
}
