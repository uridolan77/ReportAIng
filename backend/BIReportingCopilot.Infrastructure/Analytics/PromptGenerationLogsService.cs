using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Analytics;

/// <summary>
/// Service for logging and analyzing prompt generation activities
/// </summary>
public class PromptGenerationLogsService : IPromptGenerationLogsService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<PromptGenerationLogsService> _logger;

    public PromptGenerationLogsService(BICopilotContext context, ILogger<PromptGenerationLogsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<long> LogPromptGenerationAsync(PromptGenerationLogRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var logEntry = new PromptGenerationLogsEntity
            {
                UserId = request.UserId,
                UserQuestion = request.UserQuestion,
                GeneratedPrompt = request.GeneratedPrompt,
                PromptLength = request.GeneratedPrompt.Length,
                IntentType = request.IntentType,
                Domain = request.Domain,
                ConfidenceScore = request.ConfidenceScore,
                TablesUsed = request.TablesUsed,
                GenerationTimeMs = request.GenerationTimeMs,
                TemplateUsed = request.TemplateUsed,
                WasSuccessful = request.WasSuccessful,
                ErrorMessage = request.ErrorMessage,
                ExtractedEntities = request.ExtractedEntities,
                TimeContext = request.TimeContext,
                TokensUsed = request.TokensUsed,
                CostEstimate = request.CostEstimate,
                ModelUsed = request.ModelUsed,
                PromptTemplateId = request.PromptTemplateId,
                SessionId = request.SessionId,
                RequestId = request.RequestId,
                Timestamp = DateTime.UtcNow
            };

            _context.PromptGenerationLogs.Add(logEntry);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("✅ Logged prompt generation: Id={Id}, User={UserId}, Intent={IntentType}, Success={Success}", 
                logEntry.Id, request.UserId, request.IntentType, request.WasSuccessful);

            return logEntry.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error logging prompt generation for user {UserId}", request.UserId);
            throw;
        }
    }

    public async Task UpdateExecutionResultsAsync(long logId, bool sqlGenerated, bool queryExecuted, 
        string? errorMessage = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var logEntry = await _context.PromptGenerationLogs
                .FirstOrDefaultAsync(l => l.Id == logId, cancellationToken);

            if (logEntry == null)
            {
                _logger.LogWarning("⚠️ Prompt generation log not found: {LogId}", logId);
                return;
            }

            logEntry.UpdateExecutionResults(sqlGenerated, queryExecuted, errorMessage);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("✅ Updated execution results for prompt log: {LogId}, SQL={SqlGenerated}, Query={QueryExecuted}", 
                logId, sqlGenerated, queryExecuted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating execution results for prompt log: {LogId}", logId);
            throw;
        }
    }

    public async Task UpdateUserFeedbackAsync(long logId, decimal rating, string? feedback = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var logEntry = await _context.PromptGenerationLogs
                .FirstOrDefaultAsync(l => l.Id == logId, cancellationToken);

            if (logEntry == null)
            {
                _logger.LogWarning("⚠️ Prompt generation log not found: {LogId}", logId);
                return;
            }

            logEntry.UserRating = rating;
            logEntry.UserFeedback = feedback;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("✅ Updated user feedback for prompt log: {LogId}, Rating={Rating}", logId, rating);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating user feedback for prompt log: {LogId}", logId);
            throw;
        }
    }

    public async Task<IEnumerable<PromptGenerationLogRecord>> GetUserLogsAsync(string userId,
        DateTime? startDate = null, DateTime? endDate = null, int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.PromptGenerationLogs
            .Where(l => l.UserId == userId);

        if (startDate.HasValue)
            query = query.Where(l => l.Timestamp >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(l => l.Timestamp <= endDate.Value);

        query = query.OrderByDescending(l => l.Timestamp);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        var entities = await query.ToListAsync(cancellationToken);
        return entities.Select(MapToPromptGenerationLogRecord);
    }

    public async Task<IEnumerable<PromptGenerationLogRecord>> GetSessionLogsAsync(string sessionId,
        CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptGenerationLogs
            .Where(l => l.SessionId == sessionId)
            .OrderBy(l => l.Timestamp)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToPromptGenerationLogRecord);
    }

    public async Task<PromptGenerationAnalytics> GetAnalyticsAsync(DateTime startDate, DateTime endDate, 
        string? userId = null, string? intentType = null, string? domain = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.PromptGenerationLogs
            .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(l => l.UserId == userId);

        if (!string.IsNullOrEmpty(intentType))
            query = query.Where(l => l.IntentType == intentType);

        if (!string.IsNullOrEmpty(domain))
            query = query.Where(l => l.Domain == domain);

        var logs = await query.ToListAsync(cancellationToken);

        var totalPrompts = logs.Count;
        var successfulPrompts = logs.Count(l => l.WasSuccessful);
        var failedPrompts = totalPrompts - successfulPrompts;

        return new PromptGenerationAnalytics
        {
            TotalPrompts = totalPrompts,
            SuccessfulPrompts = successfulPrompts,
            FailedPrompts = failedPrompts,
            SuccessRate = totalPrompts > 0 ? (decimal)successfulPrompts / totalPrompts : 0,
            AverageConfidenceScore = logs.Any() ? logs.Average(l => l.ConfidenceScore) : 0,
            AverageGenerationTimeMs = logs.Any() ? (int)logs.Average(l => l.GenerationTimeMs) : 0,
            AverageTablesUsed = logs.Any() ? (int)logs.Average(l => l.TablesUsed) : 0,
            TotalTokensUsed = logs.Where(l => l.TokensUsed.HasValue).Sum(l => l.TokensUsed),
            TotalCost = logs.Where(l => l.CostEstimate.HasValue).Sum(l => l.CostEstimate),
            IntentTypeBreakdown = logs.GroupBy(l => l.IntentType)
                .ToDictionary(g => g.Key, g => g.Count()),
            DomainBreakdown = logs.GroupBy(l => l.Domain)
                .ToDictionary(g => g.Key, g => g.Count()),
            TemplateUsageBreakdown = logs.Where(l => !string.IsNullOrEmpty(l.TemplateUsed))
                .GroupBy(l => l.TemplateUsed!)
                .ToDictionary(g => g.Key, g => g.Count()),
            StartDate = startDate,
            EndDate = endDate
        };
    }

    public async Task<IEnumerable<TemplateSuccessRate>> GetTemplateSuccessRatesAsync(DateTime startDate, DateTime endDate, 
        CancellationToken cancellationToken = default)
    {
        var templateStats = await _context.PromptGenerationLogs
            .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate && 
                       !string.IsNullOrEmpty(l.TemplateUsed))
            .GroupBy(l => l.TemplateUsed)
            .Select(g => new TemplateSuccessRate
            {
                TemplateName = g.Key!,
                TotalUsage = g.Count(),
                SuccessfulUsage = g.Count(l => l.WasSuccessful),
                SuccessRate = g.Count() > 0 ? (decimal)g.Count(l => l.WasSuccessful) / g.Count() : 0,
                AverageConfidenceScore = g.Average(l => l.ConfidenceScore),
                AverageGenerationTimeMs = (int)g.Average(l => l.GenerationTimeMs)
            })
            .OrderByDescending(t => t.TotalUsage)
            .ToListAsync(cancellationToken);

        return templateStats;
    }

    public async Task<PromptGenerationPerformanceMetrics> GetPerformanceMetricsAsync(DateTime startDate, DateTime endDate, 
        string? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.PromptGenerationLogs
            .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(l => l.UserId == userId);

        var logs = await query.ToListAsync(cancellationToken);

        if (!logs.Any())
        {
            return new PromptGenerationPerformanceMetrics();
        }

        var generationTimes = logs.Select(l => l.GenerationTimeMs).OrderBy(t => t).ToList();
        var promptLengths = logs.Select(l => l.PromptLength).ToList();
        var tokensUsed = logs.Where(l => l.TokensUsed.HasValue).Select(l => l.TokensUsed!.Value).ToList();
        var costs = logs.Where(l => l.CostEstimate.HasValue).Select(l => l.CostEstimate!.Value).ToList();

        return new PromptGenerationPerformanceMetrics
        {
            AverageGenerationTimeMs = (int)generationTimes.Average(),
            MedianGenerationTimeMs = generationTimes[generationTimes.Count / 2],
            P95GenerationTimeMs = generationTimes[(int)(generationTimes.Count * 0.95)],
            P99GenerationTimeMs = generationTimes[(int)(generationTimes.Count * 0.99)],
            AveragePromptLength = promptLengths.Any() ? (decimal)promptLengths.Average() : 0,
            AverageTokensUsed = tokensUsed.Any() ? (decimal)tokensUsed.Average() : 0,
            AverageCost = costs.Any() ? costs.Average() : 0,
            PerformanceBuckets = new Dictionary<string, int>
            {
                ["Fast (<1s)"] = generationTimes.Count(t => t < 1000),
                ["Normal (1-3s)"] = generationTimes.Count(t => t >= 1000 && t < 3000),
                ["Slow (3-10s)"] = generationTimes.Count(t => t >= 3000 && t < 10000),
                ["Very Slow (>10s)"] = generationTimes.Count(t => t >= 10000)
            }
        };
    }

    public async Task<IEnumerable<ErrorPattern>> GetErrorPatternsAsync(DateTime startDate, DateTime endDate, 
        int topCount = 10, CancellationToken cancellationToken = default)
    {
        var errorLogs = await _context.PromptGenerationLogs
            .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate && 
                       !l.WasSuccessful && !string.IsNullOrEmpty(l.ErrorMessage))
            .ToListAsync(cancellationToken);

        var totalErrors = errorLogs.Count;
        if (totalErrors == 0) return new List<ErrorPattern>();

        var errorPatterns = errorLogs
            .GroupBy(l => l.ErrorMessage!)
            .Select(g => new ErrorPattern
            {
                ErrorType = DetermineErrorType(g.Key),
                ErrorMessage = g.Key,
                Frequency = g.Count(),
                Percentage = (decimal)g.Count() / totalErrors * 100,
                CommonIntentType = g.GroupBy(l => l.IntentType)
                    .OrderByDescending(ig => ig.Count())
                    .FirstOrDefault()?.Key,
                CommonDomain = g.GroupBy(l => l.Domain)
                    .OrderByDescending(dg => dg.Count())
                    .FirstOrDefault()?.Key
            })
            .OrderByDescending(e => e.Frequency)
            .Take(topCount)
            .ToList();

        return errorPatterns;
    }

    public async Task<IEnumerable<PromptGenerationTrend>> GetTrendsAsync(DateTime startDate, DateTime endDate, 
        string groupBy = "day", string? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.PromptGenerationLogs
            .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(l => l.UserId == userId);

        var logs = await query.ToListAsync(cancellationToken);

        var trends = logs
            .GroupBy(l => groupBy.ToLower() switch
            {
                "hour" => new DateTime(l.Timestamp.Year, l.Timestamp.Month, l.Timestamp.Day, l.Timestamp.Hour, 0, 0),
                "day" => l.Timestamp.Date,
                "week" => l.Timestamp.Date.AddDays(-(int)l.Timestamp.DayOfWeek),
                "month" => new DateTime(l.Timestamp.Year, l.Timestamp.Month, 1),
                _ => l.Timestamp.Date
            })
            .Select(g => new PromptGenerationTrend
            {
                Period = g.Key,
                TotalPrompts = g.Count(),
                SuccessfulPrompts = g.Count(l => l.WasSuccessful),
                SuccessRate = g.Count() > 0 ? (decimal)g.Count(l => l.WasSuccessful) / g.Count() : 0,
                AverageConfidenceScore = g.Average(l => l.ConfidenceScore),
                AverageGenerationTimeMs = (int)g.Average(l => l.GenerationTimeMs)
            })
            .OrderBy(t => t.Period)
            .ToList();

        return trends;
    }

    private static string DetermineErrorType(string errorMessage)
    {
        var lowerError = errorMessage.ToLower();
        
        if (lowerError.Contains("timeout") || lowerError.Contains("time out"))
            return "Timeout";
        if (lowerError.Contains("connection") || lowerError.Contains("network"))
            return "Connection";
        if (lowerError.Contains("authentication") || lowerError.Contains("unauthorized"))
            return "Authentication";
        if (lowerError.Contains("rate limit") || lowerError.Contains("quota"))
            return "Rate Limit";
        if (lowerError.Contains("validation") || lowerError.Contains("invalid"))
            return "Validation";
        if (lowerError.Contains("schema") || lowerError.Contains("table") || lowerError.Contains("column"))
            return "Schema";

        return "Other";
    }

    /// <summary>
    /// Map entity to Core model
    /// </summary>
    private static PromptGenerationLogRecord MapToPromptGenerationLogRecord(PromptGenerationLogsEntity entity)
    {
        return new PromptGenerationLogRecord
        {
            Id = entity.Id,
            UserId = entity.UserId,
            UserQuestion = entity.UserQuestion,
            GeneratedPrompt = entity.GeneratedPrompt,
            PromptLength = entity.PromptLength,
            IntentType = entity.IntentType,
            Domain = entity.Domain,
            ConfidenceScore = entity.ConfidenceScore,
            TablesUsed = entity.TablesUsed,
            GenerationTimeMs = entity.GenerationTimeMs,
            TemplateUsed = entity.TemplateUsed,
            WasSuccessful = entity.WasSuccessful,
            ErrorMessage = entity.ErrorMessage,
            SQLGenerated = entity.SqlGeneratedSuccessfully ?? false,
            QueryExecuted = entity.QueryExecutedSuccessfully ?? false,
            ExtractedEntities = entity.ExtractedEntities,
            TimeContext = entity.TimeContext,
            TokensUsed = entity.TokensUsed,
            CostEstimate = entity.CostEstimate,
            ModelUsed = entity.ModelUsed,
            PromptTemplateId = entity.PromptTemplateId,
            UserRating = entity.UserRating,
            UserFeedback = entity.UserFeedback,
            SessionId = entity.SessionId,
            RequestId = entity.RequestId,
            Timestamp = entity.Timestamp
        };
    }
}
