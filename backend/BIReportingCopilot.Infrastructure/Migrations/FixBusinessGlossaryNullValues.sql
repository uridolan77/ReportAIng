-- Fix NULL values in BusinessGlossary table
-- Replace all NULL string values with empty strings to prevent SqlNullValueException

PRINT 'Fixing NULL values in BusinessGlossary table...';

UPDATE BusinessGlossary SET
    Term = ISNULL(Term, ''),
    Definition = ISNULL(Definition, ''),
    BusinessContext = ISNULL(BusinessContext, ''),
    Synonyms = ISNULL(Synonyms, ''),
    RelatedTerms = ISNULL(RelatedTerms, ''),
    Category = ISNULL(Category, ''),
    Domain = ISNULL(Domain, ''),
    Examples = ISNULL(Examples, ''),
    MappedTables = ISNULL(MappedTables, ''),
    MappedColumns = ISNULL(MappedColumns, ''),
    HierarchicalRelations = ISNULL(HierarchicalRelations, ''),
    PreferredCalculation = ISNULL(PreferredCalculation, ''),
    DisambiguationRules = ISNULL(DisambiguationRules, ''),
    BusinessOwner = ISNULL(BusinessOwner, ''),
    RegulationReferences = ISNULL(RegulationReferences, ''),
    ContextualVariations = ISNULL(ContextualVariations, ''),
    SemanticEmbedding = ISNULL(SemanticEmbedding, ''),
    QueryPatterns = ISNULL(QueryPatterns, ''),
    LLMPromptTemplates = ISNULL(LLMPromptTemplates, ''),
    DisambiguationContext = ISNULL(DisambiguationContext, ''),
    SemanticRelationships = ISNULL(SemanticRelationships, ''),
    ConceptualLevel = ISNULL(ConceptualLevel, ''),
    CrossDomainMappings = ISNULL(CrossDomainMappings, ''),
    InferenceRules = ISNULL(InferenceRules, ''),
    CreatedBy = ISNULL(CreatedBy, ''),
    UpdatedBy = ISNULL(UpdatedBy, ''),
    -- BaseEntity columns that exist
    BusinessFriendlyName = ISNULL(BusinessFriendlyName, ''),
    BusinessPurpose = ISNULL(BusinessPurpose, ''),
    NaturalLanguageDescription = ISNULL(NaturalLanguageDescription, ''),
    RelatedBusinessTerms = ISNULL(RelatedBusinessTerms, ''),
    BusinessRules = ISNULL(BusinessRules, ''),
    RelationshipContext = ISNULL(RelationshipContext, ''),
    DataGovernanceLevel = ISNULL(DataGovernanceLevel, '');

SELECT @@ROWCOUNT as UpdatedRows;

PRINT 'BusinessGlossary NULL values fixed!';

-- Verify no NULL values remain in string columns
PRINT 'Verifying no NULL values remain...';

SELECT 
    'BusinessGlossary' as TableName,
    COUNT(*) as TotalRows,
    SUM(CASE WHEN Term IS NULL THEN 1 ELSE 0 END) as NullTerm,
    SUM(CASE WHEN Definition IS NULL THEN 1 ELSE 0 END) as NullDefinition,
    SUM(CASE WHEN BusinessContext IS NULL THEN 1 ELSE 0 END) as NullBusinessContext,
    SUM(CASE WHEN Synonyms IS NULL THEN 1 ELSE 0 END) as NullSynonyms,
    SUM(CASE WHEN RelatedTerms IS NULL THEN 1 ELSE 0 END) as NullRelatedTerms,
    SUM(CASE WHEN Category IS NULL THEN 1 ELSE 0 END) as NullCategory,
    SUM(CASE WHEN BusinessPurpose IS NULL THEN 1 ELSE 0 END) as NullBusinessPurpose,
    SUM(CASE WHEN RelatedBusinessTerms IS NULL THEN 1 ELSE 0 END) as NullRelatedBusinessTerms
FROM BusinessGlossary;

PRINT 'Verification complete!';
