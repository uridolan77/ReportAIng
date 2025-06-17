# PowerShell script to run the BusinessSchemas migration
# This script adds the missing IsActive and IsDefault columns to the BusinessSchemas table

param(
    [string]$ServerName = "localhost",
    [string]$DatabaseName = "BIReportingCopilot_Dev",
    [switch]$WhatIf = $false
)

$ErrorActionPreference = "Stop"

# Define paths
$ScriptRoot = $PSScriptRoot
$MigrationScript = Join-Path $ScriptRoot "Add_BusinessSchemas_IsActive_IsDefault_Columns.sql"
$CheckScript = Join-Path $ScriptRoot "Check_BusinessSchemas_Structure.sql"

Write-Host "BusinessSchemas Migration Script" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host ""

# Check if migration script exists
if (-not (Test-Path $MigrationScript)) {
    Write-Error "Migration script not found at: $MigrationScript"
    exit 1
}

# Display connection info
Write-Host "Server: $ServerName" -ForegroundColor Yellow
Write-Host "Database: $DatabaseName" -ForegroundColor Yellow
Write-Host ""

if ($WhatIf) {
    Write-Host "WHAT-IF MODE: This will show what the script would do without making changes" -ForegroundColor Cyan
    Write-Host ""
}

try {
    # Test SQL connection
    Write-Host "Testing database connection..." -ForegroundColor Blue
    
    $connectionString = "Server=$ServerName;Database=$DatabaseName;Integrated Security=true;TrustServerCertificate=true;"
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    $connection.Close()
    
    Write-Host "✓ Database connection successful" -ForegroundColor Green
    Write-Host ""

    if ($WhatIf) {
        Write-Host "Would execute migration script: $MigrationScript" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "Script content preview:" -ForegroundColor Cyan
        Get-Content $MigrationScript | Select-Object -First 20
        Write-Host "... (truncated)" -ForegroundColor Gray
    }
    else {
        # Run the check script first
        Write-Host "Checking current table structure..." -ForegroundColor Blue
        if (Test-Path $CheckScript) {
            sqlcmd -S $ServerName -d $DatabaseName -E -i $CheckScript
        }
        
        Write-Host ""
        Write-Host "Running migration script..." -ForegroundColor Blue
        
        # Execute the migration
        sqlcmd -S $ServerName -d $DatabaseName -E -i $MigrationScript
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "✓ Migration completed successfully!" -ForegroundColor Green
            Write-Host ""
            Write-Host "The following columns have been added to BusinessSchemas table:" -ForegroundColor Yellow
            Write-Host "  - IsActive (BIT, NOT NULL, DEFAULT 1)" -ForegroundColor Yellow
            Write-Host "  - IsDefault (BIT, NOT NULL, DEFAULT 0)" -ForegroundColor Yellow
        }
        else {
            Write-Error "Migration script failed with exit code: $LASTEXITCODE"
        }
    }
}
catch {
    Write-Error "Error executing migration: $($_.Exception.Message)"
    exit 1
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Green
Write-Host "1. Test your application to ensure the runtime error is resolved" -ForegroundColor White
Write-Host "2. Restart your API if it's currently running" -ForegroundColor White
Write-Host "3. Verify that BusinessSchemas operations work correctly" -ForegroundColor White
