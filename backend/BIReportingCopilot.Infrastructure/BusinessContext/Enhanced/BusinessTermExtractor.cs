using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;
using System.Text.RegularExpressions;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Enhanced business term extractor that identifies business-specific terminology
/// </summary>
public class BusinessTermExtractor : IBusinessTermExtractor
{
    private readonly IAIService _aiService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<BusinessTermExtractor> _logger;

    // Common business term patterns
    private static readonly string[] BusinessTermPatterns = {
        @"\b(?:revenue|sales|profit|margin|cost|expense|budget|forecast)\b",
        @"\b(?:customer|client|user|account|lead|prospect)\b",
        @"\b(?:product|service|item|sku|category|brand)\b",
        @"\b(?:quarter|month|year|period|date|time|daily|weekly|monthly|yearly)\b",
        @"\b(?:region|territory|location|country|state|city|market)\b",
        @"\b(?:department|division|team|group|unit|segment)\b",
        @"\b(?:performance|metric|kpi|indicator|measure|target|goal)\b",
        @"\b(?:analysis|report|dashboard|chart|graph|trend|pattern)\b"
    };

    public BusinessTermExtractor(
        IAIService aiService,
        ICacheService cacheService,
        ILogger<BusinessTermExtractor> logger)
    {
        _aiService = aiService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<List<string>> ExtractBusinessTermsAsync(string userQuestion)
    {
        try
        {
            var cacheKey = $"business_terms:{userQuestion.GetHashCode()}";
            var cached = await _cacheService.GetAsync<List<string>>(cacheKey);
            if (cached != null)
            {
                return cached;
            }

            var terms = new List<string>();

            // 1. Pattern-based extraction
            var patternTerms = ExtractPatternBasedTerms(userQuestion);
            terms.AddRange(patternTerms);

            // 2. AI-based extraction for more sophisticated terms
            var aiTerms = await ExtractAIBasedTermsAsync(userQuestion);
            terms.AddRange(aiTerms);

            // 3. Clean and deduplicate
            var cleanedTerms = CleanAndDeduplicateTerms(terms);

            await _cacheService.SetAsync(cacheKey, cleanedTerms, TimeSpan.FromMinutes(30));
            
            _logger.LogDebug("Extracted {Count} business terms from question: {Question}", 
                cleanedTerms.Count, userQuestion);

            return cleanedTerms;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting business terms from question: {Question}", userQuestion);
            return new List<string>();
        }
    }

    private List<string> ExtractPatternBasedTerms(string userQuestion)
    {
        var terms = new List<string>();
        var lowerQuestion = userQuestion.ToLower();

        foreach (var pattern in BusinessTermPatterns)
        {
            var matches = Regex.Matches(lowerQuestion, pattern, RegexOptions.IgnoreCase);
            foreach (Match match in matches)
            {
                terms.Add(match.Value.Trim());
            }
        }

        return terms;
    }

    private async Task<List<string>> ExtractAIBasedTermsAsync(string userQuestion)
    {
        try
        {
            var prompt = $@"
Extract business terms from this question. Focus on:
- Business metrics and KPIs
- Domain-specific terminology
- Industry jargon
- Technical business concepts
- Organizational terms

Question: {userQuestion}

Return only the terms as a comma-separated list, no explanations:";

            var response = await _aiService.GenerateSQLAsync(prompt);
            
            if (string.IsNullOrWhiteSpace(response))
                return new List<string>();

            return response
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim().ToLower())
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI-based term extraction failed, falling back to pattern matching");
            return new List<string>();
        }
    }

    private List<string> CleanAndDeduplicateTerms(List<string> terms)
    {
        return terms
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToLower())
            .Where(t => t.Length > 2) // Filter out very short terms
            .Distinct()
            .OrderBy(t => t)
            .ToList();
    }
}
