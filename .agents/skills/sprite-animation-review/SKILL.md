---
name: sprite-animation-review
description: Reviews sprite cycles and actions through key poses, contact, timing, directions and continuity. Use when a sheet seems to slide, jitter, repeat poses or lose identity during animation.
---

# Skill: Sprite Animation Review

Generated sheets can have correct cells without producing the intended action.

**Core rule: a static strip proves poses and crops; motion requires playback with known order and timing.**

## When to use
- Review walking, idle, attacks or animated transitions.
- Distinguish missing motion from crop/pivot jitter or direction errors.

## Essential checklist
- [ ] Action, frame order and intended timing known.
- [ ] Key poses, contact and support evaluated; identity preserved.
- [ ] Intentional duplication separated from lack of progression.
- [ ] Direction strips and playback classified as distinct evidence.
- [ ] Loop, mirroring and transitions evaluated when applicable.

## Procedure
1. Open the exact base and sheet; identify layout in the consumer or target contract.
2. Examine every used direction, including mirrored ones, using [motion criteria](references/motion-diagnostics.md).
3. Watch playback with actual order and durations at intended speed; slow motion can aid diagnosis.
4. Classify drawing, registration, timing or wiring defects before suggesting regeneration.
5. Report frame analysis, external playback and in-game observation separately.

## On-demand references
- For the specific NPC 5x5 contract: [npc-walk-animation](../npc-walk-animation/SKILL.md).
- For alpha/fidelity: [visual-asset-review](../visual-asset-review/SKILL.md).
- For import, pivot and physical scale: [sprite-scene-integration](../sprite-scene-integration/SKILL.md).

## On-demand materials
- Only when producing an animation report, copy the [template](assets/templates/animation-review.md).
- Only if unsure how to distinguish holds, jitter and weapon arcs while filling it, consult the [hypothetical example](references/examples/walk-and-attack-review.md).

## When NOT to use
- Static sprite without an action or sequence: use `visual-asset-review`.

## When to stop and report
If playback is inaccessible, deliver static analysis and mark motion/timing `NOT RUN`.
Do not infer fluidity from a contact sheet or impose a universal frame count.

## Expected output
Affected file/direction/frames, evidence, diagnosis and smallest suggested correction, with unobserved gates.
