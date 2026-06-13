# SPEC — UI Input Focus Modal Routing Runtime

> **Spec ID:** `11_spec_ui_input_focus_modal_routing_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 11 — UI / UX / Input / Menus  
> **Priority:** P0  
> **Type:** Runtime / UI / Input / Focus / Modal Routing  
> **Domain:** UI / InputFocus / Modal Stack / Gameplay Block  
> **Parallelizable:** YES  
> **Parallel group:** WAVE_11_UI_UX_RUNTIME  
> **Can run with:** UI HUD projection specs that only read focus state.  
> **Must not run with:** qualquer spec que altere player movement/combat input, menu prefabs, Input System package, save schema, scene event wiring ou final UI visual assets.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Input/**`, `Assets/_Game/Scripts/Player/**`, `Assets/_Game/Scripts/Editor/Validation/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/11_spec_ui_input_focus_modal_routing_runtime_execution_report.md`  
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
  - `docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md`
  - `docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md`
  - `docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md`
  - `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
> **Blocks:**  
  - all modal UI specs;
  - dialogue/shop/crafting/quest/Fonte menus;
  - pause/system menu;
  - final human validation flow.
> **Scope:** definir/endurecer foco de UI, stack de modais, routing de input e bloqueio de gameplay action enquanto UI modal está aberta.  
> **Out of scope:** Input System package changes, UI prefabs/layout final, keybind remap screen, gamepad final, scene wiring.

---

# /speckit.specify

## 1. Contexto

O direction registra problema real: ao abrir HUD com I, dialogar ou navegar opções, WASD ainda move o personagem. Regra canônica: UI modal bloqueia input de gameplay, tem foco explícito e navegação de UI.

Esta spec cria o contrato runtime de foco/input. É fundação de todos os menus.

---

## 2. Problema

Sem roteamento de foco:

```text
WASD move o personagem atrás de inventário;
ataque/interact dispara atrás de diálogo;
ESC fecha camada errada;
hotbar troca item enquanto shop está aberto;
modal de confirmação recebe input junto com menu base;
debug HUD confunde foco final;
submodal pode deixar o jogo travado sem voltar foco.
```

---

## 3. Objetivo

Criar/endurecer:

```text
UIFocusState;
UIFocusLayer;
UIFocusStack;
ModalInputPolicy;
GameplayInputGate;
InputRoutingDecision;
Submodal handling;
Confirm/Cancel contract;
Focus transition events;
tests.
```

---

## 4. Regras de design

```text
Só um foco principal ativo por vez.
Submodal pode existir dentro do foco atual.
Input de gameplay só é aceito em GameplayFocus.
ESC/B/back fecha camada atual ou volta uma camada.
Confirm/Interact confirma apenas elemento focado.
Cancel fecha/volta; não executa ação de mundo.
System modal tem prioridade máxima.
```

---

## 5. User stories / engineering stories

```text
Como jogador, quando abro inventário, WASD não move.
Como diálogo, escolhas recebem input, não o mundo.
Como shop/crafting/skill tree, confirmação afeta apenas elemento focado.
Como system menu, prioridade bloqueia todos os outros inputs.
Como debug, quero ver foco atual sem HUD final depender disso.
```

---

## 6. Escopo

Inclui:

```text
focus states;
modal layer stack;
input priority;
gameplay action gating;
confirm/cancel routing;
submodal handling;
focus debug projection;
tests/validators.
```

Não inclui:

```text
final UI layout;
input remapping;
gamepad full navigation;
scene/prefab wiring;
save/load settings.
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
- docs/design/gameplay/player/PLAYER_CORE_SYSTEMS_DIRECTION.md
- docs/design/gameplay/combat/COMBAT_CORE_DIRECTION.md
- docs/design/gameplay/combat/STATUS_EFFECTS_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md

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

- UI modal bloqueia input de gameplay.
- UI precisa ter foco explícito.
- Quando UI modal está aberta, WASD não move, Dash/Dodge/Block não executam, ataque não executa, Interact não interage com mundo atrás, hotbar não troca salvo se UI permitir.
- Estados mínimos incluem GameplayFocus, DialogueFocus, MenuFocus, ShopFocus, InventoryFocus, CraftingFocus, SkillTreeFocus, QuestLogFocus, SocialLogFocus, SystemFocus e DebugFocus.
- Prioridade: System modal, Confirmation modal, Dialogue choice, Shop/crafting/inventory action, Menu navigation, Gameplay action.
- ESC/B/back fecha a camada atual ou volta uma camada.

### Deferred / future from directions

- Gamepad final.
- Keybind remapping UI.
- Prefab/layout implementation.
- Pause rules final for every menu.
- Accessibility options screen.

### Explicitly not redefined here

- Player movement implementation.
- Combat input implementation.
- Specific menu view models.
- System menu/pause implementation.
- Input package settings.

## 7. Modelo de domínio

### 7.1 UIFocusState

```text
GameplayFocus
DialogueFocus
MenuFocus
ShopFocus
InventoryFocus
CraftingFocus
SkillTreeFocus
QuestLogFocus
SocialLogFocus
CalendarFocus
MapFocus
FonteFocus
SystemFocus
DebugFocus
```

### 7.2 UIFocusLayer

```text
LayerId
FocusState
ModalKind
BlocksGameplayMovement
BlocksGameplayActions
BlocksHotbar
ConsumesInteract
ConsumesCancel
ConsumesConfirm
ParentLayerId optional
OpenedBy
OpenedAtFrame optional
DebugName
```

### 7.3 ModalKind

```text
None
LightOverlay
ModalGameplayMenu
DialogueModal
ConfirmationModal
SystemMenu
DebugOverlay
```

### 7.4 InputRoutingDecision

```text
InputActionId
Target: Gameplay | UI | Blocked
FocusState
LayerId
Reason
CanBubble
DebugNotes[]
```

---

## 8. Input priority table

| Input | GameplayFocus | Modal focus |
|---|---|---|
| WASD/move | gameplay | blocked/UI nav if mapped |
| Attack | gameplay | blocked |
| Dash/Dodge/Block | gameplay | blocked |
| Interact | world interact | UI confirm if focused |
| Hotbar | gameplay hotbar | blocked unless UI consumes |
| ESC/Cancel | system/pause | close top layer |
| Confirm | gameplay interact | focused UI element |
| Mouse click | world/click if no UI capture | UI element/captured |

---

## 9. Focus stack rules

```text
Open modal:
  push layer.
Close modal:
  pop layer.
Close submodal:
  return to parent layer.
Close final modal:
  return to GameplayFocus unless SystemFocus remains.
Invalid close:
  log warning, repair stack if safe.
```

---

## 10. Criteria

```text
Focus stack exists or is hardened.
Gameplay action gating is deterministic.
Modal priority order respected.
Submodal returns focus correctly.
Debug focus visible only in debug.
Tests cover inventory, dialogue, confirmation, shop, system menu, nested modal and invalid stack recovery.
```

---

# /speckit.plan

## 11. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Input/UIFocusState.cs
Assets/_Game/Scripts/UI/Input/UIFocusLayer.cs
Assets/_Game/Scripts/UI/Input/ModalKind.cs
Assets/_Game/Scripts/UI/Input/InputRoutingDecision.cs
Assets/_Game/Scripts/UI/Input/UIFocusStack.cs
Assets/_Game/Scripts/UI/Input/GameplayInputGate.cs
Assets/_Game/Scripts/UI/Input/UIInputRouter.cs
Assets/_Game/Tests/EditMode/UI/UIInputFocusModalRoutingTests.cs
```

Consolidar existentes se houver.

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/Input/**
Assets/_Game/Scripts/Player/**
Assets/_Game/Scripts/Editor/Validation/**
Assets/_Game/Tests/EditMode/UI/**
docs/validation/11_spec_ui_input_focus_modal_routing_runtime_execution_report.md
```

---

## 13. Arquivos proibidos

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

## 14. Estratégia

```text
1. Auditar UI/input/focus/player input existentes.
2. Consolidar focus states/layers.
3. Implementar/harden input router/gate.
4. Garantir nested modal behavior.
5. Criar tests.
6. Criar report.
```

---

## 15. Paralelização

- Parallelizable: YES, after locking `UI/Input/**`.
- Can run with:
  - HUD projection if read-only.
- Must not run with:
  - player input rewrite;
  - Input System package settings;
  - scene/prefab menu wiring.
- Reason: foundation of UI input.

---

## 16. Impacto save/load

```text
Does this change save schema? NO.
Does this add save section? NO.
Does this require migration? NO.
Does this persist Unity references? NO.
```

---

## 17. Impacto eventos

```text
Adds events: CONDITIONAL, e.g. UIFocusChangedEvent.
Changes events: SHOULD BE NO.
Requires unsubscribe pattern: YES if subscribers are created.
```

---

## 18. Impacto UI/Unity

```text
Changes UI runtime focus/input: YES.
Changes prefabs/layout: NO.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED for menu/input visual validation.
```

---

## 19. Riscos

```text
Risco: input action names differ.
Mitigação: local audit and adapter.

Risco: focus stack leaks after modal close.
Mitigação: stack tests.

Risco: gameplay blocked permanently.
Mitigação: invalid stack recovery.
```

---

# /speckit.tasks

## 20. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar input/focus systems.
- [ ] T003 — Consolidar focus contracts.
- [ ] T004 — Implementar/harden router/gate.
- [ ] T005 — Criar tests.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions de UI/UX e domínios afetados? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes relacionados a UI input focus/modal routing foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| UI não é fonte | A UI apenas projeta/comanda, sem virar fonte de verdade? | Contrato ViewModel/Command separado de State. | PARTIAL |
| Input seguro | UI modal bloqueia gameplay input quando aplicável? | Teste/validator de foco. | PARTIAL |
| Anti-spoiler | UI não revela Quest/Main/Fonte/Anya/101/final antes de descoberta? | Visibility/filter tests. | PARTIAL |
| Anti-debug | HUD final não depende de Debug HUD e não mostra ids internos? | Checklist/test. | PARTIAL |
| Pets/social future | A execução não implementou Pets/Social/Romance runtime? | Checklist explícito. | BLOCKED se violar |
| Save/load | Houve alteração de schema? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/11_spec_ui_input_focus_modal_routing_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "UIFocus|InputFocus|Modal|GameplayInputGate|UIInputRouter|WASD|Interact|Cancel|Confirm|Hotbar" Assets/_Game/Scripts docs/design .specs
rg -n "UIFocus|InputFocus|Modal|HUD|Hotbar|Tooltip|Inventory|Equipment|Shop|Crafting|SkillTree|QuestLog|Fonte|Notification|DebugHUD|Breath|Folego|Pet|Social|Romance|FinalChoice" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a UI input focus/modal routing existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o comportamento segue UI_UX_FULL_GAMEPLAY_DIRECTION
And input, foco, projeções, tooltips, notifications e debug separation permanecem consistentes
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

### Scenario 3 — Modal/input safety

```text
Given uma UI modal, diálogo, loja, inventário, crafting, skill tree, quest log, Fonte menu ou system menu está aberta
When o jogador pressiona WASD/ataque/interact/hotbar
Then gameplay action não executa atrás da UI
And input vai para foco/camada correta.
```

### Scenario 4 — Spoiler/protected state

```text
Given item quest/key, Fonte não desbloqueada, final choice não disponível, quest oculta, social/pet future ou debug data
When UI/projection/tooltip/render consulta o estado
Then só informação autorizada aparece
And ações irreversíveis/destrutivas exigem confirmação adequada.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige inspeção visual de layout, foco, tooltip, modal, HUD, menu, notification ou feedback
When a spec termina tecnicamente
Then o report registra cenário final em docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md
And não pede validação humana imediata por spec.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- WASD moves while modal open.
- Attack/interact fires behind dialogue.
- Hotbar changes behind inventory/shop.
- ESC closes wrong layer.
- Nested confirmation loses parent focus.
- System menu not highest priority.
- Gameplay stays blocked after modal close.
- DebugFocus becomes final UX dependency.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Input Focus Modal Routing Runtime

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

## UI/UX compliance
- UI projection not source of truth:
- Modal input blocking:
- Focus stack/layering:
- Anti-spoiler:
- Debug HUD separation:
- No Breath/Folego/BR final HUD:
- No Pet/Social/Romance runtime:
- Final human validation deferred:

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Existing partial implementation:
- Modal/input safety:
- Spoiler/protected state:
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
4. A implementação exigir reescrever UI/Input/Inventory/Quest/Fonte/Economy canônico existente.
5. A implementação permitir WASD/ataque/interact atrás de UI modal.
6. A implementação tornar UI fonte de verdade de inventory/equipment/shop/quest/Fonte.
7. A implementação mostrar Breath/Fôlego/BR na HUD final.
8. A implementação vazar spoiler de Anya, nível 101, final choice ou quest oculta.
9. A implementação criar Pet/Social/Romance runtime fora de specs futuras dedicadas.
10. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Modal Blocking Matrix

| Modal | Blocks movement | Blocks attack | Consumes interact | Can allow hotbar |
|---|---:|---:|---:|---:|
| Dialogue | YES | YES | YES | NO |
| Inventory | YES | YES | YES | contextual only |
| Shop | YES | YES | YES | NO |
| Crafting | YES | YES | YES | NO |
| SkillTree | YES | YES | YES | NO |
| QuestLog | YES | YES | YES | NO |
| Fonte | YES | YES | YES | NO |
| Confirmation | YES | YES | YES | NO |
| System | YES | YES | YES | NO |
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "UIFocus|InputFocus|Modal|HUD|Hotbar|Tooltip|Inventory|Equipment|Shop|Crafting|SkillTree|QuestLog|Fonte|Notification|DebugHUD|Breath|Folego|Pet|Social|Romance|FinalChoice" Assets/_Game/Scripts docs/design .specs
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
Quando houver cenário visual/gameplay de UI, foco, HUD, tooltip, shop, crafting, skill tree, quest log, Fonte menu ou notification, registrar em execution report e vincular a docs/validation/FINAL_HUMAN_VALIDATION_BY_WAVE.md.
```

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES, input routing/focus stack logic is deterministic.
- Requires EditMode tests: YES for focus stack/routing/modal/blocking/nested tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for actual UI/input visual validation.
- Requires regression test: YES if fixing existing WASD/input bleed bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver UI/gameplay integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; modal input bleed blocked.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/11_spec_ui_input_focus_modal_routing_runtime_execution_report.md.
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
Não deixar gameplay input passar por modal.
Não mostrar Breath/Fôlego/BR.
Não vazar spoiler oculto cedo.
Não implementar Pets/Social/Romance.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
