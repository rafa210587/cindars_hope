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

# 6. Check for Unix/Bash commands in recent reports without retry
Write-Host "6. Checking for Unix/Bash commands without PowerShell retry..." -ForegroundColor Yellow

$bashCommandIssues = @()
$bashPatterns = @('head ', 'tail ', ' ls ', 'find ', ' grep ', ' cat ', 'pwd', 'bash')

# Get recently modified reports (from git status)
$gitStatusFull = & git status --porcelain
$recentReports = @()
foreach ($line in $gitStatusFull) {
    if ($line -match 'docs/validation.*execution_report.*\.md$') {
        $file = $line.Substring(3).Trim()
        if (Test-Path $file) {
            $recentReports += $file
        }
    }
}

foreach ($report in $recentReports) {
    if (-not $report) { continue }

    if (-not (Test-Path $report)) { continue }

    $content = Get-Content -Path $report -Raw

    # Check for Unix commands
    $hasBashCommand = $false
    foreach ($pattern in $bashPatterns) {
        if ($content -match [regex]::Escape($pattern)) {
            $hasBashCommand = $true
            break
        }
    }

    if ($hasBashCommand) {
        # Check if retry was documented
        $hasRetry = $content -match 'PowerShell retry|ENV_COMMAND_RETRY_REQUIRED'

        if (-not $hasRetry) {
            $reportName = Split-Path -Leaf $report
            $bashCommandIssues += "  - $reportName`: Unix command found, no PowerShell retry documented"
        }
    }
}

if ($bashCommandIssues.Count -gt 0) {
    $issues += 'WARN: Unix/Bash commands without PowerShell retry'
    $issues += $bashCommandIssues
} else {
    Write-Host "   PASS: No Unix/Bash commands without retry" -ForegroundColor Green
}

Write-Host ""

# 7. Check for false environmental blockers
Write-Host "7. Checking for false environmental blockers..." -ForegroundColor Yellow

$falseBlockerIssues = @()
$falseBlockerPatterns = @(
    'Build validation commands not available in sandbox',
    'Quality check script not available in sandbox',
    'Git command execution unavailable',
    'PowerShell execution not allowed'
)

foreach ($report in $reportFiles) {
    $content = Get-Content -Path $report.FullName -Raw

    foreach ($pattern in $falseBlockerPatterns) {
        if ($content -match [regex]::Escape($pattern)) {
            # Check if retry was documented
            $hasRetry = $content -match 'PowerShell retry performed|ENV_COMMAND_FAILURE after retry'

            if (-not $hasRetry) {
                $falseBlockerIssues += "  - $($report.Name): false environmental blocker without retry evidence"
            }
        }
    }
}

if ($falseBlockerIssues.Count -gt 0) {
    $issues += 'WARN: False environmental blockers (missing retry evidence)'
    $issues += $falseBlockerIssues
} else {
    Write-Host "   PASS: No false environmental blockers" -ForegroundColor Green
}

Write-Host ""

# 8. Check for BLOCKED_BY_DEPENDENCY_PENDING without plan
Write-Host "8. Checking for dependency pending specs..." -ForegroundColor Yellow

$dependencyIssues = @()

foreach ($report in $reportFiles) {
    $content = Get-Content -Path $report.FullName -Raw

    if ($content -match 'BLOCKED_BY_DEPENDENCY_PENDING') {
        # Check if dependency plan exists
        $planExists = $false

        # Try to find wave from report path
        if ($report.Name -match '05_spec') {
            if (Test-Path 'docs/validation/WAVE_05_DEPENDENCY_RESOLUTION_PLAN.md') {
                $planExists = $true
            }
        } elseif ($report.Name -match '04_spec') {
            if (Test-Path 'docs/validation/WAVE_04_DEPENDENCY_RESOLUTION_PLAN.md') {
                $planExists = $true
            }
        }

        if (-not $planExists) {
            $dependencyIssues += "  - $($report.Name): BLOCKED_BY_DEPENDENCY_PENDING but no dependency plan found"
        }
    }
}

if ($dependencyIssues.Count -gt 0) {
    $issues += 'WARN: Dependency pending without resolution plan'
    $issues += $dependencyIssues
} else {
    Write-Host "   PASS: All dependency pending specs have resolution plan" -ForegroundColor Green
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
