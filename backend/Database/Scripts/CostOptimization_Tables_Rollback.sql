-- =====================================================
-- Cost Optimization & Performance Enhancement Tables
-- ROLLBACK Script for BI Reporting Copilot
-- =====================================================

USE [BIReportingCopilot_Dev]
GO

PRINT '============================================='
PRINT 'Starting Cost Optimization Tables Rollback'
PRINT '============================================='

-- =====================================================
-- 1. Drop Views First
-- =====================================================
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_LLMCostAnalyticsSummary')
BEGIN
    DROP VIEW [dbo].[vw_LLMCostAnalyticsSummary]
    PRINT 'Dropped vw_LLMCostAnalyticsSummary view'
END
ELSE
    PRINT 'vw_LLMCostAnalyticsSummary view does not exist'
GO

-- =====================================================
-- 2. Drop Tables (in reverse dependency order)
-- =====================================================

-- Drop Cost Optimization Recommendations Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CostOptimizationRecommendations]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[CostOptimizationRecommendations]
    PRINT 'Dropped CostOptimizationRecommendations table'
END
ELSE
    PRINT 'CostOptimizationRecommendations table does not exist'
GO

-- Drop LLM Cost Predictions Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMCostPredictions]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[LLMCostPredictions]
    PRINT 'Dropped LLMCostPredictions table'
END
ELSE
    PRINT 'LLMCostPredictions table does not exist'
GO

-- Drop LLM Resource Quotas Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMResourceQuotas]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[LLMResourceQuotas]
    PRINT 'Dropped LLMResourceQuotas table'
END
ELSE
    PRINT 'LLMResourceQuotas table does not exist'
GO

-- Drop LLM Cache Configuration Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMCacheConfiguration]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[LLMCacheConfiguration]
    PRINT 'Dropped LLMCacheConfiguration table'
END
ELSE
    PRINT 'LLMCacheConfiguration table does not exist'
GO

-- Drop LLM Cache Statistics Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMCacheStatistics]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[LLMCacheStatistics]
    PRINT 'Dropped LLMCacheStatistics table'
END
ELSE
    PRINT 'LLMCacheStatistics table does not exist'
GO

-- Drop LLM Performance Metrics Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMPerformanceMetrics]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[LLMPerformanceMetrics]
    PRINT 'Dropped LLMPerformanceMetrics table'
END
ELSE
    PRINT 'LLMPerformanceMetrics table does not exist'
GO

-- Drop LLM Resource Usage Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMResourceUsage]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[LLMResourceUsage]
    PRINT 'Dropped LLMResourceUsage table'
END
ELSE
    PRINT 'LLMResourceUsage table does not exist'
GO

-- Drop LLM Budget Management Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMBudgetManagement]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[LLMBudgetManagement]
    PRINT 'Dropped LLMBudgetManagement table'
END
ELSE
    PRINT 'LLMBudgetManagement table does not exist'
GO

-- Drop LLM Cost Tracking Table
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LLMCostTracking]') AND type in (N'U'))
BEGIN
    DROP TABLE [dbo].[LLMCostTracking]
    PRINT 'Dropped LLMCostTracking table'
END
ELSE
    PRINT 'LLMCostTracking table does not exist'
GO

-- =====================================================
-- 3. Verification
-- =====================================================
DECLARE @RemainingTables INT
SELECT @RemainingTables = COUNT(*)
FROM sys.objects
WHERE object_id IN (
    OBJECT_ID(N'[dbo].[LLMCostTracking]'),
    OBJECT_ID(N'[dbo].[LLMBudgetManagement]'),
    OBJECT_ID(N'[dbo].[LLMResourceUsage]'),
    OBJECT_ID(N'[dbo].[LLMPerformanceMetrics]'),
    OBJECT_ID(N'[dbo].[LLMCacheStatistics]'),
    OBJECT_ID(N'[dbo].[LLMCacheConfiguration]'),
    OBJECT_ID(N'[dbo].[LLMResourceQuotas]'),
    OBJECT_ID(N'[dbo].[LLMCostPredictions]'),
    OBJECT_ID(N'[dbo].[CostOptimizationRecommendations]')
) AND type in (N'U')

DECLARE @RemainingViews INT
SELECT @RemainingViews = COUNT(*)
FROM sys.views
WHERE name IN ('vw_LLMCostAnalyticsSummary')

IF @RemainingTables = 0 AND @RemainingViews = 0
BEGIN
    PRINT '✓ All cost optimization tables and views successfully removed'
    PRINT '✓ Rollback completed successfully'
END
ELSE
BEGIN
    PRINT '⚠ Warning: Some tables or views may still exist'
    PRINT 'Remaining tables: ' + CAST(@RemainingTables AS NVARCHAR(10))
    PRINT 'Remaining views: ' + CAST(@RemainingViews AS NVARCHAR(10))
END

PRINT '============================================='
PRINT 'Cost Optimization Tables Rollback Complete'
PRINT '============================================='
