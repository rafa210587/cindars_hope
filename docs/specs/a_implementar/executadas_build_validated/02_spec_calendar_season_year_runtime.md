# SPEC — Calendar Season Year Runtime Audit and Hardening

> **Spec ID:** `02_spec_calendar_season_year_runtime`  
> **Status:** A implementar  
> **Revision:** EXPANDED_01_05_CORRECTED  
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
| Report | Execution report foi criado? | `docs/validation/02_spec_calendar_season_year_runtime_execution_report.md`. | PARTIAL |

---

## 23B. Local Audit Checklist

Antes de alterar qualquer arquivo, Claude Code/Codex deve rodar e registrar no execution report:

```bash
rg -n "Time|Clock|Calendar|Season|Weather|Rain|Irrigation|Lunar|Festival|DayTransition|Save" Assets/_Game/Scripts docs/design docs/specs
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
# Execution Report — Calendar Season Year Runtime Audit and Hardening

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
