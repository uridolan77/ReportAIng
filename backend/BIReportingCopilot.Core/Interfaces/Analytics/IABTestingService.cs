using BIReportingCopilot.Core.Models.Analytics;

namespace BIReportingCopilot.Core.Interfaces.Analytics;

/// <summary>
/// Service for managing A/B testing of template variations
/// </summary>
public interface IABTestingService
{
    /// <summary>
    /// Create a new A/B test for template variants
    /// </summary>
    Task<ABTestResult> CreateABTestAsync(ABTestRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get A/B test by ID
    /// </summary>
    Task<ABTestDetails?> GetABTestAsync(long testId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active A/B tests
    /// </summary>
    Task<List<ABTestDetails>> GetActiveTestsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get A/B tests by status
    /// </summary>
    Task<List<ABTestDetails>> GetTestsByStatusAsync(ABTestStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get A/B tests for a specific template
    /// </summary>
    Task<List<ABTestDetails>> GetTestsForTemplateAsync(string templateKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Select template variant for user based on active A/B tests
    /// </summary>
    Task<TemplateSelectionResult> SelectTemplateForUserAsync(string userId, string intentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record A/B test interaction (usage, success, failure)
    /// </summary>
    Task RecordTestInteractionAsync(long testId, string templateKey, bool wasSuccessful, decimal confidenceScore, string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze A/B test results and determine statistical significance
    /// </summary>
    Task<ABTestAnalysis> AnalyzeTestResultsAsync(long testId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete an A/B test and implement the winning variant
    /// </summary>
    Task<ImplementationResult> CompleteTestAsync(long testId, bool implementWinner = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pause an active A/B test
    /// </summary>
    Task<bool> PauseTestAsync(long testId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resume a paused A/B test
    /// </summary>
    Task<bool> ResumeTestAsync(long testId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel an A/B test
    /// </summary>
    Task<bool> CancelTestAsync(long testId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get A/B test performance dashboard
    /// </summary>
    Task<ABTestDashboard> GetTestDashboardAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get tests requiring analysis (sufficient data collected)
    /// </summary>
    Task<List<ABTestDetails>> GetTestsRequiringAnalysisAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get expired tests that should be concluded
    /// </summary>
    Task<List<ABTestDetails>> GetExpiredTestsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate if A/B test can be created for given templates
    /// </summary>
    Task<ABTestValidationResult> ValidateTestCreationAsync(string originalTemplateKey, string variantTemplateKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get A/B test recommendations based on template performance
    /// </summary>
    Task<List<ABTestRecommendation>> GetTestRecommendationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Export A/B test results for analysis
    /// </summary>
    Task<byte[]> ExportTestResultsAsync(long testId, ExportFormat format = ExportFormat.CSV, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate statistical significance for A/B test
    /// </summary>
    Task<StatisticalSignificanceResult> CalculateStatisticalSignificanceAsync(long testId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get A/B test history and trends
    /// </summary>
    Task<ABTestHistory> GetTestHistoryAsync(string? templateKey = null, TimeSpan? timeWindow = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Detailed A/B test information
/// </summary>
public class ABTestDetails
{
    public long Id { get; set; }
    public string TestName { get; set; } = string.Empty;
    public string OriginalTemplateKey { get; set; } = string.Empty;
    public string VariantTemplateKey { get; set; } = string.Empty;
    public int TrafficSplitPercent { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public ABTestStatus Status { get; set; }
    public int OriginalUsageCount { get; set; }
    public int VariantUsageCount { get; set; }
    public decimal? OriginalSuccessRate { get; set; }
    public decimal? VariantSuccessRate { get; set; }
    public decimal? StatisticalSignificance { get; set; }
    public string? WinnerTemplateKey { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public Dictionary<string, object> TestParameters { get; set; } = new();
    public List<string> MetricsTracked { get; set; } = new();
}

/// <summary>
/// Template selection result for A/B testing
/// </summary>
public class TemplateSelectionResult
{
    public string SelectedTemplateKey { get; set; } = string.Empty;
    public bool IsVariant { get; set; }
    public long? TestId { get; set; }
    public string SelectionReason { get; set; } = string.Empty;
    public Dictionary<string, object> SelectionMetadata { get; set; } = new();
}

/// <summary>
/// A/B test dashboard data
/// </summary>
public class ABTestDashboard
{
    public int TotalActiveTests { get; set; }
    public int TotalCompletedTests { get; set; }
    public int TestsRequiringAnalysis { get; set; }
    public int ExpiredTests { get; set; }
    public decimal AverageTestDuration { get; set; }
    public decimal AverageImprovementRate { get; set; }
    public List<ABTestDetails> RecentTests { get; set; } = new();
    public List<ABTestDetails> TopPerformingTests { get; set; } = new();
    public Dictionary<string, int> TestsByStatus { get; set; } = new();
    public List<TestTrendDataPoint> TestTrends { get; set; } = new();
    public DateTime GeneratedDate { get; set; }
}

/// <summary>
/// A/B test validation result
/// </summary>
public class ABTestValidationResult
{
    public bool CanCreateTest { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public List<string> ValidationWarnings { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// A/B test recommendation
/// </summary>
public class ABTestRecommendation
{
    public string TemplateKey { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string RecommendationType { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public decimal Priority { get; set; }
    public decimal ExpectedImpact { get; set; }
    public List<string> SuggestedVariations { get; set; } = new();
    public Dictionary<string, object> RecommendationData { get; set; } = new();
    public DateTime GeneratedDate { get; set; }
}

/// <summary>
/// Statistical significance calculation result
/// </summary>
public class StatisticalSignificanceResult
{
    public long TestId { get; set; }
    public decimal StatisticalSignificance { get; set; }
    public decimal ConfidenceLevel { get; set; }
    public decimal PValue { get; set; }
    public bool IsSignificant { get; set; }
    public int SampleSizeOriginal { get; set; }
    public int SampleSizeVariant { get; set; }
    public decimal EffectSize { get; set; }
    public decimal PowerAnalysis { get; set; }
    public string Interpretation { get; set; } = string.Empty;
    public DateTime CalculatedDate { get; set; }
}

/// <summary>
/// A/B test history and trends
/// </summary>
public class ABTestHistory
{
    public string? TemplateKey { get; set; }
    public TimeSpan TimeWindow { get; set; }
    public List<ABTestDetails> Tests { get; set; } = new();
    public int TotalTests { get; set; }
    public int SuccessfulTests { get; set; }
    public decimal AverageImprovementRate { get; set; }
    public Dictionary<string, int> TestOutcomes { get; set; } = new();
    public List<TestTrendDataPoint> Trends { get; set; } = new();
    public DateTime AnalysisDate { get; set; }
}

#region Supporting Models

public class TestTrendDataPoint
{
    public DateTime Date { get; set; }
    public int ActiveTests { get; set; }
    public int CompletedTests { get; set; }
    public decimal AverageSuccessRate { get; set; }
    public decimal AverageImprovementRate { get; set; }
}

#endregion
