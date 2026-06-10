# WAVE_INTEGRATION_14 — Recipe + Station Authoring Model

> **Date:** 2026-06-10
> **For:** Game designers and human Unity operators adding crafting content

---

## 1. How to Add an Instant Recipe

1. Unity Editor → `Assets/_Game/Data/Recipes/`
2. Right-click → Create → CindarsHope → Craft → Recipe
3. Set fields:
   - `Id`: unique string e.g. `recipe_workbench_wood_plank`
   - `DisplayName`: "Wood Plank"
   - `RequiredStationType`: WorkshopType.Workbench
   - `RequiredWorkshopLevel`: 1
   - `CraftTimeSeconds`: 0 (instant)
   - `Ingredients`: [{ItemId="item_material_wood", Amount=2}]
   - `OutputItemId`: "item_material_wood_plank"
   - `OutputAmount`: 1
   - `IsUnlockedByDefault`: true (or false for gated)
4. Add to `Assets/_Game/Data/Registries/RecipeDatabase.asset` → All array

---

## 2. How to Add a Processing Recipe (Timed)

Same as instant recipe, but:
- `CraftTimeSeconds`: > 0 (e.g. 30 = 30 seconds of processing)
- The crafting station will start a `CraftingJob` instead of delivering immediately
- Player must return to station when complete and press Collect

---

## 3. How to Add a Crafting Station to FarmScene

1. Open `Assets/_Game/Scenes/FarmScene.unity` in Unity Editor
2. Create empty GameObject named `CraftingStation_Workbench_01`
3. Add component: `CraftingPoint` (via CindarsHope.Craft namespace)
4. Set `_stationInstanceId`: `"station_basic_workbench"` (must be unique)
5. Set `_stationType`: WorkshopType.Workbench
6. Wire `_craftingRuntime`: drag the CraftingRuntime manager object from scene
7. Wire `_craftingModal`: drag the CraftingModal UI object from scene
8. Add `BoxCollider2D` (IsTrigger = true), set Size to (~1.5, 1.5)
9. Position near construction area or farm building

---

## 4. How to Add a Processing Station (different type)

Same as above, but:
- Use WorkshopType.Forge, CookingStation, Alchemy etc.
- Set `_stationInstanceId` to a unique ID e.g. `"station_basic_forge"`
- The same `CraftingRuntime` manages all station types in the scene

---

## 5. How to Add CraftingRuntime to FarmScene

1. Create empty GameObject named `CraftingManager_Farm`
2. Add component: `CraftingRuntime`
3. Wire `_inventoryManager`: drag InventoryManager from GameBootstrap (or let `CraftingStationRuntimeBootstrap` auto-wire)
4. Wire `_recipeDatabase`: assign `Assets/_Game/Data/Registries/RecipeDatabase.asset`
5. Wire `_staminaManager`: drag StaminaManager (optional, for stamina cost recipes)

**Auto-wiring (optional):** `CraftingStationRuntimeBootstrap` (RuntimeInitializeOnLoadMethod) auto-binds
InventoryManager and StaminaManager from `GameBootstrap.Instance` at AfterSceneLoad.
You may leave `_inventoryManager` unset in the prefab and rely on the bootstrap.

---

## 6. How to Require Skill / Workshop Unlock

On `RecipeDataSO`:
- `RequiredSkillNodeId`: string ID of skill node required (from DefaultSkillCatalog)
- `UnlockConditionIds`: additional unlock condition IDs
- `RequiredPlayerLevel`: minimum player level
- `RequiredWorkshopLevel`: minimum station level (CraftingStation checks `StationLevel`)

Station level is set at runtime via `CraftingStation.StationLevel` (default 1, upgraded via game systems).

---

## 7. How to Define Inputs / Outputs

`RecipeDataSO.Ingredients` is an array of `RecipeIngredient`:
```
RecipeIngredient {
    string ItemId;  // must match ItemDatabase
    int Amount;     // >= 1
}
```

`RecipeDataSO.OutputItemId` + `OutputAmount` define what is produced.

Multiple outputs: currently not supported in the canonical runtime — one output item ID.
Multi-output workaround: author multiple recipes or create a kit item ID.

---

## 8. How to Prevent Duplicate Collect

This is handled automatically by `CraftingStation`:
- After `TryCollectOutput()` succeeds → `_job = null`
- Next call to `TryCollectOutput()` → `HasCompletedOutput` = false → "No completed output to collect."
- Player cannot collect same job twice

---

## 9. How to Connect UI to Service

The UI chain is:
```
CraftingPoint.Interact(gameObject)
  → CraftingRuntime.GetOrCreateStation(stationId, stationType)
  → CraftingModal.Open(station)  ← modal opens, shows recipes
  → CraftingModal.ExecuteAction("Craft")
  → CraftingRuntime.TryStartCraft(station, recipe)
  → CraftingStation.TryStartCraft(recipe, inventoryManager, ...) ← inventory ops here
```

`CraftingModal` must be wired to `CraftingRuntime` via `[SerializeField] CraftingRuntime _runtime`.

---

## 10. Known Debts

| Debt | Detail | Impact |
|---|---|---|
| Canvas UI prefab not wired | CraftingModal needs Canvas prefab wired by human | Play Mode: manual wiring required |
| Smoke test recipes unconfirmed | item_kit_repair_basic, item_herb_dried_lavender, item_bait_simple not confirmed in ItemDatabase | Fallback: use recipe_workbench_processed_wood (confirmed) |
| Multi-output recipes | Currently one OutputItemId supported | No multi-drop recipe until CraftingStation extended |
| Advanced recipe filters | RecipeDataSO.RequiredQuestFlag not checked by CraftingStation (only by CraftingService in alt namespace) | No quest-gated recipe blocking for now |
| RecipeRuntimeView / ProcessingJobRuntimeView | UI detail views not created as separate prefabs | CraftingModal handles all display |

---

*Generated: WAVE_INTEGRATION_14 (2026-06-10)*
