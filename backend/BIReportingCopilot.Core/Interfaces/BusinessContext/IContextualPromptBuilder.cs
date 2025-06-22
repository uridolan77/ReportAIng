using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.Models.PromptGeneration;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.BusinessContext;

/// <summary>
/// Service for building contextually-aware prompts for LLM consumption
/// </summary>
public interface IContextualPromptBuilder
{
    /// <summary>
    /// Build a business-aware prompt from user question and context
    /// </summary>
    /// <param name="userQuestion">The natural language question</param>
    /// <param name="profile">Business context profile</param>
    /// <param name="schema">Contextual business schema</param>
    /// <returns>Generated business-aware prompt</returns>
    Task<string> BuildBusinessAwarePromptAsync(
        string userQuestion, 
        BusinessContextProfile profile, 
        ContextualBusinessSchema schema);

    /// <summary>
    /// Select the optimal prompt template for a given context
    /// </summary>
    /// <param name="profile">Business context profile</param>
    /// <returns>Selected prompt template</returns>
    Task<PromptTemplate> SelectOptimalTemplateAsync(BusinessContextProfile profile);

    /// <summary>
    /// Enrich a base prompt with business context
    /// </summary>
    /// <param name="basePrompt">Base prompt template</param>
    /// <param name="schema">Contextual business schema</param>
    /// <returns>Enriched prompt with business context</returns>
    Task<string> EnrichPromptWithBusinessContextAsync(
        string basePrompt, 
        ContextualBusinessSchema schema);

    /// <summary>
    /// Find relevant query examples for context
    /// </summary>
    /// <param name="profile">Business context profile</param>
    /// <param name="maxExamples">Maximum number of examples to return</param>
    /// <returns>List of relevant query examples</returns>
    Task<List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample>> FindRelevantExamplesAsync(
        BusinessContextProfile profile,
        int maxExamples = 3);
}
