#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Validates that all architectural tests use YML configuration instead of hardcoded rules.

.DESCRIPTION
    This script scans architectural test files to ensure they use the centralized
    YML configuration approach rather than hardcoded rules. It helps maintain
    consistency and enforce the Single Source of Truth principle.

.PARAMETER TestDirectory
    Path to the architectural tests directory.

.PARAMETER ConfigFile
    Path to the architecture configuration file.

.EXAMPLE
    .\Validate-ArchitectureConfigUsage.ps1

.EXAMPLE
    .\Validate-ArchitectureConfigUsage.ps1 -TestDirectory "C:\path\to\tests" -ConfigFile "C:\path\to\config.yaml"
#>

param(
    [Parameter(Mandatory=$false)]
    [string]$TestDirectory = "g:\dev\game\plugins\framework\GameComposition\cs\Tests\Architectural",
    
    [Parameter(Mandatory=$false)]
    [string]$ConfigFile = "g:\dev\game\plugins\framework\GameComposition\cs\Tests\Architectural\architecture-config.yaml"
)

# Initialize counters
$script:TotalTests = 0
$script:ConfigDrivenTests = 0
$script:HardcodedTests = 0
$script:Issues = @()

function Test-ArchitectureConfigUsage {
    param(
        [string]$TestPath,
        [string]$ConfigPath
    )
    
    Write-Host "🔍 Validating Architecture Configuration Usage" -ForegroundColor Cyan
    Write-Host "📁 Test Directory: $TestPath" -ForegroundColor Gray
    Write-Host "⚙️  Config File: $ConfigPath" -ForegroundColor Gray
    Write-Host ""
    
    # Check if config file exists
    if (-not (Test-Path $ConfigPath)) {
        Write-Host "❌ Configuration file not found: $ConfigPath" -ForegroundColor Red
        return $false
    }
    
    # Get all test files
    $testFiles = Get-ChildItem -Path $TestDirectory -Filter "*.cs" -Recurse
    
    if ($testFiles.Count -eq 0) {
        Write-Host "⚠️  No test files found in directory" -ForegroundColor Yellow
        return $false
    }
    
    Write-Host "📊 Found $($testFiles.Count) test files" -ForegroundColor Gray
    Write-Host ""
    
    # Analyze each test file
    foreach ($file in $testFiles) {
        Test-FileConfigUsage -FilePath $file.FullName
    }
    
    # Generate report
    Show-ValidationReport
    
    # Return success if no hardcoded tests found
    return $script:HardcodedTests -eq 0
}

function Test-FileConfigUsage {
    param(
        [string]$FilePath
    )
    
    $script:TotalTests++
    $fileName = Split-Path $FilePath -Leaf
    $content = Get-Content $FilePath -Raw
    
    Write-Host "🔍 Analyzing: $fileName" -ForegroundColor White
    
    # Check if file uses configuration
    $usesConfig = $false
    $isHardcoded = $false
    $issues = @()
    
    # Positive indicators of config usage
    if ($content -match "ArchitectureConfigLoader\.LoadConfig") {
        $usesConfig = $true
        Write-Host "  ✅ Uses ArchitectureConfigLoader" -ForegroundColor Green
    }
    
    if ($content -match "_config\.") {
        $usesConfig = $true
        Write-Host "  ✅ References config object" -ForegroundColor Green
    }
    
    # Negative indicators of hardcoded rules
    $hardcodedPatterns = @(
        "HaveNameContaining\(`"GlobalEventBus`"\)",
        "HaveNameContaining\(`"CustomPool`"\)",
        "HaveNameContaining\(`"Dictionary<string, object>`"\)",
        "NotHaveNameContaining\(`"Custom",
        "ResideInNamespace\(`"\.Data\.`"\)",
        "ImplementInterface\(`"IEventDispatcher`"\)"
    )
    
    foreach ($pattern in $hardcodedPatterns) {
        if ($content -match $pattern) {
            $isHardcoded = $true
            $issues += "Hardcoded pattern detected: $pattern"
            Write-Host "  ❌ Hardcoded pattern: $pattern" -ForegroundColor Red
        }
    }
    
    # Check for deprecated tests
    if ($content -match "DEPRECATED.*Use ConfigurationDrivenArchitectureTests") {
        Write-Host "  ⚠️  Marked as deprecated (good)" -ForegroundColor Yellow
    }
    
    # Special case: ConfigurationDrivenArchitectureTests
    if ($fileName -eq "ConfigurationDrivenArchitectureTests.cs") {
        $usesConfig = $true
        Write-Host "  ✅ Configuration-driven test (reference implementation)" -ForegroundColor Green
    }
    
    # Categorize the test
    if ($usesConfig -and -not $isHardcoded) {
        $script:ConfigDrivenTests++
        Write-Host "  📋 Status: ✅ Configuration-driven" -ForegroundColor Green
    }
    elseif ($isHardcoded) {
        $script:HardcodedTests++
        Write-Host "  📋 Status: ❌ Contains hardcoded rules" -ForegroundColor Red
        $script:Issues += @{
            File = $fileName
            Issues = $issues
        }
    }
    else {
        Write-Host "  📋 Status: ⚠️  Unclear (manual review needed)" -ForegroundColor Yellow
    }
    
    Write-Host ""
}

function Show-ValidationReport {
    Write-Host "📊 VALIDATION REPORT" -ForegroundColor Cyan
    Write-Host "========================" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "📈 Summary:" -ForegroundColor White
    Write-Host "  Total test files: $script:TotalTests" -ForegroundColor Gray
    Write-Host "  Configuration-driven: $script:ConfigDrivenTests" -ForegroundColor Green
    Write-Host "  Contains hardcoded rules: $script:HardcodedTests" -ForegroundColor $(if ($script:HardcodedTests -gt 0) { 'Red' } else { 'Green' })
    Write-Host ""
    
    # Calculate percentage
    if ($script:TotalTests -gt 0) {
        $configPercentage = [math]::Round(($script:ConfigDrivenTests / $script:TotalTests) * 100, 1)
        Write-Host "📊 Configuration adoption: $configPercentage%" -ForegroundColor $(if ($configPercentage -ge 80) { 'Green' } elseif ($configPercentage -ge 60) { 'Yellow' } else { 'Red' })
    }
    
    Write-Host ""
    
    # Show issues if any
    if ($script:Issues.Count -gt 0) {
        Write-Host "🚨 ISSUES FOUND:" -ForegroundColor Red
        Write-Host "================" -ForegroundColor Red
        Write-Host ""
        
        foreach ($issue in $script:Issues) {
            Write-Host "📁 File: $($issue.File)" -ForegroundColor Red
            foreach ($problem in $issue.Issues) {
                Write-Host "  ❌ $problem" -ForegroundColor Red
            }
            Write-Host ""
        }
        
        Write-Host "💡 RECOMMENDATIONS:" -ForegroundColor Yellow
        Write-Host "==================" -ForegroundColor Yellow
        Write-Host "1. Migrate hardcoded tests to use ConfigurationDrivenArchitectureTests pattern"
        Write-Host "2. Add new rules to architecture-config.yaml instead of hardcoding"
        Write-Host "3. Use ArchitectureConfigLoader.LoadConfig() to access configuration"
        Write-Host "4. Reference config properties instead of hardcoded values"
        Write-Host ""
    }
    else {
        Write-Host "🎉 SUCCESS: All tests are configuration-driven!" -ForegroundColor Green
        Write-Host ""
    }
    
    # Show next steps
    Write-Host "📋 NEXT STEPS:" -ForegroundColor Cyan
    Write-Host "===============" -ForegroundColor Cyan
    if ($script:HardcodedTests -gt 0) {
        Write-Host "1. Fix hardcoded tests identified above"
        Write-Host "2. Run this validation script again"
        Write-Host "3. Ensure all tests pass with configuration-driven approach"
    }
    else {
        Write-Host "1. Maintain configuration-driven approach for new tests"
        Write-Host "2. Update architecture-config.yaml for new rules"
        Write-Host "3. Periodically run this validation script"
    }
    Write-Host ""
}

# Main execution
try {
    $success = Test-ArchitectureConfigUsage -TestPath $TestDirectory -ConfigPath $ConfigFile
    
    if ($success) {
        Write-Host "✅ Validation completed successfully" -ForegroundColor Green
        exit 0
    } else {
        Write-Host "❌ Validation failed - issues found" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "💥 Error during validation: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
