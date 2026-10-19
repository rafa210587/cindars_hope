# WAVE INTEGRATION 02 - Scene Architecture

## Status

VALIDATED

## Scene Inventory Imported From Spec 01

| Role | Existing scene/path | Classification | Decision | Notes |
|---|---|---|---|---|
| BootScene | `MISSING` | MISSING | MISSING_CREATE_IN_FOLLOWUP_OR_HUMAN_UNITY_ACTION | Target boot entry scene for WAVE_INTEGRATION flow. Do not create automatically in this spec. |
| PersistentManagers | `MISSING` | MISSING | MISSING_CREATE_IN_FOLLOWUP_OR_HUMAN_UNITY_ACTION | Target scene or prefab root for persistent `GameBootstrap`. Do not create automatically in this spec. |
| FarmScene | `Assets/_Game/Scenes/FarmScene.unity` | ACTIVE_TARGET | USE_AS_WAVE_INTEGRATION_03_PLAYER_SPAWN_TARGET | Current playable target for player spawn/camera baseline. |
| TownScene | `Assets/_Game/Scenes/TownScene.unity` | ACTIVE_TARGET | KEEP_AS_FOLLOWUP_TRANSITION_TARGET | Existing town scene; transition wiring remains follow-up. |
| CaveEntranceScene | `MISSING` | MISSING | MISSING_CREATE_IN_FOLLOWUP_OR_HUMAN_UNITY_ACTION | Optional for current slice; create later if cave entry is split from runtime. |
| CaveRuntimeScene | `Assets/_Game/Scenes/CaveScene.unity` | ACTIVE_TARGET | USE_ALIAS_CAVE_RUNTIME_TO_CAVESCENE | Existing cave target remains `CaveScene` for now. |
| HomeInteriorScene | `MISSING` | MISSING | MISSING_CREATE_IN_FOLLOWUP_OR_HUMAN_UNITY_ACTION | Optional for current slice. |
| TextMesh Pro example scenes | `Assets/TextMesh Pro/Examples & Extras/Scenes/*.unity` | LEGACY | EXCLUDE_FROM_GAMEPLAY_FLOW | Vendor/example scenes only. |

## Required Scene Roles

| Role | Required for WAVE 07? | Scene/path | Exists | Decision |
|---|---:|---|---:|---|
| BootScene | Sim | `MISSING` | No | Create later in Unity; target initial flow is BootScene -> PersistentManagers -> FarmScene. |
| PersistentManagers | Sim | `MISSING` | No | Reuse existing `GameBootstrap` as content of future persistent root; create scene/prefab later in Unity. |
| FarmScene | Sim | `Assets/_Game/Scenes/FarmScene.unity` | Yes | Use as direct temporary scene and WAVE_INTEGRATION_03 player spawn target. |
| TownScene | Sim, placeholder | `Assets/_Game/Scenes/TownScene.unity` | Yes | Keep as follow-up transition target. |
| CaveEntranceScene | Opcional agora | `MISSING` | No | Defer to follow-up/human Unity action. |
| CaveRuntimeScene | Opcional agora | `Assets/_Game/Scenes/CaveScene.unity` | Yes | Treat as current cave runtime scene; `SceneNames.CaveRuntime` aliases to `CaveScene`. |
| HomeInteriorScene | Opcional | `MISSING` | No | Defer to follow-up/human Unity action. |

## Load Flow

```text
Current temporary flow:
FarmScene opened directly
-> existing serialized GameBootstrap/scene wiring must provide runtime managers
-> WAVE_INTEGRATION_03 spawns player/camera in FarmScene

Target flow:
BootScene
-> initializes or loads PersistentManagers
-> PersistentManagers hosts or references the existing GameBootstrap root
-> loads FarmScene
-> future: FarmScene <-> TownScene through ScenePortal/SceneSpawnInstaller
-> future: FarmScene/TownScene <-> CaveEntranceScene
-> future: CaveEntranceScene <-> CaveScene
```

## Next Spec Scene Target

WAVE_INTEGRATION_03 should use:

```text
Primary scene: Assets/_Game/Scenes/FarmScene.unity
Scene name contract: SceneNames.Farm
Spawn policy: use existing SceneSpawnInstaller/SceneSpawnPoint pattern where available; otherwise add a minimal spawn point in the next spec with explicit scene-change authorization.
```

## Required Human Unity Actions

| Action | Reason | Target spec |
|---|---|---|
| Create `BootScene` and add it to Build Settings. | Required for final boot flow; creating scenes is deferred from this documentation/code-only spec. | WAVE_INTEGRATION_03+ |
| Create `PersistentManagers` scene or prefab root, using existing `GameBootstrap`. | Prevents manager duplication and centralizes persistent services. | WAVE_INTEGRATION_03+ |
| Add target gameplay scenes to Build Settings. | `ScenePortal` runtime name loading depends on Build Settings outside editor path loading. | WAVE_INTEGRATION_03+ |
| Confirm `FarmScene` opens without red Console errors in Unity. | Human Unity validation remains pending. | Before/while executing WAVE_INTEGRATION_03 |
