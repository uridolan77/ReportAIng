using BIReportingCopilot.Core.Models.BusinessContext;

namespace BIReportingCopilot.Core.Interfaces.BusinessContext;

/// <summary>
/// Service for analyzing business context from natural language queries
/// </summary>
public interface IBusinessContextAnalyzer
{
    /// <summary>
    /// Analyze a user's natural language question to extract business context
    /// </summary>
    /// <param name="userQuestion">The natural language question</param>
    /// <param name="userId">Optional user ID for personalization</param>
    /// <returns>Complete business context profile</returns>
    Task<BusinessContextProfile> AnalyzeUserQuestionAsync(string userQuestion, string? userId = null);

    /// <summary>
    /// Classify the business intent of a query
    /// </summary>
    /// <param name="userQuestion">The natural language question</param>
    /// <returns>Classified query intent</returns>
    Task<QueryIntent> ClassifyBusinessIntentAsync(string userQuestion);

    /// <summary>
    /// Detect the business domain of a query
    /// </summary>
    /// <param name="userQuestion">The natural language question</param>
    /// <returns>Detected business domain</returns>
    Task<BusinessDomain> DetectBusinessDomainAsync(string userQuestion);

    /// <summary>
    /// Extract business entities from a query
    /// </summary>
    /// <param name="userQuestion">The natural language question</param>
    /// <returns>List of extracted business entities</returns>
    Task<List<BusinessEntity>> ExtractBusinessEntitiesAsync(string userQuestion);

    /// <summary>
    /// Extract business terms from a query
    /// </summary>
    /// <param name="userQuestion">The natural language question</param>
    /// <returns>List of business terms</returns>
    Task<List<string>> ExtractBusinessTermsAsync(string userQuestion);

    /// <summary>
    /// Extract time context from a query
    /// </summary>
    /// <param name="userQuestion">The natural language question</param>
    /// <returns>Time range context if found</returns>
    Task<TimeRange?> ExtractTimeContextAsync(string userQuestion);
}
