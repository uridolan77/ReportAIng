using BIReportingCopilot.Core.Interfaces.Analytics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// Background service for automated A/B test management
/// </summary>
public class ABTestAutomationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ABTestAutomationService> _logger;
    private readonly TimeSpan _processingInterval = TimeSpan.FromHours(6); // Run every 6 hours

    public ABTestAutomationService(
        IServiceProvider serviceProvider,
        ILogger<ABTestAutomationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("A/B Test Automation Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessAutomatedTestManagement(stoppingToken);
                await Task.Delay(_processingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("A/B Test Automation Service is stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in A/B Test Automation Service");
                // Wait a shorter time before retrying on error
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }

    private async Task ProcessAutomatedTestManagement(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var abTestingService = scope.ServiceProvider.GetRequiredService<IABTestingService>();

        try
        {
            _logger.LogInformation("Starting automated A/B test management cycle");

            // Process automated winner selection
            var results = await abTestingService.ProcessAutomatedWinnerSelectionAsync(cancellationToken);
            
            var implementedCount = results.Count(r => r.Action == Core.Models.Analytics.AutomatedTestAction.ImplementedVariant);
            var keptOriginalCount = results.Count(r => r.Action == Core.Models.Analytics.AutomatedTestAction.KeptOriginal);
            var expiredCount = results.Count(r => r.Action == Core.Models.Analytics.AutomatedTestAction.ExpiredTest);
            var errorCount = results.Count(r => r.Action == Core.Models.Analytics.AutomatedTestAction.Error);

            _logger.LogInformation(
                "Automated A/B test management completed. " +
                "Implemented variants: {ImplementedCount}, " +
                "Kept originals: {KeptOriginalCount}, " +
                "Expired tests: {ExpiredCount}, " +
                "Errors: {ErrorCount}",
                implementedCount, keptOriginalCount, expiredCount, errorCount);

            // Log details for implemented variants
            foreach (var result in results.Where(r => r.Action == Core.Models.Analytics.AutomatedTestAction.ImplementedVariant))
            {
                _logger.LogInformation(
                    "Implemented variant for test {TestName} (ID: {TestId}): {TemplateKey}. Reason: {Reason}",
                    result.TestName, result.TestId, result.ImplementedTemplateKey, result.Reason);
            }

            // Log errors
            foreach (var result in results.Where(r => r.Action == Core.Models.Analytics.AutomatedTestAction.Error))
            {
                _logger.LogWarning(
                    "Error processing test {TestName} (ID: {TestId}): {Reason}",
                    result.TestName, result.TestId, result.Reason);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during automated A/B test management");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("A/B Test Automation Service is stopping");
        await base.StopAsync(cancellationToken);
    }
}
