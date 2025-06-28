#!/usr/bin/env pwsh

# Test script for MetadataEnhancementService API endpoints
# This script tests the new metadata enhancement functionality

param(
    [string]$BaseUrl = "https://localhost:7001",
    [string]$AuthToken = ""
)

Write-Host "🚀 Testing Metadata Enhancement Service API" -ForegroundColor Green
Write-Host "Base URL: $BaseUrl" -ForegroundColor Cyan

# Skip certificate validation for local testing
if (-not ([System.Management.Automation.PSTypeName]'ServerCertificateValidationCallback').Type) {
    $certCallback = @"
        using System;
        using System.Net;
        using System.Net.Security;
        using System.Security.Cryptography.X509Certificates;
        public class ServerCertificateValidationCallback
        {
            public static void Ignore()
            {
                if(ServicePointManager.ServerCertificateValidationCallback ==null)
                {
                    ServicePointManager.ServerCertificateValidationCallback += 
                        delegate
                        (
                            Object obj, 
                            X509Certificate certificate, 
                            X509Chain chain, 
                            SslPolicyErrors errors
                        )
                        {
                            return true;
                        };
                }
            }
        }
"@
    Add-Type $certCallback
}
[ServerCertificateValidationCallback]::Ignore()

# Common headers
$headers = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
}

if ($AuthToken) {
    $headers["Authorization"] = "Bearer $AuthToken"
}

function Test-Endpoint {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [string]$Description
    )
    
    Write-Host "`n📋 Testing: $Description" -ForegroundColor Yellow
    Write-Host "   $Method $Endpoint" -ForegroundColor Gray
    
    try {
        $params = @{
            Uri = "$BaseUrl$Endpoint"
            Method = $Method
            Headers = $headers
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

# Test 1: Get Enhancement Status
Write-Host "`n🔍 Test 1: Get Enhancement Status" -ForegroundColor Magenta
$status = Test-Endpoint -Method "GET" -Endpoint "/api/MetadataEnhancement/status" -Description "Get service status"

# Test 2: Get Enhancement Modes
Write-Host "`n🔍 Test 2: Get Enhancement Modes" -ForegroundColor Magenta
$modes = Test-Endpoint -Method "GET" -Endpoint "/api/MetadataEnhancement/modes" -Description "Get available enhancement modes"

# Test 3: Preview Enhancement (EmptyFieldsOnly)
Write-Host "`n🔍 Test 3: Preview Enhancement" -ForegroundColor Magenta
$previewRequest = @{
    mode = "EmptyFieldsOnly"
    batchSize = 5
    previewOnly = $true
}
$preview = Test-Endpoint -Method "POST" -Endpoint "/api/MetadataEnhancement/preview" -Body $previewRequest -Description "Preview metadata enhancement"

# Test 4: Actual Enhancement (Small Batch)
Write-Host "`n🔍 Test 4: Small Batch Enhancement" -ForegroundColor Magenta
$enhanceRequest = @{
    mode = "EmptyFieldsOnly"
    batchSize = 3
    previewOnly = $false
}
$enhancement = Test-Endpoint -Method "POST" -Endpoint "/api/MetadataEnhancement/enhance" -Body $enhanceRequest -Description "Enhance metadata (small batch)"

# Test 5: Selective Enhancement
Write-Host "`n🔍 Test 5: Selective Enhancement" -ForegroundColor Magenta
$selectiveRequest = @{
    mode = "Selective"
    targetFields = @("SemanticContext", "AnalyticalContext")
    batchSize = 2
    previewOnly = $false
}
$selective = Test-Endpoint -Method "POST" -Endpoint "/api/MetadataEnhancement/enhance" -Body $selectiveRequest -Description "Selective field enhancement"

# Summary
Write-Host "`n📊 Test Summary" -ForegroundColor Green
Write-Host "===============" -ForegroundColor Green

$tests = @(
    @{ Name = "Get Status"; Result = if ($status) { "✅ PASS" } else { "❌ FAIL" } }
    @{ Name = "Get Modes"; Result = if ($modes) { "✅ PASS" } else { "❌ FAIL" } }
    @{ Name = "Preview Enhancement"; Result = if ($preview) { "✅ PASS" } else { "❌ FAIL" } }
    @{ Name = "Small Batch Enhancement"; Result = if ($enhancement) { "✅ PASS" } else { "❌ FAIL" } }
    @{ Name = "Selective Enhancement"; Result = if ($selective) { "✅ PASS" } else { "❌ FAIL" } }
)

foreach ($test in $tests) {
    Write-Host "$($test.Name): $($test.Result)" -ForegroundColor $(if ($test.Result.Contains("✅")) { "Green" } else { "Red" })
}

# Performance Summary
if ($enhancement -and $enhancement.success) {
    Write-Host "`n⚡ Performance Metrics" -ForegroundColor Cyan
    Write-Host "=====================" -ForegroundColor Cyan
    Write-Host "Fields Enhanced: $($enhancement.fieldsEnhanced)" -ForegroundColor White
    Write-Host "Records Processed: $($enhancement.columnsProcessed + $enhancement.tablesProcessed + $enhancement.glossaryTermsProcessed)" -ForegroundColor White
    Write-Host "Processing Time: $($enhancement.processingTime)" -ForegroundColor White
    Write-Host "Total Cost: $($enhancement.totalCost)" -ForegroundColor White
}

Write-Host "`n🎉 Metadata Enhancement Service Testing Complete!" -ForegroundColor Green

# Usage examples
Write-Host "`n📖 Usage Examples" -ForegroundColor Cyan
Write-Host "=================" -ForegroundColor Cyan
Write-Host "Basic usage:" -ForegroundColor White
Write-Host "  .\test-metadata-enhancement-service.ps1" -ForegroundColor Gray
Write-Host ""
Write-Host "With custom URL:" -ForegroundColor White
Write-Host "  .\test-metadata-enhancement-service.ps1 -BaseUrl 'https://myserver:5001'" -ForegroundColor Gray
Write-Host ""
Write-Host "With authentication:" -ForegroundColor White
Write-Host "  .\test-metadata-enhancement-service.ps1 -AuthToken 'your-jwt-token'" -ForegroundColor Gray
