# WAVE INTEGRATION 12 - NPC Shop Services

Status: MVP_SHOP_SERVICES_WIRED_WITH_HUMAN_PLAYMODE_PENDING

| NpcId | ShopId | ServiceType | StockSource | Buy? | Sell? | PricingSource | InventoryIntegration | Temporary? | Notes |
|---|---|---|---|---:|---:|---|---|---:|---|
| npc_sylveth | shop_seeds_tools | SeedVendor | `Assets/_Game/Data/Economy/Shop_Seeds_Tools.asset` | 1 | 1 | `ShopManager` item BaseValue + multipliers | `InventoryManager` + `PlayerManager` through `BuyPanel`/`SellPanel` | 0 | Seeds/tools/farm supplies |
| npc_brumdar | shop_blacksmith | BlacksmithRepairUpgrade | `Assets/_Game/Data/Economy/Shop_Blacksmith.asset` | 1 | 1 | `ShopManager` item BaseValue + multipliers | `InventoryManager` + `PlayerManager` through `BuyPanel`/`SellPanel` | 0 | Weapons/armor/repair kits |
| npc_renko | shop_general_store | GeneralMerchant | `Assets/_Game/Data/Economy/Shop_General_Store.asset` | 1 | 1 | `ShopManager` item BaseValue + multipliers | `InventoryManager` + `PlayerManager` through `BuyPanel`/`SellPanel` | 0 | Food/common supplies |
| npc_zrix | shop_cave_supplies | CaveRumorInfo | `Assets/_Game/Data/Economy/Shop_Cave_Supplies.asset` | 1 | 1 | `ShopManager` item BaseValue + multipliers | `InventoryManager` + `PlayerManager` through `BuyPanel`/`SellPanel` | 0 | Cave prep supplies; does not start WAVE13/cave transition |
| npc_thalindra |  | QuestLibrary | None | 0 | 0 | N/A | N/A | 1 | Dialogue/service hook only |
| npc_nimble |  | CraftingWorkshop | None | 0 | 0 | N/A | N/A | 1 | Workshop/crafting hook only |
| npc_pip_miudinho |  | TownGuide | None | 0 | 0 | N/A | N/A | 1 | Tutorial/guide only |

Debt:
- Advanced pricing profile modifiers are still deferred.
- Human Play Mode must confirm buy/sell transaction path, gold delta, item delta, stock decrement, and modal close.
