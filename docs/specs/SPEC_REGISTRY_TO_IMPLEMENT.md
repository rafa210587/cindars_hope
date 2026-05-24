# Registry de specs a implementar

Fonte unica de specs futuras: `docs/specs/a_implementar/`. A pasta raiz `specs/` foi removida e nao deve ser recriada.

A antiga spec 00 de reconciliacao documental foi reclassificada como implementada/parcial em `docs/specs/implementados/spec_docs_001_single_source_specs_refinements_reconciliation_parcial.md`. Ela nao deve ser executada novamente.

| Ordem | Spec | Status | Dependencia | Observacao |
|---|---|---|---|---|
| 02 | [spec_save_schema_migration_v2.md](a_implementar/spec_save_schema_migration_v2.md) | A implementar | 00 e 01 implementadas/parciais | Implementar migracao robusta de save com versao, backup e DTOs simples. |
| 03 | [spec_inventory_slots_capacity_ui_final.md](a_implementar/spec_inventory_slots_capacity_ui_final.md) | A implementar | 00, 01, 02 | Evoluir Dictionary<string,int> para slots reais, multiplas stacks, capacidade, drag/drop e UI. |
| 04 | [spec_farm_irrigacao_solo_planting_ui.md](a_implementar/spec_farm_irrigacao_solo_planting_ui.md) | A implementar | 00, 01, 02, 03 | Completar solo, irrigacao, hoe/watering can, planting UI e condicoes de crescimento. |
| 05 | [spec_world_activities_fishing_trees_pickups_loot.md](a_implementar/spec_world_activities_fishing_trees_pickups_loot.md) | A implementar | 00-04 | Completar pesca, arvores, pickups persistentes e loot tables de atividades. |
| 06 | [spec_economy_shop_stock_pricing_ui.md](a_implementar/spec_economy_shop_stock_pricing_ui.md) | A implementar | 00-05 | Evoluir lojas para estoque, precos, UI, NPC/local e save. |
| 07 | [spec_crafting_queue_workstations_recipes_ui.md](a_implementar/spec_crafting_queue_workstations_recipes_ui.md) | A implementar | 00-06 | Completar crafting com bancadas, fila, tempo, recipes, ingredientes e UI. |
| 08 | [spec_town_npc_dialogue_schedule_quests.md](a_implementar/spec_town_npc_dialogue_schedule_quests.md) | A implementar | 00-07 | Evoluir Town/Pip para NPCs com dialogo, agenda, lojas e quests leves. |
| 09 | [spec_hunger_stamina_status_balance.md](a_implementar/spec_hunger_stamina_status_balance.md) | A implementar | 00-05 | Integrar fome com stamina, buffs/debuffs, status e balanceamento. |
| 10 | [spec_equipment_durability_environment_loot_runtime.md](a_implementar/spec_equipment_durability_environment_loot_runtime.md) | A implementar | 00-09 | Completar equipment slots, durabilidade, resistencias ambientais e loot runtime. |
| 11 | [spec_damage_status_elements_resistances_runtime.md](a_implementar/spec_damage_status_elements_resistances_runtime.md) | A implementar | 00-10 | Completar pipeline de dano, elementos, resistencias e status effects. |
| 12 | [spec_player_combat_weapons_spells_skill_actions_runtime.md](a_implementar/spec_player_combat_weapons_spells_skill_actions_runtime.md) | A implementar | 00-11 | Substituir punch MVP por armas, magias, skill actions, mana/stamina e active slots. |
| 13 | [spec_enemy_ai_roster_bestiary_faction_locks_runtime.md](a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md) | A implementar | 00-12 | Completar IA, roster 40+, bestiario, faction locks, ecologia e XP. |
| 14 | [spec_cave_runtime_generation_checkpoints_boss_gates.md](a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md) | A implementar | 00-13 | Completar cave runtime, snapshots, checkpoints, boss gates, confinement e materializacao. |
| 15 | [spec_cave_entry_death_anya_corpse_recovery.md](a_implementar/spec_cave_entry_death_anya_corpse_recovery.md) | A implementar | 00-14 | Completar entrada cave, loadout, morte, Fonte de Anya, corpse recovery e penalidades. |
| 16 | [spec_skill_trees_active_slots_respec_anya_runtime.md](a_implementar/spec_skill_trees_active_slots_respec_anya_runtime.md) | A implementar | 00-15 | Completar skill trees, active slots, capstones, save/load e respec na Fonte de Anya. |
| 17 | [spec_ui_ux_full_gameplay_inventory_hotbar_menus.md](a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md) | A implementar | 00-16 | Completar HUD, hotbar, inventory, equipment, crafting, skills, shop, cave/death e menus. |
