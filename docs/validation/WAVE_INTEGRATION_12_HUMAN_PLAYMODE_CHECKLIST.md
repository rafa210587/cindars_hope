# WAVE INTEGRATION 12 - Human Play Mode Checklist

## Status

PENDING

## Preconditions

- Unity opens without red console errors.
- WAVE_INTEGRATION_11 did not break input/HUD/player.
- `TownScene` contains NPC/dialogue/shop wiring.
- Inventory/economy managers are available.

## Checklist

| Step | Expected result | Pass/Fail | Notes |
|---|---|---|---|
| Open target scene | TownScene opens |  |  |
| Press Play | Game starts |  |  |
| Player movement | Player moves |  |  |
| Dialogue NPC visible | Pip or Wanderer appears |  |  |
| Approach dialogue NPC | Interaction prompt appears |  |  |
| Interact dialogue NPC | Dialogue panel opens |  |  |
| Dialogue content | Speaker line and choices appear |  |  |
| Navigate choices | Choice changes dialogue node |  |  |
| Close dialogue | Panel closes |  |  |
| Movement restored | Player moves again |  |  |
| Merchant/shop NPC visible | Seeds/tools or weapons merchant appears |  |  |
| Approach merchant | Interaction prompt appears |  |  |
| Open shop | Shop menu opens |  |  |
| Buy item | Gold decreases/item added or blocked reason appears |  |  |
| Sell item | Item removed/gold increases or blocked reason appears |  |  |
| Close shop | Shop closes |  |  |
| Focus/modal | No stuck UI state |  |  |
| Stop Play | Scene is not corrupted |  |  |

## Result

NOT RUN

## Bugs found

None recorded yet.

## Can start next wave

NO - human Play Mode validation is still required.
