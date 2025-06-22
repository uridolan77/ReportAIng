using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.Business;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Models;
using System.Security.Claims;
using BusinessTableStatistics = BIReportingCopilot.Core.Models.BusinessTableStatistics;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Business information management controller for tables, columns, and business context
/// </summary>
[ApiController]
[Route("api/business")]
[Authorize]
public class BusinessController : ControllerBase
{
    private readonly ILogger<BusinessController> _logger;
    private readonly IBusinessTableManagementService _businessTableService;
    private readonly IGlossaryManagementService _glossaryService;
    private readonly IQueryPatternManagementService _queryPatternService;

    public BusinessController(
        ILogger<BusinessController> logger,
        IBusinessTableManagementService businessTableService,
        IGlossaryManagementService glossaryService,
        IQueryPatternManagementService queryPatternService)
    {
        _logger = logger;
        _businessTableService = businessTableService;
        _glossaryService = glossaryService;
        _queryPatternService = queryPatternService;
    }

    #region Business Tables

    /// <summary>
    /// Get all business tables with their complete metadata including metrics
    /// </summary>
    [HttpGet("tables")]
    public async Task<ActionResult<List<BusinessTableInfoDto>>> GetBusinessTables()
    {
        try
        {
            _logger.LogInformation("Getting all business tables with complete metadata");
            var tables = await _businessTableService.GetBusinessTablesAsync();
            return Ok(tables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business tables");
            return StatusCode(500, new { error = "Failed to retrieve business tables", details = ex.Message });
        }
    }

    /// <summary>
    /// Get optimized business tables list (for performance)
    /// </summary>
    [HttpGet("tables/optimized")]
    public async Task<ActionResult<List<BusinessTableInfoOptimizedDto>>> GetBusinessTablesOptimized()
    {
        try
        {
            _logger.LogInformation("Getting optimized business tables list");
            var tables = await _businessTableService.GetBusinessTablesOptimizedAsync();
            return Ok(tables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting optimized business tables");
            return StatusCode(500, new { error = "Failed to retrieve business tables", details = ex.Message });
        }
    }

    /// <summary>
    /// Get specific business table by ID
    /// </summary>
    [HttpGet("tables/{id:long}")]
    public async Task<ActionResult<BusinessTableInfoDto>> GetBusinessTable(long id)
    {
        try
        {
            _logger.LogInformation("Getting business table {TableId}", id);
            var table = await _businessTableService.GetBusinessTableAsync(id);
            
            if (table == null)
            {
                return NotFound(new { error = $"Business table with ID {id} not found" });
            }

            return Ok(table);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business table {TableId}", id);
            return StatusCode(500, new { error = "Failed to retrieve business table", details = ex.Message });
        }
    }

    /// <summary>
    /// Create new business table
    /// </summary>
    [HttpPost("tables")]
    public async Task<ActionResult<BusinessTableInfoDto>> CreateBusinessTable([FromBody] CreateTableInfoRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Creating business table {TableName} by user {UserId}", request.TableName, userId);
            
            var table = await _businessTableService.CreateBusinessTableAsync(request, userId);
            return CreatedAtAction(nameof(GetBusinessTable), new { id = table.Id }, table);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating business table {TableName}", request.TableName);
            return StatusCode(500, new { error = "Failed to create business table", details = ex.Message });
        }
    }

    /// <summary>
    /// Update existing business table
    /// </summary>
    [HttpPut("tables/{id:long}")]
    public async Task<ActionResult<BusinessTableInfoDto>> UpdateBusinessTable(long id, [FromBody] CreateTableInfoRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Updating business table {TableId} by user {UserId}", id, userId);
            
            var table = await _businessTableService.UpdateBusinessTableAsync(id, request, userId);
            
            if (table == null)
            {
                return NotFound(new { error = $"Business table with ID {id} not found" });
            }

            return Ok(table);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating business table {TableId}", id);
            return StatusCode(500, new { error = "Failed to update business table", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete business table
    /// </summary>
    [HttpDelete("tables/{id:long}")]
    public async Task<ActionResult> DeleteBusinessTable(long id)
    {
        try
        {
            _logger.LogInformation("Deleting business table {TableId}", id);
            var result = await _businessTableService.DeleteBusinessTableAsync(id);
            
            if (!result)
            {
                return NotFound(new { error = $"Business table with ID {id} not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting business table {TableId}", id);
            return StatusCode(500, new { error = "Failed to delete business table", details = ex.Message });
        }
    }

    /// <summary>
    /// Search business tables
    /// </summary>
    [HttpGet("tables/search")]
    public async Task<ActionResult<List<BusinessTableInfoDto>>> SearchBusinessTables([FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(new { error = "Search term is required" });
            }

            _logger.LogInformation("Searching business tables with term: {SearchTerm}", searchTerm);
            var tables = await _businessTableService.SearchBusinessTablesAsync(searchTerm);
            return Ok(tables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching business tables with term: {SearchTerm}", searchTerm);
            return StatusCode(500, new { error = "Failed to search business tables", details = ex.Message });
        }
    }

    /// <summary>
    /// Get business table statistics
    /// </summary>
    [HttpGet("tables/statistics")]
    public async Task<ActionResult<BusinessTableStatistics>> GetTableStatistics()
    {
        try
        {
            _logger.LogInformation("Getting business table statistics");
            var statistics = await _businessTableService.GetTableStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business table statistics");
            return StatusCode(500, new { error = "Failed to retrieve table statistics", details = ex.Message });
        }
    }

    #endregion

    #region Business Columns

    /// <summary>
    /// Get all columns for a specific table
    /// </summary>
    [HttpGet("tables/{tableId:long}/columns")]
    public async Task<ActionResult<List<BusinessColumnInfoDto>>> GetTableColumns(long tableId)
    {
        try
        {
            _logger.LogInformation("Getting columns for table {TableId}", tableId);
            var table = await _businessTableService.GetBusinessTableAsync(tableId);

            if (table == null)
            {
                return NotFound(new { error = $"Business table with ID {tableId} not found" });
            }

            return Ok(table.Columns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting columns for table {TableId}", tableId);
            return StatusCode(500, new { error = "Failed to retrieve table columns", details = ex.Message });
        }
    }

    /// <summary>
    /// Get specific column by ID
    /// </summary>
    [HttpGet("columns/{columnId:long}")]
    public async Task<ActionResult<BusinessColumnInfoDto>> GetColumn(long columnId)
    {
        try
        {
            _logger.LogInformation("Getting column {ColumnId}", columnId);
            var column = await _businessTableService.GetColumnAsync(columnId);

            if (column == null)
            {
                return NotFound(new { error = $"Column with ID {columnId} not found" });
            }

            return Ok(column);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting column {ColumnId}", columnId);
            return StatusCode(500, new { error = "Failed to retrieve column", details = ex.Message });
        }
    }

    /// <summary>
    /// Update specific column
    /// </summary>
    [HttpPut("columns/{columnId:long}")]
    public async Task<ActionResult<BusinessColumnInfoDto>> UpdateColumn(long columnId, [FromBody] UpdateColumnRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Updating column {ColumnId} by user {UserId}", columnId, userId);

            var column = await _businessTableService.UpdateColumnAsync(columnId, request, userId);

            if (column == null)
            {
                return NotFound(new { error = $"Column with ID {columnId} not found" });
            }

            return Ok(column);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating column {ColumnId}", columnId);
            return StatusCode(500, new { error = "Failed to update column", details = ex.Message });
        }
    }

    #endregion

    #region Business Glossary

    /// <summary>
    /// Get all glossary terms with pagination
    /// </summary>
    [HttpGet("glossary")]
    public async Task<ActionResult<object>> GetGlossaryTerms([FromQuery] int page = 1, [FromQuery] int limit = 50, [FromQuery] string? category = null)
    {
        try
        {
            _logger.LogInformation("Getting glossary terms - Page: {Page}, Limit: {Limit}, Category: {Category}", page, limit, category);
            var allTerms = await _glossaryService.GetGlossaryTermsAsync();

            // Filter by category if specified
            if (!string.IsNullOrEmpty(category))
            {
                allTerms = allTerms.Where(t => t.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

            // Apply pagination
            var totalCount = allTerms.Count;
            var skip = (page - 1) * limit;
            var paginatedTerms = allTerms.Skip(skip).Take(limit).ToList();

            var response = new
            {
                terms = paginatedTerms,
                total = totalCount
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting glossary terms");
            return StatusCode(500, new { error = "Failed to retrieve glossary terms", details = ex.Message });
        }
    }

    /// <summary>
    /// Get specific glossary term by ID
    /// </summary>
    [HttpGet("glossary/{termId}")]
    public async Task<ActionResult<BusinessGlossaryDto>> GetGlossaryTerm(string termId)
    {
        try
        {
            _logger.LogInformation("Getting glossary term {TermId}", termId);
            var term = await _glossaryService.GetGlossaryTermAsync(termId);
            
            if (term == null)
            {
                return NotFound(new { error = $"Glossary term with ID {termId} not found" });
            }

            return Ok(term);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting glossary term {TermId}", termId);
            return StatusCode(500, new { error = "Failed to retrieve glossary term", details = ex.Message });
        }
    }

    /// <summary>
    /// Create new glossary term
    /// </summary>
    [HttpPost("glossary")]
    public async Task<ActionResult<BusinessGlossaryDto>> CreateGlossaryTerm([FromBody] BusinessGlossaryDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Creating glossary term {Term} by user {UserId}", request.Term, userId);
            
            var term = await _glossaryService.CreateGlossaryTermAsync(request, userId);
            return CreatedAtAction(nameof(GetGlossaryTerm), new { termId = term.Id }, term);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating glossary term {Term}", request.Term);
            return StatusCode(500, new { error = "Failed to create glossary term", details = ex.Message });
        }
    }

    /// <summary>
    /// Update existing glossary term
    /// </summary>
    [HttpPut("glossary/{id:long}")]
    public async Task<ActionResult<BusinessGlossaryDto>> UpdateGlossaryTerm(long id, [FromBody] BusinessGlossaryDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Updating glossary term {TermId} by user {UserId}", id, userId);
            
            var term = await _glossaryService.UpdateGlossaryTermAsync(id, request, userId);
            
            if (term == null)
            {
                return NotFound(new { error = $"Glossary term with ID {id} not found" });
            }

            return Ok(term);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating glossary term {TermId}", id);
            return StatusCode(500, new { error = "Failed to update glossary term", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete glossary term
    /// </summary>
    [HttpDelete("glossary/{id:long}")]
    public async Task<ActionResult> DeleteGlossaryTerm(long id)
    {
        try
        {
            _logger.LogInformation("Deleting glossary term {TermId}", id);
            var result = await _glossaryService.DeleteGlossaryTermAsync(id);
            
            if (!result)
            {
                return NotFound(new { error = $"Glossary term with ID {id} not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting glossary term {TermId}", id);
            return StatusCode(500, new { error = "Failed to delete glossary term", details = ex.Message });
        }
    }

    /// <summary>
    /// Search glossary terms
    /// </summary>
    [HttpGet("glossary/search")]
    public async Task<ActionResult<List<BusinessGlossaryDto>>> SearchGlossaryTerms([FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(new { error = "Search term is required" });
            }

            _logger.LogInformation("Searching glossary terms with term: {SearchTerm}", searchTerm);
            var terms = await _glossaryService.SearchGlossaryTermsAsync(searchTerm);
            return Ok(terms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching glossary terms with term: {SearchTerm}", searchTerm);
            return StatusCode(500, new { error = "Failed to search glossary terms", details = ex.Message });
        }
    }

    #endregion

    #region Query Patterns

    /// <summary>
    /// Get all query patterns
    /// </summary>
    [HttpGet("patterns")]
    public async Task<ActionResult<List<QueryPatternDto>>> GetQueryPatterns()
    {
        try
        {
            _logger.LogInformation("Getting all query patterns");
            var patterns = await _queryPatternService.GetQueryPatternsAsync();
            return Ok(patterns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query patterns");
            return StatusCode(500, new { error = "Failed to retrieve query patterns", details = ex.Message });
        }
    }

    /// <summary>
    /// Get specific query pattern by ID
    /// </summary>
    [HttpGet("patterns/{id:long}")]
    public async Task<ActionResult<QueryPatternDto>> GetQueryPattern(long id)
    {
        try
        {
            _logger.LogInformation("Getting query pattern {PatternId}", id);
            var pattern = await _queryPatternService.GetQueryPatternAsync(id);
            
            if (pattern == null)
            {
                return NotFound(new { error = $"Query pattern with ID {id} not found" });
            }

            return Ok(pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query pattern {PatternId}", id);
            return StatusCode(500, new { error = "Failed to retrieve query pattern", details = ex.Message });
        }
    }

    /// <summary>
    /// Create new query pattern
    /// </summary>
    [HttpPost("patterns")]
    public async Task<ActionResult<QueryPatternDto>> CreateQueryPattern([FromBody] CreateQueryPatternRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Creating query pattern {PatternName} by user {UserId}", request.PatternName, userId);
            
            var pattern = await _queryPatternService.CreateQueryPatternAsync(request, userId);
            return CreatedAtAction(nameof(GetQueryPattern), new { id = pattern.Id }, pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating query pattern {PatternName}", request.PatternName);
            return StatusCode(500, new { error = "Failed to create query pattern", details = ex.Message });
        }
    }

    /// <summary>
    /// Update existing query pattern
    /// </summary>
    [HttpPut("patterns/{id:long}")]
    public async Task<ActionResult<QueryPatternDto>> UpdateQueryPattern(long id, [FromBody] CreateQueryPatternRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Updating query pattern {PatternId} by user {UserId}", id, userId);
            
            var pattern = await _queryPatternService.UpdateQueryPatternAsync(id, request, userId);
            
            if (pattern == null)
            {
                return NotFound(new { error = $"Query pattern with ID {id} not found" });
            }

            return Ok(pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating query pattern {PatternId}", id);
            return StatusCode(500, new { error = "Failed to update query pattern", details = ex.Message });
        }
    }

    #endregion

    #region Business Context Analysis

    /// <summary>
    /// Analyze business context from a natural language question
    /// </summary>
    [HttpPost("context/analyze")]
    public async Task<ActionResult<object>> AnalyzeBusinessContext([FromBody] AnalyzeContextRequest request)
    {
        try
        {
            _logger.LogInformation("Analyzing business context for question: {Question}", request.UserQuestion);

            // For now, return a mock response since we don't have the context analyzer service
            var mockProfile = new
            {
                intent = new { type = "DataRetrieval", confidence = 0.85 },
                entities = new[] { "sales", "revenue", "monthly" },
                domain = "Sales",
                confidenceScore = 0.85,
                suggestedTables = new[] { "SalesData", "Revenue" }
            };

            return Ok(mockProfile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing business context");
            return StatusCode(500, new { error = "Failed to analyze business context", details = ex.Message });
        }
    }

    /// <summary>
    /// Get relevant business metadata for a context profile
    /// </summary>
    [HttpPost("context/metadata")]
    public async Task<ActionResult<object>> GetRelevantMetadata([FromBody] GetMetadataRequest request)
    {
        try
        {
            _logger.LogInformation("Retrieving business metadata for context analysis");

            // Mock response for now
            var mockSchema = new
            {
                relevantTables = new[]
                {
                    new { tableName = "SalesData", relevanceScore = 0.9, columns = new[] { "SaleDate", "Amount", "ProductId" } },
                    new { tableName = "Revenue", relevanceScore = 0.8, columns = new[] { "Month", "TotalRevenue", "Department" } }
                },
                complexity = "Medium",
                confidence = 0.85
            };

            return Ok(mockSchema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving business metadata");
            return StatusCode(500, new { error = "Failed to retrieve business metadata", details = ex.Message });
        }
    }

    /// <summary>
    /// Generate a business-aware prompt for LLM consumption
    /// </summary>
    [HttpPost("context/prompt")]
    public async Task<ActionResult<object>> GenerateBusinessAwarePrompt([FromBody] BusinessPromptRequest request)
    {
        try
        {
            _logger.LogInformation("Generating business-aware prompt for: {Question}", request.UserQuestion);

            // Mock response for now
            var mockResponse = new
            {
                generatedPrompt = $"Based on the business context, generate a SQL query for: {request.UserQuestion}",
                contextProfile = new { intent = "DataRetrieval", domain = "Sales" },
                usedSchema = new { tables = new[] { "SalesData", "Revenue" } },
                confidenceScore = 0.85,
                warnings = new string[] { },
                metadata = new { processingTimeMs = 150, tokensEstimate = 250 }
            };

            return Ok(mockResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating business-aware prompt");
            return StatusCode(500, new { error = "Failed to generate prompt", details = ex.Message });
        }
    }

    /// <summary>
    /// Classify business intent of a question
    /// </summary>
    [HttpPost("context/intent")]
    public async Task<ActionResult<object>> ClassifyIntent([FromBody] ClassifyIntentRequest request)
    {
        try
        {
            var query = request.Query ?? request.UserQuestion;
            _logger.LogInformation("Classifying business intent for: {Query}", query);

            // Mock intent classification
            var mockIntent = new
            {
                type = "DataRetrieval",
                confidence = 0.85,
                subType = "Aggregation",
                domain = "Sales"
            };

            return Ok(mockIntent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying business intent");
            return StatusCode(500, new { error = "Failed to classify intent", details = ex.Message });
        }
    }

    /// <summary>
    /// Extract business entities from a question
    /// </summary>
    [HttpPost("context/entities")]
    public async Task<ActionResult<object>> ExtractEntities([FromBody] ExtractEntitiesRequest request)
    {
        try
        {
            var query = request.Query ?? request.UserQuestion;
            _logger.LogInformation("Extracting business entities from: {Query}", query);

            // Mock entity extraction
            var mockEntities = new[]
            {
                new { name = "sales", type = "BusinessConcept", confidence = 0.9 },
                new { name = "monthly", type = "TimeFrame", confidence = 0.8 },
                new { name = "revenue", type = "Metric", confidence = 0.85 }
            };

            return Ok(mockEntities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting business entities");
            return StatusCode(500, new { error = "Failed to extract entities", details = ex.Message });
        }
    }

    /// <summary>
    /// Find relevant tables based on business context
    /// </summary>
    [HttpPost("context/tables")]
    public async Task<ActionResult<object>> FindRelevantTables([FromBody] FindTablesRequest request)
    {
        try
        {
            _logger.LogInformation("Finding relevant tables for business context");

            // Mock relevant tables
            var mockTables = new[]
            {
                new { tableName = "SalesData", relevanceScore = 0.9, description = "Sales transaction data" },
                new { tableName = "Revenue", relevanceScore = 0.8, description = "Revenue aggregation data" },
                new { tableName = "Products", relevanceScore = 0.7, description = "Product information" }
            };

            return Ok(mockTables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding relevant tables");
            return StatusCode(500, new { error = "Failed to find tables", details = ex.Message });
        }
    }

    #endregion
}

// Request DTOs for Business Context endpoints
public class AnalyzeContextRequest
{
    public string UserQuestion { get; set; } = string.Empty;
    public string? UserId { get; set; }
}

public class GetMetadataRequest
{
    public object ContextProfile { get; set; } = new();
    public int MaxTables { get; set; } = 5;
}

public class BusinessPromptRequest
{
    public string UserQuestion { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? PreferredDomain { get; set; }
    public string ComplexityLevel { get; set; } = "Standard";
    public bool IncludeExamples { get; set; } = true;
    public bool IncludeBusinessRules { get; set; } = true;
    public int MaxTables { get; set; } = 5;
    public int MaxTokens { get; set; } = 4000;
}

public class ClassifyIntentRequest
{
    public string Query { get; set; } = string.Empty;
    public string UserQuestion { get; set; } = string.Empty; // For backward compatibility
}

public class ExtractEntitiesRequest
{
    public string Query { get; set; } = string.Empty;
    public string UserQuestion { get; set; } = string.Empty; // For backward compatibility
}

public class FindTablesRequest
{
    public object ContextProfile { get; set; } = new();
    public int MaxTables { get; set; } = 5;
}
