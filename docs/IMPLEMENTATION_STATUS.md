# Cindar's Hope - Implementation Status

> Status: tracking reorganizado.
> Historico anterior: `docs_old/IMPLEMENTATION_STATUS.md`.

## Resumo

| Area | Status | Spec nova |
|---|---|---|
| Core/event bus/bootstrap | Implementado parcial | [spec_core_001](specs/implementados/spec_core_001_event_bus_e_eventos_base.md), [spec_core_002](specs/implementados/spec_core_002_bootstrap_managers_e_runtime_references.md) |
| Data/IDs/registries | Implementado | [spec_data_001](specs/implementados/spec_data_001_ids_registries_e_scriptableobjects.md) |
| Save/load JSON cross-scene | Implementado parcial | [spec_save_001](specs/implementados/spec_save_001_json_save_load_cross_scene.md) |
| Inventory/itens/gold/stacks | Implementado | [spec_inventory_001](specs/implementados/spec_inventory_001_inventario_itens_gold_e_stacks.md) |
| Farm loop | Implementado parcial | [spec_farm_001](specs/implementados/spec_farm_001_farm_scene_movimento_interacao.md), [spec_farm_002](specs/implementados/spec_farm_002_plots_seeds_growth_harvest.md), [spec_farm_003](specs/implementados/spec_farm_003_arvores_pesca_pickups_world_activities.md) |
| World pickups persistentes | Implementado parcial | [spec_world_001](specs/implementados/spec_world_001_pickups_persistentes_save_load.md) |
| Economy/hunger/crafting/town | Implementado parcial | [spec_economy_001](specs/implementados/spec_economy_001_compra_venda_gold_e_sellables.md), [spec_hunger_001](specs/implementados/spec_hunger_001_fome_comida_e_hp_por_fome.md), [spec_craft_001](specs/implementados/spec_craft_001_crafting_mvp.md), [spec_town_001](specs/implementados/spec_town_001_town_scene_portais_npc_pip_comercio.md) |
| Combat/damage/enemy stats | Implementado parcial | [spec_combat_001](specs/implementados/spec_combat_001_slime_melee_contact_damage_drops.md), [spec_combat_002](specs/implementados/spec_combat_002_enemy_data_driven_stats.md), [spec_damage_001](specs/implementados/spec_damage_001_damage_formula_mvp.md) |
| UI/tools/hotbar/progression debug | Implementado parcial | [spec_ui_001](specs/implementados/spec_ui_001_debug_hud_e_feedback_mvp.md), [spec_ui_002](specs/implementados/spec_ui_002_hud_tools_hotbar_progression_debug.md), [spec_tools_001](specs/implementados/spec_tools_001_tools_equipment_hotbar_parcial.md), [spec_progression_001](specs/implementados/spec_progression_001_xp_level_atributos_parcial.md) |
| Cave runtime/procedural/stable run/boss gates | Implementado em codigo - validacao Unity pendente | [spec_cave_001](specs/implementados/spec_cave_001_cave_scene_portal_e_runtime_basico.md), [spec_cave_002](specs/implementados/spec_cave_002_procedural_contracts_resources_parcial.md), [spec_cave_003](specs/implementados/spec_cave_003_stable_run_snapshots_replay_parcial.md), [spec_cave_004](specs/implementados/spec_cave_004_boss_gates_checkpoints_confinement_parcial.md), [spec_cave_005](specs/implementados/spec_cave_005_visual_runtime_camera_enemy_visuals.md), [spec_cave_006](specs/implementados/spec_cave_006_spawn_anchor_safe_positioning.md), [spec_cave_007](specs/implementados/spec_cave_007_snapshot_replay_full_layout_hardening.md), [spec_cave_008](specs/implementados/spec_cave_008_debug_skip_confinement_wall_distance_hardening.md) |
| Validation/process | Implementado parcial | [spec_validation_001](specs/implementados/spec_validation_001_scene_generators_validators.md), [spec_repo_001](specs/implementados/spec_repo_001_pr099_reconciliation_audit.md) |

## Capacidades jogáveis hoje

- Iniciar na FarmScene.
- Mover jogador.
- Interagir com objetos.
- Plantar seeds.
- Avançar dia.
- Crescer e colher crops.
- Ver HUD debug.
- Comprar/vender.
- Fome e comida.
- Save/load JSON.
- Persistir plots, árvores, pickups, inventário, ouro, fome, HP, dia e posição.
- Transitar Farm ↔ Town.
- Interagir com Pip placeholder.
- Craftar madeira processada.
- Cortar árvore.
- Pescar peixe comum.
- Coletar pickup persistente.
- Entrar/sair da CaveScene.
- Enfrentar Slime básico.
- Receber drops básicos.
- Usar base procedural de cave parcialmente implementada conforme specs cave.
- Usar checkpoints/boss gates/debug cave conforme specs cave, com validação Unity pendente.

## Blocos implementados por wave

| Bloco | Status | Spec/ref |
|---|---|---|
| PR-001 a PR-012 Core Foundation | Implementado | `docs/refinements/implementados/ref_core_foundation_pr001_012.md` |
| PR-013 a PR-017 Farm Loop | Implementado MVP | `docs/refinements/implementados/ref_farm_loop_pr013_017.md` |
| PR-018 a PR-024 Economy/Hunger/HUD | Implementado parcial | `docs/refinements/implementados/ref_economy_hunger_hud_pr018_024.md` |
| PR-025 a PR-030 Save/Load | Implementado parcial | `docs/refinements/implementados/ref_save_load_pr025_030.md` |
| PR-031 a PR-045 World/Shop/Hardening | Implementado parcial | `docs/refinements/implementados/ref_world_shop_hardening_pr031_045.md` |
| PR-046 a PR-052 Crafting | Implementado MVP | `docs/refinements/implementados/ref_crafting_pr046_052.md` |
| PR-053 a PR-063 Town | Implementado MVP | `docs/refinements/implementados/ref_town_pr053_063.md` |
| PR-065 Cross-scene hardening | Implementado parcial | `docs/refinements/implementados/ref_cross_scene_hardening_pr065.md` |
| PR-101 a PR-130 Tools/Equipment/Hotbar/Progression/Damage | Implementado parcial | `docs/refinements/implementados/ref_tools_equipment_hotbar_progression_damage_pr101_130.md` |
| PR-131 HUD/tools/progression validation | Implementado/documentado | `docs/refinements/implementados/ref_pr131_validacao_hud_tools_progression.md` |
| PR-140 Cave procedural contracts | Implementado parcial | `docs/specs/implementados/spec_cave_002_procedural_contracts_resources_parcial.md` |
| PR-141 a PR-153 Cave procedural runtime | Implementado parcial | `docs/refinements/implementados/ref_cave_procedural_runtime_pr141_153.md` |
| PR-170 a PR-192 Stable run/replay | Código implementado, Unity pendente | `docs/refinements/implementados/ref_cave_stable_run_replay_pr170_192.md` |
| PR-193 a PR-202 Boss gates/checkpoints/confinement | Código implementado, Unity pendente | `docs/refinements/implementados/ref_cave_boss_gates_checkpoints_confinement_pr193_202.md` |
| Cave visual/spawn/snapshot/confinement fixes | Código implementado, Unity pendente | `docs/refinements/implementados/ref_fix_cave_procedural_visual_handoff.md` |

## Specs futuras rastreadas

Fonte: `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md`.

Este arquivo deve permanecer curto; detalhes ficam nos registries, specs e refinements.
