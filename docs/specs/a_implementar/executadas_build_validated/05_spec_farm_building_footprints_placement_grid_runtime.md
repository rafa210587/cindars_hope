# SPEC — Farm Building Footprints Placement Grid Runtime

> **Spec ID:** `05_spec_farm_building_footprints_placement_grid_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Layout / Buildings Foundation  
> **Priority:** P0  
> **Type:** Runtime / Farm Buildings / Placement Grid / Footprints  
> **Domain:** Farm / Buildings / Footprint / Placement Validation / Construction Mode  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_LAYOUT_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere building backend, farm layout anchors, expansion zones, storage/buildings save schema, inventory/crafting material consumption, scene/prefab building assets or construction UI.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/Buildings/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/05_spec_farm_building_footprints_placement_grid_runtime_execution_report.md`  
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
  - `docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md`
  - `docs/specs/a_implementar/05_spec_farm_level1_layout_fixed_anchors_runtime.md`
> **Blocks:**  
  - farm layout anchors;
  - farm expansion zones;
  - storage/building systems;
  - construction mode UI;
  - save/load building positions.
> **Scope:** definir/endurecer footprints de construções e regras determinísticas de placement grid, sem implementar UI final de modo construção.  
> **Out of scope:** building placement UI final, scene/prefab building art, save schema migration, construction timers/economy, companion/pet/animal runtime, all building interiors.

---

# /speckit.specify

## 1. Contexto

O direction de farm layout define que toda construção posicionável deve declarar BuildingId, DisplayName, FootprintTiles, FootprintPx, EntranceTiles, RequiredClearance, CanMove, CanRotate, BlocksPath, ValidTerrainTags e RequiredFarmLevel.

Também define estados de validação de placement: ValidPlacement, BlockedByObject, BlockedByTerrain, BlockedByZone, BlockedByPath, BlockedByLoreAnchor, InsufficientResources e RequiresUpgrade.

Esta spec cria a base determinística para placement. Não é a UI final do modo construção.

---

## 2. Problema

Sem footprint/placement contract:

```text
construções podem sobrepor anchors fixos;
entrada pode ficar bloqueada;
building pode ser colocado em zona travada;
UI pode aceitar posição que backend recusa;
footprint visual e colisão podem divergir sem controle;
save/load pode perder posição/rotação;
Fonte/raiz/pedreira podem ser tratadas como building comum.
```

---

## 3. Objetivo

Criar/endurecer contrato de building placement:

```text
BuildingDefinition metadata;
FootprintTiles and FootprintPx;
EntranceTiles and clearance;
terrain tags;
zone unlock requirement;
lore anchor blocking;
path blocking rules;
CanMove/CanRotate;
placement validation result;
testable placement validator.
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
- docs/design/gameplay/equipment/EQUIPMENT_WEAPONS_ARMOR_MATERIALS_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Toda construção posicionável deve declarar BuildingId, DisplayName, FootprintTiles, FootprintPx, EntranceTiles, RequiredClearance, CanMove, CanRotate, BlocksPath, ValidTerrainTags e RequiredFarmLevel.
- Footprint visual e footprint de colisão podem ser diferentes.
- Área de entrada precisa ficar livre.
- Placement states: ValidPlacement, BlockedByObject, BlockedByTerrain, BlockedByZone, BlockedByPath, BlockedByLoreAnchor, InsufficientResources, RequiresUpgrade.
- Construções principais têm footprints definidos, como farm_house 10x8, sellpoint 2x2, storage chest 2x1, workshop 6x5/10x8, coop 8x6, barn 10x8, greenhouse 10x8/16x12.
- Fonte de Anya é 6x6, fixa, não rotaciona, não duplica.
- Raiz Dormente de Mana e Pedreira final são fixas/endgame.

### Deferred / future from directions

- UI final do modo construção.
- Construção com tempo/obra.
- Economy/material costs final.
- Building interiors.
- Animal/pet/companion runtime.
- Scene/prefab art final.

### Explicitly not redefined here

- Inventory/crafting material consumption.
- Save schema for buildings.
- Farm expansion unlock runtime.
- Pathfinding/navmesh final.
- UI confirmation patterns.

## 4. Estado atual do repo

```text
Farm buildings/storage/crafting podem existir parcialmente.
Building placement livre pode não existir.
Esta spec deve priorizar metadata/validator antes de runtime amplo.
```

A confirmar localmente:

```text
BuildingDefinition;
FarmBuildingManager;
PlacementValidator;
FarmTileZone;
Storage/building save data;
Construction UI;
building prefabs/assets.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero colocar construção apenas em tile válido.
Como jogador, quero saber por que placement está bloqueado.
Como dev, quero validator determinístico para footprint/entrance/zone.
Como save/load, quero building position/rotation serializável quando esse sistema existir.
```

---

## 6. Escopo

```text
building metadata contract;
placement validator;
footprint/entrance/clearance rules;
blocked state enum/result;
lore anchor blocking;
tests/validators;
execution report.
```

---

## 7. Fora de escopo

```text
construction UI final;
cost/timer final;
save schema migration;
interiors;
building art/prefabs;
full pathfinding.
```

---

## 8. Regras de não duplicação

```text
Não criar building system paralelo se existir.
Não implementar pets/companions/animals.
Não mover fixed lore buildings.
Não persistir Unity references.
Não alterar scene/prefab assets.
```

---

## 9. Critérios de aceite

- Building metadata contract exists or documented.
- Placement validation covers all states.
- Lore anchors block placement.
- Entrance/clearance validated.
- Footprint tiles/pixels consistent.
- Tests cover valid/invalid placement.
- Report documents save/schema decision.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Buildings/FarmBuildingDefinition.cs
Assets/_Game/Scripts/Farm/Buildings/FarmBuildingPlacementValidator.cs
Assets/_Game/Scripts/Farm/Buildings/FarmPlacementValidationResult.cs
Assets/_Game/Scripts/Farm/Buildings/FarmBuildingFootprint.cs
Assets/_Game/Tests/EditMode/Farm/FarmBuildingPlacementValidatorTests.cs
```

Consolidar existentes se houver.

---

## 11. Contratos

### Data

```text
BuildingId stable string/id.
Position stored as tile coordinate only if building persistence already exists.
No Unity references in save.
```

### Runtime

```text
Validator is deterministic and scene-independent where possible.
```

### UI

```text
UI consumes validation result; does not decide placement alone.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Buildings/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/05_spec_farm_building_footprints_placement_grid_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Crafting/**
Assets/_Game/Scripts/Save/**
Assets/_Game/Scenes/**
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

## 14. Estratégia

```text
1. Auditar building/placement systems.
2. Create or harden metadata contract.
3. Create deterministic validator.
4. Add tests for placement states.
5. Report.
```

---

## 15. Ordem segura

```text
Scale -> Level1 anchors -> Building footprints -> Expansion zones.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with expansion zones, FarmScene layout, storage/building save, construction UI or economy material cost specs.
- Reason: placement validator touches shared building/farm layout contract.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO.
Does this add save section? NO.
Does this require migration? NO unless creating building persistence; then STOP.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: NO by default.
Changes events: NO.
Requires unsubscribe pattern: NO.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO final; only validation result contracts.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for placement preview/scene verification if wired later.
```

---

## 20. Riscos

```text
Risco: metadata divergir de prefabs.
Mitigação: validator/report and no prefab edits.

Risco: save schema creep.
Mitigação: STOP if persistence required.
```

---

## 21. Rollback

```text
Remover building metadata/validator/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar building/placement systems.
- [ ] T003 — Criar/harden metadata contract.
- [ ] T004 — Implementar deterministic validator.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Tilemaps, colliders, farm scene e anchors existentes foram auditados? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escala visual | A spec respeita 32x32 tile, player 32x48 e footbox? | Validação/constantes/checklist no report. | PARTIAL |
| Anchors fixos | Fonte/lago/caverna/cidade/bordas fixas são preservadas? | Matriz de fixed/free tiles. | BLOCKED se mover indevidamente |
| Save/load | A spec altera estado persistido ou só prepara layout/metadata? | Declaração de schema/no schema. | PARTIAL |
| UI/PlayMode | A mudança exige inspeção visual/cena? | Cenário final deferido. | BUILD_VALIDATED no máximo |
| Testes | Há lógica determinística de validação/placement? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report criado? | `docs/validation/05_spec_farm_building_footprints_placement_grid_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar:

```bash
rg -n "BuildingId|FootprintTiles|FootprintPx|EntranceTiles|RequiredClearance|CanMove|CanRotate|BlocksPath|ValidTerrainTags|RequiredFarmLevel|PlacementValidator" Assets/_Game/Scripts Assets/_Game docs/design docs/specs
rg -n "Farm|FarmScene|Tilemap|Grid|Footbox|Collider|Sorting|Anchor|Building|Footprint|Placement|Expansion|Zone" Assets docs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" docs/specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
```

Classificar achados como:

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
Given a fazenda possui base de farm building footprint/placement
When o jogador entra na FarmScene ou interage com o sistema correspondente
Then o comportamento respeita a direção canônica
And nenhuma âncora fixa é movida indevidamente
And nenhuma feature futura é implementada fora de escopo
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

### Scenario 3 — Invalid state / invalid placement / invalid anchor

```text
Given um tile, anchor, building ou zone viola regra canônica
When validator/teste é executado
Then a violação é reportada com motivo claro
And a execução não tenta corrigir scene/prefab fora do escopo sem registrar risco.
```

### Scenario 4 — Save/load safety

```text
Given a mudança envolve posição, anchor, zone ou building metadata
When save/load for relevante
Then nenhum UnityEngine.Object é persistido
And IDs/positions serializáveis são usados apenas quando já houver contrato
And schema change exige spec/migration separada.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de cena/tilemap
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Footprint visual e colisão divergindo sem registro.
- EntranceTiles bloqueados.
- Building posicionado em zona travada.
- Fonte/Raiz/Pedreira tratadas como movable.
- Placement accepted by UI but rejected by backend.
- Save schema criado sem migration spec.
- Building overlaps fixed anchor.
- InsufficientResources handled inside UI only.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Building Footprints Placement Grid Runtime

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

```text
1. A implementação exigir Packages/ ou ProjectSettings/.
2. A implementação exigir scene/prefab/tilemap wiring massivo fora do escopo.
3. A implementação exigir mudança de save schema sem migration spec.
4. A implementação exigir reescrever farm/crop/inventory/building system canônico existente.
5. A implementação mover Fonte de Anya, lago, caverna, saída para cidade, pedreira final ou Raiz de Mana indevidamente.
6. A implementação criar invasão/defesa/monstro em fazenda, proibido nesta fase.
7. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Placement Validation States

| State | Meaning |
|---|---|
| ValidPlacement | All checks pass |
| BlockedByObject | Occupied by object/building/resource |
| BlockedByTerrain | Invalid terrain tag |
| BlockedByZone | Zone locked/not unlocked |
| BlockedByPath | Entrance/path/access blocked |
| BlockedByLoreAnchor | Overlaps Fonte/lake/cave/city/lore fixed anchor |
| InsufficientResources | Cost/material missing if costs integrated |
| RequiresUpgrade | Farm level/building unlock missing |

## 23H. Core Building Footprints

| BuildingId | Tiles | Move | Notes |
|---|---:|---:|---|
| farm_house | 10x8 | later | sleep/save/cook/storage |
| farm_sellpoint | 2x2 | yes | preserve pending content |
| farm_storage_chest | 2x1 | yes | simple storage |
| farm_storage_shed | 8x6 | yes | category storage |
| farm_workshop_small | 6x5 | yes | initial processors |
| farm_workshop_large | 10x8 | yes | advanced |
| farm_chicken_coop | 8x6 | yes | future animal runtime |
| farm_barn | 10x8 | yes | future animal runtime |
| farm_greenhouse_small | 10x8 | yes | off-season future |
| farm_greenhouse_large | 16x12 | yes | advanced |
| farm_fountain_anya | 6x6 | no | unique, fixed |
| farm_mana_root | 5x5/event | no | endgame fixed |
| farm_quarry_final | 16x12 | no | level 5 fixed |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Farm|FarmScene|Tilemap|Grid|Footbox|Collider|Sorting|Anchor|Building|Footprint|Placement|Expansion|Zone" Assets/_Game/Scripts Assets docs/design docs/specs
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
Quando houver cenário visual de FarmScene/tilemap/building placement, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES if placement validator/metadata logic changes.
- Requires EditMode tests: YES for deterministic placement validation tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED if building placement is visually wired later.
- Requires regression test: YES if fixing known placement overlap bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cena/tilemap/building visual; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode placement tests PASS or NOT RUN justified; no save schema change; no scene/prefab edits.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_farm_building_footprints_placement_grid_runtime_execution_report.md.
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
Não mover anchors fixos sem regra explícita.
Não implementar invasão/defesa/inimigos na fazenda.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
