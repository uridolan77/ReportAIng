$body = @{
    query = "Top 10 depositors yesterday from UK"
    requestedSteps = @("BusinessContextAnalysis", "TokenBudgetManagement", "SchemaRetrieval")
} | ConvertTo-Json -Depth 3

Write-Host "Testing Enhanced Schema Contextualization System..."
Write-Host "Request Body: $body"

try {
    $response = Invoke-RestMethod -Uri "http://localhost:55244/api/pipeline/test" -Method POST -Body $body -ContentType "application/json"
    Write-Host "✅ SUCCESS: API Response received"
    $response | ConvertTo-Json -Depth 10
} catch {
    Write-Host "❌ ERROR: $($_.Exception.Message)"
    Write-Host "Status Code: $($_.Exception.Response.StatusCode)"
    Write-Host "Response: $($_.Exception.Response)"
}
