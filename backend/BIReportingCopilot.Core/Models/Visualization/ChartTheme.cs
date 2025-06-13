namespace BIReportingCopilot.Core.Models.Visualization;

/// <summary>
/// Chart theme enumeration for visualization styling
/// </summary>
public enum ChartTheme
{
    Default,
    Light,
    Dark,
    Blue,
    Green,
    Purple,
    Orange,
    Red,
    Corporate,
    Modern,
    Classic,
    Minimal
}

/// <summary>
/// Color scheme enumeration for chart styling
/// </summary>
public enum ColorScheme
{
    Default,
    Monochrome,
    Colorful,
    Pastel,
    Vibrant,
    Cool,
    Warm,
    Earth,
    Ocean,
    Sunset,
    Forest,
    Corporate,
    Business
}

/// <summary>
/// Performance estimate for visualization rendering
/// </summary>
public class PerformanceEstimate
{
    /// <summary>
    /// Estimated rendering time in milliseconds
    /// </summary>
    public double EstimatedRenderTimeMs { get; set; }

    /// <summary>
    /// Estimated memory usage in bytes
    /// </summary>
    public long EstimatedMemoryUsageBytes { get; set; }

    /// <summary>
    /// Performance rating (1-5, where 5 is best)
    /// </summary>
    public int PerformanceRating { get; set; }

    /// <summary>
    /// Whether the visualization is optimized for large datasets
    /// </summary>
    public bool IsOptimizedForLargeData { get; set; }

    /// <summary>
    /// Recommended maximum number of data points
    /// </summary>
    public int RecommendedMaxDataPoints { get; set; }

    /// <summary>
    /// Estimated render time (alias for EstimatedRenderTimeMs)
    /// </summary>
    public TimeSpan EstimatedRenderTime
    {
        get => TimeSpan.FromMilliseconds(EstimatedRenderTimeMs);
        set => EstimatedRenderTimeMs = value.TotalMilliseconds;
    }

    /// <summary>
    /// Performance warnings or recommendations
    /// </summary>
    public List<string> Warnings { get; set; } = new();
}
