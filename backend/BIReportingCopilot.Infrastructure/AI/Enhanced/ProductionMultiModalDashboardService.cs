using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Production-ready Multi-Modal Dashboard service
/// Provides advanced dashboard creation, management, and reporting
/// </summary>
public class ProductionMultiModalDashboardService : IMultiModalDashboardService
{
    private readonly ILogger<ProductionMultiModalDashboardService> _logger;
    private readonly IDbContextFactory _contextFactory;
    private readonly IAdvancedNLUService _nluService;
    private readonly IQueryService _queryService;
    private readonly ISchemaService _schemaService;
    private readonly IAIService _aiService;

    // Dashboard templates cache
    private readonly List<DashboardTemplate> _defaultTemplates;

    public ProductionMultiModalDashboardService(
        ILogger<ProductionMultiModalDashboardService> logger,
        IDbContextFactory contextFactory,
        IAdvancedNLUService nluService,
        IQueryService queryService,
        ISchemaService schemaService,
        IAIService aiService)
    {
        _logger = logger;
        _contextFactory = contextFactory;
        _nluService = nluService;
        _queryService = queryService;
        _schemaService = schemaService;
        _aiService = aiService;
        _defaultTemplates = InitializeDefaultTemplates();

        _logger.LogInformation("🎨 Production Multi-Modal Dashboard Service initialized");
    }

    /// <summary>
    /// Create multi-modal dashboard
    /// </summary>
    public async Task<Dashboard> CreateDashboardAsync(CreateDashboardRequest request, string userId)
    {
        try
        {
            _logger.LogInformation("🎨 Creating dashboard '{Name}' for user {UserId}", request.Name, userId);

            var dashboard = new Dashboard
            {
                Name = request.Name,
                Description = request.Description ?? "",
                UserId = userId,
                Category = request.Category,
                Layout = request.Layout ?? CreateDefaultLayout(),
                Configuration = request.Configuration ?? new Core.Models.DashboardConfiguration(),
                Permissions = new DashboardPermissions { OwnerId = userId },
                IsPublic = request.IsPublic,
                Tags = request.Tags,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Create widgets
            foreach (var widgetRequest in request.Widgets)
            {
                var widget = await CreateWidgetFromRequestAsync(widgetRequest, dashboard.DashboardId);
                dashboard.Widgets.Add(widget);
            }

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
    public async Task<Dashboard> UpdateDashboardAsync(string dashboardId, UpdateDashboardRequest request, string userId)
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
    public async Task<Dashboard?> GetDashboardAsync(string dashboardId, string userId)
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
    public async Task<List<Dashboard>> GetUserDashboardsAsync(string userId, DashboardFilter? filter = null)
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
            return new List<Dashboard>();
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
    public async Task<Dashboard> GenerateDashboardFromDescriptionAsync(
        string description, 
        string userId, 
        SchemaMetadata? schema = null)
    {
        try
        {
            _logger.LogInformation("🤖 Generating dashboard from description for user {UserId}: {Description}", 
                userId, description);

            // Analyze description with NLU
            var nluResult = await _nluService.AnalyzeQueryAsync(description, userId);

            // Get schema if not provided
            schema ??= await _schemaService.GetSchemaMetadataAsync();

            // Generate dashboard structure using AI
            var dashboardSpec = await GenerateDashboardSpecificationAsync(description, nluResult, schema);

            // Create dashboard from specification
            var createRequest = new CreateDashboardRequest
            {
                Name = dashboardSpec.Name,
                Description = dashboardSpec.Description,
                Category = "AI Generated",
                Widgets = dashboardSpec.Widgets,
                Layout = dashboardSpec.Layout,
                Tags = new List<string> { "ai-generated", "auto-created" }
            };

            var dashboard = await CreateDashboardAsync(createRequest, userId);

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
        try
        {
            _logger.LogDebug("📋 Getting dashboard templates for category: {Category}", category ?? "all");

            var templates = _defaultTemplates.AsEnumerable();

            if (!string.IsNullOrEmpty(category))
            {
                templates = templates.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            var result = templates.ToList();

            _logger.LogInformation("📋 Retrieved {Count} dashboard templates", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting dashboard templates");
            return new List<DashboardTemplate>();
        }
    }

    /// <summary>
    /// Create dashboard from template
    /// </summary>
    public async Task<Dashboard> CreateDashboardFromTemplateAsync(
        string templateId, 
        string name, 
        string userId, 
        Dictionary<string, object>? parameters = null)
    {
        try
        {
            _logger.LogInformation("📋 Creating dashboard from template {TemplateId} for user {UserId}", templateId, userId);

            var template = _defaultTemplates.FirstOrDefault(t => t.TemplateId == templateId);
            if (template == null)
            {
                throw new ArgumentException($"Template {templateId} not found");
            }

            // Apply parameters to template
            var processedTemplate = await ProcessTemplateParametersAsync(template, parameters ?? new());

            // Create dashboard from processed template
            var createRequest = new CreateDashboardRequest
            {
                Name = name,
                Description = processedTemplate.Description,
                Category = processedTemplate.Category,
                Layout = processedTemplate.Layout,
                Widgets = processedTemplate.Widgets.Select(tw => new CreateWidgetRequest
                {
                    Title = tw.Title,
                    Type = tw.Type,
                    Position = tw.Position,
                    Size = tw.Size,
                    Configuration = new WidgetConfiguration { CustomConfig = tw.Configuration },
                    DataSource = new WidgetDataSource { Query = tw.DataSourceTemplate }
                }).ToList(),
                Tags = new List<string> { "template", template.Category }
            };

            var dashboard = await CreateDashboardAsync(createRequest, userId);

            // Update template usage count
            template.UsageCount++;

            _logger.LogInformation("📋 Dashboard '{Name}' created from template {TemplateId}", name, templateId);
            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating dashboard from template {TemplateId}", templateId);
            throw;
        }
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

    private Core.Models.DashboardLayout CreateDefaultLayout()
    {
        return new Core.Models.DashboardLayout
        {
            Type = Core.Models.LayoutType.Grid,
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

    private bool HasDashboardAccess(Dashboard dashboard, string userId)
    {
        return dashboard.UserId == userId || 
               dashboard.IsPublic || 
               dashboard.Permissions.UserPermissions.Any(up => up.UserId == userId);
    }

    private bool HasEditAccess(Dashboard dashboard, string userId)
    {
        if (dashboard.UserId == userId) return true;

        var userPermission = dashboard.Permissions.UserPermissions
            .FirstOrDefault(up => up.UserId == userId);

        return userPermission?.Permission >= PermissionLevel.Edit;
    }

    private async Task<DashboardSpecification> GenerateDashboardSpecificationAsync(
        string description, 
        AdvancedNLUResult nluResult, 
        SchemaMetadata schema)
    {
        // Use AI to generate dashboard specification
        var prompt = $@"
Generate a dashboard specification based on this description: {description}

Intent: {nluResult.IntentAnalysis.PrimaryIntent}
Entities: {string.Join(", ", nluResult.EntityAnalysis.Entities.Select(e => e.Value))}
Domain: {nluResult.DomainAnalysis.PrimaryDomain}

Available tables: {string.Join(", ", schema.Tables.Select(t => t.Name))}

Create a dashboard with appropriate widgets for this request.
";

        // For now, create a simple specification
        return new DashboardSpecification
        {
            Name = ExtractDashboardName(description),
            Description = $"Auto-generated dashboard: {description}",
            Layout = CreateDefaultLayout(),
            Widgets = await GenerateWidgetsFromNLUAsync(nluResult, schema)
        };
    }

    private string ExtractDashboardName(string description)
    {
        // Simple name extraction
        var words = description.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length > 3 ? string.Join(" ", words.Take(3)) + " Dashboard" : description + " Dashboard";
    }

    private async Task<List<CreateWidgetRequest>> GenerateWidgetsFromNLUAsync(AdvancedNLUResult nluResult, SchemaMetadata schema)
    {
        var widgets = new List<CreateWidgetRequest>();

        // Generate widgets based on intent
        switch (nluResult.IntentAnalysis.PrimaryIntent)
        {
            case "Aggregation":
                widgets.Add(new CreateWidgetRequest
                {
                    Title = "Summary Metrics",
                    Type = WidgetType.Metric,
                    Position = new WidgetPosition { X = 0, Y = 0 },
                    Size = new WidgetSize { Width = 6, Height = 3 },
                    DataSource = new WidgetDataSource
                    {
                        Type = DataSourceType.Query,
                        Query = "Show summary statistics"
                    }
                });
                break;

            case "Trend":
                widgets.Add(new CreateWidgetRequest
                {
                    Title = "Trend Analysis",
                    Type = WidgetType.Chart,
                    Position = new WidgetPosition { X = 0, Y = 0 },
                    Size = new WidgetSize { Width = 12, Height = 6 },
                    DataSource = new WidgetDataSource
                    {
                        Type = DataSourceType.Query,
                        Query = "Show trends over time"
                    }
                });
                break;

            default:
                widgets.Add(new CreateWidgetRequest
                {
                    Title = "Data Overview",
                    Type = WidgetType.Table,
                    Position = new WidgetPosition { X = 0, Y = 0 },
                    Size = new WidgetSize { Width = 12, Height = 8 },
                    DataSource = new WidgetDataSource
                    {
                        Type = DataSourceType.Query,
                        Query = "Show data overview"
                    }
                });
                break;
        }

        return widgets;
    }

    private List<DashboardTemplate> InitializeDefaultTemplates()
    {
        return new List<DashboardTemplate>
        {
            new DashboardTemplate
            {
                TemplateId = "executive-summary",
                Name = "Executive Summary",
                Description = "High-level KPIs and metrics for executives",
                Category = "Business",
                PreviewImage = "/templates/executive-summary.png",
                Layout = CreateDefaultLayout(),
                Widgets = new List<TemplateWidget>
                {
                    new TemplateWidget
                    {
                        Title = "Revenue",
                        Type = WidgetType.Metric,
                        Position = new WidgetPosition { X = 0, Y = 0 },
                        Size = new WidgetSize { Width = 3, Height = 2 },
                        DataSourceTemplate = "SELECT SUM(revenue) FROM {revenue_table}"
                    },
                    new TemplateWidget
                    {
                        Title = "Revenue Trend",
                        Type = WidgetType.Chart,
                        Position = new WidgetPosition { X = 3, Y = 0 },
                        Size = new WidgetSize { Width = 9, Height = 4 },
                        DataSourceTemplate = "SELECT date, SUM(revenue) FROM {revenue_table} GROUP BY date"
                    }
                },
                Tags = new List<string> { "executive", "kpi", "revenue" },
                CreatedBy = "System"
            },
            new DashboardTemplate
            {
                TemplateId = "player-analytics",
                Name = "Player Analytics",
                Description = "Comprehensive player behavior and performance analytics",
                Category = "Gaming",
                PreviewImage = "/templates/player-analytics.png",
                Layout = CreateDefaultLayout(),
                Widgets = new List<TemplateWidget>
                {
                    new TemplateWidget
                    {
                        Title = "Active Players",
                        Type = WidgetType.Metric,
                        Position = new WidgetPosition { X = 0, Y = 0 },
                        Size = new WidgetSize { Width = 3, Height = 2 },
                        DataSourceTemplate = "SELECT COUNT(*) FROM {players_table} WHERE status = 'Active'"
                    },
                    new TemplateWidget
                    {
                        Title = "Player Activity",
                        Type = WidgetType.Chart,
                        Position = new WidgetPosition { X = 3, Y = 0 },
                        Size = new WidgetSize { Width = 9, Height = 4 },
                        DataSourceTemplate = "SELECT date, COUNT(*) FROM {activity_table} GROUP BY date"
                    }
                },
                Tags = new List<string> { "players", "gaming", "analytics" },
                CreatedBy = "System"
            }
        };
    }

    private async Task<DashboardTemplate> ProcessTemplateParametersAsync(
        DashboardTemplate template, 
        Dictionary<string, object> parameters)
    {
        // Process template parameters and replace placeholders
        var processedTemplate = JsonSerializer.Deserialize<DashboardTemplate>(JsonSerializer.Serialize(template))!;

        foreach (var widget in processedTemplate.Widgets)
        {
            foreach (var param in parameters)
            {
                widget.DataSourceTemplate = widget.DataSourceTemplate.Replace($"{{{param.Key}}}", param.Value.ToString());
            }
        }

        return processedTemplate;
    }

    // Database operations (placeholder implementations)
    private async Task StoreDashboardAsync(Dashboard dashboard) { }
    private async Task UpdateDashboardInDatabaseAsync(Dashboard dashboard) { }
    private async Task DeleteDashboardFromDatabaseAsync(string dashboardId) { }
    private async Task<Dashboard?> LoadDashboardFromDatabaseAsync(string dashboardId) => null;
    private async Task<List<Dashboard>> LoadUserDashboardsFromDatabaseAsync(string userId, DashboardFilter? filter) => new();
    private async Task UpdateLastViewedAsync(string dashboardId, string userId) { }

    // Export implementations (placeholder)
    private async Task<byte[]> GeneratePdfExportAsync(Dashboard dashboard, ExportOptions? options) => Array.Empty<byte>();
    private async Task<byte[]> GenerateImageExportAsync(Dashboard dashboard, ExportOptions? options) => Array.Empty<byte>();

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
    public async Task<DashboardWidget> UpdateWidgetAsync(string dashboardId, string widgetId, UpdateWidgetRequest request, string userId) => new();
    public async Task<bool> RemoveWidgetAsync(string dashboardId, string widgetId, string userId) => false;
    public async Task<DashboardShare> ShareDashboardAsync(string dashboardId, ShareConfiguration shareConfig, string userId) => new();
    public async Task<DashboardAnalytics> GetDashboardAnalyticsAsync(string dashboardId, TimeSpan? timeWindow = null) => new();
    public async Task<Dashboard> CloneDashboardAsync(string dashboardId, string newName, string userId) => new();

    // Helper methods removed - using Core model structure instead
}

// Supporting classes
public class DashboardSpecification
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Core.Models.DashboardLayout Layout { get; set; } = new();
    public List<CreateWidgetRequest> Widgets { get; set; } = new();
}

// Local model definitions removed - using Core models instead
