using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Infrastructure.Services;

namespace BIReportingCopilot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VisualizationController : ControllerBase
{
    private readonly IVisualizationService _visualizationService;
    private readonly ISqlQueryService _sqlQueryService;
    private readonly ILogger<VisualizationController> _logger;

    public VisualizationController(
        IVisualizationService visualizationService,
        ISqlQueryService sqlQueryService,
        ILogger<VisualizationController> logger)
    {
        _visualizationService = visualizationService;
        _sqlQueryService = sqlQueryService;
        _logger = logger;
    }

    /// <summary>
    /// Get multiple visualization options for given data
    /// </summary>
    [HttpPost("options")]
    public async Task<ActionResult<VisualizationOptionsResponse>> GetVisualizationOptions(
        [FromBody] VisualizationOptionsRequest request)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("Getting visualization options for user {UserId}", userId);

            // Execute the query to get data
            var queryResult = await _sqlQueryService.ExecuteSelectQueryAsync(request.Sql, request.Options);

            if (!queryResult.IsSuccessful || queryResult.Data == null)
            {
                return BadRequest(new { error = "Failed to execute query for visualization", details = queryResult.Metadata.Error });
            }

            // Generate multiple visualization options
            var options = await _visualizationService.GenerateMultipleVisualizationOptionsAsync(
                request.Question, queryResult.Metadata.Columns, queryResult.Data);

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
    /// Optimize a visualization configuration for better performance
    /// </summary>
    [HttpPost("optimize")]
    public async Task<ActionResult<VisualizationConfig>> OptimizeVisualization(
        [FromBody] OptimizeVisualizationRequest request)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("Optimizing visualization for user {UserId}", userId);

            var optimizedConfig = await _visualizationService.OptimizeVisualizationForDataSizeAsync(
                request.Config, request.DataSize);

            return Ok(optimizedConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing visualization");
            return StatusCode(500, new { error = "Failed to optimize visualization", details = ex.Message });
        }
    }

    /// <summary>
    /// Validate if a visualization type is suitable for given data characteristics
    /// </summary>
    [HttpPost("validate")]
    public ActionResult<VisualizationValidationResponse> ValidateVisualization(
        [FromBody] VisualizationValidationRequest request)
    {
        try
        {
            var isSuitable = _visualizationService.IsVisualizationSuitableForData(
                request.VisualizationType, request.Columns, request.RowCount);

            var recommendations = new List<string>();

            if (!isSuitable)
            {
                recommendations.AddRange(GetAlternativeRecommendations(request.VisualizationType, request.Columns, request.RowCount));
            }

            return Ok(new VisualizationValidationResponse
            {
                IsSuitable = isSuitable,
                RecommendationScore = isSuitable ? 1.0 : 0.3,
                Recommendations = recommendations.ToArray(),
                Issues = isSuitable ? Array.Empty<string>() : GetVisualizationIssues(request.VisualizationType, request.Columns, request.RowCount)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating visualization");
            return StatusCode(500, new { error = "Failed to validate visualization", details = ex.Message });
        }
    }

    /// <summary>
    /// Get available chart types and their descriptions
    /// </summary>
    [HttpGet("types")]
    public ActionResult<ChartTypeInfo[]> GetAvailableChartTypes()
    {
        var chartTypes = new[]
        {
            new ChartTypeInfo { Type = "bar", Name = "Bar Chart", Description = "Compare categories with horizontal bars", BestFor = "Categorical comparisons" },
            new ChartTypeInfo { Type = "column", Name = "Column Chart", Description = "Compare categories with vertical bars", BestFor = "Categorical comparisons" },
            new ChartTypeInfo { Type = "line", Name = "Line Chart", Description = "Show trends over time", BestFor = "Time series data" },
            new ChartTypeInfo { Type = "area", Name = "Area Chart", Description = "Show trends with filled areas", BestFor = "Time series with volume" },
            new ChartTypeInfo { Type = "pie", Name = "Pie Chart", Description = "Show parts of a whole", BestFor = "Proportional data (≤10 categories)" },
            new ChartTypeInfo { Type = "donut", Name = "Donut Chart", Description = "Pie chart with center space", BestFor = "Proportional data with focus" },
            new ChartTypeInfo { Type = "scatter", Name = "Scatter Plot", Description = "Show correlation between variables", BestFor = "Two numeric variables" },
            new ChartTypeInfo { Type = "bubble", Name = "Bubble Chart", Description = "Three-dimensional scatter plot", BestFor = "Three numeric variables" },
            new ChartTypeInfo { Type = "heatmap", Name = "Heat Map", Description = "Show patterns in matrix data", BestFor = "Large datasets, correlations" },
            new ChartTypeInfo { Type = "treemap", Name = "Tree Map", Description = "Hierarchical data as nested rectangles", BestFor = "Hierarchical proportions" },
            new ChartTypeInfo { Type = "sunburst", Name = "Sunburst Chart", Description = "Hierarchical data in circular layout", BestFor = "Multi-level hierarchies" },
            new ChartTypeInfo { Type = "gauge", Name = "Gauge Chart", Description = "Single value against a scale", BestFor = "KPIs and metrics" },
            new ChartTypeInfo { Type = "radar", Name = "Radar Chart", Description = "Multiple variables on radial axes", BestFor = "Multi-dimensional comparisons" },
            new ChartTypeInfo { Type = "waterfall", Name = "Waterfall Chart", Description = "Show cumulative effect of changes", BestFor = "Financial analysis" },
            new ChartTypeInfo { Type = "funnel", Name = "Funnel Chart", Description = "Show process stages", BestFor = "Conversion analysis" },
            new ChartTypeInfo { Type = "sankey", Name = "Sankey Diagram", Description = "Show flow between categories", BestFor = "Flow analysis" },
            new ChartTypeInfo { Type = "table", Name = "Data Table", Description = "Structured data display", BestFor = "Detailed data review" }
        };

        return Ok(chartTypes);
    }

    private double CalculateRecommendationScore(VisualizationConfig config, ColumnInfo[] columns, int rowCount)
    {
        var score = 0.5; // Base score

        // Boost score based on data suitability
        if (_visualizationService.IsVisualizationSuitableForData(config.Type, columns, rowCount))
            score += 0.3;

        // Adjust based on data size
        score += config.Type switch
        {
            "table" => Math.Min(0.2, 1000.0 / Math.Max(rowCount, 1)), // Better for smaller datasets
            "heatmap" => Math.Min(0.2, rowCount / 1000.0), // Better for larger datasets
            "pie" or "donut" => Math.Max(0, 0.2 - (rowCount - 10) * 0.01), // Worse with more categories
            _ => 0.1
        };

        return Math.Min(1.0, Math.Max(0.0, score));
    }

    private string EstimatePerformanceImpact(string visualizationType, int dataSize)
    {
        return (visualizationType, dataSize) switch
        {
            (_, <= 100) => "Low",
            ("table", _) => "Low",
            ("heatmap", > 10000) => "High",
            ("scatter", > 5000) => "High",
            ("bubble", > 1000) => "High",
            (_, > 5000) => "Medium",
            _ => "Low"
        };
    }

    private string GetVisualizationDescription(string type)
    {
        return type switch
        {
            "bar" => "Horizontal bars for comparing categories",
            "column" => "Vertical bars for comparing categories",
            "line" => "Connected points showing trends over time",
            "area" => "Filled line chart showing volume trends",
            "pie" => "Circular chart showing proportions",
            "donut" => "Pie chart with hollow center",
            "scatter" => "Points showing correlation between two variables",
            "bubble" => "Scatter plot with size representing third variable",
            "heatmap" => "Color-coded matrix showing patterns",
            "treemap" => "Nested rectangles for hierarchical data",
            "sunburst" => "Circular hierarchy visualization",
            "gauge" => "Speedometer-style single value display",
            "radar" => "Multi-axis comparison chart",
            "waterfall" => "Cumulative changes visualization",
            "funnel" => "Process flow with decreasing stages",
            "sankey" => "Flow diagram between categories",
            "table" => "Structured data in rows and columns",
            _ => "Data visualization"
        };
    }

    private List<string> GetAlternativeRecommendations(string type, ColumnInfo[] columns, int rowCount)
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

    private string[] GetVisualizationIssues(string type, ColumnInfo[] columns, int rowCount)
    {
        var issues = new List<string>();

        if (type == "pie" && rowCount > 20)
            issues.Add("Too many categories for effective pie chart display");

        if (type == "scatter" && columns.Count(c => IsNumericType(c.DataType)) < 2)
            issues.Add("Scatter plot requires at least two numeric columns");

        if ((type == "line" || type == "area") && !columns.Any(c => IsDateTimeType(c.DataType)))
            issues.Add("Time-based charts work best with date/time data");

        return issues.ToArray();
    }

    private bool IsNumericType(string dataType)
    {
        var lowerType = dataType.ToLowerInvariant();
        return lowerType.Contains("int") || lowerType.Contains("decimal") ||
               lowerType.Contains("float") || lowerType.Contains("double") ||
               lowerType.Contains("number") || lowerType.Contains("numeric");
    }

    private bool IsDateTimeType(string dataType)
    {
        var lowerType = dataType.ToLowerInvariant();
        return lowerType.Contains("date") || lowerType.Contains("time") || lowerType.Contains("timestamp");
    }

    /// <summary>
    /// Generate interactive visualization with advanced features
    /// </summary>
    [HttpPost("interactive")]
    public async Task<ActionResult<InteractiveVisualizationConfig>> GenerateInteractiveVisualization(
        [FromBody] VisualizationRequest request)
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
    public async Task<ActionResult<DashboardConfig>> GenerateDashboard(
        [FromBody] VisualizationRequest request)
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
}

// Request/Response models
public class VisualizationOptionsRequest
{
    public string Question { get; set; } = string.Empty;
    public string Sql { get; set; } = string.Empty;
    public QueryOptions Options { get; set; } = new();
}

public class VisualizationOptionsResponse
{
    public VisualizationOption[] Options { get; set; } = Array.Empty<VisualizationOption>();
    public DataSummary DataSummary { get; set; } = new();
    public VisualizationConfig? RecommendedOption { get; set; }
}

public class VisualizationOption
{
    public VisualizationConfig Config { get; set; } = new();
    public bool IsSuitable { get; set; }
    public double RecommendationScore { get; set; }
    public string PerformanceImpact { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}



public class OptimizeVisualizationRequest
{
    public VisualizationConfig Config { get; set; } = new();
    public int DataSize { get; set; }
}

public class VisualizationValidationRequest
{
    public string VisualizationType { get; set; } = string.Empty;
    public ColumnInfo[] Columns { get; set; } = Array.Empty<ColumnInfo>();
    public int RowCount { get; set; }
}

public class VisualizationValidationResponse
{
    public bool IsSuitable { get; set; }
    public double RecommendationScore { get; set; }
    public string[] Recommendations { get; set; } = Array.Empty<string>();
    public string[] Issues { get; set; } = Array.Empty<string>();
}

public class VisualizationRequest
{
    public string Query { get; set; } = string.Empty;
    public ColumnInfo[] Columns { get; set; } = Array.Empty<ColumnInfo>();
    public object[] Data { get; set; } = Array.Empty<object>();
}

public class ChartTypeInfo
{
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BestFor { get; set; } = string.Empty;
}

public class EnhancedChartTypeInfo : ChartTypeInfo
{
    public string[] InteractiveFeatures { get; set; } = Array.Empty<string>();
    public string[] SupportedDataTypes { get; set; } = Array.Empty<string>();
    public int MaxRecommendedRows { get; set; }
}
