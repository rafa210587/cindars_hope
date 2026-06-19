# Mapa de refinamentos futuros

> Status: camada ativa para refinamentos ainda nao implementados. A pasta `pre_refinamentos/` contem os pre-refinamentos vivos; cada item aponta para a(s) spec(s) `fable_*` consolidada(s) em `.specs/a_implementar/fable/`.

O refinamento de tracking documental foi reclassificado como implementado/parcial em `docs/refinements/implementados/ref_docs_single_source_tracking_reconciliation_parcial.md` e nao deve mais ser tratado como backlog vivo.

> **Re-mapa (fable_67, 2026-06-19):** auditoria verificou que as 8 specs originais `.specs/a_implementar/spec_*.md` deste mapa **nao existem mais** (o planejamento migrou para `.specs/a_implementar/fable/`) e que os 8 arquivos `pre_refinamentos/refinamento_init_*.md` que este mapa citava **tambem nao existem** (seu conteudo foi consolidado nos documentos `PRE_REFINAMENTO_*` de direcao e absorvido pelas specs `fable_*`). Para fechar a cadeia documental sem inventar arquivos, a tabela abaixo lista os refinamentos que **realmente existem** nesta pasta, cada um re-apontado para a(s) spec `fable_*` equivalente verificada por Test-Path. O bloco "Historia (dominios consolidados)" preserva os 8 dominios originais e seu destino fable.

## Pre-refinamentos vivos (arquivos existentes)

| Refinement (existe) | Spec(s) fable relacionada(s) | Observacao |
|---|---|---|
| [pre_refinamentos/PRE_REFINAMENTO_VISAO_GERAL_JOGO_v1.1.md](pre_refinamentos/PRE_REFINAMENTO_VISAO_GERAL_JOGO_v1.1.md) | direcao macro — alimenta o lote `fable_*` (ver `.specs/a_implementar/fable/fable_00_index_gap_analysis.md`) | Vigente; v1.0 superada e arquivada (delete candidates). |
| [pre_refinamentos/PRE_REFINAMENTO_FARM_ESTRUTURA_ATIVIDADES_RECURSOS_v1.1.md](pre_refinamentos/PRE_REFINAMENTO_FARM_ESTRUTURA_ATIVIDADES_RECURSOS_v1.1.md) | farm/recursos — coberto por `fable_15`+`fable_17` (clima/refresh/qualidade/Fonte) e catalogos farm | Vigente; v1.0 superada e arquivada (delete candidates). |
| [pre_refinamentos/refinamento_combat_movement_projectiles_melee_visuals.md](pre_refinamentos/refinamento_combat_movement_projectiles_melee_visuals.md) | [fable_02 (combate)](../../../.specs/a_implementar/fable/fable_02_spec_combat_weapon_actions_derived_stats_runtime.md) + [fable_08 (spell shapes/visual)](../../../.specs/a_implementar/fable/fable_08_spec_magic_spell_shapes_targeting_runtime.md) | O header deste arquivo ainda cita `spec_combat_movement_*` (inexistente) — fora do escopo de fable_67; registrado como achado no execution report. |

## Historia (dominios consolidados — re-mapa fable_67)

Os 8 dominios que este mapa originalmente listava como specs `spec_*` apontam hoje para as specs `fable_*` abaixo (verificadas por Test-Path). Os arquivos-fonte `refinamento_init_*` foram consolidados nos `PRE_REFINAMENTO_*` acima:

| Dominio original (spec_* aposentada) | Destino fable (existente) |
|---|---|
| equipment_durability_environment_loot | [fable_03](../../../.specs/a_implementar/fable/fable_03_spec_equipment_mechanical_baselines_runtime.md) + [fable_06](../../../.specs/a_implementar/fable/fable_06_spec_enemy_loot_tables_vulnerability_tags_runtime.md) |
| damage_status_elements_resistances | [fable_01](../../../.specs/a_implementar/fable/fable_01_spec_status_effects_canonical_set_runtime.md) + [fable_02](../../../.specs/a_implementar/fable/fable_02_spec_combat_weapon_actions_derived_stats_runtime.md) |
| player_combat_weapons_spells_skill_actions | [fable_02](../../../.specs/a_implementar/fable/fable_02_spec_combat_weapon_actions_derived_stats_runtime.md) + [fable_07](../../../.specs/a_implementar/fable/fable_07_spec_magic_learning_unlock_sources_runtime.md) + [fable_08](../../../.specs/a_implementar/fable/fable_08_spec_magic_spell_shapes_targeting_runtime.md) |
| enemy_ai_roster_bestiary_faction_locks | [fable_04](../../../.specs/a_implementar/fable/fable_04_spec_enemy_threat_pack_coordination_runtime.md) + [fable_33](../../../.specs/a_implementar/fable/fable_33_spec_bestiary_data_expansion_60_creatures.md) |
| cave_runtime_generation_checkpoints_boss_gates | [fable_05](../../../.specs/a_implementar/fable/fable_05_spec_cave_boss_phase_ai_runtime.md) + [fable_09](../../../.specs/a_implementar/fable/fable_09_spec_cave_biome_layout_variety_runtime.md) |
| cave_entry_death_anya_corpse_recovery | [fable_17](../../../.specs/a_implementar/fable/fable_17_spec_fonte_anya_physical_interactable_runtime.md) + [fable_44](../../../.specs/a_implementar/fable/fable_44_spec_cave_save_completion_policy.md) |
| skill_trees_active_slots_respec_anya | [fable_29](../../../.specs/a_implementar/fable/fable_29_spec_canonical_skill_catalog_migration.md) |
| ui_ux_full_gameplay_inventory_hotbar_menus | [fable_14](../../../.specs/a_implementar/fable/fable_14_spec_ui_canvas_screens_integration_runtime.md) |
