using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Core.Interfaces;

namespace BIReportingCopilot.Infrastructure.Jobs;

public interface ICleanupJob
{
    Task PerformCleanup();
}

public class CleanupJob : ICleanupJob
{
    private readonly BICopilotContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CleanupJob> _logger;
    private readonly IConfiguration _configuration;

    public CleanupJob(
        BICopilotContext context,
        ICacheService cacheService,
        ILogger<CleanupJob> logger,
        IConfiguration configuration)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
        _configuration = configuration;
    }

    [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
    public async Task PerformCleanup()
    {
        _logger.LogInformation("Starting cleanup job");

        // Clean up old query history
        await CleanupQueryHistory();

        // Clean up expired cache entries
        await CleanupExpiredCache();

        // Clean up old audit logs
        await CleanupAuditLogs();

        // Clean up inactive sessions
        await CleanupInactiveSessions();

        _logger.LogInformation("Cleanup job completed");
    }

    private async Task CleanupQueryHistory()
    {
        var retentionDays = _configuration.GetValue<int>("QueryHistory:RetentionDays", 90);
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

        var deletedCount = await _context.QueryHistory
            .Where(q => q.QueryTimestamp < cutoffDate)
            .ExecuteDeleteAsync();

        _logger.LogInformation("Deleted {Count} old query history records", deletedCount);
    }

    private async Task CleanupExpiredCache()
    {
        var expiredCacheEntries = await _context.QueryCache
            .Where(c => c.ExpiryTimestamp < DateTime.UtcNow)
            .Select(c => c.Id)
            .ToListAsync();

        if (expiredCacheEntries.Any())
        {
            await _context.QueryCache
                .Where(c => expiredCacheEntries.Contains(c.Id))
                .ExecuteDeleteAsync();

            _logger.LogInformation("Deleted {Count} expired cache entries", expiredCacheEntries.Count);
        }
    }

    private async Task CleanupAuditLogs()
    {
        var retentionDays = _configuration.GetValue<int>("AuditLog:RetentionDays", 365);
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

        var deletedCount = await _context.AuditLog
            .Where(a => a.Timestamp < cutoffDate && a.Severity != "Critical")
            .ExecuteDeleteAsync();

        _logger.LogInformation("Deleted {Count} old audit log entries", deletedCount);
    }

    private async Task CleanupInactiveSessions()
    {
        var sessionTimeout = TimeSpan.FromHours(24);
        var cutoffTime = DateTime.UtcNow.Subtract(sessionTimeout);

        try
        {
            // Clean up inactive user sessions
            var inactiveSessions = await _context.UserSessions
                .Where(s => s.LastActivity < cutoffTime && s.IsActive)
                .ToListAsync();

            foreach (var session in inactiveSessions)
            {
                session.IsActive = false;
            }

            if (inactiveSessions.Any())
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Marked {Count} sessions as inactive", inactiveSessions.Count);
            }

            // Clean up old refresh tokens
            var expiredTokens = await _context.RefreshTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow || t.IsRevoked)
                .ToListAsync();

            if (expiredTokens.Any())
            {
                _context.RefreshTokens.RemoveRange(expiredTokens);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Deleted {Count} expired refresh tokens", expiredTokens.Count);
            }

            // Clean up very old inactive sessions (older than 30 days)
            var oldSessionCutoff = DateTime.UtcNow.AddDays(-30);
            var deletedSessionCount = await _context.UserSessions
                .Where(s => !s.IsActive && s.LastActivity < oldSessionCutoff)
                .ExecuteDeleteAsync();

            if (deletedSessionCount > 0)
            {
                _logger.LogInformation("Deleted {Count} old inactive sessions", deletedSessionCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during session cleanup");
        }
    }
}
