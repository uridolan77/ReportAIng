# Test script for AI Transparency functionality
Write-Host "🧪 Testing AI Transparency Implementation" -ForegroundColor Green

# Step 1: Get authentication token
Write-Host "`n1. Getting authentication token..." -ForegroundColor Yellow
try {
    $tokenRequest = @{
        userId = "test@example.com"
        userName = "Test User"
    } | ConvertTo-Json

    $tokenResponse = Invoke-RestMethod -Uri "http://localhost:55244/api/test/auth/token" -Method POST -ContentType "application/json" -Body $tokenRequest
    
    if ($tokenResponse -is [string]) {
        Write-Host "❌ Token response is a string (unexpected): $tokenResponse" -ForegroundColor Red
        exit 1
    }
    
    $token = $tokenResponse.token
    Write-Host "✅ Token obtained successfully" -ForegroundColor Green
    Write-Host "   Token preview: $($token.Substring(0,20))..." -ForegroundColor Gray
    
    $headers = @{
        "Authorization" = "Bearer $token"
    }
} catch {
    Write-Host "❌ Failed to get token: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Test transparency database
Write-Host "`n2. Testing transparency database..." -ForegroundColor Yellow
try {
    $transparencyResponse = Invoke-RestMethod -Uri "http://localhost:55244/api/test/transparency-db" -Method POST -Headers $headers
    Write-Host "✅ Transparency database test completed successfully" -ForegroundColor Green
    Write-Host "   Message: $($transparencyResponse.message)" -ForegroundColor Gray
    
    # Show some results
    if ($transparencyResponse.results) {
        Write-Host "   Saved records:" -ForegroundColor Gray
        Write-Host "     - Trace ID: $($transparencyResponse.results.savedTrace.TraceId)" -ForegroundColor Gray
        Write-Host "     - Step: $($transparencyResponse.results.savedStep.StepName)" -ForegroundColor Gray
        Write-Host "     - Context Domain: $($transparencyResponse.results.savedContext.DomainName)" -ForegroundColor Gray
        Write-Host "     - Budget: $($transparencyResponse.results.savedBudget.MaxTotalTokens) tokens" -ForegroundColor Gray
    }
} catch {
    Write-Host "❌ Transparency database test failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 3: Test enhanced query endpoint
Write-Host "`n3. Testing enhanced query endpoint..." -ForegroundColor Yellow
try {
    $queryRequest = @{
        Query = "Show me sales data for Q3 2024"
        ExecuteQuery = $false
        IncludeAlternatives = $true
        IncludeSemanticAnalysis = $true
    } | ConvertTo-Json

    $queryResponse = Invoke-RestMethod -Uri "http://localhost:55244/api/test/query/enhanced" -Method POST -Headers $headers -ContentType "application/json" -Body $queryRequest
    Write-Host "✅ Enhanced query test completed successfully" -ForegroundColor Green
    Write-Host "   Trace ID: $($queryResponse.TraceId)" -ForegroundColor Gray
    Write-Host "   Success: $($queryResponse.Success)" -ForegroundColor Gray
    Write-Host "   Query: $($queryResponse.Query)" -ForegroundColor Gray
} catch {
    Write-Host "❌ Enhanced query test failed: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   Response: $($_.Exception.Response)" -ForegroundColor Red
}

Write-Host "`n🎉 AI Transparency testing completed!" -ForegroundColor Green
