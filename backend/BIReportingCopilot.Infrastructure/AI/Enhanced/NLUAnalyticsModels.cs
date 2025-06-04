namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// NLU model metadata
/// </summary>
public class NLUModelMetadata
{
    public int TrainingDataCount { get; set; }
    public string? Domain { get; set; }
    public DateTime TrainingDate { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
    public ModelPerformanceMetrics PerformanceMetrics { get; set; } = new();
}

/// <summary>
/// Model performance metrics
/// </summary>
public class ModelPerformanceMetrics
{
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public Dictionary<string, Dictionary<string, int>> ConfusionMatrix { get; set; } = new();
}

/// <summary>
/// NLU analytics
/// </summary>
public class NLUAnalytics
{
    public TimeSpan Period { get; set; }
    public int TotalQueries { get; set; }
    public double AverageConfidence { get; set; }
    public Dictionary<string, int> IntentDistribution { get; set; } = new();
    public Dictionary<string, int> EntityDistribution { get; set; } = new();
    public Dictionary<string, int> LanguageDistribution { get; set; } = new();
    public Dictionary<string, double> PerformanceMetrics { get; set; } = new();
    public ErrorAnalysis ErrorAnalysis { get; set; } = new();
    public List<string> ImprovementSuggestions { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Error analysis
/// </summary>
public class ErrorAnalysis
{
    public int TotalErrors { get; set; }
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    public List<string> CommonErrorPatterns { get; set; } = new();
}

/// <summary>
/// Conversation analysis
/// </summary>
public class ConversationAnalysis
{
    public string UserId { get; set; } = string.Empty;
    public TimeSpan AnalysisWindow { get; set; }
    public int TotalInteractions { get; set; }
    public double AverageQueryLength { get; set; }
    public List<IntentFrequency> CommonIntents { get; set; } = new();
    public ConversationFlow ConversationFlow { get; set; } = new();
    public UserPreferences UserPreferences { get; set; } = new();
    public EngagementMetrics EngagementMetrics { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Conversation flow
/// </summary>
public class ConversationFlow
{
    public int AverageConversationLength { get; set; }
    public Dictionary<string, string> CommonTransitions { get; set; } = new();
    public List<string> ConversationPatterns { get; set; } = new();
}

/// <summary>
/// User preferences
/// </summary>
public class UserPreferences
{
    public List<string> PreferredQueryTypes { get; set; } = new();
    public List<string> PreferredTimeRanges { get; set; } = new();
    public List<string> PreferredMetrics { get; set; } = new();
    public string CommunicationStyle { get; set; } = string.Empty;
    public string TechnicalLevel { get; set; } = string.Empty;
}

/// <summary>
/// Engagement metrics
/// </summary>
public class EngagementMetrics
{
    public TimeSpan AverageSessionDuration { get; set; }
    public double QueriesPerSession { get; set; }
    public double SuccessfulQueryRate { get; set; }
    public double UserSatisfactionScore { get; set; }
    public double ReturnUserRate { get; set; }
}
