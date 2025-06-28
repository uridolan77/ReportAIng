# Check which records were enhanced by the MetadataEnhancementService
param(
    [string]$BaseUrl = "http://localhost:55244",
    [string]$Username = "admin",
    [string]$Password = "Admin123!"
)

Write-Host "🔍 Checking Enhanced Metadata Records" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green

# Skip certificate validation
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

# Step 1: Login to get token
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

# Step 2: Get current status to see what was enhanced
Write-Host "`n📊 Getting Enhancement Status..." -ForegroundColor Yellow

try {
    $statusResponse = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/status" -Method GET -Headers $authHeaders -UseBasicParsing
    Write-Host "Service Status: $($statusResponse.serviceStatus)" -ForegroundColor Cyan
    
    if ($statusResponse.lastEnhancement) {
        Write-Host "Last Enhancement: $($statusResponse.lastEnhancement)" -ForegroundColor Cyan
    }
    
    if ($statusResponse.totalEnhancements) {
        Write-Host "Total Enhancements: $($statusResponse.totalEnhancements)" -ForegroundColor Cyan
    }
}
catch {
    Write-Host "❌ Could not get status: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 3: Check if we can query the database through an API endpoint
Write-Host "`n🔍 Checking for Business Metadata..." -ForegroundColor Yellow

# Try to get business table info to see what exists
try {
    $tablesResponse = Invoke-RestMethod -Uri "$BaseUrl/api/BusinessMetadata/tables" -Method GET -Headers $authHeaders -UseBasicParsing
    Write-Host "✅ Found $($tablesResponse.Count) business tables" -ForegroundColor Green
    
    if ($tablesResponse.Count -gt 0) {
        Write-Host "`n📋 Sample Business Tables:" -ForegroundColor Cyan
        $tablesResponse | Select-Object -First 5 | ForEach-Object {
            Write-Host "  - $($_.tableName): $($_.businessPurpose)" -ForegroundColor White
            if ($_.semanticDescription) {
                Write-Host "    Semantic Description: $($_.semanticDescription)" -ForegroundColor Gray
            }
            if ($_.llmContextHints) {
                Write-Host "    LLM Context Hints: $($_.llmContextHints)" -ForegroundColor Gray
            }
        }
    }
}
catch {
    Write-Host "⚠️  Could not access business tables endpoint: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Try to get business columns
try {
    $columnsResponse = Invoke-RestMethod -Uri "$BaseUrl/api/BusinessMetadata/columns" -Method GET -Headers $authHeaders -UseBasicParsing
    Write-Host "`n✅ Found $($columnsResponse.Count) business columns" -ForegroundColor Green
    
    if ($columnsResponse.Count -gt 0) {
        Write-Host "`n📋 Sample Business Columns with Enhanced Fields:" -ForegroundColor Cyan
        $enhancedColumns = $columnsResponse | Where-Object { 
            $_.semanticContext -or $_.analyticalContext -or $_.semanticRelevanceScore -gt 0 
        } | Select-Object -First 5
        
        if ($enhancedColumns.Count -gt 0) {
            $enhancedColumns | ForEach-Object {
                Write-Host "  - $($_.columnName) ($($_.tableName))" -ForegroundColor White
                if ($_.semanticContext) {
                    Write-Host "    Semantic Context: $($_.semanticContext)" -ForegroundColor Green
                }
                if ($_.analyticalContext) {
                    Write-Host "    Analytical Context: $($_.analyticalContext)" -ForegroundColor Green
                }
                if ($_.semanticRelevanceScore -gt 0) {
                    Write-Host "    Relevance Score: $($_.semanticRelevanceScore)" -ForegroundColor Green
                }
            }
        } else {
            Write-Host "  No enhanced columns found yet" -ForegroundColor Yellow
        }
    }
}
catch {
    Write-Host "⚠️  Could not access business columns endpoint: $($_.Exception.Message)" -ForegroundColor Yellow
}

# Step 4: Run another enhancement to see real-time changes
Write-Host "`n🔧 Running Another Enhancement to See Changes..." -ForegroundColor Yellow

$enhanceBody = @{
    mode = "EmptyFieldsOnly"
    batchSize = 5
    previewOnly = $false
} | ConvertTo-Json

try {
    $enhanceResponse = Invoke-RestMethod -Uri "$BaseUrl/api/MetadataEnhancement/enhance" -Method POST -Headers $authHeaders -Body $enhanceBody -UseBasicParsing
    
    Write-Host "✅ Enhancement completed!" -ForegroundColor Green
    Write-Host "Fields Enhanced: $($enhanceResponse.fieldsEnhanced)" -ForegroundColor Cyan
    Write-Host "Columns Processed: $($enhanceResponse.columnsProcessed)" -ForegroundColor Cyan
    Write-Host "Tables Processed: $($enhanceResponse.tablesProcessed)" -ForegroundColor Cyan
    Write-Host "Glossary Terms Processed: $($enhanceResponse.glossaryTermsProcessed)" -ForegroundColor Cyan
    Write-Host "Processing Time: $($enhanceResponse.processingTime)" -ForegroundColor Cyan
    
    if ($enhanceResponse.enhancedRecords) {
        Write-Host "`n📝 Enhanced Records Details:" -ForegroundColor Green
        $enhanceResponse.enhancedRecords | ForEach-Object {
            Write-Host "  - $($_.entityType): $($_.entityName)" -ForegroundColor White
            Write-Host "    Fields: $($_.enhancedFields -join ', ')" -ForegroundColor Gray
        }
    }
    
    if ($enhanceResponse.warnings -and $enhanceResponse.warnings.Count -gt 0) {
        Write-Host "`n⚠️  Warnings:" -ForegroundColor Yellow
        $enhanceResponse.warnings | ForEach-Object {
            Write-Host "  - $_" -ForegroundColor Yellow
        }
    }
}
catch {
    Write-Host "❌ Enhancement failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n💡 To see the actual database changes, you can:" -ForegroundColor Cyan
Write-Host "1. Run the SQL script: Scripts\check-enhanced-records.sql" -ForegroundColor White
Write-Host "2. Check the AuditLogs table for METADATA_ENHANCEMENT actions" -ForegroundColor White
Write-Host "3. Look for records with UpdatedBy = 'MetadataEnhancementService'" -ForegroundColor White

Write-Host "`n🎉 Analysis Complete!" -ForegroundColor Green
