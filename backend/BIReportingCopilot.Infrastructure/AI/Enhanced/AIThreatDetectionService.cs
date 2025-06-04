using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Performance;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

// Supporting models and classes for AI Threat Detection

/// <summary>
/// Threat analysis context
/// </summary>
public class ThreatAnalysisContext
{
    public string NaturalLanguageQuery { get; set; } = string.Empty;
    public string GeneratedSQL { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? IPAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> AdditionalContext { get; set; } = new();
}

/// <summary>
/// Security threat detection result
/// </summary>
public class SecurityThreat
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public ThreatType ThreatType { get; set; }
    public ThreatRiskLevel RiskLevel { get; set; }
    public double Confidence { get; set; }
    public double PriorityScore { get; set; }
    public string Description { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? IPAddress { get; set; }
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public string DetectionMethod { get; set; } = string.Empty;
    public List<ThreatIndicator> Indicators { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Threat indicator
/// </summary>
public class ThreatIndicator
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Threat analysis result
/// </summary>
public class ThreatAnalysisResult
{
    public string AnalysisId { get; set; } = string.Empty;
    public List<SecurityThreat> Threats { get; set; } = new();
    public int TotalThreats { get; set; }
    public int HighRiskCount { get; set; }
    public int MediumRiskCount { get; set; }
    public int LowRiskCount { get; set; }
    public double OverallRiskScore { get; set; }
    public ThreatIntelligenceResult ThreatIntelligence { get; set; } = new();
    public List<SecurityRecommendation> RecommendedActions { get; set; } = new();
    public DateTime AnalysisTimestamp { get; set; }
    public long ProcessingTimeMs { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Threat intelligence result
/// </summary>
public class ThreatIntelligenceResult
{
    public List<ThreatPattern> KnownPatterns { get; set; } = new();
    public List<string> SimilarAttacks { get; set; } = new();
    public double ThreatLandscapeScore { get; set; }
    public List<string> RecommendedMitigations { get; set; } = new();
}

/// <summary>
/// Threat pattern
/// </summary>
public class ThreatPattern
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ThreatType Type { get; set; }
    public double MatchConfidence { get; set; }
    public List<string> Signatures { get; set; } = new();
}

/// <summary>
/// Security recommendation
/// </summary>
public class SecurityRecommendation
{
    public RecommendationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecommendationPriority Priority { get; set; }
    public string EstimatedEffort { get; set; } = string.Empty;
    public List<string> AffectedThreats { get; set; } = new();
}

/// <summary>
/// Threat statistics
/// </summary>
public class ThreatStatistics
{
    public TimeSpan Period { get; set; }
    public int TotalThreats { get; set; }
    public Dictionary<ThreatType, int> ThreatsByType { get; set; } = new();
    public Dictionary<ThreatRiskLevel, int> ThreatsByRiskLevel { get; set; } = new();
    public Dictionary<DateTime, int> ThreatsByDay { get; set; } = new();
    public List<UserThreatSummary> TopTargetedUsers { get; set; } = new();
    public List<ThreatSourceSummary> TopThreatSources { get; set; } = new();
    public ThreatTrendAnalysis TrendAnalysis { get; set; } = new();
    public double MitigationEffectiveness { get; set; }
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// User threat summary
/// </summary>
public class UserThreatSummary
{
    public string UserId { get; set; } = string.Empty;
    public int ThreatCount { get; set; }
    public ThreatRiskLevel HighestRiskLevel { get; set; }
}

/// <summary>
/// Threat source summary
/// </summary>
public class ThreatSourceSummary
{
    public string IPAddress { get; set; } = string.Empty;
    public int ThreatCount { get; set; }
    public List<ThreatType> ThreatTypes { get; set; } = new();
}

/// <summary>
/// Threat trend analysis
/// </summary>
public class ThreatTrendAnalysis
{
    public TrendDirection Direction { get; set; }
    public int RecentThreatCount { get; set; }
    public int HistoricalThreatCount { get; set; }
    public double TrendStrength { get; set; }
}

/// <summary>
/// Threat training data
/// </summary>
public class ThreatTrainingData
{
    public string Query { get; set; } = string.Empty;
    public string SQL { get; set; } = string.Empty;
    public bool IsThreat { get; set; }
    public ThreatType? ThreatType { get; set; }
    public ThreatRiskLevel? RiskLevel { get; set; }
    public List<string> Labels { get; set; } = new();
    public Dictionary<string, object> Features { get; set; } = new();
}

/// <summary>
/// Threat detection configuration
/// </summary>
public class ThreatDetectionConfiguration
{
    public bool EnableSQLInjectionDetection { get; set; } = true;
    public bool EnableAnomalousQueryDetection { get; set; } = true;
    public bool EnableUserBehaviorAnalysis { get; set; } = true;
    public bool EnableDataExfiltrationDetection { get; set; } = true;
    public bool EnableRealTimeAlerts { get; set; } = true;
    public double MinimumThreatConfidence { get; set; } = 0.7;
    public int MaxThreatsPerAnalysis { get; set; } = 50;
    public TimeSpan AlertCooldownPeriod { get; set; } = TimeSpan.FromMinutes(15);
    public SQLInjectionSettings SQLInjectionSettings { get; set; } = new();
    public AnomalousQuerySettings AnomalousQuerySettings { get; set; } = new();
    public UserBehaviorSettings UserBehaviorSettings { get; set; } = new();
    public DataExfiltrationSettings DataExfiltrationSettings { get; set; } = new();
}

/// <summary>
/// SQL injection detection settings
/// </summary>
public class SQLInjectionSettings
{
    public double ConfidenceThreshold { get; set; } = 0.8;
    public List<string> SuspiciousPatterns { get; set; } = new()
    {
        "union select", "drop table", "delete from", "insert into",
        "update set", "exec ", "execute", "sp_", "xp_", "'; --"
    };
    public bool EnablePatternMatching { get; set; } = true;
    public bool EnableMLDetection { get; set; } = true;
}

/// <summary>
/// Anomalous query detection settings
/// </summary>
public class AnomalousQuerySettings
{
    public double AnomalyThreshold { get; set; } = 0.75;
    public int BaselineWindowDays { get; set; } = 30;
    public bool EnableComplexityAnalysis { get; set; } = true;
    public bool EnableFrequencyAnalysis { get; set; } = true;
}

/// <summary>
/// User behavior analysis settings
/// </summary>
public class UserBehaviorSettings
{
    public double BehaviorDeviationThreshold { get; set; } = 0.7;
    public int LearningPeriodDays { get; set; } = 14;
    public bool EnableAccessPatternAnalysis { get; set; } = true;
    public bool EnableTimingAnalysis { get; set; } = true;
}

/// <summary>
/// Data exfiltration detection settings
/// </summary>
public class DataExfiltrationSettings
{
    public double ExfiltrationThreshold { get; set; } = 0.8;
    public int MaxRowsPerQuery { get; set; } = 10000;
    public List<string> SensitiveColumns { get; set; } = new()
    {
        "password", "ssn", "credit_card", "email", "phone"
    };
    public bool EnableVolumeAnalysis { get; set; } = true;
    public bool EnableSensitiveDataDetection { get; set; } = true;
}

/// <summary>
/// Threat types
/// </summary>
public enum ThreatType
{
    SQLInjection,
    AnomalousQuery,
    SuspiciousBehavior,
    DataExfiltration,
    UnauthorizedAccess,
    PrivilegeEscalation,
    Correlated,
    Unknown
}

/// <summary>
/// Threat risk levels
/// </summary>
public enum ThreatRiskLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

/// <summary>
/// AI-powered threat detection service for security monitoring
/// Implements Enhancement 25: AI-Powered Threat Detection
/// </summary>
public class AIThreatDetectionService
{
    private readonly ILogger<AIThreatDetectionService> _logger;
    private readonly ICacheService _cacheService;
    private readonly SQLInjectionDetector _sqlInjectionDetector;
    private readonly AnomalousQueryDetector _anomalousQueryDetector;
    private readonly UserBehaviorAnalyzer _userBehaviorAnalyzer;
    private readonly DataExfiltrationDetector _dataExfiltrationDetector;
    private readonly ThreatIntelligenceEngine _threatIntelligence;
    private readonly SecurityAlertManager _alertManager;
    private readonly ThreatDetectionConfiguration _config;

    public AIThreatDetectionService(
        ILogger<AIThreatDetectionService> logger,
        ICacheService cacheService,
        IOptions<ThreatDetectionConfiguration> config)
    {
        _logger = logger;
        _cacheService = cacheService;
        _config = config.Value;

        _sqlInjectionDetector = new SQLInjectionDetector(logger, config.Value);
        _anomalousQueryDetector = new AnomalousQueryDetector(logger, config.Value);
        _userBehaviorAnalyzer = new UserBehaviorAnalyzer(logger, cacheService, config.Value);
        _dataExfiltrationDetector = new DataExfiltrationDetector(logger, config.Value);
        _threatIntelligence = new ThreatIntelligenceEngine(logger, cacheService);
        _alertManager = new SecurityAlertManager(logger, cacheService);
    }

    /// <summary>
    /// Analyze query for security threats using multiple detection methods
    /// </summary>
    public async Task<ThreatAnalysisResult> AnalyzeQueryThreatAsync(
        string naturalLanguageQuery,
        string generatedSQL,
        string userId,
        string? ipAddress = null,
        string? userAgent = null)
    {
        try
        {
            _logger.LogDebug("Starting threat analysis for user {UserId}: {Query}", userId, naturalLanguageQuery);

            var analysisContext = new ThreatAnalysisContext
            {
                NaturalLanguageQuery = naturalLanguageQuery,
                GeneratedSQL = generatedSQL,
                UserId = userId,
                IPAddress = ipAddress,
                UserAgent = userAgent,
                Timestamp = DateTime.UtcNow
            };

            // Run multiple threat detection methods in parallel
            var detectionTasks = new List<Task<List<SecurityThreat>>>();

            if (_config.EnableSQLInjectionDetection)
            {
                detectionTasks.Add(_sqlInjectionDetector.DetectThreatsAsync(analysisContext));
            }

            if (_config.EnableAnomalousQueryDetection)
            {
                detectionTasks.Add(_anomalousQueryDetector.DetectThreatsAsync(analysisContext));
            }

            if (_config.EnableUserBehaviorAnalysis)
            {
                detectionTasks.Add(_userBehaviorAnalyzer.DetectThreatsAsync(analysisContext));
            }

            if (_config.EnableDataExfiltrationDetection)
            {
                detectionTasks.Add(_dataExfiltrationDetector.DetectThreatsAsync(analysisContext));
            }

            // Wait for all detection methods to complete
            var detectionResults = await Task.WhenAll(detectionTasks);
            var allThreats = detectionResults.SelectMany(threats => threats).ToList();

            // Correlate and prioritize threats
            var correlatedThreats = await CorrelateThreatSignalsAsync(allThreats, analysisContext);
            var prioritizedThreats = await PrioritizeThreatsAsync(correlatedThreats);

            // Generate threat intelligence insights
            var threatIntelligence = await _threatIntelligence.AnalyzeThreatContextAsync(prioritizedThreats, analysisContext);

            // Create analysis result
            var result = new ThreatAnalysisResult
            {
                AnalysisId = Guid.NewGuid().ToString(),
                Threats = prioritizedThreats,
                TotalThreats = prioritizedThreats.Count,
                HighRiskCount = prioritizedThreats.Count(t => t.RiskLevel == ThreatRiskLevel.High),
                MediumRiskCount = prioritizedThreats.Count(t => t.RiskLevel == ThreatRiskLevel.Medium),
                LowRiskCount = prioritizedThreats.Count(t => t.RiskLevel == ThreatRiskLevel.Low),
                OverallRiskScore = CalculateOverallRiskScore(prioritizedThreats),
                ThreatIntelligence = threatIntelligence,
                RecommendedActions = await GenerateSecurityRecommendationsAsync(prioritizedThreats, analysisContext),
                AnalysisTimestamp = DateTime.UtcNow,
                ProcessingTimeMs = 0, // Would be calculated in real implementation
                Metadata = new Dictionary<string, object>
                {
                    ["detection_methods_used"] = GetEnabledDetectionMethods(),
                    ["user_id"] = userId,
                    ["ip_address"] = ipAddress ?? "unknown",
                    ["analysis_version"] = "2.0"
                }
            };

            // Process security alerts for high-risk threats
            await ProcessSecurityAlertsAsync(prioritizedThreats, analysisContext);

            // Update user behavior profile
            await _userBehaviorAnalyzer.UpdateUserProfileAsync(userId, analysisContext, result);

            _logger.LogDebug("Threat analysis completed: {ThreatCount} threats found (Risk Score: {RiskScore:F2})",
                result.TotalThreats, result.OverallRiskScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in threat analysis");
            return new ThreatAnalysisResult
            {
                AnalysisId = Guid.NewGuid().ToString(),
                Threats = new List<SecurityThreat>(),
                AnalysisTimestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object> { ["error"] = true }
            };
        }
    }

    /// <summary>
    /// Get threat detection statistics and trends
    /// </summary>
    public async Task<ThreatStatistics> GetThreatStatisticsAsync(TimeSpan period, string? userId = null)
    {
        try
        {
            var endTime = DateTime.UtcNow;
            var startTime = endTime - period;

            var threats = await GetHistoricalThreatsAsync(startTime, endTime, userId);

            return new ThreatStatistics
            {
                Period = period,
                TotalThreats = threats.Count,
                ThreatsByType = GroupThreatsByType(threats),
                ThreatsByRiskLevel = GroupThreatsByRiskLevel(threats),
                ThreatsByDay = GroupThreatsByDay(threats),
                TopTargetedUsers = GetTopTargetedUsers(threats),
                TopThreatSources = GetTopThreatSources(threats),
                TrendAnalysis = AnalyzeThreatTrends(threats),
                MitigationEffectiveness = CalculateMitigationEffectiveness(threats),
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating threat statistics");
            return new ThreatStatistics
            {
                Period = period,
                GeneratedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Update threat detection models with new data
    /// </summary>
    public async Task UpdateThreatModelsAsync(List<ThreatTrainingData> trainingData)
    {
        try
        {
            _logger.LogInformation("Updating threat detection models with {DataCount} training samples",
                trainingData.Count);

            // Update SQL injection detection models
            await _sqlInjectionDetector.UpdateModelsAsync(trainingData);

            // Update anomalous query detection models
            await _anomalousQueryDetector.UpdateModelsAsync(trainingData);

            // Update user behavior models
            await _userBehaviorAnalyzer.UpdateModelsAsync(trainingData);

            // Update data exfiltration detection models
            await _dataExfiltrationDetector.UpdateModelsAsync(trainingData);

            // Update threat intelligence
            await _threatIntelligence.UpdateIntelligenceAsync(trainingData);

            _logger.LogInformation("Threat detection models updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating threat detection models");
        }
    }

    /// <summary>
    /// Configure threat detection rules and thresholds
    /// </summary>
    public async Task UpdateThreatConfigurationAsync(ThreatDetectionConfiguration configuration)
    {
        try
        {
            _logger.LogInformation("Updating threat detection configuration");

            // Update detection thresholds
            await _sqlInjectionDetector.UpdateConfigurationAsync(configuration);
            await _anomalousQueryDetector.UpdateConfigurationAsync(configuration);
            await _userBehaviorAnalyzer.UpdateConfigurationAsync(configuration);
            await _dataExfiltrationDetector.UpdateConfigurationAsync(configuration);

            // Store updated configuration
            await _cacheService.SetAsync("threat_detection_config", configuration, TimeSpan.FromDays(30));

            _logger.LogInformation("Threat detection configuration updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating threat detection configuration");
        }
    }

    // Private methods

    private async Task<List<SecurityThreat>> CorrelateThreatSignalsAsync(
        List<SecurityThreat> threats,
        ThreatAnalysisContext context)
    {
        var correlatedThreats = new List<SecurityThreat>();
        var processedThreats = new HashSet<string>();

        foreach (var threat in threats.OrderByDescending(t => t.Confidence))
        {
            if (processedThreats.Contains(threat.Id)) continue;

            // Find related threats
            var relatedThreats = threats
                .Where(t => t.Id != threat.Id &&
                           AreThreatsRelated(threat, t, context))
                .ToList();

            if (relatedThreats.Any())
            {
                // Create correlated threat
                var correlatedThreat = CreateCorrelatedThreat(threat, relatedThreats, context);
                correlatedThreats.Add(correlatedThreat);

                // Mark all related threats as processed
                processedThreats.Add(threat.Id);
                foreach (var related in relatedThreats)
                {
                    processedThreats.Add(related.Id);
                }
            }
            else
            {
                correlatedThreats.Add(threat);
                processedThreats.Add(threat.Id);
            }
        }

        return correlatedThreats;
    }

    private bool AreThreatsRelated(SecurityThreat threat1, SecurityThreat threat2, ThreatAnalysisContext context)
    {
        // Check if threats are related based on various factors
        if (threat1.UserId == threat2.UserId &&
            Math.Abs((threat1.DetectedAt - threat2.DetectedAt).TotalMinutes) < 5)
        {
            return true;
        }

        if (threat1.ThreatType == threat2.ThreatType &&
            threat1.Confidence > 0.7 && threat2.Confidence > 0.7)
        {
            return true;
        }

        return false;
    }

    private SecurityThreat CreateCorrelatedThreat(
        SecurityThreat primaryThreat,
        List<SecurityThreat> relatedThreats,
        ThreatAnalysisContext context)
    {
        var allThreats = new List<SecurityThreat> { primaryThreat };
        allThreats.AddRange(relatedThreats);

        return new SecurityThreat
        {
            Id = Guid.NewGuid().ToString(),
            ThreatType = ThreatType.Correlated,
            RiskLevel = allThreats.Max(t => t.RiskLevel),
            Confidence = allThreats.Average(t => t.Confidence),
            Description = $"Correlated threat pattern involving {allThreats.Count} related threats",
            UserId = primaryThreat.UserId,
            IPAddress = primaryThreat.IPAddress,
            DetectedAt = primaryThreat.DetectedAt,
            DetectionMethod = "Threat Correlation",
            Indicators = allThreats.SelectMany(t => t.Indicators).Distinct().ToList(),
            Metadata = new Dictionary<string, object>
            {
                ["correlated_threat_count"] = allThreats.Count,
                ["primary_threat_id"] = primaryThreat.Id,
                ["related_threat_ids"] = relatedThreats.Select(t => t.Id).ToList(),
                ["correlation_score"] = CalculateCorrelationScore(allThreats)
            }
        };
    }

    private double CalculateCorrelationScore(List<SecurityThreat> threats)
    {
        // Simple correlation score based on threat similarity
        var score = 0.5; // Base score

        // Boost score for multiple threats from same user
        if (threats.Select(t => t.UserId).Distinct().Count() == 1)
        {
            score += 0.2;
        }

        // Boost score for threats detected close in time
        var timeSpan = threats.Max(t => t.DetectedAt) - threats.Min(t => t.DetectedAt);
        if (timeSpan.TotalMinutes < 10)
        {
            score += 0.3;
        }

        return Math.Min(1.0, score);
    }

    private async Task<List<SecurityThreat>> PrioritizeThreatsAsync(List<SecurityThreat> threats)
    {
        // Apply threat prioritization logic
        foreach (var threat in threats)
        {
            var priorityScore = await CalculateThreatPriorityAsync(threat);
            threat.PriorityScore = priorityScore;
        }

        return threats
            .OrderByDescending(t => t.PriorityScore)
            .ThenByDescending(t => t.RiskLevel)
            .ThenByDescending(t => t.Confidence)
            .ToList();
    }

    private async Task<double> CalculateThreatPriorityAsync(SecurityThreat threat)
    {
        var priority = 0.0;

        // Risk level factor (40% weight)
        priority += (int)threat.RiskLevel * 0.4;

        // Confidence factor (30% weight)
        priority += threat.Confidence * 0.3;

        // Threat type factor (20% weight)
        var typeWeight = threat.ThreatType switch
        {
            ThreatType.SQLInjection => 0.9,
            ThreatType.DataExfiltration => 0.8,
            ThreatType.AnomalousQuery => 0.6,
            ThreatType.SuspiciousBehavior => 0.5,
            _ => 0.3
        };
        priority += typeWeight * 0.2;

        // User history factor (10% weight)
        var userRiskScore = await GetUserRiskScoreAsync(threat.UserId);
        priority += userRiskScore * 0.1;

        return Math.Min(1.0, priority);
    }

    private async Task<double> GetUserRiskScoreAsync(string userId)
    {
        // Get user's historical risk score
        var userRiskKey = $"user_risk_score:{userId}";
        var riskScore = await _cacheService.GetAsync<double?>(userRiskKey);
        return riskScore ?? 0.5; // Default medium risk
    }

    private double CalculateOverallRiskScore(List<SecurityThreat> threats)
    {
        if (!threats.Any()) return 0.0;

        var highRiskThreats = threats.Count(t => t.RiskLevel == ThreatRiskLevel.High);
        var mediumRiskThreats = threats.Count(t => t.RiskLevel == ThreatRiskLevel.Medium);
        var lowRiskThreats = threats.Count(t => t.RiskLevel == ThreatRiskLevel.Low);

        var weightedScore = (highRiskThreats * 3.0 + mediumRiskThreats * 2.0 + lowRiskThreats * 1.0) / threats.Count;
        var averageConfidence = threats.Average(t => t.Confidence);

        return (weightedScore / 3.0) * averageConfidence;
    }

    private async Task<List<SecurityRecommendation>> GenerateSecurityRecommendationsAsync(
        List<SecurityThreat> threats,
        ThreatAnalysisContext context)
    {
        var recommendations = new List<SecurityRecommendation>();

        // Generate recommendations based on threat types
        var sqlInjectionThreats = threats.Where(t => t.ThreatType == ThreatType.SQLInjection).ToList();
        if (sqlInjectionThreats.Any())
        {
            recommendations.Add(new SecurityRecommendation
            {
                Type = RecommendationType.Immediate,
                Title = "SQL Injection Threats Detected",
                Description = "Implement additional input validation and parameterized queries",
                Priority = RecommendationPriority.High,
                EstimatedEffort = "2-4 hours",
                AffectedThreats = sqlInjectionThreats.Select(t => t.Id).ToList()
            });
        }

        var dataExfiltrationThreats = threats.Where(t => t.ThreatType == ThreatType.DataExfiltration).ToList();
        if (dataExfiltrationThreats.Any())
        {
            recommendations.Add(new SecurityRecommendation
            {
                Type = RecommendationType.Monitoring,
                Title = "Data Exfiltration Risk",
                Description = "Increase monitoring of data access patterns and implement DLP controls",
                Priority = RecommendationPriority.High,
                EstimatedEffort = "4-8 hours",
                AffectedThreats = dataExfiltrationThreats.Select(t => t.Id).ToList()
            });
        }

        return recommendations;
    }

    private async Task ProcessSecurityAlertsAsync(List<SecurityThreat> threats, ThreatAnalysisContext context)
    {
        var highRiskThreats = threats.Where(t => t.RiskLevel == ThreatRiskLevel.High).ToList();

        foreach (var threat in highRiskThreats)
        {
            await _alertManager.SendSecurityAlertAsync(threat, context);
        }
    }

    private List<string> GetEnabledDetectionMethods()
    {
        var methods = new List<string>();

        if (_config.EnableSQLInjectionDetection) methods.Add("SQL Injection");
        if (_config.EnableAnomalousQueryDetection) methods.Add("Anomalous Query");
        if (_config.EnableUserBehaviorAnalysis) methods.Add("User Behavior");
        if (_config.EnableDataExfiltrationDetection) methods.Add("Data Exfiltration");

        return methods;
    }

    private async Task<List<SecurityThreat>> GetHistoricalThreatsAsync(DateTime startTime, DateTime endTime, string? userId)
    {
        // In a real implementation, this would query a security database
        return new List<SecurityThreat>();
    }

    private Dictionary<ThreatType, int> GroupThreatsByType(List<SecurityThreat> threats)
    {
        return threats.GroupBy(t => t.ThreatType).ToDictionary(g => g.Key, g => g.Count());
    }

    private Dictionary<ThreatRiskLevel, int> GroupThreatsByRiskLevel(List<SecurityThreat> threats)
    {
        return threats.GroupBy(t => t.RiskLevel).ToDictionary(g => g.Key, g => g.Count());
    }

    private Dictionary<DateTime, int> GroupThreatsByDay(List<SecurityThreat> threats)
    {
        return threats.GroupBy(t => t.DetectedAt.Date).ToDictionary(g => g.Key, g => g.Count());
    }

    private List<UserThreatSummary> GetTopTargetedUsers(List<SecurityThreat> threats)
    {
        return threats
            .GroupBy(t => t.UserId)
            .Select(g => new UserThreatSummary
            {
                UserId = g.Key,
                ThreatCount = g.Count(),
                HighestRiskLevel = g.Max(t => t.RiskLevel)
            })
            .OrderByDescending(u => u.ThreatCount)
            .Take(10)
            .ToList();
    }

    private List<ThreatSourceSummary> GetTopThreatSources(List<SecurityThreat> threats)
    {
        return threats
            .Where(t => !string.IsNullOrEmpty(t.IPAddress))
            .GroupBy(t => t.IPAddress)
            .Select(g => new ThreatSourceSummary
            {
                IPAddress = g.Key!,
                ThreatCount = g.Count(),
                ThreatTypes = g.Select(t => t.ThreatType).Distinct().ToList()
            })
            .OrderByDescending(s => s.ThreatCount)
            .Take(10)
            .ToList();
    }

    private ThreatTrendAnalysis AnalyzeThreatTrends(List<SecurityThreat> threats)
    {
        // Simple trend analysis
        var recentThreats = threats.Where(t => t.DetectedAt > DateTime.UtcNow.AddDays(-7)).Count();
        var olderThreats = threats.Where(t => t.DetectedAt <= DateTime.UtcNow.AddDays(-7)).Count();

        var trendDirection = TrendDirection.Stable;
        if (recentThreats > olderThreats * 1.2) trendDirection = TrendDirection.Increasing;
        else if (recentThreats < olderThreats * 0.8) trendDirection = TrendDirection.Decreasing;

        return new ThreatTrendAnalysis
        {
            Direction = trendDirection,
            RecentThreatCount = recentThreats,
            HistoricalThreatCount = olderThreats,
            TrendStrength = Math.Abs(recentThreats - olderThreats) / (double)Math.Max(1, olderThreats)
        };
    }

    private double CalculateMitigationEffectiveness(List<SecurityThreat> threats)
    {
        // Calculate how effective current mitigations are
        var mitigatedThreats = threats.Count(t => t.Metadata.ContainsKey("mitigated") &&
                                                 (bool)t.Metadata["mitigated"]);

        return threats.Any() ? (double)mitigatedThreats / threats.Count : 1.0;
    }
}
