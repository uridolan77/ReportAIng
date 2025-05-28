using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

namespace BIReportingCopilot.Infrastructure.Security;

/// <summary>
/// Enhanced SQL validation with advanced security checks and pattern detection
/// </summary>
public class EnhancedSqlValidator : ISqlQueryValidator
{
    private readonly ILogger<EnhancedSqlValidator> _logger;
    private readonly SqlValidationConfiguration _config;

    // Dangerous SQL patterns that should be blocked
    private static readonly string[] DangerousPatterns = {
        @"\b(DROP|DELETE|TRUNCATE|ALTER|CREATE|INSERT|UPDATE)\b",
        @"\b(EXEC|EXECUTE|SP_|XP_)\b",
        @"\b(UNION\s+ALL\s+SELECT|UNION\s+SELECT)\b",
        @";\s*(DROP|DELETE|TRUNCATE|ALTER|CREATE|INSERT|UPDATE)",
        @"--\s*$",
        @"/\*.*?\*/",
        @"\b(WAITFOR|DELAY)\b",
        @"\b(OPENROWSET|OPENDATASOURCE)\b",
        @"\b(BULK\s+INSERT)\b"
    };

    // Suspicious patterns that warrant additional scrutiny
    private static readonly string[] SuspiciousPatterns = {
        @"'\s*OR\s*'.*?'\s*=\s*'",
        @"'\s*OR\s*1\s*=\s*1",
        @"'\s*OR\s*'1'\s*=\s*'1'",
        @"'\s*;\s*--",
        @"CHAR\s*\(\s*\d+\s*\)",
        @"ASCII\s*\(\s*",
        @"SUBSTRING\s*\(\s*",
        @"LEN\s*\(\s*",
        @"CAST\s*\(\s*",
        @"CONVERT\s*\(\s*"
    };

    public EnhancedSqlValidator(ILogger<EnhancedSqlValidator> logger)
    {
        _logger = logger;
        _config = new SqlValidationConfiguration();
    }

    public async Task<SqlValidationResult> ValidateAsync(string sql)
    {
        var result = new SqlValidationResult
        {
            IsValid = true,
            SecurityLevel = SecurityLevel.Safe,
            Errors = new List<string>(),
            Warnings = new List<string>(),
            ValidationTime = DateTime.UtcNow
        };

        if (string.IsNullOrWhiteSpace(sql))
        {
            result.IsValid = false;
            result.Errors.Add("SQL query cannot be empty");
            return result;
        }

        // Check for dangerous patterns
        await CheckDangerousPatternsAsync(sql, result);

        // Check for suspicious patterns
        await CheckSuspiciousPatternsAsync(sql, result);

        // Check for SQL injection patterns
        await CheckSqlInjectionPatternsAsync(sql, result);

        // Validate query structure
        await ValidateQueryStructureAsync(sql, result);

        // Check for parameter usage
        await CheckParameterUsageAsync(sql, result);

        // Calculate risk score
        result.RiskScore = CalculateRiskScore(result);

        // Set final security level
        result.SecurityLevel = DetermineSecurityLevel(result);

        _logger.LogInformation("SQL validation completed. Risk Score: {RiskScore}, Security Level: {SecurityLevel}", 
            result.RiskScore, result.SecurityLevel);

        return result;
    }

    private async Task CheckDangerousPatternsAsync(string sql, SqlValidationResult result)
    {
        await Task.Run(() =>
        {
            foreach (var pattern in DangerousPatterns)
            {
                if (Regex.IsMatch(sql, pattern, RegexOptions.IgnoreCase))
                {
                    result.IsValid = false;
                    result.Errors.Add($"Dangerous SQL pattern detected: {pattern}");
                    result.SecurityLevel = SecurityLevel.Blocked;
                    _logger.LogWarning("Dangerous SQL pattern detected: {Pattern} in query: {Query}", pattern, sql);
                }
            }
        });
    }

    private async Task CheckSuspiciousPatternsAsync(string sql, SqlValidationResult result)
    {
        await Task.Run(() =>
        {
            foreach (var pattern in SuspiciousPatterns)
            {
                if (Regex.IsMatch(sql, pattern, RegexOptions.IgnoreCase))
                {
                    result.Warnings.Add($"Suspicious SQL pattern detected: {pattern}");
                    result.SecurityLevel = SecurityLevel.Warning;
                    _logger.LogWarning("Suspicious SQL pattern detected: {Pattern} in query: {Query}", pattern, sql);
                }
            }
        });
    }

    private async Task CheckSqlInjectionPatternsAsync(string sql, SqlValidationResult result)
    {
        await Task.Run(() =>
        {
            // Check for common SQL injection patterns
            var injectionPatterns = new[]
            {
                @"'\s*OR\s*'",
                @"'\s*AND\s*'",
                @"1\s*=\s*1",
                @"1\s*=\s*0",
                @"'\s*=\s*'",
                @"--",
                @"/\*",
                @"\*/"
            };

            foreach (var pattern in injectionPatterns)
            {
                if (Regex.IsMatch(sql, pattern, RegexOptions.IgnoreCase))
                {
                    result.Warnings.Add($"Potential SQL injection pattern: {pattern}");
                    _logger.LogWarning("Potential SQL injection pattern detected: {Pattern}", pattern);
                }
            }
        });
    }

    private async Task ValidateQueryStructureAsync(string sql, SqlValidationResult result)
    {
        await Task.Run(() =>
        {
            // Ensure query starts with SELECT
            if (!Regex.IsMatch(sql.Trim(), @"^\s*SELECT\b", RegexOptions.IgnoreCase))
            {
                result.IsValid = false;
                result.Errors.Add("Only SELECT queries are allowed");
                result.SecurityLevel = SecurityLevel.Blocked;
            }

            // Check for multiple statements
            var statements = sql.Split(';', StringSplitOptions.RemoveEmptyEntries);
            if (statements.Length > 1)
            {
                result.Warnings.Add("Multiple SQL statements detected");
                result.SecurityLevel = SecurityLevel.Warning;
            }
        });
    }

    private async Task CheckParameterUsageAsync(string sql, SqlValidationResult result)
    {
        await Task.Run(() =>
        {
            // Check if query contains user input without parameters
            var hasUserInput = ContainsUserInput(sql);
            var hasParameters = ContainsParameterPlaceholders(sql);

            if (hasUserInput && !hasParameters)
            {
                result.Warnings.Add("Direct user input detected - consider using parameterized queries");
                result.SecurityLevel = SecurityLevel.Warning;
            }
        });
    }

    private bool ContainsUserInput(string sql)
    {
        // Simple heuristic to detect potential user input
        return Regex.IsMatch(sql, @"'\s*\+\s*", RegexOptions.IgnoreCase) ||
               Regex.IsMatch(sql, @"CONCAT\s*\(", RegexOptions.IgnoreCase);
    }

    private bool ContainsParameterPlaceholders(string sql)
    {
        // Check for parameter placeholders
        return Regex.IsMatch(sql, @"@\w+|\?\s*|\$\d+", RegexOptions.IgnoreCase);
    }

    private double CalculateRiskScore(SqlValidationResult result)
    {
        double score = 0.0;

        // Add points for errors and warnings
        score += result.Errors.Count * 10.0;
        score += result.Warnings.Count * 3.0;

        // Normalize to 0-1 scale
        return Math.Min(1.0, score / 100.0);
    }

    private SecurityLevel DetermineSecurityLevel(SqlValidationResult result)
    {
        if (!result.IsValid || result.Errors.Any())
            return SecurityLevel.Blocked;

        if (result.RiskScore > 0.7)
            return SecurityLevel.Blocked;

        if (result.RiskScore > 0.3 || result.Warnings.Any())
            return SecurityLevel.Warning;

        return SecurityLevel.Safe;
    }
}

/// <summary>
/// Configuration for SQL validation
/// </summary>
public class SqlValidationConfiguration
{
    public bool EnableAdvancedPatternDetection { get; set; } = true;
    public bool EnableMLAnomalyDetection { get; set; } = false;
    public double RiskThreshold { get; set; } = 0.5;
    public int MaxQueryLength { get; set; } = 10000;
    public bool AllowStoredProcedures { get; set; } = false;
    public bool AllowDynamicSql { get; set; } = false;
}

/// <summary>
/// Security levels for SQL queries
/// </summary>
public enum SecurityLevel
{
    Safe,
    Warning,
    Blocked
}

/// <summary>
/// Result of SQL validation
/// </summary>
public class SqlValidationResult
{
    public bool IsValid { get; set; }
    public SecurityLevel SecurityLevel { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public double RiskScore { get; set; }
    public DateTime ValidationTime { get; set; }
}
