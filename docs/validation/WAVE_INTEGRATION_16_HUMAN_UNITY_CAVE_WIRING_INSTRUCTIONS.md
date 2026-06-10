# WAVE_INTEGRATION_16 — Human Unity Cave Wiring Instructions

## Status

HUMAN_ACTION_REQUIRED — These steps must be performed in Unity Editor.

## Overview

WAVE16 code is ready. The following Unity Editor actions are needed to wire the cave entrance/exit and spawn anchors into the scenes.

---

## Step 1 — FarmScene: Wire CaveEntranceInteractable

1. Open **FarmScene** in Unity Editor
2. In Hierarchy, find **Zone_CaveEntrance**
3. With Zone_CaveEntrance selected:
   - Click **Add Component**
   - Add `CaveEntranceInteractable`
4. Set fields in Inspector:
   - **Transition Gate Id**: `gate_farm_cave_entrance`
   - **From Scene Id**: `FarmScene`
   - **Target Cave Scene**: `CaveScene`
   - **Target Spawn Id**: `spawn_cave_from_farm`
   - **Interaction Prompt**: `Entrar na caverna`
5. Save FarmScene (Ctrl+S)

> Note: Zone_CaveEntrance also needs an InteractionTrigger (collider trigger) if not already present. Check if there is a collider on the object or its children. If not, add a BoxCollider2D (IsTrigger=true) sized appropriately for the entrance area.

---

## Step 2 — FarmScene: Add spawn_farm_from_cave anchor

1. In FarmScene Hierarchy, find the **SpawnPoints** GameObject (or create one if absent)
2. Create a child empty GameObject named `spawn_farm_from_cave`
3. Position it near Zone_CaveEntrance (just outside the entrance)
   - Suggested position: same X/Y as Zone_CaveEntrance but slightly offset to the right/down so player doesn't immediately re-trigger the entrance
4. Add Component: `SceneSpawnAnchor` (from WAVE13)
   - Set **Spawn Anchor Id**: `spawn_farm_from_cave`
5. Save FarmScene

---

## Step 3 — CaveScene: Wire CaveEntryController

1. Open **CaveScene** in Unity Editor
2. In Hierarchy, find the **CaveRuntime** GameObject (has CaveRunManager, CaveLevelRuntimeController)
3. With CaveRuntime selected:
   - Click **Add Component**
   - Add `CaveEntryController`
4. Set fields in Inspector:
   - **Cave Run Manager**: drag CaveRunManager from same object (or leave for auto-resolve via GetComponent)
   - **Level Controller**: drag CaveLevelRuntimeController from same object
5. Save CaveScene

---

## Step 4 — CaveScene: Wire CaveExitPortal (level 1 BackExit → Farm)

1. In CaveScene Hierarchy, find the **SpawnPoints** or **Portals** group (or create a new empty GO named `CavePortals`)
2. Create a child empty GameObject named `CaveExit_ToSurface_01`
3. Position it near the cave entrance spawn area (where player arrives from Farm)
   - This is where the player stands when they want to exit to Farm
   - Suggested: near the spawn_cave_from_farm position, slightly offset
4. Add Component: `CaveExitPortal`
5. Set fields in Inspector:
   - **Target Scene Name**: `FarmScene`
   - **Target Scene Path**: `Assets/_Game/Scenes/FarmScene.unity`
   - **Target Spawn Id**: `spawn_farm_from_cave`
   - **Interaction Prompt**: `Sair da caverna`
6. Add Component: `BoxCollider2D` (IsTrigger=true) for interaction trigger
7. Save CaveScene

> Note: The CaveExitPortal.InitializeBackExit() method is called by code to configure it as BackExit. For the surface-exit portal (level 1 → Farm), you can either:
> - Leave the serialized fields set directly (simpler), OR
> - Call InitializeBackExit from CaveEntryController after wiring it

---

## Step 5 — CaveScene: Verify PlayerSpawnResolver

1. In CaveScene Hierarchy, find (or add) a **PlayerSpawnResolver** MonoBehaviour (WAVE13)
2. Verify it is present on an active GameObject in the scene
3. If not present: add a new empty GO named `PlayerSpawnResolver_Cave` and add the component
4. The resolver reads `SceneTransitionState.PendingSpawn` and moves the Player to the matching SceneSpawnPoint/SceneSpawnAnchor
5. Verify the **spawn_cave_from_farm** anchor exists in scene's SpawnPoints

---

## Step 6 — Build Settings (if not already set)

1. File → Build Settings
2. Verify **CaveScene**, **FarmScene**, **TownScene** are all in the Scenes in Build list
3. If missing, drag from Project window into the Build Settings list

---

## Step 7 — Smoke test

After completing the above steps:

1. Open FarmScene
2. Press **Play**
3. Walk to Zone_CaveEntrance
4. Press E (interact)
5. Verify CaveScene loads and player spawns near cave entrance
6. Walk to CaveExit_ToSurface_01
7. Press E (interact)
8. Verify FarmScene loads and player spawns near spawn_farm_from_cave

If all steps pass, proceed to `WAVE_INTEGRATION_16_HUMAN_PLAYMODE_CHECKLIST.md` for full validation.

---

## Optional: TownScene cave entrance (WAVE17+)

If you want a cave entrance in TownScene:
1. Create a new empty GO in TownScene named `CaveEntrance_ToRuntime_01`
2. Add Component: `CaveEntranceInteractable`
   - **From Scene Id**: `TownScene`
   - **Target Spawn Id**: `spawn_cave_from_town`
3. Add `spawn_cave_from_town` anchor in CaveScene SpawnPoints
4. This is deferred to WAVE17 or later.

---

*Created: 2026-06-10 (WAVE_INTEGRATION_16)*
