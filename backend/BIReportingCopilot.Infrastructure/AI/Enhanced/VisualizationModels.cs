using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Visualization recommendations result
/// </summary>
public class VisualizationRecommendations
{
    public ChartRecommendation? PrimaryVisualization { get; set; }
    public List<ChartRecommendation> AlternativeVisualizations { get; set; } = new();
    public DashboardLayoutRecommendation? DashboardLayout { get; set; }
    public List<InteractiveFeature> InteractiveFeatures { get; set; } = new();
    public List<ChartConfiguration> ChartConfigurations { get; set; } = new();
    public List<DataInsight> DataInsights { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Chart recommendation with confidence score
/// </summary>
public class ChartRecommendation
{
    public ChartType ChartType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> Reasons { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// Dashboard layout recommendation
/// </summary>
public class DashboardLayoutRecommendation
{
    public LayoutType LayoutType { get; set; }
    public int Columns { get; set; }
    public List<PanelPlacement> PanelPlacements { get; set; } = new();
    public Dictionary<string, object> LayoutParameters { get; set; } = new();
}

/// <summary>
/// Panel placement in dashboard
/// </summary>
public class PanelPlacement
{
    public string PanelId { get; set; } = string.Empty;
    public PanelPosition Position { get; set; } = new();
    public PanelSize Size { get; set; } = new();
    public int Priority { get; set; }
}

/// <summary>
/// Interactive feature definition
/// </summary>
public class InteractiveFeature
{
    public InteractiveFeatureType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
}

/// <summary>
/// Data insight generated from analysis
/// </summary>
public class DataInsight
{
    public InsightType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// Dashboard configuration
/// </summary>
public class DashboardConfiguration
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<DashboardPanel> Panels { get; set; } = new();
    public List<GlobalFilter> GlobalFilters { get; set; } = new();
    public DashboardTheme Theme { get; set; }
    public DashboardLayout Layout { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Dashboard panel definition
/// </summary>
public class DashboardPanel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public ChartType ChartType { get; set; }
    public ChartConfiguration? Configuration { get; set; }
    public List<InteractiveFeature> InteractiveFeatures { get; set; } = new();
    public PanelPosition Position { get; set; } = new();
    public PanelSize Size { get; set; } = new();
}

/// <summary>
/// Panel position in dashboard grid
/// </summary>
public class PanelPosition
{
    public int Row { get; set; }
    public int Column { get; set; }
}

/// <summary>
/// Panel size in dashboard grid
/// </summary>
public class PanelSize
{
    public int Width { get; set; }
    public int Height { get; set; }
}

/// <summary>
/// Chart configuration for rendering
/// </summary>
public class ChartConfiguration
{
    public ChartType ChartType { get; set; }
    public string Title { get; set; } = string.Empty;
    public AxisConfiguration? XAxis { get; set; }
    public AxisConfiguration? YAxis { get; set; }
    public string? DataKey { get; set; }
    public string? NameKey { get; set; }
    public string[] Colors { get; set; } = Array.Empty<string>();
    public bool ShowLegend { get; set; }
    public bool ShowTooltip { get; set; }
    public bool ShowPagination { get; set; }
    public int PageSize { get; set; } = 25;
    public bool Sortable { get; set; }
    public bool Filterable { get; set; }
    public bool Smooth { get; set; }
    public Dictionary<string, object> CustomProperties { get; set; } = new();
}

/// <summary>
/// Axis configuration for charts
/// </summary>
public class AxisConfiguration
{
    public string Title { get; set; } = string.Empty;
    public string DataKey { get; set; } = string.Empty;
    public string Type { get; set; } = "category"; // category, value, datetime
    public bool ShowGrid { get; set; } = true;
    public string? Format { get; set; }
}

/// <summary>
/// Global filter for dashboard
/// </summary>
public class GlobalFilter
{
    public string Name { get; set; } = string.Empty;
    public FilterType Type { get; set; }
    public string Label { get; set; } = string.Empty;
    public List<string> Values { get; set; } = new();
    public object? DefaultValue { get; set; }
}

/// <summary>
/// Dashboard layout configuration
/// </summary>
public class DashboardLayout
{
    public LayoutType Type { get; set; }
    public int Columns { get; set; }
    public Dictionary<string, int> ResponsiveBreakpoints { get; set; } = new();
    public string Spacing { get; set; } = "medium";
}

/// <summary>
/// Chart customization options
/// </summary>
public class ChartCustomization
{
    public string? Title { get; set; }
    public string[]? Colors { get; set; }
    public Dictionary<string, object> CustomProperties { get; set; } = new();
}

/// <summary>
/// Data characteristics for visualization analysis
/// </summary>
public class DataCharacteristics
{
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public List<ColumnCharacteristics> Columns { get; set; } = new();
    public bool HasTimeSeriesData { get; set; }
    public bool HasCategoricalData { get; set; }
    public bool HasNumericData { get; set; }
    public bool IsAggregatedData { get; set; }
}

/// <summary>
/// Individual column characteristics
/// </summary>
public class ColumnCharacteristics
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNumeric { get; set; }
    public bool IsDateTime { get; set; }
    public bool IsCategorical { get; set; }
    public int UniqueValueCount { get; set; }
    public bool HasNulls { get; set; }
}

/// <summary>
/// Column statistics
/// </summary>
public class ColumnStatistics
{
    public double Average { get; set; }
    public double Maximum { get; set; }
    public double Minimum { get; set; }
    public int Count { get; set; }
}

/// <summary>
/// Chart types supported by the visualization engine
/// </summary>
public enum ChartType
{
    Table,
    BarChart,
    LineChart,
    PieChart,
    ScatterPlot,
    Heatmap,
    AreaChart,
    Histogram,
    BoxPlot,
    TreeMap
}

/// <summary>
/// Interactive feature types
/// </summary>
public enum InteractiveFeatureType
{
    Filter,
    DrillDown,
    TimeRange,
    Export,
    Zoom,
    Brush,
    Tooltip,
    CrossFilter
}

/// <summary>
/// Data insight types
/// </summary>
public enum InsightType
{
    Statistical,
    Trend,
    Distribution,
    Correlation,
    Outlier,
    Pattern
}

/// <summary>
/// Dashboard themes
/// </summary>
public enum DashboardTheme
{
    Light,
    Dark,
    Modern,
    Professional,
    Colorful
}

/// <summary>
/// Layout types for dashboards
/// </summary>
public enum LayoutType
{
    Grid,
    Masonry,
    Flexible,
    Fixed
}

/// <summary>
/// Filter types
/// </summary>
public enum FilterType
{
    Categorical,
    Numeric,
    DateTime,
    Text
}

/// <summary>
/// Visualization recommendation engine
/// </summary>
public class VisualizationRecommendationEngine
{
    private readonly ILogger _logger;

    public VisualizationRecommendationEngine(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<List<ChartRecommendation>> GenerateChartRecommendationsAsync(
        DataCharacteristics dataCharacteristics,
        SemanticAnalysis semanticAnalysis)
    {
        var recommendations = new List<ChartRecommendation>();

        try
        {
            // Rule-based recommendations based on data characteristics
            if (dataCharacteristics.HasTimeSeriesData && dataCharacteristics.HasNumericData)
            {
                recommendations.Add(new ChartRecommendation
                {
                    ChartType = ChartType.LineChart,
                    Title = "Time Series Analysis",
                    Description = "Shows trends over time",
                    Confidence = 0.9,
                    Reasons = new List<string> { "Time series data detected", "Numeric values present" }
                });
            }

            if (dataCharacteristics.HasCategoricalData && dataCharacteristics.HasNumericData)
            {
                recommendations.Add(new ChartRecommendation
                {
                    ChartType = ChartType.BarChart,
                    Title = "Category Comparison",
                    Description = "Compares values across categories",
                    Confidence = 0.85,
                    Reasons = new List<string> { "Categorical data detected", "Numeric values for comparison" }
                });

                // Add pie chart for small number of categories
                var categoricalColumn = dataCharacteristics.Columns.FirstOrDefault(c => c.IsCategorical);
                if (categoricalColumn != null && categoricalColumn.UniqueValueCount <= 8)
                {
                    recommendations.Add(new ChartRecommendation
                    {
                        ChartType = ChartType.PieChart,
                        Title = "Distribution Analysis",
                        Description = "Shows proportion of each category",
                        Confidence = 0.75,
                        Reasons = new List<string> { "Small number of categories", "Good for showing proportions" }
                    });
                }
            }

            // Multiple numeric columns suggest scatter plot
            var numericColumns = dataCharacteristics.Columns.Where(c => c.IsNumeric).ToList();
            if (numericColumns.Count >= 2)
            {
                recommendations.Add(new ChartRecommendation
                {
                    ChartType = ChartType.ScatterPlot,
                    Title = "Correlation Analysis",
                    Description = "Shows relationship between numeric variables",
                    Confidence = 0.7,
                    Reasons = new List<string> { "Multiple numeric columns", "Good for correlation analysis" }
                });
            }

            // Always include table as fallback
            recommendations.Add(new ChartRecommendation
            {
                ChartType = ChartType.Table,
                Title = "Data Table",
                Description = "Detailed view of all data",
                Confidence = 0.6,
                Reasons = new List<string> { "Always available", "Shows all data details" }
            });

            // Sort by confidence and return top recommendations
            return recommendations.OrderByDescending(r => r.Confidence).Take(4).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chart recommendations");
            return new List<ChartRecommendation>
            {
                new ChartRecommendation
                {
                    ChartType = ChartType.Table,
                    Title = "Data Table",
                    Description = "Fallback table view",
                    Confidence = 0.5
                }
            };
        }
    }
}

/// <summary>
/// Chart configuration generator
/// </summary>
public class ChartConfigurationGenerator
{
    private readonly ILogger _logger;

    public ChartConfigurationGenerator(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<List<ChartConfiguration>> GenerateChartConfigurationsAsync(
        List<ChartRecommendation> recommendations,
        QueryResult queryResult)
    {
        var configurations = new List<ChartConfiguration>();

        foreach (var recommendation in recommendations)
        {
            try
            {
                var config = await GenerateConfigurationForChartTypeAsync(
                    recommendation.ChartType, queryResult, recommendation);
                configurations.Add(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating configuration for {ChartType}", recommendation.ChartType);
            }
        }

        return configurations;
    }

    private async Task<ChartConfiguration> GenerateConfigurationForChartTypeAsync(
        ChartType chartType,
        QueryResult queryResult,
        ChartRecommendation recommendation)
    {
        // This would contain the specific logic for each chart type
        // For now, return a basic configuration
        return new ChartConfiguration
        {
            ChartType = chartType,
            Title = recommendation.Title,
            ShowLegend = true,
            ShowTooltip = true,
            Colors = new[] { "#3B82F6", "#10B981", "#F59E0B", "#EF4444" }
        };
    }
}

/// <summary>
/// Dashboard layout optimizer
/// </summary>
public class DashboardLayoutOptimizer
{
    private readonly ILogger _logger;

    public DashboardLayoutOptimizer(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<DashboardLayoutRecommendation> GenerateDashboardLayoutAsync(
        List<ChartRecommendation> chartRecommendations,
        DataCharacteristics dataCharacteristics)
    {
        return new DashboardLayoutRecommendation
        {
            LayoutType = LayoutType.Grid,
            Columns = 2,
            PanelPlacements = GeneratePanelPlacements(chartRecommendations)
        };
    }

    public async Task<List<DashboardPanel>> OptimizeDashboardLayoutAsync(List<DashboardPanel> panels)
    {
        // Simple optimization - arrange panels by priority and size
        return panels.OrderBy(p => p.Position.Row).ThenBy(p => p.Position.Column).ToList();
    }

    private List<PanelPlacement> GeneratePanelPlacements(List<ChartRecommendation> recommendations)
    {
        var placements = new List<PanelPlacement>();

        for (int i = 0; i < recommendations.Count; i++)
        {
            placements.Add(new PanelPlacement
            {
                PanelId = $"panel_{i}",
                Position = new PanelPosition { Row = i / 2, Column = i % 2 },
                Size = new PanelSize { Width = 6, Height = 4 },
                Priority = i
            });
        }

        return placements;
    }
}
