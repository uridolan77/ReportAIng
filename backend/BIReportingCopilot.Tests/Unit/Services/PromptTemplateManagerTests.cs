using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.AI;
using BIReportingCopilot.Infrastructure.Services;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BIReportingCopilot.Tests.Unit.Services;

/// <summary>
/// Unit tests for PromptService (formerly PromptTemplateManager)
/// </summary>
public class PromptTemplateManagerTests
{
    private readonly PromptService _promptManager;

    public PromptTemplateManagerTests()
    {
        // Create mocks for PromptService dependencies
        var mockContext = new Mock<TuningDbContext>();
        var mockLogger = new Mock<ILogger<PromptService>>();

        _promptManager = new PromptService(
            mockContext.Object,
            mockLogger.Object);
    }

    [Fact]
    public async Task BuildSQLGenerationPromptAsync_WithBasicPrompt_ReturnsValidPrompt()
    {
        // Arrange
        var userPrompt = "Show me all customers";

        // Act
        var result = await _promptManager.BuildSQLGenerationPromptAsync(userPrompt);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(userPrompt, result);
        Assert.Contains("SQL", result);
        Assert.Contains("Response Instructions", result);
    }

    [Fact]
    public async Task BuildSQLGenerationPromptAsync_WithSchema_IncludesSchemaInformation()
    {
        // Arrange
        var userPrompt = "Show me all customers";
        var schema = new SchemaMetadata
        {
            Tables = new List<TableMetadata>
            {
                new TableMetadata
                {
                    Name = "Customers",
                    Description = "Customer information table",
                    RowCount = 1000,
                    Columns = new List<ColumnMetadata>
                    {
                        new ColumnMetadata
                        {
                            Name = "Id",
                            DataType = "int",
                            IsPrimaryKey = true,
                            Description = "Primary key"
                        },
                        new ColumnMetadata
                        {
                            Name = "Name",
                            DataType = "varchar(100)",
                            Description = "Customer name"
                        }
                    }
                }
            }
        };

        // Act
        var result = await _promptManager.BuildSQLGenerationPromptAsync(userPrompt, schema);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Database Schema", result);
        Assert.Contains("Customers", result);
        Assert.Contains("Customer information table", result);
        Assert.Contains("Id", result);
        Assert.Contains("Name", result);
        Assert.Contains("1,000", result); // Formatted row count
    }

    [Fact]
    public async Task BuildSQLGenerationPromptAsync_WithContext_IncludesContextInformation()
    {
        // Arrange
        var userPrompt = "Show me sales data";
        var context = new QueryContext
        {
            UserId = "test-user",
            SessionId = "test-session",
            BusinessDomain = "E-commerce",
            PreferredComplexity = StreamingQueryComplexity.Complex,
            PreviousQueries = new List<string> { "SELECT * FROM Products", "SELECT COUNT(*) FROM Orders" }
        };

        // Act
        var result = await _promptManager.BuildSQLGenerationPromptAsync(userPrompt, null, context);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Context", result);
        Assert.Contains("E-commerce", result);
        Assert.Contains("Complex", result);
        Assert.Contains("Previous queries", result);
        Assert.Contains("SELECT * FROM Products", result);
    }

    [Theory]
    [InlineData(StreamingQueryComplexity.Simple, "basic SELECT")]
    [InlineData(StreamingQueryComplexity.Medium, "JOINs, subqueries")]
    [InlineData(StreamingQueryComplexity.Complex, "advanced SQL features")]
    [InlineData(StreamingQueryComplexity.Expert, "full range of SQL capabilities")]
    public async Task BuildSQLGenerationPromptAsync_WithDifferentComplexities_IncludesAppropriateGuidelines(
        StreamingQueryComplexity complexity, string expectedContent)
    {
        // Arrange
        var userPrompt = "Show me data";
        var context = new QueryContext { PreferredComplexity = complexity };

        // Act
        var result = await _promptManager.BuildSQLGenerationPromptAsync(userPrompt, null, context);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(expectedContent, result);
    }

    [Fact]
    public async Task BuildInsightGenerationPromptAsync_WithBasicData_ReturnsValidPrompt()
    {
        // Arrange
        var query = "SELECT COUNT(*) as CustomerCount FROM Customers";
        var data = new object[] { new { CustomerCount = 100 } };

        // Act
        var result = await _promptManager.BuildInsightGenerationPromptAsync(query, data);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("business intelligence analyst", result);
        Assert.Contains("SQL Query", result);
        Assert.Contains(query, result);
        Assert.Contains("Data Summary", result);
        Assert.Contains("Total records: 1", result);
    }

    [Fact]
    public async Task BuildInsightGenerationPromptAsync_WithAnalysisContext_IncludesContextInformation()
    {
        // Arrange
        var query = "SELECT SUM(Revenue) FROM Sales";
        var data = new object[] { new { Revenue = 10000 } };
        var context = new AnalysisContext
        {
            BusinessGoal = "Increase quarterly revenue",
            TimeFrame = "Q1 2024",
            KeyMetrics = new List<string> { "Revenue", "Growth Rate" },
            Type = AnalysisType.Predictive
        };

        // Act
        var result = await _promptManager.BuildInsightGenerationPromptAsync(query, data, context);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Analysis Context", result);
        Assert.Contains("Increase quarterly revenue", result);
        Assert.Contains("Q1 2024", result);
        Assert.Contains("Revenue", result);
        Assert.Contains("Predictive", result);
    }

    [Theory]
    [InlineData(AnalysisType.Descriptive, "Summarize what the data shows")]
    [InlineData(AnalysisType.Diagnostic, "Explain why certain patterns exist")]
    [InlineData(AnalysisType.Predictive, "Identify trends that might continue")]
    [InlineData(AnalysisType.Prescriptive, "Provide specific actionable recommendations")]
    public async Task BuildInsightGenerationPromptAsync_WithDifferentAnalysisTypes_IncludesAppropriateInstructions(
        AnalysisType analysisType, string expectedInstruction)
    {
        // Arrange
        var query = "SELECT * FROM Sales";
        var data = new object[] { new { Sales = 1000 } };
        var context = new AnalysisContext { Type = analysisType };

        // Act
        var result = await _promptManager.BuildInsightGenerationPromptAsync(query, data, context);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(expectedInstruction, result);
    }

    [Fact]
    public async Task BuildSQLExplanationPromptAsync_WithBasicSQL_ReturnsValidPrompt()
    {
        // Arrange
        var sql = "SELECT c.Name, COUNT(o.Id) FROM Customers c LEFT JOIN Orders o ON c.Id = o.CustomerId GROUP BY c.Name";
        var complexity = StreamingQueryComplexity.Medium;

        // Act
        var result = await _promptManager.BuildSQLExplanationPromptAsync(sql, complexity);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("database expert", result);
        Assert.Contains("SQL Query to Explain", result);
        Assert.Contains(sql, result);
        Assert.Contains("Explanation Requirements", result);
    }

    [Theory]
    [InlineData(StreamingQueryComplexity.Simple, "simple terms suitable for beginners")]
    [InlineData(StreamingQueryComplexity.Medium, "moderate technical detail")]
    [InlineData(StreamingQueryComplexity.Complex, "detailed technical explanation")]
    [InlineData(StreamingQueryComplexity.Expert, "comprehensive technical analysis")]
    public async Task BuildSQLExplanationPromptAsync_WithDifferentComplexities_IncludesAppropriateRequirements(
        StreamingQueryComplexity complexity, string expectedRequirement)
    {
        // Arrange
        var sql = "SELECT * FROM Users WHERE Active = 1";

        // Act
        var result = await _promptManager.BuildSQLExplanationPromptAsync(sql, complexity);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(expectedRequirement, result);
    }

    [Fact]
    public void GetSQLSystemPrompt_ReturnsValidPrompt()
    {
        // Act
        var result = _promptManager.GetSQLSystemPrompt();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("expert SQL developer", result);
        Assert.Contains("business intelligence", result);
        Assert.Contains("security", result);
        Assert.Contains("performant", result);
    }

    [Fact]
    public void GetInsightSystemPrompt_ReturnsValidPrompt()
    {
        // Act
        var result = _promptManager.GetInsightSystemPrompt();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("business intelligence analyst", result);
        Assert.Contains("actionable", result);
        Assert.Contains("business value", result);
        Assert.Contains("recommendations", result);
    }

    [Fact]
    public void GetExplanationSystemPrompt_ReturnsValidPrompt()
    {
        // Act
        var result = _promptManager.GetExplanationSystemPrompt();

        // Assert
        Assert.NotNull(result);
        Assert.Contains("database expert", result);
        Assert.Contains("technical educator", result);
        Assert.Contains("step-by-step", result);
        Assert.Contains("performance considerations", result);
    }

    [Fact]
    public async Task BuildSQLGenerationPromptAsync_WithEmptyPrompt_HandlesGracefully()
    {
        // Arrange
        var userPrompt = "";

        // Act
        var result = await _promptManager.BuildSQLGenerationPromptAsync(userPrompt);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("User Request", result);
        // Should not throw exception
    }

    [Fact]
    public async Task BuildInsightGenerationPromptAsync_WithEmptyData_HandlesGracefully()
    {
        // Arrange
        var query = "SELECT COUNT(*) FROM Users";
        var data = new object[0];

        // Act
        var result = await _promptManager.BuildInsightGenerationPromptAsync(query, data);

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Total records: 0", result);
        // Should not throw exception
    }
}
