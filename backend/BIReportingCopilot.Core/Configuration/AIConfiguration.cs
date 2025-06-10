namespace BIReportingCopilot.Core.Configuration;

/// <summary>
/// Consolidated AI configuration for all AI services
/// </summary>
public class AIConfiguration
{
    /// <summary>
    /// Core AI service configuration
    /// </summary>
    public CoreAIConfig Core { get; set; } = new();

    /// <summary>
    /// Semantic cache configuration
    /// </summary>
    public SemanticCacheConfig SemanticCache { get; set; } = new();

    /// <summary>
    /// Intelligence services configuration
    /// </summary>
    public IntelligenceConfig Intelligence { get; set; } = new();

    /// <summary>
    /// Streaming services configuration
    /// </summary>
    public StreamingConfig Streaming { get; set; } = new();

    /// <summary>
    /// Dashboard services configuration
    /// </summary>
    public DashboardConfig Dashboard { get; set; } = new();

    /// <summary>
    /// Learning and ML configuration
    /// </summary>
    public LearningConfig Learning { get; set; } = new();

    // Additional properties expected by Infrastructure services
    public string OpenAIApiKey { get; set; } = string.Empty;
    public bool EnableSemanticAnalysis { get; set; } = true;
    public bool EnableQueryClassification { get; set; } = true;
}

/// <summary>
/// Core AI service configuration
/// </summary>
public class CoreAIConfig
{
    public string DefaultProvider { get; set; } = "OpenAI";
    public int MaxRetries { get; set; } = 3;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public double DefaultTemperature { get; set; } = 0.1;
    public int DefaultMaxTokens { get; set; } = 2000;
    public bool EnableAdaptiveLearning { get; set; } = true;
    public bool EnableMetricsCollection { get; set; } = true;
}

/// <summary>
/// Semantic cache configuration
/// </summary>
public class SemanticCacheConfig
{
    public bool EnableVectorSearch { get; set; } = true;
    public double SimilarityThreshold { get; set; } = 0.85;
    public int MaxSimilarQueries { get; set; } = 5;
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromHours(2);
    public string EmbeddingModel { get; set; } = "text-embedding-ada-002";
    public bool EnableDatabasePersistence { get; set; } = true;
    public int MaxCacheSize { get; set; } = 10000;
}

/// <summary>
/// Intelligence services configuration
/// </summary>
public class IntelligenceConfig
{
    public NLUConfig NLU { get; set; } = new();
    public OptimizationConfig Optimization { get; set; } = new();
    public VectorSearchConfig VectorSearch { get; set; } = new();
}

/// <summary>
/// NLU configuration
/// </summary>
public class NLUConfig
{
    public bool EnableSemanticAnalysis { get; set; } = true;
    public bool EnableContextTracking { get; set; } = true;
    public bool EnableLearning { get; set; } = true;
    public int CacheExpirationMinutes { get; set; } = 30;
    public int MaxContextQueries { get; set; } = 10;
    public double ConfidenceThreshold { get; set; } = 0.7;
}

/// <summary>
/// Optimization configuration
/// </summary>
public class OptimizationConfig
{
    public bool EnableQueryOptimization { get; set; } = true;
    public bool EnableSchemaOptimization { get; set; } = true;
    public int MaxOptimizationAttempts { get; set; } = 3;
    public TimeSpan OptimizationTimeout { get; set; } = TimeSpan.FromSeconds(10);
}

/// <summary>
/// Vector search configuration
/// </summary>
public class VectorSearchConfig
{
    public int VectorDimensions { get; set; } = 1536;
    public string IndexType { get; set; } = "InMemory";
    public int MaxVectors { get; set; } = 100000;
    public double SimilarityThreshold { get; set; } = 0.8;
}

/// <summary>
/// Streaming configuration
/// </summary>
public class StreamingConfig
{
    public bool EnableRealTimeStreaming { get; set; } = true;
    public TimeSpan DefaultUpdateInterval { get; set; } = TimeSpan.FromSeconds(5);
    public int MaxConcurrentStreams { get; set; } = 10;
    public int BufferSize { get; set; } = 1000;
    public bool EnableCompression { get; set; } = true;
    public TimeSpan SessionTimeout { get; set; } = TimeSpan.FromHours(24);
}

/// <summary>
/// Dashboard configuration
/// </summary>
public class DashboardConfig
{
    public bool EnableMultiModalDashboards { get; set; } = true;
    public bool EnableAutoGeneration { get; set; } = true;
    public int MaxWidgetsPerDashboard { get; set; } = 20;
    public int MaxDashboardsPerUser { get; set; } = 100;
    public bool EnableSharing { get; set; } = true;
    public bool EnableExport { get; set; } = true;
    public List<string> SupportedExportFormats { get; set; } = new() { "PDF", "PNG", "JSON" };
}

/// <summary>
/// Learning and ML configuration
/// </summary>
public class LearningConfig
{
    public bool EnableFeedbackLearning { get; set; } = true;
    public bool EnableAnomalyDetection { get; set; } = true;
    public bool EnableFederatedLearning { get; set; } = false;
    public int MinFeedbackSamples { get; set; } = 10;
    public double LearningRate { get; set; } = 0.01;
    public TimeSpan ModelUpdateInterval { get; set; } = TimeSpan.FromHours(24);
    public int MaxTrainingIterations { get; set; } = 1000;
}

/// <summary>
/// Federated learning configuration
/// </summary>
public class FederatedLearningConfig
{
    public bool EnableDifferentialPrivacy { get; set; } = true;
    public bool EnableSecureAggregation { get; set; } = true;
    public double InitialPrivacyBudget { get; set; } = 1.0;
    public double NoiseMultiplier { get; set; } = 1.1;
    public string EncryptionAlgorithm { get; set; } = "AES256";
    public int MinParticipants { get; set; } = 3;
    public int MaxRounds { get; set; } = 100;
}

/// <summary>
/// Phase 3 configuration
/// </summary>
public class Phase3Config
{
    public bool EnablePhase3Features { get; set; } = true;
    public bool EnableAdvancedAnalytics { get; set; } = true;
    public bool EnablePredictiveModeling { get; set; } = true;
    public bool EnableRealTimeInsights { get; set; } = true;
    public StreamingProtocol StreamingProtocol { get; set; } = StreamingProtocol.WebSocket;
    public int BufferSize { get; set; } = 1000;
    public TimeSpan FlushInterval { get; set; } = TimeSpan.FromSeconds(5);
    public int MaxConcurrentStreams { get; set; } = 10;
    public bool EnableCompression { get; set; } = true;
}

/// <summary>
/// Streaming protocol options
/// </summary>
public enum StreamingProtocol
{
    WebSocket,
    SignalR,
    ServerSentEvents,
    HTTP2
}
