using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.Health;

public class FastOpenAIHealthCheck : IHealthCheck
{
    private readonly IStartupHealthValidator _validator;
    private readonly ILogger<FastOpenAIHealthCheck> _logger;

    public FastOpenAIHealthCheck(IStartupHealthValidator validator, ILogger<FastOpenAIHealthCheck> logger)
    {
        _validator = validator;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var result = _validator.GetCachedStatus("openai");
        _logger.LogDebug("OpenAI health check returned cached status: {Status}", result.Status);
        return Task.FromResult(result);
    }
}

public class FastDatabaseHealthCheck : IHealthCheck
{
    private readonly IStartupHealthValidator _validator;
    private readonly ILogger<FastDatabaseHealthCheck> _logger;
    private readonly string _serviceName;

    public FastDatabaseHealthCheck(IStartupHealthValidator validator, ILogger<FastDatabaseHealthCheck> logger, string serviceName)
    {
        _validator = validator;
        _logger = logger;
        _serviceName = serviceName;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var result = _validator.GetCachedStatus(_serviceName);
        _logger.LogDebug("Database {ServiceName} health check returned cached status: {Status}", _serviceName, result.Status);
        return Task.FromResult(result);
    }
}

public class FastRedisHealthCheck : IHealthCheck
{
    private readonly IStartupHealthValidator _validator;
    private readonly ILogger<FastRedisHealthCheck> _logger;

    public FastRedisHealthCheck(IStartupHealthValidator validator, ILogger<FastRedisHealthCheck> logger)
    {
        _validator = validator;
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var result = _validator.GetCachedStatus("redis");
        _logger.LogDebug("Redis health check returned cached status: {Status}", result.Status);
        return Task.FromResult(result);
    }
}

// Factory classes for dependency injection
public class BIDatabaseHealthCheck : FastDatabaseHealthCheck
{
    public BIDatabaseHealthCheck(IStartupHealthValidator validator, ILogger<FastDatabaseHealthCheck> logger)
        : base(validator, logger, "bidatabase")
    {
    }
}

public class DefaultDatabaseHealthCheck : FastDatabaseHealthCheck
{
    public DefaultDatabaseHealthCheck(IStartupHealthValidator validator, ILogger<FastDatabaseHealthCheck> logger)
        : base(validator, logger, "defaultdb")
    {
    }
}
