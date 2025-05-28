using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Caching.Distributed;
using Azure.AI.OpenAI;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Infrastructure.Configuration;
using Microsoft.Data.SqlClient;
using System.Text;

namespace BIReportingCopilot.API.HealthChecks;

public class OpenAIHealthCheck : IHealthCheck
{
    private readonly IOpenAIService _openAIService;
    private readonly ILogger<OpenAIHealthCheck> _logger;
    private readonly IConfiguration _configuration;

    public OpenAIHealthCheck(
        IOpenAIService openAIService,
        ILogger<OpenAIHealthCheck> logger,
        IConfiguration configuration)
    {
        _openAIService = openAIService;
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
            var isValid = await _openAIService.ValidateQueryIntentAsync("test");

            if (!isValid)
            {
                return HealthCheckResult.Degraded("OpenAI service validation failed", null, new Dictionary<string, object>
                {
                    ["description"] = "OpenAI service validation failed",
                    ["exception"] = (string?)null,
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

public class BIDatabaseHealthCheck : IHealthCheck
{
    private readonly IConnectionStringProvider _connectionStringProvider;
    private readonly ILogger<BIDatabaseHealthCheck> _logger;

    public BIDatabaseHealthCheck(
        IConnectionStringProvider connectionStringProvider,
        ILogger<BIDatabaseHealthCheck> logger)
    {
        _connectionStringProvider = connectionStringProvider;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var timeout = TimeSpan.FromSeconds(3); // Reduced from 10 to 3 seconds
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout);

            // Get the resolved connection string from the provider
            var connectionString = await _connectionStringProvider.GetConnectionStringAsync("BIDatabase");

            if (string.IsNullOrEmpty(connectionString))
            {
                return HealthCheckResult.Unhealthy("BIDatabase connection string is not configured");
            }

            // Test the database connection
            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cts.Token);

            // Test a simple query
            using var command = new SqlCommand("SELECT 1", connection);
            var result = await command.ExecuteScalarAsync(cts.Token);

            if (result?.ToString() == "1")
            {
                return HealthCheckResult.Healthy($"BIDatabase is accessible (Server: {connection.DataSource}, Database: {connection.Database})");
            }
            else
            {
                return HealthCheckResult.Degraded("BIDatabase connection succeeded but query test failed");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("BIDatabase health check timed out");
            return HealthCheckResult.Unhealthy("BIDatabase health check timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BIDatabase health check failed");
            return HealthCheckResult.Unhealthy("BIDatabase is unavailable", ex);
        }
    }
}