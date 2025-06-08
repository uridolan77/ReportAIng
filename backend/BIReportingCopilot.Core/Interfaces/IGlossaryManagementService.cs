using BIReportingCopilot.Core.Models.DTOs;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for managing business glossary terms and definitions
/// </summary>
public interface IGlossaryManagementService
{
    // Glossary Operations
    Task<List<BusinessGlossaryDto>> GetGlossaryTermsAsync();
    Task<List<BusinessGlossaryDto>> GetGlossaryTermsByCategoryAsync(string category);
    Task<BusinessGlossaryDto?> GetGlossaryTermAsync(long id);
    Task<BusinessGlossaryDto?> FindGlossaryTermByNameAsync(string termName);
    Task<BusinessGlossaryDto> CreateGlossaryTermAsync(BusinessGlossaryDto request, string userId);
    Task<BusinessGlossaryDto?> UpdateGlossaryTermAsync(long id, BusinessGlossaryDto request, string userId);
    Task<bool> DeleteGlossaryTermAsync(long id);

    // Search and Categories
    Task<List<string>> GetCategoriesAsync();
    Task<List<BusinessGlossaryDto>> SearchGlossaryTermsAsync(string searchTerm);

    // Statistics and Usage
    Task<GlossaryStatistics> GetGlossaryStatisticsAsync();
    Task IncrementTermUsageAsync(long termId);
}

/// <summary>
/// Statistics for business glossary
/// </summary>
public class GlossaryStatistics
{
    public int TotalTerms { get; set; }
    public Dictionary<string, int> CategoryStats { get; set; } = new();
    public double AverageUsageCount { get; set; }
    public int TotalCategories { get; set; }
}
