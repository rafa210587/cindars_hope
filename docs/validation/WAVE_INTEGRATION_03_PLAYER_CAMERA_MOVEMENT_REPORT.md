# WAVE INTEGRATION 03 - Player Spawn, Camera e Movement Baseline - Execution Report

## Status

BUILD_VALIDATED_SCENE_WIRED

## Summary

`FarmScene` was confirmed as the target scene from WAVE_INTEGRATION_02 and is already structurally wired for the first visual baseline: it contains a `Player` with `PlayerController`, `Rigidbody2D`, collider, default spawn wiring, camera follow, and bounds. No new player/movement/camera placeholder was created because equivalent systems already exist.

No scene, prefab, ScriptableObject, package, ProjectSettings, gameplay, HUD, inventory, shop, crafting, quest, skill, or cave feature changes were made. The validation performed here is build + static scene YAML/code validation. Human Play Mode validation is still required in Unity to confirm actual visual movement and camera behavior.

## Source documents read

| Document | Found | Notes |
|---|---|---|
| `AGENTS.md` | YES | Project rules and forbidden runtime search APIs. |
| Attached `WAVE_INTEGRATION_03_player_spawn_camera_movement_baseline.md` | YES | Active spec supplied by human. |
| `docs/project/CURRENT_STATE.md` | YES | Confirms WAVE_INTEGRATION_02 state. |
| `docs/specs/SPEC_REGISTRY_TO_IMPLEMENT.md` | YES | Updated with WAVE_INTEGRATION_03 result. |
| `docs/validation/WAVE_INTEGRATION_02_SCENE_ARCHITECTURE.md` | YES | Defines `FarmScene` as WAVE_INTEGRATION_03 target. |
| `docs/validation/WAVE_INTEGRATION_02_SCENE_ARCHITECTURE_DECISION.md` | YES | Confirms scene/prefab work was deferred to human Unity action. |
| `docs/validation/WAVE_INTEGRATION_02_MANAGER_AUDIT.md` | YES | Confirms existing manager architecture. |
| `docs/validation/WAVE_INTEGRATION_02_SCENE_ARCHITECTURE_PERSISTENT_MANAGERS_REPORT.md` | YES | Confirms WAVE_INTEGRATION_02 build validation and next target. |
| `docs/validation/WAVE_07_SCENE_INVENTORY.md` | YES | Confirms `FarmScene` exists as ACTIVE_TARGET. |
| `docs/validation/WAVE_00_12_RECONCILIATION_AUDIT.md` | YES | Legacy warning context. |
| `docs/validation/WAVE_00_12_SPEC_CODE_AUDIT_REPORT.md` | YES | Legacy docs validation classification. |
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
| Latest commits inspected | `5b5c60d`, `1c71b70`, `d5abb13`, `d0e895c`, `d3f3364`, `30627d8`, `e083f75`, `5aa2fe3` |

## Build validation

| Target | Before | After | Result |
|---|---|---|---|
| Assembly-CSharp | PASS - exit code 0, 0 warnings, 0 errors | PASS - exit code 0, 0 warnings, 0 errors | PASS |
| Assembly-CSharp-Editor | PASS - exit code 0, 3 pre-existing warnings, 0 errors | PASS - exit code 0, 3 pre-existing warnings, 0 errors | PASS |
| Docs validation | EXPECTED_FAIL_LEGACY_ONLY | EXPECTED_FAIL_LEGACY_ONLY | PASS_WITH_LEGACY_WARNINGS |

## Target scene

| Field | Value |
|---|---|
| TargetScene | FarmScene |
| TargetScenePath | `Assets/_Game/Scenes/FarmScene.unity` |
| Classification | ACTIVE_TARGET |
| Can be modified | Not modified in this spec; WAVE_INTEGRATION_02 deferred scene/prefab changes to human Unity action. |
| Static scene status | FOUND_AND_STRUCTURALLY_WIRED |

## Player audit

| Item | Found | Evidence | Decision |
|---|---:|---|---|
| Player object in FarmScene | YES | `Assets/_Game/Scenes/FarmScene.unity:1241` | Reuse. |
| PlayerController | YES | `Assets/_Game/Scripts/Player/PlayerController.cs`; scene reference at `FarmScene.unity:1289` | Reuse existing player controller. |
| PlayerDataSO assignment | YES | `FarmScene.unity:1290` | Reuse existing config. |
| Rigidbody2D | YES | `FarmScene.unity:1340`, gravity scale `0` | Reuse for movement/collision. |
| BoxCollider2D | YES | `FarmScene.unity:1294`, non-trigger | Reuse for physical footprint. |

## Movement audit

| Item | Found | Evidence | Decision |
|---|---:|---|---|
| Keyboard movement | YES | `PlayerController.ReadMoveInput()` uses WASD/arrows fallback | Reuse. |
| Rigidbody movement | YES | `PlayerController.FixedUpdate()` uses `Rigidbody2D.MovePosition` | Reuse. |
| Modal/menu movement suppression | YES | `PlayerController` checks farm action menu and modal manager | Reuse. |
| Placeholder movement needed | NO | Existing movement is functional by code and scene references | Do not create parallel movement script. |

## Camera audit

| Item | Found | Evidence | Decision |
|---|---:|---|---|
| Main Camera | YES | `FarmScene.unity:2859` | Reuse. |
| CameraFollow2D | YES | `Assets/_Game/Scripts/Camera/CameraFollow2D.cs`; scene reference at `FarmScene.unity:2876` | Reuse. |
| Camera target assigned | YES | `_target: {fileID: 502903062}` points to Player transform | Reuse. |
| Orthographic camera | YES | `FarmScene.unity` camera has `orthographic: 1`, size `8.5` | Validate visually in Play Mode. |
| Placeholder camera follow needed | NO | Existing follow script and scene reference are present | Do not create parallel camera script. |

## Scale/sorting audit

| Item | Found | Evidence | Decision |
|---|---:|---|---|
| FarmScaleContract | YES | `Assets/_Game/Scripts/Farm/FarmScaleContract.cs` | Use as scale/sorting reference. |
| Player footbox guidance | YES | `PlayerFootboxHeightPixels = 16f`; sorting method `Y_Foot` | Human visual validation required. |
| Camera tile guidance | YES | Farm camera width/height target constants exist | Human visual validation required. |
| Bounds object | YES | `FarmScene.unity:4254` | Reuse existing scene bounds. |

## Code created

| File | Reason |
|---|---|
| None | Existing player, movement, camera, spawn, scale, and scene wiring are present. |

## Scene/prefab/asset changes

| File | Changed? | Reason |
|---|---:|---|
| `Assets/**/*.unity` | NO | FarmScene already structurally wired; no direct scene edit needed. |
| `Assets/**/*.prefab` | NO | No prefab changes needed. |
| `Assets/**/*.asset` | NO | No ScriptableObject/asset changes needed. |
| `Packages/**` | NO | Package changes forbidden/not needed. |
| `ProjectSettings/**` | NO | Build Settings/ProjectSettings not touched. |

## Human Unity actions required

| Action | Required | Reason |
|---|---:|---|
| Run Play Mode checklist in `FarmScene` | YES | Static/build validation cannot prove visual movement/camera behavior. |
| Confirm no red Console errors | YES | Required integration gate. |
| Confirm player/camera movement and bounds | YES | Final visual validation for this spec. |

## Acceptance criteria matrix

| AC | Result | Evidence |
|---|---|---|
| AC-01 | PASS | WAVE_INTEGRATION_02 exists and defines FarmScene target. |
| AC-02 | PASS | Runtime build passed before/after. |
| AC-03 | PASS | Editor build passed before/after. |
| AC-04 | PASS | Player audited in code and scene YAML. |
| AC-05 | PASS | Movement audited in `PlayerController`. |
| AC-06 | PASS | Camera audited in `CameraFollow2D` and scene YAML. |
| AC-07 | PASS | Decision document created. |
| AC-08 | PASS | Spawn strategy defined using existing `SceneSpawnInstaller`. |
| AC-09 | PASS | No placeholder code created because existing systems are present. |
| AC-10 | PASS | No scene/prefab changes were made; existing wiring documented. |
| AC-11 | PASS | Human checklist created. |
| AC-12 | PASS | WAVE_INTEGRATION_04 can use FarmScene, pending human Play Mode validation. |

## Decision

- Can start WAVE_INTEGRATION_04: YES after human Play Mode checklist passes without blocker.
- Blocking issues: None in C# build/static scene wiring.
- Human Play Mode validation required: YES.

## Unity validation

Unity validation: NOT RUN
Reason: Unity Editor processes are already running for this editor/project, and starting a concurrent batchmode validation risks project lock/editor contention.
Command attempted: Not attempted to avoid concurrent Unity project lock while existing Unity processes are active.
Residual risk: Play Mode visual behavior and Console state not validated locally by this run

## Docs validation residual

`tools/docs/validate_docs.ps1` returned exit code 1 with known legacy-only failures outside this spec scope:

- `docs/specs/a_implementar/spec_test_harness_editmode_playmode_quality_gate.md` missing dependency headers: `Ordem de execucao`, `Depende de`, `Bloqueia`.
- `docs/specs/a_implementar/spec_test_harness_editmode_playmode_quality_gate.md` missing `required_adrs` and `required_game_rules`.
- Older validation reports missing `validated_adrs` and `validated_game_rules`.
- Two implemented specs cite amendments as canonical sources.

No new WAVE_INTEGRATION_03 report/doc was listed as a docs validation failure.
