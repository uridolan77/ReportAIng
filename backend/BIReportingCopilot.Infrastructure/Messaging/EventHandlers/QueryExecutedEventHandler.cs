using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models.ML;
using BIReportingCopilot.Infrastructure.Monitoring;
using BIReportingCopilot.Infrastructure.Security;
using BIReportingCopilot.Infrastructure.AI;
using System.Diagnostics;

namespace BIReportingCopilot.Infrastructure.Messaging.EventHandlers;

/// <summary>
/// Handles QueryExecutedEvent to update metrics, learning models, and cache
/// </summary>
public class QueryExecutedEventHandler : IEventHandler<QueryExecutedEvent>
{
    private readonly ILogger<QueryExecutedEventHandler> _logger;
    private readonly MonitoringManagementService _monitoringService;
    private readonly ISemanticCacheService _semanticCache;
    private readonly SecurityManagementService _securityService;
    private readonly IMetricsCollector _metricsCollector;
    private readonly IAnomalyDetector _anomalyDetector;

    public QueryExecutedEventHandler(
        ILogger<QueryExecutedEventHandler> logger,
        MonitoringManagementService monitoringService,
        ISemanticCacheService semanticCache,
        SecurityManagementService securityService,
        IMetricsCollector metricsCollector,
        IAnomalyDetector anomalyDetector)
    {
        _logger = logger;
        _monitoringService = monitoringService;
        _semanticCache = semanticCache;
        _securityService = securityService;
        _metricsCollector = metricsCollector;
        _anomalyDetector = anomalyDetector;
    }

    public async Task HandleAsync(QueryExecutedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing QueryExecutedEvent for user {UserId}, Success: {Success}",
                @event.UserId, @event.IsSuccessful);

            // Update metrics
            await UpdateMetricsAsync(@event);

            // Analyze for anomalies if successful
            if (@event.IsSuccessful)
            {
                await AnalyzeForAnomaliesAsync(@event);
            }

            // Update semantic cache statistics
            await UpdateCacheStatisticsAsync(@event);

            _logger.LogDebug("Successfully processed QueryExecutedEvent {EventId}", @event.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing QueryExecutedEvent {EventId}: {Error}",
                @event.EventId, ex.Message);
        }
    }

    private async Task UpdateMetricsAsync(QueryExecutedEvent @event)
    {
        try
        {
            // Record query execution metrics
            _metricsCollector.RecordQueryExecution(
                "natural_language",
                @event.ExecutionTimeMs,
                @event.IsSuccessful,
                @event.RowCount);

            // Record confidence score if available
            if (@event.ConfidenceScore.HasValue)
            {
                _metricsCollector.RecordHistogram(
                    "query_confidence_score",
                    @event.ConfidenceScore.Value);
            }

            // Record user activity
            _metricsCollector.IncrementCounter("user_queries_total", new TagList
            {
                { "user_id", @event.UserId },
                { "success", @event.IsSuccessful.ToString().ToLower() }
            });

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error updating metrics for QueryExecutedEvent");
        }
    }

    private async Task AnalyzeForAnomaliesAsync(QueryExecutedEvent @event)
    {
        try
        {
            var anomalyResult = await _anomalyDetector.AnalyzeQueryAsync(
                @event.UserId,
                @event.NaturalLanguageQuery,
                @event.GeneratedSQL);

            if (anomalyResult.RiskLevel >= RiskLevel.Medium)
            {
                _logger.LogWarning("Anomaly detected for user {UserId}: Risk Level {RiskLevel}, Score: {Score:F3}",
                    @event.UserId, anomalyResult.RiskLevel, anomalyResult.AnomalyScore);

                // Could publish AnomalyDetectedEvent here for further processing
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error analyzing query for anomalies");
        }
    }

    private async Task UpdateCacheStatisticsAsync(QueryExecutedEvent @event)
    {
        try
        {
            // Update cache access patterns for future optimization
            var cacheStats = await _semanticCache.GetCacheStatisticsAsync();

            _metricsCollector.SetGaugeValue("semantic_cache_entries", cacheStats.TotalEntries);
            _metricsCollector.SetGaugeValue("semantic_cache_hit_rate", cacheStats.HitRate);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error updating cache statistics");
        }
    }
}

/// <summary>
/// Handles FeedbackReceivedEvent to update learning models
/// </summary>
public class FeedbackReceivedEventHandler : IEventHandler<FeedbackReceivedEvent>
{
    private readonly ILogger<FeedbackReceivedEventHandler> _logger;
    private readonly IMetricsCollector _metricsCollector;
    private readonly BIReportingCopilot.Infrastructure.AI.FeedbackLearningEngine _learningEngine;

    public FeedbackReceivedEventHandler(
        ILogger<FeedbackReceivedEventHandler> logger,
        IMetricsCollector metricsCollector,
        BIReportingCopilot.Infrastructure.AI.FeedbackLearningEngine learningEngine)
    {
        _logger = logger;
        _metricsCollector = metricsCollector;
        _learningEngine = learningEngine;
    }

    public async Task HandleAsync(FeedbackReceivedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing FeedbackReceivedEvent from user {UserId}, Rating: {Rating}",
                @event.UserId, @event.Rating);

            // Update feedback metrics
            _metricsCollector.IncrementCounter("feedback_received_total", new TagList
            {
                { "user_id", @event.UserId },
                { "rating", @event.Rating.ToString() },
                { "feedback_type", @event.FeedbackType }
            });

            _metricsCollector.RecordHistogram("feedback_rating", @event.Rating);

            // Trigger learning model update if this is significant feedback
            if (@event.Rating <= 2 || @event.Rating >= 4)
            {
                await TriggerLearningUpdateAsync(@event);
            }

            _logger.LogDebug("Successfully processed FeedbackReceivedEvent {EventId}", @event.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing FeedbackReceivedEvent {EventId}: {Error}",
                @event.EventId, ex.Message);
        }
    }

    private async Task TriggerLearningUpdateAsync(FeedbackReceivedEvent @event)
    {
        try
        {
            // In a real implementation, you might queue a background job
            // to retrain models or update learning patterns
            _logger.LogInformation("Triggering learning update for significant feedback: Rating {Rating}", @event.Rating);

            // Could publish ModelRetrainRequestEvent here
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error triggering learning update");
        }
    }
}

/// <summary>
/// Handles AnomalyDetectedEvent for security monitoring
/// </summary>
public class AnomalyDetectedEventHandler : IEventHandler<AnomalyDetectedEvent>
{
    private readonly ILogger<AnomalyDetectedEventHandler> _logger;
    private readonly IMetricsCollector _metricsCollector;

    public AnomalyDetectedEventHandler(
        ILogger<AnomalyDetectedEventHandler> logger,
        IMetricsCollector metricsCollector)
    {
        _logger = logger;
        _metricsCollector = metricsCollector;
    }

    public async Task HandleAsync(AnomalyDetectedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogWarning("Processing AnomalyDetectedEvent for user {UserId}: {AnomalyType} - Risk Level {RiskLevel}",
                @event.UserId, @event.AnomalyType, @event.RiskLevel);

            // Update security metrics
            _metricsCollector.IncrementCounter("anomalies_detected_total", new TagList
            {
                { "user_id", @event.UserId },
                { "anomaly_type", @event.AnomalyType },
                { "risk_level", @event.RiskLevel }
            });

            _metricsCollector.RecordHistogram("anomaly_score", @event.AnomalyScore);

            // Handle high-risk anomalies
            if (@event.RiskLevel == "High" || @event.RiskLevel == "Critical")
            {
                await HandleHighRiskAnomalyAsync(@event);
            }

            _logger.LogDebug("Successfully processed AnomalyDetectedEvent {EventId}", @event.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AnomalyDetectedEvent {EventId}: {Error}",
                @event.EventId, ex.Message);
        }
    }

    private async Task HandleHighRiskAnomalyAsync(AnomalyDetectedEvent @event)
    {
        try
        {
            // In a real implementation, you might:
            // 1. Send alerts to security team
            // 2. Temporarily restrict user access
            // 3. Log to security audit system
            // 4. Trigger additional monitoring

            _logger.LogCritical("High-risk anomaly detected for user {UserId}: {Anomalies}",
                @event.UserId, string.Join(", ", @event.DetectedAnomalies));

            // Could publish SecurityAlertEvent here
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling high-risk anomaly");
        }
    }
}

/// <summary>
/// Handles CacheInvalidatedEvent to update cache statistics
/// </summary>
public class CacheInvalidatedEventHandler : IEventHandler<CacheInvalidatedEvent>
{
    private readonly ILogger<CacheInvalidatedEventHandler> _logger;
    private readonly IMetricsCollector _metricsCollector;

    public CacheInvalidatedEventHandler(
        ILogger<CacheInvalidatedEventHandler> logger,
        IMetricsCollector metricsCollector)
    {
        _logger = logger;
        _metricsCollector = metricsCollector;
    }

    public async Task HandleAsync(CacheInvalidatedEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Processing CacheInvalidatedEvent: {CacheType} - {Reason}, Affected Keys: {KeyCount}",
                @event.CacheType, @event.InvalidationReason, @event.AffectedKeys.Count);

            // Update cache metrics
            _metricsCollector.IncrementCounter("cache_invalidations_total", new TagList
            {
                { "cache_type", @event.CacheType },
                { "reason", @event.InvalidationReason },
                { "table_name", @event.TableName ?? "unknown" }
            });

            _metricsCollector.RecordHistogram("cache_invalidation_size", @event.AffectedKeys.Count);

            _logger.LogDebug("Successfully processed CacheInvalidatedEvent {EventId}", @event.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing CacheInvalidatedEvent {EventId}: {Error}",
                @event.EventId, ex.Message);
        }

        await Task.CompletedTask;
    }
}

/// <summary>
/// Handles PerformanceMetricsEvent for system monitoring
/// </summary>
public class PerformanceMetricsEventHandler : IEventHandler<PerformanceMetricsEvent>
{
    private readonly ILogger<PerformanceMetricsEventHandler> _logger;
    private readonly IMetricsCollector _metricsCollector;

    public PerformanceMetricsEventHandler(
        ILogger<PerformanceMetricsEventHandler> logger,
        IMetricsCollector metricsCollector)
    {
        _logger = logger;
        _metricsCollector = metricsCollector;
    }

    public async Task HandleAsync(PerformanceMetricsEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            // Update system performance metrics
            _metricsCollector.SetGaugeValue("system_cpu_usage_percent", @event.CpuUsagePercent);
            _metricsCollector.SetGaugeValue("system_memory_usage_mb", @event.MemoryUsageMB);
            _metricsCollector.SetGaugeValue("system_active_connections", @event.ActiveConnections);
            _metricsCollector.SetGaugeValue("system_cache_hit_rate", @event.CacheHitRate);
            _metricsCollector.SetGaugeValue("system_total_queries", @event.TotalQueries);
            _metricsCollector.SetGaugeValue("system_avg_response_time_ms", @event.AverageResponseTimeMs);

            // Log warnings for concerning metrics
            if (@event.CpuUsagePercent > 80)
            {
                _logger.LogWarning("High CPU usage detected: {CpuUsage:F1}%", @event.CpuUsagePercent);
            }

            if (@event.MemoryUsageMB > 1024) // 1GB
            {
                _logger.LogWarning("High memory usage detected: {MemoryUsage:F1} MB", @event.MemoryUsageMB);
            }

            if (@event.CacheHitRate < 0.5) // Less than 50%
            {
                _logger.LogWarning("Low cache hit rate detected: {HitRate:P1}", @event.CacheHitRate);
            }

            _logger.LogDebug("Successfully processed PerformanceMetricsEvent {EventId}", @event.EventId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PerformanceMetricsEvent {EventId}: {Error}",
                @event.EventId, ex.Message);
        }

        await Task.CompletedTask;
    }
}
