# WAVE_INTEGRATION_13 — Human Play Mode Checklist

## Prerequisites

- [ ] All gates placed per WAVE_INTEGRATION_13_HUMAN_UNITY_SCENE_WIRING_INSTRUCTIONS.md
- [ ] All spawn anchors placed in FarmScene, TownScene, CaveScene
- [ ] PlayerSpawnResolver wired in each scene
- [ ] Scenes added to Build Settings (optional for Editor Play Mode)

## Test Session Setup

1. Open Unity Editor
2. Open FarmScene (Assets/_Game/Scenes/FarmScene.unity)
3. Press Play

## Phase 1: FarmScene Baseline

- [ ] Scene opens without errors in Console
- [ ] Player is visible and at expected spawn position
- [ ] Player can move (WASD or arrow keys)
- [ ] Camera follows player
- [ ] HUD is visible (DebugHud or Canvas HUD)

## Phase 2: Farm → Town Transition

- [ ] Move player toward gate_farm_town_exit GameObject
- [ ] Interaction prompt appears ("Ir para a Cidade" or configured text)
- [ ] Press F (or configured interact key)
- [ ] Scene transitions to TownScene
- [ ] No "Scene not found" or "LoadScene" errors in Console
- [ ] Player appears in TownScene at expected position (spawn_town_from_farm)
- [ ] Player can move in TownScene
- [ ] Camera follows player

## Phase 3: Town Spawn Validation

- [ ] Player is at spawn_town_from_farm location (near western edge of TownScene)
- [ ] Player is not at origin (0, 0) — would indicate missing anchor
- [ ] Console shows "[PlayerSpawnResolver] Spawned player at 'spawn_town_from_farm'"

## Phase 4: Town → Farm Transition

- [ ] Move player toward gate_town_farm_exit
- [ ] Interaction prompt appears
- [ ] Press F
- [ ] Scene transitions to FarmScene
- [ ] Player appears at spawn_farm_from_town
- [ ] Player is NOT at origin — spawn anchor was found

## Phase 5: Farm Cave Return Spawn

- [ ] Confirm player is at spawn_farm_from_town (not spawn_farm_default)
- [ ] Player can move normally

## Phase 6: Farm → Cave Transition

- [ ] Move player toward gate_farm_cave_entrance
- [ ] Interaction prompt appears
- [ ] Press F
- [ ] Scene transitions to CaveScene
- [ ] Cave procedural level generates
- [ ] Player visible in CaveScene at spawn_cave_from_farm

## Phase 7: Cave Spawn Validation

- [ ] Player is at spawn_cave_from_farm (near cave entrance)
- [ ] Player can move
- [ ] No enemy immediately in contact

## Phase 8: Cave → Farm Transition

- [ ] Move player toward gate_cave_farm_exit (or CaveExitPortal)
- [ ] Interact (F or collision trigger)
- [ ] Scene transitions to FarmScene
- [ ] Player appears at spawn_farm_from_cave

## Phase 9: State Preservation Check

After completing full route:

- [ ] Gold amount matches pre-tour amount (or note delta)
- [ ] Inventory contents match pre-tour inventory (or note delta)
- [ ] Skill nodes purchased match pre-tour skills
- [ ] Active slots match pre-tour configuration
- [ ] HUD is visible and responsive
- [ ] No lingering red errors in Console

## Phase 10: Camera Validation

- [ ] Camera follows player in FarmScene after return
- [ ] Camera follows player in TownScene
- [ ] Camera follows player in CaveScene
- [ ] Camera does not show black bars or error textures

## Final Sign-Off

| Item | Pass/Fail | Notes |
|------|-----------|-------|
| Farm→Town transition | | |
| Town spawn correct | | |
| Town→Farm transition | | |
| Farm from-Town spawn correct | | |
| Farm→Cave transition | | |
| Cave spawn correct | | |
| Cave→Farm transition | | |
| Farm from-Cave spawn correct | | |
| Inventory preserved | | |
| Gold preserved | | |
| HUD survived all transitions | | |
| No critical console errors | | |

Signed off by: ___________________ Date: ___________________
