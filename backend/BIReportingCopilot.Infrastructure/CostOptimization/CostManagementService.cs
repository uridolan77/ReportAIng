using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.CostOptimization;

/// <summary>
/// Cost management service implementation for tracking, budgeting, and optimizing AI costs
/// </summary>
public class CostManagementService : ICostManagementService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<CostManagementService> _logger;
    private readonly Dictionary<string, decimal> _providerCostRates = new();
    private DateTime _lastRateUpdate = DateTime.MinValue;
    private readonly TimeSpan _rateUpdateInterval = TimeSpan.FromHours(1);

    public CostManagementService(
        BICopilotContext context,
        ILogger<CostManagementService> logger)
    {
        _context = context;
        _logger = logger;
        InitializeDefaultCostRates();
    }

    #region Cost Tracking

    public async Task<CostTrackingEntry> TrackCostAsync(CostTrackingEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new CostTrackingEntity
            {
                UserId = entry.UserId,
                ProviderId = entry.ProviderId,
                ModelId = entry.ModelId,
                RequestType = entry.RequestType,
                InputTokens = entry.InputTokens,
                OutputTokens = entry.OutputTokens,
                TotalTokens = entry.TotalTokens,
                Cost = entry.Cost,
                CostPerToken = entry.CostPerToken,
                DurationMs = entry.DurationMs,
                Timestamp = entry.Timestamp,
                RequestId = entry.RequestId,
                QueryId = entry.QueryId,
                Department = entry.Department,
                Project = entry.Project,
                Metadata = JsonSerializer.Serialize(entry.Metadata),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = entry.UserId
            };

            _context.CostTracking.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            entry.Id = entity.Id.ToString();
            
            // Update budget spent amounts
            await UpdateBudgetSpentAmountsAsync(entry.UserId, entry.Cost, cancellationToken);

            _logger.LogInformation("Tracked cost entry: {Cost} for user {UserId} using {ProviderId}/{ModelId}", 
                entry.Cost, entry.UserId, entry.ProviderId, entry.ModelId);

            return entry;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking cost entry for user {UserId}", entry.UserId);
            throw;
        }
    }

    public async Task<List<CostTrackingEntry>> GetCostHistoryAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.CostTracking.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(c => c.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(c => c.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.Timestamp <= endDate.Value);

            var entities = await query
                .OrderByDescending(c => c.Timestamp)
                .Take(1000) // Limit results
                .ToListAsync(cancellationToken);

            return entities.Select(MapToModel).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost history for user {UserId}", userId);
            return new List<CostTrackingEntry>();
        }
    }

    public async Task<decimal> GetTotalCostAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.CostTracking.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(c => c.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(c => c.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.Timestamp <= endDate.Value);

            return await query.SumAsync(c => c.Cost, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total cost for user {UserId}", userId);
            return 0;
        }
    }

    public async Task<Dictionary<string, decimal>> GetCostByProviderAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.CostTracking.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(c => c.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(c => c.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.Timestamp <= endDate.Value);

            return await query
                .GroupBy(c => c.ProviderId)
                .Select(g => new { Provider = g.Key, Cost = g.Sum(c => c.Cost) })
                .ToDictionaryAsync(x => x.Provider, x => x.Cost, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost by provider for user {UserId}", userId);
            return new Dictionary<string, decimal>();
        }
    }

    public async Task<Dictionary<string, decimal>> GetCostByModelAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.CostTracking.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(c => c.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(c => c.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.Timestamp <= endDate.Value);

            return await query
                .GroupBy(c => c.ModelId)
                .Select(g => new { Model = g.Key, Cost = g.Sum(c => c.Cost) })
                .ToDictionaryAsync(x => x.Model, x => x.Cost, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost by model for user {UserId}", userId);
            return new Dictionary<string, decimal>();
        }
    }

    #endregion

    #region Budget Management

    public async Task<BudgetManagement> CreateBudgetAsync(BudgetManagement budget, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new BudgetManagementEntity
            {
                Name = budget.Name,
                Type = budget.Type,
                EntityId = budget.EntityId,
                BudgetAmount = budget.BudgetAmount,
                SpentAmount = budget.SpentAmount,
                Period = budget.Period.ToString(),
                StartDate = budget.StartDate,
                EndDate = budget.EndDate,
                AlertThreshold = budget.AlertThreshold,
                BlockThreshold = budget.BlockThreshold,
                IsActive = budget.IsActive,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "system"
            };

            _context.BudgetManagement.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            budget.Id = entity.Id.ToString();

            _logger.LogInformation("Created budget: {Name} for {Type} {EntityId}", 
                budget.Name, budget.Type, budget.EntityId);

            return budget;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating budget: {Name}", budget.Name);
            throw;
        }
    }

    public async Task<BudgetManagement> UpdateBudgetAsync(BudgetManagement budget, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!long.TryParse(budget.Id, out var id))
                throw new ArgumentException("Invalid budget ID");

            var entity = await _context.BudgetManagement
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

            if (entity == null)
                throw new ArgumentException($"Budget not found: {budget.Id}");

            entity.Name = budget.Name;
            entity.BudgetAmount = budget.BudgetAmount;
            entity.Period = budget.Period.ToString();
            entity.StartDate = budget.StartDate;
            entity.EndDate = budget.EndDate;
            entity.AlertThreshold = budget.AlertThreshold;
            entity.BlockThreshold = budget.BlockThreshold;
            entity.IsActive = budget.IsActive;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = "system";

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated budget: {Name}", budget.Name);

            return budget;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating budget: {Id}", budget.Id);
            throw;
        }
    }

    public async Task<BudgetManagement?> GetBudgetAsync(string budgetId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!long.TryParse(budgetId, out var id))
                return null;

            var entity = await _context.BudgetManagement
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

            return entity != null ? MapBudgetToModel(entity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budget: {BudgetId}", budgetId);
            return null;
        }
    }

    public async Task<List<BudgetManagement>> GetBudgetsAsync(string entityId, string type, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.BudgetManagement.AsQueryable();

            if (!string.IsNullOrEmpty(entityId))
                query = query.Where(b => b.EntityId == entityId);

            if (!string.IsNullOrEmpty(type))
                query = query.Where(b => b.Type == type);

            var entities = await query
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync(cancellationToken);

            return entities.Select(MapBudgetToModel).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budgets for {Type} {EntityId}", type, entityId);
            return new List<BudgetManagement>();
        }
    }

    public async Task<bool> DeleteBudgetAsync(string budgetId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!long.TryParse(budgetId, out var id))
                return false;

            var entity = await _context.BudgetManagement
                .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

            if (entity == null)
                return false;

            entity.IsActive = false;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = "system";

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted budget: {BudgetId}", budgetId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting budget: {BudgetId}", budgetId);
            return false;
        }
    }

    public async Task<bool> CheckBudgetLimitAsync(string userId, decimal additionalCost, CancellationToken cancellationToken = default)
    {
        try
        {
            var budgets = await GetBudgetsAsync(userId, "User", cancellationToken);

            foreach (var budget in budgets.Where(b => b.IsActive))
            {
                var projectedSpent = budget.SpentAmount + additionalCost;
                var utilizationRate = budget.BudgetAmount > 0 ? projectedSpent / budget.BudgetAmount : 0;

                if (utilizationRate >= budget.BlockThreshold)
                {
                    _logger.LogWarning("Budget limit exceeded for user {UserId}. Budget: {BudgetName}, Utilization: {Utilization:P}",
                        userId, budget.Name, utilizationRate);
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking budget limit for user {UserId}", userId);
            return true; // Fail open
        }
    }

    public async Task<List<BudgetManagement>> GetBudgetsNearLimitAsync(decimal threshold = 0.8m, CancellationToken cancellationToken = default)
    {
        try
        {
            var budgets = await _context.BudgetManagement
                .Where(b => b.IsActive)
                .ToListAsync(cancellationToken);

            return budgets
                .Where(b => b.BudgetAmount > 0 && (b.SpentAmount / b.BudgetAmount) >= threshold)
                .Select(MapBudgetToModel)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budgets near limit");
            return new List<BudgetManagement>();
        }
    }

    #endregion

    #region Cost Prediction

    public async Task<CostPrediction> PredictCostAsync(string queryId, string userId, ModelSelectionCriteria criteria, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple cost prediction based on historical data and model characteristics
            var historicalCosts = await GetCostHistoryAsync(userId, DateTime.UtcNow.AddDays(-30), null, cancellationToken);

            var avgCostPerToken = historicalCosts.Any() ?
                historicalCosts.Average(c => c.CostPerToken) :
                await GetProviderCostRateAsync(criteria.Priority == "Cost" ? "openai" : "azure", "gpt-4", cancellationToken);

            var estimatedTokens = EstimateTokensFromComplexity(criteria.QueryComplexity);
            var estimatedCost = avgCostPerToken * estimatedTokens;
            var confidenceScore = CalculateConfidenceScore(historicalCosts.Count, criteria);

            var prediction = new CostPrediction
            {
                QueryId = queryId,
                UserId = userId,
                EstimatedCost = estimatedCost,
                ConfidenceScore = confidenceScore,
                ModelUsed = "cost-prediction-v1",
                EstimatedTokens = estimatedTokens,
                EstimatedDurationMs = EstimateDurationFromComplexity(criteria.QueryComplexity),
                Factors = new Dictionary<string, object>
                {
                    ["historical_data_points"] = historicalCosts.Count,
                    ["avg_cost_per_token"] = avgCostPerToken,
                    ["query_complexity"] = criteria.QueryComplexity,
                    ["priority"] = criteria.Priority
                },
                CreatedAt = DateTime.UtcNow
            };

            // Store prediction
            var entity = new CostPredictionEntity
            {
                QueryId = prediction.QueryId,
                UserId = prediction.UserId,
                EstimatedCost = prediction.EstimatedCost,
                ConfidenceScore = prediction.ConfidenceScore,
                ModelUsed = prediction.ModelUsed,
                EstimatedTokens = prediction.EstimatedTokens,
                EstimatedDurationMs = prediction.EstimatedDurationMs,
                Factors = JsonSerializer.Serialize(prediction.Factors),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.CostPredictions.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            prediction.Id = entity.Id.ToString();

            return prediction;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting cost for query {QueryId}", queryId);
            throw;
        }
    }

    public async Task<List<CostPrediction>> GetCostPredictionsAsync(string userId, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.CostPredictions.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(p => p.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(p => p.CreatedDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.CreatedDate <= endDate.Value);

            var entities = await query
                .OrderByDescending(p => p.CreatedDate)
                .Take(100)
                .ToListAsync(cancellationToken);

            return entities.Select(MapPredictionToModel).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost predictions for user {UserId}", userId);
            return new List<CostPrediction>();
        }
    }

    #endregion

    #region Cost Analytics

    public async Task<CostAnalyticsSummary> GetCostAnalyticsAsync(string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.CostTracking.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(c => c.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(c => c.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.Timestamp <= endDate.Value);

            var costs = await query.ToListAsync(cancellationToken);

            var summary = new CostAnalyticsSummary
            {
                TotalCost = costs.Sum(c => c.Cost),
                DailyCost = costs.Where(c => c.Timestamp >= DateTime.UtcNow.Date).Sum(c => c.Cost),
                WeeklyCost = costs.Where(c => c.Timestamp >= DateTime.UtcNow.AddDays(-7)).Sum(c => c.Cost),
                MonthlyCost = costs.Where(c => c.Timestamp >= DateTime.UtcNow.AddDays(-30)).Sum(c => c.Cost),
                CostByProvider = costs.GroupBy(c => c.ProviderId).ToDictionary(g => g.Key, g => g.Sum(c => c.Cost)),
                CostByUser = costs.GroupBy(c => c.UserId).ToDictionary(g => g.Key, g => g.Sum(c => c.Cost)),
                CostByDepartment = costs.Where(c => !string.IsNullOrEmpty(c.Department)).GroupBy(c => c.Department!).ToDictionary(g => g.Key, g => g.Sum(c => c.Cost)),
                CostByModel = costs.GroupBy(c => c.ModelId).ToDictionary(g => g.Key, g => g.Sum(c => c.Cost)),
                Trends = await GenerateCostTrendsAsync(costs),
                GeneratedAt = DateTime.UtcNow
            };

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cost analytics");
            return new CostAnalyticsSummary { GeneratedAt = DateTime.UtcNow };
        }
    }

    public async Task<List<CostTrend>> GetCostTrendsAsync(string? userId = null, string? category = null, int days = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var startDate = DateTime.UtcNow.AddDays(-days);
            var query = _context.CostTracking
                .Where(c => c.Timestamp >= startDate);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(c => c.UserId == userId);

            var costs = await query.ToListAsync(cancellationToken);

            return costs
                .GroupBy(c => c.Timestamp.Date)
                .Select(g => new CostTrend
                {
                    Date = g.Key,
                    Amount = g.Sum(c => c.Cost),
                    Category = category ?? "Total",
                    Metadata = new Dictionary<string, object>
                    {
                        ["transaction_count"] = g.Count(),
                        ["avg_cost_per_transaction"] = g.Average(c => c.Cost)
                    }
                })
                .OrderBy(t => t.Date)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost trends");
            return new List<CostTrend>();
        }
    }

    public async Task<Dictionary<string, decimal>> GetCostBreakdownAsync(string breakdownType, string? userId = null, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.CostTracking.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(c => c.UserId == userId);

            if (startDate.HasValue)
                query = query.Where(c => c.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.Timestamp <= endDate.Value);

            var costs = await query.ToListAsync(cancellationToken);

            return breakdownType.ToLower() switch
            {
                "provider" => costs.GroupBy(c => c.ProviderId).ToDictionary(g => g.Key, g => g.Sum(c => c.Cost)),
                "model" => costs.GroupBy(c => c.ModelId).ToDictionary(g => g.Key, g => g.Sum(c => c.Cost)),
                "user" => costs.GroupBy(c => c.UserId).ToDictionary(g => g.Key, g => g.Sum(c => c.Cost)),
                "department" => costs.Where(c => !string.IsNullOrEmpty(c.Department)).GroupBy(c => c.Department!).ToDictionary(g => g.Key, g => g.Sum(c => c.Cost)),
                "project" => costs.Where(c => !string.IsNullOrEmpty(c.Project)).GroupBy(c => c.Project!).ToDictionary(g => g.Key, g => g.Sum(c => c.Cost)),
                _ => new Dictionary<string, decimal>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost breakdown by {BreakdownType}", breakdownType);
            return new Dictionary<string, decimal>();
        }
    }

    #endregion

    #region Cost Optimization

    public async Task<List<CostOptimizationRecommendation>> GetCostOptimizationRecommendationsAsync(string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.CostOptimizationRecommendations.AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                // Filter recommendations relevant to the user
                var userCosts = await GetCostHistoryAsync(userId, DateTime.UtcNow.AddDays(-30), null, cancellationToken);
                // For now, return all recommendations - could be enhanced to filter by user patterns
            }

            var entities = await query
                .Where(r => !r.IsImplemented)
                .OrderByDescending(r => r.ImpactScore)
                .Take(10)
                .ToListAsync(cancellationToken);

            return entities.Select(MapRecommendationToModel).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost optimization recommendations");
            return new List<CostOptimizationRecommendation>();
        }
    }

    public async Task<CostOptimizationRecommendation> CreateRecommendationAsync(CostOptimizationRecommendation recommendation, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = new CostOptimizationRecommendationEntity
            {
                Type = recommendation.Type,
                Title = recommendation.Title,
                Description = recommendation.Description,
                PotentialSavings = recommendation.PotentialSavings,
                ImpactScore = recommendation.ImpactScore,
                Priority = recommendation.Priority,
                Implementation = recommendation.Implementation,
                Benefits = JsonSerializer.Serialize(recommendation.Benefits),
                Risks = JsonSerializer.Serialize(recommendation.Risks),
                IsImplemented = recommendation.IsImplemented,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "system"
            };

            _context.CostOptimizationRecommendations.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            recommendation.Id = entity.Id.ToString();

            _logger.LogInformation("Created cost optimization recommendation: {Title}", recommendation.Title);

            return recommendation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cost optimization recommendation");
            throw;
        }
    }

    public async Task<bool> ImplementRecommendationAsync(string recommendationId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!long.TryParse(recommendationId, out var id))
                return false;

            var entity = await _context.CostOptimizationRecommendations
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            if (entity == null)
                return false;

            entity.IsImplemented = true;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = "system";

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Implemented cost optimization recommendation: {RecommendationId}", recommendationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error implementing recommendation: {RecommendationId}", recommendationId);
            return false;
        }
    }

    #endregion

    #region Provider Cost Management

    public async Task<Dictionary<string, decimal>> GetProviderCostRatesAsync(CancellationToken cancellationToken = default)
    {
        await RefreshCostRatesIfNeededAsync();
        return new Dictionary<string, decimal>(_providerCostRates);
    }

    public async Task UpdateProviderCostRateAsync(string providerId, string modelId, decimal costPerToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{providerId}:{modelId}";
            _providerCostRates[key] = costPerToken;
            _lastRateUpdate = DateTime.UtcNow;

            _logger.LogInformation("Updated cost rate for {ProviderId}/{ModelId}: {CostPerToken}",
                providerId, modelId, costPerToken);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating provider cost rate");
            throw;
        }
    }

    public async Task<decimal> CalculateEstimatedCostAsync(string providerId, string modelId, int estimatedTokens, CancellationToken cancellationToken = default)
    {
        try
        {
            var costPerToken = await GetProviderCostRateAsync(providerId, modelId, cancellationToken);
            return costPerToken * estimatedTokens;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating estimated cost");
            return 0;
        }
    }

    #endregion

    #region Alerts and Notifications

    public async Task<List<CostAlert>> GetActiveAlertsAsync(string? userId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // Generate alerts based on budget thresholds
            var alerts = new List<CostAlert>();
            var budgets = await _context.BudgetManagement
                .Where(b => b.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var budget in budgets)
            {
                if (!string.IsNullOrEmpty(userId) && budget.Type == "User" && budget.EntityId != userId)
                    continue;

                var utilizationRate = budget.BudgetAmount > 0 ? budget.SpentAmount / budget.BudgetAmount : 0;

                if (utilizationRate >= budget.AlertThreshold)
                {
                    alerts.Add(new CostAlert
                    {
                        ProviderId = "budget-system",
                        AlertType = utilizationRate >= budget.BlockThreshold ? "BUDGET_EXCEEDED" : "BUDGET_WARNING",
                        Threshold = budget.AlertThreshold,
                        ThresholdAmount = budget.BudgetAmount * budget.AlertThreshold,
                        CurrentValue = (double)utilizationRate,
                        Message = $"Budget '{budget.Name}' is at {utilizationRate:P} utilization",
                        CreatedAt = DateTime.UtcNow,
                        IsResolved = false,
                        IsEnabled = true
                    });
                }
            }

            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active alerts");
            return new List<CostAlert>();
        }
    }

    public async Task<CostAlert> CreateAlertAsync(CostAlert alert, CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, just return the alert - could be enhanced to store in database
            alert.Id = Guid.NewGuid().ToString();
            alert.CreatedAt = DateTime.UtcNow;

            _logger.LogInformation("Created cost alert: {AlertType} - {Message}", alert.AlertType, alert.Message);

            return alert;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating alert");
            throw;
        }
    }

    public async Task<bool> ResolveAlertAsync(string alertId, CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, just log - could be enhanced to update database
            _logger.LogInformation("Resolved cost alert: {AlertId}", alertId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving alert: {AlertId}", alertId);
            return false;
        }
    }

    #endregion

    #region Helper Methods

    private void InitializeDefaultCostRates()
    {
        // Default cost rates (per 1K tokens) - these should be configurable
        _providerCostRates["openai:gpt-4"] = 0.03m;
        _providerCostRates["openai:gpt-3.5-turbo"] = 0.002m;
        _providerCostRates["azure:gpt-4"] = 0.03m;
        _providerCostRates["azure:gpt-35-turbo"] = 0.002m;
        _lastRateUpdate = DateTime.UtcNow;
    }

    private async Task RefreshCostRatesIfNeededAsync()
    {
        if (DateTime.UtcNow - _lastRateUpdate > _rateUpdateInterval)
        {
            // Could fetch updated rates from external API or configuration
            _lastRateUpdate = DateTime.UtcNow;
        }
        await Task.CompletedTask;
    }

    private async Task<decimal> GetProviderCostRateAsync(string providerId, string modelId, CancellationToken cancellationToken = default)
    {
        await RefreshCostRatesIfNeededAsync();
        var key = $"{providerId}:{modelId}";
        return _providerCostRates.TryGetValue(key, out var rate) ? rate : 0.01m; // Default fallback rate
    }

    private async Task UpdateBudgetSpentAmountsAsync(string userId, decimal cost, CancellationToken cancellationToken)
    {
        try
        {
            var userBudgets = await _context.BudgetManagement
                .Where(b => b.IsActive && b.Type == "User" && b.EntityId == userId)
                .ToListAsync(cancellationToken);

            foreach (var budget in userBudgets)
            {
                budget.SpentAmount += cost;
                budget.UpdatedDate = DateTime.UtcNow;
                budget.UpdatedBy = "system";
            }

            if (userBudgets.Any())
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating budget spent amounts for user {UserId}", userId);
        }
    }

    private static int EstimateTokensFromComplexity(string complexity)
    {
        return complexity?.ToLower() switch
        {
            "simple" => 500,
            "medium" => 1500,
            "complex" => 3000,
            "very_complex" => 5000,
            _ => 1000
        };
    }

    private static long EstimateDurationFromComplexity(string complexity)
    {
        return complexity?.ToLower() switch
        {
            "simple" => 2000,
            "medium" => 5000,
            "complex" => 10000,
            "very_complex" => 20000,
            _ => 5000
        };
    }

    private static decimal CalculateConfidenceScore(int historicalDataPoints, ModelSelectionCriteria criteria)
    {
        var baseConfidence = Math.Min(historicalDataPoints / 10.0m, 1.0m);
        var complexityAdjustment = criteria.QueryComplexity?.ToLower() switch
        {
            "simple" => 0.1m,
            "medium" => 0.0m,
            "complex" => -0.1m,
            "very_complex" => -0.2m,
            _ => 0.0m
        };

        return Math.Max(0.1m, Math.Min(1.0m, baseConfidence + complexityAdjustment));
    }

    private async Task<List<CostTrend>> GenerateCostTrendsAsync(List<CostTrackingEntity> costs)
    {
        return costs
            .GroupBy(c => c.Timestamp.Date)
            .Select(g => new CostTrend
            {
                Date = g.Key,
                Amount = g.Sum(c => c.Cost),
                Category = "Total",
                Metadata = new Dictionary<string, object>
                {
                    ["transaction_count"] = g.Count(),
                    ["avg_cost_per_transaction"] = g.Average(c => c.Cost)
                }
            })
            .OrderBy(t => t.Date)
            .ToList();
    }

    private static CostTrackingEntry MapToModel(CostTrackingEntity entity)
    {
        return new CostTrackingEntry
        {
            Id = entity.Id.ToString(),
            UserId = entity.UserId,
            ProviderId = entity.ProviderId,
            ModelId = entity.ModelId,
            RequestType = entity.RequestType,
            InputTokens = entity.InputTokens,
            OutputTokens = entity.OutputTokens,
            TotalTokens = entity.TotalTokens,
            Cost = entity.Cost,
            CostPerToken = entity.CostPerToken,
            DurationMs = entity.DurationMs,
            Timestamp = entity.Timestamp,
            RequestId = entity.RequestId,
            QueryId = entity.QueryId,
            Department = entity.Department,
            Project = entity.Project,
            Metadata = string.IsNullOrEmpty(entity.Metadata) ?
                new Dictionary<string, object>() :
                JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Metadata) ?? new Dictionary<string, object>()
        };
    }

    private static BudgetManagement MapBudgetToModel(BudgetManagementEntity entity)
    {
        return new BudgetManagement
        {
            Id = entity.Id.ToString(),
            Name = entity.Name,
            Type = entity.Type,
            EntityId = entity.EntityId,
            BudgetAmount = entity.BudgetAmount,
            SpentAmount = entity.SpentAmount,
            Period = Enum.TryParse<BudgetPeriod>(entity.Period, out var period) ? period : BudgetPeriod.Monthly,
            StartDate = entity.StartDate,
            EndDate = entity.EndDate,
            AlertThreshold = entity.AlertThreshold,
            BlockThreshold = entity.BlockThreshold,
            IsActive = entity.IsActive,
            CreatedAt = entity.CreatedDate,
            UpdatedAt = entity.UpdatedDate ?? entity.CreatedDate
        };
    }

    private static CostPrediction MapPredictionToModel(CostPredictionEntity entity)
    {
        return new CostPrediction
        {
            Id = entity.Id.ToString(),
            QueryId = entity.QueryId,
            UserId = entity.UserId,
            EstimatedCost = entity.EstimatedCost,
            ConfidenceScore = entity.ConfidenceScore,
            ModelUsed = entity.ModelUsed,
            EstimatedTokens = entity.EstimatedTokens,
            EstimatedDurationMs = entity.EstimatedDurationMs,
            Factors = string.IsNullOrEmpty(entity.Factors) ?
                new Dictionary<string, object>() :
                JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Factors) ?? new Dictionary<string, object>(),
            CreatedAt = entity.CreatedDate
        };
    }

    private static CostOptimizationRecommendation MapRecommendationToModel(CostOptimizationRecommendationEntity entity)
    {
        return new CostOptimizationRecommendation
        {
            Id = entity.Id.ToString(),
            Type = entity.Type,
            Title = entity.Title,
            Description = entity.Description,
            PotentialSavings = entity.PotentialSavings,
            ImpactScore = entity.ImpactScore,
            Priority = entity.Priority,
            Implementation = entity.Implementation,
            Benefits = string.IsNullOrEmpty(entity.Benefits) ?
                new List<string>() :
                JsonSerializer.Deserialize<List<string>>(entity.Benefits) ?? new List<string>(),
            Risks = string.IsNullOrEmpty(entity.Risks) ?
                new List<string>() :
                JsonSerializer.Deserialize<List<string>>(entity.Risks) ?? new List<string>(),
            CreatedAt = entity.CreatedDate,
            IsImplemented = entity.IsImplemented
        };
    }

    #endregion
}
