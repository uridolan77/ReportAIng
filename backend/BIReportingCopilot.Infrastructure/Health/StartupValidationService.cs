using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace BIReportingCopilot.Infrastructure.Health;

public class StartupValidationService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StartupValidationService> _logger;
    private Timer? _refreshTimer;

    public StartupValidationService(IServiceProvider serviceProvider, ILogger<StartupValidationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting startup validation service...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IStartupHealthValidator>();
            
            // Run comprehensive validation at startup
            await validator.ValidateAllServicesAsync();
            
            // Set up periodic refresh (every 5 minutes)
            _refreshTimer = new Timer(RefreshHealthStatus, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
            
            _logger.LogInformation("Startup validation completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Startup validation failed");
            // Don't throw - let the application start even if some services are down
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping startup validation service...");
        
        _refreshTimer?.Dispose();
        
        return Task.CompletedTask;
    }

    private async void RefreshHealthStatus(object? state)
    {
        try
        {
            _logger.LogDebug("Refreshing health status for all services...");
            
            using var scope = _serviceProvider.CreateScope();
            var validator = scope.ServiceProvider.GetRequiredService<IStartupHealthValidator>();
            
            // Refresh all services in parallel
            var refreshTasks = new[]
            {
                validator.RefreshServiceStatusAsync("openai"),
                validator.RefreshServiceStatusAsync("defaultdb"),
                validator.RefreshServiceStatusAsync("bidatabase"),
                validator.RefreshServiceStatusAsync("redis")
            };

            await Task.WhenAll(refreshTasks);
            
            _logger.LogDebug("Health status refresh completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh health status");
        }
    }
}
