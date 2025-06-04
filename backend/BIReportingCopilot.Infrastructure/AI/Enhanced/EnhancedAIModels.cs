using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

// =============================================================================
// VISUALIZATION MODELS
// =============================================================================

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

// =============================================================================
// SQL GENERATOR MODELS
// =============================================================================

/// <summary>
/// Generated query result with enhanced metadata
/// </summary>
public class GeneratedQuery
{
    public string SQL { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public List<string> Alternatives { get; set; } = new();
    public string ExecutionPlan { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Sub-query SQL generation result
/// </summary>
public class SubQuerySQLResult
{
    public SubQuery SubQuery { get; set; } = new();
    public string SQL { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> UsedTables { get; set; } = new();
    public double EstimatedCost { get; set; }
}

/// <summary>
/// Combined SQL result
/// </summary>
public class CombinedSQLResult
{
    public string SQL { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string CombinationStrategy { get; set; } = string.Empty;
}

/// <summary>
/// Schema relationship definition
/// </summary>
public class SchemaRelationship
{
    public string FromTable { get; set; } = string.Empty;
    public string FromColumn { get; set; } = string.Empty;
    public string ToTable { get; set; } = string.Empty;
    public string ToColumn { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty; // "foreign_key", "one_to_many", etc.
    public double Confidence { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is SchemaRelationship other)
        {
            return FromTable == other.FromTable &&
                   FromColumn == other.FromColumn &&
                   ToTable == other.ToTable &&
                   ToColumn == other.ToColumn;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FromTable, FromColumn, ToTable, ToColumn);
    }
}

/// <summary>
/// Optimized SQL result
/// </summary>
public class OptimizedSQLResult
{
    public string SQL { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> Optimizations { get; set; } = new();
}

/// <summary>
/// Sub-query information for SQL generation
/// </summary>
public class SubQuery
{
    public string Id { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public List<string> RequiredTables { get; set; } = new();
    public List<string> RequiredColumns { get; set; } = new();
    public QueryComplexity Complexity { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

// =============================================================================
// ENUMERATIONS
// =============================================================================

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
