# WAVE INTEGRATION 12 - NPC Shop Services

Status: SHOP_SERVICES_WIRED_WITH_HUMAN_PLAYMODE_PENDING

| NpcId | ShopId | ServiceType | StockSource | Buy? | Sell? | PricingSource | InventoryIntegration | Temporary? | Notes |
|---|---|---|---|---:|---:|---|---|---:|---|
| npc_shop_seeds_tools | shop_seeds_tools | Seeds/tools/basic supplies | `Assets/_Game/Data/Economy/Shop_Seeds_Tools.asset` | 1 | 1 | `ShopManager` using item BaseValue, BuyPriceMultiplier, SellPriceMultiplier | `InventoryManager` + `PlayerManager` via `BuyPanel`/`SellPanel` | 0 | Sells seeds, hoe, bread, HP potion |
| npc_shop_weapons_armor | shop_weapons_armor | Weapons/armor/repair supplies | `Assets/_Game/Data/Economy/Shop_Weapons_Armor.asset` | 1 | 1 | `ShopManager` using item BaseValue, BuyPriceMultiplier, SellPriceMultiplier | `InventoryManager` + `PlayerManager` via `BuyPanel`/`SellPanel` | 0 | Sells sword, leather armor, repair kits |

Debt:
- Advanced pricing service/profile modifiers are not wired into shop UI yet.
- Human Play Mode must confirm buy/sell transaction path, gold delta, item delta, stock decrement, and modal close.
