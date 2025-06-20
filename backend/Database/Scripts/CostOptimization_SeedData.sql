-- =====================================================
-- Cost Optimization & Performance Enhancement
-- Seed Data Script for BI Reporting Copilot
-- =====================================================

USE [BIReportingCopilot_Dev]
GO

-- Set proper options for indexed views and computed columns
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

PRINT '============================================='
PRINT 'Starting Cost Optimization Seed Data Insert'
PRINT '============================================='

-- =====================================================
-- 1. Seed Cache Configurations
-- =====================================================
PRINT 'Seeding Cache Configurations...'

MERGE [dbo].[LLMCacheConfiguration] AS target
USING (VALUES 
    ('SemanticCache', 10000, 3600, 'LRU', 1, 0, 1, 'SELECT TOP 100 * FROM BusinessTableInfo ORDER BY LastUpdated DESC'),
    ('QueryCache', 5000, 1800, 'LRU', 1, 0, 0, NULL),
    ('ResultCache', 2000, 900, 'LFU', 1, 0, 0, NULL),
    ('MetadataCache', 1000, 7200, 'LRU', 0, 0, 1, 'SELECT * FROM BusinessTableInfo WHERE IsActive = 1'),
    ('UserSessionCache', 500, 1800, 'TTL', 0, 1, 0, NULL),
    ('AIModelCache', 1000, 14400, 'LRU', 1, 0, 1, NULL),
    ('SchemaCache', 2000, 21600, 'LRU', 0, 0, 1, 'SELECT * FROM BusinessTableInfo'),
    ('PerformanceCache', 1500, 3600, 'LFU', 1, 0, 0, NULL)
) AS source ([CacheType], [MaxSize], [TTLSeconds], [EvictionPolicy], [CompressionEnabled], [EncryptionEnabled], [WarmupEnabled], [WarmupQuery])
ON target.[CacheType] = source.[CacheType]
WHEN NOT MATCHED THEN
    INSERT ([CacheType], [MaxSize], [TTLSeconds], [EvictionPolicy], [CompressionEnabled], [EncryptionEnabled], [WarmupEnabled], [WarmupQuery], [IsActive])
    VALUES (source.[CacheType], source.[MaxSize], source.[TTLSeconds], source.[EvictionPolicy], source.[CompressionEnabled], source.[EncryptionEnabled], source.[WarmupEnabled], source.[WarmupQuery], 1);

PRINT 'Cache configurations seeded successfully'

-- =====================================================
-- 2. Seed Default Resource Quotas for All Users
-- =====================================================
PRINT 'Seeding Default Resource Quotas...'

-- Insert default quotas for existing users
INSERT INTO [dbo].[LLMResourceQuotas] ([UserId], [ResourceType], [MaxQuantity], [PeriodSeconds], [ResetDate], [IsActive])
SELECT
    u.Id,
    quota.[ResourceType],
    quota.[MaxQuantity],
    quota.[PeriodSeconds],
    DATEADD(day, 1, GETUTCDATE()) as [ResetDate],
    1 as [IsActive]
FROM [dbo].[Users] u
CROSS JOIN (VALUES
    ('ai-requests', 1000, 86400),      -- 1000 AI requests per day
    ('database-queries', 5000, 86400), -- 5000 DB queries per day
    ('cache-operations', 10000, 86400), -- 10000 cache ops per day
    ('report-generations', 100, 86400), -- 100 reports per day
    ('data-exports', 50, 86400),       -- 50 exports per day
    ('api-calls', 2000, 86400)         -- 2000 API calls per day
) AS quota([ResourceType], [MaxQuantity], [PeriodSeconds])
WHERE NOT EXISTS (
    SELECT 1 FROM [dbo].[LLMResourceQuotas] rq
    WHERE rq.UserId = u.Id AND rq.ResourceType = quota.[ResourceType] AND rq.IsActive = 1
)

PRINT 'Default resource quotas seeded successfully'

-- =====================================================
-- 3. Seed Cost Optimization Recommendations
-- =====================================================
PRINT 'Seeding Cost Optimization Recommendations...'

INSERT INTO [dbo].[CostOptimizationRecommendations] 
([Type], [Title], [Description], [PotentialSavings], [ImpactScore], [Priority], [Implementation], [Benefits], [Risks])
VALUES 
('CacheOptimization', 'Implement Semantic Caching', 'Enable semantic caching to reduce duplicate AI model calls for similar queries', 150.00, 0.85, 'High', 
 'Configure semantic similarity threshold and enable cache warming for frequently accessed queries', 
 'Reduced API costs, Faster response times, Better user experience', 
 'Initial setup complexity, Cache invalidation challenges'),

('ModelSelection', 'Dynamic Model Selection', 'Automatically select the most cost-effective AI model based on query complexity', 200.00, 0.90, 'High',
 'Implement model selection algorithm based on query analysis and cost thresholds',
 'Significant cost reduction, Maintained quality, Automatic optimization',
 'Potential quality variations, Complex implementation'),

('QueryOptimization', 'Query Prompt Optimization', 'Optimize AI prompts to reduce token usage while maintaining accuracy', 100.00, 0.75, 'Medium',
 'Analyze prompt patterns and implement template optimization',
 'Reduced token costs, Faster processing, Consistent results',
 'Development time investment, Testing requirements'),

('ResourceManagement', 'Implement Request Throttling', 'Add intelligent request throttling to prevent cost spikes', 75.00, 0.70, 'Medium',
 'Configure rate limiting and priority-based queuing system',
 'Cost predictability, Resource protection, Fair usage',
 'User experience impact, Configuration complexity'),

('BudgetControl', 'Automated Budget Alerts', 'Set up proactive budget monitoring and automatic cost controls', 50.00, 0.80, 'High',
 'Configure budget thresholds and automated blocking mechanisms',
 'Cost control, Proactive monitoring, Automated protection',
 'Potential service interruptions, Alert fatigue'),

('PerformanceOptimization', 'Database Query Optimization', 'Optimize database queries to reduce execution time and resource usage', 80.00, 0.65, 'Medium',
 'Analyze slow queries and implement indexing and query optimization',
 'Faster response times, Reduced resource usage, Better scalability',
 'Database maintenance overhead, Index storage costs')

PRINT 'Cost optimization recommendations seeded successfully'

-- =====================================================
-- 4. Seed Performance Metrics Baselines
-- =====================================================
PRINT 'Seeding Performance Metrics Baselines...'

INSERT INTO [dbo].[LLMPerformanceMetrics]
([EntityType], [EntityId], [MetricName], [MetricValue], [MetricUnit], [AverageExecutionTime], [TotalOperations], [SuccessCount], [ErrorCount], [Metadata])
VALUES 
('System', 'Main', 'AverageResponseTime', 2500.0, 'Milliseconds', 2500, 1000, 950, 50, '{"baseline": true, "target": 2000}'),
('System', 'Main', 'ThroughputPerSecond', 10.0, 'RequestsPerSecond', 0, 1000, 1000, 0, '{"baseline": true, "target": 15}'),
('System', 'Main', 'ErrorRate', 0.05, 'Percentage', 0, 1000, 950, 50, '{"baseline": true, "target": 0.02}'),
('System', 'Main', 'CacheHitRate', 0.75, 'Percentage', 0, 1000, 750, 0, '{"baseline": true, "target": 0.85}'),
('Database', 'Primary', 'QueryExecutionTime', 150.0, 'Milliseconds', 150, 5000, 4950, 50, '{"baseline": true, "target": 100}'),
('Database', 'Primary', 'ConnectionPoolUtilization', 0.60, 'Percentage', 0, 0, 0, 0, '{"baseline": true, "target": 0.70}'),
('AI', 'OpenAI', 'TokensPerSecond', 50.0, 'TokensPerSecond', 0, 500, 500, 0, '{"baseline": true, "target": 75}'),
('AI', 'OpenAI', 'CostPerRequest', 0.05, 'USD', 2000, 500, 500, 0, '{"baseline": true, "target": 0.04}'),
('Cache', 'SemanticCache', 'HitRate', 0.70, 'Percentage', 5, 2000, 1400, 0, '{"baseline": true, "target": 0.85}'),
('Cache', 'QueryCache', 'HitRate', 0.80, 'Percentage', 2, 3000, 2400, 0, '{"baseline": true, "target": 0.90}')

PRINT 'Performance metrics baselines seeded successfully'

-- =====================================================
-- 5. Seed Cache Statistics
-- =====================================================
PRINT 'Seeding Cache Statistics...'

DECLARE @PeriodStart DATETIME2(7) = DATEADD(hour, -24, GETUTCDATE())
DECLARE @PeriodEnd DATETIME2(7) = GETUTCDATE()

INSERT INTO [dbo].[LLMCacheStatistics]
([CacheType], [TotalOperations], [HitCount], [MissCount], [SetCount], [DeleteCount], [HitRate], [AverageResponseTime], [TotalSizeBytes], [PeriodStart], [PeriodEnd], [Metadata])
VALUES 
('SemanticCache', 2000, 1400, 600, 600, 50, 0.70, 5.2, 1048576, @PeriodStart, @PeriodEnd, '{"compression_ratio": 0.6, "evictions": 25}'),
('QueryCache', 3000, 2400, 600, 600, 100, 0.80, 2.1, 524288, @PeriodStart, @PeriodEnd, '{"compression_ratio": 0.7, "evictions": 50}'),
('ResultCache', 1500, 1200, 300, 300, 75, 0.80, 1.8, 262144, @PeriodStart, @PeriodEnd, '{"compression_ratio": 0.8, "evictions": 30}'),
('MetadataCache', 800, 720, 80, 80, 20, 0.90, 0.5, 131072, @PeriodStart, @PeriodEnd, '{"compression_ratio": 0.5, "evictions": 5}'),
('UserSessionCache', 1200, 960, 240, 240, 60, 0.80, 1.2, 65536, @PeriodStart, @PeriodEnd, '{"compression_ratio": 0.9, "evictions": 40}'),
('AIModelCache', 500, 400, 100, 100, 25, 0.80, 3.5, 2097152, @PeriodStart, @PeriodEnd, '{"compression_ratio": 0.4, "evictions": 10}'),
('SchemaCache', 300, 285, 15, 15, 5, 0.95, 0.8, 32768, @PeriodStart, @PeriodEnd, '{"compression_ratio": 0.3, "evictions": 2}'),
('PerformanceCache', 600, 480, 120, 120, 30, 0.80, 2.0, 98304, @PeriodStart, @PeriodEnd, '{"compression_ratio": 0.6, "evictions": 15}')

PRINT 'Cache statistics seeded successfully'

-- =====================================================
-- 6. Create Sample Budget for Admin User
-- =====================================================
PRINT 'Creating Sample Budget...'

DECLARE @AdminUserId NVARCHAR(256)
SELECT @AdminUserId = Id FROM [dbo].[Users] WHERE Email = 'admin@bireportingcopilot.com'

IF @AdminUserId IS NOT NULL
BEGIN
    INSERT INTO [dbo].[LLMBudgetManagement]
    ([Name], [Type], [EntityId], [BudgetAmount], [SpentAmount], [RemainingAmount], [Period], [StartDate], [EndDate], [AlertThreshold], [BlockThreshold], [IsActive])
    VALUES 
    ('Monthly AI Costs', 'User', @AdminUserId, 1000.00, 150.00, 850.00, 'Monthly', 
     DATEFROMPARTS(YEAR(GETUTCDATE()), MONTH(GETUTCDATE()), 1), 
     EOMONTH(GETUTCDATE()), 0.80, 0.95, 1),
    ('Weekly Development Budget', 'User', @AdminUserId, 250.00, 45.00, 205.00, 'Weekly',
     DATEADD(day, -(DATEPART(weekday, GETUTCDATE()) - 1), CAST(GETUTCDATE() AS DATE)),
     DATEADD(day, 7 - DATEPART(weekday, GETUTCDATE()), CAST(GETUTCDATE() AS DATE)), 0.75, 0.90, 1)
    
    PRINT 'Sample budgets created for admin user'
END
ELSE
    PRINT 'Admin user not found - skipping budget creation'

-- =====================================================
-- 7. Verification
-- =====================================================
PRINT 'Verifying seed data...'

SELECT
    'LLMCacheConfiguration' as TableName,
    COUNT(*) as RecordCount
FROM [dbo].[LLMCacheConfiguration]
UNION ALL
SELECT
    'LLMResourceQuotas' as TableName,
    COUNT(*) as RecordCount
FROM [dbo].[LLMResourceQuotas]
UNION ALL
SELECT
    'CostOptimizationRecommendations' as TableName,
    COUNT(*) as RecordCount
FROM [dbo].[CostOptimizationRecommendations]
UNION ALL
SELECT
    'LLMPerformanceMetrics' as TableName,
    COUNT(*) as RecordCount
FROM [dbo].[LLMPerformanceMetrics]
UNION ALL
SELECT
    'LLMCacheStatistics' as TableName,
    COUNT(*) as RecordCount
FROM [dbo].[LLMCacheStatistics]
UNION ALL
SELECT
    'LLMBudgetManagement' as TableName,
    COUNT(*) as RecordCount
FROM [dbo].[LLMBudgetManagement]

PRINT '============================================='
PRINT 'Cost Optimization Seed Data Insert Complete'
PRINT '============================================='
