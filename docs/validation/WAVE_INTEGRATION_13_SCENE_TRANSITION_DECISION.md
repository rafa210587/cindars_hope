# WAVE_INTEGRATION_13 — Scene Transition Decision

## Status

CODE_READY_HUMAN_UNITY_ACTION_REQUIRED

## Scene Strategy

### Load Mode

`Single` load mode — each scene transition replaces the current scene. No additive loading.

FarmScene serves as temporary entrypoint (FARMSCENE_TEMPORARY_ENTRYPOINT) because BootScene and PersistentManagers scene do not exist yet.

GameBootstrap is serialized in FarmScene and provides all manager refs to gameplay systems.

### Bootstrap Decision

`FARMSCENE_TEMPORARY_ENTRYPOINT` — GameBootstrap is wired to FarmScene. When transitioning to TownScene or CaveScene:
- GameBootstrap from FarmScene is DESTROYED when FarmScene unloads
- TownScene must have its own GameBootstrap (or a minimal subset for that scene's managers)
- CaveScene already has its own runtime controller (CaveLevelRuntimeController, CaveRunManager, etc.)

Debt: Persistent manager scene (PersistentManagers) is required for robust state preservation. This is deferred to WAVE_INTEGRATION_14 or human Unity action.

### Player Strategy

Player is a scene-local object. On transition, it is destroyed and re-spawned in the destination scene by PlayerSpawnResolver reading SceneTransitionState.PendingSpawnId.

PlayerSpawnResolver resolution order:
1. SceneSpawnAnchor with matching spawnAnchorId
2. Legacy SceneSpawnPoint with matching spawnId
3. Default spawn anchor ID on PlayerSpawnResolver component
4. Warning + no-move fallback

### Camera Strategy

Camera is scene-local. CameraFollow2D is already wired in FarmScene to player transform. Each destination scene needs its own CameraFollow2D and player. No cross-scene camera persistence required.

### HUD Strategy

DebugHud is RuntimeInitializeOnLoad — it persists automatically via DontDestroyOnLoad. ModalManager-based panels (Inventory, Equipment, SkillTree) also use RuntimeInitializeOnLoad.

HUD will survive scene transitions without manual wiring. Validated in prior waves.

### Manager Strategy

No duplicate manager creation. Do NOT add new DontDestroyOnLoad managers in WAVE_INTEGRATION_13.

If a manager is missing in TownScene or CaveScene, document as debt, not as a parallel system creation.

### Duplicate Prevention

SceneTransitionRouter has a `_transitionInProgress` guard that blocks re-entrant calls within the same play session.

GameBootstrap uses `[DisallowMultipleComponent]` to prevent scene-level duplication.

### Input Lock

No explicit input lock on transition. Transition happens synchronously on next frame. Future fade system (deferred) should lock input during fade.

### Failure Recovery

See WAVE_INTEGRATION_13_TRANSITION_ERROR_RECOVERY_MATRIX.md.

## Stable IDs

All IDs are compile-time constants in `SceneId.cs`. They must never change once wired in scenes.

| Constant | Value |
|----------|-------|
| SceneId.Farm | "FarmScene" |
| SceneId.Town | "TownScene" |
| SceneId.Cave | "CaveScene" |
| SceneId.GateFarmTownExit | "gate_farm_town_exit" |
| SceneId.GateFarmCaveEntrance | "gate_farm_cave_entrance" |
| SceneId.GateTownFarmExit | "gate_town_farm_exit" |
| SceneId.GateCaveFarmExit | "gate_cave_farm_exit" |
| SceneId.SpawnFarmFromTown | "spawn_farm_from_town" |
| SceneId.SpawnFarmFromCave | "spawn_farm_from_cave" |
| SceneId.SpawnTownFromFarm | "spawn_town_from_farm" |
| SceneId.SpawnCaveFromFarm | "spawn_cave_from_farm" |
| SceneId.SpawnFarmDefault | "spawn_farm_default" |

## Design / Direction Compliance Matrix

| Requirement | Status | Notes |
|-------------|--------|-------|
| Farm→Town transition | CODE_READY | SceneTransitionGate + SceneTransitionRouter wired; scene objects need human placement |
| Town→Farm transition | CODE_READY | Same |
| Farm→Cave transition | CODE_READY | Same |
| Cave→Farm transition | CODE_READY | CaveExitPortal already existed; SceneTransitionGate is additional path |
| Stable IDs | IMPLEMENTED | SceneId.cs constants |
| No GameObject.Find runtime | COMPLIANT | PlayerSpawnResolver uses FindObjectsByType (allowed fallback) |
| Event bus for transition | COMPLIANT | SceneTransitionStartedEvent + SceneTransitionCompletedEvent published |
| Save DTO simple types | COMPLIANT | SceneTransitionRequest/Result have only strings and bool |
| No YAML editing | COMPLIANT | All scene wiring deferred to human Unity Editor action |

## Debt Policy

- Fade system: FADE_LOADING_DEFERRED — no fade exists; scene pops instantly
- PersistentManagers scene: DEFERRED — manager state may reset on cross-scene transition
- TownScene GameBootstrap: DEBT — TownScene may lack required managers after transition from Farm
- Input lock on transition: DEFERRED — input not blocked during load
- Build Settings: HUMAN_ACTION_REQUIRED — scenes must be manually added to Build Settings
