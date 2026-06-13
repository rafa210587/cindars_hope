# SPEC EXECUTION ORDER

> **RECONCILIAÇÃO 2026-06-12:** a fila wave-based abaixo foi EXECUTADA (BUILD_VALIDATED) e movida para `.specs/a_implementar/executadas_build_validated/`. A fila executável atual é `.specs/a_implementar/fable/` (ver SPEC_REGISTRY_TO_IMPLEMENT, Lote FABLE). O conteúdo abaixo é registro histórico.


> **Dependency registry and execution sequence.**
> Agents implementing a spec: read ONLY the row for the target spec and its direct dependencies.
> Do not read the full file as part of minimum context — use `docs/00_PROJECT/CURRENT_STATE.md` instead.

Regra: uma spec so pode ser implementada se suas dependencias anteriores estiverem reconciliadas e sem pendencia bloqueadora.

---

## 📋 Consolidation Note (2026-06-01 SPEC_29B)

**MVP Closeout Status:** SPEC_18-28 Consolidation package created to close specs SPEC_10-17 (which were partial/residual).

- **Code Status:** All SPEC_18-28 have Phase 0-1 complete (Phase 0 audit + Phase 1 build/docs validation)
- **Play Mode Status:** Phase 2-3 human validation NOT YET EXECUTED (checklists prepared, awaiting local Unity Editor)
- **Spec Movement:** DO NOT MOVE specs to `implementados/` until Phase 2-3 evidence collected
- **Old Specs:** SPEC_10-17 status clarified below per closeout findings

See `docs/validation/spec_29b_phase0_human_acceptance_reconciliation_audit_matrix.md` for full reconciliation audit.

---

A etapa 00 foi reclassificada como implementado documental parcial. As specs runtime continuam dependendo da governanca documental consolidada em `.specs/`, mas nao devem tentar executar novamente a spec 00 antiga de `a_implementar`.

| Ordem | Spec | Status | Depende de | Bloqueia | Risco se antecipar |
|---|---|---|---|---|---|
| 00 | [spec_docs_001_single_source_specs_refinements_reconciliation_parcial](implementados/spec_docs_001_single_source_specs_refinements_reconciliation_parcial.md) | Implementado documental parcial | Nenhuma | Base para todas as specs futuras | Agentes podem implementar a partir de fonte errada se esta governanca for ignorada. |
| 01 | [spec_unity_compile_validation_protocol_and_scripts](implementados/spec_unity_compile_validation_protocol_and_scripts.md) | Implementado completo | 00 | 02-17 | Sem validacao Unity, mudancas runtime podem mascarar erro de compilacao. |
| 02 | [spec_save_002_schema_migration_v2](implementados/spec_save_002_schema_migration_v2.md) | Implementado parcial | 00, 01 implementada/parcial | 03-17 | Specs posteriores podem persistir dados sem contrato de migracao. |
| 03 | [spec_inventory_002_slots_capacity_ui_final](implementados/spec_inventory_002_slots_capacity_ui_final.md) | Implementado parcial | 00, 01, 02 implementada/parcial | 04, 06, 07, 10, 12, 17 | Economy, craft e equipment podem depender de um modelo de inventory ainda provisorio. |
| 04 | [spec_farm_004_irrigacao_solo_planting_ui](implementados/spec_farm_004_irrigacao_solo_planting_ui.md) | Implementado parcial | 00, 01, 02, 03 implementada/parcial | 05, 09, 17 | World activities e stamina podem duplicar custos/regras de tools. |
| 05 | [spec_world_002_activities_fishing_trees_pickups_loot](implementados/spec_world_002_activities_fishing_trees_pickups_loot.md) | Implementado parcial | 00-04 implementada/parcial | 06, 07, 10 | Loot/economy podem ser balanceados sobre drops incompletos. |
| 06 | [spec_economy_shop_stock_pricing_ui](implementados/spec_economy_shop_stock_pricing_ui.md) | Implementado completo | 00-05 implementada/parcial | 07, 08, 17 | Shop NPC, stock, pricing, modal UI e save validados em batchmode; Play Mode final pendente. |
| 07 | [spec_crafting_queue_workstations_recipes_ui](implementados/spec_crafting_queue_workstations_recipes_ui.md) | Implementado completo | 00-06 | 10, 17 | Workstations, queue, modal, starter kit e save/load validados em batchmode; Play Mode final pendente. |
| 08 | [spec_town_npc_dialogue_schedule_quests](implementados/spec_town_npc_dialogue_schedule_quests.md) | Implementado completo | 00-07 | 15, 17 | Runtime, dados, cena gerada e validator passaram em batchmode; Play Mode humano final pendente. |
| 09 | [spec_hunger_stamina_status_balance](implementados/spec_hunger_stamina_status_balance.md) | Implementado completo | 00-05 implementada/parcial | 10, 11, 12, 14, 17 | Hunger/stamina/status/time ligados nas cenas e validados em batchmode; Canvas final e Play Mode humano ficam para spec 17/checklist final. |
| 10 | [spec_equipment_durability_environment_loot_runtime](implementados/spec_equipment_durability_environment_loot_runtime.md) | Implementado parcial | 00-09 | 11, 12, 13, 14, 17 | Damage e enemy tuning ficam instaveis sem stats/equipment finais. |
| 11 | [spec_damage_status_elements_resistances_runtime](implementados/spec_damage_status_elements_resistances_runtime.md) | Implementado parcial | 00-10 | 12, 13, 14 | Armas, spells e IA podem duplicar formulas de dano. |
| 12 | [spec_player_combat_weapons_spells_skill_actions_runtime](implementados/spec_player_combat_weapons_spells_skill_actions_runtime.md) | Implementado parcial | 00-11 | 13, 14, 16, 17 | Play Mode testing humano; UI consolidada em SPEC 17. |
| 15 | [spec_cave_entry_death_anya_corpse_recovery](implementados/spec_cave_entry_death_anya_corpse_recovery.md) | Implementado parcial em codigo - gaps de orquestracao/Anya/spawn/restore e Play Mode pendente | 00-14 | 16, 17 | Skill respec e UI podem criar fluxos sem falha/recovery definidos; nao tratar corpse/death como fechamento funcional completo. |
| 16 | [spec_skill_trees_active_slots_respec_anya_runtime](implementados/spec_skill_trees_active_slots_respec_anya_runtime.md) | Implementado em codigo - compile/Unity e Play Mode humano pendentes | 00-15 | 17 | UI final pode expor skill tree incompleta ou sem persistencia validada em Play Mode. |
| 17C | [spec_ui_gameplay_closeout_skill_shop_prompts_actions_hud](implementados/spec_ui_gameplay_closeout_skill_shop_prompts_actions_hud.md) | Implementado completo - Play Mode validado 2026-05-26 | 15-17 | Nenhuma | U/K/L/buy/sell/save/load validados por humano sem erros. |
| 17D | [spec_ui_gameplay_shop_injection_equipment_slot_picker_closeout](implementados/spec_ui_gameplay_shop_injection_equipment_slot_picker_closeout.md) | Implementado completo - Play Mode validado 2026-05-26 | 17C | Nenhuma | Slot picker L com Chest/RightHand/LeftHand/Accessory; filtro e Esc validados por humano. |
| 17E | [spec_ui_gameplay_shop_session_lifecycle_npc_readiness_closeout](implementados/spec_ui_gameplay_shop_session_lifecycle_npc_readiness_closeout.md) | Implementado completo - Play Mode validado 2026-05-26 | 17D | Nenhuma | Lifecycle persistente e duas sessoes de shop validados por humano sem erros. |
| 17F | [spec_ui_gameplay_shop_modal_stack_responsive_names_closeout](implementados/spec_ui_gameplay_shop_modal_stack_responsive_names_closeout.md) | Implementado completo - Play Mode validado 2026-05-26 | 17E | Nenhuma | Modal stack sem mismatch; layout responsivo e nomes curtos validados por humano. |
| 17A | [spec_visual_world_scale_camera_sprite_profiles](implementados/spec_visual_world_scale_camera_sprite_profiles.md) | Implementado em codigo - Play Mode humano pendente | 17F | Nenhuma | VisualScaleProfileSO, VisualScaleApplicator, CameraScaleConfigSO, CameraScaleController; cave corridors parametrizados; Farm/Town bounds 4x; dotnet PASS 0 erros. |

## Observacao operacional

A spec 00 antiga em `a_implementar/spec_docs_single_source_specs_refinements_reconciliation_v1.md` nao deve ser executada novamente; ela permanece apenas como ponte historica ate remocao fisica futura.

A spec 01 foi implementada completamente como tooling minimo (validacao documental e Unity batchmode).
A spec 02 foi implementada parcialmente como infraestrutura de migration.
A spec 03 foi implementada parcialmente com slots, capacidade, migration v1->v2 e painel minimo; Drop runtime e Use especifico permanecem pendentes.
A spec 04 foi implementada parcialmente com solo/agua/plantio por inventory/menu contextual; Play Mode manual segue pendente.
A spec 05 foi implementada parcialmente com loot table, fishing timing e tree HP/regrowth; spawner dinamico/cave fishing/farm scene spots seguem pendentes.
As copias `a_implementar` ja promovidas das specs 10, 11, 12, 15 e 16 foram removidas durante o closeout 17C; o escopo residual ativo inicia nas specs parciais ainda listadas no registry.

A spec 15 possui implementacao parcial em codigo: `PlayerDeathController`, `Corpse`, `CorpseRecoveryManager`, DTOs de death/corpse e captura de `DeathSaveData` em `SaveManager`. Na validacao de codigo de 2026-05-26 nao foram encontrados `CaveDeathResolver`, `DeathSystemBootstrap`, `CorpseInteractable`, `CorpseSpawner`, `AnyaFountain`, `AnyaRespawnService` ou `AnyaFountainInteractable`; `RestoreDeathSaveData` ainda contem TODO. Portanto, a SPEC 15 nao deve ser tratada como fechamento funcional completo.

A spec 16 foi implementada em codigo em 2026-05-26. SkillTreeManager reescrito como MonoBehaviour; 5 arvores (55 nodes); SkillPurchaseService, SkillRespecService, SkillPassiveApplicator; skill trees acessiveis por `U`; save/load v5; respec na Fonte de Anya habilitado em codigo. Compile Unity e Play Mode humano seguem pendentes.

A spec 17 recebeu em 2026-05-26 o incremento UI Gameplay MVP: lojas com estoque real, buy/sell, inventory equipavel, painel de personagem/attributes (`K`), skill trees (`U`) e bloqueio de input durante modais. A spec ampla permanece em `a_implementar` porque Canvas final, pause/options, fluxos cave/corpse/toasts e validacao Play Mode nao foram fechados.

A spec 17C executa o closeout de integracao: `U` skill trees, `K` atributos/progressao, `L` equipamento, wiring de shop e scanner de missing scripts. Fechada em 2026-05-26 com Play Mode humano validado sem erros.

A spec 17D endurece a injecao de buy/sell e adiciona o picker de equipamento por slot via `L` com Chest/RightHand/LeftHand/Accessory. Fechada em 2026-05-26 com Play Mode humano validado sem erros.

A spec 17E corrige o lifecycle das sessoes de shop: o `ShopManager` passa a pertencer ao `GameBootstrap` persistente, os NPCs fazem rebind e readiness idempotentes. Fechada em 2026-05-26 com Play Mode humano validado sem erros.

A spec 17F corrige a higiene da stack de buy/sell com pop condicional, introduz nomes curtos e painel de detalhes e aplica scroll/layout responsivo. Fechada em 2026-05-26 com Play Mode humano validado sem erros.

A spec 17A (visual scale / world scale / camera scale / sprite profiles) foi implementada em codigo em 2026-05-26: VisualScaleProfileSO (23 categorias), VisualScaleApplicator, CameraScaleConfigSO, CameraScaleController com SmoothDamp; cave corridors parametrizados (CorridorMinWidth/MaxWidth); Farm bounds 40x34 (~4x area anterior), Town bounds 36x30 (~4x area anterior); editor tool CreateDefaultScaleAssets; validator ValidateSpec17AScaleConfig. dotnet build runtime e editor: PASS 0 erros. Play Mode humano pendente.

---

---

## Legacy Specs Cleanup (2026-06-07)

**Action:** 7 legacy specs from pre-wave era (SPEC_10-17 MVP) moved to `.specs/absorvidas/legacy_pre_wave_reconciliation/`.

**Reason:** All legacy specs are fully absorbed into new wave-based specs; no execution needed.

**Specs moved:**
- `spec_14a_cave_enemy_spawnplan_materialization_run_stability.md` → covered by WAVE 06, 10
- `spec_14a_fix2_spawn_density_combat_feedback_damage_numbers.md` → covered by WAVE 06, 11
- `spec_14b_cave_snapshot_replay_enemy_plan.md` → covered by WAVE 06
- `spec_cave_runtime_generation_checkpoints_boss_gates.md` → covered by WAVE 06, 10
- `spec_combat_movement_projectiles_melee_visuals_runtime.md` → covered by WAVE 06, 10 (movement may be gap for WAVE 25+)
- `spec_enemy_ai_roster_bestiary_faction_locks_runtime.md` → covered by WAVE 06, 22, 23
- `spec_ui_ux_full_gameplay_inventory_hotbar_menus.md` → covered by WAVE 04, 11

**Impact:** 
- Specs in `a_implementar/` now 100% wave-based (147 specs)
- No loss of functionality — all legacy features migrated to new wave specs
- Reference in `LEGACY_SPECS_CROSSWALK.md` for traceability

---

## Reconciliation Note (2026-06-07 BATCH_36 — Generated Specs)

**Generated Specs Reconciliation (2026-06-07 post-cleanup):** 154 specs generated, 7 legacy absorbed, 147 active.

- **Specs antes da limpeza:** 154 em `.specs/a_implementar/` (146 wave-based + 7 legacy)
- **Legacy specs absorvidas:** 7 movidas para `.specs/absorvidas/legacy_pre_wave_reconciliation/`
- **Fila ativa após limpeza:** 147 specs wave-based em `.specs/a_implementar/`
- **WAVE 00:** 1 governance spec (00.04 audit)
- **WAVE 01:** 8 hardening/quality gate specs (01.01-01.05, 01.06, 01.07, 01Q)
- **WAVE 02-12:** ~93 core runtime specs (blocked until 01Q)
- **WAVE 17-24:** ~32 future/expansion specs (blocked by policy)
- **HOLD/BLOCKED_SCOPE (Pets):** 4 pet/companion specs in WAVE 23 — explicit authorization required
- **Review required:** 0
- **Duplicates/Obsolete:** None in active queue

**Execution Readiness:**

✓ Quality gate (01Q) ready before WAVE 02+ runtime
✓ Governance/hardening specs (WAVE 00-01) ready for execution in order
✓ Legacy cleanup complete — no obsolete specs in active queue
✓ Future specs (17-24) properly marked and blocked; will not execute
✓ Pet/companion specs (23) marked as HOLD — execution blocked until explicitly authorized

**Blockage Status:** READY_FOR_00_04 only — execute WAVE 00.04 next. WAVE 02+ blocked until 01Q completes.

See `docs/validation/SPEC_RECONCILIATION_BATCH_36_EXECUTION_REPORT.md` for full reconciliation audit.
