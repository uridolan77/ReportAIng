using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Real-time streaming analytics service interface
/// Provides live data processing, streaming dashboards, and real-time insights
/// </summary>
public interface IRealTimeStreamingService
{
    /// <summary>
    /// Start real-time streaming session for a user
    /// </summary>
    Task<StreamingSession> StartStreamingSessionAsync(string userId, StreamingConfiguration config);

    /// <summary>
    /// Stop streaming session
    /// </summary>
    Task<bool> StopStreamingSessionAsync(string sessionId, string userId);

    /// <summary>
    /// Process real-time data stream event
    /// </summary>
    Task ProcessDataStreamEventAsync(DataStreamEvent streamEvent);

    /// <summary>
    /// Process real-time query stream event
    /// </summary>
    Task ProcessQueryStreamEventAsync(QueryStreamEvent queryEvent);

    /// <summary>
    /// Get real-time dashboard data
    /// </summary>
    Task<RealTimeDashboard> GetRealTimeDashboardAsync(string userId);

    /// <summary>
    /// Get streaming analytics for time window
    /// </summary>
    Task<StreamingAnalyticsResult> GetStreamingAnalyticsAsync(
        TimeSpan timeWindow, 
        string? userId = null);

    /// <summary>
    /// Subscribe to real-time data stream
    /// </summary>
    Task<string> SubscribeToDataStreamAsync(
        string userId, 
        StreamSubscription subscription);

    /// <summary>
    /// Unsubscribe from data stream
    /// </summary>
    Task<bool> UnsubscribeFromDataStreamAsync(string subscriptionId, string userId);

    /// <summary>
    /// Get active streaming sessions
    /// </summary>
    Task<List<StreamingSession>> GetActiveSessionsAsync(string? userId = null);

    /// <summary>
    /// Get real-time metrics
    /// </summary>
    Task<RealTimeMetrics> GetRealTimeMetricsAsync(string? userId = null);

    /// <summary>
    /// Create real-time alert
    /// </summary>
    Task<string> CreateRealTimeAlertAsync(RealTimeAlert alert, string userId);

    /// <summary>
    /// Get streaming performance metrics
    /// </summary>
    Task<StreamingPerformanceMetrics> GetStreamingPerformanceAsync();
}

/// <summary>
/// Multi-modal dashboard service interface
/// Provides advanced dashboard creation, management, and reporting
/// </summary>
public interface IMultiModalDashboardService
{
    /// <summary>
    /// Create multi-modal dashboard
    /// </summary>
    Task<Dashboard> CreateDashboardAsync(CreateDashboardRequest request, string userId);

    /// <summary>
    /// Update existing dashboard
    /// </summary>
    Task<Dashboard> UpdateDashboardAsync(string dashboardId, UpdateDashboardRequest request, string userId);

    /// <summary>
    /// Delete dashboard
    /// </summary>
    Task<bool> DeleteDashboardAsync(string dashboardId, string userId);

    /// <summary>
    /// Get dashboard by ID
    /// </summary>
    Task<Dashboard?> GetDashboardAsync(string dashboardId, string userId);

    /// <summary>
    /// Get user's dashboards
    /// </summary>
    Task<List<Dashboard>> GetUserDashboardsAsync(string userId, DashboardFilter? filter = null);

    /// <summary>
    /// Add widget to dashboard
    /// </summary>
    Task<DashboardWidget> AddWidgetToDashboardAsync(
        string dashboardId, 
        CreateWidgetRequest request, 
        string userId);

    /// <summary>
    /// Update dashboard widget
    /// </summary>
    Task<DashboardWidget> UpdateWidgetAsync(
        string dashboardId, 
        string widgetId, 
        UpdateWidgetRequest request, 
        string userId);

    /// <summary>
    /// Remove widget from dashboard
    /// </summary>
    Task<bool> RemoveWidgetAsync(string dashboardId, string widgetId, string userId);

    /// <summary>
    /// Generate dashboard from natural language description
    /// </summary>
    Task<Dashboard> GenerateDashboardFromDescriptionAsync(
        string description, 
        string userId, 
        SchemaMetadata? schema = null);

    /// <summary>
    /// Export dashboard to various formats
    /// </summary>
    Task<DashboardExport> ExportDashboardAsync(
        string dashboardId, 
        ExportFormat format, 
        ExportOptions? options = null);

    /// <summary>
    /// Generate automated report from dashboard
    /// </summary>
    Task<GeneratedReport> GenerateReportAsync(
        string dashboardId, 
        ReportConfiguration config, 
        string userId);

    /// <summary>
    /// Share dashboard with other users
    /// </summary>
    Task<DashboardShare> ShareDashboardAsync(
        string dashboardId, 
        ShareConfiguration shareConfig, 
        string userId);

    /// <summary>
    /// Get dashboard analytics and usage metrics
    /// </summary>
    Task<DashboardAnalytics> GetDashboardAnalyticsAsync(
        string dashboardId, 
        TimeSpan? timeWindow = null);

    /// <summary>
    /// Clone dashboard
    /// </summary>
    Task<Dashboard> CloneDashboardAsync(
        string dashboardId, 
        string newName, 
        string userId);

    /// <summary>
    /// Get dashboard templates
    /// </summary>
    Task<List<DashboardTemplate>> GetDashboardTemplatesAsync(string? category = null);

    /// <summary>
    /// Create dashboard from template
    /// </summary>
    Task<Dashboard> CreateDashboardFromTemplateAsync(
        string templateId, 
        string name, 
        string userId, 
        Dictionary<string, object>? parameters = null);
}

/// <summary>
/// Advanced reporting service interface
/// Provides comprehensive reporting capabilities with AI-generated insights
/// </summary>
public interface IAdvancedReportingService
{
    /// <summary>
    /// Generate comprehensive report with AI insights
    /// </summary>
    Task<ComprehensiveReport> GenerateComprehensiveReportAsync(
        ReportRequest request, 
        string userId);

    /// <summary>
    /// Generate text-based report from data
    /// </summary>
    Task<TextReport> GenerateTextReportAsync(
        string query, 
        QueryResponse queryResponse, 
        ReportStyle style = ReportStyle.Executive);

    /// <summary>
    /// Generate visual report with charts and insights
    /// </summary>
    Task<VisualReport> GenerateVisualReportAsync(
        List<DashboardWidget> widgets, 
        ReportConfiguration config);

    /// <summary>
    /// Generate automated insights from data
    /// </summary>
    Task<List<DataInsight>> GenerateDataInsightsAsync(
        QueryResponse queryResponse, 
        InsightConfiguration? config = null);

    /// <summary>
    /// Generate trend analysis report
    /// </summary>
    Task<TrendAnalysisReport> GenerateTrendAnalysisAsync(
        List<QueryHistoryItem> historicalData, 
        TrendAnalysisConfiguration config);

    /// <summary>
    /// Generate comparative analysis report
    /// </summary>
    Task<ComparativeAnalysisReport> GenerateComparativeAnalysisAsync(
        List<QueryResponse> datasets, 
        ComparativeAnalysisConfiguration config);

    /// <summary>
    /// Export report to various formats
    /// </summary>
    Task<ReportExport> ExportReportAsync(
        string reportId, 
        ExportFormat format, 
        ExportOptions? options = null);

    /// <summary>
    /// Schedule automated report generation
    /// </summary>
    Task<string> ScheduleReportAsync(
        ScheduledReportConfiguration config, 
        string userId);

    /// <summary>
    /// Get report templates
    /// </summary>
    Task<List<ReportTemplate>> GetReportTemplatesAsync(string? category = null);

    /// <summary>
    /// Get report generation metrics
    /// </summary>
    Task<ReportingMetrics> GetReportingMetricsAsync(TimeSpan? timeWindow = null);
}
