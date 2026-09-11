# FarmScene Keyart — Human Play Mode Acceptance Scenario

> **Spec:** `spec_farm_scene_keyart_playmode_acceptance_v1`  
> **Status:** `NOT RUN` — the required Unity validation has not been executed in this environment.  
> **Tester:** Human with a compatible Unity Editor  
> **Estimated time:** 15–20 minutes

## Purpose

This is the final acceptance route for the FARM KEYART batch. It verifies the regenerated
FarmScene in the actual game camera and Physics2D runtime; it is not satisfied by static source
review or an editor capture alone.

The six predecessor specs are source-reviewed `PARTIAL`. Their reports are:

- `docs/validation/spec_farm_scene_spatial_contract_v1_execution_report.md`
- `docs/validation/spec_farm_scene_collision_navigation_v1_execution_report.md`
- `docs/validation/spec_farm_scene_keyart_macro_composition_v1_execution_report.md`
- `docs/validation/spec_farm_scene_organic_terrain_water_v1_execution_report.md`
- `docs/validation/spec_farm_scene_landmarks_and_agriculture_v1_execution_report.md`
- `docs/validation/spec_farm_scene_biome_decoration_v1_execution_report.md`

## Required setup and evidence

1. Use a compatible Unity Editor, let the project import completely, and resolve any compile
   failure before continuing. Do not edit scene YAML manually.
2. Run the canonical FarmScene regeneration command: `CindarsHope/Inicializar Projeto`.
3. Save the Unity Console output for this session. Any unexpected `ERROR` or exception is a FAIL.
4. Before entering Play Mode, run all six validators below and record their exact Console lines:

| Validator menu | Required success evidence |
|---|---|
| `CindarsHope/Validar Layout Espacial FarmScene` | `ValidateFarmSceneSpatialContract: 0 error(s)` |
| `CindarsHope/Validar Navegacao FarmScene` | `Reachable required landmarks: 10/10` and `0 orphan collider(s)` |
| `CindarsHope/Validar Composicao FarmScene` | `6/6 groups valid` and `Scale outliers: 0` |
| `CindarsHope/Validar Mascaras de Terreno FarmScene` | `Water cells outside contract: 0`, `Unringed exposed water cells: 0`, `Legacy rectangle painters referenced: 0`, and `0 error(s)` |
| `CindarsHope/Validar Marcos FarmScene` | `Landmarks valid: 7/7` and `0 error(s)` |
| `CindarsHope/Validar Decoracao FarmScene` | `Forbidden decoration placements: 0` and `Decoration biomes valid: 6/6` |

5. Exit Play Mode, run `CindarsHope/Dev/Capturar FarmScene (PNG)`, then retain the three
   resulting files: `farm_capture.png`, `farm_capture_closeup.png`, and
   `farm_capture_animals.png`. Record their timestamps and the three Console lines beginning
   `[FarmSceneCapture] salvo:`.
6. Enter Play Mode from the regenerated FarmScene on a disposable/new save with a hoe, watering
   can, and seed available through the normal debug/loadout path. Do not use teleports to satisfy
   route reachability.

## Route acceptance — 16 required results

Mark every item with exactly `PASS`, `FAIL`, or `NOT RUN`, plus the tester, date, build/editor
version, and supporting Console/capture evidence. All 16 must be `PASS` for this spec to be
accepted.

| # | Check | Status | Evidence / notes |
|---:|---|---|---|
| 1 | Cross the bridge from each direction without a blocking collider in its corridor. | NOT RUN | |
| 2 | Attempt to enter the lake at a non-bridge edge; Physics2D blocks the player. | NOT RUN | |
| 3 | Attempt to enter the river at a non-bridge edge; Physics2D blocks the player. | NOT RUN | |
| 4 | Attempt to enter the northern mountain/cliff; Physics2D blocks the player. | NOT RUN | |
| 5 | Reach the house/homestead from default spawn and use its normal accessible approach. | NOT RUN | |
| 6 | Reach Fonte de Anya and trigger its normal interaction. | NOT RUN | |
| 7 | Reach the central field from a valid approach without decoration obscuring the route. | NOT RUN | |
| 8 | Reach the crafting yard/stations and confirm the normal prompt is accessible. | NOT RUN | |
| 9 | Reach the shipping bin and confirm its normal prompt is accessible. | NOT RUN | |
| 10 | Reach the sell point and confirm its normal prompt is accessible. | NOT RUN | |
| 11 | Reach the south animal-row release/interact approach without visual props blocking it. | NOT RUN | |
| 12 | Reach the fishing spot/lake edge and confirm its normal interaction is accessible. | NOT RUN | |
| 13 | Reach the cave entrance and confirm its normal approach/prompt is accessible. | NOT RUN | |
| 14 | Reach the town exit and confirm its normal approach/prompt is accessible. | NOT RUN | |
| 15 | On a permitted central-field tile, till, plant one seed, and water it using the existing controls. | NOT RUN | |
| 16 | Attempt tilling on water and on a building footprint; both attempts are rejected without a crash or unexpected Console error. | NOT RUN | |

## Visual comparison — eight required results

Compare the three regenerated captures and Game View against
`docs/art_catalog/_reference_keyart_GPT/farm_keyart_layout_aprovado_v1.png`. Capture evidence is
valid only when produced after the regeneration above; pre-existing PNGs are reference baseline,
not acceptance proof.

| # | Visual criterion | Status | Evidence / notes |
|---:|---|---|---|
| 1 | The six macro masses read clearly: north cliff, west forest, central agriculture, northeast homestead, south animal row, southeast lake. | NOT RUN | |
| 2 | Homestead/bridge/trees show no scale outlier and read coherently relative to the player camera. | NOT RUN | |
| 3 | Terrain/path shapes have no dominant rectangular artifact or unintended terrain overlap. | NOT RUN | |
| 4 | Lake and river read as water, preserve the bridge corridor, and have no magenta/missing tile. | NOT RUN | |
| 5 | Central cultivation reads as a usable field; rows are visible but do not replace or cover usable tiles. | NOT RUN | |
| 6 | West forest and north cliff create the intended boundary mass without concealing required approaches. | NOT RUN | |
| 7 | Homestead, animal pens, cave, fountain, crafting and shipping landmarks are visually legible from Game View. | NOT RUN | |
| 8 | Decoration density is coherent, non-obstructive, and contains no placeholder/magenta sprite. | NOT RUN | |

## Console and failure policy

- Expected: the six validator outputs and the three capture success lines listed above.
- Forbidden: any unexpected Console `ERROR`, exception, missing-root/approach error, validator
  non-zero count, collider orphan, or failed route/visual row.
- A single failed validator, route item, or visual criterion is `FAIL`; open a separate bugfix/spec.
  Do not repair it inside this acceptance spec.
- If Unity cannot run, retain `NOT RUN` and complete the blocker/evidence fields in the execution
  report. `NOT RUN` is never equivalent to `PASS`.

## Final human sign-off

```text
Unity Editor version/build:
Tester:
Date:
All six validators PASS: YES / NO
Route 1–16 all PASS: YES / NO
Visual comparison 8/8 PASS: YES / NO
Unexpected Console errors: NONE / LIST
Overall: PASS / FAIL / NOT RUN
```
