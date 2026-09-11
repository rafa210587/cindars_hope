# HYPOTHETICAL example — isolated dock prompt

Teaching example without generation or image inspection. Dock, palette and dimensions are
illustrative proposals, not canonical data. An actual reference must be attached and checked during execution.

## Brief and destination
- Use: isolated wooden dock for the north bank of a fictional lake.
- Scenario identity: brown planks, two posts and beige rope; no boats or character.
- Perspective: visible upper surface, posts with consistent frontal faces; no isometric camera.
- Illustrative format: 96×64 native pixels. Confirm dimensions and channel capability before submitting.
- Channel: user-selected generator with a text prompt; no assumed model/API.

## English prompt
```text
Create one isolated wooden dock sprite for a 2D top-down three-quarter farm scene.
Use warm brown planks, two wooden posts and a beige rope, matching the supplied reference.
Show the deck's upper surface and consistent front-facing post sides, with light from the upper left.
Use a native 96 by 64 pixel canvas, crisp pixel clusters and clear edges, without smooth gradients.
Keep the complete silhouette and rope inside the canvas with a small clear margin.
Provide a genuinely transparent background with no painted checkerboard.
Do not include water, shoreline, boats, characters, text or interface elements.
```

This prompt should say “supplied reference” only if the image is actually attached/available.
If the tool cannot produce the requested size/alpha, record the limitation and evaluate the output;
do not claim compliance simply because the prompt contains those requirements.

## Scenario error → diagnosis → correction
A fictional previous attempt included a lake and boat. Hypothesis: “dock by a lake” induced scenery.
Smallest change: “one isolated wooden dock” and explicit exclusion of water/bank/boat.
If a compatible negative field exists, exercise option: `water, shoreline, boat, people, text, ui`.
Do not send that field to a channel accepting only a single prompt.

## Proof criterion
Open the delivered image and compare identity/perspective with the actual reference; measure alpha
and examine rope/margins. An apparent checkerboard background does not prove transparency.
This filled prompt example represents no generation or asset approval.
