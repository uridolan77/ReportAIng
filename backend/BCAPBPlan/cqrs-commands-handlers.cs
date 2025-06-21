// Application/Commands/GenerateBusinessPromptCommand.cs
namespace ReportAIng.Application.Commands
{
    public class GenerateBusinessPromptCommand : IRequest<BusinessPromptResult>
    {
        public string UserQuestion { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public BusinessDomainType? PreferredDomain { get; set; }
        public PromptComplexityLevel ComplexityLevel { get; set; }
        public PromptGenerationOptions Options { get; set; } = new();
    }

    public class PromptGenerationOptions
    {
        public bool IncludeExamples { get; set; } = true;
        public bool IncludeBusinessRules { get; set; } = true;
        public int MaxTables { get; set; } = 5;
        public int MaxExamples { get; set; } = 3;
        public bool IncludePerformanceHints { get; set; } = true;
        public Dictionary<string, string> AdditionalContext { get; set; } = new();
    }

    public class BusinessPromptResult
    {
        public string GeneratedPrompt { get; set; } = string.Empty;
        public BusinessContextProfile ContextProfile { get; set; } = new();
        public ContextualBusinessSchema Schema { get; set; } = new();
        public double ConfidenceScore { get; set; }
        public List<string> Warnings { get; set; } = new();
        public GenerationMetadata Metadata { get; set; } = new();
    }

    public class GenerationMetadata
    {
        public DateTime GeneratedAt { get; set; }
        public int PromptLength { get; set; }
        public int TablesUsed { get; set; }
        public int ExamplesIncluded { get; set; }
        public TimeSpan GenerationTime { get; set; }
        public string TemplateUsed { get; set; } = string.Empty;
    }
}

// Application/Handlers/GenerateBusinessPromptHandler.cs
namespace ReportAIng.Application.Handlers
{
    public class GenerateBusinessPromptHandler : IRequestHandler<GenerateBusinessPromptCommand, BusinessPromptResult>
    {
        private readonly IBusinessContextAnalyzer _contextAnalyzer;
        private readonly IBusinessMetadataRetrievalService _metadataService;
        private readonly IContextualPromptBuilder _promptBuilder;
        private readonly ILLMUsageLogService _usageLogService;
        private readonly ILogger<GenerateBusinessPromptHandler> _logger;
        private readonly IMetrics _metrics;

        public GenerateBusinessPromptHandler(
            IBusinessContextAnalyzer contextAnalyzer,
            IBusinessMetadataRetrievalService metadataService,
            IContextualPromptBuilder promptBuilder,
            ILLMUsageLogService usageLogService,
            ILogger<GenerateBusinessPromptHandler> logger,
            IMetrics metrics)
        {
            _contextAnalyzer = contextAnalyzer;
            _metadataService = metadataService;
            _promptBuilder = promptBuilder;
            _usageLogService = usageLogService;
            _logger = logger;
            _metrics = metrics;
        }

        public async Task<BusinessPromptResult> Handle(
            GenerateBusinessPromptCommand request, 
            CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                _logger.LogInformation("Handling business prompt generation for user: {UserId}", 
                    request.UserId);

                // Step 1: Analyze business context
                var contextProfile = await _contextAnalyzer.AnalyzeUserQuestionAsync(
                    request.UserQuestion, 
                    request.UserId);

                // Apply domain preference if specified
                if (request.PreferredDomain.HasValue)
                {
                    await ApplyDomainPreference(contextProfile, request.PreferredDomain.Value);
                }

                // Step 2: Retrieve relevant business metadata
                var businessSchema = await _metadataService.GetRelevantBusinessMetadataAsync(
                    contextProfile,
                    request.Options.MaxTables);

                // Validate schema
                var schemaValidation = ValidateSchema(businessSchema);
                if (!schemaValidation.IsValid)
                {
                    _logger.LogWarning("Schema validation failed: {Reasons}", 
                        string.Join(", ", schemaValidation.Warnings));
                }

                // Step 3: Build the prompt
                var generatedPrompt = await _promptBuilder.BuildBusinessAwarePromptAsync(
                    request.UserQuestion,
                    contextProfile,
                    businessSchema);

                // Apply options
                generatedPrompt = ApplyGenerationOptions(generatedPrompt, request.Options);

                // Step 4: Log usage
                await LogPromptGeneration(request, generatedPrompt, stopwatch.Elapsed);

                // Step 5: Track metrics
                _metrics.Increment("prompt.generation.success");
                _metrics.RecordTime("prompt.generation.duration", stopwatch.ElapsedMilliseconds);

                // Prepare result
                var result = new BusinessPromptResult
                {
                    GeneratedPrompt = generatedPrompt,
                    ContextProfile = contextProfile,
                    Schema = businessSchema,
                    ConfidenceScore = CalculateConfidenceScore(contextProfile, businessSchema),
                    Warnings = schemaValidation.Warnings,
                    Metadata = new GenerationMetadata
                    {
                        GeneratedAt = DateTime.UtcNow,
                        PromptLength = generatedPrompt.Length,
                        TablesUsed = businessSchema.RelevantTables.Count,
                        ExamplesIncluded = CountExamples(generatedPrompt),
                        GenerationTime = stopwatch.Elapsed,
                        TemplateUsed = contextProfile.Intent.Type.ToString()
                    }
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating business prompt");
                _metrics.Increment("prompt.generation.error");
                throw;
            }
            finally
            {
                stopwatch.Stop();
            }
        }

        private async Task ApplyDomainPreference(
            BusinessContextProfile profile, 
            BusinessDomainType preferredDomain)
        {
            // Override domain if user specified preference
            profile.Domain = new BusinessDomain
            {
                Name = preferredDomain.ToString(),
                Description = $"User-specified domain: {preferredDomain}",
                RelevanceScore = 1.0
            };
        }

        private SchemaValidationResult ValidateSchema(ContextualBusinessSchema schema)
        {
            var result = new SchemaValidationResult { IsValid = true };

            if (!schema.RelevantTables.Any())
            {
                result.Warnings.Add("No relevant tables found for the query");
                result.IsValid = false;
            }

            if (schema.RelevantTables.Count > 10)
            {
                result.Warnings.Add("Too many tables identified. Consider refining the query");
            }

            if (schema.Complexity == SchemaComplexity.VeryComplex)
            {
                result.Warnings.Add("Complex schema detected. Query optimization may be needed");
            }

            return result;
        }

        private string ApplyGenerationOptions(string prompt, PromptGenerationOptions options)
        {
            if (!options.IncludeExamples)
            {
                prompt = RemoveExamplesSection(prompt);
            }

            if (!options.IncludeBusinessRules)
            {
                prompt = RemoveBusinessRulesSection(prompt);
            }

            if (!options.IncludePerformanceHints)
            {
                prompt = RemovePerformanceSection(prompt);
            }

            return prompt;
        }

        private async Task LogPromptGeneration(
            GenerateBusinessPromptCommand request,
            string generatedPrompt,
            TimeSpan duration)
        {
            await _usageLogService.LogPromptGenerationAsync(new PromptGenerationLog
            {
                UserId = request.UserId,
                UserQuestion = request.UserQuestion,
                GeneratedPrompt = generatedPrompt,
                PromptLength = generatedPrompt.Length,
                GenerationTimeMs = (int)duration.TotalMilliseconds,
                Timestamp = DateTime.UtcNow
            });
        }

        private double CalculateConfidenceScore(
            BusinessContextProfile profile,
            ContextualBusinessSchema schema)
        {
            var scores = new List<double>
            {
                profile.ConfidenceScore,
                schema.RelevanceScore,
                schema.RelevantTables.Any() ? 1.0 : 0.0,
                Math.Min(1.0, schema.RelevantGlossaryTerms.Count / 5.0)
            };

            return scores.Average();
        }

        private int CountExamples(string prompt)
        {
            // Count SQL code blocks in the prompt
            return Regex.Matches(prompt, @"```sql", RegexOptions.IgnoreCase).Count;
        }

        private class SchemaValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Warnings { get; set; } = new();
        }
    }
}

// Application/Queries/GetBusinessContextQuery.cs
namespace ReportAIng.Application.Queries
{
    public class GetBusinessContextQuery : IRequest<BusinessContextResult>
    {
        public string UserQuestion { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public bool IncludeTableSuggestions { get; set; } = true;
        public bool IncludeDomainSuggestions { get; set; } = true;
    }

    public class BusinessContextResult
    {
        public BusinessContextProfile Profile { get; set; } = new();
        public List<TableSuggestion> TableSuggestions { get; set; } = new();
        public List<DomainSuggestion> DomainSuggestions { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
    }
}

// Application/Handlers/GetBusinessContextHandler.cs
namespace ReportAIng.Application.Handlers
{
    public class GetBusinessContextHandler : IRequestHandler<GetBusinessContextQuery, BusinessContextResult>
    {
        private readonly IBusinessContextAnalyzer _contextAnalyzer;
        private readonly IBusinessTableRepository _tableRepository;
        private readonly IBusinessDomainRepository _domainRepository;
        private readonly ILogger<GetBusinessContextHandler> _logger;

        public GetBusinessContextHandler(
            IBusinessContextAnalyzer contextAnalyzer,
            IBusinessTableRepository tableRepository,
            IBusinessDomainRepository domainRepository,
            ILogger<GetBusinessContextHandler> logger)
        {
            _contextAnalyzer = contextAnalyzer;
            _tableRepository = tableRepository;
            _domainRepository = domainRepository;
            _logger = logger;
        }

        public async Task<BusinessContextResult> Handle(
            GetBusinessContextQuery request,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("Analyzing business context for question: {Question}", 
                request.UserQuestion);

            // Analyze context
            var profile = await _contextAnalyzer.AnalyzeUserQuestionAsync(
                request.UserQuestion,
                request.UserId);

            var result = new BusinessContextResult
            {
                Profile = profile,
                Recommendations = GenerateRecommendations(profile)
            };

            // Get table suggestions if requested
            if (request.IncludeTableSuggestions)
            {
                result.TableSuggestions = await GetTableSuggestions(profile);
            }

            // Get domain suggestions if requested
            if (request.IncludeDomainSuggestions)
            {
                result.DomainSuggestions = await GetDomainSuggestions(profile);
            }

            return result;
        }

        private async Task<List<TableSuggestion>> GetTableSuggestions(BusinessContextProfile profile)
        {
            var suggestions = new List<TableSuggestion>();

            // Get suggestions from entities
            foreach (var entity in profile.Entities.Where(e => e.Type == EntityType.Table))
            {
                var tables = await _tableRepository.SearchByNameAsync(entity.Name);
                suggestions.AddRange(tables.Select(t => new TableSuggestion
                {
                    TableName = t.TableName,
                    SchemaName = t.SchemaName,
                    ConfidenceScore = entity.ConfidenceScore,
                    MatchReason = $"Matched entity: {entity.OriginalText}"
                }));
            }

            // Get suggestions from domain
            if (!string.IsNullOrEmpty(profile.Domain.Name))
            {
                var domainTables = await _tableRepository.GetByDomainAsync(profile.Domain.Name);
                suggestions.AddRange(domainTables.Select(t => new TableSuggestion
                {
                    TableName = t.TableName,
                    SchemaName = t.SchemaName,
                    ConfidenceScore = profile.Domain.RelevanceScore * 0.8,
                    MatchReason = $"Domain match: {profile.Domain.Name}"
                }));
            }

            return suggestions
                .GroupBy(s => $"{s.SchemaName}.{s.TableName}")
                .Select(g => g.OrderByDescending(s => s.ConfidenceScore).First())
                .OrderByDescending(s => s.ConfidenceScore)
                .Take(10)
                .ToList();
        }

        private async Task<List<DomainSuggestion>> GetDomainSuggestions(BusinessContextProfile profile)
        {
            var allDomains = await _domainRepository.GetActiveDomainsAsync();
            var suggestions = new List<DomainSuggestion>();

            foreach (var domain in allDomains)
            {
                var relevanceScore = CalculateDomainRelevance(domain, profile);
                if (relevanceScore > 0.3)
                {
                    suggestions.Add(new DomainSuggestion
                    {
                        DomainName = domain.DomainName,
                        RelevanceScore = relevanceScore,
                        Reason = GenerateDomainMatchReason(domain, profile)
                    });
                }
            }

            return suggestions
                .OrderByDescending(s => s.RelevanceScore)
                .Take(5)
                .ToList();
        }

        private List<string> GenerateRecommendations(BusinessContextProfile profile)
        {
            var recommendations = new List<string>();

            if (profile.ConfidenceScore < 0.5)
            {
                recommendations.Add("Consider adding more specific table or column names to your question");
            }

            if (!profile.TimeContext.HasValue && 
                profile.Intent.Type == IntentType.Trend)
            {
                recommendations.Add("Specify a time range for trend analysis (e.g., 'last month', 'year to date')");
            }

            if (profile.Entities.Count == 0)
            {
                recommendations.Add("Include specific business terms or metrics in your question");
            }

            if (profile.Intent.Type == IntentType.Comparison && 
                profile.ComparisonTerms.Count == 0)
            {
                recommendations.Add("Clarify what you want to compare (e.g., 'compare X vs Y')");
            }

            return recommendations;
        }

        private double CalculateDomainRelevance(BusinessDomainEntity domain, BusinessContextProfile profile)
        {
            // Implementation would calculate relevance based on:
            // - Keyword matches
            // - Entity overlap
            // - Business term matches
            return 0.75; // Placeholder
        }

        private string GenerateDomainMatchReason(BusinessDomainEntity domain, BusinessContextProfile profile)
        {
            // Generate explanation for why domain was suggested
            return $"Keywords match: {string.Join(", ", domain.KeyConcepts.Take(3))}";
        }
    }
}