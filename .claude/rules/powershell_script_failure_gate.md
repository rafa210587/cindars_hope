# Rule: PowerShell Script Failure Gate

## Central Rule

**A PowerShell script failure is never secondary.**

If any validation script:
- Prints an exception
- Throws an error
- Returns non-zero exit code
- Has `$?` status = `$false`

**Then the entire validation must STOP immediately.**

There is no "secondary issue" or "proceed anyway" exception.

---

## Required Checks

For every `.ps1` script invocation in validation, use safe invocation:

```powershell
# Unsafe (old pattern):
.\tools\docs\check_spec_quality.ps1
# Script fails silently; $? and $LASTEXITCODE lost

# Safe (required):
try {
    & .\tools\docs\check_spec_quality.ps1
    $scriptSuccess = $?
    $scriptExitCode = $LASTEXITCODE
} catch {
    Write-Host "SCRIPT_EXCEPTION: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

if (-not $scriptSuccess) {
    Write-Host "SCRIPT_FAILED_BY_POWERSHELL_STATUS: check_spec_quality.ps1" -ForegroundColor Red
    exit 1
}

if ($null -ne $scriptExitCode -and $scriptExitCode -ne 0) {
    Write-Host "SCRIPT_FAILED_BY_EXIT_CODE: $scriptExitCode" -ForegroundColor Red
    exit 1
}

Write-Host "PASS: check_spec_quality.ps1" -ForegroundColor Green
```

---

## Prohibited Behavior

Do **NOT** do any of these:

```powershell
# ❌ FORBIDDEN: Ignore script exception
try {
    & .\check_spec_quality.ps1
} catch {
    Write-Host "Script error, continuing anyway"
}

# ❌ FORBIDDEN: Treat script failure as warning
if ($LASTEXITCODE -ne 0) {
    Write-Host "Quality check secondary issue"
    # continue
}

# ❌ FORBIDDEN: Silence script output to hide errors
& .\script.ps1 *>&1 | Out-Null
# Script may have failed; no way to know
```

---

## Required Behavior

If **any** validation script fails:

1. **Stop immediately** — do not continue to next step
2. **Do not commit** — the code is not validated
3. **Report failure** — document which script failed and why
4. **Fix the issue** — either:
   - Fix the code that caused validation to fail, or
   - Fix the validation script itself

---

## Script Failure Categories

| Category | Example | Action |
|----------|---------|--------|
| Build failure | Assembly-CSharp exits 1 | STOP, report BUILD_FAILURE |
| Test exception | check_spec_quality.ps1 throws | STOP, report SCRIPT_EXCEPTION |
| Unknown exit code | docs validation returns 2 | STOP, report, investigate |
| Quality gate fail | 10+ forbidden patterns detected | STOP, report QUALITY_FAILURE |

---

## Applies To

- All validation scripts in `tools/docs/`
- All quality gates in `tools/docs/check_spec_quality.ps1`
- All invocations in `run_strict_validation.ps1`
- All `/execute-spec-strict` implementations
- All `/loop-spec-batch-strict` iterations

---

## Historical Note

**Incident (2026-06-08, commit d053a29):**

- `run_strict_validation.ps1` executed `check_spec_quality.ps1`
- Script threw exception: "Should command may only be used inside a Describe block"
- Agent logged: "Quality check: secondary issue"
- Harness continued; commit was created
- Result: unvalidated code was committed (commit 401263f with farm animals code)

This rule prevents that pattern permanently.

---

*Created: 2026-06-08 (Harness Hardening Phase 1)*  
*Mandatory for all validation scripts and spec execution*
