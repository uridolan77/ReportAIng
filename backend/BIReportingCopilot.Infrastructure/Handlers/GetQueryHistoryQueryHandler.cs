using MediatR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BIReportingCopilot.Core.Queries;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;

namespace BIReportingCopilot.Infrastructure.Handlers;

public class GetQueryHistoryQueryHandler : IRequestHandler<GetQueryHistoryQuery, PagedResult<QueryHistoryItem>>
{
    private readonly BICopilotContext _context;
    private readonly IMapper _mapper;

    public GetQueryHistoryQueryHandler(BICopilotContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PagedResult<QueryHistoryItem>> Handle(
        GetQueryHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.QueryHistory
            .Where(q => q.UserId == request.UserId)
            .AsQueryable();

        // Apply filters
        if (request.StartDate.HasValue)
            query = query.Where(q => q.QueryTimestamp >= request.StartDate.Value);

        if (request.EndDate.HasValue)
            query = query.Where(q => q.QueryTimestamp <= request.EndDate.Value);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(q =>
                q.NaturalLanguageQuery.Contains(request.SearchTerm) ||
                q.GeneratedSQL.Contains(request.SearchTerm));
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Get paged data
        var items = await query
            .OrderByDescending(q => q.QueryTimestamp)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var mappedItems = _mapper.Map<List<QueryHistoryItem>>(items);

        return new PagedResult<QueryHistoryItem>
        {
            Items = mappedItems,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
