# Farm-side entrance candidates — 2026-09-10

Prepared outside Assets while root captured stage10. No live C#, Unity scene, imported art or metadata changed by this slice.

Scope authority: `.specs/a_implementar/spec_farm_enclosed_valley_keyart_v2.md`, final physical-affordance refinement. This is a Farm-side correction; destination IDs, Cave stable-run behavior and Town internals are unchanged.

## Apply order and ownership

1. Copy `FarmEntrancePhysicsContract.cs` to `Assets/_Game/Scripts/Farm/Scene/` through the normal root integration flow.
2. Apply `FarmSettlementVisualComposer.cs.patch.txt`: extract existing renderer to `internal CreateFenceRun(Transform, FarmSolidRect)`. Its geometry, crop assets and sorting behavior remain unchanged.
3. Apply `FarmPerimeterVisualComposer.cs.patch.txt`: retain the existing TownGate PolygonCollider, draw one shared measured fence run, skip the legacy overlapping fence sprites.
4. Apply `CreateMvpFarmScene.cs.patch.txt`: only `CreateScenePortal` and `CreateCaveEntrance` method replacements. Never replace the complete creator file; root owns concurrent house changes.
5. Root registers `FarmEntrancePhysicsContract.CaveSideBases` as non-arable and adds physical/interaction probes. Neither interaction trigger nor `TownGateVisualRun` should become another physical collider.

Patch files use apply_patch syntax. `build-entrance-candidates.cjs` only reads live files and writes the three patch candidates plus normalized baseline hashes here. Re-run only after reviewing any concurrent modifications to these exact methods.

## Measured contract

Existing cave source inspected directly: `Assets/_Game/Art/Generated/World/props/cave_entrance.png`, canvas531×435; alpha bbox(8,8)..(522,426), opaque515×419. Existing bottom-center pivot and6.75u opaque-height presentation retained. World conversion is `(sourceBottomLeft - (265.5,0)) * (6.75/419) + (-22.75,20.1)`.

Stage9 actual640×480 `docs/validation/farm_keyart_v4/gameplay_stage9/mountain.png` inspected. It shows the player below the threshold, ore clusters and exposed jambs without corresponding local physical bases.

| Region | Source pixels, bottom-left | World bounds x / y |
|---|---|---|
| CaveWestBase | x12..200, y43..205 | [-26.83383,-23.80519] / [20.79272,23.40251] |
| CaveEastBase | x337..515, y43..205 | [-21.59815,-18.73061] / [20.79272,23.40251] |
| CaveInteractionTrigger | aperture width, old front retained | [-23.80519,-21.59815] / [18.35,21.95263] |
| Threshold support | (265.5,43) | (-22.75,20.79272) |
| TownGateVisualRun | derived from unchanged boundary endpoints/depth | [36,36.35] / [1.5,4.5] |
| TownInteractionTrigger | explicit old effective1.8×1.8 | [33.6,35.4] / [2.1,3.9] |

Two basal rectangles include the adjoining ore and jamb masses, extending rearward into the permanent mountain band to close any route behind the bases. Small foreground chips/contact shadows below source y43 remain decorative. The central opening is2.20704u wide, leaving a1.56642u feet-center corridor for the measured.640625×1.125 player collider. Cave root, sprite position, scale, `CaveEntranceInteractable` and `cave_from_farm` remain unchanged. Trigger narrows laterally to the opening and preserves the old approach front.

Town keeps `Portal_Farm_To_Town` and all serialized destination/prompt IDs. The root becomes unit scale and uses the explicit1.8×1.8 trigger, preserving its effective previous dimensions. The old generic colored placeholder renderer is removed; the measured fence supplies the visible gate. Existing fence renderer keeps posts upright, rotates only rails, uses native.328125u post width and.164063u rail thickness. This remains an interactable boundary gate, not an animated swinging opening.

## Offline evidence

`node dev/art/aseprite/keyart-v4/runtime-candidates/verify-entrance-geometry.cjs`: PASS. Details in `entrance-geometry-report.json`.

- Recreated the current Catmull-Rom ribbon sampling and full polygon, then clipped it against feet-collider-expanded WellNorth/WellEast rectangles. Both new positions have zero swept-road overlap: North(-4.2,14.3),2.7×.28; East(-1.8,13.1),.28×1.05.
- Central cave approach is clear, both prior lateral penetration examples now intersect a side base, and base rears reach the permanent cliff throughout their widths.
- Current cave road centerline remains clear. The curved road's right shoulder has0.236717u² of feet-center positions intersecting the east base. This is a visible road-width limitation to inspect, not a blocked central route. No path geometry was changed by this slice.
- Existing cave selection position(-22.5,18.5) remains inside its trigger. Town barrier-contact feet(35.6796875,3) are.2796875u from the trigger, below the existing.45u selection limit.

The geometry script mirrors measured candidate values and authored path data. It is offline evidence, not a substitute for compiling the C# or checking actual collision/contact filtering, sprite sorting, scene transitions, and interaction selection in Unity.

Unity validation: NOT RUN
Reason: Root owns Unity and requested candidates outside Assets during stage10 capture.
Command attempted: None; deliberate candidate-only scope.
Residual risk: Unity compile, regenerated scene, route casts and actual transitions remain pending root integration. Check lateral cave contact, center approach from spawn, Town fence alignment/sorting and actual Farm→Town/Cave→Farm behavior.
