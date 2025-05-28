using BIReportingCopilot.Core.Models;
using CoreModels = BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for streaming OpenAI service with advanced capabilities
/// </summary>
public interface IStreamingOpenAIService
{
    /// <summary>
    /// Generate SQL with streaming response and sophisticated prompt engineering
    /// </summary>
    IAsyncEnumerable<StreamingResponse> GenerateSQLStreamAsync(
        string prompt,
        CoreModels.SchemaMetadata? schema = null,
        QueryContext? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate insights with streaming response
    /// </summary>
    IAsyncEnumerable<StreamingResponse> GenerateInsightStreamAsync(
        string query,
        object[] data,
        AnalysisContext? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate SQL explanation with streaming response
    /// </summary>
    IAsyncEnumerable<StreamingResponse> GenerateExplanationStreamAsync(
        string sql,
        StreamingQueryComplexity complexity = StreamingQueryComplexity.Medium,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Streaming response from OpenAI service
/// </summary>
public class StreamingResponse
{
    public StreamingResponseType Type { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsComplete { get; set; }
    public int ChunkIndex { get; set; }
    public object? Metadata { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Types of streaming responses
/// </summary>
public enum StreamingResponseType
{
    SQLGeneration,
    Insight,
    Explanation,
    Error,
    Progress
}

/// <summary>
/// Query context for enhanced prompt engineering
/// </summary>
public class QueryContext
{
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public List<string> PreviousQueries { get; set; } = new();
    public Dictionary<string, object> UserPreferences { get; set; } = new();
    public string? BusinessDomain { get; set; }
    public StreamingQueryComplexity PreferredComplexity { get; set; } = StreamingQueryComplexity.Medium;
}

/// <summary>
/// Analysis context for insight generation
/// </summary>
public class AnalysisContext
{
    public string? BusinessGoal { get; set; }
    public string? TimeFrame { get; set; }
    public List<string> KeyMetrics { get; set; } = new();
    public string? ComparisonPeriod { get; set; }
    public AnalysisType Type { get; set; } = AnalysisType.Descriptive;
}

/// <summary>
/// Query complexity levels for streaming AI
/// </summary>
public enum StreamingQueryComplexity
{
    Simple,
    Medium,
    Complex,
    Expert
}

/// <summary>
/// Analysis types for insights
/// </summary>
public enum AnalysisType
{
    Descriptive,
    Diagnostic,
    Predictive,
    Prescriptive
}


