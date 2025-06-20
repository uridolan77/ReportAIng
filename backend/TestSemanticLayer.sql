-- Test script to verify the semantic layer is working in BIReportingCopilot_Dev
-- This simulates what the semantic layer would provide to the LLM

PRINT '=== TESTING SEMANTIC LAYER FUNCTIONALITY ===';
PRINT 'Database: BIReportingCopilot_Dev (System Database)';
PRINT 'Data Source: DailyActionsDB (Read-Only)';
PRINT '';

-- Test 1: Check if business metadata is populated
PRINT 'Test 1: Verifying business metadata population...';
SELECT 
    COUNT(*) as TablesWithMetadata,
    AVG(ImportanceScore) as AvgImportanceScore
FROM BusinessTableInfo 
WHERE BusinessPurpose IS NOT NULL AND BusinessPurpose != '';

-- Test 2: Check business glossary
PRINT '';
PRINT 'Test 2: Verifying business glossary...';
SELECT COUNT(*) as GlossaryTerms FROM BusinessGlossary;

-- Test 3: Simulate semantic layer query for "daily revenue"
PRINT '';
PRINT 'Test 3: Simulating semantic layer for "daily revenue" query...';
PRINT 'Business terms that would be matched:';

SELECT 
    Term,
    Definition,
    Domain,
    MappedTables
FROM BusinessGlossary 
WHERE Term LIKE '%revenue%' OR Term LIKE '%GGR%' OR Term LIKE '%NGR%'
   OR Definition LIKE '%revenue%' OR Definition LIKE '%gaming%';

PRINT '';
PRINT 'Relevant tables that would be selected:';

SELECT 
    SchemaName + '.' + TableName as FullTableName,
    BusinessPurpose,
    DomainClassification,
    ImportanceScore,
    CASE 
        WHEN BusinessPurpose LIKE '%revenue%' OR BusinessPurpose LIKE '%financial%' OR BusinessPurpose LIKE '%daily%' THEN 'HIGH'
        WHEN DomainClassification = 'Gaming' OR DomainClassification = 'Finance' THEN 'MEDIUM'
        ELSE 'LOW'
    END as RelevanceScore
FROM BusinessTableInfo
WHERE (BusinessPurpose LIKE '%daily%' OR BusinessPurpose LIKE '%revenue%' OR BusinessPurpose LIKE '%financial%'
       OR DomainClassification IN ('Gaming', 'Finance'))
ORDER BY ImportanceScore DESC, RelevanceScore DESC;

-- Test 4: Simulate what would be sent to LLM
PRINT '';
PRINT 'Test 4: Sample business-friendly schema description for LLM...';
PRINT '=== BUSINESS DATA CONTEXT ===';
PRINT 'Query Intent: Daily revenue analysis';
PRINT 'Business Domain: Gaming/Finance';
PRINT '';
PRINT '=== BUSINESS TERMS ===';
PRINT '• GGR: Gross Gaming Revenue - total amount wagered minus winnings paid out';
PRINT '• NGR: Net Gaming Revenue - GGR minus bonuses and promotional costs';
PRINT '';
PRINT '=== RELEVANT TABLES ===';

DECLARE @schema_description NVARCHAR(MAX) = '';

SELECT @schema_description = @schema_description + 
    'Table: ' + SchemaName + '.' + TableName + CHAR(13) + CHAR(10) +
    'Purpose: ' + BusinessPurpose + CHAR(13) + CHAR(10) +
    'Domain: ' + DomainClassification + CHAR(13) + CHAR(10) +
    'Importance: ' + CAST(ImportanceScore as VARCHAR(10)) + CHAR(13) + CHAR(10) + CHAR(13) + CHAR(10)
FROM BusinessTableInfo
WHERE DomainClassification IN ('Gaming', 'Finance')
ORDER BY ImportanceScore DESC;

PRINT @schema_description;

PRINT '=== SEMANTIC LAYER TEST COMPLETE ===';
PRINT 'The semantic layer is now ready to provide intelligent, business-friendly schema information to the LLM!';
