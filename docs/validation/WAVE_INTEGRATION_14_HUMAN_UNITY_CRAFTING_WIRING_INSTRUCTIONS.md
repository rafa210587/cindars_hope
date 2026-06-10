# WAVE_INTEGRATION_14 — Human Unity Crafting Wiring Instructions

> **Date:** 2026-06-10
> **For:** Human operating Unity Editor to wire crafting stations in FarmScene

---

## Prerequisites

- Unity Editor open with the project
- FarmScene loaded (`Assets/_Game/Scenes/FarmScene.unity`)
- RecipeDatabase.asset exists at `Assets/_Game/Data/Registries/RecipeDatabase.asset`
- At least 1 RecipeDataSO assigned to RecipeDatabase.asset (see optional doc 2)

---

## Step 1: Add CraftingRuntime Manager Object

1. In FarmScene Hierarchy, create empty GameObject named `CraftingManager_Farm`
2. Position at (0, 0, 0)
3. Add component: `CraftingRuntime` (CindarsHope.Craft namespace)
4. Wire `_recipeDatabase`: drag `Assets/_Game/Data/Registries/RecipeDatabase.asset`
5. Leave `_inventoryManager` empty — `CraftingStationRuntimeBootstrap` will auto-bind at Play Mode start
6. Optionally wire `_staminaManager` from scene

---

## Step 2: Add Workbench Crafting Station

1. Create empty GameObject named `CraftingStation_Workbench_01`
2. Position near construction area or farm buildings, e.g. (2, -3, 0)
3. Add component: `CraftingPoint` (CindarsHope.Craft namespace)
4. Set `_stationInstanceId`: `"station_basic_workbench"` (must be unique)
5. Set `_stationType`: WorkshopType.Workbench
6. Wire `_craftingRuntime`: drag `CraftingManager_Farm` from scene
7. Wire `_craftingModal`: drag CraftingModal object (see Step 4)
8. Add component: `BoxCollider2D`
   - Set `Is Trigger`: true
   - Set `Size`: (1.5, 1.5)
9. Add a SpriteRenderer or other visual to show station placement (optional for smoke test)

---

## Step 3: Add Optional Forge Station (for processing recipe smoke test)

1. Create empty GameObject named `CraftingStation_Forge_01`
2. Position e.g. (4, -3, 0)
3. Add component: `CraftingPoint`
4. Set `_stationInstanceId`: `"station_basic_forge"`
5. Set `_stationType`: WorkshopType.Forge
6. Wire `_craftingRuntime` and `_craftingModal` same as Workbench
7. Add `BoxCollider2D` (IsTrigger = true, Size = 1.5, 1.5)

---

## Step 4: Add CraftingModal to Canvas

1. In FarmScene Hierarchy, find or create a Canvas (Screen Space - Overlay, sort order above HUD)
2. Create child Panel named `CraftingModalPanel`
3. Add component: `CraftingModal` (CindarsHope.UI.Crafting namespace)
4. Wire `_runtime`: drag `CraftingManager_Farm`
5. Wire `_modalManager`: drag `ModalManager` (from GameBootstrap scene object or CindarsHope/UI/Modal)
6. Set `_pocketCraftKey`: KeyCode.C (default pocket crafting hotkey)

---

## Step 5: Verify CraftingStationRuntimeBootstrap Auto-Wire

`CraftingStationRuntimeBootstrap` is a RuntimeInitializeOnLoadMethod and runs automatically.
It will:
1. Create `CraftingStationRuntimeBootstrap` GameObject at AfterSceneLoad
2. Find all `CraftingRuntime` instances in the scene
3. Call `RebindInventoryManager(GameBootstrap.InventoryManager)` on each
4. Call `RebindStaminaManager(GameBootstrap.StaminaManager)` on each
5. Call `Initialize()` if not yet initialized

Verify in console: `[CraftingStationRuntimeBootstrap] Bound CraftingRuntime 'CraftingManager_Farm' to InventoryManager and StaminaManager.`

---

## Step 6: Enter Play Mode Smoke Test

1. Press Play in Unity Editor
2. Walk to CraftingStation_Workbench_01
3. Observe interaction prompt: "Craftar - Workbench"
4. Press E (interact key)
5. CraftingModal opens
6. Recipe list appears (recipe_workbench_processed_wood or similar)
7. If player has wood: press Craft → processed wood appears in inventory
8. Press Escape to close modal

---

## Troubleshooting

| Problem | Cause | Fix |
|---|---|---|
| "CraftingRuntime requires InventoryManager" in console | Bootstrap didn't bind | Check GameBootstrap.Instance is in scene and has InventoryManager wired |
| CraftingModal doesn't open | _craftingModal not wired on CraftingPoint | Wire CraftingModal reference |
| No recipes shown | RecipeDatabaseSO empty or not assigned | Assign RecipeDatabase.asset and add at least 1 RecipeDataSO |
| Interaction prompt not appearing | BoxCollider2D trigger missing | Add BoxCollider2D (IsTrigger = true) to CraftingPoint object |
| "Station X does not accept CraftRecipe" | WorkshopType mismatch | Match RecipeDataSO.RequiredStationType to CraftingPoint._stationType |

---

*Generated: WAVE_INTEGRATION_14 (2026-06-10)*
