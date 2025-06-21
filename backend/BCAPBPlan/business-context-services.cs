// Application/Services/Interfaces/IBusinessContextAnalyzer.cs
namespace ReportAIng.Application.Services.Interfaces
{
    public interface IBusinessContextAnalyzer
    {
        Task<BusinessContextProfile> AnalyzeUserQuestionAsync(string userQuestion, string? userId = null);
        Task<QueryIntent> ClassifyBusinessIntentAsync(string userQuestion);
        Task<BusinessDomain> DetectBusinessDomainAsync(string userQuestion);
        Task<List<BusinessEntity>> ExtractBusinessEntitiesAsync(string userQuestion);
        Task<List<string>> ExtractBusinessTermsAsync(string userQuestion);
        Task<TimeRange?> ExtractTimeContextAsync(string userQuestion);
    }

    public interface IBusinessMetadataRetrievalService
    {
        Task<ContextualBusinessSchema> GetRelevantBusinessMetadataAsync(
            BusinessContextProfile profile,
            int maxTables = 5);
        Task<List<BusinessTableInfoDto>> FindRelevantTablesAsync(
            BusinessContextProfile profile, 
            int maxTables = 5);
        Task<List<BusinessColumnInfoDto>> FindRelevantColumnsAsync(
            List<long> tableIds, 
            BusinessContextProfile profile);
        Task<List<BusinessGlossaryDto>> FindRelevantGlossaryTermsAsync(
            List<string> businessTerms);
        Task<List<TableRelationship>> DiscoverTableRelationshipsAsync(
            List<string> tableNames);
    }

    public interface IContextualPromptBuilder
    {
        Task<string> BuildBusinessAwarePromptAsync(
            string userQuestion, 
            BusinessContextProfile profile, 
            ContextualBusinessSchema schema);
        Task<PromptTemplate> SelectOptimalTemplateAsync(BusinessContextProfile profile);
        Task<string> EnrichPromptWithBusinessContextAsync(
            string basePrompt, 
            ContextualBusinessSchema schema);
        Task<List<QueryExample>> FindRelevantExamplesAsync(
            BusinessContextProfile profile, 
            int maxExamples = 3);
    }

    public interface ISemanticMatchingService
    {
        Task<List<(string term, double score)>> FindSimilarTermsAsync(
            string searchTerm, 
            int topK = 5);
        Task<double> CalculateSemanticSimilarityAsync(string text1, string text2);
        Task<List<BusinessTableInfoDto>> SemanticTableSearchAsync(
            string query, 
            int topK = 10);
        Task<List<BusinessColumnInfoDto>> SemanticColumnSearchAsync(
            string query, 
            long? tableId = null, 
            int topK = 10);
    }
}

// Application/Services/BusinessContextAnalyzer.cs
namespace ReportAIng.Application.Services
{
    public class BusinessContextAnalyzer : IBusinessContextAnalyzer
    {
        private readonly IOpenAIService _openAIService;
        private readonly IBusinessGlossaryRepository _glossaryRepository;
        private readonly ISemanticMatchingService _semanticMatchingService;
        private readonly ILogger<BusinessContextAnalyzer> _logger;
        private readonly IMemoryCache _cache;

        public BusinessContextAnalyzer(
            IOpenAIService openAIService,
            IBusinessGlossaryRepository glossaryRepository,
            ISemanticMatchingService semanticMatchingService,
            ILogger<BusinessContextAnalyzer> logger,
            IMemoryCache cache)
        {
            _openAIService = openAIService;
            _glossaryRepository = glossaryRepository;
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

            var response = await _openAIService.GetCompletionAsync(prompt);
            return ParseQueryIntent(response);
        }

        public async Task<BusinessDomain> DetectBusinessDomainAsync(string userQuestion)
        {
            // Get all active domains from database
            var domains = await GetActiveDomainsAsync();
            
            // Use semantic matching to find best domain
            var domainDescriptions = domains.Select(d => 
                $"{d.DomainName}: {d.Description} - Key concepts: {string.Join(", ", d.KeyConcepts)}"
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

            var response = await _openAIService.GetCompletionAsync(prompt);
            var entities = ParseBusinessEntities(response);

            // Enhance entities with glossary matching
            foreach (var entity in entities)
            {
                await EnhanceEntityWithGlossaryAsync(entity);
            }

            return entities;
        }

        public async Task<List<string>> ExtractBusinessTermsAsync(string userQuestion)
        {
            // Extract potential business terms
            var terms = await ExtractPotentialTermsAsync(userQuestion);
            
            // Match against glossary
            var glossaryTerms = await _glossaryRepository.GetActiveTermsAsync();
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

            var response = await _openAIService.GetCompletionAsync(prompt);
            return ParseTimeRange(response);
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

        // Additional helper methods would be implemented here...
    }
}