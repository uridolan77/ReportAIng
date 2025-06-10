using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Advanced chart types for enhanced visualizations
/// </summary>
public enum AdvancedChartType
{
    // Basic charts (existing)
    Bar,
    Line,
    Pie,
    Scatter,

    // Advanced charts
    Area,
    Heatmap,
    Treemap,
    Sunburst,
    Gauge,
    Radar,
    Waterfall,
    Funnel,
    Sankey,
    Candlestick,
    BoxPlot,
    Violin,
    Histogram,
    Bubble,
    Timeline,
    Gantt,
    Network,
    Choropleth,
    Parallel,
    Polar
}

/// <summary>
/// Advanced visualization configuration with enhanced features
/// </summary>
public class AdvancedVisualizationConfig : VisualizationConfig
{
    public AdvancedChartType ChartType { get; set; }
    public AnimationConfig? Animation { get; set; }
    public InteractionConfig? Interaction { get; set; }
    public new ThemeConfig? Theme { get; set; }
    public DataProcessingConfig? DataProcessing { get; set; }
    public ExportConfig? Export { get; set; }
    public AccessibilityConfig? Accessibility { get; set; }
    public PerformanceConfig? Performance { get; set; }
}

/// <summary>
/// Animation configuration for charts
/// </summary>
public class AnimationConfig
{
    public bool Enabled { get; set; } = true;
    public int Duration { get; set; } = 1000;
    public string Easing { get; set; } = "ease-in-out";
    public bool DelayByCategory { get; set; } = false;
    public int DelayIncrement { get; set; } = 100;
    public bool AnimateOnDataChange { get; set; } = true;
    public string[] AnimatedProperties { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Interaction configuration for charts
/// </summary>
public class InteractionConfig
{
    public bool EnableZoom { get; set; } = true;
    public bool EnablePan { get; set; } = true;
    public bool EnableBrush { get; set; } = false;
    public bool EnableCrosshair { get; set; } = true;
    public bool EnableTooltip { get; set; } = true;
    public bool EnableLegendToggle { get; set; } = true;
    public bool EnableDataPointSelection { get; set; } = true;
    public bool EnableDrillDown { get; set; } = false;
    public DrillDownConfig? DrillDown { get; set; }
    public TooltipConfig? Tooltip { get; set; }
}

/// <summary>
/// Drill-down configuration
/// </summary>
public class DrillDownConfig
{
    public string[] Levels { get; set; } = Array.Empty<string>();
    public Dictionary<string, string> LevelQueries { get; set; } = new();
    public bool EnableBreadcrumb { get; set; } = true;
    public int MaxDepth { get; set; } = 3;
}

/// <summary>
/// Tooltip configuration
/// </summary>
public class TooltipConfig
{
    public bool Enabled { get; set; } = true;
    public string Position { get; set; } = "auto"; // auto, top, bottom, left, right
    public string[] DisplayFields { get; set; } = Array.Empty<string>();
    public string? CustomTemplate { get; set; }
    public bool ShowStatistics { get; set; } = false;
    public bool EnableHtml { get; set; } = false;
}

/// <summary>
/// Theme configuration for charts
/// </summary>
public class ThemeConfig
{
    public string Name { get; set; } = "default";
    public ColorPalette? Colors { get; set; }
    public FontConfig? Fonts { get; set; }
    public BorderConfig? Borders { get; set; }
    public ShadowConfig? Shadows { get; set; }
    public bool DarkMode { get; set; } = false;
}

// ColorPalette moved to VisualizationModels.cs to avoid duplicates

/// <summary>
/// Gradient configuration
/// </summary>
public class GradientConfig
{
    public bool Enabled { get; set; } = false;
    public string Direction { get; set; } = "vertical"; // vertical, horizontal, radial
    public string[] Colors { get; set; } = Array.Empty<string>();
    public float[] Stops { get; set; } = Array.Empty<float>();
}

/// <summary>
/// Font configuration
/// </summary>
public class FontConfig
{
    public string Family { get; set; } = "Arial, sans-serif";
    public int TitleSize { get; set; } = 16;
    public int LabelSize { get; set; } = 12;
    public int LegendSize { get; set; } = 11;
    public int TooltipSize { get; set; } = 10;
    public string Weight { get; set; } = "normal";
}

/// <summary>
/// Border configuration
/// </summary>
public class BorderConfig
{
    public string Color { get; set; } = "#cccccc";
    public int Width { get; set; } = 1;
    public string Style { get; set; } = "solid";
    public int Radius { get; set; } = 0;
}

/// <summary>
/// Shadow configuration
/// </summary>
public class ShadowConfig
{
    public bool Enabled { get; set; } = false;
    public string Color { get; set; } = "rgba(0,0,0,0.1)";
    public int OffsetX { get; set; } = 2;
    public int OffsetY { get; set; } = 2;
    public int Blur { get; set; } = 4;
}

/// <summary>
/// Data processing configuration
/// </summary>
public class DataProcessingConfig
{
    public bool EnableSampling { get; set; } = false;
    public int SampleSize { get; set; } = 1000;
    public string SamplingMethod { get; set; } = "random"; // random, systematic, stratified
    public bool EnableAggregation { get; set; } = false;
    public AggregationConfig? Aggregation { get; set; }
    public bool EnableOutlierDetection { get; set; } = false;
    public OutlierConfig? Outliers { get; set; }
}

/// <summary>
/// Aggregation configuration
/// </summary>
public class AggregationConfig
{
    public string Method { get; set; } = "auto"; // auto, sum, avg, count, min, max
    public string GroupBy { get; set; } = string.Empty;
    public int BinCount { get; set; } = 10;
    public string TimeInterval { get; set; } = "auto"; // auto, hour, day, week, month, year
}

/// <summary>
/// Outlier detection configuration
/// </summary>
public class OutlierConfig
{
    public string Method { get; set; } = "iqr"; // iqr, zscore, isolation
    public double Threshold { get; set; } = 1.5;
    public bool RemoveOutliers { get; set; } = false;
    public bool HighlightOutliers { get; set; } = true;
    public string OutlierColor { get; set; } = "#ff0000";
}

/// <summary>
/// Export configuration
/// </summary>
public class ExportConfig
{
    public string[] SupportedFormats { get; set; } = { "PNG", "SVG", "PDF", "Excel", "CSV" };
    public int ImageWidth { get; set; } = 1200;
    public int ImageHeight { get; set; } = 800;
    public int ImageDPI { get; set; } = 300;
    public bool IncludeData { get; set; } = true;
    public bool IncludeMetadata { get; set; } = true;
    public string DefaultFilename { get; set; } = "chart";
}

/// <summary>
/// Accessibility configuration
/// </summary>
public class AccessibilityConfig
{
    public bool Enabled { get; set; } = true;
    public bool HighContrast { get; set; } = false;
    public bool ScreenReaderSupport { get; set; } = true;
    public bool KeyboardNavigation { get; set; } = true;
    public string[] AriaLabels { get; set; } = Array.Empty<string>();
    public string? Description { get; set; }
    public bool ColorBlindFriendly { get; set; } = true;
}

/// <summary>
/// Performance configuration
/// </summary>
public class PerformanceConfig
{
    public bool EnableVirtualization { get; set; } = false;
    public int VirtualizationThreshold { get; set; } = 10000;
    public bool EnableLazyLoading { get; set; } = false;
    public bool EnableCaching { get; set; } = true;
    public int CacheTTL { get; set; } = 300; // seconds
    public bool EnableWebGL { get; set; } = false;
    public int MaxDataPoints { get; set; } = 50000;
}

/// <summary>
/// Advanced dashboard configuration
/// </summary>
public class AdvancedDashboardConfig : DashboardConfig
{
    public DashboardTheme? Theme { get; set; }
    public ResponsiveConfig? Responsive { get; set; }
    public RealTimeConfig? RealTime { get; set; }
    public CollaborationConfig? Collaboration { get; set; }
    public SecurityConfig? Security { get; set; }
    public AnalyticsConfig? Analytics { get; set; }
}

/// <summary>
/// Dashboard theme configuration
/// </summary>
public class DashboardTheme
{
    public string Name { get; set; } = "default";
    public string BackgroundColor { get; set; } = "#f5f5f5";
    public string CardColor { get; set; } = "#ffffff";
    public string TextColor { get; set; } = "#333333";
    public string AccentColor { get; set; } = "#1890ff";
    public int BorderRadius { get; set; } = 8;
    public string FontFamily { get; set; } = "Arial, sans-serif";
}

/// <summary>
/// Responsive configuration
/// </summary>
public class ResponsiveConfig
{
    public bool Enabled { get; set; } = true;
    public Dictionary<string, BreakpointConfig> Breakpoints { get; set; } = new();
    public bool AutoResize { get; set; } = true;
    public bool MaintainAspectRatio { get; set; } = true;
}

/// <summary>
/// Breakpoint configuration for responsive design
/// </summary>
public class BreakpointConfig
{
    public int MinWidth { get; set; }
    public int MaxWidth { get; set; }
    public int Columns { get; set; }
    public string[] ChartSizes { get; set; } = Array.Empty<string>();
    public bool HideCharts { get; set; } = false;
    public string[] HiddenChartIds { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Real-time configuration
/// </summary>
public class RealTimeConfig
{
    public bool Enabled { get; set; } = false;
    public int RefreshInterval { get; set; } = 30; // seconds
    public bool AutoRefresh { get; set; } = true;
    public bool ShowLastUpdated { get; set; } = true;
    public bool EnableNotifications { get; set; } = false;
    public AlertConfig[] Alerts { get; set; } = Array.Empty<AlertConfig>();
}

/// <summary>
/// Alert configuration for real-time dashboards
/// </summary>
public class AlertConfig
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public string Threshold { get; set; } = string.Empty;
    public string Action { get; set; } = "notification";
    public bool Enabled { get; set; } = true;
}

/// <summary>
/// Collaboration configuration
/// </summary>
public class CollaborationConfig
{
    public bool Enabled { get; set; } = false;
    public bool AllowComments { get; set; } = true;
    public bool AllowSharing { get; set; } = true;
    public bool AllowEditing { get; set; } = false;
    public string[] SharedWith { get; set; } = Array.Empty<string>();
    public string Permission { get; set; } = "view"; // view, edit, admin
}

/// <summary>
/// Security configuration
/// </summary>
public class SecurityConfig
{
    public bool RequireAuthentication { get; set; } = true;
    public string[] AllowedRoles { get; set; } = Array.Empty<string>();
    public bool EnableRowLevelSecurity { get; set; } = false;
    public Dictionary<string, string> DataFilters { get; set; } = new();
    public bool EnableAuditLog { get; set; } = true;
}

/// <summary>
/// Global filter for dashboards
/// </summary>
public class GlobalFilter
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Column { get; set; } = string.Empty;
    public object? DefaultValue { get; set; }
    public object[]? Options { get; set; }
}



/// <summary>
/// Analytics configuration for dashboard usage tracking
/// </summary>
public class AnalyticsConfig
{
    public bool Enabled { get; set; } = false;
    public bool TrackViews { get; set; } = true;
    public bool TrackInteractions { get; set; } = true;
    public bool TrackPerformance { get; set; } = true;
    public string[] CustomEvents { get; set; } = Array.Empty<string>();
}
