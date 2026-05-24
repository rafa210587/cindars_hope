# SPEC EXECUTION ORDER

Regra: uma spec so pode ser implementada se suas dependencias anteriores estiverem reconciliadas e sem pendencia bloqueadora.

A etapa 00 foi reclassificada como implementado documental parcial. As specs runtime continuam dependendo da governanca documental consolidada em `docs/specs/`, mas nao devem tentar executar novamente a spec 00 antiga de `a_implementar`.

| Ordem | Spec | Status | Depende de | Bloqueia | Risco se antecipar |
|---|---|---|---|---|---|
| 00 | [spec_docs_001_single_source_specs_refinements_reconciliation_parcial](implementados/spec_docs_001_single_source_specs_refinements_reconciliation_parcial.md) | Implementado documental parcial | Nenhuma | Base para todas as specs futuras | Agentes podem implementar a partir de fonte errada se esta governanca for ignorada. |
| 01 | [spec_unity_compile_validation_protocol_and_scripts](implementados/spec_unity_compile_validation_protocol_and_scripts.md) | Implementado parcial | 00 | 02-17 | Sem validacao Unity, mudancas runtime podem mascarar erro de compilacao. |
| 02 | [spec_save_schema_migration_v2](a_implementar/spec_save_schema_migration_v2.md) | A implementar | 00, 01 implementada/parcial | 03-17 | Specs posteriores podem persistir dados sem contrato de migracao. |
| 03 | [spec_inventory_slots_capacity_ui_final](a_implementar/spec_inventory_slots_capacity_ui_final.md) | A implementar | 00, 01, 02 | 04, 06, 07, 10, 12, 17 | Economy, craft e equipment podem depender de um modelo de inventory ainda provisorio. |
| 04 | [spec_farm_irrigacao_solo_planting_ui](a_implementar/spec_farm_irrigacao_solo_planting_ui.md) | A implementar | 00, 01, 02, 03 | 05, 09, 17 | World activities e stamina podem duplicar custos/regras de tools. |
| 05 | [spec_world_activities_fishing_trees_pickups_loot](a_implementar/spec_world_activities_fishing_trees_pickups_loot.md) | A implementar | 00-04 | 06, 07, 10 | Loot/economy podem ser balanceados sobre drops incompletos. |
| 06 | [spec_economy_shop_stock_pricing_ui](a_implementar/spec_economy_shop_stock_pricing_ui.md) | A implementar | 00-05 | 07, 08, 17 | Crafting e town podem assumir precificacao ou estoque inexistente. |
| 07 | [spec_crafting_queue_workstations_recipes_ui](a_implementar/spec_crafting_queue_workstations_recipes_ui.md) | A implementar | 00-06 | 10, 17 | Equipment e consumables podem criar receitas fora do modelo final. |
| 08 | [spec_town_npc_dialogue_schedule_quests](a_implementar/spec_town_npc_dialogue_schedule_quests.md) | A implementar | 00-07 | 15, 17 | Fonte de Anya e lojas podem nascer sem contratos de NPC/quest. |
| 09 | [spec_hunger_stamina_status_balance](a_implementar/spec_hunger_stamina_status_balance.md) | A implementar | 00-05 | 10, 11, 12, 14, 17 | Combat/tools podem consumir recursos com regras divergentes. |
| 10 | [spec_equipment_durability_environment_loot_runtime](a_implementar/spec_equipment_durability_environment_loot_runtime.md) | A implementar | 00-09 | 11, 12, 13, 14, 17 | Damage e enemy tuning ficam instaveis sem stats/equipment finais. |
| 11 | [spec_damage_status_elements_resistances_runtime](a_implementar/spec_damage_status_elements_resistances_runtime.md) | A implementar | 00-10 | 12, 13, 14 | Armas, spells e IA podem duplicar formulas de dano. |
| 12 | [spec_player_combat_weapons_spells_skill_actions_runtime](a_implementar/spec_player_combat_weapons_spells_skill_actions_runtime.md) | A implementar | 00-11 | 13, 14, 16, 17 | Enemy AI e skill trees podem acoplar em comandos provisorios. |
| 13 | [spec_enemy_ai_roster_bestiary_faction_locks_runtime](a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md) | A implementar | 00-12 | 14, 15, 17 | Cave generation pode distribuir inimigos sem regras finais. |
| 14 | [spec_cave_runtime_generation_checkpoints_boss_gates](a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md) | A implementar | 00-13 | 15, 17 | Entry/death pode quebrar replay se cave ainda rerollar. |
| 15 | [spec_cave_entry_death_anya_corpse_recovery](a_implementar/spec_cave_entry_death_anya_corpse_recovery.md) | A implementar | 00-14 | 16, 17 | Skill respec e UI podem criar fluxos sem falha/recovery definidos. |
| 16 | [spec_skill_trees_active_slots_respec_anya_runtime](a_implementar/spec_skill_trees_active_slots_respec_anya_runtime.md) | A implementar | 00-15 | 17 | UI final pode expor skill tree incompleta ou sem persistencia. |
| 17 | [spec_ui_ux_full_gameplay_inventory_hotbar_menus](a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md) | A implementar | 00-16 | Nenhuma | UI pode cristalizar contratos de sistemas ainda instaveis se antecipada. |

## Observacao operacional

A spec 00 antiga em `a_implementar/spec_docs_single_source_specs_refinements_reconciliation_v1.md` nao deve ser executada novamente; ela permanece apenas como ponte historica ate remocao fisica futura.

A spec 01 foi implementada parcialmente como tooling minimo. A primeira spec futura executavel passa a ser `a_implementar/spec_save_schema_migration_v2.md`, ainda exigindo refinamento antes de runtime.
