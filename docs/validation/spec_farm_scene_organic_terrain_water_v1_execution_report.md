# spec_farm_scene_organic_terrain_water_v1 — Execution Report

Status: `PARTIAL`

## Scope Lock

Changed only the assigned terrain painter, FarmScene generator, terrain validator, focused EditMode tests, and this report. No scene, prefab, ScriptableObject, metadata, PNG, generated project file, Player, Package, ProjectSettings, spec, or execution trace was edited. No commit was created.

## Dependency Chain

`spec_farm_scene_spatial_contract_v1`, `spec_farm_scene_collision_navigation_v1`, and `spec_farm_scene_keyart_macro_composition_v1` were source-reviewed but remain `PARTIAL` because Unity is unavailable and the generated projects are stale. Their canonical `FarmSceneSpatialContract` polygons are consumed unchanged here. This implementation does not alter physical collision; the existing collision materializer continues to consume those same lake/river polygons and preserve the bridge corridor.

## Existing Systems Audit

| System | Finding | Decision |
|---|---|---|
| `WorldTilemapGround` | Sole existing Tilemap painter; only rectangle APIs existed. | Extended with deterministic polygon and transition-ring rasterization; no second painter or RuleTile framework. |
| `FarmSceneSpatialContract` | Owns canonical non-rectangular lake and river polygons plus bridge footprint. | Reused without edits for visual water. |
| `CreateMvpFarmScene` | Sole FarmScene generator; lake was previously four overlapping rectangles and river was segmented rectangles. | It now paints the lake and river from contract polygons; paths use the same polygon API. |
| Existing tiles | `ground_water`, `ground_path_dirt`, and `ground_cliff_rock` exist; named grass-dirt and water-rock edge sprites do not. | No PNG was fabricated. Existing dirt/rock tiles are used as temporary transition visual and the missing authored edge kit is explicit debt. |

## Spec Compliance Matrix

| Requirement | Evidence | Status |
|---|---|---|
| Deterministic polygon mask | `RasterizePolygonCells` samples cell centers using a pure parity test; `PaintPolygon` uses its result. | Implemented; EditMode execution pending. |
| Concave polygon not bounding-box filled | `FarmTerrainMaskTests.PaintPolygon_ConcaveMask_DoesNotFillBoundingBox`. | Implemented; Unity Test Runner NOT RUN. |
| Eight-neighbour transition ring | `RasterizeTransitionRingCells` and its focused test. | Implemented; Unity Test Runner NOT RUN. |
| Lake/river from spatial contract | `PaintWaterFootprint` uses `FarmSceneSpatialContract.Lake` and `.River`; water tiles are no longer painted through rectangle APIs. | Source reviewed; regeneration NOT RUN. |
| Organic paths | Four path polygons use non-rectangular corners and the same painter. | Source reviewed; capture NOT RUN. |
| Bridge corridor physical consistency | Terrain does not create colliders; existing canonical collision paths still omit the bridge corridor. | Source reviewed; Unity navigation validation NOT RUN. |
| Validator | `ValidateFarmTerrainMasks` checks out-of-contract water, exposed unringed water, path intersections, and 64×44 ground coverage. | Implemented; menu NOT RUN. |

## Transition Art Debt

The source audit confirms that `ground_dirt_edge_*` and `ground_water_rock_edge_*` do not exist. Per task constraint, no art generation/import was attempted. `WaterToRock` temporarily paints `ground_cliff_rock` and `GrassToDirt` temporarily paints `ground_path_dirt`; these are deterministic, existing assets, not a claim that authored transition art is complete. Replace these aliases only through the canonical art pipeline once edge sprites exist, then rerun the terrain validator and capture.

## Testing Quality Gate

```text
Changed runtime code:           NO (editor-only painter/generator/validator)
Changed deterministic logic:    YES
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES
Automated tests command:        powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter 'CindarsHope.Tests.EditMode.Editor.FarmTerrainMaskTests'
Manual Play Mode scenario:      DEFERRED_TO_FINAL_VALIDATION (per spec)
Justification if no tests:      Focused tests exist but Unity Test Runner cannot start without the configured Unity executable.
Residual risk:                  Unity has not imported the new editor scripts, regenerated FarmScene, run the terrain validator, or captured the resulting masks.
```

## Validation

| Command | Exit code | Result |
|---|---:|---|
| `git diff --check` | 0 | PASS. |
| `dotnet build .\CindarsHope.Editor.csproj --no-restore` | 1 | Pre-existing generated-project failure: `CindarsHope.Runtime.csproj` omits dependency source `FarmSceneSpatialContract.cs`, producing ten `CS0103` errors in `FarmSceneRuntimeBootstrap.cs`. No project file was edited. |
| `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\docs\run_strict_validation.ps1` | 1 | `UNITY_PROJECT_BUILD_FAILURE`: pre-existing docs-governance errors plus the stale `FarmSceneSpatialContract` generated-project failure. |
| Unity compile / EditMode / terrain menu / capture | NOT RUN | Unity executable is unavailable locally. |

Unity validation: NOT RUN
Reason: configured Unity Editor executable is unavailable locally; generated C# project files are stale and out of scope.
Command attempted: `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\unity\RunUnityCompileValidation.ps1`
Residual risk: Unity compilation, scene regeneration, mask painting, validator output, capture, and bridge traversal remain unvalidated locally.

## Honest Status Rationale

The code and focused test coverage are present, and static whitespace validation passes. The target is `UNITY_VALIDATED` plus visual capture, neither of which is available in this environment. The known stale generated-project failure also prevents a C# build claim. Therefore this execution remains `PARTIAL`; it is not `BUILD_VALIDATED`, `UNITY_VALIDATED`, or accepted.

## Independent Review Correction

The initial slice left no-op `CreateRiverSegment` and `CreateLakeBody` calls/helpers after switching to canonical mask painting. They have been removed. `CreateRiverAndBridge` now creates water only through `PaintWaterFootprint` for the canonical river and lake polygons, then creates the bridge. The legacy-name absence scan below is the supporting evidence for the validator's `Legacy rectangle painters referenced: 0` contract.

## Remaining Work

1. Open the project with a valid Unity Editor, regenerate FarmScene with the canonical menu, and run `CindarsHope/Validar Mascaras de Terreno FarmScene`.
2. Confirm literal validator results `Water cells outside contract: 0`, `Unringed exposed water cells: 0`, and `Legacy rectangle painters referenced: 0`.
3. Run the focused EditMode suite and Unity compile validation with real exit code 0.
4. Capture the full map to verify curved lake/river/path contours and bridge traversal.
5. Generate/import authored grass-dirt and water-rock edge tiles through the canonical art pipeline; remove the temporary existing-tile aliases only after that asset evidence exists.
