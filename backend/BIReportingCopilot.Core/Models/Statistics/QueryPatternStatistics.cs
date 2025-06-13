namespace BIReportingCopilot.Core.Models.Statistics;

/// <summary>
/// Statistics for query pattern management
/// </summary>
public class QueryPatternStatistics
{
    /// <summary>
    /// Total number of query patterns
    /// </summary>
    public int TotalPatterns { get; set; }

    /// <summary>
    /// Number of active patterns
    /// </summary>
    public int ActivePatterns { get; set; }

    /// <summary>
    /// Number of patterns created this month
    /// </summary>
    public int PatternsCreatedThisMonth { get; set; }

    /// <summary>
    /// Number of patterns used this month
    /// </summary>
    public int PatternsUsedThisMonth { get; set; }

    /// <summary>
    /// Most frequently used patterns
    /// </summary>
    public List<string> MostUsedPatterns { get; set; } = new();

    /// <summary>
    /// Patterns by complexity distribution
    /// </summary>
    public Dictionary<string, int> PatternsByComplexity { get; set; } = new();

    /// <summary>
    /// Average pattern match accuracy
    /// </summary>
    public double AverageMatchAccuracy { get; set; }

    /// <summary>
    /// Pattern performance metrics
    /// </summary>
    public Dictionary<string, double> PerformanceMetrics { get; set; } = new();

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
