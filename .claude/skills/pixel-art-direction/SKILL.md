---
name: pixel-art-direction
description: Defines pixel art direction from approved art, palette, perspective and native scale. Use before creating sprites or aligning a batch with a scene's style guide.
---

# Skill: Pixel Art Direction

Cindar's Hope combines 2D characters and environments; the target's approved reference guides visual decisions.

**Core rule: distinguish requirements established by approved art from suggestions for the new piece.**

## When to use
- Define the brief for a sprite, batch or modular family.
- Resolve drift in palette, perspective, silhouette or pixel density.

## Essential checklist
- [ ] Actual reference opened and its source recorded.
- [ ] Piece plane, perspective, palette and native size defined for the target.
- [ ] Silhouette readability at game scale and fit with existing pieces considered.
- [ ] New decisions separated from approved constraints.

## Procedure
1. Open the approved art and the style guide section covering the target; do not load all lore.
2. Extract observable features: clusters, outline, contrast, materials, lighting and proportions.
3. Define native resolution and subject occupancy. A large PNG with smoothed pixels does not prove native pixel art.
4. State perspective per component and scale relative to the player or existing module.
5. Deliver a reusable brief with verifiable criteria and remaining questions.

## On-demand references
- For palette, resolution or modular pieces: [direction criteria](references/style-and-perspective.md).
- To turn the brief into a prompt: [pixel-art-prompt-authoring](../pixel-art-prompt-authoring/SKILL.md).
- To judge an image already produced: [visual-asset-review](../visual-asset-review/SKILL.md).

## On-demand materials
- Only when producing a visual brief, copy the [template](assets/templates/visual-brief.md).
- Only if unsure how to fill the brief or choose perspective, consult the [hypothetical example](references/examples/farm-bridge-brief.md).

## When NOT to use
- Import or collider correction only, with art already defined: use `sprite-scene-integration`.
- Timing or gait audit: use `sprite-animation-review`.

## When to stop and report
If the approved reference is inaccessible, label provisional decisions; do not claim visual fidelity.

## Expected output
Brief with target, references, observed requirements, proposed choices and comparison criteria.
