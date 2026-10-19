# SPEC — UI Calendar Day Detail Runtime

> **Spec ID:** `04_spec_ui_calendar_day_detail_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 04 — UI / UX Foundation  
> **Priority:** P1  
> **Type:** Runtime / UI / Calendar / Day Detail / Spoiler-safe Projection  
> **Domain:** UI / Calendar / Day Detail / Weather / Lunar / Festivals / Orders  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_04_UI_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere calendar runtime, weather/lunar/festival generation, quest time conditions, farm orders expiry, shop schedule, or calendar save state.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Quests/**`, `Assets/_Game/Tests/EditMode/UI/**`, `docs/validation/04_spec_ui_calendar_day_detail_runtime_execution_report.md`  
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
  - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
  - `.specs/a_implementar/02_spec_calendar_ui_weather_lunar_display.md`
  - `.specs/a_implementar/04_spec_ui_input_focus_modal_routing_runtime.md`
> **Blocks:**  
  - calendar/weather/lunar runtime;
  - quest log waiting conditions;
  - farm order deadlines;
  - shop schedule/open closed display;
> **Scope:** consolidar Calendar Day Detail UI com dia/estação/ano/clima/previsão/eventos lunares/festivais/orders/quests conhecidos sem revelar segredos.  
> **Out of scope:** calendar runtime backend, weather generation, lunar event mechanics, festival minigames, birthdays/social full integration, visual prefab final.

---

# /speckit.specify

## 1. Contexto

O direction de menu define Calendar overview e Day Detail: dia atual, estação, ano, semana, clima atual, previsão conhecida, evento lunar conhecido, festivais, orders/encomendas com prazo, aniversários futuros se existirem, eventos de quest conhecidos e lojas fechadas/abertas por evento conhecido.

Ele também define spoiler control: não revelar evento secreto, Nyx oculto antes da descoberta, condição exata de Mana cedo ou evento do nível 101 cedo.

Esta spec é a versão UI detalhada da calendar projection, complementando specs de runtime/calendário já geradas.

---

## 2. Problema

Sem Calendar Day Detail seguro:

```text
calendar pode revelar evento secreto;
condição lunar pode aparecer cedo demais;
order vencendo pode não aparecer;
festival conhecido pode não mostrar impacto;
shop fechado/aberto pode parecer bug;
quest waiting condition pode ser invisível;
calendar UI pode duplicar lógica de world/calendar runtime.
```

---

## 3. Objetivo

Criar/endurecer Calendar Day Detail:

```text
overview de dia/estação/ano/semana;
day detail drawer;
weather/current/tomorrow when known;
known lunar event;
known festival;
orders vencendo;
known quest time/weather/lunar waits;
shop open/closed if known;
strict spoiler policy.
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
- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável com escopo, locks, validações e quality gate.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Calendar overview mostra dia, estação, ano, semana, clima atual, previsão, evento lunar conhecido, festivais, orders, aniversários futuros, eventos de quest e lojas abertas/fechadas.
- Day detail mostra data, estação, clima previsto, evento lunar conhecido, festival, NPC/evento associado, orders vencendo, quest esperando condição temporal e observações descobertas.
- Spoiler control: não revelar evento secreto, Nyx oculto, condição exata de Mana cedo ou nível 101.
- Focus order: Month/season grid -> Day detail -> Event list -> Close.
- Calendar não revela segredo antes da descoberta.

### Deferred / future from directions

- Birthdays/social full runtime.
- Festival minigames.
- Shop schedules finais.
- Map markers.
- Calendar art/prefab final.

### Explicitly not redefined here

- Calendar runtime.
- Weather generation.
- Lunar event mechanics.
- Quest condition backend.
- Farm order expiry backend.

## 4. Estado atual do repo

```text
Batch 03 já gerou calendar/weather/lunar display spec.
Esta spec refina Day Detail e focus/empty/spoiler UI.
```

A confirmar localmente:

```text
CalendarUI;
CalendarViewModel;
Weather forecast data;
Lunar visibility state;
FestivalState;
FarmOrder deadlines;
Quest waiting conditions;
Shop open/closed state.
```

---

## 5. User stories

```text
Como jogador, quero ver o que sei sobre um dia específico.
Como jogador, quero saber se uma order vence em breve.
Como jogador, quero ver festival conhecido e clima previsto.
Como jogador, não quero receber spoiler de evento secreto.
```

---

## 6. Escopo

```text
calendar grid/day detail projection;
known events;
orders/deadlines;
weather/lunar/festival known visibility;
shop open/closed projection if known;
quest waiting condition projection;
tests/validators.
```

---

## 7. Fora de escopo

```text
calendar backend rewrite;
festival gameplay;
social birthdays runtime;
shop schedule backend;
weather/lunar generation;
visual prefab final.
```

---

## 8. Regras de não duplicação

```text
Não criar calendar runtime paralelo.
Não calcular weather/lunar na UI.
Não revelar hidden events.
Não prometer birthdays/social se runtime não existir.
Não usar calendar UI como source of truth para quest/farm/shop.
```

---

## 9. Critérios de aceite

- Day detail mostra apenas known data.
- Hidden/secret events not shown.
- Orders vencendo aparecem when known.
- Waiting conditions shown without exact spoiler when not discovered.
- Shop open/closed only if known/reliable.
- Report inclui PlayMode scenario.

---

# /speckit.plan

## 10. Arquitetura alvo

```text
Assets/_Game/Scripts/UI/Calendar/CalendarScreenController.cs
Assets/_Game/Scripts/UI/Calendar/CalendarDayDetailViewModel.cs
Assets/_Game/Scripts/UI/Calendar/CalendarEventVisibilityPolicy.cs
Assets/_Game/Tests/EditMode/UI/CalendarDayDetailProjectionTests.cs
```

Consolidar existentes se houver.

---

## 11. Contratos

### Runtime

```text
UI consumes Calendar/World/Quest projections.
UI does not generate weather/lunar/festival events.
```

### Save

```text
No save schema change.
```

### UI

```text
Unknown/hidden events remain hidden.
```

---

## 12. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/04_spec_ui_calendar_day_detail_runtime_execution_report.md
```

Leitura permitida:

```text
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Quests/**
Assets/_Game/Scripts/Economy/**
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
1. Auditar calendar UI/runtime.
2. Consolidar day detail projection.
3. Add visibility policy tests.
4. Report PlayMode scenario.
```

---

## 15. Ordem segura

```text
Calendar runtime -> calendar display -> day detail projection -> quest/order integration.
```

---

## 16. Paralelização

- Parallelizable: NO
- Must not run with calendar/weather/lunar/festival runtime changes.
- Reason: UI projection depends on those states.

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
Mitigação: visibility policy tests.

Risco: duplicated calendar logic.
Mitigação: UI consumes projection only.
```

---

## 21. Rollback

```text
Reverter calendar UI/view model/tests/report.
```

---

# /speckit.tasks

## 22. Tasks

- [ ] T001 — Ler fontes.
- [ ] T002 — Auditar calendar UI/runtime.
- [ ] T003 — Consolidar day detail projection.
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
| Spoiler/future | A spec respeita dados ocultos e features future? | Matriz de visibility/future no report. | PARTIAL |
| Save/load | A UI altera estado persistido ou só despacha comando? | Declaração de schema/no schema. | PARTIAL |
| Eventos | Subscriptions têm lifecycle? | Mapa de publishers/subscribers se houver. | PARTIAL |
| Testes | Há view model/projection determinística? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report criado? | `docs/validation/04_spec_ui_calendar_day_detail_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar:

```bash
rg -n "Calendar|DayDetail|Weather|TomorrowWeather|Lunar|Festival|FarmOrder|Deadline|Birthday|ShopClosed|QuestWaiting|HiddenEvent|Nyx|Level101" Assets/_Game/Scripts docs/design .specs
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
Given a tela/flow de calendar day detail UI é aberto com dependências válidas
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

- Evento secreto revelado.
- Nyx oculto revelado antes da descoberta.
- Condição exata de Mana/nível 101 revelada cedo.
- Order vencendo omitida.
- Festival conhecido sem data/efeito visível.
- Shop open/closed mostrado sem dado confiável.
- UI gera weather/lunar em vez de ler projection.
- Quest waiting condition aparece como bug/sem explicação.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — UI Calendar Day Detail Runtime

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


## 23G. Calendar Day Detail Field Matrix

| Field | Show when | Hidden behavior |
|---|---|---|
| Date/season/year/week | Always | N/A |
| Current weather | Known/current | N/A |
| Forecast | Forecast known | Hidden/unknown |
| Lunar event | Discovered/known | Hidden or vague clue |
| Festival | Known festival | Hidden if secret |
| Orders expiring | Known active orders | N/A |
| Quest waiting condition | Condition discovered | Generic hint if partial |
| NPC birthday/social | Future/runtime exists | Hidden/future |
| Shop open/closed | Known schedule/event | Unknown/not shown |
| Level 101/final event | Never early | Hidden |

## 23H. Spoiler States

```text
KnownExact
  Full detail can be shown.

KnownPartial
  Hint can be shown, exact condition hidden.

UnknownHidden
  Do not show.

FutureNotImplemented
  Do not show in runtime final, debug only if needed.
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

- Changed deterministic logic: YES if projection/visibility logic changes.
- Requires EditMode tests: YES for day detail and spoiler projection tests.
- Requires PlayMode automated or final human scenario: YES, DEFERRED for calendar navigation/day detail flow.
- Requires regression test: YES if fixing known calendar spoiler/deadline bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION se houver cenário integrado; caso contrário NOT REQUIRED.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; EditMode tests PASS or NOT RUN justified; no hidden event spoiler; no calendar backend rewrite.

---

## 27. Definition of Done

```text
Spec executada sem alterar arquivos proibidos.
Contratos/data/runtime implementados apenas dentro do escopo.
Execution report criado em docs/validation/04_spec_ui_calendar_day_detail_runtime_execution_report.md.
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
