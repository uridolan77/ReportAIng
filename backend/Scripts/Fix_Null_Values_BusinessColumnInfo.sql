-- =====================================================================================
-- Fix NULL Values in BusinessColumnInfo Table
-- =====================================================================================

USE [BIReportingCopilot_Dev]
GO

PRINT 'Fixing NULL values in BusinessColumnInfo table...'

-- Update NULL values in semantic fields to empty strings
UPDATE [dbo].[BusinessColumnInfo] 
SET 
    [SemanticContext] = ISNULL([SemanticContext], ''),
    [ConceptualRelationships] = ISNULL([ConceptualRelationships], ''),
    [DomainSpecificTerms] = ISNULL([DomainSpecificTerms], ''),
    [QueryIntentMapping] = ISNULL([QueryIntentMapping], ''),
    [BusinessQuestionTypes] = ISNULL([BusinessQuestionTypes], ''),
    [SemanticSynonyms] = ISNULL([SemanticSynonyms], ''),
    [AnalyticalContext] = ISNULL([AnalyticalContext], ''),
    [BusinessMetrics] = ISNULL([BusinessMetrics], ''),
    [LLMPromptHints] = ISNULL([LLMPromptHints], ''),
    [VectorSearchTags] = ISNULL([VectorSearchTags], ''),
    [UpdatedDate] = GETUTCDATE(),
    [UpdatedBy] = 'System_NullValueFix'
WHERE 
    [SemanticContext] IS NULL 
    OR [ConceptualRelationships] IS NULL 
    OR [DomainSpecificTerms] IS NULL
    OR [QueryIntentMapping] IS NULL
    OR [BusinessQuestionTypes] IS NULL
    OR [SemanticSynonyms] IS NULL
    OR [AnalyticalContext] IS NULL
    OR [BusinessMetrics] IS NULL
    OR [LLMPromptHints] IS NULL
    OR [VectorSearchTags] IS NULL

PRINT 'Updated ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' rows with NULL values'

-- Verify the fix
SELECT 
    COUNT(*) as TotalRows,
    SUM(CASE WHEN [SemanticContext] IS NULL THEN 1 ELSE 0 END) as NullSemanticContext,
    SUM(CASE WHEN [ConceptualRelationships] IS NULL THEN 1 ELSE 0 END) as NullConceptualRelationships,
    SUM(CASE WHEN [DomainSpecificTerms] IS NULL THEN 1 ELSE 0 END) as NullDomainSpecificTerms,
    SUM(CASE WHEN [QueryIntentMapping] IS NULL THEN 1 ELSE 0 END) as NullQueryIntentMapping,
    SUM(CASE WHEN [BusinessQuestionTypes] IS NULL THEN 1 ELSE 0 END) as NullBusinessQuestionTypes,
    SUM(CASE WHEN [SemanticSynonyms] IS NULL THEN 1 ELSE 0 END) as NullSemanticSynonyms,
    SUM(CASE WHEN [AnalyticalContext] IS NULL THEN 1 ELSE 0 END) as NullAnalyticalContext,
    SUM(CASE WHEN [BusinessMetrics] IS NULL THEN 1 ELSE 0 END) as NullBusinessMetrics,
    SUM(CASE WHEN [LLMPromptHints] IS NULL THEN 1 ELSE 0 END) as NullLLMPromptHints,
    SUM(CASE WHEN [VectorSearchTags] IS NULL THEN 1 ELSE 0 END) as NullVectorSearchTags
FROM [dbo].[BusinessColumnInfo]

PRINT 'NULL value fix completed successfully!'
