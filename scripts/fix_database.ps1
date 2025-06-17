# Fix Database - Create Missing QuerySuggestions Table
# Run this PowerShell script to fix the missing QuerySuggestions table

Write-Host "🔧 Fixing Database - Creating Missing QuerySuggestions Table" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green

# Database connection details
$ServerName = "(localdb)\mssqllocaldb"
$DatabaseName = "BIReportingCopilot_Dev"
$SqlScriptPath = "database/fix_query_suggestions_table.sql"

# Check if SQL script exists
if (-not (Test-Path $SqlScriptPath)) {
    Write-Host "❌ Error: SQL script not found at $SqlScriptPath" -ForegroundColor Red
    Write-Host "Please make sure you're running this from the project root directory." -ForegroundColor Yellow
    exit 1
}

Write-Host "📁 SQL Script: $SqlScriptPath" -ForegroundColor Cyan
Write-Host "🗄️  Server: $ServerName" -ForegroundColor Cyan
Write-Host "💾 Database: $DatabaseName" -ForegroundColor Cyan
Write-Host ""

try {
    Write-Host "🚀 Executing SQL script..." -ForegroundColor Yellow
    
    # Execute the SQL script using sqlcmd
    $result = sqlcmd -S $ServerName -d $DatabaseName -i $SqlScriptPath -b
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ Database fix completed successfully!" -ForegroundColor Green
        Write-Host ""
        Write-Host "📋 What was fixed:" -ForegroundColor Cyan
        Write-Host "   • Created QuerySuggestions table" -ForegroundColor White
        Write-Host "   • Created SuggestionCategories table" -ForegroundColor White
        Write-Host "   • Added foreign key constraints" -ForegroundColor White
        Write-Host "   • Inserted default categories" -ForegroundColor White
        Write-Host "   • Inserted sample query suggestions" -ForegroundColor White
        Write-Host ""
        Write-Host "🎉 Your application should now work without the 500 errors!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Next steps:" -ForegroundColor Yellow
        Write-Host "1. Refresh your browser (Ctrl+F5)" -ForegroundColor White
        Write-Host "2. The QuerySuggestions API should now work" -ForegroundColor White
        Write-Host "3. No more 'Invalid object name QuerySuggestions' errors" -ForegroundColor White
    } else {
        Write-Host "❌ Error executing SQL script" -ForegroundColor Red
        Write-Host "Exit code: $LASTEXITCODE" -ForegroundColor Red
        if ($result) {
            Write-Host "Output: $result" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Troubleshooting:" -ForegroundColor Yellow
    Write-Host "1. Make sure SQL Server LocalDB is running" -ForegroundColor White
    Write-Host "2. Check if the database 'BIReportingCopilot_Dev' exists" -ForegroundColor White
    Write-Host "3. Verify you have permissions to modify the database" -ForegroundColor White
    Write-Host ""
    Write-Host "Alternative: Run the SQL script manually in SQL Server Management Studio" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "=================================================" -ForegroundColor Green
