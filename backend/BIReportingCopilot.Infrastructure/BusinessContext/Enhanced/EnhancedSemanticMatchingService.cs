using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Extensions;
using BIReportingCopilot.Infrastructure.AI.Enhanced;
using BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Enhanced semantic matching service that replaces all mock implementations with real vector embeddings
/// </summary>
public class EnhancedSemanticMatchingService : IEnhancedSemanticMatchingService
{
    private readonly IRealVectorEmbeddingService _embeddingService;
    private readonly IDynamicThresholdOptimizer _thresholdOptimizer;
    private readonly IContextualScoringEngine _scoringEngine;
    private readonly ICacheService _cacheService;
    private readonly IBusinessTableService _tableService;
    private readonly IBusinessColumnService _columnService;
    private readonly ILogger<EnhancedSemanticMatchingService> _logger;

    // Pre-computed embeddings cache for frequently accessed tables/columns
    private readonly Dictionary<string, float[]> _precomputedEmbeddings = new();
    private readonly object _embeddingsLock = new();

    public EnhancedSemanticMatchingService(
        IRealVectorEmbeddingService embeddingService,
        IDynamicThresholdOptimizer thresholdOptimizer,
        IContextualScoringEngine scoringEngine,
        ICacheService cacheService,
        IBusinessTableService tableService,
        IBusinessColumnService columnService,
        ILogger<EnhancedSemanticMatchingService> logger)
    {
        _embeddingService = embeddingService;
        _thresholdOptimizer = thresholdOptimizer;
        _scoringEngine = scoringEngine;
        _cacheService = cacheService;
        _tableService = tableService;
        _columnService = columnService;
        _logger = logger;
    }

    public async Task<List<BusinessTableInfoDto>> SemanticTableSearchAsync(
        string query, 
        BusinessContextProfile profile,
        int topK = 5)
    {
        var cacheKey = $"semantic_table_search:{query.GetHashCode()}:{profile.Intent.Type}:{topK}";
        var (found, cachedResults) = await _cacheService.TryGetAsync<List<BusinessTableInfoDto>>(cacheKey);
        if (found && cachedResults != null)
        {
            _logger.LogDebug("Retrieved cached semantic table search results");
            return cachedResults;
        }

        _logger.LogInformation("Performing semantic table search for query: {Query}", 
            query.Substring(0, Math.Min(100, query.Length)));

        var startTime = DateTime.UtcNow;

        try
        {
            // Get all available tables
            var allTables = await _tableService.GetAllBusinessTablesAsync();
            if (!allTables.Any())
            {
                _logger.LogWarning("No business tables available for semantic search");
                return new List<BusinessTableInfoDto>();
            }

            // Generate query embedding
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);

            // Get or generate table embeddings
            var tableEmbeddings = await GetTableEmbeddingsAsync(allTables);

            // Calculate contextual similarities
            var similarities = await CalculateContextualTableSimilaritiesAsync(
                queryEmbedding, allTables, tableEmbeddings, profile);

            // Apply dynamic threshold
            var threshold = await _thresholdOptimizer.GetOptimalThresholdAsync(
                profile.Intent.Type, profile.Domain.Name, "table_search");

            // Filter and rank results
            var results = similarities
                .Where(s => s.Score > threshold)
                .OrderByDescending(s => s.Score)
                .Take(topK)
                .Select(s => s.Table)
                .ToList();

            // Cache results for 1 hour
            await _cacheService.SetAsync(cacheKey, results, TimeSpan.FromHours(1));

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogInformation("Semantic table search completed in {Duration}ms, found {Count} results above threshold {Threshold:F3}", 
                duration, results.Count, threshold);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in semantic table search");
            return new List<BusinessTableInfoDto>();
        }
    }

    public async Task<List<BusinessColumnInfo>> SemanticColumnSearchAsync(
        string query,
        BusinessContextProfile profile,
        List<long> tableIds,
        int topK = 10)
    {
        var cacheKey = $"semantic_column_search:{query.GetHashCode()}:{string.Join(",", tableIds)}:{topK}";
        var (found, cachedResults) = await _cacheService.TryGetAsync<List<BusinessColumnInfo>>(cacheKey);
        if (found && cachedResults != null)
        {
            _logger.LogDebug("Retrieved cached semantic column search results");
            return cachedResults;
        }

        _logger.LogInformation("Performing semantic column search for {TableCount} tables", tableIds.Count);

        try
        {
            // Get all columns for specified tables
            var allColumns = new List<BusinessColumnInfo>();
            foreach (var tableId in tableIds)
            {
                var tableColumns = await _columnService.GetColumnsByTableIdAsync(tableId);
                allColumns.AddRange(tableColumns);
            }

            if (!allColumns.Any())
            {
                return new List<BusinessColumnInfo>();
            }

            // Generate query embedding
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);

            // Get or generate column embeddings
            var columnEmbeddings = await GetColumnEmbeddingsAsync(allColumns);

            // Calculate contextual similarities
            var similarities = await CalculateContextualColumnSimilaritiesAsync(
                queryEmbedding, allColumns, columnEmbeddings, profile);

            // Apply dynamic threshold
            var threshold = await _thresholdOptimizer.GetOptimalThresholdAsync(
                profile.Intent.Type, profile.Domain.Name, "column_search");

            // Filter and rank results
            var results = similarities
                .Where(s => s.Score > threshold)
                .OrderByDescending(s => s.Score)
                .Take(topK)
                .Select(s => s.Column)
                .ToList();

            // Cache results for 30 minutes
            await _cacheService.SetAsync(cacheKey, results, TimeSpan.FromMinutes(30));

            _logger.LogInformation("Semantic column search found {Count} results above threshold {Threshold:F3}", 
                results.Count, threshold);

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in semantic column search");
            return new List<BusinessColumnInfo>();
        }
    }

    public async Task<double> CalculateSemanticSimilarityAsync(string text1, string text2)
    {
        if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2))
        {
            return 0.0;
        }

        var cacheKey = $"similarity:{text1.GetHashCode()}:{text2.GetHashCode()}";
        var (found, cachedSimilarity) = await _cacheService.TryGetValueAsync<double>(cacheKey);
        if (found)
        {
            return cachedSimilarity;
        }

        try
        {
            var embeddings = await _embeddingService.GenerateBatchEmbeddingsAsync(new List<string> { text1, text2 });
            var similarity = _embeddingService.CalculateCosineSimilarity(embeddings[0], embeddings[1]);

            // Cache for 2 hours
            await _cacheService.SetAsync(cacheKey, similarity, TimeSpan.FromHours(2));

            return similarity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating semantic similarity");
            return 0.0;
        }
    }

    public async Task<List<(string term, double score)>> FindSimilarTermsAsync(string searchTerm, int topK = 5)
    {
        try
        {
            // Get business terms from glossary or predefined list
            var businessTerms = await GetBusinessTermsAsync();
            
            if (!businessTerms.Any())
            {
                return new List<(string, double)>();
            }

            // Find most similar terms using embeddings
            var similarities = await _embeddingService.FindMostSimilarAsync(
                searchTerm, businessTerms, topK);

            return similarities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar terms for: {SearchTerm}", searchTerm);
            return new List<(string, double)>();
        }
    }

    private async Task<Dictionary<long, float[]>> GetTableEmbeddingsAsync(List<BusinessTableInfoDto> tables)
    {
        var embeddings = new Dictionary<long, float[]>();
        var textsToEmbed = new List<(long id, string text)>();

        foreach (var table in tables)
        {
            var embeddingKey = $"table_embedding:{table.Id}";
            
            lock (_embeddingsLock)
            {
                if (_precomputedEmbeddings.TryGetValue(embeddingKey, out var cachedEmbedding))
                {
                    embeddings[table.Id] = cachedEmbedding;
                    continue;
                }
            }

            // Check persistent cache
            var (found, persistentEmbedding) = await _cacheService.TryGetAsync<float[]>(embeddingKey);
            if (found && persistentEmbedding != null)
            {
                embeddings[table.Id] = persistentEmbedding;

                lock (_embeddingsLock)
                {
                    _precomputedEmbeddings[embeddingKey] = persistentEmbedding;
                }
                continue;
            }

            // Need to generate embedding
            var tableText = BuildTableSearchText(table);
            textsToEmbed.Add((table.Id, tableText));
        }

        // Generate embeddings for tables that don't have them
        if (textsToEmbed.Any())
        {
            var newEmbeddings = await _embeddingService.GenerateBatchEmbeddingsAsync(
                textsToEmbed.Select(t => t.text).ToList());

            for (int i = 0; i < textsToEmbed.Count; i++)
            {
                var (id, _) = textsToEmbed[i];
                var embedding = newEmbeddings[i];
                embeddings[id] = embedding;

                // Cache the embedding
                var embeddingKey = $"table_embedding:{id}";
                await _cacheService.SetAsync(embeddingKey, embedding, TimeSpan.FromDays(7));
                
                lock (_embeddingsLock)
                {
                    _precomputedEmbeddings[embeddingKey] = embedding;
                }
            }
        }

        return embeddings;
    }

    private async Task<Dictionary<long, float[]>> GetColumnEmbeddingsAsync(List<BusinessColumnInfo> columns)
    {
        var embeddings = new Dictionary<long, float[]>();
        var textsToEmbed = new List<(long id, string text)>();

        foreach (var column in columns)
        {
            var embeddingKey = $"column_embedding:{column.Id}";
            
            var (found, cachedEmbedding) = await _cacheService.TryGetAsync<float[]>(embeddingKey);
            if (found && cachedEmbedding != null)
            {
                embeddings[column.Id] = cachedEmbedding;
                continue;
            }

            var columnText = BuildColumnSearchText(column);
            textsToEmbed.Add((column.Id, columnText));
        }

        if (textsToEmbed.Any())
        {
            var newEmbeddings = await _embeddingService.GenerateBatchEmbeddingsAsync(
                textsToEmbed.Select(t => t.text).ToList());

            for (int i = 0; i < textsToEmbed.Count; i++)
            {
                var (id, _) = textsToEmbed[i];
                var embedding = newEmbeddings[i];
                embeddings[id] = embedding;

                var embeddingKey = $"column_embedding:{id}";
                await _cacheService.SetAsync(embeddingKey, embedding, TimeSpan.FromDays(7));
            }
        }

        return embeddings;
    }

    private async Task<List<(BusinessTableInfoDto Table, double Score)>> CalculateContextualTableSimilaritiesAsync(
        float[] queryEmbedding,
        List<BusinessTableInfoDto> tables,
        Dictionary<long, float[]> tableEmbeddings,
        BusinessContextProfile profile)
    {
        var similarities = new List<(BusinessTableInfoDto Table, double Score)>();

        foreach (var table in tables)
        {
            if (!tableEmbeddings.TryGetValue(table.Id, out var tableEmbedding))
            {
                continue;
            }

            // Calculate base semantic similarity
            var baseSimilarity = _embeddingService.CalculateCosineSimilarity(queryEmbedding, tableEmbedding);

            // Apply contextual scoring
            var contextualScore = await _scoringEngine.ApplyContextualAdjustmentsAsync(
                baseSimilarity, table, profile);

            similarities.Add((table, contextualScore));
        }

        return similarities;
    }

    private async Task<List<(BusinessColumnInfo Column, double Score)>> CalculateContextualColumnSimilaritiesAsync(
        float[] queryEmbedding,
        List<BusinessColumnInfo> columns,
        Dictionary<long, float[]> columnEmbeddings,
        BusinessContextProfile profile)
    {
        var similarities = new List<(BusinessColumnInfo Column, double Score)>();

        foreach (var column in columns)
        {
            if (!columnEmbeddings.TryGetValue(column.Id, out var columnEmbedding))
            {
                continue;
            }

            var baseSimilarity = _embeddingService.CalculateCosineSimilarity(queryEmbedding, columnEmbedding);
            var contextualScore = await _scoringEngine.ApplyColumnContextualAdjustmentsAsync(
                baseSimilarity, column, profile);

            similarities.Add((column, contextualScore));
        }

        return similarities;
    }

    private string BuildTableSearchText(BusinessTableInfoDto table)
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrEmpty(table.BusinessPurpose))
            parts.Add(table.BusinessPurpose);
        
        if (!string.IsNullOrEmpty(table.BusinessContext))
            parts.Add(table.BusinessContext);
        
        if (!string.IsNullOrEmpty(table.PrimaryUseCase))
            parts.Add(table.PrimaryUseCase);
        
        if (!string.IsNullOrEmpty(table.DomainClassification))
            parts.Add(table.DomainClassification);
        
        parts.Add($"Table: {table.SchemaName}.{table.TableName}");
        
        return string.Join(" ", parts);
    }

    private string BuildColumnSearchText(BusinessColumnInfo column)
    {
        var parts = new List<string>();
        
        if (!string.IsNullOrEmpty(column.BusinessMeaning))
            parts.Add(column.BusinessMeaning);
        
        if (!string.IsNullOrEmpty(column.BusinessContext))
            parts.Add(column.BusinessContext);
        
        if (!string.IsNullOrEmpty(column.SemanticContext))
            parts.Add(column.SemanticContext);
        
        parts.Add($"Column: {column.ColumnName} ({column.DataType})");
        
        return string.Join(" ", parts);
    }

    private async Task<List<string>> GetBusinessTermsAsync()
    {
        // This would typically come from a business glossary or predefined terms
        return new List<string>
        {
            "revenue", "profit", "sales", "customers", "users", "players", "sessions",
            "conversion", "retention", "churn", "engagement", "acquisition", "monetization",
            "analytics", "metrics", "kpi", "dashboard", "report", "insights", "trends",
            "segmentation", "cohort", "funnel", "attribution", "lifetime value", "arpu"
        };
    }
}

public interface IEnhancedSemanticMatchingService
{
    Task<List<BusinessTableInfoDto>> SemanticTableSearchAsync(string query, BusinessContextProfile profile, int topK = 5);
    Task<List<BusinessColumnInfo>> SemanticColumnSearchAsync(string query, BusinessContextProfile profile, List<long> tableIds, int topK = 10);
    Task<double> CalculateSemanticSimilarityAsync(string text1, string text2);
    Task<List<(string term, double score)>> FindSimilarTermsAsync(string searchTerm, int topK = 5);
}

public interface IDynamicThresholdOptimizer
{
    Task<double> GetOptimalThresholdAsync(IntentType intentType, string domainName, string searchType);
}

public interface IContextualScoringEngine
{
    Task<double> ApplyContextualAdjustmentsAsync(double baseSimilarity, BusinessTableInfoDto table, BusinessContextProfile profile);
    Task<double> ApplyColumnContextualAdjustmentsAsync(double baseSimilarity, BusinessColumnInfo column, BusinessContextProfile profile);
}

public interface IBusinessTableService
{
    Task<List<BusinessTableInfoDto>> GetAllBusinessTablesAsync();
}

public interface IBusinessColumnService
{
    Task<List<BusinessColumnInfo>> GetColumnsByTableIdAsync(long tableId);
}
