using MediatR;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Interfaces;

namespace BIReportingCopilot.Infrastructure.Handlers;

/// <summary>
/// Command handler for enhanced semantic cache lookup using vector embeddings
/// </summary>
public class GetSemanticCacheCommandHandler : IRequestHandler<GetSemanticCacheCommand, SemanticCacheResult?>
{
    private readonly ILogger<GetSemanticCacheCommandHandler> _logger;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly ISemanticCacheService _semanticCacheService;

    public GetSemanticCacheCommandHandler(
        ILogger<GetSemanticCacheCommandHandler> logger,
        IVectorSearchService vectorSearchService,
        ISemanticCacheService semanticCacheService)
    {
        _logger = logger;
        _vectorSearchService = vectorSearchService;
        _semanticCacheService = semanticCacheService;
    }

    public async Task<SemanticCacheResult?> Handle(GetSemanticCacheCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("🔍 Enhanced semantic cache lookup for query: {Query}", request.NaturalLanguageQuery);

            // Use enhanced semantic cache service for vector-based lookup
            var result = await _semanticCacheService.GetSemanticallySimilarAsync(
                request.NaturalLanguageQuery, 
                request.SqlQuery);

            if (result?.IsHit == true)
            {
                _logger.LogInformation("🎯 Enhanced semantic cache hit - Similarity: {Similarity:P2}", 
                    result.SimilarityScore);
            }
            else
            {
                _logger.LogDebug("🎯 Enhanced semantic cache miss");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in enhanced semantic cache lookup");
            return null;
        }
    }
}

/// <summary>
/// Command handler for storing queries in enhanced semantic cache
/// </summary>
public class StoreSemanticCacheCommandHandler : IRequestHandler<StoreSemanticCacheCommand, bool>
{
    private readonly ILogger<StoreSemanticCacheCommandHandler> _logger;
    private readonly ISemanticCacheService _semanticCacheService;

    public StoreSemanticCacheCommandHandler(
        ILogger<StoreSemanticCacheCommandHandler> logger,
        ISemanticCacheService semanticCacheService)
    {
        _logger = logger;
        _semanticCacheService = semanticCacheService;
    }

    public async Task<bool> Handle(StoreSemanticCacheCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("💾 Storing query in enhanced semantic cache: {Query}", request.NaturalLanguageQuery);

            await _semanticCacheService.CacheSemanticQueryAsync(
                request.NaturalLanguageQuery,
                request.SqlQuery,
                request.Response,
                request.Expiry);

            _logger.LogInformation("💾 Query stored in enhanced semantic cache successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error storing query in enhanced semantic cache");
            return false;
        }
    }
}

/// <summary>
/// Command handler for optimizing semantic cache performance
/// </summary>
public class OptimizeSemanticCacheCommandHandler : IRequestHandler<OptimizeSemanticCacheCommand, bool>
{
    private readonly ILogger<OptimizeSemanticCacheCommandHandler> _logger;
    private readonly IVectorSearchService _vectorSearchService;

    public OptimizeSemanticCacheCommandHandler(
        ILogger<OptimizeSemanticCacheCommandHandler> logger,
        IVectorSearchService vectorSearchService)
    {
        _logger = logger;
        _vectorSearchService = vectorSearchService;
    }

    public async Task<bool> Handle(OptimizeSemanticCacheCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔧 Optimizing enhanced semantic cache");

            await _vectorSearchService.OptimizeIndexAsync(cancellationToken);

            _logger.LogInformation("🔧 Enhanced semantic cache optimization completed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error optimizing enhanced semantic cache");
            return false;
        }
    }
}

/// <summary>
/// Query handler for getting semantic cache metrics
/// </summary>
public class GetSemanticCacheMetricsQueryHandler : IRequestHandler<GetSemanticCacheMetricsQuery, SemanticCacheMetrics>
{
    private readonly ILogger<GetSemanticCacheMetricsQueryHandler> _logger;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly ISemanticCacheService _semanticCacheService;

    public GetSemanticCacheMetricsQueryHandler(
        ILogger<GetSemanticCacheMetricsQueryHandler> logger,
        IVectorSearchService vectorSearchService,
        ISemanticCacheService semanticCacheService)
    {
        _logger = logger;
        _vectorSearchService = vectorSearchService;
        _semanticCacheService = semanticCacheService;
    }

    public async Task<SemanticCacheMetrics> Handle(GetSemanticCacheMetricsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("📊 Getting enhanced semantic cache metrics");

            var vectorMetrics = await _vectorSearchService.GetMetricsAsync(cancellationToken);
            var cacheStats = await _semanticCacheService.GetCacheStatisticsAsync();

            var metrics = new SemanticCacheMetrics
            {
                TotalEmbeddings = vectorMetrics.TotalEmbeddings,
                TotalSearches = vectorMetrics.TotalSearches,
                AverageSearchTime = vectorMetrics.AverageSearchTime,
                CacheHitRate = vectorMetrics.CacheHitRate,
                IndexSizeBytes = vectorMetrics.IndexSizeBytes,
                LastOptimized = vectorMetrics.LastOptimized,
                TotalCacheEntries = cacheStats.TotalEntries,
                MemoryCacheHitRate = cacheStats.HitRate,
                PerformanceMetrics = vectorMetrics.PerformanceMetrics
            };

            _logger.LogDebug("📊 Retrieved semantic cache metrics - Embeddings: {Count}, Hit Rate: {HitRate:P2}",
                metrics.TotalEmbeddings, metrics.CacheHitRate);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting semantic cache metrics");
            return new SemanticCacheMetrics();
        }
    }
}

/// <summary>
/// Query for getting semantic cache metrics
/// </summary>
public class GetSemanticCacheMetricsQuery : IRequest<SemanticCacheMetrics>
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

/// <summary>
/// Enhanced semantic cache metrics
/// </summary>
public class SemanticCacheMetrics
{
    public int TotalEmbeddings { get; set; }
    public int TotalSearches { get; set; }
    public double AverageSearchTime { get; set; }
    public double CacheHitRate { get; set; }
    public long IndexSizeBytes { get; set; }
    public DateTime LastOptimized { get; set; }
    public int TotalCacheEntries { get; set; }
    public double MemoryCacheHitRate { get; set; }
    public Dictionary<string, object> PerformanceMetrics { get; set; } = new();
}
