using MediatR;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Query;

namespace BIReportingCopilot.Infrastructure.Handlers;

/// <summary>
/// Command handler for advanced NLU analysis
/// </summary>
public class AnalyzeNLUCommandHandler : IRequestHandler<AnalyzeNLUCommand, AdvancedNLUResult>
{
    private readonly ILogger<AnalyzeNLUCommandHandler> _logger;
    private readonly IAdvancedNLUService _nluService;

    public AnalyzeNLUCommandHandler(
        ILogger<AnalyzeNLUCommandHandler> logger,
        IAdvancedNLUService nluService)
    {
        _logger = logger;
        _nluService = nluService;
    }

    public async Task<AdvancedNLUResult> Handle(AnalyzeNLUCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("🧠 Processing advanced NLU analysis command for user {UserId}", request.UserId);

            var result = await _nluService.AnalyzeQueryAsync(
                request.NaturalLanguageQuery, 
                request.UserId, 
                request.Context);

            _logger.LogInformation("🧠 Advanced NLU analysis completed - Confidence: {Confidence:P2}, Intent: {Intent}",
                result.ConfidenceScore, result.IntentAnalysis.PrimaryIntent);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in advanced NLU analysis command handler");
            return new AdvancedNLUResult
            {
                OriginalQuery = request.NaturalLanguageQuery,
                Error = ex.Message,
                ConfidenceScore = 0.0
            };
        }
    }
}

/// <summary>
/// Command handler for schema optimization analysis
/// </summary>
public class AnalyzeSchemaOptimizationCommandHandler : IRequestHandler<AnalyzeSchemaOptimizationCommand, QueryOptimizationResult>
{
    private readonly ILogger<AnalyzeSchemaOptimizationCommandHandler> _logger;
    private readonly ISchemaOptimizationService _schemaOptimizationService;

    public AnalyzeSchemaOptimizationCommandHandler(
        ILogger<AnalyzeSchemaOptimizationCommandHandler> logger,
        ISchemaOptimizationService schemaOptimizationService)
    {
        _logger = logger;
        _schemaOptimizationService = schemaOptimizationService;
    }

    public async Task<QueryOptimizationResult> Handle(AnalyzeSchemaOptimizationCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("⚡ Processing schema optimization analysis command");

            var result = await _schemaOptimizationService.AnalyzeQueryPerformanceAsync(
                request.Sql, 
                request.Schema, 
                request.Metrics);

            _logger.LogInformation("⚡ Schema optimization analysis completed - Improvement Score: {Score:P2}",
                result.ImprovementScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in schema optimization analysis command handler");
            return new QueryOptimizationResult
            {
                OriginalSql = request.Sql,
                OptimizedSql = request.Sql,
                ImprovementScore = 0.0
            };
        }
    }
}

/// <summary>
/// Command handler for comprehensive query intelligence analysis
/// </summary>
public class AnalyzeQueryIntelligenceCommandHandler : IRequestHandler<AnalyzeQueryIntelligenceCommand, QueryIntelligenceResult>
{
    private readonly ILogger<AnalyzeQueryIntelligenceCommandHandler> _logger;
    private readonly IQueryIntelligenceService _queryIntelligenceService;

    public AnalyzeQueryIntelligenceCommandHandler(
        ILogger<AnalyzeQueryIntelligenceCommandHandler> logger,
        IQueryIntelligenceService queryIntelligenceService)
    {
        _logger = logger;
        _queryIntelligenceService = queryIntelligenceService;
    }

    public async Task<QueryIntelligenceResult> Handle(AnalyzeQueryIntelligenceCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("🧠⚡ Processing comprehensive query intelligence analysis command");

            var result = await _queryIntelligenceService.AnalyzeQueryAsync(
                request.NaturalLanguageQuery, 
                request.UserId, 
                request.Schema);

            _logger.LogInformation("🧠⚡ Query intelligence analysis completed - Overall Score: {Score:P2}",
                result.OverallScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in query intelligence analysis command handler");
            return new QueryIntelligenceResult
            {
                NLUResult = new AdvancedNLUResult
                {
                    OriginalQuery = request.NaturalLanguageQuery,
                    Error = ex.Message
                },
                OptimizationResult = new QueryOptimizationResult
                {
                    OriginalSql = "",
                    ImprovementScore = 0.0
                },
                OverallScore = 0.0
            };
        }
    }
}

/// <summary>
/// Command handler for generating index suggestions
/// </summary>
public class GenerateIndexSuggestionsCommandHandler : IRequestHandler<GenerateIndexSuggestionsCommand, List<IndexSuggestion>>
{
    private readonly ILogger<GenerateIndexSuggestionsCommandHandler> _logger;
    private readonly ISchemaOptimizationService _schemaOptimizationService;

    public GenerateIndexSuggestionsCommandHandler(
        ILogger<GenerateIndexSuggestionsCommandHandler> logger,
        ISchemaOptimizationService schemaOptimizationService)
    {
        _logger = logger;
        _schemaOptimizationService = schemaOptimizationService;
    }

    public async Task<List<IndexSuggestion>> Handle(GenerateIndexSuggestionsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("📊 Processing index suggestions generation command");

            var suggestions = await _schemaOptimizationService.SuggestIndexesAsync(
                request.QueryHistory, 
                request.Schema);

            _logger.LogInformation("📊 Index suggestions generated - Count: {Count}", suggestions.Count);

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in index suggestions generation command handler");
            return new List<IndexSuggestion>();
        }
    }
}

/// <summary>
/// Command handler for SQL optimization
/// </summary>
public class OptimizeSqlCommandHandler : IRequestHandler<OptimizeSqlCommand, SqlOptimizationResult>
{
    private readonly ILogger<OptimizeSqlCommandHandler> _logger;
    private readonly ISchemaOptimizationService _schemaOptimizationService;

    public OptimizeSqlCommandHandler(
        ILogger<OptimizeSqlCommandHandler> logger,
        ISchemaOptimizationService schemaOptimizationService)
    {
        _logger = logger;
        _schemaOptimizationService = schemaOptimizationService;
    }

    public async Task<SqlOptimizationResult> Handle(OptimizeSqlCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("⚡ Processing SQL optimization command");

            var result = await _schemaOptimizationService.OptimizeSqlAsync(
                request.OriginalSql, 
                request.Schema, 
                request.Goals);

            _logger.LogInformation("⚡ SQL optimization completed - Confidence: {Confidence:P2}",
                result.ConfidenceScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in SQL optimization command handler");
            return new SqlOptimizationResult
            {
                OriginalSql = request.OriginalSql,
                OptimizedSql = request.OriginalSql,
                ConfidenceScore = 0.0
            };
        }
    }
}

/// <summary>
/// Command handler for learning from user interactions
/// </summary>
public class LearnFromInteractionCommandHandler : IRequestHandler<LearnFromInteractionCommand, bool>
{
    private readonly ILogger<LearnFromInteractionCommandHandler> _logger;
    private readonly IQueryIntelligenceService _queryIntelligenceService;

    public LearnFromInteractionCommandHandler(
        ILogger<LearnFromInteractionCommandHandler> logger,
        IQueryIntelligenceService queryIntelligenceService)
    {
        _logger = logger;
        _queryIntelligenceService = queryIntelligenceService;
    }

    public async Task<bool> Handle(LearnFromInteractionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("📚 Processing learning from interaction command for user {UserId}", request.UserId);

            await _queryIntelligenceService.LearnFromInteractionAsync(
                request.UserId, 
                request.Query, 
                request.Response, 
                request.Feedback);

            _logger.LogInformation("📚 Learning from interaction completed for user {UserId}", request.UserId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in learning from interaction command handler");
            return false;
        }
    }
}

/// <summary>
/// Query handler for getting intelligent suggestions
/// </summary>
public class GetIntelligentSuggestionsQueryHandler : IRequestHandler<GetIntelligentSuggestionsQuery, List<IntelligentQuerySuggestion>>
{
    private readonly ILogger<GetIntelligentSuggestionsQueryHandler> _logger;
    private readonly IQueryIntelligenceService _queryIntelligenceService;

    public GetIntelligentSuggestionsQueryHandler(
        ILogger<GetIntelligentSuggestionsQueryHandler> logger,
        IQueryIntelligenceService queryIntelligenceService)
    {
        _logger = logger;
        _queryIntelligenceService = queryIntelligenceService;
    }

    public async Task<List<IntelligentQuerySuggestion>> Handle(GetIntelligentSuggestionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("💡 Processing intelligent suggestions query for user {UserId}", request.UserId);

            var suggestions = await _queryIntelligenceService.GenerateIntelligentSuggestionsAsync(
                request.UserId,
                request.Schema,
                request.Context?.UserId); // Convert NLUAnalysisContext to string

            _logger.LogInformation("💡 Intelligent suggestions generated - Count: {Count}", suggestions.Count);

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in intelligent suggestions query handler");
            return new List<IntelligentQuerySuggestion>();
        }
    }
}

/// <summary>
/// Query handler for getting query assistance
/// </summary>
public class GetQueryAssistanceQueryHandler : IRequestHandler<GetQueryAssistanceQuery, QueryAssistance>
{
    private readonly ILogger<GetQueryAssistanceQueryHandler> _logger;
    private readonly IQueryIntelligenceService _queryIntelligenceService;

    public GetQueryAssistanceQueryHandler(
        ILogger<GetQueryAssistanceQueryHandler> logger,
        IQueryIntelligenceService queryIntelligenceService)
    {
        _logger = logger;
        _queryIntelligenceService = queryIntelligenceService;
    }

    public async Task<QueryAssistance> Handle(GetQueryAssistanceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("🤝 Processing query assistance query for user {UserId}", request.UserId);

            var assistance = await _queryIntelligenceService.GetQueryAssistanceAsync(
                request.PartialQuery, 
                request.UserId, 
                request.Schema);

            _logger.LogInformation("🤝 Query assistance provided - Autocomplete: {AutocompleteCount}, Hints: {HintCount}",
                assistance.AutocompleteSuggestions.Count, assistance.PerformanceHints.Count);

            return assistance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in query assistance query handler");
            return new QueryAssistance();
        }
    }
}

/// <summary>
/// Query handler for NLU metrics
/// </summary>
public class GetNLUMetricsQueryHandler : IRequestHandler<GetNLUMetricsQuery, NLUMetrics>
{
    private readonly ILogger<GetNLUMetricsQueryHandler> _logger;
    private readonly IAdvancedNLUService _nluService;

    public GetNLUMetricsQueryHandler(
        ILogger<GetNLUMetricsQueryHandler> logger,
        IAdvancedNLUService nluService)
    {
        _logger = logger;
        _nluService = nluService;
    }

    public async Task<NLUMetrics> Handle(GetNLUMetricsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("📊 Processing NLU metrics query");

            var metrics = await _nluService.GetMetricsAsync();

            _logger.LogInformation("📊 NLU metrics retrieved - Analyses: {Count}, Avg Confidence: {Confidence:P2}",
                metrics.TotalAnalyses, metrics.AverageConfidence);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in NLU metrics query handler");
            return new NLUMetrics();
        }
    }
}

/// <summary>
/// Query handler for schema optimization metrics
/// </summary>
public class GetSchemaOptimizationMetricsQueryHandler : IRequestHandler<GetSchemaOptimizationMetricsQuery, SchemaOptimizationMetrics>
{
    private readonly ILogger<GetSchemaOptimizationMetricsQueryHandler> _logger;
    private readonly ISchemaOptimizationService _schemaOptimizationService;

    public GetSchemaOptimizationMetricsQueryHandler(
        ILogger<GetSchemaOptimizationMetricsQueryHandler> logger,
        ISchemaOptimizationService schemaOptimizationService)
    {
        _logger = logger;
        _schemaOptimizationService = schemaOptimizationService;
    }

    public async Task<SchemaOptimizationMetrics> Handle(GetSchemaOptimizationMetricsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("📊 Processing schema optimization metrics query");

            var metrics = await _schemaOptimizationService.GetOptimizationMetricsAsync();

            _logger.LogInformation("📊 Schema optimization metrics retrieved - Optimizations: {Count}, Avg Improvement: {Improvement:P2}",
                metrics.TotalOptimizations, metrics.AverageImprovementScore);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in schema optimization metrics query handler");
            return new SchemaOptimizationMetrics();
        }
    }
}

/// <summary>
/// Query handler for intent classification
/// </summary>
public class ClassifyIntentQueryHandler : IRequestHandler<ClassifyIntentQuery, IntentAnalysis>
{
    private readonly ILogger<ClassifyIntentQueryHandler> _logger;
    private readonly IAdvancedNLUService _nluService;

    public ClassifyIntentQueryHandler(
        ILogger<ClassifyIntentQueryHandler> logger,
        IAdvancedNLUService nluService)
    {
        _logger = logger;
        _nluService = nluService;
    }

    public async Task<IntentAnalysis> Handle(ClassifyIntentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("🎯 Processing intent classification query");

            var result = await _nluService.ClassifyIntentAsync(request.Query, request.UserId);

            _logger.LogInformation("🎯 Intent classified - Primary: {Intent}, Confidence: {Confidence:P2}",
                result.PrimaryIntent, result.Confidence);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in intent classification query handler");
            return new IntentAnalysis
            {
                PrimaryIntent = "DataQuery",
                Confidence = 0.0
            };
        }
    }
}
