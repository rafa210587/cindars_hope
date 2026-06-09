# WAVE_INTEGRATION_10 — Skill Tree UI + Active Skill Equip — Decision Document

**Date:** 2026-06-08
**Branch:** dev
**Status:** REUSE_EXISTING_CONTROLLERS

---

## Decision Summary

**Strategy:** REUSE_EXISTING_CONTROLLERS — no new UI controller needed.

The skill tree runtime and UI are already fully implemented. The spec's Play Mode goals are achievable without creating any new MonoBehaviour panels or duplicating existing code.

---

## System Audit Results

### Existing controllers found

| Controller | Type | Status |
|------------|------|--------|
| `SkillTreeGameplayPanelController` | IMGUI singleton, `RuntimeInitializeOnLoadMethod` | FULLY_IMPLEMENTED — open/close, node list, purchase, active slot assignment |
| `SkillTreePanel` (UI/Skills/) | Canvas-based `ModalBase`, requires prefab | EXISTS but deferred (prefab not wired in CreateMvpFarmScene) |
| `SkillTreeInputHandler` | Canvas companion, requires `SkillTreePanel` prefab | EXISTS but deferred (not wired in CreateMvpFarmScene) |
| `SkillTreeMenuViewModel` | ViewModel with `ActiveSlotSummary` | EXISTS, pure C# |

### Runtime managers found

| Manager | Status |
|---------|--------|
| `SkillTreeManager` | FULLY_IMPLEMENTED — purchase, active slot assign/clear, save/load, event publishing |
| `SkillTreeState` | FULLY_IMPLEMENTED — available points, purchased nodes, 4 active slots |
| `DefaultSkillCatalog` | FULLY_IMPLEMENTED — 55 nodes across 5 trees (melee/ranged/magic/survival/crafting) |
| `SkillTreeRegistrySO` | EXISTS — ScriptableObject registry (optional; DefaultSkillCatalog used as fallback) |
| `SkillNodeDatabaseSO` | EXISTS — ScriptableObject database (optional; DefaultSkillCatalog used as fallback) |
| `PlayerProgressionManager` | FULLY_IMPLEMENTED — `UnspentSkillPoints`, `TrySpendSkillPoints` |

### Events found

| Event | Direction | Status |
|-------|-----------|--------|
| `SkillTreeOpenedEvent` | `GameplayInputRouter` → panel | PUBLISHED by GameplayInputRouter on U key; NOT subscribed by panel |
| `SkillTreeClosedEvent` | Panel → bus | PUBLISHED by SkillTreePanel (Canvas); NOT published by GameplayPanelController (gap) |
| `ActiveSkillSlotAssignedEvent` | SkillTreeManager → bus | FULLY_IMPLEMENTED |
| `ActiveSkillSlotClearedEvent` | SkillTreeManager → bus | FULLY_IMPLEMENTED |
| `SkillPointGrantedEvent` | SkillTreeManager → bus | FULLY_IMPLEMENTED |
| `SkillDerivedStatsChangedEvent` | SkillTreeManager → bus | FULLY_IMPLEMENTED |

### Input/focus

| Item | Status |
|------|--------|
| `GameplayInputRouter` U key | PUBLISHES `SkillTreeOpenedEvent` — but panel did not subscribe (gap) |
| `SkillTreeGameplayPanelController` U key | Self-handled when `GameplayInputRouter.IsActive == false` — covered |
| `ModalManager.PushModal(SkillTree)` | Called in `Toggle()` — input is blocked to gameplay while open |
| `ModalType.SkillTree` | DEFINED in `ModalType` enum |
| `UIFocusState.SkillTreeFocus` | DEFINED in `InputFocusModalRoutingModel` |

---

## Gaps Identified and Resolution

### Gap 1: SkillTreeGameplayPanelController does not subscribe to SkillTreeOpenedEvent

**Problem:** When `GameplayInputRouter` is active and publishes `SkillTreeOpenedEvent` (U key), the IMGUI panel controller yields its own key handling but never opens because it doesn't subscribe to the event.

**Resolution:** Added `OnEnable`/`OnDisable` with `GameEventBus.Subscribe/Unsubscribe<SkillTreeOpenedEvent>` in `SkillTreeGameplayPanelController`. The handler calls `Toggle()` guarded against double-open.

### Gap 2: DebugHud does not display active skill slots

**Problem:** The HUD shows skill points and hotbar but not the active skill slots (R/T/Y/G). This was explicitly deferred in WAVE_INTEGRATION_08 as `ACTIVE_SKILL_RUNTIME_DEFERRED_TO_WAVE_INTEGRATION_10_11`.

**Resolution:** Added `DrawActiveSkillSlots()` method to `DebugHud.cs` that reads `GameBootstrap.Instance.SkillTreeManager.State.GetActiveSlotSkillActionId(i)` for slots 0-3 (R/T/Y/G). Called from `DrawInfoPanel()` after `DrawProgressionState()`.

### Gap 3: No editor validator for skill tree runtime binding

**Resolution:** Created `ValidateSkillTreeRuntimeBinding.cs` under `Assets/_Game/Scripts/Editor/Validation/` with `[MenuItem("CindarsHope/Validate/Validate Skill Tree Runtime", priority = 48)]`. Validates DefaultSkillCatalog node count, SkillTreeState contracts, GameBootstrap wiring.

---

## Files Changed

| File | Action |
|------|--------|
| `Assets/_Game/Scripts/UI/Skills/SkillTreeGameplayPanelController.cs` | MODIFIED — added GameEventBus subscription for SkillTreeOpenedEvent |
| `Assets/_Game/Scripts/UI/DebugHud.cs` | MODIFIED — added DrawActiveSkillSlots(), CindarsHope.Skills using |
| `Assets/_Game/Scripts/Editor/Validation/ValidateSkillTreeRuntimeBinding.cs` | CREATED — editor validator |

---

## What is NOT in scope for WAVE_INTEGRATION_10

- Skill effect gameplay behavior (combat modifiers, farm yield, etc.) — WAVE_INTEGRATION_11
- Canvas-based SkillTreePanel prefab wiring — requires Unity Editor UI prefab creation
- SkillTreeInputHandler wiring in CreateMvpFarmScene — deferred (Canvas path not active)
- New skill tree SO assets (DefaultSkillCatalog is the authoritative fallback)
- Save/load changes (SkillTreeSaveData already implemented)

---

## CreateMvpFarmScene.cs changes

**None required.** `SkillTreeManager` is already added to `_Bootstrap` (line 142) and wired via `SetReference` (line 173). `SkillTreeGameplayPanelController` creates itself via `RuntimeInitializeOnLoadMethod`. `GameplayInputRouter` is already added (WAVE_INTEGRATION_09).

---

## Data-driven compliance

All node data comes from `DefaultSkillCatalog.BuildAllNodes()` / `BuildAllTrees()` (55 nodes, 5 trees). No hardcoded node list in UI. Inspector-assigned `SkillTreeRegistrySO` + `SkillNodeDatabaseSO` take priority if present; code catalog is fallback. This satisfies the principle-data-driven requirement.

---

*Created: 2026-06-08 (WAVE_INTEGRATION_10)*
