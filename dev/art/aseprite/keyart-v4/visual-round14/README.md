# Round14 — one bounded visual proposal

Candidate only. Apply `visual-round14.patch`; complete candidate texts and original byte SHA256 values are in `manifest.json`. Read-only `git apply --check --ignore-space-change` PASS. No live Assets changes or Unity execution.

Opened the exact approved `farm_enclosed_valley_approved_v2.png` and actual stage13 `farm_capture_keyart_composition.png`. The reference has broader irregular flower/leaf masses around clearings; stage13 retains tiny disconnected patches and more abrupt eastern road branches. This proposal addresses those local differences only.

## Delta

- Three eastern junctions: gentler entry below the NE field corner; a curved approach from the field-side road toward Town; one extra control before the SE return joins the crop loop. Existing endpoints, lane widths, Ribbon interpolation and all interaction approaches stay unchanged. No central crop-gap road moves.
- Six asymmetric plant groups: west/north sides of the well, northwest/southeast fountain margins, western inner forest and southern inner forest. Two unequal leafy pieces plus four flower accents per candidate group. Existing source sprites, measured alpha/support and `Place` are reused; no new art.
- Heights are authored per piece: main leaves2.15u, secondary leaves1.45u, flowers1.02–1.26u. These are local group sizes, not global resizing. Up to36visual pieces; complete alpha footprints plus0.2u margin are checked against the unchanged planner mask on a0.25u grid, with an additional1.4u well-gate clearance. Rejected pieces remain absent.
- No water, soil logic, furniture, physical tree, perimeter, collider, ID, entrance or Animator edits. The plant pass is called from the existing landscape entry point; existing cliff rendering remains intact.

## Physical-tree preservation reasoning

Only three path ribbons change; all affected Catmull-Rom spans are in the conservative eastern envelope x[5.5,24.5], y[-14,11]. The ten ForestClusterCenters admit candidate feet at radius<4u. Their northern/western disks are entirely west of x=-21; all remaining southern disks end at or below y=-17.5. No possible physical-tree candidate foot intersects the changed road envelope. Therefore those feet see the same path rejection result; the tree sequence, seed, candidate generator and every other tree rejection rule are unchanged.

This is a bounded geometric argument, not a captured68-position comparison. Root should still compare IDs/positions on regeneration as requested; any unexpected change rejects this candidate. `FarmTileGrid` does not consume AuthoredPaths, and all changed road spans stay east of the field's xMax5, so arable registration and the internal crop gap remain unchanged.

## One-round acceptance

1. Same camera: the three named junctions should read as bends rather than abrupt branches; reject if any entrance moves or painted road crosses crops.
2. At least four of the six regions should show a readable irregular plant mass rather than isolated tiny stamps. Check accepted child counts: if masks reject nearly everything, reject this proposal instead of removing safety buffers.
3. Poço/fonte remain visible; the well gate, fountain front and all established approaches stay clear. Check silhouette margins at gameplay scale, not just the1536overview.
4. Compare physical tree IDs/positions and existing collider geometry with the baseline. Run the existing planner regression checks and relevant route/farming checks; no new mirror tests added.

Unity validation: NOT RUN. Root owns integration and the Unity window. Final placement counts, renderer appearance and tree-position comparison remain pending. This source candidate is not visual acceptance.
