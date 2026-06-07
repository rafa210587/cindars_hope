# SPEC — Calendar Season Year Runtime Audit and Hardening

> **Spec ID:** `02_spec_calendar_season_year_runtime`  
> **Status:** A implementar  
> **Wave:** WAVE 02 — Time / Calendar / Weather / Lunar  
> **Priority:** P1  
> **Type:** Runtime / Data / World / Calendar / Save / Hardening  
> **Domain:** World / Calendar / Season / Year / DayOfWeek  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_02_TIME_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** time clock/day transition implementation, weather/lunar generation, festival events, calendar UI, farm crop season rules, NPC schedules, shop restock or quest time hooks.  
> **Repo lock scope:** `Assets/_Game/Scripts/World/Calendar/**`, `Assets/_Game/Scripts/World/Time/**`, `Assets/_Game/Scripts/Save/**Calendar*`, `Assets/_Game/Scripts/Save/**Time*`, `Assets/_Game/Tests/EditMode/World/**`, `docs/validation/02_spec_calendar_season_year_runtime_execution_report.md`.  
> **Depends on:**  
> - `docs/specs/a_implementar/02_spec_time_clock_day_transition_runtime.md`  
> - `docs/specs/a_implementar/spec_test_harness_editmode_playmode_quality_gate.md`  
> - `docs/specs/a_implementar/01_spec_stable_ids_registry_runtime.md`  
> - `docs/specs/a_implementar/01_spec_game_event_contracts_runtime.md`  
> - `docs/specs/a_implementar/01_spec_save_restore_order_contract_runtime.md`  
> - `docs/specs/a_implementar/01_spec_save_section_ownership_registry.md`  
> - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`  
> - `docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`  
> - `docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`  
> - `docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md`  
> **Blocks:**  
> - weather generation/forecast;  
> - lunar cycle runtime;  
> - festival/calendar events;  
> - crop season rules;  
> - NPC/shop schedule by season/day;  
> - quest deadlines/calendar hooks;  
> - calendar UI display.  
> **Scope:** audit and implement/harden deterministic calendar date calculation: 4 seasons, 28 days per season, 7-day week, year/day indexing and save/load compatibility.  
> **Out of scope:** weather generation, lunar events, festivals details, birthdays, calendar UI, crop availability, NPC schedule content, shop restock, quest deadlines, scenes/prefabs or human PlayMode execution.

---

# /speckit.specify

## 1. Contexto

The world direction defines the canonical calendar baseline:

```text
4 seasons per year;
28 days per season;
112 days per year;
7 days per week;
initial season names: Primavera, Verão, Outono, Inverno;
calendar public display later includes day, season, year, forecast, lunar event, festivals, deadlines and discovered events.
```

This spec implements only the deterministic calendar backbone. It depends on the canonical time/day transition spec and does not implement weather, lunar, festivals, UI or domain reactions.

The audit report marks calendar/weather/lunar as PARTIAL / UNKNOWN. Therefore local audit is mandatory before creating anything new.

---

## 2. Problema

Without a canonical calendar backbone:

```text
weather and lunar systems may calculate different days;
farm crop seasons may use ad hoc season values;
NPC/shop/restock schedules may drift;
quest deadlines may become inconsistent;
save/load may restore day but not season/year;
UI calendar may reveal or calculate wrong dates;
festival specs may choose incompatible day numbering.
```

---

## 3. Objetivo

At the end of this spec, the project must have a deterministic calendar runtime contract and implementation/hardening path.

Expected result:

```text
absolute day index maps to day-in-season, season, year and day-of-week;
4 seasons x 28 days x 7-day week are explicit;
calendar state is derived from or synchronized with canonical time/day index;
calendar save/load behavior is documented;
no weather/lunar/festival/UI logic is implemented here;
EditMode tests cover boundary transitions when practical.
```

---

## 4. Fontes obrigatórias lidas

Execution must read:

```text
CLAUDE.md
AGENTS.md
docs/project/CURRENT_STATE.md
docs/specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
docs/specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
docs/specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
docs/specs/SPEC_VALIDATION_MATRIX_MASTER.md
docs/specs/SPEC_GENERATION_ROADMAP_MASTER.md
docs/specs/a_implementar/02_spec_time_clock_day_transition_runtime.md
docs/specs/a_implementar/spec_test_harness_editmode_playmode_quality_gate.md
docs/specs/a_implementar/01_spec_save_restore_order_contract_runtime.md
docs/specs/a_implementar/01_spec_save_section_ownership_registry.md
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/unity-validation/SKILL.md
```

Conditional reads:

```text
FARM_DESIGN_DIRECTION_v1.3.md only for naming future hooks, not crop implementation.
CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md only for future schedule compatibility.
UI_UX_FULL_GAMEPLAY_DIRECTION.md only for future calendar display constraints.
```

---

## 5. Estado atual do repo

Documented state:

```text
- MVP time exists in some form, but full calendar is not proven by documental audit.
- Weather/lunar/calendar complete is not documented as complete.
- Time/day transition spec must be generated/executed before calendar execution.
- WAVE 02 runtime execution must remain gated by 01Q/testing quality gate.
```

Required local audit:

```text
- classes named Calendar, Season, Year, DayOfWeek, GameDate, GameTime;
- save DTOs for calendar/time;
- existing season enums or constants;
- existing UI text that assumes date format;
- tests for date rollover;
- event hooks for day started/season changed/year changed.
```

---

## 6. User stories / engineering stories

```text
As weather system, I need deterministic date input for forecast generation.
As farm crops, I need canonical season/day values.
As quest system, I need stable dates and deadlines.
As UI calendar, I need a safe public date model that does not reveal secrets.
As save/load, I need date state restored consistently from absolute day/time.
```

---

## 7. Escopo

Includes:

```text
- audit existing calendar/date/season implementation;
- define or harden calendar data model;
- implement/confirm mapping from absolute day to season/year/day-of-week;
- define boundaries: day 28 -> next season, winter day 28 -> next year;
- define save/load strategy for date state;
- create calendar events only if missing and required as non-breaking notifications;
- add EditMode tests for deterministic date boundaries when practical;
- create execution report.
```

---

## 8. Fora de escopo

Does not include:

```text
- weather generation/forecast;
- lunar cycle;
- festivals implementation;
- birthdays/social calendar;
- shop restock logic;
- crop season effects;
- NPC schedules;
- quest deadline mechanics;
- calendar UI;
- scene/prefab edits;
- human PlayMode execution now.
```

---

## 9. Regras de não duplicação

```text
Do not create calendar date if existing GameDate equivalent exists.
Do not duplicate season enum/constants.
Do not store season/year separately if absolute day already safely derives them, unless save compatibility requires it.
Do not implement weather/lunar/festival logic here.
Do not let UI formatting become source of date truth.
Do not change time/day transition behavior in this spec.
```

---

## 10. Critérios de aceite

### 10.1 Calendar model

Must define:

```text
seasons: Primavera, Verão, Outono, Inverno or existing equivalent;
days per season: 28;
seasons per year: 4;
days per year: 112;
days per week: 7;
day-of-week range: 1-7 or existing enum;
year starts at 1 unless repo already uses different baseline.
```

### 10.2 Date conversion

Must support deterministic conversion between:

```text
absolute day index;
year;
season;
day in season;
day of week.
```

Boundary cases must be covered:

```text
first day;
last day of season;
first day of next season;
last day of winter;
first day of next year;
week rollover.
```

### 10.3 Save/load

Must define whether save persists:

```text
absolute day index only;
or absolute day plus cached derived fields;
or existing project equivalent.
```

Cached derived fields must not become inconsistent with absolute day.

### 10.4 Events

If events are added or confirmed, they must be notifications:

```text
DateChangedEvent;
SeasonChangedEvent;
YearChangedEvent;
WeekChangedEvent, if needed.
```

Do not add if existing events already cover the case.

### 10.5 Tests/report

When practical, add EditMode tests for conversion/boundaries and create:

```text
docs/validation/02_spec_calendar_season_year_runtime_execution_report.md
```

---

# /speckit.plan

## 11. Arquitetura alvo

Possible targets depending on local audit:

```text
Assets/_Game/Scripts/World/Calendar/GameDate.cs
Assets/_Game/Scripts/World/Calendar/Season.cs
Assets/_Game/Scripts/World/Calendar/GameCalendarService.cs
Assets/_Game/Scripts/Core/Events/DateChangedEvent.cs
Assets/_Game/Scripts/Core/Events/SeasonChangedEvent.cs
Assets/_Game/Scripts/Save/CalendarSaveData.cs
Assets/_Game/Tests/EditMode/World/Calendar/GameCalendarServiceTests.cs
docs/validation/02_spec_calendar_season_year_runtime_execution_report.md
```

If equivalents exist, use/harden them instead.

---

## 12. Contratos, dados e eventos

### 12.1 Data contracts

```text
Date model must be deterministic and serializable.
Absolute day index is preferred source of truth unless existing save model differs.
Season/year/day-of-week are derived or validated against source.
```

### 12.2 Runtime contracts

```text
Calendar depends on canonical time/day index.
Calendar does not advance independently.
Calendar does not own day transition.
```

### 12.3 Event contracts

```text
Calendar events fire after date changes.
Events are notifications, not source of save state.
```

### 12.4 Save contracts

```text
No Unity references.
No schema change unless ownership/migration path exists.
Missing calendar section defaults to day 1/year 1/season 1 or existing baseline.
```

### 12.5 UI contracts

```text
No UI implementation.
Future UI must not reveal hidden events before discovery.
```

---

## 13. Sistemas afetados

```text
World calendar/date;
time/day index integration;
save/load date state;
calendar events;
future weather/lunar/farm/NPC/shop/quest/UI systems;
EditMode validation.
```

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/World/Calendar/**
Assets/_Game/Scripts/World/Time/**, only for integration with existing day index
Assets/_Game/Scripts/Core/Events/*Date*
Assets/_Game/Scripts/Core/Events/*Season*
Assets/_Game/Scripts/Core/Events/*Year*
Assets/_Game/Scripts/Save/**Calendar*SaveData.cs
Assets/_Game/Tests/EditMode/World/Calendar/**
Assets/_Game/Scripts/Editor/Validation/**Calendar*
docs/validation/02_spec_calendar_season_year_runtime_execution_report.md
```

---

## 15. Arquivos proibidos

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

## 16. Estratégia de implementação

### Fase 0 — Auditoria local obrigatória

```bash
rg -n "GameDate|Calendar|Season|Year|DayOfWeek|DayOfSeason|AbsoluteDay|DateChanged|SeasonChanged" Assets/_Game/Scripts
rg -n "CalendarSaveData|GameTimeSaveData|dayIndex|currentDay|season|year" Assets/_Game/Scripts/Save Assets/_Game/Scripts
```

### Fase 1 — Model

```text
1. Use existing model if present.
2. Define constants and conversion rules.
3. Avoid UI/festival/weather logic.
```

### Fase 2 — Integration

```text
1. Connect to canonical day index from time spec.
2. Add save/default behavior if needed.
3. Add non-breaking events only if needed.
```

### Fase 3 — Tests/report

```text
1. Add EditMode boundary tests.
2. Run validations.
3. Create execution report.
```

---

## 17. Ordem segura de execução

```text
1. Read sources.
2. Confirm time/day transition spec/report exists.
3. Audit existing calendar/date code.
4. Harden or create minimal date model.
5. Add tests.
6. Run validations.
7. Create report.
```

---

## 18. Paralelização

- Parallelizable: NO
- Parallel group: WAVE_02_TIME_CORE_LOCKED
- Can run with:
  - N/A
- Must not run with:
  - time clock/day transition;
  - weather/lunar;
  - crop season rules;
  - calendar UI;
  - festival/quest deadline specs.
- Reason:
  - Calendar date is a shared deterministic contract.

---

## 19. Impacto em save/load

```text
Does this change save schema? CONDITIONAL. Prefer deriving from existing day index. If new section is required, use ownership/defaults and avoid migration unless necessary.
Does this add a save section? CONDITIONAL.
Does this require migration? SHOULD BE NO; YES requires separate migration spec.
Does this persist Unity references? MUST BE NO.
```

---

## 20. Impacto em eventos

```text
Adds events: CONDITIONAL — Date/Season/Year notification only if absent and necessary.
Changes existing events: SHOULD BE NO.
Requires unsubscribe pattern: YES for future subscribers, but broad subscribers out of scope.
```

---

## 21. Impacto em UI/Unity

```text
Changes UI: NO
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires PlayMode automated or final human scenario: NO for pure date math; YES only if runtime scene/time integration is touched, then DEFERRED_TO_FINAL_VALIDATION.
Human validation timing: NOT REQUIRED unless runtime scene-bound behavior is touched.
```

---

## 22. Riscos técnicos

```text
Risk: calendar duplicates existing date model.
Mitigation: local audit first; harden existing if present.

Risk: cached date fields drift from absolute day.
Mitigation: prefer derived fields or validation.

Risk: calendar spec spills into weather/festival/UI.
Mitigation: keep this spec to deterministic date model.

Risk: save schema change needed.
Mitigation: prefer derived date from time day index; otherwise separate migration path.
```

---

## 23. Rollback

```text
Revert calendar model/integration changes.
Remove tests/validators added.
Remove execution report.
No scenes/prefabs/assets should have changed.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Read sources.
- [ ] T002 — Confirm time/day transition dependency exists.
- [ ] T003 — Audit existing calendar/date model.
- [ ] T004 — Define/harden constants and conversion rules.
- [ ] T005 — Integrate with canonical day index.
- [ ] T006 — Add save/default handling if needed.
- [ ] T007 — Add EditMode boundary tests when practical.
- [ ] T008 — Run validations.
- [ ] T009 — Create execution report.

---

## 25. Validações obrigatórias

```powershell
.\tools\docs\validate_docs.ps1
dotnet build .\Assembly-CSharp.csproj --no-restore
dotnet build .\Assembly-CSharp-Editor.csproj --no-restore
.\tools\unity\RunUnityCompileValidation.ps1 -ProjectPath "." -LogFile ".\Logs\unity-compile-validation.log"
.\tools\unity\ScanUnityLogs.ps1 -LogFile ".\Logs\unity-compile-validation.log"
```

EditMode tests if date/calendar code is added/changed.

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES if date/calendar code is added or changed.
- Requires EditMode tests: YES if date conversion logic is added/changed and harness is available.
- Requires PlayMode automated or final human scenario: NO for pure date math; YES only if scene-bound runtime behavior is touched.
- Requires regression test: YES if fixing a known calendar/date bug; otherwise NO.
- Human validation timing: NOT REQUIRED unless scene-bound behavior is touched; then DEFERRED_TO_FINAL_VALIDATION.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS; Unity compile PASS; EditMode tests for date boundaries PASS or justified NOT RUN; no weather/lunar/festival/UI scope creep; execution report created.

---

## 27. Definition of Done

```text
Calendar/date model audited and preserved or minimally created.
4 seasons / 28 days / 112-day year / 7-day week constants defined.
Date conversion boundary tests added or residual risk documented.
Save/default behavior documented.
No weather/lunar/festival/UI domain work included.
Execution report created.
SPEC_EXECUTION_ORDER.md unchanged.
```

---

## 28. Anti-regressão

```text
Do not duplicate date sources.
Do not implement weather/lunar/festivals here.
Do not let UI formatting own date truth.
Do not change scenes/prefabs.
Do not accept deterministic date logic without tests or documented risk.
```

---

## 29. Notas para execução posterior

This spec feeds weather, lunar, festivals, crop seasons, NPC schedule modifiers, shop restock, quest deadlines and calendar UI specs.
