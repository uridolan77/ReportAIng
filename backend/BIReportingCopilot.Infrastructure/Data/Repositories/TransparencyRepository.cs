using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.Interfaces;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository implementation for AI transparency data persistence
    /// </summary>
    public class TransparencyRepository : ITransparencyRepository
    {
        private readonly TransparencyDbContext _context;
        private readonly ILogger<TransparencyRepository> _logger;

        public TransparencyRepository(
            TransparencyDbContext context,
            ILogger<TransparencyRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Prompt Construction Traces

        public async Task<PromptConstructionTraceEntity> SavePromptTraceAsync(PromptConstructionTraceEntity trace)
        {
            try
            {
                _logger.LogDebug("Saving prompt trace {TraceId} for user {UserId}", trace.TraceId, trace.UserId);
                
                _context.PromptConstructionTraces.Add(trace);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Successfully saved prompt trace {TraceId}", trace.TraceId);
                return trace;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save prompt trace {TraceId}", trace.TraceId);
                throw;
            }
        }

        public async Task<PromptConstructionTraceEntity?> GetPromptTraceAsync(string traceId)
        {
            try
            {
                return await _context.PromptConstructionTraces
                    .Include(t => t.Steps)
                    .FirstOrDefaultAsync(t => t.TraceId == traceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get prompt trace {TraceId}", traceId);
                throw;
            }
        }

        public async Task<List<PromptConstructionTraceEntity>> GetPromptTracesByUserAsync(string userId, int limit = 50)
        {
            try
            {
                return await _context.PromptConstructionTraces
                    .Where(t => t.UserId == userId)
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get prompt traces for user {UserId}", userId);
                throw;
            }
        }

        public async Task UpdatePromptTraceAsync(PromptConstructionTraceEntity trace)
        {
            try
            {
                _context.PromptConstructionTraces.Update(trace);
                await _context.SaveChangesAsync();
                
                _logger.LogDebug("Updated prompt trace {TraceId}", trace.TraceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update prompt trace {TraceId}", trace.TraceId);
                throw;
            }
        }

        #endregion

        #region Prompt Construction Steps

        public async Task<PromptConstructionStepEntity> SavePromptStepAsync(PromptConstructionStepEntity step)
        {
            try
            {
                _context.PromptConstructionSteps.Add(step);
                await _context.SaveChangesAsync();
                
                _logger.LogDebug("Saved prompt step {StepName} for trace {TraceId}", step.StepName, step.TraceId);
                return step;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save prompt step {StepName} for trace {TraceId}", step.StepName, step.TraceId);
                throw;
            }
        }

        public async Task<List<PromptConstructionStepEntity>> GetPromptStepsByTraceAsync(string traceId)
        {
            try
            {
                return await _context.PromptConstructionSteps
                    .Where(s => s.TraceId == traceId)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get prompt steps for trace {TraceId}", traceId);
                throw;
            }
        }

        #endregion

        #region Token Budgets

        public async Task<TokenBudgetEntity> SaveTokenBudgetAsync(TokenBudgetEntity budget)
        {
            try
            {
                _context.TokenBudgets.Add(budget);
                await _context.SaveChangesAsync();
                
                _logger.LogDebug("Saved token budget for user {UserId}, intent {IntentType}", budget.UserId, budget.IntentType);
                return budget;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save token budget for user {UserId}", budget.UserId);
                throw;
            }
        }

        public async Task<TokenBudgetEntity?> GetTokenBudgetAsync(string id)
        {
            try
            {
                return await _context.TokenBudgets.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get token budget {Id}", id);
                throw;
            }
        }

        public async Task<List<TokenBudgetEntity>> GetTokenBudgetsByUserAsync(string userId, int limit = 50)
        {
            try
            {
                return await _context.TokenBudgets
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get token budgets for user {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Business Context Profiles

        public async Task<BusinessContextProfileEntity> SaveBusinessContextAsync(BusinessContextProfileEntity profile)
        {
            try
            {
                _context.BusinessContextProfiles.Add(profile);
                await _context.SaveChangesAsync();
                
                _logger.LogDebug("Saved business context profile for user {UserId}, intent {IntentType}", profile.UserId, profile.IntentType);
                return profile;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save business context profile for user {UserId}", profile.UserId);
                throw;
            }
        }

        public async Task<BusinessContextProfileEntity?> GetBusinessContextAsync(string id)
        {
            try
            {
                return await _context.BusinessContextProfiles.FindAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get business context profile {Id}", id);
                throw;
            }
        }

        public async Task<List<BusinessContextProfileEntity>> GetBusinessContextsByUserAsync(string userId, int limit = 50)
        {
            try
            {
                return await _context.BusinessContextProfiles
                    .Where(p => p.UserId == userId)
                    .OrderByDescending(p => p.CreatedAt)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get business context profiles for user {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Business Entities

        public async Task<BusinessEntityEntity> SaveBusinessEntityAsync(BusinessEntityEntity entity)
        {
            try
            {
                _context.BusinessEntities.Add(entity);
                await _context.SaveChangesAsync();
                
                _logger.LogDebug("Saved business entity {EntityType}: {EntityValue}", entity.EntityType, entity.EntityValue);
                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save business entity {EntityType}: {EntityValue}", entity.EntityType, entity.EntityValue);
                throw;
            }
        }

        public async Task<List<BusinessEntityEntity>> GetBusinessEntitiesByProfileAsync(string profileId)
        {
            try
            {
                return await _context.BusinessEntities
                    .Where(e => e.ProfileId == profileId)
                    .OrderBy(e => e.StartPosition)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get business entities for profile {ProfileId}", profileId);
                throw;
            }
        }

        #endregion

        #region Transparency Reports

        public async Task<TransparencyReportEntity> SaveTransparencyReportAsync(TransparencyReportEntity report)
        {
            try
            {
                _context.TransparencyReports.Add(report);
                await _context.SaveChangesAsync();
                
                _logger.LogDebug("Saved transparency report for trace {TraceId}", report.TraceId);
                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save transparency report for trace {TraceId}", report.TraceId);
                throw;
            }
        }

        public async Task<TransparencyReportEntity?> GetTransparencyReportAsync(string traceId)
        {
            try
            {
                return await _context.TransparencyReports
                    .FirstOrDefaultAsync(r => r.TraceId == traceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get transparency report for trace {TraceId}", traceId);
                throw;
            }
        }

        public async Task<List<TransparencyReportEntity>> GetTransparencyReportsByUserAsync(string userId, int limit = 50)
        {
            try
            {
                return await _context.TransparencyReports
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.GeneratedAt)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get transparency reports for user {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Verification and Analytics

        public async Task<bool> VerifyTraceExistsAsync(string traceId)
        {
            try
            {
                return await _context.PromptConstructionTraces
                    .AnyAsync(t => t.TraceId == traceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to verify trace exists {TraceId}", traceId);
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetTransparencyStatisticsAsync(string userId)
        {
            try
            {
                var stats = new Dictionary<string, int>();
                
                stats["TotalTraces"] = await _context.PromptConstructionTraces
                    .CountAsync(t => t.UserId == userId);
                
                stats["SuccessfulTraces"] = await _context.PromptConstructionTraces
                    .CountAsync(t => t.UserId == userId && t.Success);
                
                stats["TotalSteps"] = await _context.PromptConstructionSteps
                    .CountAsync(s => _context.PromptConstructionTraces
                        .Any(t => t.TraceId == s.TraceId && t.UserId == userId));
                
                stats["TotalBudgets"] = await _context.TokenBudgets
                    .CountAsync(b => b.UserId == userId);
                
                stats["TotalContextProfiles"] = await _context.BusinessContextProfiles
                    .CountAsync(p => p.UserId == userId);
                
                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get transparency statistics for user {UserId}", userId);
                throw;
            }
        }

        public async Task<List<PromptConstructionTraceEntity>> GetRecentTracesAsync(int limit = 10)
        {
            try
            {
                return await _context.PromptConstructionTraces
                    .OrderByDescending(t => t.CreatedAt)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get recent traces");
                throw;
            }
        }

        #endregion
    }
}
