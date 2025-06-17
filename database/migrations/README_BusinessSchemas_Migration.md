# BusinessSchemas Table Migration

## Overview

This migration adds missing columns to the `BusinessSchemas` table to resolve runtime errors related to schema management functionality in the BIReportingCopilot system.

## Problem

The application code expects the `BusinessSchemas` table to have the following columns that are currently missing:
- `IsActive` (BIT, NOT NULL)
- `IsDefault` (BIT, NOT NULL)

This results in runtime errors like:
```
Invalid column name 'IsActive'
Invalid column name 'IsDefault'
```

## Solution

This migration adds the missing columns with appropriate defaults and constraints.

## Files Included

### Migration Scripts
- **`Add_BusinessSchemas_IsActive_IsDefault_Columns.sql`** - Main migration script
- **`Check_BusinessSchemas_Structure.sql`** - Script to check current table structure

### Execution Scripts
- **`Run_BusinessSchemas_Migration.ps1`** - PowerShell script to run the migration
- **`Run_BusinessSchemas_Migration.bat`** - Batch script alternative

## How to Run

### Option 1: PowerShell Script (Recommended)

```powershell
# Navigate to the migrations directory
cd c:\dev\ReportAIng\database\migrations

# Run with default settings (localhost, BIReportingCopilot_Dev)
.\Run_BusinessSchemas_Migration.ps1

# Run with custom server/database
.\Run_BusinessSchemas_Migration.ps1 -ServerName "YourServer" -DatabaseName "YourDatabase"

# Preview what would be done without making changes
.\Run_BusinessSchemas_Migration.ps1 -WhatIf
```

### Option 2: Batch Script

```cmd
cd c:\dev\ReportAIng\database\migrations
Run_BusinessSchemas_Migration.bat
```

### Option 3: Manual SQL Execution

1. Open SQL Server Management Studio
2. Connect to your database server
3. Open `Add_BusinessSchemas_IsActive_IsDefault_Columns.sql`
4. Ensure you're connected to the correct database (`BIReportingCopilot_Dev`)
5. Execute the script

## What the Migration Does

1. **Checks Prerequisites**: Verifies that the `BusinessSchemas` table exists
2. **Adds Missing Columns**:
   - `IsActive BIT NOT NULL DEFAULT(1)` - Indicates if the schema is active
   - `IsDefault BIT NOT NULL DEFAULT(0)` - Indicates if this is the default schema
3. **Updates Existing Data**: Sets appropriate defaults for existing records
4. **Ensures Data Integrity**: Makes sure only one schema is marked as default
5. **Provides Feedback**: Shows the updated table structure

## Column Details

### IsActive Column
- **Type**: BIT (Boolean)
- **Nullable**: NOT NULL
- **Default**: 1 (True)
- **Purpose**: Controls whether the schema is active and available for use

### IsDefault Column
- **Type**: BIT (Boolean)
- **Nullable**: NOT NULL
- **Default**: 0 (False)
- **Purpose**: Indicates which schema is the system default

## Safety Features

- **Transaction-based**: All changes are wrapped in a transaction
- **Idempotent**: Safe to run multiple times - checks if columns already exist
- **Error Handling**: Comprehensive error handling with rollback on failure
- **Validation**: Checks table existence before attempting modifications

## After Migration

1. **Restart the API** if it's currently running
2. **Test schema operations** to ensure they work correctly
3. **Verify the fix** by checking that the runtime errors are resolved

## Rollback

If you need to rollback this migration, you can remove the columns:

```sql
USE [BIReportingCopilot_Dev]
GO

BEGIN TRANSACTION;
BEGIN TRY
    ALTER TABLE BusinessSchemas DROP COLUMN IsActive;
    ALTER TABLE BusinessSchemas DROP COLUMN IsDefault;
    COMMIT TRANSACTION;
    PRINT 'Rollback completed successfully';
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    THROW;
END CATCH
```

## Support

If you encounter issues:

1. Check that SQL Server is running
2. Verify database connection and permissions
3. Ensure the `BusinessSchemas` table exists
4. Review the error messages in the output
5. Check SQL Server logs for detailed error information

## Notes

- This migration is specifically for the `BIReportingCopilot_Dev` database
- The script includes comprehensive logging and error handling
- Existing data will not be lost or modified beyond adding the new columns
- The migration maintains referential integrity and data consistency
