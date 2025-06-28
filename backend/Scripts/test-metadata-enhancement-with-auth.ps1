#!/usr/bin/env pwsh

# Comprehensive test for MetadataEnhancementService with authentication
param(
    [string]$BaseUrl = "http://localhost:55244",
    [string]$Username = "admin",
    [string]$Password = "Admin123!"
)

Write-Host "🚀 Testing Metadata Enhancement Service with Authentication" -ForegroundColor Green
Write-Host "Base URL: $BaseUrl" -ForegroundColor Cyan
Write-Host "Username: $Username" -ForegroundColor Cyan

# Skip certificate validation
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

function Test-Endpoint {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [string]$Description,
        [hashtable]$CustomHeaders = $headers
    )
    
    Write-Host "`n📋 Testing: $Description" -ForegroundColor Yellow
    Write-Host "   $Method $Endpoint" -ForegroundColor Gray
    
    try {
        $params = @{
            Uri = "$BaseUrl$Endpoint"
            Method = $Method
            Headers = $CustomHeaders
            UseBasicParsing = $true
        }
        
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
            Write-Host "   Request Body: $($params.Body)" -ForegroundColor Gray
        }
        
        $response = Invoke-RestMethod @params
        
        Write-Host "   ✅ Success!" -ForegroundColor Green
        Write-Host "   Response: $($response | ConvertTo-Json -Depth 3)" -ForegroundColor White
        
        return $response
    }
    catch {
        Write-Host "   ❌ Error: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            $statusCode = $_.Exception.Response.StatusCode
            Write-Host "   Status Code: $statusCode" -ForegroundColor Red
        }
        return $null
    }
}

# Step 1: Login to get authentication token
Write-Host "`n🔐 Step 1: Authentication" -ForegroundColor Magenta
$loginBody = @{
    username = $Username
    password = $Password
}

$authResult = Test-Endpoint -Method "POST" -Endpoint "/api/auth/login" -Body $loginBody -Description "Login to get JWT token"

if (-not $authResult -or -not $authResult.success -or -not $authResult.accessToken) {
    Write-Host "❌ Authentication failed! Cannot proceed with tests." -ForegroundColor Red
    exit 1
}

$token = $authResult.accessToken
Write-Host "✅ Authentication successful! Token obtained." -ForegroundColor Green
Write-Host "   Token length: $($token.Length) characters" -ForegroundColor Cyan
Write-Host "   User ID: $($authResult.userId)" -ForegroundColor Cyan
Write-Host "   Expires: $($authResult.expiresAt)" -ForegroundColor Cyan

# Create authenticated headers
$authHeaders = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
    "Authorization" = "Bearer $token"
}

# Step 2: Test MetadataEnhancementService endpoints
Write-Host "`n🔧 Step 2: MetadataEnhancementService Tests" -ForegroundColor Magenta

# Test 2.1: Get Enhancement Status
Write-Host "`n🔍 Test 2.1: Get Enhancement Status" -ForegroundColor Yellow
$status = Test-Endpoint -Method "GET" -Endpoint "/api/MetadataEnhancement/status" -Description "Get service status" -CustomHeaders $authHeaders

# Test 2.2: Get Enhancement Modes
Write-Host "`n🔍 Test 2.2: Get Enhancement Modes" -ForegroundColor Yellow
$modes = Test-Endpoint -Method "GET" -Endpoint "/api/MetadataEnhancement/modes" -Description "Get available enhancement modes" -CustomHeaders $authHeaders

# Test 2.3: Preview Enhancement (Safe Test)
Write-Host "`n🔍 Test 2.3: Preview Enhancement" -ForegroundColor Yellow
$previewRequest = @{
    mode = "EmptyFieldsOnly"
    batchSize = 5
    previewOnly = $true
}
$preview = Test-Endpoint -Method "POST" -Endpoint "/api/MetadataEnhancement/preview" -Body $previewRequest -Description "Preview metadata enhancement" -CustomHeaders $authHeaders

# Test 2.4: Small Batch Enhancement (Actual)
Write-Host "`n🔍 Test 2.4: Small Batch Enhancement" -ForegroundColor Yellow
$enhanceRequest = @{
    mode = "EmptyFieldsOnly"
    batchSize = 3
    previewOnly = $false
}
$enhancement = Test-Endpoint -Method "POST" -Endpoint "/api/MetadataEnhancement/enhance" -Body $enhanceRequest -Description "Enhance metadata (small batch)" -CustomHeaders $authHeaders

# Test 2.5: Selective Enhancement
Write-Host "`n🔍 Test 2.5: Selective Enhancement" -ForegroundColor Yellow
$selectiveRequest = @{
    mode = "Selective"
    targetFields = @("SemanticContext", "AnalyticalContext")
    batchSize = 2
    previewOnly = $false
}
$selective = Test-Endpoint -Method "POST" -Endpoint "/api/MetadataEnhancement/enhance" -Body $selectiveRequest -Description "Selective field enhancement" -CustomHeaders $authHeaders

# Test 2.6: Larger Batch Enhancement
Write-Host "`n🔍 Test 2.6: Larger Batch Enhancement" -ForegroundColor Yellow
$largerBatchRequest = @{
    mode = "EmptyFieldsOnly"
    batchSize = 10
    previewOnly = $false
}
$largerBatch = Test-Endpoint -Method "POST" -Endpoint "/api/MetadataEnhancement/enhance" -Body $largerBatchRequest -Description "Larger batch enhancement" -CustomHeaders $authHeaders

# Step 3: Summary and Results
Write-Host "`n📊 Step 3: Test Summary" -ForegroundColor Magenta
Write-Host "========================" -ForegroundColor Magenta

$tests = @(
    @{ Name = "Authentication"; Result = if ($authResult) { "✅ PASS" } else { "❌ FAIL" } }
    @{ Name = "Get Status"; Result = if ($status) { "✅ PASS" } else { "❌ FAIL" } }
    @{ Name = "Get Modes"; Result = if ($modes) { "✅ PASS" } else { "❌ FAIL" } }
    @{ Name = "Preview Enhancement"; Result = if ($preview) { "✅ PASS" } else { "❌ FAIL" } }
    @{ Name = "Small Batch Enhancement"; Result = if ($enhancement) { "✅ PASS" } else { "❌ FAIL" } }
    @{ Name = "Selective Enhancement"; Result = if ($selective) { "✅ PASS" } else { "❌ FAIL" } }
    @{ Name = "Larger Batch Enhancement"; Result = if ($largerBatch) { "✅ PASS" } else { "❌ FAIL" } }
)

foreach ($test in $tests) {
    Write-Host "$($test.Name): $($test.Result)" -ForegroundColor $(if ($test.Result.Contains("✅")) { "Green" } else { "Red" })
}

# Performance Summary
Write-Host "`n⚡ Performance Metrics" -ForegroundColor Cyan
Write-Host "=====================" -ForegroundColor Cyan

$totalFieldsEnhanced = 0
$totalRecordsProcessed = 0
$totalProcessingTime = [TimeSpan]::Zero

foreach ($result in @($enhancement, $selective, $largerBatch)) {
    if ($result -and $result.success) {
        $totalFieldsEnhanced += $result.fieldsEnhanced
        $totalRecordsProcessed += ($result.columnsProcessed + $result.tablesProcessed + $result.glossaryTermsProcessed)
        if ($result.processingTime) {
            $totalProcessingTime = $totalProcessingTime.Add([TimeSpan]::Parse($result.processingTime))
        }
    }
}

Write-Host "Total Fields Enhanced: $totalFieldsEnhanced" -ForegroundColor White
Write-Host "Total Records Processed: $totalRecordsProcessed" -ForegroundColor White
Write-Host "Total Processing Time: $totalProcessingTime" -ForegroundColor White

if ($totalFieldsEnhanced -gt 0) {
    Write-Host "`n🎉 SUCCESS: MetadataEnhancementService is working!" -ForegroundColor Green
    Write-Host "   The service successfully enhanced $totalFieldsEnhanced semantic fields" -ForegroundColor Green
    Write-Host "   across $totalRecordsProcessed database records." -ForegroundColor Green
} else {
    Write-Host "`n⚠️  No fields were enhanced. This could mean:" -ForegroundColor Yellow
    Write-Host "   - All semantic fields are already populated" -ForegroundColor Yellow
    Write-Host "   - No records match the enhancement criteria" -ForegroundColor Yellow
    Write-Host "   - The database tables are empty" -ForegroundColor Yellow
}

Write-Host "`n🎯 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Check the database to see the enhanced semantic fields" -ForegroundColor White
Write-Host "2. Run larger batches to enhance more records" -ForegroundColor White
Write-Host "3. Integrate with actual LLM services for intelligent content generation" -ForegroundColor White

Write-Host "`n🎉 Metadata Enhancement Service Testing Complete!" -ForegroundColor Green
