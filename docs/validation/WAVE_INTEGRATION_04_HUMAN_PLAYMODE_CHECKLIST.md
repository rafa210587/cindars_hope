# WAVE INTEGRATION 04 - Human Play Mode Checklist

## Status

PENDING

## Preconditions

- Unity opens without red Console errors.
- WAVE_INTEGRATION_03 player/camera baseline is complete.
- Target FarmScene is `Assets/_Game/Scenes/FarmScene.unity`.
- `FarmSceneFoundationZones` exists in the scene hierarchy.

## Checklist

| Step | Expected result | Pass/Fail | Notes |
|---|---|---|---|
| Open FarmScene | Scene opens without missing script warnings |  |  |
| Inspect hierarchy | `FarmSceneFoundationZones` exists with 11 `Zone_*` children |  |  |
| Press Play | Game starts |  |  |
| Player visible | Player appears in FarmScene near `farm_default` |  |  |
| Move player | Player moves inside farm |  |  |
| Camera | Camera frames/follows player |  |  |
| Farm bounds | Player does not leave expected playable area |  |  |
| Crop area | Crop area is visually identifiable around `FarmPlots` |  |  |
| Resource areas | Tree, rock, and forage zones are visually identifiable |  |  |
| Lake area | Lake/fishing area is visible or marked |  |  |
| Shipping area | Shipping/sellpoint zone is visible or marked |  |  |
| House/town/cave markers | House entrance, Town exit, and Cave entrance markers exist |  |  |
| Stop Play | Scene does not retain unwanted Play Mode changes |  |  |

## Result

PENDING

## Bugs found

TBD

## Can start WAVE_INTEGRATION_05

YES after this checklist passes or the human explicitly accepts the residual Play Mode risk.
