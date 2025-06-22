using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Models.PromptGeneration;

/// <summary>
/// Context for generating business-aware prompts
/// </summary>
public class PromptGenerationContext
{
    public string UserQuestion { get; set; } = string.Empty;
    public BusinessContextProfile BusinessContext { get; set; } = new();
    public ContextualBusinessSchema Schema { get; set; } = new();
    public PromptTemplate SelectedTemplate { get; set; } = new();
    public Dictionary<string, string> PlaceholderValues { get; set; } = new();
    public List<BIReportingCopilot.Core.Models.PromptGeneration.QueryExample> RelevantExamples { get; set; } = new();
    public PromptComplexityLevel ComplexityLevel { get; set; }
    public List<string> OptimizationHints { get; set; } = new();
}

/// <summary>
/// Example query for prompt context
/// </summary>
public class QueryExample
{
    public string NaturalLanguageQuery { get; set; } = string.Empty;
    public string GeneratedSql { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public string IntentType { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public double SuccessRate { get; set; }
    public double RelevanceScore { get; set; }
    public List<string> UsedTables { get; set; } = new();
}

/// <summary>
/// Prompt complexity levels
/// </summary>
public enum PromptComplexityLevel
{
    Basic,
    Standard,
    Advanced,
    Expert
}
