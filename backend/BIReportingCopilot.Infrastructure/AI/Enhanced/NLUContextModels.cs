namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Contextual analysis
/// </summary>
public class ContextualAnalysis
{
    public double ContextualRelevance { get; set; }
    public List<ContextualCue> ContextualCues { get; set; } = new();
    public ConversationContext ConversationContext { get; set; } = new();
    public TemporalContext TemporalContext { get; set; } = new();
    public UserContext UserContext { get; set; } = new();
    public List<ContextualInference> Inferences { get; set; } = new();
}

/// <summary>
/// Contextual cue
/// </summary>
public class ContextualCue
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Strength { get; set; }
    public string Source { get; set; } = string.Empty;
}

/// <summary>
/// Conversation context
/// </summary>
public class ConversationContext
{
    public List<ConversationTurn> RecentTurns { get; set; } = new();
    public string CurrentTopic { get; set; } = string.Empty;
    public List<string> ActiveEntities { get; set; } = new();
    public Dictionary<string, object> ConversationState { get; set; } = new();
}

/// <summary>
/// Conversation turn
/// </summary>
public class ConversationTurn
{
    public string TurnId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public List<string> Entities { get; set; } = new();
    public DateTime Timestamp { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Temporal context
/// </summary>
public class TemporalContext
{
    public DateTime QueryTime { get; set; }
    public List<TemporalReference> TemporalReferences { get; set; } = new();
    public TimeZone UserTimeZone { get; set; } = TimeZone.CurrentTimeZone;
    public BusinessCalendar BusinessCalendar { get; set; } = new();
}

/// <summary>
/// Temporal reference
/// </summary>
public class TemporalReference
{
    public string Expression { get; set; } = string.Empty;
    public DateTime ResolvedDateTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public TemporalType Type { get; set; }
    public double Confidence { get; set; }
}

/// <summary>
/// Business calendar
/// </summary>
public class BusinessCalendar
{
    public List<DateTime> Holidays { get; set; } = new();
    public List<DayOfWeek> BusinessDays { get; set; } = new();
    public TimeSpan BusinessHoursStart { get; set; }
    public TimeSpan BusinessHoursEnd { get; set; }
}

/// <summary>
/// User context
/// </summary>
public class UserContext
{
    public string UserId { get; set; } = string.Empty;
    public UserProfile UserProfile { get; set; } = new();
    public List<string> UserPreferences { get; set; } = new();
    public UserBehaviorPattern BehaviorPattern { get; set; } = new();
    public Dictionary<string, object> CustomAttributes { get; set; } = new();
}

/// <summary>
/// User profile
/// </summary>
public class UserProfile
{
    public string Role { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string ExperienceLevel { get; set; } = string.Empty;
    public List<string> Permissions { get; set; } = new();
    public Dictionary<string, object> Preferences { get; set; } = new();
}

/// <summary>
/// User behavior pattern
/// </summary>
public class UserBehaviorPattern
{
    public List<string> FrequentQueries { get; set; } = new();
    public List<string> PreferredMetrics { get; set; } = new();
    public List<string> CommonTimeRanges { get; set; } = new();
    public string CommunicationStyle { get; set; } = string.Empty;
}

/// <summary>
/// Contextual inference
/// </summary>
public class ContextualInference
{
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> SupportingEvidence { get; set; } = new();
}

/// <summary>
/// Conversation state
/// </summary>
public class ConversationState
{
    public string SessionId { get; set; } = string.Empty;
    public string CurrentTopic { get; set; } = string.Empty;
    public List<string> ActiveEntities { get; set; } = new();
    public Dictionary<string, object> StateVariables { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Temporal type enumeration
/// </summary>
public enum TemporalType
{
    Absolute,
    Relative,
    Duration,
    Recurring
}
