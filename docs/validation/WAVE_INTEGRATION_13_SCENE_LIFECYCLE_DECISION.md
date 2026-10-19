# WAVE_INTEGRATION_13 — Scene Lifecycle Decision

## Load Mode Decision

`LoadSceneMode.Single` — full scene replacement. Additive loading deferred.

### Rationale

- Single mode is simpler and compatible with current GameBootstrap per-scene wiring
- Additive loading requires careful manager deduplication (PersistentManagers scene prerequisite)
- Deferred until PersistentManagers scene exists

## Player Strategy

| State | Decision |
|-------|----------|
| Player destroyed on unload | YES — player is scene-local object |
| Player re-spawned in new scene | YES — via PlayerSpawnResolver |
| Player position carried over | NO — resolved from SceneSpawnAnchor |
| Player health/stamina on transition | PRESERVED via SaveManager if SaveManager persists; else DEBT |
| Player inventory on transition | PRESERVED if InventoryManager uses persistent data (SO-backed) |

## Camera Strategy

| State | Decision |
|-------|----------|
| Camera destroyed on unload | YES — camera is scene-local |
| Camera re-initialized in new scene | YES — each scene has its own CameraFollow2D |
| Camera bounds on transition | Resolved by per-scene CameraScaleController |
| Camera target after spawn | Must be re-wired to new player in scene |

## HUD Strategy

| Component | Strategy |
|-----------|----------|
| DebugHud | DontDestroyOnLoad via RuntimeInitializeOnLoadMethod — survives transitions |
| InventoryPanelController | DontDestroyOnLoad via RuntimeInitializeOnLoadMethod — survives |
| CharacterEquipmentPanelController | DontDestroyOnLoad — survives |
| SkillTreeGameplayPanelController | DontDestroyOnLoad — survives |
| GameplayInputRouter | DontDestroyOnLoad — survives |

## Manager Strategy

| Approach | Decision |
|----------|----------|
| Persistent manager scene | NOT_CREATED_DEFERRED |
| GameBootstrap | Per-scene (FarmScene has it wired; TownScene/CaveScene may differ) |
| Manager duplication guard | GameBootstrap uses _instance static + [DisallowMultipleComponent] |
| EconomyManager on Town transition | DEBT — may not exist in TownScene; purchases may fail |

## Bootstrap Policy

`FARMSCENE_TEMPORARY_ENTRYPOINT`:
- Game starts in FarmScene (opened directly in Editor Play Mode)
- GameBootstrap initializes all managers from FarmScene
- On Farm→Town: FarmScene unloads, GameBootstrap destroyed, TownScene must init its own Bootstrap
- On Farm→Cave: same — CaveScene has its own controllers
- This is a known architectural debt; target is PersistentManagers scene

## Duplicate Prevention

1. SceneTransitionRouter._transitionInProgress guard: blocks re-entrant transitions
2. GameBootstrap._instance: singleton guard per-scene
3. ModalManager already handles duplicate guards via RuntimeInitializeOnLoad pattern

## Input Lock

DEFERRED — no input lock during scene loading. User may click during loading.

Risk: low in practice (scene loads quickly in Editor Play Mode).

## Failure Recovery

See WAVE_INTEGRATION_13_TRANSITION_ERROR_RECOVERY_MATRIX.md.
