# Document Delete Candidates

> These documents are **marked for future deletion only**.  
> **Nothing is deleted in SPEC_DOCS_30.**  
> Deletion happens in SPEC_DOCS_31 after human review.

---

## Deletion Policy

A document may be deleted ONLY if:
1. Has a canonical substitute
2. Is NOT validation evidence
3. Does NOT contain a unique decision not captured elsewhere
4. Is NOT referenced by AGENTS.md or CURRENT_STATE.md
5. Is NOT an active spec
6. Has passed human review

---

## Batch 1 — Deleted in SPEC_DOCS_31 ✓

**Criteria:** Superseded, no active references, archived specification.

**Status:** DELETED on 2026-06-01

| Path | Status | Notes |
|------|--------|-------|
| docs/specs/a_implementar/reorg/SPEC_00 through SPEC_12 (13 files) | ✓ DELETED | CLOSED per README_STATUS.md; validation evidence preserved |
| docs/specs/a_implementar/reorg/SPEC_00_STRATEGY_SUBAGENTS.md | ✓ DELETED | CLOSED |
| docs/roadmap/NEXT_WAVES_ROADMAP_v1.0.md | ✓ DELETED | Superseded by v1.1 + current ROADMAP.md |
| docs/architecture/ARCH_fase4_v2.2.md | ✓ DELETED | Superseded by v2.3 FASE9C_DELTA |
| docs/design/GDD_v2.6.md | ✓ DELETED | Superseded by v2.7 FASE9C_DELTA |
| docs/design/CHANGELOG_ATUALIZACAO_v2.6.md | ✓ DELETED | Old changelog for deprecated GDD |
| docs/implementation_runs/RUN_20260523_0000_wave00_planning.md | ✓ DELETED | Consolidado em PROJECT_LOG.md |
| docs/implementation_runs/RUN_20260523_0100_wave01_data_save_progression.md | ✓ DELETED | Consolidado em PROJECT_LOG.md |
| docs/implementation_runs/RUN_FINAL_OVERNIGHT_20260523_0300.md | ✓ DELETED | Consolidado em PROJECT_LOG.md |
| docs/implementation_runs/RUN_POST_MERGE_AUDIT_HOTFIXES_20260523.md | ✓ DELETED | Consolidado em PROJECT_LOG.md |
| docs/implementation_runs/RUN_STABILIZATION_OVERNIGHT_20260523.md | ✓ DELETED | Consolidado em PROJECT_LOG.md |

**Total:** 20 files successfully deleted in SPEC_DOCS_31 (2026-06-01).

---

## Batch 1B — Deleted in SPEC_DOCS_32 + SPEC_DOCS_33 ✓

**Criteria:** Legacy documentation, contradictory to governance, corrupted encoding, or obsolete artifact system.

**Status:** DELETED on 2026-06-01

### Operations & Legacy Docs (SPEC_DOCS_32 Phase 0)

| Path | Status | Notes |
|------|--------|-------|
| docs/operations/LLM_HANDOFF_INSTRUCTIONS.md | ✓ DELETED | Mojibake corruption; contradicts CLAUDE.md governance |
| docs/operations/READING_MATRIX.md | ✓ DELETED | Mojibake corruption; references obsolete spec registries |
| docs/operations/HANDOFF_MERGE_STABILIZATION_TO_DEV_20260523.md | ✓ DELETED | Historical merge handoff; consolidated in PROJECT_LOG.md |
| docs/roadmap/NEXT_WAVES_ROADMAP_v1.1_FASE9C_DELTA.md | ✓ DELETED | Orphaned delta (base v1.0 already deleted) |
| docs/backlog/FASE6_FARM_backlog_v1.2.md | ✓ DELETED | FASE6 historical; items in current_backlog |
| docs/backlog/FASE6_INDEX_global_v1.2.md | ✓ DELETED | FASE6 index; superseded by current backlog |
| docs/backlog/FASE6_INDEX_global_v1.3_FASE9C_DELTA.md | ✓ DELETED | FASE6 delta; historical |

### Agent Prompts & Packages (SPEC_DOCS_33)

| Path | Status | Notes |
|------|--------|-------|
| docs/agent_prompts/ (26 files total) | ✓ DELETED | Historical Codex prompts for implemented specs (SPEC_01-17F); not used by current harness |
| docs/agent_prompts/a_executar/ | ✓ DELETED | Includes SPEC_17B prompt + validation template |
| docs/agent_prompts/implementados/ | ✓ DELETED | 22 historical spec prompts |
| docs/agent_prompts/bloqueados/ | ✓ DELETED | Empty except .gitkeep |
| docs/agent_prompts/executados/ | ✓ DELETED | Empty except .gitkeep |
| docs/agent_packages/ (6 files total) | ✓ DELETED | Legacy orchestration packages (P00, P12A, P12B); Codex artifact system |
| docs/agent_packages/README.md | ✓ DELETED | Describes obsolete package orchestration system |
| docs/agent_packages/PACKAGE_*.md (4 files) | ✓ DELETED | Legacy package definitions + templates |

**Total:** 39 files successfully deleted in SPEC_DOCS_32 + SPEC_DOCS_33 (2026-06-01).

---

## Batch 1C — Deleted in SPEC_DOCS_32 Phase 1 Investigations ✓

**Criteria:** Legacy Codex orchestration, corrupted/contradictory governance, historical design deltas.

**Status:** DELETED on 2026-06-01

| Path | Status | Notes |
|------|--------|-------|
| docs/operations/CODEX_SPEC_EXECUTION_HARNESS.md | ✓ DELETED | Old Codex harness; references deleted agent_prompts/ |
| docs/operations/CODEX_ORCHESTRATION_PROMPT.md | ✓ DELETED | Old Codex prompt; references deleted agent_prompts/ |
| docs/operations/SPECKIT_DRIFT_CONTROL_v1.0.md | ✓ DELETED | Mojibake corruption; rules already in .claude/rules/ |
| docs/operations/AGENT_EXECUTION_PROTOCOL.md | ✓ DELETED | Mojibake corruption; contradicts context-reading-policy.md |
| docs/design/GDD_v2.7_FASE9C_DELTA.md | ✓ DELETED | Historical delta (base v2.6 deleted in SPEC_DOCS_31); not referenced by specs |
| docs/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md | ✓ DELETED | Historical roadmap; authoritative source is amendment + rule in .claude/rules/ |

**Total:** 6 files successfully deleted in SPEC_DOCS_32 Phase 1 (2026-06-01).

---

## Batch 1D — Deleted in SPEC_DOCS_34 Phase 1-2 Final Cleanup ✓

**Criteria:** Final legacy file reconciliation: orphaned architecture delta, legacy Python orchestrator tool.

**Status:** DELETED on 2026-06-01

| Path | Status | Notes |
|------|--------|-------|
| docs/architecture/ARCH_fase4_v2.3_FASE9C_DELTA.md | ✓ DELETED | Orphaned delta (base v2.2 deleted in SPEC_DOCS_31); authority in CORE_CONTRACTS |
| orquestrador/ (64 files + directory) | ✓ DELETED | Legacy Python orchestrator for Codex; replaced by .claude/ harness; 0 active references |

**Total:** 65 files successfully deleted in SPEC_DOCS_34 Phase 1-2 (2026-06-01).

---

## Batch 2 — Temporary Addendum Files (Incorporados em 2026-06-05)

**Criteria:** Temporary addendum files created for migration that are now incorporated into canonical sources.

**Status:** DELETED on 2026-06-05

| Path | Status | Notes |
|------|--------|-------|
| docs/design/SPEC_SOURCE_MAP_WORLD_TIME_ADDENDUM.md | ✓ DELETED | Temporary migration addendum; content incorporated into SPEC_SOURCE_MAP.md (commit: World/Time centralization) |
| docs/design/SPEC_SOURCE_MAP_UI_MENU_FLOWS_ADDENDUM.md | ✓ DELETED | Temporary migration addendum; content incorporated into SPEC_SOURCE_MAP.md (commit: UI Menu Flows centralization) |
| docs/design/SPEC_SOURCE_MAP_BESTIARY_ADDENDUM.md | ✓ DELETED | Temporary migration addendum; content incorporated into SPEC_SOURCE_MAP.md (commit: Bestiary/Knowledge Discovery centralization) |

**Total:** 3 files successfully deleted in 2026-06-05.

---

## Batch 3 — Defer Until After Phase 2-3 Closeout

**Criteria:** Content covered by SPEC_18-28 closeout, but source SPECS not yet promoted to implementados/.

**Risk Level:** MEDIUM — Deletion would be safe after SPEC_18-28 closeout, but premature now could create confusion.

**Action:** Delete only after SPEC_23, SPEC_24, SPEC_28 are promoted (Phase 2-3 complete).

| Path | Type | Reason | Covered By | Status |
|------|------|--------|------------|--------|
| docs/specs/a_implementar/spec_14a_cave_enemy_spawnplan_materialization_run_stability.md | spec (superseded) | Covered by SPEC_24 closeout | SPEC_24 | blocked |
| docs/specs/a_implementar/spec_14a_fix2_spawn_density_combat_feedback_damage_numbers.md | spec (superseded) | Covered by SPEC_24 closeout | SPEC_24 | blocked |
| docs/specs/a_implementar/spec_14b_cave_snapshot_replay_enemy_plan.md | spec (superseded) | Covered by SPEC_24 closeout | SPEC_24 | blocked |
| docs/specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md | spec (superseded) | Covered by SPEC_23 closeout; duplicate exists in implementados | SPEC_23 | blocked |
| docs/specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md | spec (superseded) | Covered by SPEC_24 closeout; duplicate exists in implementados | SPEC_24 | blocked |
| docs/specs/a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md | spec (superseded) | Covered by SPEC_28 closeout | SPEC_28 | blocked |

**Total:** 6 files. Safe after Phase 2-3 completion.

---

## Batch 3 — DO NOT DELETE (Protected Categories)

**Criteria:** Governance, evidence, operational necessity, or active specs.

**Risk Level:** NONE — Deleting any of these would break governance, lose evidence, or disrupt operations.

**Protected Categories:**

```
AGENTS.md                                            (governance)
CLAUDE.md                                            (governance)
docs/00_PROJECT/*                                    (governance hub)
docs/validation/*                                    (evidence)
docs/specs/implementados/*                           (implemented history, searchable)
docs/refinements/implementados/*                     (refinement history)
docs/release/MVP_ACCEPTANCE_REPORT.md                (release acceptance evidence)
docs/backlog/post_mvp_backlog.md                     (active backlog)
docs/architecture/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.1_FASE9C_DELTA.md  (active architecture)
docs/amendments/*                                    (governance: critical amendments including FASE9F cave stable run)
docs/operations/AGENT_EXECUTION_PROTOCOL.md         (governance: operational protocol)
docs/operations/READING_MATRIX.md                    (governance: reading policy)
PROJECT_LOG.md                                       (historical record)
docs/IMPLEMENTATION_STATUS.md                        (active status tracking)
docs/specs/SPEC_EXECUTION_ORDER.md                   (execution dependency matrix)
docs/specs/a_implementar/closeout_mvp/*              (active pending Phase 2-3)
docs/05_VALIDATION/playmode/*                        (Phase 3 human test scenarios)
docs/05_VALIDATION/current/LAST_VALIDATION_STATUS.md (current validation status)
```

Total: 19+ protected categories. None are deletion candidates.

---

## Review Process

Before deleting any candidate:

1. Human reviews this list
2. Confirms substitute exists
3. Runs docs validation after deletion
4. Commits with clear message explaining deletion

*Created: 2026-06-01 (SPEC_DOCS_30)*
