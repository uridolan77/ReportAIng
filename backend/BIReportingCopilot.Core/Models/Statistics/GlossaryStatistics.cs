namespace BIReportingCopilot.Core.Models.Statistics;

/// <summary>
/// Statistics for glossary management
/// </summary>
public class GlossaryStatistics
{
    /// <summary>
    /// Total number of glossary terms
    /// </summary>
    public int TotalTerms { get; set; }

    /// <summary>
    /// Number of active terms
    /// </summary>
    public int ActiveTerms { get; set; }

    /// <summary>
    /// Number of terms created this month
    /// </summary>
    public int TermsCreatedThisMonth { get; set; }

    /// <summary>
    /// Number of terms updated this month
    /// </summary>
    public int TermsUpdatedThisMonth { get; set; }

    /// <summary>
    /// Most frequently accessed terms
    /// </summary>
    public List<string> MostAccessedTerms { get; set; } = new();

    /// <summary>
    /// Terms by category distribution
    /// </summary>
    public Dictionary<string, int> TermsByCategory { get; set; } = new();

    /// <summary>
    /// Average term usage frequency
    /// </summary>
    public double AverageUsageFrequency { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
