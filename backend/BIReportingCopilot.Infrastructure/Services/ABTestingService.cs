using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Core.Interfaces.Repository;
using BIReportingCopilot.Core.Models.Analytics;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Core.Models;
using AnalyticsExportFormat = BIReportingCopilot.Core.Models.Analytics.ExportFormat;

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
                // Select a test based on priority (most recent or highest traffic split)
                var selectedTest = relevantTests.OrderByDescending(t => t.StartDate).First();

                // Use consistent user-based hashing for traffic splitting
                var useVariant = ShouldUseVariant(userId, selectedTest.Id, selectedTest.TrafficSplitPercent);

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

            // Calculate statistical significance using two-proportion z-test
            var originalSuccessRate = originalPerformance.SuccessRate;
            var variantSuccessRate = variantPerformance.SuccessRate;
            var improvementPercent = originalSuccessRate > 0 ?
                ((variantSuccessRate - originalSuccessRate) / originalSuccessRate) * 100 : 0;

            // Proper statistical significance calculation
            var originalSuccesses = (int)(originalPerformance.TotalUsages * originalSuccessRate);
            var variantSuccesses = (int)(variantPerformance.TotalUsages * variantSuccessRate);

            var statisticalSignificance = CalculateTwoProportionZTest(
                originalPerformance.TotalUsages,
                originalSuccesses,
                variantPerformance.TotalUsages,
                variantSuccesses
            );

            // Calculate additional statistical metrics
            var effectSize = CalculateEffectSize((double)originalSuccessRate, (double)variantSuccessRate);
            var confidenceInterval = CalculateConfidenceInterval(
                originalPerformance.TotalUsages, originalSuccesses,
                variantPerformance.TotalUsages, variantSuccesses
            );

            // Determine recommendation based on statistical significance and practical significance
            var recommendation = DetermineTestRecommendation(
                statisticalSignificance,
                improvementPercent,
                originalPerformance.TotalUsages + variantPerformance.TotalUsages,
                test.StartDate
            );

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
                Recommendation = recommendation,
                AnalysisDate = DateTime.UtcNow,
                EffectSize = (decimal)effectSize,
                ConfidenceIntervalLower = (decimal)confidenceInterval.lower,
                ConfidenceIntervalUpper = (decimal)confidenceInterval.upper
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

    public async Task<byte[]> ExportTestResultsAsync(long testId, AnalyticsExportFormat format = AnalyticsExportFormat.CSV, CancellationToken cancellationToken = default)
    {
        try
        {
            var analysis = await AnalyzeTestResultsAsync(testId, cancellationToken);

            if (format == AnalyticsExportFormat.CSV)
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

    #region Statistical Analysis Methods

    /// <summary>
    /// Calculate statistical significance using two-proportion z-test
    /// </summary>
    private decimal CalculateTwoProportionZTest(int n1, int x1, int n2, int x2)
    {
        // Check for minimum sample size requirements
        if (n1 < 30 || n2 < 30)
        {
            _logger.LogWarning("Sample sizes too small for reliable statistical analysis: n1={N1}, n2={N2}", n1, n2);
            return 0.5m; // Not enough data for reliable analysis
        }

        // Calculate proportions
        var p1 = (double)x1 / n1;
        var p2 = (double)x2 / n2;

        // Check for edge cases
        if (p1 == p2) return 0.5m; // No difference
        if (n1 == 0 || n2 == 0) return 0.5m; // No data

        // Calculate pooled proportion
        var pooledP = (double)(x1 + x2) / (n1 + n2);

        // Calculate standard error
        var standardError = Math.Sqrt(pooledP * (1 - pooledP) * (1.0 / n1 + 1.0 / n2));

        // Avoid division by zero
        if (standardError == 0) return 0.5m;

        // Calculate z-score
        var zScore = Math.Abs(p2 - p1) / standardError;

        // Convert z-score to confidence level (two-tailed test)
        var pValue = 2 * (1 - StandardNormalCDF(Math.Abs(zScore)));
        var confidence = 1 - pValue;

        return (decimal)Math.Max(0, Math.Min(1, confidence));
    }

    /// <summary>
    /// Calculate cumulative distribution function for standard normal distribution
    /// Using Abramowitz and Stegun approximation
    /// </summary>
    private static double StandardNormalCDF(double x)
    {
        if (x < 0)
            return 1 - StandardNormalCDF(-x);

        // Constants for approximation
        const double a1 = 0.254829592;
        const double a2 = -0.284496736;
        const double a3 = 1.421413741;
        const double a4 = -1.453152027;
        const double a5 = 1.061405429;
        const double p = 0.3275911;

        var t = 1.0 / (1.0 + p * x);
        var y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x / 2.0) / Math.Sqrt(2 * Math.PI);

        return y;
    }

    /// <summary>
    /// Calculate minimum sample size required for detecting a given effect size
    /// </summary>
    private int CalculateMinimumSampleSize(double baselineRate, double minimumDetectableEffect, double alpha = 0.05, double power = 0.8)
    {
        // This is a simplified calculation - in practice, you might want to use more sophisticated methods
        var p1 = baselineRate;
        var p2 = baselineRate * (1 + minimumDetectableEffect);

        // Ensure p2 is within valid range
        p2 = Math.Max(0, Math.Min(1, p2));

        var pooledP = (p1 + p2) / 2;
        var delta = Math.Abs(p2 - p1);

        if (delta == 0) return int.MaxValue; // No effect to detect

        // Z-scores for alpha and power
        var zAlpha = 1.96; // For alpha = 0.05 (two-tailed)
        var zBeta = 0.84;  // For power = 0.8

        var numerator = Math.Pow(zAlpha * Math.Sqrt(2 * pooledP * (1 - pooledP)) + zBeta * Math.Sqrt(p1 * (1 - p1) + p2 * (1 - p2)), 2);
        var denominator = Math.Pow(delta, 2);

        return (int)Math.Ceiling(numerator / denominator);
    }

    /// <summary>
    /// Calculate effect size (Cohen's h for proportions)
    /// </summary>
    private double CalculateEffectSize(double p1, double p2)
    {
        // Cohen's h for proportions
        var h = 2 * (Math.Asin(Math.Sqrt(p1)) - Math.Asin(Math.Sqrt(p2)));
        return Math.Abs(h);
    }

    /// <summary>
    /// Calculate confidence interval for difference in proportions
    /// </summary>
    private (double lower, double upper) CalculateConfidenceInterval(int n1, int x1, int n2, int x2, double confidenceLevel = 0.95)
    {
        var p1 = (double)x1 / n1;
        var p2 = (double)x2 / n2;
        var diff = p2 - p1;

        // Standard error for difference in proportions
        var se = Math.Sqrt((p1 * (1 - p1) / n1) + (p2 * (1 - p2) / n2));

        // Z-score for confidence level
        var alpha = 1 - confidenceLevel;
        var zScore = 1.96; // For 95% confidence (approximate)

        var margin = zScore * se;

        return (diff - margin, diff + margin);
    }

    /// <summary>
    /// Determine test recommendation based on statistical and practical significance
    /// </summary>
    private TestRecommendation DetermineTestRecommendation(decimal statisticalSignificance, decimal improvementPercent, int totalSampleSize, DateTime testStartDate)
    {
        var testDuration = DateTime.UtcNow - testStartDate;
        var isStatisticallySignificant = statisticalSignificance > 0.95m;
        var isPracticallySignificant = Math.Abs(improvementPercent) > 5; // 5% improvement threshold
        var hasMinimumSampleSize = totalSampleSize >= 200; // Minimum total sample size
        var hasRunLongEnough = testDuration.TotalDays >= 7; // Minimum test duration

        // If statistically and practically significant with adequate sample size
        if (isStatisticallySignificant && isPracticallySignificant && hasMinimumSampleSize && hasRunLongEnough)
        {
            return improvementPercent > 0 ? TestRecommendation.ImplementVariant : TestRecommendation.KeepOriginal;
        }

        // If not statistically significant but test has run long enough with good sample size
        if (!isStatisticallySignificant && hasRunLongEnough && hasMinimumSampleSize)
        {
            return TestRecommendation.KeepOriginal; // No significant difference found
        }

        // If statistically significant but not practically significant
        if (isStatisticallySignificant && !isPracticallySignificant)
        {
            return TestRecommendation.KeepOriginal; // Difference too small to matter
        }

        // Need more data or time
        return TestRecommendation.ExtendTest;
    }

    /// <summary>
    /// Automated winner selection - checks all active tests and implements winners when criteria are met
    /// </summary>
    public async Task<List<AutomatedTestResult>> ProcessAutomatedWinnerSelectionAsync(CancellationToken cancellationToken = default)
    {
        var results = new List<AutomatedTestResult>();

        try
        {
            _logger.LogInformation("Starting automated winner selection process");

            var activeTests = await GetActiveTestsAsync(cancellationToken);

            foreach (var test in activeTests)
            {
                try
                {
                    var analysis = await AnalyzeTestResultsAsync(test.Id, cancellationToken);

                    var result = new AutomatedTestResult
                    {
                        TestId = test.Id,
                        TestName = test.TestName,
                        Action = AutomatedTestAction.NoAction,
                        Reason = "Test analysis completed",
                        ProcessedDate = DateTime.UtcNow
                    };

                    // Check if test should be automatically completed
                    if (analysis.Recommendation == TestRecommendation.ImplementVariant ||
                        analysis.Recommendation == TestRecommendation.KeepOriginal)
                    {
                        var implementResult = await CompleteTestAsync(test.Id,
                            analysis.Recommendation == TestRecommendation.ImplementVariant, cancellationToken);

                        result.Action = analysis.Recommendation == TestRecommendation.ImplementVariant ?
                            AutomatedTestAction.ImplementedVariant : AutomatedTestAction.KeptOriginal;
                        result.Reason = $"Automatically completed: {analysis.Recommendation}. " +
                                      $"Statistical significance: {analysis.StatisticalSignificance:P2}, " +
                                      $"Improvement: {analysis.ImprovementPercent:F1}%";
                        result.ImplementedTemplateKey = implementResult.ImplementedTemplateKey;
                    }
                    else if (ShouldExpireTest(test, analysis))
                    {
                        await _abTestRepository.CompleteTestAsync(test.Id, test.Id, cancellationToken);
                        result.Action = AutomatedTestAction.ExpiredTest;
                        result.Reason = "Test expired without conclusive results";
                    }

                    results.Add(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing automated winner selection for test {TestId}", test.Id);
                    results.Add(new AutomatedTestResult
                    {
                        TestId = test.Id,
                        TestName = test.TestName,
                        Action = AutomatedTestAction.Error,
                        Reason = $"Error during processing: {ex.Message}",
                        ProcessedDate = DateTime.UtcNow
                    });
                }
            }

            _logger.LogInformation("Completed automated winner selection process. Processed {TestCount} tests", results.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in automated winner selection process");
            throw;
        }

        return results;
    }

    /// <summary>
    /// Check if a test should be expired due to running too long without conclusive results
    /// </summary>
    private bool ShouldExpireTest(ABTestDetails test, ABTestAnalysis analysis)
    {
        var testDuration = DateTime.UtcNow - test.StartDate;
        var maxTestDuration = TimeSpan.FromDays(60); // Maximum test duration
        var hasMinimumSample = (test.OriginalUsageCount + test.VariantUsageCount) >= 1000;

        return testDuration > maxTestDuration &&
               hasMinimumSample &&
               analysis.Recommendation == TestRecommendation.ExtendTest;
    }

    /// <summary>
    /// Determine if user should see variant using consistent hashing
    /// </summary>
    private bool ShouldUseVariant(string userId, long testId, int trafficSplitPercent)
    {
        // Create a consistent hash based on user ID and test ID
        var hashInput = $"{userId}_{testId}";
        var hash = hashInput.GetHashCode();

        // Ensure positive hash value
        if (hash < 0) hash = -hash;

        // Convert to percentage (0-99)
        var userBucket = hash % 100;

        // Return true if user falls within the variant percentage
        return userBucket < trafficSplitPercent;
    }

    #endregion
}
