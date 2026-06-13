# SPEC — Farm Forage Fishing Lake Runtime

> **Spec ID:** `05_spec_farm_forage_fishing_lake_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Resources / World Activities  
> **Priority:** P1  
> **Type:** Runtime / Farm / Forage / Fishing / Lake  
> **Domain:** Farm / Foraging / Lake / Fishing / Seasonal Spawns  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_RESOURCES_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere ResourceNode core, fishing minigame, lake layout, weather/season generation, inventory fish/forage definitions, economy values or FarmOrder delivery.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/Fishing/**`, `Assets/_Game/Scripts/Resources/**`, `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/05_spec_farm_forage_fishing_lake_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `.specs/a_implementar/05_spec_farm_resource_node_refresh_runtime.md`
  - `.specs/a_implementar/02_spec_calendar_season_year_runtime.md`
  - `.specs/a_implementar/02_spec_weather_generation_forecast_runtime.md`
> **Blocks:**  
  - resource node refresh;
  - calendar/season/weather runtime;
  - inventory fish/forage item definitions;
  - farm orders adapter;
  - economy pricing and quality.
> **Scope:** definir/endurecer forage sazonal e pesca/lago da fazenda como recursos leves, com spawns sazonais/climáticos e save/load seguro.  
> **Out of scope:** fishing minigame final, fish roster completo, festivals/competition, rare lunar fish, pet hints, NPC visits, lake scene/prefab/tilemap edits.

---

# /speckit.specify

## 1. Contexto

Roadmap 1 exige lago funcional e bosque com árvores. Roadmap 0 cita árvores/pesca/world activities como existentes/parciais. Farm Direction também descreve produção como crops, árvores, animais, pesca, recursos leves e itens processados.

Esta spec cobre forage sazonal e pesca básica do lago da fazenda como economia leve e rotina, não como minigame final.

---

## 2. Problema

Sem contrato de forage/fishing:

```text
forage pode respawnar todo reload;
fish pode ser capturado infinitamente sem tempo/energia;
season/weather podem ser ignorados;
lago pode substituir pesca/caverna/cidade avançada;
rare lunar fish pode aparecer cedo;
quality/rarity pode se perder em inventory;
forage/fishing pode completar FarmOrder indevidamente;
spawn pode ocorrer em building/fixed anchor.
```

---

## 3. Objetivo

Criar/endurecer contrato:

```text
ForageDefinition;
ForageSpawnState;
FishingSpotDefinition;
FishCatchTableRef;
Season/weather spawn gates;
Daily/seasonal refresh;
Inventory add with quality/rarity hooks;
No rare/lunar/endgame fish early;
Save/load spawn/catch state;
Tests/validators.
```

---

## 4. Regras de design

```text
Forage/fishing are light resources and daily variety.
Season/weather should matter without blocking essential progress.
Farm lake is accessible early and safe.
Rare lunar/arcane fish are future/gated.
Fishing should not create infinite gold per minute without cost/time/capacity.
Pet hints for forage/fishing are future and not implemented here.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero coletar foraging sazonal na fazenda.
Como jogador, quero pescar no lago funcional sem minigame final obrigatório agora.
Como sistema, quero spawn/catch respeitando estação/clima quando aplicável.
Como save/load, quero não duplicar forage/fish ao recarregar.
Como economy, quero que fishing/forage gere variedade, não exploit.
```

---

## 6. Escopo

Inclui:

```text
Forage spawn contract;
Fishing spot/catch contract;
season/weather gates;
daily/seasonal refresh;
quality/rarity hooks;
inventory add/overflow handling;
save/load state;
tests/validators.
```

Não inclui:

```text
fishing minigame final;
full fish roster;
lunar/arcane fish final;
pet hints;
festival competition;
visual/prefab/lake tilemap edits;
NPC fishing/social events.
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

- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md

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

- Roadmap 0 cita pesca/world activities existentes/parciais a preservar/refinar.
- Roadmap 1 inclui lago funcional.
- Farm production inclui pesca e recursos leves.
- World/time/clima/estações podem influenciar crops, eventos, economia e recursos.
- Economy define Fish e Forage como categorias com BaseValue inicial e variação por raridade/local/clima.
- Tempo cria decisão, mas não deve impedir o jogador de jogar.

### Deferred / future from directions

- Fishing minigame final.
- Fish roster completo.
- Rare lunar/arcane fish.
- Pet hints.
- Festival/competition fishing.
- NPC social fishing events.

### Explicitly not redefined here

- Inventory backend.
- Economy price final.
- Calendar/weather generation.
- FarmOrder reward pipeline.
- UI final/prefabs.

## 7. Modelo de domínio

### 7.1 ForageDefinition

```text
ForageId
DisplayName
ItemId
AllowedSeasons
AllowedWeather optional
AllowedZones
RequiredFarmLevel
SpawnPolicy
Rarity
BaseQualityRange optional
CanRespawnSameSeason
IsLoreProtected
IsEndgameReserved
```

### 7.2 ForageSpawnState

```text
ForageInstanceId
ForageId
TilePosition
ZoneId
CurrentState: Available | Collected | Hidden | Reserved
SpawnedDay
CollectedDay optional
NextEligibleSpawnDay optional
RandomSeed optional
```

### 7.3 FishingSpotDefinition

```text
FishingSpotId
DisplayName
ZoneId
WaterBodyId
AllowedSeasons
AllowedWeather optional
CatchTableId
RequiredRodTier optional
RequiredFarmLevel
DailyCatchSoftLimit optional
IsLoreProtected
```

### 7.4 FishCatchResult

```text
Success
FailureReason
FishItemId optional
Quantity
Quality optional
Rarity
TimeCost optional
Stamina/FatigueCost optional
InventoryAddResult
Events[]
```

---

## 8. Forage spawn rules

```text
Forage spawns only on valid free/natural tiles.
Forage does not spawn under buildings, crops, fixed anchors, cave/city exit, Fonte, lake water tile unless aquatic forage type.
Seasonal forage should clear/rotate by season when rules say.
Weather-triggered forage must use known weather context and saved seed/state.
Collected forage should not reappear on reload.
```

---

## 9. Fishing rules

```text
Farm lake fishing is safe and accessible early.
Fishing spot uses catch table; catch table decides fish/item.
Fishing may have time/stamina/fatigue hook.
Daily catch soft limit or diminishing returns can prevent infinite gold if needed.
Farm lake should not drop cave/endgame/lunar rare fish unless gated.
Inventory full must not silently delete catch.
```

---

## 10. Quality/rarity hooks

```text
Forage quality may depend on season, weather, farm level, rarity and seeded chance.
Fish quality may depend on rod tier, timing, weather, skill/future and seeded chance.
Quality must be stored on item if inventory supports it; otherwise MISSING_BUT_DEFER.
Rarity affects economy later, but final price formula is out of scope.
```

---

## 11. Save/load

Must preserve if applicable:

```text
ForageInstanceId and collected state.
Fishing daily catch counters if limits exist.
Random seed for spawn/catch if deterministic replay matters.
SpawnedDay/CollectedDay.
WaterBodyId/FishingSpotId stable IDs.
```

Must not persist:

```text
forage GameObject reference;
fish prefab reference;
lake tilemap object;
UI selection as state.
```

---

## 12. Criteria

```text
Forage spawns only in valid zones and respects season/weather gates.
Collected forage does not duplicate after reload.
Fishing returns valid catch/failure and respects inventory full.
Farm lake does not produce endgame/lunar rare fish early.
Quality/rarity are preserved or explicitly deferred.
Refresh/catch logic is testable and bounded.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Forage/ForageDefinition.cs
Assets/_Game/Scripts/Farm/Forage/ForageSpawnState.cs
Assets/_Game/Scripts/Farm/Forage/FarmForageSpawnService.cs
Assets/_Game/Scripts/Farm/Fishing/FarmFishingSpotDefinition.cs
Assets/_Game/Scripts/Farm/Fishing/FarmFishingService.cs
Assets/_Game/Scripts/Farm/Fishing/FishCatchResult.cs
Assets/_Game/Tests/EditMode/Farm/FarmForageFishingTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Fishing/**
Assets/_Game/Scripts/Resources/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/05_spec_farm_forage_fishing_lake_runtime_execution_report.md
```

---

## 15. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 16. Estratégia

```text
1. Auditar forage/fishing/world activity systems.
2. Consolidar forage spawn contracts.
3. Consolidar fishing spot/catch contracts.
4. Integrar season/weather gates.
5. Integrar inventory add/overflow.
6. Criar tests.
7. Criar report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - resource node refresh;
  - inventory quality schema;
  - calendar/weather runtime;
  - fish/forage economy;
  - FarmOrder delivery.
- Reason: forage/fishing consume shared world/economy/inventory systems.

---

## 18. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if world activity state exists; otherwise STOP.
Does this add save section? NO unless dedicated save spec approves.
Does this require migration? NO unless adding forage/fishing state; then STOP.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. ForageCollectedEvent, FishCaughtEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if event subscribers are created.
```

---

## 20. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for collect forage/fish lake flow.
```

---

## 21. Riscos

```text
Risco: fishing exploit.
Mitigação: soft limit/time/stamina/fatigue hooks.

Risco: forage duplicate after reload.
Mitigação: collected state/idempotency tests.

Risco: rare/endgame fish early.
Mitigação: catch table gate validation.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar forage/fishing systems.
- [ ] T003 — Consolidar forage definitions/states.
- [ ] T004 — Consolidar fishing spot/catch contracts.
- [ ] T005 — Integrar season/weather gates.
- [ ] T006 — Integrar inventory add/overflow.
- [ ] T007 — Criar tests.
- [ ] T008 — Rodar validações.
- [ ] T009 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | World activities, nodes, save e inventory existentes foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de forage/fishing/lake? | Arquivos alterados e justificativa. | PARTIAL |
| Resource refresh | A regeneração/respawn é determinística e não cria farm infinito sem regra? | Testes/validador/report. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/05_spec_farm_forage_fishing_lake_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Forage|Fishing|Fish|Lake|WaterBody|CatchTable|SeasonalSpawn|FishCaught|ForageCollected|Weather" Assets/_Game/Scripts docs/design .specs
rg -n "ResourceNode|NodeRefresh|Tree|Stump|Rock|Stone|Forage|Fishing|Lake|WorldActivity|Respawn|Regrowth|Save|Inventory|Tool" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a forage/fishing/lake existe ou foi criado de forma mínima
When o jogador executa a atividade principal da spec
Then o recurso é coletado/alterado de forma consistente
And inventário, ferramenta, stamina/fatigue e save/load não ficam inconsistentes
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Day transition / refresh

```text
Given o dia avança por dormir, colapso ou transição válida
When resource refresh/regrowth/spawn é processado
Then o sistema aplica apenas regras canônicas
And não cria ouro/recurso infinito cedo
And não respawna em tiles bloqueados, fixed anchors ou áreas de lore/endgame indevidas.
```

### Scenario 3 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 4 — Invalid state / invalid data

```text
Given um node, item, tool, drop table, spawn tile ou save payload inválido
When validator/teste é executado
Then a violação é reportada com motivo claro
And o runtime não duplica loot, não perde item e não corrompe save.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de FarmScene/UI/gameplay
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Forage coletado reaparece após reload.
- Forage spawna em building/crop/fixed anchor.
- Fishing gera gold infinito sem limite/custo.
- Rare lunar/endgame fish aparece cedo.
- Weather/season gates ignored.
- Inventory full deletes fish/forage.
- Quality/rarity lost silently.
- FarmOrder completion triggered by catch without adapter.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Forage Fishing Lake Runtime

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

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Day transition / refresh:
- Existing partial implementation:
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
4. A implementação exigir reescrever WorldActivity/Farm/Inventory/Save system canônico existente.
5. A implementação criar conflito com day transition, save ownership, stable IDs ou Testing Quality Gate.
6. A implementação criar pets/companions/animals runtime fora do escopo desta wave.
7. A implementação criar invasão/defesa/inimigos/dano a crops na fazenda, proibido no roadmap atual.
8. A implementação permitir recurso infinito, duplicação de drop, refresh sem cooldown ou save exploit.
9. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Forage/Fishing Spawn Matrix

| Resource | Gate | Refresh |
|---|---|---|
| Common forage | Season + valid natural tile | NextDayChance/SeasonStart |
| Weather forage | Weather + season + valid tile | WeatherTriggered |
| Aquatic forage | Lake edge/water tag | FixedDays/Season |
| Common lake fish | FishingSpot + catch table | per action, bounded |
| Rare seasonal fish | Season + weather/farm level | gated/future |
| Lunar/arcane fish | Lunar/story/endgame | future only |
| Festival fish | Festival event | future only |

## 23H. Fishing Anti-exploit Rules

```text
Fishing action may consume time/stamina/fatigue.
Catch tables must be gated by fish rarity.
Daily soft limits or diminishing returns may be used if economy exploit appears.
Farm lake must not replace cave/endgame rare resources.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "ResourceNode|NodeRefresh|Tree|Stump|Rock|Stone|Forage|Fishing|Lake|WorldActivity|Respawn|Regrowth|Save|Inventory|Tool" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de FarmScene, coleta, corte, mineração, foraging ou pesca, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, spawn/catch/gating/idempotency logic is deterministic.
- Requires EditMode tests: YES for forage spawn/collect/reload and fishing catch/inventory/gate tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for forage collection and fishing at lake visual flow.
- Requires regression test: YES if fixing existing forage/fishing duplicate bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver FarmScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no duplicate forage/catch exploit; no early rare/lunar fish.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_farm_forage_fishing_lake_runtime_execution_report.md.
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
Não implementar pets/companions/animals runtime.
Não criar invasão/defesa/inimigos na fazenda.
Não criar recurso infinito/duplicação de loot.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
