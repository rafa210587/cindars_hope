# TownScene Preservation Baseline Manifest

> Spec: `spec_city_preservation_first_coherent_relayout`
> Captured before TownScene regeneration: 2026-07-01
> Scene source: `Assets/_Game/Scenes/TownScene.unity`
> Generator source: `Assets/_Game/Scripts/Editor/SceneCreation/CreateMvpTownScene.cs`

## Purpose

This manifest freezes the materialized TownScene contracts that the relayout may move or enrich but
must not remove. Counts discovered after this capture also become preservation floors.

## Baseline counts

| Family | Before | Preservation rule |
|---|---:|---|
| `House_*` | 24 | after >= before; stable names retained |
| `HouseDoorInteractable` | 24 | after >= before |
| `RoofReveal` | 24 | after >= before |
| `Stall_npc_*` | 23 | after >= before; NPC association retained |
| `MarketSquare_Stall_*` | 6 | after >= before |
| total stalls | 29 | after >= before |
| `TownTree_*` | 334 baseline / 497 v8 floor / 1153 current | six irregular exterior bands expand the perimeter without reducing visuals |
| canonical scheduled NPCs | 28 | three anchors per NPC retained |
| spawn `town_default` | 1 | ID retained |
| spawn `town_from_farm` | 1 | ID retained |

## Stable buildings

```text
House_AlchemyLab
House_AnimalYard
House_Archive
House_Bakery
House_Blacksmith
House_CarvalhoTorto
House_Chamber
House_Dagna
House_Fishery
House_GateKeeper
House_Inn
House_Manor
House_MarketHall
House_Pip
House_Prison
House_Registry
House_Residential_1
House_Residential_2
House_Residential_3
House_Residential_4
House_Tannery
House_Temple
House_Tovin
House_Workshop
```

## NPC role and movement contracts

The implementation retains the existing runtime schedule windows:

| Archetype | Work/social/home contract |
|---|---|
| shopkeeper | work 09:00–18:00; social 18:00–22:00; home 22:00–09:00; deep night 00:00–06:00 |
| guard/patrol | duty 06:00–22:00; night 22:00–06:00 |
| night | work 20:00–02:00; home 02:00–20:00 |
| wanderer | social/route 08:00–20:00; home/night 20:00–08:00 |

| NPC | Role | Work/route identity | Home contract |
|---|---|---|---|
| Corvus | priest of Kanthor | temple and temple frontage | `House_Temple` |
| Mara | registrar | Registry desk/civic plaza | `House_Registry` |
| Tovin | clerk and licenses | permits/records | `House_Tovin` |
| Sylveth | seeds and horticulture | seed service/garden | `House_Residential_1` |
| Renko | general merchant | general store/market | `House_Residential_2` |
| Mirela | tailor | tailor workshop/market | `House_Residential_3` |
| Orlan | innkeeper | inn/reception | `House_Inn` |
| Gruta | tavern and kitchen | inn/tavern stage | `House_Inn` |
| Brumdar | blacksmith | forge | `House_Blacksmith` |
| Dagna | quarry/mining | quarry road/forge | `House_Dagna` |
| Hund | guard and builder | town road/construction | `House_CarvalhoTorto` |
| Thalindra | archivist | archive/research | `House_Archive` |
| Alaric | guard captain | west gate/town patrol | `House_GateKeeper` |
| Pip | messenger | entrance/deliveries | `House_Pip` |
| Nimble | carpenter | workshop | `House_Workshop` |
| Gurd | heavy construction | construction yard | `House_CarvalhoTorto` |
| Yael | night merchant | night market | hidden night tent |
| Maelor | night route | cemetery/statue garden | cemetery |
| Zrix | cave-road guide | cave road | cave mouth |
| Savra | herbalist | forest/herb route | `House_Residential_4` |
| Ozzra | alchemist | alchemy laboratory | `House_AlchemyLab` |
| Eiran | rancher | animal yard | `House_AnimalYard` |
| Liora | musician | statue garden/evening stage | statue garden |
| Velorin | village leader | Chamber/council | `House_Manor` |
| Sael | fisher | fishery/dock | `House_Fishery` |
| Mella | baker/miller | bakery | `House_Bakery` |
| Hess | tanner | tannery | `House_Tannery` |
| Tibbet | gravedigger/acolyte | cemetery/sacristy | cemetery |

## Landmark contracts

```text
Temple of Kanthor remains the public temple.
Cemetery retains crypt, at least 9 graves, fence/gate and Maelor/Tibbet anchors.
House_Chamber remains the council/decision building.
TownHallBuilding remains a separate administrative/mural/notice-board landmark.
MarketHall and all 29 stalls remain materialized.
Central plaza, events plaza, statue garden, lake/park, cave mouth and night tent remain.
West farm gate, town_default and town_from_farm remain functional.
```

## Capture evidence

```powershell
rg -c "m_Name: House_" Assets/_Game/Scenes/TownScene.unity
rg -c "m_Name: Stall_npc_" Assets/_Game/Scenes/TownScene.unity
rg -c "m_Name: MarketSquare_Stall_" Assets/_Game/Scenes/TownScene.unity
rg -c "m_Name: TownTree_" Assets/_Game/Scenes/TownScene.unity
rg -c "HouseDoorInteractable" Assets/_Game/Scenes/TownScene.unity
rg -c "m_Name: RoofReveal" Assets/_Game/Scenes/TownScene.unity
```

## Files excluded from this execution

Inventory, starter items, ItemDatabase, WeaponDatabase, combat, enemies, Q/E input, save and HUD
are outside scope and must remain unchanged.
