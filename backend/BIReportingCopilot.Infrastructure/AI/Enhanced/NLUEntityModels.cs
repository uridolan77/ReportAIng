namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Entity analysis
/// </summary>
public class EntityAnalysis
{
    public List<ExtractedEntity> Entities { get; set; } = new();
    public List<EntityRelation> EntityRelations { get; set; } = new();
    public List<string> MissingEntities { get; set; } = new();
    public double OverallConfidence { get; set; }
    public EntityResolutionResult ResolutionResult { get; set; } = new();
}

/// <summary>
/// Extracted entity
/// </summary>
public class ExtractedEntity
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string NormalizedValue { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
    public double Confidence { get; set; }
    public List<EntityAttribute> Attributes { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Entity relation
/// </summary>
public class EntityRelation
{
    public string SourceEntityId { get; set; } = string.Empty;
    public string TargetEntityId { get; set; } = string.Empty;
    public string RelationType { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

/// <summary>
/// Entity attribute
/// </summary>
public class EntityAttribute
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public double Confidence { get; set; }
}

/// <summary>
/// Entity resolution result
/// </summary>
public class EntityResolutionResult
{
    public List<ResolvedEntity> ResolvedEntities { get; set; } = new();
    public List<string> UnresolvedEntities { get; set; } = new();
    public double ResolutionRate { get; set; }
}

/// <summary>
/// Resolved entity
/// </summary>
public class ResolvedEntity
{
    public string EntityId { get; set; } = string.Empty;
    public string ResolvedValue { get; set; } = string.Empty;
    public string KnowledgeBaseId { get; set; } = string.Empty;
    public double ResolutionConfidence { get; set; }
    public Dictionary<string, object> ResolvedAttributes { get; set; } = new();
}

/// <summary>
/// Entity annotation for training
/// </summary>
public class EntityAnnotation
{
    public string EntityType { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
}
