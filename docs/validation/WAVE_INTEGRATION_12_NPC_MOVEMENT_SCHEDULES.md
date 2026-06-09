# WAVE INTEGRATION 12 - NPC Movement Schedules

Status: MVP_MOVEMENT_DOCUMENTED_WITH_SCHEDULE_DEBT

| NpcId | MovementProfile | Scene | Waypoints/Anchors | ActiveHours | Purpose | ImplementedNow | DeferredReason |
|---|---|---|---|---|---|---:|---|
| npc_pip_miudinho | Stationary | TownScene | town_reception_pip (-5,1) | All day MVP | Tutorial/social anchor | 1 | Full daily schedule deferred |
| npc_vaalara_wanderer_01 | WanderWithinZone | TownScene | town_wander_route_01, wander bounds -6,-3.5 to 6,3.5 | All day MVP | Lore wanderer | 1 | Weather/time route modifiers deferred |
| npc_shop_seeds_tools | ShopKeeperFixed | TownScene | town_shop_seeds_tools (4.5,2) | All day MVP | Seed/tool merchant | 1 | Opening hours deferred |
| npc_shop_weapons_armor | ShopKeeperFixed | TownScene | town_shop_weapons_armor (-3,2) | All day MVP | Equipment merchant | 1 | Opening hours deferred |
| canonical future roster | FutureScheduleOnly | TownScene/FarmScene/CaveScene | See roster | TBD | Social/service expansion | 0 | Future NPC content and schedules out of scope |

TownScene movement note:
- `NpcScheduleDefinition` and `NpcScheduleResolver` exist as pure runtime contracts.
- This WAVE12 does not implement final relationship/reputation/calendar schedule logic.
