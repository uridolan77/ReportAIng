# =====================================================
# Database Column Validation Script
# =====================================================
# This PowerShell script validates BusinessColumnInfo against actual remote tables
# Remote Server: 185.64.56.157
# Remote Database: DailyActionsDB
# Local Database: BIReportingCopilot_Dev

param(
    [string]$RemoteServer = "185.64.56.157",
    [string]$RemoteDatabase = "DailyActionsDB", 
    [string]$RemoteUser = "ReportsUser",
    [string]$RemotePassword = "",  # Pass as parameter for security
    [string]$LocalServer = "(localdb)\MSSQLLocalDB",
    [string]$LocalDatabase = "BIReportingCopilot_Dev"
)

# Import SQL Server module
Import-Module SqlServer -ErrorAction SilentlyContinue

function Get-RemoteTableColumns {
    param(
        [string]$TableName,
        [string]$Schema = "common"
    )
    
    $query = @"
        SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
        FROM INFORMATION_SCHEMA.COLUMNS 
        WHERE TABLE_NAME = '$TableName' 
        AND TABLE_SCHEMA = '$Schema'
        ORDER BY ORDINAL_POSITION
"@
    
    try {
        $connectionString = "Server=$RemoteServer;Database=$RemoteDatabase;User Id=$RemoteUser;Password=$RemotePassword;TrustServerCertificate=true;"
        return Invoke-Sqlcmd -Query $query -ConnectionString $connectionString
    }
    catch {
        Write-Error "Failed to connect to remote database: $_"
        return @()
    }
}

function Get-BusinessTableColumns {
    param([string]$TableName)
    
    $query = @"
        SELECT bci.ColumnName, bci.BusinessDataType
        FROM BusinessColumnInfo bci
        JOIN BusinessTableInfo bti ON bci.TableInfoId = bti.Id
        WHERE bti.TableName = '$TableName'
        ORDER BY bci.ColumnName
"@
    
    try {
        return Invoke-Sqlcmd -Query $query -ServerInstance $LocalServer -Database $LocalDatabase
    }
    catch {
        Write-Error "Failed to connect to local database: $_"
        return @()
    }
}

function Validate-TableColumns {
    param(
        [string]$TableName,
        [string]$Schema = "common"
    )
    
    Write-Host "=== VALIDATING $TableName ===" -ForegroundColor Yellow
    
    # Get columns from both databases
    $remoteColumns = Get-RemoteTableColumns -TableName $TableName -Schema $Schema
    $businessColumns = Get-BusinessTableColumns -TableName $TableName
    
    if ($remoteColumns.Count -eq 0) {
        Write-Host "❌ Could not retrieve remote columns for $TableName" -ForegroundColor Red
        return
    }
    
    # Convert to arrays for comparison
    $remoteColumnNames = $remoteColumns | ForEach-Object { $_.COLUMN_NAME }
    $businessColumnNames = $businessColumns | ForEach-Object { $_.ColumnName }
    
    # Find missing columns (in remote but not in business)
    $missingColumns = $remoteColumnNames | Where-Object { $_ -notin $businessColumnNames }
    
    # Find extra columns (in business but not in remote)
    $extraColumns = $businessColumnNames | Where-Object { $_ -notin $remoteColumnNames }
    
    # Display results
    Write-Host "Remote Columns: $($remoteColumns.Count)" -ForegroundColor Cyan
    Write-Host "Business Columns: $($businessColumns.Count)" -ForegroundColor Cyan
    
    if ($missingColumns.Count -gt 0) {
        Write-Host "⚠️  MISSING COLUMNS ($($missingColumns.Count)):" -ForegroundColor Red
        $missingColumns | ForEach-Object { Write-Host "   - $_" -ForegroundColor Red }
    }
    
    if ($extraColumns.Count -gt 0) {
        Write-Host "❌ EXTRA/INVENTED COLUMNS ($($extraColumns.Count)):" -ForegroundColor Magenta
        $extraColumns | ForEach-Object { Write-Host "   - $_" -ForegroundColor Magenta }
    }
    
    if ($missingColumns.Count -eq 0 -and $extraColumns.Count -eq 0) {
        Write-Host "✅ PERFECT MATCH!" -ForegroundColor Green
    }
    
    Write-Host ""
    
    return @{
        TableName = $TableName
        RemoteCount = $remoteColumns.Count
        BusinessCount = $businessColumns.Count
        MissingCount = $missingColumns.Count
        ExtraCount = $extraColumns.Count
        Missing = $missingColumns
        Extra = $extraColumns
    }
}

# =====================================================
# Main Validation
# =====================================================

Write-Host "=== DATABASE COLUMN VALIDATION ===" -ForegroundColor Green
Write-Host "Remote Server: $RemoteServer" -ForegroundColor Gray
Write-Host "Remote Database: $RemoteDatabase" -ForegroundColor Gray
Write-Host "Local Server: $LocalServer" -ForegroundColor Gray
Write-Host "Local Database: $LocalDatabase" -ForegroundColor Gray
Write-Host ""

# Check if password is provided
if ([string]::IsNullOrEmpty($RemotePassword)) {
    $RemotePassword = Read-Host "Enter password for $RemoteUser" -AsSecureString
    $RemotePassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($RemotePassword))
}

# Validate each table
$results = @()

# Gaming tables (common schema)
$gamingTables = @(
    "tbl_Daily_actions",
    "tbl_Daily_actions_players", 
    "tbl_Daily_actionsGBP_transactions",
    "tbl_Daily_actions_games",
    "tbl_Currencies",
    "tbl_Countries",
    "tbl_White_labels"
)

foreach ($table in $gamingTables) {
    $result = Validate-TableColumns -TableName $table -Schema "common"
    $results += $result
}

# Games table (dbo schema)
$result = Validate-TableColumns -TableName "Games" -Schema "dbo"
$results += $result

# =====================================================
# Summary Report
# =====================================================

Write-Host "=== VALIDATION SUMMARY ===" -ForegroundColor Green

$totalRemote = ($results | Measure-Object -Property RemoteCount -Sum).Sum
$totalBusiness = ($results | Measure-Object -Property BusinessCount -Sum).Sum
$totalMissing = ($results | Measure-Object -Property MissingCount -Sum).Sum
$totalExtra = ($results | Measure-Object -Property ExtraCount -Sum).Sum

Write-Host "Total Remote Columns: $totalRemote" -ForegroundColor Cyan
Write-Host "Total Business Columns: $totalBusiness" -ForegroundColor Cyan
Write-Host "Total Missing: $totalMissing" -ForegroundColor Red
Write-Host "Total Extra: $totalExtra" -ForegroundColor Magenta

if ($totalMissing -eq 0 -and $totalExtra -eq 0) {
    Write-Host "🎉 ALL TABLES PERFECTLY VALIDATED!" -ForegroundColor Green
} else {
    Write-Host "⚠️  Validation issues found. See details above." -ForegroundColor Yellow
}

# Export results to CSV
$results | Export-Csv -Path "validation_results.csv" -NoTypeInformation
Write-Host "Results exported to validation_results.csv" -ForegroundColor Gray
