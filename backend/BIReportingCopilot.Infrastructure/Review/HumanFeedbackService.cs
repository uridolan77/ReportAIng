using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.Review;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Interfaces;
using System.Collections.Concurrent;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Core.Interfaces.AI;

namespace BIReportingCopilot.Infrastructure.Review;

/// <summary>
/// Phase 4: Human Feedback Service Implementation
/// Collects and processes human feedback to improve AI performance
/// </summary>
public class HumanFeedbackService : IHumanFeedbackService
{
    private readonly ILogger<HumanFeedbackService> _logger;
    private readonly IAuditService _auditService;
    private readonly IAILearningService _learningService;
    
    // In-memory storage for demo (would be database in production)
    private readonly ConcurrentDictionary<string, List<HumanFeedback>> _feedbackByReview = new();
    private readonly ConcurrentDictionary<string, FeedbackPattern> _patterns = new();

    public HumanFeedbackService(
        ILogger<HumanFeedbackService> logger,
        IAuditService auditService,
        IAILearningService learningService)
    {
        _logger = logger;
        _auditService = auditService;
        _learningService = learningService;
    }

    /// <summary>
    /// Submit feedback for a review
    /// </summary>
    public async Task<HumanFeedback> SubmitFeedbackAsync(
        string reviewRequestId,
        string reviewerId,
        FeedbackType type,
        FeedbackAction action,
        string? correctedSql = null,
        string? comments = null,
        int qualityRating = 3,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("💬 Submitting feedback for review: {ReviewId} by {ReviewerId} - Action: {Action}", 
                reviewRequestId, reviewerId, action);

            var feedback = new HumanFeedback
            {
                ReviewRequestId = reviewRequestId,
                ReviewerId = reviewerId,
                Type = type,
                Action = action,
                CorrectedSql = correctedSql,
                Comments = comments,
                QualityRating = qualityRating,
                IsApproved = action == FeedbackAction.Approve,
                ProvidedAt = DateTime.UtcNow
            };

            // Analyze feedback for issues and improvements
            await AnalyzeFeedbackAsync(feedback, cancellationToken);

            // Store feedback
            if (!_feedbackByReview.ContainsKey(reviewRequestId))
            {
                _feedbackByReview[reviewRequestId] = new List<HumanFeedback>();
            }
            _feedbackByReview[reviewRequestId].Add(feedback);

            // Learn from feedback to improve AI
            await LearnFromFeedbackAsync(feedback, cancellationToken);

            // Log audit event
            await _auditService.LogAsync(
                "FeedbackSubmitted",
                reviewerId,
                "HumanFeedback",
                feedback.Id,
                new { ReviewRequestId = reviewRequestId, Action = action, QualityRating = qualityRating });

            _logger.LogInformation("✅ Feedback submitted successfully: {FeedbackId}", feedback.Id);
            return feedback;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error submitting feedback");
            throw;
        }
    }

    /// <summary>
    /// Get feedback for a review
    /// </summary>
    public async Task<List<HumanFeedback>> GetFeedbackAsync(
        string reviewRequestId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async operation
            
            return _feedbackByReview.TryGetValue(reviewRequestId, out var feedback) 
                ? feedback.OrderBy(f => f.ProvidedAt).ToList()
                : new List<HumanFeedback>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting feedback for review: {ReviewId}", reviewRequestId);
            return new List<HumanFeedback>();
        }
    }

    /// <summary>
    /// Get feedback analytics
    /// </summary>
    public async Task<FeedbackAnalytics> GetFeedbackAnalyticsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? reviewerId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("📊 Generating feedback analytics");

            await Task.Delay(1, cancellationToken); // Simulate async operation

            var allFeedback = _feedbackByReview.Values
                .SelectMany(f => f)
                .Where(f => startDate == null || f.ProvidedAt >= startDate)
                .Where(f => endDate == null || f.ProvidedAt <= endDate)
                .Where(f => reviewerId == null || f.ReviewerId == reviewerId)
                .ToList();

            var analytics = new FeedbackAnalytics
            {
                Period = $"{startDate?.ToString("yyyy-MM-dd") ?? "All"} to {endDate?.ToString("yyyy-MM-dd") ?? "All"}",
                TotalFeedback = allFeedback.Count,
                AverageQualityRating = allFeedback.Any() ? allFeedback.Average(f => f.QualityRating) : 0,
                GeneratedAt = DateTime.UtcNow
            };

            // Feedback by type
            analytics.FeedbackByType = allFeedback
                .GroupBy(f => f.Type)
                .ToDictionary(g => g.Key, g => g.Count());

            // Actions by type
            analytics.ActionsByType = allFeedback
                .GroupBy(f => f.Action)
                .ToDictionary(g => g.Key, g => g.Count());

            // Common issues
            analytics.CommonIssues = allFeedback
                .SelectMany(f => f.IssuesIdentified)
                .GroupBy(i => i)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList();

            // Improvement suggestions
            analytics.ImprovementSuggestions = allFeedback
                .SelectMany(f => f.SuggestedImprovements)
                .GroupBy(s => s)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList();

            // Reviewer ratings
            analytics.ReviewerRatings = allFeedback
                .GroupBy(f => f.ReviewerId)
                .ToDictionary(g => g.Key, g => g.Average(f => f.QualityRating));

            _logger.LogDebug("✅ Feedback analytics generated");
            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating feedback analytics");
            throw;
        }
    }

    /// <summary>
    /// Learn from human feedback to improve AI
    /// </summary>
    public async Task<bool> LearnFromFeedbackAsync(
        HumanFeedback feedback,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🧠 Learning from feedback: {FeedbackId}", feedback.Id);

            // Extract learning patterns
            var learningData = new Dictionary<string, object>
            {
                ["feedback_type"] = feedback.Type.ToString(),
                ["action"] = feedback.Action.ToString(),
                ["quality_rating"] = feedback.QualityRating,
                ["has_corrections"] = !string.IsNullOrEmpty(feedback.CorrectedSql),
                ["issues_count"] = feedback.IssuesIdentified.Count,
                ["improvements_count"] = feedback.SuggestedImprovements.Count
            };

            // If SQL was corrected, analyze the differences
            if (!string.IsNullOrEmpty(feedback.CorrectedSql))
            {
                learningData["correction_provided"] = true;
                learningData["correction_length"] = feedback.CorrectedSql.Length;
                
                // In production, would perform detailed SQL diff analysis
                await AnalyzeSqlCorrectionsAsync(feedback, cancellationToken);
            }

            // Submit to AI learning service
            await _learningService.SubmitFeedbackAsync(
                feedback.ReviewRequestId,
                feedback.ReviewerId,
                learningData,
                cancellationToken);

            // Update feedback patterns
            await UpdateFeedbackPatternsAsync(feedback, cancellationToken);

            _logger.LogInformation("✅ Learning from feedback completed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error learning from feedback");
            return false;
        }
    }

    /// <summary>
    /// Get common feedback patterns
    /// </summary>
    public async Task<List<FeedbackPattern>> GetFeedbackPatternsAsync(
        ReviewType? type = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async operation

            var patterns = _patterns.Values.ToList();

            if (type.HasValue)
            {
                patterns = patterns.Where(p => p.Type == type.Value).ToList();
            }

            return patterns.OrderByDescending(p => p.ImpactScore).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting feedback patterns");
            return new List<FeedbackPattern>();
        }
    }

    // Private helper methods
    private async Task AnalyzeFeedbackAsync(HumanFeedback feedback, CancellationToken cancellationToken)
    {
        try
        {
            // Analyze comments for common issues
            if (!string.IsNullOrEmpty(feedback.Comments))
            {
                var issues = ExtractIssuesFromComments(feedback.Comments);
                feedback.IssuesIdentified.AddRange(issues);

                var improvements = ExtractImprovementsFromComments(feedback.Comments);
                feedback.SuggestedImprovements.AddRange(improvements);
            }

            // Analyze SQL corrections
            if (!string.IsNullOrEmpty(feedback.CorrectedSql))
            {
                var sqlIssues = await AnalyzeSqlCorrectionsAsync(feedback, cancellationToken);
                feedback.IssuesIdentified.AddRange(sqlIssues);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error analyzing feedback");
        }
    }

    private List<string> ExtractIssuesFromComments(string comments)
    {
        var issues = new List<string>();
        var lowerComments = comments.ToLower();

        // Common issue patterns
        var issuePatterns = new Dictionary<string, string>
        {
            ["join"] = "Incorrect table joins",
            ["where"] = "Missing or incorrect WHERE clause",
            ["performance"] = "Performance concerns",
            ["syntax"] = "SQL syntax errors",
            ["logic"] = "Business logic issues",
            ["security"] = "Security vulnerabilities",
            ["permission"] = "Permission or access issues"
        };

        foreach (var pattern in issuePatterns)
        {
            if (lowerComments.Contains(pattern.Key))
            {
                issues.Add(pattern.Value);
            }
        }

        return issues;
    }

    private List<string> ExtractImprovementsFromComments(string comments)
    {
        var improvements = new List<string>();
        var lowerComments = comments.ToLower();

        // Common improvement patterns
        var improvementPatterns = new Dictionary<string, string>
        {
            ["index"] = "Add database indexes",
            ["optimize"] = "Optimize query performance",
            ["simplify"] = "Simplify query structure",
            ["validate"] = "Add input validation",
            ["document"] = "Improve documentation",
            ["test"] = "Add more test cases"
        };

        foreach (var pattern in improvementPatterns)
        {
            if (lowerComments.Contains(pattern.Key))
            {
                improvements.Add(pattern.Value);
            }
        }

        return improvements;
    }

    private async Task<List<string>> AnalyzeSqlCorrectionsAsync(HumanFeedback feedback, CancellationToken cancellationToken)
    {
        var issues = new List<string>();

        try
        {
            // In production, would perform detailed SQL analysis
            // For now, simulate basic analysis
            await Task.Delay(1, cancellationToken);

            if (!string.IsNullOrEmpty(feedback.CorrectedSql))
            {
                // Simulate SQL correction analysis
                issues.Add("SQL structure improved");
                
                if (feedback.CorrectedSql.Contains("JOIN"))
                {
                    issues.Add("Table join corrections applied");
                }
                
                if (feedback.CorrectedSql.Contains("WHERE"))
                {
                    issues.Add("Filter conditions improved");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error analyzing SQL corrections");
        }

        return issues;
    }

    private async Task UpdateFeedbackPatternsAsync(HumanFeedback feedback, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async operation

            // Create or update patterns based on feedback
            foreach (var issue in feedback.IssuesIdentified)
            {
                var patternKey = $"{feedback.Type}_{issue}";
                
                if (_patterns.TryGetValue(patternKey, out var existingPattern))
                {
                    existingPattern.Frequency++;
                    existingPattern.ImpactScore = CalculateImpactScore(existingPattern);
                }
                else
                {
                    var newPattern = new FeedbackPattern
                    {
                        Pattern = issue,
                        Description = $"Common issue: {issue}",
                        Frequency = 1,
                        Type = ReviewType.SqlValidation, // Default to SQL validation for feedback patterns
                        CommonIssues = new List<string> { issue },
                        SuggestedImprovement = GetSuggestedImprovement(issue),
                        ImpactScore = 1.0
                    };
                    
                    _patterns[patternKey] = newPattern;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating feedback patterns");
        }
    }

    private double CalculateImpactScore(FeedbackPattern pattern)
    {
        // Simple impact score calculation based on frequency and type
        var baseScore = Math.Log(pattern.Frequency + 1);
        var typeMultiplier = pattern.Type switch
        {
            ReviewType.SecurityReview => 2.0,
            ReviewType.BusinessLogic => 1.5,
            ReviewType.SqlValidation => 1.2,
            _ => 1.0
        };
        
        return baseScore * typeMultiplier;
    }

    private string GetSuggestedImprovement(string issue)
    {
        return issue.ToLower() switch
        {
            var i when i.Contains("join") => "Improve table relationship understanding",
            var i when i.Contains("where") => "Enhance filter condition generation",
            var i when i.Contains("performance") => "Add performance optimization rules",
            var i when i.Contains("syntax") => "Improve SQL syntax validation",
            var i when i.Contains("logic") => "Enhance business logic understanding",
            var i when i.Contains("security") => "Strengthen security validation",
            _ => "General AI model improvement needed"
        };
    }
}
