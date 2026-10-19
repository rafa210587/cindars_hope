# Candidate acceptance

Read when a candidate exists. Separate successful execution from visual acceptance.

When present, compare user data/custom properties and adapter-created hidden layers against
the baseline. Identify ownership before cleaning metadata; preserve artist-authored hidden data.

1. Reopen the `.aseprite` candidate and verify expected layers/cels remain editable. Compare source hash. Confirm bounds, color mode/palette, frame count/order/durations, tag ranges/direction, slices/pivot keys and cel positions against the baseline. Record intentional deltas; inspect external importer metadata separately when it defines the pivot. Do not assume export JSON preserves every native property.
2. View baseline and candidate at 1:1 pixels and at the documented game display scale. Use nearest-neighbor enlargement only as an additional pixel inspection. If game-scale/camera evidence is unavailable, mark that check unobserved rather than infer scene correctness from a large preview.
3. Inspect silhouette, material clusters, outline rhythm, palette hierarchy, perspective and readability against approved art. Check real alpha against light and dark backgrounds. Confirm a correction is visible and beneficial at the target size; reject ornamental noise or indiscriminate smoothing.
4. For motion edits, compare contact poses and playback with original timings, direction consistency, foot anchoring, seams and loop continuity. Frame sheets alone cannot establish timing quality. Extra in-betweens need a demonstrated motion problem and must respect the consumer's frame contract.
5. Record candidate path, preview paths, commands/version, structural checks, visual evidence actually viewed and residual issues. Do not label a candidate accepted solely because the script completed. When promotion/import is in scope, preserve Unity GUIDs and pivots and use Point filtering, no compression and no mipmaps through the applicable integration workflow.
