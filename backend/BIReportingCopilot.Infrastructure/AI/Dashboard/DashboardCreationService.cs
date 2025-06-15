using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Core.Models;
using Microsoft.Extensions.Logging;
using DashboardModel = BIReportingCopilot.Core.Models.Dashboard;

namespace BIReportingCopilot.Infrastructure.AI.Dashboard;

/// <summary>
/// Focused service for dashboard creation operations
/// Extracted from ProductionMultiModalDashboardService for better modularity
/// </summary>
public class DashboardCreationService
{
    private readonly ILogger<DashboardCreationService> _logger;
    private readonly IAdvancedNLUService _nluService;
    private readonly ISchemaService _schemaService;

    public DashboardCreationService(
        ILogger<DashboardCreationService> logger,
        IAdvancedNLUService nluService,
        ISchemaService schemaService)
    {
        _logger = logger;
        _nluService = nluService;
        _schemaService = schemaService;
    }

    /// <summary>
    /// Create dashboard from request
    /// </summary>
    public Task<DashboardModel> CreateDashboardAsync(CreateDashboardRequest request, string userId)
    {
        try
        {
            _logger.LogInformation("🎨 Creating dashboard '{Name}' for user {UserId}", request.Name, userId);

            var dashboard = new DashboardModel
            {
                Name = request.Name,
                Description = request.Description ?? "",
                UserId = userId,
                Category = request.Category,
                Layout = request.Layout ?? new BIReportingCopilot.Core.Models.DashboardLayout { Type = BIReportingCopilot.Core.Models.LayoutType.Grid, Columns = 12 },
                Configuration = request.Configuration ?? new BIReportingCopilot.Core.Models.DashboardConfiguration(),
                Permissions = new DashboardPermissions { OwnerId = userId },
                IsPublic = request.IsPublic,
                Tags = request.Tags,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Add widgets if provided
            if (request.Widgets?.Any() == true)
            {
                dashboard.Widgets = request.Widgets.Select(w => new DashboardWidget
                {
                    Title = w.Title,
                    Type = w.Type,
                    Position = w.Position,
                    Size = w.Size,
                    Configuration = w.Configuration ?? new WidgetConfiguration(),
                    DataSource = w.DataSource ?? new WidgetDataSource(),
                    CreatedAt = DateTime.UtcNow
                }).ToList();
            }

            _logger.LogInformation("✅ Dashboard '{Name}' created successfully", dashboard.Name);
            return Task.FromResult(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating dashboard '{Name}'", request.Name);
            throw;
        }
    }

    /// <summary>
    /// Generate dashboard from natural language description
    /// </summary>
    public async Task<DashboardModel> GenerateFromDescriptionAsync(string description, string userId, SchemaMetadata? schema = null)
    {
        try
        {
            _logger.LogInformation("🤖 Generating dashboard from description for user {UserId}", userId);

            // TODO: Implement AnalyzeQueryAsync in IAdvancedNLUService
            // var nluResult = await _nluService.AnalyzeQueryAsync(description, userId);
            var nluResult = new AdvancedNLUResult
            {
                EntityAnalysis = new EntityAnalysis { Entities = new List<ExtractedEntity>() },
                IntentAnalysis = new IntentAnalysis { PrimaryIntent = "Lookup" },
                AdvancedContext = new AdvancedContextAnalysis { ContextData = new Dictionary<string, object> { ["description"] = description } }
            };

            // Get schema if not provided
            schema ??= await _schemaService.GetSchemaMetadataAsync();

            // Generate basic dashboard from NLU analysis
            var createRequest = new CreateDashboardRequest
            {
                Name = ExtractDashboardName(description),
                Description = $"Auto-generated dashboard: {description}",
                Category = "AI Generated",
                Widgets = await GenerateWidgetsFromNLUAsync(nluResult, schema),
                Layout = new BIReportingCopilot.Core.Models.DashboardLayout { Type = BIReportingCopilot.Core.Models.LayoutType.Grid, Columns = 12 },
                Tags = new List<string> { "ai-generated", "auto-created" }
            };

            return await CreateDashboardAsync(createRequest, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating dashboard from description");
            throw;
        }
    }

    /// <summary>
    /// Create default dashboard layout
    /// </summary>
    public BIReportingCopilot.Core.Models.DashboardLayout CreateDefaultLayout()
    {
        return new BIReportingCopilot.Core.Models.DashboardLayout
        {
            Type = BIReportingCopilot.Core.Models.LayoutType.Grid,
            Columns = 12
        };
    }



    /// <summary>
    /// Extract dashboard name from description
    /// </summary>
    private string ExtractDashboardName(string description)
    {
        var words = description.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length > 3 ? string.Join(" ", words.Take(3)) + " Dashboard" : description + " Dashboard";
    }

    /// <summary>
    /// Generate widgets from NLU analysis
    /// </summary>
    private Task<List<CreateWidgetRequest>> GenerateWidgetsFromNLUAsync(AdvancedNLUResult nluResult, SchemaMetadata schema)
    {
        var widgets = new List<CreateWidgetRequest>();

        // Create basic widgets based on NLU analysis
        if (nluResult.EntityAnalysis?.Entities?.Any() == true)
        {
            var chartWidget = new CreateWidgetRequest
            {
                Title = "Data Overview",
                Type = WidgetType.Chart,
                Position = new WidgetPosition { X = 0, Y = 0 },
                Size = new WidgetSize { Width = 6, Height = 4 },
                Configuration = new WidgetConfiguration
                {
                    CustomConfig = new Dictionary<string, object>
                    {
                        ["chartType"] = "bar",
                        ["showLegend"] = true,
                        ["showTooltip"] = true
                    }
                },
                DataSource = new WidgetDataSource
                {
                    Query = "SELECT TOP 10 * FROM " + (schema.Tables?.FirstOrDefault()?.Name ?? "DefaultTable")
                }
            };

            widgets.Add(chartWidget);
        }

        // Add a summary widget
        var summaryWidget = new CreateWidgetRequest
        {
            Title = "Summary",
            Type = WidgetType.Metric,
            Position = new WidgetPosition { X = 6, Y = 0 },
            Size = new WidgetSize { Width = 6, Height = 2 },
            Configuration = new WidgetConfiguration
            {
                CustomConfig = new Dictionary<string, object>
                {
                    ["format"] = "number",
                    ["precision"] = 2
                }
            },
            DataSource = new WidgetDataSource
            {
                Query = "SELECT COUNT(*) as TotalRecords FROM " + (schema.Tables?.FirstOrDefault()?.Name ?? "DefaultTable")
            }
        };

        widgets.Add(summaryWidget);

        return Task.FromResult(widgets);
    }
}

// Dashboard specification models are now in Core.Models
