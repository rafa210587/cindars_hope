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

## Candidates List

| Path | Type | Reason | Substitute | Risk | Status |
|------|------|--------|------------|------|--------|
| docs/specs/a_implementar/reorg/SPEC_00.md | spec (superseded) | CLOSED per README_STATUS | docs/validation/spec_arch_reorg_00_* | LOW | candidate |
| docs/specs/a_implementar/reorg/SPEC_01.md | spec (superseded) | CLOSED | docs/validation/spec_arch_reorg_01_* | LOW | candidate |
| docs/specs/a_implementar/reorg/SPEC_02.md | spec (superseded) | CLOSED | docs/validation/spec_arch_reorg_02_* | LOW | candidate |
| docs/specs/a_implementar/reorg/SPEC_03.md | spec (superseded) | CLOSED | docs/validation/spec_arch_reorg_03_* | LOW | candidate |
| docs/specs/a_implementar/reorg/SPEC_04.md | spec (superseded) | CLOSED | docs/validation/spec_arch_reorg_04_* | LOW | candidate |
| docs/specs/a_implementar/reorg/SPEC_05.md | spec (superseded) | CLOSED | docs/validation/spec_arch_reorg_05_* | LOW | candidate |
| docs/specs/a_implementar/reorg/SPEC_06.md | spec (superseded) | CLOSED | docs/validation/spec_arch_reorg_06_* | LOW | candidate |
| docs/specs/a_implementar/reorg/SPEC_07.md | spec (superseded) | CLOSED | docs/validation/spec_arch_reorg_07_* | LOW | candidate |
| docs/specs/a_implementar/reorg/SPEC_08.md | spec (superseded) | CLOSED | docs/validation/spec_arch_reorg_08_* | LOW | candidate |
| docs/specs/a_implementar/reorg/SPEC_09.md | spec (superseded) | CLOSED | docs/validation/spec_arch_reorg_09_* | LOW | candidate |
| docs/specs/a_implementar/reorg/SPEC_10.md | spec (superseded) | CLOSED | docs/validation/spec_arch_reorg_10_* | LOW | candidate |
| docs/specs/a_implementar/reorg/SPEC_11.md | spec (superseded) | CLOSED | docs/validation/spec_arch_reorg_11_* | LOW | candidate |
| docs/specs/a_implementar/reorg/SPEC_12.md | spec (superseded) | CLOSED | docs/validation/spec_arch_reorg_12_* | LOW | candidate |
| docs/specs/a_implementar/reorg/SPEC_00_STRATEGY_SUBAGENTS.md | spec (superseded) | CLOSED | README_STATUS.md | LOW | candidate |
| docs/specs/a_implementar/spec_14a_cave_enemy_spawnplan_materialization_run_stability.md | spec (superseded) | Covered by SPEC_24 | SPEC_24 report | LOW | candidate — after Phase 2-3 |
| docs/specs/a_implementar/spec_14a_fix2_spawn_density_combat_feedback_damage_numbers.md | spec (superseded) | Covered by SPEC_24 | SPEC_24 report | LOW | candidate — after Phase 2-3 |
| docs/specs/a_implementar/spec_14b_cave_snapshot_replay_enemy_plan.md | spec (superseded) | Covered by SPEC_24 | SPEC_24 report | LOW | candidate — after Phase 2-3 |
| docs/specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md | spec (superseded) | Covered by SPEC_23; copy in implementados | SPEC_23 report | LOW | candidate — after Phase 2-3 |
| docs/specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md | spec (superseded) | Covered by SPEC_24; copy in implementados | SPEC_24 report | LOW | candidate — after Phase 2-3 |
| docs/specs/a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md | spec (superseded) | Covered by SPEC_28 | SPEC_28 report | LOW | candidate — after Phase 2-3 |
| docs/roadmap/NEXT_WAVES_ROADMAP_v1.0.md | roadmap (old) | Superseded by v1.1 | v1.1 + new ROADMAP.md | LOW | candidate |
| docs/architecture/ARCH_fase4_v2.2.md | architecture (old) | Superseded by v2.3 | v2.3 | LOW | candidate |
| docs/design/GDD_v2.6.md | product (old) | Superseded by v2.7 | GDD_v2.7 | LOW | candidate |
| docs/design/CHANGELOG_ATUALIZACAO_v2.6.md | historical | Not needed standalone | GDD_v2.7 | LOW | candidate |
| docs/implementation_runs/RUN_20260523_0000_wave00_planning.md | historical_log | Session log superseded by PROJECT_LOG | PROJECT_LOG.md | LOW | candidate |
| docs/implementation_runs/RUN_20260523_0100_wave01_data_save_progression.md | historical_log | Session log | PROJECT_LOG.md | LOW | candidate |
| docs/implementation_runs/RUN_FINAL_OVERNIGHT_20260523_0300.md | historical_log | Session log | PROJECT_LOG.md | LOW | candidate |
| docs/implementation_runs/RUN_POST_MERGE_AUDIT_HOTFIXES_20260523.md | historical_log | Session log | PROJECT_LOG.md | LOW | candidate |
| docs/implementation_runs/RUN_STABILIZATION_OVERNIGHT_20260523.md | historical_log | Session log | PROJECT_LOG.md | LOW | candidate |
| docs/IMPLEMENTATION_DELIVERY_20260523.md | historical_log | Snapshot superseded | PROJECT_LOG.md | LOW | candidate |

---

## NOT Delete Candidates

The following must be preserved regardless:

```
AGENTS.md
CLAUDE.md
docs/00_PROJECT/*  (new governance)
docs/validation/*  (evidence)
docs/specs/implementados/*  (implemented history)
docs/refinements/implementados/*  (refinement history)
docs/release/MVP_ACCEPTANCE_REPORT.md
docs/backlog/post_mvp_backlog.md
docs/architecture/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.1_FASE9C_DELTA.md
docs/amendments/*  (critical amendments including FASE9F cave stable run)
docs/operations/AGENT_EXECUTION_PROTOCOL.md
docs/operations/READING_MATRIX.md
PROJECT_LOG.md
docs/IMPLEMENTATION_STATUS.md
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/a_implementar/closeout_mvp/*  (pending Phase 2-3)
```

---

## Review Process

Before deleting any candidate:

1. Human reviews this list
2. Confirms substitute exists
3. Runs docs validation after deletion
4. Commits with clear message explaining deletion

*Created: 2026-06-01 (SPEC_DOCS_30)*
