using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.Analytics;

/// <summary>
/// Service for comprehensive template management and lifecycle operations
/// </summary>
public interface ITemplateManagementService
{
    /// <summary>
    /// Get template by key with performance data
    /// </summary>
    Task<TemplateWithMetrics?> GetTemplateAsync(string templateKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active templates with performance data
    /// </summary>
    Task<List<TemplateWithMetrics>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get templates by intent type
    /// </summary>
    Task<List<TemplateWithMetrics>> GetTemplatesByIntentTypeAsync(string intentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new template with initial performance tracking
    /// </summary>
    Task<TemplateCreationResult> CreateTemplateAsync(CreateTemplateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing template and create new version
    /// </summary>
    Task<TemplateUpdateResult> UpdateTemplateAsync(string templateKey, UpdateTemplateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Activate a template for use
    /// </summary>
    Task<bool> ActivateTemplateAsync(string templateKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivate a template
    /// </summary>
    Task<bool> DeactivateTemplateAsync(string templateKey, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a template (soft delete)
    /// </summary>
    Task<bool> DeleteTemplateAsync(string templateKey, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get template versions and history
    /// </summary>
    Task<List<TemplateVersion>> GetTemplateVersionsAsync(string templateKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rollback to a previous template version
    /// </summary>
    Task<TemplateRollbackResult> RollbackTemplateAsync(string templateKey, string targetVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search templates by criteria
    /// </summary>
    Task<TemplateSearchResult> SearchTemplatesAsync(TemplateSearchCriteria criteria, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get templates needing review or attention
    /// </summary>
    Task<List<TemplateWithMetrics>> GetTemplatesNeedingReviewAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update template business metadata
    /// </summary>
    Task<bool> UpdateBusinessMetadataAsync(string templateKey, TemplateBusinessMetadata metadata, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get template usage analytics
    /// </summary>
    Task<TemplateUsageAnalytics> GetUsageAnalyticsAsync(string templateKey, TimeSpan? timeWindow = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clone a template with modifications
    /// </summary>
    Task<TemplateCreationResult> CloneTemplateAsync(string sourceTemplateKey, CloneTemplateRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate template content and structure
    /// </summary>
    Task<TemplateValidationResult> ValidateTemplateAsync(string templateContent, string intentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get template recommendations for improvement
    /// </summary>
    Task<List<TemplateRecommendation>> GetTemplateRecommendationsAsync(string templateKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Export templates for backup or migration
    /// </summary>
    Task<byte[]> ExportTemplatesAsync(List<string> templateKeys, ExportFormat format = ExportFormat.JSON, CancellationToken cancellationToken = default);

    /// <summary>
    /// Import templates from backup or migration
    /// </summary>
    Task<TemplateImportResult> ImportTemplatesAsync(byte[] templateData, ImportOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get template management dashboard data
    /// </summary>
    Task<TemplateManagementDashboard> GetDashboardAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Template with performance metrics
/// </summary>
public class TemplateWithMetrics
{
    public PromptTemplateEntity Template { get; set; } = new();
    public TemplatePerformanceMetrics? PerformanceMetrics { get; set; }
    public List<ABTestDetails> ActiveTests { get; set; } = new();
    public List<TemplateImprovementSuggestion> PendingSuggestions { get; set; } = new();
    public TemplateHealthStatus HealthStatus { get; set; }
    public DateTime LastAnalyzed { get; set; }
}

/// <summary>
/// Template creation request
/// </summary>
public class CreateTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string TemplateKey { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string IntentType { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;
    public List<string> Tags { get; set; } = new();
    public TemplateBusinessMetadata? BusinessMetadata { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Template update request
/// </summary>
public class UpdateTemplateRequest
{
    public string? Name { get; set; }
    public string? Content { get; set; }
    public string? Description { get; set; }
    public int? Priority { get; set; }
    public List<string>? Tags { get; set; }
    public TemplateBusinessMetadata? BusinessMetadata { get; set; }
    public string UpdatedBy { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }
    public bool CreateNewVersion { get; set; } = true;
}

/// <summary>
/// Template creation result
/// </summary>
public class TemplateCreationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? TemplateKey { get; set; }
    public long? TemplateId { get; set; }
    public string? Version { get; set; }
    public List<string> ValidationWarnings { get; set; } = new();
}

/// <summary>
/// Template update result
/// </summary>
public class TemplateUpdateResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? NewVersion { get; set; }
    public string? PreviousVersion { get; set; }
    public List<string> ChangesApplied { get; set; } = new();
    public List<string> ValidationWarnings { get; set; } = new();
}

/// <summary>
/// Template version information
/// </summary>
public class TemplateVersion
{
    public string Version { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? ChangeReason { get; set; }
    public bool IsActive { get; set; }
    public TemplatePerformanceMetrics? PerformanceMetrics { get; set; }
}

/// <summary>
/// Template rollback result
/// </summary>
public class TemplateRollbackResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? RolledBackToVersion { get; set; }
    public string? PreviousVersion { get; set; }
    public DateTime RollbackDate { get; set; }
}

/// <summary>
/// Template search criteria
/// </summary>
public class TemplateSearchCriteria
{
    public string? SearchTerm { get; set; }
    public List<string>? IntentTypes { get; set; }
    public List<string>? Tags { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public decimal? MinSuccessRate { get; set; }
    public int? MinUsageCount { get; set; }
    public string? CreatedBy { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// Template search result
/// </summary>
public class TemplateSearchResult
{
    public List<TemplateWithMetrics> Templates { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Template business metadata
/// </summary>
public class TemplateBusinessMetadata
{
    public string? BusinessPurpose { get; set; }
    public string? BusinessFriendlyName { get; set; }
    public string? NaturalLanguageDescription { get; set; }
    public List<string>? RelatedBusinessTerms { get; set; }
    public string? BusinessRules { get; set; }
    public string? RelationshipContext { get; set; }
    public string? DataGovernanceLevel { get; set; }
    public decimal? ImportanceScore { get; set; }
    public string? UsageFrequency { get; set; }
}

/// <summary>
/// Template usage analytics
/// </summary>
public class TemplateUsageAnalytics
{
    public string TemplateKey { get; set; } = string.Empty;
    public TimeSpan AnalysisWindow { get; set; }
    public int TotalUsages { get; set; }
    public Dictionary<string, int> UsageByHour { get; set; } = new();
    public Dictionary<string, int> UsageByDay { get; set; } = new();
    public Dictionary<string, int> UsageByUser { get; set; } = new();
    public List<UsagePattern> Patterns { get; set; } = new();
    public DateTime AnalysisDate { get; set; }
}

/// <summary>
/// Clone template request
/// </summary>
public class CloneTemplateRequest
{
    public string NewName { get; set; } = string.Empty;
    public string NewTemplateKey { get; set; } = string.Empty;
    public string? ContentModifications { get; set; }
    public string? NewIntentType { get; set; }
    public List<string>? NewTags { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string? CloneReason { get; set; }
}

/// <summary>
/// Template validation result
/// </summary>
public class TemplateValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    public List<ValidationWarning> Warnings { get; set; } = new();
    public List<ValidationSuggestion> Suggestions { get; set; } = new();
    public decimal QualityScore { get; set; }
}

/// <summary>
/// Template recommendation
/// </summary>
public class TemplateRecommendation
{
    public string RecommendationType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Priority { get; set; }
    public decimal ExpectedImpact { get; set; }
    public List<string> ActionItems { get; set; } = new();
    public Dictionary<string, object> RecommendationData { get; set; } = new();
}

/// <summary>
/// Template import options
/// </summary>
public class ImportOptions
{
    public bool OverwriteExisting { get; set; } = false;
    public bool ValidateBeforeImport { get; set; } = true;
    public bool CreateBackup { get; set; } = true;
    public string ImportedBy { get; set; } = string.Empty;
}

/// <summary>
/// Template import result
/// </summary>
public class TemplateImportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TemplatesImported { get; set; }
    public int TemplatesSkipped { get; set; }
    public int TemplatesUpdated { get; set; }
    public List<string> ImportErrors { get; set; } = new();
    public List<string> ImportWarnings { get; set; } = new();
}

/// <summary>
/// Template management dashboard
/// </summary>
public class TemplateManagementDashboard
{
    public int TotalTemplates { get; set; }
    public int ActiveTemplates { get; set; }
    public int TemplatesNeedingReview { get; set; }
    public int RecentlyCreated { get; set; }
    public int RecentlyUpdated { get; set; }
    public Dictionary<string, int> TemplatesByIntentType { get; set; } = new();
    public List<TemplateWithMetrics> TopPerformers { get; set; } = new();
    public List<TemplateWithMetrics> NeedsAttention { get; set; } = new();
    public List<TemplateActivityDataPoint> RecentActivity { get; set; } = new();
    public DateTime GeneratedDate { get; set; }
}

#region Supporting Models

public enum TemplateHealthStatus
{
    Excellent,
    Good,
    Fair,
    Poor,
    Critical
}

public class ValidationError
{
    public string ErrorType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Suggestion { get; set; }
}

public class ValidationWarning
{
    public string WarningType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Suggestion { get; set; }
}

public class ValidationSuggestion
{
    public string SuggestionType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public decimal Priority { get; set; }
    public string? ActionRequired { get; set; }
}

public class TemplateActivityDataPoint
{
    public DateTime Date { get; set; }
    public int TemplatesCreated { get; set; }
    public int TemplatesUpdated { get; set; }
    public int TemplatesActivated { get; set; }
    public int TemplatesDeactivated { get; set; }
}

#endregion
