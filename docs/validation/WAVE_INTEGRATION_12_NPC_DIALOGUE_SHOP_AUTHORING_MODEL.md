# WAVE INTEGRATION 12 - NPC, Dialogue and Shop Authoring Model

## Status

COMPLETE_WITH_RUNTIME_PLACEHOLDER_DEBT

## Purpose

This document explains how to add, modify and connect NPCs, dialogues and shops after WAVE_INTEGRATION_12 without creating one class per NPC or parallel runtime systems.

## NPC Definition Sources

| Source | Type | Path | Editable by design? |
|---|---|---|---|
| NPC runtime asset | ScriptableObject | `Assets/_Game/Data/NPCs/Npc_<Name>.asset` | Yes |
| Pure NPC contract | C# data contract | `Assets/_Game/Scripts/NPC/NpcDefinition.cs` | Engineering |
| Scene placement | Scene object marker | `NpcScenePlacementMarker` in `TownScene.unity` | Scene authoring |

## Dialogue Definition Sources

| Source | Type | Path | Editable by design? |
|---|---|---|---|
| Dialogue tree | ScriptableObject | `Assets/_Game/Data/Dialogues/DialogueTree_<Name>.asset` | Yes |
| Dialogue runtime | Generic controller | `NpcController`, `DialogueModal` | Engineering |

## Shop Definition Sources

| Source | Type | Path | Editable by design? |
|---|---|---|---|
| Shop stock/pricing | ScriptableObject | `Assets/_Game/Data/Economy/Shop_*.asset` | Yes |
| Shop runtime | Generic controller/service | `NpcShopController`, `ShopManager`, `BuyPanel`, `SellPanel` | Engineering |

## Stable IDs

| ID type | Format/rule | Used by save? | Notes |
|---|---|---:|---|
| NpcId | `npc_<canonical_or_existing_stable_id>` | 1 | Do not use GameObject name as persistent ID |
| DialogueId | `dialogue_<npc_or_service>` | 0 now | Used by `DialogueTreeSO.Id` |
| ShopId | `shop_<service>` | 1 | Used by `ShopManager` sessions/stock |
| PlacementId | `town_<location>_<npc>` | 0 now | Used by `NpcScenePlacementMarker` and docs |

## How To Add A New NPC

1. Create or reuse one `NpcDataSO` under `Assets/_Game/Data/NPCs/`.
2. Set `NpcId`, `DisplayName`, `DefaultSceneId`, `DefaultPositionId`, `DefaultPosition`, `MovementMode`, `DialogueTree`, and optional `ShopId`.
3. Create one `DialogueTreeSO` under `Assets/_Game/Data/Dialogues/` with at least 10 nodes for MVP NPCs.
4. If merchant, create/reuse one `ShopDataSO` and set `ShopDataSO.NpcId` to the NPC's stable `NpcId`.
5. Place the NPC in `TownScene` using `NpcController` for dialogue-only NPCs or `NpcShopController` for merchants.
6. Add `NpcScenePlacementMarker` with `NpcId`, `TownScene`, placement id, movement profile and reachable flag.
7. Register the component in `NpcManager._npcs` or `_shopNpcs`.

## How To Avoid One Class Per NPC

Use:
- `NpcDataSO` for identity/content.
- `DialogueTreeSO` for dialogue.
- `ShopDataSO` for shop stock.
- `NpcController` / `NpcShopController` for runtime interaction.
- `NpcScenePlacementMarker` for scene evidence.
- `NpcWanderer` or future schedule controllers for movement.

Do not create `SylvethNpc.cs`, `BrumdarNpc.cs`, `Merchant01Npc.cs`, or other one-class-per-character runtime scripts.

## Minimum 10 Dialogue Entries

Each MVP NPC must have:
1. Greeting
2. Role
3. Place/town/farm/cave context
4. Gameplay tip
5. Non-spoiler rumor
6. Time/weather/day placeholder if final runtime is not ready
7. Shop/service note if applicable
8. Future quest hook
9. Repeat/fallback
10. Goodbye/close

## How Pricing And Inventory Work

`NpcShopController` opens `ShopMenuModal`, `BuyPanel`, and `SellPanel`. Those call `ShopManager`, which uses `ShopDataSO`, `ItemDatabaseSO`, `InventoryManager`, and `PlayerManager`. Do not calculate prices in UI and do not create a separate wallet/inventory.

## How Focus/Modal Should Behave

`DialogueModal`, `ShopMenuModal`, `BuyPanel`, and `SellPanel` use the existing `ModalManager`. Player movement and interaction should resume after close. Human Play Mode must confirm no stuck modal remains.

## Future Bridges

Relationship, reputation, romance, companion/pet, final quests, final daily schedules and final city simulation are future scope. WAVE12 only adds authoring-ready hooks and functional NPC/shop/dialogue runtime wiring.

## Validation Checklist For New NPC Authoring

- NpcId stable:
- DisplayName:
- Purpose/responsibility:
- Placement marker:
- MovementProfile:
- DialogueTree with >=10 nodes:
- ShopDataSO + ShopId if merchant:
- Registered in NpcManager:
- No class-per-NPC runtime script:

## Known Debts

| Debt | Impact | Required before |
|---|---|---|
| TEMPORARY_DIALOGUE_AUTHORING_PLACEHOLDER | Dialogue is functional but not final narrative | Narrative/content polish |
| DAILY_SCHEDULE_DEFERRED | NPCs do not follow final calendar routines | City simulation wave |
| Relationship/reputation deferred | No social progression impact | Social system wave |
| Human Play Mode pending | Static/build checks cannot prove interaction feel | WAVE12 acceptance and WAVE13 continuation |
