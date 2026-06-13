# SPEC — Farm Automation Reports Risk Cost Balance Future Runtime

> **Spec ID:** `21_spec_farm_automation_reports_risk_cost_balance_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 21 — Full Automation by NPC/Partners Future  
> **Priority:** P2  
> **Type:** Runtime / Future / Automation / Reporting / Balance  
> **Domain:** Automation Reports / Risk Cost / Expected Output / Failure Reasons / UI Projection  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_21_FULL_AUTOMATION_NPC_PARTNERS_FUTURE  
> **Can run with:** automation planner if no same UI/report files.  
> **Must not run with:** qualquer spec que altere job execution, economy formula, UI prefab/layout, save migration, pet runtime or final balancing values.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/Automation/Reports/**`, `Assets/_Game/Scripts/UI/Farm/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/21_spec_farm_automation_reports_risk_cost_balance_future_runtime_execution_report.md`  
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
  - automation planner;
  - automation governor;
  - storage I/O;
  - job board UI;
  - final human validation.
> **Scope:** definir/endurecer relatórios/projections de automação: custo, risco, output esperado/real, falhas, warnings e review manual.  
> **Out of scope:** visual UI, final balancing, job execution, economy formula, save migration.

---

# /speckit.specify

## 1. Contexto

Full automation precisa ser legível. O jogador deve saber custo, risco, duração, inputs, outputs esperados, falhas e limites. Relatório não é fonte de verdade, apenas projection/auditoria para validar automação e impedir “caixa preta”.

---

## 2. Problema

Sem relatório:

```text
jogador não entende por que output foi menor;
falha de storage parece bug;
helper cansado parece não funcionar;
job consome input sem feedback;
automação parece mágica;
economia quebra sem alerta;
risco de endgame/Mana não aparece;
review manual não existe para partial/fail.
```

---

## 3. Objetivo

Criar/endurecer:

```text
AutomationExecutionReport;
AutomationExpectedOutputProjection;
AutomationActualOutputRecord;
AutomationFailureReason;
AutomationRiskWarning;
AutomationCostBreakdown;
AutomationManualReviewItem;
AutomationReportRetentionPolicy.
```

---

## 4. Regras de design

```text
Report é projection/audit, não executor.
Expected output deve ser estimativa, não promessa.
Actual output vem de transação real.
Falhas devem explicar missing input, missing tool, storage full, actor unavailable, schedule conflict, budget exceeded, weather/season block.
High-risk/endgame jobs exigem warning.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero ver o que a automação tentou fazer.
Como UI, quero mostrar custo/risco/output.
Como economy, quero detectar loops e outputs suspeitos.
Como storage, quero reportar partial/failure.
Como QA, quero auditoria de cada automation tick/day.
```

---

## 6. Escopo

Inclui:

```text
execution report contracts;
expected/actual output;
failure reasons;
risk warnings;
cost breakdown;
manual review items;
retention policy;
tests.
```

Não inclui:

```text
visual UI final;
job execution;
economy formulas;
save migration;
pet reports.
```

## 7. Modelo de domínio

### 7.1 AutomationExecutionReport

```text
ReportId
PlanId
WorkOrderId
ActorId
Day
StartedAt
FinishedAt optional
State: Planned | Skipped | Partial | Completed | Failed | NeedsReview
ExpectedOutput
ActualOutput
CostBreakdown
FailureReasons[]
RiskWarnings[]
ManualReviewItems[]
IdempotencyKey
```

### 7.2 AutomationExpectedOutputProjection

```text
InputSummary[]
EstimatedOutputSummary[]
EstimatedDuration
EstimatedBudgetCost
EstimatedStaminaCost
RiskClass
Confidence: Low | Medium | High
CanShowExactNumbers
```

### 7.3 AutomationActualOutputRecord

```text
InputConsumed[]
OutputCreated[]
OutputDeposited[]
OutputLost should_be_empty
StorageOverflow[]
RelationshipDelta optional
FatigueDelta optional
Warnings[]
```

### 7.4 AutomationFailureReason

```text
MissingInput
MissingTool
MissingStation
InvalidArea
InvalidTile
StorageFull
ActorUnavailable
ActorExhausted
ScheduleConflict
BudgetExceeded
WeatherBlocked
SeasonBlocked
QuestBlocked
EndgameBlocked
PetDeferred
UnknownConflict
```

### 7.5 AutomationRiskWarning

```text
HighValueInput
RareResource
EndgameMana
AutoSellBlocked
LowConfidence
StorageNearFull
ActorFatigueHigh
SeasonEnding
FestivalConflict
```

### 7.6 AutomationManualReviewItem

```text
ReviewId
ReportId
Reason
SuggestedAction: Retry | EditPlan | AddInput | ChangeStorage | PausePlan | AcceptPartial | Cancel
BlocksNextRun
```

---

## 8. Report rules

```text
Expected output before execution.
Actual output after transaction commit.
Partial/fail requires reasons.
No hidden item loss.
High-risk warnings before execution.
Endgame Mana warnings always.
Reports retained with cap.
Debug data not shown in final UI.
```

---

## 9. Criteria

```text
Report/projection contracts exist.
Failure reasons explicit.
Risk warnings explicit.
Manual review item exists.
Report does not mutate gameplay.
Tests cover expected vs actual, missing input, storage full, budget exceeded, endgame warning, manual review, retention cap and no hidden loss.
```

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Automation/Reports/AutomationExecutionReport.cs
Assets/_Game/Scripts/Farm/Automation/Reports/AutomationExpectedOutputProjection.cs
Assets/_Game/Scripts/Farm/Automation/Reports/AutomationActualOutputRecord.cs
Assets/_Game/Scripts/Farm/Automation/Reports/AutomationFailureReason.cs
Assets/_Game/Scripts/Farm/Automation/Reports/AutomationRiskWarning.cs
Assets/_Game/Scripts/Farm/Automation/Reports/AutomationCostBreakdown.cs
Assets/_Game/Scripts/Farm/Automation/Reports/AutomationManualReviewItem.cs
Assets/_Game/Scripts/Farm/Automation/Reports/AutomationReportRetentionPolicy.cs
Assets/_Game/Tests/EditMode/Farm/FarmAutomationReportsRiskCostBalanceTests.cs
```

## 11. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/Automation/**
Assets/_Game/Scripts/UI/Farm/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/21_spec_farm_automation_reports_risk_cost_balance_future_runtime_execution_report.md
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
- [ ] T002 — Auditar automation report/UI systems.
- [ ] T003 — Consolidar report contracts.
- [ ] T004 — Implementar report validators.
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

- Farm deve gerar decisões, não apenas tarefas repetitivas.
- Job board deve mostrar tempo/custo/resultado esperado e evitar que NPC faça tudo sem comando.
- Companion de fazenda deve reduzir repetição, não remover planejamento.
- Jobs exigem output rules/failure rules/fatigue/stamina.
- Economy directions exigem anti-exploit e transparência suficiente para não parecer bug.
- UI/UX exige feedback claro e notificações úteis sem cobrir gameplay.

### Deferred / future from directions

- Visual UI final.
- Job execution final.
- Economy values.
- Save migration.
- Pet reports.
- Final balancing.

### Explicitly not redefined here

- Notification journal.
- Economy formula.
- Storage backend.
- Companion job rules.
- Planner.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu farm, companions, social, city, economy, save, UI, world e pet direction? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a farm automation reports/risk/cost/balance foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
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
| Report | Execution report criado? | `docs/validation/21_spec_farm_automation_reports_risk_cost_balance_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "AutomationReport|ExpectedOutput|ActualOutput|FailureReason|RiskWarning|ManualReview|CostBreakdown|EndgameMana" Assets/_Game/Scripts docs/design .specs
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

- Report becomes gameplay source.
- Expected output shown as guaranteed.
- Hidden item loss.
- Partial failure unexplained.
- Endgame Mana warning missing.
- Retention grows unbounded.
- Debug IDs in final UI.
- Manual review missing for partial state.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Automation Reports Risk Cost Balance Future Runtime

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

- Changed deterministic logic: YES, report/projection/retention validation logic is deterministic.
- Requires EditMode tests: YES for expected/actual/failures/risk/review/retention/no-loss tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for automation report UI validation.
- Requires regression test: YES if fixing existing automation report bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; automation reports transparent and bounded.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/21_spec_farm_automation_reports_risk_cost_balance_future_runtime_execution_report.md.
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
