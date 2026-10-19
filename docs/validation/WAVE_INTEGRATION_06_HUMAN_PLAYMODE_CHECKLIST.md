# WAVE_INTEGRATION_06 Human Play Mode Checklist

Date created: 2026-06-08
Status: PENDING_HUMAN_EXECUTION
Prerequisites: Human must have run CindarsHope/Advanced/Legacy/Scenes/Create MVP FarmScene first

---

## Pre-Checklist Setup

- [ ] Run `CindarsHope/Advanced/Legacy/Scenes/Create MVP FarmScene` in Unity Editor
- [ ] Confirm Console: no errors or missing script warnings
- [ ] Run `CindarsHope/Repair and Validate/Validate Farm Resource Interactables` — confirm "0 issues"
- [ ] Open FarmScene in the Unity Editor hierarchy and confirm:
  - [ ] `FarmResourceInteractables` parent exists
  - [ ] `TreeResource_01` child exists with `FarmResourceInteractable` + `FarmResourceVisualController`
  - [ ] `RockResource_01` child exists with `FarmResourceInteractable` + `FarmResourceVisualController`
  - [ ] `ForageResource_01` child exists with `FarmResourceInteractable` + `FarmResourceVisualController`
  - [ ] Each interactable has `_inventoryManager` wired (not null)
  - [ ] Each interactable has `_visualController` wired (not null)
  - [ ] `FishingSpot` exists at (7.8, -2.8) with `_inventoryManager` wired

---

## Tree Resource Test

- [ ] Enter Play Mode
- [ ] Walk player to `TreeResource_01` (approximately (7.5, 3.5) — green rectangle in east zone)
- [ ] Verify interaction prompt appears: "Cortar árvore"
- [ ] Press E to interact
- [ ] Verify in Console: `[FarmResource] Tree: added 2x item_wood to inventory.`
- [ ] Verify TreeResource_01 turns gray/faded (depleted color)
- [ ] Verify pressing E again does not trigger interaction (depleted state)
- [ ] Check Debug HUD or inventory — item_wood count increased by 2

---

## Rock Resource Test

- [ ] Walk player to `RockResource_01` (approximately (-9.0, 4.5) — gray rectangle in northwest zone)
- [ ] Verify interaction prompt appears: "Minerar pedra"
- [ ] Press E to interact
- [ ] Verify in Console: `[FarmResource] Rock: added 2x item_stone to inventory.`
- [ ] Verify RockResource_01 turns gray/faded (depleted color)
- [ ] Check inventory — item_stone count increased by 2

---

## Forage Resource Test

- [ ] Walk player to `ForageResource_01` (approximately (-8.0, -2.5) — olive/green rectangle in west zone)
- [ ] Verify interaction prompt appears: "Coletar ervas"
- [ ] Press E to interact
- [ ] Verify in Console: `[FarmResource] Forage: added 1x item_herbs to inventory.`
- [ ] Verify ForageResource_01 turns gray/faded (depleted color)
- [ ] Check inventory — item_herbs count increased by 1

---

## Lake Fishing Test

- [ ] Walk player to `FishingSpot` (approximately (7.8, -2.8) — blue rectangle)
- [ ] Walk to the edge of the lake (not center)
- [ ] Verify interaction prompt appears: "Pescar"
- [ ] Note: Fishing requires a Fishing Rod tool in equipment
  - [ ] If fishing rod is not equipped: verify message "Requires Fishing Rod." appears
  - [ ] If fishing rod IS equipped: press E, verify "Fishing..." then "Press E now!" prompts, press E again in the time window
- [ ] Verify player is blocked from walking through the lake (blocking collider)

---

## Zone Spatial Verification

- [ ] Confirm Tree resources are in the east zone (x > 5)
- [ ] Confirm Rock resource is in the northwest zone (x < -7, y > 3)
- [ ] Confirm Forage resource is in the west zone (x < -5, y < 0)
- [ ] Confirm FishingSpot is in the southeast zone (x > 5, y < 0)
- [ ] Confirm crop field (FarmPlots) is in the center zone (x near 1, y near -1.5)

---

## Regression Checks (WAVE05 preserved)

- [ ] Walk to FarmPlot_00 (center area)
- [ ] Verify FarmPlot interaction is still functional (soil states, prompt "Plantar" or "Colher")
- [ ] Confirm WAVE04 zone markers are visible as colored overlays (if debug mode enabled)

---

## Pass Criteria

All boxes checked = WAVE_INTEGRATION_06 PLAYMODE_VALIDATED.
Any box failing = document in WAVE_INTEGRATION_06_RESOURCE_INTERACTABLE_REPORT.md under "Play Mode Issues Found".

---

Executor: _______________
Date executed: _______________
Result: PASS / FAIL / PARTIAL
