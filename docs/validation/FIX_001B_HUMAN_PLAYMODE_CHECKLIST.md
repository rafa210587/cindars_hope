# FIX-001B Human Play Mode Checklist

Date: 2026-06-10
Branch: dev
Status: PENDING HUMAN EXECUTION

This checklist must be executed by a human in Unity Editor Play Mode to complete validation of FIX-001B.

---

## Prerequisites

1. Open Unity Editor with project at `D:\Projetos\Jogos\Cindars_hope\cindars_hope`
2. Confirm no compile errors in Console (Assembly-CSharp builds clean)
3. Have TownScene, FarmScene, and CaveScene available

---

## Checklist

### Section 1 — TownScene Initial Load

- [ ] 1.1 Open TownScene, press Play
- [ ] 1.2 Confirm zero red errors in Console regarding shop initialization (no "ItemId not found in ItemDatabaseSO" or similar)
- [ ] 1.3 Confirm Console shows no duplicate `QuestOfferPanelController` or `QuestLogPanelController` instances
- [ ] 1.4 Confirm no warning about `CaveRunManager.Instance is null` on TownScene load (expected: no cave manager here)

### Section 2 — NPC Shop Interaction (TownScene)

Walk to each NPC and interact to verify shops open:

- [ ] 2.1 Thalindra → interact → shop opens with items
- [ ] 2.2 Corvus → interact → shop opens with items
- [ ] 2.3 Savra → interact → shop opens with items
- [ ] 2.4 Mirela → interact → shop opens with items
- [ ] 2.5 Hund → interact → shop opens with items

### Section 3 — Quest Panel Behavior (TownScene)

- [ ] 3.1 Press J key → QuestLogPanelController opens (IMGUI log panel)
- [ ] 3.2 Press Esc → panel closes
- [ ] 3.3 Interact with NPC that has quest (Thalindra) → QuestOfferPanelController shows offer panel

### Section 4 — FarmScene CraftingRuntime

- [ ] 4.1 Open FarmScene, press Play
- [ ] 4.2 If FarmScene has CraftingPoint/CraftingRuntime in scene: confirm Console shows `[CraftingStationRuntimeBootstrap] Bound CraftingRuntime` message
- [ ] 4.3 If FarmScene has no CraftingRuntime: confirm Console shows `[CraftingStationRuntimeBootstrap] No CraftingRuntime found in scene` (not an error — expected)
- [ ] 4.4 Confirm no `FindObjectsOfType` or `FindObjectsByType` warning in Console (log "CLEAN" not required — just confirm no errors)

### Section 5 — Scene Reload Duplicate Guard

- [ ] 5.1 Play TownScene → stop Play Mode → play TownScene again (scene reload simulation)
- [ ] 5.2 Confirm Console shows only ONE `QuestOfferPanelController` and ONE `QuestLogPanelController` instance (duplicates are destroyed)
- [ ] 5.3 Confirm no doubled IMGUI rendering of quest panels

### Section 6 — Cave Entry/Exit (CaveScene)

- [ ] 6.1 Open CaveScene, press Play
- [ ] 6.2 Confirm Console shows `[CaveRuntimeBridge] CaveScene entry validated. CaveRunManager present.` (if CaveRunManager is wired)
- [ ] 6.3 If CaveRunManager is NOT wired: confirm warning `[CaveRuntimeBridge] CaveScene loaded but no CaveRunManager found.` appears (diagnostic only, not blocking)
- [ ] 6.4 Trigger CaveScene exit (or simulate via SceneTransitionStartedEvent) → confirm `[CaveRuntimeBridge] Player exiting cave to surface.` log appears
- [ ] 6.5 Confirm NO warning `CaveRunManager.Instance is null when publishing CaveExitedEvent` (if CaveRunManager is present)

### Section 7 — ValidateTownShopCatalogIntegrity (Unity Editor Menu)

- [ ] 7.1 In Unity Editor (Edit Mode, not Play Mode): go to `CindarsHope > Validate > Validate Town Shop Catalog Integrity`
- [ ] 7.2 Confirm Console shows: `ValidateTownShopCatalogIntegrity PASS: all shop catalogs in 'Assets/_Game/Data/Economy' are valid.`
- [ ] 7.3 Confirm no red errors in Console from the validator
- [ ] 7.4 Confirm the log does NOT say "5 town shop catalogs" (old message) — should say "all shop catalogs"

---

## Pass Criteria

All 7 sections must pass with no unexpected red errors.

## Failure Criteria

Any of the following is a failure requiring investigation:
- Red error about ItemId missing from ItemDatabase
- Duplicate Quest panel instances active simultaneously
- `CaveRunManager.Instance is null` warning when CaveRunManager is in scene and wired
- `ValidateTownShopCatalogIntegrity` fails with errors

---

*To record completion: update this file with date, Unity version, and executor name.*

Completed: [ ] / Date: ____________ / Unity: ____________ / Executor: ____________
