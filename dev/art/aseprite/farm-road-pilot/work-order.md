# Farm road finishing pilot

Source: Assets/_Game/Art/Generated/World/tiles/ground_path_dirt.png (64x64 RGB PNG).
Reference: docs/art_catalog/_reference_keyart_GPT/farm_enclosed_valley_approved_v2.png.
Correction: warm packed earth with readable pixel clusters, sparse pebbles and gentle wear;
replace the heavily reduced tinted sand used by PaintPath. No animation or slice metadata exists in this PNG.
Keep native 64x64 canvas and opaque ground. Retain original on a hidden layer, corrections on named layers.
Display at 32 native pixels/world unit, split into eight-pixel cells to retain existing 0.25-unit geometry.
World-aligned atlas lookup avoids repeating a whole texture inside every small cell. Source remains unchanged.
Accept only after native image review, reopened Aseprite, exported PNG check and matching Unity captures.
Scope: road material and fence visual/physical agreement. Larger composition remains a separate refinement.
