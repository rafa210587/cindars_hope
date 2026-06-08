# Spec Execution Harness Report

> **Status:** READY_WITH_CURRENT_WAVE_WARNINGS  
> **Created:** 2026-06-08  
> **Purpose:** Lightweight quality gates for spec execution

---

## What This Harness Is

A set of:
1. **Rule** (`.claude/rules/spec_quality_gate.md`) — when to use each status
2. **Command** (`.claude/commands/execute-spec-strict.md`) — how to execute specs safely
3. **Report template** (`docs/specs/SPEC_EXECUTION_REPORT_TEMPLATE_STRICT.md`) — mandatory evidence format
4. **Quality script** (`tools/docs/check_spec_quality.ps1`) — automated validation
5. **This documentation** — integration guide

---

## Files Created

| File | Purpose | Type |
|---|---|---|
| `.claude/rules/spec_quality_gate.md` | Define valid status values and when to use each | Rule (mandatory) |
| `.claude/commands/execute-spec-strict.md` | Execute one spec with quality rigor | Command (manual + loop-safe) |
| `docs/specs/SPEC_EXECUTION_REPORT_TEMPLATE_STRICT.md` | Mandatory report structure | Template |
| `tools/docs/check_spec_quality.ps1` | Automated quality checks | Script (manual or pre-commit) |
| `docs/validation/SPEC_EXECUTION_HARNESS_REPORT.md` | This file | Documentation |

---

## What This Harness Prevents

| Problem | Prevent How | Status |
|---------|-------------|--------|
| Build-only false positives | Report required, compliance matrix required, status rationale required | ✓ |
| Status inflation (CONTRACT_ONLY → BUILD_VALIDATED) | Quality script checks report sections | ✓ |
| Missing execution reports | Report generation is mandatory step in `/execute-spec-strict` | ✓ |
| Tests under runtime scripts | Quality script detects `Assets/_Game/Scripts/**/*Tests.cs` | ✓ |
| Forbidden files altered (Packages, ProjectSettings, scenes, prefabs, assets) | Quality script scans git status | ✓ |
| `.claude/*.lock` committed | Quality script detects lock files | ✓ |
| CONTRACT_ONLY treated as implementation complete | Explicit CONTRACT_ONLY status guides next steps | ✓ |
| Loop drift (executing too many specs, ignoring blockers) | `/execute-spec-strict` stops after one spec; loop re-invokes | ✓ |

---

## How to Use This Harness

### Manual Execution (Single Spec)

```text
/execute-spec-strict docs/specs/a_implementar/04_spec_ui_calendar_day_detail_runtime.md
```

Output:
```text
SPEC_EXECUTION_RESULT
Spec: 04_spec_ui_calendar_day_detail_runtime
Status: BUILD_VALIDATED
...
```

Then manually review and decide next action.

### Loop-Safe Execution (Multiple Specs: Up to 10 per batch)

**Small batch (new/unstable wave):**
```text
/loop
Run /execute-spec-strict next --wave 04
Max specs this batch: 3
Stop on: NEEDS_REWORK, BLOCKED, or quality check failure
```

**Large batch (established wave with known patterns):**
```text
/loop
Run /execute-spec-strict next --wave 05
Max specs this batch: 10
Stop on: BLOCKED, NEEDS_REWORK, CONTRACT_ONLY_NEEDS_INTEGRATION on foundational spec, build failure, docs validation new failure, quality check failure
Commit after each successful spec
Do not start next wave in this loop
```

Loop will:
1. Invoke `/execute-spec-strict`
2. Wait for command to complete (one spec)
3. Check if status allows continuing
4. Re-invoke with `next` flag, or stop
5. Repeat up to N times (3 or 10, depending on batch config)

### Manual Quality Check

```powershell
.\tools\docs\check_spec_quality.ps1
```

Output:
```text
SPEC_QUALITY_CHECK: PASS
```

or

```text
SPEC_QUALITY_CHECK: FAIL
❌ Forbidden files altered:
   - Packages/com.example/file.json
...
```

Exit code: 0 (pass) or 1 (fail).

### Pre-Commit Hook (Future)

Can be added to `.git/hooks/pre-commit`:

```bash
#!/bin/bash
cd "$(git rev-parse --show-toplevel)"
pwsh .\tools\docs\check_spec_quality.ps1 || exit 1
```

Currently **NOT auto-running** — manual execution only.

---

## Current Status: WAVE 04

### Known Unresolved Issues

**This harness is being introduced AFTER WAVE 04 batch execution.** Current state:

| Issue | Status | Impact |
|-------|--------|--------|
| Missing execution reports | SPECS 3-7, 9, 11-16 lack individual reports | Blocks WAVE 05 start |
| Status inflation | 12 specs marked BUILD_VALIDATED, should be CONTRACT_ONLY | Will catch with script |
| SPEC 8 critical blocker | NEEDS_REWORK → BUILD_VALIDATED_WITH_WARNINGS (patched) | Resolved |
| Test file locations | Fixed in prior patches (moved to EditMode/) | Resolved |

### Expected Script Behavior on Current WAVE 04

Running quality check now:

```powershell
.\tools\docs\check_spec_quality.ps1
```

May warn or fail on WAVE 04 issues, which is **EXPECTED_FAIL_CURRENT_WAVE_04**:

```text
⚠ Review: WAVE_04_LOOP_BATCH_STATUS.md mentions BUILD_VALIDATED; ensure corresponding report exists
   (12 specs missing reports, so warning expected)
```

This is acceptable because:
1. WAVE 04 was executed BEFORE harness creation
2. New rules don't retroactively invalidate prior work
3. Going forward, quality check will prevent this

### Recommended Next Flow

1. ✅ SPEC 8 rework + patch complete and pushed
2. ⏳ Create missing WAVE 04 reports (SPECS 3-7, 9, 11-16) with honest status (CONTRACT_ONLY)
3. ✅ Run quality check (will warn about WAVE 04, which is expected)
4. 🚀 Resume WAVE 04 execution using `/execute-spec-strict` for any remaining specs
5. 🚀 Once WAVE 04 reports complete, use `/execute-spec-strict` for WAVE 05

---

## Rule: spec_quality_gate.md

**What it defines:**
- 7 allowed status values during execution
- 2 prohibited status values (ACCEPTED, PLAYMODE_VALIDATED without justification)
- Checklist of 12 criteria for BUILD_VALIDATED
- Decision matrix for choosing correct status

**Key rule:**
```
Proibido marcar BUILD_VALIDATED sem:
1. Report individual
2. Compliance matrix com OK para critérios centrais
3. Build/docs/test validação PASS
4. Auditorias de sistemas existentes
5. Justificativa honesta de por que status não inflado
```

---

## Command: execute-spec-strict.md

**What it does:**
- Executes ONE spec with quality rigor
- Reads spec, audits existing systems, implements, creates report
- Validates build/docs/tests, checks quality
- Stops after one spec (loop-safe: doesn't loop itself)

**Mandatory steps:**
1. Read spec completely
2. Extract acceptance criteria
3. Audit existing systems (don't create parallels)
4. Decide strategy (reuse/adapter/contract/deferred/blocked)
5. Implement minimally
6. Create execution report
7. Fill Spec Compliance Matrix
8. Validate (docs/build/tests/quality check)
9. Classify status honestly
10. Commit if all passed
11. **STOP** (don't proceed to next spec)

**Output:**
```
SPEC_EXECUTION_RESULT
Spec: <id>
Status: <status>
...
Can continue next spec: Y/N
Can start next wave: Y/N
```

---

## Template: SPEC_EXECUTION_REPORT_TEMPLATE_STRICT.md

**Mandatory sections:**
1. Acceptance Criteria Extracted — table of spec criteria + evidence + status
2. Existing Systems Audit — found systems, decision to reuse/adapt/create
3. Scope Executed — what was in/out of scope, confirmed respected
4. Files Changed — detailed list with reasons
5. Tests / Validators — where tests live, status
6. Spec Compliance Matrix — spec requirement → implementation evidence → status
7. Validation — docs/build/editor/EditMode results
8. Honest Status Rationale — why status is not inflated
9. Remaining Work — what's deferred and why

**Status meanings in Compliance Matrix:**
- `OK` — implemented
- `OK_WITH_WARNINGS` — implemented, known limits
- `CONTRACT_ONLY` — contract/DTO only
- `DEFERRED` — explicit defer documented
- `FAIL` — not implemented, defer not documented
- `NOT_APPLICABLE` — not relevant

---

## Script: check_spec_quality.ps1

**Checks:**
1. Forbidden files not altered (Packages, ProjectSettings, scenes, prefabs, assets, `.claude/*.lock`)
2. Tests are under `Assets/_Game/Tests/EditMode/**`, never `Assets/_Game/Scripts/**`
3. Execution reports contain 5 mandatory sections
4. Status fields don't contain ACCEPTED/PLAYMODE_VALIDATED without justification
5. BUILD_VALIDATED claims are checked (warning if report might be missing)

**Output:**
```
SPEC_QUALITY_CHECK: PASS / FAIL
Exit code: 0 / 1
```

**Can be run manually:**
```powershell
.\tools\docs\check_spec_quality.ps1
```

**Can be added to pre-commit hook (future):**
```bash
pwsh .\tools\docs\check_spec_quality.ps1 || exit 1
```

---

## Integration Timeline

### Phase 1 — Now (2026-06-08)
- ✅ Files created
- ✅ Documentation written
- ✅ No auto-hooks (manual only)
- ✅ WAVE 04 completion in progress

### Phase 2 — After WAVE 04 Completion
- Create missing WAVE 04 reports with honest status
- Run quality check (will warn expected issues, then pass)
- Resume WAVE 04 execution using `/execute-spec-strict`

### Phase 3 — WAVE 05 and Beyond
- Use `/execute-spec-strict` for all new spec execution
- Use `/loop` + `/execute-spec-strict` for batch execution (with safety stops)
- Quality check runs manually before commits

### Phase 4 — Future (Optional)
- Add pre-commit hook for automated checks
- Integrate quality check into CI/CD pipeline
- No changes to this rule unless governance evolves

---

## Not Required By This Harness

These are **NOT** required by the harness (still manually decided):

- When to run `/loop` vs single spec execution
- Which `/loop` interval to use
- Whether to schedule future waves
- Whether to mark specs as ACCEPTED (harness forbids it; accept decision is manual + phase-gated)

This is intentional: harness enforces quality, not workflow. Teams choose their own tempo.

---

## Troubleshooting

### Quality Check Fails on Forbidden Files
```
❌ Forbidden files altered: Packages/...
```
→ Revert with `git checkout Packages/...` or exclude via git rm.

### Quality Check Fails on Tests in Scripts
```
❌ Tests found in Assets/_Game/Scripts/**:
   - Assets/_Game/Scripts/UI/Crafting/CraftingRecipeViewModelTests.cs
```
→ Move file: `git mv Assets/_Game/Scripts/.../Tests.cs Assets/_Game/Tests/EditMode/.../Tests.cs`

### Quality Check Fails on Missing Report Sections
```
❌ Execution reports missing mandatory sections:
   - spec_04_execution_report.md: missing section 'Spec Compliance Matrix'
```
→ Add missing section to report using `SPEC_EXECUTION_REPORT_TEMPLATE_STRICT.md`.

### Quality Check Warns on BUILD_VALIDATED without Report
```
⚠ Review: CURRENT_STATE.md mentions BUILD_VALIDATED; ensure corresponding report exists
```
→ Create report: `docs/validation/<spec_id>_execution_report.md`

---

## Loop Batch Policy Summary

**Max specs per batch:**
- **3 specs:** Recommended for new/unstable waves (WAVE 04 phase 1)
- **10 specs:** Allowed for established waves with known patterns (WAVE 05+, homogeneous specs)
- **Never >10:** Without external code review

**Quality is enforced per-spec, not at batch end.** Each spec must pass all gates before continuing to the next.

**See:** `.claude/rules/spec_quality_gate.md` → "Loop Batch Policy" section for detailed rules.

---

## FAQ

**Q: Does this harness require using `/loop`?**  
A: No. Use `/execute-spec-strict` manually for one spec, or use `/loop` for up to 10 specs. Harness is agnostic.

**Q: Can I skip the execution report?**  
A: No. Report is mandatory for any status. Even CONTRACT_ONLY needs a report.

**Q: Can I use BUILD_VALIDATED for CONTRACT_ONLY code?**  
A: No. CONTRACT_ONLY code cannot be BUILD_VALIDATED. Use the correct status.

**Q: What if a spec is BLOCKED?**  
A: Document why in execution report, create report with BLOCKED status, don't commit code, stop. Next spec cannot execute until blocker is resolved.

**Q: Can quality check be automated?**  
A: Yes, can be added to pre-commit hook later. For now, manual execution only.

**Q: What about WAVE 04 issues?**  
A: Harness doesn't retroactively invalidate prior work. WAVE 04 issues (missing reports, status inflation) are addressed separately by creating missing reports. Quality check will note this as EXPECTED_FAIL_CURRENT_WAVE_04 until resolved.

---

## Sign-Off

**Status:** READY  
**Safe to use:** YES  
**Blocks any existing workflow:** NO  
**Recommended for:** All future spec execution (WAVE 05+)  
**Optional integration:** Pre-commit hook (Phase 4)

---

*Harness created: 2026-06-08*  
*Designed for lightweight, manual-first, loop-safe spec execution*  
*No mandatory automation; teams choose their own tempo*
