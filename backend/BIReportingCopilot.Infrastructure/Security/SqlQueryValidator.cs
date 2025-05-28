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
    bool ValidateParameterizedQuery(string sql, Dictionary<string, object> parameters);
    SqlValidationResult ValidateQueryStructure(string sql);
}

public class SqlValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public SecurityLevel SecurityLevel { get; set; } = SecurityLevel.Safe;
    public double RiskScore { get; set; }
    public DateTime ValidationTime { get; set; } = DateTime.UtcNow;
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

    // Enhanced injection patterns with more sophisticated detection
    private readonly string[] _injectionPatterns = {
        @"(\b(UNION|INTERSECT|EXCEPT)\s+(ALL\s+)?SELECT\b)",
        @"(xp_cmdshell|sp_executesql|sp_makewebtask)",
        @"(INFORMATION_SCHEMA|sys\.objects|sys\.tables)",
        @"(0x[0-9a-fA-F]+)", // Hex encoding attempts
        @"(CHAR\s*\([0-9]+\))", // Character encoding
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
        @"SLEEP\s*\(",
        @"(\bOR\b|\bAND\b)\s+['""]?\w*['""]?\s*[=<>!]+\s*['""]?\w*['""]?\s*--",
        @"(\bOR\b|\bAND\b)\s+['""]?1['""]?\s*=\s*['""]?1['""]?",
        @"(\'\s*(or|and)\s*\')",
        @"(exec(\s|\+)+(s|x)p\w+)",
        @"(script\s*:)",
        @"(javascript\s*:)",
        @"(vbscript\s*:)",
        @"(<\s*script)",
        @"(onload\s*=)",
        @"(onerror\s*=)",
        @"(onclick\s*=)"
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

        foreach (var pattern in _injectionPatterns)
        {
            if (Regex.IsMatch(sql, pattern, RegexOptions.IgnoreCase))
            {
                detectedPatterns.Add(pattern);
            }
        }

        return detectedPatterns;
    }

    public bool ValidateParameterizedQuery(string sql, Dictionary<string, object> parameters)
    {
        try
        {
            // Ensure all parameters are properly typed and validated
            if (parameters == null || !parameters.Any())
                return true; // No parameters to validate

            foreach (var param in parameters)
            {
                // Check parameter name format
                if (!IsValidParameterName(param.Key))
                    return false;

                // Check for suspicious parameter values
                if (param.Value != null && ContainsSuspiciousContent(param.Value.ToString() ?? ""))
                    return false;

                // Validate parameter type safety
                if (!IsValidParameterType(param.Value))
                    return false;
            }

            // Ensure all parameters in SQL have corresponding values
            var sqlParams = ExtractParameterNames(sql);
            return sqlParams.All(p => parameters.ContainsKey(p));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error validating parameterized query");
            return false;
        }
    }

    public SqlValidationResult ValidateQueryStructure(string sql)
    {
        var result = new SqlValidationResult();

        try
        {
            // Check for proper SQL structure
            var trimmedSql = sql.Trim();

            // Must start with SELECT
            if (!trimmedSql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            {
                result.Errors.Add("Query must start with SELECT");
                result.SecurityLevel = SecurityLevel.Blocked;
                return result;
            }

            // Check for balanced parentheses
            if (!HasBalancedParentheses(sql))
            {
                result.Errors.Add("Query has unbalanced parentheses");
                result.SecurityLevel = SecurityLevel.Dangerous;
            }

            // Check for balanced quotes
            if (!HasBalancedQuotes(sql))
            {
                result.Errors.Add("Query has unbalanced quotes");
                result.SecurityLevel = SecurityLevel.Dangerous;
            }

            // Validate FROM clause existence
            if (!Regex.IsMatch(sql, @"\bFROM\b", RegexOptions.IgnoreCase))
            {
                result.Warnings.Add("Query does not contain FROM clause");
            }

            // Check for potential performance issues
            CheckPerformanceWarnings(sql, result);

            result.IsValid = !result.Errors.Any();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating query structure");
            result.Errors.Add("Failed to validate query structure");
            result.SecurityLevel = SecurityLevel.Dangerous;
        }

        return result;
    }

    private bool IsValidParameterName(string paramName)
    {
        // Parameter names should start with @ and contain only alphanumeric characters and underscores
        return Regex.IsMatch(paramName, @"^@?[a-zA-Z_][a-zA-Z0-9_]*$");
    }

    private bool ContainsSuspiciousContent(string value)
    {
        if (string.IsNullOrEmpty(value))
            return false;

        // Check for common injection attempts in parameter values
        var suspiciousPatterns = new[]
        {
            @"'.*--",
            @"'.*\bOR\b.*'",
            @"'.*\bAND\b.*'",
            @"'.*\bUNION\b.*'",
            @"'.*\bSELECT\b.*'",
            @"<script",
            @"javascript:",
            @"vbscript:"
        };

        return suspiciousPatterns.Any(pattern =>
            Regex.IsMatch(value, pattern, RegexOptions.IgnoreCase));
    }

    private bool IsValidParameterType(object? value)
    {
        if (value == null)
            return true;

        // Allow only safe parameter types
        var allowedTypes = new[]
        {
            typeof(string), typeof(int), typeof(long), typeof(decimal), typeof(double), typeof(float),
            typeof(DateTime), typeof(bool), typeof(Guid), typeof(byte[])
        };

        return allowedTypes.Contains(value.GetType());
    }

    private List<string> ExtractParameterNames(string sql)
    {
        var parameters = new List<string>();
        var matches = Regex.Matches(sql, @"@([a-zA-Z_][a-zA-Z0-9_]*)", RegexOptions.IgnoreCase);

        foreach (Match match in matches)
        {
            var paramName = match.Groups[1].Value;
            if (!parameters.Contains(paramName))
                parameters.Add(paramName);
        }

        return parameters;
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
        var doubleQuoteCount = 0;

        for (int i = 0; i < sql.Length; i++)
        {
            if (sql[i] == '\'' && (i == 0 || sql[i - 1] != '\\'))
                singleQuoteCount++;
            else if (sql[i] == '"' && (i == 0 || sql[i - 1] != '\\'))
                doubleQuoteCount++;
        }

        return singleQuoteCount % 2 == 0 && doubleQuoteCount % 2 == 0;
    }

    private void CheckPerformanceWarnings(string sql, SqlValidationResult result)
    {
        // Check for SELECT *
        if (Regex.IsMatch(sql, @"SELECT\s+\*", RegexOptions.IgnoreCase))
        {
            result.Warnings.Add("Consider specifying columns instead of using SELECT *");
        }

        // Check for missing WHERE clause
        if (!Regex.IsMatch(sql, @"\bWHERE\b", RegexOptions.IgnoreCase))
        {
            result.Warnings.Add("Query does not contain WHERE clause - may return large result set");
        }

        // Check for potential Cartesian product
        var fromMatches = Regex.Matches(sql, @"\bFROM\b", RegexOptions.IgnoreCase);
        var joinMatches = Regex.Matches(sql, @"\b(INNER\s+JOIN|LEFT\s+JOIN|RIGHT\s+JOIN|FULL\s+JOIN|JOIN)\b", RegexOptions.IgnoreCase);
        var tableCount = Regex.Matches(sql, @"\b[a-zA-Z_][a-zA-Z0-9_]*\s*(?=\s|,|$|\)|WHERE|ORDER|GROUP)", RegexOptions.IgnoreCase).Count;

        if (tableCount > 1 && joinMatches.Count < tableCount - 1)
        {
            result.Warnings.Add("Potential Cartesian product detected - verify JOIN conditions");
        }
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
