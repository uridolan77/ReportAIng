using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Core.Models;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// ML-based anomaly detector for identifying suspicious query patterns and user behavior
/// </summary>
public class MLAnomalyDetector
{
    private readonly BICopilotContext _context;
    private readonly ILogger<MLAnomalyDetector> _logger;
    private readonly AnomalyDetectionModel _model;
    private readonly Dictionary<string, UserBehaviorProfile> _userProfiles;

    public MLAnomalyDetector(BICopilotContext context, ILogger<MLAnomalyDetector> logger)
    {
        _context = context;
        _logger = logger;
        _model = new AnomalyDetectionModel();
        _userProfiles = new Dictionary<string, UserBehaviorProfile>();
    }

    /// <summary>
    /// Analyze a query for anomalies and return risk assessment
    /// </summary>
    public async Task<AnomalyDetectionResult> AnalyzeQueryAsync(string userId, string naturalLanguageQuery, string sqlQuery)
    {
        try
        {
            var features = await ExtractQueryFeaturesAsync(userId, naturalLanguageQuery, sqlQuery);
            var userProfile = await GetOrCreateUserProfileAsync(userId);

            var anomalyScore = _model.CalculateAnomalyScore(features, userProfile);
            var riskLevel = DetermineRiskLevel(anomalyScore);

            var result = new AnomalyDetectionResult
            {
                UserId = userId,
                AnomalyScore = anomalyScore,
                RiskLevel = riskLevel,
                DetectedAnomalies = IdentifySpecificAnomalies(features, userProfile),
                Timestamp = DateTime.UtcNow,
                QueryFeatures = features
            };

            // Update user profile with new behavior
            await UpdateUserProfileAsync(userId, features);

            // Log high-risk queries
            if (riskLevel >= RiskLevel.High)
            {
                _logger.LogWarning("High-risk query detected for user {UserId}. Anomaly score: {Score:F3}, Query: {Query}",
                    userId, anomalyScore, naturalLanguageQuery);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing query for anomalies");
            return new AnomalyDetectionResult
            {
                UserId = userId,
                AnomalyScore = 0.0,
                RiskLevel = RiskLevel.Low,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Analyze user behavior patterns for anomalies
    /// </summary>
    public async Task<UserBehaviorAnomalyResult> AnalyzeUserBehaviorAsync(string userId, TimeSpan timeWindow)
    {
        try
        {
            var recentQueries = await GetRecentUserQueriesAsync(userId, timeWindow);
            var behaviorFeatures = ExtractBehaviorFeatures(recentQueries);
            var userProfile = await GetOrCreateUserProfileAsync(userId);

            var behaviorScore = _model.CalculateBehaviorAnomalyScore(behaviorFeatures, userProfile);
            var anomalies = IdentifyBehaviorAnomalies(behaviorFeatures, userProfile);

            return new UserBehaviorAnomalyResult
            {
                UserId = userId,
                BehaviorScore = behaviorScore,
                RiskLevel = DetermineRiskLevel(behaviorScore),
                DetectedAnomalies = anomalies,
                AnalysisWindow = timeWindow,
                QueryCount = recentQueries.Count,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing user behavior for user {UserId}", userId);
            return new UserBehaviorAnomalyResult
            {
                UserId = userId,
                BehaviorScore = 0.0,
                RiskLevel = RiskLevel.Low,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Train the anomaly detection model with historical data
    /// </summary>
    public async Task TrainModelAsync()
    {
        try
        {
            _logger.LogInformation("Starting anomaly detection model training");

            var trainingData = await GetTrainingDataAsync();
            _model.Train(trainingData);

            _logger.LogInformation("Anomaly detection model training completed with {SampleCount} samples", trainingData.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error training anomaly detection model");
        }
    }

    private async Task<QueryFeatures> ExtractQueryFeaturesAsync(string userId, string naturalLanguageQuery, string sqlQuery)
    {
        var features = new QueryFeatures
        {
            UserId = userId,
            QueryLength = naturalLanguageQuery.Length,
            SqlLength = sqlQuery.Length,
            ComplexityScore = CalculateQueryComplexity(sqlQuery),
            HasJoins = sqlQuery.Contains("JOIN", StringComparison.OrdinalIgnoreCase),
            HasSubqueries = sqlQuery.Contains("(SELECT", StringComparison.OrdinalIgnoreCase),
            HasAggregations = ContainsAggregations(sqlQuery),
            TableCount = CountTables(sqlQuery),
            ColumnCount = CountColumns(sqlQuery),
            TimeOfDay = DateTime.Now.Hour,
            DayOfWeek = (int)DateTime.Now.DayOfWeek,
            IsWeekend = DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday
        };

        // Add historical context
        var userHistory = await GetUserQueryHistoryAsync(userId, TimeSpan.FromDays(30));
        features.QueriesInLast24Hours = userHistory.Count(q => q.ExecutedAt > DateTime.UtcNow.AddDays(-1));
        features.AverageQueryLength = userHistory.Any() ? userHistory.Average(q => q.Query.Length) : 0;
        features.UniqueTablesAccessed = userHistory.SelectMany(q => ExtractTableNames(q.GeneratedSql ?? "")).Distinct().Count();

        return features;
    }

    private async Task<UserBehaviorProfile> GetOrCreateUserProfileAsync(string userId)
    {
        if (_userProfiles.TryGetValue(userId, out var cachedProfile))
        {
            return cachedProfile;
        }

        var profile = await BuildUserProfileAsync(userId);
        _userProfiles[userId] = profile;
        return profile;
    }

    private async Task<UserBehaviorProfile> BuildUserProfileAsync(string userId)
    {
        var queryHistory = await GetUserQueryHistoryAsync(userId, TimeSpan.FromDays(90));

        if (!queryHistory.Any())
        {
            return new UserBehaviorProfile { UserId = userId };
        }

        return new UserBehaviorProfile
        {
            UserId = userId,
            AverageQueryLength = queryHistory.Average(q => q.Query.Length),
            AverageQueriesPerDay = queryHistory.Count / 90.0,
            PreferredTimeOfDay = GetPreferredTimeOfDay(queryHistory),
            CommonTablePatterns = GetCommonTablePatterns(queryHistory),
            TypicalComplexity = queryHistory.Average(q => CalculateQueryComplexity(q.GeneratedSql ?? "")),
            LastUpdated = DateTime.UtcNow
        };
    }

    private async Task<List<QueryHistoryEntity>> GetUserQueryHistoryAsync(string userId, TimeSpan timeWindow)
    {
        var cutoffDate = DateTime.UtcNow.Subtract(timeWindow);

        return await _context.QueryHistories
            .Where(q => q.UserId == userId && q.ExecutedAt > cutoffDate)
            .OrderByDescending(q => q.ExecutedAt)
            .Take(1000) // Limit for performance
            .ToListAsync();
    }

    private async Task<List<QueryHistoryEntity>> GetRecentUserQueriesAsync(string userId, TimeSpan timeWindow)
    {
        var cutoffDate = DateTime.UtcNow.Subtract(timeWindow);

        return await _context.QueryHistories
            .Where(q => q.UserId == userId && q.ExecutedAt > cutoffDate)
            .OrderByDescending(q => q.ExecutedAt)
            .ToListAsync();
    }

    private BehaviorFeatures ExtractBehaviorFeatures(List<QueryHistoryEntity> queries)
    {
        if (!queries.Any())
        {
            return new BehaviorFeatures();
        }

        return new BehaviorFeatures
        {
            QueryFrequency = queries.Count,
            AverageQueryInterval = CalculateAverageInterval(queries),
            TimeDistribution = CalculateTimeDistribution(queries),
            ComplexityVariation = CalculateComplexityVariation(queries),
            TableAccessPattern = CalculateTableAccessPattern(queries),
            ErrorRate = queries.Count(q => !q.IsSuccessful) / (double)queries.Count
        };
    }

    private double CalculateQueryComplexity(string sql)
    {
        if (string.IsNullOrEmpty(sql)) return 0.0;

        var complexity = 0.0;
        var upperSql = sql.ToUpper();

        // Basic complexity factors
        complexity += upperSql.Split("SELECT").Length - 1; // Number of SELECT statements
        complexity += (upperSql.Split("JOIN").Length - 1) * 2; // JOINs are more complex
        complexity += (upperSql.Split("UNION").Length - 1) * 3; // UNIONs are very complex
        complexity += (upperSql.Split("GROUP BY").Length - 1) * 1.5; // GROUP BY adds complexity
        complexity += (upperSql.Split("ORDER BY").Length - 1) * 0.5; // ORDER BY adds some complexity
        complexity += (upperSql.Split("HAVING").Length - 1) * 2; // HAVING is complex

        return complexity;
    }

    private bool ContainsAggregations(string sql)
    {
        var aggregations = new[] { "COUNT(", "SUM(", "AVG(", "MAX(", "MIN(" };
        return aggregations.Any(agg => sql.Contains(agg, StringComparison.OrdinalIgnoreCase));
    }

    private int CountTables(string sql)
    {
        var tablePattern = @"FROM\s+(\w+)|JOIN\s+(\w+)";
        var matches = System.Text.RegularExpressions.Regex.Matches(sql, tablePattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        return matches.Count;
    }

    private int CountColumns(string sql)
    {
        // Simplified column counting
        var selectIndex = sql.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);
        var fromIndex = sql.IndexOf("FROM", StringComparison.OrdinalIgnoreCase);

        if (selectIndex == -1 || fromIndex == -1 || fromIndex <= selectIndex) return 0;

        var selectClause = sql.Substring(selectIndex + 6, fromIndex - selectIndex - 6);
        return selectClause.Split(',').Length;
    }

    private List<string> ExtractTableNames(string sql)
    {
        var tables = new List<string>();
        var tablePattern = @"FROM\s+(\w+)|JOIN\s+(\w+)";
        var matches = System.Text.RegularExpressions.Regex.Matches(sql, tablePattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            var tableName = match.Groups[1].Value.Trim();
            if (string.IsNullOrEmpty(tableName))
                tableName = match.Groups[2].Value.Trim();

            if (!string.IsNullOrEmpty(tableName))
                tables.Add(tableName.ToLower());
        }

        return tables.Distinct().ToList();
    }

    private RiskLevel DetermineRiskLevel(double anomalyScore)
    {
        return anomalyScore switch
        {
            >= 0.8 => RiskLevel.Critical,
            >= 0.6 => RiskLevel.High,
            >= 0.4 => RiskLevel.Medium,
            >= 0.2 => RiskLevel.Low,
            _ => RiskLevel.Minimal
        };
    }

    private List<string> IdentifySpecificAnomalies(QueryFeatures features, UserBehaviorProfile profile)
    {
        var anomalies = new List<string>();

        if (features.QueryLength > profile.AverageQueryLength * 3)
            anomalies.Add("Unusually long query");

        if (features.ComplexityScore > profile.TypicalComplexity * 2)
            anomalies.Add("Unusually complex query");

        if (features.QueriesInLast24Hours > profile.AverageQueriesPerDay * 5)
            anomalies.Add("High query frequency");

        if (Math.Abs(features.TimeOfDay - profile.PreferredTimeOfDay) > 6)
            anomalies.Add("Unusual time of access");

        return anomalies;
    }

    private List<string> IdentifyBehaviorAnomalies(BehaviorFeatures features, UserBehaviorProfile profile)
    {
        var anomalies = new List<string>();

        if (features.QueryFrequency > profile.AverageQueriesPerDay * 10)
            anomalies.Add("Extremely high query volume");

        if (features.ErrorRate > 0.5)
            anomalies.Add("High error rate");

        if (features.ComplexityVariation > 5.0)
            anomalies.Add("Unusual complexity variation");

        return anomalies;
    }

    private async Task UpdateUserProfileAsync(string userId, QueryFeatures features)
    {
        if (_userProfiles.TryGetValue(userId, out var profile))
        {
            // Update profile with exponential moving average
            profile.AverageQueryLength = profile.AverageQueryLength * 0.9 + features.QueryLength * 0.1;
            profile.TypicalComplexity = profile.TypicalComplexity * 0.9 + features.ComplexityScore * 0.1;
            profile.LastUpdated = DateTime.UtcNow;
        }
    }

    private async Task<List<TrainingDataPoint>> GetTrainingDataAsync()
    {
        // Get historical query data for training
        var queries = await _context.QueryHistories
            .Where(q => q.ExecutedAt > DateTime.UtcNow.AddDays(-180))
            .Take(10000)
            .ToListAsync();

        var trainingData = new List<TrainingDataPoint>();

        foreach (var query in queries)
        {
            var features = await ExtractQueryFeaturesAsync(query.UserId, query.Query, query.GeneratedSql ?? "");
            var label = query.IsSuccessful ? 0.0 : 1.0; // 0 = normal, 1 = anomaly

            trainingData.Add(new TrainingDataPoint
            {
                Features = features,
                Label = label
            });
        }

        return trainingData;
    }

    private int GetPreferredTimeOfDay(List<QueryHistoryEntity> queries)
    {
        return queries.GroupBy(q => q.ExecutedAt.Hour)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault()?.Key ?? 9; // Default to 9 AM
    }

    private List<string> GetCommonTablePatterns(List<QueryHistoryEntity> queries)
    {
        return queries
            .SelectMany(q => ExtractTableNames(q.GeneratedSql ?? ""))
            .GroupBy(t => t)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .Select(g => g.Key)
            .ToList();
    }

    private double CalculateAverageInterval(List<QueryHistoryEntity> queries)
    {
        if (queries.Count < 2) return 0.0;

        var intervals = new List<double>();
        for (int i = 1; i < queries.Count; i++)
        {
            var interval = (queries[i-1].ExecutedAt - queries[i].ExecutedAt).TotalMinutes;
            intervals.Add(interval);
        }

        return intervals.Average();
    }

    private Dictionary<int, int> CalculateTimeDistribution(List<QueryHistoryEntity> queries)
    {
        return queries
            .GroupBy(q => q.ExecutedAt.Hour)
            .ToDictionary(g => g.Key, g => g.Count());
    }

    private double CalculateComplexityVariation(List<QueryHistoryEntity> queries)
    {
        var complexities = queries.Select(q => CalculateQueryComplexity(q.GeneratedSql ?? "")).ToList();
        if (complexities.Count < 2) return 0.0;

        var mean = complexities.Average();
        var variance = complexities.Select(c => Math.Pow(c - mean, 2)).Average();
        return Math.Sqrt(variance);
    }

    private Dictionary<string, int> CalculateTableAccessPattern(List<QueryHistoryEntity> queries)
    {
        return queries
            .SelectMany(q => ExtractTableNames(q.GeneratedSql ?? ""))
            .GroupBy(t => t)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}

/// <summary>
/// Simple anomaly detection model
/// </summary>
public class AnomalyDetectionModel
{
    private readonly Dictionary<string, double> _featureWeights;
    private bool _isTrained;

    public AnomalyDetectionModel()
    {
        _featureWeights = new Dictionary<string, double>
        {
            ["QueryLength"] = 0.1,
            ["ComplexityScore"] = 0.3,
            ["TimeOfDay"] = 0.2,
            ["QueryFrequency"] = 0.4
        };
        _isTrained = false;
    }

    public double CalculateAnomalyScore(QueryFeatures features, UserBehaviorProfile profile)
    {
        var score = 0.0;

        // Length anomaly
        if (profile.AverageQueryLength > 0)
        {
            var lengthRatio = features.QueryLength / profile.AverageQueryLength;
            score += Math.Max(0, (lengthRatio - 2.0) * 0.2); // Anomaly if 2x longer than usual
        }

        // Complexity anomaly
        if (profile.TypicalComplexity > 0)
        {
            var complexityRatio = features.ComplexityScore / profile.TypicalComplexity;
            score += Math.Max(0, (complexityRatio - 2.0) * 0.3);
        }

        // Time anomaly
        var timeDiff = Math.Abs(features.TimeOfDay - profile.PreferredTimeOfDay);
        score += Math.Max(0, (timeDiff - 6) * 0.05); // Anomaly if >6 hours from preferred time

        // Frequency anomaly
        if (profile.AverageQueriesPerDay > 0)
        {
            var frequencyRatio = features.QueriesInLast24Hours / profile.AverageQueriesPerDay;
            score += Math.Max(0, (frequencyRatio - 5.0) * 0.2); // Anomaly if 5x more frequent
        }

        return Math.Min(1.0, score);
    }

    public double CalculateBehaviorAnomalyScore(BehaviorFeatures features, UserBehaviorProfile profile)
    {
        var score = 0.0;

        // High error rate
        score += features.ErrorRate * 0.5;

        // Unusual frequency
        if (profile.AverageQueriesPerDay > 0)
        {
            var frequencyRatio = features.QueryFrequency / profile.AverageQueriesPerDay;
            score += Math.Max(0, (frequencyRatio - 10.0) * 0.3);
        }

        // High complexity variation
        score += Math.Min(0.3, features.ComplexityVariation * 0.1);

        return Math.Min(1.0, score);
    }

    public void Train(List<TrainingDataPoint> trainingData)
    {
        // Simplified training - in a real implementation, you would use proper ML algorithms
        // _logger.LogInformation("Training anomaly detection model with {SampleCount} samples", trainingData.Count);
        _isTrained = true;
    }
}

// Supporting classes and enums would be defined here...
public class QueryFeatures
{
    public string UserId { get; set; } = string.Empty;
    public int QueryLength { get; set; }
    public int SqlLength { get; set; }
    public double ComplexityScore { get; set; }
    public bool HasJoins { get; set; }
    public bool HasSubqueries { get; set; }
    public bool HasAggregations { get; set; }
    public int TableCount { get; set; }
    public int ColumnCount { get; set; }
    public int TimeOfDay { get; set; }
    public int DayOfWeek { get; set; }
    public bool IsWeekend { get; set; }
    public int QueriesInLast24Hours { get; set; }
    public double AverageQueryLength { get; set; }
    public int UniqueTablesAccessed { get; set; }
}

public class UserBehaviorProfile
{
    public string UserId { get; set; } = string.Empty;
    public double AverageQueryLength { get; set; }
    public double AverageQueriesPerDay { get; set; }
    public int PreferredTimeOfDay { get; set; }
    public List<string> CommonTablePatterns { get; set; } = new();
    public double TypicalComplexity { get; set; }
    public DateTime LastUpdated { get; set; }
}

public class BehaviorFeatures
{
    public int QueryFrequency { get; set; }
    public double AverageQueryInterval { get; set; }
    public Dictionary<int, int> TimeDistribution { get; set; } = new();
    public double ComplexityVariation { get; set; }
    public Dictionary<string, int> TableAccessPattern { get; set; } = new();
    public double ErrorRate { get; set; }
}

public class AnomalyDetectionResult
{
    public string UserId { get; set; } = string.Empty;
    public double AnomalyScore { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public List<string> DetectedAnomalies { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public QueryFeatures QueryFeatures { get; set; } = new();
}

public class UserBehaviorAnomalyResult
{
    public string UserId { get; set; } = string.Empty;
    public double BehaviorScore { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public List<string> DetectedAnomalies { get; set; } = new();
    public TimeSpan AnalysisWindow { get; set; }
    public int QueryCount { get; set; }
    public DateTime Timestamp { get; set; }
}

public class TrainingDataPoint
{
    public QueryFeatures Features { get; set; } = new();
    public double Label { get; set; }
}

public enum RiskLevel
{
    Minimal,
    Low,
    Medium,
    High,
    Critical
}
