using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

// =============================================================================
// PHASE 3 ENHANCED AI MODELS - CONSOLIDATED
// =============================================================================
// This file consolidates all Phase 3 model classes that are currently excluded
// from compilation. These models support advanced features like:
// - Real-time streaming analytics
// - Federated learning
// - Quantum-resistant security
// - Advanced semantic caching
// - Provider routing
// - Anomaly detection
// =============================================================================

// =============================================================================
// STREAMING MODELS
// =============================================================================

/// <summary>
/// Phase 3 real-time streaming configuration (renamed to avoid conflict with Core.Models.StreamingConfiguration)
/// </summary>
public class Phase3StreamingConfiguration
{
    public int BufferSize { get; set; } = 1000;
    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(5);
    public int MaxConcurrentStreams { get; set; } = 10;
    public bool EnableCompression { get; set; } = true;
    public StreamingProtocol Protocol { get; set; } = StreamingProtocol.WebSocket;
    public Dictionary<string, object> AdvancedSettings { get; set; } = new();
}

/// <summary>
/// Streaming data chunk
/// </summary>
public class StreamingDataChunk
{
    public string Id { get; set; } = string.Empty;
    public int SequenceNumber { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public DateTime Timestamp { get; set; }
    public bool IsLastChunk { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Phase 3 streaming analytics result (renamed to avoid conflict with Core.Models.StreamingAnalyticsResult)
/// </summary>
public class Phase3StreamingAnalyticsResult
{
    public string StreamId { get; set; } = string.Empty;
    public List<StreamingDataChunk> Chunks { get; set; } = new();
    public Phase3StreamingMetrics Metrics { get; set; } = new();
    public List<StreamingInsight> Insights { get; set; } = new();
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
}

/// <summary>
/// Phase 3 streaming metrics (renamed to avoid conflict with Core.Models.StreamingMetrics)
/// </summary>
public class Phase3StreamingMetrics
{
    public long TotalBytes { get; set; }
    public int ChunkCount { get; set; }
    public double AverageLatency { get; set; }
    public double Throughput { get; set; }
    public int ErrorCount { get; set; }
}

/// <summary>
/// Streaming insight
/// </summary>
public class StreamingInsight
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

// =============================================================================
// FEDERATED LEARNING MODELS
// =============================================================================

/// <summary>
/// Federated learning configuration
/// </summary>
public class FederatedLearningConfiguration
{
    public int MinimumParticipants { get; set; } = 3;
    public int MaximumRounds { get; set; } = 100;
    public double ConvergenceThreshold { get; set; } = 0.001;
    public TimeSpan RoundTimeout { get; set; } = TimeSpan.FromMinutes(30);
    public FederatedAggregationStrategy AggregationStrategy { get; set; } = FederatedAggregationStrategy.FederatedAveraging;
    public PrivacyConfiguration Privacy { get; set; } = new();
}

/// <summary>
/// Federated model update
/// </summary>
public class FederatedModelUpdate
{
    public string ParticipantId { get; set; } = string.Empty;
    public int Round { get; set; }
    public byte[] ModelWeights { get; set; } = Array.Empty<byte>();
    public FederatedMetrics Metrics { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public string Signature { get; set; } = string.Empty;
}

/// <summary>
/// Federated learning metrics
/// </summary>
public class FederatedMetrics
{
    public double Accuracy { get; set; }
    public double Loss { get; set; }
    public int TrainingSamples { get; set; }
    public TimeSpan TrainingTime { get; set; }
    public Dictionary<string, double> CustomMetrics { get; set; } = new();
}

/// <summary>
/// Privacy configuration for federated learning
/// </summary>
public class PrivacyConfiguration
{
    public bool EnableDifferentialPrivacy { get; set; } = true;
    public double EpsilonValue { get; set; } = 1.0;
    public bool EnableSecureAggregation { get; set; } = true;
    public EncryptionMethod EncryptionMethod { get; set; } = EncryptionMethod.AES256;
}

// =============================================================================
// QUANTUM SECURITY MODELS
// =============================================================================

/// <summary>
/// Quantum-resistant security configuration
/// </summary>
public class QuantumSecurityConfiguration
{
    public QuantumAlgorithm PrimaryAlgorithm { get; set; } = QuantumAlgorithm.Kyber;
    public QuantumAlgorithm BackupAlgorithm { get; set; } = QuantumAlgorithm.Dilithium;
    public int KeySize { get; set; } = 3072;
    public TimeSpan KeyRotationInterval { get; set; } = TimeSpan.FromDays(30);
    public bool EnableQuantumRandomness { get; set; } = true;
}

/// <summary>
/// Quantum key pair
/// </summary>
public class QuantumKeyPair
{
    public string Id { get; set; } = string.Empty;
    public byte[] PublicKey { get; set; } = Array.Empty<byte>();
    public byte[] PrivateKey { get; set; } = Array.Empty<byte>();
    public QuantumAlgorithm Algorithm { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Quantum signature
/// </summary>
public class QuantumSignature
{
    public string KeyId { get; set; } = string.Empty;
    public byte[] Signature { get; set; } = Array.Empty<byte>();
    public QuantumAlgorithm Algorithm { get; set; }
    public DateTime Timestamp { get; set; }
}

// =============================================================================
// SEMANTIC CACHE MODELS
// =============================================================================

/// <summary>
/// Semantic cache configuration
/// </summary>
public class SemanticCacheConfiguration
{
    public int MaxCacheSize { get; set; } = 10000;
    public TimeSpan DefaultTTL { get; set; } = TimeSpan.FromHours(24);
    public double SimilarityThreshold { get; set; } = 0.85;
    public SemanticCacheStrategy Strategy { get; set; } = SemanticCacheStrategy.EmbeddingBased;
    public bool EnableCompression { get; set; } = true;
}

/// <summary>
/// Semantic cache entry
/// </summary>
public class SemanticCacheEntry
{
    public string Id { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public float[] QueryEmbedding { get; set; } = Array.Empty<float>();
    public object CachedResult { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int AccessCount { get; set; }
    public DateTime LastAccessed { get; set; }
}

/// <summary>
/// Semantic similarity result
/// </summary>
public class SemanticSimilarityResult
{
    public string CacheEntryId { get; set; } = string.Empty;
    public double SimilarityScore { get; set; }
    public object CachedResult { get; set; } = new();
    public bool IsExactMatch { get; set; }
}

// =============================================================================
// PROVIDER ROUTING MODELS
// =============================================================================

/// <summary>
/// AI provider routing configuration
/// </summary>
public class ProviderRoutingConfiguration
{
    public List<ProviderEndpoint> Providers { get; set; } = new();
    public RoutingStrategy Strategy { get; set; } = RoutingStrategy.RoundRobin;
    public FailoverConfiguration Failover { get; set; } = new();
    public LoadBalancingConfiguration LoadBalancing { get; set; } = new();
}

/// <summary>
/// AI provider endpoint
/// </summary>
public class ProviderEndpoint
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public ProviderType Type { get; set; }
    public int Priority { get; set; }
    public int Weight { get; set; } = 1;
    public bool IsEnabled { get; set; } = true;
    public ProviderCapabilities Capabilities { get; set; } = new();
    public Dictionary<string, string> Configuration { get; set; } = new();
}

/// <summary>
/// Provider capabilities
/// </summary>
public class ProviderCapabilities
{
    public bool SupportsStreaming { get; set; }
    public bool SupportsEmbeddings { get; set; }
    public bool SupportsImageGeneration { get; set; }
    public int MaxTokens { get; set; }
    public List<string> SupportedModels { get; set; } = new();
}

/// <summary>
/// Failover configuration
/// </summary>
public class FailoverConfiguration
{
    public bool EnableFailover { get; set; } = true;
    public int MaxRetries { get; set; } = 3;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    public List<string> FailoverOrder { get; set; } = new();
}

/// <summary>
/// Load balancing configuration
/// </summary>
public class LoadBalancingConfiguration
{
    public LoadBalancingStrategy Strategy { get; set; } = LoadBalancingStrategy.WeightedRoundRobin;
    public bool EnableHealthChecks { get; set; } = true;
    public TimeSpan HealthCheckInterval { get; set; } = TimeSpan.FromMinutes(1);
}

// =============================================================================
// ANOMALY DETECTION MODELS
// =============================================================================

/// <summary>
/// Anomaly detection configuration
/// </summary>
public class AnomalyDetectionConfiguration
{
    public double SensitivityThreshold { get; set; } = 0.7;
    public int WindowSize { get; set; } = 100;
    public AnomalyDetectionAlgorithm Algorithm { get; set; } = AnomalyDetectionAlgorithm.IsolationForest;
    public bool EnableRealTimeDetection { get; set; } = true;
    public Dictionary<string, object> AlgorithmParameters { get; set; } = new();
}

/// <summary>
/// Detected anomaly
/// </summary>
public class DetectedAnomaly
{
    public string Id { get; set; } = string.Empty;
    public AnomalyType Type { get; set; }
    public double Severity { get; set; }
    public double Confidence { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
    public List<string> AffectedEntities { get; set; } = new();
}

/// <summary>
/// Anomaly detection result
/// </summary>
public class AnomalyDetectionResult
{
    public List<DetectedAnomaly> Anomalies { get; set; } = new();
    public AnomalyDetectionMetrics Metrics { get; set; } = new();
    public DateTime AnalysisTimestamp { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
}

/// <summary>
/// Anomaly detection metrics
/// </summary>
public class AnomalyDetectionMetrics
{
    public int TotalSamples { get; set; }
    public int AnomaliesDetected { get; set; }
    public double AnomalyRate { get; set; }
    public double AverageConfidence { get; set; }
    public TimeSpan ProcessingTime { get; set; }
}

// =============================================================================
// ENUMERATIONS
// =============================================================================

/// <summary>
/// Streaming protocol types
/// </summary>
public enum StreamingProtocol
{
    WebSocket,
    ServerSentEvents,
    gRPC,
    TCP,
    UDP
}

/// <summary>
/// Federated aggregation strategies
/// </summary>
public enum FederatedAggregationStrategy
{
    FederatedAveraging,
    FederatedProx,
    FederatedSGD,
    SecureAggregation
}

/// <summary>
/// Encryption methods
/// </summary>
public enum EncryptionMethod
{
    AES256,
    ChaCha20,
    RSA4096,
    ECC521
}

/// <summary>
/// Quantum-resistant algorithms
/// </summary>
public enum QuantumAlgorithm
{
    Kyber,
    Dilithium,
    Falcon,
    SPHINCS,
    McEliece
}

/// <summary>
/// Semantic cache strategies
/// </summary>
public enum SemanticCacheStrategy
{
    EmbeddingBased,
    HashBased,
    HybridApproach,
    GraphBased
}

/// <summary>
/// Routing strategies
/// </summary>
public enum RoutingStrategy
{
    RoundRobin,
    WeightedRoundRobin,
    LeastConnections,
    PerformanceBased,
    CostOptimized
}

/// <summary>
/// Provider types
/// </summary>
public enum ProviderType
{
    OpenAI,
    AzureOpenAI,
    Anthropic,
    Google,
    AWS,
    Local
}

/// <summary>
/// Load balancing strategies
/// </summary>
public enum LoadBalancingStrategy
{
    RoundRobin,
    WeightedRoundRobin,
    LeastConnections,
    ResourceBased,
    LatencyBased
}

/// <summary>
/// Anomaly detection algorithms
/// </summary>
public enum AnomalyDetectionAlgorithm
{
    IsolationForest,
    OneClassSVM,
    LocalOutlierFactor,
    EllipticEnvelope,
    DBSCAN
}

/// <summary>
/// Anomaly types
/// </summary>
public enum AnomalyType
{
    Statistical,
    Behavioral,
    Temporal,
    Contextual,
    Collective
}
