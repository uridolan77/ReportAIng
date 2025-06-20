using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Caching.Distributed;
using Azure.AI.OpenAI;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Infrastructure.Configuration;
using BIReportingCopilot.Core.Interfaces.Repository;
using Microsoft.Data.SqlClient;
using System.Text;

namespace BIReportingCopilot.API.HealthChecks;

/// <summary>
/// Health check implementations for core application services
/// </summary>

public class OpenAIHealthCheck : IHealthCheck
{
    private readonly IAIService _aiService;
    private readonly ILogger<OpenAIHealthCheck> _logger;
    private readonly IConfiguration _configuration;

    public OpenAIHealthCheck(
        IAIService aiService,
        ILogger<OpenAIHealthCheck> logger,
        IConfiguration configuration)
    {
        _aiService = aiService;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeout = TimeSpan.FromSeconds(3); // Fast timeout
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            // Quick validation test - lightweight API call
            var isValid = await _aiService.ValidateQueryIntentAsync("test", "data_query");

            if (!isValid)
            {
                return HealthCheckResult.Degraded("OpenAI service validation failed", null, new Dictionary<string, object>
                {
                    ["description"] = "OpenAI service validation failed",
                    ["possibleCauses"] = new[]
                    {
                        "OpenAI API key not configured",
                        "Azure OpenAI endpoint not accessible",
                        "API quota exceeded",
                        "Network connectivity issues"
                    }
                });
            }

            // Check configuration to determine which service is being used
            var azureEndpoint = _configuration["AzureOpenAI:Endpoint"];
            var azureApiKey = _configuration["AzureOpenAI:ApiKey"];
            var openAIApiKey = _configuration["OpenAI:ApiKey"];

            var hasAzureConfig = !string.IsNullOrEmpty(azureEndpoint) && !string.IsNullOrEmpty(azureApiKey);
            var hasOpenAIConfig = !string.IsNullOrEmpty(openAIApiKey) && openAIApiKey != "your-openai-api-key-here";

            var configType = hasAzureConfig ? "Azure OpenAI" : "OpenAI";
            return HealthCheckResult.Healthy($"OpenAI service is healthy (using {configType})");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("OpenAI health check timed out");
            return HealthCheckResult.Unhealthy("OpenAI service health check timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenAI health check failed");
            return HealthCheckResult.Unhealthy("OpenAI service is unavailable", ex);
        }
    }
}

public class RedisHealthCheck : IHealthCheck
{
    private readonly Microsoft.Extensions.Caching.Distributed.IDistributedCache _distributedCache;
    private readonly ILogger<RedisHealthCheck> _logger;

    public RedisHealthCheck(
        Microsoft.Extensions.Caching.Distributed.IDistributedCache distributedCache,
        ILogger<RedisHealthCheck> logger)
    {
        _distributedCache = distributedCache;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var testKey = $"health_check_{Guid.NewGuid()}";
            var testValue = "health_check_value";
            var timeout = TimeSpan.FromSeconds(2); // Reduced for faster health checks

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            // Test write operation
            await _distributedCache.SetStringAsync(testKey, testValue, cts.Token);

            // Test read operation
            var retrievedValue = await _distributedCache.GetStringAsync(testKey, cts.Token);

            // Test delete operation
            await _distributedCache.RemoveAsync(testKey, cts.Token);

            if (retrievedValue == testValue)
            {
                return HealthCheckResult.Healthy("Redis cache is responding correctly");
            }
            else
            {
                return HealthCheckResult.Degraded("Redis cache responded but data integrity check failed");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Redis health check timed out");
            return HealthCheckResult.Unhealthy("Redis cache health check timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed");
            return HealthCheckResult.Unhealthy("Redis cache is unavailable", ex);
        }
    }
}
