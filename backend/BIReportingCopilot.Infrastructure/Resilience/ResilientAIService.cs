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
        IAsyncEnumerable<StreamingResponse> stream;

        try
        {
            stream = _innerService.GenerateSQLStreamAsync(prompt, schema, context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize streaming SQL generation");
            stream = CreateErrorStreamAsync("Service temporarily unavailable. Using fallback response.", GenerateFallbackSQL(prompt));
        }

        var hasYieldedAny = false;

        await foreach (var response in stream.WithCancellation(cancellationToken))
        {
            hasYieldedAny = true;
            yield return response;
        }

        // If no responses were yielded and we didn't get an error stream, provide fallback
        if (!hasYieldedAny)
        {
            await foreach (var response in CreateErrorStreamAsync("Service temporarily unavailable. Using fallback response.", GenerateFallbackSQL(prompt)))
            {
                yield return response;
            }
        }
    }

    public async IAsyncEnumerable<StreamingResponse> GenerateInsightStreamAsync(
        string query,
        object[] data,
        AnalysisContext? context = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<StreamingResponse> stream;

        try
        {
            stream = _innerService.GenerateInsightStreamAsync(query, data, context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize streaming insight generation");
            stream = CreateSingleErrorStreamAsync("Unable to generate insights at this time");
        }

        await foreach (var response in stream.WithCancellation(cancellationToken))
        {
            yield return response;
        }
    }

    public async IAsyncEnumerable<StreamingResponse> GenerateExplanationStreamAsync(
        string sql,
        StreamingQueryComplexity complexity = StreamingQueryComplexity.Medium,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IAsyncEnumerable<StreamingResponse> stream;

        try
        {
            stream = _innerService.GenerateExplanationStreamAsync(sql, complexity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize streaming explanation generation");
            stream = CreateSingleErrorStreamAsync("Unable to generate explanation at this time");
        }

        await foreach (var response in stream.WithCancellation(cancellationToken))
        {
            yield return response;
        }
    }

    private string GenerateFallbackSQL(string prompt)
    {
        return $"-- Fallback response for: {prompt}\nSELECT 'Service temporarily unavailable' as Message";
    }

    private async IAsyncEnumerable<StreamingResponse> CreateErrorStreamAsync(string errorMessage, string? fallbackContent = null)
    {
        yield return new StreamingResponse
        {
            Type = StreamingResponseType.Error,
            Content = errorMessage,
            IsComplete = fallbackContent == null
        };

        if (fallbackContent != null)
        {
            yield return new StreamingResponse
            {
                Type = StreamingResponseType.SQLGeneration,
                Content = fallbackContent,
                IsComplete = true
            };
        }
    }

    private async IAsyncEnumerable<StreamingResponse> CreateSingleErrorStreamAsync(string errorMessage)
    {
        yield return new StreamingResponse
        {
            Type = StreamingResponseType.Error,
            Content = errorMessage,
            IsComplete = true
        };
    }
}
