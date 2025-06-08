using MediatR;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Interfaces;

namespace BIReportingCopilot.Infrastructure.Handlers;

// Real-time Streaming Command Handlers

/// <summary>
/// Command handler for starting streaming session
/// </summary>
public class StartStreamingSessionCommandHandler : IRequestHandler<StartStreamingSessionCommand, StreamingSession>
{
    private readonly ILogger<StartStreamingSessionCommandHandler> _logger;
    private readonly IRealTimeStreamingService _streamingService;

    public StartStreamingSessionCommandHandler(
        ILogger<StartStreamingSessionCommandHandler> logger,
        IRealTimeStreamingService streamingService)
    {
        _logger = logger;
        _streamingService = streamingService;
    }

    public async Task<StreamingSession> Handle(StartStreamingSessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("🎬 Processing start streaming session command for user {UserId}", request.UserId);

            var session = await _streamingService.StartStreamingSessionAsync(request.UserId, request.Configuration);

            _logger.LogInformation("🎬 Streaming session {SessionId} started for user {UserId}", 
                session.SessionId, request.UserId);

            return session;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error starting streaming session for user {UserId}", request.UserId);
            throw;
        }
    }
}

/// <summary>
/// Command handler for stopping streaming session
/// </summary>
public class StopStreamingSessionCommandHandler : IRequestHandler<StopStreamingSessionCommand, bool>
{
    private readonly ILogger<StopStreamingSessionCommandHandler> _logger;
    private readonly IRealTimeStreamingService _streamingService;

    public StopStreamingSessionCommandHandler(
        ILogger<StopStreamingSessionCommandHandler> logger,
        IRealTimeStreamingService streamingService)
    {
        _logger = logger;
        _streamingService = streamingService;
    }

    public async Task<bool> Handle(StopStreamingSessionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("🛑 Processing stop streaming session command for session {SessionId}", request.SessionId);

            var result = await _streamingService.StopStreamingSessionAsync(request.SessionId, request.UserId);

            _logger.LogInformation("🛑 Streaming session {SessionId} stopped: {Success}", request.SessionId, result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error stopping streaming session {SessionId}", request.SessionId);
            return false;
        }
    }
}

/// <summary>
/// Command handler for processing data stream events
/// </summary>
public class ProcessDataStreamEventCommandHandler : IRequestHandler<ProcessDataStreamEventCommand, bool>
{
    private readonly ILogger<ProcessDataStreamEventCommandHandler> _logger;
    private readonly IRealTimeStreamingService _streamingService;

    public ProcessDataStreamEventCommandHandler(
        ILogger<ProcessDataStreamEventCommandHandler> logger,
        IRealTimeStreamingService streamingService)
    {
        _logger = logger;
        _streamingService = streamingService;
    }

    public async Task<bool> Handle(ProcessDataStreamEventCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("📊 Processing data stream event: {EventType}", request.StreamEvent.EventType);

            await _streamingService.ProcessDataStreamEventAsync(request.StreamEvent);

            _logger.LogDebug("📊 Data stream event processed: {EventId}", request.StreamEvent.EventId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing data stream event {EventId}", request.StreamEvent.EventId);
            return false;
        }
    }
}

/// <summary>
/// Query handler for getting real-time dashboard
/// </summary>
public class GetRealTimeDashboardQueryHandler : IRequestHandler<GetRealTimeDashboardQuery, RealTimeDashboard>
{
    private readonly ILogger<GetRealTimeDashboardQueryHandler> _logger;
    private readonly IRealTimeStreamingService _streamingService;

    public GetRealTimeDashboardQueryHandler(
        ILogger<GetRealTimeDashboardQueryHandler> logger,
        IRealTimeStreamingService streamingService)
    {
        _logger = logger;
        _streamingService = streamingService;
    }

    public async Task<RealTimeDashboard> Handle(GetRealTimeDashboardQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("📈 Processing get real-time dashboard query for user {UserId}", request.UserId);

            var dashboard = await _streamingService.GetRealTimeDashboardAsync(request.UserId);

            _logger.LogInformation("📈 Real-time dashboard retrieved for user {UserId} with {ChartCount} charts", 
                request.UserId, dashboard.LiveCharts.Count);

            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting real-time dashboard for user {UserId}", request.UserId);
            return new RealTimeDashboard { UserId = request.UserId };
        }
    }
}

// Multi-Modal Dashboard Command Handlers

/// <summary>
/// Command handler for creating dashboard
/// </summary>
public class CreateDashboardCommandHandler : IRequestHandler<CreateDashboardCommand, Dashboard>
{
    private readonly ILogger<CreateDashboardCommandHandler> _logger;
    private readonly IMultiModalDashboardService _dashboardService;

    public CreateDashboardCommandHandler(
        ILogger<CreateDashboardCommandHandler> logger,
        IMultiModalDashboardService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }

    public async Task<Dashboard> Handle(CreateDashboardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("🎨 Processing create dashboard command for user {UserId}: {Name}", 
                request.UserId, request.Request.Name);

            var dashboard = await _dashboardService.CreateDashboardAsync(request.Request, request.UserId);

            _logger.LogInformation("🎨 Dashboard '{Name}' created with ID {DashboardId} for user {UserId}", 
                dashboard.Name, dashboard.DashboardId, request.UserId);

            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating dashboard for user {UserId}", request.UserId);
            throw;
        }
    }
}

/// <summary>
/// Command handler for generating dashboard from description
/// </summary>
public class GenerateDashboardFromDescriptionCommandHandler : IRequestHandler<GenerateDashboardFromDescriptionCommand, Dashboard>
{
    private readonly ILogger<GenerateDashboardFromDescriptionCommandHandler> _logger;
    private readonly IMultiModalDashboardService _dashboardService;

    public GenerateDashboardFromDescriptionCommandHandler(
        ILogger<GenerateDashboardFromDescriptionCommandHandler> logger,
        IMultiModalDashboardService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }

    public async Task<Dashboard> Handle(GenerateDashboardFromDescriptionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("🤖 Processing generate dashboard from description command for user {UserId}", request.UserId);

            var dashboard = await _dashboardService.GenerateDashboardFromDescriptionAsync(
                request.Description, request.UserId, request.Schema);

            _logger.LogInformation("🤖 AI-generated dashboard '{Name}' created for user {UserId} with {WidgetCount} widgets", 
                dashboard.Name, request.UserId, dashboard.Widgets.Count);

            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating dashboard from description for user {UserId}", request.UserId);
            throw;
        }
    }
}

/// <summary>
/// Query handler for getting user dashboards
/// </summary>
public class GetUserDashboardsQueryHandler : IRequestHandler<GetUserDashboardsQuery, List<Dashboard>>
{
    private readonly ILogger<GetUserDashboardsQueryHandler> _logger;
    private readonly IMultiModalDashboardService _dashboardService;

    public GetUserDashboardsQueryHandler(
        ILogger<GetUserDashboardsQueryHandler> logger,
        IMultiModalDashboardService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }

    public async Task<List<Dashboard>> Handle(GetUserDashboardsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("📋 Processing get user dashboards query for user {UserId}", request.UserId);

            var dashboards = await _dashboardService.GetUserDashboardsAsync(request.UserId, request.Filter);

            _logger.LogInformation("📋 Retrieved {Count} dashboards for user {UserId}", dashboards.Count, request.UserId);

            return dashboards;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting dashboards for user {UserId}", request.UserId);
            return new List<Dashboard>();
        }
    }
}

/// <summary>
/// Query handler for getting dashboard templates
/// </summary>
public class GetDashboardTemplatesQueryHandler : IRequestHandler<GetDashboardTemplatesQuery, List<DashboardTemplate>>
{
    private readonly ILogger<GetDashboardTemplatesQueryHandler> _logger;
    private readonly IMultiModalDashboardService _dashboardService;

    public GetDashboardTemplatesQueryHandler(
        ILogger<GetDashboardTemplatesQueryHandler> logger,
        IMultiModalDashboardService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }

    public async Task<List<DashboardTemplate>> Handle(GetDashboardTemplatesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("📋 Processing get dashboard templates query for category: {Category}", request.Category ?? "all");

            var templates = await _dashboardService.GetDashboardTemplatesAsync(request.Category);

            _logger.LogInformation("📋 Retrieved {Count} dashboard templates", templates.Count);

            return templates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting dashboard templates");
            return new List<DashboardTemplate>();
        }
    }
}

/// <summary>
/// Command handler for creating dashboard from template
/// </summary>
public class CreateDashboardFromTemplateCommandHandler : IRequestHandler<CreateDashboardFromTemplateCommand, Dashboard>
{
    private readonly ILogger<CreateDashboardFromTemplateCommandHandler> _logger;
    private readonly IMultiModalDashboardService _dashboardService;

    public CreateDashboardFromTemplateCommandHandler(
        ILogger<CreateDashboardFromTemplateCommandHandler> logger,
        IMultiModalDashboardService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }

    public async Task<Dashboard> Handle(CreateDashboardFromTemplateCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("📋 Processing create dashboard from template command: {TemplateId} for user {UserId}", 
                request.TemplateId, request.UserId);

            var dashboard = await _dashboardService.CreateDashboardFromTemplateAsync(
                request.TemplateId, request.Name, request.UserId, request.Parameters);

            _logger.LogInformation("📋 Dashboard '{Name}' created from template {TemplateId} for user {UserId}", 
                dashboard.Name, request.TemplateId, request.UserId);

            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating dashboard from template {TemplateId} for user {UserId}", 
                request.TemplateId, request.UserId);
            throw;
        }
    }
}

/// <summary>
/// Command handler for exporting dashboard
/// </summary>
public class ExportDashboardCommandHandler : IRequestHandler<ExportDashboardCommand, DashboardExport>
{
    private readonly ILogger<ExportDashboardCommandHandler> _logger;
    private readonly IMultiModalDashboardService _dashboardService;

    public ExportDashboardCommandHandler(
        ILogger<ExportDashboardCommandHandler> logger,
        IMultiModalDashboardService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }

    public async Task<DashboardExport> Handle(ExportDashboardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("📤 Processing export dashboard command: {DashboardId} to {Format}", 
                request.DashboardId, request.Format);

            var export = await _dashboardService.ExportDashboardAsync(
                request.DashboardId, request.Format, request.Options);

            _logger.LogInformation("📤 Dashboard {DashboardId} exported to {Format} ({Size} bytes)", 
                request.DashboardId, request.Format, export.Data.Length);

            return export;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error exporting dashboard {DashboardId}", request.DashboardId);
            throw;
        }
    }
}

/// <summary>
/// Command handler for adding widget to dashboard
/// </summary>
public class AddWidgetToDashboardCommandHandler : IRequestHandler<AddWidgetToDashboardCommand, DashboardWidget>
{
    private readonly ILogger<AddWidgetToDashboardCommandHandler> _logger;
    private readonly IMultiModalDashboardService _dashboardService;

    public AddWidgetToDashboardCommandHandler(
        ILogger<AddWidgetToDashboardCommandHandler> logger,
        IMultiModalDashboardService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }

    public async Task<DashboardWidget> Handle(AddWidgetToDashboardCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("🧩 Processing add widget to dashboard command: {DashboardId}", request.DashboardId);

            var widget = await _dashboardService.AddWidgetToDashboardAsync(
                request.DashboardId, request.Request, request.UserId);

            _logger.LogInformation("🧩 Widget '{Title}' added to dashboard {DashboardId}", 
                widget.Title, request.DashboardId);

            return widget;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error adding widget to dashboard {DashboardId}", request.DashboardId);
            throw;
        }
    }
}

/// <summary>
/// Query handler for getting streaming analytics
/// </summary>
public class GetStreamingAnalyticsQueryHandler : IRequestHandler<GetStreamingAnalyticsQuery, StreamingAnalyticsResult>
{
    private readonly ILogger<GetStreamingAnalyticsQueryHandler> _logger;
    private readonly IRealTimeStreamingService _streamingService;

    public GetStreamingAnalyticsQueryHandler(
        ILogger<GetStreamingAnalyticsQueryHandler> logger,
        IRealTimeStreamingService streamingService)
    {
        _logger = logger;
        _streamingService = streamingService;
    }

    public async Task<StreamingAnalyticsResult> Handle(GetStreamingAnalyticsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("📊 Processing get streaming analytics query for window {TimeWindow}", request.TimeWindow);

            var analytics = await _streamingService.GetStreamingAnalyticsAsync(request.TimeWindow, request.UserId);

            _logger.LogInformation("📊 Streaming analytics retrieved - Events: {EventCount}, Users: {UserCount}", 
                analytics.TotalEvents, analytics.UserActivitySummary.ActiveUsers);

            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting streaming analytics");
            return new StreamingAnalyticsResult
            {
                TimeWindow = request.TimeWindow,
                StartTime = DateTime.UtcNow - request.TimeWindow,
                EndTime = DateTime.UtcNow
            };
        }
    }
}

/// <summary>
/// Command handler for subscribing to data stream
/// </summary>
public class SubscribeToDataStreamCommandHandler : IRequestHandler<SubscribeToDataStreamCommand, string>
{
    private readonly ILogger<SubscribeToDataStreamCommandHandler> _logger;
    private readonly IRealTimeStreamingService _streamingService;

    public SubscribeToDataStreamCommandHandler(
        ILogger<SubscribeToDataStreamCommandHandler> logger,
        IRealTimeStreamingService streamingService)
    {
        _logger = logger;
        _streamingService = streamingService;
    }

    public async Task<string> Handle(SubscribeToDataStreamCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("📡 Processing subscribe to data stream command for user {UserId}", request.UserId);

            var subscriptionId = await _streamingService.SubscribeToDataStreamAsync(request.UserId, request.Subscription);

            _logger.LogInformation("📡 Data stream subscription {SubscriptionId} created for user {UserId}", 
                subscriptionId, request.UserId);

            return subscriptionId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error subscribing to data stream for user {UserId}", request.UserId);
            throw;
        }
    }
}

/// <summary>
/// Command handler for creating real-time alert
/// </summary>
public class CreateRealTimeAlertCommandHandler : IRequestHandler<CreateRealTimeAlertCommand, string>
{
    private readonly ILogger<CreateRealTimeAlertCommandHandler> _logger;
    private readonly IRealTimeStreamingService _streamingService;

    public CreateRealTimeAlertCommandHandler(
        ILogger<CreateRealTimeAlertCommandHandler> logger,
        IRealTimeStreamingService streamingService)
    {
        _logger = logger;
        _streamingService = streamingService;
    }

    public async Task<string> Handle(CreateRealTimeAlertCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("🚨 Processing create real-time alert command for user {UserId}", request.UserId);

            var alertId = await _streamingService.CreateRealTimeAlertAsync(request.Alert, request.UserId);

            _logger.LogInformation("🚨 Real-time alert {AlertId} created for user {UserId}: {Title}", 
                alertId, request.UserId, request.Alert.Title);

            return alertId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating real-time alert for user {UserId}", request.UserId);
            throw;
        }
    }
}
