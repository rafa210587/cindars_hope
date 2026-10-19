# WAVE 05 Batch State

> **Status:** DEPENDENCY_RESOLUTION_READY  
> **Created:** 2026-06-08  
> **Purpose:** Track dependency resolution and batch execution state for WAVE 05 specs

---

## Current Batch Configuration

| Setting | Value |
|---------|-------|
| Wave | 05 |
| Max specs per batch | 10 |
| Execution mode | STRICT_WITH_DEPENDENCY_RESOLUTION |
| Dependency plan | `docs/validation/WAVE_05_DEPENDENCY_RESOLUTION_PLAN.md` |
| Current status | READY (awaiting `/loop` invocation) |

---

## Current Dependency Stack

No active dependency resolution in progress.

| Order | Spec | Status | Commit |
|-------|------|--------|--------|

---

## Executed Specs This Batch

None yet (batch not started).

| Spec | Status | Validation | Commit |
|------|--------|-----------|--------|

---

## Pending Specs

Not yet evaluated. Will populate as specs are executed.

| Spec | Blocked By | Plan | Status |
|------|-----------|------|--------|

---

## Batch Summary

| Metric | Value |
|--------|-------|
| Specs started | 0 |
| Specs completed | 0 |
| Specs blocked | 0 |
| Dependency chains resolved | 0 |
| Commands retried (Unix→PowerShell) | 0 |

---

## Next Action

**Awaiting:** `/loop` invocation with WAVE 05 specs

**Command to run:**
```text
/loop
Run /execute-spec-strict next --wave 05
Max specs this batch: 10
Stop on: BLOCKED, NEEDS_REWORK, CONTRACT_ONLY_NEEDS_INTEGRATION on foundational spec, DEFERRED_UI_VISUAL on foundational spec, build failure, docs validation new failure, quality check failure
Commit after each successful spec
Do not start next wave in this loop
Do not mark ACCEPTED
```

**What will happen:**
1. First `/execute-spec-strict` invocation will identify target spec
2. If target has same-wave dependency, dependency chain will be resolved automatically
3. Each spec in chain will execute and return BUILD_VALIDATED or better
4. Dependency plan will be updated with execution order and commits
5. Batch state will be updated after each spec
6. Loop will continue until BLOCKED or max 10 specs reached

---

## Stop Conditions

Loop will STOP IMMEDIATELY if any spec:

- Status is `BLOCKED` (final)
- Status is `NEEDS_REWORK` (P0/P1 spec incomplete)
- Status is `CONTRACT_ONLY_NEEDS_INTEGRATION` on foundational spec
- Status is `DEFERRED_UI_VISUAL` on foundational spec
- Build fails with new errors
- Docs validation fails with new errors
- Quality check fails critically
- Execution report is missing/incomplete

---

## State Persistence

This file persists:
- Across agent invocations
- Within a single `/loop` batch (up to 10 specs)
- Dependency chain progress
- Next-spec pointer

**Updated by:** `/execute-spec-strict` after each spec completes

**Read by:** `/loop` before re-invoking `/execute-spec-strict next`

---

## Dependency Resolution Examples

**Example 1: No dependencies**

```
Spec: 05_spec_companion_role_enum_contract
Depends on: (none)
Result: BUILD_VALIDATED ✓
Batch continues to next spec
```

**Example 2: Same-wave dependency chain**

```
Spec: 05_spec_companion_farm_job_board_automation_runtime_execution
Depends on: farm_animals
  farm_animals depends on: farm_buildings
  farm_buildings depends on: farm_building_footprints
  farm_building_footprints depends on: farm_level1_layout
  farm_level1_layout depends on: farm_scale_tilemap
  farm_scale_tilemap depends on: (none)

Resolution order:
  1. farm_scale_tilemap → BUILD_VALIDATED ✓
  2. farm_level1_layout → BUILD_VALIDATED ✓
  3. farm_building_footprints → BUILD_VALIDATED ✓
  4. farm_buildings → BUILD_VALIDATED ✓
  5. farm_animals → BUILD_VALIDATED ✓
  6. companion_farm_job_board → [execute next]

Batch continues to next unrelated spec after original completes
```

**Example 3: Forbidden dependency (stop)**

```
Spec: 05_spec_future_system
Depends on: structure_upgrades (WAVE 06+)

Result: BLOCKED_BY_FORBIDDEN_SCOPE
Reason: Cross-wave dependency
Batch stops
```

---

## Communication During Batch

After each spec, output must include:

```text
SPEC_EXECUTION_RESULT
────────────────────────
Spec:           <spec_id>
Status:         <BUILD_VALIDATED | ... | BLOCKED>
Dependency chain: [resolved / none / forbidden]
Docs validation: ✓ PASS
Assembly build: ✓ PASS
Quality check:  ✓ PASS
Commit:         <hash or NONE>
Can continue:   YES/NO
```

---

*Maintained by: `/execute-spec-strict` and `/loop-spec-batch-strict`*  
*Last updated: 2026-06-08 (initial creation)*
