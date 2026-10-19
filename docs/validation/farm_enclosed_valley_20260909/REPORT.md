# FarmScene — enclosed valley v2

Status: CODE_COMPLETE / SCOPED_PASS; DEFERRED_TO_FINAL_HUMAN_VALIDATION. Promotion: NO; visual acceptance and a player-controlled walkthrough remain pending.

Spec: [enclosed valley v2](../../../.specs/a_implementar/spec_farm_enclosed_valley_keyart_v2.md). The user's approved direction replaces the open outskirts, while preserving its historical evidence. The user explicitly chose **an empty initial farm: crops only after planting**.

## Implemented result

- Expanded the playable envelope from 64×44 to 72×50 world units (about 28% more envelope area; the irregular walkable clearing is smaller than that rectangle).
- Rebuilt the perimeter with dense forest, rocks and a northern escarpment. Twenty-eight contiguous polygon bands replace the four rectangular perimeter walls and former mountain block. Their geometry is shared with the visual layout. The east transition corridor has a visible shallow closure beyond the reachable town portal.
- Repositioned the cave, fountain, orchard, pasture, two central fields, farmhouse, greenhouse, four southern buildings and lake/dock around the approved composition. Cleared northern tree masses and the southern building frontage; prevented tree canopies from hiding the east gate.
- Added base collision to coop/barn and three greenhouse sides, retaining entrance approaches. Removed rocks mistakenly rendered at the internal river/lake join.
- Preserved world coordinates of existing farm tile IDs while enlarging the grid; rebuilt non-arable masks, including fractional building edges. This preserves tile addressing, not a complete migration of old cultivation around moved buildings.
- Used existing animal art as decoration. Generated animal candidates were rejected and not imported. No free livestock economy, crop renderer or initial crops were added.

![Unity scene, matched overview framing](diagnostic_final/farm_capture_keyart_composition.png)

Reference: [approved image](../../art_catalog/_reference_keyart_GPT/farm_enclosed_valley_approved_v2.png). Overview is 1536×1024, orthographic size 28: 84×56 world units. It is an Editor camera capture of the generated scene, not the gameplay camera.

## Evidence verified by the orchestrator

Unity 6000.5.7f1, Windows. Executable `C:/Program Files/Unity/Hub/Editor/6000.5.7f1/Editor/Unity.exe`; project `D:/Projetos/Cindars_Hope/cindars_hope`.

| Gate | Result and evidence |
|---|---|
| Unity generation/compile | PASS, process exit 0. `-batchmode -quit -executeMethod CindarsHope.Editor.Dev.FarmSceneCapture.RegenAndCapture`; [final log](regen_final.log). Output configured with `CINDARS_FARM_CAPTURE_OUTPUT`. |
| Scoped EditMode | PASS, **36/36**, zero failures: [XML](tests_03.xml), [log](tests_03.log). |
| Final PlayMode capture | PASS, process exit 0, **18 views**, zero captured runtime errors: [metadata](gameplay_final/capture-metadata.json), [log](gameplay_final.log). `-batchmode -executeMethod CindarsHope.Editor.Dev.FarmSceneCapture.CaptureFarmGameplayBatch`, without `-quit`; output configured with `CINDARS_FARM_GAMEPLAY_OUTPUT`. |
| Physical access | PASS, **19/19 routes**, including orchard/pasture gates and the actual town portal; **480/480** perimeter samples blocked by natural-boundary colliders; **3/3** dock-water samples blocked. |
| House planting mask | PASS, **42/42** sampled tile centers within the house footprint are non-tillable in the live grid. |
| Live interaction selection | PASS, **7/7**: house, workbench, forge, cooking station, cave entrance, fishing and town portal. |
| Final visual review | Orchestrator inspected overview and gameplay north/south/east/house captures. Independent audit found no new P1/P2 visual issue in this scope; remaining differences listed below. |
| Identity/inventory | [Static comparison](IDENTITY_INVENTORY_REVIEW.md): zero ID or registry-membership differences; 263 MonoBehaviours unchanged, physical components 171→199, SpriteRenderers 1470→1902. Zrix retained; 71 TreeNodes in SceneRuntimeReferences, of which 68 are in TreeRegistry. |
| Documentation | Global **FAIL**, exit 1, 59 diagnostics: [log](docs-validation.log), same error lines as the previous outskirts report. No GLOBAL_PASS claim. Spec index regenerated (444 entries). |

EditMode command: `./tools/unity/RunUnityEditModeTests.ps1 -TestFilter 'FarmEnclosedValleyBoundaryTests;FarmLevel1LayoutContractTests;FarmSceneSpatialContractTests;FarmSceneNavigationContractTests;FarmDecorationPlannerTests' -ResultsPath docs/validation/farm_enclosed_valley_20260909/tests_03.xml -LogFile docs/validation/farm_enclosed_valley_20260909/tests_03.log -TimeoutSeconds 300`.

Earlier failed iterations are retained. They exposed an out-of-bounds mountain footprint, displaced biome budgets and obsolete spatial assertions. Corrections preserved biome ceilings, deterministic tree count, real canopy clearance and pairwise spacing checks; the final suite passed. Tests cover old tile identity through expansion, contiguous collision bands, gate placement, region bounds and decoration constraints. A separate compile-only invocation and unrelated full suite were not repeated: the bootstrap change is confined to farm bounds/masks, with scoped tests and live scene checks above; runtime installer architecture and assembly wiring were not changed.

Final scene SHA256: `c6e2af6e04b449047ad47e7c3f745fc1ad0531cd660c53a0c293c0444f30ff77`, equal before/after PlayMode. Player save directory was isolated from save/load calls; before/after hash lists were both empty. [Pre-change scene backup](FarmScene.before.unity.backup) preserves this task's baseline. Sources and relevant evidence are recorded in `input-hashes.json` at closeout.

The perimeter subtree has 936 SpriteRenderers and 28 polygon colliders, with no MonoBehaviour. This is a measured static inventory, not a frame-time benchmark. SceneRuntimeReferences has reordered arrays and a legacy crafting alias changed from forge to cooking station; the corresponding rebinding hook is a no-op. Membership and station IDs remain unchanged, with all three live station selections verified. See the [machine-readable comparison](identity-inventory-comparison.json) and [reproducible method](identity-inventory-method.json).

## What these checks do not establish

Physical routes use actual player-collider overlaps and sweeps through a 0.5-unit BFS graph, followed by exact endpoint sweeps for fishing and town. Boundary checks sample all segments, including endpoints. They are physics queries, not keyboard movement or animation tests. Selection uses the live interaction system after physics steps; it does not invoke crafting, fishing, doors or scene transitions.

Gameplay captures are **640×480, 4:3** in this batch environment; boundary Editor diagnostics are 1600×900, 16:9. Do not infer a 16:9 gameplay run, HUD validation, animation fluidity, performance profiling or human acceptance from these captures.

The composition is recognizable but **not pixel-for-pixel identical**: cliff is more uniform, northern clearing larger, paths straighter/wider, fences simpler and fountain/well/workstations less prominent. Empty crops are an approved intentional difference. Existing decorative animals do not introduce simulation. See [visual review and walkthrough](VISUAL_REVIEW.md).

Residual risks: old saved cultivation may now overlap relocated landmarks despite stable tile coordinates; loading a real progressed save was NOT RUN. Existing crop visual integration was not repaired by this scene task, so post-plant appearance is not certified. Zrix dialogue and unrelated global wiring/test/docs debt are not reclassified as passing.

## Human scenario — NOT RUN

1. Start a new game: no pre-planted crops or free crop rewards. Check scale with the farmer at the house, orchard, barn and bridge.
2. Walk continuously along north, west, south and east boundaries, including joins and the gate. Forest/rock bases must explain the stops; no escape from the clearing or unexplained wall across an open approach.
3. Reach orchard/pasture, cross the bridge, interact with the town exit and return. Enter/leave house and cave exterior; test fishing from the dock. Verify actual transitions and camera behavior.
4. Try tilling beside the fractional house edges and in greenhouse entrances. Plant a seed on valid land and assess the existing crop visual pipeline separately; do not accept mere prepared-soil art as growing crops.
5. Find Zrix and retained resource/crafting points. Check movement and sorting behind tall sprites, and review north/south/east/west at the normal display aspect.
6. On a backed-up progressed save, confirm tile state stays at original world coordinates and inspect any overlap with moved landmarks before accepting save compatibility for this new layout.

No commits, push, manual scene YAML edits or changes to Cave procedural behavior were made for this task. Subagents owned limits and interior composition; an independent reviewer audited the result. Root integrated once sources were synchronized and verified actual logs, XML, metadata and images.

## Proportion and enclosure refinement

Human review correctly identified that the southern buildings read too small and that a continuous low rock
chain beneath the northern forest/cliff looked like an unexplained constructed wall.

- Building calibration now uses opaque pixel bounds instead of the source canvas. Visible target heights changed
  from4.5→5.1world units for the coop,6.2→7.0 for the barn and4.9→5.5 for both processing sheds. Their gameplay
  roots, stable IDs and interaction triggers were retained. Coop/barn solid bases widened proportionally. The main
  farmhouse and greenhouse retained their approved dimensions after same-camera inspection.
- The northern accent chain was removed because the cliff face already explains collision. Other forest edges use
  sparser, mostly undergrowth accents with occasional rocks; the physical28-band boundary is unchanged. The orchard
  now has sparse side posts so its fence reads as an intentional permeable orchard enclosure, separate from the
  permanent world boundary.
- The canonical `keyart-scene-fidelity` rule now points to the approved enclosed-valley v2 reference and requires
  same-camera comparison, opaque-size/player ratios and review of unintended repeated wall motifs. Farm composition
  validation enforces the authored visible extents and rejects a miniature south row; automated thresholds still do
  not replace image review.

Evidence: [final same-framing overview](proportion_refinement_final/farm_capture_keyart_composition.png),
[southern row at gameplay scale](proportion_gameplay_final/animals.png), [final regeneration log](proportion_regen_final.log),
[19/19 focused EditMode tests](proportion_tests_final_02.xml) and [PlayMode metadata](proportion_gameplay_final/capture-metadata.json).
PlayMode final:18views,19/19physical routes,480/480boundary samples,7/7live selections and0captured runtime errors;
scene hash `ea12cecf0fcd14ebf3bcb600efbd58ceb9228cc12fb4663d4ec1caed74b1c6c2` remained stable before/after.
Two earlier focused runs remain as honest failed evidence: the first exposed a missing namespace qualification; the
second exposed two stale composition anchors. Both were corrected before the passing run.

## Compact southern nucleus refinement

The remaining small-building impression came from spacing rather than sprite scale. The four southern centers now
span 15.2 world units instead of 18.79, with every center gap at or below 5.3. The sprites keep their approved opaque
heights. Three separate pairs of courtyard fence fragments were replaced by two end posts and a shared work frontage
with hay, crates and a trough; these details have no collider. The frontage road width changed from 1.6 to 1.3 world
units and its east turn was redrawn around the compact row. Door faces remain visually clear.

Evidence: [same-framing overview](compact_cluster/farm_capture_keyart_composition.png),
[south row with farmer](compact_cluster_gameplay/animals.png), [regeneration log](compact_cluster_regen.log),
[47/47 focused EditMode tests](compact_cluster_tests_final.xml), and
[PlayMode metadata](compact_cluster_gameplay/capture-metadata.json). PlayMode: 18 views, 19/19 routes,
480/480 natural-boundary samples, 3/3 dock-water checks, 42/42 house tilling-mask checks, 7/7 live selections,
zero captured runtime errors, and a stable scene hash before/after Play Mode.

## Keyart delta, path geometry and landmark physics refinement

The approved keyart was compared again against the same-framing scene capture and normal gameplay framing. The
farmhouse was the clearest proportional mismatch: its target visual width changed from 8.5 to 10.25 world units and
its walk-in footprint from 7x6 to 8.5x6.75. The resulting house now reads as the primary residence beside the
greenhouse and remains correctly scaled against the farmer. The physical tilling mask follows the larger footprint.

Path centerlines were redrawn with intermediate bends and narrower authored widths: 1.3 for primary routes, 1.1 for
secondary routes, 0.9 for short spurs and 1.45 for the town route. This removes the previous oversized rectangular
ribbons and makes the circulation closer to the keyart. The existing dirt tile still limits edge variety: junctions
remain more geometric than the painted reference. A future pixel-art pass should add compatible edge, corner and
wear variants rather than enlarging the roads again.

Physical affordances are now explicit and match the visible scene. Ten continuous orchard/pasture fence segments
block movement while their two authored gates remain open. Coop, barn, both processing sheds, farmhouse and
greenhouse have blocked structural bases with clear front approaches. The bridge deck, water, dock and natural
perimeter are also probed with the real player collider.

Evidence: [same-framing overview](keyart_delta/farm_capture_keyart_composition.png),
[farmhouse at gameplay scale](keyart_delta_gameplay_02/homestead.png),
[fields and revised paths](keyart_delta_gameplay_02/cultivation.png),
[orchard gate](keyart_delta_gameplay_02/orchard.png), and
[southern buildings](keyart_delta_gameplay_02/animals.png).

- Focused EditMode: PASS, **48/48**, [XML](keyart_delta_tests_02.xml) and [log](keyart_delta_tests_02.log).
- Scene regeneration/compile: PASS, [log](keyart_delta_regen.log).
- Final PlayMode: PASS, [metadata](keyart_delta_gameplay_02/capture-metadata.json) and
  [log](keyart_delta_gameplay_02/keyart_delta_gameplay.log): 18 views, 19/19 physical routes, 480/480 natural
  boundary samples, 27/27 landmark physics checks, 3/3 dock-water checks, 63/63 farmhouse tilling-mask checks,
  7/7 live interaction selections and zero captured runtime errors.
- Scene SHA256 `eef304db77089f2492150eb7566bac2eec66b386d255360122efeb04bbbe2a1e` remained stable before/after Play Mode.

The first PlayMode attempt is retained in `keyart_delta_gameplay/`: 25/27 landmark checks passed. Its two failed
coordinates sampled the edge of the farmhouse base and bridge bank rather than the clear player line. Both probe
positions were aligned with the already-passing live routes, after which all 27 landmark checks passed. This is a
validation-coordinate correction; no collision was removed to obtain the pass.
