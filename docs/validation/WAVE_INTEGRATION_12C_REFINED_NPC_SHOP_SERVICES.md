# WAVE INTEGRATION 12C - Refined NPC Shop Services

Status: SERVICE_DATA_CREATED_WITH_ADVANCED_SERVICE_DEBT

20 NPCs have shop/service definitions. 17 are wired to `NpcShopController` in TownScene. Pip, Nimble and Thalindra keep dialogue-scene controllers with service data only because their service interaction needs non-final UI/flow. Alaric, Liora and Maelor have no initial shop per spec.

| NpcId | DisplayName | ServiceType | ShopId | SceneRuntimeStatus |
|---|---|---|---|---|
| npc_corvus | Padre Corvus | healing_blessing_temple | shop_corvus | SCENE_SHOP_RUNTIME |
| npc_mara | Mara Vellum | licenses_contracts | shop_mara | SCENE_SHOP_RUNTIME |
| npc_sylveth | Sylveth | seeds_crops_herbs | shop_sylveth | SCENE_SHOP_RUNTIME |
| npc_brumdar | Brumdar Ferro-Quieto | blacksmith_repair_upgrade | shop_brumdar | SCENE_SHOP_RUNTIME |
| npc_nimble | Nimble Galhobaixo | construction_buildings_move | shop_nimble | SERVICE_DATA_ONLY_DEBT |
| npc_gurd | Gurd Carvalho-Torto | heavy_clearance | shop_gurd | SCENE_SHOP_RUNTIME |
| npc_hund | Hund Carvalho-Torto | guard_transport | shop_hund | SCENE_SHOP_RUNTIME |
| npc_ozzra | Ozzra Fumacazul | alchemy_potions_fertilizer | shop_ozzra | SCENE_SHOP_RUNTIME |
| npc_gruta | Gruta Panela-Funda | tavern_food_rumors | shop_gruta | SCENE_SHOP_RUNTIME |
| npc_zrix | Zrix das Estradas | maps_cave_contracts | shop_zrix | SCENE_SHOP_RUNTIME |
| npc_yael | Yael Noite-Mansa | night_shop_rare_items | shop_yael | SCENE_SHOP_RUNTIME |
| npc_thalindra | Thalindra Veu-de-Lua | archive_lore_quests_blueprints | shop_thalindra | SERVICE_DATA_ONLY_DEBT |
| npc_dagna | Dagna Rocha-Morna | ore_mining_cave | shop_dagna | SCENE_SHOP_RUNTIME |
| npc_pip | Pip Semente-Solta | tutorial_delivery | shop_pip | SERVICE_DATA_ONLY_DEBT |
| npc_alaric | Ser Alaric Veyr | guard_combat_training |  | DIALOGUE_ONLY |
| npc_mirela | Mirela dos Lacos | tailor_bags_clothing | shop_mirela | SCENE_SHOP_RUNTIME |
| npc_renko | Renko Tres-Sorrisos | general_store_bargain | shop_renko | SCENE_SHOP_RUNTIME |
| npc_eiran | Eiran Valeclaro | animals_pets_feed | shop_eiran | SCENE_SHOP_RUNTIME |
| npc_liora | Liora Canta-Rio | music_dream_lore |  | DIALOGUE_ONLY |
| npc_orlan | Orlan Pouso-Curto | inn_lodging_news | shop_orlan | SCENE_SHOP_RUNTIME |
| npc_savra | Savra Escama-Verde | herbs_antidote_forest | shop_savra | SCENE_SHOP_RUNTIME |
| npc_tovin | Tovin Maos-de-Selo | permits_altars_registry | shop_tovin | SCENE_SHOP_RUNTIME |
| npc_maelor | Maelor Cinza | late_lore_memory |  | DIALOGUE_ONLY |
