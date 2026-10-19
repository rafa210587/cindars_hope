# SPEC — Calendar Public Board Forecast Secret Visibility Future Runtime

> **Spec ID:** `15_spec_calendar_public_board_forecast_secret_visibility_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 15 — Calendar / Festival / Weather / Lunar Future Systems  
> **Priority:** P1  
> **Type:** Runtime / Future / Calendar / Forecast / Visibility  
> **Domain:** Calendar / Public Board / Weather Forecast / Lunar Visibility / Secret Event Filtering  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_15_CALENDAR_WEATHER_LUNAR_FUTURE  
> **Can run with:** seasonal economy/event specs if lock scopes do not overlap.  
> **Must not run with:** qualquer spec que altere time/day transition core, UI prefab/layout, quest state, NPC schedule concrete data, save migration ou pet runtime.  
> **Repo lock scope:** `Assets/_Game/Scripts/World/Calendar/**`, `Assets/_Game/Scripts/UI/Calendar/**`, `Assets/_Game/Scripts/World/Weather/**`, `Assets/_Game/Scripts/World/Lunar/**`, `Assets/_Game/Tests/EditMode/World/**`, `docs/validation/15_spec_calendar_public_board_forecast_secret_visibility_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
> **Blocks:**  
  - festival/event boards;
  - weather/lunar cave modifiers;
  - seasonal economy modifiers;
  - quest temporal conditions;
  - calendar UI.
> **Scope:** definir/endurecer calendário público, forecast tiers, known/hidden event projection e visibility policy para eventos sazonais, climáticos, lunares e de quest.  
> **Out of scope:** UI visual final, time core rewrite, concrete NPC schedules, quest content, save migration.

---

# /speckit.specify

## 1. Contexto

O calendário deve registrar dia, estação, ano, clima previsto, evento lunar conhecido, festivais, restock especial, encomendas com prazo, eventos de cidade conhecidos, eventos de quest descobertos e dias de loja fechada. Eventos secretos não aparecem até serem descobertos.

---

## 2. Problema

Sem visibility/forecast contract:

```text
evento secreto aparece no calendário cedo;
lua oculta revela condição de Mana;
quest não iniciada aparece por data;
jogador não sabe quando festival acontece;
forecast mostra clima impossível/oculto;
calendário vira fonte de verdade de quest;
loja fechada/festival não tem aviso;
bloqueio temporal parece bug.
```

---

## 3. Objetivo

Criar/endurecer:

```text
CalendarPublicEntry;
CalendarVisibilityPolicy;
ForecastTier;
WeatherForecastEntry;
LunarForecastEntry;
SecretEventVisibilityState;
CalendarSourceType;
KnownEventProjection;
CalendarBoardService.
```

---

## 4. Regras de design

```text
Calendário mostra eventos públicos e descobertos.
Eventos secretos ficam ocultos até descoberta.
Forecast nível 1 mostra amanhã no quadro público.
Forecast avançado exige upgrade/serviço/quest/NPC/artefato.
Eventos que bloqueiam progresso precisam ser descobertos, sinalizados ou previsíveis.
Calendário não é fonte de QuestState.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero ver festivais, clima e eventos conhecidos.
Como quest, quero esconder evento temporal até a descoberta.
Como UI, quero mostrar previsão sem spoiler.
Como economia, quero anunciar restock/evento quando público.
Como designer, quero fontes de calendário por praça, quadro, loja, taverna ou Guilda.
```

---

## 6. Escopo

Inclui:

```text
public board projection;
forecast tiers;
known/secret event visibility;
sources of calendar information;
quest-discovered event hooks;
known lunar/weather projection;
tests.
```

Não inclui:

```text
time/day transition core;
festival minigame runtime;
shop stock runtime;
UI prefab/layout;
save migration.
```

---

## 7. Modelo de domínio

### 7.1 CalendarPublicEntry

```text
EntryId
Date
Season
Year
EntryType: Weather | Lunar | Festival | Restock | OrderDeadline | CityEvent | QuestEvent | ShopClosure
DisplayNameKey
DisplaySummaryKey
VisibilityState
SourceType
SpoilerTier
RequiresDiscoveryFlag optional
RequiresQuestState optional
CanShowExactDate
CanShowExactTime
CanShowLocation
```

### 7.2 ForecastTier

```text
None
PublicTomorrow
ServiceTwoThreeDays
AdvancedLunarWeather
QuestArtifact
Debug
```

### 7.3 CalendarSourceType

```text
TownSquareCalendar
PublicBoard
SeedShop
TownHall
TavernRumor
RoadsGuild
PlayerUIKnown
NpcDialogue
QuestDiscovery
Debug
```

### 7.4 SecretEventVisibilityState

```text
Hidden
Rumored
KnownApproximate
KnownDate
KnownDateAndTime
Resolved
Expired
```

### 7.5 CalendarBoardQuery

```text
CurrentDate
LookaheadDays
AllowedForecastTier
KnownQuestFlags[]
KnownLunarEvents[]
KnownWeatherSources[]
RequestingContext
```

---

## 8. Projection rules

```text
Public festivals:
  visible with date/time/location if public.

Weather tomorrow:
  visible after interacting with public board.

Weather 2-3 days:
  requires service/upgrade.

Lunar known event:
  visible if lunar effect discovered or public tradition says so.

Secret quest event:
  hidden until discovery.

Mana condition:
  hidden until lore gate and source allows.

Shop closure:
  visible if public/store announced.

Order deadline:
  visible if order accepted or public board lists it.
```

---

## 9. Criteria

```text
Calendar projection exists.
Secret event visibility is enforced.
Forecast tiers exist.
Known event sources are represented.
No hidden quest/Mana/lunar spoiler leak.
Tests cover public festival, hidden secret, weather tomorrow, advanced forecast blocked, known lunar event, shop closure and quest event discovery.
```

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/World/Calendar/CalendarPublicEntry.cs
Assets/_Game/Scripts/World/Calendar/CalendarVisibilityPolicy.cs
Assets/_Game/Scripts/World/Calendar/CalendarSourceType.cs
Assets/_Game/Scripts/World/Calendar/SecretEventVisibilityState.cs
Assets/_Game/Scripts/World/Weather/ForecastTier.cs
Assets/_Game/Scripts/World/Calendar/CalendarBoardService.cs
Assets/_Game/Scripts/UI/Calendar/CalendarBoardProjection.cs
Assets/_Game/Tests/EditMode/World/CalendarPublicBoardForecastTests.cs
```

Consolidar existentes se houver.

---

## 11. Arquivos permitidos

```text
Assets/_Game/Scripts/World/Calendar/**
Assets/_Game/Scripts/World/Weather/**
Assets/_Game/Scripts/World/Lunar/**
Assets/_Game/Scripts/UI/Calendar/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/World/**
docs/validation/15_spec_calendar_public_board_forecast_secret_visibility_future_runtime_execution_report.md
```

## 12. Arquivos proibidos

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

# /speckit.tasks

## 13. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar calendar/weather/lunar/UI systems.
- [ ] T003 — Consolidar entry/source/visibility/forecast.
- [ ] T004 — Implementar projection service.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

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

- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md

### Required interpretation

```text
Esta spec deriva de SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.
Ela não redefine rotinas individuais de NPC, layouts, crops finais, preços finais, questlines completas, cave generation ou UI visual final.
Ela implementa apenas o recorte future/mapped declarado no escopo.
Eventos secretos, luas ocultas, eventos de quest não iniciados e condições exatas de Mana antes da lore não podem ser revelados.
Pets permanecem deferidos; qualquer menção a pet é compatibilidade/guardrail, não runtime.
Quando houver conflito com City, Farm, Quest, Economy, Cave, UI ou Save directions, o executor deve parar e registrar CONFLICT.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Tempo/calendário/clima/luas conectam fazenda, cidade, caverna, economia, Fonte, Mana, quests, NPCs e eventos.
- Tempo cria decisão, mas não deve impedir o jogador de jogar.
- Calendário público pode registrar dia, estação, ano, clima previsto, evento lunar conhecido, festivais, restock especial, encomendas com prazo, eventos de cidade conhecidos, eventos de quest descobertos e dias de loja fechada.
- Eventos secretos não aparecem até serem descobertos.
- Previsão pode vir de calendário/quadro público, NPC/serviço/observação/upgrade.
- Luas podem afetar mundo antes do jogador entender, mas efeitos que bloqueiam progressão precisam ser descobertos, sinalizados ou previsíveis.
- Economia pode variar por restock semanal/sazonal, itens sazonais, festival, mercador raro, loja noturna, sementes raras, itens mágicos instáveis e demanda temporária.
- Evitar arbitragem fácil, preço opaco, restock ao abrir menu, reset de loja sem day transition, economia quebrada por Mana ou evento lunar gerando dinheiro infinito.

### Deferred / future from directions

- Astronomia real completa.
- UI visual final de calendário/clima/lua.
- Eventos específicos finais de cada festival.
- Balance final de preços/crops/economia.
- Questlines completas.
- Pet runtime.
- Romance/social deep runtime.
- Scene/prefab/tilemap edits.
- Full cave procedural changes.

### Explicitly not redefined here

- NPC schedules concretos.
- Crop growth formulas finais.
- Pricing formula final.
- Enemy stats/spawns finais.
- QuestState/MainProgression/FonteAnya ownership.
- Pet systems.
- Companion systems.
- UI prefab/layout final.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu world/calendar/weather/lunar e domains afetados? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a calendar public board/forecast/secret visibility foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Anti-spoiler | Eventos secretos, luas ocultas, Mana, Anya, Pedra Negra, 100/101 e final choice não vazam? | Tests/checklist. | PARTIAL |
| Anti-softlock | Evento temporal/lunar/climático não bloqueia progresso essencial sem pista/controle razoável? | Tests/checklist. | PARTIAL |
| Anti-economia | Variações de evento não criam arbitragem, restock indevido ou dinheiro infinito. | Tests/checklist. | PARTIAL |
| Pet deferido | Não há runtime pet. | Checklist explícito. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/15_spec_calendar_public_board_forecast_secret_visibility_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "CalendarPublic|CalendarBoard|ForecastTier|WeatherForecast|LunarForecast|SecretEvent|ShopClosure|QuestEvent" Assets/_Game/Scripts docs/design .specs
rg -n "Calendar|Season|Weather|Festival|Forecast|Lunar|Alihana|Senya|Nyx|Restock|Seasonal|RareMerchant|NightShop|CaveModifier|Mana|Fonte|BlackStone|PedraNegra|SecretEvent|Pet|Companion" Assets/_Game/Scripts docs/design .specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" .specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
```

Classificar achados:

```text
EXISTING_CANONICAL
EXISTING_PARTIAL
MISSING_SAFE_TO_CREATE
MISSING_BUT_DEFER
CONFLICT
```

---

## 23C. Functional Acceptance Scenarios

### Scenario 1 — Happy path

```text
Given o estado de calendário/clima/lua/evento é conhecido ou descoberto
When o sistema desta spec projeta ou aplica seu recorte
Then somente efeitos autorizados e sinalizados ocorrem
And nenhum evento secreto/lore sensível é revelado cedo
And economia, quest, Fonte, Mana, cave e city schedules respeitam seus domínios de origem.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Anti-spoiler temporal

```text
Given evento secreto, lua oculta, condição exata de Mana, evento de quest não iniciado, Pedra Negra, Anya, Arquivista, nível 100/101 ou final choice
When calendário/UI/forecast/economy/cave consulta eventos futuros
Then informação fica oculta, genérica ou gated
And só aparece quando descoberta/autorizada.
```

### Scenario 4 — Anti-softlock

```text
Given uma quest/progressão depende de clima, festival ou lua
When a condição não está ativa
Then o jogador tem pista, previsão, janela futura, alternativa ou wait/retry claro
And progresso essencial não fica bloqueado silenciosamente.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay de calendário, festival, forecast, cave modifier, shop/event board ou notification
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Secret event shown early.
- Mana condition shown before lore.
- Quest event not initiated appears in calendar.
- Forecast tier shows advanced lunar/weather data for free.
- Shop closure/festival not visible when public.
- Calendar mutates QuestState.
- Hidden event blocks progression silently.
- UI shows debug IDs.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Calendar Public Board Forecast Secret Visibility Future Runtime

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

## Calendar/weather/lunar compliance
- Secret events hidden:
- Known events visible:
- Quest conditions signaled:
- Economy anti-arbitrage:
- No pet runtime:
- No romance/social deep runtime:
- No scene/prefab/tilemap:
- Save/load safe:
- UI projection not source of truth:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Anti-spoiler temporal:
- Anti-softlock:
- Anti-economy exploit:
- Save/load:
- UI/final scenario:

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
2. A implementação exigir scene/prefab/tilemap/asset wiring fora do escopo.
3. A implementação exigir save migration sem spec de migration.
4. A implementação revelar evento secreto/quest/lua/Mana/Pedra Negra/100/101/final cedo.
5. A implementação bloquear progressão essencial por clima/lua/festival sem pista/previsão/alternativa.
6. A implementação criar arbitragem, restock por abrir menu, preço opaco ou dinheiro infinito.
7. A implementação criar runtime pet.
8. A implementação criar romance/social deep runtime.
9. A implementação redefinir NPC schedules concretos, crop formulas, prices, enemy spawns ou cave generation.
10. Não for possível decidir se sistema existente é canônico ou obsoleto.
```



---

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Calendar|Season|Weather|Festival|Forecast|Lunar|Alihana|Senya|Nyx|Restock|Seasonal|RareMerchant|NightShop|CaveModifier|Mana|SecretEvent" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de calendário, forecast, festival, shop/event board, cave weather/lunar modifier ou notification, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, visibility/forecast projection logic is deterministic.
- Requires EditMode tests: YES for public/hidden/forecast-tier/lunar/quest/shop tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for calendar board UI validation.
- Requires regression test: YES if fixing existing calendar visibility bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no secret event/Mana/lunar spoiler leak.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/15_spec_calendar_public_board_forecast_secret_visibility_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não revelar evento secreto cedo.
Não revelar Mana/Anya/Pedra Negra/100/101/final cedo.
Não bloquear progresso sem pista.
Não criar economia infinita.
Não resetar loja ao abrir menu.
Não implementar Pet runtime.
Não implementar romance/social deep runtime.
Não editar scenes/prefabs/assets.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped. Deve ser executada apenas quando calendar/time, economy, city schedules, UI, quest and cave foundations estiverem estáveis ou quando houver decisão humana explícita de antecipar este bloco.
