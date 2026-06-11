# WAVE INTEGRATION 25 - NPC Service, Shop, and Quest Matrix

Status: THALINDRA_QUEST_SHOP_PASS - ALL_20_SHOP_NPCS_WIRED

## Thalindra Quest+Shop Mandatory Check

| Check | Status | Evidence |
|---|---|---|
| npc_thalindra NpcDataSO exists | PASS | Npc_Thalindra.asset |
| npc_thalindra DialogueTreeSO exists | PASS | DialogueTree_Thalindra.asset (10 nodes) |
| npc_thalindra NpcShopController | PASS | WAVE12C - full shop runtime wired |
| quest_first_supplies_for_cindar | PASS | QuestRegistry (WAVE15) |
| Thalindra "! Qual e a tarefa?" | PASS | NpcShopController.ShowThalindraQuestShopDialogue() |
| Thalindra "Comprar" | PASS | NpcShopController case "buy" |
| Thalindra "Vender" | PASS | NpcShopController case "sell" |
| Thalindra "Adeus" | PASS | NpcShopController case "exit" |
| Quest offer → QuestGiverInteractedEvent | PASS | NpcShopController publishes event |
| Quest turn-in logic | PASS | QuestRuntimeBootstrap.QuestService.CanTurnIn() checked |

THALINDRA_QUEST_SHOP_FLOW: PASS (no blocking debt)

## Full NPC Shop/Service Matrix

| NpcId | DisplayName | ServiceType | ShopId | RuntimeStatus | Controller |
|---|---|---|---|---|---|
| npc_corvus | Padre Corvus | healing_blessing_temple | shop_corvus | SCENE_SHOP_RUNTIME | NpcShopController |
| npc_mara | Mara Vellum | licenses_contracts | shop_mara | SCENE_SHOP_RUNTIME | NpcShopController |
| npc_sylveth | Sylveth | seeds_crops_herbs | shop_sylveth | SCENE_SHOP_RUNTIME | NpcShopController |
| npc_brumdar | Brumdar Ferro-Quieto | blacksmith_repair_upgrade | shop_brumdar | SCENE_SHOP_RUNTIME | NpcShopController |
| npc_nimble | Nimble Galhobaixo | construction_buildings_move | shop_nimble | SHOP_RUNTIME_BASIC_WITH_ADVANCED_SERVICE_DEBT | NpcShopController |
| npc_gurd | Gurd Carvalho-Torto | heavy_clearance | shop_gurd | SCENE_SHOP_RUNTIME | NpcShopController |
| npc_hund | Hund Carvalho-Torto | guard_transport | shop_hund | SCENE_SHOP_RUNTIME | NpcShopController |
| npc_ozzra | Ozzra Fumacazul | alchemy_potions_fertilizer | shop_ozzra | SCENE_SHOP_RUNTIME | NpcShopController |
| npc_gruta | Gruta Panela-Funda | tavern_food_rumors | shop_gruta | SCENE_SHOP_RUNTIME | NpcShopController |
| npc_zrix | Zrix das Estradas | maps_cave_contracts | shop_zrix | SCENE_SHOP_RUNTIME | NpcShopController |
| npc_yael | Yael Noite-Mansa | night_shop_rare_items | shop_yael | SCENE_SHOP_RUNTIME | NpcShopController |
| npc_thalindra | Thalindra Veu-de-Lua | archive_lore_quests_blueprints | shop_thalindra | SHOP_RUNTIME_BASIC_WITH_ADVANCED_SERVICE_DEBT | NpcShopController |
| npc_dagna | Dagna Rocha-Morna | ore_mining_cave | shop_dagna | SCENE_SHOP_RUNTIME | NpcShopController |
| npc_pip | Pip Semente-Solta | tutorial_delivery | shop_pip | SHOP_RUNTIME_BASIC_WITH_ADVANCED_SERVICE_DEBT | NpcShopController |
| npc_alaric | Ser Alaric Veyr | guard_combat_training | - | DIALOGUE_ONLY | NpcController |
| npc_mirela | Mirela dos Lacos | tailor_bags_clothing | shop_mirela | SCENE_SHOP_RUNTIME | NpcShopController |
| npc_renko | Renko Tres-Sorrisos | general_store_bargain | shop_renko | SCENE_SHOP_RUNTIME | NpcShopController |
| npc_eiran | Eiran Valeclaro | animals_pets_feed | shop_eiran | SCENE_SHOP_RUNTIME | NpcShopController |
| npc_liora | Liora Canta-Rio | music_dream_lore | - | DIALOGUE_ONLY | NpcController |
| npc_orlan | Orlan Pouso-Curto | inn_lodging_news | shop_orlan | SCENE_SHOP_RUNTIME | NpcShopController |
| npc_savra | Savra Escama-Verde | herbs_antidote_forest | shop_savra | SCENE_SHOP_RUNTIME | NpcShopController |
| npc_tovin | Tovin Maos-de-Selo | permits_altars_registry | shop_tovin | SCENE_SHOP_RUNTIME | NpcShopController |
| npc_maelor | Maelor Cinza | late_lore_memory | - | DIALOGUE_ONLY | NpcController |

20 shop NPCs wired. 3 dialogue-only (alaric, liora, maelor).

## Known Debts

- SHOP_RUNTIME_BASIC_WITH_ADVANCED_SERVICE_DEBT: Pip (delivery), Nimble (construction), Thalindra (archive/blueprint) show basic shop but need custom service UI for full service experience.
- Final quest/social/reputation/romance systems: FUTURE_SCOPE.
