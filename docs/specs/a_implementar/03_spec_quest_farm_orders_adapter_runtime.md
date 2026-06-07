# SPEC — Quest Farm Orders Adapter Runtime

> **Spec ID:** `03_spec_quest_farm_orders_adapter_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 03 — Quest / Objective / Event System  
> **Priority:** P1  
> **Type:** Runtime / Quest Adapter / Farm Orders / Economy Integration  
> **Domain:** Quests / Farm Orders / Shipping / Delivery / Rewards  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_03_QUEST_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere QuestState, Reward runtime, FarmSection, Inventory, Shipping/SellPoint, Economy rewards, Calendar expiry ou Quest Log visibility.  
> **Repo lock scope:** `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/Economy/**`, `Assets/_Game/Scripts/Inventory/**`, `docs/validation/03_spec_quest_farm_orders_adapter_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/specs/a_implementar/03_spec_quest_objective_event_contract_runtime.md`
  - `docs/specs/a_implementar/03_spec_quest_condition_trigger_runtime.md`
  - `docs/specs/a_implementar/03_spec_quest_state_save_load_runtime.md`
  - `docs/specs/a_implementar/03_spec_quest_reward_application_idempotency_runtime.md`
> **Blocks:**  
  - farm order boards / notice board runtime;
  - shipping/order settlement;
  - festival order integration;
  - economy reward balance;
  - Quest Log order presentation.
> **Scope:** implementar ou endurecer um adapter que permita FarmOrder usar o sistema genérico de quest/objective/reward, sem criar sistema paralelo de encomendas.  
> **Out of scope:** criar economia nova, reescrever shipping, alterar crop growth, criar UI final de board, alterar calendário/festival runtime, ou transformar farm orders em main quest.

---

# /speckit.specify

## 1. Contexto

O direction de Quest define `FarmOrder` como uma categoria canônica do sistema genérico de quest. Ela serve para encomendas de crops, itens processados, entregas por prazo, orders sazonais, pedidos de loja e shipping especial.

O direction de Farm define que a fazenda deve gerar economia, encomendas, produtos de qualidade, recursos raros e decisões de rotina, mas sem reimplementar a base de plantio já existente.

Esta spec cria o adapter entre os dois mundos:

```text
FarmOrder é uma QuestCategory.
FarmOrder usa QuestDefinition/QuestState/Objectives/Conditions/Triggers/Rewards.
FarmOrder não cria um sistema paralelo de state.
FarmOrder pode ter prazo, expirar e repetir por tabela.
FarmOrder não bloqueia main quest.
```

---

## 2. Problema

Sem adapter claro, orders de fazenda podem nascer como outro sistema paralelo.

Riscos:

```text
farm order com estado fora do QuestState;
reward aplicado fora do reward idempotency;
prazo duplicado fora do calendário;
item entregue sem trigger de quest;
shipping processado sem objective;
pedido expirado sem feedback;
recompensa infinita por repeat mal configurado;
quest log não mostrando order conhecida;
save/load perdendo order em andamento.
```

---

## 3. Objetivo

Criar contrato/runtime para `FarmOrder` como adapter do sistema genérico de quest.

Resultado esperado:

```text
FarmOrder usa QuestId, QuestState, Objectives e Rewards;
orders podem iniciar por notice board, NPC, shop ou festival futuro;
objectives agrícolas usam triggers de farm/inventory/shipping;
expiry usa calendar/day state;
reward usa pipeline idempotente;
save/load usa QuestStateSection;
Quest Log mostra order conhecida sem spoiler.
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

- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável, com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- `FarmOrder` como categoria canônica de quest.
- Objectives agrícolas: PlantCrop, WaterCrop, HarvestCrop, DeliverCrop, ProcessItem, ShipItem.
- Orders podem ter prazo, expirar e repetir se design permitir.
- Orders não bloqueiam main quest.
- Farm é base econômica e de produção, mas sem reimplementar plantio/crescimento.
- Reward precisa ser idempotente e não quebrar economia.

### Deferred / future from directions

- Notice board UI final.
- Balance final de recompensa/economia.
- Farm orders sazonais avançadas.
- Festival competitions/minigames.
- Orders lendárias/endgame.

### Explicitly not redefined here

- Crop growth.
- Shipping base.
- Shop stock/economy base.
- Calendar runtime base.
- Quest Log visual final.

## 4. Estado atual do repo

Estado documental:

```text
Farm base existe/parcial: soil, planting, watering, harvest, save de farm em algum grau.
Economy/shop/crafting existem parcialmente ou completos conforme audit report.
Quest system base ainda está sendo especificado na WAVE 03.
FarmOrder é direction canônico, mas deve ser runtime adapter futuro.
```

Estado a confirmar localmente:

```text
classes existentes de order/encomenda;
SellPoint/shipping managers;
events de crop/item/shipping;
inventory delivery APIs;
calendar/day transition APIs;
quest runtime, se já existir.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero aceitar uma encomenda agrícola e ver objetivo/prazo/recompensa.
Como sistema de farm, quero emitir triggers sem conhecer QuestState internamente.
Como sistema de quest, quero receber eventos agrícolas e atualizar objectives.
Como sistema de reward, quero aplicar recompensa de order uma vez só.
Como save/load, quero preservar orders ativas, concluídas, expiradas e recompensas aplicadas.
```

---

## 6. Escopo

Inclui:

```text
FarmOrder category adapter;
FarmOrder objective mapping;
delivery/shipping trigger mapping;
expiry policy por calendário;
reward idempotency integration;
save/load via QuestStateSection;
debug/validation para order sem prazo/reward/objective;
execution report.
```

---

## 7. Fora de escopo

```text
UI final de notice board;
balance numérico final;
minigames de festival;
economy pricing completo;
crop system novo;
shipping system novo;
social/relationship rewards;
companion/pet orders.
```

---

## 8. Regras de não duplicação

```text
Não criar FarmOrderState separado se QuestState cobre.
Não aplicar reward fora do reward pipeline idempotente.
Não usar QuestFlag como substituto de QuestState.
Não persistir item/crop definition diretamente; usar IDs.
Não fazer FarmOrder depender de hidden main quest.
```

---

## 9. Critérios de aceite

### 9.1 Adapter

- FarmOrder é representável como QuestDefinition/QuestState.
- Objectives agrícolas mapeiam para tipos existentes ou criam types genéricos reutilizáveis.
- Delivery/shipping não aplica reward diretamente sem QuestReward pipeline.
- Expiry usa calendário/day state, não contador isolado.

### 9.2 Save/load

- FarmOrder em andamento sobrevive save/load.
- Completed/Expired não reaplica reward.
- Delivered amount parcial, se existir, é preservado.
- GrantedRewardIds e GrantedFlagIds impedem duplicidade.

### 9.3 Validação/dev

Criar ou planejar validator para:

```text
FarmOrder sem prazo quando marcado expirable;
FarmOrder com reward ausente;
FarmOrder com item/crop ID inexistente;
FarmOrder repetível sem repeat policy;
FarmOrder que bloqueia main quest;
FarmOrder reward infinito sem controle.
```

---

# /speckit.plan

## 10. Arquitetura alvo

Possíveis componentes:

```text
Assets/_Game/Scripts/Quests/Adapters/FarmOrderQuestAdapter.cs
Assets/_Game/Scripts/Quests/Adapters/FarmOrderObjectiveMapper.cs
Assets/_Game/Scripts/Quests/Conditions/FarmOrderConditions.cs
Assets/_Game/Scripts/Quests/Triggers/FarmOrderTriggers.cs
Assets/_Game/Scripts/Quests/Validation/FarmOrderQuestValidator.cs
Assets/_Game/Tests/EditMode/Quests/FarmOrderQuestAdapterTests.cs
```

---

## 11. Contratos

### Data

```text
QuestDefinition.Category = FarmOrder.
ObjectiveType pode usar DeliverItem, HarvestCrop, ShipItem, ProcessItem.
Reward usa RewardDefinition/RewardApplication pipeline.
QuestState persiste progresso.
```

### Runtime

```text
Farm systems publicam eventos.
Quest system consome eventos.
Adapter traduz eventos agrícolas em objective progress.
```

### Save

```text
Sem nova save section.
Usa QuestStateSection.
```

### UI

```text
Quest Log pode exibir como Farm Order / Encomenda.
Board UI final é fora desta spec.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Tests/EditMode/Quests/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/03_spec_quest_farm_orders_adapter_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/World/**
```

---

## 13. Arquivos proibidos

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

## 14. Estratégia de implementação

```text
1. Auditar sistemas existentes de farm orders/shipping.
2. Auditar quest runtime criado pelas specs anteriores.
3. Se já existir order system, criar adapter residual.
4. Se não existir, criar apenas contrato mínimo FarmOrder no quest runtime.
5. Integrar triggers sem acoplar farm ao QuestState.
6. Adicionar teste EditMode para progresso/reward/expiry quando praticável.
7. Criar execution report.
```

---

## 15. Ordem segura

```text
Quest contracts -> Conditions/Triggers -> Reward idempotency -> QuestState save/load -> FarmOrder adapter.
```

---

## 16. Paralelização

- Parallelizable: NO
- Parallel group: WAVE_03_QUEST_LOCKED
- Must not run with:
  - quest state save/load;
  - reward idempotency;
  - calendar/festival expiry;
  - farm/shipping/economy changes.
- Reason:
  - Adapter toca quest, farm, inventory, economy e calendar.

---

## 17. Impacto em save/load

```text
Does this change save schema? NO, usa QuestStateSection.
Does this add a save section? NO.
Does this require migration? NO, salvo se criar novo payload persistido fora de QuestState.
Does this persist Unity references? NO.
```

---

## 18. Impacto em eventos

```text
Adds events: CONDITIONAL, apenas se trigger agrícola reutilizável estiver ausente.
Changes existing events: SHOULD BE NO.
Requires unsubscribe pattern: YES if runtime subscribers are created.
```

---

## 19. Impacto em UI/Unity

```text
Changes UI: NO, exceto hook de Quest Log existente se já houver.
Changes scenes: NO.
Changes prefabs: NO.
Changes ScriptableObjects/assets: NO, salvo test fixtures se seguro.
Requires PlayMode automated or final human scenario: DEFERRED_TO_FINAL_VALIDATION para fluxo integrado board/order/shipping.
```

---

## 20. Riscos técnicos

```text
Risco: criar encomenda paralela fora de QuestState.
Mitigação: adapter usa QuestState.

Risco: reward duplicado.
Mitigação: GrantedRewardIds.

Risco: order expirar sem feedback.
Mitigação: QuestState Expired + notification/Quest Log.

Risco: farm system depender de quest.
Mitigação: farm publica evento; quest consome.
```

---

## 21. Rollback

```text
Remover adapter/mapper/validator/testes criados.
Reverter hooks de quest events se houver.
Remover execution report.
Nenhum save schema ou scene/prefab deve ser alterado.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes obrigatórias e confirmar branch.
- [ ] T002 — Auditar farm/shipping/order/economy existentes.
- [ ] T003 — Auditar quest runtime base.
- [ ] T004 — Mapear FarmOrder para QuestDefinition/QuestState/Objectives.
- [ ] T005 — Implementar/ajustar adapter mínimo.
- [ ] T006 — Integrar reward idempotency.
- [ ] T007 — Adicionar validator/teste quando praticável.
- [ ] T008 — Rodar validações obrigatórias.
- [ ] T009 — Criar execution report.

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Quest|Objective|Condition|Trigger|Reward|Flag|FarmOrder|Festival|Bestiary|Fonte|MainProgression" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário integrado, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, se adapter/validator/progress logic for implementado.
- Requires EditMode tests: YES para progress/reward/expiry logic quando harness estiver disponível.
- Requires PlayMode automated or final human scenario: YES, cenário final humano deferido para aceitar board/order/shipping end-to-end.
- Requires regression test: YES se houver bugfix de order/reward/shipping existente; caso contrário NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS se C# mudou; EditMode tests PASS ou NOT RUN justificado; FarmOrder adapter report criado; reward idempotency preservada; sem save schema novo.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/03_spec_quest_farm_orders_adapter_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não duplicar sistemas canônicos existentes.
Não quebrar save/load.
Não usar UI como fonte de verdade.
Não revelar spoilers antes de discovery/visibility policy.
Não pedir human test por spec.
Não executar runtime WAVE 03 em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
