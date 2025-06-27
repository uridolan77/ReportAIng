# Simple Enhanced Schema Contextualization System Test
Write-Host "Testing Enhanced Schema Contextualization System..." -ForegroundColor Cyan

$testQuery = "Top 10 depositors yesterday from UK"
$apiUrl = "http://localhost:55244/api/pipeline/test"

$requestBody = @{
    query = $testQuery
    requestedSteps = @("BusinessContextAnalysis", "TokenBudgetManagement", "SchemaRetrieval")
} | ConvertTo-Json

Write-Host "Testing query: $testQuery" -ForegroundColor Yellow

try {
    Write-Host "Making API call..." -ForegroundColor Blue
    $response = Invoke-RestMethod -Uri $apiUrl -Method POST -Body $requestBody -ContentType "application/json"
    
    Write-Host "API Response received" -ForegroundColor Green
    
    # Extract results
    $businessContext = $response.results.BusinessContextAnalysis
    $schemaRetrieval = $response.results.SchemaRetrieval
    $tables = $schemaRetrieval.schemaMetadata.relevantTables
    $totalColumns = 0
    
    # Calculate total columns
    if ($schemaRetrieval.schemaMetadata.tableColumns) {
        $totalColumns = ($schemaRetrieval.schemaMetadata.tableColumns.PSObject.Properties.Value | Measure-Object -Sum Count).Sum
    }
    
    Write-Host "`nTEST RESULTS:" -ForegroundColor Cyan
    Write-Host "Domain: $($businessContext.domain)" -ForegroundColor White
    Write-Host "Tables Found: $($tables.Count)" -ForegroundColor White
    Write-Host "Total Columns: $totalColumns" -ForegroundColor White
    
    # Show tables
    foreach ($table in $tables) {
        Write-Host "  - $($table.tableName)" -ForegroundColor Gray
    }
    
    # Check for gaming tables
    $gamingTables = $tables | Where-Object { 
        $_.tableName -like "*game*" -or 
        $_.tableName -eq "tbl_Daily_actions" -or
        $_.tableName -like "*gaming*" -or
        $_.tableName -like "*bet*"
    }
    
    Write-Host "`nANALYSIS:" -ForegroundColor Cyan
    
    if ($businessContext.domain -eq "Banking" -or $businessContext.domain -eq "Financial") {
        Write-Host "Domain Detection: WORKING" -ForegroundColor Green
    } else {
        Write-Host "Domain Detection: FAILED" -ForegroundColor Red
    }
    
    if ($gamingTables.Count -eq 0) {
        Write-Host "Gaming Table Filtering: WORKING" -ForegroundColor Green
    } else {
        Write-Host "Gaming Table Filtering: FAILED (Found: $($gamingTables.tableName -join ', '))" -ForegroundColor Red
    }
    
    if ($totalColumns -lt 100) {
        Write-Host "Column Filtering: WORKING ($totalColumns columns)" -ForegroundColor Green
    } elseif ($totalColumns -lt 1000) {
        Write-Host "Column Filtering: PARTIAL ($totalColumns columns)" -ForegroundColor Yellow
    } else {
        Write-Host "Column Filtering: FAILED ($totalColumns columns)" -ForegroundColor Red
    }
    
    # Save results
    $response | ConvertTo-Json -Depth 10 | Out-File -FilePath "test-results.json" -Encoding UTF8
    Write-Host "`nResults saved to test-results.json" -ForegroundColor Green
    
} catch {
    Write-Host "API call failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Make sure the API is running on http://localhost:55244" -ForegroundColor Yellow
}

Write-Host "`nTest completed" -ForegroundColor Cyan
