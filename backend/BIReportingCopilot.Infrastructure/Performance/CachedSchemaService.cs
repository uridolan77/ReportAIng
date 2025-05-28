using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.Performance;

public class CachedSchemaService : ISchemaService
{
    private readonly ISchemaService _innerService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedSchemaService> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromHours(24);

    public CachedSchemaService(
        ISchemaService innerService,
        IDistributedCache cache,
        ILogger<CachedSchemaService> logger)
    {
        _innerService = innerService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<SchemaMetadata> GetSchemaMetadataAsync(string? dataSource = null)
    {
        var cacheKey = $"schema:metadata:{dataSource ?? "default"}";

        try
        {
            var cachedData = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedData))
            {
                _logger.LogDebug("Returning cached schema metadata");
                return JsonSerializer.Deserialize<SchemaMetadata>(cachedData)!;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving cached schema metadata");
        }

        var metadata = await _innerService.GetSchemaMetadataAsync(dataSource);

        try
        {
            var serialized = JsonSerializer.Serialize(metadata);
            await _cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _cacheExpiration
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error caching schema metadata");
        }

        return metadata;
    }

    public async Task<SchemaMetadata> RefreshSchemaMetadataAsync(string? dataSource = null)
    {
        var cacheKey = $"schema:metadata:{dataSource ?? "default"}";
        await _cache.RemoveAsync(cacheKey);
        return await GetSchemaMetadataAsync(dataSource);
    }

    public async Task<string> GetSchemaSummaryAsync(string? dataSource = null)
    {
        return await _innerService.GetSchemaSummaryAsync(dataSource);
    }

    public async Task<TableMetadata?> GetTableMetadataAsync(string tableName, string? schema = null)
    {
        return await _innerService.GetTableMetadataAsync(tableName, schema);
    }

    public async Task<List<SchemaSuggestion>> GetSchemaSuggestionsAsync(string userId)
    {
        return await _innerService.GetSchemaSuggestionsAsync(userId);
    }

    public async Task<bool> ValidateTableAccessAsync(string tableName, string userId)
    {
        return await _innerService.ValidateTableAccessAsync(tableName, userId);
    }

    public async Task<List<string>> GetAccessibleTablesAsync(string userId)
    {
        return await _innerService.GetAccessibleTablesAsync(userId);
    }

    public async Task UpdateTableStatisticsAsync(string tableName)
    {
        await _innerService.UpdateTableStatisticsAsync(tableName);
    }

    public async Task<DataQualityScore> AssessDataQualityAsync(string tableName)
    {
        return await _innerService.AssessDataQualityAsync(tableName);
    }

    // Note: GetAvailableDataSourcesAsync method removed as it's not in the interface
}
