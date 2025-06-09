using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Query execution metrics for ML analysis
/// </summary>
public class QueryMetrics
{
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
    public Dictionary<string, object> AdditionalMetrics { get; set; } = new();
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
