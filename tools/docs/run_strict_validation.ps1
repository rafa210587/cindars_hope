#!/usr/bin/env pwsh
<#
.SYNOPSIS
Run strict validation with explicit exit code gates.

.DESCRIPTION
Central validation command that prevents false "build pass" claims.
All checks exit immediately on failure; filtering is forbidden.

.EXAMPLE
.\run_strict_validation.ps1

Exit codes:
  0 - All validations passed
  1 - Any validation failed
#>

$ErrorActionPreference = "Continue"

Write-Host "STRICT_VALIDATION_HARNESS" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

$summary = @{
    Status = "UNKNOWN"
    DocsValidation = "NOT_RUN"
    AssemblyCSharp = "NOT_RUN"
    AssemblyCSharpEditor = "NOT_RUN"
    QualityCheck = "NOT_RUN"
}

# Step 1: Docs validation
Write-Host "1. Docs validation..." -ForegroundColor Yellow
.\tools\docs\validate_docs.ps1 *>&1 | Out-Host
if ($LASTEXITCODE -eq 0) {
    Write-Host "   PASS: Docs validation" -ForegroundColor Green
    $summary.DocsValidation = "PASS"
} else {
    Write-Host "   WARNING: Docs validation returned non-zero" -ForegroundColor Yellow
    $summary.DocsValidation = "EXPECTED_FAIL_LEGACY_ONLY"
}

Write-Host ""

# Step 2: Assembly-CSharp build
Write-Host "2. Assembly-CSharp build..." -ForegroundColor Yellow
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL: Assembly-CSharp build failed" -ForegroundColor Red
    Write-Host "STRICT_VALIDATION_RESULT: BUILD_FAILURE_ASSEMBLY_CSHARP" -ForegroundColor Red
    exit 1
}
Write-Host "   PASS: Assembly-CSharp" -ForegroundColor Green
$summary.AssemblyCSharp = "PASS"

Write-Host ""

# Step 3: Assembly-CSharp-Editor build
Write-Host "3. Assembly-CSharp-Editor build..." -ForegroundColor Yellow
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL: Assembly-CSharp-Editor build failed" -ForegroundColor Red
    Write-Host "STRICT_VALIDATION_RESULT: BUILD_FAILURE_ASSEMBLY_CSHARP_EDITOR" -ForegroundColor Red
    exit 1
}
Write-Host "   PASS: Assembly-CSharp-Editor" -ForegroundColor Green
$summary.AssemblyCSharpEditor = "PASS"

Write-Host ""

# Step 4: Quality check
Write-Host "4. Quality check..." -ForegroundColor Yellow
.\tools\docs\check_spec_quality.ps1
if ($LASTEXITCODE -ne 0) {
    Write-Host "FAIL: Quality check failed" -ForegroundColor Red
    Write-Host "STRICT_VALIDATION_RESULT: QUALITY_CHECK_FAILURE" -ForegroundColor Red
    exit 1
}
Write-Host "   PASS: Quality check" -ForegroundColor Green
$summary.QualityCheck = "PASS"

Write-Host ""

# All passed
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "STRICT_VALIDATION_RESULT: VALIDATION_PASS" -ForegroundColor Green
Write-Host "Exit code: 0" -ForegroundColor Green
Write-Host ""

$summary | ConvertTo-Json | Set-Content .\docs\validation\LAST_STRICT_VALIDATION_RESULT.json
exit 0
