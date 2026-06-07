# SPEC — Calendar UI / Weather / Lunar Display

> **Spec ID:** `02_spec_calendar_ui_weather_lunar_display`  
> **Status:** A implementar  
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
- `docs/specs/a_implementar/04_spec_ui_input_focus_modal_routing.md or existing UI modal/input stack audit`
- `docs/specs/a_implementar/02_spec_time_clock_day_transition_runtime.md`
- `docs/specs/a_implementar/02_spec_calendar_season_year_runtime.md`
- `docs/specs/a_implementar/02_spec_weather_generation_forecast_runtime.md`
- `docs/specs/a_implementar/02_spec_lunar_cycle_event_runtime.md`
- `docs/specs/a_implementar/02_spec_calendar_festivals_events_runtime.md`
- `docs/specs/a_implementar/02_spec_time_calendar_weather_lunar_save_state.md`
> **Blocks:**  
- `quest log temporal visibility integration`
- `festival quest UI`
- `NPC schedule availability display`
- `shop restock/calendar display`
- `farm seasonal planning UI`
> **Scope:** implementar/adaptar a tela de calendário para mostrar dia, estação, clima atual/previsão, eventos lunares conhecidos e festivais/eventos conhecidos, respeitando anti-spoiler e input modal.  
> **Out of scope:** criar runtime de tempo/clima/lua/festival, criar social birthdays completos, criar quest log completo, criar gamepad final, criar minigames de festival.

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
- `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`
- `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`
- `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`
- `docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`
- `docs/project/CURRENT_STATE.md`
- `docs/IMPLEMENTATION_STATUS.md`

### 4.2 Directions de domínio lidos

- `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`
- `docs/design/gameplay/ui_ux/UI_UX_MENU_SCREEN_FLOWS_DIRECTION.md`
- `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
- `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
- `docs/design/gameplay/quests/QUEST_OBJECTIVE_EVENT_SYSTEM_DIRECTION.md`

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
docs/specs/SPEC_EXECUTION_ORDER.md
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
