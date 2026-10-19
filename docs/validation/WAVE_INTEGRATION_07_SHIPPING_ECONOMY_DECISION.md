# WAVE_INTEGRATION_07 — Shipping Economy Decision

Date: 2026-06-08
Branch: dev
Author: Claude Code (WAVE_INTEGRATION_07)

---

## Design Question

"Should we create a new ShippingBin interactable, or reuse the existing SellPoint?"

---

## Decision: REUSE_EXISTING_SHIPPING_RUNTIME

**Strategy:** Wire existing `SellPoint.cs` into FarmScene via `CreateMvpFarmScene.cs`.

**Economy model:** `IMMEDIATE_GOLD` — SellPoint iterates inventory, validates via SellableItemPolicy,
calculates `BaseValue * amount`, removes items, calls `PlayerManager.AddGold(totalGold)`.

**Scene delivery model:** `CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED` —
code changes wire the SellPoint at scene-generation time; human must run CreateMvpFarmScene generator in Unity Editor.

---

## Rationale

### Why REUSE_EXISTING_SHIPPING_RUNTIME

| Criteria | Finding |
|----------|---------|
| SellPoint.cs exists | YES — `Assets/_Game/Scripts/Economy/SellPoint.cs` fully implemented |
| Implements IInteractable | YES — interaction prompt "Vender" |
| Iterates inventory | YES — `_inventoryManager.Items` snapshot |
| Validates sellability | YES — `SellableItemPolicy.IsSellable(itemId)` |
| Calculates price | YES — `itemData.BaseValue * amount` |
| Removes items | YES — `_inventoryManager.RemoveItem(itemId, amount)` |
| Awards gold | YES — `_playerManager.AddGold(totalGold)` |
| CreateSellPoint() already exists | YES — line 619 in CreateMvpFarmScene.cs |
| CreateSellPoint() called from CreateScene() | NO — was missing; now added |

Creating a new ShippingBin system would duplicate SellPoint behavior for no benefit in the MVP.
The FarmShippingService end-of-day deferred payout exists but is NOT required for the immediate economy loop.

### Why IMMEDIATE_GOLD instead of DEFERRED_PAYOUT

- FarmShippingService requires save-tick integration, EndOfDayEvent processing, and ShippingBinState save section.
- WAVE_INTEGRATION_07 spec scope: "close the first economic loop" — immediate feedback is sufficient.
- Deferred payout can be a future WAVE_INTEGRATION integration slice if needed.

### Why IMMEDIATE_GOLD is compatible with SellableItemPolicy

| Category | Sellable? | Reason |
|----------|-----------|--------|
| Crops (item_crop_carrot etc.) | YES if BaseValue > 0 | Category is Crop, not KeyItem/Quest |
| Materials (item_material_wood etc.) | YES if BaseValue > 0 | Category is Material |
| Seeds | YES if BaseValue > 0 | Category is Seed |
| Fish | YES if BaseValue > 0 | Category is Fish |
| Tools (hoe, watering can, pickaxe) | NO | IsEssentialTool() filter blocks them |
| KeyItem / Quest | NO | Blocked by category filter |
| BaseValue = 0 items | NO | Policy returns false for zero-value items |

---

## Code Changes Made

| File | Change |
|------|--------|
| `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs` | Updated `CreateSellPoint()` position from (-4.75f, -1.75f) to (3.5f, 7.5f) matching Zone_ShippingSellpoint |
| `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs` | Added `CreateSellPoint(inventoryManager, playerManager)` call in `CreateScene()` after `CreateFarmResourceInteractables(inventoryManager)` |

No new C# files created. Zero runtime code added.

---

## Zone Reference

Zone_ShippingSellpoint per `docs/validation/WAVE_INTEGRATION_04_FARMSCENE_ZONE_MAP.md`:
- Position: (3.5, 7.5)
- Size: 3.5 x 2.5
- Zone ID: farm_zone_shipping_sellpoint
- Location: north-center of farm

---

## What This Does NOT Include (Per Spec Scope)

- Shop NPC
- Shop UI (buy/sell panel)
- Full economy engine (FarmShippingService end-of-day)
- Crafting integration
- Quest rewards
- Inventory UI

---

## Alternatives Rejected

| Alternative | Reason Rejected |
|------------|----------------|
| Create new ShippingBin.cs | Duplicates SellPoint; adds complexity with no gameplay gain |
| FarmShippingService deferred payout | Out of scope for this slice; requires EndOfDayEvent, save section, ShippingBinState |
| EconomyPricingService PriceChannel.SellPoint (0.90f) | Out of scope; BaseValue direct is acceptable for MVP |
