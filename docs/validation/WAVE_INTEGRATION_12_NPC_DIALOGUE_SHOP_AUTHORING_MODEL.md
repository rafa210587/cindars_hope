# WAVE INTEGRATION 12 - NPC, Dialogue and Shop Authoring Model

## Status

PARTIAL_COMPLETE_WITH_MERCHANT_DIALOGUE_TREE_DEBT

## Purpose

This document explains how to add, modify and connect NPCs, dialogues and shops after WAVE_INTEGRATION_12.

## NPC definition sources

| Source | Type | Path | Editable by design? |
|---|---|---|---|
| Runtime NPC asset | ScriptableObject | `Assets/_Game/Data/NPCs/*.asset` | YES |
| Canon roster status | Markdown | `docs/validation/WAVE_INTEGRATION_12_NPC_CANONICAL_ROSTER.md` | YES |
| Pure NPC contract | C# data contract | `Assets/_Game/Scripts/NPC/NpcDefinition.cs` | NO |

## Dialogue definition sources

| Source | Type | Path | Editable by design? |
|---|---|---|---|
| Runtime dialogue tree | ScriptableObject | `Assets/_Game/Data/Dialogues/*.asset` | YES |
| Dialogue coverage table | Markdown | `docs/validation/WAVE_INTEGRATION_12_NPC_DIALOGUE_SETS.md` | YES |

## Shop definition sources

| Source | Type | Path | Editable by design? |
|---|---|---|---|
| Shop data | ScriptableObject | `Assets/_Game/Data/Economy/Shop_*.asset` | YES |
| Item data | ScriptableObject | `Assets/_Game/Data/Items/*.asset` | YES |
| Item registry | ScriptableObject | `Assets/_Game/Data/Registries/ItemDatabase.asset` | YES |

## Stable IDs

| ID type | Format/rule | Used by save? | Notes |
|---|---|---:|---|
| NpcId | lower snake id, stable forever after scene/save use | YES | Existing Pip alias debt: `npc_pip_miudinho` vs canonical `npc_pip` |
| DialogueId | `dialogue_<npc or service>` | NO currently | Stored in `DialogueTreeSO.Id` |
| ShopId | `shop_<service>` | YES for shop stock | Stored in `ShopDataSO.Id` |

## How to add a new NPC

1. Create an `NpcDataSO` under `Assets/_Game/Data/NPCs/`.
2. Assign stable `NpcId`, `DisplayName`, `DefaultSceneId`, `DefaultPositionId`, `DefaultPosition`, `MovementMode`.
3. Add a row to `WAVE_INTEGRATION_12_NPC_CANONICAL_ROSTER.md`.
4. Add or update `CreateMvpTownScene.cs` if the NPC should be generated in TownScene.
5. Use `NpcController` for dialogue NPCs and `NpcShopController` for merchant NPCs.

## How to avoid creating one class per NPC

Use data fields in `NpcDataSO`, `DialogueTreeSO`, and `ShopDataSO`. Create a new C# behavior only when the NPC has unique runtime logic that cannot be expressed as data.

## How to define NPC purpose and responsibilities

Update the roster row: `Role`, `Purpose`, and `Responsibilities`. Do not mark an NPC as MVP-ready if these are blank.

## How to place an NPC in a scene

Use the scene generator for persistent placement. For TownScene, add the object through `CreateMvpTownScene.CreateDialogueNpc` or `CreateShopNpc`, then regenerate the scene in Unity.

## How to define NPC movement/schedule

For MVP use `Stationary`, `ShopKeeperFixed`, or `WanderWithinZone`. For future schedules, add `NpcScheduleDefinition` data and document in `WAVE_INTEGRATION_12_NPC_MOVEMENT_SCHEDULES.md`.

## How to attach dialogue to an NPC

1. Create a `DialogueTreeSO`.
2. Ensure `StartNodeId` points to an existing `DialogueNode`.
3. Add at least 10 entries/options for MVP NPCs.
4. Assign the tree to `NpcDataSO.DialogueTree`.

## Minimum 10 dialogue entries/options per MVP NPC

Dialogue NPCs should have 10 runtime nodes where possible. Merchant NPCs currently have documented 10-entry placeholders and runtime shop menu; merchant `DialogueTreeSO` consumption remains debt.

## How to add a dialogue choice

Add a `DialogueChoice` to a node:
- `Label`: visible choice.
- `NextNodeId`: next node for normal branching.
- `ActionType`: `None`, `CloseDialogue`, or `OpenShop`.

`NpcController` resolves `NextNodeId` and closes on `CloseDialogue`.

## How to attach shop to an NPC

1. Create or reuse `ShopDataSO`.
2. Set `ShopDataSO.Id` and `ShopDataSO.NpcId`.
3. Set matching `NpcDataSO.ShopId`.
4. Use `NpcShopController` in scene and assign `ShopDataSO`, `ShopMenuModal`, `BuyPanel`, `SellPanel`, `ShopManager`, `InventoryManager`, `PlayerManager`, and `ItemDatabaseSO`.

## How to add a shop item

Add an entry to `ShopDataSO.Items` with `ItemId`, `BaseDailyStock`, `IsFiniteStock`, optional `BuyPriceOverride`, and optional unlock tag. Ensure the item exists in `ItemDatabase.asset`.

## How pricing is resolved

`ShopManager` uses item `BaseValue`, `BuyPriceMultiplier`, `SellPriceMultiplier`, and stock. Advanced pricing profiles remain future debt.

## How inventory/gold is updated

`BuyPanel` and `SellPanel` call `ShopManager.TryBuyItem` / `TrySellItem`, which update `PlayerManager` gold and `InventoryManager` inventory. No parallel wallet or inventory is allowed.

## How focus/modal should behave

Dialogue, shop menu, buy panel, and sell panel use `ModalManager`. Gameplay input should be blocked while `ModalManager.HasActiveModal` is true.

## Future relationship/reputation/quest/schedule bridge

Keep future hooks in docs and data IDs. Do not implement final relationship, romance, reputation, quest, or calendar schedule in this integration spec.

## Validation checklist for new NPC authoring

- NpcId stable:
- Role:
- Purpose:
- Responsibilities:
- Placement:
- MovementProfile:
- DialogueSet with >=10 entries:
- ShopId/service if applicable:
- Future hooks:

## Known debts

| Debt | Impact | Required before |
|---|---|---|
| Merchant dialogue trees not consumed by `NpcShopController` | Merchant has opening/shop/closing but not 10 runtime choices | Final NPC dialogue acceptance |
| `npc_pip_miudinho` alias differs from canonical `npc_pip` | Canon/runtime ID mismatch must not be silently renamed | Save/roster reconciliation |
| Advanced pricing profile not fully consumed | Buy/sell uses current `ShopManager` multipliers only | Economy polish |
| Human Play Mode not run | Static/build validation only | Next wave acceptance |
