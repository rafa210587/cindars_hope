# Non-Regression Review — Cidade Coerente Preservation-First

> Date: 2026-07-01
> Spec: `spec_city_preservation_first_coherent_relayout`
> Result: PASS_WITH_DEFERRED_HUMAN_PLAYMODE

| Dimensão | Resultado | Evidência |
|---|---|---|
| File & Directory | PASS | runtime/cena restritos ao escopo City/TownScene; nenhum `spec/`, `specs/` ou `docs_old` criado |
| Git safety | PASS | nenhum push, reset, clean, stash, merge ou remoção de branch |
| Runtime APIs | PASS | nenhuma adição de `GameObject.Find`, `FindObjectOfType` ou `FindObjectsByType` |
| Save DTO safety | N/A | save e DTOs não alterados |
| Balance values | PASS | horários existentes preservados; geometria na fonte editorial `TownCityLayout` |
| Event bus | N/A | nenhum evento/subscriber novo |
| Namespace | PASS | nenhum namespace `CindarsHope.Debug` |
| Status integrity | PASS | não marcado ACCEPTED/PLAYMODE_VALIDATED |
| Testing Quality Gate | PASS_WITH_DEFERRED | 22/22 EditMode + validator 21/21; Play Mode documentado |

## Existing Systems Audit

Searched: `TownLayout`, `TownHouseSpecs`, `NpcScheduleAnchor`, `NpcScheduleBlockResolver`,
`HouseDoorInteractable`, `RoofRevealController`, `WorldSpriteLibrary`, `WorldTilemapGround`.

- Reutilizados gerador, portas, roof reveal, schedule service, anchors, waypoints, fallback, sprites e tilemap.
- Criada somente `TownCityLayout`, fonte editorial determinística de lotes/vias/destinos.
- Nenhum manager, service, event bus, save provider ou sistema paralelo criado.

## Preservation evidence

```text
House_*: 24 -> 24
HouseDoorInteractable: 24 -> 24
RoofReveal: 24 -> 24
Stall_npc_*: 23 -> 23
MarketSquare_Stall_*: 6 -> 6
TownTree_*: 334 -> 497
Schedule anchors: 84 (28 NPCs × work/social/home)
Stable spawns: town_default + town_from_farm present
```

No changes detected in inventory, items, combat, enemies, save, Packages or ProjectSettings.

## Residual risk

Composição visual, clearance real e movimento ao longo do dia dependem do Play Mode humano. Status
máximo: `BUILD_VALIDATED_PENDING_HUMAN_PLAYMODE`.
