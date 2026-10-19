# WAVE INTEGRATION 12C - Shop Price Validation Audit

Status: PASS_AFTER_TEMPORARY_OVERRIDES

Scope: all `Shop_*.asset` files under `Assets/_Game/Data/Economy`.

Validation rule: a shop item is valid only when `ItemId` exists in `ItemDatabaseSO` and either `BuyPriceOverride > 0` or `ItemDataSO.BaseValue > 0`.

| ShopId | NpcId | ItemId | BaseValue | BuyPriceOverride | Status | Fix |
|---|---|---|---:|---:|---|---|
| shop_blacksmith | npc_brumdar | item_shop_weapon_sword_iron | 50 | 0 | OK | - |
| shop_blacksmith | npc_brumdar | item_shop_armor_leather | 35 | 0 | OK | - |
| shop_blacksmith | npc_brumdar | item_consumable_repair_kit_basic | 25 | 0 | OK | - |
| shop_blacksmith | npc_brumdar | item_consumable_repair_kit_standard | 50 | 0 | OK | - |
| shop_brumdar | npc_brumdar | item_shop_weapon_sword_iron | 50 | 0 | OK | - |
| shop_brumdar | npc_brumdar | item_shop_armor_leather | 35 | 0 | OK | - |
| shop_brumdar | npc_brumdar | item_consumable_repair_kit_basic | 25 | 0 | OK | - |
| shop_brumdar | npc_brumdar | item_consumable_repair_kit_standard | 50 | 0 | OK | - |
| shop_cave_supplies | npc_zrix | item_consumable_potion_hp_small | 15 | 0 | OK | - |
| shop_cave_supplies | npc_zrix | item_consumable_food_bread | 8 | 0 | OK | - |
| shop_cave_supplies | npc_zrix | item_consumable_repair_kit_basic | 25 | 0 | OK | - |
| shop_corvus | npc_corvus | item_consumable_potion_hp_small | 15 | 0 | OK | - |
| shop_corvus | npc_corvus | item_consumable_food_bread | 8 | 0 | OK | - |
| shop_dagna | npc_dagna | item_material_copper_ore | 0 | 12 | OK | TEMPORARY_SHOP_PRICE_OVERRIDE; TODO_ECONOMY_BALANCE_FINAL |
| shop_dagna | npc_dagna | item_material_iron_ore | 8 | 0 | OK | - |
| shop_dagna | npc_dagna | item_material_stone | 0 | 3 | OK | TEMPORARY_SHOP_PRICE_OVERRIDE; TODO_ECONOMY_BALANCE_FINAL |
| shop_eiran | npc_eiran | item_seed_wheat | 10 | 0 | OK | - |
| shop_eiran | npc_eiran | item_seed_carrot | 12 | 0 | OK | - |
| shop_eiran | npc_eiran | item_consumable_food_bread | 8 | 0 | OK | - |
| shop_general_store | npc_renko | item_seed_wheat | 10 | 0 | OK | - |
| shop_general_store | npc_renko | item_seed_carrot | 12 | 0 | OK | - |
| shop_general_store | npc_renko | item_consumable_food_bread | 8 | 0 | OK | - |
| shop_general_store | npc_renko | item_consumable_potion_hp_small | 15 | 0 | OK | - |
| shop_general_store | npc_renko | item_consumable_repair_kit_basic | 25 | 0 | OK | - |
| shop_gruta | npc_gruta | item_consumable_food_bread | 8 | 0 | OK | - |
| shop_gruta | npc_gruta | item_consumable_potion_hp_small | 15 | 0 | OK | - |
| shop_gruta | npc_gruta | item_seed_carrot | 12 | 0 | OK | - |
| shop_gurd | npc_gurd | item_material_stone | 0 | 3 | OK | TEMPORARY_SHOP_PRICE_OVERRIDE; TODO_ECONOMY_BALANCE_FINAL |
| shop_gurd | npc_gurd | item_material_wood | 2 | 0 | OK | - |
| shop_hund | npc_hund | item_consumable_food_bread | 8 | 0 | OK | - |
| shop_hund | npc_hund | item_consumable_potion_hp_small | 15 | 0 | OK | - |
| shop_mara | npc_mara | item_material_wood | 2 | 0 | OK | - |
| shop_mara | npc_mara | item_material_stone | 0 | 3 | OK | TEMPORARY_SHOP_PRICE_OVERRIDE; TODO_ECONOMY_BALANCE_FINAL |
| shop_mirela | npc_mirela | item_material_processed_wood | 15 | 0 | OK | - |
| shop_mirela | npc_mirela | item_consumable_food_bread | 8 | 0 | OK | - |
| shop_nimble | npc_nimble | item_material_wood | 2 | 0 | OK | - |
| shop_nimble | npc_nimble | item_material_stone | 0 | 3 | OK | TEMPORARY_SHOP_PRICE_OVERRIDE; TODO_ECONOMY_BALANCE_FINAL |
| shop_nimble | npc_nimble | item_material_processed_wood | 15 | 0 | OK | - |
| shop_orlan | npc_orlan | item_consumable_food_bread | 8 | 0 | OK | - |
| shop_orlan | npc_orlan | item_consumable_potion_hp_small | 15 | 0 | OK | - |
| shop_ozzra | npc_ozzra | item_consumable_potion_hp_small | 15 | 0 | OK | - |
| shop_ozzra | npc_ozzra | item_material_copper_ore | 0 | 12 | OK | TEMPORARY_SHOP_PRICE_OVERRIDE; TODO_ECONOMY_BALANCE_FINAL |
| shop_ozzra | npc_ozzra | item_material_iron_ore | 8 | 0 | OK | - |
| shop_pip | npc_pip | item_consumable_food_bread | 8 | 0 | OK | - |
| shop_pip | npc_pip | item_seed_wheat | 10 | 0 | OK | - |
| shop_renko | npc_renko | item_seed_wheat | 10 | 0 | OK | - |
| shop_renko | npc_renko | item_seed_carrot | 12 | 0 | OK | - |
| shop_renko | npc_renko | item_consumable_food_bread | 8 | 0 | OK | - |
| shop_renko | npc_renko | item_consumable_potion_hp_small | 15 | 0 | OK | - |
| shop_renko | npc_renko | item_consumable_repair_kit_basic | 25 | 0 | OK | - |
| shop_savra | npc_savra | item_consumable_potion_hp_small | 15 | 0 | OK | - |
| shop_savra | npc_savra | item_material_copper_ore | 0 | 12 | OK | TEMPORARY_SHOP_PRICE_OVERRIDE; TODO_ECONOMY_BALANCE_FINAL |
| shop_savra | npc_savra | item_material_iron_ore | 8 | 0 | OK | - |
| shop_seeds_tools | npc_sylveth | item_seed_wheat | 10 | 0 | OK | - |
| shop_seeds_tools | npc_sylveth | item_seed_carrot | 12 | 0 | OK | - |
| shop_seeds_tools | npc_sylveth | item_shop_tool_hoe_basic | 20 | 0 | OK | - |
| shop_seeds_tools | npc_sylveth | item_consumable_food_bread | 8 | 0 | OK | - |
| shop_seeds_tools | npc_sylveth | item_consumable_potion_hp_small | 15 | 0 | OK | - |
| shop_sylveth | npc_sylveth | item_seed_wheat | 10 | 0 | OK | - |
| shop_sylveth | npc_sylveth | item_seed_carrot | 12 | 0 | OK | - |
| shop_sylveth | npc_sylveth | item_consumable_food_bread | 8 | 0 | OK | - |
| shop_sylveth | npc_sylveth | item_consumable_potion_hp_small | 15 | 0 | OK | - |
| shop_thalindra | npc_thalindra | item_material_processed_wood | 15 | 0 | OK | - |
| shop_thalindra | npc_thalindra | item_material_stone | 0 | 3 | OK | TEMPORARY_SHOP_PRICE_OVERRIDE; TODO_ECONOMY_BALANCE_FINAL |
| shop_tovin | npc_tovin | item_material_wood | 2 | 0 | OK | - |
| shop_tovin | npc_tovin | item_material_stone | 0 | 3 | OK | TEMPORARY_SHOP_PRICE_OVERRIDE; TODO_ECONOMY_BALANCE_FINAL |
| shop_weapons_armor | npc_shop_weapons_armor | item_shop_weapon_sword_iron | 50 | 0 | OK | - |
| shop_weapons_armor | npc_shop_weapons_armor | item_shop_armor_leather | 35 | 0 | OK | - |
| shop_weapons_armor | npc_shop_weapons_armor | item_consumable_repair_kit_basic | 25 | 0 | OK | - |
| shop_weapons_armor | npc_shop_weapons_armor | item_consumable_repair_kit_standard | 50 | 0 | OK | - |
| shop_yael | npc_yael | item_consumable_food_bread | 8 | 0 | OK | - |
| shop_yael | npc_yael | item_material_copper_ore | 0 | 12 | OK | TEMPORARY_SHOP_PRICE_OVERRIDE; TODO_ECONOMY_BALANCE_FINAL |
| shop_yael | npc_yael | item_consumable_potion_hp_small | 15 | 0 | OK | - |
| shop_zrix | npc_zrix | item_consumable_potion_hp_small | 15 | 0 | OK | - |
| shop_zrix | npc_zrix | item_consumable_food_bread | 8 | 0 | OK | - |
| shop_zrix | npc_zrix | item_consumable_repair_kit_basic | 25 | 0 | OK | - |

Result: 76 shop entries audited, 0 invalid prices after fix.
