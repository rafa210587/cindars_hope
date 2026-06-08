# WAVE INTEGRATION 02 - Scene Architecture Decision

## Decision

DOCUMENTATION_ONLY_HUMAN_UNITY_ACTION_REQUIRED

## Reason

The project already has a persistent manager root implementation in `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs`: it is a `MonoBehaviour`, exposes manager references, initializes runtime services, uses a static `Instance`, destroys duplicate bootstraps, and calls `DontDestroyOnLoad(gameObject)`.

Creating or modifying `BootScene`, `PersistentManagers`, prefabs, or Build Settings would require scene/prefab/project changes that are outside the default scope of this spec. The correct action is to document the architecture, add only a scene-name contract, and defer Unity scene/prefab work to an explicitly authorized follow-up.

## Alternatives considered

| Alternative | Why not chosen |
|---|---|
| REUSE_EXISTING_MANAGER_ROOT | Chosen as the runtime strategy, but not enough as the execution status because the physical Boot/PersistentManagers scene or prefab still requires Unity action. |
| CREATE_PERSISTENT_MANAGERS_SCENE | Not chosen because creating `.unity` files is deferred without explicit scene authorization. |
| CREATE_PERSISTENT_MANAGERS_PREFAB | Not chosen because creating/modifying prefabs is forbidden without explicit authorization. |
| Create a new SceneTransitionService | Not chosen because `ScenePortal`, `SceneTransitionState`, and `SceneSpawnInstaller` already provide a minimal transition/spawn pattern. |

## Manager lifetime rule

Exactly one `GameBootstrap` should exist at runtime. `GameBootstrap` is the owner/root for persistent gameplay managers and must survive scene loads through `DontDestroyOnLoad`. Duplicate `GameBootstrap` instances should be destroyed by the existing `_instance` guard.

## Scene loading rule

Short term:

```text
Open FarmScene directly for WAVE_INTEGRATION_03.
```

Target:

```text
BootScene -> PersistentManagers/GameBootstrap -> FarmScene
```

Scene transitions should reuse `ScenePortal`, `SceneTransitionState`, and `SceneSpawnInstaller`. Runtime scene loads by name require Build Settings validation in Unity.

## Duplicate prevention rule

Do not place independent copies of core managers in each gameplay scene. Gameplay scenes should reference the persistent `GameBootstrap` root or scene-local installers only. If a scene temporarily contains serialized manager objects, WAVE_INTEGRATION_03+ must verify there is no duplicate persistent root in Play Mode.

## Required human Unity action

| Action | Required before | Reason |
|---|---|---|
| Create/verify BootScene. | Final boot flow. | Missing scene; cannot be created in this code/docs-only spec. |
| Create/verify PersistentManagers scene or prefab root using existing `GameBootstrap`. | Final boot flow and manager lifetime validation. | Requires scene/prefab work. |
| Add BootScene, FarmScene, TownScene, and CaveScene to Build Settings. | Runtime scene transitions. | ProjectSettings/Unity action deferred. |
| Run Play Mode duplicate-manager check. | Final acceptance of persistent architecture. | Human Unity validation required. |

## Next spec impact

WAVE_INTEGRATION_03 can start with `FarmScene` as the direct temporary target. It should not invent a new manager architecture. It should use `SceneNames.Farm` and the existing `SceneSpawnInstaller`/`SceneSpawnPoint` pattern for spawn placement.
