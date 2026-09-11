# spec_farm_scene_keyart_macro_composition_v1 — Execution Report

Status: `PARTIAL`

## Scope Lock

Owned changes are limited to:

- `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpFarmScene.cs`
- `Assets/_Game/Scripts/Editor/Validation/ValidateFarmSceneComposition.cs`
- `Assets/_Game/Scripts/World/Scale/FarmSceneCompositionContract.cs`
- `Assets/_Game/Tests/EditMode/Farm/FarmSceneCompositionContractTests.cs`
- `docs/validation/playmode/spec_farm_scene_keyart_macro_composition_v1_human_test_scenario.md`
- this report

No `.unity`, `.prefab`, `.asset`, `.meta`, art, Player, `Packages/**`, `ProjectSettings/**`, spec, or execution-trace file was edited. Existing shared-worktree changes, including the dependency implementation in `CreateMvpFarmScene.cs`, were preserved. No generated project file was changed and no commit was created.

## Dependency Chain

Original target: `spec_farm_scene_keyart_macro_composition_v1`.

Dependency: `spec_farm_scene_collision_navigation_v1` — source-reviewed `PARTIAL`; it remains unvalidated only because Unity and generated projects are unavailable/stale. Its `FarmSceneSpatialContract` remains the physical-contract owner. This implementation does not alter its IDs, collision polygons, functional roots, or approach cells.

Can continue original target: YES (per owner instruction and source-reviewed dependency evidence).

## Existing Systems Audit

| System | Audit finding | Decision |
|---|---|---|
| `CreateMvpFarmScene` | Sole FarmScene generator; prior code already separates some visual children from functional roots. | Reused; only visual scale constants were centralized. |
| `FarmSceneSpatialContract` / navigation policy | Own physical footprints, terrain collision and approach validation. | Reused untouched; no collider/root movement. |
| `WorldSpriteLibrary` / `ScaleProfileLibrary` | Existing visual-data libraries. | Reused; no asset/importer/profile asset created. |
| `FarmSceneCapture` | Already emits full, homestead-closeup and animal-row capture lines. | Reused unchanged. |

## Spec Compliance Matrix

| Requirement | Evidence | Status |
|---|---|---|
| Six macro groups have target bounds | `FarmSceneCompositionContract.GetTargetBounds` defines NorthCliff, WestForest, CentralAgriculture, NorthEastHomestead, SouthAnimalRow and SouthEastLake. | Source implemented; Unity menu NOT RUN. |
| Generator respects functional limits | The bridge visual changed from `(4.6, 2.0, 1)` to `(1.8, 0.8, 1)`. It has no collider; the walkable corridor remains owned by the unchanged `FarmSceneSpatialContract` collision materialization. `LakeCompositionAnchor` is renderer/collider/ID-free evidence only. | Source reviewed; Unity physics NOT RUN. |
| Scale outliers are checked | `ValidateFarmSceneComposition` checks bridge, roof, coop, barn and tree scale policy and emits the exact required scale line. | Source implemented; menu NOT RUN. |
| Six groups are checked | Validator emits the exact `ValidateFarmSceneComposition: 6/6 groups valid` form on success. | Source implemented; menu NOT RUN. |
| Required captures | Existing `FarmSceneCapture` already loops over full, homestead and animal-row views, logging `[FarmSceneCapture] salvo:` once per view. | Source reviewed; Unity capture NOT RUN. |
| Physics remains valid | No physical contract/collider/approach implementation was edited; human scenario requires the pre-existing navigation validator to prove `10/10`. | NOT RUN in Unity. |

## Macrocomposition Change Audit

| Keyart mass | Change in this spec | Evidence/status |
|---|---|---|
| North cliff | Catalogued with a target bound only. | No visual geometry changed. |
| West forest | Catalogued with a target bound; existing tree multiplier centralized. | No tree placement or effective tree scale changed. |
| Central agriculture | Catalogued with a target bound only. | No terrain/crop geometry changed. |
| Northeast homestead | Roof target width reduced from the former inline `11` to the contract value `9`. | Actual visual-width reduction; no position/root/collider changed; Unity capture pending. |
| South animal row | Catalogued with a target bound; existing building heights centralized. | No effective scale or placement changed. |
| Southeast lake / bridge | Bridge visual scale reduced from `(4.6, 2.0, 1)` to `(1.8, 0.8, 1)`, preserving approximately the former silhouette ratio (`2.30` -> `2.25`). | Actual visual-scale change; corridor/footprint unchanged and must be proven in Unity. |

Only the northeast homestead roof and southeast bridge have actual visual-scale reductions in this execution. The other four groups are a measurable validator/catalog baseline, not a claim of completed visual relayout. No physical footprint was adjusted.

## Testing Quality Gate

```text
Changed runtime code:           NO (editor generator/tooling and pure scale contract only)
Changed deterministic logic:    YES
Changed Unity scene/prefab:     NO
Automated tests added/updated:  YES
Automated tests command:        powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\unity\RunUnityEditModeTests.ps1 -TestFilter 'CindarsHope.Tests.EditMode.Farm.FarmSceneCompositionContractTests'
Manual Play Mode scenario:      docs/validation/playmode/spec_farm_scene_keyart_macro_composition_v1_human_test_scenario.md
Justification if no tests:      Focused tests were added; Unity Test Runner cannot start without the configured Unity executable.
Residual risk:                  Unity has not imported the new editor/contract files, regenerated FarmScene, run either validator, captured the views, or executed tests.
```

## Validation

| Command | Exit code | Result |
|---|---:|---|
| `git diff --check` | 0 | PASS. |
| Forbidden API static scan of owned source | 0 | No new runtime global scene search. `FindObjectsByType` appears only in the editor validator, consistent with existing editor validators. |
| `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\docs\run_strict_validation.ps1` | 1 | FAIL due to pre-existing docs governance errors and stale Unity-generated project build; no generated project was edited. |
| `dotnet build .\CindarsHope.Runtime.csproj --no-restore` | 1 | Same known stale generated-project failure: ten `CS0103` references to dependency `FarmSceneSpatialContract` are absent from `CindarsHope.Runtime.csproj`. |
| `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\unity\RunUnityCompileValidation.ps1` | NOT RUN | Configured Unity executable is unavailable locally. |
| Focused EditMode test / menu validators / capture | NOT RUN | Requires the unavailable Unity Editor and regenerated scene. |

Unity validation: NOT RUN
Reason: configured Unity Editor executable is unavailable locally; generated C# projects are stale and are outside scope.
Command attempted: `powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\unity\RunUnityCompileValidation.ps1`
Residual risk: Unity compilation, regeneration, composition output, navigation `10/10`, and all three captures remain unproven locally.

## Non-Regression Review

PASS with residual Unity risk. Scope is contained; no forbidden scene/prefab/asset/project-file edit, save DTO, event subscription, gameplay cross-system call, or runtime global scene-search API was introduced. New balance-like visual dimensions are named contract constants rather than literals scattered through creation helpers. Status is not promoted beyond `PARTIAL`.

## How to Test

Human test scenario: `docs/validation/playmode/spec_farm_scene_keyart_macro_composition_v1_human_test_scenario.md`.

Expected test time: 5 minutes. Tester: human. Status: NOT RUN until Unity validation is available.

## Honest Status Rationale and Remaining Work

The source contract, focused tests, editor validator and human scenario are ready for Unity import. The northeast roof and bridge have actual visual-scale adjustments; the other four groups are catalogued/validated but have not been visually relaid out by this spec. The target validation level is `UNITY_VALIDATED` plus capture, but it cannot be claimed: strict validation exits 1 on known repository/governance and stale-generated-project failures, while Unity itself is absent. Open the project in Unity, regenerate through the canonical menu, run the composition and navigation validators, run the focused EditMode test, produce the three captures, then update this report with real output before promotion.
