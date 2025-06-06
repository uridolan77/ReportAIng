namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Result of query optimization analysis
/// </summary>
public class QueryOptimizationResult
{
    public string OriginalQuery { get; set; } = string.Empty;

    public string OptimizedQuery { get; set; } = string.Empty;

    public List<OptimizationSuggestion> Suggestions { get; set; } = new();

    public PerformanceMetrics EstimatedPerformance { get; set; } = new();

    public PerformanceMetrics OriginalPerformance { get; set; } = new();

    public double ImprovementScore { get; set; }

    public string OptimizationLevel { get; set; } = "None"; // None, Basic, Advanced, Aggressive

    public List<string> AppliedOptimizations { get; set; } = new();

    public List<string> Warnings { get; set; } = new();

    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;

    public int AnalysisTimeMs { get; set; }

    public string? Metadata { get; set; } // JSON for additional data


}

/// <summary>
/// Individual optimization suggestion
/// </summary>
public class OptimizationSuggestion
{
    public string Type { get; set; } = string.Empty; // Index, Rewrite, Join, etc.

    public string Description { get; set; } = string.Empty;

    public string Recommendation { get; set; } = string.Empty;

    public double ImpactScore { get; set; } // 0-1 scale

    public string Difficulty { get; set; } = "Medium"; // Easy, Medium, Hard

    public string Category { get; set; } = string.Empty; // Performance, Readability, etc.

    public List<string> AffectedTables { get; set; } = new();

    public List<string> AffectedColumns { get; set; } = new();

    public string? BeforeCode { get; set; }

    public string? AfterCode { get; set; }

    public string? Reasoning { get; set; }
}

// PerformanceMetrics class already exists in DashboardModels.cs and other files

/// <summary>
/// Query suggestion for autocomplete and recommendations
/// </summary>
public class QuerySuggestion
{
    public string Text { get; set; } = string.Empty;

    public string DisplayText { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty; // Table, Column, Function, Keyword, etc.

    public string Category { get; set; } = string.Empty; // Schema, Syntax, Template, etc.

    public double Relevance { get; set; } = 1.0;



    public int Priority { get; set; } = 0;

    public string? Icon { get; set; }

    public string? Documentation { get; set; }

    public List<string> Tags { get; set; } = new();

    public string? InsertText { get; set; } // Text to insert when selected

    public int CursorOffset { get; set; } = 0; // Where to place cursor after insertion

    public Dictionary<string, object> Metadata { get; set; } = new();

    public bool RequiresConfirmation { get; set; } = false;

    public string? ConfirmationMessage { get; set; }
}

// AIOptions class already exists in AIModels.cs

// StreamingResponse class already exists in AIModels.cs
