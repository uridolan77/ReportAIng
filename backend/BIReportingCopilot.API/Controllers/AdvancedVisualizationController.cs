using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Constants;
using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Advanced visualization controller with sophisticated chart generation and AI-powered recommendations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdvancedVisualizationController : ControllerBase
{
    private readonly IAdvancedVisualizationService _advancedVisualizationService;
    private readonly IQueryService _queryService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AdvancedVisualizationController> _logger;

    public AdvancedVisualizationController(
        IAdvancedVisualizationService advancedVisualizationService,
        IQueryService queryService,
        IAuditService auditService,
        ILogger<AdvancedVisualizationController> logger)
    {
        _advancedVisualizationService = advancedVisualizationService;
        _queryService = queryService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Generate advanced visualization with AI-powered chart type selection and sophisticated configuration
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<AdvancedVisualizationResponse>> GenerateAdvancedVisualization(
        [FromBody] AdvancedVisualizationRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? "anonymous";

        try
        {
            _logger.LogInformation("Generating advanced visualization for user {UserId}", userId);

            // Execute query to get data
            var queryRequest = new QueryRequest { Question = request.Query };
            var queryResult = await _queryService.ProcessQueryAsync(queryRequest, userId);

            if (!queryResult.Success || queryResult.Result?.Data == null)
            {
                return BadRequest(new { error = "Failed to execute query", details = queryResult.Error });
            }

            // Generate advanced visualization
            var visualization = await _advancedVisualizationService.GenerateAdvancedVisualizationAsync(
                request.Query,
                queryResult.Result.Metadata.Columns,
                queryResult.Result.Data,
                request.Preferences);

            // Optimize for performance if requested
            if (request.OptimizeForPerformance)
            {
                visualization = await _advancedVisualizationService.OptimizeVisualizationForPerformanceAsync(
                    visualization,
                    queryResult.Result.Data.Length);
            }

            // Audit the visualization generation
            await _auditService.LogAsync(
                ApplicationConstants.AuditActions.VisualizationGenerated,
                userId,
                ApplicationConstants.EntityTypes.Visualization,
                HttpContext.TraceIdentifier,
                new {
                    Query = request.Query,
                    ChartType = visualization.ChartType.ToString(),
                    DataRows = queryResult.Result.Data.Length
                });

            var response = new AdvancedVisualizationResponse
            {
                Success = true,
                Visualization = visualization,
                DataSummary = new DataSummary
                {
                    RowCount = queryResult.Result.Data.Length,
                    ColumnCount = queryResult.Result.Metadata.Columns.Length,
                    ExecutionTimeMs = queryResult.ExecutionTimeMs,
                    DataTypes = queryResult.Result.Metadata.Columns.ToDictionary(c => c.Name, c => c.DataType)
                },
                PerformanceMetrics = new ChartPerformanceMetrics
                {
                    RenderTime = "50ms", // Estimated
                    MemoryUsage = Math.Max(10, queryResult.Result.Data.Length / 1000),
                    DataPointsRendered = queryResult.Result.Data.Length,
                    UsedSampling = visualization.DataProcessing?.EnableSampling ?? false,
                    UsedVirtualization = visualization.Performance?.EnableVirtualization ?? false,
                    UsedWebGL = visualization.Performance?.EnableWebGL ?? false
                }
            };

            _logger.LogInformation("Successfully generated advanced visualization with type {ChartType}",
                visualization.ChartType);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating advanced visualization for user {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Generate advanced dashboard with multiple sophisticated charts
    /// </summary>
    [HttpPost("dashboard")]
    public async Task<ActionResult<AdvancedDashboardResponse>> GenerateAdvancedDashboard(
        [FromBody] AdvancedDashboardRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? "anonymous";

        try
        {
            _logger.LogInformation("Generating advanced dashboard for user {UserId}", userId);

            // Execute query to get data
            var queryRequest = new QueryRequest { Question = request.Query };
            var queryResult = await _queryService.ProcessQueryAsync(queryRequest, userId);

            if (!queryResult.Success || queryResult.Result?.Data == null)
            {
                return BadRequest(new { error = "Failed to execute query", details = queryResult.Error });
            }

            // Generate advanced dashboard
            var dashboard = await _advancedVisualizationService.GenerateAdvancedDashboardAsync(
                request.Query,
                queryResult.Result.Metadata.Columns,
                queryResult.Result.Data,
                request.Preferences);

            // Audit the dashboard generation
            await _auditService.LogAsync(
                ApplicationConstants.AuditActions.DashboardGenerated,
                userId,
                ApplicationConstants.EntityTypes.Dashboard,
                HttpContext.TraceIdentifier,
                new {
                    Query = request.Query,
                    ChartCount = dashboard.Charts.Length,
                    DataRows = queryResult.Result.Data.Length
                });

            var response = new AdvancedDashboardResponse
            {
                Success = true,
                Dashboard = dashboard,
                DataSummary = new DataSummary
                {
                    RowCount = queryResult.Result.Data.Length,
                    ColumnCount = queryResult.Result.Metadata.Columns.Length,
                    ExecutionTimeMs = queryResult.ExecutionTimeMs,
                    DataTypes = queryResult.Result.Metadata.Columns.ToDictionary(c => c.Name, c => c.DataType)
                },
                GenerationMetrics = new DashboardGenerationMetrics
                {
                    TotalCharts = dashboard.Charts.Length,
                    GenerationTime = "2s", // Estimated
                    ComplexityScore = CalculateComplexityScore(dashboard),
                    RecommendationConfidence = 0.85 // Estimated
                }
            };

            _logger.LogInformation("Successfully generated advanced dashboard with {ChartCount} charts",
                dashboard.Charts.Length);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating advanced dashboard for user {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Get AI-powered visualization recommendations based on data characteristics
    /// </summary>
    [HttpPost("recommendations")]
    public async Task<ActionResult<VisualizationRecommendationsResponse>> GetVisualizationRecommendations(
        [FromBody] VisualizationRecommendationsRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? "anonymous";

        try
        {
            _logger.LogInformation("Getting visualization recommendations for user {UserId}", userId);

            // Execute query to get data
            var queryRequest = new QueryRequest { Question = request.Query };
            var queryResult = await _queryService.ProcessQueryAsync(queryRequest, userId);

            if (!queryResult.Success || queryResult.Result?.Data == null)
            {
                return BadRequest(new { error = "Failed to execute query", details = queryResult.Error });
            }

            // Get AI-powered recommendations
            var recommendations = await _advancedVisualizationService.GetVisualizationRecommendationsAsync(
                queryResult.Result.Metadata.Columns,
                queryResult.Result.Data,
                request.Context);

            // Audit the recommendation request
            await _auditService.LogAsync(
                "RECOMMENDATIONS_GENERATED",
                userId,
                ApplicationConstants.EntityTypes.Visualization,
                HttpContext.TraceIdentifier,
                new {
                    Query = request.Query,
                    RecommendationCount = recommendations.Length,
                    Context = request.Context
                });

            var response = new VisualizationRecommendationsResponse
            {
                Success = true,
                Recommendations = recommendations,
                DataSummary = new DataSummary
                {
                    RowCount = queryResult.Result.Data.Length,
                    ColumnCount = queryResult.Result.Metadata.Columns.Length,
                    ExecutionTimeMs = queryResult.ExecutionTimeMs,
                    DataTypes = queryResult.Result.Metadata.Columns.ToDictionary(c => c.Name, c => c.DataType)
                },
                AnalysisMetrics = new RecommendationAnalysisMetrics
                {
                    TotalRecommendations = recommendations.Length,
                    HighConfidenceCount = recommendations.Count(r => r.Confidence > 0.8),
                    AnalysisTime = "500ms", // Estimated
                    DataComplexity = CalculateDataComplexity(queryResult.Result.Metadata.Columns, queryResult.Result.Data)
                }
            };

            _logger.LogInformation("Generated {Count} visualization recommendations", recommendations.Length);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting visualization recommendations for user {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    /// <summary>
    /// Optimize existing visualization configuration for performance
    /// </summary>
    [HttpPost("optimize")]
    public async Task<ActionResult<OptimizationResponse>> OptimizeVisualization(
        [FromBody] OptimizationRequest request)
    {
        var userId = User.FindFirst("sub")?.Value ?? "anonymous";

        try
        {
            _logger.LogInformation("Optimizing visualization for user {UserId}", userId);

            var optimizedConfig = await _advancedVisualizationService.OptimizeVisualizationForPerformanceAsync(
                request.Configuration,
                request.DataSize);

            var response = new OptimizationResponse
            {
                Success = true,
                OriginalConfig = request.Configuration,
                OptimizedConfig = optimizedConfig,
                OptimizationSummary = GenerateOptimizationSummary(request.Configuration, optimizedConfig),
                PerformanceGain = EstimatePerformanceGain(request.Configuration, optimizedConfig)
            };

            _logger.LogInformation("Successfully optimized visualization configuration");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing visualization for user {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    private double CalculateComplexityScore(AdvancedDashboardConfig dashboard)
    {
        var score = dashboard.Charts.Length * 0.2; // Base complexity from chart count

        // Add complexity for advanced features
        if (dashboard.RealTime?.Enabled == true) score += 0.3;
        if (dashboard.Collaboration?.Enabled == true) score += 0.2;
        if (dashboard.Analytics?.Enabled == true) score += 0.1;

        return Math.Min(1.0, score);
    }

    private string CalculateDataComplexity(ColumnInfo[] columns, object[] data)
    {
        var numericCount = columns.Count(c => IsNumericType(c.DataType));
        var categoricalCount = columns.Count(c => IsCategoricalType(c.DataType));
        var dateTimeCount = columns.Count(c => IsDateTimeType(c.DataType));

        if (data.Length > 100000 || columns.Length > 20) return "High";
        if (data.Length > 10000 || columns.Length > 10) return "Medium";
        return "Low";
    }

    private bool IsNumericType(string dataType) =>
        dataType.Contains("int") || dataType.Contains("decimal") || dataType.Contains("float") || dataType.Contains("double");

    private bool IsCategoricalType(string dataType) =>
        dataType.Contains("string") || dataType.Contains("varchar") || dataType.Contains("char");

    private bool IsDateTimeType(string dataType) =>
        dataType.Contains("date") || dataType.Contains("time");

    private OptimizationSummary GenerateOptimizationSummary(
        AdvancedVisualizationConfig original,
        AdvancedVisualizationConfig optimized)
    {
        var changes = new List<string>();

        if (original.DataProcessing?.EnableSampling != optimized.DataProcessing?.EnableSampling)
            changes.Add("Data sampling configuration");

        if (original.Performance?.EnableVirtualization != optimized.Performance?.EnableVirtualization)
            changes.Add("Virtualization settings");

        if (original.Animation?.Enabled != optimized.Animation?.Enabled)
            changes.Add("Animation configuration");

        return new OptimizationSummary
        {
            ChangesApplied = changes.ToArray(),
            OptimizationLevel = changes.Count switch
            {
                0 => "None",
                1 => "Light",
                2 => "Moderate",
                _ => "Aggressive"
            },
            EstimatedImpact = "Improved rendering performance and reduced memory usage"
        };
    }

    private PerformanceGain EstimatePerformanceGain(
        AdvancedVisualizationConfig original,
        AdvancedVisualizationConfig optimized)
    {
        // Simple estimation logic
        var renderTimeImprovement = 0.0;
        var memoryImprovement = 0.0;

        if (optimized.DataProcessing?.EnableSampling == true && original.DataProcessing?.EnableSampling != true)
        {
            renderTimeImprovement += 0.4;
            memoryImprovement += 0.3;
        }

        if (optimized.Performance?.EnableVirtualization == true && original.Performance?.EnableVirtualization != true)
        {
            renderTimeImprovement += 0.3;
            memoryImprovement += 0.5;
        }

        return new PerformanceGain
        {
            RenderTimeImprovement = renderTimeImprovement,
            MemoryUsageImprovement = memoryImprovement,
            OverallScore = (renderTimeImprovement + memoryImprovement) / 2
        };
    }
}

/// <summary>
/// Request for advanced visualization generation
/// </summary>
public class AdvancedVisualizationRequest
{
    [Required]
    public string Query { get; set; } = string.Empty;

    public VisualizationPreferences? Preferences { get; set; }

    public bool OptimizeForPerformance { get; set; } = true;
}

/// <summary>
/// Response for advanced visualization generation
/// </summary>
public class AdvancedVisualizationResponse
{
    public bool Success { get; set; }
    public AdvancedVisualizationConfig? Visualization { get; set; }
    public DataSummary? DataSummary { get; set; }
    public ChartPerformanceMetrics? PerformanceMetrics { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Request for advanced dashboard generation
/// </summary>
public class AdvancedDashboardRequest
{
    [Required]
    public string Query { get; set; } = string.Empty;

    public DashboardPreferences? Preferences { get; set; }
}

/// <summary>
/// Response for advanced dashboard generation
/// </summary>
public class AdvancedDashboardResponse
{
    public bool Success { get; set; }
    public AdvancedDashboardConfig? Dashboard { get; set; }
    public DataSummary? DataSummary { get; set; }
    public DashboardGenerationMetrics? GenerationMetrics { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Request for visualization recommendations
/// </summary>
public class VisualizationRecommendationsRequest
{
    [Required]
    public string Query { get; set; } = string.Empty;

    public string? Context { get; set; }
}

/// <summary>
/// Response for visualization recommendations
/// </summary>
public class VisualizationRecommendationsResponse
{
    public bool Success { get; set; }
    public VisualizationRecommendation[]? Recommendations { get; set; }
    public DataSummary? DataSummary { get; set; }
    public RecommendationAnalysisMetrics? AnalysisMetrics { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Request for visualization optimization
/// </summary>
public class OptimizationRequest
{
    [Required]
    public AdvancedVisualizationConfig Configuration { get; set; } = new();

    [Required]
    public int DataSize { get; set; }
}

/// <summary>
/// Response for visualization optimization
/// </summary>
public class OptimizationResponse
{
    public bool Success { get; set; }
    public AdvancedVisualizationConfig? OriginalConfig { get; set; }
    public AdvancedVisualizationConfig? OptimizedConfig { get; set; }
    public OptimizationSummary? OptimizationSummary { get; set; }
    public PerformanceGain? PerformanceGain { get; set; }
    public string? ErrorMessage { get; set; }
}



/// <summary>
/// Data summary for visualization responses
/// </summary>
public class DataSummary
{
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public int ExecutionTimeMs { get; set; }
    public Dictionary<string, string> DataTypes { get; set; } = new();
    public bool HasNumericData { get; set; }
    public bool HasCategoricalData { get; set; }
    public bool HasTimeData { get; set; }
}

/// <summary>
/// Dashboard generation metrics
/// </summary>
public class DashboardGenerationMetrics
{
    public int TotalCharts { get; set; }
    public string GenerationTime { get; set; } = string.Empty;
    public double ComplexityScore { get; set; }
    public double RecommendationConfidence { get; set; }
}

/// <summary>
/// Recommendation analysis metrics
/// </summary>
public class RecommendationAnalysisMetrics
{
    public int TotalRecommendations { get; set; }
    public int HighConfidenceCount { get; set; }
    public string AnalysisTime { get; set; } = string.Empty;
    public string DataComplexity { get; set; } = string.Empty;
}

/// <summary>
/// Optimization summary
/// </summary>
public class OptimizationSummary
{
    public string[] ChangesApplied { get; set; } = Array.Empty<string>();
    public string OptimizationLevel { get; set; } = string.Empty;
    public string EstimatedImpact { get; set; } = string.Empty;
}

/// <summary>
/// Performance gain from optimization
/// </summary>
public class PerformanceGain
{
    public double RenderTimeImprovement { get; set; }
    public double MemoryUsageImprovement { get; set; }
    public double OverallScore { get; set; }
}
