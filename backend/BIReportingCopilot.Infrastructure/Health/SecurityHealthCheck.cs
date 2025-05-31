using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using BIReportingCopilot.Infrastructure.Security;
using StackExchange.Redis;

namespace BIReportingCopilot.Infrastructure.Health;

/// <summary>
/// Health check for security services including rate limiting, secrets management, and SQL validation
/// </summary>
public class SecurityHealthCheck : IHealthCheck
{
    private readonly IRateLimitingService _rateLimitingService;
    private readonly ISecretsManagementService _secretsManagementService;
    private readonly IEnhancedSqlQueryValidator _sqlValidator;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SecurityHealthCheck> _logger;

    public SecurityHealthCheck(
        IRateLimitingService rateLimitingService,
        ISecretsManagementService secretsManagementService,
        IEnhancedSqlQueryValidator sqlValidator,
        IServiceProvider serviceProvider,
        ILogger<SecurityHealthCheck> logger)
    {
        _rateLimitingService = rateLimitingService;
        _secretsManagementService = secretsManagementService;
        _sqlValidator = sqlValidator;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var healthData = new Dictionary<string, object>();
        var issues = new List<string>();
        var overallHealth = HealthStatus.Healthy;

        try
        {
            // Check Redis connectivity for rate limiting
            await CheckRedisConnectivity(healthData, issues, cancellationToken);

            // Check secrets management service
            await CheckSecretsManagement(healthData, issues, cancellationToken);

            // Check SQL validation service
            await CheckSqlValidation(healthData, issues, cancellationToken);

            // Check rate limiting service
            await CheckRateLimiting(healthData, issues, cancellationToken);

            // Determine overall health status
            if (issues.Any(i => i.Contains("Critical") || i.Contains("Failed")))
            {
                overallHealth = HealthStatus.Unhealthy;
            }
            else if (issues.Any(i => i.Contains("Warning") || i.Contains("Degraded")))
            {
                overallHealth = HealthStatus.Degraded;
            }

            var description = issues.Any()
                ? $"Security services health check completed with {issues.Count} issues"
                : "All security services are healthy";

            return new HealthCheckResult(
                overallHealth,
                description,
                data: healthData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Security health check failed");

            return new HealthCheckResult(
                HealthStatus.Unhealthy,
                "Security health check failed with exception",
                ex,
                healthData);
        }
    }

    private async Task CheckRedisConnectivity(Dictionary<string, object> healthData, List<string> issues, CancellationToken cancellationToken)
    {
        try
        {
            // Try to get Redis connection from service provider
            var redis = _serviceProvider.GetService<IConnectionMultiplexer>();

            if (redis != null)
            {
                var database = redis.GetDatabase();
                var testKey = "health-check-test";
                var testValue = DateTime.UtcNow.ToString();

                // Test basic Redis operations
                await database.StringSetAsync(testKey, testValue, TimeSpan.FromSeconds(10));
                var retrievedValue = await database.StringGetAsync(testKey);
                await database.KeyDeleteAsync(testKey);

                if (retrievedValue == testValue)
                {
                    healthData["redis_connectivity"] = "Healthy";
                    healthData["redis_latency_ms"] = await MeasureRedisLatency(database);
                }
                else
                {
                    issues.Add("Redis connectivity test failed - value mismatch");
                    healthData["redis_connectivity"] = "Failed";
                }
            }
            else
            {
                // Redis is disabled - this is not an issue, just informational
                healthData["redis_connectivity"] = "Disabled";
                healthData["redis_note"] = "Redis is disabled - using in-memory caching";
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Redis connectivity check failed");
            issues.Add($"Redis connectivity failed: {ex.Message}");
            healthData["redis_connectivity"] = "Failed";
            healthData["redis_error"] = ex.Message;
        }
    }

    private async Task CheckSecretsManagement(Dictionary<string, object> healthData, List<string> issues, CancellationToken cancellationToken)
    {
        try
        {
            var secretsHealth = await _secretsManagementService.GetHealthStatusAsync(cancellationToken);

            healthData["secrets_management"] = secretsHealth.IsHealthy ? "Healthy" : "Unhealthy";
            healthData["secrets_key_vault_connectivity"] = secretsHealth.KeyVaultConnectivity;
            healthData["secrets_cached_count"] = secretsHealth.CachedSecretsCount;
            healthData["secrets_expired_count"] = secretsHealth.ExpiredSecretsCount;

            if (!secretsHealth.IsHealthy)
            {
                issues.Add($"Secrets management unhealthy: {secretsHealth.ErrorMessage}");
            }

            if (secretsHealth.ExpiredSecretsCount > 0)
            {
                issues.Add($"Warning: {secretsHealth.ExpiredSecretsCount} cached secrets have expired");
            }

            if (!secretsHealth.KeyVaultConnectivity)
            {
                issues.Add("Warning: Key Vault connectivity is not available");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Secrets management health check failed");
            issues.Add($"Secrets management check failed: {ex.Message}");
            healthData["secrets_management"] = "Failed";
            healthData["secrets_error"] = ex.Message;
        }
    }

    private async Task CheckSqlValidation(Dictionary<string, object> healthData, List<string> issues, CancellationToken cancellationToken)
    {
        try
        {
            // Test SQL validation with a safe query
            var testQuery = "SELECT COUNT(*) FROM Users WHERE Id = 1";
            var validationResult = await _sqlValidator.ValidateQueryAsync(testQuery, "health-check", "system");

            healthData["sql_validation"] = "Healthy";
            healthData["sql_validation_test_result"] = validationResult.IsValid;
            healthData["sql_validation_risk_score"] = validationResult.RiskScore;

            if (!validationResult.IsValid)
            {
                issues.Add("Warning: SQL validation test query was marked as invalid");
            }

            // Test with a dangerous query to ensure detection works (this is intentional for testing)
            _logger.LogDebug("Testing SQL validation with intentionally dangerous query for health check");
            var dangerousQuery = "DROP TABLE Users; --";
            var dangerousResult = await _sqlValidator.ValidateQueryAsync(dangerousQuery, "health-check-intentional-test", "system");

            healthData["sql_validation_dangerous_test"] = !dangerousResult.IsValid;

            if (dangerousResult.IsValid)
            {
                issues.Add("Critical: SQL validation failed to detect dangerous query");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SQL validation health check failed");
            issues.Add($"SQL validation check failed: {ex.Message}");
            healthData["sql_validation"] = "Failed";
            healthData["sql_validation_error"] = ex.Message;
        }
    }

    private async Task CheckRateLimiting(Dictionary<string, object> healthData, List<string> issues, CancellationToken cancellationToken)
    {
        try
        {
            var testPolicy = new RateLimitPolicy
            {
                Name = "health-check-test",
                RequestLimit = 1000,
                WindowSizeSeconds = 60,
                Description = "Health check test policy"
            };

            var testIdentifier = "health-check-test-user";
            var rateLimitResult = await _rateLimitingService.CheckRateLimitAsync(testIdentifier, testPolicy, cancellationToken);

            healthData["rate_limiting"] = "Healthy";
            healthData["rate_limiting_test_allowed"] = rateLimitResult.IsAllowed;
            healthData["rate_limiting_test_count"] = rateLimitResult.RequestCount;

            if (!rateLimitResult.IsAllowed)
            {
                issues.Add("Warning: Rate limiting test was not allowed");
            }

            // Check if rate limiting service is active
            var isActive = await _rateLimitingService.IsRateLimitActiveAsync(testIdentifier, testPolicy.Name, cancellationToken);
            healthData["rate_limiting_active"] = isActive;

            // Clean up test data
            await _rateLimitingService.ResetRateLimitAsync(testIdentifier, testPolicy.Name, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Rate limiting health check failed");
            issues.Add($"Rate limiting check failed: {ex.Message}");
            healthData["rate_limiting"] = "Failed";
            healthData["rate_limiting_error"] = ex.Message;
        }
    }

    private async Task<long> MeasureRedisLatency(IDatabase database)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await database.PingAsync();
        stopwatch.Stop();
        return stopwatch.ElapsedMilliseconds;
    }
}

/// <summary>
/// Extension methods for registering security health checks
/// </summary>
public static class SecurityHealthCheckExtensions
{
    /// <summary>
    /// Add security health checks to the health check builder
    /// </summary>
    public static IServiceCollection AddSecurityHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<SecurityHealthCheck>(
                "security",
                HealthStatus.Degraded,
                new[] { "security", "rate-limiting", "secrets", "sql-validation" });

        return services;
    }
}
