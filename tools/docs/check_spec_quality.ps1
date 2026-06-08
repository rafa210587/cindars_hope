#!/usr/bin/env pwsh

<#
.SYNOPSIS
Check spec execution quality against Cindar's Hope standards.

.DESCRIPTION
Validate that executed specs meet quality gates:
- No forbidden files altered
- No tests in wrong location
- Reports have mandatory sections
- No prohibited status values

.EXAMPLE
.\check_spec_quality.ps1
#>

$ErrorActionPreference = "Stop"

$exitCode = 0
$issues = @()

Write-Host "SPEC_QUALITY_CHECK" -ForegroundColor Cyan
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host ""

# 1. Check for forbidden files
Write-Host "1. Checking forbidden files..." -ForegroundColor Yellow

$forbiddenPatterns = @(
    "^Packages/",
    "^ProjectSettings/",
    "\.unity$",
    "\.prefab$",
    "\.asset$",
    "\.mat$",
    "^\.claude/.*\.lock$"
)

$gitStatus = & git status --porcelain
if ($gitStatus) {
    $forbiddenAltered = @()
    foreach ($line in $gitStatus) {
        $file = $line.Substring(3).Trim()
        foreach ($pattern in $forbiddenPatterns) {
            if ($file -match $pattern) {
                $forbiddenAltered += $file
            }
        }
    }

    if ($forbiddenAltered.Count -gt 0) {
        $issues += 'FAIL: Forbidden files altered'
        foreach ($file in $forbiddenAltered) {
            $issues += "  - $file"
        }
        $exitCode = 1
    } else {
        Write-Host "   PASS: No forbidden files altered" -ForegroundColor Green
    }
} else {
    Write-Host "   PASS: No changes detected" -ForegroundColor Green
}

Write-Host ""

# 2. Check for tests in wrong location (both tracked and untracked)
Write-Host "2. Checking test file locations..." -ForegroundColor Yellow

$testsInScripts = @()

# Check untracked files
$gitFilesUntracked = & git ls-files --others --exclude-standard
foreach ($file in $gitFilesUntracked) {
    if ($file -match "Assets.*Scripts.*Tests\.cs$") {
        $testsInScripts += $file
    }
}

# Check tracked files
$gitFilesTracked = & git ls-files
foreach ($file in $gitFilesTracked) {
    if ($file -match "Assets.*Scripts.*Tests\.cs$") {
        $testsInScripts += $file
    }
}

# Remove duplicates
$testsInScripts = $testsInScripts | Select-Object -Unique

if ($testsInScripts.Count -gt 0) {
    $issues += 'FAIL: Tests in wrong location (must be EditMode)'
    foreach ($file in $testsInScripts) {
        $issues += "  - $file"
    }
    $exitCode = 1
} else {
    Write-Host "   PASS: All tests in correct location" -ForegroundColor Green
}

Write-Host ""

# 2b. Check for tracked .claude/*.lock files
Write-Host "2b. Checking for operational artifacts..." -ForegroundColor Yellow

$trackedLocks = @()
$gitFilesTracked = & git ls-files
foreach ($file in $gitFilesTracked) {
    if ($file -match "^\.claude/.*\.lock$") {
        $trackedLocks += $file
    }
}

if ($trackedLocks.Count -gt 0) {
    $issues += 'FAIL: Operational artifacts committed (.claude/*.lock)'
    foreach ($file in $trackedLocks) {
        $issues += "  - $file"
    }
    $exitCode = 1
} else {
    Write-Host "   PASS: No operational artifacts found" -ForegroundColor Green
}

Write-Host ""

# 3. Check execution reports
Write-Host "3. Checking execution reports..." -ForegroundColor Yellow

$reportFiles = @()
$reportPath = "docs/validation"
if (Test-Path $reportPath) {
    $reportFiles = Get-ChildItem -Path $reportPath -Filter "*execution_report*.md" -ErrorAction SilentlyContinue
}

$mandatorySections = @(
    "Acceptance criteria extracted",
    "Existing systems audit",
    "Spec Compliance Matrix",
    "Validation",
    "Honest status rationale"
)

$reportIssues = @()
foreach ($report in $reportFiles) {
    $content = Get-Content -Path $report.FullName -Raw
    foreach ($section in $mandatorySections) {
        if ($content -notmatch "^## $section") {
            $reportIssues += "  - $($report.Name): missing '$section'"
        }
    }
}

if ($reportIssues.Count -gt 0) {
    $issues += "WARN: Report sections missing"
    $issues += $reportIssues
} else {
    Write-Host "   PASS: All reports have mandatory sections" -ForegroundColor Green
}

Write-Host ""

# 4. Check for prohibited status
Write-Host "4. Checking for prohibited status..." -ForegroundColor Yellow

$statusIssues = @()
foreach ($report in $reportFiles) {
    $content = Get-Content -Path $report.FullName -Raw
    if ($content -match 'Status.*ACCEPTED' -and $content -notmatch 'Cannot|Not|Deferred') {
        $statusIssues += "  - $($report.Name): has unqualified ACCEPTED"
    }
}

if ($statusIssues.Count -gt 0) {
    $issues += "FAIL: Prohibited status found"
    $issues += $statusIssues
    $exitCode = 1
} else {
    Write-Host "   PASS: No prohibited status values" -ForegroundColor Green
}

Write-Host ""

# 5. Check for status inflation (BUILD_VALIDATED without evidence)
Write-Host "5. Checking for status inflation..." -ForegroundColor Yellow

$statusInflationIssues = @()
foreach ($report in $reportFiles) {
    $content = Get-Content -Path $report.FullName -Raw

    # If report claims BUILD_VALIDATED, check for evidence
    if ($content -match '## Status\s*`BUILD_VALIDATED`') {
        $hasCriteria = $content -match '## Acceptance criteria extracted'
        $hasAudit = $content -match '## Existing systems audit'
        $hasCompliance = $content -match '## Spec Compliance Matrix'
        $hasRationale = $content -match '## Honest status rationale'

        if (-not ($hasCriteria -and $hasAudit -and $hasCompliance -and $hasRationale)) {
            $statusInflationIssues += "  - $($report.Name): BUILD_VALIDATED missing required evidence sections"
        } else {
            # Check if Compliance Matrix has FAIL in Required criterion
            if ($content -match 'Status.*\| FAIL' -and $content -match 'Required.*\| FAIL') {
                $statusInflationIssues += "  - $($report.Name): BUILD_VALIDATED claims but Compliance Matrix has FAIL on Required criterion"
            }
        }
    }
}

if ($statusInflationIssues.Count -gt 0) {
    $issues += 'WARN: Status inflation detected (BUILD_VALIDATED without evidence)'
    $issues += $statusInflationIssues
} else {
    Write-Host "   PASS: No status inflation detected" -ForegroundColor Green
}

Write-Host ""

# Final summary
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan

if ($exitCode -eq 0) {
    Write-Host "SPEC_QUALITY_CHECK: PASS" -ForegroundColor Green
    exit 0
} else {
    Write-Host "SPEC_QUALITY_CHECK: FAIL" -ForegroundColor Red
    Write-Host ""
    foreach ($issue in $issues) {
        Write-Host $issue -ForegroundColor Red
    }
    Write-Host ""
    exit 1
}
