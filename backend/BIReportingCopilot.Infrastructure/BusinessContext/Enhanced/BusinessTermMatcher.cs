using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;
using System.Text.RegularExpressions;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Enhanced business term matcher that finds similar business terms using multiple matching strategies
/// </summary>
public class BusinessTermMatcher : IBusinessTermMatcher
{
    private readonly IBusinessMetadataRetrievalService _metadataService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<BusinessTermMatcher> _logger;

    // Common business term synonyms
    private static readonly Dictionary<string, string[]> BusinessSynonyms = new()
    {
        ["revenue"] = new[] { "sales", "income", "earnings", "turnover" },
        ["profit"] = new[] { "margin", "earnings", "net income", "bottom line" },
        ["customer"] = new[] { "client", "user", "account", "buyer" },
        ["product"] = new[] { "item", "sku", "merchandise", "goods" },
        ["region"] = new[] { "territory", "area", "zone", "market" },
        ["quarter"] = new[] { "q1", "q2", "q3", "q4", "quarterly" },
        ["month"] = new[] { "monthly", "period", "month-end" },
        ["year"] = new[] { "annual", "yearly", "year-end", "ytd" },
        ["performance"] = new[] { "metrics", "kpi", "results", "achievement" },
        ["analysis"] = new[] { "report", "analytics", "insights", "review" }
    };

    public BusinessTermMatcher(
        IBusinessMetadataRetrievalService metadataService,
        ICacheService cacheService,
        ILogger<BusinessTermMatcher> logger)
    {
        _metadataService = metadataService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<List<(string term, double similarity)>> FindSimilarTermsAsync(string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<(string, double)>();

            var cacheKey = $"similar_terms:{searchTerm.ToLower()}";
            var cached = await _cacheService.GetAsync<List<(string, double)>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var results = new List<(string term, double similarity)>();

            // 1. Exact matches from business glossary
            var exactMatches = await FindExactGlossaryMatches(searchTerm);
            results.AddRange(exactMatches);

            // 2. Synonym matches
            var synonymMatches = FindSynonymMatches(searchTerm);
            results.AddRange(synonymMatches);

            // 3. Fuzzy matches from business metadata
            var fuzzyMatches = await FindFuzzyMetadataMatches(searchTerm);
            results.AddRange(fuzzyMatches);

            // 4. Pattern-based matches
            var patternMatches = await FindPatternMatches(searchTerm);
            results.AddRange(patternMatches);

            // Deduplicate and sort by similarity
            var finalResults = results
                .GroupBy(r => r.term.ToLower())
                .Select(g => (g.Key, g.Max(x => x.similarity)))
                .Where(r => r.Item2 > 0.3) // Filter low similarity matches
                .OrderByDescending(r => r.Item2)
                .Take(10)
                .ToList();

            await _cacheService.SetAsync(cacheKey, finalResults, TimeSpan.FromMinutes(30));
            
            _logger.LogDebug("Found {Count} similar terms for '{SearchTerm}'", finalResults.Count, searchTerm);

            return finalResults;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar terms for: {SearchTerm}", searchTerm);
            return new List<(string, double)>();
        }
    }

    private async Task<List<(string term, double similarity)>> FindExactGlossaryMatches(string searchTerm)
    {
        try
        {
            var businessTerms = new List<string> { searchTerm };
            var glossaryTerms = await _metadataService.FindRelevantGlossaryTermsAsync(businessTerms);
            
            var matches = new List<(string, double)>();
            
            foreach (var term in glossaryTerms)
            {
                // Exact match
                if (string.Equals(term.Term, searchTerm, StringComparison.OrdinalIgnoreCase))
                {
                    matches.Add((term.Term, 1.0));
                }
                // Synonym match
                else if (term.Synonyms?.Any(s => string.Equals(s, searchTerm, StringComparison.OrdinalIgnoreCase)) == true)
                {
                    matches.Add((term.Term, 0.9));
                }
                // Related term match
                else if (term.RelatedTerms?.Any(r => string.Equals(r, searchTerm, StringComparison.OrdinalIgnoreCase)) == true)
                {
                    matches.Add((term.Term, 0.8));
                }
                // Partial match in definition
                else if (term.Definition?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)
                {
                    matches.Add((term.Term, 0.6));
                }
            }
            
            return matches;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error finding exact glossary matches");
            return new List<(string, double)>();
        }
    }

    private List<(string term, double similarity)> FindSynonymMatches(string searchTerm)
    {
        var matches = new List<(string, double)>();
        var lowerSearchTerm = searchTerm.ToLower();

        foreach (var (mainTerm, synonyms) in BusinessSynonyms)
        {
            // Check if search term is the main term
            if (mainTerm.Equals(lowerSearchTerm, StringComparison.OrdinalIgnoreCase))
            {
                matches.AddRange(synonyms.Select(s => (s, 0.85)));
            }
            // Check if search term is a synonym
            else if (synonyms.Any(s => s.Equals(lowerSearchTerm, StringComparison.OrdinalIgnoreCase)))
            {
                matches.Add((mainTerm, 0.85));
                matches.AddRange(synonyms.Where(s => !s.Equals(lowerSearchTerm, StringComparison.OrdinalIgnoreCase))
                    .Select(s => (s, 0.8)));
            }
        }

        return matches;
    }

    private async Task<List<(string term, double similarity)>> FindFuzzyMetadataMatches(string searchTerm)
    {
        try
        {
            var matches = new List<(string, double)>();
            
            // Create a dummy profile for metadata search
            var dummyProfile = new Core.Models.BusinessContext.BusinessContextProfile
            {
                AnalysisId = Guid.NewGuid().ToString(),
                OriginalQuestion = searchTerm,
                UserId = "system",
                BusinessTerms = new List<string> { searchTerm }
            };
            
            // Search business tables for similar terms
            var tables = await _metadataService.FindRelevantTablesAsync(dummyProfile, 5);
            
            foreach (var table in tables)
            {
                var similarity = CalculateStringSimilarity(searchTerm, table.BusinessName ?? table.TableName);
                if (similarity > 0.5)
                {
                    matches.Add((table.BusinessName ?? table.TableName, similarity));
                }
            }
            
            return matches;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error finding fuzzy metadata matches");
            return new List<(string, double)>();
        }
    }

    private async Task<List<(string term, double similarity)>> FindPatternMatches(string searchTerm)
    {
        var matches = new List<(string, double)>();
        
        try
        {
            // Pattern-based matching for common business term variations
            var patterns = new[]
            {
                $@"\b{Regex.Escape(searchTerm)}\w*\b", // Terms starting with search term
                $@"\b\w*{Regex.Escape(searchTerm)}\b", // Terms ending with search term
                $@"\b\w*{Regex.Escape(searchTerm)}\w*\b" // Terms containing search term
            };

            // This would typically search through a larger corpus of business terms
            // For now, we'll use a simplified approach with common business terms
            var commonTerms = new[]
            {
                "revenue", "sales", "profit", "margin", "cost", "expense", "budget", "forecast",
                "customer", "client", "user", "account", "lead", "prospect",
                "product", "service", "item", "sku", "category", "brand",
                "quarter", "month", "year", "period", "date", "time",
                "region", "territory", "location", "country", "state", "city",
                "department", "division", "team", "group", "unit", "segment",
                "performance", "metric", "kpi", "indicator", "measure", "target",
                "analysis", "report", "dashboard", "chart", "graph", "trend"
            };

            foreach (var term in commonTerms)
            {
                var similarity = CalculateStringSimilarity(searchTerm, term);
                if (similarity > 0.6)
                {
                    matches.Add((term, similarity));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error finding pattern matches");
        }

        return matches;
    }

    private double CalculateStringSimilarity(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            return 0.0;

        str1 = str1.ToLower();
        str2 = str2.ToLower();

        if (str1 == str2)
            return 1.0;

        // Simple Levenshtein distance-based similarity
        var distance = CalculateLevenshteinDistance(str1, str2);
        var maxLength = Math.Max(str1.Length, str2.Length);
        
        return maxLength == 0 ? 1.0 : 1.0 - (double)distance / maxLength;
    }

    private int CalculateLevenshteinDistance(string str1, string str2)
    {
        var matrix = new int[str1.Length + 1, str2.Length + 1];

        for (int i = 0; i <= str1.Length; i++)
            matrix[i, 0] = i;

        for (int j = 0; j <= str2.Length; j++)
            matrix[0, j] = j;

        for (int i = 1; i <= str1.Length; i++)
        {
            for (int j = 1; j <= str2.Length; j++)
            {
                var cost = str1[i - 1] == str2[j - 1] ? 0 : 1;
                matrix[i, j] = Math.Min(
                    Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                    matrix[i - 1, j - 1] + cost);
            }
        }

        return matrix[str1.Length, str2.Length];
    }
}
