using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.Review;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Interfaces;
using System.Collections.Concurrent;
using BIReportingCopilot.Core.Interfaces.Security;

namespace BIReportingCopilot.Infrastructure.Review;

/// <summary>
/// Phase 4: Approval Workflow Service Implementation
/// Manages multi-step approval processes for human review
/// </summary>
public class ApprovalWorkflowService : IApprovalWorkflowService
{
    private readonly ILogger<ApprovalWorkflowService> _logger;
    private readonly IReviewNotificationService _notificationService;
    private readonly IAuditService _auditService;
    
    // In-memory storage for demo (would be database in production)
    private readonly ConcurrentDictionary<string, ApprovalWorkflow> _workflows = new();
    private readonly ConcurrentDictionary<string, WorkflowTemplate> _templates = new();

    public ApprovalWorkflowService(
        ILogger<ApprovalWorkflowService> logger,
        IReviewNotificationService notificationService,
        IAuditService auditService)
    {
        _logger = logger;
        _notificationService = notificationService;
        _auditService = auditService;
        
        InitializeDefaultTemplates();
    }

    /// <summary>
    /// Start an approval workflow
    /// </summary>
    public async Task<ApprovalWorkflow> StartWorkflowAsync(
        string reviewRequestId,
        string workflowName,
        List<ApprovalStep> steps,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🚀 Starting approval workflow: {WorkflowName} for review: {ReviewId}", 
                workflowName, reviewRequestId);

            var workflow = new ApprovalWorkflow
            {
                ReviewRequestId = reviewRequestId,
                WorkflowName = workflowName,
                Steps = steps.OrderBy(s => s.Order).ToList(),
                Status = WorkflowStatus.InProgress,
                StartedAt = DateTime.UtcNow
            };

            // Initialize first step
            if (workflow.Steps.Any())
            {
                var firstStep = workflow.Steps[0];
                firstStep.Status = ApprovalStepStatus.InProgress;
                firstStep.StartedAt = DateTime.UtcNow;

                // Assign to user if specified
                if (!string.IsNullOrEmpty(firstStep.AssignedTo))
                {
                    await _notificationService.SendReviewAssignmentNotificationAsync(
                        reviewRequestId, firstStep.AssignedTo, cancellationToken);
                }
            }

            _workflows[workflow.Id] = workflow;

            // Log audit event
            await _auditService.LogAsync(
                "WorkflowStarted",
                "System",
                "ApprovalWorkflow",
                workflow.Id,
                new { WorkflowName = workflowName, ReviewRequestId = reviewRequestId, StepCount = steps.Count });

            _logger.LogInformation("✅ Workflow started: {WorkflowId}", workflow.Id);
            return workflow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error starting workflow");
            throw;
        }
    }

    /// <summary>
    /// Get workflow by ID
    /// </summary>
    public async Task<ApprovalWorkflow?> GetWorkflowAsync(string workflowId, CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async operation
            return _workflows.TryGetValue(workflowId, out var workflow) ? workflow : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting workflow: {WorkflowId}", workflowId);
            return null;
        }
    }

    /// <summary>
    /// Process approval step
    /// </summary>
    public async Task<bool> ProcessApprovalStepAsync(
        string workflowId,
        string stepId,
        string decision,
        string? comments = null,
        string? processedBy = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("⚡ Processing approval step: {StepId} in workflow: {WorkflowId} - Decision: {Decision}", 
                stepId, workflowId, decision);

            var workflow = await GetWorkflowAsync(workflowId, cancellationToken);
            if (workflow == null)
            {
                _logger.LogWarning("⚠️ Workflow not found: {WorkflowId}", workflowId);
                return false;
            }

            var step = workflow.Steps.FirstOrDefault(s => s.Id == stepId);
            if (step == null)
            {
                _logger.LogWarning("⚠️ Step not found: {StepId}", stepId);
                return false;
            }

            // Update step
            step.Status = decision.ToLower() switch
            {
                "approve" or "approved" => ApprovalStepStatus.Approved,
                "reject" or "rejected" => ApprovalStepStatus.Rejected,
                "skip" or "skipped" => ApprovalStepStatus.Skipped,
                _ => ApprovalStepStatus.Rejected
            };

            step.Decision = decision;
            step.Comments = comments;
            step.CompletedAt = DateTime.UtcNow;

            // Check if workflow should continue or complete
            if (step.Status == ApprovalStepStatus.Rejected && step.IsRequired)
            {
                // Required step rejected - fail workflow
                workflow.Status = WorkflowStatus.Failed;
                workflow.CompletedAt = DateTime.UtcNow;
                workflow.FinalDecision = "Rejected";
            }
            else if (step.Status == ApprovalStepStatus.Approved || step.Status == ApprovalStepStatus.Skipped)
            {
                // Move to next step or complete workflow
                await AdvanceWorkflowAsync(workflow, cancellationToken);
            }

            // Log audit event
            await _auditService.LogAsync(
                "ApprovalStepProcessed",
                processedBy ?? "System",
                "ApprovalStep",
                stepId,
                new { WorkflowId = workflowId, Decision = decision, Status = step.Status });

            _logger.LogInformation("✅ Approval step processed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing approval step");
            return false;
        }
    }

    /// <summary>
    /// Get active workflows for a user
    /// </summary>
    public async Task<List<ApprovalWorkflow>> GetActiveWorkflowsAsync(
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async operation

            var activeWorkflows = _workflows.Values
                .Where(w => w.Status == WorkflowStatus.InProgress)
                .ToList();

            if (!string.IsNullOrEmpty(userId))
            {
                activeWorkflows = activeWorkflows
                    .Where(w => w.Steps.Any(s => s.AssignedTo == userId && s.Status == ApprovalStepStatus.Pending))
                    .ToList();
            }

            return activeWorkflows;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting active workflows");
            return new List<ApprovalWorkflow>();
        }
    }

    /// <summary>
    /// Cancel a workflow
    /// </summary>
    public async Task<bool> CancelWorkflowAsync(string workflowId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("❌ Cancelling workflow: {WorkflowId} - Reason: {Reason}", workflowId, reason);

            var workflow = await GetWorkflowAsync(workflowId, cancellationToken);
            if (workflow == null)
            {
                return false;
            }

            workflow.Status = WorkflowStatus.Cancelled;
            workflow.CompletedAt = DateTime.UtcNow;
            workflow.FinalDecision = $"Cancelled: {reason}";

            // Cancel all pending steps
            foreach (var step in workflow.Steps.Where(s => s.Status == ApprovalStepStatus.Pending))
            {
                step.Status = ApprovalStepStatus.Skipped;
                step.Comments = $"Cancelled: {reason}";
                step.CompletedAt = DateTime.UtcNow;
            }

            _logger.LogInformation("✅ Workflow cancelled successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error cancelling workflow");
            return false;
        }
    }

    /// <summary>
    /// Get workflow templates
    /// </summary>
    public async Task<List<WorkflowTemplate>> GetWorkflowTemplatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async operation
            return _templates.Values.Where(t => t.IsActive).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting workflow templates");
            return new List<WorkflowTemplate>();
        }
    }

    // Private helper methods
    private async Task AdvanceWorkflowAsync(ApprovalWorkflow workflow, CancellationToken cancellationToken)
    {
        try
        {
            // Find next pending step
            var nextStep = workflow.Steps
                .Where(s => s.Status == ApprovalStepStatus.Pending)
                .OrderBy(s => s.Order)
                .FirstOrDefault();

            if (nextStep != null)
            {
                // Start next step
                nextStep.Status = ApprovalStepStatus.InProgress;
                nextStep.StartedAt = DateTime.UtcNow;
                workflow.CurrentStepIndex = workflow.Steps.IndexOf(nextStep);

                // Send notification if assigned
                if (!string.IsNullOrEmpty(nextStep.AssignedTo))
                {
                    await _notificationService.SendReviewAssignmentNotificationAsync(
                        workflow.ReviewRequestId, nextStep.AssignedTo, cancellationToken);
                }

                _logger.LogInformation("➡️ Advanced to next step: {StepName}", nextStep.Name);
            }
            else
            {
                // No more steps - complete workflow
                workflow.Status = WorkflowStatus.Completed;
                workflow.CompletedAt = DateTime.UtcNow;
                workflow.FinalDecision = "Approved";

                _logger.LogInformation("🎉 Workflow completed: {WorkflowId}", workflow.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error advancing workflow");
        }
    }

    private void InitializeDefaultTemplates()
    {
        // SQL Validation Workflow
        var sqlValidationTemplate = new WorkflowTemplate
        {
            Id = "sql-validation-workflow",
            Name = "SQL Validation Workflow",
            Description = "Standard workflow for SQL validation reviews",
            ApplicableType = ReviewType.SqlValidation,
            Steps = new List<ApprovalStep>
            {
                new ApprovalStep
                {
                    Name = "Technical Review",
                    Description = "Technical validation of SQL syntax and logic",
                    AssignedRole = "Developer",
                    Order = 0,
                    Timeout = TimeSpan.FromHours(4)
                },
                new ApprovalStep
                {
                    Name = "Senior Review",
                    Description = "Senior developer approval",
                    AssignedRole = "SeniorDeveloper",
                    Order = 1,
                    Timeout = TimeSpan.FromHours(8)
                }
            }
        };

        // Security Review Workflow
        var securityReviewTemplate = new WorkflowTemplate
        {
            Id = "security-review-workflow",
            Name = "Security Review Workflow",
            Description = "Enhanced workflow for security-sensitive queries",
            ApplicableType = ReviewType.SecurityReview,
            Steps = new List<ApprovalStep>
            {
                new ApprovalStep
                {
                    Name = "Security Scan",
                    Description = "Automated security vulnerability scan",
                    AssignedRole = "SecurityAnalyst",
                    Order = 0,
                    Timeout = TimeSpan.FromHours(2)
                },
                new ApprovalStep
                {
                    Name = "Security Review",
                    Description = "Manual security review",
                    AssignedRole = "SecurityOfficer",
                    Order = 1,
                    Timeout = TimeSpan.FromHours(24)
                },
                new ApprovalStep
                {
                    Name = "Final Approval",
                    Description = "Final security approval",
                    AssignedRole = "SecurityManager",
                    Order = 2,
                    Timeout = TimeSpan.FromHours(48)
                }
            }
        };

        _templates[sqlValidationTemplate.Id] = sqlValidationTemplate;
        _templates[securityReviewTemplate.Id] = securityReviewTemplate;

        _logger.LogInformation("✅ Initialized {Count} workflow templates", _templates.Count);
    }
}
