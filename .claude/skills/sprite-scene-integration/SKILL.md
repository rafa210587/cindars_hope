---
name: sprite-scene-integration
description: Integrates sprites into scenes by checking alpha, PPU, pivot, camera, footprint and interaction. Use when importing approved art or correcting scale, support, sorting and fit in the Unity world.
---

# Skill: Sprite Scene Integration

Art must work with the camera and existing gameplay contracts in Cindar's Hope.

**Core rule: check visual size and physical footprint separately before accepting integration.**

## When to use
- Import or replace an approved sprite in a scene/prefab.
- Diagnose floating objects, inconsistent scale, sorting or misaligned interaction.

## Essential checklist
- [ ] Actual alpha; Point, Compression None and Generate Mip Maps false.
- [ ] PPU, pivot and scale relative to the target known.
- [ ] Camera, sorting, support and collision footprint verified.
- [ ] Interaction anchor and stable ID preserved where present.
- [ ] Visual evidence separated from import/build and observed gameplay.

## Procedure
1. Open the approved art and current scene state; identify the existing importer/creator before editing.
2. Check [measurements and fit](references/scene-fit.md) for the target.
3. Wire through the Editor API and canonical commands; do not edit YAML manually.
4. Compare before/after framing with the same camera and observe affected behavior.
5. Record validation performed and pending items without automatically granting human approval.

## On-demand references
- To run generators: [unity-asset-generation](../unity-asset-generation/SKILL.md).
- For a new interactive object: [scene-interactable-wiring](../scene-interactable-wiring/SKILL.md).
- For ground/Tilemap or world composition: [tilemap-world-rendering](../tilemap-world-rendering/SKILL.md).
- To reject art before import: [visual-asset-review](../visual-asset-review/SKILL.md).

## On-demand materials
- Only when producing an integration record, copy the [template](assets/templates/scene-integration.md).
- Only if unsure how to fill measurements and preserve the footprint, consult the [hypothetical example](references/examples/farm-bridge-fit.md).

## When NOT to use
- Creating a concept/prompt only: use `pixel-art-direction` or `pixel-art-prompt-authoring`.

## When to stop and report
If Unity/the scene is inaccessible, deliver static inspection and mark in-game observation `NOT RUN`.
A sprite replacement does not authorize behavior or save changes outside the spec.

## Expected output
Changed assets and wiring, compared measurements, camera/interaction evidence and remaining risks.
