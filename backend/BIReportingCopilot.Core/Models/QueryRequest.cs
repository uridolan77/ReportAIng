using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

public class QueryRequest
{
    [Required(ErrorMessage = "Question is required")]
    [StringLength(2000, MinimumLength = 3, ErrorMessage = "Question must be between 3 and 2000 characters")]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "SessionId is required")]
    public string SessionId { get; set; } = string.Empty;

    public QueryOptions Options { get; set; } = new();

    // Property expected by Infrastructure services
    public string UserId { get; set; } = string.Empty;
}

public class QueryOptions
{
    public bool IncludeVisualization { get; set; } = true;

    [Range(1, 10000, ErrorMessage = "MaxRows must be between 1 and 10000")]
    public int MaxRows { get; set; } = 1000;

    public bool EnableCache { get; set; } = true;

    [Range(0.0, 1.0, ErrorMessage = "ConfidenceThreshold must be between 0 and 1")]
    public double ConfidenceThreshold { get; set; } = 0.7;

    public int TimeoutSeconds { get; set; } = 30;

    [Range(100, 5000, ErrorMessage = "ChunkSize must be between 100 and 5000")]
    public int? ChunkSize { get; set; } = 1000;

    public bool EnableStreaming { get; set; } = false;

    public string? DataSource { get; set; }
    public string[]? DataSources { get; set; }

    public Dictionary<string, object>? Parameters { get; set; }

    // Enhanced Schema Contextualization System options
    [Range(1000, 8000, ErrorMessage = "MaxTokens must be between 1000 and 8000")]
    public int? MaxTokens { get; set; } = 4000;

    [Range(1, 20, ErrorMessage = "MaxTables must be between 1 and 20")]
    public int? MaxTables { get; set; } = 5;

    // LLM Management options
    public string? ProviderId { get; set; }
    public string? ModelId { get; set; }
}

public class QueryResponse
{
    public string QueryId { get; set; } = string.Empty;
    public string Sql { get; set; } = string.Empty;
    public QueryResult? Result { get; set; }
    public VisualizationConfig? Visualization { get; set; }
    public double Confidence { get; set; }
    public string[] Suggestions { get; set; } = Array.Empty<string>();
    public bool Cached { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int ExecutionTimeMs { get; set; }
    public PromptDetails? PromptDetails { get; set; }

    // Properties expected by Infrastructure services
    public object[]? Data { get; set; }
    public ColumnMetadata[]? Columns { get; set; }
    public TimeSpan? ExecutionTime { get; set; }
    public bool IsSuccessful { get; set; } = true;

    /// <summary>
    /// Error message (alias for Error property)
    /// </summary>
    public string? ErrorMessage
    {
        get => Error;
        set => Error = value;
    }
}

public class QueryResult
{
    public object[]? Data { get; set; }
    public List<Dictionary<string, object>> DataList { get; set; } = new();
    public List<ColumnMetadata> Columns { get; set; } = new();
    public int TotalRows { get; set; }
    public long ExecutionTimeMs { get; set; }
    public QueryMetadata Metadata { get; set; } = new();
    public bool IsSuccessful { get; set; } = true;
    public string? ErrorMessage { get; set; }
}

public class QueryMetadata
{
    public int ColumnCount { get; set; }
    public int RowCount { get; set; }
    public int ExecutionTimeMs { get; set; }
    public ColumnMetadata[] Columns { get; set; } = Array.Empty<ColumnMetadata>();
    public string? DataSource { get; set; }
    public DateTime QueryTimestamp { get; set; } = DateTime.UtcNow;
    public string? Error { get; set; }

    // Additional properties expected by Infrastructure services
    public int EstimatedRowCount { get; set; }
    public string QueryComplexity { get; set; } = "Unknown";
    public bool SupportsStreaming { get; set; }
    public List<string> RequiredTables { get; set; } = new();
    public TimeSpan EstimatedExecutionTime { get; set; }
}



// VisualizationConfig, VisualizationMetadata, ChartTheme, and ColorScheme moved to Visualization.cs

// Enhanced visualization models for interactive features
public class InteractiveVisualizationConfig
{
    public VisualizationConfig BaseVisualization { get; set; } = new();
    public Dictionary<string, object> InteractiveFeatures { get; set; } = new();
    public FilterConfig[] Filters { get; set; } = Array.Empty<FilterConfig>();
    public DrillDownOption[] DrillDownOptions { get; set; } = Array.Empty<DrillDownOption>();
    public bool CrossFiltering { get; set; }
    public bool RealTimeUpdates { get; set; }
    public string[] ExportOptions { get; set; } = Array.Empty<string>();
}

public class FilterConfig
{
    public string ColumnName { get; set; } = string.Empty;
    public string FilterType { get; set; } = string.Empty; // range, multiSelect, dateRange
    public string Label { get; set; } = string.Empty;
    public object? DefaultValue { get; set; }
    public object[]? Options { get; set; }
}

public class DrillDownOption
{
    public string Name { get; set; } = string.Empty;
    public string[] Levels { get; set; } = Array.Empty<string>();
    public string TargetColumn { get; set; } = string.Empty;
}

// DashboardConfig moved to Visualization.cs

// DashboardLayout moved to MultiModalDashboards.cs to avoid duplicates

public class QueryHistoryItem
{
    public string Id { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string Sql { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public bool Successful { get; set; }
    public int ExecutionTimeMs { get; set; }
    public double Confidence { get; set; }
    public string? Error { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
}

public class QueryFeedback
{
    [Required]
    public string QueryId { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(positive|negative|neutral)$", ErrorMessage = "Feedback must be 'positive', 'negative', or 'neutral'")]
    public string Feedback { get; set; } = string.Empty;

    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
    public int Rating { get; set; } = 3;

    [StringLength(1000, ErrorMessage = "Comments cannot exceed 1000 characters")]
    public string? Comments { get; set; }

    [StringLength(500, ErrorMessage = "Suggested improvement cannot exceed 500 characters")]
    public string? SuggestedImprovement { get; set; }

    /// <summary>
    /// Feedback text (alias for Comments property)
    /// </summary>
    public string? FeedbackText
    {
        get => Comments;
        set => Comments = value;
    }

    /// <summary>
    /// Submitted at timestamp
    /// </summary>
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}

public class PromptDetails
{
    public string FullPrompt { get; set; } = string.Empty;
    public string TemplateName { get; set; } = string.Empty;
    public string TemplateVersion { get; set; } = string.Empty;
    public PromptSection[] Sections { get; set; } = Array.Empty<PromptSection>();
    public Dictionary<string, string> Variables { get; set; } = new();
    public int TokenCount { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // Template analytics tracking properties
    public string? TemplateKey { get; set; }
    public string? UsageId { get; set; }
    public long? ABTestId { get; set; }

    // Enhanced Schema Contextualization System properties
    public int PromptLength { get; set; }
    public int SchemaTablesCount { get; set; }
    public string? BusinessDomain { get; set; }
    public double ConfidenceScore { get; set; }
    public bool IsEnhancedPrompt { get; set; }
    public string? EnhancementSource { get; set; } // "Enhanced", "Basic", "Fallback"
}

public class PromptSection
{
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // schema, business_rules, examples, context, template
    public int Order { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}

// DashboardLayout and FilterConfig classes moved to MultiModalDashboards.cs to avoid duplicates
