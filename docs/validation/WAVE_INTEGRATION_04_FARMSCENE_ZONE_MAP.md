# WAVE INTEGRATION 04 - FarmScene Zone Map

## Status

VALIDATED_STATIC_SCENE_WIRED

## Coordinate model

| Field | Value |
|---|---|
| Scene | `Assets/_Game/Scenes/FarmScene.unity` |
| Coordinate basis | Existing FarmScene world coordinates |
| Scale contract | `FarmScaleContract.TileSizePixels = 32` |
| Layout contract | `FarmLevel1LayoutContract` exists; current scene uses centered world coordinates rather than the contract's tile-coordinate anchors. |
| Bounds | Existing `Bounds` object: top/bottom/left/right colliders around the playable area. |

## Zones

| Zone | Purpose | Scene marker | Suggested location | Size/notes | Required in scene |
|---|---|---|---|---|---|
| Player Spawn Area | Spawn and return transition baseline | `Zone_PlayerSpawn` / `farm_zone_player_spawn` | Center, around `farm_default` | Small blue marker around player spawn | YES |
| Crop Field Area | Future soil/crops hookup | `Zone_CropField` / `farm_zone_crop_field` | Existing `FarmPlots` area | Brown marker covering current 3x3 plot cluster | YES |
| Resource Area - Trees | Future tree resource hookup | `Zone_ResourceTrees` / `farm_zone_resource_trees` | East/northeast tree cluster | Green marker around existing trees | YES |
| Resource Area - Rocks | Future rock/mining hookup | `Zone_ResourceRocks` / `farm_zone_resource_rocks` | Northwest spare area | Gray marker; no mining gameplay yet | YES |
| Forage Area | Future forage pickup hookup | `Zone_Forage` / `farm_zone_forage` | West side between trees/town route | Olive marker; no forage gameplay yet | YES |
| Lake/Fishing Area | Fishing/lake visual baseline | `Zone_LakeFishing` / `farm_zone_lake_fishing` | Existing `FishingSpot`/lake | Blue marker aligned to lake area | YES |
| Shipping/Sellpoint Area | Future shipping/sellpoint hookup | `Zone_ShippingSellpoint` / `farm_zone_shipping_sellpoint` | Northeast clear area | Gold marker; no shipping gameplay added | YES |
| Construction Area | Future building placement | `Zone_Construction` / `farm_zone_construction` | North/central clear area | Brown marker; no building gameplay added | YES |
| House Entrance Marker | Future home/interior transition | `Zone_HouseEntrance` / `farm_zone_house_entrance` | Southwest edge | Brown marker; no home scene created | YES |
| Town Exit Marker | Farm to Town transition | `Zone_TownExit` / `farm_zone_town_exit` | Existing Town portal | Yellow marker aligned to `Portal_Farm_To_Town` | YES |
| Cave Exit/Entrance Marker | Farm to Cave transition | `Zone_CaveEntrance` / `farm_zone_cave_entrance` | Existing Cave portal | Purple marker aligned to `Portal_Farm_To_Cave` | YES |

## Anchors

| Anchor | Purpose | Scene position | Notes |
|---|---|---|---|
| `farm_default` | Default player spawn | `(0, 0)` | Existing `SceneSpawnPoint`; preserved. |
| `farm_from_town` | Return from Town | `(-7.25, -4.75)` | Existing `SceneSpawnPoint`; preserved. |
| `farm_from_cave` | Return from Cave | `(-5, 0)` | Existing `SceneSpawnPoint`; preserved. |
| `Portal_Farm_To_Town` | Temporary Town transition | `(-8.25, -4.75)` | Existing `ScenePortal`; preserved. |
| `Portal_Farm_To_Cave` | Temporary Cave transition | `(-5.5, 0)` | Existing `ScenePortal`; preserved. |
| `FishingSpot` | Lake/fishing baseline | `(7.8, -2.8)` | Existing interaction target; preserved. |

## Collision/limits

Existing boundary colliders are preserved:
- `Bounds/Top`
- `Bounds/Bottom`
- `Bounds/Left`
- `Bounds/Right`

Zone marker colliders are triggers only. They do not block player movement and do not implement gameplay.

## Camera framing notes

The existing `Main Camera` and `CameraFollow2D` target are preserved. Camera visual fit still requires human Play Mode validation because static YAML/build checks cannot prove final framing.

## Next spec dependencies

WAVE_INTEGRATION_05 can connect the first farm interactables to these stable IDs without inventing new layout anchors:
- `farm_zone_crop_field`
- `farm_zone_resource_trees`
- `farm_zone_resource_rocks`
- `farm_zone_forage`
- `farm_zone_shipping_sellpoint`
