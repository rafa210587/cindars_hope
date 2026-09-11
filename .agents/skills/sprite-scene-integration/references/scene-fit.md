# Scene measurements and fit

## Import and scale
- Check file alpha before import; a checkerboard preview does not prove transparency.
- Check Sprite mode/slicing, PPU, pivot, padding and absence of unintended import rescaling.
- Visible world height = occupied pixel height / PPU × effective scale on that axis.
  Compare with player and neighboring pieces; a larger canvas/padding does not mean a larger character.
- For an orthographic camera, relate viewport and orthographicSize to readability; inspect the
  game framing and target resolution as well as Scene View zoom.
- For animation, compare idle and walk: equal PPU does not compensate for different drawing heights.

## Support, footprint and interaction
- Pivot represents the anchor used for sorting/positioning, often foot support;
  do not impose BottomCenter on doors, effects, tiles or any asset without checking its contract.
- Collider/footprint must match the intended walkable/blocked area, not an entire tree canopy
  or roof. Compare visual support, collider and interaction anchor.
- Preserve GUIDs and IDs used by save/interaction. An image replacement does not authorize new IDs,
  state resets, reward changes or another system.
- Check ordering when passing in front/behind, interaction range and affected transitions.

## Farm example, not a universal rule
A bridge painted in a horizontal perspective may look wrong when rotated 90 degrees
to cross a vertical river: rotation also turns the painted perspective and lighting.
In that situation, use art with the correct orientation or reconsider the fit required by the reference.
This does not prohibit rotating tiles/props whose art and contract allow rotation.
An oversized farm tree requires comparing occupancy/PPU/scale/camera;
do not automatically shrink all trees in other scenes by the same factor.

## Evidence
Capture through the same camera before/after, relevant import/Transform values and
collision/interaction observation when affected. Build PASS does not prove composition or perceived scale.
