# Registry de specs a implementar

Fonte unica de specs futuras: `docs/specs/a_implementar/`. A pasta raiz `specs/` foi removida e nao deve ser recriada.

A antiga spec 00 de reconciliacao documental foi reclassificada como implementada/parcial em `docs/specs/implementados/spec_docs_001_single_source_specs_refinements_reconciliation_parcial.md`. Ela nao deve ser executada novamente.

Documentos canônicos de governança que não são specs executáveis:

```text
docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
```

| Ordem | Spec | Status | Dependencia | Observacao |
|---|---|---|---|---|
| 00.04 | [00_spec_existing_implementation_audit.md](a_implementar/00_spec_existing_implementation_audit.md) | A implementar - auditoria/governanca | 00 docs canonicos | Audita o estado real implementado/parcial/faltante do repo antes de gerar specs runtime para evitar duplicacao e definir residual/hardening/future. |
| 01.01 | [01_spec_stable_ids_registry_runtime.md](a_implementar/01_spec_stable_ids_registry_runtime.md) | A implementar - hardening/residual | 00.04 audit + spec_data_001 implementada | Audita e endurece IDs/registries existentes sem recriar arquitetura; bloqueia specs que persistem/referenciam IDs. |
| 01.02 | [01_spec_game_event_contracts_runtime.md](a_implementar/01_spec_game_event_contracts_runtime.md) | A implementar - hardening/residual | 01.01 + spec_core_001 implementada | Audita e endurece GameEventBus/event contracts existentes sem recriar barramento; define catálogo, payload rules e unsubscribe lifecycle. |
| 01.03 | [01_spec_save_restore_order_contract_runtime.md](a_implementar/01_spec_save_restore_order_contract_runtime.md) | A implementar - hardening/residual | 01.01 + 01.02 + save specs implementadas | Audita e endurece restore order/save sections existentes sem reescrever SaveManager; produz matriz de sections, defaults, dependencies e post-restore notifications. |
| 01.04 | [01_spec_save_section_ownership_registry.md](a_implementar/01_spec_save_section_ownership_registry.md) | A implementar - hardening/residual | 01.03 + save specs implementadas | Cria ou consolida ownership registry de save sections, owners, defaults, dependencies e migration responsibility sem migrar tudo para providers. |
| 01.05 | [01_spec_save_provider_architecture_runtime.md](a_implementar/01_spec_save_provider_architecture_runtime.md) | A implementar - gradual/residual | 01.04 + save ownership registry | Consolida arquitetura incremental de save providers sem substituir SaveManager, sem schema change e sem migração massiva de sections. |
| 01Q | [spec_test_harness_editmode_playmode_quality_gate.md](a_implementar/spec_test_harness_editmode_playmode_quality_gate.md) | A implementar - quality gate fundacional | 01 validacao Unity + rules/skills atuais | Deve ser executada antes das novas waves runtime para exigir EditMode tests, PlayMode/manual scenarios, regression tests e evidencia de risco residual. |

---

## Specs Geradas Pós-Refinamento — Registry Reconciliado

Após geração da batch 36 (154 specs) e limpeza de legacy (7 specs absorvidas), o registry foi reconciliado.
- **Specs antes da limpeza:** 154 em `docs/specs/a_implementar/` (146 wave-based + 7 legacy + 1 README)
- **Specs legacy absorvidas:** 7 movidas para `docs/specs/absorvidas/legacy_pre_wave_reconciliation/` (2026-06-07)
- **Fila ativa após limpeza:** 147 specs wave-based em `docs/specs/a_implementar/`
- **Review required:** 0
- **Nenhuma spec movida para `implementados/`**

### WAVE 00 — Governança & Auditoria

| Ordem | Spec | Status | Wave | Observacao |
|---|---|---|---|---|
| 00.04 | `00_spec_existing_implementation_audit.md` | **BUILD_VALIDATED** | 00 | ✓ Auditoria completa; commit ee1c0fb+ |

### WAVE 01 — Hardening & Quality Gate

| Ordem | Spec | Status | Wave | Observacao |
|---|---|---|---|---|
| 01.01 | `01_spec_stable_ids_registry_runtime.md` | **BUILD_VALIDATED** | 01 | ✓ IDs estabilizados; commit WAVE 01 |
| 01.02 | `01_spec_game_event_contracts_runtime.md` | **BUILD_VALIDATED** | 01 | ✓ Event contracts; commit WAVE 01 |
| 01.03 | `01_spec_save_restore_order_contract_runtime.md` | **BUILD_VALIDATED** | 01 | ✓ Restore order; commit WAVE 01 |
| 01.04 | `01_spec_save_section_ownership_registry.md` | **BUILD_VALIDATED** | 01 | ✓ Ownership registry; commit WAVE 01 |
| 01.05 | `01_spec_save_provider_architecture_runtime.md` | **BUILD_VALIDATED** | 01 | ✓ Providers; commit WAVE 01 |
| 01.06 | `01_spec_invalid_id_fallback_rules.md` | **BUILD_VALIDATED** | 01 | ✓ ID fallback; commit WAVE 01 |
| 01.07 | `01_spec_playmode_validation_baseline.md` | **BUILD_VALIDATED** | 01 | ✓ Play Mode baseline; commit WAVE 01 |
| 01Q | `spec_test_harness_editmode_playmode_quality_gate.md` | **BUILD_VALIDATED** | 01 | ✓ Quality gate fundacional; commit WAVE 01 |

**Total WAVE 01:** 8 specs BUILD_VALIDATED — WAVE 02+ desbloqueado

### WAVE 02-10 — Core Runtime

| Wave | Specs | Contagem | Status | Observacao |
|---|---|---|---|---|
| 02 | Time/Calendar/Weather/Lunar | 8 | **BUILD_VALIDATED** | ✓ 7/8 BUILD_VALIDATED; 1 UI deferred; closeout WAVE_02 |
| 03 | Quest/Objective/Event | 10 | **BUILD_VALIDATED** | ✓ 8/8 runtime BUILD_VALIDATED; 4 future blocked; closeout WAVE_03 |
| 04 | UI Foundation | 21 | **BUILD_VALIDATED** | ✓ 14/14 reports; 3 BUILD_VALIDATED, 11 CONTRACT_ONLY; closeout WAVE_04 |
| 05 | Farm/Inventory/Companion | 23 | **BUILD_VALIDATED** | ✓ 20/20 BUILD_VALIDATED; ~123 EditMode tests; closeout WAVE_05 |
| 06 | Economy/Loot/Crafting | 8 | **BUILD_VALIDATED** | ✓ 8/8 BUILD_VALIDATED; ~75 EditMode tests; closeout WAVE_06 |
| 07 | (merged to other waves) | 0 | — | DOES_NOT_EXIST — no 07_spec_*.md files found |
| 08 | City/NPC/Dialogue | 4 | **BUILD_VALIDATED** | ✓ 4/4 BUILD_VALIDATED; ~61 EditMode tests; closeout WAVE_08 |
| 09 | Quest/Player/Skills (duplicate 03) | 7 | **BUILD_VALIDATED** | ✓ 8/8 BUILD_VALIDATED; ~107 EditMode tests; closeout WAVE_09 |
| 10 | Endgame/Memory/Fonte | 4 | **BUILD_VALIDATED** | ✓ 4/4 BUILD_VALIDATED; ~72 EditMode tests; closeout WAVE_10 |
| 11 | UI Projections/HUD | 4 | **BUILD_VALIDATED** | ✓ 4/4 BUILD_VALIDATED; ~96 EditMode tests; closeout WAVE_11 |
| 12 | Docs/Consolidation | 2 | IN_PROGRESS | Spec 1 in execution (2026-06-08) |
| 13 | Bestiary | 2 | A implementar | Bestiary state/UI future hooks |

**Total WAVE 02-12:** 93 specs core/runtime

### WAVE 17-24 — Future Mapped

| Wave | Specs | Contagem | Status | Observacao |
|---|---|---|---|---|
| Future Specs | Mixed (_future suffix) | ~32 | Future mapped | Future/expansion specs marked explicitly |
| 23 | Pets | 4 | **HOLD / BLOCKED_SCOPE** | **NÃO EXECUTAR** — pets bloqueados até explícita autorização |

**Total WAVE 17-24 + future:** ~32 specs future; 4 pets bloqueados

### Resumo (Atualizado 2026-06-07 pós-limpeza legacy)

- ✓ **Specs antes da limpeza:** 154 em `docs/specs/a_implementar/`
- ✓ **Legacy specs absorvidas:** 7 em `docs/specs/absorvidas/legacy_pre_wave_reconciliation/`
- ✓ **Fila ativa após limpeza:** 147 specs wave-based em `docs/specs/a_implementar/`
- ✓ **Governança:** 1 spec (00.04)
- ✓ **Hardening/Quality gate:** 8 specs (01.01-01Q)
- ✓ **Core runtime:** 93 specs (02-12)
- ✓ **Documentação/consolidação:** 2 specs (12)
- ✓ **Future/mapeado:** ~32 specs (17-24 + future suffix)
- ✓ **Pets bloqueados:** 4 (WAVE 23)
- ✓ **Review required:** 0
- ✓ **Nenhuma spec ativa obsoleta**
- ✓ **Status:** READY_FOR_00_04

---

## Specs Antigas Absorvidas — Legacy Pre-Wave Cleanup (2026-06-07)

Após a reconciliação das 154 specs novas wave-based, 7 specs antigas da era pré-wave (SPEC_10-17 MVP) foram movidas para histórico:

### Moved to docs/specs/absorvidas/legacy_pre_wave_reconciliation/

| Spec antiga | Status | Motivo | Specs novas que cobrem | Decisão |
|---|---|---|---|---|
| `spec_14a_cave_enemy_spawnplan_materialization_run_stability.md` | MOVED_ABSORBED | Cobertura completa por WAVE 06 (loot/drops) e WAVE 10 (bosses) | WAVE 06, WAVE 10 | Não executar |
| `spec_14a_fix2_spawn_density_combat_feedback_damage_numbers.md` | MOVED_ABSORBED | Tuning de combat; coberto por WAVE 06 e WAVE 11 (UI hotbar) | WAVE 06, WAVE 11 | Não executar |
| `spec_14b_cave_snapshot_replay_enemy_plan.md` | MOVED_ABSORBED | Snapshot/replay movido integralmente para WAVE 06 | WAVE 06 | Não executar |
| `spec_cave_runtime_generation_checkpoints_boss_gates.md` | MOVED_ABSORBED | Cave runtime fatiado em WAVE 06 (procedural) e WAVE 10 (gates/bosses) | WAVE 06, WAVE 10 | Não executar |
| `spec_combat_movement_projectiles_melee_visuals_runtime.md` | MOVED_ABSORBED | Combat movement/projectiles distribuído; WAVE 06/10 covers bosses; movimento pode ser gap em WAVE 25+ | WAVE 06, WAVE 10, (possível WAVE 25+) | Candidato a legacy gap se gap confirmado |
| `spec_enemy_ai_roster_bestiary_faction_locks_runtime.md` | MOVED_ABSORBED | AI/roster/bestiary distribuído em WAVE 06 (AI), WAVE 22 (bestiary UI), WAVE 23 (pets); faction locks podem ser gap | WAVE 06, WAVE 22, WAVE 23 | Candidato a legacy gap se gap confirmado |
| `spec_ui_ux_full_gameplay_inventory_hotbar_menus.md` | MOVED_ABSORBED | Cobertura completa por WAVE 04 (21 UI specs) e WAVE 11 (HUD/projections) | WAVE 04, WAVE 11 | Não executar |

**Total absorvido:** 7 specs  
**Crosswalk:** `docs/specs/absorvidas/legacy_pre_wave_reconciliation/LEGACY_SPECS_CROSSWALK.md`  
**Review required:** 0 specs — todos têm cobertura clara

### Contagem Atualizada Após Cleanup

- ✓ **Specs antes de limpeza:** 154 em `a_implementar/` (154 novas wave-based + 7 antigas)
- ✓ **Specs após limpeza:** 147 em `a_implementar/` (todas new wave-based)
- ✓ **Specs absorvidas:** 7 em `absorvidas/legacy_pre_wave_reconciliation/`
- ✓ **Fila ativa:** 147 specs (100% wave-based)

### Status de Execução (Atualizado 2026-06-08)

**Waves executadas e BUILD_VALIDATED:**
- ✓ WAVE 00 — Auditoria existente
- ✓ WAVE 01 — Hardening/Quality Gate (8 specs)
- ✓ WAVE 02 — Time/Calendar/Weather/Lunar (7/8 BUILD_VALIDATED)
- ✓ WAVE 03 — Quest/Objective/Event (8/8 runtime BUILD_VALIDATED)
- ✓ WAVE 04 — UI Foundation (14/14 reports; BUILD_VALIDATED + CONTRACT_ONLY)
- ✓ WAVE 05 — Farm Gameplay Core (20/20 BUILD_VALIDATED; ~123 EditMode tests)
- ✓ WAVE 06 — Economy/Loot/Crafting/Shop/Cave (8/8 BUILD_VALIDATED; ~75 EditMode tests)
- (WAVE 07 NÃO EXISTE — sem specs 07_spec_*.md)
- ✓ WAVE 08 — City/NPC/Dialogue/Services (4/4 BUILD_VALIDATED; ~61 EditMode tests)
- ✓ WAVE 09 — Quest System (8/8 BUILD_VALIDATED; ~107 EditMode tests)
- ✓ WAVE 10 — Main Progression/Fonte/Endgame (4/4 BUILD_VALIDATED; ~72 EditMode tests)
- ✓ WAVE 11 — UI Projections/HUD/Input/Inventory/Menus (4/4 BUILD_VALIDATED; ~96 EditMode tests)

**Em execução:**
- WAVE 12 — Docs/Consolidation (2 specs, in progress 2026-06-08)

**Próxima execução permitida após WAVE 12:**
- WAVE 13 — Bestiary (2 specs)
- WAVE 17-24 — Future Mapped (somente quando autorizado)

**Permanentemente bloqueado:**
- WAVE 23 (pets) — HOLD/BLOCKED_SCOPE
- Specs com sufixo _future — NÃO EXECUTAR
- Future/mapped (WAVE 17-24) bloqueado por política
- Pets (WAVE 23) bloqueado como HOLD/BLOCKED_SCOPE
