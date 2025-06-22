using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Core.Interfaces.Repositories;
using BIReportingCopilot.Core.Models.Analytics;
using BIReportingCopilot.Core.Models.TemplateEntities;
using BIReportingCopilot.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BIReportingCopilot.Tests.Unit.Services;

public class TemplateImprovementServiceTests
{
    private readonly Mock<ITemplateImprovementRepository> _mockImprovementRepository;
    private readonly Mock<IEnhancedPromptTemplateRepository> _mockTemplateRepository;
    private readonly Mock<ITemplatePerformanceRepository> _mockPerformanceRepository;
    private readonly Mock<ITemplateABTestRepository> _mockABTestRepository;
    private readonly Mock<IABTestingService> _mockABTestingService;
    private readonly Mock<ILogger<TemplateImprovementService>> _mockLogger;
    private readonly TemplateImprovementService _service;

    public TemplateImprovementServiceTests()
    {
        _mockImprovementRepository = new Mock<ITemplateImprovementRepository>();
        _mockTemplateRepository = new Mock<IEnhancedPromptTemplateRepository>();
        _mockPerformanceRepository = new Mock<ITemplatePerformanceRepository>();
        _mockABTestRepository = new Mock<ITemplateABTestRepository>();
        _mockABTestingService = new Mock<IABTestingService>();
        _mockLogger = new Mock<ILogger<TemplateImprovementService>>();

        _service = new TemplateImprovementService(
            _mockImprovementRepository.Object,
            _mockTemplateRepository.Object,
            _mockPerformanceRepository.Object,
            _mockABTestRepository.Object,
            _mockABTestingService.Object,
            _mockLogger.Object);
    }

    [Fact]
    public async Task AnalyzeTemplatePerformanceAsync_WithUnderperformingTemplate_GeneratesSuggestions()
    {
        // Arrange
        var templateKey = "test_template";
        var template = new EnhancedPromptTemplateEntity
        {
            Id = 1,
            TemplateKey = templateKey,
            Name = "Test Template",
            Content = "Simple template content",
            IntentType = "test_intent",
            Version = "1.0"
        };

        var performance = new TemplatePerformanceMetricsEntity
        {
            TemplateId = 1,
            TemplateKey = templateKey,
            TotalUsages = 100,
            SuccessfulUsages = 60,
            SuccessRate = 0.6m, // Below threshold
            AverageConfidenceScore = 0.5m, // Low confidence
            AverageProcessingTimeMs = 6000, // Slow response
            AverageUserRating = 3.0m // Low rating
        };

        _mockTemplateRepository.Setup(x => x.GetByKeyAsync(templateKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _mockPerformanceRepository.Setup(x => x.GetByTemplateIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(performance);
        _mockImprovementRepository.Setup(x => x.CreateAsync(It.IsAny<TemplateImprovementSuggestionEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TemplateImprovementSuggestionEntity { Id = 1 });

        // Act
        var suggestions = await _service.AnalyzeTemplatePerformanceAsync(templateKey);

        // Assert
        Assert.NotNull(suggestions);
        Assert.NotEmpty(suggestions);
        
        // Should generate suggestions for low success rate, confidence, response time, and user rating
        Assert.Contains(suggestions, s => s.Type == ImprovementType.PerformanceOptimization);
        Assert.Contains(suggestions, s => s.Type == ImprovementType.ContentOptimization);
        Assert.All(suggestions, s => Assert.True(s.ExpectedImprovementPercent > 0));
    }

    [Fact]
    public async Task OptimizeTemplateAsync_WithPerformanceStrategy_AppliesOptimizations()
    {
        // Arrange
        var templateKey = "test_template";
        var template = new EnhancedPromptTemplateEntity
        {
            Id = 1,
            TemplateKey = templateKey,
            Content = "This is a very long template with lots of complex instructions that might be too verbose for optimal performance and could benefit from simplification to improve response times.",
            IntentType = "test_intent"
        };

        var performance = new TemplatePerformanceMetricsEntity
        {
            TemplateId = 1,
            SuccessRate = 0.7m,
            AverageConfidenceScore = 0.6m,
            AverageProcessingTimeMs = 5000
        };

        _mockTemplateRepository.Setup(x => x.GetByKeyAsync(templateKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _mockPerformanceRepository.Setup(x => x.GetByTemplateIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(performance);

        // Act
        var optimizedTemplate = await _service.OptimizeTemplateAsync(templateKey, OptimizationStrategy.PerformanceFocused);

        // Assert
        Assert.NotNull(optimizedTemplate);
        Assert.Equal(templateKey, optimizedTemplate.OriginalTemplateKey);
        Assert.Equal(OptimizationStrategy.PerformanceFocused, optimizedTemplate.StrategyUsed);
        Assert.True(optimizedTemplate.OptimizedContent.Length <= template.Content.Length);
        Assert.NotEmpty(optimizedTemplate.ChangesApplied);
        Assert.True(optimizedTemplate.ExpectedPerformanceImprovement > 0);
    }

    [Fact]
    public async Task PredictTemplatePerformanceAsync_WithNewTemplate_ReturnsReasonablePrediction()
    {
        // Arrange
        var templateContent = "Analyze the data and provide insights. Be specific and accurate.";
        var intentType = "data_analysis";

        var similarTemplate = new EnhancedPromptTemplateEntity
        {
            Id = 1,
            Content = "Analyze data carefully",
            IntentType = intentType
        };

        var similarPerformance = new TemplatePerformanceMetricsEntity
        {
            TemplateId = 1,
            SuccessRate = 0.8m,
            AverageUserRating = 4.2m,
            AverageProcessingTimeMs = 3000
        };

        _mockTemplateRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EnhancedPromptTemplateEntity> { similarTemplate });
        _mockPerformanceRepository.Setup(x => x.GetByTemplateIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(similarPerformance);

        // Act
        var prediction = await _service.PredictTemplatePerformanceAsync(templateContent, intentType);

        // Assert
        Assert.NotNull(prediction);
        Assert.Equal(templateContent, prediction.TemplateContent);
        Assert.Equal(intentType, prediction.IntentType);
        Assert.True(prediction.PredictedSuccessRate >= 0 && prediction.PredictedSuccessRate <= 1);
        Assert.True(prediction.PredictedUserRating >= 1 && prediction.PredictedUserRating <= 5);
        Assert.True(prediction.PredictedResponseTime > 0);
        Assert.True(prediction.PredictionConfidence >= 0 && prediction.PredictionConfidence <= 1);
        Assert.NotEmpty(prediction.FeatureScores);
    }

    [Fact]
    public async Task AnalyzeTemplateContentQualityAsync_WithPoorContent_IdentifiesIssues()
    {
        // Arrange
        var poorContent = "bad"; // Very short, poor quality content

        // Act
        var analysis = await _service.AnalyzeTemplateContentQualityAsync(poorContent);

        // Assert
        Assert.NotNull(analysis);
        Assert.Equal(poorContent, analysis.TemplateContent);
        Assert.True(analysis.OverallQualityScore < 0.7m); // Should be low quality
        Assert.NotEmpty(analysis.IdentifiedIssues);
        Assert.Contains(analysis.IdentifiedIssues, i => i.IssueType == "Length");
        Assert.NotEmpty(analysis.ImprovementSuggestions);
        Assert.NotNull(analysis.Readability);
        Assert.NotNull(analysis.Structure);
        Assert.NotNull(analysis.Completeness);
    }

    [Fact]
    public async Task GenerateTemplateVariantsAsync_WithTemplate_CreatesVariants()
    {
        // Arrange
        var templateKey = "test_template";
        var template = new EnhancedPromptTemplateEntity
        {
            Id = 1,
            TemplateKey = templateKey,
            Content = "Original template content with instructions",
            IntentType = "test_intent"
        };

        var performance = new TemplatePerformanceMetricsEntity
        {
            TemplateId = 1,
            SuccessRate = 0.7m
        };

        _mockTemplateRepository.Setup(x => x.GetByKeyAsync(templateKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _mockPerformanceRepository.Setup(x => x.GetByTemplateIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(performance);

        // Act
        var variants = await _service.GenerateTemplateVariantsAsync(templateKey, 3);

        // Assert
        Assert.NotNull(variants);
        Assert.Equal(3, variants.Count);
        Assert.All(variants, v => 
        {
            Assert.Equal(templateKey, v.OriginalTemplateKey);
            Assert.NotEqual(template.Content, v.VariantContent);
            Assert.True(v.ExpectedPerformanceChange >= 0);
            Assert.True(v.ConfidenceScore > 0);
        });
    }

    [Fact]
    public async Task ReviewImprovementSuggestionAsync_WithApprovalAction_UpdatesStatus()
    {
        // Arrange
        var suggestionId = 1L;
        var suggestion = new TemplateImprovementSuggestionEntity
        {
            Id = suggestionId,
            TemplateKey = "test_template",
            Status = "pending",
            ExpectedImprovementPercent = 15m
        };

        _mockImprovementRepository.Setup(x => x.GetByIdAsync(suggestionId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(suggestion);
        _mockImprovementRepository.Setup(x => x.UpdateAsync(It.IsAny<TemplateImprovementSuggestionEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(suggestion);

        // Act
        var result = await _service.ReviewImprovementSuggestionAsync(
            suggestionId, 
            SuggestionReviewAction.Approve, 
            "Looks good");

        // Assert
        Assert.NotNull(result);
        Assert.Equal(suggestionId, result.SuggestionId);
        Assert.Equal(SuggestionReviewAction.Approve, result.Action);
        Assert.Equal("approved", result.NewStatus);
        Assert.True(result.Success);
        Assert.Equal("Looks good", result.Comments);
    }

    [Theory]
    [InlineData(OptimizationStrategy.PerformanceFocused)]
    [InlineData(OptimizationStrategy.AccuracyFocused)]
    [InlineData(OptimizationStrategy.UserSatisfactionFocused)]
    [InlineData(OptimizationStrategy.ResponseTimeFocused)]
    [InlineData(OptimizationStrategy.Balanced)]
    public async Task OptimizeTemplateAsync_WithDifferentStrategies_AppliesAppropriateOptimizations(OptimizationStrategy strategy)
    {
        // Arrange
        var templateKey = "test_template";
        var template = new EnhancedPromptTemplateEntity
        {
            Id = 1,
            TemplateKey = templateKey,
            Content = "Template content for optimization testing",
            IntentType = "test_intent"
        };

        _mockTemplateRepository.Setup(x => x.GetByKeyAsync(templateKey, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);
        _mockPerformanceRepository.Setup(x => x.GetByTemplateIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TemplatePerformanceMetricsEntity { TemplateId = 1 });

        // Act
        var optimizedTemplate = await _service.OptimizeTemplateAsync(templateKey, strategy);

        // Assert
        Assert.NotNull(optimizedTemplate);
        Assert.Equal(strategy, optimizedTemplate.StrategyUsed);
        Assert.NotEmpty(optimizedTemplate.ChangesApplied);
        Assert.Contains(optimizedTemplate.OptimizationReasoning, strategy.ToString());
    }
}
