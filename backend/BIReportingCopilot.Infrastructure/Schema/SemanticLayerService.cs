using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Schema;

/// <summary>
/// Service for managing the semantic layer that bridges raw database schemas with business-friendly terminology
/// </summary>
public class SemanticLayerService : ISemanticLayerService
{
    private readonly ILogger<SemanticLayerService> _logger;
    private readonly BICopilotContext _context;
    private readonly IDynamicSchemaContextualizationService _contextualizationService;
    private readonly IVectorSearchService _vectorSearchService;

    public SemanticLayerService(
        ILogger<SemanticLayerService> logger,
        BICopilotContext context,
        IDynamicSchemaContextualizationService contextualizationService,
        IVectorSearchService vectorSearchService)
    {
        _logger = logger;
        _context = context;
        _contextualizationService = contextualizationService;
        _vectorSearchService = vectorSearchService;
    }

    /// <summary>
    /// Get business-friendly schema description for LLM consumption
    /// </summary>
    public async Task<string> GetBusinessFriendlySchemaAsync(string naturalLanguageQuery, int maxTokens = 2000)
    {
        try
        {
            _logger.LogInformation("🧠 Generating business-friendly schema for query: {Query}", naturalLanguageQuery);

            // Get contextually relevant schema elements
            var contextualizedSchema = await _contextualizationService.GetRelevantSchemaAsync(naturalLanguageQuery);

            // Build business-friendly description
            var schemaDescription = await BuildBusinessFriendlyDescriptionAsync(contextualizedSchema, maxTokens);

            _logger.LogInformation("✅ Generated schema description with {TokenCount} estimated tokens", 
                contextualizedSchema.TokenEstimate);

            return schemaDescription;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating business-friendly schema");
            return await GetFallbackSchemaDescriptionAsync();
        }
    }

    /// <summary>
    /// Build a comprehensive business-friendly schema description
    /// </summary>
    private async Task<string> BuildBusinessFriendlyDescriptionAsync(ContextualizedSchemaResult contextualizedSchema, int maxTokens)
    {
        var description = new List<string>();

        // Add business context header
        description.Add("=== BUSINESS DATA CONTEXT ===");
        description.Add($"Query Intent: {contextualizedSchema.QueryAnalysis.Intent}");
        description.Add($"Business Domain: {contextualizedSchema.QueryAnalysis.QueryCategory}");
        description.Add($"Confidence: {contextualizedSchema.ConfidenceScore:P1}");
        description.Add("");

        // Add business glossary terms
        if (contextualizedSchema.BusinessTermsUsed.Any())
        {
            description.Add("=== BUSINESS TERMS ===");
            foreach (var term in contextualizedSchema.BusinessTermsUsed)
            {
                var glossaryEntry = await GetBusinessTermDefinitionAsync(term);
                if (glossaryEntry != null)
                {
                    description.Add($"• {term}: {glossaryEntry.Definition}");
                    if (glossaryEntry.Synonyms.Any())
                    {
                        description.Add($"  Synonyms: {string.Join(", ", glossaryEntry.Synonyms)}");
                    }
                }
            }
            description.Add("");
        }

        // Add relevant tables with business context
        description.Add("=== RELEVANT TABLES ===");
        foreach (var table in contextualizedSchema.RelevantTables.Take(8)) // Limit to prevent token overflow
        {
            description.Add($"Table: {table.SchemaName}.{table.TableName}");
            description.Add($"Purpose: {table.BusinessPurpose}");
            description.Add($"Context: {table.BusinessContext}");
            
            if (table.NaturalLanguageAliases.Any())
            {
                description.Add($"Also known as: {string.Join(", ", table.NaturalLanguageAliases)}");
            }

            if (!string.IsNullOrEmpty(table.PrimaryUseCase))
            {
                description.Add($"Primary use: {table.PrimaryUseCase}");
            }

            // Add key columns for this table
            var tableColumns = contextualizedSchema.RelevantColumns
                .Where(c => c.TableName == table.TableName)
                .Take(10) // Limit columns per table
                .ToList();

            if (tableColumns.Any())
            {
                description.Add("Key columns:");
                foreach (var column in tableColumns)
                {
                    var columnDesc = $"  - {column.ColumnName}";
                    if (!string.IsNullOrEmpty(column.BusinessMeaning))
                    {
                        columnDesc += $": {column.BusinessMeaning}";
                    }
                    if (!string.IsNullOrEmpty(column.BusinessDataType))
                    {
                        columnDesc += $" ({column.BusinessDataType})";
                    }
                    description.Add(columnDesc);
                }
            }
            description.Add("");
        }

        // Add business rules and constraints
        var businessRules = await GetRelevantBusinessRulesAsync(contextualizedSchema);
        if (businessRules.Any())
        {
            description.Add("=== BUSINESS RULES ===");
            foreach (var rule in businessRules.Take(5))
            {
                description.Add($"• {rule}");
            }
            description.Add("");
        }

        // Add common query patterns
        var queryPatterns = await GetRelevantQueryPatternsAsync(contextualizedSchema);
        if (queryPatterns.Any())
        {
            description.Add("=== COMMON PATTERNS ===");
            foreach (var pattern in queryPatterns.Take(3))
            {
                description.Add($"• {pattern.NaturalLanguagePattern}");
                description.Add($"  Example: {pattern.SqlTemplate}");
            }
            description.Add("");
        }

        var fullDescription = string.Join("\n", description);

        // Truncate if too long (rough token estimation: 1 token ≈ 4 characters)
        if (fullDescription.Length > maxTokens * 4)
        {
            var truncated = fullDescription.Substring(0, maxTokens * 4);
            var lastNewline = truncated.LastIndexOf('\n');
            if (lastNewline > 0)
            {
                fullDescription = truncated.Substring(0, lastNewline) + "\n\n[Schema description truncated for token limit]";
            }
        }

        return fullDescription;
    }

    /// <summary>
    /// Get business term definition from glossary
    /// </summary>
    private async Task<EnhancedBusinessGlossaryDto?> GetBusinessTermDefinitionAsync(string term)
    {
        try
        {
            var glossaryEntry = await _context.BusinessGlossary
                .FirstOrDefaultAsync(g => g.Term.ToLower() == term.ToLower() && g.IsActive);

            if (glossaryEntry == null)
            {
                // Try to find by synonym
                var synonymMatch = await _context.BusinessGlossary
                    .Where(g => g.IsActive && !string.IsNullOrEmpty(g.Synonyms))
                    .ToListAsync();

                foreach (var entry in synonymMatch)
                {
                    var synonyms = JsonSerializer.Deserialize<List<string>>(entry.Synonyms) ?? new List<string>();
                    if (synonyms.Any(s => s.ToLower() == term.ToLower()))
                    {
                        glossaryEntry = entry;
                        break;
                    }
                }
            }

            return glossaryEntry != null ? MapToEnhancedGlossaryDto(glossaryEntry) : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting business term definition for: {Term}", term);
            return null;
        }
    }

    /// <summary>
    /// Get relevant business rules for the contextualized schema
    /// </summary>
    private async Task<List<string>> GetRelevantBusinessRulesAsync(ContextualizedSchemaResult contextualizedSchema)
    {
        var rules = new List<string>();

        try
        {
            foreach (var table in contextualizedSchema.RelevantTables)
            {
                if (!string.IsNullOrEmpty(table.BusinessRules))
                {
                    var tableRules = JsonSerializer.Deserialize<List<string>>(table.BusinessRules) ?? new List<string>();
                    rules.AddRange(tableRules);
                }
            }

            // Get domain-specific rules
            var domains = contextualizedSchema.RelevantTables
                .Select(t => t.DomainClassification)
                .Where(d => !string.IsNullOrEmpty(d))
                .Distinct();

            foreach (var domain in domains)
            {
                var domainEntity = await _context.BusinessDomains
                    .FirstOrDefaultAsync(d => d.DomainName.ToLower() == domain.ToLower() && d.IsActive);

                if (domainEntity != null && !string.IsNullOrEmpty(domainEntity.CommonQueries))
                {
                    var domainRules = JsonSerializer.Deserialize<List<string>>(domainEntity.CommonQueries) ?? new List<string>();
                    rules.AddRange(domainRules);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting relevant business rules");
        }

        return rules.Distinct().ToList();
    }

    /// <summary>
    /// Get relevant query patterns for the contextualized schema
    /// </summary>
    private async Task<List<QueryPatternEntity>> GetRelevantQueryPatternsAsync(ContextualizedSchemaResult contextualizedSchema)
    {
        try
        {
            var tableNames = contextualizedSchema.RelevantTables.Select(t => t.TableName).ToList();
            
            var patterns = await _context.QueryPatterns
                .Where(p => p.IsActive)
                .ToListAsync();

            // Filter patterns that are relevant to the tables in context
            var relevantPatterns = patterns.Where(p =>
            {
                if (string.IsNullOrEmpty(p.RequiredTables)) return false;
                
                var requiredTables = JsonSerializer.Deserialize<List<string>>(p.RequiredTables) ?? new List<string>();
                return requiredTables.Any(rt => tableNames.Any(tn => tn.ToLower().Contains(rt.ToLower())));
            }).ToList();

            return relevantPatterns.OrderByDescending(p => p.UsageCount).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting relevant query patterns");
            return new List<QueryPatternEntity>();
        }
    }

    /// <summary>
    /// Update semantic metadata for a table
    /// </summary>
    public async Task<bool> UpdateTableSemanticMetadataAsync(long tableId, UpdateTableSemanticRequest request)
    {
        try
        {
            var table = await _context.BusinessTableInfo.FindAsync(tableId);
            if (table == null) return false;

            table.DomainClassification = request.DomainClassification;
            table.NaturalLanguageAliases = JsonSerializer.Serialize(request.NaturalLanguageAliases);
            table.BusinessOwner = request.BusinessOwner;
            table.DataGovernancePolicies = JsonSerializer.Serialize(request.DataGovernancePolicies);
            table.ImportanceScore = request.ImportanceScore;
            table.LastAnalyzed = DateTime.UtcNow;
            table.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Updated semantic metadata for table: {TableName}", table.TableName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating table semantic metadata for ID: {TableId}", tableId);
            return false;
        }
    }

    /// <summary>
    /// Create or update semantic schema mapping
    /// </summary>
    public async Task<long> CreateSemanticMappingAsync(CreateSemanticMappingRequest request)
    {
        try
        {
            var mapping = new SemanticSchemaMappingEntity
            {
                QueryIntent = request.QueryIntent,
                RelevantTables = JsonSerializer.Serialize(request.RelevantTables),
                RelevantColumns = JsonSerializer.Serialize(request.RelevantColumns),
                BusinessTerms = JsonSerializer.Serialize(request.BusinessTerms),
                QueryCategory = request.QueryCategory,
                ConfidenceScore = request.ConfidenceScore,
                UsageCount = 1,
                LastUsed = DateTime.UtcNow,
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            _context.SemanticSchemaMappings.Add(mapping);
            await _context.SaveChangesAsync();

            // Store in vector search for similarity matching
            await _vectorSearchService.IndexDocumentAsync(new VectorDocument
            {
                Id = mapping.Id.ToString(),
                Content = request.QueryIntent,
                Metadata = new Dictionary<string, object>
                {
                    ["QueryCategory"] = request.QueryCategory,
                    ["ConfidenceScore"] = request.ConfidenceScore,
                    ["CreatedDate"] = mapping.CreatedDate
                }
            });

            _logger.LogInformation("✅ Created semantic mapping for query intent: {Intent}", request.QueryIntent);
            return mapping.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating semantic mapping");
            return 0;
        }
    }

    /// <summary>
    /// Get fallback schema description when semantic layer fails
    /// </summary>
    private async Task<string> GetFallbackSchemaDescriptionAsync()
    {
        try
        {
            var basicTables = await _context.BusinessTableInfo
                .Where(t => t.IsActive)
                .OrderByDescending(t => t.ImportanceScore)
                .Take(5)
                .ToListAsync();

            var description = new List<string>
            {
                "=== BASIC SCHEMA INFORMATION ===",
                "Available tables:"
            };

            foreach (var table in basicTables)
            {
                description.Add($"• {table.SchemaName}.{table.TableName}: {table.BusinessPurpose}");
            }

            return string.Join("\n", description);
        }
        catch
        {
            return "Schema information temporarily unavailable.";
        }
    }

    /// <summary>
    /// Map entity to enhanced DTO
    /// </summary>
    private EnhancedBusinessGlossaryDto MapToEnhancedGlossaryDto(BusinessGlossaryEntity entity)
    {
        return new EnhancedBusinessGlossaryDto
        {
            Id = entity.Id,
            Term = entity.Term,
            Definition = entity.Definition,
            BusinessContext = entity.BusinessContext,
            Synonyms = string.IsNullOrEmpty(entity.Synonyms) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(entity.Synonyms) ?? new List<string>(),
            RelatedTerms = string.IsNullOrEmpty(entity.RelatedTerms)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(entity.RelatedTerms) ?? new List<string>(),
            Category = entity.Category,
            Domain = entity.Domain,
            Examples = string.IsNullOrEmpty(entity.Examples)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(entity.Examples) ?? new List<string>(),
            MappedTables = string.IsNullOrEmpty(entity.MappedTables)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(entity.MappedTables) ?? new List<string>(),
            MappedColumns = string.IsNullOrEmpty(entity.MappedColumns)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(entity.MappedColumns) ?? new List<string>(),
            BusinessOwner = entity.BusinessOwner,
            ConfidenceScore = entity.ConfidenceScore,
            AmbiguityScore = entity.AmbiguityScore,
            IsActive = entity.IsActive,
            UsageCount = entity.UsageCount,
            LastUsed = entity.LastUsed,
            LastValidated = entity.LastValidated
        };
    }
}
