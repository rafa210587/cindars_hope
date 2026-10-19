# WAVE_INTEGRATION_13 — Human Unity Scene Wiring Instructions

## Overview

These are step-by-step instructions for placing transition gates and spawn anchors in FarmScene, TownScene, and CaveScene. All YAML editing is forbidden; these instructions use the Unity Editor UI.

## Required Before Starting

- Unity Editor open with the cindars_hope project
- Both C# assemblies compile with 0 errors (verify in Console)
- Assets/_Game/Scripts/World/Scenes/*.cs files visible in Project window

---

## FarmScene Wiring

### Open FarmScene

1. In Project window: Assets/_Game/Scenes/
2. Double-click FarmScene.unity
3. Confirm scene loads without red errors

### Add PlayerSpawnResolver

1. Hierarchy > Create Empty > name it "SceneSetup_Farm"
2. Add Component > search "PlayerSpawnResolver"
3. Assign Player Transform: drag the Player GameObject's Transform field
4. Set Default Spawn Anchor Id: "spawn_farm_default"

### Add gate_farm_town_exit

1. Hierarchy > Create Empty > name it "gate_farm_town_exit"
2. Position at eastern edge of FarmScene (approx X=18, Y=0)
3. Add Component > SceneTransitionGate
4. Set fields:
   - TransitionId: gate_farm_town_exit
   - FromSceneId: FarmScene
   - ToSceneId: TownScene
   - TargetSpawnAnchorId: spawn_town_from_farm
   - InteractionPrompt: Ir para a Cidade
5. Add Component > BoxCollider2D (set as trigger, size 1x2)

### Add gate_farm_cave_entrance

1. Hierarchy > Create Empty > name it "gate_farm_cave_entrance"
2. Position at southern edge of FarmScene (approx X=0, Y=-13)
3. Add Component > SceneTransitionGate
4. Set fields:
   - TransitionId: gate_farm_cave_entrance
   - FromSceneId: FarmScene
   - ToSceneId: CaveScene
   - TargetSpawnAnchorId: spawn_cave_from_farm
   - InteractionPrompt: Entrar na Caverna
5. Add Component > BoxCollider2D (trigger, size 1x2)

### Add spawn_farm_from_town

1. Create Empty > name "spawn_farm_from_town"
2. Position at eastern edge, slightly inside the farm (approx X=16, Y=0)
3. Add Component > SceneSpawnAnchor
4. Set Spawn Anchor Id: spawn_farm_from_town

### Add spawn_farm_from_cave

1. Create Empty > name "spawn_farm_from_cave"
2. Position at southern edge, slightly inside (approx X=0, Y=-11)
3. Add Component > SceneSpawnAnchor
4. Set Spawn Anchor Id: spawn_farm_from_cave

### Add spawn_farm_default (if not already exists as SceneSpawnPoint)

1. Create Empty > name "spawn_farm_default"
2. Position at player start area (approx X=-2, Y=0)
3. Add Component > SceneSpawnAnchor
4. Set Spawn Anchor Id: spawn_farm_default

### Save FarmScene

File > Save (Ctrl+S)

---

## TownScene Wiring

### Open TownScene

1. File > Open Scene > Assets/_Game/Scenes/TownScene.unity
2. Confirm scene loads (may be minimal/placeholder)

### Add PlayerSpawnResolver

1. Create Empty > "SceneSetup_Town"
2. Add Component > PlayerSpawnResolver
3. Assign Player Transform (must have player in scene — add minimal player if missing)
4. Set Default Spawn Anchor Id: spawn_town_from_farm

### Add spawn_town_from_farm

1. Create Empty > "spawn_town_from_farm"
2. Position near western edge of TownScene (approx X=-15, Y=0)
3. Add Component > SceneSpawnAnchor
4. Set Spawn Anchor Id: spawn_town_from_farm

### Add gate_town_farm_exit

1. Create Empty > "gate_town_farm_exit"
2. Position at western edge (approx X=-18, Y=0)
3. Add Component > SceneTransitionGate
4. Set fields:
   - TransitionId: gate_town_farm_exit
   - FromSceneId: TownScene
   - ToSceneId: FarmScene
   - TargetSpawnAnchorId: spawn_farm_from_town
   - InteractionPrompt: Ir para a Fazenda
5. Add Component > BoxCollider2D (trigger, size 1x2)

### Save TownScene

File > Save (Ctrl+S)

---

## CaveScene Wiring

### Open CaveScene

1. File > Open Scene > Assets/_Game/Scenes/CaveScene.unity
2. Confirm scene loads

### Add spawn_cave_from_farm

1. Create Empty > "spawn_cave_from_farm"
2. Position near cave entrance area (exact position depends on cave generation)
3. Add Component > SceneSpawnAnchor
4. Set Spawn Anchor Id: spawn_cave_from_farm

Note: CaveScene is procedurally generated. The spawn anchor should be at a fixed position
near where the cave entrance would logically be. CaveRuntimeMaterializer places the entrance.

### Add gate_cave_farm_exit (Optional — CaveExitPortal already exists)

If you want to replace CaveExitPortal with the new gate:
1. Find existing CaveExitPortal in Hierarchy
2. Either: Remove CaveExitPortal component, add SceneTransitionGate
   - Or: Keep CaveExitPortal and add gate_cave_farm_exit as a new object

If adding new gate:
1. Create Empty > "gate_cave_farm_exit"
2. Position at cave exit area
3. Add Component > SceneTransitionGate
4. Set fields:
   - TransitionId: gate_cave_farm_exit
   - FromSceneId: CaveScene
   - ToSceneId: FarmScene
   - TargetSpawnAnchorId: spawn_farm_from_cave
   - InteractionPrompt: Sair da Caverna

### Add PlayerSpawnResolver (or reuse CaveSpawnInstaller if exists)

1. Create Empty > "SceneSetup_Cave"
2. Add Component > PlayerSpawnResolver
3. Assign Player Transform
4. Set Default Spawn Anchor Id: spawn_cave_from_farm

### Save CaveScene

File > Save (Ctrl+S)

---

## Verify Wiring

After wiring all three scenes:
1. Run CindarsHope/Validation/Validate Scene Transitions in Unity menu
2. Check Console for PASS or FAIL
3. Fix any MISSING_GATE or MISSING_ANCHOR warnings
4. Run WAVE_INTEGRATION_13_HUMAN_PLAYMODE_CHECKLIST.md

## Build Settings

See WAVE_INTEGRATION_13_HUMAN_UNITY_BUILD_SETTINGS_INSTRUCTIONS.md
