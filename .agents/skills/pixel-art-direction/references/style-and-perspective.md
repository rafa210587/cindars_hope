# Direction by visual target

Read only the needed section of the project guide: `docs/design/art/ART_DIRECTION_ILLUSTRATOR_GUIDE.md`;
for cave creatures, `docs/design/art/CAVE_MONSTER_VISUAL_SPRITE_DIRECTION.md`.
Also open the specific approved keyart/base. The written guide does not replace the image.

## Palette and pixels
- Record colors by role: base, shadow, highlight, outline and accent. Hex is a reference,
  not a promise that a generator will reproduce it exactly.
- Compare contrast and cluster size at native resolution and intended camera scale.
- Inspection enlargement uses nearest-neighbor; do not confuse enlargement with new detail.
- Determine size from the existing target, keeping proportions and materials recognizable.
  Do not impose resolution, color count or head count on the entire game.

## Perspective and modularity
- Separate ground planes, vertical faces and volumes/roofs. Each piece must share
  projection, lighting direction and connection points with its neighbors.
- In 3/4 top-down sets, ground may be seen from above while a door remains frontal;
  this is a composition example, not an inference from the character's direction count.
- State whether sides are visible, how much top is shown, eave orientation and edge continuity.
- For tiles, check repetition in a mosaic; for isolated pieces, leave useful margins without accidental ground.
- Do not turn the style of one farm, one NPC or one previous failure into a universal rule.

## Minimum brief
Target and function; approved reference; perspective/plane; native dimensions and occupancy;
palette/material/lighting; silhouette and identity details; modular connections; acceptance criteria.
Record assumptions where the reference does not resolve a choice.
