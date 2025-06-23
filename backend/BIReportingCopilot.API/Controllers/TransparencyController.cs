using Microsoft.AspNetCore.Mvc;
using BIReportingCopilot.Core.Interfaces.Services;
using BIReportingCopilot.Core.Models.ProcessFlow;
using BIReportingCopilot.Infrastructure.AI.Core;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.Interfaces.Data;
using BIReportingCopilot.Core.DTOs;
// using BIReportingCopilot.Core.Interfaces.Audit; // REMOVED - legacy audit interfaces

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Controller for AI transparency and explainability features - CONSOLIDATED ProcessFlow Integration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TransparencyController : ControllerBase
{
    private readonly IProcessFlowService _processFlowService;
    private readonly ProcessFlowTracker _processFlowTracker;
    private readonly IBusinessContextAnalyzer _contextAnalyzer;
    private readonly IBusinessMetadataRetrievalService _metadataService;
    private readonly IContextualPromptBuilder _promptBuilder;
    private readonly IAuditService _auditService;
    private readonly ILogger<TransparencyController> _logger;

    public TransparencyController(
        IProcessFlowService processFlowService,
        ProcessFlowTracker processFlowTracker,
        IBusinessContextAnalyzer contextAnalyzer,
        IBusinessMetadataRetrievalService metadataService,
        IContextualPromptBuilder promptBuilder,
        IAuditService auditService,
        ILogger<TransparencyController> logger)
    {
        _processFlowService = processFlowService;
        _processFlowTracker = processFlowTracker;
        _contextAnalyzer = contextAnalyzer;
        _metadataService = metadataService;
        _promptBuilder = promptBuilder;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Get transparency trace for a specific session ID - CONSOLIDATED ProcessFlow Integration
    /// </summary>
    [HttpGet("trace/{sessionId}")]
    public async Task<ActionResult<ProcessFlowSession>> GetTransparencyTrace(string sessionId)
    {
        try
        {
            _logger.LogInformation("🔍 [PROCESS-FLOW] Retrieving transparency trace for session: {SessionId}", sessionId);

            var session = await _processFlowService.GetSessionAsync(sessionId);
            
            if (session == null)
            {
                return NotFound(new { error = "Process flow session not found", sessionId = sessionId });
            }

            _logger.LogInformation("✅ [PROCESS-FLOW] Retrieved session with {StepCount} steps and {LogCount} logs", 
                session.Steps.Count, session.Logs.Count);

            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [PROCESS-FLOW] Error retrieving transparency trace {SessionId}", sessionId);
            return StatusCode(500, new { error = "Failed to retrieve transparency trace", details = ex.Message });
        }
    }

    /// <summary>
    /// Analyze prompt construction for a user query - CONSOLIDATED ProcessFlow Integration
    /// </summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<ProcessFlowSession>> AnalyzePromptConstruction(
        [FromBody] AnalyzePromptRequest request)
    {
        try
        {
            _logger.LogInformation("🔍 [PROCESS-FLOW] Analyzing prompt construction for query: {Query}", request.UserQuery);

            // Start a new process flow session for analysis
            var sessionId = await _processFlowTracker.StartSessionAsync(
                request.UserId ?? "anonymous", 
                request.UserQuery, 
                "analysis");

            // Step 1: Analyze business context
            var profile = await _processFlowTracker.TrackStepWithConfidenceAsync(
                ProcessFlowSteps.SemanticAnalysis,
                async () => {
                    var result = await _contextAnalyzer.AnalyzeUserQuestionAsync(request.UserQuery, request.UserId);
                    return (result, (decimal)result.ConfidenceScore);
                });

            // Step 2: Get relevant metadata
            var schema = await _processFlowTracker.TrackStepAsync(
                ProcessFlowSteps.SchemaRetrieval,
                async () => await _metadataService.GetRelevantBusinessMetadataAsync(profile, 10));

            // Step 3: Build prompt
            var prompt = await _processFlowTracker.TrackStepAsync(
                ProcessFlowSteps.PromptBuilding,
                async () => await _promptBuilder.BuildBusinessAwarePromptAsync(request.UserQuery, profile, schema));

            // Complete the analysis session
            await _processFlowTracker.CompleteSessionAsync(
                ProcessFlowStatus.Completed,
                generatedSQL: null,
                executionResult: "Prompt analysis completed successfully",
                overallConfidence: (decimal)profile.ConfidenceScore);

            // Get the completed session
            var session = await _processFlowService.GetSessionAsync(sessionId);

            // Log the analysis for audit
            await _auditService.LogAsync(
                "PromptAnalysis", 
                request.UserId ?? "anonymous", 
                "ProcessFlowSession", 
                sessionId,
                new { userQuery = request.UserQuery, confidence = profile.ConfidenceScore, stepCount = session?.Steps.Count ?? 0 });

            _logger.LogInformation("✅ [PROCESS-FLOW] Prompt analysis completed for session: {SessionId}", sessionId);

            return Ok(session);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [PROCESS-FLOW] Error analyzing prompt construction");
            return StatusCode(500, new { error = "Failed to analyze prompt construction", details = ex.Message });
        }
    }

    /// <summary>
    /// Get transparency metrics and analytics - CONSOLIDATED ProcessFlow Integration
    /// </summary>
    [HttpGet("metrics")]
    public async Task<ActionResult<object>> GetTransparencyMetrics(
        [FromQuery] string? userId = null,
        [FromQuery] int days = 7)
    {
        try
        {
            _logger.LogInformation("📊 [PROCESS-FLOW] Retrieving transparency metrics for user: {UserId}, days: {Days}", userId, days);

            // Get user sessions from ProcessFlow
            var sessions = userId != null 
                ? await _processFlowService.GetUserSessionsAsync(userId, 100)
                : new List<ProcessFlowSession>();

            // Get step performance analytics
            var stepPerformance = await _processFlowService.GetStepPerformanceAsync();

            // Calculate metrics from ProcessFlow data
            var metrics = new
            {
                TotalSessions = sessions.Count(),
                SuccessfulSessions = sessions.Count(s => s.Status == ProcessFlowStatus.Completed),
                SuccessRate = sessions.Any() ? (double)sessions.Count(s => s.Status == ProcessFlowStatus.Completed) / sessions.Count() * 100 : 0,
                AverageConfidence = sessions.Where(s => s.OverallConfidence.HasValue).Average(s => (double?)s.OverallConfidence) ?? 0,
                AverageProcessingTime = sessions.Where(s => s.TotalDurationMs.HasValue).Average(s => (double?)s.TotalDurationMs) ?? 0,
                StepPerformance = stepPerformance.Select(sp => new {
                    StepId = sp.StepId,
                    Name = sp.Name,
                    ExecutionCount = sp.ExecutionCount,
                    AvgDurationMs = sp.AvgDurationMs,
                    SuccessRate = sp.SuccessRate,
                    AvgConfidence = sp.AvgConfidence
                }).ToList(),
                TokenUsage = sessions.Where(s => s.Transparency != null).Sum(s => s.Transparency!.TotalTokens ?? 0),
                EstimatedCost = sessions.Where(s => s.Transparency != null).Sum(s => (double)(s.Transparency!.EstimatedCost ?? 0))
            };

            _logger.LogInformation("✅ [PROCESS-FLOW] Retrieved metrics - Sessions: {Sessions}, Success Rate: {SuccessRate:F1}%", 
                metrics.TotalSessions, metrics.SuccessRate);

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [PROCESS-FLOW] Error retrieving transparency metrics");
            return StatusCode(500, new { error = "Failed to retrieve transparency metrics", details = ex.Message });
        }
    }

    // NOTE: All other legacy transparency endpoints have been removed and replaced by ProcessFlow system
    // Legacy endpoints removed:
    // - GET /confidence/{analysisId} -> Use ProcessFlowSession.Steps for confidence data
    // - GET /alternatives/{traceId} -> Use ProcessFlow analytics for alternatives
    // - POST /optimize -> Use ProcessFlow performance metrics for optimization
    // - GET /dashboard/metrics -> Use ProcessFlow metrics endpoint
    // - GET /settings -> Use ProcessFlow configuration
    // - PUT /settings -> Use ProcessFlow configuration
    // - POST /export -> Use ProcessFlow data export
    // - GET /traces/recent -> Use ProcessFlow session queries
    // - GET /traces/{traceId}/detail -> Use ProcessFlow session detail
    // - GET /trends/confidence -> Use ProcessFlow analytics
    // - GET /analytics/token-usage -> Use AnalyticsController ProcessFlow endpoints
    // - GET /performance -> Use ProcessFlow performance metrics
}
