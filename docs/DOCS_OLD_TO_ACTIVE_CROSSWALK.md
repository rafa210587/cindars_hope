# DOCS OLD → ACTIVE CROSSWALK

> Status: mapa de preservação documental.
> Objetivo: garantir que `docs_old/` continue preservado e que a documentação ativa tenha referência explícita ao que foi migrado, absorvido ou mantido como histórico.

## Legenda

| Status | Significado |
|---|---|
| Ativo copiado | Arquivo antigo foi copiado para uma pasta ativa em `docs/`. |
| Absorvido em spec implementada | Conteúdo antigo virou spec em `docs/specs/implementados/`. |
| Absorvido em spec futura | Conteúdo antigo virou spec em `docs/specs/a_implementar/`. |
| Absorvido em refinement implementado | Conteúdo antigo virou refinement em `docs/refinements/implementados/`. |
| Absorvido em refinement futuro | Conteúdo antigo virou refinement em `docs/refinements/a_implementar/`. |
| Histórico preservado | Mantido apenas em `docs_old/`, com referência ativa quando necessário. |

## Crosswalk principal

| Arquivo em docs_old | Destino ativo | Tipo de migração | Status | Observação |
|---|---|---|---|---|
| `docs_old/GDD_v2.6.md` | `docs/design/GDD_v2.6.md` | cópia ativa | Ativo copiado | Design principal do jogo. |
| `docs_old/GDD_v2.7_FASE9C_DELTA.md` | `docs/design/GDD_v2.7_FASE9C_DELTA.md` | cópia ativa | Ativo copiado | Delta de design da FASE9C. |
| `docs_old/CHANGELOG_ATUALIZACAO_v2.6.md` | `docs/design/CHANGELOG_ATUALIZACAO_v2.6.md` | cópia ativa | Ativo copiado | Changelog de design. |
| `docs_old/ARCH_fase4_v2.2.md` | `docs/architecture/ARCH_fase4_v2.2.md`; `docs/specs/implementados/spec_core_002_bootstrap_managers_e_runtime_references.md` | cópia ativa + absorção | Ativo copiado | Arquitetura base e runtime. |
| `docs_old/ARCH_fase4_v2.3_FASE9C_DELTA.md` | `docs/architecture/ARCH_fase4_v2.3_FASE9C_DELTA.md` | cópia ativa | Ativo copiado | Delta arquitetural FASE9C. |
| `docs_old/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md` | `docs/architecture/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.0.md`; `docs/specs/implementados/spec_core_001_event_bus_e_eventos_base.md`; `docs/specs/implementados/spec_data_001_ids_registries_e_scriptableobjects.md`; `docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md` | cópia ativa + absorção | Ativo copiado | Eventos, IDs, registries e save. |
| `docs_old/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.1_FASE9C_DELTA.md` | `docs/architecture/CORE_CONTRACTS_EVENTS_SAVE_IDS_v1.1_FASE9C_DELTA.md` | cópia ativa | Ativo copiado | Delta FASE9C dos contratos core. |
| `docs_old/FASE5_ambiente_v1.2.md` | `docs/operations/FASE5_ambiente_v1.2.md` | cópia ativa | Ativo copiado | Setup de ambiente. |
| `docs_old/LLM_HANDOFF_INSTRUCTIONS.md` | `docs/operations/LLM_HANDOFF_INSTRUCTIONS.md` | cópia ativa | Ativo copiado | Instruções para agentes e handoff. |
| `docs_old/SPECKIT_DRIFT_CONTROL_v1.0.md` | `docs/operations/SPECKIT_DRIFT_CONTROL_v1.0.md` | cópia ativa | Ativo copiado | Controle de drift SpecKit. |
| `docs_old/SPEC_EVOLUTION_POLICY_v1.0.md` | `docs/operations/SPEC_EVOLUTION_POLICY_v1.0.md` | cópia ativa | Ativo copiado | Política de evolução de specs. |
| `docs_old/SPRITE_PIPELINE_AI_ASEPRITE_v1.0.md` | `docs/operations/SPRITE_PIPELINE_AI_ASEPRITE_v1.0.md` | cópia ativa | Ativo copiado | Pipeline de sprites e arte. |
| `docs_old/PRE_CODEX_CORRECTIONS.md` | `docs_old/PRE_CODEX_CORRECTIONS.md` | referência histórica | Histórico preservado | Mantido no histórico para auditoria de correções pré-Codex. |
| `docs_old/VALIDATION_FASE9F_IMPORTS_STRUCTURE.md` | `docs/validation/VALIDATION_FASE9F_IMPORTS_STRUCTURE.md` | cópia ativa | Ativo copiado | Validação estrutural/imports FASE9F. |
| `docs_old/FASE6_INDEX_global_v1.2.md` | `docs/backlog/FASE6_INDEX_global_v1.2.md` | cópia ativa | Ativo copiado | Backlog global base. |
| `docs_old/FASE6_INDEX_global_v1.3_FASE9C_DELTA.md` | `docs/backlog/FASE6_INDEX_global_v1.3_FASE9C_DELTA.md` | cópia ativa | Ativo copiado | Delta FASE9C do backlog. |
| `docs_old/FASE6_FARM_backlog_v1.2.md` | `docs/backlog/FASE6_FARM_backlog_v1.2.md` | cópia ativa | Ativo copiado | Backlog FARM. |
| `docs_old/NEXT_WAVES_ROADMAP_v1.0.md` | `docs/roadmap/NEXT_WAVES_ROADMAP_v1.0.md` | cópia ativa | Ativo copiado | Roadmap ativo. |
| `docs_old/NEXT_WAVES_ROADMAP_v1.1_FASE9C_DELTA.md` | `docs/roadmap/NEXT_WAVES_ROADMAP_v1.1_FASE9C_DELTA.md` | cópia ativa | Ativo copiado | Delta FASE9C do roadmap. |
| `docs_old/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md` | `docs/roadmap/FASE9F_CAVE_STABLE_RUN_ROADMAP_PR170_192.md` | cópia ativa | Ativo copiado | Roadmap stable run/replay. |
| `docs_old/FUTURE_IDEAS_TODO_v1.0.md` | `docs/backlog/FUTURE_IDEAS_TODO_v1.0.md`; `docs/specs/a_implementar/spec_future_ideas_todo.md`; `docs/refinements/a_implementar/ref_future_ideas_todo.md` | cópia ativa + absorção | Ativo copiado | Ideias futuras preservadas sem compromisso imediato. |

## Crosswalk de fases, specs e handoffs

| Arquivo em docs_old | Destino ativo | Tipo de migração | Status | Observação |
|---|---|---|---|---|
| `docs_old/FASE7_SPEC_MVP_FARM_v2.2.md` | `docs/specs/implementados/spec_farm_001_farm_scene_movimento_interacao.md`; `docs/specs/implementados/spec_farm_002_plots_seeds_growth_harvest.md`; `docs/specs/implementados/spec_inventory_001_inventario_itens_gold_e_stacks.md` | absorção | Absorvido em spec implementada | MVP Farm normalizado. |
| `docs_old/FASE8_EXECUTION_PLAN_CODEX_v1.0.md` | `docs/refinements/implementados/ref_core_foundation_pr001_012.md`; `docs/specs/implementados/spec_validation_001_scene_generators_validators.md` | absorção | Absorvido em refinement implementado | Plano de execução e validação. |
| `docs_old/FASE8_SYNC_PR003_ASSETS_v1.0.md` | `docs/refinements/implementados/ref_core_foundation_pr001_012.md` | absorção | Absorvido em refinement implementado | Sincronização de assets do início do projeto. |
| `docs_old/FASE9A_HANDOFF_FARM_TOWN_SAVE_v1.0.md` | `docs/specs/implementados/spec_town_001_town_scene_portais_npc_pip_comercio.md`; `docs/specs/implementados/spec_craft_001_crafting_mvp.md`; `docs/specs/implementados/spec_economy_001_compra_venda_gold_e_sellables.md`; `docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md` | absorção | Absorvido em spec implementada | Town, crafting, economy e save. |
| `docs_old/FASE9B_CAVE_COMBAT_MVP_v1.0.md` | `docs/specs/implementados/spec_cave_001_cave_scene_portal_e_runtime_basico.md`; `docs/specs/implementados/spec_combat_001_slime_melee_contact_damage_drops.md`; `docs/specs/implementados/spec_damage_001_damage_formula_mvp.md` | absorção | Absorvido em spec implementada | Cave/combat MVP. |
| `docs_old/FASE9B3_ENEMY_DATA_DRIVEN_STATS_v1.0.md` | `docs/specs/implementados/spec_combat_002_enemy_data_driven_stats.md` | absorção | Absorvido em spec implementada | Enemy stats data-driven. |
| `docs_old/FASE9B_DOC_SYNC_MACRO_ADDENDUM_v1.0.md` | `docs_old/FASE9B_DOC_SYNC_MACRO_ADDENDUM_v1.0.md`; `docs/DOCS_REORGANIZATION_HANDOFF.md` | referência ativa | Histórico preservado | Contexto de sincronização documental. |
| `docs_old/FASE9C_HANDOFF_DOCUMENTAL_v1.0.md` | `docs/DOCS_REORGANIZATION_HANDOFF.md`; `docs/refinements/a_implementar/ref_fase9c_tools_farm_combat_refinement_remaining.md` | absorção | Absorvido em refinement futuro | Transição documental FASE9C. |
| `docs_old/FASE9C_PLAYER_EQUIPMENT_ITEMS_COMBAT_SPEC_v1.0.md` | `docs/specs/a_implementar/spec_fase9c_player_equipment_items_combat_remaining.md`; `docs/refinements/a_implementar/ref_fase9c_player_equipment_items_combat_remaining.md` | absorção | Absorvido em spec futura | Restante de equipment/items/combat. |
| `docs_old/FASE9C_TOOLS_FARM_COMBAT_REFINEMENT_v1.0.md` | `docs/specs/a_implementar/spec_fase9c_tools_farm_combat_refinement_remaining.md`; `docs/refinements/a_implementar/ref_fase9c_tools_farm_combat_refinement_remaining.md`; `docs/specs/implementados/spec_tools_001_tools_equipment_hotbar_parcial.md`; `docs/specs/implementados/spec_farm_003_arvores_pesca_pickups_world_activities.md` | absorção mista | Absorvido em spec futura | Implementado parcial separado do restante futuro. |
| `docs_old/FASE9D_ENEMY_ACTIONS_AI_COMBAT_SPEC_v1.0.md` | `docs/specs/a_implementar/spec_fase9d_enemy_actions_ai_combat.md`; `docs/refinements/a_implementar/ref_fase9d_enemy_actions_ai_combat.md` | absorção | Absorvido em spec futura | IA, ações e combate de inimigos. |
| `docs_old/FASE9D_ENEMY_ARCHITECTURE_40_MONSTERS_v1.1.md` | `docs/specs/a_implementar/spec_fase9d_enemy_architecture_40_monsters.md`; `docs/refinements/a_implementar/ref_fase9d_enemy_architecture_40_monsters.md` | absorção | Absorvido em spec futura | Arquitetura data-driven para 40+ monstros. |
| `docs_old/FASE9E_UI_HOTBAR_INVENTORY_EQUIPMENT_SPEC_v1.0.md` | `docs/specs/a_implementar/spec_fase9e_ui_hotbar_inventory_equipment_final.md`; `docs/refinements/a_implementar/ref_fase9e_ui_hotbar_inventory_equipment_final.md`; `docs/specs/implementados/spec_ui_001_debug_hud_e_feedback_mvp.md`; `docs/specs/implementados/spec_tools_001_tools_equipment_hotbar_parcial.md` | absorção mista | Absorvido em spec futura | Debug/parcial implementado e UI final futura. |
| `docs_old/FASE9E_DAMAGE_STATUS_FORMULA_SPEC_v1.0.md` | `docs/specs/a_implementar/spec_fase9e_damage_status_elements_complete.md`; `docs/refinements/a_implementar/ref_fase9e_damage_status_elements_complete.md`; `docs/specs/implementados/spec_damage_001_damage_formula_mvp.md` | absorção mista | Absorvido em spec futura | Fórmula MVP implementada; status/elementos futuros. |
| `docs_old/FASE9E_ITEM_TAXONOMY_IDS_SPEC_v1.0.md` | `docs/specs/a_implementar/spec_fase9e_item_taxonomy_ids.md`; `docs/refinements/a_implementar/ref_fase9e_item_taxonomy_ids.md`; `docs/specs/implementados/spec_data_001_ids_registries_e_scriptableobjects.md` | absorção mista | Absorvido em spec futura | IDs implementados e taxonomia futura. |
| `docs_old/FASE9E_ITEM_EXAMPLES_VARIATIONS_v1.0.md` | `docs/specs/a_implementar/spec_fase9e_item_examples_variations.md`; `docs/refinements/a_implementar/ref_fase9e_item_examples_variations.md` | absorção | Absorvido em spec futura | Exemplos e variações de itens. |
| `docs_old/FASE9E_SAVE_SCHEMA_MIGRATION_SPEC_v1.0.md` | `docs/specs/a_implementar/spec_fase9e_save_schema_migration.md`; `docs/refinements/a_implementar/ref_fase9e_save_schema_migration.md`; `docs/specs/implementados/spec_save_001_json_save_load_cross_scene.md` | absorção mista | Absorvido em spec futura | Save atual e migration futura. |
| `docs_old/FASE9E_PLAYER_LEVEL_UP_PROGRESSION_SPEC_v1.0.md` | `docs/specs/a_implementar/spec_fase9e_player_level_up_progression.md`; `docs/refinements/a_implementar/ref_fase9e_player_level_up_progression.md`; `docs/specs/implementados/spec_progression_001_xp_level_atributos_parcial.md` | absorção mista | Absorvido em spec futura | Progression parcial e evolução futura. |
| `docs_old/FASE9F_CAVE_RESOURCES_ENCOUNTERS_SPEC_v1.0.md` | `docs/specs/a_implementar/spec_fase9f_cave_resources_encounters_complete.md`; `docs/refinements/a_implementar/ref_fase9f_cave_resources_encounters_complete.md`; `docs/specs/implementados/spec_cave_002_procedural_contracts_resources_parcial.md` | absorção mista | Absorvido em spec futura | Cave procedural parcial e complete futuro. |
| `docs_old/FASE9F_CAVE_REPLAY_CONTRACTS_v1.0.md` | `docs/specs/implementados/spec_cave_003_stable_run_snapshots_replay_parcial.md`; `docs/refinements/implementados/ref_cave_stable_run_replay_pr170_192.md` | absorção | Absorvido em spec implementada | Stable run/replay. |
| `docs_old/FASE9F_CAVE_REPLAY_HANDOFF_PR170_192.md` | `docs/refinements/implementados/ref_cave_stable_run_replay_pr170_192.md`; `docs/refinements/implementados/ref_pr170_192_cave_stable_run_progression_handoff.md` | absorção | Absorvido em refinement implementado | Handoff PR170-192. |
| `docs_old/FASE9G_CAVE_BESTIARY_FACTION_LOCKS_PORTAL_ECOLOGY_SPEC_v1.0.md` | `docs/specs/a_implementar/spec_fase9g_cave_bestiary_faction_locks.md`; `docs/refinements/a_implementar/ref_fase9g_bestiary_faction_locks_portal_ecology.md`; `docs/specs/implementados/spec_cave_004_boss_gates_checkpoints_confinement_parcial.md` | absorção mista | Absorvido em spec futura | Bestiário/faction locks futuro e boss gates parciais. |
| `docs_old/FASE9H_CAVE_LOOT_CRAFTING_EQUIPMENT_PROGRESSION_SPEC_v1.0.md` | `docs/specs/a_implementar/spec_fase9h_loot_crafting_equipment_durability_environment.md`; `docs/refinements/a_implementar/ref_fase9h_loot_crafting_equipment_durability_environment.md` | absorção | Absorvido em spec futura | Loot, crafting, equipment, durability, environment. |
| `docs_old/FASE9I_PLAYER_COMBAT_WEAPONS_MAGIC_SKILL_TREES_SPEC_v1.0.md` | `docs/specs/a_implementar/spec_fase9i_player_combat_weapons_magic_skill_actions.md`; `docs/refinements/a_implementar/ref_fase9i_player_combat_weapons_magic_skill_actions.md` | absorção | Absorvido em spec futura | Combate do jogador, armas, magia e skill actions. |
| `docs_old/FASE9J_CAVE_RUN_ENTRY_LOADOUT_HUD_AND_FAILURE_FLOW_SPEC_v1.0.md` | `docs/specs/a_implementar/spec_fase9j_cave_entry_loadout_death_anya_corpse.md`; `docs/refinements/a_implementar/ref_fase9j_cave_entry_loadout_death_anya_corpse.md` | absorção | Absorvido em spec futura | Entrada da cave, morte, Fonte de Anya e corpse recovery. |
| `docs_old/FASE9K_SKILL_TREES_FULL_NODE_AND_UNLOCK_SPEC_v1.0.md` | `docs/specs/a_implementar/spec_fase9k_skill_trees_nodes_active_slots_respec.md`; `docs/refinements/a_implementar/ref_fase9k_skill_trees_nodes_active_slots_respec.md` | absorção | Absorvido em spec futura | Skill trees, nodes, active slots, capstones e respec. |

## Crosswalk de amendments

| Arquivo em docs_old | Destino ativo | Tipo de migração | Status | Observação |
|---|---|---|---|---|
| `docs_old/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md` | `docs/amendments/FASE9F_CAVE_STABLE_RUN_AND_REPLAY_AMENDMENT_v1.0.md`; `docs/specs/implementados/spec_cave_003_stable_run_snapshots_replay_parcial.md` | cópia ativa + absorção | Ativo copiado | Amendment stable run/replay. |
| `docs_old/amendments/FASE9G_AMENDMENT_ENEMY_COMBAT_ROLES_AI_STATUS_v1.1.md` | `docs/amendments/FASE9G_AMENDMENT_ENEMY_COMBAT_ROLES_AI_STATUS_v1.1.md`; `docs/specs/a_implementar/spec_fase9g_enemy_combat_roles_ai_status_amendment.md`; `docs/refinements/a_implementar/ref_fase9g_enemy_combat_roles_ai_status_amendment.md` | cópia ativa + absorção | Ativo copiado | Amendment combat roles, AI e status dos inimigos. |

## Crosswalk de audits, fixes e handoffs

| Arquivo em docs_old | Destino ativo | Tipo de migração | Status | Observação |
|---|---|---|---|---|
| `docs_old/audits/PR100_POST_PR099_REPO_AUDIT.md` | `docs/refinements/implementados/ref_pr100_post_pr099_repo_audit.md`; `docs/specs/implementados/spec_repo_001_pr099_reconciliation_audit.md` | absorção | Absorvido em refinement implementado | Auditoria pós PR099. |
| `docs_old/audits/PR101_PR099_BRANCH_RECONCILIATION.md` | `docs/refinements/implementados/ref_pr101_pr099_branch_reconciliation.md`; `docs/specs/implementados/spec_repo_001_pr099_reconciliation_audit.md` | absorção | Absorvido em refinement implementado | Reconciliação pós PR099. |
| `docs_old/audits/PR116_ITEM_ID_AUDIT.md` | `docs/refinements/implementados/ref_pr116_item_id_audit.md`; `docs/specs/implementados/spec_data_001_ids_registries_e_scriptableobjects.md` | absorção | Absorvido em refinement implementado | Auditoria de IDs. |
| `docs_old/audits/PR130_RECONCILIACAO_HANDOFF.md` | `docs/refinements/implementados/ref_pr130_reconciliacao_handoff.md` | absorção | Absorvido em refinement implementado | Handoff de reconciliação PR130. |
| `docs_old/audits/PR131_VALIDACAO_HUD_TOOLS_PROGRESSION.md` | `docs/refinements/implementados/ref_pr131_validacao_hud_tools_progression.md`; `docs/specs/implementados/spec_ui_002_hud_tools_hotbar_progression_debug.md` | absorção | Absorvido em refinement implementado | Validação HUD/tools/progression. |
| `docs_old/audits/PR153_CAVE_PROCEDURAL_HANDOFF.md` | `docs/refinements/implementados/ref_pr153_cave_procedural_handoff.md`; `docs/specs/implementados/spec_cave_002_procedural_contracts_resources_parcial.md` | absorção | Absorvido em refinement implementado | Handoff cave procedural. |
| `docs_old/audits/PR170_192_CAVE_STABLE_RUN_PRE_IMPLEMENTATION_AUDIT.md` | `docs/refinements/implementados/ref_pr170_192_cave_stable_run_pre_implementation_audit.md`; `docs/specs/implementados/spec_cave_003_stable_run_snapshots_replay_parcial.md` | absorção | Absorvido em refinement implementado | Auditoria pré-implementação stable run. |
| `docs_old/audits/PR170_192_CAVE_STABLE_RUN_PROGRESSION_HANDOFF.md` | `docs/refinements/implementados/ref_pr170_192_cave_stable_run_progression_handoff.md`; `docs/specs/implementados/spec_cave_003_stable_run_snapshots_replay_parcial.md` | absorção | Absorvido em refinement implementado | Handoff stable run/replay. |
| `docs_old/audits/FASE9F-B_IMPLEMENTATION_SUMMARY_v1.0.md` | `docs/refinements/implementados/ref_fase9f_b_implementation_summary.md` | absorção | Absorvido em refinement implementado | Summary FASE9F-B. |
| `docs_old/audits/MARCO0_FASE9F-B_CAVE_PROCEDURAL_REAL_LOOP_AUDIT.md` | `docs/refinements/implementados/ref_fase9f_b_cave_procedural_real_loop_audit.md` | absorção | Absorvido em refinement implementado | Audit do real loop cave procedural. |
| `docs_old/audits/FIX_CAVE_PROCEDURAL_VISUAL_HANDOFF.md` | `docs/refinements/implementados/ref_fix_cave_procedural_visual_handoff.md`; `docs/specs/implementados/spec_cave_005_visual_runtime_camera_enemy_visuals.md` | absorção | Absorvido em refinement implementado | Visual runtime/camera/enemy visuals. |
| `docs_old/audits/FIX_CAVE_SPAWN_ANCHOR_SAFE_POSITIONING.md` | `docs/refinements/implementados/ref_fix_cave_spawn_anchor_safe_positioning.md`; `docs/specs/implementados/spec_cave_006_spawn_anchor_safe_positioning.md` | absorção | Absorvido em refinement implementado | Spawn anchor seguro. |
| `docs_old/audits/FIX_CAVE_SNAPSHOT_REPLAY_FULL_LAYOUT.md` | `docs/refinements/implementados/ref_fix_cave_snapshot_replay_full_layout.md`; `docs/specs/implementados/spec_cave_007_snapshot_replay_full_layout_hardening.md` | absorção | Absorvido em refinement implementado | Snapshot replay full layout. |
| `docs_old/audits/FIX_CAVE_DEBUG_SKIP_AND_CONFINEMENT_TOLERANCE.md` | `docs/refinements/implementados/ref_fix_cave_debug_skip_and_confinement_tolerance.md`; `docs/specs/implementados/spec_cave_008_debug_skip_confinement_wall_distance_hardening.md` | absorção | Absorvido em refinement implementado | Debug skip e tolerância de confinamento. |
| `docs_old/audits/FIX_CAVE_BOSS_REGISTRY_INJECTION_AND_CONFINEMENT_RESET.md` | `docs/refinements/implementados/ref_fix_cave_boss_registry_injection_and_confinement_reset.md`; `docs/specs/implementados/spec_cave_008_debug_skip_confinement_wall_distance_hardening.md` | absorção | Absorvido em refinement implementado | Boss registry injection e confinement reset. |
| `docs_old/audits/FIX_CAVE_WALL_DISTANCE_BOSS_POSITION_HUD_GATE.md` | `docs/refinements/implementados/ref_fix_cave_wall_distance_boss_position_hud_gate.md`; `docs/specs/implementados/spec_cave_008_debug_skip_confinement_wall_distance_hardening.md` | absorção | Absorvido em refinement implementado | Wall distance, boss position e HUD gate. |
| `docs_old/audits/FIX_PR193_202_BOSS_GATE_CHECKPOINT_CONFINEMENT_DEBUG_SKIP_AUDIT.md` | `docs/refinements/implementados/ref_fix_pr193_202_boss_gate_checkpoint_confinement_debug_skip.md`; `docs/specs/implementados/spec_cave_004_boss_gates_checkpoints_confinement_parcial.md`; `docs/specs/implementados/spec_cave_008_debug_skip_confinement_wall_distance_hardening.md` | absorção | Absorvido em refinement implementado | Boss gate/checkpoint/confinement/debug skip. |
| `docs_old/audits/FASE9G_AMENDMENT_ENEMY_COMBAT_ROLES_AI_STATUS_HANDOFF.md` | `docs/refinements/implementados/ref_fase9g_enemy_combat_roles_ai_status_handoff.md`; `docs/specs/a_implementar/spec_fase9g_enemy_combat_roles_ai_status_amendment.md` | absorção | Absorvido em refinement implementado | Handoff enemy combat roles/AI/status. |
| `docs_old/audits/FASE9H_CAVE_LOOT_CRAFTING_EQUIPMENT_PROGRESSION_HANDOFF.md` | `docs/refinements/implementados/ref_fase9h_cave_loot_crafting_equipment_progression_handoff.md`; `docs/specs/a_implementar/spec_fase9h_loot_crafting_equipment_durability_environment.md` | absorção | Absorvido em refinement implementado | Handoff FASE9H. |
| `docs_old/audits/FASE9I_PLAYER_COMBAT_WEAPONS_MAGIC_SKILL_TREES_HANDOFF.md` | `docs/refinements/implementados/ref_fase9i_player_combat_weapons_magic_skill_trees_handoff.md`; `docs/specs/a_implementar/spec_fase9i_player_combat_weapons_magic_skill_actions.md` | absorção | Absorvido em refinement implementado | Handoff FASE9I. |
| `docs_old/audits/FASE9J_CAVE_RUN_ENTRY_LOADOUT_HUD_AND_FAILURE_FLOW_HANDOFF.md` | `docs/refinements/implementados/ref_fase9j_cave_run_entry_loadout_hud_failure_flow_handoff.md`; `docs/specs/a_implementar/spec_fase9j_cave_entry_loadout_death_anya_corpse.md` | absorção | Absorvido em refinement implementado | Handoff FASE9J. |
| `docs_old/audits/FASE9K_SKILL_TREES_FULL_NODE_AND_UNLOCK_HANDOFF.md` | `docs/refinements/implementados/ref_fase9k_skill_trees_full_node_unlock_handoff.md`; `docs/specs/a_implementar/spec_fase9k_skill_trees_nodes_active_slots_respec.md` | absorção | Absorvido em refinement implementado | Handoff FASE9K. |

## Crosswalk de validation, logs e históricos preservados

| Arquivo em docs_old | Destino ativo | Tipo de migração | Status | Observação |
|---|---|---|---|---|
| `docs_old/validation/SMOKE_TEST_FARM_TOWN_CAVE_MVP.md` | `docs/validation/SMOKE_TEST_FARM_TOWN_CAVE_MVP.md` | cópia ativa | Ativo copiado | Smoke test Farm/Town/Cave. |
| `docs_old/validation/SMOKE_TEST_CAVE_PROCEDURAL_VISUAL.md` | `docs/validation/SMOKE_TEST_CAVE_PROCEDURAL_VISUAL.md` | cópia ativa | Ativo copiado | Smoke visual cave procedural. |
| `docs_old/logs/PROJECT_LOG_ARCHIVE_2026-05-18_FULL_BEFORE_SPLIT.md` | `docs_old/logs/PROJECT_LOG_ARCHIVE_2026-05-18_FULL_BEFORE_SPLIT.md` | referência histórica | Histórico preservado | Não copiado para docs ativo para evitar duplicidade de log. Referenciado por `PROJECT_LOG.md` e `docs_old/MANIFEST.md`. |
| `docs_old/PR001_CORE_FOUNDATION_HANDOFF.md` | `docs/refinements/implementados/ref_core_foundation_pr001_012.md` | absorção | Absorvido em refinement implementado | Handoff inicial da fundação. |
| `docs_old/CAVE_DEBUG_CONFINEMENT_VALIDATION.md` | `docs/refinements/implementados/ref_fix_cave_debug_skip_and_confinement_tolerance.md`; `docs/validation/` | absorção | Absorvido em refinement implementado | Validação de confinamento/debug cave. |
| `docs_old/CAVE_DEBUG_SKIP_CONFINEMENT_FIX.md` | `docs/refinements/implementados/ref_fix_cave_debug_skip_and_confinement_tolerance.md`; `docs/specs/implementados/spec_cave_008_debug_skip_confinement_wall_distance_hardening.md` | absorção | Absorvido em refinement implementado | Fix debug skip/confinement. |
| `docs_old/CHANGELOG_FASE9C_DOCUMENTACAO_v1.0.md` | `docs_old/CHANGELOG_FASE9C_DOCUMENTACAO_v1.0.md`; `docs/DOCS_REORGANIZATION_HANDOFF.md` | referência histórica | Histórico preservado | Mantido em histórico; handoff ativo resume a reorganização. |
| `docs_old/README.md` | `docs_old/README.md` | referência histórica | Histórico preservado | README da camada histórica. |
| `docs_old/README_LEGACY.md` | `docs_old/README_LEGACY.md` | referência histórica | Histórico preservado | README legado preservado. |
| `docs_old/MANIFEST.md` | `docs_old/MANIFEST.md`; `docs/DOCS_OLD_TO_ACTIVE_CROSSWALK.md` | referência histórica | Histórico preservado | Manifesto histórico e crosswalk ativo. |
| `docs_old/IMPLEMENTATION_STATUS.md` | `docs/IMPLEMENTATION_STATUS.md`; `docs/specs/SPEC_REGISTRY_IMPLEMENTED.md` | absorção | Ativo copiado | Status antigo absorvido no status reorganizado e registry. |

## Validação de preservação

- Nenhum arquivo de `docs_old/` deve ser removido pela reorganização.
- Conteúdo ativo vive em `docs/`.
- Execução SpecKit por feature continua em `specs/`.
- A pasta raiz `spec/` não é fonte oficial e permanece removida.
- Quando houver dúvida, priorizar preservação no `docs_old/` e linkar a partir dos registries/refinements.
