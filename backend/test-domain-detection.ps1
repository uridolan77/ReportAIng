# Enhanced Schema Contextualization System Test Script
Write-Host "🧪 Testing Enhanced Schema Contextualization System..." -ForegroundColor Cyan

# Test query
$testQuery = "Top 10 depositors yesterday from UK"
$apiUrl = "https://localhost:55244/api/pipeline/test"

# Create test request for pipeline test
$requestBody = @{
    query = $testQuery
    requestedSteps = @("BusinessContextAnalysis", "TokenBudgetManagement", "SchemaRetrieval")
} | ConvertTo-Json

Write-Host "📝 Testing query: $testQuery" -ForegroundColor Yellow
Write-Host "🔗 API URL: $apiUrl" -ForegroundColor Gray

try {
    # Make API call
    Write-Host "🚀 Making API call..." -ForegroundColor Blue
    $response = Invoke-RestMethod -Uri $apiUrl -Method POST -Body $requestBody -ContentType "application/json" -SkipCertificateCheck

    Write-Host "✅ API Response received" -ForegroundColor Green

    # Extract results
    $businessContext = $response.results.BusinessContextAnalysis
    $schemaRetrieval = $response.results.SchemaRetrieval
    $tables = $schemaRetrieval.schemaMetadata.relevantTables
    $totalColumns = 0

    # Calculate total columns
    if ($schemaRetrieval.schemaMetadata.tableColumns) {
        $totalColumns = ($schemaRetrieval.schemaMetadata.tableColumns.PSObject.Properties.Value | Measure-Object -Sum Count).Sum
    }

    Write-Host "`n🧪 ENHANCED SCHEMA CONTEXTUALIZATION TEST RESULTS:" -ForegroundColor Cyan
    Write-Host "=" * 60 -ForegroundColor Gray

    # Test 1: Domain Classification
    Write-Host "Domain: $($businessContext.domain)" -ForegroundColor White
    if ($businessContext.domain -eq "Banking" -or $businessContext.domain -eq "Financial") {
        Write-Host "Domain Detection: WORKING (Banking/Financial detected)" -ForegroundColor Green
    } else {
        Write-Host "Domain Detection: FAILED (Expected Banking/Financial, got $($businessContext.domain))" -ForegroundColor Red
    }

    # Test 2: Table Selection
    Write-Host "Tables Found: $($tables.Count)" -ForegroundColor White
    foreach ($table in $tables) {
        Write-Host "   - $($table.tableName) ($($table.businessName))" -ForegroundColor Gray
    }

    # Test 3: Gaming Table Filtering
    $gamingTables = $tables | Where-Object {
        $_.tableName -like "*game*" -or
        $_.tableName -eq "tbl_Daily_actions" -or
        $_.tableName -like "*gaming*" -or
        $_.tableName -like "*bet*"
    }

    if ($gamingTables.Count -eq 0) {
        Write-Host "Gaming Table Filtering: WORKING (No gaming tables found)" -ForegroundColor Green
    } else {
        Write-Host "Gaming Table Filtering: FAILED" -ForegroundColor Red
        Write-Host "   Gaming tables found: $($gamingTables.tableName -join ', ')" -ForegroundColor Red
    }

    # Test 4: Column Filtering
    Write-Host "Total Columns: $totalColumns" -ForegroundColor White
    if ($totalColumns -lt 100) {
        Write-Host "Column Filtering: WORKING ($totalColumns columns - good performance)" -ForegroundColor Green
    } elseif ($totalColumns -lt 1000) {
        Write-Host "Column Filtering: PARTIAL ($totalColumns columns - needs improvement)" -ForegroundColor Yellow
    } else {
        Write-Host "Column Filtering: FAILED ($totalColumns columns - too many)" -ForegroundColor Red
    }

    # Test 5: Expected Tables Check
    $expectedTables = @("Players", "tbl_Countries", "GBP_transactions", "Countries")
    $foundExpectedTables = $tables | Where-Object {
        $expectedTables -contains $_.tableName -or
        $_.tableName -like "*player*" -or
        $_.tableName -like "*country*" -or
        $_.tableName -like "*transaction*"
    }

    if ($foundExpectedTables.Count -gt 0) {
        Write-Host "Expected Tables: FOUND ($($foundExpectedTables.Count) relevant tables)" -ForegroundColor Green
        foreach ($table in $foundExpectedTables) {
            Write-Host "   + $($table.tableName)" -ForegroundColor Green
        }
    } else {
        Write-Host "Expected Tables: MISSING (No Players/Countries/Transactions tables found)" -ForegroundColor Red
    }

    # Test 6: Performance Check
    $duration = $response.totalDurationMs
    if ($duration -lt 5000) {
        Write-Host "Performance: GOOD ($duration ms)" -ForegroundColor Green
    } elseif ($duration -lt 10000) {
        Write-Host "Performance: ACCEPTABLE ($duration ms)" -ForegroundColor Yellow
    } else {
        Write-Host "Performance: SLOW ($duration ms)" -ForegroundColor Red
    }

    Write-Host "`n📋 SUMMARY:" -ForegroundColor Cyan
    Write-Host "=" * 60 -ForegroundColor Gray

    # Overall assessment
    $passedTests = 0
    $totalTests = 6

    if ($businessContext.domain -eq "Banking" -or $businessContext.domain -eq "Financial") { $passedTests++ }
    if ($gamingTables.Count -eq 0) { $passedTests++ }
    if ($totalColumns -lt 100) { $passedTests++ }
    if ($foundExpectedTables.Count -gt 0) { $passedTests++ }
    if ($duration -lt 5000) { $passedTests++ }
    if ($tables.Count -le 5 -and $tables.Count -gt 0) { $passedTests++ }

    $successRate = [math]::Round(($passedTests / $totalTests) * 100, 1)

    if ($successRate -ge 80) {
        Write-Host "ENHANCED SCHEMA CONTEXTUALIZATION: WORKING ($successRate)" -ForegroundColor Green
    } elseif ($successRate -ge 60) {
        Write-Host "ENHANCED SCHEMA CONTEXTUALIZATION: PARTIAL ($successRate)" -ForegroundColor Yellow
    } else {
        Write-Host "ENHANCED SCHEMA CONTEXTUALIZATION: FAILED ($successRate)" -ForegroundColor Red
    }

    Write-Host "`nSaving detailed results to test-results.json..." -ForegroundColor Blue
    $response | ConvertTo-Json -Depth 10 | Out-File -FilePath "test-results.json" -Encoding UTF8
    Write-Host "Results saved to test-results.json" -ForegroundColor Green

} catch {
    Write-Host "API call failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Make sure the API is running on https://localhost:55244" -ForegroundColor Yellow
    Write-Host "Check if the pipeline test endpoint is available" -ForegroundColor Yellow
}

Write-Host "`nDEBUGGING INFORMATION:" -ForegroundColor Yellow
Write-Host "=" * 60 -ForegroundColor Gray
Write-Host "If column filtering is still not working, check the API logs for:" -ForegroundColor Yellow
Write-Host "[ENHANCED-COLUMN-DEBUG] - Enhanced column filtering debug messages" -ForegroundColor Yellow
Write-Host "[MAIN-COLUMN-DEBUG] - Main column retrieval debug messages" -ForegroundColor Yellow
Write-Host "[COLUMN-FILTER-DEBUG] - Column filter debug messages" -ForegroundColor Yellow
Write-Host "`nIf these messages are missing, the Enhanced Column Filtering System is not being called." -ForegroundColor Yellow

Write-Host "`nTest completed" -ForegroundColor Cyan
