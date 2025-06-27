using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Enhanced user feedback learner that adapts analysis based on user patterns
/// </summary>
public class UserFeedbackLearner : IUserFeedbackLearner
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<UserFeedbackLearner> _logger;

    public UserFeedbackLearner(
        ICacheService cacheService,
        ILogger<UserFeedbackLearner> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<UserAnalysisPatterns> GetUserPatternsAsync(string userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return CreateDefaultPatterns();
            }

            var cacheKey = $"user_patterns:{userId}";
            var cached = await _cacheService.GetAsync<UserAnalysisPatterns>(cacheKey);
            
            if (cached != null)
            {
                return cached;
            }

            // If no cached patterns, create default and cache them
            var defaultPatterns = CreateDefaultPatterns();
            await _cacheService.SetAsync(cacheKey, defaultPatterns, TimeSpan.FromHours(24));
            
            return defaultPatterns;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user patterns for user: {UserId}", userId);
            return CreateDefaultPatterns();
        }
    }

    public async Task RecordAnalysisAsync(BusinessContextProfile profile, string userQuestion, string? userId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return;
            }

            var patterns = await GetUserPatternsAsync(userId);
            
            // Update patterns based on the analysis
            UpdatePatterns(patterns, profile, userQuestion);
            
            // Cache updated patterns
            var cacheKey = $"user_patterns:{userId}";
            await _cacheService.SetAsync(cacheKey, patterns, TimeSpan.FromHours(24));
            
            // Also record the analysis for historical tracking
            await RecordAnalysisHistoryAsync(userId, profile, userQuestion);
            
            _logger.LogDebug("Recorded analysis for user {UserId}: {Question}", userId, userQuestion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording analysis for user: {UserId}", userId);
        }
    }

    private UserAnalysisPatterns CreateDefaultPatterns()
    {
        return new UserAnalysisPatterns
        {
            IntentPatterns = new Dictionary<IntentType, IntentPattern>
            {
                [IntentType.Analytical] = new IntentPattern { IntentType = IntentType.Analytical, UsageCount = 3, SuccessRate = 0.8, CommonKeywords = new List<string> { "analyze", "compare", "trend" } },
                [IntentType.Exploratory] = new IntentPattern { IntentType = IntentType.Exploratory, UsageCount = 3, SuccessRate = 0.7, CommonKeywords = new List<string> { "explore", "discover", "find" } },
                [IntentType.Operational] = new IntentPattern { IntentType = IntentType.Operational, UsageCount = 2, SuccessRate = 0.9, CommonKeywords = new List<string> { "report", "status", "current" } },
                [IntentType.Aggregation] = new IntentPattern { IntentType = IntentType.Aggregation, UsageCount = 2, SuccessRate = 0.8, CommonKeywords = new List<string> { "sum", "total", "count", "average" } }
            },
            FrequentEntities = new Dictionary<string, int>
            {
                ["revenue"] = 10,
                ["sales"] = 8,
                ["customer"] = 6,
                ["product"] = 5
            },
            DomainPreferences = new Dictionary<string, double>
            {
                ["Sales"] = 0.25,
                ["Finance"] = 0.25,
                ["Operations"] = 0.25,
                ["Marketing"] = 0.25
            },
            TermPreferences = new Dictionary<string, double>
            {
                ["revenue"] = 0.3,
                ["sales"] = 0.25,
                ["customer"] = 0.2,
                ["product"] = 0.15,
                ["performance"] = 0.1
            },
            LastUpdated = DateTime.UtcNow
        };
    }

    private void UpdatePatterns(UserAnalysisPatterns patterns, BusinessContextProfile profile, string userQuestion)
    {
        // Update intent patterns
        if (profile.Intent != null)
        {
            var intentType = profile.Intent.Type;
            if (patterns.IntentPatterns.ContainsKey(intentType))
            {
                var pattern = patterns.IntentPatterns[intentType];
                var newPattern = pattern with
                {
                    UsageCount = pattern.UsageCount + 1,
                    SuccessRate = (pattern.SuccessRate + (profile.ConfidenceScore > 0.6 ? 1.0 : 0.0)) / 2.0
                };
                patterns.IntentPatterns[intentType] = newPattern;
            }
            else
            {
                patterns.IntentPatterns[intentType] = new IntentPattern
                {
                    IntentType = intentType,
                    UsageCount = 1,
                    SuccessRate = profile.ConfidenceScore > 0.6 ? 1.0 : 0.0,
                    CommonKeywords = new List<string>()
                };
            }
        }

        // Update domain preferences
        if (profile.Domain != null)
        {
            UpdatePreference(patterns.DomainPreferences, profile.Domain.Name, profile.Domain.RelevanceScore);
        }

        // Update entity frequencies
        foreach (var entity in profile.Entities)
        {
            var entityName = entity.Name.ToLower();
            if (patterns.FrequentEntities.ContainsKey(entityName))
            {
                patterns.FrequentEntities[entityName]++;
            }
            else
            {
                patterns.FrequentEntities[entityName] = 1;
            }
        }

        // Update term preferences
        foreach (var term in profile.BusinessTerms)
        {
            UpdatePreference(patterns.TermPreferences, term.ToLower(), 0.8);
        }

        // Update last updated timestamp
        patterns = patterns with { LastUpdated = DateTime.UtcNow };
    }

    private void UpdatePreference(Dictionary<string, double> preferences, string key, double confidence)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        var learningRate = 0.1; // How quickly to adapt to new patterns
        
        if (preferences.ContainsKey(key))
        {
            // Weighted update based on confidence
            preferences[key] = preferences[key] * (1 - learningRate) + confidence * learningRate;
        }
        else
        {
            preferences[key] = confidence * learningRate;
        }

        // Normalize preferences to sum to 1
        NormalizePreferences(preferences);
    }

    private void NormalizePreferences(Dictionary<string, double> preferences)
    {
        var total = preferences.Values.Sum();
        if (total > 0)
        {
            var keys = preferences.Keys.ToList();
            foreach (var key in keys)
            {
                preferences[key] = preferences[key] / total;
            }
        }
    }

    private async Task RecordAnalysisHistoryAsync(string userId, BusinessContextProfile profile, string userQuestion)
    {
        try
        {
            var historyKey = $"user_analysis_history:{userId}";
            var history = await _cacheService.GetAsync<List<AnalysisRecord>>(historyKey) ?? new List<AnalysisRecord>();
            
            var record = new AnalysisRecord
            {
                Timestamp = DateTime.UtcNow,
                Question = userQuestion,
                IntentType = profile.Intent?.Type.ToString(),
                DomainName = profile.Domain?.Name,
                EntityCount = profile.Entities.Count,
                ConfidenceScore = profile.ConfidenceScore,
                HasTimeContext = profile.TimeContext != null
            };
            
            history.Add(record);
            
            // Keep only last 100 records
            if (history.Count > 100)
            {
                history = history.OrderByDescending(r => r.Timestamp).Take(100).ToList();
            }
            
            await _cacheService.SetAsync(historyKey, history, TimeSpan.FromDays(30));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to record analysis history for user: {UserId}", userId);
        }
    }

    private record AnalysisRecord
    {
        public DateTime Timestamp { get; init; }
        public string Question { get; init; } = string.Empty;
        public string? IntentType { get; init; }
        public string? DomainName { get; init; }
        public int EntityCount { get; init; }
        public double ConfidenceScore { get; init; }
        public bool HasTimeContext { get; init; }
    }
}


