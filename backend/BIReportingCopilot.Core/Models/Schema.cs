namespace BIReportingCopilot.Core.Models;

public class SchemaMetadata
{
    public string DatabaseName { get; set; } = string.Empty;
    public List<TableMetadata> Tables { get; set; } = new();
    public List<ViewMetadata> Views { get; set; } = new();
    public List<RelationshipMetadata> Relationships { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public string Version { get; set; } = "1.0";
    public Dictionary<string, object> Properties { get; set; } = new();
}

public class TableMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Schema { get; set; } = "dbo";
    public string? Description { get; set; }
    public List<ColumnMetadata> Columns { get; set; } = new();
    public List<IndexMetadata> Indexes { get; set; } = new();
    public long RowCount { get; set; }
    public DateTime LastUpdated { get; set; }
    public string[] SemanticTags { get; set; } = Array.Empty<string>();
    public string? BusinessPurpose { get; set; }
    public string[] NaturalLanguageAliases { get; set; } = Array.Empty<string>();
    public DataQualityScore? QualityScore { get; set; }
}

public class ColumnMetadata
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsForeignKey { get; set; }
    public string? DefaultValue { get; set; }
    public int? MaxLength { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public string? Description { get; set; }
    public string[] SemanticTags { get; set; } = Array.Empty<string>();
    public string[] SampleValues { get; set; } = Array.Empty<string>();
    public ColumnStatistics? Statistics { get; set; }
    public string? BusinessMeaning { get; set; }
    public string[] NaturalLanguageAliases { get; set; } = Array.Empty<string>();
}

public class ViewMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Schema { get; set; } = "dbo";
    public string? Description { get; set; }
    public List<ColumnMetadata> Columns { get; set; } = new();
    public string Definition { get; set; } = string.Empty;
    public string[] DependentTables { get; set; } = Array.Empty<string>();
    public DateTime LastUpdated { get; set; }
    public string[] SemanticTags { get; set; } = Array.Empty<string>();
}

public class RelationshipMetadata
{
    public string Name { get; set; } = string.Empty;
    public string ParentTable { get; set; } = string.Empty;
    public string ParentColumn { get; set; } = string.Empty;
    public string ChildTable { get; set; } = string.Empty;
    public string ChildColumn { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = "FK"; // FK, PK, etc.
    public string? Description { get; set; }
    public bool IsEnforced { get; set; } = true;
}

public class IndexMetadata
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // CLUSTERED, NONCLUSTERED, etc.
    public string[] Columns { get; set; } = Array.Empty<string>();
    public bool IsUnique { get; set; }
    public bool IsPrimaryKey { get; set; }
    public string? FilterDefinition { get; set; }
}

public class ColumnStatistics
{
    public long DistinctCount { get; set; }
    public long NullCount { get; set; }
    public double NullPercentage { get; set; }
    public object? MinValue { get; set; }
    public object? MaxValue { get; set; }
    public object? AvgValue { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public Dictionary<object, long>? ValueDistribution { get; set; }
}

public class DataQualityScore
{
    public double OverallScore { get; set; }
    public double CompletenessScore { get; set; }
    public double UniquenessScore { get; set; }
    public double ValidityScore { get; set; }
    public double ConsistencyScore { get; set; }
    public double TimelinessScore { get; set; }
    public DateTime LastAssessed { get; set; } = DateTime.UtcNow;
    public List<DataQualityIssue> Issues { get; set; } = new();
}

public class DataQualityIssue
{
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Low, Medium, High, Critical
    public string Description { get; set; } = string.Empty;
    public string? Column { get; set; }
    public long AffectedRows { get; set; }
    public string? Recommendation { get; set; }
}

public class SchemaSuggestion
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public double Confidence { get; set; } = 0.8;
    public List<string> SampleQueries { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public int Priority { get; set; } = 1;
    public string[] RequiredTables { get; set; } = Array.Empty<string>();
}

public class SchemaChangeEvent
{
    public string Id { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty; // TABLE_ADDED, COLUMN_MODIFIED, etc.
    public string ObjectName { get; set; } = string.Empty;
    public string ObjectType { get; set; } = string.Empty; // TABLE, COLUMN, INDEX, etc.
    public Dictionary<string, object>? OldValue { get; set; }
    public Dictionary<string, object>? NewValue { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? DetectedBy { get; set; }
    public ImpactAssessment? Impact { get; set; }
}

public class ImpactAssessment
{
    public string Level { get; set; } = string.Empty; // Low, Medium, High, Critical
    public string[] AffectedQueries { get; set; } = Array.Empty<string>();
    public string[] AffectedUsers { get; set; } = Array.Empty<string>();
    public string[] AffectedReports { get; set; } = Array.Empty<string>();
    public string? RecommendedAction { get; set; }
    public bool RequiresUserNotification { get; set; }
}
