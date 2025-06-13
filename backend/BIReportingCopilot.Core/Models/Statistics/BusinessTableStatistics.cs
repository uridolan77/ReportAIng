namespace BIReportingCopilot.Core.Models.Statistics;

/// <summary>
/// Statistics for business table management
/// </summary>
public class BusinessTableStatistics
{
    /// <summary>
    /// Total number of business tables
    /// </summary>
    public int TotalTables { get; set; }

    /// <summary>
    /// Number of active tables
    /// </summary>
    public int ActiveTables { get; set; }

    /// <summary>
    /// Number of tables created this month
    /// </summary>
    public int TablesCreatedThisMonth { get; set; }

    /// <summary>
    /// Number of tables updated this month
    /// </summary>
    public int TablesUpdatedThisMonth { get; set; }

    /// <summary>
    /// Most frequently queried tables
    /// </summary>
    public List<string> MostQueriedTables { get; set; } = new();

    /// <summary>
    /// Tables by category distribution
    /// </summary>
    public Dictionary<string, int> TablesByCategory { get; set; } = new();

    /// <summary>
    /// Average table size in rows
    /// </summary>
    public long AverageTableSize { get; set; }

    /// <summary>
    /// Total data volume in bytes
    /// </summary>
    public long TotalDataVolume { get; set; }

    /// <summary>
    /// Last update timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
