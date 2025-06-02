using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using BIReportingCopilot.Infrastructure.Configuration;
using Microsoft.Data.SqlClient;

namespace BIReportingCopilot.Infrastructure.Health;

/// <summary>
/// Interface for startup health validation
/// </summary>
public interface IStartupHealthValidator
{
    /// <summary>
    /// Validate application startup health
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    Task<StartupValidationResult> ValidateStartupHealthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Startup health validator implementation
/// </summary>
public class StartupHealthValidator : IStartupHealthValidator
{
    private readonly ILogger<StartupHealthValidator> _logger;
    private readonly IServiceProvider _serviceProvider;

    public StartupHealthValidator(
        ILogger<StartupHealthValidator> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Validate application startup health
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    public async Task<StartupValidationResult> ValidateStartupHealthAsync(CancellationToken cancellationToken = default)
    {
        var result = new StartupValidationResult();

        try
        {
            _logger.LogInformation("Starting startup health validation");

            // Validate configuration
            await ValidateConfigurationAsync(result, cancellationToken);

            // Validate dependencies
            await ValidateDependenciesAsync(result, cancellationToken);

            // Validate services
            await ValidateServicesAsync(result, cancellationToken);

            result.IsValid = result.ValidationErrors.Count == 0;
            result.CompletedAt = DateTime.UtcNow;

            _logger.LogInformation("Startup health validation completed. Valid: {IsValid}, Errors: {ErrorCount}",
                result.IsValid, result.ValidationErrors.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during startup health validation");
            result.ValidationErrors.Add($"Validation failed with exception: {ex.Message}");
            result.IsValid = false;
            result.CompletedAt = DateTime.UtcNow;
            return result;
        }
    }

    private async Task ValidateConfigurationAsync(StartupValidationResult result, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);

        // Simulate configuration validation
        _logger.LogDebug("Validating configuration");

        // Add any configuration validation logic here
        // For now, assume configuration is valid
    }

    private async Task ValidateDependenciesAsync(StartupValidationResult result, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Validating database dependencies...");

        // Test BIDatabase connection using scoped service
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var connectionStringProvider = scope.ServiceProvider.GetRequiredService<IConnectionStringProvider>();

            _logger.LogInformation("Testing BIDatabase connection...");
            var connectionString = await connectionStringProvider.GetConnectionStringAsync("BIDatabase");

            if (string.IsNullOrEmpty(connectionString))
            {
                result.ValidationErrors.Add("BIDatabase connection string is not configured");
                _logger.LogError("BIDatabase connection string is empty or null");
                return;
            }

            _logger.LogInformation("BIDatabase connection string resolved, testing connection...");

            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                ConnectTimeout = 10 // 10 seconds for startup validation
            };

            using var connection = new SqlConnection(builder.ConnectionString);

            _logger.LogInformation("Opening connection to BIDatabase server: {Server}, database: {Database}",
                builder.DataSource, builder.InitialCatalog);

            var startTime = DateTime.UtcNow;
            await connection.OpenAsync(cancellationToken);
            var connectionTime = DateTime.UtcNow - startTime;

            _logger.LogInformation("BIDatabase connection opened successfully in {ConnectionTime}ms", connectionTime.TotalMilliseconds);

            // Test a simple query
            using var command = new SqlCommand("SELECT 1", connection) { CommandTimeout = 5 };
            var queryResult = await command.ExecuteScalarAsync(cancellationToken);

            _logger.LogInformation("BIDatabase test query successful, result: {Result}", queryResult);
        }
        catch (Exception ex)
        {
            var errorMessage = $"BIDatabase connection failed: {ex.Message}";
            result.ValidationErrors.Add(errorMessage);
            _logger.LogError(ex, "BIDatabase connection validation failed: {Message}", ex.Message);
        }
    }

    private async Task ValidateServicesAsync(StartupValidationResult result, CancellationToken cancellationToken)
    {
        await Task.Delay(10, cancellationToken);

        // Simulate service validation
        _logger.LogDebug("Validating services");

        // Add any service validation logic here
        // For now, assume services are valid
    }
}


