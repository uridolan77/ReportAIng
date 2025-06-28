# Comprehensive enhancement for all BusinessColumnInfo records
param(
    [string]$BaseUrl = "http://localhost:55244",
    [string]$Username = "admin",
    [string]$Password = "Admin123!"
)

Write-Host "COMPREHENSIVE ENHANCEMENT: BusinessColumnInfo" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Green

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

# Phase 1: Multiple batches to enhance all BusinessColumnInfo records
Write-Host "`nPhase 1: Enhancing BusinessColumnInfo Records" -ForegroundColor Yellow
Write-Host "===========================================" -ForegroundColor Yellow

$totalColumnsEnhanced = 0
$totalFieldsEnhanced = 0
$batchNumber = 1
$maxBatches = 20  # Safety limit for large datasets

# Run multiple batches until no more fields are enhanced
do {
    Write-Host "`nBatch $batchNumber - Processing 25 column records..." -ForegroundColor Cyan
    
    $enhanceBody = @{
        mode = "EmptyFieldsOnly"
        batchSize = 25
        previewOnly = $false
    } | ConvertTo-Json
    
    try {
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/enhance" -Method POST -Headers $authHeaders -Body $enhanceBody -UseBasicParsing
        
        Write-Host "Batch $batchNumber completed!" -ForegroundColor Green
        Write-Host "  Fields Enhanced: $($response.fieldsEnhanced)" -ForegroundColor White
        Write-Host "  Columns Processed: $($response.columnsProcessed)" -ForegroundColor White
        Write-Host "  Processing Time: $($response.processingTime)" -ForegroundColor White
        
        $totalFieldsEnhanced += $response.fieldsEnhanced
        $totalColumnsEnhanced += $response.columnsProcessed
        $batchNumber++
        
        # If no fields were enhanced, we're done
        if ($response.fieldsEnhanced -eq 0) {
            Write-Host "  No more empty fields found - columns complete!" -ForegroundColor Yellow
            break
        }
        
        # Small delay between batches to avoid overwhelming the system
        Start-Sleep -Seconds 1
    }
    catch {
        Write-Host "Batch $batchNumber failed: $($_.Exception.Message)" -ForegroundColor Red
        break
    }
} while ($batchNumber -le $maxBatches)

Write-Host "`nBusinessColumnInfo Enhancement Summary:" -ForegroundColor Green
Write-Host "  Total Fields Enhanced: $totalFieldsEnhanced" -ForegroundColor White
Write-Host "  Total Columns Processed: $totalColumnsEnhanced" -ForegroundColor White
Write-Host "  Batches Processed: $($batchNumber - 1)" -ForegroundColor White

# Phase 2: Larger batch to catch any remaining fields
Write-Host "`nPhase 2: Final Large Batch Enhancement" -ForegroundColor Yellow
Write-Host "====================================" -ForegroundColor Yellow

$finalEnhanceBody = @{
    mode = "EmptyFieldsOnly"
    batchSize = 100
    previewOnly = $false
} | ConvertTo-Json

try {
    $finalResponse = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/enhance" -Method POST -Headers $authHeaders -Body $finalEnhanceBody -UseBasicParsing
    
    Write-Host "Final large batch completed!" -ForegroundColor Green
    Write-Host "  Fields Enhanced: $($finalResponse.fieldsEnhanced)" -ForegroundColor White
    Write-Host "  Columns Processed: $($finalResponse.columnsProcessed)" -ForegroundColor White
    Write-Host "  Processing Time: $($finalResponse.processingTime)" -ForegroundColor White
    
    $totalFieldsEnhanced += $finalResponse.fieldsEnhanced
    $totalColumnsEnhanced += $finalResponse.columnsProcessed
}
catch {
    Write-Host "Final enhancement failed: $($_.Exception.Message)" -ForegroundColor Red
}

# Phase 3: Selective enhancement for specific fields
Write-Host "`nPhase 3: Selective Field Enhancement" -ForegroundColor Yellow
Write-Host "===================================" -ForegroundColor Yellow

$selectiveFields = @(
    @("SemanticContext", "AnalyticalContext"),
    @("SemanticRelevanceScore", "ConceptualRelationships"),
    @("DomainSpecificTerms", "QueryIntentMapping"),
    @("BusinessQuestionTypes", "SemanticSynonyms"),
    @("BusinessMetrics", "LLMPromptHints"),
    @("VectorSearchTags")
)

foreach ($fieldGroup in $selectiveFields) {
    Write-Host "`nSelective enhancement for: $($fieldGroup -join ', ')" -ForegroundColor Cyan
    
    $selectiveBody = @{
        mode = "Selective"
        targetFields = $fieldGroup
        batchSize = 30
        previewOnly = $false
    } | ConvertTo-Json
    
    try {
        $selectiveResponse = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/enhance" -Method POST -Headers $authHeaders -Body $selectiveBody -UseBasicParsing
        
        Write-Host "  Fields Enhanced: $($selectiveResponse.fieldsEnhanced)" -ForegroundColor White
        Write-Host "  Columns Processed: $($selectiveResponse.columnsProcessed)" -ForegroundColor White
        
        $totalFieldsEnhanced += $selectiveResponse.fieldsEnhanced
        
        if ($selectiveResponse.fieldsEnhanced -eq 0) {
            Write-Host "  No empty fields found for this group" -ForegroundColor Yellow
        }
        
        Start-Sleep -Seconds 1
    }
    catch {
        Write-Host "  Selective enhancement failed: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Phase 4: Final status check
Write-Host "`nPhase 4: Final Status Check" -ForegroundColor Yellow
Write-Host "==========================" -ForegroundColor Yellow

try {
    $statusResponse = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/status" -Method GET -Headers $authHeaders -UseBasicParsing
    Write-Host "Service Status: $($statusResponse.serviceStatus)" -ForegroundColor Green
    
    if ($statusResponse.totalEnhancements) {
        Write-Host "Total System Enhancements: $($statusResponse.totalEnhancements)" -ForegroundColor Cyan
    }
}
catch {
    Write-Host "Could not get final status: $($_.Exception.Message)" -ForegroundColor Red
}

# Final Summary
Write-Host "`n" -NoNewline
Write-Host "FINAL COMPREHENSIVE SUMMARY" -ForegroundColor Green
Write-Host "===========================" -ForegroundColor Green
Write-Host "Total Column Fields Enhanced: $totalFieldsEnhanced" -ForegroundColor White
Write-Host "Total Column Records Processed: $totalColumnsEnhanced" -ForegroundColor White

if ($totalFieldsEnhanced -gt 0) {
    Write-Host "`nSUCCESS! BusinessColumnInfo enhancement complete!" -ForegroundColor Green
    Write-Host "Enhanced semantic fields include:" -ForegroundColor Cyan
    Write-Host "  - SemanticContext (business context)" -ForegroundColor White
    Write-Host "  - AnalyticalContext (analysis usage)" -ForegroundColor White
    Write-Host "  - SemanticRelevanceScore (relevance scoring)" -ForegroundColor White
    Write-Host "  - ConceptualRelationships (relationships)" -ForegroundColor White
    Write-Host "  - DomainSpecificTerms (domain terms)" -ForegroundColor White
    Write-Host "  - QueryIntentMapping (query patterns)" -ForegroundColor White
    Write-Host "  - BusinessQuestionTypes (question types)" -ForegroundColor White
    Write-Host "  - SemanticSynonyms (synonyms)" -ForegroundColor White
    Write-Host "  - BusinessMetrics (metrics)" -ForegroundColor White
    Write-Host "  - LLMPromptHints (AI hints)" -ForegroundColor White
    Write-Host "  - VectorSearchTags (search tags)" -ForegroundColor White
} else {
    Write-Host "`nAll BusinessColumnInfo fields were already populated!" -ForegroundColor Yellow
}

Write-Host "`nALL METADATA ENHANCEMENT COMPLETE!" -ForegroundColor Green
Write-Host "Your BI Reporting Copilot now has comprehensive semantic metadata!" -ForegroundColor Cyan
