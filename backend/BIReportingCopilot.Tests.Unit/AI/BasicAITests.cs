using Xunit;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Tests.Unit.AI;

public class BasicAITests
{
    [Fact]
    public void SemanticAnalysis_CanBeCreated()
    {
        // Arrange & Act
        var analysis = new SemanticAnalysis
        {
            OriginalQuery = "test query",
            Intent = QueryIntent.General,
            ConfidenceScore = 0.8
        };

        // Assert
        Assert.NotNull(analysis);
        Assert.Equal("test query", analysis.OriginalQuery);
        Assert.Equal(QueryIntent.General, analysis.Intent);
        Assert.Equal(0.8, analysis.ConfidenceScore);
    }

    [Fact]
    public void QueryClassification_CanBeCreated()
    {
        // Arrange & Act
        var classification = new QueryClassification
        {
            Category = QueryCategory.Aggregation,
            Complexity = QueryComplexity.Medium,
            RecommendedVisualization = VisualizationType.BarChart
        };

        // Assert
        Assert.NotNull(classification);
        Assert.Equal(QueryCategory.Aggregation, classification.Category);
        Assert.Equal(QueryComplexity.Medium, classification.Complexity);
        Assert.Equal(VisualizationType.BarChart, classification.RecommendedVisualization);
    }

    [Fact]
    public void ProcessedQuery_CanBeCreated()
    {
        // Arrange & Act
        var processedQuery = new ProcessedQuery
        {
            Sql = "SELECT * FROM Sales",
            Explanation = "Test explanation",
            Confidence = 0.9
        };

        // Assert
        Assert.NotNull(processedQuery);
        Assert.Equal("SELECT * FROM Sales", processedQuery.Sql);
        Assert.Equal("Test explanation", processedQuery.Explanation);
        Assert.Equal(0.9, processedQuery.Confidence);
    }

    [Theory]
    [InlineData(QueryIntent.General)]
    [InlineData(QueryIntent.Aggregation)]
    [InlineData(QueryIntent.Trend)]
    [InlineData(QueryIntent.Comparison)]
    [InlineData(QueryIntent.Filtering)]
    public void QueryIntent_AllValuesAreValid(QueryIntent intent)
    {
        // Act & Assert
        Assert.True(Enum.IsDefined(typeof(QueryIntent), intent));
    }

    [Theory]
    [InlineData(QueryComplexity.Low)]
    [InlineData(QueryComplexity.Medium)]
    [InlineData(QueryComplexity.High)]
    public void QueryComplexity_AllValuesAreValid(QueryComplexity complexity)
    {
        // Act & Assert
        Assert.True(Enum.IsDefined(typeof(QueryComplexity), complexity));
    }

    [Theory]
    [InlineData(EntityType.Table)]
    [InlineData(EntityType.Column)]
    [InlineData(EntityType.Value)]
    [InlineData(EntityType.DateRange)]
    [InlineData(EntityType.Number)]
    [InlineData(EntityType.Aggregation)]
    [InlineData(EntityType.Condition)]
    [InlineData(EntityType.Sort)]
    [InlineData(EntityType.Limit)]
    public void EntityType_AllValuesAreValid(EntityType entityType)
    {
        // Act & Assert
        Assert.True(Enum.IsDefined(typeof(EntityType), entityType));
    }
}
