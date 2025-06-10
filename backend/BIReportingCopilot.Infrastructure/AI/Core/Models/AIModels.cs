using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Infrastructure.AI.Core.Models;

/// <summary>
/// Extracted entity from NLU processing
/// </summary>
public class ExtractedEntity
{
    public string EntityId { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; } = 0.8;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public string Context { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime ExtractedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Intent classification result
/// </summary>
public class IntentClassification
{
    public string IntentId { get; set; } = Guid.NewGuid().ToString();
    public string Intent { get; set; } = string.Empty;
    public double Confidence { get; set; } = 0.8;
    public List<string> Keywords { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public DateTime ClassifiedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Semantic analysis result
/// </summary>
public class SemanticAnalysisResult
{
    public string AnalysisId { get; set; } = Guid.NewGuid().ToString();
    public string Query { get; set; } = string.Empty;
    public List<ExtractedEntity> Entities { get; set; } = new();
    public IntentClassification Intent { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public Dictionary<string, double> TopicScores { get; set; } = new();
    public double OverallConfidence { get; set; } = 0.8;
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Query context information
/// </summary>
public class QueryContext
{
    public string ContextId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public List<string> PreviousQueries { get; set; } = new();
    public Dictionary<string, object> UserPreferences { get; set; } = new();
    public Dictionary<string, object> SessionData { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// NLU processing result
/// </summary>
public class NLUResult
{
    public string ResultId { get; set; } = Guid.NewGuid().ToString();
    public string OriginalQuery { get; set; } = string.Empty;
    public SemanticAnalysisResult SemanticAnalysis { get; set; } = new();
    public QueryContext Context { get; set; } = new();
    public List<string> SuggestedTables { get; set; } = new();
    public List<string> SuggestedColumns { get; set; } = new();
    public double ProcessingConfidence { get; set; } = 0.8;
    public TimeSpan ProcessingTime { get; set; }
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Query optimization insight
/// </summary>
public class QueryOptimizationInsight
{
    public string InsightId { get; set; } = Guid.NewGuid().ToString();
    public string QueryId { get; set; } = string.Empty;
    public string InsightType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public double ImpactScore { get; set; } = 0.5;
    public double Confidence { get; set; } = 0.8;
    public List<string> AffectedTables { get; set; } = new();
    public List<string> AffectedColumns { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Performance prediction result
/// </summary>
public class PerformancePrediction
{
    public string PredictionId { get; set; } = Guid.NewGuid().ToString();
    public string QueryId { get; set; } = string.Empty;
    public TimeSpan EstimatedExecutionTime { get; set; }
    public long EstimatedRowCount { get; set; }
    public double EstimatedCost { get; set; }
    public string PerformanceCategory { get; set; } = "Medium"; // Fast, Medium, Slow
    public List<string> PerformanceWarnings { get; set; } = new();
    public List<QueryOptimizationInsight> OptimizationSuggestions { get; set; } = new();
    public double Confidence { get; set; } = 0.8;
    public DateTime PredictedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Learning model metadata
/// </summary>
public class LearningModelMetadata
{
    public string ModelId { get; set; } = Guid.NewGuid().ToString();
    public string ModelName { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public DateTime TrainedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public double Accuracy { get; set; } = 0.8;
    public long TrainingDataSize { get; set; }
    public Dictionary<string, object> Hyperparameters { get; set; } = new();
    public Dictionary<string, double> Metrics { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Model metadata for federated learning
/// </summary>
public class ModelMetadata
{
    public string ModelType { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string Architecture { get; set; } = string.Empty;
    public Dictionary<string, object> Hyperparameters { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public Dictionary<string, double> Metrics { get; set; } = new();
}

/// <summary>
/// Federated learning update
/// </summary>
public class FederatedLearningUpdate
{
    public string UpdateId { get; set; } = Guid.NewGuid().ToString();
    public string ModelId { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public byte[] ModelWeights { get; set; } = Array.Empty<byte>();
    public Dictionary<string, double> PerformanceMetrics { get; set; } = new();
    public long TrainingDataSize { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsValidated { get; set; } = false;
}

/// <summary>
/// AI model training result
/// </summary>
public class ModelTrainingResult
{
    public string TrainingId { get; set; } = Guid.NewGuid().ToString();
    public string ModelId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, double> Metrics { get; set; } = new();
    public TimeSpan TrainingDuration { get; set; }
    public long DataPointsProcessed { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public List<string> Warnings { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Template optimization result
/// </summary>
public class TemplateOptimizationResult
{
    public string OptimizationId { get; set; } = Guid.NewGuid().ToString();
    public string TemplateId { get; set; } = string.Empty;
    public string OptimizedTemplate { get; set; } = string.Empty;
    public double ImprovementScore { get; set; } = 0.0;
    public List<string> OptimizationSteps { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime OptimizedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Dashboard template analysis result
/// </summary>
public class DashboardTemplateAnalysis
{
    public string AnalysisId { get; set; } = Guid.NewGuid().ToString();
    public string TemplateId { get; set; } = string.Empty;
    public List<string> RecommendedComponents { get; set; } = new();
    public Dictionary<string, double> ComponentScores { get; set; } = new();
    public List<string> OptimizationSuggestions { get; set; } = new();
    public double OverallScore { get; set; } = 0.8;
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Query intent classification
/// </summary>
public class QueryIntent
{
    public string IntentId { get; set; } = Guid.NewGuid().ToString();
    public string Intent { get; set; } = string.Empty;
    public double Confidence { get; set; } = 0.8;
    public List<string> Keywords { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public DateTime ClassifiedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Intent candidate for classification
/// </summary>
public class IntentCandidate
{
    public string CandidateId { get; set; } = Guid.NewGuid().ToString();
    public string Intent { get; set; } = string.Empty;
    public double Score { get; set; } = 0.0;
    public List<string> MatchedKeywords { get; set; } = new();
    public Dictionary<string, object> Evidence { get; set; } = new();
    public string Reasoning { get; set; } = string.Empty;
}

/// <summary>
/// Entity relation between extracted entities
/// </summary>
public class EntityRelation
{
    public string RelationId { get; set; } = Guid.NewGuid().ToString();
    public string SourceEntityId { get; set; } = string.Empty;
    public string TargetEntityId { get; set; } = string.Empty;
    public string RelationType { get; set; } = string.Empty;
    public double Confidence { get; set; } = 0.8;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Cache query classification result
/// </summary>
public class CacheQueryClassification
{
    public string ClassificationId { get; set; } = Guid.NewGuid().ToString();
    public string Query { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Confidence { get; set; } = 0.8;
    public List<string> Tags { get; set; } = new();
    public DateTime ClassifiedAt { get; set; } = DateTime.UtcNow;

    // Additional properties expected by Infrastructure services
    public string Type { get; set; } = "analytical";
    public string Domain { get; set; } = "general";
    public string Complexity { get; set; } = "medium";
}

/// <summary>
/// Cache performance metrics
/// </summary>
public class CachePerformanceMetrics
{
    public string MetricsId { get; set; } = Guid.NewGuid().ToString();
    public double HitRate { get; set; } = 0.0;
    public double MissRate { get; set; } = 0.0;
    public TimeSpan AverageRetrievalTime { get; set; }
    public long TotalRequests { get; set; }
    public long CacheSize { get; set; }
    public DateTime MeasuredAt { get; set; } = DateTime.UtcNow;

    // Additional properties expected by Infrastructure services
    public TimeSpan OriginalExecutionTime { get; set; }
    public long ResultSizeBytes { get; set; }
    public TimeSpan AverageHitTime { get; set; }
    public TimeSpan AverageMissTime { get; set; }
    public double HitRatio { get; set; }
}
