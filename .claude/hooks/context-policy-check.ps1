# Context Policy Check Hook
# Warns when an execution plan includes heavy historical reads without justification.
# Disabled by default. Run manually via /validate-spec or explicit invocation.

# Heavy files that should not be default reads in implementation tasks
$HeavyDefaultReads = @(
    "PROJECT_LOG.md",
    "docs/IMPLEMENTATION_STATUS.md",
    "docs/operations/AGENT_EXECUTION_PROTOCOL.md",
    "SPEC_EXECUTION_ORDER.md",
    "ROADMAP.md"
)

# Justification keywords that make heavy reads acceptable
$AcceptedJustifications = @(
    "audit",
    "reconciliation",
    "reconcile",
    "regression investigation",
    "wave planning",
    "plan-wave",
    "explicit human request"
)

param(
    [string]$PlanText = ""
)

if ($PlanText -eq "") {
    Write-Host "Context Policy Check: No plan text provided. Skipping."
    exit 0
}

$warnings = @()

foreach ($heavyFile in $HeavyDefaultReads) {
    if ($PlanText -match [regex]::Escape($heavyFile)) {
        $hasJustification = $false
        foreach ($justification in $AcceptedJustifications) {
            if ($PlanText -match $justification) {
                $hasJustification = $true
                break
            }
        }
        if (-not $hasJustification) {
            $warnings += "WARNING: Plan reads '$heavyFile' without audit/reconciliation/regression justification."
        }
    }
}

if ($warnings.Count -gt 0) {
    Write-Host ""
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    Write-Host "Context Policy Check — WARNINGS"
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    foreach ($w in $warnings) {
        Write-Host "  $w"
    }
    Write-Host ""
    Write-Host "  Rule: context-reading-policy.md"
    Write-Host "  Use docs/00_PROJECT/CURRENT_STATE.md instead for execution context."
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
    Write-Host ""
    exit 1
}

Write-Host "Context Policy Check: PASS — no heavy default reads detected."
exit 0
