-- =====================================================
-- Populate Enhanced Semantic Metadata - Phase 2
-- =====================================================
-- This script populates the enhanced semantic metadata fields
-- for our existing 100% validated business intelligence foundation

-- =====================================================
-- Enhanced Table Semantic Metadata
-- =====================================================

-- Update tbl_Daily_actions with enhanced semantic metadata
UPDATE BusinessTableInfo 
SET 
    SemanticDescription = 'Comprehensive daily gaming activity aggregation table containing all player actions, financial transactions, and gaming metrics for a specific date. This is the primary fact table for daily operational reporting and player behavior analysis.',
    BusinessProcesses = '["Daily Reporting", "Player Activity Analysis", "Financial Reconciliation", "Gaming Performance Tracking", "Regulatory Reporting", "Revenue Analysis"]',
    AnalyticalUseCases = '["Daily Revenue Reports", "Player Engagement Analysis", "Game Performance Metrics", "Financial Reconciliation", "Regulatory Compliance Reports", "Operational Dashboards"]',
    ReportingCategories = '["Financial Reports", "Gaming Analytics", "Player Reports", "Operational Reports", "Compliance Reports"]',
    SemanticRelationships = '{"aggregates_from": ["tbl_Daily_actions_players", "tbl_Daily_actions_games"], "relates_to": ["tbl_Currencies", "tbl_Countries", "tbl_White_labels"], "supports": ["Daily Reporting", "Financial Analysis"]}',
    QueryComplexityHints = '["High volume table", "Date-based partitioning recommended", "Aggregation-friendly", "Complex financial calculations"]',
    BusinessGlossaryTerms = '["Daily Actions", "Gaming Activity", "Financial Transactions", "Player Behavior", "Revenue Analysis", "Gaming Metrics"]',
    SemanticCoverageScore = 1.0,
    LLMContextHints = '["Primary fact table for daily analysis", "Contains comprehensive gaming metrics", "Financial transaction aggregations", "Player activity summaries"]',
    VectorSearchKeywords = '["daily", "actions", "gaming", "activity", "financial", "transactions", "player", "behavior", "revenue", "metrics", "aggregation", "fact table"]'
WHERE TableName = 'tbl_Daily_actions'

-- Update tbl_Daily_actions_players with enhanced semantic metadata
UPDATE BusinessTableInfo 
SET 
    SemanticDescription = 'Comprehensive player profile and activity table containing detailed player information, preferences, financial history, and behavioral data. Essential for player analytics, segmentation, and personalized experiences.',
    BusinessProcesses = '["Player Management", "Customer Segmentation", "Personalization", "Responsible Gambling", "Marketing Campaigns", "Customer Service", "Compliance Management"]',
    AnalyticalUseCases = '["Player Segmentation", "Lifetime Value Analysis", "Churn Prediction", "Responsible Gambling Monitoring", "Marketing Attribution", "Customer Journey Analysis"]',
    ReportingCategories = '["Player Reports", "Customer Analytics", "Responsible Gambling", "Marketing Reports", "Compliance Reports"]',
    SemanticRelationships = '{"dimension_for": ["tbl_Daily_actions"], "relates_to": ["tbl_Countries", "tbl_Currencies"], "supports": ["Player Analytics", "Customer Management"]}',
    QueryComplexityHints = '["Large dimension table", "Complex player attributes", "Privacy-sensitive data", "Frequent updates"]',
    BusinessGlossaryTerms = '["Player Profile", "Customer Data", "Player Behavior", "Responsible Gambling", "Player Segmentation", "Customer Journey"]',
    SemanticCoverageScore = 1.0,
    LLMContextHints = '["Primary player dimension table", "Contains comprehensive player profiles", "Supports customer analytics", "Privacy-sensitive information"]',
    VectorSearchKeywords = '["player", "customer", "profile", "behavior", "segmentation", "responsible", "gambling", "preferences", "demographics", "dimension"]'
WHERE TableName = 'tbl_Daily_actions_players'

-- Update tbl_Daily_actionsGBP_transactions with enhanced semantic metadata
UPDATE BusinessTableInfo 
SET 
    SemanticDescription = 'Detailed financial transaction records in GBP currency providing granular transaction-level data for financial analysis, audit trails, and regulatory compliance.',
    BusinessProcesses = '["Financial Management", "Transaction Processing", "Audit Trail", "Regulatory Compliance", "Payment Processing", "Financial Reconciliation"]',
    AnalyticalUseCases = '["Transaction Analysis", "Payment Method Performance", "Financial Audit", "Fraud Detection", "Revenue Reconciliation", "Regulatory Reporting"]',
    ReportingCategories = '["Financial Reports", "Transaction Reports", "Audit Reports", "Compliance Reports"]',
    SemanticRelationships = '{"detail_for": ["tbl_Daily_actions"], "relates_to": ["tbl_Currencies"], "supports": ["Financial Analysis", "Audit Trail"]}',
    QueryComplexityHints = '["High volume transaction table", "Time-series data", "Financial precision required", "Audit trail importance"]',
    BusinessGlossaryTerms = '["Financial Transactions", "Payment Processing", "Audit Trail", "Financial Reconciliation", "Transaction Details"]',
    SemanticCoverageScore = 1.0,
    LLMContextHints = '["Detailed transaction records", "Financial audit trail", "GBP currency focus", "High precision financial data"]',
    VectorSearchKeywords = '["transaction", "financial", "payment", "audit", "gbp", "currency", "reconciliation", "detail", "money", "processing"]'
WHERE TableName = 'tbl_Daily_actionsGBP_transactions'

-- Update tbl_Daily_actions_games with enhanced semantic metadata
UPDATE BusinessTableInfo 
SET 
    SemanticDescription = 'Game-specific activity and performance metrics providing detailed insights into individual game performance, player engagement, and gaming analytics.',
    BusinessProcesses = '["Game Performance Analysis", "Content Management", "Player Engagement", "Game Optimization", "Revenue Analysis by Game"]',
    AnalyticalUseCases = '["Game Performance Reports", "Player Engagement by Game", "Game Revenue Analysis", "Content Optimization", "Game Portfolio Management"]',
    ReportingCategories = '["Gaming Reports", "Content Analytics", "Performance Reports", "Revenue Reports"]',
    SemanticRelationships = '{"detail_for": ["tbl_Daily_actions"], "relates_to": ["Games"], "supports": ["Game Analytics", "Content Performance"]}',
    QueryComplexityHints = '["Game-level aggregations", "Performance metrics", "Content analysis", "Engagement tracking"]',
    BusinessGlossaryTerms = '["Game Performance", "Gaming Analytics", "Player Engagement", "Content Analysis", "Game Metrics"]',
    SemanticCoverageScore = 1.0,
    LLMContextHints = '["Game-specific metrics", "Performance analytics", "Player engagement data", "Content optimization insights"]',
    VectorSearchKeywords = '["game", "gaming", "performance", "engagement", "content", "analytics", "metrics", "optimization", "revenue", "activity"]'
WHERE TableName = 'tbl_Daily_actions_games'

-- Update Games with enhanced semantic metadata
UPDATE BusinessTableInfo 
SET 
    SemanticDescription = 'Master game catalog containing comprehensive game information, metadata, and configuration details. Central reference for all gaming operations and analytics.',
    BusinessProcesses = '["Game Catalog Management", "Content Management", "Game Configuration", "Gaming Operations", "Content Analytics"]',
    AnalyticalUseCases = '["Game Catalog Reports", "Content Performance Analysis", "Game Configuration Management", "Gaming Portfolio Analysis"]',
    ReportingCategories = '["Game Catalog", "Content Reports", "Configuration Reports", "Gaming Operations"]',
    SemanticRelationships = '{"master_for": ["tbl_Daily_actions_games"], "supports": ["Gaming Operations", "Content Management"]}',
    QueryComplexityHints = '["Master reference table", "Game metadata", "Configuration data", "Lookup table"]',
    BusinessGlossaryTerms = '["Game Catalog", "Game Metadata", "Content Management", "Gaming Operations", "Game Configuration"]',
    SemanticCoverageScore = 1.0,
    LLMContextHints = '["Master game reference", "Game catalog and metadata", "Configuration information", "Gaming operations support"]',
    VectorSearchKeywords = '["game", "catalog", "master", "reference", "metadata", "configuration", "content", "gaming", "operations", "lookup"]'
WHERE TableName = 'Games'

-- Update tbl_Currencies with enhanced semantic metadata
UPDATE BusinessTableInfo 
SET 
    SemanticDescription = 'Master currency reference table providing currency codes, symbols, exchange rates, and currency-related metadata for multi-currency operations.',
    BusinessProcesses = '["Currency Management", "Financial Operations", "Multi-Currency Support", "Exchange Rate Management", "Financial Reporting"]',
    AnalyticalUseCases = '["Currency Analysis", "Exchange Rate Reporting", "Multi-Currency Financial Reports", "Currency Performance Analysis"]',
    ReportingCategories = '["Reference Data", "Financial Reports", "Currency Reports", "Exchange Rate Reports"]',
    SemanticRelationships = '{"reference_for": ["tbl_Daily_actions", "tbl_Daily_actions_players"], "supports": ["Financial Operations", "Multi-Currency"]}',
    QueryComplexityHints = '["Reference table", "Currency lookups", "Exchange rate calculations", "Multi-currency support"]',
    BusinessGlossaryTerms = '["Currency", "Exchange Rate", "Multi-Currency", "Financial Reference", "Currency Code"]',
    SemanticCoverageScore = 1.0,
    LLMContextHints = '["Currency reference data", "Exchange rate information", "Multi-currency support", "Financial operations"]',
    VectorSearchKeywords = '["currency", "exchange", "rate", "financial", "reference", "multi-currency", "code", "symbol", "money", "international"]'
WHERE TableName = 'tbl_Currencies'

-- Update tbl_Countries with enhanced semantic metadata
UPDATE BusinessTableInfo 
SET 
    SemanticDescription = 'Master country reference table providing country codes, names, regional information, and geographic metadata for international operations and compliance.',
    BusinessProcesses = '["Geographic Analysis", "Regulatory Compliance", "International Operations", "Regional Reporting", "Localization"]',
    AnalyticalUseCases = '["Geographic Reports", "Regional Analysis", "Compliance Reporting", "International Performance", "Localization Analytics"]',
    ReportingCategories = '["Reference Data", "Geographic Reports", "Regional Reports", "Compliance Reports"]',
    SemanticRelationships = '{"reference_for": ["tbl_Daily_actions_players"], "supports": ["Geographic Analysis", "Compliance"]}',
    QueryComplexityHints = '["Reference table", "Geographic lookups", "Regional groupings", "Compliance mapping"]',
    BusinessGlossaryTerms = '["Country", "Geographic", "Regional", "International", "Compliance", "Localization"]',
    SemanticCoverageScore = 1.0,
    LLMContextHints = '["Country reference data", "Geographic information", "Regional analysis support", "Compliance requirements"]',
    VectorSearchKeywords = '["country", "geographic", "regional", "international", "compliance", "location", "territory", "jurisdiction", "reference"]'
WHERE TableName = 'tbl_Countries'

-- Update tbl_White_labels with enhanced semantic metadata
UPDATE BusinessTableInfo 
SET 
    SemanticDescription = 'White label brand configuration and metadata table containing brand-specific settings, configurations, and operational parameters for multi-brand operations.',
    BusinessProcesses = '["Brand Management", "Multi-Brand Operations", "Brand Configuration", "Brand Analytics", "Operational Management"]',
    AnalyticalUseCases = '["Brand Performance Analysis", "Multi-Brand Reporting", "Brand Configuration Management", "Brand Comparison Analysis"]',
    ReportingCategories = '["Brand Reports", "Configuration Reports", "Multi-Brand Analytics", "Operational Reports"]',
    SemanticRelationships = '{"reference_for": ["tbl_Daily_actions"], "supports": ["Brand Management", "Multi-Brand Operations"]}',
    QueryComplexityHints = '["Brand reference table", "Configuration data", "Multi-brand support", "Operational parameters"]',
    BusinessGlossaryTerms = '["White Label", "Brand Management", "Multi-Brand", "Brand Configuration", "Brand Operations"]',
    SemanticCoverageScore = 1.0,
    LLMContextHints = '["Brand reference data", "Multi-brand configuration", "Brand-specific settings", "Operational parameters"]',
    VectorSearchKeywords = '["white", "label", "brand", "multi-brand", "configuration", "operations", "settings", "parameters", "reference"]'
WHERE TableName = 'tbl_White_labels'

SELECT 'Enhanced table semantic metadata populated successfully' as Status
