using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

// ===== CONSOLIDATED DASHBOARD MODELS =====
// This file now contains all dashboard-related models from both:
// - MultiModalDashboards.cs (advanced dashboard functionality)
// - DashboardModels.cs (basic dashboard analytics and overview)

#region Basic Dashboard Analytics Models (from DashboardModels.cs)

public class DashboardOverview
{
    public UserActivitySummary UserActivity { get; set; } = new();
    public List<QueryHistoryItem> RecentQueries { get; set; } = new();
    public SystemMetrics SystemMetrics { get; set; } = new();
    public QuickStats QuickStats { get; set; } = new();
}

public class SystemMetrics
{
    public int DatabaseConnections { get; set; }
    public decimal CacheHitRate { get; set; }
    public double AverageQueryTime { get; set; }
    public TimeSpan SystemUptime { get; set; }
}

public class QuickStats
{
    public int TotalQueries { get; set; }
    public int QueriesThisWeek { get; set; }
    public double AverageQueryTime { get; set; }
    public string FavoriteTable { get; set; } = string.Empty;
}

public class UsageAnalytics
{
    public QueryTrends QueryTrends { get; set; } = new();
    public List<PopularTable> PopularTables { get; set; } = new();
    public PerformanceMetrics PerformanceMetrics { get; set; } = new();
    public ErrorAnalysis ErrorAnalysis { get; set; } = new();
}

public class QueryTrends
{
    public Dictionary<DateTime, int> DailyQueryCounts { get; set; } = new();
    public Dictionary<string, int> QueryTypeDistribution { get; set; } = new();
    public List<int> PeakUsageHours { get; set; } = new();
}

public class PopularTable
{
    public string TableName { get; set; } = string.Empty;
    public int QueryCount { get; set; }
    public double AverageResponseTime { get; set; }
    public DateTime LastAccessed { get; set; }
}

// ErrorAnalysis moved to PerformanceModels.cs to consolidate duplications

public class SystemStatistics
{
    public int TotalUsers { get; set; }
    public int TotalQueries { get; set; }
    public double AverageResponseTime { get; set; }
    public double ErrorRate { get; set; }
    public List<TopUser> TopUsers { get; set; } = new();
    public ResourceUsage ResourceUsage { get; set; } = new();
}

public class TopUser
{
    public string UserId { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int QueryCount { get; set; }
    public double AverageResponseTime { get; set; }
}

public class ResourceUsage
{
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public double NetworkUsage { get; set; }
}

public class ActivityItem
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string Severity { get; set; } = "Info";
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class Recommendation
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Priority { get; set; } = "low";
    public string? ActionUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

#endregion

#region Advanced Multi-Modal Dashboard Models

/// <summary>
/// Multi-modal dashboard
/// </summary>
public class Dashboard
{
    public string DashboardId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string? Category { get; set; }
    public List<DashboardWidget> Widgets { get; set; } = new();
    public DashboardLayout Layout { get; set; } = new();
    public DashboardConfiguration Configuration { get; set; } = new();
    public DashboardPermissions Permissions { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastViewedAt { get; set; }
    public bool IsPublic { get; set; } = false;
    public bool IsTemplate { get; set; } = false;
    public List<string> Tags { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Dashboard widget
/// </summary>
public class DashboardWidget
{
    public string WidgetId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WidgetType Type { get; set; } = WidgetType.Chart;
    public WidgetPosition Position { get; set; } = new();
    public WidgetSize Size { get; set; } = new();
    public WidgetConfiguration Configuration { get; set; } = new();
    public WidgetDataSource DataSource { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsVisible { get; set; } = true;
    public Dictionary<string, object> CustomProperties { get; set; } = new();
}

/// <summary>
/// Dashboard layout
/// </summary>
public class DashboardLayout
{
    public LayoutType Type { get; set; } = LayoutType.Grid;
    public int Columns { get; set; } = 12;
    public int Rows { get; set; } = 10;
    public LayoutConfiguration Configuration { get; set; } = new();
    public List<LayoutBreakpoint> Breakpoints { get; set; } = new();
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

/// <summary>
/// Dashboard configuration
/// </summary>
public class DashboardConfiguration
{
    public TimeSpan? RefreshInterval { get; set; }
    public bool AutoRefresh { get; set; } = false;
    public bool EnableRealTime { get; set; } = false;
    public bool EnableFilters { get; set; } = true;
    public bool EnableExport { get; set; } = true;
    public bool EnableSharing { get; set; } = true;
    public ThemeConfiguration Theme { get; set; } = new();
    public List<GlobalFilter> GlobalFilters { get; set; } = new();
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

/// <summary>
/// Create dashboard request
/// </summary>
public class CreateDashboardRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public List<CreateWidgetRequest> Widgets { get; set; } = new();
    public DashboardLayout? Layout { get; set; }
    public DashboardConfiguration? Configuration { get; set; }
    public List<string> Tags { get; set; } = new();
    public bool IsPublic { get; set; } = false;
}

/// <summary>
/// Update dashboard request
/// </summary>
public class UpdateDashboardRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public DashboardLayout? Layout { get; set; }
    public DashboardConfiguration? Configuration { get; set; }
    public List<string>? Tags { get; set; }
    public bool? IsPublic { get; set; }
}

/// <summary>
/// Create widget request
/// </summary>
public class CreateWidgetRequest
{
    [Required]
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    [Required]
    public WidgetType Type { get; set; }
    public WidgetPosition? Position { get; set; }
    public WidgetSize? Size { get; set; }
    public WidgetConfiguration? Configuration { get; set; }
    [Required]
    public WidgetDataSource DataSource { get; set; } = new();
}

/// <summary>
/// Update widget request
/// </summary>
public class UpdateWidgetRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public WidgetPosition? Position { get; set; }
    public WidgetSize? Size { get; set; }
    public WidgetConfiguration? Configuration { get; set; }
    public WidgetDataSource? DataSource { get; set; }
    public bool? IsVisible { get; set; }
}

/// <summary>
/// Widget position
/// </summary>
public class WidgetPosition
{
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public int Z { get; set; } = 0; // Layer/depth
}

/// <summary>
/// Widget size
/// </summary>
public class WidgetSize
{
    public int Width { get; set; } = 4;
    public int Height { get; set; } = 3;
    public int MinWidth { get; set; } = 2;
    public int MinHeight { get; set; } = 2;
    public int MaxWidth { get; set; } = 12;
    public int MaxHeight { get; set; } = 10;
}

/// <summary>
/// Widget configuration
/// </summary>
public class WidgetConfiguration
{
    public ChartConfiguration? ChartConfig { get; set; }
    public TableConfiguration? TableConfig { get; set; }
    public MetricConfiguration? MetricConfig { get; set; }
    public TextConfiguration? TextConfig { get; set; }
    public FilterConfiguration? FilterConfig { get; set; }
    public Dictionary<string, object> CustomConfig { get; set; } = new();
}

/// <summary>
/// Widget data source
/// </summary>
public class WidgetDataSource
{
    public DataSourceType Type { get; set; } = DataSourceType.Query;
    public string? Query { get; set; }
    public string? SqlQuery { get; set; }
    public string? ApiEndpoint { get; set; }
    public TimeSpan? RefreshInterval { get; set; }
    public bool EnableRealTime { get; set; } = false;
    public Dictionary<string, object> Parameters { get; set; } = new();
    public DataTransformation? Transformation { get; set; }
}

/// <summary>
/// Dashboard permissions
/// </summary>
public class DashboardPermissions
{
    public string OwnerId { get; set; } = string.Empty;
    public List<UserPermission> UserPermissions { get; set; } = new();
    public List<RolePermission> RolePermissions { get; set; } = new();
    public PermissionLevel DefaultPermission { get; set; } = PermissionLevel.None;
    public bool AllowPublicView { get; set; } = false;
}

/// <summary>
/// Dashboard filter
/// </summary>
public class DashboardFilter
{
    public string? Category { get; set; }
    public List<string>? Tags { get; set; }
    public DateTime? CreatedAfter { get; set; }
    public DateTime? CreatedBefore { get; set; }
    public bool? IsPublic { get; set; }
    public bool? IsTemplate { get; set; }
    public string? SearchTerm { get; set; }
    public DashboardSortBy SortBy { get; set; } = DashboardSortBy.UpdatedAt;
    public SortDirection SortDirection { get; set; } = SortDirection.Descending;
}

/// <summary>
/// Dashboard export
/// </summary>
public class DashboardExport
{
    public string ExportId { get; set; } = Guid.NewGuid().ToString();
    public string DashboardId { get; set; } = string.Empty;
    public ExportFormat Format { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public ExportMetadata Metadata { get; set; } = new();
}

/// <summary>
/// Dashboard share
/// </summary>
public class DashboardShare
{
    public string ShareId { get; set; } = Guid.NewGuid().ToString();
    public string DashboardId { get; set; } = string.Empty;
    public string SharedBy { get; set; } = string.Empty;
    public ShareConfiguration Configuration { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public List<ShareAccess> AccessLog { get; set; } = new();
}

/// <summary>
/// Dashboard analytics
/// </summary>
public class DashboardAnalytics
{
    public string DashboardId { get; set; } = string.Empty;
    public int TotalViews { get; set; }
    public int UniqueViewers { get; set; }
    public TimeSpan AverageViewDuration { get; set; }
    public DateTime LastViewed { get; set; }
    public List<ViewAnalytics> ViewHistory { get; set; } = new();
    public List<WidgetAnalytics> WidgetAnalytics { get; set; } = new();
    public Dictionary<string, int> ViewsByTimeOfDay { get; set; } = new();
    public Dictionary<string, int> ViewsByDayOfWeek { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Dashboard template
/// </summary>
public class DashboardTemplate
{
    public string TemplateId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string PreviewImage { get; set; } = string.Empty;
    public List<TemplateWidget> Widgets { get; set; } = new();
    public DashboardLayout Layout { get; set; } = new();
    public List<TemplateParameter> Parameters { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public bool IsPublic { get; set; } = true;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int UsageCount { get; set; } = 0;
}

// Supporting classes
public class LayoutConfiguration
{
    public int GridGap { get; set; } = 10;
    public bool EnableDragAndDrop { get; set; } = true;
    public bool EnableResize { get; set; } = true;
    public bool AutoFit { get; set; } = false;
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

public class LayoutBreakpoint
{
    public string Name { get; set; } = string.Empty;
    public int MinWidth { get; set; }
    public int Columns { get; set; }
    public Dictionary<string, object> Settings { get; set; } = new();
}

public class ThemeConfiguration
{
    public string PrimaryColor { get; set; } = "#007bff";
    public string SecondaryColor { get; set; } = "#6c757d";
    public string BackgroundColor { get; set; } = "#ffffff";
    public string TextColor { get; set; } = "#333333";
    public string FontFamily { get; set; } = "Arial, sans-serif";
    public Dictionary<string, string> CustomColors { get; set; } = new();
}

// GlobalFilter moved to avoid duplicate definition

public class TableConfiguration
{
    public bool EnablePaging { get; set; } = true;
    public int PageSize { get; set; } = 10;
    public bool EnableSorting { get; set; } = true;
    public bool EnableFiltering { get; set; } = true;
    public List<ColumnConfiguration> Columns { get; set; } = new();
}

public class MetricConfiguration
{
    public string Format { get; set; } = "number";
    public string Unit { get; set; } = string.Empty;
    public bool ShowTrend { get; set; } = true;
    public bool ShowComparison { get; set; } = false;
    public string? ComparisonPeriod { get; set; }
}

public class TextConfiguration
{
    public string Content { get; set; } = string.Empty;
    public string FontSize { get; set; } = "14px";
    public string FontWeight { get; set; } = "normal";
    public string TextAlign { get; set; } = "left";
    public string Color { get; set; } = "#333333";
}

public class FilterConfiguration
{
    public FilterType Type { get; set; } = FilterType.Text;
    public List<string> Options { get; set; } = new();
    public object? DefaultValue { get; set; }
    public bool IsMultiSelect { get; set; } = false;
}

public class DataTransformation
{
    public List<TransformationStep> Steps { get; set; } = new();
    public Dictionary<string, object> Parameters { get; set; } = new();
}

public class TransformationStep
{
    public string Type { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
}

public class UserPermission
{
    public string UserId { get; set; } = string.Empty;
    public PermissionLevel Permission { get; set; } = PermissionLevel.View;
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    public string? GrantedBy { get; set; }
}

public class RolePermission
{
    public string RoleName { get; set; } = string.Empty;
    public PermissionLevel Permission { get; set; } = PermissionLevel.View;
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
}

public class ShareConfiguration
{
    public ShareType Type { get; set; } = ShareType.Link;
    public PermissionLevel Permission { get; set; } = PermissionLevel.View;
    public bool RequireAuthentication { get; set; } = true;
    public DateTime? ExpiresAt { get; set; }
    public List<string> AllowedUsers { get; set; } = new();
    public string? Password { get; set; }
}

public class ShareAccess
{
    public string UserId { get; set; } = string.Empty;
    public DateTime AccessedAt { get; set; } = DateTime.UtcNow;
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
}

public class ExportMetadata
{
    public int PageCount { get; set; }
    public long FileSizeBytes { get; set; }
    public string GeneratedBy { get; set; } = string.Empty;
    public Dictionary<string, object> CustomMetadata { get; set; } = new();
}

public class ViewAnalytics
{
    public DateTime ViewedAt { get; set; }
    public string UserId { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public List<string> WidgetsViewed { get; set; } = new();
}

public class WidgetAnalytics
{
    public string WidgetId { get; set; } = string.Empty;
    public int Views { get; set; }
    public TimeSpan AverageViewDuration { get; set; }
    public int Interactions { get; set; }
    public DateTime LastViewed { get; set; }
}

public class TemplateWidget
{
    public string Title { get; set; } = string.Empty;
    public WidgetType Type { get; set; }
    public WidgetPosition Position { get; set; } = new();
    public WidgetSize Size { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
    public string DataSourceTemplate { get; set; } = string.Empty;
}

public class TemplateParameter
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public object? DefaultValue { get; set; }
    public bool IsRequired { get; set; } = false;
    public string? Description { get; set; }
}

public class ColumnConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public bool Sortable { get; set; } = true;
    public bool Filterable { get; set; } = true;
    public bool Visible { get; set; } = true;
    public int Width { get; set; } = 100;
}

// Enumerations
public enum WidgetType
{
    Chart,
    Table,
    Metric,
    Text,
    Filter,
    Image,
    Map,
    Gauge,
    Progress,
    Calendar
}

public enum LayoutType
{
    Grid,
    Flex,
    Absolute,
    Flow
}

public enum DataSourceType
{
    Query,
    Api,
    Static,
    RealTime,
    File
}

public enum PermissionLevel
{
    None,
    View,
    Edit,
    Admin,
    Owner
}

public enum DashboardSortBy
{
    Name,
    CreatedAt,
    UpdatedAt,
    LastViewedAt,
    ViewCount
}

public enum SortDirection
{
    Ascending,
    Descending
}

public enum ExportFormat
{
    PDF,
    PNG,
    JPEG,
    SVG,
    Excel,
    CSV,
    JSON
}

public enum ShareType
{
    Link,
    Email,
    Embed,
    Download
}

public enum FilterType
{
    Text,
    Number,
    Date,
    Select,
    MultiSelect,
    Range,
    Boolean
}

#endregion
