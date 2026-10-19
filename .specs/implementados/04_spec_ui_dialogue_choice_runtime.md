# SPEC — UI Dialogue Choice Runtime

> **Spec ID:** `04_spec_ui_dialogue_choice_runtime`  
> **Status:** Implementado e ACCEPTED  
> **Evidência:** Código presente — `Assets/_Game/Scripts/UI/Dialogue/DialogueModal.cs`, `DialogueStateViewModel.cs`, `Assets/_Game/Scripts/UI/Input/InputFocusModalRoutingModel.cs` (foco de diálogo bloqueando gameplay input). Play Mode humano PASS 2026-08-13 (fluxo 6 do playtest — diálogo com NPC: abre, bloqueia movimento, avança falas, encerra e devolve controle — usuário confirmou OK).  
> **Revision:** EXPANDED_06_07  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P0  
> **Type:** Runtime / UI / Dialogue / Choice Modal / Input Focus  
> **Domain:** UI / Dialogue / Choices / Quest Acceptance / Confirmations  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere input focus/modal routing, quest triggers, NPC dialogue runtime, shop/service dialogue, player movement input ou confirmation modals.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Dialogue/**`, `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/04_spec_ui_dialogue_choice_runtime_execution_report.md`  
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
  - `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`
  - `.specs/a_implementar/04_spec_ui_input_focus_modal_routing_runtime.md`
> **Blocks:**  
  - quest dialogue triggers;
  - NPC service dialogue;
  - shop/service entry flow;
  - confirmation modal patterns;
> **Scope:** implementar/endurecer diálogo compacto com escolhas, foco modal, bloqueio de gameplay input e hooks seguros para aceitar quest/entregar item/iniciar serviço.  
> **Out of scope:** conteúdo final de diálogos, retratos/art final, romance/social runtime, NPC schedules, quest content authoring completo, cutscenes/timelines.

---

# /speckit.specify

## 1. Contexto

O direction de UI/UX define que diálogo deve ocupar apenas o necessário, com retrato/sprite se disponível, nome do NPC, texto, opções e indicadores de quest/social quando relevantes. Também define que diálogo não deve deixar WASD mover o personagem e deve ser navegável por teclado/mouse e gamepad futuro.

Esta spec cria a base runtime de Dialogue Modal/Choice UI, dependente do input/focus modal routing.

---

## 2. Problema

Sem contrato de diálogo:

```text
WASD pode mover o jogador durante fala;
Interact pode ativar o mundo atrás do diálogo;
opções podem disparar ação errada;
aceitar quest/entregar item pode acontecer sem confirmação;
diálogo pode ocupar tela inteira sem necessidade;
serviço/shop pode abrir sem foco claro.
```

---

## 3. Objetivo

Garantir diálogo compacto e seguro:

```text
DialogueFocus bloqueia gameplay input;
opções têm foco navegável;
choice action é explícita;
quest acceptance/delivery/service hooks são separados;
ações irreversíveis usam confirmação;
layout não ocupa tela inteira sem necessidade.
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
- docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Dialogue layout: retrato/sprite, nome NPC, fala, opções, indicadores de quest/social.
- Diálogo não deve ocupar tela inteira sem necessidade.
- Diálogo não deve deixar WASD mover personagem.
- Escolhas indicam ação especial: aceitar quest, entregar item, comprar/vender, iniciar ritual/Fonte etc.
- Escolhas irreversíveis/grandes exigem confirmação.

### Deferred / future from directions

- Conteúdo final de diálogos.
- Romance/social conversation trees.
- Gamepad final.
- Cutscenes/timelines.
- Retratos/art final.

### Explicitly not redefined here

- Quest runtime content.
- NPC schedule.
- Shop economy.
- Fonte progression.
- Social relationship runtime.

## 4. Estado atual do repo

```text
UI/dialogue podem existir parcialmente.
Input/modal focus foundation será consolidado pela spec 04_spec_ui_input_focus_modal_routing_runtime.
Esta spec deve auditar sistemas existentes antes de criar novos.
```

A confirmar localmente:

```text
DialogueController;
DialogueUI;
DialogueChoiceView;
Quest acceptance hooks;
NPC/service interaction hooks;
ModalManager/GameplayInputRouter.
```

---

## 5. User stories

```text
Como jogador, quero ler fala e escolher opção sem mover o personagem.
Como jogador, quero saber quando uma escolha aceita quest, entrega item ou abre loja.
Como UI, quero foco claro na opção atual.
Como quest system, quero hook explícito de choice, sem acoplamento visual.
```

---

## 6. Escopo

```text
Dialogue modal state;
choice list/focus;
confirm/cancel behavior;
special-action markers;
quest/service hook contracts;
compact layout rules;
execution report.
```

---

## 7. Fora de escopo

```text
Dialogue content authoring;
NPC schedules;
romance/social logic;
shop screen itself;
cutscenes;
voice/audio final.
```

---

## 8. Regras de não duplicação

```text
Não criar input router paralelo.
Não criar quest state dentro do diálogo.
Não fazer dialogue UI aplicar reward diretamente.
Não usar dialogue choice para burlar confirmation rules.
```

---

## 9. Critérios de aceite

- DialogueFocus bloqueia gameplay input.
- Choice navigation funciona sem mundo receber input.
- Special choices indicam ação.
- Confirmation exigida para ações irreversíveis/custosas.
- Quest/service hooks são contratos separados.
- Report inclui cenário final humano deferido.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Dialogue/DialogueModalController.cs
Assets/_Game/Scripts/UI/Dialogue/DialogueChoiceViewModel.cs
Assets/_Game/Scripts/UI/Dialogue/DialogueChoiceAction.cs
Assets/_Game/Tests/EditMode/UI/DialogueChoiceRoutingTests.cs
```

Consolidar existentes se houver.

---

## 11. Contratos

### Runtime

```text
Dialogue modal requests DialogueFocus.
Choices expose action type and payload.
UI does not own quest/service state.
```

### Events

```text
Choice selected may publish/dispatch a domain command.
Subscribers must unsubscribe if event-based.
```

### Save

```text
No save schema change.
```

### UI

```text
Compact panel; focused option visible; cancel/back clear.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/Dialogue/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_dialogue_choice_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/NPC/**
Assets/_Game/Scripts/City/**
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
1. Auditar diálogo/UI/input existentes.
2. Integrar com focus/modal foundation.
3. Criar/ajustar choice view model e action contract.
4. Criar tests para focus/confirm/cancel quando praticável.
5. Criar report.
```

---

## 15. Ordem segura

```text
Input focus -> Dialogue modal -> Quest/NPC service integration.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with input focus modal, quest trigger or NPC dialogue runtime changes.
- Reason: diálogo cruza input, UI, quest e serviços.

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
Adds events: CONDITIONAL for dialogue choice command if absent.
Changes existing events: SHOULD BE NO.
Requires unsubscribe pattern: YES if event subscribers touched.
```

---

## 19. Impacto UI/Unity

```text
Changes UI: YES dialogue behavior/view model.
Changes scenes/prefabs/assets: NO.
Requires PlayMode/final human scenario: YES, DEFERRED.
```

---

## 20. Riscos

```text
Risco: ação especial sem confirmação.
Mitigação: action type + confirmation policy.

Risco: diálogo aplicar estado de domínio.
Mitigação: UI dispatches command only.
```

---

## 21. Rollback

```text
Reverter dialogue modal/action/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar diálogo e input existentes.
- [ ] T003 — Consolidar DialogueFocus/choice action.
- [ ] T004 — Implementar hardening mínimo.
- [ ] T005 — Criar tests.
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
| Report | Execution report foi criado? | `docs/validation/04_spec_ui_dialogue_choice_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Dialogue|DialogueChoice|ChoiceAction|DialogueFocus|QuestAccept|QuestTurnIn|Service|Confirmation|NPC" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a dialogue choice UI existe ou foi criado de forma mínima
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

- Choice selecionada dispara ação irreversível sem confirmação.
- Dialogue UI aplica reward diretamente.
- WASD move durante fala.
- Interact confirma choice e interage com mundo simultaneamente.
- Choice de quest não respeita visibility/spoiler.
- Choice de serviço abre shop sem trocar focus.
- Cancel/back perde estado de diálogo sem política clara.
- Quest turn-in aceita item errado ou parcial sem feedback.

---

## 23E. Minimum Execution Report Template

O execution report desta spec deve conter, no mínimo:

```md
# Execution Report — UI Dialogue Choice Runtime

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


## 23G. Dialogue Choice Action Matrix

| ActionType | Exige confirmação | Domain owner | Observação |
|---|---:|---|---|
| Continue | NO | Dialogue | Avança fala. |
| Close | NO | Dialogue/UI | Fecha e restaura foco. |
| AcceptQuest | SHOULD | Quest | Mostrar nome/resumo sem spoiler. |
| TurnInQuestItem | YES | Quest/Inventory | Validar item/quantidade. |
| OpenShop | NO | Shop/UI | Troca para ShopFocus. |
| StartService | CONDITIONAL | Service/NPC | Custo/risco exige confirmação. |
| SpendGold | YES | Economy | Mostrar custo total. |
| UseFonte | YES | Fonte/Future | Evitar spoiler. |
| MajorChoice | YES | Quest/MainProgression | Registrar escolha e irreversibilidade. |

## 23H. Dialogue Layout Requirements

```text
Nome do NPC visível.
Texto atual legível.
Opção focada destacada.
Ações especiais marcadas com ícone/texto curto.
Botão de back/cancel previsível.
Sem bloquear toda tela salvo evento especial.
```

## 25. Validações obrigatórias

Docs:

```powershell
.\tools\docs\validate_docs.ps1
```

Busca local mínima:

```bash
rg -n "Dialogue|Inventory|Equipment|Tooltip|Shop|Buy|Sell|Modal|Focus|Confirm|ItemDetail|Compare|Stock|Price" Assets/_Game/Scripts docs/design .specs
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

- Changed deterministic logic: YES if choice routing/confirmation logic changes.
- Requires EditMode tests: YES for view model/choice action logic.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for dialogue blocking player movement and choice selection flow.
- Requires regression test: YES if fixing known WASD/dialogue input leak; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; PlayMode scenario documented; no domain state hidden in UI.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_dialogue_choice_runtime_execution_report.md.
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
