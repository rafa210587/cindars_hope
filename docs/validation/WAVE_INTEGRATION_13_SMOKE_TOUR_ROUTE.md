# WAVE_INTEGRATION_13 — Smoke Tour Route

## Route: Farm → Town → Farm → Cave → Farm

### Purpose

Validate the full playable path through all three scenes in one continuous play session.
This smoke tour is the minimum bar for PLAYMODE_VALIDATED status.

### Prerequisites

- Human has wired all gates and anchors per WAVE_INTEGRATION_13_HUMAN_UNITY_SCENE_WIRING_INSTRUCTIONS.md
- Scenes added to Build Settings
- Player, PlayerSpawnResolver, and camera wired in FarmScene, TownScene, CaveScene

## Baseline State Capture (Before Starting)

Before pressing Play, note:

| State | Expected Value | Actual Value (fill in) |
|-------|---------------|------------------------|
| Starting scene | FarmScene | |
| Player position | Default spawn | |
| Gold | Initial amount | |
| Inventory | Empty or provisioned | |
| Skill nodes purchased | 0 or whatever | |
| Current time | 06:00 Day 1 | |

## Route Steps

### Step 1: FarmScene → TownScene

1. Open FarmScene in Unity Editor
2. Press Play
3. Move player toward gate_farm_town_exit
4. Approach gate — confirm interaction prompt appears
5. Press F (or configured interact key)
6. Confirm: scene transitions to TownScene
7. Confirm: player spawns at spawn_town_from_farm
8. Confirm: player is visible and can move

### Step 2: TownScene → FarmScene

1. Move player toward gate_town_farm_exit
2. Press F to interact
3. Confirm: scene transitions to FarmScene
4. Confirm: player spawns at spawn_farm_from_town
5. Confirm: player visible and moveable

### Step 3: FarmScene → CaveScene

1. Move player toward gate_farm_cave_entrance
2. Press F to interact
3. Confirm: scene transitions to CaveScene
4. Confirm: player spawns at spawn_cave_from_farm
5. Confirm: cave generates / renders

### Step 4: CaveScene → FarmScene

1. Move player toward gate_cave_farm_exit (or CaveExitPortal)
2. Press F (or use CaveExitPortal trigger)
3. Confirm: scene transitions to FarmScene
4. Confirm: player spawns at spawn_farm_from_cave

## Post-Tour State Check

After completing the route, check:

| State | Before Tour | After Tour | Preserved? |
|-------|------------|-----------|-----------|
| Gold | | | |
| Inventory | | | |
| Skills | | | |
| Active slots | | | |
| HUD visible | YES | | |
| No errors in Console | YES | | |

## Known Expected Failures (Pre-Wiring)

- Farm→Town transition fails: gate not placed in FarmScene yet
- Town→Farm transition fails: gate not placed in TownScene yet
- Farm→Cave transition fails: gate not placed in FarmScene yet
- Cave→Farm: CaveExitPortal may work already (from prior wave)

All failures are expected until human Unity wiring is complete.
