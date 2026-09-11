# HYPOTHETICAL example — checkerboard on a dock sprite

Observations and measurements are simulated to teach report completion. No file was opened,
no pixel was measured and no actual asset was approved or rejected by this example.

## Scenario Scope and Evidence
- Fictional target: isolated dock sprite, teaching revision B, exported after generation.
- Fictional reference: wooden dock without water embedded in the drawing.
- Illustrative dimensions: 128×96 pixels; planned inspection at native zoom and nearest-neighbor enlargement.
- In actual use, record path, revision and the image reader that examined the delivered file.

## Criteria examined in the exercise
| Criterion | Simulated observation | Consequence |
|---|---|---|
| Alpha | RGBA, but all sampled background pixels have A=255 | An alpha channel alone does not prove transparency |
| Background | Two gray tones form a checkerboard also visible in RGB | Painted-checkerboard hypothesis; confirm alpha distribution |
| Crop | Light rope touches the outer edge | Global removal of light tones may erase part of the subject |
| Identity | Silhouette matches the exercise description | A description cannot establish fidelity to the actual reference |

## Demonstration finding
Priority: high impact on isolated-object presentation.
Location: fictional dock's outer background, particularly between posts.
Observation: opaque alpha in simulated samples despite apparent transparency in the preview.
Impact: rectangle/checkerboard over in-game water.
Suggested correction: ask the executor for background removal that preserves rope and light wood;
the reviewer does not execute their own suggestion.
How to prove: measure alpha in the new file and composite over contrasting backgrounds, examining rope,
edges and gaps. A new PNG extension or an RGBA file is insufficient.

## Limits
No actual review status is issued. The scenario teaches error → measurement → diagnosis;
pixel acceptance, fidelity and scene presentation still require separate actual evidence.
