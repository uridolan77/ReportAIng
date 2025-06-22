using BIReportingCopilot.Core.Models.Analytics;

namespace BIReportingCopilot.Core.Interfaces.Analytics;

/// <summary>
/// Service for automated ML-based template improvement and optimization
/// </summary>
public interface ITemplateImprovementService
{
    /// <summary>
    /// Analyze template performance and generate improvement suggestions
    /// </summary>
    Task<List<TemplateImprovementSuggestion>> AnalyzeTemplatePerformanceAsync(string templateKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate improvement suggestions for all underperforming templates
    /// </summary>
    Task<List<TemplateImprovementSuggestion>> GenerateImprovementSuggestionsAsync(double performanceThreshold = 0.7, int minDataPoints = 100, CancellationToken cancellationToken = default);

    /// <summary>
    /// Apply ML-based optimization to a template
    /// </summary>
    Task<OptimizedTemplate> OptimizeTemplateAsync(string templateKey, OptimizationStrategy strategy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create A/B test for template variants
    /// </summary>
    Task<ABTestResult> CreateABTestAsync(ABTestRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get A/B test results and recommendations
    /// </summary>
    Task<ABTestAnalysis> GetABTestResultsAsync(long testId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Automatically implement winning template variants
    /// </summary>
    Task<ImplementationResult> ImplementWinningVariantAsync(long testId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze user feedback patterns for template improvements
    /// </summary>
    Task<FeedbackAnalysis> AnalyzeUserFeedbackAsync(string templateKey, TimeSpan? timeWindow = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate template variants using ML techniques
    /// </summary>
    Task<List<TemplateVariant>> GenerateTemplateVariantsAsync(string templateKey, int variantCount = 3, CancellationToken cancellationToken = default);

    /// <summary>
    /// Predict template performance for new templates
    /// </summary>
    Task<PerformancePrediction> PredictTemplatePerformanceAsync(string templateContent, string intentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get template improvement recommendations based on successful patterns
    /// </summary>
    Task<List<ImprovementRecommendation>> GetImprovementRecommendationsAsync(string templateKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze template content quality and suggest enhancements
    /// </summary>
    Task<ContentQualityAnalysis> AnalyzeTemplateContentQualityAsync(string templateContent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get template optimization history and trends
    /// </summary>
    Task<OptimizationHistory> GetOptimizationHistoryAsync(string templateKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Train improvement models with latest performance data
    /// </summary>
    Task<ModelTrainingResult> TrainImprovementModelsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get model performance metrics and accuracy
    /// </summary>
    Task<ModelPerformanceMetrics> GetModelPerformanceMetricsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Export improvement suggestions for review
    /// </summary>
    Task<byte[]> ExportImprovementSuggestionsAsync(DateTime startDate, DateTime endDate, ExportFormat format = ExportFormat.CSV, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approve or reject improvement suggestions
    /// </summary>
    Task<ReviewResult> ReviewImprovementSuggestionAsync(long suggestionId, SuggestionReviewAction action, string? reviewComments = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Enums for template improvement
/// </summary>
public enum OptimizationStrategy
{
    PerformanceFocused,
    AccuracyFocused,
    UserSatisfactionFocused,
    ResponseTimeFocused,
    Balanced
}

public enum SuggestionReviewAction
{
    Approve,
    Reject,
    RequestChanges,
    ScheduleABTest
}

public enum ImprovementType
{
    ContentOptimization,
    StructureImprovement,
    ContextEnhancement,
    ExampleAddition,
    InstructionClarification,
    PerformanceOptimization
}

public enum VariantType
{
    ContentVariation,
    StructureVariation,
    StyleVariation,
    ComplexityVariation
}
