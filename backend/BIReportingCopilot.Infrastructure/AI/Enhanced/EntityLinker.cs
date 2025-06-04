using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Links extracted entities to schema elements and business concepts
/// Provides entity resolution and disambiguation
/// </summary>
public class EntityLinker
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, List<SchemaMapping>> _schemaMappings;
    private readonly Dictionary<string, List<BusinessConcept>> _businessConcepts;

    public EntityLinker(ILogger logger)
    {
        _logger = logger;
        _schemaMappings = InitializeSchemaMappings();
        _businessConcepts = InitializeBusinessConcepts();
    }

    /// <summary>
    /// Link entities to schema elements based on domain context
    /// </summary>
    public async Task<List<SemanticEntity>> LinkToSchemaAsync(
        List<SemanticEntity> entities, 
        string? domain = null)
    {
        var linkedEntities = new List<SemanticEntity>();

        foreach (var entity in entities)
        {
            var linkedEntity = await LinkEntityToSchemaAsync(entity, domain);
            linkedEntities.Add(linkedEntity);
        }

        _logger.LogDebug("Linked {EntityCount} entities to schema elements", linkedEntities.Count);
        return linkedEntities;
    }

    /// <summary>
    /// Link individual entity to schema elements
    /// </summary>
    private async Task<SemanticEntity> LinkEntityToSchemaAsync(SemanticEntity entity, string? domain)
    {
        var linkedEntity = new SemanticEntity
        {
            Text = entity.Text,
            Type = entity.Type,
            Confidence = entity.Confidence,
            StartPosition = entity.StartPosition,
            EndPosition = entity.EndPosition,
            ResolvedValue = entity.ResolvedValue,
            Properties = new Dictionary<string, object>(entity.Properties)
        };

        // Try to link to schema elements
        var schemaLinks = await FindSchemaLinksAsync(entity, domain);
        if (schemaLinks.Any())
        {
            linkedEntity.Properties["schema_links"] = schemaLinks;
            linkedEntity.Properties["primary_schema_link"] = schemaLinks.First();
            
            // Boost confidence if we found good schema links
            linkedEntity.Confidence = Math.Min(1.0, linkedEntity.Confidence * 1.2);
        }

        // Try to link to business concepts
        var businessLinks = await FindBusinessConceptLinksAsync(entity, domain);
        if (businessLinks.Any())
        {
            linkedEntity.Properties["business_concepts"] = businessLinks;
            linkedEntity.Properties["primary_business_concept"] = businessLinks.First();
        }

        return linkedEntity;
    }

    /// <summary>
    /// Find schema element links for an entity
    /// </summary>
    private async Task<List<SchemaLink>> FindSchemaLinksAsync(SemanticEntity entity, string? domain)
    {
        var links = new List<SchemaLink>();
        var entityText = entity.Text.ToLowerInvariant();

        // Direct mapping lookup
        if (_schemaMappings.ContainsKey(entityText))
        {
            var mappings = _schemaMappings[entityText];
            
            // Filter by domain if specified
            if (!string.IsNullOrEmpty(domain))
            {
                mappings = mappings.Where(m => m.Domain == domain || m.Domain == "general").ToList();
            }

            foreach (var mapping in mappings.Take(3)) // Top 3 matches
            {
                links.Add(new SchemaLink
                {
                    TableName = mapping.TableName,
                    ColumnName = mapping.ColumnName,
                    LinkType = mapping.LinkType,
                    Confidence = mapping.Confidence,
                    Description = mapping.Description
                });
            }
        }

        // Fuzzy matching for partial matches
        var fuzzyLinks = await FindFuzzySchemaLinksAsync(entityText, domain);
        links.AddRange(fuzzyLinks);

        return links.OrderByDescending(l => l.Confidence).Take(5).ToList();
    }

    /// <summary>
    /// Find business concept links for an entity
    /// </summary>
    private async Task<List<BusinessConceptLink>> FindBusinessConceptLinksAsync(SemanticEntity entity, string? domain)
    {
        var links = new List<BusinessConceptLink>();
        var entityText = entity.Text.ToLowerInvariant();

        // Look for business concept matches
        foreach (var (conceptKey, concepts) in _businessConcepts)
        {
            var matchingConcepts = concepts.Where(c => 
                c.Keywords.Any(k => k.ToLowerInvariant().Contains(entityText) || 
                                   entityText.Contains(k.ToLowerInvariant())) &&
                (string.IsNullOrEmpty(domain) || c.Domain == domain || c.Domain == "general")
            ).ToList();

            foreach (var concept in matchingConcepts.Take(2))
            {
                links.Add(new BusinessConceptLink
                {
                    ConceptName = concept.Name,
                    ConceptType = concept.Type,
                    Description = concept.Description,
                    Confidence = CalculateConceptConfidence(entityText, concept),
                    Domain = concept.Domain
                });
            }
        }

        return links.OrderByDescending(l => l.Confidence).Take(3).ToList();
    }

    /// <summary>
    /// Find fuzzy schema links using similarity matching
    /// </summary>
    private async Task<List<SchemaLink>> FindFuzzySchemaLinksAsync(string entityText, string? domain)
    {
        var links = new List<SchemaLink>();

        // Common table name patterns
        var tablePatterns = new Dictionary<string, List<string>>
        {
            ["player"] = new() { "tbl_daily_actions_players", "players", "user", "account" },
            ["transaction"] = new() { "tbl_daily_actions", "transactions", "activity" },
            ["deposit"] = new() { "tbl_daily_actions", "deposits", "payments" },
            ["country"] = new() { "tbl_countries", "countries", "location" },
            ["currency"] = new() { "tbl_currencies", "currencies", "money" }
        };

        foreach (var (pattern, tables) in tablePatterns)
        {
            if (entityText.Contains(pattern) || pattern.Contains(entityText))
            {
                foreach (var table in tables)
                {
                    var similarity = CalculateStringSimilarity(entityText, table);
                    if (similarity > 0.6)
                    {
                        links.Add(new SchemaLink
                        {
                            TableName = table,
                            ColumnName = null,
                            LinkType = "fuzzy_table_match",
                            Confidence = similarity * 0.8, // Reduce confidence for fuzzy matches
                            Description = $"Fuzzy match to table {table}"
                        });
                    }
                }
            }
        }

        return links;
    }

    /// <summary>
    /// Calculate confidence score for business concept matching
    /// </summary>
    private double CalculateConceptConfidence(string entityText, BusinessConcept concept)
    {
        var maxKeywordSimilarity = concept.Keywords
            .Select(k => CalculateStringSimilarity(entityText, k.ToLowerInvariant()))
            .DefaultIfEmpty(0.0)
            .Max();

        // Boost confidence for exact matches
        if (concept.Keywords.Any(k => k.ToLowerInvariant() == entityText))
        {
            return 0.95;
        }

        return maxKeywordSimilarity * 0.8;
    }

    /// <summary>
    /// Calculate string similarity using Levenshtein distance
    /// </summary>
    private double CalculateStringSimilarity(string str1, string str2)
    {
        if (string.IsNullOrEmpty(str1) || string.IsNullOrEmpty(str2))
            return 0.0;

        var maxLength = Math.Max(str1.Length, str2.Length);
        if (maxLength == 0) return 1.0;

        var distance = CalculateLevenshteinDistance(str1, str2);
        return 1.0 - (double)distance / maxLength;
    }

    /// <summary>
    /// Calculate Levenshtein distance between two strings
    /// </summary>
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

    /// <summary>
    /// Initialize schema mappings for common entities
    /// </summary>
    private Dictionary<string, List<SchemaMapping>> InitializeSchemaMappings()
    {
        return new Dictionary<string, List<SchemaMapping>>
        {
            ["player"] = new()
            {
                new SchemaMapping { TableName = "tbl_Daily_actions_players", ColumnName = null, LinkType = "table", Confidence = 0.9, Domain = "gaming", Description = "Player information table" },
                new SchemaMapping { TableName = "tbl_Daily_actions", ColumnName = "PlayerID", LinkType = "column", Confidence = 0.8, Domain = "gaming", Description = "Player identifier" }
            },
            ["deposit"] = new()
            {
                new SchemaMapping { TableName = "tbl_Daily_actions", ColumnName = "TotalDepositsAmount", LinkType = "column", Confidence = 0.9, Domain = "gaming", Description = "Total deposit amount" },
                new SchemaMapping { TableName = "tbl_Daily_actions", ColumnName = "TotalDepositsCount", LinkType = "column", Confidence = 0.8, Domain = "gaming", Description = "Number of deposits" }
            },
            ["country"] = new()
            {
                new SchemaMapping { TableName = "tbl_Countries", ColumnName = null, LinkType = "table", Confidence = 0.9, Domain = "general", Description = "Countries reference table" },
                new SchemaMapping { TableName = "tbl_Daily_actions", ColumnName = "CountryID", LinkType = "column", Confidence = 0.8, Domain = "gaming", Description = "Country identifier" }
            },
            ["revenue"] = new()
            {
                new SchemaMapping { TableName = "tbl_Daily_actions", ColumnName = "TotalRevenueAmount", LinkType = "column", Confidence = 0.9, Domain = "gaming", Description = "Total revenue amount" }
            },
            ["bet"] = new()
            {
                new SchemaMapping { TableName = "tbl_Daily_actions", ColumnName = "TotalBetsAmount", LinkType = "column", Confidence = 0.9, Domain = "gaming", Description = "Total bets amount" },
                new SchemaMapping { TableName = "tbl_Daily_actions", ColumnName = "TotalBetsCount", LinkType = "column", Confidence = 0.8, Domain = "gaming", Description = "Number of bets" }
            },
            ["currency"] = new()
            {
                new SchemaMapping { TableName = "tbl_Currencies", ColumnName = null, LinkType = "table", Confidence = 0.9, Domain = "general", Description = "Currencies reference table" },
                new SchemaMapping { TableName = "tbl_Daily_actions", ColumnName = "CurrencyID", LinkType = "column", Confidence = 0.8, Domain = "gaming", Description = "Currency identifier" }
            }
        };
    }

    /// <summary>
    /// Initialize business concepts for entity linking
    /// </summary>
    private Dictionary<string, List<BusinessConcept>> InitializeBusinessConcepts()
    {
        return new Dictionary<string, List<BusinessConcept>>
        {
            ["financial_metrics"] = new()
            {
                new BusinessConcept { Name = "Revenue", Type = "metric", Keywords = new[] { "revenue", "income", "earnings" }, Domain = "gaming", Description = "Total revenue generated" },
                new BusinessConcept { Name = "Profit", Type = "metric", Keywords = new[] { "profit", "margin", "earnings" }, Domain = "gaming", Description = "Net profit after costs" },
                new BusinessConcept { Name = "Deposits", Type = "transaction", Keywords = new[] { "deposit", "funding", "payment" }, Domain = "gaming", Description = "Money deposited by players" }
            },
            ["player_metrics"] = new()
            {
                new BusinessConcept { Name = "Active Players", Type = "metric", Keywords = new[] { "active", "player", "user" }, Domain = "gaming", Description = "Players who performed actions" },
                new BusinessConcept { Name = "New Players", Type = "metric", Keywords = new[] { "new", "registration", "signup" }, Domain = "gaming", Description = "Newly registered players" },
                new BusinessConcept { Name = "Player Retention", Type = "metric", Keywords = new[] { "retention", "returning", "loyal" }, Domain = "gaming", Description = "Player retention rate" }
            },
            ["gaming_metrics"] = new()
            {
                new BusinessConcept { Name = "Betting Volume", Type = "metric", Keywords = new[] { "bet", "wager", "stake" }, Domain = "gaming", Description = "Total betting volume" },
                new BusinessConcept { Name = "Game Sessions", Type = "metric", Keywords = new[] { "session", "game", "play" }, Domain = "gaming", Description = "Gaming sessions" },
                new BusinessConcept { Name = "Win Rate", Type = "metric", Keywords = new[] { "win", "victory", "success" }, Domain = "gaming", Description = "Player win rate" }
            }
        };
    }
}

/// <summary>
/// Schema mapping configuration
/// </summary>
public class SchemaMapping
{
    public string TableName { get; set; } = string.Empty;
    public string? ColumnName { get; set; }
    public string LinkType { get; set; } = string.Empty; // "table", "column", "relationship"
    public double Confidence { get; set; }
    public string Domain { get; set; } = "general";
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Schema link result
/// </summary>
public class SchemaLink
{
    public string TableName { get; set; } = string.Empty;
    public string? ColumnName { get; set; }
    public string LinkType { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Business concept definition
/// </summary>
public class BusinessConcept
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "metric", "dimension", "transaction", etc.
    public string[] Keywords { get; set; } = Array.Empty<string>();
    public string Domain { get; set; } = "general";
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Business concept link result
/// </summary>
public class BusinessConceptLink
{
    public string ConceptName { get; set; } = string.Empty;
    public string ConceptType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Domain { get; set; } = string.Empty;
}
