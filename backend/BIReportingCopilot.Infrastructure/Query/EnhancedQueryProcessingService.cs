using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.Models.ProcessFlow;
using BIReportingCopilot.Infrastructure.AI.Core;
using BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.Query;

/// <summary>
/// Enhanced query processing service with comprehensive ProcessFlow tracking
/// </summary>
public class EnhancedQueryProcessingService : IEnhancedQueryProcessingService
{
    private readonly ILogger<EnhancedQueryProcessingService> _logger;
    private readonly ProcessFlowTracker _processFlowTracker;
    private readonly IEnhancedBusinessContextAnalyzer _businessContextAnalyzer;
    private readonly IBusinessMetadataRetrievalService _metadataService;
    private readonly IForeignKeyRelationshipService _relationshipService;
    private readonly ISqlJoinGeneratorService _joinGeneratorService;
    private readonly ISqlDateFilterService _dateFilterService;
    private readonly ISqlAggregationService _aggregationService;

    public EnhancedQueryProcessingService(
        ILogger<EnhancedQueryProcessingService> logger,
        ProcessFlowTracker processFlowTracker,
        IEnhancedBusinessContextAnalyzer businessContextAnalyzer,
        IBusinessMetadataRetrievalService metadataService,
        IForeignKeyRelationshipService relationshipService,
        ISqlJoinGeneratorService joinGeneratorService,
        ISqlDateFilterService dateFilterService,
        ISqlAggregationService aggregationService)
    {
        _logger = logger;
        _processFlowTracker = processFlowTracker;
        _businessContextAnalyzer = businessContextAnalyzer;
        _metadataService = metadataService;
        _relationshipService = relationshipService;
        _joinGeneratorService = joinGeneratorService;
        _dateFilterService = dateFilterService;
        _aggregationService = aggregationService;
    }

    /// <summary>
    /// Process a query with comprehensive tracking and enhanced SQL generation
    /// </summary>
    public async Task<EnhancedQueryResult> ProcessQueryAsync(
        string userQuestion,
        string userId,
        string? traceId = null)
    {
        traceId ??= Guid.NewGuid().ToString("N")[..8];
        
        try
        {
            _logger.LogInformation("🚀 [ENHANCED-QUERY] Starting enhanced query processing [TraceId: {TraceId}]", traceId);

            // Start ProcessFlow session
            var sessionId = await _processFlowTracker.StartSessionAsync(userId, userQuestion, "enhanced-v2", null, traceId);

            // Step 1: Business Context Analysis
            var businessProfile = await _processFlowTracker.TrackStepWithConfidenceAsync(
                ProcessFlowSteps.SemanticAnalysis,
                async () => {
                    var result = await _businessContextAnalyzer.AnalyzeUserQuestionAsync(userQuestion, userId);
                    return (result, (decimal)result.ConfidenceScore);
                });

            // Step 2: Schema Retrieval
            var relevantMetadata = await _processFlowTracker.TrackStepAsync(
                ProcessFlowSteps.SchemaRetrieval,
                async () => {
                    var metadata = await _metadataService.GetRelevantBusinessMetadataAsync(businessProfile, 10);
                    await _processFlowTracker.SetStepOutputAsync(ProcessFlowSteps.SchemaRetrieval, new {
                        TableCount = metadata.RelevantTables.Count,
                        ColumnCount = metadata.TableColumns.Values.SelectMany(cols => cols).Count(),
                        RelationshipCount = metadata.TableRelationships.Count
                    });
                    return metadata;
                });

            // Step 3: Relationship Discovery
            var relationships = await _processFlowTracker.TrackStepAsync(
                ProcessFlowSteps.RelationshipDiscovery,
                async () => {
                    var tableNames = relevantMetadata.RelevantTables.Select(t => $"{t.SchemaName}.{t.TableName}").ToList();
                    var fkRelationships = await _relationshipService.GetRelationshipsForTablesAsync(tableNames);
                    
                    await _processFlowTracker.SetStepOutputAsync(ProcessFlowSteps.RelationshipDiscovery, new {
                        TableCount = tableNames.Count,
                        RelationshipCount = fkRelationships.Count,
                        ConnectedTables = fkRelationships.Select(r => new { r.ParentTable, r.ReferencedTable }).ToList()
                    });
                    
                    return fkRelationships;
                });

            // Step 4: JOIN Generation
            var joinResult = await _processFlowTracker.TrackStepAsync(
                ProcessFlowSteps.JoinGeneration,
                async () => {
                    var tableNames = relevantMetadata.RelevantTables.Select(t => $"{t.SchemaName}.{t.TableName}").ToList();
                    var result = await _joinGeneratorService.GenerateJoinsAsync(tableNames, strategy: JoinStrategy.Optimal);
                    
                    await _processFlowTracker.SetStepOutputAsync(ProcessFlowSteps.JoinGeneration, new {
                        Success = result.Success,
                        JoinClauseLength = result.JoinClause.Length,
                        TableAliasCount = result.TableAliases.Count,
                        JoinPathCount = result.JoinPaths.Count,
                        Strategy = result.JoinStrategy.ToString()
                    });
                    
                    return result;
                });

            // Step 5: Date Filter Generation
            var dateFilterResult = await _processFlowTracker.TrackStepAsync(
                ProcessFlowSteps.DateFilterGeneration,
                async () => {
                    var dateColumns = relevantMetadata.TableColumns.Values
                        .SelectMany(cols => cols)
                        .Where(c => c.ColumnName.ToLower().Contains("date") || c.ColumnName.ToLower().Contains("time"))
                        .Select(c => c.ColumnName)
                        .ToList();
                    
                    var result = await _dateFilterService.GenerateDateFilterAsync(
                        businessProfile.TimeContext, 
                        dateColumns, 
                        DateFilterStrategy.Optimal);
                    
                    await _processFlowTracker.SetStepOutputAsync(ProcessFlowSteps.DateFilterGeneration, new {
                        Success = result.Success,
                        WhereClauseLength = result.WhereClause.Length,
                        DateColumnCount = result.DateColumns.Count,
                        Strategy = result.Strategy.ToString(),
                        TimeRange = businessProfile.TimeContext?.RelativeExpression
                    });
                    
                    return result;
                });

            // Step 6: Aggregation Generation
            var aggregationResult = await _processFlowTracker.TrackStepAsync(
                ProcessFlowSteps.AggregationGeneration,
                async () => {
                    var availableColumns = relevantMetadata.TableColumns.Values
                        .SelectMany(cols => cols)
                        .Select(c => c.ColumnName)
                        .ToList();
                    var result = await _aggregationService.GenerateAggregationAsync(
                        businessProfile, 
                        availableColumns, 
                        AggregationStrategy.Optimal);
                    
                    await _processFlowTracker.SetStepOutputAsync(ProcessFlowSteps.AggregationGeneration, new {
                        Success = result.Success,
                        MetricCount = result.Metrics.Count,
                        DimensionCount = result.Dimensions.Count,
                        SelectClauseLength = result.SelectClause.Length,
                        Strategy = result.Strategy.ToString()
                    });
                    
                    return result;
                });

            // Step 7: Enhanced SQL Assembly
            var enhancedSql = await _processFlowTracker.TrackStepAsync(
                ProcessFlowSteps.PromptBuilding,
                async () => {
                    var sql = AssembleEnhancedSql(aggregationResult, joinResult, dateFilterResult);
                    
                    await _processFlowTracker.SetStepOutputAsync(ProcessFlowSteps.PromptBuilding, new {
                        GeneratedSQLLength = sql.Length,
                        HasJoins = !string.IsNullOrEmpty(joinResult.JoinClause),
                        HasDateFilter = !string.IsNullOrEmpty(dateFilterResult.WhereClause),
                        HasAggregation = !string.IsNullOrEmpty(aggregationResult.GroupByClause)
                    });
                    
                    return sql;
                });

            // Calculate overall confidence
            var overallConfidence = CalculateOverallConfidence(businessProfile, joinResult, dateFilterResult, aggregationResult);

            // Complete ProcessFlow session
            await _processFlowTracker.CompleteSessionAsync(
                ProcessFlowStatus.Completed,
                generatedSQL: enhancedSql,
                executionResult: "Enhanced query processing completed successfully",
                overallConfidence: (decimal)overallConfidence
            );

            var result = new EnhancedQueryResult
            {
                Success = true,
                GeneratedSql = enhancedSql,
                BusinessProfile = businessProfile,
                JoinResult = joinResult,
                DateFilterResult = dateFilterResult,
                AggregationResult = aggregationResult,
                OverallConfidence = overallConfidence,
                ProcessingMetadata = new Dictionary<string, object>
                {
                    ["SessionId"] = sessionId,
                    ["TraceId"] = traceId,
                    ["TableCount"] = relevantMetadata.RelevantTables.Count,
                    ["RelationshipCount"] = relationships.Count,
                    ["ProcessingSteps"] = 7
                }
            };

            _logger.LogInformation("✅ [ENHANCED-QUERY] Enhanced query processing completed successfully [TraceId: {TraceId}]", traceId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [ENHANCED-QUERY] Error in enhanced query processing [TraceId: {TraceId}]", traceId);
            
            await _processFlowTracker.CompleteSessionAsync(
                ProcessFlowStatus.Error,
                generatedSQL: null,
                executionResult: $"Error: {ex.Message}",
                overallConfidence: 0m
            );

            return new EnhancedQueryResult
            {
                Success = false,
                Error = ex.Message,
                GeneratedSql = string.Empty,
                OverallConfidence = 0.0
            };
        }
    }

    #region Private Helper Methods

    private string AssembleEnhancedSql(
        SqlAggregationResult aggregationResult,
        SqlJoinResult joinResult,
        SqlDateFilterResult dateFilterResult)
    {
        var sqlParts = new List<string>();

        // SELECT clause
        if (aggregationResult.Success && !string.IsNullOrEmpty(aggregationResult.SelectClause))
        {
            sqlParts.Add(aggregationResult.SelectClause);
        }
        else
        {
            sqlParts.Add("SELECT TOP 100 *");
        }

        // FROM and JOIN clauses
        if (joinResult.Success && !string.IsNullOrEmpty(joinResult.JoinClause))
        {
            sqlParts.Add(joinResult.JoinClause);
        }

        // WHERE clause
        var whereConditions = new List<string>();
        if (dateFilterResult.Success && !string.IsNullOrEmpty(dateFilterResult.WhereClause))
        {
            whereConditions.Add(dateFilterResult.WhereClause);
        }

        if (whereConditions.Any())
        {
            sqlParts.Add("WHERE " + string.Join(" AND ", whereConditions));
        }

        // GROUP BY clause
        if (aggregationResult.Success && !string.IsNullOrEmpty(aggregationResult.GroupByClause))
        {
            sqlParts.Add(aggregationResult.GroupByClause);
        }

        // ORDER BY clause
        if (aggregationResult.Success && !string.IsNullOrEmpty(aggregationResult.OrderByClause))
        {
            sqlParts.Add(aggregationResult.OrderByClause);
        }

        return string.Join("\n", sqlParts);
    }

    private double CalculateOverallConfidence(
        BusinessContextProfile businessProfile,
        SqlJoinResult joinResult,
        SqlDateFilterResult dateFilterResult,
        SqlAggregationResult aggregationResult)
    {
        var confidenceFactors = new List<double>
        {
            businessProfile.ConfidenceScore,
            joinResult.Success ? 0.9 : 0.3,
            dateFilterResult.Success ? 0.8 : 0.5,
            aggregationResult.Success ? 0.8 : 0.5
        };

        return confidenceFactors.Average();
    }

    #endregion
}
