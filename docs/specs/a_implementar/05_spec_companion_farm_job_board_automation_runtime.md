# SPEC — Companion Farm Job Board Automation Runtime

> **Spec ID:** `05_spec_companion_farm_job_board_automation_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Animals / Companions  
> **Priority:** P1  
> **Type:** Runtime / Companions / Farm Jobs / Job Board / Controlled Automation  
> **Domain:** Companions / Farm Jobs / Job Board / Automation Limits  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_ANIMALS_COMPANIONS_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere companion eligibility/bond, crop growth, watering, harvest, animal care, processing, storage, inventory permissions, pets, save schema ou UI final do job board.  
> **Repo lock scope:** `Assets/_Game/Scripts/Companions/**`, `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/Storage/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Companions/**`, `docs/validation/05_spec_companion_farm_job_board_automation_runtime_execution_report.md`  
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
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/specs/a_implementar/05_spec_companion_eligibility_bond_availability_save_runtime.md`
  - `docs/specs/a_implementar/05_spec_farm_animals_housing_feeding_care_runtime.md`
  - `docs/specs/a_implementar/05_spec_farm_buildings_construction_workshops_storage_runtime.md`
> **Blocks:**  
  - controlled farm automation;
  - companion job UI;
  - animal caretaker automation;
  - workshop/storage automation;
  - companion bond progression.
> **Scope:** definir/endurecer job board e automação controlada de companions na fazenda, com área, horários, recursos autorizados, destino de output e limites diários.  
> **Out of scope:** pets, cave companion AI/combat, party system, farm defense/invasions, full UI/prefabs, romance/casamento, companion equipment completo.

---

# /speckit.specify

## 1. Contexto

Farm Direction lista jobs: Plantador, Regador, Colhedor, Lenhador, Pescador, Tratador, Artesão, Organizador e Minerador. O Companion Direction define que farm jobs devem ser explícitos e conter JobId, CompanionId, JobType, AllowedArea, AllowedToolOrStation, StartTime, EndTime, StaminaBudget, OutputRules, FailureRules, RelationshipGain e FatigueGain.

Automação deve exigir configuração: quem faz, onde faz, quando faz, quais recursos pode gastar, onde deposita output, prioridade e limite diário.

---

## 2. Problema

Sem contrato de companion jobs:

```text
companion pode farmar tudo sozinho;
job pode gastar Fruto Mana/Água Viva/fertilizante raro sem autorização;
job pode vender item raro automaticamente;
output pode aparecer do nada;
companion pode coletar fora de área marcada;
job pode ignorar stamina/fatigue/ferramenta/estação;
storage access pode ser amplo demais;
automation pode substituir exploração da caverna.
```

---

## 3. Objetivo

Criar/endurecer:

```text
CompanionFarmJobDefinition;
CompanionFarmJobAssignment;
JobBoardState;
AllowedArea;
AllowedResources;
OutputRules;
DailyLimit;
Priority;
StaminaBudget/FatigueGain;
Storage permissions;
Execution result;
Failure rules;
Save/load state;
No pets.
```

---

## 4. Regras de design

```text
Companion de fazenda reduz repetição, não remove planejamento.
Jobs exigem área marcada, ferramenta/estação e limites de tempo/stamina.
No early game, companion faz pouco e ensina o sistema.
No mid/late, companion melhora automação controlada.
Companion não cria item do nada nem coleta recurso fora do estado real do mundo.
Automação não deve gastar Fruto Mana, Água Viva, fertilizante raro ou vender item raro automaticamente.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero configurar companion para job limitado e previsível.
Como companion, quero respeitar disponibilidade, stamina, ferramenta e área.
Como storage/inventory, quero controlar quais itens podem ser usados/depositados.
Como economia, quero impedir automação infinita.
Como design, quero companion como ajuda, não substituto do jogador.
```

---

## 6. Escopo

Inclui:

```text
job board state;
farm job assignment;
allowed area/resource/storage policies;
job types as contracts;
daily limit and priority;
execution result;
failure rules;
relationship/fatigue hooks;
save/load;
tests/validators.
```

Não inclui:

```text
visual UI job board final;
pathfinding/animation;
cave companion AI;
pet automation;
farm defense;
romance/casamento;
full companion equipment.
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
- docs/design/gameplay/companions/COMPANIONS_DIRECTION.md
- docs/design/gameplay/pets/PETS_DIRECTION.md
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

- Farm jobs list includes Plantador, Regador, Colhedor, Lenhador, Pescador, Tratador, Artesão, Organizador e Minerador.
- Automação deve exigir quem faz, onde faz, quando faz, recursos permitidos, destino do output, prioridade e limite diário.
- Companion farm jobs devem conter JobId, CompanionId, JobType, AllowedArea, AllowedToolOrStation, StartTime, EndTime, StaminaBudget, OutputRules, FailureRules, RelationshipGain e FatigueGain.
- Companion de fazenda deve reduzir repetição, não remover planejamento.
- Companion não deve criar item do nada nem coletar recurso fora do estado real do mundo.
- Pets estão deferidos e não podem ser integrados a companion jobs agora.

### Deferred / future from directions

- UI final do Job Board.
- Pathfinding/animation.
- Cave companion combat AI.
- Pet automation.
- Farm defense/invasions.
- Party with multiple companions.
- Full companion equipment.

### Explicitly not redefined here

- Companion eligibility/bond base.
- Crop/watering/harvest runtime.
- Animal care/product runtime.
- Storage/inventory backend.
- Processing/workshop backend.
- Save schema migration.

## 7. Modelo de domínio

### 7.1 CompanionFarmJobType

```text
Planter
Waterer
Harvester
Woodcutter
Fisher
AnimalCaretaker
Artisan
Organizer
Miner
BuilderFuture
AlchemistFuture
```

### 7.2 CompanionFarmJobAssignment

```text
JobAssignmentId
CompanionId
JobType
AllowedAreaId
AllowedToolOrStationId optional
StartTime
EndTime
StaminaBudget
DailyActionLimit
Priority
AllowedResourcePolicy
OutputDestinationPolicy
FailureRules
RelationshipGainPolicy
FatigueGain
IsEnabled
```

### 7.3 AllowedResourcePolicy

```text
CanUseSeeds
AllowedSeedItemIds[]
CanUseWater
CanUseFertilizer
AllowedFertilizerIds[]
CanUseRareItems
CanUseLivingWater
CanUseMana
CanSellItems
AllowedSellCategories[]
```

Defaults:

```text
CanUseRareItems = false
CanUseLivingWater = false
CanUseMana = false
CanSellItems = false unless explicit low-risk rule
```

### 7.4 OutputDestinationPolicy

```text
DestinationType: Inventory | Chest | StorageCategory | SellPoint | Processor | LeaveOnGround | ReportOnly
DestinationId optional
OverflowBehavior: Stop | Skip | LeaveOutput | FailJob
```

### 7.5 JobExecutionResult

```text
Success
Partial
Failed
FailureReason
ActionsPerformed
ItemsConsumed[]
ItemsProduced[]
WorldStateChanges[]
FatigueGained
RelationshipDelta
Events[]
```

---

## 8. Job-specific guardrails

```text
Planter:
  - plants only allowed seeds in marked area;
  - cannot spend rare seeds unless authorized.

Waterer:
  - waters allowed plots;
  - cannot use Água Viva.

Harvester:
  - harvests ready crops only;
  - deposits output by policy.

Woodcutter:
  - cuts marked/common trees only;
  - cannot cut lore/Mana tree.

Fisher:
  - uses allowed fishing spot;
  - respects daily limit.

AnimalCaretaker:
  - feeds/collects only if animal systems exist;
  - cannot handle pets.

Artisan:
  - operates built station with allowed inputs;
  - cannot process rare/Mana/LivingWater without authorization.

Organizer:
  - moves items to allowed storage;
  - cannot sell/drop rare items.

Miner:
  - restricted to farm light nodes or final quarry future;
  - cannot replace cave mining.
```

---

## 9. Job board rules

```text
Job board not complete at start.
New job types open with relationship, building, tool upgrades or progression.
Job board is configuration, not magical automation.
Jobs should fail safely if prerequisites disappear.
Jobs must not execute while companion unavailable/injured.
```

---

## 10. Save/load

Must preserve:

```text
JobAssignmentId
CompanionId
JobType
AllowedAreaId
Schedule
Budgets/limits
Resource permissions
Output destination
Enabled/disabled state
LastExecutedDay optional
```

Must not persist:

```text
companion GameObject reference;
area collider reference as only identifier;
UI selected row;
pet state.
```

If companion job save section is absent and required, STOP for save/migration spec.

---

## 11. Criteria

```text
Job assignment requires eligible/available companion.
Job has explicit area/time/resource/output/limit.
Job cannot spend Mana/Água Viva/rare fertilizer without authorization.
Job cannot create items without consuming real world/inventory state.
Job respects storage/output policy.
Job state persists.
Pets are not integrated.
Tests cover assignment, permission denial, output routing, daily limit and save/load.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Companions/FarmJobs/CompanionFarmJobType.cs
Assets/_Game/Scripts/Companions/FarmJobs/CompanionFarmJobAssignment.cs
Assets/_Game/Scripts/Companions/FarmJobs/CompanionJobBoardState.cs
Assets/_Game/Scripts/Companions/FarmJobs/AllowedResourcePolicy.cs
Assets/_Game/Scripts/Companions/FarmJobs/OutputDestinationPolicy.cs
Assets/_Game/Scripts/Companions/FarmJobs/CompanionFarmJobExecutor.cs
Assets/_Game/Tests/EditMode/Companions/CompanionFarmJobBoardTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Companions/**
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Storage/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Companions/**
docs/validation/05_spec_companion_farm_job_board_automation_runtime_execution_report.md
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
1. Auditar companion/farm/job/storage systems.
2. Consolidar job type/assignment/board contracts.
3. Implementar resource/output permission policies.
4. Implementar safe executor or validator only if dependencies exist.
5. Criar tests.
6. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - companion eligibility base;
  - crop/watering/harvest;
  - animal care/product;
  - storage/inventory;
  - processing/workshops;
  - pets.
- Reason: farm jobs cross many domain states.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if CompanionJobs[] exists; otherwise STOP.
Does this add save section? NO unless dedicated save spec approves.
Does this require migration? NO unless adding CompanionJobs[]; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. CompanionJobAssignedEvent, CompanionJobCompletedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final; exposes data for future job board UI.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for assign job -> day/run job flow.
```

---

## 20. Riscos

```text
Risco: automation plays game for player.
Mitigação: daily limits/permissions/area.

Risco: rare resource spent automatically.
Mitigação: deny by default.

Risco: pet integration.
Mitigação: forbidden paths and tests.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar companion/farm job/storage systems.
- [ ] T003 — Consolidar job contracts.
- [ ] T004 — Implementar resource/output policies.
- [ ] T005 — Implementar executor/validator safely.
- [ ] T006 — Criar tests.
- [ ] T007 — Rodar validações.
- [ ] T008 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a companion farm jobs/job board foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de companion farm jobs/job board? | Arquivos alterados e justificativa. | PARTIAL |
| Pets | A execução respeitou que pets estão deferidos e não criou runtime/UI/save de pets? | Checklist explícito no report. | BLOCKED se violar |
| Economia | A spec não cria produto/automação/loot infinito? | Anti-exploit tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/05_spec_companion_farm_job_board_automation_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "CompanionJob|JobBoard|FarmJob|AllowedArea|AllowedResource|OutputDestination|Plantador|Regador|Colhedor|Tratador|Pet" Assets/_Game/Scripts docs/design docs/specs
rg -n "Animal|Livestock|Cow|Chicken|Sheep|Egg|Milk|Wool|Feed|Pasture|Coop|Barn|Companion|JobBoard|FarmJob|Pet|Save|Inventory|Product" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a companion farm jobs/job board existe ou foi criado de forma mínima
When o jogador executa o fluxo principal desta spec
Then o comportamento segue o direction canônico
And save/load, economia, inventário e rotina diária permanecem consistentes
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

### Scenario 3 — Pets remain deferred

```text
Given o repo ou directions mencionam pets
When a execução encontra referências de pet
Then ela não cria runtime, UI, save/load, data assets ou validação Unity de pets
And registra que pets permanecem deferidos até decisão explícita futura.
```

### Scenario 4 — Economy/save invalid state

```text
Given produto, job, animal, companion, feed, output ou save payload inválido
When validator/teste é executado
Then a violação é reportada com motivo claro
And o runtime não duplica item/ouro, não cria output do nada e não corrompe save.
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

- Companion job cria item do nada.
- Companion gasta Mana/Água Viva/fertilizante raro.
- Companion vende item raro automaticamente.
- Job executa fora da área marcada.
- Job ignora stamina/fatigue/horário.
- Output perde item por storage cheio.
- Pet integrado como worker.
- Save schema alterado sem migration.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Companion Farm Job Board Automation Runtime

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

## Pets deferred compliance
- Did this spec create pet runtime/UI/save/data assets? NO
- Pet references found:
- Pet-related work deferred:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Pets deferred:
- Economy/save invalid state:
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
4. A implementação exigir reescrever Farm/Companion/Inventory/Save system canônico existente.
5. A implementação criar pets runtime, pets UI, pets save/load ou pets data assets.
6. A implementação criar romance/casamento profundo, companion equipment completo, party com múltiplos companions ou farm defense.
7. A implementação criar invasão/defesa/inimigos/dano a crops na fazenda, proibido no roadmap atual.
8. A implementação permitir item/ouro/produto/job output infinito.
9. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Companion Farm Job Matrix

| Job | Requires | Must not |
|---|---|---|
| Plantador | seeds + area + permission | spend rare seed by default |
| Regador | water source/tool + area | use Água Viva |
| Colhedor | ready crops + output dest | harvest unready/dead |
| Lenhador | marked trees + axe/tool | cut lore/Mana trees |
| Pescador | lake spot + limit | infinite fish |
| Tratador | animal system + feed | care for pets now |
| Artesão | built station + inputs | process rare items without permission |
| Organizador | storage permissions | sell/drop rare items |
| Minerador | farm light node/quarry future | replace cave mining |

## 23H. Automation Permission Defaults

```text
Rare items: DENY.
Fruto Mana: DENY.
Água Viva: DENY.
Rare fertilizer: DENY.
Sell items: DENY unless explicit low-risk category.
Unlock progression: DENY.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Animal|Livestock|Cow|Chicken|Sheep|Egg|Milk|Wool|Feed|Pasture|Coop|Barn|Companion|JobBoard|FarmJob|Pet|Save|Inventory|Product" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário visual/gameplay de animal, produto, job board, companion job ou rotina, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, job permissions/daily limits/output routing logic is deterministic.
- Requires EditMode tests: YES for assignment/permission denial/output/daily limit/save tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for assign companion job and observe execution flow.
- Requires regression test: YES if fixing existing companion automation/resource exploit bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver FarmScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no pets; no automation exploit.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_companion_farm_job_board_automation_runtime_execution_report.md.
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
Não implementar pets runtime/UI/save/data assets.
Não criar companion como segundo player.
Não criar invasão/defesa/inimigos na fazenda.
Não criar item/ouro/produto/job output infinito.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
