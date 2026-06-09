# WAVE INTEGRATION 12 - NPC Canonical Roster

Status: CANONICAL_ROSTER_EXTRACTED_WITH_MVP_SUBSET_AND_ID_DEBT

NPC implementation model: DATA_DRIVEN_GENERIC_CONTROLLERS

Source:
- `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
- Existing playable assets in `Assets/_Game/Data/NPCs/`

Important debt:
- Canonical design ID for Pip is `npc_pip`, while the existing playable asset uses `npc_pip_miudinho`. This execution preserves the existing stable runtime ID and documents the alias debt instead of renaming scene/save-facing data.

| NpcId | DisplayName | Role | Purpose | Responsibilities | PrimaryScene | Placement | MovementProfile | DialogueSetId | DialogueOptions | ShopId | MVP? | Status |
|---|---|---|---|---|---|---|---|---|---:|---|---:|---|
| npc_pip_miudinho | Pip Miudinho | Tutorial messenger | Introduce town/social/shop basics | Tutorial guidance, shop directions, gameplay tips, future first delivery hook | TownScene | town_reception_pip (-5,1) | Stationary | dialogue_pip_miudinho | 10 | 1 | PLACED_DIALOGUE_ONLY |
| npc_vaalara_wanderer_01 | Peregrino de Vaalara | Lore/town wanderer | Provide non-spoiler lore and cave/town hints | Rumors, lore, future route hints | TownScene | town_wander_route_01 (0,-1.5) | WanderWithinZone | dialogue_vaalara_wanderer_01 | 10 | 1 | PLACED_DIALOGUE_ONLY |
| npc_shop_seeds_tools | Vendedor de Sementes | Merchant | First farm/shop commercial loop | Sell seeds/tools/food/potions, buy/sell through shop UI | TownScene | town_shop_seeds_tools (4.5,2) | ShopKeeperFixed | dialogue_shop_seeds_tools_placeholder | 10 | shop_seeds_tools | 1 | PLACED_SHOP_ONLY |
| npc_shop_weapons_armor | Lojista de Armas | Merchant | First equipment/repair shop loop | Sell weapons/armor/repair kits, buy/sell through shop UI | TownScene | town_shop_weapons_armor (-3,2) | ShopKeeperFixed | dialogue_shop_weapons_armor_placeholder | 10 | shop_weapons_armor | 1 | PLACED_SHOP_ONLY |
| npc_corvus | Padre Corvus | Curandeiro/Guardiao | Temple/healing/canon future service | Healing, oath, temple lore | TownScene | TBD temple | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_mara | Mara Vellum | Escriba/Comerciante | Licenses/contracts future service | Licenses, contracts, reputation hooks | TownScene | TBD registry | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_sylveth | Sylveth | Plantador/Curandeiro | Farm/seed future NPC | Seeds, crops, rural tutorial | TownScene/FarmScene | TBD seed shop/farm visit | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_brumdar | Brumdar Ferro-Quieto | Artesao/Combatente | Forge/tool future service | Tool upgrades, weapons, ore | TownScene | TBD forge | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_nimble | Nimble Galhobaixo | Construtor/Artesao | Construction future service | Buildings, moving structures | TownScene/FarmScene | TBD carpenter | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_gurd | Gurd Carvalho-Torto | Construtor/Combatente | Heavy construction future helper | Clearing, expansion | TownScene/FarmScene | TBD construction | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_hund | Hund Carvalho-Torto | Guardiao/Construtor | Guard/transport future helper | Defense, delivery protection | TownScene | TBD guard route | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_ozzra | Ozzra Fumacazul | Alquimista/Artesao | Alchemy future service | Potions, fertilizer, reagents | TownScene/FarmScene | TBD lab | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_gruta | Gruta Panela-Funda | Comerciante/Musico | Tavern/food/social future service | Food, music, rumors | TownScene | TBD tavern | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_zrix | Zrix das Estradas | Explorador/Comerciante | Cave/route future NPC | Cave routes, supply trade | TownScene/CaveScene | TBD guild/road | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_yael | Yael Noite-Mansa | Comerciante/Explorador | Night shop future NPC | Rare items, secrets | TownScene | TBD night shop | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_thalindra | Thalindra Veu-de-Lua | Pesquisador/Alquimista | Lore/research future service | Archive, ruins, translation | TownScene | TBD archive | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_dagna | Dagna Rocha-Morna | Minerador/Combatente | Mining future service | Ore, cave, quarry | TownScene/CaveScene | TBD mine route | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_alaric | Ser Alaric Veyr | Guardiao/Combatente | Guard/combat future service | Patrols, security, combat hooks | TownScene | TBD guard post | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_mirela | Mirela dos Lacos | Artesao/Comerciante | Clothing/bag future service | Bags, clothing, accessories | TownScene | TBD tailor | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_renko | Renko Tres-Sorrisos | Comerciante/Artesao | General store future merchant | Common goods, bargaining | TownScene | TBD general store | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_eiran | Eiran Valeclaro | Tratador/Plantador | Animals/pets future service | Animal care, pet hooks | TownScene/FarmScene | TBD animal area | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_liora | Liora Canta-Rio | Musico/Pesquisador | Music/dream lore future NPC | Music, dreams, Anya hints | TownScene | TBD social point | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_orlan | Orlan Pouso-Curto | Comerciante/Escriba | Inn/news future service | Lodging, travelers, news | TownScene | TBD inn | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_savra | Savra Escama-Verde | Curandeiro/Explorador | Herb/antidote future service | Herbs, antidotes, forest lore | TownScene/FarmScene | TBD herb route | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_tovin | Tovin Maos-de-Selo | Escriba/Artesao | Registry/altar permit future service | Licenses, stamps, permits | TownScene | TBD registry | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
| npc_maelor | Maelor Cinza | Explorador/Pesquisador | Late lore future NPC | Night memory, ruins, secrets | TownScene | TBD night route | FutureScheduleOnly | TBD | 0 | TBD | 0 | FUTURE_SCOPE |
