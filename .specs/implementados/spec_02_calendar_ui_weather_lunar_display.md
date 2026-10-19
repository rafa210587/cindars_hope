# SPEC — Calendar UI / Weather / Lunar Display

> **Spec ID:** `02_spec_calendar_ui_weather_lunar_display`  
> **Status:** A implementar  
> **Revision:** EXPANDED_01_05_CORRECTED  
> **Wave:** WAVE 02 — Time / Calendar / Weather / Lunar  
> **Priority:** P1  
> **Type:** UI / Runtime Adapter / Validation / Hardening  
> **Domain:** Calendar UI / Weather Forecast / Lunar / Festivals  
> **Parallelizable:** CONDITIONAL  
> **Parallel group:** WAVE_02_UI_AFTER_RUNTIME  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere modal/input routing global, calendar runtime, weather/lunar runtime, quest log visibility ou UI foundation sem lock explícito.  
> **Repo lock scope:** `Assets/_Game/Scripts/UI/**`, `Assets/_Game/Scripts/Calendar/**`, `Assets/_Game/Scripts/Weather/**`, `Assets/_Game/Scripts/Lunar/**`, `Assets/_Game/Scripts/Core/UI/**`, `docs/validation/02_spec_calendar_ui_weather_lunar_display_execution_report.md`.  
> **Depends on:**  
- `.specs/a_implementar/04_spec_ui_input_focus_modal_routing_runtime.md or existing UI modal/input stack audit`
- `.specs/a_implementar/02_spec_time_clock_day_transition_runtime.md`
- `.specs/a_implementar/02_spec_calendar_season_year_runtime.md`
- `.specs/a_implementar/02_spec_weather_generation_forecast_runtime.md`
- `.specs/a_implementar/02_spec_lunar_cycle_event_runtime.md`
- `.specs/a_implementar/02_spec_calendar_festivals_events_runtime.md`
- `.specs/a_implementar/02_spec_time_calendar_weather_lunar_save_state.md`
> **Blocks:**  
- `quest log temporal visibility integration`
- `festival quest UI`
- `NPC schedule availability display`
- `shop restock/calendar display`
- `farm seasonal planning UI`
> **Scope:** implementar/adaptar a tela de calendário para mostrar dia, estação, clima atual/previsão, eventos lunares conhecidos e festivais/eventos conhecidos, respeitando anti-spoiler e input modal.  
> **Out of scope:** criar runtime de tempo/clima/lua/festival, criar social birthdays completos, criar quest log completo, criar gamepad final, criar minigames de festival.

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

- docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
- docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
- docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md

### Required interpretation

```text
Esta spec é derivada dos directions/refinements canônicos e do roadmap macro.
Ela não substitui os directions.
Ela transforma parte do refinement em contrato implementável, com escopo, locks, validações e quality gate.
Quando houver divergência entre esta spec e os directions, o executor deve parar e registrar CONFLICT no execution report.
```

---

## Direction / Refinement Coverage

### Covered from directions

- Tempo, calendário, estação, clima, chuva, lua, festival, UI de calendário ou persistência desses estados conforme escopo da spec.
- Regra de visibilidade: eventos conhecidos aparecem; eventos secretos não são revelados cedo.
- Save/load e restore order para world state quando aplicável.

### Deferred / future from directions

- Minigames de festival.
- Clima visual final, VFX/SFX e assets.
- NPC schedules completos, aniversários/social completo e balance final de clima.

### Explicitly not redefined here

- Farm crop growth completo.
- Quest runtime completo.
- UI visual/prefab final.
- Sistema social/romance/pets/companions.

---

# /speckit.specify

## 1. Contexto

O direction de UI/UX define que Calendar deve suportar dia, estação, clima atual/previsão quando desbloqueado, festivais, restock relevante, orders/encomendas, eventos lunares e eventos de quest quando conhecidos, sem revelar eventos secretos antes da descoberta. O direction de Menu Screen Flows define que Calendar é tela modal completa e deve seguir regras de foco, input e drawer.

Esta spec transforma a camada de apresentação do calendário em spec implementável, consumindo runtime/save já definidos na WAVE 02.

---

## 2. Problema

Sem UI de calendário clara, o jogador não consegue planejar crops, eventos, festivais, clima, lua, restock e quests temporais. Sem anti-spoiler, a UI pode revelar Nyx, eventos ocultos, quests secretas, festival hidden ou condições lunares antes da descoberta.

Também há risco de modal calendar deixar input de gameplay vazar, já observado em outros menus.

---

## 3. Objetivo

Criar ou endurecer Calendar UI como tela modal com foco explícito, day detail drawer, weather/lunar/festival visibility policies, empty states, forecast known/unknown states e integração com runtime sem salvar UI state.

---

## 4. Source Map Compliance

### 4.1 Fontes globais lidas

- `docs/design/SPEC_SOURCE_MAP.md`
- `docs/design/SPECIFICATION_PROCESS.md`
- `docs/design/lore/VAALARA_GAME_CANON_DIRECTION_v1.0.md`
- `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
- `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
- `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`
- `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
- `docs/project/CURRENT_STATE.md`
- `docs/IMPLEMENTATION_STATUS.md`

### 4.2 Directions de domínio lidos

- `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
- `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
- `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
- `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
- `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`

## Direction / Refinement Coverage

### 4.3 Covered from directions

```text
Calendar modal com input de gameplay bloqueado.
Exibição de day, season, current weather e tomorrow forecast quando disponível.
Exibição de festivals/calendar events conhecidos.
Exibição de lunar events conhecidos sem revelar ocultos.
Day detail drawer com eventos conhecidos do dia.
Empty/unknown states explícitos para forecast/lunar/festival.
UI state não persistido como gameplay state.
```

### 4.4 Deferred / future from directions

```text
Social birthdays completos.
Map markers avançados de quest.
Gamepad navigation final.
Festival minigames UI.
Bestiary/research UI.
Fonte UI específica.
```

### 4.5 Explicitly not redefined here

```text
Não redefine calendar/weather/lunar runtime.
Não redefine quest log como fonte de verdade.
Não redefine modal/input stack global; consome a stack existente.
Não redefine save/load.
```

---

## 5. Estado atual do repo

```text
UI/UX direction centraliza foco, modal, input routing e calendar display.
Menu screen direction define Calendar como tela modal completa e day detail drawer.
World/Time direction define calendário, clima, lua e festivais.
Batch anterior gerou specs runtime de calendar/weather/lunar/festival.
Calendar UI não deve ser executada antes do runtime mínimo ou adapter seguro existir.
```

---

## 6. User stories / engineering stories

```text
Como jogador, quero ver dia/estação/clima para planejar fazenda.
Como jogador, quero ver previsão quando desbloqueada sem receber spoiler.
Como jogador, quero ver festivais conhecidos com data/horário.
Como jogador, quero ver eventos lunares conhecidos, sem revelar Nyx/secretos cedo.
Como executor, quero Calendar UI que não vaze input de gameplay.
```

---

## 7. Escopo

```text
Auditar UI modal/input stack existente.
Criar/adaptar Calendar screen.
Criar day detail drawer.
Consumir calendar/weather/lunar/festival runtime por adapters.
Aplicar visibility policy para secrets/known events.
Implementar empty/unknown states.
Criar execution report.
```

---

## 8. Fora de escopo

```text
Não implementar calendar runtime.
Não implementar weather/lunar generation.
Não implementar quest log completo.
Não implementar social birthdays se social runtime não existir.
Não implementar festival minigames.
Não alterar save schema.
```

---

## 9. Regras de não duplicação

```text
Não criar segundo input router/modal manager.
Não salvar UI state no GameSaveData.
Não revelar hidden quest/lunar/festival.
Não duplicar quest log dentro do calendar.
Não depender de debug HUD para comunicar estado.
```

---

## 10. Critérios de aceite

### 10.1 Calendar UI funcional

- Calendar abre como modal e bloqueia input de gameplay.
- ESC/B/Cancel fecha ou volta camada.
- Day/season/current weather/tomorrow forecast aparecem quando runtime fornece dados.
- Estados desconhecidos aparecem como unknown/locked, não vazios bugados.

### 10.2 Visibility policy

- Eventos ocultos não aparecem antes de descobertos.
- Lunar events conhecidos podem aparecer.
- Festival conhecido mostra data/horário.
- Quest temporal conhecida pode aparecer como pista sem revelar spoiler.

### 10.3 Day detail drawer

- Selecionar dia mostra eventos conhecidos, clima/previsão conhecida, festival, lunar event e hooks conhecidos.
- Texto longo tem scroll ou layout adequado.
- Drawer não cobre lista principal desnecessariamente.

### 10.4 Evidence

- Execution report criado em `docs/validation/02_spec_calendar_ui_weather_lunar_display_execution_report.md`.
- PlayMode automated ou cenário final humano documentado se UI/cena for alterada.

---

# /speckit.plan

## 11. Arquitetura alvo

Possíveis alvos:
```text
Assets/_Game/Scripts/UI/Calendar/CalendarScreenController.cs
Assets/_Game/Scripts/UI/Calendar/CalendarDayCellView.cs
Assets/_Game/Scripts/UI/Calendar/CalendarDayDetailDrawer.cs
Assets/_Game/Scripts/UI/Calendar/CalendarVisibilityPresenter.cs
Assets/_Game/Tests/EditMode/UI/Calendar/CalendarVisibilityTests.cs
docs/validation/02_spec_calendar_ui_weather_lunar_display_execution_report.md
```

A UI deve consumir runtime/adapters, não calcular regras primárias.

---

## 12. Contratos, dados e eventos

### Data contracts

Calendar UI recebe view models derivados do runtime. UI não é fonte de verdade.

### Runtime contracts

Runtime fornece day/season/weather/lunar/festival state. Calendar UI não gera clima/lua.

### Event contracts

UI pode ouvir change events e atualizar view; deve unsubscribe ao fechar/destroy.

### Save contracts

UI state transitório não entra em GameSaveData.

### UI contracts

Modal focus, input blocking, drawer, tooltip/unknown state e accessibility básica seguem UI directions.

---

## 13. Sistemas afetados

```text
Calendar UI
Modal/input routing
Weather forecast presenter
Lunar event visibility presenter
Festival/calendar event presenter
Quest temporal hint adapter
```

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/Core/UI/**
Assets/_Game/Tests/EditMode/UI/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/02_spec_calendar_ui_weather_lunar_display_execution_report.md
```

---

## 15. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity unless explicitly required for UI prefab wiring and reported
Assets/**/*.prefab unless UI prefab wiring is explicitly required and reported
Assets/_Game/Scripts/Save/** except read-only
.specs/SPEC_EXECUTION_ORDER.md
PROJECT_LOG.md
```

---

## 16. Estratégia de implementação

### Fase 0 — Auditoria local

```bash
rg -n "Calendar|Weather|Lunar|Festival|Modal|InputRouter|Focus|QuestLog" Assets/_Game/Scripts
rg -n "Calendar" Assets/_Game
```

### Fase 1 — Adapter/view model

Criar/adaptar view models para day cells e detail drawer.

### Fase 2 — UI e visibility

Implementar known/unknown/hidden states. Bloquear input de gameplay quando modal aberto.

### Fase 3 — Validation/report

Testar visibility logic em EditMode quando possível; UI/cena em PlayMode/final human scenario.

---

## 17. Ordem segura de execução

```text
Ler fontes obrigatórias.
Confirmar runtime calendar/weather/lunar/festival existe ou marcar dependency blocked.
Auditar modal/input stack.
Implementar Calendar UI adapter/view/presenter.
Aplicar visibility/anti-spoiler.
Rodar validações.
Criar execution report.
```

---

## 18. Paralelização

- Parallelizable: CONDITIONAL
- Parallel group: WAVE_02_UI_AFTER_RUNTIME
- Can run with:
  - N/A
- Must not run with:
  - qualquer spec que altere modal/input routing global, calendar runtime, weather/lunar runtime, quest log visibility ou UI foundation sem lock explícito.
- Reason:
  - Pode rodar depois do runtime, mas não junto de modal/input global ou calendar runtime.

---

## 19. Impacto em save/load

```text
Does this change save schema? NO.
Does this add save section? NO.
Does this persist UI state? MUST BE NO.
```

---

## 20. Impacto em eventos

```text
Adds events: NO by default.
Requires unsubscribe pattern: YES if subscribing to runtime events.
Events are UI refresh signals only.
```

---

## 21. Impacto em UI/Unity

```text
Changes UI: YES.
Changes scenes/prefabs: CONDITIONAL if Calendar prefab/wiring is needed.
Requires PlayMode automated or final human scenario: YES.
Human validation timing: DEFERRED_TO_FINAL_VALIDATION.
```

---

## 22. Riscos técnicos

```text
Risco: revelar eventos ocultos. Mitigação: visibility policy.
Risco: gameplay input vazar. Mitigação: modal focus/input router.
Risco: UI recalcular regra incorreta. Mitigação: consumir runtime view model.
Risco: depender de prefab local. Mitigação: reportar wiring e final validation.
```

---

## 23. Rollback

```text
Remover controllers/views/adapters criados.
Reverter prefab/scene wiring se houver.
Remover execution report.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Ler fontes e confirmar dependências.
- [ ] T002 — Auditar UI/modal/calendar runtime existente.
- [ ] T003 — Criar/adaptar CalendarScreen/DayCell/DetailDrawer.
- [ ] T004 — Implementar visibility/unknown states.
- [ ] T005 — Adicionar tests de visibility quando possível.
- [ ] T006 — Rodar validações obrigatórias.
- [ ] T007 — Criar execution report.

---

## 23A. Execution Readiness Matrix

| Área | Pergunta obrigatória | Evidência esperada | Status se faltar |
|---|---|---|---|
| Fonte canônica | A execução leu Source Map e directions do domínio? | Lista de fontes no execution report. | PARTIAL |
| Estado real do repo | Sistemas existentes foram auditados antes de criar novos? | Comandos `rg` e achados no report. | PARTIAL |
| Não duplicação | Existe sistema equivalente já implementado/parcial? | Decisão REUSE/HARDEN/CREATE/DEFER. | BLOCKED se duplicar |
| Escopo | A execução ficou dentro de world/time/calendar/weather/lunar? | Arquivos alterados e justificativa. | PARTIAL |
| Save/load | Houve schema change? | Declaração explícita NO ou STOP se migration necessária. | BLOCKED se alterar sem migration |
| Eventos | Publishers/subscribers/lifecycle foram mapeados? | Mapa de eventos e unsubscribe policy se houver. | PARTIAL |
| UI/PlayMode | Há fluxo visual ou gameplay integrado? | Cenário final deferido documentado. | BUILD_VALIDATED no máximo se ausente |
| Testes | Há lógica determinística nova? | EditMode test ou NOT RUN justificado. | PARTIAL |
| Report | Execution report foi criado? | `docs/validation/02_spec_calendar_ui_weather_lunar_display_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Time|Clock|Calendar|Season|Weather|Rain|Irrigation|Lunar|Festival|DayTransition|Save" Assets/_Game/Scripts docs/design .specs
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
Given o sistema base relacionado a world/time/calendar/weather/lunar existe ou foi criado de forma mínima
When o fluxo principal desta spec é executado
Then o resultado segue o direction canônico
And nenhum sistema paralelo é criado
And o execution report registra evidência.
```

### Scenario 2 — Existing implementation is found

```text
Given existe implementação parcial ou completa no repo
When a execução audita o estado real
Then ela muda para REUSE_EXISTING ou HARDEN_EXISTING
And não recria arquitetura paralela
And documenta residual/future gaps.
```

### Scenario 3 — Missing dependency

```text
Given uma dependência runtime não existe no repo local
When a execução encontra essa ausência
Then ela não inventa uma solução massiva
And marca como MISSING_BUT_DEFER ou cria apenas adapter/validator mínimo se seguro
And registra risco residual.
```

### Scenario 4 — Save/load safety

```text
Given o fluxo envolve estado persistido direta ou indiretamente
When save/load ocorre após a ação
Then nenhum dado derivado de UI/debug substitui a fonte de verdade
And nenhum UnityEngine.Object é persistido
And schema change exige spec/migration separada.
```

### Scenario 5 — Final human validation deferred

```text
Given o fluxo exige interação visual, PlayMode ou gameplay integrado
When a spec é concluída tecnicamente
Then o report registra cenário final em vez de pedir validação humana imediata
And o status máximo respeita SPEC_VALIDATION_MATRIX_MASTER.md.
```

---

## 23D. Edge Cases and Failure Modes

A execução deve cobrir ou registrar risco residual para:

- Clima/lua/festival secreto revelado cedo.
- Day transition duplicado ou fora de ordem.
- Rain/Storm molhando estufa/interior indevidamente.
- World state não persistido ou restaurado em ordem errada.
- UI gerando estado em vez de consumir projection.
- Runtime depender de human test por spec.
- Mudança determinística sem EditMode test ou justificativa.
- Execução de WAVE 02 antes da 01Q sem exceção humana explícita.

---

## 23E. Minimum Execution Report Template

```md
# Execution Report — Calendar UI / Weather / Lunar Display

## Summary
- Spec:
- Wave: WAVE 02
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
- Existing implementation handling:
- Missing dependency:
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
6. A implementação executar WAVE 02+ em massa antes da 01Q ou exceção humana explícita.
7. Não for possível decidir se sistema existente é canônico ou obsoleto.
```

## 25. Validações obrigatórias

Docs validation obrigatória.

C# build/Unity compile se houver alteração C#.

EditMode para visibility/view model quando criado.

PlayMode automated ou final human scenario porque UI/modal/input são afetados.

---

## 26. Testing Quality Gate

```text
Changed deterministic logic: YES if visibility/view model logic is added.
Requires EditMode tests: YES for visibility rules when practical.
Requires PlayMode automated or final human scenario: YES.
Requires regression test: YES if fixing known calendar/input leak bug.
Human validation timing: DEFERRED_TO_FINAL_VALIDATION.
Minimum validation evidence for ACCEPTED: docs validation PASS; C# build/Unity compile PASS; EditMode PASS for visibility or justified NOT RUN; PlayMode/final human scenario documented; execution report created.
```

---

## 27. Definition of Done

```text
Calendar UI modal works or scenario documented.
Input gameplay blocked while modal open.
Known/hidden/unknown visibility respected.
Day detail drawer exists or explicit residual risk.
No UI state saved.
Execution report created.
```

---

## 28. Anti-regressão

```text
Não revelar hidden events.
Não usar Calendar UI como fonte de truth.
Não deixar WASD mover personagem em modal.
Não salvar selected day/scroll/hover como gameplay state.
Não declarar ACCEPTED sem PlayMode/cenário final.
```

---

## 29. Notas para execução posterior

Esta spec prepara quest temporal hints, festival UI, restock display e NPC schedule availability UI.
