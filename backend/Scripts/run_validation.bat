@echo off
echo =====================================================
echo Database Column Validation
echo =====================================================
echo.
echo This script will help you validate BusinessColumnInfo against remote tables
echo.
echo You need to run this in two steps:
echo 1. Connect to REMOTE database (185.64.56.157 - DailyActionsDB)
echo 2. Connect to LOCAL database (BIReportingCopilot_Dev)
echo.

set /p choice="Do you want to run PowerShell validation (P) or manual SQL validation (M)? "

if /i "%choice%"=="P" goto powershell
if /i "%choice%"=="M" goto manual
goto end

:powershell
echo.
echo Running PowerShell validation...
echo.
set /p password="Enter password for ReportsUser: "
powershell -ExecutionPolicy Bypass -File "Validate-DatabaseColumns.ps1" -RemotePassword "%password%"
goto end

:manual
echo.
echo Manual SQL Validation Steps:
echo.
echo STEP 1: Connect to REMOTE database
echo ===================================
echo Server: 185.64.56.157
echo Database: DailyActionsDB
echo User: ReportsUser
echo.
echo Run this command:
echo sqlcmd -S 185.64.56.157 -d DailyActionsDB -U ReportsUser -P [password] -i manual_validation.sql -o remote_results.txt
echo.
echo STEP 2: Connect to LOCAL database  
echo ==================================
echo Server: (localdb)\MSSQLLocalDB
echo Database: BIReportingCopilot_Dev
echo.
echo Run this command:
echo sqlcmd -S "(localdb)\MSSQLLocalDB" -d BIReportingCopilot_Dev -i manual_validation.sql -o local_results.txt
echo.
echo STEP 3: Compare Results
echo =======================
echo Compare remote_results.txt and local_results.txt to find:
echo - Missing columns (in remote but not in BusinessColumnInfo)
echo - Extra columns (in BusinessColumnInfo but not in remote)
echo.
pause

:end
echo.
echo Validation complete. Check the output files for results.
pause
