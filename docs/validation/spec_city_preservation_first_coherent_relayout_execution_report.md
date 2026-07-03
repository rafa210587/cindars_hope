---
doc_type: validation
status: evidence
spec_id: spec_city_preservation_first_coherent_relayout
validation_type: automated_and_deferred_human
result: BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE
date: 2026-07-01
executor: Codex
source_of_truth: false
validated_adrs: [ADR-0011]
validated_game_rules: [city_rules.md]
---

# Execution Report — Cidade Coerente Preservation-First

> This report is evidence, not an execution queue.

## Revisão v8 — 2026-07-01

O código-fonte do gerador foi atualizado para a planta v8 aprovada: `120×90`, portão/portal sul,
zonas cívica/comercial/ofícios/água/residencial ampliadas, 24 prédios preservados e reinterpretados,
28 NPCs realocados por papel, 497 árvores de perímetro e placeholders semânticos substituíveis.

O build C# de `Assembly-CSharp-Editor.csproj` passou sem erros. A `TownScene.unity` foi regenerada em
batchmode após o ajuste de circulação: telhados tiled contidos no footprint, avenida principal de 7
tiles, vias secundárias de 4 tiles, bancas afastadas das portas e seis residências 8×7 com interior
6×5 contendo cama, cozinha, mesa e armazenamento. EditMode e validator passaram; resta Play Mode humano.

## Summary

TownScene reorganizada por uma fonte determinística de 24 lotes e 10 segmentos viários. A entrega
preserva casas, portas, telhados, 29 barracas, 497 árvores, 28 NPCs, 84 anchors, spawns e portais;
reconcilia trabalho/social/casa por papel; adiciona portas N/S/E/W, identidade por arquétipo,
collider de tronco e lago bloqueado com cais.

## Dependency Chain

```text
Original target: spec_city_preservation_first_coherent_relayout
Dependency chain: current TownScene + implemented fable_40 contracts
Forbidden dependencies: none
Resolved depth: 0
Can continue original target: YES
```

## Existing Systems Audit

Searched: town layout/lot/navigation types, `CreateMvpTownScene`, `NpcScheduleAnchor`,
`NpcScheduleBlockResolver`, `HouseDoorInteractable`, `RoofRevealController`, `WorldSpriteLibrary`,
`WorldTilemapGround`.

Found and reused: scene generator, walk-in interiors, physical doors, roof reveal, NPC schedule
service, three-anchor model, waypoint movement/fallback, world sprites and tilemap ground.

Created new: `TownCityLayout`, pure editorial geometry/role contract. No runtime manager or parallel system.

## Acceptance criteria extracted

```text
Preserve every baseline object family and stable ID.
Keep 24 walk-in buildings, 29 stalls, 497 trees and 84 schedule anchors.
Remove building/building and building/road overlaps.
Connect every building entrance to the road graph.
Keep temple, cemetery, Chamber, Town Hall, market, lake and west access.
Align NPC work/social/home destinations with role and schedule window.
Retain schedule service, waypoint movement, fallback, doors and roof reveal.
Require automated geometry/regression tests and final human Play Mode.
```

## Spec Compliance Matrix

| Criterion | Status | Evidence |
|---|---|---|
| baseline counts and IDs preserved | PASS | scene count audit + validator |
| 24 deterministic lots without overlap | PASS | `TownLayoutTests` |
| every door reaches a road | PASS | `BuildingLots_DoNotOverlapRoads_AndEveryDoorReachesRoad` |
| N/S/E/W door support | PASS | `DoorFrontages_SupportAllFourDirections` |
| 28 NPC roles and destinations | PASS | `NpcPlaces_PreserveAllRoles_AndStayNearAssociatedBuildings` |
| existing schedule windows preserved | PASS | resolver tests; only `Stationary` classification corrected |
| landmarks retained | PASS | validator landmark check |
| tree visuals retained/colliders tightened | PASS | 497 trees, exactly 8 internal colliders |
| final visual/feel validation | DEFERRED | human scenario NOT RUN |

## Files changed

```text
Assets/_Game/Scenes/TownScene.unity
Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs
Assets/_Game/Scripts/Editor/SceneCreation/TownDistrictLayout.cs
Assets/_Game/Scripts/Editor/SceneCreation/City/TownCityLayout.cs
Assets/_Game/Scripts/Editor/Validation/ValidateFableCitySchedule.cs
Assets/_Game/Scripts/NPC/Schedule/NpcScheduleBlockResolver.cs
Assets/_Game/Tests/EditMode/City/Editor/TownLayoutTests.cs
Assets/_Game/Tests/EditMode/City/NpcScheduleBlocksTests.cs
Assets/_Game/Art/Generated/World/_TileAssets/* (Unity-generated Tile assets)
docs/validation/spec_city_preservation_first_baseline_manifest.md
docs/validation/spec_city_preservation_first_coherent_relayout_non_regression_review.md
docs/validation/playmode/spec_city_preservation_first_coherent_relayout_human_test_scenario.md
```

Forbidden files touched by this execution: NO.

## Results

| Check | Result | Evidence |
|---|---|---|
| TownScene generation | PASS, exit 0 | `Logs/town-preservation-generation.log` |
| Runtime build | PASS, exit 0, 0 errors | `Logs/town-runtime-build.log` |
| Editor build | PASS, exit 0, 0 errors | `Logs/town-editor-build.log` |
| Town EditMode tests | PASS, 22/22 | `Logs/town-circulation-editmode-results.xml` |
| City schedule/scene validator | PASS, 21/21 | `Logs/town-circulation-validator.log` |
| Preservation manifest diff | PASS | 24 houses, 29 stalls, 497 trees, 24 doors/roofs, 84 anchors |
| Non-regression review | PASS_WITH_DEFERRED | linked review report |
| Docs validation | EXPECTED_FAIL_PREEXISTING | unrelated NPC physics companion spec + dirty harness placeholder scan |
| Strict validation | builds PASS; global result initially blocked because this report did not exist | `Logs/town-strict-validation.log` |
| Human Play Mode | NOT RUN | deferred scenario linked below |

## Warnings

```text
Runtime: 1 pre-existing CS0649 warning in CombatTelemetrySession.
Editor: 3 pre-existing warnings in enemy taxonomy/CSharpProjectPostprocessor.
Docs: unrelated active spec format debt and harness placeholder false positives/preexisting changes.
Unity compile wrapper returned non-zero although Unity log ended return code 0; direct builds,
Unity generation, EditMode tests and validator provide compile evidence.
```

## Validation

```powershell
dotnet build .\Assembly-CSharp.csproj
dotnet build .\Assembly-CSharp-Editor.csproj
.\tools\docs\validate_docs.ps1
.\tools\docs\run_strict_validation.ps1
```

`run_strict_validation.ps1` recompilou ambas as assemblies com sucesso. Seu resultado global
permanece FAIL porque `check_spec_quality.ps1` proíbe qualquer `.unity`/`.asset` modificado sem
consultar a autorização da spec; nesta execução a TownScene e os Tile assets são outputs Unity
explicitamente autorizados. Também permanecem débitos documentais preexistentes fora do escopo.

## Testing Quality Gate

```text
Changed deterministic logic: YES
Requires EditMode tests: YES — PASS 22/22
Requires PlayMode automated or final human scenario: YES — scenario documented, NOT RUN
Requires regression test: YES — preservation/count/geometry/schedule tests PASS
Human validation timing: DEFERRED_TO_FINAL_VALIDATION
Status cap: BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE
```

## How to Test

Human scenario:
`docs/validation/playmode/spec_city_preservation_first_coherent_relayout_human_test_scenario.md`

Expected time: 15–20 minutes. Tester: human. Status: NOT RUN.

## Phase Status

| Phase | Status |
|---|---|
| Phase 0 — baseline/reuse audit | COMPLETE |
| Phase 1 — implementation/tests | PASS |
| Phase 2 — generation/Unity validators | PASS |
| Phase 3 — human Play Mode | DEFERRED |

## Final Status

`BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE`

No ACCEPTED or PLAYMODE_VALIDATED claim is made.

## Honest status rationale

Build, testes, geração e validator comprovam a estrutura e a preservação. Eles não comprovam o feel
visual, o clearance real do player nem um ciclo diário completo em Play Mode. Por isso o status fica
em `BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE`, apesar de toda evidência automatizada específica passar.
