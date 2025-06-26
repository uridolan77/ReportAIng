using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Request for enhanced query processing with comprehensive business context analysis
/// </summary>
public class EnhancedQueryRequest
{
    [Required]
    public string Query { get; set; } = string.Empty;
    
    public bool ExecuteQuery { get; set; } = true;
    public bool IncludeAlternatives { get; set; } = true;
    public bool IncludeSemanticAnalysis { get; set; } = true;
    public string? Context { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}

/// <summary>
/// Response for enhanced query processing with business context and transparency data
/// </summary>
public class EnhancedQueryResponse
{
    public ProcessedQuery? ProcessedQuery { get; set; }
    public QueryResult? QueryResult { get; set; }
    public SemanticAnalysisResponse? SemanticAnalysis { get; set; }
    public ClassificationResponse? Classification { get; set; }
    public List<AlternativeQueryResponse> Alternatives { get; set; } = new();
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    // Business Context Integration
    public object? BusinessContext { get; set; }
    
    // ProcessFlow Integration
    public string? SessionId { get; set; }
    public string? TraceId { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    
    // AI Transparency Foundation integration
    public TransparencyMetadata? TransparencyData { get; set; }
}

/// <summary>
/// Semantic analysis response with business context
/// </summary>
public class SemanticAnalysisResponse
{
    public string Intent { get; set; } = string.Empty;
    public List<EntityResponse> Entities { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public double Confidence { get; set; }
    public string ProcessedQuery { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Entity response for semantic analysis
/// </summary>
public class EntityResponse
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Value { get; set; } = string.Empty;
    public string? MappedTable { get; set; }
    public string? MappedColumn { get; set; }
}

/// <summary>
/// Classification response for query categorization
/// </summary>
public class ClassificationResponse
{
    public string Category { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty;
    public TimeSpan EstimatedExecutionTime { get; set; }
    public string RecommendedVisualization { get; set; } = string.Empty;
    public List<string> OptimizationSuggestions { get; set; } = new();
}

/// <summary>
/// Alternative query response for query suggestions
/// </summary>
public class AlternativeQueryResponse
{
    public string Query { get; set; } = string.Empty;
    public string Sql { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Explanation { get; set; } = string.Empty;
    public List<string> Benefits { get; set; } = new();
}

// ProcessedQuery is already defined in another file

/// <summary>
/// Semantic entity for query processing
/// </summary>
public class SemanticEntity
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Context { get; set; } = string.Empty;
}

// QueryClassification and SchemaContext are already defined in other files

/// <summary>
/// Transparency metadata for AI operations
/// </summary>
public class TransparencyMetadata
{
    public string TraceId { get; set; } = string.Empty;
    public BusinessContextSummary? BusinessContext { get; set; }
    public TokenUsageSummary? TokenUsage { get; set; }
    public int ProcessingSteps { get; set; }
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

/// <summary>
/// Business context summary for transparency
/// </summary>
public class BusinessContextSummary
{
    public string Intent { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Domain { get; set; } = string.Empty;
    public List<string> Entities { get; set; } = new();
}

/// <summary>
/// Token usage summary for cost tracking
/// </summary>
public class TokenUsageSummary
{
    public int AllocatedTokens { get; set; }
    public decimal EstimatedCost { get; set; }
    public string Provider { get; set; } = string.Empty;
    public int UsedTokens { get; set; }
}
