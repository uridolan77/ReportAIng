# AI Transparency Foundation - Database Setup Guide

## Overview

This directory contains the complete database schema and migration scripts for the AI Transparency Foundation implementation. The foundation provides comprehensive transparency, token management, and intelligent agent orchestration capabilities for the BI Reporting Copilot.

## Files Included

### 1. `AI_Transparency_Foundation_Complete_Schema.sql`
**Complete database schema with sample data**
- Creates all 12 transparency foundation tables
- Includes proper indexes, constraints, and foreign keys
- Seeds sample data for testing and development
- Safe to run multiple times (idempotent)

### 2. `Migration_AI_Transparency_Foundation.sql`
**Production-safe migration script**
- Creates core transparency tables only
- Minimal, focused migration for production environments
- Includes detailed logging and progress tracking
- Safe to run in production with minimal downtime

### 3. `README_AI_Transparency_Foundation.md`
**This documentation file**

## Database Tables Created

### Core Transparency Tables
1. **PromptConstructionTraces** - Track AI prompt building process
2. **PromptConstructionSteps** - Detailed steps in prompt construction
3. **TokenBudgets** - Token allocation and cost management
4. **TokenUsageAnalytics** - Daily usage analytics and trends

### Business Intelligence Tables
5. **BusinessContextProfiles** - Business intent and context analysis
6. **BusinessEntities** - Extracted business entities from queries

### Agent Orchestration Tables
7. **AgentOrchestrationSessions** - Multi-agent coordination tracking
8. **AgentExecutionResults** - Individual agent execution results

### Streaming Tables
9. **StreamingSessions** - Real-time streaming session management
10. **StreamingChunks** - Individual streaming data chunks

### Analytics Tables
11. **TransparencyReports** - Generated transparency reports
12. **OptimizationSuggestions** - AI optimization recommendations

## Installation Instructions

### Option 1: Complete Setup (Recommended for Development)

```sql
-- Run the complete schema script
-- This creates all tables and seeds sample data
sqlcmd -S your_server -d BIReportingCopilot_dev -i AI_Transparency_Foundation_Complete_Schema.sql
```

### Option 2: Production Migration (Recommended for Production)

```sql
-- Run the minimal migration script first
-- This creates core tables only
sqlcmd -S your_server -d BIReportingCopilot_dev -i Migration_AI_Transparency_Foundation.sql

-- Then optionally run the complete script to add remaining tables
sqlcmd -S your_server -d BIReportingCopilot_dev -i AI_Transparency_Foundation_Complete_Schema.sql
```

### Option 3: Manual Execution

1. Open SQL Server Management Studio (SSMS)
2. Connect to your database server
3. Select the `BIReportingCopilot_dev` database
4. Open and execute the desired script file

## Verification

After running the scripts, verify the installation:

```sql
-- Check that all tables were created
SELECT 
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_NAME IN (
    'PromptConstructionTraces',
    'PromptConstructionSteps', 
    'TokenBudgets',
    'TokenUsageAnalytics',
    'BusinessContextProfiles',
    'BusinessEntities',
    'AgentOrchestrationSessions',
    'AgentExecutionResults',
    'StreamingSessions',
    'StreamingChunks',
    'TransparencyReports',
    'OptimizationSuggestions'
)
ORDER BY TABLE_NAME;

-- Check sample data (if complete schema was run)
SELECT COUNT(*) as SampleTraces FROM PromptConstructionTraces;
SELECT COUNT(*) as SampleBudgets FROM TokenBudgets;
SELECT COUNT(*) as SampleProfiles FROM BusinessContextProfiles;
```

## Sample Data Included

The complete schema script includes realistic sample data:

- **5 Prompt Construction Traces** with detailed steps
- **5 Token Budget Allocations** with actual usage data
- **5 Business Context Profiles** with extracted entities
- **6 Daily Usage Analytics** records showing trends
- **5 Optimization Suggestions** for performance improvements

## Integration with Application

After running the database scripts:

1. **Restart the application** to register new services
2. **Test transparency features** through the enhanced query endpoint:
   ```
   POST /api/query/enhanced
   ```
3. **Monitor transparency data** through the new endpoints:
   ```
   GET /api/transparency/reports
   GET /api/transparency/metrics
   ```

## Performance Considerations

### Indexes Created
- All primary keys have clustered indexes
- Foreign keys have non-clustered indexes
- Frequently queried columns (UserId, CreatedAt, etc.) have indexes
- Composite indexes for complex queries

### Maintenance
- Consider implementing data retention policies for high-volume tables
- Monitor index fragmentation on heavily used tables
- Set up regular statistics updates for optimal query performance

## Security Considerations

- All tables use NVARCHAR(450) for user identifiers (compatible with ASP.NET Identity)
- JSON columns store metadata and complex objects
- Foreign key constraints ensure data integrity
- No sensitive data is stored in plain text

## Troubleshooting

### Common Issues

1. **Permission Errors**
   ```
   -- Ensure the user has CREATE TABLE permissions
   GRANT CREATE TABLE TO [your_user];
   ```

2. **Foreign Key Constraint Errors**
   ```
   -- Check if parent tables exist before creating child tables
   -- The scripts handle this automatically
   ```

3. **Index Creation Failures**
   ```
   -- Ensure sufficient disk space for index creation
   -- Check for existing indexes with same names
   ```

### Rollback (if needed)

```sql
-- To remove all transparency tables (USE WITH CAUTION)
DROP TABLE IF EXISTS [dbo].[PromptConstructionSteps];
DROP TABLE IF EXISTS [dbo].[PromptConstructionTraces];
DROP TABLE IF EXISTS [dbo].[TokenBudgets];
DROP TABLE IF EXISTS [dbo].[TokenUsageAnalytics];
DROP TABLE IF EXISTS [dbo].[BusinessEntities];
DROP TABLE IF EXISTS [dbo].[BusinessContextProfiles];
DROP TABLE IF EXISTS [dbo].[AgentExecutionResults];
DROP TABLE IF EXISTS [dbo].[AgentOrchestrationSessions];
DROP TABLE IF EXISTS [dbo].[StreamingChunks];
DROP TABLE IF EXISTS [dbo].[StreamingSessions];
DROP TABLE IF EXISTS [dbo].[TransparencyReports];
DROP TABLE IF EXISTS [dbo].[OptimizationSuggestions];
```

## Support

For issues or questions:
1. Check the application logs for detailed error messages
2. Verify database connectivity and permissions
3. Ensure all prerequisite tables exist
4. Review the transparency service registration in the application startup

## Version History

- **v1.0** - Initial AI Transparency Foundation implementation
- Complete schema with 12 tables
- Sample data for development and testing
- Production-ready migration scripts
