# SPEC — UI Menu Gamepad Navigation Future

> **Spec ID:** `04_spec_ui_menu_gamepad_navigation_future`  
> **Status:** A implementar  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P2 / Future  
> **Type:** Future / UI / Input / Gamepad Navigation / Focus Order  
> **Domain:** UI / Gamepad / Navigation / Focus Order / Accessibility Future  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere input system package/config, ProjectSettings input actions, UI focus stack runtime, per-screen focus order, or scene/prefab navigation wiring.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Input/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/04_spec_ui_menu_gamepad_navigation_future_execution_report.md`  
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
  - `.specs/a_implementar/04_spec_ui_empty_error_confirmation_patterns_runtime.md`
> **Blocks:**  
  - input focus modal routing;
  - focus order per screen;
  - common confirmation patterns;
  - future accessibility settings;
> **Scope:** preparar contrato future de navegação por gamepad sem alterar ProjectSettings/Input Actions e sem wiring de prefabs nesta etapa.  
> **Out of scope:** configurar Input System package, alterar ProjectSettings, mapear controles finais, prefab navigation wiring, accessibility settings completas, rebinding final.

---

# /speckit.specify

## 1. Contexto

O direction de menu lista focus order por tela e deixa gamepad final como pendência aberta. Esta spec é future: prepara contratos para navegação sem mexer em ProjectSettings/Input Actions.

O foco agora é impedir que telas criadas fiquem impossíveis de navegar no futuro e garantir que cada tela declare ordem de foco, back/cancel/confirm e comportamento de submodal.

---

## 2. Problema

Sem contrato de gamepad future:

```text
telas ficam dependentes de mouse;
focus order é inconsistente;
back/cancel fecha tela errada;
confirmation modal pode focar Confirm em ação destrutiva;
skill tree graph pode ser impossível de navegar;
storage dual-grid pode perder origem/destino;
futuro gamepad exigirá refactor de todas as telas.
```

---

## 3. Objetivo

Criar contrato future-safe:

```text
cada tela tem focus order conceitual;
confirm/cancel/back consistentes;
submodal owns focus;
destructive confirmation initial focus on Cancel;
left/right/up/down navigation profile future;
no ProjectSettings changes;
no prefab wiring now.
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

- Focus order por tela é definido: Inventory, Equipment, Skill Tree, Shop, Crafting, Quest Log, Calendar e Fonte.
- Runtime flags conceituais incluem CurrentUIScreen, CurrentUIFocus, PreviousUIFocus, IsModalOpen, IsGameplayInputBlocked e PendingConfirmationAction.
- Gamepad final ainda é pendência aberta.
- Todo menu modal bloqueia WASD e input de gameplay.
- Back/Cancel deve fechar camada correta.
- Confirmações fortes exigem cuidado em foco inicial.

### Deferred / future from directions

- Input System mapping final.
- ProjectSettings/Input Actions.
- Prefab navigation wiring.
- Full gamepad rebinding.
- Accessibility settings.
- Controller glyphs.

### Explicitly not redefined here

- Input focus runtime.
- Individual screen view models.
- Confirmation modal runtime.
- Scene/prefab navigation.

## 4. Estado atual do repo

```text
Input focus/modal foundation foi especificado.
Gamepad final não deve ser implementado agora.
Esta spec é contrato/future validation para evitar dívida técnica.
```

A confirmar localmente:

```text
Input focus profiles;
UIScreenConfig;
existing navigation helpers;
confirmation modal focus;
per-screen selected IDs.
```

---

## 5. User stories

```text
Como dev, quero cada tela com focus order claro para gamepad futuro.
Como jogador futuro de gamepad, quero navegar sem mouse.
Como UI, quero back/cancel/confirm previsíveis.
Como spec futura, quero saber onde alterar sem refactor massivo.
```

---

## 6. Escopo

```text
focus order profiles;
future navigation contract;
confirmation focus defaults;
screen-level selected IDs;
validation helpers;
no ProjectSettings changes.
```

---

## 7. Fora de escopo

```text
Input System mapping;
gamepad rebinding;
controller glyphs;
prefab navigation wiring;
accessibility settings;
final gamepad testing.
```

---

## 8. Regras de não duplicação

```text
Não alterar ProjectSettings.
Não criar novo input system.
Não wiring prefabs.
Não exigir gamepad final agora.
Não criar per-screen ad hoc navigation sem profile.
```

---

## 9. Critérios de aceite

- Focus order contract documented/implemented as metadata if safe.
- Per-screen selected IDs supported conceptually.
- Confirmation strong initial focus policy.
- No ProjectSettings changes.
- Report lists future tasks.
- Structural tests/validators if feasible.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Navigation/UINavigationProfile.cs
Assets/_Game/Scripts/UI/Navigation/UIScreenFocusOrder.cs
Assets/_Game/Scripts/UI/Navigation/UIFutureGamepadNavigationValidator.cs
Assets/_Game/Tests/EditMode/UI/UINavigationProfileTests.cs
```

Criar apenas metadata/validator se seguro; otherwise document future contract only.

---

## 11. Contratos

### Runtime

```text
Future navigation profile is metadata/control contract.
It must not alter ProjectSettings or Input Actions.
```

### Save

```text
No save schema change.
```

### UI

```text
Each modal/screen should define default focus and close/back behavior.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/Input/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_menu_gamepad_navigation_future_execution_report.md
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
1. Auditar focus/navigation helpers.
2. Criar metadata/validator only if safe.
3. Do not touch ProjectSettings.
4. Add tests for focus order and destructive confirm default.
5. Report future tasks.
```

---

## 15. Ordem segura

```text
Input focus -> common patterns -> future navigation metadata -> final gamepad later.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with input system/package/settings changes.
- Reason: future gamepad contract must not mutate runtime input config now.

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
Changes UI: CONDITIONAL metadata/validators only.
Scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: NO, future.
```

---

## 20. Riscos

```text
Risco: tocar ProjectSettings por engano.
Mitigação: explicit stop condition.

Risco: metadata virar promessa final.
Mitigação: future-only status.

Risco: profile muito rígido.
Mitigação: validators warning-first.
```

---

## 21. Rollback

```text
Remover navigation metadata/validators/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar focus/navigation helpers.
- [ ] T003 — Criar metadata/validator se seguro.
- [ ] T004 — Criar tests.
- [ ] T005 — Rodar validações.
- [ ] T006 — Criar report.

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Classes/sistemas existentes foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| UI focus | A tela usa focus/modal stack canônico? | Integração com input/modal foundation. | BUILD_VALIDATED no máximo |
| Spoiler/future | A spec respeita dados ocultos e features future? | Matriz de visibility/future no report. | PARTIAL |
| Save/load | A UI altera estado persistido ou só despacha comando? | Declaração de schema/no schema. | PARTIAL |
| Eventos | Subscriptions têm lifecycle? | Mapa de publishers/subscribers se houver. | PARTIAL |
| Testes | Há view model/projection determinística? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report criado? | `docs/validation/04_spec_ui_menu_gamepad_navigation_future_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar:

```bash
rg -n "Gamepad|Navigation|FocusOrder|UIScreenConfig|UIFocusProfile|InputAction|ProjectSettings|Confirm|Cancel|Back|SelectedInventorySlot|SelectedQuestId" Assets/_Game/Scripts docs/design .specs
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
Given a tela/flow de future gamepad/navigation profile é aberto com dados válidos
When o jogador navega, seleciona elemento e executa ação segura
Then a UI mostra foco claro, detalhe correto e ações disponíveis
And gameplay input fica bloqueado se modal estiver aberto
And o domínio recebe apenas comando/intent válido, não mutação escondida de UI.
```

### Scenario 2 — Unknown/hidden/future data

```text
Given existe informação oculta, futura, bloqueada por história ou ainda não descoberta
When a UI renderiza a tela
Then ela não revela spoiler nem promessa quebrada
And mostra estado genérico/??? apenas quando o direction permitir
And registra no report qualquer placeholder/future hook.
```

### Scenario 3 — Missing requirement / unavailable state

```text
Given o jogador não cumpre requisito, não tem recurso ou não conhece a informação
When a ação ou detalhe é selecionado
Then a UI mostra motivo bloqueado
And não executa comando inválido
And o backend continua sendo fonte de verdade.
```

### Scenario 4 — Costly or irreversible action

```text
Given uma ação é custosa, destrutiva, irreversível ou protegida
When o jogador tenta executá-la
Then confirmação é exigida quando o direction manda
And foco inicial respeita regra de ação destrutiva
And cancel/back não executa ação de mundo.
```

### Scenario 5 — Final human validation deferred

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

- ProjectSettings/Input Actions alterado.
- Prefab navigation wiring alterado.
- Gamepad final prometido sem implementação.
- Tela sem default focus.
- Back/cancel sem behavior definido.
- Strong confirmation focando Confirm por padrão.
- Skill tree/storage sem navegação conceitual.
- Metadata divergente do focus/modal foundation.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Menu Gamepad Navigation Future

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
- Unknown/hidden/future:
- Missing requirement:
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
6. A implementação revelar spoiler/future state proibido pelo direction.
7. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Future Focus Order Matrix

| Screen | Focus order |
|---|---|
| Inventory | Grid -> Detail actions -> Category tabs -> Hotbar -> Close |
| Equipment | Equipment slots -> Candidate list -> Comparison drawer actions -> Close |
| Skill Tree | Tree tabs -> Node graph -> Node detail actions -> Active slots -> Respec -> Close |
| Shop | Buy/Sell tabs -> Item list -> Quantity selector -> Confirm -> Detail drawer -> Close |
| Crafting | Category tabs -> Recipe list -> Recipe detail -> Quantity selector -> Craft -> Close |
| Quest Log | Category tabs -> Quest list -> Quest detail -> Track/untrack -> Close |
| Calendar | Month/season grid -> Day detail -> Event list -> Close |
| Fonte | Function list -> Function detail -> Confirm/Cancel -> Close |

## 23H. Strong Confirmation Focus Rule

```text
Strong confirmation initial focus should default to Cancel.
Light confirmation may default to Confirm only if action is reversible/low risk.
Back/cancel never executes the action.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Weapon|Armor|Repair|Upgrade|Spell|Magic|Gamepad|Navigation|Focus|Detail|Durability|Material|MP|Cooldown|Requirement" Assets/_Game/Scripts docs/design .specs
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

- Changed deterministic logic: YES if navigation metadata/validator logic is created.
- Requires EditMode tests: YES for focus order/confirmation focus validator tests.
- Requires PlayMode automated or final human scenario: NO, future-only unless UI metadata is wired; then DEFERRED.
- Requires regression test: YES if fixing known focus/navigation bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no ProjectSettings changes; gamepad remains future.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_menu_gamepad_navigation_future_execution_report.md.
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
Não revelar spoiler, future state ou função bloqueada.
```

---

## 29. Notas para execução posterior

Esta spec deve ser executada por Claude Code/Codex com auditoria local do repo, usando `rg` e validando o estado real dos sistemas já implementados.

Se a auditoria local revelar sistema equivalente já existente, a execução deve mudar para hardening/residual, não recriar do zero.
