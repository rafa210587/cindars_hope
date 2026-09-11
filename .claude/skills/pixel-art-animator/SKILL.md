---
name: pixel-art-animator
description: Author or revise sprite animation frames, timing, tags and linked cels in Aseprite. Use for animation creation or edits; use sprite-animation-review for audit-only requests.
---

# Skill: Pixel Art Animator

Adapted from willibrandon/pixel-plugin under MIT. [Provenance](references/provenance.md)
is for upstream comparison, not a required execution read.

## Core workflow
1. Identify the actual animation consumer: canvas, directions, frame order, timings, pivots,
   loop/one-shot behavior and gameplay event anchors. Read the NPC 5x5 contract only for that consumer.
2. Inspect source poses and choose the smallest justified change. More frames do not guarantee
   better motion. Separate contact, passing, weight transfer and recovery; keep planted feet grounded.
3. Use [aseprite-authoring](../aseprite-authoring/SKILL.md) for preserved-source layered candidates
   and CLI/Lua execution. No Aseprite MCP is required by this adaptation.
4. For frame/tag/timing/link edits, load [operations](references/operations.md). For a sequence
   brief, adapt the [template](assets/templates/animation-plan.md). If design is unclear, consult
   the [hypothetical examples](references/examples.md), not every resource by default.
5. Reopen the saved candidate and verify frame counts, duration units, tag ranges and linked-image
   sharing. Recheck frame references after insertion/deletion; preserve external import contracts.
6. Use [sprite-animation-review](../sprite-animation-review/SKILL.md) for poses AND timed playback.
   Static exports prove layout only. Route authorized Unity integration through
   [sprite-scene-integration](../sprite-scene-integration/SKILL.md).

## Ownership and delivery
The implementing owner authors assets; pixel-art-scene-reviewer independently audits results.
Deliver editable candidate, timed preview, changed frame/direction list and performed/pending checks.
Never alter runtime combat timings, hitboxes or save data merely to accommodate new artwork.
Do not mark an animation game-ready from frame counts or successful exports alone.
