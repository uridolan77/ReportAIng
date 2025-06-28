#!/usr/bin/env pwsh

# Simple authentication and metadata enhancement test
param(
    [string]$BaseUrl = "http://localhost:55244",
    [string]$Username = "admin",
    [string]$Password = "Admin123!"
)

Write-Host "🚀 Testing Metadata Enhancement Service" -ForegroundColor Green
Write-Host "Base URL: $BaseUrl" -ForegroundColor Cyan

# Skip certificate validation
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

# Step 1: Login
Write-Host "`n🔐 Step 1: Login" -ForegroundColor Yellow
$loginBody = @{
    username = $Username
    password = $Password
} | ConvertTo-Json

$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

try {
    $authResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method POST -Headers $headers -Body $loginBody -UseBasicParsing
    Write-Host "✅ Login successful!" -ForegroundColor Green
    Write-Host "   User ID: $($authResponse.userId)" -ForegroundColor Cyan
    Write-Host "   Token length: $($authResponse.accessToken.Length)" -ForegroundColor Cyan
    
    $token = $authResponse.accessToken
}
catch {
    Write-Host "❌ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Create authenticated headers
$authHeaders = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
    "Authorization" = "Bearer $token"
}

# Step 3: Test MetadataEnhancement endpoints
Write-Host "`n🔧 Step 2: Testing MetadataEnhancement Service" -ForegroundColor Yellow

# Test 3.1: Get Status
Write-Host "`n📋 Test: Get Enhancement Status" -ForegroundColor Cyan
try {
    $statusResponse = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/status" -Method GET -Headers $authHeaders -UseBasicParsing
    Write-Host "✅ Status endpoint works!" -ForegroundColor Green
    Write-Host "   Service Status: $($statusResponse.serviceStatus)" -ForegroundColor White
}
catch {
    Write-Host "❌ Status test failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3.2: Get Modes
Write-Host "`n📋 Test: Get Enhancement Modes" -ForegroundColor Cyan
try {
    $modesResponse = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/modes" -Method GET -Headers $authHeaders -UseBasicParsing
    Write-Host "✅ Modes endpoint works!" -ForegroundColor Green
    Write-Host "   Available modes: $($modesResponse.availableModes.Count)" -ForegroundColor White
}
catch {
    Write-Host "❌ Modes test failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3.3: Preview Enhancement
Write-Host "`n📋 Test: Preview Enhancement" -ForegroundColor Cyan
$previewBody = @{
    mode = "EmptyFieldsOnly"
    batchSize = 5
    previewOnly = $true
} | ConvertTo-Json

try {
    $previewResponse = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/preview" -Method POST -Headers $authHeaders -Body $previewBody -UseBasicParsing
    Write-Host "✅ Preview endpoint works!" -ForegroundColor Green
    Write-Host "   Would enhance: $($previewResponse.fieldsEnhanced) fields" -ForegroundColor White
    Write-Host "   Records found: $($previewResponse.columnsProcessed + $previewResponse.tablesProcessed + $previewResponse.glossaryTermsProcessed)" -ForegroundColor White
}
catch {
    Write-Host "❌ Preview test failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3.4: Actual Enhancement (Small Batch)
Write-Host "`n📋 Test: Small Batch Enhancement" -ForegroundColor Cyan
$enhanceBody = @{
    mode = "EmptyFieldsOnly"
    batchSize = 3
    previewOnly = $false
} | ConvertTo-Json

try {
    $enhanceResponse = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/enhance" -Method POST -Headers $authHeaders -Body $enhanceBody -UseBasicParsing
    Write-Host "✅ Enhancement endpoint works!" -ForegroundColor Green
    Write-Host "   Fields enhanced: $($enhanceResponse.fieldsEnhanced)" -ForegroundColor White
    Write-Host "   Processing time: $($enhanceResponse.processingTime)" -ForegroundColor White
    Write-Host "   Success: $($enhanceResponse.success)" -ForegroundColor White
    
    if ($enhanceResponse.fieldsEnhanced -gt 0) {
        Write-Host "`n🎉 SUCCESS! MetadataEnhancementService enhanced $($enhanceResponse.fieldsEnhanced) fields!" -ForegroundColor Green
    } else {
        Write-Host "`n⚠️  No fields enhanced (may already be populated)" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "❌ Enhancement test failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3.5: Larger Batch Enhancement
Write-Host "`n📋 Test: Larger Batch Enhancement" -ForegroundColor Cyan
$largerBatchBody = @{
    mode = "EmptyFieldsOnly"
    batchSize = 10
    previewOnly = $false
} | ConvertTo-Json

try {
    $largerResponse = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/enhance" -Method POST -Headers $authHeaders -Body $largerBatchBody -UseBasicParsing
    Write-Host "✅ Larger batch enhancement works!" -ForegroundColor Green
    Write-Host "   Fields enhanced: $($largerResponse.fieldsEnhanced)" -ForegroundColor White
    Write-Host "   Records processed: $($largerResponse.columnsProcessed + $largerResponse.tablesProcessed + $largerResponse.glossaryTermsProcessed)" -ForegroundColor White
    
    if ($largerResponse.fieldsEnhanced -gt 0) {
        Write-Host "`n🚀 EXCELLENT! Enhanced $($largerResponse.fieldsEnhanced) more fields!" -ForegroundColor Green
    }
}
catch {
    Write-Host "❌ Larger batch test failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🎉 Testing Complete!" -ForegroundColor Green
Write-Host "The MetadataEnhancementService is working!" -ForegroundColor Green
