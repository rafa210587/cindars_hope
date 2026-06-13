# SPEC — Farm Level 1 Layout Fixed Anchors Runtime

> **Spec ID:** `05_spec_farm_level1_layout_fixed_anchors_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Layout / Buildings Foundation  
> **Priority:** P0  
> **Type:** Runtime / Scene Contract / Farm Level 1 Layout / Fixed Anchors  
> **Domain:** Farm / Level 1 / Anchors / Zones / FarmScene Contract  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_LAYOUT_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere FarmScene layout, Fonte, lake, cave entrance, city exit, sellpoint, player house, initial field, farm expansion zones, tilemap assets or scene/prefab wiring.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/05_spec_farm_level1_layout_fixed_anchors_runtime_execution_report.md`  
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
  - `docs/specs/a_implementar/05_spec_farm_scale_tilemap_player_footbox_runtime.md`
> **Blocks:**  
  - farm scale/footbox;
  - building footprint placement;
  - farm expansion zones;
  - sellpoint/shipping runtime;
  - Fonte/Menu and cave entrance flows.
> **Scope:** definir/endurecer contrato do layout inicial da fazenda nível 1 com anchors fixos e zonas livres sem redesenhar toda a cena.  
> **Out of scope:** editar tilemap/scene/prefab em massa, implementar expansão visual completa, mover Fonte/lago/caverna/cidade, implementar cave/city transitions, implementar pets/companions runtime.

---

# /speckit.specify

## 1. Contexto

A fazenda nível 1 deve conter casa, campo inicial, lago, bosque, Fonte de Anya, SellPoint, entrada da caverna e saída para cidade. Ela começa maior que uma tela e deve ensinar rotina, produção e preparação para a caverna.

As âncoras fixas não devem ser movidas livremente: Fonte, lago principal, entrada da caverna, saída para cidade, bordas naturais, cliffs/rochas grandes, marcos de lore e áreas bloqueadas de expansão.

---

## 2. Problema

Sem contrato de layout nível 1:

```text
Fonte pode ser tratada como decoração móvel;
SellPoint pode bloquear entrada/saída;
lago/caverna/saída podem ficar sem anchor estável;
campo inicial pode ser pequeno ou mal posicionado;
zona livre e zona fixa ficam misturadas;
expansão futura exige redesenho da cena;
save/load pode quebrar posição de anchors.
```

---

## 3. Objetivo

Criar/endurecer layout contract:

```text
farm level 1 around 40x32 tiles;
fixed anchors declared;
free zones declared;
initial field readable;
house starts fixed, may move later;
SellPoint starts available, movable later with content preservation;
Fonte fixed and unique;
lake/cave/city exit fixed;
expansion blocked zones identified.
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

- Farm level 1 é Terreno Inicial com 40x32 tiles como direção.
- Level 1 contém casa, campo inicial, lago, bosque, Fonte, SellPoint e entrada da caverna.
- Fonte de Anya, lago principal, entrada da caverna, saída para cidade e bordas naturais são fixas.
- Tiles livres incluem grama/terra/solo arável/árvores removíveis/pedras pequenas/arbustos/caminhos/cercas/decor/canteiros.
- Casa começa em posição fixa, mas pode mover depois com modo construção/expansão.
- SellPoint pode mover depois, preservando conteúdo e sem bloquear entrada/saída.
- Fazenda deve ser maior que uma tela já no nível 1.

### Deferred / future from directions

- Tilemap final editado.
- Cave/city transition implementation.
- Expansion unlock runtime completo.
- House movement runtime.
- Pets/companions/animals layout final.
- Visual decoration final.

### Explicitly not redefined here

- Crop runtime.
- Shipping backend.
- Cave runtime.
- City scene runtime.
- Fonte gameplay functions.
- Building placement mode.

## 4. Estado atual do repo

```text
Farm loop MVP e FarmScene existem em algum grau.
Anchors podem existir como scene objects ou hardcoded positions.
Esta spec deve auditar e criar validators/metadata antes de qualquer mudança visual.
```

A confirmar localmente:

```text
FarmScene anchors;
Fonte object/id;
Lake bounds;
Cave entrance trigger;
City exit trigger;
House location;
SellPoint location and pending content behavior;
Initial field bounds;
Expansion blockers.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero entender a fazenda inicial sem perder a entrada da caverna/cidade.
Como designer, quero saber quais zonas são fixas e quais são livres.
Como dev, quero validar anchors sem mexer manualmente na cena a cada spec.
Como save/load, quero anchors estáveis e sem Unity references persistidas.
```

---

## 6. Escopo

```text
FarmLevel1LayoutContract;
anchor IDs/metadata;
fixed/free tile classification;
initial zones;
validator/report;
final visual scenario deferred.
```

---

## 7. Fora de escopo

```text
mass scene editing;
expansion runtime;
building placement runtime;
cave/city transition runtime;
pet/companion/animal runtime;
crop system rewrite.
```

---

## 8. Regras de não duplicação

```text
Não criar segunda FarmScene.
Não mover Fonte/lago/cave/city anchors sem decisão explícita.
Não tratar Fonte como altar genérico.
Não criar pet/companion areas as active runtime.
Não persistir anchor Unity references.
```

---

## 9. Critérios de aceite

- Level 1 anchors mapeados.
- Fixed/free zones declaradas.
- Fonte fixed/unique.
- Lake/cave/city exit fixed.
- SellPoint movable-later policy preserved.
- Validator/report criado ou gap justificado.
- PlayMode/final scenario deferido.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Layout/FarmLevelLayoutContract.cs
Assets/_Game/Scripts/Farm/Layout/FarmAnchorId.cs
Assets/_Game/Scripts/Farm/Layout/FarmTileZoneType.cs
Assets/_Game/Scripts/Editor/Validation/ValidateFarmLevel1Anchors.cs
Assets/_Game/Tests/EditMode/Farm/FarmLevel1LayoutContractTests.cs
```

Se já existir equivalente, consolidar.

---

## 11. Contratos

### Data

```text
Anchors are stable IDs/positions, not Unity object references in save.
```

### Runtime

```text
FarmScene may expose anchors for interaction, transition and placement validation.
```

### Save

```text
No save schema change by default.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/05_spec_farm_level1_layout_fixed_anchors_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scenes/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Cave/**
Assets/_Game/Scripts/City/**
Assets/_Game/Scripts/Shipping/**
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
1. Auditar FarmScene and anchors.
2. Criar/ajustar layout contract metadata if safe.
3. Criar validators for fixed anchors if possible.
4. Não alterar scene/prefab massivamente.
5. Criar report.
```

---

## 15. Ordem segura

```text
Farm scale/footbox -> Level1 anchors -> Building footprints -> Expansion zones.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with FarmScene, cave/city transitions, building placement or expansion specs.
- Reason: anchors are shared scene contract.

---

## 17. Impacto save/load

```text
Does this change save schema? NO.
Does this add save section? NO.
Does this require migration? NO.
Does this persist Unity references? NO.
```

---

## 18. Impacto eventos

```text
Adds events: NO.
Changes events: NO.
Requires unsubscribe pattern: NO.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: NO.
Changes scenes/prefabs/assets: NO by default.
Requires PlayMode/final human scenario: YES, DEFERRED for visual anchor verification.
```

---

## 20. Riscos

```text
Risco: contract divergir da cena real.
Mitigação: validator/report local.

Risco: anchors em scene objects sem stable IDs.
Mitigação: register by stable anchor ID.
```

---

## 21. Rollback

```text
Remover layout contract/validator/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar FarmScene anchors.
- [ ] T003 — Criar/ajustar FarmLevel1 layout contract.
- [ ] T004 — Criar validator/tests.
- [ ] T005 — Rodar validações.
- [ ] T006 — Criar report.

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
| Report | Execution report criado? | `docs/validation/05_spec_farm_level1_layout_fixed_anchors_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar:

```bash
rg -n "FarmLevel|FarmAnchor|Fonte|Fountain|Lake|CaveEntrance|CityExit|SellPoint|FarmHouse|InitialField|ExpansionZone|FixedTile|FreeTile" Assets/_Game/Scripts Assets/_Game docs/design docs/specs
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
Given a fazenda possui base de farm level 1 anchors/layout
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

- Fonte de Anya movida ou duplicada.
- Fonte tratada como altar genérico.
- Lake/cave/city exit sem anchor estável.
- SellPoint movido apagando pending content.
- House movement enabled early without unlock.
- Initial field pequeno demais ou sem leitura.
- Expansion blockers não marcados.
- Scene/prefab edit massivo realizado indevidamente.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Level 1 Layout Fixed Anchors Runtime

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


## 23G. Level 1 Anchor Matrix

| Anchor | Level | Flexibility | Notes |
|---|---:|---|---|
| FarmHouse | 1 | Fixed initially, movable after unlock | Sleep/save/cooking/storage |
| InitialField | 1 | Free within arable rules | Learn planting/watering/harvest |
| FonteAnya | 1 | Fixed | Unique physical Anya representation |
| MainLake | 1 | Fixed | Fishing/common water |
| InitialGrove | 1 | Partially free | Trees/removable resources |
| SellPoint | 1 | Movable after unlock | Preserve pending content |
| CaveEntrance | 1 | Fixed | Exploration anchor |
| CityExit | 1 | Fixed | CityScene connection |
| ExpansionBlockers | 1+ | Fixed until unlock | Cliffs/rocks/natural borders |

## 23H. Fixed vs Free Tile Rules

```text
Fixed: Fonte, lake, cave entrance, city exit, borders, cliffs, lore marks, final quarry, arcane Fonte area, Mana root, locked expansions.
Free: common grass, common dirt, arable soil, removable trees, small rocks, bushes, paths, fences, decoration, beds, valid buildings.
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

- Changed deterministic logic: YES if anchor/zone validators/contracts are created.
- Requires EditMode tests: YES for deterministic anchor/fixed-free validation when harness is available.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for FarmScene visual anchor verification.
- Requires regression test: YES if fixing known anchor/layout bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cena/tilemap/building visual; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; visual scenario documented; no scene/prefab mass edit.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_farm_level1_layout_fixed_anchors_runtime_execution_report.md.
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
