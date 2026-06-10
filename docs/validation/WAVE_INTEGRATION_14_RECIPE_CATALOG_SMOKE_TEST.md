# WAVE_INTEGRATION_14 — Recipe Catalog: Smoke Test

> **Date:** 2026-06-10
> **Status:** TEMPORARY_CRAFTING_TEST_RECIPE — smoke test only; not canonical game balance
> **Note:** All smoke test recipes use `IsUnlockedByDefault = true` and no gating.

---

## Item ID Audit

The following item IDs were confirmed present in `ItemDatabase.asset` via prior wave audits
(WAVE_INTEGRATION_06A_DEBUG_LOADOUT_ITEM_USE_AUDIT.md, ValidateCraftingSystem.cs):

| Item ID | Type | Source Evidence |
|---|---|---|
| `item_material_wood` | Material | ValidateCraftingSystem.cs seed test |
| `item_material_iron_ore` | Material | ValidateCraftingSystem.cs seed test |
| `item_material_processed_wood` | Material | ValidateCraftingSystem.cs expected output |
| `item_material_iron_sword` | Equipment | ValidateCraftingSystem.cs expected output |
| `item_material_bread` | Food | ValidateCraftingSystem.cs seed test |
| `item_herb_lavender` | Herb | WAVE06A debug loadout audit |
| `item_fish_basic` | Fish | WAVE07 sell point audit |

**Note:** Items flagged UNCONFIRMED may not exist in the database and require human verification before adding recipes that use them.

| Item ID | Type | Status |
|---|---|---|
| `item_kit_repair_basic` | Consumable | UNCONFIRMED — verify in ItemDatabase |
| `item_herb_dried_lavender` | Processed herb | UNCONFIRMED — verify in ItemDatabase |
| `item_bait_simple` | Consumable | UNCONFIRMED — verify in ItemDatabase |

---

## Smoke Test Recipes

All 3 recipes below are **TEMPORARY_CRAFTING_TEST_RECIPE**. They should be replaced with
canonical balance-reviewed recipes once item IDs are confirmed.

---

### Recipe 1: recipe_basic_repair_kit

| Field | Value |
|---|---|
| ID | `recipe_basic_repair_kit` |
| Display name | "Basic Repair Kit" |
| Station type | WorkshopType.Workbench |
| Station level | 1 |
| Is instant | YES (`CraftTimeSeconds = 0`) |
| Ingredients | `item_material_wood` x2, `item_material_iron_ore` x1 |
| Output | `item_kit_repair_basic` x1 |
| Unlock | `IsUnlockedByDefault = true` |
| Status | TEMPORARY_CRAFTING_TEST_RECIPE (item_kit_repair_basic unconfirmed) |

**Fallback if item_kit_repair_basic absent:** use `item_material_processed_wood` as output
(confirmed in database).

---

### Recipe 2: recipe_dried_herbs

| Field | Value |
|---|---|
| ID | `recipe_dried_herbs` |
| Display name | "Dried Herbs" |
| Station type | WorkshopType.Workbench |
| Station level | 1 |
| Is instant | NO (`CraftTimeSeconds = 30`) — Processing recipe |
| Ingredients | `item_herb_lavender` x3 |
| Output | `item_herb_dried_lavender` x2 |
| Unlock | `IsUnlockedByDefault = true` |
| Status | TEMPORARY_CRAFTING_TEST_RECIPE (item_herb_dried_lavender unconfirmed) |

**Fallback if herbs absent:** use Workbench instant with confirmed items for smoke test.

---

### Recipe 3: recipe_simple_bait

| Field | Value |
|---|---|
| ID | `recipe_simple_bait` |
| Display name | "Simple Fishing Bait" |
| Station type | WorkshopType.Workbench |
| Station level | 1 |
| Is instant | YES (`CraftTimeSeconds = 0`) |
| Ingredients | `item_herb_lavender` x1, `item_material_wood` x1 |
| Output | `item_bait_simple` x2 |
| Unlock | `IsUnlockedByDefault = true` |
| Status | TEMPORARY_CRAFTING_TEST_RECIPE (item_bait_simple unconfirmed) |

---

## Recipe State Coverage

The `CraftingStation` + `CraftingRuntime` + `CraftingModal` cover all 8 required states:

| State | Recipe Type | How Triggered | Evidence |
|---|---|---|---|
| KnownCraftable | recipe_basic_repair_kit | IsUnlockedByDefault=true + ingredients present | CraftingStation.CanStartCraft() → true |
| KnownMissingMaterials | Any recipe | ingredients missing | CraftingStation.ValidateIngredients() → failureReason set |
| KnownLockedBySkill | Any recipe | RequiredSkillNodeId set + not purchased | CraftingStation.CanStartCraft() checks IsUnlockedByDefault |
| KnownLockedByLevel | Any recipe | RequiredWorkshopLevel > current | CraftingStation.CanStartCraft() level check |
| KnownLockedByRecipe | Any recipe | IsUnlockedByDefault = false | CanStartCraft() → "Recipe is locked." |
| ProcessingInProgress | recipe_dried_herbs | CraftTimeSeconds > 0, job started | CraftingJob.Status = InProgress |
| ProcessingReadyToCollect | recipe_dried_herbs | deltaTime elapsed | CraftingJob.Status = Completed |
| Collected | recipe_dried_herbs | TryCollectOutput() | _job = null, event published |

---

## Existing Recipes (from ValidateCraftingSystem.cs audit)

Pre-existing smoke test recipes confirmed in RecipeDatabase.asset:

| Recipe ID | Station | Type |
|---|---|---|
| `recipe_pocket_processed_wood` | None (pocket) | Instant |
| `recipe_workbench_processed_wood` | Workbench | Instant |
| `recipe_forge_iron_sword` | Forge | Timed (processing) |
| `recipe_cooking_bread` | CookingStation | Timed (processing) |

These existing recipes already cover the instant + timed recipe smoke test.
No new recipes strictly required for WAVE14 code validation — existing recipes suffice.

---

## How to Create RecipeDataSO Assets

1. Unity Editor → Assets/_Game/Data/Recipes/ (create folder if missing)
2. Right-click → Create → CindarsHope → Craft → Recipe
3. Set Id, DisplayName, RequiredStationType, CraftTimeSeconds, Ingredients, OutputItemId, OutputAmount
4. Assign to RecipeDatabase.asset → All array

---

*TEMPORARY_CRAFTING_TEST_RECIPE — replace with canonical balance-reviewed recipes before final acceptance*
*Generated: WAVE_INTEGRATION_14 (2026-06-10)*
