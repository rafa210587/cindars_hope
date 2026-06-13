# SPEC — UI Quest Log Screen Runtime

> **Spec ID:** `04_spec_ui_quest_log_screen_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P0  
> **Type:** Runtime / UI / Quest Log / Spoiler-safe Projection  
> **Domain:** UI / Quest Log / Quest Detail / Categories / Visibility  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere QuestState, quest visibility/spoiler policy, QuestFlags, MainProgression, calendar/lunar quest conditions or UI input focus.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/04_spec_ui_quest_log_screen_runtime_execution_report.md`  
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
  - `.specs/a_implementar/03_spec_quest_log_visibility_spoiler_runtime.md`
  - `.specs/a_implementar/04_spec_ui_input_focus_modal_routing_runtime.md`
> **Blocks:**  
  - quest visibility/spoiler runtime;
  - quest state save/load;
  - main progression UI;
  - calendar day detail;
  - farm orders UI;
> **Scope:** consolidar Quest Log Screen com categorias, detail drawer, estados, prazos e spoiler-safe projection.  
> **Out of scope:** quest runtime backend, quest content, map markers final, localization, main quest cinematics, final art/prefab layout.

---

# /speckit.specify

## 1. Contexto

O direction de menu define Quest Log Screen para organizar objetivos sem revelar spoilers. Deve ter categorias Main Quest, Side Quest, Social Quest, Farm Order/Encomenda, Companion Quest, Pet/Farm Hint, Completed e Failed/Expired.

O direction de Quest define visibility policy: objetivos ocultos, rewards secretos, boss/branch invisível, condição lunar não descoberta e final/nível 101 não podem aparecer cedo.

---

## 2. Problema

Sem Quest Log UI contract:

```text
Quest Log pode revelar objetivo oculto;
main quest pode revelar Anya/final/nível 101 cedo;
farm order pode não mostrar prazo;
waiting condition pode parecer bug;
completed/expired pode sumir sem histórico;
quest state pode ser editado pela UI.
```

---

## 3. Objetivo

Criar/endurecer Quest Log Screen:

```text
categories;
quest list;
detail drawer;
current objective;
known hint/location/NPC/item;
deadline if known;
condition state if discovered;
history of completed steps;
reward known/unknown;
strict spoiler-safe projection.
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

- Quest Log organiza objetivos sem revelar spoilers.
- Categorias incluem Main Quest, Side Quest, Social Quest, Farm Order, Companion Quest, Pet/Farm Hint, Completed, Failed/Expired.
- Quest detail drawer mostra título, categoria, resumo, objetivo atual, local/pista, NPC, item, estado, recompensa conhecida/desconhecida, prazo, condição temporal descoberta e histórico.
- Main quest mostra ato atual, fragmento se descoberto, estado da Fonte se conhecido e não revela nível 101/Anya completa cedo.
- Quest condition states incluem WaitingForTime, WaitingForWeather, WaitingForLunarEvent, WaitingForNPC, WaitingForItem, WaitingForCaveDepth.
- Visibility policy evita spoiler.

### Deferred / future from directions

- Map markers final.
- Quest content final.
- Localization/text polish.
- Main quest cinematic UI.
- Companion/social full integration.

### Explicitly not redefined here

- Quest runtime state.
- Quest visibility backend.
- Calendar/weather/lunar runtime.
- MainProgression/Fonte state.
- Reward/idempotency.

## 4. Estado atual do repo

```text
Quest Log visibility/spoiler spec já foi gerada na WAVE 03.
Esta spec é a UI projection da Quest Log, não o backend de quest.
```

A confirmar localmente:

```text
QuestLogUI;
QuestState runtime;
QuestVisibilityService;
QuestFlag registry;
Calendar/lunar condition projection;
MainProgression/Fonte UI hooks.
```

---

## 5. User stories

```text
Como jogador, quero saber objetivo atual sem spoiler.
Como jogador, quero distinguir main, side, order e completed.
Como jogador, quero saber quando esperar tempo/clima/lua se descoberto.
Como jogador, quero ver histórico breve sem revelar branches ocultas.
```

---

## 6. Escopo

```text
Quest Log screen/view model;
categories;
quest list/detail drawer;
condition state projection;
deadline/prior hints;
reward known/unknown;
spoiler-safe projection;
tests/validators.
```

---

## 7. Fora de escopo

```text
quest backend;
quest content;
map marker system;
calendar UI full;
localization;
visual prefab final.
```

---

## 8. Regras de não duplicação

```text
Não criar QuestState dentro da UI.
Não revelar hidden objectives/rewards.
Não usar QuestFlag como projection completa.
Não alterar quest progression pelo Quest Log.
Não mostrar Anya/final/nível 101 cedo.
```

---

## 9. Critérios de aceite

- Categories match direction.
- Detail drawer safe and complete.
- Hidden data not shown.
- Waiting states shown only when discovered.
- Completed/Expired visible in categories.
- Report includes PlayMode scenario.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Quests/QuestLogScreenController.cs
Assets/_Game/Scripts/UI/Quests/QuestLogViewModel.cs
Assets/_Game/Scripts/UI/Quests/QuestDetailDrawerViewModel.cs
Assets/_Game/Tests/EditMode/UI/QuestLogProjectionTests.cs
```

Consolidar existentes se houver.

---

## 11. Contratos

### Runtime

```text
UI consumes QuestVisibilityProjection.
UI does not mutate QuestState.
```

### Save

```text
No save schema change.
```

### UI

```text
Detail drawer updates with selection.
Known/unknown reward uses explicit label.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_quest_log_screen_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Fonte/**
Assets/_Game/Scripts/Progression/**
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
1. Auditar QuestLog UI/runtime.
2. Consolidar visibility projection view model.
3. Add spoiler-safe tests.
4. Add category/detail tests.
5. Report PlayMode scenarios.
```

---

## 15. Ordem segura

```text
Quest visibility runtime -> Quest Log UI -> Calendar/MainProgression details.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with quest runtime/visibility/backend or calendar UI changes.
- Reason: shared projection and spoiler rules.

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
Mitigação: visibility projection tests.

Risco: UI mutar state.
Mitigação: read-only projection.

Risco: waiting condition parecer bug.
Mitigação: explicit known state.
```

---

## 21. Rollback

```text
Reverter quest log UI/view model/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar QuestLog UI/runtime.
- [ ] T003 — Consolidar category/detail projection.
- [ ] T004 — Implementar spoiler-safe hardening.
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
| Report | Execution report criado? | `docs/validation/04_spec_ui_quest_log_screen_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar:

```bash
rg -n "QuestLog|QuestDetail|QuestVisibility|QuestState|QuestFlag|MainQuest|FarmOrder|WaitingForTime|WaitingForWeather|WaitingForLunarEvent|Hidden|Expired" Assets/_Game/Scripts docs/design .specs
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
Given a tela/flow de quest log/spoiler-safe projection é aberto com dependências válidas
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

- Hidden objective revelado.
- Secret reward revelado.
- Anya/final/nível 101 revelado cedo.
- Waiting condition oculta mostrada como requisito exato.
- Completed/Expired sumindo sem histórico.
- UI alterando QuestState.
- FarmOrder sem prazo visível quando conhecido.
- Main quest sem ato/fragmento conhecido quando descoberto.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Quest Log Screen Runtime

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


## 23G. Quest Log Category Matrix

| Category | Shows | Notes |
|---|---|---|
| Main Quest | Active main progression visible to player | No final/101 spoiler |
| Side Quest | Active side quests | Visibility policy applies |
| Social Quest | NPC known quests | No hidden relationship spoilers |
| Farm Order | Orders/encomendas | Deadline if known |
| Companion Quest | Future | Hidden unless unlocked |
| Pet/Farm Hint | Future/hints | No hard quest state unless defined |
| Completed | Completed known quests | Brief history |
| Failed/Expired | Failed/expired known quests | Reason if known |

## 23H. Spoiler Projection Rules

```text
Never show hidden objective text.
Never show secret reward before discovery.
Never show exact lunar/weather condition until discovered.
Never show boss/final/level 101 early.
Show "Unknown reward" instead of reward if hidden.
Show "Wait for the right condition" only when clue discovered.
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

- Changed deterministic logic: YES if quest log projection/visibility logic changes.
- Requires EditMode tests: YES for spoiler-safe projection tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for Quest Log navigation/detail visibility scenario.
- Requires regression test: YES if fixing known spoiler/quest log bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode projection tests PASS or NOT RUN justified; no hidden spoiler; UI read-only.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_quest_log_screen_runtime_execution_report.md.
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
