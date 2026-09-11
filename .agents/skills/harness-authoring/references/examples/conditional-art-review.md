# Example: decide what to load for an art review

**HYPOTHETICAL — teaching example of routing, without an actual image or farm assessment.**
Uses installed skills; do not create a new skill named after this example.

## Request: "the dock seems to have a checkerboard background"
Selected entry: `visual-asset-review/SKILL.md`.
Relevant detail: `references/image-inspection.md`, because a visible checkerboard does not prove alpha.
If a report is requested, use the review template linked by that entry. The filled example
helps only when unsure how to distinguish observation from hypothesis; it is not mandatory reading.
Do not load attack diagnostics, the NPC 5x5 contract or local-generation guidance for this case.

Illustrative decision: without reading pixels, "the background looks checkered" is a preview
observation; "opaque alpha" remains a hypothesis. The report records pending measurement, not approval.
The executor inspects the exact file; the audit-only reviewer does not remove the background unilaterally.

## Request: "the walk seems to slide"
Selected entry: `sprite-animation-review/SKILL.md`.
Relevant detail: `references/motion-diagnostics.md`, because support, character displacement,
timing and cell registration must be distinguished. Open the NPC contract only if the target
consumes `NpcWalkAnimator`; do not automatically apply the 5x5 layout to the player.

Illustrative decision: a contact sheet with different poses allows drawing review;
without playback, it cannot establish fluidity or synchronization with in-game speed.

## Why this disclosure helps
The same art request selects different resources depending on the question. A template structures
output; references explain criteria; examples clarify decisions. Loading everything before
identifying the problem keeps files separate but loses the benefit of on-demand reading.
