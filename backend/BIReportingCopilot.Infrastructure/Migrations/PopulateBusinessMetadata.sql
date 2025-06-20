-- Business Metadata Population Script
-- This script provides multiple approaches to populate business metadata for your specific tables

PRINT '=== BUSINESS METADATA POPULATION GUIDE ===';
PRINT 'Choose the approach that best fits your needs:';
PRINT '1. Manual SQL Population (immediate, precise)';
PRINT '2. CSV Import Template (bulk, structured)';
PRINT '3. Automated Service (intelligent, scalable)';
PRINT '';

-- =====================================================
-- APPROACH 1: MANUAL SQL POPULATION
-- =====================================================

PRINT '=== APPROACH 1: MANUAL SQL POPULATION ===';
PRINT 'Direct SQL statements for immediate population';
PRINT '';

-- Example: Populate metadata for a gaming table
-- Replace with your actual table names and business context

-- Sample for tbl_Daily_actions (if it exists)
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'tbl_Daily_actions')
BEGIN
    PRINT 'Populating metadata for tbl_Daily_actions...';
    
    -- Insert or update table metadata
    MERGE [dbo].[BusinessTableInfo] AS target
    USING (VALUES (
        'common',                                           -- SchemaName
        'tbl_Daily_actions',                               -- TableName
        'Main statistics table holding all player statistics aggregated by player by day', -- BusinessPurpose
        'Core table for daily reporting and player activity analysis. Contains comprehensive daily financial and gaming metrics per player.', -- BusinessContext
        'Daily player activity reporting and financial analysis', -- PrimaryUseCase
        '["Daily revenue reports", "Player activity analysis", "Financial summaries"]', -- CommonQueryPatterns (JSON)
        'Data is aggregated daily. Always filter by date ranges for performance.', -- BusinessRules
        'Gaming',                                          -- DomainClassification
        '["Daily Stats", "Player Statistics", "Daily Actions", "Gaming Metrics"]', -- NaturalLanguageAliases (JSON)
        '{"queryFrequency": 0.9, "commonJoins": ["tbl_users"], "typicalFilters": ["date_range", "player_id"]}', -- UsagePatterns (JSON)
        '{"completenessScore": 0.95, "accuracyScore": 0.98}', -- DataQualityIndicators (JSON)
        '[]',                                              -- RelationshipSemantics (JSON)
        0.9,                                               -- ImportanceScore
        0.8,                                               -- UsageFrequency
        'Business Team',                                   -- BusinessOwner
        '["Internal", "Daily Reporting"]',                 -- DataGovernancePolicies (JSON)
        'System'                                           -- CreatedBy
    )) AS source (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy)
    ON target.SchemaName = source.SchemaName AND target.TableName = source.TableName
    WHEN MATCHED THEN
        UPDATE SET 
            BusinessPurpose = source.BusinessPurpose,
            BusinessContext = source.BusinessContext,
            PrimaryUseCase = source.PrimaryUseCase,
            CommonQueryPatterns = source.CommonQueryPatterns,
            BusinessRules = source.BusinessRules,
            DomainClassification = source.DomainClassification,
            NaturalLanguageAliases = source.NaturalLanguageAliases,
            UsagePatterns = source.UsagePatterns,
            DataQualityIndicators = source.DataQualityIndicators,
            RelationshipSemantics = source.RelationshipSemantics,
            ImportanceScore = source.ImportanceScore,
            UsageFrequency = source.UsageFrequency,
            BusinessOwner = source.BusinessOwner,
            DataGovernancePolicies = source.DataGovernancePolicies,
            UpdatedDate = GETUTCDATE(),
            UpdatedBy = source.CreatedBy
    WHEN NOT MATCHED THEN
        INSERT (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy, CreatedDate)
        VALUES (source.SchemaName, source.TableName, source.BusinessPurpose, source.BusinessContext, source.PrimaryUseCase, source.CommonQueryPatterns, source.BusinessRules, source.DomainClassification, source.NaturalLanguageAliases, source.UsagePatterns, source.DataQualityIndicators, source.RelationshipSemantics, source.ImportanceScore, source.UsageFrequency, source.BusinessOwner, source.DataGovernancePolicies, source.CreatedBy, GETUTCDATE());
    
    PRINT '✅ tbl_Daily_actions metadata populated';
END

-- Template for additional tables - copy and modify as needed
/*
MERGE [dbo].[BusinessTableInfo] AS target
USING (VALUES (
    'your_schema',                                         -- SchemaName
    'your_table_name',                                     -- TableName
    'Your business purpose description',                   -- BusinessPurpose
    'Your business context explanation',                   -- BusinessContext
    'Primary use case description',                        -- PrimaryUseCase
    '["Query pattern 1", "Query pattern 2"]',            -- CommonQueryPatterns (JSON)
    'Your business rules',                                 -- BusinessRules
    'Your_Domain',                                         -- DomainClassification (Finance/Customer/Gaming/Analytics)
    '["Alias 1", "Alias 2"]',                            -- NaturalLanguageAliases (JSON)
    '{"queryFrequency": 0.5}',                           -- UsagePatterns (JSON)
    '{"completenessScore": 0.8}',                        -- DataQualityIndicators (JSON)
    '[]',                                                  -- RelationshipSemantics (JSON)
    0.5,                                                   -- ImportanceScore (0.0-1.0)
    0.3,                                                   -- UsageFrequency (0.0-1.0)
    'Your Team',                                           -- BusinessOwner
    '["Policy 1"]',                                        -- DataGovernancePolicies (JSON)
    'System'                                               -- CreatedBy
)) AS source (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy)
ON target.SchemaName = source.SchemaName AND target.TableName = source.TableName
WHEN MATCHED THEN UPDATE SET [... same as above ...]
WHEN NOT MATCHED THEN INSERT [... same as above ...];
*/

-- =====================================================
-- APPROACH 2: BUSINESS GLOSSARY POPULATION
-- =====================================================

PRINT '';
PRINT '=== BUSINESS GLOSSARY POPULATION ===';
PRINT 'Populating common business terms for gaming/casino domain...';

-- Gaming domain business terms
MERGE [dbo].[BusinessGlossary] AS target
USING (VALUES 
    ('Revenue', 'Total income generated from gaming activities', 'Financial metric representing gross income before costs', 'Finance', '["Income", "Gross Revenue", "Turnover"]', '["Profit", "GGR", "NGR"]', '["tbl_Daily_actions"]', '["revenue_amount", "total_revenue"]', 0.95, 0.1),
    ('GGR', 'Gross Gaming Revenue - total amount wagered minus winnings paid out', 'Key performance indicator for gaming operations', 'Gaming', '["Gross Gaming Revenue", "Gaming Revenue"]', '["Revenue", "NGR", "Win Amount"]', '["tbl_Daily_actions"]', '["ggr_amount", "gross_gaming_revenue"]', 0.98, 0.05),
    ('NGR', 'Net Gaming Revenue - GGR minus bonuses and promotions', 'Net revenue after promotional costs', 'Gaming', '["Net Gaming Revenue", "Net Revenue"]', '["GGR", "Revenue", "Profit"]', '["tbl_Daily_actions"]', '["ngr_amount", "net_gaming_revenue"]', 0.98, 0.05),
    ('Player', 'Individual user who participates in gaming activities', 'Core entity representing customers/users', 'Customer', '["User", "Customer", "Gambler"]', '["Account", "Member"]', '["tbl_users", "tbl_Daily_actions"]', '["player_id", "user_id", "customer_id"]', 0.99, 0.02),
    ('Bet', 'Amount wagered by a player on a game', 'Financial transaction representing player stake', 'Gaming', '["Wager", "Stake", "Bet Amount"]', '["Win", "Payout"]', '["tbl_Daily_actions"]', '["bet_amount", "total_bets", "wager_amount"]', 0.95, 0.1),
    ('Win', 'Amount paid out to player for successful bets', 'Financial transaction representing player winnings', 'Gaming', '["Payout", "Winnings", "Prize"]', '["Bet", "Loss"]', '["tbl_Daily_actions"]', '["win_amount", "total_wins", "payout_amount"]', 0.95, 0.1),
    ('Daily Actions', 'Aggregated player activity data for a single day', 'Summary of all player activities within a 24-hour period', 'Analytics', '["Daily Stats", "Daily Summary", "Daily Metrics"]', '["Player Activity", "Gaming Statistics"]', '["tbl_Daily_actions"]', '["action_date", "daily_*"]', 0.9, 0.15)
) AS source (Term, Definition, BusinessContext, Domain, Synonyms, RelatedTerms, MappedTables, MappedColumns, ConfidenceScore, AmbiguityScore)
ON target.Term = source.Term
WHEN MATCHED THEN
    UPDATE SET 
        Definition = source.Definition,
        BusinessContext = source.BusinessContext,
        Domain = source.Domain,
        Synonyms = source.Synonyms,
        RelatedTerms = source.RelatedTerms,
        MappedTables = source.MappedTables,
        MappedColumns = source.MappedColumns,
        ConfidenceScore = source.ConfidenceScore,
        AmbiguityScore = source.AmbiguityScore,
        UpdatedDate = GETUTCDATE()
WHEN NOT MATCHED THEN
    INSERT (Term, Definition, BusinessContext, Domain, Synonyms, RelatedTerms, MappedTables, MappedColumns, ConfidenceScore, AmbiguityScore, CreatedBy, CreatedDate)
    VALUES (source.Term, source.Definition, source.BusinessContext, source.Domain, source.Synonyms, source.RelatedTerms, source.MappedTables, source.MappedColumns, source.ConfidenceScore, source.AmbiguityScore, 'System', GETUTCDATE());

PRINT '✅ Business glossary populated with gaming terms';

-- =====================================================
-- APPROACH 3: DISCOVERY QUERY
-- =====================================================

PRINT '';
PRINT '=== TABLE DISCOVERY FOR METADATA POPULATION ===';
PRINT 'Run this query to see what tables need metadata:';
PRINT '';

-- Query to identify tables that need business metadata
SELECT 
    t.TABLE_SCHEMA as SchemaName,
    t.TABLE_NAME as TableName,
    (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = t.TABLE_SCHEMA AND TABLE_NAME = t.TABLE_NAME) as ColumnCount,
    CASE 
        WHEN bti.Id IS NOT NULL THEN 'Has Metadata'
        ELSE 'Needs Metadata'
    END as MetadataStatus,
    CASE 
        WHEN t.TABLE_NAME LIKE '%user%' OR t.TABLE_NAME LIKE '%customer%' OR t.TABLE_NAME LIKE '%player%' THEN 'Customer'
        WHEN t.TABLE_NAME LIKE '%transaction%' OR t.TABLE_NAME LIKE '%payment%' OR t.TABLE_NAME LIKE '%financial%' THEN 'Finance'
        WHEN t.TABLE_NAME LIKE '%game%' OR t.TABLE_NAME LIKE '%bet%' OR t.TABLE_NAME LIKE '%activity%' THEN 'Gaming'
        WHEN t.TABLE_NAME LIKE '%daily%' OR t.TABLE_NAME LIKE '%summary%' OR t.TABLE_NAME LIKE '%report%' THEN 'Analytics'
        ELSE 'General'
    END as SuggestedDomain
FROM INFORMATION_SCHEMA.TABLES t
LEFT JOIN [dbo].[BusinessTableInfo] bti ON t.TABLE_SCHEMA = bti.SchemaName AND t.TABLE_NAME = bti.TableName
WHERE t.TABLE_TYPE = 'BASE TABLE'
AND t.TABLE_SCHEMA NOT IN ('sys', 'information_schema')
ORDER BY 
    CASE WHEN bti.Id IS NULL THEN 0 ELSE 1 END,  -- Show tables without metadata first
    t.TABLE_SCHEMA, 
    t.TABLE_NAME;

PRINT '';
PRINT '=== NEXT STEPS ===';
PRINT '1. Review the discovery query results above';
PRINT '2. Use the manual SQL templates to populate high-priority tables';
PRINT '3. Consider using the automated BusinessMetadataPopulationService for bulk processing';
PRINT '4. Populate business glossary with domain-specific terms';
PRINT '5. Test the semantic layer with populated metadata';
PRINT '';
PRINT 'Business metadata population guide complete.';
