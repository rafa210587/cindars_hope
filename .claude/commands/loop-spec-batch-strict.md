# /loop-spec-batch-strict

Reference text for running a controlled batch of specs via `/loop`.

## Usage

Use this command text inside `/loop` to run a validated batch of specs.

```text
/loop
Run /execute-spec-strict next --wave <WAVE>.
Max specs this batch: 10.
Stop on: BLOCKED, NEEDS_REWORK, CONTRACT_ONLY_NEEDS_INTEGRATION on foundational spec, DEFERRED_UI_VISUAL on foundational spec, build failure, docs validation new failure, quality check failure.
Commit after each successful spec.
Do not start next wave in this loop.
Do not mark ACCEPTED.
```

Replace `<WAVE>` with the target wave (e.g., `05`).

## Rules

### Per-Spec Validation (NOT at end of batch)

1. **One spec per iteration** — each `/loop` cycle executes exactly ONE spec
2. **Max 10 specs per batch** — loop can re-invoke `/execute-spec-strict` up to 10 times
3. **Max recommended per wave:**
   - 3 specs for new/unstable waves (WAVE 04 phase 1)
   - 10 specs for established waves with known patterns (WAVE 05+)
   - Never >10 without external code review

### Quality Gates Per Spec

Each spec MUST pass before continuing to the next:

- ✓ `docs validation` PASS (no new errors)
- ✓ `dotnet build Assembly-CSharp` — 0 errors
- ✓ `dotnet build Assembly-CSharp-Editor` — 0 errors
- ✓ `./tools/docs/check_spec_quality.ps1` PASS (no critical failures)
- ✓ Status is honest (not inflated)
- ✓ Execution report created and complete
- ✓ No forbidden files altered (Packages/, ProjectSettings/, scenes, prefabs, assets, runtime)

### Stop Conditions (Immediate)

Stop the loop IMMEDIATELY if:

- Status is `BLOCKED`
- Status is `NEEDS_REWORK`
- Status is `CONTRACT_ONLY_NEEDS_INTEGRATION` for a foundational spec
- Status is `DEFERRED_UI_VISUAL` for a foundational spec
- Build fails with new errors
- Docs validation fails with new errors
- Quality check fails critically (not just warnings)
- Forbidden file was altered
- Execution report is missing or incomplete
- Status is inflated (e.g., `BUILD_VALIDATED` without evidence)

### Per-Spec Commit

After each spec:

- If status allows continuing (BUILD_VALIDATED, CONTRACT_ONLY, etc.):
  - Create commit: `feat: execute <spec_id> [<priority>]`
  - Continue to next spec
- If status blocks (BLOCKED, NEEDS_REWORK):
  - Document reason in execution report
  - Stop immediately
  - Do NOT commit (unless documentation-only)

## Required Output Format

After each spec in the batch, output MUST include:

```text
SPEC_EXECUTION_RESULT
═══════════════════════

Spec:                           <spec_id>
Status:                         <BUILD_VALIDATED | BUILD_VALIDATED_WITH_WARNINGS | CONTRACT_ONLY | CONTRACT_ONLY_NEEDS_INTEGRATION | DEFERRED_UI_VISUAL | NEEDS_REWORK | BLOCKED>
Acceptance criteria matched:    <count> / <total>
Execution report created:       YES <path> | NO <reason>
Spec Compliance Matrix:         <OK count> / <DEFERRED count> / <FAIL count>

Validation Results
──────────────────
Docs validation:                PASS | FAIL <reason>
Assembly-CSharp:               PASS (0E, <W>W) | FAIL
Assembly-CSharp-Editor:        PASS (0E, <W>W) | FAIL
Quality check:                 PASS | FAIL <reason>

Files Changed
─────────────
Count:                         <N>
Tests in correct location:     YES | NO <wrong locations>
Forbidden files altered:       NO | YES <files>

Commit
──────
Hash:                          <commit hash or NONE>
Message:                       <one-liner>

Decision
────────
Can continue next spec:        YES | NO <reason>
Can start next wave:           NO (always NO in batch loop)
Remaining blockers:            <list or NONE>
```

## Batch Status Is NOT Wave Acceptance

Completing a 10-spec batch does NOT mean:
- `ACCEPTED` (still forbidden)
- `PLAYMODE_VALIDATED` (still forbidden)
- Wave is ready to release (still requires further phases)

A batch can only produce:
- `BUILD_VALIDATED`
- `BUILD_VALIDATED_WITH_WARNINGS`
- `CONTRACT_ONLY`
- `CONTRACT_ONLY_NEEDS_INTEGRATION`
- `DEFERRED_UI_VISUAL`
- `NEEDS_REWORK`
- `BLOCKED`

## Batch Example: WAVE 05 Phase 1

```text
/loop
Run /execute-spec-strict next --wave 05.
Max specs this batch: 10.
Stop on: BLOCKED, NEEDS_REWORK, CONTRACT_ONLY_NEEDS_INTEGRATION on foundational spec, build failure, docs validation new failure, quality check failure.
Commit after each successful spec.
Do not start next wave in this loop.
Do not mark ACCEPTED.
```

This will execute up to 10 WAVE 05 specs, each with full validation.

If WAVE 05 SPEC 1 is `NEEDS_REWORK`, stop immediately.
If WAVE 05 SPEC 5 is `BLOCKED`, stop immediately.
Continue only if each spec passes its per-spec gates.

## Loop Fallback

If the loop encounters an error (e.g., network timeout, file system issue):

1. Check the status of the last spec that ran
2. If it completed validation and report exists: safe to retry `/execute-spec-strict next --wave <WAVE>`
3. If it failed mid-execution: review execution report for errors before retrying

## When to Use

- Recommended for batches of homogeneous specs (e.g., all UI view models)
- Recommended for established waves (WAVE 02+) after phase 1 is stable
- Use 3-spec batches for new waves (WAVE 04, WAVE 05 phase 1)
- Use 10-spec batches for well-understood patterns (UI VMs, DTO creation)

## When NOT to Use

- Do NOT use for P0 specs in new waves (use 1-spec execution first)
- Do NOT use if any foundational spec is undecided (blocked, needs rework)
- Do NOT use if batch contains mixed patterns (some UI, some systems, some logic)
- Do NOT use if previous wave had critical failures

---

*Created: 2026-06-08 (Loop Batch Reference)*
*Use inside `/loop` to run controlled multi-spec batches.*
