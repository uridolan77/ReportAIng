using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using DashboardModel = BIReportingCopilot.Core.Models.Dashboard;

namespace BIReportingCopilot.Infrastructure.AI.Dashboard;

/// <summary>
/// Multi-Modal Dashboard service
/// Provides advanced dashboard creation, management, and reporting
/// </summary>
public class MultiModalDashboardService : IMultiModalDashboardService
{
    private readonly ILogger<MultiModalDashboardService> _logger;
    private readonly IDbContextFactory _contextFactory;
    private readonly DashboardCreationService _creationService;
    private readonly DashboardTemplateService _templateService;
    private readonly IQueryService _queryService;
    private readonly IAIService _aiService;

    public MultiModalDashboardService(
        ILogger<MultiModalDashboardService> logger,
        IDbContextFactory contextFactory,
        DashboardCreationService creationService,
        DashboardTemplateService templateService,
        IQueryService queryService,
        IAIService aiService)
    {
        _logger = logger;
        _contextFactory = contextFactory;
        _creationService = creationService;
        _templateService = templateService;
        _queryService = queryService;
        _aiService = aiService;

        _logger.LogInformation("🎨 Multi-Modal Dashboard Service initialized with modular components");
    }

    /// <summary>
    /// Create multi-modal dashboard
    /// </summary>
    public async Task<DashboardModel> CreateDashboardAsync(CreateDashboardRequest request, string userId)
    {
        try
        {
            // Use the modular creation service
            var dashboard = await _creationService.CreateDashboardAsync(request, userId);

            // Store dashboard in database
            await StoreDashboardAsync(dashboard);

            _logger.LogInformation("🎨 Dashboard '{Name}' created with ID {DashboardId} and {WidgetCount} widgets",
                dashboard.Name, dashboard.DashboardId, dashboard.Widgets.Count);

            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating dashboard for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Update existing dashboard
    /// </summary>
    public async Task<DashboardModel> UpdateDashboardAsync(string dashboardId, UpdateDashboardRequest request, string userId)
    {
        try
        {
            _logger.LogInformation("🔄 Updating dashboard {DashboardId} for user {UserId}", dashboardId, userId);

            var dashboard = await GetDashboardAsync(dashboardId, userId);
            if (dashboard == null)
            {
                throw new ArgumentException($"Dashboard {dashboardId} not found or access denied");
            }

            // Update properties
            if (!string.IsNullOrEmpty(request.Name)) dashboard.Name = request.Name;
            if (request.Description != null) dashboard.Description = request.Description;
            if (request.Category != null) dashboard.Category = request.Category;
            if (request.Layout != null) dashboard.Layout = request.Layout;
            if (request.Configuration != null) dashboard.Configuration = request.Configuration;
            if (request.Tags != null) dashboard.Tags = request.Tags;
            if (request.IsPublic.HasValue) dashboard.IsPublic = request.IsPublic.Value;

            dashboard.UpdatedAt = DateTime.UtcNow;

            // Update in database
            await UpdateDashboardInDatabaseAsync(dashboard);

            _logger.LogInformation("🔄 Dashboard {DashboardId} updated successfully", dashboardId);
            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating dashboard {DashboardId}", dashboardId);
            throw;
        }
    }

    /// <summary>
    /// Delete dashboard
    /// </summary>
    public async Task<bool> DeleteDashboardAsync(string dashboardId, string userId)
    {
        try
        {
            _logger.LogInformation("🗑️ Deleting dashboard {DashboardId} for user {UserId}", dashboardId, userId);

            var dashboard = await GetDashboardAsync(dashboardId, userId);
            if (dashboard == null || dashboard.UserId != userId)
            {
                return false;
            }

            await DeleteDashboardFromDatabaseAsync(dashboardId);

            _logger.LogInformation("🗑️ Dashboard {DashboardId} deleted successfully", dashboardId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error deleting dashboard {DashboardId}", dashboardId);
            return false;
        }
    }

    /// <summary>
    /// Get dashboard by ID
    /// </summary>
    public async Task<DashboardModel?> GetDashboardAsync(string dashboardId, string userId)
    {
        try
        {
            _logger.LogDebug("📋 Getting dashboard {DashboardId} for user {UserId}", dashboardId, userId);

            var dashboard = await LoadDashboardFromDatabaseAsync(dashboardId);
            
            if (dashboard == null)
            {
                return null;
            }

            // Check permissions
            if (!HasDashboardAccess(dashboard, userId))
            {
                _logger.LogWarning("🚫 User {UserId} denied access to dashboard {DashboardId}", userId, dashboardId);
                return null;
            }

            // Update last viewed
            dashboard.LastViewedAt = DateTime.UtcNow;
            await UpdateLastViewedAsync(dashboardId, userId);

            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting dashboard {DashboardId}", dashboardId);
            return null;
        }
    }

    /// <summary>
    /// Get user's dashboards
    /// </summary>
    public async Task<List<DashboardModel>> GetUserDashboardsAsync(string userId, DashboardFilter? filter = null)
    {
        try
        {
            _logger.LogDebug("📋 Getting dashboards for user {UserId}", userId);

            var dashboards = await LoadUserDashboardsFromDatabaseAsync(userId, filter);

            _logger.LogInformation("📋 Retrieved {Count} dashboards for user {UserId}", dashboards.Count, userId);
            return dashboards;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting dashboards for user {UserId}", userId);
            return new List<DashboardModel>();
        }
    }

    /// <summary>
    /// Add widget to dashboard
    /// </summary>
    public async Task<DashboardWidget> AddWidgetToDashboardAsync(
        string dashboardId, 
        CreateWidgetRequest request, 
        string userId)
    {
        try
        {
            _logger.LogInformation("🧩 Adding widget '{Title}' to dashboard {DashboardId}", request.Title, dashboardId);

            var dashboard = await GetDashboardAsync(dashboardId, userId);
            if (dashboard == null || !HasEditAccess(dashboard, userId))
            {
                throw new UnauthorizedAccessException("Dashboard not found or access denied");
            }

            var widget = await CreateWidgetFromRequestAsync(request, dashboardId);
            dashboard.Widgets.Add(widget);
            dashboard.UpdatedAt = DateTime.UtcNow;

            await UpdateDashboardInDatabaseAsync(dashboard);

            _logger.LogInformation("🧩 Widget '{Title}' added to dashboard {DashboardId}", widget.Title, dashboardId);
            return widget;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error adding widget to dashboard {DashboardId}", dashboardId);
            throw;
        }
    }

    /// <summary>
    /// Generate dashboard from natural language description
    /// </summary>
    public async Task<DashboardModel> GenerateDashboardFromDescriptionAsync(
        string description,
        string userId,
        SchemaMetadata? schema = null)
    {
        try
        {
            // Use the modular creation service for AI generation
            var dashboard = await _creationService.GenerateFromDescriptionAsync(description, userId, schema);

            // Store dashboard in database
            await StoreDashboardAsync(dashboard);

            _logger.LogInformation("🤖 AI-generated dashboard '{Name}' created with {WidgetCount} widgets",
                dashboard.Name, dashboard.Widgets.Count);

            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating dashboard from description");
            throw;
        }
    }

    /// <summary>
    /// Export dashboard to various formats
    /// </summary>
    public async Task<DashboardExport> ExportDashboardAsync(
        string dashboardId, 
        ExportFormat format, 
        ExportOptions? options = null)
    {
        try
        {
            _logger.LogInformation("📤 Exporting dashboard {DashboardId} to {Format}", dashboardId, format);

            var dashboard = await LoadDashboardFromDatabaseAsync(dashboardId);
            if (dashboard == null)
            {
                throw new ArgumentException($"Dashboard {dashboardId} not found");
            }

            var export = new DashboardExport
            {
                DashboardId = dashboardId,
                Format = format,
                GeneratedAt = DateTime.UtcNow
            };

            switch (format)
            {
                case ExportFormat.JSON:
                    export.Data = System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dashboard));
                    export.ContentType = "application/json";
                    export.FileName = $"{dashboard.Name}_{DateTime.UtcNow:yyyyMMdd}.json";
                    break;

                case ExportFormat.PDF:
                    export.Data = await GeneratePdfExportAsync(dashboard, options);
                    export.ContentType = "application/pdf";
                    export.FileName = $"{dashboard.Name}_{DateTime.UtcNow:yyyyMMdd}.pdf";
                    break;

                case ExportFormat.PNG:
                    export.Data = await GenerateImageExportAsync(dashboard, options);
                    export.ContentType = "image/png";
                    export.FileName = $"{dashboard.Name}_{DateTime.UtcNow:yyyyMMdd}.png";
                    break;

                default:
                    throw new NotSupportedException($"Export format {format} not supported");
            }

            export.Metadata = new ExportMetadata
            {
                FileSizeBytes = export.Data.Length,
                GeneratedBy = "System"
            };

            _logger.LogInformation("📤 Dashboard {DashboardId} exported to {Format} ({Size} bytes)", 
                dashboardId, format, export.Data.Length);

            return export;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error exporting dashboard {DashboardId}", dashboardId);
            throw;
        }
    }

    /// <summary>
    /// Get dashboard templates
    /// </summary>
    public async Task<List<DashboardTemplate>> GetDashboardTemplatesAsync(string? category = null)
    {
        return await _templateService.GetTemplatesAsync(category);
    }

    /// <summary>
    /// Create dashboard from template
    /// </summary>
    public async Task<DashboardModel> CreateDashboardFromTemplateAsync(
        string templateId,
        string name,
        string userId,
        Dictionary<string, object>? parameters = null)
    {
        try
        {
            // Use the modular template service
            var dashboard = await _templateService.CreateFromTemplateAsync(templateId, name, userId, parameters);

            // Store dashboard in database
            await StoreDashboardAsync(dashboard);

            _logger.LogInformation("📋 Dashboard '{Name}' created from template {TemplateId}", name, templateId);
            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating dashboard from template {TemplateId}", templateId);
            throw;
        }
    }

    /// <summary>
    /// Generate automated report from dashboard
    /// </summary>
    public async Task<GeneratedReport> GenerateReportAsync(string dashboardId, ReportConfiguration config, string userId)
    {
        try
        {
            _logger.LogInformation("📊 Generating report for dashboard {DashboardId} by user {UserId}", dashboardId, userId);

            var dashboard = await GetDashboardAsync(dashboardId, userId);
            if (dashboard == null)
            {
                throw new ArgumentException($"Dashboard {dashboardId} not found or access denied");
            }

            var report = new GeneratedReport
            {
                ReportId = Guid.NewGuid().ToString(),
                Title = config.Title ?? $"Report for {dashboard.Name}",
                Type = ReportType.Dashboard,
                Data = System.Text.Encoding.UTF8.GetBytes($"Dashboard Report: {dashboard.Name}"),
                ContentType = "application/json",
                GeneratedAt = DateTime.UtcNow,
                GeneratedBy = userId,
                Metadata = new ReportMetadata
                {
                    DashboardId = dashboardId,
                    WidgetCount = dashboard.Widgets.Count,
                    GenerationMethod = "AI",
                    CustomData = new Dictionary<string, object>
                    {
                        ["dashboard_name"] = dashboard.Name,
                        ["widget_count"] = dashboard.Widgets.Count
                    }
                }
            };

            _logger.LogInformation("📊 Report generated for dashboard {DashboardId} with {WidgetCount} widgets",
                dashboardId, dashboard.Widgets.Count);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating report for dashboard {DashboardId}", dashboardId);
            throw;
        }
    }

    // Interface compliance methods (placeholder implementations)
    public Task<DashboardWidget> UpdateWidgetAsync(string dashboardId, string widgetId, UpdateWidgetRequest request, string userId)
    {
        return Task.FromResult(new DashboardWidget());
    }

    public Task<bool> RemoveWidgetAsync(string dashboardId, string widgetId, string userId)
    {
        return Task.FromResult(false);
    }

    public Task<DashboardShare> ShareDashboardAsync(string dashboardId, ShareConfiguration shareConfig, string userId)
    {
        return Task.FromResult(new DashboardShare());
    }

    public Task<DashboardAnalytics> GetDashboardAnalyticsAsync(string dashboardId, TimeSpan? timeWindow = null)
    {
        return Task.FromResult(new DashboardAnalytics());
    }

    public Task<DashboardModel> CloneDashboardAsync(string dashboardId, string newName, string userId)
    {
        return Task.FromResult(new DashboardModel());
    }

    // Helper methods
    private async Task<DashboardWidget> CreateWidgetFromRequestAsync(CreateWidgetRequest request, string dashboardId)
    {
        var widget = new DashboardWidget
        {
            Title = request.Title,
            Description = request.Description ?? "",
            Type = request.Type,
            Position = request.Position ?? new WidgetPosition(),
            Size = request.Size ?? new WidgetSize(),
            Configuration = request.Configuration ?? new WidgetConfiguration(),
            DataSource = request.DataSource,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Validate and process data source
        await ValidateAndProcessDataSourceAsync(widget.DataSource);

        return widget;
    }

    private async Task ValidateAndProcessDataSourceAsync(WidgetDataSource dataSource)
    {
        if (dataSource.Type == DataSourceType.Query && !string.IsNullOrEmpty(dataSource.Query))
        {
            // Validate query syntax and generate SQL if needed
            try
            {
                if (string.IsNullOrEmpty(dataSource.SqlQuery))
                {
                    dataSource.SqlQuery = await _aiService.GenerateSQLAsync(dataSource.Query);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not generate SQL for widget query: {Query}", dataSource.Query);
            }
        }
    }

    private BIReportingCopilot.Core.Models.DashboardLayout CreateDefaultLayout()
    {
        return new BIReportingCopilot.Core.Models.DashboardLayout
        {
            Type = BIReportingCopilot.Core.Models.LayoutType.Grid,
            Columns = 12,
            Rows = 10,
            Configuration = new LayoutConfiguration
            {
                GridGap = 10,
                EnableDragAndDrop = true,
                EnableResize = true
            }
        };
    }

    private bool HasDashboardAccess(DashboardModel dashboard, string userId)
    {
        return dashboard.UserId == userId ||
               dashboard.IsPublic ||
               dashboard.Permissions.UserPermissions.Any(up => up.UserId == userId);
    }

    private bool HasEditAccess(DashboardModel dashboard, string userId)
    {
        if (dashboard.UserId == userId) return true;

        var userPermission = dashboard.Permissions.UserPermissions
            .FirstOrDefault(up => up.UserId == userId);

        return userPermission?.Permission >= PermissionLevel.Edit;
    }

    // Database operations (placeholder implementations)
    private Task StoreDashboardAsync(DashboardModel dashboard)
    {
        return Task.CompletedTask;
    }

    private Task UpdateDashboardInDatabaseAsync(DashboardModel dashboard)
    {
        return Task.CompletedTask;
    }

    private Task DeleteDashboardFromDatabaseAsync(string dashboardId)
    {
        return Task.CompletedTask;
    }

    private Task<DashboardModel?> LoadDashboardFromDatabaseAsync(string dashboardId)
    {
        return Task.FromResult<DashboardModel?>(null);
    }

    private Task<List<DashboardModel>> LoadUserDashboardsFromDatabaseAsync(string userId, DashboardFilter? filter)
    {
        return Task.FromResult(new List<DashboardModel>());
    }

    private Task UpdateLastViewedAsync(string dashboardId, string userId)
    {
        return Task.CompletedTask;
    }

    // Export implementations (placeholder)
    private Task<byte[]> GeneratePdfExportAsync(DashboardModel dashboard, ExportOptions? options)
    {
        return Task.FromResult(Array.Empty<byte>());
    }

    private Task<byte[]> GenerateImageExportAsync(DashboardModel dashboard, ExportOptions? options)
    {
        return Task.FromResult(Array.Empty<byte>());
    }

    #region Missing Interface Method Implementation

    /// <summary>
    /// Create dashboard (IMultiModalDashboardService interface)
    /// </summary>
    public async Task<DashboardResult> CreateDashboardAsync(DashboardRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🎨 Creating dashboard from request: {Title}", request.Title);

            // Convert DashboardRequest to CreateDashboardRequest
            var createRequest = new CreateDashboardRequest
            {
                Name = request.Title,
                Description = request.Description,
                Category = "General", // DashboardRequest doesn't have Category property
                Layout = CreateDefaultLayout(),
                Configuration = new DashboardConfiguration
                {
                    Theme = new ThemeConfiguration(), // Use ThemeConfiguration object instead of string
                    RefreshInterval = TimeSpan.FromMinutes(5),
                    AutoRefresh = true, // Use AutoRefresh instead of EnableAutoRefresh
                    EnableExport = true,
                    EnableSharing = true
                },
                Tags = request.Tags,
                IsPublic = request.IsPublic
            };

            // Create dashboard using existing method
            var dashboard = await CreateDashboardAsync(createRequest, request.UserId);

            return new DashboardResult
            {
                DashboardId = dashboard.DashboardId,
                Title = dashboard.Name,
                Description = dashboard.Description,
                Status = "Success",
                CreatedAt = dashboard.CreatedAt,
                Widgets = dashboard.Widgets,
                WidgetResults = dashboard.Widgets.Select(w => new WidgetResult
                {
                    WidgetId = w.WidgetId,
                    Title = w.Title,
                    Type = w.Type.ToString(),
                    Status = "Success"
                }).ToList(),
                Metadata = new Dictionary<string, object>
                {
                    ["widget_count"] = dashboard.Widgets.Count,
                    ["category"] = dashboard.Category,
                    ["is_public"] = dashboard.IsPublic
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating dashboard from request");
            return new DashboardResult
            {
                DashboardId = Guid.NewGuid().ToString(),
                Title = request.Title,
                Status = "Error",
                CreatedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                }
            };
        }
    }

    #endregion

    #region Missing Interface Method Implementation

    /// <summary>
    /// Create dashboard (IMultiModalDashboardService interface)
    /// </summary>
    async Task<DashboardResult> IMultiModalDashboardService.CreateDashboardAsync(DashboardRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🎨 Creating dashboard from request: {Title}", request.Title);

            // Convert DashboardRequest to CreateDashboardRequest
            var createRequest = new CreateDashboardRequest
            {
                Name = request.Title,
                Description = request.Description,
                Category = "General", // DashboardRequest doesn't have Category property
                Layout = CreateDefaultLayout(),
                Configuration = new DashboardConfiguration
                {
                    Theme = new ThemeConfiguration(), // Use ThemeConfiguration object instead of string
                    RefreshInterval = TimeSpan.FromMinutes(5),
                    AutoRefresh = true, // Use AutoRefresh instead of EnableAutoRefresh
                    EnableExport = true,
                    EnableSharing = true
                },
                Tags = request.Tags,
                IsPublic = request.IsPublic
            };

            // Create dashboard using existing method
            var dashboard = await CreateDashboardAsync(createRequest, request.UserId);

            return new DashboardResult
            {
                DashboardId = dashboard.DashboardId,
                Title = dashboard.Name,
                Description = dashboard.Description,
                Status = "Success",
                CreatedAt = dashboard.CreatedAt,
                Widgets = dashboard.Widgets,
                WidgetResults = dashboard.Widgets.Select(w => new WidgetResult
                {
                    WidgetId = w.WidgetId,
                    Title = w.Title,
                    Type = w.Type.ToString(),
                    Status = "Success"
                }).ToList(),
                Metadata = new Dictionary<string, object>
                {
                    ["widget_count"] = dashboard.Widgets.Count,
                    ["category"] = dashboard.Category,
                    ["is_public"] = dashboard.IsPublic
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating dashboard from request");
            return new DashboardResult
            {
                DashboardId = Guid.NewGuid().ToString(),
                Title = request.Title,
                Status = "Error",
                CreatedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                }
            };
        }
    }

    /// <summary>
    /// Generate dashboard from description (IMultiModalDashboardService interface)
    /// </summary>
    public async Task<BIReportingCopilot.Core.Models.Dashboard> GenerateDashboardFromDescriptionAsync(string description, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var dashboardModel = await GenerateDashboardFromDescriptionAsync(description, userId, (SchemaMetadata?)null);

            // Convert DashboardModel to Dashboard
            return new BIReportingCopilot.Core.Models.Dashboard
            {
                DashboardId = dashboardModel.DashboardId,
                Name = dashboardModel.Name,
                Description = dashboardModel.Description,
                UserId = dashboardModel.UserId,
                Category = dashboardModel.Category,
                Widgets = dashboardModel.Widgets,
                Layout = dashboardModel.Layout,
                Configuration = dashboardModel.Configuration,
                Permissions = dashboardModel.Permissions,
                CreatedAt = dashboardModel.CreatedAt,
                UpdatedAt = dashboardModel.UpdatedAt,
                LastViewedAt = dashboardModel.LastViewedAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating dashboard from description via interface");
            throw;
        }
    }

    #endregion
}
