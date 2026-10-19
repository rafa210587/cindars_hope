# SPEC — UI Fonte Menu Flow Runtime

> **Spec ID:** `04_spec_ui_fonte_menu_flow_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P1  
> **Type:** Runtime / UI / Fonte / Fragment-gated Functions  
> **Domain:** UI / Fonte / Respawn / Água Viva / Respec / Purification / Final Choice  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere Fonte runtime, MainProgressionSection, FonteAnyaSection, respec backend, Living Water runtime, purification/advanced healing, final choice or death/respawn.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Fonte/**`, `Assets/_Game/Scripts/Progression/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/04_spec_ui_fonte_menu_flow_runtime_execution_report.md`  
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
  - `docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md`
  - `.specs/a_implementar/03_spec_quest_fonte_main_progression_hooks_future.md`
  - `.specs/a_implementar/04_spec_ui_input_focus_modal_routing_runtime.md`
> **Blocks:**  
  - Fonte/Main hooks;
  - respec via Fonte;
  - Living Water runtime;
  - purification runtime;
  - final choice runtime;
> **Scope:** consolidar Fonte Menu como projection fragment-gated, mostrando apenas funções desbloqueadas e ocultando spoilers de Anya/final/nível 101.  
> **Out of scope:** Fonte runtime completo, respec backend, Living Water mechanics, purification/healing mechanics, final choice gameplay, scene/prefab/art final.

---

# /speckit.specify

## 1. Contexto

O direction de menu define Fonte Menu com estados: Adormecida/Respawn, Fragmento da Água/Água Viva, Fragmento da Memória/Respec, Fragmento da Vida/Purificação e cura avançada, Fragmento da Esperança/decisão final.

O direction de main progression define a ordem canônica dos fragmentos: Água -> Memória -> Vida -> Esperança. Também define que Anya não pode voltar inteira e que o nível 101/final não deve aparecer cedo.

Esta spec é UI/projection: não implementa as funções profundas da Fonte.

---

## 2. Problema

Sem Fonte Menu seguro:

```text
respec pode aparecer antes da Memória;
purificação pode aparecer antes da Vida;
decisão final pode aparecer antes da Esperança;
Anya/final/nível 101 podem ser revelados cedo;
ação custosa pode ser executada sem confirmação;
UI pode gravar estado da Fonte em vez de chamar serviço.
```

---

## 3. Objetivo

Criar/endurecer Fonte Menu:

```text
mostrar estado visual/conceitual da Fonte;
listar funções desbloqueadas;
ocultar ou ??? funções futuras apenas quando design permitir;
mostrar custo/recurso;
usar texto curto sem spoiler;
exigir confirmação para ações custosas;
não revelar Anya completa/final/nível 101.
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
- docs/design/gameplay/quests/QUESTS_MAIN_PROGRESSION_REFINEMENT_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Fonte Menu mostra funções desbloqueadas por fragmentos.
- Estados: Adormecida/Respawn, Água/Água Viva, Memória/Respec, Vida/Purificação e cura avançada, Esperança/decisão final.
- Não mostrar respec antes do Fragmento da Memória.
- Não mostrar purificação antes do Fragmento da Vida.
- Não mostrar decisão final antes do Fragmento da Esperança.
- Não explicar Anya completamente cedo.
- Não revelar nível 101 cedo.
- Fragmentos seguem ordem Água -> Memória -> Vida -> Esperança.

### Deferred / future from directions

- Fonte runtime completo.
- Respec backend completo.
- Living Water gameplay completo.
- Purification/healing mechanics.
- Final choice implementation.
- Fonte art/scene/prefab final.

### Explicitly not redefined here

- MainProgression backend.
- FonteAnyaSection save.
- Death/respawn system.
- Skill respec system.
- Quest content.

## 4. Estado atual do repo

```text
Fonte/Main hooks foram planejados na WAVE 03.
Skill respec/Fonte pode existir parcialmente por specs antigas.
Esta spec deve auditar e agir como UI hardening/projection.
```

A confirmar localmente:

```text
FonteManager;
Fonte UI;
MainProgression/Fonte state;
Skill respec gates;
Living Water fields;
Purification fields;
death/respawn hooks.
```

---

## 5. User stories

```text
Como jogador, quero ver funções da Fonte que já desbloqueei.
Como jogador, não quero spoiler do final/Anya/nível 101.
Como jogador, quero confirmação para respec ou ação custosa.
Como sistema, quero UI chamando serviço, não gravando estado.
```

---

## 6. Escopo

```text
Fonte menu projection;
fragment-gated function list;
cost/resource display;
confirmation policy;
spoiler-safe labels;
future/locked function policy;
tests/validators.
```

---

## 7. Fora de escopo

```text
Fonte runtime full;
respec backend;
Living Water gameplay;
purification/cure mechanics;
final choice runtime;
scene/prefab/art final.
```

---

## 8. Regras de não duplicação

```text
Não criar FonteManager paralelo.
Não persistir Fonte state na UI.
Não revelar locked functions.
Não mostrar respec/purification/final before fragment gate.
Não explicar Anya completamente.
```

---

## 9. Critérios de aceite

- Function visibility follows fragment gates.
- Costly actions require confirmation.
- Hidden functions do not reveal spoilers.
- UI dispatches commands only.
- Report includes PlayMode scenario.
- No save schema change.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Fonte/FonteMenuController.cs
Assets/_Game/Scripts/UI/Fonte/FonteFunctionViewModel.cs
Assets/_Game/Scripts/UI/Fonte/FonteFunctionVisibilityPolicy.cs
Assets/_Game/Tests/EditMode/UI/FonteMenuProjectionTests.cs
```

Consolidar existentes se houver.

---

## 11. Contratos

### Runtime

```text
UI consumes Fonte/MainProgression projection.
UI dispatches Fonte action commands.
Services own state and effects.
```

### Save

```text
No save schema change.
```

### UI

```text
Locked/future functions are hidden or ??? without spoiler.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_fonte_menu_flow_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Fonte/**
Assets/_Game/Scripts/Progression/**
Assets/_Game/Scripts/Skills/**
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
.specs/SPEC_EXECUTION_ORDER.md
.specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 14. Estratégia

```text
1. Auditar Fonte/UI/progression existentes.
2. Consolidar function visibility policy.
3. Add confirmation policy for costly actions.
4. Add tests for fragment gates/spoilers.
5. Report.
```

---

## 15. Ordem segura

```text
Fonte/Main hooks -> Skill respec backend -> Fonte menu projection.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with Fonte runtime, respec, Living Water, purification or main progression changes.
- Reason: menu reflects central progression states.

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
Risco: spoiler.
Mitigação: fragment gate visibility tests.

Risco: UI mutar Fonte state.
Mitigação: command/service ownership.

Risco: action costly sem confirmação.
Mitigação: confirmation policy.
```

---

## 21. Rollback

```text
Reverter Fonte UI/view model/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar Fonte/UI/progression.
- [ ] T003 — Consolidar visibility policy.
- [ ] T004 — Implementar confirmation hardening.
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
| Spoiler/future | A spec respeita dados ocultos e features future? | Matriz de visibility/future no report. | PARTIAL |
| Save/load | A UI altera estado persistido ou só despacha comando? | Declaração de schema/no schema. | PARTIAL |
| Eventos | Subscriptions têm lifecycle? | Mapa de publishers/subscribers se houver. | PARTIAL |
| Testes | Há view model/projection determinística? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report criado? | `docs/validation/04_spec_ui_fonte_menu_flow_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar:

```bash
rg -n "Fonte|FonteAnya|MainProgression|Fragment|LivingWater|Água Viva|Respec|Purification|FinalChoice|Anya|Level101|Nível 101" Assets/_Game/Scripts docs/design .specs
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
Given a tela/flow de Fonte menu/function visibility é aberto com dependências válidas
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

### Scenario 3 — Empty / unavailable / blocked state

```text
Given não há dados disponíveis ou requisito está ausente
When a tela abre ou item/entry/função/dia é selecionado
Then a UI mostra empty state ou motivo bloqueado
And nenhuma ação inválida é executada
And o report registra o estado esperado.
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

- Respec visível antes do Fragmento da Memória.
- Purificação visível antes do Fragmento da Vida.
- Decisão final visível antes da Esperança.
- Anya explicada cedo demais.
- Nível 101 revelado cedo.
- Ação custosa sem confirmação.
- UI mutando Fonte state diretamente.
- Função future mostrada como promessa quebrada.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Fonte Menu Flow Runtime

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
6. A implementação revelar spoiler/future state proibido pelo direction.
7. Não for possível decidir se sistema existente é canônico ou obsoleto.
```


## 23G. Fonte Function Visibility Matrix

| Fonte state | Visible functions | Hidden |
|---|---|---|
| Adormecida / Respawn | Return/respawn info if known | Água Viva, Respec, Purification, Final |
| Fragmento da Água | Água Viva limited functions | Respec, Purification, Final |
| Fragmento da Memória | Água Viva + Respec | Purification, Final |
| Fragmento da Vida | Água Viva + Respec + Purification/advanced cure | Final |
| Fragmento da Esperança | All final-allowed functions | Only content not yet discovered |

## 23H. Fonte Confirmation Policy

```text
Respec: strong confirmation.
Purification/cure with scarce resource: confirmation if cost meaningful.
Final choice: strong confirmation and no accidental default confirm.
Living Water minor use: light confirmation or no confirmation depending cost.
```

## 23I. Spoiler Guardrails

```text
Do not name Anya's full truth early.
Do not mention level 101 before discovery.
Do not describe final choice before Esperança.
Do not expose locked fragment function names if design opts for hidden.
```


## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Social|NPC|Calendar|Fonte|Confirmation|EmptyState|BlockedState|Focus|Modal|Known|Hidden|Future" Assets/_Game/Scripts docs/design .specs
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

- Changed deterministic logic: YES if visibility/confirmation projection logic changes.
- Requires EditMode tests: YES for fragment gate/spoiler/confirmation projection tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for Fonte menu opening and function selection flow.
- Requires regression test: YES if fixing known Fonte spoiler/gate bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no Fonte runtime rewrite; no early spoiler.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_fonte_menu_flow_runtime_execution_report.md.
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
