using MediatR;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Commands;

// Real-time Streaming Commands

/// <summary>
/// Command to start real-time streaming session
/// </summary>
public class StartStreamingSessionCommand : IRequest<StreamingSession>
{
    public string UserId { get; set; } = string.Empty;
    public StreamingConfiguration Configuration { get; set; } = new();
}

/// <summary>
/// Command to stop streaming session
/// </summary>
public class StopStreamingSessionCommand : IRequest<bool>
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to process data stream event
/// </summary>
public class ProcessDataStreamEventCommand : IRequest<bool>
{
    public DataStreamEvent StreamEvent { get; set; } = new();
}

/// <summary>
/// Command to process query stream event
/// </summary>
public class ProcessQueryStreamEventCommand : IRequest<bool>
{
    public QueryStreamEvent QueryEvent { get; set; } = new();
}

/// <summary>
/// Command to subscribe to data stream
/// </summary>
public class SubscribeToDataStreamCommand : IRequest<string>
{
    public string UserId { get; set; } = string.Empty;
    public StreamSubscription Subscription { get; set; } = new();
}

/// <summary>
/// Command to unsubscribe from data stream
/// </summary>
public class UnsubscribeFromDataStreamCommand : IRequest<bool>
{
    public string SubscriptionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to create real-time alert
/// </summary>
public class CreateRealTimeAlertCommand : IRequest<string>
{
    public RealTimeAlert Alert { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
}

// Real-time Streaming Queries

/// <summary>
/// Query to get real-time dashboard
/// </summary>
public class GetRealTimeDashboardQuery : IRequest<RealTimeDashboard>
{
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Query to get streaming analytics
/// </summary>
public class GetStreamingAnalyticsQuery : IRequest<StreamingAnalyticsResult>
{
    public TimeSpan TimeWindow { get; set; } = TimeSpan.FromHours(1);
    public string? UserId { get; set; }
}

/// <summary>
/// Query to get active streaming sessions
/// </summary>
public class GetActiveStreamingSessionsQuery : IRequest<List<StreamingSession>>
{
    public string? UserId { get; set; }
}

/// <summary>
/// Query to get real-time metrics
/// </summary>
public class GetRealTimeMetricsQuery : IRequest<RealTimeMetrics>
{
    public string? UserId { get; set; }
}

/// <summary>
/// Query to get streaming performance metrics
/// </summary>
public class GetStreamingPerformanceQuery : IRequest<StreamingPerformanceMetrics>
{
    public TimeSpan? TimeWindow { get; set; }
}

// Multi-Modal Dashboard Commands

/// <summary>
/// Command to create dashboard
/// </summary>
public class CreateDashboardCommand : IRequest<Dashboard>
{
    public CreateDashboardRequest Request { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to update dashboard
/// </summary>
public class UpdateDashboardCommand : IRequest<Dashboard>
{
    public string DashboardId { get; set; } = string.Empty;
    public UpdateDashboardRequest Request { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to delete dashboard
/// </summary>
public class DeleteDashboardCommand : IRequest<bool>
{
    public string DashboardId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to add widget to dashboard
/// </summary>
public class AddWidgetToDashboardCommand : IRequest<DashboardWidget>
{
    public string DashboardId { get; set; } = string.Empty;
    public CreateWidgetRequest Request { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to update dashboard widget
/// </summary>
public class UpdateDashboardWidgetCommand : IRequest<DashboardWidget>
{
    public string DashboardId { get; set; } = string.Empty;
    public string WidgetId { get; set; } = string.Empty;
    public UpdateWidgetRequest Request { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to remove widget from dashboard
/// </summary>
public class RemoveWidgetFromDashboardCommand : IRequest<bool>
{
    public string DashboardId { get; set; } = string.Empty;
    public string WidgetId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to generate dashboard from description
/// </summary>
public class GenerateDashboardFromDescriptionCommand : IRequest<Dashboard>
{
    public string Description { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public SchemaMetadata? Schema { get; set; }
}

/// <summary>
/// Command to export dashboard
/// </summary>
public class ExportDashboardCommand : IRequest<DashboardExport>
{
    public string DashboardId { get; set; } = string.Empty;
    public ExportFormat Format { get; set; } = ExportFormat.PDF;
    public ExportOptions? Options { get; set; }
}

/// <summary>
/// Command to share dashboard
/// </summary>
public class ShareDashboardCommand : IRequest<DashboardShare>
{
    public string DashboardId { get; set; } = string.Empty;
    public ShareConfiguration ShareConfig { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to clone dashboard
/// </summary>
public class CloneDashboardCommand : IRequest<Dashboard>
{
    public string DashboardId { get; set; } = string.Empty;
    public string NewName { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to create dashboard from template
/// </summary>
public class CreateDashboardFromTemplateCommand : IRequest<Dashboard>
{
    public string TemplateId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}

// Multi-Modal Dashboard Queries

/// <summary>
/// Query to get dashboard by ID
/// </summary>
public class GetDashboardQuery : IRequest<Dashboard?>
{
    public string DashboardId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Query to get user dashboards
/// </summary>
public class GetUserDashboardsQuery : IRequest<List<Dashboard>>
{
    public string UserId { get; set; } = string.Empty;
    public DashboardFilter? Filter { get; set; }
}

/// <summary>
/// Query to get dashboard templates
/// </summary>
public class GetDashboardTemplatesQuery : IRequest<List<DashboardTemplate>>
{
    public string? Category { get; set; }
}

/// <summary>
/// Query to get dashboard analytics
/// </summary>
public class GetDashboardAnalyticsQuery : IRequest<DashboardAnalytics>
{
    public string DashboardId { get; set; } = string.Empty;
    public TimeSpan? TimeWindow { get; set; }
}

// Advanced Reporting Commands

/// <summary>
/// Command to generate comprehensive report
/// </summary>
public class GenerateComprehensiveReportCommand : IRequest<ComprehensiveReport>
{
    public ReportRequest Request { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to generate text report
/// </summary>
public class GenerateTextReportCommand : IRequest<TextReport>
{
    public string Query { get; set; } = string.Empty;
    public QueryResponse QueryResponse { get; set; } = new();
    public ReportStyle Style { get; set; } = ReportStyle.Executive;
}

/// <summary>
/// Command to generate visual report
/// </summary>
public class GenerateVisualReportCommand : IRequest<VisualReport>
{
    public List<DashboardWidget> Widgets { get; set; } = new();
    public ReportConfiguration Configuration { get; set; } = new();
}

/// <summary>
/// Command to generate data insights
/// </summary>
public class GenerateDataInsightsCommand : IRequest<List<DataInsight>>
{
    public QueryResponse QueryResponse { get; set; } = new();
    public InsightConfiguration? Configuration { get; set; }
}

/// <summary>
/// Command to schedule report
/// </summary>
public class ScheduleReportCommand : IRequest<string>
{
    public ScheduledReportConfiguration Configuration { get; set; } = new();
    public string UserId { get; set; } = string.Empty;
}

/// <summary>
/// Command to export report
/// </summary>
public class ExportReportCommand : IRequest<ReportExport>
{
    public string ReportId { get; set; } = string.Empty;
    public ExportFormat Format { get; set; } = ExportFormat.PDF;
    public ExportOptions? Options { get; set; }
}

// Advanced Reporting Queries

/// <summary>
/// Query to get report templates
/// </summary>
public class GetReportTemplatesQuery : IRequest<List<ReportTemplate>>
{
    public string? Category { get; set; }
}

/// <summary>
/// Query to get reporting metrics
/// </summary>
public class GetReportingMetricsQuery : IRequest<ReportingMetrics>
{
    public TimeSpan? TimeWindow { get; set; }
}

// Additional queries for NLU and Schema Optimization

// Duplicate commands removed - these are defined in QueryIntelligenceCommands.cs
