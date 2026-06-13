# SPEC — Farm Shipping SellPoint Runtime

> **Spec ID:** `05_spec_farm_shipping_sellpoint_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Gameplay Core  
> **Priority:** P0  
> **Type:** Runtime / Farm / Shipping / SellPoint / Payment  
> **Domain:** Farm / SellPoint / Shipping / Economy / Day Transition  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_GAMEPLAY_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere economy price formulas, inventory item model, shop sell flow, FarmOrder rewards, day transition order, save/load shipping section or SellPoint movement/building placement.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/Economy/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/05_spec_farm_shipping_sellpoint_runtime_execution_report.md`  
> **Depends on:**  
  - `farm orders adapter;`
  - `economy pricing service;`
  - `inventory item quality;`
  - `shop buy/sell UI;`
  - `farm building movement/SellPoint relocation.`
> **Blocks:**  

> **Scope:** endurecer/implementar SellPoint/caixa de envio: aceitar itens vendáveis, processar pagamento na manhã seguinte, preservar conteúdo em save/load e impedir duplicação.  
> **Out of scope:** economy formula final, shop selling, FarmOrder reward payout, UI final do SellPoint, mover SellPoint visualmente, market demand advanced, festival competition.

---

# /speckit.specify

## 1. Contexto

Roadmap 1 exige SellPoint/caixa de venda. Roadmap 2 explicita caixa de envio com pagamento na manhã seguinte. O direction de economy diferencia ShippingPrice, SellPoint, ShopBuyFromPlayerPrice e OrderRewardValue.

Esta spec formaliza SellPoint como canal de venda simples/seguro, não necessariamente o melhor preço, e processado na virada do dia.

---

## 2. Problema

Sem shipping contract:

```text
item pode ser pago imediatamente ou amanhã sem regra;
item pode ser pago duas vezes após reload;
quality pode ser ignorada no preço;
Quest/Key item pode ser vendido;
SellPoint movido pode apagar itens pendentes;
order delivery pode confundir com shipping comum;
shipping pode usar preço de loja errada;
inventory removal pode ocorrer sem pending entry persistida.
```

---

## 3. Objetivo

Criar/endurecer SellPoint/Shipping:

```text
SellPoint accepts sellable items only;
creates PendingShippingEntry;
removes item from inventory only after pending accepted;
payment processed at next day transition;
ShippingPrice uses economy service;
quality modifier is considered if supported;
Quest/Key/NonSellable blocked;
processed entries idempotent;
save/load preserves pending and processed state;
SellPoint move preserves pending content.
```

---

## 4. Design rules

```text
SellPoint é seguro/simples, mas nem sempre melhor.
Shop specialized may pay better for accepted categories.
Encomenda is separate from common shipping and can reward more.
Nenhuma spec deve permitir loop infinito de comprar barato e vender caro.
Items should often have alternative uses beyond selling.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero colocar itens na caixa e receber pagamento no dia seguinte.
Como jogador, quero saber se um item não é vendável.
Como save/load, quero preservar itens pendentes sem duplicar pagamento.
Como economy, quero calcular ShippingPrice por canal correto.
Como FarmOrder, quero não confundir entrega de encomenda com venda comum.
```

---

## 6. Escopo

Inclui:

```text
SellPoint deposit command;
PendingShippingEntry;
ShippingBatch for day transition;
price calculation call to economy service;
non-sellable protection;
quality-aware price input;
save/load pending entries;
idempotent payment;
SellPoint move preservation rule;
tests/validators.
```

Não inclui:

```text
shop sell flow;
FarmOrder delivery/rewards;
final price formulas;
market demand advanced;
SellPoint UI/prefab final;
building move runtime;
festival competition selling.
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
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
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

- Roadmap 1 inclui SellPoint/caixa de venda.
- Roadmap 2 inclui caixa de envio com pagamento na manhã seguinte.
- Day transition processa shipping/SellPoint e pagamentos pendentes.
- Economy define ShippingPrice, SellPoint, ShopBuyFromPlayerPrice, ShopSellToPlayerPrice e OrderRewardValue como conceitos distintos.
- SellPoint tem ChannelSellMultiplier x0.90-x1.00 e é seguro/simples, mas nem sempre melhor.
- QuestItem/KeyItem/Unique devem ser NonSellable ou exigir confirmação forte.
- Nenhuma spec deve permitir loop infinito de comprar barato e vender caro.

### Deferred / future from directions

- Market demand advanced.
- Festival/competition shipping.
- Shop specialized selling.
- FarmOrder reward pipeline.
- Full sellpoint UI/prefab.
- SellPoint building movement implementation.

### Explicitly not redefined here

- Inventory backend.
- Economy pricing formula final.
- Day transition global runtime.
- Building placement/move runtime.
- Quest FarmOrder adapter.
- UI confirmation patterns.

## 7. Domain model

### 7.1 SellPointDepositCommand

```text
ActorId optional
SellPointId
InventorySlotId or ItemInstanceId
ItemId
Quantity
Quality optional
RequestedAtDayTime
```

### 7.2 PendingShippingEntry

```text
ShippingEntryId
SellPointId
ItemId
ItemInstanceId optional
Quantity
Quality optional
BaseValueSnapshot optional
PricePreview optional
DepositedDay
ProcessOnDay
State: Pending | Processed | Cancelled | Invalid
ProcessedPaymentId optional
```

### 7.3 ShippingBatch

```text
BatchId
ProcessDay
Entries[]
TotalGold
State: Pending | Processed | FailedPartial
```

### 7.4 Failure reasons

```text
ItemNotSellable
QuestItemProtected
KeyItemProtected
QuantityInvalid
InventoryItemMissing
InventoryRemoveFailed
EconomyPriceMissing
DuplicateEntry
AlreadyProcessed
SaveStateInvalid
SellPointUnavailable
```

---

## 8. Deposit rules

```text
Deposit should validate item sellability before removing from inventory.
If item is accepted:
  - create PendingShippingEntry;
  - remove item quantity from inventory;
  - persist pending entry;
  - show feedback if UI exists.

If pending entry cannot be persisted:
  - do not remove item;
  - fail safely.

If item is Quest/Key/NonSellable:
  - block deposit;
  - do not remove item.
```

---

## 9. Day transition processing

Shipping processing must happen in the global day transition payment phase:

```text
1. Read pending shipping entries due for ProcessDay.
2. For each entry, skip if already processed.
3. Resolve ShippingPrice via economy service.
4. Apply quality/channel/relevant modifiers.
5. Accumulate total gold.
6. Create payment record or gold delta.
7. Mark entries processed with ProcessedPaymentId.
8. Publish GoldChanged/ShippingProcessed event if event system exists.
9. Save processed state.
```

Idempotency rule:

```text
Reloading after payment must not pay again.
Reloading before payment must preserve pending entries.
```

---

## 10. Price rules

ShippingPrice should call economy/pricing service and respect:

```text
BaseValue
ChannelSellMultiplier for SellPoint
QualityMultiplier if supported
RaritySellModifier if supported
DemandModifier if supported
SeasonModifier if supported
StoryFlagModifier if supported
```

This spec does not define final numbers. It must not duplicate pricing formula in UI or Farm systems.

---

## 11. SellPoint movement/preservation

If SellPoint can move now or later:

```text
SellPointId remains stable.
Pending entries remain attached to SellPointId or global farm shipping section.
Moving SellPoint cannot delete pending content.
SellPoint cannot be placed blocking city/cave/house access.
This spec does not implement movement; it preserves invariant.
```

---

## 12. Save/load

Must persist or safely represent:

```text
PendingShippingEntries
ProcessedShippingEntries or ProcessedPaymentIds
SellPointId
ItemId
Quantity
Quality if applicable
DepositedDay/ProcessDay
State
```

Must not persist:

```text
Unity references;
inventory slot as sole identity after removal;
visual sellpoint GameObject;
price as only source of truth if economy should recalc.
```

If shipping section does not exist and cannot be added safely, STOP and create save/migration spec.

---

## 13. Quest/FarmOrder separation

```text
Common shipping is selling.
FarmOrder delivery is quest/order fulfillment.
Depositing an item in SellPoint should not complete FarmOrder unless a specific FarmOrder adapter intentionally listens for that channel.
OrderRewardValue is not ShippingPrice.
```

---

## 14. Critérios de aceite

```text
Sellable item can be deposited.
NonSellable/Quest/Key item blocked.
Deposit persists pending entry and removes inventory safely.
Payment occurs next day, once.
Reload before payment keeps pending.
Reload after payment does not pay twice.
ShippingPrice uses economy service/channel, not ad hoc UI formula.
Quality is considered or deferred explicitly.
SellPoint move invariant documented/preserved.
Tests cover deposit/block/process/reload/idempotency.
```

---

# /speckit.plan

## 15. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Shipping/SellPointDepositCommand.cs
Assets/_Game/Scripts/Farm/Shipping/PendingShippingEntry.cs
Assets/_Game/Scripts/Farm/Shipping/ShippingBatch.cs
Assets/_Game/Scripts/Farm/Shipping/FarmShippingService.cs
Assets/_Game/Scripts/Farm/Shipping/ShippingPriceResolver.cs
Assets/_Game/Scripts/Farm/Shipping/ShippingDayTransitionProcessor.cs
Assets/_Game/Tests/EditMode/Farm/FarmShippingServiceTests.cs
```

Consolidar existentes se houver.

---

## 16. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/05_spec_farm_shipping_sellpoint_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Core/Events/**
Assets/_Game/Scripts/UI/**
```

---

## 17. Arquivos proibidos

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

## 18. Estratégia de implementação

```text
1. Auditar SellPoint/shipping/economy/inventory save existentes.
2. Consolidar deposit command and pending entry.
3. Ensure non-sellable protection.
4. Ensure day transition idempotency.
5. Integrate price resolver via economy service.
6. Add save/load tests where harness allows.
7. Create report.
```

---

## 19. Paralelização

- Parallelizable: NO
- Must not run with:
  - economy pricing;
  - inventory item model/quality;
  - day transition;
  - farm orders adapter;
  - building/SellPoint movement.
- Reason:
  - Shipping touches inventory, economy, save/load, day transition and farm orders.

---

## 20. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if shipping already exists; otherwise STOP before schema change.
Does this add save section? NO unless dedicated save spec approves.
Does this require migration? NO unless adding pending shipping state; then STOP.
Does this persist Unity references? NO.
```

---

## 21. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. ItemDepositedForShippingEvent, ShippingProcessedEvent, GoldChangedEvent if absent and event contract allows.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if event subscribers are created.
```

---

## 22. Impacto UI/Unity

```text
Changes UI: NO final; UI may consume failure reasons later.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for deposit/sleep/payment flow.
```

---

## 23. Riscos

```text
Risco: duplicate payment.
Mitigação: ProcessedPaymentId/idempotency tests.

Risco: item loss on failed persist.
Mitigação: create pending before inventory remove or atomic service behavior.

Risco: price formula drift.
Mitigação: call economy service.

Risco: sellpoint movement deletes pending entries.
Mitigação: stable SellPointId/global shipping section.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar shipping/sellpoint/economy/inventory save.
- [ ] T003 — Consolidar deposit command/pending entry.
- [ ] T004 — Implementar/harden non-sellable protection.
- [ ] T005 — Implementar/harden day transition processor.
- [ ] T006 — Integrar price resolver com economy service.
- [ ] T007 — Criar idempotency/save tests.
- [ ] T008 — Rodar validações.
- [ ] T009 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes de farm/crop/economy/save foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de shipping/sellpoint/payment? | Arquivos alterados e justificativa. | PARTIAL |
| Day transition | A spec respeita a ordem de virada do dia? | Evidência de integração/ordem no report. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/05_spec_farm_shipping_sellpoint_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "SellPoint|Shipping|PendingShipping|ShippingPrice|GoldChanged|Deposit|NonSellable|QuestItem|KeyItem|ProcessedPayment|DayTransition" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a shipping/sellpoint/payment existe ou foi criado de forma mínima
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

- Item depositado e perdido se pending entry falhar.
- Pagamento duplicado após reload.
- Pagamento perdido após reload.
- Quest/Key item vendido.
- FarmOrder reward confundido com ShippingPrice.
- Preço calculado fora do economy service.
- Quality ignorada sem decisão.
- SellPoint movido apagando entries pendentes.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Shipping SellPoint Runtime

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


## 23G. Shipping State Matrix

| State | Meaning | Allowed transition |
|---|---|---|
| Pending | Item accepted, awaiting process day | Processed/Cancelled |
| Processed | Payment applied once | terminal |
| Cancelled | Cancelled before processing if supported | terminal or returned |
| Invalid | Entry cannot process due data issue | manual/report only |

## 23H. Deposit Atomicity Rules

```text
If pending entry cannot be created, inventory must not be removed.
If inventory removal fails, pending entry must not remain active.
If save fails after deposit, report risk; do not mark ACCEPTED.
If payment succeeds, ProcessedPaymentId must prevent duplicate payout.
```

## 23I. Sell Channel Separation

```text
SellPoint = ShippingPrice.
Shop selling = ShopBuyFromPlayerPrice.
Order delivery = OrderRewardValue.
These channels must not share UI/data source implicitly.
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

- Changed deterministic logic: YES, deposit/payment/idempotency/pricing channel logic is deterministic.
- Requires EditMode tests: YES for deposit/block/payment/reload/idempotency tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for deposit item -> sleep -> receive payment flow.
- Requires regression test: YES if fixing existing duplicate payment/item loss bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver FarmScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no duplicate payout; no non-sellable item shipping; report created.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_farm_shipping_sellpoint_runtime_execution_report.md.
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
