using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.Services;
using BIReportingCopilot.Core.Models.ProcessFlow;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Interfaces;
using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// Service for managing process flow tracking and transparency
/// CONSOLIDATED - Uses only the new ProcessFlow tables for all transparency data
/// </summary>
public class ProcessFlowService : IProcessFlowService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<ProcessFlowService> _logger;

    public ProcessFlowService(BICopilotContext context, ILogger<ProcessFlowService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ProcessFlowSession> StartSessionAsync(string sessionId, string userId, string query, string queryType = "enhanced", string? conversationId = null, string? messageId = null)
    {
        _logger.LogInformation("🔄 [PROCESS-FLOW] Starting session {SessionId} for user {UserId}", sessionId, userId);

        var entity = new ProcessFlowSessionEntity
        {
            SessionId = sessionId,
            UserId = userId,
            Query = query,
            QueryType = queryType,
            Status = ProcessFlowStatus.Running,
            StartTime = DateTime.UtcNow,
            ConversationId = conversationId,
            MessageId = messageId,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        _context.ProcessFlowSessions.Add(entity);
        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ [PROCESS-FLOW] Session {SessionId} started successfully", sessionId);

        return MapToModel(entity);
    }

    public async Task UpdateSessionAsync(string sessionId, ProcessFlowSessionUpdate update)
    {
        var entity = await _context.ProcessFlowSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (entity == null)
        {
            _logger.LogWarning("⚠️ [PROCESS-FLOW] Session {SessionId} not found for update", sessionId);
            return;
        }

        if (update.Status != null) entity.Status = update.Status;
        if (update.EndTime.HasValue) entity.EndTime = update.EndTime.Value;
        if (update.TotalDurationMs.HasValue) entity.TotalDurationMs = update.TotalDurationMs.Value;
        if (update.OverallConfidence.HasValue) entity.OverallConfidence = update.OverallConfidence.Value;
        if (update.GeneratedSQL != null) entity.GeneratedSQL = update.GeneratedSQL;
        if (update.ExecutionResult != null) entity.ExecutionResult = update.ExecutionResult;
        if (update.ErrorMessage != null) entity.ErrorMessage = update.ErrorMessage;
        if (update.Metadata != null) entity.Metadata = update.Metadata;

        entity.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("✅ [PROCESS-FLOW] Session {SessionId} updated successfully", sessionId);
    }

    public async Task CompleteSessionAsync(string sessionId, ProcessFlowSessionCompletion completion)
    {
        var entity = await _context.ProcessFlowSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        if (entity == null)
        {
            _logger.LogWarning("⚠️ [PROCESS-FLOW] Session {SessionId} not found for completion", sessionId);
            return;
        }

        entity.Status = completion.Status;
        entity.EndTime = completion.EndTime;
        entity.TotalDurationMs = completion.TotalDurationMs;
        entity.OverallConfidence = completion.OverallConfidence;
        entity.GeneratedSQL = completion.GeneratedSQL;
        entity.ExecutionResult = completion.ExecutionResult;
        entity.ErrorMessage = completion.ErrorMessage;
        entity.Metadata = completion.Metadata;
        entity.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("🏁 [PROCESS-FLOW] Session {SessionId} completed with status {Status}", sessionId, completion.Status);
    }

    public async Task<ProcessFlowStep> AddOrUpdateStepAsync(string sessionId, ProcessFlowStepUpdate stepUpdate)
    {
        var existingStep = await _context.ProcessFlowSteps
            .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.StepId == stepUpdate.StepId);

        ProcessFlowStepEntity entity;

        if (existingStep != null)
        {
            // Update existing step
            entity = existingStep;
            if (stepUpdate.Status != null) entity.Status = stepUpdate.Status;
            if (stepUpdate.StartTime.HasValue) entity.StartTime = stepUpdate.StartTime.Value;
            if (stepUpdate.EndTime.HasValue) entity.EndTime = stepUpdate.EndTime.Value;
            if (stepUpdate.DurationMs.HasValue) entity.DurationMs = stepUpdate.DurationMs.Value;
            if (stepUpdate.Confidence.HasValue) entity.Confidence = stepUpdate.Confidence.Value;
            if (stepUpdate.InputData != null) entity.InputData = stepUpdate.InputData;
            if (stepUpdate.OutputData != null) entity.OutputData = stepUpdate.OutputData;
            if (stepUpdate.ErrorMessage != null) entity.ErrorMessage = stepUpdate.ErrorMessage;
            if (stepUpdate.Metadata != null) entity.Metadata = stepUpdate.Metadata;
            entity.RetryCount = stepUpdate.RetryCount;
            entity.UpdatedDate = DateTime.UtcNow;

            _logger.LogDebug("🔄 [PROCESS-FLOW] Updated step {StepId} in session {SessionId}", stepUpdate.StepId, sessionId);
        }
        else
        {
            // Create new step
            entity = new ProcessFlowStepEntity
            {
                SessionId = sessionId,
                StepId = stepUpdate.StepId,
                ParentStepId = stepUpdate.ParentStepId,
                Name = stepUpdate.Name,
                Description = stepUpdate.Description,
                StepOrder = stepUpdate.StepOrder,
                Status = stepUpdate.Status,
                StartTime = stepUpdate.StartTime,
                EndTime = stepUpdate.EndTime,
                DurationMs = stepUpdate.DurationMs,
                Confidence = stepUpdate.Confidence,
                InputData = stepUpdate.InputData,
                OutputData = stepUpdate.OutputData,
                ErrorMessage = stepUpdate.ErrorMessage,
                Metadata = stepUpdate.Metadata,
                RetryCount = stepUpdate.RetryCount,
                CreatedBy = "system",
                UpdatedBy = "system"
            };

            _context.ProcessFlowSteps.Add(entity);

            _logger.LogDebug("➕ [PROCESS-FLOW] Added step {StepId} to session {SessionId}", stepUpdate.StepId, sessionId);
        }

        await _context.SaveChangesAsync();

        return MapStepToModel(entity);
    }

    public async Task UpdateStepStatusAsync(string sessionId, string stepId, string status, string? errorMessage = null)
    {
        var entity = await _context.ProcessFlowSteps
            .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.StepId == stepId);

        if (entity == null)
        {
            _logger.LogWarning("⚠️ [PROCESS-FLOW] Step {StepId} not found in session {SessionId}", stepId, sessionId);
            return;
        }

        var previousStatus = entity.Status;
        entity.Status = status;
        entity.ErrorMessage = errorMessage;

        // Set timing information
        if (status == ProcessFlowStatus.Running && entity.StartTime == null)
        {
            entity.StartTime = DateTime.UtcNow;
        }
        else if ((status == ProcessFlowStatus.Completed || status == ProcessFlowStatus.Error) && entity.StartTime.HasValue && entity.EndTime == null)
        {
            entity.EndTime = DateTime.UtcNow;
            entity.DurationMs = (long)(entity.EndTime.Value - entity.StartTime.Value).TotalMilliseconds;
        }

        entity.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogDebug("🔄 [PROCESS-FLOW] Step {StepId} status changed from {PreviousStatus} to {NewStatus}", stepId, previousStatus, status);
    }

    public async Task AddLogAsync(string sessionId, string? stepId, string logLevel, string message, string? details = null, string? source = null)
    {
        var entity = new ProcessFlowLogEntity
        {
            SessionId = sessionId,
            StepId = stepId,
            LogLevel = logLevel,
            Message = message,
            Details = details,
            Source = source,
            Timestamp = DateTime.UtcNow,
            CreatedBy = "system",
            UpdatedBy = "system"
        };

        _context.ProcessFlowLogs.Add(entity);
        await _context.SaveChangesAsync();

        _logger.LogTrace("📝 [PROCESS-FLOW] Added {LogLevel} log to session {SessionId}, step {StepId}: {Message}", logLevel, sessionId, stepId ?? "N/A", message);
    }

    public async Task SetTransparencyAsync(string sessionId, ProcessFlowTransparency transparency)
    {
        var existingEntity = await _context.ProcessFlowTransparency
            .FirstOrDefaultAsync(t => t.SessionId == sessionId);

        ProcessFlowTransparencyEntity entity;

        if (existingEntity != null)
        {
            entity = existingEntity;
        }
        else
        {
            entity = new ProcessFlowTransparencyEntity
            {
                SessionId = sessionId,
                CreatedBy = "system",
                UpdatedBy = "system"
            };
            _context.ProcessFlowTransparency.Add(entity);
        }

        // Update transparency data
        entity.Model = transparency.Model;
        entity.Temperature = transparency.Temperature;
        entity.MaxTokens = transparency.MaxTokens;
        entity.PromptTokens = transparency.PromptTokens;
        entity.CompletionTokens = transparency.CompletionTokens;
        entity.TotalTokens = transparency.TotalTokens;
        entity.EstimatedCost = transparency.EstimatedCost;
        entity.Confidence = transparency.Confidence;
        entity.AIProcessingTimeMs = transparency.AIProcessingTimeMs;
        entity.ApiCallCount = transparency.ApiCallCount;
        entity.PromptDetails = transparency.PromptDetails;
        entity.ResponseAnalysis = transparency.ResponseAnalysis;
        entity.QualityMetrics = transparency.QualityMetrics;
        entity.OptimizationSuggestions = transparency.OptimizationSuggestions;
        entity.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogDebug("🔍 [PROCESS-FLOW] Transparency data set for session {SessionId}", sessionId);
    }

    public async Task<ProcessFlowSession?> GetSessionAsync(string sessionId)
    {
        var entity = await _context.ProcessFlowSessions
            .Include(s => s.Steps.OrderBy(step => step.StepOrder))
            .Include(s => s.Logs.OrderBy(log => log.Timestamp))
            .Include(s => s.Transparency)
            .FirstOrDefaultAsync(s => s.SessionId == sessionId);

        return entity != null ? MapToModel(entity) : null;
    }

    public async Task<IEnumerable<ProcessFlowSession>> GetUserSessionsAsync(string userId, int limit = 50)
    {
        var entities = await _context.ProcessFlowSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.StartTime)
            .Take(limit)
            .Include(s => s.Steps)
            .Include(s => s.Transparency)
            .ToListAsync();

        return entities.Select(MapToModel);
    }

    public async Task<IEnumerable<ProcessFlowSession>> GetConversationSessionsAsync(string conversationId)
    {
        var entities = await _context.ProcessFlowSessions
            .Where(s => s.ConversationId == conversationId)
            .OrderByDescending(s => s.StartTime)
            .Include(s => s.Steps)
            .Include(s => s.Transparency)
            .ToListAsync();

        return entities.Select(MapToModel);
    }

    public async Task<IEnumerable<ProcessFlowStepPerformance>> GetStepPerformanceAsync()
    {
        // This would typically use the view we created in the migration
        var query = @"
            SELECT 
                StepId,
                Name,
                COUNT(*) as ExecutionCount,
                AVG(CAST(DurationMs AS FLOAT)) as AvgDurationMs,
                MIN(DurationMs) as MinDurationMs,
                MAX(DurationMs) as MaxDurationMs,
                AVG(CAST(Confidence AS FLOAT)) as AvgConfidence,
                SUM(CASE WHEN Status = 'completed' THEN 1 ELSE 0 END) as SuccessCount,
                SUM(CASE WHEN Status = 'error' THEN 1 ELSE 0 END) as ErrorCount,
                CAST(SUM(CASE WHEN Status = 'completed' THEN 1 ELSE 0 END) AS FLOAT) / COUNT(*) * 100 as SuccessRate
            FROM ProcessFlowSteps
            WHERE StartTime IS NOT NULL AND EndTime IS NOT NULL
            GROUP BY StepId, Name";

        var results = await _context.Database.SqlQueryRaw<ProcessFlowStepPerformance>(query).ToListAsync();
        return results;
    }

    public async Task<ProcessFlowSummary> GetSessionSummaryAsync(string sessionId)
    {
        // This would use the view we created in the migration
        var query = @"
            SELECT * FROM vw_ProcessFlowSummary WHERE SessionId = {0}";

        var result = await _context.Database.SqlQueryRaw<ProcessFlowSummary>(query, sessionId).FirstOrDefaultAsync();
        return result ?? new ProcessFlowSummary { SessionId = sessionId };
    }

    public async Task CleanupOldDataAsync(TimeSpan retentionPeriod)
    {
        var cutoffDate = DateTime.UtcNow - retentionPeriod;

        var oldSessions = await _context.ProcessFlowSessions
            .Where(s => s.CreatedDate < cutoffDate)
            .ToListAsync();

        if (oldSessions.Any())
        {
            _context.ProcessFlowSessions.RemoveRange(oldSessions);
            await _context.SaveChangesAsync();

            _logger.LogInformation("🧹 [PROCESS-FLOW] Cleaned up {Count} old process flow sessions", oldSessions.Count);
        }
    }

    public async Task<IEnumerable<object>> GetTokenUsageTrendsAsync(DateTime startDate, DateTime endDate, string? userId = null, string? requestType = null)
    {
        var query = _context.ProcessFlowTransparency
            .Include(t => t.Session)
            .Where(t => t.Session.StartTime >= startDate && t.Session.StartTime <= endDate);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(t => t.Session.UserId == userId);

        if (!string.IsNullOrEmpty(requestType))
            query = query.Where(t => t.Session.QueryType == requestType);

        var trends = await query
            .GroupBy(t => t.Session.StartTime.Date)
            .Select(g => new
            {
                Date = g.Key,
                TotalTokens = g.Sum(t => t.TotalTokens ?? 0),
                TotalCost = g.Sum(t => t.EstimatedCost ?? 0),
                RequestCount = g.Count(),
                AverageTokensPerRequest = g.Average(t => t.TotalTokens ?? 0)
            })
            .OrderBy(t => t.Date)
            .ToListAsync();

        return trends;
    }

    public async Task<IEnumerable<object>> GetTopUsersByTokenUsageAsync(DateTime startDate, DateTime endDate, int topCount = 10)
    {
        var topUsers = await _context.ProcessFlowTransparency
            .Include(t => t.Session)
            .Where(t => t.Session.StartTime >= startDate && t.Session.StartTime <= endDate)
            .GroupBy(t => t.Session.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                TotalTokens = g.Sum(t => t.TotalTokens ?? 0),
                TotalCost = g.Sum(t => t.EstimatedCost ?? 0),
                RequestCount = g.Count(),
                AverageTokensPerRequest = g.Average(t => t.TotalTokens ?? 0),
                AverageConfidence = g.Average(t => t.Confidence ?? 0)
            })
            .OrderByDescending(u => u.TotalTokens)
            .Take(topCount)
            .ToListAsync();

        return topUsers;
    }

    public async Task<object> GetSuccessAnalyticsAsync(DateTime startDate, DateTime endDate, string? userId = null)
    {
        var query = _context.ProcessFlowSessions
            .Where(s => s.StartTime >= startDate && s.StartTime <= endDate);

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(s => s.UserId == userId);

        var totalSessions = await query.CountAsync();
        var completedSessions = await query.CountAsync(s => s.Status == "completed");
        var errorSessions = await query.CountAsync(s => s.Status == "error");
        var averageConfidence = await query.AverageAsync(s => s.OverallConfidence ?? 0);
        var averageDuration = await query.AverageAsync(s => s.TotalDurationMs ?? 0);

        return new
        {
            TotalSessions = totalSessions,
            CompletedSessions = completedSessions,
            ErrorSessions = errorSessions,
            SuccessRate = totalSessions > 0 ? (double)completedSessions / totalSessions : 0,
            ErrorRate = totalSessions > 0 ? (double)errorSessions / totalSessions : 0,
            AverageConfidence = averageConfidence,
            AverageDurationMs = averageDuration
        };
    }

    private static ProcessFlowSession MapToModel(ProcessFlowSessionEntity entity)
    {
        return new ProcessFlowSession
        {
            Id = entity.Id.ToString(),
            SessionId = entity.SessionId,
            UserId = entity.UserId,
            Query = entity.Query,
            QueryType = entity.QueryType,
            Status = entity.Status,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            TotalDurationMs = entity.TotalDurationMs,
            OverallConfidence = entity.OverallConfidence,
            GeneratedSQL = entity.GeneratedSQL,
            ExecutionResult = entity.ExecutionResult,
            ErrorMessage = entity.ErrorMessage,
            Metadata = entity.Metadata,
            ConversationId = entity.ConversationId,
            MessageId = entity.MessageId,
            CreatedAt = entity.CreatedDate,
            UpdatedAt = entity.UpdatedDate ?? DateTime.UtcNow,
            Steps = entity.Steps?.Select(MapStepToModel).ToList() ?? new List<ProcessFlowStep>(),
            Logs = entity.Logs?.Select(MapLogToModel).ToList() ?? new List<ProcessFlowLog>(),
            Transparency = entity.Transparency != null ? MapTransparencyToModel(entity.Transparency) : null
        };
    }

    private static ProcessFlowStep MapStepToModel(ProcessFlowStepEntity entity)
    {
        return new ProcessFlowStep
        {
            Id = entity.Id.ToString(),
            SessionId = entity.SessionId,
            StepId = entity.StepId,
            ParentStepId = entity.ParentStepId,
            Name = entity.Name,
            Description = entity.Description,
            StepOrder = entity.StepOrder,
            Status = entity.Status,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime,
            DurationMs = entity.DurationMs,
            Confidence = entity.Confidence,
            InputData = entity.InputData,
            OutputData = entity.OutputData,
            ErrorMessage = entity.ErrorMessage,
            Metadata = entity.Metadata,
            RetryCount = entity.RetryCount,
            CreatedAt = entity.CreatedDate,
            UpdatedAt = entity.UpdatedDate ?? DateTime.UtcNow,
            Logs = entity.Logs?.Select(MapLogToModel).ToList() ?? new List<ProcessFlowLog>(),
            SubSteps = entity.SubSteps?.Select(MapStepToModel).ToList() ?? new List<ProcessFlowStep>()
        };
    }

    private static ProcessFlowLog MapLogToModel(ProcessFlowLogEntity entity)
    {
        return new ProcessFlowLog
        {
            Id = entity.Id.ToString(),
            SessionId = entity.SessionId,
            StepId = entity.StepId,
            LogLevel = entity.LogLevel,
            Message = entity.Message,
            Details = entity.Details,
            Exception = entity.Exception,
            Source = entity.Source,
            Timestamp = entity.Timestamp,
            CreatedAt = entity.CreatedDate
        };
    }

    private static ProcessFlowTransparency MapTransparencyToModel(ProcessFlowTransparencyEntity entity)
    {
        return new ProcessFlowTransparency
        {
            Id = entity.Id.ToString(),
            SessionId = entity.SessionId,
            Model = entity.Model,
            Temperature = entity.Temperature,
            MaxTokens = entity.MaxTokens,
            PromptTokens = entity.PromptTokens,
            CompletionTokens = entity.CompletionTokens,
            TotalTokens = entity.TotalTokens,
            EstimatedCost = entity.EstimatedCost,
            Confidence = entity.Confidence,
            AIProcessingTimeMs = entity.AIProcessingTimeMs,
            ApiCallCount = entity.ApiCallCount,
            PromptDetails = entity.PromptDetails,
            ResponseAnalysis = entity.ResponseAnalysis,
            QualityMetrics = entity.QualityMetrics,
            OptimizationSuggestions = entity.OptimizationSuggestions,
            CreatedAt = entity.CreatedDate,
            UpdatedAt = entity.UpdatedDate ?? DateTime.UtcNow
        };
    }
}
