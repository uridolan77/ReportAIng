-- Complete Column Metadata Population for ACTUAL Database Tables
-- Populates metadata for all columns in the real DailyActions and Transactions tables

PRINT '=== POPULATING COMPLETE COLUMN METADATA FOR ACTUAL TABLES ===';
PRINT 'Processing ALL columns in DailyActions (15 columns) and Transactions (10 columns)';
PRINT '';

-- First, update the table metadata to reflect the actual table names
PRINT 'Updating table metadata for actual table names...';

-- Update table metadata to match actual database structure
UPDATE BusinessTableInfo 
SET TableName = 'DailyActions', SchemaName = 'dbo'
WHERE SchemaName = 'common' AND TableName = 'tbl_Daily_actions';

-- Add Transactions table metadata
MERGE [dbo].[BusinessTableInfo] AS target
USING (VALUES (
    'dbo', 'Transactions',
    'Individual transaction records for all player activities',
    'Detailed transaction-level data capturing every financial movement including bets, wins, deposits, and withdrawals with full audit trail',
    'Transaction analysis, audit trails, detailed financial reporting, fraud detection',
    '["Transaction history", "Individual bet tracking", "Payment audit", "Detailed financial analysis"]',
    'High-volume table - always use date and player filters. Each row represents a single transaction.',
    'Finance', '["Transactions", "Transaction Log", "Financial Records", "Audit Trail"]',
    '{"queryFrequency": 0.8, "commonJoins": ["DailyActions"], "typicalFilters": ["TransactionDate", "PlayerId", "TransactionType"]}',
    '{"completenessScore": 0.99, "accuracyScore": 0.99}',
    '[{"relatedTable": "DailyActions", "relationshipType": "OneToMany", "businessMeaning": "Transactions aggregate to daily actions"}]',
    0.85, 0.8, 'Finance Team', '["Transaction Data", "Audit Required", "High Volume"]', 'System'
)) AS source (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy)
ON target.SchemaName = source.SchemaName AND target.TableName = source.TableName
WHEN NOT MATCHED THEN
    INSERT (SchemaName, TableName, BusinessPurpose, BusinessContext, PrimaryUseCase, CommonQueryPatterns, BusinessRules, DomainClassification, NaturalLanguageAliases, UsagePatterns, DataQualityIndicators, RelationshipSemantics, ImportanceScore, UsageFrequency, BusinessOwner, DataGovernancePolicies, CreatedBy, CreatedDate)
    VALUES (source.SchemaName, source.TableName, source.BusinessPurpose, source.BusinessContext, source.PrimaryUseCase, source.CommonQueryPatterns, source.BusinessRules, source.DomainClassification, source.NaturalLanguageAliases, source.UsagePatterns, source.DataQualityIndicators, source.RelationshipSemantics, source.ImportanceScore, source.UsageFrequency, source.BusinessOwner, source.DataGovernancePolicies, source.CreatedBy, GETUTCDATE());

PRINT '✅ Table metadata updated for actual database structure';

-- Clear existing column metadata for the updated table
DELETE FROM BusinessColumnInfo WHERE TableInfoId IN (
    SELECT Id FROM BusinessTableInfo WHERE TableName IN ('DailyActions', 'Transactions')
);

-- =====================================================
-- 1. DailyActions Table - ALL 15 Columns
-- =====================================================

PRINT 'Populating ALL column metadata for DailyActions (15 columns)...';

-- Get the TableInfoId for DailyActions
DECLARE @DailyActionsTableId BIGINT;
SELECT @DailyActionsTableId = Id FROM BusinessTableInfo WHERE SchemaName = 'dbo' AND TableName = 'DailyActions';

-- Insert ALL 15 columns for DailyActions
INSERT INTO BusinessColumnInfo (
    TableInfoId, ColumnName, BusinessMeaning, BusinessContext, DataExamples, ValidationRules, IsKeyColumn, 
    NaturalLanguageAliases, ValueExamples, DataLineage, CalculationRules, SemanticTags, BusinessDataType, 
    ConstraintsAndRules, DataQualityScore, UsageFrequency, PreferredAggregation, RelatedBusinessTerms, 
    IsSensitiveData, IsCalculatedField, CreatedBy, CreatedDate
) VALUES
-- Primary Key and Identifiers
(@DailyActionsTableId, 'Id', 'Unique record identifier', 'Primary key for each daily actions record', '1, 2, 3, 1000', 'Auto-incrementing, unique', 1, '["Record ID", "Primary Key"]', '["1", "2", "1000"]', 'Database auto-generated', 'IDENTITY(1,1)', '["primary_key", "identifier"]', 'Integer ID', 'NOT NULL, IDENTITY', 1.0, 0.3, 'COUNT', '["Primary Key"]', 0, 0, 'System', GETUTCDATE()),

(@DailyActionsTableId, 'WhiteLabelID', 'Brand/casino identifier', 'Identifies which casino brand the activity occurred on', '1, 2, 5, 10', 'Must be valid brand ID', 1, '["Brand ID", "Casino ID", "Label ID"]', '["1", "2", "5"]', 'Brand configuration', 'Assigned based on player brand', '["brand", "white_label", "identifier"]', 'Brand ID', 'NOT NULL', 0.95, 0.7, 'GROUP BY', '["White Label", "Brand"]', 0, 0, 'System', GETUTCDATE()),

(@DailyActionsTableId, 'Date', 'Date of daily activity summary', 'Critical time dimension for all daily aggregations', '2024-01-15, 2024-12-31', 'Must be valid date', 1, '["Activity Date", "Summary Date", "Report Date"]', '["2024-01-15", "2024-12-31"]', 'Transaction aggregation', 'Daily aggregation date', '["date", "time_dimension", "critical"]', 'Date', 'NOT NULL', 1.0, 0.95, 'GROUP BY', '["Date", "Time Period"]', 0, 0, 'System', GETUTCDATE()),

-- Player Lifecycle Metrics
(@DailyActionsTableId, 'Registration', 'New player registrations count', 'Number of new players who registered on this date for this brand', '0, 1, 5, 10', 'Must be >= 0', 0, '["New Registrations", "Player Acquisition", "Sign Ups"]', '["0", "1", "5"]', 'Player registration system', 'COUNT of new registrations', '["registration", "acquisition", "count"]', 'Count', '>= 0', 1.0, 0.4, 'SUM', '["Registration", "Acquisition"]', 0, 1, 'System', GETUTCDATE()),

(@DailyActionsTableId, 'FTD', 'First Time Deposits count', 'Number of players who made their first deposit on this date', '0, 1, 3, 8', 'Must be >= 0, <= Registration', 0, '["First Deposits", "FTD Count", "Conversions"]', '["0", "1", "3"]', 'Payment processing', 'COUNT of first deposits', '["ftd", "conversion", "count"]', 'Count', '>= 0', 1.0, 0.5, 'SUM', '["FTD", "First Deposit", "Conversion"]', 0, 1, 'System', GETUTCDATE()),

-- Financial Metrics
(@DailyActionsTableId, 'Deposits', 'Total deposit amount', 'Sum of all successful deposits for the date and brand', '0.00, 100.50, 5000.00', 'Must be >= 0', 0, '["Total Deposits", "Deposit Amount", "Money In"]', '["0.00", "100.50", "5000.00"]', 'Payment processing', 'SUM of deposit amounts', '["deposit", "revenue", "financial"]', 'Currency Amount', '>= 0', 0.98, 0.8, 'SUM', '["Deposits", "Revenue"]', 0, 1, 'System', GETUTCDATE()),

(@DailyActionsTableId, 'PaidCashouts', 'Total withdrawal amount', 'Sum of all successful withdrawals paid out', '0.00, 50.00, 2000.00', 'Must be >= 0', 0, '["Withdrawals", "Cashouts", "Money Out"]', '["0.00", "50.00", "2000.00"]', 'Payment processing', 'SUM of withdrawal amounts', '["withdrawal", "cashout", "financial"]', 'Currency Amount', '>= 0', 0.98, 0.7, 'SUM', '["Withdrawals", "Cashouts"]', 0, 1, 'System', GETUTCDATE()),

-- Casino Gaming Metrics
(@DailyActionsTableId, 'BetsCasino', 'Casino games bet amount', 'Total amount wagered on casino games (slots, table games)', '0.00, 250.00, 10000.00', 'Must be >= 0', 0, '["Casino Bets", "Casino Wagers", "Casino Stakes"]', '["0.00", "250.00", "10000.00"]', 'Casino gaming platform', 'SUM of casino bet amounts', '["casino", "bet", "gaming_vertical"]', 'Currency Amount', '>= 0', 0.99, 0.8, 'SUM', '["Casino Bets", "Gaming"]', 0, 1, 'System', GETUTCDATE()),

(@DailyActionsTableId, 'WinsCasino', 'Casino games win amount', 'Total amount won from casino games', '0.00, 180.00, 8000.00', 'Must be >= 0', 0, '["Casino Wins", "Casino Payouts", "Casino Winnings"]', '["0.00", "180.00", "8000.00"]', 'Casino gaming platform', 'SUM of casino win amounts', '["casino", "win", "gaming_vertical"]', 'Currency Amount', '>= 0', 0.99, 0.8, 'SUM', '["Casino Wins", "Gaming"]', 0, 1, 'System', GETUTCDATE()),

-- Sports Betting Metrics
(@DailyActionsTableId, 'BetsSport', 'Sports betting wager amount', 'Total amount wagered on sports events', '0.00, 150.00, 5000.00', 'Must be >= 0', 0, '["Sports Bets", "Sports Wagers", "Sportsbook Stakes"]', '["0.00", "150.00", "5000.00"]', 'Sportsbook platform', 'SUM of sports bet amounts', '["sports", "bet", "gaming_vertical"]', 'Currency Amount', '>= 0', 0.98, 0.7, 'SUM', '["Sports Bets", "Sportsbook"]', 0, 1, 'System', GETUTCDATE()),

(@DailyActionsTableId, 'WinsSport', 'Sports betting win amount', 'Total amount won from sports betting', '0.00, 120.00, 4000.00', 'Must be >= 0', 0, '["Sports Wins", "Sports Payouts", "Sportsbook Winnings"]', '["0.00", "120.00", "4000.00"]', 'Sportsbook platform', 'SUM of sports win amounts', '["sports", "win", "gaming_vertical"]', 'Currency Amount', '>= 0', 0.98, 0.7, 'SUM', '["Sports Wins", "Sportsbook"]', 0, 1, 'System', GETUTCDATE()),

-- Live Gaming Metrics
(@DailyActionsTableId, 'BetsLive', 'Live games bet amount', 'Total amount wagered on live dealer games', '0.00, 200.00, 3000.00', 'Must be >= 0', 0, '["Live Bets", "Live Gaming Wagers", "Live Dealer Stakes"]', '["0.00", "200.00", "3000.00"]', 'Live gaming platform', 'SUM of live game bet amounts', '["live", "bet", "gaming_vertical"]', 'Currency Amount', '>= 0', 0.97, 0.6, 'SUM', '["Live Bets", "Live Gaming"]', 0, 1, 'System', GETUTCDATE()),

(@DailyActionsTableId, 'WinsLive', 'Live games win amount', 'Total amount won from live dealer games', '0.00, 150.00, 2500.00', 'Must be >= 0', 0, '["Live Wins", "Live Gaming Payouts", "Live Dealer Winnings"]', '["0.00", "150.00", "2500.00"]', 'Live gaming platform', 'SUM of live game win amounts', '["live", "win", "gaming_vertical"]', 'Currency Amount', '>= 0', 0.97, 0.6, 'SUM', '["Live Wins", "Live Gaming"]', 0, 1, 'System', GETUTCDATE()),

-- Bingo Gaming Metrics
(@DailyActionsTableId, 'BetsBingo', 'Bingo games bet amount', 'Total amount wagered on bingo games', '0.00, 50.00, 500.00', 'Must be >= 0', 0, '["Bingo Bets", "Bingo Wagers", "Bingo Stakes"]', '["0.00", "50.00", "500.00"]', 'Bingo gaming platform', 'SUM of bingo bet amounts', '["bingo", "bet", "gaming_vertical"]', 'Currency Amount', '>= 0', 0.95, 0.4, 'SUM', '["Bingo Bets", "Bingo Gaming"]', 0, 1, 'System', GETUTCDATE()),

(@DailyActionsTableId, 'WinsBingo', 'Bingo games win amount', 'Total amount won from bingo games', '0.00, 40.00, 400.00', 'Must be >= 0', 0, '["Bingo Wins", "Bingo Payouts", "Bingo Winnings"]', '["0.00", "40.00", "400.00"]', 'Bingo gaming platform', 'SUM of bingo win amounts', '["bingo", "win", "gaming_vertical"]', 'Currency Amount', '>= 0', 0.95, 0.4, 'SUM', '["Bingo Wins", "Bingo Gaming"]', 0, 1, 'System', GETUTCDATE());

PRINT '✅ DailyActions - ALL 15 columns populated with business metadata';

-- =====================================================
-- 2. Transactions Table - ALL 10 Columns
-- =====================================================

PRINT 'Populating ALL column metadata for Transactions (10 columns)...';

-- Get the TableInfoId for Transactions
DECLARE @TransactionsTableId BIGINT;
SELECT @TransactionsTableId = Id FROM BusinessTableInfo WHERE SchemaName = 'dbo' AND TableName = 'Transactions';

-- Insert ALL 10 columns for Transactions
INSERT INTO BusinessColumnInfo (
    TableInfoId, ColumnName, BusinessMeaning, BusinessContext, DataExamples, ValidationRules, IsKeyColumn, 
    NaturalLanguageAliases, ValueExamples, DataLineage, CalculationRules, SemanticTags, BusinessDataType, 
    ConstraintsAndRules, DataQualityScore, UsageFrequency, PreferredAggregation, RelatedBusinessTerms, 
    IsSensitiveData, IsCalculatedField, CreatedBy, CreatedDate
) VALUES
-- Primary Keys and Identifiers
(@TransactionsTableId, 'Id', 'Database record identifier', 'Auto-incrementing primary key for each transaction record', '1, 2, 3, 1000000', 'Unique, auto-incrementing', 1, '["Record ID", "Primary Key"]', '["1", "2", "1000000"]', 'Database auto-generated', 'IDENTITY(1,1)', '["primary_key", "identifier"]', 'Integer ID', 'NOT NULL, IDENTITY', 1.0, 0.3, 'COUNT', '["Primary Key"]', 0, 0, 'System', GETUTCDATE()),

(@TransactionsTableId, 'TransactionId', 'Unique transaction identifier', 'Business transaction ID from source gaming platform', 'TXN_123456, BET_789012, DEP_345678', 'Must be unique business identifier', 1, '["Transaction ID", "Business ID", "External ID"]', '["TXN_123456", "BET_789012"]', 'Gaming platform', 'Generated by source system', '["transaction_id", "business_key", "external"]', 'Transaction ID', 'NOT NULL, UNIQUE', 1.0, 0.8, 'COUNT', '["Transaction ID"]', 0, 0, 'System', GETUTCDATE()),

(@TransactionsTableId, 'TransactionDate', 'Transaction timestamp', 'Exact date and time when the transaction occurred', '2024-01-15 14:30:25, 2024-12-31 23:59:59', 'Must be valid datetime', 1, '["Transaction Time", "Event Time", "Timestamp"]', '["2024-01-15 14:30:25"]', 'Gaming platform', 'Real-time transaction timestamp', '["datetime", "timestamp", "critical"]', 'DateTime', 'NOT NULL', 1.0, 0.9, 'GROUP BY', '["Transaction Date", "Time"]', 0, 0, 'System', GETUTCDATE()),

(@TransactionsTableId, 'PlayerId', 'Player identifier', 'Unique identifier for the player who made the transaction', 'PLR_12345, USR_67890, 999999', 'Must be valid player ID', 1, '["Player ID", "Customer ID", "User ID"]', '["PLR_12345", "USR_67890"]', 'Player management system', 'Player registration ID', '["player_id", "customer", "foreign_key"]', 'Player ID', 'NOT NULL', 1.0, 0.9, 'GROUP BY', '["Player ID", "Customer"]', 0, 0, 'System', GETUTCDATE()),

(@TransactionsTableId, 'WhitelabelId', 'Brand identifier', 'Identifies which casino brand the transaction occurred on', 'WL_1, WL_2, BRAND_5', 'Must be valid brand ID', 1, '["Brand ID", "Casino ID", "White Label ID"]', '["WL_1", "WL_2", "BRAND_5"]', 'Brand configuration', 'Brand assignment', '["brand", "white_label", "identifier"]', 'Brand ID', 'NOT NULL', 0.95, 0.7, 'GROUP BY', '["White Label", "Brand"]', 0, 0, 'System', GETUTCDATE()),

(@TransactionsTableId, 'GameId', 'Game identifier', 'Unique identifier for the specific game (if gaming transaction)', 'GAME_123, SLOT_456, NULL', 'Optional, valid game ID if gaming', 0, '["Game ID", "Product ID", "Game Code"]', '["GAME_123", "SLOT_456", "NULL"]', 'Game catalog', 'Game registration ID', '["game_id", "product", "optional"]', 'Game ID', 'NULLABLE', 0.8, 0.6, 'GROUP BY', '["Game ID", "Product"]', 0, 0, 'System', GETUTCDATE()),

(@TransactionsTableId, 'GameName', 'Game name', 'Human-readable name of the game (if gaming transaction)', 'Starburst, Blackjack, Football Betting', 'Optional, descriptive name', 0, '["Game Name", "Product Name", "Game Title"]', '["Starburst", "Blackjack"]', 'Game catalog', 'Game display name', '["game_name", "product", "optional"]', 'Game Name', 'NULLABLE', 0.8, 0.5, 'GROUP BY', '["Game Name", "Product"]', 0, 0, 'System', GETUTCDATE()),

(@TransactionsTableId, 'Amount', 'Transaction amount', 'Financial amount of the transaction (positive or negative)', '10.50, -25.00, 100.00, 0.00', 'Must be numeric, can be negative', 0, '["Transaction Amount", "Value", "Financial Amount"]', '["10.50", "-25.00", "100.00"]', 'Gaming platform', 'Transaction value', '["amount", "financial", "critical"]', 'Currency Amount', 'NOT NULL, NUMERIC', 1.0, 0.9, 'SUM', '["Amount", "Value", "Financial"]', 0, 0, 'System', GETUTCDATE()),

(@TransactionsTableId, 'TransactionType', 'Type of transaction', 'Category of transaction (bet, win, deposit, withdrawal, etc.)', 'BET, WIN, DEPOSIT, WITHDRAWAL, BONUS', 'Must be valid transaction type', 0, '["Transaction Type", "Transaction Category", "Activity Type"]', '["BET", "WIN", "DEPOSIT"]', 'Gaming platform', 'Transaction classification', '["transaction_type", "category", "classification"]', 'Transaction Type', 'NOT NULL', 1.0, 0.8, 'GROUP BY', '["Transaction Type", "Category"]', 0, 0, 'System', GETUTCDATE()),

(@TransactionsTableId, 'Currency', 'Transaction currency', 'Currency code for the transaction amount', 'GBP, EUR, USD, CAD', 'Must be valid ISO currency code', 0, '["Currency Code", "Currency", "Denomination"]', '["GBP", "EUR", "USD"]', 'Player account settings', 'Player account currency', '["currency", "financial", "iso_code"]', 'Currency Code', 'NOT NULL', 0.98, 0.7, 'GROUP BY', '["Currency"]', 0, 0, 'System', GETUTCDATE());

PRINT '✅ Transactions - ALL 10 columns populated with business metadata';

PRINT '';
PRINT '=== COMPLETE COLUMN METADATA POPULATION FINISHED ===';
PRINT 'Successfully populated business metadata for ALL columns:';
PRINT '• DailyActions: 15 columns with complete business context';
PRINT '• Transactions: 10 columns with complete business context';
PRINT 'Total: 25 columns with rich semantic metadata';
PRINT 'Each column includes: meaning, context, examples, validation, semantic tags, business terms';
PRINT '';
