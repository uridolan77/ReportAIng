using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.Services;
using BIReportingCopilot.Core.Models.ProcessFlow;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.AI.Core;

/// <summary>
/// Helper service for tracking process flow during query processing
/// </summary>
public class ProcessFlowTracker
{
    private readonly IProcessFlowService _processFlowService;
    private readonly ILogger<ProcessFlowTracker> _logger;
    private string? _currentSessionId;
    private string? _currentUserId;

    public ProcessFlowTracker(IProcessFlowService processFlowService, ILogger<ProcessFlowTracker> logger)
    {
        _processFlowService = processFlowService;
        _logger = logger;
    }

    /// <summary>
    /// Start tracking a new process flow session
    /// </summary>
    public async Task<string> StartSessionAsync(string userId, string query, string queryType = "enhanced", string? conversationId = null, string? messageId = null)
    {
        _currentSessionId = $"session-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..8]}";
        _currentUserId = userId;

        await _processFlowService.StartSessionAsync(_currentSessionId, userId, query, queryType, conversationId, messageId);

        _logger.LogInformation("🚀 [PROCESS-FLOW-TRACKER] Started session {SessionId} for user {UserId}", _currentSessionId, userId);

        return _currentSessionId;
    }

    /// <summary>
    /// Start a process step
    /// </summary>
    public async Task StartStepAsync(string stepId, string? parentStepId = null)
    {
        if (_currentSessionId == null) return;

        var stepDefinition = ProcessFlowStepDefinitions.Steps.GetValueOrDefault(stepId);
        if (stepDefinition == null)
        {
            _logger.LogWarning("⚠️ [PROCESS-FLOW-TRACKER] Unknown step ID: {StepId}", stepId);
            return;
        }

        var stepUpdate = new ProcessFlowStepUpdate
        {
            StepId = stepId,
            ParentStepId = parentStepId ?? stepDefinition.ParentStepId,
            Name = stepDefinition.Name,
            Description = stepDefinition.Description,
            StepOrder = stepDefinition.Order,
            Status = ProcessFlowStatus.Running,
            StartTime = DateTime.UtcNow
        };

        await _processFlowService.AddOrUpdateStepAsync(_currentSessionId, stepUpdate);
        await LogAsync(stepId, ProcessFlowLogLevel.Info, $"Step {stepDefinition.Name} started", source: "ProcessFlowTracker");

        _logger.LogDebug("▶️ [PROCESS-FLOW-TRACKER] Started step {StepId}: {StepName}", stepId, stepDefinition.Name);
    }

    /// <summary>
    /// Complete a process step
    /// </summary>
    public async Task CompleteStepAsync(string stepId, decimal? confidence = null, object? outputData = null)
    {
        if (_currentSessionId == null) return;

        await _processFlowService.UpdateStepStatusAsync(_currentSessionId, stepId, ProcessFlowStatus.Completed);

        if (confidence.HasValue || outputData != null)
        {
            var stepUpdate = new ProcessFlowStepUpdate
            {
                StepId = stepId,
                Status = ProcessFlowStatus.Completed,
                EndTime = DateTime.UtcNow,
                Confidence = confidence,
                OutputData = outputData != null ? JsonSerializer.Serialize(outputData) : null
            };

            await _processFlowService.AddOrUpdateStepAsync(_currentSessionId, stepUpdate);
        }

        var stepDefinition = ProcessFlowStepDefinitions.Steps.GetValueOrDefault(stepId);
        await LogAsync(stepId, ProcessFlowLogLevel.Info, $"Step {stepDefinition?.Name ?? stepId} completed successfully", source: "ProcessFlowTracker");

        _logger.LogDebug("✅ [PROCESS-FLOW-TRACKER] Completed step {StepId}", stepId);
    }

    /// <summary>
    /// Mark a process step as failed
    /// </summary>
    public async Task FailStepAsync(string stepId, string errorMessage, Exception? exception = null)
    {
        if (_currentSessionId == null) return;

        await _processFlowService.UpdateStepStatusAsync(_currentSessionId, stepId, ProcessFlowStatus.Error, errorMessage);

        var details = exception != null ? JsonSerializer.Serialize(new { 
            Message = exception.Message, 
            StackTrace = exception.StackTrace,
            Type = exception.GetType().Name
        }) : null;

        await LogAsync(stepId, ProcessFlowLogLevel.Error, $"Step failed: {errorMessage}", details, "ProcessFlowTracker");

        _logger.LogError("❌ [PROCESS-FLOW-TRACKER] Step {StepId} failed: {ErrorMessage}", stepId, errorMessage);
    }

    /// <summary>
    /// Update step with input data
    /// </summary>
    public async Task SetStepInputAsync(string stepId, object inputData)
    {
        if (_currentSessionId == null) return;

        var stepUpdate = new ProcessFlowStepUpdate
        {
            StepId = stepId,
            InputData = JsonSerializer.Serialize(inputData)
        };

        await _processFlowService.AddOrUpdateStepAsync(_currentSessionId, stepUpdate);
    }

    /// <summary>
    /// Update step with output data
    /// </summary>
    public async Task SetStepOutputAsync(string stepId, object outputData)
    {
        if (_currentSessionId == null) return;

        var stepUpdate = new ProcessFlowStepUpdate
        {
            StepId = stepId,
            OutputData = JsonSerializer.Serialize(outputData)
        };

        await _processFlowService.AddOrUpdateStepAsync(_currentSessionId, stepUpdate);
    }

    /// <summary>
    /// Add a log entry
    /// </summary>
    public async Task LogAsync(string? stepId, string logLevel, string message, string? details = null, string? source = null)
    {
        if (_currentSessionId == null) return;

        await _processFlowService.AddLogAsync(_currentSessionId, stepId, logLevel, message, details, source);
    }

    /// <summary>
    /// Set AI transparency information
    /// </summary>
    public async Task SetTransparencyAsync(string? model = null, decimal? temperature = null, int? promptTokens = null, int? completionTokens = null, decimal? cost = null, decimal? confidence = null, long? processingTimeMs = null)
    {
        if (_currentSessionId == null) return;

        var transparency = new ProcessFlowTransparency
        {
            SessionId = _currentSessionId,
            Model = model,
            Temperature = temperature,
            PromptTokens = promptTokens,
            CompletionTokens = completionTokens,
            TotalTokens = (promptTokens ?? 0) + (completionTokens ?? 0),
            EstimatedCost = cost,
            Confidence = confidence,
            AIProcessingTimeMs = processingTimeMs,
            ApiCallCount = 1
        };

        await _processFlowService.SetTransparencyAsync(_currentSessionId, transparency);

        _logger.LogDebug("🔍 [PROCESS-FLOW-TRACKER] Set transparency data - Model: {Model}, Tokens: {Tokens}, Cost: {Cost}", 
            model, transparency.TotalTokens, cost);
    }

    /// <summary>
    /// Complete the entire process flow session
    /// </summary>
    public async Task CompleteSessionAsync(string status = ProcessFlowStatus.Completed, string? generatedSQL = null, string? executionResult = null, string? errorMessage = null, decimal? overallConfidence = null)
    {
        if (_currentSessionId == null) return;

        var completion = new ProcessFlowSessionCompletion
        {
            Status = status,
            EndTime = DateTime.UtcNow,
            GeneratedSQL = generatedSQL,
            ExecutionResult = executionResult,
            ErrorMessage = errorMessage,
            OverallConfidence = overallConfidence
        };

        // Calculate total duration from session start
        var session = await _processFlowService.GetSessionAsync(_currentSessionId);
        if (session != null)
        {
            completion.TotalDurationMs = (long)(completion.EndTime - session.StartTime).TotalMilliseconds;
        }

        await _processFlowService.CompleteSessionAsync(_currentSessionId, completion);

        _logger.LogInformation("🏁 [PROCESS-FLOW-TRACKER] Session {SessionId} completed with status {Status}", _currentSessionId, status);

        // Reset current session
        _currentSessionId = null;
        _currentUserId = null;
    }

    /// <summary>
    /// Get the current session ID
    /// </summary>
    public string? GetCurrentSessionId() => _currentSessionId;

    /// <summary>
    /// Get the current user ID
    /// </summary>
    public string? GetCurrentUserId() => _currentUserId;

    /// <summary>
    /// Check if tracking is active
    /// </summary>
    public bool IsTracking => _currentSessionId != null;
}

/// <summary>
/// Extension methods for easier process flow tracking
/// </summary>
public static class ProcessFlowTrackerExtensions
{
    /// <summary>
    /// Execute an action with automatic step tracking
    /// </summary>
    public static async Task<T> TrackStepAsync<T>(this ProcessFlowTracker tracker, string stepId, Func<Task<T>> action, string? parentStepId = null)
    {
        await tracker.StartStepAsync(stepId, parentStepId);
        
        try
        {
            var result = await action();
            await tracker.CompleteStepAsync(stepId, outputData: result);
            return result;
        }
        catch (Exception ex)
        {
            await tracker.FailStepAsync(stepId, ex.Message, ex);
            throw;
        }
    }

    /// <summary>
    /// Execute an action with automatic step tracking (void return)
    /// </summary>
    public static async Task TrackStepAsync(this ProcessFlowTracker tracker, string stepId, Func<Task> action, string? parentStepId = null)
    {
        await tracker.StartStepAsync(stepId, parentStepId);
        
        try
        {
            await action();
            await tracker.CompleteStepAsync(stepId);
        }
        catch (Exception ex)
        {
            await tracker.FailStepAsync(stepId, ex.Message, ex);
            throw;
        }
    }

    /// <summary>
    /// Execute an action with automatic step tracking and confidence scoring
    /// </summary>
    public static async Task<T> TrackStepWithConfidenceAsync<T>(this ProcessFlowTracker tracker, string stepId, Func<Task<(T result, decimal confidence)>> action, string? parentStepId = null)
    {
        await tracker.StartStepAsync(stepId, parentStepId);
        
        try
        {
            var (result, confidence) = await action();
            await tracker.CompleteStepAsync(stepId, confidence, result);
            return result;
        }
        catch (Exception ex)
        {
            await tracker.FailStepAsync(stepId, ex.Message, ex);
            throw;
        }
    }
}
