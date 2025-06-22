# Template Analytics System - Database Migration Guide

## 📋 Overview

This guide provides comprehensive database migration scripts for the **Template Analytics System**, which includes performance metrics tracking, A/B testing capabilities, and automated template improvement suggestions.

## 🗂️ Migration Scripts

### 1. **Template_Analytics_Migration_Complete.sql**
**Primary migration script** that creates/updates core analytics tables:

- **TemplatePerformanceMetrics** - Tracks template usage, success rates, and performance metrics
- **TemplateABTests** - Manages A/B testing for template variations
- **TemplateImprovementSuggestions** - Stores AI-generated improvement recommendations
- **Foreign Key Constraints** - Ensures data integrity
- **Seed Data** - Sample data for testing and development
- **Admin Configuration** - Analytics system settings

### 2. **Template_Analytics_Additional_Tables.sql**
**Enhanced features script** that adds advanced analytics capabilities:

- **TemplateUsageTracking** - Detailed per-interaction tracking
- **PerformanceAlerts** - Real-time performance monitoring and alerts
- **AnalyticsDashboardCache** - Performance optimization for dashboards
- **TemplateAnalyticsAggregations** - Pre-computed analytics summaries
- **RealTimeAnalyticsSessions** - SignalR real-time update management

## 🚀 Installation Instructions

### Prerequisites
- SQL Server 2019 or later
- Database: `BIReportingCopilot_Dev` (or update script to match your database name)
- Existing `PromptTemplates` table (for foreign key relationships)

### Step 1: Run Core Migration
```sql
-- Execute the primary migration script
sqlcmd -S your_server -d BIReportingCopilot_Dev -i Template_Analytics_Migration_Complete.sql
```

### Step 2: Run Additional Features (Optional)
```sql
-- Execute the enhanced features script
sqlcmd -S your_server -d BIReportingCopilot_Dev -i Template_Analytics_Additional_Tables.sql
```

### Step 3: Verify Installation
```sql
-- Check that all tables were created
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN (
    'TemplatePerformanceMetrics',
    'TemplateABTests', 
    'TemplateImprovementSuggestions',
    'TemplateUsageTracking',
    'PerformanceAlerts',
    'AnalyticsDashboardCache',
    'TemplateAnalyticsAggregations',
    'RealTimeAnalyticsSessions'
)
ORDER BY TABLE_NAME;
```

## 📊 Database Schema Details

### Core Tables

#### TemplatePerformanceMetrics
Tracks comprehensive performance metrics for each template:
```sql
- Id (bigint, PK)
- TemplateId (bigint, FK to PromptTemplates)
- TemplateKey (nvarchar(100))
- IntentType (nvarchar(50))
- TotalUsages (int)
- SuccessfulUsages (int)
- SuccessRate (decimal(5,4))
- AverageConfidenceScore (decimal(5,4))
- AverageProcessingTimeMs (int)
- AverageUserRating (decimal(3,2))
- LastUsedDate (datetime2)
- AnalysisDate (datetime2)
- CreatedBy (nvarchar(256))
- AdditionalMetrics (nvarchar(max)) -- JSON
```

#### TemplateABTests
Manages A/B testing experiments:
```sql
- Id (bigint, PK)
- TestName (nvarchar(100))
- OriginalTemplateId (bigint, FK)
- VariantTemplateId (bigint, FK)
- TrafficSplitPercent (int)
- StartDate/EndDate (datetime2)
- Status (nvarchar(20)) -- 'active', 'completed', 'paused'
- OriginalSuccessRate/VariantSuccessRate (decimal(5,4))
- StatisticalSignificance (decimal(5,4))
- WinnerTemplateId (bigint)
- TestResults (nvarchar(max)) -- JSON
- ConfidenceLevel (decimal(5,4))
- Conclusion (nvarchar(1000))
```

#### TemplateImprovementSuggestions
Stores AI-generated improvement recommendations:
```sql
- Id (bigint, PK)
- TemplateId (bigint, FK)
- SuggestionType (nvarchar(50))
- SuggestedChanges (nvarchar(max)) -- JSON
- ReasoningExplanation (nvarchar(2000))
- ExpectedImprovementPercent (decimal(5,2))
- Status (nvarchar(20)) -- 'pending', 'approved', 'rejected'
- ConfidenceScore (decimal(5,4))
- Priority (int)
```

### Enhanced Tables

#### TemplateUsageTracking
Detailed per-interaction tracking:
```sql
- Id (bigint, PK)
- TemplateId (bigint, FK)
- UserId (nvarchar(256))
- SessionId (nvarchar(128))
- QueryText (nvarchar(max))
- ResponseGenerated (bit)
- ProcessingTimeMs (int)
- UserFeedback (nvarchar(20))
- ErrorOccurred (bit)
- ContextData (nvarchar(max)) -- JSON
```

#### PerformanceAlerts
Real-time monitoring and alerting:
```sql
- Id (bigint, PK)
- AlertType (nvarchar(50))
- Severity (nvarchar(20)) -- 'low', 'medium', 'high', 'critical'
- TemplateId (bigint, FK, optional)
- AlertTitle/AlertMessage (nvarchar)
- CurrentValue/ThresholdValue (decimal(18,6))
- Status (nvarchar(20)) -- 'active', 'acknowledged', 'resolved'
```

## 🔧 Configuration Settings

The migration scripts automatically configure the following admin settings:

| Setting | Default Value | Description |
|---------|---------------|-------------|
| `TEMPLATE_ANALYTICS_ENABLED` | `true` | Enable template analytics system |
| `TEMPLATE_PERFORMANCE_RETENTION_DAYS` | `365` | Data retention period |
| `AB_TEST_MIN_SAMPLE_SIZE` | `100` | Minimum sample size for A/B tests |
| `AB_TEST_CONFIDENCE_THRESHOLD` | `0.95` | Statistical significance threshold |
| `PERFORMANCE_METRICS_UPDATE_INTERVAL_HOURS` | `6` | Metrics update frequency |
| `REAL_TIME_ANALYTICS_ENABLED` | `true` | Enable real-time updates |

## 🌱 Sample Data

Both scripts include sample data for immediate testing:

- **Performance Metrics**: 5 sample templates with realistic usage statistics
- **A/B Tests**: 2 sample tests (one active, one completed)
- **Improvement Suggestions**: 3 sample AI-generated suggestions
- **Performance Alerts**: 3 sample alerts with different severity levels
- **Dashboard Cache**: Sample cached dashboard data

## 🔍 Verification Queries

### Check Template Performance Data
```sql
SELECT TOP 10 
    TemplateKey,
    IntentType,
    TotalUsages,
    SuccessRate,
    AverageProcessingTimeMs
FROM TemplatePerformanceMetrics
ORDER BY TotalUsages DESC;
```

### Check Active A/B Tests
```sql
SELECT 
    TestName,
    Status,
    TrafficSplitPercent,
    OriginalSuccessRate,
    VariantSuccessRate,
    StatisticalSignificance
FROM TemplateABTests
WHERE Status = 'active';
```

### Check Pending Improvements
```sql
SELECT 
    TemplateKey,
    SuggestionType,
    ExpectedImprovementPercent,
    ConfidenceScore,
    Status
FROM TemplateImprovementSuggestions
WHERE Status = 'pending'
ORDER BY ConfidenceScore DESC;
```

## 🚨 Troubleshooting

### Common Issues

1. **Foreign Key Constraint Errors**
   - Ensure `PromptTemplates` table exists before running migration
   - Check that referenced template IDs exist in the PromptTemplates table

2. **Permission Errors**
   - Ensure the database user has `db_ddladmin` and `db_datawriter` permissions
   - Run scripts with appropriate SQL Server authentication

3. **Duplicate Key Errors**
   - Scripts are designed to be re-runnable
   - Use `IF NOT EXISTS` checks to prevent duplicate creation

### Rollback Instructions

To remove the analytics tables (if needed):
```sql
-- Drop tables in reverse dependency order
DROP TABLE IF EXISTS RealTimeAnalyticsSessions;
DROP TABLE IF EXISTS TemplateAnalyticsAggregations;
DROP TABLE IF EXISTS AnalyticsDashboardCache;
DROP TABLE IF EXISTS PerformanceAlerts;
DROP TABLE IF EXISTS TemplateUsageTracking;
DROP TABLE IF EXISTS TemplateImprovementSuggestions;
DROP TABLE IF EXISTS TemplateABTests;
DROP TABLE IF EXISTS TemplatePerformanceMetrics;
```

## 📞 Support

For issues or questions regarding the Template Analytics migration:

1. Check the migration script output for specific error messages
2. Verify database permissions and connectivity
3. Ensure all prerequisite tables exist
4. Review the troubleshooting section above

## 🎯 Next Steps

After successful migration:

1. **Configure Analytics Settings**: Review and adjust admin configuration values
2. **Set Up Monitoring**: Configure performance alert thresholds
3. **Enable Real-time Updates**: Ensure SignalR is configured for live analytics
4. **Test API Endpoints**: Verify Template Analytics Controller endpoints
5. **Initialize Data Collection**: Begin collecting template usage data

The Template Analytics System is now ready for production use! 🚀
