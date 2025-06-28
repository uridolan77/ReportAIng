# Enhance all BusinessTableInfo and BusinessGlossary records
param(
    [string]$BaseUrl = "http://localhost:55244",
    [string]$Username = "admin",
    [string]$Password = "Admin123!"
)

Write-Host "Comprehensive Enhancement: Tables and Glossary" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green

# Skip certificate validation
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

# Login
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
    Write-Host "Authenticated successfully" -ForegroundColor Green
}
catch {
    Write-Host "Authentication failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

$authHeaders = @{
    "Content-Type" = "application/json"
    "Accept" = "application/json"
    "Authorization" = "Bearer $token"
}

# Phase 1: Enhance BusinessTableInfo with multiple batches
Write-Host "`nPhase 1: Enhancing BusinessTableInfo Records" -ForegroundColor Yellow
Write-Host "===========================================" -ForegroundColor Yellow

$totalEnhanced = 0
$batchNumber = 1

# Run multiple batches until no more fields are enhanced
do {
    Write-Host "`nBatch $batchNumber - Processing 20 table records..." -ForegroundColor Cyan
    
    $enhanceBody = @{
        mode = "EmptyFieldsOnly"
        batchSize = 20
        previewOnly = $false
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/enhance" -Method POST -Headers $authHeaders -Body $enhanceBody -UseBasicParsing
        
        Write-Host "Batch $batchNumber completed!" -ForegroundColor Green
        Write-Host "  Fields Enhanced: $($response.fieldsEnhanced)" -ForegroundColor White
        Write-Host "  Tables Processed: $($response.tablesProcessed)" -ForegroundColor White
        Write-Host "  Processing Time: $($response.processingTime)" -ForegroundColor White
        
        $totalEnhanced += $response.fieldsEnhanced
        $batchNumber++
        
        # If no fields were enhanced, we're done
        if ($response.fieldsEnhanced -eq 0) {
            Write-Host "  No more empty fields found - tables complete!" -ForegroundColor Yellow
            break
        }
        
        # Small delay between batches
        Start-Sleep -Seconds 1
    }
    catch {
        Write-Host "Batch $batchNumber failed: $($_.Exception.Message)" -ForegroundColor Red
        break
    }
} while ($batchNumber -le 10)  # Safety limit

Write-Host "`nBusinessTableInfo Enhancement Summary:" -ForegroundColor Green
Write-Host "  Total Fields Enhanced: $totalEnhanced" -ForegroundColor White
Write-Host "  Batches Processed: $($batchNumber - 1)" -ForegroundColor White

# Phase 2: Enhance BusinessGlossary with multiple batches
Write-Host "`nPhase 2: Enhancing BusinessGlossary Records" -ForegroundColor Yellow
Write-Host "=========================================" -ForegroundColor Yellow

$totalGlossaryEnhanced = 0
$glossaryBatchNumber = 1

# Run multiple batches until no more fields are enhanced
do {
    Write-Host "`nBatch $glossaryBatchNumber - Processing 20 glossary records..." -ForegroundColor Cyan
    
    $enhanceBody = @{
        mode = "EmptyFieldsOnly"
        batchSize = 20
        previewOnly = $false
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/enhance" -Method POST -Headers $authHeaders -Body $enhanceBody -UseBasicParsing
        
        Write-Host "Batch $glossaryBatchNumber completed!" -ForegroundColor Green
        Write-Host "  Fields Enhanced: $($response.fieldsEnhanced)" -ForegroundColor White
        Write-Host "  Glossary Terms Processed: $($response.glossaryTermsProcessed)" -ForegroundColor White
        Write-Host "  Processing Time: $($response.processingTime)" -ForegroundColor White
        
        $totalGlossaryEnhanced += $response.fieldsEnhanced
        $glossaryBatchNumber++
        
        # If no fields were enhanced, we're done
        if ($response.fieldsEnhanced -eq 0) {
            Write-Host "  No more empty fields found - glossary complete!" -ForegroundColor Yellow
            break
        }
        
        # Small delay between batches
        Start-Sleep -Seconds 1
    }
    catch {
        Write-Host "Batch $glossaryBatchNumber failed: $($_.Exception.Message)" -ForegroundColor Red
        break
    }
} while ($glossaryBatchNumber -le 10)  # Safety limit

Write-Host "`nBusinessGlossary Enhancement Summary:" -ForegroundColor Green
Write-Host "  Total Fields Enhanced: $totalGlossaryEnhanced" -ForegroundColor White
Write-Host "  Batches Processed: $($glossaryBatchNumber - 1)" -ForegroundColor White

# Phase 3: Final large batch to catch any remaining fields
Write-Host "`nPhase 3: Final Comprehensive Enhancement" -ForegroundColor Yellow
Write-Host "=======================================" -ForegroundColor Yellow

$finalEnhanceBody = @{
    mode = "EmptyFieldsOnly"
    batchSize = 50
    previewOnly = $false
} | ConvertTo-Json

try {
    $finalResponse = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/enhance" -Method POST -Headers $authHeaders -Body $finalEnhanceBody -UseBasicParsing
    
    Write-Host "Final enhancement completed!" -ForegroundColor Green
    Write-Host "  Fields Enhanced: $($finalResponse.fieldsEnhanced)" -ForegroundColor White
    Write-Host "  Tables Processed: $($finalResponse.tablesProcessed)" -ForegroundColor White
    Write-Host "  Glossary Terms Processed: $($finalResponse.glossaryTermsProcessed)" -ForegroundColor White
    Write-Host "  Processing Time: $($finalResponse.processingTime)" -ForegroundColor White
    
    $totalEnhanced += $finalResponse.fieldsEnhanced
    $totalGlossaryEnhanced += $finalResponse.fieldsEnhanced
}
catch {
    Write-Host "Final enhancement failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Summary
Write-Host "`nFINAL SUMMARY" -ForegroundColor Green
Write-Host "=============" -ForegroundColor Green
Write-Host "Total Table Fields Enhanced: $totalEnhanced" -ForegroundColor White
Write-Host "Total Glossary Fields Enhanced: $totalGlossaryEnhanced" -ForegroundColor White
Write-Host "Grand Total Fields Enhanced: $($totalEnhanced + $totalGlossaryEnhanced)" -ForegroundColor Cyan

if (($totalEnhanced + $totalGlossaryEnhanced) -gt 0) {
    Write-Host "`nSUCCESS! Tables and Glossary enhancement complete!" -ForegroundColor Green
    Write-Host "Ready to proceed with BusinessColumnInfo enhancement." -ForegroundColor Cyan
} else {
    Write-Host "`nAll table and glossary fields were already populated!" -ForegroundColor Yellow
}

Write-Host "`nEnhancement Complete!" -ForegroundColor Green
