# Rule: Keyart Scene Fidelity

**Invariant:** A world scene with approved keyart can only be reported visually ready after a
current capture is compared with the actual reference, with explicit differences and limits.

## Canonical keyart

| Scene | Keyart |
|---|---|
| FarmScene | `docs/art_catalog/_reference_keyart_GPT/farm_enclosed_valley_approved_v2.png` |

Register new keyart when approved. Do not assume Farm-specific criteria apply to Town, Cave or interiors.

## Evidence before reporting
1. Open the canonical PNG rather than validating from memory or description.
2. Capture the result without Gizmos. Materialize the generator when it changed and capture requires
   that output. Never regenerate routinely when it could overwrite concurrent scene work.
3. Compare affected regions with the keyart: shape, color, density, paths, enclosure language and props.
   Include adjacent context when composition or scale changes.
4. For buildings and landmarks, measure visible opaque bounds at the intended camera scale. Record the
   ratio to the player and neighboring structures; transparent canvas size and positive Transform scale
   are not evidence of correct perceived size.
5. Flag a repeated border motif when it reads as an unintended wall. Collision must be explained by the
   visible terrain, but continuous rocks, fences or shrubs require an authored reason consistent with the keyart.
6. Attach the capture and verdict with remaining differences. Never claim equality from anchor positions,
   object counts, import settings or automated tests alone.

FarmScene uses `FarmSceneCapture` in `Assets/_Game/Scripts/Editor/Dev/FarmSceneCapture.cs`.
Review the capture method and effects before running it. `RegenAndCapture` also regenerates; use it only
when regeneration is in scope and preserves existing work. Collider captures help inspect walkability;
an Editor capture does not prove traversal in Play Mode. Run Unity batchmode sequentially with Unity closed,
as required by `unity-assets`.

## Ownership and capability
Distribute implementation and review according to risk, independence and available tools, without binding
the workflow to model aliases. An agent that can open images may review them; otherwise mark visual review
`NOT RUN` and hand artifacts to a capable reviewer. Independent review may use the audit-only
`pixel-art-scene-reviewer`. The closeout owner checks files and evidence under `subagent-results-not-evidence`;
a summary alone is not proof. A reviewer does not implement its own findings during an audit.

## Scope
Generators, art, terrain, layout, spatial composition and wiring that change the appearance of a scene with
approved keyart. Gameplay and collision evidence remain separate from visual acceptance.

## Enforcement
`visual-asset-review`, `sprite-scene-integration`, `non-regression-review` and closeout check the current
comparison capture. Scene-specific validators may enforce approved opaque extents and player ratios, but
automated thresholds never replace image review. Without an accessible image/camera, report the gate and risk
as pending; do not call the scene ready or confuse static review with human gameplay acceptance.
