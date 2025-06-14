namespace BIReportingCopilot.Core.Models;

public class AuditLogEntry
{
    public long Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public object? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Severity { get; set; } = "Info";
}


public class SecurityReport
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int TotalSecurityEvents { get; set; }
    public int FailedLoginAttempts { get; set; }
    public int SuccessfulLogins { get; set; }
    public int SuspiciousActivities { get; set; }
    public Dictionary<string, int> TopFailedLoginIPs { get; set; } = new();
    public List<string> TopRisks { get; set; } = new();
    public Dictionary<string, int> EventsByType { get; set; } = new();
    public List<string> RecommendedActions { get; set; } = new();
    public double SecurityScore { get; set; }
}

public class UsageReport
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int TotalQueries { get; set; }
    public int UniqueUsers { get; set; }
    public double AverageResponseTime { get; set; }
    public double SuccessRate { get; set; }
    public Dictionary<string, int> QueriesByUser { get; set; } = new();
    public Dictionary<string, int> QueriesByHour { get; set; } = new();
    public List<string> MostPopularQueries { get; set; } = new();
    public List<string> SlowestQueries { get; set; } = new();
}

public class PromptTemplate
{
    // Properties expected by Infrastructure services
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Category { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public List<string> Variables { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();

    // Original properties
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public string Content { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public double? SuccessRate { get; set; }
    public int UsageCount { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class PromptPerformanceMetrics
{
    public string TemplateName { get; set; } = string.Empty;
    public double SuccessRate { get; set; }
    public double AverageConfidence { get; set; }
    public int TotalUsage { get; set; }
    public int UsageCount { get; set; }
    public TimeSpan AverageResponseTime { get; set; }
    public DateTime LastUsed { get; set; }
    public List<string> CommonErrors { get; set; } = new();
}

public class QueryExecutionPlan
{
    public string PlanXml { get; set; } = string.Empty;
    public double EstimatedCost { get; set; }
    public List<string> Recommendations { get; set; } = new();
    public bool HasWarnings { get; set; }
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Query property (for compatibility)
    /// </summary>
    public string Query { get; set; } = string.Empty;
}

// QueryPerformanceMetrics moved to OptimizedDTOs.cs to avoid duplication
