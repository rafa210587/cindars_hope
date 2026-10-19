# WAVE INTEGRATION 25 - NPC Positioning and Anchor Matrix

Status: SCENE_WIRING_DEBT (positions set in NpcDataSO.DefaultPosition; scene wiring requires Unity Editor)

Source: WAVE12C placement map + NpcDataSO.DefaultPosition fields.

| NpcId | DisplayName | Zone | PlacementId | DefaultPosition | HomeAnchor | SceneMarker |
|---|---|---|---|---|---|---|
| npc_pip | Pip Semente-Solta | TownEntrance/Market | town_pip | WanderWithinZone | npc_pip_home | NpcScenePlacementMarker (WAVE12C) |
| npc_sylveth | Sylveth | SeedShop/Garden | town_sylveth | ShopKeeperFixed | npc_sylveth_home | NpcScenePlacementMarker (WAVE12C) |
| npc_brumdar | Brumdar Ferro-Quieto | Forge | town_brumdar | ShopKeeperFixed | npc_brumdar_home | NpcScenePlacementMarker (WAVE12C) |
| npc_renko | Renko Tres-Sorrisos | GeneralStore | town_renko | ShopKeeperFixed | npc_renko_home | NpcScenePlacementMarker (WAVE12C) |
| npc_thalindra | Thalindra Veu-de-Lua | Archive | town_thalindra | Stationary/ArchiveDesk | npc_thalindra_home | NpcScenePlacementMarker (WAVE12C) |
| npc_zrix | Zrix das Estradas | Guild/RoadGate | town_zrix | Patrol/CaveRoad | npc_zrix_home | NpcScenePlacementMarker (WAVE12C) |
| npc_nimble | Nimble Galhobaixo | Carpenter | town_nimble | Patrol/WorkshopDesk | npc_nimble_home | NpcScenePlacementMarker (WAVE12C) |
| npc_corvus | Padre Corvus | Temple | town_corvus | Stationary/TemplePatrol | npc_corvus_home | NpcScenePlacementMarker (WAVE12C) |
| npc_mara | Mara Vellum | Registry | town_mara | Stationary/RegistryDesk | npc_mara_home | NpcScenePlacementMarker (WAVE12C) |
| npc_gurd | Gurd Carvalho-Torto | ConstructionYard | town_gurd | Patrol/HeavyWorkZone | npc_gurd_home | NpcScenePlacementMarker (WAVE12C) |
| npc_hund | Hund Carvalho-Torto | GuardRoute | town_hund | Patrol/TownRoad | npc_hund_home | NpcScenePlacementMarker (WAVE12C) |
| npc_ozzra | Ozzra Fumacazul | AlchemyLab | town_ozzra | WanderWithinZone/Lab | npc_ozzra_home | NpcScenePlacementMarker (WAVE12C) |
| npc_gruta | Gruta Panela-Funda | Tavern | town_gruta | ShopKeeperFixed/TavernStage | npc_gruta_home | NpcScenePlacementMarker (WAVE12C) |
| npc_yael | Yael Noite-Mansa | NightMarket | town_yael | NightOnly/WanderHidden | npc_yael_home | NpcScenePlacementMarker (WAVE12C) |
| npc_dagna | Dagna Rocha-Morna | Quarry/MineOffice | town_dagna | Patrol/QuarryRoad | npc_dagna_home | NpcScenePlacementMarker (WAVE12C) |
| npc_alaric | Ser Alaric Veyr | GuardPost | town_alaric | Patrol/TownGate | npc_alaric_home | NpcScenePlacementMarker (WAVE12C) |
| npc_mirela | Mirela dos Lacos | Tailor | town_mirela | ShopKeeperFixed | npc_mirela_home | NpcScenePlacementMarker (WAVE12C) |
| npc_eiran | Eiran Valeclaro | AnimalYard | town_eiran | WanderWithinZone/AnimalArea | npc_eiran_home | NpcScenePlacementMarker (WAVE12C) |
| npc_liora | Liora Canta-Rio | Tavern/StatueGarden | town_liora | WanderWithinZone/EveningStage | npc_liora_home | NpcScenePlacementMarker (WAVE12C) |
| npc_orlan | Orlan Pouso-Curto | Inn | town_orlan | ShopKeeperFixed | npc_orlan_home | NpcScenePlacementMarker (WAVE12C) |
| npc_savra | Savra Escama-Verde | Herbalist/ForestGate | town_savra | Patrol/HerbRoute | npc_savra_home | NpcScenePlacementMarker (WAVE12C) |
| npc_tovin | Tovin Maos-de-Selo | Registry | town_tovin | Stationary/PermitDesk | npc_tovin_home | NpcScenePlacementMarker (WAVE12C) |
| npc_maelor | Maelor Cinza | StatueGarden/NightRoute | town_maelor | NightOnly/WanderHidden | npc_maelor_home | NpcScenePlacementMarker (WAVE12C) |

## Anchor Strategy

NpcScheduleService uses NpcDataSO.DefaultPosition as the HomeAnchor position.
NpcScheduleAnchor MonoBehaviour can be placed in scene for named positions (optional; Unity Editor wiring).
On DayStartedEvent: NpcScheduleService sets NPC transform.position to their DefaultPosition (home anchor fallback).
Player-reachable: all 23 declared YES in WAVE12C placement map — no change.

## Debt

SCENE_WIRING_DEBT: NpcScheduleAnchor GameObjects must be placed in TownScene by human in Unity Editor.
Without scene anchors, positions fall back to NpcDataSO.DefaultPosition (safe fallback).
