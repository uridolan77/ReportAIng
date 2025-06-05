# PowerShell script to run the Schema Management migration
# Usage: .\RunMigration.ps1 -ConnectionString "your_connection_string"

param(
    [Parameter(Mandatory=$true)]
    [string]$ConnectionString
)

Write-Host "🚀 Running Schema Management Migration..." -ForegroundColor Green

try {
    # Get the SQL script path
    $scriptPath = Join-Path $PSScriptRoot "AddBusinessSchemaManagement.sql"
    
    if (-not (Test-Path $scriptPath)) {
        throw "Migration script not found at: $scriptPath"
    }
    
    Write-Host "📄 Reading migration script from: $scriptPath" -ForegroundColor Yellow
    
    # Read the SQL script
    $sqlScript = Get-Content $scriptPath -Raw
    
    Write-Host "🔗 Connecting to database..." -ForegroundColor Yellow
    
    # Execute the SQL script
    Invoke-Sqlcmd -ConnectionString $ConnectionString -Query $sqlScript -Verbose
    
    Write-Host "✅ Schema Management migration completed successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Created tables:" -ForegroundColor Cyan
    Write-Host "  • BusinessSchemas" -ForegroundColor White
    Write-Host "  • BusinessSchemaVersions" -ForegroundColor White
    Write-Host "  • SchemaTableContexts" -ForegroundColor White
    Write-Host "  • SchemaColumnContexts" -ForegroundColor White
    Write-Host "  • SchemaGlossaryTerms" -ForegroundColor White
    Write-Host "  • SchemaRelationships" -ForegroundColor White
    Write-Host "  • UserSchemaPreferences" -ForegroundColor White
    Write-Host ""
    Write-Host "🎯 Default schema created and ready for use!" -ForegroundColor Green
    
} catch {
    Write-Host "❌ Migration failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
