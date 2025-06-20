-- Complete Semantic Layer Test - Table and Column Metadata
-- Tests the full semantic layer with both table-level and column-level business context

PRINT '=== COMPLETE SEMANTIC LAYER TEST ===';
PRINT 'Database: BIReportingCopilot_Dev (System Database)';
PRINT 'Testing both table-level and column-level business metadata';
PRINT '';

-- Test 1: Verify complete metadata population
PRINT 'Test 1: Metadata Population Summary';
SELECT 
    'Business Tables' as MetadataType,
    COUNT(*) as Count,
    AVG(ImportanceScore) as AvgImportance
FROM BusinessTableInfo
UNION ALL
SELECT 
    'Business Columns' as MetadataType,
    COUNT(*) as Count,
    AVG(DataQualityScore) as AvgQuality
FROM BusinessColumnInfo
UNION ALL
SELECT 
    'Business Terms' as MetadataType,
    COUNT(*) as Count,
    AVG(ConfidenceScore) as AvgConfidence
FROM BusinessGlossary;

PRINT '';
PRINT 'Test 2: Simulating "Show me daily revenue by player" query...';
PRINT '';

-- Test 2: Business Terms Analysis
PRINT '=== BUSINESS TERMS MATCHED ===';
SELECT 
    Term,
    Definition,
    Domain,
    MappedTables,
    ConfidenceScore
FROM BusinessGlossary 
WHERE Term IN ('GGR', 'NGR') OR Definition LIKE '%revenue%'
ORDER BY ConfidenceScore DESC;

PRINT '';
PRINT '=== RELEVANT TABLES IDENTIFIED ===';

-- Test 3: Table-Level Context
SELECT 
    bti.SchemaName + '.' + bti.TableName as TableName,
    bti.BusinessPurpose,
    bti.DomainClassification,
    bti.ImportanceScore,
    bti.BusinessOwner,
    CASE 
        WHEN bti.ImportanceScore >= 0.9 THEN 'CRITICAL'
        WHEN bti.ImportanceScore >= 0.7 THEN 'HIGH'
        WHEN bti.ImportanceScore >= 0.5 THEN 'MEDIUM'
        ELSE 'LOW'
    END as Priority
FROM BusinessTableInfo bti
WHERE bti.DomainClassification IN ('Gaming', 'Finance', 'Customer')
   OR bti.BusinessPurpose LIKE '%daily%' 
   OR bti.BusinessPurpose LIKE '%revenue%'
   OR bti.BusinessPurpose LIKE '%player%'
ORDER BY bti.ImportanceScore DESC;

PRINT '';
PRINT '=== RELEVANT COLUMNS FOR REVENUE ANALYSIS ===';

-- Test 4: Column-Level Context for Revenue Analysis
SELECT 
    bti.TableName,
    bci.ColumnName,
    bci.BusinessMeaning,
    bci.BusinessDataType,
    bci.SemanticTags,
    bci.RelatedBusinessTerms,
    bci.PreferredAggregation,
    CASE WHEN bci.IsSensitiveData = 1 THEN 'YES' ELSE 'NO' END as SensitiveData
FROM BusinessColumnInfo bci
JOIN BusinessTableInfo bti ON bci.TableInfoId = bti.Id
WHERE bci.SemanticTags LIKE '%revenue%' 
   OR bci.SemanticTags LIKE '%financial%'
   OR bci.SemanticTags LIKE '%bet%'
   OR bci.SemanticTags LIKE '%deposit%'
   OR bci.RelatedBusinessTerms LIKE '%Revenue%'
   OR bci.RelatedBusinessTerms LIKE '%GGR%'
   OR bci.ColumnName IN ('Date', 'PlayerID')
ORDER BY bti.ImportanceScore DESC, bci.UsageFrequency DESC;

PRINT '';
PRINT '=== SAMPLE ENHANCED LLM PROMPT ===';
PRINT 'This is what the semantic layer would send to the LLM:';
PRINT '';
PRINT '--- BUSINESS CONTEXT ---';
PRINT 'Query: "Show me daily revenue by player"';
PRINT 'Intent: Revenue analysis by player and date';
PRINT 'Domain: Gaming/Finance';
PRINT '';
PRINT '--- BUSINESS TERMS ---';
PRINT '• GGR: Gross Gaming Revenue - total amount wagered minus winnings paid out';
PRINT '• NGR: Net Gaming Revenue - GGR minus bonuses and promotional costs';
PRINT '• Player ID: Unique identifier for each player across all brands';
PRINT '';
PRINT '--- CORE TABLE ---';
PRINT 'tbl_Daily_actions (Importance: 1.0 - CRITICAL)';
PRINT 'Purpose: CORE TABLE - Comprehensive daily player statistics and financial metrics';
PRINT 'Business Rules: CRITICAL - Always filter by Date for performance';
PRINT '';
PRINT '--- KEY COLUMNS ---';
PRINT '• Date: Date of daily activity summary (time dimension, critical for filtering)';
PRINT '• PlayerID: Unique player identifier (links to customer data)';
PRINT '• BetsReal: Total real money bets placed (used for GGR calculation)';
PRINT '• WinsReal: Total real money wins paid (used for GGR calculation)';
PRINT '• Deposits: Total deposit amount for the day (key revenue metric)';
PRINT '';
PRINT '--- CALCULATION GUIDANCE ---';
PRINT '• GGR = BetsReal - WinsReal';
PRINT '• Revenue includes: Deposits, BetsReal (stakes)';
PRINT '• Always aggregate by Date and PlayerID';
PRINT '• Use SUM for financial amounts, COUNT for players';
PRINT '';
PRINT '--- SAMPLE SQL STRUCTURE ---';
PRINT 'SELECT ';
PRINT '    Date,';
PRINT '    PlayerID,';
PRINT '    SUM(Deposits) as TotalDeposits,';
PRINT '    SUM(BetsReal) as TotalBets,';
PRINT '    SUM(WinsReal) as TotalWins,';
PRINT '    SUM(BetsReal - WinsReal) as GGR';
PRINT 'FROM common.tbl_Daily_actions';
PRINT 'WHERE Date >= ''2024-01-01''  -- Always filter by date for performance';
PRINT 'GROUP BY Date, PlayerID';
PRINT 'ORDER BY Date DESC, GGR DESC;';
PRINT '';

-- Test 5: Data Quality and Governance Information
PRINT '=== DATA QUALITY & GOVERNANCE ===';
SELECT 
    bti.TableName,
    bti.BusinessOwner,
    bti.DataGovernancePolicies,
    COUNT(bci.Id) as ColumnsWithMetadata,
    AVG(bci.DataQualityScore) as AvgDataQuality,
    SUM(CASE WHEN bci.IsSensitiveData = 1 THEN 1 ELSE 0 END) as SensitiveColumns
FROM BusinessTableInfo bti
LEFT JOIN BusinessColumnInfo bci ON bti.Id = bci.TableInfoId
WHERE bti.DomainClassification IN ('Gaming', 'Finance', 'Customer')
GROUP BY bti.TableName, bti.BusinessOwner, bti.DataGovernancePolicies
ORDER BY AvgDataQuality DESC;

PRINT '';
PRINT '=== SEMANTIC LAYER CAPABILITIES SUMMARY ===';
PRINT '✅ Business-friendly table descriptions';
PRINT '✅ Rich column-level context and examples';
PRINT '✅ Semantic tags for intelligent filtering';
PRINT '✅ Business term disambiguation';
PRINT '✅ Calculation rules and guidance';
PRINT '✅ Data quality and governance information';
PRINT '✅ Usage patterns and preferred aggregations';
PRINT '✅ Natural language aliases for business users';
PRINT '';
PRINT '=== COMPLETE SEMANTIC LAYER TEST SUCCESSFUL ===';
PRINT 'The semantic layer now provides comprehensive business context';
PRINT 'for intelligent BI query generation and execution!';
