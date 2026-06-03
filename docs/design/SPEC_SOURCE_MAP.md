# Cindar's Hope — Spec Source Map

> **Status:** mapa canônico de fontes para refinamento e criação de specs  
> **Local:** `docs/design/SPEC_SOURCE_MAP.md`  
> **Função:** dizer quais documentos devem ser lidos antes de criar/refinar cada spec.  
> **Regra:** specs não devem ser criadas apenas com conversa solta; devem apontar suas fontes de design.

---

## 1. Regra principal

Toda spec em `docs/specs/a_implementar/` deve declarar uma seção:

```md
## Fontes obrigatórias lidas

- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
- docs/design/...
```

A spec também deve declarar:

```md
## Fora de escopo
## Estado atual do repo
## Dependências
## Arquivos permitidos
## Arquivos proibidos
## Critérios de aceite
## Validação Unity
```

---

## 2. Fontes globais obrigatórias

Toda spec de gameplay/lore/sistemas deve ler:

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
```

Motivo:

- mantém Vaalara como contexto obrigatório;
- define como especificar;
- evita specs sem rastreabilidade;
- evita contradições sobre Anya, Kanthor, Thandra, Nyx, Senya, Alihana, Finan, Merithus, Thoren, Kaand, Bromécia, Elyndor e Mana;
- garante que cada sistema continue ligado ao mundo.

---

# PARTE A — Fazenda

## 3. Specs de fazenda

Fontes obrigatórias:

```text
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
```

Usar `FARM_DESIGN_DIRECTION_v1.3.md` para visão, regras, sistemas, lore, roadmap e funcionamento amplo.

Usar `FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md` para escala visual, tile 32x32, player 32x48, footprints, props, construções, interiores, colisão, pathfinding e tamanhos em pixels.

Ler para specs de:

```text
farm layout
farm scale
farm expansion
free build
building footprints
soil/crops
watering/rain
crop death
fertilizers
quality
animals
pets
shipping bin
farm buildings
workshops
storage
companion farm jobs
fatigue/stamina/hunger/sleep
Fonte de Anya
Água Viva
Raiz Dormente de Mana
pedreira final
farm economy
```

Specs recomendadas derivadas:

```text
spec_farm_scale_tilemap_player_footbox.md
spec_farm_level1_layout_fixed_anchors.md
spec_farm_building_footprints_placement_grid.md
spec_farm_layout_expansion_zones_free_build.md
spec_player_condition_fatigue_sleep_hunger_stamina.md
spec_farm_buildings_construction_workshops_storage.md
spec_farm_crop_death_quality_fertilizers.md
spec_farm_weather_rain_irrigation_automation.md
spec_farm_shipping_bin_orders_processing.md
spec_farm_animals_pasture_products_care.md
spec_farm_pets_dog_cat_bond_buffs.md
spec_farm_companion_jobs_automation.md
spec_farm_fonte_anya_living_water.md
spec_farm_mana_root_arcane_soil_endgame.md
spec_farm_final_quarry_late_game_resources.md
spec_farm_fountain_anya_no_buildable_statue.md
```

---

# PARTE B — Cidade

## 4. Specs gerais de cidade

Fontes obrigatórias:

```text
docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
```

Ler para specs de:

```text
city layout
city buildings
city interiors
city NPCs
NPC residences
NPC beds
NPC schedules
NPC pathfinding
shops
services
contracts
reputation
romance
marriage
farm visits
festivals
moon events
Kanthor temple
altars
Anya statue garden
night shop
Nyx behaviours
Bromecia/Elyndor hooks
pixel scale
sprite scale
building sizes
props/interactables
```

Specs recomendadas derivadas:

```text
spec_city_scene_tilemap_collision_spawns.md
spec_city_buildings_exteriors_and_doors.md
spec_city_core_interiors_shops_services.md
spec_city_npc_residences_beds_schedule_markers.md
spec_city_npc_data_roster_stats.md
spec_city_npc_pathfinding_waypoints.md
spec_city_props_interactables_calendar_boards.md
spec_city_relationship_romance_marriage.md
spec_city_farm_visits_schedule_hooks.md
spec_city_kanthor_temple_and_altars.md
spec_city_deity_preferences_and_reputation.md
spec_city_kanthor_temple_statue_garden_no_anya_altar.md
spec_city_night_shop_nyx_behaviour.md
spec_city_festivals_layout_variations.md
spec_city_hidden_subsoil_bromecia_elyndor_hooks.md
```

## 5. Specs de cidade que tocam fazenda

Além das fontes de cidade, ler:

```text
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
```

Obrigatório para:

```text
farm visits
shipping bin/city economy
construction permits
animals/pets deliveries
marriage moving to farm
altar placement on farm
Fonte de Anya interactions
```

---

# PARTE C — Caverna

## 6. Specs de caverna

Fontes obrigatórias:

```text
docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
docs/game_rules/cave_rules.md
docs/decisions/ADR-0005-cave-stable-run-and-replay.md
```

Usar `CAVE_DESIGN_DIRECTION.md` para visão, estrutura macro, densidade, biomas, mineração, tesouros, boss gates, checkpoints, level 100, level 101 e roadmap.

Usar `CAVE_MONSTER_ROSTER_DIRECTION.md` para criaturas, bosses, packs, XP, drops, ataques, comportamento, bestiary e loot direction.

Além dessas fontes, specs de caverna que tocam runtime já existente devem ler:

```text
docs/specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md
docs/specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md
docs/specs/implementados/spec_cave_001_cave_scene_portal_e_runtime_basico.md
docs/specs/implementados/spec_cave_002_procedural_contracts_resources_parcial.md
docs/specs/implementados/spec_cave_003_stable_run_snapshots_replay_parcial.md
docs/specs/implementados/spec_cave_004_boss_gates_checkpoints_confinement_parcial.md
docs/specs/implementados/spec_cave_005_visual_runtime_camera_enemy_visuals.md
docs/specs/implementados/spec_cave_006_spawn_anchor_safe_positioning.md
docs/specs/implementados/spec_cave_007_snapshot_replay_full_layout_hardening.md
docs/specs/implementados/spec_cave_008_debug_skip_confinement_wall_distance_hardening.md
```

Temas esperados:

```text
100 levels
level 101/endgame
boss gate 100
biomes
mining
resource nodes
treasure rooms
special rooms
hazards
monster density
monster packs
bosses
checkpoints
Elyndor portals
Bromecia ruins
Pedra Negra corrompida
Pedra de Meteoro Negra Estabilizada
death/corpse recovery
Fonte de Anya integration
Anya partial power liberation
fatigue in cave
companions in cave
pet dog support
```

Specs recomendadas futuras:

```text
spec_cave_design_reconciliation_density_rules.md
spec_cave_world_generation_levels_biomes.md
spec_cave_enemy_density_packs_spawnplan_rebalance.md
spec_cave_special_rooms_mining_treasure_generation.md
spec_cave_mining_resources_nodes.md
spec_cave_resources_treasures_biome_tables.md
spec_cave_boss_gate_100_level_101_unlock.md
spec_cave_level_101_boss_gauntlet_anya_lore.md
spec_cave_monster_roster_data_expansion.md
spec_cave_enemy_actions_vulnerability_profiles.md
spec_cave_boss_gate_roster_rewards.md
spec_cave_bestiary_entries_lore_rewards.md
spec_cave_snapshot_special_elements_persistence.md
spec_cave_checkpoints_elyndor_portals.md
spec_cave_death_corpse_recovery_fonte.md
spec_cave_city_guild_contracts_integration.md
```

---

# PARTE D — Combate, magia e progressão

## 7. Specs de combate/magia/progressão

Fonte futura principal:

```text
docs/design/gameplay/combat_magic_progression/COMBAT_MAGIC_PROGRESSION_DESIGN_DIRECTION_v1.0.md
```

Enquanto não existir, ler:

```text
docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
```

Temas esperados:

```text
HP
MP
Stamina
Breath/Fôlego
Força
Constituição
Destreza
Inteligência
Vontade
Carisma
Fome
Exaustão
Frio
Calor
Veneno
Medo
Morte
weapons
magic skills
skill trees
levels
classes/jobs do jogo
companions combat
pets combat support
cave combat balance
```

Specs recomendadas futuras:

```text
spec_player_stats_hp_mp_stamina_breath_attributes.md
spec_status_effects_hunger_exhaustion_temperature_poison_fear_death.md
spec_combat_player_weapons_actions.md
spec_magic_mp_skills_progression.md
spec_skill_trees_active_slots_respec.md
spec_jobs_classes_gameplay_roles.md
```

---

# PARTE E — Companions e pets

## 8. Specs de companions

Fonte futura principal:

```text
docs/design/gameplay/companions/COMPANIONS_DESIGN_DIRECTION_v1.0.md
```

Enquanto não existir, ler:

```text
docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
```

Temas esperados:

```text
companion recruitment
relationship
farm jobs
cave companions
death/unavailable state
resurrection at Fonte
affinity
marriage distinction
NPC service mode
pet system separate from companion
pet dog cave support
```

Specs recomendadas futuras:

```text
spec_companions_recruitment_relationship_affinity.md
spec_companions_farm_jobs_automation.md
spec_companions_cave_party_slots.md
spec_companions_death_unavailable_resurrection_fonte.md
spec_pets_dog_cat_bond_buffs_combat_support.md
```

---

# PARTE F — UI/UX

## 9. Specs de UI/UX

Fonte futura principal:

```text
docs/design/gameplay/ui_ux/UI_UX_DESIGN_DIRECTION_v1.0.md
```

Enquanto não existir, ler fontes do domínio tocado:

```text
Fazenda -> FARM_DESIGN_DIRECTION_v1.3.md + FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
Cidade -> CITY_DESIGN_DIRECTION_v1.2.md + CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
Caverna -> CAVE_DESIGN_DIRECTION.md + CAVE_MONSTER_ROSTER_DIRECTION.md
NPCs -> CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
Canon -> VAALARA_GAME_CANON_DIRECTION_v1.0.md
Processo -> SPECIFICATION_PROCESS.md
```

Temas esperados:

```text
HUD
inventory
equipment
hotbar
shops
dialogue
relationship UI
calendar
map
building mode
skill tree
crafting
shipping bin
quests
contracts
festival UI
cave checkpoint side menu
cave boss UI
bestiary UI
```

Specs recomendadas futuras:

```text
spec_ui_hud_status_bars_hp_mp_stamina_breath_hunger_fatigue.md
spec_ui_inventory_equipment_hotbar.md
spec_ui_dialogue_relationship_gifts.md
spec_ui_city_map_calendar_shops_contracts.md
spec_ui_farm_build_mode_layout.md
spec_ui_skill_tree_respec_fonte.md
spec_ui_cave_checkpoint_bestiary_boss_feedback.md
```

---

# PARTE G — Template obrigatório de spec

## 10. Cabeçalho mínimo

Toda spec deve começar com:

```md
# Cindar's Hope — <Nome da Spec>

> Status: a implementar
> Tipo: spec implementável
> Fontes obrigatórias lidas:
> - docs/design/SPEC_SOURCE_MAP.md
> - docs/design/SPECIFICATION_PROCESS.md
> - docs/design/...

## Objetivo

## Escopo

## Fora de escopo

## Estado atual do repo

## Dependências

## Contratos/dados/eventos

## Arquivos permitidos

## Arquivos proibidos

## Critérios de aceite

## Validação Unity

## Riscos e rollback
```

## 11. Regra de rastreabilidade

Toda spec deve conseguir responder:

```text
De qual design direction esta decisão veio?
Qual arquivo sustenta essa regra?
Qual parte é lore/canon?
Qual parte é mecânica?
Qual parte é implementação proposta agora?
```

Se a spec não conseguir responder isso, ela ainda é pré-refinamento, não spec.
