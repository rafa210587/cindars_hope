# SPEC — Farm Harvest Processing Quality Runtime

> **Spec ID:** `05_spec_farm_harvest_processing_quality_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Gameplay Core  
> **Priority:** P0  
> **Type:** Runtime / Farm / Harvest / Processing / Quality  
> **Domain:** Farm / Harvest / Item Yield / Quality / Processing / Inventory  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_GAMEPLAY_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere crop growth, inventory item instances/stacks, item quality schema, crafting/processing backend, economy base value, shipping/sellpoint or quest FarmOrder adapter.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Scripts/Crafting/**`, `Assets/_Game/Scripts/Items/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/05_spec_farm_harvest_processing_quality_runtime_execution_report.md`  
> **Depends on:**  
  - `shipping/sellpoint;`
  - `farm orders adapter;`
  - `crafting screen/runtime;`
  - `economy pricing/base value;`
  - `storage/chest and inventory UI.`
> **Blocks:**  

> **Scope:** endurecer/implementar colheita, geração de yield, item quality e processamento simples sem reescrever inventory/crafting/economy.  
> **Out of scope:** balance final de todos os crops, recipe database completa, processors/makers avançados, food/potion buffs, animal products, magical/lunar processing.

---

# /speckit.specify

## 1. Contexto

A fazenda produz crops e itens processados. A economia define que produtos processados têm valor maior, mas exigem tempo, construção, receita ou capacidade. Produtos de qualidade devem ser melhores para encomendas, presentes e receitas.

Esta spec conecta crop ready -> harvest -> inventory item(s) -> quality -> processing hooks.

---

## 2. Problema

Sem contrato de harvest/processing:

```text
crop ready pode ser colhido múltiplas vezes;
yield pode ignorar qualidade/regrow;
inventory full pode deletar item;
quality pode se perder no stack;
processed good pode multiplicar valor infinitamente;
processing pode consumir item sem output em save/load;
harvest pode completar FarmOrder sem trigger formal;
crop bruto e processado podem divergir de economy rules.
```

---

## 3. Objetivo

Criar/endurecer contrato:

```text
HarvestCommand;
HarvestResult;
YieldResolver;
QualityResolver integration;
Inventory add with overflow policy;
Regrow or clear plot after harvest;
ProcessableItem contract;
ProcessingJob or hook;
Processed output quality propagation;
No infinite multiplier without time/capacity/station.
```

---

## 4. Design rules

```text
Cada item deve ter função clara: vender, craft, receita, poção/fertilizante, encomenda, presente, upgrade/reparo, consumível ou progressão.
Farm é economia segura e previsível.
Processed goods têm valor maior, mas exigem tempo, construção, receita ou energia logística.
Quality improves orders/gifts/recipes.
Crafting/processamento não deve virar multiplicador infinito sem gargalo.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero colher crop pronto e receber item correto.
Como jogador, se inventário estiver cheio, quero evitar perda silenciosa.
Como sistema, quero preservar qualidade do item colhido.
Como crafting/processing, quero consumir inputs e produzir output de forma idempotente.
Como economy, quero que processamento aumente valor sem loop infinito.
```

---

## 6. Escopo

Inclui:

```text
harvest availability;
harvest command/result;
yield resolver;
quality transfer to items;
inventory add/overflow handling contract;
regrow/clear plot post-harvest;
processing hook/job minimal contract;
processed quality propagation;
tests/validators.
```

Não inclui:

```text
all recipe data;
processing machines final;
food/potion effects;
animal products;
advanced workshops;
shipping payment;
FarmOrder reward delivery;
UI final/prefabs.
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

- Fazenda produz crops, recursos, itens processados e economia.
- Produtos processados têm valor maior, mas exigem tempo, construção, receita ou energia logística.
- Produtos de qualidade são melhores para encomendas, presentes e receitas.
- Loop da fazenda: plantar/cuidar/colher -> vender bruto ou processar -> comida/poção/presente/encomenda/craft -> ouro/reputação/preparo.
- Cada item deve ter pelo menos uma função clara; itens sem função devem ser evitados.
- ProcessedBaseValue usa soma dos inputs, multiplicador de processamento e possível bonus de tier, mas não pode virar multiplicador infinito sem gargalo.

### Deferred / future from directions

- Full recipe database.
- Processing machines/workshops art and full runtime.
- Food/potion buff balance.
- Animal products.
- Festival/competition processing.
- Lunar/arcane processing.

### Explicitly not redefined here

- Inventory backend schema.
- Economy final balance.
- Shipping/SellPoint payment.
- Quest/FarmOrder completion.
- Crafting UI final.
- Save schema migrations.

## 7. Harvest model

### 7.1 HarvestCommand

```text
PlotId
ActorId optional
ToolId optional
RequestedDay
AllowInventoryOverflowHandling
```

### 7.2 HarvestResult

```text
Success
FailureReason
HarvestedItems[]
Quality
YieldAmount
PlotAfterHarvestState
RegrowStarted
InventoryAddResult
TriggeredEvents[]
```

### 7.3 Failure reasons

```text
PlotEmpty
CropNotReady
CropDead
InvalidCropId
InventoryFull
InvalidTool
AlreadyHarvestedThisFrameOrCommand
BlockedByQuestOrEvent optional/future
```

### 7.4 Post-harvest plot rules

```text
Normal crop:
  - clear CropId;
  - soil remains tilled or returns to tilled/dry depending existing farm contract;
  - growth fields reset.

Regrow crop:
  - keep CropId;
  - reset GrowthProgressDays/regrow progress;
  - mark not ready;
  - preserve regrow definition.

Dead crop:
  - harvest should fail;
  - requires cleanup action.
```

---

## 8. Quality propagation

```text
Crop quality should be represented on harvested item if inventory supports quality.
If item stack cannot mix quality, stack key must include quality or create ItemInstance.
If inventory lacks quality support, execution must mark MISSING_BUT_DEFER and not fake quality.
Quality affects:
  - sell/shipping value later;
  - processing output quality;
  - FarmOrders quality requirements;
  - gifts/recipes future.
```

Quality must not:

```text
be stored only in UI;
be recalculated after reload differently;
be ignored silently by processing if direction requires it.
```

---

## 9. Processing contract

### 9.1 ProcessableItem

```text
InputItemId
InputQualityPolicy
InputQuantity
OutputItemId
OutputQuantity
StationId optional
ProcessingTime
RequiredRecipeId optional
RequiredFarmLevel optional
RequiredBuildingId optional
QualityTransferPolicy
```

### 9.2 QualityTransferPolicy

```text
PreserveMinimum
AverageInputs
HighestInputWithChance
FixedOutput
RecipeDefined
NoQuality
```

### 9.3 ProcessingJob

```text
JobId
StationId
InputStacks
OutputDefinition
StartDayTime
FinishDayTime
State: Queued | Processing | ReadyToCollect | Collected | Cancelled
```

This spec may only create minimal hooks if processing backend is absent.

---

## 10. Economy guardrails

```text
Processing should increase value through time/capacity/station/recipe, not free multiplication.
Processed output must not be immediately reprocessed into infinite profit loop unless explicitly bounded.
ProcessedBaseValue baseline comes from economy direction, but exact values are not set here.
Sell/shipping formulas are out of scope.
```

---

## 11. Save/load

Harvest:

```text
Harvest command itself is not saved unless asynchronous/future.
Plot state after harvest must persist through existing FarmPlot state.
Inventory add result must persist via inventory system.
```

Processing:

```text
If processing jobs exist, state must be persisted by existing crafting/processing save section.
If processing jobs do not exist, do not create save schema here.
If processing job persistence is required but absent, STOP and create save/migration spec.
```

---

## 12. Critérios de aceite

```text
Ready crop harvest produces correct item/yield.
Crop cannot be harvested twice.
Inventory full does not silently delete output.
Quality is preserved or explicitly deferred.
Regrow crop enters regrow state.
Dead crop cannot be harvested.
Processing hook/job has clear states if present.
Processed output value/quality does not create infinite multiplier.
Tests cover harvest success/failure/regrow/inventory full/quality.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Harvest/HarvestCommand.cs
Assets/_Game/Scripts/Farm/Harvest/HarvestResult.cs
Assets/_Game/Scripts/Farm/Harvest/FarmHarvestService.cs
Assets/_Game/Scripts/Farm/Harvest/CropYieldResolver.cs
Assets/_Game/Scripts/Farm/Harvest/CropQualityTransferService.cs
Assets/_Game/Scripts/Farm/Processing/FarmProcessingJob.cs
Assets/_Game/Scripts/Farm/Processing/FarmProcessingValidator.cs
Assets/_Game/Tests/EditMode/Farm/FarmHarvestServiceTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Crafting/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/05_spec_farm_harvest_processing_quality_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Core/Events/**
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

## 16. Estratégia de implementação

```text
1. Auditar harvest/inventory/processing existentes.
2. Consolidar HarvestCommand/Result ou mapear equivalentes.
3. Garantir no duplicate harvest.
4. Integrar inventory add/overflow.
5. Preservar quality or defer with report.
6. Criar minimal processing contract only if safe.
7. Criar tests and report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - crop growth;
  - inventory quality schema;
  - crafting/processing backend;
  - shipping/sellpoint;
  - farm orders.
- Reason:
  - Harvest is the boundary between crop state and inventory/economy.

---

## 18. Impacto save/load

```text
Does this change save schema? SHOULD BE NO.
Does this add save section? NO.
Does this require migration? NO unless processing jobs require persistence; then STOP.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. CropHarvestedEvent or ProcessingJobCompletedEvent if absent and event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if event subscribers are created.
```

---

## 20. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for harvest/inventory/processing flow.
```

---

## 21. Riscos

```text
Risco: item duplication.
Mitigação: command idempotency and state transition tests.

Risco: inventory full deletes output.
Mitigação: overflow failure/result handling.

Risco: processing infinite value.
Mitigação: time/capacity/station gate and no economy formula here.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar harvest/inventory/processing.
- [ ] T003 — Consolidar HarvestCommand/Result.
- [ ] T004 — Implementar/harden harvest service.
- [ ] T005 — Integrar inventory add/overflow.
- [ ] T006 — Preservar/defer quality safely.
- [ ] T007 — Definir processing hook/job minimal se seguro.
- [ ] T008 — Criar tests.
- [ ] T009 — Rodar validações.
- [ ] T010 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes de farm/crop/economy/save foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de harvest/processing/quality? | Arquivos alterados e justificativa. | PARTIAL |
| Day transition | A spec respeita a ordem de virada do dia? | Evidência de integração/ordem no report. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/05_spec_farm_harvest_processing_quality_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Harvest|CropHarvested|Yield|Quality|InventoryFull|Regrow|Processing|Processor|Recipe|FarmProcessingJob|Processed" Assets/_Game/Scripts docs/design .specs
rg -n "Crop|FarmPlot|Soil|Water|Irrigation|Rain|Greenhouse|Harvest|Quality|Process|Shipping|SellPoint|DayTransition|Save" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a harvest/processing/quality existe ou foi criado de forma mínima
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

- Crop colhido duas vezes.
- Inventory full apaga colheita.
- Quality perdida em stack.
- Regrow vira crop novo incorreto.
- Dead crop colhido.
- Processing job não persiste se assíncrono.
- Processed output cria loop de valor infinito.
- FarmOrder completion dispara sem trigger formal.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Harvest Processing Quality Runtime

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


## 23G. Harvest Test Matrix

| Case | Expected |
|---|---|
| Ready crop + inventory space | Items added; plot transitions |
| Ready crop + inventory full | Failure/overflow policy, no item loss |
| Crop not ready | Failure, plot unchanged |
| Dead crop | Failure, cleanup needed |
| Regrow crop | Output added, crop enters regrow |
| Duplicate command | No duplicate output |
| Invalid HarvestItemId | Safe failure/report |
| Quality-enabled crop | Item quality preserved or deferred |

## 23H. Processing State Matrix

| State | Meaning |
|---|---|
| Queued | Inputs reserved or accepted |
| Processing | Time running |
| ReadyToCollect | Output ready, no duplicate collection |
| Collected | Output claimed |
| Cancelled | Cancelled by rule/future |

If these states do not exist, spec should create hook/validator only, not full async backend.


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Crop|FarmPlot|Soil|Water|Irrigation|Rain|Greenhouse|Harvest|Quality|Process|Shipping|SellPoint|DayTransition|Save" Assets/_Game/Scripts docs/design .specs
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

- Changed deterministic logic: YES, harvest/yield/quality/processing projection logic is deterministic.
- Requires EditMode tests: YES for harvest success/failure/regrow/inventory full/quality tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for harvest and processing gameplay flow.
- Requires regression test: YES if fixing existing harvest/duplication/quality bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver FarmScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no duplicate harvest; no silent item loss; report created.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_farm_harvest_processing_quality_runtime_execution_report.md.
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
