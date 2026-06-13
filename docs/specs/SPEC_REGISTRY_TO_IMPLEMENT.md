# Registry de specs a implementar

Fonte unica de specs executaveis: `docs/specs/a_implementar/`.

Specs futuras/future mapped ficam fora da fila executavel em `docs/specs/a_implementar/features_futuras/` e devem ser ignoradas por automacoes de implementacao.

A pasta raiz `specs/` foi removida e nao deve ser recriada.

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
- **Fila ativa após limpeza:** 94 specs executaveis em `docs/specs/a_implementar/` (future specs movidas para `features_futuras/`)
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
| 07 | Playable Scene Integration | 5 integration gates executed | **WAVE_INTEGRATION_05_BUILD_VALIDATED_CODE_READY_SCENE_REVERTED** | 01 baseline + 02 architecture/manager audit + 03 player/camera/movement + 04 FarmScene foundation zones + 05 crop code ready; direct scene wiring reverted after FarmScene YAML corruption; WAVE_INTEGRATION_06 blocked until safe Unity scene wiring + Play Mode validation |
| 08 | City/NPC/Dialogue | 4 | **BUILD_VALIDATED** | ✓ 4/4 BUILD_VALIDATED; ~61 EditMode tests; closeout WAVE_08 |
| 09 | Quest/Player/Skills (duplicate 03) | 7 | **BUILD_VALIDATED** | ✓ 8/8 BUILD_VALIDATED; ~107 EditMode tests; closeout WAVE_09 |
| 10 | Endgame/Memory/Fonte | 4 | **BUILD_VALIDATED** | ✓ 4/4 BUILD_VALIDATED; ~72 EditMode tests; closeout WAVE_10 |
| 11 | UI Projections/HUD | 4 | **BUILD_VALIDATED** | ✓ 4/4 BUILD_VALIDATED; ~96 EditMode tests; closeout WAVE_11 |
| 12 | Docs/Consolidation | 2 | **BUILD_VALIDATED** | ✓ 2/2 BUILD_VALIDATED; closeout WAVE_12 |
| 13 | Bestiary | 4 | **BLOCKED_BY_FUTURE_SCOPE_MOVED_TO_FEATURES_FUTURAS** | All 4 specs moved to `a_implementar/features_futuras/`; do not execute without explicit human decision |

**Total WAVE 02-12:** 93 specs core/runtime
**WAVE 07:** WAVE_INTEGRATION_05_BUILD_VALIDATED_CODE_READY_SCENE_REVERTED (WAVE_INTEGRATION_06 is blocked until WAVE05 scene wiring is reapplied safely in Unity and Play Mode passes)
**Future specs moved out of active queue:** 53 files in `a_implementar/features_futuras/`

### Features futuras / Future mapped

Specs futuras foram movidas para:

`docs/specs/a_implementar/features_futuras/`

Elas nao fazem parte da fila executavel atual. Loop/batch execution must ignore `features_futuras`.

| Spec |
|---|
| `03_spec_quest_anti_softlock_validation_future.md` |
| `03_spec_quest_bestiary_discovery_objectives_future.md` |
| `03_spec_quest_debug_validation_tools_future.md` |
| `03_spec_quest_fonte_main_progression_hooks_future.md` |
| `04_spec_ui_menu_gamepad_navigation_future.md` |
| `04_spec_ui_social_npc_detail_future_runtime.md` |
| `13_spec_bestiary_knowledge_state_save_load_future_runtime.md` |
| `13_spec_bestiary_knowledge_ui_projection_future_runtime.md` |
| `13_spec_knowledge_discovery_event_runtime_future.md` |
| `13_spec_knowledge_research_npc_books_ruins_services_future_runtime.md` |
| `14_spec_companion_cave_assist_brain_balance_future_runtime.md` |
| `14_spec_companion_eligibility_recruitment_state_save_future_runtime.md` |
| `14_spec_companion_farm_jobs_board_automation_future_runtime.md` |
| `14_spec_companion_ui_hud_invite_visit_dialogue_hooks_future_runtime.md` |
| `15_spec_calendar_public_board_forecast_secret_visibility_future_runtime.md` |
| `15_spec_festival_event_minigames_seasonal_activities_future_runtime.md` |
| `15_spec_seasonal_economy_restock_demand_modifiers_future_runtime.md` |
| `15_spec_weather_lunar_cave_deep_modifiers_future_runtime.md` |
| `16_spec_cave_final_save_restriction_policy_future_runtime.md` |
| `16_spec_cave_snapshot_save_provider_restore_order_future_runtime.md` |
| `16_spec_final_choice_cinematic_presentation_future_runtime.md` |
| `16_spec_postgame_world_state_modifiers_endings_future_runtime.md` |
| `17_spec_partner_helper_companion_bridge_future_runtime.md` |
| `17_spec_social_dialogue_visits_personal_quest_hooks_future_runtime.md` |
| `17_spec_social_gifts_preferences_limits_future_runtime.md` |
| `17_spec_social_relationship_state_save_future_runtime.md` |
| `18_spec_ui_dialogue_choice_confirmation_advanced_future_runtime.md` |
| `18_spec_ui_localization_text_keys_icon_conventions_future_runtime.md` |
| `18_spec_ui_notification_journal_feedback_history_future_runtime.md` |
| `18_spec_ui_system_menu_options_accessibility_hooks_future_runtime.md` |
| `19_spec_level_100_boss_gate_preconditions_future_runtime.md` |
| `19_spec_level_101_encounters_rewards_anti_farm_future_runtime.md` |
| `19_spec_level_101_fixed_sequence_chamber_runtime_future.md` |
| `19_spec_level_101_ui_save_portal_handoff_future_runtime.md` |
| `20_spec_dormant_mana_root_endgame_farm_future_runtime.md` |
| `20_spec_living_water_mana_growth_conditions_future_runtime.md` |
| `20_spec_mana_economy_crafting_magic_anti_exploit_future_runtime.md` |
| `20_spec_mana_lunar_events_corruption_risk_future_runtime.md` |
| `21_spec_farm_automation_plan_blueprint_scheduling_future_runtime.md` |
| `21_spec_farm_automation_reports_risk_cost_balance_future_runtime.md` |
| `21_spec_farm_automation_storage_io_idempotency_future_runtime.md` |
| `21_spec_farm_full_automation_governor_future_runtime.md` |
| `22_spec_bestiary_combat_hud_known_weakness_overlay_future_runtime.md` |
| `22_spec_bestiary_compendium_knowledge_log_ui_future_runtime.md` |
| `22_spec_knowledge_books_collections_documentation_achievements_future_runtime.md` |
| `22_spec_research_service_npc_laboratory_knowledge_unlock_future_runtime.md` |
| `23_spec_pet_cave_alerts_treasure_trap_light_support_future_runtime.md` |
| `23_spec_pet_core_identity_bond_routine_save_future_runtime.md` |
| `23_spec_pet_home_area_feeding_items_farm_hints_future_runtime.md` |
| `23_spec_pet_hud_feedback_data_assets_economy_future_runtime.md` |
| `24_spec_cave_weather_lunar_deep_modifiers_future_runtime.md` |
| `24_spec_companion_advanced_party_equipment_tactical_ai_future_runtime.md` |
| `24_spec_festival_minigames_event_framework_future_runtime.md` |

**Total future moved:** 53 specs
**Pets:** WAVE 23 remains HOLD / BLOCKED_SCOPE

### Resumo (Atualizado 2026-06-07 pós-limpeza legacy)

- ✓ **Specs antes da limpeza:** 154 em `docs/specs/a_implementar/`
- ✓ **Legacy specs absorvidas:** 7 em `docs/specs/absorvidas/legacy_pre_wave_reconciliation/`
- ✓ **Fila ativa após limpeza:** 94 specs executaveis em `docs/specs/a_implementar/` (future specs movidas para `features_futuras/`)
- ✓ **Governança:** 1 spec (00.04)
- ✓ **Hardening/Quality gate:** 8 specs (01.01-01Q)
- ✓ **Core runtime:** 93 specs (02-12)
- ✓ **Documentação/consolidação:** 2 specs (12)
- ✓ **Future/mapeado:** 53 specs movidas para `docs/specs/a_implementar/features_futuras/`
- ✓ **Pets bloqueados:** 4 (WAVE 23)
- ✓ **Review required:** 0
- ✓ **Nenhuma spec ativa obsoleta**
- ✓ **Status:** ACTIVE_QUEUE_RECONCILED_FUTURE_MOVED

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
- ✓ **Fila ativa:** 94 specs executaveis (future specs movidas para `features_futuras/`)

### Status de Execução (Atualizado 2026-06-08)

**Waves executadas e BUILD_VALIDATED:**
- ✓ WAVE 00 — Auditoria existente
- ✓ WAVE 01 — Hardening/Quality Gate (8 specs)
- ✓ WAVE 02 — Time/Calendar/Weather/Lunar (7/8 BUILD_VALIDATED)
- ✓ WAVE 03 — Quest/Objective/Event (8/8 runtime BUILD_VALIDATED)
- ✓ WAVE 04 — UI Foundation (14/14 reports; BUILD_VALIDATED + CONTRACT_ONLY)
- ✓ WAVE 05 — Farm Gameplay Core (20/20 BUILD_VALIDATED; ~123 EditMode tests)
- ✓ WAVE 06 — Economy/Loot/Crafting/Shop/Cave (8/8 BUILD_VALIDATED; ~75 EditMode tests)
- WAVE 07.01 - Unity Clean Baseline + Scene Inventory (BUILD_VALIDATED; attached spec authorized by human; no scene/prefab/asset changes)
- WAVE_INTEGRATION_02 - Scene Architecture e Persistent Managers (BUILD_VALIDATED; manager audit and architecture decision complete)
- WAVE_INTEGRATION_03 - Player Spawn, Camera, and Movement Baseline (BUILD_VALIDATED_SCENE_WIRED; FarmScene structural wiring validated, Play Mode pending human Unity action)
- WAVE_INTEGRATION_04 - FarmScene Rebuild Foundation (BUILD_VALIDATED_SCENE_WIRED; FarmScene foundation zone markers added, Play Mode pending human Unity action)
- WAVE_INTEGRATION_05 - Farm Interactables/Crops/Soil/Water/Harvest (BUILD_VALIDATED_CODE_READY_SCENE_REVERTED; existing FarmPlot runtime reused, code ready, direct scene wiring reverted after FarmScene YAML corruption; WAVE_INTEGRATION_06 blocked pending safe Unity wiring and Play Mode)
- ✓ WAVE 08 — City/NPC/Dialogue/Services (4/4 BUILD_VALIDATED; ~61 EditMode tests)
- ✓ WAVE 09 — Quest System (8/8 BUILD_VALIDATED; ~107 EditMode tests)
- ✓ WAVE 10 — Main Progression/Fonte/Endgame (4/4 BUILD_VALIDATED; ~72 EditMode tests)
- ✓ WAVE 11 — UI Projections/HUD/Input/Inventory/Menus (4/4 BUILD_VALIDATED; ~96 EditMode tests)

**Concluídas:**
- ✓ WAVE 12 — Docs/Consolidation (2 specs, BUILD_VALIDATED 2026-06-08)

**Bloqueadas (requerem autorização humana explícita):**
- WAVE 13 — Bestiary (4 specs — BLOCKED_BY_FUTURE_SCOPE_MOVED_TO_FEATURES_FUTURAS)
- Features futuras / Future mapped em `docs/specs/a_implementar/features_futuras/` (somente quando autorizado)

**Permanentemente bloqueado:**
- WAVE 23 (pets) — HOLD/BLOCKED_SCOPE
- Specs com sufixo _future em `features_futuras/` — NÃO EXECUTAR
- Future/mapped (WAVE 17-24) bloqueado por política
- Pets (WAVE 23) bloqueado como HOLD/BLOCKED_SCOPE

---

## Lote FABLE — Gap Closure (gerado 2026-06-12)

Specs densas geradas por gap analysis completa de `docs/design/**` (42 directions) contra o
código real. Local: `docs/specs/a_implementar/fable/` (subpasta — fora da fila executável
automática até promoção humana). Índice e análise: `fable/fable_00_index_gap_analysis.md`.

| Ordem | Spec | Bloco | Prioridade | Lacuna que fecha |
|---|---|---|---|---|
| F01 | `fable/fable_01_spec_status_effects_canonical_set_runtime.md` | A combate | P1 | 6 status canônicos faltantes + aplicação em skills/spells |
| F02 | `fable/fable_02_spec_combat_weapon_actions_derived_stats_runtime.md` | A combate | P1 | light/heavy/charged + stagger + DerivedStats no dano real |
| F03 | `fable/fable_03_spec_equipment_mechanical_baselines_runtime.md` | A combate | P1 | ASPD/scaling/charged por arma + armadura reduzindo dano |
| F04 | `fable/fable_04_spec_enemy_threat_pack_coordination_runtime.md` | B inimigos | P2 | threat memory + pack alert + leash coletivo |
| F05 | `fable/fable_05_spec_cave_boss_phase_ai_runtime.md` | B inimigos | P2 | IA de fases de boss (thresholds/action sets/adds) |
| F06 | `fable/fable_06_spec_enemy_loot_tables_vulnerability_tags_runtime.md` | A/B | P2 | loot tables por família + matching de vulnerabilidade |
| F07 | `fable/fable_07_spec_magic_learning_unlock_sources_runtime.md` | C magia | P2 | knownSpellIds + scrolls/tomes/wands (seção de save) |
| F08 | `fable/fable_08_spec_magic_spell_shapes_targeting_runtime.md` | C magia | P2 | spell shapes (cone/nova/self/barrier) + cast time |
| F09 | `fable/fable_09_spec_cave_biome_layout_variety_runtime.md` | D mundo | P2 | layout por bioma + hazards + salas de tesouro (ADR-0005) |
| F10 | `fable/fable_10_spec_main_quest_act1_playable_runtime.md` | E conteúdo | P1 | Ato 1 da main quest jogável (Fragmento da Água) |
| F11 | `fable/fable_11_spec_city_interiors_doors_schedule_anchors_scene.md` | D mundo | P2 | TIME_BLOCK_DEBT + SCENE_WIRING_DEBT (WI-25) + portas/interiores |
| F12 | `fable/fable_12_spec_farm_animals_runtime_scene_integration.md` | D mundo | P2 | animais de fazenda ponta a ponta (seção de save) |
| F13 | `fable/fable_13_spec_save_debt_closure_runtime.md` | F fundação | P1 | CAVE_ENEMY_HP + DAILY_GOALS + CAVE_RUN save debts |
| F14 | `fable/fable_14_spec_ui_canvas_screens_integration_runtime.md` | F fundação | P1 | 4 telas IMGUI → Canvas (destrava specs 04_ CONTRACT_ONLY) |

Regras do lote: F13 e specs com seção de save (F07/F12) nunca em paralelo entre si;
F02/F03/F05/F06/F08/F14 sem paralelismo (locks centrais). Directions cobertas por
`features_futuras/` (bestiary, companions, pets, social, nível 100/101, mana, automação,
festivais) NÃO foram duplicadas — ver tabela no índice fable_00.

### Corretivas de Aderência (auditoria fable_00B, 2026-06-12)

Auditoria de código real revelou sistemas das WAVES 02-11 **órfãos** (testados, jamais
instanciados). Specs corretivas de wiring:

| # | Spec | Fecha |
|---|---|---|
| F15 | `fable/fable_15_spec_world_weather_farm_orphan_systems_wiring.md` | P0 — clima/chuva/refresh/qualidade/fertilizante ligados |
| F16 | `fable/fable_16_spec_player_fatigue_sleep_collapse_wiring.md` | P0 — fadiga/sono/colapso 02:00 + cama |
| F17 | `fable/fable_17_spec_fonte_anya_physical_interactable_runtime.md` | P0 — Fonte física + respawn + Água Viva |
| F18 | `fable/fable_18_spec_derived_stats_vitals_application_runtime.md` | P1 — vitals/resistências derivados aplicados |
| F19 | `fable/fable_19_spec_city_services_schedule_reconciliation.md` | P1 — dedup schedule (City/ vs NPC/) + licenças vivas |
| F20 | `fable/fable_20_spec_calendar_clock_hud_day_detail_ui.md` | P2 — refatoração da 02_spec_calendar_ui (absorvida) |

### Triagem da fila (2026-06-12)

- **102 specs executadas/absorvidas** movidas do nível raiz para
  `a_implementar/executadas_build_validated/` (README com estado e crosswalks). NÃO reexecutar.
- Fila executável atual = `fable/` (F01-F20, após promoção humana) + `features_futuras/` (bloqueadas).
- Análise completa: `fable/fable_00B_adherence_audit_queue_triage.md`.
- Docs validation: **exit 0, zero erros** (10 reports retro-preenchidos; 2 citações de
  amendment marcadas archived; FIX_001/test_harness fora do nível raiz).


### Expansão F21-F42 + Plano Mestre de Execução (2026-06-12)

Pós-canonização dos 6 catálogos FABLE, geradas 22 specs novas e o plano mestre
`fable/fable_00C_master_execution_plan.md` (revisão F01-F20 com 9 emendas vinculantes,
ordem global em 9 batches, matriz de paralelismo, protocolo por spec, checkpoints M1-M4).

| # | Spec | Fecha |
|---|---|---|
| F21 | `fable/fable_21_spec_bestiary_knowledge_runtime.md` | conhecimento por descoberta + save |
| F22 | `fable/fable_22_spec_essence_tempering_forge.md` | têmpera elemental permanente (Brumdar) |
| F23 | `fable/fable_23_spec_accessories_relics_runtime.md` | 3 slots + 12 acessórios + 4 relíquias |
| F24 | `fable/fable_24_spec_enemy_moves_elite_affixes_runtime.md` | 12 Moves canônicos faltantes + elites |
| F25 | `fable/fable_25_spec_npc_unique_services_runtime.md` | 8 serviços únicos por NPC |
| F26 | `fable/fable_26_spec_friendship_state_contract.md` | amizade níveis 0-5 + save |
| F27 | `fable/fable_27_spec_perfect_block_posture_runtime.md` | perfect block + CoreExposed |
| F28 | `fable/fable_28_spec_dialogue_conditions_pools.md` | falas condicionais (estação/clima/amizade) |
| F29 | `fable/fable_29_spec_canonical_skill_catalog_migration.md` | ~70 skills canônicas + migração (SOLO) |
| F30 | `fable/fable_30_spec_catalog_consistency_validator.md` | validador editor de catálogos |
| F31 | `fable/fable_31_spec_magic_items_unidentified_runtime.md` | itens mágicos não-identificados |
| F32 | `fable/fable_32_spec_item_catalog_data_expansion.md` | ItemDatabase = catálogo (~118) |
| F33 | `fable/fable_33_spec_bestiary_data_expansion_60_creatures.md` | 60 criaturas + 4 bosses finais |
| F34 | `fable/fable_34_spec_quest_sources_infrastructure.md` | 5 fontes de quest + XP escalado |
| F35 | `fable/fable_35_spec_npc_side_quest_chains.md` | 12 cadeias de side quest |
| F36 | `fable/fable_36_spec_main_quest_acts_2_4.md` | main quest atos 2-4 |
| F37 | `fable/fable_37_spec_festivals_lunar_events_runtime.md` | festivais + picos lunares + eventos |
| F38 | `fable/fable_38_spec_minimap_v1_runtime.md` | minimapa v1 (fog-of-war na caverna) |
| F39 | `fable/fable_39_spec_inferred_player_class_runtime.md` | classe inferida + títulos |
| F40 | `fable/fable_40_spec_town_48x42_relayout.md` | cidade 48×42 distritos canônicos |
| F41 | `fable/fable_41_spec_farm_lot_expansions.md` | lotes compráveis da fazenda |
| F42 | `fable/fable_42_spec_progression_cap100_xp_curve.md` | cap 100 + curva XP (SOLO, P0) |

Ordem linear de execução (fable_00C PARTE C):
`F15 F16 F17 | F13 F42 | F01 F02 F03 F18 F27 | F04 F24 F05 | F07 F08 F31 |
F30 F32 F06 F33 F22 F23 | F19 F11 F09 F40 F41 F12 F37 |
F29 F39 F25 F28 F26 F34 F35 F10 F36 | F14 F20 F38` — 42 specs.
### Specs finais F43-F47 + enumeração de execução (2026-06-12)

Fechamento da geração: 5 specs finais para os gaps aprovados que estavam anotados como
"futuros" + todas as F21-F42 expandidas ao formato SpecKit denso (≈300 linhas cada).

| # | Spec | Fecha |
|---|---|---|
| F43 | `fable/fable_43_spec_endgame_act5_final_bosses_choice.md` | endgame: Ato 5, 4 bosses finais, escolha Proteger/Selar/Usar |
| F44 | `fable/fable_44_spec_cave_save_completion_policy.md` | snapshot multi-nível no save + política de save em boss |
| F45 | `fable/fable_45_spec_bestiary_ui_full_codex.md` | aba Bestiário completa (codex com fichas/spoiler tiers) |
| F46 | `fable/fable_46_spec_romance_foundation_runtime.md` | romance: candidatos canônicos, bi, poliamor 2, gates |
| F47 | `fable/fable_47_spec_derived_stats_followups_closeout.md` | PlayerSpeedComposer + craft/repair hooks + duração por resistência |

ORDEM DE EXECUÇÃO OFICIAL: `fable/fable_00C_master_execution_plan.md` PARTE E —
enumeração E01-E47 (E01-E10 já BUILD_VALIDATED), grupos paralelos P1-P5, save specs
nunca simultâneas, E33 (skills) solo total.
### Auditorias de reconstrutibilidade/completude → F48-F60 + retro-specs (2026-06-12)

3 auditorias paralelas (código↔specs; design↔specs ×2) acharam gaps. Gerados:

| # | Spec | Fecha |
|---|---|---|
| F48 | `fable/fable_48_spec_bow_ammo_elemental_arrows_runtime.md` | munição de arco + flechas elementais (GAP-ALTA) |
| F49 | `fable/fable_49_spec_high_tier_gear_crafting_upgrades.md` | craft Mithril+ / upgrades +1..+3 (GAP-ALTA) |
| F50 | `fable/fable_50_spec_fishing_v2_lakes_tables.md` | pesca v2: tabelas bioma/estação/clima (GAP-ALTA) |
| F51 | `fable/fable_51_spec_zrix_cave_contracts.md` | contratos do Zrix cc_* (GAP-ALTA) |
| F52 | `fable/fable_52_spec_cave_secret_quests_goblin_visitor.md` | 8 scq_* + goblin visitante (GAP-ALTA) |
| F53 | `fable/fable_53_spec_festival_quests.md` | 8 fq_* (GAP-ALTA) |
| F54 | `fable/fable_54_spec_forage_shipping_overnight_runtime.md` | forrageio sazonal + shipping noturno |
| F55 | `fable/fable_55_spec_farm_processing_greenhouse.md` | queijaria/barril + estufa mínima |
| F56 | `fable/fable_56_spec_system_tab_title_flow.md` | aba Sistema + título/new game |
| F57 | `fable/fable_57_spec_living_city_birthdays_inn_reputation_adr.md` | aniversários + estalagem + ADR reputação |
| F58 | `fable/fable_58_spec_audio_sfx_hooks.md` | AudioManager + hooks por evento |
| F59 | `fable/fable_59_spec_combat_telemetry_playmode.md` | telemetria TTK/stamina vs BALANCE parte G |
| F60 | `fable/fable_60_spec_cave_traps_by_biome_tier.md` | armadilhas determinísticas por bioma/tier |

Emendas: F21 (+1 skill point por 10 entradas Estudadas, máx 5) e F34 (QuestSource.CaveContract).
Retro-specs: `implementados/spec_retro_01..09` documentam o código sem spec (slice 2026-06-12
+ WI-17..26) para reconstrutibilidade. Ordem/janelas: fable_00C PARTE F. Placar: 60 funcionais
(10 executadas), 9 retro, fundação+legado migrados para .specs/.
### Auditorias de MVP/shipping → F61-F67 + Refinamento v2 (2026-06-12)

| # | Spec | Fecha |
|---|---|---|
| F61 | `fable/fable_61_spec_build_standalone_windows.md` | build standalone + cenas no EditorBuildSettings (BLOQUEADOR) |
| F62 | `fable/fable_62_spec_onboarding_control_hints.md` | hints contextuais de controles + tutorial de combate (BLOQUEIA MVP) |
| F63 | `fable/fable_63_spec_new_game_intro_hook.md` | intro do New Game + carta→Corvus (texto gated por lore) |
| F64 | `fable/fable_64_spec_death_screen_canvas_corpse_messaging.md` | tela de morte Canvas explicando corpse recovery |
| F65 | `fable/fable_65_spec_daily_goals_closeout_reward_hud.md` | recompensa + HUD de metas (fecha débitos WI-24) |
| F66 | `fable/fable_66_spec_code_debt_cleanup_slice_mode.md` | slice mode, CaveDeathResolver TODOs, PlayerHitEvent, STAMINA_BLOCK_DEBT |
| F67 | `fable/fable_67_spec_canonical_governance_input_map_adr.md` | input map game_rule + ADR densidade + re-mapa refinamentos |

Emendas: F34-C (PREREQUISITE_UI_DEBT), F20-C (widget XP/nível + toast level up).

### Refinamento v2 RESPONDIDO → F68-F70 + 18 emendas EMENDA-D (2026-06-12)

| # | Spec | Fecha |
|---|---|---|
| F68 | `fable/fable_68_spec_world_god_marks_altars.md` | Marcas dos Deuses: 11 altares com bônus diário em caverna/cidade/fazenda (decisão 3.4; catálogo no apêndice A.5 do doc de decisões) |
| F69 | `fable/fable_69_spec_combat_sprint_runtime.md` | sprint em combate 3.8-4.2 tiles/s a 8 stamina/s (decisão 6.5-A; após F47) |
| F70 | `fable/fable_70_spec_npc_side_quest_chains_wave2.md` | 2ª leva de cadeias side: 11 NPCs, ~33 quests (decisão 8.3; após F35) |

Decisões vinculantes: `docs/design/FABLE_DECISOES_RESPOSTAS_v2.0.md` (+ APÊNDICE LORE A.1-A.5).
Emendas EMENDA-D: F08/F11/F12/F14/F20/F21/F37/F41/F43/F45/F46/F49/F50/F56/F61/F63/F66.
MVP = LOTE INTEIRO (decisão 7.6). Placar: 70 funcionais (10 executadas) + 9 retro.

### Refinamento v3 RESPONDIDO → fable_71 + ADR-0010..0014 + emendas EMENDA-V3 (2026-06-13)

| # | Spec/Artefato | Fecha |
|---|---|---|
| F71 | `fable/fable_71_spec_combat_feel_pass.md` | hit-stop + screen shake + HudSuppressionChangedEvent (números de dano já existem) |

Decisões vinculantes: `docs/design/FABLE_DECISOES_RESPOSTAS_v3.0.md` (skills/itens/companions/boas práticas).
ADRs novos: ADR-0010 (reconcilia skill_tree_rules+inventory_equipment_rules com código FABLE), ADR-0011
(arte 32px/tile), ADR-0012 (localização P4), ADR-0013 (input teclado/mouse v1), ADR-0014 (dificuldade única).
Novos docs de design: SKILL_NUMERIC_ADDENDUM_v1.0, NPC_GIFT_TASTE_MATRIX_v1.0, COMPANION_ROLES_CATALOG_v1.0.
Emendas EMENDA-V3: F03/F14/F26/F29/F32/F42/F43/F56/F58 + COMPANIONS_DIRECTION + 4 specs 14_companion spec-ready.
Placar: 71 funcionais (10 executadas, 61 a executar E11-E71) + 9 retro. Ordem: fable_00C PARTE I.

### Cobertura de design v3 → fable_72/73 + companions densas (2026-06-13)

| # | Spec | Fecha |
|---|---|---|
| F72 | `fable/fable_72_spec_npc_gift_giving_taste_runtime.md` | dar presente + reação por gosto por NPC (reusa F26 + NPC_GIFT_TASTE_MATRIX) |
| F73 | `fable/fable_73_spec_localization_string_table_runtime.md` | tabela id→string desde a P4 (ADR-0012) |

Companions: as 4 specs `features_futuras/14_spec_companion_*` foram ELEVADAS de esqueleto a SpecKit
denso (cave-assist/brain, eligibility/recruitment/save+provider, farm-jobs/board, ui/hud) refletindo
o COMPANION_ROLES_CATALOG e as decisões v3 bloco 3 — seguem WAVE 14 (gated), fora do lote v1.
Game_rules NÃO viram spec (regras consumidas; domínios já cobertos). Placar lote v1: 73 funcionais
(10 executadas, 63 a executar E11-E73) + 9 retro. Ordem: fable_00C PARTE J.
Ordem/janelas: fable_00C PARTE H e .specs/README.md.