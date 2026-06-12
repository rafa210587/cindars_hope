# Rule: Validation Truth

Consolidates: `build_validation_truth_gate`, `powershell_script_failure_gate`, `unity-validation-honesty`, `no-premature-acceptance-claims` (originals are stubs pointing here).

## 1. Build success = exit code 0. Nothing else.

Never infer build success from filtered output.

```powershell
# FORBIDDEN (loses $LASTEXITCODE, hides errors) — pre-bash-guard hook blocks this:
dotnet build ... | Select-String "error"

# REQUIRED:
dotnet build .\Assembly-CSharp.csproj --no-restore
if ($LASTEXITCODE -ne 0) { exit 1 }

# PREFERRED (runs everything, checks every exit code, writes JSON artifact):
.\tools\docs\run_strict_validation.ps1
if ($LASTEXITCODE -ne 0) { exit 1 }
```

A spec cannot be `BUILD_VALIDATED` unless `run_strict_validation.ps1` returned exit code 0. Do not commit if any build/check failed, was unknown, or was verified only via filtered output.

## 2. A PowerShell script failure is never secondary

If any validation script throws, prints an exception, returns non-zero, or has `$?` = false: **STOP immediately**. No "secondary issue", no "proceed anyway", no commit. Check both `$?` and `$LASTEXITCODE` after every script; wrap in `try/catch` and treat the catch as failure.

## 3. Validation levels are different claims — never conflate them

| Claim | Means only |
|---|---|
| `dotnet build` PASS | C# fallback compile passed |
| Unity batchmode PASS | Unity compile validation passed |
| Play Mode / manual PASS | gameplay validation passed |

Blocked validation is reported, never converted to PASS:

```text
Unity validation: NOT RUN or BLOCKED
Reason: <lock | license | sandbox | timeout | approval>
Command attempted: <command>
Residual risk: <explicit>
```

Triage: any `error CS` is real unless proven stale; "another Unity instance running" is an editor lock, not a code failure.

## 4. No premature acceptance claims

Never write without evidence in the repo: "MVP accepted", "100% fulfilled", "Play Mode PASS", "Unity validated" (blanket), "Phase 2/3 PASS", "Human acceptance complete".

Honest alternatives: `Phase 0-1 COMPLETE`, `BUILD_VALIDATED`, `Phase 2 NOT RUN`, `Phase 3 PENDING`, `ACCEPTED pending Phase 2-3`.

## Required report block

```text
Validation method: run_strict_validation.ps1
Exit code: 0
Assembly-CSharp: PASS | FAIL
Assembly-CSharp-Editor: PASS | FAIL
Quality check: PASS | FAIL
Docs validation: PASS | EXPECTED_FAIL_LEGACY_ONLY | NEW_FAILURE
Result artifact: docs/validation/LAST_STRICT_VALIDATION_RESULT.json
```

## Enforcement

- Hook `pre-bash-guard.ps1` (PreToolUse) blocks filtered `dotnet build` pipes.
- `tools/docs/check_spec_quality.ps1` detects prohibited claim phrases in reports.

*Historical incidents that motivated this rule: commits fcfe6d0/53e9698 (filtered build hid syntax error) and d053a29 ("secondary issue" committed unvalidated code). Details in git history.*
