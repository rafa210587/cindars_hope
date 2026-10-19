#!/usr/bin/env pwsh
<#
.SYNOPSIS
Check that code commits have corresponding reports, tests, and evidence.

.DESCRIPTION
Validate that any new/modified code in Assets/_Game/Scripts/ has:
- Execution report
- Test (if not deferred)
- Status update
- Run strict validation invocation

.PARAMETER CommitMessage
Optional commit message to validate against generic patterns.

.EXAMPLE
.\check_spec_diff_completeness.ps1

.EXAMPLE
.\check_spec_diff_completeness.ps1 -CommitMessage "feat: implement farm animals"
#>

param(
    [string]$CommitMessage,
    [string]$ScopePath = ''
)

$ErrorActionPreference = "Continue"
$exitCode = 0
$issues = @()
$validationScope = $null
if ($ScopePath) {
    . (Join-Path $PSScriptRoot 'ValidationScope.ps1')
    try { $validationScope = Read-ValidationScope $ScopePath }
    catch { Write-Host "SPEC_DIFF_COMPLETENESS_CHECK: FAIL; $($_.Exception.Message)"; exit 1 }
}

Write-Host "SPEC_DIFF_COMPLETENESS_CHECK" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# 1. Get git status
Write-Host "1. Checking git diff..." -ForegroundColor Yellow

$gitStatus = & git status --porcelain
if ($validationScope) { $gitStatus = @($validationScope.changedFiles | ForEach-Object { ' M ' + $_ }) }
$runtimeChanges = @()
$testChanges = @()
$reportChanges = @()

foreach ($line in $gitStatus) {
    $file = $line.Substring(3).Trim()

    # Detect runtime code changes
    if ($file -match '^Assets/_Game/Scripts/' -and $file -match '\.(cs|csproj)$') {
        if ($file -notmatch 'Editor|Tests') {
            $runtimeChanges += $file
        }
    }

    # Detect test changes
    if ($file -match 'Assets/_Game/Tests/' -and $file -match '\.cs$') {
        $testChanges += $file
    }

    # Detect execution report changes
    if ($file -match 'docs/validation/.*execution_report' -and $file -match '\.md$') {
        $reportChanges += $file
    }
}

if ($validationScope) { $reportChanges = @(@($reportChanges) + @($validationScope.reportPaths) | Select-Object -Unique) }
$reportChanges = @($reportChanges | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf })
Write-Host "   Runtime code files changed: $($runtimeChanges.Count)" -ForegroundColor Gray
Write-Host "   Test files changed: $($testChanges.Count)" -ForegroundColor Gray
Write-Host "   Report files changed: $($reportChanges.Count)" -ForegroundColor Gray

Write-Host ""

# 2. Check for code without reports
Write-Host "2. Checking for code without execution reports..." -ForegroundColor Yellow

if ($runtimeChanges.Count -gt 0 -and $reportChanges.Count -eq 0) {
    $issues += "FAIL: Runtime code changed but no execution report found"
    $issues += "  Code changes: $(($runtimeChanges | Measure-Object).Count) files"
    $issues += "  Reports: 0 files"
    $issues += "  → Required: Create docs/validation/*execution_report.md"
    $exitCode = 1
} elseif ($runtimeChanges.Count -gt 0) {
    Write-Host "   PASS: Code changes have reports" -ForegroundColor Green
} else {
    Write-Host "   PASS: No code changes detected" -ForegroundColor Green
}

Write-Host ""

# 3. Check execution report contents
Write-Host "3. Checking execution report contents..." -ForegroundColor Yellow

if ($reportChanges.Count -gt 0) {
    $reportIssues = @()

    foreach ($reportFile in $reportChanges) {
        if (-not (Test-Path $reportFile)) { continue }

        $content = Get-Content -Path $reportFile -Raw

        $mandatorySections = @(
            "Acceptance criteria extracted",
            "Existing systems audit",
            "Spec Compliance Matrix",
            "Validation"
        )

        foreach ($section in $mandatorySections) {
            if ($content -notmatch "##\s*$section") {
                $reportIssues += "  - $(Split-Path -Leaf $reportFile): missing '$section'"
            }
        }

        # Check for run_strict_validation mention
        if ($content -notmatch 'run_strict_validation') {
            # Skip if status is BLOCKED, DEFERRED, CONTRACT_ONLY
            if ($content -notmatch 'BLOCKED|DEFERRED|CONTRACT_ONLY|NEEDS_REWORK') {
                $reportIssues += "  - $(Split-Path -Leaf $reportFile): no mention of 'run_strict_validation.ps1'"
            }
        }

        # Check for generic commit message keywords in report
        if ($CommitMessage) {
            $genericPatterns = @('fix', 'update', 'changes', 'wip', 'misc')
            foreach ($pattern in $genericPatterns) {
                if ($CommitMessage -match "^\s*$pattern\s*$") {
                    $reportIssues += "  - $(Split-Path -Leaf $reportFile): commit message is generic ('$pattern')"
                    break
                }
            }
        }
    }

    if ($reportIssues.Count -gt 0) {
        $issues += "WARN: Execution report issues"
        $issues += $reportIssues
    } else {
        Write-Host "   PASS: All execution reports have mandatory sections" -ForegroundColor Green
    }
}

Write-Host ""

# 4. Check for code without tests
Write-Host "4. Checking for deterministic code without tests..." -ForegroundColor Yellow

if ($runtimeChanges.Count -gt 0 -and $testChanges.Count -eq 0) {
    # Testes existentes executados sao evidencia; um arquivo de teste novo nao e obrigatorio.
    $testDeferred = $false
    $existingTestsEvidenced = $false
    foreach ($reportFile in $reportChanges) {
        if (-not (Test-Path $reportFile)) { continue }

        $content = Get-Content -Path $reportFile -Raw
        if ($content -match '(?im)^Existing tests executed:\s*YES\s*$' -and
            $content -match '(?im)^Test command:\s*\S.+' -and
            $content -match '(?im)^Test evidence:\s*\S.+' -and
            $content -match '(?im)^Test result:\s*(PASS|FAIL)\b') {
            $existingTestsEvidenced = $true
        }
        if ($content -match 'Automated tests.*not added.*JUSTIFIED|Tests not added.*Justified|Testing deferred') {
            $testDeferred = $true
            break
        }
    }

    if ($existingTestsEvidenced) {
        Write-Host '   Evidence declared for existing tests; reviewer must verify artifacts, inputs and result (not a test PASS claim).'
    } elseif (-not $testDeferred) {
        $issues += "WARN: Runtime code changed but no test files found and not deferred in report"
        $issues += "  → If testing was deferred: add 'Automated tests not added: JUSTIFIED' to report"
        $issues += "  → If testing was forgotten: add tests to Assets/_Game/Tests/EditMode/"
    } else {
        Write-Host "   PASS: Testing deferred (documented in report)" -ForegroundColor Green
    }
} else {
    Write-Host "   PASS: Tests present or not required" -ForegroundColor Green
}

Write-Host ""

# 5. Check commit message validity
Write-Host "5. Checking commit message..." -ForegroundColor Yellow

if ($CommitMessage) {
    $genericPatterns = @('^fix$', '^update$', '^changes$', '^wip$', '^misc$', '^update$')
    $isGeneric = $false

    foreach ($pattern in $genericPatterns) {
        if ($CommitMessage -match $pattern) {
            $issues += "FAIL: Generic commit message: '$CommitMessage'"
            $issues += "  → Use: 'feat: <description>' or 'fix: <description>' with details"
            $exitCode = 1
            $isGeneric = $true
            break
        }
    }

    if (-not $isGeneric) {
        Write-Host "   PASS: Commit message is specific" -ForegroundColor Green
    }
} else {
    Write-Host "   SKIP: No commit message provided (use -CommitMessage)" -ForegroundColor Gray
}

Write-Host ""

# Final result
Write-Host "================================================" -ForegroundColor Cyan

if ($exitCode -eq 0 -and $issues.Count -eq 0) {
    Write-Host "SPEC_DIFF_COMPLETENESS_CHECK: PASS" -ForegroundColor Green
    exit 0
} else {
    Write-Host "SPEC_DIFF_COMPLETENESS_CHECK: FAIL/WARN" -ForegroundColor $(if ($exitCode -eq 1) { "Red" } else { "Yellow" })
    Write-Host ""
    foreach ($issue in $issues) {
        Write-Host $issue -ForegroundColor $(if ($issue -match "^FAIL") { "Red" } else { "Yellow" })
    }
    Write-Host ""
    exit $exitCode
}
