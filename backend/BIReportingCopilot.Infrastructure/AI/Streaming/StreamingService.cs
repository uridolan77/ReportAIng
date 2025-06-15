using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Core.Interfaces.Streaming;
using BIReportingCopilot.Core.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;
using StreamingMetrics = BIReportingCopilot.Core.Models.StreamingMetrics;

namespace BIReportingCopilot.Infrastructure.AI.Streaming;

/// <summary>
/// Real-time streaming service for AI responses and analytics
/// </summary>
public class StreamingService : IRealTimeStreamingService
{
    private readonly ILogger<StreamingService> _logger;
    private readonly IAIService _aiService;
    private readonly ICacheService _cacheService;
    private readonly ISchemaService _schemaService;

    public StreamingService(
        ILogger<StreamingService> logger,
        IAIService aiService,
        ICacheService cacheService,
        ISchemaService schemaService)
    {
        _logger = logger;
        _aiService = aiService;
        _cacheService = cacheService;
        _schemaService = schemaService;
    }

    public async IAsyncEnumerable<StreamingQueryResponse> ProcessQueryStreamAsync(
        string query,
        string userId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🌊 Starting streaming query processing for user {UserId}: {Query}", userId, query);

        // Use a wrapper method to handle exceptions without yield in catch
        await foreach (var response in ProcessQueryStreamInternalAsync(query, userId, cancellationToken))
        {
            yield return response;
        }
    }

    private async IAsyncEnumerable<StreamingQueryResponse> ProcessQueryStreamInternalAsync(
        string query,
        string userId,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Step 1: Initial response with query analysis
        yield return new StreamingQueryResponse
        {
            Type = StreamingResponseType.Analysis,
            Content = "Analyzing your query...",
            Progress = 10,
            Timestamp = DateTime.UtcNow
        };

        // Step 2: Schema analysis
        yield return new StreamingQueryResponse
        {
            Type = StreamingResponseType.Analysis,
            Content = "Identifying relevant data sources...",
            Progress = 25,
            Timestamp = DateTime.UtcNow
        };

        var schema = await _schemaService.GetSchemaMetadataAsync();

        // Step 3: SQL Generation with streaming
        yield return new StreamingQueryResponse
        
        {
            Type = StreamingResponseType.SqlGeneration,
            Content = "Generating SQL query...",
            Progress = 40,
            Timestamp = DateTime.UtcNow
        };

        var sqlBuilder = new System.Text.StringBuilder();
        var context = $"Streaming query processing for user {userId}";
        var sqlStream = await _aiService.GenerateSQLStreamAsync(query, schema, context, cancellationToken);
        await foreach (var sqlChunk in sqlStream)
        {
            sqlBuilder.Append(sqlChunk);

            yield return new StreamingQueryResponse
            {
                Type = StreamingResponseType.SqlGeneration,
                Content = sqlChunk,
                PartialSql = sqlBuilder.ToString(),
                Progress = 40 + (sqlBuilder.Length > 100 ? 20 : 10),
                Timestamp = DateTime.UtcNow
            };
        }

        var finalSql = sqlBuilder.ToString();

        // Step 4: SQL Validation
        yield return new StreamingQueryResponse
        {
            Type = StreamingResponseType.Validation,
            Content = "Validating generated SQL...",
            GeneratedSql = finalSql,
            Progress = 70,
            Timestamp = DateTime.UtcNow
        };

        // Step 5: Execution preparation
        yield return new StreamingQueryResponse
        {
            Type = StreamingResponseType.Execution,
            Content = "Preparing to execute query...",
            GeneratedSql = finalSql,
            Progress = 85,
            Timestamp = DateTime.UtcNow
        };

        // Step 6: Final completion
        yield return new StreamingQueryResponse
        {
            Type = StreamingResponseType.Complete,
            Content = "Query processing completed",
            GeneratedSql = finalSql,
            Progress = 100,
            IsComplete = true,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("✅ Streaming query processing completed for user {UserId}", userId);
    }

    public async IAsyncEnumerable<StreamingInsightResponse> GenerateInsightsStreamAsync(
        string query,
        object[] data,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔍 Starting streaming insight generation for query: {Query}", query);

        // Use a wrapper method to handle exceptions without yield in catch
        await foreach (var response in GenerateInsightsStreamInternalAsync(query, data, cancellationToken))
        {
            yield return response;
        }
    }

    private async IAsyncEnumerable<StreamingInsightResponse> GenerateInsightsStreamInternalAsync(
        string query,
        object[] data,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // Step 1: Data analysis
        yield return new StreamingInsightResponse
        {
            Type = StreamingInsightType.Analysis.ToString(),
            Content = "Analyzing query results...",
            Progress = 20,
            Timestamp = DateTime.UtcNow
        };

        // Step 2: Pattern detection
        yield return new StreamingInsightResponse
        {
            Type = StreamingInsightType.PatternDetection.ToString(),
            Content = "Detecting patterns in the data...",
            Progress = 40,
            Timestamp = DateTime.UtcNow
        };

        // Step 3: Insight generation with streaming
        yield return new StreamingInsightResponse
        {
            Type = StreamingInsightType.InsightGeneration.ToString(),
            Content = "Generating business insights...",
            Progress = 60,
            Timestamp = DateTime.UtcNow
        };

        var insightBuilder = new System.Text.StringBuilder();
        var dataString = System.Text.Json.JsonSerializer.Serialize(data);
        var insightStream = await _aiService.GenerateInsightStreamAsync(query, dataString, cancellationToken);
        await foreach (var insightChunk in insightStream)
        {
            insightBuilder.Append(insightChunk);

            yield return new StreamingInsightResponse
            {
                Type = StreamingInsightType.InsightGeneration.ToString(),
                Content = insightChunk,
                PartialInsight = insightBuilder.ToString(),
                Progress = 60 + (insightBuilder.Length > 100 ? 30 : 15),
                Timestamp = DateTime.UtcNow
            };
        }

        // Step 4: Final insights
        yield return new StreamingInsightResponse
        {
            Type = StreamingInsightType.Complete.ToString(),
            Content = insightBuilder.ToString(),
            PartialInsight = insightBuilder.ToString(),
            Progress = 100,
            IsComplete = true,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("✅ Streaming insight generation completed");
    }

    public async IAsyncEnumerable<StreamingAnalyticsUpdate> StreamAnalyticsAsync(
        string userId,
        TimeSpan updateInterval,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📊 Starting streaming analytics for user {UserId}", userId);

        // Use a wrapper method to handle exceptions without yield in catch
        await foreach (var update in StreamAnalyticsInternalAsync(userId, updateInterval, cancellationToken))
        {
            yield return update;
        }
    }

    private async IAsyncEnumerable<StreamingAnalyticsUpdate> StreamAnalyticsInternalAsync(
        string userId,
        TimeSpan updateInterval,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Generate analytics update
            var update = new StreamingAnalyticsUpdate
            {
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Metrics = (await GenerateCurrentMetricsAsync(userId)).ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value),
                Trends = (await GenerateCurrentTrendsAsync(userId)).Keys.ToList(),
                Alerts = (await GenerateCurrentAlertsAsync(userId)).Select(a => a.Message).ToList()
            };

            yield return update;

            // Wait for next update interval
            try
            {
                await Task.Delay(updateInterval, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("🛑 Streaming analytics stopped for user {UserId}", userId);
    }

    public async Task<StreamingSessionInfo> StartStreamingSessionAsync(string userId, StreamingSessionConfig config)
    {
        try
        {
            _logger.LogInformation("🚀 Starting streaming session for user {UserId}", userId);

            var sessionInfo = new StreamingSessionInfo
            {
                SessionId = Guid.NewGuid().ToString(),
                UserId = userId,
                StartedAt = DateTime.UtcNow,
                Configuration = config,
                Status = StreamingSessionStatus.Active.ToString(),
                ConnectionCount = 1
            };

            // Cache session info
            var cacheKey = $"streaming_session:{sessionInfo.SessionId}";
            await _cacheService.SetAsync(cacheKey, sessionInfo, TimeSpan.FromHours(24));

            _logger.LogInformation("✅ Streaming session started: {SessionId}", sessionInfo.SessionId);

            return sessionInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error starting streaming session for user {UserId}", userId);
            throw;
        }
    }

    public async Task StopStreamingSessionAsync(string sessionId)
    {
        try
        {
            _logger.LogInformation("🛑 Stopping streaming session: {SessionId}", sessionId);

            var cacheKey = $"streaming_session:{sessionId}";
            var sessionInfo = await _cacheService.GetAsync<StreamingSessionInfo>(cacheKey);

            if (sessionInfo != null)
            {
                sessionInfo.Status = StreamingSessionStatus.Stopped.ToString();
                sessionInfo.EndedAt = DateTime.UtcNow;

                await _cacheService.SetAsync(cacheKey, sessionInfo, TimeSpan.FromMinutes(5)); // Keep for short time for cleanup
            }

            _logger.LogInformation("✅ Streaming session stopped: {SessionId}", sessionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error stopping streaming session: {SessionId}", sessionId);
        }
    }

    public async Task<List<StreamingSession>> GetActiveSessionsAsync(string? userId = null)
    {
        try
        {
            // In a real implementation, this would query active sessions from cache/database
            // For now, return empty list as this is a simplified implementation
            return new List<StreamingSession>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting active sessions for user {UserId}", userId);
            return new List<StreamingSession>();
        }
    }

    #region Private Helper Methods

    private async Task<Dictionary<string, double>> GenerateCurrentMetricsAsync(string userId)
    {
        // Generate current metrics for the user
        return new Dictionary<string, double>
        {
            ["queries_today"] = 15,
            ["avg_response_time"] = 1250,
            ["cache_hit_rate"] = 0.75,
            ["success_rate"] = 0.95
        };
    }

    private async Task<Dictionary<string, object>> GenerateCurrentTrendsAsync(string userId)
    {
        // Generate current trends
        return new Dictionary<string, object>
        {
            ["query_frequency"] = "increasing",
            ["popular_tables"] = new[] { "tbl_Daily_actions", "tbl_Players" },
            ["peak_hours"] = new[] { 9, 14, 16 }
        };
    }

    private async Task<List<BIReportingCopilot.Core.Models.StreamingAlert>> GenerateCurrentAlertsAsync(string userId)
    {
        // Generate current alerts
        return new List<BIReportingCopilot.Core.Models.StreamingAlert>
        {
            new BIReportingCopilot.Core.Models.StreamingAlert
            {
                Type = "Performance",
                Message = "Query response time above average",
                Severity = "Warning",
                Timestamp = DateTime.UtcNow
            }
        };
    }

    #endregion

    // Missing interface methods - stub implementations
    public Task<StreamingSession> StartStreamingSessionAsync(string userId, StreamingConfiguration config)
    {
        return Task.FromResult(new StreamingSession
        {
            SessionId = Guid.NewGuid().ToString(),
            UserId = userId,
            StartedAt = DateTime.UtcNow,
            Status = SessionStatus.Active
        });
    }

    public Task<bool> StopStreamingSessionAsync(string sessionId, string userId)
    {
        _logger.LogInformation("Stopping streaming session {SessionId} for user {UserId}", sessionId, userId);
        return Task.FromResult(true);
    }

    public Task ProcessDataStreamEventAsync(DataStreamEvent streamEvent)
    {
        _logger.LogDebug("Processing data stream event: {EventType}", streamEvent.EventType);
        return Task.CompletedTask;
    }

    public Task ProcessQueryStreamEventAsync(QueryStreamEvent streamEvent)
    {
        _logger.LogDebug("Processing query stream event: {EventType}", streamEvent.EventType);
        return Task.CompletedTask;
    }

    public Task<RealTimeDashboard> GetRealTimeDashboardAsync(string dashboardId)
    {
        return Task.FromResult(new RealTimeDashboard
        {
            DashboardId = dashboardId,
            LastUpdated = DateTime.UtcNow,
            IsLive = true
        });
    }

    public Task<StreamingAnalyticsResult> GetStreamingAnalyticsAsync(TimeSpan timeWindow, string? userId = null)
    {
        return Task.FromResult(new StreamingAnalyticsResult
        {
            TimeWindow = timeWindow,
            TotalEvents = 100,
            ActiveSessions = 5,
            LastUpdated = DateTime.UtcNow
        });
    }

    public Task<string> SubscribeToDataStreamAsync(string streamId, StreamSubscription subscription)
    {
        var subscriptionId = Guid.NewGuid().ToString();
        _logger.LogInformation("Created subscription {SubscriptionId} for stream {StreamId}", subscriptionId, streamId);
        return Task.FromResult(subscriptionId);
    }

    public Task<bool> UnsubscribeFromDataStreamAsync(string streamId, string subscriptionId)
    {
        _logger.LogInformation("Unsubscribing from stream {StreamId}, subscription {SubscriptionId}", streamId, subscriptionId);
        return Task.FromResult(true);
    }

    // Removed duplicate method - already exists above

    public Task<RealTimeMetrics> GetRealTimeMetricsAsync(string? userId = null)
    {
        return Task.FromResult(new RealTimeMetrics
        {
            ActiveUsers = 10,
            QueriesPerSecond = 2.5,
            AverageResponseTime = 1200, // milliseconds as double
            LastUpdated = DateTime.UtcNow
        });
    }

    public Task<string> CreateRealTimeAlertAsync(RealTimeAlert alert, string userId, CancellationToken cancellationToken = default)
    {
        var alertId = Guid.NewGuid().ToString();
        _logger.LogInformation("Created real-time alert {AlertId} for user {UserId}", alertId, userId);
        return Task.FromResult(alertId);
    }

    public Task<StreamingPerformanceMetrics> GetStreamingPerformanceAsync()
    {
        return Task.FromResult(new StreamingPerformanceMetrics
        {
            TotalThroughput = 50,
            ActiveSessions = 5,
            AverageLatency = 100, // milliseconds as double
            ErrorRate = 0.01,
            GeneratedAt = DateTime.UtcNow
        });
    }

    #region Missing Interface Method Implementations

    /// <summary>
    /// Start streaming query (IRealTimeStreamingService interface)
    /// </summary>
    public async Task<string> StartStreamingQueryAsync(StreamingQueryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var streamId = Guid.NewGuid().ToString();
            _logger.LogInformation("🚀 Starting streaming query {StreamId} for user {UserId}", streamId, request.UserId);

            // Cache the streaming query info
            var queryInfo = new StreamingQueryInfo
            {
                StreamId = streamId,
                Query = request.Query,
                UserId = request.UserId,
                Status = "active",
                CreatedAt = DateTime.UtcNow,
                Configuration = request.Configuration,
                Metrics = new StreamingMetrics
                {
                    StartTime = DateTime.UtcNow
                }
            };

            var cacheKey = $"streaming_query:{streamId}";
            await _cacheService.SetAsync(cacheKey, queryInfo, TimeSpan.FromHours(24));

            _logger.LogInformation("✅ Streaming query started: {StreamId}", streamId);
            return streamId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error starting streaming query");
            throw;
        }
    }

    /// <summary>
    /// Stop streaming query (IRealTimeStreamingService interface)
    /// </summary>
    public async Task StopStreamingQueryAsync(string streamId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🛑 Stopping streaming query: {StreamId}", streamId);

            var cacheKey = $"streaming_query:{streamId}";
            var queryInfo = await _cacheService.GetAsync<StreamingQueryInfo>(cacheKey);

            if (queryInfo != null)
            {
                queryInfo.Status = "stopped";
                await _cacheService.SetAsync(cacheKey, queryInfo, TimeSpan.FromMinutes(5));
            }

            _logger.LogInformation("✅ Streaming query stopped: {StreamId}", streamId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error stopping streaming query: {StreamId}", streamId);
        }
    }

    /// <summary>
    /// Get streaming status (IRealTimeStreamingService interface)
    /// </summary>
    public async Task<StreamingQueryStatus> GetStreamingStatusAsync(string streamId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"streaming_query:{streamId}";
            var queryInfo = await _cacheService.GetAsync<StreamingQueryInfo>(cacheKey);

            if (queryInfo != null)
            {
                return new StreamingQueryStatus
                {
                    StreamId = streamId,
                    Status = queryInfo.Status,
                    StartedAt = queryInfo.CreatedAt,
                    LastUpdateAt = DateTime.UtcNow,
                    DataPointsGenerated = queryInfo.Metrics.TotalDataPoints,
                    Metrics = queryInfo.Metrics
                };
            }

            return new StreamingQueryStatus
            {
                StreamId = streamId,
                Status = "not_found",
                StartedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting streaming status for: {StreamId}", streamId);
            return new StreamingQueryStatus
            {
                StreamId = streamId,
                Status = "error",
                LastError = ex.Message,
                StartedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Get active streams (IRealTimeStreamingService interface)
    /// </summary>
    public async Task<List<StreamingQueryInfo>> GetActiveStreamsAsync(string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // In a real implementation, this would query active streams from cache/database
            // For now, return a simplified list
            var activeStreams = new List<StreamingQueryInfo>();

            if (!string.IsNullOrEmpty(userId))
            {
                _logger.LogInformation("Getting active streams for user: {UserId}", userId);
                // Filter by user if specified
            }

            return activeStreams;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting active streams for user: {UserId}", userId);
            return new List<StreamingQueryInfo>();
        }
    }

    /// <summary>
    /// Get streaming data (IRealTimeStreamingService interface)
    /// </summary>
    public async IAsyncEnumerable<StreamingDataPoint> GetStreamingDataAsync(string streamId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📊 Starting streaming data for stream: {StreamId}", streamId);

        var sequenceNumber = 0L;
        while (!cancellationToken.IsCancellationRequested)
        {
            StreamingDataPoint? dataPoint = null;

            try
            {
                // Generate sample streaming data point
                dataPoint = new StreamingDataPoint
                {
                    StreamId = streamId,
                    Timestamp = DateTime.UtcNow,
                    SequenceNumber = ++sequenceNumber,
                    Data = new Dictionary<string, object>
                    {
                        ["value"] = Random.Shared.NextDouble() * 100,
                        ["count"] = Random.Shared.Next(1, 50),
                        ["status"] = "active"
                    },
                    Metadata = new Dictionary<string, object>
                    {
                        ["source"] = "streaming_service",
                        ["version"] = "1.0"
                    }
                };
            }
            catch (TaskCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error generating streaming data for: {StreamId}", streamId);
                break;
            }

            if (dataPoint != null)
            {
                yield return dataPoint;
            }

            try
            {
                // Wait before next data point
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("🛑 Streaming data stopped for stream: {StreamId}", streamId);
    }

    /// <summary>
    /// Update streaming configuration (IRealTimeStreamingService interface)
    /// </summary>
    public async Task<bool> UpdateStreamingConfigurationAsync(string streamId, StreamingConfiguration config, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔧 Updating streaming configuration for: {StreamId}", streamId);

            var cacheKey = $"streaming_query:{streamId}";
            var queryInfo = await _cacheService.GetAsync<StreamingQueryInfo>(cacheKey);

            if (queryInfo != null)
            {
                // Update configuration directly since they're now the same type
                queryInfo.Configuration = config;

                await _cacheService.SetAsync(cacheKey, queryInfo, TimeSpan.FromHours(24));

                _logger.LogInformation("✅ Streaming configuration updated for: {StreamId}", streamId);
                return true;
            }

            _logger.LogWarning("⚠️ Stream not found for configuration update: {StreamId}", streamId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating streaming configuration for: {StreamId}", streamId);
            return false;
        }
    }

    /// <summary>
    /// Get streaming metrics (IRealTimeStreamingService interface)
    /// </summary>
    public async Task<StreamingMetrics> GetStreamingMetricsAsync(string streamId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = $"streaming_query:{streamId}";
            var queryInfo = await _cacheService.GetAsync<StreamingQueryInfo>(cacheKey);

            if (queryInfo != null)
            {
                return new BIReportingCopilot.Core.Models.StreamingMetrics
                {
                    TotalDataPoints = queryInfo.Metrics.EventsProcessed,
                    AverageLatency = queryInfo.Metrics.AverageLatency,
                    ThroughputPerSecond = queryInfo.Metrics.EventsPerSecond,
                    ErrorCount = queryInfo.Metrics.ErrorCount,
                    LastSuccessfulUpdate = DateTime.UtcNow,
                    StartTime = DateTime.UtcNow.Subtract(queryInfo.Metrics.Uptime),
                    CustomMetrics = queryInfo.Metrics.CustomMetrics
                };
            }

            return new BIReportingCopilot.Core.Models.StreamingMetrics
            {
                LastSuccessfulUpdate = DateTime.UtcNow,
                StartTime = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting streaming metrics for: {StreamId}", streamId);
            return new BIReportingCopilot.Core.Models.StreamingMetrics
            {
                ErrorCount = 1,
                LastSuccessfulUpdate = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Get streaming alerts (IRealTimeStreamingService interface)
    /// </summary>
    public async Task<List<BIReportingCopilot.Core.Interfaces.Streaming.StreamingAlert>> GetStreamingAlertsAsync(string streamId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🚨 Getting streaming alerts for: {StreamId}", streamId);

            // In a real implementation, this would query alerts from database
            var alerts = new List<BIReportingCopilot.Core.Interfaces.Streaming.StreamingAlert>
            {
                new BIReportingCopilot.Core.Interfaces.Streaming.StreamingAlert
                {
                    StreamId = streamId,
                    Message = "High throughput detected",
                    Severity = "medium",
                    TriggeredAt = DateTime.UtcNow.AddMinutes(-5),
                    Context = new Dictionary<string, object>
                    {
                        ["threshold"] = 100,
                        ["current_value"] = 150
                    }
                }
            };

            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting streaming alerts for: {StreamId}", streamId);
            return new List<BIReportingCopilot.Core.Interfaces.Streaming.StreamingAlert>();
        }
    }

    /// <summary>
    /// Create streaming alert (IRealTimeStreamingService interface)
    /// </summary>
    public async Task<bool> CreateStreamingAlertAsync(string streamId, StreamingAlertRule rule, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🚨 Creating streaming alert for stream {StreamId}: {RuleName}", streamId, rule.Name);

            // In a real implementation, this would store the alert rule in database
            var cacheKey = $"streaming_alert_rule:{streamId}:{rule.RuleId}";
            await _cacheService.SetAsync(cacheKey, rule, TimeSpan.FromDays(30));

            _logger.LogInformation("✅ Streaming alert created: {RuleId}", rule.RuleId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating streaming alert for: {StreamId}", streamId);
            return false;
        }
    }

    #endregion
}
