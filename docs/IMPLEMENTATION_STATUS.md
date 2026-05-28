# Cindar's Hope - Implementation Status

> Status: tracking reconciliado por validacao estatica de codigo em 2026-05-26.
> Fonte oficial de specs: `docs/specs/`.
> A pasta raiz `specs/` foi removida e nao deve ser recriada.

## 1. Resumo executivo

A validacao de codigo ate a SPEC 16 confirma que o projeto possui baseline suficiente para seguir para a proxima etapa de UI/closeout, desde que as proximas execucoes nao tratem specs parciais como completas.

Nao ha erro CS0023 ativo em `ValidateItemAndShopData.cs`: o validador usa `entry.Item == null` para `StartingItem` struct.

Principais conclusoes:

- SPECS 00-09: base documental/tooling/runtime MVP majoritariamente implementada, com algumas areas historicas ainda parciais.
- SPECS 10-14: implementacao parcial/residual ativa; nao tratar como completas.
- SPEC 15: implementacao parcial em codigo. Existem death DTOs, `PlayerDeathController`, `Corpse`, `CorpseRecoveryManager` e captura de `DeathSaveData`, mas a orquestracao completa de cave death/Anya/corpse spawn/restore nao esta fechada no codigo validado.
- SPEC 16: implementada em codigo. Skill trees, 5 arvores/55 nodes, compra, slots ativos, respec service, save v5 e manager existem; Unity compile e Play Mode humano seguem pendentes.
- SPEC 17+: ja recebeu incrementos, mas permanece etapa de UI/closeout e validacao final.

## 2. Resumo por area

| Area | Status real | Evidencia / observacao |
|---|---|---|
| Claude Code project structure (.claude/) | Implementado completo | `.claude/settings.json`, `.claude/commands/`, `.claude/skills/`, `.claude/agents/`, `.claude/hooks/` |
| Governanca documental / fonte unica | Implementado documental parcial | `docs/specs/implementados/spec_docs_001_single_source_specs_refinements_reconciliation_parcial.md` |
| Unity compile validation protocol | Implementado completo | `docs/specs/implementados/spec_unity_compile_validation_protocol_and_scripts.md` |
| Core/event bus/bootstrap | Implementado parcial | `docs/specs/implementados/spec_core_001_event_bus_e_eventos_base.md`, `docs/specs/implementados/spec_core_002_bootstrap_managers_e_runtime_references.md` |
| Data/IDs/registries | Implementado | `docs/specs/implementados/spec_data_001_ids_registries_e_scriptableobjects.md` |
| Save/load JSON cross-scene | Implementado parcial | `docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md` |
| Save schema migration v2 | Implementado completo | `docs/specs/implementados/spec_save_002_schema_migration_v2.md` |
| Inventory slots/capacidade/UI minima | Implementado completo | `docs/specs/implementados/spec_inventory_002_slots_capacity_ui_final.md` |
| Farm irrigacao/solo/planting UI | Implementado completo | `docs/specs/implementados/spec_farm_004_irrigacao_solo_planting_ui.md` |
| World activities/fishing/trees/loot | Implementado completo | `docs/specs/implementados/spec_world_002_activities_fishing_trees_pickups_loot.md` |
| Farm loop/world activities | Implementado parcial | `docs/specs/implementados/spec_farm_001_farm_scene_movimento_interacao.md`, `docs/specs/implementados/spec_farm_002_plots_seeds_growth_harvest.md`, `docs/specs/implementados/spec_farm_003_arvores_pesca_pickups_world_activities.md` |
| Game time / day-night cycles / hunger-stamina balance | Implementado completo | `docs/specs/implementados/spec_hunger_stamina_status_balance.md` |
| Economy/hunger/crafting/town | Implementado parcial | `docs/specs/implementados/spec_economy_001_compra_venda_gold_e_sellables.md`, `docs/specs/implementados/spec_hunger_001_fome_comida_e_hp_por_fome.md`, `docs/specs/implementados/spec_craft_001_crafting_mvp.md`, `docs/specs/implementados/spec_town_001_town_scene_portais_npc_pip_comercio.md` |
| Combat/damage/enemy stats | Implementado parcial | `docs/specs/implementados/spec_combat_001_slime_melee_contact_damage_drops.md`, `docs/specs/implementados/spec_combat_002_enemy_data_driven_stats.md`, `docs/specs/implementados/spec_damage_001_damage_formula_mvp.md` |
| UI/tools/hotbar/progression debug | Implementado parcial | `docs/specs/implementados/spec_ui_001_debug_hud_e_feedback_mvp.md`, `docs/specs/implementados/spec_ui_002_hud_tools_hotbar_progression_debug.md`, `docs/specs/implementados/spec_tools_001_tools_equipment_hotbar_parcial.md`, `docs/specs/implementados/spec_progression_001_xp_level_atributos_parcial.md` |
| Cave runtime/procedural/stable run/boss gates | Implementado em codigo - validacao Unity pendente | `docs/specs/implementados/spec_cave_001_cave_scene_portal_e_runtime_basico.md` ate `spec_cave_008_debug_skip_confinement_wall_distance_hardening.md` |
| Overnight 2026-05-23 | Executado parcialmente | `docs/IMPLEMENTATION_DELIVERY_20260523.md`, `docs/refinements/implementados/ref_stabilizacao_overnight_specs_20260523.md` |
| Economy/shop/stock/pricing/UI (Spec 06) | Implementado completo | `docs/specs/implementados/spec_economy_shop_stock_pricing_ui.md` |
| Crafting queue/workstations/recipes/UI (Spec 07) | Implementado completo | `docs/specs/implementados/spec_crafting_queue_workstations_recipes_ui.md` |
| Town NPC/dialogue/wanderer/save hooks (Spec 08) | Implementado completo | `docs/specs/implementados/spec_town_npc_dialogue_schedule_quests.md` |
| Hunger/stamina/status/time (Spec 09) | Implementado completo | `docs/specs/implementados/spec_hunger_stamina_status_balance.md` |
| Equipment/durability/environment/loot (Spec 10) | Implementado parcial | `docs/specs/implementados/spec_equipment_durability_environment_loot_runtime.md` |
| Damage/status/elements/resistances (Spec 11) | Implementado parcial | `docs/specs/implementados/spec_damage_status_elements_resistances_runtime.md` |
| Player Combat/weapons/spells/skill actions (Spec 12) | Implementado parcial | `docs/specs/implementados/spec_player_combat_weapons_spells_skill_actions_runtime.md` |
| UI/Input/Shop/Sell Bugfix Bundle (Post-SPEC 12) | Implementado completo | `docs/specs/implementados/spec_bugfix_ui_input_shop_sell_bundle.md` |
| Enemy AI/roster/bestiary/faction locks (Spec 13) | Implementado parcial | `docs/specs/implementados/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md` |
| Skill trees/active slots/respec Anya (Spec 16) | Implementado em codigo - Play Mode humano pendente | `docs/specs/implementados/spec_skill_trees_active_slots_respec_anya_runtime.md` |
| UI Gameplay MVP: shops/sell/equipment/attributes/skills (Spec 17 incremento) | Implementado em codigo - Play Mode humano pendente; SPEC 17 ampla permanece aberta | `docs/specs/a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md`, `docs/validation/SPEC17_UI_GAMEPLAY_MVP_VALIDATION_20260526.md` |
| UI Gameplay closeout: skills/shop/K-L/prompts/actions (Spec 17C) | Implementado completo - Play Mode humano validado 2026-05-26 | `docs/specs/implementados/spec_ui_gameplay_closeout_skill_shop_prompts_actions_hud.md`, `docs/validation/SPEC17C_CLOSEOUT_VALIDATION_20260526.md` |
| UI Gameplay shop injection/equipment slot picker (Spec 17D) | Implementado completo - Play Mode humano validado 2026-05-26 | `docs/specs/implementados/spec_ui_gameplay_shop_injection_equipment_slot_picker_closeout.md`, `docs/validation/SPEC17D_CLOSEOUT_VALIDATION_20260526.md` |
| UI Gameplay ShopSession lifecycle/readiness (Spec 17E) | Implementado completo - Play Mode humano validado 2026-05-26 | `docs/specs/implementados/spec_ui_gameplay_shop_session_lifecycle_npc_readiness_closeout.md`, `docs/validation/SPEC17E_SHOP_SESSION_FIX_VALIDATION_20260526.md` |
| UI Gameplay shop modal/responsive/names (Spec 17F) | Implementado completo - Play Mode humano validado 2026-05-26 | `docs/specs/implementados/spec_ui_gameplay_shop_modal_stack_responsive_names_closeout.md`, `docs/validation/SPEC17F_SHOP_MODAL_UI_VALIDATION_20260526.md` |
| Visual scale / world scale / camera profiles (Spec 17A) | Implementado em codigo completo (17A-FIX aplicado) - Play Mode humano pendente | `docs/specs/implementados/spec_visual_world_scale_camera_sprite_profiles.md` |
| GameScaleConfigSO central config + valores reais 2x/3x/6x (17A-FIX) | Implementado - cave 160x96, boss 2.5x, arvores 3x, lago 6x, boss gates 15-90 no registry, enemy_meteor_ooze_king real | commit `bd06a3a` |
| UI/UX infraestrutura 17B: input routing, pause, toasts, context hint, death screen, checkpoint menu (Spec 17B parcial) | Implementado em codigo - GameplayInputRouter, PauseMenuController, NotificationToastController, ContextHintController, DeathScreenController, CaveCheckpointSideMenuController; UIEvents.cs; ModalType Pause/Death/CaveCheckpoint; SkillTreeGameplayPanelController yield a GameplayInputRouter. Play Mode humano pendente. | branch `dev` 2026-05-27 |
| SPEC 13A - Enemy taxonomy, profiles e contracts | Implementado em codigo - LoreTagline em EnemyDataSO; MinimumRoomSize em EnemySizeProfileSO; 4 novos VulnerabilityTriggerMode; CreateDefaultEnemyProfiles editor (16 factions, 6 size, 10 movement, 10 vuln); ValidateSpec13EnemyTaxonomyProfiles; 0 erros build. Assets gerados pelo menu Unity pendentes. | branch `dev` 2026-05-27 |
| SPEC 13B - Roster 40 EnemyDataSO | Implementado em codigo - PrimaryDamageTypeId em EnemyDataSO; CreateRoster40EnemyData editor (44 entries: 7 band1, 9 band2, 9 band3, 8 band4, 6 band5, 5 bosses); ValidateSpec13EnemyRoster; 0 erros build. Assets gerados pelo menu Unity pendentes. NOTA: roster alternativo (enemy_verdant_mite etc.) - reconciliacao com roster canonico (SPEC 13C) pendente. | branch `dev` 2026-05-27 |
| SPEC 13C - Enemy actions/action sets | Implementado em codigo - EnemyActionSO estendido (StatusApplyChance, VulnerabilityWindowTrigger, MinRange, MaxTargets, RequiresLineOfSight, IsInterruptible); EnemyActionSetSO estendido (FallbackActionId, RoleTags, Notes); CreateEnemyActionsAndSets editor (8 telegraph profiles, 71 EnemyActionSO, 40 EnemyActionSetSO); ValidateSpec13EnemyActions; 0 erros build. Assets gerados pelo menu Unity pendentes. | branch `dev` 2026-05-27 |
| SPEC 13D - EnemyBrain runtime MVP | Implementado em codigo - EnemyBrain reescrito (state machine data-driven: Idle/Patrol/Alert/Chase/Kite/GuardHold/AttackWindup/AttackRecover/Stunned/Dead); EnemyActionRuntime (cooldown por acao); EnemyVulnerabilityState (janela de vulnerabilidade); 3 database SOs (EnemyActionDatabaseSO, EnemyActionSetDatabaseSO, EnemyTelegraphProfileDatabaseSO); EnemyVulnerabilityStartedEvent/EndedEvent; ValidateSpec13EnemyBrainRuntime; 0 erros build. Wiring no Unity Editor (prefabs + databases) pendente. Roster 13B vs 13C reconciliacao ainda pendente. | branch `dev` 2026-05-27 |
| Input Manager | Debito tecnico futuro | Input Manager legado ativo; migracao para Input System requer spec propria para evitar regressao de gameplay/UI. |

## 3. Status oficial por SPEC/prompt ate 16

| Spec | Status real apos validacao de codigo | Observacao |
|---|---|---|
| 00 | Implementado documental parcial | Nao reexecutar spec antiga. |
| 01 | Implementado completo | Tooling minimo existe. |
| 02 | Implementado parcial | Infra de migration existe; manter parcial conforme ordem oficial. |
| 03 | Implementado parcial | Inventory tem slots e capacidade, mas ainda ha pendencias funcionais/UI. |
| 04 | Implementado parcial | Farm planting/irrigacao existem, mas Play Mode/polimento pendem. |
| 05 | Implementado parcial | World activities existem parcialmente; residual ativo. |
| 06 | Implementado completo em codigo | Shop/economy implementado; Play Mode final ainda depende das specs de closeout. |
| 07 | Implementado completo em codigo | Crafting/workstations/recipes em codigo; Play Mode final pendente. |
| 08 | Implementado completo em codigo | Town/NPC/dialogue em codigo; Play Mode final pendente. |
| 09 | Implementado completo MVP | Hunger/stamina/status/time fechados como MVP. |
| 10 | Implementado parcial | Nao tratar como completo. |
| 11 | Implementado parcial | Nao tratar como completo. |
| 12 | Implementado parcial | Nao tratar como completo. |
| 13 | Implementado parcial - residual ativo | Ainda listado como residual ativo. |
| 14 | Implementado parcial - residual ativo | Ainda listado como residual ativo. |
| 15 | Implementado parcial em codigo | Death/corpse base existe; Anya/orquestracao/spawn/restore incompletos no codigo validado. |
| 16 | Implementado em codigo | Skill tree stack existe; Unity compile/Play Mode humano pendentes. |

## 4. Evidencias de codigo validadas

### 4.1 Erro CS0023 ja corrigido

`Assets/_Game/Scripts/Editor/Validation/ValidateItemAndShopData.cs` usa:

```csharp
var entry = playerData.StartingItems[i];
if (entry.Item == null)
```

Como `StartingItem` e struct, isso esta correto. Nao ha mais `entry?.Item` nesse trecho.

### 4.2 SaveManager v5 / migrations

`SaveManager` esta em schema v5 e registra:

- `InventorySlotsV1ToV2Migration`
- `SaveV2ToV3Migration`
- `SaveV3ToV4Migration`
- `SaveV4ToV5Migration`

Tambem captura `SkillTree`, `ActiveSkillSlots`, `Death`, `Economy`, `Crafting`, `Stamina`, `GameTime`, `StatusEffects`, `EquipmentDurability` e `Npcs`.

## 5. SPEC 15 — Cave entry, death, Anya e corpse recovery

Status real: **implementado parcial em codigo**.

### 5.1 Confirmado em codigo

Arquivos/elementos encontrados e coerentes:

- `PlayerDeathController`: escuta `HPChangedEvent` e publica `PlayerDiedEvent` quando HP chega a zero.
- `Corpse`: modelo runtime com id, status, run/cave data, posicao, gold e listas de itens/equipment.
- `CorpseRecoveryManager`: controla active corpse, recuperacao de gold, itens e equipment, e publica eventos de recovery/parcial/replaced.
- `CorpseSaveData`, `CorpseItemSaveData`, `DeathStatsSaveData`, `DeathSaveData`: DTOs serializaveis.
- `SaveManager.CaptureDeathSaveData`: captura/preserva `DeathStats` e `ActiveCorpse` existente.

### 5.2 Nao encontrado / nao fechado no codigo validado

Nao foram encontrados no `dev` durante esta validacao:

- `CaveDeathResolver`
- `DeathSystemBootstrap`
- `CorpseInteractable`
- `CorpseSpawner`
- `AnyaFountain`
- `AnyaRespawnService`
- `AnyaFountainInteractable`

Alem disso, `SaveManager.RestoreDeathSaveData` ainda contem TODO e nao restaura active corpse para o runtime.

Conclusao: SPEC 15 nao deve ser tratada como completa. Ela e suficiente como fundacao parcial, mas nao como fluxo funcional fechado de morte -> respawn Anya -> corpse persistente -> recover.

## 6. SPEC 16 — Skill trees, active slots e respec Anya

Status real: **implementado em codigo; Unity compile/Play Mode humano pendentes**.

Confirmado em codigo/documentacao:

- `SkillTreeManager` como `MonoBehaviour`.
- Fallback `DefaultSkillCatalog` com 5 arvores / 55 nodes.
- `SkillPurchaseService` para custo, prerequisites, level minimo e capstone rules.
- `SkillRespecService` com primeiro respec gratuito e custo padrao posterior.
- `SkillPassiveApplicator` e eventos de derived stats.
- `SkillTreeSaveData` com `PurchasedNodeIds`, `ActiveSkillSlots` e `RespecCount`.
- `SaveV4ToV5Migration` inicializa `SkillTreeSaveData`.
- `SaveManager` captura e restaura `SkillTree`.
- `SkillTreeInputHandler` abre skill tree em `U`.

Pendencias:

- Rodar Unity compile real.
- Play Mode humano: subir level par, comprar node, equipar slot, respec, salvar/carregar.
- Validar a integracao real com Fonte de Anya em cena, porque SPEC 15/Anya ainda esta parcial no codigo validado.

## 7. Pendencias que nao devem bloquear a proxima etapa de UI/closeout

As pendencias abaixo nao impedem iniciar a proxima etapa, desde que fiquem declaradas como riscos/residuais:

- SPEC 10-14 continuam parciais/residuais.
- SPEC 15 nao fecha orquestracao/Anya/corpse restore.
- SPEC 16 precisa Unity/Play Mode humano.
- SPEC 17 ampla ainda precisa Canvas final, pause/options, fluxos cave/corpse/toasts e validacao final.

## 8. Proximo passo recomendado

Seguir para SPEC 17/UI/UX/closeout, com guardrails:

- Nao marcar 10-15 como completas.
- Nao depender de Anya/corpse restore como pronto.
- Validar UI em cima do que existe em codigo.
- Deixar claro que a validacao humana sera feita no final do pacote.
- Antes de fechamento final, executar Unity compile, scanners e Play Mode.
