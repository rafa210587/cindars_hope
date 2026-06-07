# SPEC — Time / Calendar / Weather / Lunar Save State

> **Spec ID:** `02_spec_time_calendar_weather_lunar_save_state`  
> **Status:** A implementar  
> **Wave:** WAVE 02 — Time / Calendar / Weather / Lunar  
> **Priority:** P0  
> **Type:** Runtime / Save / Validation / Hardening  
> **Domain:** World / Time / Calendar / Weather / Lunar / Save  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_02_WORLD_TIME_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** qualquer spec que altere GameSaveData, SaveManager, migrations, GameTime, calendar/weather/lunar runtime, festival state, day transition ou UI calendar.  
> **Repo lock scope:** `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Time/**`, `Assets/_Game/Scripts/Calendar/**`, `Assets/_Game/Scripts/Weather/**`, `Assets/_Game/Scripts/Lunar/**`, `docs/validation/02_spec_time_calendar_weather_lunar_save_state_execution_report.md`.  
> **Depends on:**  
- `docs/specs/a_implementar/01_spec_stable_ids_registry_runtime.md`
- `docs/specs/a_implementar/01_spec_save_restore_order_contract_runtime.md`
- `docs/specs/a_implementar/01_spec_save_section_ownership_registry.md`
- `docs/specs/a_implementar/01_spec_invalid_id_fallback_rules.md`
- `docs/specs/a_implementar/02_spec_time_clock_day_transition_runtime.md`
- `docs/specs/a_implementar/02_spec_calendar_season_year_runtime.md`
- `docs/specs/a_implementar/02_spec_weather_generation_forecast_runtime.md`
- `docs/specs/a_implementar/02_spec_lunar_cycle_event_runtime.md`
- `docs/specs/a_implementar/02_spec_calendar_festivals_events_runtime.md`
> **Blocks:**  
- `02_spec_calendar_ui_weather_lunar_display.md`
- `03_spec_quest_condition_trigger_runtime.md`
- `quest temporal conditions`
- `NPC schedule modifiers by time/weather/lunar`
- `farm crop seasonal/weather/lunar integrations`
- `shop/calendar/festival restock rules`
> **Scope:** persistir e restaurar o estado de tempo, calendário, clima, lua e festivais como seção controlada de save, sem reescrever SaveManager e sem implementar UI.  
> **Out of scope:** definir UI de calendário, criar novos festivais de conteúdo, criar quest system, alterar política final de save em caverna, implementar pets/companions/social save.

---

# /speckit.specify

## 1. Contexto

As specs anteriores da WAVE 02 criam ou endurecem tempo, calendário, clima, lua e festivais. O direction de Save/Load define que o save atual já usa `SaveManager`, `GameSaveData`, schema version, migrations, safe write e diversas seções existentes. Também define que o próximo trabalho é consolidar full-state, não recriar save/load.

O direction de World/Time exige persistir `CurrentDay`, `CurrentSeason`, `CurrentYear`, `CurrentTime`, `CurrentWeather`, `TomorrowWeather`, `ActiveLunarEvent`, `KnownLunarEvents`, `FestivalState`, `WeatherSeed`, `CalendarEventStates`, `PendingDayTransitionState`, `LastProcessedDay` e `DayTransitionVersion`.

Esta spec transforma esse refinement em contrato implementável de persistência.

---

## 2. Problema

Sem uma seção persistida e restaurada de forma explícita para time/calendar/weather/lunar/festival, o jogo pode recalcular estado temporal de modo inconsistente após load. Isso afeta crops, previsão, NPC schedules, lojas, quests temporais, eventos lunares, festivais, Fonte, Mana e day transition.

O risco mais grave é salvar parte do tempo em um campo antigo e derivar clima/lua/festival em runtime sem seed/default/migration, fazendo o jogador perder estado ou ver eventos mudarem após reload.

---

## 3. Objetivo

Criar ou endurecer a seção de save responsável por tempo/calendário/clima/lua/festivais, com defaults, restore order, validação de IDs, seed determinístico e relatório de execução. O `SaveManager` continua orquestrador e o schema só muda se a auditoria local provar necessidade e houver migration/default explícito.

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

- `docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md`
- `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`
- `docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md`

### 4.3 Covered from directions

```text
Persistir CurrentDay, CurrentSeason, CurrentYear e CurrentTime.
Persistir CurrentWeather e TomorrowWeather sem salvar texto renderizado de forecast.
Persistir ActiveLunarEvent e KnownLunarEvents sem revelar eventos ocultos.
Persistir FestivalState e CalendarEventStates.
Persistir WeatherSeed, PendingDayTransitionState, LastProcessedDay e DayTransitionVersion quando runtime existir.
Restaurar tempo/calendário antes de NPC schedules, shops, crops, quests e UI derivada.
Usar IDs estáveis para WeatherId, LunarEventId e FestivalId.
Não persistir VFX/SFX/UI transitória/pathfinding/preço final calculado.
```

### 4.4 Deferred / future from directions

```text
Pet/companion/social save completo.
FonteAnyaSection full state, salvo hooks/IDs se já existirem.
MainProgression full state.
Política final de save em caverna.
Autosave/multiple slots/cloud save.
UI de save/load.
```

### 4.5 Explicitly not redefined here

```text
Não redefine GameSaveData como root DTO.
Não redefine SaveManager como orquestrador.
Não redefine algoritmo de clima/lua/calendário; apenas persiste o estado produzido por esses sistemas.
Não redefine QuestState/MainProgression/FonteAnya sections.
Não redefine Calendar UI; essa é a spec seguinte.
```

---

## 5. Estado atual do repo

```text
Save/load atual já existe com SaveManager, GameSaveData, schema version, migrations e safe write.
GameTime já aparece como seção atual de GameSaveData segundo Save/Load direction, mas precisa evoluir para cobrir calendário, clima, estação e lua.
World/Time direction exige persistência de tempo, clima, lua e festival.
Spec de restore order e section ownership ainda devem ser executadas antes desta spec.
Este trabalho não deve rodar em massa antes da 01Q ou exceção humana explícita.
```

---

## 6. User stories / engineering stories

```text
Como sistema de calendário, quero restaurar dia/estação/ano/hora sem drift após load.
Como sistema de clima, quero restaurar clima atual e previsão do dia seguinte sem reroll indevido.
Como sistema lunar, quero preservar evento ativo e conhecimento descoberto sem spoiler.
Como festival runtime, quero restaurar estado ativo/conhecido/encerrado sem repetir recompensa.
Como save/load, quero defaults seguros para saves antigos sem apagar progresso.
```

---

## 7. Escopo

```text
Auditar GameSaveData/GameTime/time/weather/lunar/festival sections existentes.
Criar ou consolidar TimeCalendarWeatherLunar save DTO/section se necessário.
Definir defaults para saves antigos.
Definir migration requirement se shape persistido mudar.
Validar WeatherId/LunarEventId/FestivalId contra registries quando existirem.
Garantir restore order antes de schedules, shops, crops, quests e UI derivada.
Criar execution report.
```

---

## 8. Fora de escopo

```text
Não criar Calendar UI.
Não criar conteúdo final de festivais.
Não criar quest system.
Não criar full Fonte/Main save.
Não alterar scenes/prefabs/assets.
Não alterar save em caverna policy.
Não criar autosave/multi-slot.
```

---

## 9. Regras de não duplicação

```text
Não criar segundo SaveManager.
Não criar seção paralela se GameTime já cobre o mesmo estado.
Não persistir texto de forecast ou UI state.
Não persistir ScriptableObject/GameObject/Transform.
Não salvar preço final calculado.
Não salvar NPC schedule derivado como pathfinding transitório.
```

---

## 10. Critérios de aceite

### 10.1 Section auditada/criada

- Existe uma section/DTO clara para TimeCalendarWeatherLunar state ou a seção existente `GameTime` foi consolidada para esse papel.
- O report lista fields atuais, fields adicionados e fields deliberadamente não adicionados.

### 10.2 Defaults e migrations

- Se o shape de save mudar, a execução declara migration/default seguro.
- Se não houver migration, há justificativa explícita de compatibilidade.
- Seções ausentes em save legado não causam crash.

### 10.3 Restore order

- Tempo/calendário/clima/lua/festivais restauram antes de NPC schedules, shops, crops, quests e UI derivada.
- Post-restore notifications só disparam depois do estado primário.

### 10.4 Validação

- Validator/EditMode test cobre round-trip ou defaults quando praticável.
- IDs inválidos de Weather/Lunar/Festival geram fallback/log claro.
- Execution report criado em `docs/validation/02_spec_time_calendar_weather_lunar_save_state_execution_report.md`.

---

# /speckit.plan

## 11. Arquitetura alvo

Arquitetura alvo preserva `SaveManager` e `GameSaveData`.

Possíveis alvos:
```text
Assets/_Game/Scripts/Save/TimeCalendarWeatherLunarSaveData.cs
Assets/_Game/Scripts/Save/GameSaveData.cs
Assets/_Game/Scripts/World/Time/**
Assets/_Game/Scripts/World/Calendar/**
Assets/_Game/Scripts/World/Weather/**
Assets/_Game/Scripts/World/Lunar/**
Assets/_Game/Tests/EditMode/Save/TimeCalendarWeatherLunarSaveTests.cs
docs/validation/02_spec_time_calendar_weather_lunar_save_state_execution_report.md
```

Se `GameTimeSaveData` já existir, preferir consolidar o existente em vez de criar DTO paralelo.

---

## 12. Contratos, dados e eventos

### Data contracts

`GameSaveData` permanece root DTO. Time/calendar/weather/lunar/festival state deve usar tipos simples e IDs estáveis.

### Runtime contracts

Runtime de tempo é fonte primária. UI deriva do runtime restaurado.

### Event contracts

Eventos de load/day started/weather changed/lunar/festival são notificações pós-restore, não fonte primária.

### Save contracts

Não salvar VFX/SFX/UI/pathfinding/texto renderizado. Defaults não podem revelar eventos ocultos.

### UI contracts

N/A — UI implementada em spec própria.

---

## 13. Sistemas afetados

```text
SaveManager
GameSaveData
GameTime/time runtime
Calendar runtime
Weather runtime
Lunar runtime
Festival runtime
NPC schedule consumers
Quest temporal conditions
Farm crop/weather consumers
Shop calendar/restock consumers
```

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/Save/**
Assets/_Game/Scripts/World/**
Assets/_Game/Scripts/Time/**
Assets/_Game/Scripts/Calendar/**
Assets/_Game/Scripts/Weather/**
Assets/_Game/Scripts/Lunar/**
Assets/_Game/Tests/EditMode/Save/**
Assets/_Game/Scripts/Editor/Validation/**
docs/validation/02_spec_time_calendar_weather_lunar_save_state_execution_report.md
```

---

## 15. Arquivos proibidos

```text
Packages/**
ProjectSettings/**
Assets/**/*.unity
Assets/**/*.prefab
Assets/**/*.asset salvo data asset test-only explicitamente criado
docs/specs/SPEC_EXECUTION_ORDER.md
docs/specs/implementados/**
docs/refinements/implementados/**
docs/project/CURRENT_STATE.md
PROJECT_LOG.md
```

---

## 16. Estratégia de implementação

### Fase 0 — Auditoria local

```bash
rg -n "GameTime|CurrentDay|CurrentSeason|CurrentYear|CurrentWeather|TomorrowWeather|Lunar|Festival|WeatherSeed|DayTransition" Assets/_Game/Scripts
rg -n "GameSaveData|SaveData|ValidateAndNormalizeSave|Migration|SchemaVersion" Assets/_Game/Scripts/Save
```

### Fase 1 — Mapeamento

Listar estado existente, section atual e consumers. Marcar campos ausentes como required/deferred.

### Fase 2 — Implementação mínima

Adicionar/consolidar DTO/fields/defaults somente se necessário. Não alterar schema sem migration/default.

### Fase 3 — Validação

Rodar docs/build/Unity compile/EditMode se C# mudar. Criar report com matrix de fields e riscos.

---

## 17. Ordem segura de execução

```text
Ler fontes obrigatórias.
Confirmar que restore-order/ownership specs foram executadas ou registrar BLOCKED.
Auditar GameSaveData e runtime temporal existente.
Definir fields/defaults/migration.
Implementar mínimo ou registrar deferred.
Rodar validações.
Criar execution report.
```

---

## 18. Paralelização

- Parallelizable: NO
- Parallel group: WAVE_02_WORLD_TIME_LOCKED
- Can run with:
  - N/A
- Must not run with:
  - qualquer spec que altere GameSaveData, SaveManager, migrations, GameTime, calendar/weather/lunar runtime, festival state, day transition ou UI calendar.
- Reason:
  - Time/calendar/weather/lunar save state é cross-cutting e toca GameSaveData/SaveManager/restore order.

---

## 19. Impacto em save/load

```text
Does this change save schema? YES if fields/DTOs are added; requires default/migration decision.
Does this add a save section? CONDITIONAL; prefer consolidate existing GameTime if present.
Does this require migration? CONDITIONAL; if shape changes, require migration/default.
Does this persist Unity references? MUST BE NO.
```

---

## 20. Impacto em eventos

```text
Adds events: NO by default.
Changes events: NO by default.
Post-restore notifications only after state restored.
```

---

## 21. Impacto em UI/Unity

```text
Changes UI: NO.
Changes scenes/prefabs: NO.
Requires PlayMode/final human scenario: NO unless scene-bound restore behavior is touched.
Human validation timing: NOT REQUIRED or DEFERRED_TO_FINAL_VALIDATION if scene behavior is touched.
```

---

## 22. Riscos técnicos

```text
Risco: reroll de clima/lua após load. Mitigação: persistir seed/current/tomorrow/active event.
Risco: save legado sem seção quebrar load. Mitigação: defaults/normalização.
Risco: revelar eventos lunares ocultos. Mitigação: KnownLunarEvents separado de ActiveLunarEvent.
Risco: quebrar consumers por restore order. Mitigação: respeitar Save/Load direction.
```

---

## 23. Rollback

```text
Reverter fields/DTOs/validators criados.
Reverter migration se criada.
Remover execution report.
Não tocar em cenas/prefabs/assets.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Ler fontes obrigatórias e confirmar branch.
- [ ] T002 — Auditar GameSaveData/GameTime/weather/lunar/festival state.
- [ ] T003 — Mapear fields existentes vs direction.
- [ ] T004 — Definir section/DTO/default/migration.
- [ ] T005 — Implementar/consolidar estado mínimo se necessário.
- [ ] T006 — Adicionar validator/EditMode test quando praticável.
- [ ] T007 — Rodar validações obrigatórias.
- [ ] T008 — Criar execution report.

---

## 25. Validações obrigatórias

Docs:
```powershell
.\tools\docs\validate_docs.ps1
```

C# quando houver alteração:
```powershell
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
```

Unity compile quando houver alteração C#:
```powershell
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

EditMode tests quando test/validator for criado.

---

## 26. Testing Quality Gate

```text
Changed deterministic logic: YES if DTO/default/validator/migration logic changes.
Requires EditMode tests: YES when deterministic save/default/round-trip logic changes and harness is available.
Requires PlayMode automated or final human scenario: NO unless scene-bound restore behavior is touched.
Requires regression test: YES if fixing known save/load bug.
Human validation timing: NOT REQUIRED or DEFERRED_TO_FINAL_VALIDATION if scene-bound behavior is touched.
Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS if C# changed; Unity compile PASS if Unity C# changed; EditMode/default/round-trip validation PASS or NOT RUN with justified risk; execution report created.
```

---

## 27. Definition of Done

```text
Time/calendar/weather/lunar/festival save state audited.
Fields/defaults/migration decision documented.
No Unity references persisted.
Known/hidden lunar/festival knowledge separated.
Restore order respected.
Validator/test added or risk documented.
Execution report created.
SPEC_EXECUTION_ORDER.md unchanged.
```

---

## 28. Anti-regressão

```text
Não recriar SaveManager.
Não salvar UI state/texto renderizado/VFX/SFX.
Não revelar lunar/festival hidden state por save/UI.
Não apagar sections scene-bound ao salvar fora de cena.
Não declarar ACCEPTED apenas por compile.
```

---

## 29. Notas para execução posterior

Esta spec alimenta a Calendar UI, quest temporal conditions, NPC schedules, shop restock, crop weather/season rules e festival quests.
