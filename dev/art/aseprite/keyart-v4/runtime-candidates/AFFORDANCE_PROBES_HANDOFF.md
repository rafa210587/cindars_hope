# Local physical affordance probes — candidate handoff

2026-09-10. No Assets files changed. Root applies `FarmPhysicalRouteProbe.patch` or uses the complete candidate `FarmPhysicalRouteProbe.cs`. Patch applicability: PASS with `git apply --check --ignore-space-change` (live source contains mixed line endings).

## Checks added

- 24specific collider checks:9crates,6barrels,2troughs,2hay, Bed/SolidBase, FarmHouseChest/SolidBase, both CaveEntrance jamb bases, and NaturalBoundary_TownGate.
- The19settlement names are explicit in `SettlementSolidNames`; static verification confirms each name has a current authoring call. No wildcard acceptance of an arbitrary nearby prop.
- Missing/ambiguous object, missing/multiple non-trigger colliders, wrong collider type, disabled/inactive collider, nonsimulated rigidbody, or exclusion from the actual player's collision mask FAILS.
- The real player body is placed so its existing center offset meets the expected collider. `OverlapCollider` uses the real layer filter; only reference equality with the expected collider satisfies the check. Hit paths and expected instance ID are recorded. A mountain or neighboring prop cannot substitute.
- 4local swept accesses: bed, chest, cave central passage, Town approach. Start and end must be clear and the actual player collider cast must be unblocked. These queries run before the existing exterior BFS; no interior BFS target was added.
- Bed feet(12.7,12.8), chest feet(18.1,13.85). Each requires its exact enabled interaction trigger and closest-point distance<=0.45u. The root's revised2u trigger gives offline distances0.30/0.35u. Full1.125u body height leaves0.155/0.165u before the solid bases.
- Live `FarmTileGrid` is queried for every tile overlapped by each expected solid's bounds, including edge tiles. Any tillable tile is individually recorded as FAIL and prevents overall PASS. Probe never mutates tilling registrations.

## Non-arable gap and minimal resolution

Current `FarmSceneRuntimeBootstrap.Start` registers house, major landmarks, service bodies, fences and natural areas, but has no explicit registration for the19new settlement props. `FarmTileGrid.IsTillable` consults `FarmNonArableZones`, not physical colliders, so adding WorldSolid colliders alone does not change tilling. Several prop tiles may already be covered by broader zones; only the live grid probe establishes which still fail.

Root's minimum correction is to pass the same authored base rectangles to the existing `RegisterBlockedSolidRect` after scene-zone reset. Keep source geometry shared with the composer, or wire explicit collider references during scene generation and register their world bounds. Avoid a global scene scan or blanket blocking of all decorations. Register all touched tiles using the existing min/max conversion; do not mark only the object's center tile. Cave jambs similarly need their shared rectangles registered. Bed/chest already fall within the existing house zone.

Greenhouse priority is unchanged: `FarmNonArableZones.IsBlocked` returns false for registered greenhouse interior tiles. Do not weaken that rule to hide a misplaced prop. If a solid overlaps a deliberately cultivable greenhouse tile, inspect its actual footprint/placement.

## Evidence

`verify-affordance-probes.py`:15static checks PASS, including exact19names, required collider identity/state guards, exterior target preservation, aggregate check counts, per-tile live queries, and offline furniture approach geometry. Output: `affordance-probes-static-report.json`.

These are source/geometry checks, not a C# compile or Unity physics result. The existing Evaluate finally block still restores player position and velocity; no door state, interaction or animation is altered by this slice. Motion remains the separate probe's responsibility.

Unity validation: NOT RUN
Reason: Root owns Unity and requested candidate-only source while Town coordinated the Editor window.
Command attempted: None.
Residual risk: Compile, regenerated collider identities, overlap/cast filtering, actual trigger reach, cave passage and live non-arable tile results require root's integrated run.
