using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.Services;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using FluentAssertions;

namespace BIReportingCopilot.Tests.Unit.Services;

public class VisualizationServiceTests
{
    private readonly Mock<ILogger<VisualizationService>> _mockLogger;
    private readonly Mock<IAIService> _mockAIService;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly VisualizationService _service;

    public VisualizationServiceTests()
    {
        _mockLogger = new Mock<ILogger<VisualizationService>>();
        _mockAIService = new Mock<IAIService>();
        _mockCacheService = new Mock<ICacheService>();

        _service = new VisualizationService(
            _mockAIService.Object,
            _mockLogger.Object,
            _mockCacheService.Object);
    }

    [Fact]
    public async Task GenerateAdvancedVisualizationAsync_WithValidData_ReturnsAdvancedConfig()
    {
        // Arrange
        var query = "SELECT sales, region FROM sales_data";
        var columns = new[]
        {
            new ColumnInfo { Name = "sales", DataType = "decimal", IsNullable = false },
            new ColumnInfo { Name = "region", DataType = "varchar", IsNullable = false }
        };
        var data = new object[]
        {
            new { sales = 1000, region = "North" },
            new { sales = 1500, region = "South" }
        };

        var baseConfig = new VisualizationConfig
        {
            Type = "bar",
            Title = "Sales by Region",
            XAxis = "region",
            YAxis = "sales"
        };

        _mockAIService
            .Setup(x => x.GenerateVisualizationConfigAsync(query, columns, data))
            .ReturnsAsync("{\"type\":\"bar\",\"title\":\"Sales by Region\"}");

        // Act
        var result = await _service.GenerateAdvancedVisualizationAsync(query, columns, data);

        // Assert
        result.Should().NotBeNull();
        result.ChartType.Should().Be(AdvancedChartType.Bar);
        result.Title.Should().Be("Sales by Region");
        result.Animation.Should().NotBeNull();
        result.Interaction.Should().NotBeNull();
        result.Theme.Should().NotBeNull();
        result.Performance.Should().NotBeNull();
    }

    [Fact]
    public async Task GenerateAdvancedDashboardAsync_WithValidData_ReturnsAdvancedDashboard()
    {
        // Arrange
        var query = "SELECT sales, region, date FROM sales_data";
        var columns = new[]
        {
            new ColumnInfo { Name = "sales", DataType = "decimal", IsNullable = false },
            new ColumnInfo { Name = "region", DataType = "varchar", IsNullable = false },
            new ColumnInfo { Name = "date", DataType = "datetime", IsNullable = false }
        };
        var data = new object[]
        {
            new { sales = 1000, region = "North", date = DateTime.Now },
            new { sales = 1500, region = "South", date = DateTime.Now.AddDays(-1) }
        };

        var baseConfig = new VisualizationConfig
        {
            Type = "bar",
            Title = "Sales Analysis",
            XAxis = "region",
            YAxis = "sales"
        };

        _mockAIService
            .Setup(x => x.GenerateVisualizationConfigAsync(query, columns, data))
            .ReturnsAsync("{\"type\":\"bar\",\"title\":\"Sales Analysis\"}");

        // Act
        var result = await _service.GenerateAdvancedDashboardAsync(query, columns, data);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Contain("Advanced Analysis");
        result.Charts.Should().NotBeEmpty();
        result.Layout.Should().NotBeNull();
        result.Theme.Should().NotBeNull();
        result.Responsive.Should().NotBeNull();
    }

    [Fact]
    public async Task GetVisualizationRecommendationsAsync_WithNumericData_ReturnsMultipleRecommendations()
    {
        // Arrange
        var columns = new[]
        {
            new ColumnInfo { Name = "sales", DataType = "decimal", IsNullable = false },
            new ColumnInfo { Name = "profit", DataType = "decimal", IsNullable = false },
            new ColumnInfo { Name = "region", DataType = "varchar", IsNullable = false }
        };
        var data = new object[]
        {
            new { sales = 1000, profit = 200, region = "North" },
            new { sales = 1500, profit = 300, region = "South" }
        };

        // Act
        var result = await _service.GetVisualizationRecommendationsAsync(columns, data);

        // Assert
        result.Should().NotBeNull();
        result.Should().NotBeEmpty();
        result.Should().Contain(r => r.ChartType == AdvancedChartType.Bar);
        result.Should().Contain(r => r.ChartType == AdvancedChartType.Scatter);
        result.Should().OnlyContain(r => r.Confidence >= 0.0 && r.Confidence <= 1.0);
        result.Should().OnlyContain(r => !string.IsNullOrEmpty(r.Reasoning));
    }

    [Fact]
    public async Task GetVisualizationRecommendationsAsync_WithTimeSeriesData_RecommendsTrendCharts()
    {
        // Arrange
        var columns = new[]
        {
            new ColumnInfo { Name = "date", DataType = "datetime", IsNullable = false },
            new ColumnInfo { Name = "value", DataType = "decimal", IsNullable = false }
        };
        var data = new object[]
        {
            new { date = DateTime.Now.AddDays(-30), value = 100 },
            new { date = DateTime.Now.AddDays(-15), value = 150 },
            new { date = DateTime.Now, value = 200 }
        };

        // Act
        var result = await _service.GetVisualizationRecommendationsAsync(columns, data);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(r => r.ChartType == AdvancedChartType.Line);
        result.Should().Contain(r => r.ChartType == AdvancedChartType.Area);
        result.Should().Contain(r => r.ChartType == AdvancedChartType.Timeline);

        var lineRecommendation = result.First(r => r.ChartType == AdvancedChartType.Line);
        lineRecommendation.Confidence.Should().BeGreaterThan(0.7);
        lineRecommendation.BestFor.ToLower().Should().Contain("time series");
    }

    [Fact]
    public async Task GetVisualizationRecommendationsAsync_WithSingleValue_RecommendsGauge()
    {
        // Arrange
        var columns = new[]
        {
            new ColumnInfo { Name = "kpi_value", DataType = "decimal", IsNullable = false }
        };
        var data = new object[]
        {
            new { kpi_value = 85.5 }
        };

        // Act
        var result = await _service.GetVisualizationRecommendationsAsync(columns, data);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain(r => r.ChartType == AdvancedChartType.Gauge);

        var gaugeRecommendation = result.First(r => r.ChartType == AdvancedChartType.Gauge);
        gaugeRecommendation.Confidence.Should().BeGreaterThan(0.8);
        gaugeRecommendation.BestFor.Should().Contain("KPI");
    }

    [Fact]
    public async Task OptimizeVisualizationForPerformanceAsync_WithLargeDataset_EnablesOptimizations()
    {
        // Arrange
        var config = new AdvancedVisualizationConfig
        {
            ChartType = AdvancedChartType.Scatter,
            Animation = new AnimationConfig { Enabled = true, Duration = 1000 },
            DataProcessing = new DataProcessingConfig { EnableSampling = false },
            Performance = new PerformanceConfig { EnableVirtualization = false }
        };
        var dataSize = 100000;

        // Act
        var result = await _service.OptimizeVisualizationForPerformanceAsync(config, dataSize);

        // Assert
        result.Should().NotBeNull();
        result.Performance!.EnableVirtualization.Should().BeTrue();
        result.Performance.EnableWebGL.Should().BeTrue();
        result.DataProcessing!.EnableSampling.Should().BeTrue();
        result.Animation!.Enabled.Should().BeFalse();
    }

    [Fact]
    public async Task OptimizeVisualizationForPerformanceAsync_WithSmallDataset_MaintainsQuality()
    {
        // Arrange
        var config = new AdvancedVisualizationConfig
        {
            ChartType = AdvancedChartType.Bar,
            Animation = new AnimationConfig { Enabled = true, Duration = 1000 },
            DataProcessing = new DataProcessingConfig { EnableSampling = false },
            Performance = new PerformanceConfig { EnableVirtualization = false }
        };
        var dataSize = 100;

        // Act
        var result = await _service.OptimizeVisualizationForPerformanceAsync(config, dataSize);

        // Assert
        result.Should().NotBeNull();
        result.Animation!.Enabled.Should().BeTrue();
        result.DataProcessing!.EnableSampling.Should().BeFalse();
        result.Performance!.EnableVirtualization.Should().BeFalse();
    }

    [Fact]
    public async Task GenerateAdvancedVisualizationAsync_WithHierarchicalData_ReturnsTreemap()
    {
        // Arrange
        var query = "SELECT category, subcategory, sales FROM product_sales";
        var columns = new[]
        {
            new ColumnInfo { Name = "category", DataType = "varchar", IsNullable = false },
            new ColumnInfo { Name = "subcategory", DataType = "varchar", IsNullable = false },
            new ColumnInfo { Name = "sales", DataType = "decimal", IsNullable = false }
        };
        var data = new object[]
        {
            new { category = "Electronics", subcategory = "Phones", sales = 1000 },
            new { category = "Electronics", subcategory = "Laptops", sales = 1500 },
            new { category = "Clothing", subcategory = "Shirts", sales = 800 }
        };

        var baseConfig = new VisualizationConfig
        {
            Type = "treemap",
            Title = "Product Sales Hierarchy",
            XAxis = "category",
            YAxis = "sales"
        };

        _mockAIService
            .Setup(x => x.GenerateVisualizationConfigAsync(query, columns, data))
            .ReturnsAsync("{\"type\":\"treemap\",\"title\":\"Product Sales Hierarchy\"}");

        // Act
        var result = await _service.GenerateAdvancedVisualizationAsync(query, columns, data);

        // Assert
        result.Should().NotBeNull();
        result.ChartType.Should().Be(AdvancedChartType.Treemap);
        result.Interaction!.EnableDrillDown.Should().BeTrue();
        result.Interaction.DrillDown.Should().NotBeNull();
        result.Interaction.DrillDown!.Levels.Should().Contain("category");
    }

    [Fact]
    public async Task GenerateAdvancedVisualizationAsync_WithCorrelationData_ReturnsHeatmap()
    {
        // Arrange
        var query = "SELECT sales, marketing, profit FROM business_metrics";
        var columns = new[]
        {
            new ColumnInfo { Name = "sales", DataType = "decimal", IsNullable = false },
            new ColumnInfo { Name = "marketing", DataType = "decimal", IsNullable = false },
            new ColumnInfo { Name = "profit", DataType = "decimal", IsNullable = false }
        };
        var data = new object[1000]; // Large dataset for correlation analysis

        var baseConfig = new VisualizationConfig
        {
            Type = "heatmap",
            Title = "Business Metrics Correlation",
            XAxis = "sales",
            YAxis = "marketing"
        };

        _mockAIService
            .Setup(x => x.GenerateVisualizationConfigAsync(query, columns, data))
            .ReturnsAsync("{\"type\":\"heatmap\",\"title\":\"Business Metrics Correlation\"}");

        // Act
        var result = await _service.GenerateAdvancedVisualizationAsync(query, columns, data);

        // Assert
        result.Should().NotBeNull();
        result.ChartType.Should().Be(AdvancedChartType.Heatmap);
        result.Config.Should().ContainKey("colorScheme");
        result.Config["colorScheme"].Should().Be("viridis");
    }

    [Theory]
    [InlineData(AdvancedChartType.Bar, "Category comparison, ranking analysis")]
    [InlineData(AdvancedChartType.Line, "Time series analysis, trend identification")]
    [InlineData(AdvancedChartType.Heatmap, "Correlation analysis, pattern detection")]
    [InlineData(AdvancedChartType.Gauge, "KPI dashboards, performance monitoring")]
    public async Task GetVisualizationRecommendationsAsync_ReturnsCorrectUseCases(
        AdvancedChartType chartType,
        string expectedUseCase)
    {
        // Arrange
        var columns = new[]
        {
            new ColumnInfo { Name = "value", DataType = "decimal", IsNullable = false },
            new ColumnInfo { Name = "category", DataType = "varchar", IsNullable = false }
        };
        var data = new object[] { new { value = 100, category = "A" } };

        // Act
        var result = await _service.GetVisualizationRecommendationsAsync(columns, data);

        // Assert
        var recommendation = result.FirstOrDefault(r => r.ChartType == chartType);
        if (recommendation != null)
        {
            recommendation.BestFor.Should().Be(expectedUseCase);
        }
    }

    [Fact]
    public async Task GenerateAdvancedDashboardAsync_WithPreferences_AppliesCustomizations()
    {
        // Arrange
        var query = "SELECT * FROM sales_data";
        var columns = new[]
        {
            new ColumnInfo { Name = "sales", DataType = "decimal", IsNullable = false }
        };
        var data = new object[] { new { sales = 1000 } };
        var preferences = new DashboardPreferences
        {
            Title = "Custom Dashboard",
            ThemeName = "dark",
            EnableRealTime = true,
            EnableCollaboration = true,
            Layout = LayoutPreference.Grid
        };

        var baseConfig = new VisualizationConfig
        {
            Type = "bar",
            Title = "Sales",
            XAxis = "category",
            YAxis = "sales"
        };

        _mockAIService
            .Setup(x => x.GenerateVisualizationConfigAsync(query, columns, data))
            .ReturnsAsync("{\"type\":\"bar\",\"title\":\"Sales\"}");

        // Act
        var result = await _service.GenerateAdvancedDashboardAsync(query, columns, data, preferences);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Custom Dashboard");
        result.Theme!.Name.Should().Be("dark");
        result.RealTime!.Enabled.Should().BeTrue();
        result.Collaboration!.Enabled.Should().BeTrue();
        result.Layout.Type.Should().Be("grid");
    }
}
