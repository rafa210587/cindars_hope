# WAVE_INTEGRATION_14 — Human Play Mode Checklist

> **Date:** 2026-06-10
> **Status:** PENDING_HUMAN_PLAYMODE — requires Unity Editor + FarmScene human wiring
> **Prerequisite:** Complete `WAVE_INTEGRATION_14_HUMAN_UNITY_CRAFTING_WIRING_INSTRUCTIONS.md` first

---

## Prerequisite Human Wiring (before Play Mode)

- [ ] CraftingRuntime added to FarmScene manager object
- [ ] RecipeDatabaseSO wired on CraftingRuntime
- [ ] CraftingPoint GameObjects placed in FarmScene (at least 1: Workbench)
- [ ] CraftingModal placed in FarmScene Canvas, wired to CraftingRuntime + ModalManager
- [ ] At least 1 smoke test recipe exists in RecipeDatabase.asset
- [ ] CraftingStationRuntimeBootstrap will auto-bind InventoryManager at runtime

---

## Checklist

### Station Interaction

- [ ] Step 1: Open FarmScene in Play Mode
- [ ] Step 2: Walk player character to CraftingPoint collider trigger zone
- [ ] Step 3: Observe interaction prompt appears ("Craftar - Workbench" or similar)
- [ ] Step 4: Press Interact (E key or default interaction key)
- [ ] Step 5: CraftingModal opens showing recipe list
- [ ] Step 6: Confirm `CraftingStationOpenedEvent` is published (check console or DebugHud)

### Recipe List Display

- [ ] Step 7: Recipe list shows at least 1 recipe from RecipeDatabase (e.g. recipe_workbench_processed_wood)
- [ ] Step 8: Recipe detail shows: ingredients required, output, station type, craft time
- [ ] Step 9: If player has 0 wood: recipe shows "Missing ingredient" or similar blocked state
- [ ] Step 10: If player has required wood: recipe shows "Can craft" state

### Instant Craft Flow

- [ ] Step 11: Use debug loadout (CindarsHope/Integration/Debug/Provision Farm Smoke Loadout) to add wood to inventory
- [ ] Step 12: Open crafting modal at Workbench station
- [ ] Step 13: Select recipe_workbench_processed_wood (Workbench, instant)
- [ ] Step 14: Press Craft action
- [ ] Step 15: Observe: wood consumed from inventory, processed wood added to inventory
- [ ] Step 16: Confirm `ItemCraftedEvent` published (check console)
- [ ] Step 17: Confirm `CraftingOutputCollectedEvent` published
- [ ] Step 18: Inventory count is correct (wood decreased, processed wood increased)

### Processing (Timed) Craft Flow

- [ ] Step 19: If Forge recipe available: select recipe_forge_iron_sword (requires iron_ore x4, timed)
- [ ] Step 20: Seed inventory: use debug loadout or manual AddItem
- [ ] Step 21: Start craft at Forge station
- [ ] Step 22: Observe: iron ore consumed, CraftingJobStartedEvent published, station shows "In Progress"
- [ ] Step 23: Wait for timer to complete (or set CraftTimeSeconds = 1 in test recipe)
- [ ] Step 24: Observe: CraftingJobCompletedEvent published, station shows "Ready to Collect"
- [ ] Step 25: Press Collect
- [ ] Step 26: Observe: iron sword added to inventory, ItemCraftedEvent + CraftingOutputCollectedEvent published

### Idempotency Checks

- [ ] Step 27: After collecting, press Collect again on same station → "No completed output to collect."
- [ ] Step 28: Start a new craft immediately after collect → Succeeds (station is idle)
- [ ] Step 29: Start timed craft, then press Cancel → Ingredients refunded, CraftingJobCancelledEvent published
- [ ] Step 30: Station is idle again after cancel

### Modal Close Behavior

- [ ] Step 31: Open crafting modal
- [ ] Step 32: Press Escape → modal closes
- [ ] Step 33: Player can move again after close
- [ ] Step 34: CraftingStationClosedEvent published on close

### Failure Cases

- [ ] Step 35: Open recipe with missing ingredients → Cannot craft, shows failure reason
- [ ] Step 36: Open recipe requiring higher station level → Cannot craft, shows station level reason
- [ ] Step 37: Fill inventory completely, attempt instant craft → "Inventory is full for crafted output.", no items consumed

### Console Validation

- [ ] Step 38: No NullReferenceException in console during crafting flow
- [ ] Step 39: No "CraftingRuntime requires InventoryManager" error (bootstrap should have wired it)
- [ ] Step 40: No "Station X does not accept" errors for recipes correctly mapped to station type

---

## Acceptance Gate

| Criterion | Status |
|---|---|
| Station interaction + modal open | PENDING |
| Recipe list visible | PENDING |
| Instant craft: consume + produce | PENDING |
| Processing craft: timer + collect | PENDING |
| Idempotency: no double collect | PENDING |
| Cancel: ingredient refund | PENDING |
| Failure cases handled | PENDING |
| No NullRef or unexpected errors | PENDING |

**Play Mode Result:** PENDING — requires human Unity action first

---

*Generated: WAVE_INTEGRATION_14 (2026-06-10)*
*Must be completed before WAVE_INTEGRATION_14 can be marked ACCEPTED*
