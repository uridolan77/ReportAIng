using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Infrastructure.Configuration;
using Microsoft.Data.SqlClient;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace BIReportingCopilot.Infrastructure.Health;

public interface IStartupHealthValidator
{
    Task ValidateAllServicesAsync();
    HealthCheckResult GetCachedStatus(string serviceName);
    Task RefreshServiceStatusAsync(string serviceName);
}

public class StartupHealthValidator : IStartupHealthValidator
{
    private readonly ILogger<StartupHealthValidator> _logger;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, HealthCheckResult> _statusCache;

    public StartupHealthValidator(
        ILogger<StartupHealthValidator> logger,
        IConfiguration configuration,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _statusCache = new ConcurrentDictionary<string, HealthCheckResult>();
    }

    public async Task ValidateAllServicesAsync()
    {
        _logger.LogInformation("Starting comprehensive service validation at startup...");

        var validationTasks = new[]
        {
            ValidateOpenAIAsync(),
            ValidateDatabaseAsync("defaultdb"),
            ValidateDatabaseAsync("bidatabase"),
            ValidateRedisAsync()
        };

        await Task.WhenAll(validationTasks);

        _logger.LogInformation("Startup service validation completed");
    }

    public HealthCheckResult GetCachedStatus(string serviceName)
    {
        return _statusCache.GetValueOrDefault(serviceName,
            HealthCheckResult.Unhealthy($"Service {serviceName} not validated yet"));
    }

    public async Task RefreshServiceStatusAsync(string serviceName)
    {
        switch (serviceName.ToLower())
        {
            case "openai":
                await ValidateOpenAIAsync();
                break;
            case "defaultdb":
                await ValidateDatabaseAsync("defaultdb");
                break;
            case "bidatabase":
                await ValidateDatabaseAsync("bidatabase");
                break;
            case "redis":
                await ValidateRedisAsync();
                break;
        }
    }

    private async Task ValidateOpenAIAsync()
    {
        try
        {
            _logger.LogInformation("Validating OpenAI service...");

            using var scope = _serviceProvider.CreateScope();
            var openAIService = scope.ServiceProvider.GetRequiredService<IOpenAIService>();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Test with a simple validation first
            _logger.LogInformation("Testing OpenAI service with ValidateQueryIntentAsync...");
            var isValid = await openAIService.ValidateQueryIntentAsync("show me test data");
            _logger.LogInformation("ValidateQueryIntentAsync result: {IsValid}", isValid);

            if (!isValid)
            {
                _logger.LogWarning("ValidateQueryIntentAsync returned false for 'show me test data' query");
                var result = HealthCheckResult.Degraded("OpenAI service validation failed", null, new Dictionary<string, object>
                {
                    ["description"] = "OpenAI service validation failed",
                    ["exception"] = string.Empty,
                    ["possibleCauses"] = new[]
                    {
                        "OpenAI API key not configured",
                        "Azure OpenAI endpoint not accessible",
                        "API quota exceeded",
                        "Network connectivity issues"
                    }
                });
                _statusCache["openai"] = result;
                _logger.LogWarning("OpenAI service validation failed - ValidateQueryIntentAsync returned false");
                return;
            }

            // Now test with an actual OpenAI API call
            _logger.LogInformation("Testing OpenAI service with actual API call...");
            try
            {
                var testSql = await openAIService.GenerateSQLAsync("show me test data", cts.Token);
                _logger.LogInformation("OpenAI API call successful. Generated SQL: {TestSql}", testSql);
            }
            catch (Exception apiEx)
            {
                _logger.LogError(apiEx, "OpenAI API call failed during startup validation");
                var result = HealthCheckResult.Degraded("OpenAI API call failed", apiEx, new Dictionary<string, object>
                {
                    ["description"] = "OpenAI API call failed",
                    ["exception"] = apiEx.Message,
                    ["possibleCauses"] = new[]
                    {
                        "OpenAI API key invalid or expired",
                        "Network connectivity issues",
                        "API quota exceeded",
                        "OpenAI service temporarily unavailable"
                    }
                });
                _statusCache["openai"] = result;
                return;
            }

            // Determine which service is being used
            var azureEndpoint = _configuration["AzureOpenAI:Endpoint"];
            var azureApiKey = _configuration["AzureOpenAI:ApiKey"];
            var openAIApiKey = _configuration["OpenAI:ApiKey"];

            var hasAzureConfig = !string.IsNullOrEmpty(azureEndpoint) && !string.IsNullOrEmpty(azureApiKey);
            var hasOpenAIConfig = !string.IsNullOrEmpty(openAIApiKey) && openAIApiKey != "your-openai-api-key-here";

            var configType = hasAzureConfig ? "Azure OpenAI" : "OpenAI";
            var healthyResult = HealthCheckResult.Healthy($"OpenAI service is healthy (using {configType})");
            _statusCache["openai"] = healthyResult;

            _logger.LogInformation("OpenAI service validation successful (using {ConfigType})", configType);
        }
        catch (OperationCanceledException)
        {
            var result = HealthCheckResult.Unhealthy("OpenAI service validation timed out");
            _statusCache["openai"] = result;
            _logger.LogWarning("OpenAI service validation timed out");
        }
        catch (Exception ex)
        {
            var result = HealthCheckResult.Unhealthy("OpenAI service is unavailable", ex);
            _statusCache["openai"] = result;
            _logger.LogError(ex, "OpenAI service validation failed");
        }
    }

    private async Task ValidateDatabaseAsync(string serviceName)
    {
        try
        {
            _logger.LogInformation("Validating database service: {ServiceName}", serviceName);

            using var scope = _serviceProvider.CreateScope();

            string connectionString;
            if (serviceName.ToLower() == "bidatabase")
            {
                // Use the connection string provider for BIDatabase to handle Azure Key Vault
                var connectionStringProvider = scope.ServiceProvider.GetRequiredService<IConnectionStringProvider>();
                connectionString = await connectionStringProvider.GetConnectionStringAsync("BIDatabase");
            }
            else
            {
                // Use direct configuration for DefaultConnection
                connectionString = _configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                var result = HealthCheckResult.Unhealthy($"Connection string for {serviceName} not configured");
                _statusCache[serviceName] = result;
                return;
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            using var connection = new SqlConnection(connectionString);

            await connection.OpenAsync(cts.Token);

            using var command = new SqlCommand("SELECT 1", connection);
            var queryResult = await command.ExecuteScalarAsync(cts.Token);

            if (queryResult?.ToString() == "1")
            {
                var description = serviceName.ToLower() == "bidatabase"
                    ? "BIDatabase is accessible (Server: 185.64.56.157, Database: DailyActionsDB)"
                    : $"Database {serviceName} is accessible";

                var healthyResult = HealthCheckResult.Healthy(description);
                _statusCache[serviceName] = healthyResult;
                _logger.LogInformation("Database {ServiceName} validation successful", serviceName);
            }
            else
            {
                var unhealthyResult = HealthCheckResult.Unhealthy($"Database {serviceName} query returned unexpected result");
                _statusCache[serviceName] = unhealthyResult;
                _logger.LogWarning("Database {ServiceName} validation failed - unexpected result", serviceName);
            }
        }
        catch (OperationCanceledException)
        {
            var result = HealthCheckResult.Unhealthy($"Database {serviceName} connection timed out");
            _statusCache[serviceName] = result;
            _logger.LogWarning("Database {ServiceName} validation timed out", serviceName);
        }
        catch (Exception ex)
        {
            var result = HealthCheckResult.Unhealthy($"Database {serviceName} is unavailable", ex);
            _statusCache[serviceName] = result;
            _logger.LogError(ex, "Database {ServiceName} validation failed", serviceName);
        }
    }

    private async Task ValidateRedisAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();

            // Try to get Redis connection multiplexer from DI
            var redis = scope.ServiceProvider.GetService<IConnectionMultiplexer>();

            if (redis == null)
            {
                _statusCache["redis"] = HealthCheckResult.Healthy("Redis not configured (using in-memory cache)");
                return;
            }

            _logger.LogInformation("Validating Redis service...");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            var database = redis.GetDatabase();

            var testKey = $"health-check-{Guid.NewGuid()}";
            var testValue = "test-value";

            await database.StringSetAsync(testKey, testValue);
            var retrievedValue = await database.StringGetAsync(testKey);
            await database.KeyDeleteAsync(testKey);

            if (retrievedValue == testValue)
            {
                _statusCache["redis"] = HealthCheckResult.Healthy("Redis is accessible and functioning");
                _logger.LogInformation("Redis validation successful");
            }
            else
            {
                _statusCache["redis"] = HealthCheckResult.Unhealthy("Redis read/write test failed");
                _logger.LogWarning("Redis validation failed - read/write test failed");
            }
        }
        catch (Exception ex)
        {
            _statusCache["redis"] = HealthCheckResult.Unhealthy("Redis is unavailable", ex);
            _logger.LogError(ex, "Redis validation failed");
        }
    }
}
