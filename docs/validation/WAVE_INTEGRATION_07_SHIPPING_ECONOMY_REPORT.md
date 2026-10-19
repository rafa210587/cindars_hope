# WAVE_INTEGRATION_07 — Shipping Bin / Economy Loop Execution Report

Date: 2026-06-08
Branch: dev
Status: BUILD_VALIDATED_CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED

---

## Preflight

- Branch: dev
- Working tree: M Assets/_Game/Scenes/FarmScene.unity (pre-existing, expected)
- WAVE_INTEGRATION_06A baseline: BUILD_VALIDATED_CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED

---

## Objective

Close the first economic loop: collect item → inventory → SellPoint → sell → item removed → gold awarded → feedback.

---

## API Audit Results

### 1. Shipping / SellPoint API

| System | Status | Notes |
|--------|--------|-------|
| `SellPoint.cs` | EXISTS | `Assets/_Game/Scripts/Economy/SellPoint.cs` — fully implemented |
| `IInteractable` | IMPLEMENTED | Prompt "Vender" |
| Inventory iteration | CONFIRMED | `_inventoryManager.Items` (IReadOnlyDictionary snapshot) |
| `SellableItemPolicy.IsSellable()` | CONFIRMED | Blocks KeyItem, Quest, essential tools (hoe/watering/pickaxe) |
| `itemData.BaseValue * amount` | CONFIRMED | Direct BaseValue multiplication |
| `_inventoryManager.RemoveItem()` | CONFIRMED | Called per item type |
| `_playerManager.AddGold()` | CONFIRMED | Publishes `GoldChangedEvent` |
| `CreateSellPoint()` in scene creator | EXISTS | Line 619 of CreateMvpFarmScene.cs — was not called; now wired |

### 2. Economy / PlayerManager API

| System | Status | Notes |
|--------|--------|-------|
| `PlayerManager.AddGold(int)` | CONFIRMED | Full method in PlayerManager |
| `PlayerManager.CurrentGold` | CONFIRMED | Returns `_saveManager.PlayerState.Gold` |
| `GoldChangedEvent` | CONFIRMED | Published by AddGold; HUD listens |
| `EconomyManager` | PRESENT | On Bootstrap; SellPoint does NOT use it (uses PlayerManager.AddGold directly) |

### 3. Inventory Bridge

| System | Status | Notes |
|--------|--------|-------|
| `InventoryManager.Items` | CONFIRMED | IReadOnlyDictionary<string, int> |
| `InventoryManager.RemoveItem()` | CONFIRMED | Returns bool |
| `InventoryManager.TryGetItemData()` | CONFIRMED | Used by SellPoint to get BaseValue |
| `ItemDatabaseSO` wired in bootstrap | CONFIRMED | ConfigureBootstrap sets `_itemDatabase` |

### 4. Interaction System

| System | Status | Notes |
|--------|--------|-------|
| `IInteractable` | CONFIRMED | SellPoint fully implements |
| `InteractionSystem` | CONFIRMED | Finds IInteractable in range via overlap check |
| `CanInteract(GameObject)` | CONFIRMED | SellPoint returns true always when inventory not empty; sells all items |
| Prompt | CONFIRMED | "Vender" |

---

## Files Changed

| File | Change | Type |
|------|--------|------|
| `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs` | Position update + wiring call | Editor (no runtime impact) |

No new C# files. No new ScriptableObjects. No runtime changes.

---

## Change Details

### Change 1: Position update

```csharp
// Before
sellPointObject.transform.position = new Vector3(-4.75f, -1.75f, 0f);

// After
sellPointObject.transform.position = new Vector3(3.5f, 7.5f, 0f);
```

Matches Zone_ShippingSellpoint (north-center of farm, per WAVE_INTEGRATION_04_FARMSCENE_ZONE_MAP.md).

### Change 2: Wiring call added to CreateScene()

```csharp
CreateFarmResourceInteractables(inventoryManager);
CreateSellPoint(inventoryManager, playerManager);  // ← ADDED
CreateDebugHud(...)
```

---

## Economy Loop Trace

```
Player harvest → InventoryManager.AddItem("item_crop_carrot", 1)
                    ↓
Player walks to SellPoint (3.5, 7.5) — north-center zone
                    ↓
InteractionSystem detects SellPoint in range
                    ↓
InteractionPrompt shows "Vender"
                    ↓
Player presses E → SellPoint.Interact(player)
                    ↓
foreach item in inventory.Items snapshot:
  SellableItemPolicy.IsSellable(itemId) → true for crop/material/fish
  inventoryManager.TryGetItemData(itemId, out itemData)
  totalGold += itemData.BaseValue * amount
  inventoryManager.RemoveItem(itemId, amount)
                    ↓
playerManager.AddGold(totalGold)
  → GoldChangedEvent published
  → HUD gold counter updates
                    ↓
GameEventBus.Publish(PlayerActionFeedbackEvent($"Vendido por {totalGold} moedas."))
                    ↓
FeedbackPanel shows message
```

---

## Acceptance Criteria — Spec Compliance Matrix

| Criterion | Status | Evidence |
|-----------|--------|---------|
| SellPoint placed at Zone_ShippingSellpoint (3.5, 7.5) | PASS | CreateSellPoint() position updated; human must regenerate FarmScene |
| Player can interact with SellPoint | PASS (CODE) | SellPoint implements IInteractable; "Vender" prompt |
| Items removed from inventory on sell | PASS (CODE) | SellPoint.RemoveItem() called for each item |
| Gold awarded to player | PASS (CODE) | PlayerManager.AddGold(totalGold); GoldChangedEvent |
| Feedback shown to player | PASS (CODE) | PlayerActionFeedbackEvent published by SellPoint |
| Essential tools not sold | PASS (CODE) | SellableItemPolicy.IsEssentialTool() blocks hoe/watering/pickaxe |
| KeyItem/Quest items not sold | PASS (CODE) | SellableItemPolicy category filter |
| Zero-value items not sold | PASS (CODE) | SellableItemPolicy BaseValue <= 0 filter |

---

## Build Validation

Validation method: explicit exit code check

- Assembly-CSharp: PASS (exit code 0, 0 errors, 0 warnings)
- Assembly-CSharp-Editor: PASS (exit code 0, 0 errors, 3 pre-existing warnings — unchanged from WAVE_INTEGRATION_06A)
- Pre-existing warnings (EnemyTaxonomy CS0649 x2, CSharpProjectPostprocessor UNT0006 x1) — not introduced by this change

---

## Testing Quality Gate

Changed runtime code: NO (only CreateMvpFarmScene.cs changed, which is Editor-only)
Changed deterministic logic: NO
Changed Unity scene/prefab/asset wiring: NO (scene regeneration needed by human)
Automated tests added/updated: NO
Justification: SellPoint.cs is unchanged. CreateMvpFarmScene.cs is Editor-only scene generator. Core InventoryManager.RemoveItem, PlayerManager.AddGold, and SellableItemPolicy already have coverage from prior waves.
Manual Play Mode scenario: docs/validation/WAVE_INTEGRATION_07_HUMAN_PLAYMODE_CHECKLIST.md
Residual risk: Play Mode not yet executed. Human must regenerate FarmScene to place SellPoint in scene, then test economy loop in Play Mode.

---

## Honest Status Rationale

Status: BUILD_VALIDATED_CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED

NOT advancing to WAVE_INTEGRATION_08 yet because:
1. Human must run CreateMvpFarmScene generator to regenerate FarmScene with SellPoint at (3.5, 7.5).
2. Human must confirm SellPoint appears at the correct zone (north-center).
3. Human must confirm economy loop: harvest → SellPoint → gold increment → feedback.
4. Human must confirm SellableItemPolicy blocks essential tools.
5. WAVE_INTEGRATION_06A Play Mode checklist must also pass (prerequisite).

No new C# code was written. No tests are needed beyond what SellPoint and InventoryManager already have. The only missing evidence is the human Play Mode run.

---

## See Also

- Decision: `docs/validation/WAVE_INTEGRATION_07_SHIPPING_ECONOMY_DECISION.md`
- Human checklist: `docs/validation/WAVE_INTEGRATION_07_HUMAN_PLAYMODE_CHECKLIST.md`
- Unity wiring instructions: `docs/validation/WAVE_INTEGRATION_07_HUMAN_UNITY_SHIPPING_WIRING_INSTRUCTIONS.md`
- Zone map: `docs/validation/WAVE_INTEGRATION_04_FARMSCENE_ZONE_MAP.md`
