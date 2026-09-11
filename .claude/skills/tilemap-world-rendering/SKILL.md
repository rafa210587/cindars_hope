---
name: tilemap-world-rendering
description: Build or repair 2D top-down world rendering with Tilemap, Rule Tiles, sorting, collision and pixel-art import. Use for ground seams, stretched structures or world-depth problems.
---

# Skill: Tilemap world rendering

Inspect the existing scene creator, palette, camera and physical contracts before changing world
rendering. Ground tiling, large structures, depth and collision have distinct owners.

## Essential workflow
1. Classify the problem: ground tiling/seams, structure rendering, Y-sort/depth, cave
   materialization or collision. Do not rebuild unrelated layers.
2. Read [rendering contracts](references/rendering-contracts.md) only for the affected category.
   Cave work additionally requires `cave-stable-run-guard`.
3. Preserve Point filtering, Compression None, no mipmaps, native scale/PPU and approved alpha.
   Avoid stretched quads or SpriteRenderer tiling for world ground.
4. Persist changes through existing scene creators and canonical Editor orchestration. Read
   [integration and validation](references/integration-and-validation.md) before executing a
   generator or claiming scene behavior.
5. Verify the actual scene at game scale: seams, sorting, footprint, traversal and console.
   Static asset or Editor inspection alone does not prove Play Mode behavior.

Use `sprite-scene-integration` for imported sprites and `unity-asset-generation` for generator
execution. Deliver changed layers/assets/creator, observed camera and collision behavior, and
separate automated, visual and human evidence.
