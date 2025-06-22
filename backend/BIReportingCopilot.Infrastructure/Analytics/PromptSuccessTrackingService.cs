using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Analytics;

/// <summary>
/// Service for tracking end-to-end prompt success metrics and user feedback
/// </summary>
public class PromptSuccessTrackingService : IPromptSuccessTrackingService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<PromptSuccessTrackingService> _logger;

    public PromptSuccessTrackingService(BICopilotContext context, ILogger<PromptSuccessTrackingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<long> TrackPromptSessionAsync(PromptSuccessTrackingRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var trackingEntity = new PromptSuccessTrackingEntity
            {
                SessionId = request.SessionId,
                UserId = request.UserId,
                UserQuestion = request.UserQuestion,
                GeneratedPrompt = request.GeneratedPrompt,
                TemplateUsed = request.TemplateUsed,
                IntentClassified = request.IntentClassified,
                DomainClassified = request.DomainClassified,
                TablesRetrieved = request.TablesRetrieved,
                GeneratedSQL = request.GeneratedSQL,
                ProcessingTimeMs = request.ProcessingTimeMs,
                ConfidenceScore = request.ConfidenceScore,
                CreatedDate = DateTime.UtcNow
            };

            _context.PromptSuccessTracking.Add(trackingEntity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("✅ Tracked prompt session: Id={Id}, SessionId={SessionId}, User={UserId}, Intent={Intent}", 
                trackingEntity.Id, request.SessionId, request.UserId, request.IntentClassified);

            return trackingEntity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error tracking prompt session for user {UserId}, session {SessionId}", 
                request.UserId, request.SessionId);
            throw;
        }
    }

    public async Task UpdateSQLExecutionResultAsync(long sessionId, bool success, string? errorMessage = null, 
        int? executionTimeMs = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _context.PromptSuccessTracking
                .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

            if (session == null)
            {
                _logger.LogWarning("⚠️ Prompt success tracking session not found: {SessionId}", sessionId);
                return;
            }

            session.UpdateSQLExecution(success, errorMessage, executionTimeMs);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("✅ Updated SQL execution result for session: {SessionId}, Success={Success}", 
                sessionId, success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating SQL execution result for session: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task RecordUserFeedbackAsync(long sessionId, int rating, string? comments = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _context.PromptSuccessTracking
                .FirstOrDefaultAsync(s => s.Id == sessionId, cancellationToken);

            if (session == null)
            {
                _logger.LogWarning("⚠️ Prompt success tracking session not found: {SessionId}", sessionId);
                return;
            }

            session.UpdateUserFeedback(rating, comments);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("✅ Recorded user feedback for session: {SessionId}, Rating={Rating}", 
                sessionId, rating);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error recording user feedback for session: {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<IEnumerable<PromptSuccessTrackingRecord>> GetUserSessionsAsync(string userId,
        DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.PromptSuccessTracking
            .Where(s => s.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(s => s.CreatedDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(s => s.CreatedDate <= endDate.Value);

        var entities = await query
            .OrderByDescending(s => s.CreatedDate)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToPromptSuccessTrackingRecord);
    }

    public async Task<PromptSuccessTrackingRecord?> GetSessionAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PromptSuccessTracking
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);

        return entity != null ? MapToPromptSuccessTrackingRecord(entity) : null;
    }

    public async Task<PromptSuccessAnalytics> GetSuccessAnalyticsAsync(DateTime startDate, DateTime endDate, 
        string? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.PromptSuccessTracking
            .Where(s => s.CreatedDate >= startDate && s.CreatedDate <= endDate);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(s => s.UserId == userId);

        var sessions = await query.ToListAsync(cancellationToken);

        var totalSessions = sessions.Count;
        var successfulSessions = sessions.Count(s => s.IsSuccessfulSession());
        var sessionsWithFeedback = sessions.Count(s => s.UserFeedbackRating.HasValue);

        return new PromptSuccessAnalytics
        {
            TotalSessions = totalSessions,
            SuccessfulSessions = successfulSessions,
            OverallSuccessRate = totalSessions > 0 ? (decimal)successfulSessions / totalSessions : 0,
            AverageConfidenceScore = sessions.Any() ? sessions.Average(s => s.ConfidenceScore) : 0,
            AverageProcessingTimeMs = sessions.Any() ? (int)sessions.Average(s => s.ProcessingTimeMs) : 0,
            SessionsWithUserFeedback = sessionsWithFeedback,
            AverageUserRating = sessionsWithFeedback > 0 ?
                (decimal)sessions.Where(s => s.UserFeedbackRating.HasValue).Average(s => s.UserFeedbackRating!.Value) : 0,
            IntentBreakdown = sessions.GroupBy(s => s.IntentClassified)
                .ToDictionary(g => g.Key, g => g.Count()),
            DomainBreakdown = sessions.GroupBy(s => s.DomainClassified)
                .ToDictionary(g => g.Key, g => g.Count()),
            TemplateSuccessRates = sessions.Where(s => !string.IsNullOrEmpty(s.TemplateUsed))
                .GroupBy(s => s.TemplateUsed!)
                .ToDictionary(g => g.Key, g => g.Count(s => s.IsSuccessfulSession()) / (decimal)g.Count()),
            StartDate = startDate,
            EndDate = endDate
        };
    }

    public async Task<IEnumerable<TemplatePerformanceMetrics>> GetTemplatePerformanceAsync(DateTime startDate, DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        var templateMetrics = await _context.PromptSuccessTracking
            .Where(s => s.CreatedDate >= startDate && s.CreatedDate <= endDate && 
                       !string.IsNullOrEmpty(s.TemplateUsed))
            .GroupBy(s => s.TemplateUsed)
            .Select(g => new TemplatePerformanceMetrics
            {
                TemplateName = g.Key!,
                TotalUsage = g.Count(),
                SuccessfulUsage = g.Count(s => s.IsSuccessfulSession()),
                SuccessRate = g.Count() > 0 ? (decimal)g.Count(s => s.IsSuccessfulSession()) / g.Count() : 0,
                AverageConfidenceScore = g.Average(s => s.ConfidenceScore),
                AverageProcessingTimeMs = (int)g.Average(s => s.ProcessingTimeMs),
                AverageUserRating = g.Where(s => s.UserFeedbackRating.HasValue).Any() ?
                    (decimal?)g.Where(s => s.UserFeedbackRating.HasValue).Average(s => s.UserFeedbackRating!.Value) : null
            })
            .OrderByDescending(t => t.TotalUsage)
            .ToListAsync(cancellationToken);

        return templateMetrics ?? new List<TemplatePerformanceMetrics>();
    }

    public async Task<IEnumerable<IntentPerformanceMetrics>> GetIntentPerformanceAsync(DateTime startDate, DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        var intentMetrics = await _context.PromptSuccessTracking
            .Where(s => s.CreatedDate >= startDate && s.CreatedDate <= endDate)
            .GroupBy(s => s.IntentClassified)
            .Select(g => new IntentPerformanceMetrics
            {
                IntentType = g.Key,
                TotalSessions = g.Count(),
                SuccessfulSessions = g.Count(s => s.IsSuccessfulSession()),
                SuccessRate = g.Count() > 0 ? (decimal)g.Count(s => s.IsSuccessfulSession()) / g.Count() : 0,
                AverageConfidenceScore = g.Average(s => s.ConfidenceScore),
                AverageProcessingTimeMs = (int)g.Average(s => s.ProcessingTimeMs)
            })
            .OrderByDescending(i => i.TotalSessions)
            .ToListAsync(cancellationToken);

        return intentMetrics;
    }

    public async Task<IEnumerable<DomainPerformanceMetrics>> GetDomainPerformanceAsync(DateTime startDate, DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        var domainMetrics = await _context.PromptSuccessTracking
            .Where(s => s.CreatedDate >= startDate && s.CreatedDate <= endDate)
            .GroupBy(s => s.DomainClassified)
            .Select(g => new DomainPerformanceMetrics
            {
                DomainName = g.Key,
                TotalSessions = g.Count(),
                SuccessfulSessions = g.Count(s => s.IsSuccessfulSession()),
                SuccessRate = g.Count() > 0 ? (decimal)g.Count(s => s.IsSuccessfulSession()) / g.Count() : 0,
                AverageConfidenceScore = g.Average(s => s.ConfidenceScore),
                AverageProcessingTimeMs = (int)g.Average(s => s.ProcessingTimeMs)
            })
            .OrderByDescending(d => d.TotalSessions)
            .ToListAsync(cancellationToken);

        return domainMetrics;
    }

    public async Task<IEnumerable<SuccessTrendPoint>> GetSuccessTrendsAsync(DateTime startDate, DateTime endDate, 
        string groupBy = "day", CancellationToken cancellationToken = default)
    {
        var sessions = await _context.PromptSuccessTracking
            .Where(s => s.CreatedDate >= startDate && s.CreatedDate <= endDate)
            .ToListAsync(cancellationToken);

        var trends = sessions
            .GroupBy(s => groupBy.ToLower() switch
            {
                "hour" => new DateTime(s.CreatedDate.Year, s.CreatedDate.Month, s.CreatedDate.Day, s.CreatedDate.Hour, 0, 0),
                "day" => s.CreatedDate.Date,
                "week" => s.CreatedDate.Date.AddDays(-(int)s.CreatedDate.DayOfWeek),
                "month" => new DateTime(s.CreatedDate.Year, s.CreatedDate.Month, 1),
                _ => s.CreatedDate.Date
            })
            .Select(g => new SuccessTrendPoint
            {
                Period = g.Key,
                TotalSessions = g.Count(),
                SuccessfulSessions = g.Count(s => s.IsSuccessfulSession()),
                SuccessRate = g.Count() > 0 ? (decimal)g.Count(s => s.IsSuccessfulSession()) / g.Count() : 0,
                AverageConfidenceScore = g.Average(s => s.ConfidenceScore),
                AverageProcessingTimeMs = (int)g.Average(s => s.ProcessingTimeMs)
            })
            .OrderBy(t => t.Period)
            .ToList();

        return trends;
    }

    /// <summary>
    /// Map entity to Core model
    /// </summary>
    private static PromptSuccessTrackingRecord MapToPromptSuccessTrackingRecord(PromptSuccessTrackingEntity entity)
    {
        return new PromptSuccessTrackingRecord
        {
            Id = entity.Id,
            SessionId = entity.SessionId,
            UserId = entity.UserId,
            UserQuestion = entity.UserQuestion,
            GeneratedPrompt = entity.GeneratedPrompt,
            TemplateUsed = entity.TemplateUsed,
            IntentClassified = entity.IntentClassified,
            DomainClassified = entity.DomainClassified,
            TablesRetrieved = entity.TablesRetrieved,
            GeneratedSQL = entity.GeneratedSQL,
            SQLExecutionSuccess = entity.SQLExecutionSuccess ?? false,
            SQLExecutionError = entity.SQLExecutionError,
            SQLExecutionTimeMs = null, // This field doesn't exist in the entity
            ProcessingTimeMs = entity.ProcessingTimeMs,
            ConfidenceScore = entity.ConfidenceScore,
            UserFeedbackRating = entity.UserFeedbackRating,
            UserFeedbackComments = entity.UserFeedbackComments,
            CreatedDate = entity.CreatedDate
        };
    }
}
