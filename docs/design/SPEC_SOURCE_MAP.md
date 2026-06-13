# Cindar's Hope — Spec Source Map

> **Status:** mapa canônico de fontes para refinamento e criação de specs  
> **Local:** `docs/design/SPEC_SOURCE_MAP.md`  
> **Função:** dizer quais documentos devem ser lidos antes de criar/refinar cada spec.  
> **Regra:** specs não devem ser criadas apenas com conversa solta; devem apontar suas fontes de design.

---

## 1. Regra principal

Toda spec em `.specs/a_implementar/` deve declarar:

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

Fontes globais obrigatórias para gameplay/lore/sistemas:

```text
docs/design/SPEC_SOURCE_MAP.md
docs/design/SPECIFICATION_PROCESS.md
docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
```

**Regra UI/UX centralizada:**

Specs que envolvam UI, HUD, menus, modal, input routing, foco de UI, tooltip, inventory UI, equipment UI, shop UI, crafting UI, skill tree UI, dialogue UI, quest log, social log, calendar, Fonte UI, cave HUD, combat feedback, notifications ou debug HUD devem ler obrigatoriamente:

```text
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
```

**Regra World/Time centralizada:**

Specs que envolvam tempo, calendário, estações, clima, chuva, neve, tempestade, névoa, calor, frio, previsão, passagem de dia, sono, colapso por horário, day transition, festivais, eventos lunares, Alihana, Senya, Nyx, crops sazonais, crops lunares, Mana por estação/lua, Fonte reagindo a clima/lua, schedules dependentes de clima/lua ou eventos temporais devem ler obrigatoriamente:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
```

**Regra Save/Load full-state centralizada:**

Specs que envolvam save/load, `GameSaveData`, `SaveManager`, migration, schema, backup, escrita segura, providers, save sections, capture/restore, restore order, save em caverna, save cross-scene, DTOs, IDs persistidos, defaults de seção, validação de save, preservação de seção fora de cena, UI state não persistível ou qualquer novo estado persistido devem ler obrigatoriamente:

```text
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
```

**Regra Bestiary/Knowledge Discovery centralizada:**

Specs que envolvam bestiário, conhecimento de inimigos, descoberta de vulnerabilidades, identificação de criaturas, knowledge states, enemy knowledge save/load, drops conhecidos, resistências conhecidas, imunidades conhecidas, behavior windows descobertas, lore notes de criaturas, NPC/livro/quest desbloqueando conhecimento, equipment tooltip com known enemy interactions, spell tooltip com known effectiveness, Bestiary HUD futura, Bestiary Menu futuro ou spoiler control de inimigos devem ler obrigatoriamente:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
```

**Regra Quest/Objective/Event centralizada:**

Specs que envolvam quest system, QuestId, QuestDefinition, QuestState, QuestStep, Objective, Condition, Trigger, Reward, QuestFlag, QuestEvent, branching, failure, expiry, quest UI, quest log, quest save/load, anti-softlock, anti-spoiler, main quest hooks, Fonte hooks, time/weather/lunar hooks, NPC/dialogue hooks, farm orders, festival quests, cave contracts, hidden quests ou tutorial quests devem ler obrigatoriamente:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
```

---

# PARTE A — Fazenda

## 2. Specs de fazenda

Fontes obrigatórias:

```text
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
```

Specs de fazenda que envolvam ferramentas, crafting, materiais, pedreira final, oficinas, reparo, shipping bin, encomendas, storage, economia, processamento, venda, preço, BaseValue, SellPoint, pending payments, resource refresh, node refresh ou anti-arbitragem devem ler também:

```text
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
```

Specs de fazenda que envolvam visitors, companion jobs, job board, automação por NPC, visitas de NPCs, spouse/partner helper, vínculo funcional, romance/casamento, poliamor, visitas sociais à fazenda ou partner companion hooks devem ler também:

```text
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
```

Specs de fazenda que envolvam pets, cachorro, gato, cama/tigela/brinquedo, pet home area, vínculo de pet, rotina de pet, pet farm hints, pet foraging hints ou interação pet-fazenda devem ler também:

```text
docs/design/gameplay/pets/PETS_DIRECTION.md
```

Specs de fazenda que envolvam Fonte de Anya, Água Viva, fragmentos de Anya, Cindar, Mana, Arco da Memória, main quest, eventos de quest na fazenda ou progressão da Fonte devem ler também:

```text
docs/design/gameplay/quests/QUESTS_LORE_WEAVING_BRIEF.md
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
```

Specs de fazenda que envolvam crops sazonais, chuva, irrigação, morte de planta por falta de água, clima, estufa, eventos sazonais, festivais agrícolas, fertilizante lunar, crops mágicas/lunares, Mana, Água Viva, Fonte reagindo a lua/clima ou day transition devem ler também:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
```

Specs de fazenda **não devem implementar invasões, defesa contra inimigos, dano a crops/estruturas ou inimigos no mapa da fazenda agora**.

Backlog futuro, não atual:

```text
spec_farm_invasion_events_future.md
spec_farm_defense_repair_recovery_future.md
```

Quando esse tema entrar explicitamente no roadmap futuro, ler:

```text
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
docs/design/gameplay/pets/PETS_DIRECTION.md
```

Specs recomendadas atuais:

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
spec_sellpoint_shipping_price_pending_payment_runtime.md
spec_resource_node_refresh_runtime.md
```

---

# PARTE B — Cidade

## 3. Specs de cidade

Fontes obrigatórias:

```text
docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
```

Specs de cidade que tocam fazenda devem ler:

```text
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
```

Specs de cidade que envolvam companion eligibility, companion unlock, convite, disponibilidade, rotina de companion, visitas à fazenda, spouse helper ou companion services devem ler também:

```text
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
```

Specs de cidade que envolvam relationship, friendship, trust, gift, romance, casamento, casamento poliamoroso, até 3 parceiros, partner companion unlock, partner helper, spouse/partner farm visit ou social memory devem ler também:

```text
docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
```

Specs de cidade que envolvam pet adoption, pet shop, pet food, pet toys, pet bed/bowl, pet services ou NPCs interagindo com pets devem ler também:

```text
docs/design/gameplay/pets/PETS_DIRECTION.md
```

Specs de cidade que envolvam loja, ferreiro, crafting, reparo, equipamentos, armas, armaduras, materiais, pergaminhos, wands, arrows, encomendas, reputação econômica, serviços, estoque, refresh/restock, limited stock, preço, compra/venda, economia de gear, ensino de magia ou venda de item mágico devem ler:

```text
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
```

Specs de cidade que envolvam main quest, Cindar, Anya, Litania do Primeiro Retorno, Padre Corvus, Vaelrion, Sethra, Yael, Arco da Memória, culto de Nyx, Pedra Negra, Fonte, fragmentos de Anya, ruínas urbanas ou progressão de atos devem ler também:

```text
docs/design/gameplay/quests/QUESTS_LORE_WEAVING_BRIEF.md
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
```

Specs de cidade que envolvam calendário público, quadro público, festivais, horários, portas por horário, lojas por horário, restock ligado a calendário, NPC schedules modificados por clima/lua, loja noturna, rumores, eventos de praça, aniversários futuros, evento de Alihana/Senya/Nyx, Jardim das Estátuas reagindo a lua ou cidade mudando por clima devem ler também:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
```

Regra:

```text
CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md vence para rotina individual de NPC, camas, waypoints, prédios, interiores e portas.
SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md vence para condições globais de tempo, clima, estação, lua e festival que schedules consomem.
```

Specs de cidade **não devem implementar eventos hostis, defesa urbana, cultistas, monstros ou civis fugindo agora**.

Specs de cidade **não devem implementar runtime completo de social/romance/casamento/poliamor na execução atual** sem nova decisão de roadmap. Devem apenas preparar hooks, IDs, anchors, schedules e compatibilidade futura quando necessário.

Backlog futuro, não atual:

```text
spec_city_hostile_event_behaviors_future.md
spec_city_relationship_romance_marriage_future.md
spec_city_farm_visits_schedule_hooks_future.md
spec_social_relationship_profile_contract_future.md
spec_social_friendship_trust_runtime_future.md
spec_social_gift_preferences_reactions_future.md
spec_social_romance_route_runtime_future.md
spec_social_poly_relationship_runtime_future.md
spec_social_partner_companion_unlock_future.md
spec_social_partner_helper_farm_runtime_future.md
```

Specs recomendadas atuais:

```text
spec_city_scene_tilemap_collision_spawns.md
spec_city_buildings_exteriors_and_doors.md
spec_city_core_interiors_shops_services.md
spec_city_npc_residences_beds_schedule_markers.md
spec_city_npc_data_roster_stats.md
spec_city_npc_pathfinding_waypoints.md
spec_city_props_interactables_calendar_boards.md
spec_city_kanthor_temple_and_altars.md
spec_city_deity_preferences_and_reputation.md
spec_city_kanthor_temple_statue_garden_no_anya_altar.md
spec_city_night_shop_nyx_behaviour.md
spec_city_festivals_layout_variations.md
spec_city_hidden_subsoil_bromecia_elyndor_hooks.md
spec_shop_inventory_stockline_restock_contract.md
spec_shop_buy_sell_price_runtime.md
spec_economy_anti_arbitrage_tests.md
```

---

# PARTE C — Caverna

## 4. Specs de caverna

Fontes obrigatórias:

```text
docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_ENEMY_BEHAVIOR_ADAPTER.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_VISUAL_SPRITE_DIRECTION.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
docs/design/gameplay/pets/PETS_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
docs/game_rules/cave_rules.md
docs/decisions/ADR-0005-cave-stable-run-and-replay.md
```

Specs de caverna que envolvam main quest, níveis 100/101, Arco da Memória, Cindar, Anya, fragmentos de Anya, Vaelrion, Sethra, Arquivista do Silêncio, Pedra Negra, Água Viva, Mana, Elyndor, Bromécia ou boss gate narrativo devem ler também:

```text
docs/design/gameplay/quests/QUESTS_LORE_WEAVING_BRIEF.md
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
```

Specs de caverna que envolvam modificadores por clima/lua, eventos de Alihana/Senya/Nyx, Pedra Negra mais ativa em Nyx, caverna alterada por névoa/tempestade, lagos subterrâneos brilhando, inscrições reveladas, inimigos noturnos, criaturas caóticas, eventos de memória, boss gate condicionado por lua ou nível 100/101 usando alinhamento lunar devem ler também:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
```

Specs de caverna que tocam runtime já existente devem ler também:

```text
.specs/a_implementar/spec_cave_runtime_generation_checkpoints_boss_gates.md
.specs/a_implementar/spec_enemy_ai_roster_bestiary_faction_locks_runtime.md
.specs/implementados/spec_cave_001_cave_scene_portal_e_runtime_basico.md
.specs/implementados/spec_cave_002_procedural_contracts_resources_parcial.md
.specs/implementados/spec_cave_003_stable_run_snapshots_replay_parcial.md
.specs/implementados/spec_cave_004_boss_gates_checkpoints_confinement_parcial.md
.specs/implementados/spec_cave_005_visual_runtime_camera_enemy_visuals.md
.specs/implementados/spec_cave_006_spawn_anchor_safe_positioning.md
.specs/implementados/spec_cave_007_snapshot_replay_full_layout_hardening.md
.specs/implementados/spec_cave_008_debug_skip_confinement_wall_distance_hardening.md
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
spec_cave_monster_roster_to_enemy_data_conversion.md
spec_cave_enemy_component_loot_tables.md
spec_cave_enemy_material_vulnerability_tags.md
spec_cave_treasure_mining_loot_tables.md
spec_cave_loot_refresh_snapshot_rules.md
spec_enemy_brain_action_selection_runtime.md
spec_enemy_movement_profiles_official_moves.md
spec_enemy_pack_coordination_leash_runtime.md
```

---

# PARTE D — Combate

## 5. Specs de combate

Fontes obrigatórias:

```text
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
```

Specs de combate que envolvam companions, companion AI, companion assist, companion healer/guardian, target priority de companion, downed companion ou cave party balance devem ler também:

```text
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
```

Specs de combate que envolvam pets, pet follow/leash, pet interrupt, pet alert, pet trap/treasure hint, pet cave state ou pet boss limits devem ler também:

```text
docs/design/gameplay/pets/PETS_DIRECTION.md
```

Specs de combate que envolvam spells, MP casting, staff charged, magic actions, healing, barrier, purification, wands, scrolls, tomes, focuses, known spells ou spell source devem ler também:

```text
docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
```

Specs de combate que envolvam loot, drops, reward, gold, item use, consumíveis, durability economy, repair cost, upgrade cost ou crafting devem ler também:

```text
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
```

Specs de combate que envolvam boss final, Arquivista do Silêncio, Água Viva corrompida, fragmentos de Anya, Pedra Negra, boss gate do nível 100 ou nível 101 devem ler também:

```text
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
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
spec_combat_status_effects_runtime.md
spec_combat_hud_feedback_telemetry.md
```

---

# PARTE E — Inimigos / Enemy AI

## 6. Specs de inimigos

Fontes obrigatórias:

```text
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
docs/design/gameplay/pets/PETS_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_ENEMY_BEHAVIOR_ADAPTER.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
```

Usar para specs atuais de:

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
boss AI phases
pet/companion target logic
cave roster to enemy data conversion
enemy family equipment counters
enemy component drops
spell vulnerability matching
LootTableSO
EnemyDropTableSO
BossDropTableSO
MaterialVulnerability
ResistanceTags
ImmunityTags
StatusVulnerability
BodyTags
FamilyTags
```

Não usar em specs atuais sem nova decisão de roadmap:

```text
EnemyObjectiveProfileSO
EnemyInvasionProfileSO
farm invasion enemies
town hostile event enemies
world enemy events
```

Specs recomendadas derivadas atuais:

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
spec_cave_monster_roster_to_enemy_data_conversion.md
spec_enemy_material_vulnerability_contract.md
spec_enemy_drop_tables_by_family.md
```

Backlog futuro, não atual:

```text
spec_enemy_farm_invasion_profile_future.md
spec_enemy_objective_profile_events_future.md
spec_city_hostile_event_behaviors_future.md
```

---

# PARTE F — Player / atributos / skills

## 7. Specs de player core

Fontes obrigatórias:

```text
docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_TABLETOP_EXAMPLE.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
```

Specs de player que envolvam equipamentos, slots, armas, armaduras, escudos, acessórios, peso, ASPD, durabilidade, arrows, wands, scrolls, inventory, storage, ItemStack, ItemInstance, save/load de itens ou economia devem ler também:

```text
docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
```

Specs de player que envolvam known spells, equipped spells, MP casting, spell unlocks, spell learning, spell source, staff/focus, wands, scrolls, tomes, magic HUD ou capstones Anya/Senya devem ler também:

```text
docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
```

Specs de player que envolvam companion state, active companion, companion interaction, companion save/load, spouse helper, companion bond ou party state devem ler também:

```text
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
```

Specs de player que envolvam pet state, active pet, pet bond, pet mood, pet energy, pet food, pet home area, pet save/load ou pet interaction devem ler também:

```text
docs/design/gameplay/pets/PETS_DIRECTION.md
```

Specs de player que envolvam respawn na Fonte, respec pela Fonte, Água Viva, fragmentos de Anya, purificação, cansaço reduzido por Água Viva, main quest ou escolhas finais devem ler também:

```text
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
```

Specs que envolvam relógio, tempo rodando, pausa em UI modal, sono, colapso, recuperação diária, Cansaço por horário/clima, Fome por tempo, day transition, save/load de tempo, CurrentDay, CurrentSeason, CurrentYear, CurrentTime, CurrentWeather, TomorrowWeather, ActiveLunarEvent, FestivalState ou WeatherSeed devem ler:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
```

---

# PARTE G — Equipamentos, armas, magia, companions, pets e HUD

## 8. Specs de equipamentos/armas/armaduras/materiais

Fontes obrigatórias:

```text
docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
```

Specs de equipamentos mágicos, staff, wands, scrolls, tomes, focuses, SpellActionDataSO, SpellUnlockSourceSO, spell learning, spell sources ou spell modifiers devem ler também:

```text
docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
```

Specs de companion equipment futuro devem ler também:

```text
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
```

Specs de pet items, pet food, pet toys, pet bed, pet bowl ou pet-related shop items devem ler também:

```text
docs/design/gameplay/pets/PETS_DIRECTION.md
```

Specs recomendadas derivadas:

```text
spec_equipment_item_so_contract.md
spec_weapon_data_so_contract.md
spec_weapon_charged_effect_profiles.md
spec_armor_shield_accessory_data_contract.md
spec_material_tiers_and_modifiers.md
spec_equipment_mechanical_baselines_runtime.md
spec_equipment_bows_arrows_ammo_runtime.md
spec_equipment_wands_scrolls_tomes_focuses_runtime.md
spec_equipment_enemy_vulnerability_adapter.md
spec_equipment_durability_repair_runtime.md
spec_equipment_upgrade_crafting_recipes.md
spec_equipment_enemy_family_counters.md
spec_equipment_tooltip_inventory_comparison.md
spec_equipment_save_load_instances.md
```

## 9. Specs de magia

Fontes obrigatórias:

```text
docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
```

Specs de healing/support magic que afetam companions ou pets devem ler também:

```text
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
docs/design/gameplay/pets/PETS_DIRECTION.md
```

Specs de magia que envolvam Anya, Fonte, Água Viva, purificação, fragmentos de Anya, Nyx, Pedra Negra, Senya/Alihana como gatilhos de quest ou spell source narrativo devem ler também:

```text
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
```

Usar para specs de:

```text
SpellActionDataSO
SpellUnlockSourceSO
SpellShapeProfileSO
SpellScalingProfileSO
SpellUnlockRuleSO
SpellUpgradeRuleSO
SpellStatusApplicationSO
SpellVFXProfileSO
SpellSFXProfileSO
SpellHUDProfileSO
TomeSpellUnlockSO
FocusSpellModifierSO
WandSpellProfileSO
ScrollSpellProfileSO
MagicBalanceProfileSO
knownSpellIds
knownSpellVariants
discoveredSpellIds
studiedTomeIds
equippedSpellSlots
ItemProvided spells
ConsumableProvided spells
spell source state
spell cooldowns
spell targeting
spell shapes
MP costs
cast time
interrupt
healing
barrier
purification
elemental damage
Anya/Senya capstone spell modifiers
```

Specs recomendadas derivadas:

```text
spec_magic_spell_action_data_contract.md
spec_magic_spell_unlock_source_contract.md
spec_magic_spell_learning_scroll_tome_runtime.md
spec_magic_spell_shapes_targeting_runtime.md
spec_magic_mp_cost_cast_cooldown_runtime.md
spec_magic_spell_unlocks_tomes_focus_modifiers.md
spec_magic_status_application_and_vulnerability_matching.md
spec_magic_staff_wand_scroll_integration.md
spec_magic_healing_barrier_purification_runtime.md
spec_magic_elemental_damage_runtime.md
spec_magic_hud_cast_preview_feedback.md
spec_magic_save_load_known_spells_slots_cooldowns.md
spec_magic_balance_playtest_profile.md
```

## 10. Specs de companions

Fontes obrigatórias:

```text
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
```

Specs de companions que envolvam interação com pets devem ler também:

```text
docs/design/gameplay/pets/PETS_DIRECTION.md
```

Specs de companions que envolvam romance, casamento, poliamor, partner companion unlock, spouse/partner helper ou social bond futuro devem ler também:

```text
docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
```

Usar para specs de:

```text
CompanionDataSO
CompanionEligibilitySO
CompanionBrainProfileSO
CompanionRoleProfileSO
CompanionAbilitySO
CompanionJobProfileSO
CompanionBondProfileSO
CompanionCaveProfileSO
CompanionFarmProfileSO
CompanionScheduleOverrideSO
CompanionRecoveryProfileSO
CompanionHUDProfileSO
CompanionBalanceProfileSO
active companion
companion unlock
companion availability
companion farm jobs
companion cave follow/leash
companion combat assist
companion downed/injury/recovery
companion bond progression
companion save/load
```

Specs recomendadas derivadas:

```text
spec_companion_data_contract.md
spec_companion_unlock_availability_runtime.md
spec_companion_farm_jobs_runtime.md
spec_companion_cave_follow_and_leash_runtime.md
spec_companion_combat_assist_runtime.md
spec_companion_healing_guardian_support_limits.md
spec_companion_downed_injury_recovery_runtime.md
spec_companion_bond_progression_runtime.md
spec_companion_hud_feedback.md
spec_companion_save_load_state.md
spec_companion_balance_playtest_profile.md
```

## 11. Specs de pets em combate/fazenda

Fontes obrigatórias:

```text
docs/design/gameplay/pets/PETS_DIRECTION.md
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
```

Specs de pets que envolvam NPC reaction, partner-pet interaction, social hooks, romance/casamento ou visitas sociais devem ler também:

```text
docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
```

Usar para specs futuras de:

```text
PetDataSO
PetBondProfileSO
PetMoodProfileSO
PetFoodDataSO
PetRoutineProfileSO
PetFarmBehaviorProfileSO
PetCaveBehaviorProfileSO
PetAlertProfileSO
PetTreasureHintProfileSO
PetTrapHintProfileSO
PetHUDProfileSO
PetSaveProfileSO
pet home area
pet bed/bowl/toy
pet bond/mood/energy
pet follow/leash/safe spawn
pet cave alert
pet treasure/trap hints
pet light interrupt
pet save/load
```

Specs recomendadas futuras:

```text
spec_pet_data_contract.md
spec_pet_home_area_bed_bowl_runtime.md
spec_pet_bond_mood_energy_runtime.md
spec_pet_follow_home_routine_runtime.md
spec_pet_farm_alerts_and_foraging_hints.md
spec_pet_cave_follow_leash_safe_spawn.md
spec_pet_cave_alerts_treasure_trap_hints.md
spec_pet_light_interrupt_runtime.md
spec_pet_hud_icons_feedback.md
spec_pet_save_load_state.md
```

## 12. Specs de HUD gameplay/combat

Fontes obrigatórias centrais para UI/HUD/menus/input/foco/feedback:

```text
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
```

Fontes obrigatórias de sistemas:

```text
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
docs/design/gameplay/pets/PETS_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
```

Specs de HUD/UI que envolvam quest log, Fonte, social log, romance, companions/pets, fragmentos, final choices ou feedback de main quest devem ler também:

```text
docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
```

### Specs de menu screen flows e submenus derivados

Fontes obrigatórias:

```text
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
```

Specs que envolvam menus, submenus, drawers, screen flows, inventory screen, storage/chest screen, equipment screen, weapon/armor detail, item detail, tooltip expandido, repair/upgrade UI, skill tree screen, skill node detail, active slot assignment, spell detail, shop buy/sell screen, crafting screen, quest detail, social/NPC detail future, calendar day detail, Fonte menu, confirmation modal, empty state, error state, focus order ou menu navigation devem ler obrigatoriamente essas fontes.

---

# PARTE H — Loot / Crafting / Economy

## 13. Specs de loot, crafting e economia

Fontes obrigatórias:

```text
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
docs/design/gameplay/pets/PETS_DIRECTION.md
docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_ENEMY_BEHAVIOR_ADAPTER.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
```

Specs de loot/economia que envolvam Mana, Água Viva, Pedra Negra estabilizada/cultista, fragmentos de Anya, Fonte, rewards de boss da main quest ou final choices devem ler também:

```text
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
```

Specs que envolvam restock diário/semanal/sazonal, preço por festival, demanda sazonal, mercador raro de Finan, loja noturna por Nyx, seed/crop raro por Alihana, item mágico instável por Senya, orders/encomendas por prazo ou SellPoint por day transition devem ler também:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
```

Regra:

```text
SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md define quando eventos sazonais/climáticos/lunares podem solicitar variação.
ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md vence para preço, BaseValue, restock, SellPoint, stock state e anti-arbitragem.
```

Usar para specs de:

```text
ItemDefinitionSO
ItemCategorySO
ItemTag
QualityProfileSO
RarityProfileSO
PricingProfileSO
RestockPolicySO
StockLineSO
ShopInventorySO
ShopPriceRulesSO
ShippingPriceProfileSO
ResourceRefreshProfileSO
LootTableSO
EnemyDropTableSO
BossDropTableSO
TreasureTableSO
MiningNodeTableSO
ForageTableSO
FishingTableSO
RecipeSO
CraftingStationSO
ProcessingRecipeSO
OrderSO
RewardTableSO
EconomyBalanceProfileSO
ItemStack
ItemInstance
storage
shipping bin
pending payments
shop inventory
shop stock state
refresh/restock
orders/encomendas
reputation rewards
gold economy
repair costs
upgrade costs
loot rarity
quality
crafting timers
processing timers
spell tomes
learnable scrolls
cast scrolls
wands
focuses
SpellUnlockSourceSO
companion job output
companion quest rewards
pet food
pet toys
pet bed/bowl
pet-related shop items
BaseValue
BuyPrice
SellPrice
ShopSellToPlayerPrice
ShopBuyFromPlayerPrice
anti-arbitrage
resource node refresh
cave loot refresh
boss first-time/repeat reward
```

Specs recomendadas derivadas:

```text
spec_item_definition_so_contract.md
spec_item_tags_categories_quality_rarity.md
spec_loot_table_so_contract.md
spec_enemy_drop_tables_by_family.md
spec_cave_treasure_mining_loot_tables.md
spec_recipe_so_and_crafting_station_contract.md
spec_processing_recipes_and_timers.md
spec_shop_inventory_price_rules.md
spec_shipping_bin_pending_payment_runtime.md
spec_inventory_stack_instance_storage_rules.md
spec_economy_balance_profile.md
spec_repair_upgrade_cost_rules.md
spec_bows_arrows_scrolls_wands_item_instances.md
spec_pricing_profile_so_contract.md
spec_shop_inventory_stockline_restock_contract.md
spec_shop_buy_sell_price_runtime.md
spec_sellpoint_shipping_price_pending_payment_runtime.md
spec_resource_node_refresh_runtime.md
spec_cave_loot_refresh_snapshot_rules.md
spec_item_base_values_initial_tables.md
spec_economy_anti_arbitrage_tests.md
```

---

# PARTE I — Quests / Main Progression / Social Future

## 14. Specs de quests, main progression e narrativa jogável

Fontes obrigatórias para qualquer spec de quests/main progression:

```text
docs/design/gameplay/quests/QUESTS_LORE_WEAVING_BRIEF.md
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
```

Specs de quests que envolvam cidade, NPCs, Padre Corvus, Vaelrion, Sethra, Yael, loja noturna, templo de Kanthor, Litania do Primeiro Retorno, social hooks ou relação com romance/companions devem ler também:

```text
docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
```

Specs de quests que envolvam fazenda, Fonte, Água Viva, Mana plantável, pedreira final, visitas à fazenda ou estado da fazenda devem ler também:

```text
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
```

Specs de quests que envolvam caverna, nível 100/101, boss gates, Arco da Memória, Arquivista do Silêncio, boss final, checkpoints, Pedra Negra, Bromécia ou Elyndor devem ler também:

```text
docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
docs/design/gameplay/cave/CAVE_LEVEL_GENERATION_LAYOUT_BIOME_DIRECTION.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
```

Specs de quests que envolvam itens, rewards, Água Viva, Mana, Pedra Negra, boss rewards, loja noturna, economia de final ou crafting devem ler também:

```text
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
```

Specs de social/romance/casamento/poliamor são futuras, não atuais. Quando entrarem explicitamente no roadmap, fontes obrigatórias:

```text
docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
docs/design/gameplay/pets/PETS_DIRECTION.md
```

Specs recomendadas futuras de social:

```text
spec_social_relationship_profile_contract_future.md
spec_social_friendship_trust_runtime_future.md
spec_social_gift_preferences_reactions_future.md
spec_social_dialogue_conditions_memory_future.md
spec_social_personal_quests_future.md
spec_social_romance_route_runtime_future.md
spec_social_poly_relationship_runtime_future.md
spec_social_marriage_ceremony_state_future.md
spec_social_partner_companion_unlock_future.md
spec_social_partner_helper_farm_runtime_future.md
spec_social_farm_visits_runtime_future.md
spec_social_festivals_dates_birthdays_future.md
spec_social_hud_log_feedback_future.md
spec_social_save_load_state_future.md
spec_social_balance_anti_exploit_future.md
```

Specs recomendadas futuras de main quest:

```text
spec_quest_main_state_contract_future.md
spec_quest_main_act1_fonte_esquecimento_future.md
spec_quest_main_fragmento_agua_future.md
spec_quest_main_act2_cindar_arco_memoria_future.md
spec_quest_main_fragmento_memoria_future.md
spec_quest_main_act3_culto_pedra_negra_vida_future.md
spec_quest_main_fragmento_vida_future.md
spec_quest_main_act4_nivel_100_101_esperanca_future.md
spec_quest_main_arquivista_silencio_boss_future.md
spec_quest_main_final_choices_proteger_selar_usar_future.md
spec_quest_main_fonte_progression_state_future.md
spec_quest_main_luas_event_hooks_future.md
spec_quest_main_city_memory_events_future.md
```

Specs de quests que envolvam condição temporal, calendário, estação, clima, previsão, festival, evento lunar, Alihana, Senya, Nyx, Fonte reagindo a lua/clima, Água Viva recarregando por lua, Mana florescendo por condição temporal, diário legível por Alihana, loja noturna por Nyx, ritual instável por Senya ou main quest condicionada por alinhamento lunar devem ler também:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
```

Regra:

```text
Quest obrigatória não deve depender de evento raro sem calendário, pista e forma razoável de esperar.
```

---

# PARTE K — World / Time / Calendar / Weather / Lunar

## Specs de tempo, calendário, clima e luas

Fontes obrigatórias:

```text
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
```

Specs que envolvam tempo, calendário, estações, clima, chuva, neve, tempestade, névoa, calor, frio, previsão, passagem de dia, sono, colapso por horário, day transition, festivais, eventos lunares, Alihana, Senya, Nyx, crops sazonais, crops lunares, Mana por estação/lua, Fonte reagindo a clima/lua, schedules dependentes de clima/lua ou eventos temporais devem ler obrigatoriamente esta fonte.

Specs futuras recomendadas:

```text
spec_time_clock_day_transition_runtime.md
spec_calendar_season_year_runtime.md
spec_weather_generation_forecast_runtime.md
spec_rain_irrigation_crop_integration.md
spec_weather_farm_pet_companion_reactions.md
spec_lunar_cycle_event_runtime.md
spec_lunar_fonte_mana_reactions_future.md
spec_calendar_festivals_events_runtime.md
spec_npc_schedule_weather_lunar_modifiers.md
spec_shop_calendar_weather_lunar_modifiers.md
spec_cave_weather_lunar_modifiers_future.md
spec_calendar_ui_weather_lunar_display.md
spec_time_save_load_state.md
spec_calendar_quest_temporal_conditions.md
```

---

# PARTE M — Save / Load / Full State

## Specs de save/load e persistência full-state

Fontes obrigatórias:

```text
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
```

Specs que envolvam save/load, GameSaveData, SaveManager, migration, schema, backup, escrita segura, providers, save sections, capture/restore, restore order, save em caverna, save cross-scene, DTOs, IDs persistidos, defaults de seção, validação de save, preservação de seção fora de cena, UI state não persistível ou qualquer novo estado persistido devem ler obrigatoriamente esta fonte.

## Specs de sistemas com estado persistido

Specs que adicionem ou alterem estado persistido em player, inventory, equipment, hotbar, skill tree, active slots, farm, world, time/calendar/weather/lunar, economy, crafting, NPCs, quests, main progression, Fonte, cave, death/corpse recovery, bestiary, pets, companions ou social devem ler:

```text
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
```

Regra:

```text
Nenhuma spec futura pode adicionar estado persistido sem declarar seção de save, dono, IDs usados, capture policy, preserve policy, restore order, migration necessária ou justificativa de não precisar, defaults e validação mínima.
```

## Regras por domínio

**Tempo/calendário/clima/luas:**

```text
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
```

**Quests, main progression e Fonte:**

```text
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
```

**Inventory, item instances e equipment:**

```text
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
```

**Cave / death / corpse recovery:**

```text
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
```

**Bestiary / knowledge discovery:**

```text
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
```

**Pets, companions e social futuro:**

```text
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
docs/design/gameplay/pets/PETS_DIRECTION.md
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
```

**UI/HUD/menus:**

```text
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
```

Regra:

```text
UI state não é gameplay state e não deve ser salvo no save principal.
HUD deriva do runtime restaurado.
Preferências de UI/acessibilidade podem ser salvas futuramente fora do gameplay save.
```

## Specs futuras recomendadas

```text
spec_save_provider_architecture_runtime.md
spec_save_restore_order_contract_runtime.md
spec_save_section_ownership_registry.md
spec_save_time_calendar_weather_lunar_state_future.md
spec_save_quest_main_fonte_state_future.md
spec_save_item_instance_equipment_upgrade_state_future.md
spec_save_pet_companion_social_state_future.md
spec_save_bestiary_knowledge_state_future.md
spec_save_cave_policy_checkpoint_future.md
spec_save_playmode_validation_matrix.md
spec_save_invalid_id_fallback_rules.md
```

---

# PARTE N — Bestiary / Knowledge Discovery

## Specs de bestiário e descoberta de conhecimento

Fontes obrigatórias:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
```

Specs que envolvam bestiário, conhecimento de inimigos, descoberta de vulnerabilidades, identificação de criaturas, knowledge states, enemy knowledge save/load, drops conhecidos, resistências conhecidas, imunidades conhecidas, behavior windows descobertas, lore notes de criaturas, NPC/livro/quest desbloqueando conhecimento, equipment tooltip com known enemy interactions, spell tooltip com known effectiveness, Bestiary HUD futura, Bestiary Menu futuro ou spoiler control de inimigos devem ler obrigatoriamente esta fonte.

## Specs de inimigos/caverna

Specs que envolvam EnemyBestiaryEntrySO, EnemyKnowledgeState, EnemyDataSO com BestiaryEntryId, enemy families, cave roster conversion, boss entries, creature variants, faction knowledge, behavior observed, attacks observed, boss phase observed ou spoiler tier devem ler:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
```

## Specs de combate/vulnerabilidades

Specs que envolvam descoberta de ElementVulnerability, StatusVulnerability, AttackTypeVulnerability, WeaponVulnerability, MaterialVulnerability, BehavioralVulnerabilityWindow, ResistanceTags, ImmunityTags ou feedback de efetividade devem ler:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
```

Regra:

```text
Bestiary não cria vulnerabilidade; apenas revela conhecimento sobre vulnerabilidades já autoradas.
```

## Specs de equipment/spell tooltips

Specs que envolvam known enemy interactions em Equipment UI, Weapon Detail, Armor Detail, Material interaction display, Spell known effectiveness, tooltip expandido, comparison drawer ou ocultação de vulnerabilidades desconhecidas devem ler:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
```

Regra:

```text
Equipment UI e Spell UI só mostram interações conhecidas ou autorizadas por fonte legítima de conhecimento.
```

## Specs de loot/crafting/economy

Specs que envolvam drops conhecidos, rare drops ocultos, origem conhecida de material, fonte de crafting item, boss first-time/repeat rewards revelados ou quest/encomenda que revela drop source devem ler:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
```

## Specs de quests/main quest/lore

Specs que envolvam quest de pesquisa, identificar criatura, coletar amostra, descobrir fraqueza, confirmar rumor, documentar comportamento, pesquisar Pedra Negra, entender criatura corrompida, Arquivista do Silêncio, nível 100/101, boss spoiler control ou conhecimento concedido por NPC/livro/ruína devem ler:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
```

## Specs de UI/HUD futura

Specs que envolvam Bestiary Menu, Bestiary HUD, Knowledge Log, Bestiary notifications, Bestiary entry detail, filters, KnownEnemyInteractions, Bestiary spoiler control, Knowledge confidence ou enemy entry cards devem ler:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
```

Regra:

```text
Bestiary UI/HUD completa é futura e não entra na primeira entrega executável atual.
```

## Specs de pets/companions/NPC knowledge

Specs que envolvam pet hints, companion knowledge hints, NPC teaching, books, research service, rumor confidence, partial/confirmed knowledge ou social unlocks de conhecimento devem ler:

```text
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
docs/design/gameplay/pets/PETS_DIRECTION.md
docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
```

## Specs futuras recomendadas

```text
spec_bestiary_entry_data_contract_future.md
spec_enemy_knowledge_state_save_load_future.md
spec_bestiary_discovery_events_runtime_future.md
spec_bestiary_ui_menu_future.md
spec_bestiary_hud_notifications_future.md
spec_equipment_tooltip_known_interactions_future.md
spec_spell_tooltip_known_effectiveness_future.md
spec_bestiary_npc_books_quest_unlocks_future.md
spec_bestiary_boss_spoiler_control_future.md
spec_bestiary_pet_companion_hints_future.md
spec_bestiary_research_service_future.md
```

---

# PARTE O — Quest / Objective / Event System

## Specs de sistema genérico de quests/objectives/events

Fontes obrigatórias:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
```

Specs que envolvam quest system, QuestId, QuestDefinition, QuestState, QuestStep, Objective, Condition, Trigger, Reward, QuestFlag, QuestEvent, branching, failure, expiry, quest UI, quest log, quest save/load, anti-softlock, anti-spoiler, main quest hooks, Fonte hooks, time/weather/lunar hooks, NPC/dialogue hooks, farm orders, festival quests, cave contracts, hidden quests ou tutorial quests devem ler obrigatoriamente esta fonte.

## Specs de main quest

Specs de main quest que envolvam atos, fragmentos, Fonte, Cindar, Arco da Memória, Pedra Negra, nível 100/101, Arquivista do Silêncio, final choices, gates, events ou quest states devem ler:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
```

Regra:

```text
Main quest lore/progression continuam em documentos próprios.
QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md define o sistema genérico de steps, objectives, conditions, triggers, rewards e flags usado por essas quests.
```

## Specs de quest UI/log

Specs que envolvam Quest Log, Quest Detail, objective visibility, tracked quest, hidden quest, known hints, quest notification, spoiler control ou quest log HUD devem ler:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
```

Regra:

```text
Quest Log não revela objetivo, reward, boss, condição ou final oculto cedo.
```

## Specs de save/load de quests

Specs que envolvam QuestState, CurrentStepId, ObjectiveStates, KnownObjectiveIds, KnownHints, StartedAtDay, CompletedAtDay, ExpiresAtDay, Tracked, Discovered, ChoiceHistory, GrantedRewardIds, GrantedFlagIds ou QuestFlags devem ler:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
```

Regra:

```text
QuestState, MainProgression e FonteAnya são seções separadas de save/load.
QuestFlag não substitui QuestState.
```

## Specs de tempo, clima, lua e festivais em quests

Specs que envolvam WaitForTime, WaitForDay, WaitForSeason, WaitForWeather, WaitForLunarEvent, AttendFestival, FestivalStarted, FestivalEnded ou quest expiry por calendário devem ler:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
```

Regra:

```text
Quest obrigatória com tempo/lua precisa dar pista e controle razoável.
Main quest não expira por tempo.
FarmOrder e Festival quests podem expirar se prazo for claro.
```

## Specs de NPC/dialogue hooks

Specs que envolvam diálogo iniciando quest, diálogo avançando step, DialogueChoiceBranch, NPC availability, NPC schedule integration, NPC teaching, service unlocks ou quest flags por diálogo devem ler:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
```

## Specs de farm/city/cave objectives

Specs que envolvam FarmOrder, crop objective, shipping objective, city board, cave contract, enemy defeat objective, cave depth objective, corpse recovery objective, Fonte use objective ou world interaction objective devem ler:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
```

## Specs de bestiary/knowledge objectives

Specs que envolvam DiscoverBestiaryKnowledge, DiscoverWeakness, ConfirmRumor, DocumentBehavior, CollectSample, ReadLoreNote ou knowledge unlock rewards devem ler:

```text
docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md
```

## Specs futuras recomendadas

```text
spec_quest_objective_event_contract_runtime.md
spec_quest_state_save_load_runtime.md
spec_quest_condition_trigger_runtime.md
spec_quest_reward_application_idempotency_runtime.md
spec_quest_log_visibility_spoiler_runtime.md
spec_quest_flags_registry_runtime.md
spec_quest_farm_orders_adapter_runtime.md
spec_quest_festival_expiry_runtime_future.md
spec_quest_bestiary_discovery_objectives_future.md
spec_quest_fonte_main_progression_hooks_future.md
spec_quest_debug_validation_tools_future.md
spec_quest_anti_softlock_validation_future.md
```

---

# PARTE FABLE — Catálogos canonizados (2026-06-12)

Decisões humanas vinculantes: `docs/design/FABLE_DECISOES_RESPOSTAS_v1.0.md`.
Novos documentos canônicos de catálogo (em conflito de MODELO, a direction-mãe vence;
em conflito de LISTA/NÚMERO, o catálogo vence):

```text
docs/design/gameplay/cave/CAVE_BESTIARY_CATALOG_DIRECTION_v1.0.md
  60 criaturas + 4 chefes finais, stat blocks, renomeações (Veilkin/Gravedelver).
docs/design/gameplay/loot_crafting_economy/ITEM_CATALOG_DIRECTION_v1.0.md
  ~118 itens nominais com BaseValue; qualidade de crops como itens separados.
docs/design/gameplay/quests/QUEST_CATALOG_DIRECTION_v1.0.md
  ~86 quests (5 fontes, incl. secretas da caverna), conexões, XP escalado.
docs/design/gameplay/combat/BALANCE_CURVES_DIRECTION_v1.0.md
  level cap 100, curva de XP, multiplicadores por tipo, TTK, validação de dano.
docs/design/gameplay/combat/SKILL_ACTION_MOVEMENT_TABLE_DIRECTION_v1.0.md
  execução física das skills ativas (telegraph/lunge/shape/recovery).
docs/design/gameplay/ui_ux/HUD_LAYOUT_SCENES_DIRECTION_v1.0.md
  layout HUD/minimapa/abas; cenas (Town 48×42, Cave 55×55±, lotes da farm).
docs/design/gameplay/FABLE_SYSTEMS_DEEPENING_DIRECTION_v1.0.md (v1.1)
  síntese do corpus + gaps de definição restantes.
```

Specs que toquem bestiário/itens/quests/balance/HUD/cenas devem ler o catálogo correspondente.

---

# PARTE J — Regra anti-regressão

## 15. Quando houver conflito

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
Enemy Behaviors é fonte canônica geral de Move, BehaviorProfile, Trait, EnemyAction e EnemyBrain.
Cave Monster Roster é fonte canônica das criaturas concretas, stats, drops, packs, bosses e scaling da caverna.
Cave Monster Roster Enemy Behavior Adapter é ponte obrigatória para converter o roster em runtime.
Status Effects é fonte canônica do significado mecânico de Bleed, Burn, Chill, Poison, Stun, Root, Fear, ConfusionLite, DurabilityStress e Corruption.
Equipment Weapons Armor Materials é fonte canônica de direção geral de armas, armaduras, escudos, materiais, durabilidade, upgrades e balance de gear contra famílias de inimigos.
Equipment Mechanical Baselines é fonte canônica de baseline mecânico inicial: WeaponDamage, ASPD, scaling por atributo, StaminaCost, charged effects, range, armor, shield, arrows, wands, scrolls, tomes e focuses.
Equipment Enemy Vulnerability Adapter é fonte canônica da ponte entre tags de equipamentos e vulnerabilidades de inimigos, incluindo MaterialVulnerability, sem duplicar matriz por família.
Magic Spells Actions é fonte canônica da lista enxuta de spells, SpellActionDataSO, spell shapes, MP costs, cast time, spell unlocks, spell scaling, staff/wand/scroll/tome/focus integration e spell HUD.
Magic Learning Unlocks Sources é fonte canônica para spell source, LearnableScroll, CastScroll, Tome, Wand, Staff/Weapon spell, Focus, NPC teaching, Fonte de Anya story unlock, knownSpellIds, ItemProvided e ConsumableProvided.
Companions é fonte canônica do sistema de companions, companion eligibility, active companion, farm jobs, cave follow/leash, companion combat assist, downed/injury/recovery, bond, HUD e save/load.
Pets é fonte canônica do sistema de pets, pet bond, pet mood/energy, pet food, pet home area, pet bed/bowl/toy, pet routine, pet farm alerts, pet cave follow/leash, pet treasure/trap hints, pet light interrupt, HUD e save/load.
Social Relationship Romance é fonte canônica futura de friendship, trust, gift, romance, casamento, casamento poliamoroso consentido até 3 parceiros, partner companion unlock, partner helper e social memory.
Quests Main Lore é fonte canônica da lore principal: Anya fragmentada e não restaurável por completo, Cindar como última sacerdotisa Nymiriana local, Fonte por fragmentos, Arco da Memória, Vaelrion Aelth-Silberharth, Sethra Veyl-Nocthar, Litania do Primeiro Retorno e Arquivista do Silêncio.
Quests Main Progression Refinement é fonte canônica de progressão jogável da main quest: 4 atos, fragmentos Água/Memória/Vida/Esperança, Fonte como respawn/Água Viva/respec/purificação/decisão final e finais Proteger/Selar/Usar.
Loot Crafting Economy é fonte canônica de fluxos de itens, loot tables, recipes, quality, rarity, shops, orders, shipping, storage, ItemStack, ItemInstance e economy balance.
Economy Pricing Stock Refresh é fonte canônica de BaseValue, BuyPrice, SellPrice, ShopStockState, StockLineSO, RestockPolicySO, SellPoint, ShippingPriceProfileSO, ResourceRefreshProfileSO, anti-arbitrage, cave loot refresh e boss first-time/repeat reward.
MaterialVulnerability deve existir em specs futuras de EnemyDataSO.
Vulnerabilidades por família/inimigo ficam em Cave Combat Balance, Cave Monster Roster e EnemyDataSO futuro; não em documentos de equipment.
Quality é diferente de Rarity.
Tier é diferente de Quality e Rarity.
Reputação é desbloqueio social/econômico, não moeda comum.
Fruto de Mana, Água Viva da Fonte e Pedra Negra estabilizada não são commodities comuns.
Mana pode ser plantado/cultivado em condições raras, mas não é crop comum, não é plantio em massa e não deve quebrar economia/cura/MP/atributos.
Anya não pode ser restaurada por completo na main quest; o objetivo é proteger, selar ou usar os fragmentos restantes.
Fonte de Anya evolui por fragmentos e não deve liberar Água Viva, respec, purificação ou decisão final cedo demais.
Magia não é concedida automaticamente só por level ou skill point.
Skill tree libera capacidade/domínio; fonte libera spell.
LearnableScroll ensina permanentemente se pré-requisitos forem cumpridos.
CastScroll casta e consome, mas não ensina.
Wand/Staff/arma/focus podem fornecer magia temporária enquanto equipados, sem adicionar knownSpellIds por padrão.
Magias podem ter nomes de deuses, mas tipo mecânico é definido por DamageType, SpellCategory, SpellShape, Tags e Scaling.
Corruption/Nyx em magia é risco late/endgame, não sistema obrigatório inicial.
Companion ajuda, mas não joga pelo jogador.
Baseline de caverna é 1 companion ativo.
Pet é sistema separado e não conta como companion.
Pet não tem Breath/Fôlego.
Pet não substitui companion, build, skill tree, execução de combate ou ferramentas.
Pet não gera rolagem separada de loot por padrão.
Pet não deve puxar packs novos sozinho.
Pet não resolve boss.
Pet light interrupt, trap hint e treasure hint são suporte leve e limitado.
Companion não tem Breath/Fôlego.
Companion não gera rolagem separada de loot por padrão.
Companion equipment completo é futuro, não MVP.
Romance/casamento não devem ser caminho obrigatório de poder.
Romance homoafetivo é permitido para NPCs elegíveis.
Casamento poliamoroso consentido é permitido até 3 parceiros totais, mas é feature futura e não entra nas specs atuais sem decisão de roadmap.
Parceiros românticos/cônjuges podem virar companions futuros se elegíveis, mas baseline de caverna continua 1 companion ativo por run.
Partner helper é futuro e deve ter orçamento global; 3 parceiros aumentam variedade, não produção linear.
BaseValue é obrigatório para item vendável.
Preço final é recalculável e não deve ser persistido como fonte primária.
ShopSellToPlayerPrice deve ser maior que ShopBuyFromPlayerPrice salvo exceção limitada.
Restock não acontece ao abrir menu.
UniqueStock não repõe.
LimitedStock persiste counters.
Itens vendidos pelo jogador não entram automaticamente no estoque da loja no baseline.
SellPoint processa no day transition e não é loja.
Cave loot usa run/snapshot/floor generation conforme regras da caverna.
Boss first-time reward deve ser separado de repeat reward.
Farm invasion, town hostile events e world enemy events são backlog futuro; não entram em specs atuais sem nova decisão de roadmap.
Hearing/audição não é elemento atual de IA inimiga.
UI_UX_FULL_GAMEPLAY_DIRECTION.md é fonte canônica de apresentação, layout, input routing, foco, modal, navegação, tooltip, feedback visual/sonoro, notificações, HUD e debug HUD.
Documentos de sistema definem o que precisa ser comunicado; UI_UX_FULL_GAMEPLAY_DIRECTION.md define como comunicar.
UI modal bloqueia input de gameplay.
WASD não move o personagem enquanto Dialogue, Inventory, Equipment, SkillTree, Crafting, Shop, QuestLog, SocialLog, Calendar, Fonte menu ou System menu estiver aberto.
HUD final não mostra Breath/Fôlego/BR.
Buy/Sell UI deve separar inventário da loja e inventário vendável do jogador.
Empty state de loja/inventário deve ser explícito.
SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md é fonte canônica de tempo, calendário, estações, clima, chuva, neve, tempestade, névoa, eventos lunares, Alihana, Senya, Nyx, festivais e impactos sistêmicos globais.
CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md continua vencendo para rotina individual de NPC.
UI_UX_FULL_GAMEPLAY_DIRECTION.md continua vencendo para apresentação de relógio, calendário, clima, lua, notificações e feedback visual.
ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md continua vencendo para preço, BaseValue, restock, SellPoint e anti-arbitragem.
O jogo usa 4 estações por ano, 28 dias por estação, 112 dias por ano e 7 dias por semana.
Dia jogável começa às 06:00.
02:00 é limite padrão de colapso/sono forçado.
Escala inicial recomendada: 1 hora in-game = 60 segundos reais.
Tempo pausa em UI modal.
Chuva molha áreas externas.
As três luas são sistemas de gameplay, não decoração.
Alihana = memória, sonhos, Fonte, Cindar, Água Viva.
Senya = caos, magia, mutação, instabilidade.
Nyx = noite, segredo, loja noturna, Pedra Negra, esquecimento.
Mana pode depender de estação/lua/Água Viva/Fonte, mas nenhuma condição isolada basta.
Quest obrigatória com tempo/lua precisa dar pista e controle razoável ao jogador.
UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md é fonte canônica de menus, submenus, drawers, screen flows, detail panels, comparison drawers, focus order, confirmation patterns, empty states e error states.
UI_UX_FULL_GAMEPLAY_DIRECTION.md continua vencendo para princípios gerais de UI/UX, input routing, modal focus, HUD e feedback.
Todo menu modal bloqueia WASD e input de gameplay.
Diálogo não deixa o personagem andar.
Shop Sell sempre mostra inventário vendável do jogador ou empty state explícito.
Buy e Sell nunca usam a mesma lista de dados.
Skill node sempre tem detail drawer antes de gastar ponto.
Gastar SkillPoint exige confirmação visual clara.
Active skill comprada não equipa automaticamente em slot cheio.
Equipment compare nunca equipa por hover ou foco.
Repair e Upgrade são ações diferentes.
Quest/Key item não pode ser vendido/descartado por padrão.
Tooltip curto não substitui detail drawer para informação complexa.
Fonte Menu não mostra função ainda não desbloqueada por fragmento.
Calendar não revela segredo antes da descoberta.
Social future não transforma NPC em planilha.
HUD final não mostra debug.
SAVE_LOAD_FULL_STATE_DIRECTION.md é fonte canônica de persistência full-state, capture/restore, providers, section ownership, restore order, cross-scene preservation e regras de save/load para specs futuras.
Save/load não deve ser refeito do zero.
SaveManager é o orquestrador atual.
Provider architecture é alvo gradual, não refactor massivo imediato.
Não trabalhar com números futuros de schema neste direction.
Cada spec futura define migration se alterar payload persistido.
Save em caverna continua permitido por enquanto.
Política final de save em caverna será refinada depois.
DTOs de save usam IDs e tipos simples.
Não serializar referências Unity.
UI state não é gameplay state e não deve ser salvo no save principal.
HUD deriva do runtime restaurado.
Salvar fora da cena de um sistema não pode apagar a seção desse sistema.
Seção ausente em save legado precisa de default seguro ou migration.
QuestState, MainProgression e FonteAnya devem ser seções separadas.
Pet, Companion e Social futuro devem ser seções separadas.
Bestiary knowledge futuro deve persistir conhecimento descoberto sem revelar spoiler.
Fonte não deve ficar persistida apenas dentro de FarmSection.
Schedules de NPC devem ser recalculados por tempo/clima/lua, não salvos como pathfinding transitório.
Preço final calculado não é fonte primária de save.
Migration falha não pode corromper save original.
BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md é fonte canônica de bestiário, descoberta de conhecimento, estados de conhecimento, revelação de vulnerabilidades, drops conhecidos, spoiler control e HUD/UI futura de bestiário.
Bestiary não entra na primeira entrega executável atual.
Bestiary é sistema de descoberta, não lista completa revelada de início.
Bestiary não cria vulnerabilidade; apenas revela conhecimento sobre vulnerabilidades já autoradas.
Equipment UI não revela vulnerabilidade desconhecida.
Spell UI não revela resistência/imunidade desconhecida.
Drop raro não descoberto não aparece com nome completo.
Derrotar uma criatura uma vez não precisa revelar tudo.
Ver uma criatura não revela fraquezas automaticamente.
NPC/livro/quest pode revelar rumor, partial ou confirmed knowledge.
Pet/companion pode dar hint, mas não completa bestiário sozinho.
Bosses e main quest têm spoiler control.
Arquivista do Silêncio não aparece completo antes da revelação apropriada.
Pedra Negra não revela natureza completa cedo.
Conhecimento descoberto deve ser persistido em save/load.
HUD de bestiário futura não deve virar planilha em combate.
Bestiary UI futura deve mostrar apenas conhecimento conhecido ou autorizado pelo SpoilerTier.
QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md é fonte canônica de sistema genérico de quests, objectives, conditions, triggers, rewards, quest flags e quest events.
Main quest lore/progression continuam em documentos próprios.
Main quest, side quests, farm orders, social future, companion future, pet future, festival, cave contracts e tutorial usam o mesmo sistema base.
QuestState, MainProgression e FonteAnya são seções separadas de save/load.
QuestFlag não substitui QuestState.
Condition não avança quest sozinha; Trigger/event avança quando condições permitem.
Reward precisa ser idempotente.
Quest Log não revela objetivo, reward, boss, condição ou final oculto cedo.
Main quest não falha por tempo.
Main quest não pode ficar impossível por item vendido, NPC fora de schedule, clima/lua perdido, morte do jogador ou inventário cheio.
FarmOrder e Festival quests podem expirar se prazo for claro.
Hidden quest não aparece no log até ser descoberta.
Quest crítica precisa de anti-softlock.
Quest system consome eventos de gameplay; não substitui farm, combat, inventory, economy, bestiary ou Fonte.
```
