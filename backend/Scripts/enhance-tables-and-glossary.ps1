# Comprehensive enhancement for BusinessTableInfo and BusinessGlossary tables
param(
    [string]$BaseUrl = "http://localhost:55244",
    [string]$Username = "admin",
    [string]$Password = "Admin123!"
)

Write-Host "🚀 Comprehensive Enhancement: Tables and Glossary" -ForegroundColor Green
Write-Host "=================================================" -ForegroundColor Green

# Skip certificate validation
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

# Step 1: Login
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
    $token = $authResponse.accessToken
    Write-Host "✅ Authenticated successfully" -ForegroundColor Green
}
catch {
    Write-Host "❌ Authentication failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$authHeaders = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
    "Authorization" = "Bearer $token"
}

# Step 2: Enhanced BusinessTableInfo with larger batches
Write-Host "`n📊 Phase 1: Enhancing BusinessTableInfo Records" -ForegroundColor Magenta
Write-Host "=============================================" -ForegroundColor Magenta

$batchSizes = @(20, 30, 50)
$totalTablesEnhanced = 0

foreach ($batchSize in $batchSizes) {
    Write-Host "`n🔧 Processing batch of $batchSize tables..." -ForegroundColor Yellow
    
    $enhanceBody = @{
        mode = "EmptyFieldsOnly"
        batchSize = $batchSize
        previewOnly = $false
        targetTables = @()  # Empty array means all tables
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/enhance" -Method POST -Headers $authHeaders -Body $enhanceBody -UseBasicParsing
        
        Write-Host "✅ Batch completed!" -ForegroundColor Green
        Write-Host "   Fields Enhanced: $($response.fieldsEnhanced)" -ForegroundColor Cyan
        Write-Host "   Tables Processed: $($response.tablesProcessed)" -ForegroundColor Cyan
        Write-Host "   Processing Time: $($response.processingTime)" -ForegroundColor Cyan
        
        $totalTablesEnhanced += $response.tablesProcessed
        
        if ($response.fieldsEnhanced -eq 0) {
            Write-Host "   No more empty fields found in tables - moving to next phase" -ForegroundColor Yellow
            break
        }
        
        # Small delay between batches
        Start-Sleep -Seconds 2
    }
    catch {
        Write-Host "❌ Batch failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n📈 BusinessTableInfo Enhancement Summary:" -ForegroundColor Green
Write-Host "   Total Tables Processed: $totalTablesEnhanced" -ForegroundColor White

# Step 3: Enhanced BusinessGlossary with larger batches
Write-Host "`n📚 Phase 2: Enhancing BusinessGlossary Records" -ForegroundColor Magenta
Write-Host "===========================================" -ForegroundColor Magenta

$totalGlossaryEnhanced = 0

foreach ($batchSize in $batchSizes) {
    Write-Host "`n🔧 Processing batch of $batchSize glossary terms..." -ForegroundColor Yellow
    
    $enhanceBody = @{
        mode = "EmptyFieldsOnly"
        batchSize = $batchSize
        previewOnly = $false
        targetTables = @()  # Empty array means all tables
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/enhance" -Method POST -Headers $authHeaders -Body $enhanceBody -UseBasicParsing
        
        Write-Host "✅ Batch completed!" -ForegroundColor Green
        Write-Host "   Fields Enhanced: $($response.fieldsEnhanced)" -ForegroundColor Cyan
        Write-Host "   Glossary Terms Processed: $($response.glossaryTermsProcessed)" -ForegroundColor Cyan
        Write-Host "   Processing Time: $($response.processingTime)" -ForegroundColor Cyan
        
        $totalGlossaryEnhanced += $response.glossaryTermsProcessed
        
        if ($response.fieldsEnhanced -eq 0) {
            Write-Host "   No more empty fields found in glossary - enhancement complete" -ForegroundColor Yellow
            break
        }
        
        # Small delay between batches
        Start-Sleep -Seconds 2
    }
    catch {
        Write-Host "❌ Batch failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "`n📈 BusinessGlossary Enhancement Summary:" -ForegroundColor Green
Write-Host "   Total Glossary Terms Processed: $totalGlossaryEnhanced" -ForegroundColor White

# Step 4: Final comprehensive enhancement to catch any remaining fields
Write-Host "`n🎯 Phase 3: Final Comprehensive Enhancement" -ForegroundColor Magenta
Write-Host "=========================================" -ForegroundColor Magenta

$finalEnhanceBody = @{
    mode = "EmptyFieldsOnly"
    batchSize = 100
    previewOnly = $false
} | ConvertTo-Json

try {
    $finalResponse = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/enhance" -Method POST -Headers $authHeaders -Body $finalEnhanceBody -UseBasicParsing
    
    Write-Host "✅ Final enhancement completed!" -ForegroundColor Green
    Write-Host "   Fields Enhanced: $($finalResponse.fieldsEnhanced)" -ForegroundColor Cyan
    Write-Host "   Total Records Processed: $($finalResponse.columnsProcessed + $finalResponse.tablesProcessed + $finalResponse.glossaryTermsProcessed)" -ForegroundColor Cyan
    Write-Host "   Processing Time: $($finalResponse.processingTime)" -ForegroundColor Cyan
}
catch {
    Write-Host "❌ Final enhancement failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 5: Get final status
Write-Host "`n📊 Final Status Check" -ForegroundColor Magenta
Write-Host "===================" -ForegroundColor Magenta

try {
    $statusResponse = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/status" -Method GET -Headers $authHeaders -UseBasicParsing
    Write-Host "✅ Service Status: $($statusResponse.serviceStatus)" -ForegroundColor Green
    
    if ($statusResponse.totalEnhancements) {
        Write-Host "   Total Enhancements: $($statusResponse.totalEnhancements)" -ForegroundColor Cyan
    }
}
catch {
    Write-Host "❌ Could not get final status: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n🎉 Tables and Glossary Enhancement Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green
Write-Host "✅ BusinessTableInfo tables have been enhanced" -ForegroundColor White
Write-Host "✅ BusinessGlossary terms have been enhanced" -ForegroundColor White
Write-Host "`n🔄 Ready to proceed with BusinessColumnInfo enhancement!" -ForegroundColor Cyan
