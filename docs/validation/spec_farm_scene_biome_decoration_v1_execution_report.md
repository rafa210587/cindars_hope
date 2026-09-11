# spec_farm_scene_biome_decoration_v1 — Execution Report

Status: `PARTIAL`

## Scope Lock

Changed only the permitted editor planner, FarmScene generator call, editor validator, focused EditMode test and this execution report. No runtime Farm files, scenes, prefabs, ScriptableObjects, art PNG/meta, generated project file, Player, Package, ProjectSettings, spec or execution trace was changed. Shared dirty-worktree edits were preserved. No commit was created.

## Dependency Chain

`spec_farm_scene_landmarks_and_agriculture_v1` (and its specs 1–4 predecessors) is source-reviewed as `PARTIAL`; its canonical spatial contract and visual-only landmark roots are reused unchanged. Unity remains unavailable and generated C# projects remain stale, so this slice does not claim dependency validation beyond source review.

## Existing Systems and Art Gap Audit

| Existing item | Biome reuse | Decision |
|---|---|---|
| `foliage/flower_patch`, `bush_leafy`, `bush_berry` | Meadow, WaterEdge, Homestead, AnimalPen | Reused; no PNG generated. |
| `trees/tree_oak`, `tree_pine`; `foliage/tree_stump`, `log_fallen`, `mushroom_cluster` | Forest | Reused; no PNG generated. |
| `props/rock_ore_0..2`, `foliage/tree_stump` | CliffBase | Reused; no PNG generated. |
| `WorldSpriteLibrary` | All placement resolution | Reused as the single existing editor art loader. |
| Authored water-edge reeds/lilies and bespoke cliff-edge scatter | WaterEdge / CliffBase | Real art gap debt only; no generation or import was attempted because the current catalog already supplies a coherent safe fallback and the task forbids art generation. |

## Spec Compliance Matrix

| Requirement | Evidence | Status |
|---|---|---|
| Stable seeded plan | `FarmDecorationPlanner.Seed = 20260814`; FNV-style integer cell hash over seed/biome/cell, with no `UnityEngine.Random` or `string.GetHashCode`. | Implemented; Unity test NOT RUN. |
| Six biomes | `FarmDecorationBiome` and deterministic candidate regions define Meadow, Forest, WaterEdge, CliffBase, Homestead and AnimalPen. | Implemented. |
| Exact forbidden mask | Planner rejects bounds exterior, spatial water/solid/building/crop/bridge/trigger footprints, four declared path corridors and named interaction approach squares. | Implemented; menu NOT RUN. |
| Visual-only materialization | `CreateFarmDecoration` creates one `FarmDecoration` root and sprite-renderer children only; it adds no collider, interaction or gameplay component. | Source reviewed; regeneration NOT RUN. |
| Density contract | Named density and min/max constants exist per biome; validator and focused test check all six declared ranges. | Implemented; Unity test/menu NOT RUN. |
| Reuse before generation | The art table above maps every used ID to an existing catalog asset. | Implemented; no art files changed. |

## Testing Quality Gate

```text
Changed runtime code:           NO (editor-only planner/generator/validator)
Changed deterministic logic:    YES
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES
Automated tests command:        powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter 'CindarsHope.Tests.EditMode.Editor.FarmDecorationPlannerTests'
Manual Play Mode scenario:      DEFERRED_TO_FINAL_VALIDATION (capture is owned by the final Farm keyart acceptance spec)
Justification if no tests:      Focused tests exist, but the Unity Test Runner cannot start because the configured Unity executable is unavailable.
Residual risk:                  Unity has not imported the editor scripts, regenerated FarmScene, run the menu validator, or visually inspected decoration density and overlap.
```

## Validation

| Command | Exit code | Result |
|---|---:|---|
| `git diff --check` | 0 | PASS. |
| Static forbidden API scan | 0 | PASS for new planner, validator and tests: no `UnityEngine.Random`, `string.GetHashCode`, `GameObject.Find` or runtime global scene search. |
| `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\docs\run_strict_validation.ps1` | 1 | `UNITY_PROJECT_BUILD_FAILURE`: pre-existing docs-governance errors and stale generated C# project build (`CindarsHope.Runtime.csproj` omits `FarmSceneSpatialContract.cs`). |
| Unity compile / focused EditMode / decoration menu / capture | NOT RUN | Configured Unity executable is unavailable locally. |

Unity validation: NOT RUN
Reason: configured Unity Editor executable is unavailable locally; generated C# project files are stale and outside scope.
Command attempted: `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\unity\RunUnityCompileValidation.ps1`
Residual risk: Unity compilation, scene regeneration, `Forbidden decoration placements: 0`, `Decoration biomes valid: 6/6`, sprite loading and visual density remain unvalidated locally.

## Required Human Capture Instructions

After Unity is available, regenerate FarmScene through the canonical generator, run `CindarsHope/Validar Decoracao FarmScene`, and capture one full-map image plus close-ups of the west forest, north cliff, homestead, animal pen and lake edge. The validator must literally report `Forbidden decoration placements: 0` and `Decoration biomes valid: 6/6`. Confirm visually that `FarmDecoration` has no colliders or interaction components, props do not cover bridge/crop/path/approach cells, and no magenta fallback sprite is visible.

## Honest Status Rationale and Remaining Work

The pure planner, focused tests, minimal generator materialization and plan-level validator are present. This target requires Unity validation plus visual capture; neither can be produced in the current environment, and generated C# projects are known stale. The status is therefore `PARTIAL`, not build-, Unity- or Play Mode-validated. Run the focused suite and validator in Unity, regenerate/capture the scene, then record the actual outputs before promotion.
