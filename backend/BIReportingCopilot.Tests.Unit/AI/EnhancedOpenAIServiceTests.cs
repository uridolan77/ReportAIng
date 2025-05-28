using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.AI;
using Azure.AI.OpenAI;
using FluentAssertions;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace BIReportingCopilot.Tests.Unit.AI;

public class EnhancedOpenAIServiceTests
{
    private readonly ILogger<EnhancedOpenAIService> _logger;
    private readonly IConfiguration _configuration;
    private readonly OpenAIClient _mockClient;

    public EnhancedOpenAIServiceTests()
    {
        _logger = NullLogger<EnhancedOpenAIService>.Instance;

        // Create test configuration
        var configData = new Dictionary<string, string>
        {
            ["OpenAI:ApiKey"] = "test-api-key",
            ["OpenAI:Model"] = "gpt-4",
            ["OpenAI:Temperature"] = "0.1",
            ["OpenAI:MaxTokens"] = "1000",
            ["OpenAI:TimeoutSeconds"] = "30",
            ["OpenAI:MaxRetries"] = "3",
            ["OpenAI:FrequencyPenalty"] = "0.0",
            ["OpenAI:PresencePenalty"] = "0.0",
            ["AzureOpenAI:Endpoint"] = "",
            ["AzureOpenAI:ApiKey"] = "",
            ["AzureOpenAI:DeploymentName"] = "gpt-4"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData!)
            .Build();

        // Create mock OpenAI client
        _mockClient = new OpenAIClient("test-key");
    }

    [Fact]
    public async Task GenerateSQLAsync_WithoutRealOpenAI_ShouldReturnFallbackSQL()
    {
        // Arrange
        var service = new EnhancedOpenAIService(_mockClient, _configuration, _logger);
        var prompt = "Show me all customers";

        // Act
        var result = await service.GenerateSQLAsync(prompt);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("SELECT");
        // Since we don't have real OpenAI configured, it should return fallback SQL
        // The service is smart enough to return customer-specific SQL for customer queries
        result.Should().Contain("Customer");
    }

    [Fact]
    public async Task GenerateSQLAsync_WithRevenueQuery_ShouldReturnRevenueSQL()
    {
        // Arrange
        var service = new EnhancedOpenAIService(_mockClient, _configuration, _logger);
        var prompt = "Show me total revenue by month";

        // Act
        var result = await service.GenerateSQLAsync(prompt);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("SELECT");
        result.Should().Contain("Revenue");
        result.Should().Contain("MONTH");
    }

    [Fact]
    public async Task GenerateSQLAsync_WithCustomerQuery_ShouldReturnCustomerSQL()
    {
        // Arrange
        var service = new EnhancedOpenAIService(_mockClient, _configuration, _logger);
        var prompt = "Show me customer information";

        // Act
        var result = await service.GenerateSQLAsync(prompt);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("SELECT");
        result.Should().Contain("Customer");
    }

    [Fact]
    public async Task GenerateInsightAsync_WithoutRealOpenAI_ShouldReturnFallbackInsight()
    {
        // Arrange
        var service = new EnhancedOpenAIService(_mockClient, _configuration, _logger);
        var query = "SELECT COUNT(*) FROM Orders";
        var data = new object[] { new { Count = 100 } };

        // Act
        var result = await service.GenerateInsightAsync(query, data);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("single result");
        result.Should().Contain("OpenAI services");
    }

    [Fact]
    public async Task CalculateConfidenceScoreAsync_WithSimpleQuery_ShouldReturnReasonableScore()
    {
        // Arrange
        var service = new EnhancedOpenAIService(_mockClient, _configuration, _logger);
        var naturalLanguageQuery = "Show me all users";
        var generatedSQL = "SELECT * FROM Users";

        // Act
        var result = await service.CalculateConfidenceScoreAsync(naturalLanguageQuery, generatedSQL);

        // Assert
        result.Should().BeGreaterThan(0);
        result.Should().BeLessOrEqualTo(1);
    }

    [Fact]
    public async Task CalculateConfidenceScoreAsync_WithComplexQuery_ShouldReturnAppropriateScore()
    {
        // Arrange
        var service = new EnhancedOpenAIService(_mockClient, _configuration, _logger);
        var naturalLanguageQuery = "Show me revenue by product category with year-over-year growth";
        var generatedSQL = @"SELECT
                                pc.CategoryName,
                                YEAR(o.OrderDate) as Year,
                                SUM(oi.Quantity * oi.UnitPrice) as Revenue
                             FROM Orders o
                             JOIN OrderItems oi ON o.OrderId = oi.OrderId
                             JOIN Products p ON oi.ProductId = p.ProductId
                             JOIN ProductCategories pc ON p.CategoryId = pc.CategoryId
                             GROUP BY pc.CategoryName, YEAR(o.OrderDate)
                             ORDER BY pc.CategoryName, Year";

        // Act
        var result = await service.CalculateConfidenceScoreAsync(naturalLanguageQuery, generatedSQL);

        // Assert
        result.Should().BeGreaterThan(0);
        result.Should().BeLessOrEqualTo(1);
    }

    [Fact]
    public async Task ValidateQueryIntentAsync_WithValidDataQuery_ShouldReturnTrue()
    {
        // Arrange
        var service = new EnhancedOpenAIService(_mockClient, _configuration, _logger);
        var query = "Show me all users where active = 1";

        // Act
        var result = await service.ValidateQueryIntentAsync(query);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateQueryIntentAsync_WithInvalidQuery_ShouldReturnFalse()
    {
        // Arrange
        var service = new EnhancedOpenAIService(_mockClient, _configuration, _logger);
        var query = "DROP TABLE Users";

        // Act
        var result = await service.ValidateQueryIntentAsync(query);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("Show me all users", true)]
    [InlineData("Get users where active = 1", true)]
    [InlineData("Count all orders", true)]
    [InlineData("Find top customers", true)]
    [InlineData("List all products", true)]
    [InlineData("What is the total revenue", true)]
    [InlineData("How many users are active", true)]
    [InlineData("DROP TABLE Users", false)]
    [InlineData("DELETE FROM Users", false)]
    [InlineData("INVALID SQL", false)]
    [InlineData("Random text without query intent", false)]
    public async Task ValidateQueryIntentAsync_ShouldCorrectlyIdentifyValidQueries(string query, bool expectedValid)
    {
        // Arrange
        var service = new EnhancedOpenAIService(_mockClient, _configuration, _logger);

        // Act
        var result = await service.ValidateQueryIntentAsync(query);

        // Assert
        result.Should().Be(expectedValid);
    }

    [Fact]
    public async Task GenerateVisualizationConfigAsync_WithQueryAndData_ShouldReturnValidJSON()
    {
        // Arrange
        var service = new EnhancedOpenAIService(_mockClient, _configuration, _logger);
        var query = "SELECT Category, SUM(Revenue) FROM Sales GROUP BY Category";
        var columns = new[]
        {
            new BIReportingCopilot.Core.Models.ColumnInfo { Name = "Category", DataType = "string" },
            new BIReportingCopilot.Core.Models.ColumnInfo { Name = "Revenue", DataType = "decimal" }
        };
        var data = new object[]
        {
            new { Category = "Electronics", Revenue = 10000 },
            new { Category = "Clothing", Revenue = 8000 },
            new { Category = "Books", Revenue = 5000 }
        };

        // Act
        var result = await service.GenerateVisualizationConfigAsync(query, columns, data);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("type");
    }

    [Fact]
    public async Task GenerateQuerySuggestionsAsync_WithSchema_ShouldReturnRelevantSuggestions()
    {
        // Arrange
        var service = new EnhancedOpenAIService(_mockClient, _configuration, _logger);
        var schema = new BIReportingCopilot.Core.Models.SchemaMetadata
        {
            Tables = new List<BIReportingCopilot.Core.Models.TableMetadata>
            {
                new BIReportingCopilot.Core.Models.TableMetadata
                {
                    Name = "Users",
                    Columns = new List<BIReportingCopilot.Core.Models.ColumnMetadata>
                    {
                        new BIReportingCopilot.Core.Models.ColumnMetadata { Name = "Id", DataType = "int" },
                        new BIReportingCopilot.Core.Models.ColumnMetadata { Name = "Name", DataType = "string" }
                    }
                },
                new BIReportingCopilot.Core.Models.TableMetadata
                {
                    Name = "Orders",
                    Columns = new List<BIReportingCopilot.Core.Models.ColumnMetadata>
                    {
                        new BIReportingCopilot.Core.Models.ColumnMetadata { Name = "Id", DataType = "int" },
                        new BIReportingCopilot.Core.Models.ColumnMetadata { Name = "OrderDate", DataType = "datetime" }
                    }
                }
            }
        };

        // Act
        var result = await service.GenerateQuerySuggestionsAsync("general business queries", schema);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().HaveCountGreaterThan(0);
    }
}
