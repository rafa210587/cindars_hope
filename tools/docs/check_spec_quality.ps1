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

# 2. Check for tests in wrong location
Write-Host "2. Checking test file locations..." -ForegroundColor Yellow

$testsInScripts = @()
$gitFiles = & git ls-files --others --exclude-standard
foreach ($file in $gitFiles) {
    if ($file -match "Assets.*Scripts.*Tests\.cs$") {
        $testsInScripts += $file
    }
}

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
