# WAVE INTEGRATION 12 - NPC Canonical Roster

Status: MVP_CANONICAL_PLAYABLE_SLICE_ROSTER_PLACED_WITH_DEBT

NPC implementation model: DATA_DRIVEN_GENERIC_CONTROLLERS

Sources used:
- `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
- `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
- Existing runtime assets under `Assets/_Game/Data/NPCs/`, `Assets/_Game/Data/Dialogues/`, and `Assets/_Game/Data/Economy/`

Definition used in this fix:
- "All NPCs" means all NPCs selected as MVP/playable-slice functional coverage, not the full future social roster.
- Future/lore-only NPCs remain future scope and are not required in TownScene now.
- Dialogue text is technical placeholder authoring, not final narrative.

| NpcId | DisplayName | Role | Purpose | Responsibilities | PrimaryScene | Placement | MovementProfile | DialogueSetId | DialogueOptions | ShopId | MVP? | Status |
|---|---|---|---|---|---|---|---|---|---:|---|---:|---|
| npc_pip_miudinho | Pip Miudinho | Town guide/tutorial | Introduce town, shops and runtime interaction loop | Tutorial, shop directions, gameplay tips, future delivery hook | TownScene | town_reception_pip (-5,1) | Stationary | dialogue_pip_miudinho | 10 |  | 1 | PLACED_AND_WIRED |
| npc_sylveth | Sylveth | Seed/farm merchant | First farm supply loop | Seeds, crop tips, farm supplies, buy/sell via real shop runtime | TownScene | town_shop_seed_vendor (4.5,2) | ShopKeeperFixed | dialogue_sylveth | 10 | shop_seeds_tools | 1 | PLACED_AND_WIRED |
| npc_brumdar | Brumdar Ferro-Quieto | Blacksmith/repair merchant | First equipment and repair support | Weapons, armor, repair kits, cave preparation | TownScene | town_blacksmith_brumdar (-4.75,2.2) | ShopKeeperFixed | dialogue_brumdar | 10 | shop_blacksmith | 1 | PLACED_AND_WIRED |
| npc_renko | Renko Tres-Sorrisos | General merchant | Common goods and sell/buy coverage | Food, basic supplies, general store buy/sell | TownScene | town_general_store_renko (0,3.25) | ShopKeeperFixed | dialogue_renko | 10 | shop_general_store | 1 | PLACED_AND_WIRED |
| npc_thalindra | Thalindra Veu-de-Lua | Library/quest hook NPC | Lore and future quest authoring point without creating final quest | Non-spoiler lore, archive hints, future quest hook | TownScene | town_library_thalindra (-6.2,-0.5) | Stationary | dialogue_thalindra | 10 |  | 1 | PLACED_AND_WIRED |
| npc_zrix | Zrix das Estradas | Cave rumor/supplies NPC | Cave readiness bridge without executing WAVE13 | Cave rumors, supply shop, route hints | TownScene | town_cave_route_zrix (5.75,-1.75) | ShopKeeperFixed | dialogue_zrix | 10 | shop_cave_supplies | 1 | PLACED_AND_WIRED |
| npc_nimble | Nimble Galhobaixo | Crafting/workshop NPC | Workshop/crafting future service anchor | Workshop hints, crafting direction, future building hooks | TownScene | town_workshop_nimble (2.75,-3.2) | Stationary | dialogue_nimble | 10 |  | 1 | PLACED_AND_WIRED |
| npc_vaalara_wanderer_01 | Peregrino de Vaalara | Temporary lore wanderer | Existing non-canonical runtime placeholder retained for compatibility | Rumors and route hints | TownScene | town_wander_route_01 (0,-1.5) | WanderWithinZone | dialogue_vaalara_wanderer_01 | 10 |  | 0 | TEMPORARY_EXISTING_RUNTIME_EXTRA |
| npc_corvus | Padre Corvus | Curandeiro/Guardiao | Temple/healing future service | Healing, oath, temple lore | TownScene | TBD temple | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_mara | Mara Vellum | Escriba/Comerciante | Licenses/contracts future service | Licenses, contracts, reputation hooks | TownScene | TBD registry | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_gurd | Gurd Carvalho-Torto | Construtor/Combatente | Heavy construction future helper | Clearing, expansion | TownScene/FarmScene | TBD construction | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_hund | Hund Carvalho-Torto | Guardiao/Construtor | Guard/transport future helper | Defense, delivery protection | TownScene | TBD guard route | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_ozzra | Ozzra Fumacazul | Alquimista/Artesao | Alchemy future service | Potions, fertilizer, reagents | TownScene/FarmScene | TBD lab | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_gruta | Gruta Panela-Funda | Comerciante/Musico | Tavern/food/social future service | Food, music, rumors | TownScene | TBD tavern | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_yael | Yael Noite-Mansa | Comerciante/Explorador | Night shop future NPC | Rare items, secrets | TownScene | TBD night shop | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_dagna | Dagna Rocha-Morna | Minerador/Combatente | Mining future service | Ore, cave, quarry | TownScene/CaveScene | TBD mine route | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_alaric | Ser Alaric Veyr | Guardiao/Combatente | Guard/combat future service | Patrols, security, combat hooks | TownScene | TBD guard post | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_mirela | Mirela dos Lacos | Artesao/Comerciante | Clothing/bag future service | Bags, clothing, accessories | TownScene | TBD tailor | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_eiran | Eiran Valeclaro | Tratador/Plantador | Animals/pets future service | Animal care, pet hooks | TownScene/FarmScene | TBD animal area | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_liora | Liora Canta-Rio | Musico/Pesquisador | Music/dream lore future NPC | Music, dreams, Anya hints | TownScene | TBD social point | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_orlan | Orlan Pouso-Curto | Comerciante/Escriba | Inn/news future service | Lodging, travelers, news | TownScene | TBD inn | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_savra | Savra Escama-Verde | Curandeiro/Explorador | Herb/antidote future service | Herbs, antidotes, forest lore | TownScene/FarmScene | TBD herb route | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_tovin | Tovin Maos-de-Selo | Escriba/Artesao | Registry/altar permit future service | Licenses, stamps, permits | TownScene | TBD registry | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_maelor | Maelor Cinza | Explorador/Pesquisador | Late lore future NPC | Night memory, ruins, secrets | TownScene | TBD night route | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |

Debt:
- TEMPORARY_DIALOGUE_AUTHORING_PLACEHOLDER remains for all MVP dialogue text.
- Relationship, romance, reputation, final daily schedules, final quest content and final city simulation remain out of scope.
- `npc_pip_miudinho` keeps existing stable runtime ID instead of renaming to canonical `npc_pip`.
