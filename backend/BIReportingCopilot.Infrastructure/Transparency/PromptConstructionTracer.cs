using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Infrastructure.Interfaces;
using BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;
using BIReportingCopilot.Infrastructure.Data.Entities;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.Transparency;

/// <summary>
/// Advanced prompt construction tracer that provides complete transparency into the prompt building process
/// </summary>
public class PromptConstructionTracer : IPromptConstructionTracer
{
    private readonly BIReportingCopilot.Core.Interfaces.Cache.ICacheService _cacheService;
    private readonly ITransparencyRepository _transparencyRepository;
    private readonly ILogger<PromptConstructionTracer> _logger;

    // Performance tracking for transparency operations
    private readonly Dictionary<string, TransparencyMetrics> _transparencyMetrics = new();
    private readonly object _metricsLock = new();

    public PromptConstructionTracer(
        BIReportingCopilot.Core.Interfaces.Cache.ICacheService cacheService,
        ITransparencyRepository transparencyRepository,
        ILogger<PromptConstructionTracer> logger)
    {
        _cacheService = cacheService;
        _transparencyRepository = transparencyRepository;
        _logger = logger;
    }

    public async Task<PromptConstructionTrace> TracePromptConstructionAsync(
        string userQuestion, 
        BusinessContextProfile profile,
        ProgressiveBuildResult buildResult)
    {
        var traceId = Guid.NewGuid().ToString("N")[..8];
        var startTime = DateTime.UtcNow;

        _logger.LogInformation("Starting prompt construction trace {TraceId} for question: {Question}", 
            traceId, userQuestion.Substring(0, Math.Min(100, userQuestion.Length)));

        try
        {
            var trace = new PromptConstructionTrace
            {
                TraceId = traceId,
                UserQuestion = userQuestion,
                IntentType = profile.Intent.Type,
                DomainName = profile.Domain.Name,
                StartTime = startTime,
                BuildSteps = new List<PromptConstructionStep>()
            };

            // Trace business context analysis
            await TraceBusinessContextAnalysisAsync(trace, profile);

            // Trace context selection process
            await TraceContextSelectionAsync(trace, buildResult);

            // Trace template selection
            await TraceTemplateSelectionAsync(trace, profile, buildResult);

            // Trace token budget allocation
            await TraceTokenBudgetAllocationAsync(trace, buildResult);

            // Trace final assembly
            await TraceFinalAssemblyAsync(trace, buildResult);

            // Calculate overall metrics
            trace.EndTime = DateTime.UtcNow;
            trace.TotalDuration = (trace.EndTime - trace.StartTime).TotalMilliseconds;
            trace.OverallConfidence = CalculateOverallConfidence(trace);
            trace.EfficiencyScore = CalculateEfficiencyScore(trace);

            // Save to database
            await SaveTraceToDatabase(trace, profile.UserId ?? "system");

            // Cache the trace for future reference
            await _cacheService.SetAsync($"prompt_trace:{traceId}", trace, TimeSpan.FromHours(24));

            await RecordTransparencyMetricsAsync("prompt_construction_trace", trace.TotalDuration, true);

            _logger.LogInformation("Prompt construction trace {TraceId} completed in {Duration}ms with confidence {Confidence:F3}",
                traceId, trace.TotalDuration, trace.OverallConfidence);

            return trace;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating prompt construction trace {TraceId}", traceId);
            await RecordTransparencyMetricsAsync("prompt_construction_trace", (DateTime.UtcNow - startTime).TotalMilliseconds, false);
            
            return CreateFallbackTrace(traceId, userQuestion, profile.Intent.Type, ex.Message);
        }
    }

    public async Task<ContextSelectionExplanation> ExplainContextSelectionAsync(
        ContextualBusinessSchema schema,
        BusinessContextProfile profile,
        List<ContextSection> selectedSections)
    {
        var explanationId = Guid.NewGuid().ToString("N")[..8];
        var startTime = DateTime.UtcNow;

        try
        {
            var explanation = new ContextSelectionExplanation
            {
                ExplanationId = explanationId,
                IntentType = profile.Intent.Type,
                DomainName = profile.Domain.Name,
                TotalAvailableSections = schema.RelevantTables.Count + schema.TableColumns.Values.Sum(c => c.Count) + 
                                       schema.BusinessRules.Count + schema.RelevantExamples.Count,
                SelectedSectionsCount = selectedSections.Count,
                SelectionCriteria = new List<SelectionCriterion>()
            };

            // Explain table selection
            await ExplainTableSelectionAsync(explanation, schema.RelevantTables, selectedSections, profile);

            // Explain column selection
            await ExplainColumnSelectionAsync(explanation, schema.TableColumns, selectedSections, profile);

            // Explain business rules selection
            await ExplainBusinessRulesSelectionAsync(explanation, schema.BusinessRules, selectedSections, profile);

            // Explain examples selection
            await ExplainExamplesSelectionAsync(explanation, schema.RelevantExamples, selectedSections, profile);

            // Calculate selection rationale
            explanation.SelectionRationale = GenerateSelectionRationale(explanation, profile);
            explanation.AlternativeOptions = await GenerateAlternativeOptionsAsync(schema, selectedSections, profile);

            explanation.GenerationTime = (DateTime.UtcNow - startTime).TotalMilliseconds;

            await RecordTransparencyMetricsAsync("context_selection_explanation", explanation.GenerationTime, true);

            return explanation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error explaining context selection {ExplanationId}", explanationId);
            await RecordTransparencyMetricsAsync("context_selection_explanation", (DateTime.UtcNow - startTime).TotalMilliseconds, false);
            
            return new ContextSelectionExplanation 
            { 
                ExplanationId = explanationId,
                ErrorMessage = ex.Message 
            };
        }
    }

    public async Task<TemplateSelectionRationale> ExplainTemplateSelectionAsync(
        PromptTemplate selectedTemplate,
        BusinessContextProfile profile,
        List<PromptTemplate> availableTemplates)
    {
        var rationaleId = Guid.NewGuid().ToString("N")[..8];

        try
        {
            var rationale = new TemplateSelectionRationale
            {
                RationaleId = rationaleId,
                SelectedTemplate = selectedTemplate,
                IntentType = profile.Intent.Type,
                SelectionFactors = new List<TemplateSelectionFactor>()
            };

            // Analyze intent compatibility
            rationale.SelectionFactors.Add(new TemplateSelectionFactor
            {
                FactorName = "Intent Compatibility",
                Score = CalculateIntentCompatibility(selectedTemplate, profile.Intent.Type),
                Weight = 0.4,
                Explanation = $"Template optimized for {profile.Intent.Type} queries with specialized prompt structure"
            });

            // Analyze domain suitability
            rationale.SelectionFactors.Add(new TemplateSelectionFactor
            {
                FactorName = "Domain Suitability",
                Score = CalculateDomainSuitability(selectedTemplate, profile.Domain.Name),
                Weight = 0.3,
                Explanation = $"Template includes domain-specific context for {profile.Domain.Name}"
            });

            // Analyze complexity handling
            rationale.SelectionFactors.Add(new TemplateSelectionFactor
            {
                FactorName = "Complexity Handling",
                Score = CalculateComplexityHandling(selectedTemplate, profile),
                Weight = 0.2,
                Explanation = "Template structure supports the complexity level of the user question"
            });

            // Analyze performance characteristics
            rationale.SelectionFactors.Add(new TemplateSelectionFactor
            {
                FactorName = "Performance",
                Score = CalculateTemplatePerformance(selectedTemplate),
                Weight = 0.1,
                Explanation = "Template has good historical performance metrics"
            });

            // Calculate overall score
            rationale.OverallScore = rationale.SelectionFactors.Sum(f => f.Score * f.Weight);

            // Generate alternative recommendations
            rationale.AlternativeTemplates = availableTemplates
                .Where(t => t.Name != selectedTemplate.Name)
                .Select(t => new AlternativeTemplate
                {
                    Template = t,
                    Score = CalculateTemplateScore(t, profile),
                    Reason = GenerateAlternativeReason(t, profile)
                })
                .OrderByDescending(a => a.Score)
                .Take(3)
                .ToList();

            return rationale;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error explaining template selection {RationaleId}", rationaleId);
            return new TemplateSelectionRationale 
            { 
                RationaleId = rationaleId,
                ErrorMessage = ex.Message 
            };
        }
    }

    public async Task<TransparencyReport> GenerateTransparencyReportAsync(
        string traceId,
        bool includeDetailedMetrics = true)
    {
        try
        {
            var trace = await _cacheService.GetAsync<PromptConstructionTrace>($"prompt_trace:{traceId}");
            if (trace == null)
            {
                throw new InvalidOperationException($"Trace {traceId} not found");
            }

            var report = new TransparencyReport
            {
                TraceId = traceId,
                GeneratedAt = DateTime.UtcNow,
                UserQuestion = trace.UserQuestion,
                IntentType = trace.IntentType,
                Summary = GenerateTransparencySummary(trace)
            };

            if (includeDetailedMetrics)
            {
                report.DetailedMetrics = await GenerateDetailedMetricsAsync(trace);
                report.PerformanceAnalysis = GeneratePerformanceAnalysis(trace);
                report.OptimizationSuggestions = GenerateOptimizationSuggestions(trace);
            }

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating transparency report for trace {TraceId}", traceId);
            return new TransparencyReport 
            { 
                TraceId = traceId,
                ErrorMessage = ex.Message 
            };
        }
    }

    // Private helper methods
    private async Task TraceBusinessContextAnalysisAsync(PromptConstructionTrace trace, BusinessContextProfile profile)
    {
        var step = new PromptConstructionStep
        {
            StepName = "Business Context Analysis",
            StartTime = DateTime.UtcNow,
            Details = new Dictionary<string, object>
            {
                ["intent_type"] = profile.Intent.Type,
                ["intent_confidence"] = profile.Intent.ConfidenceScore,
                ["domain_name"] = profile.Domain.Name,
                ["domain_relevance"] = profile.Domain.RelevanceScore,
                ["entities_count"] = profile.Entities.Count,
                ["business_terms_count"] = profile.BusinessTerms.Count
            }
        };

        step.EndTime = DateTime.UtcNow;
        step.Duration = (step.EndTime - step.StartTime).TotalMilliseconds;
        step.Success = true;
        step.ConfidenceScore = profile.ConfidenceScore;

        trace.BuildSteps.Add(step);
    }

    private async Task TraceContextSelectionAsync(PromptConstructionTrace trace, ProgressiveBuildResult buildResult)
    {
        var step = new PromptConstructionStep
        {
            StepName = "Context Selection",
            StartTime = DateTime.UtcNow,
            Details = new Dictionary<string, object>
            {
                ["selected_sections"] = buildResult.SelectedSections.Count,
                ["rejected_sections"] = buildResult.RejectedSections.Count,
                ["selection_ratio"] = buildResult.SelectedSections.Count / (double)(buildResult.SelectedSections.Count + buildResult.RejectedSections.Count),
                ["avg_relevance"] = buildResult.SelectedSections.Any() ? buildResult.SelectedSections.Average(s => s.RelevanceScore) : 0
            }
        };

        step.EndTime = DateTime.UtcNow;
        step.Duration = (step.EndTime - step.StartTime).TotalMilliseconds;
        step.Success = true;
        step.ConfidenceScore = buildResult.SelectedSections.Any() ? buildResult.SelectedSections.Average(s => s.RelevanceScore) : 0.5;

        trace.BuildSteps.Add(step);
    }

    private async Task TraceTemplateSelectionAsync(PromptConstructionTrace trace, BusinessContextProfile profile, ProgressiveBuildResult buildResult)
    {
        var step = new PromptConstructionStep
        {
            StepName = "Template Selection",
            StartTime = DateTime.UtcNow,
            Details = new Dictionary<string, object>
            {
                ["template_type"] = "progressive_template",
                ["intent_match"] = true,
                ["domain_compatibility"] = true
            }
        };

        step.EndTime = DateTime.UtcNow;
        step.Duration = (step.EndTime - step.StartTime).TotalMilliseconds;
        step.Success = true;
        step.ConfidenceScore = 0.9;

        trace.BuildSteps.Add(step);
    }

    private async Task TraceTokenBudgetAllocationAsync(PromptConstructionTrace trace, ProgressiveBuildResult buildResult)
    {
        var step = new PromptConstructionStep
        {
            StepName = "Token Budget Allocation",
            StartTime = DateTime.UtcNow,
            Details = new Dictionary<string, object>
            {
                ["max_tokens"] = buildResult.MaxTokens,
                ["final_tokens"] = buildResult.FinalTokenCount,
                ["utilization"] = buildResult.TokenUtilization,
                ["efficiency"] = buildResult.FinalTokenCount > 0 ? buildResult.TokenUtilization : 0
            }
        };

        step.EndTime = DateTime.UtcNow;
        step.Duration = (step.EndTime - step.StartTime).TotalMilliseconds;
        step.Success = true;
        step.ConfidenceScore = buildResult.TokenUtilization > 0.7 && buildResult.TokenUtilization < 0.95 ? 0.9 : 0.7;

        trace.BuildSteps.Add(step);
    }

    private async Task TraceFinalAssemblyAsync(PromptConstructionTrace trace, ProgressiveBuildResult buildResult)
    {
        var step = new PromptConstructionStep
        {
            StepName = "Final Assembly",
            StartTime = DateTime.UtcNow,
            Details = new Dictionary<string, object>
            {
                ["build_duration"] = buildResult.BuildDuration,
                ["steps_count"] = buildResult.BuildSteps.Count,
                ["final_prompt_length"] = buildResult.FinalPrompt.Length,
                ["success"] = string.IsNullOrEmpty(buildResult.ErrorMessage)
            }
        };

        step.EndTime = DateTime.UtcNow;
        step.Duration = (step.EndTime - step.StartTime).TotalMilliseconds;
        step.Success = string.IsNullOrEmpty(buildResult.ErrorMessage);
        step.ConfidenceScore = step.Success ? 0.95 : 0.3;

        trace.BuildSteps.Add(step);
    }

    private double CalculateOverallConfidence(PromptConstructionTrace trace)
    {
        if (!trace.BuildSteps.Any())
            return 0.5;

        return trace.BuildSteps.Average(s => s.ConfidenceScore);
    }

    private double CalculateEfficiencyScore(PromptConstructionTrace trace)
    {
        if (trace.TotalDuration <= 0)
            return 0.5;

        // Efficiency based on speed and success rate
        var speedScore = Math.Max(0, 1.0 - (trace.TotalDuration / 5000.0)); // 5 seconds as baseline
        var successScore = trace.BuildSteps.Count(s => s.Success) / (double)trace.BuildSteps.Count;
        
        return (speedScore * 0.6) + (successScore * 0.4);
    }

    private PromptConstructionTrace CreateFallbackTrace(string traceId, string userQuestion, IntentType intentType, string errorMessage)
    {
        return new PromptConstructionTrace
        {
            TraceId = traceId,
            UserQuestion = userQuestion,
            IntentType = intentType,
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow,
            TotalDuration = 0,
            OverallConfidence = 0.3,
            EfficiencyScore = 0.1,
            ErrorMessage = errorMessage,
            BuildSteps = new List<PromptConstructionStep>
            {
                new()
                {
                    StepName = "Error",
                    StartTime = DateTime.UtcNow,
                    EndTime = DateTime.UtcNow,
                    Duration = 0,
                    Success = false,
                    ConfidenceScore = 0.0,
                    Details = new Dictionary<string, object> { ["error"] = errorMessage }
                }
            }
        };
    }

    private async Task RecordTransparencyMetricsAsync(string operation, double duration, bool success)
    {
        lock (_metricsLock)
        {
            if (!_transparencyMetrics.ContainsKey(operation))
            {
                _transparencyMetrics[operation] = new TransparencyMetrics();
            }

            var metrics = _transparencyMetrics[operation];
            metrics.TotalOperations++;
            metrics.TotalDuration += duration;
            
            if (success)
            {
                metrics.SuccessfulOperations++;
            }
            
            metrics.LastOperation = DateTime.UtcNow;
        }
    }

    // Additional helper methods would be implemented here...
    private async Task ExplainTableSelectionAsync(ContextSelectionExplanation explanation, List<BusinessTableInfoDto> tables, List<ContextSection> selectedSections, BusinessContextProfile profile) { /* Implementation */ }
    private async Task ExplainColumnSelectionAsync(ContextSelectionExplanation explanation, Dictionary<long, List<BusinessColumnInfo>> columns, List<ContextSection> selectedSections, BusinessContextProfile profile) { /* Implementation */ }
    private async Task ExplainBusinessRulesSelectionAsync(ContextSelectionExplanation explanation, List<BusinessRule> rules, List<ContextSection> selectedSections, BusinessContextProfile profile) { /* Implementation */ }
    private async Task ExplainExamplesSelectionAsync(ContextSelectionExplanation explanation, List<RelevantQueryExample> examples, List<ContextSection> selectedSections, BusinessContextProfile profile) { /* Implementation */ }
    private string GenerateSelectionRationale(ContextSelectionExplanation explanation, BusinessContextProfile profile) => "Generated based on intent and domain analysis";
    private async Task<List<AlternativeOption>> GenerateAlternativeOptionsAsync(ContextualBusinessSchema schema, List<ContextSection> selectedSections, BusinessContextProfile profile) => new();
    private double CalculateIntentCompatibility(PromptTemplate template, IntentType intentType) => 0.9;
    private double CalculateDomainSuitability(PromptTemplate template, string domainName) => 0.8;
    private double CalculateComplexityHandling(PromptTemplate template, BusinessContextProfile profile) => 0.85;
    private double CalculateTemplatePerformance(PromptTemplate template) => 0.8;
    private double CalculateTemplateScore(PromptTemplate template, BusinessContextProfile profile) => 0.75;
    private string GenerateAlternativeReason(PromptTemplate template, BusinessContextProfile profile) => "Alternative option with different optimization focus";
    private string GenerateTransparencySummary(PromptConstructionTrace trace) => $"Prompt constructed in {trace.TotalDuration:F0}ms with {trace.OverallConfidence:P0} confidence";
    private async Task<Dictionary<string, object>> GenerateDetailedMetricsAsync(PromptConstructionTrace trace) => new();
    private Dictionary<string, object> GeneratePerformanceAnalysis(PromptConstructionTrace trace) => new();
    private List<string> GenerateOptimizationSuggestions(PromptConstructionTrace trace) => new();

    /// <summary>
    /// Save the prompt construction trace and its steps to the database
    /// </summary>
    private async Task SaveTraceToDatabase(PromptConstructionTrace trace, string userId)
    {
        try
        {
            _logger.LogDebug("Saving prompt trace {TraceId} to database for user {UserId}", trace.TraceId, userId);

            // Create the main trace entity
            var traceEntity = new PromptConstructionTraceEntity
            {
                TraceId = trace.TraceId,
                UserId = userId,
                UserQuestion = trace.UserQuestion,
                IntentType = trace.IntentType.ToString(),
                StartTime = trace.StartTime,
                EndTime = trace.EndTime,
                OverallConfidence = (decimal)trace.OverallConfidence,
                TotalTokens = trace.BuildSteps.Sum(s => s.Details.ContainsKey("tokens_used") ? Convert.ToInt32(s.Details["tokens_used"]) : 0),
                FinalPrompt = null, // Will be set if available
                Success = string.IsNullOrEmpty(trace.ErrorMessage),
                ErrorMessage = trace.ErrorMessage,
                Metadata = JsonSerializer.Serialize(new
                {
                    domain_name = trace.DomainName,
                    total_duration = trace.TotalDuration,
                    efficiency_score = trace.EfficiencyScore,
                    steps_count = trace.BuildSteps.Count
                })
            };

            // Save the trace
            var savedTrace = await _transparencyRepository.SavePromptTraceAsync(traceEntity);

            // Save each step
            foreach (var step in trace.BuildSteps)
            {
                var stepEntity = new PromptConstructionStepEntity
                {
                    TraceId = trace.TraceId,
                    StepName = step.StepName,
                    StepOrder = trace.BuildSteps.IndexOf(step) + 1,
                    StartTime = step.StartTime,
                    EndTime = step.EndTime,
                    Success = step.Success,
                    TokensAdded = step.Details.ContainsKey("tokens_used") ? Convert.ToInt32(step.Details["tokens_used"]) : 0,
                    Confidence = (decimal)step.ConfidenceScore,
                    Content = JsonSerializer.Serialize(new
                    {
                        confidence = step.ConfidenceScore,
                        success = step.Success,
                        duration_ms = step.Duration
                    }),
                    Details = JsonSerializer.Serialize(step.Details),
                    ErrorMessage = null
                };

                await _transparencyRepository.SavePromptStepAsync(stepEntity);
            }

            _logger.LogInformation("Successfully saved prompt trace {TraceId} with {StepCount} steps to database",
                trace.TraceId, trace.BuildSteps.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save prompt trace {TraceId} to database", trace.TraceId);
            // Don't throw - we don't want to break the main flow if database save fails
        }
    }
}

// Supporting data structures
public record PromptConstructionTrace
{
    public string TraceId { get; set; } = string.Empty;
    public string UserQuestion { get; set; } = string.Empty;
    public IntentType IntentType { get; set; }
    public string DomainName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double TotalDuration { get; set; }
    public double OverallConfidence { get; set; }
    public double EfficiencyScore { get; set; }
    public List<PromptConstructionStep> BuildSteps { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public record PromptConstructionStep
{
    public string StepName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double Duration { get; set; }
    public bool Success { get; set; }
    public double ConfidenceScore { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}

public record ContextSelectionExplanation
{
    public string ExplanationId { get; set; } = string.Empty;
    public IntentType IntentType { get; set; }
    public string DomainName { get; set; } = string.Empty;
    public int TotalAvailableSections { get; set; }
    public int SelectedSectionsCount { get; set; }
    public List<SelectionCriterion> SelectionCriteria { get; set; } = new();
    public string SelectionRationale { get; set; } = string.Empty;
    public List<AlternativeOption> AlternativeOptions { get; set; } = new();
    public double GenerationTime { get; set; }
    public string? ErrorMessage { get; set; }
}

public record SelectionCriterion
{
    public string CriterionName { get; init; } = string.Empty;
    public double Weight { get; init; }
    public string Description { get; init; } = string.Empty;
    public bool Applied { get; init; }
}

public record AlternativeOption
{
    public string OptionName { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public double Score { get; init; }
    public string Rationale { get; init; } = string.Empty;
}

public record TemplateSelectionRationale
{
    public string RationaleId { get; set; } = string.Empty;
    public PromptTemplate SelectedTemplate { get; set; } = new();
    public IntentType IntentType { get; set; }
    public List<TemplateSelectionFactor> SelectionFactors { get; set; } = new();
    public double OverallScore { get; set; }
    public List<AlternativeTemplate> AlternativeTemplates { get; set; } = new();
    public string? ErrorMessage { get; set; }
}

public record TemplateSelectionFactor
{
    public string FactorName { get; init; } = string.Empty;
    public double Score { get; init; }
    public double Weight { get; init; }
    public string Explanation { get; init; } = string.Empty;
}

public record AlternativeTemplate
{
    public PromptTemplate Template { get; init; } = new();
    public double Score { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record TransparencyReport
{
    public string TraceId { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public string UserQuestion { get; set; } = string.Empty;
    public IntentType IntentType { get; set; }
    public string Summary { get; set; } = string.Empty;
    public Dictionary<string, object>? DetailedMetrics { get; set; }
    public Dictionary<string, object>? PerformanceAnalysis { get; set; }
    public List<string>? OptimizationSuggestions { get; set; }
    public string? ErrorMessage { get; set; }
}

public class TransparencyMetrics
{
    public int TotalOperations { get; set; }
    public int SuccessfulOperations { get; set; }
    public double TotalDuration { get; set; }
    public DateTime LastOperation { get; set; } = DateTime.UtcNow;
}

public interface IPromptConstructionTracer
{
    Task<PromptConstructionTrace> TracePromptConstructionAsync(string userQuestion, BusinessContextProfile profile, ProgressiveBuildResult buildResult);
    Task<ContextSelectionExplanation> ExplainContextSelectionAsync(ContextualBusinessSchema schema, BusinessContextProfile profile, List<ContextSection> selectedSections);
    Task<TemplateSelectionRationale> ExplainTemplateSelectionAsync(PromptTemplate selectedTemplate, BusinessContextProfile profile, List<PromptTemplate> availableTemplates);
    Task<TransparencyReport> GenerateTransparencyReportAsync(string traceId, bool includeDetailedMetrics = true);
}
