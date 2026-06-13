---
doc_type: validation
status: evidence
spec_id: SPEC_CLAUDE_32
validation_type: audit
result: COMPLETE
date: 2026-06-01
executor: Claude Code
source_of_truth: false
---

# Phase 0 Audit Matrix — SPEC_CLAUDE_32 Test Scenario Skill & Delete Candidates Review

> Audit only. No files deleted in this phase.

---

## 1. Current State of `/finish-spec`

| Item | Current | Proposed | Action |
|------|---------|----------|--------|
| Reads execution report | YES | YES | Keep |
| Checks phase status | YES (BUILD_VALIDATED / ACCEPTED) | YES + test scenario | Update |
| Requires test scenario | NO | YES (if runtime/gameplay changed) | Add |
| Output includes "How to Test" | NO | YES (if applicable) | Add |
| Knows what "runtime change" means | Implicit (Unity files changed) | Explicit via change-scope.json | Update |

**Finding:** `/finish-spec` currently does not require human test scenarios. It can promote a spec to `implementados/` based on build validation alone, even if gameplay was implemented.

---

## 2. Test Scenario Skills & Templates Inventory

| Item | Exists | Current Function | Problem | Action |
|------|--------|-----------------|---------|--------|
| `gameplay-test-scenario` skill | NO | — | Missing: no skill guides human test scenario creation | Create |
| `PLAYMODE_TEST_SCENARIO_TEMPLATE.md` | NO | — | Missing: no template for human test format | Create |
| Human test checklist in validation reports | NO | — | Missing: no standard format for recording human test results | Create template |
| Hook for requiring test scenario | NO | — | Missing: no guard against shipping without test plan | Create |

**Finding:** Zero infrastructure for human test scenarios. This is a significant gap given Phase 3 is now mandatory for gameplay specs.

---

## 3. Spec Types & Test Scenario Requirements

| Spec Type | Change Type | Runtime/Assets Changed? | Test Scenario Required? | Current Handling |
|-----------|-------------|----------------------|----------------------|-------------------|
| Docs-only (SPEC_DOCS_30) | Documentation | NO | NO | Skip — currently works |
| Code logic (SPEC_CLAUDE_31) | .cs runtime logic | YES | YES — if gameplay/UI/event bus | NOT REQUIRED — gap |
| Gameplay feature (e.g., SPEC_18) | Game behavior, UI, combat | YES | YES — critical | NOT REQUIRED — gap |
| Asset wiring (asset-wiring-specialist) | ScriptableObject refs | YES | MAYBE — if affects gameplay | NOT REQUIRED — unclear |
| Save/load system change | Save data flow | YES | YES — if save behavior changed | NOT REQUIRED — gap |
| Cave generation change | Procedural generation | YES | YES — critical for stable run | NOT REQUIRED — gap |

**Finding:** Gameplay specs (SPEC_18-28, most future FASE 10+) all require test scenarios but there's no enforcement mechanism.

---

## 4. Hook Inventory

| Hook | Exists | Trigger | Always On | Can Check Test Scenario? |
|------|--------|---------|-----------|--------------------------|
| `pre-bash-guard.ps1` | YES | pre-bash | YES | NO |
| `detect-change-scope.ps1` | YES | post-edit | YES | YES — writes change-scope.json |
| `stop-summary-check.ps1` | YES | stop | YES | Could be extended |
| `test-scenario-required-guard.ps1` | NO | manual | — | Would read change-scope.json + check for test scenario |

**Finding:** `detect-change-scope.ps1` already detects runtime changes. A new manual hook can check if test scenario is documented.

---

## 5. Delete Candidates Inventory

Current state: `docs/00_PROJECT/DOCUMENT_DELETE_CANDIDATES.md` has 28 candidates listed but NOT batched.

### Batch 1 — Safe Now (no active references)

Candidates:
- `.specs/a_implementar/reorg/SPEC_00.md` through `SPEC_12.md` (12 files, CLOSED per README_STATUS.md, never executed)
- `.specs/a_implementar/reorg/SPEC_00_STRATEGY_SUBAGENTS.md`
- `docs/roadmap/NEXT_WAVES_ROADMAP_v1.0.md` (superseded by newer ROADMAP.md)
- `docs/architecture/ARCH_fase4_v2.2.md` (old version)
- `docs/design/GDD_v2.6.md` (old version, v2.7+ exists)
- `docs/design/CHANGELOG_ATUALIZACAO_v2.6.md` (changelog for old GDD)
- `docs/implementation_runs/RUN_*.md` (7 run logs — execution artifacts)
- `docs/IMPLEMENTATION_DELIVERY_20260523.md` (dated delivery report)

**Risk:** LOW — These are superseded, archived, or execution artifacts. Not referenced by active specs or CURRENT_STATE.md.

### Batch 2 — After Phase 2-3 (blocked on human validation)

Candidates:
- `.specs/a_implementar/spec_14a*.md` (covered by SPEC_24 closeout, not yet moved)
- `.specs/a_implementar/spec_14b*.md` (covered by SPEC_24 closeout, not yet moved)
- `.specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md` (covered by SPEC_23, not yet moved)
- `.specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md` (covered by SPEC_24, already in implementados but duplicate in a_implementar)
- `.specs/a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md` (covered by SPEC_28, not yet moved)

**Risk:** MEDIUM — These ARE covered by SPEC_18-28 closeout, but SPEC_18-28 not yet promoted (Phase 2-3 pending). Deleting before promotion could create confusion. Defer until after Phase 2-3.

### Batch 3 — DO NOT DELETE

Protected categories:
- `docs/validation/*` — all validation reports are evidence
- `.specs/implementados/*` — implemented specs are archive
- `docs/refinements/implementados/*` — implemented refinements are archive
- `.specs/a_implementar/closeout_mvp/*` — active closeout specs
- `docs/amendments/*` — amendments are governance
- `docs/operations/*` — operational protocol
- `PROJECT_LOG.md` — historical record
- `docs/IMPLEMENTATION_STATUS.md` — status tracking
- `.specs/SPEC_EXECUTION_ORDER.md` — dependency matrix
- `AGENTS.md`, `CLAUDE.md` — governance
- `docs/00_PROJECT/*` — governance hub

**Risk:** NONE — These are all either active governance, evidence, or operational necessity. DO NOT DELETE without explicit future authorization.

---

## 6. Risk Assessment

| Risk | Category | Severity | Mitigation |
|------|----------|----------|-----------|
| Test scenario not created for gameplay spec | Acceptance | HIGH | Add to `/finish-spec` requirement; create skill; create hook |
| Batch 1 deletion causes confusion | Documentation | LOW | Review against CURRENT_STATE.md and DOCUMENT_INDEX.md before deleting |
| Batch 2 deleted before Phase 2-3 | Workflow | MEDIUM | Do not execute Batch 2 until Phase 2-3 complete or explicitly authorized |
| Hook added but not enabled | Governance | LOW | Document as disabled; enable in future SPEC when needed |
| Test scenario template incomplete | Quality | MEDIUM | Use comprehensive template; provide examples |

---

## Summary

**Problems identified (require action in this spec):**
1. No skill for creating human test scenarios
2. No template for documenting human test scenarios
3. `/finish-spec` does not require test scenarios for runtime/gameplay specs
4. No hook to warn when test scenario is missing
5. Delete candidates not organized into safe batches

**No change needed:**
- Phase 0-1 closeout logic (works for docs-only and code-only specs)
- Validation infrastructure (Phase 1 build validation solid)

**Execution plan:**
1. Create `gameplay-test-scenario` skill
2. Create `PLAYMODE_TEST_SCENARIO_TEMPLATE.md`
3. Update `/finish-spec` to require test scenario for runtime/gameplay
4. Update `spec-execution` skill to invoke test scenario skill when needed
5. Create `test-scenario-required-guard.ps1` hook (disabled)
6. Update governance docs
7. Reorganize delete candidates into 3 batches (no deletion yet)
8. Validate docs
9. Create execution report

---

*Audit complete: 2026-06-01*
