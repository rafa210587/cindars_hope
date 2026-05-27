# SPEC EXECUTION ORDER

Regra: uma spec so pode ser implementada se suas dependencias anteriores estiverem reconciliadas e sem pendencia bloqueadora.

A etapa 00 foi reclassificada como implementado documental parcial. As specs runtime continuam dependendo da governanca documental consolidada em `docs/specs/`, mas nao devem tentar executar novamente a spec 00 antiga de `a_implementar`.

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
| 13 | [spec_enemy_ai_roster_bestiary_faction_locks_runtime](a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md) | Implementado parcial - escopo residual ativo | 00-12 | 14, 15, 17 | Cave generation pode distribuir inimigos sem regras finais. |
| 14 | [spec_cave_runtime_generation_checkpoints_boss_gates](a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md) | Implementado parcial - escopo residual ativo | 00-13 | 15, 17 | Entry/death pode quebrar replay se cave ainda rerollar. |
| 15 | [spec_cave_entry_death_anya_corpse_recovery](implementados/spec_cave_entry_death_anya_corpse_recovery.md) | Implementado em codigo - Play Mode humano pendente | 00-14 | 16, 17 | Skill respec e UI podem criar fluxos sem falha/recovery definidos. |
| 16 | [spec_skill_trees_active_slots_respec_anya_runtime](implementados/spec_skill_trees_active_slots_respec_anya_runtime.md) | Implementado em codigo - Play Mode humano pendente | 00-15 | 17 | UI final pode expor skill tree incompleta ou sem persistencia. |
| 17 | [spec_ui_ux_full_gameplay_inventory_hotbar_menus](a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md) | Implementacao parcial - MVP gameplay em codigo | 00-16 | Nenhuma | Restam superficies da spec ampla e Play Mode humano antes de promocao. |
| 17C | [spec_ui_gameplay_closeout_skill_shop_prompts_actions_hud](implementados/spec_ui_gameplay_closeout_skill_shop_prompts_actions_hud.md) | Implementado completo - Play Mode validado 2026-05-26 | 15-17 | Nenhuma | U/K/L/buy/sell/save/load validados por humano sem erros. |
| 17D | [spec_ui_gameplay_shop_injection_equipment_slot_picker_closeout](implementados/spec_ui_gameplay_shop_injection_equipment_slot_picker_closeout.md) | Implementado completo - Play Mode validado 2026-05-26 | 17C | Nenhuma | Slot picker L com Chest/RightHand/LeftHand/Accessory; filtro e Esc validados por humano. |
| 17E | [spec_ui_gameplay_shop_session_lifecycle_npc_readiness_closeout](implementados/spec_ui_gameplay_shop_session_lifecycle_npc_readiness_closeout.md) | Implementado completo - Play Mode validado 2026-05-26 | 17D | Nenhuma | Lifecycle persistente e duas sessoes de shop validados por humano sem erros. |
| 17F | [spec_ui_gameplay_shop_modal_stack_responsive_names_closeout](implementados/spec_ui_gameplay_shop_modal_stack_responsive_names_closeout.md) | Implementado completo - Play Mode validado 2026-05-26 | 17E | Nenhuma | Modal stack sem mismatch; layout responsivo e nomes curtos validados por humano. |

## Observacao operacional

A spec 00 antiga em `a_implementar/spec_docs_single_source_specs_refinements_reconciliation_v1.md` nao deve ser executada novamente; ela permanece apenas como ponte historica ate remocao fisica futura.

A spec 01 foi implementada completamente como tooling minimo (validacao documental e Unity batchmode).
A spec 02 foi implementada parcialmente como infraestrutura de migration.
A spec 03 foi implementada parcialmente com slots, capacidade, migration v1->v2 e painel minimo; Drop runtime e Use especifico permanecem pendentes.
A spec 04 foi implementada parcialmente com solo/agua/plantio por inventory/menu contextual; Play Mode manual segue pendente.
A spec 05 foi implementada parcialmente com loot table, fishing timing e tree HP/regrowth; spawner dinamico/cave fishing/farm scene spots seguem pendentes.
As copias `a_implementar` ja promovidas das specs 10, 11, 12, 15 e 16 foram removidas durante o closeout 17C; o escopo residual ativo inicia nas specs parciais ainda listadas no registry.

A spec 15 foi implementada em codigo em 2026-05-25/26. Compile validation requer Unity Editor fechado para rodar RunUnityCompileValidation.ps1. Play Mode humano permanece pendente.

A spec 16 foi implementada em codigo em 2026-05-26. SkillTreeManager reescrito como MonoBehaviour; 5 arvores (55 nodes); SkillPurchaseService, SkillRespecService, SkillPassiveApplicator; skill trees acessiveis por `U`; save/load v5; respec na Fonte de Anya habilitado. Play Mode humano pendente.

A spec 17 recebeu em 2026-05-26 o incremento UI Gameplay MVP: lojas com estoque real, buy/sell, inventory equipavel, painel de personagem/attributes (`K`), skill trees (`U`) e bloqueio de input durante modais. A spec ampla permanece em `a_implementar` porque Canvas final, pause/options, fluxos cave/corpse/toasts e validacao Play Mode nao foram fechados.

A spec 17C executa o closeout de integracao: `U` skill trees, `K` atributos/progressao, `L` equipamento, wiring de shop e scanner de missing scripts. Fechada em 2026-05-26 com Play Mode humano validado sem erros.

A spec 17D endurece a injecao de buy/sell e adiciona o picker de equipamento por slot via `L` com Chest/RightHand/LeftHand/Accessory. Fechada em 2026-05-26 com Play Mode humano validado sem erros.

A spec 17E corrige o lifecycle das sessoes de shop: o `ShopManager` passa a pertencer ao `GameBootstrap` persistente, os NPCs fazem rebind e readiness idempotentes. Fechada em 2026-05-26 com Play Mode humano validado sem erros.

A spec 17F corrige a higiene da stack de buy/sell com pop condicional, introduz nomes curtos e painel de detalhes e aplica scroll/layout responsivo. Fechada em 2026-05-26 com Play Mode humano validado sem erros.
