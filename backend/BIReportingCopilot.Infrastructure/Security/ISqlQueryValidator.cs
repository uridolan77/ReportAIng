using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.Security;

/// <summary>
/// Interface for SQL query validation
/// </summary>
public interface ISqlQueryValidator
{
    /// <summary>
    /// Validates a SQL query for security and syntax issues
    /// </summary>
    /// <param name="query">The SQL query to validate</param>
    /// <returns>Validation result</returns>
    SqlValidationResult ValidateQuery(string query);

    /// <summary>
    /// Validates a SQL query asynchronously
    /// </summary>
    /// <param name="query">The SQL query to validate</param>
    /// <returns>Validation result</returns>
    Task<SqlValidationResult> ValidateQueryAsync(string query);

    /// <summary>
    /// Gets the security level for a query
    /// </summary>
    /// <param name="query">The SQL query</param>
    /// <returns>Security level</returns>
    SecurityLevel GetSecurityLevel(string query);
}

/// <summary>
/// SQL validation result
/// </summary>
public class SqlValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public SecurityLevel SecurityLevel { get; set; }
    public string? SanitizedQuery { get; set; }
    public double RiskScore { get; set; }
    public DateTime ValidationTime { get; set; }
    public DateTime ValidationTimestamp { get; set; }
    public List<ValidationIssue> Issues { get; set; } = new();
}

/// <summary>
/// Validation issue
/// </summary>
public class ValidationIssue
{
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public SecurityLevel Severity { get; set; }
}

/// <summary>
/// Security level enumeration
/// </summary>
public enum SecurityLevel
{
    Safe,
    Warning,
    Dangerous,
    Blocked
}
