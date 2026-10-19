# WAVE_INTEGRATION_14 — Human Data: Recipe + Item Instructions

> **Date:** 2026-06-10
> **For:** Human adding recipe ScriptableObject assets and wiring RecipeRegistry to ScriptableObjects

---

## 1. Verify Item IDs in ItemDatabase

Before creating recipe assets, confirm item IDs exist in `ItemDatabase.asset`:

1. Open `Assets/_Game/Data/Registries/ItemDatabase.asset` in Unity Inspector
2. Expand `All` array
3. Confirm presence of:
   - `item_material_wood`
   - `item_material_processed_wood`
   - `item_material_iron_ore`
   - `item_herb_lavender` (if herb recipes desired)

For any item ID not present, either:
- Use a substitute confirmed ID
- Create a new ItemDataSO (follow existing pattern in ItemDatabase folder)

---

## 2. Confirm Existing Smoke Test Recipes

Run `CindarsHope/Validate/Validate Crafting System (SPEC07)` in Unity Editor.
This confirms these recipes exist in RecipeDatabase.asset:
- `recipe_pocket_processed_wood`
- `recipe_workbench_processed_wood` (Workbench, instant, needs 2x wood)
- `recipe_forge_iron_sword` (Forge, timed, needs 4x iron_ore)
- `recipe_cooking_bread` (CookingStation, timed)

If these pass, no new smoke test recipes strictly required for WAVE14.

---

## 3. Add New Recipe ScriptableObject (Optional)

1. Unity Editor → `Assets/_Game/Data/Recipes/` (create folder if missing)
2. Right-click → Create → CindarsHope → Craft → Recipe
3. Set fields in Inspector:
   - **Id**: e.g. `recipe_basic_repair_kit` (must be unique)
   - **DisplayName**: "Basic Repair Kit"
   - **RequiredStationType**: Workbench
   - **RequiredWorkshopLevel**: 1
   - **CraftTimeSeconds**: 0 (instant) or > 0 (timed)
   - **Ingredients**: add entries [{ItemId="item_material_wood", Amount=2}, {ItemId="item_material_iron_ore", Amount=1}]
   - **OutputItemId**: `item_kit_repair_basic` (must exist in ItemDatabase)
   - **OutputAmount**: 1
   - **IsUnlockedByDefault**: true (no gating for smoke test)
4. Save the asset

---

## 4. Register Recipe in RecipeDatabase.asset

1. Open `Assets/_Game/Data/Registries/RecipeDatabase.asset`
2. Expand `All` array
3. Click `+` to add new element
4. Drag the new RecipeDataSO into the slot
5. Save

---

## 5. Wire RecipeRegistry to ScriptableObjects (for non-smoke use)

The `CindarsHope.Crafting.RecipeRegistry` class (parallel namespace) holds a `List<RecipeDefinition>`.
This is separate from the primary `RecipeDatabaseSO` used by CraftingRuntime.

For production use, the canonical path is `RecipeDatabaseSO` (ScriptableObject-backed registry).
The `RecipeRegistry` in `CindarsHope.Crafting` namespace is a lightweight bridge not used by CraftingRuntime.

**Recommendation:** Use `RecipeDatabaseSO` exclusively for all recipe registration.

---

## 6. Validate After Adding Recipes

Run: `CindarsHope/Validate/Validate WAVE14 Crafting Processing Runtime`

This confirms:
- All required code files exist
- All required documentation files exist
- No forbidden scene-search patterns in bootstrap
- CraftingJob has Collected state

---

## 7. Known Item ID Gaps (TEMPORARY_CRAFTING_TEST_RECIPE)

| Item ID | In Database? | Action |
|---|---|---|
| `item_kit_repair_basic` | UNCONFIRMED | Either create ItemDataSO or use item_material_processed_wood as output |
| `item_herb_dried_lavender` | UNCONFIRMED | Either create ItemDataSO or skip dried herbs recipe |
| `item_bait_simple` | UNCONFIRMED | Either create ItemDataSO or skip bait recipe |

For smoke test validation, fallback to confirmed item IDs:
- Input: `item_material_wood` x2
- Output: `item_material_processed_wood` x1

---

*Generated: WAVE_INTEGRATION_14 (2026-06-10)*
