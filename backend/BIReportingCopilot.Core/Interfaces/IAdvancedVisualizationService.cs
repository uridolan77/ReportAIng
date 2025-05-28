using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for advanced visualization service with sophisticated chart generation
/// </summary>
public interface IAdvancedVisualizationService
{
    /// <summary>
    /// Generate advanced visualization configuration with AI-powered recommendations
    /// </summary>
    Task<AdvancedVisualizationConfig> GenerateAdvancedVisualizationAsync(
        string query,
        ColumnInfo[] columns,
        object[] data,
        VisualizationPreferences? preferences = null);

    /// <summary>
    /// Generate advanced dashboard with multiple sophisticated charts
    /// </summary>
    Task<AdvancedDashboardConfig> GenerateAdvancedDashboardAsync(
        string query,
        ColumnInfo[] columns,
        object[] data,
        DashboardPreferences? preferences = null);

    /// <summary>
    /// Get AI-powered visualization recommendations based on data characteristics
    /// </summary>
    Task<VisualizationRecommendation[]> GetVisualizationRecommendationsAsync(
        ColumnInfo[] columns,
        object[] data,
        string? context = null);

    /// <summary>
    /// Optimize visualization configuration for performance based on data size
    /// </summary>
    Task<AdvancedVisualizationConfig> OptimizeVisualizationForPerformanceAsync(
        AdvancedVisualizationConfig config,
        int dataSize);
}

/// <summary>
/// User preferences for visualization generation
/// </summary>
public class VisualizationPreferences
{
    public string? PreferredChartType { get; set; }
    public string? Theme { get; set; }
    public bool EnableAnimations { get; set; } = true;
    public bool EnableInteractivity { get; set; } = true;
    public string? ColorScheme { get; set; }
    public PerformanceLevel Performance { get; set; } = PerformanceLevel.Balanced;
    public AccessibilityLevel Accessibility { get; set; } = AccessibilityLevel.Standard;
}

/// <summary>
/// Dashboard generation preferences
/// </summary>
public class DashboardPreferences
{
    public string? Title { get; set; }
    public string? ThemeName { get; set; }
    public int? RefreshInterval { get; set; }
    public bool EnableRealTime { get; set; } = false;
    public bool EnableCollaboration { get; set; } = false;
    public bool EnableAnalytics { get; set; } = false;
    public LayoutPreference Layout { get; set; } = LayoutPreference.Auto;
    public string[] PreferredChartTypes { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Performance level preferences
/// </summary>
public enum PerformanceLevel
{
    HighQuality,
    Balanced,
    HighPerformance
}

/// <summary>
/// Accessibility level preferences
/// </summary>
public enum AccessibilityLevel
{
    Basic,
    Standard,
    Enhanced
}

/// <summary>
/// Layout preference for dashboards
/// </summary>
public enum LayoutPreference
{
    Auto,
    Grid,
    Masonry,
    Responsive,
    Custom
}

/// <summary>
/// Visualization recommendation with confidence and reasoning
/// </summary>
public class VisualizationRecommendation
{
    public AdvancedChartType ChartType { get; set; }
    public double Confidence { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public string BestFor { get; set; } = string.Empty;
    public string[] Limitations { get; set; } = Array.Empty<string>();
    public PerformanceEstimate EstimatedPerformance { get; set; } = new();
    public Dictionary<string, object> SuggestedConfig { get; set; } = new();
}

/// <summary>
/// Performance estimate for visualization
/// </summary>
public class PerformanceEstimate
{
    public TimeSpan EstimatedRenderTime { get; set; }
    public int MemoryUsageMB { get; set; }
    public bool RequiresWebGL { get; set; }
    public bool RequiresSampling { get; set; }
    public int RecommendedMaxDataPoints { get; set; }
}

/// <summary>
/// Advanced data characteristics for sophisticated analysis
/// </summary>
public class AdvancedDataCharacteristics
{
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public List<ColumnInfo> NumericColumns { get; set; } = new();
    public List<ColumnInfo> CategoricalColumns { get; set; } = new();
    public List<ColumnInfo> DateTimeColumns { get; set; } = new();
    public List<ColumnInfo> TextColumns { get; set; } = new();
    public bool HasNulls { get; set; }
    public double DataDensity { get; set; }
    public Dictionary<string, int> Cardinality { get; set; } = new();
    public Dictionary<string, string> DataDistribution { get; set; } = new();
    public List<CorrelationInfo> Correlations { get; set; } = new();
    public bool Seasonality { get; set; }
    public List<OutlierInfo> Outliers { get; set; } = new();
}

/// <summary>
/// Correlation information between columns
/// </summary>
public class CorrelationInfo
{
    public string Column1 { get; set; } = string.Empty;
    public string Column2 { get; set; } = string.Empty;
    public double Coefficient { get; set; }
    public CorrelationType Type { get; set; }
    public double Significance { get; set; }
}

/// <summary>
/// Types of correlation
/// </summary>
public enum CorrelationType
{
    Pearson,
    Spearman,
    Kendall
}

/// <summary>
/// Outlier information
/// </summary>
public class OutlierInfo
{
    public string Column { get; set; } = string.Empty;
    public object Value { get; set; } = new();
    public int RowIndex { get; set; }
    public double Score { get; set; }
    public OutlierMethod DetectionMethod { get; set; }
}

/// <summary>
/// Outlier detection methods
/// </summary>
public enum OutlierMethod
{
    IQR,
    ZScore,
    IsolationForest,
    LocalOutlierFactor
}

/// <summary>
/// Chart type suitability information
/// </summary>
public class ChartTypeSuitability
{
    public AdvancedChartType ChartType { get; set; }
    public double SuitabilityScore { get; set; }
    public string[] RequiredDataTypes { get; set; } = Array.Empty<string>();
    public int MinColumns { get; set; }
    public int MaxColumns { get; set; }
    public int MinRows { get; set; }
    public int MaxRows { get; set; }
    public string[] BestUseCases { get; set; } = Array.Empty<string>();
    public string[] Limitations { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Interactive feature configuration
/// </summary>
public class InteractiveFeature
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public bool Enabled { get; set; } = true;
    public Dictionary<string, object> Config { get; set; } = new();
    public string[] Dependencies { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Chart performance metrics
/// </summary>
public class ChartPerformanceMetrics
{
    public string RenderTime { get; set; } = string.Empty;
    public int MemoryUsage { get; set; }
    public int DataPointsRendered { get; set; }
    public bool UsedSampling { get; set; }
    public bool UsedVirtualization { get; set; }
    public bool UsedWebGL { get; set; }
    public double FrameRate { get; set; }
}

/// <summary>
/// Visualization export result
/// </summary>
public class VisualizationExportResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public byte[]? Data { get; set; }
    public string? MimeType { get; set; }
    public long FileSizeBytes { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
