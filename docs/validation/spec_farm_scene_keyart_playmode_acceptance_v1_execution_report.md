---
doc_type: validation
status: evidence
spec_id: spec_farm_scene_keyart_playmode_acceptance_v1
validation_type: playmode_acceptance
result: NOT_RUN
date: 2026-08-14
executor: Codex Unity Validator
source_of_truth: false
validated_adrs: []
validated_game_rules: []
---

# Validation Report — FarmScene Keyart Play Mode Acceptance

> **Overall status: BLOCKED / NOT RUN.** This report is evidence, not an execution queue and not
> an acceptance promotion. The required Unity import, validator execution, regeneration, capture,
> and human Play Mode route have not occurred.

## Scope and predecessor state

This documentation-only closeout creates the final human scenario and this report. It did not
modify source, scenes, assets, metadata, generated project files, the active spec, or
`FARM_KEYART_EXECUTION_TRACE.md`. No commit was created.

The execution trace records the six implementation predecessors as source-reviewed `PARTIAL`:

| Order | Spec | Predecessor status |
|---:|---|---|
| 1 | `spec_farm_scene_spatial_contract_v1` | PARTIAL |
| 2 | `spec_farm_scene_collision_navigation_v1` | PARTIAL |
| 3 | `spec_farm_scene_keyart_macro_composition_v1` | PARTIAL |
| 4 | `spec_farm_scene_organic_terrain_water_v1` | PARTIAL |
| 5 | `spec_farm_scene_landmarks_and_agriculture_v1` | PARTIAL |
| 6 | `spec_farm_scene_biome_decoration_v1` | PARTIAL |

Their reports and `docs/validation/FARM_KEYART_EXECUTION_TRACE.md` consistently state that Unity
was not run and the generated C# projects were stale. This gate preserves that status; it does
not convert source review into a build, validator, capture, or Play Mode pass.

## What was run

| Check | Command / method | Exit code | Result | Evidence |
|---|---|---:|---|---|
| Documentation validation | `powershell -NoProfile -ExecutionPolicy Bypass -File .\\tools\\docs\\validate_docs.ps1` | 1 | FAIL — pre-existing repository governance findings | See findings below. |
| Whitespace/conflict check | `git diff --check` | 0 | PASS | No output. |
| Configured Unity location | `Test-Path 'C:\\Program Files\\Unity\\Hub\\Editor\\6000.4.7f1\\Editor\\Unity.exe'` | 0 | NOT FOUND | Configured path is absent; `UNITY_EDITOR_PATH` is unset. |
| Alternate Unity discovery | List `C:\\Program Files\\Unity\\Hub\\Editor` and test `Unity.exe` | 0 | INFORMATIONAL | `6000.5.7f1` exists, but was not used. |
| Unity process lock check | `Get-Process -Name Unity,UnityHub -ErrorAction SilentlyContinue` | 0 | PASS | No Unity/UnityHub process running. |
| Acceptance-validator source inventory | Existence check of spatial, navigation, composition, terrain, landmarks, decoration validators | 0 | PARTIAL | All six source files exist; their menu outputs were not executed. |
| Runtime global-search scan | `rg` for `GameObject.Find`, `FindObjectOfType`, `FindObjectsOfType`, `FindObjectsByType` in new Farm contracts/policies | 0 | PASS | No match in the reviewed runtime files. Editor validator lookup is outside runtime scope. |
| Capture inventory | PNG metadata/hash check for full, closeup, animal images | 0 | PARTIAL | All three exist at 1600×1200; provenance is not a post-regeneration acceptance session. |

### Documentation-validation triage

`validate_docs.ps1` returned exit code 1. It reports existing repository-wide governance issues
outside this spec’s allowed files, including legacy implemented-spec naming, future-spec markers
and dependency headers, placeholder detections in unrelated architecture/tooling files, and
missing ADR/game-rule fields in unrelated active specs. The same category of legacy/docs
governance failure is recorded by the six predecessor reports.

No clean-baseline run exists in this execution to prove every individual finding predates this
report; therefore this report does **not** claim a docs PASS or attribute the failures to this
spec. They are out of scope for this documentation-only acceptance gate and still block a clean
repository docs-validation result.

## Unity compile and log scan

Unity validation: NOT RUN

Reason: the configured validation executable,
`C:\\Program Files\\Unity\\Hub\\Editor\\6000.4.7f1\\Editor\\Unity.exe`, is absent and
`UNITY_EDITOR_PATH` is unset. An alternate 6000.5.7f1 executable was discovered, but using an
unconfigured editor version could import/regenerate the shared project and is outside this
documentation-only task. The six predecessor reports also document stale generated `.csproj`
files, so a .NET build is not substitute evidence for Unity.

Command intentionally not launched:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\unity\RunUnityCompileValidation.ps1 `
  -UnityEditorPath "C:\Program Files\Unity\Hub\Editor\6000.4.7f1\Editor\Unity.exe" `
  -ProjectPath "." `
  -LogFile ".\Logs\unity-compile-validation.log"
```

Log scan: NOT RUN

Reason: `ScanUnityLogs.ps1` requires a current Unity compile log. No Unity process was launched,
so no current compile log exists for this acceptance pass.

Residual risk: Unity compilation, asset/script import, FarmScene regeneration, all six editor
validator results, Physics2D collision behavior, actual Game View sorting/composition, interaction
prompts, farming restrictions, and Console health remain unvalidated locally.

## Static acceptance evidence

The following facts are source/inventory evidence only:

| Requirement area | Static evidence | Status |
|---|---|---|
| Six automatic gates are implemented | Validator sources exist for spatial contract, navigation, composition, terrain masks, landmarks, and decoration. | PARTIAL — menu output NOT RUN |
| Required success signatures are defined | Source declares `0 error(s)`, navigation `10/10`, composition `6/6`, landmarks `7/7`, and decoration `6/6`/zero forbidden placement messages. | PARTIAL — no Console evidence |
| Three captures exist | `farm_capture.png`, `farm_capture_closeup.png`, and `farm_capture_animals.png` each exist at 1600×1200. | PARTIAL — not proven regenerated/final |
| Runtime architecture scan | Reviewed new Farm contract/policy files contain no global scene-search API. | PASS — static only |
| Worktree conflict check | `git diff --check` succeeds. | PASS — does not validate behavior |

Existing capture SHA-256 inventory, for traceability only:

```text
farm_capture.png          D5271ADE0E4E7061C884DE6AD0BCF6DE3887F1655084C4DBBF910D84078E21EE
farm_capture_closeup.png  F4DC60DC84ACD8220EA7804E66AF797CE1D6E3531F11A29BA0510B8E09E16658
farm_capture_animals.png  017CA45E32AC6376DF42BCAB2C5CCFB6810B662D622833EF914F18DE3291D186
```

## Acceptance criteria status

| Spec criterion | Result | Why it cannot be proven now |
|---|---|---|
| 14.1 — six automatic gates clean | NOT RUN | The six Unity Editor menu validators have source presence only; no real scene or Console output. |
| 14.2 — human physical route items 1–16 all PASS | NOT RUN | No compatible Unity Play Mode session was executed. |
| 14.3 — three final captures and 8/8 visual PASS | NOT RUN | Existing images are not proven post-regeneration final captures; no human visual comparison was recorded. |
| 14.4 — validation truth | PASS | Every unexecuted Unity/Play Mode item is explicitly `NOT RUN` with reason, command, and residual risk. |

Criteria that cannot be proven from source/static checks:

1. A regenerated FarmScene materializes every contract, collider, tile, sprite, and validator input.
2. All six validators emit their required zero-error/complete counts.
3. The bridge is physically passable while lake, river, and mountain block movement.
4. The ten required landmarks and their prompts are reachable from spawn.
5. A player can till, plant, and water a permitted tile and is rejected on water/buildings.
6. Decoration neither obscures interactions nor produces missing/magenta sprites or bad sorting.
7. The final game-camera composition meets all eight visual comparison criteria.
8. The current Unity Console contains no unexpected errors during the complete route.

## Human Play Mode acceptance hand-off

Scenario: `docs/validation/playmode/spec_farm_scene_keyart_playmode_scenario.md`  
Expected test time: 15–20 minutes  
Tester: human  
Status: NOT RUN

The human must use the scenario after Unity/editor-version compatibility is explicitly resolved.
Record all six validator outputs, regenerate/capture the three images, complete route rows 1–16,
and complete the eight visual rows. Any FAIL opens a separate bugfix/spec; no correction is in
scope for this acceptance spec.

## Phase status

| Phase | Status | Date |
|---|---|---|
| Phase 0 — audit/source evidence | COMPLETE | 2026-08-14 |
| Phase 1 — available non-mutating checks | PARTIAL | 2026-08-14 |
| Phase 2 — Unity compile and editor validators | NOT RUN | 2026-08-14 |
| Phase 3 — human Play Mode/capture acceptance | NOT RUN | 2026-08-14 |

## Next action

Provide/approve a compatible Unity Editor path, then regenerate FarmScene through the canonical
menu and execute the human scenario. Re-run Unity compile validation and log scan with real logs.
Only after all six validators, 16 route checks, and eight visual checks are recorded `PASS` may
the FARM KEYART batch be considered for acceptance. Until then this spec remains `NOT RUN` and
the batch remains blocked from promotion.

*Report generated: 2026-08-14*
