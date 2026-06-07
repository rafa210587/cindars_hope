# SPEC — Farm Layout Expansion Zones Free Build Runtime

> **Spec ID:** `05_spec_farm_layout_expansion_zones_free_build_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Layout / Buildings Foundation  
> **Priority:** P1  
> **Type:** Runtime / Farm Expansion / Zones / Free Build  
> **Domain:** Farm / Expansion Levels / Zones / Unlocks / Free Build  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_LAYOUT_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere farm property level, expansion unlocks, building placement, FarmScene tilemaps, save schema for zones/buildings, economy/cost progression or story/Fonte progression gates.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/05_spec_farm_layout_expansion_zones_free_build_runtime_execution_report.md`  
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
  - `docs/specs/a_implementar/05_spec_farm_level1_layout_fixed_anchors_runtime.md`
  - `docs/specs/a_implementar/05_spec_farm_building_footprints_placement_grid_runtime.md`
> **Blocks:**  
  - farm level 1 anchors;
  - building footprint placement;
  - farm building construction/workshops;
  - farm economy and material costs;
  - Fonte/endgame zones.
> **Scope:** definir/endurecer property levels, expansion zones e regras de free build sem implementar todos os unlocks, custos e tilemap final.  
> **Out of scope:** economy/cost final, visual unlocking of tilemaps, full construction mode, save schema migration, endgame quarry/Mana/Fonte gameplay.

---

# /speckit.specify

## 1. Contexto

A fazenda cresce por níveis de propriedade: 1 Terreno Inicial, 2 Fazenda Aberta, 3 Fazenda Produtiva, 4 Fazenda Especializada e 5 Fazenda Plena. Cada nível desbloqueia área, aumenta capacidade, libera construção, cria decisão nova, tem custo em ouro+materiais e exige progresso.

A implementação pode usar uma cena grande com zonas bloqueadas ou tilemaps habilitados por expansão. O importante é separar zonas fixas, livres e bloqueadas.

---

## 2. Problema

Sem expansion/free build contract:

```text
expansão pode exigir redesenhar cena toda;
building pode ser colocado em zona não desbloqueada;
área livre pode invadir marcos de lore;
pedreira/fonte/raiz podem aparecer cedo;
farm level pode virar campo solto sem validação;
free build pode permitir bloquear acesso ao lago/caverna/cidade.
```

---

## 3. Objetivo

Criar/endurecer contrato de expansion zones:

```text
FarmPropertyLevel 1..5;
zones unlock by level/progress;
zone type fixed/free/blocked/lore/endgame;
valid terrain tags;
free build respects anchors/path/access;
size direction per level;
deferred visual scene changes;
deterministic validators.
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

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Fazenda cresce por níveis de propriedade.
- Cada nível desbloqueia nova área, aumenta capacidade, libera construção, cria decisão, custa ouro+materiais e exige progresso.
- Níveis: 1 Terreno Inicial, 2 Fazenda Aberta, 3 Fazenda Produtiva, 4 Fazenda Especializada, 5 Fazenda Plena.
- Tamanho por nível: 40x32, 56x40, 72x52, 88x64, 104x72 tiles.
- Implementação pode usar única cena grande com zonas bloqueadas ou tilemaps habilitados por expansão.
- Quase todo terreno terra/grama válido pode virar produtivo/decorativo.
- Bordas, água, entradas fixas, marcos narrativos e expansões bloqueadas ficam fixos.
- Endgame inclui pedreira final, área arcana, Raiz Dormente de Mana, upgrades da Fonte e produção avançada.

### Deferred / future from directions

- Unlock costs final.
- Visual tilemap unlock/polish.
- Full construction mode.
- Endgame Mana/Fonte/quarry gameplay.
- Companion/pet/animal automation.
- Farm invasion/defense, explicitly not current.

### Explicitly not redefined here

- Building placement validator.
- Farm save schema.
- Economy/crafting material costs.
- Quest/MainProgression gates.
- Fonte gameplay functions.

## 4. Estado atual do repo

```text
Farm MVP may exist with fixed farm scene.
Expansion may not exist or may be hardcoded.
This spec should create metadata/validator path, not redesign the scene.
```

A confirmar localmente:

```text
Farm property level fields;
Farm expansion unlocks;
zone/tile validators;
FarmScene blocked areas;
building placement zones;
save/load farm sections.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero expandir a fazenda com áreas novas e decisões reais.
Como designer, quero saber que nível libera qual tipo de zona.
Como dev, quero validar que building não entra em zona bloqueada.
Como save/load, quero property level e zones sem Unity references.
```

---

## 6. Escopo

```text
FarmPropertyLevel contract;
zone metadata;
zone unlock validation;
free build allowed tile rules;
endgame/lore zone reserved states;
tests/validators;
execution report.
```

---

## 7. Fora de escopo

```text
cost balance final;
quest unlock content;
tilemap art/scene editing;
save schema migration;
full build mode UI;
endgame gameplay.
```

---

## 8. Regras de não duplicação

```text
Não criar expansion manager paralelo se existir.
Não desbloquear endgame lore zones cedo.
Não permitir building em locked zone.
Não implementar farm invasion/defense.
Não persistir Unity references.
```

---

## 9. Critérios de aceite

- Property levels represented.
- Zone types represented.
- Unlock validation deterministic.
- Free build respects fixed anchors and paths.
- Endgame/lore zones reserved.
- No scene/prefab mass edit.
- Report includes future visual scenario.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Expansion/FarmPropertyLevel.cs
Assets/_Game/Scripts/Farm/Expansion/FarmExpansionZone.cs
Assets/_Game/Scripts/Farm/Expansion/FarmZoneType.cs
Assets/_Game/Scripts/Farm/Expansion/FarmExpansionValidator.cs
Assets/_Game/Tests/EditMode/Farm/FarmExpansionZoneTests.cs
```

Consolidar existentes se houver.

---

## 11. Contratos

### Data

```text
FarmPropertyLevel is stable enum/int 1..5.
Zones are identified by stable IDs.
No Unity references in save.
```

### Runtime

```text
Validator blocks actions in locked zones.
```

### Save

```text
No save schema change by default.
If farm level persistence absent, STOP for save spec.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/05_spec_farm_layout_expansion_zones_free_build_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/Economy/**
Assets/_Game/Scripts/Crafting/**
Assets/_Game/Scripts/Quests/**
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
1. Auditar farm expansion/property level.
2. Create/harden zone contracts.
3. Add validators for locked/free/lore zones.
4. Avoid save/schema changes.
5. Report.
```

---

## 15. Ordem segura

```text
Level1 anchors -> building placement -> expansion zones -> construction/economy unlocks.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with FarmScene layout, building placement, economy construction costs or save schema specs.
- Reason: expansion zones gate multiple systems.

---

## 17. Impacto save/load

```text
Does this change save schema? SHOULD BE NO.
Does this add save section? NO.
Does this require migration? NO unless farm level persistence is missing; then STOP.
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
Changes UI: NO final.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for visual expansion/zone verification.
```

---

## 20. Riscos

```text
Risco: expansion metadata divergir from scene.
Mitigação: validators/report.

Risco: unlock state needs save schema.
Mitigação: STOP for save spec if missing.

Risco: endgame lore leaks.
Mitigação: reserved hidden zone policy.
```

---

## 21. Rollback

```text
Remover expansion contracts/validators/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar expansion/property level.
- [ ] T003 — Criar/harden zone contracts.
- [ ] T004 — Implementar validators.
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
| Report | Execution report criado? | `docs/validation/05_spec_farm_layout_expansion_zones_free_build_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar:

```bash
rg -n "FarmPropertyLevel|ExpansionZone|FarmZone|ZoneType|FreeBuild|LockedZone|BlockedZone|LoreZone|EndgameZone|FarmLevel|BuildingPlacement" Assets/_Game/Scripts Assets/_Game docs/design docs/specs
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
Given a fazenda possui base de farm expansion zones/free build
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

- Building allowed in locked zone.
- Endgame lore zone revealed early.
- Farm level outside 1..5.
- Zone ID missing/stale.
- Free build blocks cave/city/lake access.
- Save schema created without migration.
- Expansion visual state assumed without scene evidence.
- Farm invasion/defense added despite prohibition.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Layout Expansion Zones Free Build Runtime

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


## 23G. Farm Property Level Matrix

| Level | Name | Approx size | Unlock intent |
|---:|---|---:|---|
| 1 | Terreno Inicial | 40x32 | house, field, lake, grove, Fonte, SellPoint, cave |
| 2 | Fazenda Aberta | 56x40 | more arable land, chests, pasture, workshops |
| 3 | Fazenda Produtiva | 72x52 | coop/barn, greenhouse, processors, storage |
| 4 | Fazenda Especializada | 88x64 | irrigation, workshops, companion jobs, orchard |
| 5 | Fazenda Plena | 104x72 | quarry, arcane area, Mana root, Fonte upgrades |

## 23H. Zone Type Matrix

| ZoneType | Build allowed | Notes |
|---|---:|---|
| FreeBuild | YES | valid terrain/path rules |
| InitialField | YES/LIMITED | crops/path/decor/building if valid |
| FixedAnchor | NO | Fonte/lake/cave/city/borders |
| LockedExpansion | NO until unlocked | expansion blockers |
| LoreReserved | NO or special | Fonte arcane area/Mana root |
| EndgameReserved | NO until level/story | quarry/Mana/Fonte upgrades |
| Water | NO for buildings | lake/pond unless bridge/future |
| PathAccess | LIMITED | must not block access |
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

- Changed deterministic logic: YES if expansion zone validation logic changes.
- Requires EditMode tests: YES for property level and zone validation tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for visual zone/expansion verification.
- Requires regression test: YES if fixing known expansion/placement bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cena/tilemap/building visual; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no save schema change; no endgame lore leak.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_farm_layout_expansion_zones_free_build_runtime_execution_report.md.
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
