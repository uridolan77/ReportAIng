# Test Transparency Integration
param(
    [string]$BaseUrl = "http://localhost:55244"
)

Write-Host "Testing Transparency Integration" -ForegroundColor Yellow
Write-Host "================================" -ForegroundColor Yellow

# Get auth token
$loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method POST -Body (@{
    username = "admin"
    password = "Admin123!"
} | ConvertTo-Json) -ContentType "application/json"

$token = $loginResponse.accessToken
Write-Host "✅ Authenticated successfully" -ForegroundColor Green

# Test enhanced query endpoint
Write-Host "`n🔍 Testing Enhanced Query with Transparency..." -ForegroundColor Cyan

try {
    $queryResponse = Invoke-RestMethod -Uri "$BaseUrl/api/query/enhanced" -Method POST -Headers @{
        'Authorization' = "Bearer $token"
    } -Body (@{
        query = "Show me sales data for last month"
        executeQuery = $false
        includeSemanticAnalysis = $true
    } | ConvertTo-Json) -ContentType "application/json"
    
    Write-Host "✅ Enhanced query executed successfully!" -ForegroundColor Green
    
    if ($queryResponse.transparencyData) {
        Write-Host "✅ Transparency data found in response!" -ForegroundColor Green
        Write-Host "TraceId: $($queryResponse.transparencyData.traceId)" -ForegroundColor Cyan
        Write-Host "Business Context Intent: $($queryResponse.transparencyData.businessContext.intent)" -ForegroundColor Cyan
        Write-Host "Confidence: $($queryResponse.transparencyData.businessContext.confidence)" -ForegroundColor Cyan
        Write-Host "Processing Steps: $($queryResponse.transparencyData.processingSteps)" -ForegroundColor Cyan
    } else {
        Write-Host "⚠️ No transparency data in response" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "❌ Enhanced query failed: $($_.Exception.Message)" -ForegroundColor Red
    return
}

# Test transparency endpoints
Write-Host "`n🔍 Testing Transparency Endpoints..." -ForegroundColor Cyan

try {
    # Test traces list
    $tracesResponse = Invoke-RestMethod -Uri "$BaseUrl/api/transparency/traces?page=1&pageSize=10" -Method GET -Headers @{
        'Authorization' = "Bearer $token"
    }
    
    Write-Host "✅ Transparency traces endpoint working!" -ForegroundColor Green
    Write-Host "Found $($tracesResponse.data.Count) traces" -ForegroundColor Cyan
    
    if ($tracesResponse.data.Count -gt 0) {
        $firstTrace = $tracesResponse.data[0]
        Write-Host "Latest trace: $($firstTrace.traceId) - $($firstTrace.userQuestion)" -ForegroundColor Cyan
        
        # Test individual trace details
        try {
            $traceDetails = Invoke-RestMethod -Uri "$BaseUrl/api/transparency/trace/$($firstTrace.traceId)" -Method GET -Headers @{
                'Authorization' = "Bearer $token"
            }
            Write-Host "✅ Individual trace details working!" -ForegroundColor Green
        } catch {
            Write-Host "⚠️ Individual trace details failed: $($_.Exception.Message)" -ForegroundColor Yellow
        }
    } else {
        Write-Host "⚠️ No transparency traces found in database" -ForegroundColor Yellow
    }
    
} catch {
    Write-Host "❌ Transparency traces endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test dashboard metrics
try {
    $metricsResponse = Invoke-RestMethod -Uri "$BaseUrl/api/transparency/metrics/dashboard?days=7" -Method GET -Headers @{
        'Authorization' = "Bearer $token"
    }
    
    Write-Host "✅ Transparency metrics endpoint working!" -ForegroundColor Green
    Write-Host "Total traces: $($metricsResponse.totalTraces)" -ForegroundColor Cyan
    Write-Host "Average confidence: $($metricsResponse.averageConfidence)" -ForegroundColor Cyan
    
} catch {
    Write-Host "❌ Transparency metrics endpoint failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🎯 Test Summary:" -ForegroundColor Yellow
Write-Host "- Enhanced query endpoint: Working" -ForegroundColor Green
Write-Host "- Transparency data in response: Check above" -ForegroundColor Cyan
Write-Host "- Transparency endpoints: Check above" -ForegroundColor Cyan
Write-Host "`nIf transparency traces are found, the integration is working! 🚀" -ForegroundColor Green
