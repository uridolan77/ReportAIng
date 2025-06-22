# Simple Transparency Test
Write-Host "🧪 Testing Transparency Endpoints" -ForegroundColor Green

# Get token first
try {
    $tokenRequest = @{
        userId = "test@example.com"
        userName = "Test User"
    } | ConvertTo-Json

    $tokenResponse = Invoke-WebRequest -Uri "http://localhost:55244/api/test/auth/token" -Method POST -ContentType "application/json" -Body $tokenRequest
    $tokenData = $tokenResponse.Content | ConvertFrom-Json
    $token = $tokenData.token
    
    Write-Host "✅ Token obtained" -ForegroundColor Green
    
    $headers = @{
        "Authorization" = "Bearer $token"
    }
} catch {
    Write-Host "❌ Failed to get token: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test transparency settings
Write-Host "`nTesting transparency settings..." -ForegroundColor Yellow
try {
    $settings = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/settings" -Method GET -Headers $headers
    Write-Host "✅ Settings retrieved:" -ForegroundColor Green
    Write-Host "   - Detailed Logging: $($settings.enableDetailedLogging)" -ForegroundColor Gray
    Write-Host "   - Confidence Threshold: $($settings.confidenceThreshold)" -ForegroundColor Gray
    Write-Host "   - Retention Days: $($settings.retentionDays)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Settings failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test transparency metrics
Write-Host "`nTesting transparency metrics..." -ForegroundColor Yellow
try {
    $metrics = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/metrics?days=7" -Method GET -Headers $headers
    Write-Host "✅ Metrics retrieved:" -ForegroundColor Green
    Write-Host "   - Total Analyses: $($metrics.totalAnalyses)" -ForegroundColor Gray
    Write-Host "   - Average Confidence: $($metrics.averageConfidence)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Metrics failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test recent traces
Write-Host "`nTesting recent traces..." -ForegroundColor Yellow
try {
    $traces = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/traces/recent?limit=3" -Method GET -Headers $headers
    Write-Host "✅ Recent traces retrieved:" -ForegroundColor Green
    Write-Host "   - Count: $($traces.Count)" -ForegroundColor Gray
    
    if ($traces.Count -gt 0) {
        $testTraceId = $traces[0].traceId
        Write-Host "   - First trace: $testTraceId" -ForegroundColor Gray
        
        # Test confidence breakdown
        Write-Host "`nTesting confidence breakdown..." -ForegroundColor Yellow
        try {
            $confidence = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/confidence/$testTraceId" -Method GET -Headers $headers
            Write-Host "✅ Confidence breakdown retrieved:" -ForegroundColor Green
            Write-Host "   - Overall Confidence: $($confidence.overallConfidence)" -ForegroundColor Gray
            Write-Host "   - Factors: $($confidence.confidenceFactors.Count)" -ForegroundColor Gray
        } catch {
            Write-Host "❌ Confidence breakdown failed: $($_.Exception.Message)" -ForegroundColor Red
        }
        
        # Test alternatives
        Write-Host "`nTesting alternatives..." -ForegroundColor Yellow
        try {
            $alternatives = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/alternatives/$testTraceId" -Method GET -Headers $headers
            Write-Host "✅ Alternatives retrieved:" -ForegroundColor Green
            Write-Host "   - Count: $($alternatives.Count)" -ForegroundColor Gray
        } catch {
            Write-Host "❌ Alternatives failed: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "❌ Recent traces failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🎉 Transparency endpoints test completed!" -ForegroundColor Green
