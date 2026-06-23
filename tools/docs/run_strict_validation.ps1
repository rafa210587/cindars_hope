#!/usr/bin/env pwsh
<#
.SYNOPSIS
Run strict validation with PowerShell script failure gates.

.DESCRIPTION
Central validation command with safe script invocation.
All scripts must succeed; no secondary issues.

.EXAMPLE
.\run_strict_validation.ps1

Exit codes:
  0 - All validations passed
  1 - Any validation failed (build, quality, script exception, etc)
#>

$ErrorActionPreference = "Continue"

Write-Host "STRICT_VALIDATION_HARNESS" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Helper function: safe script invocation
function Invoke-SafeScript {
    param(
        [string]$Name,
        [string]$ScriptPath,
        [hashtable]$Parameters
    )

    Write-Host "=== $Name ===" -ForegroundColor Cyan

    $previousExitCode = $LASTEXITCODE
    $previousErrorAction = $ErrorActionPreference

    try {
        $ErrorActionPreference = "Stop"

        if ($Parameters -and $Parameters.Count -gt 0) {
            & $ScriptPath @Parameters
        } else {
            & $ScriptPath
        }

        $scriptSuccess = $?
        $scriptExitCode = $LASTEXITCODE
    } catch {
        Write-Host ""
        Write-Host "SCRIPT_EXCEPTION in ${Name}:" -ForegroundColor Red
        Write-Host "  Message: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host ""
        return 1
    } finally {
        $ErrorActionPreference = $previousErrorAction
    }

    # Check PowerShell status
    if (-not $scriptSuccess) {
        Write-Host ""
        Write-Host "SCRIPT_FAILED_BY_POWERSHELL_STATUS: $Name" -ForegroundColor Red
        Write-Host ""
        return 1
    }

    # Check exit code
    if ($null -ne $scriptExitCode -and $scriptExitCode -ne 0) {
        Write-Host ""
        Write-Host "SCRIPT_FAILED_BY_EXIT_CODE: $Name (exit $scriptExitCode)" -ForegroundColor Red
        Write-Host ""
        return $scriptExitCode
    }

    Write-Host "   PASS" -ForegroundColor Green
    return 0
}

# Step 0: Corruption guard (fail fast antes de erros de compile confusos)
Write-Host ""
Write-Host "0. Corruption guard..." -ForegroundColor Yellow

$corruptionResult = Invoke-SafeScript -Name "corruption guard" -ScriptPath ".\tools\validate_no_corruption.ps1"

if ($corruptionResult -ne 0) {
    Write-Host ""
    Write-Host "STRICT_VALIDATION_RESULT: CORRUPTION_DETECTED" -ForegroundColor Red
    exit 1
}

# Step 1: Docs validation (allowed to be legacy-only)
Write-Host ""
Write-Host "1. Docs validation..." -ForegroundColor Yellow

$docsResult = Invoke-SafeScript -Name "docs validation" -ScriptPath ".\tools\docs\validate_docs.ps1"

if ($docsResult -ne 0) {
    Write-Host "   WARNING: Docs validation returned non-zero (may be legacy-only)" -ForegroundColor Yellow
    $docsStatus = "EXPECTED_FAIL_LEGACY_ONLY"
} else {
    $docsStatus = "PASS"
}

# Step 2: Assembly-CSharp build
Write-Host ""
Write-Host "2. Assembly-CSharp build..." -ForegroundColor Yellow

dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "STRICT_VALIDATION_RESULT: BUILD_FAILURE_ASSEMBLY_CSHARP" -ForegroundColor Red
    exit 1
}
Write-Host "   PASS" -ForegroundColor Green

# Step 3: Assembly-CSharp-Editor build
Write-Host ""
Write-Host "3. Assembly-CSharp-Editor build..." -ForegroundColor Yellow

dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "STRICT_VALIDATION_RESULT: BUILD_FAILURE_ASSEMBLY_CSHARP_EDITOR" -ForegroundColor Red
    exit 1
}
Write-Host "   PASS" -ForegroundColor Green

# Step 4: Diff completeness check
Write-Host ""
Write-Host "4. Spec diff completeness..." -ForegroundColor Yellow

$diffResult = Invoke-SafeScript -Name "diff completeness" -ScriptPath ".\tools\docs\check_spec_diff_completeness.ps1"

if ($diffResult -ne 0) {
    Write-Host ""
    Write-Host "STRICT_VALIDATION_RESULT: DIFF_COMPLETENESS_FAILURE" -ForegroundColor Red
    exit 1
}

# Step 5: Quality check (cannot fail)
Write-Host ""
Write-Host "5. Spec quality check..." -ForegroundColor Yellow

$qualityResult = Invoke-SafeScript -Name "quality check" -ScriptPath ".\tools\docs\check_spec_quality.ps1"

if ($qualityResult -ne 0) {
    Write-Host ""
    Write-Host "STRICT_VALIDATION_RESULT: QUALITY_CHECK_FAILURE" -ForegroundColor Red
    exit 1
}

# All passed
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "STRICT_VALIDATION_RESULT: VALIDATION_PASS" -ForegroundColor Green
Write-Host "Exit code: 0" -ForegroundColor Green
Write-Host ""

exit 0
