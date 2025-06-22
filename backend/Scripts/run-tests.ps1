# BCAPB Testing Suite Runner
# Comprehensive testing script for Business-Context-Aware Prompt Building system

param(
    [string]$TestType = "all",  # all, unit, integration, performance
    [switch]$Coverage = $false,
    [switch]$Verbose = $false,
    [string]$Filter = "",
    [switch]$Parallel = $true
)

Write-Host "🧪 BCAPB Testing Suite" -ForegroundColor Cyan
Write-Host "======================" -ForegroundColor Cyan

# Test configuration
$UnitTestProject = "BIReportingCopilot.Tests.Unit"
$IntegrationTestProject = "BIReportingCopilot.Tests.Integration"
$PerformanceTestProject = "BIReportingCopilot.Tests.Performance"

$TestResults = @()
$StartTime = Get-Date

function Write-TestHeader {
    param([string]$Title)
    Write-Host ""
    Write-Host "🔬 $Title" -ForegroundColor Yellow
    Write-Host ("=" * ($Title.Length + 3)) -ForegroundColor Yellow
}

function Run-TestProject {
    param(
        [string]$ProjectName,
        [string]$TestCategory,
        [string]$AdditionalArgs = ""
    )
    
    Write-TestHeader "Running $TestCategory Tests"
    
    $TestStartTime = Get-Date
    $TestArgs = @(
        "test"
        $ProjectName
        "--logger", "console;verbosity=normal"
        "--logger", "trx;LogFileName=$TestCategory-results.trx"
    )
    
    if ($Coverage) {
        $TestArgs += @(
            "--collect", "XPlat Code Coverage"
            "--results-directory", "TestResults"
        )
    }
    
    if ($Verbose) {
        $TestArgs += @("--verbosity", "detailed")
    }
    
    if ($Filter) {
        $TestArgs += @("--filter", $Filter)
    }
    
    if ($Parallel) {
        $TestArgs += @("--parallel")
    }
    
    if ($AdditionalArgs) {
        $TestArgs += $AdditionalArgs.Split(' ')
    }
    
    Write-Host "Command: dotnet $($TestArgs -join ' ')" -ForegroundColor Gray
    
    try {
        $Process = Start-Process -FilePath "dotnet" -ArgumentList $TestArgs -Wait -PassThru -NoNewWindow
        $TestEndTime = Get-Date
        $Duration = ($TestEndTime - $TestStartTime).TotalSeconds
        
        $Result = @{
            Category = $TestCategory
            Project = $ProjectName
            ExitCode = $Process.ExitCode
            Duration = $Duration
            Success = $Process.ExitCode -eq 0
        }
        
        if ($Result.Success) {
            Write-Host "✅ $TestCategory tests completed successfully in $([math]::Round($Duration, 2))s" -ForegroundColor Green
        } else {
            Write-Host "❌ $TestCategory tests failed (Exit Code: $($Process.ExitCode))" -ForegroundColor Red
        }
        
        return $Result
    }
    catch {
        Write-Host "❌ Error running $TestCategory tests: $($_.Exception.Message)" -ForegroundColor Red
        return @{
            Category = $TestCategory
            Project = $ProjectName
            ExitCode = -1
            Duration = 0
            Success = $false
            Error = $_.Exception.Message
        }
    }
}

function Run-PerformanceBenchmarks {
    Write-TestHeader "Running Performance Benchmarks"
    
    try {
        $BenchmarkArgs = @(
            "run"
            "--project", $PerformanceTestProject
            "--configuration", "Release"
            "--", "--job", "short"
        )
        
        Write-Host "Command: dotnet $($BenchmarkArgs -join ' ')" -ForegroundColor Gray
        
        $Process = Start-Process -FilePath "dotnet" -ArgumentList $BenchmarkArgs -Wait -PassThru -NoNewWindow
        
        if ($Process.ExitCode -eq 0) {
            Write-Host "✅ Performance benchmarks completed successfully" -ForegroundColor Green
        } else {
            Write-Host "❌ Performance benchmarks failed" -ForegroundColor Red
        }
        
        return @{
            Category = "Benchmarks"
            Project = $PerformanceTestProject
            ExitCode = $Process.ExitCode
            Success = $Process.ExitCode -eq 0
        }
    }
    catch {
        Write-Host "❌ Error running benchmarks: $($_.Exception.Message)" -ForegroundColor Red
        return @{
            Category = "Benchmarks"
            Project = $PerformanceTestProject
            ExitCode = -1
            Success = $false
            Error = $_.Exception.Message
        }
    }
}

function Show-TestSummary {
    param([array]$Results)
    
    Write-Host ""
    Write-Host "📊 Test Summary" -ForegroundColor Cyan
    Write-Host "===============" -ForegroundColor Cyan
    
    $TotalTests = $Results.Count
    $PassedTests = ($Results | Where-Object { $_.Success }).Count
    $FailedTests = $TotalTests - $PassedTests
    $TotalDuration = ($Results | Measure-Object -Property Duration -Sum).Sum
    
    Write-Host ""
    Write-Host "Results by Category:" -ForegroundColor White
    foreach ($Result in $Results) {
        $Status = if ($Result.Success) { "✅ PASS" } else { "❌ FAIL" }
        $Duration = if ($Result.Duration) { "$([math]::Round($Result.Duration, 2))s" } else { "N/A" }
        Write-Host "  $($Result.Category): $Status ($Duration)" -ForegroundColor $(if ($Result.Success) { "Green" } else { "Red" })
        
        if ($Result.Error) {
            Write-Host "    Error: $($Result.Error)" -ForegroundColor Red
        }
    }
    
    Write-Host ""
    Write-Host "Overall Results:" -ForegroundColor White
    Write-Host "  Total Test Categories: $TotalTests" -ForegroundColor White
    Write-Host "  Passed: $PassedTests" -ForegroundColor Green
    Write-Host "  Failed: $FailedTests" -ForegroundColor $(if ($FailedTests -gt 0) { "Red" } else { "Green" })
    Write-Host "  Total Duration: $([math]::Round($TotalDuration, 2))s" -ForegroundColor White
    
    $OverallSuccess = $FailedTests -eq 0
    $OverallStatus = if ($OverallSuccess) { "✅ ALL TESTS PASSED" } else { "❌ SOME TESTS FAILED" }
    Write-Host ""
    Write-Host $OverallStatus -ForegroundColor $(if ($OverallSuccess) { "Green" } else { "Red" })
    
    return $OverallSuccess
}

function Show-CoverageReport {
    if ($Coverage -and (Test-Path "TestResults")) {
        Write-Host ""
        Write-Host "📈 Code Coverage Report" -ForegroundColor Cyan
        Write-Host "=======================" -ForegroundColor Cyan
        
        $CoverageFiles = Get-ChildItem -Path "TestResults" -Filter "*.xml" -Recurse
        if ($CoverageFiles.Count -gt 0) {
            Write-Host "Coverage files generated in TestResults directory:" -ForegroundColor Green
            foreach ($File in $CoverageFiles) {
                Write-Host "  $($File.FullName)" -ForegroundColor Gray
            }
            
            # Try to generate HTML report if reportgenerator is available
            try {
                $ReportGenerator = Get-Command "reportgenerator" -ErrorAction SilentlyContinue
                if ($ReportGenerator) {
                    Write-Host "Generating HTML coverage report..." -ForegroundColor Yellow
                    & reportgenerator "-reports:TestResults\**\*.xml" "-targetdir:TestResults\CoverageReport" "-reporttypes:Html"
                    Write-Host "HTML report generated: TestResults\CoverageReport\index.html" -ForegroundColor Green
                }
            }
            catch {
                Write-Host "Note: Install reportgenerator for HTML coverage reports: dotnet tool install -g dotnet-reportgenerator-globaltool" -ForegroundColor Yellow
            }
        }
    }
}

# Main execution
Write-Host "Test Type: $TestType" -ForegroundColor White
Write-Host "Coverage: $Coverage" -ForegroundColor White
Write-Host "Parallel: $Parallel" -ForegroundColor White
if ($Filter) {
    Write-Host "Filter: $Filter" -ForegroundColor White
}

# Clean previous test results
if (Test-Path "TestResults") {
    Remove-Item "TestResults" -Recurse -Force
}

# Run tests based on type
switch ($TestType.ToLower()) {
    "unit" {
        $TestResults += Run-TestProject $UnitTestProject "Unit"
    }
    "integration" {
        $TestResults += Run-TestProject $IntegrationTestProject "Integration"
    }
    "performance" {
        $TestResults += Run-TestProject $PerformanceTestProject "Performance"
        $TestResults += Run-PerformanceBenchmarks
    }
    "all" {
        $TestResults += Run-TestProject $UnitTestProject "Unit"
        $TestResults += Run-TestProject $IntegrationTestProject "Integration"
        $TestResults += Run-TestProject $PerformanceTestProject "Performance"
    }
    default {
        Write-Host "❌ Invalid test type: $TestType" -ForegroundColor Red
        Write-Host "Valid options: all, unit, integration, performance" -ForegroundColor Yellow
        exit 1
    }
}

# Show results
$OverallSuccess = Show-TestSummary $TestResults
Show-CoverageReport

$EndTime = Get-Date
$TotalDuration = ($EndTime - $StartTime).TotalSeconds
Write-Host ""
Write-Host "🏁 Testing completed in $([math]::Round($TotalDuration, 2))s" -ForegroundColor Cyan

# Exit with appropriate code
exit $(if ($OverallSuccess) { 0 } else { 1 })
