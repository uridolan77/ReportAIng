using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using System.Data.SqlClient;

namespace BIReportingCopilot.API.HealthChecks;

/// <summary>
/// Health check for BI database connectivity
/// </summary>
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

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var data = new Dictionary<string, object>();

        try
        {
            // Get connection string
            var connectionString = await _connectionStringProvider.GetConnectionStringAsync("BIDatabase");
            
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogWarning("No BIDatabase connection string available");
                return HealthCheckResult.Unhealthy("No BIDatabase connection string configured", data: data);
            }

            // Parse connection string for logging (without password)
            var builder = new SqlConnectionStringBuilder(connectionString);
            data["server"] = builder.DataSource;
            data["database"] = builder.InitialCatalog;
            data["connection_timeout"] = builder.ConnectTimeout;

            // Test connection
            using var connection = new SqlConnection(connectionString);
            
            var connectTask = connection.OpenAsync(cancellationToken);
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            
            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                _logger.LogWarning("BIDatabase connection timeout after 5 seconds");
                return HealthCheckResult.Unhealthy("BIDatabase connection timeout", data: data);
            }

            if (connectTask.Exception != null)
            {
                _logger.LogError(connectTask.Exception, "BIDatabase connection failed");
                return HealthCheckResult.Unhealthy($"BIDatabase connection failed: {connectTask.Exception.Message}", connectTask.Exception, data);
            }

            // Test a simple query
            using var command = new SqlCommand("SELECT 1", connection);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            
            data["test_query_result"] = result?.ToString() ?? "null";
            data["status"] = "connected";

            _logger.LogDebug("BIDatabase health check passed");
            return HealthCheckResult.Healthy("BIDatabase is accessible", data);
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL error during BIDatabase health check");
            data["sql_error_number"] = sqlEx.Number;
            data["sql_error_severity"] = sqlEx.Class;
            
            return HealthCheckResult.Unhealthy(
                $"BIDatabase SQL error: {sqlEx.Message}",
                sqlEx,
                data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during BIDatabase health check");
            return HealthCheckResult.Unhealthy(
                $"BIDatabase health check failed: {ex.Message}",
                ex,
                data);
        }
    }
}
