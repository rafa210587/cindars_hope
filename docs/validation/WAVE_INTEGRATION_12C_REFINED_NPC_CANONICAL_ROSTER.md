# WAVE INTEGRATION 12C - Refined NPC Canonical Roster

Status: BUILD_VALIDATED_WITH_REFINED_NPC_DEBT

Source: `CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md` and WAVE12C attached spec.

| NpcId | DisplayName | Role | Zone | MovementProfile | ServiceType | ShopId | ImplementationStatus |
|---|---|---|---|---|---|---|---|
| npc_corvus | Padre Corvus | Curandeiro/Guardiao | Temple | Stationary/TemplePatrol | healing_blessing_temple | shop_corvus | SCENE_SHOP_RUNTIME |
| npc_mara | Mara Vellum | Escriba/Comerciante | Registry | Stationary/RegistryDesk | licenses_contracts | shop_mara | SCENE_SHOP_RUNTIME |
| npc_sylveth | Sylveth | Plantador/Curandeiro | SeedShop/Garden | ShopKeeperFixed/FarmVisit | seeds_crops_herbs | shop_sylveth | SCENE_SHOP_RUNTIME |
| npc_brumdar | Brumdar Ferro-Quieto | Artesao/Combatente | Forge | ShopKeeperFixed | blacksmith_repair_upgrade | shop_brumdar | SCENE_SHOP_RUNTIME |
| npc_nimble | Nimble Galhobaixo | Construtor/Artesao | Carpenter | Patrol/WorkshopDesk | construction_buildings_move | shop_nimble | SERVICE_DATA_ONLY_DEBT |
| npc_gurd | Gurd Carvalho-Torto | Construtor/Combatente | ConstructionYard | Patrol/HeavyWorkZone | heavy_clearance | shop_gurd | SCENE_SHOP_RUNTIME |
| npc_hund | Hund Carvalho-Torto | Guardiao/Construtor | GuardRoute | Patrol/TownRoad | guard_transport | shop_hund | SCENE_SHOP_RUNTIME |
| npc_ozzra | Ozzra Fumacazul | Alquimista/Artesao | AlchemyLab | WanderWithinZone/Lab | alchemy_potions_fertilizer | shop_ozzra | SCENE_SHOP_RUNTIME |
| npc_gruta | Gruta Panela-Funda | Comerciante/Musico | Tavern | ShopKeeperFixed/TavernStage | tavern_food_rumors | shop_gruta | SCENE_SHOP_RUNTIME |
| npc_zrix | Zrix das Estradas | Explorador/Comerciante | Guild/RoadGate | Patrol/CaveRoad | maps_cave_contracts | shop_zrix | SCENE_SHOP_RUNTIME |
| npc_yael | Yael Noite-Mansa | Comerciante/Explorador | NightMarket | NightOnly/WanderHidden | night_shop_rare_items | shop_yael | SCENE_SHOP_RUNTIME |
| npc_thalindra | Thalindra Veu-de-Lua | Pesquisador/Alquimista | Archive | Stationary/ArchiveDesk | archive_lore_quests_blueprints | shop_thalindra | SERVICE_DATA_ONLY_DEBT |
| npc_dagna | Dagna Rocha-Morna | Minerador/Combatente | Quarry/MineOffice | Patrol/QuarryRoad | ore_mining_cave | shop_dagna | SCENE_SHOP_RUNTIME |
| npc_pip | Pip Semente-Solta | Comerciante/Explorador | TownEntrance/Market | WanderWithinZone | tutorial_delivery | shop_pip | SERVICE_DATA_ONLY_DEBT |
| npc_alaric | Ser Alaric Veyr | Guardiao/Combatente | GuardPost | Patrol/TownGate | guard_combat_training |  | DIALOGUE_ONLY |
| npc_mirela | Mirela dos Lacos | Artesao/Comerciante | Tailor | ShopKeeperFixed | tailor_bags_clothing | shop_mirela | SCENE_SHOP_RUNTIME |
| npc_renko | Renko Tres-Sorrisos | Comerciante/Artesao | GeneralStore | ShopKeeperFixed | general_store_bargain | shop_renko | SCENE_SHOP_RUNTIME |
| npc_eiran | Eiran Valeclaro | Tratador/Plantador | AnimalYard | WanderWithinZone/AnimalArea | animals_pets_feed | shop_eiran | SCENE_SHOP_RUNTIME |
| npc_liora | Liora Canta-Rio | Musico/Pesquisador | Tavern/StatueGarden | WanderWithinZone/EveningStage | music_dream_lore |  | DIALOGUE_ONLY |
| npc_orlan | Orlan Pouso-Curto | Comerciante/Escriba | Inn | ShopKeeperFixed | inn_lodging_news | shop_orlan | SCENE_SHOP_RUNTIME |
| npc_savra | Savra Escama-Verde | Curandeiro/Explorador | Herbalist/ForestGate | Patrol/HerbRoute | herbs_antidote_forest | shop_savra | SCENE_SHOP_RUNTIME |
| npc_tovin | Tovin Maos-de-Selo | Escriba/Artesao | Registry | Stationary/PermitDesk | permits_altars_registry | shop_tovin | SCENE_SHOP_RUNTIME |
| npc_maelor | Maelor Cinza | Explorador/Pesquisador | StatueGarden/NightRoute | NightOnly/WanderHidden | late_lore_memory |  | DIALOGUE_ONLY |

Debt: final relationship, romance, reputation, daily schedules, personal quests, companion/pet systems and advanced service UIs remain out of scope.
