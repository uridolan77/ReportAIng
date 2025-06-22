using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Analytics;

/// <summary>
/// Service for tracking and analyzing token usage patterns
/// </summary>
public class TokenUsageAnalyticsService : ITokenUsageAnalyticsService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<TokenUsageAnalyticsService> _logger;

    public TokenUsageAnalyticsService(BICopilotContext context, ILogger<TokenUsageAnalyticsService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task RecordTokenUsageAsync(string userId, string requestType, string intentType, 
        int tokensUsed, decimal cost, CancellationToken cancellationToken = default)
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            
            // Try to find existing record for today
            var existingRecord = await _context.TokenUsageAnalytics
                .FirstOrDefaultAsync(t => t.UserId == userId && 
                                        t.Date == today && 
                                        t.RequestType == requestType && 
                                        t.IntentType == intentType, 
                                   cancellationToken);

            if (existingRecord != null)
            {
                // Update existing record
                existingRecord.AddUsage(tokensUsed, cost);
                _context.TokenUsageAnalytics.Update(existingRecord);
            }
            else
            {
                // Create new record
                var newRecord = new TokenUsageAnalyticsEntity
                {
                    UserId = userId,
                    Date = today,
                    RequestType = requestType,
                    IntentType = intentType,
                    TotalRequests = 1,
                    TotalTokensUsed = tokensUsed,
                    TotalCost = cost
                };
                newRecord.CalculateAverages();
                
                _context.TokenUsageAnalytics.Add(newRecord);
            }

            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("✅ Recorded token usage: User={UserId}, Type={RequestType}, Intent={IntentType}, Tokens={Tokens}, Cost={Cost:C}", 
                userId, requestType, intentType, tokensUsed, cost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error recording token usage for user {UserId}", userId);
            throw;
        }
    }

    public async Task<IEnumerable<TokenUsageRecord>> GetDailyUsageAsync(string userId,
        DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var entities = await _context.TokenUsageAnalytics
            .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
            .OrderBy(t => t.Date)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToTokenUsageRecord);
    }

    public async Task<IEnumerable<TokenUsageRecord>> GetUsageByRequestTypeAsync(string requestType,
        DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var entities = await _context.TokenUsageAnalytics
            .Where(t => t.RequestType == requestType && t.Date >= startDate && t.Date <= endDate)
            .OrderBy(t => t.Date)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToTokenUsageRecord);
    }

    public async Task<IEnumerable<TokenUsageRecord>> GetUsageByIntentTypeAsync(string intentType,
        DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        var entities = await _context.TokenUsageAnalytics
            .Where(t => t.IntentType == intentType && t.Date >= startDate && t.Date <= endDate)
            .OrderBy(t => t.Date)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToTokenUsageRecord);
    }

    public async Task<TokenUsageStatistics> GetUsageStatisticsAsync(DateTime startDate, DateTime endDate, 
        string? userId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.TokenUsageAnalytics
            .Where(t => t.Date >= startDate && t.Date <= endDate);

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(t => t.UserId == userId);
        }

        var data = await query.ToListAsync(cancellationToken);

        var totalRequests = data.Sum(t => t.TotalRequests);
        var totalTokens = data.Sum(t => t.TotalTokensUsed);
        var totalCost = data.Sum(t => t.TotalCost);

        return new TokenUsageStatistics
        {
            TotalRequests = totalRequests,
            TotalTokens = totalTokens,
            TotalCost = totalCost,
            AverageTokensPerRequest = totalRequests > 0 ? (decimal)totalTokens / totalRequests : 0,
            AverageCostPerRequest = totalRequests > 0 ? totalCost / totalRequests : 0,
            RequestTypeBreakdown = data.GroupBy(t => t.RequestType)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.TotalRequests)),
            IntentTypeBreakdown = data.GroupBy(t => t.IntentType)
                .ToDictionary(g => g.Key, g => g.Sum(t => t.TotalRequests)),
            StartDate = startDate,
            EndDate = endDate
        };
    }

    public async Task<IEnumerable<UserTokenUsageSummary>> GetTopUsersByUsageAsync(DateTime startDate, DateTime endDate, 
        int topCount = 10, CancellationToken cancellationToken = default)
    {
        var userSummaries = await _context.TokenUsageAnalytics
            .Where(t => t.Date >= startDate && t.Date <= endDate)
            .GroupBy(t => t.UserId)
            .Select(g => new UserTokenUsageSummary
            {
                UserId = g.Key,
                TotalRequests = g.Sum(t => t.TotalRequests),
                TotalTokens = g.Sum(t => t.TotalTokensUsed),
                TotalCost = g.Sum(t => t.TotalCost),
                AverageTokensPerRequest = g.Sum(t => t.TotalRequests) > 0 ? 
                    (decimal)g.Sum(t => t.TotalTokensUsed) / g.Sum(t => t.TotalRequests) : 0,
                MostUsedRequestType = g.GroupBy(t => t.RequestType)
                    .OrderByDescending(rt => rt.Sum(t => t.TotalRequests))
                    .Select(rt => rt.Key)
                    .FirstOrDefault() ?? "",
                MostUsedIntentType = g.GroupBy(t => t.IntentType)
                    .OrderByDescending(it => it.Sum(t => t.TotalRequests))
                    .Select(it => it.Key)
                    .FirstOrDefault() ?? ""
            })
            .OrderByDescending(u => u.TotalTokens)
            .Take(topCount)
            .ToListAsync(cancellationToken);

        return userSummaries;
    }

    public async Task<IEnumerable<TokenUsageTrend>> GetUsageTrendsAsync(DateTime startDate, DateTime endDate, 
        string? userId = null, string? requestType = null, CancellationToken cancellationToken = default)
    {
        var query = _context.TokenUsageAnalytics
            .Where(t => t.Date >= startDate && t.Date <= endDate);

        if (!string.IsNullOrEmpty(userId))
        {
            query = query.Where(t => t.UserId == userId);
        }

        if (!string.IsNullOrEmpty(requestType))
        {
            query = query.Where(t => t.RequestType == requestType);
        }

        var trends = await query
            .GroupBy(t => t.Date)
            .Select(g => new TokenUsageTrend
            {
                Date = g.Key,
                TotalRequests = g.Sum(t => t.TotalRequests),
                TotalTokens = g.Sum(t => t.TotalTokensUsed),
                TotalCost = g.Sum(t => t.TotalCost),
                AverageTokensPerRequest = g.Sum(t => t.TotalRequests) > 0 ? 
                    (decimal)g.Sum(t => t.TotalTokensUsed) / g.Sum(t => t.TotalRequests) : 0,
                RequestType = requestType,
                IntentType = null
            })
            .OrderBy(t => t.Date)
            .ToListAsync(cancellationToken);

        return trends;
    }

    public async Task AggregateUsageDataAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔄 Starting token usage aggregation for date: {Date}", date.ToString("yyyy-MM-dd"));

            // This method would typically aggregate raw usage data into daily summaries
            // For now, since we're already aggregating in real-time, this is a placeholder
            // In a production system, you might aggregate from more granular logs

            var existingRecords = await _context.TokenUsageAnalytics
                .Where(t => t.Date == date.Date)
                .ToListAsync(cancellationToken);

            _logger.LogInformation("✅ Token usage aggregation completed for {Date}. Found {Count} existing records", 
                date.ToString("yyyy-MM-dd"), existingRecords.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during token usage aggregation for date: {Date}", date.ToString("yyyy-MM-dd"));
            throw;
        }
    }

    public async Task UpdateBusinessContextAsync(string id, string? naturalLanguageDescription = null, 
        string? businessRules = null, string? relationshipContext = null, 
        string? dataGovernanceLevel = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var record = await _context.TokenUsageAnalytics
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (record == null)
            {
                _logger.LogWarning("⚠️ Token usage analytics record not found: {Id}", id);
                return;
            }

            if (!string.IsNullOrEmpty(naturalLanguageDescription))
                record.NaturalLanguageDescription = naturalLanguageDescription;
            
            if (!string.IsNullOrEmpty(businessRules))
                record.BusinessRules = businessRules;
            
            if (!string.IsNullOrEmpty(relationshipContext))
                record.RelationshipContext = relationshipContext;
            
            if (!string.IsNullOrEmpty(dataGovernanceLevel))
                record.DataGovernanceLevel = dataGovernanceLevel;

            record.LastBusinessReview = DateTime.UtcNow;
            record.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogDebug("✅ Updated business context for token usage analytics: {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating business context for token usage analytics: {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Map entity to Core model
    /// </summary>
    private static TokenUsageRecord MapToTokenUsageRecord(TokenUsageAnalyticsEntity entity)
    {
        return new TokenUsageRecord
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Date = entity.Date,
            RequestType = entity.RequestType,
            IntentType = entity.IntentType,
            TotalRequests = entity.TotalRequests,
            TotalTokensUsed = entity.TotalTokensUsed,
            TotalCost = entity.TotalCost,
            AverageTokensPerRequest = entity.AverageTokensPerRequest,
            AverageCostPerRequest = entity.AverageCostPerRequest,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
