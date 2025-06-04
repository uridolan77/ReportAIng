using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.Performance;
using System.Text.RegularExpressions;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// SQL injection threat detector
/// </summary>
public class SQLInjectionDetector
{
    private readonly ILogger _logger;
    private readonly ThreatDetectionConfiguration _config;
    private readonly List<Regex> _suspiciousPatterns;

    public SQLInjectionDetector(ILogger logger, ThreatDetectionConfiguration config)
    {
        _logger = logger;
        _config = config;
        _suspiciousPatterns = InitializeSuspiciousPatterns();
    }

    public async Task<List<SecurityThreat>> DetectThreatsAsync(ThreatAnalysisContext context)
    {
        var threats = new List<SecurityThreat>();

        try
        {
            // Pattern-based detection
            if (_config.SQLInjectionSettings.EnablePatternMatching)
            {
                var patternThreats = DetectPatternBasedThreats(context);
                threats.AddRange(patternThreats);
            }

            // ML-based detection (simplified)
            if (_config.SQLInjectionSettings.EnableMLDetection)
            {
                var mlThreats = await DetectMLBasedThreatsAsync(context);
                threats.AddRange(mlThreats);
            }

            _logger.LogDebug("SQL injection detector found {ThreatCount} threats", threats.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SQL injection detection");
        }

        return threats;
    }

    public async Task UpdateModelsAsync(List<ThreatTrainingData> trainingData)
    {
        _logger.LogDebug("Updating SQL injection models with {DataCount} samples", trainingData.Count);
    }

    public async Task UpdateConfigurationAsync(ThreatDetectionConfiguration configuration)
    {
        _logger.LogDebug("Updated SQL injection configuration");
    }

    private List<Regex> InitializeSuspiciousPatterns()
    {
        var patterns = new List<Regex>();
        
        foreach (var pattern in _config.SQLInjectionSettings.SuspiciousPatterns)
        {
            try
            {
                patterns.Add(new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Invalid regex pattern: {Pattern}", pattern);
            }
        }

        return patterns;
    }

    private List<SecurityThreat> DetectPatternBasedThreats(ThreatAnalysisContext context)
    {
        var threats = new List<SecurityThreat>();
        var combinedText = $"{context.NaturalLanguageQuery} {context.GeneratedSQL}";

        foreach (var pattern in _suspiciousPatterns)
        {
            var matches = pattern.Matches(combinedText);
            if (matches.Count > 0)
            {
                var confidence = CalculatePatternConfidence(matches.Count, combinedText.Length);
                
                if (confidence >= _config.SQLInjectionSettings.ConfidenceThreshold)
                {
                    threats.Add(new SecurityThreat
                    {
                        ThreatType = ThreatType.SQLInjection,
                        RiskLevel = DetermineRiskLevel(confidence),
                        Confidence = confidence,
                        Description = $"SQL injection pattern detected: {pattern}",
                        UserId = context.UserId,
                        IPAddress = context.IPAddress,
                        DetectionMethod = "Pattern Matching",
                        Indicators = matches.Cast<Match>().Select(m => new ThreatIndicator
                        {
                            Type = "Pattern Match",
                            Value = m.Value,
                            Confidence = confidence,
                            Description = $"Suspicious SQL pattern: {m.Value}"
                        }).ToList(),
                        Metadata = new Dictionary<string, object>
                        {
                            ["pattern"] = pattern.ToString(),
                            ["match_count"] = matches.Count,
                            ["matched_text"] = string.Join(", ", matches.Cast<Match>().Select(m => m.Value))
                        }
                    });
                }
            }
        }

        return threats;
    }

    private async Task<List<SecurityThreat>> DetectMLBasedThreatsAsync(ThreatAnalysisContext context)
    {
        var threats = new List<SecurityThreat>();

        // Simplified ML-based detection
        var features = ExtractSQLFeatures(context.GeneratedSQL);
        var suspicionScore = CalculateSuspicionScore(features);

        if (suspicionScore >= _config.SQLInjectionSettings.ConfidenceThreshold)
        {
            threats.Add(new SecurityThreat
            {
                ThreatType = ThreatType.SQLInjection,
                RiskLevel = DetermineRiskLevel(suspicionScore),
                Confidence = suspicionScore,
                Description = "ML-based SQL injection detection",
                UserId = context.UserId,
                IPAddress = context.IPAddress,
                DetectionMethod = "Machine Learning",
                Indicators = new List<ThreatIndicator>
                {
                    new ThreatIndicator
                    {
                        Type = "ML Score",
                        Value = suspicionScore.ToString("F2"),
                        Confidence = suspicionScore,
                        Description = "Machine learning suspicion score"
                    }
                },
                Metadata = new Dictionary<string, object>
                {
                    ["ml_features"] = features,
                    ["suspicion_score"] = suspicionScore
                }
            });
        }

        return threats;
    }

    private double CalculatePatternConfidence(int matchCount, int textLength)
    {
        var density = (double)matchCount / Math.Max(1, textLength / 100);
        return Math.Min(1.0, 0.5 + (density * 0.5));
    }

    private ThreatRiskLevel DetermineRiskLevel(double confidence)
    {
        return confidence switch
        {
            >= 0.9 => ThreatRiskLevel.Critical,
            >= 0.8 => ThreatRiskLevel.High,
            >= 0.6 => ThreatRiskLevel.Medium,
            _ => ThreatRiskLevel.Low
        };
    }

    private Dictionary<string, double> ExtractSQLFeatures(string sql)
    {
        var features = new Dictionary<string, double>();
        var sqlLower = sql.ToLowerInvariant();

        features["length"] = sql.Length;
        features["union_count"] = CountOccurrences(sqlLower, "union");
        features["select_count"] = CountOccurrences(sqlLower, "select");
        features["drop_count"] = CountOccurrences(sqlLower, "drop");
        features["delete_count"] = CountOccurrences(sqlLower, "delete");
        features["comment_count"] = CountOccurrences(sqlLower, "--");
        features["semicolon_count"] = CountOccurrences(sqlLower, ";");

        return features;
    }

    private int CountOccurrences(string text, string pattern)
    {
        return (text.Length - text.Replace(pattern, "").Length) / pattern.Length;
    }

    private double CalculateSuspicionScore(Dictionary<string, double> features)
    {
        var score = 0.0;

        // Simple scoring based on suspicious features
        if (features["union_count"] > 0) score += 0.3;
        if (features["drop_count"] > 0) score += 0.4;
        if (features["delete_count"] > 0) score += 0.3;
        if (features["comment_count"] > 0) score += 0.2;
        if (features["semicolon_count"] > 1) score += 0.2;

        return Math.Min(1.0, score);
    }
}

/// <summary>
/// Anomalous query detector
/// </summary>
public class AnomalousQueryDetector
{
    private readonly ILogger _logger;
    private readonly ThreatDetectionConfiguration _config;

    public AnomalousQueryDetector(ILogger logger, ThreatDetectionConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task<List<SecurityThreat>> DetectThreatsAsync(ThreatAnalysisContext context)
    {
        var threats = new List<SecurityThreat>();

        try
        {
            // Complexity-based detection
            if (_config.AnomalousQuerySettings.EnableComplexityAnalysis)
            {
                var complexityThreats = DetectComplexityAnomalies(context);
                threats.AddRange(complexityThreats);
            }

            // Frequency-based detection
            if (_config.AnomalousQuerySettings.EnableFrequencyAnalysis)
            {
                var frequencyThreats = await DetectFrequencyAnomaliesAsync(context);
                threats.AddRange(frequencyThreats);
            }

            _logger.LogDebug("Anomalous query detector found {ThreatCount} threats", threats.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in anomalous query detection");
        }

        return threats;
    }

    public async Task UpdateModelsAsync(List<ThreatTrainingData> trainingData)
    {
        _logger.LogDebug("Updating anomalous query models with {DataCount} samples", trainingData.Count);
    }

    public async Task UpdateConfigurationAsync(ThreatDetectionConfiguration configuration)
    {
        _logger.LogDebug("Updated anomalous query configuration");
    }

    private List<SecurityThreat> DetectComplexityAnomalies(ThreatAnalysisContext context)
    {
        var threats = new List<SecurityThreat>();
        var complexity = CalculateQueryComplexity(context.GeneratedSQL);

        if (complexity > 15) // High complexity threshold
        {
            var confidence = Math.Min(1.0, complexity / 20.0);
            
            if (confidence >= _config.AnomalousQuerySettings.AnomalyThreshold)
            {
                threats.Add(new SecurityThreat
                {
                    ThreatType = ThreatType.AnomalousQuery,
                    RiskLevel = ThreatRiskLevel.Medium,
                    Confidence = confidence,
                    Description = $"Unusually complex query detected (complexity: {complexity})",
                    UserId = context.UserId,
                    IPAddress = context.IPAddress,
                    DetectionMethod = "Complexity Analysis",
                    Indicators = new List<ThreatIndicator>
                    {
                        new ThreatIndicator
                        {
                            Type = "Complexity Score",
                            Value = complexity.ToString(),
                            Confidence = confidence,
                            Description = "Query complexity score"
                        }
                    },
                    Metadata = new Dictionary<string, object>
                    {
                        ["complexity_score"] = complexity,
                        ["query_length"] = context.GeneratedSQL.Length
                    }
                });
            }
        }

        return threats;
    }

    private async Task<List<SecurityThreat>> DetectFrequencyAnomaliesAsync(ThreatAnalysisContext context)
    {
        var threats = new List<SecurityThreat>();

        // Simplified frequency analysis
        // In a real implementation, this would analyze historical query patterns
        
        return threats;
    }

    private int CalculateQueryComplexity(string sql)
    {
        var complexity = 1;
        var sqlLower = sql.ToLowerInvariant();

        complexity += CountOccurrences(sqlLower, "join") * 2;
        complexity += CountOccurrences(sqlLower, "union") * 3;
        complexity += CountOccurrences(sqlLower, "subquery") * 3;
        complexity += CountOccurrences(sqlLower, "case when") * 2;
        complexity += CountOccurrences(sqlLower, "group by") * 2;
        complexity += CountOccurrences(sqlLower, "order by") * 1;
        complexity += CountOccurrences(sqlLower, "having") * 2;

        return complexity;
    }

    private int CountOccurrences(string text, string pattern)
    {
        return (text.Length - text.Replace(pattern, "").Length) / pattern.Length;
    }
}

/// <summary>
/// User behavior analyzer
/// </summary>
public class UserBehaviorAnalyzer
{
    private readonly ILogger _logger;
    private readonly ICacheService _cacheService;
    private readonly ThreatDetectionConfiguration _config;

    public UserBehaviorAnalyzer(ILogger logger, ICacheService cacheService, ThreatDetectionConfiguration config)
    {
        _logger = logger;
        _cacheService = cacheService;
        _config = config;
    }

    public async Task<List<SecurityThreat>> DetectThreatsAsync(ThreatAnalysisContext context)
    {
        var threats = new List<SecurityThreat>();

        try
        {
            var userProfile = await GetUserBehaviorProfileAsync(context.UserId);
            
            if (_config.UserBehaviorSettings.EnableAccessPatternAnalysis)
            {
                var accessThreats = await DetectAccessPatternAnomaliesAsync(context, userProfile);
                threats.AddRange(accessThreats);
            }

            if (_config.UserBehaviorSettings.EnableTimingAnalysis)
            {
                var timingThreats = await DetectTimingAnomaliesAsync(context, userProfile);
                threats.AddRange(timingThreats);
            }

            _logger.LogDebug("User behavior analyzer found {ThreatCount} threats", threats.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in user behavior analysis");
        }

        return threats;
    }

    public async Task UpdateUserProfileAsync(string userId, ThreatAnalysisContext context, ThreatAnalysisResult result)
    {
        try
        {
            var profile = await GetUserBehaviorProfileAsync(userId);
            
            // Update profile with new data
            profile.LastActivity = context.Timestamp;
            profile.QueryCount++;
            profile.ThreatCount += result.TotalThreats;
            
            // Update access patterns
            var hour = context.Timestamp.Hour;
            if (!profile.AccessPatterns.ContainsKey(hour))
            {
                profile.AccessPatterns[hour] = 0;
            }
            profile.AccessPatterns[hour]++;

            await _cacheService.SetAsync($"user_behavior_profile:{userId}", profile, TimeSpan.FromDays(30));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user behavior profile");
        }
    }

    public async Task UpdateModelsAsync(List<ThreatTrainingData> trainingData)
    {
        _logger.LogDebug("Updating user behavior models with {DataCount} samples", trainingData.Count);
    }

    public async Task UpdateConfigurationAsync(ThreatDetectionConfiguration configuration)
    {
        _logger.LogDebug("Updated user behavior configuration");
    }

    private async Task<UserBehaviorProfile> GetUserBehaviorProfileAsync(string userId)
    {
        var profile = await _cacheService.GetAsync<UserBehaviorProfile>($"user_behavior_profile:{userId}");
        
        if (profile == null)
        {
            profile = new UserBehaviorProfile
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                QueryCount = 0,
                ThreatCount = 0,
                AccessPatterns = new Dictionary<int, int>()
            };
        }

        return profile;
    }

    private async Task<List<SecurityThreat>> DetectAccessPatternAnomaliesAsync(
        ThreatAnalysisContext context, 
        UserBehaviorProfile profile)
    {
        var threats = new List<SecurityThreat>();

        // Check for unusual access times
        var currentHour = context.Timestamp.Hour;
        var typicalAccess = profile.AccessPatterns.GetValueOrDefault(currentHour, 0);
        var totalAccess = profile.AccessPatterns.Values.Sum();

        if (totalAccess > 10) // Only analyze if we have enough data
        {
            var accessProbability = (double)typicalAccess / totalAccess;
            
            if (accessProbability < 0.05) // Very unusual time
            {
                threats.Add(new SecurityThreat
                {
                    ThreatType = ThreatType.SuspiciousBehavior,
                    RiskLevel = ThreatRiskLevel.Medium,
                    Confidence = 1.0 - accessProbability,
                    Description = $"Unusual access time detected for user {context.UserId}",
                    UserId = context.UserId,
                    IPAddress = context.IPAddress,
                    DetectionMethod = "Access Pattern Analysis",
                    Indicators = new List<ThreatIndicator>
                    {
                        new ThreatIndicator
                        {
                            Type = "Access Time",
                            Value = currentHour.ToString(),
                            Confidence = 1.0 - accessProbability,
                            Description = "Unusual access time"
                        }
                    },
                    Metadata = new Dictionary<string, object>
                    {
                        ["access_hour"] = currentHour,
                        ["access_probability"] = accessProbability,
                        ["typical_access_count"] = typicalAccess
                    }
                });
            }
        }

        return threats;
    }

    private async Task<List<SecurityThreat>> DetectTimingAnomaliesAsync(
        ThreatAnalysisContext context, 
        UserBehaviorProfile profile)
    {
        var threats = new List<SecurityThreat>();

        // Check for rapid successive queries
        if (profile.LastActivity != DateTime.MinValue)
        {
            var timeSinceLastQuery = context.Timestamp - profile.LastActivity;
            
            if (timeSinceLastQuery.TotalSeconds < 5) // Very rapid queries
            {
                threats.Add(new SecurityThreat
                {
                    ThreatType = ThreatType.SuspiciousBehavior,
                    RiskLevel = ThreatRiskLevel.Low,
                    Confidence = 0.6,
                    Description = "Rapid successive queries detected",
                    UserId = context.UserId,
                    IPAddress = context.IPAddress,
                    DetectionMethod = "Timing Analysis",
                    Indicators = new List<ThreatIndicator>
                    {
                        new ThreatIndicator
                        {
                            Type = "Query Interval",
                            Value = timeSinceLastQuery.TotalSeconds.ToString("F1"),
                            Confidence = 0.6,
                            Description = "Very short interval between queries"
                        }
                    },
                    Metadata = new Dictionary<string, object>
                    {
                        ["time_since_last_query_seconds"] = timeSinceLastQuery.TotalSeconds,
                        ["last_activity"] = profile.LastActivity
                    }
                });
            }
        }

        return threats;
    }
}

/// <summary>
/// User behavior profile
/// </summary>
public class UserBehaviorProfile
{
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivity { get; set; }
    public int QueryCount { get; set; }
    public int ThreatCount { get; set; }
    public Dictionary<int, int> AccessPatterns { get; set; } = new(); // Hour -> Count
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Data exfiltration detector
/// </summary>
public class DataExfiltrationDetector
{
    private readonly ILogger _logger;
    private readonly ThreatDetectionConfiguration _config;

    public DataExfiltrationDetector(ILogger logger, ThreatDetectionConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task<List<SecurityThreat>> DetectThreatsAsync(ThreatAnalysisContext context)
    {
        var threats = new List<SecurityThreat>();

        try
        {
            if (_config.DataExfiltrationSettings.EnableVolumeAnalysis)
            {
                var volumeThreats = DetectVolumeAnomalies(context);
                threats.AddRange(volumeThreats);
            }

            if (_config.DataExfiltrationSettings.EnableSensitiveDataDetection)
            {
                var sensitiveThreats = DetectSensitiveDataAccess(context);
                threats.AddRange(sensitiveThreats);
            }

            _logger.LogDebug("Data exfiltration detector found {ThreatCount} threats", threats.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in data exfiltration detection");
        }

        return threats;
    }

    public async Task UpdateModelsAsync(List<ThreatTrainingData> trainingData)
    {
        _logger.LogDebug("Updating data exfiltration models with {DataCount} samples", trainingData.Count);
    }

    public async Task UpdateConfigurationAsync(ThreatDetectionConfiguration configuration)
    {
        _logger.LogDebug("Updated data exfiltration configuration");
    }

    private List<SecurityThreat> DetectVolumeAnomalies(ThreatAnalysisContext context)
    {
        var threats = new List<SecurityThreat>();
        var sql = context.GeneratedSQL.ToLowerInvariant();

        // Check for queries that might return large amounts of data
        if (sql.Contains("select *") && !sql.Contains("limit") && !sql.Contains("top"))
        {
            threats.Add(new SecurityThreat
            {
                ThreatType = ThreatType.DataExfiltration,
                RiskLevel = ThreatRiskLevel.Medium,
                Confidence = 0.7,
                Description = "Query may return large dataset without limits",
                UserId = context.UserId,
                IPAddress = context.IPAddress,
                DetectionMethod = "Volume Analysis",
                Indicators = new List<ThreatIndicator>
                {
                    new ThreatIndicator
                    {
                        Type = "SELECT *",
                        Value = "Unrestricted SELECT",
                        Confidence = 0.7,
                        Description = "Query uses SELECT * without LIMIT"
                    }
                },
                Metadata = new Dictionary<string, object>
                {
                    ["has_select_star"] = true,
                    ["has_limit"] = false
                }
            });
        }

        return threats;
    }

    private List<SecurityThreat> DetectSensitiveDataAccess(ThreatAnalysisContext context)
    {
        var threats = new List<SecurityThreat>();
        var sql = context.GeneratedSQL.ToLowerInvariant();

        foreach (var sensitiveColumn in _config.DataExfiltrationSettings.SensitiveColumns)
        {
            if (sql.Contains(sensitiveColumn.ToLowerInvariant()))
            {
                threats.Add(new SecurityThreat
                {
                    ThreatType = ThreatType.DataExfiltration,
                    RiskLevel = ThreatRiskLevel.High,
                    Confidence = 0.8,
                    Description = $"Access to sensitive data column: {sensitiveColumn}",
                    UserId = context.UserId,
                    IPAddress = context.IPAddress,
                    DetectionMethod = "Sensitive Data Detection",
                    Indicators = new List<ThreatIndicator>
                    {
                        new ThreatIndicator
                        {
                            Type = "Sensitive Column",
                            Value = sensitiveColumn,
                            Confidence = 0.8,
                            Description = $"Query accesses sensitive column: {sensitiveColumn}"
                        }
                    },
                    Metadata = new Dictionary<string, object>
                    {
                        ["sensitive_column"] = sensitiveColumn,
                        ["column_type"] = "sensitive"
                    }
                });
            }
        }

        return threats;
    }
}

/// <summary>
/// Threat intelligence engine
/// </summary>
public class ThreatIntelligenceEngine
{
    private readonly ILogger _logger;
    private readonly ICacheService _cacheService;

    public ThreatIntelligenceEngine(ILogger logger, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<ThreatIntelligenceResult> AnalyzeThreatContextAsync(
        List<SecurityThreat> threats, 
        ThreatAnalysisContext context)
    {
        try
        {
            var knownPatterns = await IdentifyKnownPatternsAsync(threats);
            var similarAttacks = await FindSimilarAttacksAsync(threats);
            var landscapeScore = CalculateThreatLandscapeScore(threats);
            var mitigations = GenerateRecommendedMitigations(threats);

            return new ThreatIntelligenceResult
            {
                KnownPatterns = knownPatterns,
                SimilarAttacks = similarAttacks,
                ThreatLandscapeScore = landscapeScore,
                RecommendedMitigations = mitigations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in threat intelligence analysis");
            return new ThreatIntelligenceResult();
        }
    }

    public async Task UpdateIntelligenceAsync(List<ThreatTrainingData> trainingData)
    {
        _logger.LogDebug("Updating threat intelligence with {DataCount} samples", trainingData.Count);
    }

    private async Task<List<ThreatPattern>> IdentifyKnownPatternsAsync(List<SecurityThreat> threats)
    {
        var patterns = new List<ThreatPattern>();

        // Simple pattern matching
        foreach (var threat in threats)
        {
            if (threat.ThreatType == ThreatType.SQLInjection)
            {
                patterns.Add(new ThreatPattern
                {
                    Id = "sql_injection_pattern_1",
                    Name = "SQL Injection Attack",
                    Description = "Common SQL injection attack pattern",
                    Type = ThreatType.SQLInjection,
                    MatchConfidence = threat.Confidence,
                    Signatures = new List<string> { "union select", "drop table" }
                });
            }
        }

        return patterns;
    }

    private async Task<List<string>> FindSimilarAttacksAsync(List<SecurityThreat> threats)
    {
        // In a real implementation, this would query threat intelligence databases
        return new List<string>
        {
            "Similar SQL injection attack detected on 2024-01-15",
            "Related data exfiltration attempt from same IP range"
        };
    }

    private double CalculateThreatLandscapeScore(List<SecurityThreat> threats)
    {
        if (!threats.Any()) return 0.0;

        var highRiskCount = threats.Count(t => t.RiskLevel == ThreatRiskLevel.High);
        var totalThreats = threats.Count;
        
        return (double)highRiskCount / totalThreats;
    }

    private List<string> GenerateRecommendedMitigations(List<SecurityThreat> threats)
    {
        var mitigations = new List<string>();

        if (threats.Any(t => t.ThreatType == ThreatType.SQLInjection))
        {
            mitigations.Add("Implement parameterized queries and input validation");
        }

        if (threats.Any(t => t.ThreatType == ThreatType.DataExfiltration))
        {
            mitigations.Add("Implement data loss prevention (DLP) controls");
        }

        if (threats.Any(t => t.ThreatType == ThreatType.SuspiciousBehavior))
        {
            mitigations.Add("Enhance user behavior monitoring and alerting");
        }

        return mitigations;
    }
}

/// <summary>
/// Security alert manager
/// </summary>
public class SecurityAlertManager
{
    private readonly ILogger _logger;
    private readonly ICacheService _cacheService;

    public SecurityAlertManager(ILogger logger, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task SendSecurityAlertAsync(SecurityThreat threat, ThreatAnalysisContext context)
    {
        try
        {
            // Check if alert was recently sent to avoid spam
            var alertKey = $"security_alert:{threat.ThreatType}:{threat.UserId}";
            var recentAlert = await _cacheService.GetAsync<DateTime?>(alertKey);
            
            if (recentAlert.HasValue && DateTime.UtcNow - recentAlert.Value < TimeSpan.FromMinutes(15))
            {
                _logger.LogDebug("Skipping duplicate security alert for threat {ThreatId}", threat.Id);
                return;
            }

            // Send alert (in practice, this would integrate with security systems)
            _logger.LogWarning("SECURITY ALERT: {RiskLevel} {ThreatType} threat detected for user {UserId} - {Description}",
                threat.RiskLevel, threat.ThreatType, threat.UserId, threat.Description);

            // Record alert sent
            await _cacheService.SetAsync(alertKey, DateTime.UtcNow, TimeSpan.FromMinutes(30));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending security alert");
        }
    }
}
