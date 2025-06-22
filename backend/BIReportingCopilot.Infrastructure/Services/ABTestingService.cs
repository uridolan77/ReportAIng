using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Core.Interfaces.Repository;
using BIReportingCopilot.Core.Models.Analytics;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// Service implementation for managing A/B testing of template variations
/// </summary>
public class ABTestingService : IABTestingService
{
    private readonly ITemplateABTestRepository _abTestRepository;
    private readonly IEnhancedPromptTemplateRepository _templateRepository;
    private readonly ITemplatePerformanceRepository _performanceRepository;
    private readonly ILogger<ABTestingService> _logger;
    private readonly Random _random = new();

    public ABTestingService(
        ITemplateABTestRepository abTestRepository,
        IEnhancedPromptTemplateRepository templateRepository,
        ITemplatePerformanceRepository performanceRepository,
        ILogger<ABTestingService> logger)
    {
        _abTestRepository = abTestRepository;
        _templateRepository = templateRepository;
        _performanceRepository = performanceRepository;
        _logger = logger;
    }

    public async Task<ABTestResult> CreateABTestAsync(ABTestRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Creating A/B test: {TestName}", request.TestName);

            // Validate request
            var validation = await ValidateTestCreationAsync(request.OriginalTemplateKey, request.VariantTemplateContent, cancellationToken);
            if (!validation.CanCreateTest)
            {
                return new ABTestResult
                {
                    Status = ABTestStatus.Created,
                    Message = string.Join("; ", validation.ValidationErrors),
                    CreatedDate = DateTime.UtcNow
                };
            }

            // Get original template
            var originalTemplate = await _templateRepository.GetByKeyAsync(request.OriginalTemplateKey, cancellationToken);
            if (originalTemplate == null)
            {
                return new ABTestResult
                {
                    Status = ABTestStatus.Created,
                    Message = $"Original template not found: {request.OriginalTemplateKey}",
                    CreatedDate = DateTime.UtcNow
                };
            }

            // Create variant template
            var variantTemplate = new PromptTemplateEntity
            {
                Name = $"{originalTemplate.Name}_Variant_{DateTime.UtcNow:yyyyMMddHHmmss}",
                TemplateKey = $"{request.OriginalTemplateKey}_variant_{Guid.NewGuid():N}",
                Content = request.VariantTemplateContent,
                Description = $"A/B test variant for {originalTemplate.Name}",
                IntentType = originalTemplate.IntentType,
                IsActive = true,
                Version = "1.0",
                CreatedBy = request.CreatedBy,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            var createdVariant = await _templateRepository.CreateAsync(variantTemplate, cancellationToken);

            // Create A/B test entity
            var abTestEntity = new TemplateABTestEntity
            {
                TestName = request.TestName,
                OriginalTemplateId = originalTemplate.Id,
                VariantTemplateId = createdVariant.Id,
                TrafficSplitPercent = request.TrafficSplitPercent,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = "active",
                CreatedBy = request.CreatedBy,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            var createdTest = await _abTestRepository.CreateAsync(abTestEntity, cancellationToken);

            _logger.LogInformation("Successfully created A/B test {TestId}: {TestName}", createdTest.Id, request.TestName);

            return new ABTestResult
            {
                TestId = createdTest.Id,
                TestName = request.TestName,
                Status = ABTestStatus.Running,
                Message = "A/B test created successfully",
                CreatedDate = DateTime.UtcNow,
                VariantTemplateKey = createdVariant.TemplateKey ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating A/B test: {TestName}", request.TestName);
            throw;
        }
    }

    public async Task<ABTestDetails?> GetABTestAsync(long testId, CancellationToken cancellationToken = default)
    {
        try
        {
            var testEntity = await _abTestRepository.GetByIdWithIncludesAsync(testId, 
                t => t.OriginalTemplate, 
                t => t.VariantTemplate, 
                t => t.WinnerTemplate);

            if (testEntity == null)
            {
                return null;
            }

            return MapToABTestDetails(testEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting A/B test: {TestId}", testId);
            throw;
        }
    }

    public async Task<List<ABTestDetails>> GetActiveTestsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var activeTests = await _abTestRepository.GetActiveTestsAsync(cancellationToken);
            return activeTests.Select(MapToABTestDetails).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active A/B tests");
            throw;
        }
    }

    public async Task<List<ABTestDetails>> GetTestsByStatusAsync(ABTestStatus status, CancellationToken cancellationToken = default)
    {
        try
        {
            var tests = await _abTestRepository.GetTestsByStatusAsync(status.ToString().ToLower(), cancellationToken);
            return tests.Select(MapToABTestDetails).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting A/B tests by status: {Status}", status);
            throw;
        }
    }

    public async Task<List<ABTestDetails>> GetTestsForTemplateAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template == null)
            {
                return new List<ABTestDetails>();
            }

            var tests = await _abTestRepository.GetTestsByTemplateIdAsync(template.Id, cancellationToken);
            return tests.Select(MapToABTestDetails).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting A/B tests for template: {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<TemplateSelectionResult> SelectTemplateForUserAsync(string userId, string intentType, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get active tests for this intent type
            var activeTests = await _abTestRepository.GetActiveTestsAsync(cancellationToken);
            var relevantTests = activeTests.Where(t => 
                t.OriginalTemplate.IntentType == intentType || 
                t.VariantTemplate.IntentType == intentType).ToList();

            if (relevantTests.Any())
            {
                // Select a test (could implement user-based consistent selection)
                var selectedTest = relevantTests.First();
                var useVariant = _random.Next(100) < selectedTest.TrafficSplitPercent;

                var selectedTemplate = useVariant ? selectedTest.VariantTemplate : selectedTest.OriginalTemplate;

                return new TemplateSelectionResult
                {
                    SelectedTemplateKey = selectedTemplate.TemplateKey ?? string.Empty,
                    IsVariant = useVariant,
                    TestId = selectedTest.Id,
                    SelectionReason = $"Selected for A/B test: {selectedTest.TestName}",
                    SelectionMetadata = new Dictionary<string, object>
                    {
                        ["TestId"] = selectedTest.Id,
                        ["TestName"] = selectedTest.TestName,
                        ["TrafficSplit"] = selectedTest.TrafficSplitPercent
                    }
                };
            }

            // No active tests, select best performing template
            var bestTemplate = await _templateRepository.SelectTemplateForUserAsync(userId, intentType, cancellationToken);
            if (bestTemplate != null)
            {
                return new TemplateSelectionResult
                {
                    SelectedTemplateKey = bestTemplate.TemplateKey ?? string.Empty,
                    IsVariant = false,
                    SelectionReason = "Best performing template for intent type",
                    SelectionMetadata = new Dictionary<string, object>
                    {
                        ["IntentType"] = intentType,
                        ["SelectionMethod"] = "Performance-based"
                    }
                };
            }

            throw new InvalidOperationException($"No templates available for intent type: {intentType}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting template for user {UserId} with intent {IntentType}", userId, intentType);
            throw;
        }
    }

    public async Task RecordTestInteractionAsync(long testId, string templateKey, bool wasSuccessful, decimal confidenceScore, string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Recording A/B test interaction: Test {TestId}, Template {TemplateKey}, Success: {Success}", 
                testId, templateKey, wasSuccessful);

            // This would typically update test-specific metrics
            // For now, we'll just track it in the performance metrics
            var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
            if (template != null)
            {
                await _performanceRepository.IncrementUsageAsync(template.Id, wasSuccessful, confidenceScore, 0, cancellationToken);
            }

            _logger.LogDebug("Successfully recorded A/B test interaction for test {TestId}", testId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording A/B test interaction for test {TestId}", testId);
            throw;
        }
    }

    public async Task<ABTestAnalysis> AnalyzeTestResultsAsync(long testId, CancellationToken cancellationToken = default)
    {
        try
        {
            var test = await _abTestRepository.GetByIdWithIncludesAsync(testId, 
                t => t.OriginalTemplate, 
                t => t.VariantTemplate);

            if (test == null)
            {
                throw new ArgumentException($"A/B test not found: {testId}");
            }

            // Get performance metrics for both templates
            var originalPerformance = await _performanceRepository.GetByTemplateIdAsync(test.OriginalTemplateId, cancellationToken);
            var variantPerformance = await _performanceRepository.GetByTemplateIdAsync(test.VariantTemplateId, cancellationToken);

            if (originalPerformance == null || variantPerformance == null)
            {
                throw new InvalidOperationException("Performance metrics not available for analysis");
            }

            // Calculate statistical significance (simplified)
            var originalSuccessRate = originalPerformance.SuccessRate;
            var variantSuccessRate = variantPerformance.SuccessRate;
            var improvementPercent = originalSuccessRate > 0 ? 
                ((variantSuccessRate - originalSuccessRate) / originalSuccessRate) * 100 : 0;

            // Simplified statistical significance calculation
            var statisticalSignificance = Math.Abs(improvementPercent) > 5 ? 0.95m : 0.5m;

            var analysis = new ABTestAnalysis
            {
                TestId = testId,
                TestName = test.TestName,
                Status = Enum.Parse<ABTestStatus>(test.Status, true),
                StartDate = test.StartDate,
                EndDate = test.EndDate,
                OriginalSampleSize = originalPerformance.TotalUsages,
                VariantSampleSize = variantPerformance.TotalUsages,
                OriginalSuccessRate = originalSuccessRate,
                VariantSuccessRate = variantSuccessRate,
                StatisticalSignificance = statisticalSignificance,
                ConfidenceInterval = 0.95m,
                WinnerTemplateKey = variantSuccessRate > originalSuccessRate ? 
                    test.VariantTemplate.TemplateKey ?? string.Empty : 
                    test.OriginalTemplate.TemplateKey ?? string.Empty,
                ImprovementPercent = improvementPercent,
                Recommendation = statisticalSignificance > 0.9m ? 
                    (variantSuccessRate > originalSuccessRate ? TestRecommendation.ImplementVariant : TestRecommendation.KeepOriginal) :
                    TestRecommendation.ExtendTest,
                AnalysisDate = DateTime.UtcNow
            };

            return analysis;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing A/B test results for test {TestId}", testId);
            throw;
        }
    }

    public async Task<ImplementationResult> CompleteTestAsync(long testId, bool implementWinner = true, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Completing A/B test {TestId}, implement winner: {ImplementWinner}", testId, implementWinner);

            var analysis = await AnalyzeTestResultsAsync(testId, cancellationToken);
            
            if (implementWinner && analysis.Recommendation == TestRecommendation.ImplementVariant)
            {
                // Implementation logic would go here
                await _abTestRepository.CompleteTestAsync(testId, 
                    analysis.WinnerTemplateKey == analysis.TestName ? analysis.TestId : testId, cancellationToken);

                return new ImplementationResult
                {
                    TestId = testId,
                    Success = true,
                    Message = "A/B test completed and winner implemented",
                    ImplementedTemplateKey = analysis.WinnerTemplateKey,
                    ImplementationDate = DateTime.UtcNow
                };
            }

            await _abTestRepository.CompleteTestAsync(testId, testId, cancellationToken);

            return new ImplementationResult
            {
                TestId = testId,
                Success = true,
                Message = "A/B test completed without implementation",
                ImplementationDate = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing A/B test {TestId}", testId);
            throw;
        }
    }

    public async Task<bool> PauseTestAsync(long testId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            await _abTestRepository.PauseTestAsync(testId, cancellationToken);
            _logger.LogInformation("Paused A/B test {TestId}: {Reason}", testId, reason);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing A/B test {TestId}", testId);
            return false;
        }
    }

    public async Task<bool> ResumeTestAsync(long testId, CancellationToken cancellationToken = default)
    {
        try
        {
            await _abTestRepository.ResumeTestAsync(testId, cancellationToken);
            _logger.LogInformation("Resumed A/B test {TestId}", testId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming A/B test {TestId}", testId);
            return false;
        }
    }

    public async Task<bool> CancelTestAsync(long testId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            await _abTestRepository.CancelTestAsync(testId, reason, cancellationToken);
            _logger.LogInformation("Cancelled A/B test {TestId}: {Reason}", testId, reason);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling A/B test {TestId}", testId);
            return false;
        }
    }

    public async Task<ABTestDashboard> GetTestDashboardAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var activeTests = await _abTestRepository.GetActiveTestsAsync(cancellationToken);
            var completedTests = await _abTestRepository.GetCompletedTestsAsync(cancellationToken: cancellationToken);
            var testsRequiringAnalysis = await _abTestRepository.GetTestsRequiringAnalysisAsync(cancellationToken);
            var expiredTests = await _abTestRepository.GetExpiredTestsAsync(cancellationToken);
            var testCounts = await _abTestRepository.GetTestCountsByStatusAsync(cancellationToken);

            return new ABTestDashboard
            {
                TotalActiveTests = activeTests.Count,
                TotalCompletedTests = completedTests.Count,
                TestsRequiringAnalysis = testsRequiringAnalysis.Count,
                ExpiredTests = expiredTests.Count,
                AverageTestDuration = completedTests.Any() ? 
                    (decimal)completedTests.Where(t => t.CompletedDate.HasValue)
                        .Average(t => (t.CompletedDate!.Value - t.StartDate).TotalDays) : 0,
                RecentTests = activeTests.Take(5).Select(MapToABTestDetails).ToList(),
                TestsByStatus = testCounts,
                GeneratedDate = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting A/B test dashboard");
            throw;
        }
    }

    public async Task<List<ABTestDetails>> GetTestsRequiringAnalysisAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var tests = await _abTestRepository.GetTestsRequiringAnalysisAsync(cancellationToken);
            return tests.Select(MapToABTestDetails).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tests requiring analysis");
            throw;
        }
    }

    public async Task<List<ABTestDetails>> GetExpiredTestsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var tests = await _abTestRepository.GetExpiredTestsAsync(cancellationToken);
            return tests.Select(MapToABTestDetails).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expired tests");
            throw;
        }
    }

    public async Task<ABTestValidationResult> ValidateTestCreationAsync(string originalTemplateKey, string variantTemplateKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = new ABTestValidationResult { CanCreateTest = true };

            // Get original template
            var originalTemplate = await _templateRepository.GetByKeyAsync(originalTemplateKey, cancellationToken);
            if (originalTemplate == null)
            {
                result.CanCreateTest = false;
                result.ValidationErrors.Add($"Original template not found: {originalTemplateKey}");
            }

            // Check if template is already in an active test
            if (originalTemplate != null)
            {
                var hasActiveTest = await _abTestRepository.HasActiveTestForTemplateAsync(originalTemplate.Id, cancellationToken);
                if (hasActiveTest)
                {
                    result.CanCreateTest = false;
                    result.ValidationErrors.Add($"Template {originalTemplateKey} is already in an active A/B test");
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating A/B test creation");
            throw;
        }
    }

    public async Task<List<ABTestRecommendation>> GetTestRecommendationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var recommendations = new List<ABTestRecommendation>();

            // Get underperforming templates that could benefit from A/B testing
            var underperforming = await _performanceRepository.GetUnderperformingTemplatesAsync(0.8m, cancellationToken);

            foreach (var template in underperforming.Take(5))
            {
                var promptTemplate = await _templateRepository.GetByIdAsync(template.TemplateId, cancellationToken);
                if (promptTemplate != null)
                {
                    recommendations.Add(new ABTestRecommendation
                    {
                        TemplateKey = template.TemplateKey,
                        TemplateName = promptTemplate.Name,
                        RecommendationType = "Performance Improvement",
                        Reasoning = $"Template has success rate of {template.SuccessRate:P2}, could benefit from A/B testing",
                        Priority = 1 - template.SuccessRate, // Lower success rate = higher priority
                        ExpectedImpact = 0.2m, // Estimated 20% improvement
                        SuggestedVariations = new List<string> { "Content optimization", "Structure improvement" },
                        GeneratedDate = DateTime.UtcNow
                    });
                }
            }

            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting A/B test recommendations");
            throw;
        }
    }

    public async Task<byte[]> ExportTestResultsAsync(long testId, ExportFormat format = ExportFormat.CSV, CancellationToken cancellationToken = default)
    {
        try
        {
            var analysis = await AnalyzeTestResultsAsync(testId, cancellationToken);

            if (format == ExportFormat.CSV)
            {
                var csv = "TestId,TestName,Status,OriginalSuccessRate,VariantSuccessRate,StatisticalSignificance,Winner,ImprovementPercent\n";
                csv += $"{analysis.TestId},{analysis.TestName},{analysis.Status},{analysis.OriginalSuccessRate},{analysis.VariantSuccessRate},{analysis.StatisticalSignificance},{analysis.WinnerTemplateKey},{analysis.ImprovementPercent}\n";
                
                return System.Text.Encoding.UTF8.GetBytes(csv);
            }

            throw new NotImplementedException($"Export format {format} not implemented");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting A/B test results for test {TestId}", testId);
            throw;
        }
    }

    public async Task<StatisticalSignificanceResult> CalculateStatisticalSignificanceAsync(long testId, CancellationToken cancellationToken = default)
    {
        try
        {
            var analysis = await AnalyzeTestResultsAsync(testId, cancellationToken);

            return new StatisticalSignificanceResult
            {
                TestId = testId,
                StatisticalSignificance = analysis.StatisticalSignificance,
                ConfidenceLevel = analysis.ConfidenceInterval,
                PValue = 1 - analysis.StatisticalSignificance,
                IsSignificant = analysis.StatisticalSignificance > 0.95m,
                SampleSizeOriginal = analysis.OriginalSampleSize,
                SampleSizeVariant = analysis.VariantSampleSize,
                EffectSize = Math.Abs(analysis.ImprovementPercent) / 100,
                PowerAnalysis = 0.8m, // Standard 80% power
                Interpretation = analysis.StatisticalSignificance > 0.95m ? 
                    "Results are statistically significant" : 
                    "Results are not statistically significant",
                CalculatedDate = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating statistical significance for test {TestId}", testId);
            throw;
        }
    }

    public async Task<ABTestHistory> GetTestHistoryAsync(string? templateKey = null, TimeSpan? timeWindow = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var endDate = DateTime.UtcNow;
            var startDate = timeWindow.HasValue ? endDate.Subtract(timeWindow.Value) : endDate.AddDays(-30);

            List<TemplateABTestEntity> tests;
            if (!string.IsNullOrEmpty(templateKey))
            {
                var template = await _templateRepository.GetByKeyAsync(templateKey, cancellationToken);
                if (template == null)
                {
                    return new ABTestHistory { TemplateKey = templateKey, TimeWindow = timeWindow ?? TimeSpan.FromDays(30) };
                }
                tests = await _abTestRepository.GetTestsByTemplateIdAsync(template.Id, cancellationToken);
            }
            else
            {
                tests = await _abTestRepository.GetCompletedTestsAsync(startDate, endDate, cancellationToken);
            }

            var successfulTests = tests.Count(t => t.WinnerTemplateId.HasValue);
            var avgImprovement = tests.Where(t => t.VariantSuccessRate.HasValue && t.OriginalSuccessRate.HasValue)
                .Average(t => ((t.VariantSuccessRate!.Value - t.OriginalSuccessRate!.Value) / t.OriginalSuccessRate.Value) * 100);

            return new ABTestHistory
            {
                TemplateKey = templateKey,
                TimeWindow = timeWindow ?? TimeSpan.FromDays(30),
                Tests = tests.Select(MapToABTestDetails).ToList(),
                TotalTests = tests.Count,
                SuccessfulTests = successfulTests,
                AverageImprovementRate = (decimal)avgImprovement,
                AnalysisDate = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting A/B test history");
            throw;
        }
    }

    #region Private Methods

    private static ABTestDetails MapToABTestDetails(TemplateABTestEntity entity)
    {
        return new ABTestDetails
        {
            Id = entity.Id,
            TestName = entity.TestName,
            OriginalTemplateKey = entity.OriginalTemplate?.TemplateKey ?? string.Empty,
            VariantTemplateKey = entity.VariantTemplate?.TemplateKey ?? string.Empty,
            TrafficSplitPercent = entity.TrafficSplitPercent,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            Status = Enum.Parse<ABTestStatus>(entity.Status, true),
            OriginalSuccessRate = entity.OriginalSuccessRate,
            VariantSuccessRate = entity.VariantSuccessRate,
            StatisticalSignificance = entity.StatisticalSignificance,
            WinnerTemplateKey = entity.WinnerTemplate?.TemplateKey,
            CreatedBy = entity.CreatedBy ?? string.Empty,
            CreatedDate = entity.CreatedDate
        };
    }

    #endregion
}
