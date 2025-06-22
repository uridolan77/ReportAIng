using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Interfaces
{
    /// <summary>
    /// Repository interface for AI transparency data persistence
    /// </summary>
    public interface ITransparencyRepository
    {
        // Prompt Construction Traces
        Task<PromptConstructionTraceEntity> SavePromptTraceAsync(PromptConstructionTraceEntity trace);
        Task<PromptConstructionTraceEntity?> GetPromptTraceAsync(string traceId);
        Task<List<PromptConstructionTraceEntity>> GetPromptTracesByUserAsync(string userId, int limit = 50);
        Task UpdatePromptTraceAsync(PromptConstructionTraceEntity trace);

        // Prompt Construction Steps
        Task<PromptConstructionStepEntity> SavePromptStepAsync(PromptConstructionStepEntity step);
        Task<List<PromptConstructionStepEntity>> GetPromptStepsByTraceAsync(string traceId);

        // Token Budgets
        Task<TokenBudgetEntity> SaveTokenBudgetAsync(TokenBudgetEntity budget);
        Task<TokenBudgetEntity?> GetTokenBudgetAsync(string id);
        Task<List<TokenBudgetEntity>> GetTokenBudgetsByUserAsync(string userId, int limit = 50);

        // Business Context Profiles
        Task<BusinessContextProfileEntity> SaveBusinessContextAsync(BusinessContextProfileEntity profile);
        Task<BusinessContextProfileEntity?> GetBusinessContextAsync(string id);
        Task<List<BusinessContextProfileEntity>> GetBusinessContextsByUserAsync(string userId, int limit = 50);

        // Business Entities
        Task<BusinessEntityEntity> SaveBusinessEntityAsync(BusinessEntityEntity entity);
        Task<List<BusinessEntityEntity>> GetBusinessEntitiesByProfileAsync(string profileId);

        // Transparency Reports
        Task<TransparencyReportEntity> SaveTransparencyReportAsync(TransparencyReportEntity report);
        Task<TransparencyReportEntity?> GetTransparencyReportAsync(string traceId);
        Task<List<TransparencyReportEntity>> GetTransparencyReportsByUserAsync(string userId, int limit = 50);

        // Verification and Analytics
        Task<bool> VerifyTraceExistsAsync(string traceId);
        Task<Dictionary<string, int>> GetTransparencyStatisticsAsync(string userId);
        Task<List<PromptConstructionTraceEntity>> GetRecentTracesAsync(int limit = 10);
    }
}
