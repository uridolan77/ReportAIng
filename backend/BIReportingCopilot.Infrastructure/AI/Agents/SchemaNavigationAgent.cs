using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BIReportingCopilot.Core.Interfaces.Agents;
using BIReportingCopilot.Core.Interfaces.Business;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Core.Models.Agents;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.Business;
using BIReportingCopilot.Core.DTOs;
using System.Text.Json;
using AgentQueryIntent = BIReportingCopilot.Core.Models.Agents.QueryIntent;
using AgentSchemaContext = BIReportingCopilot.Core.Models.Agents.SchemaContext;

namespace BIReportingCopilot.Infrastructure.AI.Agents;

/// <summary>
/// Schema Navigation Agent - Specialized in intelligent schema discovery and context building
/// </summary>
public class SchemaNavigationAgent : ISchemaNavigationAgent
{
    private readonly ILogger<SchemaNavigationAgent> _logger;
    private readonly IBusinessTableManagementService _businessMetadataService;
    private readonly ISchemaService _schemaService;
    private readonly IConfiguration _configuration;
    private readonly AgentCapabilities _capabilities;
    private bool _isInitialized = false;

    public string AgentType => "SchemaNavigation";
    public AgentCapabilities Capabilities => _capabilities;

    public SchemaNavigationAgent(
        ILogger<SchemaNavigationAgent> logger,
        IBusinessTableManagementService businessMetadataService,
        ISchemaService schemaService,
        IConfiguration configuration)
    {
        _logger = logger;
        _businessMetadataService = businessMetadataService;
        _schemaService = schemaService;
        _configuration = configuration;
        
        _capabilities = new AgentCapabilities
        {
            AgentType = AgentType,
            SupportedOperations = new List<string>
            {
                "DiscoverRelevantTables",
                "BuildContext",
                "SuggestOptimalJoins",
                "FindRelatedTables",
                "GetTableMetadata",
                "ValidateSchemaElements"
            },
            Metadata = new Dictionary<string, object>
            {
                ["Version"] = "2.0.0",
                ["Specialization"] = "Schema Discovery and Navigation",
                ["SupportedDatabases"] = new[] { "SQL Server", "PostgreSQL", "MySQL" }
            },
            PerformanceScore = 0.92,
            IsAvailable = true
        };
    }

    #region ISpecializedAgent Implementation

    public async Task<AgentResponse> ProcessAsync(AgentRequest request, AgentContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("🗺️ SchemaNavigationAgent processing request {RequestId} of type {RequestType}", 
                request.RequestId, request.RequestType);

            object? result = request.RequestType switch
            {
                "DiscoverRelevantTables" => await DiscoverRelevantTablesAsync(
                    JsonSerializer.Deserialize<AgentQueryIntent>(request.Payload.ToString()!)!, context),
                "BuildContext" => await BuildContextAsync(
                    JsonSerializer.Deserialize<List<RelevantTable>>(request.Payload.ToString()!)!, context),
                "SuggestOptimalJoins" => await SuggestOptimalJoinsAsync(
                    JsonSerializer.Deserialize<List<RelevantTable>>(request.Payload.ToString()!)!, context),
                "FindRelatedTables" => await FindRelatedTablesAsync(
                    request.Parameters["tableName"].ToString()!, 
                    (int)(request.Parameters.GetValueOrDefault("maxDepth", 2)), context),
                "GetTableMetadata" => await GetTableMetadataAsync(
                    request.Payload.ToString()!, context),
                "ValidateSchemaElements" => await ValidateSchemaElementsAsync(
                    JsonSerializer.Deserialize<List<EntityReference>>(request.Payload.ToString()!)!, context),
                _ => throw new NotSupportedException($"Request type {request.RequestType} is not supported")
            };

            stopwatch.Stop();

            return new AgentResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Result = result,
                ExecutionTime = stopwatch.Elapsed,
                Metadata = new Dictionary<string, object>
                {
                    ["AgentType"] = AgentType,
                    ["ProcessedAt"] = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Error processing request {RequestId}", request.RequestId);
            
            return new AgentResponse
            {
                RequestId = request.RequestId,
                Success = false,
                ErrorMessage = ex.Message,
                ExecutionTime = stopwatch.Elapsed
            };
        }
    }

    public async Task<HealthStatus> GetHealthStatusAsync()
    {
        try
        {
            // Test business metadata service connectivity
            await _businessMetadataService.GetBusinessTablesAsync();
            
            return new HealthStatus
            {
                IsHealthy = true,
                Status = "Healthy",
                Metrics = new Dictionary<string, object>
                {
                    ["LastCheck"] = DateTime.UtcNow,
                    ["BusinessMetadataServiceAvailable"] = true,
                    ["SchemaServiceAvailable"] = true,
                    ["IsInitialized"] = _isInitialized
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed for SchemaNavigationAgent");
            
            return new HealthStatus
            {
                IsHealthy = false,
                Status = "Unhealthy",
                Issues = new List<string> { ex.Message }
            };
        }
    }

    public async Task InitializeAsync(Dictionary<string, object> configuration)
    {
        _logger.LogInformation("🚀 Initializing SchemaNavigationAgent");
        
        // Initialize any required resources
        await Task.Delay(100); // Simulate initialization
        
        _isInitialized = true;
        _capabilities.LastHealthCheck = DateTime.UtcNow;
        
        _logger.LogInformation("✅ SchemaNavigationAgent initialized successfully");
    }

    public async Task ShutdownAsync()
    {
        _logger.LogInformation("🛑 Shutting down SchemaNavigationAgent");
        
        _isInitialized = false;
        _capabilities.IsAvailable = false;
        
        await Task.CompletedTask;
    }

    #endregion

    #region ISchemaNavigationAgent Implementation

    public async Task<List<RelevantTable>> DiscoverRelevantTablesAsync(AgentQueryIntent intent, AgentContext? context = null)
    {
        _logger.LogDebug("🔍 Discovering relevant tables for intent: {QueryType}", intent.QueryType);

        try
        {
            var relevantTables = new List<RelevantTable>();
            
            // Get all business tables
            var businessTables = await _businessMetadataService.GetBusinessTablesAsync();
            
            // Score tables based on entity matches
            foreach (var table in businessTables)
            {
                var relevanceScore = CalculateTableRelevance(table, intent);
                
                if (relevanceScore > 0.3) // Minimum relevance threshold
                {
                    var relevantColumns = await GetRelevantColumnsForTable(table, intent);
                    
                    relevantTables.Add(new RelevantTable
                    {
                        TableName = table.TableName,
                        SchemaName = table.SchemaName,
                        RelevanceScore = relevanceScore,
                        RelevantColumns = relevantColumns,
                        ReasonForInclusion = GenerateInclusionReason(table, intent, relevanceScore),
                        Metadata = new Dictionary<string, object>
                        {
                            ["BusinessPurpose"] = table.BusinessPurpose ?? "",
                            ["BusinessContext"] = table.BusinessContext ?? "",
                            ["TableType"] = "Table"
                        }
                    });
                }
            }

            // Sort by relevance score
            relevantTables = relevantTables.OrderByDescending(t => t.RelevanceScore).ToList();
            
            _logger.LogDebug("✅ Discovered {Count} relevant tables", relevantTables.Count);
            return relevantTables;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error discovering relevant tables");
            throw;
        }
    }

    public async Task<AgentSchemaContext> BuildContextAsync(List<RelevantTable> tables, AgentContext? context = null)
    {
        _logger.LogDebug("🏗️ Building schema context for {Count} tables", tables.Count);

        try
        {
            var schemaContext = new AgentSchemaContext
            {
                Tables = tables,
                AvailableJoins = await DiscoverJoinPathsAsync(tables),
                AvailableFunctions = await GetAvailableFunctionsAsync(),
                BusinessRules = await GetBusinessRulesAsync(tables),
                Constraints = await GetConstraintsAsync(tables)
            };

            _logger.LogDebug("✅ Schema context built with {JoinCount} joins and {FunctionCount} functions", 
                schemaContext.AvailableJoins.Count, schemaContext.AvailableFunctions.Count);

            return schemaContext;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error building schema context");
            throw;
        }
    }

    public async Task<List<JoinPath>> SuggestOptimalJoinsAsync(List<RelevantTable> tables, AgentContext? context = null)
    {
        _logger.LogDebug("🔗 Suggesting optimal joins for {Count} tables", tables.Count);

        try
        {
            var joinPaths = new List<JoinPath>();
            
            // Get foreign key relationships
            var relationships = await GetTableRelationshipsAsync(tables);
            
            // Generate join paths based on relationships
            foreach (var relationship in relationships)
            {
                var joinPath = new JoinPath
                {
                    FromTable = relationship.FromTable,
                    ToTable = relationship.ToTable,
                    JoinType = DetermineOptimalJoinType(relationship),
                    Conditions = new List<JoinCondition>
                    {
                        new JoinCondition
                        {
                            LeftColumn = relationship.FromColumn,
                            RightColumn = relationship.ToColumn,
                            Operator = "="
                        }
                    },
                    PerformanceScore = CalculateJoinPerformanceScore(relationship),
                    IsRecommended = relationship.IsRecommended
                };
                
                joinPaths.Add(joinPath);
            }

            // Sort by performance score
            joinPaths = joinPaths.OrderByDescending(j => j.PerformanceScore).ToList();
            
            _logger.LogDebug("✅ Suggested {Count} optimal joins", joinPaths.Count);
            return joinPaths;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error suggesting optimal joins");
            throw;
        }
    }

    public async Task<List<RelevantTable>> FindRelatedTablesAsync(string tableName, int maxDepth = 2, AgentContext? context = null)
    {
        _logger.LogDebug("🔍 Finding related tables for {TableName} with max depth {MaxDepth}", tableName, maxDepth);

        try
        {
            var relatedTables = new List<RelevantTable>();
            var visited = new HashSet<string>();
            
            await FindRelatedTablesRecursive(tableName, maxDepth, 0, visited, relatedTables);
            
            _logger.LogDebug("✅ Found {Count} related tables", relatedTables.Count);
            return relatedTables;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error finding related tables for {TableName}", tableName);
            throw;
        }
    }

    public async Task<SchemaMetadata> GetTableMetadataAsync(string tableName, AgentContext? context = null)
    {
        _logger.LogDebug("📊 Getting metadata for table {TableName}", tableName);

        try
        {
            // Get schema metadata from schema service
            var metadata = await _schemaService.GetSchemaMetadataAsync();
            
            // Find the specific table
            var tableMetadata = metadata.Tables.FirstOrDefault(t => 
                t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            
            if (tableMetadata == null)
            {
                throw new ArgumentException($"Table {tableName} not found in schema metadata");
            }

            _logger.LogDebug("✅ Retrieved metadata for table {TableName} with {ColumnCount} columns", 
                tableName, tableMetadata.Columns.Count);

            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting metadata for table {TableName}", tableName);
            throw;
        }
    }

    public async Task<bool> ValidateSchemaElementsAsync(List<EntityReference> entities, AgentContext? context = null)
    {
        _logger.LogDebug("✅ Validating {Count} schema elements", entities.Count);

        try
        {
            var metadata = await _schemaService.GetSchemaMetadataAsync();
            var allValid = true;

            foreach (var entity in entities)
            {
                var isValid = entity.Type.ToLowerInvariant() switch
                {
                    "table" => metadata.Tables.Any(t => t.Name.Equals(entity.Name, StringComparison.OrdinalIgnoreCase)),
                    "column" => metadata.Tables.Any(t => t.Columns.Any(c => c.Name.Equals(entity.Name, StringComparison.OrdinalIgnoreCase))),
                    _ => false
                };

                if (!isValid)
                {
                    _logger.LogWarning("⚠️ Schema element not found: {EntityType} {EntityName}", entity.Type, entity.Name);
                    allValid = false;
                }
            }

            _logger.LogDebug("✅ Schema validation complete. All valid: {AllValid}", allValid);
            return allValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error validating schema elements");
            throw;
        }
    }

    #endregion

    #region Private Helper Methods

    private double CalculateTableRelevance(BusinessTableInfoDto table, AgentQueryIntent intent)
    {
        double score = 0.0;

        // Check entity matches
        foreach (var entity in intent.Entities)
        {
            if (table.TableName.Contains(entity.Name, StringComparison.OrdinalIgnoreCase) ||
                table.BusinessPurpose?.Contains(entity.Name, StringComparison.OrdinalIgnoreCase) == true)
            {
                score += 0.4;
            }
        }

        // Check business context matches
        if (!string.IsNullOrEmpty(intent.BusinessContext.Domain))
        {
            if (table.BusinessPurpose?.Contains(intent.BusinessContext.Domain, StringComparison.OrdinalIgnoreCase) == true)
            {
                score += 0.3;
            }
        }

        // Check business terms
        foreach (var term in intent.BusinessContext.BusinessTerms)
        {
            if (table.BusinessPurpose?.Contains(term, StringComparison.OrdinalIgnoreCase) == true)
            {
                score += 0.2;
            }
        }

        return Math.Min(score, 1.0);
    }

    private async Task<List<string>> GetRelevantColumnsForTable(BusinessTableInfoDto table, AgentQueryIntent intent)
    {
        var relevantColumns = new List<string>();

        // Use columns from the table DTO (already loaded)
        var columns = table.Columns ?? new List<BusinessColumnInfo>();

        foreach (var column in columns)
        {
            // Check if column matches any entities or business terms
            foreach (var entity in intent.Entities)
            {
                if (column.ColumnName.Contains(entity.Name, StringComparison.OrdinalIgnoreCase) ||
                    column.BusinessMeaning?.Contains(entity.Name, StringComparison.OrdinalIgnoreCase) == true)
                {
                    relevantColumns.Add(column.ColumnName);
                    break;
                }
            }
        }

        return relevantColumns.Distinct().ToList();
    }

    private string GenerateInclusionReason(BusinessTableInfoDto table, AgentQueryIntent intent, double relevanceScore)
    {
        var reasons = new List<string>();

        if (relevanceScore > 0.7)
            reasons.Add("High relevance to query entities");
        else if (relevanceScore > 0.5)
            reasons.Add("Moderate relevance to query context");
        else
            reasons.Add("Potential relevance based on business metadata");

        if (!string.IsNullOrEmpty(intent.BusinessContext.Domain))
            reasons.Add($"Matches {intent.BusinessContext.Domain} domain");

        return string.Join("; ", reasons);
    }

    private async Task<List<JoinPath>> DiscoverJoinPathsAsync(List<RelevantTable> tables)
    {
        // Simplified join discovery - in production, this would use actual FK relationships
        var joinPaths = new List<JoinPath>();
        
        // Common join patterns based on table names
        var commonJoins = new Dictionary<string, List<string>>
        {
            ["customers"] = new() { "orders", "customer_orders" },
            ["orders"] = new() { "order_items", "customers" },
            ["products"] = new() { "order_items", "categories" },
            ["users"] = new() { "user_preferences", "user_sessions" }
        };

        foreach (var table in tables)
        {
            var tableName = table.TableName.ToLowerInvariant();
            if (commonJoins.ContainsKey(tableName))
            {
                foreach (var relatedTable in commonJoins[tableName])
                {
                    if (tables.Any(t => t.TableName.ToLowerInvariant().Contains(relatedTable)))
                    {
                        joinPaths.Add(new JoinPath
                        {
                            FromTable = table.TableName,
                            ToTable = relatedTable,
                            JoinType = "INNER",
                            PerformanceScore = 0.8,
                            IsRecommended = true
                        });
                    }
                }
            }
        }

        return joinPaths;
    }

    private async Task<List<string>> GetAvailableFunctionsAsync()
    {
        // Return common SQL functions
        return new List<string>
        {
            "COUNT", "SUM", "AVG", "MAX", "MIN",
            "UPPER", "LOWER", "SUBSTRING", "CONCAT",
            "DATEPART", "DATEDIFF", "GETDATE",
            "CASE", "COALESCE", "ISNULL"
        };
    }

    private async Task<Dictionary<string, object>> GetBusinessRulesAsync(List<RelevantTable> tables)
    {
        return new Dictionary<string, object>
        {
            ["MaxRowsPerQuery"] = 10000,
            ["RequiredFilters"] = new[] { "date_range" },
            ["RestrictedColumns"] = new[] { "password", "ssn", "credit_card" }
        };
    }

    private async Task<Dictionary<string, object>> GetConstraintsAsync(List<RelevantTable> tables)
    {
        return new Dictionary<string, object>
        {
            ["MaxJoins"] = 5,
            ["MaxSubqueries"] = 3,
            ["TimeoutSeconds"] = 30
        };
    }

    private async Task<List<TableRelationship>> GetTableRelationshipsAsync(List<RelevantTable> tables)
    {
        // Simplified relationship discovery
        var relationships = new List<TableRelationship>();
        
        // Add common relationships based on naming conventions
        foreach (var table in tables)
        {
            var tableName = table.TableName.ToLowerInvariant();
            
            if (tableName.Contains("order") && !tableName.Contains("item"))
            {
                // Orders table typically joins to customers and order_items
                if (tables.Any(t => t.TableName.ToLowerInvariant().Contains("customer")))
                {
                    relationships.Add(new TableRelationship
                    {
                        FromTable = table.TableName,
                        ToTable = tables.First(t => t.TableName.ToLowerInvariant().Contains("customer")).TableName,
                        FromColumn = "customer_id",
                        ToColumn = "id",
                        IsRecommended = true
                    });
                }
            }
        }

        return relationships;
    }

    private string DetermineOptimalJoinType(TableRelationship relationship)
    {
        // Simple logic for join type determination
        return relationship.IsRecommended ? "INNER" : "LEFT";
    }

    private double CalculateJoinPerformanceScore(TableRelationship relationship)
    {
        // Simple performance scoring
        return relationship.IsRecommended ? 0.9 : 0.6;
    }

    private async Task FindRelatedTablesRecursive(string tableName, int maxDepth, int currentDepth, 
        HashSet<string> visited, List<RelevantTable> relatedTables)
    {
        if (currentDepth >= maxDepth || visited.Contains(tableName))
            return;

        visited.Add(tableName);

        // Find direct relationships (simplified)
        var relationships = await GetTableRelationshipsAsync(new List<RelevantTable>());
        
        foreach (var rel in relationships.Where(r => r.FromTable.Equals(tableName, StringComparison.OrdinalIgnoreCase)))
        {
            if (!visited.Contains(rel.ToTable))
            {
                relatedTables.Add(new RelevantTable
                {
                    TableName = rel.ToTable,
                    RelevanceScore = 0.8 - (currentDepth * 0.2),
                    ReasonForInclusion = $"Related to {tableName} via {rel.FromColumn}"
                });

                await FindRelatedTablesRecursive(rel.ToTable, maxDepth, currentDepth + 1, visited, relatedTables);
            }
        }
    }

    #endregion
}

// Supporting classes
public class TableRelationship
{
    public string FromTable { get; set; } = string.Empty;
    public string ToTable { get; set; } = string.Empty;
    public string FromColumn { get; set; } = string.Empty;
    public string ToColumn { get; set; } = string.Empty;
    public bool IsRecommended { get; set; }
}
