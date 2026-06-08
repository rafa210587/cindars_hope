# WAVE INTEGRATION 02 - Manager Audit

## Status

VALIDATED

## Managers found

| System | File/type | Runtime role | Singleton/static? | Scene dependency? | Duplicate risk | Decision |
|---|---|---|---|---|---|---|
| GameEventBus | `Assets/_Game/Scripts/Core/GameEventBus.cs` / static class | Global typed event bus. | Static global dictionary. | No scene object required. | Medium if subscriptions leak. | Reuse; keep OnEnable/OnDisable unsubscribe rule. |
| GameBootstrap | `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs` / MonoBehaviour | Persistent root and runtime manager initializer. | Static `Instance`, duplicate destroy, `DontDestroyOnLoad`. | Must exist in boot/persistent scene or temporary gameplay scene. | Low after boot root exists; medium while scenes can be opened directly. | Reuse as canonical manager root. |
| SaveManager | `Assets/_Game/Scripts/Save/SaveManager.cs` / MonoBehaviour | Save/load coordinator and save section capture/restore. | No static singleton; referenced by GameBootstrap. | Requires serialized references. | Medium if multiple scene copies exist. | Reuse through GameBootstrap; do not create parallel save manager. |
| InventoryManager | `Assets/_Game/Scripts/Inventory/InventoryManager.cs` / MonoBehaviour | Inventory runtime state and item operations. | No static singleton; referenced by GameBootstrap. | Requires item database binding. | Medium if duplicated per scene. | Reuse through GameBootstrap. |
| EconomyManager | `Assets/_Game/Scripts/Economy/EconomyManager.cs` / MonoBehaviour | Purchase/sell event handling. | No static singleton; subscribes to GameEventBus. | Requires InventoryManager and PlayerManager. | Medium if duplicated, because event handlers duplicate. | Reuse through GameBootstrap; ensure one active instance. |
| GameTimeManager | `Assets/_Game/Scripts/Core/GameTimeManager.cs` / MonoBehaviour | Day/night phase and game time ticks. | No static singleton; subscribes to DayStartedEvent. | Requires TimeManager and optional ModalManager. | Medium if duplicated, because ticking/events duplicate. | Reuse through GameBootstrap. |
| TimeManager | `Assets/_Game/Scripts/Core/Time/TimeManager.cs` / MonoBehaviour | Current day state and day advance events. | No static singleton. | Referenced by GameBootstrap and WorldTimeProvider. | Medium if duplicated. | Reuse through GameBootstrap. |
| WorldTimeProvider | `Assets/_Game/Scripts/World/WorldTimeProvider.cs` / MonoBehaviour | Save/restore bridge for world calendar/lunar state. | No static singleton. | Requires TimeManager/calendar/lunar services. | Low/medium if scene-local copies diverge. | Reuse existing provider when scene wiring needs world save. |
| ScenePortal | `Assets/_Game/Scripts/SceneManagement/ScenePortal.cs` / MonoBehaviour | Interactable scene transition entrypoint. | No singleton. | Scene-local portal objects. | Low if portal objects are scene-local. | Reuse; no new transition service created. |
| SceneTransitionState | `Assets/_Game/Scripts/SceneManagement/SceneTransitionState.cs` / static class | Pending spawn id handoff. | Static state. | No scene object required. | Low; must clear after spawn. | Reuse. |
| SceneSpawnInstaller | `Assets/_Game/Scripts/SceneManagement/SceneSpawnInstaller.cs` / MonoBehaviour | Applies pending/default spawn point to player transform. | No singleton. | Scene-local spawn wiring. | Low if one per loaded gameplay scene. | Reuse in WAVE_INTEGRATION_03. |
| ModalManager | `Assets/_Game/Scripts/UI/Modal/ModalManager.cs` / MonoBehaviour | Modal stack/UI modal state. | No static singleton found. | Referenced by GameBootstrap/GameTimeManager. | Medium if duplicated with active modals. | Reuse through GameBootstrap/UI root. |
| GameplayInputRouter | `Assets/_Game/Scripts/UI/Input/GameplayInputRouter.cs` / MonoBehaviour | Gameplay/UI input routing. | No static singleton found. | Scene/UI root object. | Medium if multiple routers process input. | Reuse one active router in future UI root. |
| HUD components | `Assets/_Game/Scripts/UI/HUD/*.cs`, `Assets/_Game/Scripts/UI/DebugHud.cs` | HUD views/projections. | No canonical HUDRoot class found. | UI scene/canvas objects. | Medium if multiple HUD canvases exist. | Defer visual HUD root to later UI integration spec. |

## Managers missing

| System | Required by | Decision |
|---|---|---|
| BootScene | Target boot flow. | Missing; create in Unity follow-up. |
| PersistentManagers scene or prefab root | Stable manager lifetime. | Missing as named scene/prefab; reuse `GameBootstrap` as content when created. |
| UIRoot | Future HUD/inventory/menu integration. | No canonical class found; defer to UI scene/prefab spec. |
| HUDRoot | Future HUD integration. | No canonical root found; HUD components exist. Defer visual root creation. |
| SceneNames | WAVE_INTEGRATION scene-name contract. | Created as minimal static contract in `Assets/_Game/Scripts/SceneManagement/SceneNames.cs`. |

## Duplicate risk

| Manager | Risk | Mitigation |
|---|---|---|
| GameBootstrap | Multiple gameplay scenes may contain a bootstrap while direct scene opens are still allowed. | Existing static `Instance` destroys duplicates; target Boot/PersistentManagers flow should keep only one root. |
| EconomyManager | Duplicate GameEventBus subscriptions could process purchases/sells more than once. | Host only under canonical GameBootstrap/persistent root; verify one active instance in Play Mode. |
| GameTimeManager/TimeManager | Duplicate ticks/day events could desync time. | Host under canonical GameBootstrap; avoid scene-local persistent copies. |
| ModalManager/Input router | Duplicate UI/input handlers could process input twice. | Defer to one UI root; future UI wiring should verify only one active router/modal stack. |
| SceneTransitionState | Static pending spawn state may survive failed transitions. | Existing `SceneSpawnInstaller` clears pending spawn after use/failure; future transitions should keep that rule. |

## Required follow-up

| Follow-up | Target spec |
|---|---|
| Create/verify BootScene and PersistentManagers root in Unity. | WAVE_INTEGRATION_03+ |
| Add gameplay scenes to Build Settings. | WAVE_INTEGRATION_03+ |
| Validate one active GameBootstrap and no duplicated event-driven managers in Play Mode. | WAVE_INTEGRATION_03+ / human validation |
| Place player spawn/camera using FarmScene as primary target. | WAVE_INTEGRATION_03 |
