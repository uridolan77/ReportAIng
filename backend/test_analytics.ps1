# Test Analytics Endpoints
Write-Host "Testing Analytics Endpoints" -ForegroundColor Green

# Use a simple approach - generate a new token each time
$tokenRequest = '{"userId": "test@example.com", "userName": "Test User"}'

try {
    # Get token using Invoke-WebRequest to avoid parsing issues
    $tokenResponse = Invoke-WebRequest -Uri "http://localhost:55244/api/test/auth/token" -Method POST -ContentType "application/json" -Body $tokenRequest
    $tokenData = $tokenResponse.Content | ConvertFrom-Json
    $token = $tokenData.token
    
    Write-Host "Token obtained" -ForegroundColor Green
    
    $headers = @{
        "Authorization" = "Bearer $token"
    }
    
    # Test 1: Transparency Metrics
    Write-Host "`n1. Testing Transparency Metrics..." -ForegroundColor Yellow
    try {
        $metrics = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/metrics" -Method GET -Headers $headers
        Write-Host "Transparency Metrics successful" -ForegroundColor Green
        Write-Host "   Response type: $($metrics.GetType().Name)" -ForegroundColor Gray
    } catch {
        Write-Host "Transparency Metrics failed: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test 2: Analyze Prompt Construction
    Write-Host "`n2. Testing Analyze Prompt..." -ForegroundColor Yellow
    try {
        $analyzeRequest = @{
            prompt = "Show me sales data for Q3 2024"
            userId = "test@example.com"
        } | ConvertTo-Json

        $analysis = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/analyze" -Method POST -Headers $headers -ContentType "application/json" -Body $analyzeRequest
        Write-Host "Analyze Prompt successful" -ForegroundColor Green
        Write-Host "   Trace ID: $($analysis.traceId)" -ForegroundColor Gray
    } catch {
        Write-Host "Analyze Prompt failed: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test 3: Get Transparency Trace (using known trace ID)
    Write-Host "`n3. Testing Get Transparency Trace..." -ForegroundColor Yellow
    try {
        $traceId = "aa8c8f69-3a5e-40c5-b019-fd38503549a1"  # From our earlier test
        $trace = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/trace/$traceId" -Method GET -Headers $headers
        Write-Host "Get Transparency Trace successful" -ForegroundColor Green
        Write-Host "   Trace found: $($null -ne $trace)" -ForegroundColor Gray
    } catch {
        Write-Host "Get Transparency Trace failed: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Test 4: Optimization Suggestions
    Write-Host "`n4. Testing Optimization Suggestions..." -ForegroundColor Yellow
    try {
        $optimizeRequest = @{
            prompt = "Show me sales data for Q3 2024"
            context = "Sales analysis"
        } | ConvertTo-Json

        $optimization = Invoke-RestMethod -Uri "http://localhost:55244/api/transparency/optimize" -Method POST -Headers $headers -ContentType "application/json" -Body $optimizeRequest
        Write-Host "Optimization Suggestions successful" -ForegroundColor Green
        Write-Host "   Suggestions count: $($optimization.Count)" -ForegroundColor Gray
    } catch {
        Write-Host "Optimization Suggestions failed: $($_.Exception.Message)" -ForegroundColor Red
    }
    
} catch {
    Write-Host "Failed to get token: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nAnalytics testing completed!" -ForegroundColor Green
