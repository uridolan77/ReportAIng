using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Infrastructure.AI;
using BIReportingCopilot.Infrastructure.Monitoring;

namespace BIReportingCopilot.Infrastructure.Security;

/// <summary>
/// Enhanced SQL query validator with ML-based anomaly detection and comprehensive security checks
/// </summary>
public class EnhancedSqlQueryValidator : IEnhancedSqlQueryValidator
{
    private readonly ILogger<EnhancedSqlQueryValidator> _logger;
    private readonly MLAnomalyDetector _anomalyDetector;
    private readonly IMetricsCollector _metricsCollector;
    private readonly SqlValidationConfiguration _config;

    // Enhanced dangerous SQL patterns with more comprehensive coverage
    private static readonly SqlPattern[] DangerousPatterns = {
        new("Data Manipulation", @"\b(DROP|DELETE|TRUNCATE|ALTER|CREATE|INSERT|UPDATE)\s+", 10),
        new("System Procedures", @"\b(EXEC|EXECUTE|SP_|XP_)\s*\(", 10),
        new("Union Injection", @"\b(UNION\s+(ALL\s+)?SELECT)\b", 9),
        new("SQL Comments", @"(--|\#|/\*.*?\*/)", 8),
        new("System Tables", @"\b(INFORMATION_SCHEMA|SYS\.|SYSOBJECTS|SYSCOLUMNS)\b", 9),
        new("File Operations", @"\b(OPENROWSET|OPENDATASOURCE|BULK\s+INSERT)\b", 10),
        new("Dynamic SQL", @"\b(EXEC\s*\(|EXECUTE\s*\()", 8),
        new("Privilege Escalation", @"\b(GRANT|REVOKE|ALTER\s+LOGIN|CREATE\s+USER)\b", 10),
        new("Database Functions", @"\b(DB_NAME|USER_NAME|SYSTEM_USER|SESSION_USER)\s*\(", 7),
        new("Conditional Bypass", @"\b(IF\s*\(|CASE\s+WHEN)", 6)
    };

    // Enhanced suspicious patterns with risk scoring
    private static readonly SqlPattern[] SuspiciousPatterns = {
        new("OR Injection", @"'\s*OR\s*'", 8),
        new("AND Injection", @"'\s*AND\s*'", 8),
        new("Always True", @"(1\s*=\s*1|'1'\s*=\s*'1')", 7),
        new("Always False", @"(1\s*=\s*0|'1'\s*=\s*'0')", 6),
        new("Statement Termination", @"';\s*", 7),
        new("Time Delays", @"\b(WAITFOR|DELAY|SLEEP)\s*\(", 8),
        new("Hex Encoding", @"0x[0-9a-fA-F]+", 5),
        new("Concatenation", @"\|\||CONCAT\s*\(", 4),
        new("Subquery Injection", @"\(\s*SELECT\s+", 6),
        new("Multiple Statements", @";\s*\w+", 5),
        new("Blind Injection", @"\b(AND|OR)\s+\d+\s*[<>=]", 6),
        new("Error-based Injection", @"\b(CONVERT|CAST)\s*\(.*,.*\)", 5)
    };

    // Whitelist patterns for common legitimate SQL constructs
    private static readonly string[] WhitelistPatterns = {
        @"^\s*SELECT\s+[\w\s,\.\*\(\)]+\s+FROM\s+[\w\.]+(\s+WHERE\s+[\w\s=<>!']+)?(\s+ORDER\s+BY\s+[\w\s,]+)?(\s+LIMIT\s+\d+)?\s*$",
        @"^\s*SELECT\s+COUNT\s*\(\s*\*?\s*\)\s+FROM\s+[\w\.]+(\s+WHERE\s+[\w\s=<>!']+)?\s*$"
    };

    public EnhancedSqlQueryValidator(
        ILogger<EnhancedSqlQueryValidator> logger,
        MLAnomalyDetector anomalyDetector,
        IMetricsCollector metricsCollector,
        IOptions<SqlValidationConfiguration> config)
    {
        _logger = logger;
        _anomalyDetector = anomalyDetector;
        _metricsCollector = metricsCollector;
        _config = config.Value;
    }

    public async Task<EnhancedSqlValidationResult> ValidateQueryAsync(string query, string? context = null, string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new EnhancedSqlValidationResult
            {
                IsValid = false,
                SecurityLevel = SecurityLevel.Blocked,
                RiskScore = 10,
                Issues = new[] { new SecurityIssue("Empty Query", "Query cannot be empty", 10) },
                Query = query ?? "",
                Context = context,
                UserId = userId
            };
        }

        var issues = new List<SecurityIssue>();
        var riskScore = 0;
        var securityLevel = SecurityLevel.Safe;

        // Normalize query for analysis
        var normalizedQuery = NormalizeQuery(query);

        // Check whitelist first (if enabled)
        if (_config.EnableWhitelist && IsWhitelisted(normalizedQuery))
        {
            _logger.LogDebug("Query matched whitelist pattern for user {UserId}", userId);
            return new EnhancedSqlValidationResult
            {
                IsValid = true,
                SecurityLevel = SecurityLevel.Safe,
                RiskScore = 0,
                Issues = Array.Empty<SecurityIssue>(),
                Query = query,
                Context = context,
                UserId = userId
            };
        }

        // Check for dangerous patterns
        foreach (var pattern in DangerousPatterns)
        {
            if (Regex.IsMatch(normalizedQuery, pattern.Pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
            {
                var issue = new SecurityIssue(pattern.Name, $"Dangerous SQL pattern detected: {pattern.Name}", pattern.RiskScore);
                issues.Add(issue);
                riskScore += pattern.RiskScore;
                securityLevel = SecurityLevel.Blocked;

                _logger.LogWarning("Dangerous SQL pattern detected for user {UserId}: {Pattern}", userId, pattern.Name);
                _metricsCollector.IncrementCounter("sql_validation_dangerous_patterns", new TagList
                {
                    { "pattern", pattern.Name },
                    { "user_id", userId ?? "unknown" }
                });
            }
        }

        // Check for suspicious patterns
        foreach (var pattern in SuspiciousPatterns)
        {
            if (Regex.IsMatch(normalizedQuery, pattern.Pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline))
            {
                var issue = new SecurityIssue(pattern.Name, $"Suspicious SQL pattern detected: {pattern.Name}", pattern.RiskScore);
                issues.Add(issue);
                riskScore += pattern.RiskScore;

                if (securityLevel == SecurityLevel.Safe)
                {
                    securityLevel = riskScore >= _config.WarningThreshold ? SecurityLevel.Warning : SecurityLevel.Safe;
                }

                _logger.LogWarning("Suspicious SQL pattern detected for user {UserId}: {Pattern}", userId, pattern.Name);
                _metricsCollector.IncrementCounter("sql_validation_suspicious_patterns", new TagList
                {
                    { "pattern", pattern.Name },
                    { "user_id", userId ?? "unknown" }
                });
            }
        }

        // ML-based anomaly detection
        if (_config.EnableAnomalyDetection && !string.IsNullOrEmpty(userId))
        {
            try
            {
                var anomalyResult = await _anomalyDetector.AnalyzeQueryAsync(userId, query, query);
                if (anomalyResult.AnomalyScore > 0.1) // Consider it anomalous if score > 0.1
                {
                    var anomalyReason = anomalyResult.DetectedAnomalies.Any()
                        ? string.Join(", ", anomalyResult.DetectedAnomalies)
                        : "Unusual query pattern detected";

                    var anomalyIssue = new SecurityIssue(
                        "ML Anomaly",
                        $"Query anomaly detected: {anomalyReason}",
                        (int)(anomalyResult.AnomalyScore * 10));

                    issues.Add(anomalyIssue);
                    riskScore += anomalyIssue.RiskScore;

                    if (anomalyResult.AnomalyScore >= _config.AnomalyBlockThreshold)
                    {
                        securityLevel = SecurityLevel.Blocked;
                    }
                    else if (anomalyResult.AnomalyScore >= _config.AnomalyWarningThreshold && securityLevel == SecurityLevel.Safe)
                    {
                        securityLevel = SecurityLevel.Warning;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "ML anomaly detection failed for user {UserId}", userId);
            }
        }

        // Final risk assessment
        if (riskScore >= _config.BlockThreshold)
        {
            securityLevel = SecurityLevel.Blocked;
        }
        else if (riskScore >= _config.WarningThreshold && securityLevel == SecurityLevel.Safe)
        {
            securityLevel = SecurityLevel.Warning;
        }

        var result = new EnhancedSqlValidationResult
        {
            IsValid = securityLevel != SecurityLevel.Blocked,
            SecurityLevel = securityLevel,
            RiskScore = riskScore,
            Issues = issues.ToArray(),
            Query = query,
            Context = context,
            UserId = userId,
            ValidationTimestamp = DateTime.UtcNow
        };

        // Record metrics
        _metricsCollector.RecordValue("sql_validation_risk_score", riskScore);
        _metricsCollector.RecordHistogram("sql_validation_risk_score_detailed", riskScore, new TagList
        {
            { "security_level", securityLevel.ToString() },
            { "user_id", userId ?? "unknown" }
        });

        return result;
    }

    public async Task<bool> IsQuerySafeAsync(string query, string? userId = null)
    {
        var result = await ValidateQueryAsync(query, null, userId);
        return result.IsValid && result.SecurityLevel != SecurityLevel.Blocked;
    }

    public async Task<SqlSecurityReport> GenerateSecurityReportAsync(string query, string? userId = null)
    {
        var validationResult = await ValidateQueryAsync(query, null, userId);

        return new SqlSecurityReport
        {
            Query = query,
            UserId = userId,
            ValidationResult = validationResult,
            Recommendations = GenerateRecommendations(validationResult),
            GeneratedAt = DateTime.UtcNow
        };
    }

    private string NormalizeQuery(string query)
    {
        // Remove extra whitespace and normalize case
        return Regex.Replace(query.Trim(), @"\s+", " ");
    }

    private bool IsWhitelisted(string query)
    {
        return WhitelistPatterns.Any(pattern =>
            Regex.IsMatch(query, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline));
    }

    private List<string> GenerateRecommendations(EnhancedSqlValidationResult result)
    {
        var recommendations = new List<string>();

        if (result.RiskScore > 0)
        {
            recommendations.Add("Review the query for potential security issues");

            if (result.Issues.Any(i => i.Type.Contains("Injection")))
            {
                recommendations.Add("Use parameterized queries to prevent SQL injection");
            }

            if (result.Issues.Any(i => i.Type.Contains("System")))
            {
                recommendations.Add("Avoid accessing system tables and procedures");
            }

            if (result.Issues.Any(i => i.Type.Contains("Dynamic")))
            {
                recommendations.Add("Minimize use of dynamic SQL construction");
            }
        }

        return recommendations;
    }

    private class SqlPattern
    {
        public string Name { get; }
        public string Pattern { get; }
        public int RiskScore { get; }

        public SqlPattern(string name, string pattern, int riskScore)
        {
            Name = name;
            Pattern = pattern;
            RiskScore = riskScore;
        }
    }
}

/// <summary>
/// Interface for enhanced SQL query validation
/// </summary>
public interface IEnhancedSqlQueryValidator
{
    Task<EnhancedSqlValidationResult> ValidateQueryAsync(string query, string? context = null, string? userId = null);
    Task<bool> IsQuerySafeAsync(string query, string? userId = null);
    Task<SqlSecurityReport> GenerateSecurityReportAsync(string query, string? userId = null);
}

/// <summary>
/// Enhanced SQL validation result with detailed security analysis
/// </summary>
public class EnhancedSqlValidationResult
{
    public bool IsValid { get; set; }
    public SecurityLevel SecurityLevel { get; set; }
    public int RiskScore { get; set; }
    public SecurityIssue[] Issues { get; set; } = Array.Empty<SecurityIssue>();
    public string Query { get; set; } = string.Empty;
    public string? Context { get; set; }
    public string? UserId { get; set; }
    public DateTime ValidationTimestamp { get; set; }
}

/// <summary>
/// Security issue details
/// </summary>
public class SecurityIssue
{
    public string Type { get; set; }
    public string Description { get; set; }
    public int RiskScore { get; set; }

    public SecurityIssue(string type, string description, int riskScore)
    {
        Type = type;
        Description = description;
        RiskScore = riskScore;
    }
}

/// <summary>
/// SQL security report
/// </summary>
public class SqlSecurityReport
{
    public string Query { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public EnhancedSqlValidationResult ValidationResult { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Configuration for SQL validation
/// </summary>
public class SqlValidationConfiguration
{
    public bool EnableWhitelist { get; set; } = false;
    public bool EnableAnomalyDetection { get; set; } = true;
    public int WarningThreshold { get; set; } = 5;
    public int BlockThreshold { get; set; } = 8;
    public double AnomalyWarningThreshold { get; set; } = 0.6;
    public double AnomalyBlockThreshold { get; set; } = 0.8;
}
