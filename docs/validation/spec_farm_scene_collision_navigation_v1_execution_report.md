# spec_farm_scene_collision_navigation_v1 — Execution Report

Status: `PARTIAL`

## Scope lock

Changed only generator/editor-validation/Farm Scene contract/test files and this report/scenario. No scene, prefab, asset, meta, art, Player, Packages or ProjectSettings file was edited. Existing dirty files were preserved; no commit was created.

## Dependency and reuse audit

`spec_farm_scene_spatial_contract_v1` is source-reviewed but `PARTIAL`: its catalog is reused as the sole footprint contract, without claiming Unity validation. The audit found `CreateMvpFarmScene` as the sole generator, existing `Collider2D` physics and `PlayerController`; no navmesh, pathfinding or Player system was introduced. `FarmSceneZoneMarker` remains trigger-only and is not used for blocking.

## Spec compliance matrix

| Requirement | Evidence | Status |
|---|---|---|
| Named terrain collision ownership | `MaterializeFarmSpatialCollision` creates `Collision_<footprintId>` only for Lake, River and Mountain. Buildings retain their functional owners and are checked by the editor validator without duplicate colliders. | Implemented; Unity scene generation NOT RUN. |
| Irregular water/mountain physics | Lake, river and mountain have canonical polygons with more than four points. All three are explicitly materialized as `PolygonCollider2D`; river uses two canonical paths around the bridge. | Implemented; Unity NOT RUN. |
| Bridge has no blocking corridor | River physical paths stop at `x=[13,17]` around the bridge, so only the actual river-crossing corridor is free; focused tests check `(15,3)` walkable and `(18,3)` remains unblocked because it is outside the river contour. | Implemented; EditMode NOT RUN. |
| Reachability and owner validation | Editor-only `ValidateFarmSceneNavigation` flood-fills 64x44 cells from `DefaultSpawn`, verifies 10 approaches, and requires an existing non-trigger collider intersecting each Building footprint outside `FarmSpatialCollision`. | Implemented; menu NOT RUN. |
| Optional collision evidence | `CindarsHope/Dev/Capturar FarmScene Colliders (PNG)` overlays non-trigger collider bounds in `farm_capture_colliders.png` without regenerating the scene. | Implemented; NOT RUN. |
| No Player/input change | No file below `Assets/_Game/Scripts/Player/` was changed by this execution. | PASS (static). |

## Testing Quality Gate

```text
Changed runtime code:           YES
Changed deterministic logic:    YES
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES
Automated tests command:        .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter 'CindarsHope.Tests.EditMode.Farm.FarmSceneNavigationContractTests'
Manual Play Mode scenario:      docs/validation/playmode/spec_farm_scene_collision_navigation_v1_human_test_scenario.md
Justification if no tests:      Tests were added; execution requires unavailable Unity Editor.
Residual risk:                  Unity has not imported scripts, regenerated FarmScene, run the menu validator or executed physics in Play Mode.
```

## Validation

| Command | Exit code | Result |
|---|---:|---|
| `git diff --check` | 0 | PASS. |
| `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\docs\run_strict_validation.ps1` | 1 | FAIL: pre-existing docs validation errors and stale Unity-generated project (`FarmSceneSpatialContract` absent from generated project). |
| `dotnet build .\Assembly-CSharp.csproj --no-restore` | 1 | FAIL: stale generated Runtime project has ten pre-existing `FarmSceneSpatialContract` unresolved references. |
| `dotnet build .\CindarsHope.Editor.csproj --no-restore` | 1 | FAIL: same stale Runtime dependency failure. |
| `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\unity\RunUnityCompileValidation.ps1` | 1 / NOT RUN | configured `Unity.exe` path does not exist. |
| Focused EditMode / menu validator | NOT RUN | Unity Editor is unavailable locally; source tests now include non-rectangular polygon and outside-contour coverage. |

Unity validation: NOT RUN
Reason: configured Unity Editor executable is unavailable; generated C# projects are known stale from the dependency report.
Command attempted: `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\unity\RunUnityCompileValidation.ps1`
Residual risk: collider materialization, scene regeneration, validator output and Play Mode movement have not been validated locally.

## Non-regression review

Static review: no Player changes, no save DTO/event-bus changes, no runtime global scene search, no navmesh/pathfinding, and no status inflation. The editor validator uses editor-only scene inspection. Human scenario is present but NOT RUN.

## Honest status rationale

The source implementation, focused deterministic tests and Play Mode scenario are present. `PARTIAL` is retained because the required Unity compile, generated scene validation, EditMode execution and Play Mode route cannot run in this environment. The spatial-contract dependency is reused only as source-reviewed contract evidence.
