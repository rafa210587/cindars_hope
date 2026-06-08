# SPEC — Companion Farm Jobs Board Automation Future Runtime

> **Spec ID:** `14_spec_companion_farm_jobs_board_automation_future_runtime`  
> **Status:** A implementar / Future mapped  
> **Wave:** WAVE 14 — Companions / Jobs / Cave Assist / Future Social Hooks  
> **Priority:** P1  
> **Type:** Runtime / Future / Farm Automation / Companion Jobs  
> **Domain:** Companion / Farm Jobs / Job Board / Automation Limits / Output Rules  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_14_COMPANIONS_FUTURE  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere farm crop runtime, inventory/storage backend, city schedule, save migration, pet jobs, UI prefab/job board layout ou economy balance final.  
> **Repo lock scope:** `Assets/_Game/Scripts/Companions/**`, `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Tests/EditMode/Companions/**`, `docs/validation/14_spec_companion_farm_jobs_board_automation_future_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/companions/COMPANIONS_DIRECTION.md`
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md`
  - `docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md`
  - `docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md`
  - `docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md`
  - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`
  - `docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md`
  - `docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md`
  - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`
  - `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`
  - `docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md`
  - `docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_ENEMY_BEHAVIOR_ADAPTER.md`
  - `docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md`
  - `docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md`
  - `docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md`
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md`
> **Blocks:**  
  - job board UI;
  - farm automation validations;
  - companion relationship/job rank;
  - economy anti-exploit;
  - day transition jobs.
> **Scope:** definir/endurecer jobs de fazenda por companion com board, área marcada, ferramenta/estação, stamina, horários, output/failure rules e limites anti-automação.  
> **Out of scope:** crop/animal/process runtime rewrite, visual board UI, pet jobs, full economy balance, save migration.

---

# /speckit.specify

## 1. Contexto

Companions podem ajudar em jobs da fazenda, mas devem reduzir repetição sem remover planejamento. Jobs exigem área marcada, ferramenta/estação, limites de tempo/stamina e não podem criar item do nada.

Esta spec cria o runtime futuro de job contracts, não o visual final do board.

---

## 2. Problema

Sem job rules:

```text
companion planta/colhe tudo sozinho no early game;
job cria item sem world state real;
storage é acessado sem permissão;
job ignora clima/season/horário/ferramenta;
companion trabalha com stamina infinita;
output gera economia infinita;
pet vira worker sem spec;
NPC teleporta para fazenda sem agenda.
```

---

## 3. Objetivo

Criar/endurecer:

```text
CompanionFarmJobDefinition;
CompanionJobType;
CompanionJobAssignment;
CompanionJobBoardState;
AllowedArea;
AllowedToolOrStation;
StaminaBudget;
OutputRules;
FailureRules;
JobExecutionResult;
JobAutomationValidator.
```

---

## 4. Regras de design

```text
Companion de fazenda reduz repetição, não remove planejamento.
Jobs exigem área marcada, ferramenta/estação e limites.
Early game companion faz pouco e ensina.
Mid/late melhora automação controlada.
Companion não cria item do nada.
Companion não coleta recurso fora do estado real do mundo.
Job board limita automação e mostra tempo/custo/resultado esperado.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero atribuir job com área, tempo, ferramenta e resultado esperado.
Como farm, quero validar se crop/item/estação existem.
Como companion, quero consumir stamina/fatigue e respeitar vínculo/job rank.
Como economy, quero impedir output infinito.
Como save/load, quero preservar job do dia sem duplicar resultado.
```

---

## 6. Escopo

Inclui:

```text
farm job definitions;
job board state;
assignment rules;
allowed area/tool/station;
time/stamina/fatigue;
output/failure rules;
storage access policy;
anti-economy exploit;
tests.
```

Não inclui:

```text
board prefab/layout;
crop/animal/process backend rewrite;
pet jobs;
full relationship system;
final economy tuning.
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

- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md
- docs/design/gameplay/city/CITY_NPC_ROSTER_SERVICES_DIRECTION_v1.1.md
- docs/design/gameplay/city/CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md
- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
- docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
- docs/design/gameplay/player/PLAYER_DERIVED_ATTRIBUTES_DIRECTION.md
- docs/design/gameplay/player/PLAYER_SKILL_TREES_DIRECTION.md
- docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
- docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
- docs/design/gameplay/enemies/ENEMY_BEHAVIORS_DIRECTION.md
- docs/design/gameplay/cave/CAVE_DESIGN_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_DIRECTION.md
- docs/design/gameplay/cave/CAVE_MONSTER_ROSTER_ENEMY_BEHAVIOR_ADAPTER.md
- docs/design/gameplay/cave/CAVE_COMBAT_BALANCE_VULNERABILITIES_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_ENEMY_VULNERABILITY_ADAPTER.md
- docs/design/gameplay/magic/MAGIC_SPELLS_ACTIONS_DIRECTION.md
- docs/design/gameplay/magic/MAGIC_LEARNING_UNLOCKS_SOURCES_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md
- docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_LORE_DIRECTION.md
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md
- docs/design/gameplay/bestiary/BESTIARY_KNOWLEDGE_DISCOVERY_DIRECTION.md

### Required interpretation

```text
Esta spec deriva de COMPANIONS_DIRECTION.
Ela não redefine NPC concreto, romance/casamento detalhado, pet, enemy AI, loot tables, fórmula final de dano/HP/MP/Stamina, layout de fazenda/cidade/caverna ou stats de monstros.
Ela deve respeitar que Pet é sistema separado e está explicitamente deferido.
Ela deve preservar a regra: companion ajuda, mas não joga pelo jogador.
Quando houver conflito com roster de cidade, farm, combat, enemy behavior, cave balance, pets ou economy directions, o executor deve parar e registrar CONFLICT.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Farm jobs devem ser explícitos com JobId, CompanionId, JobType, AllowedArea, AllowedToolOrStation, StartTime, EndTime, StaminaBudget, OutputRules, FailureRules, RelationshipGain e FatigueGain.
- Job board atribui tarefa, limita automação, mostra tempo/custo, mostra resultado esperado, evita que NPC faça tudo sem comando e permite planejamento diário.
- Novos tipos de job abrem com relação, construção, tool upgrades ou progresso.
- Companion deve respeitar área marcada, horário, ferramenta, stamina diária, vínculo/job, storage autorizado, estações construídas, clima/season.
- Companion não deve criar item do nada nem coletar recurso fora do estado real do mundo.
- Companion de fazenda deve reduzir repetição, não remover planejamento.

### Deferred / future from directions

- Pet jobs.
- Farm defense/invasões.
- Full worker/helper hired system.
- Board visual final.
- Animal product final system.
- Economy tuning final.

### Explicitly not redefined here

- Farm crop/animal/product state.
- Inventory/storage backend.
- City schedule and NPC movement.
- Relationship system.
- Pet system.
- Save migration.

## 7. Modelo de domínio

### 7.1 CompanionJobType

```text
Plant
Harvest
ChopMarkedTrees
MineMarkedRocks
AnimalCareFuture
OperateWorkshop
AlchemyProcessing
BuildAssist
ForageMarkedArea
CleanFarmArea
```

### 7.2 CompanionFarmJobDefinition

```text
JobType
RequiredRole
RequiredUnlockFlags[]
RequiredBuildingIds[]
RequiredToolTags[]
RequiredStationTags[]
AllowedSeasonPolicy
AllowedWeatherPolicy
BaseDuration
BaseStaminaCost
BaseFatigueGain
OutputPolicyId
FailurePolicyId
CanRunEarlyGame
```

### 7.3 CompanionJobAssignment

```text
AssignmentId
CompanionId
JobType
AllowedAreaId
AllowedToolOrStationId optional
StorageAccessPolicyId
StartTime
EndTime
StaminaBudget
Priority
RepeatPolicy: None | DailyUntilCanceled | ManualOnly
AssignedDay
```

### 7.4 CompanionJobBoardState

```text
UnlockedJobTypes[]
Assignments[]
DailyCapacity
UsedCapacity
BoardLevel
AutomationLimitPolicy
Warnings[]
```

### 7.5 JobExecutionResult

```text
Success
PartialSuccess
Failed
SkippedUnavailable
SkippedMissingTool
SkippedMissingInput
SkippedBadWeather
Outputs[]
InputsConsumed[]
StaminaUsed
FatigueGained
RelationshipGain
Warnings[]
```

---

## 8. Output rules

```text
Output must be derived from real farm world/inventory state.
Planting consumes real seeds.
Harvesting requires mature crop in allowed area.
Processing consumes real input and built station.
Chopping/mining requires marked tree/rock or allowed area resource.
AnimalCareFuture requires future animal system and is blocked now unless available.
No job creates item from nothing.
No job auto-sells.
No job opens shipping by default.
```

---

## 9. Storage access policy

```text
NoStorageAccess:
  job cannot consume/store items.

LimitedInputChest:
  can consume inputs from assigned chest/area only.

LimitedOutputChest:
  can deposit outputs in assigned chest only.

FarmSharedStorageFuture:
  requires explicit farm storage system.

Never access player inventory without explicit command.
```

---

## 10. Automation progression

```text
BoardLevel 0:
  no jobs or tutorial placeholder.

BoardLevel 1:
  simple daily manual job, small area.

BoardLevel 2:
  more job types, small repeat.

BoardLevel 3:
  larger area and better output.

Late:
  still bounded by stamina/time/capacity.
```

---

## 11. Criteria

```text
Farm job contracts exist.
Job assignment requires area/tool/station/time/stamina.
Output derives from real state.
Job board caps automation.
No pet job.
No infinite output/economy.
Tests cover valid assignment, missing tool, missing input, bad weather/season, storage policy, output derivation, daily cap and no auto-sell.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Companions/FarmJobs/CompanionJobType.cs
Assets/_Game/Scripts/Companions/FarmJobs/CompanionFarmJobDefinition.cs
Assets/_Game/Scripts/Companions/FarmJobs/CompanionJobAssignment.cs
Assets/_Game/Scripts/Companions/FarmJobs/CompanionJobBoardState.cs
Assets/_Game/Scripts/Companions/FarmJobs/CompanionJobExecutionService.cs
Assets/_Game/Scripts/Companions/FarmJobs/CompanionJobAutomationValidator.cs
Assets/_Game/Tests/EditMode/Companions/CompanionFarmJobsTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Companions/**
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Companions/**
docs/validation/14_spec_companion_farm_jobs_board_automation_future_runtime_execution_report.md
```

---

## 14. Arquivos proibidos

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

---

## 15. Estratégia

```text
1. Auditar farm automation/job systems.
2. Consolidar job definition/assignment/board.
3. Implementar execution service with dry-run validation.
4. Enforce output real-state and storage policy.
5. Criar tests.
6. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - farm crop/runtime rewrite;
  - inventory/storage rewrite;
  - save migration;
  - companion state foundation;
  - pet runtime.
- Reason: jobs cross farm, inventory, economy, schedule and save.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if Companion/Farm job state exists; otherwise STOP.
Does this add save section? NO unless dedicated migration is approved.
Does this require migration? NO unless new job state is persisted without existing section; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. CompanionJobAssignedEvent, CompanionJobCompletedEvent.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final; exposes job board state.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for board/job visual flow.
```

---

## 20. Riscos

```text
Risco: output duplicates on day transition.
Mitigação: idempotency tests.

Risco: storage/inventory systems missing.
Mitigação: DEFER or adapter only.

Risco: job automates too much.
Mitigação: board capacity/stamina/time caps.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar farm job systems.
- [ ] T003 — Consolidar job contracts.
- [ ] T004 — Implementar validation/execution.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | Executor leu Companions, Pets, City, Farm, Combat, Cave, Economy, UI e Save directions? | Lista no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a companion farm jobs/job board automation foram auditados antes de criar novos? | Comandos `rg` e achados. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Companion ajuda | Companion não joga pelo jogador, não substitui build e não é obrigatório para terminar o jogo? | Tests/checklist. | PARTIAL |
| Pet separado | Pet não foi implementado nem tratado como companion? | Checklist explícito. | BLOCKED se violar |
| Romance separado | Romance/casamento profundo não foi implementado? | Checklist explícito. | BLOCKED se violar |
| Balance | Companion não tanka boss, não cura infinito, não farma loot e respeita active combat budget? | Tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| UI/PlayMode | Há fluxo visual/gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Lógica determinística nova tem EditMode tests quando praticável? | Test list. | PARTIAL |
| Report | Execution report criado? | `docs/validation/14_spec_companion_farm_jobs_board_automation_future_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "CompanionJob|FarmJob|JobBoard|AllowedArea|StaminaBudget|OutputRules|StorageAccess|FatigueGain|AnimalCaretaker|Pet" Assets/_Game/Scripts docs/design docs/specs
rg -n "Companion|FarmCompanion|CaveCompanion|CompanionJob|CompanionBrain|CompanionState|CompanionSave|Relationship|Romance|Spouse|Pet|Breath|Folego|Boss|ActiveCombatBudget|JobBoard" Assets/_Game/Scripts docs/design docs/specs
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
Given companion elegível/desbloqueado/disponível
When o jogador usa o fluxo desta spec
Then companion oferece ajuda limitada e explícita
And não substitui o jogador
And não ativa pet runtime
And não cria romance/casamento profundo.
```

### Scenario 2 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 3 — Balance guardrail

```text
Given companion está em job/fazenda/caverna/combate
When o sistema calcula ajuda, ação ou output
Then há limite por stamina/tempo/cooldown/área/ferramenta/vínculo/capacidade
And companion não gera recurso, cura, loot, dano ou progressão infinita.
```

### Scenario 4 — Pet/romance separation

```text
Given NPC companion, pet future ou romance candidate
When a spec cria contratos de companion
Then pet permanece sistema separado/deferido
And romance/casamento detalhado fica fora do escopo
And spouse não vira combat companion automaticamente.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual/gameplay de convite, job board, cave entry, HUD ou companion behavior
When a implementação técnica terminar
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Job creates item from nothing.
- Job consumes player inventory without consent.
- Job ignores area/tool/station.
- Job runs with infinite stamina/time.
- Job repeats output after reload/day transition.
- Job auto-sells/shipping items.
- Pet job added accidentally.
- Automation removes planning.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Companion Farm Jobs Board Automation Future Runtime

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

## Companion compliance
- Companion helps but does not play for player:
- No pet runtime:
- No romance/deep marriage:
- No Breath/Folego:
- Active combat budget respected:
- Boss guardrails:
- Farm automation limits:
- Save/load safe:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Balance guardrail:
- Pet/romance separation:
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
2. A implementação exigir scene/prefab/asset wiring fora do escopo.
3. A implementação exigir save migration sem spec de migration.
4. A implementação criar pet runtime, pet save, pet HUD ou pet data assets.
5. A implementação criar romance/casamento profundo ou spouse system.
6. A implementação tornar companion obrigatório para terminar main quest.
7. A implementação permitir companion tankar boss, curar infinito, matar boss sozinho ou farmar enemies sem jogador ativo.
8. A implementação permitir companion gerar loot/recurso/economia infinita.
9. A implementação adicionar Breath/Fôlego como recurso de companion.
10. A implementação tratar classes de D&D como papéis mecânicos de companion.
11. Não for possível decidir se sistema existente é canônico ou obsoleto.
```



## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Companion|FarmCompanion|CaveCompanion|CompanionJob|CompanionBrain|CompanionState|Relationship|Romance|Spouse|Pet|Breath|Folego|Boss|ActiveCombatBudget" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário visual/gameplay de companion invite, job board, cave entry, HUD, combat assist ou farm jobs, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, job validation/execution/output/cap logic is deterministic.
- Requires EditMode tests: YES for assignment/missing-tool/missing-input/weather/storage/output/cap/idempotency tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for job board and farm job visual validation.
- Requires regression test: YES if fixing existing farm job automation bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no output from nothing; automation capped.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/14_spec_companion_farm_jobs_board_automation_future_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não implementar Pet runtime.
Não implementar romance/casamento profundo.
Não adicionar Breath/Fôlego.
Não usar classes de D&D como papéis mecânicos.
Não tornar companion obrigatório.
Não deixar companion jogar pelo jogador.
Não gerar loot/economia infinita.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec é future/mapped. Deve ser executada apenas quando City/NPC, Farm, Cave, Combat, Save, UI e Quest estiverem estáveis ou quando houver decisão humana explícita de antecipar Companions.
