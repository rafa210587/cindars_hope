# SPEC — City Layout Buildings Doors Beds Schedule Runtime

> **Spec ID:** `08_spec_city_layout_buildings_doors_beds_schedule_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 08 — City / NPC / Dialogue / Services  
> **Priority:** P0  
> **Type:** Runtime / City / Layout Metadata / Buildings / Doors / Beds / Schedule  
> **Domain:** City / Layout / BuildingDefinition / DoorTrigger / Bed / NpcSchedule / Pathfinding Metadata  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_08_CITY_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere CityScene tilemaps/prefabs, NPCData contract, dialogue runtime, service opening hours, festival schedule, farm visit runtime, save schema ou pathfinding implementation massiva.  
> **Repo lock scope:** `Assets/_Game/Scripts/City/**`, `Assets/_Game/Scripts/NPC/**`, `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/City/**`, `docs/validation/08_spec_city_layout_buildings_doors_beds_schedule_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
> **Blocks:**  
  - city scene/prefab setup;
  - door/interior transitions;
  - NPC schedule movement;
  - shop opening hours;
  - festival overrides;
  - farm visits.
> **Scope:** definir/endurecer contratos de layout de cidade, building definitions, door triggers, beds, schedule periods, schedule modifiers e metadata de navegação sem editar cenas/prefabs.  
> **Out of scope:** edição de tilemaps/prefabs, pathfinding completo, festival full implementation, interiors art, NPC animation, full schedule AI.

---

# /speckit.specify

## 1. Contexto

City Layout Direction define escala visual 32x32 tiles, player/NPC 32x48, cidade externa 128x96, zonas, prédios, interiores, camas, portas, períodos do dia, agenda padrão e modificadores por clima, luas e festival.

Esta spec cria contratos runtime/data para esses elementos. Não edita CityScene, tilemaps, prefabs ou assets.

---

## 2. Problema

Sem contrato de layout/schedule:

```text
porta pode apontar para cena/spawn inválido;
NPC pode não ter cama/home/work;
loja fechada pode não mostrar horário;
festival pode suspender agenda sem regra;
Yael/loja noturna pode abrir em horário errado;
NPC pode teleportar visível sem regra;
camas de NPC podem ser usadas pelo jogador indevidamente;
pets podem ser puxados por props de cidade antes da hora.
```

---

## 3. Objetivo

Criar/endurecer:

```text
CityZoneDefinition;
CityBuildingDefinition;
DoorTriggerDefinition;
BedDefinition;
NpcScheduleDefinition;
SchedulePeriod;
ScheduleModifier;
LocationId/SpawnPointId;
OpenHoursRule;
Navigation metadata;
validators/tests.
```

---

## 4. Regras de design

```text
Cidade externa: 128x96 tiles.
Tile base: 32x32.
Player/NPC comum: 32x48.
Collider é footbox inferior, não sprite inteiro.
NPCs usam waypoints por agenda.
Offscreen NPC pode trocar estado por schedule tick.
Festival suspende agenda normal.
Loja noturna depende de condição.
Cama de NPC é objeto de schedule, não cama do player.
```

---

## 5. User stories / engineering stories

```text
Como player, quero portas funcionando com locked message/open hours.
Como NPC schedule, quero home/work/bed/local por período.
Como loja, quero horário e fechamento consistentes.
Como cidade, quero festival/weather/lunar modifiers.
Como validator, quero impedir door/bed/schedule quebrados.
```

---

## 6. Escopo

Inclui:

```text
city zones/buildings metadata;
door trigger contract;
bed contract;
schedule periods;
NPC schedule definition;
schedule modifiers for rain/Alihana/Senya/Nyx/festival;
offscreen tick vs visible walking policy;
validators/tests.
```

Não inclui:

```text
CityScene tilemap;
building prefabs;
interior art;
pathfinding implementation complete;
NPC animation;
festival implementation final;
pet behavior.
```

## Source Map Compliance

### Global sources read

- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
- .specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
- .specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- .specs/SPEC_VALIDATION_MATRIX_MASTER.md
- .specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
- docs/project/CURRENT_STATE.md

### Domain directions read

- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
Quando houver divergência entre esta spec e os directions, o executor deve parar e registrar CONFLICT no execution report.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Toda spec de cidade que envolva mapa, cena, NPC, loja, rotina, pathfinding, residência, cama, interior, horário, festival ou visita à fazenda deve ler City Layout.
- Tile base é 32x32, player visual 32x48, NPC comum 32x48 e collider usa footbox.
- Cidade externa recomendada: 128x96 tiles.
- Lista canônica de zonas inclui praça, mercado, ofícios, taverna, templo, jardim, cartório, guilda, caravançará, caminho da fazenda, entrada da caverna, beco, ruína e residências.
- Lista canônica de construções inclui templo, cartório, sementes, loja geral, forja, carpintaria, alquimia, taverna, guilda, arquivo, costura, rancho, ervas, loja noturna, jardim e caverna.
- Períodos do dia são Morning, WorkStart, Midday, WorkAfternoon, Evening, Night e Sleep/LateNight; chuva, Alihana, Senya, Nyx e festival modificam agendas.

### Deferred / future from directions

- Tilemap/prefab placement final.
- Pathfinding completo.
- Interior scenes final.
- Festival content final.
- Pet props runtime.
- Full NPC movement animation.

### Explicitly not redefined here

- NPCData contract.
- World time/calendar/weather/lunar runtime.
- Door transition service.
- Shop open/closed UI.
- Farm visit runtime.
- Save schema migration.

## 7. Modelo de domínio

### 7.1 CityZoneDefinition

```text
ZoneId
DisplayName
ApproxTileOrigin
ApproxTileSize
FunctionTags[]
AllowedBuildingIds[]
NavigationPriority
FestivalEligible
NightActivityEligible
LoreSensitive
```

### 7.2 CityBuildingDefinition

```text
BuildingId
DisplayName
ZoneId
ExternalSizeTiles
ExternalSizePx
InteriorSizeTiles
InteriorSizePx
OwnerNpcIds[]
ResidentNpcIds[]
ServiceTags[]
DoorTriggerIds[]
BedIds[]
OpenHoursRule optional
LoreFlags[]
IsHidden
```

### 7.3 DoorTriggerDefinition

```text
DoorTriggerId
SourceLocationId
TargetSceneId
TargetSpawnPointId
RequiredState optional
LockedMessage
OpenHoursRule optional
DoorSizeTiles
DoorSizePx
RequiresRelationship optional
RequiresQuestFlag optional
IsHiddenDoor
```

### 7.4 BedDefinition

```text
BedId
OwnerNpcIds[]
LocationId
BedType: Simple | Couple | Guest | Hidden | ScheduleOnly
ScheduleOnly
CanPlayerUse
SizeTiles
SizePx
```

### 7.5 SchedulePeriod

```text
Morning         06:00-09:00
WorkStart       09:00-12:00
Midday          12:00-14:00
WorkAfternoon   14:00-18:00
Evening         18:00-21:00
Night           21:00-00:00
SleepLateNight  00:00-06:00
```

### 7.6 NpcScheduleDefinition

```text
ScheduleId
NpcId
DefaultPeriodBlocks[]
WeatherModifiers[]
LunarModifiers[]
FestivalOverrides[]
QuestOverrides[]
RelationshipOverrides[]
FallbackWaypointId
VisibleMovementPolicy
OffscreenTickPolicy
```

---

## 8. Door/open hour rules

```text
Door common size: 2x2 tiles / 64x64 px.
Temple/guild door can be 3x3 / 96x96 px.
Closed shop door shows hours.
Private house can require relationship/quest.
Inn allows broader access.
Night shop is conditionally active.
Hidden doors require explicit state/quest/lore.
```

---

## 9. Bed rules

```text
NPC beds are schedule markers.
Player cannot use NPC bed except guest beds if inn/lodging is implemented.
Couple bed supports two married NPCs.
Pip bed is not simulated initially.
Hidden beds/locations require hidden state.
```

---

## 10. Schedule modifiers

```text
Rain:
  Sylveth shop more; Eiran ranch; Pip fewer external deliveries; Yael may open earlier if Nyx night; Maelor less visible.

Alihana:
  Liora goes to Garden in Evening; Thalindra may go at night; statues subtle trigger.

Senya:
  Tavern fuller; Gruta food/festival; Ozzra unstable item; Gurd conflict chance.

Nyx:
  Yael opens night shop; Maelor visible routes; fewer common NPCs; Garden event; Alaric patrols more.

Festival:
  normal agenda suspended; NPCs central/theme area; normal shops closed; special stalls open.
```

---

## 11. Movement policy

```text
NPCs use waypoints by schedule.
Visible NPCs walk smoothly between nearby waypoints if supported.
Offscreen/different scene NPCs can tick/teleport state.
When player enters scene, NPC appears at waypoint matching time/state.
Blocked path uses fallback waypoint.
Large NPCs use same graph with larger footbox.
```

---

## 12. Criteria

```text
City zones/building/door/bed/schedule contracts exist.
Door target/spawn/open hours can be validated.
NPC schedule references valid locations/waypoints/beds.
Festival/weather/lunar modifiers are explicit.
NPC bed cannot be used by player unless guest rule.
No scene/prefab edits.
Tests cover door validation, bed ownership, schedule period, modifiers and hidden/night shop cases.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/City/Layout/CityZoneDefinition.cs
Assets/_Game/Scripts/City/Layout/CityBuildingDefinition.cs
Assets/_Game/Scripts/City/Layout/DoorTriggerDefinition.cs
Assets/_Game/Scripts/City/Layout/BedDefinition.cs
Assets/_Game/Scripts/City/Schedule/SchedulePeriod.cs
Assets/_Game/Scripts/City/Schedule/NpcScheduleDefinition.cs
Assets/_Game/Scripts/City/Schedule/NpcScheduleResolver.cs
Assets/_Game/Scripts/City/Validation/CityLayoutScheduleValidator.cs
Assets/_Game/Tests/EditMode/City/CityLayoutScheduleValidationTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/City/**
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/City/**
docs/validation/08_spec_city_layout_buildings_doors_beds_schedule_runtime_execution_report.md
```

---

## 15. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Pets/**
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 16. Estratégia

```text
1. Auditar City layout/schedule/door systems.
2. Consolidar zone/building/door/bed contracts.
3. Consolidar schedule periods and modifiers.
4. Implementar validators/resolver mínimo.
5. Criar tests.
6. Criar report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - NPCData contract;
  - city scene/prefab wiring;
  - door transition service;
  - shop open hours;
  - festival runtime.
- Reason: layout/schedule metadata is shared by all city systems.

---

## 18. Impacto save/load

```text
Does this change save schema? SHOULD BE NO.
Does this add save section? NO.
Does this require migration? NO unless persistent NPC location overrides are added; then STOP.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. NpcSchedulePeriodChangedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 20. Impacto UI/Unity

```text
Changes UI: NO final; exposes locked/open hours messages.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for door/schedule/city navigation flow.
```

---

## 21. Riscos

```text
Risco: scene/prefab needed.
Mitigação: data contracts only; STOP if wiring required.

Risco: schedule teleports visible NPC.
Mitigação: visible/offscreen policy.

Risco: hidden/night shop always visible.
Mitigação: open condition validators.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar city layout/schedule/doors.
- [ ] T003 — Consolidar layout contracts.
- [ ] T004 — Consolidar schedule contracts.
- [ ] T005 — Implementar validators.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a city layout/buildings/doors/beds/schedule foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de city layout/buildings/doors/beds/schedule? | Arquivos alterados e justificativa. | PARTIAL |
| Canon de cidade | Kanthor/Anya/classes funcionais/sem D&D/sem Fôlego foram preservados? | Checklist no report. | PARTIAL |
| Pets | A execução não criou runtime/UI/save/data asset de pets? | Checklist explícito no report. | BLOCKED se violar |
| Romance | A execução não criou romance/casamento profundo fora do escopo? | Checklist explícito no report. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/08_spec_city_layout_buildings_doors_beds_schedule_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "CityZone|CityBuilding|DoorTrigger|BedId|NpcSchedule|SchedulePeriod|OpenHoursRule|NightShop|FestivalOverride|Waypoint|Footbox" Assets/_Game/Scripts docs/design .specs
rg -n "NPC|NpcData|Schedule|Dialogue|Rumor|Service|Shop|Contract|License|DoorTrigger|BedId|FarmVisit|Relationship|Romance|Pet|Kanthor|Anya|Folego|Breath" Assets/_Game/Scripts docs/design .specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" .specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
```

A execução deve classificar cada achado como:

```text
EXISTING_CANONICAL
  Sistema já existe e deve ser reaproveitado/endurecido.

EXISTING_PARTIAL
  Sistema existe, mas precisa hardening/delta.

MISSING_SAFE_TO_CREATE
  Sistema não existe e criação é pequena, isolada e dentro do escopo.

MISSING_BUT_DEFER
  Sistema não existe, mas criação exigiria outro domínio/spec.

CONFLICT
  Há dois caminhos possíveis ou contrato divergente. Parar e reportar.
```

---

## 23C. Functional Acceptance Scenarios

### Scenario 1 — Happy path

```text
Given o sistema base relacionado a city layout/buildings/doors/beds/schedule existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue o direction canônico
And save/load, agenda, serviços, diálogo e UI projection permanecem consistentes
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Canon protection

```text
Given a spec toca NPC, serviço, religião, cidade, diálogo ou agenda
When validar os dados
Then templo público principal permanece Kanthor
And Anya não ganha templo/altar ativo independente
And NPCs usam classes funcionais do jogo, não classes de D&D
And Fôlego/Breath/BR não é criado como stat de NPC
And pets permanecem deferidos.
```

### Scenario 4 — Invalid state

```text
Given NPC, agenda, serviço, diálogo, farm visit, porta, cama, licença ou contrato inválido
When validator/teste é executado
Then a violação é reportada com motivo claro
And o runtime não corrompe save, não libera serviço indevido e não bloqueia progresso sem fallback.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de cidade, NPC, porta, interior, serviço, diálogo ou visita à fazenda
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Door target scene/spawn invalid.
- NPC without home/work/bed where required.
- NPC visible teleport without policy.
- Night shop active in daytime.
- Festival does not suspend normal schedule.
- NPC bed usable by player accidentally.
- Scene/prefab edit required.
- Save schema altered without migration.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — City Layout Buildings Doors Beds Schedule Runtime

## Summary
- Spec:
- Branch:
- Executor:
- Date:
- Final status:

## Sources read
- ...

## Local audit
- Commands executed:
- Existing systems found:
- Existing partial systems found:
- Missing systems:
- Conflicts:

## Canon compliance
- Kanthor public temple preserved:
- Anya altar/temple not created:
- Functional classes used:
- Folego/Breath/BR not created:
- Pets not implemented:
- Deep romance/marriage not implemented:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Canon protection:
- Invalid state:
- Save/load safety:
- Visual/final scenario:

## Validation
- Docs validation:
- C# build:
- Unity compile:
- EditMode tests:
- PlayMode automated:
- Final human scenario:

## Testing Quality Gate
- Changed deterministic logic:
- Requires EditMode tests:
- Requires PlayMode automated or final human scenario:
- Requires regression test:
- Human validation timing:
- Minimum validation evidence for ACCEPTED:

## Residual risks
- ...

## Next specs impacted
- ...
```

---

## 23F. Stop Conditions

Parar a execução e registrar `BLOCKED` se ocorrer qualquer um destes casos:

```text
1. A implementação exigir alterar Packages/ ou ProjectSettings/.
2. A implementação exigir scene/prefab/asset wiring fora do escopo.
3. A implementação exigir mudança de save schema sem migration spec.
4. A implementação exigir reescrever City/NPC/Dialogue/Shop/Quest system canônico existente.
5. A implementação criar templo/altar ativo independente de Anya.
6. A implementação usar classe de D&D como classe mecânica de NPC.
7. A implementação recriar Fôlego/Breath/BR como stat/recurso/campo de NPC.
8. A implementação criar pets runtime/UI/save/data assets.
9. A implementação criar romance/casamento profundo ou conteúdo explícito.
10. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. City Schedule Modifier Matrix

| Modifier | Expected behavior |
|---|---|
| Rain | outdoor schedules reduce/alter |
| Alihana | Garden/statue dream/lore routes |
| Senya | Tavern/festival/social routes |
| Nyx | night shop, Maelor, patrols, fewer NPCs |
| Festival | normal schedule suspended |
| Quest | explicit override only |
| Relationship | farm visit/social route only if unlocked |

## 23H. Door/Bed Safety Matrix

| Object | Validation |
|---|---|
| DoorTrigger | target scene/spawn + locked/open rule |
| Shop door | open hours + closed message |
| Hidden door | state/quest/lore gate |
| NPC bed | owner + ScheduleOnly |
| Guest bed | CanPlayerUse only if inn system |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "NPC|NpcData|Schedule|Dialogue|Rumor|Service|Shop|Contract|License|DoorTrigger|BedId|FarmVisit|Relationship|Romance|Pet|Kanthor|Anya|Folego|Breath" Assets/_Game/Scripts docs/design .specs
```

C# runtime/editor quando houver alteração C#:

```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

Unity compile quando houver alteração Unity C#:

```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

EditMode tests quando lógica determinística for criada/alterada:

```text
Unity Test Runner — EditMode, ou comando local equivalente disponível no repo.
```

PlayMode/final human validation:

```text
Não pedir validação humana por spec.
Quando houver cenário visual/gameplay de cidade, NPC, porta, interior, serviço, diálogo ou visita à fazenda, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, schedule/door/bed validation logic is deterministic.
- Requires EditMode tests: YES for door/bed/schedule/modifier/night shop/festival tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for CityScene door/schedule visual flow.
- Requires regression test: YES if fixing existing city door/schedule bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver CityScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no scene/prefab edits; schedule/door contracts valid.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/08_spec_city_layout_buildings_doors_beds_schedule_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não duplicar sistemas canônicos existentes.
Não quebrar save/load.
Não persistir Unity references.
Não usar UI como fonte de verdade.
Não criar templo/altar ativo independente de Anya.
Não usar classes de D&D como classes mecânicas.
Não recriar Fôlego/Breath/BR em NPC.
Não implementar pets.
Não implementar romance/casamento profundo.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
