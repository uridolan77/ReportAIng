# =====================================================
# Simple BI Reporting Copilot API Endpoint Tester
# =====================================================
# Credentials: admin / Admin123!
# =====================================================

param(
    [string]$BaseUrl = "http://localhost:55244",
    [string]$Username = "admin", 
    [string]$Password = "Admin123!",
    [switch]$TestCostManagement,
    [switch]$Verbose
)

# Global variables
$Global:AuthToken = $null

# Function to authenticate and get JWT token
function Get-AuthToken {
    param(
        [string]$BaseUrl,
        [string]$Username,
        [string]$Password
    )
    
    Write-Host "Authenticating with credentials: $Username" -ForegroundColor Cyan
    
    try {
        $loginBody = @{
            username = $Username
            password = $Password
        } | ConvertTo-Json
        
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"

        Write-Host "Login response: $($response | ConvertTo-Json -Depth 3)" -ForegroundColor Yellow

        if ($response.accessToken) {
            Write-Host "Authentication successful!" -ForegroundColor Green
            return $response.accessToken
        } elseif ($response.token) {
            Write-Host "Authentication successful!" -ForegroundColor Green
            return $response.token
        } else {
            Write-Host "Authentication failed - no token received" -ForegroundColor Red
            Write-Host "Response: $($response | ConvertTo-Json -Depth 3)" -ForegroundColor Red
            return $null
        }
    }
    catch {
        Write-Host "Authentication failed: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Function to make authenticated API calls
function Invoke-AuthenticatedRequest {
    param(
        [string]$Uri,
        [string]$Method = "GET",
        [object]$Body = $null
    )
    
    if (-not $Global:AuthToken) {
        Write-Host "No authentication token available" -ForegroundColor Red
        return $null
    }
    
    $headers = @{
        "Authorization" = "Bearer $Global:AuthToken"
        "Accept" = "application/json"
    }
    
    try {
        if ($Verbose) {
            Write-Host "$Method $Uri" -ForegroundColor Yellow
        }
        
        if ($Body) {
            $jsonBody = ($Body | ConvertTo-Json -Depth 10)
            $response = Invoke-RestMethod -Uri $Uri -Method $Method -Headers $headers -Body $jsonBody -ContentType "application/json"
        } else {
            $response = Invoke-RestMethod -Uri $Uri -Method $Method -Headers $headers
        }
        
        Write-Host "SUCCESS: $Method $Uri" -ForegroundColor Green
        return $response
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "FAILED: $Method $Uri - Status: $statusCode" -ForegroundColor Red
        
        if ($Verbose) {
            Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
        }
        return $null
    }
}

# Function to test Cost Management endpoints
function Test-CostManagementEndpoints {
    param([string]$BaseUrl)
    
    Write-Host "`nTesting Cost Management Endpoints" -ForegroundColor Magenta
    Write-Host "=" * 50 -ForegroundColor Magenta
    
    # Test analytics endpoint
    $analytics = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/cost-management/analytics"
    if ($analytics) {
        Write-Host "Cost Analytics: Retrieved data successfully" -ForegroundColor Cyan
    }
    
    # Test cost trends
    $trends = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/cost-management/trends?days=30"
    if ($trends) {
        Write-Host "Cost Trends: Retrieved trends data" -ForegroundColor Cyan
    }
    
    # Test budget status
    $budget = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/cost-management/budget/status"
    if ($budget) {
        Write-Host "Budget Status: Retrieved budget information" -ForegroundColor Cyan
    }
    
    # Test cost predictions
    $predictions = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/cost-management/predictions"
    if ($predictions) {
        Write-Host "Cost Predictions: Retrieved prediction data" -ForegroundColor Cyan
    }
    
    # Test optimization recommendations
    $recommendations = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/cost-management/recommendations"
    if ($recommendations) {
        Write-Host "Optimization Recommendations: Found recommendations" -ForegroundColor Cyan
    }
    
    # Test cost breakdown by provider
    $providerBreakdown = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/cost-management/breakdown/provider"
    if ($providerBreakdown) {
        Write-Host "Provider Breakdown: Retrieved cost by provider" -ForegroundColor Cyan
    }
    
    # Test cost breakdown by model
    $modelBreakdown = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/cost-management/breakdown/model"
    if ($modelBreakdown) {
        Write-Host "Model Breakdown: Retrieved cost by model" -ForegroundColor Cyan
    }
}

# Function to test Core endpoints
function Test-CoreEndpoints {
    param([string]$BaseUrl)
    
    Write-Host "`nTesting Core API Endpoints" -ForegroundColor Magenta
    Write-Host "=" * 50 -ForegroundColor Magenta
    
    # Test health check (no auth required)
    try {
        $health = Invoke-RestMethod -Uri "$BaseUrl/health" -Method GET
        Write-Host "Health Check: SUCCESS" -ForegroundColor Green
    }
    catch {
        Write-Host "Health Check: FAILED" -ForegroundColor Red
    }
    
    # Test user profile
    $profile = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/auth/profile"
    if ($profile) {
        Write-Host "User Profile: $($profile.email)" -ForegroundColor Cyan
    }
    
    # Test features
    $features = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/features"
    if ($features) {
        Write-Host "Features: Retrieved features data" -ForegroundColor Cyan
    }
    
    # Test schema endpoints
    $schema = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/schema/tables"
    if ($schema) {
        Write-Host "Schema Tables: Retrieved schema data" -ForegroundColor Cyan
    }
}

# Function to test LLM Management endpoints
function Test-LLMManagementEndpoints {
    param([string]$BaseUrl)
    
    Write-Host "`nTesting LLM Management Endpoints" -ForegroundColor Magenta
    Write-Host "=" * 50 -ForegroundColor Magenta
    
    # Test providers
    $providers = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/llmmanagement/providers"
    if ($providers) {
        Write-Host "LLM Providers: Retrieved providers data" -ForegroundColor Cyan
    }

    # Test models
    $models = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/llmmanagement/models"
    if ($models) {
        Write-Host "LLM Models: Retrieved models data" -ForegroundColor Cyan
    }

    # Test usage history
    $usage = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/llmmanagement/usage/history?take=10"
    if ($usage) {
        Write-Host "Usage History: Retrieved usage data" -ForegroundColor Cyan
    }

    # Test usage analytics
    $analytics = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/llmmanagement/usage/analytics?startDate=2025-05-01&endDate=2025-06-22"
    if ($analytics) {
        Write-Host "Usage Analytics: Retrieved analytics data" -ForegroundColor Cyan
    }

    # Test dashboard summary
    $dashboard = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/llmmanagement/dashboard/summary"
    if ($dashboard) {
        Write-Host "Dashboard Summary: Retrieved dashboard data" -ForegroundColor Cyan
    }

    # Test provider health
    $health = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/llmmanagement/providers/health"
    if ($health) {
        Write-Host "Provider Health: Retrieved health status" -ForegroundColor Cyan
    }
}

# Main execution
Write-Host "BI Reporting Copilot API Endpoint Tester" -ForegroundColor Magenta
Write-Host "=" * 60 -ForegroundColor Magenta
Write-Host "Base URL: $BaseUrl" -ForegroundColor Cyan
Write-Host "Username: $Username" -ForegroundColor Cyan
Write-Host ""

# Authenticate
$Global:AuthToken = Get-AuthToken -BaseUrl $BaseUrl -Username $Username -Password $Password

if (-not $Global:AuthToken) {
    Write-Host "Cannot proceed without authentication" -ForegroundColor Red
    exit 1
}

# Test endpoints based on parameters
if ($TestCostManagement) {
    Test-CostManagementEndpoints -BaseUrl $BaseUrl
} else {
    Write-Host "`nUsage Examples:" -ForegroundColor Yellow
    Write-Host "  .\SimpleApiTester.ps1 -TestCostManagement    # Test Cost Management endpoints" -ForegroundColor Yellow
    Write-Host "  .\SimpleApiTester.ps1 -TestCostManagement -Verbose  # Test with detailed output" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Testing Core Endpoints by default..." -ForegroundColor Cyan
    Test-CoreEndpoints -BaseUrl $BaseUrl
    Test-LLMManagementEndpoints -BaseUrl $BaseUrl
}

Write-Host "`nTesting completed!" -ForegroundColor Green
