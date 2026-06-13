# SPEC — UI Storage Chest Transfer Runtime

> **Spec ID:** `04_spec_ui_storage_chest_transfer_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P1  
> **Type:** Runtime / UI / Storage / Chest / Transfer  
> **Domain:** UI / Storage / Container / Inventory Transfer / Split / Sort  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere inventory backend, storage/chest data, item move/split commands, modal focus, shop sell or crafting material selection.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Inventory/**`, `Assets/_Game/Scripts/Storage/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/04_spec_ui_storage_chest_transfer_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `.specs/a_implementar/04_spec_ui_input_focus_modal_routing_runtime.md`
  - `.specs/a_implementar/04_spec_ui_inventory_items_tooltips_runtime.md`
> **Blocks:**  
  - storage/chest persistence;
  - inventory item detail;
  - crafting material source selection;
  - farm building/storage UI future;
> **Scope:** consolidar Storage/Chest UI como tela modal dividida com container inventory, player inventory, foco claro, transferência, split, sort e empty states.  
> **Out of scope:** storage backend rewrite, chest placement/building system, shared storage network, scene/prefab wiring, multiplayer/remote storage.

---

# /speckit.specify

## 1. Contexto

O direction de menu define Storage/Chest como tela modal dividida. A função é mover itens entre inventário do jogador e container, mostrando claramente qual grid está focado, dando feedback imediato de transferência, empty state e erro quando inventário cheio.

Esta spec trata da UI e dos comandos seguros de transferência. Não reescreve inventário nem persistence de storage.

---

## 2. Problema

Sem contrato de storage UI:

```text
jogador pode não saber qual grid está focado;
transferência pode mover para origem errada;
stack split pode gerar quantidades inválidas;
container vazio pode parecer bug;
inventário cheio pode falhar silenciosamente;
sort pode misturar itens protegidos indevidamente;
input de gameplay pode vazar com chest aberto.
```

---

## 3. Objetivo

Garantir Storage/Chest UI segura:

```text
dois grids com fonte clara;
foco visível;
detail drawer compartilhado;
move one/move stack/split/sort/close;
empty state explícito;
mensagem de inventory full;
ações protegidas respeitam item state;
sem alteração de schema.
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

- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Storage permite mover itens entre inventário do jogador e container.
- Layout tem container inventory, player inventory/hotbar, item detail drawer e actions move one, move stack, split, sort, close.
- Deve ficar claro qual grid está focado.
- Transferência deve ter feedback imediato.
- Container vazio deve mostrar empty state.
- Player inventory cheio deve impedir transferência com mensagem clara.
- Tela modal dividida deve bloquear gameplay input.

### Deferred / future from directions

- Storage/chest persistence avançado.
- Chest placement/building UI.
- Shared storage network.
- Auto-sort avançado.
- Favorite/lock final behavior.

### Explicitly not redefined here

- Inventory backend.
- ItemStack/ItemInstance schema.
- Save schema de containers.
- Crafting material selection.
- Farm building system.

## 4. Estado atual do repo

```text
Inventory UI e inventory backend são parciais/completos em diferentes specs.
Storage/chest pode não existir ou estar parcial.
Esta spec deve auditar antes de criar qualquer UI nova.
```

A confirmar localmente:

```text
StorageManager/ChestInventory;
InventoryManager;
MoveItem/SplitStack commands;
Container save data;
Modal/focus state;
Sort existing behavior.
```

---

## 5. User stories

```text
Como jogador, quero mover item para chest sem confundir origem/destino.
Como jogador, quero mover stack inteira ou dividir quantidade.
Como jogador, quero mensagem clara se o destino estiver cheio.
Como UI, quero foco visível entre container e inventário.
```

---

## 6. Escopo

```text
storage screen/view model;
dual-grid focus;
transfer command projection;
split quantity validation;
empty/full states;
sort action guardrails;
tests/validators.
```

---

## 7. Fora de escopo

```text
storage backend rewrite;
save schema de storage;
chest placement/building;
network/shared storage;
prefab final layout.
```

---

## 8. Regras de não duplicação

```text
Não criar InventoryManager paralelo.
Não persistir UI state como storage state.
Não mover item protegido se backend não permitir.
Não fazer sort alterar ownership/protection.
Não reescrever storage save.
```

---

## 9. Critérios de aceite

- Dois grids têm foco claro.
- Move one/move stack usam source/destination corretos.
- Split valida quantidade.
- Empty/full states claros.
- Modal bloqueia gameplay input.
- Report inclui cenário final PlayMode deferido.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Storage/StorageScreenController.cs
Assets/_Game/Scripts/UI/Storage/StorageTransferViewModel.cs
Assets/_Game/Scripts/UI/Storage/StorageGridFocusState.cs
Assets/_Game/Tests/EditMode/UI/StorageTransferViewModelTests.cs
```

Consolidar existentes se houver.

---

## 11. Contratos

### Runtime

```text
UI dispatches transfer commands.
Backend owns item movement.
Focus state owns source/destination.
```

### Save

```text
No save schema change in this spec.
```

### UI

```text
Container and player inventory are visually distinct.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_storage_chest_transfer_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Inventory/**
Assets/_Game/Scripts/Storage/**
Assets/_Game/Scripts/Items/**
```

---

## 13. Arquivos proibidos

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

## 14. Estratégia

```text
1. Auditar inventory/storage/UI existentes.
2. Consolidar view model source/destination.
3. Implementar action availability and split validation.
4. Criar tests de transfer projection.
5. Criar report com PlayMode scenarios.
```

---

## 15. Ordem segura

```text
Input focus -> Inventory item projection -> Storage transfer UI.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with inventory backend/UI, shop sell or crafting material selection.
- Reason: storage compartilha inventory projection e commands.

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
Adds events: SHOULD BE NO.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if UI subscribers touched.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: YES view model/behavior.
Scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED.
```

---

## 20. Riscos

```text
Risco: perda/duplicação de item por source/destination errado.
Mitigação: view model and command tests.

Risco: sort mudar proteções.
Mitigação: sort guardrails and backend ownership.
```

---

## 21. Rollback

```text
Reverter storage UI/view model/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar inventory/storage/UI.
- [ ] T003 — Consolidar dual-grid view model.
- [ ] T004 — Implementar action availability/split validation.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Classes/sistemas existentes foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| UI focus | A tela usa focus/modal stack canônico? | Integração com input/modal foundation. | BUILD_VALIDATED no máximo |
| Save/load | A UI altera estado persistido ou só despacha comando? | Declaração de schema/no schema. | PARTIAL |
| Eventos | Subscriptions têm lifecycle? | Mapa de publishers/subscribers se houver. | PARTIAL |
| Testes | Há view model/projection determinística? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report criado? | `docs/validation/04_spec_ui_storage_chest_transfer_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar:

```bash
rg -n "Storage|Chest|Container|Inventory|Transfer|MoveStack|SplitStack|Sort|StorageScreen|ContainerInventory" Assets/_Game/Scripts docs/design .specs
rg -n "TODO|FIXME|HACK|PARTIAL|DEFERRED|BUILD_VALIDATED|ACCEPTED" .specs docs/validation docs/IMPLEMENTATION_STATUS.md docs/project/CURRENT_STATE.md
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
Given a tela/flow de storage/chest transfer UI é aberto com dependências válidas
When o jogador navega, seleciona elemento e executa ação segura
Then a UI mostra foco claro, detalhe correto e ações disponíveis
And gameplay input fica bloqueado se modal estiver aberto
And o domínio recebe apenas comando/intent válido, não mutação escondida de UI.
```

### Scenario 2 — Empty / unavailable state

```text
Given não há dados disponíveis ou requisito está ausente
When a tela abre ou item/node/recipe/quest é selecionado
Then a UI mostra empty state ou motivo bloqueado
And nenhuma ação inválida é executada
And o report registra o estado esperado.
```

### Scenario 3 — Protected / costly action

```text
Given uma ação é custosa, destrutiva, irreversível ou protegida
When o jogador tenta executá-la
Then confirmação é exigida quando o direction manda
And foco inicial respeita regra de ação destrutiva
And cancel/back não executa ação de mundo.
```

### Scenario 4 — Final human validation deferred

```text
Given o fluxo exige PlayMode ou interação visual integrada
When a spec termina tecnicamente
Then o report registra cenário final
And não pede validação humana imediata por spec
And status respeita SPEC_VALIDATION_MATRIX_MASTER.md.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Transferir do grid errado.
- Mover stack com quantidade zero/negativa/acima do disponível.
- Destino cheio falhar silenciosamente.
- Container vazio sem empty state.
- Sort misturar itens protegidos.
- Gameplay input vazar com modal aberto.
- UI duplicar item por aplicar comando duas vezes.
- UI tentar persistir container state.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Storage Chest Transfer Runtime

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
- Empty/unavailable:
- Protected/costly action:
- Negative cases:

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
2. A implementação exigir scene/prefab/asset wiring fora do escopo.
3. A implementação exigir mudança de save schema.
4. A implementação exigir reescrever sistema canônico existente.
5. A implementação conflitar com input focus/modal foundation.
6. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Transfer State Matrix

| Source | Destination | Action | Valid if | Failure message |
|---|---|---|---|---|
| PlayerInventory | Container | MoveOne | target has space | Container cheio |
| PlayerInventory | Container | MoveStack | target has space/merge | Container cheio |
| PlayerInventory | Container | Split | amount valid | Quantidade inválida |
| Container | PlayerInventory | MoveOne | player has space | Inventário cheio |
| Container | PlayerInventory | MoveStack | player has space/merge | Inventário cheio |
| Container | PlayerInventory | Split | amount valid | Quantidade inválida |

## 23H. Focus Invariants

```text
Focused grid is visually clear.
Detail drawer follows selected item in focused grid.
Source/destination never inferred from mouse hover alone if keyboard focus differs.
Cancel closes submodal before closing storage.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Storage|Chest|Crafting|Recipe|SkillTree|SkillPoint|ActiveSlot|QuestLog|QuestDetail|Modal|Focus|Confirm" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário integrado, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES if transfer projection/action availability changes.
- Requires EditMode tests: YES for transfer/split/action projection logic.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for chest open/move/split/full inventory flow.
- Requires regression test: YES if fixing item duplication/loss bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no storage schema change; no item duplication/loss.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_storage_chest_transfer_runtime_execution_report.md.
Fonte/direction coverage preservado.
Validações obrigatórias PASS ou NOT RUN com motivo, impacto e mitigação.
Sem promoção indevida para ACCEPTED apenas por compile.
```

---

## 28. Anti-regressão

```text
Não duplicar sistemas canônicos existentes.
Não quebrar save/load.
Não usar UI como fonte de verdade.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
Não alterar scenes/prefabs/assets nesta spec.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
