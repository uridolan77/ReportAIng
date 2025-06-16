using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Comprehensive query execution metrics for ML analysis (consolidated from multiple duplicates)
/// </summary>
public class QueryMetrics
{
    // Individual query properties
    public string QueryId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string NaturalLanguageQuery { get; set; } = string.Empty;
    public string GeneratedSQL { get; set; } = string.Empty;
    public DateTime ExecutedAt { get; set; }
    public long ExecutionTimeMs { get; set; }
    public int RowCount { get; set; }
    public bool IsSuccessful { get; set; }
    public double? ConfidenceScore { get; set; }
    public string? ErrorMessage { get; set; }
    [NotMapped]
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();

    // Aggregate metrics (from GetQueryHistoryQuery)
    public int TotalQueries { get; set; }
    public int SuccessfulQueries { get; set; }
    public double AverageExecutionTime { get; set; }
    public double SuccessRate { get; set; }
    [NotMapped]
    public List<string> MostCommonErrors { get; set; } = new();
    [NotMapped]
    public List<string> PopularQuestions { get; set; } = new();

    // Additional properties expected by Infrastructure services
    public bool CacheHit { get; set; }
    public double NLUConfidence { get; set; } = 0.8;
    public double QueryIntelligenceScore { get; set; } = 0.8;
    public double OptimizationScore { get; set; } = 0.8;
}

/// <summary>
/// Result of anomaly detection analysis
/// </summary>
public class AnomalyDetectionResult
{
    public string QueryId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public double AnomalyScore { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public bool IsAnomalous { get; set; }
    public List<DetectedAnomaly> DetectedAnomalies { get; set; } = new();
    public DateTime AnalyzedAt { get; set; }
    public string? Recommendations { get; set; }
}

/// <summary>
/// Individual behavior anomaly detection
/// </summary>
public class BehaviorAnomaly
{
    public string AnomalyId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string AnomalyType { get; set; } = string.Empty;
    public double Severity { get; set; }
    public DateTime DetectedAt { get; set; }
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// User feedback for learning system
/// </summary>
public class UserFeedback
{
    public string FeedbackId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string QueryId { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5 scale
    public string? Comments { get; set; }
    public string FeedbackType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime ProvidedAt { get; set; }
    public bool IsProcessed { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Learning insights from user behavior
/// </summary>
public class LearningInsights
{
    public string InsightId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string InsightType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public DateTime GeneratedAt { get; set; }
    public InsightContext Context { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();

    // Additional properties for learning optimization
    public List<string> SuccessfulPatterns { get; set; } = new();
    public List<string> CommonMistakes { get; set; } = new();
    public List<string> OptimizationSuggestions { get; set; } = new();
    public Dictionary<string, object> UserPreferences { get; set; } = new();
    public Dictionary<string, double> PerformanceInsights { get; set; } = new();
}

/// <summary>
/// Query execution context for analysis
/// </summary>
public class QueryExecutionContext
{
    public string QueryId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public List<string> TablesAccessed { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public string? UserAgent { get; set; }
    public string? IpAddress { get; set; }
}

/// <summary>
/// Personalized recommendations for users
/// </summary>
public class PersonalizedRecommendation
{
    public string RecommendationId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public DateTime GeneratedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsViewed { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Detected anomaly details
/// </summary>
public class DetectedAnomaly
{
    public string AnomalyId { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Score { get; set; }
    public AnomalySeverity Severity { get; set; }
    public DateTime DetectedAt { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
}

/// <summary>
/// Context information for insights
/// </summary>
public class InsightContext
{
    public string Domain { get; set; } = string.Empty;
    public List<string> RelatedQueries { get; set; } = new();
    public Dictionary<string, double> Metrics { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public DateTime ContextDate { get; set; }
    public List<string> ContextualHints { get; set; } = new();
    public string QueryPattern { get; set; } = string.Empty;
}

/// <summary>
/// Risk level enumeration
/// </summary>
public enum RiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// Anomaly severity enumeration
/// </summary>
public enum AnomalySeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// ML model training data
/// </summary>
public class MLTrainingData
{
    public string DataId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string QueryText { get; set; } = string.Empty;
    public string GeneratedSQL { get; set; } = string.Empty;
    public double SuccessScore { get; set; }
    public List<string> Features { get; set; } = new();
    public Dictionary<string, object> Labels { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// ML model performance metrics
/// </summary>
public class MLModelMetrics
{
    public string ModelId { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public DateTime EvaluatedAt { get; set; }
    public Dictionary<string, double> AdditionalMetrics { get; set; } = new();
}

/// <summary>
/// Query pattern analysis result
/// </summary>
public class QueryPatternAnalysis
{
    public string PatternId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string PatternType { get; set; } = string.Empty;
    public double Frequency { get; set; }
    public List<string> CommonQueries { get; set; } = new();
    public Dictionary<string, object> PatternData { get; set; } = new();
    public DateTime AnalyzedAt { get; set; }
}

/// <summary>
/// Semantic similarity result
/// </summary>
public class SemanticSimilarityResult
{
    public string QueryId1 { get; set; } = string.Empty;
    public string QueryId2 { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public string SimilarityType { get; set; } = string.Empty;
    public List<string> CommonConcepts { get; set; } = new();
    public DateTime CalculatedAt { get; set; }
}

// SemanticCacheStatistics moved to SemanticCaching.cs to avoid duplicates

/// <summary>
/// Federated learning session
/// </summary>
public class FederatedLearningSession
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> Participants { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();

    // Additional properties expected by Infrastructure services
    public string ModelType { get; set; } = string.Empty;
}

/// <summary>
/// Federated learning request
/// </summary>
public class FederatedLearningRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string ParticipantId { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Local model for federated learning
/// </summary>
public class LocalModel
{
    public string ModelId { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public byte[] ModelData { get; set; } = Array.Empty<byte>();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Additional properties expected by Infrastructure services
    public Dictionary<string, double> Parameters { get; set; } = new();
}

/// <summary>
/// Aggregated model
/// </summary>
public class AggregatedModel
{
    public string ModelId { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public byte[] ModelData { get; set; } = Array.Empty<byte>();
    public List<string> SourceModels { get; set; } = new();
    public Dictionary<string, object> AggregationMetrics { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Additional properties expected by Infrastructure services
    public Dictionary<string, double> Parameters { get; set; } = new();
    public int ParticipatingClients { get; set; }
    public object Metadata { get; set; } = new();
}

/// <summary>
/// Model updates for federated learning
/// </summary>
public class ModelUpdates
{
    public string UpdateId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public byte[] UpdateData { get; set; } = Array.Empty<byte>();
    public Dictionary<string, object> Metrics { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Additional properties expected by Infrastructure services
    public Dictionary<string, double> Parameters { get; set; } = new();
    public Dictionary<string, double> Gradients { get; set; } = new();
    public int UpdateRound { get; set; }
}

/// <summary>
/// Federated aggregation result
/// </summary>
public class FederatedAggregationResult
{
    public string ResultId { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public AggregatedModel AggregatedModel { get; set; } = new();
    public Dictionary<string, object> AggregationMetrics { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Additional properties expected by Infrastructure services
    public string AggregationId { get; set; } = string.Empty;
    public int Round { get; set; }
    public double PrivacyBudgetUsed { get; set; }
    public bool Success { get; set; } = true;
    public AggregationMetrics Metrics { get; set; } = new();
}

/// <summary>
/// Local training data
/// </summary>
public class LocalTrainingData
{
    public string DataId { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Privatized training data
/// </summary>
public class PrivatizedTrainingData
{
    public string DataId { get; set; } = string.Empty;
    public byte[] PrivatizedData { get; set; } = Array.Empty<byte>();
    public Dictionary<string, object> PrivacyMetrics { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Training example
/// </summary>
public class TrainingExample
{
    public string ExampleId { get; set; } = string.Empty;
    public Dictionary<string, object> Features { get; set; } = new();
    public object Label { get; set; } = new();
    public double Weight { get; set; } = 1.0;
}

/// <summary>
/// Aggregation metrics for federated learning
/// </summary>
public class AggregationMetrics
{
    public int ParticipatingClients { get; set; }
    public double AverageAccuracy { get; set; }
    public double ModelConvergence { get; set; }
    public TimeSpan AggregationTime { get; set; }
    public double SecurityScore { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Local model training result
/// </summary>
public class LocalModelTrainingResult
{
    public string ResultId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public Dictionary<string, double> Metrics { get; set; } = new();
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Model validation result
/// </summary>
public class ModelValidationResult
{
    public string ValidationId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public Dictionary<string, double> ValidationMetrics { get; set; } = new();
    public bool IsValid { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Anomaly model for ML analysis
/// </summary>
public class AnomalyModel
{
    public string ModelId { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public byte[] ModelData { get; set; } = Array.Empty<byte>();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime TrainedAt { get; set; } = DateTime.UtcNow;
    public double Accuracy { get; set; }
}

/// <summary>
/// Learning model for ML analysis
/// </summary>
public class LearningModel
{
    public string ModelId { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public byte[] ModelData { get; set; } = Array.Empty<byte>();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime TrainedAt { get; set; } = DateTime.UtcNow;
    public double Accuracy { get; set; }
    public List<string> Features { get; set; } = new();
}

/// <summary>
/// Query optimization suggestion
/// </summary>
public class QueryOptimizationSuggestion
{
    public string SuggestionId { get; set; } = string.Empty;
    public string QueryId { get; set; } = string.Empty;
    public string SuggestionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OptimizedQuery { get; set; } = string.Empty;
    public double ExpectedImprovement { get; set; }
    public double Confidence { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    // Additional properties expected by Infrastructure services
    public string OriginalQuery { get; set; } = string.Empty;
    public List<OptimizationSuggestion> Suggestions { get; set; } = new();
    public double EstimatedImprovement { get; set; }
}

/// <summary>
/// Query trend analysis
/// </summary>
public class QueryTrendAnalysis
{
    public string AnalysisId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; } = DateTime.UtcNow;
    public Dictionary<string, double> Trends { get; set; } = new();
    public List<string> PopularQueries { get; set; } = new();
    public Dictionary<string, int> QueryFrequency { get; set; } = new();
    public double OverallPerformanceTrend { get; set; }

    // Additional properties expected by Infrastructure services
    public string TrendDirection { get; set; } = "stable";
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromDays(7);
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, double> TrendMetrics { get; set; } = new();
    public List<string> EmergingPatterns { get; set; } = new();
    public Dictionary<string, double> PerformanceTrends { get; set; } = new();
}
