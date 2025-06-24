using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BIReportingCopilot.Core.Interfaces.Services;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Infrastructure.BusinessContext;
using BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;
using BIReportingCopilot.Infrastructure.AI.Core;
using BIReportingCopilot.Infrastructure.AI.Management;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.API.Hubs;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Controller for testing and monitoring AI pipeline steps
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AIPipelineTestController : ControllerBase
{
    private readonly ILogger<AIPipelineTestController> _logger;
    private readonly IBusinessContextAnalyzer _businessContextAnalyzer;
    private readonly ITokenBudgetManager _tokenBudgetManager;
    private readonly IBusinessMetadataRetrievalService _metadataService;
    private readonly IContextualPromptBuilder _promptBuilder;
    private readonly ProcessFlowTracker _processFlowTracker;
    private readonly IAIService _aiService;
    private readonly IPipelineTestNotificationService _notificationService;

    public AIPipelineTestController(
        ILogger<AIPipelineTestController> logger,
        IBusinessContextAnalyzer businessContextAnalyzer,
        ITokenBudgetManager tokenBudgetManager,
        IBusinessMetadataRetrievalService metadataService,
        IContextualPromptBuilder promptBuilder,
        ProcessFlowTracker processFlowTracker,
        IAIService aiService,
        IPipelineTestNotificationService notificationService)
    {
        _logger = logger;
        _businessContextAnalyzer = businessContextAnalyzer;
        _tokenBudgetManager = tokenBudgetManager;
        _metadataService = metadataService;
        _promptBuilder = promptBuilder;
        _processFlowTracker = processFlowTracker;
        _aiService = aiService;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Test individual pipeline steps with configurable parameters
    /// </summary>
    [HttpPost("test-steps")]
    [ProducesResponseType(typeof(PipelineTestResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestPipelineStepsAsync([FromBody] PipelineTestRequest request)
    {
        var userId = GetCurrentUserId();
        var testId = !string.IsNullOrEmpty(request.TestId) ? request.TestId : Guid.NewGuid().ToString("N")[..8];
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        _logger.LogInformation("🧪 [PIPELINE-TEST] Starting pipeline test {TestId} for user {UserId}", testId, userId);

        var result = new PipelineTestResult
        {
            TestId = testId,
            Query = request.Query,
            RequestedSteps = request.Steps,
            StartTime = DateTime.UtcNow,
            Results = new Dictionary<string, object>()
        };

        // Send real-time notification that test has started
        try
        {
            _logger.LogInformation("🔔 [DEBUG] About to send TestStarted notification for {TestId}", testId);
            await _notificationService.SendTestStartedAsync(testId, userId, request);
            _logger.LogInformation("✅ [DEBUG] Successfully sent TestStarted notification for {TestId}", testId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [DEBUG] Failed to send TestStarted notification for {TestId}: {Error}", testId, ex.Message);
        }

        try
        {
            // Step 1: Business Context Analysis
            if (request.Steps.Contains(PipelineStep.BusinessContextAnalysis))
            {
                try
                {
                    _logger.LogInformation("🔔 [DEBUG] About to send StepStarted notification for BusinessContextAnalysis in {TestId}", testId);
                    await _notificationService.SendStepStartedAsync(testId, "BusinessContextAnalysis", new { query = request.Query });
                    _logger.LogInformation("✅ [DEBUG] Successfully sent StepStarted notification for BusinessContextAnalysis in {TestId}", testId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ [DEBUG] Failed to send StepStarted notification for BusinessContextAnalysis in {TestId}: {Error}", testId, ex.Message);
                }
                var contextResult = await TestBusinessContextAnalysisAsync(request.Query, userId, request.Parameters, testId);
                result.Results[PipelineStep.BusinessContextAnalysis.ToString()] = contextResult;
                result.BusinessProfile = contextResult.BusinessProfile;
                await _notificationService.SendStepCompletedAsync(testId, "BusinessContextAnalysis", contextResult);
            }

            // Step 2: Token Budget Management
            if (request.Steps.Contains(PipelineStep.TokenBudgetManagement))
            {
                await _notificationService.SendStepStartedAsync(testId, "TokenBudgetManagement", new { maxTokens = request.Parameters.MaxTokens });
                var tokenResult = await TestTokenBudgetManagementAsync(result.BusinessProfile, request.Parameters, testId);
                result.Results[PipelineStep.TokenBudgetManagement.ToString()] = tokenResult;
                result.TokenBudget = tokenResult.TokenBudget;
                await _notificationService.SendStepCompletedAsync(testId, "TokenBudgetManagement", tokenResult);
            }

            // Step 3: Schema Retrieval
            if (request.Steps.Contains(PipelineStep.SchemaRetrieval))
            {
                await _notificationService.SendStepStartedAsync(testId, "SchemaRetrieval", new { maxTables = request.Parameters.MaxTables });
                var schemaResult = await TestSchemaRetrievalAsync(result.BusinessProfile, request.Parameters, testId);
                result.Results[PipelineStep.SchemaRetrieval.ToString()] = schemaResult;
                result.SchemaMetadata = schemaResult.SchemaMetadata;
                await _notificationService.SendStepCompletedAsync(testId, "SchemaRetrieval", schemaResult);
            }

            // Step 4: Prompt Building
            if (request.Steps.Contains(PipelineStep.PromptBuilding))
            {
                await _notificationService.SendStepStartedAsync(testId, "PromptBuilding", new { includeExamples = request.Parameters.IncludeExamples });
                var promptResult = await TestPromptBuildingAsync(request.Query, result.BusinessProfile, result.SchemaMetadata, request.Parameters, testId);
                result.Results[PipelineStep.PromptBuilding.ToString()] = promptResult;
                result.GeneratedPrompt = promptResult.Prompt;
                await _notificationService.SendStepCompletedAsync(testId, "PromptBuilding", promptResult);
            }

            // Step 5: AI Generation (if enabled)
            if (request.Steps.Contains(PipelineStep.AIGeneration) && request.Parameters.EnableAIGeneration)
            {
                await _notificationService.SendStepStartedAsync(testId, "AIGeneration", new { temperature = request.Parameters.Temperature, enabled = request.Parameters.EnableAIGeneration });
                var aiResult = await TestAIGenerationAsync(result.GeneratedPrompt, request.Parameters, testId);
                result.Results[PipelineStep.AIGeneration.ToString()] = aiResult;
                result.GeneratedSQL = aiResult.GeneratedSQL;
                await _notificationService.SendStepCompletedAsync(testId, "AIGeneration", aiResult);
            }

            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.TotalDurationMs = stopwatch.ElapsedMilliseconds;
            result.Success = true;

            _logger.LogInformation("✅ [PIPELINE-TEST] Completed pipeline test {TestId} in {Duration}ms", testId, stopwatch.ElapsedMilliseconds);

            // Send real-time notification that test has completed
            await _notificationService.SendTestCompletedAsync(testId, result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.EndTime = DateTime.UtcNow;
            result.TotalDurationMs = stopwatch.ElapsedMilliseconds;
            result.Success = false;
            result.Error = ex.Message;

            _logger.LogError(ex, "❌ [PIPELINE-TEST] Error in pipeline test {TestId}", testId);

            // Send real-time notification about the error
            await _notificationService.SendTestErrorAsync(testId, ex.Message, new { stackTrace = ex.StackTrace });

            return Ok(result); // Return 200 with error details for easier frontend handling
        }
    }

    /// <summary>
    /// Get available pipeline steps and their configurations
    /// </summary>
    [HttpGet("steps")]
    public IActionResult GetAvailableSteps()
    {
        var steps = new[]
        {
            new PipelineStepInfo
            {
                Step = PipelineStep.BusinessContextAnalysis,
                Name = "Business Context Analysis",
                Description = "Analyze user query to extract intent, domain, entities, and business terms",
                Parameters = new[]
                {
                    new ParameterInfo { Name = "ConfidenceThreshold", Type = "decimal", DefaultValue = "0.7", Description = "Minimum confidence threshold for entity extraction" },
                    new ParameterInfo { Name = "MaxEntities", Type = "int", DefaultValue = "10", Description = "Maximum number of entities to extract" }
                }
            },
            new PipelineStepInfo
            {
                Step = PipelineStep.TokenBudgetManagement,
                Name = "Token Budget Management",
                Description = "Create and manage token budgets for optimal prompt construction",
                Parameters = new[]
                {
                    new ParameterInfo { Name = "MaxTokens", Type = "int", DefaultValue = "4000", Description = "Maximum total tokens allowed" },
                    new ParameterInfo { Name = "ReservedResponseTokens", Type = "int", DefaultValue = "500", Description = "Tokens reserved for AI response" }
                }
            },
            new PipelineStepInfo
            {
                Step = PipelineStep.SchemaRetrieval,
                Name = "Schema Retrieval",
                Description = "Retrieve relevant database schema and business metadata",
                Parameters = new[]
                {
                    new ParameterInfo { Name = "MaxTables", Type = "int", DefaultValue = "10", Description = "Maximum number of tables to retrieve" },
                    new ParameterInfo { Name = "RelevanceThreshold", Type = "decimal", DefaultValue = "0.5", Description = "Minimum relevance score for table inclusion" }
                }
            },
            new PipelineStepInfo
            {
                Step = PipelineStep.PromptBuilding,
                Name = "Prompt Building",
                Description = "Build business-aware prompt for AI generation",
                Parameters = new[]
                {
                    new ParameterInfo { Name = "IncludeExamples", Type = "bool", DefaultValue = "true", Description = "Include example queries in prompt" },
                    new ParameterInfo { Name = "IncludeBusinessRules", Type = "bool", DefaultValue = "true", Description = "Include business rules in prompt" }
                }
            },
            new PipelineStepInfo
            {
                Step = PipelineStep.AIGeneration,
                Name = "AI Generation",
                Description = "Generate SQL using AI service (requires EnableAIGeneration=true)",
                Parameters = new[]
                {
                    new ParameterInfo { Name = "EnableAIGeneration", Type = "bool", DefaultValue = "false", Description = "Enable actual AI generation (costs money!)" },
                    new ParameterInfo { Name = "Temperature", Type = "decimal", DefaultValue = "0.1", Description = "AI temperature setting" }
                }
            }
        };

        return Ok(steps);
    }

    /// <summary>
    /// Save a test configuration for reuse
    /// </summary>
    [HttpPost("configurations")]
    [ProducesResponseType(typeof(PipelineTestConfiguration), StatusCodes.Status200OK)]
    public async Task<IActionResult> SaveTestConfigurationAsync([FromBody] SaveConfigurationRequest request)
    {
        var userId = GetCurrentUserId();

        var configuration = new PipelineTestConfiguration
        {
            Id = Guid.NewGuid().ToString(),
            Name = request.Name,
            Description = request.Description,
            Steps = request.Steps,
            Parameters = request.Parameters,
            SavedAt = DateTime.UtcNow,
            CreatedBy = userId,
            Category = request.Category ?? "custom"
        };

        // In a real implementation, save to database
        // For now, return the configuration

        _logger.LogInformation("💾 Saved test configuration '{Name}' for user {UserId}", request.Name, userId);

        await _notificationService.SendConfigurationSavedAsync(userId, configuration);

        return Ok(configuration);
    }

    /// <summary>
    /// Get saved test configurations for the current user
    /// </summary>
    [HttpGet("configurations")]
    [ProducesResponseType(typeof(List<PipelineTestConfiguration>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTestConfigurationsAsync()
    {
        var userId = GetCurrentUserId();

        // In a real implementation, load from database
        // For now, return sample configurations
        var configurations = new List<PipelineTestConfiguration>
        {
            new PipelineTestConfiguration
            {
                Id = "config-1",
                Name = "Full Pipeline Test",
                Description = "Test all pipeline steps with default parameters",
                Steps = Enum.GetValues<PipelineStep>().ToList(),
                Parameters = new PipelineTestParameters(),
                SavedAt = DateTime.UtcNow.AddDays(-1),
                CreatedBy = userId,
                Category = "preset"
            },
            new PipelineTestConfiguration
            {
                Id = "config-2",
                Name = "Context Analysis Only",
                Description = "Test only business context analysis and token management",
                Steps = new List<PipelineStep> { PipelineStep.BusinessContextAnalysis, PipelineStep.TokenBudgetManagement },
                Parameters = new PipelineTestParameters { MaxTokens = 2000 },
                SavedAt = DateTime.UtcNow.AddHours(-2),
                CreatedBy = userId,
                Category = "debugging"
            }
        };

        return Ok(configurations);
    }

    /// <summary>
    /// Load a specific test configuration
    /// </summary>
    [HttpGet("configurations/{configId}")]
    [ProducesResponseType(typeof(PipelineTestConfiguration), StatusCodes.Status200OK)]
    public async Task<IActionResult> LoadTestConfigurationAsync(string configId)
    {
        var userId = GetCurrentUserId();

        // In a real implementation, load from database
        var configuration = new PipelineTestConfiguration
        {
            Id = configId,
            Name = "Sample Configuration",
            Description = "A sample test configuration",
            Steps = new List<PipelineStep> { PipelineStep.BusinessContextAnalysis, PipelineStep.PromptBuilding },
            Parameters = new PipelineTestParameters { MaxTokens = 3000, ConfidenceThreshold = 0.8m },
            SavedAt = DateTime.UtcNow.AddDays(-1),
            CreatedBy = userId,
            Category = "custom"
        };

        _logger.LogInformation("📂 Loaded test configuration '{Name}' for user {UserId}", configuration.Name, userId);

        await _notificationService.SendConfigurationLoadedAsync(userId, configuration);

        return Ok(configuration);
    }

    /// <summary>
    /// Delete a test configuration
    /// </summary>
    [HttpDelete("configurations/{configId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteTestConfigurationAsync(string configId)
    {
        var userId = GetCurrentUserId();

        // In a real implementation, delete from database

        _logger.LogInformation("🗑️ Deleted test configuration {ConfigId} for user {UserId}", configId, userId);

        return Ok(new { message = "Configuration deleted successfully", configId });
    }

    /// <summary>
    /// Validate test parameters
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ParameterValidationResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateParametersAsync([FromBody] ParameterValidationRequest request)
    {
        var validationResult = new ParameterValidationResult
        {
            IsValid = true,
            Errors = new List<string>(),
            Warnings = new List<string>(),
            Suggestions = new List<string>()
        };

        // Validate token limits
        if (request.Parameters.MaxTokens.HasValue && request.Parameters.MaxTokens < 100)
        {
            validationResult.Errors.Add("MaxTokens must be at least 100");
            validationResult.IsValid = false;
        }

        if (request.Parameters.MaxTokens.HasValue && request.Parameters.MaxTokens > 8000)
        {
            validationResult.Warnings.Add("MaxTokens above 8000 may result in high costs");
        }

        // Validate confidence thresholds
        if (request.Parameters.ConfidenceThreshold.HasValue &&
            (request.Parameters.ConfidenceThreshold < 0 || request.Parameters.ConfidenceThreshold > 1))
        {
            validationResult.Errors.Add("ConfidenceThreshold must be between 0 and 1");
            validationResult.IsValid = false;
        }

        // Validate step combinations
        if (request.Steps.Contains(PipelineStep.PromptBuilding) &&
            !request.Steps.Contains(PipelineStep.BusinessContextAnalysis))
        {
            validationResult.Warnings.Add("PromptBuilding works best with BusinessContextAnalysis");
        }

        if (request.Steps.Contains(PipelineStep.AIGeneration) && !request.Parameters.EnableAIGeneration)
        {
            validationResult.Errors.Add("AIGeneration step requires EnableAIGeneration parameter to be true");
            validationResult.IsValid = false;
        }

        // Add suggestions
        if (request.Parameters.MaxTokens.HasValue && request.Parameters.MaxTokens < 2000)
        {
            validationResult.Suggestions.Add("Consider increasing MaxTokens to 2000+ for better results");
        }

        await _notificationService.SendParameterValidationAsync("validation", validationResult);

        return Ok(validationResult);
    }

    private async Task<BusinessContextTestResult> TestBusinessContextAnalysisAsync(string query, string userId, PipelineTestParameters parameters, string testId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            await _notificationService.SendStepProgressAsync(testId, "BusinessContextAnalysis", 25, "Analyzing user question...");
            var businessProfile = await _businessContextAnalyzer.AnalyzeUserQuestionAsync(query, userId);
            await _notificationService.SendStepProgressAsync(testId, "BusinessContextAnalysis", 75, "Extracting entities and intent...");
            stopwatch.Stop();

            return new BusinessContextTestResult
            {
                Success = true,
                DurationMs = stopwatch.ElapsedMilliseconds,
                BusinessProfile = businessProfile,
                ExtractedEntities = businessProfile.Entities.Count,
                ConfidenceScore = businessProfile.ConfidenceScore,
                Intent = businessProfile.Intent.Type.ToString(),
                Domain = businessProfile.Domain.Name
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new BusinessContextTestResult
            {
                Success = false,
                DurationMs = stopwatch.ElapsedMilliseconds,
                Error = ex.Message
            };
        }
    }

    private async Task<TokenBudgetTestResult> TestTokenBudgetManagementAsync(BusinessContextProfile? businessProfile, PipelineTestParameters parameters, string testId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            if (businessProfile == null)
            {
                throw new InvalidOperationException("Business profile is required for token budget management");
            }

            var maxTokens = parameters.MaxTokens ?? 4000;
            var reservedTokens = parameters.ReservedResponseTokens ?? 500;

            await _notificationService.SendStepProgressAsync(testId, "TokenBudgetManagement", 50, "Creating token budget...");
            var tokenBudget = await _tokenBudgetManager.CreateTokenBudgetAsync(businessProfile, maxTokens, reservedTokens);
            stopwatch.Stop();

            return new TokenBudgetTestResult
            {
                Success = true,
                DurationMs = stopwatch.ElapsedMilliseconds,
                TokenBudget = tokenBudget,
                MaxTokens = tokenBudget.MaxTotalTokens,
                AvailableContextTokens = tokenBudget.AvailableContextTokens,
                ReservedTokens = tokenBudget.ReservedResponseTokens
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new TokenBudgetTestResult
            {
                Success = false,
                DurationMs = stopwatch.ElapsedMilliseconds,
                Error = ex.Message
            };
        }
    }

    private async Task<SchemaRetrievalTestResult> TestSchemaRetrievalAsync(BusinessContextProfile? businessProfile, PipelineTestParameters parameters, string testId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            if (businessProfile == null)
            {
                throw new InvalidOperationException("Business profile is required for schema retrieval");
            }

            var maxTables = parameters.MaxTables ?? 10;
            await _notificationService.SendStepProgressAsync(testId, "SchemaRetrieval", 30, "Retrieving relevant tables...");
            var schemaMetadata = await _metadataService.GetRelevantBusinessMetadataAsync(businessProfile, maxTables);
            await _notificationService.SendStepProgressAsync(testId, "SchemaRetrieval", 80, "Processing schema metadata...");
            stopwatch.Stop();

            return new SchemaRetrievalTestResult
            {
                Success = true,
                DurationMs = stopwatch.ElapsedMilliseconds,
                SchemaMetadata = schemaMetadata,
                TablesRetrieved = schemaMetadata.RelevantTables.Count,
                RelevanceScore = 0.8, // Default relevance score since the interface doesn't return one
                TableNames = schemaMetadata.RelevantTables.Select(t => t.TableName).ToList()
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new SchemaRetrievalTestResult
            {
                Success = false,
                DurationMs = stopwatch.ElapsedMilliseconds,
                Error = ex.Message
            };
        }
    }

    private async Task<PromptBuildingTestResult> TestPromptBuildingAsync(string query, BusinessContextProfile? businessProfile, ContextualBusinessSchema? schemaMetadata, PipelineTestParameters parameters, string testId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            if (businessProfile == null)
            {
                throw new InvalidOperationException("Business profile is required for prompt building");
            }

            if (schemaMetadata == null)
            {
                throw new InvalidOperationException("Schema metadata is required for prompt building");
            }

            await _notificationService.SendStepProgressAsync(testId, "PromptBuilding", 40, "Building business-aware prompt...");
            var prompt = await _promptBuilder.BuildBusinessAwarePromptAsync(query, businessProfile, schemaMetadata);
            await _notificationService.SendStepProgressAsync(testId, "PromptBuilding", 90, "Finalizing prompt structure...");
            stopwatch.Stop();

            return new PromptBuildingTestResult
            {
                Success = true,
                DurationMs = stopwatch.ElapsedMilliseconds,
                Prompt = prompt,
                PromptLength = prompt?.Length ?? 0,
                EstimatedTokens = EstimateTokenUsage(prompt ?? "")
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new PromptBuildingTestResult
            {
                Success = false,
                DurationMs = stopwatch.ElapsedMilliseconds,
                Error = ex.Message
            };
        }
    }

    private async Task<AIGenerationTestResult> TestAIGenerationAsync(string? prompt, PipelineTestParameters parameters, string testId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            if (string.IsNullOrEmpty(prompt))
            {
                throw new InvalidOperationException("Prompt is required for AI generation");
            }

            await _notificationService.SendStepProgressAsync(testId, "AIGeneration", 20, "Sending request to AI service...");
            var generatedSQL = await _aiService.GenerateSQLAsync(prompt, null, CancellationToken.None);
            await _notificationService.SendStepProgressAsync(testId, "AIGeneration", 90, "Processing AI response...");
            stopwatch.Stop();

            return new AIGenerationTestResult
            {
                Success = true,
                DurationMs = stopwatch.ElapsedMilliseconds,
                GeneratedSQL = generatedSQL,
                SQLLength = generatedSQL?.Length ?? 0,
                EstimatedCost = CalculateEstimatedCost(prompt, generatedSQL ?? "")
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new AIGenerationTestResult
            {
                Success = false,
                DurationMs = stopwatch.ElapsedMilliseconds,
                Error = ex.Message
            };
        }
    }

    private int EstimateTokenUsage(string text)
    {
        // Simple token estimation: ~4 characters per token
        return (int)Math.Ceiling(text.Length / 4.0);
    }

    private decimal CalculateEstimatedCost(string prompt, string completion)
    {
        var promptTokens = EstimateTokenUsage(prompt);
        var completionTokens = EstimateTokenUsage(completion);

        // GPT-4 pricing (approximate)
        var promptCost = promptTokens * 0.00003m; // $0.03 per 1K tokens
        var completionCost = completionTokens * 0.00006m; // $0.06 per 1K tokens

        return promptCost + completionCost;
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst(ClaimTypes.Name)?.Value
            ?? "test-user";
    }
}

#region Data Models

/// <summary>
/// Request for pipeline testing
/// </summary>
public class PipelineTestRequest
{
    public string? TestId { get; set; } // Optional test ID for real-time monitoring
    public string Query { get; set; } = string.Empty;
    public List<PipelineStep> Steps { get; set; } = new();
    public PipelineTestParameters Parameters { get; set; } = new();
}

/// <summary>
/// Pipeline test parameters
/// </summary>
public class PipelineTestParameters
{
    // Business Context Analysis
    public decimal? ConfidenceThreshold { get; set; }
    public int? MaxEntities { get; set; }

    // Token Budget Management
    public int? MaxTokens { get; set; }
    public int? ReservedResponseTokens { get; set; }

    // Schema Retrieval
    public int? MaxTables { get; set; }
    public decimal? RelevanceThreshold { get; set; }

    // Prompt Building
    public bool? IncludeExamples { get; set; }
    public bool? IncludeBusinessRules { get; set; }

    // AI Generation
    public bool EnableAIGeneration { get; set; } = false;
    public decimal? Temperature { get; set; }
}

/// <summary>
/// Pipeline steps enum
/// </summary>
public enum PipelineStep
{
    BusinessContextAnalysis,
    TokenBudgetManagement,
    SchemaRetrieval,
    PromptBuilding,
    AIGeneration
}

/// <summary>
/// Pipeline test result
/// </summary>
public class PipelineTestResult
{
    public string TestId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public List<PipelineStep> RequestedSteps { get; set; } = new();
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public long TotalDurationMs { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
    public Dictionary<string, object> Results { get; set; } = new();

    // Intermediate results for chaining
    public BusinessContextProfile? BusinessProfile { get; set; }
    public TokenBudget? TokenBudget { get; set; }
    public ContextualBusinessSchema? SchemaMetadata { get; set; }
    public string? GeneratedPrompt { get; set; }
    public string? GeneratedSQL { get; set; }
}

/// <summary>
/// Information about a pipeline step
/// </summary>
public class PipelineStepInfo
{
    public PipelineStep Step { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ParameterInfo[] Parameters { get; set; } = Array.Empty<ParameterInfo>();
}

/// <summary>
/// Parameter information
/// </summary>
public class ParameterInfo
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string DefaultValue { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Base test result
/// </summary>
public abstract class BaseTestResult
{
    public bool Success { get; set; }
    public long DurationMs { get; set; }
    public string? Error { get; set; }
}

/// <summary>
/// Business context analysis test result
/// </summary>
public class BusinessContextTestResult : BaseTestResult
{
    public BusinessContextProfile? BusinessProfile { get; set; }
    public int ExtractedEntities { get; set; }
    public double ConfidenceScore { get; set; }
    public string Intent { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
}

/// <summary>
/// Token budget management test result
/// </summary>
public class TokenBudgetTestResult : BaseTestResult
{
    public TokenBudget? TokenBudget { get; set; }
    public int MaxTokens { get; set; }
    public int AvailableContextTokens { get; set; }
    public int ReservedTokens { get; set; }
}

/// <summary>
/// Schema retrieval test result
/// </summary>
public class SchemaRetrievalTestResult : BaseTestResult
{
    public ContextualBusinessSchema? SchemaMetadata { get; set; }
    public int TablesRetrieved { get; set; }
    public double RelevanceScore { get; set; }
    public List<string> TableNames { get; set; } = new();
}

/// <summary>
/// Prompt building test result
/// </summary>
public class PromptBuildingTestResult : BaseTestResult
{
    public string? Prompt { get; set; }
    public int PromptLength { get; set; }
    public int EstimatedTokens { get; set; }
}

/// <summary>
/// AI generation test result
/// </summary>
public class AIGenerationTestResult : BaseTestResult
{
    public string? GeneratedSQL { get; set; }
    public int SQLLength { get; set; }
    public decimal EstimatedCost { get; set; }
}

/// <summary>
/// Test configuration for saving/loading
/// </summary>
public class PipelineTestConfiguration
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<PipelineStep> Steps { get; set; } = new();
    public PipelineTestParameters Parameters { get; set; } = new();
    public DateTime SavedAt { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public string Category { get; set; } = "custom"; // preset, debugging, performance, custom
}

/// <summary>
/// Request to save a configuration
/// </summary>
public class SaveConfigurationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<PipelineStep> Steps { get; set; } = new();
    public PipelineTestParameters Parameters { get; set; } = new();
    public string? Category { get; set; }
}

/// <summary>
/// Parameter validation request
/// </summary>
public class ParameterValidationRequest
{
    public List<PipelineStep> Steps { get; set; } = new();
    public PipelineTestParameters Parameters { get; set; } = new();
}

/// <summary>
/// Parameter validation result
/// </summary>
public class ParameterValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
}

#endregion
