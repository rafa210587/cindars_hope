# SPEC — UI Input Focus Modal Routing Runtime

> **Spec ID:** `04_spec_ui_input_focus_modal_routing_runtime`  
> **Status:** A implementar  
> **Revision:** EXPANDED_06_07  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P0  
> **Type:** Runtime / UI / Input Routing / Modal Focus / Hardening  
> **Domain:** UI / Input / Focus / Modal Stack  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere GameplayInputRouter, ModalManager, UIEvents, dialogue/shop/inventory/crafting/quest log/calendar input, player movement input ou pause/system menu.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Input/**`, `Assets/_Game/Scripts/Player/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/04_spec_ui_input_focus_modal_routing_runtime_execution_report.md`  
> **Depends on:**  
  - `docs/design/SPEC_SOURCE_MAP.md`
  - `docs/design/SPECIFICATION_PROCESS.md`
  - `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
  - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
  - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
  - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
  - `docs/project/CURRENT_STATE.md`
  - `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
  - `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
  - `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
> **Blocks:**  
  - inventory/equipment/crafting/shop UI;
  - dialogue choice runtime;
  - quest log/calendar UI;
  - pause/system menu;
  - PlayMode UI validation baseline.
> **Scope:** auditar e endurecer input routing/focus/modal stack para garantir que UI modal bloqueia gameplay input e roteia foco corretamente.  
> **Out of scope:** criar layouts finais de telas, reescrever todos os menus, criar gamepad final, alterar scenes/prefabs sem spec própria, implementar acessibilidade completa.

---

# /speckit.specify

## 1. Contexto

O direction de UI/UX define regra principal: quando qualquer UI modal estiver aberta, WASD não move o personagem, ataque/dash/block/interact/hotbar não executam ações de mundo, e input vai para navegação da UI.

A auditoria documental indica que já existem peças parciais como GameplayInputRouter/ModalManager/UIEvents em specs antigas. Esta spec deve ser hardening/residual se esses sistemas existirem.

---

## 2. Problema

Sem roteamento/foco central:

```text
jogador move enquanto navega menu;
interact ativa mundo atrás da UI;
ESC fecha camada errada;
submodal perde foco;
dialogue choice e shop brigam por input;
UI final depende de hacks locais por tela.
```

---

## 3. Objetivo

Garantir contrato único de foco/modal:

```text
GameplayFocus aceita gameplay input;
DialogueFocus/MenuFocus/ShopFocus/etc bloqueiam gameplay input;
um foco principal por vez;
submodal usa stack;
ESC/B/back fecha camada atual;
Confirm/Interact só confirma elemento focado;
debug focus separado.
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

- UI modal bloqueia WASD, dash, attack, interact e hotbar de gameplay.
- Estados mínimos: GameplayFocus, DialogueFocus, MenuFocus, ShopFocus, InventoryFocus, CraftingFocus, SkillTreeFocus, QuestLogFocus, SystemFocus, DebugFocus.
- Só um foco principal ativo por vez.
- ESC/B/back fecha camada atual ou volta uma camada.
- Prioridade de input: system modal, confirmation modal, dialogue choice, shop/crafting/inventory action, menu navigation, gameplay action.

### Deferred / future from directions

- Gamepad final.
- Accessibility settings complete.
- Visual layout final de cada menu.
- Localization/input rebinding final.

### Explicitly not redefined here

- Player movement system.
- Dialogue content.
- Shop economy.
- Inventory data model.
- Quest Log data.

## 4. Estado atual do repo

```text
Specs anteriores mencionam GameplayInputRouter, ModalManager, UIEvents e fixes de input modal.
Esta spec deve auditar o que existe antes de criar qualquer coisa nova.
```

A confirmar localmente:

```text
GameplayInputRouter;
ModalManager;
UIEvents;
PlayerInputController;
current focus enum/state;
UI close/back handling;
modal/submodal behavior.
```

---

## 5. User stories

```text
Como jogador, ao abrir inventário, WASD não move personagem.
Como jogador, ESC fecha submodal antes do menu pai.
Como UI, quero foco explícito e previsível.
Como dev, quero adicionar tela nova sem criar input hack.
```

---

## 6. Escopo

```text
focus state contract;
modal stack;
gameplay input block;
UI navigation routing;
back/cancel/confirm behavior;
debug validation/tests.
```

---

## 7. Fora de escopo

```text
layouts finais;
gamepad final;
input rebinding;
todos os menus;
visual polish;
scene/prefab wiring final.
```

---

## 8. Regras de não duplicação

```text
Não criar segundo input router se já existir.
Não criar modal manager paralelo.
Não deixar tela decidir sozinha se gameplay input passa.
Não usar debug HUD para comunicar foco ao jogador.
```

---

## 9. Critérios de aceite

- Modal aberto bloqueia gameplay action.
- Focus state único.
- Stack fecha na ordem correta.
- Confirm/cancel roteiam para UI focada.
- Tests/validator cobrem foco determinístico quando praticável.
- Report lista PlayMode scenarios finais.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Input/GameplayInputRouter.cs
Assets/_Game/Scripts/UI/Modal/ModalManager.cs
Assets/_Game/Scripts/UI/Modal/UIModalFocusState.cs
Assets/_Game/Tests/EditMode/UI/InputFocusRoutingTests.cs
```

Se já existirem, consolidar em vez de duplicar.

---

## 11. Contratos

### Runtime

```text
Gameplay input is allowed only in GameplayFocus.
Modal stack owns focus.
Submodal is scoped inside current focus.
```

### Events

```text
UI focus changed event may exist but cannot replace source of truth.
```

### Save

```text
No save schema change.
```

### UI

```text
Every modal declares desired focus.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/Input/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_input_focus_modal_routing_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Player/**
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
1. Auditar input/router/modal existentes.
2. Consolidar focus enum/contract.
3. Adicionar tests de routing sem cena quando possível.
4. Documentar PlayMode final scenarios.
5. Criar report.
```

---

## 15. Ordem segura

```text
Input focus foundation -> HUD/menus/dialogue/shop/crafting/quest log.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with any UI screen/input spec.
- Reason: foco/modal é base global.

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
Adds events: CONDITIONAL for UI focus changed.
Changes existing events: SHOULD BE NO.
Requires unsubscribe pattern: YES if event subscribers are added.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: YES, focus/input behavior.
Changes scenes/prefabs/assets: NO in this spec.
Requires PlayMode automated or final human scenario: YES, DEFERRED_TO_FINAL_VALIDATION.
```

---

## 20. Riscos

```text
Risco: quebrar controles existentes.
Mitigação: audit + tests.

Risco: focos locais divergirem.
Mitigação: central router.
```

---

## 21. Rollback

```text
Reverter input/router/modal changes/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar input/modal/focus existentes.
- [ ] T003 — Consolidar contrato.
- [ ] T004 — Implementar hardening mínimo.
- [ ] T005 — Criar tests/validator.
- [ ] T006 — Rodar validações.
- [ ] T007 — Criar report.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu os directions e refinements listados em Source Map Compliance? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | A execução auditou classes existentes antes de criar novas? | Comandos `rg` e achados principais no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão: reuse/harden/create. | BLOCKED se criar duplicata |
| Save/load | A spec altera ou depende de estado persistido? | Declaração explícita de schema/no schema. | PARTIAL |
| Eventos | A spec cria/usa eventos ou subscriptions? | Mapa de publishers/subscribers e unsubscribe policy. | PARTIAL |
| UI/Input | Há foco/modal/PlayMode relevante? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo |
| Testes | Há lógica determinística nova? | EditMode test ou justificativa NOT RUN. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/04_spec_ui_input_focus_modal_routing_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "GameplayInputRouter|ModalManager|InputFocus|UIFocus|DialogueFocus|MenuFocus|ShopFocus|InventoryFocus|Pause|Back|Cancel|Confirm" Assets/_Game/Scripts docs/design docs/specs
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
Given o sistema base relacionado a UI input focus/modal routing existe ou foi criado de forma mínima
When o usuário/sistema executa o fluxo principal desta spec
Then o estado visível/resultado segue o direction canônico
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Missing dependency

```text
Given uma dependência runtime não existe no repo local
When a execução encontra essa ausência
Then ela não inventa uma solução massiva
And marca como MISSING_BUT_DEFER ou cria apenas adapter/validator mínimo se seguro
And registra risco residual.
```

### Scenario 3 — Save/load safety

```text
Given o fluxo envolve estado persistido direta ou indiretamente
When save/load ocorre após a ação
Then nenhum dado derivado de UI/debug substitui a fonte de verdade
And nenhum UnityEngine.Object é persistido
And schema change exige spec/migration separada.
```

### Scenario 4 — Final human validation deferred

```text
Given o fluxo exige interação visual ou PlayMode integrado
When a spec é concluída tecnicamente
Then o report registra cenário final em vez de pedir validação humana imediata
And o status máximo respeita SPEC_VALIDATION_MATRIX_MASTER.md.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Abrir modal e WASD ainda mover personagem.
- Interact confirmar UI e também acionar objeto do mundo.
- ESC fechar menu pai antes de submodal.
- Confirmação crítica não capturar foco.
- DialogueFocus e ShopFocus ativos ao mesmo tempo.
- Focus state preso após fechar tela.
- Subscriptions duplicadas após abrir/fechar modal repetidas vezes.
- DebugFocus interferindo no gameplay final.

---

## 23E. Minimum Execution Report Template

O execution report desta spec deve conter, no mínimo:

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

## Implementation decision
- REUSE_EXISTING / HARDEN_EXISTING / CREATE_MINIMAL / DEFER
- Justification:

## Files changed
- ...

## Functional evidence
- Happy path:
- Edge cases:
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

Parar a execução e registrar `BLOCKED` se ocorrer qualquer um destes casos:

```text
1. A implementação exigir alterar Packages/ ou ProjectSettings/.
2. A implementação exigir scene/prefab/asset wiring fora do escopo.
3. A implementação exigir mudança de save schema sem migration spec.
4. A implementação exigir reescrever sistema canônico existente.
5. A implementação criar conflito com 01Q, input focus, save ownership ou registry.
6. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Focus State Contract

| Focus | Gameplay movement | Combat input | Hotbar | UI navigation | Back/Cancel |
|---|---|---|---|---|---|
| GameplayFocus | ENABLED | ENABLED | ENABLED | DISABLED/N/A | Opens pause/menu if mapped |
| DialogueFocus | BLOCKED | BLOCKED | BLOCKED | ENABLED | Close/previous dialogue layer |
| MenuFocus | BLOCKED | BLOCKED | BLOCKED | ENABLED | Close current menu |
| ShopFocus | BLOCKED | BLOCKED | BLOCKED | ENABLED | Close shop or tab/back |
| InventoryFocus | BLOCKED | BLOCKED | BLOCKED | ENABLED | Close inventory/submodal |
| CraftingFocus | BLOCKED | BLOCKED | BLOCKED | ENABLED | Close crafting/submodal |
| SkillTreeFocus | BLOCKED | BLOCKED | BLOCKED | ENABLED | Close skill tree/submodal |
| QuestLogFocus | BLOCKED | BLOCKED | BLOCKED | ENABLED | Close quest log |
| SystemFocus | BLOCKED | BLOCKED | BLOCKED | ENABLED | Close confirmation/options |
| DebugFocus | CONFIGURABLE | CONFIGURABLE | CONFIGURABLE | ENABLED | Debug-only |

## 23H. Modal Stack Invariants

```text
Top modal owns input.
Only top modal receives confirm/cancel.
Closing top modal restores previous focus.
Closing last modal restores GameplayFocus unless paused/system state remains.
Nested confirmation must not allow gameplay input.
Destroyed modal must unregister itself.
```

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Quest|Objective|Condition|Trigger|Reward|Flag|Softlock|Debug|InputFocus|Modal|HUD|Notification" Assets/_Game/Scripts docs/design docs/specs
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

- Changed deterministic logic: YES, input routing/focus logic is deterministic.
- Requires EditMode tests: YES for focus/modal routing when harness available.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for modal open/close and player input blocked PlayMode scenario.
- Requires regression test: YES if fixing existing WASD-moves-during-menu bug.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode routing tests PASS or NOT RUN justified; final PlayMode scenario documented; no scene/prefab changes.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_input_focus_modal_routing_runtime_execution_report.md.
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
Não revelar spoilers antes de discovery/visibility policy.
Não pedir human test por spec.
Não executar runtime em massa antes da 01Q ou exceção humana explícita.
Não alterar SPEC_EXECUTION_ORDER.md.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
