# WAVE INTEGRATION 12 - Human Unity NPC/Dialogue/Shop Wiring Instructions

Status: UPDATED_FOR_CORRECTIVE_PASS_PENDING_HUMAN_PLAYMODE
Date: 2026-06-09
Scene: `Assets/_Game/Scenes/TownScene.unity`

This guide validates the corrective WAVE12 TownScene wiring. It does not authorize WAVE13, WAVE14, WAVE15, final quest systems, social systems, reputation, romance, companions, or pets.

## Expected TownScene NPCs

| Scene object | Runtime components | Data assets |
|---|---|---|
| `NPC_Pip_Miudinho` | `NpcController`, `NpcScenePlacementMarker` | `Npc_Pip_Miudinho.asset` |
| `NPC_Sylveth_SeedVendor` | `NpcShopController`, `NpcScenePlacementMarker` | `Npc_Sylveth.asset`, `Shop_Seeds_Tools.asset` |
| `NPC_Brumdar_Blacksmith` | `NpcShopController`, `NpcScenePlacementMarker` | `Npc_Brumdar.asset`, `Shop_Blacksmith.asset` |
| `NPC_Renko_GeneralMerchant` | `NpcShopController`, `NpcScenePlacementMarker` | `Npc_Renko.asset`, `Shop_General_Store.asset` |
| `NPC_Thalindra_Library` | `NpcController`, `NpcScenePlacementMarker` | `Npc_Thalindra.asset` |
| `NPC_Zrix_CaveRumor` | `NpcShopController`, `NpcScenePlacementMarker` | `Npc_Zrix.asset`, `Shop_Cave_Supplies.asset` |
| `NPC_Nimble_Workshop` | `NpcController`, `NpcScenePlacementMarker` | `Npc_Nimble.asset` |

`NPC_Vaalara_Wanderer_01` may remain in the scene as temporary legacy ambience, but it is not counted as MVP roster completion.

## Unity Checklist

1. Open `TownScene.unity`.
2. Confirm `_Bootstrap` has `GameBootstrap`, `ModalManager`, `ShopManager`, `InventoryManager`, `PlayerManager`, and `ItemDatabase`.
3. Confirm `ShopCanvas` exists with `DialogueModal`, `ShopMenuModal`, `BuyPanel`, and `SellPanel`.
4. Confirm each expected NPC above exists once under the NPC root.
5. Confirm each expected NPC has `NpcScenePlacementMarker` with its canonical `NpcId`, `TownScene`, placement id, and movement profile.
6. Run `CindarsHope/Validate/Validate WAVE12 NPC Dialogue Shop Bridge`.
7. Enter Play Mode.
8. Approach Pip, Thalindra, and Nimble and validate dialogue prompt, dialogue choices, and close behavior.
9. Approach Sylveth, Brumdar, Renko, and Zrix and validate dialogue prompt, shop prompt, buy menu, sell menu, and exit behavior.
10. Confirm player movement and interaction input return after every modal closes.
11. Confirm no NPC blocks farm/cave/shop paths in TownScene.

## Expected Results

| Area | Expected result |
|---|---|
| Roster | 7 MVP NPCs present and reachable |
| Dialogue | Each MVP NPC has at least 10 authored dialogue nodes |
| Shops | Sylveth, Brumdar, Renko, and Zrix open real shop/service data |
| Movement profile | Each MVP NPC exposes a profile marker |
| Implemented movement | Stationary/fixed placement only |
| Scene runtime | Existing dialogue/shop UI and manager path reused |
| Out of scope | No final quest/social/reputation/romance/companion/pet system |

## Gate

WAVE13 remains blocked until this checklist passes in human Play Mode validation.
