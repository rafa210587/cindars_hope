# spec_farm_scene_landmarks_and_agriculture_v1 — Execution Report

Status: `PARTIAL`

## Scope Lock

Owned changes are limited to `CreateMvpFarmScene.cs`, `FarmLevel1LayoutContract.cs`, the new landmark validator, focused Farm EditMode test, this report, and its human scenario. No scene, prefab, ScriptableObject, asset metadata, PNG, save code, project file, Package, ProjectSettings, spec, or trace was changed. Shared-worktree changes in the generator and spatial contracts were preserved and not attributed to this spec. No commit was created.

`FarmLandmarkCompositionContract` is in the existing permitted `FarmLevel1LayoutContract.cs` rather than a new Farm file: it is pure, editor-agnostic landmark data tied to existing layout anchors, and avoids creating a parallel Farm layout catalog.

## Dependency Chain

The preceding spatial, collision/navigation, macro-composition and organic-terrain slices were source-reviewed as `PARTIAL`. Their canonical `FarmSceneSpatialContract.CropField` polygon is reused unchanged for the field soil. Unity is unavailable and generated C# projects are stale, so the dependencies have no Unity evidence yet; this implementation does not alter their collision polygons, navigation policy, or gameplay roots.

## Existing Systems Audit

| System / asset | Finding | Decision |
|---|---|---|
| `FarmPlot` / `FarmPlots` | Existing tile farming uses an empty registry and arable tile system; fixed plot roots were intentionally removed. | Reused unchanged; no `FarmPlot` added. Field soil is painted from the existing `CropField` polygon and crop rows are visual-only. |
| `AnimalReleaseHandler` | Existing handlers are on `Coop_01` and `Barn_01`. | Reused unchanged; pen props are children of `FarmAnimalHousings`, with no collider or handler. |
| `FishingSpot`, `FonteAnya`, cave and ore roots | Existing functional roots already provide interactions. | Reused unchanged; only child visual props added. |
| `WorldSpriteLibrary` | Existing generated sprites include soil, crops, well, fence, hay, animal, foliage, cave and ore art. | Reused; no asset was generated or edited. |

Created new runtime gameplay systems: none.

## Spec Compliance Matrix

| Requirement | Evidence | Status |
|---|---|---|
| Seven named landmarks | `FarmLandmarkCompositionContract` supplies required roots and bounds; `ValidateFarmSceneLandmarks` checks each. | Implemented; Unity menu NOT RUN. |
| Central field uses canonical shape | `PaintCentralFieldSoil` calls `PaintPolygon` with `FarmSceneSpatialContract.CropField`; four `Visual_CropRow_*` roots each receive existing crop sprites. | Source reviewed; scene regeneration NOT RUN. |
| Functional systems are not duplicated | Visual helper adds only `GameObject` + `SpriteRenderer`; no collider, `FarmPlot`, `AnimalReleaseHandler`, `FishingSpot`, save or event code. | Source reviewed. |
| Approaches validated | Validator checks four central-field approach samples and one approach per other landmark using the existing navigation raster. | Implemented; Unity navigation NOT RUN. |
| Deterministic pure coverage | `FarmLandmarkCompositionTests` checks required inventories, bounds, row count and central approach count. | Added; Unity Test Runner NOT RUN. |

## Testing Quality Gate

```text
Changed runtime code:           NO (editor scene generator/validator plus pure layout contract)
Changed deterministic logic:    YES
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES
Automated tests command:        powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter 'CindarsHope.Tests.EditMode.Farm.FarmLandmarkCompositionTests'
Manual Play Mode scenario:      docs/validation/playmode/spec_farm_scene_landmarks_and_agriculture_v1_human_test_scenario.md
Justification if no tests:      Focused tests exist, but the Unity Test Runner cannot start because the configured Unity executable is unavailable.
Residual risk:                  Unity has not imported scripts, regenerated FarmScene, validated approaches, or confirmed visual props do not obscure interactions.
```

## Validation

| Command | Exit code | Result |
|---|---:|---|
| `git diff --check` | 0 | PASS. |
| Owned-source forbidden API scan | 0 | No `GameObject.Find`/`FindObjectOfType`; `FindObjectsByType` is editor-only scene creation/validation. |
| `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\docs\run_strict_validation.ps1` | 1 | Known repository docs-governance errors and stale generated C# project build; no out-of-scope project file was edited. |
| Unity compile / focused EditMode / landmark and navigation menus / Play Mode | NOT RUN | Configured Unity executable is unavailable locally. |

Unity validation: NOT RUN
Reason: configured Unity Editor executable is unavailable locally; generated C# project files are stale and outside scope.
Command attempted: `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\unity\RunUnityCompileValidation.ps1`
Residual risk: Unity compilation, scene regeneration, validator output, crop interaction and landmark approach accessibility remain unvalidated locally.

## Non-Regression Review

PASS with residual Unity risk. The scope contains no save DTO change, gameplay system, event wiring, manual YAML, collider on a new visual child, or runtime global scene search. `FindObjectsByType` is used exclusively in editor code to locate generator roots and validate a materialized scene.

## How to Test

Human test scenario: `docs/validation/playmode/spec_farm_scene_landmarks_and_agriculture_v1_human_test_scenario.md`.

Expected test time: 8 minutes. Tester: human. Status: NOT RUN until Unity validation is available.

## Honest Status Rationale and Remaining Work

The pure contract, focused test, editor validator and visual-only composition are ready for Unity import. This cannot be promoted: the requested target requires Unity validation and Play Mode, strict validation exits 1 on known repository/governance and stale-generated-project failures, and Unity is absent. Regenerate FarmScene in Unity, run the landmark/navigation validators and focused test, perform the human scenario, then update this report with actual output before any promotion.
