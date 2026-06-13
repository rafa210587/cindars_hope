# SPEC — Farm Trees Wood Stumps Regrowth Runtime

> **Spec ID:** `05_spec_farm_trees_wood_stumps_regrowth_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Resources / World Activities  
> **Priority:** P1  
> **Type:** Runtime / Farm / Trees / Wood / Stumps / Regrowth  
> **Domain:** Farm / Trees / Wood / Stumps / Orchard Future / Save  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_RESOURCES_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere ResourceNode core, farm expansion zones, inventory/drop tables, axe/tool tiers, stamina/fatigue, orchard, fruit trees, Mana tree/root or scene/prefab tree assets.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/Resources/**`, `Assets/_Game/Scripts/Tools/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/05_spec_farm_trees_wood_stumps_regrowth_runtime_execution_report.md`  
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
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md`
  - `.specs/a_implementar/05_spec_farm_resource_node_refresh_runtime.md`
  - `.specs/a_implementar/05_spec_farm_level1_layout_fixed_anchors_runtime.md`
> **Blocks:**  
  - farm resource node refresh;
  - tool actions and fatigue;
  - farm buildings/workshop material costs;
  - common orchard future;
  - inventory wood/fiber resource definitions.
> **Scope:** definir/endurecer árvores comuns da fazenda, corte, drops de madeira, stump, remoção/regrowth e separação de árvores de Mana.  
> **Out of scope:** árvores frutíferas completas, pomar final, Mana Tree/Root runtime, sprites/prefabs/tilemap, pets/companions, invasão/defesa.

---

# /speckit.specify

## 1. Contexto

Roadmap 1 pede bosque com árvores. Roadmap 4 pede pomar comum e árvores frutíferas não-Mana. A direção também corrige o canon: Árvore de Mana é consciente, rara e escolhe onde crescer; Fruto Mana não é crop comum.

Esta spec é para árvores comuns e recursos de madeira/fibra. Não implementa árvore de Mana nem pomar completo.

---

## 2. Problema

Sem contrato de árvores:

```text
cortar árvore pode duplicar madeira após reload;
stump pode desaparecer sem regra;
árvores podem crescer em tile bloqueado/caminho/anchor;
madeira comum pode substituir materiais de caverna;
Mana tree pode ser tratada como crop/tree comum;
tool tier/stamina/fatigue podem ser ignorados;
regrowth pode destruir layout do jogador;
pomares comuns podem se misturar com bosque wild.
```

---

## 3. Objetivo

Criar/endurecer contrato de árvore comum:

```text
TreeDefinition;
TreeInstanceState;
TreeGrowthStage;
TreeChopAction;
WoodDropProfile;
Stump state;
Stump removal action;
Regrowth policy;
Zone/farm-level gates;
No Mana tree as common tree;
Save/load stable state.
```

---

## 4. Regras de design

```text
Bosque inicial existe para madeira/rotina.
Árvores comuns podem fornecer wood/fiber/seeds/sap conforme drop table.
Pomars comuns são future/roadmap 4 e não equivalem a Mana.
Árvore de Mana é rara/consciente/lore e não entra neste sistema comum.
Regrowth não deve punir layout livre nem bloquear acessos essenciais.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero cortar árvore comum e receber madeira sem duplicação.
Como jogador, quero remover stump quando fizer sentido.
Como fazenda, quero árvores voltando em área de bosque ou zona válida com tempo.
Como save/load, quero preservar árvore cortada/stump/regrowth.
Como canon, quero impedir Mana tree como árvore plantável comum.
```

---

## 6. Escopo

Inclui:

```text
Common tree definitions;
tree instance states;
chop/hit/depletion rules;
stump state and removal;
drop integration;
regrowth policy;
zone restrictions;
save/load safety;
tests/validators.
```

Não inclui:

```text
orchard full runtime;
fruit tree seasonal products;
Mana tree/root;
tree sprites/prefabs/tilemap edit;
advanced automation;
pet/companion interactions;
enemy damage to trees.
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
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md
- docs/design/gameplay/equipment/EQUIPMENT_MECHANICAL_BASELINES_DIRECTION.md

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

- Roadmap 1 inclui bosque com árvores.
- Roadmap 4 inclui pomar comum e árvores frutíferas não-Mana.
- Árvore de Mana é consciente, rara e escolhe onde crescer.
- Fruto Mana deixa de ser crop plantável comum.
- Farm resource nodes e world activities existentes/parciais devem ser preservados e formalizados.
- Farm common resources alimentam construção, crafting e economia segura.

### Deferred / future from directions

- Pomar completo.
- Árvores frutíferas por estação.
- Árvore/Raiz de Mana.
- Automação por companion/pet.
- Sprites/prefabs/tilemap final.
- Defesa/invasão/dano por inimigos.

### Explicitly not redefined here

- Axe/tool durability formulas.
- Inventory backend.
- Wood item BaseValue final.
- Building material costs.
- Expansion zone visual edits.

## 7. Modelo de domínio

### 7.1 TreeDefinition

```text
TreeId
TreeKind: CommonWild | CommonFruitFuture | Decorative | LoreProtected | ManaReserved
DisplayName
AllowedZones
RequiredFarmLevel
RequiredAxeTier
MaxHits
DropTableId
StumpDefinitionId optional
CanRegrow
RegrowthPolicy
BlocksPath
CanBeRemoved
IsLoreProtected
```

### 7.2 TreeInstanceState

```text
TreeInstanceId
TreeId
TilePosition
ZoneId
GrowthStage: Sapling | Young | Mature | Stump | Removed | Regrowing | Reserved
RemainingHits
LastChoppedDay optional
NextRegrowthEligibleDay optional
RandomSeed optional
```

### 7.3 Chop action

```text
Tool must satisfy RequiredAxeTier.
Each valid hit reduces RemainingHits.
Final hit grants drops and transitions to Stump or Removed.
Drop generation must be idempotent.
Stamina/Fatigue hooks are called if systems exist.
```

---

## 8. Regrowth policy

```text
Trees may regrow only in valid zones.
Regrowth cannot happen under buildings, crops, paths, fixed anchors or locked expansions.
Regrowth cannot block house/cave/city/lake access.
Regrowth can be chance-based only if seeded/saved.
Player-cleared area may stay clear if zone is player-controlled/free build.
Bosque zone may have controlled regrowth.
```

---

## 9. Stump rules

```text
Stump is not the same as tree.
Stump may block tile until removed.
Stump may require axe/tool action.
Stump removal may grant small wood/fiber or no drop.
Stump must persist through save/load.
```

---

## 10. Mana/Lore protection

```text
TreeKind ManaReserved cannot be spawned by common regrowth.
TreeKind LoreProtected cannot be chopped unless a story spec explicitly allows.
Raiz Dormente de Mana is not a stump/tree resource node.
Água Viva/Fonte do not use this tree system.
```

---

## 11. Criteria

```text
Tree can be chopped with valid tool.
Tree does not duplicate drops after reload.
Stump persists and can be removed if allowed.
Regrowth happens only in valid zones/timing.
Mana tree/root cannot be created as common resource.
Tool/stamina/fatigue hooks are respected or deferred.
Tests cover chop, depletion, stump, regrowth and protected trees.
```

---

# /speckit.plan

## 12. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Trees/FarmTreeDefinition.cs
Assets/_Game/Scripts/Farm/Trees/FarmTreeInstanceState.cs
Assets/_Game/Scripts/Farm/Trees/FarmTreeChopService.cs
Assets/_Game/Scripts/Farm/Trees/FarmTreeRegrowthProcessor.cs
Assets/_Game/Scripts/Farm/Trees/FarmTreeValidator.cs
Assets/_Game/Tests/EditMode/Farm/FarmTreeChopRegrowthTests.cs
```

Consolidar existentes se houver.

---

## 13. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Resources/**
Assets/_Game/Scripts/Tools/**
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/05_spec_farm_trees_wood_stumps_regrowth_runtime_execution_report.md
```

---

## 14. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 15. Estratégia

```text
1. Auditar tree/wood/world activity systems.
2. Consolidar TreeDefinition/Instance se necessário.
3. Implementar/harden chop/depletion/stump.
4. Integrar resource node refresh.
5. Bloquear Mana/lore tree misuse.
6. Criar tests.
7. Criar report.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with:
  - resource node refresh;
  - tool actions;
  - inventory drops;
  - farm expansion zones;
  - orchard/Mana tree future.
- Reason: tree state depends on shared resource/zone/save contracts.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO if tree/node state exists; otherwise STOP.
Does this add save section? NO unless dedicated save spec approves.
Does this require migration? NO unless adding tree state persistence; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. TreeChoppedEvent, StumpRemovedEvent, TreeRegrownEvent if event contracts allow.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for chop/stump/regrowth visual flow.
```

---

## 20. Riscos

```text
Risco: Mana tree implemented as normal tree.
Mitigação: TreeKind validation.

Risco: wood duplication.
Mitigação: depletion state/idempotency tests.

Risco: regrowth blocks layout.
Mitigação: zone/path validation.
```

---

# /speckit.tasks

## 21. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar trees/world activities.
- [ ] T003 — Consolidar tree definitions/states.
- [ ] T004 — Implementar/harden chop/stump.
- [ ] T005 — Implementar/harden regrowth policy.
- [ ] T006 — Integrar drops/inventory.
- [ ] T007 — Criar tests.
- [ ] T008 — Rodar validações.
- [ ] T009 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | World activities, nodes, save e inventory existentes foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de trees/wood/stumps/regrowth? | Arquivos alterados e justificativa. | PARTIAL |
| Resource refresh | A regeneração/respawn é determinística e não cria farm infinito sem regra? | Testes/validador/report. | PARTIAL |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/05_spec_farm_trees_wood_stumps_regrowth_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Tree|Wood|Stump|Axe|Chop|Regrowth|TreeChopped|StumpRemoved|ManaTree|Mana Root|Raiz" Assets/_Game/Scripts docs/design .specs
rg -n "ResourceNode|NodeRefresh|Tree|Stump|Rock|Stone|Forage|Fishing|Lake|WorldActivity|Respawn|Regrowth|Save|Inventory|Tool" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a trees/wood/stumps/regrowth existe ou foi criado de forma mínima
When o jogador executa a atividade principal da spec
Then o recurso é coletado/alterado de forma consistente
And inventário, ferramenta, stamina/fatigue e save/load não ficam inconsistentes
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Day transition / refresh

```text
Given o dia avança por dormir, colapso ou transição válida
When resource refresh/regrowth/spawn é processado
Then o sistema aplica apenas regras canônicas
And não cria ouro/recurso infinito cedo
And não respawna em tiles bloqueados, fixed anchors ou áreas de lore/endgame indevidas.
```

### Scenario 3 — Existing partial implementation

```text
Given já existe implementação parcial no repo
When a execução audita o sistema
Then ela escolhe HARDEN_EXISTING em vez de recriar do zero
And registra divergências do direction
And altera apenas o menor conjunto seguro de arquivos.
```

### Scenario 4 — Invalid state / invalid data

```text
Given um node, item, tool, drop table, spawn tile ou save payload inválido
When validator/teste é executado
Then a violação é reportada com motivo claro
And o runtime não duplica loot, não perde item e não corrompe save.
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

- Tree drop duplicado após reload.
- Stump removido sem persistir.
- Regrowth em tile bloqueado/anchor fixo.
- Mana tree criada como árvore comum.
- Tree blocks cave/city/lake access after regrowth.
- Tool tier ignored.
- Stamina/fatigue ignored without justification.
- Scene/prefab edit massivo fora do escopo.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Trees Wood Stumps Regrowth Runtime

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
- Day transition / refresh:
- Existing partial implementation:
- Invalid state:
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
4. A implementação exigir reescrever WorldActivity/Farm/Inventory/Save system canônico existente.
5. A implementação criar conflito com day transition, save ownership, stable IDs ou Testing Quality Gate.
6. A implementação criar pets/companions/animals runtime fora do escopo desta wave.
7. A implementação criar invasão/defesa/inimigos/dano a crops na fazenda, proibido no roadmap atual.
8. A implementação permitir recurso infinito, duplicação de drop, refresh sem cooldown ou save exploit.
9. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Tree State Matrix

| State | Action | Result |
|---|---|---|
| Sapling | Usually no chop or low drop | future/growth |
| Young | Chop if allowed | reduced drops or stump |
| Mature | Chop | full drops + stump/removed |
| Stump | Remove stump | clear tile, optional minor drop |
| Removed | None unless regrowth policy | stays clear or regrows later |
| Regrowing | Wait/day transition | becomes sapling/young |
| Reserved | None | lore/endgame protected |

## 23H. Mana Protection Invariants

```text
Common tree systems must reject ManaReserved as normal spawn.
Fruto Mana cannot be harvest item from normal tree.
Raiz Dormente de Mana cannot be stump/regrowth output.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "ResourceNode|NodeRefresh|Tree|Stump|Rock|Stone|Forage|Fishing|Lake|WorldActivity|Respawn|Regrowth|Save|Inventory|Tool" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de FarmScene, coleta, corte, mineração, foraging ou pesca, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, chop/stump/regrowth/protection logic is deterministic.
- Requires EditMode tests: YES for chop/depletion/stump/regrowth/protected tree tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for chop/stump/regrowth visual flow.
- Requires regression test: YES if fixing existing tree duplication/regrowth bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver FarmScene/UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no Mana tree misuse; no wood duplication.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_farm_trees_wood_stumps_regrowth_runtime_execution_report.md.
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
Não criar recurso infinito/duplicação de loot.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
