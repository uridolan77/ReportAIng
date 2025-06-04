using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.Performance;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Federated learning configuration
/// </summary>
public class FederatedLearningConfiguration
{
    public bool EnableFederatedLearning { get; set; } = true;
    public bool EnableDifferentialPrivacy { get; set; } = true;
    public bool EnableSecureAggregation { get; set; } = true;
    public double InitialPrivacyBudget { get; set; } = 1.0;
    public int MinimumPrivacyLevel { get; set; } = 5;
    public int MinimumDataSize { get; set; } = 1000;
    public int MaxClientsPerRound { get; set; } = 100;
    public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromHours(24);
    public string EncryptionAlgorithm { get; set; } = "AES-256";
    public double NoiseMultiplier { get; set; } = 1.1;
}

/// <summary>
/// Federated learning request
/// </summary>
public class FederatedLearningRequest
{
    public string ModelType { get; set; } = string.Empty;
    public string ClientType { get; set; } = string.Empty;
    public int EstimatedDataSize { get; set; }
    public int PrivacyLevel { get; set; }
    public FederatedSessionConfiguration Configuration { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Federated learning session
/// </summary>
public class FederatedLearningSession
{
    public string SessionId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public string LocalModelId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime LastActivity { get; set; }
    public FederatedSessionStatus Status { get; set; }
    public double PrivacyBudget { get; set; }
    public FederatedSessionConfiguration Configuration { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Federated session configuration
/// </summary>
public class FederatedSessionConfiguration
{
    public int MaxTrainingRounds { get; set; } = 10;
    public double LearningRate { get; set; } = 0.01;
    public int BatchSize { get; set; } = 32;
    public bool EnableEarlystopping { get; set; } = true;
    public double ConvergenceThreshold { get; set; } = 0.001;
    public Dictionary<string, object> ModelParameters { get; set; } = new();
}

/// <summary>
/// Local training data
/// </summary>
public class LocalTrainingData
{
    public string DataId { get; set; } = string.Empty;
    public List<TrainingExample> Examples { get; set; } = new();
    public DataStatistics Statistics { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Training example
/// </summary>
public class TrainingExample
{
    public Dictionary<string, object> Features { get; set; } = new();
    public object Label { get; set; } = new();
    public double Weight { get; set; } = 1.0;
}

/// <summary>
/// Data statistics
/// </summary>
public class DataStatistics
{
    public int SampleCount { get; set; }
    public int FeatureCount { get; set; }
    public Dictionary<string, double> FeatureStatistics { get; set; } = new();
    public Dictionary<string, int> LabelDistribution { get; set; } = new();
}

/// <summary>
/// Privatized training data
/// </summary>
public class PrivatizedTrainingData
{
    public string DataId { get; set; } = string.Empty;
    public List<TrainingExample> PrivatizedExamples { get; set; } = new();
    public double EpsilonUsed { get; set; }
    public double DeltaUsed { get; set; }
    public double NoiseLevel { get; set; }
    public double UtilityScore { get; set; }
    public PrivacyMechanism MechanismUsed { get; set; }
}

/// <summary>
/// Local training result
/// </summary>
public class LocalTrainingResult
{
    public string SessionId { get; set; } = string.Empty;
    public string TrainingId { get; set; } = string.Empty;
    public ModelUpdates ModelUpdates { get; set; } = new();
    public TrainingMetrics TrainingMetrics { get; set; } = new();
    public PrivacyMetrics PrivacyMetrics { get; set; } = new();
    public ModelValidationResult ValidationResult { get; set; } = new();
    public DateTime CompletedAt { get; set; }
    public bool Success { get; set; }
}

/// <summary>
/// Model updates
/// </summary>
public class ModelUpdates
{
    public string ModelId { get; set; } = string.Empty;
    public string UpdateId { get; set; } = string.Empty;
    public Dictionary<string, double> Parameters { get; set; } = new();
    public Dictionary<string, double> Gradients { get; set; } = new();
    public int UpdateRound { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Training metrics
/// </summary>
public class TrainingMetrics
{
    public double Accuracy { get; set; }
    public double Loss { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public TimeSpan TrainingTime { get; set; }
    public int Epochs { get; set; }
    public Dictionary<string, double> CustomMetrics { get; set; } = new();
}

/// <summary>
/// Privacy metrics
/// </summary>
public class PrivacyMetrics
{
    public double EpsilonUsed { get; set; }
    public double DeltaUsed { get; set; }
    public double PrivacyBudgetRemaining { get; set; }
    public double NoiseLevel { get; set; }
    public double DataUtilityScore { get; set; }
    public PrivacyMechanism MechanismUsed { get; set; }
}

/// <summary>
/// Model validation result
/// </summary>
public class ModelValidationResult
{
    public bool IsValid { get; set; }
    public double ValidationScore { get; set; }
    public List<string> ValidationErrors { get; set; } = new();
    public Dictionary<string, double> ValidationMetrics { get; set; } = new();
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Federated aggregation result
/// </summary>
public class FederatedAggregationResult
{
    public string AggregationId { get; set; } = string.Empty;
    public int Round { get; set; }
    public AggregatedModel AggregatedModel { get; set; } = new();
    public double PrivacyBudgetUsed { get; set; }
    public AggregationMetrics Metrics { get; set; } = new();
    public bool Success { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Aggregated model
/// </summary>
public class AggregatedModel
{
    public string ModelId { get; set; } = string.Empty;
    public Dictionary<string, double> Parameters { get; set; } = new();
    public ModelMetadata Metadata { get; set; } = new();
    public int ParticipatingClients { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Model metadata
/// </summary>
public class ModelMetadata
{
    public string ModelType { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public Dictionary<string, object> Architecture { get; set; } = new();
    public Dictionary<string, object> Hyperparameters { get; set; } = new();
}

/// <summary>
/// Aggregation metrics
/// </summary>
public class AggregationMetrics
{
    public int ParticipatingClients { get; set; }
    public double AverageAccuracy { get; set; }
    public double ModelConvergence { get; set; }
    public TimeSpan AggregationTime { get; set; }
    public double SecurityScore { get; set; }
}

/// <summary>
/// Client eligibility
/// </summary>
public class ClientEligibility
{
    public bool IsEligible { get; set; }
    public string Reason { get; set; } = string.Empty;
    public List<string> Requirements { get; set; } = new();
}

/// <summary>
/// Client info
/// </summary>
public class ClientInfo
{
    public string ClientId { get; set; } = string.Empty;
    public bool IsRegistered { get; set; }
    public string PermissionLevel { get; set; } = string.Empty;
    public DateTime LastActivity { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Local model
/// </summary>
public class LocalModel
{
    public string ModelId { get; set; } = string.Empty;
    public string ModelType { get; set; } = string.Empty;
    public Dictionary<string, double> Parameters { get; set; } = new();
    public ModelMetadata Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Local model training result
/// </summary>
public class LocalModelTrainingResult
{
    public string ModelId { get; set; } = string.Empty;
    public int TrainingRounds { get; set; }
    public TrainingMetrics Metrics { get; set; } = new();
    public bool Success { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Federated learning analytics
/// </summary>
public class FederatedLearningAnalytics
{
    public TimeSpan Period { get; set; }
    public int TotalSessions { get; set; }
    public int ActiveClients { get; set; }
    public int SuccessfulTrainingRounds { get; set; }
    public double AveragePrivacyBudgetUsed { get; set; }
    public ModelPerformanceMetrics ModelPerformanceMetrics { get; set; } = new();
    public PrivacyPreservationMetrics PrivacyPreservationMetrics { get; set; } = new();
    public ClientParticipationMetrics ClientParticipationMetrics { get; set; } = new();
    public SecurityMetrics SecurityMetrics { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Model performance metrics
/// </summary>
public class ModelPerformanceMetrics
{
    public double AverageAccuracy { get; set; }
    public double AverageLoss { get; set; }
    public double ConvergenceRate { get; set; }
    public double ModelStability { get; set; }
}

/// <summary>
/// Privacy preservation metrics
/// </summary>
public class PrivacyPreservationMetrics
{
    public double AverageEpsilonUsed { get; set; }
    public double AverageDeltaUsed { get; set; }
    public double PrivacyBudgetUtilization { get; set; }
    public double DataUtilityPreservation { get; set; }
}

/// <summary>
/// Client participation metrics
/// </summary>
public class ClientParticipationMetrics
{
    public int TotalClients { get; set; }
    public int ActiveClients { get; set; }
    public double AverageParticipationRate { get; set; }
    public double ClientRetentionRate { get; set; }
}

/// <summary>
/// Security metrics
/// </summary>
public class SecurityMetrics
{
    public double SecureAggregationSuccess { get; set; }
    public string EncryptionStrength { get; set; } = string.Empty;
    public double ThreatDetectionRate { get; set; }
    public int SecurityIncidents { get; set; }
}

/// <summary>
/// Enums for federated learning
/// </summary>
public enum FederatedSessionStatus
{
    Initialized,
    Active,
    LocalTrainingCompleted,
    AggregationCompleted,
    Completed,
    Failed,
    Cancelled
}

public enum PrivacyMechanism
{
    DifferentialPrivacy,
    SecureMultipartyComputation,
    HomomorphicEncryption,
    SecureAggregation
}
