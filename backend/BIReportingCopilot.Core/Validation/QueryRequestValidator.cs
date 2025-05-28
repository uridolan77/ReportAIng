using FluentValidation;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Validation;

/// <summary>
/// Validator for query requests
/// </summary>
public class QueryRequestValidator : AbstractValidator<ValidationQueryRequest>
{
    public QueryRequestValidator()
    {
        RuleFor(x => x.NaturalLanguageQuery)
            .NotEmpty()
            .WithMessage("Natural language query is required")
            .MinimumLength(3)
            .WithMessage("Query must be at least 3 characters long")
            .MaximumLength(2000)
            .WithMessage("Query cannot exceed 2000 characters")
            .Must(BeValidQuery)
            .WithMessage("Query contains invalid or potentially harmful content");

        RuleFor(x => x.DatabaseName)
            .MaximumLength(128)
            .WithMessage("Database name cannot exceed 128 characters")
            .Matches(@"^[a-zA-Z0-9_\-\.]*$")
            .When(x => !string.IsNullOrEmpty(x.DatabaseName))
            .WithMessage("Database name contains invalid characters");

        RuleFor(x => x.SchemaName)
            .MaximumLength(128)
            .WithMessage("Schema name cannot exceed 128 characters")
            .Matches(@"^[a-zA-Z0-9_\-\.]*$")
            .When(x => !string.IsNullOrEmpty(x.SchemaName))
            .WithMessage("Schema name contains invalid characters");

        RuleFor(x => x.MaxRows)
            .GreaterThan(0)
            .WithMessage("MaxRows must be greater than 0")
            .LessThanOrEqualTo(10000)
            .WithMessage("MaxRows cannot exceed 10,000 for performance reasons")
            .When(x => x.MaxRows.HasValue);

        RuleFor(x => x.TimeoutSeconds)
            .GreaterThan(0)
            .WithMessage("Timeout must be greater than 0")
            .LessThanOrEqualTo(300)
            .WithMessage("Timeout cannot exceed 300 seconds")
            .When(x => x.TimeoutSeconds.HasValue);

        RuleFor(x => x.IncludeExplanation)
            .NotNull()
            .WithMessage("IncludeExplanation must be specified");

        RuleFor(x => x.CacheResults)
            .NotNull()
            .WithMessage("CacheResults must be specified");
    }

    private static bool BeValidQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return false;

        // Check for suspicious patterns
        var suspiciousPatterns = new[]
        {
            @"\b(exec|execute)\s*\(",
            @"\b(sp_|xp_)",
            @"--\s*$",
            @"/\*.*\*/",
            @"\b(waitfor|delay)\b",
            @"\b(shutdown|restart)\b",
            @"@@version",
            @"\b(bulk\s+insert|openrowset|opendatasource)\b"
        };

        foreach (var pattern in suspiciousPatterns)
        {
            if (System.Text.RegularExpressions.Regex.IsMatch(query, pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}

/// <summary>
/// Validator for feedback requests
/// </summary>
public class FeedbackRequestValidator : AbstractValidator<FeedbackRequest>
{
    public FeedbackRequestValidator()
    {
        RuleFor(x => x.QueryId)
            .NotEmpty()
            .WithMessage("Query ID is required")
            .Must(BeValidGuid)
            .WithMessage("Query ID must be a valid GUID");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5");

        RuleFor(x => x.Comments)
            .MaximumLength(1000)
            .WithMessage("Comments cannot exceed 1000 characters")
            .Must(NotContainHarmfulContent)
            .When(x => !string.IsNullOrEmpty(x.Comments))
            .WithMessage("Comments contain inappropriate content");

        RuleFor(x => x.FeedbackType)
            .NotEmpty()
            .WithMessage("Feedback type is required")
            .Must(BeValidFeedbackType)
            .WithMessage("Invalid feedback type");

        RuleFor(x => x.IsHelpful)
            .NotNull()
            .WithMessage("IsHelpful must be specified");
    }

    private static bool BeValidGuid(string value)
    {
        return Guid.TryParse(value, out _);
    }

    private static bool NotContainHarmfulContent(string? comments)
    {
        if (string.IsNullOrEmpty(comments))
            return true;

        var harmfulPatterns = new[]
        {
            @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>",
            @"javascript:",
            @"vbscript:",
            @"onload\s*=",
            @"onerror\s*=",
            @"onclick\s*="
        };

        return !harmfulPatterns.Any(pattern =>
            System.Text.RegularExpressions.Regex.IsMatch(comments, pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }

    private static bool BeValidFeedbackType(string feedbackType)
    {
        var validTypes = new[] { "Rating", "Bug", "Feature", "General", "Performance", "Accuracy" };
        return validTypes.Contains(feedbackType, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Validator for user registration requests
/// </summary>
public class UserRegistrationValidator : AbstractValidator<UserRegistrationRequest>
{
    public UserRegistrationValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .MinimumLength(3)
            .WithMessage("Username must be at least 3 characters long")
            .MaximumLength(50)
            .WithMessage("Username cannot exceed 50 characters")
            .Matches(@"^[a-zA-Z0-9_\-\.]+$")
            .WithMessage("Username can only contain letters, numbers, underscores, hyphens, and dots");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format")
            .MaximumLength(255)
            .WithMessage("Email cannot exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long")
            .MaximumLength(128)
            .WithMessage("Password cannot exceed 128 characters")
            .Must(HaveValidPasswordComplexity)
            .WithMessage("Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Display name is required")
            .MaximumLength(100)
            .WithMessage("Display name cannot exceed 100 characters")
            .Must(NotContainHarmfulContent)
            .WithMessage("Display name contains inappropriate content");

        RuleFor(x => x.Department)
            .MaximumLength(100)
            .WithMessage("Department cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.Department));

        RuleFor(x => x.Role)
            .NotEmpty()
            .WithMessage("Role is required")
            .Must(BeValidRole)
            .WithMessage("Invalid role specified");
    }

    private static bool HaveValidPasswordComplexity(string password)
    {
        if (string.IsNullOrEmpty(password))
            return false;

        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(c => "!@#$%^&*()_+-=[]{}|;:,.<>?".Contains(c));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }

    private static bool NotContainHarmfulContent(string? content)
    {
        if (string.IsNullOrEmpty(content))
            return true;

        var harmfulPatterns = new[]
        {
            @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>",
            @"javascript:",
            @"vbscript:",
            @"[<>""']"
        };

        return !harmfulPatterns.Any(pattern =>
            System.Text.RegularExpressions.Regex.IsMatch(content, pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }

    private static bool BeValidRole(string role)
    {
        var validRoles = new[] { "User", "PowerUser", "Admin", "Analyst", "Viewer" };
        return validRoles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}

/// <summary>
/// Validator for business table configuration
/// </summary>
public class BusinessTableConfigValidator : AbstractValidator<BusinessTableConfigRequest>
{
    public BusinessTableConfigValidator()
    {
        RuleFor(x => x.SchemaName)
            .NotEmpty()
            .WithMessage("Schema name is required")
            .MaximumLength(128)
            .WithMessage("Schema name cannot exceed 128 characters")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Schema name can only contain letters, numbers, and underscores");

        RuleFor(x => x.TableName)
            .NotEmpty()
            .WithMessage("Table name is required")
            .MaximumLength(128)
            .WithMessage("Table name cannot exceed 128 characters")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Table name can only contain letters, numbers, and underscores");

        RuleFor(x => x.BusinessPurpose)
            .NotEmpty()
            .WithMessage("Business purpose is required")
            .MaximumLength(500)
            .WithMessage("Business purpose cannot exceed 500 characters");

        RuleFor(x => x.DataClassification)
            .NotEmpty()
            .WithMessage("Data classification is required")
            .Must(BeValidDataClassification)
            .WithMessage("Invalid data classification");

        RuleFor(x => x.AccessLevel)
            .NotEmpty()
            .WithMessage("Access level is required")
            .Must(BeValidAccessLevel)
            .WithMessage("Invalid access level");

        RuleFor(x => x.Tags)
            .Must(HaveValidTags)
            .When(x => x.Tags != null && x.Tags.Any())
            .WithMessage("Tags contain invalid characters or exceed length limits");
    }

    private static bool BeValidDataClassification(string classification)
    {
        var validClassifications = new[] { "Public", "Internal", "Confidential", "Restricted" };
        return validClassifications.Contains(classification, StringComparer.OrdinalIgnoreCase);
    }

    private static bool BeValidAccessLevel(string accessLevel)
    {
        var validLevels = new[] { "Read", "ReadWrite", "Admin" };
        return validLevels.Contains(accessLevel, StringComparer.OrdinalIgnoreCase);
    }

    private static bool HaveValidTags(List<string>? tags)
    {
        if (tags == null || !tags.Any())
            return true;

        return tags.All(tag =>
            !string.IsNullOrWhiteSpace(tag) &&
            tag.Length <= 50 &&
            System.Text.RegularExpressions.Regex.IsMatch(tag, @"^[a-zA-Z0-9_\-\s]+$"));
    }
}

/// <summary>
/// Request models for validation
/// </summary>
public class ValidationQueryRequest
{
    public string NaturalLanguageQuery { get; set; } = string.Empty;
    public string? DatabaseName { get; set; }
    public string? SchemaName { get; set; }
    public int? MaxRows { get; set; }
    public int? TimeoutSeconds { get; set; }
    public bool IncludeExplanation { get; set; }
    public bool CacheResults { get; set; } = true;
}

public class FeedbackRequest
{
    public string QueryId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comments { get; set; }
    public string FeedbackType { get; set; } = string.Empty;
    public bool IsHelpful { get; set; }
}

public class UserRegistrationRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string Role { get; set; } = string.Empty;
}

public class BusinessTableConfigRequest
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string BusinessPurpose { get; set; } = string.Empty;
    public string DataClassification { get; set; } = string.Empty;
    public string AccessLevel { get; set; } = string.Empty;
    public List<string>? Tags { get; set; }
}
