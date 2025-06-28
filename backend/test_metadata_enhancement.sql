-- Test script to verify metadata enhancement works correctly
-- Check current state of BusinessColumnInfo before enhancement

SELECT 
    Id,
    ColumnName,
    BusinessMeaning,
    SemanticContext,
    AnalyticalContext,
    ConceptualRelationships,
    DomainSpecificTerms,
    QueryIntentMapping,
    BusinessQuestionTypes,
    SemanticSynonyms,
    BusinessMetrics,
    LLMPromptHints,
    VectorSearchTags,
    SemanticRelevanceScore
FROM [BIReportingCopilot_Dev].[dbo].[BusinessColumnInfo]
WHERE Id IN (1, 2, 3, 4, 5)
ORDER BY Id;

-- Check which records have empty semantic fields
SELECT 
    COUNT(*) as TotalRecords,
    SUM(CASE WHEN SemanticContext = '' OR SemanticContext IS NULL THEN 1 ELSE 0 END) as EmptySemanticContext,
    SUM(CASE WHEN AnalyticalContext = '' OR AnalyticalContext IS NULL THEN 1 ELSE 0 END) as EmptyAnalyticalContext,
    SUM(CASE WHEN ConceptualRelationships = '' OR ConceptualRelationships IS NULL THEN 1 ELSE 0 END) as EmptyConceptualRelationships,
    SUM(CASE WHEN DomainSpecificTerms = '' OR DomainSpecificTerms IS NULL THEN 1 ELSE 0 END) as EmptyDomainSpecificTerms,
    SUM(CASE WHEN QueryIntentMapping = '' OR QueryIntentMapping IS NULL THEN 1 ELSE 0 END) as EmptyQueryIntentMapping,
    SUM(CASE WHEN BusinessQuestionTypes = '' OR BusinessQuestionTypes IS NULL THEN 1 ELSE 0 END) as EmptyBusinessQuestionTypes,
    SUM(CASE WHEN SemanticSynonyms = '' OR SemanticSynonyms IS NULL THEN 1 ELSE 0 END) as EmptySemanticSynonyms,
    SUM(CASE WHEN BusinessMetrics = '' OR BusinessMetrics IS NULL THEN 1 ELSE 0 END) as EmptyBusinessMetrics,
    SUM(CASE WHEN LLMPromptHints = '' OR LLMPromptHints IS NULL THEN 1 ELSE 0 END) as EmptyLLMPromptHints,
    SUM(CASE WHEN VectorSearchTags = '' OR VectorSearchTags IS NULL THEN 1 ELSE 0 END) as EmptyVectorSearchTags,
    SUM(CASE WHEN SemanticRelevanceScore = 0 THEN 1 ELSE 0 END) as ZeroSemanticRelevanceScore
FROM [BIReportingCopilot_Dev].[dbo].[BusinessColumnInfo];
