-- =====================================================================================
-- TEMPLATE ANALYTICS SYSTEM - VALIDATION SCRIPT
-- =====================================================================================
-- This script validates that the Template Analytics System has been properly installed
-- and configured. Run this after executing the migration scripts.
-- 
-- Version: 1.0
-- Date: 2024-12-22
-- =====================================================================================

USE [BIReportingCopilot_Dev]
GO

PRINT '🔍 Template Analytics System Validation'
PRINT '========================================'
PRINT ''

-- =====================================================================================
-- 1. CHECK TABLE EXISTENCE
-- =====================================================================================

PRINT '📋 Checking Table Existence...'
PRINT ''

DECLARE @TableCount int = 0
DECLARE @ExpectedTables int = 8

-- Core Tables
IF EXISTS (SELECT * FROM sysobjects WHERE name='TemplatePerformanceMetrics' AND xtype='U')
BEGIN
    PRINT '  ✅ TemplatePerformanceMetrics - EXISTS'
    SET @TableCount = @TableCount + 1
END
ELSE
    PRINT '  ❌ TemplatePerformanceMetrics - MISSING'

IF EXISTS (SELECT * FROM sysobjects WHERE name='TemplateABTests' AND xtype='U')
BEGIN
    PRINT '  ✅ TemplateABTests - EXISTS'
    SET @TableCount = @TableCount + 1
END
ELSE
    PRINT '  ❌ TemplateABTests - MISSING'

IF EXISTS (SELECT * FROM sysobjects WHERE name='TemplateImprovementSuggestions' AND xtype='U')
BEGIN
    PRINT '  ✅ TemplateImprovementSuggestions - EXISTS'
    SET @TableCount = @TableCount + 1
END
ELSE
    PRINT '  ❌ TemplateImprovementSuggestions - MISSING'

-- Enhanced Tables
IF EXISTS (SELECT * FROM sysobjects WHERE name='TemplateUsageTracking' AND xtype='U')
BEGIN
    PRINT '  ✅ TemplateUsageTracking - EXISTS'
    SET @TableCount = @TableCount + 1
END
ELSE
    PRINT '  ⚠️  TemplateUsageTracking - MISSING (Optional - run Additional Tables script)'

IF EXISTS (SELECT * FROM sysobjects WHERE name='PerformanceAlerts' AND xtype='U')
BEGIN
    PRINT '  ✅ PerformanceAlerts - EXISTS'
    SET @TableCount = @TableCount + 1
END
ELSE
    PRINT '  ⚠️  PerformanceAlerts - MISSING (Optional - run Additional Tables script)'

IF EXISTS (SELECT * FROM sysobjects WHERE name='AnalyticsDashboardCache' AND xtype='U')
BEGIN
    PRINT '  ✅ AnalyticsDashboardCache - EXISTS'
    SET @TableCount = @TableCount + 1
END
ELSE
    PRINT '  ⚠️  AnalyticsDashboardCache - MISSING (Optional - run Additional Tables script)'

IF EXISTS (SELECT * FROM sysobjects WHERE name='TemplateAnalyticsAggregations' AND xtype='U')
BEGIN
    PRINT '  ✅ TemplateAnalyticsAggregations - EXISTS'
    SET @TableCount = @TableCount + 1
END
ELSE
    PRINT '  ⚠️  TemplateAnalyticsAggregations - MISSING (Optional - run Additional Tables script)'

IF EXISTS (SELECT * FROM sysobjects WHERE name='RealTimeAnalyticsSessions' AND xtype='U')
BEGIN
    PRINT '  ✅ RealTimeAnalyticsSessions - EXISTS'
    SET @TableCount = @TableCount + 1
END
ELSE
    PRINT '  ⚠️  RealTimeAnalyticsSessions - MISSING (Optional - run Additional Tables script)'

PRINT ''
PRINT CONCAT('📊 Tables Found: ', @TableCount, ' / ', @ExpectedTables)
PRINT ''

-- =====================================================================================
-- 2. CHECK FOREIGN KEY CONSTRAINTS
-- =====================================================================================

PRINT '🔗 Checking Foreign Key Constraints...'
PRINT ''

DECLARE @FKCount int = 0

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TemplatePerformanceMetrics_PromptTemplates')
BEGIN
    PRINT '  ✅ FK_TemplatePerformanceMetrics_PromptTemplates - EXISTS'
    SET @FKCount = @FKCount + 1
END
ELSE
    PRINT '  ❌ FK_TemplatePerformanceMetrics_PromptTemplates - MISSING'

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TemplateABTests_OriginalTemplate')
BEGIN
    PRINT '  ✅ FK_TemplateABTests_OriginalTemplate - EXISTS'
    SET @FKCount = @FKCount + 1
END
ELSE
    PRINT '  ❌ FK_TemplateABTests_OriginalTemplate - MISSING'

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TemplateABTests_VariantTemplate')
BEGIN
    PRINT '  ✅ FK_TemplateABTests_VariantTemplate - EXISTS'
    SET @FKCount = @FKCount + 1
END
ELSE
    PRINT '  ❌ FK_TemplateABTests_VariantTemplate - MISSING'

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_TemplateImprovementSuggestions_PromptTemplates')
BEGIN
    PRINT '  ✅ FK_TemplateImprovementSuggestions_PromptTemplates - EXISTS'
    SET @FKCount = @FKCount + 1
END
ELSE
    PRINT '  ❌ FK_TemplateImprovementSuggestions_PromptTemplates - MISSING'

PRINT ''
PRINT CONCAT('🔗 Foreign Keys Found: ', @FKCount, ' / 4 (Core)')
PRINT ''

-- =====================================================================================
-- 3. CHECK INDEXES
-- =====================================================================================

PRINT '📇 Checking Key Indexes...'
PRINT ''

DECLARE @IndexCount int = 0

-- Check critical indexes
IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('TemplatePerformanceMetrics') AND name = 'IX_TemplatePerformanceMetrics_TemplateKey')
BEGIN
    PRINT '  ✅ TemplatePerformanceMetrics.TemplateKey Index - EXISTS'
    SET @IndexCount = @IndexCount + 1
END

IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('TemplateABTests') AND name = 'IX_TemplateABTests_Status')
BEGIN
    PRINT '  ✅ TemplateABTests.Status Index - EXISTS'
    SET @IndexCount = @IndexCount + 1
END

IF EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'IX_TemplateImprovementSuggestions_Status')
BEGIN
    PRINT '  ✅ TemplateImprovementSuggestions.Status Index - EXISTS'
    SET @IndexCount = @IndexCount + 1
END

PRINT ''
PRINT CONCAT('📇 Key Indexes Found: ', @IndexCount, ' / 3')
PRINT ''

-- =====================================================================================
-- 4. CHECK SAMPLE DATA
-- =====================================================================================

PRINT '🌱 Checking Sample Data...'
PRINT ''

DECLARE @PerformanceMetricsCount int = 0
DECLARE @ABTestsCount int = 0
DECLARE @ImprovementSuggestionsCount int = 0

SELECT @PerformanceMetricsCount = COUNT(*) FROM TemplatePerformanceMetrics
SELECT @ABTestsCount = COUNT(*) FROM TemplateABTests
SELECT @ImprovementSuggestionsCount = COUNT(*) FROM TemplateImprovementSuggestions

PRINT CONCAT('  📊 TemplatePerformanceMetrics Records: ', @PerformanceMetricsCount)
PRINT CONCAT('  🧪 TemplateABTests Records: ', @ABTestsCount)
PRINT CONCAT('  💡 TemplateImprovementSuggestions Records: ', @ImprovementSuggestionsCount)

IF @PerformanceMetricsCount > 0 AND @ABTestsCount > 0 AND @ImprovementSuggestionsCount > 0
    PRINT '  ✅ Sample data is present in all core tables'
ELSE
    PRINT '  ⚠️  Some tables may be missing sample data'

PRINT ''

-- =====================================================================================
-- 5. CHECK ADMIN CONFIGURATION
-- =====================================================================================

PRINT '⚙️  Checking Admin Configuration...'
PRINT ''

DECLARE @ConfigCount int = 0

IF EXISTS (SELECT * FROM AdminConfiguration WHERE ConfigurationKey = 'TEMPLATE_ANALYTICS_ENABLED')
BEGIN
    DECLARE @AnalyticsEnabled nvarchar(10)
    SELECT @AnalyticsEnabled = ConfigurationValue FROM AdminConfiguration WHERE ConfigurationKey = 'TEMPLATE_ANALYTICS_ENABLED'
    PRINT CONCAT('  ✅ TEMPLATE_ANALYTICS_ENABLED: ', @AnalyticsEnabled)
    SET @ConfigCount = @ConfigCount + 1
END

IF EXISTS (SELECT * FROM AdminConfiguration WHERE ConfigurationKey = 'AB_TEST_MIN_SAMPLE_SIZE')
BEGIN
    DECLARE @MinSampleSize nvarchar(10)
    SELECT @MinSampleSize = ConfigurationValue FROM AdminConfiguration WHERE ConfigurationKey = 'AB_TEST_MIN_SAMPLE_SIZE'
    PRINT CONCAT('  ✅ AB_TEST_MIN_SAMPLE_SIZE: ', @MinSampleSize)
    SET @ConfigCount = @ConfigCount + 1
END

IF EXISTS (SELECT * FROM AdminConfiguration WHERE ConfigurationKey = 'REAL_TIME_ANALYTICS_ENABLED')
BEGIN
    DECLARE @RealTimeEnabled nvarchar(10)
    SELECT @RealTimeEnabled = ConfigurationValue FROM AdminConfiguration WHERE ConfigurationKey = 'REAL_TIME_ANALYTICS_ENABLED'
    PRINT CONCAT('  ✅ REAL_TIME_ANALYTICS_ENABLED: ', @RealTimeEnabled)
    SET @ConfigCount = @ConfigCount + 1
END

PRINT ''
PRINT CONCAT('⚙️  Configuration Settings Found: ', @ConfigCount, ' / 8 (Expected)')
PRINT ''

-- =====================================================================================
-- 6. PERFORMANCE TEST QUERIES
-- =====================================================================================

PRINT '🚀 Running Performance Test Queries...'
PRINT ''

-- Test query performance
DECLARE @StartTime datetime2 = GETUTCDATE()

-- Test 1: Template Performance Summary
SELECT TOP 5
    TemplateKey,
    IntentType,
    TotalUsages,
    SuccessRate,
    AverageProcessingTimeMs
FROM TemplatePerformanceMetrics
ORDER BY TotalUsages DESC

DECLARE @Query1Time int = DATEDIFF(millisecond, @StartTime, GETUTCDATE())
PRINT CONCAT('  ✅ Template Performance Query: ', @Query1Time, 'ms')

-- Test 2: Active A/B Tests
SET @StartTime = GETUTCDATE()

SELECT 
    TestName,
    Status,
    TrafficSplitPercent,
    DATEDIFF(day, StartDate, GETUTCDATE()) as DaysRunning
FROM TemplateABTests
WHERE Status = 'active'

DECLARE @Query2Time int = DATEDIFF(millisecond, @StartTime, GETUTCDATE())
PRINT CONCAT('  ✅ Active A/B Tests Query: ', @Query2Time, 'ms')

-- Test 3: Pending Improvements (with column existence check)
SET @StartTime = GETUTCDATE()

-- Check if required columns exist before querying
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'TemplateKey')
   AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TemplateImprovementSuggestions') AND name = 'ConfidenceScore')
BEGIN
    SELECT TOP 5
        TemplateKey,
        SuggestionType,
        ExpectedImprovementPercent,
        ConfidenceScore
    FROM TemplateImprovementSuggestions
    WHERE Status = 'pending'
    ORDER BY ConfidenceScore DESC

    DECLARE @Query3Time int = DATEDIFF(millisecond, @StartTime, GETUTCDATE())
    PRINT CONCAT('  ✅ Pending Improvements Query: ', @Query3Time, 'ms')
END
ELSE
BEGIN
    -- Fallback query without optional columns
    SELECT TOP 5
        CAST(TemplateId AS nvarchar(10)) as TemplateId,
        SuggestionType,
        ExpectedImprovementPercent,
        Status
    FROM TemplateImprovementSuggestions
    WHERE Status = 'pending'
    ORDER BY ExpectedImprovementPercent DESC

    DECLARE @Query3LimitedTime int = DATEDIFF(millisecond, @StartTime, GETUTCDATE())
    PRINT CONCAT('  ⚠️  Pending Improvements Query (Limited): ', @Query3LimitedTime, 'ms')
    PRINT '      Note: Some columns missing - run Template_Analytics_Fix_Missing_Columns.sql'
END

PRINT ''

-- =====================================================================================
-- 7. VALIDATION SUMMARY
-- =====================================================================================

PRINT '📋 VALIDATION SUMMARY'
PRINT '===================='
PRINT ''

DECLARE @OverallStatus nvarchar(20) = 'PASS'

-- Core requirements check
IF @TableCount < 3 -- At minimum, need the 3 core tables
BEGIN
    SET @OverallStatus = 'FAIL'
    PRINT '❌ CRITICAL: Missing core analytics tables'
END

IF @FKCount < 4
BEGIN
    SET @OverallStatus = 'FAIL'
    PRINT '❌ CRITICAL: Missing foreign key constraints'
END

IF @PerformanceMetricsCount = 0 AND @ABTestsCount = 0 AND @ImprovementSuggestionsCount = 0
BEGIN
    SET @OverallStatus = 'WARNING'
    PRINT '⚠️  WARNING: No sample data found'
END

-- Final status
IF @OverallStatus = 'PASS'
BEGIN
    PRINT '🎉 VALIDATION PASSED!'
    PRINT ''
    PRINT '✅ Template Analytics System is properly installed and configured'
    PRINT '✅ All core tables and constraints are in place'
    PRINT '✅ Sample data is available for testing'
    PRINT '✅ Performance queries are executing successfully'
    PRINT ''
    PRINT '🚀 The system is ready for production use!'
END
ELSE IF @OverallStatus = 'WARNING'
BEGIN
    PRINT '⚠️  VALIDATION PASSED WITH WARNINGS'
    PRINT ''
    PRINT '✅ Core system is functional'
    PRINT '⚠️  Some optional features or data may be missing'
    PRINT ''
    PRINT '📝 Review the warnings above and consider running additional migration scripts'
END
ELSE
BEGIN
    PRINT '❌ VALIDATION FAILED!'
    PRINT ''
    PRINT '❌ Critical components are missing'
    PRINT '📝 Review the errors above and re-run the migration scripts'
    PRINT ''
    PRINT '🔧 Troubleshooting:'
    PRINT '   1. Ensure you have proper database permissions'
    PRINT '   2. Verify the PromptTemplates table exists'
    PRINT '   3. Re-run Template_Analytics_Migration_Complete.sql'
END

PRINT ''
PRINT '🔍 Validation completed at: ' + CONVERT(nvarchar(30), GETUTCDATE(), 120)
PRINT ''
