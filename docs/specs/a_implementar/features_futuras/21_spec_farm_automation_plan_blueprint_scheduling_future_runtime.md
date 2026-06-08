# SPEC — Farm Automation Plan Blueprint Scheduling Future Runtime

> **Spec ID:** `21_spec_farm_automation_plan_blueprint_scheduling_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 21 — Full Automation by NPC/Partners Future  
> **Priority:** P1  
> **Type:** Runtime / Future / Farm Automation / Planner  
> **Domain:** Automation Plan / Blueprint / Daily Schedule / Work Orders / Area Selection  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_21_FULL_AUTOMATION_NPC_PARTNERS_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere crop core, tilemap/prefab, job execution final, city schedule core, save migration or pet runtime.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/Automation/**`, `Assets/_Game/Scripts/Farm/Layout/**`, `Assets/_Game/Scripts/City/Schedule/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/21_spec_farm_automation_plan_blueprint_scheduling_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
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
  - storage I/O policy;
  - automation reports;
  - job board UI.
> **Scope:** definir/endurecer planos/blueprints/schedules de automação: áreas, work orders, repetição, prioridade, janelas e validação.  
> **Out of scope:** tilemap/prefab UI, crop implementation, job execution, save migration.

---

# /speckit.specify

## 1. Contexto

Farm v1.3 prevê plantio por plano/blueprint de campo e rotinas automatizadas configuráveis. Full automation não deve ser “ligar e esquecer”: precisa de planos, áreas, prioridades, janelas, recursos e falhas visíveis.

---

## 2. Problema

Sem planner:

```text
automação roda em todo mapa;
jobs não têm área;
blueprint planta em tile inválido;
rotina repete para sempre sem input;
prioridade indefinida consome recursos errados;
agenda do NPC é ignorada;
plano de temporada troca crop sem aviso;
save/load duplica work orders.
```

---

## 3. Objetivo

Criar/endurecer:

```text
FarmAutomationPlan;
FarmAutomationBlueprint;
AutomationWorkOrder;
AutomationScheduleWindow;
AutomationRepeatPolicy;
AutomationPriorityPolicy;
AutomationAreaConstraint;
AutomationPlanValidationResult.
```

---

## 4. Regras de design

```text
Todo plano tem área.
Todo plano tem work orders explícitas.
Repeat precisa de limites.
Planos não atravessam estação/quest/festival sem política.
Blueprint não cria crop/input.
Planner é fonte de intenção, não executor.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero criar plano para área marcada.
Como automation, quero work orders com prioridade e janela.
Como farm, quero validar tiles/áreas/estações.
Como NPC/partner, quero respeitar agenda.
Como save/load, quero plano idempotente.
```

---

## 6. Escopo

Inclui:

```text
plan/blueprint contracts;
work orders;
schedule windows;
repeat policies;
priority policies;
area constraints;
validation/tests.
```

Não inclui:

```text
job execution;
visual planner UI;
tilemap/prefab;
crop core;
pet jobs.
```

## 7. Modelo de domínio

### 7.1 FarmAutomationPlan

```text
PlanId
DisplayNameKey
FarmAreaId
OwnerPlayerId
WorkOrderIds[]
AllowedActorTypes[]
ScheduleWindowIds[]
RepeatPolicy
PriorityPolicy
StoragePolicyId
Active
Paused
CreatedDay
LastExecutedDay optional
```

### 7.2 FarmAutomationBlueprint

```text
BlueprintId
PlanId
TilePattern
AllowedTileTags[]
RequiredInputTags[]
ForbiddenTileTags[]
SeasonPolicy
WeatherPolicy
CanRotate
CanMirror
MaxAreaSize
```

### 7.3 AutomationWorkOrder

```text
WorkOrderId
PlanId
OrderType: Plant | Water | Harvest | ClearDead | ChopMarked | MineMarked | Process | MoveToStorage | BuildAssist
TargetAreaId
RequiredToolTags[]
RequiredStationTags[]
InputPolicyId
OutputPolicyId
EstimatedBudgetCost
Priority
IdempotencyKey
```

### 7.4 AutomationScheduleWindow

```text
WindowId
AllowedDays[]
StartTime
EndTime
SeasonPolicy
WeatherPolicy
FestivalPolicy
NpcScheduleConflictPolicy
```

### 7.5 AutomationRepeatPolicy

```text
None
UntilInputsExhausted
UntilSeasonEnd
FixedCount
DailyWithCap
WeeklyWithCap
ManualReviewRequired
```

---

## 8. Plan validation rules

```text
Area must exist and be unlocked.
Tiles must be valid for order type.
Inputs must be real and available through policy.
Schedule must not conflict with actor availability.
Repeat must have cap.
Plan cannot include auto-sell by default.
Endgame Mana cannot be included unless explicit future gate.
```

---

## 9. Criteria

```text
Plan/blueprint/work order contracts exist.
Area/schedule/repeat validation exists.
Planner does not execute jobs.
Repeat capped.
No auto-sell by default.
Tests cover valid plan, invalid tile, missing area, season block, schedule conflict, repeat cap, paused plan and reload no duplicate work order.
```

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Automation/FarmAutomationPlan.cs
Assets/_Game/Scripts/Farm/Automation/FarmAutomationBlueprint.cs
Assets/_Game/Scripts/Farm/Automation/AutomationWorkOrder.cs
Assets/_Game/Scripts/Farm/Automation/AutomationScheduleWindow.cs
Assets/_Game/Scripts/Farm/Automation/AutomationRepeatPolicy.cs
Assets/_Game/Scripts/Farm/Automation/AutomationPriorityPolicy.cs
Assets/_Game/Scripts/Farm/Automation/AutomationAreaConstraint.cs
Assets/_Game/Scripts/Farm/Automation/AutomationPlanValidationResult.cs
Assets/_Game/Tests/EditMode/Farm/FarmAutomationPlanBlueprintSchedulingTests.cs
```

## 11. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/Automation/**
Assets/_Game/Scripts/Farm/Layout/**
Assets/_Game/Scripts/City/Schedule/**
Assets/_Game/Scripts/UI/Farm/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/21_spec_farm_automation_plan_blueprint_scheduling_future_runtime_execution_report.md
```

## 12. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
Assets/_Game/Scripts/Pets/**
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

# /speckit.tasks

## 13. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar plan/blueprint/schedule systems.
- [ ] T003 — Consolidar plan contracts.
- [ ] T004 — Implementar validators.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

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

- Farm v1.3 prevê plantio por plano/blueprint de campo.
- Farm v1.3 prevê rotinas automatizadas configuráveis.
- Fazenda deve gerar decisões, não apenas tarefas repetitivas.
- Quase todo terreno válido pode virar área produtiva/decorativa, mas bordas, água, entradas fixas e marcos narrativos permanecem fixos.
- Companion jobs exigem área marcada, ferramenta/estação e limites de tempo/stamina.
- Automação controlada aparece como mid/late game, não early game completo.

### Deferred / future from directions

- Visual planner UI.
- Tilemap editing.
- Job execution final.
- Pet routines.
- Save migration.
- Final balancing.

### Explicitly not redefined here

- Crop core.
- Farm layout unlocks.
- City schedule core.
- Storage backend.
- Companion job execution.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu farm, companions, social, city, economy, save, UI, world e pet direction? | Lista no report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a farm automation plan/blueprint/scheduling foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
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
| Report | Execution report criado? | `docs/validation/21_spec_farm_automation_plan_blueprint_scheduling_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "AutomationPlan|AutomationBlueprint|WorkOrder|RepeatPolicy|ScheduleWindow|FarmArea|Blueprint|DailyPlan" Assets/_Game/Scripts docs/design docs/specs
rg -n "Automation|FarmAutomation|AutomationPlan|AutomationBudget|JobBoard|CompanionJob|PartnerHelper|NpcWorker|HiredWorker|FarmWorker|AutoSell|StorageAccess|InputChest|OutputChest|Blueprint|DailyPlan|WorkOrder|ProductionPlan|Audit|Pet" Assets/_Game/Scripts docs/design docs/specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
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

- Plan affects whole farm.
- Blueprint allows invalid tiles.
- Repeat infinite.
- Schedule ignores NPC availability.
- Season change silently breaks plan.
- Plan executes instead of only declaring intent.
- Work order duplicates after reload.
- Plan includes auto-sell by default.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Automation Plan Blueprint Scheduling Future Runtime

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
rg -n "Automation|FarmAutomation|AutomationPlan|JobBoard|PartnerHelper|CompanionJob|StorageAccess|Blueprint|WorkOrder|ProductionPlan|AutoSell|HiredWorker|NpcWorker" Assets/_Game/Scripts docs/design docs/specs
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

- Changed deterministic logic: YES, plan/blueprint/schedule validation logic is deterministic.
- Requires EditMode tests: YES for valid/invalid-tile/area/season/schedule/repeat/pause/reload tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for planner UI/gameplay validation.
- Requires regression test: YES if fixing existing automation plan bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; planner declares bounded intent only.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/21_spec_farm_automation_plan_blueprint_scheduling_future_runtime_execution_report.md.
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
