# SPEC — Quest Farm Orders Adapter Runtime

> **Spec ID:** `09_spec_quest_farm_orders_adapter_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 09 — Quest / Objective / Event / Main Progression  
> **Priority:** P0  
> **Type:** Runtime / Quest Adapter / Farm Orders / Encomendas  
> **Domain:** Quest / FarmOrder / Delivery / Deadline / RepeatPolicy / Economy Reward  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_09_QUEST_ADAPTERS_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere farm crop/animal product runtime, shipping payment, economy pricing, reward engine, quest save schema, board UI, shop service contracts ou final order table content.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/Economy/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Quests/**`, `docs/validation/09_spec_quest_farm_orders_adapter_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
> **Blocks:**  
  - farm order board UI;
  - NPC/shop order generation;
  - shipping bin special orders;
  - economy balance validation;
  - farm tutorial/orders.
> **Scope:** definir/endurecer adapter de FarmOrder para o sistema genérico de quests, com deadlines, delivery, quality acceptance, repeat policy, reward idempotency e anti-economy exploit.  
> **Out of scope:** conteúdo final de encomendas, UI board final, shipping bin payment implementation, final reward tuning, crop/animal systems.

---

# /speckit.specify

## 1. Contexto

FarmOrder é categoria canônica de quest para encomendas de crops, itens processados, entregas por prazo, orders sazonais, pedidos de loja e shipping especial. Pode expirar, repetir por tabela e não deve bloquear main quest nem quebrar economia com recompensa infinita.

Esta spec cria o adapter que traduz FarmOrder para QuestDefinition/Objective/Reward, sem criar o catálogo final.

---

## 2. Problema

Sem adapter:

```text
encomendas podem virar sistema paralelo;
order pode usar contador próprio fora de QuestState;
item entregue pode ser consumido sem completar;
quality pode ser rejeitada sem regra clara;
reward pode duplicar no reload;
order expirado pode pagar mesmo assim;
repeat order pode gerar ouro infinito;
quest item agrícola pode ser vendido/shipping acidentalmente.
```

---

## 3. Objetivo

Criar/endurecer:

```text
FarmOrderDefinition;
FarmOrderObjectiveAdapter;
FarmOrderDeliveryPolicy;
FarmOrderDeadlinePolicy;
FarmOrderRepeatPolicy;
QualityAcceptancePolicy;
OrderRewardAdapter;
OrderExpirationHandler;
Quest integration;
Economy anti-exploit validation.
```

---

## 4. Regras de design

```text
FarmOrder usa o mesmo sistema base de quests.
FarmOrder pode ter prazo e expirar.
FarmOrder pode repetir por tabela se design permitir.
FarmOrder não bloqueia main quest.
FarmOrder não quebra economia com recompensa infinita.
Crop quest deve aceitar itens de qualidade quando permitido.
Quest item agrícola não deve ser perdido por shipping acidental se protegido.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero aceitar/entregar encomenda com item, quantidade, quality e prazo claro.
Como farm economy, quero reward melhor por condição, mas sem exploit.
Como quest system, quero FarmOrder como categoria/adapters, não sistema paralelo.
Como inventory/shipping, quero proteger item necessário quando aplicável.
Como save/load, quero prazo, progresso e reward idempotente preservados.
```

---

## 6. Escopo

Inclui:

```text
FarmOrder definition adapter;
objective mapping for deliver/ship/harvest/process;
deadline/expiry;
quality acceptance;
repeat policy;
reward adapter;
protected item/shipping guardrail;
tests/validators.
```

Não inclui:

```text
final order tables;
board UI final;
shipping payment runtime;
crop/animal/processing runtime implementation;
economy tuning final.
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

- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
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

- FarmOrder é usado para encomendas de crops, itens processados, entregas por prazo, orders sazonais, pedidos de loja e shipping especial.
- FarmOrder pode ter prazo, pode expirar, pode repetir por tabela, não deve bloquear main quest e não deve quebrar economia com recompensa infinita.
- Farm objectives incluem PlantCrop, WaterCrop, HarvestCrop, DeliverCrop, ProcessItem, ShipItem, BuildOrUpgrade, UseFonte e ProtectFragment.
- FarmOrder usa o mesmo sistema base.
- Crop quest deve aceitar itens de qualidade quando permitido.
- Shipping quest processa no day transition se definido.

### Deferred / future from directions

- Final order catalog.
- Order board UI.
- Shipping bin payment implementation.
- Exact reward values.
- Festival/social orders.
- Pet/companion orders.

### Explicitly not redefined here

- QuestDefinition/QuestState.
- Reward application engine.
- Inventory and shipping systems.
- Farm crop/product systems.
- Pricing/reward balance.
- City service/board systems.

## 7. Modelo de domínio

### 7.1 FarmOrderDefinition

```text
FarmOrderId
QuestId
RequesterNpcId optional
RequesterServiceId optional
OrderType: DeliverItem | ShipItem | HarvestCrop | ProcessItem | CraftItem | BuildOrUpgrade | SeasonalDelivery
RequiredItems[]
AcceptedQualityPolicy
RequiredSeason optional
DeadlinePolicy
RepeatPolicy
RewardTableId
ReputationReward optional
FailurePolicy
EconomyRiskTag
DebugTags[]
```

### 7.2 RequiredOrderItem

```text
ItemId
RequiredAmount
MinimumQuality optional
AcceptHigherQuality
AcceptAnyQuality
ConsumeOnDelivery
ProtectFromShippingWhenTracked
```

### 7.3 FarmOrderDeadlinePolicy

```text
NoDeadline
EndOfDay
DaysFromAccept
SeasonEnd
FestivalEnd
SpecificDay
```

### 7.4 FarmOrderRepeatPolicy

```text
Never
DailyTable
WeeklyTable
SeasonalTable
NpcRequestPool
ShopRequestPool
ManualStoryOnly
```

### 7.5 FarmOrderDeliveryResult

```text
Success
FailureReason
ItemsConsumed[]
AcceptedQualitySummary
RewardApplied
Expired
PartialProgress
DebugNotes[]
```

---

## 8. Objective mapping

```text
DeliverItem:
  uses DeliverItem objective and ItemDelivered trigger.

ShipItem:
  uses ShipItem objective and OnItemShipped/day transition trigger.

HarvestCrop:
  uses HarvestCrop objective and OnCropHarvested trigger.

ProcessItem:
  uses ProcessItem objective and OnItemProcessed trigger.

CraftItem:
  uses CraftItem objective and OnItemCrafted trigger.

BuildOrUpgrade:
  uses BuildOrUpgrade and OnBuildingUpgraded trigger.
```

No FarmOrder may invent a custom counter if QuestObjectiveState can represent it.

---

## 9. Quality policy

```text
AcceptAnyQuality:
  any quality counts.

MinimumQuality:
  equal or higher counts.

ExactQuality:
  only exact quality counts; should be rare and clearly communicated.

BonusForHigherQuality:
  accepts lower threshold but reward modifier may apply.

RejectLowerQuality:
  explicit failure reason.
```

---

## 10. Expiry and repeat rules

```text
Expired order transitions to Expired.
Expired order does not apply completion reward.
Expired order may apply small relationship penalty only if future social system exists; otherwise defer.
Repeat order creates new RepeatInstanceId.
Repeat orders must use bounded table and reward balance.
Main quest cannot depend on repeat FarmOrder.
```

---

## 11. Economy and shipping guardrails

```text
OrderRewardValue can be higher than normal sale because it requires condition.
Order cannot be always-best channel for every item.
Shipping-based order must distinguish normal shipping payment from quest reward.
Quest/protected agricultural item should not be consumed by normal SellPoint if tracked/protected.
Reward idempotency uses QuestRewardApplicator.
```

---

## 12. Criteria

```text
FarmOrder adapter maps to generic quest contracts.
Deadline/expiry is clear and persisted or STOP.
Quality acceptance is explicit.
Delivery/ship/harvest/process objectives use standard triggers.
Reward applies once.
Repeat policy bounded.
Tests cover delivery, quality, expiry, repeat, shipping/day transition and reward duplication.
```

---

# /speckit.plan

## 13. Arquitetura alvo

```text
Assets/_Game/Scripts/Quests/FarmOrders/FarmOrderDefinition.cs
Assets/_Game/Scripts/Quests/FarmOrders/FarmOrderObjectiveAdapter.cs
Assets/_Game/Scripts/Quests/FarmOrders/FarmOrderDeadlinePolicy.cs
Assets/_Game/Scripts/Quests/FarmOrders/FarmOrderRepeatPolicy.cs
Assets/_Game/Scripts/Quests/FarmOrders/FarmOrderDeliveryService.cs
Assets/_Game/Scripts/Quests/FarmOrders/FarmOrderValidator.cs
Assets/_Game/Tests/EditMode/Quests/FarmOrderAdapterTests.cs
```

Consolidar existentes se houver.

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Quests/**
docs/validation/09_spec_quest_farm_orders_adapter_runtime_execution_report.md
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
1. Auditar farm orders/order board/shipping quest systems.
2. Consolidar FarmOrderDefinition and policies.
3. Implementar objective adapter.
4. Integrar delivery/ship trigger contracts.
5. Integrar reward idempotency.
6. Criar tests.
7. Criar report.
```

---

## 17. Paralelização

- Parallelizable: NO
- Must not run with:
  - Quest core contracts;
  - reward engine;
  - shipping payment runtime;
  - crop/product quality systems;
  - economy balance validators.
- Reason: FarmOrder adapter crosses farm, inventory, economy and quest state.

---

## 18. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if QuestState handles deadlines/repeat ids; otherwise STOP.
Does this add save section? NO.
Does this require migration? NO unless FarmOrder-specific state is missing; then STOP.
Does this persist Unity references? NO.
```

---

## 19. Impacto eventos

```text
Adds event subscribers: CONDITIONAL if adapter subscribes to quest/event router.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 20. Impacto UI/Unity

```text
Changes UI: NO final; exposes board/projection data.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for order board/delivery/shipping visual flow.
```

---

## 21. Riscos

```text
Risco: order duplicates economy reward.
Mitigação: QuestRewardApplicator idempotency tests.

Risco: shipping consumes quest item accidentally.
Mitigação: protected tracked item policy.

Risco: repeat orders infinite gold.
Mitigação: repeat policy and economy validator.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar farm order/shipping systems.
- [ ] T003 — Consolidar FarmOrderDefinition/policies.
- [ ] T004 — Implementar objective adapter.
- [ ] T005 — Integrar reward/delivery/expiry.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a quest farm orders adapter foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Quest core | A spec usa os contratos de QuestDefinition, QuestState, Condition, Trigger, Reward e QuestFlag existentes? | Referência explícita no report. | PARTIAL |
| Separação de estado | QuestState, QuestFlag, MainProgression e FonteAnya permanecem separados? | Checklist explícito no report. | BLOCKED se misturar |
| Anti-softlock | Quest crítica, order ou contrato não cria bloqueio irreversível sem fallback? | Plano/validator. | PARTIAL |
| Anti-spoiler | UI/log/flags/adapters não revelam Arquivista, 101, finais ou Anya cedo? | Checklist/visibility. | PARTIAL |
| Pets/social future | A execução não implementou PetFuture/SocialFuture/romance/casamento profundo? | Checklist explícito. | BLOCKED se violar |
| Economia | FarmOrder/Contract/Festival não gera reward infinito nem buy/sell exploit? | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/09_spec_quest_farm_orders_adapter_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "FarmOrder|Encomenda|OrderBoard|DeliverItem|ShipItem|HarvestCrop|ProcessItem|Deadline|RepeatPolicy|QualityAcceptance" Assets/_Game/Scripts docs/design .specs
rg -n "QuestLog|QuestVisibility|QuestFlag|FarmOrder|CaveContract|FestivalQuest|ObjectiveAdapter|RewardAdapter|Spoiler|Softlock|Expired|Deadline|RepeatPolicy|MainProgression|FonteAnya|PetFuture|SocialFuture" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a quest farm orders adapter existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue o direction canônico
And QuestState, QuestFlag, rewards, UI projection e adapters permanecem consistentes
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

### Scenario 3 — Anti-spoiler / visibility

```text
Given quest, flag, order, festival, cave contract ou adapter contém informação oculta
When a UI/log/projection/validator consulta o estado
Then só conhecimento autorizado aparece
And passos futuros, rewards secretos, boss oculto, nível 101, finais e segredos de Anya permanecem ocultos até descoberta.
```

### Scenario 4 — Idempotency and expiry

```text
Given reward, flag, order, festival ou contract já foi aplicado/completado/expirado
When reload, day transition, event repeat ou reentrada ocorre
Then reward/flag não duplica
And expired/completed state não retorna ativo sem repeat policy explícita.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de Quest Log, board, festival, cave contract, notification ou delivery UI
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- FarmOrder creates parallel state outside QuestState.
- Order reward duplicates after reload.
- Expired order pays reward.
- Quality acceptance unclear.
- Shipping consumes protected quest item.
- Repeat order yields infinite gold.
- Order blocks main quest.
- FarmOrder requires missing crop/product system.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Quest Farm Orders Adapter Runtime

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

## Canon / quest compliance
- Quest core contracts reused:
- QuestState vs QuestFlag separation:
- MainProgression vs FonteAnya separation:
- Anti-softlock:
- Anti-spoiler:
- No PetFuture runtime:
- No SocialFuture/romance deep runtime:
- No reward/economy exploit:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Anti-spoiler/visibility:
- Idempotency/expiry:
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
4. A implementação exigir reescrever Quest/Save/EventBus/Fonte/MainProgression canônico existente.
5. A implementação esconder MainProgression ou FonteAnya dentro de QuestState genérico.
6. A implementação usar QuestFlag como substituto para estado complexo de quest.
7. A implementação permitir reward duplicado após reload/evento repetido.
8. A implementação revelar cedo Arquivista do Silêncio, final choices, nível 101, boss oculto ou segredo de Anya.
9. A implementação tornar main quest expirável por tempo/calendário.
10. A implementação criar PetFuture, SocialFuture, romance/casamento profundo ou companion full runtime fora do escopo.
11. A implementação gerar reward infinito por FarmOrder/Festival/CaveContract.
12. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. FarmOrder Objective Matrix

| Order type | Objective | Trigger |
|---|---|---|
| DeliverItem | DeliverItem | OnItemDelivered |
| ShipItem | ShipItem | OnItemShipped |
| HarvestCrop | HarvestCrop | OnCropHarvested |
| ProcessItem | ProcessItem | OnItemProcessed |
| CraftItem | CraftItem | OnItemCrafted |
| BuildOrUpgrade | BuildOrUpgrade | OnBuildingUpgraded |

## 23H. FarmOrder Economy Guardrail

```text
OrderRewardValue may exceed sale price only because it has:
  deadline;
  quality requirement;
  quantity requirement;
  requester constraint;
  season constraint;
  repeat limit;
  or story/service requirement.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "QuestLog|QuestVisibility|QuestFlag|FarmOrder|CaveContract|FestivalQuest|ObjectiveAdapter|RewardAdapter|Spoiler|Softlock|Expired|Deadline|RepeatPolicy|MainProgression|FonteAnya|PetFuture|SocialFuture" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de Quest Log, board, Festival, FarmOrder, CaveContract ou notification, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, order delivery/expiry/repeat/quality logic is deterministic.
- Requires EditMode tests: YES for delivery/quality/expiry/repeat/shipping/reward-idempotency tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for board/delivery/shipping visual flow.
- Requires regression test: YES if fixing existing FarmOrder/order reward bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no reward duplication; no repeat gold exploit.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/09_spec_quest_farm_orders_adapter_runtime_execution_report.md.
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
Não misturar QuestState com MainProgression/FonteAnya.
Não usar QuestFlag como substituto de QuestState.
Não aplicar reward/flag duas vezes.
Não revelar spoiler oculto cedo.
Não fazer main quest expirar por tempo.
Não implementar PetFuture/SocialFuture/romance profundo.
Não criar exploit de reward por order/festival/contract.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
