using BIReportingCopilot.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces;

namespace BIReportingCopilot.Infrastructure.Services;

public interface IVisualizationService
{
    // Basic visualization methods
    Task<VisualizationConfig> GenerateVisualizationConfigAsync(string query, ColumnMetadata[] columns, object[] data);
    Task<VisualizationConfig[]> GenerateMultipleVisualizationOptionsAsync(string query, ColumnMetadata[] columns, object[] data);
    Task<VisualizationConfig> OptimizeVisualizationForDataSizeAsync(VisualizationConfig config, int dataSize);
    bool IsVisualizationSuitableForData(string visualizationType, ColumnMetadata[] columns, int rowCount);
    Task<InteractiveVisualizationConfig> GenerateInteractiveVisualizationAsync(string query, ColumnMetadata[] columns, object[] data);
    Task<DashboardConfig> GenerateMultiChartDashboardAsync(string query, ColumnMetadata[] columns, object[] data);

    // Advanced visualization methods (consolidated from IAdvancedVisualizationService)
    Task<AdvancedVisualizationConfig> GenerateAdvancedVisualizationAsync(
        string query,
        ColumnMetadata[] columns,
        object[] data,
        VisualizationPreferences? preferences = null);

    Task<AdvancedDashboardConfig> GenerateAdvancedDashboardAsync(
        string query,
        ColumnMetadata[] columns,
        object[] data,
        DashboardPreferences? preferences = null);

    Task<VisualizationRecommendation[]> GetVisualizationRecommendationsAsync(
        ColumnMetadata[] columns,
        object[] data,
        string? context = null);

    Task<AdvancedVisualizationConfig> OptimizeVisualizationForPerformanceAsync(
        AdvancedVisualizationConfig config,
        int dataSize);
}

public class VisualizationService : IVisualizationService
{
    private readonly IAIService _aiService;
    private readonly ILogger<VisualizationService> _logger;
    private readonly ICacheService _cacheService;

    public VisualizationService(
        IAIService aiService,
        ILogger<VisualizationService> logger,
        ICacheService cacheService)
    {
        _aiService = aiService;
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task<VisualizationConfig> GenerateVisualizationConfigAsync(string query, ColumnMetadata[] columns, object[] data)
    {
        try
        {
            var dataAnalysis = AnalyzeDataCharacteristics(columns, data);
            var recommendedType = DetermineOptimalVisualizationType(dataAnalysis);

            // Use AI to generate enhanced configuration
            var aiConfig = await _aiService.GenerateVisualizationConfigAsync(query, columns, data);
            var parsedConfig = ParseAIVisualizationConfig(aiConfig);

            var config = new VisualizationConfig
            {
                Type = recommendedType,
                Title = GenerateTitle(query, dataAnalysis),
                XAxis = DetermineXAxis(columns, dataAnalysis),
                YAxis = DetermineYAxis(columns, dataAnalysis),
                Series = DetermineSeries(columns, dataAnalysis),
                Config = MergeConfigurations(GetDefaultConfig(recommendedType), parsedConfig),
                Theme = ChartTheme.Modern,
                ColorScheme = ColorScheme.Business,
                EnableInteractivity = true,
                EnableAnimation = data.Length <= 1000,
                Metadata = new VisualizationMetadata
                {
                    DataPointCount = data.Length,
                    ConfidenceScore = CalculateConfidence(dataAnalysis, recommendedType),
                    GeneratedAt = DateTime.UtcNow,
                    OptimizationLevel = data.Length > 1000 ? "Performance" : "Standard",
                    DataSource = "Query Result"
                }
            };

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating visualization config for query: {Query}", query);
            return GetFallbackVisualizationConfig(query, columns, data);
        }
    }

    public async Task<VisualizationConfig[]> GenerateMultipleVisualizationOptionsAsync(string query, ColumnMetadata[] columns, object[] data)
    {
        try
        {
            var dataAnalysis = AnalyzeDataCharacteristics(columns, data);
            var options = new List<VisualizationConfig>();

            // Generate primary recommendation
            var primary = await GenerateVisualizationConfigAsync(query, columns, data);
            options.Add(primary);

            // Generate alternative options based on data characteristics
            if (dataAnalysis.NumericColumns.Count >= 2)
            {
                options.Add(CreateVisualizationConfig("scatter", "Correlation Analysis", columns, data, dataAnalysis));
            }

            if (dataAnalysis.HasTimeColumn && dataAnalysis.NumericColumns.Any())
            {
                options.Add(CreateVisualizationConfig("line", "Trend Analysis", columns, data, dataAnalysis));
            }

            if (dataAnalysis.CategoricalColumns.Any() && dataAnalysis.NumericColumns.Any())
            {
                options.Add(CreateVisualizationConfig("bar", "Category Comparison", columns, data, dataAnalysis));
            }

            // Always include table as fallback
            if (!options.Any(o => o.Type == "table"))
            {
                options.Add(CreateVisualizationConfig("table", "Detailed View", columns, data, dataAnalysis));
            }

            return options.Take(4).ToArray(); // Limit to 4 options
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating multiple visualization options");
            return new[] { GetFallbackVisualizationConfig(query, columns, data) };
        }
    }

    public Task<VisualizationConfig> OptimizeVisualizationForDataSizeAsync(VisualizationConfig config, int dataSize)
    {
        var optimizedConfig = new VisualizationConfig
        {
            Type = config.Type,
            Title = config.Title,
            XAxis = config.XAxis,
            YAxis = config.YAxis,
            Series = config.Series,
            Config = new Dictionary<string, object>(config.Config)
        };

        // Optimize based on data size
        if (dataSize > 10000)
        {
            // For large datasets, use aggregation and sampling
            optimizedConfig.Config["enableDataSampling"] = true;
            optimizedConfig.Config["sampleSize"] = 5000;
            optimizedConfig.Config["aggregationLevel"] = "high";

            if (config.Type == "scatter")
            {
                // Switch to heatmap for large scatter plots
                optimizedConfig.Type = "heatmap";
                optimizedConfig.Config["binSize"] = 50;
            }
        }

        if (dataSize > 50000)
        {
            // For very large datasets, force table view with pagination
            optimizedConfig.Type = "table";
            optimizedConfig.Config["enablePagination"] = true;
            optimizedConfig.Config["pageSize"] = 100;
            optimizedConfig.Config["enableVirtualScrolling"] = true;
        }

        // Disable animations for large datasets
        optimizedConfig.EnableAnimation = dataSize <= 1000;

        return Task.FromResult(optimizedConfig);
    }

    public bool IsVisualizationSuitableForData(string visualizationType, ColumnMetadata[] columns, int rowCount)
    {
        var dataAnalysis = AnalyzeDataCharacteristics(columns, Array.Empty<object>());
        dataAnalysis.RowCount = rowCount;

        return visualizationType.ToLowerInvariant() switch
        {
            "pie" => dataAnalysis.CategoricalColumns.Any() && dataAnalysis.NumericColumns.Any() && rowCount <= 10,
            "scatter" => dataAnalysis.NumericColumns.Count >= 2 && rowCount <= 10000,
            "heatmap" => dataAnalysis.NumericColumns.Count >= 2 && rowCount <= 50000,
            "line" => dataAnalysis.HasTimeColumn && dataAnalysis.NumericColumns.Any(),
            "bar" => dataAnalysis.CategoricalColumns.Any() && dataAnalysis.NumericColumns.Any() && rowCount <= 100,
            "table" => true, // Table is always suitable
            _ => true
        };
    }

    public async Task<InteractiveVisualizationConfig> GenerateInteractiveVisualizationAsync(string query, ColumnMetadata[] columns, object[] data)
    {
        var baseConfig = await GenerateVisualizationConfigAsync(query, columns, data);
        var dataAnalysis = AnalyzeDataCharacteristics(columns, data);

        var interactiveConfig = new InteractiveVisualizationConfig
        {
            BaseVisualization = baseConfig,
            InteractiveFeatures = GenerateInteractiveFeatures(baseConfig.Type, dataAnalysis),
            Filters = GenerateSmartFilters(columns, data),
            DrillDownOptions = GenerateDrillDownOptions(dataAnalysis),
            CrossFiltering = dataAnalysis.CategoricalColumns.Count > 1,
            RealTimeUpdates = false,
            ExportOptions = new[] { "PNG", "SVG", "PDF", "Excel", "CSV" }
        };

        return interactiveConfig;
    }

    public async Task<DashboardConfig> GenerateMultiChartDashboardAsync(string query, ColumnMetadata[] columns, object[] data)
    {
        try
        {
            var charts = await GenerateMultipleVisualizationOptionsAsync(query, columns, data);
            var dataAnalysis = AnalyzeDataCharacteristics(columns, data);

            var dashboardConfig = new DashboardConfig
            {
                Title = $"Dashboard: {ExtractQuerySubject(query)}",
                Description = $"Multi-chart analysis for: {query}",
                Charts = charts,
                Layout = GenerateDashboardLayout(charts.Length),
                GlobalFilters = GenerateGlobalFilters(dataAnalysis),
                RefreshInterval = null
            };

            return dashboardConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating dashboard");
            throw;
        }
    }

    // Advanced visualization methods
    public async Task<AdvancedVisualizationConfig> GenerateAdvancedVisualizationAsync(
        string query,
        ColumnMetadata[] columns,
        object[] data,
        VisualizationPreferences? preferences = null)
    {
        try
        {
            var baseConfig = await GenerateVisualizationConfigAsync(query, columns, data);
            var dataAnalysis = AnalyzeDataCharacteristics(columns, data);

            var advancedConfig = new AdvancedVisualizationConfig
            {
                Type = baseConfig.Type,
                Title = baseConfig.Title,
                XAxis = baseConfig.XAxis,
                YAxis = baseConfig.YAxis,
                Series = baseConfig.Series,
                Config = baseConfig.Config,
                ChartType = DetermineAdvancedChartType(baseConfig.Type),
                Animation = GenerateAnimationConfig(data.Length),
                Interaction = GenerateInteractionConfig(dataAnalysis),
                Theme = GenerateAdvancedThemeConfig(preferences?.Theme),
                DataProcessing = GenerateDataProcessingConfig(data.Length),
                Export = GenerateExportConfig(),
                Accessibility = GenerateAccessibilityConfig(),
                Performance = GeneratePerformanceConfig(data.Length)
            };

            return advancedConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating advanced visualization");
            throw;
        }
    }

    public async Task<AdvancedDashboardConfig> GenerateAdvancedDashboardAsync(
        string query,
        ColumnMetadata[] columns,
        object[] data,
        DashboardPreferences? preferences = null)
    {
        try
        {
            var charts = await GenerateMultipleVisualizationOptionsAsync(query, columns, data);
            var advancedCharts = new List<AdvancedVisualizationConfig>();

            foreach (var chart in charts)
            {
                var advancedChart = await GenerateAdvancedVisualizationAsync(query, columns, data, null);
                advancedCharts.Add(advancedChart);
            }

            var dashboardConfig = new AdvancedDashboardConfig
            {
                Title = preferences?.Title ?? $"Advanced Analysis: {ExtractQuerySubject(query)}",
                Description = $"Comprehensive dashboard generated from: {query}",
                Charts = advancedCharts.Cast<VisualizationConfig>().ToArray(),
                Layout = GenerateAdvancedDashboardLayout(advancedCharts.Count, preferences),
                GlobalFilters = new FilterConfig[0],
                RefreshInterval = preferences?.RefreshInterval
            };

            return dashboardConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating advanced dashboard");
            throw;
        }
    }

    public Task<VisualizationRecommendation[]> GetVisualizationRecommendationsAsync(
        ColumnMetadata[] columns,
        object[] data,
        string? context = null)
    {
        try
        {
            var dataAnalysis = AnalyzeDataCharacteristics(columns, data);
            var recommendations = new List<VisualizationRecommendation>();

            if (dataAnalysis.NumericColumns.Count >= 2)
            {
                recommendations.Add(new VisualizationRecommendation
                {
                    ChartType = AdvancedChartType.Scatter,
                    Confidence = 0.85,
                    Reasoning = "Multiple numeric columns detected",
                    BestFor = "Correlation analysis",
                    Limitations = new[] { "May be cluttered with large datasets" },
                    EstimatedPerformance = new PerformanceEstimate
                    {
                        EstimatedRenderTime = TimeSpan.FromMilliseconds(100),
                        MemoryUsageMB = 50,
                        RequiresWebGL = false,
                        RequiresSampling = false,
                        RecommendedMaxDataPoints = 10000
                    },
                    SuggestedConfig = new Dictionary<string, object> { ["enableRegression"] = true }
                });
            }

            return Task.FromResult(recommendations.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendations");
            return Task.FromResult(Array.Empty<VisualizationRecommendation>());
        }
    }

    public Task<AdvancedVisualizationConfig> OptimizeVisualizationForPerformanceAsync(
        AdvancedVisualizationConfig config,
        int dataSize)
    {
        try
        {
            var optimizedConfig = new AdvancedVisualizationConfig
            {
                Type = config.Type,
                Title = config.Title,
                XAxis = config.XAxis,
                YAxis = config.YAxis,
                Series = config.Series,
                Config = new Dictionary<string, object>(config.Config),
                ChartType = config.ChartType,
                Animation = config.Animation,
                Interaction = config.Interaction,
                Theme = config.Theme,
                DataProcessing = config.DataProcessing,
                Export = config.Export,
                Accessibility = config.Accessibility,
                Performance = config.Performance
            };

            if (dataSize > 10000)
            {
                optimizedConfig.Performance!.EnableWebGL = true;
                optimizedConfig.DataProcessing!.EnableSampling = true;
                optimizedConfig.DataProcessing.SampleSize = 5000;
            }

            return Task.FromResult(optimizedConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing visualization");
            return Task.FromResult(config);
        }
    }

    // Helper methods that are missing - need to add them
    private DataCharacteristics AnalyzeDataCharacteristics(ColumnMetadata[] columns, object[] data)
    {
        var analysis = new DataCharacteristics
        {
            RowCount = data.Length,
            NumericColumns = new List<ColumnMetadata>(),
            CategoricalColumns = new List<ColumnMetadata>(),
            DateTimeColumns = new List<ColumnMetadata>(),
            HasTimeColumn = false
        };

        foreach (var column in columns)
        {
            switch (column.DataType.ToLowerInvariant())
            {
                case "int":
                case "decimal":
                case "float":
                case "double":
                case "money":
                case "numeric":
                    analysis.NumericColumns.Add(column);
                    break;
                case "datetime":
                case "date":
                case "time":
                case "timestamp":
                    analysis.DateTimeColumns.Add(column);
                    analysis.HasTimeColumn = true;
                    break;
                default:
                    analysis.CategoricalColumns.Add(column);
                    break;
            }
        }

        return analysis;
    }

    private string DetermineOptimalVisualizationType(DataCharacteristics analysis)
    {
        if (analysis.HasTimeColumn && analysis.NumericColumns.Any())
            return "line";

        if (analysis.NumericColumns.Count >= 2)
            return "scatter";

        if (analysis.CategoricalColumns.Any() && analysis.NumericColumns.Any())
            return "bar";

        return "table";
    }

    private string GenerateTitle(string query, DataCharacteristics analysis)
    {
        return $"Analysis: {ExtractQuerySubject(query)}";
    }

    private string DetermineXAxis(ColumnMetadata[] columns, DataCharacteristics analysis)
    {
        var xColumn = analysis.HasTimeColumn ?
            analysis.DateTimeColumns.First() :
            analysis.CategoricalColumns.FirstOrDefault() ?? columns.First();

        return xColumn.Name;
    }

    private string DetermineYAxis(ColumnMetadata[] columns, DataCharacteristics analysis)
    {
        var yColumn = analysis.NumericColumns.FirstOrDefault() ?? columns.Last();
        return yColumn.Name;
    }

    private string[] DetermineSeries(ColumnMetadata[] columns, DataCharacteristics analysis)
    {
        var series = new List<string>();

        foreach (var numericColumn in analysis.NumericColumns.Take(3))
        {
            series.Add(numericColumn.Name);
        }

        if (!series.Any())
        {
            series.Add(columns.First().Name);
        }

        return series.ToArray();
    }

    private Dictionary<string, object> MergeConfigurations(Dictionary<string, object> defaultConfig, Dictionary<string, object> aiConfig)
    {
        var merged = new Dictionary<string, object>(defaultConfig);
        foreach (var kvp in aiConfig)
        {
            merged[kvp.Key] = kvp.Value;
        }
        return merged;
    }

    private Dictionary<string, object> GetDefaultConfig(string visualizationType)
    {
        return new Dictionary<string, object>
        {
            ["responsive"] = true,
            ["maintainAspectRatio"] = false,
            ["plugins"] = new Dictionary<string, object>
            {
                ["legend"] = new { display = true },
                ["tooltip"] = new { enabled = true }
            }
        };
    }

    private Dictionary<string, object> ParseAIVisualizationConfig(string aiConfig)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(aiConfig) ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    private double CalculateConfidence(DataCharacteristics analysis, string recommendedType)
    {
        return 0.85; // Simplified confidence calculation
    }

    private VisualizationConfig GetFallbackVisualizationConfig(string query, ColumnMetadata[] columns, object[] data)
    {
        return new VisualizationConfig
        {
            Type = "table",
            Title = "Data View",
            XAxis = columns.FirstOrDefault()?.Name ?? "Index",
            YAxis = columns.LastOrDefault()?.Name ?? "Value",
            Series = columns.Take(3).Select(c => c.Name).ToArray(),
            Config = GetDefaultConfig("table")
        };
    }

    private VisualizationConfig CreateVisualizationConfig(string type, string title, ColumnMetadata[] columns, object[] data, DataCharacteristics analysis)
    {
        return new VisualizationConfig
        {
            Type = type,
            Title = title,
            XAxis = DetermineXAxis(columns, analysis),
            YAxis = DetermineYAxis(columns, analysis),
            Series = DetermineSeries(columns, analysis),
            Config = GetDefaultConfig(type)
        };
    }

    private Dictionary<string, object> GenerateInteractiveFeatures(string chartType, DataCharacteristics analysis)
    {
        var features = new Dictionary<string, object>
        {
            ["zoom"] = new { enabled = true, type = "xy" },
            ["tooltip"] = new { enabled = true, shared = true }
        };

        if (analysis.NumericColumns.Count >= 2)
        {
            features["brush"] = new { enabled = true, type = "selection" };
        }

        if (analysis.HasTimeColumn)
        {
            features["timeline"] = new { enabled = true, showNavigator = true };
        }

        return features;
    }

    private FilterConfig[] GenerateSmartFilters(ColumnMetadata[] columns, object[] data)
    {
        var filters = new List<FilterConfig>();

        foreach (var column in columns.Take(5))
        {
            filters.Add(new FilterConfig
            {
                ColumnName = column.Name,
                FilterType = column.DataType.ToLowerInvariant().Contains("date") ? "dateRange" : "text",
                Label = column.Name
            });
        }

        return filters.ToArray();
    }

    private DrillDownOption[] GenerateDrillDownOptions(DataCharacteristics analysis)
    {
        var options = new List<DrillDownOption>();

        foreach (var column in analysis.CategoricalColumns.Take(3))
        {
            options.Add(new DrillDownOption
            {
                Name = column.Name,
                Levels = new[] { column.Name },
                TargetColumn = column.Name
            });
        }

        return options.ToArray();
    }

    private DashboardLayout GenerateDashboardLayout(int chartCount)
    {
        return new DashboardLayout
        {
            Type = LayoutType.Grid,
            Rows = chartCount <= 2 ? 1 : 2,
            Columns = Math.Min(chartCount, 2),
            Configuration = new LayoutConfiguration
            {
                GridGap = 10,
                EnableDragAndDrop = true,
                EnableResize = true
            }
        };
    }

    private FilterConfig[] GenerateGlobalFilters(DataCharacteristics analysis)
    {
        var filters = new List<FilterConfig>();

        if (analysis.HasTimeColumn)
        {
            filters.Add(new FilterConfig
            {
                ColumnName = analysis.DateTimeColumns.First().Name,
                FilterType = "dateRange",
                Label = analysis.DateTimeColumns.First().Name
            });
        }

        return filters.ToArray();
    }

    private string ExtractQuerySubject(string query)
    {
        var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return words.Length > 2 ? string.Join(" ", words.Take(3)) : query;
    }

    // Advanced helper methods
    private AdvancedChartType DetermineAdvancedChartType(string basicType)
    {
        return basicType.ToLowerInvariant() switch
        {
            "bar" => AdvancedChartType.Bar,
            "line" => AdvancedChartType.Line,
            "pie" => AdvancedChartType.Pie,
            "scatter" => AdvancedChartType.Scatter,
            "area" => AdvancedChartType.Area,
            "heatmap" => AdvancedChartType.Heatmap,
            _ => AdvancedChartType.Bar
        };
    }

    private AnimationConfig GenerateAnimationConfig(int dataSize)
    {
        return new AnimationConfig
        {
            Duration = dataSize > 1000 ? 300 : 500,
            Easing = "easeInOut",
            Enabled = true,
            DelayByCategory = false,
            DelayIncrement = 0,
            AnimateOnDataChange = dataSize <= 5000,
            AnimatedProperties = new[] { "opacity", "transform" }
        };
    }

    private InteractionConfig GenerateInteractionConfig(DataCharacteristics analysis)
    {
        return new InteractionConfig
        {
            EnableZoom = true,
            EnablePan = analysis.NumericColumns.Count >= 2,
            EnableBrush = analysis.HasTimeColumn,
            EnableTooltip = true,
            EnableCrosshair = false,
            EnableLegendToggle = analysis.CategoricalColumns.Any(),
            EnableDataPointSelection = true,
            EnableDrillDown = analysis.CategoricalColumns.Any()
        };
    }

    private ThemeConfig GenerateAdvancedThemeConfig(string? themeName)
    {
        return new ThemeConfig
        {
            Name = themeName ?? "modern",
            DarkMode = false,
            Colors = new ColorPalette
            {
                Primary = new[] { "#007acc", "#0066cc", "#004499" },
                Secondary = new[] { "#6c757d", "#5a6268", "#495057" },
                Background = "#ffffff",
                Text = "#333333",
                Grid = "#e0e0e0",
                Axis = "#666666"
            }
        };
    }

    private DataProcessingConfig GenerateDataProcessingConfig(int dataSize)
    {
        return new DataProcessingConfig
        {
            EnableSampling = dataSize > 10000,
            SampleSize = Math.Min(dataSize, 5000),
            EnableAggregation = dataSize > 50000,
            Aggregation = new AggregationConfig
            {
                Method = "avg",
                GroupBy = "auto"
            }
        };
    }

    private ExportConfig GenerateExportConfig()
    {
        return new ExportConfig
        {
            SupportedFormats = new[] { "PNG", "SVG", "PDF", "Excel", "CSV" },
            ImageWidth = 1200,
            ImageHeight = 800,
            ImageDPI = 300,
            IncludeData = true,
            IncludeMetadata = true,
            DefaultFilename = "chart_export"
        };
    }

    private AccessibilityConfig GenerateAccessibilityConfig()
    {
        return new AccessibilityConfig
        {
            Enabled = true,
            HighContrast = false,
            ColorBlindFriendly = true,
            ScreenReaderSupport = true,
            KeyboardNavigation = true,
            AriaLabels = new[] { "Chart", "Data visualization" }
        };
    }

    private PerformanceConfig GeneratePerformanceConfig(int dataSize)
    {
        return new PerformanceConfig
        {
            EnableWebGL = dataSize > 10000,
            EnableVirtualization = dataSize > 50000,
            EnableCaching = true,
            EnableLazyLoading = dataSize > 1000,
            VirtualizationThreshold = 1000,
            CacheTTL = 300000, // 5 minutes
            MaxDataPoints = 100000
        };
    }

    private DashboardLayout GenerateAdvancedDashboardLayout(int chartCount, DashboardPreferences? preferences)
    {
        return new DashboardLayout
        {
            Type = LayoutType.Grid,
            Rows = chartCount <= 2 ? 1 : 2,
            Columns = Math.Min(chartCount, 2),
            Configuration = new LayoutConfiguration
            {
                GridGap = preferences?.GridGap ?? 10,
                EnableDragAndDrop = true,
                EnableResize = true
            }
        };
    }

    private ThemeConfig GenerateDashboardTheme(string? themeName)
    {
        return GenerateAdvancedThemeConfig(themeName);
    }

    private ResponsiveConfig GenerateResponsiveConfig()
    {
        return new ResponsiveConfig
        {
            Enabled = true,
            Breakpoints = new Dictionary<string, BreakpointConfig>
            {
                ["mobile"] = new BreakpointConfig { MinWidth = 0, MaxWidth = 768, Columns = 1, ChartSizes = new[] { "full" } },
                ["tablet"] = new BreakpointConfig { MinWidth = 769, MaxWidth = 1024, Columns = 2, ChartSizes = new[] { "half", "half" } },
                ["desktop"] = new BreakpointConfig { MinWidth = 1025, MaxWidth = 9999, Columns = 3, ChartSizes = new[] { "third", "third", "third" } }
            },
            AutoResize = true,
            MaintainAspectRatio = true
        };
    }

    private RealTimeConfig GenerateRealTimeConfig(bool enabled)
    {
        return new RealTimeConfig
        {
            Enabled = enabled,
            RefreshInterval = enabled ? 30 : 0,
            AutoRefresh = enabled,
            ShowLastUpdated = true,
            EnableNotifications = enabled,
            Alerts = Array.Empty<AlertConfig>()
        };
    }

    private CollaborationConfig GenerateCollaborationConfig(bool enabled)
    {
        return new CollaborationConfig
        {
            Enabled = enabled,
            AllowComments = enabled,
            AllowSharing = enabled,
            AllowEditing = enabled,
            SharedWith = Array.Empty<string>(),
            Permission = "read"
        };
    }

    private SecurityConfig GenerateSecurityConfig()
    {
        return new SecurityConfig
        {
            RequireAuthentication = true,
            AllowedRoles = new[] { "User", "Admin" },
            EnableRowLevelSecurity = false,
            DataFilters = new Dictionary<string, string>(),
            EnableAuditLog = true
        };
    }
}

public class DataCharacteristics
{
    public int RowCount { get; set; }
    public List<ColumnMetadata> NumericColumns { get; set; } = new();
    public List<ColumnMetadata> CategoricalColumns { get; set; } = new();
    public List<ColumnMetadata> DateTimeColumns { get; set; } = new();
    public bool HasTimeColumn { get; set; }
}