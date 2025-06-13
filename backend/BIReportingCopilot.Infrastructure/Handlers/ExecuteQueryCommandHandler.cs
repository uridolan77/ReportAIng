using MediatR;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Query;

namespace BIReportingCopilot.Infrastructure.Handlers;

public class ExecuteQueryCommandHandler : IRequestHandler<ExecuteQueryCommand, QueryResponse>
{
    private readonly IQueryService _queryService;
    private readonly ILogger<ExecuteQueryCommandHandler> _logger;
    private readonly IHubContext<Hub> _hubContext;
    private readonly IValidator<ExecuteQueryCommand> _validator;

    public ExecuteQueryCommandHandler(
        IQueryService queryService,
        ILogger<ExecuteQueryCommandHandler> logger,
        IHubContext<Hub> hubContext,
        IValidator<ExecuteQueryCommand> validator)
    {
        _queryService = queryService;
        _logger = logger;
        _hubContext = hubContext;
        _validator = validator;
    }

    public async Task<QueryResponse> Handle(ExecuteQueryCommand request, CancellationToken cancellationToken)
    {
        // Validate request
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var queryId = Guid.NewGuid().ToString();

        try
        {
            // Notify query started via SignalR
            await _hubContext.Clients.All.SendAsync("QueryStarted", new
            {
                QueryId = queryId,
                UserId = request.UserId,
                Question = request.Question,
                Timestamp = DateTime.UtcNow
            }, cancellationToken);

            // Process query
            var queryRequest = new QueryRequest
            {
                Question = request.Question,
                SessionId = request.SessionId,
                Options = request.Options
            };

            var response = await _queryService.ProcessQueryAsync(queryRequest, request.UserId);
            response.QueryId = queryId;

            // Notify completion
            await _hubContext.Clients.All.SendAsync("QueryCompleted", new
            {
                QueryId = queryId,
                UserId = request.UserId,
                Success = response.Success,
                ExecutionTimeMs = response.ExecutionTimeMs,
                Error = response.Error,
                Timestamp = DateTime.UtcNow
            }, cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query command for user {UserId}", request.UserId);

            // Notify failure
            await _hubContext.Clients.All.SendAsync("QueryCompleted", new
            {
                QueryId = queryId,
                UserId = request.UserId,
                Success = false,
                ExecutionTimeMs = 0,
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            }, cancellationToken);

            throw;
        }
    }
}
