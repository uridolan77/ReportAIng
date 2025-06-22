using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.DTOs;

/// <summary>
/// Update table info request DTO
/// </summary>
public class UpdateTableInfoRequest
{
    [StringLength(500)]
    public string BusinessPurpose { get; set; } = string.Empty;

    [StringLength(1000)]
    public string BusinessContext { get; set; } = string.Empty;

    [StringLength(500)]
    public string PrimaryUseCase { get; set; } = string.Empty;

    public List<string> CommonQueryPatterns { get; set; } = new();

    [StringLength(1000)]
    public string BusinessRules { get; set; } = string.Empty;

    [StringLength(100)]
    public string DomainClassification { get; set; } = string.Empty;

    public List<string> NaturalLanguageAliases { get; set; } = new();
    public List<string> BusinessProcesses { get; set; } = new();
    public List<string> AnalyticalUseCases { get; set; } = new();
    public List<string> ReportingCategories { get; set; } = new();
    public List<string> VectorSearchKeywords { get; set; } = new();
    public List<string> BusinessGlossaryTerms { get; set; } = new();
    public List<string> LLMContextHints { get; set; } = new();
    public List<string> QueryComplexityHints { get; set; } = new();

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Business table filter for search and pagination
/// </summary>
public class BusinessTableFilter
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public string? SchemaName { get; set; }
    public string? Domain { get; set; }
    public string? BusinessOwner { get; set; }
    public bool? IsActive { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Paginated result wrapper
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasNextPage => CurrentPage < TotalPages;
    public bool HasPreviousPage => CurrentPage > 1;
}

/// <summary>
/// Bulk operation request for business tables
/// </summary>
public class BulkBusinessTableRequest
{
    public List<long> TableIds { get; set; } = new();
    public BulkOperationType Operation { get; set; }
    public object? OperationData { get; set; }
}

/// <summary>
/// Bulk operation types
/// </summary>
public enum BulkOperationType
{
    Delete,
    UpdateDomain,
    UpdateOwner,
    UpdateTags,
    Activate,
    Deactivate,
    RegenerateMetadata
}

/// <summary>
/// Business table search request
/// </summary>
public class BusinessTableSearchRequest
{
    [Required]
    public string SearchQuery { get; set; } = string.Empty;
    
    public List<string> Schemas { get; set; } = new();
    public List<string> Domains { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public bool IncludeColumns { get; set; } = false;
    public bool IncludeGlossaryTerms { get; set; } = false;
    public int MaxResults { get; set; } = 50;
    public double MinRelevanceScore { get; set; } = 0.1;
}

/// <summary>
/// Business table validation request
/// </summary>
public class BusinessTableValidationRequest
{
    public long? TableId { get; set; }
    public string? SchemaName { get; set; }
    public string? TableName { get; set; }
    public bool ValidateBusinessRules { get; set; } = true;
    public bool ValidateDataQuality { get; set; } = true;
    public bool ValidateRelationships { get; set; } = true;
}

/// <summary>
/// Business table validation result
/// </summary>
public class BusinessTableValidationResult
{
    public bool IsValid { get; set; }
    public List<BusinessValidationIssue> Issues { get; set; } = new();
    public List<BusinessValidationWarning> Warnings { get; set; } = new();
    public List<BusinessValidationSuggestion> Suggestions { get; set; } = new();
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Business validation issue (specific to business metadata)
/// </summary>
public class BusinessValidationIssue
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Error";
    public string? Field { get; set; }
    public object? Context { get; set; }
}

/// <summary>
/// Business validation warning (specific to business metadata)
/// </summary>
public class BusinessValidationWarning
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Field { get; set; }
    public object? Context { get; set; }
}

/// <summary>
/// Business validation suggestion (specific to business metadata)
/// </summary>
public class BusinessValidationSuggestion
{
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RecommendedAction { get; set; }
    public object? Context { get; set; }
}

/// <summary>
/// Business metadata statistics
/// </summary>
public class BusinessMetadataStatistics
{
    public int TotalTables { get; set; }
    public int PopulatedTables { get; set; }
    public int TablesWithAIMetadata { get; set; }
    public int TablesWithRuleBasedMetadata { get; set; }
    public int TotalColumns { get; set; }
    public int PopulatedColumns { get; set; }
    public int TotalGlossaryTerms { get; set; }
    public DateTime LastPopulationRun { get; set; }
    public Dictionary<string, int> TablesByDomain { get; set; } = new();
    public Dictionary<string, int> TablesBySchema { get; set; } = new();
    public List<string> MostActiveUsers { get; set; } = new();
    public double AverageMetadataCompleteness { get; set; }
}
