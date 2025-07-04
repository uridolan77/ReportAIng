using MediatR;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Business;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Infrastructure.Extensions;

namespace BIReportingCopilot.Infrastructure.Handlers.Query;

/// <summary>
/// Enhanced command handler for processing natural language queries
/// Orchestrates the complete query processing pipeline using CQRS pattern
/// </summary>
public class ProcessQueryCommandHandler : IRequestHandler<ProcessQueryCommand, QueryResponse>
{
    private readonly ILogger<ProcessQueryCommandHandler> _logger;
    private readonly IMediator _mediator;
    private readonly IAITuningSettingsService _settingsService;
    private readonly IQueryProgressNotifier? _progressNotifier;
    private readonly IServiceProvider _serviceProvider;

    public ProcessQueryCommandHandler(
        ILogger<ProcessQueryCommandHandler> logger,
        IMediator mediator,
        IAITuningSettingsService settingsService,
        IServiceProvider serviceProvider,
        IQueryProgressNotifier? progressNotifier = null)
    {
        _logger = logger;
        _mediator = mediator;
        _settingsService = settingsService;
        _serviceProvider = serviceProvider;
        _progressNotifier = progressNotifier;
    }

    public async Task<QueryResponse> Handle(ProcessQueryCommand request, CancellationToken cancellationToken)
    {
        var queryId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("💬 [CHAT-CQRS] ProcessQueryCommandHandler - Processing query {QueryId} for user {UserId}: {Question}",
                queryId, request.UserId, request.Question);

            // Check if enhanced context is available and valid
            var hasEnhancedContext = IsEnhancedContextValid(request);
            if (hasEnhancedContext)
            {
                _logger.LogInformation("🚀 [ENHANCED-CONTEXT] Using Enhanced Schema Contextualization System for query {QueryId} - Confidence: {Confidence:P2}, Tables: {TableCount}",
                    queryId, request.BusinessProfile?.ConfidenceScore ?? 0, request.SchemaMetadata?.RelevantTables.Count ?? 0);

                try
                {
                    return await ProcessWithEnhancedContextAsync(request, queryId, startTime, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ [ENHANCED-CONTEXT] Enhanced processing failed, falling back to basic pipeline for query {QueryId}", queryId);
                    // Continue with basic pipeline as fallback
                }
            }
            else
            {
                _logger.LogInformation("🔄 [BASIC-PIPELINE] Using basic pipeline for query {QueryId} - Reason: {Reason}",
                    queryId, GetFallbackReason(request));
            }

            // Step 1: Check cache first
            await NotifyProgress(request.UserId, queryId, "cache_check", "Checking query cache", 5);
            var cachedResult = await CheckCacheAsync(request, cancellationToken);
            if (cachedResult != null)
            {
                cachedResult.QueryId = queryId;
                return cachedResult;
            }

            // Step 2: Advanced NLU Analysis (Enhancement #2)
            await NotifyProgress(request.UserId, queryId, "nlu_analysis", "Performing advanced NLU analysis", 10);
            var nluResult = await _mediator.Send(new AnalyzeNLUCommand
            {
                NaturalLanguageQuery = request.Question,
                UserId = request.UserId,
                Context = new NLUAnalysisContext { SessionId = request.SessionId }
            }, cancellationToken);

            // Step 3: Get relevant schema
            await NotifyProgress(request.UserId, queryId, "schema_analysis", "Analyzing relevant schema", 20);
            var schema = await _mediator.Send(new GetRelevantSchemaQuery
            {
                Question = request.Question,
                UserId = request.UserId
            }, cancellationToken);

            // Step 4: Query Intelligence Analysis (Enhancement #2)
            await NotifyProgress(request.UserId, queryId, "query_intelligence", "Analyzing query intelligence", 25);
            var intelligenceResult = await _mediator.Send(new AnalyzeQueryIntelligenceCommand
            {
                NaturalLanguageQuery = request.Question,
                UserId = request.UserId,
                Schema = schema
            }, cancellationToken);

            // Step 5: Generate SQL with enhanced context
            await NotifyProgress(request.UserId, queryId, "ai_processing", "Generating optimized SQL with AI", 40);
            _logger.LogInformation("🔍 [PROCESS-QUERY] About to send GenerateSqlCommand for question: {Question}", request.Question);

            // Track template selection for analytics
            try
            {
                await _serviceProvider.TrackTemplateSelectionAsync(
                    "sql_generation",
                    "query_generation",
                    request.UserId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ [ANALYTICS] Failed to track template selection");
            }

            var sqlResult = await _mediator.Send(new GenerateSqlCommand
            {
                Question = request.Question,
                Schema = schema,
                UserId = request.UserId,
                SessionId = request.SessionId,
                ProviderId = request.Options.ProviderId,
                ModelId = request.Options.ModelId
            }, cancellationToken);
            _logger.LogInformation("🔍 [PROCESS-QUERY] GenerateSqlCommand completed. Success: {Success}, SQL: {Sql}", sqlResult.Success, sqlResult.Sql?.Substring(0, Math.Min(100, sqlResult.Sql?.Length ?? 0)) + "...");

            if (!sqlResult.Success)
            {
                return CreateErrorResponse(queryId, sqlResult.Error ?? "SQL generation failed", "", sqlResult.PromptDetails);
            }

            // Step 6: Schema Optimization Analysis (Enhancement #2)
            await NotifyProgress(request.UserId, queryId, "sql_optimization", "Analyzing SQL optimization opportunities", 50);
            var optimizationResult = await _mediator.Send(new OptimizeSqlCommand
            {
                OriginalSql = sqlResult.Sql,
                Schema = schema,
                Goals = new OptimizationGoals
                {
                    Goals = new List<OptimizationGoal> { OptimizationGoal.Performance, OptimizationGoal.Readability }
                }
            }, cancellationToken);

            // Use optimized SQL if available and confidence is high
            var finalSql = optimizationResult.ConfidenceScore > 0.8 ? optimizationResult.OptimizedSql : sqlResult.Sql;

            // Step 7: Validate SQL
            await NotifyProgress(request.UserId, queryId, "sql_validation", "Validating generated SQL", 65);
            var isValid = await _mediator.Send(new ValidateSqlQuery { Sql = finalSql }, cancellationToken);
            if (!isValid)
            {
                var error = "Generated SQL failed validation";
                await LogQueryAsync(request, finalSql, false, 0, error);
                return CreateErrorResponse(queryId, error, finalSql, sqlResult.PromptDetails);
            }

            // Step 8: Execute SQL
            await NotifyProgress(request.UserId, queryId, "sql_execution", "Executing optimized SQL query", 70);
            var queryResult = await _mediator.Send(new ExecuteSqlCommand
            {
                Sql = finalSql,
                Options = request.Options,
                UserId = request.UserId,
                CancellationToken = cancellationToken
            }, cancellationToken);

            if (!queryResult.IsSuccessful)
            {
                var error = GetErrorFromMetadata(queryResult.Metadata) ?? "SQL execution failed";
                await LogQueryAsync(request, sqlResult.Sql, false, 0, error);
                return CreateErrorResponse(queryId, error, sqlResult.Sql, sqlResult.PromptDetails);
            }

            // Step 6: Build successful response
            await NotifyProgress(request.UserId, queryId, "response_building", "Building response", 90);
            var totalExecutionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            // Convert SqlQueryResult to QueryResult
            var convertedResult = ConvertSqlQueryResultToQueryResult(queryResult);

            var response = new QueryResponse
            {
                QueryId = queryId,
                Sql = finalSql,
                Result = convertedResult,
                Confidence = sqlResult.Confidence,
                Success = true,
                Timestamp = DateTime.UtcNow,
                ExecutionTimeMs = totalExecutionTime,
                PromptDetails = sqlResult.PromptDetails,
                Cached = false
            };

            // Step 9: Real-time Streaming Event (Enhancement #3)
            await NotifyProgress(request.UserId, queryId, "streaming_analytics", "Processing real-time analytics", 92);
            try
            {
                await _mediator.Send(new ProcessQueryStreamEventCommand
                {
                    QueryEvent = new QueryStreamEvent
                    {
                        QueryId = queryId,
                        UserId = request.UserId,
                        Query = request.Question,
                        GeneratedSQL = finalSql,
                        Timestamp = DateTime.UtcNow,
                        ExecutionTimeMs = totalExecutionTime,
                        Success = queryResult.IsSuccessful,
                        Error = queryResult.IsSuccessful ? null : GetErrorFromMetadata(queryResult.Metadata),
                        Metrics = new Core.Models.QueryMetrics
                        {
                            ExecutionTimeMs = totalExecutionTime,
                            RowCount = GetRowCountFromMetadata(queryResult.Metadata),
                            CacheHit = response.Cached,
                            NLUConfidence = response.Confidence,
                            QueryIntelligenceScore = response.Confidence,
                            OptimizationScore = optimizationResult?.ConfidenceScore ?? 0.0
                        },
                        Context = new Dictionary<string, object>
                        {
                            ["cache_hit"] = response.Cached,
                            ["optimization_applied"] = optimizationResult?.ConfidenceScore > 0.8
                        }
                    }
                }, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing real-time streaming event");
            }

            // Step 10: Cache result if enabled
            await CacheResultIfEnabled(request, response, cancellationToken);

            // Step 11: Log successful query
            await LogQueryAsync(request, finalSql, true, totalExecutionTime);

            // Step 12: Track template usage for analytics
            try
            {
                await _serviceProvider.TrackTemplateUsageFromQueryResponseAsync(response, sqlResult.PromptDetails, request.UserId);
            }
            catch (Exception trackingEx)
            {
                _logger.LogWarning(trackingEx, "⚠️ Failed to track template usage from query response");
            }

            await NotifyProgress(request.UserId, queryId, "completed", "Query completed successfully", 100);

            _logger.LogInformation("✅ Query completed successfully - QueryId: {QueryId}, ExecutionTime: {ExecutionTime}ms",
                queryId, totalExecutionTime);

            return response;
        }
        catch (Exception ex)
        {
            var errorExecutionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "❌ Query processing failed - QueryId: {QueryId}, Error: {Error}", queryId, ex.Message);

            await NotifyProgress(request.UserId, queryId, "error", $"Query processing failed: {ex.Message}", 0);
            await LogQueryAsync(request, "", false, errorExecutionTime, ex.Message);

            return CreateErrorResponse(queryId, ex.Message, "", null);
        }
    }

    private async Task<QueryResponse?> CheckCacheAsync(ProcessQueryCommand request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if caching is enabled
            var adminCachingEnabled = await _settingsService.GetBooleanSettingAsync("EnableQueryCaching");
            var requestCachingEnabled = request.Options.EnableCache;
            var isCachingEnabled = requestCachingEnabled && adminCachingEnabled;

            if (!isCachingEnabled)
            {
                _logger.LogInformation("🚫 Cache disabled - Admin: {AdminCache}, Request: {RequestCache}",
                    adminCachingEnabled, requestCachingEnabled);
                return null;
            }

            // Check enhanced semantic cache first (vector-based similarity)
            var enhancedSemanticEnabled = await _settingsService.GetBooleanSettingAsync("EnableEnhancedSemanticCache");
            if (enhancedSemanticEnabled)
            {
                var semanticCacheCommand = new GetSemanticCacheCommand
                {
                    NaturalLanguageQuery = request.Question,
                    SqlQuery = "", // We don't have SQL yet
                    SimilarityThreshold = 0.85,
                    MaxResults = 5
                };

                var semanticResult = await _mediator.Send(semanticCacheCommand, cancellationToken);
                if (semanticResult?.IsHit == true)
                {
                    _logger.LogInformation("🎯 Enhanced semantic cache hit - Similarity: {Similarity:P2} for query: {Question}",
                        semanticResult.SimilarityScore, request.Question);

                    if (semanticResult.CachedResponse != null)
                    {
                        semanticResult.CachedResponse.Cached = true;
                        // Note: QueryResponse doesn't have Metadata property, so we skip this assignment
                        return semanticResult.CachedResponse;
                    }
                }
            }

            // Fallback to traditional cache
            var cacheKey = GenerateQueryHash(request.Question);
            var cachedResult = await _mediator.Send(new GetCachedQueryQuery { QueryHash = cacheKey }, cancellationToken);

            if (cachedResult != null)
            {
                _logger.LogInformation("🎯 Traditional cache hit for query: {Question}", request.Question);
                cachedResult.Cached = true;
                return cachedResult;
            }

            _logger.LogInformation("🎯 Cache miss (both semantic and traditional) for query: {Question}", request.Question);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking cache, proceeding without cache");
            return null;
        }
    }

    private async Task CacheResultIfEnabled(ProcessQueryCommand request, QueryResponse response, CancellationToken cancellationToken = default)
    {
        try
        {
            var adminCachingEnabled = await _settingsService.GetBooleanSettingAsync("EnableQueryCaching");
            var requestCachingEnabled = request.Options.EnableCache;
            var isCachingEnabled = requestCachingEnabled && adminCachingEnabled;

            if (isCachingEnabled)
            {
                // Store in enhanced semantic cache (vector-based)
                var enhancedSemanticEnabled = await _settingsService.GetBooleanSettingAsync("EnableEnhancedSemanticCache");
                if (enhancedSemanticEnabled)
                {
                    var storeSemanticCommand = new StoreSemanticCacheCommand
                    {
                        NaturalLanguageQuery = request.Question,
                        SqlQuery = response.Sql,
                        Response = response,
                        Expiry = TimeSpan.FromHours(24)
                    };

                    await _mediator.Send(storeSemanticCommand, cancellationToken);
                    _logger.LogInformation("💾 Query stored in enhanced semantic cache: {Question}", request.Question);
                }

                // Also store in traditional cache for backward compatibility
                var cacheKey = GenerateQueryHash(request.Question);
                await _mediator.Send(new CacheQueryCommand
                {
                    QueryHash = cacheKey,
                    Response = response,
                    Expiry = TimeSpan.FromHours(24)
                }, cancellationToken);
                _logger.LogInformation("💾 Query result cached (traditional): {Question}", request.Question);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error caching query result");
        }
    }

    private Task LogQueryAsync(ProcessQueryCommand request, string sql, bool success, int executionTime, string? error = null)
    {
        try
        {
            // This would typically use a dedicated logging command
            _logger.LogInformation("📝 Query logged - User: {UserId}, Success: {Success}, Time: {Time}ms",
                request.UserId, success, executionTime);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error logging query");
        }

        return Task.CompletedTask;
    }

    private async Task NotifyProgress(string userId, string queryId, string stage, string message, int progress, object? data = null)
    {
        try
        {
            if (_progressNotifier != null)
            {
                await _progressNotifier.NotifyProcessingStageAsync(userId, queryId, stage, message, progress, data);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error notifying progress");
        }
    }

    private QueryResponse CreateErrorResponse(string queryId, string error, string sql, PromptDetails? promptDetails)
    {
        return new QueryResponse
        {
            QueryId = queryId,
            Sql = sql,
            Success = false,
            Error = error,
            Timestamp = DateTime.UtcNow,
            PromptDetails = promptDetails,
            Confidence = 0,
            Suggestions = Array.Empty<string>(),
            Cached = false
        };
    }

    private string GenerateQueryHash(string question)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(question.Trim().ToLowerInvariant()));
        return Convert.ToBase64String(hashBytes);
    }

    /// <summary>
    /// Convert SqlQueryResult to QueryResult for compatibility
    /// </summary>
    private static QueryResult ConvertSqlQueryResultToQueryResult(SqlQueryResult sqlResult)
    {
        return new QueryResult
        {
            Data = ConvertDataToObjectArray(sqlResult.Data),
            Metadata = new QueryMetadata
            {
                RowCount = sqlResult.RowCount,
                ExecutionTimeMs = sqlResult.ExecutionTimeMs,
                Error = sqlResult.Error,
                ColumnCount = sqlResult.Data?.FirstOrDefault()?.Count ?? 0,
                Columns = ExtractColumnsFromData(sqlResult.Data)
            },
            IsSuccessful = sqlResult.IsSuccessful
        };
    }

    /// <summary>
    /// Convert List<Dictionary<string, object>> to object[] format
    /// </summary>
    private static object[] ConvertDataToObjectArray(List<Dictionary<string, object>> data)
    {
        return data?.Cast<object>().ToArray() ?? Array.Empty<object>();
    }

    /// <summary>
    /// Extract column metadata from data
    /// </summary>
    private static ColumnMetadata[] ExtractColumnsFromData(List<Dictionary<string, object>>? data)
    {
        if (data == null || !data.Any())
            return Array.Empty<ColumnMetadata>();

        var firstRow = data.First();
        return firstRow.Keys.Select(key => new ColumnMetadata
        {
            Name = key,
            DataType = "string", // Default type
            IsNullable = true
        }).ToArray();
    }

    /// <summary>
    /// Get error message from metadata dictionary
    /// </summary>
    private static string? GetErrorFromMetadata(Dictionary<string, object>? metadata)
    {
        return metadata?.TryGetValue("Error", out var error) == true ? error?.ToString() : null;
    }

    /// <summary>
    /// Get row count from metadata dictionary
    /// </summary>
    private static int GetRowCountFromMetadata(Dictionary<string, object>? metadata)
    {
        if (metadata?.TryGetValue("RowCount", out var rowCount) == true && rowCount is int count)
            return count;
        return 0;
    }

    /// <summary>
    /// Process query using Enhanced Schema Contextualization System
    /// </summary>
    private async Task<QueryResponse> ProcessWithEnhancedContextAsync(
        ProcessQueryCommand request,
        string queryId,
        DateTime startTime,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🚀 [ENHANCED-CONTEXT] Processing query with enhanced business context - Confidence: {Confidence:P2}, Domain: {Domain}",
                request.BusinessProfile?.ConfidenceScore ?? 0, request.BusinessProfile?.Domain.Name ?? "Unknown");

            // Step 1: Check cache first (even with enhanced context)
            await NotifyProgress(request.UserId, queryId, "cache_check", "Checking enhanced query cache", 5);
            var cachedResult = await CheckCacheAsync(request, cancellationToken);
            if (cachedResult != null)
            {
                cachedResult.QueryId = queryId;
                _logger.LogInformation("🎯 [ENHANCED-CONTEXT] Cache hit for enhanced query: {Question}", request.Question);
                return cachedResult;
            }

            // Step 2: Skip NLU analysis since we already have business context
            _logger.LogInformation("⏭️ [ENHANCED-CONTEXT] Skipping NLU analysis - using provided business context");

            // Step 3: Convert enhanced schema metadata to traditional schema format
            await NotifyProgress(request.UserId, queryId, "schema_conversion", "Converting enhanced schema metadata", 20);
            var schema = ConvertEnhancedSchemaToTraditional(request.SchemaMetadata!);

            _logger.LogInformation("✅ [ENHANCED-CONTEXT] Schema conversion completed - Tables: {TableCount}, Total Columns: {ColumnCount}",
                schema.Tables.Count, schema.Tables.Sum(t => t.Columns.Count));

            // Step 4: Skip query intelligence analysis since we have enhanced context
            _logger.LogInformation("⏭️ [ENHANCED-CONTEXT] Skipping query intelligence analysis - using enhanced context");

            // Step 5: Generate SQL with enhanced prompt
            await NotifyProgress(request.UserId, queryId, "ai_processing", "Generating SQL with enhanced context", 40);
            _logger.LogInformation("🔍 [ENHANCED-CONTEXT] Generating SQL with enhanced prompt - Length: {PromptLength} characters",
                request.EnhancedPrompt?.Length ?? 0);

            var sqlResult = await _mediator.Send(new GenerateSqlCommand
            {
                Question = request.Question,
                Schema = schema,
                UserId = request.UserId,
                SessionId = request.SessionId,
                ProviderId = request.Options.ProviderId,
                ModelId = request.Options.ModelId,
                // Enhanced properties
                BusinessProfile = request.BusinessProfile,
                SchemaMetadata = request.SchemaMetadata,
                EnhancedPrompt = request.EnhancedPrompt
            }, cancellationToken);

            _logger.LogInformation("🔍 [ENHANCED-CONTEXT] SQL generation completed - Success: {Success}, Confidence: {Confidence:P2}",
                sqlResult.Success, sqlResult.Confidence);

            if (!sqlResult.Success)
            {
                return CreateErrorResponse(queryId, sqlResult.Error ?? "Enhanced SQL generation failed", "", sqlResult.PromptDetails);
            }

            // Continue with the rest of the pipeline (validation, optimization, execution)
            return await ContinueWithStandardPipeline(request, queryId, startTime, sqlResult, schema, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [ENHANCED-CONTEXT] Error processing query with enhanced context: {Question}", request.Question);
            var executionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            return CreateErrorResponse(queryId, $"Enhanced context processing failed: {ex.Message}", "", null);
        }
    }

    /// <summary>
    /// Convert enhanced schema metadata to traditional schema format
    /// </summary>
    private SchemaMetadata ConvertEnhancedSchemaToTraditional(ContextualBusinessSchema enhancedSchema)
    {
        var schema = new SchemaMetadata
        {
            Tables = enhancedSchema.RelevantTables.Select(table => new TableMetadata
            {
                Name = table.TableName,
                Schema = table.SchemaName ?? "dbo",
                Columns = table.Columns.Select(column => new ColumnMetadata
                {
                    Name = column.ColumnName,
                    DataType = MapBusinessDataTypeToSqlType(column.BusinessDataType),
                    IsNullable = true, // Default to nullable since BusinessColumnInfoDto doesn't have this property
                    IsPrimaryKey = column.IsKeyColumn, // Use IsKeyColumn instead of IsPrimaryKey
                    IsForeignKey = false, // Default to false since BusinessColumnInfoDto doesn't have this property
                    MaxLength = null // BusinessColumnInfoDto doesn't have MaxLength property
                }).ToList(),
                Description = table.BusinessPurpose // Use BusinessPurpose instead of BusinessDescription
            }).ToList()
        };

        _logger.LogInformation("🔄 [SCHEMA-CONVERSION] Converted {EnhancedTableCount} enhanced tables to {TraditionalTableCount} traditional tables",
            enhancedSchema.RelevantTables.Count, schema.Tables.Count);

        return schema;
    }

    /// <summary>
    /// Map business data types to SQL data types
    /// </summary>
    private string MapBusinessDataTypeToSqlType(string businessDataType)
    {
        return businessDataType?.ToLower() switch
        {
            "integer" => "bigint",
            "text" => "nvarchar",
            "decimal" => "decimal(18,2)",
            "date" => "datetime2",
            "boolean" => "bit",
            "currency" => "decimal(18,2)",
            _ => businessDataType ?? "nvarchar"
        };
    }

    /// <summary>
    /// Continue with standard pipeline after SQL generation
    /// </summary>
    private async Task<QueryResponse> ContinueWithStandardPipeline(
        ProcessQueryCommand request,
        string queryId,
        DateTime startTime,
        GenerateSqlResponse sqlResult,
        SchemaMetadata schema,
        CancellationToken cancellationToken)
    {
        // Continue with existing pipeline steps (optimization, validation, execution)
        // This reuses the existing logic from the original Handle method

        // Step 6: Schema Optimization Analysis
        await NotifyProgress(request.UserId, queryId, "sql_optimization", "Analyzing SQL optimization opportunities", 50);
        var optimizationResult = await _mediator.Send(new OptimizeSqlCommand
        {
            OriginalSql = sqlResult.Sql,
            Schema = schema
        }, cancellationToken);

        var finalSql = optimizationResult?.OptimizedSql ?? sqlResult.Sql;
        _logger.LogInformation("🔧 [ENHANCED-CONTEXT] SQL optimization completed - Original: {OriginalLength}, Optimized: {OptimizedLength}",
            sqlResult.Sql.Length, finalSql.Length);

        // Step 7: SQL Validation
        await NotifyProgress(request.UserId, queryId, "sql_validation", "Validating generated SQL", 60);
        var isValidSql = await _mediator.Send(new ValidateSqlQuery
        {
            Sql = finalSql
        }, cancellationToken);

        if (!isValidSql)
        {
            await LogQueryAsync(request, finalSql, false, 0, "SQL validation failed");
            return CreateErrorResponse(queryId, "SQL validation failed", finalSql, sqlResult.PromptDetails);
        }

        // Step 8: Execute SQL
        await NotifyProgress(request.UserId, queryId, "sql_execution", "Executing optimized SQL query", 70);
        var queryResult = await _mediator.Send(new ExecuteSqlCommand
        {
            Sql = finalSql,
            Options = request.Options,
            UserId = request.UserId,
            CancellationToken = cancellationToken
        }, cancellationToken);

        if (!queryResult.IsSuccessful)
        {
            var error = GetErrorFromMetadata(queryResult.Metadata) ?? "SQL execution failed";
            await LogQueryAsync(request, sqlResult.Sql, false, 0, error);
            return CreateErrorResponse(queryId, error, sqlResult.Sql, sqlResult.PromptDetails);
        }

        // Build successful response
        await NotifyProgress(request.UserId, queryId, "response_building", "Building enhanced response", 90);
        var totalExecutionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
        var convertedResult = ConvertSqlQueryResultToQueryResult(queryResult);

        var response = new QueryResponse
        {
            QueryId = queryId,
            Sql = finalSql,
            Result = convertedResult,
            Confidence = sqlResult.Confidence,
            Success = true,
            Timestamp = DateTime.UtcNow,
            ExecutionTimeMs = totalExecutionTime,
            PromptDetails = sqlResult.PromptDetails,
            Cached = false
        };

        // Cache the result
        await CacheResultIfEnabled(request, response, cancellationToken);
        await LogQueryAsync(request, finalSql, true, totalExecutionTime, null);
        await NotifyProgress(request.UserId, queryId, "completed", "Enhanced query processing completed", 100);

        _logger.LogInformation("🎉 [ENHANCED-CONTEXT] Query processing completed successfully - QueryId: {QueryId}, Confidence: {Confidence:P2}, Time: {Time}ms",
            queryId, response.Confidence, totalExecutionTime);

        return response;
    }

    /// <summary>
    /// Validate if enhanced context is available and usable
    /// </summary>
    private bool IsEnhancedContextValid(ProcessQueryCommand request)
    {
        // Check if all required enhanced context components are present
        if (request.BusinessProfile == null)
        {
            _logger.LogDebug("🔍 [VALIDATION] BusinessProfile is null");
            return false;
        }

        if (request.SchemaMetadata == null)
        {
            _logger.LogDebug("🔍 [VALIDATION] SchemaMetadata is null");
            return false;
        }

        if (string.IsNullOrEmpty(request.EnhancedPrompt))
        {
            _logger.LogDebug("🔍 [VALIDATION] EnhancedPrompt is null or empty");
            return false;
        }

        // Check if business context has reasonable confidence
        if (request.BusinessProfile.ConfidenceScore < 0.1)
        {
            _logger.LogWarning("⚠️ [VALIDATION] Business context confidence too low: {Confidence:P2}", request.BusinessProfile.ConfidenceScore);
            return false;
        }

        // Check if schema metadata has relevant tables
        if (!request.SchemaMetadata.RelevantTables.Any())
        {
            _logger.LogWarning("⚠️ [VALIDATION] No relevant tables found in schema metadata");
            return false;
        }

        // Check if enhanced prompt is reasonable length
        if (request.EnhancedPrompt.Length < 100)
        {
            _logger.LogWarning("⚠️ [VALIDATION] Enhanced prompt too short: {Length} characters", request.EnhancedPrompt.Length);
            return false;
        }

        _logger.LogDebug("✅ [VALIDATION] Enhanced context validation passed");
        return true;
    }

    /// <summary>
    /// Get reason for falling back to basic pipeline
    /// </summary>
    private string GetFallbackReason(ProcessQueryCommand request)
    {
        if (request.BusinessProfile == null)
            return "No business profile provided";

        if (request.SchemaMetadata == null)
            return "No schema metadata provided";

        if (string.IsNullOrEmpty(request.EnhancedPrompt))
            return "No enhanced prompt provided";

        if (request.BusinessProfile.ConfidenceScore < 0.1)
            return $"Low business context confidence ({request.BusinessProfile.ConfidenceScore:P2})";

        if (!request.SchemaMetadata.RelevantTables.Any())
            return "No relevant tables in schema metadata";

        if (request.EnhancedPrompt.Length < 100)
            return $"Enhanced prompt too short ({request.EnhancedPrompt.Length} chars)";

        return "Enhanced context validation failed";
    }
}
