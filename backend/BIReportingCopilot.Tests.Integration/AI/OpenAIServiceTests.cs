using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.AI;
using Xunit;

namespace BIReportingCopilot.Tests.Integration.AI;

public class OpenAIServiceTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly IAIService _aiService;
    private readonly bool _isConfigured;

    public OpenAIServiceTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;

        using var scope = _factory.Services.CreateScope();

        // Get the unified AI service
        _aiService = scope.ServiceProvider.GetRequiredService<IAIService>();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        // Check if OpenAI is actually configured for integration tests
        var azureEndpoint = configuration["AzureOpenAI:Endpoint"];
        var azureApiKey = configuration["AzureOpenAI:ApiKey"];
        var openAIApiKey = configuration["OpenAI:ApiKey"];

        _isConfigured = (!string.IsNullOrEmpty(azureEndpoint) && !string.IsNullOrEmpty(azureApiKey)) ||
                       (!string.IsNullOrEmpty(openAIApiKey) && openAIApiKey != "your-openai-api-key-here");
    }

    [Fact]
    public async Task GenerateSQLAsync_WithSimpleQuery_ShouldReturnValidSQL()
    {
        // Arrange
        var prompt = "Show me all users";

        // Act
        var sql = await _aiService.GenerateSQLAsync(prompt);

        // Assert
        sql.Should().NotBeNullOrEmpty();
        sql.ToLowerInvariant().Should().Contain("select");
        sql.ToLowerInvariant().Should().Contain("from");
        sql.Should().NotContain("DROP");
        sql.Should().NotContain("DELETE");
        sql.Should().NotContain("INSERT");

        if (!_isConfigured)
        {
            // When not configured, should use fallback SQL
            sql.Should().Contain("Sample Data");
        }
    }

    [Fact]
    public async Task GenerateSQLAsync_WhenNotConfigured_ShouldReturnFallbackSQL()
    {
        // This test verifies fallback behavior when OpenAI is not configured
        // The service should still return valid SQL based on patterns

        // Arrange
        var prompt = "Show me revenue data";

        // Act
        var sql = await _aiService.GenerateSQLAsync(prompt);

        // Assert
        sql.Should().NotBeNullOrEmpty();
        sql.ToLowerInvariant().Should().Contain("select");

        if (!_isConfigured)
        {
            // Should contain revenue-related fallback SQL
            sql.Should().Contain("Revenue");
            sql.Should().Contain("Orders");
        }
    }

    [Fact]
    public async Task GenerateSQLAsync_WithRevenueQuery_ShouldReturnRevenueSQL()
    {
        // Arrange
        var prompt = "Show me total revenue by month";

        // Act
        var sql = await _aiService.GenerateSQLAsync(prompt);

        // Assert
        sql.Should().NotBeNullOrEmpty();
        sql.ToLowerInvariant().Should().Contain("select");
        sql.ToLowerInvariant().Should().Contain("sum");
        sql.ToLowerInvariant().Should().Contain("group by");
        sql.ToLowerInvariant().Should().Contain("month");
    }

    [Fact]
    public async Task GenerateSQLAsync_WithCustomersQuery_ShouldReturnCustomersSQL()
    {
        // Arrange
        var prompt = "Count customers by country";

        // Act
        var sql = await _aiService.GenerateSQLAsync(prompt);

        // Assert
        sql.Should().NotBeNullOrEmpty();
        sql.ToLowerInvariant().Should().Contain("select");
        sql.ToLowerInvariant().Should().Contain("count");
        sql.ToLowerInvariant().Should().Contain("customers");
        sql.ToLowerInvariant().Should().Contain("country");
    }

    [Fact]
    public async Task GenerateInsightAsync_WithQueryAndData_ShouldReturnInsight()
    {
        // Arrange
        var query = "SELECT COUNT(*) as TotalUsers FROM Users";
        var data = new object[] { new { TotalUsers = 150 } };

        // Act
        var insight = await _aiService.GenerateInsightAsync(query, data);

        // Assert
        insight.Should().NotBeNullOrEmpty();
        insight.Should().NotBe("Unable to generate insights at this time.");
    }

    [Fact]
    public async Task GenerateVisualizationConfigAsync_WithQueryAndData_ShouldReturnValidJSON()
    {
        // Arrange
        var query = "SELECT Country, COUNT(*) as UserCount FROM Users GROUP BY Country";
        var columns = new[]
        {
            new ColumnInfo { Name = "Country", DataType = "string" },
            new ColumnInfo { Name = "UserCount", DataType = "int" }
        };
        var data = new object[]
        {
            new { Country = "USA", UserCount = 100 },
            new { Country = "UK", UserCount = 50 }
        };

        // Act
        var config = await _aiService.GenerateVisualizationConfigAsync(query, columns, data);

        // Assert
        config.Should().NotBeNullOrEmpty();
        config.Should().StartWith("{");
        config.Should().EndWith("}");
        config.Should().Contain("type");
    }

    [Fact]
    public async Task CalculateConfidenceScoreAsync_WithSimpleQuery_ShouldReturnReasonableScore()
    {
        // Arrange
        var naturalQuery = "Show me all users";
        var generatedSQL = "SELECT * FROM Users";

        // Act
        var confidence = await _aiService.CalculateConfidenceScoreAsync(naturalQuery, generatedSQL);

        // Assert
        confidence.Should().BeGreaterThan(0.0);
        confidence.Should().BeLessOrEqualTo(1.0);
        confidence.Should().BeGreaterThan(0.5); // Should have decent confidence for simple query
    }

    [Fact]
    public async Task CalculateConfidenceScoreAsync_WithComplexQuery_ShouldReturnAppropriateScore()
    {
        // Arrange
        var naturalQuery = "Show me the average revenue per customer by country for the last 6 months";
        var generatedSQL = @"SELECT
                                c.Country,
                                AVG(o.Total) as AvgRevenue
                             FROM Customers c
                             JOIN Orders o ON c.Id = o.CustomerId
                             WHERE o.OrderDate >= DATEADD(month, -6, GETDATE())
                             GROUP BY c.Country";

        // Act
        var confidence = await _aiService.CalculateConfidenceScoreAsync(naturalQuery, generatedSQL);

        // Assert
        confidence.Should().BeGreaterThan(0.0);
        confidence.Should().BeLessOrEqualTo(1.0);
        confidence.Should().BeGreaterThan(0.6); // Should have good confidence for well-structured query
    }

    [Fact]
    public async Task CalculateConfidenceScoreAsync_WithInvalidSQL_ShouldReturnLowScore()
    {
        // Arrange
        var naturalQuery = "Show me all users";
        var invalidSQL = "INVALID SQL STATEMENT";

        // Act
        var confidence = await _aiService.CalculateConfidenceScoreAsync(naturalQuery, invalidSQL);

        // Assert
        confidence.Should().BeGreaterThan(0.0);
        confidence.Should().BeLessOrEqualTo(1.0);
        confidence.Should().BeLessThan(0.3); // Should have low confidence for invalid SQL
    }

    [Fact]
    public async Task GenerateQuerySuggestionsAsync_WithSchema_ShouldReturnRelevantSuggestions()
    {
        // Arrange
        var context = "User management system";
        var schema = new SchemaMetadata
        {
            Tables = new List<TableMetadata>
            {
                new TableMetadata
                {
                    Name = "Users",
                    Columns = new List<ColumnMetadata>
                    {
                        new ColumnMetadata { Name = "Id", DataType = "int" },
                        new ColumnMetadata { Name = "Username", DataType = "string" },
                        new ColumnMetadata { Name = "CreatedDate", DataType = "datetime" }
                    }
                },
                new TableMetadata
                {
                    Name = "Orders",
                    Columns = new List<ColumnMetadata>
                    {
                        new ColumnMetadata { Name = "Id", DataType = "int" },
                        new ColumnMetadata { Name = "UserId", DataType = "int" },
                        new ColumnMetadata { Name = "OrderDate", DataType = "datetime" }
                    }
                }
            }
        };

        // Act
        var suggestions = await _aiService.GenerateQuerySuggestionsAsync(context, schema);

        // Assert
        suggestions.Should().NotBeNull();
        suggestions.Should().HaveCountGreaterThan(0);
        suggestions.Should().HaveCountLessOrEqualTo(8);
        suggestions.Should().Contain(s => s.ToLowerInvariant().Contains("users"));
    }

    [Fact]
    public async Task ValidateQueryIntentAsync_WithValidDataQuery_ShouldReturnTrue()
    {
        // Arrange
        var validQueries = new[]
        {
            "Show me all users",
            "Get the total count of orders",
            "Find customers from USA",
            "List all products",
            "Count active users",
            "What is the average order value?"
        };

        foreach (var query in validQueries)
        {
            // Act
            var isValid = await _aiService.ValidateQueryIntentAsync(query);

            // Assert
            isValid.Should().BeTrue($"Query '{query}' should be recognized as valid");
        }
    }

    [Fact]
    public async Task ValidateQueryIntentAsync_WithInvalidQuery_ShouldReturnFalse()
    {
        // Arrange
        var invalidQueries = new[]
        {
            "Hello there",
            "How are you?",
            "Random text without data intent",
            "Tell me a joke"
        };

        foreach (var query in invalidQueries)
        {
            // Act
            var isValid = await _aiService.ValidateQueryIntentAsync(query);

            // Assert
            isValid.Should().BeFalse($"Query '{query}' should not be recognized as valid data query");
        }
    }

    [Theory]
    [InlineData("SELECT * FROM Users", true)]
    [InlineData("SELECT Name FROM Users WHERE Active = 1", true)]
    [InlineData("SELECT COUNT(*) FROM Orders", true)]
    [InlineData("INVALID SQL", false)]
    [InlineData("SELECT FROM", false)]
    [InlineData("DROP TABLE Users", false)]
    [InlineData("DELETE FROM Users", false)]
    public async Task SQLValidation_ShouldCorrectlyIdentifyValidSQL(string sql, bool expectedValid)
    {
        // This tests the internal SQL validation logic
        // We'll test it indirectly through confidence scoring

        // Arrange
        var naturalQuery = "test query";

        // Act
        var confidence = await _aiService.CalculateConfidenceScoreAsync(naturalQuery, sql);

        // Assert
        if (expectedValid)
        {
            confidence.Should().BeGreaterThan(0.2, $"SQL '{sql}' should be considered valid");
        }
        else
        {
            confidence.Should().BeLessThan(0.5, $"SQL '{sql}' should be considered invalid or dangerous");
        }
    }
}
