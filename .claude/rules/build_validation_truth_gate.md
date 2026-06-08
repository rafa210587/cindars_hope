# Rule: Build Validation Truth Gate

## Central Rule

**Never infer build success from filtered output.**

Build success is valid **only** when the process exit code is 0.

---

## Why

The ConstructionJobTests hotfix exposed a critical gap: an agent can declare "build PASS" by filtering `dotnet build` output with `Select-String`, which:

1. **Hides real errors** — the error is in stderr, but piping to `Select-String` may miss it
2. **Loses exit code** — `Select-String` consumes `$LASTEXITCODE`; the agent sees green output and falsely assumes success
3. **Delays detection** — a broken build goes undetected through multiple specs until the next full validation run

This rule prevents that pattern permanently.

---

## Forbidden Build Validation Patterns

Do **NOT** use any of these patterns to decide build success:

```powershell
# ❌ FORBIDDEN
dotnet build ... | Select-String "error"
dotnet build ... 2>&1 | Select-String "error|Error"
dotnet build ... | Out-String
dotnet build ... | Tee-Object
```

These patterns may:
- Lose `$LASTEXITCODE`
- Filter away real errors
- Produce false PASS claims

---

## Required Pattern

Use **explicit exit code capture**:

```powershell
# ✅ REQUIRED
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Assembly-CSharp build FAILED"
    exit 1
}

dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Assembly-CSharp-Editor build FAILED"
    exit 1
}
```

---

## Preferred Command

Use the central validation script:

```powershell
.\tools\docs\run_strict_validation.ps1
if ($LASTEXITCODE -ne 0) {
    # Validation failed; do not commit
    exit 1
}
```

This script:
- Runs all validations in order
- Checks exit codes for each step
- Returns single clear result
- Produces JSON artifact for auditing
- Fails immediately on any failure

---

## Required Status Taxonomy

When reporting validation results:

| Status | Meaning |
|--------|---------|
| `VALIDATION_PASS` | All checks passed; exit code 0 |
| `DOCS_EXPECTED_FAIL_LEGACY_ONLY` | Docs have legacy errors (pre-existing); not blocking |
| `DOCS_NEW_FAILURE` | Docs validation failed with new errors; stop |
| `BUILD_FAILURE_ASSEMBLY_CSHARP` | Assembly-CSharp build failed; stop |
| `BUILD_FAILURE_ASSEMBLY_CSHARP_EDITOR` | Assembly-CSharp-Editor build failed; stop |
| `QUALITY_CHECK_FAILURE` | Quality check failed; stop |
| `VALIDATION_SCRIPT_FAILURE` | Validation script itself failed; investigate |

---

## Applies To

All agent-run spec execution, validation, and quality gate tasks.

### Mandatory for WAVE 05 and later

Every spec execution in WAVE 05+ **must** run `run_strict_validation.ps1` and report its result before marking any status as `BUILD_VALIDATED` or better.

---

## Commit Policy

**Do NOT commit spec implementation if:**

- Assembly-CSharp build failed (`exit 1` received)
- Assembly-CSharp-Editor build failed (`exit 1` received)
- check_spec_quality.ps1 failed
- Validation result is unknown or not run
- Build was checked using filtered output only
- run_strict_validation.ps1 did not complete

**Only commit if:**

- run_strict_validation.ps1 exit code is 0
- All checks reported PASS (or EXPECTED_FAIL_LEGACY_ONLY for docs)
- Execution report documents the validation method

---

## Report Policy

Every execution report must include validation metadata:

```text
## Validation

Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS
Assembly-CSharp-Editor: PASS
Quality check: PASS
Docs validation: PASS / EXPECTED_FAIL_LEGACY_ONLY
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

Never say:

```text
❌ Builds: PASS
❌ Assembly-CSharp build: PASS (from filtered Select-String output)
```

Instead:

```text
✓ Assembly-CSharp: PASS (run_strict_validation.ps1 exit code 0)
```

---

## Detection / Audit

The updated `tools/docs/check_spec_quality.ps1` will detect and fail on:

1. **Filtered build patterns** in execution reports or command definitions
2. **Missing run_strict_validation** in WAVE 05+ spec reports
3. **Prohibited phrases** ("Builds: PASS") without validation evidence
4. **Unknown validation result** (report doesn't cite validation method)

---

## Historical Note

**Prior incident (2026-06-08):**

- Commit `fcfe6d0` introduced harness with dependency resolution
- Batch WAVE 05 farm_buildings generated syntax error in ConstructionJobTests.cs
- Agent declared "build PASS" because it used:
  ```powershell
  dotnet build ... | Select-String "error"
  ```
  which filtered output but lost `$LASTEXITCODE`
- Error went undetected until manual hotfix (commit `53e9698`)

This rule ensures that pattern never happens again.

---

*Created: 2026-06-08 (Build Validation Truth Gate)*  
*Mandatory for all spec execution, especially WAVE 05+*
