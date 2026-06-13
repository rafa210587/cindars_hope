# SPEC — Farm Automation Storage IO Idempotency Future Runtime

> **Spec ID:** `21_spec_farm_automation_storage_io_idempotency_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 21 — Full Automation by NPC/Partners Future  
> **Priority:** P1  
> **Type:** Runtime / Future / Automation / Storage I/O  
> **Domain:** Storage Access / Input Output Chest / Idempotency / No Duplicate Output  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_21_FULL_AUTOMATION_NPC_PARTNERS_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere inventory/storage backend, shop/sell core, save migration, job execution final, scene/prefab assets or pet runtime.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/Automation/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Scripts/Storage/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/21_spec_farm_automation_storage_io_idempotency_future_runtime_execution_report.md`  
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
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
> **Blocks:**  
  - automation governor;
  - automation planner;
  - automation reports;
  - economy anti-arbitrage.
> **Scope:** definir/endurecer contratos de I/O de automação: input/output chests, permissões, reservations, commits, rollback e idempotency keys.  
> **Out of scope:** storage backend rewrite, inventory rewrite, shop/sell implementation, save migration.

---

# /speckit.specify

## 1. Contexto

Full automation só é segura se consumir inputs reais e depositar outputs em storage permitido. Companions/partners/workers não podem acessar inventário do jogador livremente, criar item do nada ou vender automaticamente sem política explícita.

---

## 2. Problema

Sem I/O policy:

```text
job consome item do inventário pessoal sem permissão;
input é lido mas não consumido;
output duplica após reload;
storage cheio apaga item;
auto-sell vende item raro;
job parcial fica sem rollback;
dois helpers usam mesmo input;
reservation expira e gera inconsistência.
```

---

## 3. Objetivo

Criar/endurecer:

```text
AutomationStorageAccessPolicy;
AutomationInputReservation;
AutomationOutputCommit;
AutomationStorageTransaction;
AutomationIdempotencyKey;
AutomationRollbackPolicy;
AutomationStorageFailureReason;
AutomationStorageAuditRecord.
```

---

## 4. Regras de design

```text
Automação não acessa player inventory por padrão.
Inputs vêm de storage autorizado.
Outputs vão para output storage autorizado.
Transações são idempotentes.
Storage cheio gera partial/fail, não perda silenciosa.
Auto-sell é blocked por default.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero controlar quais baús a automação usa.
Como automation, quero reservar input e commitar output com segurança.
Como save/load, quero impedir duplicação.
Como economy, quero bloquear auto-sell massivo.
Como UI, quero relatório de falhas claro.
```

---

## 6. Escopo

Inclui:

```text
storage access policy;
input reservation;
output commit;
transaction/rollback;
idempotency key;
audit record;
tests.
```

Não inclui:

```text
inventory backend rewrite;
storage UI;
shop/sell core;
save migration;
job execution final.
```

## 7. Modelo de domínio

### 7.1 AutomationStorageAccessPolicy

```text
PolicyId
AllowedInputStorageIds[]
AllowedOutputStorageIds[]
CanUsePlayerInventory false
CanUseSharedFarmStorage
CanUseCategoryChests
CanAutoSell false
ProtectedItemTags[]
MaxInputValuePerDay optional
MaxOutputValuePerDay optional
```

### 7.2 AutomationInputReservation

```text
ReservationId
WorkOrderId
StorageId
ItemId
ItemInstanceIds[]
Quantity
ReservedDay
ExpiresAtGameTime optional
Consumed
Released
IdempotencyKey
```

### 7.3 AutomationOutputCommit

```text
CommitId
WorkOrderId
OutputStorageId
ItemOutputs[]
CommitDay
Committed
Partial
Failed
OverflowPolicy
IdempotencyKey
```

### 7.4 AutomationStorageTransaction

```text
TransactionId
PlanId
WorkOrderId
ActorId
InputReservations[]
OutputCommits[]
State: Pending | InputsReserved | OutputsCommitted | RolledBack | Failed | Partial
FailureReasons[]
IdempotencyKey
```

### 7.5 AutomationRollbackPolicy

```text
RollbackInputsIfOutputFails
ReleaseReservationsIfSkipped
DoNotDuplicateOutputsOnRetry
ManualReviewIfPartial
OverflowToMailFuture false
OverflowToGround false
```

---

## 8. I/O rules

```text
Reserve before execution.
Consume only after execution confirmed.
Commit output once.
If output storage full:
  partial/fail/manual review, not delete.
If job skipped:
  release reservation.
If reload:
  resume or normalize by transaction state.
If transaction already committed:
  do not commit again.
```

---

## 9. Criteria

```text
Storage I/O contracts exist.
Player inventory inaccessible by default.
Input reservation and output commit idempotent.
Auto-sell blocked by default.
Rollback policies explicit.
Tests cover reservation, consume, release, commit, storage full, reload pending, reload committed no duplicate, two workers same input and auto-sell blocked.
```

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Automation/Storage/AutomationStorageAccessPolicy.cs
Assets/_Game/Scripts/Farm/Automation/Storage/AutomationInputReservation.cs
Assets/_Game/Scripts/Farm/Automation/Storage/AutomationOutputCommit.cs
Assets/_Game/Scripts/Farm/Automation/Storage/AutomationStorageTransaction.cs
Assets/_Game/Scripts/Farm/Automation/Storage/AutomationIdempotencyKey.cs
Assets/_Game/Scripts/Farm/Automation/Storage/AutomationRollbackPolicy.cs
Assets/_Game/Scripts/Farm/Automation/Storage/AutomationStorageFailureReason.cs
Assets/_Game/Scripts/Farm/Automation/Storage/AutomationStorageAuditRecord.cs
Assets/_Game/Tests/EditMode/Farm/FarmAutomationStorageIOIdempotencyTests.cs
```

## 11. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/Automation/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Storage/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/21_spec_farm_automation_storage_io_idempotency_future_runtime_execution_report.md
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
- [ ] T002 — Auditar inventory/storage/automation systems.
- [ ] T003 — Consolidar storage I/O contracts.
- [ ] T004 — Implementar idempotency validators.
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

- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/social/SOCIAL_RELATIONSHIP_ROMANCE_DIRECTION.md
- docs/design/gameplay/city/CITY_DESIGN_DIRECTION_v1.2.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md

### Required interpretation

```text
Esta spec é future/mapped.
Ela deriva principalmente de FARM_DESIGN_DIRECTION_v1.3 e COMPANIONS_DIRECTION.
Ela não substitui as specs anteriores de companion jobs, partner helper, city schedules, economy, storage, crafting, save/load ou UI.
Ela só define uma camada futura de full automation por NPC/partners, com budget, limites, auditoria e failsafes.
Automação deve reduzir repetição sem remover planejamento.
NPC/partner/helper/companion ajuda, mas não joga pelo jogador.
Pets continuam deferidos: nenhuma automação de pets deve ser implementada aqui.
Quando houver conflito entre Farm, Companions, Social, City schedule, Economy, Save ou UI directions, o executor deve parar e registrar CONFLICT.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Jobs de companion exigem storage autorizado e output rules.
- Companion não cria item do nada nem coleta recurso fora do estado real do mundo.
- Economy directions bloqueiam arbitragem e loops de venda/restock.
- Automação por NPC/partners deve ser future e dependente de planejamento.
- Farm v1.3 prevê storage central e workshops em nível alto.
- Save/load deve preservar estado por IDs, não referências Unity.

### Deferred / future from directions

- Storage UI.
- Inventory backend rewrite.
- Shop/sell core.
- Save migration.
- Pet storage/routine.
- Job execution.

### Explicitly not redefined here

- Inventory slot model.
- Storage backend.
- Shipping/SellPoint core.
- Economy formulas.
- Companion jobs.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu farm, companions, social, city, economy, save, UI, world e pet direction? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a farm automation storage IO/idempotency foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Planejamento | Automação exige plano, área, input, storage, ferramenta/estação, agenda ou orçamento. | Tests/checklist. | PARTIAL |
| Não joga pelo jogador | Automação não colhe/planta/processa/vende/minera tudo sem comando e não substitui build/skill tree. | Tests/checklist. | BLOCKED se violar |
| Anti-economia | Automação não cria item do nada, não auto-sell em massa, não duplica output e não gera arbitragem. | Tests/checklist. | PARTIAL |
| Idempotência | Day transition, reload, scene reload e retry não duplicam jobs/outputs/rewards. | Tests/checklist. | PARTIAL |
| Pet deferido | Nenhum runtime, job, HUD, data asset ou save de pet foi criado. | Checklist explícito. | BLOCKED se violar |
| Social/partner seguro | Partner helper não torna romance/casamento obrigatório nem gera power path dominante. | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/21_spec_farm_automation_storage_io_idempotency_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "AutomationStorage|InputReservation|OutputCommit|StorageTransaction|IdempotencyKey|AutoSell|Rollback|InputChest|OutputChest" Assets/_Game/Scripts docs/design .specs
rg -n "Automation|FarmAutomation|AutomationPlan|AutomationBudget|JobBoard|CompanionJob|PartnerHelper|NpcWorker|HiredWorker|FarmWorker|AutoSell|StorageAccess|InputChest|OutputChest|Blueprint|DailyPlan|WorkOrder|ProductionPlan|Audit|Pet" Assets/_Game/Scripts docs/design .specs
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
Given a fazenda tem NPC/partner/companion elegível e recursos reais disponíveis
When o jogador cria plano/job/automação dentro dos limites
Then o sistema executa apenas ações autorizadas
And consome inputs reais
And deposita outputs em storage permitido
And registra auditoria/idempotência
And não vende, duplica ou gera item do nada.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Missing resources / blocked automation

```text
Given falta ferramenta, estação, storage, área, input, agenda, stamina, orçamento, relação ou permissão
When job/automação tenta rodar
Then a execução é skipped/partial/failed de forma explícita
And nenhum item é criado
And relatório mostra motivo sem mutar estado indevido.
```

### Scenario 4 — Anti-exploit and idempotency

```text
Given day transition, reload, menu reopen, scene reload or retry
When automação já foi calculada ou parcialmente aplicada
Then outputs não duplicam
And auto-sell não ocorre sem política explícita
And economia/storage/crafting permanecem bounded.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay de job board, planner, automation report, storage routing ou NPC/partner helper animation
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Input consumed twice.
- Output committed twice.
- Storage full deletes output.
- Job uses player inventory silently.
- Two actors reserve same item.
- Auto-sell mass enabled.
- Rollback duplicates items.
- Transaction state corrupt after reload.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Automation Storage IO Idempotency Future Runtime

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

## Automation compliance
- Automation requires player plan:
- No item from nothing:
- Real input consumed:
- Storage permissions respected:
- No auto-sell exploit:
- No reload/day duplicate:
- Companion/partner budget respected:
- City schedule respected:
- No pet runtime:
- Save/load safe:
- UI not source of truth:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial:
- Missing resources:
- Anti-exploit/idempotency:
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
2. A implementação exigir scene/prefab/tilemap/asset/font/icon/audio changes.
3. A implementação alterar save schema sem migration spec.
4. A implementação automatizar fazenda inteira sem plano/área/input/orçamento/comando do jogador.
5. A implementação criar item do nada ou duplicar output em reload/day transition.
6. A implementação vender automaticamente em massa ou criar arbitragem.
7. A implementação ignorar tool/station/storage/city schedule/stamina/relationship gates.
8. A implementação tornar romance/casamento/partner obrigatório para automação competitiva.
9. A implementação criar pet runtime/job/HUD/save/data asset.
10. A implementação substituir companion jobs/partner helper specs já geradas em vez de construir sobre elas.
11. Não for possível decidir se sistema existente é canônico ou obsoleto.
```



---

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Automation|FarmAutomation|AutomationPlan|JobBoard|PartnerHelper|CompanionJob|StorageAccess|Blueprint|WorkOrder|ProductionPlan|AutoSell|HiredWorker|NpcWorker" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de automation planner, job board, storage routing, NPC worker, partner helper ou reports, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, storage reservation/commit/rollback/idempotency logic is deterministic.
- Requires EditMode tests: YES for reserve/consume/release/commit/full/reload/same-input/autosell tests.
- Requires PlayMode automated or final human scenario: NO by default; contracts/services only.
- Requires regression test: YES if fixing existing automation storage duplication; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; storage I/O idempotent and bounded.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/21_spec_farm_automation_storage_io_idempotency_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não automatizar tudo sem decisão do jogador.
Não criar item do nada.
Não duplicar outputs.
Não auto-vender em massa.
Não ignorar storage/tool/station/stamina/schedule.
Não tornar romance/casamento obrigatório.
Não criar pet runtime.
Não editar scenes/prefabs/assets.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped. Deve ser executada apenas quando farm jobs, storage, economy, crafting, city schedule, companion/partner base, save/load and UI planner foundations estiverem estáveis ou quando houver decisão humana explícita de antecipar full automation.
