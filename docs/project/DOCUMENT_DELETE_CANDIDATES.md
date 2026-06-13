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
| .specs/a_implementar/reorg/SPEC_00 through SPEC_12 (13 files) | ✓ DELETED | CLOSED per README_STATUS.md; validation evidence preserved |
| .specs/a_implementar/reorg/SPEC_00_STRATEGY_SUBAGENTS.md | ✓ DELETED | CLOSED |
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
| docs/design/SPEC_SOURCE_MAP_QUEST_OBJECTIVE_EVENT_ADDENDUM.md | ✓ DELETED | Temporary migration addendum; content incorporated into SPEC_SOURCE_MAP.md (commit: Quest/Objective/Event System centralization) |

**Total:** 4 files successfully deleted in 2026-06-05.

---

## Batch 3 — Defer Until After Phase 2-3 Closeout

**Criteria:** Content covered by SPEC_18-28 closeout, but source SPECS not yet promoted to implementados/.

**Risk Level:** MEDIUM — Deletion would be safe after SPEC_18-28 closeout, but premature now could create confusion.

**Action:** Delete only after SPEC_23, SPEC_24, SPEC_28 are promoted (Phase 2-3 complete).

| Path | Type | Reason | Covered By | Status |
|------|------|--------|------------|--------|
| .specs/a_implementar/spec_14a_cave_enemy_spawnplan_materialization_run_stability.md | spec (superseded) | Covered by SPEC_24 closeout | SPEC_24 | blocked |
| .specs/a_implementar/spec_14a_fix2_spawn_density_combat_feedback_damage_numbers.md | spec (superseded) | Covered by SPEC_24 closeout | SPEC_24 | blocked |
| .specs/a_implementar/spec_14b_cave_snapshot_replay_enemy_plan.md | spec (superseded) | Covered by SPEC_24 closeout | SPEC_24 | blocked |
| .specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md | spec (superseded) | Covered by SPEC_23 closeout; duplicate exists in implementados | SPEC_23 | blocked |
| .specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md | spec (superseded) | Covered by SPEC_24 closeout; duplicate exists in implementados | SPEC_24 | blocked |
| .specs/a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md | spec (superseded) | Covered by SPEC_28 closeout | SPEC_28 | blocked |

**Total:** 6 files. Safe after Phase 2-3 completion.

---

## Batch 3 — DO NOT DELETE (Protected Categories)

**Criteria:** Governance, evidence, operational necessity, or active specs.

**Risk Level:** NONE — Deleting any of these would break governance, lose evidence, or disrupt operations.

**Protected Categories:**

```
AGENTS.md                                            (governance)
CLAUDE.md                                            (governance)
docs/project/*                                       (governance hub)
docs/validation/*                                    (evidence and validation checklists)
.specs/implementados/*                           (implemented history, searchable)
docs/refinements/implementados/*                     (refinement history)
docs/release/MVP_ACCEPTANCE_REPORT.md                (release acceptance evidence)
docs/backlog/post_mvp_backlog.md                     (active backlog)
docs/architecture/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.1_FASE9C_DELTA.md  (active architecture)
docs/amendments/*                                    (governance: critical amendments including FASE9F cave stable run)
.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md          (canonical: execution governance and phases)
.specs/SPEC_VALIDATION_MATRIX_MASTER.md         (canonical: validation requirements matrix)
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md           (canonical: SpecKit format template)
docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md   (canonical: wave-end human validation checklist)
PROJECT_LOG.md                                       (historical record)
docs/IMPLEMENTATION_STATUS.md                        (active status tracking)
.specs/SPEC_EXECUTION_ORDER.md                   (execution dependency matrix)
.specs/a_implementar/closeout_mvp/*              (active pending Phase 2-3)
```

Total: 19+ protected categories (updated with canonical specs docs). None are deletion candidates.

---

## Batch 4 — Redirect specs replaced by canonical governance docs ✓

**Criteria:** Intermediate redirect files created during governance cleanup; canonical documents now exist directly in `.specs/`.

**Status:** APPROVED FOR DELETION by human request on 2026-06-07.

| Path | Type | Reason | Canonical Substitute | Status |
|------|------|--------|----------------------|--------|
| .specs/a_implementar/00_spec_wave_execution_protocol.md | redirect spec | Intermediate redirect; not an executable spec | .specs/SPEC_WAVE_EXECUTION_PROTOCOL.md | ✓ DELETED |
| .specs/a_implementar/00_spec_validation_matrix_master.md | redirect spec | Intermediate redirect; not an executable spec | .specs/SPEC_VALIDATION_MATRIX_MASTER.md | ✓ DELETED |

**Total:** 2 files successfully deleted on 2026-06-07.

---

## Review Process

Before deleting any candidate:

1. Human reviews this list
2. Confirms substitute exists
3. Runs docs validation after deletion
4. Commits with clear message explaining deletion

*Created: 2026-06-01 (SPEC_DOCS_30)*

---

## Batch FABLE — Candidatos adicionados 2026-06-12 (triagem fable_00B)

**Status:** AGUARDANDO CONFIRMACAO HUMANA ("delete this candidate" por item)

| Path | Motivo | Substituto canonico |
|------|--------|---------------------|
| docs/validation/playmode | ARQUIVO solto sem extensao (template salvo errado); bloqueia a pasta convencional de cenarios | copia preservada em docs/validation/_templates/HUMAN_TEST_SCENARIO_TEMPLATE_FROM_STRAY_PLAYMODE_FILE.md |
| .specs/a_implementar/executadas_build_validated/FIX_001_runtime_warnings_town_shop_catalog_alignment.md | FIX-001/001B ja executados e reportados (CURRENT_STATE 2026-06-10); spec stale com naming fora do padrao | reports FIX_001B em docs/validation/ |
| .specs/a_implementar/executadas_build_validated/03_spec_quest_*.md (8 arquivos) | duplicata literal da serie 09_spec_quest_* (WAVE 09 foi a executada) | serie 09_spec_quest_* + reports WAVE 09 |

---

## Batch FABLE-2 — Varredura de redundância 2026-06-12 (AUTORIZADO E EXECUTADO)

**Autorização humana:** mensagem de 2026-06-12 "remova tudo o que já não faz mais sentido pro projeto".

| Path | Motivo | Substituto canônico | Status |
|------|--------|---------------------|--------|
| build_logs.zip (raiz) | artefato de build de 8MB rastreado no git | logs regeneráveis | DELETED |
| CHECKLIST_PR001.md (raiz) | checklist da era FASE8 pré-spec-governance, mojibake | fluxo /implement-spec | DELETED |
| validate_quick.py (raiz) | validador python obsoleto (checa estrutura antiga); viola windows_powershell_only | tools/docs/validate_docs.ps1 (25+ checks) | DELETED |
| BACKLOG.md (raiz) | backlog da era SPEC 14; zero referências vivas | docs/backlog/post_mvp_backlog.md + current_backlog.md | DELETED |
| docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.0.md | superada pela v1.3 (SPEC_SOURCE_MAP só manda v1.3); zero referências | FARM_DESIGN_DIRECTION_v1.3.md | DELETED |
| compile_check_waves_08_12.log (raiz, untracked) | log solto de validação antiga | docs/validation/ | DELETED |

### Batch FABLE (anterior) — agora autorizado e executado
| docs/validation/playmode (ARQUIVO solto) | DELETED — pasta playmode/ correta criada; cenário movido para dentro |
| executadas_build_validated/FIX_001_runtime_warnings_town_shop_catalog_alignment.md | DELETED — FIX-001/001B executados |
| executadas_build_validated/03_spec_quest_*.md (8 arquivos) | DELETED — duplicatas literais da série 09 (WAVE 09 foi a executada) |
