using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.Visualization;

/// <summary>
/// Visualization service interface for creating and managing data visualizations
/// </summary>
public interface IVisualizationService
{
    Task<VisualizationResult> CreateVisualizationAsync(VisualizationRequest request, CancellationToken cancellationToken = default);
    Task<List<VisualizationRecommendation>> GetVisualizationRecommendationsAsync(object[] data, string[] columnNames, CancellationToken cancellationToken = default);
    Task<VisualizationConfiguration> OptimizeVisualizationAsync(VisualizationConfiguration config, object[] data, CancellationToken cancellationToken = default);
    Task<bool> ValidateVisualizationConfigAsync(VisualizationConfiguration config, CancellationToken cancellationToken = default);
    Task<List<VisualizationTemplate>> GetVisualizationTemplatesAsync(string? category = null, CancellationToken cancellationToken = default);
    Task<VisualizationTemplate> CreateVisualizationTemplateAsync(VisualizationTemplate template, CancellationToken cancellationToken = default);
    Task<bool> UpdateVisualizationTemplateAsync(VisualizationTemplate template, CancellationToken cancellationToken = default);
    Task<bool> DeleteVisualizationTemplateAsync(string templateId, CancellationToken cancellationToken = default);
    Task<VisualizationExportResult> ExportVisualizationAsync(string visualizationId, string format, CancellationToken cancellationToken = default);
    Task<AIDataAnalysisResult> AnalyzeDataForVisualizationAsync(object[] data, string[] columnNames, CancellationToken cancellationToken = default);
}

/// <summary>
/// Visualization request
/// </summary>
public class VisualizationRequest
{
    public string ChartType { get; set; } = string.Empty;
    public object[] Data { get; set; } = Array.Empty<object>();
    public string[] ColumnNames { get; set; } = Array.Empty<string>();
    public VisualizationConfiguration Configuration { get; set; } = new();
    public string? UserId { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Visualization result
/// </summary>
public class VisualizationResult
{
    public string VisualizationId { get; set; } = Guid.NewGuid().ToString();
    public string ChartType { get; set; } = string.Empty;
    public VisualizationConfiguration Configuration { get; set; } = new();
    public object ChartData { get; set; } = new();
    public List<DataInsight> Insights { get; set; } = new();
    public VisualizationMetadata Metadata { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSuccessful { get; set; } = true;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Visualization template
/// </summary>
public class VisualizationTemplate
{
    public string TemplateId { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ChartType { get; set; } = string.Empty;
    public VisualizationConfiguration DefaultConfiguration { get; set; } = new();
    public List<string> RequiredColumns { get; set; } = new();
    public List<string> OptionalColumns { get; set; } = new();
    public string PreviewImageUrl { get; set; } = string.Empty;
    public bool IsPublic { get; set; } = true;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Visualization export result
/// </summary>
public class VisualizationExportResult
{
    public string ExportId { get; set; } = Guid.NewGuid().ToString();
    public string Format { get; set; } = string.Empty;
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;
    public bool IsSuccessful { get; set; } = true;
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Visualization metadata
/// </summary>
public class VisualizationMetadata
{
    public int DataPointCount { get; set; }
    public List<string> ColumnTypes { get; set; } = new();
    public Dictionary<string, object> Statistics { get; set; } = new();
    public TimeSpan RenderTime { get; set; }
    public string DataSource { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> CustomMetadata { get; set; } = new();
}
