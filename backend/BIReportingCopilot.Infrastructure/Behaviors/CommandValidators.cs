using FluentValidation;
using BIReportingCopilot.Core.Commands;

namespace BIReportingCopilot.Infrastructure.Behaviors;

/// <summary>
/// Validator for ProcessQueryCommand
/// </summary>
public class ProcessQueryCommandValidator : AbstractValidator<ProcessQueryCommand>
{
    public ProcessQueryCommandValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty()
            .WithMessage("Question cannot be empty")
            .MinimumLength(3)
            .WithMessage("Question must be at least 3 characters long")
            .MaximumLength(2000)
            .WithMessage("Question cannot exceed 2000 characters");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("SessionId is required");

        RuleFor(x => x.Options)
            .NotNull()
            .WithMessage("Options cannot be null");

        When(x => x.Options != null, () =>
        {
            RuleFor(x => x.Options.MaxRows)
                .GreaterThan(0)
                .WithMessage("MaxRows must be greater than 0")
                .LessThanOrEqualTo(10000)
                .WithMessage("MaxRows cannot exceed 10000");

            RuleFor(x => x.Options.TimeoutSeconds)
                .GreaterThan(0)
                .WithMessage("TimeoutSeconds must be greater than 0")
                .LessThanOrEqualTo(300)
                .WithMessage("TimeoutSeconds cannot exceed 300 seconds");
        });
    }
}

/// <summary>
/// Validator for ExecuteQueryCommand
/// </summary>
public class ExecuteQueryCommandValidator : AbstractValidator<ExecuteQueryCommand>
{
    public ExecuteQueryCommandValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty()
            .WithMessage("Question cannot be empty")
            .MinimumLength(3)
            .WithMessage("Question must be at least 3 characters long")
            .MaximumLength(2000)
            .WithMessage("Question cannot exceed 2000 characters");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("SessionId is required");

        RuleFor(x => x.Options)
            .NotNull()
            .WithMessage("Options cannot be null");
    }
}

/// <summary>
/// Validator for GenerateSqlCommand
/// </summary>
public class GenerateSqlCommandValidator : AbstractValidator<GenerateSqlCommand>
{
    public GenerateSqlCommandValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty()
            .WithMessage("Question cannot be empty")
            .MinimumLength(3)
            .WithMessage("Question must be at least 3 characters long");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("SessionId is required");

        RuleFor(x => x.Schema)
            .NotNull()
            .WithMessage("Schema cannot be null");

        When(x => x.Schema != null, () =>
        {
            RuleFor(x => x.Schema.Tables)
                .NotNull()
                .WithMessage("Schema tables cannot be null")
                .Must(tables => tables.Any())
                .WithMessage("Schema must contain at least one table");
        });
    }
}

/// <summary>
/// Validator for ExecuteSqlCommand
/// </summary>
public class ExecuteSqlCommandValidator : AbstractValidator<ExecuteSqlCommand>
{
    public ExecuteSqlCommandValidator()
    {
        RuleFor(x => x.Sql)
            .NotEmpty()
            .WithMessage("SQL cannot be empty")
            .MinimumLength(10)
            .WithMessage("SQL must be at least 10 characters long")
            .MaximumLength(50000)
            .WithMessage("SQL cannot exceed 50000 characters");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.Options)
            .NotNull()
            .WithMessage("Options cannot be null");

        // Basic SQL injection protection
        RuleFor(x => x.Sql)
            .Must(NotContainDangerousKeywords)
            .WithMessage("SQL contains potentially dangerous keywords");
    }

    private bool NotContainDangerousKeywords(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return true;

        var dangerousKeywords = new[] { "DROP", "DELETE", "TRUNCATE", "ALTER", "CREATE", "EXEC", "EXECUTE" };
        var upperSql = sql.ToUpperInvariant();

        return !dangerousKeywords.Any(keyword => upperSql.Contains(keyword));
    }
}

/// <summary>
/// Validator for CacheQueryCommand
/// </summary>
public class CacheQueryCommandValidator : AbstractValidator<CacheQueryCommand>
{
    public CacheQueryCommandValidator()
    {
        RuleFor(x => x.QueryHash)
            .NotEmpty()
            .WithMessage("QueryHash cannot be empty");

        RuleFor(x => x.Response)
            .NotNull()
            .WithMessage("Response cannot be null");

        When(x => x.Expiry.HasValue, () =>
        {
            RuleFor(x => x.Expiry!.Value)
                .GreaterThan(TimeSpan.Zero)
                .WithMessage("Expiry must be greater than zero")
                .LessThanOrEqualTo(TimeSpan.FromDays(30))
                .WithMessage("Expiry cannot exceed 30 days");
        });
    }
}

/// <summary>
/// Validator for SubmitQueryFeedbackCommand
/// </summary>
public class SubmitQueryFeedbackCommandValidator : AbstractValidator<SubmitQueryFeedbackCommand>
{
    public SubmitQueryFeedbackCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.Feedback)
            .NotNull()
            .WithMessage("Feedback cannot be null");

        When(x => x.Feedback != null, () =>
        {
            RuleFor(x => x.Feedback.QueryId)
                .NotEmpty()
                .WithMessage("QueryId is required");

            RuleFor(x => x.Feedback.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5");

            When(x => !string.IsNullOrEmpty(x.Feedback.Feedback), () =>
            {
                RuleFor(x => x.Feedback.Feedback)
                    .MaximumLength(2000)
                    .WithMessage("Feedback cannot exceed 2000 characters");
            });
        });
    }
}

/// <summary>
/// Validator for InvalidateQueryCacheCommand
/// </summary>
public class InvalidateQueryCacheCommandValidator : AbstractValidator<InvalidateQueryCacheCommand>
{
    public InvalidateQueryCacheCommandValidator()
    {
        RuleFor(x => x.Pattern)
            .NotEmpty()
            .WithMessage("Pattern cannot be empty")
            .MaximumLength(500)
            .WithMessage("Pattern cannot exceed 500 characters");
    }
}
