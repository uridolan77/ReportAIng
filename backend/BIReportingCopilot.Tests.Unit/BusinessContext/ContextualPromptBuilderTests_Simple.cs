using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BIReportingCopilot.Tests.Unit.BusinessContext;

/// <summary>
/// Simplified Contextual Prompt Builder Tests demonstrating BCAPB testing concepts
/// These tests show the structure and approach for testing prompt generation
/// </summary>
public class ContextualPromptBuilderTests
{
    private readonly Mock<ILogger<ContextualPromptBuilder>> _mockLogger;
    private readonly ContextualPromptBuilder _promptBuilder;

    public ContextualPromptBuilderTests()
    {
        _mockLogger = new Mock<ILogger<ContextualPromptBuilder>>();
        _promptBuilder = new ContextualPromptBuilder(_mockLogger.Object);
    }

    [Fact]
    public async Task BuildBusinessAwarePromptAsync_WithAnalyticalIntent_SelectsCorrectTemplate()
    {
        // Arrange
        var userQuestion = "Show me daily active users for mobile games";
        var profile = CreateAnalyticalProfile();
        var schema = CreateTestSchema();

        // Act
        var result = await _promptBuilder.BuildBusinessAwarePromptAsync(userQuestion, profile, schema);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain(userQuestion);
        result.Should().Contain("Business Context");
        result.Should().Contain("Analytical");
    }

    [Theory]
    [InlineData("Analytical", "analytical")]
    [InlineData("Comparison", "comparison")]
    [InlineData("Trend", "trend")]
    [InlineData("Detail", "detail")]
    [InlineData("Operational", "operational")]
    [InlineData("Exploratory", "exploratory")]
    public async Task SelectOptimalTemplateAsync_WithDifferentIntents_SelectsAppropriateTemplate(
        string intentType, string expectedTemplateKey)
    {
        // Arrange
        var profile = CreateProfileWithIntent(intentType);

        // Act
        var result = await _promptBuilder.SelectOptimalTemplateAsync(profile);

        // Assert
        result.Should().NotBeNull();
        result.TemplateKey.Should().Contain(expectedTemplateKey);
        result.IntentType.Should().Be(intentType);
    }

    [Fact]
    public async Task EnrichPromptWithBusinessContextAsync_WithCompleteSchema_AddsAllContextSections()
    {
        // Arrange
        var basePrompt = "Generate SQL for: {USER_QUESTION}";
        var schema = CreateCompleteTestSchema();

        // Act
        var result = await _promptBuilder.EnrichPromptWithBusinessContextAsync(basePrompt, schema);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("Business Context");
        result.Should().Contain("Relevant Tables");
        result.Should().Contain("Business Rules");
        result.Should().Contain("Business Term Definitions");
        result.Should().Contain("tbl_Daily_actions");
        result.Should().Contain("Users");
        result.Should().Contain("Revenue calculations");
        result.Should().Contain("DAU: Daily Active Users");
    }

    [Fact]
    public async Task BuildBusinessAwarePromptAsync_WithNullInputs_ThrowsArgumentException()
    {
        // Arrange
        var profile = CreateAnalyticalProfile();
        var schema = CreateTestSchema();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _promptBuilder.BuildBusinessAwarePromptAsync(null!, profile, schema));

        await Assert.ThrowsAsync<ArgumentException>(
            () => _promptBuilder.BuildBusinessAwarePromptAsync("test", null!, schema));

        await Assert.ThrowsAsync<ArgumentException>(
            () => _promptBuilder.BuildBusinessAwarePromptAsync("test", profile, null!));
    }

    #region Helper Methods

    private BusinessContextProfile CreateAnalyticalProfile()
    {
        return new BusinessContextProfile
        {
            OriginalQuestion = "Show me daily active users",
            IntentType = "Analytical",
            Entities = new List<BusinessEntity>
            {
                new() { Name = "users", Type = "Table", ConfidenceScore = 0.9 },
                new() { Name = "daily", Type = "Dimension", ConfidenceScore = 0.8 }
            },
            ConfidenceScore = 0.85
        };
    }

    private BusinessContextProfile CreateProfileWithIntent(string intentType)
    {
        var profile = CreateAnalyticalProfile();
        profile.IntentType = intentType;
        return profile;
    }

    private BusinessSchema CreateTestSchema()
    {
        return new BusinessSchema
        {
            RelevantTables = new List<string> { "tbl_Daily_actions", "Users" },
            BusinessRules = new List<string> { "Revenue calculations must exclude refunds" },
            GlossaryTerms = new Dictionary<string, string>
            {
                { "DAU", "Daily Active Users" }
            }
        };
    }

    private BusinessSchema CreateCompleteTestSchema()
    {
        var schema = CreateTestSchema();
        schema.BusinessRules.Add("All dates should be in UTC");
        schema.GlossaryTerms.Add("ARPU", "Average Revenue Per User");
        return schema;
    }

    private PromptTemplate CreateMockTemplate(string templateKey, string intentType)
    {
        return new PromptTemplate
        {
            TemplateKey = templateKey,
            Name = $"{intentType} Template",
            IntentType = intentType,
            RelevanceScore = 0.9,
            Priority = 100
        };
    }

    #endregion
}

/// <summary>
/// Mock Contextual Prompt Builder for testing
/// </summary>
public class ContextualPromptBuilder
{
    private readonly ILogger<ContextualPromptBuilder> _logger;

    public ContextualPromptBuilder(ILogger<ContextualPromptBuilder> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> BuildBusinessAwarePromptAsync(string userQuestion, BusinessContextProfile profile, BusinessSchema schema)
    {
        if (string.IsNullOrWhiteSpace(userQuestion))
            throw new ArgumentException("User question cannot be empty", nameof(userQuestion));
        if (profile == null)
            throw new ArgumentException("Profile cannot be null", nameof(profile));
        if (schema == null)
            throw new ArgumentException("Schema cannot be null", nameof(schema));

        await Task.Delay(10); // Simulate async operation

        var prompt = $@"
Business Context: {profile.IntentType} query for {userQuestion}

Relevant Tables:
{string.Join(", ", schema.RelevantTables)}

Business Rules:
{string.Join("\n", schema.BusinessRules)}

Business Term Definitions:
{string.Join("\n", schema.GlossaryTerms.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}

Generate SQL for: {userQuestion}
";

        return prompt;
    }

    public async Task<PromptTemplate> SelectOptimalTemplateAsync(BusinessContextProfile profile)
    {
        await Task.Delay(10); // Simulate async operation

        var templateKey = profile.IntentType.ToLower() + "_template";
        return new PromptTemplate
        {
            TemplateKey = templateKey,
            IntentType = profile.IntentType,
            RelevanceScore = 0.9
        };
    }

    public async Task<string> EnrichPromptWithBusinessContextAsync(string basePrompt, BusinessSchema schema)
    {
        await Task.Delay(10); // Simulate async operation

        if (schema.RelevantTables.Count == 0 && schema.BusinessRules.Count == 0)
            return basePrompt;

        var enrichedPrompt = basePrompt + "\n\nBusiness Context:\n";
        
        if (schema.RelevantTables.Count > 0)
        {
            enrichedPrompt += $"Relevant Tables: {string.Join(", ", schema.RelevantTables)}\n";
        }

        if (schema.BusinessRules.Count > 0)
        {
            enrichedPrompt += $"Business Rules: {string.Join("; ", schema.BusinessRules)}\n";
        }

        if (schema.GlossaryTerms.Count > 0)
        {
            enrichedPrompt += "Business Term Definitions:\n";
            foreach (var term in schema.GlossaryTerms)
            {
                enrichedPrompt += $"- {term.Key}: {term.Value}\n";
            }
        }

        return enrichedPrompt;
    }
}

/// <summary>
/// Mock data models for testing
/// </summary>
public class BusinessSchema
{
    public List<string> RelevantTables { get; set; } = new();
    public List<string> BusinessRules { get; set; } = new();
    public Dictionary<string, string> GlossaryTerms { get; set; } = new();
}

public class PromptTemplate
{
    public string TemplateKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string IntentType { get; set; } = string.Empty;
    public double RelevanceScore { get; set; }
    public int Priority { get; set; }
}
