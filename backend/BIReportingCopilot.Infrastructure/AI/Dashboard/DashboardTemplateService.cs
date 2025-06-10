using BIReportingCopilot.Core.Models;
using Microsoft.Extensions.Logging;
using DashboardModel = BIReportingCopilot.Core.Models.Dashboard;

namespace BIReportingCopilot.Infrastructure.AI.Dashboard;

/// <summary>
/// Focused service for dashboard template operations
/// Extracted from ProductionMultiModalDashboardService for better modularity
/// </summary>
public class DashboardTemplateService
{
    private readonly ILogger<DashboardTemplateService> _logger;
    private readonly DashboardCreationService _creationService;

    public DashboardTemplateService(
        ILogger<DashboardTemplateService> logger,
        DashboardCreationService creationService)
    {
        _logger = logger;
        _creationService = creationService;
    }

    /// <summary>
    /// Get available dashboard templates
    /// </summary>
    public Task<List<DashboardTemplate>> GetTemplatesAsync(string? category = null)
    {
        try
        {
            _logger.LogInformation("📋 Getting dashboard templates for category: {Category}", category ?? "all");

            var templates = GetBuiltInTemplates();

            if (!string.IsNullOrEmpty(category))
            {
                templates = templates.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Task.FromResult(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting dashboard templates");
            throw;
        }
    }

    /// <summary>
    /// Create dashboard from template
    /// </summary>
    public async Task<DashboardModel> CreateFromTemplateAsync(
        string templateId, 
        string name, 
        string userId, 
        Dictionary<string, object>? parameters = null)
    {
        try
        {
            _logger.LogInformation("🎨 Creating dashboard from template {TemplateId} for user {UserId}", templateId, userId);

            // Get template
            var template = await GetTemplateByIdAsync(templateId);
            if (template == null)
            {
                throw new ArgumentException($"Template not found: {templateId}");
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

            return await _creationService.CreateDashboardAsync(createRequest, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating dashboard from template {TemplateId}", templateId);
            throw;
        }
    }

    /// <summary>
    /// Get template by ID
    /// </summary>
    public async Task<DashboardTemplate?> GetTemplateByIdAsync(string templateId)
    {
        var templates = await GetTemplatesAsync();
        return templates.FirstOrDefault(t => t.TemplateId == templateId);
    }

    /// <summary>
    /// Process template parameters
    /// </summary>
    private Task<DashboardTemplate> ProcessTemplateParametersAsync(
        DashboardTemplate template,
        Dictionary<string, object> parameters)
    {
        // Clone the template
        var processedTemplate = new DashboardTemplate
        {
            TemplateId = template.TemplateId,
            Name = template.Name,
            Description = ProcessStringTemplate(template.Description, parameters),
            Category = template.Category,
            Layout = template.Layout,
            Widgets = template.Widgets.Select(w => new TemplateWidget
            {
                Title = ProcessStringTemplate(w.Title, parameters),
                Type = w.Type,
                Position = w.Position,
                Size = w.Size,
                Configuration = w.Configuration,
                DataSourceTemplate = ProcessStringTemplate(w.DataSourceTemplate, parameters)
            }).ToList(),
            Parameters = template.Parameters,
            PreviewImage = template.PreviewImage,
            Tags = template.Tags
        };

        return Task.FromResult(processedTemplate);
    }

    /// <summary>
    /// Process string template with parameters
    /// </summary>
    private string ProcessStringTemplate(string template, Dictionary<string, object> parameters)
    {
        var result = template;
        
        foreach (var param in parameters)
        {
            var placeholder = $"{{{param.Key}}}";
            result = result.Replace(placeholder, param.Value?.ToString() ?? "");
        }

        return result;
    }

    /// <summary>
    /// Get built-in dashboard templates
    /// </summary>
    private List<DashboardTemplate> GetBuiltInTemplates()
    {
        return new List<DashboardTemplate>
        {
            new DashboardTemplate
            {
                TemplateId = "sales-overview",
                Name = "Sales Overview",
                Description = "Comprehensive sales performance dashboard",
                Category = "Sales",
                Layout = new DashboardLayout { Type = LayoutType.Grid, Columns = 12 },
                Widgets = new List<TemplateWidget>
                {
                    new TemplateWidget
                    {
                        Title = "Total Sales",
                        Type = WidgetType.Metric,
                        Position = new WidgetPosition { X = 0, Y = 0 },
                        Size = new WidgetSize { Width = 3, Height = 2 },
                        Configuration = new Dictionary<string, object> { ["format"] = "currency" },
                        DataSourceTemplate = "SELECT SUM(amount) as total_sales FROM {sales_table}"
                    },
                    new TemplateWidget
                    {
                        Title = "Sales Trend",
                        Type = WidgetType.Chart,
                        Position = new WidgetPosition { X = 3, Y = 0 },
                        Size = new WidgetSize { Width = 9, Height = 4 },
                        Configuration = new Dictionary<string, object> { ["chartType"] = "line" },
                        DataSourceTemplate = "SELECT date, SUM(amount) as sales FROM {sales_table} GROUP BY date ORDER BY date"
                    }
                },
                Parameters = new List<TemplateParameter>
                {
                    new TemplateParameter { Name = "sales_table", Type = "string", DefaultValue = "sales", IsRequired = true }
                },
                Tags = new List<string> { "sales", "overview", "business" }
            },
            new DashboardTemplate
            {
                TemplateId = "user-analytics",
                Name = "User Analytics",
                Description = "User behavior and engagement analytics",
                Category = "Analytics",
                Layout = new DashboardLayout { Type = LayoutType.Grid, Columns = 12 },
                Widgets = new List<TemplateWidget>
                {
                    new TemplateWidget
                    {
                        Title = "Active Users",
                        Type = WidgetType.Metric,
                        Position = new WidgetPosition { X = 0, Y = 0 },
                        Size = new WidgetSize { Width = 4, Height = 2 },
                        Configuration = new Dictionary<string, object> { ["format"] = "number" },
                        DataSourceTemplate = "SELECT COUNT(DISTINCT user_id) as active_users FROM {user_table} WHERE last_active >= DATEADD(day, -30, GETDATE())"
                    },
                    new TemplateWidget
                    {
                        Title = "User Growth",
                        Type = WidgetType.Chart,
                        Position = new WidgetPosition { X = 4, Y = 0 },
                        Size = new WidgetSize { Width = 8, Height = 4 },
                        Configuration = new Dictionary<string, object> { ["chartType"] = "area" },
                        DataSourceTemplate = "SELECT CAST(created_date as DATE) as date, COUNT(*) as new_users FROM {user_table} GROUP BY CAST(created_date as DATE) ORDER BY date"
                    }
                },
                Parameters = new List<TemplateParameter>
                {
                    new TemplateParameter { Name = "user_table", Type = "string", DefaultValue = "users", IsRequired = true }
                },
                Tags = new List<string> { "users", "analytics", "growth" }
            }
        };
    }
}

// Models are now defined in Core.Models.MultiModalDashboards
