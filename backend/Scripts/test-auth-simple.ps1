# Simple test for authentication and metadata enhancement
param(
    [string]$BaseUrl = "http://localhost:55244",
    [string]$Username = "admin",
    [string]$Password = "Admin123!"
)

Write-Host "Testing Metadata Enhancement Service" -ForegroundColor Green
Write-Host "Base URL: $BaseUrl" -ForegroundColor Cyan

# Skip certificate validation
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

# Step 1: Login
Write-Host "Step 1: Login" -ForegroundColor Yellow
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
    Write-Host "Login successful!" -ForegroundColor Green
    Write-Host "User ID: $($authResponse.userId)" -ForegroundColor Cyan
    
    $token = $authResponse.accessToken
}
catch {
    Write-Host "Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Test with authentication
$authHeaders = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
    "Authorization" = "Bearer $token"
}

Write-Host "Step 2: Testing MetadataEnhancement Service" -ForegroundColor Yellow

# Test Status
Write-Host "Testing Status endpoint..." -ForegroundColor Cyan
try {
    $statusResponse = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/status" -Method GET -Headers $authHeaders -UseBasicParsing
    Write-Host "Status endpoint works!" -ForegroundColor Green
    Write-Host "Service Status: $($statusResponse.serviceStatus)" -ForegroundColor White
}
catch {
    Write-Host "Status test failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Test Enhancement
Write-Host "Testing Enhancement endpoint..." -ForegroundColor Cyan
$enhanceBody = @{
    mode = "EmptyFieldsOnly"
    batchSize = 5
    previewOnly = $false
} | ConvertTo-Json

try {
    $enhanceResponse = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/enhance" -Method POST -Headers $authHeaders -Body $enhanceBody -UseBasicParsing
    Write-Host "Enhancement endpoint works!" -ForegroundColor Green
    Write-Host "Fields enhanced: $($enhanceResponse.fieldsEnhanced)" -ForegroundColor White
    Write-Host "Success: $($enhanceResponse.success)" -ForegroundColor White
    
    if ($enhanceResponse.fieldsEnhanced -gt 0) {
        Write-Host "SUCCESS! Enhanced $($enhanceResponse.fieldsEnhanced) fields!" -ForegroundColor Green
    } else {
        Write-Host "No fields enhanced (may already be populated)" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "Enhancement test failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Testing Complete!" -ForegroundColor Green
