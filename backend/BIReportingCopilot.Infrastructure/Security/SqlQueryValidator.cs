using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Core.Constants;

namespace BIReportingCopilot.Infrastructure.Security;

public interface ISqlQueryValidator
{
    Task<SqlValidationResult> ValidateAsync(string sql);
    bool IsSelectOnlyQuery(string sql);
    bool ContainsDangerousKeywords(string sql);
    bool HasValidSyntax(string sql);
}

public class SqlValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public SecurityLevel SecurityLevel { get; set; } = SecurityLevel.Safe;
}

public enum SecurityLevel
{
    Safe,
    Warning,
    Dangerous,
    Blocked
}

public class SqlQueryValidator : ISqlQueryValidator
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SqlQueryValidator> _logger;
    private readonly QuerySettings _querySettings;

    // Dangerous keywords that should be blocked (enhanced list)
    private readonly string[] _dangerousKeywords;

    // Suspicious patterns
    private readonly string[] _suspiciousPatterns = {
        @"--.*",           // SQL comments
        @"/\*.*?\*/",      // Block comments
        @";\s*\w+",        // Multiple statements
        @"UNION\s+SELECT", // Union-based injection
        @"OR\s+1\s*=\s*1", // Always true conditions
        @"AND\s+1\s*=\s*1",
        @"'\s*OR\s*'",     // Quote-based injection
        @"'\s*AND\s*'",
        @"WAITFOR\s+DELAY", // Time-based attacks
        @"BENCHMARK\s*\(",
        @"SLEEP\s*\("
    };

    public SqlQueryValidator(IConfiguration configuration, ILogger<SqlQueryValidator> logger, IOptions<QuerySettings> querySettings)
    {
        _configuration = configuration;
        _logger = logger;
        _querySettings = querySettings.Value;

        // Initialize dangerous keywords from configuration or use defaults
        _dangerousKeywords = _querySettings.BlockedKeywords.Any()
            ? _querySettings.BlockedKeywords.ToArray()
            : new[] {
                "DROP", "DELETE", "INSERT", "UPDATE", "ALTER", "CREATE", "TRUNCATE",
                "EXEC", "EXECUTE", "SP_", "XP_", "OPENROWSET", "OPENDATASOURCE",
                "BULK", "BACKUP", "RESTORE", "SHUTDOWN", "RECONFIGURE", "GRANT", "REVOKE",
                "DENY", "KILL", "WAITFOR", "DBCC", "USE", "GO", "DECLARE", "SET"
            };
    }

    public async Task<SqlValidationResult> ValidateAsync(string sql)
    {
        var result = new SqlValidationResult();

        if (string.IsNullOrWhiteSpace(sql))
        {
            result.Errors.Add("SQL query cannot be empty");
            return result;
        }

        // Check if it's a SELECT-only query
        if (!IsSelectOnlyQuery(sql))
        {
            result.Errors.Add("Only SELECT queries are allowed");
            result.SecurityLevel = SecurityLevel.Blocked;
            return result;
        }

        // Check for dangerous keywords
        if (ContainsDangerousKeywords(sql))
        {
            result.Errors.Add("Query contains dangerous keywords");
            result.SecurityLevel = SecurityLevel.Blocked;
            return result;
        }

        // Check for suspicious patterns
        var suspiciousPatterns = CheckSuspiciousPatterns(sql);
        if (suspiciousPatterns.Any())
        {
            result.Warnings.AddRange(suspiciousPatterns.Select(p => $"Suspicious pattern detected: {p}"));
            result.SecurityLevel = SecurityLevel.Warning;
        }

        // Check syntax (basic validation)
        if (!HasValidSyntax(sql))
        {
            result.Errors.Add("Invalid SQL syntax detected");
            result.SecurityLevel = SecurityLevel.Dangerous;
            return result;
        }

        // Check query complexity
        var complexityWarnings = CheckQueryComplexity(sql);
        result.Warnings.AddRange(complexityWarnings);

        result.IsValid = !result.Errors.Any();

        if (result.IsValid && result.SecurityLevel == SecurityLevel.Safe)
        {
            _logger.LogDebug("SQL query validation passed: {QueryLength} characters", sql.Length);
        }
        else
        {
            _logger.LogWarning("SQL query validation issues: {Errors}, {Warnings}",
                string.Join(", ", result.Errors),
                string.Join(", ", result.Warnings));
        }

        await Task.CompletedTask;
        return result;
    }

    public bool IsSelectOnlyQuery(string sql)
    {
        var trimmed = sql.Trim();

        // Must start with SELECT (case insensitive)
        if (!trimmed.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            return false;

        // Should not contain other statement types
        var upperSql = sql.ToUpperInvariant();
        var statementKeywords = new[] { "INSERT", "UPDATE", "DELETE", "DROP", "CREATE", "ALTER", "TRUNCATE" };

        return !statementKeywords.Any(keyword =>
            Regex.IsMatch(upperSql, $@"\b{keyword}\b", RegexOptions.IgnoreCase));
    }

    public bool ContainsDangerousKeywords(string sql)
    {
        var upperSql = sql.ToUpperInvariant();
        return _dangerousKeywords.Any(keyword =>
            Regex.IsMatch(upperSql, $@"\b{keyword}\b", RegexOptions.IgnoreCase));
    }

    public bool HasValidSyntax(string sql)
    {
        try
        {
            // Basic syntax checks
            var openParens = sql.Count(c => c == '(');
            var closeParens = sql.Count(c => c == ')');

            if (openParens != closeParens)
                return false;

            // Check for unmatched quotes
            var singleQuotes = sql.Count(c => c == '\'');
            if (singleQuotes % 2 != 0)
                return false;

            // More sophisticated syntax validation could be added here
            return true;
        }
        catch
        {
            return false;
        }
    }

    private List<string> CheckSuspiciousPatterns(string sql)
    {
        var detectedPatterns = new List<string>();

        foreach (var pattern in _suspiciousPatterns)
        {
            if (Regex.IsMatch(sql, pattern, RegexOptions.IgnoreCase))
            {
                detectedPatterns.Add(pattern);
            }
        }

        return detectedPatterns;
    }

    private List<string> CheckQueryComplexity(string sql)
    {
        var warnings = new List<string>();

        // Check for too many JOINs
        var joinCount = Regex.Matches(sql, @"\bJOIN\b", RegexOptions.IgnoreCase).Count;
        if (joinCount > 5)
        {
            warnings.Add($"Query has {joinCount} JOINs, which may impact performance");
        }

        // Check for nested subqueries
        var subqueryCount = Regex.Matches(sql, @"\bSELECT\b", RegexOptions.IgnoreCase).Count - 1;
        if (subqueryCount > 3)
        {
            warnings.Add($"Query has {subqueryCount} subqueries, which may impact performance");
        }

        // Check query length
        if (sql.Length > 5000)
        {
            warnings.Add("Query is very long and may be complex");
        }

        return warnings;
    }
}
