# PowerShell script to run SQL script against the database
param(
    [string]$SqlFile = "insert-sample-llm-data.sql",
    [string]$Server = "185.64.56.157",
    [string]$Database = "BICopilot"
)

try {
    Write-Host "Executing SQL script: $SqlFile" -ForegroundColor Green
    Write-Host "Server: $Server" -ForegroundColor Yellow
    Write-Host "Database: $Database" -ForegroundColor Yellow
    
    # Use Windows Authentication (integrated security)
    $connectionString = "Server=$Server;Database=$Database;Integrated Security=true;TrustServerCertificate=true;"
    
    # Read the SQL file
    $sqlContent = Get-Content -Path $SqlFile -Raw
    
    # Execute the SQL
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    $command = New-Object System.Data.SqlClient.SqlCommand($sqlContent, $connection)
    $command.CommandTimeout = 300  # 5 minutes timeout
    
    $result = $command.ExecuteNonQuery()
    
    Write-Host "SQL script executed successfully!" -ForegroundColor Green
    Write-Host "Rows affected: $result" -ForegroundColor Cyan
    
    $connection.Close()
}
catch {
    Write-Host "Error executing SQL script: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Full error: $($_.Exception)" -ForegroundColor Red
}

Write-Host "Press any key to continue..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
