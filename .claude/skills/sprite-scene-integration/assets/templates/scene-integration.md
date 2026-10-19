# Visual integration — {{asset/scene/revision}}

Fill with values read from the target; this template defines no value or result.
Justify omitting inapplicable criteria and preserve the spec's scope.

## Scope and source
- Spec/ownership and approved file: {{paths and revision}}.
- Scene/prefab and existing creator/importer: {{targets and authorized method}}.
- Comparison reference/camera: {{file, framing and resolution}}.
- Concurrent work to preserve: {{relevant files/state}}.

## Before/after measurements
| Measurement | Before | After | Reading source |
|---|---|---|---|
| Visible pixel occupancy | {{value}} | {{value}} | {{file/bbox}} |
| PPU and effective scale | {{values}} | {{values}} | {{import/Transform}} |
| World dimension | {{pixels/PPU × scale}} | {{calculation}} | {{measurements above}} |
| Pivot and support | {{anchor}} | {{anchor}} | {{slice/capture}} |
| Camera/viewport | {{configuration}} | {{configuration}} | {{source}} |

## Import and contracts
- Alpha and import: {{measurement; Point, None, mipmaps false; slice when applicable}}.
- Sorting and fit: {{expected behavior, front/behind and perspective}}.
- Footprint/collider: {{physical area versus drawing; how to verify}}.
- Interaction: {{anchor, range and affected behavior; or not applicable}}.
- Save GUID/ID: {{identity preserved or not applicable; do not invent a new ID}}.

## Execution and evidence
- Method actually executed: {{Editor API/existing command and evidence}}.
- Comparable before/after captures: {{files and observations}}.
- Collision/interaction/game: {{actual observation or reason for absence}}.
- Result by criterion: {{supported conclusions; separate import from visual acceptance and gameplay}}.

## Deviations and Residual risk
{{Remaining defect, hypothesis, correction within scope and evidence needed to close.}}
