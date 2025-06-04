using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Performance;
using BIReportingCopilot.Core.Interfaces;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Anomaly detection result
/// </summary>
public class AnomalyDetectionResult
{
    public List<Anomaly> Anomalies { get; set; } = new();
    public int TotalAnomalies { get; set; }
    public int HighSeverityCount { get; set; }
    public int MediumSeverityCount { get; set; }
    public int LowSeverityCount { get; set; }
    public List<AnomalyInsight> Insights { get; set; } = new();
    public List<AnomalyRecommendation> Recommendations { get; set; } = new();
    public int DetectionMethods { get; set; }
    public DateTime ProcessingTime { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Individual anomaly detection
/// </summary>
public class Anomaly
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public AnomalyType Type { get; set; }
    public AnomalySeverity Severity { get; set; }
    public double Confidence { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? AffectedColumn { get; set; }
    public List<int>? AffectedRows { get; set; }
    public object? ExpectedValue { get; set; }
    public object? ActualValue { get; set; }
    public string DetectionMethod { get; set; } = string.Empty;
    public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Anomaly insight
/// </summary>
public class AnomalyInsight
{
    public InsightType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> AffectedAnomalies { get; set; } = new();
}

/// <summary>
/// Anomaly recommendation
/// </summary>
public class AnomalyRecommendation
{
    public RecommendationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RecommendationPriority Priority { get; set; }
    public string EstimatedEffort { get; set; } = string.Empty;
    public List<string> AffectedAnomalies { get; set; } = new();
}

/// <summary>
/// Anomaly trend analysis
/// </summary>
public class AnomalyTrendAnalysis
{
    public TimeSpan Period { get; set; }
    public int TotalAnomalies { get; set; }
    public Dictionary<DateTime, int> AnomaliesByDay { get; set; } = new();
    public Dictionary<AnomalyType, int> AnomaliesByType { get; set; } = new();
    public Dictionary<AnomalySeverity, int> AnomaliesBySeverity { get; set; } = new();
    public TrendDirection TrendDirection { get; set; }
    public List<AnomalyTypeFrequency> MostCommonAnomalies { get; set; } = new();
    public List<string> RecommendedActions { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
}

/// <summary>
/// Anomaly type frequency
/// </summary>
public class AnomalyTypeFrequency
{
    public AnomalyType Type { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Anomaly detection configuration
/// </summary>
public class AnomalyDetectionConfiguration
{
    public StatisticalThresholds? StatisticalThresholds { get; set; }
    public TemporalParameters? TemporalParameters { get; set; }
    public List<BusinessRule>? BusinessRules { get; set; }
    public PatternDetectionSettings? PatternSettings { get; set; }
}

/// <summary>
/// Statistical thresholds for anomaly detection
/// </summary>
public class StatisticalThresholds
{
    public double ZScoreThreshold { get; set; } = 3.0;
    public double IQRMultiplier { get; set; } = 1.5;
    public double PercentileThreshold { get; set; } = 0.95;
    public int MinimumSampleSize { get; set; } = 30;
}

/// <summary>
/// Temporal detection parameters
/// </summary>
public class TemporalParameters
{
    public int SeasonalityPeriod { get; set; } = 7; // Days
    public double TrendThreshold { get; set; } = 0.2;
    public int MovingAverageWindow { get; set; } = 7;
    public double VolatilityThreshold { get; set; } = 0.3;
}

/// <summary>
/// Business rule for anomaly detection
/// </summary>
public class BusinessRule
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;
    public AnomalySeverity Severity { get; set; }
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Pattern detection settings
/// </summary>
public class PatternDetectionSettings
{
    public double SimilarityThreshold { get; set; } = 0.8;
    public int MinimumPatternLength { get; set; } = 3;
    public bool EnableSequenceDetection { get; set; } = true;
    public bool EnableFrequencyAnalysis { get; set; } = true;
}

/// <summary>
/// Anomaly configuration
/// </summary>
public class AnomalyConfiguration
{
    public bool EnableStatisticalDetection { get; set; } = true;
    public bool EnableTemporalDetection { get; set; } = true;
    public bool EnablePatternDetection { get; set; } = true;
    public bool EnableBusinessRuleDetection { get; set; } = true;
    public double MinimumConfidenceThreshold { get; set; } = 0.7;
    public int MaxAnomaliesPerQuery { get; set; } = 50;
    public bool EnableRealTimeAlerts { get; set; } = true;
    public TimeSpan AlertCooldownPeriod { get; set; } = TimeSpan.FromMinutes(15);
}

/// <summary>
/// Anomaly model metadata
/// </summary>
public class AnomalyModelMetadata
{
    public int TrainingDataCount { get; set; }
    public DateTime LastTrainingDate { get; set; }
    public string ModelVersion { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public Dictionary<string, object> ModelParameters { get; set; } = new();
}

/// <summary>
/// Enums for anomaly detection
/// </summary>
public enum AnomalyType
{
    Statistical,
    Temporal,
    Pattern,
    BusinessRule,
    Outlier,
    Trend,
    Seasonal
}

public enum AnomalySeverity
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum TrendDirection
{
    Increasing,
    Decreasing,
    Stable
}

public enum RecommendationType
{
    Investigation,
    Monitoring,
    Action,
    Prevention
}

public enum RecommendationPriority
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Statistical anomaly detector
/// </summary>
public class StatisticalAnomalyDetector
{
    private readonly ILogger _logger;
    private readonly AnomalyConfiguration _config;
    private StatisticalThresholds _thresholds;

    public StatisticalAnomalyDetector(ILogger logger, AnomalyConfiguration config)
    {
        _logger = logger;
        _config = config;
        _thresholds = new StatisticalThresholds();
    }

    public async Task<List<Anomaly>> DetectAnomaliesAsync(QueryResult queryResult, SemanticAnalysis semanticAnalysis)
    {
        var anomalies = new List<Anomaly>();

        try
        {
            if (queryResult.Data == null || !queryResult.Data.Any()) return anomalies;

            // Detect anomalies in numeric columns
            for (int colIndex = 0; colIndex < (queryResult.Columns?.Count ?? 0); colIndex++)
            {
                var column = queryResult.Columns![colIndex];
                if (IsNumericColumn(column))
                {
                    var columnAnomalies = await DetectColumnAnomaliesAsync(queryResult, colIndex, column);
                    anomalies.AddRange(columnAnomalies);
                }
            }

            _logger.LogDebug("Statistical detector found {AnomalyCount} anomalies", anomalies.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in statistical anomaly detection");
        }

        return anomalies;
    }

    public async Task UpdateThresholdsAsync(StatisticalThresholds thresholds)
    {
        _thresholds = thresholds;
        _logger.LogDebug("Updated statistical thresholds");
    }

    public async Task TrainAsync(List<QueryResult> historicalData)
    {
        // Train statistical models with historical data
        _logger.LogDebug("Training statistical anomaly detector with {DataCount} samples", historicalData.Count);
    }

    private async Task<List<Anomaly>> DetectColumnAnomaliesAsync(QueryResult queryResult, int columnIndex, ColumnMetadata column)
    {
        var anomalies = new List<Anomaly>();
        var values = ExtractNumericValues(queryResult.Data!, columnIndex);

        if (values.Count < _thresholds.MinimumSampleSize) return anomalies;

        // Z-Score based detection
        var zScoreAnomalies = DetectZScoreAnomalies(values, columnIndex, column.Name);
        anomalies.AddRange(zScoreAnomalies);

        // IQR based detection
        var iqrAnomalies = DetectIQRAnomalies(values, columnIndex, column.Name);
        anomalies.AddRange(iqrAnomalies);

        return anomalies;
    }

    private List<Anomaly> DetectZScoreAnomalies(List<double> values, int columnIndex, string columnName)
    {
        var anomalies = new List<Anomaly>();
        var mean = values.Average();
        var stdDev = CalculateStandardDeviation(values, mean);

        if (stdDev == 0) return anomalies;

        for (int i = 0; i < values.Count; i++)
        {
            var zScore = Math.Abs((values[i] - mean) / stdDev);
            if (zScore > _thresholds.ZScoreThreshold)
            {
                anomalies.Add(new Anomaly
                {
                    Type = AnomalyType.Statistical,
                    Severity = zScore > 4 ? AnomalySeverity.High : AnomalySeverity.Medium,
                    Confidence = Math.Min(0.99, zScore / 5.0),
                    Description = $"Statistical outlier detected in {columnName} (Z-Score: {zScore:F2})",
                    AffectedColumn = columnName,
                    AffectedRows = new List<int> { i },
                    ExpectedValue = mean,
                    ActualValue = values[i],
                    DetectionMethod = "Z-Score",
                    Metadata = new Dictionary<string, object>
                    {
                        ["z_score"] = zScore,
                        ["mean"] = mean,
                        ["std_dev"] = stdDev
                    }
                });
            }
        }

        return anomalies;
    }

    private List<Anomaly> DetectIQRAnomalies(List<double> values, int columnIndex, string columnName)
    {
        var anomalies = new List<Anomaly>();
        var sortedValues = values.OrderBy(v => v).ToList();

        var q1 = CalculatePercentile(sortedValues, 0.25);
        var q3 = CalculatePercentile(sortedValues, 0.75);
        var iqr = q3 - q1;
        var lowerBound = q1 - (_thresholds.IQRMultiplier * iqr);
        var upperBound = q3 + (_thresholds.IQRMultiplier * iqr);

        for (int i = 0; i < values.Count; i++)
        {
            if (values[i] < lowerBound || values[i] > upperBound)
            {
                var distance = Math.Min(Math.Abs(values[i] - lowerBound), Math.Abs(values[i] - upperBound));
                var confidence = Math.Min(0.99, distance / (iqr * 2));

                anomalies.Add(new Anomaly
                {
                    Type = AnomalyType.Outlier,
                    Severity = confidence > 0.8 ? AnomalySeverity.High : AnomalySeverity.Medium,
                    Confidence = confidence,
                    Description = $"IQR outlier detected in {columnName}",
                    AffectedColumn = columnName,
                    AffectedRows = new List<int> { i },
                    ExpectedValue = $"[{lowerBound:F2}, {upperBound:F2}]",
                    ActualValue = values[i],
                    DetectionMethod = "IQR",
                    Metadata = new Dictionary<string, object>
                    {
                        ["q1"] = q1,
                        ["q3"] = q3,
                        ["iqr"] = iqr,
                        ["lower_bound"] = lowerBound,
                        ["upper_bound"] = upperBound
                    }
                });
            }
        }

        return anomalies;
    }

    private bool IsNumericColumn(ColumnMetadata column)
    {
        var numericTypes = new[] { "int", "decimal", "float", "double", "money", "numeric", "bigint" };
        return numericTypes.Any(type => column.DataType.ToLowerInvariant().Contains(type));
    }

    private List<double> ExtractNumericValues(List<List<object?>> data, int columnIndex)
    {
        var values = new List<double>();

        foreach (var row in data)
        {
            if (columnIndex < row.Count && row[columnIndex] != null)
            {
                if (double.TryParse(row[columnIndex]!.ToString(), out var value))
                {
                    values.Add(value);
                }
            }
        }

        return values;
    }

    private double CalculateStandardDeviation(List<double> values, double mean)
    {
        var sumOfSquares = values.Sum(v => Math.Pow(v - mean, 2));
        return Math.Sqrt(sumOfSquares / values.Count);
    }

    private double CalculatePercentile(List<double> sortedValues, double percentile)
    {
        var index = percentile * (sortedValues.Count - 1);
        var lower = (int)Math.Floor(index);
        var upper = (int)Math.Ceiling(index);

        if (lower == upper) return sortedValues[lower];

        var weight = index - lower;
        return sortedValues[lower] * (1 - weight) + sortedValues[upper] * weight;
    }
}

/// <summary>
/// Temporal anomaly detector
/// </summary>
public class TemporalAnomalyDetector
{
    private readonly ILogger _logger;
    private readonly AnomalyConfiguration _config;
    private TemporalParameters _parameters;

    public TemporalAnomalyDetector(ILogger logger, AnomalyConfiguration config)
    {
        _logger = logger;
        _config = config;
        _parameters = new TemporalParameters();
    }

    public async Task<List<Anomaly>> DetectAnomaliesAsync(QueryResult queryResult, SemanticAnalysis semanticAnalysis)
    {
        var anomalies = new List<Anomaly>();

        try
        {
            // Detect temporal anomalies if date/time columns are present
            var temporalColumns = FindTemporalColumns(queryResult.Columns);

            foreach (var (columnIndex, column) in temporalColumns)
            {
                var temporalAnomalies = await DetectTemporalColumnAnomaliesAsync(queryResult, columnIndex, column);
                anomalies.AddRange(temporalAnomalies);
            }

            _logger.LogDebug("Temporal detector found {AnomalyCount} anomalies", anomalies.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in temporal anomaly detection");
        }

        return anomalies;
    }

    public async Task UpdateParametersAsync(TemporalParameters parameters)
    {
        _parameters = parameters;
        _logger.LogDebug("Updated temporal parameters");
    }

    public async Task TrainAsync(List<QueryResult> historicalData)
    {
        _logger.LogDebug("Training temporal anomaly detector with {DataCount} samples", historicalData.Count);
    }

    private List<(int Index, ColumnMetadata Column)> FindTemporalColumns(List<ColumnMetadata>? columns)
    {
        if (columns == null) return new List<(int, ColumnMetadata)>();

        var temporalColumns = new List<(int, ColumnMetadata)>();

        for (int i = 0; i < columns.Count; i++)
        {
            if (IsTemporalColumn(columns[i]))
            {
                temporalColumns.Add((i, columns[i]));
            }
        }

        return temporalColumns;
    }

    private bool IsTemporalColumn(ColumnMetadata column)
    {
        var temporalTypes = new[] { "datetime", "date", "time", "timestamp" };
        return temporalTypes.Any(type => column.DataType.ToLowerInvariant().Contains(type));
    }

    private async Task<List<Anomaly>> DetectTemporalColumnAnomaliesAsync(
        QueryResult queryResult,
        int columnIndex,
        ColumnMetadata column)
    {
        var anomalies = new List<Anomaly>();

        // Simple temporal anomaly detection
        // In a real implementation, this would include sophisticated time series analysis

        return anomalies;
    }
}

/// <summary>
/// Pattern anomaly detector
/// </summary>
public class PatternAnomalyDetector
{
    private readonly ILogger _logger;
    private readonly AnomalyConfiguration _config;

    public PatternAnomalyDetector(ILogger logger, AnomalyConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    public async Task<List<Anomaly>> DetectAnomaliesAsync(QueryResult queryResult, SemanticAnalysis semanticAnalysis)
    {
        var anomalies = new List<Anomaly>();

        try
        {
            // Detect pattern anomalies
            _logger.LogDebug("Pattern detector found {AnomalyCount} anomalies", anomalies.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in pattern anomaly detection");
        }

        return anomalies;
    }

    public async Task TrainAsync(List<QueryResult> historicalData)
    {
        _logger.LogDebug("Training pattern anomaly detector with {DataCount} samples", historicalData.Count);
    }
}

/// <summary>
/// Business rule anomaly detector
/// </summary>
public class BusinessRuleAnomalyDetector
{
    private readonly ILogger _logger;
    private readonly AnomalyConfiguration _config;
    private List<BusinessRule> _rules = new();

    public BusinessRuleAnomalyDetector(ILogger logger, AnomalyConfiguration config)
    {
        _logger = logger;
        _config = config;
        InitializeDefaultRules();
    }

    public async Task<List<Anomaly>> DetectAnomaliesAsync(QueryResult queryResult, SemanticAnalysis semanticAnalysis)
    {
        var anomalies = new List<Anomaly>();

        try
        {
            foreach (var rule in _rules.Where(r => r.IsEnabled))
            {
                var ruleAnomalies = await EvaluateBusinessRuleAsync(rule, queryResult);
                anomalies.AddRange(ruleAnomalies);
            }

            _logger.LogDebug("Business rule detector found {AnomalyCount} anomalies", anomalies.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in business rule anomaly detection");
        }

        return anomalies;
    }

    public async Task UpdateRulesAsync(List<BusinessRule> rules)
    {
        _rules = rules;
        _logger.LogDebug("Updated business rules: {RuleCount} rules", rules.Count);
    }

    private void InitializeDefaultRules()
    {
        _rules.Add(new BusinessRule
        {
            Name = "Negative Revenue",
            Description = "Revenue values should not be negative",
            Condition = "revenue < 0",
            Severity = AnomalySeverity.High
        });

        _rules.Add(new BusinessRule
        {
            Name = "Excessive Deposit Amount",
            Description = "Single deposit amounts over $10,000 require review",
            Condition = "deposit > 10000",
            Severity = AnomalySeverity.Medium
        });
    }

    private async Task<List<Anomaly>> EvaluateBusinessRuleAsync(BusinessRule rule, QueryResult queryResult)
    {
        var anomalies = new List<Anomaly>();

        // Simple rule evaluation - in practice would use a rule engine
        // For now, just return empty list

        return anomalies;
    }
}

/// <summary>
/// Anomaly alert manager
/// </summary>
public class AnomalyAlertManager
{
    private readonly ILogger _logger;
    private readonly ICacheService _cacheService;

    public AnomalyAlertManager(ILogger logger, ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public async Task SendAnomalyAlertAsync(Anomaly anomaly, string? userId)
    {
        try
        {
            // Check if alert was recently sent to avoid spam
            var alertKey = $"anomaly_alert:{anomaly.Type}:{anomaly.AffectedColumn}:{userId}";
            var recentAlert = await _cacheService.GetAsync<DateTime?>(alertKey);

            if (recentAlert.HasValue && DateTime.UtcNow - recentAlert.Value < TimeSpan.FromMinutes(15))
            {
                _logger.LogDebug("Skipping duplicate alert for anomaly {AnomalyId}", anomaly.Id);
                return;
            }

            // Send alert (in practice, this would integrate with notification systems)
            _logger.LogWarning("ANOMALY ALERT: {Severity} {Type} anomaly detected in {Column} - {Description}",
                anomaly.Severity, anomaly.Type, anomaly.AffectedColumn, anomaly.Description);

            // Record alert sent
            await _cacheService.SetAsync(alertKey, DateTime.UtcNow, TimeSpan.FromMinutes(30));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending anomaly alert");
        }
    }
}
