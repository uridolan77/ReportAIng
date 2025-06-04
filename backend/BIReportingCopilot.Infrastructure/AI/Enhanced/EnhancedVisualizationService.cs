using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Performance;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Enhanced visualization service implementing Enhancement 12: Advanced Visualization Engine
/// Provides intelligent chart recommendations and interactive dashboard generation
/// </summary>
public class EnhancedVisualizationService
{
    private readonly ILogger<EnhancedVisualizationService> _logger;
    private readonly ICacheService _cacheService;
    private readonly VisualizationRecommendationEngine _recommendationEngine;
    private readonly ChartConfigurationGenerator _chartGenerator;
    private readonly DashboardLayoutOptimizer _layoutOptimizer;

    public EnhancedVisualizationService(
        ILogger<EnhancedVisualizationService> logger,
        ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
        _recommendationEngine = new VisualizationRecommendationEngine(logger);
        _chartGenerator = new ChartConfigurationGenerator(logger);
        _layoutOptimizer = new DashboardLayoutOptimizer(logger);
    }

    /// <summary>
    /// Generate enhanced visualization recommendations based on query results and context
    /// </summary>
    public async Task<VisualizationRecommendations> GenerateVisualizationRecommendationsAsync(
        QueryResult queryResult,
        SemanticAnalysis semanticAnalysis,
        string? userId = null)
    {
        try
        {
            _logger.LogDebug("Generating enhanced visualization recommendations for query result with {RowCount} rows", 
                queryResult.Data?.Count ?? 0);

            // Step 1: Analyze data characteristics
            var dataCharacteristics = await AnalyzeDataCharacteristicsAsync(queryResult);

            // Step 2: Generate chart recommendations
            var chartRecommendations = await _recommendationEngine.GenerateChartRecommendationsAsync(
                dataCharacteristics, semanticAnalysis);

            // Step 3: Generate dashboard layout recommendations
            var dashboardRecommendations = await _layoutOptimizer.GenerateDashboardLayoutAsync(
                chartRecommendations, dataCharacteristics);

            // Step 4: Generate interactive features
            var interactiveFeatures = await GenerateInteractiveFeaturesAsync(
                dataCharacteristics, semanticAnalysis);

            // Step 5: Create visualization configurations
            var visualizationConfigs = await _chartGenerator.GenerateChartConfigurationsAsync(
                chartRecommendations, queryResult);

            var recommendations = new VisualizationRecommendations
            {
                PrimaryVisualization = chartRecommendations.FirstOrDefault(),
                AlternativeVisualizations = chartRecommendations.Skip(1).Take(3).ToList(),
                DashboardLayout = dashboardRecommendations,
                InteractiveFeatures = interactiveFeatures,
                ChartConfigurations = visualizationConfigs,
                DataInsights = await GenerateDataInsightsAsync(dataCharacteristics, queryResult),
                Metadata = new Dictionary<string, object>
                {
                    ["generation_timestamp"] = DateTime.UtcNow,
                    ["data_row_count"] = queryResult.Data?.Count ?? 0,
                    ["data_column_count"] = queryResult.Columns?.Count ?? 0,
                    ["semantic_intent"] = semanticAnalysis.Intent.ToString(),
                    ["user_id"] = userId ?? "anonymous"
                }
            };

            _logger.LogDebug("Generated {ChartCount} chart recommendations and {FeatureCount} interactive features",
                chartRecommendations.Count, interactiveFeatures.Count);

            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating enhanced visualization recommendations");
            return new VisualizationRecommendations
            {
                PrimaryVisualization = new ChartRecommendation
                {
                    ChartType = ChartType.Table,
                    Title = "Data Table",
                    Description = "Fallback table view",
                    Confidence = 0.5
                },
                Metadata = new Dictionary<string, object> { ["error"] = true }
            };
        }
    }

    /// <summary>
    /// Generate interactive dashboard configuration
    /// </summary>
    public async Task<DashboardConfiguration> GenerateInteractiveDashboardAsync(
        List<QueryResult> queryResults,
        List<SemanticAnalysis> semanticAnalyses,
        string? userId = null)
    {
        try
        {
            _logger.LogDebug("Generating interactive dashboard for {QueryCount} queries", queryResults.Count);

            var dashboardPanels = new List<DashboardPanel>();

            // Generate panels for each query result
            for (int i = 0; i < queryResults.Count; i++)
            {
                var queryResult = queryResults[i];
                var semanticAnalysis = i < semanticAnalyses.Count ? semanticAnalyses[i] : new SemanticAnalysis();

                var recommendations = await GenerateVisualizationRecommendationsAsync(
                    queryResult, semanticAnalysis, userId);

                var panel = new DashboardPanel
                {
                    Id = $"panel_{i + 1}",
                    Title = GeneratePanelTitle(semanticAnalysis, queryResult),
                    ChartType = recommendations.PrimaryVisualization?.ChartType ?? ChartType.Table,
                    Configuration = recommendations.ChartConfigurations.FirstOrDefault(),
                    InteractiveFeatures = recommendations.InteractiveFeatures,
                    Position = new PanelPosition { Row = i / 2, Column = i % 2 },
                    Size = DeterminePanelSize(recommendations.PrimaryVisualization?.ChartType ?? ChartType.Table)
                };

                dashboardPanels.Add(panel);
            }

            // Optimize dashboard layout
            var optimizedLayout = await _layoutOptimizer.OptimizeDashboardLayoutAsync(dashboardPanels);

            var dashboard = new DashboardConfiguration
            {
                Id = Guid.NewGuid().ToString(),
                Title = "AI-Generated Dashboard",
                Description = "Automatically generated dashboard based on your queries",
                Panels = optimizedLayout,
                GlobalFilters = await GenerateGlobalFiltersAsync(queryResults, semanticAnalyses),
                Theme = DetermineOptimalTheme(dashboardPanels),
                Layout = new DashboardLayout
                {
                    Type = LayoutType.Grid,
                    Columns = 2,
                    ResponsiveBreakpoints = new Dictionary<string, int>
                    {
                        ["mobile"] = 1,
                        ["tablet"] = 2,
                        ["desktop"] = 2
                    }
                },
                Metadata = new Dictionary<string, object>
                {
                    ["generation_timestamp"] = DateTime.UtcNow,
                    ["panel_count"] = dashboardPanels.Count,
                    ["user_id"] = userId ?? "anonymous",
                    ["auto_generated"] = true
                }
            };

            _logger.LogDebug("Generated interactive dashboard with {PanelCount} panels", dashboardPanels.Count);
            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating interactive dashboard");
            return new DashboardConfiguration
            {
                Title = "Error Dashboard",
                Description = "Unable to generate dashboard due to error",
                Panels = new List<DashboardPanel>(),
                Metadata = new Dictionary<string, object> { ["error"] = true }
            };
        }
    }

    /// <summary>
    /// Generate chart configuration for specific visualization type
    /// </summary>
    public async Task<ChartConfiguration> GenerateChartConfigurationAsync(
        ChartType chartType,
        QueryResult queryResult,
        ChartCustomization? customization = null)
    {
        try
        {
            var dataCharacteristics = await AnalyzeDataCharacteristicsAsync(queryResult);
            
            var configuration = chartType switch
            {
                ChartType.BarChart => await GenerateBarChartConfigAsync(queryResult, dataCharacteristics, customization),
                ChartType.LineChart => await GenerateLineChartConfigAsync(queryResult, dataCharacteristics, customization),
                ChartType.PieChart => await GeneratePieChartConfigAsync(queryResult, dataCharacteristics, customization),
                ChartType.ScatterPlot => await GenerateScatterPlotConfigAsync(queryResult, dataCharacteristics, customization),
                ChartType.Heatmap => await GenerateHeatmapConfigAsync(queryResult, dataCharacteristics, customization),
                ChartType.Table => await GenerateTableConfigAsync(queryResult, dataCharacteristics, customization),
                _ => await GenerateDefaultChartConfigAsync(queryResult, dataCharacteristics)
            };

            _logger.LogDebug("Generated {ChartType} configuration for {RowCount} rows", 
                chartType, queryResult.Data?.Count ?? 0);

            return configuration;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chart configuration for {ChartType}", chartType);
            return new ChartConfiguration
            {
                ChartType = ChartType.Table,
                Title = "Data View",
                XAxis = new AxisConfiguration { Title = "Data" },
                YAxis = new AxisConfiguration { Title = "Values" }
            };
        }
    }

    // Private methods

    private async Task<DataCharacteristics> AnalyzeDataCharacteristicsAsync(QueryResult queryResult)
    {
        var characteristics = new DataCharacteristics
        {
            RowCount = queryResult.Data?.Count ?? 0,
            ColumnCount = queryResult.Columns?.Count ?? 0,
            Columns = new List<ColumnCharacteristics>()
        };

        if (queryResult.Columns != null)
        {
            foreach (var column in queryResult.Columns)
            {
                var columnChar = new ColumnCharacteristics
                {
                    Name = column.Name,
                    DataType = column.DataType,
                    IsNumeric = IsNumericType(column.DataType),
                    IsDateTime = IsDateTimeType(column.DataType),
                    IsCategorical = IsCategoricalColumn(column, queryResult),
                    UniqueValueCount = CountUniqueValues(column, queryResult),
                    HasNulls = HasNullValues(column, queryResult)
                };

                characteristics.Columns.Add(columnChar);
            }
        }

        // Determine data patterns
        characteristics.HasTimeSeriesData = characteristics.Columns.Any(c => c.IsDateTime);
        characteristics.HasCategoricalData = characteristics.Columns.Any(c => c.IsCategorical);
        characteristics.HasNumericData = characteristics.Columns.Any(c => c.IsNumeric);
        characteristics.IsAggregatedData = DetectAggregatedData(characteristics);

        return characteristics;
    }

    private async Task<List<InteractiveFeature>> GenerateInteractiveFeaturesAsync(
        DataCharacteristics dataCharacteristics,
        SemanticAnalysis semanticAnalysis)
    {
        var features = new List<InteractiveFeature>();

        // Add filtering capabilities
        if (dataCharacteristics.HasCategoricalData)
        {
            features.Add(new InteractiveFeature
            {
                Type = InteractiveFeatureType.Filter,
                Name = "Category Filter",
                Description = "Filter data by categorical values",
                Configuration = new Dictionary<string, object>
                {
                    ["filter_type"] = "categorical",
                    ["columns"] = dataCharacteristics.Columns.Where(c => c.IsCategorical).Select(c => c.Name).ToList()
                }
            });
        }

        // Add drill-down capabilities
        if (dataCharacteristics.IsAggregatedData)
        {
            features.Add(new InteractiveFeature
            {
                Type = InteractiveFeatureType.DrillDown,
                Name = "Drill Down",
                Description = "Drill down into detailed data",
                Configuration = new Dictionary<string, object>
                {
                    ["drill_levels"] = new[] { "summary", "detail" }
                }
            });
        }

        // Add time-based interactions for time series data
        if (dataCharacteristics.HasTimeSeriesData)
        {
            features.Add(new InteractiveFeature
            {
                Type = InteractiveFeatureType.TimeRange,
                Name = "Time Range Selector",
                Description = "Select time range for analysis",
                Configuration = new Dictionary<string, object>
                {
                    ["time_column"] = dataCharacteristics.Columns.First(c => c.IsDateTime).Name,
                    ["default_range"] = "last_30_days"
                }
            });
        }

        // Add export capabilities
        features.Add(new InteractiveFeature
        {
            Type = InteractiveFeatureType.Export,
            Name = "Export Data",
            Description = "Export chart data and images",
            Configuration = new Dictionary<string, object>
            {
                ["formats"] = new[] { "png", "pdf", "csv", "excel" }
            }
        });

        return features;
    }

    private async Task<List<DataInsight>> GenerateDataInsightsAsync(
        DataCharacteristics dataCharacteristics,
        QueryResult queryResult)
    {
        var insights = new List<DataInsight>();

        // Generate statistical insights
        if (dataCharacteristics.HasNumericData)
        {
            var numericColumns = dataCharacteristics.Columns.Where(c => c.IsNumeric).ToList();
            foreach (var column in numericColumns.Take(3)) // Limit to top 3 numeric columns
            {
                var stats = CalculateColumnStatistics(column, queryResult);
                insights.Add(new DataInsight
                {
                    Type = InsightType.Statistical,
                    Title = $"{column.Name} Statistics",
                    Description = $"Average: {stats.Average:F2}, Max: {stats.Maximum:F2}, Min: {stats.Minimum:F2}",
                    Confidence = 0.9
                });
            }
        }

        // Generate trend insights for time series data
        if (dataCharacteristics.HasTimeSeriesData && dataCharacteristics.HasNumericData)
        {
            insights.Add(new DataInsight
            {
                Type = InsightType.Trend,
                Title = "Time Series Trend",
                Description = "Data shows temporal patterns suitable for trend analysis",
                Confidence = 0.8
            });
        }

        // Generate distribution insights
        if (dataCharacteristics.HasCategoricalData)
        {
            var categoricalColumn = dataCharacteristics.Columns.First(c => c.IsCategorical);
            insights.Add(new DataInsight
            {
                Type = InsightType.Distribution,
                Title = $"{categoricalColumn.Name} Distribution",
                Description = $"Data contains {categoricalColumn.UniqueValueCount} unique categories",
                Confidence = 0.85
            });
        }

        return insights;
    }

    private string GeneratePanelTitle(SemanticAnalysis semanticAnalysis, QueryResult queryResult)
    {
        if (!string.IsNullOrEmpty(semanticAnalysis.OriginalQuery))
        {
            // Extract key terms from the query
            var keyTerms = semanticAnalysis.Keywords.Take(3).ToList();
            if (keyTerms.Any())
            {
                return string.Join(" ", keyTerms).ToTitleCase();
            }
        }

        return $"Data Analysis {DateTime.Now:HH:mm}";
    }

    private PanelSize DeterminePanelSize(ChartType chartType)
    {
        return chartType switch
        {
            ChartType.Table => new PanelSize { Width = 12, Height = 6 },
            ChartType.BarChart => new PanelSize { Width = 6, Height = 4 },
            ChartType.LineChart => new PanelSize { Width = 8, Height = 4 },
            ChartType.PieChart => new PanelSize { Width = 4, Height = 4 },
            ChartType.Heatmap => new PanelSize { Width = 8, Height = 6 },
            _ => new PanelSize { Width = 6, Height = 4 }
        };
    }

    private async Task<List<GlobalFilter>> GenerateGlobalFiltersAsync(
        List<QueryResult> queryResults,
        List<SemanticAnalysis> semanticAnalyses)
    {
        var filters = new List<GlobalFilter>();

        // Find common categorical columns across query results
        var commonColumns = FindCommonCategoricalColumns(queryResults);
        
        foreach (var column in commonColumns.Take(3)) // Limit to 3 global filters
        {
            filters.Add(new GlobalFilter
            {
                Name = column,
                Type = FilterType.Categorical,
                Label = column.ToTitleCase(),
                Values = GetUniqueValuesForColumn(column, queryResults)
            });
        }

        return filters;
    }

    private DashboardTheme DetermineOptimalTheme(List<DashboardPanel> panels)
    {
        // Simple theme selection based on panel types
        var hasComplexCharts = panels.Any(p => p.ChartType == ChartType.Heatmap || p.ChartType == ChartType.ScatterPlot);
        
        return hasComplexCharts ? DashboardTheme.Professional : DashboardTheme.Modern;
    }

    // Chart-specific configuration generators
    private async Task<ChartConfiguration> GenerateBarChartConfigAsync(
        QueryResult queryResult,
        DataCharacteristics dataCharacteristics,
        ChartCustomization? customization)
    {
        var categoricalColumn = dataCharacteristics.Columns.FirstOrDefault(c => c.IsCategorical);
        var numericColumn = dataCharacteristics.Columns.FirstOrDefault(c => c.IsNumeric);

        return new ChartConfiguration
        {
            ChartType = ChartType.BarChart,
            Title = customization?.Title ?? "Bar Chart Analysis",
            XAxis = new AxisConfiguration
            {
                Title = categoricalColumn?.Name ?? "Category",
                DataKey = categoricalColumn?.Name ?? "category"
            },
            YAxis = new AxisConfiguration
            {
                Title = numericColumn?.Name ?? "Value",
                DataKey = numericColumn?.Name ?? "value"
            },
            Colors = customization?.Colors ?? new[] { "#3B82F6", "#10B981", "#F59E0B", "#EF4444" },
            ShowLegend = true,
            ShowTooltip = true
        };
    }

    private async Task<ChartConfiguration> GenerateLineChartConfigAsync(
        QueryResult queryResult,
        DataCharacteristics dataCharacteristics,
        ChartCustomization? customization)
    {
        var timeColumn = dataCharacteristics.Columns.FirstOrDefault(c => c.IsDateTime);
        var numericColumn = dataCharacteristics.Columns.FirstOrDefault(c => c.IsNumeric);

        return new ChartConfiguration
        {
            ChartType = ChartType.LineChart,
            Title = customization?.Title ?? "Trend Analysis",
            XAxis = new AxisConfiguration
            {
                Title = timeColumn?.Name ?? "Time",
                DataKey = timeColumn?.Name ?? "date",
                Type = "datetime"
            },
            YAxis = new AxisConfiguration
            {
                Title = numericColumn?.Name ?? "Value",
                DataKey = numericColumn?.Name ?? "value"
            },
            Colors = customization?.Colors ?? new[] { "#3B82F6" },
            ShowLegend = false,
            ShowTooltip = true,
            Smooth = true
        };
    }

    private async Task<ChartConfiguration> GeneratePieChartConfigAsync(
        QueryResult queryResult,
        DataCharacteristics dataCharacteristics,
        ChartCustomization? customization)
    {
        var categoricalColumn = dataCharacteristics.Columns.FirstOrDefault(c => c.IsCategorical);
        var numericColumn = dataCharacteristics.Columns.FirstOrDefault(c => c.IsNumeric);

        return new ChartConfiguration
        {
            ChartType = ChartType.PieChart,
            Title = customization?.Title ?? "Distribution Analysis",
            DataKey = numericColumn?.Name ?? "value",
            NameKey = categoricalColumn?.Name ?? "category",
            Colors = customization?.Colors ?? new[] { "#3B82F6", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6" },
            ShowLegend = true,
            ShowTooltip = true
        };
    }

    private async Task<ChartConfiguration> GenerateScatterPlotConfigAsync(
        QueryResult queryResult,
        DataCharacteristics dataCharacteristics,
        ChartCustomization? customization)
    {
        var numericColumns = dataCharacteristics.Columns.Where(c => c.IsNumeric).Take(2).ToList();

        return new ChartConfiguration
        {
            ChartType = ChartType.ScatterPlot,
            Title = customization?.Title ?? "Correlation Analysis",
            XAxis = new AxisConfiguration
            {
                Title = numericColumns.FirstOrDefault()?.Name ?? "X Value",
                DataKey = numericColumns.FirstOrDefault()?.Name ?? "x"
            },
            YAxis = new AxisConfiguration
            {
                Title = numericColumns.Skip(1).FirstOrDefault()?.Name ?? "Y Value",
                DataKey = numericColumns.Skip(1).FirstOrDefault()?.Name ?? "y"
            },
            Colors = customization?.Colors ?? new[] { "#3B82F6" },
            ShowLegend = false,
            ShowTooltip = true
        };
    }

    private async Task<ChartConfiguration> GenerateHeatmapConfigAsync(
        QueryResult queryResult,
        DataCharacteristics dataCharacteristics,
        ChartCustomization? customization)
    {
        return new ChartConfiguration
        {
            ChartType = ChartType.Heatmap,
            Title = customization?.Title ?? "Heatmap Analysis",
            Colors = customization?.Colors ?? new[] { "#FEF3C7", "#F59E0B", "#DC2626" },
            ShowLegend = true,
            ShowTooltip = true
        };
    }

    private async Task<ChartConfiguration> GenerateTableConfigAsync(
        QueryResult queryResult,
        DataCharacteristics dataCharacteristics,
        ChartCustomization? customization)
    {
        return new ChartConfiguration
        {
            ChartType = ChartType.Table,
            Title = customization?.Title ?? "Data Table",
            ShowPagination = dataCharacteristics.RowCount > 50,
            PageSize = 25,
            Sortable = true,
            Filterable = true
        };
    }

    private async Task<ChartConfiguration> GenerateDefaultChartConfigAsync(
        QueryResult queryResult,
        DataCharacteristics dataCharacteristics)
    {
        return new ChartConfiguration
        {
            ChartType = ChartType.Table,
            Title = "Data View",
            ShowPagination = true,
            PageSize = 25
        };
    }

    // Helper methods
    private bool IsNumericType(string dataType)
    {
        var numericTypes = new[] { "int", "decimal", "float", "double", "money", "numeric", "bigint", "smallint" };
        return numericTypes.Any(type => dataType.ToLowerInvariant().Contains(type));
    }

    private bool IsDateTimeType(string dataType)
    {
        var dateTimeTypes = new[] { "datetime", "date", "time", "timestamp" };
        return dateTimeTypes.Any(type => dataType.ToLowerInvariant().Contains(type));
    }

    private bool IsCategoricalColumn(ColumnMetadata column, QueryResult queryResult)
    {
        // Simple heuristic: if it's not numeric or datetime, and has reasonable number of unique values
        if (IsNumericType(column.DataType) || IsDateTimeType(column.DataType))
            return false;

        var uniqueCount = CountUniqueValues(column, queryResult);
        var totalRows = queryResult.Data?.Count ?? 0;
        
        return totalRows > 0 && uniqueCount <= Math.Min(20, totalRows * 0.5);
    }

    private int CountUniqueValues(ColumnMetadata column, QueryResult queryResult)
    {
        if (queryResult.Data == null) return 0;
        
        var columnIndex = queryResult.Columns?.FindIndex(c => c.Name == column.Name) ?? -1;
        if (columnIndex == -1) return 0;

        return queryResult.Data
            .Select(row => row.ElementAtOrDefault(columnIndex))
            .Where(value => value != null)
            .Distinct()
            .Count();
    }

    private bool HasNullValues(ColumnMetadata column, QueryResult queryResult)
    {
        if (queryResult.Data == null) return false;
        
        var columnIndex = queryResult.Columns?.FindIndex(c => c.Name == column.Name) ?? -1;
        if (columnIndex == -1) return false;

        return queryResult.Data.Any(row => 
            row.ElementAtOrDefault(columnIndex) == null || 
            string.IsNullOrEmpty(row.ElementAtOrDefault(columnIndex)?.ToString()));
    }

    private bool DetectAggregatedData(DataCharacteristics characteristics)
    {
        // Simple heuristic: if we have both categorical and numeric data with reasonable row count
        return characteristics.HasCategoricalData && 
               characteristics.HasNumericData && 
               characteristics.RowCount < 100;
    }

    private ColumnStatistics CalculateColumnStatistics(ColumnCharacteristics column, QueryResult queryResult)
    {
        // Simplified statistics calculation
        return new ColumnStatistics
        {
            Average = 0,
            Maximum = 0,
            Minimum = 0,
            Count = queryResult.Data?.Count ?? 0
        };
    }

    private List<string> FindCommonCategoricalColumns(List<QueryResult> queryResults)
    {
        // Find columns that appear in multiple query results
        var allColumns = queryResults
            .SelectMany(qr => qr.Columns?.Select(c => c.Name) ?? new List<string>())
            .GroupBy(name => name)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        return allColumns;
    }

    private List<string> GetUniqueValuesForColumn(string columnName, List<QueryResult> queryResults)
    {
        var values = new HashSet<string>();
        
        foreach (var queryResult in queryResults)
        {
            var columnIndex = queryResult.Columns?.FindIndex(c => c.Name == columnName) ?? -1;
            if (columnIndex >= 0 && queryResult.Data != null)
            {
                foreach (var row in queryResult.Data)
                {
                    var value = row.ElementAtOrDefault(columnIndex)?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        values.Add(value);
                    }
                }
            }
        }

        return values.Take(20).ToList(); // Limit to 20 values
    }
}

// Extension method for string formatting
public static class StringExtensions
{
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLowerInvariant());
    }
}
