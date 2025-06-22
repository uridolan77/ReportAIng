# Simple Transparency Test
$BaseUrl = "http://localhost:55244"

# Get auth token
$loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method POST -Body (@{
    username = "admin"
    password = "Admin123!"
} | ConvertTo-Json) -ContentType "application/json"

$token = $loginResponse.accessToken
Write-Host "Authenticated successfully" -ForegroundColor Green

# Test transparency endpoints
$headers = @{ 'Authorization' = "Bearer $token" }

Write-Host "`nTesting transparency endpoints..." -ForegroundColor Yellow

# Test 1: Dashboard metrics
try {
    $metrics = Invoke-RestMethod -Uri "$BaseUrl/api/transparency/metrics/dashboard?days=7" -Headers $headers -Method GET
    Write-Host "✅ Dashboard metrics: Working" -ForegroundColor Green
    Write-Host "   Total traces: $($metrics.totalTraces)" -ForegroundColor Cyan
} catch {
    Write-Host "❌ Dashboard metrics: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Traces list
try {
    $traces = Invoke-RestMethod -Uri "$BaseUrl/api/transparency/traces?page=1&pageSize=5" -Headers $headers -Method GET
    Write-Host "✅ Traces list: Working" -ForegroundColor Green
    Write-Host "   Found traces: $($traces.data.Count)" -ForegroundColor Cyan
    
    if ($traces.data.Count -gt 0) {
        $firstTrace = $traces.data[0]
        Write-Host "   Latest: $($firstTrace.traceId)" -ForegroundColor Cyan
        
        # Test 3: Individual trace
        try {
            $trace = Invoke-RestMethod -Uri "$BaseUrl/api/transparency/trace/$($firstTrace.traceId)" -Headers $headers -Method GET
            Write-Host "✅ Individual trace: Working" -ForegroundColor Green
        } catch {
            Write-Host "❌ Individual trace: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
} catch {
    Write-Host "❌ Traces list: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Enhanced query with transparency
try {
    $query = Invoke-RestMethod -Uri "$BaseUrl/api/query/enhanced" -Headers $headers -Method POST -Body (@{
        query = "Show me recent sales data"
        executeQuery = $false
        includeSemanticAnalysis = $true
    } | ConvertTo-Json) -ContentType "application/json"
    
    Write-Host "✅ Enhanced query: Working" -ForegroundColor Green
    if ($query.transparencyData) {
        Write-Host "   TraceId: $($query.transparencyData.traceId)" -ForegroundColor Cyan
        Write-Host "   Intent: $($query.transparencyData.businessContext.intent)" -ForegroundColor Cyan
    } else {
        Write-Host "   ⚠️ No transparency data in response" -ForegroundColor Yellow
    }
} catch {
    Write-Host "❌ Enhanced query: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nTest completed!" -ForegroundColor Green
