# Test LLM Management and Cost Management endpoints
Write-Host "Testing LLM Management and Cost Management Endpoints" -ForegroundColor Green

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
    
    Write-Host "Token obtained successfully" -ForegroundColor Green
    
    $headers = @{
        "Authorization" = "Bearer $token"
    }
} catch {
    Write-Host "Failed to get token: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Test LLM Management endpoints
Write-Host "`n2. Testing LLM Management endpoints..." -ForegroundColor Yellow

# Test providers endpoint
try {
    $providers = Invoke-RestMethod -Uri "http://localhost:55244/api/llmmanagement/providers" -Method GET -Headers $headers
    Write-Host "SUCCESS: Providers endpoint - Found $($providers.Count) providers" -ForegroundColor Green
    
    # Show provider details
    foreach ($provider in $providers) {
        Write-Host "  - $($provider.Name) ($($provider.Type)) - Enabled: $($provider.IsEnabled)" -ForegroundColor Gray
    }
} catch {
    Write-Host "FAILED: Providers endpoint - $($_.Exception.Message)" -ForegroundColor Red
}

# Test provider health endpoint
try {
    $health = Invoke-RestMethod -Uri "http://localhost:55244/api/llmmanagement/providers/health" -Method GET -Headers $headers
    Write-Host "SUCCESS: Provider health endpoint - $($health.Count) health statuses" -ForegroundColor Green
} catch {
    Write-Host "FAILED: Provider health endpoint - $($_.Exception.Message)" -ForegroundColor Red
}

# Test dashboard summary endpoint
try {
    $dashboard = Invoke-RestMethod -Uri "http://localhost:55244/api/llmmanagement/dashboard/summary" -Method GET -Headers $headers
    Write-Host "SUCCESS: Dashboard summary endpoint" -ForegroundColor Green
    Write-Host "  - Total Providers: $($dashboard.providers.total)" -ForegroundColor Gray
    Write-Host "  - Enabled Providers: $($dashboard.providers.enabled)" -ForegroundColor Gray
    Write-Host "  - Total Requests: $($dashboard.usage.totalRequests)" -ForegroundColor Gray
} catch {
    Write-Host "FAILED: Dashboard summary endpoint - $($_.Exception.Message)" -ForegroundColor Red
}

# Test models endpoint
try {
    $models = Invoke-RestMethod -Uri "http://localhost:55244/api/llmmanagement/models" -Method GET -Headers $headers
    Write-Host "SUCCESS: Models endpoint - Found $($models.Count) models" -ForegroundColor Green
    
    # Show model details
    foreach ($model in $models) {
        Write-Host "  - $($model.DisplayName) ($($model.UseCase)) - Cost: $($model.CostPerToken)" -ForegroundColor Gray
    }
} catch {
    Write-Host "FAILED: Models endpoint - $($_.Exception.Message)" -ForegroundColor Red
}

# Step 3: Test Cost Management endpoints
Write-Host "`n3. Testing Cost Management endpoints..." -ForegroundColor Yellow

# Test recommendations endpoint
try {
    $recommendations = Invoke-RestMethod -Uri "http://localhost:55244/api/cost-management/recommendations" -Method GET -Headers $headers
    Write-Host "SUCCESS: Recommendations endpoint" -ForegroundColor Green
    if ($recommendations.recommendations) {
        Write-Host "  - Found $($recommendations.recommendations.Count) recommendations" -ForegroundColor Gray
    }
} catch {
    Write-Host "FAILED: Recommendations endpoint - $($_.Exception.Message)" -ForegroundColor Red
}

# Test analytics endpoint
try {
    $analytics = Invoke-RestMethod -Uri "http://localhost:55244/api/cost-management/analytics" -Method GET -Headers $headers
    Write-Host "SUCCESS: Analytics endpoint" -ForegroundColor Green
    if ($analytics.data) {
        Write-Host "  - Analytics data retrieved" -ForegroundColor Gray
    }
} catch {
    Write-Host "FAILED: Analytics endpoint - $($_.Exception.Message)" -ForegroundColor Red
}

# Test history endpoint
try {
    $history = Invoke-RestMethod -Uri "http://localhost:55244/api/cost-management/history" -Method GET -Headers $headers
    Write-Host "SUCCESS: History endpoint" -ForegroundColor Green
    if ($history.data) {
        Write-Host "  - Found $($history.total) history records" -ForegroundColor Gray
    }
} catch {
    Write-Host "FAILED: History endpoint - $($_.Exception.Message)" -ForegroundColor Red
}

# Test budgets endpoint
try {
    $budgets = Invoke-RestMethod -Uri "http://localhost:55244/api/cost-management/budgets" -Method GET -Headers $headers
    Write-Host "SUCCESS: Budgets endpoint" -ForegroundColor Green
    if ($budgets.budgets) {
        Write-Host "  - Found $($budgets.budgets.Count) budgets" -ForegroundColor Gray
    }
} catch {
    Write-Host "FAILED: Budgets endpoint - $($_.Exception.Message)" -ForegroundColor Red
}

# Test forecast endpoint
try {
    $forecast = Invoke-RestMethod -Uri "http://localhost:55244/api/cost-management/forecast" -Method GET -Headers $headers
    Write-Host "SUCCESS: Forecast endpoint" -ForegroundColor Green
} catch {
    Write-Host "FAILED: Forecast endpoint - $($_.Exception.Message)" -ForegroundColor Red
}

# Test realtime endpoint
try {
    $realtime = Invoke-RestMethod -Uri "http://localhost:55244/api/cost-management/realtime" -Method GET -Headers $headers
    Write-Host "SUCCESS: Realtime endpoint" -ForegroundColor Green
    if ($realtime.metrics) {
        Write-Host "  - Real-time metrics retrieved" -ForegroundColor Gray
    }
} catch {
    Write-Host "FAILED: Realtime endpoint - $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nLLM Management and Cost Management endpoint testing completed!" -ForegroundColor Green
