---
name: visual-asset-review
description: Audits sprite, tile and prop images for fidelity, actual alpha and quality at game scale. Use when accepting a generation, comparing revisions or investigating opaque backgrounds and crops.
---

# Skill: Visual Asset Review

The project's visual evidence requires examining the delivered file and the target's reference.

**Core rule: descriptions, alt-text and executor summaries do not replace the actual image.**

## When to use
- Accept or reject a generated or edited sprite.
- Investigate checkerboard backgrounds, edges, crops, apparent scale or identity drift.

## Essential checklist
- [ ] Exact file opened; source and revision identified.
- [ ] Side-by-side comparison with the approved reference.
- [ ] Alpha measured when transparency is required.
- [ ] Inspection at native pixels and intended scale.
- [ ] Findings distinguish observation, inference and missing evidence.

## Procedure
1. Identify image, version and reference. Use the image reader actually available in the session.
2. Compare silhouette, colors, accessories, perspective and pixel clusters.
3. For transparency, downloads, crops or tiles, read [technical inspection](references/image-inspection.md).
4. Record localized defects, impact and suggested correction; do not approve details you could not inspect.

## On-demand references
- For repeated scene/reference revisions: apply [bounded visual iteration](../../../.claude/rules/visual-iteration-budget.md)
  to select a small correction round and stop low-return retries.
- For style criteria not yet defined: [pixel-art-direction](../pixel-art-direction/SKILL.md).
- For frames and playback: [sprite-animation-review](../sprite-animation-review/SKILL.md).
- For camera/import/collision effects: [sprite-scene-integration](../sprite-scene-integration/SKILL.md).

## On-demand materials
- Only when producing a review report, copy the [template](assets/templates/asset-review.md).
- Only if unsure how to fill findings or distinguish alpha from a preview, consult the [hypothetical example](references/examples/dock-alpha-review.md).

## When NOT to use
- Writing a prompt without a resulting image; use `pixel-art-prompt-authoring`.

## When to stop and report
Without a tool capable of opening the image, mark visual review `NOT RUN` and identify the required file.
Without alpha measurement, keep that criterion pending even if the preview looks transparent.

## Expected output
Scope and files examined; findings by impact; PASS/FAIL/NOT RUN criteria with evidence and limits.
