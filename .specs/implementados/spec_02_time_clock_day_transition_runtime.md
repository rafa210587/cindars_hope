# SPEC — Time Clock and Day Transition Runtime Audit and Hardening

> **Spec ID:** `02_spec_time_clock_day_transition_runtime`  
> **Status:** A implementar  
> **Revision:** EXPANDED_01_05_CORRECTED  
> **Wave:** WAVE 02 — Time / Calendar / Weather / Lunar  
> **Priority:** P1  
> **Type:** Runtime / World / Time / Events / Save / Hardening  
> **Domain:** World / Time / Clock / Day Transition  
> **Parallelizable:** NO  
> **Parallel group:** WAVE_02_TIME_CORE_LOCKED  
> **Can run with:** N/A  
> **Must not run with:** calendar/season specs, weather/lunar specs, farm day transition/crop growth, NPC schedule, economy restock, quest time hooks, save restore order changes, UI modal/input routing changes.  
> **Repo lock scope:** `Assets/_Game/Scripts/World/**`, `Assets/_Game/Scripts/Time/**`, `Assets/_Game/Scripts/Core/Events/*Day*`, `Assets/_Game/Scripts/Core/Events/*Time*`, `Assets/_Game/Scripts/Save/**`, `Assets/_Game/Tests/EditMode/**`, `docs/validation/02_spec_time_clock_day_transition_runtime_execution_report.md`.  
> **Depends on:**  
> - `.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md`  
> - `.specs/a_implementar/spec_test_harness_editmode_playmode_quality_gate.md`  
> - `.specs/a_implementar/01_spec_stable_ids_registry_runtime.md`  
> - `.specs/a_implementar/01_spec_game_event_contracts_runtime.md`  
> - `.specs/a_implementar/01_spec_save_restore_order_contract_runtime.md`  
> - `.specs/a_implementar/01_spec_save_section_ownership_registry.md`  
> - `.specs/a_implementar/01_spec_invalid_id_fallback_rules.md`  
> - `docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md`  
> - `.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md`  
> - `.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md`  
> - `.specs/SPEC_VALIDATION_MATRIX_MASTER.md`  
> **Blocks:**  
> - calendar/season/year runtime;  
> - weather forecast generation;  
> - lunar cycle runtime;  
> - crop day transition integration;  
> - NPC schedule time hooks;  
> - shop restock/day processing;  
> - quest time conditions.  
> **Scope:** audit and harden the existing time/day transition runtime or create minimal canonical clock if absent, without implementing calendar/weather/lunar/farm/NPC/economy logic.  
> **Out of scope:** full calendar UI, weather generation, lunar cycle, crop growth, shipping, restock, NPC schedules, questlines, scenes/prefabs or human Play Mode validation.

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

The world direction defines the global time model: a standard playable day from 06:00 to 02:00, 02:00 to 06:00 as forced transition/collapse/sleep window, an initial scale of 1 in-game hour = 60 real seconds, and modal UI usually pausing time.

The audit report classifies time/calendar/weather/lunar as PARTIAL / UNKNOWN: hunger/stamina/status/time exists as MVP, but full calendar/weather/lunar is not proven. This spec must therefore begin with local audit and avoid reimplementing existing time systems.

---

## 2. Problema

Without a canonical clock/day transition contract:

```text
farm, NPC, economy, quest and cave systems may advance days differently;
modal UI may fail to pause time;
02:00 collapse/transition may conflict with cave/death rules;
save/load may restore time after systems already consumed it;
day-start/day-end events may fire multiple times;
calendar/weather/lunar specs may build on wrong day counter;
PlayMode final validation may not know what one day means.
```

---

## 3. Objetivo

At the end of this spec, the project must have a canonical time clock/day transition contract and implementation/hardening path.

Expected result:

```text
single source for current time/day progression;
explicit playable window 06:00 -> 02:00;
explicit forced transition/collapse hook after 02:00;
modal pause integration documented/audited;
day transition pipeline creates ordered extension points but does not implement all domain effects;
time state can be saved/restored safely;
EditMode tests cover deterministic time advancement when practical;
execution report documents gaps and final human scenario.
```

---

## 4. Fontes obrigatórias lidas

Execution must read:

```text
CLAUDE.md
AGENTS.md
docs/project/CURRENT_STATE.md
.specs/SPEC_EXISTING_IMPLEMENTATION_AUDIT.md
.specs/SPEC_IMPLEMENTABLE_TEMPLATE.md
.specs/SPEC_WAVE_EXECUTION_PROTOCOL.md
.specs/SPEC_VALIDATION_MATRIX_MASTER.md
.specs/SPEC_GENERATION_ROADMAP_MASTER.md
.specs/a_implementar/spec_test_harness_editmode_playmode_quality_gate.md
.specs/a_implementar/01_spec_game_event_contracts_runtime.md
.specs/a_implementar/01_spec_save_restore_order_contract_runtime.md
.specs/a_implementar/01_spec_save_section_ownership_registry.md
docs/design/gameplay/world/SEASONS_CALENDAR_WEATHER_LUNAR_DIRECTION.md
docs/design/gameplay/ui_ux/UI_UX_FULL_GAMEPLAY_DIRECTION.md
docs/design/gameplay/save_load/SAVE_LOAD_FULL_STATE_DIRECTION.md
.claude/rules/testing-quality-gate.md
.claude/skills/spec-execution/SKILL.md
.claude/skills/unity-validation/SKILL.md
```

Conditional domain docs:

```text
FARM_DESIGN_DIRECTION_v1.3.md for future crop/day hooks only.
CITY_LAYOUT_BUILDINGS_SCHEDULE_DIRECTION.md for future NPC schedule hooks only.
ECONOMY_PRICING_STOCK_REFRESH_DIRECTION.md for future restock hooks only.
```

---

## 5. Estado atual do repo

Documented state:

```text
- Hunger/stamina/status/time is documented as MVP complete.
- Full calendar/weather/lunar is not documented as complete.
- Phase 2-3 Play Mode remains pending.
- WAVE 02 runtime execution in mass remains blocked until 01Q or explicit exception.
```

Required local audit:

```text
- GameTime/TimeManager/Clock/DayStartedEvent classes;
- current time save DTOs;
- modal pause integration;
- day transition event publishers/subscribers;
- sleep/collapse handling;
- any existing tests.
```

Do not recreate existing time manager without audit.

---

## 6. User stories / engineering stories

```text
As farm systems, I need one day transition event/pipeline.
As UI modal system, I need a clear rule for pausing time.
As save/load, I need time state restored before day-dependent systems consume it.
As calendar/weather/lunar, I need a stable day counter.
As final validator, I need reproducible steps to advance time and observe a day transition.
```

---

## 7. Escopo

Includes:

```text
- audit existing clock/time runtime;
- consolidate or create minimal clock/day transition service if absent;
- implement/harden time advancement scale and playable window;
- define day transition phases/hooks without implementing domain effects;
- publish/use events only via existing GameEventBus contracts;
- ensure save/load compatibility for time state;
- add EditMode tests for deterministic time advancement when practical;
- create execution report.
```

---

## 8. Fora de escopo

Does not include:

```text
- calendar seasons/year rules beyond a day counter needed by this spec;
- weather/lunar generation;
- crop growth implementation;
- shipping/restock implementation;
- NPC schedules;
- quest time conditions;
- UI calendar screen;
- scene/prefab edits;
- human Play Mode execution now.
```

---

## 9. Regras de não duplicação

```text
Do not create a second time manager if one exists.
Do not create domain-specific day counters.
Do not make farm/NPC/economy systems own day transition.
Do not publish day events outside the canonical event bus.
Do not make modal pause rules conflict with UI/input foundation.
Do not change save schema without migration spec.
```

---

## 10. Critérios de aceite

### 10.1 Clock contract

Must define:

```text
start of day time: 06:00;
normal playable end: 02:00;
late-night/forced transition window: 02:00-06:00;
initial scale: 1 in-game hour = 60 real seconds, configurable if existing system supports it;
time paused in modal UI unless exception is explicit;
interiors do not pause time by default.
```

### 10.2 Day transition phases

Define phases without implementing all domain work:

```text
pre-day-end checks;
sleep/collapse/forced transition trigger;
save checkpoint if allowed;
primary day increment;
post-day-start notification;
domain extension points for crops, shipping, restock, NPCs, weather, lunar, quests.
```

### 10.3 Save/load

Time state must be persisted/restored or explicitly documented as already persisted.

At minimum:

```text
current day or absolute day index;
time of day;
pause state not persisted unless required;
transition-in-progress handled safely;
missing time section default documented.
```

### 10.4 Tests/validation

When practical, add EditMode tests for:

```text
time advances with configured scale;
time does not advance while paused;
02:00 threshold triggers transition/collapse state;
day increments exactly once;
post-transition event fires once;
round-trip time state if save DTO is touched.
```

### 10.5 Report

Create:

```text
docs/validation/02_spec_time_clock_day_transition_runtime_execution_report.md
```

---

# /speckit.plan

## 11. Arquitetura alvo

Possible targets depending on local audit:

```text
Assets/_Game/Scripts/World/Time/GameTimeService.cs
Assets/_Game/Scripts/World/Time/DayTransitionService.cs
Assets/_Game/Scripts/Core/Events/TimeAdvancedEvent.cs
Assets/_Game/Scripts/Core/Events/DayStartedEvent.cs
Assets/_Game/Scripts/Save/GameTimeSaveData.cs
Assets/_Game/Tests/EditMode/World/Time/GameTimeServiceTests.cs
docs/validation/02_spec_time_clock_day_transition_runtime_execution_report.md
```

If equivalent files already exist, harden them instead of creating new ones.

---

## 12. Contratos, dados e eventos

### 12.1 Data contracts

```text
Time should be represented by deterministic values, not derived from real DateTime during gameplay.
Use integer minutes or equivalent deterministic representation when practical.
```

### 12.2 Runtime contracts

```text
One canonical service owns time advancement.
Day transition service owns transition state and phases.
Domain systems subscribe or are invoked through explicit extension points.
```

### 12.3 Event contracts

```text
DayStarted/DayEnded/TimeAdvanced events must be notifications only.
Events should fire once per transition and after primary state changes.
```

### 12.4 Save contracts

```text
Time state must be restored before domain systems process day-dependent state.
Missing section defaults to day 1 / 06:00 or current project equivalent, documented in report.
```

### 12.5 UI contracts

```text
Modal UI pauses time unless spec explicitly says otherwise.
This spec does not create HUD/calendar UI.
```

---

## 13. Sistemas afetados

```text
World time;
day transition;
GameEventBus events;
save/load time section;
UI modal pause integration;
future farm/calendar/weather/lunar/NPC/economy/quest hooks;
EditMode validation.
```

---

## 14. Arquivos permitidos

```text
Assets/_Game/Scripts/World/Time/**
Assets/_Game/Scripts/Time/**
Assets/_Game/Scripts/Core/Events/*Time*
Assets/_Game/Scripts/Core/Events/*Day*
Assets/_Game/Scripts/Save/**Time*SaveData.cs
Assets/_Game/Tests/EditMode/World/Time/**
Assets/_Game/Scripts/Editor/Validation/**Time*
docs/validation/02_spec_time_clock_day_transition_runtime_execution_report.md
```

Read-only unless strictly necessary:

```text
Assets/_Game/Scripts/UI/**
Assets/_Game/Scripts/Input/**
Assets/_Game/Scripts/Farm/**
Assets/_Game/Scripts/City/**
Assets/_Game/Scripts/Save/**
```

---

## 15. Arquivos proibidos

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

## 16. Estratégia de implementação

### Fase 0 — Auditoria local obrigatória

```bash
rg -n "GameTime|TimeManager|Clock|DayStarted|DayEnded|TimeAdvanced|DayTransition|Sleep|Collapse|PauseTime|TimeScale" Assets/_Game/Scripts
rg -n "GameTimeSaveData|TimeSaveData|currentDay|timeOfDay|dayIndex|hour|minute" Assets/_Game/Scripts/Save Assets/_Game/Scripts
rg -n "Modal|Pause|GameplayInputRouter|Time" Assets/_Game/Scripts/UI Assets/_Game/Scripts/Input Assets/_Game/Scripts
```

### Fase 1 — Contract

```text
1. Consolidate existing time service if present.
2. Document day window and time scale.
3. Define transition phases and extension points.
```

### Fase 2 — Minimal hardening

```text
1. Implement or adjust only missing core behavior.
2. Add deterministic EditMode tests when practical.
3. Avoid domain effects.
```

### Fase 3 — Report

```text
1. Document existing implementation and gaps.
2. Record final human scenario coverage.
```

---

## 17. Ordem segura de execução

```text
1. Read sources.
2. Audit existing time code.
3. Decide harden existing / create minimal / defer.
4. Patch only core time/day transition.
5. Add tests/validators.
6. Run validations.
7. Create execution report.
```

---

## 18. Paralelização

- Parallelizable: NO
- Parallel group: WAVE_02_TIME_CORE_LOCKED
- Can run with:
  - N/A
- Must not run with:
  - calendar/season runtime;
  - weather/lunar runtime;
  - farm day transition;
  - NPC schedule;
  - economy restock;
  - quest time hooks.
- Reason:
  - Time/day transition is the foundation for the entire WAVE 02 and many later waves.

---

## 19. Impacto em save/load

```text
Does this change save schema? CONDITIONAL. If adding time section, must use ownership registry and defaults; if schema change is nontrivial, STOP for migration spec.
Does this add a save section? CONDITIONAL, only if time is not already persisted and ownership is defined.
Does this require migration? SHOULD BE NO for defaults; YES requires separate migration spec.
Does this persist Unity references? MUST BE NO.
```

---

## 20. Impacto em eventos

```text
Adds events: CONDITIONAL — only missing core time/day events.
Changes existing events: SHOULD BE NO unless audit finds safe non-breaking fix.
Requires unsubscribe pattern: YES for subscribers, but this spec should not broadly alter subscribers.
```

---

## 21. Impacto em UI/Unity

```text
Changes UI: NO, except optional read-only integration with modal pause state.
Changes scenes: NO
Changes prefabs: NO
Changes ScriptableObjects/assets: NO
Requires PlayMode automated or final human scenario: YES, because time pause/day transition behavior is gameplay lifecycle.
Human validation timing: DEFERRED_TO_FINAL_VALIDATION.
```

---

## 22. Riscos técnicos

```text
Risk: existing time system differs from direction.
Mitigation: harden incrementally and document delta.

Risk: day transition triggers domain work too early.
Mitigation: define phases but leave domain effects to later specs.

Risk: modal pause integration touches UI global systems.
Mitigation: read-only audit unless explicit small safe interface exists.

Risk: save schema change needed.
Mitigation: use defaults if possible; otherwise STOP for migration spec.
```

---

## 23. Rollback

```text
Revert time service/day transition changes.
Remove tests/validators added.
Remove execution report.
No scenes/prefabs/assets should have been changed.
```

---

# /speckit.tasks

## 24. Tasks

- [ ] T001 — Read required sources.
- [ ] T002 — Audit existing time/day transition code.
- [ ] T003 — Define/confirm canonical clock contract.
- [ ] T004 — Define/confirm day transition phases.
- [ ] T005 — Add minimal hardening only if needed.
- [ ] T006 — Add EditMode tests for deterministic behavior when practical.
- [ ] T007 — Record PlayMode/final human scenario reference.
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
| Report | Execution report foi criado? | `docs/validation/02_spec_time_clock_day_transition_runtime_execution_report.md`. | PARTIAL |

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
# Execution Report — Time Clock and Day Transition Runtime Audit and Hardening

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

EditMode tests if code added/changed. PlayMode automated or final human scenario required for final acceptance beyond build status.

---

## 26. Testing Quality Gate

- Changed deterministic logic: YES if time/day transition code or tests are changed.
- Requires EditMode tests: YES when time advancement/day transition logic is changed and harness is available.
- Requires PlayMode automated or final human scenario: YES.
- Requires regression test: YES if fixing known time/day bug; otherwise NO.
- Human validation timing: DEFERRED_TO_FINAL_VALIDATION.
- Minimum validation evidence for ACCEPTED: docs validation PASS; C# build PASS; Unity compile PASS; EditMode tests PASS or justified NOT RUN; final human scenario documented; no final ACCEPTED until PlayMode/final validation evidence exists.

---

## 27. Definition of Done

```text
Canonical clock/day transition contract exists.
Existing implementation audited/hardened or minimal implementation created.
Day transition phases documented.
Time save/default behavior documented.
No domain effects implemented prematurely.
EditMode tests/validators added or risk documented.
Execution report created.
Final human validation deferred, not claimed.
```

---

## 28. Anti-regressão

```text
Do not create multiple clocks/day counters.
Do not let domain systems own day transition.
Do not change scenes/prefabs.
Do not accept runtime behavior by compile only.
Do not ask human to validate this spec immediately.
```

---

## 29. Notas para execução posterior

This spec feeds `02_spec_calendar_season_year_runtime.md`, weather generation, lunar cycle, farm day processing, NPC schedules, economy restock and quest time hooks.
