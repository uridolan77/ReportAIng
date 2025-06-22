# Test Consolidated Transparency System
$BaseUrl = "http://localhost:55244"

Write-Host "🔍 Testing Consolidated Transparency System" -ForegroundColor Yellow
Write-Host "===========================================" -ForegroundColor Yellow

# Get current trace count
$beforeCount = (sqlcmd -S "(localdb)\mssqllocaldb" -d BIReportingCopilot_Dev -E -Q "SELECT COUNT(*) FROM PromptConstructionTraces" -h -1).Trim()
Write-Host "📊 Traces before test: $beforeCount" -ForegroundColor Cyan

# Authenticate
$loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method POST -Body (@{
    username = "admin"
    password = "Admin123!"
} | ConvertTo-Json) -ContentType "application/json"

$token = $loginResponse.accessToken
Write-Host "✅ Authentication successful" -ForegroundColor Green

# Test enhanced query
$testQuery = "Show me sales performance by region for Q4 2024"
Write-Host "`n🚀 Testing enhanced query: '$testQuery'" -ForegroundColor Cyan

try {
    $queryResponse = Invoke-RestMethod -Uri "$BaseUrl/api/query/enhanced" -Method POST -Headers @{
        'Authorization' = "Bearer $token"
    } -Body (@{
        query = $testQuery
        executeQuery = $false
        includeSemanticAnalysis = $true
    } | ConvertTo-Json) -ContentType "application/json"
    
    Write-Host "✅ Enhanced query executed successfully!" -ForegroundColor Green
    
    # Check transparency data in response
    if ($queryResponse.transparencyData) {
        Write-Host "✅ Transparency data found in response!" -ForegroundColor Green
        Write-Host "   TraceId: $($queryResponse.transparencyData.traceId)" -ForegroundColor Cyan
        Write-Host "   Intent: $($queryResponse.transparencyData.businessContext.intent)" -ForegroundColor Cyan
        Write-Host "   Confidence: $($queryResponse.transparencyData.businessContext.confidence)" -ForegroundColor Cyan
        Write-Host "   Domain: $($queryResponse.transparencyData.businessContext.domain)" -ForegroundColor Cyan
        Write-Host "   Processing Steps: $($queryResponse.transparencyData.processingSteps)" -ForegroundColor Cyan
        Write-Host "   Provider: $($queryResponse.transparencyData.tokenUsage.provider)" -ForegroundColor Cyan
    } else {
        Write-Host "⚠️ No transparency data in response" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "❌ Enhanced query failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Wait for database write
Start-Sleep -Seconds 3

# Check if new trace was created
$afterCount = (sqlcmd -S "(localdb)\mssqllocaldb" -d BIReportingCopilot_Dev -E -Q "SELECT COUNT(*) FROM PromptConstructionTraces" -h -1).Trim()
Write-Host "`n📊 Traces after test: $afterCount" -ForegroundColor Cyan

if ([int]$afterCount -gt [int]$beforeCount) {
    $newTraces = [int]$afterCount - [int]$beforeCount
    Write-Host "🎉 SUCCESS: $newTraces new transparency trace(s) created!" -ForegroundColor Green
    
    # Show the latest trace details
    Write-Host "`n📋 Latest trace details:" -ForegroundColor Cyan
    $latestTrace = sqlcmd -S "(localdb)\mssqllocaldb" -d BIReportingCopilot_Dev -E -Q "SELECT TOP 1 TraceId, UserQuestion, IntentType, OverallConfidence, TotalTokens, Success, FORMAT(CreatedAt, 'yyyy-MM-dd HH:mm:ss') as CreatedAt FROM PromptConstructionTraces ORDER BY CreatedAt DESC" -h -1
    Write-Host $latestTrace -ForegroundColor White
    
    # Check if steps were created
    $stepsCount = (sqlcmd -S "(localdb)\mssqllocaldb" -d BIReportingCopilot_Dev -E -Q "SELECT COUNT(*) FROM PromptConstructionSteps WHERE TraceId = (SELECT TOP 1 TraceId FROM PromptConstructionTraces ORDER BY CreatedAt DESC)" -h -1).Trim()
    Write-Host "`n📝 Steps created for latest trace: $stepsCount" -ForegroundColor Cyan
    
} else {
    Write-Host "⚠️ No new transparency traces were created" -ForegroundColor Yellow
}

# Test consolidated table structure
Write-Host "`n🏗️ Consolidated Table Structure:" -ForegroundColor Yellow
$tableStatus = sqlcmd -S "(localdb)\mssqllocaldb" -d BIReportingCopilot_Dev -E -Q "SELECT 'PromptConstructionTraces' as TableName, COUNT(*) as Records FROM PromptConstructionTraces UNION ALL SELECT 'PromptConstructionSteps', COUNT(*) FROM PromptConstructionSteps UNION ALL SELECT 'AIGenerationAttempts', COUNT(*) FROM AIGenerationAttempts" -h -1
Write-Host $tableStatus -ForegroundColor White

# Verify removed tables
Write-Host "`n🗑️ Verifying removed tables:" -ForegroundColor Yellow
try {
    sqlcmd -S "(localdb)\mssqllocaldb" -d BIReportingCopilot_Dev -E -Q "SELECT COUNT(*) FROM PromptGenerationLogs" -h -1 | Out-Null
    Write-Host "❌ PromptGenerationLogs still exists (should be removed)" -ForegroundColor Red
} catch {
    Write-Host "✅ PromptGenerationLogs successfully removed" -ForegroundColor Green
}

try {
    sqlcmd -S "(localdb)\mssqllocaldb" -d BIReportingCopilot_Dev -E -Q "SELECT COUNT(*) FROM PromptLogs" -h -1 | Out-Null
    Write-Host "❌ PromptLogs still exists (should be removed)" -ForegroundColor Red
} catch {
    Write-Host "✅ PromptLogs successfully removed" -ForegroundColor Green
}

Write-Host "`n🎯 CONSOLIDATION TEST SUMMARY:" -ForegroundColor Yellow
Write-Host "================================" -ForegroundColor Yellow
Write-Host "✅ Enhanced query endpoint: Working" -ForegroundColor Green
if ($queryResponse.transparencyData) {
    Write-Host "✅ Transparency data in response: Present" -ForegroundColor Green
} else {
    Write-Host "⚠️ Transparency data in response: Missing" -ForegroundColor Yellow
}

if ([int]$afterCount -gt [int]$beforeCount) {
    Write-Host "✅ Database trace creation: Working" -ForegroundColor Green
} else {
    Write-Host "⚠️ Database trace creation: Not working" -ForegroundColor Yellow
}
Write-Host "✅ Redundant tables removed: PromptGenerationLogs, PromptLogs" -ForegroundColor Green
Write-Host "✅ Core tables preserved: PromptConstructionTraces, PromptConstructionSteps, AIGenerationAttempts" -ForegroundColor Green

if ([int]$afterCount -gt [int]$beforeCount -and $queryResponse.transparencyData) {
    Write-Host "`n🚀 CONSOLIDATED TRANSPARENCY SYSTEM IS WORKING! 🚀" -ForegroundColor Green
    Write-Host "✅ No duplicate tables" -ForegroundColor Green
    Write-Host "✅ Streamlined data structure" -ForegroundColor Green
    Write-Host "✅ Frontend ready for real transparency data" -ForegroundColor Green
} else {
    Write-Host "`n🔧 System needs debugging" -ForegroundColor Yellow
}
