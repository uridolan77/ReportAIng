namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Intent analysis
/// </summary>
public class IntentAnalysis
{
    public string PrimaryIntent { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<IntentCandidate> AlternativeIntents { get; set; } = new();
    public IntentHierarchy IntentHierarchy { get; set; } = new();
    public List<IntentModifier> Modifiers { get; set; } = new();
    public Dictionary<string, object> IntentParameters { get; set; } = new();
}

/// <summary>
/// Intent candidate
/// </summary>
public class IntentCandidate
{
    public string Intent { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> SupportingFeatures { get; set; } = new();
}

/// <summary>
/// Intent hierarchy
/// </summary>
public class IntentHierarchy
{
    public string Domain { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Subcategory { get; set; } = string.Empty;
    public string SpecificIntent { get; set; } = string.Empty;
}

/// <summary>
/// Intent modifier
/// </summary>
public class IntentModifier
{
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

/// <summary>
/// Intent frequency
/// </summary>
public class IntentFrequency
{
    public string Intent { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public double Percentage { get; set; }
}
