# PowerShell script to fix database schema issues
# This script adds missing columns to QuerySuggestions table and fixes decimal precision

param(
    [string]$ConnectionString = $null
)

# Function to execute SQL command
function Execute-SqlCommand {
    param(
        [string]$ConnectionString,
        [string]$SqlCommand
    )
    
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
        $connection.Open()
        
        $command = New-Object System.Data.SqlClient.SqlCommand($SqlCommand, $connection)
        $command.CommandTimeout = 60
        
        $result = $command.ExecuteNonQuery()
        Write-Host "✓ Executed successfully. Rows affected: $result" -ForegroundColor Green
        
        $connection.Close()
        return $true
    }
    catch {
        Write-Host "✗ Error: $($_.Exception.Message)" -ForegroundColor Red
        if ($connection.State -eq 'Open') {
            $connection.Close()
        }
        return $false
    }
}

# Function to check if column exists
function Test-ColumnExists {
    param(
        [string]$ConnectionString,
        [string]$TableName,
        [string]$ColumnName
    )

    $sql = "SELECT COUNT(*) FROM sys.columns WHERE object_id = OBJECT_ID('dbo.$TableName') AND name = '$ColumnName'"

    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
        $connection.Open()

        $command = New-Object System.Data.SqlClient.SqlCommand($sql, $connection)
        $result = $command.ExecuteScalar()

        $connection.Close()
        return $result -gt 0
    }
    catch {
        Write-Host "Error checking column existence: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
}

Write-Host "=== Database Schema Fix Script ===" -ForegroundColor Cyan
Write-Host "This script will fix missing columns in QuerySuggestions table" -ForegroundColor Yellow

# Get connection string from appsettings if not provided
if ([string]::IsNullOrEmpty($ConnectionString)) {
    Write-Host "Reading connection string from appsettings..." -ForegroundColor Yellow
    
    $appsettingsPath = "BIReportingCopilot.API/appsettings.json"
    if (Test-Path $appsettingsPath) {
        $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
        $ConnectionString = $appsettings.ConnectionStrings.DefaultConnection
        Write-Host "✓ Connection string loaded from appsettings.json" -ForegroundColor Green
    }
    else {
        Write-Host "✗ appsettings.json not found at: $appsettingsPath" -ForegroundColor Red
        exit 1
    }
}

if ([string]::IsNullOrEmpty($ConnectionString)) {
    Write-Host "✗ Connection string is empty" -ForegroundColor Red
    exit 1
}

Write-Host "Connection string: $($ConnectionString.Substring(0, [Math]::Min(50, $ConnectionString.Length)))..." -ForegroundColor Gray

# Test connection
Write-Host "`nTesting database connection..." -ForegroundColor Yellow
$testResult = Execute-SqlCommand -ConnectionString $ConnectionString -SqlCommand "SELECT 1"
if (-not $testResult) {
    Write-Host "✗ Failed to connect to database" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Database connection successful" -ForegroundColor Green

# Check if QuerySuggestions table exists
Write-Host "`nChecking if QuerySuggestions table exists..." -ForegroundColor Yellow
$tableExistsQuery = "SELECT COUNT(*) FROM sys.tables WHERE name = 'QuerySuggestions'"
try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $connection.Open()
    $command = New-Object System.Data.SqlClient.SqlCommand($tableExistsQuery, $connection)
    $tableExists = $command.ExecuteScalar() -gt 0
    $connection.Close()
    
    if (-not $tableExists) {
        Write-Host "✗ QuerySuggestions table does not exist. Please run the CreateQuerySuggestionsSystem.sql script first." -ForegroundColor Red
        exit 1
    }
    Write-Host "✓ QuerySuggestions table exists" -ForegroundColor Green
}
catch {
    Write-Host "✗ Error checking table existence: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Check and add Text column
Write-Host "`nChecking Text column..." -ForegroundColor Yellow
if (-not (Test-ColumnExists -ConnectionString $ConnectionString -TableName "QuerySuggestions" -ColumnName "Text")) {
    Write-Host "Adding Text column..." -ForegroundColor Yellow
    $addTextColumn = "ALTER TABLE [dbo].[QuerySuggestions] ADD [Text] NVARCHAR(500) NOT NULL DEFAULT ''; UPDATE [dbo].[QuerySuggestions] SET [Text] = [QueryText] WHERE [Text] = '';"
    Execute-SqlCommand -ConnectionString $ConnectionString -SqlCommand $addTextColumn
}
else {
    Write-Host "✓ Text column already exists" -ForegroundColor Green
}

# Check and add Relevance column
Write-Host "`nChecking Relevance column..." -ForegroundColor Yellow
if (-not (Test-ColumnExists -ConnectionString $ConnectionString -TableName "QuerySuggestions" -ColumnName "Relevance")) {
    Write-Host "Adding Relevance column..." -ForegroundColor Yellow
    $addRelevanceColumn = "ALTER TABLE [dbo].[QuerySuggestions] ADD [Relevance] DECIMAL(3,2) NOT NULL DEFAULT 0.8;"
    Execute-SqlCommand -ConnectionString $ConnectionString -SqlCommand $addRelevanceColumn
}
else {
    Write-Host "✓ Relevance column already exists" -ForegroundColor Green
}

# Fix PromptTemplates SuccessRate precision if table exists
Write-Host "`nChecking PromptTemplates table..." -ForegroundColor Yellow
$promptTableExistsQuery = "SELECT COUNT(*) FROM sys.tables WHERE name = 'PromptTemplates'"
try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $connection.Open()
    $command = New-Object System.Data.SqlClient.SqlCommand($promptTableExistsQuery, $connection)
    $promptTableExists = $command.ExecuteScalar() -gt 0
    $connection.Close()
    
    if ($promptTableExists) {
        Write-Host "✓ PromptTemplates table exists, checking SuccessRate precision..." -ForegroundColor Green
        
        # Check if SuccessRate column needs precision fix
        $checkPrecisionQuery = "SELECT COUNT(*) FROM sys.columns c JOIN sys.types t ON c.system_type_id = t.system_type_id WHERE c.object_id = OBJECT_ID('dbo.PromptTemplates') AND c.name = 'SuccessRate' AND (c.precision != 5 OR c.scale != 2)"
        
        $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
        $connection.Open()
        $command = New-Object System.Data.SqlClient.SqlCommand($checkPrecisionQuery, $connection)
        $needsPrecisionFix = $command.ExecuteScalar() -gt 0
        $connection.Close()
        
        if ($needsPrecisionFix) {
            Write-Host "Fixing SuccessRate column precision..." -ForegroundColor Yellow
            $fixPrecisionQuery = "ALTER TABLE [dbo].[PromptTemplates] ADD [SuccessRate_New] DECIMAL(5,2) NULL; UPDATE [dbo].[PromptTemplates] SET [SuccessRate_New] = CAST([SuccessRate] AS DECIMAL(5,2)); ALTER TABLE [dbo].[PromptTemplates] DROP COLUMN [SuccessRate]; EXEC sp_rename 'dbo.PromptTemplates.SuccessRate_New', 'SuccessRate', 'COLUMN';"
            Execute-SqlCommand -ConnectionString $ConnectionString -SqlCommand $fixPrecisionQuery
        }
        else {
            Write-Host "✓ SuccessRate column precision is correct" -ForegroundColor Green
        }
    }
    else {
        Write-Host "ℹ PromptTemplates table does not exist, skipping..." -ForegroundColor Gray
    }
}
catch {
    Write-Host "✗ Error checking PromptTemplates: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Schema Fix Complete ===" -ForegroundColor Cyan
Write-Host "✓ Database schema has been updated successfully!" -ForegroundColor Green
Write-Host "You can now restart your application." -ForegroundColor Yellow
