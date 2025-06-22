// Application/Services/BusinessMetadataRetrievalService.cs
namespace ReportAIng.Application.Services
{
    public class BusinessMetadataRetrievalService : IBusinessMetadataRetrievalService
    {
        private readonly IBusinessTableRepository _tableRepository;
        private readonly IBusinessColumnRepository _columnRepository;
        private readonly IBusinessGlossaryRepository _glossaryRepository;
        private readonly ISemanticMatchingService _semanticMatchingService;
        private readonly IMemoryCache _cache;
        private readonly ILogger<BusinessMetadataRetrievalService> _logger;
        private readonly BusinessMetadataConfig _config;

        public BusinessMetadataRetrievalService(
            IBusinessTableRepository tableRepository,
            IBusinessColumnRepository columnRepository,
            IBusinessGlossaryRepository glossaryRepository,
            ISemanticMatchingService semanticMatchingService,
            IMemoryCache cache,
            ILogger<BusinessMetadataRetrievalService> logger,
            IOptions<BusinessMetadataConfig> config)
        {
            _tableRepository = tableRepository;
            _columnRepository = columnRepository;
            _glossaryRepository = glossaryRepository;
            _semanticMatchingService = semanticMatchingService;
            _cache = cache;
            _logger = logger;
            _config = config.Value;
        }

        public async Task<ContextualBusinessSchema> GetRelevantBusinessMetadataAsync(
            BusinessContextProfile profile,
            int maxTables = 5)
        {
            _logger.LogInformation("Retrieving business metadata for context: {Intent}", 
                profile.Intent.Type);

            // Find relevant tables
            var relevantTables = await FindRelevantTablesAsync(profile, maxTables);
            
            // Get columns for relevant tables
            var tableIds = relevantTables.Select(t => t.Id).ToList();
            var tableColumns = new Dictionary<long, List<BusinessColumnInfoDto>>();
            
            foreach (var tableId in tableIds)
            {
                var columns = await FindRelevantColumnsAsync(
                    new List<long> { tableId }, 
                    profile);
                tableColumns[tableId] = columns;
            }

            // Find glossary terms
            var glossaryTerms = await FindRelevantGlossaryTermsAsync(profile.BusinessTerms);

            // Discover relationships
            var tableNames = relevantTables.Select(t => $"{t.SchemaName}.{t.TableName}").ToList();
            var relationships = await DiscoverTableRelationshipsAsync(tableNames);

            // Extract business rules
            var businessRules = ExtractBusinessRules(relevantTables, tableColumns);

            // Build semantic mappings
            var semanticMappings = BuildSemanticMappings(profile, relevantTables, glossaryTerms);

            // Calculate schema complexity
            var complexity = CalculateSchemaComplexity(relevantTables.Count, relationships.Count);

            // Generate performance hints
            var performanceHints = GeneratePerformanceHints(profile, relevantTables);

            return new ContextualBusinessSchema
            {
                RelevantTables = relevantTables,
                TableColumns = tableColumns,
                RelevantGlossaryTerms = glossaryTerms,
                BusinessRules = businessRules,
                Relationships = relationships,
                SemanticMappings = semanticMappings,
                RelevanceScore = CalculateOverallRelevance(relevantTables),
                Complexity = complexity,
                SuggestedIndexes = performanceHints.indexes,
                PartitioningHints = performanceHints.partitions
            };
        }

        public async Task<List<BusinessTableInfoDto>> FindRelevantTablesAsync(
            BusinessContextProfile profile,
            int maxTables = 5)
        {
            var cacheKey = $"relevant_tables_{profile.GetHashCode()}";
            if (_cache.TryGetValue<List<BusinessTableInfoDto>>(cacheKey, out var cachedTables))
            {
                return cachedTables!;
            }

            // Multiple strategies for finding relevant tables
            var strategies = new List<Func<Task<List<TableRelevanceScore>>>>
            {
                () => SemanticSearchTablesAsync(profile),
                () => DomainBasedTableSearchAsync(profile.Domain),
                () => EntityBasedTableSearchAsync(profile.Entities),
                () => GlossaryBasedTableSearchAsync(profile.BusinessTerms)
            };

            // Execute all strategies in parallel
            var strategyResults = await Task.WhenAll(strategies.Select(s => s()));

            // Combine and score results
            var combinedScores = CombineTableScores(strategyResults);

            // Get top tables by score
            var topTableIds = combinedScores
                .OrderByDescending(s => s.Value)
                .Take(maxTables)
                .Select(s => s.Key)
                .ToList();

            var tables = await _tableRepository.GetByIdsAsync(topTableIds);
            var result = tables.Select(t => MapToDto(t)).ToList();

            // Cache results
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_config.CacheExpirationMinutes));

            return result;
        }

        public async Task<List<BusinessColumnInfoDto>> FindRelevantColumnsAsync(
            List<long> tableIds,
            BusinessContextProfile profile)
        {
            var allColumns = new List<BusinessColumnInfoDto>();

            foreach (var tableId in tableIds)
            {
                var columns = await _columnRepository.GetByTableIdAsync(tableId);
                
                // Score each column based on relevance
                var scoredColumns = new List<(BusinessColumnInfo column, double score)>();
                
                foreach (var column in columns.Where(c => c.IsActive))
                {
                    var score = await CalculateColumnRelevanceScoreAsync(column, profile);
                    scoredColumns.Add((column, score));
                }

                // Select most relevant columns
                var relevantColumns = scoredColumns
                    .Where(sc => sc.score > _config.MinColumnRelevanceScore)
                    .OrderByDescending(sc => sc.score)
                    .Take(_config.MaxColumnsPerTable)
                    .Select(sc => MapToDto(sc.column))
                    .ToList();

                allColumns.AddRange(relevantColumns);
            }

            return allColumns;
        }

        public async Task<List<BusinessGlossaryDto>> FindRelevantGlossaryTermsAsync(
            List<string> businessTerms)
        {
            var glossaryTerms = new List<BusinessGlossaryDto>();

            foreach (var term in businessTerms)
            {
                // Direct match
                var directMatches = await _glossaryRepository.SearchByTermAsync(term);
                glossaryTerms.AddRange(directMatches.Select(MapToDto));

                // Semantic match
                var semanticMatches = await _semanticMatchingService.FindSimilarTermsAsync(term, 3);
                foreach (var (matchedTerm, score) in semanticMatches.Where(m => m.score > 0.8))
                {
                    var glossaryEntry = await _glossaryRepository.GetByTermAsync(matchedTerm);
                    if (glossaryEntry != null && !glossaryTerms.Any(g => g.Id == glossaryEntry.Id))
                    {
                        glossaryTerms.Add(MapToDto(glossaryEntry));
                    }
                }
            }

            return glossaryTerms.Distinct().ToList();
        }

        public async Task<List<TableRelationship>> DiscoverTableRelationshipsAsync(
            List<string> tableNames)
        {
            var relationships = new List<TableRelationship>();

            // Get foreign key relationships from database metadata
            var fkRelationships = await _tableRepository.GetForeignKeyRelationshipsAsync(tableNames);
            
            foreach (var fk in fkRelationships)
            {
                relationships.Add(new TableRelationship
                {
                    FromTable = fk.FromTable,
                    ToTable = fk.ToTable,
                    FromColumn = fk.FromColumn,
                    ToColumn = fk.ToColumn,
                    Type = RelationshipType.OneToMany,
                    BusinessMeaning = await GenerateRelationshipMeaningAsync(fk)
                });
            }

            // Discover implicit relationships through naming conventions
            var implicitRelationships = await DiscoverImplicitRelationshipsAsync(tableNames);
            relationships.AddRange(implicitRelationships);

            return relationships;
        }

        // Private helper methods
        private async Task<List<TableRelevanceScore>> SemanticSearchTablesAsync(
            BusinessContextProfile profile)
        {
            var searchQuery = BuildSemanticSearchQuery(profile);
            var tables = await _semanticMatchingService.SemanticTableSearchAsync(searchQuery, 20);
            
            return tables.Select(t => new TableRelevanceScore
            {
                TableId = t.Id,
                Score = CalculateSemanticRelevance(t, profile),
                Source = "SemanticSearch"
            }).ToList();
        }

        private async Task<List<TableRelevanceScore>> DomainBasedTableSearchAsync(
            BusinessDomain domain)
        {
            var domainTables = await _tableRepository.GetByDomainAsync(domain.Name);
            
            return domainTables.Select(t => new TableRelevanceScore
            {
                TableId = t.Id,
                Score = CalculateDomainRelevance(t, domain),
                Source = "DomainSearch"
            }).ToList();
        }

        private async Task<List<TableRelevanceScore>> EntityBasedTableSearchAsync(
            List<BusinessEntity> entities)
        {
            var scores = new List<TableRelevanceScore>();
            
            foreach (var entity in entities.Where(e => e.Type == EntityType.Table))
            {
                var matchedTables = await _tableRepository.SearchByNameAsync(entity.Name);
                scores.AddRange(matchedTables.Select(t => new TableRelevanceScore
                {
                    TableId = t.Id,
                    Score = entity.ConfidenceScore,
                    Source = "EntityMatch"
                }));
            }

            return scores;
        }

        private async Task<double> CalculateColumnRelevanceScoreAsync(
            BusinessColumnInfo column,
            BusinessContextProfile profile)
        {
            var scores = new List<double>();

            // Check if column is mentioned in entities
            var entityScore = profile.Entities
                .Where(e => e.Type == EntityType.Column || e.Type == EntityType.Metric)
                .Any(e => StringSimilarity(e.Name, column.ColumnName) > 0.8) ? 1.0 : 0.0;
            scores.Add(entityScore);

            // Semantic similarity with question
            var semanticScore = await _semanticMatchingService.CalculateSemanticSimilarityAsync(
                profile.OriginalQuestion,
                $"{column.BusinessMeaning} {column.BusinessContext}");
            scores.Add(semanticScore);

            // Importance score from metadata
            scores.Add((double)column.ImportanceScore);

            // Usage frequency
            scores.Add((double)column.UsageFrequency);

            // Business term matching
            var termMatchScore = column.RelatedBusinessTerms
                .Split(',', StringSplitOptions.TrimEntries)
                .Any(term => profile.BusinessTerms.Contains(term)) ? 1.0 : 0.0;
            scores.Add(termMatchScore);

            // Weight the scores
            var weights = new[] { 0.3, 0.25, 0.2, 0.15, 0.1 };
            return scores.Select((score, i) => score * weights[i]).Sum();
        }

        private Dictionary<long, double> CombineTableScores(
            List<TableRelevanceScore>[] strategyResults)
        {
            var combinedScores = new Dictionary<long, double>();
            var weights = new Dictionary<string, double>
            {
                ["SemanticSearch"] = 0.35,
                ["DomainSearch"] = 0.25,
                ["EntityMatch"] = 0.3,
                ["GlossaryMatch"] = 0.1
            };

            foreach (var results in strategyResults)
            {
                foreach (var result in results)
                {
                    var weight = weights.GetValueOrDefault(result.Source, 0.1);
                    if (combinedScores.ContainsKey(result.TableId))
                    {
                        combinedScores[result.TableId] += result.Score * weight;
                    }
                    else
                    {
                        combinedScores[result.TableId] = result.Score * weight;
                    }
                }
            }

            return combinedScores;
        }

        private List<BusinessRule> ExtractBusinessRules(
            List<BusinessTableInfoDto> tables,
            Dictionary<long, List<BusinessColumnInfoDto>> tableColumns)
        {
            var rules = new List<BusinessRule>();
            int ruleId = 0;

            // Extract table-level rules
            foreach (var table in tables)
            {
                if (!string.IsNullOrEmpty(table.BusinessRules))
                {
                    var tableRules = ParseBusinessRules(table.BusinessRules, $"rule_{++ruleId}");
                    rules.AddRange(tableRules);
                }
            }

            // Extract column-level rules
            foreach (var (tableId, columns) in tableColumns)
            {
                foreach (var column in columns)
                {
                    if (!string.IsNullOrEmpty(column.ValidationRules))
                    {
                        rules.Add(new BusinessRule
                        {
                            Id = $"rule_{++ruleId}",
                            Description = column.ValidationRules,
                            Type = RuleType.Validation,
                            AffectedColumns = new List<string> { column.ColumnName },
                            Priority = column.ImportanceScore > 0.8m ? 1 : 2
                        });
                    }
                }
            }

            return rules;
        }

        private SchemaComplexity CalculateSchemaComplexity(int tableCount, int relationshipCount)
        {
            if (tableCount == 1) return SchemaComplexity.Simple;
            if (tableCount <= 3 && relationshipCount <= 2) return SchemaComplexity.Moderate;
            if (tableCount <= 5 && relationshipCount <= 5) return SchemaComplexity.Complex;
            return SchemaComplexity.VeryComplex;
        }

        private class TableRelevanceScore
        {
            public long TableId { get; set; }
            public double Score { get; set; }
            public string Source { get; set; } = string.Empty;
        }
    }

    // Configuration class
    public class BusinessMetadataConfig
    {
        public int CacheExpirationMinutes { get; set; } = 30;
        public double MinColumnRelevanceScore { get; set; } = 0.3;
        public int MaxColumnsPerTable { get; set; } = 20;
        public double SemanticThreshold { get; set; } = 0.7;
    }
}