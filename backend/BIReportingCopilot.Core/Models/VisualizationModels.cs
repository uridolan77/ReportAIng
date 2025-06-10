using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

// =============================================================================
// VISUALIZATION MODELS
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
/// Color palette for themes
/// </summary>
public class ColorPalette
{
    public List<string> Primary { get; set; } = new() { "#007bff", "#28a745", "#ffc107", "#dc3545" };
    public List<string> Secondary { get; set; } = new() { "#6c757d", "#17a2b8", "#fd7e14", "#e83e8c" };
    public string Accent { get; set; } = "#6f42c1";

    // Additional properties expected by Infrastructure services
    public string Background { get; set; } = "#ffffff";
    public string Text { get; set; } = "#333333";
    public string Grid { get; set; } = "#e0e0e0";
    public string Axis { get; set; } = "#666666";
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

// DataInsight moved to ReportingModels.cs to avoid duplicates

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

    // Additional properties expected by Infrastructure services
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
    public double Relevance { get; set; }
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
