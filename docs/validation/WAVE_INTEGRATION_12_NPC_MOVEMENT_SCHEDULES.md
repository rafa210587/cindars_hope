# WAVE INTEGRATION 12 - NPC Movement Schedules

Status: MVP_MOVEMENT_PROFILES_PLACED_WITH_DAILY_SCHEDULE_DEBT

| NpcId | MovementProfile | Scene | Waypoints/Anchors | ActiveHours | Purpose | ImplementedNow | DeferredReason |
|---|---|---|---|---|---|---:|---|
| npc_pip_miudinho | Stationary | TownScene | town_reception_pip (-5,1) | All day MVP | Tutorial/social anchor | 1 | Full daily schedule deferred |
| npc_sylveth | ShopKeeperFixed | TownScene | town_shop_seed_vendor (4.5,2) | All day MVP | Seed/farm merchant | 1 | Opening hours deferred |
| npc_brumdar | ShopKeeperFixed | TownScene | town_blacksmith_brumdar (-4.75,2.2) | All day MVP | Blacksmith/repair merchant | 1 | Opening hours deferred |
| npc_renko | ShopKeeperFixed | TownScene | town_general_store_renko (0,3.25) | All day MVP | General merchant | 1 | Opening hours deferred |
| npc_thalindra | Stationary | TownScene | town_library_thalindra (-6.2,-0.5) | All day MVP | Library/quest hook | 1 | Final library schedule deferred |
| npc_zrix | ShopKeeperFixed | TownScene | town_cave_route_zrix (5.75,-1.75) | All day MVP | Cave rumor/supplies | 1 | Patrol/route schedule deferred |
| npc_nimble | Stationary | TownScene | town_workshop_nimble (2.75,-3.2) | All day MVP | Workshop/crafting hook | 1 | Workshop hours deferred |
| npc_vaalara_wanderer_01 | WanderWithinZone | TownScene | town_wander_route_01, wander bounds -6,-3.5 to 6,3.5 | All day temporary | Existing lore wanderer | 1 | Extra, not MVP completeness |

Notes:
- Merchants are intentionally `ShopKeeperFixed` for this playable slice.
- `NpcScheduleDefinition` and `NpcScheduleResolver` remain future schedule contracts.
- No final relationship/reputation/calendar routing was added.
