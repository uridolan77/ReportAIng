namespace BIReportingCopilot.Core;

/// <summary>
/// Application-wide constants to eliminate magic strings and improve maintainability
/// </summary>
public static class ApplicationConstants
{
    /// <summary>
    /// User roles used throughout the application
    /// </summary>
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string User = "User";
        public const string Analyst = "Analyst";
        public const string Viewer = "Viewer";

        public static readonly string[] AllRoles = { Admin, User, Analyst, Viewer };
    }

    /// <summary>
    /// User permissions for fine-grained access control
    /// </summary>
    public static class Permissions
    {
        public const string ReadData = "read_data";
        public const string QueryData = "query_data";
        public const string ManageUsers = "manage_users";
        public const string ViewSystemStats = "view_system_stats";
        public const string ManageConnections = "manage_connections";
        public const string ViewAuditLogs = "view_audit_logs";
        public const string ManageSchema = "manage_schema";
        public const string ExportData = "export_data";

        public static readonly string[] AllPermissions =
        {
            ReadData, QueryData, ManageUsers, ViewSystemStats,
            ManageConnections, ViewAuditLogs, ManageSchema, ExportData
        };
    }

    /// <summary>
    /// Cache key prefixes for consistent cache management
    /// </summary>
    public static class CacheKeys
    {
        public const string QueryPrefix = "query:";
        public const string SchemaPrefix = "schema:";
        public const string UserPrefix = "user:";
        public const string PermissionsPrefix = "permissions:";
        public const string RateLimitPrefix = "ratelimit:";
        public const string SessionPrefix = "session:";
        public const string SQLGeneration = "sql_generation:";

        // Cache key builders
        public static string QueryKey(string queryHash) => $"{QueryPrefix}{queryHash}";
        public static string SchemaKey(string connectionName) => $"{SchemaPrefix}{connectionName}";
        public static string UserKey(string userId) => $"{UserPrefix}{userId}";
        public static string PermissionsKey(string userId) => $"{PermissionsPrefix}{userId}";
        public static string RateLimitKey(string identifier) => $"{RateLimitPrefix}{identifier}";
        public static string SessionKey(string sessionId) => $"{SessionPrefix}{sessionId}";
    }

    /// <summary>
    /// SignalR group names for real-time communication
    /// </summary>
    public static class SignalRGroups
    {
        public const string AllUsers = "AllUsers";
        public const string AdminUsers = "AdminUsers";

        public static string UserGroup(string userId) => $"User_{userId}";
        public static string QueryGroup(string queryId) => $"Query_{queryId}";
    }

    /// <summary>
    /// Audit action types for consistent logging
    /// </summary>
    public static class AuditActions
    {
        public const string Login = "LOGIN";
        public const string Logout = "LOGOUT";
        public const string QueryExecuted = "QUERY_EXECUTED";
        public const string SchemaAccessed = "SCHEMA_ACCESSED";
        public const string UserCreated = "USER_CREATED";
        public const string UserUpdated = "USER_UPDATED";
        public const string UserDeleted = "USER_DELETED";
        public const string PreferencesUpdated = "USER_PREFERENCES_UPDATED";
        public const string PasswordChanged = "PASSWORD_CHANGED";
        public const string TokenRefreshed = "TOKEN_REFRESHED";
        public const string SecurityViolation = "SECURITY_VIOLATION";
        public const string DataExported = "DATA_EXPORTED";
        public const string ConnectionTested = "CONNECTION_TESTED";
        public const string SchemaRefreshed = "SCHEMA_REFRESHED";
        public const string SQLGenerated = "SQL_GENERATED";
        public const string InsightGenerated = "INSIGHT_GENERATED";
        public const string SQLExplained = "SQL_EXPLAINED";
        public const string VisualizationGenerated = "VISUALIZATION_GENERATED";
        public const string DashboardGenerated = "DASHBOARD_GENERATED";
    }

    /// <summary>
    /// Entity types for audit logging
    /// </summary>
    public static class EntityTypes
    {
        public const string User = "USER";
        public const string Query = "QUERY";
        public const string Schema = "SCHEMA";
        public const string Connection = "CONNECTION";
        public const string Session = "SESSION";
        public const string System = "SYSTEM";
        public const string Analysis = "ANALYSIS";
        public const string Visualization = "VISUALIZATION";
        public const string Dashboard = "DASHBOARD";
    }

    /// <summary>
    /// Query status values for real-time updates
    /// </summary>
    public static class QueryStatus
    {
        public const string Pending = "pending";
        public const string Processing = "processing";
        public const string Completed = "completed";
        public const string Failed = "failed";
        public const string Cancelled = "cancelled";
    }

    /// <summary>
    /// Default configuration values
    /// </summary>
    public static class Defaults
    {
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
        public const int DefaultCacheExpirationMinutes = 30;
        public const int DefaultRateLimitRequests = 100;
        public const int DefaultRateLimitWindowMinutes = 1;
        public const int DefaultQueryTimeoutSeconds = 30;
        public const int DefaultMaxRowsPerQuery = 1000;
        public const int DefaultCleanupRetentionDays = 30;
        public const string DefaultDateRange = "last_30_days";
        public const string DefaultChartType = "bar";
        public const string DefaultTimezone = "UTC";
    }

    /// <summary>
    /// HTTP header names used throughout the application
    /// </summary>
    public static class Headers
    {
        public const string CorrelationId = "X-Correlation-ID";
        public const string RequestId = "X-Request-ID";
        public const string ApiVersion = "X-API-Version";
        public const string RateLimitRemaining = "X-RateLimit-Remaining";
        public const string RateLimitReset = "X-RateLimit-Reset";
    }

    /// <summary>
    /// Content types and MIME types
    /// </summary>
    public static class ContentTypes
    {
        public const string Json = "application/json";
        public const string Csv = "text/csv";
        public const string Excel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string Pdf = "application/pdf";
    }

    /// <summary>
    /// Feature flags for conditional functionality
    /// </summary>
    public static class FeatureFlags
    {
        public const string EnableQuerySuggestions = "EnableQuerySuggestions";
        public const string EnableAutoVisualization = "EnableAutoVisualization";
        public const string EnableRealTimeUpdates = "EnableRealTimeUpdates";
        public const string EnableAdvancedAnalytics = "EnableAdvancedAnalytics";
        public const string EnableDataExport = "EnableDataExport";
        public const string EnableSchemaInference = "EnableSchemaInference";
    }

    /// <summary>
    /// Validation constants
    /// </summary>
    public static class Validation
    {
        public const int MinPasswordLength = 8;
        public const int MaxPasswordLength = 128;
        public const int MaxUsernameLength = 50;
        public const int MaxEmailLength = 254;
        public const int MaxDisplayNameLength = 100;
        public const int MaxQueryLength = 10000;
        public const int MaxTableNameLength = 128;
        public const int MaxColumnNameLength = 128;
        public const int MinQueryCacheSeconds = 60;
        public const int MaxQueryCacheSeconds = 3600;
    }

    /// <summary>
    /// Regular expressions for validation
    /// </summary>
    public static class RegexPatterns
    {
        public const string Email = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        public const string Username = @"^[a-zA-Z0-9._-]{3,50}$";
        public const string SqlIdentifier = @"^[a-zA-Z_][a-zA-Z0-9_]*$";
        public const string SafeFileName = @"^[a-zA-Z0-9._-]+$";
    }
}
