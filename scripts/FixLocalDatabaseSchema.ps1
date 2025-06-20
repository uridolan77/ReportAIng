# PowerShell script to fix local database schema issues
# This script adds missing columns to QuerySuggestions table in BIReportingCopilot_Dev

Write-Host "=== Local Database Schema Fix Script ===" -ForegroundColor Cyan
Write-Host "Targeting: BIReportingCopilot_Dev database on localhost" -ForegroundColor Yellow

# Connection string for local database
$ConnectionString = "Server=localhost;Database=BIReportingCopilot_Dev;Integrated Security=true;TrustServerCertificate=true;"

Write-Host "Connection string: $ConnectionString" -ForegroundColor Gray

# Function to execute SQL command
function Execute-SqlCommand {
    param(
        [string]$ConnectionString,
        [string]$SqlCommand,
        [string]$Description = ""
    )
    
    try {
        Add-Type -AssemblyName "System.Data"
        $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
        $connection.Open()
        
        $command = New-Object System.Data.SqlClient.SqlCommand($SqlCommand, $connection)
        $command.CommandTimeout = 60
        
        $result = $command.ExecuteNonQuery()
        Write-Host "✓ $Description - Rows affected: $result" -ForegroundColor Green
        
        $connection.Close()
        return $true
    }
    catch {
        Write-Host "✗ $Description - Error: $($_.Exception.Message)" -ForegroundColor Red
        if ($connection -and $connection.State -eq 'Open') {
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
        Add-Type -AssemblyName "System.Data"
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

# Test connection
Write-Host "`nTesting database connection..." -ForegroundColor Yellow
$testResult = Execute-SqlCommand -ConnectionString $ConnectionString -SqlCommand "SELECT 1" -Description "Connection test"
if (-not $testResult) {
    Write-Host "✗ Failed to connect to database" -ForegroundColor Red
    Write-Host "Make sure SQL Server is running and BIReportingCopilot_Dev database exists" -ForegroundColor Yellow
    exit 1
}

# Check if QuerySuggestions table exists
Write-Host "`nChecking if QuerySuggestions table exists..." -ForegroundColor Yellow
$tableExistsQuery = "SELECT COUNT(*) FROM sys.tables WHERE name = 'QuerySuggestions'"
try {
    Add-Type -AssemblyName "System.Data"
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $connection.Open()
    $command = New-Object System.Data.SqlClient.SqlCommand($tableExistsQuery, $connection)
    $tableExists = $command.ExecuteScalar() -gt 0
    $connection.Close()
    
    if (-not $tableExists) {
        Write-Host "✗ QuerySuggestions table does not exist." -ForegroundColor Red
        Write-Host "Please run Entity Framework migrations first: dotnet ef database update" -ForegroundColor Yellow
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
    $addTextColumn = "ALTER TABLE [dbo].[QuerySuggestions] ADD [Text] NVARCHAR(500) NOT NULL DEFAULT ''"
    $updateTextColumn = "UPDATE [dbo].[QuerySuggestions] SET [Text] = [QueryText] WHERE [Text] = ''"
    
    Execute-SqlCommand -ConnectionString $ConnectionString -SqlCommand $addTextColumn -Description "Add Text column"
    Execute-SqlCommand -ConnectionString $ConnectionString -SqlCommand $updateTextColumn -Description "Populate Text column"
}
else {
    Write-Host "✓ Text column already exists" -ForegroundColor Green
}

# Check and add Relevance column
Write-Host "`nChecking Relevance column..." -ForegroundColor Yellow
if (-not (Test-ColumnExists -ConnectionString $ConnectionString -TableName "QuerySuggestions" -ColumnName "Relevance")) {
    Write-Host "Adding Relevance column..." -ForegroundColor Yellow
    $addRelevanceColumn = "ALTER TABLE [dbo].[QuerySuggestions] ADD [Relevance] DECIMAL(3,2) NOT NULL DEFAULT 0.8"
    Execute-SqlCommand -ConnectionString $ConnectionString -SqlCommand $addRelevanceColumn -Description "Add Relevance column"
}
else {
    Write-Host "✓ Relevance column already exists" -ForegroundColor Green
}

# Verify the changes
Write-Host "`nVerifying schema changes..." -ForegroundColor Yellow
$verifyQuery = @"
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length,
    c.precision,
    c.scale,
    c.is_nullable
FROM sys.columns c
JOIN sys.types t ON c.system_type_id = t.system_type_id
WHERE c.object_id = OBJECT_ID('dbo.QuerySuggestions')
AND c.name IN ('Text', 'Relevance', 'QueryText')
ORDER BY c.name
"@

try {
    Add-Type -AssemblyName "System.Data"
    $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
    $connection.Open()
    
    $command = New-Object System.Data.SqlClient.SqlCommand($verifyQuery, $connection)
    $reader = $command.ExecuteReader()
    
    Write-Host "`nColumn verification results:" -ForegroundColor Cyan
    Write-Host "ColumnName`tDataType`tMaxLength`tPrecision`tScale`tNullable" -ForegroundColor White
    Write-Host "----------`t--------`t---------`t---------`t-----`t--------" -ForegroundColor White
    
    while ($reader.Read()) {
        $columnName = $reader["ColumnName"]
        $dataType = $reader["DataType"]
        $maxLength = $reader["max_length"]
        $precision = $reader["precision"]
        $scale = $reader["scale"]
        $nullable = $reader["is_nullable"]
        
        Write-Host "$columnName`t`t$dataType`t`t$maxLength`t`t$precision`t`t$scale`t$nullable" -ForegroundColor Gray
    }
    
    $reader.Close()
    $connection.Close()
}
catch {
    Write-Host "Error verifying changes: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== Schema Fix Complete ===" -ForegroundColor Cyan
Write-Host "✓ Local database schema has been updated successfully!" -ForegroundColor Green
Write-Host "You can now restart your application." -ForegroundColor Yellow
