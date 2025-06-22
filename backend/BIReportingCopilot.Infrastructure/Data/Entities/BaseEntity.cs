namespace BIReportingCopilot.Infrastructure.Data.Entities;

public abstract class BaseEntity
{
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    public string? CreatedBy { get; set; }
    public string? UpdatedBy { get; set; }
}

public class SchemaMetadataEntity : BaseEntity
{
    public long Id { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsForeignKey { get; set; }
    public string? BusinessDescription { get; set; }
    public string? SemanticTags { get; set; }
    public string? SampleValues { get; set; }

    // Phase 2 Enhanced Business-Friendly Descriptions
    public string? BusinessFriendlyName { get; set; } // Human-readable name for the schema element
    public string? BusinessPurpose { get; set; } // What business purpose this element serves
    public string? BusinessContext { get; set; } // Business context and usage scenarios
    public string? NaturalLanguageDescription { get; set; } // Description in natural language for LLM
    public string? BusinessDomain { get; set; } // Business domain (Finance, Marketing, Operations, etc.)
    public string? UsageExamples { get; set; } // JSON array of usage examples
    public string? RelatedBusinessTerms { get; set; } // JSON array of related business glossary terms
    public decimal? BusinessImportance { get; set; } // Business importance score (0.0 to 1.0)
    public string? QueryIntents { get; set; } // JSON array of query intents this element supports
    public string? SemanticSynonyms { get; set; } // JSON array of semantic synonyms for better matching

    // Enhanced business context fields (BusinessFriendlyName and NaturalLanguageDescription already defined above)
    public string? BusinessRules { get; set; } // JSON - governance and business rules
    public decimal ImportanceScore { get; set; } = 0.5m; // 0-1 scale for dynamic prioritization
    public decimal UsageFrequency { get; set; } = 0.0m; // How often used in queries
    public string? RelationshipContext { get; set; } // JSON - business meaning of relationships
    public string? DataGovernanceLevel { get; set; } // Public, Internal, Confidential, Restricted

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public DateTime? LastBusinessReview { get; set; } // When business context was last validated
    public bool IsActive { get; set; } = true;
}

// QueryHistoryEntity moved to Core/Models/UnifiedQueryHistory.cs as UnifiedQueryHistoryEntity
// This eliminates the duplicate QueryHistoryEntity classes and provides a single source of truth

public class PromptTemplateEntity : BaseEntity
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public decimal? SuccessRate { get; set; }
    public int UsageCount { get; set; }
    public string? Parameters { get; set; }

    // Additional fields from database schema
    public string? BusinessPurpose { get; set; }
    public string? RelatedBusinessTerms { get; set; }
    public string? BusinessFriendlyName { get; set; }
    public string? NaturalLanguageDescription { get; set; }
    public string? BusinessRules { get; set; }
    public string? RelationshipContext { get; set; }
    public string? DataGovernanceLevel { get; set; }
    public DateTime? LastBusinessReview { get; set; }
    public decimal? ImportanceScore { get; set; }
    public string? UsageFrequency { get; set; }
    public string? TemplateKey { get; set; }
    public string? IntentType { get; set; }
    public int? Priority { get; set; }
    public string? Tags { get; set; }
    public string? UpdatedBy { get; set; }

    // Navigation properties
    public virtual ICollection<TemplatePerformanceMetricsEntity> PerformanceMetrics { get; set; } = new List<TemplatePerformanceMetricsEntity>();
    public virtual ICollection<TemplateABTestEntity> OriginalABTests { get; set; } = new List<TemplateABTestEntity>();
    public virtual ICollection<TemplateABTestEntity> VariantABTests { get; set; } = new List<TemplateABTestEntity>();
    public virtual ICollection<TemplateImprovementSuggestionEntity> ImprovementSuggestions { get; set; } = new List<TemplateImprovementSuggestionEntity>();
}

public class QueryCacheEntity : BaseEntity
{
    public long Id { get; set; }
    public string QueryHash { get; set; } = string.Empty;
    public string NormalizedQuery { get; set; } = string.Empty;
    public string ResultData { get; set; } = string.Empty;
    public string? ResultMetadata { get; set; }
    public DateTime CacheTimestamp { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryTimestamp { get; set; }
    public int HitCount { get; set; }
    public DateTime LastAccessedDate { get; set; } = DateTime.UtcNow;
}

public class UserPreferencesEntity : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string? PreferredChartTypes { get; set; }
    public string? DefaultDateRange { get; set; }
    public int MaxRowsPerQuery { get; set; } = 1000;
    public bool EnableQuerySuggestions { get; set; } = true;
    public bool EnableAutoVisualization { get; set; } = true;
    public string? NotificationSettings { get; set; }
    public DateTime? LastLoginDate { get; set; }
}

public class SystemConfigurationEntity : BaseEntity
{
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsEncrypted { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public new string UpdatedBy { get; set; } = string.Empty;
}

public class AuditLogEntity : BaseEntity
{
    public long Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? Details { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Severity { get; set; } = "Info";
}

public class UserEntity : BaseEntity
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
    public DateTime? LastLoginDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    // MFA Properties
    public bool IsMfaEnabled { get; set; } = false;
    public string? MfaSecret { get; set; }
    public string? MfaMethod { get; set; }
    public string? PhoneNumber { get; set; }
    public bool IsPhoneNumberVerified { get; set; } = false;
    public DateTime? LastMfaValidationDate { get; set; }
    public string? BackupCodes { get; set; }
}

public class UserSessionEntity : BaseEntity
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsActive { get; set; } = true;
    public string? SessionData { get; set; }
}

public class RefreshTokenEntity : BaseEntity
{
    public long Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
}

public class QueryPerformanceEntity : BaseEntity
{
    public long Id { get; set; }
    public string QueryHash { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public int ExecutionTimeMs { get; set; }
    public long LogicalReads { get; set; }
    public long PhysicalReads { get; set; }
    public long RowsAffected { get; set; }
    public double CpuTime { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class SystemMetricsEntity : BaseEntity
{
    public long Id { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public string? MetricCategory { get; set; }
    public double Value { get; set; }
    public string? Unit { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Tags { get; set; }
    public DateTime? RetentionDate { get; set; }
}

public class MfaChallengeEntity : BaseEntity
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string ChallengeCode { get; set; } = string.Empty;
    public string MfaMethod { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public string? DeliveryAddress { get; set; }
    public int AttemptCount { get; set; } = 0;
}
