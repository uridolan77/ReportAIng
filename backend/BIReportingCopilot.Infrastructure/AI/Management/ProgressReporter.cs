using BIReportingCopilot.Core.Interfaces.Messaging;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.AI.Management;

/// <summary>
/// Handles progress reporting for auto-generation processes
/// </summary>
public class ProgressReporter
{
    private readonly IProgressHub? _progressHub;
    private readonly ILogger<ProgressReporter> _logger;

    public ProgressReporter(
        IProgressHub? progressHub,
        ILogger<ProgressReporter> logger)
    {
        _progressHub = progressHub;
        _logger = logger;
    }

    /// <summary>
    /// Create a progress callback that reports to SignalR
    /// </summary>
    public Func<string, string, string?, Task> CreateProgressCallback(string userId, string baseStage)
    {
        return async (stage, message, currentItem) =>
        {
            if (_progressHub != null)
            {
                // Calculate progress based on stage
                var progress = CalculateProgressFromStage(stage);
                _logger.LogInformation("📡 Sending progress update to user {UserId}: {Progress}% - {Message} ({Stage})", 
                    userId, progress, message, stage);
                await _progressHub.SendProgressUpdateAsync(userId, "auto-generation", progress, message);
            }
        };
    }

    /// <summary>
    /// Send direct progress update
    /// </summary>
    public async Task SendProgressUpdateAsync(string userId, int progress, string message)
    {
        if (_progressHub != null)
        {
            _logger.LogInformation("📡 Sending direct progress update to user {UserId}: {Progress}% - {Message}", 
                userId, progress, message);
            await _progressHub.SendProgressUpdateAsync(userId, "auto-generation", progress, message);
        }
    }

    /// <summary>
    /// Calculate progress percentage based on stage
    /// </summary>
    private int CalculateProgressFromStage(string stage)
    {
        return stage switch
        {
            "Schema Loading" => 10,
            "Table Processing" => 30,
            "Column Analysis" => 40,
            "AI Processing" => 50,
            "Content Generation" => 70,
            "Glossary Generation" => 80,
            "Validation" => 90,
            "Completion" => 100,
            _ => 25
        };
    }
}
