using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.DTOs;

/// <summary>
/// Enhanced business table information with semantic metadata
/// </summary>
public class EnhancedBusinessTableDto
{
    public long Id { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public string BusinessPurpose { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public string PrimaryUseCase { get; set; } = string.Empty;
    public List<string> CommonQueryPatterns { get; set; } = new();
    public string BusinessRules { get; set; } = string.Empty;
    
    // Enhanced semantic fields
    public string DomainClassification { get; set; } = string.Empty;
    public List<string> NaturalLanguageAliases { get; set; } = new();
    public UsagePatternDto UsagePatterns { get; set; } = new();
    public DataQualityDto DataQualityIndicators { get; set; } = new();
    public List<RelationshipSemanticDto> RelationshipSemantics { get; set; } = new();
    public decimal ImportanceScore { get; set; } = 0.5m;
    public decimal UsageFrequency { get; set; } = 0.0m;
    public DateTime? LastAnalyzed { get; set; }
    public string BusinessOwner { get; set; } = string.Empty;
    public List<string> DataGovernancePolicies { get; set; } = new();
    
    public List<EnhancedBusinessColumnDto> Columns { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public DateTime CreatedDate { get; set; }
    public DateTime UpdatedDate { get; set; }
}

/// <summary>
/// Enhanced business column information with semantic metadata
/// </summary>
public class EnhancedBusinessColumnDto
{
    public long Id { get; set; }
    public long TableInfoId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string BusinessMeaning { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public List<string> DataExamples { get; set; } = new();
    public string ValidationRules { get; set; } = string.Empty;
    
    // Enhanced semantic fields
    public List<string> NaturalLanguageAliases { get; set; } = new();
    public List<ValueExampleDto> ValueExamples { get; set; } = new();
    public DataLineageDto DataLineage { get; set; } = new();
    public string CalculationRules { get; set; } = string.Empty;
    public List<string> SemanticTags { get; set; } = new();
    public string BusinessDataType { get; set; } = string.Empty;
    public List<string> ConstraintsAndRules { get; set; } = new();
    public decimal DataQualityScore { get; set; } = 0.0m;
    public decimal UsageFrequency { get; set; } = 0.0m;
    public string PreferredAggregation { get; set; } = string.Empty;
    public List<string> RelatedBusinessTerms { get; set; } = new();
    
    public bool IsKeyColumn { get; set; }
    public bool IsSensitiveData { get; set; } = false;
    public bool IsCalculatedField { get; set; } = false;
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Enhanced business glossary with semantic relationships
/// </summary>
public class EnhancedBusinessGlossaryDto
{
    public long Id { get; set; }
    public string Term { get; set; } = string.Empty;
    public string Definition { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public List<string> Synonyms { get; set; } = new();
    public List<string> RelatedTerms { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    
    // Enhanced semantic fields
    public string Domain { get; set; } = string.Empty;
    public List<string> Examples { get; set; } = new();
    public List<string> MappedTables { get; set; } = new();
    public List<string> MappedColumns { get; set; } = new();
    public List<HierarchicalRelationDto> HierarchicalRelations { get; set; } = new();
    public string PreferredCalculation { get; set; } = string.Empty;
    public List<DisambiguationRuleDto> DisambiguationRules { get; set; } = new();
    public string BusinessOwner { get; set; } = string.Empty;
    public List<string> RegulationReferences { get; set; } = new();
    public decimal ConfidenceScore { get; set; } = 1.0m;
    public decimal AmbiguityScore { get; set; } = 0.0m;
    public List<ContextualVariationDto> ContextualVariations { get; set; } = new();
    
    public bool IsActive { get; set; } = true;
    public int UsageCount { get; set; } = 0;
    public DateTime? LastUsed { get; set; }
    public DateTime? LastValidated { get; set; }
}

/// <summary>
/// Semantic schema mapping for dynamic contextualization
/// </summary>
public class SemanticSchemaMappingDto
{
    public long Id { get; set; }
    public string QueryIntent { get; set; } = string.Empty;
    public List<TableRelevanceDto> RelevantTables { get; set; } = new();
    public List<ColumnRelevanceDto> RelevantColumns { get; set; } = new();
    public List<string> BusinessTerms { get; set; } = new();
    public string QueryCategory { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; } = 0.0m;
    public float[] SemanticVector { get; set; } = Array.Empty<float>();
    public int UsageCount { get; set; } = 0;
    public DateTime? LastUsed { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Business domain classification
/// </summary>
public class BusinessDomainDto
{
    public long Id { get; set; }
    public string DomainName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> RelatedTables { get; set; } = new();
    public List<string> KeyConcepts { get; set; } = new();
    public List<string> CommonQueries { get; set; } = new();
    public string BusinessOwner { get; set; } = string.Empty;
    public List<string> RelatedDomains { get; set; } = new();
    public decimal ImportanceScore { get; set; } = 0.5m;
    public bool IsActive { get; set; } = true;
}

// Supporting DTOs
public class UsagePatternDto
{
    public decimal QueryFrequency { get; set; } = 0.0m;
    public List<string> CommonJoins { get; set; } = new();
    public List<string> TypicalFilters { get; set; } = new();
    public List<string> PreferredAggregations { get; set; } = new();
}

public class DataQualityDto
{
    public decimal CompletenessScore { get; set; } = 0.0m;
    public decimal AccuracyScore { get; set; } = 0.0m;
    public decimal ConsistencyScore { get; set; } = 0.0m;
    public DateTime? LastAssessed { get; set; }
}

public class RelationshipSemanticDto
{
    public string RelatedTable { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty; // OneToMany, ManyToOne, etc.
    public string BusinessMeaning { get; set; } = string.Empty;
    public List<string> JoinColumns { get; set; } = new();
}

public class ValueExampleDto
{
    public string Value { get; set; } = string.Empty;
    public string BusinessContext { get; set; } = string.Empty;
    public decimal Frequency { get; set; } = 0.0m;
}

public class DataLineageDto
{
    public List<string> SourceSystems { get; set; } = new();
    public List<string> Transformations { get; set; } = new();
    public DateTime? LastUpdated { get; set; }
}

public class HierarchicalRelationDto
{
    public string RelatedTerm { get; set; } = string.Empty;
    public string RelationType { get; set; } = string.Empty; // Parent, Child, Sibling
    public string Description { get; set; } = string.Empty;
}

public class DisambiguationRuleDto
{
    public string Context { get; set; } = string.Empty;
    public string PreferredMeaning { get; set; } = string.Empty;
    public List<string> Keywords { get; set; } = new();
}

public class ContextualVariationDto
{
    public string Context { get; set; } = string.Empty;
    public string VariantMeaning { get; set; } = string.Empty;
    public List<string> ContextKeywords { get; set; } = new();
}

public class TableRelevanceDto
{
    public string TableName { get; set; } = string.Empty;
    public string SchemaName { get; set; } = string.Empty;
    public decimal RelevanceScore { get; set; } = 0.0m;
    public string ReasonForRelevance { get; set; } = string.Empty;
}

public class ColumnRelevanceDto
{
    public string TableName { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public decimal RelevanceScore { get; set; } = 0.0m;
    public string ReasonForRelevance { get; set; } = string.Empty;
}

/// <summary>
/// Request model for creating enhanced semantic mappings
/// </summary>
public class CreateSemanticMappingRequest
{
    [Required]
    public string QueryIntent { get; set; } = string.Empty;
    
    [Required]
    public List<TableRelevanceDto> RelevantTables { get; set; } = new();
    
    public List<ColumnRelevanceDto> RelevantColumns { get; set; } = new();
    public List<string> BusinessTerms { get; set; } = new();
    public string QueryCategory { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; } = 0.0m;
}

/// <summary>
/// Request model for updating business table semantic metadata
/// </summary>
public class UpdateTableSemanticRequest
{
    public string DomainClassification { get; set; } = string.Empty;
    public List<string> NaturalLanguageAliases { get; set; } = new();
    public string BusinessOwner { get; set; } = string.Empty;
    public List<string> DataGovernancePolicies { get; set; } = new();
    public decimal ImportanceScore { get; set; } = 0.5m;
}
