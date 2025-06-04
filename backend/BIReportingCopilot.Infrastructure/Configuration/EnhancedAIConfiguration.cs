namespace BIReportingCopilot.Infrastructure.Configuration;

/// <summary>
/// Configuration for Enhanced AI services (Enhancements 6 & 8)
/// </summary>
public class EnhancedAIConfiguration
{
    /// <summary>
    /// Enable conversation context tracking
    /// </summary>
    public bool EnableConversationContext { get; set; } = true;

    /// <summary>
    /// Enable query decomposition for complex queries
    /// </summary>
    public bool EnableQueryDecomposition { get; set; } = true;

    /// <summary>
    /// Enable entity linking to schema elements
    /// </summary>
    public bool EnableEntityLinking { get; set; } = true;

    /// <summary>
    /// Enable multi-dimensional confidence scoring
    /// </summary>
    public bool EnableAdvancedConfidenceScoring { get; set; } = true;

    /// <summary>
    /// Maximum number of queries to keep in conversation context
    /// </summary>
    public int MaxContextQueries { get; set; } = 10;

    /// <summary>
    /// Time-to-live for conversation context in hours
    /// </summary>
    public int ContextTtlHours { get; set; } = 2;

    /// <summary>
    /// Minimum confidence threshold for query execution
    /// </summary>
    public double MinimumConfidenceThreshold { get; set; } = 0.6;

    /// <summary>
    /// Minimum similarity threshold for semantic caching
    /// </summary>
    public double MinimumSimilarityThreshold { get; set; } = 0.85;

    /// <summary>
    /// Enable AI-powered intent classification
    /// </summary>
    public bool EnableAIIntentClassification { get; set; } = true;

    /// <summary>
    /// Enable schema relationship analysis
    /// </summary>
    public bool EnableSchemaRelationshipAnalysis { get; set; } = true;

    /// <summary>
    /// Enable SQL optimization
    /// </summary>
    public bool EnableSQLOptimization { get; set; } = true;

    /// <summary>
    /// Maximum complexity score before triggering decomposition
    /// </summary>
    public int MaxComplexityBeforeDecomposition { get; set; } = 7;

    /// <summary>
    /// Confidence weights for multi-dimensional scoring
    /// </summary>
    public ConfidenceWeights ConfidenceWeights { get; set; } = new();

    /// <summary>
    /// Performance settings
    /// </summary>
    public PerformanceSettings Performance { get; set; } = new();

    /// <summary>
    /// Logging settings for enhanced AI
    /// </summary>
    public LoggingSettings Logging { get; set; } = new();
}

/// <summary>
/// Confidence scoring weights
/// </summary>
public class ConfidenceWeights
{
    public float ModelConfidence { get; set; } = 0.3f;
    public float SchemaAlignment { get; set; } = 0.25f;
    public float ExecutionValidity { get; set; } = 0.25f;
    public float HistoricalPerformance { get; set; } = 0.2f;
}

/// <summary>
/// Performance settings for enhanced AI
/// </summary>
public class PerformanceSettings
{
    /// <summary>
    /// Enable parallel processing for sub-queries
    /// </summary>
    public bool EnableParallelProcessing { get; set; } = true;

    /// <summary>
    /// Maximum number of parallel sub-queries
    /// </summary>
    public int MaxParallelSubQueries { get; set; } = 3;

    /// <summary>
    /// Timeout for individual sub-query processing in seconds
    /// </summary>
    public int SubQueryTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable caching of decomposition results
    /// </summary>
    public bool EnableDecompositionCaching { get; set; } = true;

    /// <summary>
    /// Cache TTL for decomposition results in minutes
    /// </summary>
    public int DecompositionCacheTtlMinutes { get; set; } = 60;
}

/// <summary>
/// Logging settings for enhanced AI
/// </summary>
public class LoggingSettings
{
    /// <summary>
    /// Enable detailed logging for semantic analysis
    /// </summary>
    public bool EnableSemanticAnalysisLogging { get; set; } = true;

    /// <summary>
    /// Enable detailed logging for SQL generation
    /// </summary>
    public bool EnableSQLGenerationLogging { get; set; } = true;

    /// <summary>
    /// Enable detailed logging for confidence scoring
    /// </summary>
    public bool EnableConfidenceLogging { get; set; } = true;

    /// <summary>
    /// Enable performance metrics logging
    /// </summary>
    public bool EnablePerformanceLogging { get; set; } = true;

    /// <summary>
    /// Log conversation context changes
    /// </summary>
    public bool LogConversationContext { get; set; } = false;

    /// <summary>
    /// Log query decomposition details
    /// </summary>
    public bool LogQueryDecomposition { get; set; } = true;
}

/// <summary>
/// Feature flags for gradual rollout
/// </summary>
public class FeatureFlags
{
    /// <summary>
    /// Enable enhanced query processor for all users
    /// </summary>
    public bool EnableEnhancedQueryProcessor { get; set; } = false;

    /// <summary>
    /// Enable enhanced query processor for specific users (comma-separated list)
    /// </summary>
    public string EnabledUsers { get; set; } = string.Empty;

    /// <summary>
    /// Percentage of users to enable enhanced features for (0-100)
    /// </summary>
    public int RolloutPercentage { get; set; } = 0;

    /// <summary>
    /// Enable A/B testing between old and new processors
    /// </summary>
    public bool EnableABTesting { get; set; } = false;

    /// <summary>
    /// Enable enhanced features for admin users only
    /// </summary>
    public bool AdminOnlyMode { get; set; } = true;

    /// <summary>
    /// Enable fallback to original processor on errors
    /// </summary>
    public bool EnableFallbackOnError { get; set; } = true;

    /// <summary>
    /// Enable enhanced features for specific domains
    /// </summary>
    public string EnabledDomains { get; set; } = "gaming,finance";

    /// <summary>
    /// Enable enhanced conversation context
    /// </summary>
    public bool EnableConversationContext { get; set; } = true;

    /// <summary>
    /// Enable query decomposition
    /// </summary>
    public bool EnableQueryDecomposition { get; set; } = true;

    /// <summary>
    /// Enable schema-aware SQL generation
    /// </summary>
    public bool EnableSchemaAwareSQL { get; set; } = true;

    /// <summary>
    /// Enable multi-dimensional confidence scoring
    /// </summary>
    public bool EnableAdvancedConfidence { get; set; } = true;
}

/// <summary>
/// Migration settings for transitioning from old to new system
/// </summary>
public class MigrationSettings
{
    /// <summary>
    /// Enable side-by-side comparison logging
    /// </summary>
    public bool EnableComparisonLogging { get; set; } = true;

    /// <summary>
    /// Enable performance comparison metrics
    /// </summary>
    public bool EnablePerformanceComparison { get; set; } = true;

    /// <summary>
    /// Enable accuracy comparison metrics
    /// </summary>
    public bool EnableAccuracyComparison { get; set; } = true;

    /// <summary>
    /// Percentage of queries to run through both systems for comparison
    /// </summary>
    public int ComparisonPercentage { get; set; } = 10;

    /// <summary>
    /// Enable automatic migration based on performance metrics
    /// </summary>
    public bool EnableAutoMigration { get; set; } = false;

    /// <summary>
    /// Minimum improvement threshold for auto-migration (percentage)
    /// </summary>
    public double AutoMigrationThreshold { get; set; } = 15.0;

    /// <summary>
    /// Enable rollback on performance degradation
    /// </summary>
    public bool EnableAutoRollback { get; set; } = true;

    /// <summary>
    /// Performance degradation threshold for rollback (percentage)
    /// </summary>
    public double RollbackThreshold { get; set; } = 10.0;
}
