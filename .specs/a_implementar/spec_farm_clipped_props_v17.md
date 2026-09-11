# Farm v17 — clipped prop silhouettes

Status: INTEGRATED_VISUAL_REVIEWED; user authorized boat repair and inspection of other clipped components.

## Spec
Repair the boat stern extracted while occluded by the source dock. Preserve identity, canvas76x68, support(35,24), world placement, scale, IDs and collision. Inspect other Farm props for source truncation versus intentional scene occlusion; report concrete findings, avoid speculative reconstruction.
Allowed: dev/art/aseprite/keyart-v4/boat-v17/**, new Assets/_Game/Art/Generated/World/props/boat_keyart_v17.png and import metadata, boat sprite selection in CreateMvpFarmScene.cs, generated FarmScene, docs/validation/farm_keyart_v4/boat_v17/** and latest delivery/status docs. Original boat remains intact. Additional repairs require adding exact measured scope here first.
Acceptance: continuous closed stern/hull silhouette, transparent background with margins, unchanged support/canvas; source hash unchanged; independent visual review at native/game scale; regenerated Unity capture shows complete boat. No global visual PASS.

## Plan
Use aseprite-authoring for layered candidate with baseline and stern restoration layer. Use visual-asset-review to compare source and live captures. Integrate only sprite selection via existing WorldSpriteLibrary.Prop; preserve transforms and collider. Verify Point/None/no-mips and capture Farm with same camera. No gameplay tests for sprite-only change; reuse v16 physical evidence because geometry unchanged.

## Tasks
- [x] T1 Audit boat and other extracted silhouettes.
- [x] T2 Author layered stern restoration; inspect alpha/support and review candidate.
- [x] T3 Integrate approved candidate, generate scene/capture serially under Editor+Assets lock.
- [x] T4 Independent visual review, HTML before/after and report residuals.


