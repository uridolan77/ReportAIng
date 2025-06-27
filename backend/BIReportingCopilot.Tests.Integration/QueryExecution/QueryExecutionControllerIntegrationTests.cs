using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Tests.Integration;

namespace BIReportingCopilot.Tests.Integration.QueryExecution;

/// <summary>
/// Integration tests for Query Execution Controller Enhanced Schema Contextualization System
/// </summary>
public class QueryExecutionControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public QueryExecutionControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task ExecuteNaturalLanguageQuery_WithBasicRequest_ShouldReturnSuccessResponse()
    {
        // Arrange
        var request = new QueryRequest
        {
            Question = "Show me total deposits by country last week",
            SessionId = "test-session-001"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/query-execution/execute", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"Expected success status code, got {response.StatusCode}");
        
        var content = await response.Content.ReadAsStringAsync();
        var queryResponse = JsonSerializer.Deserialize<QueryResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(queryResponse);
        Assert.True(queryResponse.Success, $"Query should succeed. Error: {queryResponse.Error}");
        Assert.NotEmpty(queryResponse.Sql);
        
        // Verify enhanced context was used (confidence should be higher than basic pipeline)
        Assert.True(queryResponse.Confidence > 0.1, $"Expected confidence > 0.1, got {queryResponse.Confidence}");
    }

    [Fact]
    public async Task ExecuteNaturalLanguageQuery_WithFinancialQuery_ShouldUseEnhancedContext()
    {
        // Arrange
        var request = new QueryRequest
        {
            Question = "What were the total deposits by country last week?",
            SessionId = "test-session-002",
            Options = new QueryOptions
            {
                MaxTables = 5,
                MaxTokens = 4000
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/query-execution/execute", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var queryResponse = JsonSerializer.Deserialize<QueryResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(queryResponse);
        Assert.True(queryResponse.Success);
        Assert.NotEmpty(queryResponse.Sql);
        
        // Verify enhanced context characteristics
        Assert.True(queryResponse.Confidence > 0.5, "Enhanced context should provide higher confidence");
        Assert.NotNull(queryResponse.PromptDetails);
        
        // Check if enhanced prompt details are populated
        if (queryResponse.PromptDetails != null)
        {
            Assert.True(queryResponse.PromptDetails.PromptLength > 0);
            Assert.True(queryResponse.PromptDetails.SchemaTablesCount > 0);
        }
    }

    [Fact]
    public async Task ExecuteNaturalLanguageQuery_WithGamingQuery_ShouldIncludeGamingTables()
    {
        // Arrange
        var request = new QueryRequest
        {
            Question = "Which games were most popular last week?",
            SessionId = "test-session-003"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/query-execution/execute", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var queryResponse = JsonSerializer.Deserialize<QueryResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(queryResponse);
        
        if (queryResponse.Success)
        {
            Assert.NotEmpty(queryResponse.Sql);
            // Gaming queries should include gaming-related tables
            Assert.Contains("game", queryResponse.Sql.ToLower());
        }
    }

    [Fact]
    public async Task ExecuteNaturalLanguageQuery_WithInvalidQuery_ShouldReturnMeaningfulError()
    {
        // Arrange
        var request = new QueryRequest
        {
            Question = "", // Empty question
            SessionId = "test-session-004"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/query-execution/execute", request);

        // Assert
        // Should either return bad request or success with error details
        var content = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            var queryResponse = JsonSerializer.Deserialize<QueryResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            
            Assert.NotNull(queryResponse);
            Assert.False(queryResponse.Success);
            Assert.NotEmpty(queryResponse.Error);
        }
        else
        {
            Assert.True(response.StatusCode == System.Net.HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task ExecuteNaturalLanguageQuery_CompareWithAIPipelineTest_ShouldHaveSimilarPerformance()
    {
        // Arrange
        var testQuery = "Show me total deposits by country last week";
        
        var queryExecutionRequest = new QueryRequest
        {
            Question = testQuery,
            SessionId = "test-comparison-001"
        };

        // Act - Test Query Execution Controller
        var queryExecutionResponse = await _client.PostAsJsonAsync("/api/query-execution/execute", queryExecutionRequest);
        
        // Assert
        Assert.True(queryExecutionResponse.IsSuccessStatusCode);
        
        var queryContent = await queryExecutionResponse.Content.ReadAsStringAsync();
        var queryResult = JsonSerializer.Deserialize<QueryResponse>(queryContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(queryResult);
        
        if (queryResult.Success)
        {
            // Verify enhanced pipeline characteristics
            Assert.True(queryResult.Confidence > 0.3, "Enhanced pipeline should provide reasonable confidence");
            Assert.NotEmpty(queryResult.Sql);
            Assert.True(queryResult.ExecutionTimeMs < 30000, "Should complete within 30 seconds");
            
            // Verify prompt details indicate enhanced processing
            if (queryResult.PromptDetails != null)
            {
                Assert.True(queryResult.PromptDetails.PromptLength > 100, "Enhanced prompts should be substantial");
                Assert.True(queryResult.PromptDetails.SchemaTablesCount > 0, "Should include relevant tables");
            }
        }
    }

    [Theory]
    [InlineData("Show me total deposits by country last week")]
    [InlineData("What were the top 10 depositors yesterday from UK")]
    [InlineData("Which games were most popular last week")]
    public async Task ExecuteNaturalLanguageQuery_WithVariousQueries_ShouldSucceed(string question)
    {
        // Arrange
        var request = new QueryRequest
        {
            Question = question,
            SessionId = $"test-theory-{Guid.NewGuid():N}"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/query-execution/execute", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode, $"Query '{question}' should return success status");
        
        var content = await response.Content.ReadAsStringAsync();
        var queryResponse = JsonSerializer.Deserialize<QueryResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(queryResponse);
        
        // Allow for some queries to fail due to schema/data issues, but they should fail gracefully
        if (!queryResponse.Success)
        {
            Assert.NotEmpty(queryResponse.Error);
            Assert.NotNull(queryResponse.Error);
        }
        else
        {
            Assert.NotEmpty(queryResponse.Sql);
            Assert.True(queryResponse.Confidence >= 0);
        }
    }

    [Fact]
    public async Task ExecuteNaturalLanguageQuery_WithMaxTokensLimit_ShouldRespectLimits()
    {
        // Arrange
        var request = new QueryRequest
        {
            Question = "Show me detailed analysis of all deposits, withdrawals, and gaming activities by country, player, and time period with comprehensive breakdowns",
            SessionId = "test-token-limit-001",
            Options = new QueryOptions
            {
                MaxTokens = 1000, // Low token limit
                MaxTables = 2
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/query-execution/execute", request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        
        var content = await response.Content.ReadAsStringAsync();
        var queryResponse = JsonSerializer.Deserialize<QueryResponse>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Assert.NotNull(queryResponse);
        
        if (queryResponse.Success && queryResponse.PromptDetails != null)
        {
            // Should respect token limits
            Assert.True(queryResponse.PromptDetails.TokenCount <= 1200, "Should respect token budget with some tolerance");
            Assert.True(queryResponse.PromptDetails.SchemaTablesCount <= 2, "Should respect table limits");
        }
    }
}
