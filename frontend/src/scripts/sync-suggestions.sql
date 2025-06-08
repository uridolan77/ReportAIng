-- =====================================================
-- Sync Improved Query Suggestions to Database
-- This script replaces existing suggestions with the improved ones from the frontend
-- =====================================================

USE [BIReportingCopilot_Dev];
GO

-- Start transaction for safety
BEGIN TRANSACTION;

PRINT 'Starting suggestion database sync...';

-- =====================================================
-- Step 1: Clean up existing data
-- =====================================================

PRINT 'Cleaning up existing suggestions and categories...';

-- Delete existing suggestions (this will cascade due to foreign keys)
DELETE FROM [dbo].[QuerySuggestions];
PRINT 'Deleted existing suggestions';

-- Delete existing categories
DELETE FROM [dbo].[SuggestionCategories];
PRINT 'Deleted existing categories';

-- Reset identity seeds
DBCC CHECKIDENT ('[dbo].[SuggestionCategories]', RESEED, 0);
DBCC CHECKIDENT ('[dbo].[QuerySuggestions]', RESEED, 0);

-- =====================================================
-- Step 2: Create new categories
-- =====================================================

PRINT 'Creating new suggestion categories...';

INSERT INTO [dbo].[SuggestionCategories] (
    [CategoryKey], [Title], [Icon], [Description], [SortOrder], [IsActive], [CreatedBy], [CreatedDate]
) VALUES
    ('financial', 'Financial & Revenue', 'Dollar', 'Revenue, deposits, withdrawals, and financial performance metrics', 1, 1, 'system', GETDATE()),
    ('players', 'Player Analytics', 'Target', 'Player behavior, demographics, and lifecycle analysis', 2, 1, 'system', GETDATE()),
    ('gaming', 'Gaming & Products', 'Game', 'Game performance, provider analysis, and product metrics', 3, 1, 'system', GETDATE()),
    ('transactions', 'Transactions & Payments', 'Credit', 'Payment methods, transaction analysis, and processing metrics', 4, 1, 'system', GETDATE()),
    ('demographics', 'Demographics & Behavior', 'Global', 'Player demographics, behavior patterns, and segmentation analysis', 5, 1, 'system', GETDATE()),
    ('account', 'Account & Status', 'Lock', 'Account management, player status, and lifecycle tracking', 6, 1, 'system', GETDATE()),
    ('bonus', 'Bonus & Promotions', 'Gift', 'Bonus campaigns, promotional effectiveness, and reward analysis', 7, 1, 'system', GETDATE()),
    ('compliance', 'Compliance & Risk', 'Shield', 'Risk management, compliance monitoring, and regulatory reporting', 8, 1, 'system', GETDATE()),
    ('business', 'Business Intelligence', 'Chart', 'Strategic insights, KPI tracking, and business performance analysis', 9, 1, 'system', GETDATE()),
    ('operations', 'Operations & Trends', 'Setting', 'Operational metrics, trends, and performance monitoring', 10, 1, 'system', GETDATE());

PRINT 'Created 10 new categories';

-- =====================================================
-- Step 3: Create new suggestions
-- =====================================================

PRINT 'Creating new suggestions...';

-- Get category IDs for reference
DECLARE @FinancialId INT = (SELECT Id FROM [dbo].[SuggestionCategories] WHERE CategoryKey = 'financial');
DECLARE @PlayersId INT = (SELECT Id FROM [dbo].[SuggestionCategories] WHERE CategoryKey = 'players');
DECLARE @GamingId INT = (SELECT Id FROM [dbo].[SuggestionCategories] WHERE CategoryKey = 'gaming');
DECLARE @TransactionsId INT = (SELECT Id FROM [dbo].[SuggestionCategories] WHERE CategoryKey = 'transactions');
DECLARE @DemographicsId INT = (SELECT Id FROM [dbo].[SuggestionCategories] WHERE CategoryKey = 'demographics');
DECLARE @AccountId INT = (SELECT Id FROM [dbo].[SuggestionCategories] WHERE CategoryKey = 'account');
DECLARE @BonusId INT = (SELECT Id FROM [dbo].[SuggestionCategories] WHERE CategoryKey = 'bonus');
DECLARE @ComplianceId INT = (SELECT Id FROM [dbo].[SuggestionCategories] WHERE CategoryKey = 'compliance');
DECLARE @BusinessId INT = (SELECT Id FROM [dbo].[SuggestionCategories] WHERE CategoryKey = 'business');
DECLARE @OperationsId INT = (SELECT Id FROM [dbo].[SuggestionCategories] WHERE CategoryKey = 'operations');

-- Verify categories were created
IF @FinancialId IS NULL OR @PlayersId IS NULL OR @GamingId IS NULL OR @TransactionsId IS NULL
   OR @DemographicsId IS NULL OR @AccountId IS NULL OR @BonusId IS NULL OR @ComplianceId IS NULL
   OR @BusinessId IS NULL OR @OperationsId IS NULL
BEGIN
    PRINT 'ERROR: Failed to create categories properly';
    ROLLBACK TRANSACTION;
    RETURN;
END

PRINT 'Category IDs: Financial=' + CAST(@FinancialId AS VARCHAR(10)) +
      ', Players=' + CAST(@PlayersId AS VARCHAR(10)) +
      ', Gaming=' + CAST(@GamingId AS VARCHAR(10)) +
      ', Transactions=' + CAST(@TransactionsId AS VARCHAR(10)) +
      ', Demographics=' + CAST(@DemographicsId AS VARCHAR(10)) +
      ', Account=' + CAST(@AccountId AS VARCHAR(10)) +
      ', Bonus=' + CAST(@BonusId AS VARCHAR(10)) +
      ', Compliance=' + CAST(@ComplianceId AS VARCHAR(10)) +
      ', Business=' + CAST(@BusinessId AS VARCHAR(10)) +
      ', Operations=' + CAST(@OperationsId AS VARCHAR(10));

-- Financial & Revenue Suggestions (3 suggestions)
INSERT INTO [dbo].[QuerySuggestions] (
    [CategoryId], [QueryText], [Description], [DefaultTimeFrame], [SortOrder], [IsActive],
    [TargetTables], [Complexity], [RequiredPermissions], [Tags], [CreatedDate], [CreatedBy]
) VALUES
    (@FinancialId, 'Show me total deposits for yesterday', 'Daily deposit performance tracking', 'yesterday', 1, 1,
     'tbl_Daily_actions', 1, 'query:execute', 'deposits,daily,financial', GETDATE(), 'system'),

    (@FinancialId, 'Revenue breakdown by country for last week', 'Geographic revenue analysis and performance', 'last_7_days', 2, 1,
     'tbl_Daily_actions,tbl_Daily_actions_players,tbl_Countries', 2, 'query:execute', 'revenue,geography,countries', GETDATE(), 'system'),

    (@FinancialId, 'Net gaming revenue trends last 30 days', 'Revenue trend analysis and performance tracking', 'last_30_days', 3, 1,
     'tbl_Daily_actions', 2, 'query:execute', 'ngr,trends,performance', GETDATE(), 'system');

-- Player Analytics Suggestions (3 suggestions)
INSERT INTO [dbo].[QuerySuggestions] (
    [CategoryId], [QueryText], [Description], [DefaultTimeFrame], [SortOrder], [IsActive],
    [TargetTables], [Complexity], [RequiredPermissions], [Tags], [CreatedDate], [CreatedBy]
) VALUES
    (@PlayersId, 'Top 10 players by deposits in the last 7 days', 'High-value player identification and analysis', 'last_7_days', 1, 1,
     'tbl_Daily_actions,tbl_Daily_actions_players', 2, 'query:execute', 'high_value,players,deposits,top_performers', GETDATE(), 'system'),

    (@PlayersId, 'New player registrations by country this week', 'Player acquisition analysis by geography', 'this_week', 2, 1,
     'tbl_Daily_actions,tbl_Daily_actions_players,tbl_Countries', 2, 'query:execute', 'registrations,acquisition,geography', GETDATE(), 'system'),

    (@PlayersId, 'Active players by white label brand yesterday', 'Brand performance comparison and analysis', 'yesterday', 3, 1,
     'tbl_Daily_actions,tbl_White_labels', 2, 'query:execute', 'active_players,brands,performance', GETDATE(), 'system');

-- Gaming & Products Suggestions (3 suggestions)
INSERT INTO [dbo].[QuerySuggestions] (
    [CategoryId], [QueryText], [Description], [DefaultTimeFrame], [SortOrder], [IsActive],
    [TargetTables], [Complexity], [RequiredPermissions], [Tags], [CreatedDate], [CreatedBy]
) VALUES
    (@GamingId, 'Top performing games by net gaming revenue this month', 'Game performance ranking and analysis', 'this_month', 1, 1,
     'tbl_Daily_actions_games,Games', 2, 'query:execute', 'games,performance,ngr', GETDATE(), 'system'),

    (@GamingId, 'Slot vs table games revenue comparison this week', 'Game type performance analysis', 'this_week', 2, 1,
     'tbl_Daily_actions_games,Games', 2, 'query:execute', 'game_types,slots,tables,comparison', GETDATE(), 'system'),

    (@GamingId, 'Casino vs sports betting revenue split this month', 'Product vertical comparison and analysis', 'this_month', 3, 1,
     'tbl_Daily_actions', 2, 'query:execute', 'casino,sports,verticals,comparison', GETDATE(), 'system');

-- Transactions & Payments Suggestions (3 suggestions)
INSERT INTO [dbo].[QuerySuggestions] (
    [CategoryId], [QueryText], [Description], [DefaultTimeFrame], [SortOrder], [IsActive],
    [TargetTables], [Complexity], [RequiredPermissions], [Tags], [CreatedDate], [CreatedBy]
) VALUES
    (@TransactionsId, 'Transaction volumes by payment method today', 'Payment method usage analysis', 'today', 1, 1,
     'tbl_Daily_actions_transactions', 2, 'query:execute', 'transactions,payment_methods,volumes', GETDATE(), 'system'),

    (@TransactionsId, 'Failed vs successful transactions by payment type this week', 'Payment success rate analysis', 'this_week', 2, 1,
     'tbl_Daily_actions_transactions', 2, 'query:execute', 'transaction_status,success_rates,payment_types', GETDATE(), 'system'),

    (@TransactionsId, 'Average transaction amount by payment method this month', 'Payment method value insights', 'this_month', 3, 1,
     'tbl_Daily_actions_transactions', 2, 'query:execute', 'transaction_amounts,payment_methods,averages', GETDATE(), 'system');

-- Demographics & Behavior Suggestions (2 suggestions)
INSERT INTO [dbo].[QuerySuggestions] (
    [CategoryId], [QueryText], [Description], [DefaultTimeFrame], [SortOrder], [IsActive],
    [TargetTables], [Complexity], [RequiredPermissions], [Tags], [CreatedDate], [CreatedBy]
) VALUES
    (@DemographicsId, 'Player age distribution by country this month', 'Demographic analysis by geography', 'this_month', 1, 1,
     'tbl_Daily_actions_players,tbl_Countries', 2, 'query:execute', 'demographics,age,geography', GETDATE(), 'system'),

    (@DemographicsId, 'Player session duration patterns this week', 'Behavioral pattern analysis', 'this_week', 2, 1,
     'tbl_Daily_actions_players', 2, 'query:execute', 'behavior,sessions,patterns', GETDATE(), 'system');

-- Account & Status Suggestions (2 suggestions)
INSERT INTO [dbo].[QuerySuggestions] (
    [CategoryId], [QueryText], [Description], [DefaultTimeFrame], [SortOrder], [IsActive],
    [TargetTables], [Complexity], [RequiredPermissions], [Tags], [CreatedDate], [CreatedBy]
) VALUES
    (@AccountId, 'New player conversion rates by registration channel', 'Account lifecycle and conversion analysis', 'this_month', 1, 1,
     'tbl_Daily_actions_players', 2, 'query:execute', 'conversion,registration,lifecycle', GETDATE(), 'system'),

    (@AccountId, 'Player verification status breakdown today', 'Account verification and compliance tracking', 'today', 2, 1,
     'tbl_Daily_actions_players', 1, 'query:execute', 'verification,compliance,status', GETDATE(), 'system');

-- Bonus & Promotions Suggestions (2 suggestions)
INSERT INTO [dbo].[QuerySuggestions] (
    [CategoryId], [QueryText], [Description], [DefaultTimeFrame], [SortOrder], [IsActive],
    [TargetTables], [Complexity], [RequiredPermissions], [Tags], [CreatedDate], [CreatedBy]
) VALUES
    (@BonusId, 'Bonus utilization rates by promotion type this week', 'Promotional campaign effectiveness analysis', 'this_week', 1, 1,
     'tbl_Daily_actions_bonuses', 2, 'query:execute', 'bonuses,promotions,utilization', GETDATE(), 'system'),

    (@BonusId, 'Top performing bonus campaigns by revenue impact', 'Bonus campaign ROI and performance tracking', 'this_month', 2, 1,
     'tbl_Daily_actions_bonuses,tbl_Daily_actions', 3, 'query:execute', 'campaigns,roi,performance', GETDATE(), 'system');

-- Compliance & Risk Suggestions (2 suggestions)
INSERT INTO [dbo].[QuerySuggestions] (
    [CategoryId], [QueryText], [Description], [DefaultTimeFrame], [SortOrder], [IsActive],
    [TargetTables], [Complexity], [RequiredPermissions], [Tags], [CreatedDate], [CreatedBy]
) VALUES
    (@ComplianceId, 'High-risk transaction patterns this week', 'Risk assessment and fraud detection analysis', 'this_week', 1, 1,
     'tbl_Daily_actions_transactions', 3, 'risk_analysis', 'risk,fraud,compliance', GETDATE(), 'system'),

    (@ComplianceId, 'Regulatory reporting metrics for last month', 'Compliance reporting and regulatory metrics', 'last_month', 2, 1,
     'tbl_Daily_actions,tbl_Daily_actions_players', 2, 'compliance_reports', 'regulatory,reporting,compliance', GETDATE(), 'system');

-- Business Intelligence Suggestions (2 suggestions)
INSERT INTO [dbo].[QuerySuggestions] (
    [CategoryId], [QueryText], [Description], [DefaultTimeFrame], [SortOrder], [IsActive],
    [TargetTables], [Complexity], [RequiredPermissions], [Tags], [CreatedDate], [CreatedBy]
) VALUES
    (@BusinessId, 'Key performance indicators dashboard for this quarter', 'Strategic KPI tracking and business metrics', 'this_quarter', 1, 1,
     'tbl_Daily_actions,tbl_Daily_actions_players', 3, 'query:execute', 'kpi,dashboard,strategic', GETDATE(), 'system'),

    (@BusinessId, 'Market share analysis by product vertical', 'Competitive analysis and market positioning', 'this_month', 2, 1,
     'tbl_Daily_actions', 3, 'query:execute', 'market_share,competitive,analysis', GETDATE(), 'system');

-- Operations & Trends Suggestions (2 suggestions)
INSERT INTO [dbo].[QuerySuggestions] (
    [CategoryId], [QueryText], [Description], [DefaultTimeFrame], [SortOrder], [IsActive],
    [TargetTables], [Complexity], [RequiredPermissions], [Tags], [CreatedDate], [CreatedBy]
) VALUES
    (@OperationsId, 'Hourly revenue patterns for weekends this month', 'Peak activity analysis and operational insights', 'this_month', 1, 1,
     'tbl_Daily_actions', 2, 'query:execute', 'hourly,patterns,operations', GETDATE(), 'system'),

    (@OperationsId, 'System performance metrics and uptime this week', 'Operational health and system monitoring', 'this_week', 2, 1,
     'tbl_Daily_actions', 2, 'query:execute', 'performance,uptime,monitoring', GETDATE(), 'system');

PRINT 'Created 24 new suggestions (3 Financial, 3 Players, 3 Gaming, 3 Transactions, 2 Demographics, 2 Account, 2 Bonus, 2 Compliance, 2 Business, 2 Operations)';

-- =====================================================
-- Step 4: Verification
-- =====================================================

PRINT 'Verifying sync results...';

-- Count categories
DECLARE @CategoryCount INT = (SELECT COUNT(*) FROM [dbo].[SuggestionCategories] WHERE IsActive = 1);
PRINT 'Active categories: ' + CAST(@CategoryCount AS VARCHAR(10));

-- Count suggestions
DECLARE @SuggestionCount INT = (SELECT COUNT(*) FROM [dbo].[QuerySuggestions] WHERE IsActive = 1);
PRINT 'Active suggestions: ' + CAST(@SuggestionCount AS VARCHAR(10));

-- Show breakdown by category
SELECT 
    sc.Title as CategoryTitle,
    COUNT(qs.Id) as SuggestionCount
FROM [dbo].[SuggestionCategories] sc
LEFT JOIN [dbo].[QuerySuggestions] qs ON sc.Id = qs.CategoryId AND qs.IsActive = 1
WHERE sc.IsActive = 1
GROUP BY sc.Title, sc.SortOrder
ORDER BY sc.SortOrder;

-- =====================================================
-- Step 5: Commit transaction
-- =====================================================

IF @@ERROR = 0
BEGIN
    COMMIT TRANSACTION;
    PRINT 'SUCCESS: Suggestion database sync completed successfully!';
    PRINT 'Summary:';
    PRINT '- 10 categories created';
    PRINT '- 24 suggestions created';
    PRINT '- All suggestions are active and ready to use';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT 'ERROR: Transaction rolled back due to errors';
END

GO
