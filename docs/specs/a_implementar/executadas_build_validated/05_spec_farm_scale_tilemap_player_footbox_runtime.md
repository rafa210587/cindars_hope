# SPEC — Farm Scale Tilemap Player Footbox Runtime

> **Spec ID:** `05_spec_farm_scale_tilemap_player_footbox_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 05 — Farm Layout / Buildings Foundation  
> **Priority:** P0  
> **Type:** Runtime / Scene Contract / Tilemap Scale / Collider / Sorting  
> **Domain:** Farm / Tilemap / Player Scale / Footbox / Camera / Sorting  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_05_FARM_LAYOUT_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere FarmScene tilemaps, player/NPC/pet colliders, camera bounds, sorting Y, city scale, movement controller, interaction hitbox ou scene/prefab assets.  
> **Repo lock scope:** `Assets/_Game/Scripts/Farm/**`, `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Player/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/Farm/**`, `docs/validation/05_spec_farm_scale_tilemap_player_footbox_runtime_execution_report.md`  
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
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
> **Blocks:**  
  - farm level 1 layout;
  - city/farm asset consistency;
  - building footprint validation;
  - farm expansion zones;
  - player interaction and sorting consistency.
> **Scope:** definir/endurecer o contrato de escala visual da fazenda: tile 32x32, player/NPC 32x48, footbox inferior, sorting Y e câmera base.  
> **Out of scope:** redesenhar FarmScene, alterar sprites/prefabs, alterar ProjectSettings, criar city scale spec, implementar pets/companions/animals runtime.

---

# /speckit.specify

## 1. Contexto

A fazenda deve usar a mesma régua visual da cidade. O direction fixa tile base 32x32 px, player visual 32x48 px, NPC comum visual 32x48 px e collider footbox inferior em vez de sprite inteiro.

Isso é fundação para farm layout, buildings, footprints, camera, sorting, interação e reutilização de assets entre cidade e fazenda.

---

## 2. Problema

Sem contrato de escala:

```text
player pode parecer gigante/pequeno na fazenda;
collider pode bater com corpo inteiro e não footbox;
sorting Y pode errar atrás/frente de props;
portas/camas/casas ficam fora de proporção;
farm e city não compartilham régua visual;
building footprints ficam inconsistentes;
interação pode usar hitbox grande demais.
```

---

## 3. Objetivo

Criar ou endurecer contrato técnico mínimo:

```text
Tile base 32x32 px;
Player visual 32x48 px;
NPC comum 32x48 px;
footbox inferior para player/NPC;
pivot bottom-center;
sorting Y por pés;
interação curto à frente do player;
farm maior que uma tela já no nível 1;
câmera 20x12 a 24x14 tiles como referência.
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

- Tile base 32x32 px.
- Player visual e NPC comum 32x48 px.
- Collider recomendado como footbox inferior, não sprite inteiro.
- Pivot visual bottom-center e sorting Y baseado nos pés.
- Interação como caixa/cone curto à frente do player.
- Câmera padrão 20x12 a 24x14 tiles.
- Fazenda nível 1 maior que uma tela.
- Cidade e fazenda compartilham assets, colisão e sorting.

### Deferred / future from directions

- Redesenho final da cena.
- Sprites finais de pets/companions/animals.
- Game feel final de câmera.
- Configuração final de todos os prefabs.
- City scale hardening se necessário.

### Explicitly not redefined here

- Movement controller core.
- Combat hitboxes.
- Pet/companion/animal runtime.
- Farm layout anchors.
- Building placement runtime.

## 4. Estado atual do repo

```text
FarmScene existe em algum grau pelo MVP farm loop.
Tilemap/colliders podem estar funcionais, mas precisam auditoria de escala.
Esta spec é hardening/contract antes de layout/building expansion.
```

A confirmar localmente:

```text
FarmScene tile grid size;
player prefab visual size/collider;
NPC/pet/animal placeholder colliders;
sorting scripts;
camera bounds/follow;
interaction hitbox.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quero que fazenda e cidade tenham proporção consistente.
Como dev, quero uma régua única para tilemap, building footprint e colliders.
Como designer, quero saber quanto cabe na tela e no farm level 1.
Como QA, quero detectar se o player está colidindo pelo sprite inteiro.
```

---

## 6. Escopo

```text
scale constants/validation;
footbox contract;
sorting Y contract;
camera reference contract;
interaction hitbox contract;
editor validator/test when safe;
execution report.
```

---

## 7. Fora de escopo

```text
scene/prefab mass editing;
sprite import settings;
city scene changes;
building placement;
crop runtime;
pets/companions/animals runtime.
```

---

## 8. Regras de não duplicação

```text
Não criar segundo movement controller.
Não criar segundo sorting system se já existir.
Não alterar player prefab diretamente sem auditoria.
Não alterar ProjectSettings.
Não redefinir cidade.
```

---

## 9. Critérios de aceite

- Contrato de escala documentado em código/validator/report.
- Tile size 32x32 validado ou gap documentado.
- Player/NPC footbox validado ou gap documentado.
- Sorting Y por pés validado ou gap documentado.
- Camera reference registrada.
- Report inclui cenário visual final deferido.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/Farm/Validation/FarmScaleContract.cs
Assets/_Game/Scripts/Editor/Validation/ValidateFarmScaleContract.cs
Assets/_Game/Tests/EditMode/Farm/FarmScaleContractTests.cs
```

Se já existir sistema equivalente, consolidar.

---

## 11. Contratos

### Runtime

```text
Scale contract is metadata/validation first.
Movement and interaction systems consume existing player state.
```

### Save

```text
No save schema change.
```

### UI/Scene

```text
No scene/prefab wiring in this spec unless already test-only metadata.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/Farm/**
docs/validation/05_spec_farm_scale_tilemap_player_footbox_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Interaction/**
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
1. Auditar farm/city scale references.
2. Criar/ajustar contrato/validator se seguro.
3. Não alterar prefabs/scene sem spec específica.
4. Criar tests de constantes/metadata.
5. Criar report.
```

---

## 15. Ordem segura

```text
Scale/footbox -> Level1 anchors -> Building footprints -> Expansion zones.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with player movement/collider/camera/farm scene/building placement specs.
- Reason: escala é fundação compartilhada.

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
Requires PlayMode/final human scenario: YES, DEFERRED for visual/collider verification.
```

---

## 20. Riscos

```text
Risco: validator sem acesso confiável aos prefabs.
Mitigação: report gap and do not fake pass.

Risco: alterar prefab em massa.
Mitigação: forbidden without separate scene/prefab spec.
```

---

## 21. Rollback

```text
Remover contract/validator/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar FarmScene/player/camera/sorting/interactions.
- [ ] T003 — Criar/ajustar contract/validator mínimo.
- [ ] T004 — Criar tests.
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
| Report | Execution report criado? | `docs/validation/05_spec_farm_scale_tilemap_player_footbox_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar:

```bash
rg -n "FarmScene|Tilemap|Grid|CellSize|Footbox|Collider|SortingY|Pivot|InteractionBox|CameraFollow|CameraBounds" Assets/_Game/Scripts Assets/_Game docs/design docs/specs
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
Given a fazenda possui base de farm scale/tilemap/footbox
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

- Tile size diferente de 32x32 sem justificativa.
- Player collider usando corpo inteiro.
- Pivot não bottom-center causando sorting incorreto.
- Interaction hitbox longa demais ou atrás do player.
- Camera mostrando menos/mais que referência sem justificativa.
- Farm/city usando escalas divergentes.
- Validator alterando scene/prefab indevidamente.
- Pets/animals/companions sendo implementados fora do escopo.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Farm Scale Tilemap Player Footbox Runtime

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


## 23G. Canonical Scale Matrix

| Element | Visual | Collider | Notes |
|---|---:|---:|---|
| Tile | 32x32 px | N/A | Grid lógico |
| Player | 32x48 px | 20x12 a 24x16 px nos pés | referência principal |
| NPC comum | 32x48 px | 20x12 a 24x16 px | igual cidade |
| Companion comum | 32x48 px | 20x12 a 24x16 px | future/runtime separado |
| Pet gato | 24x24 px | 14x10 px | future/defer if no pet runtime |
| Pet cachorro | 32x32 px | 18x12 px | future/defer if no pet runtime |
| Animal pequeno | 32x32 px | 18x12 px | future/defer if no animal runtime |
| Animal médio | 48x40 px | 28x16 px | future/defer |
| Animal grande | 64x48 px | 36x20 px | future/defer |

## 23H. Sorting and Interaction Invariants

```text
Pivot visual bottom-center.
Sorting Y based on foot position.
Collider represents base/feet.
Body visual may overlap objects through sorting.
Interaction is a short box/cone in front of player.
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

- Changed deterministic logic: YES if validators/contracts are created.
- Requires EditMode tests: YES for deterministic scale/metadata validator tests when harness is available.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for visual/collider FarmScene verification.
- Requires regression test: YES if fixing known collider/sorting scale bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cena/tilemap/building visual; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; visual scenario documented; no scene/prefab mass edit.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/05_spec_farm_scale_tilemap_player_footbox_runtime_execution_report.md.
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
