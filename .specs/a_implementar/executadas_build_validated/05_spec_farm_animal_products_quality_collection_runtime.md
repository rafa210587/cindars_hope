# SPEC — Farm Animal Products Quality Collection Runtime

> **Spec ID:** `05_spec_farm_animal_products_quality_collection_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Animals / Companions  
> **Priority:** P1  
> **Type:** Runtime / Farm / Animal Products / Quality / Collection  
> **Domain:** Farm / Milk / Egg / Wool / Fertilizer Base / Product Quality  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_ANIMALS_COMPANIONS_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere animal care daily processor, inventory item quality schema, processing recipes, economy pricing, FarmOrders, companion Tratador jobs ou save schema.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/Animals/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Scripts/Items/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/05_spec_farm_animal_products_quality_collection_runtime_execution_report.md`  
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
  - `docs/design/gameplay/loot_crafting_economy/LOOT_CRAFTING_ECONOMY_DIRECTION.md`
  - `docs/design/gameplay/loot_crafting_economy/ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md`
  - `.specs/a_implementar/05_spec_farm_animals_housing_feeding_care_runtime.md`
  - `.specs/a_implementar/05_spec_farm_harvest_processing_quality_runtime.md`
  - `.specs/a_implementar/05_spec_farm_fertilizer_crop_quality_runtime.md`
> **Blocks:**  
  - processing/makers: cheese, mayonnaise, fabric;
  - farm orders requiring animal products;
  - economy pricing for animal products;
  - companion Tratador job quality bonus;
  - crafting/cooking/alchemy recipes.
> **Scope:** definir/endurecer geração, coleta e qualidade de produtos animais como leite, ovos, lã e fertilizante base.  
> **Out of scope:** animal care core, processing machines final, recipe database, animal breeding, product market balance, companion automation, UI/prefab final.

---

# /speckit.specify

## 1. Contexto

Farm Direction define animais e produtos: vaca gera leite/fertilizante base; galinha gera ovos; ovelha gera lã; animal fantástico pequeno é late game. Produtos entram em comida, queijo, alquimia, fertilizante, quests, costura, tecido e roupas.

Esta spec usa AnimalInstanceState/ProductReady da spec anterior e define collection/product quality. Não implementa companion Tratador nem processamento final.

---

## 2. Problema

Sem contrato de animal products:

```text
leite/ovo/lã podem ser coletados várias vezes por dia;
quality pode ignorar CareScore;
produto pode ser criado mesmo sem Animal ProductReady;
inventário cheio pode deletar produto;
fertilizante base pode virar Mana/arcano indevido;
produto animal pode completar FarmOrder sem adapter formal;
processing pode assumir item quality inexistente;
save/load pode duplicar produto pronto.
```

---

## 3. Objetivo

Criar/endurecer:

```text
AnimalProductDefinition;
ProductReady state consumption;
AnimalProductCollectionCommand;
AnimalProductCollectionResult;
QualityResolver from CareScore/feed/farm level;
Inventory add/overflow handling;
ProductReady reset/idempotency;
Fertilizer base as common item, not Mana;
Save/load safety.
```

---

## 4. Regras de design

```text
Animal alimentado produz.
Cuidado/afeto melhora qualidade.
Produtos têm usos claros: comida, processamento, quests, crafting, fertilizante.
Produtos processados têm valor maior, mas exigem tempo/construção/receita.
Produtos animais não devem gerar economia infinita sem limite diário/capacidade.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero coletar leite/ovo/lã quando produto estiver pronto.
Como sistema animal, quero consumir ProductReady uma vez.
Como inventory, quero preservar quality e evitar perda por inventário cheio.
Como economy, quero produtos úteis sem loop infinito.
Como FarmOrder, quero usar produto animal por adapter próprio depois.
```

---

## 6. Escopo

Inclui:

```text
animal product definitions;
collection command/result;
quality resolver;
inventory add/overflow;
product ready reset;
fertilizer base guardrail;
save/load idempotency;
tests/validators.
```

Não inclui:

```text
processing final: queijo/maionese/tecido;
recipe database;
FarmOrder adapter final;
companion Tratador automation;
animal breeding;
market price balance;
UI final.
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

- docs/design/gameplay/farm/FARM_DESIGN_DIRECTION_v1.3.md
- docs/design/gameplay/farm/FARM_LAYOUT_SCALE_BUILDINGS_DIRECTION.md
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

- Animais planejados: vaca, galinha, ovelha e animal fantástico pequeno late game.
- Produtos: leite/fertilizante base, ovos e lã.
- Animal alimentado produz; animal sem comida para de produzir; cuidado/afeto melhora qualidade.
- Produtos de qualidade são melhores para encomendas, presentes e receitas.
- Processamento aumenta valor, mas consome tempo, construção e planejamento.
- Itens devem ter função clara: vender, craft, receita, poção/fertilizante, encomenda, presente, upgrade/reparo, consumível ou progressão.

### Deferred / future from directions

- Processing machines final.
- Recipes: cheese, mayonnaise, fabric.
- Animal breeding.
- Fantasy animal products.
- Companion caretaker automation.
- Price balance final.
- UI/prefab final.

### Explicitly not redefined here

- Animal care state.
- Inventory quality schema.
- Economy price formulas.
- Crafting/processing backend.
- FarmOrder reward pipeline.
- Save schema migration.

## 7. Modelo de domínio

### 7.1 AnimalProductDefinition

```text
ProductDefinitionId
SourceAnimalSpecies
OutputItemId
BaseQuantity
ProductionCadenceDays
RequiresFedToday
RequiresProductReady
QualityEnabled
QualityInputs
CanBeProcessed
CanBeUsedAsFertilizerBase
IsLateGameReserved
```

### 7.2 AnimalProductCollectionCommand

```text
AnimalInstanceId
ActorId optional
RequestedDay
TargetInventoryId
AllowOverflowHandling
```

### 7.3 AnimalProductCollectionResult

```text
Success
FailureReason
OutputItems[]
Quality
Quantity
ProductReadyConsumed
InventoryAddResult
Events[]
```

### 7.4 Failure reasons

```text
AnimalNotFound
AnimalUnavailable
ProductNotReady
AnimalNotFed
InventoryFull
InvalidProductDefinition
DuplicateCollection
SaveStateInvalid
```

---

## 8. Quality resolver

Inputs:

```text
CareScore
FedToday
FavoriteFeedUsed optional
AnimalHealthState
BuildingQuality optional
CompanionCaretakerBonus future
Season/weather optional
RandomSeed optional
```

Rules:

```text
Quality must be deterministic or saved/seeded.
Quality should map to Normal/Boa/Excelente/Rara/Lunar only if supported.
Lunar quality is future/gated, not common product baseline.
If inventory cannot store quality, execution must mark MISSING_BUT_DEFER.
```

---

## 9. Collection and idempotency

```text
Collection succeeds only if ProductReady is true.
Successful collection consumes ProductReady once.
Reload after collection must not allow duplicate collection.
Reload before collection must preserve ProductReady.
Inventory full must not delete product silently.
```

---

## 10. Fertilizer base guardrail

```text
Cow/farm animals can provide common fertilizer base.
Common fertilizer base does not produce Mana.
Common animal products cannot become Água Viva, Fruto Mana or arcane fertilizer without future gated recipe.
```

---

## 11. Criteria

```text
Product can be collected once when ready.
Product cannot be collected when not ready.
Quality is calculated from care/feed safely.
Inventory full fails or uses overflow without item loss.
ProductReady resets after success.
Save/load does not duplicate product.
Products do not trigger FarmOrder except through adapter.
Tests cover collection, duplicate prevention, inventory full and quality.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Animals/AnimalProductDefinition.cs
Assets/_Game/Scripts/Farm/Animals/AnimalProductCollectionCommand.cs
Assets/_Game/Scripts/Farm/Animals/AnimalProductCollectionResult.cs
Assets/_Game/Scripts/Farm/Animals/AnimalProductCollectionService.cs
Assets/_Game/Scripts/Farm/Animals/AnimalProductQualityResolver.cs
Assets/_Game/Tests/EditMode/Farm/AnimalProductCollectionTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Animals/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Items/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/05_spec_farm_animal_products_quality_collection_runtime_execution_report.md
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
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 15. Estratégia

```text
1. Auditar animal/product/inventory quality systems.
2. Consolidar product definitions.
3. Implementar collection service.
4. Integrar quality resolver.
5. Integrar inventory add/overflow.
6. Criar idempotency tests.
7. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - animal care daily processor;
  - inventory item quality schema;
  - processing/crafting recipes;
  - FarmOrder adapter;
  - companion caretaker jobs.
- Reason: product collection depends on animal state and inventory/economy contracts.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if ProductReady exists; otherwise STOP.
Does this add save section? NO.
Does this require migration? NO unless adding animal product state; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. AnimalProductCollectedEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for product collection flow.
```

---

## 20. Riscos

```text
Risco: duplicate product.
Mitigação: ProductReady consumed and reload tests.

Risco: quality lost.
Mitigação: quality schema check/defer.

Risco: animal product becomes Mana/arcane resource.
Mitigação: common fertilizer guardrail.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar animal products/inventory quality.
- [ ] T003 — Consolidar product definitions.
- [ ] T004 — Implementar collection service.
- [ ] T005 — Implementar quality resolver.
- [ ] T006 — Integrar inventory add/overflow.
- [ ] T007 — Criar tests.
- [ ] T008 — Rodar validações.
- [ ] T009 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a animal products/quality/collection foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de animal products/quality/collection? | Arquivos alterados e justificativa. | PARTIAL |
| Pets | A execução respeitou que pets estão deferidos e não criou runtime/UI/save de pets? | Checklist explícito no report. | BLOCKED se violar |
| Economia | A spec não cria produto/automação/loot infinito? | Anti-exploit tests/checklist. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/05_spec_farm_animal_products_quality_collection_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "AnimalProduct|ProductReady|Milk|Egg|Wool|FertilizerBase|AnimalProductCollected|ProductQuality|CareScore" Assets/_Game/Scripts docs/design .specs
rg -n "Animal|Livestock|Cow|Chicken|Sheep|Egg|Milk|Wool|Feed|Pasture|Coop|Barn|Companion|JobBoard|FarmJob|Pet|Save|Inventory|Product" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a animal products/quality/collection existe ou foi criado de forma mínima
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

- Produto coletado duas vezes após reload.
- Inventory full apaga produto.
- Quality perdida silenciosamente.
- ProductReady não reseta.
- Produto gerado sem alimentação/cuidado válido.
- Fertilizante base vira Mana/arcano indevidamente.
- FarmOrder completada sem adapter formal.
- Pet product criado por engano.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Animal Products Quality Collection Runtime

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


## 23G. Animal Product Matrix

| Animal | Product | Uses |
|---|---|---|
| Cow | Milk | food, cheese, alchemy |
| Cow | FertilizerBase | fertilizer/crafting |
| Chicken | Egg | food, mayonnaise, quests |
| Sheep | Wool | tailoring, fabric, clothing |
| FantasySmallFuture | RareResource | late game only |

## 23H. Product Idempotency Invariant

```text
ProductReady true -> collection may succeed once.
After success -> ProductReady false or next cadence state.
Reload after success -> no duplicate.
Reload before success -> product remains ready.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Animal|Livestock|Cow|Chicken|Sheep|Egg|Milk|Wool|Feed|Pasture|Coop|Barn|Companion|JobBoard|FarmJob|Pet|Save|Inventory|Product" Assets/_Game/Scripts docs/design .specs
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

- Changed deterministic logic: YES, collection/quality/idempotency logic is deterministic.
- Requires EditMode tests: YES for product ready/not ready/inventory full/quality/reload tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for product collection gameplay flow.
- Requires regression test: YES if fixing existing duplicate product/quality bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver FarmScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no product duplication; no pet runtime.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_farm_animal_products_quality_collection_runtime_execution_report.md.
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
