using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Visualization controller providing standard and advanced visualization capabilities
/// </summary>
[ApiController]
[Route("api/visualization")]
[Authorize]
public class VisualizationController : ControllerBase
{
    private readonly ILogger<VisualizationController> _logger;
    private readonly BIReportingCopilot.Infrastructure.Services.IVisualizationService _visualizationService;
    private readonly IQueryService _queryService;

    public VisualizationController(
        ILogger<VisualizationController> logger,
        BIReportingCopilot.Infrastructure.Services.IVisualizationService visualizationService,
        IQueryService queryService)
    {
        _logger = logger;
        _visualizationService = visualizationService;
        _queryService = queryService;
    }

    #region Standard Visualization Operations

    /// <summary>
    /// Generate visualization configuration from query
    /// </summary>
    /// <param name="request">Visualization request</param>
    /// <returns>Visualization configuration</returns>
    [HttpPost("generate")]
    public async Task<ActionResult<VisualizationConfig>> GenerateVisualization([FromBody] VisualizationRequest request)
    {
        try
        {
            _logger.LogInformation("Generating visualization for query: {Query}", request.Query);

            var config = await _visualizationService.GenerateVisualizationConfigAsync(
                request.Query, request.Columns, request.Data);

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating visualization");
            return StatusCode(500, new { error = "Failed to generate visualization", details = ex.Message });
        }
    }

    /// <summary>
    /// Get multiple visualization options for the same data
    /// </summary>
    /// <param name="request">Visualization request</param>
    /// <returns>Multiple visualization options with recommendations</returns>
    [HttpPost("options")]
    public async Task<ActionResult<VisualizationOptionsResponse>> GetVisualizationOptions([FromBody] VisualizationRequest request)
    {
        try
        {
            _logger.LogInformation("Getting visualization options for query: {Query}", request.Query);

            // Execute query to get data if not provided
            QueryResult queryResult;
            if (request.Data?.Any() != true)
            {
                var queryRequest = new QueryRequest { Question = request.Query };
                var result = await _queryService.ProcessQueryAsync(queryRequest, GetCurrentUserId());

                if (!result.Success || result.Result?.Data == null)
                {
                    return BadRequest(new { error = "Failed to execute query for visualization", details = result.Error });
                }

                queryResult = result.Result;
            }
            else
            {
                queryResult = new QueryResult
                {
                    Data = request.Data,
                    Metadata = new QueryMetadata { Columns = request.Columns }
                };
            }

            var options = await _visualizationService.GenerateMultipleVisualizationOptionsAsync(
                request.Query, queryResult.Metadata.Columns, queryResult.Data);

            // Add suitability analysis for each option
            var enhancedOptions = options.Select(option => new VisualizationOption
            {
                Config = option,
                IsSuitable = _visualizationService.IsVisualizationSuitableForData(
                    option.Type, queryResult.Metadata.Columns, queryResult.Data.Length),
                RecommendationScore = CalculateRecommendationScore(option, queryResult.Metadata.Columns, queryResult.Data.Length),
                PerformanceImpact = EstimatePerformanceImpact(option.Type, queryResult.Data.Length),
                Description = GetVisualizationDescription(option.Type)
            }).OrderByDescending(o => o.RecommendationScore).ToArray();

            return Ok(new VisualizationOptionsResponse
            {
                Options = enhancedOptions,
                DataSummary = new DataSummary
                {
                    RowCount = queryResult.Data.Length,
                    ColumnCount = queryResult.Metadata.Columns.Length,
                    HasNumericData = queryResult.Metadata.Columns.Any(c => IsNumericType(c.DataType)),
                    HasCategoricalData = queryResult.Metadata.Columns.Any(c => !IsNumericType(c.DataType) && !IsDateTimeType(c.DataType)),
                    HasTimeData = queryResult.Metadata.Columns.Any(c => IsDateTimeType(c.DataType))
                },
                RecommendedOption = enhancedOptions.FirstOrDefault()?.Config
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting visualization options");
            return StatusCode(500, new { error = "Failed to get visualization options", details = ex.Message });
        }
    }

    /// <summary>
    /// Validate if a visualization type is suitable for given data
    /// </summary>
    /// <param name="request">Validation request</param>
    /// <returns>Validation result with recommendations</returns>
    [HttpPost("validate")]
    public Task<ActionResult<VisualizationValidationResponse>> ValidateVisualization([FromBody] VisualizationValidationRequest request)
    {
        try
        {
            _logger.LogInformation("Validating visualization type {Type} for data", request.VisualizationType);

            var isSuitable = _visualizationService.IsVisualizationSuitableForData(
                request.VisualizationType, request.Columns, request.RowCount);

            var recommendations = GetAlternativeRecommendations(request.VisualizationType, request.Columns, request.RowCount);

            return Task.FromResult<ActionResult<VisualizationValidationResponse>>(Ok(new VisualizationValidationResponse
            {
                IsSuitable = isSuitable,
                RecommendationScore = isSuitable ? 1.0 : 0.3,
                Recommendations = recommendations.ToArray(),
                Issues = isSuitable ? Array.Empty<string>() : GetVisualizationIssues(request.VisualizationType, request.Columns, request.RowCount)
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating visualization");
            return Task.FromResult<ActionResult<VisualizationValidationResponse>>(StatusCode(500, new { error = "Failed to validate visualization", details = ex.Message }));
        }
    }

    /// <summary>
    /// Generate interactive visualization with advanced features
    /// </summary>
    [HttpPost("interactive")]
    public async Task<ActionResult<InteractiveVisualizationConfig>> GenerateInteractiveVisualization([FromBody] VisualizationRequest request)
    {
        try
        {
            _logger.LogInformation("Generating interactive visualization for query: {Query}", request.Query);

            var interactiveConfig = await _visualizationService.GenerateInteractiveVisualizationAsync(
                request.Query, request.Columns, request.Data);

            return Ok(interactiveConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating interactive visualization");
            return StatusCode(500, new { error = "Failed to generate interactive visualization", details = ex.Message });
        }
    }

    /// <summary>
    /// Generate multi-chart dashboard from query results
    /// </summary>
    [HttpPost("dashboard")]
    public async Task<ActionResult<DashboardConfig>> GenerateDashboard([FromBody] VisualizationRequest request)
    {
        try
        {
            _logger.LogInformation("Generating dashboard for query: {Query}", request.Query);

            var dashboardConfig = await _visualizationService.GenerateMultiChartDashboardAsync(
                request.Query, request.Columns, request.Data);

            return Ok(dashboardConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dashboard");
            return StatusCode(500, new { error = "Failed to generate dashboard", details = ex.Message });
        }
    }

    #endregion

    #region Advanced Visualization Operations

    /// <summary>
    /// Generate advanced visualization with AI-powered recommendations
    /// </summary>
    /// <param name="request">Advanced visualization request</param>
    /// <returns>Advanced visualization configuration with performance metrics</returns>
    [HttpPost("advanced")]
    public async Task<ActionResult<AdvancedVisualizationResponse>> GenerateAdvancedVisualization([FromBody] AdvancedVisualizationRequest request)
    {
        var userId = GetCurrentUserId();

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
            var visualization = await _visualizationService.GenerateAdvancedVisualizationAsync(
                request.Query,
                queryResult.Result.Metadata.Columns,
                queryResult.Result.Data,
                request.Preferences);

            var response = new AdvancedVisualizationResponse
            {
                Success = true,
                Visualization = visualization,
                DataSummary = new DataSummary
                {
                    RowCount = queryResult.Result.Data.Length,
                    ColumnCount = queryResult.Result.Metadata.Columns.Length,
                    HasNumericData = queryResult.Result.Metadata.Columns.Any(c => IsNumericType(c.DataType)),
                    HasCategoricalData = queryResult.Result.Metadata.Columns.Any(c => !IsNumericType(c.DataType) && !IsDateTimeType(c.DataType)),
                    HasTimeData = queryResult.Result.Metadata.Columns.Any(c => IsDateTimeType(c.DataType)),
                    DataComplexity = CalculateDataComplexity(queryResult.Result.Metadata.Columns, queryResult.Result.Data)
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
    /// <param name="request">Dashboard request</param>
    /// <returns>Advanced dashboard configuration</returns>
    [HttpPost("advanced/dashboard")]
    public async Task<ActionResult<AdvancedDashboardResponse>> GenerateAdvancedDashboard([FromBody] AdvancedDashboardRequest request)
    {
        var userId = GetCurrentUserId();

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
            var dashboard = await _visualizationService.GenerateAdvancedDashboardAsync(
                request.Query,
                queryResult.Result.Metadata.Columns,
                queryResult.Result.Data,
                request.Preferences);

            var response = new AdvancedDashboardResponse
            {
                Success = true,
                Dashboard = dashboard,
                ComplexityScore = CalculateComplexityScore(dashboard),
                EstimatedLoadTime = EstimateLoadTime(dashboard),
                RecommendedLayout = DetermineOptimalLayout(dashboard.Charts.Length)
            };

            _logger.LogInformation("Successfully generated advanced dashboard with {ChartCount} charts", dashboard.Charts.Length);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating advanced dashboard for user {UserId}", userId);
            return StatusCode(500, new { error = "Internal server error", details = ex.Message });
        }
    }

    #endregion

    #region Helper Methods

    private string GetCurrentUserId()
    {
        return User.FindFirst("sub")?.Value ?? "anonymous";
    }

    private double CalculateRecommendationScore(VisualizationConfig config, ColumnMetadata[] columns, int dataLength)
    {
        var score = 0.5; // Base score

        // Boost score based on data suitability
        if (_visualizationService.IsVisualizationSuitableForData(config.Type, columns, dataLength))
        {
            score += 0.3;
        }

        // Adjust based on data size and complexity
        if (dataLength < 1000) score += 0.1;
        if (columns.Length <= 5) score += 0.1;

        return Math.Min(1.0, score);
    }

    private string EstimatePerformanceImpact(string visualizationType, int dataLength)
    {
        return visualizationType switch
        {
            "table" when dataLength > 10000 => "High",
            "scatter" when dataLength > 5000 => "High",
            "heatmap" when dataLength > 1000 => "Medium",
            _ when dataLength > 50000 => "High",
            _ when dataLength > 10000 => "Medium",
            _ => "Low"
        };
    }

    private string GetVisualizationDescription(string type)
    {
        return type switch
        {
            "bar" => "Best for comparing categories or showing changes over time",
            "line" => "Ideal for showing trends and changes over continuous data",
            "pie" => "Perfect for showing proportions of a whole",
            "scatter" => "Great for showing relationships between two variables",
            "heatmap" => "Excellent for showing patterns in large datasets",
            "table" => "Comprehensive view of all data with sorting and filtering",
            _ => "Suitable visualization for your data"
        };
    }

    private bool IsNumericType(string dataType) =>
        dataType.Contains("int") || dataType.Contains("decimal") || dataType.Contains("float") || dataType.Contains("double");

    private bool IsDateTimeType(string dataType) =>
        dataType.Contains("date") || dataType.Contains("time");

    private List<string> GetAlternativeRecommendations(string type, ColumnMetadata[] columns, int rowCount)
    {
        var recommendations = new List<string>();

        if (type == "pie" && rowCount > 10)
        {
            recommendations.Add("Consider using a bar chart for better readability with many categories");
            recommendations.Add("Try a treemap for hierarchical proportional data");
        }

        if (type == "scatter" && rowCount > 5000)
        {
            recommendations.Add("Use a heatmap to handle large datasets more efficiently");
            recommendations.Add("Consider data sampling or aggregation");
        }

        if (!columns.Any(c => IsNumericType(c.DataType)) && (type == "line" || type == "area"))
        {
            recommendations.Add("Line and area charts require numeric data");
            recommendations.Add("Consider a bar chart for categorical data");
        }

        return recommendations;
    }

    private string[] GetVisualizationIssues(string type, ColumnMetadata[] columns, int rowCount)
    {
        var issues = new List<string>();

        if (type == "pie" && rowCount > 15)
        {
            issues.Add("Too many categories for pie chart readability");
        }

        if ((type == "line" || type == "area") && !columns.Any(c => IsNumericType(c.DataType)))
        {
            issues.Add("No numeric data available for line/area chart");
        }

        if (type == "scatter" && columns.Count(c => IsNumericType(c.DataType)) < 2)
        {
            issues.Add("Scatter plot requires at least two numeric columns");
        }

        return issues.ToArray();
    }

    private string CalculateDataComplexity(ColumnMetadata[] columns, object[] data)
    {
        var numericCount = columns.Count(c => IsNumericType(c.DataType));
        var categoricalCount = columns.Count(c => IsCategoricalType(c.DataType));
        var dateTimeCount = columns.Count(c => IsDateTimeType(c.DataType));

        if (data.Length > 100000 || columns.Length > 20) return "High";
        if (data.Length > 10000 || columns.Length > 10) return "Medium";
        return "Low";
    }

    private bool IsCategoricalType(string dataType) =>
        dataType.Contains("string") || dataType.Contains("varchar") || dataType.Contains("char");

    private double CalculateComplexityScore(AdvancedDashboardConfig dashboard)
    {
        var score = dashboard.Charts.Length * 0.2; // Base complexity from chart count

        // Add complexity for advanced features
        if (dashboard.RealTime?.Enabled == true) score += 0.3;
        if (dashboard.Collaboration?.Enabled == true) score += 0.2;
        if (dashboard.Analytics?.Enabled == true) score += 0.1;

        return Math.Min(1.0, score);
    }

    private string EstimateLoadTime(AdvancedDashboardConfig dashboard)
    {
        var chartCount = dashboard.Charts.Length;
        var hasRealTime = dashboard.RealTime?.Enabled == true;

        return (chartCount, hasRealTime) switch
        {
            ( > 10, true) => "5-8 seconds",
            ( > 10, false) => "3-5 seconds",
            ( > 5, true) => "3-4 seconds",
            ( > 5, false) => "2-3 seconds",
            (_, true) => "2-3 seconds",
            _ => "1-2 seconds"
        };
    }

    private string DetermineOptimalLayout(int chartCount)
    {
        return chartCount switch
        {
            1 => "single",
            2 => "side-by-side",
            <= 4 => "grid-2x2",
            <= 6 => "grid-2x3",
            <= 9 => "grid-3x3",
            _ => "masonry"
        };
    }

    #endregion
}

#region Request/Response Models

/// <summary>
/// Basic visualization request
/// </summary>
public class VisualizationRequest
{
    public string Query { get; set; } = string.Empty;
    public ColumnMetadata[] Columns { get; set; } = Array.Empty<ColumnMetadata>();
    public object[] Data { get; set; } = Array.Empty<object>();
}

/// <summary>
/// Visualization options response
/// </summary>
public class VisualizationOptionsResponse
{
    public VisualizationOption[] Options { get; set; } = Array.Empty<VisualizationOption>();
    public DataSummary DataSummary { get; set; } = new();
    public VisualizationConfig? RecommendedOption { get; set; }
}

/// <summary>
/// Individual visualization option
/// </summary>
public class VisualizationOption
{
    public VisualizationConfig Config { get; set; } = new();
    public bool IsSuitable { get; set; }
    public double RecommendationScore { get; set; }
    public string PerformanceImpact { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Data summary for visualization recommendations
/// </summary>
public class DataSummary
{
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public bool HasNumericData { get; set; }
    public bool HasCategoricalData { get; set; }
    public bool HasTimeData { get; set; }
    public string DataComplexity { get; set; } = string.Empty;
}

/// <summary>
/// Visualization validation request
/// </summary>
public class VisualizationValidationRequest
{
    [Required]
    public string VisualizationType { get; set; } = string.Empty;

    [Required]
    public ColumnMetadata[] Columns { get; set; } = Array.Empty<ColumnMetadata>();

    public int RowCount { get; set; }
}

/// <summary>
/// Visualization validation response
/// </summary>
public class VisualizationValidationResponse
{
    public bool IsSuitable { get; set; }
    public double RecommendationScore { get; set; }
    public string[] Recommendations { get; set; } = Array.Empty<string>();
    public string[] Issues { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Advanced visualization request
/// </summary>
public class AdvancedVisualizationRequest
{
    public string Query { get; set; } = string.Empty;
    public VisualizationPreferences? Preferences { get; set; }
}

/// <summary>
/// Advanced visualization response
/// </summary>
public class AdvancedVisualizationResponse
{
    public bool Success { get; set; }
    public AdvancedVisualizationConfig? Visualization { get; set; }
    public DataSummary DataSummary { get; set; } = new();
    public ChartPerformanceMetrics PerformanceMetrics { get; set; } = new();
}

/// <summary>
/// Advanced dashboard request
/// </summary>
public class AdvancedDashboardRequest
{
    public string Query { get; set; } = string.Empty;
    public DashboardPreferences? Preferences { get; set; }
}

/// <summary>
/// Advanced dashboard response
/// </summary>
public class AdvancedDashboardResponse
{
    public bool Success { get; set; }
    public AdvancedDashboardConfig? Dashboard { get; set; }
    public double ComplexityScore { get; set; }
    public string EstimatedLoadTime { get; set; } = string.Empty;
    public string RecommendedLayout { get; set; } = string.Empty;
}

/// <summary>
/// Chart performance metrics
/// </summary>
public class ChartPerformanceMetrics
{
    public string RenderTime { get; set; } = string.Empty;
    public int MemoryUsage { get; set; }
    public int DataPointsRendered { get; set; }
    public bool UsedSampling { get; set; }
    public bool UsedVirtualization { get; set; }
    public bool UsedWebGL { get; set; }
}

#endregion
