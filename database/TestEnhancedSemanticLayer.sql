-- =====================================================
-- Test Enhanced Semantic Layer - Phase 2
-- =====================================================
-- This script tests the enhanced semantic metadata and validates
-- the Phase 2 semantic layer enhancements

-- =====================================================
-- Test 1: Verify Enhanced Table Metadata
-- =====================================================
SELECT 
    '=== ENHANCED TABLE METADATA TEST ===' as Section,
    TableName,
    CASE 
        WHEN LEN(SemanticDescription) > 100 THEN '✅ Rich Description'
        WHEN LEN(SemanticDescription) > 0 THEN '⚠️ Basic Description'
        ELSE '❌ No Description'
    END as DescriptionStatus,
    CASE 
        WHEN LEN(BusinessProcesses) > 10 THEN '✅ Processes Defined'
        ELSE '❌ No Processes'
    END as ProcessesStatus,
    CASE 
        WHEN LEN(AnalyticalUseCases) > 10 THEN '✅ Use Cases Defined'
        ELSE '❌ No Use Cases'
    END as UseCasesStatus,
    CASE 
        WHEN LEN(VectorSearchKeywords) > 10 THEN '✅ Keywords Defined'
        ELSE '❌ No Keywords'
    END as KeywordsStatus,
    SemanticCoverageScore
FROM BusinessTableInfo
WHERE IsActive = 1
ORDER BY TableName

-- =====================================================
-- Test 2: Verify Enhanced Column Metadata
-- =====================================================
SELECT 
    '=== ENHANCED COLUMN METADATA TEST ===' as Section,
    bti.TableName,
    bci.ColumnName,
    CASE 
        WHEN LEN(bci.SemanticContext) > 50 THEN '✅ Rich Context'
        WHEN LEN(bci.SemanticContext) > 0 THEN '⚠️ Basic Context'
        ELSE '❌ No Context'
    END as ContextStatus,
    CASE 
        WHEN LEN(bci.SemanticSynonyms) > 10 THEN '✅ Synonyms Defined'
        ELSE '❌ No Synonyms'
    END as SynonymsStatus,
    CASE 
        WHEN LEN(bci.BusinessMetrics) > 10 THEN '✅ Metrics Defined'
        ELSE '❌ No Metrics'
    END as MetricsStatus,
    bci.SemanticRelevanceScore
FROM BusinessColumnInfo bci
JOIN BusinessTableInfo bti ON bci.TableInfoId = bti.Id
WHERE bci.IsActive = 1 
AND (LEN(bci.SemanticContext) > 0 OR LEN(bci.SemanticSynonyms) > 0)
ORDER BY bti.TableName, bci.ColumnName

-- =====================================================
-- Test 3: Verify Enhanced Business Glossary
-- =====================================================
SELECT 
    '=== ENHANCED BUSINESS GLOSSARY TEST ===' as Section,
    Term,
    CASE 
        WHEN LEN(QueryPatterns) > 10 THEN '✅ Query Patterns'
        ELSE '❌ No Patterns'
    END as PatternsStatus,
    CASE 
        WHEN LEN(LLMPromptTemplates) > 10 THEN '✅ LLM Templates'
        ELSE '❌ No Templates'
    END as TemplatesStatus,
    CASE 
        WHEN LEN(SemanticRelationships) > 10 THEN '✅ Relationships'
        ELSE '❌ No Relationships'
    END as RelationshipsStatus,
    ConceptualLevel,
    SemanticStability
FROM BusinessGlossary
WHERE IsActive = 1
AND (LEN(QueryPatterns) > 0 OR LEN(LLMPromptTemplates) > 0)
ORDER BY Term

-- =====================================================
-- Test 4: Semantic Coverage Analysis
-- =====================================================
SELECT 
    '=== SEMANTIC COVERAGE ANALYSIS ===' as Section,
    COUNT(*) as TotalTables,
    SUM(CASE WHEN LEN(SemanticDescription) > 0 THEN 1 ELSE 0 END) as TablesWithDescription,
    SUM(CASE WHEN LEN(BusinessProcesses) > 0 THEN 1 ELSE 0 END) as TablesWithProcesses,
    SUM(CASE WHEN LEN(AnalyticalUseCases) > 0 THEN 1 ELSE 0 END) as TablesWithUseCases,
    SUM(CASE WHEN LEN(VectorSearchKeywords) > 0 THEN 1 ELSE 0 END) as TablesWithKeywords,
    AVG(SemanticCoverageScore) as AvgCoverageScore,
    CASE 
        WHEN AVG(SemanticCoverageScore) >= 0.9 THEN '🎉 EXCELLENT'
        WHEN AVG(SemanticCoverageScore) >= 0.7 THEN '✅ GOOD'
        WHEN AVG(SemanticCoverageScore) >= 0.5 THEN '⚠️ FAIR'
        ELSE '❌ POOR'
    END as OverallStatus
FROM BusinessTableInfo
WHERE IsActive = 1

-- =====================================================
-- Test 5: Column Semantic Enrichment Analysis
-- =====================================================
SELECT 
    '=== COLUMN SEMANTIC ENRICHMENT ===' as Section,
    COUNT(*) as TotalColumns,
    SUM(CASE WHEN LEN(SemanticContext) > 0 THEN 1 ELSE 0 END) as ColumnsWithContext,
    SUM(CASE WHEN LEN(SemanticSynonyms) > 0 THEN 1 ELSE 0 END) as ColumnsWithSynonyms,
    SUM(CASE WHEN LEN(BusinessMetrics) > 0 THEN 1 ELSE 0 END) as ColumnsWithMetrics,
    SUM(CASE WHEN LEN(LLMPromptHints) > 0 THEN 1 ELSE 0 END) as ColumnsWithLLMHints,
    AVG(SemanticRelevanceScore) as AvgRelevanceScore,
    CASE 
        WHEN AVG(SemanticRelevanceScore) >= 0.8 THEN '🎉 EXCELLENT'
        WHEN AVG(SemanticRelevanceScore) >= 0.6 THEN '✅ GOOD'
        WHEN AVG(SemanticRelevanceScore) >= 0.4 THEN '⚠️ FAIR'
        ELSE '❌ POOR'
    END as OverallStatus
FROM BusinessColumnInfo
WHERE IsActive = 1

-- =====================================================
-- Test 6: Business Glossary Enhancement Analysis
-- =====================================================
SELECT 
    '=== BUSINESS GLOSSARY ENHANCEMENT ===' as Section,
    COUNT(*) as TotalTerms,
    SUM(CASE WHEN LEN(QueryPatterns) > 0 THEN 1 ELSE 0 END) as TermsWithPatterns,
    SUM(CASE WHEN LEN(LLMPromptTemplates) > 0 THEN 1 ELSE 0 END) as TermsWithTemplates,
    SUM(CASE WHEN LEN(SemanticRelationships) > 0 THEN 1 ELSE 0 END) as TermsWithRelationships,
    SUM(CASE WHEN LEN(InferenceRules) > 0 THEN 1 ELSE 0 END) as TermsWithRules,
    AVG(SemanticStability) as AvgStability,
    CASE 
        WHEN AVG(SemanticStability) >= 0.9 THEN '🎉 VERY STABLE'
        WHEN AVG(SemanticStability) >= 0.7 THEN '✅ STABLE'
        WHEN AVG(SemanticStability) >= 0.5 THEN '⚠️ MODERATE'
        ELSE '❌ UNSTABLE'
    END as StabilityStatus
FROM BusinessGlossary
WHERE IsActive = 1

-- =====================================================
-- Test 7: Semantic Search Readiness
-- =====================================================
SELECT 
    '=== SEMANTIC SEARCH READINESS ===' as Section,
    'Tables' as ElementType,
    COUNT(*) as TotalElements,
    SUM(CASE WHEN LEN(VectorSearchKeywords) > 0 THEN 1 ELSE 0 END) as ElementsWithKeywords,
    CAST(ROUND((SUM(CASE WHEN LEN(VectorSearchKeywords) > 0 THEN 1 ELSE 0 END) * 100.0 / COUNT(*)), 1) as VARCHAR(10)) + '%' as ReadinessPercentage
FROM BusinessTableInfo
WHERE IsActive = 1

UNION ALL

SELECT 
    '=== SEMANTIC SEARCH READINESS ===' as Section,
    'Columns' as ElementType,
    COUNT(*) as TotalElements,
    SUM(CASE WHEN LEN(VectorSearchTags) > 0 THEN 1 ELSE 0 END) as ElementsWithKeywords,
    CAST(ROUND((SUM(CASE WHEN LEN(VectorSearchTags) > 0 THEN 1 ELSE 0 END) * 100.0 / COUNT(*)), 1) as VARCHAR(10)) + '%' as ReadinessPercentage
FROM BusinessColumnInfo
WHERE IsActive = 1

-- =====================================================
-- Test 8: LLM Context Optimization
-- =====================================================
SELECT 
    '=== LLM CONTEXT OPTIMIZATION ===' as Section,
    'Tables' as ElementType,
    COUNT(*) as TotalElements,
    SUM(CASE WHEN LEN(LLMContextHints) > 0 THEN 1 ELSE 0 END) as ElementsWithHints,
    CAST(ROUND((SUM(CASE WHEN LEN(LLMContextHints) > 0 THEN 1 ELSE 0 END) * 100.0 / COUNT(*)), 1) as VARCHAR(10)) + '%' as OptimizationPercentage
FROM BusinessTableInfo
WHERE IsActive = 1

UNION ALL

SELECT 
    '=== LLM CONTEXT OPTIMIZATION ===' as Section,
    'Columns' as ElementType,
    COUNT(*) as TotalElements,
    SUM(CASE WHEN LEN(LLMPromptHints) > 0 THEN 1 ELSE 0 END) as ElementsWithHints,
    CAST(ROUND((SUM(CASE WHEN LEN(LLMPromptHints) > 0 THEN 1 ELSE 0 END) * 100.0 / COUNT(*)), 1) as VARCHAR(10)) + '%' as OptimizationPercentage
FROM BusinessColumnInfo
WHERE IsActive = 1

UNION ALL

SELECT 
    '=== LLM CONTEXT OPTIMIZATION ===' as Section,
    'Glossary Terms' as ElementType,
    COUNT(*) as TotalElements,
    SUM(CASE WHEN LEN(LLMPromptTemplates) > 0 THEN 1 ELSE 0 END) as ElementsWithHints,
    CAST(ROUND((SUM(CASE WHEN LEN(LLMPromptTemplates) > 0 THEN 1 ELSE 0 END) * 100.0 / COUNT(*)), 1) as VARCHAR(10)) + '%' as OptimizationPercentage
FROM BusinessGlossary
WHERE IsActive = 1

-- =====================================================
-- Test 9: Phase 2 Enhancement Summary
-- =====================================================
SELECT 
    '=== PHASE 2 ENHANCEMENT SUMMARY ===' as Section,
    'Enhanced Semantic Layer Status' as Metric,
    CASE 
        WHEN (SELECT AVG(SemanticCoverageScore) FROM BusinessTableInfo WHERE IsActive = 1) >= 0.9 
        AND (SELECT COUNT(*) FROM BusinessTableInfo WHERE IsActive = 1 AND LEN(SemanticDescription) > 0) = (SELECT COUNT(*) FROM BusinessTableInfo WHERE IsActive = 1)
        THEN '🎉 PHASE 2 COMPLETE - WORLD-CLASS SEMANTIC LAYER'
        WHEN (SELECT AVG(SemanticCoverageScore) FROM BusinessTableInfo WHERE IsActive = 1) >= 0.7
        THEN '✅ PHASE 2 GOOD PROGRESS'
        ELSE '⚠️ PHASE 2 IN PROGRESS'
    END as Status,
    CAST((SELECT AVG(SemanticCoverageScore) FROM BusinessTableInfo WHERE IsActive = 1) as VARCHAR(10)) as AvgCoverageScore,
    (SELECT COUNT(*) FROM BusinessTableInfo WHERE IsActive = 1) as TotalTables,
    (SELECT COUNT(*) FROM BusinessColumnInfo WHERE IsActive = 1) as TotalColumns,
    (SELECT COUNT(*) FROM BusinessGlossary WHERE IsActive = 1) as TotalGlossaryTerms
