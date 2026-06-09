# WAVE INTEGRATION 12 - Human Unity NPC/Dialogue/Shop Wiring Instructions

## Target scene

`Assets/_Game/Scenes/TownScene.unity`

Scene regeneration is not required for the existing static wiring because current TownScene already contains the NPCs and shop UI. If the scene is regenerated, use `CindarsHope/Create Scenes/Town Scene`.

## Required hierarchy

| Object | Component | References |
|---|---|---|
| `NPC_Pip_Miudinho` | `NpcController` | `Npc_Pip_Miudinho.asset`, `DialogueModal` |
| `NPC_Vaalara_Wanderer_01` | `NpcController`, `NpcWanderer` | `Npc_Vaalara_Wanderer_01.asset`, `DialogueModal` |
| `NPC_SeedsToolsShop` | `NpcShopController` | `Npc_Shop_Seeds_Tools.asset`, `Shop_Seeds_Tools.asset`, shop UI panels |
| `NPC_WeaponsArmorShop` | `NpcShopController` | `Npc_Shop_Weapons_Armor.asset`, `Shop_Weapons_Armor.asset`, shop UI panels |
| `ShopCanvas/DialogueModal` | `DialogueModal` | ModalManager |
| `ShopCanvas/ShopMenuModal` | `ShopMenuModal` | ModalManager |
| `ShopCanvas/BuyPanel` | `BuyPanel` | ShopManager, PlayerManager, InventoryManager, ItemDatabase |
| `ShopCanvas/SellPanel` | `SellPanel` | ShopManager, PlayerManager, InventoryManager, ItemDatabase |

## Step-by-step

1. Open `TownScene`.
2. Confirm `_Bootstrap` has `GameBootstrap`, `ModalManager`, `ShopManager`, `InventoryManager`, `PlayerManager`, and `ItemDatabase`.
3. Confirm `ShopCanvas` exists with `DialogueModal`, `ShopMenuModal`, `BuyPanel`, and `SellPanel`.
4. Confirm `NPC_Pip_Miudinho` has `NpcController`.
5. Confirm `NPC_SeedsToolsShop` and `NPC_WeaponsArmorShop` have `NpcShopController`.
6. Run `CindarsHope/Validate/Validate WAVE12 NPC Dialogue Shop Bridge`.
7. Enter Play Mode.
8. Approach Pip and press interact.
9. Navigate dialogue choices and close.
10. Approach merchant.
11. Open shop menu.
12. Buy an item.
13. Confirm gold decreases and item appears in inventory.
14. Sell a sellable item.
15. Confirm item amount decreases and gold increases.
16. Close shop.
17. Confirm movement/input returns and no modal is stuck.

## Validation checklist

| Check | Expected |
|---|---|
| Dialogue prompt | Appears near Pip/Wanderer |
| Dialogue choices | Choices appear and navigate |
| Dialogue close | Modal closes and movement returns |
| Shop prompt | Appears near merchant |
| Shop menu | Buy/Sell/Exit visible |
| Buy | Uses real gold/inventory |
| Sell | Uses real inventory/gold |
| Focus | No stuck modal |
