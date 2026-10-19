# WAVE_INTEGRATION_13 — Scene Camera Bounds Matrix

## Status

DOCUMENTATION_ONLY — camera bounds must be verified in Unity Editor during human Play Mode.

## Matrix

| SceneId | Scene Name | Walkable Bounds (approx) | Camera Bounds | Spawn Valid? | Gates Reachable? | Notes |
|---------|-----------|--------------------------|---------------|-------------|-----------------|-------|
| scene_farm | FarmScene | (-20, -15) to (20, 15) | Match walkable or slight margin | YES (requires human wiring of spawn anchors) | YES if gates placed at edges | CameraScaleController resolves per SceneNames.Farm |
| scene_town | TownScene | UNKNOWN — TOWNSCENE_PLACEHOLDER_MINIMAL | UNKNOWN | YES after spawn_town_from_farm placed | YES after gate_town_farm_exit placed | TownScene is minimal placeholder; human must place NPCs + gates |
| scene_cave | CaveScene | Procedurally generated | Procedurally generated | YES after spawn_cave_from_farm placed | YES after gate_cave_farm_exit placed | CaveExitPortal already provides cave→farm; new gate is additional |

## CameraScaleController Configuration

CameraScaleController already reads `SceneManager.GetActiveScene().name` and applies per-scene camera size from CameraScaleConfigSO. Existing configs:
- "FarmScene" → configured (from prior waves)
- "TownScene" → may have entry; human should verify
- "CaveScene" → may have entry; human should verify

## Spawn Reachability

Spawn anchors must be placed within walkable area. If placed outside walkable bounds, player will be stuck.

For FarmScene:
- spawn_farm_from_town: near eastern edge (where town gate is placed)
- spawn_farm_from_cave: near southern edge (where cave entrance is placed)
- spawn_farm_default: center-left of FarmScene (existing player start area)

For TownScene:
- spawn_town_from_farm: near western edge (where farm exit gate is placed)

For CaveScene:
- spawn_cave_from_farm: near cave entrance area

## Camera Handoff After Transition

After PlayerSpawnResolver repositions the player, CameraFollow2D (if wired to the player transform) will immediately start following. Camera may jump if it starts far from spawn point. This is acceptable for MVP.

Smooth camera transition deferred with fade system.
