# PowerShell script to run the SemanticCacheEntries fix migration
# Usage: .\RunSemanticCacheFixMigration.ps1 -ConnectionString "your_connection_string"

param(
    [Parameter(Mandatory=$false)]
    [string]$ConnectionString = "Server=localhost;Database=BIReportingCopilot_Dev;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=true;"
)

Write-Host "Running SemanticCacheEntries Fix Migration..." -ForegroundColor Green

try {
    # Get the SQL script path
    $scriptPath = Join-Path $PSScriptRoot "FixSemanticCacheEntriesColumns.sql"
    
    if (-not (Test-Path $scriptPath)) {
        throw "Migration script not found at: $scriptPath"
    }
    
    Write-Host "Reading migration script from: $scriptPath" -ForegroundColor Yellow
    
    # Read the SQL script
    $sqlScript = Get-Content $scriptPath -Raw
    
    Write-Host "Connecting to database..." -ForegroundColor Yellow
    Write-Host "Connection: $ConnectionString" -ForegroundColor Gray
    
    # Check if SqlServer module is available
    if (-not (Get-Module -ListAvailable -Name SqlServer)) {
        Write-Host "SqlServer PowerShell module not found. Installing..." -ForegroundColor Yellow
        Install-Module -Name SqlServer -Force -AllowClobber -Scope CurrentUser
    }
    
    # Import SqlServer module
    Import-Module SqlServer -Force
    
    # Execute the SQL script
    Write-Host "Executing migration script..." -ForegroundColor Yellow
    Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $sqlScript -Verbose

    Write-Host "SemanticCacheEntries fix migration completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Changes applied:" -ForegroundColor Cyan
    Write-Host "  - Fixed ID column type from int to bigint" -ForegroundColor White
    Write-Host "  - Added CreatedBy column" -ForegroundColor White
    Write-Host "  - Added UpdatedBy column" -ForegroundColor White
    Write-Host "  - Added CreatedDate column" -ForegroundColor White
    Write-Host "  - Added LastUpdated column" -ForegroundColor White
    Write-Host "  - Added IsActive column" -ForegroundColor White
    Write-Host "  - Created performance indexes" -ForegroundColor White
    Write-Host ""
    Write-Host "SemanticCacheEntries table is now compatible with Entity Framework!" -ForegroundColor Green
    
} catch {
    Write-Host "❌ Migration failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "Troubleshooting tips:" -ForegroundColor Yellow
    Write-Host "  - Make sure SQL Server is running" -ForegroundColor White
    Write-Host "  - Verify the connection string is correct" -ForegroundColor White
    Write-Host "  - Check if you have permissions to modify the database" -ForegroundColor White
    Write-Host "  - Ensure no applications are currently using the SemanticCacheEntries table" -ForegroundColor White
    exit 1
}
