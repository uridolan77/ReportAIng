# =====================================================
# BI Reporting Copilot API Endpoint Tester
# =====================================================
# This PowerShell script tests all API endpoints with proper authentication
# Credentials: admin / Admin123!
# =====================================================

param(
    [string]$BaseUrl = "http://localhost:55244",
    [string]$Username = "admin",
    [string]$Password = "Admin123!",
    [switch]$TestCostManagement,
    [switch]$TestAll,
    [switch]$Verbose
)

# Global variables
$Global:AuthToken = $null
$Global:Headers = @{}

# Helper function to write colored output
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    
    switch ($Color) {
        "Green" { Write-Host $Message -ForegroundColor Green }
        "Red" { Write-Host $Message -ForegroundColor Red }
        "Yellow" { Write-Host $Message -ForegroundColor Yellow }
        "Cyan" { Write-Host $Message -ForegroundColor Cyan }
        "Magenta" { Write-Host $Message -ForegroundColor Magenta }
        default { Write-Host $Message }
    }
}

# Function to authenticate and get JWT token
function Get-AuthToken {
    param(
        [string]$BaseUrl,
        [string]$Username,
        [string]$Password
    )
    
    Write-ColorOutput "🔐 Authenticating with credentials: $Username" "Cyan"
    
    try {
        $loginBody = @{
            email = $Username
            password = $Password
        } | ConvertTo-Json
        
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
        
        if ($response.token) {
            Write-ColorOutput "✅ Authentication successful!" "Green"
            return $response.token
        } else {
            Write-ColorOutput "❌ Authentication failed - no token received" "Red"
            return $null
        }
    }
    catch {
        Write-ColorOutput "❌ Authentication failed: $($_.Exception.Message)" "Red"
        return $null
    }
}

# Function to make authenticated API calls
function Invoke-AuthenticatedRequest {
    param(
        [string]$Uri,
        [string]$Method = "GET",
        [object]$Body = $null,
        [string]$ContentType = "application/json"
    )
    
    if (-not $Global:AuthToken) {
        Write-ColorOutput "❌ No authentication token available" "Red"
        return $null
    }
    
    $headers = @{
        "Authorization" = "Bearer $Global:AuthToken"
        "Accept" = "application/json"
    }
    
    try {
        $params = @{
            Uri = $Uri
            Method = $Method
            Headers = $headers
        }
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
            $params.ContentType = $ContentType
        }
        
        if ($Verbose) {
            Write-ColorOutput "🔍 $Method $Uri" "Yellow"
        }
        
        $response = Invoke-RestMethod @params
        Write-ColorOutput "✅ $Method $Uri - Success" "Green"
        return $response
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        $statusDescription = $_.Exception.Response.StatusDescription
        Write-ColorOutput "❌ $Method $Uri - Failed ($statusCode $statusDescription)" "Red"
        
        if ($Verbose) {
            Write-ColorOutput "   Error: $($_.Exception.Message)" "Red"
        }
        return $null
    }
}

# Function to test Cost Management endpoints
function Test-CostManagementEndpoints {
    param([string]$BaseUrl)
    
    Write-ColorOutput "`n🧮 Testing Cost Management Endpoints" "Magenta"
    Write-ColorOutput "=" * 50 "Magenta"
    
    # Test analytics endpoint
    $analytics = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/cost-management/analytics"
    if ($analytics) {
        Write-ColorOutput "📊 Cost Analytics: Found $($analytics.Count) records" "Cyan"
    }
    
    # Test cost trends
    $trends = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/cost-management/trends?days=30"
    if ($trends) {
        Write-ColorOutput "📈 Cost Trends: Retrieved trends data" "Cyan"
    }
    
    # Test budget status
    $budget = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/cost-management/budget/status"
    if ($budget) {
        Write-ColorOutput "💰 Budget Status: Retrieved budget information" "Cyan"
    }
    
    # Test cost predictions
    $predictions = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/cost-management/predictions"
    if ($predictions) {
        Write-ColorOutput "🔮 Cost Predictions: Retrieved prediction data" "Cyan"
    }
    
    # Test optimization recommendations
    $recommendations = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/cost-management/recommendations"
    if ($recommendations) {
        Write-ColorOutput "💡 Optimization Recommendations: Found recommendations" "Cyan"
    }
}

# Function to test LLM Management endpoints
function Test-LLMManagementEndpoints {
    param([string]$BaseUrl)
    
    Write-ColorOutput "`n🤖 Testing LLM Management Endpoints" "Magenta"
    Write-ColorOutput "=" * 50 "Magenta"
    
    # Test providers
    $providers = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/llm-management/providers"
    if ($providers) {
        Write-ColorOutput "🏭 LLM Providers: Found $($providers.Count) providers" "Cyan"
    }
    
    # Test models
    $models = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/llm-management/models"
    if ($models) {
        Write-ColorOutput "🧠 LLM Models: Found $($models.Count) models" "Cyan"
    }
    
    # Test usage logs
    $usage = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/llm-management/usage?limit=10"
    if ($usage) {
        Write-ColorOutput "📋 Usage Logs: Retrieved usage data" "Cyan"
    }
    
    # Test performance metrics
    $performance = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/llm-management/performance"
    if ($performance) {
        Write-ColorOutput "⚡ Performance Metrics: Retrieved metrics" "Cyan"
    }
}

# Function to test Core API endpoints
function Test-CoreEndpoints {
    param([string]$BaseUrl)
    
    Write-ColorOutput "`n🔧 Testing Core API Endpoints" "Magenta"
    Write-ColorOutput "=" * 50 "Magenta"
    
    # Test health check
    try {
        $health = Invoke-RestMethod -Uri "$BaseUrl/health" -Method GET
        Write-ColorOutput "❤️ Health Check: $($health.status)" "Green"
    }
    catch {
        Write-ColorOutput "❌ Health Check: Failed" "Red"
    }
    
    # Test user profile
    $profile = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/auth/profile"
    if ($profile) {
        Write-ColorOutput "👤 User Profile: $($profile.email)" "Cyan"
    }
    
    # Test features
    $features = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/features"
    if ($features) {
        Write-ColorOutput "🎯 Features: Found $($features.Count) features" "Cyan"
    }
    
    # Test schema endpoints
    $schema = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/schema/tables"
    if ($schema) {
        Write-ColorOutput "🗄️ Schema Tables: Found $($schema.Count) tables" "Cyan"
    }
}

# Function to test Business Context endpoints
function Test-BusinessContextEndpoints {
    param([string]$BaseUrl)
    
    Write-ColorOutput "`n📊 Testing Business Context Endpoints" "Magenta"
    Write-ColorOutput "=" * 50 "Magenta"
    
    # Test business metadata
    $metadata = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/business-metadata/tables"
    if ($metadata) {
        Write-ColorOutput "📋 Business Metadata: Found $($metadata.Count) tables" "Cyan"
    }
    
    # Test glossary
    $glossary = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/business-metadata/glossary"
    if ($glossary) {
        Write-ColorOutput "📖 Business Glossary: Found $($glossary.Count) terms" "Cyan"
    }
    
    # Test auto-generation status
    $autoGen = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/business-metadata/auto-generation/status"
    if ($autoGen) {
        Write-ColorOutput "🤖 Auto-generation Status: Retrieved status" "Cyan"
    }
}

# Function to test Query endpoints
function Test-QueryEndpoints {
    param([string]$BaseUrl)
    
    Write-ColorOutput "`n🔍 Testing Query Endpoints" "Magenta"
    Write-ColorOutput "=" * 50 "Magenta"
    
    # Test query suggestions
    $suggestions = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/query/suggestions"
    if ($suggestions) {
        Write-ColorOutput "💡 Query Suggestions: Found $($suggestions.Count) suggestions" "Cyan"
    }
    
    # Test query history
    $history = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/query/history?limit=5"
    if ($history) {
        Write-ColorOutput "📚 Query History: Found $($history.Count) queries" "Cyan"
    }
}

# Function to create sample cost tracking data
function Add-SampleCostData {
    param([string]$BaseUrl)

    Write-ColorOutput "`n📝 Adding Sample Cost Tracking Data" "Magenta"
    Write-ColorOutput "=" * 50 "Magenta"

    $sampleData = @{
        userId = "admin-user-001"
        queryId = "test-query-$(Get-Date -Format 'yyyyMMdd-HHmmss')"
        providerId = "openai-main"
        modelId = "gpt-4-turbo"
        cost = 0.0234
        costPerToken = 0.00003
        inputTokens = 780
        outputTokens = 156
        totalTokens = 936
        durationMs = 2340
        category = "Query"
        requestType = "sql_generation"
        department = "Analytics"
        project = "BI Dashboard"
        metadata = '{"complexity": "high", "tables": 5}'
    }

    $result = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/cost-management/track" -Method POST -Body $sampleData
    if ($result) {
        Write-ColorOutput "✅ Sample cost data added successfully" "Green"
    }
}

# Function to test specific Cost Management scenarios
function Test-CostManagementScenarios {
    param([string]$BaseUrl)

    Write-ColorOutput "`n🧪 Testing Cost Management Scenarios" "Magenta"
    Write-ColorOutput "=" * 50 "Magenta"

    # Add sample data first
    Add-SampleCostData -BaseUrl $BaseUrl

    # Test cost analytics with filters
    $analyticsFiltered = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/cost-management/analytics?department=Analytics&days=7"
    if ($analyticsFiltered) {
        Write-ColorOutput "📊 Filtered Analytics: Retrieved department-specific data" "Cyan"
    }

    # Test cost breakdown by provider
    $providerBreakdown = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/cost-management/breakdown/provider"
    if ($providerBreakdown) {
        Write-ColorOutput "🏭 Provider Breakdown: Retrieved cost by provider" "Cyan"
    }

    # Test cost breakdown by model
    $modelBreakdown = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/cost-management/breakdown/model"
    if ($modelBreakdown) {
        Write-ColorOutput "🧠 Model Breakdown: Retrieved cost by model" "Cyan"
    }

    # Test real-time cost tracking
    $realTime = Invoke-AuthenticatedRequest -Uri "$BaseUrl/api/cost-management/realtime"
    if ($realTime) {
        Write-ColorOutput "⚡ Real-time Tracking: Retrieved current usage" "Cyan"
    }
}

# Function to generate test report
function Generate-TestReport {
    param(
        [hashtable]$Results,
        [string]$OutputPath = "test-report-$(Get-Date -Format 'yyyyMMdd-HHmmss').txt"
    )

    $report = @"
BI Reporting Copilot API Test Report
Generated: $(Get-Date)
Base URL: $BaseUrl
Username: $Username

Test Results:
=============
"@

    foreach ($category in $Results.Keys) {
        $report += "`n$category`n"
        $report += "-" * $category.Length + "`n"

        foreach ($test in $Results[$category]) {
            $status = if ($test.Success) { "PASS" } else { "FAIL" }
            $report += "$status - $($test.Name)`n"
            if (-not $test.Success -and $test.Error) {
                $report += "    Error: $($test.Error)`n"
            }
        }
    }

    $report | Out-File -FilePath $OutputPath -Encoding UTF8
    Write-ColorOutput "📄 Test report saved to: $OutputPath" "Green"
}

# Main execution
function Main {
    Write-ColorOutput "🚀 BI Reporting Copilot API Endpoint Tester" "Magenta"
    Write-ColorOutput "=" * 60 "Magenta"
    Write-ColorOutput "Base URL: $BaseUrl" "Cyan"
    Write-ColorOutput "Username: $Username" "Cyan"
    Write-ColorOutput ""

    # Authenticate
    $Global:AuthToken = Get-AuthToken -BaseUrl $BaseUrl -Username $Username -Password $Password

    if (-not $Global:AuthToken) {
        Write-ColorOutput "❌ Cannot proceed without authentication" "Red"
        exit 1
    }

    # Test endpoints based on parameters
    if ($TestCostManagement -or $TestAll) {
        Test-CostManagementEndpoints -BaseUrl $BaseUrl
        Test-CostManagementScenarios -BaseUrl $BaseUrl
    }

    if ($TestAll) {
        Test-CoreEndpoints -BaseUrl $BaseUrl
        Test-LLMManagementEndpoints -BaseUrl $BaseUrl
        Test-BusinessContextEndpoints -BaseUrl $BaseUrl
        Test-QueryEndpoints -BaseUrl $BaseUrl
    }

    if (-not $TestCostManagement -and -not $TestAll) {
        Write-ColorOutput "`nUsage Examples:" "Yellow"
        Write-ColorOutput "  .\EndpointTester.ps1 -TestCostManagement    # Test only Cost Management" "Yellow"
        Write-ColorOutput "  .\EndpointTester.ps1 -TestAll               # Test all endpoints" "Yellow"
        Write-ColorOutput "  .\EndpointTester.ps1 -TestAll -Verbose      # Test all with detailed output" "Yellow"
        Write-ColorOutput ""
        Write-ColorOutput "🔧 Testing Core Endpoints by default..." "Cyan"
        Test-CoreEndpoints -BaseUrl $BaseUrl
    }

    Write-ColorOutput "`n✅ Testing completed!" "Green"
}

# Run the main function
Main
