# BusinessSchemas Runtime Error - Diagnosis and Solutions

## Problem Summary

You're experiencing a runtime error: **"Invalid column name 'IsActive'"** when accessing BusinessSchemas, despite the columns existing in the database.

## Root Cause Analysis

Based on the investigation, the issue is **NOT** missing database columns. The columns `IsActive` and `IsDefault` exist in your `BIReportingCopilot_Dev` database, as confirmed by your schema dump.

### Potential Causes

1. **Database Context Confusion**: Your application has multiple database contexts (`BICopilotContext` and `SchemaDbContext`) that both define `BusinessSchemas`
2. **Connection String Issues**: The application might be connecting to the wrong database or using in-memory database
3. **Environment Configuration**: The application might not be running in Development mode
4. **Entity Framework Cache**: Stale model cache from previous migrations

## Current Configuration

- **Production Connection**: `BIReportingCopilot` (appsettings.json)
- **Development Connection**: `BIReportingCopilot_Dev` (appsettings.Development.json) ✅
- **Schema Location**: Columns exist in `BIReportingCopilot_Dev` ✅

## Solutions

### Solution 1: Verify Database Connection (Immediate)

1. **Run the diagnostic API endpoint** I created:
   ```
   GET /api/diagnostic/database-info
   GET /api/diagnostic/business-schemas-test
   ```

2. **Check the application environment**:
   - Ensure `ASPNETCORE_ENVIRONMENT=Development`
   - Verify the app is using the correct appsettings file

### Solution 2: Clear Entity Framework Cache

```powershell
# Stop the application completely
# Delete Entity Framework cache and compiled files
Remove-Item -Recurse -Force "c:\dev\ReportAIng\backend\BIReportingCopilot.API\bin"
Remove-Item -Recurse -Force "c:\dev\ReportAIng\backend\BIReportingCopilot.API\obj"
Remove-Item -Recurse -Force "c:\dev\ReportAIng\backend\BIReportingCopilot.Infrastructure\bin"
Remove-Item -Recurse -Force "c:\dev\ReportAIng\backend\BIReportingCopilot.Infrastructure\obj"

# Rebuild the solution
dotnet clean BIReportingCopilot.sln
dotnet build BIReportingCopilot.sln
```

### Solution 3: Force Database Recreation (If Needed)

If the above doesn't work, create the missing columns in the **production** database:

```sql
USE [BIReportingCopilot]  -- Note: Production database
GO

-- Add missing columns if they don't exist
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'BusinessSchemas' AND COLUMN_NAME = 'IsActive')
BEGIN
    ALTER TABLE BusinessSchemas ADD IsActive BIT NOT NULL DEFAULT(1);
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'BusinessSchemas' AND COLUMN_NAME = 'IsDefault')
BEGIN
    ALTER TABLE BusinessSchemas ADD IsDefault BIT NOT NULL DEFAULT(0);
END
```

### Solution 4: Verify Environment Variables

Add this to your `launchSettings.json` or environment:

```json
{
  "environmentVariables": {
    "ASPNETCORE_ENVIRONMENT": "Development"
  }
}
```

### Solution 5: Database Context Consolidation

If the issue persists, consider using only one context for BusinessSchemas. Check which service registrations use which context.

## Debugging Steps

1. **Check current database connection**:
   ```csharp
   // Add this temporarily to your controller
   var dbName = _context.Database.GetDbConnection().Database;
   var connectionString = _context.Database.GetDbConnection().ConnectionString;
   ```

2. **Verify table structure at runtime**:
   ```sql
   SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS 
   WHERE TABLE_NAME = 'BusinessSchemas'
   ```

3. **Check Entity Framework provider**:
   ```csharp
   var provider = _context.Database.ProviderName;
   // Should be "Microsoft.EntityFrameworkCore.SqlServer"
   // NOT "Microsoft.EntityFrameworkCore.InMemory"
   ```

## Files Created for Diagnosis

- `c:\dev\ReportAIng\database\diagnostics\Test_BusinessSchemas_Connection.sql`
- `c:\dev\ReportAIng\backend\BIReportingCopilot.API\Controllers\DiagnosticController.cs`

## Next Steps

1. **Test the diagnostic endpoints** to identify the exact cause
2. **Verify environment configuration** (Development vs Production)
3. **Clear EF cache** and rebuild
4. **Check database connectivity** and context usage

The columns exist in your database, so this is a configuration or caching issue, not a schema problem.
