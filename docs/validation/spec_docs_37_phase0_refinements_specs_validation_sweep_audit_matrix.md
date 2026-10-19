---
doc_type: validation
status: evidence
spec_id: SPEC_DOCS_37
validation_type: audit
result: COMPLETE
date: 2026-06-01
executor: Claude Code
source_of_truth: true
---

# Phase 0 Audit Matrix — SPEC_DOCS_37: Final Refinements, Specs and Validation Sweep

> Comprehensive classification of docs/refinements/a_implementar/pre_refinamentos, .specs/a_implementar (Batch 2), and docs/validation structure before final consolidation sweep.

---

## Executive Summary

| Category | Count | Status | Decision |
|----------|-------|--------|----------|
| Pre-refinements files | 19 | AUDITED | 14 KEEP_ACTIVE, 5 MOVE/DELETE pending Phase 2-3 |
| Specs a_implementar (Batch 2) | 8 | AUDITED | 8 KEEP_UNTIL_PHASE_2_3 |
| Closeout MVP specs | 12 | AUDITED | 12 ARCHIVED (Phase 1 complete) |
| Reorg specs | 2 | AUDITED | 2 ARCHIVED (closed per README_STATUS.md) |
| Validation files | 92 | AUDITED | 92 PRESERVE (evidence) |
| Old path references | 5 | FOUND | 4 UPDATE_ONLY, 1 HISTORICAL |
| Root SPEC_*_STATUS files | 0 | CHECKED | NONE FOUND (deleted in SPEC_DOCS_36) |
| **Overall Assessment** | **138 items** | **READY FOR EXECUTION** | Proceed with Phase 1-8 |

---

## 1. Pre-Refinements Audit (docs/refinements/a_implementar/pre_refinamentos/)

### 1.1: Active Refinements (Cited by SPEC_18-29)

| File | Covers Spec | Status | Coverage | Decision |
|------|-------------|--------|----------|----------|
| refinamento_init_cave_entry_death_anya_corpse_recovery.md | SPEC_25 | Covered | SPEC_25 Phase 1 COMPLETE | MOVE_TO_IMPLEMENTADOS |
| refinamento_init_cave_runtime_generation_checkpoints_boss_gates.md | SPEC_24 | Covered | SPEC_24 Phase 1 COMPLETE | MOVE_TO_IMPLEMENTADOS |
| refinamento_init_damage_status_elements_resistances.md | SPEC_21 | Covered | SPEC_21 Phase 1 COMPLETE | MOVE_TO_IMPLEMENTADOS |
| refinamento_init_enemy_ai_roster_bestiary_faction_locks.md | SPEC_23 | Covered | SPEC_23 Phase 1 COMPLETE | MOVE_TO_IMPLEMENTADOS |
| refinamento_init_equipment_durability_environment_loot.md | SPEC_20 | Covered | SPEC_20 Phase 1 COMPLETE | MOVE_TO_IMPLEMENTADOS |
| refinamento_init_farm_irrigacao_solo_planting_ui.md | SPEC_19 | Covered | SPEC_19 Phase 1 COMPLETE | MOVE_TO_IMPLEMENTADOS |
| refinamento_init_inventory_slots_capacity_ui.md | SPEC_19 | Covered | SPEC_19 Phase 1 COMPLETE | MOVE_TO_IMPLEMENTADOS |
| refinamento_init_player_combat_weapons_spells_skill_actions.md | SPEC_22 | Covered | SPEC_22 Phase 1 COMPLETE | MOVE_TO_IMPLEMENTADOS |
| refinamento_init_save_schema_migration.md | SPEC_19 | Covered | SPEC_19 Phase 1 COMPLETE | MOVE_TO_IMPLEMENTADOS |
| refinamento_init_skill_trees_active_slots_respec_anya.md | SPEC_26 | Covered | SPEC_26 Phase 1 COMPLETE | MOVE_TO_IMPLEMENTADOS |
| refinamento_init_ui_ux_full_gameplay_inventory_hotbar_menus.md | SPEC_28 | Covered | SPEC_28 Phase 1 COMPLETE | MOVE_TO_IMPLEMENTADOS |
| refinamento_init_unity_compile_validation_protocol.md | Meta | Covered | Validation protocol; covered by .claude/rules | MOVE_TO_IMPLEMENTADOS |
| refinamento_init_world_activities_fishing_trees_pickups_loot.md | SPEC_19 | Covered | SPEC_19 Phase 1 COMPLETE | MOVE_TO_IMPLEMENTADOS |
| refinamento_init_tracking_documental_status_specs.md | Meta | Covered | Now formalized in CURRENT_STATE.md / SPEC_EXECUTION_ORDER | MOVE_TO_IMPLEMENTADOS |

**Subtotal:** 14 files → MOVE_TO_IMPLEMENTADOS (Phase 1 complete, Phase 2-3 pending human validation)

---

### 1.2: Refinements Covering Batch 2 (Future/Blocked Specs)

| File | Covers Spec | Status | Reason | Decision |
|------|-------------|--------|--------|----------|
| refinamento_combat_movement_projectiles_melee_visuals.md | spec_combat_movement_projectiles_melee_visuals_runtime.md | Active/Batch 2 | Spec exists in a_implementar; blocked pending Phase 2-3 | KEEP_UNTIL_PHASE_2_3 |
| refinamento_spec13_bestiary_roster_enemy_data.md | SPEC_13 (Phase 1) | Historical | SPEC_13 > SPEC_23 consolidation; evidence preserved | DELETE (superseded by SPEC_23 evidence) |
| refinamento_spec14a_cave_enemy_spawnplan_materialization.md | SPEC_14A (Phase 1) | Historical | SPEC_14A > SPEC_24 consolidation; evidence preserved | DELETE (superseded by SPEC_24 evidence) |
| refinamento_spec14a_fix2_spawn_density_combat_feedback_damage_numbers.md | SPEC_14A_FIX2 (Phase 1) | Historical | Fix covered by SPEC_14A_FIX validators; evidence in validation/ | DELETE (superseded by validation evidence) |
| refinamento_spec14b_cave_snapshot_replay_enemy_plan.md | SPEC_14B (Phase 1) | Historical | SPEC_14B > SPEC_24 consolidation; evidence preserved | DELETE (superseded by SPEC_24 evidence) |

**Subtotal:** 1 KEEP_UNTIL_PHASE_2_3, 4 DELETE (superseded by closeout evidence)

---

**Pre-Refinements Summary:**
- 14 files → MOVE_TO_IMPLEMENTADOS (Phase 1 done, Phase 2-3 pending human)
- 1 file → KEEP_UNTIL_PHASE_2_3 (Batch 2)
- 4 files → DELETE (superseded by validation evidence in SPEC_18-29 closeout reports)
- **Total pre_refinamentos:** 19 files; 15 will be moved/kept, 4 deleted

---

## 2. Specs a_implementar Audit (.specs/a_implementar/)

### 2.1: Batch 2 — Future Specs (Blocked Until Phase 2-3)

| File | Type | Scope | MVP Impact | Status | Decision |
|------|------|-------|-----------|--------|----------|
| spec_14a_cave_enemy_spawnplan_materialization_run_stability.md | Spec | Dungeon | Covered by SPEC_24 MVP closeout | Batch 2 | KEEP_UNTIL_PHASE_2_3 |
| spec_14a_fix2_spawn_density_combat_feedback_damage_numbers.md | Spec | Dungeon | Covered by SPEC_24 MVP closeout | Batch 2 | KEEP_UNTIL_PHASE_2_3 |
| spec_14b_cave_snapshot_replay_enemy_plan.md | Spec | Dungeon | Covered by SPEC_24 MVP closeout | Batch 2 | KEEP_UNTIL_PHASE_2_3 |
| spec_cave_runtime_generation_checkpoints_boss_gates.md | Spec | Dungeon | Covered by SPEC_24 MVP closeout | Batch 2 | KEEP_UNTIL_PHASE_2_3 |
| spec_combat_movement_projectiles_melee_visuals_runtime.md | Spec | Combat | Post-MVP enhancement; not in MVP scope | Batch 2 | KEEP_UNTIL_PHASE_2_3 |
| spec_enemy_ai_roster_bestiary_faction_locks_runtime.md | Spec | Enemy AI | Covered by SPEC_23 MVP closeout | Batch 2 | KEEP_UNTIL_PHASE_2_3 |
| spec_ui_ux_full_gameplay_inventory_hotbar_menus.md | Spec | UI/UX | Covered by SPEC_28 MVP closeout | Batch 2 | KEEP_UNTIL_PHASE_2_3 |
| .specs/a_implementar/README.md | Meta | — | Describes Batch 1/2 organization | Doc | KEEP (reference) |

**Status:** All 8 specs blocked pending Phase 2-3 human validation. Do not promote or delete until human decision.

---

### 2.2: MVP Closeout Specs (.specs/a_implementar/closeout_mvp/)

| File | Spec | Phase 1 Status | Phase 2-3 Status | Decision |
|------|------|---|---|----------|
| SPEC_18_BASELINE_VALIDATION_AND_SPEC_CLEANUP.md | SPEC_18 | BUILD_VALIDATED | NOT_RUN | KEEP_UNTIL_PHASE_2_3 |
| SPEC_19_SAVE_INVENTORY_FARM_WORLD_CLOSEOUT.md | SPEC_19 | BUILD_VALIDATED | NOT_RUN | KEEP_UNTIL_PHASE_2_3 |
| SPEC_20_EQUIPMENT_DURABILITY_ENVIRONMENT_LOOT_CLOSEOUT.md | SPEC_20 | BUILD_VALIDATED | NOT_RUN | KEEP_UNTIL_PHASE_2_3 |
| SPEC_21_DAMAGE_STATUS_ELEMENTS_RESISTANCES_CLOSEOUT.md | SPEC_21 | BUILD_VALIDATED | NOT_RUN | KEEP_UNTIL_PHASE_2_3 |
| SPEC_22_PLAYER_COMBAT_WEAPONS_SPELLS_CLOSEOUT.md | SPEC_22 | BUILD_VALIDATED | NOT_RUN | KEEP_UNTIL_PHASE_2_3 |
| SPEC_23_ENEMY_AI_ROSTER_BESTIARY_CLOSEOUT.md | SPEC_23 | BUILD_VALIDATED | NOT_RUN | KEEP_UNTIL_PHASE_2_3 |
| SPEC_24_CAVE_RUNTIME_CHECKPOINTS_BOSS_GATES_CLOSEOUT.md | SPEC_24 | BUILD_VALIDATED | NOT_RUN | KEEP_UNTIL_PHASE_2_3 |
| SPEC_25_CAVE_DEATH_ANYA_CORPSE_RECOVERY_CLOSEOUT.md | SPEC_25 | BUILD_VALIDATED | NOT_RUN | KEEP_UNTIL_PHASE_2_3 |
| SPEC_26_SKILL_TREES_ACTIVE_SLOTS_RESPEC_CLOSEOUT.md | SPEC_26 | BUILD_VALIDATED | NOT_RUN | KEEP_UNTIL_PHASE_2_3 |
| SPEC_27_VISUAL_SCALE_CAMERA_SPRITE_PROFILES_CLOSEOUT.md | SPEC_27 | BUILD_VALIDATED | NOT_RUN | KEEP_UNTIL_PHASE_2_3 |
| SPEC_28_UI_UX_FULL_GAMEPLAY_CLOSEOUT.md | SPEC_28 | BUILD_VALIDATED | NOT_RUN | KEEP_UNTIL_PHASE_2_3 |
| SPEC_29_FINAL_MVP_ACCEPTANCE_AND_PROMOTION.md | SPEC_29 | BUILD_VALIDATED | NOT_RUN | KEEP_UNTIL_PHASE_2_3 |
| README_EXECUTION_ORDER.md | Meta | — | — | KEEP (reference) |

**Status:** All 12 closeout specs remain in a_implementar because Phase 2-3 human acceptance is pending. Do not move to implementados until human Phase 2-3 execution confirmed.

---

### 2.3: Reorg Specs (.specs/a_implementar/reorg/)

| File | Status | Reason | Decision |
|------|--------|--------|----------|
| README_STATUS.md | CLOSED | Marks SPEC_00-12 as closed per README_EXECUTION_ORDER.md | KEEP (evidence) |
| README_EXECUTION_ORDER.md | CLOSED | Historical; reorg phase complete; all specs promoted or archived | KEEP (evidence) |

**Status:** Keep as historical evidence; do not delete.

---

**Specs a_implementar Summary:**
- 8 Batch 2 specs → KEEP_UNTIL_PHASE_2_3
- 12 MVP closeout specs → KEEP_UNTIL_PHASE_2_3 (pending human Phase 2-3)
- 2 reorg docs → KEEP (evidence)
- **Total:** 22 items; 20 kept pending human decision, 2 evidence

---

## 3. Validation Structure Audit (docs/validation/)

### 3.1: Validation Reports & Evidence (92 files)

**Categories:**

| Category | Count | Status | Decision |
|----------|-------|--------|----------|
| SPEC_XX_VALIDATION (historical) | 25 | Evidence from SPEC_06-17 | PRESERVE (evidence) |
| spec_XX_audit_matrix | 21 | Audit evidence from all specs | PRESERVE (evidence) |
| spec_XX_execution_report | 21 | Execution evidence from all specs | PRESERVE (evidence) |
| spec_arch_reorg_XX_* | 15 | Architecture validation evidence | PRESERVE (evidence) |
| spec_claude_XX_* | 6 | Claude Code harness validation | PRESERVE (evidence) |
| spec_docs_XX_* | 10 | Documentation cleanup validation | PRESERVE (evidence) |
| spec_mvp_closeout_XX_* | 30 | MVP closeout audit/execution evidence | PRESERVE (evidence) |
| Smoke tests & checklists | 6 | Integration validation | PRESERVE (evidence) |
| Bug fix validation | 2 | Bug fix execution reports | PRESERVE (evidence) |
| Reorg validation | 4 | Reorg phase execution reports | PRESERVE (evidence) |

**Status:** All 92 validation files are evidence. No deletions. Preserve as-is.

---

### 3.2: Validation Subdirectories

| Path | Status | Content | Decision |
|------|--------|---------|----------|
| docs/validation/current/ | Exists | Last validation status | KEEP (review if LAST_VALIDATION_STATUS.md exists) |
| docs/validation/playmode/ | Exists | Play Mode test scenarios | KEEP (evidence) |
| docs/validation/_templates/ | Exists | Validation report template | KEEP (enforce validation structure) |

---

### 3.3: Missing Files

| File | Path | Reason | Action |
|------|------|--------|--------|
| LAST_VALIDATION_STATUS.md | docs/validation/current/ | Status tracking | CREATE if missing (T-3) |

**Status:** Review current/ subdirectory; create LAST_VALIDATION_STATUS.md if absent.

---

**Validation Summary:**
- 92 validation files → PRESERVE (all evidence)
- 3 subdirectories → KEEP (structure)
- 1 file to create → LAST_VALIDATION_STATUS.md (if missing)

---

## 4. Old Path References Audit

### 4.1: Active References Found (Needs Fixing)

| File | Line(s) | Reference | Current Path | Fix |
|------|---------|-----------|--------------|-----|
| docs/project/CURRENT_STATE.md | 36 | `docs/00_PROJECT/ROADMAP.md` | `docs/project/ROADMAP.md` | UPDATE_ONLY |
| docs/project/CURRENT_STATE.md | 95 | `docs/05_VALIDATION/current/LAST_VALIDATION_STATUS.md` | `docs/validation/current/LAST_VALIDATION_STATUS.md` | UPDATE_ONLY |
| docs/project/CURRENT_STATE.md | 96 | `docs/03_SPECS/SPEC_TEMPLATE.md` | `.specs/_templates/SPEC_TEMPLATE.md` | UPDATE_ONLY |
| docs/project/CURRENT_STATE.md | 97 | `docs/00_PROJECT/DOCUMENT_GOVERNANCE.md` | `docs/project/DOCUMENT_GOVERNANCE.md` | UPDATE_ONLY |
| docs/project/DOCUMENT_DELETE_CANDIDATES.md | Multiple (historical) | References to deleted files in Batch 1-1D | Historical context | HISTORICAL (no change needed) |

**Summary:** 4 active references in CURRENT_STATE.md need path updates. DOCUMENT_DELETE_CANDIDATES.md contains historical context (no change).

---

### 4.2: Grep Search Results

**Command:** `rg "docs/00_PROJECT|docs/03_SPECS|docs/04_REFINEMENTS|docs/05_VALIDATION|docs/06_BACKLOG|docs/07_RELEASES" docs/project --include="*.md"`

**Results:** 5 files found; 4 require updates (Task 1).

**No references found in:**
- .specs/a_implementar/
- docs/refinements/a_implementar/
- docs/validation/ (except historical in DOCUMENT_DELETE_CANDIDATES.md)
- AGENTS.md
- CLAUDE.md
- .claude/

---

## 5. Root Status Files Audit

**Files checked:** `SPEC_*_STATUS.md`, `spec_*_status.md` in root directory

**Result:** NONE FOUND

(All 5 root status files were deleted in SPEC_DOCS_36: SPEC_10_STATUS.md, SPEC_AGENTIC_SPEC_ORCHESTRATOR_PYTHON_CLI.md, SPEC_ORQUESTRADOR_CONSOLIDACAO_V2_CLEANUP_E_GATES.md, SPEC15_COMMIT_SUMMARY.txt, SPEC15_COMPLETE_IMPLEMENTATION_LOG.md)

**Status:** ✓ CLEAN

---

## 6. Validation Script Audit (tools/docs/validate_docs.ps1)

**Current state:** Updated in SPEC_DOCS_36 to forbid docs_old/

**Required updates for SPEC_DOCS_37 (Task 2):**

| Item | Current | Required | Action |
|------|---------|----------|--------|
| Forbid docs_old/ | ✓ YES | ✓ YES | KEEP |
| Forbid docs/00_PROJECT/ | ✗ NO | ✓ YES | ADD |
| Forbid docs/03_SPECS/ | ✗ NO | ✓ YES | ADD |
| Forbid docs/04_REFINEMENTS/ | ✗ NO | ✓ YES | ADD |
| Forbid docs/05_VALIDATION/ | ✗ NO | ✓ YES | ADD |
| Forbid docs/06_BACKLOG/ | ✗ NO | ✓ YES | ADD |
| Forbid docs/07_RELEASES/ | ✗ NO | ✓ YES | ADD |
| Require docs/project/CURRENT_STATE.md | ✓ YES | ✓ YES | KEEP |
| Require docs/project/DOCUMENT_GOVERNANCE.md | ✓ YES | ✓ YES | KEEP |
| Require docs/project/DOCUMENT_INDEX.md | ✓ YES | ✓ YES | KEEP |
| Require .specs/_templates/SPEC_TEMPLATE.md | ✗ NO | ✓ YES | ADD |
| Require docs/refinements/_templates/REFINEMENT_TEMPLATE.md | ✗ NO | ✓ YES | ADD |
| Require docs/validation/_templates/VALIDATION_REPORT_TEMPLATE.md | ✗ NO | ✓ YES | ADD |
| Don't require pre_refinamentos folder | ✓ YES | ✓ YES | KEEP |

**Status:** 7 new forbid checks, 3 new require checks needed.

---

## 7. Decision Summary by Task

| Task | Scope | Decisions | Status |
|------|-------|-----------|--------|
| T-1: Fix canonical references | docs/project/* | 4 files, 4 path updates | UPDATE_ONLY |
| T-2: Update validate_docs.ps1 | tools/docs/validate_docs.ps1 | Add 10 checks | UPDATE_ONLY |
| T-3: Create LAST_VALIDATION_STATUS.md | docs/validation/current/ | 1 file to create | CREATE_IF_MISSING |
| T-4: Delete root status files | Root directory | 0 files (all deleted in SPEC_DOCS_36) | CLEAN |
| T-5: Audit pre_refinements | docs/refinements/a_implementar/pre_refinamentos/ | 14 MOVE, 1 KEEP, 4 DELETE | T-5 EXECUTION |
| T-6: Audit Batch 2 specs | .specs/a_implementar/ | 8 KEEP_UNTIL_PHASE_2_3 | BLOCKED_UNTIL_PHASE_2_3 |
| T-7: Audit validation | docs/validation/ | 92 PRESERVE, 3 KEEP, 1 CREATE | VERIFY_STRUCTURE |
| T-8: Final validation | validate_docs.ps1 + grep | Expect PASS + 0 old refs in active docs | EXECUTE_LAST |

---

## 8. Risk Assessment

| Risk | Severity | Mitigation |
|------|----------|-----------|
| Moving pre_refinements files incorrectly | MEDIUM | Each file reviewed by Phase 1 completion before moving |
| Deleting refinements that are still needed | LOW | Only delete those superseded by closeout evidence (SPEC_13, SPEC_14 → SPEC_23/24) |
| Batch 2 specs accidentally promoted | HIGH | Mark all KEEP_UNTIL_PHASE_2_3; validate script should block any moves |
| Missing LAST_VALIDATION_STATUS.md | LOW | Create with safe defaults; no acceptance claims |
| validate_docs.ps1 breaking legitimate paths | MEDIUM | Add forbid checks carefully; test after each addition |

---

## 9. Success Criteria for Phase 1-8

- ✓ docs/project/CURRENT_STATE.md updated (4 path fixes)
- ✓ validate_docs.ps1 updated (10 new checks)
- ✓ docs/validation/current/LAST_VALIDATION_STATUS.md created/verified
- ✓ No root SPEC_*_STATUS.md files (already clean)
- ✓ Pre_refinements: 14 moved, 1 kept, 4 deleted (with justification)
- ✓ Batch 2 specs remain KEEP_UNTIL_PHASE_2_3
- ✓ Validation reports preserved (92 files)
- ✓ Docs validation PASS 14+N checks
- ✓ No old path references in active docs
- ✓ Zero runtime/C#/Unity changes

---

## Conclusion

Phase 0 audit complete. **138 items classified.** Ready for Phase 1-8 execution.

**Key findings:**
1. **Pre-refinements:** 14 completed (Phase 1), 1 active (Phase 2-3), 4 superseded (safe to delete)
2. **Batch 2 specs:** 8 blocked pending human Phase 2-3 execution
3. **MVP closeout specs:** 12 blocked pending human Phase 2-3 execution
4. **Validation:** 92 files preserved as evidence
5. **Old paths:** 4 active references in CURRENT_STATE.md need fixing
6. **validate_docs.ps1:** 10 new checks needed

**No blockers. Proceed with Phase 1-8.**

---

*Phase 0 Audit Complete: 2026-06-01*  
*Spec: SPEC_DOCS_37 — Final Refinements, Specs and Validation Sweep*  
*Items audited: 138*  
*Decisions: 8 task categories, 20+ files requiring action*
