using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Enhanced schema result with semantic enrichment
/// </summary>
public class EnhancedSchemaResult
{
    public string Query { get; set; } = string.Empty;
    public QuerySemanticAnalysis QueryAnalysis { get; set; } = new();
    public List<EnhancedTableInfo> RelevantTables { get; set; } = new();
    public LLMOptimizedContext LLMContext { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public double ConfidenceScore { get; set; } = 0.0;
}

/// <summary>
/// Semantic analysis of a query
/// </summary>
public class QuerySemanticAnalysis
{
    public string Query { get; set; } = string.Empty;
    public QueryIntent Intent { get; set; } = QueryIntent.General;
    public List<string> BusinessTerms { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public List<EntityExtraction> Entities { get; set; } = new();
    public double ConfidenceScore { get; set; } = 0.0;
}

/// <summary>
/// Query intent classification
/// </summary>
public enum QueryIntent
{
    General,
    Reporting,
    Analytics,
    Lookup,
    Aggregation,
    Comparison,
    TrendAnalysis,
    Trend, // Alias for TrendAnalysis for backward compatibility
    Filtering,
    Ranking,
    Forecasting,
    Unknown // Default fallback value
}

/// <summary>
/// Enhanced table information with semantic metadata
/// </summary>
public class EnhancedTableInfo
{
    public string TableName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string BusinessPurpose { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public string SemanticDescription { get; set; } = string.Empty;
    public double RelevanceScore { get; set; } = 0.0;
    public List<EnhancedColumnInfo> Columns { get; set; } = new();
    public List<string> BusinessProcesses { get; set; } = new();
    public List<string> AnalyticalUseCases { get; set; } = new();
    public List<string> ReportingCategories { get; set; } = new();
    public List<string> BusinessGlossaryTerms { get; set; } = new();
    public List<string> LLMContextHints { get; set; } = new();
    public decimal ImportanceScore { get; set; } = 0.5m;
    public decimal SemanticCoverageScore { get; set; } = 0.5m;
    public Dictionary<string, GlossaryTermContext> GlossaryContext { get; set; } = new();
}

/// <summary>
/// Enhanced column information with semantic metadata
/// </summary>
public class EnhancedColumnInfo
{
    public string ColumnName { get; set; } = string.Empty;
    public string BusinessMeaning { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public string SemanticContext { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public double RelevanceScore { get; set; } = 0.0;
    public List<string> NaturalLanguageAliases { get; set; } = new();
    public List<string> SemanticSynonyms { get; set; } = new();
    public List<string> BusinessMetrics { get; set; } = new();
    public Dictionary<string, object> QueryIntentMapping { get; set; } = new();
    public bool IsKeyColumn { get; set; } = false;
    public bool IsSensitiveData { get; set; } = false;
}

/// <summary>
/// Business glossary term context
/// </summary>
public class GlossaryTermContext
{
    public string Definition { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public List<string> Examples { get; set; } = new();
    public List<string> DisambiguationRules { get; set; } = new();
}

/// <summary>
/// LLM-optimized context for query processing
/// </summary>
public class LLMOptimizedContext
{
    public string ContextSummary { get; set; } = string.Empty;
    public List<EnhancedTableInfo> PrioritizedTables { get; set; } = new();
    public Dictionary<string, GlossaryTermContext> BusinessGlossaryContext { get; set; } = new();
    public List<string> QuerySpecificHints { get; set; } = new();
    public List<string> SchemaNavigationHints { get; set; } = new();
    public List<string> OptimizationSuggestions { get; set; } = new();
    public double ConfidenceScore { get; set; } = 0.0;
}

/// <summary>
/// Contextual prompts for LLM processing
/// </summary>
public class ContextualPrompts
{
    public List<string> QueryHints { get; set; } = new();
    public List<string> NavigationHints { get; set; } = new();
    public List<string> OptimizationSuggestions { get; set; } = new();
}

/// <summary>
/// LLM context builder for generating optimized prompts
/// </summary>
public class LLMContextBuilder
{
    private readonly List<string> _contextElements = new();
    private readonly List<string> _tableContexts = new();
    private readonly List<string> _glossaryContexts = new();

    public void AddQueryContext(QuerySemanticAnalysis analysis)
    {
        _contextElements.Add($"Query Intent: {analysis.Intent}");
        
        if (analysis.BusinessTerms.Any())
        {
            _contextElements.Add($"Business Terms: {string.Join(", ", analysis.BusinessTerms)}");
        }
        
        if (analysis.Keywords.Any())
        {
            _contextElements.Add($"Key Concepts: {string.Join(", ", analysis.Keywords)}");
        }
    }

    public void AddTableContext(EnhancedTableInfo table, QueryIntent intent)
    {
        var context = $"Table: {table.TableName}";
        
        if (!string.IsNullOrEmpty(table.BusinessPurpose))
        {
            context += $" - Purpose: {table.BusinessPurpose}";
        }
        
        if (!string.IsNullOrEmpty(table.SemanticDescription))
        {
            context += $" - Context: {table.SemanticDescription}";
        }
        
        if (table.Columns.Any())
        {
            var relevantColumns = table.Columns
                .Where(c => c.RelevanceScore > 0.5)
                .OrderByDescending(c => c.RelevanceScore)
                .Take(5)
                .Select(c => $"{c.ColumnName} ({c.BusinessMeaning})")
                .ToList();
            
            if (relevantColumns.Any())
            {
                context += $" - Key Columns: {string.Join(", ", relevantColumns)}";
            }
        }
        
        _tableContexts.Add(context);
    }

    public void AddGlossaryContext(IEnumerable<GlossaryTermContext> glossaryTerms)
    {
        foreach (var term in glossaryTerms)
        {
            if (!string.IsNullOrEmpty(term.Definition))
            {
                _glossaryContexts.Add($"{term.Definition}");
            }
        }
    }

    public string BuildSummary()
    {
        var summary = new List<string>();
        
        if (_contextElements.Any())
        {
            summary.Add("Query Context:");
            summary.AddRange(_contextElements.Select(e => $"  - {e}"));
        }
        
        if (_tableContexts.Any())
        {
            summary.Add("Relevant Tables:");
            summary.AddRange(_tableContexts.Select(t => $"  - {t}"));
        }
        
        if (_glossaryContexts.Any())
        {
            summary.Add("Business Definitions:");
            summary.AddRange(_glossaryContexts.Take(5).Select(g => $"  - {g}"));
        }
        
        return string.Join("\n", summary);
    }
}

/// <summary>
/// Semantic layer enhancement configuration
/// </summary>
public class SemanticLayerConfiguration
{
    public double DefaultRelevanceThreshold { get; set; } = 0.7;
    public int MaxTablesPerQuery { get; set; } = 10;
    public int MaxColumnsPerTable { get; set; } = 20;
    public bool EnableVectorSearch { get; set; } = true;
    public bool EnableBusinessGlossaryEnrichment { get; set; } = true;
    public bool EnableLLMContextOptimization { get; set; } = true;
    public TimeSpan CacheExpiration { get; set; } = TimeSpan.FromHours(2);
}

/// <summary>
/// Semantic enrichment request
/// </summary>
public class SemanticEnrichmentRequest
{
    [Required]
    public string Query { get; set; } = string.Empty;
    
    public double RelevanceThreshold { get; set; } = 0.7;
    public int MaxTables { get; set; } = 10;
    public bool IncludeBusinessGlossary { get; set; } = true;
    public bool OptimizeForLLM { get; set; } = true;
    public List<string> PreferredDomains { get; set; } = new();
    public List<string> ExcludedTables { get; set; } = new();
}

/// <summary>
/// Semantic enrichment response
/// </summary>
public class SemanticEnrichmentResponse
{
    public bool Success { get; set; } = false;
    public string Message { get; set; } = string.Empty;
    public EnhancedSchemaResult? Result { get; set; }
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}
