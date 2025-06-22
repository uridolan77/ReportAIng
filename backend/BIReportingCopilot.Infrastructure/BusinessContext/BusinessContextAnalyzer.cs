using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.DTOs;

namespace BIReportingCopilot.Infrastructure.BusinessContext;

/// <summary>
/// Service for analyzing business context from natural language queries
/// </summary>
public class BusinessContextAnalyzer : IBusinessContextAnalyzer
{
    private readonly IAIService _aiService;
    private readonly ISemanticMatchingService _semanticMatchingService;
    private readonly ILogger<BusinessContextAnalyzer> _logger;
    private readonly IMemoryCache _cache;

    public BusinessContextAnalyzer(
        IAIService aiService,
        ISemanticMatchingService semanticMatchingService,
        ILogger<BusinessContextAnalyzer> logger,
        IMemoryCache cache)
    {
        _aiService = aiService;
        _semanticMatchingService = semanticMatchingService;
        _logger = logger;
        _cache = cache;
    }

    public async Task<BusinessContextProfile> AnalyzeUserQuestionAsync(
        string userQuestion, 
        string? userId = null)
    {
        _logger.LogInformation("Analyzing user question: {Question}", userQuestion);

        // Check cache
        var cacheKey = $"context_profile_{userQuestion.GetHashCode()}_{userId}";
        if (_cache.TryGetValue<BusinessContextProfile>(cacheKey, out var cachedProfile))
        {
            return cachedProfile!;
        }

        // Parallel analysis tasks
        var intentTask = ClassifyBusinessIntentAsync(userQuestion);
        var domainTask = DetectBusinessDomainAsync(userQuestion);
        var entitiesTask = ExtractBusinessEntitiesAsync(userQuestion);
        var termsTask = ExtractBusinessTermsAsync(userQuestion);
        var timeTask = ExtractTimeContextAsync(userQuestion);

        await Task.WhenAll(intentTask, domainTask, entitiesTask, termsTask, timeTask);

        var profile = new BusinessContextProfile
        {
            OriginalQuestion = userQuestion,
            UserId = userId ?? string.Empty,
            Intent = await intentTask,
            Domain = await domainTask,
            Entities = await entitiesTask,
            BusinessTerms = await termsTask,
            TimeContext = await timeTask,
            ConfidenceScore = CalculateOverallConfidence(
                await intentTask, 
                await domainTask, 
                await entitiesTask)
        };

        // Extract metrics and dimensions
        profile.IdentifiedMetrics = ExtractMetrics(profile.Entities);
        profile.IdentifiedDimensions = ExtractDimensions(profile.Entities);
        profile.ComparisonTerms = ExtractComparisonTerms(userQuestion);

        // Cache the result
        _cache.Set(cacheKey, profile, TimeSpan.FromMinutes(30));

        return profile;
    }

    public async Task<QueryIntent> ClassifyBusinessIntentAsync(string userQuestion)
    {
        var prompt = $@"
Classify the following business question into one of these intent types:
- Analytical: Complex analysis requiring aggregations, calculations
- Operational: Transactional or operational data queries
- Exploratory: Discovery queries to understand data
- Comparison: Comparing values across dimensions
- Aggregation: Summary or rollup queries
- Trend: Time-series or trend analysis
- Detail: Drill-down to detailed records

Question: {userQuestion}

Respond in JSON format:
{{
    ""type"": ""<intent_type>"",
    ""description"": ""<brief description>"",
    ""confidence"": <0.0-1.0>,
    ""subIntents"": [""<optional sub-intents>""]
}}";

        try
        {
            var response = await _aiService.GenerateSQLAsync(prompt);
            return ParseQueryIntent(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying business intent");
            return new QueryIntent
            {
                Type = IntentType.Unknown,
                Description = "Unable to classify intent",
                ConfidenceScore = 0.0
            };
        }
    }

    public async Task<BusinessDomain> DetectBusinessDomainAsync(string userQuestion)
    {
        // Get all active domains from database (this would be implemented)
        var domains = await GetActiveDomainsAsync();
        
        // Use semantic matching to find best domain
        var domainDescriptions = domains.Select(d => 
            $"{d.Name}: {d.Description} - Key concepts: {string.Join(", ", d.KeyConcepts)}"
        ).ToList();

        var bestMatch = await FindBestDomainMatchAsync(userQuestion, domainDescriptions);
        
        return new BusinessDomain
        {
            Name = bestMatch.domainName,
            Description = bestMatch.description,
            RelatedTables = bestMatch.relatedTables,
            KeyConcepts = bestMatch.keyConcepts,
            RelevanceScore = bestMatch.score
        };
    }

    public async Task<List<BusinessEntity>> ExtractBusinessEntitiesAsync(string userQuestion)
    {
        var prompt = $@"
Extract business entities from this question. Identify:
- Table references (explicit or implied)
- Column/field references
- Metrics (measures, KPIs)
- Dimensions (grouping attributes)
- Time references
- Comparison values

Question: {userQuestion}

Respond in JSON format with an array of entities:
[
    {{
        ""name"": ""<entity name>"",
        ""type"": ""<Table|Column|Metric|Dimension|TimeReference|ComparisonValue>"",
        ""originalText"": ""<text from question>"",
        ""confidence"": <0.0-1.0>
    }}
]";

        try
        {
            var response = await _aiService.GenerateSQLAsync(prompt);
            var entities = ParseBusinessEntities(response);

            // Enhance entities with semantic matching
            foreach (var entity in entities)
            {
                await EnhanceEntityWithSemanticMatchingAsync(entity);
            }

            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting business entities");
            return new List<BusinessEntity>();
        }
    }

    public async Task<List<string>> ExtractBusinessTermsAsync(string userQuestion)
    {
        // Extract potential business terms using simple keyword extraction
        var terms = await ExtractPotentialTermsAsync(userQuestion);
        
        // Match against semantic search
        var matchedTerms = new List<string>();

        foreach (var term in terms)
        {
            var matches = await _semanticMatchingService.FindSimilarTermsAsync(term, 3);
            if (matches.Any(m => m.score > 0.8))
            {
                matchedTerms.Add(term);
            }
        }

        return matchedTerms.Distinct().ToList();
    }

    public async Task<TimeRange?> ExtractTimeContextAsync(string userQuestion)
    {
        var prompt = $@"
Extract time context from this question. Look for:
- Specific dates
- Relative time expressions (last month, year to date, etc.)
- Time comparisons
- Time granularity

Question: {userQuestion}

If no time context found, return null.
Otherwise return JSON:
{{
    ""startDate"": ""<ISO date or null>"",
    ""endDate"": ""<ISO date or null>"",
    ""relativeExpression"": ""<expression or empty>"",
    ""granularity"": ""<Hour|Day|Week|Month|Quarter|Year|Unknown>""
}}";

        try
        {
            var response = await _aiService.GenerateSQLAsync(prompt);
            return ParseTimeRange(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting time context");
            return null;
        }
    }

    // Helper methods
    private double CalculateOverallConfidence(
        QueryIntent intent, 
        BusinessDomain domain, 
        List<BusinessEntity> entities)
    {
        var weights = new { Intent = 0.3, Domain = 0.3, Entities = 0.4 };
        var entityConfidence = entities.Any() 
            ? entities.Average(e => e.ConfidenceScore) 
            : 0.5;

        return (intent.ConfidenceScore * weights.Intent) +
               (domain.RelevanceScore * weights.Domain) +
               (entityConfidence * weights.Entities);
    }

    private List<string> ExtractMetrics(List<BusinessEntity> entities)
    {
        return entities
            .Where(e => e.Type == EntityType.Metric)
            .Select(e => e.Name)
            .ToList();
    }

    private List<string> ExtractDimensions(List<BusinessEntity> entities)
    {
        return entities
            .Where(e => e.Type == EntityType.Dimension)
            .Select(e => e.Name)
            .ToList();
    }

    private List<string> ExtractComparisonTerms(string userQuestion)
    {
        var comparisonKeywords = new[] 
        { 
            "vs", "versus", "compared to", "compare", "difference between",
            "higher than", "lower than", "better than", "worse than"
        };

        return comparisonKeywords
            .Where(keyword => userQuestion.ToLower().Contains(keyword))
            .ToList();
    }

    // Parsing methods (simplified implementations)
    private QueryIntent ParseQueryIntent(string response)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement>(response);
            return new QueryIntent
            {
                Type = Enum.Parse<IntentType>(json.GetProperty("type").GetString() ?? "Unknown", true),
                Description = json.GetProperty("description").GetString() ?? "",
                ConfidenceScore = json.GetProperty("confidence").GetDouble(),
                SubIntents = json.TryGetProperty("subIntents", out var subIntents) 
                    ? subIntents.EnumerateArray().Select(x => x.GetString() ?? "").ToList()
                    : new List<string>()
            };
        }
        catch
        {
            return new QueryIntent { Type = IntentType.Unknown, ConfidenceScore = 0.0 };
        }
    }

    private List<BusinessEntity> ParseBusinessEntities(string response)
    {
        try
        {
            var json = JsonSerializer.Deserialize<JsonElement[]>(response);
            return json.Select(item => new BusinessEntity
            {
                Name = item.GetProperty("name").GetString() ?? "",
                Type = Enum.Parse<EntityType>(item.GetProperty("type").GetString() ?? "Unknown", true),
                OriginalText = item.GetProperty("originalText").GetString() ?? "",
                ConfidenceScore = item.GetProperty("confidence").GetDouble()
            }).ToList();
        }
        catch
        {
            return new List<BusinessEntity>();
        }
    }

    private TimeRange? ParseTimeRange(string response)
    {
        try
        {
            if (response.Trim().ToLower() == "null") return null;
            
            var json = JsonSerializer.Deserialize<JsonElement>(response);
            return new TimeRange
            {
                StartDate = DateTime.TryParse(json.GetProperty("startDate").GetString(), out var start) ? start : null,
                EndDate = DateTime.TryParse(json.GetProperty("endDate").GetString(), out var end) ? end : null,
                RelativeExpression = json.GetProperty("relativeExpression").GetString() ?? "",
                Granularity = Enum.Parse<TimeGranularity>(json.GetProperty("granularity").GetString() ?? "Unknown", true)
            };
        }
        catch
        {
            return null;
        }
    }

    // Placeholder methods that would be implemented
    private async Task<List<BusinessDomain>> GetActiveDomainsAsync()
    {
        // This would query the database for active business domains
        await Task.CompletedTask;
        return new List<BusinessDomain>
        {
            new() { Name = "Gaming", Description = "Gaming analytics and player behavior", KeyConcepts = new[] { "players", "games", "revenue", "sessions" }.ToList() },
            new() { Name = "Finance", Description = "Financial data and transactions", KeyConcepts = new[] { "revenue", "costs", "profit", "transactions" }.ToList() }
        };
    }

    private async Task<(string domainName, string description, List<string> relatedTables, List<string> keyConcepts, double score)> FindBestDomainMatchAsync(
        string userQuestion, List<string> domainDescriptions)
    {
        // This would use semantic matching to find the best domain
        await Task.CompletedTask;
        return ("Gaming", "Gaming analytics domain", new List<string> { "tbl_Daily_actions" }, new List<string> { "players", "games" }, 0.8);
    }

    private async Task<List<string>> ExtractPotentialTermsAsync(string userQuestion)
    {
        // Simple keyword extraction - would be enhanced with NLP
        await Task.CompletedTask;
        var words = userQuestion.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Where(w => w.Length > 3).ToList();
    }

    private async Task EnhanceEntityWithSemanticMatchingAsync(BusinessEntity entity)
    {
        // This would enhance entities with database mappings
        await Task.CompletedTask;
    }
}
