using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Core.Interfaces.Repositories;
using BIReportingCopilot.Core.Models.Analytics;
using BIReportingCopilot.Core.Models.TemplateEntities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// ML-based template improvement service with automated analysis and recommendation generation
/// </summary>
public class TemplateImprovementService : ITemplateImprovementService
{
    private readonly ITemplateImprovementRepository _improvementRepository;
    private readonly IEnhancedPromptTemplateRepository _templateRepository;
    private readonly ITemplatePerformanceRepository _performanceRepository;
    private readonly ITemplateABTestRepository _abTestRepository;
    private readonly IABTestingService _abTestingService;
    private readonly ILogger<TemplateImprovementService> _logger;

    public TemplateImprovementService(
        ITemplateImprovementRepository improvementRepository,
        IEnhancedPromptTemplateRepository templateRepository,
        ITemplatePerformanceRepository performanceRepository,
        ITemplateABTestRepository abTestRepository,
        IABTestingService abTestingService,
        ILogger<TemplateImprovementService> logger)
    {
        _improvementRepository = improvementRepository;
        _templateRepository = templateRepository;
        _performanceRepository = performanceRepository;
        _abTestRepository = abTestRepository;
        _abTestingService = abTestingService;
        _logger = logger;
    }

    public async Task<List<TemplateImprovementSuggestion>> AnalyzeTemplatePerformanceAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing template performance for improvement suggestions: {TemplateKey}", templateKey);

            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template == null)
            {
                throw new ArgumentException($"Template not found: {templateKey}");
            }

            var performance = await _performanceRepository.GetByTemplateIdAsync(template.Id, cancellationToken);
            if (performance == null)
            {
                _logger.LogWarning("No performance data available for template: {TemplateKey}", templateKey);
                return new List<TemplateImprovementSuggestion>();
            }

            var suggestions = new List<TemplateImprovementSuggestion>();

            // Analyze performance metrics and generate suggestions
            suggestions.AddRange(await AnalyzeSuccessRateAsync(template, performance, cancellationToken));
            suggestions.AddRange(await AnalyzeConfidenceScoreAsync(template, performance, cancellationToken));
            suggestions.AddRange(await AnalyzeResponseTimeAsync(template, performance, cancellationToken));
            suggestions.AddRange(await AnalyzeUserRatingAsync(template, performance, cancellationToken));
            suggestions.AddRange(await AnalyzeContentQualityAsync(template, cancellationToken));

            // Store suggestions in database
            foreach (var suggestion in suggestions)
            {
                await CreateImprovementSuggestionAsync(suggestion, cancellationToken);
            }

            _logger.LogInformation("Generated {Count} improvement suggestions for template {TemplateKey}", 
                suggestions.Count, templateKey);

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing template performance for {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<List<TemplateImprovementSuggestion>> GenerateImprovementSuggestionsAsync(double performanceThreshold = 0.7, int minDataPoints = 100, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating improvement suggestions for underperforming templates");

            var allSuggestions = new List<TemplateImprovementSuggestion>();
            var underperformingTemplates = await GetUnderperformingTemplatesAsync(performanceThreshold, minDataPoints, cancellationToken);

            foreach (var template in underperformingTemplates)
            {
                var suggestions = await AnalyzeTemplatePerformanceAsync(template.TemplateKey ?? string.Empty, cancellationToken);
                allSuggestions.AddRange(suggestions);
            }

            _logger.LogInformation("Generated {Count} total improvement suggestions for {TemplateCount} templates", 
                allSuggestions.Count, underperformingTemplates.Count);

            return allSuggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating improvement suggestions");
            throw;
        }
    }

    public async Task<OptimizedTemplate> OptimizeTemplateAsync(string templateKey, OptimizationStrategy strategy, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Optimizing template {TemplateKey} with strategy {Strategy}", templateKey, strategy);

            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template == null)
            {
                throw new ArgumentException($"Template not found: {templateKey}");
            }

            var performance = await _performanceRepository.GetByTemplateIdAsync(template.Id, cancellationToken);
            var optimizedContent = await ApplyOptimizationStrategyAsync(template.Content, strategy, performance, cancellationToken);

            var optimizedTemplate = new OptimizedTemplate
            {
                OriginalTemplateKey = templateKey,
                OptimizedContent = optimizedContent.Content,
                StrategyUsed = strategy,
                ChangesApplied = optimizedContent.Changes,
                ExpectedPerformanceImprovement = optimizedContent.ExpectedImprovement,
                ConfidenceScore = optimizedContent.ConfidenceScore,
                OptimizationReasoning = optimizedContent.Reasoning,
                MetricPredictions = optimizedContent.MetricPredictions,
                OptimizedDate = DateTime.UtcNow,
                OptimizedBy = "ML_System"
            };

            return optimizedTemplate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing template {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<ABTestResult> CreateABTestAsync(ABTestRequest request, CancellationToken cancellationToken = default)
    {
        return await _abTestingService.CreateABTestAsync(request, cancellationToken);
    }

    public async Task<ABTestAnalysis> GetABTestResultsAsync(long testId, CancellationToken cancellationToken = default)
    {
        return await _abTestingService.AnalyzeTestResultsAsync(testId, cancellationToken);
    }

    public async Task<ImplementationResult> ImplementWinningVariantAsync(long testId, CancellationToken cancellationToken = default)
    {
        return await _abTestingService.CompleteTestAsync(testId, true, cancellationToken);
    }

    public async Task<FeedbackAnalysis> AnalyzeUserFeedbackAsync(string templateKey, TimeSpan? timeWindow = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing user feedback for template: {TemplateKey}", templateKey);

            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template == null)
            {
                throw new ArgumentException($"Template not found: {templateKey}");
            }

            var performance = await _performanceRepository.GetByTemplateIdAsync(template.Id, cancellationToken);
            var window = timeWindow ?? TimeSpan.FromDays(30);

            // Simulate feedback analysis - in a real implementation, this would analyze actual user feedback
            var feedbackAnalysis = new FeedbackAnalysis
            {
                TemplateKey = templateKey,
                AnalysisTimeWindow = window,
                TotalFeedbackCount = performance?.TotalUsages ?? 0,
                AverageRating = performance?.AverageUserRating ?? 0,
                FeedbackThemes = GenerateFeedbackThemes(performance),
                SentimentDistribution = GenerateSentimentDistribution(performance),
                ImprovementSuggestions = GenerateImprovementSuggestionsFromFeedback(performance),
                AnalysisDate = DateTime.UtcNow
            };

            return feedbackAnalysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing user feedback for {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<List<TemplateVariant>> GenerateTemplateVariantsAsync(string templateKey, int variantCount = 3, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating {Count} variants for template: {TemplateKey}", variantCount, templateKey);

            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template == null)
            {
                throw new ArgumentException($"Template not found: {templateKey}");
            }

            var variants = new List<TemplateVariant>();
            var performance = await _performanceRepository.GetByTemplateIdAsync(template.Id, cancellationToken);

            for (int i = 0; i < variantCount; i++)
            {
                var variantType = (VariantType)(i % Enum.GetValues<VariantType>().Length);
                var variant = await GenerateVariantAsync(template, variantType, performance, cancellationToken);
                variants.Add(variant);
            }

            return variants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating template variants for {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<PerformancePrediction> PredictTemplatePerformanceAsync(string templateContent, string intentType, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Predicting performance for new template with intent type: {IntentType}", intentType);

            // Analyze similar templates for prediction baseline
            var similarTemplates = await GetSimilarTemplatesAsync(templateContent, intentType, cancellationToken);
            var contentAnalysis = await AnalyzeTemplateContentQualityAsync(templateContent, cancellationToken);

            var prediction = new PerformancePrediction
            {
                TemplateContent = templateContent,
                IntentType = intentType,
                PredictedSuccessRate = CalculatePredictedSuccessRate(similarTemplates, contentAnalysis),
                PredictedUserRating = CalculatePredictedUserRating(similarTemplates, contentAnalysis),
                PredictedResponseTime = CalculatePredictedResponseTime(similarTemplates, contentAnalysis),
                PredictionConfidence = CalculatePredictionConfidence(similarTemplates.Count, contentAnalysis),
                StrengthFactors = ExtractStrengthFactors(contentAnalysis),
                WeaknessFactors = ExtractWeaknessFactors(contentAnalysis),
                ImprovementSuggestions = contentAnalysis.ImprovementSuggestions,
                FeatureScores = CalculateFeatureScores(templateContent, contentAnalysis),
                PredictionDate = DateTime.UtcNow
            };

            return prediction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting template performance");
            throw;
        }
    }

    public async Task<List<ImprovementRecommendation>> GetImprovementRecommendationsAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting improvement recommendations for template: {TemplateKey}", templateKey);

            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template == null)
            {
                throw new ArgumentException($"Template not found: {templateKey}");
            }

            var performance = await _performanceRepository.GetByTemplateIdAsync(template.Id, cancellationToken);
            var successfulPatterns = await GetSuccessfulPatternsAsync(template.IntentType, cancellationToken);

            var recommendations = new List<ImprovementRecommendation>();

            // Generate recommendations based on successful patterns
            foreach (var pattern in successfulPatterns)
            {
                var recommendation = new ImprovementRecommendation
                {
                    RecommendationId = Guid.NewGuid().ToString(),
                    TemplateKey = templateKey,
                    Type = DetermineImprovementType(pattern),
                    Title = pattern.Title,
                    Description = pattern.Description,
                    SpecificSuggestion = pattern.Suggestion,
                    ExpectedImpact = pattern.ExpectedImpact,
                    ImplementationEffort = pattern.ImplementationEffort,
                    Priority = CalculatePriority(pattern.ExpectedImpact, pattern.ImplementationEffort),
                    BasedOnPatterns = pattern.BasedOnPatterns,
                    SimilarSuccessfulTemplates = pattern.SimilarTemplates,
                    GeneratedDate = DateTime.UtcNow
                };

                recommendations.Add(recommendation);
            }

            return recommendations.OrderByDescending(r => r.Priority).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting improvement recommendations for {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<ContentQualityAnalysis> AnalyzeTemplateContentQualityAsync(string templateContent, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Analyzing template content quality");

            var analysis = new ContentQualityAnalysis
            {
                TemplateContent = templateContent,
                OverallQualityScore = CalculateOverallQualityScore(templateContent),
                QualityDimensions = AnalyzeQualityDimensions(templateContent),
                IdentifiedIssues = IdentifyQualityIssues(templateContent),
                Strengths = IdentifyQualityStrengths(templateContent),
                ImprovementSuggestions = GenerateContentImprovementSuggestions(templateContent),
                Readability = AnalyzeReadability(templateContent),
                Structure = AnalyzeStructure(templateContent),
                Completeness = AnalyzeCompleteness(templateContent),
                AnalysisDate = DateTime.UtcNow
            };

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing template content quality");
            throw;
        }
    }

    public async Task<OptimizationHistory> GetOptimizationHistoryAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting optimization history for template: {TemplateKey}", templateKey);

            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template == null)
            {
                throw new ArgumentException($"Template not found: {templateKey}");
            }

            var suggestions = await _improvementRepository.GetByTemplateIdAsync(template.Id, cancellationToken);
            var abTests = await _abTestRepository.GetTestsByTemplateIdAsync(template.Id, cancellationToken);

            var history = new OptimizationHistory
            {
                TemplateKey = templateKey,
                OptimizationEvents = BuildOptimizationEvents(suggestions, abTests),
                PerformanceTrend = await BuildPerformanceTrend(template.Id, cancellationToken),
                TotalOptimizations = suggestions.Count,
                SuccessfulOptimizations = suggestions.Count(s => s.Status == "implemented"),
                AverageImprovementPercent = suggestions.Where(s => s.ExpectedImprovementPercent.HasValue)
                    .Average(s => s.ExpectedImprovementPercent.Value),
                LastOptimizationDate = suggestions.Any() ? suggestions.Max(s => s.CreatedDate) : null,
                GeneratedDate = DateTime.UtcNow
            };

            return history;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting optimization history for {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<ModelTrainingResult> TrainImprovementModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Training improvement models with latest performance data");

            var startTime = DateTime.UtcNow;
            var allTemplates = await _templateRepository.GetAllAsync(cancellationToken);
            var performanceData = new List<TemplatePerformanceMetricsEntity>();

            foreach (var template in allTemplates)
            {
                var performance = await _performanceRepository.GetByTemplateIdAsync(template.Id, cancellationToken);
                if (performance != null)
                {
                    performanceData.Add(performance);
                }
            }

            // Simulate model training - in a real implementation, this would train actual ML models
            var trainingResult = new ModelTrainingResult
            {
                ModelVersion = $"v{DateTime.UtcNow:yyyyMMdd_HHmmss}",
                TrainingStartTime = startTime,
                TrainingEndTime = DateTime.UtcNow,
                TrainingDuration = DateTime.UtcNow - startTime,
                DataPointsUsed = performanceData.Count,
                ModelAccuracy = CalculateModelAccuracy(performanceData),
                ValidationScore = CalculateValidationScore(performanceData),
                TrainingMetrics = CalculateTrainingMetrics(performanceData),
                FeatureImportance = CalculateFeatureImportance(),
                ModelPerformanceImprovement = CalculateModelImprovement(),
                TrainingStatus = "Completed",
                TrainingNotes = "Model trained successfully with latest performance data"
            };

            _logger.LogInformation("Model training completed. Accuracy: {Accuracy:P2}, Duration: {Duration}",
                trainingResult.ModelAccuracy, trainingResult.TrainingDuration);

            return trainingResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error training improvement models");
            throw;
        }
    }

    public async Task<ModelPerformanceMetrics> GetModelPerformanceMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting model performance metrics");

            var recentSuggestions = await _improvementRepository.GetRecentSuggestionsAsync(30, cancellationToken);
            var implementedSuggestions = recentSuggestions.Where(s => s.Status == "implemented").ToList();

            var metrics = new ModelPerformanceMetrics
            {
                ModelVersion = "v1.0.0",
                LastTrainingDate = DateTime.UtcNow.AddDays(-7), // Simulate last training
                PredictionAccuracy = CalculatePredictionAccuracy(implementedSuggestions),
                RecommendationRelevance = CalculateRecommendationRelevance(implementedSuggestions),
                ImprovementSuccessRate = CalculateImprovementSuccessRate(implementedSuggestions),
                PredictionsMade = recentSuggestions.Count,
                CorrectPredictions = implementedSuggestions.Count,
                MetricsByCategory = CalculateMetricsByCategory(recentSuggestions),
                ModelStrengths = GetModelStrengths(),
                ModelWeaknesses = GetModelWeaknesses(),
                LastEvaluationDate = DateTime.UtcNow
            };

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model performance metrics");
            throw;
        }
    }

    public async Task<byte[]> ExportImprovementSuggestionsAsync(DateTime startDate, DateTime endDate, ExportFormat format = ExportFormat.CSV, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Exporting improvement suggestions from {StartDate} to {EndDate} in {Format} format",
                startDate, endDate, format);

            var suggestions = await _improvementRepository.GetRecentSuggestionsAsync(
                (int)(endDate - startDate).TotalDays, cancellationToken);

            var filteredSuggestions = suggestions.Where(s => s.CreatedDate >= startDate && s.CreatedDate <= endDate).ToList();

            return format switch
            {
                ExportFormat.CSV => ExportToCsv(filteredSuggestions),
                ExportFormat.JSON => ExportToJson(filteredSuggestions),
                ExportFormat.Excel => ExportToExcel(filteredSuggestions),
                _ => throw new ArgumentException($"Unsupported export format: {format}")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting improvement suggestions");
            throw;
        }
    }

    public async Task<ReviewResult> ReviewImprovementSuggestionAsync(long suggestionId, SuggestionReviewAction action, string? reviewComments = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Reviewing improvement suggestion {SuggestionId} with action {Action}",
                suggestionId, action);

            var suggestion = await _improvementRepository.GetByIdAsync(suggestionId, cancellationToken);
            if (suggestion == null)
            {
                throw new ArgumentException($"Suggestion not found: {suggestionId}");
            }

            var newStatus = action switch
            {
                SuggestionReviewAction.Approve => "approved",
                SuggestionReviewAction.Reject => "rejected",
                SuggestionReviewAction.RequestChanges => "needs_changes",
                SuggestionReviewAction.ScheduleABTest => "scheduled_for_testing",
                _ => suggestion.Status
            };

            suggestion.Status = newStatus;
            suggestion.ReviewedBy = "current_user"; // Would get from auth context
            suggestion.ReviewedDate = DateTime.UtcNow;
            suggestion.ReviewComments = reviewComments;

            await _improvementRepository.UpdateAsync(suggestion, cancellationToken);

            var result = new ReviewResult
            {
                SuggestionId = suggestionId,
                Action = action,
                NewStatus = newStatus,
                ReviewedBy = suggestion.ReviewedBy,
                ReviewDate = suggestion.ReviewedDate.Value,
                Comments = reviewComments,
                Success = true,
                Message = $"Suggestion {action.ToString().ToLower()} successfully"
            };

            // If approved, consider creating A/B test
            if (action == SuggestionReviewAction.Approve || action == SuggestionReviewAction.ScheduleABTest)
            {
                await ConsiderABTestCreationAsync(suggestion, cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reviewing improvement suggestion {SuggestionId}", suggestionId);
            throw;
        }
    }

    #region Private Helper Methods

    private async Task<List<TemplateImprovementSuggestion>> AnalyzeSuccessRateAsync(
        EnhancedPromptTemplateEntity template,
        TemplatePerformanceMetricsEntity performance,
        CancellationToken cancellationToken)
    {
        var suggestions = new List<TemplateImprovementSuggestion>();

        if (performance.SuccessRate < 0.7m && performance.TotalUsages >= 50)
        {
            var suggestion = new TemplateImprovementSuggestion
            {
                TemplateKey = template.TemplateKey ?? string.Empty,
                TemplateName = template.Name ?? string.Empty,
                Type = ImprovementType.PerformanceOptimization,
                CurrentVersion = template.Version ?? "1.0",
                SuggestedChanges = JsonSerializer.Serialize(new
                {
                    type = "success_rate_improvement",
                    current_rate = performance.SuccessRate,
                    target_rate = 0.8m,
                    suggestions = new[]
                    {
                        "Add more specific instructions",
                        "Include better examples",
                        "Clarify expected output format"
                    }
                }),
                ReasoningExplanation = $"Template has a low success rate of {performance.SuccessRate:P2}. " +
                                     "Analysis suggests improving clarity and adding examples could increase success rate.",
                ExpectedImprovementPercent = 15m,
                BasedOnDataPoints = performance.TotalUsages,
                ConfidenceScore = 0.8m,
                Status = SuggestionStatus.Pending,
                CreatedDate = DateTime.UtcNow
            };

            suggestions.Add(suggestion);
        }

        return suggestions;
    }
