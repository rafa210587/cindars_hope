# Spec Promotion Guard Hook
# Warns when a spec is being moved to implementados/ without a corresponding execution report.
# Disabled by default. Run manually before /finish-spec operations.

param(
    [string]$SpecFile = ""
)

if ($SpecFile -eq "") {
    # Try to detect from git diff if no argument
    try {
        $movedFiles = git diff --name-status HEAD 2>$null | Where-Object {
            $_ -match "^R" -and $_ -match "implementados"
        }
        if ($movedFiles.Count -eq 0) {
            $movedFiles = git status --short 2>$null | Where-Object {
                $_ -match "implementados.*spec_"
            }
        }
    }
    catch {
        $movedFiles = @()
    }
}
else {
    $movedFiles = @($SpecFile)
}

if ($movedFiles.Count -eq 0) {
    Write-Host "Spec Promotion Guard: No specs being moved detected."
    exit 0
}

$violations = @()

foreach ($move in $movedFiles) {
    # Extract spec ID from path
    if ($move -match "(spec_[\w_]+)") {
        $specId = $matches[1]

        # Check for execution report
        $reportPattern = "docs/validation/$specId*_execution_report.md"
        $reportExists = Test-Path $reportPattern -ErrorAction SilentlyContinue

        if (-not $reportExists) {
            # Also check docs/validation/ directory
            $reports = Get-ChildItem "docs/validation/" -Filter "*${specId}*execution_report*" -ErrorAction SilentlyContinue
            $reportExists = ($null -ne $reports -and $reports.Count -gt 0)
        }

        if (-not $reportExists) {
            $violations += "  Spec '$specId' being promoted but no execution report found in docs/validation/"
        }
    }
}

if ($violations.Count -gt 0) {
    Write-Host ""
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    Write-Host "Spec Promotion Guard — WARNINGS"
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    foreach ($v in $violations) {
        Write-Host $v
    }
    Write-Host ""
    Write-Host "  Rule: spec-promotion-requires-evidence.md"
    Write-Host "  Create execution report before promoting spec."
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    Write-Host ""
    exit 1
}

Write-Host "Spec Promotion Guard: PASS — execution reports found for promoted specs."
exit 0
