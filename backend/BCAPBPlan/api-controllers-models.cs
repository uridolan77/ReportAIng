// API/Controllers/BusinessPromptController.cs
namespace ReportAIng.API.Controllers
{
    [ApiController]
    [Route("api/prompts")]
    [Authorize]
    public class BusinessPromptController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IBusinessContextAnalyzer _contextAnalyzer;
        private readonly IBusinessMetadataRetrievalService _metadataService;
        private readonly IContextualPromptBuilder _promptBuilder;
        private readonly ILogger<BusinessPromptController> _logger;

        public BusinessPromptController(
            IMediator mediator,
            IBusinessContextAnalyzer contextAnalyzer,
            IBusinessMetadataRetrievalService metadataService,
            IContextualPromptBuilder promptBuilder,
            ILogger<BusinessPromptController> logger)
        {
            _mediator = mediator;
            _contextAnalyzer = contextAnalyzer;
            _metadataService = metadataService;
            _promptBuilder = promptBuilder;
            _logger = logger;
        }

        /// <summary>
        /// Generate a business-aware prompt from a natural language question
        /// </summary>
        [HttpPost("business-aware")]
        [ProducesResponseType(typeof(BusinessAwarePromptResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BusinessAwarePromptResponse>> GenerateBusinessAwarePrompt(
            [FromBody] BusinessPromptRequest request)
        {
            try
            {
                _logger.LogInformation("Generating business-aware prompt for user: {UserId}", 
                    request.UserId);

                // Validate request
                var validationResult = await ValidateRequest(request);
                if (!validationResult.IsValid)
                {
                    return BadRequest(new ErrorResponse 
                    { 
                        Message = "Invalid request",
                        Details = validationResult.Errors 
                    });
                }

                // Analyze business context
                var contextProfile = await _contextAnalyzer.AnalyzeUserQuestionAsync(
                    request.UserQuestion, 
                    request.UserId);

                // Apply domain preference if specified
                if (request.PreferredDomain.HasValue)
                {
                    contextProfile.Domain = await EnrichDomainInfo(request.PreferredDomain.Value);
                }

                // Retrieve relevant business metadata
                var businessSchema = await _metadataService.GetRelevantBusinessMetadataAsync(
                    contextProfile,
                    request.MaxTables ?? 5);

                // Build the prompt
                var generatedPrompt = await _promptBuilder.BuildBusinessAwarePromptAsync(
                    request.UserQuestion,
                    contextProfile,
                    businessSchema);

                // Prepare response
                var response = new BusinessAwarePromptResponse
                {
                    GeneratedPrompt = generatedPrompt,
                    ContextProfile = contextProfile,
                    UsedSchema = businessSchema,
                    ConfidenceScore = contextProfile.ConfidenceScore,
                    Metadata = new Dictionary<string, object>
                    {
                        ["GeneratedAt"] = DateTime.UtcNow,
                        ["PromptLength"] = generatedPrompt.Length,
                        ["TablesUsed"] = businessSchema.RelevantTables.Count,
                        ["ComplexityLevel"] = request.ComplexityLevel.ToString()
                    }
                };

                // Check for warnings
                response.Warnings = GenerateWarnings(contextProfile, businessSchema);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating business-aware prompt");
                return StatusCode(500, new ErrorResponse 
                { 
                    Message = "An error occurred while generating the prompt",
                    Details = new[] { ex.Message }
                });
            }
        }

        /// <summary>
        /// Analyze business context without generating a prompt
        /// </summary>
        [HttpPost("analyze-context")]
        [ProducesResponseType(typeof(BusinessContextAnalysisResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<BusinessContextAnalysisResponse>> AnalyzeBusinessContext(
            [FromBody] ContextAnalysisRequest request)
        {
            var contextProfile = await _contextAnalyzer.AnalyzeUserQuestionAsync(
                request.UserQuestion, 
                request.UserId);

            var response = new BusinessContextAnalysisResponse
            {
                ContextProfile = contextProfile,
                SuggestedDomains = await GetSuggestedDomains(contextProfile),
                IdentifiedTables = await GetIdentifiedTables(contextProfile),
                Recommendations = GenerateRecommendations(contextProfile)
            };

            return Ok(response);
        }

        /// <summary>
        /// Get relevant business metadata for a context
        /// </summary>
        [HttpPost("metadata")]
        [ProducesResponseType(typeof(BusinessMetadataResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<BusinessMetadataResponse>> GetBusinessMetadata(
            [FromBody] MetadataRequest request)
        {
            var contextProfile = await _contextAnalyzer.AnalyzeUserQuestionAsync(
                request.UserQuestion, 
                request.UserId);

            var businessSchema = await _metadataService.GetRelevantBusinessMetadataAsync(
                contextProfile,
                request.MaxTables ?? 5);

            var response = new BusinessMetadataResponse
            {
                Schema = businessSchema,
                TableDetails = await EnrichTableDetails(businessSchema.RelevantTables),
                GlossaryTerms = businessSchema.RelevantGlossaryTerms,
                Relationships = businessSchema.Relationships
            };

            return Ok(response);
        }

        // Private helper methods
        private async Task<ValidationResult> ValidateRequest(BusinessPromptRequest request)
        {
            var validator = new BusinessPromptRequestValidator();
            return await validator.ValidateAsync(request);
        }

        private List<string> GenerateWarnings(
            BusinessContextProfile profile, 
            ContextualBusinessSchema schema)
        {
            var warnings = new List<string>();

            if (profile.ConfidenceScore < 0.5)
            {
                warnings.Add("Low confidence in business context understanding. Consider providing more specific details.");
            }

            if (!schema.RelevantTables.Any())
            {
                warnings.Add("No relevant tables found. The query might need manual table specification.");
            }

            if (schema.Complexity == SchemaComplexity.VeryComplex)
            {
                warnings.Add("Complex schema detected. The generated SQL might require optimization.");
            }

            if (profile.Entities.Count(e => e.Type == EntityType.Table) > 5)
            {
                warnings.Add("Many tables referenced. Consider breaking down into smaller queries.");
            }

            return warnings;
        }
    }
}

// API/Models/Requests/BusinessPromptRequest.cs
namespace ReportAIng.API.Models.Requests
{
    public class BusinessPromptRequest
    {
        public string UserQuestion { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public BusinessDomainType? PreferredDomain { get; set; }
        public PromptComplexityLevel ComplexityLevel { get; set; } = PromptComplexityLevel.Standard;
        public bool IncludeExamples { get; set; } = true;
        public bool IncludeBusinessRules { get; set; } = true;
        public int? MaxTables { get; set; }
        public Dictionary<string, string>? AdditionalContext { get; set; }
    }

    public class ContextAnalysisRequest
    {
        public string UserQuestion { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public bool IncludeTableSuggestions { get; set; } = true;
    }

    public class MetadataRequest
    {
        public string UserQuestion { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public int? MaxTables { get; set; }
        public bool IncludeColumnDetails { get; set; } = true;
        public bool IncludeRelationships { get; set; } = true;
    }

    public enum BusinessDomainType
    {
        Gaming,
        Finance,
        Operations,
        Marketing,
        Sales,
        Customer,
        Product,
        Analytics
    }
}

// API/Models/Responses/BusinessPromptResponses.cs
namespace ReportAIng.API.Models.Responses
{
    public class BusinessAwarePromptResponse
    {
        public string GeneratedPrompt { get; set; } = string.Empty;
        public BusinessContextProfile ContextProfile { get; set; } = new();
        public ContextualBusinessSchema UsedSchema { get; set; } = new();
        public double ConfidenceScore { get; set; }
        public List<string> Warnings { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    public class BusinessContextAnalysisResponse
    {
        public BusinessContextProfile ContextProfile { get; set; } = new();
        public List<DomainSuggestion> SuggestedDomains { get; set; } = new();
        public List<TableSuggestion> IdentifiedTables { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }

    public class BusinessMetadataResponse
    {
        public ContextualBusinessSchema Schema { get; set; } = new();
        public List<EnrichedTableInfo> TableDetails { get; set; } = new();
        public List<BusinessGlossaryDto> GlossaryTerms { get; set; } = new();
        public List<TableRelationship> Relationships { get; set; } = new();
    }

    public class DomainSuggestion
    {
        public string DomainName { get; set; } = string.Empty;
        public double RelevanceScore { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class TableSuggestion
    {
        public string TableName { get; set; } = string.Empty;
        public string SchemaName { get; set; } = string.Empty;
        public double ConfidenceScore { get; set; }
        public string MatchReason { get; set; } = string.Empty;
    }

    public class EnrichedTableInfo
    {
        public BusinessTableInfoDto Table { get; set; } = new();
        public List<BusinessColumnInfoDto> RelevantColumns { get; set; } = new();
        public int TotalColumns { get; set; }
        public List<string> SampleQueries { get; set; } = new();
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public IEnumerable<string> Details { get; set; } = new List<string>();
        public string TraceId { get; set; } = Guid.NewGuid().ToString();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}

// API/Validators/BusinessPromptRequestValidator.cs
namespace ReportAIng.API.Validators
{
    public class BusinessPromptRequestValidator : AbstractValidator<BusinessPromptRequest>
    {
        public BusinessPromptRequestValidator()
        {
            RuleFor(x => x.UserQuestion)
                .NotEmpty().WithMessage("User question is required")
                .MinimumLength(3).WithMessage("Question must be at least 3 characters")
                .MaximumLength(1000).WithMessage("Question must not exceed 1000 characters");

            RuleFor(x => x.MaxTables)
                .InclusiveBetween(1, 10)
                .When(x => x.MaxTables.HasValue)
                .WithMessage("MaxTables must be between 1 and 10");

            RuleFor(x => x.ComplexityLevel)
                .IsInEnum().WithMessage("Invalid complexity level");

            RuleFor(x => x.PreferredDomain)
                .IsInEnum()
                .When(x => x.PreferredDomain.HasValue)
                .WithMessage("Invalid domain type");
        }
    }
}