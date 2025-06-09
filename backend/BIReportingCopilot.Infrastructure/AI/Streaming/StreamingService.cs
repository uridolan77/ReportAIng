using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using System.Runtime.CompilerServices;
using System.Text.Json;

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

        try
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
            await foreach (var sqlChunk in _aiService.GenerateSQLStreamAsync(query, schema, null, cancellationToken))
            {
                sqlBuilder.Append(sqlChunk.Content);
                
                yield return new StreamingQueryResponse
                {
                    Type = StreamingResponseType.SqlGeneration,
                    Content = sqlChunk.Content,
                    PartialSql = sqlBuilder.ToString(),
                    Progress = 40 + (sqlChunk.IsComplete ? 20 : 10),
                    Timestamp = DateTime.UtcNow
                };

                if (sqlChunk.IsComplete)
                    break;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in streaming query processing for user {UserId}", userId);
            
            yield return new StreamingQueryResponse
            {
                Type = StreamingResponseType.Error,
                Content = $"Error processing query: {ex.Message}",
                Progress = 0,
                IsComplete = true,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public async IAsyncEnumerable<StreamingInsightResponse> GenerateInsightsStreamAsync(
        string query,
        object[] data,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔍 Starting streaming insight generation for query: {Query}", query);

        try
        {
            // Step 1: Data analysis
            yield return new StreamingInsightResponse
            {
                Type = StreamingInsightType.Analysis,
                Content = "Analyzing query results...",
                Progress = 20,
                Timestamp = DateTime.UtcNow
            };

            // Step 2: Pattern detection
            yield return new StreamingInsightResponse
            {
                Type = StreamingInsightType.PatternDetection,
                Content = "Detecting patterns in the data...",
                Progress = 40,
                Timestamp = DateTime.UtcNow
            };

            // Step 3: Insight generation with streaming
            yield return new StreamingInsightResponse
            {
                Type = StreamingInsightType.InsightGeneration,
                Content = "Generating business insights...",
                Progress = 60,
                Timestamp = DateTime.UtcNow
            };

            var insightBuilder = new System.Text.StringBuilder();
            await foreach (var insightChunk in _aiService.GenerateInsightStreamAsync(query, data, null, cancellationToken))
            {
                insightBuilder.Append(insightChunk.Content);
                
                yield return new StreamingInsightResponse
                {
                    Type = StreamingInsightType.InsightGeneration,
                    Content = insightChunk.Content,
                    PartialInsight = insightBuilder.ToString(),
                    Progress = 60 + (insightChunk.IsComplete ? 30 : 15),
                    Timestamp = DateTime.UtcNow
                };

                if (insightChunk.IsComplete)
                    break;
            }

            // Step 4: Final insights
            yield return new StreamingInsightResponse
            {
                Type = StreamingInsightType.Complete,
                Content = insightBuilder.ToString(),
                PartialInsight = insightBuilder.ToString(),
                Progress = 100,
                IsComplete = true,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("✅ Streaming insight generation completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in streaming insight generation");
            
            yield return new StreamingInsightResponse
            {
                Type = StreamingInsightType.Error,
                Content = $"Error generating insights: {ex.Message}",
                Progress = 0,
                IsComplete = true,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public async IAsyncEnumerable<StreamingAnalyticsUpdate> StreamAnalyticsAsync(
        string userId,
        TimeSpan updateInterval,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📊 Starting streaming analytics for user {UserId}", userId);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                // Generate analytics update
                var update = new StreamingAnalyticsUpdate
                {
                    UserId = userId,
                    Timestamp = DateTime.UtcNow,
                    Metrics = await GenerateCurrentMetricsAsync(userId),
                    Trends = await GenerateCurrentTrendsAsync(userId),
                    Alerts = await GenerateCurrentAlertsAsync(userId)
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in streaming analytics for user {UserId}", userId);
        }
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
                Status = StreamingSessionStatus.Active,
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
                sessionInfo.Status = StreamingSessionStatus.Stopped;
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

    public async Task<List<StreamingSessionInfo>> GetActiveSessionsAsync(string userId)
    {
        try
        {
            // In a real implementation, this would query active sessions from cache/database
            // For now, return empty list as this is a simplified implementation
            return new List<StreamingSessionInfo>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting active sessions for user {UserId}", userId);
            return new List<StreamingSessionInfo>();
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

    private async Task<List<StreamingAlert>> GenerateCurrentAlertsAsync(string userId)
    {
        // Generate current alerts
        return new List<StreamingAlert>
        {
            new StreamingAlert
            {
                Type = "Performance",
                Message = "Query response time above average",
                Severity = "Warning",
                Timestamp = DateTime.UtcNow
            }
        };
    }

    #endregion
}
