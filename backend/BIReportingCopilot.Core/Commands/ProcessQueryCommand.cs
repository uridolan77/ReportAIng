using MediatR;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.BusinessContext;

namespace BIReportingCopilot.Core.Commands;

/// <summary>
/// Command for processing a natural language query through the complete pipeline
/// Encapsulates the entire query processing workflow including caching, AI generation, and execution
/// Enhanced with business context and schema metadata for improved SQL generation
/// </summary>
public class ProcessQueryCommand : IRequest<QueryResponse>
{
    public string Question { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public QueryOptions Options { get; set; } = new();
    public CancellationToken CancellationToken { get; set; } = default;

    // Enhanced properties for Enhanced Schema Contextualization System
    public BusinessContextProfile? BusinessProfile { get; set; }
    public ContextualBusinessSchema? SchemaMetadata { get; set; }
    public string? EnhancedPrompt { get; set; }
}

/// <summary>
/// Command for caching a query result
/// </summary>
public class CacheQueryCommand : IRequest<bool>
{
    public string QueryHash { get; set; } = string.Empty;
    public QueryResponse Response { get; set; } = new();
    public TimeSpan? Expiry { get; set; }
}

/// <summary>
/// Command for invalidating query cache
/// </summary>
public class InvalidateQueryCacheCommand : IRequest<bool>
{
    public string Pattern { get; set; } = string.Empty;
}

/// <summary>
/// Command for submitting query feedback
/// </summary>
public class SubmitQueryFeedbackCommand : IRequest<bool>
{
    public QueryFeedback Feedback { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Command for generating SQL from natural language
/// Enhanced with business context and schema metadata for improved SQL generation
/// </summary>
public class GenerateSqlCommand : IRequest<GenerateSqlResponse>
{
    public string Question { get; set; } = string.Empty;
    public SchemaMetadata Schema { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string? ProviderId { get; set; }
    public string? ModelId { get; set; }

    // Enhanced properties for Enhanced Schema Contextualization System
    public BusinessContextProfile? BusinessProfile { get; set; }
    public ContextualBusinessSchema? SchemaMetadata { get; set; }
    public string? EnhancedPrompt { get; set; }
}

/// <summary>
/// Response for SQL generation command
/// </summary>
public class GenerateSqlResponse
{
    public string Sql { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? Error { get; set; }
    public double Confidence { get; set; }
    public PromptDetails? PromptDetails { get; set; }
    public int AiExecutionTimeMs { get; set; }
}

/// <summary>
/// Command for executing SQL query
/// </summary>
public class ExecuteSqlCommand : IRequest<SqlQueryResult>
{
    public string Sql { get; set; } = string.Empty;
    public QueryOptions Options { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
    public CancellationToken CancellationToken { get; set; } = default;
}

/// <summary>
/// Command for enhanced semantic cache lookup
/// </summary>
public class GetSemanticCacheCommand : IRequest<SemanticCacheResult?>
{
    public string NaturalLanguageQuery { get; set; } = string.Empty;
    public string SqlQuery { get; set; } = string.Empty;
    public double SimilarityThreshold { get; set; } = 0.85;
    public int MaxResults { get; set; } = 5;
}

/// <summary>
/// Command for storing query in enhanced semantic cache
/// </summary>
public class StoreSemanticCacheCommand : IRequest<bool>
{
    public string NaturalLanguageQuery { get; set; } = string.Empty;
    public string SqlQuery { get; set; } = string.Empty;
    public QueryResponse Response { get; set; } = new();
    public TimeSpan? Expiry { get; set; }
}

/// <summary>
/// Command for optimizing semantic cache
/// </summary>
public class OptimizeSemanticCacheCommand : IRequest<bool>
{
    public bool ForceOptimization { get; set; } = false;
}
