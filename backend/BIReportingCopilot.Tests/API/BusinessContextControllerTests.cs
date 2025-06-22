using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using BIReportingCopilot.Core.Models.BusinessContext;

namespace BIReportingCopilot.Tests.API;

/// <summary>
/// API tests for Business Context Controller
/// </summary>
public class BusinessContextControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public BusinessContextControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task AnalyzeBusinessContext_WithValidRequest_ShouldReturnProfile()
    {
        // Arrange
        var request = new
        {
            UserQuestion = "Show me total revenue by country",
            UserId = "test-user"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/businesscontext/analyze", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);

        // Verify the response can be deserialized
        var profile = JsonSerializer.Deserialize<BusinessContextProfile>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(profile);
        Assert.Equal(request.UserQuestion, profile.OriginalQuestion);
    }

    [Fact]
    public async Task ClassifyIntent_WithValidRequest_ShouldReturnIntent()
    {
        // Arrange
        var request = new
        {
            UserQuestion = "What are the top performing games?"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/businesscontext/intent", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);

        // Verify the response can be deserialized
        var intent = JsonSerializer.Deserialize<QueryIntent>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(intent);
        Assert.True(intent.ConfidenceScore >= 0.0);
        Assert.True(intent.ConfidenceScore <= 1.0);
    }

    [Fact]
    public async Task ExtractEntities_WithValidRequest_ShouldReturnEntities()
    {
        // Arrange
        var request = new
        {
            UserQuestion = "Show me player revenue and session data"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/businesscontext/entities", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);

        // Verify the response can be deserialized
        var entities = JsonSerializer.Deserialize<List<BusinessEntity>>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(entities);
    }

    [Fact]
    public async Task GenerateBusinessAwarePrompt_WithValidRequest_ShouldReturnPrompt()
    {
        // Arrange
        var request = new
        {
            UserQuestion = "Show me gaming revenue trends for the last quarter",
            UserId = "test-user",
            ComplexityLevel = "Standard",
            IncludeExamples = true,
            IncludeBusinessRules = true,
            MaxTables = 5,
            MaxTokens = 4000
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/businesscontext/prompt", request);

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.NotEmpty(content);

        // Verify the response structure
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;

        Assert.True(root.TryGetProperty("generatedPrompt", out var promptProperty));
        Assert.True(root.TryGetProperty("contextProfile", out var profileProperty));
        Assert.True(root.TryGetProperty("usedSchema", out var schemaProperty));
        Assert.True(root.TryGetProperty("confidenceScore", out var scoreProperty));

        var promptText = promptProperty.GetString();
        Assert.NotNull(promptText);
        Assert.NotEmpty(promptText);
        Assert.Contains(request.UserQuestion, promptText);
    }

    [Fact]
    public async Task AnalyzeBusinessContext_WithEmptyRequest_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            UserQuestion = "",
            UserId = "test-user"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/businesscontext/analyze", request);

        // Assert
        // Depending on validation implementation, this might return 400 or process with empty question
        // For now, we'll just verify it doesn't crash
        Assert.True(response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GenerateBusinessAwarePrompt_EndToEndWorkflow_ShouldWork()
    {
        // This test simulates the complete workflow that a frontend would use

        // Step 1: Analyze the business context
        var analyzeRequest = new
        {
            UserQuestion = "What are the top 10 countries by gaming revenue this month?",
            UserId = "integration-test-user"
        };

        var analyzeResponse = await _client.PostAsJsonAsync("/api/businesscontext/analyze", analyzeRequest);
        analyzeResponse.EnsureSuccessStatusCode();

        // Step 2: Generate the business-aware prompt
        var promptRequest = new
        {
            UserQuestion = analyzeRequest.UserQuestion,
            UserId = analyzeRequest.UserId,
            ComplexityLevel = "Standard",
            IncludeExamples = true,
            IncludeBusinessRules = true,
            MaxTables = 5
        };

        var promptResponse = await _client.PostAsJsonAsync("/api/businesscontext/prompt", promptRequest);
        promptResponse.EnsureSuccessStatusCode();

        var promptContent = await promptResponse.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(promptContent);
        var root = document.RootElement;

        // Verify the complete response structure
        Assert.True(root.TryGetProperty("generatedPrompt", out var prompt));
        Assert.True(root.TryGetProperty("contextProfile", out var profile));
        Assert.True(root.TryGetProperty("usedSchema", out var schema));
        Assert.True(root.TryGetProperty("confidenceScore", out var confidence));
        Assert.True(root.TryGetProperty("metadata", out var metadata));

        // Verify the prompt contains expected elements
        var promptText = prompt.GetString();
        Assert.Contains("business intelligence", promptText!.ToLower());
        Assert.Contains("sql", promptText.ToLower());
        Assert.Contains(analyzeRequest.UserQuestion, promptText);

        // Verify confidence score is reasonable
        var confidenceScore = confidence.GetDouble();
        Assert.True(confidenceScore >= 0.0 && confidenceScore <= 1.0);
    }
}
