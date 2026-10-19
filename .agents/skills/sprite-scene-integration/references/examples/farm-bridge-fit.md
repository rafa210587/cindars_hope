# HYPOTHETICAL example — farm bridge fit

Filled demonstration case. No asset, scene or save was changed, and Unity was not run.
All numbers are illustrative; do not replace actual settings with this example's values.

## Scenario Scope and source
Visual replacement of a fictional bridge while retaining existing passage, interaction and persistent identity.
Creator/importer: locate the actual owner before executing; this example invents no method.
Reference: teaching view of the same crossing before/after, with camera and viewport unchanged.

## Illustrative measurements
| Measurement | Before | Proposed after | Meaning |
|---|---|---|---|
| Occupied width | 96 px | 128 px | The new drawing occupies more pixels |
| PPU | 32 | 32 | Equal PPU does not preserve size when occupancy changes |
| Effective X scale | 1 | 1 | Widths would be 3 and 4 world units |
| Physical footprint | 3 units | Preserve initially | A visual replacement does not authorize widening passage |

## Error → diagnosis → correction
Scenario error: the bridge grows visually beyond the footprint and bank, although import remains sharp.
Diagnosis: occupancy/scale difference; do not assume the collider must grow to hide the defect.
Proposed correction: adjust art/scale to the passage contract after comparing proportions
and perspective. Choose a factor only after checking the target, without imposing a global bridge factor.
If orientation is wrong, reducing scale does not correct painted perspective.

## Import and contracts to preserve
Actual alpha, Point, Compression None and mipmaps false must be checked on the actual asset.
Pivot anchors the object's intended support, with no universal BottomCenter rule.
Interaction and persisted ID: keep existing values if the target has them; do not create a
fictional ID from this example's name. No reward or save change belongs to this visual replacement.

## Criteria to prove the correction
Same-camera captures must show support on the banks, scale relative to the player and sorting.
Actual traversal must demonstrate collision limits; interaction must retain anchor and range.
Correct import and width calculation do not prove gameplay. This exercise issues no validation
result: all of that evidence still needs to be produced in actual work.
