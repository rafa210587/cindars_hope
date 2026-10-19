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

## Request: "fix the typo in this error message"

HYPOTHETICAL local correction. Use `/bugfix`, read the target text and its immediate context,
preserve IDs/placeholders and inspect the diff. If it is player-facing localized text, consult
`localization-authoring` for that contract. No new spec, class, test suite or separate report.
Correct response: state the corrected text and check. Mistake: opening the wave protocol and
requiring PlayMode solely because a `.cs` file contains the string. If the key/schema changes,
reclassify the risk before editing rather than treating it as punctuation.

## Request: "change one line to bump the save version"

HYPOTHETICAL critical contract. A one-line change can invalidate persisted data. Read the
approved save contract and affected migration/restore path; use `save-load-pattern` and the
relevant compatibility fixtures. Define what old and new saves must do before editing.
Correct response: expose the migration requirement and obtain scoped evidence. Mistake:
calling it a small fix, changing the number and declaring success from compilation.

These examples explain selection; they neither authorize a real save change nor prove routing
by a live model. Do not load the art examples when the only ambiguity concerns a code fix.
