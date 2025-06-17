@echo off
REM Batch script to run the BusinessSchemas migration
REM This script adds the missing IsActive and IsDefault columns to the BusinessSchemas table

setlocal

set SERVER_NAME=localhost
set DATABASE_NAME=BIReportingCopilot_Dev
set SCRIPT_DIR=%~dp0
set MIGRATION_SCRIPT=%SCRIPT_DIR%Add_BusinessSchemas_IsActive_IsDefault_Columns.sql

echo BusinessSchemas Migration Script
echo =================================
echo.

REM Check if migration script exists
if not exist "%MIGRATION_SCRIPT%" (
    echo ERROR: Migration script not found at: %MIGRATION_SCRIPT%
    pause
    exit /b 1
)

echo Server: %SERVER_NAME%
echo Database: %DATABASE_NAME%
echo.

echo Testing database connection...
sqlcmd -S %SERVER_NAME% -d %DATABASE_NAME% -E -Q "SELECT 1" >nul 2>&1
if %ERRORLEVEL% neq 0 (
    echo ERROR: Cannot connect to database %DATABASE_NAME% on server %SERVER_NAME%
    echo Please check:
    echo   - SQL Server is running
    echo   - Database exists
    echo   - You have proper permissions
    pause
    exit /b 1
)

echo ✓ Database connection successful
echo.

echo Running migration script...
sqlcmd -S %SERVER_NAME% -d %DATABASE_NAME% -E -i "%MIGRATION_SCRIPT%"

if %ERRORLEVEL% equ 0 (
    echo.
    echo ✓ Migration completed successfully!
    echo.
    echo The following columns have been added to BusinessSchemas table:
    echo   - IsActive ^(BIT, NOT NULL, DEFAULT 1^)
    echo   - IsDefault ^(BIT, NOT NULL, DEFAULT 0^)
    echo.
    echo Next steps:
    echo 1. Test your application to ensure the runtime error is resolved
    echo 2. Restart your API if it's currently running
    echo 3. Verify that BusinessSchemas operations work correctly
) else (
    echo.
    echo ERROR: Migration script failed with exit code: %ERRORLEVEL%
)

echo.
pause
