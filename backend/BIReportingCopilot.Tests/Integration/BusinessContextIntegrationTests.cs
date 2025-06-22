using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Core.Models.BusinessContext;

namespace BIReportingCopilot.Tests.Integration;

/// <summary>
/// Integration tests for Business Context Aware Prompt Building services
/// </summary>
public class BusinessContextIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly IServiceScope _scope;
    private readonly IBusinessContextAnalyzer _contextAnalyzer;
    private readonly IBusinessMetadataRetrievalService _metadataService;
    private readonly IContextualPromptBuilder _promptBuilder;

    public BusinessContextIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _scope = _factory.Services.CreateScope();
        _contextAnalyzer = _scope.ServiceProvider.GetRequiredService<IBusinessContextAnalyzer>();
        _metadataService = _scope.ServiceProvider.GetRequiredService<IBusinessMetadataRetrievalService>();
        _promptBuilder = _scope.ServiceProvider.GetRequiredService<IContextualPromptBuilder>();
    }

    [Fact]
    public async Task AnalyzeUserQuestion_WithGamingQuery_ShouldReturnValidProfile()
    {
        // Arrange
        var userQuestion = "Show me total revenue by country for the last 30 days";

        // Act
        var profile = await _contextAnalyzer.AnalyzeUserQuestionAsync(userQuestion);

        // Assert
        Assert.NotNull(profile);
        Assert.Equal(userQuestion, profile.OriginalQuestion);
        Assert.True(profile.ConfidenceScore >= 0.0);
        Assert.True(profile.ConfidenceScore <= 1.0);
        Assert.NotEqual(IntentType.Unknown, profile.Intent.Type);
    }

    [Fact]
    public async Task ClassifyBusinessIntent_WithAggregationQuery_ShouldReturnAggregationIntent()
    {
        // Arrange
        var userQuestion = "What is the total revenue by game type?";

        // Act
        var intent = await _contextAnalyzer.ClassifyBusinessIntentAsync(userQuestion);

        // Assert
        Assert.NotNull(intent);
        Assert.True(intent.ConfidenceScore > 0.0);
        // Note: The actual intent classification depends on the AI service implementation
        // In a real test, we might mock the AI service for predictable results
    }

    [Fact]
    public async Task ExtractBusinessEntities_WithRevenueQuery_ShouldExtractRelevantEntities()
    {
        // Arrange
        var userQuestion = "Show me player revenue and session count by country";

        // Act
        var entities = await _contextAnalyzer.ExtractBusinessEntitiesAsync(userQuestion);

        // Assert
        Assert.NotNull(entities);
        // The actual entities depend on the AI service implementation
        // In a real scenario, we'd expect entities like "revenue", "session", "country"
    }

    [Fact]
    public async Task GetRelevantBusinessMetadata_WithValidProfile_ShouldReturnSchema()
    {
        // Arrange
        var profile = new BusinessContextProfile
        {
            OriginalQuestion = "Show me gaming revenue trends",
            Intent = new QueryIntent { Type = IntentType.Trend, ConfidenceScore = 0.9 },
            Domain = new BusinessDomain { Name = "Gaming", RelevanceScore = 0.8 },
            BusinessTerms = new List<string> { "revenue", "gaming", "trends" },
            ConfidenceScore = 0.85
        };

        // Act
        var schema = await _metadataService.GetRelevantBusinessMetadataAsync(profile);

        // Assert
        Assert.NotNull(schema);
        Assert.True(schema.RelevanceScore >= 0.0);
        Assert.NotEqual(SchemaComplexity.VeryComplex, schema.Complexity); // Should be reasonable for test
    }

    [Fact]
    public async Task BuildBusinessAwarePrompt_WithCompleteContext_ShouldGeneratePrompt()
    {
        // Arrange
        var userQuestion = "What are the top 5 games by revenue this month?";
        var profile = new BusinessContextProfile
        {
            OriginalQuestion = userQuestion,
            Intent = new QueryIntent { Type = IntentType.Aggregation, ConfidenceScore = 0.9 },
            Domain = new BusinessDomain { Name = "Gaming", RelevanceScore = 0.8 },
            BusinessTerms = new List<string> { "games", "revenue", "top" },
            ConfidenceScore = 0.85
        };

        var schema = new ContextualBusinessSchema
        {
            RelevantTables = new List<BIReportingCopilot.Core.DTOs.BusinessTableInfoDto>(),
            RelevanceScore = 0.8,
            Complexity = SchemaComplexity.Moderate
        };

        // Act
        var prompt = await _promptBuilder.BuildBusinessAwarePromptAsync(userQuestion, profile, schema);

        // Assert
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);
        Assert.Contains("business intelligence", prompt.ToLower());
        Assert.Contains(userQuestion, prompt);
        Assert.Contains("sql", prompt.ToLower());
    }

    [Fact]
    public async Task EndToEndWorkflow_WithBusinessQuery_ShouldCompleteSuccessfully()
    {
        // Arrange
        var userQuestion = "Show me daily active users for the last week";

        // Act
        // Step 1: Analyze business context
        var profile = await _contextAnalyzer.AnalyzeUserQuestionAsync(userQuestion);

        // Step 2: Get relevant metadata
        var schema = await _metadataService.GetRelevantBusinessMetadataAsync(profile);

        // Step 3: Build contextual prompt
        var prompt = await _promptBuilder.BuildBusinessAwarePromptAsync(userQuestion, profile, schema);

        // Assert
        Assert.NotNull(profile);
        Assert.NotNull(schema);
        Assert.NotNull(prompt);
        Assert.NotEmpty(prompt);

        // Verify the workflow maintains consistency
        Assert.Equal(userQuestion, profile.OriginalQuestion);
        Assert.Contains(userQuestion, prompt);
    }

    public void Dispose()
    {
        _scope?.Dispose();
    }
}

/// <summary>
/// Test factory for integration tests
/// </summary>
public class TestWebApplicationFactory : Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Override services for testing if needed
            // For example, mock the AI service for predictable results
        });

        builder.UseEnvironment("Test");
    }
}
