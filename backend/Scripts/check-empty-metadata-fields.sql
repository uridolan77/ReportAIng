-- Check current state of business metadata tables for empty semantic fields
-- This script identifies records with empty semantic fields that can be enhanced

USE [BIReportingCopilot_Dev]
GO

PRINT '🔍 Checking BusinessColumnInfo for empty semantic fields...'
PRINT '============================================================'

SELECT 
    'BusinessColumnInfo' as TableName,
    COUNT(*) as TotalRecords,
    SUM(CASE WHEN SemanticContext IS NULL OR SemanticContext = '' THEN 1 ELSE 0 END) as EmptySemanticContext,
    SUM(CASE WHEN AnalyticalContext IS NULL OR AnalyticalContext = '' THEN 1 ELSE 0 END) as EmptyAnalyticalContext,
    SUM(CASE WHEN SemanticRelevanceScore = 0 OR SemanticRelevanceScore IS NULL THEN 1 ELSE 0 END) as EmptySemanticRelevanceScore,
    SUM(CASE WHEN ConceptualRelationships IS NULL OR ConceptualRelationships = '' THEN 1 ELSE 0 END) as EmptyConceptualRelationships,
    SUM(CASE WHEN DomainSpecificTerms IS NULL OR DomainSpecificTerms = '' THEN 1 ELSE 0 END) as EmptyDomainSpecificTerms,
    SUM(CASE WHEN QueryIntentMapping IS NULL OR QueryIntentMapping = '' THEN 1 ELSE 0 END) as EmptyQueryIntentMapping,
    SUM(CASE WHEN BusinessQuestionTypes IS NULL OR BusinessQuestionTypes = '' THEN 1 ELSE 0 END) as EmptyBusinessQuestionTypes,
    SUM(CASE WHEN SemanticSynonyms IS NULL OR SemanticSynonyms = '' THEN 1 ELSE 0 END) as EmptySemanticSynonyms,
    SUM(CASE WHEN BusinessMetrics IS NULL OR BusinessMetrics = '' THEN 1 ELSE 0 END) as EmptyBusinessMetrics,
    SUM(CASE WHEN LLMPromptHints IS NULL OR LLMPromptHints = '' THEN 1 ELSE 0 END) as EmptyLLMPromptHints,
    SUM(CASE WHEN VectorSearchTags IS NULL OR VectorSearchTags = '' THEN 1 ELSE 0 END) as EmptyVectorSearchTags
FROM [dbo].[BusinessColumnInfo]

PRINT ''
PRINT '🔍 Checking BusinessTableInfo for empty semantic fields...'
PRINT '========================================================='

SELECT 
    'BusinessTableInfo' as TableName,
    COUNT(*) as TotalRecords,
    SUM(CASE WHEN SemanticDescription IS NULL OR SemanticDescription = '' THEN 1 ELSE 0 END) as EmptySemanticDescription,
    SUM(CASE WHEN LLMContextHints IS NULL OR LLMContextHints = '' THEN 1 ELSE 0 END) as EmptyLLMContextHints,
    SUM(CASE WHEN VectorSearchKeywords IS NULL OR VectorSearchKeywords = '' THEN 1 ELSE 0 END) as EmptyVectorSearchKeywords,
    SUM(CASE WHEN BusinessGlossaryTerms IS NULL OR BusinessGlossaryTerms = '' THEN 1 ELSE 0 END) as EmptyBusinessGlossaryTerms,
    SUM(CASE WHEN BusinessProcesses IS NULL OR BusinessProcesses = '' THEN 1 ELSE 0 END) as EmptyBusinessProcesses,
    SUM(CASE WHEN AnalyticalUseCases IS NULL OR AnalyticalUseCases = '' THEN 1 ELSE 0 END) as EmptyAnalyticalUseCases,
    SUM(CASE WHEN SemanticRelationships IS NULL OR SemanticRelationships = '' THEN 1 ELSE 0 END) as EmptySemanticRelationships
FROM [dbo].[BusinessTableInfo]

PRINT ''
PRINT '🔍 Checking BusinessGlossary for empty semantic fields...'
PRINT '======================================================='

SELECT 
    'BusinessGlossary' as TableName,
    COUNT(*) as TotalRecords,
    SUM(CASE WHEN ContextualVariations IS NULL OR ContextualVariations = '' THEN 1 ELSE 0 END) as EmptyContextualVariations,
    SUM(CASE WHEN QueryPatterns IS NULL OR QueryPatterns = '' THEN 1 ELSE 0 END) as EmptyQueryPatterns,
    SUM(CASE WHEN LLMPromptTemplates IS NULL OR LLMPromptTemplates = '' THEN 1 ELSE 0 END) as EmptyLLMPromptTemplates,
    SUM(CASE WHEN DisambiguationContext IS NULL OR DisambiguationContext = '' THEN 1 ELSE 0 END) as EmptyDisambiguationContext,
    SUM(CASE WHEN SemanticRelationships IS NULL OR SemanticRelationships = '' THEN 1 ELSE 0 END) as EmptySemanticRelationships,
    SUM(CASE WHEN ConceptualLevel IS NULL OR ConceptualLevel = '' THEN 1 ELSE 0 END) as EmptyConceptualLevel,
    SUM(CASE WHEN CrossDomainMappings IS NULL OR CrossDomainMappings = '' THEN 1 ELSE 0 END) as EmptyCrossDomainMappings,
    SUM(CASE WHEN InferenceRules IS NULL OR InferenceRules = '' THEN 1 ELSE 0 END) as EmptyInferenceRules,
    SUM(CASE WHEN SemanticStability = 0 OR SemanticStability IS NULL THEN 1 ELSE 0 END) as EmptySemanticStability
FROM [dbo].[BusinessGlossary]

PRINT ''
PRINT '📊 Sample records with empty fields (BusinessColumnInfo):'
PRINT '========================================================'

SELECT TOP 10
    bci.ColumnName,
    bti.TableName,
    bci.BusinessMeaning,
    CASE WHEN bci.SemanticContext IS NULL OR bci.SemanticContext = '' THEN 'EMPTY' ELSE 'FILLED' END as SemanticContext_Status,
    CASE WHEN bci.AnalyticalContext IS NULL OR bci.AnalyticalContext = '' THEN 'EMPTY' ELSE 'FILLED' END as AnalyticalContext_Status,
    CASE WHEN bci.SemanticRelevanceScore = 0 OR bci.SemanticRelevanceScore IS NULL THEN 'EMPTY' ELSE 'FILLED' END as SemanticRelevanceScore_Status
FROM [dbo].[BusinessColumnInfo] bci
LEFT JOIN [dbo].[BusinessTableInfo] bti ON bci.TableInfoId = bti.Id
WHERE (bci.SemanticContext IS NULL OR bci.SemanticContext = '')
   OR (bci.AnalyticalContext IS NULL OR bci.AnalyticalContext = '')
   OR (bci.SemanticRelevanceScore = 0 OR bci.SemanticRelevanceScore IS NULL)
ORDER BY bti.TableName, bci.ColumnName

PRINT ''
PRINT '📊 Sample records with empty fields (BusinessTableInfo):'
PRINT '======================================================='

SELECT TOP 10
    TableName,
    BusinessPurpose,
    CASE WHEN SemanticDescription IS NULL OR SemanticDescription = '' THEN 'EMPTY' ELSE 'FILLED' END as SemanticDescription_Status,
    CASE WHEN LLMContextHints IS NULL OR LLMContextHints = '' THEN 'EMPTY' ELSE 'FILLED' END as LLMContextHints_Status
FROM [dbo].[BusinessTableInfo]
WHERE (SemanticDescription IS NULL OR SemanticDescription = '')
   OR (LLMContextHints IS NULL OR LLMContextHints = '')
ORDER BY TableName

PRINT ''
PRINT '📊 Sample records with empty fields (BusinessGlossary):'
PRINT '====================================================='

SELECT TOP 10
    Term,
    Definition,
    Category,
    CASE WHEN ContextualVariations IS NULL OR ContextualVariations = '' THEN 'EMPTY' ELSE 'FILLED' END as ContextualVariations_Status,
    CASE WHEN QueryPatterns IS NULL OR QueryPatterns = '' THEN 'EMPTY' ELSE 'FILLED' END as QueryPatterns_Status
FROM [dbo].[BusinessGlossary]
WHERE (ContextualVariations IS NULL OR ContextualVariations = '')
   OR (QueryPatterns IS NULL OR QueryPatterns = '')
ORDER BY Term

PRINT ''
PRINT '✅ Analysis complete! Use this data to run the MetadataEnhancementService.'
