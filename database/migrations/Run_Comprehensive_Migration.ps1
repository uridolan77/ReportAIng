# PowerShell script to run the comprehensive BusinessSchema tables migration
# This script adds the missing IsActive and IsDefault columns to ALL related tables

param(
    [string]$ServerName = "localhost",
    [string]$DatabaseName = "BIReportingCopilot_Dev",
    [switch]$WhatIf = $false
)

$ErrorActionPreference = "Stop"

# Define paths
$ScriptRoot = $PSScriptRoot
$MigrationScript = Join-Path $ScriptRoot "Fix_All_BusinessSchema_Tables.sql"

Write-Host "🔧 COMPREHENSIVE BusinessSchema Tables Migration" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green
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
Write-Host "This migration will add IsActive and IsDefault columns to:" -ForegroundColor Cyan
Write-Host "  • SchemaTableContexts" -ForegroundColor Cyan
Write-Host "  • SchemaColumnContexts" -ForegroundColor Cyan
Write-Host "  • BusinessSchemas (verification)" -ForegroundColor Cyan
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
        Get-Content $MigrationScript | Select-Object -First 30
        Write-Host "... (script continues)" -ForegroundColor Gray
    }
    else {
        Write-Host "🚀 Running comprehensive migration..." -ForegroundColor Blue
        Write-Host ""
        
        # Execute the migration
        sqlcmd -S $ServerName -d $DatabaseName -E -i $MigrationScript
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "🎉 MIGRATION COMPLETED SUCCESSFULLY!" -ForegroundColor Green
            Write-Host ""
            Write-Host "✅ All BusinessSchema-related tables now have required columns:" -ForegroundColor Yellow
            Write-Host "   • SchemaTableContexts: IsActive, IsDefault" -ForegroundColor Yellow
            Write-Host "   • SchemaColumnContexts: IsActive, IsDefault" -ForegroundColor Yellow
            Write-Host "   • BusinessSchemas: IsActive, IsDefault" -ForegroundColor Yellow
            Write-Host ""
            Write-Host "🔄 NEXT STEPS:" -ForegroundColor Cyan
            Write-Host "1. Restart your API application" -ForegroundColor White
            Write-Host "2. Test BusinessSchema operations" -ForegroundColor White
            Write-Host "3. Verify no more 'Invalid column name' errors" -ForegroundColor White
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
Write-Host "Migration process completed." -ForegroundColor Green
