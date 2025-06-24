using BIReportingCopilot.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.API.Hubs;

/// <summary>
/// No-op implementation of query progress notifier for testing
/// </summary>
public class NoOpQueryProgressNotifier : IQueryProgressNotifier
{
    private readonly ILogger<NoOpQueryProgressNotifier> _logger;

    public NoOpQueryProgressNotifier(ILogger<NoOpQueryProgressNotifier> logger)
    {
        _logger = logger;
    }

    public Task NotifyProcessingStageAsync(string userId, string queryId, string stage, string message, int progress, object? details = null)
    {
        _logger.LogInformation("📡 [NO-OP] Progress notification - User: {UserId}, QueryId: {QueryId}, Stage: {Stage}, Progress: {Progress}%",
            userId, queryId, stage, progress);
        return Task.CompletedTask;
    }

    public Task NotifyProgressAsync(string userId, double progress, string message, string stage)
    {
        _logger.LogInformation("📡 [NO-OP] Progress notification - User: {UserId}, Stage: {Stage}, Progress: {Progress}%",
            userId, stage, progress);
        return Task.CompletedTask;
    }
}
