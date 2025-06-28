#!/usr/bin/env pwsh

# Simple test for MetadataEnhancementService API
param(
    [string]$BaseUrl = "http://localhost:55244"
)

Write-Host "🚀 Testing Metadata Enhancement Service" -ForegroundColor Green
Write-Host "Base URL: $BaseUrl" -ForegroundColor Cyan

# Skip certificate validation
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

Write-Host "`n📋 Test 1: Get Enhancement Status" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/status" -Method GET -Headers $headers -UseBasicParsing
    Write-Host "✅ Success!" -ForegroundColor Green
    Write-Host "Response: $($response | ConvertTo-Json -Depth 2)" -ForegroundColor White
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n📋 Test 2: Get Enhancement Modes" -ForegroundColor Yellow
try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/modes" -Method GET -Headers $headers -UseBasicParsing
    Write-Host "✅ Success!" -ForegroundColor Green
    Write-Host "Response: $($response | ConvertTo-Json -Depth 2)" -ForegroundColor White
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n📋 Test 3: Preview Enhancement" -ForegroundColor Yellow
$previewBody = @{
    mode = "EmptyFieldsOnly"
    batchSize = 5
    previewOnly = $true
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/preview" -Method POST -Headers $headers -Body $previewBody -UseBasicParsing
    Write-Host "✅ Success!" -ForegroundColor Green
    Write-Host "Response: $($response | ConvertTo-Json -Depth 2)" -ForegroundColor White
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n📋 Test 4: Small Enhancement (Actual)" -ForegroundColor Yellow
$enhanceBody = @{
    mode = "EmptyFieldsOnly"
    batchSize = 3
    previewOnly = $false
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/enhance" -Method POST -Headers $headers -Body $enhanceBody -UseBasicParsing
    Write-Host "✅ Success!" -ForegroundColor Green
    Write-Host "Response: $($response | ConvertTo-Json -Depth 2)" -ForegroundColor White
    
    if ($response.success) {
        Write-Host "`n⚡ Enhancement Results:" -ForegroundColor Cyan
        Write-Host "  Fields Enhanced: $($response.fieldsEnhanced)" -ForegroundColor White
        Write-Host "  Records Processed: $($response.columnsProcessed + $response.tablesProcessed + $response.glossaryTermsProcessed)" -ForegroundColor White
        Write-Host "  Processing Time: $($response.processingTime)" -ForegroundColor White
    }
}
catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🎉 Testing Complete!" -ForegroundColor Green
