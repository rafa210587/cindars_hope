---
name: pixel-art-prompt-authoring
description: Authors English prompts for Vaalara sprites with explicit identity, palette and perspective. Use when preparing generations or correcting prompts that lost the target's colors, equipment, anatomy or style.
---

# Skill: Pixel Art Prompt Authoring

The project has generation prompts in tools/aseprite; the visual brief must precede generator syntax.

**Core rule: the prompt translates approved references and target requirements without inventing lore or promising fidelity.**

## When to use
- Prepare a prompt/batch or correct drift observed in generated images.

## Essential checklist
- [ ] Actual reference and relevant guide section consulted.
- [ ] Subject, colors, equipment and perspective explicit in English.
- [ ] Resolution/framing and isolation appropriate to the piece type.
- [ ] Syntax and constraints adapted to the chosen generator.

## Procedure
1. If direction is not yet defined, use [pixel-art-direction](../pixel-art-direction/SKILL.md).
2. Describe subject, identity/material, palette, equipment, pose/plane, lighting and framing.
3. Include pixel style and edges consistent with the reference. For isolated objects, state what must
   be absent; for tiles, describe continuity and fill without requiring white-background isolation.
4. For ComfyUI/aziib/prompts.json, read [local prompts](references/local-prompt-contract.md).
5. For NPC walking, use the consumer layout in [npc-walk-animation](../npc-walk-animation/SKILL.md).
6. Compare the produced image with the brief using [visual-asset-review](../visual-asset-review/SKILL.md).

## On-demand materials
- Only when producing a reusable prompt, copy the [template](assets/templates/sprite-prompt.md).
- Only if unsure how to fill or correct an isolated-piece prompt, consult the [hypothetical example](references/examples/dock-prompt.md).

## When NOT to use
- UI/dialogue text: `localization-authoring`.
- Only post-processing or importing an already approved image.

## When to stop and report
If an essential identity decision is missing, record the gap rather than inventing it.
Do not turn model vocabulary anchors into new game canon.

## Expected output
English prompt, source reference, target constraints and review criteria.
When the destination is an existing file, preserve its schema and IDs.
