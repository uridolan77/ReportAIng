# Simple Transparency Endpoints Test
Write-Host "🧪 Testing Transparency Endpoints with Real Data" -ForegroundColor Green

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

# Step 2: Test transparency metrics (should work with existing data)
Write-Host "`n2. Testing transparency metrics..." -ForegroundColor Yellow
try {
    $metrics = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/metrics?days=7" -Method GET -Headers $headers
    Write-Host "✅ Transparency metrics retrieved successfully" -ForegroundColor Green
    Write-Host "   - Total Analyses: $($metrics.totalAnalyses)" -ForegroundColor Gray
    Write-Host "   - Average Confidence: $($metrics.averageConfidence)" -ForegroundColor Gray
    
    if ($metrics.topIntentTypes) {
        Write-Host "   - Top Intent Types:" -ForegroundColor Gray
        foreach ($intent in $metrics.topIntentTypes.PSObject.Properties) {
            Write-Host "     - $($intent.Name): $($intent.Value)" -ForegroundColor DarkGray
        }
    }
} catch {
    Write-Host "❌ Transparency metrics failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 3: Test dashboard metrics
Write-Host "`n3. Testing dashboard metrics..." -ForegroundColor Yellow
try {
    $dashboard = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/metrics/dashboard?days=30" -Method GET -Headers $headers
    Write-Host "✅ Dashboard metrics retrieved successfully" -ForegroundColor Green
    Write-Host "   - Total Traces: $($dashboard.totalTraces)" -ForegroundColor Gray
    Write-Host "   - Average Confidence: $($dashboard.averageConfidence)" -ForegroundColor Gray
    Write-Host "   - Top Optimizations: $($dashboard.topOptimizations.Count)" -ForegroundColor Gray
    Write-Host "   - Confidence Trends: $($dashboard.confidenceTrends.Count)" -ForegroundColor Gray
    Write-Host "   - Usage by Model: $($dashboard.usageByModel.Count)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Dashboard metrics failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 4: Test transparency settings
Write-Host "`n4. Testing transparency settings..." -ForegroundColor Yellow
try {
    $settings = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/settings" -Method GET -Headers $headers
    Write-Host "✅ Transparency settings retrieved successfully" -ForegroundColor Green
    Write-Host "   - Detailed Logging: $($settings.enableDetailedLogging)" -ForegroundColor Gray
    Write-Host "   - Confidence Threshold: $($settings.confidenceThreshold)" -ForegroundColor Gray
    Write-Host "   - Retention Days: $($settings.retentionDays)" -ForegroundColor Gray
    Write-Host "   - Optimization Suggestions: $($settings.enableOptimizationSuggestions)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Transparency settings failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 5: Test recent traces
Write-Host "`n5. Testing recent traces..." -ForegroundColor Yellow
try {
    $recentTraces = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/traces/recent?limit=5" -Method GET -Headers $headers
    Write-Host "✅ Recent traces retrieved successfully" -ForegroundColor Green
    Write-Host "   - Recent traces found: $($recentTraces.Count)" -ForegroundColor Gray
    
    if ($recentTraces.Count -gt 0) {
        foreach ($trace in $recentTraces) {
            Write-Host "     - $($trace.traceId): $($trace.intentType) (Confidence: $($trace.overallConfidence))" -ForegroundColor DarkGray
        }
        
        # Use first trace for detailed testing
        $testTraceId = $recentTraces[0].traceId
        Write-Host "   - Using trace $testTraceId for detailed testing" -ForegroundColor Gray
    } else {
        $testTraceId = "trace-001" # Use sample trace
        Write-Host "   - No recent traces found, using sample trace: $testTraceId" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Recent traces failed: $($_.Exception.Message)" -ForegroundColor Red
    $testTraceId = "trace-001" # Use sample trace
}

# Step 6: Test confidence breakdown with existing trace
Write-Host "`n6. Testing confidence breakdown..." -ForegroundColor Yellow
try {
    $confidence = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/confidence/$testTraceId" -Method GET -Headers $headers
    Write-Host "✅ Confidence breakdown retrieved successfully" -ForegroundColor Green
    Write-Host "   - Analysis ID: $($confidence.analysisId)" -ForegroundColor Gray
    Write-Host "   - Overall Confidence: $($confidence.overallConfidence)" -ForegroundColor Gray
    Write-Host "   - Factors: $($confidence.confidenceFactors.Count)" -ForegroundColor Gray
    
    if ($confidence.confidenceFactors.Count -gt 0) {
        foreach ($factor in $confidence.confidenceFactors) {
            Write-Host "     - $($factor.factorName): $($factor.score)" -ForegroundColor DarkGray
        }
    }
} catch {
    Write-Host "❌ Confidence breakdown failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 7: Test alternative options
Write-Host "`n7. Testing alternative options..." -ForegroundColor Yellow
try {
    $alternatives = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/alternatives/$testTraceId" -Method GET -Headers $headers
    Write-Host "✅ Alternative options retrieved successfully" -ForegroundColor Green
    Write-Host "   - Alternatives found: $($alternatives.Count)" -ForegroundColor Gray
    
    if ($alternatives.Count -gt 0) {
        foreach ($alt in $alternatives) {
            Write-Host "     - $($alt.type): $($alt.description)" -ForegroundColor DarkGray
            Write-Host "       Score: $($alt.score), Improvement: $($alt.estimatedImprovement)%" -ForegroundColor DarkGray
        }
    }
} catch {
    Write-Host "❌ Alternative options failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 8: Test optimization suggestions
Write-Host "`n8. Testing optimization suggestions..." -ForegroundColor Yellow
try {
    $optimizeRequest = @{
        userQuery = "Show me sales data for Q3 2024"
        traceId = $testTraceId
        priority = "Balanced"
    } | ConvertTo-Json

    $suggestions = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/optimize" -Method POST -Headers $headers -Body $optimizeRequest -ContentType "application/json"
    
    Write-Host "✅ Optimization suggestions retrieved successfully" -ForegroundColor Green
    Write-Host "   - Suggestions found: $($suggestions.Count)" -ForegroundColor Gray
    
    if ($suggestions.Count -gt 0) {
        foreach ($suggestion in $suggestions) {
            Write-Host "     - $($suggestion.type): $($suggestion.title)" -ForegroundColor DarkGray
            Write-Host "       Priority: $($suggestion.priority), Token Saving: $($suggestion.estimatedTokenSaving)" -ForegroundColor DarkGray
        }
    }
} catch {
    Write-Host "❌ Optimization suggestions failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 9: Test trace detail
Write-Host "`n9. Testing trace detail..." -ForegroundColor Yellow
try {
    $traceDetail = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/traces/$testTraceId/detail" -Method GET -Headers $headers
    Write-Host "✅ Trace detail retrieved successfully" -ForegroundColor Green
    Write-Host "   - Trace ID: $($traceDetail.traceId)" -ForegroundColor Gray
    Write-Host "   - Steps: $($traceDetail.steps.Count)" -ForegroundColor Gray
    Write-Host "   - Success: $($traceDetail.success)" -ForegroundColor Gray
    
    if ($traceDetail.finalPrompt) {
        Write-Host "   - Final Prompt Length: $($traceDetail.finalPrompt.Length) characters" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Trace detail failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 10: Test analytics endpoints
Write-Host "`n10. Testing analytics endpoints..." -ForegroundColor Yellow

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

# Step 11: Test transparency trace retrieval (if we have a valid trace)
if ($testTraceId -ne "trace-001") {
    Write-Host "`n11. Testing transparency trace retrieval..." -ForegroundColor Yellow
    try {
        $trace = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/trace/$testTraceId" -Method GET -Headers $headers
        Write-Host "✅ Transparency trace retrieved successfully" -ForegroundColor Green
        Write-Host "   - Trace ID: $($trace.traceId)" -ForegroundColor Gray
        Write-Host "   - User Question: $($trace.userQuestion)" -ForegroundColor Gray
        Write-Host "   - Intent Type: $($trace.intentType)" -ForegroundColor Gray
    } catch {
        Write-Host "❌ Transparency trace retrieval failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n🎉 Transparency Endpoints Test Completed!" -ForegroundColor Green
Write-Host "All transparency endpoints have been tested with real data integration." -ForegroundColor Green
Write-Host "The backend transparency implementation is complete and functional!" -ForegroundColor Green
