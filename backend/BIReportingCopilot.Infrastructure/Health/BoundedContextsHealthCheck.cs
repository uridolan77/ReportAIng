using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using System.Text;

namespace BIReportingCopilot.Infrastructure.Health;

/// <summary>
/// Health check for bounded contexts
/// </summary>
public class BoundedContextsHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BoundedContextsHealthCheck> _logger;

    public BoundedContextsHealthCheck(
        IServiceProvider serviceProvider,
        ILogger<BoundedContextsHealthCheck> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<string>();
        var data = new Dictionary<string, object>();

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            // Check Query Context
            await CheckContext<QueryDbContext>("Query context", issues, data, serviceProvider, cancellationToken);

            // Check Security Context
            await CheckContext<SecurityDbContext>("Security context", issues, data, serviceProvider, cancellationToken);

            // Check Schema Context
            await CheckContext<SchemaDbContext>("Schema context", issues, data, serviceProvider, cancellationToken);

            // Check Tuning Context
            await CheckContext<TuningDbContext>("Tuning context", issues, data, serviceProvider, cancellationToken);

            // Check Monitoring Context
            await CheckContext<MonitoringDbContext>("Monitoring context", issues, data, serviceProvider, cancellationToken);

            if (issues.Count == 0)
            {
                _logger.LogInformation("All bounded contexts are healthy");
                return HealthCheckResult.Healthy("All bounded contexts are functioning properly", data);
            }

            var message = $"Multiple bounded context issues: {string.Join("; ", issues)}";
            _logger.LogWarning("Bounded contexts health check failed: {Message}", message);
            return HealthCheckResult.Unhealthy(message, data: data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during bounded contexts health check");
            return HealthCheckResult.Unhealthy(
                $"Bounded contexts health check failed: {ex.Message}",
                ex,
                data);
        }
    }

    private async Task CheckContext<TContext>(
        string contextName,
        List<string> issues,
        Dictionary<string, object> data,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
        where TContext : class
    {
        try
        {
            // Check if context can be resolved
            var context = serviceProvider.GetService<TContext>();
            if (context == null)
            {
                issues.Add($"{contextName} service not registered");
                data[$"{contextName}_status"] = "Not registered";
                return;
            }

            // If it's a DbContext, try to connect
            if (context is Microsoft.EntityFrameworkCore.DbContext dbContext)
            {
                try
                {
                    // Test database connectivity with timeout
                    var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
                    if (!canConnect)
                    {
                        issues.Add($"{contextName} connection failed");
                        data[$"{contextName}_status"] = "Connection failed";
                        return;
                    }

                    // Try a simple operation to test if model is configured properly
                    try
                    {
                        // This will trigger model building and validation
                        var model = dbContext.Model;
                        data[$"{contextName}_status"] = "Healthy";
                        data[$"{contextName}_entities"] = model.GetEntityTypes().Count();
                    }
                    catch (Exception modelEx)
                    {
                        issues.Add($"{contextName} operation failed: {modelEx.Message}");
                        data[$"{contextName}_status"] = $"Model error: {modelEx.Message}";
                    }
                }
                catch (Exception dbEx)
                {
                    issues.Add($"{contextName} database error: {dbEx.Message}");
                    data[$"{contextName}_status"] = $"Database error: {dbEx.Message}";
                }
            }
            else
            {
                data[$"{contextName}_status"] = "Service available (non-DbContext)";
            }
        }
        catch (Exception ex)
        {
            issues.Add($"{contextName} initialization failed: {ex.Message}");
            data[$"{contextName}_status"] = $"Initialization error: {ex.Message}";
        }
    }
}
