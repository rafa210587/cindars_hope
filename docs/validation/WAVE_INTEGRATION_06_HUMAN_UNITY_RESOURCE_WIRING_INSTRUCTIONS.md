# WAVE_INTEGRATION_06 Human Unity Resource Wiring Instructions

Date: 2026-06-08
Purpose: Instructions for applying WAVE_INTEGRATION_06 resource interactables in Unity Editor

---

## Overview

WAVE_INTEGRATION_06 adds three resource interactable smoke nodes to FarmScene:
- TreeResource_01 (east zone, item_wood x2)
- RockResource_01 (northwest zone, item_stone x2)
- ForageResource_01 (west zone, item_herbs x1)

The LakeFishing interaction is handled by the existing FishingSpot at (7.8, -2.8) — no new component required.

All wiring is done automatically by the CreateMvpFarmScene generator via Editor APIs. No manual Inspector editing required beyond running the generator.

---

## Step 1: Open Unity Editor

Open the project in Unity Editor (Unity 6000.4.7f1 or compatible).

---

## Step 2: Run the Scene Generator

Menu path: `CindarsHope > Advanced > Legacy > Scenes > Create MVP FarmScene`

This will:
1. Create a new FarmScene with all WAVE04+05+06 objects
2. Create a `FarmResourceInteractables` parent GameObject
3. Create `TreeResource_01` at (7.5, 3.5) with FarmResourceInteractable and FarmResourceVisualController wired
4. Create `RockResource_01` at (-9.0, 4.5) with FarmResourceInteractable and FarmResourceVisualController wired
5. Create `ForageResource_01` at (-8.0, -2.5) with FarmResourceInteractable and FarmResourceVisualController wired
6. Wire all `_inventoryManager` references to the InventoryManager on _Bootstrap
7. Wire all `_visualController` references to the FarmResourceVisualController on each object
8. Set reward item IDs and amounts (item_wood x2, item_stone x2, item_herbs x1)
9. Set interaction prompts (Cortar árvore, Minerar pedra, Coletar ervas)

Wait for Console: `MVP FarmScene created at Assets/_Game/Scenes/FarmScene.unity`

---

## Step 3: Verify in Hierarchy

In the Hierarchy panel, confirm:
```
FarmScene
├── _Bootstrap
├── Player
├── Trees (19 TreeNode children)
├── FarmPlots (9 FarmPlot children)
├── FarmResourceInteractables    ← NEW (WAVE_INTEGRATION_06)
│   ├── TreeResource_01
│   ├── RockResource_01
│   └── ForageResource_01
├── FishingSpot                  ← existing (LakeFishing)
├── FarmSceneFoundationZones
└── ...
```

---

## Step 4: Verify Wiring in Inspector

Click each resource node and confirm in the Inspector:

### TreeResource_01
- Component: FarmResourceInteractable
  - _resourceType: Tree
  - _interactionPrompt: "Cortar árvore"
  - _reward._itemId: "item_wood"
  - _reward._amount: 2
  - _visualController: (assigned — FarmResourceVisualController on same GO)
  - _inventoryManager: (assigned — InventoryManager on _Bootstrap)
- Component: FarmResourceVisualController
  - _spriteRenderer: (assigned — SpriteRenderer on same GO)

### RockResource_01
- _resourceType: Rock
- _interactionPrompt: "Minerar pedra"
- _reward._itemId: "item_stone"
- _reward._amount: 2
- _visualController: (assigned)
- _inventoryManager: (assigned)

### ForageResource_01
- _resourceType: Forage
- _interactionPrompt: "Coletar ervas"
- _reward._itemId: "item_herbs"
- _reward._amount: 1
- _visualController: (assigned)
- _inventoryManager: (assigned)

---

## Step 5: Run Validation Menu

Menu path: `CindarsHope > Repair and Validate > Validate Farm Resource Interactables`

Expected output in Console:
```
[ValidateFarmResource] All 3 FarmResourceInteractable(s) validated. No issues.
```

If any warnings appear, fix the missing references in the Inspector before proceeding to Play Mode.

---

## Step 6: LakeFishing Verification

Click `FishingSpot` in the Hierarchy and confirm in Inspector:
- Component: FishingSpot
  - _inventoryManager: (assigned — InventoryManager on _Bootstrap)
  - _staminaManager: (assigned — StaminaManager on _Bootstrap)
  - _fishItemId: "item_fish_common"
  - InteractionPrompt (read-only): "Pescar"

Note: Fishing requires the player to have a Fishing Rod equipped. During smoke testing, add one via inventory or skip the actual catch and just verify the prompt appears.

---

## Step 7: Execute Play Mode Checklist

Follow the checklist in:
`docs/validation/WAVE_INTEGRATION_06_HUMAN_PLAYMODE_CHECKLIST.md`

---

## Manual Fallback (If Generator Fails)

If the generator throws errors or fails to create FarmResourceInteractables:

1. Manually create a GameObject "FarmResourceInteractables" at position (0,0,0)
2. Create a child "TreeResource_01" at (7.5, 3.5) with:
   - SpriteRenderer (green color)
   - BoxCollider2D (isTrigger=true)
   - FarmResourceVisualController (wire _spriteRenderer)
   - FarmResourceInteractable:
     - _resourceType: Tree
     - _interactionPrompt: "Cortar árvore"
     - _reward._itemId: "item_wood", _amount: 2
     - _visualController: FarmResourceVisualController on same GO
     - _inventoryManager: InventoryManager from _Bootstrap
3. Repeat for RockResource_01 (gray color, item_stone x2) at (-9.0, 4.5)
4. Repeat for ForageResource_01 (olive color, item_herbs x1) at (-8.0, -2.5)
5. Run validator to confirm 0 issues

---

## After Completing Play Mode Checklist

Update `docs/validation/WAVE_INTEGRATION_06_RESOURCE_INTERACTABLE_REPORT.md`:
- Add "Play Mode Issues Found" section (or confirm none)
- Change status from BUILD_VALIDATED_CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED to PLAYMODE_VALIDATED (if all pass)

Then update `docs/project/CURRENT_STATE.md`:
- Change WAVE 07 status to reflect Play Mode validation complete
- Set "Can start WAVE_INTEGRATION_07: YES" (if checklist passes)
