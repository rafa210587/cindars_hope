# Motion diagnostics

Optional onion-skin overlays, alpha bounds/centroid and pixel differences can locate jitter
or duplicate images. They do not grade motion: identical frames may be intended holds and
large differences may be errors. Compare timed playback and intended support before a verdict.

## Walking and support
- Locate contacts, passing poses and opposite contacts. Drawing count depends on contract and action;
  holds and similar poses may be intentional. Do not require distinct feet in every frame.
- Check leg alternation and weight transfer, the planted foot during support and absence
  of unintended sliding. Distinguish in-place walking from displacement embedded in animation.
- The body may bob deliberately; preserving support does not mean freezing the whole character.
- Front/back views and robes hide legs: use silhouette, hem, arm and support changes as
  complementary evidence, without automatically approving a motionless pose.

## Separate causes
| Symptom | What to check | Likely correction |
|---|---|---|
| Near-copies without progression | Key poses, order and intended holds | Redraw missing phases |
| Body jumps despite plausible gait | Baseline, crop, padding and pivot | Correct registration/slicing |
| Gait stalls or accelerates | Frame durations and final/initial contact | Adjust timing/order |
| Accessory changes between frames | Reference and shape consistency | Correct identity |
| Walking in the wrong direction | Actual vector, bucket, row and flipX | Correct mapping, if proven |

## Actions and directions
- Attacks: anticipation, strike, recovery and weapon arc; the hand must support the accessory
  without teleportation between poses. A deliberate smear is not duplication or a defect by itself.
- Check weapon/shield orientation and asymmetry when mirroring. Do not invent additional directions
  if runtime resolves them through flip and this matches the approved art.
- Inspect loop closure and idle→walk→idle; compare with the base at a shared zoom.
- Labeled strips reveal poses and crops; playback reveals rhythm. In-game evidence is still needed
  to claim synchronization with speed, collisions or actual movement direction.
