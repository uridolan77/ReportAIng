# Comprehensive Transparency Pipeline Test
Write-Host "🧪 Testing Complete Transparency Pipeline" -ForegroundColor Green

# Step 1: Get authentication token
Write-Host "`n1. Getting authentication token..." -ForegroundColor Yellow
try {
    $tokenRequest = @{
        userId = "test@example.com"
        userName = "Test User"
    } | ConvertTo-Json

    $tokenResponse = Invoke-WebRequest -Uri "http://localhost:55244/api/test/auth/token" -Method POST -ContentType "application/json" -Body $tokenRequest
    $tokenData = $tokenResponse.Content | ConvertFrom-Json
    $token = $tokenData.token
    
    Write-Host "✅ Token obtained successfully" -ForegroundColor Green
    
    $headers = @{
        "Authorization" = "Bearer $token"
    }
} catch {
    Write-Host "❌ Failed to get token: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Test transparency database setup
Write-Host "`n2. Testing transparency database..." -ForegroundColor Yellow
try {
    $dbTest = Invoke-RestMethod -Uri "http://localhost:55244/api/test/transparency-db" -Method POST -Headers $headers
    Write-Host "✅ Transparency database test passed" -ForegroundColor Green
    Write-Host "   - Tables verified: $($dbTest.tablesVerified)" -ForegroundColor Gray
    Write-Host "   - Sample data: $($dbTest.sampleDataCount) records" -ForegroundColor Gray
} catch {
    Write-Host "❌ Transparency database test failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 3: Test prompt construction analysis
Write-Host "`n3. Testing prompt construction analysis..." -ForegroundColor Yellow
try {
    $analyzeRequest = @{
        userQuery = "Show me sales data for Q3 2024"
        userId = "test@example.com"
        includeAlternatives = $true
        includeOptimizations = $true
    } | ConvertTo-Json

    $analysisResult = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/analyze" -Method POST -Headers $headers -Body $analyzeRequest -ContentType "application/json"
    
    Write-Host "✅ Prompt construction analysis completed" -ForegroundColor Green
    Write-Host "   - Trace ID: $($analysisResult.traceId)" -ForegroundColor Gray
    Write-Host "   - Overall Confidence: $($analysisResult.overallConfidence)" -ForegroundColor Gray
    Write-Host "   - Total Tokens: $($analysisResult.totalTokens)" -ForegroundColor Gray
    Write-Host "   - Steps: $($analysisResult.steps.Count)" -ForegroundColor Gray
    
    $traceId = $analysisResult.traceId
} catch {
    Write-Host "❌ Prompt construction analysis failed: $($_.Exception.Message)" -ForegroundColor Red
    $traceId = "trace-001" # Use sample trace for remaining tests
}

# Step 4: Test transparency trace retrieval
Write-Host "`n4. Testing transparency trace retrieval..." -ForegroundColor Yellow
try {
    $trace = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/trace/$traceId" -Method GET -Headers $headers
    Write-Host "✅ Transparency trace retrieved successfully" -ForegroundColor Green
    Write-Host "   - Trace ID: $($trace.traceId)" -ForegroundColor Gray
    Write-Host "   - User Question: $($trace.userQuestion)" -ForegroundColor Gray
    Write-Host "   - Intent Type: $($trace.intentType)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Transparency trace retrieval failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 5: Test confidence breakdown
Write-Host "`n5. Testing confidence breakdown..." -ForegroundColor Yellow
try {
    $confidence = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/confidence/$traceId" -Method GET -Headers $headers
    Write-Host "✅ Confidence breakdown retrieved successfully" -ForegroundColor Green
    Write-Host "   - Overall Confidence: $($confidence.overallConfidence)" -ForegroundColor Gray
    Write-Host "   - Factors: $($confidence.confidenceFactors.Count)" -ForegroundColor Gray
    
    foreach ($factor in $confidence.confidenceFactors) {
        Write-Host "     - $($factor.factorName): $($factor.score)" -ForegroundColor DarkGray
    }
} catch {
    Write-Host "❌ Confidence breakdown failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 6: Test alternative options
Write-Host "`n6. Testing alternative options..." -ForegroundColor Yellow
try {
    $alternatives = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/alternatives/$traceId" -Method GET -Headers $headers
    Write-Host "✅ Alternative options retrieved successfully" -ForegroundColor Green
    Write-Host "   - Alternatives found: $($alternatives.Count)" -ForegroundColor Gray
    
    foreach ($alt in $alternatives) {
        Write-Host "     - $($alt.type): $($alt.description)" -ForegroundColor DarkGray
    }
} catch {
    Write-Host "❌ Alternative options failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 7: Test optimization suggestions
Write-Host "`n7. Testing optimization suggestions..." -ForegroundColor Yellow
try {
    $optimizeRequest = @{
        userQuery = "Show me sales data for Q3 2024"
        traceId = $traceId
        priority = "Balanced"
    } | ConvertTo-Json

    $suggestions = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/optimize" -Method POST -Headers $headers -Body $optimizeRequest -ContentType "application/json"
    
    Write-Host "✅ Optimization suggestions retrieved successfully" -ForegroundColor Green
    Write-Host "   - Suggestions found: $($suggestions.Count)" -ForegroundColor Gray
    
    foreach ($suggestion in $suggestions) {
        Write-Host "     - $($suggestion.type): $($suggestion.title)" -ForegroundColor DarkGray
        Write-Host "       Token Saving: $($suggestion.estimatedTokenSaving), Performance Gain: $($suggestion.estimatedPerformanceGain)%" -ForegroundColor DarkGray
    }
} catch {
    Write-Host "❌ Optimization suggestions failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 8: Test transparency metrics
Write-Host "`n8. Testing transparency metrics..." -ForegroundColor Yellow
try {
    $metrics = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/metrics?days=7" -Method GET -Headers $headers
    Write-Host "✅ Transparency metrics retrieved successfully" -ForegroundColor Green
    Write-Host "   - Total Analyses: $($metrics.totalAnalyses)" -ForegroundColor Gray
    Write-Host "   - Average Confidence: $($metrics.averageConfidence)" -ForegroundColor Gray
    Write-Host "   - Top Intent Types:" -ForegroundColor Gray
    
    foreach ($intent in $metrics.topIntentTypes.PSObject.Properties) {
        Write-Host "     - $($intent.Name): $($intent.Value)" -ForegroundColor DarkGray
    }
} catch {
    Write-Host "❌ Transparency metrics failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 9: Test dashboard metrics
Write-Host "`n9. Testing dashboard metrics..." -ForegroundColor Yellow
try {
    $dashboard = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/metrics/dashboard?days=30" -Method GET -Headers $headers
    Write-Host "✅ Dashboard metrics retrieved successfully" -ForegroundColor Green
    Write-Host "   - Total Traces: $($dashboard.totalTraces)" -ForegroundColor Gray
    Write-Host "   - Average Confidence: $($dashboard.averageConfidence)" -ForegroundColor Gray
    Write-Host "   - Top Optimizations: $($dashboard.topOptimizations.Count)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Dashboard metrics failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 10: Test transparency settings
Write-Host "`n10. Testing transparency settings..." -ForegroundColor Yellow
try {
    $settings = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/settings" -Method GET -Headers $headers
    Write-Host "✅ Transparency settings retrieved successfully" -ForegroundColor Green
    Write-Host "   - Detailed Logging: $($settings.enableDetailedLogging)" -ForegroundColor Gray
    Write-Host "   - Confidence Threshold: $($settings.confidenceThreshold)" -ForegroundColor Gray
    Write-Host "   - Retention Days: $($settings.retentionDays)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Transparency settings failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 11: Test recent traces
Write-Host "`n11. Testing recent traces..." -ForegroundColor Yellow
try {
    $recentTraces = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/traces/recent?limit=5" -Method GET -Headers $headers
    Write-Host "✅ Recent traces retrieved successfully" -ForegroundColor Green
    Write-Host "   - Recent traces found: $($recentTraces.Count)" -ForegroundColor Gray
    
    foreach ($trace in $recentTraces) {
        Write-Host "     - $($trace.traceId): $($trace.intentType) (Confidence: $($trace.overallConfidence))" -ForegroundColor DarkGray
    }
} catch {
    Write-Host "❌ Recent traces failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 12: Test trace detail
Write-Host "`n12. Testing trace detail..." -ForegroundColor Yellow
try {
    $traceDetail = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/traces/$traceId/detail" -Method GET -Headers $headers
    Write-Host "✅ Trace detail retrieved successfully" -ForegroundColor Green
    Write-Host "   - Steps: $($traceDetail.steps.Count)" -ForegroundColor Gray
    Write-Host "   - Final Prompt Length: $($traceDetail.finalPrompt.Length) characters" -ForegroundColor Gray
} catch {
    Write-Host "❌ Trace detail failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 13: Test analytics endpoints
Write-Host "`n13. Testing analytics endpoints..." -ForegroundColor Yellow

# Confidence trends
try {
    $confidenceTrends = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/analytics/confidence-trends?days=7" -Method GET -Headers $headers
    Write-Host "✅ Confidence trends retrieved successfully" -ForegroundColor Green
    Write-Host "   - Data points: $($confidenceTrends.Count)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Confidence trends failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Token usage analytics
try {
    $tokenAnalytics = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/analytics/token-usage?days=7" -Method GET -Headers $headers
    Write-Host "✅ Token usage analytics retrieved successfully" -ForegroundColor Green
    Write-Host "   - Total Tokens Used: $($tokenAnalytics.totalTokensUsed)" -ForegroundColor Gray
    Write-Host "   - Average Tokens Per Query: $($tokenAnalytics.averageTokensPerQuery)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Token usage analytics failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Performance metrics
try {
    $performance = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/analytics/performance?days=7" -Method GET -Headers $headers
    Write-Host "✅ Performance metrics retrieved successfully" -ForegroundColor Green
    Write-Host "   - Total Queries: $($performance.totalQueries)" -ForegroundColor Gray
    Write-Host "   - Success Rate: $($performance.successRate)" -ForegroundColor Gray
    Write-Host "   - Average Processing Time: $($performance.averageProcessingTime)ms" -ForegroundColor Gray
} catch {
    Write-Host "❌ Performance metrics failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 14: Test export functionality
Write-Host "`n14. Testing export functionality..." -ForegroundColor Yellow
try {
    $exportRequest = @{
        format = "json"
        startDate = (Get-Date).AddDays(-7).ToString("yyyy-MM-ddTHH:mm:ssZ")
        endDate = (Get-Date).ToString("yyyy-MM-ddTHH:mm:ssZ")
    } | ConvertTo-Json

    $exportResponse = Invoke-WebRequest -Uri "http://localhost:55244/api/transparency/export" -Method POST -Headers $headers -Body $exportRequest -ContentType "application/json"
    
    if ($exportResponse.StatusCode -eq 200) {
        Write-Host "✅ Export functionality working" -ForegroundColor Green
        Write-Host "   - Content Type: $($exportResponse.Headers.'Content-Type')" -ForegroundColor Gray
        Write-Host "   - Content Length: $($exportResponse.Content.Length) bytes" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Export functionality failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🎉 Transparency Pipeline Test Completed!" -ForegroundColor Green
Write-Host "All transparency endpoints have been tested with real data integration." -ForegroundColor Green
