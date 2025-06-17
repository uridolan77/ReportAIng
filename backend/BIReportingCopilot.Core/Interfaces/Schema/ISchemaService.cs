using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.Schema;

/// <summary>
/// Schema service interface for database schema operations
/// </summary>
public interface ISchemaService
{
    Task<SchemaMetadata> GetSchemaAsync(CancellationToken cancellationToken = default);
    Task<SchemaMetadata> RefreshSchemaAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetTableNamesAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetColumnNamesAsync(string tableName, CancellationToken cancellationToken = default);
    Task<SchemaMetadata> GetSchemaMetadataAsync(CancellationToken cancellationToken = default);
    Task<SchemaMetadata> GetSchemaMetadataAsync(string connectionName, CancellationToken cancellationToken = default);
    Task<TableMetadata?> GetTableMetadataAsync(string tableName, CancellationToken cancellationToken = default);
    Task<TableMetadata?> GetTableMetadataAsync(string tableName, string connectionName, CancellationToken cancellationToken = default);
    Task<SchemaMetadata> RefreshSchemaMetadataAsync(CancellationToken cancellationToken = default);
    Task RefreshSchemaMetadataAsync(string connectionName, CancellationToken cancellationToken = default);    Task<List<TableRelationship>> GetTableRelationshipsAsync(CancellationToken cancellationToken = default);
    Task<SchemaValidationResult> ValidateSchemaAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetAccessibleTablesAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<SchemaSuggestion>> GetSchemaSuggestionsAsync(string userId, CancellationToken cancellationToken = default);
    Task<DataQualityAssessment> AssessDataQualityAsync(string tableName, CancellationToken cancellationToken = default);
    Task<SchemaSummary> GetSchemaSummaryAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Schema management service interface for schema administration
/// </summary>
public interface ISchemaManagementService
{
    Task<SchemaChangeResult> ApplySchemaChangesAsync(List<SchemaChange> changes, CancellationToken cancellationToken = default);
    Task<SchemaBackupResult> CreateSchemaBackupAsync(string backupName, CancellationToken cancellationToken = default);
    Task<SchemaRestoreResult> RestoreSchemaFromBackupAsync(string backupId, CancellationToken cancellationToken = default);
    Task<List<SchemaBackup>> GetSchemaBackupsAsync(CancellationToken cancellationToken = default);
    Task<bool> DeleteSchemaBackupAsync(string backupId, CancellationToken cancellationToken = default);
    Task<SchemaComparisonResult> CompareSchemaVersionsAsync(string version1, string version2, CancellationToken cancellationToken = default);
    Task<List<SchemaVersion>> GetSchemaVersionHistoryAsync(CancellationToken cancellationToken = default);
    Task<SchemaValidationResult> ValidateSchemaChangesAsync(List<SchemaChange> changes, CancellationToken cancellationToken = default);
    Task<List<BusinessSchema>> GetBusinessSchemasAsync(string userId, CancellationToken cancellationToken = default);
    Task<BusinessSchema?> GetBusinessSchemaAsync(string schemaId, string userId, CancellationToken cancellationToken = default);    Task<BusinessSchema> CreateBusinessSchemaAsync(BIReportingCopilot.Core.DTOs.CreateBusinessSchemaRequest request, string userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateBusinessSchemaAsync(string schemaId, BIReportingCopilot.Core.DTOs.UpdateBusinessSchemaRequest request, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteBusinessSchemaAsync(string schemaId, string userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Schema optimization service interface for schema performance optimization
/// </summary>
public interface ISchemaOptimizationService
{
    Task<SchemaOptimizationResult> OptimizeSchemaAsync(SchemaMetadata schema, CancellationToken cancellationToken = default);
    Task<List<IndexRecommendation>> GetIndexRecommendationsAsync(string tableName, CancellationToken cancellationToken = default);
    Task<QueryOptimizationResult> AnalyzeQueryPerformanceAsync(string query, CancellationToken cancellationToken = default);
    Task<QueryOptimizationResult> AnalyzeQueryPerformanceAsync(string sql, SchemaMetadata schema, QueryExecutionMetrics? metrics = null);
    Task<SchemaOptimizationMetrics> GetOptimizationMetricsAsync(CancellationToken cancellationToken = default);
    Task<SqlOptimizationResult> OptimizeSqlAsync(string sql, CancellationToken cancellationToken = default);
    Task<List<IndexSuggestion>> SuggestIndexesAsync(string tableName, CancellationToken cancellationToken = default);
    Task<PerformanceAnalysisResult> AnalyzeTablePerformanceAsync(string tableName, CancellationToken cancellationToken = default);    Task<List<PartitioningRecommendation>> GetPartitioningRecommendationsAsync(string tableName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Schema change
/// </summary>
public class SchemaChange
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public SchemaChangeType Type { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}

/// <summary>
/// Schema change type enumeration
/// </summary>
public enum SchemaChangeType
{
    CreateTable,
    DropTable,
    AlterTable,
    CreateColumn,
    DropColumn,
    AlterColumn,
    CreateIndex,
    DropIndex,
    CreateConstraint,
    DropConstraint
}

/// <summary>
/// Schema change result
/// </summary>
public class SchemaChangeResult
{
    public bool Success { get; set; }
    public List<string> AppliedChanges { get; set; } = new();
    public List<string> FailedChanges { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    public TimeSpan ExecutionTime { get; set; }
}

/// <summary>
/// Schema backup
/// </summary>
public class SchemaBackup
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Description { get; set; } = string.Empty;
    public SchemaBackupStatus Status { get; set; }
}

/// <summary>
/// Schema backup status enumeration
/// </summary>
public enum SchemaBackupStatus
{
    InProgress,
    Completed,
    Failed,
    Expired
}

/// <summary>
/// Schema backup result
/// </summary>
public class SchemaBackupResult
{
    public bool Success { get; set; }
    public string BackupId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public TimeSpan ExecutionTime { get; set; }
    public long BackupSize { get; set; }
}

/// <summary>
/// Schema restore result
/// </summary>
public class SchemaRestoreResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public TimeSpan ExecutionTime { get; set; }
    public List<string> RestoredTables { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// Schema comparison result
/// </summary>
public class SchemaComparisonResult
{
    public List<SchemaDifference> Differences { get; set; } = new();
    public bool AreIdentical { get; set; }
    public string Summary { get; set; } = string.Empty;
}

/// <summary>
/// Schema difference
/// </summary>
public class SchemaDifference
{
    public SchemaDifferenceType Type { get; set; }
    public string ObjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version1Value { get; set; } = string.Empty;
    public string Version2Value { get; set; } = string.Empty;
}

/// <summary>
/// Schema difference type enumeration
/// </summary>
public enum SchemaDifferenceType
{
    TableAdded,
    TableRemoved,
    TableModified,
    ColumnAdded,
    ColumnRemoved,
    ColumnModified,
    IndexAdded,
    IndexRemoved,
    ConstraintAdded,
    ConstraintRemoved
}

/// <summary>
/// Schema version
/// </summary>
public class SchemaVersion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Version { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<SchemaChange> Changes { get; set; } = new();
}

/// <summary>
/// Schema validation result
/// </summary>
public class SchemaValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Performance analysis result
/// </summary>
public class PerformanceAnalysisResult
{
    public string TableName { get; set; } = string.Empty;
    public Dictionary<string, object> Metrics { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public double PerformanceScore { get; set; }
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Partitioning recommendation
/// </summary>
public class PartitioningRecommendation
{
    public string TableName { get; set; } = string.Empty;
    public string RecommendedPartitionColumn { get; set; } = string.Empty;
    public PartitioningStrategy Strategy { get; set; }
    public string Description { get; set; } = string.Empty;
    public double EstimatedImprovement { get; set; }
}

/// <summary>
/// Partitioning strategy enumeration
/// </summary>
public enum PartitioningStrategy
{
    Range,
    Hash,
    List,
    Composite
}

/// <summary>
/// Schema suggestion
/// </summary>
public class SchemaSuggestion
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SchemaSuggestionType Type { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Schema suggestion type enumeration
/// </summary>
public enum SchemaSuggestionType
{
    IndexRecommendation,
    DataTypeOptimization,
    ColumnAddition,
    RelationshipMapping,
    PerformanceImprovement
}

/// <summary>
/// Data quality assessment
/// </summary>
public class DataQualityAssessment
{
    public string TableName { get; set; } = string.Empty;
    public double OverallScore { get; set; }
    public Dictionary<string, double> ColumnScores { get; set; } = new();
    public List<DataQualityIssue> Issues { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Data quality issue
/// </summary>
public class DataQualityIssue
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = string.Empty;
    public DataQualitySeverity Severity { get; set; }
    public string ColumnName { get; set; } = string.Empty;
    public long AffectedRows { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
}

/// <summary>
/// Data quality severity enumeration
/// </summary>
public enum DataQualitySeverity
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Schema optimization result
/// </summary>
public class SchemaOptimizationResult
{
    public List<string> Recommendations { get; set; } = new();
    public double ImprovementScore { get; set; }
    public Dictionary<string, object> Metrics { get; set; } = new();
    public List<OptimizationAction> Actions { get; set; } = new();
    public TimeSpan EstimatedTime { get; set; }
}

/// <summary>
/// Optimization action
/// </summary>
public class OptimizationAction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public OptimizationActionType Type { get; set; }
    public double EstimatedImpact { get; set; }
    public string SqlCommand { get; set; } = string.Empty;
}

/// <summary>
/// Optimization action type enumeration
/// </summary>
public enum OptimizationActionType
{
    CreateIndex,
    DropIndex,
    UpdateStatistics,
    PartitionTable,
    ArchiveData
}

/// <summary>
/// Schema summary
/// </summary>
public class SchemaSummary
{
    public int TotalTables { get; set; }
    public int TotalColumns { get; set; }
    public int TotalRelationships { get; set; }
    public List<string> TableNames { get; set; } = new();
    public List<TableSummary> Tables { get; set; } = new();
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Table summary
/// </summary>
public class TableSummary
{
    public string Name { get; set; } = string.Empty;
    public int ColumnCount { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<string> ColumnNames { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}
