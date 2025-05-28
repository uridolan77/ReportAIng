using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// Analyzes query similarity using semantic features and patterns
/// </summary>
public class QuerySimilarityAnalyzer
{
    private readonly ILogger _logger;
    private readonly Dictionary<string, double> _keywordWeights;
    private readonly Dictionary<string, string[]> _synonymGroups;

    public QuerySimilarityAnalyzer(ILogger logger)
    {
        _logger = logger;
        _keywordWeights = InitializeKeywordWeights();
        _synonymGroups = InitializeSynonymGroups();
    }

    /// <summary>
    /// Extract semantic features from natural language and SQL queries
    /// </summary>
    public async Task<SemanticFeatures> ExtractSemanticsAsync(string naturalLanguageQuery, string sqlQuery)
    {
        try
        {
            var features = new SemanticFeatures
            {
                // Natural language features
                NaturalLanguageKeywords = ExtractKeywords(naturalLanguageQuery),
                QueryIntent = ClassifyQueryIntent(naturalLanguageQuery),
                EntityMentions = ExtractEntityMentions(naturalLanguageQuery),
                TemporalReferences = ExtractTemporalReferences(naturalLanguageQuery),
                AggregationTypes = ExtractAggregationTypes(naturalLanguageQuery),

                // SQL features
                SqlStructure = AnalyzeSqlStructure(sqlQuery),
                TableReferences = ExtractTableReferences(sqlQuery),
                ColumnReferences = ExtractColumnReferences(sqlQuery),
                JoinTypes = ExtractJoinTypes(sqlQuery),
                FilterConditions = ExtractFilterConditions(sqlQuery),
                
                // Combined features
                ComplexityScore = CalculateComplexityScore(naturalLanguageQuery, sqlQuery),
                SemanticVector = await GenerateSemanticVectorAsync(naturalLanguageQuery, sqlQuery)
            };

            return features;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting semantic features");
            return new SemanticFeatures();
        }
    }

    /// <summary>
    /// Calculate similarity between two sets of semantic features
    /// </summary>
    public double CalculateSimilarity(SemanticFeatures features1, SemanticFeatures features2)
    {
        try
        {
            var similarities = new List<double>();

            // Intent similarity (high weight)
            similarities.Add(CalculateIntentSimilarity(features1.QueryIntent, features2.QueryIntent) * 0.25);

            // Keyword similarity
            similarities.Add(CalculateKeywordSimilarity(features1.NaturalLanguageKeywords, features2.NaturalLanguageKeywords) * 0.20);

            // SQL structure similarity
            similarities.Add(CalculateSqlStructureSimilarity(features1.SqlStructure, features2.SqlStructure) * 0.20);

            // Entity similarity
            similarities.Add(CalculateEntitySimilarity(features1.EntityMentions, features2.EntityMentions) * 0.15);

            // Table/column similarity
            similarities.Add(CalculateTableColumnSimilarity(features1, features2) * 0.15);

            // Semantic vector similarity
            similarities.Add(CalculateVectorSimilarity(features1.SemanticVector, features2.SemanticVector) * 0.05);

            var overallSimilarity = similarities.Sum();
            
            _logger.LogDebug("Calculated similarity: {Similarity:P2} between queries", overallSimilarity);
            
            return Math.Max(0.0, Math.Min(1.0, overallSimilarity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating similarity");
            return 0.0;
        }
    }

    private List<string> ExtractKeywords(string text)
    {
        var words = Regex.Split(text.ToLower(), @"\W+")
            .Where(w => w.Length > 2)
            .Where(w => !IsStopWord(w))
            .ToList();

        // Normalize synonyms
        return words.Select(NormalizeSynonym).Distinct().ToList();
    }

    private QueryIntent ClassifyQueryIntent(string query)
    {
        var lowerQuery = query.ToLower();

        if (lowerQuery.Contains("show") || lowerQuery.Contains("display") || lowerQuery.Contains("list"))
            return QueryIntent.Display;
        if (lowerQuery.Contains("count") || lowerQuery.Contains("number of"))
            return QueryIntent.Count;
        if (lowerQuery.Contains("sum") || lowerQuery.Contains("total"))
            return QueryIntent.Sum;
        if (lowerQuery.Contains("average") || lowerQuery.Contains("mean"))
            return QueryIntent.Average;
        if (lowerQuery.Contains("max") || lowerQuery.Contains("maximum") || lowerQuery.Contains("highest"))
            return QueryIntent.Maximum;
        if (lowerQuery.Contains("min") || lowerQuery.Contains("minimum") || lowerQuery.Contains("lowest"))
            return QueryIntent.Minimum;
        if (lowerQuery.Contains("group") || lowerQuery.Contains("by"))
            return QueryIntent.GroupBy;
        if (lowerQuery.Contains("filter") || lowerQuery.Contains("where") || lowerQuery.Contains("condition"))
            return QueryIntent.Filter;
        if (lowerQuery.Contains("join") || lowerQuery.Contains("combine") || lowerQuery.Contains("merge"))
            return QueryIntent.Join;

        return QueryIntent.General;
    }

    private List<string> ExtractEntityMentions(string query)
    {
        var entities = new List<string>();
        var entityPatterns = new[]
        {
            @"\b(customer|client|user|person|people)\b",
            @"\b(order|purchase|transaction|sale)\b",
            @"\b(product|item|goods|merchandise)\b",
            @"\b(invoice|bill|receipt|payment)\b",
            @"\b(account|profile|record)\b",
            @"\b(company|organization|business)\b"
        };

        foreach (var pattern in entityPatterns)
        {
            var matches = Regex.Matches(query, pattern, RegexOptions.IgnoreCase);
            entities.AddRange(matches.Select(m => m.Value.ToLower()));
        }

        return entities.Distinct().ToList();
    }

    private List<string> ExtractTemporalReferences(string query)
    {
        var temporal = new List<string>();
        var temporalPatterns = new[]
        {
            @"\b(today|yesterday|tomorrow)\b",
            @"\b(last|this|next)\s+(week|month|year|day)\b",
            @"\b\d{4}-\d{2}-\d{2}\b",
            @"\b(recent|latest|current|past)\b",
            @"\b(before|after|since|until)\b"
        };

        foreach (var pattern in temporalPatterns)
        {
            var matches = Regex.Matches(query, pattern, RegexOptions.IgnoreCase);
            temporal.AddRange(matches.Select(m => m.Value.ToLower()));
        }

        return temporal.Distinct().ToList();
    }

    private List<string> ExtractAggregationTypes(string query)
    {
        var aggregations = new List<string>();
        var aggregationPatterns = new Dictionary<string, string[]>
        {
            ["count"] = new[] { "count", "number", "total records", "how many" },
            ["sum"] = new[] { "sum", "total", "add up", "combined" },
            ["average"] = new[] { "average", "mean", "avg" },
            ["max"] = new[] { "max", "maximum", "highest", "largest", "biggest" },
            ["min"] = new[] { "min", "minimum", "lowest", "smallest" }
        };

        foreach (var (aggType, patterns) in aggregationPatterns)
        {
            if (patterns.Any(p => query.Contains(p, StringComparison.OrdinalIgnoreCase)))
            {
                aggregations.Add(aggType);
            }
        }

        return aggregations;
    }

    private SqlStructure AnalyzeSqlStructure(string sql)
    {
        var upperSql = sql.ToUpper();
        
        return new SqlStructure
        {
            HasSelect = upperSql.Contains("SELECT"),
            HasWhere = upperSql.Contains("WHERE"),
            HasGroupBy = upperSql.Contains("GROUP BY"),
            HasOrderBy = upperSql.Contains("ORDER BY"),
            HasHaving = upperSql.Contains("HAVING"),
            HasJoin = upperSql.Contains("JOIN"),
            HasSubquery = upperSql.Contains("(SELECT"),
            HasUnion = upperSql.Contains("UNION"),
            HasLimit = upperSql.Contains("LIMIT") || upperSql.Contains("TOP")
        };
    }

    private List<string> ExtractTableReferences(string sql)
    {
        var tables = new List<string>();
        var tablePattern = @"FROM\s+(\w+)|JOIN\s+(\w+)";
        var matches = Regex.Matches(sql, tablePattern, RegexOptions.IgnoreCase);
        
        foreach (Match match in matches)
        {
            var tableName = match.Groups[1].Value.Trim();
            if (string.IsNullOrEmpty(tableName))
                tableName = match.Groups[2].Value.Trim();
            
            if (!string.IsNullOrEmpty(tableName))
                tables.Add(tableName.ToLower());
        }

        return tables.Distinct().ToList();
    }

    private List<string> ExtractColumnReferences(string sql)
    {
        var columns = new List<string>();
        
        // Extract from SELECT clause
        var selectPattern = @"SELECT\s+(.*?)\s+FROM";
        var selectMatch = Regex.Match(sql, selectPattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        
        if (selectMatch.Success)
        {
            var selectClause = selectMatch.Groups[1].Value;
            var columnMatches = Regex.Matches(selectClause, @"\b(\w+)\b");
            columns.AddRange(columnMatches.Select(m => m.Value.ToLower()));
        }

        return columns.Where(c => c != "select" && c != "from").Distinct().ToList();
    }

    private List<string> ExtractJoinTypes(string sql)
    {
        var joins = new List<string>();
        var joinPatterns = new[] { "INNER JOIN", "LEFT JOIN", "RIGHT JOIN", "FULL JOIN", "CROSS JOIN" };
        
        foreach (var joinType in joinPatterns)
        {
            if (sql.Contains(joinType, StringComparison.OrdinalIgnoreCase))
            {
                joins.Add(joinType.ToLower());
            }
        }

        return joins;
    }

    private List<string> ExtractFilterConditions(string sql)
    {
        var conditions = new List<string>();
        var conditionPatterns = new[] { "=", "!=", "<>", "<", ">", "<=", ">=", "LIKE", "IN", "BETWEEN", "IS NULL", "IS NOT NULL" };
        
        foreach (var condition in conditionPatterns)
        {
            if (sql.Contains(condition, StringComparison.OrdinalIgnoreCase))
            {
                conditions.Add(condition.ToLower());
            }
        }

        return conditions;
    }

    private double CalculateComplexityScore(string naturalLanguageQuery, string sqlQuery)
    {
        var score = 0.0;
        
        // Natural language complexity
        score += naturalLanguageQuery.Split(' ').Length * 0.1;
        score += ExtractEntityMentions(naturalLanguageQuery).Count * 0.2;
        score += ExtractAggregationTypes(naturalLanguageQuery).Count * 0.3;

        // SQL complexity
        var sqlStructure = AnalyzeSqlStructure(sqlQuery);
        if (sqlStructure.HasJoin) score += 1.0;
        if (sqlStructure.HasSubquery) score += 1.5;
        if (sqlStructure.HasGroupBy) score += 0.8;
        if (sqlStructure.HasHaving) score += 1.0;
        if (sqlStructure.HasUnion) score += 1.2;

        return Math.Min(10.0, score); // Cap at 10
    }

    private async Task<double[]> GenerateSemanticVectorAsync(string naturalLanguageQuery, string sqlQuery)
    {
        // Simplified semantic vector generation
        // In a real implementation, you might use embeddings from a language model
        var vector = new double[50]; // 50-dimensional vector
        
        var combinedText = $"{naturalLanguageQuery} {sqlQuery}".ToLower();
        var hash = combinedText.GetHashCode();
        var random = new Random(hash);
        
        for (int i = 0; i < vector.Length; i++)
        {
            vector[i] = random.NextDouble() * 2 - 1; // Values between -1 and 1
        }

        return vector;
    }

    private double CalculateIntentSimilarity(QueryIntent intent1, QueryIntent intent2)
    {
        if (intent1 == intent2) return 1.0;
        
        // Define intent similarity groups
        var aggregationIntents = new[] { QueryIntent.Count, QueryIntent.Sum, QueryIntent.Average, QueryIntent.Maximum, QueryIntent.Minimum };
        var displayIntents = new[] { QueryIntent.Display, QueryIntent.Filter };
        
        if (aggregationIntents.Contains(intent1) && aggregationIntents.Contains(intent2)) return 0.7;
        if (displayIntents.Contains(intent1) && displayIntents.Contains(intent2)) return 0.8;
        
        return 0.0;
    }

    private double CalculateKeywordSimilarity(List<string> keywords1, List<string> keywords2)
    {
        if (!keywords1.Any() && !keywords2.Any()) return 1.0;
        if (!keywords1.Any() || !keywords2.Any()) return 0.0;

        var intersection = keywords1.Intersect(keywords2).Count();
        var union = keywords1.Union(keywords2).Count();
        
        return (double)intersection / union; // Jaccard similarity
    }

    private double CalculateSqlStructureSimilarity(SqlStructure struct1, SqlStructure struct2)
    {
        var similarities = new[]
        {
            struct1.HasSelect == struct2.HasSelect ? 1.0 : 0.0,
            struct1.HasWhere == struct2.HasWhere ? 1.0 : 0.0,
            struct1.HasGroupBy == struct2.HasGroupBy ? 1.0 : 0.0,
            struct1.HasOrderBy == struct2.HasOrderBy ? 1.0 : 0.0,
            struct1.HasJoin == struct2.HasJoin ? 1.0 : 0.0,
            struct1.HasSubquery == struct2.HasSubquery ? 1.0 : 0.0
        };

        return similarities.Average();
    }

    private double CalculateEntitySimilarity(List<string> entities1, List<string> entities2)
    {
        if (!entities1.Any() && !entities2.Any()) return 1.0;
        if (!entities1.Any() || !entities2.Any()) return 0.0;

        var intersection = entities1.Intersect(entities2).Count();
        var union = entities1.Union(entities2).Count();
        
        return (double)intersection / union;
    }

    private double CalculateTableColumnSimilarity(SemanticFeatures features1, SemanticFeatures features2)
    {
        var tableSim = CalculateListSimilarity(features1.TableReferences, features2.TableReferences);
        var columnSim = CalculateListSimilarity(features1.ColumnReferences, features2.ColumnReferences);
        
        return (tableSim + columnSim) / 2.0;
    }

    private double CalculateVectorSimilarity(double[] vector1, double[] vector2)
    {
        if (vector1.Length != vector2.Length) return 0.0;

        // Cosine similarity
        var dotProduct = vector1.Zip(vector2, (a, b) => a * b).Sum();
        var magnitude1 = Math.Sqrt(vector1.Sum(x => x * x));
        var magnitude2 = Math.Sqrt(vector2.Sum(x => x * x));

        if (magnitude1 == 0 || magnitude2 == 0) return 0.0;

        return dotProduct / (magnitude1 * magnitude2);
    }

    private double CalculateListSimilarity(List<string> list1, List<string> list2)
    {
        if (!list1.Any() && !list2.Any()) return 1.0;
        if (!list1.Any() || !list2.Any()) return 0.0;

        var intersection = list1.Intersect(list2).Count();
        var union = list1.Union(list2).Count();
        
        return (double)intersection / union;
    }

    private Dictionary<string, double> InitializeKeywordWeights()
    {
        return new Dictionary<string, double>
        {
            ["show"] = 1.0, ["display"] = 1.0, ["list"] = 1.0,
            ["count"] = 1.5, ["sum"] = 1.5, ["total"] = 1.5,
            ["average"] = 1.5, ["max"] = 1.5, ["min"] = 1.5,
            ["group"] = 1.2, ["join"] = 1.2, ["filter"] = 1.2,
            ["customer"] = 0.8, ["order"] = 0.8, ["product"] = 0.8
        };
    }

    private Dictionary<string, string[]> InitializeSynonymGroups()
    {
        return new Dictionary<string, string[]>
        {
            ["show"] = new[] { "show", "display", "list", "get", "retrieve" },
            ["count"] = new[] { "count", "number", "total" },
            ["customer"] = new[] { "customer", "client", "user" },
            ["order"] = new[] { "order", "purchase", "transaction" },
            ["product"] = new[] { "product", "item", "goods" }
        };
    }

    private string NormalizeSynonym(string word)
    {
        foreach (var (canonical, synonyms) in _synonymGroups)
        {
            if (synonyms.Contains(word))
                return canonical;
        }
        return word;
    }

    private bool IsStopWord(string word)
    {
        var stopWords = new HashSet<string> { "the", "and", "for", "are", "but", "not", "you", "all", "can", "had", "her", "was", "one", "our", "out", "day", "get", "has", "him", "his", "how", "its", "may", "new", "now", "old", "see", "two", "who", "boy", "did", "man", "men", "put", "say", "she", "too", "use", "a", "an", "as", "at", "be", "by", "he", "in", "is", "it", "of", "on", "or", "to", "up", "we" };
        return stopWords.Contains(word);
    }
}

/// <summary>
/// Semantic features extracted from queries
/// </summary>
public class SemanticFeatures
{
    public List<string> NaturalLanguageKeywords { get; set; } = new();
    public QueryIntent QueryIntent { get; set; }
    public List<string> EntityMentions { get; set; } = new();
    public List<string> TemporalReferences { get; set; } = new();
    public List<string> AggregationTypes { get; set; } = new();
    public SqlStructure SqlStructure { get; set; } = new();
    public List<string> TableReferences { get; set; } = new();
    public List<string> ColumnReferences { get; set; } = new();
    public List<string> JoinTypes { get; set; } = new();
    public List<string> FilterConditions { get; set; } = new();
    public double ComplexityScore { get; set; }
    public double[] SemanticVector { get; set; } = Array.Empty<double>();
}

/// <summary>
/// SQL structure analysis
/// </summary>
public class SqlStructure
{
    public bool HasSelect { get; set; }
    public bool HasWhere { get; set; }
    public bool HasGroupBy { get; set; }
    public bool HasOrderBy { get; set; }
    public bool HasHaving { get; set; }
    public bool HasJoin { get; set; }
    public bool HasSubquery { get; set; }
    public bool HasUnion { get; set; }
    public bool HasLimit { get; set; }
}

/// <summary>
/// Query intent classification
/// </summary>
public enum QueryIntent
{
    General,
    Display,
    Count,
    Sum,
    Average,
    Maximum,
    Minimum,
    GroupBy,
    Filter,
    Join
}
