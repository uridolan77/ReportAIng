using MediatR;
using FluentValidation;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Commands;

public class ExecuteQueryCommand : IRequest<QueryResponse>
{
    public string Question { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public QueryOptions Options { get; set; } = new();
}

public class ExecuteQueryCommandValidator : AbstractValidator<ExecuteQueryCommand>
{
    public ExecuteQueryCommandValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty().WithMessage("Question is required")
            .MinimumLength(3).WithMessage("Question must be at least 3 characters")
            .MaximumLength(2000).WithMessage("Question cannot exceed 2000 characters");

        RuleFor(x => x.SessionId)
            .NotEmpty().WithMessage("SessionId is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Options.MaxRows)
            .InclusiveBetween(1, 10000)
            .WithMessage("MaxRows must be between 1 and 10000");

        RuleFor(x => x.Options.ConfidenceThreshold)
            .InclusiveBetween(0.0, 1.0)
            .WithMessage("ConfidenceThreshold must be between 0 and 1");
    }
}
