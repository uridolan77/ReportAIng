using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Extensions;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Advanced token budget management system for optimal context selection within token limits
/// </summary>
public class TokenBudgetManager : ITokenBudgetManager
{
    private readonly ICacheService _cacheService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TokenBudgetManager> _logger;

    // Token counting patterns and weights
    private static readonly Regex WordPattern = new(@"\b\w+\b", RegexOptions.Compiled);
    private static readonly Regex PunctuationPattern = new(@"[^\w\s]", RegexOptions.Compiled);
    
    // Token estimation multipliers for different content types
    private static readonly Dictionary<string, double> ContentTypeMultipliers = new()
    {
        ["sql"] = 1.3,           // SQL has more complex tokenization
        ["json"] = 1.2,          // JSON structure adds tokens
        ["schema"] = 1.1,        // Schema definitions are moderately complex
        ["business_context"] = 1.0,  // Natural language baseline
        ["examples"] = 1.15,     // Examples include code and explanations
        ["rules"] = 1.05,        // Business rules are mostly natural language
        ["glossary"] = 1.0       // Glossary terms are natural language
    };

    // Budget allocation strategies for different intent types
    private static readonly Dictionary<IntentType, BudgetAllocationStrategy> AllocationStrategies = new()
    {
        [IntentType.Aggregation] = new()
        {
            SchemaContextPercentage = 0.40,
            BusinessContextPercentage = 0.25,
            ExamplesPercentage = 0.20,
            RulesPercentage = 0.10,
            GlossaryPercentage = 0.05
        },
        [IntentType.Trend] = new()
        {
            SchemaContextPercentage = 0.35,
            BusinessContextPercentage = 0.30,
            ExamplesPercentage = 0.20,
            RulesPercentage = 0.10,
            GlossaryPercentage = 0.05
        },
        [IntentType.Comparison] = new()
        {
            SchemaContextPercentage = 0.45,
            BusinessContextPercentage = 0.25,
            ExamplesPercentage = 0.15,
            RulesPercentage = 0.10,
            GlossaryPercentage = 0.05
        },
        [IntentType.Detail] = new()
        {
            SchemaContextPercentage = 0.50,
            BusinessContextPercentage = 0.20,
            ExamplesPercentage = 0.15,
            RulesPercentage = 0.10,
            GlossaryPercentage = 0.05
        },
        [IntentType.Exploratory] = new()
        {
            SchemaContextPercentage = 0.30,
            BusinessContextPercentage = 0.35,
            ExamplesPercentage = 0.20,
            RulesPercentage = 0.10,
            GlossaryPercentage = 0.05
        },
        [IntentType.Operational] = new()
        {
            SchemaContextPercentage = 0.40,
            BusinessContextPercentage = 0.30,
            ExamplesPercentage = 0.15,
            RulesPercentage = 0.10,
            GlossaryPercentage = 0.05
        },
        [IntentType.Analytical] = new()
        {
            SchemaContextPercentage = 0.35,
            BusinessContextPercentage = 0.30,
            ExamplesPercentage = 0.20,
            RulesPercentage = 0.10,
            GlossaryPercentage = 0.05
        }
    };

    // Performance tracking
    private readonly Dictionary<string, TokenBudgetMetrics> _budgetMetrics = new();
    private readonly object _metricsLock = new();

    public TokenBudgetManager(
        ICacheService cacheService,
        IConfiguration configuration,
        ILogger<TokenBudgetManager> logger)
    {
        _cacheService = cacheService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<TokenBudget> CreateTokenBudgetAsync(
        BusinessContextProfile profile,
        int maxTokens = 4000,
        int reservedResponseTokens = 500)
    {
        var cacheKey = $"token_budget:{profile.Intent.Type}:{maxTokens}:{reservedResponseTokens}";
        
        var (found, cachedBudget) = await _cacheService.TryGetAsync<TokenBudget>(cacheKey);
        if (found && cachedBudget != null)
        {
            _logger.LogDebug("Retrieved cached token budget for intent {Intent}", profile.Intent.Type);
            return cachedBudget;
        }

        var startTime = DateTime.UtcNow;

        try
        {
            // Calculate base prompt tokens (system prompt, user question, template structure)
            var basePromptTokens = await EstimateBasePromptTokensAsync(profile.OriginalQuestion);
            
            // Calculate available tokens for context
            var availableContextTokens = maxTokens - basePromptTokens - reservedResponseTokens;
            
            if (availableContextTokens <= 0)
            {
                _logger.LogWarning("No tokens available for context. Max: {Max}, Base: {Base}, Reserved: {Reserved}",
                    maxTokens, basePromptTokens, reservedResponseTokens);
                
                return CreateMinimalBudget(profile.Intent.Type);
            }

            // Get allocation strategy for the intent type
            var strategy = GetAllocationStrategy(profile.Intent.Type);
            
            // Create budget allocation
            var budget = new TokenBudget
            {
                IntentType = profile.Intent.Type,
                MaxTotalTokens = maxTokens,
                BasePromptTokens = basePromptTokens,
                ReservedResponseTokens = reservedResponseTokens,
                AvailableContextTokens = availableContextTokens,
                
                SchemaContextBudget = (int)(availableContextTokens * strategy.SchemaContextPercentage),
                BusinessContextBudget = (int)(availableContextTokens * strategy.BusinessContextPercentage),
                ExamplesBudget = (int)(availableContextTokens * strategy.ExamplesPercentage),
                RulesBudget = (int)(availableContextTokens * strategy.RulesPercentage),
                GlossaryBudget = (int)(availableContextTokens * strategy.GlossaryPercentage),
                
                CreatedAt = DateTime.UtcNow,
                AllocationStrategy = strategy
            };

            // Apply domain-specific adjustments
            budget = ApplyDomainAdjustments(budget, profile.Domain.Name);
            
            // Apply user-specific adjustments
            budget = await ApplyUserAdjustmentsAsync(budget, profile.UserId);
            
            // Cache the budget for 1 hour
            await _cacheService.SetAsync(cacheKey, budget, TimeSpan.FromHours(1));
            
            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            await RecordBudgetMetricsAsync("budget_creation", duration, budget);
            
            _logger.LogInformation("Created token budget for {Intent}: Total={Total}, Context={Context}, Schema={Schema}, Business={Business}",
                profile.Intent.Type, budget.MaxTotalTokens, budget.AvailableContextTokens, 
                budget.SchemaContextBudget, budget.BusinessContextBudget);

            return budget;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating token budget");
            return CreateMinimalBudget(profile.Intent.Type);
        }
    }

    public int CountTokens(string text, string contentType = "business_context")
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        var cacheKey = $"token_count:{text.GetHashCode()}:{contentType}";

        // Simple cache check (in-memory for performance) - synchronous for performance
        var cachedCount = _cacheService.GetAsync<object>(cacheKey).Result;
        if (cachedCount is int count)
        {
            return count;
        }

        try
        {
            // Estimate tokens using word count and content type multiplier
            var wordCount = WordPattern.Matches(text).Count;
            var punctuationCount = PunctuationPattern.Matches(text).Count;
            
            // Base token estimation: words + punctuation/2 (punctuation often combines)
            var baseTokens = wordCount + (punctuationCount / 2);
            
            // Apply content type multiplier
            var multiplier = ContentTypeMultipliers.GetValueOrDefault(contentType, 1.0);
            var estimatedTokens = (int)Math.Ceiling(baseTokens * multiplier);
            
            // Cache the result for 1 hour
            _cacheService.SetAsync(cacheKey, estimatedTokens, TimeSpan.FromHours(1));
            
            return estimatedTokens;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error counting tokens, using fallback estimation");
            return text.Length / 4; // Rough fallback: ~4 characters per token
        }
    }

    public async Task<TokenAllocationResult> AllocateTokensAsync(
        TokenBudget budget,
        List<ContextSection> availableSections)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            var result = new TokenAllocationResult
            {
                Budget = budget,
                AllocatedSections = new List<ContextSection>(),
                RejectedSections = new List<ContextSection>(),
                TokenUtilization = new Dictionary<string, int>()
            };

            // Group sections by type
            var sectionsByType = availableSections.GroupBy(s => s.Type).ToDictionary(g => g.Key, g => g.ToList());

            // Allocate tokens for each section type based on budget
            await AllocateSchemaContextAsync(result, sectionsByType.GetValueOrDefault("schema", new List<ContextSection>()));
            await AllocateBusinessContextAsync(result, sectionsByType.GetValueOrDefault("business", new List<ContextSection>()));
            await AllocateExamplesAsync(result, sectionsByType.GetValueOrDefault("examples", new List<ContextSection>()));
            await AllocateRulesAsync(result, sectionsByType.GetValueOrDefault("rules", new List<ContextSection>()));
            await AllocateGlossaryAsync(result, sectionsByType.GetValueOrDefault("glossary", new List<ContextSection>()));

            // Calculate final utilization
            result.TotalAllocatedTokens = result.AllocatedSections.Sum(s => s.TokenCount);
            result.UtilizationPercentage = budget.AvailableContextTokens > 0 ? 
                (double)result.TotalAllocatedTokens / budget.AvailableContextTokens : 0;

            var duration = (DateTime.UtcNow - startTime).TotalMilliseconds;
            await RecordAllocationMetricsAsync("token_allocation", duration, result);

            _logger.LogInformation("Token allocation completed: {Allocated}/{Available} tokens ({Utilization:P1}), {AcceptedSections} sections",
                result.TotalAllocatedTokens, budget.AvailableContextTokens, result.UtilizationPercentage, result.AllocatedSections.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in token allocation");
            return new TokenAllocationResult { Budget = budget };
        }
    }

    public async Task<TokenOptimizationReport> GenerateOptimizationReportAsync(
        IntentType? intentFilter = null,
        TimeSpan? timeWindow = null)
    {
        var report = new TokenOptimizationReport
        {
            GeneratedAt = DateTime.UtcNow,
            IntentFilter = intentFilter,
            TimeWindow = timeWindow ?? TimeSpan.FromDays(7)
        };

        lock (_metricsLock)
        {
            var filteredMetrics = _budgetMetrics.Where(kvp =>
            {
                // Apply filters if specified
                return true; // Simplified for now
            }).ToList();

            foreach (var (operation, metrics) in filteredMetrics)
            {
                var avgDuration = metrics.TotalOperations > 0 ? metrics.TotalDuration / metrics.TotalOperations : 0;
                var avgUtilization = metrics.TotalOperations > 0 ? metrics.TotalUtilization / metrics.TotalOperations : 0;

                report.OperationMetrics.Add(new TokenOperationMetrics
                {
                    OperationType = operation,
                    TotalOperations = metrics.TotalOperations,
                    AverageDuration = avgDuration,
                    AverageTokenUtilization = avgUtilization,
                    LastOperation = metrics.LastOperation
                });
            }
        }

        return report;
    }

    private async Task<int> EstimateBasePromptTokensAsync(string userQuestion)
    {
        // Estimate tokens for system prompt, user question, and template structure
        var systemPromptTokens = 150; // Typical system prompt size
        var userQuestionTokens = CountTokens(userQuestion, "business_context");
        var templateStructureTokens = 100; // Template placeholders and structure
        
        return systemPromptTokens + userQuestionTokens + templateStructureTokens;
    }

    private BudgetAllocationStrategy GetAllocationStrategy(IntentType intentType)
    {
        return AllocationStrategies.GetValueOrDefault(intentType, AllocationStrategies[IntentType.Analytical]);
    }

    private TokenBudget ApplyDomainAdjustments(TokenBudget budget, string domainName)
    {
        // Domain-specific adjustments
        return domainName.ToLower() switch
        {
            "gaming" => budget with
            {
                // Gaming domain benefits from more examples
                ExamplesBudget = (int)(budget.ExamplesBudget * 1.2),
                BusinessContextBudget = (int)(budget.BusinessContextBudget * 0.9)
            },
            "financial" => budget with
            {
                // Financial domain needs more rules and compliance context
                RulesBudget = (int)(budget.RulesBudget * 1.5),
                ExamplesBudget = (int)(budget.ExamplesBudget * 0.8)
            },
            "retail" => budget, // Retail benefits from balanced approach
            _ => budget // General domain - no adjustments
        };
    }

    private async Task<TokenBudget> ApplyUserAdjustmentsAsync(TokenBudget budget, string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return budget;

        try
        {
            // Get user preferences (would come from user settings/analytics)
            var userPrefs = await GetUserTokenPreferencesAsync(userId);
            
            if (userPrefs != null)
            {
                // Apply user-specific adjustments
                if (userPrefs.PreferMoreExamples)
                {
                    budget = budget with
                    {
                        ExamplesBudget = (int)(budget.ExamplesBudget * 1.3),
                        SchemaContextBudget = (int)(budget.SchemaContextBudget * 0.9)
                    };
                }

                if (userPrefs.PreferDetailedSchema)
                {
                    budget = budget with
                    {
                        SchemaContextBudget = (int)(budget.SchemaContextBudget * 1.2),
                        BusinessContextBudget = (int)(budget.BusinessContextBudget * 0.9)
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error applying user adjustments for user {UserId}", userId);
        }

        return budget;
    }

    private TokenBudget CreateMinimalBudget(IntentType intentType)
    {
        return new TokenBudget
        {
            IntentType = intentType,
            MaxTotalTokens = 1000,
            BasePromptTokens = 300,
            ReservedResponseTokens = 200,
            AvailableContextTokens = 500,
            SchemaContextBudget = 200,
            BusinessContextBudget = 150,
            ExamplesBudget = 100,
            RulesBudget = 30,
            GlossaryBudget = 20,
            CreatedAt = DateTime.UtcNow
        };
    }

    private async Task AllocateSchemaContextAsync(TokenAllocationResult result, List<ContextSection> schemaSections)
    {
        var availableBudget = result.Budget.SchemaContextBudget;
        var allocatedTokens = 0;

        // Sort by priority (relevance score)
        var sortedSections = schemaSections.OrderByDescending(s => s.RelevanceScore).ToList();

        foreach (var section in sortedSections)
        {
            if (allocatedTokens + section.TokenCount <= availableBudget)
            {
                result.AllocatedSections.Add(section);
                allocatedTokens += section.TokenCount;
            }
            else
            {
                result.RejectedSections.Add(section);
            }
        }

        result.TokenUtilization["schema"] = allocatedTokens;
    }

    private async Task AllocateBusinessContextAsync(TokenAllocationResult result, List<ContextSection> businessSections)
    {
        var availableBudget = result.Budget.BusinessContextBudget;
        var allocatedTokens = 0;

        var sortedSections = businessSections.OrderByDescending(s => s.RelevanceScore).ToList();

        foreach (var section in sortedSections)
        {
            if (allocatedTokens + section.TokenCount <= availableBudget)
            {
                result.AllocatedSections.Add(section);
                allocatedTokens += section.TokenCount;
            }
            else
            {
                result.RejectedSections.Add(section);
            }
        }

        result.TokenUtilization["business"] = allocatedTokens;
    }

    private async Task AllocateExamplesAsync(TokenAllocationResult result, List<ContextSection> exampleSections)
    {
        var availableBudget = result.Budget.ExamplesBudget;
        var allocatedTokens = 0;

        var sortedSections = exampleSections.OrderByDescending(s => s.RelevanceScore).ToList();

        foreach (var section in sortedSections)
        {
            if (allocatedTokens + section.TokenCount <= availableBudget)
            {
                result.AllocatedSections.Add(section);
                allocatedTokens += section.TokenCount;
            }
            else
            {
                result.RejectedSections.Add(section);
            }
        }

        result.TokenUtilization["examples"] = allocatedTokens;
    }

    private async Task AllocateRulesAsync(TokenAllocationResult result, List<ContextSection> ruleSections)
    {
        var availableBudget = result.Budget.RulesBudget;
        var allocatedTokens = 0;

        var sortedSections = ruleSections.OrderByDescending(s => s.RelevanceScore).ToList();

        foreach (var section in sortedSections)
        {
            if (allocatedTokens + section.TokenCount <= availableBudget)
            {
                result.AllocatedSections.Add(section);
                allocatedTokens += section.TokenCount;
            }
            else
            {
                result.RejectedSections.Add(section);
            }
        }

        result.TokenUtilization["rules"] = allocatedTokens;
    }

    private async Task AllocateGlossaryAsync(TokenAllocationResult result, List<ContextSection> glossarySections)
    {
        var availableBudget = result.Budget.GlossaryBudget;
        var allocatedTokens = 0;

        var sortedSections = glossarySections.OrderByDescending(s => s.RelevanceScore).ToList();

        foreach (var section in sortedSections)
        {
            if (allocatedTokens + section.TokenCount <= availableBudget)
            {
                result.AllocatedSections.Add(section);
                allocatedTokens += section.TokenCount;
            }
            else
            {
                result.RejectedSections.Add(section);
            }
        }

        result.TokenUtilization["glossary"] = allocatedTokens;
    }

    private async Task<UserTokenPreferences?> GetUserTokenPreferencesAsync(string userId)
    {
        // This would query user preferences from database
        await Task.CompletedTask;
        return null; // Default - no specific preferences
    }

    private async Task RecordBudgetMetricsAsync(string operation, double duration, TokenBudget budget)
    {
        lock (_metricsLock)
        {
            if (!_budgetMetrics.ContainsKey(operation))
            {
                _budgetMetrics[operation] = new TokenBudgetMetrics();
            }

            var metrics = _budgetMetrics[operation];
            metrics.TotalOperations++;
            metrics.TotalDuration += duration;
            metrics.TotalUtilization += budget.AvailableContextTokens;
            metrics.LastOperation = DateTime.UtcNow;
        }
    }

    private async Task RecordAllocationMetricsAsync(string operation, double duration, TokenAllocationResult result)
    {
        lock (_metricsLock)
        {
            if (!_budgetMetrics.ContainsKey(operation))
            {
                _budgetMetrics[operation] = new TokenBudgetMetrics();
            }

            var metrics = _budgetMetrics[operation];
            metrics.TotalOperations++;
            metrics.TotalDuration += duration;
            metrics.TotalUtilization += result.UtilizationPercentage;
            metrics.LastOperation = DateTime.UtcNow;
        }
    }
}

// Supporting data structures
public record TokenBudget
{
    public IntentType IntentType { get; init; }
    public int MaxTotalTokens { get; init; }
    public int BasePromptTokens { get; init; }
    public int ReservedResponseTokens { get; init; }
    public int AvailableContextTokens { get; init; }
    public int SchemaContextBudget { get; init; }
    public int BusinessContextBudget { get; init; }
    public int ExamplesBudget { get; init; }
    public int RulesBudget { get; init; }
    public int GlossaryBudget { get; init; }
    public DateTime CreatedAt { get; init; }
    public BudgetAllocationStrategy? AllocationStrategy { get; init; }
}

public record BudgetAllocationStrategy
{
    public double SchemaContextPercentage { get; init; }
    public double BusinessContextPercentage { get; init; }
    public double ExamplesPercentage { get; init; }
    public double RulesPercentage { get; init; }
    public double GlossaryPercentage { get; init; }
}

public record TokenAllocationResult
{
    public TokenBudget Budget { get; init; } = new();
    public List<ContextSection> AllocatedSections { get; init; } = new();
    public List<ContextSection> RejectedSections { get; init; } = new();
    public Dictionary<string, int> TokenUtilization { get; init; } = new();
    public int TotalAllocatedTokens { get; set; }
    public double UtilizationPercentage { get; set; }
}

public record ContextSection
{
    public string Type { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public int TokenCount { get; init; }
    public double RelevanceScore { get; init; }
    public Dictionary<string, object> Metadata { get; init; } = new();
}

public record UserTokenPreferences
{
    public bool PreferMoreExamples { get; init; }
    public bool PreferDetailedSchema { get; init; }
    public bool PreferMinimalContext { get; init; }
}

public class TokenBudgetMetrics
{
    public int TotalOperations { get; set; }
    public double TotalDuration { get; set; }
    public double TotalUtilization { get; set; }
    public DateTime LastOperation { get; set; } = DateTime.UtcNow;
}

public record TokenOptimizationReport
{
    public DateTime GeneratedAt { get; init; }
    public IntentType? IntentFilter { get; init; }
    public TimeSpan TimeWindow { get; init; }
    public List<TokenOperationMetrics> OperationMetrics { get; init; } = new();
}

public record TokenOperationMetrics
{
    public string OperationType { get; init; } = string.Empty;
    public int TotalOperations { get; init; }
    public double AverageDuration { get; init; }
    public double AverageTokenUtilization { get; init; }
    public DateTime LastOperation { get; init; }
}

public interface ITokenBudgetManager
{
    Task<TokenBudget> CreateTokenBudgetAsync(BusinessContextProfile profile, int maxTokens = 4000, int reservedResponseTokens = 500);
    int CountTokens(string text, string contentType = "business_context");
    Task<TokenAllocationResult> AllocateTokensAsync(TokenBudget budget, List<ContextSection> availableSections);
    Task<TokenOptimizationReport> GenerateOptimizationReportAsync(IntentType? intentFilter = null, TimeSpan? timeWindow = null);
}
