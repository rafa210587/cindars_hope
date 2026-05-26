# Registry de specs a implementar

Fonte unica de specs futuras: `docs/specs/a_implementar/`. A pasta raiz `specs/` foi removida e nao deve ser recriada.

A antiga spec 00 de reconciliacao documental foi reclassificada como implementada/parcial em `docs/specs/implementados/spec_docs_001_single_source_specs_refinements_reconciliation_parcial.md`. Ela nao deve ser executada novamente.

| Ordem | Spec | Status | Dependencia | Observacao |
|---|---|---|---|---|
| 12 | [spec_player_combat_weapons_spells_skill_actions_runtime.md](a_implementar/spec_player_combat_weapons_spells_skill_actions_runtime.md) | A implementar | 00-11 | PlayerCombatController Q/E, melee, dodge, bow, spells, active slots. Bloqueia: 13, 14, 16, 17. |
| 13 | [spec_enemy_ai_roster_bestiary_faction_locks_runtime.md](a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md) | Implementado parcial | 00-12 | Enemy chase/patrol AI completo. Pendente: ranged enemies, faction locks, bestiary UI. Bloqueia: 14, 15, 17. |
| 14 | [spec_cave_runtime_generation_checkpoints_boss_gates.md](a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md) | Implementado parcial | 00-13 | Cave generation/runtime completo. Pendente: stable run guarantee, boss gates validation, confinement hardening. Bloqueia: 15, 17. |
| 15 | [spec_cave_entry_death_anya_corpse_recovery.md](a_implementar/spec_cave_entry_death_anya_corpse_recovery.md) | Implementado parcial | 00-14 | Cave entry/death/respawn. Pendente: Anya NPC entity, recovery UI, tentativas. Bloqueia: 16, 17. |
| 16 | [spec_skill_trees_active_slots_respec_anya_runtime.md](a_implementar/spec_skill_trees_active_slots_respec_anya_runtime.md) | Implementado parcial | 00-15 | Progression base. Pendente: skill tree UI, active slots wiring, respec mechanics. Bloqueia: 17. |
| 17 | [spec_ui_ux_full_gameplay_inventory_hotbar_menus.md](a_implementar/spec_ui_ux_full_gameplay_inventory_hotbar_menus.md) | Implementacao parcial - MVP gameplay em codigo | 00-16 implementadas/parciais | Shops/sell/inventory/equipment/attributes/skills e input modal MVP entregues; Canvas final, pause/options, cave/corpse/toasts e Play Mode ainda pendentes. |
