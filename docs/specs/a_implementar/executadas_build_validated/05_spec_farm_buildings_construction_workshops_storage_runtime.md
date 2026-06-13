# SPEC — Farm Buildings Construction Workshops Storage Runtime

> **Spec ID:** `05_spec_farm_buildings_construction_workshops_storage_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Construction / Tools / Conditions  
> **Priority:** P0  
> **Type:** Runtime / Farm / Buildings / Construction / Workshops / Storage  
> **Domain:** Farm / Construction / Workshops / Storage / Costs / Build Time  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_CONSTRUCTION_TOOLS_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere building placement grid, farm expansion zones, inventory/material consumption, economy costs, crafting/processing backend, storage save schema, scene/prefab building assets ou construction UI final.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/Buildings/**`, `Assets/_Game/Scripts/Storage/**`, `Assets/_Game/Scripts/Crafting/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/05_spec_farm_buildings_construction_workshops_storage_runtime_execution_report.md`  
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
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `docs/specs/a_implementar/05_spec_farm_building_footprints_placement_grid_runtime.md`
  - `docs/specs/a_implementar/05_spec_farm_layout_expansion_zones_free_build_runtime.md`
  - `docs/specs/a_implementar/04_spec_ui_storage_chest_transfer_runtime.md`
  - `docs/specs/a_implementar/04_spec_ui_crafting_screen_runtime.md`
> **Blocks:**  
  - construction mode UI;
  - workshop processors;
  - storage/chest runtime;
  - farm expansion/material economy;
  - city builder/NPC services.
> **Scope:** definir/endurecer construção de buildings, custos, tempo de obra, workshops e storage sem criar UI final nem editar prefabs/tilemaps.  
> **Out of scope:** UI final de modo construção, assets de edifícios, interiors, full NPC builder service, animal buildings runtime, companion jobs, pets.

---

# /speckit.specify

## 1. Contexto

A fazenda cresce por níveis de propriedade. Cada nível deve desbloquear área, capacidade produtiva, construção relevante, decisão nova, custo em ouro+materiais e exigência de progresso. O direction também exige modo construção com grid, preview de footprint, validação, rotação quando fizer sentido, confirmação, mover/demolir com recuperação parcial e bloqueios claros.

Esta spec converte isso em contrato runtime de construção, storage e workshops, usando o placement grid já especificado.

---

## 2. Problema

Sem contrato de construction/buildings:

```text
building pode ser colocado sem custo;
materiais podem ser consumidos sem save seguro;
obra pode completar sem day transition claro;
storage pode perder conteúdo ao mover/demolir;
workshop pode processar sem building/unlock;
demolir pode duplicar refund;
construction UI pode decidir regra que deveria estar no backend;
building pode bloquear entrada/caverna/lago/casa;
save/load pode perder estado de construção.
```

---

## 3. Objetivo

Criar/endurecer:

```text
FarmBuildingDefinition;
ConstructionRequest;
ConstructionJob;
ConstructionCost;
ConstructionState;
BuildTime;
Move/Demolish policy;
Storage content preservation;
Workshop unlock hooks;
Placement validator integration;
Atomic material/gold consumption;
Save/load safe state.
```

---

## 4. Regras de design

```text
Construções expandem decisão e capacidade produtiva.
Construção deve custar ouro + materiais.
Construção pode exigir progresso de cidade, caverna, fazenda ou lore.
Workshops convertem recursos, não substituem todos os loops.
Storage organiza rotina, não cria inventário infinito sem custo/progressão.
Mover SellPoint preserva conteúdo pendente.
Mover casa é ação avançada.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero construir workshop/storage pagando custo válido.
Como jogador, quero saber por que uma construção está bloqueada.
Como jogador, quero mover/demolir sem perder conteúdo crítico.
Como backend, quero transação atômica de custo, job e save.
Como crafting, quero que workshop desbloqueie processing sem bypass.
```

---

## 6. Escopo

Inclui:

```text
building construction contracts;
construction job state;
cost/material validation;
build time/day transition hooks;
move/demolish policies;
storage content preservation;
workshop unlock marker;
tests/validators.
```

Não inclui:

```text
visual construction UI final;
prefab placement;
building interiors;
animal/pet/companion buildings runtime;
NPC builder content;
full recipe/machine runtime.
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

- Fazenda cresce por níveis e libera construção/capacidade/custo/progresso.
- Construções respeitam tamanho, colisão, acesso por caminho, distância mínima, zona desbloqueada, terreno válido, ausência de obstáculos, custo pago, tempo de construção e save/load.
- Modo construção deve mostrar grid, preview, validação, rotação, confirmação, mover, demolir/remover com recuperação parcial e bloqueios claros.
- Áreas de workshops e depósitos são zonas livres; SellPoint pode mover preservando conteúdo; casa pode mover depois com requisitos.
- Gold é sink para construções, expansões, reparo, receitas, upgrades e serviços.
- Crafting/workshops devem exigir material, estação, receita e progresso.

### Deferred / future from directions

- UI final de construção.
- Scene/prefab/tilemap placement.
- Interiores.
- Animal buildings full runtime.
- Companion job board.
- NPC builder service full content.
- Balance final de custos.

### Explicitly not redefined here

- Placement validator.
- Inventory backend.
- Economy pricing final.
- Processing recipes.
- Save schema migration.
- Farm expansion visual unlocks.

## 7. Modelo de domínio

### 7.1 FarmBuildingDefinition

```text
BuildingId
DisplayName
BuildingCategory: House | SellPoint | Storage | Workshop | Animal | Greenhouse | Decorative | Lore | Endgame
RequiredFarmLevel
RequiredQuestFlag optional
RequiredCityService optional
Footprint
EntranceTiles
CanMove
CanRotate
CanDemolish
BlocksPath
ValidTerrainTags
ConstructionCost
ConstructionTimeDays
RefundPolicy
StorageProfile optional
WorkshopProfile optional
IsLoreProtected
IsEndgameReserved
```

### 7.2 ConstructionRequest

```text
BuildingId
TargetTile
Rotation
ActorId optional
RequestedDayTime
CostPreviewSnapshot
PlacementValidationResult
```

### 7.3 ConstructionJob

```text
ConstructionJobId
BuildingId
TargetTile
Rotation
State: Planned | CostReserved | UnderConstruction | Completed | Cancelled | Failed
StartedDay
CompleteOnDay
CostPaid
MaterialsReservedOrConsumed
ResultBuildingInstanceId optional
```

### 7.4 BuildingInstanceState

```text
BuildingInstanceId
BuildingId
TilePosition
Rotation
State: Active | UnderConstruction | Moving | Disabled | Demolished
StorageInstanceId optional
WorkshopInstanceId optional
CreatedDay
LastMovedDay optional
```

---

## 8. Transaction rules

```text
Validate placement before cost.
Validate cost before job creation.
If cost cannot be persisted/reserved, do not create job.
If job cannot be persisted, do not consume cost.
If job completes, create/activate building once.
Reload before completion preserves job.
Reload after completion does not create duplicate building.
```

---

## 9. Storage preservation

```text
Moving storage building must preserve StorageInstanceId and contents.
Demolishing storage building with contents must block or require explicit safe transfer.
Moving SellPoint must preserve pending shipping entries.
Moving house must preserve player spawn/sleep/save/cooking/storage hooks.
```

---

## 10. Workshop hooks

```text
Workshop building may unlock station/category.
Workshop does not own recipe database.
Workshop does not process items unless processing backend exists.
Workshop state must be visible to crafting/processing as capability, not duplicate crafting.
```

---

## 11. Criteria

```text
Construction requires valid placement and cost.
Cost cannot be paid twice or refunded twice.
Construction job persists through save/load.
Completion happens once.
Move/demolish preserves or blocks protected contents.
Workshop/storage capabilities are exposed safely.
No scene/prefab asset edits.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Buildings/FarmBuildingConstructionService.cs
Assets/_Game/Scripts/Farm/Buildings/ConstructionRequest.cs
Assets/_Game/Scripts/Farm/Buildings/ConstructionJob.cs
Assets/_Game/Scripts/Farm/Buildings/FarmBuildingInstanceState.cs
Assets/_Game/Scripts/Farm/Buildings/ConstructionCost.cs
Assets/_Game/Scripts/Farm/Buildings/BuildingMoveDemolishPolicy.cs
Assets/_Game/Tests/EditMode/Farm/FarmBuildingConstructionTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Buildings/**
Assets/_Game/Scripts/Storage/**
Assets/_Game/Scripts/Crafting/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/05_spec_farm_buildings_construction_workshops_storage_runtime_execution_report.md
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

## 15. Estratégia

```text
1. Auditar building/placement/storage/workshop/economy existentes.
2. Consolidar construction request/job/state.
3. Integrar placement validator.
4. Implementar transação de custo/job de forma segura.
5. Implementar preservation policies para storage/SellPoint/house.
6. Criar tests de atomicidade, reload, move/demolish.
7. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - building placement grid;
  - storage/chest runtime;
  - crafting/processing runtime;
  - economy costs;
  - farm expansion zones.
- Reason: construction crosses placement, inventory, economy, save and storage.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if building/job persistence exists; otherwise STOP.
Does this add save section? NO unless dedicated save spec approves.
Does this require migration? NO unless adding construction/building job state; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. BuildingConstructionStartedEvent, BuildingCompletedEvent, BuildingMovedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final; exposes backend reasons for UI.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for construct/move/demolish flow.
```

---

## 20. Riscos

```text
Risco: item/gold duplication or loss.
Mitigação: transaction tests.

Risco: storage contents lost on move/demolish.
Mitigação: preservation/blocking policy.

Risco: scene asset edits.
Mitigação: forbidden.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar construction/buildings/storage/workshops.
- [ ] T003 — Consolidar construction request/job/state.
- [ ] T004 — Integrar placement/cost validation.
- [ ] T005 — Implementar safe transaction.
- [ ] T006 — Implementar move/demolish preservation policies.
- [ ] T007 — Criar tests.
- [ ] T008 — Rodar validações.
- [ ] T009 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a farm construction/workshops/storage foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de farm construction/workshops/storage? | Arquivos alterados e justificativa. | PARTIAL |
| Economia | A spec não cria ouro infinito, bypass de custo ou upgrade grátis? | Anti-exploit tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/05_spec_farm_buildings_construction_workshops_storage_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Construction|Building|Workshop|Storage|Demolish|MoveBuilding|ConstructionJob|BuildingInstance|FarmBuilding" Assets/_Game/Scripts docs/design docs/specs
rg -n "Building|Construction|Workshop|Storage|Fertilizer|Tool|Upgrade|Fatigue|Sleep|Hunger|Stamina|PlayerCondition|Save|Economy|Inventory" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a farm construction/workshops/storage existe ou foi criado de forma mínima
When o jogador executa o fluxo principal desta spec
Then o comportamento segue o direction canônico
And save/load, economia, inventário e estado do jogador permanecem consistentes
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

### Scenario 3 — Missing dependency

```text
Given uma dependência runtime não existe no repo local
When a execução encontra essa ausência
Then ela não inventa uma solução massiva
And marca como MISSING_BUT_DEFER ou cria apenas adapter/validator mínimo se seguro
And registra risco residual.
```

### Scenario 4 — Economy/save invalid state

```text
Given custo, material, upgrade, construção, condition ou save payload inválido
When validator/teste é executado
Then a violação é reportada com motivo claro
And o runtime não duplica item/ouro, não aplica upgrade grátis e não corrompe save.
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

- Construction job criado sem custo persistido.
- Custo cobrado duas vezes após reload.
- Building completado duas vezes.
- Demolish duplica refund.
- Storage/SellPoint perde conteúdo ao mover.
- Workshop permite processing sem unlock.
- Building bloqueia caminho/anchor.
- Save schema alterado sem migration.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Buildings Construction Workshops Storage Runtime

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
- Existing partial implementation:
- Missing dependency:
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
4. A implementação exigir reescrever Farm/Economy/Inventory/PlayerCondition system canônico existente.
5. A implementação criar conflito com day transition, save ownership, stable IDs ou Testing Quality Gate.
6. A implementação criar pets/companions/animals runtime fora do escopo desta wave.
7. A implementação criar invasão/defesa/inimigos/dano a crops na fazenda, proibido no roadmap atual.
8. A implementação permitir ouro infinito, item duplication, upgrade grátis, custo negativo ou recovery exploit.
9. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Construction State Matrix

| State | Meaning | Safe transition |
|---|---|---|
| Planned | Request validated but not paid | CostReserved/Cancelled |
| CostReserved | Cost reserved/paid safely | UnderConstruction/Cancelled |
| UnderConstruction | Job active | Completed/Failed |
| Completed | Building active once | terminal |
| Cancelled | Job stopped; refund policy applies once | terminal |
| Failed | Error state requiring report | manual/report |

## 23H. Protected Building Policy

```text
Fonte/lore/endgame buildings are not movable/demolishable.
House move requires advanced unlock and spawn preservation.
SellPoint move preserves pending entries.
Storage move preserves contents.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Building|Construction|Workshop|Storage|Fertilizer|Tool|Upgrade|Fatigue|Sleep|Hunger|Stamina|PlayerCondition|Save|Economy|Inventory" Assets/_Game/Scripts docs/design docs/specs
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
Quando houver cenário visual/gameplay de construção, upgrade, fertilizante, ferramenta, sono/cansaço/fome/stamina, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, construction/cost/job/move policy is deterministic.
- Requires EditMode tests: YES for placement/cost/job/reload/move/demolish/refund tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for construction and move/demolish gameplay flow.
- Requires regression test: YES if fixing existing construction/storage loss/cost bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver FarmScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no save schema change; no content loss/duplication.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_farm_buildings_construction_workshops_storage_runtime_execution_report.md.
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
Não criar invasão/defesa/inimigos na fazenda.
Não criar item/ouro/upgrade infinito.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
