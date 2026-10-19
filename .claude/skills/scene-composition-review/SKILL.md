---
name: scene-composition-review
description: Review integrated pixel-art scenes for perspective, room boundaries, character-relative scale, pixel density and occlusion before visual acceptance or replication. Use for scene/interior critique, not isolated sprite import checks.
---

# Skill: Scene composition review

**Core rule: reject a visibly incoherent composition even when dimensions, imports and physics tests pass.**
This is an audit, not authorization to edit art, regenerate scenes or start paid image generation.

## Evidence before judgment

- Generate only one comparative evidence capture for a scene with approved keyart; overwrite
  it on rerun in the stable spec folder. Otherwise inspect existing images or live scene state.
  Open the exact latest result and approved reference when available. Identify revision,
  camera framing/resolution and whether each image is a concept, mockup or actual Unity render.
  Link runtime captures to their scene/revision through capture metadata, input hashes or another
  verifiable identifier; a filename ending in "final" is not evidence of freshness.
- Review the whole room at gameplay scale first, then inspect localized details at native pixels.
  Do not use a close-up or a more flattering camera to replace the gameplay-scale comparison.
- If reference art is unavailable, judge observable coherence but mark reference fidelity NOT RUN.
  Missing reference does not excuse visible defects. Without an inspectable image, visual review is NOT RUN.

## Composition checks

1. **Spatial construction:** trace the floor perimeter, wall bases, corners, doorway and front cut.
   They must describe one coherent room. Explain intentional exposed foundations/porches; do not
   silently treat flooring outside the boundary as walkable interior.
2. **Perspective:** distinguish wall height from thickness and projected floor depth. For a requested
   rear-wall/side-trim/front-cutaway interior, the rear face joins the side returns, sides end at the
   intended front boundary, and the foreground does not become a tall barrier. Thin lines crossing
   the floor or extending past the room are not a convincing wall merely because their width is correct.
   This convention is conditional on the art direction, not a universal rule for all scenes.
3. **Relative scale:** use the final in-scene actor profile and opaque silhouette, not canvas size,
   initial generator scale or a remembered screenshot estimate. Compare doors, seats, desks, beds
   and storage together. A projected bed length and an upright character height are different axes;
   one numeric ratio cannot prove proportion. Derive ranges from the chosen perspective/reference,
   never universalize the GateKeeper pilot's values. Measure idle/walk differences when they matter.
4. **Pixel language:** compare occupied pixels per world unit and apparent pixel clusters at the
   same camera. Uniform transform scale and Point filtering do not make mismatched source resolutions
   stylistically consistent. Check excessive tiling, stretched textures, contrast and lighting direction.
5. **Composition and function:** assess usable negative space, furniture dominance, readable entrance
   and room purpose. A cupboard with dishes is not archival storage or guard equipment just because
   a misleading filename/catalog says so. Label provisional art rather than approving its identity.
6. **Occlusion:** inspect the actor at relevant front/behind/side positions and the doorway. No head
   clipping, floating props or contradictory depth order. A central screenshot or sorting enum only
   proves that pose/configuration, not movement across a prop. Use runtime evidence for dynamic claims.

## Rejection and handoff

- A visible contradiction in boundaries, perspective, intended scale, pixel language or critical
  actor visibility is FAIL for the affected visual criterion. Do not average it away with good physics.
- A scene is not ready to replicate while mandatory visual criteria fail or remain unobserved.
  User approval is separate: an agent cannot assign human acceptance. An explicit user rejection
  remains unresolved until a revised candidate is reviewed; repeating tests does not supersede it.
- Do not invent similarity percentages. If a supplied rubric requires a score, retain its weights,
  per-criterion evidence and uncertainty; the score never overrides a blocking defect.
- For iterative corrections follow [bounded visual iteration](../../../.claude/rules/visual-iteration-budget.md).
  Choose up to three visible discrepancies and a coherent correction. Request a static layout/pilot
  only when composition is unresolved; do not require image generation for every placement change.

## Output

Give a visual verdict (PASS / FAIL / NOT RUN, with scope), inspected image/reference paths, localized
findings and observable acceptance criteria for the next correction. Separate observations from
inferences and missing evidence. Keep physics and human acceptance in separate fields.
For physical accessibility claims use [scene-physics-review](../scene-physics-review/SKILL.md);
for isolated alpha/import defects use [visual-asset-review](../visual-asset-review/SKILL.md).
