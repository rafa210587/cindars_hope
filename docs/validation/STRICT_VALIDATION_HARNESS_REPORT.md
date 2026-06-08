# Strict Validation Harness Report

> **Status:** READY  
> **Created:** 2026-06-08  
> **Purpose:** Prevent false build success from filtered output

---

## Problem Fixed

**Prior incident:** WAVE 05 batch (commits fcfe6d0 → 53e9698)

- Agent ran: `dotnet build ... | Select-String "error"`
- Syntax error in ConstructionJobTests.cs went undetected
- Agent declared "build PASS" because filtered output showed no "error" string
- Exit code was lost; error went hidden
- Build failure was only discovered manually

**Root cause:** Filtering output with `Select-String` or pipes loses `$LASTEXITCODE`, allowing false success claims.

---

## Solution Implemented

### 1. Truth Gate Rule

**`.claude/rules/build_validation_truth_gate.md`**

- Forbids `dotnet build | Select-String` pattern
- Requires explicit `$LASTEXITCODE` checks
- Mandates use of central `run_strict_validation.ps1`
- Applies to all WAVE 05+ specs

### 2. Central Validation Script

**`tools/docs/run_strict_validation.ps1`**

Single script that:
- Runs all validations in sequence
- Checks exit code after each step
- **Fails immediately** on any failure
- Produces JSON artifact: `docs/validation/LAST_STRICT_VALIDATION_RESULT.json`
- Returns clear status: `VALIDATION_PASS` or failure reason

### 3. Quality Gate Enhancement

**`tools/docs/check_spec_quality.ps1` (updated)**

Added 3 new checks:
- Detects `dotnet build | Select-String` pattern in reports/commands
- Requires `run_strict_validation.ps1` citation in WAVE 05+ specs
- Fails on phrases like "Builds: PASS" without exit-code evidence

### 4. Documentation

**All command/rule files updated:**
- `.claude/commands/execute-spec-strict.md`
- `.claude/commands/loop-spec-batch-strict.md`
- `.claude/rules/spec_quality_gate.md`
- `.claude/rules/windows_powershell_only.md`

---

## Files Created / Updated

| File | Type | Purpose |
|------|------|---------|
| `.claude/rules/build_validation_truth_gate.md` | Rule (NEW) | Forbid filtered build validation |
| `tools/docs/run_strict_validation.ps1` | Script (NEW) | Central validation with exit codes |
| `docs/validation/STRICT_VALIDATION_HARNESS_REPORT.md` | Report (NEW) | This document |
| `.claude/commands/execute-spec-strict.md` | Command (UPD) | Use run_strict_validation |
| `.claude/commands/loop-spec-batch-strict.md` | Command (UPD) | Strict validation per spec |
| `.claude/rules/spec_quality_gate.md` | Rule (UPD) | Add validation truth gate |
| `.claude/rules/windows_powershell_only.md` | Rule (UPD) | Exit code capture |
| `tools/docs/check_spec_quality.ps1` | Script (UPD) | Detect forbidden patterns |
| `docs/validation/WAVE_05_BUILD_HOTFIX_REPORT.md` | Report (UPD) | Reference new harness |

---

## Required Validation Pattern

**Old (forbidden):**
```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore 2>&1 | Select-String "error"
# Result: Output filtered, exit code lost, false PASS possible
```

**New (required):**
```powershell
.\tools\docs\run_strict_validation.ps1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Validation failed"
    exit 1
}
# Result: Clear PASS/FAIL, exit code preserved
```

---

## Status Taxonomy

After running validation, report one of:

| Status | Action |
|--------|--------|
| `VALIDATION_PASS` | Can commit spec implementation |
| `DOCS_EXPECTED_FAIL_LEGACY_ONLY` | Docs have legacy errors; not blocking |
| `BUILD_FAILURE_ASSEMBLY_CSHARP` | Do NOT commit; fix build |
| `BUILD_FAILURE_ASSEMBLY_CSHARP_EDITOR` | Do NOT commit; fix build |
| `QUALITY_CHECK_FAILURE` | Do NOT commit; fix quality issues |

---

## Baseline Before Harness

**Hotfix commit:** `53e9698`

| Check | Result |
|-------|--------|
| Assembly-CSharp | PASS ✓ |
| Assembly-CSharp-Editor | PASS ✓ (3 pre-existing warnings) |
| Quality check | PASS ✓ |
| Docs validation | LEGACY_ERRORS_ONLY |

---

## Baseline After Harness

Running `run_strict_validation.ps1` now produces:

```text
STRICT_VALIDATION_RESULT: VALIDATION_PASS
Exit code: 0
Result file: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

---

## Policy: Mandatory for WAVE 05+

Every spec execution in WAVE 05 and later:

1. **Must run:** `.\tools\docs\run_strict_validation.ps1`
2. **Must check:** `$LASTEXITCODE -eq 0`
3. **Must report:** Exit code and result status in execution report
4. **Must NOT use:** `Select-String` or pipes to infer build success

---

## Audit Trail

- **2026-06-08 fcfe6d0:** Harness 1.0 (dependency resolution + PowerShell)
- **2026-06-08 53e9698:** Hotfix (ConstructionJobTests syntax)
- **2026-06-08 [this]:** Harness 2.0 (strict validation truth gate)

---

## Can Resume WAVE 05?

**YES ✓**

**Prerequisite:** Run new harness once to establish baseline

```powershell
Set-Location 'D:\Projetos\Jogos\Cindars_hope\cindars_hope'
.\tools\docs\run_strict_validation.ps1
```

Expected result: `VALIDATION_PASS` (exit code 0)

**Next spec:**
```
05_spec_farm_animals_housing_feeding_care_runtime
```

---

*Created: 2026-06-08 (Build Validation Truth Gate + Strict Harness)*  
*Mandatory enforcement begins immediately for all WAVE 05+ spec execution.*
