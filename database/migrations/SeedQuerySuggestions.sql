-- =============================================
-- Seed Query Suggestions Data
-- Populates the suggestions system with comprehensive business questions
-- =============================================

-- Financial & Revenue Analytics
INSERT INTO [dbo].[QuerySuggestions] 
    ([CategoryId], [QueryText], [Description], [DefaultTimeFrame], [SortOrder], [TargetTables], [Complexity], [Tags], [CreatedBy])
SELECT 
    c.Id,
    'Show me total deposits for yesterday',
    'Daily deposit performance tracking',
    'yesterday',
    1,
    '["tbl_Daily_actions"]',
    1,
    '["deposits", "daily", "financial"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'financial'

UNION ALL SELECT 
    c.Id,
    'Revenue breakdown by country for last week',
    'Geographic revenue analysis and performance',
    'last_7_days',
    2,
    '["tbl_Daily_actions", "tbl_Daily_actions_players", "tbl_Countries"]',
    2,
    '["revenue", "geography", "countries"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'financial'

UNION ALL SELECT 
    c.Id,
    'First time deposit amounts by brand this month',
    'FTD performance analysis by casino brand',
    'this_month',
    3,
    '["tbl_Daily_actions", "tbl_White_labels"]',
    2,
    '["ftd", "brands", "acquisition"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'financial'

UNION ALL SELECT 
    c.Id,
    'Withdrawal vs deposit ratio by currency',
    'Financial flow analysis across currencies',
    'last_30_days',
    4,
    '["tbl_Daily_actions", "tbl_Daily_actions_players", "tbl_Currencies"]',
    2,
    '["withdrawals", "deposits", "currency", "flow"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'financial'

UNION ALL SELECT 
    c.Id,
    'Net gaming revenue trends last 30 days',
    'Revenue trend analysis and performance tracking',
    'last_30_days',
    5,
    '["tbl_Daily_actions"]',
    2,
    '["ngr", "trends", "performance"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'financial'

-- Player Analytics & Demographics
UNION ALL SELECT 
    c.Id,
    'Top 10 players by deposits in the last 7 days',
    'High-value player identification and analysis',
    'last_7_days',
    1,
    '["tbl_Daily_actions", "tbl_Daily_actions_players"]',
    2,
    '["high_value", "players", "deposits", "top_performers"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'players'

UNION ALL SELECT 
    c.Id,
    'New player registrations by country this week',
    'Player acquisition analysis by geography',
    'this_week',
    2,
    '["tbl_Daily_actions", "tbl_Daily_actions_players", "tbl_Countries"]',
    2,
    '["registrations", "acquisition", "geography"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'players'

UNION ALL SELECT 
    c.Id,
    'Active players by white label brand yesterday',
    'Brand performance comparison and analysis',
    'yesterday',
    3,
    '["tbl_Daily_actions", "tbl_White_labels"]',
    2,
    '["active_players", "brands", "performance"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'players'

UNION ALL SELECT 
    c.Id,
    'Player lifetime value by registration month',
    'Cohort value analysis and segmentation',
    'all_time',
    4,
    '["tbl_Daily_actions", "tbl_Daily_actions_players"]',
    3,
    '["ltv", "cohorts", "segmentation"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'players'

UNION ALL SELECT 
    c.Id,
    'VIP players with deposits over 10000 this month',
    'High-roller identification and targeting',
    'this_month',
    5,
    '["tbl_Daily_actions", "tbl_Daily_actions_players"]',
    2,
    '["vip", "high_rollers", "deposits"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'players'

-- Gaming & Product Analytics
UNION ALL SELECT 
    c.Id,
    'Top performing games by net gaming revenue this month',
    'Game performance ranking and analysis',
    'this_month',
    1,
    '["tbl_Daily_actions_games", "Games"]',
    2,
    '["games", "performance", "ngr"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'gaming'

UNION ALL SELECT 
    c.Id,
    'Slot vs table games revenue comparison this week',
    'Game type performance analysis',
    'this_week',
    2,
    '["tbl_Daily_actions_games", "Games"]',
    2,
    '["game_types", "slots", "tables", "comparison"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'gaming'

UNION ALL SELECT 
    c.Id,
    'NetEnt games performance this month',
    'Provider-specific performance analysis',
    'this_month',
    3,
    '["tbl_Daily_actions_games", "Games"]',
    2,
    '["providers", "netent", "performance"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'gaming'

UNION ALL SELECT 
    c.Id,
    'Games with highest player engagement this week',
    'Popular games identification and analysis',
    'this_week',
    4,
    '["tbl_Daily_actions_games", "Games"]',
    2,
    '["engagement", "popular_games", "sessions"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'gaming'

UNION ALL SELECT 
    c.Id,
    'Casino vs sports betting revenue split this month',
    'Product vertical comparison and analysis',
    'this_month',
    5,
    '["tbl_Daily_actions"]',
    2,
    '["casino", "sports", "verticals", "comparison"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'gaming'

-- Transaction & Payment Analytics
UNION ALL SELECT 
    c.Id,
    'Transaction volumes by payment method today',
    'Payment method usage analysis',
    'today',
    1,
    '["tbl_Daily_actions_transactions"]',
    2,
    '["transactions", "payment_methods", "volumes"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'transactions'

UNION ALL SELECT 
    c.Id,
    'Failed vs successful transactions by payment type this week',
    'Payment success rate analysis',
    'this_week',
    2,
    '["tbl_Daily_actions_transactions"]',
    2,
    '["transaction_status", "success_rates", "payment_types"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'transactions'

UNION ALL SELECT 
    c.Id,
    'Average transaction amount by payment method this month',
    'Payment method value insights',
    'this_month',
    3,
    '["tbl_Daily_actions_transactions"]',
    2,
    '["transaction_amounts", "payment_methods", "averages"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'transactions'

UNION ALL SELECT 
    c.Id,
    'Transaction processing times by payment provider this week',
    'Payment efficiency metrics',
    'this_week',
    4,
    '["tbl_Daily_actions_transactions"]',
    2,
    '["processing_times", "efficiency", "providers"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'transactions'

UNION ALL SELECT 
    c.Id,
    'Chargeback transactions by country and payment method this month',
    'Payment risk assessment',
    'this_month',
    5,
    '["tbl_Daily_actions_transactions", "tbl_Daily_actions_players", "tbl_Countries"]',
    3,
    '["chargebacks", "risk", "countries", "payment_methods"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'transactions'

-- Demographics & Behavior Analytics
UNION ALL SELECT
    c.Id,
    'Player age distribution by country and currency',
    'Comprehensive demographic analysis',
    'all_time',
    1,
    '["tbl_Daily_actions_players", "tbl_Countries", "tbl_Currencies"]',
    2,
    '["demographics", "age", "countries", "currencies"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'demographics'

UNION ALL SELECT
    c.Id,
    'Gender-based gaming and spending patterns this month',
    'Gender behavior insights and analysis',
    'this_month',
    2,
    '["tbl_Daily_actions", "tbl_Daily_actions_players"]',
    2,
    '["gender", "behavior", "spending", "patterns"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'demographics'

UNION ALL SELECT
    c.Id,
    'VIP players by registration source and device type',
    'High-value player profiling',
    'all_time',
    3,
    '["tbl_Daily_actions", "tbl_Daily_actions_players"]',
    3,
    '["vip", "registration_source", "device_type", "profiling"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'demographics'

UNION ALL SELECT
    c.Id,
    'Mobile vs desktop player engagement and revenue this week',
    'Device preference analysis',
    'this_week',
    4,
    '["tbl_Daily_actions", "tbl_Daily_actions_players"]',
    2,
    '["mobile", "desktop", "engagement", "device_analysis"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'demographics'

UNION ALL SELECT
    c.Id,
    'Player lifetime value by age group and location',
    'Demographic value segmentation',
    'all_time',
    5,
    '["tbl_Daily_actions", "tbl_Daily_actions_players", "tbl_Countries"]',
    3,
    '["ltv", "age_groups", "location", "segmentation"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'demographics'

-- Account Status & Player Lifecycle
UNION ALL SELECT
    c.Id,
    'Account verification completion rates by country this month',
    'KYC process efficiency analysis',
    'this_month',
    1,
    '["tbl_Daily_actions_players", "tbl_Countries"]',
    2,
    '["kyc", "verification", "countries", "completion_rates"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'accounts'

UNION ALL SELECT
    c.Id,
    'Suspended players by reason and brand this week',
    'Account status analysis',
    'this_week',
    2,
    '["tbl_Daily_actions_players", "tbl_White_labels"]',
    2,
    '["suspended", "account_status", "brands", "reasons"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'accounts'

UNION ALL SELECT
    c.Id,
    'Players with pending withdrawals over 48 hours',
    'Withdrawal queue monitoring',
    'today',
    3,
    '["tbl_Daily_actions", "tbl_Daily_actions_players"]',
    2,
    '["withdrawals", "pending", "queue", "monitoring"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'accounts'

UNION ALL SELECT
    c.Id,
    'Dormant high-value players for reactivation',
    'Win-back campaign targeting',
    'all_time',
    4,
    '["tbl_Daily_actions", "tbl_Daily_actions_players"]',
    3,
    '["dormant", "high_value", "reactivation", "winback"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'accounts'

UNION ALL SELECT
    c.Id,
    'New player conversion rates by registration channel this month',
    'Acquisition channel effectiveness',
    'this_month',
    5,
    '["tbl_Daily_actions", "tbl_Daily_actions_players"]',
    2,
    '["conversion", "registration", "channels", "acquisition"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'accounts'

-- Bonus & Promotions
UNION ALL SELECT
    c.Id,
    'Bonus bet amounts vs real money bets this week',
    'Bonus utilization analysis',
    'this_week',
    1,
    '["tbl_Daily_actions"]',
    2,
    '["bonus", "real_money", "utilization", "comparison"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'bonuses'

UNION ALL SELECT
    c.Id,
    'Players using bonuses by country this month',
    'Bonus adoption by region',
    'this_month',
    2,
    '["tbl_Daily_actions", "tbl_Daily_actions_players", "tbl_Countries"]',
    2,
    '["bonus_adoption", "countries", "regional_analysis"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'bonuses'

UNION ALL SELECT
    c.Id,
    'Bonus conversion rates by game type this month',
    'Bonus effectiveness analysis',
    'this_month',
    3,
    '["tbl_Daily_actions", "tbl_Daily_actions_games", "Games"]',
    3,
    '["bonus_conversion", "game_types", "effectiveness"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'bonuses'

-- Compliance & Risk Management
UNION ALL SELECT
    c.Id,
    'Players with unusual transaction patterns this week',
    'AML risk monitoring',
    'this_week',
    1,
    '["tbl_Daily_actions_transactions", "tbl_Daily_actions_players"]',
    3,
    '["aml", "unusual_patterns", "risk_monitoring"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'compliance'

UNION ALL SELECT
    c.Id,
    'Large transactions requiring manual review today',
    'Compliance workflow management',
    'today',
    2,
    '["tbl_Daily_actions_transactions"]',
    2,
    '["large_transactions", "manual_review", "compliance"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'compliance'

UNION ALL SELECT
    c.Id,
    'Self-excluded players activity verification this week',
    'Responsible gaming compliance',
    'this_week',
    3,
    '["tbl_Daily_actions", "tbl_Daily_actions_players"]',
    2,
    '["self_exclusion", "responsible_gaming", "verification"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'compliance'

UNION ALL SELECT
    c.Id,
    'Cross-border transaction compliance by jurisdiction this month',
    'Regulatory reporting',
    'this_month',
    4,
    '["tbl_Daily_actions_transactions", "tbl_Daily_actions_players", "tbl_Countries"]',
    3,
    '["cross_border", "jurisdiction", "regulatory", "compliance"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'compliance'

-- Business Intelligence & KPIs
UNION ALL SELECT
    c.Id,
    'Customer acquisition cost by marketing channel this month',
    'Marketing ROI analysis',
    'this_month',
    1,
    '["tbl_Daily_actions", "tbl_Daily_actions_players"]',
    3,
    '["cac", "marketing", "roi", "channels"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'business_intelligence'

UNION ALL SELECT
    c.Id,
    'Player lifetime value prediction by segment',
    'Predictive analytics',
    'all_time',
    2,
    '["tbl_Daily_actions", "tbl_Daily_actions_players"]',
    3,
    '["ltv_prediction", "predictive", "segmentation"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'business_intelligence'

UNION ALL SELECT
    c.Id,
    'Cross-sell success rates by player profile this quarter',
    'Product adoption insights',
    'this_quarter',
    3,
    '["tbl_Daily_actions", "tbl_Daily_actions_players"]',
    3,
    '["cross_sell", "product_adoption", "player_profiles"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'business_intelligence'

UNION ALL SELECT
    c.Id,
    'Churn prediction indicators by player behavior',
    'Retention risk assessment',
    'last_30_days',
    4,
    '["tbl_Daily_actions", "tbl_Daily_actions_players"]',
    3,
    '["churn_prediction", "retention", "risk_assessment"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'business_intelligence'

-- Operations & Trends
UNION ALL SELECT
    c.Id,
    'Hourly revenue patterns for weekends this month',
    'Peak activity analysis',
    'this_month',
    1,
    '["tbl_Daily_actions"]',
    2,
    '["hourly_patterns", "weekends", "peak_activity"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'operations'

UNION ALL SELECT
    c.Id,
    'Month over month growth by brand',
    'Growth trend comparison',
    'last_month',
    2,
    '["tbl_Daily_actions", "tbl_White_labels"]',
    2,
    '["mom_growth", "trends", "brands"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'operations'

UNION ALL SELECT
    c.Id,
    'Seasonal trends in player activity',
    'Seasonal pattern analysis',
    'last_year',
    3,
    '["tbl_Daily_actions", "tbl_Daily_actions_players"]',
    3,
    '["seasonal", "trends", "patterns"]',
    'System'
FROM [dbo].[SuggestionCategories] c WHERE c.CategoryKey = 'operations';
