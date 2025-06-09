using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models.ML;
using BIReportingCopilot.Infrastructure.AI;
using BIReportingCopilot.Infrastructure.Monitoring;

namespace BIReportingCopilot.Infrastructure.Security;

/// <summary>
/// Enhanced SQL query validator with ML-based anomaly detection and comprehensive security checks
/// Consolidates functionality from multiple SQL validators for unified security validation
/// </summary>
public class EnhancedSqlQueryValidator : IEnhancedSqlQueryValidator, ISqlQueryValidator
{
    private readonly ILogger<EnhancedSqlQueryValidator> _logger;
    private readonly LearningService _learningService;
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
        LearningService learningService,
        IMetricsCollector metricsCollector,
        IOptions<SqlValidationConfiguration> config)
    {
        _logger = logger;
        _learningService = learningService;
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

                // Reduce log noise for intentional health check tests
                if (context?.Contains("health-check-intentional-test") == true)
                {
                    _logger.LogDebug("Dangerous SQL pattern detected for user {UserId}: {Pattern} (health check test)", userId, pattern.Name);
                }
                else
                {
                    _logger.LogWarning("Dangerous SQL pattern detected for user {UserId}: {Pattern}", userId, pattern.Name);
                }
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
                // Create basic query metrics for analysis
                var metrics = new QueryMetrics
                {
                    ExecutionTimeMs = 100, // Default value
                    RowCount = 0,
                    IsSuccessful = true
                };

                var anomalyResult = await _learningService.DetectQueryAnomaliesAsync(query, metrics);
                if (anomalyResult.AnomalyScore > 0.1) // Consider it anomalous if score > 0.1
                {
                    var anomalyReason = anomalyResult.DetectedAnomalies.Any()
                        ? string.Join(", ", anomalyResult.DetectedAnomalies.Select(a => a.ToString()))
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

    #region ISqlQueryValidator Implementation (Consolidated from EnhancedSqlValidator)

    /// <summary>
    /// Validates SQL query using the enhanced validation logic (ISqlQueryValidator interface)
    /// </summary>
    public async Task<SqlValidationResult> ValidateAsync(string sql)
    {
        var enhancedResult = await ValidateQueryAsync(sql);

        // Convert enhanced result to basic SqlValidationResult for backward compatibility
        return new SqlValidationResult
        {
            IsValid = enhancedResult.IsValid,
            SecurityLevel = enhancedResult.SecurityLevel,
            RiskScore = enhancedResult.RiskScore / 10.0, // Normalize to 0-1 scale
            Errors = enhancedResult.Issues.Where(i => enhancedResult.SecurityLevel == SecurityLevel.Blocked)
                                         .Select(i => i.Description).ToList(),
            Warnings = enhancedResult.Issues.Where(i => enhancedResult.SecurityLevel == SecurityLevel.Warning)
                                           .Select(i => i.Description).ToList(),
            ValidationTime = enhancedResult.ValidationTimestamp
        };
    }

    /// <summary>
    /// Checks if query contains only SELECT statements
    /// </summary>
    public bool IsSelectOnlyQuery(string sql)
    {
        var trimmed = sql.Trim();
        if (!trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            return false;

        var upperSql = sql.ToUpperInvariant();
        var statementKeywords = new[] { "INSERT", "UPDATE", "DELETE", "DROP", "CREATE", "ALTER", "TRUNCATE" };

        return !statementKeywords.Any(keyword =>
            Regex.IsMatch(upperSql, $@"\b{keyword}\b", RegexOptions.IgnoreCase));
    }

    /// <summary>
    /// Checks if query contains dangerous keywords
    /// </summary>
    public bool ContainsDangerousKeywords(string sql)
    {
        var upperSql = sql.ToUpperInvariant();
        var dangerousKeywords = new[] {
            "DROP", "DELETE", "INSERT", "UPDATE", "ALTER", "CREATE", "TRUNCATE",
            "EXEC", "EXECUTE", "SP_", "XP_", "OPENROWSET", "OPENDATASOURCE",
            "BULK", "BACKUP", "RESTORE", "SHUTDOWN", "RECONFIGURE"
        };

        return dangerousKeywords.Any(keyword =>
            Regex.IsMatch(upperSql, $@"\b{keyword}\b", RegexOptions.IgnoreCase));
    }

    /// <summary>
    /// Validates SQL syntax (parentheses, quotes balance)
    /// </summary>
    public bool HasValidSyntax(string sql)
    {
        try
        {
            return HasBalancedParentheses(sql) && HasBalancedQuotes(sql);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Validates parameterized query parameters
    /// </summary>
    public bool ValidateParameterizedQuery(string sql, Dictionary<string, object> parameters)
    {
        if (parameters == null || !parameters.Any())
            return true;

        foreach (var param in parameters)
        {
            if (!IsValidParameterName(param.Key))
                return false;
            if (param.Value != null && ContainsSuspiciousContent(param.Value.ToString() ?? ""))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Validates query structure and returns detailed result
    /// </summary>
    public SqlValidationResult ValidateQueryStructure(string sql)
    {
        var result = new SqlValidationResult
        {
            IsValid = true,
            SecurityLevel = SecurityLevel.Safe,
            Errors = new List<string>(),
            Warnings = new List<string>(),
            ValidationTime = DateTime.UtcNow
        };

        try
        {
            var trimmedSql = sql.Trim();

            if (!trimmedSql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                result.Errors.Add("Query must start with SELECT");
                result.SecurityLevel = SecurityLevel.Blocked;
                result.IsValid = false;
                return result;
            }

            if (!HasBalancedParentheses(sql))
            {
                result.Errors.Add("Query has unbalanced parentheses");
                result.SecurityLevel = SecurityLevel.Dangerous;
                result.IsValid = false;
            }

            if (!HasBalancedQuotes(sql))
            {
                result.Errors.Add("Query has unbalanced quotes");
                result.SecurityLevel = SecurityLevel.Dangerous;
                result.IsValid = false;
            }

            // Check for dangerous keywords
            if (ContainsDangerousKeywords(sql))
            {
                result.Errors.Add("Query contains dangerous keywords");
                result.SecurityLevel = SecurityLevel.Blocked;
                result.IsValid = false;
            }

            // Check for multiple statements
            var statements = sql.Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (statements.Length > 1)
            {
                result.Warnings.Add("Multiple SQL statements detected");
                if (result.SecurityLevel == SecurityLevel.Safe)
                    result.SecurityLevel = SecurityLevel.Warning;
            }

            result.IsValid = !result.Errors.Any();
        }
        catch (Exception ex)
        {
            result.Errors.Add("Failed to validate query structure");
            result.SecurityLevel = SecurityLevel.Dangerous;
            result.IsValid = false;
            _logger.LogError(ex, "Error validating query structure");
        }

        return result;
    }

    #endregion

    #region ISqlQueryValidator Implementation

    /// <summary>
    /// Validates a SQL query for security and syntax issues (ISqlQueryValidator interface)
    /// </summary>
    public SqlValidationResult ValidateQuery(string query)
    {
        return ValidateQueryStructure(query);
    }

    /// <summary>
    /// Validates a SQL query asynchronously (ISqlQueryValidator interface)
    /// </summary>
    public async Task<SqlValidationResult> ValidateQueryAsync(string query)
    {
        return await Task.FromResult(ValidateQuery(query));
    }

    /// <summary>
    /// Gets the security level for a query (ISqlQueryValidator interface)
    /// </summary>
    public SecurityLevel GetSecurityLevel(string query)
    {
        var result = ValidateQuery(query);
        return result.SecurityLevel;
    }

    #endregion

    #region Private Helper Methods (Consolidated)

    private bool IsValidParameterName(string paramName)
    {
        return Regex.IsMatch(paramName, @"^@?[a-zA-Z_][a-zA-Z0-9_]*$");
    }

    private bool ContainsSuspiciousContent(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        var suspiciousPatterns = new[]
        {
            @"'.*--", @"'.*\bOR\b.*'", @"'.*\bAND\b.*'", @"'.*\bUNION\b.*'",
            @"<script", @"javascript:", @"vbscript:"
        };

        return suspiciousPatterns.Any(pattern =>
            Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase));
    }

    private bool HasBalancedParentheses(string sql)
    {
        var count = 0;
        foreach (char c in sql)
        {
            if (c == '(') count++;
            else if (c == ')') count--;
            if (count < 0) return false;
        }
        return count == 0;
    }

    private bool HasBalancedQuotes(string sql)
    {
        var singleQuoteCount = 0;
        for (int i = 0; i < sql.Length; i++)
        {
            if (sql[i] == '\'' && (i == 0 || sql[i - 1] != '\\'))
                singleQuoteCount++;
        }
        return singleQuoteCount % 2 == 0;
    }

    #endregion

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
