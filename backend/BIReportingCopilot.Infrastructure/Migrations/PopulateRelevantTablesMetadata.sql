-- Targeted Business Metadata Population for Relevant Gaming Tables
-- Based on relevant_tables.md - Gaming/Casino domain specific tables

PRINT '=== POPULATING BUSINESS METADATA FOR RELEVANT GAMING TABLES ===';
PRINT 'Processing 8 core gaming/casino tables with rich business context';
PRINT '';

-- =====================================================
-- 1. tbl_Countries - Country Reference Data
-- =====================================================

PRINT 'Populating metadata for tbl_Countries...';

MERGE [dbo].[BusinessTableInfo] AS target
USING (VALUES (
    'common',                                              -- SchemaName
    'tbl_Countries',                                       -- TableName
    'Master reference table for countries and jurisdictions', -- BusinessPurpose
    'Contains country information including jurisdiction codes, phone codes, and regulatory compliance data for gaming operations', -- BusinessContext
    'Geographic filtering, compliance reporting, and player demographics analysis', -- PrimaryUseCase
    '["Player demographics by country", "Jurisdiction compliance reports", "Geographic revenue analysis"]', -- CommonQueryPatterns (JSON)
    'Always join with player data for geographic analysis. Filter by IsActive for current countries.', -- BusinessRules
    'Reference',                                           -- DomainClassification
    '["Countries", "Jurisdictions", "Geographic Data", "Country Master"]', -- NaturalLanguageAliases (JSON)
    '{"queryFrequency": 0.6, "commonJoins": ["tbl_Daily_actions_players"], "typicalFilters": ["IsActive", "JurisdictionCode"]}', -- UsagePatterns (JSON)
    '{"completenessScore": 0.98, "accuracyScore": 0.99}', -- DataQualityIndicators (JSON)
    '[{"relatedTable": "tbl_Daily_actions_players", "relationshipType": "OneToMany", "businessMeaning": "Players belong to countries"}]', -- RelationshipSemantics (JSON)
    0.7,                                                   -- ImportanceScore
    0.6,                                                   -- UsageFrequency
    'Compliance Team',                                     -- BusinessOwner
    '["Reference Data", "Compliance Required"]',           -- DataGovernancePolicies (JSON)
    'System'                                               -- CreatedBy
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

PRINT '✅ tbl_Countries metadata populated';

-- =====================================================
-- 2. tbl_Currencies - Currency Reference Data
-- =====================================================

PRINT 'Populating metadata for tbl_Currencies...';

MERGE [dbo].[BusinessTableInfo] AS target
USING (VALUES (
    'common',                                              -- SchemaName
    'tbl_Currencies',                                      -- TableName
    'Master reference table for supported currencies and exchange rates', -- BusinessPurpose
    'Contains currency information including exchange rates to major currencies (EUR, USD, GBP) for multi-currency gaming operations', -- BusinessContext
    'Currency conversion, financial reporting, and multi-currency player support', -- PrimaryUseCase
    '["Currency conversion reports", "Multi-currency revenue analysis", "Exchange rate tracking"]', -- CommonQueryPatterns (JSON)
    'Exchange rates should be current. Always validate currency codes against this table.', -- BusinessRules
    'Finance',                                             -- DomainClassification
    '["Currencies", "Exchange Rates", "Currency Master", "Financial Reference"]', -- NaturalLanguageAliases (JSON)
    '{"queryFrequency": 0.5, "commonJoins": ["tbl_Daily_actions_players"], "typicalFilters": ["CurrencyCode"]}', -- UsagePatterns (JSON)
    '{"completenessScore": 0.95, "accuracyScore": 0.98}', -- DataQualityIndicators (JSON)
    '[{"relatedTable": "tbl_Daily_actions_players", "relationshipType": "OneToMany", "businessMeaning": "Players have default currencies"}]', -- RelationshipSemantics (JSON)
    0.8,                                                   -- ImportanceScore
    0.5,                                                   -- UsageFrequency
    'Finance Team',                                        -- BusinessOwner
    '["Reference Data", "Financial Compliance"]',         -- DataGovernancePolicies (JSON)
    'System'                                               -- CreatedBy
)) AS source (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy)
ON target.SchemaName = source.SchemaName AND target.TableName = source.TableName
WHEN MATCHED THEN UPDATE SET [BusinessPurpose] = source.BusinessPurpose, [BusinessContext] = source.BusinessContext, [PrimaryUseCase] = source.PrimaryUseCase, [CommonQueryPatterns] = source.CommonQueryPatterns, [BusinessRules] = source.BusinessRules, [DomainClassification] = source.DomainClassification, [NaturalLanguageAliases] = source.NaturalLanguageAliases, [UsagePatterns] = source.UsagePatterns, [DataQualityIndicators] = source.DataQualityIndicators, [RelationshipSemantics] = source.RelationshipSemantics, [ImportanceScore] = source.ImportanceScore, [UsageFrequency] = source.UsageFrequency, [BusinessOwner] = source.BusinessOwner, [DataGovernancePolicies] = source.DataGovernancePolicies, [UpdatedDate] = GETUTCDATE(), [UpdatedBy] = source.CreatedBy
WHEN NOT MATCHED THEN INSERT (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy, CreatedDate) VALUES (source.SchemaName, source.TableName, source.BusinessPurpose, source.BusinessContext, source.PrimaryUseCase, source.CommonQueryPatterns, source.BusinessRules, source.DomainClassification, source.NaturalLanguageAliases, source.UsagePatterns, source.DataQualityIndicators, source.RelationshipSemantics, source.ImportanceScore, source.UsageFrequency, source.BusinessOwner, source.DataGovernancePolicies, source.CreatedBy, GETUTCDATE());

PRINT '✅ tbl_Currencies metadata populated';

-- =====================================================
-- 3. tbl_Daily_actions - CORE TABLE - Daily Player Statistics
-- =====================================================

PRINT 'Populating metadata for tbl_Daily_actions (CORE TABLE)...';

MERGE [dbo].[BusinessTableInfo] AS target
USING (VALUES (
    'common',                                              -- SchemaName
    'tbl_Daily_actions',                                   -- TableName
    'CORE TABLE: Comprehensive daily player statistics and financial metrics aggregated by player and date', -- BusinessPurpose
    'The most important table containing all daily financial and gaming activity per player. Includes deposits, withdrawals, bets, wins, bonuses across all gaming verticals (Casino, Sports, Live, Bingo)', -- BusinessContext
    'Daily revenue reporting, player activity analysis, financial KPI tracking, regulatory reporting', -- PrimaryUseCase
    '["Daily revenue reports", "Player lifetime value analysis", "GGR/NGR calculations", "Deposit/withdrawal tracking", "Bonus analysis", "Multi-vertical gaming analysis"]', -- CommonQueryPatterns (JSON)
    'CRITICAL: Always filter by Date for performance. Contains money columns for all gaming verticals. Use appropriate currency conversion. Data is aggregated daily per player.', -- BusinessRules
    'Gaming',                                              -- DomainClassification
    '["Daily Stats", "Player Statistics", "Daily Actions", "Gaming Metrics", "Revenue Data", "Player Activity"]', -- NaturalLanguageAliases (JSON)
    '{"queryFrequency": 0.95, "commonJoins": ["tbl_Daily_actions_players", "tbl_White_labels"], "typicalFilters": ["Date", "PlayerID", "WhiteLabelID"], "preferredAggregations": ["SUM", "AVG", "COUNT"]}', -- UsagePatterns (JSON)
    '{"completenessScore": 0.98, "accuracyScore": 0.99, "consistencyScore": 0.97}', -- DataQualityIndicators (JSON)
    '[{"relatedTable": "tbl_Daily_actions_players", "relationshipType": "ManyToOne", "businessMeaning": "Daily actions belong to players"}, {"relatedTable": "tbl_White_labels", "relationshipType": "ManyToOne", "businessMeaning": "Actions occur on specific brands"}]', -- RelationshipSemantics (JSON)
    1.0,                                                   -- ImportanceScore (HIGHEST)
    0.95,                                                  -- UsageFrequency (HIGHEST)
    'Business Intelligence Team',                          -- BusinessOwner
    '["Core Business Data", "Daily Reporting", "Financial Compliance", "Regulatory Reporting"]', -- DataGovernancePolicies (JSON)
    'System'                                               -- CreatedBy
)) AS source (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy)
ON target.SchemaName = source.SchemaName AND target.TableName = source.TableName
WHEN MATCHED THEN UPDATE SET [BusinessPurpose] = source.BusinessPurpose, [BusinessContext] = source.BusinessContext, [PrimaryUseCase] = source.PrimaryUseCase, [CommonQueryPatterns] = source.CommonQueryPatterns, [BusinessRules] = source.BusinessRules, [DomainClassification] = source.DomainClassification, [NaturalLanguageAliases] = source.NaturalLanguageAliases, [UsagePatterns] = source.UsagePatterns, [DataQualityIndicators] = source.DataQualityIndicators, [RelationshipSemantics] = source.RelationshipSemantics, [ImportanceScore] = source.ImportanceScore, [UsageFrequency] = source.UsageFrequency, [BusinessOwner] = source.BusinessOwner, [DataGovernancePolicies] = source.DataGovernancePolicies, [UpdatedDate] = GETUTCDATE(), [UpdatedBy] = source.CreatedBy
WHEN NOT MATCHED THEN INSERT (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy, CreatedDate) VALUES (source.SchemaName, source.TableName, source.BusinessPurpose, source.BusinessContext, source.PrimaryUseCase, source.CommonQueryPatterns, source.BusinessRules, source.DomainClassification, source.NaturalLanguageAliases, source.UsagePatterns, source.DataQualityIndicators, source.RelationshipSemantics, source.ImportanceScore, source.UsageFrequency, source.BusinessOwner, source.DataGovernancePolicies, source.CreatedBy, GETUTCDATE());

PRINT '✅ tbl_Daily_actions (CORE TABLE) metadata populated';

-- Continue with remaining tables...
PRINT '';
PRINT 'Continuing with remaining tables...';

-- =====================================================
-- 4. tbl_Daily_actions_games - Game-Specific Daily Statistics
-- =====================================================

PRINT 'Populating metadata for tbl_Daily_actions_games...';

MERGE [dbo].[BusinessTableInfo] AS target
USING (VALUES (
    'common', 'tbl_Daily_actions_games',
    'Daily gaming statistics aggregated by player, game, and date',
    'Detailed game-level performance data showing player activity per specific game including bet amounts, win amounts, and session counts',
    'Game performance analysis, player game preferences, game revenue tracking',
    '["Game performance reports", "Player game preferences", "Game-specific revenue analysis", "Popular games analysis"]',
    'Always join with Games table for game details. Filter by GameDate for performance.',
    'Gaming', '["Game Stats", "Daily Game Data", "Game Performance", "Player Game Activity"]',
    '{"queryFrequency": 0.7, "commonJoins": ["Games", "tbl_Daily_actions_players"], "typicalFilters": ["GameDate", "PlayerID", "GameID"]}',
    '{"completenessScore": 0.92, "accuracyScore": 0.95}',
    '[{"relatedTable": "Games", "relationshipType": "ManyToOne", "businessMeaning": "Game statistics belong to specific games"}]',
    0.8, 0.7, 'Product Team', '["Gaming Data", "Product Analytics"]', 'System'
)) AS source (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy)
ON target.SchemaName = source.SchemaName AND target.TableName = source.TableName
WHEN MATCHED THEN UPDATE SET [BusinessPurpose] = source.BusinessPurpose, [BusinessContext] = source.BusinessContext, [PrimaryUseCase] = source.PrimaryUseCase, [CommonQueryPatterns] = source.CommonQueryPatterns, [BusinessRules] = source.BusinessRules, [DomainClassification] = source.DomainClassification, [NaturalLanguageAliases] = source.NaturalLanguageAliases, [UsagePatterns] = source.UsagePatterns, [DataQualityIndicators] = source.DataQualityIndicators, [RelationshipSemantics] = source.RelationshipSemantics, [ImportanceScore] = source.ImportanceScore, [UsageFrequency] = source.UsageFrequency, [BusinessOwner] = source.BusinessOwner, [DataGovernancePolicies] = source.DataGovernancePolicies, [UpdatedDate] = GETUTCDATE(), [UpdatedBy] = source.CreatedBy
WHEN NOT MATCHED THEN INSERT (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy, CreatedDate) VALUES (source.SchemaName, source.TableName, source.BusinessPurpose, source.BusinessContext, source.PrimaryUseCase, source.CommonQueryPatterns, source.BusinessRules, source.DomainClassification, source.NaturalLanguageAliases, source.UsagePatterns, source.DataQualityIndicators, source.RelationshipSemantics, source.ImportanceScore, source.UsageFrequency, source.BusinessOwner, source.DataGovernancePolicies, source.CreatedBy, GETUTCDATE());

PRINT '✅ tbl_Daily_actions_games metadata populated';

-- =====================================================
-- 5. tbl_Daily_actions_players - Player Profile and Demographics
-- =====================================================

PRINT 'Populating metadata for tbl_Daily_actions_players...';

MERGE [dbo].[BusinessTableInfo] AS target
USING (VALUES (
    'common', 'tbl_Daily_actions_players',
    'Comprehensive player profile and demographic information',
    'Master player table containing detailed customer information including personal details, preferences, balances, VIP status, and regulatory compliance data. Critical for customer analytics and personalization.',
    'Customer analytics, player segmentation, VIP management, compliance reporting',
    '["Player demographics analysis", "VIP player reports", "Customer lifetime value", "Compliance and KYC reporting", "Player segmentation"]',
    'Contains PII data - handle with care. Always filter by IsActive players. Join with tbl_Daily_actions for activity analysis.',
    'Customer', '["Players", "Customers", "User Profiles", "Player Demographics", "Customer Data"]',
    '{"queryFrequency": 0.8, "commonJoins": ["tbl_Daily_actions", "tbl_Countries"], "typicalFilters": ["PlayerID", "IsActive", "Country", "VIPLevel"]}',
    '{"completenessScore": 0.85, "accuracyScore": 0.92, "piiDataPresent": true}',
    '[{"relatedTable": "tbl_Daily_actions", "relationshipType": "OneToMany", "businessMeaning": "Players have multiple daily action records"}, {"relatedTable": "tbl_Countries", "relationshipType": "ManyToOne", "businessMeaning": "Players belong to countries"}]',
    0.95, 0.8, 'Customer Team', '["Confidential", "GDPR Compliant", "PII Data", "Customer Privacy"]', 'System'
)) AS source (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy)
ON target.SchemaName = source.SchemaName AND target.TableName = source.TableName
WHEN MATCHED THEN UPDATE SET [BusinessPurpose] = source.BusinessPurpose, [BusinessContext] = source.BusinessContext, [PrimaryUseCase] = source.PrimaryUseCase, [CommonQueryPatterns] = source.CommonQueryPatterns, [BusinessRules] = source.BusinessRules, [DomainClassification] = source.DomainClassification, [NaturalLanguageAliases] = source.NaturalLanguageAliases, [UsagePatterns] = source.UsagePatterns, [DataQualityIndicators] = source.DataQualityIndicators, [RelationshipSemantics] = source.RelationshipSemantics, [ImportanceScore] = source.ImportanceScore, [UsageFrequency] = source.UsageFrequency, [BusinessOwner] = source.BusinessOwner, [DataGovernancePolicies] = source.DataGovernancePolicies, [UpdatedDate] = GETUTCDATE(), [UpdatedBy] = source.CreatedBy
WHEN NOT MATCHED THEN INSERT (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy, CreatedDate) VALUES (source.SchemaName, source.TableName, source.BusinessPurpose, source.BusinessContext, source.PrimaryUseCase, source.CommonQueryPatterns, source.BusinessRules, source.DomainClassification, source.NaturalLanguageAliases, source.UsagePatterns, source.DataQualityIndicators, source.RelationshipSemantics, source.ImportanceScore, source.UsageFrequency, source.BusinessOwner, source.DataGovernancePolicies, source.CreatedBy, GETUTCDATE());

PRINT '✅ tbl_Daily_actions_players metadata populated';

-- =====================================================
-- 6. tbl_Daily_actionsGBP_transactions - Transaction Records
-- =====================================================

PRINT 'Populating metadata for tbl_Daily_actionsGBP_transactions...';

MERGE [dbo].[BusinessTableInfo] AS target
USING (VALUES (
    'common', 'tbl_Daily_actionsGBP_transactions',
    'Detailed financial transaction records for all player monetary activities',
    'Comprehensive transaction log capturing all financial movements including deposits, withdrawals, bonuses, adjustments, and payment method details. Essential for financial reporting and audit trails.',
    'Financial audit trails, payment analysis, transaction monitoring, regulatory reporting',
    '["Transaction history reports", "Payment method analysis", "Financial audit trails", "Deposit/withdrawal tracking", "Fraud monitoring"]',
    'High-volume table - always use date filters. Contains sensitive financial data. TransactionType field categorizes transaction types.',
    'Finance', '["Transactions", "Financial Records", "Payment History", "Transaction Log", "Financial Movements"]',
    '{"queryFrequency": 0.7, "commonJoins": ["tbl_Daily_actions_players"], "typicalFilters": ["TransactionDate", "PlayerID", "TransactionType", "Status"]}',
    '{"completenessScore": 0.98, "accuracyScore": 0.99, "auditTrailComplete": true}',
    '[{"relatedTable": "tbl_Daily_actions_players", "relationshipType": "ManyToOne", "businessMeaning": "Transactions belong to players"}]',
    0.9, 0.7, 'Finance Team', '["Financial Data", "Audit Required", "Transaction Monitoring", "Regulatory Compliance"]', 'System'
)) AS source (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy)
ON target.SchemaName = source.SchemaName AND target.TableName = source.TableName
WHEN MATCHED THEN UPDATE SET [BusinessPurpose] = source.BusinessPurpose, [BusinessContext] = source.BusinessContext, [PrimaryUseCase] = source.PrimaryUseCase, [CommonQueryPatterns] = source.CommonQueryPatterns, [BusinessRules] = source.BusinessRules, [DomainClassification] = source.DomainClassification, [NaturalLanguageAliases] = source.NaturalLanguageAliases, [UsagePatterns] = source.UsagePatterns, [DataQualityIndicators] = source.DataQualityIndicators, [RelationshipSemantics] = source.RelationshipSemantics, [ImportanceScore] = source.ImportanceScore, [UsageFrequency] = source.UsageFrequency, [BusinessOwner] = source.BusinessOwner, [DataGovernancePolicies] = source.DataGovernancePolicies, [UpdatedDate] = GETUTCDATE(), [UpdatedBy] = source.CreatedBy
WHEN NOT MATCHED THEN INSERT (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy, CreatedDate) VALUES (source.SchemaName, source.TableName, source.BusinessPurpose, source.BusinessContext, source.PrimaryUseCase, source.CommonQueryPatterns, source.BusinessRules, source.DomainClassification, source.NaturalLanguageAliases, source.UsagePatterns, source.DataQualityIndicators, source.RelationshipSemantics, source.ImportanceScore, source.UsageFrequency, source.BusinessOwner, source.DataGovernancePolicies, source.CreatedBy, GETUTCDATE());

PRINT '✅ tbl_Daily_actionsGBP_transactions metadata populated';

-- =====================================================
-- 7. tbl_White_labels - Brand/Casino Configuration
-- =====================================================

PRINT 'Populating metadata for tbl_White_labels...';

MERGE [dbo].[BusinessTableInfo] AS target
USING (VALUES (
    'common', 'tbl_White_labels',
    'Brand and casino configuration data for white label operations',
    'Master configuration table for different casino brands operating on the platform. Contains brand-specific settings, URLs, welcome bonuses, restricted countries, and operational parameters.',
    'Brand management, multi-brand reporting, operational configuration, compliance management',
    '["Brand performance analysis", "Multi-brand revenue reports", "Operational configuration", "Brand-specific compliance"]',
    'Each WhiteLabelID represents a different casino brand. Use for brand-specific filtering and analysis.',
    'Business', '["Brands", "Casinos", "White Labels", "Brand Configuration", "Casino Brands"]',
    '{"queryFrequency": 0.5, "commonJoins": ["tbl_Daily_actions"], "typicalFilters": ["LabelID", "IsActive"]}',
    '{"completenessScore": 0.95, "accuracyScore": 0.98}',
    '[{"relatedTable": "tbl_Daily_actions", "relationshipType": "OneToMany", "businessMeaning": "Brands have multiple daily action records"}]',
    0.7, 0.5, 'Business Team', '["Brand Configuration", "Operational Data", "Multi-Brand Management"]', 'System'
)) AS source (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy)
ON target.SchemaName = source.SchemaName AND target.TableName = source.TableName
WHEN MATCHED THEN UPDATE SET [BusinessPurpose] = source.BusinessPurpose, [BusinessContext] = source.BusinessContext, [PrimaryUseCase] = source.PrimaryUseCase, [CommonQueryPatterns] = source.CommonQueryPatterns, [BusinessRules] = source.BusinessRules, [DomainClassification] = source.DomainClassification, [NaturalLanguageAliases] = source.NaturalLanguageAliases, [UsagePatterns] = source.UsagePatterns, [DataQualityIndicators] = source.DataQualityIndicators, [RelationshipSemantics] = source.RelationshipSemantics, [ImportanceScore] = source.ImportanceScore, [UsageFrequency] = source.UsageFrequency, [BusinessOwner] = source.BusinessOwner, [DataGovernancePolicies] = source.DataGovernancePolicies, [UpdatedDate] = GETUTCDATE(), [UpdatedBy] = source.CreatedBy
WHEN NOT MATCHED THEN INSERT (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy, CreatedDate) VALUES (source.SchemaName, source.TableName, source.BusinessPurpose, source.BusinessContext, source.PrimaryUseCase, source.CommonQueryPatterns, source.BusinessRules, source.DomainClassification, source.NaturalLanguageAliases, source.UsagePatterns, source.DataQualityIndicators, source.RelationshipSemantics, source.ImportanceScore, source.UsageFrequency, source.BusinessOwner, source.DataGovernancePolicies, source.CreatedBy, GETUTCDATE());

PRINT '✅ tbl_White_labels metadata populated';

-- =====================================================
-- 8. Games - Game Catalog and Configuration
-- =====================================================

PRINT 'Populating metadata for Games...';

MERGE [dbo].[BusinessTableInfo] AS target
USING (VALUES (
    'dbo', 'Games',
    'Master game catalog containing all available games and their configuration',
    'Comprehensive game library with detailed game information including providers, game types, RTP rates, volatility, compliance settings, and availability by jurisdiction.',
    'Game management, game performance analysis, compliance reporting, game catalog management',
    '["Game catalog reports", "Game performance analysis", "Provider analysis", "Game compliance reports", "Popular games tracking"]',
    'Master reference table for all games. Join with tbl_Daily_actions_games for performance data. Filter by IsActive for current games.',
    'Gaming', '["Games", "Game Catalog", "Game Library", "Gaming Products", "Game Configuration"]',
    '{"queryFrequency": 0.6, "commonJoins": ["tbl_Daily_actions_games"], "typicalFilters": ["GameID", "IsActive", "Provider", "GameType"]}',
    '{"completenessScore": 0.98, "accuracyScore": 0.99}',
    '[{"relatedTable": "tbl_Daily_actions_games", "relationshipType": "OneToMany", "businessMeaning": "Games have multiple daily statistics records"}]',
    0.75, 0.6, 'Product Team', '["Game Configuration", "Product Data", "Gaming Content"]', 'System'
)) AS source (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy)
ON target.SchemaName = source.SchemaName AND target.TableName = source.TableName
WHEN MATCHED THEN UPDATE SET [BusinessPurpose] = source.BusinessPurpose, [BusinessContext] = source.BusinessContext, [PrimaryUseCase] = source.PrimaryUseCase, [CommonQueryPatterns] = source.CommonQueryPatterns, [BusinessRules] = source.BusinessRules, [DomainClassification] = source.DomainClassification, [NaturalLanguageAliases] = source.NaturalLanguageAliases, [UsagePatterns] = source.UsagePatterns, [DataQualityIndicators] = source.DataQualityIndicators, [RelationshipSemantics] = source.RelationshipSemantics, [ImportanceScore] = source.ImportanceScore, [UsageFrequency] = source.UsageFrequency, [BusinessOwner] = source.BusinessOwner, [DataGovernancePolicies] = source.DataGovernancePolicies, [UpdatedDate] = GETUTCDATE(), [UpdatedBy] = source.CreatedBy
WHEN NOT MATCHED THEN INSERT (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy, CreatedDate) VALUES (source.SchemaName, source.TableName, source.BusinessPurpose, source.BusinessContext, source.PrimaryUseCase, source.CommonQueryPatterns, source.BusinessRules, source.DomainClassification, source.NaturalLanguageAliases, source.UsagePatterns, source.DataQualityIndicators, source.RelationshipSemantics, source.ImportanceScore, source.UsageFrequency, source.BusinessOwner, source.DataGovernancePolicies, source.CreatedBy, GETUTCDATE());

PRINT '✅ Games metadata populated';

-- =====================================================
-- Summary and Business Glossary Population
-- =====================================================

PRINT '';
PRINT '=== POPULATING GAMING BUSINESS GLOSSARY ===';

-- Key gaming terms specific to this domain
MERGE [dbo].[BusinessGlossary] AS target
USING (VALUES 
    ('GGR', 'Gross Gaming Revenue - total amount wagered minus winnings paid out', 'Key performance indicator for gaming operations representing gross profit before operational costs', 'Gaming', '["Gross Gaming Revenue", "Gaming Revenue", "Gross Win"]', '["NGR", "Revenue", "Win Amount"]', '["tbl_Daily_actions"]', '["calculated from bets minus wins"]', 0.98, 0.05, 'Finance Team'),
    ('NGR', 'Net Gaming Revenue - GGR minus bonuses and promotional costs', 'Net revenue after deducting promotional costs and bonuses', 'Gaming', '["Net Gaming Revenue", "Net Revenue"]', '["GGR", "Revenue", "Profit"]', '["tbl_Daily_actions"]', '["calculated from GGR minus bonuses"]', 0.98, 0.05, 'Finance Team'),
    ('FTD', 'First Time Deposit - indicates if this is a players first deposit', 'Critical metric for player acquisition and conversion tracking', 'Customer', '["First Time Deposit", "First Deposit", "Initial Deposit"]', '["Deposit", "Player Acquisition"]', '["tbl_Daily_actions", "tbl_Daily_actions_players"]', '["FTD", "FirstDepositDate"]', 0.95, 0.1, 'Marketing Team'),
    ('White Label', 'Brand or casino operating under the main platform', 'Different casino brands operating on the same gaming platform with unique branding', 'Business', '["Brand", "Casino Brand", "Label"]', '["Casino", "Platform"]', '["tbl_White_labels", "tbl_Daily_actions"]', '["WhiteLabelID", "LabelID"]', 0.9, 0.15, 'Business Team'),
    ('Player ID', 'Unique identifier for each player across all brands', 'Primary key for player identification across the entire gaming platform', 'Customer', '["Player", "Customer ID", "User ID"]', '["Customer", "Account"]', '["tbl_Daily_actions", "tbl_Daily_actions_players"]', '["PlayerID"]', 0.99, 0.02, 'Customer Team')
) AS source (Term, Definition, BusinessContext, Domain, Synonyms, RelatedTerms, MappedTables, MappedColumns, ConfidenceScore, AmbiguityScore, BusinessOwner)
ON target.Term = source.Term
WHEN MATCHED THEN UPDATE SET [Definition] = source.Definition, [BusinessContext] = source.BusinessContext, [Domain] = source.Domain, [Synonyms] = source.Synonyms, [RelatedTerms] = source.RelatedTerms, [MappedTables] = source.MappedTables, [MappedColumns] = source.MappedColumns, [ConfidenceScore] = source.ConfidenceScore, [AmbiguityScore] = source.AmbiguityScore, [BusinessOwner] = source.BusinessOwner, [UpdatedDate] = GETUTCDATE()
WHEN NOT MATCHED THEN INSERT (Term, Definition, BusinessContext, Domain, Synonyms, RelatedTerms, MappedTables, MappedColumns, ConfidenceScore, AmbiguityScore, BusinessOwner, CreatedBy, CreatedDate) VALUES (source.Term, source.Definition, source.BusinessContext, source.Domain, source.Synonyms, source.RelatedTerms, source.MappedTables, source.MappedColumns, source.ConfidenceScore, source.AmbiguityScore, source.BusinessOwner, 'System', GETUTCDATE());

PRINT '✅ Gaming business glossary populated';

PRINT '';
PRINT '=== RELEVANT TABLES METADATA POPULATION COMPLETE ===';
PRINT 'Successfully populated business metadata for ALL 8 core gaming tables:';
PRINT '1. tbl_Countries (Reference) - Countries and jurisdictions';
PRINT '2. tbl_Currencies (Finance) - Currency and exchange rates';
PRINT '3. tbl_Daily_actions (Gaming) - CORE TABLE - Daily player statistics';
PRINT '4. tbl_Daily_actions_games (Gaming) - Game-specific daily statistics';
PRINT '5. tbl_Daily_actions_players (Customer) - Player profiles and demographics';
PRINT '6. tbl_Daily_actionsGBP_transactions (Finance) - Transaction records';
PRINT '7. tbl_White_labels (Business) - Brand/casino configuration';
PRINT '8. Games (Gaming) - Game catalog and configuration';
PRINT '';
PRINT 'Business glossary updated with 5 gaming-specific terms';
PRINT 'Semantic layer is now fully operational for intelligent BI queries!';
PRINT '';
