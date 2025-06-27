# Test Enhanced Schema Contextualization System Integration
# This script validates the Query Execution Controller integration fix

param(
    [string]$BaseUrl = "https://localhost:55244",
    [string]$AuthToken = "",
    [switch]$Verbose
)

Write-Host "🚀 Testing Enhanced Schema Contextualization System Integration" -ForegroundColor Green
Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow

# Test queries that should work with enhanced context
$testQueries = @(
    @{
        Name = "Financial Query - Deposits by Country"
        Question = "What were the total deposits by country last week?"
        ExpectedConfidence = 0.6
        ExpectedTables = @("tbl_Daily_actions", "tbl_Countries")
    },
    @{
        Name = "Gaming Query - Popular Games"
        Question = "Which games were most popular last week?"
        ExpectedConfidence = 0.5
        ExpectedTables = @("Games", "tbl_Daily_actions_games")
    },
    @{
        Name = "Player Analysis Query"
        Question = "Show me top 10 depositors from UK yesterday"
        ExpectedConfidence = 0.7
        ExpectedTables = @("tbl_Daily_actions", "tbl_Daily_actions_players", "tbl_Countries")
    }
)

# Headers for API calls
$headers = @{
    "Content-Type" = "application/json"
}

if ($AuthToken) {
    $headers["Authorization"] = "Bearer $AuthToken"
}

function Test-QueryExecution {
    param(
        [hashtable]$TestCase,
        [string]$Endpoint
    )
    
    Write-Host "`n🔍 Testing: $($TestCase.Name)" -ForegroundColor Cyan
    Write-Host "Query: $($TestCase.Question)" -ForegroundColor Gray
    
    $requestBody = @{
        question = $TestCase.Question
        sessionId = "test-session-$(Get-Random)"
        options = @{
            maxTables = 5
            maxTokens = 4000
        }
    } | ConvertTo-Json -Depth 3
    
    try {
        $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
        
        $response = Invoke-RestMethod -Uri "$BaseUrl$Endpoint" -Method POST -Body $requestBody -Headers $headers -TimeoutSec 60
        
        $stopwatch.Stop()
        $executionTime = $stopwatch.ElapsedMilliseconds
        
        Write-Host "✅ Request completed in $executionTime ms" -ForegroundColor Green
        
        # Validate response
        if ($response.success) {
            Write-Host "✅ Query executed successfully" -ForegroundColor Green
            Write-Host "   SQL Length: $($response.sql.Length) characters" -ForegroundColor Gray
            Write-Host "   Confidence: $($response.confidence * 100)%" -ForegroundColor Gray
            Write-Host "   Execution Time: $($response.executionTimeMs) ms" -ForegroundColor Gray
            
            # Check confidence threshold
            if ($response.confidence -ge $TestCase.ExpectedConfidence) {
                Write-Host "✅ Confidence meets expectations ($($response.confidence * 100)% >= $($TestCase.ExpectedConfidence * 100)%)" -ForegroundColor Green
            } else {
                Write-Host "⚠️ Confidence below expectations ($($response.confidence * 100)% < $($TestCase.ExpectedConfidence * 100)%)" -ForegroundColor Yellow
            }
            
            # Check for expected tables in SQL
            $sqlLower = $response.sql.ToLower()
            $foundTables = @()
            foreach ($table in $TestCase.ExpectedTables) {
                if ($sqlLower -like "*$($table.ToLower())*") {
                    $foundTables += $table
                }
            }
            
            if ($foundTables.Count -gt 0) {
                Write-Host "✅ Found expected tables: $($foundTables -join ', ')" -ForegroundColor Green
            } else {
                Write-Host "⚠️ No expected tables found in SQL" -ForegroundColor Yellow
            }
            
            # Check prompt details for enhanced context
            if ($response.promptDetails) {
                Write-Host "📝 Prompt Details:" -ForegroundColor Cyan
                Write-Host "   Template: $($response.promptDetails.templateName)" -ForegroundColor Gray
                Write-Host "   Length: $($response.promptDetails.promptLength) characters" -ForegroundColor Gray
                Write-Host "   Tables: $($response.promptDetails.schemaTablesCount)" -ForegroundColor Gray
                Write-Host "   Domain: $($response.promptDetails.businessDomain)" -ForegroundColor Gray
                Write-Host "   Enhanced: $($response.promptDetails.isEnhancedPrompt)" -ForegroundColor Gray
                Write-Host "   Source: $($response.promptDetails.enhancementSource)" -ForegroundColor Gray
                
                if ($response.promptDetails.isEnhancedPrompt) {
                    Write-Host "✅ Enhanced prompt was used" -ForegroundColor Green
                } else {
                    Write-Host "⚠️ Basic prompt was used (fallback)" -ForegroundColor Yellow
                }
            }
            
            if ($Verbose -and $response.sql) {
                Write-Host "`n📄 Generated SQL:" -ForegroundColor Cyan
                Write-Host $response.sql -ForegroundColor Gray
            }
            
            return @{
                Success = $true
                Confidence = $response.confidence
                ExecutionTime = $executionTime
                IsEnhanced = $response.promptDetails.isEnhancedPrompt
                TablesFound = $foundTables.Count
            }
        } else {
            Write-Host "❌ Query failed: $($response.error)" -ForegroundColor Red
            return @{
                Success = $false
                Error = $response.error
                ExecutionTime = $executionTime
            }
        }
    }
    catch {
        Write-Host "❌ Request failed: $($_.Exception.Message)" -ForegroundColor Red
        return @{
            Success = $false
            Error = $_.Exception.Message
            ExecutionTime = 0
        }
    }
}

# Test Query Execution Controller (Enhanced)
Write-Host "`n🎯 Testing Query Execution Controller (Enhanced Pipeline)" -ForegroundColor Magenta
$enhancedResults = @()

foreach ($testCase in $testQueries) {
    $result = Test-QueryExecution -TestCase $testCase -Endpoint "/api/query-execution/execute"
    $enhancedResults += $result
}

# Test AI Pipeline Test Controller (Reference)
Write-Host "`n🎯 Testing AI Pipeline Test Controller (Reference)" -ForegroundColor Magenta

$pipelineTestBody = @{
    testId = "comparison-test-$(Get-Random)"
    query = "What were the total deposits by country last week?"
    steps = @("BusinessContextAnalysis", "SchemaRetrieval", "PromptBuilding", "AIGeneration")
    parameters = @{
        maxTables = 5
        maxTokens = 4000
        enableAIGeneration = $false  # Don't actually call AI to avoid costs
    }
} | ConvertTo-Json -Depth 3

try {
    $pipelineResponse = Invoke-RestMethod -Uri "$BaseUrl/api/AIPipelineTest/test-steps" -Method POST -Body $pipelineTestBody -Headers $headers -TimeoutSec 60
    
    if ($pipelineResponse.success) {
        Write-Host "✅ AI Pipeline Test completed successfully" -ForegroundColor Green
        
        if ($pipelineResponse.results.BusinessContextAnalysis) {
            $businessContext = $pipelineResponse.results.BusinessContextAnalysis
            Write-Host "   Business Context Confidence: $($businessContext.confidenceScore * 100)%" -ForegroundColor Gray
            Write-Host "   Domain: $($businessContext.domain)" -ForegroundColor Gray
            Write-Host "   Intent: $($businessContext.intent)" -ForegroundColor Gray
        }
        
        if ($pipelineResponse.results.SchemaRetrieval) {
            $schemaRetrieval = $pipelineResponse.results.SchemaRetrieval
            Write-Host "   Tables Retrieved: $($schemaRetrieval.tablesRetrieved)" -ForegroundColor Gray
            Write-Host "   Table Names: $($schemaRetrieval.tableNames -join ', ')" -ForegroundColor Gray
        }
    }
}
catch {
    Write-Host "⚠️ AI Pipeline Test failed: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Summary
Write-Host "`n📊 Test Summary" -ForegroundColor Magenta
$successCount = ($enhancedResults | Where-Object { $_.Success }).Count
$enhancedCount = ($enhancedResults | Where-Object { $_.IsEnhanced }).Count
$avgConfidence = ($enhancedResults | Where-Object { $_.Success } | Measure-Object -Property Confidence -Average).Average
$avgExecutionTime = ($enhancedResults | Measure-Object -Property ExecutionTime -Average).Average

Write-Host "Total Tests: $($testQueries.Count)" -ForegroundColor Gray
Write-Host "Successful: $successCount" -ForegroundColor Green
Write-Host "Enhanced Prompts Used: $enhancedCount" -ForegroundColor Cyan
Write-Host "Average Confidence: $($avgConfidence * 100)%" -ForegroundColor Gray
Write-Host "Average Execution Time: $avgExecutionTime ms" -ForegroundColor Gray

if ($successCount -eq $testQueries.Count) {
    Write-Host "`n🎉 All tests passed! Enhanced Schema Contextualization System is working correctly." -ForegroundColor Green
} elseif ($successCount -gt 0) {
    Write-Host "`n⚠️ Some tests passed. Enhanced pipeline is partially working." -ForegroundColor Yellow
} else {
    Write-Host "`n❌ All tests failed. Enhanced pipeline needs investigation." -ForegroundColor Red
}

Write-Host "`n✅ Testing completed!" -ForegroundColor Green
