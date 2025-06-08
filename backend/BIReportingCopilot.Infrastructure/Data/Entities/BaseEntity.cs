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
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
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
