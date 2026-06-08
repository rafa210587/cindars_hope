# WAVE INTEGRATION 02 - Scene Architecture e Persistent Managers - Execution Report

## Status

BUILD_VALIDATED

## Summary

Scene architecture and persistent manager strategy were documented without changing scenes, prefabs, ScriptableObjects, packages, ProjectSettings, or gameplay behavior. The project already has a canonical persistent runtime root in `GameBootstrap`, plus existing scene transition/spawn primitives in `ScenePortal`, `SceneTransitionState`, and `SceneSpawnInstaller`.

This spec created a minimal `SceneNames` contract because no equivalent scene-name contract existed. It did not create a new transition service or manager root.

## Source documents read

| Document | Found | Notes |
|---|---|---|
| `AGENTS.md` | YES | Project implementation and git rules. |
| Attached `WAVE_INTEGRATION_02_scene_architecture_persistent_managers.md` | YES | Active spec supplied by human attachment. |
| `docs/project/CURRENT_STATE.md` | YES | Confirms WAVE 07.01 baseline complete before this spec. |
| `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md` | YES | Updated to reflect WAVE_INTEGRATION_02 status. |
| `docs/validation/WAVE_07_01_UNITY_CLEAN_BASELINE_SCENE_INVENTORY_REPORT.md` | YES | Baseline dependency evidence. |
| `docs/validation/WAVE_07_SCENE_INVENTORY.md` | YES | Scene inventory dependency. |
| `docs/validation/WAVE_00_12_RECONCILIATION_AUDIT.md` | YES | Legacy status/warnings context. |
| `docs/validation/WAVE_00_12_SPEC_CODE_AUDIT_REPORT.md` | YES | Legacy docs validation debt context. |
| `docs/validation/EDITOR_VALIDATION_CONTRACT_HOTFIX_REPORT.md` | NO | Missing in repo. |
| `docs/validation/FUTURE_SPECS_MOVE_TO_FEATURES_FUTURAS_REPORT.md` | YES | Confirms future specs are out of active queue. |
| `docs/specs/a_implementar/features_futuras/README.md` | YES | Confirms future specs execution policy. |
| `docs/design/` | YES | Folder exists. |
| `docs/directions/` | NO | Design/directions folder not found: `docs/directions` |
| `docs/design_directions/` | NO | Design/directions folder not found: `docs/design_directions` |
| `docs/project/` | YES | Folder exists. |
| `docs/validation/` | YES | Folder exists. |

## Preflight

| Check | Result |
|---|---|
| Branch | PASS - `dev` |
| Working tree | PASS - clean before execution |
| Latest commits | `1c71b70`, `d5abb13`, `d0e895c`, `d3f3364`, `30627d8`, `e083f75`, `5aa2fe3`, `04d85a8` |

## Spec 01 baseline

| Check | Result | Evidence |
|---|---|---|
| Spec 01 baseline file | FOUND | `docs/validation/WAVE_07_01_UNITY_CLEAN_BASELINE_SCENE_INVENTORY_REPORT.md` |
| Assembly-CSharp PASS | PASS | Baseline report records exit code 0. |
| Assembly-CSharp-Editor PASS | PASS | Baseline report records exit code 0. |
| Scene inventory created | PASS | `docs/validation/WAVE_07_SCENE_INVENTORY.md` exists. |
| Future specs gate treated | PASS | Baseline report records future spec gate PASS. |
| Can continue | PASS | Baseline is FOUND_AND_VALID. |

## Build validation

| Target | Before | After | Result |
|---|---|---|---|
| Assembly-CSharp | PASS - exit code 0, 0 warnings, 0 errors | PASS - exit code 0, 0 warnings, 0 errors | PASS |
| Assembly-CSharp-Editor | PASS - exit code 0, 3 pre-existing warnings, 0 errors | PASS - exit code 0, 0 warnings, 0 errors | PASS |
| Docs validation | EXPECTED_FAIL_LEGACY_ONLY | EXPECTED_FAIL_LEGACY_ONLY | PASS_WITH_LEGACY_WARNINGS |

## Scene roles

| Role | Scene/path | Exists | Decision |
|---|---|---:|---|
| BootScene | `MISSING` | No | MISSING_CREATE_IN_FOLLOWUP_OR_HUMAN_UNITY_ACTION |
| PersistentManagers | `MISSING` | No | MISSING_CREATE_IN_FOLLOWUP_OR_HUMAN_UNITY_ACTION |
| FarmScene | `Assets/_Game/Scenes/FarmScene.unity` | Yes | Use as WAVE_INTEGRATION_03 direct temporary target. |
| TownScene | `Assets/_Game/Scenes/TownScene.unity` | Yes | Keep as follow-up transition target. |
| CaveEntranceScene | `MISSING` | No | Defer to follow-up/human Unity action. |
| CaveRuntimeScene | `Assets/_Game/Scenes/CaveScene.unity` | Yes | Alias to current `CaveScene`; no asset rename. |
| HomeInteriorScene | `MISSING` | No | Defer to follow-up/human Unity action. |

## Persistent manager audit

| Manager | Found | File/type | Duplicate risk | Decision |
|---|---:|---|---|---|
| GameEventBus | Yes | `Assets/_Game/Scripts/Core/GameEventBus.cs` / static class | Medium subscription leak risk | Reuse. |
| GameBootstrap | Yes | `Assets/_Game/Scripts/Core/Bootstrap/GameBootstrap.cs` / persistent MonoBehaviour root | Low/medium until boot scene exists | Reuse as canonical root. |
| SaveManager | Yes | `Assets/_Game/Scripts/Save/SaveManager.cs` | Medium if scene copies duplicate | Reuse through GameBootstrap. |
| InventoryManager | Yes | `Assets/_Game/Scripts/Inventory/InventoryManager.cs` | Medium if scene copies duplicate | Reuse through GameBootstrap. |
| EconomyManager | Yes | `Assets/_Game/Scripts/Economy/EconomyManager.cs` | Medium because it subscribes to GameEventBus | Reuse through GameBootstrap. |
| GameTimeManager | Yes | `Assets/_Game/Scripts/Core/GameTimeManager.cs` | Medium if duplicate ticks/events | Reuse through GameBootstrap. |
| TimeManager | Yes | `Assets/_Game/Scripts/Core/Time/TimeManager.cs` | Medium if duplicate day state | Reuse through GameBootstrap. |
| WorldTimeProvider | Yes | `Assets/_Game/Scripts/World/WorldTimeProvider.cs` | Low/medium if state diverges | Reuse existing provider. |
| ScenePortal/SceneSpawnInstaller | Yes | `Assets/_Game/Scripts/SceneManagement/*.cs` | Low if scene-local | Reuse; no new transition service. |
| ModalManager/Input/HUD components | Yes/partial | `Assets/_Game/Scripts/UI/**` | Medium if duplicated | Defer visual root creation; reuse existing components. |

## Architecture decision

| Item | Decision |
|---|---|
| Strategy | DOCUMENTATION_ONLY_HUMAN_UNITY_ACTION_REQUIRED |
| Runtime manager root | Reuse existing `GameBootstrap`. |
| Persistent lifetime | One `GameBootstrap` survives with `DontDestroyOnLoad`; duplicates destroyed by existing guard. |
| Load flow | Current: direct `FarmScene`; target: `BootScene -> PersistentManagers/GameBootstrap -> FarmScene`. |
| Transition flow | Reuse `ScenePortal`, `SceneTransitionState`, and `SceneSpawnInstaller`. |
| Next spec target | `Assets/_Game/Scenes/FarmScene.unity`. |

## Optional code created

| File | Reason | Used by |
|---|---|---|
| `Assets/_Game/Scripts/SceneManagement/SceneNames.cs` | Minimal scene-name contract; no equivalent existed. | WAVE_INTEGRATION_03+ |
| `Assets/_Game/Scripts/SceneManagement/SceneNames.cs.meta` | Unity asset metadata for the new C# script. | Unity asset database |

## Scene/prefab/asset changes

| File | Changed? | Reason |
|---|---:|---|
| `Assets/**/*.unity` | NO | Scene changes deferred. |
| `Assets/**/*.prefab` | NO | Prefab changes deferred. |
| `Assets/**/*.asset` | NO | ScriptableObject/asset changes not needed. |
| `Packages/**` | NO | Package changes forbidden/not needed. |
| `ProjectSettings/**` | NO | Build Settings/ProjectSettings deferred to human Unity action. |

## Human Unity actions required

| Action | Required before | Reason |
|---|---|---|
| Create/verify `BootScene`. | Final boot flow. | Missing scene; scene creation deferred. |
| Create/verify `PersistentManagers` scene or prefab root using existing `GameBootstrap`. | Final persistent manager flow. | Requires scene/prefab work. |
| Add target scenes to Build Settings. | Runtime scene transitions by name. | Requires Unity/ProjectSettings action. |
| Validate no duplicate managers in Play Mode. | Before final acceptance of WAVE_INTEGRATION. | Human Unity validation required. |

## Acceptance criteria matrix

| AC | Result | Evidence |
|---|---|---|
| AC-01 | PASS | Spec 01 baseline found and valid. |
| AC-02 | PASS | Runtime build passed before/after. |
| AC-03 | PASS | Editor build passed before/after. |
| AC-04 | PASS | Scene inventory imported and current scan repeated. |
| AC-05 | PASS | Scene roles defined in architecture document. |
| AC-06 | PASS | Manager audit created. |
| AC-07 | PASS | Persistent strategy chosen. |
| AC-08 | PASS | Duplicate risks documented. |
| AC-09 | PASS | Load flow documented. |
| AC-10 | PASS | WAVE_INTEGRATION_03 target is FarmScene. |
| AC-11 | PASS | No gameplay created. |
| AC-12 | PASS | No forbidden scene/prefab/asset changes. |
| AC-13 | PASS | This execution report created. |

## Decision

- Can start WAVE_INTEGRATION_03: YES.
- Blocking issues: None for code/docs baseline.
- Runtime risk: BootScene/PersistentManagers and Build Settings are still missing and require Unity action.
- Editor risk: Human Unity open/Console validation still pending.

## Unity validation

Unity validation: NOT RUN
Reason: Spec explicitly excludes final PlayMode and scene/prefab changes; human Unity validation is expected after execution.
Command attempted: N/A
Residual risk: Unity Editor Console and Build Settings not validated locally
