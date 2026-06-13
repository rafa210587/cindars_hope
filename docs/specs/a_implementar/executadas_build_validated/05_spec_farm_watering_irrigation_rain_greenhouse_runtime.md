# SPEC — Farm Watering Irrigation Rain Greenhouse Runtime

> **Spec ID:** `05_spec_farm_watering_irrigation_rain_greenhouse_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Gameplay Core  
> **Priority:** P0  
> **Type:** Runtime / Farm / Watering / Irrigation / Rain / Greenhouse  
> **Domain:** Farm / Water / Rain / Irrigation / Greenhouse / Day Transition  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_GAMEPLAY_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere crop growth processor, weather generation, day transition order, greenhouse crop rules, farm plot save schema, tool action input or irrigation building placement.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Tools/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/05_spec_farm_watering_irrigation_rain_greenhouse_runtime_execution_report.md`  
> **Depends on:**  
  - `crop growth quality runtime;`
  - `farm tool/action loop;`
  - `greenhouse UI/placement;`
  - `farm expansion zones;`
  - `advanced irrigation/fertilizer future.`
> **Blocks:**  

> **Scope:** endurecer/implementar contrato de água: rega manual, chuva externa, irrigação simples/futura e greenhouse context sem reescrever weather/crop systems.  
> **Out of scope:** weather generation, crop growth formulas, building placement, greenhouse full building UI, advanced automation, Água Viva/Mana irrigation, pets/companions.

---

# /speckit.specify

## 1. Contexto

Roadmap 1 exige chuva molhando áreas externas e feedback visual de solo seco/molhado. O sistema global de tempo define que a virada do dia calcula crescimento de crops e água/irrigação. A spec anterior de rain integration já separou Rain/Storm de estufa/interior.

Esta spec formaliza o ownership: water state é input para crop growth, não duplicação de crop growth.

---

## 2. Problema

Sem contrato de água:

```text
manual watering, rain e irrigation podem sobrescrever uns aos outros;
chuva pode molhar estufa/interior indevidamente;
irrigação pode aplicar depois do crescimento, causando off-by-one;
watered state pode ser resetado cedo demais;
greenhouse pode permitir tudo sem regras;
watering can/tools podem alterar crop state diretamente;
save/load pode perder IsWateredToday.
```

---

## 3. Objetivo

Criar/endurecer Watering/Irrigation contract:

```text
ManualWatering marks plot watered.
Rain/Storm water external valid farm plots.
Snow/Fog/Cloudy do not water unless future rule.
Irrigation applies to covered plots through deterministic provider.
Greenhouse separates season override from rain watering.
Water state is consumed by growth processor at day transition.
Visual wet/dry feedback reflects state, not source of truth.
```

---

## 4. Design rules

```text
Rain/Storm molham áreas externas.
Rain não molha estufa, interior, caverna ou cidade por default.
Irrigation reduces repetition but should not remove decisions too early.
Greenhouse allows off-season growth only when building/system exists.
Água Viva is rare/lore and not normal irrigation.
Fruto Mana/Mana root not regular crop irrigation.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero regar manualmente um plot e saber que ele está molhado.
Como sistema de clima, quero chuva molhando crops externas sem conhecer crop growth internals.
Como irrigação, quero aplicar água a plots cobertos de forma determinística.
Como greenhouse, quero permitir off-season sem receber chuva externa.
Como save/load, quero preservar water state sem duplicar crop growth.
```

---

## 6. Escopo

Inclui:

```text
WaterSource enum/contract;
FarmPlotWaterState contract;
ManualWatering command handling;
Rain/Storm external watering application;
Irrigation provider interface or hardening;
GreenhouseContext contract;
day transition water reset/consume rules;
tests/validators.
```

Não inclui:

```text
weather generation;
crop growth implementation;
watering can UX/prefab final;
sprinkler/irrigation art/buildings;
greenhouse full construction;
Água Viva/arcane watering;
automated companion/pet watering.
```

## Source Map Compliance

### Global sources read

- docs/design/SPEC_SOURCE_MAP.md
- docs/design/SPECIFICATION_PROCESS.md
- docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md
- docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
- docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
- docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
- docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
- docs/project/CURRENT_STATE.md

### Domain directions read

- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
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

- Roadmap 1 exige chuva molhando áreas externas.
- Roadmap 4 prevê irrigação avançada, aspersores, canais, runas hidráulicas ou mecanismos equivalentes.
- Roadmap 3 introduz estufa inicial.
- Day transition global calcula crescimento de crops e água/irrigação.
- Tempo e clima devem conectar rotina de fazenda, crops, chuva e irrigação.
- Rain/Storm são impactos globais, mas Farm direction vence para sistemas agrícolas.

### Deferred / future from directions

- Advanced sprinklers/canals/runes full runtime.
- Greenhouse full construction and UI.
- Água Viva irrigation.
- Lunar/arcane fertilizer/watering.
- Companion automation.
- Final visual VFX/SFX.

### Explicitly not redefined here

- Weather generation.
- Crop growth/death rules.
- Building placement.
- Farm expansion unlock costs.
- Tool input mapping/prefabs.

## 7. Modelo de domínio

### 7.1 WaterSource

```text
Manual
Rain
Storm
IrrigationSimple
IrrigationAdvanced
GreenhouseInternal
MagicFuture
LivingWaterFuture
Debug
```

### 7.2 FarmPlotWaterState

```text
PlotId
TilePosition
IsWateredToday
WateredBySources
LastWateredDay
IrrigationCoverageId optional
GreenhouseId optional
IsExternal
IsInterior
IsGreenhouse
```

### 7.3 Watering rules

```text
Manual watering:
  - valid on tillable/planted/tilled plot;
  - marks IsWateredToday;
  - records Manual source;
  - does not directly grow crop.

Rain/Storm:
  - applies to external valid plots;
  - does not apply to interiors, caves, city, greenhouse;
  - can water tilled/planted plots;
  - does not create tilled state by itself.

Irrigation:
  - deterministic coverage;
  - applies before growth calculation;
  - cannot target blocked/invalid plots;
  - advanced providers future.

Greenhouse:
  - may override season rules if greenhouse exists/unlocked;
  - not watered by rain;
  - may have internal watering/irrigation future;
  - cannot be implied by crop alone.
```

---

## 8. Day transition order

```text
1. Weather for current day is already known.
2. Rain/Storm external watering has applied or is applied in water phase.
3. Irrigation coverage applies in water phase.
4. Crop growth reads final water state.
5. Crop growth/death/regrow processes.
6. Water state resets/carries according to canonical rule.
7. Tomorrow weather is prepared elsewhere.
```

If repo order differs, executor must document conflict.

---

## 9. Greenhouse policy

```text
Greenhouse is a context provider, not a universal crop cheat.
Greenhouse can allow off-season growth only when:
  - plot is inside greenhouse zone/building;
  - greenhouse system exists/unlocked;
  - crop definition allows greenhouse or global rule permits;
  - watering/internal irrigation requirement is met.

Rain does not water greenhouse plots.
Greenhouse should not allow Mana/Fruto Mana as normal crop.
```

---

## 10. Save/load

Persist only if already supported or safe:

```text
IsWateredToday
LastWateredDay
WateredBySources optional
IrrigationCoverageId optional
GreenhouseId optional
```

Do not persist:

```text
Visual wet tile only;
Unity object references;
runtime weather object reference;
irrigation GameObject reference;
greenhouse prefab reference.
```

If current save cannot preserve required water state, STOP and create/reserve save spec.

---

## 11. Critérios de aceite

```text
Manual watering marks plot watered and does not grow crop directly.
Rain/Storm water valid external plots only.
Rain does not water greenhouse/interior.
Irrigation applies before crop growth.
Water reset/consume order is deterministic.
Greenhouse season override is explicit and gated.
Save/load preserves water state or blocks with migration need.
Tests cover manual, rain, greenhouse exclusion, irrigation coverage and reset.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Watering/WaterSource.cs
Assets/_Game/Scripts/Farm/Watering/FarmPlotWaterState.cs
Assets/_Game/Scripts/Farm/Watering/FarmWateringService.cs
Assets/_Game/Scripts/Farm/Watering/RainWateringAdapter.cs
Assets/_Game/Scripts/Farm/Watering/IrrigationCoverageProvider.cs
Assets/_Game/Scripts/Farm/Watering/GreenhouseContextProvider.cs
Assets/_Game/Tests/EditMode/Farm/FarmWateringServiceTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Tools/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/05_spec_farm_watering_irrigation_rain_greenhouse_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Inventory/**
```

---

## 14. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 15. Estratégia de implementação

```text
1. Auditar watering/rain/irrigation/greenhouse existentes.
2. Mapear ownership de water state.
3. Consolidar adapters sem mexer em weather generation.
4. Garantir ordering antes do crop growth.
5. Criar tests de sources/coverage/exclusions/reset.
6. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - crop growth;
  - weather generation;
  - greenhouse building;
  - day transition order;
  - farm save schema.
- Reason:
  - Water state is consumed by crop growth and day transition.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO.
Does this add save section? NO.
Does this require migration? NO unless water state missing; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. FarmPlotWateredEvent, RainWaterAppliedEvent, IrrigationAppliedEvent if absent and event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if event subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final, only state hooks.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for watering/rain/greenhouse visual verification.
```

---

## 20. Riscos

```text
Risco: Rain waters greenhouse.
Mitigação: IsExternal/Greenhouse exclusion tests.

Risco: watering grows crop directly.
Mitigação: crop growth reads only at day transition.

Risco: reset order wrong.
Mitigação: day transition tests.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar water/rain/irrigation/greenhouse systems.
- [ ] T003 — Consolidar WaterSource/FarmPlotWaterState.
- [ ] T004 — Implementar/harden manual watering service.
- [ ] T005 — Implementar/harden rain adapter.
- [ ] T006 — Implementar/harden irrigation coverage interface.
- [ ] T007 — Implementar greenhouse context gate.
- [ ] T008 — Criar tests.
- [ ] T009 — Rodar validações.
- [ ] T010 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes de farm/crop/economy/save foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de watering/irrigation/rain/greenhouse? | Arquivos alterados e justificativa. | PARTIAL |
| Day transition | A spec respeita a ordem de virada do dia? | Evidência de integração/ordem no report. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/05_spec_farm_watering_irrigation_rain_greenhouse_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Watering|WaterSource|Rain|Storm|Irrigation|Sprinkler|Greenhouse|IsWatered|FarmPlotWaterState|DayTransition" Assets/_Game/Scripts docs/design docs/specs
rg -n "Crop|FarmPlot|Soil|Water|Irrigation|Rain|Greenhouse|Harvest|Quality|Process|Shipping|SellPoint|DayTransition|Save" Assets/_Game/Scripts docs/design docs/specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
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
Given o sistema base relacionado a watering/irrigation/rain/greenhouse existe ou foi criado de forma mínima
When o jogador executa o fluxo principal desta spec durante um dia normal
Then o comportamento segue o direction canônico
And o estado é persistível/restaurável se aplicável
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Day transition integration

```text
Given o dia avança por dormir, colapso ou transição válida
When a ordem de virada do dia processa farm systems
Then a spec respeita a ordem declarada pelo sistema global
And crop growth, água, irrigação, shipping, restock, orders e weather não disputam responsabilidade
And qualquer dependência ausente é registrada como MISSING_BUT_DEFER.
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
Given um crop, plot, item, quality, process, shipping entry ou save payload inválido
When validator/teste é executado
Then a violação é reportada com motivo claro
And o runtime não corrompe save nem duplica recompensa/produto.
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

- Rain molha greenhouse/interior/cave/city.
- Manual watering grows crop immediately.
- Irrigation applies after growth calculation.
- Water state resets before crop growth.
- Irrigation coverage includes blocked/invalid plots.
- Greenhouse allows off-season without greenhouse context.
- Save/load loses IsWateredToday.
- Água Viva treated as common irrigation.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Watering Irrigation Rain Greenhouse Runtime

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
- Day transition:
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
4. A implementação exigir reescrever farm/crop/economy/save system canônico existente.
5. A implementação criar conflito com day transition, save ownership, stable IDs ou Testing Quality Gate.
6. A implementação criar pet/companion/animal runtime fora do escopo desta wave.
7. A implementação permitir ouro infinito, duplicação de produto, shipping duplicado ou reward duplicado.
8. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Water Source Matrix

| Source | Applies to external crops | Applies to greenhouse | Applies to interior/cave/city | Notes |
|---|---:|---:|---:|---|
| Manual | YES | YES if valid access | NO unless explicitly supported | Tool/action source |
| Rain | YES | NO | NO | External farm only |
| Storm | YES | NO | NO | May add future risk |
| IrrigationSimple | YES in coverage | YES only if installed/context | NO | Deterministic coverage |
| IrrigationAdvanced | future | future | NO | Sprinklers/channels/runes |
| GreenhouseInternal | NO external | YES | NO | Future/greenhouse-owned |
| LivingWaterFuture | special | special | NO | Rare/lore, not common water |

## 23H. Day Transition Invariants

```text
Final water state must be known before crop growth.
Crop growth must not call weather generation.
Water systems must not directly harvest/grow crops.
Greenhouse is context, not crop definition cheat.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Crop|FarmPlot|Soil|Water|Irrigation|Rain|Greenhouse|Harvest|Quality|Process|Shipping|SellPoint|DayTransition|Save" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário visual/gameplay de FarmScene, plantio, irrigação, colheita, processamento ou shipping, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, water coverage/order/exclusion logic is deterministic.
- Requires EditMode tests: YES for manual/rain/irrigation/greenhouse/reset ordering tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for watering/rain visual FarmScene scenario.
- Requires regression test: YES if fixing existing watering/rain/greenhouse bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver FarmScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no save schema change; Rain does not water greenhouse; report created.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_farm_watering_irrigation_rain_greenhouse_runtime_execution_report.md.
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
Não criar inflação/duplicação econômica.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
