# WAVE_INTEGRATION_14 — Crafting Station + Processing Jobs: Decision Document

> **Date:** 2026-06-10
> **Status:** CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED
> **Spec:** WAVE_INTEGRATION_14_crafting_station_processing_jobs.md

---

## 1. Baseline Dependency

| Dependency | Status | Evidence |
|---|---|---|
| WAVE_INTEGRATION_13 | CODE_READY_HUMAN_UNITY_ACTION_REQUIRED_BUILD_VALIDATED | commit 315cfda |
| Assembly-CSharp before | PASS (0E/0W) | pre-flight build |
| Assembly-CSharp-Editor before | PASS (0E/0W) | pre-flight build |
| Branch | dev | verified pre-flight |

WAVE13 scene wiring (gates + anchors + Build Settings) is pending human action in Unity Editor. This spec uses FarmScene as crafting target without depending on WAVE13 scene wiring completion.

---

## 2. Crafting Runtime Audit

| System / File | Found | Namespace | Decision |
|---|---|---|---|
| `CraftingRuntime.cs` | YES | `CindarsHope.Craft` | USE_EXISTING — full lifecycle MonoBehaviour |
| `CraftingStation.cs` | YES | `CindarsHope.Craft` | USE_EXISTING — validate/consume/craft/collect/cancel |
| `CraftingJob.cs` + `CraftingJobSaveData` | YES | `CindarsHope.Craft` | USE_EXISTING — processing job with save/load |
| `CraftingPoint.cs` (IInteractable) | YES | `CindarsHope.Craft` | USE_EXISTING — station interactable in scene |
| `CraftingModal.cs` (UI, ModalManager-aware) | YES | `CindarsHope.UI.Crafting` | USE_EXISTING — full UI layer |
| `CraftingMenuViewModel.cs` | YES | `CindarsHope.UI.Crafting` | USE_EXISTING — view model |
| `CraftingRecipeViewModel.cs` | YES | `CindarsHope.UI.Crafting` | USE_EXISTING — recipe view model |
| `RecipeDataSO.cs` | YES | `CindarsHope.Craft.Data` | USE_EXISTING — SO recipe definition |
| `RecipeDatabaseSO.cs` | YES | `CindarsHope.Craft.Data` | USE_EXISTING — recipe registry SO |
| `WorkshopType.cs` | YES | `CindarsHope.Craft.Data` | USE_EXISTING — Workbench/Forge/CookingStation/Alchemy/Carpentry/Sewing |
| `CraftingEvents.cs` | YES | `CindarsHope.Core.Events` | USE_EXISTING — 6 event types (Opened/Closed/JobStarted/JobCompleted/Cancelled/OutputCollected/Failed) |
| `ItemCraftedEvent.cs` | YES | `CindarsHope.Core.Events` | USE_EXISTING |
| `CraftingManager.cs` | YES (in GameBootstrap) | `CindarsHope.Craft` | USE_EXISTING — lighter manager; CraftingRuntime is the full loop |
| `ValidateCraftingSystem.cs` | YES | `CindarsHope.Editor.Validation` | USE_EXISTING — validates SPEC07 recipes |
| `CraftingService.cs` (alt namespace) | YES | `CindarsHope.Crafting` | PARALLEL_NAMESPACE — not primary runtime path; canonical path is `CindarsHope.Craft` |
| `RecipeDefinition.cs` (alt namespace) | YES | `CindarsHope.Crafting` | PARALLEL_NAMESPACE — not primary path |
| `ProcessingJob.cs` (alt namespace) | YES | `CindarsHope.Crafting` (via CraftingRequest.cs) | PARALLEL_NAMESPACE — not primary path |

**Strategy: USE_EXISTING_CRAFTING_RUNTIME**

The canonical crafting path is `CindarsHope.Craft.*` + `CindarsHope.UI.Crafting.*`. This path is fully implemented including:
- Ingredient validation before consume
- Rollback on failure (RestoreFromSaveData)
- Instant craft with output add
- Timed job with Update() lifecycle
- Collect with idempotency (only collect once, then job = null)
- Cancel with ingredient refund
- Save/load via `CraftingRuntimeSaveData`
- GameEventBus events for all transitions

No new crafting service, station, or job contract needed.

---

## 3. Inventory / Item API Audit

| API | Found | Method Signature | Decision |
|---|---|---|---|
| Add item to inventory | YES | `InventoryManager.AddItem(string itemId, int amount) : bool` | READY |
| Remove item from inventory | YES | `InventoryManager.RemoveItem(string itemId, int amount) : bool` | READY |
| Check if item present | YES | `InventoryManager.HasItem(string itemId, int amount) : bool` | READY |
| Rollback / snapshot | YES | `CaptureSaveData()` + `RestoreFromSaveData()` | READY |
| Inventory full check | YES | Returns `false` on AddItem when full | READY |

**Result: INVENTORY_API_COMPLETE — no BLOCKED_BY_INVENTORY_CONSUME_API_GAP**

---

## 4. UI Crafting Audit

| Component | Found | Status |
|---|---|---|
| CraftingModal (open/close UI) | YES | USE_EXISTING |
| CraftingMenuViewModel | YES | USE_EXISTING — KnownRecipes, BlockedRecipes, MissingInputs, ProcessingJobs |
| CraftingRecipeViewModel | YES | USE_EXISTING — CanCraft, RequiredInputs, BlockedReason |
| ProcessingJobViewModel | YES | USE_EXISTING — JobId, Progress%, IsReadyToCollect |
| Input handling (W/S/Esc) | YES in CraftingModal | USE_EXISTING |
| ModalManager integration | YES in CraftingModal | USE_EXISTING — checks HasActiveModal before pocket craft |

**UI Strategy: USE_EXISTING_CRAFTING_MODAL_AND_VIEWMODELS**

---

## 5. Processing / Time Audit

| Concern | Current Implementation | Strategy |
|---|---|---|
| Processing timer | `CraftingJob.RemainingSeconds` decremented by `Time.deltaTime` in `CraftingRuntime.Update()` | USE_EXISTING — real-time seconds |
| Processing job lifecycle | Queued → Processing (InProgress) → Completed → Collected | USE_EXISTING |
| Idempotency (no double collect) | YES — after `TryCollectOutput` success, `_job = null` | USE_EXISTING |
| Save/load of processing jobs | YES — `CraftingJobSaveData` persists all fields | BUILD_VALIDATED |
| Time tick vs real-time | `RecipeDataSO.CraftTimeSeconds` (float, real-time) | Design-aligned |

**Processing Strategy: USE_EXISTING_CRAFTING_RUNTIME_JOB_LIFECYCLE**

---

## 6. Target Scene Strategy

| Item | Decision |
|---|---|
| Target scene | FarmScene (human wiring) |
| Station placement | Human must add CraftingPoint GameObjects to FarmScene |
| CraftingRuntime placement | Human must add CraftingRuntime MonoBehaviour to a manager object in FarmScene |
| InventoryManager binding | CraftingStationRuntimeBootstrap (new) wires it at AfterSceneLoad |
| Debt label | CODE_READY_HUMAN_UNITY_ACTION_REQUIRED |
| WAVE13 dependency | Not blocking — FarmScene used without WAVE13 gate wiring |

---

## 7. Crafting Station Interactable Strategy

| Decision | Value |
|---|---|
| Interactable type | CraftingPoint (existing IInteractable MonoBehaviour) |
| Station ID format | "station_basic_workbench", "station_basic_forge", etc. |
| Station type | WorkshopType.Workbench / WorkshopType.Forge |
| Opening mechanism | CraftingPoint.Interact() → CraftingRuntime.GetOrCreateStation() → CraftingModal.Open(station) |
| GameEventBus event | CraftingStationOpenedEvent published after Open() |
| New code required | CraftingStationRuntimeBootstrap.cs (bootstrap wiring only) |

---

## 8. Recipe Data Strategy

| Item | Decision |
|---|---|
| Recipe storage | RecipeDataSO ScriptableObjects in RecipeDatabaseSO |
| Smoke test recipes | TEMPORARY_CRAFTING_TEST_RECIPE — in-database via initializer or manual human creation |
| Recipe definition source | `docs/validation/WAVE_INTEGRATION_14_RECIPE_CATALOG_SMOKE_TEST.md` |
| Permanent recipe data | Human creates SO assets in Unity Editor via `Assets/_Game/Data/Recipes/` |
| Database path | `Assets/_Game/Data/Registries/RecipeDatabase.asset` |

---

## 9. Save / Load Strategy

| Item | Decision |
|---|---|
| CraftingJob save | IMPLEMENTED — `CraftingJobSaveData` serialized in `CraftingRuntimeSaveData` |
| Processing job persists across reload | YES — via `CraftingRuntime.LoadFromSaveData()` |
| Debt | None — existing save/load is complete for crafting jobs |

---

## 10. Skill / Workshop Unlock Strategy

| Field | RecipeDataSO |
|---|---|
| Required skill | `RequiredSkillNodeId` (string) |
| Required recipe unlock | `UnlockConditionIds` (string[]) |
| Required workshop level | `RequiredWorkshopLevel` (int) |
| Required player level | `RequiredPlayerLevel` (int) |
| Smoke test recipes | `IsUnlockedByDefault = true` (no gating) |

**Skill/workshop unlock status: IMPLEMENTED_IN_SCHEMA, SMOKE_TEST_RECIPES_BYPASS_GATING**

---

## 11. Design / Direction Compliance Matrix

| Direction Principle | Compliant? | Evidence |
|---|---|---|
| Crafting only at workshops/stations | YES | CraftingPoint requires stationInstanceId; CraftingStation validates WorkshopType match |
| Data-driven by ScriptableObject/config/registry | YES | RecipeDataSO + RecipeDatabaseSO |
| Recipe validates station, ingredients, unlocks | YES | CraftingStation.CanStartCraft() checks: type, level, unlock, ingredients |
| Processing idempotent — no double collect | YES | TryCollectOutput sets _job = null after success; Collect() checks HasCompletedOutput |
| UI does not manipulate inventory directly — service is owner | YES | CraftingRuntime.TryStartCraft() and TryCollect() own all inventory ops |
| MVP uses temporary test data | YES | Smoke recipes marked TEMPORARY_CRAFTING_TEST_RECIPE |

---

## 12. Code Created (WAVE_INTEGRATION_14 New Files)

| File | Type | Purpose |
|---|---|---|
| `Assets/_Game/Scripts/Craft/CraftingStationRuntimeBootstrap.cs` | Runtime MonoBehaviour | Wires CraftingRuntime to InventoryManager/StaminaManager from GameBootstrap at AfterSceneLoad |
| `Assets/_Game/Scripts/Editor/Validation/ValidateCraftingProcessingRuntimeBinding.cs` | Editor tool | Validates WAVE14 code contracts and docs are in place |

---

## 13. Human Unity Actions Required

| Action | Target | Instructions |
|---|---|---|
| Add CraftingRuntime to FarmScene | FarmScene manager object | See WAVE14 human wiring instructions |
| Wire RecipeDatabaseSO | CraftingRuntime._recipeDatabase | Assign `Assets/_Game/Data/Registries/RecipeDatabase.asset` |
| Wire InventoryManager | CraftingRuntime._inventoryManager | Assign via serialized ref (or let bootstrap handle it) |
| Add CraftingPoint to FarmScene | New GameObject "CraftingStation_Workbench_01" | Add CraftingPoint, set StationId + StationType, add BoxCollider2D trigger |
| Add CraftingModal to FarmScene | UI Canvas child | Wire _runtime + _modalManager |
| Create smoke test RecipeDataSO assets | Assets/_Game/Data/Recipes/ | Use CindarsHope/Craft/Recipe menu |

---

*Generated: WAVE_INTEGRATION_14 (2026-06-10)*
