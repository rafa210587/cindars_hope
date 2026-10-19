# spec_farm_scene_spatial_contract_v1 — Execution Report

Status: `PARTIAL`

## Scope lock

Allowed files changed: `Assets/_Game/Scripts/Farm/FarmLevel1LayoutContract.cs`, `Assets/_Game/Scripts/Farm/Scene/FarmSceneSpatialContract.cs`, `Assets/_Game/Scripts/Farm/Runtime/FarmSceneRuntimeBootstrap.cs`, `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`, `Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneSpatialContract.cs`, `Assets/_Game/Tests/EditMode/Farm/FarmSceneSpatialContractTests.cs`, and this report.

No `.unity`, `.prefab`, `.asset`, `.meta`, `Packages/**`, `ProjectSettings/**`, art, Player, or trace files were changed by this execution. No commit was created because the working tree is shared and dirty.

## Existing Systems Audit

Command: `rg -n "FarmSceneZoneMarker|FarmNonArableZones|CreateRiverSegment" Assets/_Game/Scripts --glob '*.cs'`

| Existing system | Finding | Decision |
|---|---|---|
| `FarmLevel1LayoutContract` | Existing 64x44 bounds and anchors | Extended with spatial extents; reused as coordinate source. |
| `FarmSceneZoneMarker` | Existing trigger-only zone marker | Reused; it remains a zone marker, not a solid-collision registry. |
| `FarmTileGrid` / `FarmNonArableZones` | Existing tilling exclusion mechanism | Reused by `FarmSceneRuntimeBootstrap`; no new grid or tilling system. |
| `CreateMvpFarmScene` | Existing sole scene generator, with river/lake/colliders | Extended to consume footprint bounds for foundation zones; final lake collision remains owned by the next physics spec. |
| Existing spatial catalog | None found by the audit command | Created the single `FarmSceneSpatialContract` catalog required by the spec. |

## Spec Compliance Matrix

| Requirement | Evidence | Status |
|---|---|---|
| Eleven unique canonical footprints | `FarmSceneSpatialContract.All` has lake, river, mountain, bridge, house, greenhouse, animal buildings, crafting yard, cave mouth, town exit, crop field. | Implemented; EditMode execution pending. |
| Points in 64x44 bounds and classifications | `FarmSceneSpatialContractTests.AllFootprints_HaveUniqueIdsAndAreInBounds` and `UsesHaveExplicitBlockingClassification`. | Implemented; execution pending. |
| Tilling follows catalog | Bootstrap registers catalog bounds through `FarmNonArableZones`; greenhouse remains explicit tillable exception. | Implemented; Unity execution pending. |
| Generator consumes catalog | Foundation zones use catalog bounds while preserving their established zone IDs; PlayerSpawn remains its original independent spawn trigger. | Implemented; regeneration not run. |
| Validator | Menu `CindarsHope/Validar Layout Espacial FarmScene`; success message is exactly `ValidateFarmSceneSpatialContract: 0 error(s)`. | Implemented; not run. |
| Build and scene evidence | Required Unity compile, scene generation/capture and validator have not run. | Blocked by environment. |

## Testing Quality Gate

```text
Changed runtime code:           YES
Changed deterministic logic:    YES
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES
Automated tests command:        powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter 'CindarsHope.Tests.EditMode.Farm.FarmSceneSpatialContractTests'
Manual Play Mode scenario:      DEFERRED_TO_FINAL_VALIDATION (per spec)
Justification if no tests:      N/A; tests were added but Unity Test Runner could not start.
Residual risk:                  Unity has not imported the new scripts, regenerated FarmScene, or executed the validator/tests.
```

## Validation

| Command | Exit code | Result |
|---|---:|---|
| `dotnet build .\Assembly-CSharp.csproj --no-restore` | 1 | Generated assets file missing. |
| `dotnet build .\Assembly-CSharp-Editor.csproj --no-restore` | 1 | Project file missing. |
| `dotnet build .\CindarsHope.Runtime.csproj --no-restore` | 1 | Stale Unity project does not include new `FarmSceneSpatialContract.cs`; bootstrap reports `CS0103`. |
| `dotnet build .\CindarsHope.Editor.csproj --no-restore` | 1 | Same stale Runtime dependency failure. |
| `run_strict_validation.ps1` via `powershell -ExecutionPolicy Bypass` | 1 | Pre-existing docs validation errors plus Unity generated-project build failure. |
| Unity compile validation | 1 / NOT RUN | Configured Unity path `C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe` does not exist. |
| Focused EditMode test | 1 / NOT RUN | Same unavailable Unity executable. |
| Forbidden API static scan | 0 | No newly introduced runtime global scene-search API. Existing editor-only generator uses `Object.FindObjectsByType`. |

Unity validation: NOT RUN
Reason: configured UnityEditorPath does not exist locally.
Command attempted: `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\unity\RunUnityCompileValidation.ps1`
Residual risk: Unity compile, scene regeneration, the menu validator, and EditMode tests are not validated locally.

## Honest Status Rationale

The implementation and deterministic test coverage are present, but the required Unity validation level cannot be reached. The generated C# projects are stale until Unity imports the new source file; editing generated project files is outside the allowed scope. Therefore this report deliberately records `PARTIAL`, not `BUILD_VALIDATED` or `UNITY_VALIDATED`.

## Independent Review Corrections

The independent review rejected the first implementation for three scope regressions. They were corrected without modifying scene YAML or the shared trace:

1. `Zone_PlayerSpawn` was restored to its original `(24,3)`, `1.4x1.4`, and `farm_zone_player_spawn` identity; it is deliberately not represented by one of the catalog's eleven footprints.
2. `CreateFarmSceneZoneFromFootprint` now accepts an explicit legacy `stableId`; all catalog-backed zone call sites preserve `farm_zone_crop_field`, `farm_zone_lake_fishing`, `farm_zone_construction`, `farm_zone_house_entrance`, `farm_zone_town_exit`, and `farm_zone_cave_entrance`.
3. The rectangular `FarmSpatialLakeCollider` was removed. Lake collision remains intentionally deferred to `spec_farm_scene_collision_navigation_v1`, which will materialize collision from the final polygonal/mask geometry rather than a bounding rectangle.

## Remaining Work

1. Open the project with a valid Unity Editor installation to import scripts.
2. Regenerate FarmScene through the canonical menu command, then run `CindarsHope/Validar Layout Espacial FarmScene` and confirm the required literal zero-error message.
3. Run the focused EditMode tests and Unity compile validation with exit code 0.
4. Capture the regenerated FarmScene using `FarmSceneCapture` during final validation; capture could not be produced without Unity.
