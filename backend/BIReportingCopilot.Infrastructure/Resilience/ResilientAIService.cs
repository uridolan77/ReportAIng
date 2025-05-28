using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using System.Runtime.CompilerServices;

namespace BIReportingCopilot.Infrastructure.Resilience;

/// <summary>
/// Resilient AI service with retry policies, circuit breaker, and timeout handling
/// </summary>
public class ResilientAIService : IAIService
{
    private readonly IAIService _innerService;
    private readonly ILogger<ResilientAIService> _logger;
    private readonly IAsyncPolicy<string> _retryPolicy;
    private readonly IAsyncPolicy<string> _circuitBreakerPolicy;
    private readonly IAsyncPolicy<string> _combinedPolicy;

    public ResilientAIService(IAIService innerService, ILogger<ResilientAIService> logger)
    {
        _innerService = innerService;
        _logger = logger;

        // Configure retry policy with exponential backoff
        _retryPolicy = Policy
            .HandleResult<string>(r => string.IsNullOrEmpty(r))
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("AI service retry {RetryCount} after {Delay}ms. Reason: {Reason}",
                        retryCount, timespan.TotalMilliseconds, outcome.Exception?.Message ?? "Empty result");
                });

        // Configure circuit breaker
        _circuitBreakerPolicy = Policy
            .HandleResult<string>(r => string.IsNullOrEmpty(r))
            .Or<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<TimeoutException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (exception, duration) =>
                {
                    _logger.LogError("AI service circuit breaker opened for {Duration}ms. Reason: {Reason}",
                        duration.TotalMilliseconds, exception.Exception?.Message ?? "Multiple failures");
                },
                onReset: () =>
                {
                    _logger.LogInformation("AI service circuit breaker reset - service recovered");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("AI service circuit breaker half-open - testing service");
                });

        // Combine policies: retry -> circuit breaker
        _combinedPolicy = Policy.WrapAsync(_retryPolicy, _circuitBreakerPolicy);
    }

    public async Task<string> GenerateSQLAsync(string prompt)
    {
        return await GenerateSQLAsync(prompt, CancellationToken.None);
    }

    public async Task<string> GenerateSQLAsync(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            return await _combinedPolicy.ExecuteAsync(async () =>
            {
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                var result = await _innerService.GenerateSQLAsync(prompt, combinedCts.Token);

                if (string.IsNullOrEmpty(result))
                {
                    throw new InvalidOperationException("AI service returned empty result");
                }

                return result;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate SQL after all retry attempts for prompt: {Prompt}", prompt);
            return GenerateFallbackSQL(prompt);
        }
    }

    public async Task<string> GenerateInsightAsync(string query, object[] data)
    {
        try
        {
            return await _combinedPolicy.ExecuteAsync(async () =>
            {
                var result = await _innerService.GenerateInsightAsync(query, data);

                if (string.IsNullOrEmpty(result))
                {
                    throw new InvalidOperationException("AI service returned empty insight");
                }

                return result;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate insight after all retry attempts");
            return "Unable to generate insights at this time. Please try again later.";
        }
    }

    public async Task<string> GenerateVisualizationConfigAsync(string query, ColumnMetadata[] columns, object[] data)
    {
        try
        {
            return await _combinedPolicy.ExecuteAsync(async () =>
            {
                var result = await _innerService.GenerateVisualizationConfigAsync(query, columns, data);

                if (string.IsNullOrEmpty(result))
                {
                    throw new InvalidOperationException("AI service returned empty visualization config");
                }

                return result;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate visualization config after all retry attempts");
            return """{"type": "table", "title": "Query Results"}""";
        }
    }

    public async Task<double> CalculateConfidenceScoreAsync(string naturalLanguageQuery, string generatedSQL)
    {
        try
        {
            // Confidence calculation is less critical, use simpler retry
            var simpleRetryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(2, _ => TimeSpan.FromMilliseconds(500));

            return await simpleRetryPolicy.ExecuteAsync(async () =>
            {
                return await _innerService.CalculateConfidenceScoreAsync(naturalLanguageQuery, generatedSQL);
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to calculate confidence score, using default");
            return 0.5; // Default confidence
        }
    }

    public async Task<string[]> GenerateQuerySuggestionsAsync(string context, SchemaMetadata schema)
    {
        try
        {
            var simpleRetryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(2, _ => TimeSpan.FromMilliseconds(500));

            return await simpleRetryPolicy.ExecuteAsync(async () =>
            {
                return await _innerService.GenerateQuerySuggestionsAsync(context, schema);
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to generate query suggestions, using defaults");
            return new[] { "Show me all data", "Count total records", "Show recent data" };
        }
    }

    public async Task<bool> ValidateQueryIntentAsync(string naturalLanguageQuery)
    {
        try
        {
            return await _innerService.ValidateQueryIntentAsync(naturalLanguageQuery);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to validate query intent, allowing by default");
            return true; // Fail open for validation
        }
    }

    // Streaming methods with resilience
    public async IAsyncEnumerable<StreamingResponse> GenerateSQLStreamAsync(
        string prompt,
        SchemaMetadata? schema = null,
        QueryContext? context = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var streamingRetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(1));

        var hasYieldedAny = false;

        try
        {
            await foreach (var response in _innerService.GenerateSQLStreamAsync(prompt, schema, context, cancellationToken))
            {
                hasYieldedAny = true;
                yield return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Streaming SQL generation failed");

            if (!hasYieldedAny)
            {
                // If we haven't yielded anything yet, try fallback
                yield return new StreamingResponse
                {
                    Type = StreamingResponseType.Error,
                    Content = "Service temporarily unavailable. Using fallback response.",
                    IsComplete = false
                };

                yield return new StreamingResponse
                {
                    Type = StreamingResponseType.SQLGeneration,
                    Content = GenerateFallbackSQL(prompt),
                    IsComplete = true
                };
            }
            else
            {
                // If we were in the middle of streaming, send error
                yield return new StreamingResponse
                {
                    Type = StreamingResponseType.Error,
                    Content = "Stream interrupted",
                    IsComplete = true
                };
            }
        }
    }

    public async IAsyncEnumerable<StreamingResponse> GenerateInsightStreamAsync(
        string query,
        object[] data,
        AnalysisContext? context = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        try
        {
            await foreach (var response in _innerService.GenerateInsightStreamAsync(query, data, context, cancellationToken))
            {
                yield return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Streaming insight generation failed");
            yield return new StreamingResponse
            {
                Type = StreamingResponseType.Error,
                Content = "Unable to generate insights at this time",
                IsComplete = true
            };
        }
    }

    public async IAsyncEnumerable<StreamingResponse> GenerateExplanationStreamAsync(
        string sql,
        StreamingQueryComplexity complexity = StreamingQueryComplexity.Medium,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        try
        {
            await foreach (var response in _innerService.GenerateExplanationStreamAsync(sql, complexity, cancellationToken))
            {
                yield return response;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Streaming explanation generation failed");
            yield return new StreamingResponse
            {
                Type = StreamingResponseType.Error,
                Content = "Unable to generate explanation at this time",
                IsComplete = true
            };
        }
    }

    private string GenerateFallbackSQL(string prompt)
    {
        return $"-- Fallback response for: {prompt}\nSELECT 'Service temporarily unavailable' as Message";
    }
}
