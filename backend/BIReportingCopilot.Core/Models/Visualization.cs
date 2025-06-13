using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Chart types for visualizations
/// </summary>
public enum ChartType
{
    // Basic charts (existing)
    Bar,
    Line,
    Pie,
    Scatter,

    // Extended charts
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
/// Visualization configuration with enhanced features
/// </summary>
public class VisualizationConfig
{
    // Properties expected by the services
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string XAxis { get; set; } = string.Empty;
    public string YAxis { get; set; } = string.Empty;
    public List<string> Series { get; set; } = new();
    public Dictionary<string, object> Config { get; set; } = new();
    public string ColorScheme { get; set; } = string.Empty;
    public bool EnableInteractivity { get; set; } = true;
    public bool EnableAnimation { get; set; } = true;
    public VisualizationMetadata Metadata { get; set; } = new();

    // Original properties
    public ChartType ChartType { get; set; }
    public AnimationConfig? Animation { get; set; }
    public InteractionConfig? Interaction { get; set; }
    public ThemeConfig? Theme { get; set; }
    public DataProcessingConfig? DataProcessing { get; set; }
    public ExportConfig? Export { get; set; }
    public AccessibilityConfig? Accessibility { get; set; }
    public PerformanceConfig? Performance { get; set; }
}

/// <summary>
/// Type alias for interface compatibility
/// </summary>
public class VisualizationConfiguration : VisualizationConfig
{
}

/// <summary>
/// Visualization metadata
/// </summary>
public class VisualizationMetadata
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public Dictionary<string, object> Properties { get; set; } = new();

    // Properties expected by Infrastructure services
    /// <summary>
    /// Number of data points in the visualization
    /// </summary>
    public int DataPointCount { get; set; }

    /// <summary>
    /// Confidence score for the visualization recommendation
    /// </summary>
    public double ConfidenceScore { get; set; }

    /// <summary>
    /// When this visualization was generated
    /// </summary>
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optimization level for the visualization
    /// </summary>
    public string OptimizationLevel { get; set; } = "Standard";

    /// <summary>
    /// Data source for the visualization
    /// </summary>
    public string DataSource { get; set; } = string.Empty;
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

/// <summary>
/// Color palette for themes
/// </summary>
public class ColorPalette
{
    public List<string> Primary { get; set; } = new() { "#007bff", "#28a745", "#ffc107", "#dc3545" };
    public List<string> Secondary { get; set; } = new() { "#6c757d", "#17a2b8", "#fd7e14", "#e83e8c" };
    public string Accent { get; set; } = "#6f42c1";
    public string Background { get; set; } = "#ffffff";
    public string Text { get; set; } = "#333333";
    public string Grid { get; set; } = "#e0e0e0";
    public string Axis { get; set; } = "#666666";
}

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
/// Dashboard configuration
/// </summary>
public class DashboardConfig
{
    // Properties expected by Infrastructure services
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<VisualizationConfig> Charts { get; set; } = new();
    public DashboardLayout Layout { get; set; } = new();
    public List<GlobalFilter> GlobalFilters { get; set; } = new();
    public int RefreshInterval { get; set; } = 30; // seconds

    // Original properties
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

// =============================================================================
// ADVANCED VISUALIZATION MODELS (from VisualizationModels.cs)
// =============================================================================

/// <summary>
/// Advanced visualization configuration
/// </summary>
public class AdvancedVisualizationConfiguration
{
    public string ChartType { get; set; } = string.Empty;
    public Dictionary<string, object> ChartOptions { get; set; } = new();
    public List<DataSeries> DataSeries { get; set; } = new();
    public VisualizationTheme Theme { get; set; } = new();
    public InteractivityOptions Interactivity { get; set; } = new();
    public AnimationSettings Animation { get; set; } = new();
    public ResponsiveSettings Responsive { get; set; } = new();
}

/// <summary>
/// Data series for visualization
/// </summary>
public class DataSeries
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public List<DataPoint> Data { get; set; } = new();
    public SeriesStyle Style { get; set; } = new();
    public bool Visible { get; set; } = true;
}

/// <summary>
/// Individual data point
/// </summary>
public class DataPoint
{
    public object X { get; set; } = new();
    public object Y { get; set; } = new();
    public object? Z { get; set; }
    public string? Label { get; set; }
    public string? Color { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Series styling options
/// </summary>
public class SeriesStyle
{
    public string Color { get; set; } = string.Empty;
    public int LineWidth { get; set; } = 2;
    public string LineStyle { get; set; } = "solid";
    public string MarkerStyle { get; set; } = "circle";
    public int MarkerSize { get; set; } = 5;
    public double Opacity { get; set; } = 1.0;
}

/// <summary>
/// Visualization theme
/// </summary>
public class VisualizationTheme
{
    public string Name { get; set; } = "default";
    public ColorPalette Colors { get; set; } = new();
    public FontSettings Fonts { get; set; } = new();
    public string BackgroundColor { get; set; } = "#ffffff";
    public string GridColor { get; set; } = "#e0e0e0";
}

/// <summary>
/// Font settings for visualization
/// </summary>
public class FontSettings
{
    public string Family { get; set; } = "Arial, sans-serif";
    public int TitleSize { get; set; } = 16;
    public int LabelSize { get; set; } = 12;
    public int AxisSize { get; set; } = 10;
    public string TitleWeight { get; set; } = "bold";
}

/// <summary>
/// Interactivity options
/// </summary>
public class InteractivityOptions
{
    public bool EnableZoom { get; set; } = true;
    public bool EnablePan { get; set; } = true;
    public bool EnableTooltips { get; set; } = true;
    public bool EnableLegendToggle { get; set; } = true;
    public bool EnableDataSelection { get; set; } = false;
    public bool EnableCrosshair { get; set; } = false;
}

/// <summary>
/// Animation settings
/// </summary>
public class AnimationSettings
{
    public bool Enabled { get; set; } = true;
    public int Duration { get; set; } = 1000;
    public string Easing { get; set; } = "ease-in-out";
    public bool EnableDataTransitions { get; set; } = true;
}

/// <summary>
/// Responsive settings
/// </summary>
public class ResponsiveSettings
{
    public bool Enabled { get; set; } = true;
    public List<ResponsiveBreakpoint> Breakpoints { get; set; } = new();
    public bool MaintainAspectRatio { get; set; } = true;
}

/// <summary>
/// Responsive breakpoint
/// </summary>
public class ResponsiveBreakpoint
{
    public int Width { get; set; }
    public Dictionary<string, object> Options { get; set; } = new();
}

// =============================================================================
// AI ANALYSIS MODELS
// =============================================================================

/// <summary>
/// AI-powered data analysis result
/// </summary>
public class AIDataAnalysisResult
{
    public string AnalysisId { get; set; } = string.Empty;
    public List<DataInsight> Insights { get; set; } = new();
    public List<DataPattern> Patterns { get; set; } = new();
    public List<DataAnomaly> Anomalies { get; set; } = new();
    public List<VisualizationRecommendation> Recommendations { get; set; } = new();
    public AnalysisMetadata Metadata { get; set; } = new();
    public double ConfidenceScore { get; set; }
}

/// <summary>
/// Data pattern identified by AI
/// </summary>
public class DataPattern
{
    public string PatternId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Strength { get; set; }
    public List<string> Columns { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
    public TimeSpan? TemporalScope { get; set; }
}

/// <summary>
/// Data anomaly detected by AI
/// </summary>
public class DataAnomaly
{
    public string AnomalyId { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Severity { get; set; }
    public double Confidence { get; set; }
    public List<string> AffectedRows { get; set; } = new();
    public List<string> AffectedColumns { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// AI visualization recommendation
/// </summary>
public class VisualizationRecommendation
{
    public string RecommendationId { get; set; } = string.Empty;
    public string ChartType { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public double Suitability { get; set; }
    public List<string> RequiredColumns { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
    public List<string> Benefits { get; set; } = new();
    public double Confidence { get; set; } = 0.8;
    public string BestFor { get; set; } = string.Empty;
    public List<string> Limitations { get; set; } = new();
    public string EstimatedPerformance { get; set; } = "Good";
    public Dictionary<string, object> SuggestedConfig { get; set; } = new();
}

/// <summary>
/// Analysis metadata
/// </summary>
public class AnalysisMetadata
{
    public DateTime AnalyzedAt { get; set; }
    public TimeSpan ProcessingTime { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
    public int DataPointsAnalyzed { get; set; }
    public List<string> AnalysisMethods { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

// =============================================================================
// SMART CHART MODELS
// =============================================================================

/// <summary>
/// Smart chart configuration with AI recommendations
/// </summary>
public class SmartChartConfiguration
{
    public string ChartId { get; set; } = string.Empty;
    public string RecommendedType { get; set; } = string.Empty;
    public List<string> AlternativeTypes { get; set; } = new();
    public SmartDataMapping DataMapping { get; set; } = new();
    public SmartStyling Styling { get; set; } = new();
    public List<SmartFeature> Features { get; set; } = new();
    public double ConfidenceScore { get; set; }
}

/// <summary>
/// Smart data mapping for charts
/// </summary>
public class SmartDataMapping
{
    public string XAxis { get; set; } = string.Empty;
    public string YAxis { get; set; } = string.Empty;
    public string? ZAxis { get; set; }
    public string? ColorBy { get; set; }
    public string? SizeBy { get; set; }
    public List<string> GroupBy { get; set; } = new();
    public Dictionary<string, string> CustomMappings { get; set; } = new();
}

/// <summary>
/// Smart styling recommendations
/// </summary>
public class SmartStyling
{
    public string RecommendedTheme { get; set; } = string.Empty;
    public List<string> ColorScheme { get; set; } = new();
    public string Layout { get; set; } = string.Empty;
    public Dictionary<string, object> CustomStyles { get; set; } = new();
}

/// <summary>
/// Smart chart feature
/// </summary>
public class SmartFeature
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Enabled { get; set; }
    public Dictionary<string, object> Configuration { get; set; } = new();
    public decimal Relevance { get; set; }
}

// =============================================================================
// PREDICTIVE ANALYTICS MODELS
// =============================================================================

/// <summary>
/// Predictive analytics configuration
/// </summary>
public class PredictiveAnalyticsConfiguration
{
    public string ModelType { get; set; } = string.Empty;
    public string TargetColumn { get; set; } = string.Empty;
    public List<string> FeatureColumns { get; set; } = new();
    public TimeSpan PredictionHorizon { get; set; }
    public double ConfidenceInterval { get; set; } = 0.95;
    public Dictionary<string, object> ModelParameters { get; set; } = new();
}

/// <summary>
/// Predictive analytics result
/// </summary>
public class PredictiveAnalyticsResult
{
    public string PredictionId { get; set; } = string.Empty;
    public List<PredictionPoint> Predictions { get; set; } = new();
    public ModelPerformanceMetrics Performance { get; set; } = new();
    public List<FeatureImportance> FeatureImportances { get; set; } = new();
    public PredictionMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Individual prediction point
/// </summary>
public class PredictionPoint
{
    public DateTime Timestamp { get; set; }
    public double PredictedValue { get; set; }
    public double ConfidenceLower { get; set; }
    public double ConfidenceUpper { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Model performance metrics
/// </summary>
public class ModelPerformanceMetrics
{
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public double MeanAbsoluteError { get; set; }
    public double RootMeanSquareError { get; set; }
}

/// <summary>
/// Feature importance for predictions
/// </summary>
public class FeatureImportance
{
    public string FeatureName { get; set; } = string.Empty;
    public double Importance { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Prediction metadata
/// </summary>
public class PredictionMetadata
{
    public DateTime GeneratedAt { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
    public TimeSpan TrainingTime { get; set; }
    public int TrainingSamples { get; set; }
    public Dictionary<string, object> Hyperparameters { get; set; } = new();
}

// =============================================================================
// MISSING CLASSES FOR COMPILATION
// =============================================================================

/// <summary>
/// Real-time configuration for dashboards
/// </summary>
public class RealTimeConfig
{
    public bool Enabled { get; set; } = false;
    public int RefreshInterval { get; set; } = 30; // seconds
    public bool AutoRefresh { get; set; } = false;
    public bool ShowLastUpdated { get; set; } = true;
    public bool EnableNotifications { get; set; } = false;
}

/// <summary>
/// Collaboration configuration for dashboards
/// </summary>
public class CollaborationConfig
{
    public bool Enabled { get; set; } = false;
    public bool AllowSharing { get; set; } = true;
    public bool AllowComments { get; set; } = true;
    public bool AllowAnnotations { get; set; } = false;
    public List<string> SharedWith { get; set; } = new();
}

/// <summary>
/// Security configuration for dashboards
/// </summary>
public class SecurityConfig
{
    public bool RequireAuthentication { get; set; } = true;
    public List<string> AllowedRoles { get; set; } = new();
    public bool EnableAuditLog { get; set; } = true;
    public bool EnableDataMasking { get; set; } = false;
    public Dictionary<string, object> SecurityPolicies { get; set; } = new();
}

/// <summary>
/// Analytics configuration for dashboards
/// </summary>
public class AnalyticsConfig
{
    public bool Enabled { get; set; } = true;
    public bool TrackUsage { get; set; } = true;
    public bool TrackPerformance { get; set; } = true;
    public bool TrackUserInteractions { get; set; } = false;
    public int RetentionDays { get; set; } = 90;
}

// =============================================================================
// MISSING VISUALIZATION MODELS
// =============================================================================

/// <summary>
/// Advanced visualization configuration
/// </summary>
public class AdvancedVisualizationConfig : VisualizationConfig
{
    public new bool EnableInteractivity { get; set; } = true;
    public new bool EnableAnimation { get; set; } = true;
    public List<string> SupportedInteractions { get; set; } = new();
    public Dictionary<string, object> AdvancedSettings { get; set; } = new();
}

/// <summary>
/// Advanced dashboard configuration
/// </summary>
public class AdvancedDashboardConfig : DashboardConfig
{
    public new RealTimeConfig RealTime { get; set; } = new();
    public new CollaborationConfig Collaboration { get; set; } = new();
    public new SecurityConfig Security { get; set; } = new();
    public new AnalyticsConfig Analytics { get; set; } = new();
}

/// <summary>
/// Visualization preferences
/// </summary>
public class VisualizationPreferences
{
    public string UserId { get; set; } = string.Empty;
    public string PreferredChartType { get; set; } = "bar";
    public string ColorScheme { get; set; } = "business";
    public bool EnableAnimations { get; set; } = true;
    public bool EnableInteractivity { get; set; } = true;
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

/// <summary>
/// Dashboard preferences
/// </summary>
public class DashboardPreferences
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty; // Added for service compatibility
    public string PreferredLayout { get; set; } = "grid";
    public int DefaultColumns { get; set; } = 12;
    public int DefaultRows { get; set; } = 6;
    public bool EnableAutoRefresh { get; set; } = false;
    public int RefreshInterval { get; set; } = 30;
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}


