# SPEC — Farm Animals Housing Feeding Care Runtime

> **Spec ID:** `05_spec_farm_animals_housing_feeding_care_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Animals / Companions  
> **Priority:** P1  
> **Type:** Runtime / Farm / Animals / Housing / Feeding / Care  
> **Domain:** Farm / Animals / Cow / Chicken / Sheep / Feed / Housing / Care  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_ANIMALS_COMPANIONS_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere building construction, animal products, animal product quality, companion Tratador jobs, pets, save schema, economy product values, scene/prefab animal assets ou UI final de animals.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/Animals/**`, `Assets/_Game/Scripts/Buildings/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/05_spec_farm_animals_housing_feeding_care_runtime_execution_report.md`  
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
  - `docs/design/gameplay/pets/PETS_DIRECTION.md`
  - `docs/specs/a_implementar/05_spec_farm_buildings_construction_workshops_storage_runtime.md`
  - `docs/specs/a_implementar/05_spec_farm_building_footprints_placement_grid_runtime.md`
> **Blocks:**  
  - animal product generation;
  - animal product quality;
  - companion AnimalCaretaker jobs;
  - farm economy product values;
  - animal UI/final buildings.
> **Scope:** definir/endurecer animais produtivos básicos, moradia, alimentação diária, cuidado/afeto e disponibilidade de produção sem implementar pets.  
> **Out of scope:** pets, companion Tratador automation, animal products generation details, animal breeding, death/permadeath, prefab/animation, final UI.

---

# /speckit.specify

## 1. Contexto

O Farm Direction planeja animais produtivos: vaca, galinha, ovelha e animal fantástico pequeno late game. Regras base: alimentar diariamente, animal alimentado produz, animal sem comida para de produzir, cuidado/afeto melhora qualidade, companion Tratador pode automatizar parte da rotina, e morte deve ser simples/recuperável quando possível.

Pets são sistema separado e estão explicitamente deferidos. Esta spec cobre **farm animals produtivos**, não pets.

---

## 2. Problema

Sem contrato de animal care:

```text
animal pode produzir sem alimentação;
animal pode morrer/perder estado de forma punitiva demais;
pet pode ser misturado com farm animal;
feeding pode consumir item sem persistir estado;
housing pode não limitar capacidade;
animal pode ficar sem HomeBuildingId estável;
quality pode depender de cuidado sem dado salvo;
companion Tratador pode automatizar antes do sistema existir.
```

---

## 3. Objetivo

Criar/endurecer:

```text
AnimalDefinition;
AnimalInstanceState;
AnimalSpecies/ProductIntent;
HomeBuildingId;
Feeding state;
DaysWithoutFood;
CareScore;
HealthState;
ProductEligibility;
Daily care processing;
Save/load safe state;
Pet separation guardrail.
```

---

## 4. Regras de design

```text
Animais produtivos são economia rural e rotina de cuidado.
Animal alimentado produz.
Animal sem comida para de produzir.
Cuidado/afeto melhora qualidade.
Morte deve ser simples e recuperável quando possível.
Thandra pode aparecer como referência cultural de cuidado animal.
Pet não é farm animal.
Farm animal não é companion.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero alimentar animais e ver rotina de cuidado produzir valor.
Como sistema de buildings, quero limitar animais por casa/curral/galinheiro/pasto.
Como save/load, quero preservar estado de animal, alimentação e cuidado.
Como economy, quero animais como produção estável, não ouro infinito.
Como design, quero deixar companion Tratador para automação futura controlada.
```

---

## 6. Escopo

Inclui:

```text
animal definitions;
animal instance state;
housing/home building requirements;
feeding/care state;
daily animal care processing;
capacity validation;
health/unavailable states;
save/load contract;
tests/validators.
```

Não inclui:

```text
pets;
animal product details/quality final;
breeding/reproduction;
companion Tratador automation;
animal death/permadeath;
animal animations/prefabs/UI;
market price balance.
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
- docs/design/gameplay/pets/PETS_DIRECTION.md

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

- Animais planejados: vaca, galinha, ovelha e animal fantástico pequeno late game.
- Regras: alimentar diariamente; animal alimentado produz; animal sem comida para de produzir; cuidado/afeto melhora qualidade.
- Companion Tratador pode automatizar parte da rotina no futuro, mas isso não é escopo desta spec.
- Morte deve ser tratada com tom simples e recuperável quando possível.
- Pets estão deferidos e não entram no escopo atual de specs/código/UI/save/load.
- Farm animals são diferentes de pets e companions.

### Deferred / future from directions

- Animal products generation/quality full.
- Breeding/reproduction.
- Companion caretaker automation.
- Pets.
- Animal fantástico late game.
- Animal UI/prefabs/animations final.
- Full product economy.

### Explicitly not redefined here

- Building placement/construction.
- Inventory/feed item schema.
- Product item definitions.
- Economy price final.
- Save schema migration.
- Pet system.

## 7. Modelo de domínio

### 7.1 FarmAnimalSpecies

```text
Cow
Chicken
Sheep
FantasySmallFuture
```

### 7.2 AnimalDefinition

```text
AnimalDataId
DisplayName
Species
RequiredHomeBuildingType
RequiredFarmLevel
BaseFeedItemTags
FavoriteFeedItemIds optional
ProducesProductType
ProductionCadenceDays
RequiresFedTodayToProduce
CareAffectsQuality
MaxCareScore
IsLateGameReserved
```

### 7.3 AnimalInstanceState

```text
AnimalInstanceId
AnimalDataId
Name
HomeBuildingId
FedToday
LastFedDay optional
DaysWithoutFood
HealthState: Healthy | Hungry | Tired | SickLight | Unavailable | Recovering
CareScore
ProductReady
ProductQualityBias
LastProductDay optional
```

### 7.4 HousingCapacityState

```text
HomeBuildingId
BuildingType: Coop | Barn | Pasture | StableFuture
Capacity
AnimalInstanceIds[]
FeedStorageId optional
IsAccessible
```

---

## 8. Feeding and care rules

```text
Feeding validates allowed feed item before consuming.
If feed state cannot be persisted, do not consume item.
FedToday enables next production check.
Animal without food stops producing and may lose mood/care over time.
Animal should not die easily from lack of food in base system.
CareScore increases with feeding/petting/clean routine if actions exist.
CareScore should influence product quality later, not produce direct gold.
```

---

## 9. Daily processing

```text
At day transition:
  - reset or roll FedToday according to current day semantics;
  - increment DaysWithoutFood if not fed;
  - update HealthState;
  - mark ProductReady only if cadence and feed/care rules pass;
  - do not generate inventory item automatically unless product spec allows;
  - preserve state in save.
```

---

## 10. Pet separation

```text
AnimalDefinition must not include PetType.
PetSaveData is not created or modified.
Pet behavior, pet following, pet bond, pet HUD and pet cave actions remain deferred.
```

---

## 11. Criteria

```text
Animal has stable instance state.
Animal requires home building/capacity.
Feeding consumes valid item once and persists fed state.
Unfed animal does not produce.
CareScore exists or is safely deferred as product-quality input.
Daily processing is deterministic.
Pets are not implemented.
Tests cover feeding, invalid feed, housing capacity, day transition and save/load.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Animals/FarmAnimalSpecies.cs
Assets/_Game/Scripts/Farm/Animals/AnimalDefinition.cs
Assets/_Game/Scripts/Farm/Animals/AnimalInstanceState.cs
Assets/_Game/Scripts/Farm/Animals/AnimalHousingCapacityState.cs
Assets/_Game/Scripts/Farm/Animals/AnimalCareService.cs
Assets/_Game/Scripts/Farm/Animals/AnimalDailyProcessor.cs
Assets/_Game/Tests/EditMode/Farm/FarmAnimalCareTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Animals/**
Assets/_Game/Scripts/Buildings/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/05_spec_farm_animals_housing_feeding_care_runtime_execution_report.md
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
1. Auditar animal/farm building/feed/save systems.
2. Consolidar animal definitions/states.
3. Implementar/harden housing/capacity validation.
4. Implementar/harden feeding/care service.
5. Implementar daily processor sem gerar produto final se product spec ausente.
6. Criar tests.
7. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - animal product generation;
  - building construction;
  - companion caretaker jobs;
  - pet system;
  - save schema.
- Reason: animal state is central to products and automation.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if AnimalSaveData exists; otherwise STOP.
Does this add save section? NO unless dedicated save spec approves.
Does this require migration? NO unless adding Animals[]; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. AnimalFedEvent, AnimalCareChangedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for feeding/care/housing loop.
```

---

## 20. Riscos

```text
Risco: pet accidentally implemented.
Mitigação: forbidden path and report checklist.

Risco: animal product generated without product spec.
Mitigação: ProductReady only, product spec handles collection.

Risco: feed item loss.
Mitigação: validate/persist before consume.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar animals/buildings/feed/save.
- [ ] T003 — Consolidar animal definitions/states.
- [ ] T004 — Implementar housing capacity validation.
- [ ] T005 — Implementar feeding/care service.
- [ ] T006 — Implementar daily processor.
- [ ] T007 — Criar tests.
- [ ] T008 — Rodar validações.
- [ ] T009 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a farm animal care/housing/feeding foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de farm animal care/housing/feeding? | Arquivos alterados e justificativa. | PARTIAL |
| Pets | A execução respeitou que pets estão deferidos e não criou runtime/UI/save de pets? | Checklist explícito no report. | BLOCKED se violar |
| Economia | A spec não cria produto/automação/loot infinito? | Anti-exploit tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/05_spec_farm_animals_housing_feeding_care_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Animal|FarmAnimal|Livestock|Cow|Chicken|Sheep|Feed|FedToday|CareScore|HomeBuilding|Barn|Coop|Pasture" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a farm animal care/housing/feeding existe ou foi criado de forma mínima
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

- Animal produz sem comida.
- Feed consumido sem persistir FedToday.
- Animal sem HomeBuildingId estável.
- Capacity ignorada.
- ProductReady marcado em duplicidade após reload.
- Pet system criado por engano.
- Animal morre/perde progresso de forma punitiva sem regra.
- Save schema alterado sem migration.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Animals Housing Feeding Care Runtime

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


## 23G. Animal Care Matrix

| Species | Home | Product intent | Feed rule |
|---|---|---|---|
| Cow | Barn/Pasture | Milk/fertilizer base | daily feed |
| Chicken | Coop | Egg | daily feed |
| Sheep | Barn/Pasture | Wool | daily feed |
| FantasySmallFuture | special future | rare resource | late game only |

## 23H. Pet Separation Invariant

```text
FarmAnimalSpecies is not PetType.
AnimalSaveData is not PetSaveData.
Farm animal cannot follow player as pet.
Farm animal cannot occupy companion slot.
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

- Changed deterministic logic: YES, feeding/care/daily processing/capacity logic is deterministic.
- Requires EditMode tests: YES for feeding/invalid feed/housing/day transition/save tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for feeding and animal routine flow.
- Requires regression test: YES if fixing existing animal/feed/save bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver FarmScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no pets runtime; no animal product duplication.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_farm_animals_housing_feeding_care_runtime_execution_report.md.
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
