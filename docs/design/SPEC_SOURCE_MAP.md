# Cindar's Hope — Spec Source Map

> **Status:** mapa canônico de fontes para refinamento e criação de specs  
> **Local:** `docs/design/SPEC_SOURCE_MAP.md`  
> **Função:** dizer quais documentos devem ser lidos antes de criar/refinar cada spec.  
> **Regra:** specs não devem ser criadas apenas com conversa solta; devem apontar suas fontes de design.

---

## 1. Regra principal

Toda spec em `docs/specs/a_implementar/` deve declarar:

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

---

# PARTE A — Fazenda

## 3. Specs de fazenda

Fontes obrigatórias:

```text
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
```

Specs de fazenda que envolvam invasões, defesa, dano a crops/estruturas, pets/companions em defesa ou inimigos no mapa da fazenda devem ler também:

```text
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
```

Usar para specs de:

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
farm invasions futuras
farm defense futura
repair/recovery de crops/estruturas
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
spec_farm_invasion_events_future.md
spec_farm_defense_repair_recovery_future.md
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

Specs de cidade que tocam fazenda devem ler também:

```text
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
```

Specs de cidade que envolvam eventos hostis, defesa, cultistas, monstros, civis fugindo ou combate em área urbana devem ler também:

```text
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
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
spec_city_hostile_event_behaviors_future.md
```

---

# PARTE C — Caverna

## 5. Specs de caverna

Fontes obrigatórias:

```text
docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_VISUAL_SPRITE_DIRECTION.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
docs/game_rules/cave_rules.md
docs/decisions/ADR-0005-cave-stable-run-and-replay.md
```

Uso de cada fonte:

```text
CAVE_DESIGN_DIRECTION.md
  visão macro, 100 níveis, nível 101, boss gates, mineração, tesouros e roadmap.

CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md
  tamanho mínimo/máximo dos níveis, randomização ponderada de biomas, layout archetypes, special rooms e snapshot.

CAVE_MONSTER_ROSTER_DIRECTION.md
  criaturas, bosses, packs, atributos, XP, drops, ataques, comportamento e scaling.

CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
  active combat budget, vulnerabilidades, janelas críticas, companions/pets, TTK e telemetria de Stamina.

CAVE_MONSTER_VISUAL_SPRITE_DIRECTION.md
  descrição visual, tamanho, silhueta, cores, animações, variações e telegraph de sprites.

COMBAT_CORE_DIRECTION.md
  regras gerais de combate, inputs, HP/MP/Stamina, ataque, block, dodge, dash, dano, armor, vulnerabilidades, bosses, HUD e telemetria.

ENEMY_BEHAVIORS_DIRECTION.md
  EnemyBrain, EnemyAction, reação a Dash/Dodge/Block, Stamina/MP de inimigos, pack coordination, leash, objetivos e comportamentos transversais.
```

Specs de caverna que tocam runtime já existente devem ler também:

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
procedural generation
level size ranges
weighted biome randomization
layout archetypes
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
active enemy budget
monster packs
monster visual sprites
bosses
boss phases
vulnerabilities
critical windows
enemy brain
enemy action selection
enemy stamina/MP use
pack coordination
leash/reacquire
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
combat inputs
Stamina economy
Block/Dodge/Dash
telemetry
```

Specs recomendadas futuras:

```text
spec_cave_active_enemy_budget.md
spec_cave_vulnerability_critical_windows.md
spec_cave_enemy_vulnerability_tables.md
spec_cave_boss_phase_vulnerabilities.md
spec_cave_companion_pet_combat_balance.md
spec_cave_time_to_kill_balance_targets.md
spec_cave_stamina_telemetry_playtest.md
spec_cave_treasure_trap_counterplay.md
spec_enemy_brain_action_selection_runtime.md
spec_enemy_movement_profiles_official_moves.md
spec_enemy_pack_coordination_leash_runtime.md
```

---

# PARTE D — Combate

## 6. Specs gerais de combate

Fontes obrigatórias:

```text
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
```

Usar para specs de:

```text
combat controller
input buffer
attack controller
light attack
heavy attack
charged attack
Stamina costs
Stamina regen
Dash
Dodge
Block
Perfect Block
BlockImpact
HP/MP/Stamina combat rules
DamageType
Armor/Defense
critical hit
MinorOpening
CriticalWindow
CoreExposed
posture/stagger
guard break
weapon actions
magic combat actions
enemy reactions
enemy action phases
combat HUD
combat feedback
combat telemetry
```

Specs recomendadas derivadas:

```text
spec_combat_core_controller_inputs.md
spec_combat_stamina_costs_regen_runtime.md
spec_combat_block_dodge_dash_runtime.md
spec_combat_damage_armor_resistance_contract.md
spec_combat_critical_windows_vulnerability_contract.md
spec_combat_weapon_actions_light_heavy_charged.md
spec_combat_magic_actions_mp_casting.md
spec_combat_posture_stagger_guardbreak.md
spec_combat_hud_feedback_telemetry.md
```

---

# PARTE E — Inimigos / Enemy AI

## 7. Specs gerais de inimigos

Fontes obrigatórias:

```text
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
```

Usar para specs de:

```text
EnemyBrain
EnemyActionSO
EnemyActionSetSO
EnemyBrainProfileSO
EnemyMovementProfileSO
EnemyBehaviorProfileSO
EnemyThreat/Aggro
EnemyTargetPriority
EnemyLeashRules
EnemyReactionRules
PackCoordinationRules
EnemyObjectiveProfileSO
EnemyInvasionProfileSO
farm invasion enemies
town hostile event enemies
boss AI phases
pet/companion target logic
```

Specs recomendadas derivadas:

```text
spec_enemy_brain_runtime_architecture.md
spec_enemy_action_so_contract.md
spec_enemy_actionset_so_contract.md
spec_enemy_movement_profiles_official_moves.md
spec_enemy_behavior_profiles_runtime.md
spec_enemy_threat_aggro_target_priority.md
spec_enemy_reactions_block_dodge_dash_magic_pet_companion.md
spec_enemy_pack_coordination_leash_rules.md
spec_enemy_boss_phase_ai_contract.md
spec_enemy_farm_invasion_profile_future.md
spec_enemy_objective_profile_events_future.md
```

---

# PARTE F — Player / atributos / skills

## 8. Specs de player core

Fontes obrigatórias:

```text
docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_TABLETOP_EXAMPLE.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
```

Usar para specs de:

```text
player creation
initial races
attributes
HP
MP
Stamina
hunger
fatigue
level up
attribute points
skill points
skill trees
active slots
Dash/Dodge/Block
classes/jobs inferred
save/load player state
```

---

# PARTE G — Equipamentos, armas, magia, companions, pets e HUD

## 9. Specs que devem ler Combat Core

Specs de equipamentos/armas devem ler:

```text
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
```

Specs de magia devem ler:

```text
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
```

Specs de companions/pets em combate devem ler:

```text
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
```

Specs de HUD gameplay/combat devem ler:

```text
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
```

---

# PARTE H — Regra anti-regressão

## 10. Quando houver conflito

Se houver conflito entre documentos:

```text
1. Documento mais específico vence sobre documento geral.
2. Documento de design canônico vence conversa solta.
3. Documento mais recente vence documento antigo quando ambos cobrem o mesmo tema.
4. SPEC_SOURCE_MAP.md deve ser atualizado quando uma nova fonte canônica for criada.
```

Conflitos conhecidos resolvidos:

```text
Breath/Fôlego não existe como atributo/recurso.
Breath pode continuar como nome de ataque de sopro de criatura.
BR não existe como coluna/stat de monstro.
Monstros mantêm HP, MP e STA.
Stamina é recurso físico imediato.
Cansaço é desgaste acumulado.
Constituição não é atributo defensivo universal.
Nem toda janela comportamental gera crítico automático.
Moves oficiais de inimigos vêm do CAVE_MONSTER_ROSTER_DIRECTION.md quando o inimigo pertence ao roster da caverna.
Enemy Behaviors é transversal e pode ser usado por caverna, fazenda, cidade e eventos.
```