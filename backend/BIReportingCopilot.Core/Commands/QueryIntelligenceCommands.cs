using MediatR;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Commands;

/// <summary>
/// Command for advanced NLU analysis
/// </summary>
public class AnalyzeNLUCommand : IRequest<AdvancedNLUResult>
{
    public string NaturalLanguageQuery { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public NLUAnalysisContext? Context { get; set; }
}

/// <summary>
/// Command for schema optimization analysis
/// </summary>
public class AnalyzeSchemaOptimizationCommand : IRequest<QueryOptimizationResult>
{
    public string Sql { get; set; } = string.Empty;
    public SchemaMetadata Schema { get; set; } = new();
    public QueryExecutionMetrics? Metrics { get; set; }
}

/// <summary>
/// Command for comprehensive query intelligence analysis
/// </summary>
public class AnalyzeQueryIntelligenceCommand : IRequest<QueryIntelligenceResult>
{
    public string NaturalLanguageQuery { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public SchemaMetadata Schema { get; set; } = new();
}

/// <summary>
/// Command for generating index suggestions
/// </summary>
public class GenerateIndexSuggestionsCommand : IRequest<List<IndexSuggestion>>
{
    public List<QueryHistoryItem> QueryHistory { get; set; } = new();
    public SchemaMetadata Schema { get; set; } = new();

    // Additional properties for compatibility with API controllers
    public List<string> QueryPatterns { get; set; } = new();
    public List<PerformanceGoal> PerformanceGoals { get; set; } = new();
}

/// <summary>
/// Command for SQL optimization
/// </summary>
public class OptimizeSqlCommand : IRequest<SqlOptimizationResult>
{
    public string OriginalSql { get; set; } = string.Empty;
    public SchemaMetadata Schema { get; set; } = new();
    public OptimizationGoals? Goals { get; set; }
}

/// <summary>
/// Command for schema health analysis
/// </summary>
public class AnalyzeSchemaHealthCommand : IRequest<SchemaHealthAnalysis>
{
    public SchemaMetadata Schema { get; set; } = new();

    // Additional properties for compatibility with API controllers
    public bool IncludePerformanceAnalysis { get; set; } = true;
    public bool IncludeSecurityAnalysis { get; set; } = true;
}

/// <summary>
/// Command for learning from user interactions
/// </summary>
public class LearnFromInteractionCommand : IRequest<bool>
{
    public string UserId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public QueryResponse Response { get; set; } = new();
    public UserFeedback? Feedback { get; set; }
}

/// <summary>
/// Command for training NLU models
/// </summary>
public class TrainNLUModelsCommand : IRequest<bool>
{
    public List<NLUTrainingData> TrainingData { get; set; } = new();
    public string? Domain { get; set; }
}

/// <summary>
/// Query for getting intelligent query suggestions
/// </summary>
public class GetIntelligentSuggestionsQuery : IRequest<List<IntelligentQuerySuggestion>>
{
    public string UserId { get; set; } = string.Empty;
    public SchemaMetadata Schema { get; set; } = new();
    public NLUAnalysisContext? Context { get; set; }
}

/// <summary>
/// Query for getting query assistance
/// </summary>
public class GetQueryAssistanceQuery : IRequest<QueryAssistance>
{
    public string PartialQuery { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public SchemaMetadata Schema { get; set; } = new();
}

/// <summary>
/// Query for getting NLU metrics
/// </summary>
public class GetNLUMetricsQuery : IRequest<NLUMetrics>
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    // Additional properties for compatibility
    public string? UserId { get; set; }
    public TimeSpan? TimeWindow { get; set; }
}

/// <summary>
/// Query for getting schema optimization metrics
/// </summary>
public class GetSchemaOptimizationMetricsQuery : IRequest<SchemaOptimizationMetrics>
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    // Additional properties for compatibility
    public TimeSpan? TimeWindow { get; set; }
}

/// <summary>
/// Query for conversation analysis
/// </summary>
public class AnalyzeConversationQuery : IRequest<ConversationAnalysis>
{
    public string UserId { get; set; } = string.Empty;
    public TimeSpan? AnalysisWindow { get; set; }
}

/// <summary>
/// Query for performance trend analysis
/// </summary>
public class AnalyzePerformanceTrendsQuery : IRequest<PerformanceTrendAnalysis>
{
    public List<QueryHistoryItem> QueryHistory { get; set; } = new();
    public TimeSpan AnalysisWindow { get; set; } = TimeSpan.FromDays(30);
}

/// <summary>
/// Query for execution plan analysis
/// </summary>
public class AnalyzeExecutionPlanQuery : IRequest<ExecutionPlanAnalysis>
{
    public string Sql { get; set; } = string.Empty;
    public SchemaMetadata Schema { get; set; } = new();
}

/// <summary>
/// Query for query rewrite suggestions
/// </summary>
public class GetQueryRewriteSuggestionsQuery : IRequest<List<QueryRewrite>>
{
    public string OriginalSql { get; set; } = string.Empty;
    public SchemaMetadata Schema { get; set; } = new();
}

/// <summary>
/// Query for maintenance recommendations
/// </summary>
public class GetMaintenanceRecommendationsQuery : IRequest<List<MaintenanceRecommendation>>
{
    public SchemaMetadata Schema { get; set; } = new();
    public List<QueryHistoryItem> QueryHistory { get; set; } = new();
}

/// <summary>
/// Command for updating NLU configuration
/// </summary>
public class UpdateNLUConfigurationCommand : IRequest<bool>
{
    public NLUConfiguration Configuration { get; set; } = new();
}

/// <summary>
/// Query for intent classification
/// </summary>
public class ClassifyIntentQuery : IRequest<IntentAnalysis>
{
    public string Query { get; set; } = string.Empty;
    public string? UserId { get; set; }
}

/// <summary>
/// Query for entity extraction
/// </summary>
public class ExtractEntitiesQuery : IRequest<EntityAnalysis>
{
    public string Query { get; set; } = string.Empty;
    public SchemaMetadata? Schema { get; set; }
}

/// <summary>
/// Query for contextual analysis
/// </summary>
public class AnalyzeContextQuery : IRequest<ContextualAnalysis>
{
    public string Query { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public List<string>? ConversationHistory { get; set; }
}

/// <summary>
/// Query for smart query suggestions
/// </summary>
public class GetSmartSuggestionsQuery : IRequest<List<QuerySuggestion>>
{
    public string PartialQuery { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public SchemaMetadata? Schema { get; set; }
}

/// <summary>
/// Query for query improvement suggestions
/// </summary>
public class SuggestQueryImprovementsQuery : IRequest<QueryImprovement>
{
    public string OriginalQuery { get; set; } = string.Empty;
    public AdvancedNLUResult NLUResult { get; set; } = new();
}
