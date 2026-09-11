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

param([string]$ScopePath = '')

$ErrorActionPreference = "Stop"

$validationScope = $null
if ($ScopePath) {
    . (Join-Path $PSScriptRoot 'ValidationScope.ps1')
    $validationScope = Read-ValidationScope $ScopePath
    Write-Host 'Quality mode: SCOPED; manifest must match the authorized task and actual diff.'
}

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
if ($validationScope) { $gitStatus = @($validationScope.changedFiles | ForEach-Object { ' M ' + $_ }) }
if ($gitStatus) {
    $forbiddenAltered = @()
    foreach ($line in $gitStatus) {
        $file = $line.Substring(3).Trim()
        foreach ($pattern in $forbiddenPatterns) {
            if ($file -match $pattern) {
                # Paths de assets explicitamente autorizados nao sao proibidos por extensao.
                # Artefatos operacionais continuam proibidos mesmo no manifesto.
                if ($validationScope -and $file -in $validationScope.allowedPaths -and $file -notmatch '^\.claude/.*\.lock$') { continue }
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
if ($validationScope) { $gitFilesUntracked = @($validationScope.changedFiles) }
foreach ($file in $gitFilesUntracked) {
    if ($file -match "Assets.*Scripts.*Tests\.cs$") {
        $testsInScripts += $file
    }
}

# Check tracked files
$gitFilesTracked = & git ls-files
if ($validationScope) { $gitFilesTracked = @($validationScope.changedFiles) }
foreach ($file in $gitFilesTracked) {
    if ($file -match "Assets.*Scripts.*Tests\.cs$") {
        $testsInScripts += $file
    }
}

# Remove duplicates
$testsInScripts = $testsInScripts | Select-Object -Unique

if ($testsInScripts.Count -gt 0) {
    $issues += 'FAIL: Tests in runtime Scripts directory (use the applicable test assembly)'
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
if ($validationScope) { $gitFilesTracked = @($validationScope.changedFiles) }
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
if ($validationScope) {
    $selectedReports = @($validationScope.reportPaths) + @($validationScope.changedFiles | Where-Object { $_ -match '^docs/validation/.*execution_report.*\.md$' })
    $reportFiles = @($selectedReports | Select-Object -Unique | ForEach-Object { Get-Item -LiteralPath $_ -ErrorAction Stop })
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
        # NOTE: $content is the whole file (Get-Content -Raw) and there is no (?m),
        # so "^## $section" only ever matched the very start of the file -> every
        # report was flagged as missing every section. Use "##\s*$section" (matches
        # the heading on any line), aligned with check_spec_diff_completeness.ps1.
        if ($content -notmatch "##\s*$section") {
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
if ($validationScope) { $gitStatusFull = @($validationScope.changedFiles | ForEach-Object { ' M ' + $_ }) }
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

# 9. Check for forbidden build validation patterns
Write-Host "9. Checking for forbidden build validation patterns..." -ForegroundColor Yellow

$forbiddenPatterns = @(
    'dotnet build.*Select-String',
    'Select-String.*error',
    '\| Select-String.*error',
    '\| Out-String.*error'
)

$forbiddenIssues = @()

# Check execution reports
foreach ($report in $reportFiles) {
    if (-not $report) { continue }
    if (-not (Test-Path $report.FullName)) { continue }

    $content = Get-Content -Path $report.FullName -Raw

    # Check for filtered build patterns in code blocks
    if ($content -match '```powershell') {
        $codeBlocks = $content -split '```powershell'
        for ($i = 1; $i -lt $codeBlocks.Count; $i += 2) {
            $block = $codeBlocks[$i] -split '```' | Select-Object -First 1

            foreach ($pattern in $forbiddenPatterns) {
                if ($block -match $pattern) {
                    $forbiddenIssues += "  - $($report.Name): forbidden pattern '$pattern' in code block"
                }
            }
        }
    }
}

# Check command/rule files for forbidden patterns
$cmdFiles = Get-ChildItem -Path '.\.claude\commands\*.md', '.\.claude\rules\*.md' -ErrorAction SilentlyContinue
if ($validationScope) { $cmdFiles = @($validationScope.changedFiles | Where-Object { $_ -match '^\.claude/(commands|rules)/.*\.md$' -and (Test-Path -LiteralPath $_) } | ForEach-Object { Get-Item -LiteralPath $_ }) }
foreach ($file in $cmdFiles) {
    if (-not $file) { continue }

    $content = Get-Content -Path $file.FullName -Raw

    # Skip if file explicitly documents the patterns as forbidden examples
    # (rule/command docs quote them as "FORBIDDEN" / "❌" — they define the rule,
    # they do not violate it). Consolidation stubs restate the rule as
    # "**Invariant:** ... no dotnet build piped to Select-String" without the word
    # "FORBIDDEN", so "Invariant" is included too (e.g. build_validation_truth_gate.md,
    # which was the lone false positive forcing run_strict_validation to exit 1).
    # (?s) lets . span newlines; the old single-line regex never matched, so the rule
    # docs were being flagged whenever the script ran at all.
    if ($content -match '(?s)(Forbidden|FORBIDDEN|PROIBID|Invariant).*(pattern|build|Select-String)') {
        continue
    }

    foreach ($pattern in $forbiddenPatterns) {
        if ($content -match $pattern) {
            $forbiddenIssues += "  - $($file.Name): forbidden pattern '$pattern' (should use run_strict_validation.ps1)"
        }
    }
}

if ($forbiddenIssues.Count -gt 0) {
    $issues += 'FAIL: Forbidden build validation patterns detected'
    $issues += $forbiddenIssues
    $exitCode = 1
} else {
    Write-Host "   PASS: No forbidden build validation patterns" -ForegroundColor Green
}

Write-Host ""

# 10. Check for strict validation requirement in WAVE 05+ specs
Write-Host "10. Checking for strict validation in WAVE 05+ execution reports..." -ForegroundColor Yellow

$strictValidationIssues = @()

foreach ($report in $reportFiles) {
    # Check if this is a WAVE 05+ spec report
    if ($report.Name -match '05_spec.*execution_report' -or `
        $report.Name -match '06_spec.*execution_report' -or `
        $report.Name -match '0[6-9]_spec.*execution_report') {

        $content = Get-Content -Path $report.FullName -Raw

        # Check if validation method is documented
        if ($content -notmatch 'run_strict_validation') {
            # Skip if status is BLOCKED, DEFERRED, or CONTRACT_ONLY (not applicable)
            if ($content -notmatch 'BLOCKED|DEFERRED|CONTRACT_ONLY') {
                $strictValidationIssues += "  - $($report.Name): WAVE 05+ but no 'run_strict_validation' cited"
            }
        }
    }
}

if ($strictValidationIssues.Count -gt 0) {
    $issues += 'WARN: WAVE 05+ specs should cite run_strict_validation.ps1'
    $issues += $strictValidationIssues
} else {
    Write-Host "   PASS: WAVE 05+ specs properly cite strict validation" -ForegroundColor Green
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
