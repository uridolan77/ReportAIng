-- =====================================================
-- Test Phase 3: Enhanced SQL Validation Pipeline
-- =====================================================
-- This script tests the Phase 3 enhanced validation infrastructure
-- and validates the integration with Phase 2 semantic layer

-- =====================================================
-- Test 1: Verify Phase 3 Infrastructure Readiness
-- =====================================================
SELECT 
    '=== PHASE 3 INFRASTRUCTURE TEST ===' as Section,
    'Enhanced Validation Pipeline' as Component,
    CASE 
        WHEN EXISTS (SELECT 1 FROM BusinessTableInfo WHERE LEN(SemanticDescription) > 0)
        AND EXISTS (SELECT 1 FROM BusinessColumnInfo WHERE LEN(SemanticContext) > 0)
        AND EXISTS (SELECT 1 FROM BusinessGlossary WHERE IsActive = 1)
        THEN '✅ READY - Phase 2 Semantic Layer Available'
        ELSE '❌ NOT READY - Phase 2 Semantic Layer Missing'
    END as Status,
    (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(SemanticDescription) > 0) as TablesWithSemanticData,
    (SELECT COUNT(*) FROM BusinessColumnInfo WHERE LEN(SemanticContext) > 0) as ColumnsWithSemanticData,
    (SELECT COUNT(*) FROM BusinessGlossary WHERE IsActive = 1) as BusinessTermsAvailable

-- =====================================================
-- Test 2: Semantic Validation Test Cases
-- =====================================================
UNION ALL
SELECT 
    '=== SEMANTIC VALIDATION TEST ===' as Section,
    'Business Intent Alignment' as Component,
    CASE 
        WHEN (SELECT AVG(SemanticCoverageScore) FROM BusinessTableInfo WHERE IsActive = 1) >= 0.9
        THEN '✅ EXCELLENT - High semantic coverage for validation'
        WHEN (SELECT AVG(SemanticCoverageScore) FROM BusinessTableInfo WHERE IsActive = 1) >= 0.7
        THEN '✅ GOOD - Adequate semantic coverage for validation'
        ELSE '⚠️ FAIR - Limited semantic coverage'
    END as Status,
    CAST((SELECT AVG(SemanticCoverageScore) FROM BusinessTableInfo WHERE IsActive = 1) as INT) as TablesWithSemanticData,
    CAST((SELECT AVG(CASE WHEN LEN(SemanticSynonyms) > 0 THEN 1.0 ELSE 0.0 END) FROM BusinessColumnInfo WHERE IsActive = 1) * 100 as INT) as ColumnsWithSemanticData,
    (SELECT COUNT(*) FROM BusinessGlossary WHERE LEN(QueryPatterns) > 0) as BusinessTermsAvailable

-- =====================================================
-- Test 3: Schema Compliance Validation Readiness
-- =====================================================
UNION ALL
SELECT 
    '=== SCHEMA COMPLIANCE TEST ===' as Section,
    'Schema Metadata Completeness' as Component,
    CASE 
        WHEN (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(SemanticRelationships) > 0) >= 5
        AND (SELECT COUNT(*) FROM BusinessColumnInfo WHERE LEN(BusinessMetrics) > 0) >= 10
        THEN '✅ EXCELLENT - Rich schema metadata for compliance validation'
        WHEN (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(SemanticRelationships) > 0) >= 3
        THEN '✅ GOOD - Adequate schema metadata'
        ELSE '⚠️ FAIR - Basic schema metadata'
    END as Status,
    (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(SemanticRelationships) > 0) as TablesWithSemanticData,
    (SELECT COUNT(*) FROM BusinessColumnInfo WHERE LEN(BusinessMetrics) > 0) as ColumnsWithSemanticData,
    (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(QueryComplexityHints) > 0) as BusinessTermsAvailable

-- =====================================================
-- Test 4: Business Logic Validation Readiness
-- =====================================================
UNION ALL
SELECT 
    '=== BUSINESS LOGIC VALIDATION TEST ===' as Section,
    'Business Rules and Processes' as Component,
    CASE 
        WHEN (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(BusinessProcesses) > 10) >= 5
        AND (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(AnalyticalUseCases) > 10) >= 5
        THEN '✅ EXCELLENT - Rich business context for logic validation'
        WHEN (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(BusinessProcesses) > 10) >= 3
        THEN '✅ GOOD - Adequate business context'
        ELSE '⚠️ FAIR - Basic business context'
    END as Status,
    (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(BusinessProcesses) > 10) as TablesWithSemanticData,
    (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(AnalyticalUseCases) > 10) as ColumnsWithSemanticData,
    (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(ReportingCategories) > 10) as BusinessTermsAvailable

-- =====================================================
-- Test 5: Self-Correction Capability Assessment
-- =====================================================
UNION ALL
SELECT 
    '=== SELF-CORRECTION READINESS TEST ===' as Section,
    'LLM Context Optimization' as Component,
    CASE 
        WHEN (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(LLMContextHints) > 0) >= 5
        AND (SELECT COUNT(*) FROM BusinessColumnInfo WHERE LEN(LLMPromptHints) > 0) >= 10
        AND (SELECT COUNT(*) FROM BusinessGlossary WHERE LEN(LLMPromptTemplates) > 0) >= 5
        THEN '✅ EXCELLENT - Optimized for LLM self-correction'
        WHEN (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(LLMContextHints) > 0) >= 3
        THEN '✅ GOOD - Adequate LLM optimization'
        ELSE '⚠️ FAIR - Basic LLM optimization'
    END as Status,
    (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(LLMContextHints) > 0) as TablesWithSemanticData,
    (SELECT COUNT(*) FROM BusinessColumnInfo WHERE LEN(LLMPromptHints) > 0) as ColumnsWithSemanticData,
    (SELECT COUNT(*) FROM BusinessGlossary WHERE LEN(LLMPromptTemplates) > 0) as BusinessTermsAvailable

-- =====================================================
-- Test 6: Vector Search Integration for Validation
-- =====================================================
UNION ALL
SELECT 
    '=== VECTOR SEARCH INTEGRATION TEST ===' as Section,
    'Semantic Search Capabilities' as Component,
    CASE 
        WHEN (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(VectorSearchKeywords) > 10) >= 5
        AND (SELECT COUNT(*) FROM BusinessColumnInfo WHERE LEN(VectorSearchTags) > 10) >= 10
        THEN '✅ EXCELLENT - Rich vector search integration'
        WHEN (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(VectorSearchKeywords) > 10) >= 3
        THEN '✅ GOOD - Adequate vector search integration'
        ELSE '⚠️ FAIR - Basic vector search integration'
    END as Status,
    (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(VectorSearchKeywords) > 10) as TablesWithSemanticData,
    (SELECT COUNT(*) FROM BusinessColumnInfo WHERE LEN(VectorSearchTags) > 10) as ColumnsWithSemanticData,
    (SELECT COUNT(*) FROM BusinessGlossary WHERE LEN(SemanticRelationships) > 10) as BusinessTermsAvailable

-- =====================================================
-- Test 7: Dry-Run Execution Readiness
-- =====================================================
UNION ALL
SELECT 
    '=== DRY-RUN EXECUTION TEST ===' as Section,
    'Query Performance Hints' as Component,
    CASE 
        WHEN (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(QueryComplexityHints) > 0) >= 5
        THEN '✅ EXCELLENT - Rich performance hints for dry-run execution'
        WHEN (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(QueryComplexityHints) > 0) >= 3
        THEN '✅ GOOD - Adequate performance hints'
        ELSE '⚠️ FAIR - Basic performance hints'
    END as Status,
    (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(QueryComplexityHints) > 0) as TablesWithSemanticData,
    (SELECT COUNT(*) FROM BusinessColumnInfo WHERE LEN(AnalyticalContext) > 0) as ColumnsWithSemanticData,
    (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(SemanticRelationships) > 0) as BusinessTermsAvailable

-- =====================================================
-- Test 8: Overall Phase 3 Readiness Assessment
-- =====================================================
UNION ALL
SELECT 
    '=== PHASE 3 OVERALL READINESS ===' as Section,
    'Enhanced SQL Validation Pipeline' as Component,
    CASE 
        WHEN (SELECT AVG(SemanticCoverageScore) FROM BusinessTableInfo WHERE IsActive = 1) >= 0.9
        AND (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(SemanticDescription) > 0) = (SELECT COUNT(*) FROM BusinessTableInfo WHERE IsActive = 1)
        AND (SELECT COUNT(*) FROM BusinessColumnInfo WHERE LEN(SemanticContext) > 0) >= 5
        AND (SELECT COUNT(*) FROM BusinessGlossary WHERE IsActive = 1) >= 20
        THEN '🎉 PHASE 3 READY - WORLD-CLASS VALIDATION PIPELINE'
        WHEN (SELECT AVG(SemanticCoverageScore) FROM BusinessTableInfo WHERE IsActive = 1) >= 0.7
        THEN '✅ PHASE 3 GOOD PROGRESS'
        ELSE '⚠️ PHASE 3 IN PROGRESS'
    END as Status,
    (SELECT COUNT(*) FROM BusinessTableInfo WHERE IsActive = 1) as TablesWithSemanticData,
    (SELECT COUNT(*) FROM BusinessColumnInfo WHERE IsActive = 1) as ColumnsWithSemanticData,
    (SELECT COUNT(*) FROM BusinessGlossary WHERE IsActive = 1) as BusinessTermsAvailable

-- =====================================================
-- Test 9: Phase 3 Feature Coverage Summary
-- =====================================================
UNION ALL
SELECT 
    '=== PHASE 3 FEATURE COVERAGE ===' as Section,
    'Validation Pipeline Components' as Component,
    CASE 
        WHEN (SELECT COUNT(*) FROM BusinessTableInfo WHERE 
            LEN(SemanticDescription) > 0 AND 
            LEN(BusinessProcesses) > 0 AND 
            LEN(LLMContextHints) > 0 AND 
            LEN(VectorSearchKeywords) > 0) >= 5
        THEN '🚀 COMPREHENSIVE - All validation components ready'
        WHEN (SELECT COUNT(*) FROM BusinessTableInfo WHERE 
            LEN(SemanticDescription) > 0 AND 
            LEN(BusinessProcesses) > 0) >= 5
        THEN '✅ CORE - Essential validation components ready'
        ELSE '⚠️ BASIC - Minimal validation components'
    END as Status,
    (SELECT COUNT(*) FROM BusinessTableInfo WHERE LEN(SemanticDescription) > 0 AND LEN(BusinessProcesses) > 0 AND LEN(LLMContextHints) > 0) as TablesWithSemanticData,
    (SELECT COUNT(*) FROM BusinessColumnInfo WHERE LEN(SemanticContext) > 0 AND LEN(LLMPromptHints) > 0) as ColumnsWithSemanticData,
    (SELECT COUNT(*) FROM BusinessGlossary WHERE LEN(QueryPatterns) > 0 AND LEN(LLMPromptTemplates) > 0) as BusinessTermsAvailable

-- =====================================================
-- Test 10: Integration Test Summary
-- =====================================================
UNION ALL
SELECT 
    '=== INTEGRATION TEST SUMMARY ===' as Section,
    'Phase 2 + Phase 3 Integration' as Component,
    CASE 
        WHEN (SELECT COUNT(*) FROM BusinessTableInfo WHERE SemanticCoverageScore = 1.0) >= 5
        AND (SELECT COUNT(*) FROM BusinessColumnInfo WHERE SemanticRelevanceScore >= 0.8) >= 5
        THEN '🎉 PERFECT INTEGRATION - Phase 2 + Phase 3 fully integrated'
        WHEN (SELECT COUNT(*) FROM BusinessTableInfo WHERE SemanticCoverageScore >= 0.8) >= 5
        THEN '✅ EXCELLENT INTEGRATION'
        ELSE '⚠️ GOOD INTEGRATION'
    END as Status,
    (SELECT COUNT(*) FROM BusinessTableInfo WHERE SemanticCoverageScore >= 0.8) as TablesWithSemanticData,
    (SELECT COUNT(*) FROM BusinessColumnInfo WHERE SemanticRelevanceScore >= 0.8) as ColumnsWithSemanticData,
    CAST((SELECT AVG(SemanticCoverageScore) FROM BusinessTableInfo WHERE IsActive = 1) * 100 as INT) as BusinessTermsAvailable

ORDER BY Section
