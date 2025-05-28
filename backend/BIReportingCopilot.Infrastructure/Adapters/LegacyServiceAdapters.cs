using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.Adapters;

/// <summary>
/// Adapter to provide backward compatibility for IOpenAIService
/// </summary>
public class LegacyOpenAIServiceAdapter : IOpenAIService
{
    private readonly IAIService _aiService;

    public LegacyOpenAIServiceAdapter(IAIService aiService)
    {
        _aiService = aiService;
    }

    public async Task<string> GenerateSQLAsync(string prompt)
    {
        return await _aiService.GenerateSQLAsync(prompt);
    }

    public async Task<string> GenerateSQLAsync(string prompt, CancellationToken cancellationToken)
    {
        return await _aiService.GenerateSQLAsync(prompt, cancellationToken);
    }

    public async Task<string> GenerateInsightAsync(string query, object[] data)
    {
        return await _aiService.GenerateInsightAsync(query, data);
    }

    public async Task<string> GenerateVisualizationConfigAsync(string query, ColumnInfo[] columns, object[] data)
    {
        return await _aiService.GenerateVisualizationConfigAsync(query, columns, data);
    }

    public async Task<double> CalculateConfidenceScoreAsync(string naturalLanguageQuery, string generatedSQL)
    {
        return await _aiService.CalculateConfidenceScoreAsync(naturalLanguageQuery, generatedSQL);
    }

    public async Task<string[]> GenerateQuerySuggestionsAsync(string context, SchemaMetadata schema)
    {
        return await _aiService.GenerateQuerySuggestionsAsync(context, schema);
    }

    public async Task<bool> ValidateQueryIntentAsync(string naturalLanguageQuery)
    {
        return await _aiService.ValidateQueryIntentAsync(naturalLanguageQuery);
    }
}

/// <summary>
/// Adapter to provide backward compatibility for IStreamingOpenAIService
/// </summary>
public class LegacyStreamingOpenAIServiceAdapter : IStreamingOpenAIService
{
    private readonly IAIService _aiService;

    public LegacyStreamingOpenAIServiceAdapter(IAIService aiService)
    {
        _aiService = aiService;
    }

    public IAsyncEnumerable<StreamingResponse> GenerateSQLStreamAsync(
        string prompt, 
        SchemaMetadata? schema = null, 
        QueryContext? context = null, 
        CancellationToken cancellationToken = default)
    {
        return _aiService.GenerateSQLStreamAsync(prompt, schema, context, cancellationToken);
    }

    public IAsyncEnumerable<StreamingResponse> GenerateInsightStreamAsync(
        string query, 
        object[] data, 
        AnalysisContext? context = null, 
        CancellationToken cancellationToken = default)
    {
        return _aiService.GenerateInsightStreamAsync(query, data, context, cancellationToken);
    }

    public IAsyncEnumerable<StreamingResponse> GenerateExplanationStreamAsync(
        string sql, 
        StreamingQueryComplexity complexity = StreamingQueryComplexity.Medium, 
        CancellationToken cancellationToken = default)
    {
        return _aiService.GenerateExplanationStreamAsync(sql, complexity, cancellationToken);
    }
}
