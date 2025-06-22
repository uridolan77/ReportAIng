using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.Analytics;

namespace BIReportingCopilot.Core.Interfaces.Repository;

/// <summary>
/// Repository interface for template performance metrics operations
/// </summary>
public interface ITemplatePerformanceRepository : IRepository<TemplatePerformanceMetricsEntity>
{
    // Template-specific queries
    Task<TemplatePerformanceMetricsEntity?> GetByTemplateIdAsync(long templateId, CancellationToken cancellationToken = default);
    Task<List<TemplatePerformanceMetricsEntity>> GetByIntentTypeAsync(string intentType, CancellationToken cancellationToken = default);
    Task<List<TemplatePerformanceMetricsEntity>> GetByTemplateKeyAsync(string templateKey, CancellationToken cancellationToken = default);

    // Performance analytics queries
    Task<List<TemplatePerformanceMetricsEntity>> GetTopPerformingTemplatesAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<List<TemplatePerformanceMetricsEntity>> GetUnderperformingTemplatesAsync(decimal threshold = 0.7m, CancellationToken cancellationToken = default);
    Task<List<TemplatePerformanceMetricsEntity>> GetByUsageDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<List<TemplatePerformanceMetricsEntity>> GetRecentlyUsedAsync(int days = 30, CancellationToken cancellationToken = default);

    // Metrics aggregation
    Task<Dictionary<string, decimal>> GetSuccessRatesByIntentTypeAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetUsageCountsByIntentTypeAsync(CancellationToken cancellationToken = default);
    Task<decimal> GetAverageSuccessRateAsync(string? intentType = null, CancellationToken cancellationToken = default);
    Task<decimal> GetAverageProcessingTimeAsync(string? intentType = null, CancellationToken cancellationToken = default);

    // Real-time updates
    Task IncrementUsageAsync(long templateId, bool wasSuccessful, decimal confidenceScore, int processingTimeMs, CancellationToken cancellationToken = default);
    Task UpdateUserRatingAsync(long templateId, decimal rating, CancellationToken cancellationToken = default);
    Task RecalculateMetricsAsync(long templateId, CancellationToken cancellationToken = default);
    Task UpdateLastUsedDateAsync(long templateId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for A/B testing operations
/// </summary>
public interface ITemplateABTestRepository : IRepository<TemplateABTestEntity>
{
    // Test management queries
    Task<List<TemplateABTestEntity>> GetActiveTestsAsync(CancellationToken cancellationToken = default);
    Task<List<TemplateABTestEntity>> GetTestsByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<List<TemplateABTestEntity>> GetTestsByTemplateIdAsync(long templateId, CancellationToken cancellationToken = default);
    Task<TemplateABTestEntity?> GetActiveTestForTemplateAsync(long templateId, CancellationToken cancellationToken = default);
    Task<TemplateABTestEntity?> GetByTestNameAsync(string testName, CancellationToken cancellationToken = default);

    // Test analytics
    Task<List<TemplateABTestEntity>> GetCompletedTestsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    Task<List<TemplateABTestEntity>> GetTestsRequiringAnalysisAsync(CancellationToken cancellationToken = default);
    Task<List<TemplateABTestEntity>> GetExpiredTestsAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetTestCountsByStatusAsync(CancellationToken cancellationToken = default);

    // Statistical operations
    Task UpdateTestResultsAsync(long testId, decimal originalSuccessRate, decimal variantSuccessRate,
        decimal statisticalSignificance, long? winnerTemplateId, string testResults, CancellationToken cancellationToken = default);
    Task CompleteTestAsync(long testId, long winnerTemplateId, CancellationToken cancellationToken = default);
    Task PauseTestAsync(long testId, CancellationToken cancellationToken = default);
    Task ResumeTestAsync(long testId, CancellationToken cancellationToken = default);
    Task CancelTestAsync(long testId, string reason, CancellationToken cancellationToken = default);

    // Test validation
    Task<bool> HasActiveTestForTemplateAsync(long templateId, CancellationToken cancellationToken = default);
    Task<bool> CanCreateTestAsync(long originalTemplateId, long variantTemplateId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for template improvement suggestions operations
/// </summary>
public interface ITemplateImprovementRepository : IRepository<TemplateImprovementSuggestionEntity>
{
    // Suggestion management queries
    Task<List<TemplateImprovementSuggestionEntity>> GetByTemplateIdAsync(long templateId, CancellationToken cancellationToken = default);
    Task<List<TemplateImprovementSuggestionEntity>> GetByStatusAsync(string status, CancellationToken cancellationToken = default);
    Task<List<TemplateImprovementSuggestionEntity>> GetBySuggestionTypeAsync(string suggestionType, CancellationToken cancellationToken = default);
    Task<List<TemplateImprovementSuggestionEntity>> GetPendingReviewAsync(CancellationToken cancellationToken = default);
    Task<List<TemplateImprovementSuggestionEntity>> GetPendingSuggestionsAsync(CancellationToken cancellationToken = default);
    Task<List<TemplateImprovementSuggestionEntity>> GetByReviewerAsync(string reviewerId, CancellationToken cancellationToken = default);

    // Review workflow
    Task ApproveSuggestionAsync(long suggestionId, string reviewedBy, string? reviewComments = null, CancellationToken cancellationToken = default);
    Task RejectSuggestionAsync(long suggestionId, string reviewedBy, string? reviewComments = null, CancellationToken cancellationToken = default);
    Task ImplementSuggestionAsync(long suggestionId, CancellationToken cancellationToken = default);
    Task UpdateSuggestionStatusAsync(long suggestionId, string status, string? reviewComments = null, CancellationToken cancellationToken = default);

    // Analytics and reporting
    Task<Dictionary<string, int>> GetSuggestionCountsByTypeAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetSuggestionCountsByStatusAsync(CancellationToken cancellationToken = default);
    Task<List<TemplateImprovementSuggestionEntity>> GetHighImpactSuggestionsAsync(decimal minImprovementPercent = 10m, CancellationToken cancellationToken = default);
    Task<List<TemplateImprovementSuggestionEntity>> GetRecentSuggestionsAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<List<TemplateImprovementSuggestionEntity>> GetSuggestionsByDataPointsAsync(int minDataPoints, CancellationToken cancellationToken = default);

    // Suggestion validation
    Task<bool> HasPendingSuggestionsForTemplateAsync(long templateId, CancellationToken cancellationToken = default);
    Task<int> GetSuggestionCountForTemplateAsync(long templateId, string? status = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Enhanced repository interface for prompt templates with template management features
/// </summary>
public interface IEnhancedPromptTemplateRepository : IRepository<PromptTemplateEntity>
{
    // Template-specific queries
    Task<PromptTemplateEntity?> GetByKeyAsync(string templateKey, CancellationToken cancellationToken = default);
    Task<List<PromptTemplateEntity>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default);
    Task<List<PromptTemplateEntity>> GetByIntentTypeAsync(string intentType, CancellationToken cancellationToken = default);
    Task<List<PromptTemplateEntity>> GetByPriorityRangeAsync(int minPriority, int maxPriority, CancellationToken cancellationToken = default);
    Task<List<PromptTemplateEntity>> SearchByTagsAsync(List<string> tags, CancellationToken cancellationToken = default);
    Task<List<PromptTemplateEntity>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);

    // Template with relationships
    Task<PromptTemplateEntity?> GetWithPerformanceMetricsAsync(long id, CancellationToken cancellationToken = default);
    Task<PromptTemplateEntity?> GetWithABTestsAsync(long id, CancellationToken cancellationToken = default);
    Task<PromptTemplateEntity?> GetWithImprovementSuggestionsAsync(long id, CancellationToken cancellationToken = default);
    Task<PromptTemplateEntity?> GetWithAllRelationshipsAsync(long id, CancellationToken cancellationToken = default);

    // Template selection for A/B testing
    Task<PromptTemplateEntity?> SelectTemplateForUserAsync(string userId, string intentType, CancellationToken cancellationToken = default);
    Task<List<PromptTemplateEntity>> GetTemplatesEligibleForTestingAsync(string intentType, CancellationToken cancellationToken = default);
    Task<PromptTemplateEntity?> GetRandomTemplateByIntentAsync(string intentType, CancellationToken cancellationToken = default);

    // Template analytics
    Task<Dictionary<string, int>> GetTemplateCountsByIntentTypeAsync(CancellationToken cancellationToken = default);
    Task<List<PromptTemplateEntity>> GetMostUsedTemplatesAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<List<PromptTemplateEntity>> GetRecentlyUpdatedTemplatesAsync(int days = 30, CancellationToken cancellationToken = default);
    Task<List<PromptTemplateEntity>> GetTemplatesNeedingReviewAsync(CancellationToken cancellationToken = default);

    // Template management
    Task ActivateTemplateAsync(long id, CancellationToken cancellationToken = default);
    Task DeactivateTemplateAsync(long id, CancellationToken cancellationToken = default);
    Task UpdateUsageCountAsync(long id, CancellationToken cancellationToken = default);
    Task UpdateSuccessRateAsync(long id, decimal successRate, CancellationToken cancellationToken = default);
    Task UpdateLastBusinessReviewAsync(long id, CancellationToken cancellationToken = default);

    // Template versioning
    Task<List<PromptTemplateEntity>> GetTemplateVersionsAsync(string templateKey, CancellationToken cancellationToken = default);
    Task<PromptTemplateEntity?> GetLatestVersionAsync(string templateKey, CancellationToken cancellationToken = default);
    Task<string> GenerateNextVersionAsync(string templateKey, CancellationToken cancellationToken = default);

    // Template search
    Task<List<PromptTemplateEntity>> SearchTemplatesAsync(TemplateSearchCriteria criteria, CancellationToken cancellationToken = default);
    Task<int> GetSearchCountAsync(TemplateSearchCriteria criteria, CancellationToken cancellationToken = default);
}
