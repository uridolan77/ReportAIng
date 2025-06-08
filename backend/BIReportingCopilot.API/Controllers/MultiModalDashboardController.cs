using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Queries;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Multi-Modal Dashboard controller
/// Provides advanced dashboard creation, management, and reporting
/// </summary>
[ApiController]
[Route("api/dashboards")]
[Authorize]
public class MultiModalDashboardController : ControllerBase
{
    private readonly ILogger<MultiModalDashboardController> _logger;
    private readonly IMediator _mediator;

    public MultiModalDashboardController(
        ILogger<MultiModalDashboardController> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Create a new multi-modal dashboard
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateDashboard([FromBody] CreateDashboardRequest request)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🎨 Creating dashboard '{Name}' for user {UserId}", request.Name, userId);

            var command = new CreateDashboardCommand
            {
                Request = request,
                UserId = userId
            };

            var dashboard = await _mediator.Send(command);

            _logger.LogInformation("🎨 Dashboard '{Name}' created with ID {DashboardId} for user {UserId}", 
                dashboard.Name, dashboard.DashboardId, userId);

            return Ok(new
            {
                success = true,
                data = dashboard,
                metadata = new
                {
                    dashboard_id = dashboard.DashboardId,
                    widget_count = dashboard.Widgets.Count,
                    created_at = dashboard.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating dashboard");
            return StatusCode(500, new { success = false, error = "Internal server error creating dashboard" });
        }
    }

    /// <summary>
    /// Generate dashboard from natural language description
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> GenerateDashboard([FromBody] GenerateDashboardRequest request)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🤖 Generating dashboard from description for user {UserId}: {Description}", 
                userId, request.Description);

            var command = new GenerateDashboardFromDescriptionCommand
            {
                Description = request.Description,
                UserId = userId,
                Schema = request.Schema
            };

            var dashboard = await _mediator.Send(command);

            _logger.LogInformation("🤖 AI-generated dashboard '{Name}' created for user {UserId} with {WidgetCount} widgets", 
                dashboard.Name, userId, dashboard.Widgets.Count);

            return Ok(new
            {
                success = true,
                data = dashboard,
                metadata = new
                {
                    dashboard_id = dashboard.DashboardId,
                    widget_count = dashboard.Widgets.Count,
                    ai_generated = true,
                    description = request.Description
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating dashboard from description");
            return StatusCode(500, new { success = false, error = "Internal server error generating dashboard" });
        }
    }

    /// <summary>
    /// Get dashboard by ID
    /// </summary>
    [HttpGet("{dashboardId}")]
    public async Task<IActionResult> GetDashboard(string dashboardId)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📋 Getting dashboard {DashboardId} for user {UserId}", dashboardId, userId);

            var query = new GetDashboardQuery
            {
                DashboardId = dashboardId,
                UserId = userId
            };

            var dashboard = await _mediator.Send(query);

            if (dashboard == null)
            {
                return NotFound(new { success = false, error = "Dashboard not found or access denied" });
            }

            return Ok(new
            {
                success = true,
                data = dashboard,
                metadata = new
                {
                    dashboard_id = dashboard.DashboardId,
                    widget_count = dashboard.Widgets.Count,
                    last_viewed = dashboard.LastViewedAt,
                    is_public = dashboard.IsPublic
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting dashboard {DashboardId}", dashboardId);
            return StatusCode(500, new { success = false, error = "Internal server error getting dashboard" });
        }
    }

    /// <summary>
    /// Get user's dashboards
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserDashboards([FromQuery] DashboardFilter? filter)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📋 Getting dashboards for user {UserId}", userId);

            var query = new GetUserDashboardsQuery
            {
                UserId = userId,
                Filter = filter
            };

            var dashboards = await _mediator.Send(query);

            _logger.LogInformation("📋 Retrieved {Count} dashboards for user {UserId}", dashboards.Count, userId);

            return Ok(new
            {
                success = true,
                data = dashboards,
                metadata = new
                {
                    dashboard_count = dashboards.Count,
                    public_dashboards = dashboards.Count(d => d.IsPublic),
                    template_dashboards = dashboards.Count(d => d.IsTemplate)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting dashboards for user");
            return StatusCode(500, new { success = false, error = "Internal server error getting dashboards" });
        }
    }

    /// <summary>
    /// Update dashboard
    /// </summary>
    [HttpPut("{dashboardId}")]
    public async Task<IActionResult> UpdateDashboard(string dashboardId, [FromBody] UpdateDashboardRequest request)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🔄 Updating dashboard {DashboardId} for user {UserId}", dashboardId, userId);

            var command = new UpdateDashboardCommand
            {
                DashboardId = dashboardId,
                Request = request,
                UserId = userId
            };

            var dashboard = await _mediator.Send(command);

            _logger.LogInformation("🔄 Dashboard {DashboardId} updated successfully", dashboardId);

            return Ok(new
            {
                success = true,
                data = dashboard,
                metadata = new
                {
                    dashboard_id = dashboard.DashboardId,
                    updated_at = dashboard.UpdatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating dashboard {DashboardId}", dashboardId);
            return StatusCode(500, new { success = false, error = "Internal server error updating dashboard" });
        }
    }

    /// <summary>
    /// Delete dashboard
    /// </summary>
    [HttpDelete("{dashboardId}")]
    public async Task<IActionResult> DeleteDashboard(string dashboardId)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🗑️ Deleting dashboard {DashboardId} for user {UserId}", dashboardId, userId);

            var command = new DeleteDashboardCommand
            {
                DashboardId = dashboardId,
                UserId = userId
            };

            var result = await _mediator.Send(command);

            if (result)
            {
                _logger.LogInformation("🗑️ Dashboard {DashboardId} deleted successfully", dashboardId);
                return Ok(new { success = true, message = "Dashboard deleted successfully" });
            }
            else
            {
                return NotFound(new { success = false, error = "Dashboard not found or access denied" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error deleting dashboard {DashboardId}", dashboardId);
            return StatusCode(500, new { success = false, error = "Internal server error deleting dashboard" });
        }
    }

    /// <summary>
    /// Add widget to dashboard
    /// </summary>
    [HttpPost("{dashboardId}/widgets")]
    public async Task<IActionResult> AddWidget(string dashboardId, [FromBody] CreateWidgetRequest request)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🧩 Adding widget '{Title}' to dashboard {DashboardId}", request.Title, dashboardId);

            var command = new AddWidgetToDashboardCommand
            {
                DashboardId = dashboardId,
                Request = request,
                UserId = userId
            };

            var widget = await _mediator.Send(command);

            _logger.LogInformation("🧩 Widget '{Title}' added to dashboard {DashboardId}", widget.Title, dashboardId);

            return Ok(new
            {
                success = true,
                data = widget,
                metadata = new
                {
                    widget_id = widget.WidgetId,
                    dashboard_id = dashboardId,
                    widget_type = widget.Type
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error adding widget to dashboard {DashboardId}", dashboardId);
            return StatusCode(500, new { success = false, error = "Internal server error adding widget" });
        }
    }

    /// <summary>
    /// Export dashboard to various formats
    /// </summary>
    [HttpPost("{dashboardId}/export")]
    public async Task<IActionResult> ExportDashboard(string dashboardId, [FromBody] ExportDashboardRequest request)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📤 Exporting dashboard {DashboardId} to {Format}", dashboardId, request.Format);

            var command = new ExportDashboardCommand
            {
                DashboardId = dashboardId,
                Format = request.Format,
                Options = request.Options
            };

            var export = await _mediator.Send(command);

            _logger.LogInformation("📤 Dashboard {DashboardId} exported to {Format} ({Size} bytes)", 
                dashboardId, request.Format, export.Data.Length);

            return File(export.Data, export.ContentType, export.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error exporting dashboard {DashboardId}", dashboardId);
            return StatusCode(500, new { success = false, error = "Internal server error exporting dashboard" });
        }
    }

    /// <summary>
    /// Clone dashboard
    /// </summary>
    [HttpPost("{dashboardId}/clone")]
    public async Task<IActionResult> CloneDashboard(string dashboardId, [FromBody] CloneDashboardRequest request)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📋 Cloning dashboard {DashboardId} for user {UserId}", dashboardId, userId);

            var command = new CloneDashboardCommand
            {
                DashboardId = dashboardId,
                NewName = request.NewName,
                UserId = userId
            };

            var clonedDashboard = await _mediator.Send(command);

            _logger.LogInformation("📋 Dashboard {DashboardId} cloned as '{NewName}'", dashboardId, request.NewName);

            return Ok(new
            {
                success = true,
                data = clonedDashboard,
                metadata = new
                {
                    original_dashboard_id = dashboardId,
                    cloned_dashboard_id = clonedDashboard.DashboardId,
                    new_name = request.NewName
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error cloning dashboard {DashboardId}", dashboardId);
            return StatusCode(500, new { success = false, error = "Internal server error cloning dashboard" });
        }
    }

    /// <summary>
    /// Get dashboard templates
    /// </summary>
    [HttpGet("templates")]
    public async Task<IActionResult> GetDashboardTemplates([FromQuery] string? category = null)
    {
        try
        {
            _logger.LogInformation("📋 Getting dashboard templates for category: {Category}", category ?? "all");

            var query = new GetDashboardTemplatesQuery
            {
                Category = category
            };

            var templates = await _mediator.Send(query);

            _logger.LogInformation("📋 Retrieved {Count} dashboard templates", templates.Count);

            return Ok(new
            {
                success = true,
                data = templates,
                metadata = new
                {
                    template_count = templates.Count,
                    category = category ?? "all",
                    categories = templates.Select(t => t.Category).Distinct().ToList()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting dashboard templates");
            return StatusCode(500, new { success = false, error = "Internal server error getting templates" });
        }
    }

    /// <summary>
    /// Create dashboard from template
    /// </summary>
    [HttpPost("templates/{templateId}/create")]
    public async Task<IActionResult> CreateFromTemplate(string templateId, [FromBody] CreateFromTemplateRequest request)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📋 Creating dashboard from template {TemplateId} for user {UserId}", templateId, userId);

            var command = new CreateDashboardFromTemplateCommand
            {
                TemplateId = templateId,
                Name = request.Name,
                UserId = userId,
                Parameters = request.Parameters
            };

            var dashboard = await _mediator.Send(command);

            _logger.LogInformation("📋 Dashboard '{Name}' created from template {TemplateId} for user {UserId}", 
                request.Name, templateId, userId);

            return Ok(new
            {
                success = true,
                data = dashboard,
                metadata = new
                {
                    dashboard_id = dashboard.DashboardId,
                    template_id = templateId,
                    name = request.Name,
                    widget_count = dashboard.Widgets.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating dashboard from template {TemplateId}", templateId);
            return StatusCode(500, new { success = false, error = "Internal server error creating dashboard from template" });
        }
    }
}

// Request models
public class GenerateDashboardRequest
{
    public string Description { get; set; } = string.Empty;
    public SchemaMetadata? Schema { get; set; }
}

public class ExportDashboardRequest
{
    public ExportFormat Format { get; set; } = ExportFormat.PDF;
    public ExportOptions? Options { get; set; }
}

public class CloneDashboardRequest
{
    public string NewName { get; set; } = string.Empty;
}

public class CreateFromTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public Dictionary<string, object>? Parameters { get; set; }
}
