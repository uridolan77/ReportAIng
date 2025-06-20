-- =====================================================
-- Populate Enhanced Column Semantic Metadata - Phase 2
-- =====================================================
-- This script populates enhanced semantic metadata for key columns
-- focusing on high-impact columns for LLM understanding

-- =====================================================
-- Enhanced Column Semantic Metadata for Key Columns
-- =====================================================

-- Update key financial columns in tbl_Daily_actions
UPDATE BusinessColumnInfo 
SET 
    SemanticContext = 'Primary revenue metric representing total monetary value generated from player activities. Critical for financial reporting, revenue analysis, and business performance measurement.',
    ConceptualRelationships = '["drives_kpi", "financial_metric", "aggregates_from_transactions", "business_critical"]',
    DomainSpecificTerms = '["Revenue", "GGR", "Gross Gaming Revenue", "Financial Performance", "Business Metrics"]',
    QueryIntentMapping = '{"revenue_analysis": 0.9, "financial_reporting": 0.9, "performance_metrics": 0.8, "business_intelligence": 0.8}',
    BusinessQuestionTypes = '["How much revenue did we generate?", "What is our financial performance?", "Show me revenue trends", "Compare revenue across periods"]',
    SemanticSynonyms = '["revenue", "income", "earnings", "gross gaming revenue", "ggr", "financial performance", "monetary value"]',
    AnalyticalContext = 'Aggregation-friendly metric suitable for SUM, AVG, trend analysis, and comparative reporting across time periods and dimensions.',
    BusinessMetrics = '["Total Revenue", "Average Revenue", "Revenue Growth", "Revenue per Player", "Revenue Trends"]',
    SemanticRelevanceScore = 1.0,
    LLMPromptHints = '["Primary financial KPI", "Always aggregate with SUM for totals", "Compare across time periods", "Key business performance indicator"]',
    VectorSearchTags = '["revenue", "financial", "kpi", "performance", "money", "earnings", "business", "critical"]'
WHERE ColumnName = 'Revenue' AND TableInfoId = (SELECT Id FROM BusinessTableInfo WHERE TableName = 'tbl_Daily_actions')

-- Update Deposits column
UPDATE BusinessColumnInfo 
SET 
    SemanticContext = 'Total monetary deposits made by players, representing cash inflow and player investment. Essential for cash flow analysis and player value assessment.',
    ConceptualRelationships = '["cash_inflow", "player_investment", "financial_transaction", "liquidity_metric"]',
    DomainSpecificTerms = '["Deposits", "Cash Inflow", "Player Investment", "Funding", "Account Funding"]',
    QueryIntentMapping = '{"cash_flow_analysis": 0.9, "player_value": 0.8, "financial_analysis": 0.9, "deposit_trends": 1.0}',
    BusinessQuestionTypes = '["How much did players deposit?", "What are the deposit trends?", "Show me cash inflow", "Compare deposit volumes"]',
    SemanticSynonyms = '["deposits", "cash inflow", "player funding", "account deposits", "money in", "player investment"]',
    AnalyticalContext = 'Cash flow metric suitable for trend analysis, player segmentation, and financial forecasting.',
    BusinessMetrics = '["Total Deposits", "Average Deposit", "Deposit Growth", "Deposits per Player", "Deposit Frequency"]',
    SemanticRelevanceScore = 0.95,
    LLMPromptHints = '["Cash inflow metric", "Player investment indicator", "Trend analysis suitable", "Financial health indicator"]',
    VectorSearchTags = '["deposits", "cash", "inflow", "funding", "investment", "financial", "player", "money"]'
WHERE ColumnName = 'Deposits' AND TableInfoId = (SELECT Id FROM BusinessTableInfo WHERE TableName = 'tbl_Daily_actions')

-- Update Withdrawals column
UPDATE BusinessColumnInfo 
SET 
    SemanticContext = 'Total monetary withdrawals by players, representing cash outflow and player winnings. Critical for cash flow management and payout analysis.',
    ConceptualRelationships = '["cash_outflow", "player_winnings", "financial_transaction", "payout_metric"]',
    DomainSpecificTerms = '["Withdrawals", "Cash Outflow", "Player Winnings", "Payouts", "Account Withdrawals"]',
    QueryIntentMapping = '{"cash_flow_analysis": 0.9, "payout_analysis": 1.0, "financial_analysis": 0.9, "withdrawal_trends": 1.0}',
    BusinessQuestionTypes = '["How much did we pay out?", "What are withdrawal trends?", "Show me cash outflow", "Compare withdrawal volumes"]',
    SemanticSynonyms = '["withdrawals", "cash outflow", "payouts", "winnings", "money out", "player withdrawals"]',
    AnalyticalContext = 'Cash flow metric for payout analysis, liquidity management, and financial planning.',
    BusinessMetrics = '["Total Withdrawals", "Average Withdrawal", "Withdrawal Growth", "Withdrawals per Player", "Payout Ratio"]',
    SemanticRelevanceScore = 0.95,
    LLMPromptHints = '["Cash outflow metric", "Payout indicator", "Liquidity impact", "Financial obligation"]',
    VectorSearchTags = '["withdrawals", "cash", "outflow", "payouts", "winnings", "financial", "player", "money"]'
WHERE ColumnName = 'Withdrawals' AND TableInfoId = (SELECT Id FROM BusinessTableInfo WHERE TableName = 'tbl_Daily_actions')

-- Update key player identification columns
UPDATE BusinessColumnInfo 
SET 
    SemanticContext = 'Unique player identifier serving as primary key for player-related analysis. Essential for player tracking, segmentation, and personalized analytics.',
    ConceptualRelationships = '["primary_key", "player_identity", "unique_identifier", "dimension_key"]',
    DomainSpecificTerms = '["Player ID", "Customer ID", "User Identifier", "Player Key", "Customer Key"]',
    QueryIntentMapping = '{"player_analysis": 1.0, "customer_analytics": 1.0, "player_tracking": 1.0, "segmentation": 0.9}',
    BusinessQuestionTypes = '["Show me player details", "Analyze specific players", "Track player behavior", "Player-specific reports"]',
    SemanticSynonyms = '["player id", "customer id", "user id", "player identifier", "customer identifier", "player key"]',
    AnalyticalContext = 'Dimension key for player-centric analysis, segmentation, and detailed player reporting.',
    BusinessMetrics = '["Unique Players", "Player Count", "Active Players", "Player Segments"]',
    SemanticRelevanceScore = 1.0,
    LLMPromptHints = '["Primary player identifier", "Use for player-specific analysis", "Dimension key for joins", "Unique player tracking"]',
    VectorSearchTags = '["player", "customer", "user", "id", "identifier", "key", "unique", "dimension"]'
WHERE ColumnName = 'PlayerID' AND TableInfoId = (SELECT Id FROM BusinessTableInfo WHERE TableName = 'tbl_Daily_actions_players')

-- Update Email column for player contact
UPDATE BusinessColumnInfo 
SET 
    SemanticContext = 'Player email address for communication, identification, and marketing. Privacy-sensitive data requiring careful handling and GDPR compliance.',
    ConceptualRelationships = '["contact_information", "privacy_sensitive", "communication_channel", "identification_method"]',
    DomainSpecificTerms = '["Email Address", "Contact Information", "Communication Channel", "Player Contact"]',
    QueryIntentMapping = '{"customer_service": 0.8, "marketing_analysis": 0.7, "communication": 0.9, "contact_management": 1.0}',
    BusinessQuestionTypes = '["How to contact player?", "Player communication preferences", "Email marketing analysis", "Contact information"]',
    SemanticSynonyms = '["email", "email address", "contact email", "player email", "communication email"]',
    AnalyticalContext = 'Contact information for communication analysis and marketing campaigns. Handle with privacy considerations.',
    BusinessMetrics = '["Email Deliverability", "Communication Preferences", "Contact Quality", "Marketing Reach"]',
    SemanticRelevanceScore = 0.8,
    LLMPromptHints = '["Privacy-sensitive data", "Communication channel", "Marketing contact", "Handle with GDPR compliance"]',
    VectorSearchTags = '["email", "contact", "communication", "privacy", "marketing", "gdpr", "sensitive"]'
WHERE ColumnName = 'Email' AND TableInfoId = (SELECT Id FROM BusinessTableInfo WHERE TableName = 'tbl_Daily_actions_players')

-- Update AccountBalance for financial status
UPDATE BusinessColumnInfo 
SET 
    SemanticContext = 'Current real money balance in player account representing available funds for gaming. Critical for financial operations and player liquidity analysis.',
    ConceptualRelationships = '["financial_status", "liquidity_indicator", "real_time_metric", "player_wealth"]',
    DomainSpecificTerms = '["Account Balance", "Available Funds", "Player Balance", "Real Money Balance", "Account Funds"]',
    QueryIntentMapping = '{"financial_status": 1.0, "liquidity_analysis": 0.9, "player_value": 0.8, "account_management": 1.0}',
    BusinessQuestionTypes = '["What is player balance?", "Show account balances", "Player financial status", "Available funds analysis"]',
    SemanticSynonyms = '["account balance", "available funds", "player balance", "real money balance", "account funds", "wallet balance"]',
    AnalyticalContext = 'Real-time financial metric for player wealth analysis, liquidity management, and account status reporting.',
    BusinessMetrics = '["Total Account Balances", "Average Balance", "Balance Distribution", "High-Value Players"]',
    SemanticRelevanceScore = 0.95,
    LLMPromptHints = '["Real-time financial data", "Player liquidity indicator", "Account status metric", "Financial health indicator"]',
    VectorSearchTags = '["balance", "account", "funds", "money", "financial", "liquidity", "wealth", "real-time"]'
WHERE ColumnName = 'AccountBalance' AND TableInfoId = (SELECT Id FROM BusinessTableInfo WHERE TableName = 'tbl_Daily_actions_players')

-- Update Date column for temporal analysis
UPDATE BusinessColumnInfo 
SET 
    SemanticContext = 'Date dimension for temporal analysis and time-series reporting. Essential for trend analysis, period comparisons, and time-based aggregations.',
    ConceptualRelationships = '["time_dimension", "temporal_key", "trend_analysis", "period_identifier"]',
    DomainSpecificTerms = '["Date", "Time Period", "Reporting Date", "Activity Date", "Temporal Dimension"]',
    QueryIntentMapping = '{"trend_analysis": 1.0, "time_series": 1.0, "period_comparison": 1.0, "temporal_reporting": 1.0}',
    BusinessQuestionTypes = '["Show trends over time", "Compare periods", "Daily/weekly/monthly analysis", "Time-based reports"]',
    SemanticSynonyms = '["date", "time", "period", "day", "reporting date", "activity date", "temporal"]',
    AnalyticalContext = 'Primary time dimension for all temporal analysis, trending, and period-based comparisons.',
    BusinessMetrics = '["Daily Trends", "Period Comparisons", "Time Series", "Seasonal Analysis"]',
    SemanticRelevanceScore = 1.0,
    LLMPromptHints = '["Primary time dimension", "Use for trend analysis", "Period comparisons", "Time-series reporting"]',
    VectorSearchTags = '["date", "time", "period", "trend", "temporal", "series", "comparison", "dimension"]'
WHERE ColumnName = 'Date' AND TableInfoId = (SELECT Id FROM BusinessTableInfo WHERE TableName = 'tbl_Daily_actions')

-- Update enhanced business glossary terms
UPDATE BusinessGlossary 
SET 
    SemanticEmbedding = '{"vector": "placeholder_for_actual_embedding", "model": "text-embedding-ada-002", "dimensions": 1536}',
    QueryPatterns = '["What is revenue?", "How do we calculate revenue?", "Show me revenue trends", "Revenue analysis"]',
    LLMPromptTemplates = '["Revenue represents {definition}", "When analyzing revenue, consider {context}", "Revenue calculations include {components}"]',
    DisambiguationContext = 'In gaming context, revenue typically refers to Gross Gaming Revenue (GGR) which is player losses minus player wins.',
    SemanticRelationships = '{"is_a": "financial_metric", "measured_in": "currency", "calculated_from": "player_activity", "used_for": "business_analysis"}',
    ConceptualLevel = 'Operational',
    CrossDomainMappings = '{"finance": "gross_revenue", "gaming": "ggr", "business": "income", "analytics": "kpi"}',
    SemanticStability = 1.0,
    InferenceRules = '["revenue = deposits - withdrawals + other_income", "higher_revenue = better_performance", "revenue_trends_indicate_business_health"]'
WHERE Term = 'Revenue'

SELECT 'Enhanced column semantic metadata populated successfully' as Status
