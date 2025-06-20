using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BIReportingCopilot.Infrastructure.Data.Entities;

[Table("BusinessTableInfo")]
public class BusinessTableInfoEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(128)]
    public string TableName { get; set; } = string.Empty;

    [Required]
    [MaxLength(128)]
    public string SchemaName { get; set; } = "common";

    [MaxLength(500)]
    public string BusinessPurpose { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string BusinessContext { get; set; } = string.Empty;

    [MaxLength(500)]
    public string PrimaryUseCase { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string CommonQueryPatterns { get; set; } = string.Empty; // JSON

    [MaxLength(2000)]
    public string BusinessRules { get; set; } = string.Empty;

    // Enhanced semantic metadata fields
    [MaxLength(1000)]
    public string DomainClassification { get; set; } = string.Empty; // e.g., "Financial", "Customer", "Product"

    [MaxLength(2000)]
    public string NaturalLanguageAliases { get; set; } = string.Empty; // JSON array of business-friendly names

    [MaxLength(4000)]
    public string UsagePatterns { get; set; } = string.Empty; // JSON - frequency, common joins, etc.

    [MaxLength(1000)]
    public string DataQualityIndicators { get; set; } = string.Empty; // JSON - completeness, accuracy scores

    [MaxLength(2000)]
    public string RelationshipSemantics { get; set; } = string.Empty; // JSON - business meaning of relationships

    public decimal ImportanceScore { get; set; } = 0.5m; // 0-1 scale for prioritization

    public decimal UsageFrequency { get; set; } = 0.0m; // How often this table is queried

    public DateTime? LastAnalyzed { get; set; } // When semantic analysis was last performed

    [MaxLength(500)]
    public string BusinessOwner { get; set; } = string.Empty; // Who owns this data from business perspective

    [MaxLength(1000)]
    public string DataGovernancePolicies { get; set; } = string.Empty; // JSON - access rules, retention policies

    // Phase 2 Enhanced Semantic Metadata for Tables
    [MaxLength(2000)]
    public string SemanticDescription { get; set; } = string.Empty; // Rich semantic description for LLM understanding

    [MaxLength(1000)]
    public string BusinessProcesses { get; set; } = string.Empty; // JSON array of business processes this table supports

    [MaxLength(1000)]
    public string AnalyticalUseCases { get; set; } = string.Empty; // JSON array of analytical use cases

    [MaxLength(500)]
    public string ReportingCategories { get; set; } = string.Empty; // JSON array of reporting categories

    [MaxLength(1000)]
    public string SemanticRelationships { get; set; } = string.Empty; // JSON mapping of semantic relationships to other tables

    [MaxLength(500)]
    public string QueryComplexityHints { get; set; } = string.Empty; // JSON hints for query complexity and optimization

    [MaxLength(1000)]
    public string BusinessGlossaryTerms { get; set; } = string.Empty; // JSON array of related business glossary terms

    public decimal SemanticCoverageScore { get; set; } = 0.5m; // Coverage score for semantic completeness (0.0 to 1.0)

    [MaxLength(500)]
    public string LLMContextHints { get; set; } = string.Empty; // JSON array of context hints for LLM processing

    [MaxLength(1000)]
    public string VectorSearchKeywords { get; set; } = string.Empty; // JSON array of keywords for vector search optimization

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<BusinessColumnInfoEntity> Columns { get; set; } = new List<BusinessColumnInfoEntity>();
}

[Table("BusinessColumnInfo")]
public class BusinessColumnInfoEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    public long TableInfoId { get; set; }

    [Required]
    [MaxLength(128)]
    public string ColumnName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string BusinessMeaning { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string BusinessContext { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string DataExamples { get; set; } = string.Empty; // JSON

    [MaxLength(1000)]
    public string ValidationRules { get; set; } = string.Empty;

    // Enhanced semantic metadata fields
    [MaxLength(1000)]
    public string NaturalLanguageAliases { get; set; } = string.Empty; // JSON array of business-friendly names

    // Phase 2 Enhanced Semantic Metadata
    [MaxLength(2000)]
    public string SemanticContext { get; set; } = string.Empty; // Rich contextual information for LLM understanding

    [MaxLength(1000)]
    public string ConceptualRelationships { get; set; } = string.Empty; // JSON array of related business concepts

    [MaxLength(500)]
    public string DomainSpecificTerms { get; set; } = string.Empty; // JSON array of domain-specific terminology

    [MaxLength(1000)]
    public string QueryIntentMapping { get; set; } = string.Empty; // JSON mapping of query intents to this column

    [MaxLength(500)]
    public string BusinessQuestionTypes { get; set; } = string.Empty; // JSON array of business question types this column answers

    [MaxLength(1000)]
    public string SemanticSynonyms { get; set; } = string.Empty; // JSON array of semantic synonyms for vector search

    [MaxLength(500)]
    public string AnalyticalContext { get; set; } = string.Empty; // Context for analytical queries (aggregations, filters, etc.)

    [MaxLength(500)]
    public string BusinessMetrics { get; set; } = string.Empty; // JSON array of business metrics this column supports

    public decimal SemanticRelevanceScore { get; set; } = 0.5m; // Relevance score for semantic search (0.0 to 1.0)

    [MaxLength(1000)]
    public string LLMPromptHints { get; set; } = string.Empty; // JSON array of hints for LLM prompt engineering

    [MaxLength(500)]
    public string VectorSearchTags { get; set; } = string.Empty; // JSON array of tags for vector search optimization

    [MaxLength(2000)]
    public string ValueExamples { get; set; } = string.Empty; // JSON - representative values with business context

    [MaxLength(1000)]
    public string DataLineage { get; set; } = string.Empty; // JSON - source systems, transformations

    [MaxLength(2000)]
    public string CalculationRules { get; set; } = string.Empty; // How derived/calculated fields are computed

    [MaxLength(1000)]
    public string SemanticTags { get; set; } = string.Empty; // JSON - PII, Financial, Metric, Dimension, etc.

    [MaxLength(500)]
    public string BusinessDataType { get; set; } = string.Empty; // Currency, Percentage, Count, etc.

    [MaxLength(1000)]
    public string ConstraintsAndRules { get; set; } = string.Empty; // JSON - business constraints

    public decimal DataQualityScore { get; set; } = 0.0m; // 0-1 completeness/accuracy score

    public decimal UsageFrequency { get; set; } = 0.0m; // How often this column is used in queries

    [MaxLength(500)]
    public string PreferredAggregation { get; set; } = string.Empty; // SUM, AVG, COUNT, etc.

    [MaxLength(1000)]
    public string RelatedBusinessTerms { get; set; } = string.Empty; // JSON - links to business glossary

    public bool IsKeyColumn { get; set; }
    public bool IsSensitiveData { get; set; } = false; // PII or confidential data flag
    public bool IsCalculatedField { get; set; } = false; // Whether this is a derived/calculated field
    public bool IsActive { get; set; } = true;

    // Navigation properties
    [ForeignKey("TableInfoId")]
    public virtual BusinessTableInfoEntity? TableInfo { get; set; }
}

[Table("QueryPatterns")]
public class QueryPatternEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string PatternName { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string NaturalLanguagePattern { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string SqlTemplate { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string BusinessContext { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Keywords { get; set; } = string.Empty; // JSON

    [MaxLength(1000)]
    public string RequiredTables { get; set; } = string.Empty; // JSON

    public int Priority { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public int UsageCount { get; set; } = 0;
    public DateTime? LastUsed { get; set; }

    // Alias property for compatibility
    [NotMapped]
    public DateTime? LastUsedDate
    {
        get => LastUsed;
        set => LastUsed = value;
    }
}

[Table("BusinessGlossary")]
public class BusinessGlossaryEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Term { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Definition { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string BusinessContext { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Synonyms { get; set; } = string.Empty; // JSON

    [MaxLength(1000)]
    public string RelatedTerms { get; set; } = string.Empty; // JSON

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    // Enhanced semantic metadata fields
    [MaxLength(200)]
    public string Domain { get; set; } = string.Empty; // Business domain (Finance, Marketing, Operations)

    [MaxLength(2000)]
    public string Examples { get; set; } = string.Empty; // JSON - real-world examples of the term

    [MaxLength(1000)]
    public string MappedTables { get; set; } = string.Empty; // JSON - which tables contain this concept

    [MaxLength(1000)]
    public string MappedColumns { get; set; } = string.Empty; // JSON - which columns represent this term

    [MaxLength(1000)]
    public string HierarchicalRelations { get; set; } = string.Empty; // JSON - parent/child term relationships

    [MaxLength(500)]
    public string PreferredCalculation { get; set; } = string.Empty; // How to calculate this metric

    [MaxLength(1000)]
    public string DisambiguationRules { get; set; } = string.Empty; // JSON - rules for resolving ambiguity

    [MaxLength(500)]
    public string BusinessOwner { get; set; } = string.Empty; // Who defines this term

    [MaxLength(1000)]
    public string RegulationReferences { get; set; } = string.Empty; // JSON - regulatory or compliance context

    public decimal ConfidenceScore { get; set; } = 1.0m; // How confident we are in this definition

    public decimal AmbiguityScore { get; set; } = 0.0m; // How ambiguous this term is (0 = clear, 1 = very ambiguous)

    [MaxLength(1000)]
    public string ContextualVariations { get; set; } = string.Empty; // JSON - how meaning changes by context

    // Phase 2 Enhanced Semantic Reasoning
    [MaxLength(2000)]
    public string SemanticEmbedding { get; set; } = string.Empty; // JSON - vector embedding for semantic search

    [MaxLength(1000)]
    public string QueryPatterns { get; set; } = string.Empty; // JSON - common query patterns that use this term

    [MaxLength(1000)]
    public string LLMPromptTemplates { get; set; } = string.Empty; // JSON - prompt templates for this term

    [MaxLength(500)]
    public string DisambiguationContext { get; set; } = string.Empty; // Context clues for disambiguation

    [MaxLength(1000)]
    public string SemanticRelationships { get; set; } = string.Empty; // JSON - semantic relationships (is-a, part-of, etc.)

    [MaxLength(500)]
    public string ConceptualLevel { get; set; } = string.Empty; // Abstract, Concrete, Operational, Strategic

    [MaxLength(1000)]
    public string CrossDomainMappings { get; set; } = string.Empty; // JSON - how this term maps across business domains

    public decimal SemanticStability { get; set; } = 1.0m; // How stable this term's meaning is (0 = volatile, 1 = stable)

    [MaxLength(1000)]
    public string InferenceRules { get; set; } = string.Empty; // JSON - rules for semantic inference

    public bool IsActive { get; set; } = true;
    public int UsageCount { get; set; } = 0;
    public DateTime? LastUsed { get; set; }
    public DateTime? LastValidated { get; set; } // When this definition was last reviewed

    // Alias property for compatibility
    [NotMapped]
    public DateTime? LastUsedDate
    {
        get => LastUsed;
        set => LastUsed = value;
    }
}

[Table("AITuningSettings")]
public class AITuningSettingsEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string SettingKey { get; set; } = string.Empty;

    [Required]
    [MaxLength(4000)]
    public string SettingValue { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(50)]
    public string DataType { get; set; } = "string";

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Entity for storing semantic query-to-schema mappings for dynamic contextualization
/// </summary>
[Table("SemanticSchemaMapping")]
public class SemanticSchemaMappingEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(1000)]
    public string QueryIntent { get; set; } = string.Empty; // Natural language description of query intent

    [Required]
    [MaxLength(4000)]
    public string RelevantTables { get; set; } = string.Empty; // JSON array of table names with relevance scores

    [MaxLength(4000)]
    public string RelevantColumns { get; set; } = string.Empty; // JSON array of column names with relevance scores

    [MaxLength(2000)]
    public string BusinessTerms { get; set; } = string.Empty; // JSON array of business terms mentioned

    [MaxLength(1000)]
    public string QueryCategory { get; set; } = string.Empty; // Reporting, Analytics, Lookup, etc.

    public decimal ConfidenceScore { get; set; } = 0.0m; // How confident we are in this mapping

    [MaxLength(4000)]
    public string SemanticVector { get; set; } = string.Empty; // JSON - embedding vector for similarity search

    public int UsageCount { get; set; } = 0; // How many times this mapping was used

    public DateTime? LastUsed { get; set; }

    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Entity for storing business domain classifications and relationships
/// </summary>
[Table("BusinessDomain")]
public class BusinessDomainEntity : BaseEntity
{
    [Key]
    public long Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string DomainName { get; set; } = string.Empty; // Finance, Marketing, Operations, etc.

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string RelatedTables { get; set; } = string.Empty; // JSON array of tables in this domain

    [MaxLength(2000)]
    public string KeyConcepts { get; set; } = string.Empty; // JSON array of important business concepts

    [MaxLength(1000)]
    public string CommonQueries { get; set; } = string.Empty; // JSON array of typical query patterns

    [MaxLength(500)]
    public string BusinessOwner { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string RelatedDomains { get; set; } = string.Empty; // JSON array of related business domains

    public decimal ImportanceScore { get; set; } = 0.5m; // Relative importance of this domain

    public bool IsActive { get; set; } = true;
}


