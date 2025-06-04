namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Semantic structure
/// </summary>
public class SemanticStructure
{
    public List<SemanticNode> Nodes { get; set; } = new();
    public List<SemanticRelation> Relations { get; set; } = new();
    public SyntacticStructure SyntacticStructure { get; set; } = new();
    public List<SemanticRole> SemanticRoles { get; set; } = new();
    public double ParseConfidence { get; set; }
}

/// <summary>
/// Semantic node
/// </summary>
public class SemanticNode
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public double Confidence { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Semantic relation
/// </summary>
public class SemanticRelation
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string SourceNodeId { get; set; } = string.Empty;
    public string TargetNodeId { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}

/// <summary>
/// Syntactic structure
/// </summary>
public class SyntacticStructure
{
    public List<SyntacticToken> Tokens { get; set; } = new();
    public List<SyntacticDependency> Dependencies { get; set; } = new();
    public string ParseTree { get; set; } = string.Empty;
}

/// <summary>
/// Syntactic token
/// </summary>
public class SyntacticToken
{
    public string Text { get; set; } = string.Empty;
    public string PartOfSpeech { get; set; } = string.Empty;
    public string Lemma { get; set; } = string.Empty;
    public int Position { get; set; }
    public List<string> Features { get; set; } = new();
}

/// <summary>
/// Syntactic dependency
/// </summary>
public class SyntacticDependency
{
    public string Relation { get; set; } = string.Empty;
    public int HeadIndex { get; set; }
    public int DependentIndex { get; set; }
}

/// <summary>
/// Semantic role
/// </summary>
public class SemanticRole
{
    public string Role { get; set; } = string.Empty;
    public string Argument { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public double Confidence { get; set; }
}
