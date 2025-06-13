using MediatR;
using FluentValidation;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Security;

namespace BIReportingCopilot.Infrastructure.Behaviors;

/// <summary>
/// Enhanced validation behavior for MediatR pipeline
/// Validates commands and queries before processing
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators,
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)
    {
        _validators = validators;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        if (!_validators.Any())
        {
            _logger.LogDebug("No validators found for {RequestName}", requestName);
            return await next();
        }

        _logger.LogDebug("Validating {RequestName} with {ValidatorCount} validators", requestName, _validators.Count());

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            _logger.LogWarning("Validation failed for {RequestName} with {ErrorCount} errors: {Errors}",
                requestName, failures.Count, string.Join(", ", failures.Select(f => f.ErrorMessage)));

            throw new ValidationException(failures);
        }

        _logger.LogDebug("Validation passed for {RequestName}", requestName);
        return await next();
    }
}

/// <summary>
/// Performance monitoring behavior for MediatR pipeline
/// Tracks execution time and logs performance metrics
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;

    public PerformanceBehavior(ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogDebug("⏱️ Starting {RequestName}", requestName);

            var response = await next();
            var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            if (executionTime > 1000) // Log slow requests (>1 second)
            {
                _logger.LogWarning("🐌 Slow request detected: {RequestName} took {ExecutionTime}ms",
                    requestName, executionTime);
            }
            else
            {
                _logger.LogDebug("⚡ {RequestName} completed in {ExecutionTime}ms",
                    requestName, executionTime);
            }

            return response;
        }
        catch (Exception ex)
        {
            var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "❌ {RequestName} failed after {ExecutionTime}ms: {Error}",
                requestName, executionTime, ex.Message);
            throw;
        }
    }
}

/// <summary>
/// Audit behavior for MediatR pipeline
/// Logs all commands for audit trail
/// </summary>
public class AuditBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<AuditBehavior<TRequest, TResponse>> _logger;
    private readonly IAuditService _auditService;

    public AuditBehavior(
        ILogger<AuditBehavior<TRequest, TResponse>> logger,
        IAuditService auditService)
    {
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        // Only audit commands (not queries) for performance
        if (!requestName.EndsWith("Command"))
        {
            return await next();
        }

        var startTime = DateTime.UtcNow;
        var userId = ExtractUserId(request);

        try
        {
            _logger.LogDebug("📝 Auditing command: {CommandName} for user: {UserId}", requestName, userId);

            var response = await next();
            var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Log successful command execution
            await _auditService.LogAsync(
                action: $"COMMAND_EXECUTED_{requestName.ToUpperInvariant()}",
                userId: userId ?? "system",
                entityType: "Command",
                entityId: Guid.NewGuid().ToString(),
                details: new {
                    CommandName = requestName,
                    ExecutionTimeMs = executionTime,
                    Success = true
                });

            return response;
        }
        catch (Exception ex)
        {
            var executionTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            // Log failed command execution
            await _auditService.LogAsync(
                action: $"COMMAND_FAILED_{requestName.ToUpperInvariant()}",
                userId: userId ?? "system",
                entityType: "Command",
                entityId: Guid.NewGuid().ToString(),
                details: new {
                    CommandName = requestName,
                    ExecutionTimeMs = executionTime,
                    Success = false,
                    Error = ex.Message
                });

            throw;
        }
    }

    private string? ExtractUserId(TRequest request)
    {
        // Try to extract UserId from request using reflection
        var userIdProperty = typeof(TRequest).GetProperty("UserId");
        return userIdProperty?.GetValue(request)?.ToString();
    }
}
