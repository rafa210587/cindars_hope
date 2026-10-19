# WAVE INTEGRATION 12C - Shop Price Fix Report

Status: BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE

## Root cause

ShopManager rejects shop initialization when BuyPriceOverride <= 0 and ItemData.BaseValue <= 0.

## Shops affected

| ShopId | NpcId | ItemId | Root issue |
|---|---|---|---|
| shop_tovin | npc_tovin | item_material_stone | BaseValue 0 and BuyPriceOverride 0 |
| shop_savra | npc_savra | item_material_copper_ore | BaseValue 0 and BuyPriceOverride 0 |
| shop_dagna | npc_dagna | item_material_copper_ore | BaseValue 0 and BuyPriceOverride 0 |
| shop_dagna | npc_dagna | item_material_stone | BaseValue 0 and BuyPriceOverride 0 |
| shop_thalindra | npc_thalindra | item_material_stone | BaseValue 0 and BuyPriceOverride 0 |
| shop_yael | npc_yael | item_material_copper_ore | BaseValue 0 and BuyPriceOverride 0 |
| shop_ozzra | npc_ozzra | item_material_copper_ore | BaseValue 0 and BuyPriceOverride 0 |
| shop_gurd | npc_gurd | item_material_stone | BaseValue 0 and BuyPriceOverride 0 |
| shop_nimble | npc_nimble | item_material_stone | BaseValue 0 and BuyPriceOverride 0 |
| shop_mara | npc_mara | item_material_stone | BaseValue 0 and BuyPriceOverride 0 |

## Fix applied

| ItemId | BuyPriceOverride applied | Shops |
|---|---:|---|
| item_material_stone | 3 | shop_tovin, shop_dagna, shop_thalindra, shop_gurd, shop_nimble, shop_mara |
| item_material_copper_ore | 12 | shop_savra, shop_dagna, shop_yael, shop_ozzra |

No ShopManager relaxation was applied. The runtime contract remains strict.

## Remaining debt

TEMPORARY_SHOP_PRICE_OVERRIDE
TODO_ECONOMY_BALANCE_FINAL

Advanced service UI remains debt for service-heavy NPCs now wired through basic shop runtime:

| NpcId | ShopId | Runtime status |
|---|---|---|
| npc_pip | shop_pip | SHOP_RUNTIME_BASIC_WITH_ADVANCED_SERVICE_DEBT |
| npc_nimble | shop_nimble | SHOP_RUNTIME_BASIC_WITH_ADVANCED_SERVICE_DEBT |
| npc_thalindra | shop_thalindra | SHOP_RUNTIME_BASIC_WITH_ADVANCED_SERVICE_DEBT |

## Expected Play Mode result

TownScene opens without ShopManager invalid price errors.

Pressing Play should not produce NpcShopController session creation errors caused by invalid shop item pricing.
