# WAVE INTEGRATION 04 - FarmScene Zone Map (Updated Post-Reconciliation)

## Status
CODE_READY_HUMAN_UNITY_SCENE_ACTION_REQUIRED

## Update History
- v1: VALIDATED_STATIC_SCENE_WIRED (2026-06-08) — original WAVE04 layout; scene reverted after YAML corruption
- v2: CODE_READY (2026-06-08) — updated layout after spatial reconciliation; must be regenerated via Unity Editor

## Coordinate model

| Field | Value |
|---|---|
| Scene | `Assets/_Game/Scenes/FarmScene.unity` |
| Coordinate basis | World space, centered origin |
| Scale contract | `FarmScaleContract.TileSizePixels = 32` |
| Bounds | 28x22 (x: -14 to +14, y: -11 to +11) |

## Zones (Updated Layout)

| Zone | Purpose | Object | Position | Size | Status |
|---|---|---|---|---|---|
| Zone_PlayerSpawn | Player spawn baseline | farm_zone_player_spawn | (0, 0) | 1.4 x 1.4 | CODE_READY |
| Zone_CropField | Crop plots (FarmPlot_00..08) | farm_zone_crop_field | (1, -2.25) | 5.5 x 5.5 | CODE_READY |
| Zone_ResourceTrees | Tree cluster (19 trees, east consolidated) | farm_zone_resource_trees | (9.5, 1.0) | 10.0 x 13.0 | CODE_READY |
| Zone_ResourceRocks | Future mining/rocks | farm_zone_resource_rocks | (-9, 5.0) | 4.5 x 3.5 | CODE_READY |
| Zone_Forage | Future forage pickup | farm_zone_forage | (-8, -2.0) | 4.5 x 4.5 | CODE_READY |
| Zone_LakeFishing | FishingSpot + lake | farm_zone_lake_fishing | (7.8, -2.8) | 5.8 x 4.4 | CODE_READY |
| Zone_ShippingSellpoint | Future shipping/sellpoint | farm_zone_shipping_sellpoint | (3.5, 7.5) | 3.5 x 2.5 | CODE_READY |
| Zone_Construction | Future building placement | farm_zone_construction | (-1, 9.0) | 6.0 x 3.0 | CODE_READY |
| Zone_HouseEntrance | Future home transition | farm_zone_house_entrance | (-5, -7.0) | 2.5 x 2.0 | CODE_READY |
| Zone_TownExit | Farm to Town | farm_zone_town_exit | (-8.25, -4.75) | 1.6 x 2.4 | CODE_READY |
| Zone_CaveEntrance | Farm to Cave | farm_zone_cave_entrance | (-5.5, 0) | 1.8 x 2.4 | CODE_READY |

## Existing Anchors (Preserved)
- farm_default: (0, 0)
- farm_from_town: (-7.25, -4.75)
- farm_from_cave: (-5, 0)
- Portal_Farm_To_Town: (-8.25, -4.75)
- Portal_Farm_To_Cave: (-5.5, 0)
- FishingSpot: (7.8, -2.8)

## Zone Readability Improvements vs WAVE04

| Problem | WAVE04 | Fixed in v2 |
|---|---|---|
| Trees scattered west+east | Trees 3-6 at (-8 to -6, 1-5) | All 19 trees consolidated in east (x: 6-13) |
| CropField near cave portal | FarmPlots at (-4.75, -1) adjacent to cave at (-5.5, 0) | FarmPlots moved to (1, -1.5), crop zone clear |
| HouseEntrance near bounds edge | (-12.4, -5.6) | (-5, -7) — visible, west of player |
| Zone_ResourceTrees missed west trees | East zone at (7.5, 1.2) | Trees consolidated east; zone covers all trees at (9.5, 1.0) size 10x13 |
| Oversized bounds (40x34) | 40x34 | 28x22 |

## Required Human Action

To apply this layout to FarmScene.unity:
1. Open Unity Editor
2. Menu: CindarsHope/Advanced/Legacy/Scenes/Create MVP FarmScene
3. Confirm FarmScene opens without Console errors
4. Run human Play Mode checklist (`docs/validation/WAVE_INTEGRATION_04_HUMAN_PLAYMODE_CHECKLIST.md`)
